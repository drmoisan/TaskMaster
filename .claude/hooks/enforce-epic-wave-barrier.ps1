<#
.SYNOPSIS
    Pre-tool-use hook that is the Layer 1 per-call deterrent for the epic wave barrier.

.DESCRIPTION
    Invoked by the Claude Code PreToolUse hook on the "Agent" matcher before any Agent
    (Task) call runs. Activates only when the envelope's nested tool_input.subagent_type == "orchestrator"
    and the serialized prompt contains the epic-mode kickoff marker "Epic mode: true".

    Resolution and decision procedure:
      1. Resolve the worktree whose epic checkpoint records the prompt's
         integration_branch: value (issue #690) and deny an unresolved or ambiguous target.
      2. Collect the cited feature-folder candidates with the shared resolver
         (feature-folder-resolution.ps1, issue #565): every docs/features/active/<token>
         path is truncated to its feature-folder segment, so a nested research/ or
         evidence/ citation names its own folder. No candidate denies.
      3. Read artifacts/orchestration/epic-orchestrator-state.json beneath that worktree
         and select the target among the candidates with Select-FeatureFolderTarget
         -DependencyAware: a cited upstream dependency of another cited candidate is
         pruned, and two or more remaining candidates deny as ambiguous. Neither string
         length nor prompt position takes part in the selection.
      4. Locate the target's features[] record and, for every depends_on entry, its own
         record. An entry resolves through the union index, so an issue number matches
         issue_num and a folder value matches the normalized feature_folder.
      5. Deny with reason EPIC_WAVE_BARRIER_BLOCKED unless every dependency's merge_status
         is merged or worktree_removed. A missing/unreadable checkpoint, an unresolved
         target feature_folder, or a missing dependency record also denies (fail-closed).

    This is the per-call deterrent (Layer 1) of the two-layer wave-barrier design. The
    retrospective backstop (Layer 2) is the wave-barrier ordering invariant of
    validate_epic_orchestrator_state_text, run at epic-orchestrator SubagentStop time by
    validate-orchestrator-output.ps1 through its PowerShell port
    OrchestratorStateEpicWaveBarrier.psm1, with parity pinned by tests/fixtures/epic_wave_barrier/.
    The SubagentStop transport and exit-code defect that hook depended on was resolved by epic #904
    and issue #928: a violation blocks the stop with exit 2 and a stderr reason.
    The two layers share no code.

.NOTES
    Compatible with PowerShell 7+. Depends on WorktreeRunResolution.psm1 (issue #690) and
    the pure sibling feature-folder-resolution.ps1 (issue #565), each loaded inside a
    guard: a failure is recorded and the decision denies naming the dependency. Filesystem
    reads go through an injectable wrapper function so tests can mock the boundary without
    writing temporary files.
#>
[CmdletBinding()]
param()
$script:HookDependencyGuardLoadFailed = $false; try { . (Join-Path $PSScriptRoot 'hook-dependency-guard.ps1') } catch { $script:HookDependencyGuardLoadFailed = $true }


try { Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'HookPayload.psm1' -ErrorRecord $_ }

try { Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/WorktreeRunResolution.psm1') -Force -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'WorktreeRunResolution.psm1' -ErrorRecord $_ }
$script:EpicWaveBarrierResolutionImportFailure = $null

# Shared feature-folder resolution (issue #565). Guarded so a failed dot-source denies
# rather than failing open; an earlier recorded failure is kept.
try {
    . (Join-Path $PSScriptRoot 'feature-folder-resolution.ps1')
}
catch {
    if (-not $script:EpicWaveBarrierResolutionImportFailure) {
        $script:EpicWaveBarrierResolutionImportFailure = 'feature-folder-resolution.ps1'
    }
}

$script:AllowedMergeStatuses = @('merged', 'worktree_removed')
$script:EpicModeMarker = 'Epic mode: true'

function Get-EpicWaveBarrierCheckpointContent {
    <#
    .SYNOPSIS
        Read the raw JSON text of the epic checkpoint. Tests mock this function
        (read seam).
    .PARAMETER Path
        The absolute checkpoint path composed beneath the resolved worktree root.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][ValidatePattern('^([A-Za-z]:[\\/]|/)')][string] $Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }
    return (Get-Content -LiteralPath $Path -Raw)
}

function Resolve-EpicWaveBarrierTarget {
    <#
    .SYNOPSIS
        Resolve the worktree whose epic checkpoint governs a kickoff (issue #690 seam).
    .DESCRIPTION
        Keyed on the prompt's integration_branch: value, with epic_feature_folder: as a
        cross-check; never on a feature-folder path or the payload cwd.
    .PARAMETER Prompt
        The delegation prompt text.
    .OUTPUTS
        System.Management.Automation.PSCustomObject (the worktree-resolution target result).
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Prompt)

    $signal = Find-WorktreeRunIdentitySignal -Text $Prompt
    return Resolve-WorktreeEpicTarget -IntegrationBranch $signal.IntegrationBranch -EpicSlug $signal.EpicSlug -SessionRoot (Get-Location).Path
}

function Find-EpicWaveBarrierFeatureFolderFromPrompt {
    <#
    .SYNOPSIS
        Returns the distinct feature-folder basenames cited in a prompt, in first-occurrence
        order. Returns no output when no folder is cited.
    .DESCRIPTION
        Delegates to Find-FeatureFolderCandidate in the shared resolver (issue #565): each
        docs/features/active/<token> path, in either separator style, is truncated to its
        feature-folder segment, so a nested research/ or evidence/ citation names its own
        folder. Selection among several candidates is made by Select-FeatureFolderTarget.
    .PARAMETER Prompt
        The delegation prompt text under evaluation.
    .OUTPUTS
        System.String[]
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $Prompt
    )

    return [string[]]@(Find-FeatureFolderCandidate -Text $Prompt)
}

function Find-EpicWaveBarrierFeatureRecord {
    <#
    .SYNOPSIS
        Locate the features[] record the target basename identifies, through the shared
        union index (Find-FeatureFolderRecord). A lifecycle-prefixed feature_folder value
        such as active/<folder> matches its basename; zero or several matches return $null.
    .PARAMETER Checkpoint
        Parsed epic checkpoint, or $null when absent/unreadable.
    .PARAMETER FeatureFolder
        The target feature_folder basename.
    .OUTPUTS
        System.Object or $null
    #>
    [CmdletBinding()]
    param(
        [AllowNull()]
        $Checkpoint,

        [AllowNull()]
        [string] $FeatureFolder
    )

    if ($null -eq $Checkpoint -or [string]::IsNullOrWhiteSpace($FeatureFolder)) {
        return $null
    }
    $checkpointProps = @($Checkpoint.PSObject.Properties.Name)
    if ($checkpointProps -notcontains 'features') {
        return $null
    }

    return Find-FeatureFolderRecord -Records @($Checkpoint.features) -Reference $FeatureFolder
}

function Test-EpicWaveBarrierDependenciesMerged {
    <#
    .SYNOPSIS
        Decision logic: true only when every dependency's merge_status is merged or
        worktree_removed.
    .PARAMETER Checkpoint
        Parsed epic checkpoint, or $null when absent/unreadable.
    .PARAMETER FeatureRecord
        The target feature's own features[] record, or $null when not found.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()]
        $Checkpoint,

        [AllowNull()]
        $FeatureRecord
    )

    if ($null -eq $Checkpoint -or $null -eq $FeatureRecord) {
        return $false
    }
    $featureProps = @($FeatureRecord.PSObject.Properties.Name)
    if ($featureProps -notcontains 'depends_on') {
        # No dependencies recorded: an empty/absent depends_on list has nothing to block on.
        return $true
    }

    $dependsOn = @($FeatureRecord.depends_on)
    if ($dependsOn.Count -eq 0) {
        return $true
    }

    # Every dependency edge must be durably confirmed merged or worktree_removed before
    # this wave's feature is allowed to start. Each raw entry resolves through the union
    # index, so an issue-number edge matches issue_num (issue #565).
    $features = if (@($Checkpoint.PSObject.Properties.Name) -contains 'features') { @($Checkpoint.features) } else { @() }
    foreach ($dependency in $dependsOn) {
        $dependencyRecord = Find-FeatureFolderRecord -Records $features -Reference $dependency
        if ($null -eq $dependencyRecord) {
            return $false
        }
        $dependencyProps = @($dependencyRecord.PSObject.Properties.Name)
        if ($dependencyProps -notcontains 'merge_status') {
            return $false
        }
        if ($script:AllowedMergeStatuses -notcontains ([string]$dependencyRecord.merge_status)) {
            return $false
        }
    }
    return $true
}

function Get-EpicWaveBarrierAllowDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param()

    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName      = 'PreToolUse'
            permissionDecision = 'allow'
        }
    }
}

function Get-EpicWaveBarrierBlockDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [string] $Reason
    )

    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = $Reason
        }
    }
}

function Invoke-EpicWaveBarrierDecision {
    <#
    .SYNOPSIS
        Parses the envelope's nested tool_input and returns an allow-or-block decision.
    .PARAMETER ToolInputRaw
        The raw JSON tool payload supplied by Claude Code.
    .OUTPUTS
        System.Collections.Specialized.OrderedDictionary
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [string] $ToolInputRaw
    )
    $dependencyDecision = Get-HookDependencyFailureDecision -HookEvent PreToolUse -ReasonPrefix 'EPIC_WAVE_BARRIER_BLOCKED:'
    if ($null -ne $dependencyDecision) { return $dependencyDecision }

    # A failed dependency import denies before any other logic (issue #565).
    if ($script:EpicWaveBarrierResolutionImportFailure) {
        return Get-EpicWaveBarrierBlockDecision -Reason (
            "EPIC_WAVE_BARRIER_BLOCKED: the dependency '$($script:EpicWaveBarrierResolutionImportFailure)' " +
            'failed to import, so the epic checkpoint that governs this delegation cannot be located; the gate fails closed.')
    }

    $envelope = Resolve-ClaudeHookToolInput -Raw $ToolInputRaw
    if (-not $envelope.IsValid) {
        return Get-EpicWaveBarrierBlockDecision -Reason (
            'EPIC_WAVE_BARRIER_BLOCKED: payload anomaly - ' +
            (Get-ClaudeHookPayloadAnomalyReason -Anomaly $envelope.Anomaly) +
            '. The gate fails closed on an envelope it cannot read.')
    }

    $subagent = Get-ClaudeHookToolInputString -ToolInput $envelope.Value -Name 'subagent_type'
    if (-not $subagent -or $subagent -ne 'orchestrator') {
        return Get-EpicWaveBarrierAllowDecision
    }

    $prompt = Get-ClaudeHookToolInputString -ToolInput $envelope.Value -Name 'prompt'
    if (-not $prompt -or $prompt -notlike "*$script:EpicModeMarker*") {
        return Get-EpicWaveBarrierAllowDecision
    }

    # The epic checkpoint is located by the kickoff's integration branch (issue #690).
    $target = Resolve-EpicWaveBarrierTarget -Prompt $prompt
    if ($target.Status -eq 'NoTarget' -or $target.Status -eq 'Ambiguous') {
        return Get-EpicWaveBarrierBlockDecision -Reason "EPIC_WAVE_BARRIER_BLOCKED: $($target.ReasonCode): $($target.Detail)"
    }

    $candidates = @(Find-EpicWaveBarrierFeatureFolderFromPrompt -Prompt $prompt)
    if ($candidates.Count -eq 0) {
        return Get-EpicWaveBarrierBlockDecision -Reason 'EPIC_WAVE_BARRIER_BLOCKED: an epic-mode orchestrator delegation must reference the target feature folder in the prompt so its dependency edges can be verified.'
    }

    $checkpointRaw = Get-EpicWaveBarrierCheckpointContent -Path (Get-WorktreeRunCheckpointPath -Kind epic -WorktreeRoot $target.WorktreeRoot)
    $checkpoint = $null
    if (-not [string]::IsNullOrWhiteSpace($checkpointRaw)) {
        try {
            $checkpoint = $checkpointRaw | ConvertFrom-Json -ErrorAction Stop
        } catch {
            $checkpoint = $null
        }
    }

    # The target is selected among the cited candidates (issue #565): a cited upstream
    # dependency of another candidate is pruned, and an unresolved tie denies.
    $records = if ($null -ne $checkpoint -and @($checkpoint.PSObject.Properties.Name) -contains 'features') { @($checkpoint.features) } else { @() }
    $selection = Select-FeatureFolderTarget -Records $records -Candidate $candidates -DependencyAware
    if ($selection.Status -eq 'Ambiguous') {
        return Get-EpicWaveBarrierBlockDecision -Reason "EPIC_WAVE_BARRIER_BLOCKED: $($selection.Detail)."
    }
    $featureFolder = $selection.Basename

    $featureRecord = Find-EpicWaveBarrierFeatureRecord -Checkpoint $checkpoint -FeatureFolder $featureFolder
    if (Test-EpicWaveBarrierDependenciesMerged -Checkpoint $checkpoint -FeatureRecord $featureRecord) {
        return Get-EpicWaveBarrierAllowDecision
    }

    return Get-EpicWaveBarrierBlockDecision -Reason "EPIC_WAVE_BARRIER_BLOCKED: '$featureFolder' cannot start until every dependency in its depends_on list is durably confirmed merged or worktree_removed in the epic checkpoint. The checkpoint was unreadable, the feature record was not found, or a dependency is not yet safe."
}

function Invoke-EpicWaveBarrierEntryPoint {
    <#
    .SYNOPSIS
        Runs the hook decision and returns the process exit code.
    .DESCRIPTION
        Acquires the payload through the shared reader unless the caller supplies
        one, emits the compact decision JSON, and returns 0. It never returns 1:
        exit 1 is non-blocking for PreToolUse, so every anomaly is already a deny
        decision by the time control reaches here. The function does not call exit;
        the thin tail converts the returned code into a process exit.
    .PARAMETER ToolInputRaw
        Optional pre-acquired payload text. When omitted the ReadPayload seam runs.
    .PARAMETER ReadPayload
        Seam for payload acquisition, so tests can drive the empty-on-all-transports
        case without touching a console.
    .OUTPUTS
        System.Int32
    #>
    [CmdletBinding()]
    [OutputType([int])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $ToolInputRaw,

        [scriptblock] $ReadPayload = { Read-ClaudeHookRawPayload }
    )

    if (-not $PSBoundParameters.ContainsKey('ToolInputRaw')) {
        $ToolInputRaw = [string](& $ReadPayload)
    }

    $decision = Invoke-EpicWaveBarrierDecision -ToolInputRaw $ToolInputRaw
    $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output

    return 0
}

# Guard allows dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}
if ($script:HookDependencyGuardLoadFailed) { [Console]::Error.WriteLine('EPIC_WAVE_BARRIER_BLOCKED: hook-dependency-guard.ps1 failed to load; the gate fails closed.'); exit 2 }
$dependencyDecision = Get-HookDependencyFailureDecision -HookEvent PreToolUse -ReasonPrefix 'EPIC_WAVE_BARRIER_BLOCKED:'
if ($null -ne $dependencyDecision) { $dependencyDecision | ConvertTo-Json -Compress -Depth 5 | Write-Output; exit 0 }

exit (Invoke-EpicWaveBarrierEntryPoint)
