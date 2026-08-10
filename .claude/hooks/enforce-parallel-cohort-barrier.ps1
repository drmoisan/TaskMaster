<#
.SYNOPSIS
    Pre-tool-use hook that is the Layer 1 per-call deterrent for the parallel cohort barrier.

.DESCRIPTION
    Invoked by the Claude Code PreToolUse hook on the "Agent" matcher before any Agent
    (Task) call runs. Activates only when CLAUDE_TOOL_INPUT.subagent_type == "orchestrator"
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

$script:ParallelCheckpointPath = 'artifacts/orchestration/parallel-orchestrator-state.json'
$script:AllowedMergeStatuses = @('merged', 'worktree_removed')
$script:ParallelModeMarker = 'Parallel mode: true'

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

function Find-ParallelCohortBarrierItemRecord {
    <#
    .SYNOPSIS
        Locate the items[] record whose feature_folder resolves to the target basename.
    .PARAMETER Checkpoint
        Parsed parallel checkpoint, or $null when absent/unreadable.
    .PARAMETER FeatureFolder
        The target feature_folder basename resolved from the prompt.
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
    if ($checkpointProps -notcontains 'items') {
        return $null
    }

    # Scan every recorded item for a feature_folder whose basename equals the target.
    foreach ($item in @($Checkpoint.items)) {
        $itemProps = @($item.PSObject.Properties.Name)
        if ($itemProps -notcontains 'feature_folder') {
            continue
        }
        $itemBasename = Get-ParallelCohortBarrierFolderBasename -FolderPath ([string]$item.feature_folder)
        if ($itemBasename -eq $FeatureFolder) {
            return $item
        }
    }
    return $null
}

function Find-ParallelCohortBarrierItemByKey {
    <#
    .SYNOPSIS
        Locate the items[] record whose issue_num equals the supplied key.
    .PARAMETER Checkpoint
        Parsed parallel checkpoint, or $null when absent/unreadable.
    .PARAMETER IssueKey
        The item primary key (issue_num) rendered as a string.
    .OUTPUTS
        System.Object or $null
    #>
    [CmdletBinding()]
    param(
        [AllowNull()]
        $Checkpoint,

        [AllowNull()]
        [string] $IssueKey
    )

    if ($null -eq $Checkpoint -or [string]::IsNullOrWhiteSpace($IssueKey)) {
        return $null
    }
    $checkpointProps = @($Checkpoint.PSObject.Properties.Name)
    if ($checkpointProps -notcontains 'items') {
        return $null
    }

    foreach ($item in @($Checkpoint.items)) {
        $itemProps = @($item.PSObject.Properties.Name)
        if ($itemProps -notcontains 'issue_num') {
            continue
        }
        if (([string]$item.issue_num) -eq $IssueKey) {
            return $item
        }
    }
    return $null
}

function Find-ParallelCohortBarrierCohortIndex {
    <#
    .SYNOPSIS
        Return the current-generation cohort index that holds the supplied item key.
    .DESCRIPTION
        The cohort projection is restricted to cohorts[] rows whose generation equals the
        top-level recolor_generation, which is the current coloring. Rows from a superseded
        generation are ignored. Returns $null when the checkpoint records no
        recolor_generation, no cohorts, or no current-generation cohort containing the key.
    .PARAMETER Checkpoint
        Parsed parallel checkpoint, or $null when absent/unreadable.
    .PARAMETER IssueKey
        The item primary key (issue_num) rendered as a string.
    .OUTPUTS
        System.Int32 or $null
    #>
    [CmdletBinding()]
    [OutputType([int])]
    param(
        [AllowNull()]
        $Checkpoint,

        [AllowNull()]
        [string] $IssueKey
    )

    if ($null -eq $Checkpoint -or [string]::IsNullOrWhiteSpace($IssueKey)) {
        return $null
    }
    $checkpointProps = @($Checkpoint.PSObject.Properties.Name)
    if ($checkpointProps -notcontains 'cohorts' -or $checkpointProps -notcontains 'recolor_generation') {
        return $null
    }

    $currentGeneration = [string]$Checkpoint.recolor_generation

    foreach ($cohort in @($Checkpoint.cohorts)) {
        $cohortProps = @($cohort.PSObject.Properties.Name)
        if ($cohortProps -notcontains 'index' -or $cohortProps -notcontains 'generation' -or $cohortProps -notcontains 'item_keys') {
            continue
        }
        if (([string]$cohort.generation) -ne $currentGeneration) {
            continue
        }
        $parsedIndex = 0
        if (-not [int]::TryParse(([string]$cohort.index), [ref] $parsedIndex)) {
            continue
        }
        foreach ($key in @($cohort.item_keys)) {
            if (([string]$key) -eq $IssueKey) {
                return $parsedIndex
            }
        }
    }
    return $null
}

function Get-ParallelCohortBarrierConflictNeighborList {
    <#
    .SYNOPSIS
        Return the item keys that share a conflict edge with the supplied item key.
    .PARAMETER Checkpoint
        Parsed parallel checkpoint, or $null when absent/unreadable.
    .PARAMETER IssueKey
        The item primary key (issue_num) rendered as a string.
    .OUTPUTS
        System.Object[] holding each neighbor item key rendered as a string.
    #>
    [CmdletBinding()]
    [OutputType([object[]])]
    param(
        [AllowNull()]
        $Checkpoint,

        [AllowNull()]
        [string] $IssueKey
    )

    $neighborList = @()
    if ($null -eq $Checkpoint -or [string]::IsNullOrWhiteSpace($IssueKey)) {
        return $neighborList
    }
    $checkpointProps = @($Checkpoint.PSObject.Properties.Name)
    if ($checkpointProps -notcontains 'conflict_edges') {
        return $neighborList
    }

    # Edges are undirected and canonically normalized to a < b, so both endpoints are
    # inspected and the opposite endpoint is returned as the neighbor.
    foreach ($edge in @($Checkpoint.conflict_edges)) {
        $edgeProps = @($edge.PSObject.Properties.Name)
        if ($edgeProps -notcontains 'a' -or $edgeProps -notcontains 'b') {
            continue
        }
        $endpointA = [string]$edge.a
        $endpointB = [string]$edge.b
        if ($endpointA -eq $IssueKey) {
            $neighborList += $endpointB
        } elseif ($endpointB -eq $IssueKey) {
            $neighborList += $endpointA
        }
    }
    return $neighborList
}

function Test-ParallelCohortBarrierClear {
    <#
    .SYNOPSIS
        Decision logic: true only when every conflicting neighbor in a strictly prior
        current-generation cohort has merge_status merged or worktree_removed.
    .PARAMETER Checkpoint
        Parsed parallel checkpoint, or $null when absent/unreadable.
    .PARAMETER ItemRecord
        The target item's own items[] record, or $null when not found.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()]
        $Checkpoint,

        [AllowNull()]
        $ItemRecord
    )

    if ($null -eq $Checkpoint -or $null -eq $ItemRecord) {
        return $false
    }
    $itemProps = @($ItemRecord.PSObject.Properties.Name)
    if ($itemProps -notcontains 'issue_num') {
        return $false
    }

    $targetKey = [string]$ItemRecord.issue_num
    $targetIndex = Find-ParallelCohortBarrierCohortIndex -Checkpoint $Checkpoint -IssueKey $targetKey
    if ($null -eq $targetIndex) {
        # No current-generation cohort assignment: position is unknown, so fail closed.
        return $false
    }

    foreach ($neighborKey in @(Get-ParallelCohortBarrierConflictNeighborList -Checkpoint $Checkpoint -IssueKey $targetKey)) {
        $neighborIndex = Find-ParallelCohortBarrierCohortIndex -Checkpoint $Checkpoint -IssueKey $neighborKey
        if ($null -eq $neighborIndex) {
            # Not part of the current coloring, so not a strictly prior cohort neighbor.
            continue
        }
        if ($neighborIndex -ge $targetIndex) {
            # Same-cohort and later-cohort contention is not a Layer 1 concern.
            continue
        }
        $neighborRecord = Find-ParallelCohortBarrierItemByKey -Checkpoint $Checkpoint -IssueKey $neighborKey
        if ($null -eq $neighborRecord) {
            return $false
        }
        $neighborProps = @($neighborRecord.PSObject.Properties.Name)
        if ($neighborProps -notcontains 'merge_status') {
            return $false
        }
        if ($script:AllowedMergeStatuses -notcontains ([string]$neighborRecord.merge_status)) {
            return $false
        }
    }
    return $true
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
        Parses CLAUDE_TOOL_INPUT and returns an allow-or-block decision.
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

    if (-not $ToolInputRaw) {
        return Get-ParallelCohortBarrierAllowDecision
    }

    try {
        $toolInput = $ToolInputRaw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        throw "enforce-parallel-cohort-barrier hook received malformed JSON in CLAUDE_TOOL_INPUT: $_"
    }

    $subagent = $toolInput.subagent_type
    if (-not $subagent -or $subagent -ne 'orchestrator') {
        return Get-ParallelCohortBarrierAllowDecision
    }

    $prompt = [string]$toolInput.prompt
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

# Guard allows dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}

try {
    $decision = Invoke-ParallelCohortBarrierDecision -ToolInputRaw $env:CLAUDE_TOOL_INPUT
} catch {
    Write-Error $_
    exit 1
}

$decision | ConvertTo-Json -Compress -Depth 5 | Write-Output

exit 0
