<#
.SYNOPSIS
    Pre-tool-use hook that is the Layer 1 per-call deterrent for the parallel cohort barrier.

.DESCRIPTION
    Invoked by the Claude Code PreToolUse hook on the "Agent" matcher before any Agent
    (Task) call runs. Activates only when the envelope's tool_input.subagent_type == "orchestrator"
    and the serialized prompt contains the parallel kickoff marker "Parallel mode: true".

    Adapted from enforce-epic-wave-barrier.ps1. The parallel surface has no depends_on
    field, so the epic hook's depends_on lookup is replaced by conflict-edge plus
    cohort-index logic: ordering is derived from blast-radius contention recorded as
    conflict_edges[], and position is read from the current cohort coloring.

    Resolution and decision procedure:
      1. Resolve the worktree whose parallel checkpoint records the prompt's
         parallel_slug: value (issue #690) and deny an unresolved or ambiguous target.
      2. Collect the cited feature-folder candidates with the shared resolver
         (feature-folder-resolution.ps1, issue #565): every docs/features/active/<token>
         path is truncated to its feature-folder segment, so a nested research/ or
         evidence/ citation names its own folder. No candidate denies.
      3. Read artifacts/orchestration/parallel-orchestrator-state.json beneath that
         worktree and select the target among the candidates with
         Select-FeatureFolderTarget. The canonical issue-number line, when it names exactly
         one number, breaks a tie among the cited run items; two or more remaining
         candidates otherwise deny as ambiguous. Neither string length nor prompt position
         takes part in the selection. Then locate the items[] record whose feature_folder
         resolves to the selected basename.
      4. Project the cohort coloring to the cohorts[] rows whose generation equals the
         top-level recolor_generation, and read the target item's cohort index from that
         projection.
      5. Collect every conflict_edges[] neighbor of the target item.
      6. Deny with reason PARALLEL_COHORT_BARRIER_BLOCKED unless every neighbor that sits
         in a strictly prior current-generation cohort has merge_status in
         {merged, worktree_removed}. ci_green does NOT satisfy the barrier: an item whose
         CI is green has not merged, so its worktree is still live and its contention is
         unresolved. Same-cohort and later-cohort neighbors do not block Layer 1.
      7. A missing or unparseable checkpoint, an unresolved feature-folder token, a
         missing items[] record, a target with no current-generation cohort assignment, a
         missing neighbor record, and a missing merge_status all deny (fail-closed).

    This is the per-call deterrent (Layer 1) of the two-layer cohort-barrier design.
    Neither layer alone closes the gap: a PreToolUse hook fires once per tool call with no
    cross-call or conversation-state visibility, so it cannot validate a batch of
    concurrent Agent calls. The retrospective backstop (Layer 2) is the
    PARALLEL_COHORT_BARRIER_VIOLATION ordering invariant inside
    validate_parallel_orchestrator_state_text, enforced at parallel-orchestrator
    SubagentStop time.

.NOTES
    Compatible with PowerShell 7+. Depends on WorktreeItemResolution.psm1 and
    WorktreeRunResolution.psm1 (issue #690) and the pure sibling
    feature-folder-resolution.ps1 (issue #565), each loaded inside a guard: the first
    failure is recorded and the decision denies naming the dependency. Filesystem reads go
    through an injectable wrapper function so tests can mock the boundary without writing
    temporary files.
#>
[CmdletBinding()]
param()
$script:HookDependencyGuardLoadFailed = $false; try { . (Join-Path $PSScriptRoot 'hook-dependency-guard.ps1') } catch { $script:HookDependencyGuardLoadFailed = $true }


try { Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'HookPayload.psm1' -ErrorRecord $_ }

try { Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/WorktreeItemResolution.psm1') -Force -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'WorktreeItemResolution.psm1' -ErrorRecord $_ }
try { Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/WorktreeRunResolution.psm1') -Force -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'WorktreeRunResolution.psm1' -ErrorRecord $_ }
$script:ParallelCohortBarrierResolutionImportFailure = $null

# Shared feature-folder resolution (issue #565). Guarded so a failed dot-source denies
# rather than failing open; an earlier recorded failure is kept.
try {
    . (Join-Path $PSScriptRoot 'feature-folder-resolution.ps1')
}
catch {
    if (-not $script:ParallelCohortBarrierResolutionImportFailure) {
        $script:ParallelCohortBarrierResolutionImportFailure = 'feature-folder-resolution.ps1'
    }
}

$script:AllowedMergeStatuses = @('merged', 'worktree_removed')
$script:ParallelModeMarker = 'Parallel mode: true'

# Dot-source the record-resolution and barrier helpers. Guarded so dot-sourcing this
# hook in tests loads the helpers too (issue #501 headroom split).
try { . (Join-Path $PSScriptRoot 'enforce-parallel-cohort-barrier-helpers.ps1') } catch { Add-HookDependencyFailure -Name 'enforce-parallel-cohort-barrier-helpers.ps1' -ErrorRecord $_ }

function Get-ParallelCohortBarrierCheckpointContent {
    <#
    .SYNOPSIS
        Read the raw JSON text of the parallel checkpoint. Tests mock this function
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

function Resolve-ParallelCohortBarrierTarget {
    <#
    .SYNOPSIS
        Resolve the worktree whose parallel checkpoint governs a kickoff (issue #690 seam).
    .DESCRIPTION
        Keyed on the prompt's parallel_slug: value; never on a feature-folder path or the
        payload cwd.
    .PARAMETER Prompt
        The delegation prompt text.
    .OUTPUTS
        System.Management.Automation.PSCustomObject (the worktree-resolution target result).
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Prompt)

    $signal = Find-WorktreeRunIdentitySignal -Text $Prompt
    return Resolve-WorktreeParallelTarget -ParallelSlug $signal.ParallelSlug -SessionRoot (Get-Location).Path
}

function Get-ParallelCohortBarrierFolderBasename {
    <#
    .SYNOPSIS
        Normalize a feature-folder path or token to its basename.
    .DESCRIPTION
        Separators are normalized to forward slashes, a trailing slash is trimmed, and a
        .md-suffixed value resolves to its parent directory before the basename is taken.
        Applied to both sides of the target comparison so a checkpoint that records a full
        docs/features/active/<folder> path and one that records a bare basename both match
        the prompt token.
    .PARAMETER FolderPath
        The path or token to reduce.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $FolderPath
    )

    if ([string]::IsNullOrWhiteSpace($FolderPath)) {
        return $null
    }

    $normalized = ($FolderPath -replace '\\', '/').TrimEnd('/')
    if ($normalized -match '\.md$') {
        $normalized = $normalized -replace '/[^/]+\.md$', ''
    }
    return ($normalized -split '/')[-1]
}

function Find-ParallelCohortBarrierFeatureFolderFromPrompt {
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

function Get-ParallelCohortBarrierAllowDecision {
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

function Get-ParallelCohortBarrierBlockDecision {
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

function Invoke-ParallelCohortBarrierDecision {
    <#
    .SYNOPSIS
        Parses the PreToolUse envelope and returns an allow-or-block decision.
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
    $dependencyDecision = Get-HookDependencyFailureDecision -HookEvent PreToolUse -ReasonPrefix 'PARALLEL_COHORT_BARRIER_BLOCKED:'
    if ($null -ne $dependencyDecision) { return $dependencyDecision }

    # A failed dependency import denies before any other logic (issue #565).
    if ($script:ParallelCohortBarrierResolutionImportFailure) {
        return Get-ParallelCohortBarrierBlockDecision -Reason (
            "PARALLEL_COHORT_BARRIER_BLOCKED: the dependency '$($script:ParallelCohortBarrierResolutionImportFailure)' " +
            'failed to import, so the parallel checkpoint that governs this delegation cannot be located; the gate fails closed.')
    }

    $envelope = Resolve-ClaudeHookToolInput -Raw $ToolInputRaw
    if (-not $envelope.IsValid) {
        return Get-ParallelCohortBarrierBlockDecision -Reason (
            'PARALLEL_COHORT_BARRIER_BLOCKED: payload anomaly - ' +
            (Get-ClaudeHookPayloadAnomalyReason -Anomaly $envelope.Anomaly) +
            '. The gate fails closed on an envelope it cannot read.')
    }

    $subagent = Get-ClaudeHookToolInputString -ToolInput $envelope.Value -Name 'subagent_type'
    if (-not $subagent -or $subagent -ne 'orchestrator') {
        return Get-ParallelCohortBarrierAllowDecision
    }

    $prompt = Get-ClaudeHookToolInputString -ToolInput $envelope.Value -Name 'prompt'
    if (-not $prompt -or $prompt -notlike "*$script:ParallelModeMarker*") {
        return Get-ParallelCohortBarrierAllowDecision
    }

    # The parallel checkpoint is located by the kickoff's parallel_slug (issue #690).
    $target = Resolve-ParallelCohortBarrierTarget -Prompt $prompt
    if ($target.Status -eq 'NoTarget' -or $target.Status -eq 'Ambiguous') {
        return Get-ParallelCohortBarrierBlockDecision -Reason "PARALLEL_COHORT_BARRIER_BLOCKED: $($target.ReasonCode): $($target.Detail)"
    }

    $candidates = @(Find-ParallelCohortBarrierFeatureFolderFromPrompt -Prompt $prompt)
    if ($candidates.Count -eq 0) {
        return Get-ParallelCohortBarrierBlockDecision -Reason 'PARALLEL_COHORT_BARRIER_BLOCKED: a parallel-mode orchestrator delegation must reference the target item feature folder path (docs/features/active/<folder>) in the prompt so its conflict edges and cohort position can be verified.'
    }

    $checkpointRaw = Get-ParallelCohortBarrierCheckpointContent -Path (Get-WorktreeRunCheckpointPath -Kind parallel -WorktreeRoot $target.WorktreeRoot)
    $checkpoint = $null
    if (-not [string]::IsNullOrWhiteSpace($checkpointRaw)) {
        try {
            $checkpoint = $checkpointRaw | ConvertFrom-Json -ErrorAction Stop
        } catch {
            $checkpoint = $null
        }
    }

    # The target is selected among the cited candidates (issue #565). The canonical
    # issue-number line breaks a tie only when it names exactly one number.
    $records = if ($null -ne $checkpoint -and @($checkpoint.PSObject.Properties.Name) -contains 'items') { @($checkpoint.items) } else { @() }
    $selectionArguments = @{ Records = $records; Candidate = $candidates }
    $declared = Find-WorktreeItemIssueSignal -Text $prompt
    if (@($declared).Count -eq 1) { $selectionArguments['DeclaredIssueNumber'] = [int]@($declared)[0] }
    $selection = Select-FeatureFolderTarget @selectionArguments
    if ($selection.Status -eq 'Ambiguous') {
        return Get-ParallelCohortBarrierBlockDecision -Reason "PARALLEL_COHORT_BARRIER_BLOCKED: $($selection.Detail)."
    }
    $featureFolder = $selection.Basename

    $itemRecord = Find-ParallelCohortBarrierItemRecord -Checkpoint $checkpoint -FeatureFolder $featureFolder
    if (Test-ParallelCohortBarrierClear -Checkpoint $checkpoint -ItemRecord $itemRecord) {
        return Get-ParallelCohortBarrierAllowDecision
    }

    return Get-ParallelCohortBarrierBlockDecision -Reason "PARALLEL_COHORT_BARRIER_BLOCKED: '$featureFolder' cannot start until every conflicting item in a strictly prior current-generation cohort is durably confirmed merged or worktree_removed in the parallel checkpoint. The checkpoint was unreadable, the items[] record was not found, the item has no current-generation cohort assignment, or a conflicting prior-cohort item is not yet safe (ci_green does not satisfy the barrier)."
}

function Invoke-ParallelCohortBarrierEntryPoint {
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

    $decision = Invoke-ParallelCohortBarrierDecision -ToolInputRaw $ToolInputRaw
    $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output

    return 0
}

# Guard allows dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}
if ($script:HookDependencyGuardLoadFailed) { [Console]::Error.WriteLine('PARALLEL_COHORT_BARRIER_BLOCKED: hook-dependency-guard.ps1 failed to load; the gate fails closed.'); exit 2 }
$dependencyDecision = Get-HookDependencyFailureDecision -HookEvent PreToolUse -ReasonPrefix 'PARALLEL_COHORT_BARRIER_BLOCKED:'
if ($null -ne $dependencyDecision) { $dependencyDecision | ConvertTo-Json -Compress -Depth 5 | Write-Output; exit 0 }

exit (Invoke-ParallelCohortBarrierEntryPoint)
