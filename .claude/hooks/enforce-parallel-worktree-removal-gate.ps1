<#
.SYNOPSIS
    Pre-tool-use hook that gates git worktree remove behind parallel checkpoint merge state.

.DESCRIPTION
    Invoked by the Claude Code PreToolUse hook on the "Bash" matcher before any Bash
    command runs. Regex-matches git worktree remove against the envelope's tool_input.command,
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


Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force
# Sanctioned-removal manifest reader (issue #635), consumed by the manifest branch in
# Invoke-ParallelWorktreeRemovalGateDecision. Same module the epic gate imports.
Import-Module (Join-Path $PSScriptRoot '../lib/cleanup-manifest/CleanupWorktreeManifest.psm1') -Force
# Shared command-line parser (issue #545), consumed by the scope filter in
# Invoke-ParallelWorktreeRemovalGateDecision and by Get-ParallelWorktreeRemovalCommandPath.
# Both call sites are byte-for-byte the calls the epic gate makes, so the duplicated
# concern now has one implementation.
. (Join-Path $PSScriptRoot 'hook-command-scanner.ps1')
. (Join-Path $PSScriptRoot 'hook-command-invocation.ps1')
$script:ParallelCheckpointPath = 'artifacts/orchestration/parallel-orchestrator-state.json'
$script:AllowedMergeStatuses = @('merged', 'worktree_removed')
# Sanctioned-removal manifest location. Recorded here beside the checkpoint path so
# every hook-read document this gate consults is named in one place; the module owns
# the read itself.
$script:CleanupWorktreeManifestPath = 'artifacts/orchestration/cleanup-worktrees-manifest.json'

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
    .DESCRIPTION
        The operand comes from the segment that structurally invokes git worktree remove,
        so a 'cd <path> &&' segment chained before the removal contributes nothing and a
        quoted mention of the phrase resolves to no operand at all. Quotes around the path
        are already stripped by the tokenizer.

        '--force' is a zero-argument flag, so it never contributes an operand and may be
        written on either side of the target path. Its presence is read structurally through
        Test-CommandLineFlag rather than by searching the raw text, so a '--force' spelling
        that appears inside an unrelated quoted argument cannot change how the operand list
        is read. When no operand resolves, a present '--force' is reported in its place, so
        the checkpoint lookup fails closed on a value that matches no recorded worktree_path
        - the same value the previous raw-text pattern returned for that input.

        This body is identical to Get-EpicWorktreeRemovalCommandPath in
        enforce-epic-worktree-removal-gate.ps1. The two gates fire on the same command and
        now delegate the shared concern to one parser rather than to two patterns that had
        already diverged from the Codex copy.
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

    $hasForce = Test-CommandLineFlag -CommandText $CommandText -CommandWord 'git' -SubcommandPath @('worktree', 'remove') -FlagName '--force'
    $operands = @(Get-CommandLineOperand -CommandText $CommandText -CommandWord 'git' -SubcommandPath @('worktree', 'remove'))

    if ($operands.Count -gt 0) {
        return $operands[0]
    }
    if ($hasForce) {
        return '--force'
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
        return Get-ParallelWorktreeGateBlockDecision -Reason (
            'PARALLEL_WORKTREE_REMOVAL_BLOCKED: payload anomaly - ' +
            (Get-ClaudeHookPayloadAnomalyReason -Anomaly $payload.Anomaly) +
            '. The gate fails closed on an envelope it cannot read.')
    }

    $commandText = Get-ClaudeHookToolInputString -ToolInput $payload.Value -Name 'command'
    if (-not $commandText) {
        return Get-ParallelWorktreeGateAllowDecision
    }

    # Scope filter. The test is structural, so a relocating spelling such as
    # 'git -C <dir> worktree remove <path>' is now in scope and quoted prose that merely
    # mentions the phrase is not (issue #545). This is the same call the epic gate makes.
    if (-not (Test-CommandLineInvocation -CommandText $commandText -CommandWord 'git' -SubcommandPath @('worktree', 'remove'))) {
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

    # Sanctioned-removal manifest branch (issue #635). It runs last, so a
    # checkpoint-authorized removal still allows at the same decision point it does
    # today and no transcript attribution changes.
    #
    # The coverage test is a PRESENCE test, deliberately not an authorization test. A
    # target this checkpoint records at all is excluded from this branch regardless of
    # that record's merge_status, so a removal the checkpoint does not authorize still
    # reaches the unchanged deny below. This gate defines only the parallel checkpoint
    # seam and has no epic-checkpoint seam, so its exclusion covers the items array
    # only; the epic gate covers the features array with its own exclusion.
    if (-not (Test-CleanupManifestCheckpointCoversPath -Checkpoint $checkpoint -RecordArrayName 'items' -WorktreePath $worktreePath) -and
        (Test-CleanupWorktreeManifestAuthorizesRemoval -WorktreePath $worktreePath)) {
        return Get-ParallelWorktreeGateAllowDecision
    }

    return Get-ParallelWorktreeGateBlockDecision -Reason "PARALLEL_WORKTREE_REMOVAL_BLOCKED: git worktree remove for '$worktreePath' requires a matching parallel checkpoint items[] record with merge_status in {merged, worktree_removed}. The checkpoint was unreadable, no matching record was found, or merge_status was not yet safe for removal."
}

function Invoke-ParallelWorktreeRemovalGateEntryPoint {
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

    $decision = Invoke-ParallelWorktreeRemovalGateDecision -ToolInputRaw $ToolInputRaw
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
$entryPointResult = @(Invoke-ParallelWorktreeRemovalGateEntryPoint)
if ($entryPointResult.Count -gt 1) {
    $entryPointResult[0..($entryPointResult.Count - 2)] | Write-Output
}

exit ([int]$entryPointResult[-1])