<#
.SYNOPSIS
    Pre-tool-use hook that gates gh pr merge --merge behind epic-mode checkpoint state.

.DESCRIPTION
    Invoked by the Claude Code PreToolUse hook on the "Bash" matcher before any Bash
    command runs. Regex-matches gh pr merge with a --merge flag against
    the envelope's tool_input.command and, when matched, allows the merge only when one of three
    checkpoint-only conditions holds:

      1. Child-feature path: artifacts/orchestration/orchestrator-state.json exists,
         epic_mode == true, and step9_status == "passed" (the per-feature orchestrator has
         already run S9 step 6's CI-green gate before attempting its own merge-on-green).
      2. Epic-integration path: artifacts/orchestration/epic-orchestrator-state.json exists,
         epic_merge_pr.ci_gate.conclusion == "success", and, when the command names an
         explicit PR number, that number matches epic_merge_pr.pr_number.
      3. Parallel path: artifacts/orchestration/parallel-orchestrator-state.json exists,
         route_id == "parallel", and the command's explicit PR number matches an items[]
         entry whose merge_status == "ci_green". A parallel run always names an explicit PR
         number (each item merges from its own isolated worktree), so a bare command with no
         PR number cannot satisfy this branch.

    Otherwise the command is denied with reason EPIC_MERGE_GATE_BLOCKED. A missing or
    unreadable checkpoint in any branch fails closed (denies); standalone (non-epic,
    non-parallel) orchestration never sets epic_mode, populates epic_merge_pr, or writes a
    parallel checkpoint with route_id == "parallel", so it is structurally prevented from
    invoking gh pr merge --merge at all.

    Design decision: this gate trusts the on-disk checkpoint rather than shelling out live
    to gh pr view for a real-time head-SHA check, matching the same non-adversarial,
    policy-level-not-cryptographic posture already accepted for
    enforce-pr-author-skill.ps1's own receipt mechanism. It is not a cryptographic control.

.NOTES
    Compatible with PowerShell 7+. No external module dependencies. Filesystem reads go
    through injectable wrapper functions so tests can mock the boundary without writing
    temporary files.
#>
[CmdletBinding()]
param()

Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force

$script:ChildCheckpointPath = 'artifacts/orchestration/orchestrator-state.json'
$script:EpicCheckpointPath = 'artifacts/orchestration/epic-orchestrator-state.json'
$script:ParallelCheckpointPath = 'artifacts/orchestration/parallel-orchestrator-state.json'

function Get-ChildOrchestratorCheckpointContent {
    <#
    .SYNOPSIS
        Read the raw JSON text of the per-feature orchestrator checkpoint. Tests mock
        this function (read seam).
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param()

    if (-not (Test-Path -LiteralPath $script:ChildCheckpointPath -PathType Leaf)) {
        return $null
    }
    return (Get-Content -LiteralPath $script:ChildCheckpointPath -Raw)
}

function Get-EpicOrchestratorCheckpointContent {
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

function Get-ParallelOrchestratorCheckpointContent {
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

function ConvertFrom-EpicMergeGateJson {
    <#
    .SYNOPSIS
        Parse checkpoint JSON text, returning $null on unreadable/invalid content.
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

function Get-EpicMergeGateCommandPrNumber {
    <#
    .SYNOPSIS
        Extract an explicit PR number argument from a gh pr merge command, or $null.
    .PARAMETER CommandText
        The Bash command text under evaluation.
    .OUTPUTS
        System.Nullable[int]
    #>
    [CmdletBinding()]
    [OutputType([int])]
    param(
        [Parameter(Mandatory)]
        [string] $CommandText
    )

    # Original form: the PR number appears immediately after "merge"
    # (e.g. "gh pr merge 410 --merge"). Preserved verbatim so epic-path outcomes
    # for the forms the epic path uses are unchanged.
    if ($CommandText -match '(?i)\bgh\s+pr\s+merge\s+(\d+)\b') {
        return [int]$Matches[1]
    }
    # Broadened, additive form: the parallel command places the flag before the
    # number (e.g. "gh pr merge --merge 410"). Once "gh pr merge" is confirmed,
    # capture the first standalone run of digits that is not preceded by "-" or a
    # word character, so a flag token such as "--merge" is not treated as a number
    # and a bare "gh pr merge --merge" still yields $null.
    if ($CommandText -match '(?i)\bgh\s+pr\s+merge\b' -and $CommandText -match '(?<![-\w])(\d+)\b') {
        return [int]$Matches[1]
    }
    return $null
}

function Test-ChildCheckpointAllowsEpicMerge {
    <#
    .SYNOPSIS
        Decision logic for the child-feature checkpoint path (branch 1).
    .PARAMETER Checkpoint
        Parsed child orchestrator checkpoint, or $null when absent/unreadable.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()]
        $Checkpoint
    )

    if ($null -eq $Checkpoint) {
        return $false
    }
    $props = @($Checkpoint.PSObject.Properties.Name)
    if ($props -notcontains 'epic_mode' -or -not [bool]$Checkpoint.epic_mode) {
        return $false
    }
    if ($props -notcontains 'step9_status') {
        return $false
    }
    return ([string]$Checkpoint.step9_status) -eq 'passed'
}

function Test-EpicCheckpointAllowsMerge {
    <#
    .SYNOPSIS
        Decision logic for the epic-integration checkpoint path (branch 2).
    .PARAMETER Checkpoint
        Parsed epic checkpoint, or $null when absent/unreadable.
    .PARAMETER CommandPrNumber
        The explicit PR number parsed from the command, or $null when the command
        does not name one (implying the current branch's PR).
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()]
        $Checkpoint,

        [AllowNull()]
        [Nullable[int]] $CommandPrNumber
    )

    if ($null -eq $Checkpoint) {
        return $false
    }
    $props = @($Checkpoint.PSObject.Properties.Name)
    if ($props -notcontains 'epic_merge_pr' -or $null -eq $Checkpoint.epic_merge_pr) {
        return $false
    }
    $mergePr = $Checkpoint.epic_merge_pr
    $mergePrProps = @($mergePr.PSObject.Properties.Name)
    if ($mergePrProps -notcontains 'ci_gate' -or $null -eq $mergePr.ci_gate) {
        return $false
    }
    $ciGateProps = @($mergePr.ci_gate.PSObject.Properties.Name)
    if ($ciGateProps -notcontains 'conclusion' -or ([string]$mergePr.ci_gate.conclusion) -ne 'success') {
        return $false
    }

    # When the command names an explicit PR number, it must match the checkpoint's
    # recorded epic_merge_pr.pr_number; a bare "gh pr merge --merge" implicitly targets
    # the current branch's PR and is trusted per the checkpoint-only design decision.
    if ($null -ne $CommandPrNumber) {
        if ($mergePrProps -notcontains 'pr_number') {
            return $false
        }
        $checkpointPrNumber = 0
        if (-not [int]::TryParse([string]$mergePr.pr_number, [ref] $checkpointPrNumber)) {
            return $false
        }
        if ($checkpointPrNumber -ne $CommandPrNumber) {
            return $false
        }
    }

    return $true
}

function Test-ParallelCheckpointAllowsMerge {
    <#
    .SYNOPSIS
        Decision logic for the parallel-orchestrator checkpoint path (branch 3).
    .PARAMETER Checkpoint
        Parsed parallel-orchestrator checkpoint, or $null when absent/unreadable.
    .PARAMETER CommandPrNumber
        The explicit PR number parsed from the command, or $null when the command
        does not name one. A parallel run always names an explicit PR number because
        each item merges from its own isolated worktree, so a $null value denies.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()]
        $Checkpoint,

        [AllowNull()]
        [Nullable[int]] $CommandPrNumber
    )

    if ($null -eq $Checkpoint) {
        return $false
    }
    $props = @($Checkpoint.PSObject.Properties.Name)
    if ($props -notcontains 'route_id' -or ([string]$Checkpoint.route_id) -ne 'parallel') {
        return $false
    }
    # A parallel merge always names an explicit PR number; without one the target
    # item cannot be identified, so fail closed.
    if ($null -eq $CommandPrNumber) {
        return $false
    }
    if ($props -notcontains 'items' -or $null -eq $Checkpoint.items) {
        return $false
    }

    foreach ($item in @($Checkpoint.items)) {
        if ($null -eq $item) {
            continue
        }
        $itemProps = @($item.PSObject.Properties.Name)
        if ($itemProps -notcontains 'pr_number') {
            continue
        }
        $itemPrNumber = 0
        if (-not [int]::TryParse([string]$item.pr_number, [ref] $itemPrNumber)) {
            continue
        }
        if ($itemPrNumber -ne $CommandPrNumber) {
            continue
        }
        if ($itemProps -notcontains 'merge_status') {
            return $false
        }
        return ([string]$item.merge_status) -eq 'ci_green'
    }

    return $false
}

function Get-EpicMergeGateAllowDecision {
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

function Get-EpicMergeGateBlockDecision {
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

function Invoke-EpicMergeGateDecision {
    <#
    .SYNOPSIS
        Parses the PreToolUse envelope and returns an allow-or-block decision.
    .DESCRIPTION
        Envelope-level anomalies (empty payload, unparseable JSON, missing or
        malformed tool_input) fail closed as a deny; the legacy flat root shape is
        one such anomaly. Property-level absence of command inside a well-formed
        tool_input remains an allow, because that is this gate's scope filter.
    .PARAMETER ToolInputRaw
        The raw JSON hook payload acquired by Read-ClaudeHookRawPayload.
    .OUTPUTS
        System.Collections.Specialized.OrderedDictionary
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $ToolInputRaw
    )

    $payload = Resolve-ClaudeHookToolInput -Raw $ToolInputRaw
    if (-not $payload.IsValid) {
        return Get-EpicMergeGateBlockDecision -Reason (
            'EPIC_MERGE_GATE_BLOCKED: payload anomaly - ' +
            (Get-ClaudeHookPayloadAnomalyReason -Anomaly $payload.Anomaly) +
            '. The gate fails closed on an envelope it cannot read.')
    }

    $commandText = Get-ClaudeHookToolInputString -ToolInput $payload.Value -Name 'command'
    if (-not $commandText) {
        return Get-EpicMergeGateAllowDecision
    }

    # Only a gh pr merge invocation carrying --merge is in scope for this gate; every
    # other Bash command is unaffected.
    if ($commandText -notmatch '(?i)\bgh\s+pr\s+merge\b' -or $commandText -notmatch '--merge\b') {
        return Get-EpicMergeGateAllowDecision
    }

    $commandPrNumber = Get-EpicMergeGateCommandPrNumber -CommandText $commandText

    $childCheckpoint = ConvertFrom-EpicMergeGateJson -Raw (Get-ChildOrchestratorCheckpointContent)
    if (Test-ChildCheckpointAllowsEpicMerge -Checkpoint $childCheckpoint) {
        return Get-EpicMergeGateAllowDecision
    }

    $epicCheckpoint = ConvertFrom-EpicMergeGateJson -Raw (Get-EpicOrchestratorCheckpointContent)
    if (Test-EpicCheckpointAllowsMerge -Checkpoint $epicCheckpoint -CommandPrNumber $commandPrNumber) {
        return Get-EpicMergeGateAllowDecision
    }

    $parallelCheckpoint = ConvertFrom-EpicMergeGateJson -Raw (Get-ParallelOrchestratorCheckpointContent)
    if (Test-ParallelCheckpointAllowsMerge -Checkpoint $parallelCheckpoint -CommandPrNumber $commandPrNumber) {
        return Get-EpicMergeGateAllowDecision
    }

    return Get-EpicMergeGateBlockDecision -Reason 'EPIC_MERGE_GATE_BLOCKED: gh pr merge --merge requires either a per-feature checkpoint with epic_mode == true and step9_status == "passed", an epic checkpoint with epic_merge_pr.ci_gate.conclusion == "success" and a matching pr_number, or a parallel-orchestrator checkpoint with route_id == "parallel" whose target item (matched by pr_number) has merge_status == "ci_green". No checkpoint satisfied this gate.'
}

function Invoke-EpicMergeGateEntryPoint {
    <#
    .SYNOPSIS
        Runs the merge-gate decision and returns the process exit code.
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

    $decision = Invoke-EpicMergeGateDecision -ToolInputRaw $ToolInputRaw
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
$entryPointResult = @(Invoke-EpicMergeGateEntryPoint)
if ($entryPointResult.Count -gt 1) {
    $entryPointResult[0..($entryPointResult.Count - 2)] | Write-Output
}

exit ([int]$entryPointResult[-1])