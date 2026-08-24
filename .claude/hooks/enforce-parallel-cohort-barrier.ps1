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
      1. Resolve the target item's feature folder from the prompt text by scanning for a
         docs/features/active/<token> path, mirroring the epic hook's technique
         (longest match wins; a .md-suffixed match uses its parent directory).
      2. Read artifacts/orchestration/parallel-orchestrator-state.json and locate the
         items[] record whose feature_folder resolves to the same basename.
      3. Project the cohort coloring to the cohorts[] rows whose generation equals the
         top-level recolor_generation, and read the target item's cohort index from that
         projection.
      4. Collect every conflict_edges[] neighbor of the target item.
      5. Deny with reason PARALLEL_COHORT_BARRIER_BLOCKED unless every neighbor that sits
         in a strictly prior current-generation cohort has merge_status in
         {merged, worktree_removed}. ci_green does NOT satisfy the barrier: an item whose
         CI is green has not merged, so its worktree is still live and its contention is
         unresolved. Same-cohort and later-cohort neighbors do not block Layer 1.
      6. A missing or unparseable checkpoint, an unresolved feature-folder token, a
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
    Compatible with PowerShell 7+. No external module dependencies. Filesystem reads go
    through an injectable wrapper function so tests can mock the boundary without writing
    temporary files.
#>
[CmdletBinding()]
param()


Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force
$script:ParallelCheckpointPath = 'artifacts/orchestration/parallel-orchestrator-state.json'
$script:AllowedMergeStatuses = @('merged', 'worktree_removed')
$script:ParallelModeMarker = 'Parallel mode: true'

# Dot-source the record-resolution and barrier helpers. Guarded so dot-sourcing this
# hook in tests loads the helpers too (issue #501 headroom split).
. (Join-Path $PSScriptRoot 'enforce-parallel-cohort-barrier-helpers.ps1')

function Get-ParallelCohortBarrierCheckpointContent {
    <#
    .SYNOPSIS
        Read the raw JSON text of the parallel checkpoint. Tests mock this function
        (read seam).
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param()

    if (-not (Test-Path -LiteralPath $script:ParallelCheckpointPath -PathType Leaf)) {
        return $null
    }
    return (Get-Content -LiteralPath $script:ParallelCheckpointPath -Raw)
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
        Scans a prompt string for docs/features/active/<...> path tokens and returns the
        longest unique match's basename. Returns $null when no match is found.
    .DESCRIPTION
        Mirrors the epic wave-barrier hook's technique: forward- or backslash-separated
        path tokens are accepted, the longest match wins, and a .md-suffixed match resolves
        to its parent directory before the basename is extracted.
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
    return (Get-ParallelCohortBarrierFolderBasename -FolderPath $candidates[0])
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

    $featureFolder = Find-ParallelCohortBarrierFeatureFolderFromPrompt -Prompt $prompt
    if (-not $featureFolder) {
        return Get-ParallelCohortBarrierBlockDecision -Reason 'PARALLEL_COHORT_BARRIER_BLOCKED: a parallel-mode orchestrator delegation must reference the target item feature folder path (docs/features/active/<folder>) in the prompt so its conflict edges and cohort position can be verified.'
    }

    $checkpointRaw = Get-ParallelCohortBarrierCheckpointContent
    $checkpoint = $null
    if (-not [string]::IsNullOrWhiteSpace($checkpointRaw)) {
        try {
            $checkpoint = $checkpointRaw | ConvertFrom-Json -ErrorAction Stop
        } catch {
            $checkpoint = $null
        }
    }

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

exit (Invoke-ParallelCohortBarrierEntryPoint)
