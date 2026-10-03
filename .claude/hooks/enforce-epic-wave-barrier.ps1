<#
.SYNOPSIS
    Pre-tool-use hook that is the Layer 1 per-call deterrent for the epic wave barrier.

.DESCRIPTION
    Invoked by the Claude Code PreToolUse hook on the "Agent" matcher before any Agent
    (Task) call runs. Activates only when the envelope's nested tool_input.subagent_type == "orchestrator"
    and the serialized prompt contains the epic-mode kickoff marker "Epic mode: true".

    Resolution and decision procedure:
      1. Resolve the target child feature_folder from the prompt text by scanning for a
         docs/features/active/<token> path, mirroring
         enforce-prd-feature-before-planner.ps1's Find-PrdFeatureFolderFromPrompt
         technique (longest match wins; a .md-suffixed match uses its parent directory).
      2. Resolve the worktree whose epic checkpoint records the prompt's
         integration_branch: value (issue #690), deny an unresolved or ambiguous target,
         then read artifacts/orchestration/epic-orchestrator-state.json beneath that
         worktree and locate the features[] record whose feature_folder equals the
         resolved basename.
      3. Look up that feature's depends_on list, and for every dependency, locate its own
         features[] record.
      4. Deny with reason EPIC_WAVE_BARRIER_BLOCKED unless every dependency's merge_status
         is merged or worktree_removed. A missing/unreadable checkpoint, an unresolved
         target feature_folder, or a missing dependency record also denies (fail-closed).

    This is the per-call deterrent (Layer 1) of the two-layer wave-barrier design; the
    retrospective backstop (Layer 2) is the wave-barrier ordering invariant inside
    validate_epic_orchestrator_state_text, enforced separately at epic-orchestrator
    SubagentStop time.

.NOTES
    Compatible with PowerShell 7+. Depends on WorktreeRunResolution.psm1 (issue #690),
    imported inside a guard: a failed import is recorded and the decision denies naming
    the module. Filesystem reads go through an injectable wrapper function so tests can
    mock the boundary without writing temporary files.
#>
[CmdletBinding()]
param()


Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force

# Import guard (issue #690): a failed import denies instead of failing open.
$script:EpicWaveBarrierResolutionImportFailure = $null
try {
    Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/WorktreeRunResolution.psm1') -Force -ErrorAction Stop
}
catch {
    $script:EpicWaveBarrierResolutionImportFailure = 'WorktreeRunResolution.psm1'
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
        Scans a prompt string for docs/features/active/<...> path tokens and returns the
        longest unique match's basename. Returns $null when no match is found.
    .DESCRIPTION
        Mirrors enforce-prd-feature-before-planner.ps1's
        Find-PrdFeatureFolderFromPrompt technique: forward- or backslash-separated path
        tokens are accepted, the longest match wins, and a .md-suffixed match resolves to
        its parent directory before the basename is extracted.
    .PARAMETER Prompt
        The delegation prompt text under evaluation.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $Prompt
    )

    if (-not $Prompt) {
        return $null
    }

    $pattern = 'docs[\\/]+features[\\/]+active[\\/]+[^\s"''`]+'
    $matchList = [regex]::Matches($Prompt, $pattern)
    if ($matchList.Count -eq 0) {
        return $null
    }

    $unique = @{}
    foreach ($m in $matchList) {
        $normalized = ($m.Value -replace '\\', '/').TrimEnd('/')
        $unique[$normalized] = $true
    }

    $candidates = @(@($unique.Keys) | Sort-Object -Property Length -Descending)
    $best = $candidates[0]

    if ($best -match '\.md$') {
        $best = $best -replace '/[^/]+\.md$', ''
    }

    return ($best -split '/')[-1]
}

function Find-EpicWaveBarrierFeatureRecord {
    <#
    .SYNOPSIS
        Locate the features[] record whose feature_folder equals the target basename.
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

    # Scan every recorded feature for a feature_folder that equals the target basename.
    foreach ($feature in @($Checkpoint.features)) {
        $featureProps = @($feature.PSObject.Properties.Name)
        if ($featureProps -notcontains 'feature_folder') {
            continue
        }
        if (([string]$feature.feature_folder) -eq $FeatureFolder) {
            return $feature
        }
    }
    return $null
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
    # this wave's feature is allowed to start.
    foreach ($dependencyFolder in $dependsOn) {
        $dependencyRecord = Find-EpicWaveBarrierFeatureRecord -Checkpoint $Checkpoint -FeatureFolder ([string]$dependencyFolder)
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

    # A failed worktree-resolution import denies before any other logic (issue #690).
    if ($script:EpicWaveBarrierResolutionImportFailure) {
        return Get-EpicWaveBarrierBlockDecision -Reason (
            "EPIC_WAVE_BARRIER_BLOCKED: the worktree-resolution module '$($script:EpicWaveBarrierResolutionImportFailure)' " +
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

    $featureFolder = Find-EpicWaveBarrierFeatureFolderFromPrompt -Prompt $prompt
    if (-not $featureFolder) {
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

exit (Invoke-EpicWaveBarrierEntryPoint)
