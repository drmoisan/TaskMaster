<#
.SYNOPSIS
    Pre-tool-use hook that gates every pull-request merge or merge-queue enqueue behind checkpoint state.

.DESCRIPTION
    Invoked by the Claude Code PreToolUse hook on the "Bash" matcher before any Bash
    command runs. Brings every gh pr merge invocation except a pure --disable-auto, and every gh
    api merge or enqueue mutation, into scope from the envelope's tool_input.command and, when
    matched, allows the action only when one of four
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
      4. Standalone path (issue #670), evaluated last: the command names an explicit PR
         number and one of the three checkpoints above carries a
         standalone_merge_authorizations record whose pr_number equals it, whose fields are
         well formed, and whose session_id equals the live envelope's session_id. The
         dot-sourced helpers file implements this branch.

    Otherwise the command is denied with reason EPIC_MERGE_GATE_BLOCKED; a denial from
    condition 4 also carries a standalone reason code after that token. A missing or
    unreadable checkpoint in any branch fails closed (denies). A standalone (non-epic,
    non-parallel) run never satisfies conditions 1-3, so its only route to a merge is an
    explicit, PR-specific, session-bound record under condition 4; a bare command with no
    PR number never satisfies condition 4, and a blanket flag is rejected rather than read.
    Trigger scope (issue #952): every merge method (--merge/-m, --squash/-s, --rebase/-r), --auto,
    --admin, and a bare gh pr merge are gated alike, because under a merge queue each spelling
    enqueues the pull request; only a pure --disable-auto is out of scope. The predicate and the
    PR selector reader live in enforce-epic-merge-gate-command.ps1.

    Design decision: branches 1-4 trust the on-disk checkpoint and never call gh. Issue #927
    adds a docs-only branch after branch 4 fails, only for an explicit PR number whose item,
    epic, and parallel targets are all NoTarget (no checkpoint records the PR): it calls
    Get-DocsOnlyMergeExemption (hook-docs-only-diff.ps1), which runs gh pr view and git in the
    session worktree and allows the merge when conditions M1-M9 of that file hold. Otherwise
    the deny gains " (docs-only exemption not applied: <Reason>)", where the reason may be
    DOCS_ONLY_EXEMPTION_PR_UNRESOLVED. The hook never fetches; --match-head-commit, when
    present, must equal headRefOid (OD-2). Residual limits: an ambient GH_REPO or GH_HOST is
    not visible in the command text; gh and git run in the session worktree root, so a cd
    made by an earlier Bash call is not visible either; and a push after the lookup can
    change the head unless --match-head-commit is passed. Like the receipt mechanism of
    enforce-pr-author-skill.ps1, this is a policy-level control, not a cryptographic one.

    Issue #690: with an explicit PR number, the epic and parallel checkpoints are read
    beneath the live worktree whose run checkpoint records that number. Issue #850: with
    an explicit PR number, the per-feature checkpoint is read beneath the live worktree
    whose checkpoint records the number in pr_gate.pr_number or a standalone record, and
    the child branch requires that checkpoint to bind the number (issue #788). A bare
    command reads every checkpoint beneath the session worktree.

.NOTES
    Compatible with PowerShell 7+. Depends on WorktreeRunResolution.psm1 through the
    dot-sourced enforce-epic-merge-gate-resolution.ps1, whose import guard denies on a
    failed import. Filesystem reads go through injectable wrapper functions so tests can
    mock the boundary without writing temporary files.

    Honest disclosure: the standalone-merge authorization record is a policy-level,
    auditable declaration and is not a cryptographic or security control. authorized_by is
    a declaration the hook does not verify. The record is not tamper-proof against a writer
    inside the authorizing session. The session_id cross-check rejects stale or copied
    records from other sessions; it does not stop same-session forgery.
#>
[CmdletBinding()]
param()
$script:HookDependencyGuardLoadFailed = $false; try { . (Join-Path $PSScriptRoot 'hook-dependency-guard.ps1') } catch { $script:HookDependencyGuardLoadFailed = $true }

try { Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'HookPayload.psm1' -ErrorRecord $_ }
# Shared command-line parser (issue #545), consumed by the scope filter in
# Invoke-EpicMergeGateDecision and by Get-EpicMergeGateCommandPrNumber.
try { . (Join-Path $PSScriptRoot 'hook-command-scanner.ps1') } catch { Add-HookDependencyFailure -Name 'hook-command-scanner.ps1' -ErrorRecord $_ }
try { . (Join-Path $PSScriptRoot 'hook-command-invocation.ps1') } catch { Add-HookDependencyFailure -Name 'hook-command-invocation.ps1' -ErrorRecord $_ }
# Standalone-merge authorization predicates and the two decision-envelope factories (issue #670).
try { . (Join-Path $PSScriptRoot 'enforce-epic-merge-gate-authorization.ps1') } catch { Add-HookDependencyFailure -Name 'enforce-epic-merge-gate-authorization.ps1' -ErrorRecord $_ }
# Checkpoint read seams, the import guard, and run-target resolution (issue #690).
try { . (Join-Path $PSScriptRoot 'enforce-epic-merge-gate-resolution.ps1') } catch { Add-HookDependencyFailure -Name 'enforce-epic-merge-gate-resolution.ps1' -ErrorRecord $_ }
# Merge and enqueue scope predicate and the PR selector reader (issue #952).
try { . (Join-Path $PSScriptRoot 'enforce-epic-merge-gate-command.ps1') } catch { Add-HookDependencyFailure -Name 'enforce-epic-merge-gate-command.ps1' -ErrorRecord $_ }
# Shared docs-only diff classification and the merge-side docs-only exemption (issue #927).
try { . (Join-Path $PSScriptRoot 'hook-docs-only-diff.ps1') } catch { Add-HookDependencyFailure -Name 'hook-docs-only-diff.ps1' -ErrorRecord $_ }

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
    .DESCRIPTION
        The number is resolved by Get-EpicMergeGateSelectorPrNumber in
        enforce-epic-merge-gate-command.ps1 (issue #952), from the segment that structurally
        invokes gh pr merge or gh api and never from the whole command line: value flags are
        skipped with their values, an all-digit or /pull/<n> URL selector is accepted, and a
        pulls/<n>/merge path supplies the number for gh api. A command with no resolvable number
        yields $null, which the parallel and standalone branches treat as fail-closed. No
        whole-line digit scan exists, so a cd path component can never supply the number.
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

    return Get-EpicMergeGateSelectorPrNumber -CommandText $CommandText
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

    # A failed worktree-resolution import denies before any other logic (issue #690).
    $dependencyDecision = Get-HookDependencyFailureDecision -HookEvent PreToolUse -ReasonPrefix 'EPIC_MERGE_GATE_BLOCKED:'
    if ($null -ne $dependencyDecision) { return $dependencyDecision }

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

    # Scope (issue #952): every gh pr merge invocation except a pure --disable-auto, and every gh api
    # merge or enqueue mutation, is gated; Test-EpicMergeGateCommandInScope holds the predicate.
    if (-not (Test-EpicMergeGateCommandInScope -CommandText $commandText)) {
        return Get-EpicMergeGateAllowDecision
    }

    $commandPrNumber = Get-EpicMergeGateCommandPrNumber -CommandText $commandText
    $sessionRoot = Get-EpicMergeGateSessionWorktreeRoot

    # With an explicit PR number, the per-feature checkpoint is read beneath the live
    # worktree whose checkpoint records the number in pr_gate.pr_number or a standalone
    # record (issue #850); an unresolved item target reads none. A bare command reads the
    # session worktree. The checkpoint read must still bind the number (issue #788).
    $itemTarget = $null
    $childRoot = $sessionRoot
    if ($null -ne $commandPrNumber) {
        $itemTarget = Resolve-EpicMergeGateItemTarget -PrNumber $commandPrNumber
        $childRoot = if (@('SessionRoot', 'OtherWorktree') -contains $itemTarget.Status) { $itemTarget.WorktreeRoot } else { $null }
    }
    $childCheckpoint = if ($childRoot) { ConvertFrom-EpicMergeGateJson -Raw (Get-ChildOrchestratorCheckpointContent -Path (Get-WorktreeRunCheckpointPath -Kind item -WorktreeRoot $childRoot)) } else { $null }
    if ((Test-ChildCheckpointAllowsEpicMerge -Checkpoint $childCheckpoint) -and
        (Test-ChildCheckpointPrGateBinding -Checkpoint $childCheckpoint -CommandPrNumber $commandPrNumber)) {
        return Get-EpicMergeGateAllowDecision
    }

    # A bare command targets the current branch's PR, so it reads the session worktree. An
    # explicit PR number locates each run checkpoint that records it (issue #690).
    $epicTarget = $null
    $parallelTarget = $null
    $epicRoot = $sessionRoot
    $parallelRoot = $sessionRoot
    if ($null -ne $commandPrNumber) {
        $epicTarget = Resolve-EpicMergeGateRunTarget -Kind epic -PrNumber $commandPrNumber
        $parallelTarget = Resolve-EpicMergeGateRunTarget -Kind parallel -PrNumber $commandPrNumber
        $epicRoot = if (@('SessionRoot', 'OtherWorktree') -contains $epicTarget.Status) { $epicTarget.WorktreeRoot } else { $null }
        $parallelRoot = if (@('SessionRoot', 'OtherWorktree') -contains $parallelTarget.Status) { $parallelTarget.WorktreeRoot } else { $null }
    }

    $epicCheckpoint = if ($epicRoot) { ConvertFrom-EpicMergeGateJson -Raw (Get-EpicOrchestratorCheckpointContent -Path (Get-WorktreeRunCheckpointPath -Kind epic -WorktreeRoot $epicRoot)) } else { $null }
    if (Test-EpicCheckpointAllowsMerge -Checkpoint $epicCheckpoint -CommandPrNumber $commandPrNumber) {
        return Get-EpicMergeGateAllowDecision
    }

    $parallelCheckpoint = if ($parallelRoot) { ConvertFrom-EpicMergeGateJson -Raw (Get-ParallelOrchestratorCheckpointContent -Path (Get-WorktreeRunCheckpointPath -Kind parallel -WorktreeRoot $parallelRoot)) } else { $null }
    if (Test-ParallelCheckpointAllowsMerge -Checkpoint $parallelCheckpoint -CommandPrNumber $commandPrNumber) {
        return Get-EpicMergeGateAllowDecision
    }

    # Branch 4, evaluated last and only for an explicit PR number: a PR-specific standalone
    # authorization record bound to the live session. Its deny keeps the gate token first.
    if ($null -ne $commandPrNumber) {
        $envelopeSessionId = [string](Get-ClaudeHookEnvelopeValue -Envelope $payload.Envelope -Name 'session_id')
        $standalone = Test-StandaloneCheckpointAllowsMerge -Checkpoints @($childCheckpoint, $epicCheckpoint, $parallelCheckpoint) -CommandPrNumber $commandPrNumber -EnvelopeSessionId $envelopeSessionId
        if ($standalone.Allowed) {
            return Get-EpicMergeGateAllowDecision
        }
        # Docs-only branch (issue #927): evaluated only when no checkpoint records the PR.
        $docsOnlySuffix = ''
        if (Test-EpicMergeGateNoRecordingTarget -ItemTarget $itemTarget -EpicTarget $epicTarget -ParallelTarget $parallelTarget) {
            $docsOnly = Get-DocsOnlyMergeExemption -CommandText $commandText -PrNumber $commandPrNumber -WorktreeRoot $sessionRoot
            if ($docsOnly.Exempt) {
                return Get-EpicMergeGateAllowDecision
            }
            $docsOnlySuffix = ' (docs-only exemption not applied: ' + $docsOnly.Reason + ')'
        }
        # When neither the item target nor a run branch resolved, the reason code and the
        # three details precede the standalone reason (issue #850).
        $unresolved = Get-EpicMergeGateUnresolvedReason -ItemTarget $itemTarget -EpicTarget $epicTarget -ParallelTarget $parallelTarget
        $prefix = if ($unresolved) { $unresolved + '; ' } else { '' }
        return Get-EpicMergeGateBlockDecision -Reason ('EPIC_MERGE_GATE_BLOCKED: ' + $prefix + $standalone.ReasonCode + ': ' + $standalone.Message + $docsOnlySuffix)
    }

    return Get-EpicMergeGateBlockDecision -Reason 'EPIC_MERGE_GATE_BLOCKED: a pull-request merge or merge-queue enqueue (gh pr merge with any method, --auto, or --admin, or a gh api merge or enqueue mutation) requires either a per-feature checkpoint with epic_mode == true and step9_status == "passed", an epic checkpoint with epic_merge_pr.ci_gate.conclusion == "success" and a matching pr_number, or a parallel-orchestrator checkpoint with route_id == "parallel" whose target item (matched by pr_number) has merge_status == "ci_green". No checkpoint satisfied this gate.'
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
if ($script:HookDependencyGuardLoadFailed) { [Console]::Error.WriteLine('EPIC_MERGE_GATE_BLOCKED: hook-dependency-guard.ps1 failed to load; the gate fails closed.'); exit 2 }
$dependencyDecision = Get-HookDependencyFailureDecision -HookEvent PreToolUse -ReasonPrefix 'EPIC_MERGE_GATE_BLOCKED:'
if ($null -ne $dependencyDecision) { $dependencyDecision | ConvertTo-Json -Compress -Depth 5 | Write-Output; exit 0 }

# The entry point returns its [int] exit code as the last pipeline element and the
# decision JSON before it. `exit (<call>)` would capture BOTH into the exit
# expression and emit nothing, so the decision is written explicitly here first.
$entryPointResult = @(Invoke-EpicMergeGateEntryPoint)
if ($entryPointResult.Count -gt 1) {
    $entryPointResult[0..($entryPointResult.Count - 2)] | Write-Output
}

exit ([int]$entryPointResult[-1])
