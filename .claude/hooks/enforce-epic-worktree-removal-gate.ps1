<#
.SYNOPSIS
    Pre-tool-use hook that gates git worktree remove behind epic checkpoint merge state.

.DESCRIPTION
    Invoked by the Claude Code PreToolUse hook on the "Bash" matcher before any Bash
    command runs. Regex-matches git worktree remove against the envelope's
    tool_input.command, extracts the target worktree path argument, and allows the
    removal only when one of two checkpoint-only conditions holds. The branches are
    evaluated in this order and are ORed, not ANDed: the first that authorizes wins and
    the second is not consulted.

      1. Epic path: artifacts/orchestration/epic-orchestrator-state.json carries a
         features[] record whose worktree_path matches the removal target and whose
         merge_status is merged or worktree_removed.
      2. Parallel path: artifacts/orchestration/parallel-orchestrator-state.json has
         route_id == "parallel" and carries an items[] entry whose worktree_path matches
         the removal target and whose merge_status is merged or worktree_removed.

    Otherwise the command is denied with reason EPIC_WORKTREE_REMOVAL_BLOCKED. The gate
    is fail-closed in every failure mode: neither checkpoint present, either checkpoint
    unparseable, route_id absent or not "parallel", no matching worktree_path, a matched
    record whose merge_status is not in the allowed set, or a matched record carrying no
    merge_status key. The cascade is a disjunction of two positive predicates; no
    negative path returns an allow. An envelope-level anomaly is checked first and denies
    before either checkpoint is read.

    Both branches key on the worktree PATH because the command names a path, and both
    normalize separators and trim a trailing slash on each side of the comparison, so
    Windows- and POSIX-style paths compare equal. Path is also why the two branches are
    mutually exclusive in practice: an epic run and a parallel run allocate worktrees
    under distinct per-run, per-item paths, so a path recorded in one checkpoint is not a
    path recorded in the other, and adding branch 2 cannot widen what branch 1 authorizes.
    The authorization is a property of the path rather than of the caller, and the safety
    property this gate protects - do not destroy unmerged work - is likewise a property of
    the path's recorded merge state.

    Accepted residual: artifacts/ is gitignored, so the parallel checkpoint persists after
    a run ends and a stale document remains readable here. For a stale document to
    authorize a removal it should not, a worktree would have to exist at a path
    byte-identical, after normalization, to one it records with merge_status merged or
    worktree_removed. Worktree paths carry a session or timestamp component, so a
    collision is implausible, and where the recorded status is worktree_removed the path
    was already deleted. This is a documented accepted trade, not an unexamined gap; the
    route_id check does not reduce it, because a stale parallel checkpoint legitimately
    declares that route.

    The sibling gate enforce-parallel-worktree-removal-gate.ps1 fires on the same command
    and owns its own reason prefix. PreToolUse denials are conjunctive, so both gates must
    allow for a removal to proceed; this gate keeps the EPIC_WORKTREE_REMOVAL_BLOCKED
    prefix for both of its branches so transcript attribution stays unambiguous.

.NOTES
    Compatible with PowerShell 7+. No external module dependencies. Filesystem reads go
    through an injectable wrapper function so tests can mock the boundary without writing
    temporary files.
#>
[CmdletBinding()]
param()


Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force
$script:EpicCheckpointPath = 'artifacts/orchestration/epic-orchestrator-state.json'
$script:ParallelCheckpointPath = 'artifacts/orchestration/parallel-orchestrator-state.json'
$script:AllowedMergeStatuses = @('merged', 'worktree_removed')

function Get-EpicWorktreeGateCheckpointContent {
    <#
    .SYNOPSIS
        Read the raw JSON text of the epic checkpoint. Tests mock this function
        (read seam).
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param()

    if (-not (Test-Path -LiteralPath $script:EpicCheckpointPath -PathType Leaf)) {
        return $null
    }
    return (Get-Content -LiteralPath $script:EpicCheckpointPath -Raw)
}

function Get-EpicWorktreeGateParallelCheckpointContent {
    <#
    .SYNOPSIS
        Read the raw JSON text of the parallel-orchestrator checkpoint. Tests mock
        this function (read seam).
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

function ConvertFrom-EpicWorktreeGateJson {
    <#
    .SYNOPSIS
        Parse checkpoint JSON text, returning $null on unreadable/invalid content.
    .DESCRIPTION
        Shared by both authorization branches so the two checkpoints are parsed by
        one implementation rather than by duplicated inline logic.
    .PARAMETER Raw
        Raw checkpoint text, or $null when the file does not exist.
    .OUTPUTS
        System.Object or $null
    #>
    [CmdletBinding()]
    param(
        [AllowNull()]
        [string] $Raw
    )

    if ([string]::IsNullOrWhiteSpace($Raw)) {
        return $null
    }
    try {
        return ($Raw | ConvertFrom-Json -ErrorAction Stop)
    } catch {
        return $null
    }
}

function Get-EpicWorktreeRemovalCommandPath {
    <#
    .SYNOPSIS
        Extract the target worktree path argument from a git worktree remove command.
    .PARAMETER CommandText
        The Bash command text under evaluation.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [string] $CommandText
    )

    if ($CommandText -match '(?i)\bgit\s+worktree\s+remove\s+(?<path>\S+)') {
        return $Matches['path'].Trim('"''')
    }
    return $null
}

function Find-EpicWorktreeFeatureRecord {
    <#
    .SYNOPSIS
        Locate the features[] record whose worktree_path matches the target path.
    .PARAMETER Checkpoint
        Parsed epic checkpoint, or $null when absent/unreadable.
    .PARAMETER WorktreePath
        The target worktree path extracted from the command text.
    .OUTPUTS
        System.Object or $null
    #>
    [CmdletBinding()]
    param(
        [AllowNull()]
        $Checkpoint,

        [AllowNull()]
        [string] $WorktreePath
    )

    if ($null -eq $Checkpoint -or [string]::IsNullOrWhiteSpace($WorktreePath)) {
        return $null
    }
    $checkpointProps = @($Checkpoint.PSObject.Properties.Name)
    if ($checkpointProps -notcontains 'features') {
        return $null
    }

    $normalizedTarget = ($WorktreePath -replace '\\', '/').TrimEnd('/')

    # Scan every recorded feature for a worktree_path that matches the removal target;
    # path separators are normalized so Windows- and POSIX-style paths compare equal.
    foreach ($feature in @($Checkpoint.features)) {
        $featureProps = @($feature.PSObject.Properties.Name)
        if ($featureProps -notcontains 'worktree_path') {
            continue
        }
        $normalizedFeaturePath = (([string]$feature.worktree_path) -replace '\\', '/').TrimEnd('/')
        if ($normalizedFeaturePath -eq $normalizedTarget) {
            return $feature
        }
    }
    return $null
}

function Test-EpicWorktreeRemovalAllowed {
    <#
    .SYNOPSIS
        Decision logic: allow only when the matching feature record's merge_status is
        merged or worktree_removed.
    .PARAMETER FeatureRecord
        The matched features[] record, or $null when no match was found.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()]
        $FeatureRecord
    )

    if ($null -eq $FeatureRecord) {
        return $false
    }
    $props = @($FeatureRecord.PSObject.Properties.Name)
    if ($props -notcontains 'merge_status') {
        return $false
    }
    return $script:AllowedMergeStatuses -contains ([string]$FeatureRecord.merge_status)
}

function Test-ParallelCheckpointAllowsWorktreeRemoval {
    <#
    .SYNOPSIS
        Decision logic for the parallel-orchestrator checkpoint path (branch 2).
    .DESCRIPTION
        Allows only when the checkpoint declares the parallel route identity and
        carries an items[] entry whose worktree_path matches the removal target and
        whose merge_status is in the allowed set. Every other shape returns $false,
        so the branch is a positive predicate with no negative path to an allow.
    .PARAMETER Checkpoint
        Parsed parallel-orchestrator checkpoint, or $null when absent/unreadable.
    .PARAMETER WorktreePath
        The target worktree path extracted from the command text.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()]
        $Checkpoint,

        [AllowNull()]
        [string] $WorktreePath
    )

    if ($null -eq $Checkpoint -or [string]::IsNullOrWhiteSpace($WorktreePath)) {
        return $false
    }
    $props = @($Checkpoint.PSObject.Properties.Name)
    # Route identity is orchestrator invariant 2, so a document at the parallel path
    # that does not declare it is malformed by the rule's own definition and inert here.
    if ($props -notcontains 'route_id' -or ([string]$Checkpoint.route_id) -ne 'parallel') {
        return $false
    }
    if ($props -notcontains 'items' -or $null -eq $Checkpoint.items) {
        return $false
    }

    $normalizedTarget = ($WorktreePath -replace '\\', '/').TrimEnd('/')

    # Scan every recorded item for a worktree_path that matches the removal target;
    # path separators are normalized exactly as the epic branch normalizes them.
    foreach ($item in @($Checkpoint.items)) {
        if ($null -eq $item) {
            continue
        }
        $itemProps = @($item.PSObject.Properties.Name)
        if ($itemProps -notcontains 'worktree_path') {
            continue
        }
        $normalizedItemPath = (([string]$item.worktree_path) -replace '\\', '/').TrimEnd('/')
        if ($normalizedItemPath -ne $normalizedTarget) {
            continue
        }
        if ($itemProps -notcontains 'merge_status') {
            return $false
        }
        return $script:AllowedMergeStatuses -contains ([string]$item.merge_status)
    }

    return $false
}

function Get-EpicWorktreeGateAllowDecision {
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

function Get-EpicWorktreeGateBlockDecision {
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

function Invoke-EpicWorktreeRemovalGateDecision {
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

    $payload = Resolve-ClaudeHookToolInput -Raw $ToolInputRaw
    if (-not $payload.IsValid) {
        return Get-EpicWorktreeGateBlockDecision -Reason (
            'EPIC_WORKTREE_REMOVAL_BLOCKED: payload anomaly - ' +
            (Get-ClaudeHookPayloadAnomalyReason -Anomaly $payload.Anomaly) +
            '. The gate fails closed on an envelope it cannot read.')
    }

    $commandText = Get-ClaudeHookToolInputString -ToolInput $payload.Value -Name 'command'
    if (-not $commandText) {
        return Get-EpicWorktreeGateAllowDecision
    }

    if ($commandText -notmatch '(?i)\bgit\s+worktree\s+remove\b') {
        return Get-EpicWorktreeGateAllowDecision
    }

    $worktreePath = Get-EpicWorktreeRemovalCommandPath -CommandText $commandText

    $checkpoint = ConvertFrom-EpicWorktreeGateJson -Raw (Get-EpicWorktreeGateCheckpointContent)

    $featureRecord = Find-EpicWorktreeFeatureRecord -Checkpoint $checkpoint -WorktreePath $worktreePath
    if (Test-EpicWorktreeRemovalAllowed -FeatureRecord $featureRecord) {
        return Get-EpicWorktreeGateAllowDecision
    }

    $parallelCheckpoint = ConvertFrom-EpicWorktreeGateJson -Raw (Get-EpicWorktreeGateParallelCheckpointContent)
    if (Test-ParallelCheckpointAllowsWorktreeRemoval -Checkpoint $parallelCheckpoint -WorktreePath $worktreePath) {
        return Get-EpicWorktreeGateAllowDecision
    }

    return Get-EpicWorktreeGateBlockDecision -Reason "EPIC_WORKTREE_REMOVAL_BLOCKED: git worktree remove for '$worktreePath' requires either an epic checkpoint features[] record with merge_status in {merged, worktree_removed}, or a parallel-orchestrator checkpoint with route_id == ""parallel"" whose matching items[] record (matched by worktree_path) has merge_status in {merged, worktree_removed}. No checkpoint authorized this removal."
}

function Invoke-EpicWorktreeRemovalGateEntryPoint {
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

    $decision = Invoke-EpicWorktreeRemovalGateDecision -ToolInputRaw $ToolInputRaw
    $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output

    return 0
}

# Guard allows dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}

# The entry point returns its [int] exit code as the last pipeline element and the
# decision JSON before it. `exit (<call>)` would capture BOTH into the exit
# expression and emit nothing, so the decision is written explicitly here first.
$entryPointResult = @(Invoke-EpicWorktreeRemovalGateEntryPoint)
if ($entryPointResult.Count -gt 1) {
    $entryPointResult[0..($entryPointResult.Count - 2)] | Write-Output
}

exit ([int]$entryPointResult[-1])