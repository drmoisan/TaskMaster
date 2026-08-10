<#
.SYNOPSIS
    Pre-tool-use hook that gates git worktree remove behind parallel checkpoint merge state.

.DESCRIPTION
    Invoked by the Claude Code PreToolUse hook on the "Bash" matcher before any Bash
    command runs. Regex-matches git worktree remove against CLAUDE_TOOL_INPUT.command,
    extracts the target worktree path argument, reads
    artifacts/orchestration/parallel-orchestrator-state.json, and finds the items[] record
    whose worktree_path matches. Allows removal only when that record's merge_status is
    merged or worktree_removed. Denies with reason PARALLEL_WORKTREE_REMOVAL_BLOCKED when
    the checkpoint is unreadable, no matching record exists, or merge_status is anything
    else - fail-closed, following the enforce-epic-worktree-removal-gate.ps1 precedent of
    treating an unreadable/no-match checkpoint as deny.

    Adapted from enforce-epic-worktree-removal-gate.ps1. The command interception regexes
    and the path normalization are unchanged; the checkpoint path, the read seam name, and
    the record collection differ, because the parallel surface records per-item state in
    items[] rather than features[]. A parallel run has no integration branch: each item
    opens its own pull request against main, so a removed worktree is unrecoverable work
    unless that item's own merge has been durably confirmed.

.NOTES
    Compatible with PowerShell 7+. No external module dependencies. Filesystem reads go
    through an injectable wrapper function so tests can mock the boundary without writing
    temporary files.
#>
[CmdletBinding()]
param()

$script:ParallelCheckpointPath = 'artifacts/orchestration/parallel-orchestrator-state.json'
$script:AllowedMergeStatuses = @('merged', 'worktree_removed')

function Get-ParallelWorktreeRemovalGateCheckpointContent {
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

function Get-ParallelWorktreeRemovalCommandPath {
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

function Find-ParallelWorktreeItemRecord {
    <#
    .SYNOPSIS
        Locate the items[] record whose worktree_path matches the target path.
    .PARAMETER Checkpoint
        Parsed parallel checkpoint, or $null when absent/unreadable.
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
    if ($checkpointProps -notcontains 'items') {
        return $null
    }

    $normalizedTarget = ($WorktreePath -replace '\\', '/').TrimEnd('/')

    # Scan every recorded item for a worktree_path that matches the removal target;
    # path separators are normalized so Windows- and POSIX-style paths compare equal.
    foreach ($item in @($Checkpoint.items)) {
        $itemProps = @($item.PSObject.Properties.Name)
        if ($itemProps -notcontains 'worktree_path') {
            continue
        }
        $normalizedItemPath = (([string]$item.worktree_path) -replace '\\', '/').TrimEnd('/')
        if ($normalizedItemPath -eq $normalizedTarget) {
            return $item
        }
    }
    return $null
}

function Test-ParallelWorktreeRemovalAllowed {
    <#
    .SYNOPSIS
        Decision logic: allow only when the matching item record's merge_status is
        merged or worktree_removed.
    .PARAMETER ItemRecord
        The matched items[] record, or $null when no match was found.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()]
        $ItemRecord
    )

    if ($null -eq $ItemRecord) {
        return $false
    }
    $props = @($ItemRecord.PSObject.Properties.Name)
    if ($props -notcontains 'merge_status') {
        return $false
    }
    return $script:AllowedMergeStatuses -contains ([string]$ItemRecord.merge_status)
}

function Get-ParallelWorktreeGateAllowDecision {
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

function Get-ParallelWorktreeGateBlockDecision {
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

function Invoke-ParallelWorktreeRemovalGateDecision {
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
        return Get-ParallelWorktreeGateAllowDecision
    }

    try {
        $toolInput = $ToolInputRaw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        throw "enforce-parallel-worktree-removal-gate hook received malformed JSON in CLAUDE_TOOL_INPUT: $_"
    }

    $commandText = $toolInput.command
    if (-not $commandText) {
        return Get-ParallelWorktreeGateAllowDecision
    }

    if ($commandText -notmatch '(?i)\bgit\s+worktree\s+remove\b') {
        return Get-ParallelWorktreeGateAllowDecision
    }

    $worktreePath = Get-ParallelWorktreeRemovalCommandPath -CommandText $commandText

    $checkpointRaw = Get-ParallelWorktreeRemovalGateCheckpointContent
    $checkpoint = $null
    if (-not [string]::IsNullOrWhiteSpace($checkpointRaw)) {
        try {
            $checkpoint = $checkpointRaw | ConvertFrom-Json -ErrorAction Stop
        } catch {
            $checkpoint = $null
        }
    }

    $itemRecord = Find-ParallelWorktreeItemRecord -Checkpoint $checkpoint -WorktreePath $worktreePath
    if (Test-ParallelWorktreeRemovalAllowed -ItemRecord $itemRecord) {
        return Get-ParallelWorktreeGateAllowDecision
    }

    return Get-ParallelWorktreeGateBlockDecision -Reason "PARALLEL_WORKTREE_REMOVAL_BLOCKED: git worktree remove for '$worktreePath' requires a matching parallel checkpoint items[] record with merge_status in {merged, worktree_removed}. The checkpoint was unreadable, no matching record was found, or merge_status was not yet safe for removal."
}

# Guard allows dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}

try {
    $decision = Invoke-ParallelWorktreeRemovalGateDecision -ToolInputRaw $env:CLAUDE_TOOL_INPUT
} catch {
    Write-Error $_
    exit 1
}

$decision | ConvertTo-Json -Compress -Depth 5 | Write-Output

exit 0
