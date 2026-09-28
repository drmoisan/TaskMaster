<#
.SYNOPSIS
    Pre-tool-use hook that blocks a delegation to a gated subagent when the
    orchestrator checkpoint records no model-routing receipt for that agent.

.DESCRIPTION
    Invoked by the Claude Code PreToolUse hook on the Agent (Task) tool. Reads
    tool input JSON from the envelope's nested tool_input, resolves which item the
    delegation is about, and reads that item's orchestrator checkpoint.

    The item is identified by portable identity only (issue #673): the canonical
    issue-number line in the delegation prompt, and a branch label. A feature-folder
    path never selects the worktree, because a merged folder exists in every checkout
    branched from main. A delegation the gate cannot identify is denied with a named
    reason code rather than checked against whichever checkpoint occupies the calling
    process's directory, which is how a sibling item's state used to produce an allow.

    The hook enforces presence only: it cannot read the delegate's chosen
    `model` (no `model` field is exposed in the tool input), so it verifies that
    a `model_routing_receipts[]` entry already exists for the target
    `subagent_type`. Correctness of the recorded model stays with the
    authoritative Python validator.

    Gated subagent types are the Agent-tool delegates that participate in model
    selection: atomic-planner, atomic-executor, feature-review, task-researcher,
    prd-feature, pr-author. The `orchestrator` type is deliberately excluded: it
    is the calling agent, not a subagent delegated via the Agent tool, so it is
    never a receipt-gated `subagent_type`.

    Allow-through (graceful allow) applies to a non-delegating `subagent_type`,
    empty or absent tool input, and malformed tool-input JSON.

    The checkpoint read goes through Get-ModelRoutingCheckpoint so tests can
    inject a synthetic checkpoint without touching disk. Its path is mandatory and
    carries no default: the caller supplies the absolute path of the resolved
    worktree's checkpoint, so the gate cannot fall back to a relative location.

    The delegation-identity contract this gate depends on is stated in
    .claude/skills/orchestrate/SKILL.md under `## Issue Number Consistency`: every
    delegation prompt to a receipt-gated subagent type carries the canonical issue
    number line and a `branch:` label naming the item's branch.

.NOTES
    Compatible with PowerShell 7+. Read-only presence-gating deterrent.
#>
[CmdletBinding()]
param()


Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force
# Portable-identity resolution (issue #673). Unguarded and fail-closed on purpose: a gate
# that cannot load its resolver must not degrade into the cwd-relative read it replaces.
Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/WorktreeItemResolution.psm1') -Force -ErrorAction Stop
# Epic scope (issue #663): a delegation for the epic integration branch is gated against the epic checkpoint.
Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/EpicScopeResolution.psm1') -Force -ErrorAction Stop
function Get-ModelRoutingCheckpoint {
    <#
    .SYNOPSIS
        Returns the parsed orchestrator checkpoint object, or $null when the
        file is missing or not valid JSON. Tests mock this seam.
    #>
    [CmdletBinding()]
    [OutputType([object])]
    param(
        [Parameter(Mandatory)]
        [string] $CheckpointPath
    )

    if (-not (Test-Path -LiteralPath $CheckpointPath -PathType Leaf)) {
        return $null
    }

    try {
        $raw = Get-Content -LiteralPath $CheckpointPath -Raw -ErrorAction Stop
        return $raw | ConvertFrom-Json -ErrorAction Stop
    }
    catch {
        return $null
    }
}

function Get-ModelRoutingGatedAgent {
    <#
    .SYNOPSIS
        Returns the set of subagent types that are receipt-gated: the Agent-tool
        delegates that participate in model selection. `orchestrator` is
        excluded because it is the caller, not a delegated subagent.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param()

    return [string[]] @(
        'atomic-planner',
        'atomic-executor',
        'feature-review',
        'task-researcher',
        'prd-feature',
        'pr-author'
    )
}

function Test-ModelRoutingReceiptPresent {
    <#
    .SYNOPSIS
        Returns $true when the checkpoint carries a model_routing_receipts entry
        whose agent equals the target subagent type.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)]
        [AllowNull()]
        $Checkpoint,

        [Parameter(Mandatory)]
        [string] $Subagent
    )

    if ($null -eq $Checkpoint) {
        return $false
    }
    if ($Checkpoint.PSObject.Properties.Name -notcontains 'model_routing_receipts') {
        return $false
    }

    # Scan every receipt for one whose agent matches the delegated subagent type.
    foreach ($receipt in @($Checkpoint.model_routing_receipts)) {
        if ($null -eq $receipt) {
            continue
        }
        if ($receipt.PSObject.Properties.Name -contains 'agent' -and
            [string]$receipt.agent -eq $Subagent) {
            return $true
        }
    }
    return $false
}

function Resolve-ModelRoutingWorktreeTarget {
    <#
    .SYNOPSIS
        Resolve the delegation's target worktree. Tests mock this function (resolution seam).
    .DESCRIPTION
        A seam rather than a direct call because the resolver reads real git state: the
        worktree list, each registration's branch, and the filesystem. A test that drove it
        directly would depend on whichever worktrees exist on the machine running it.
    .PARAMETER PromptText
        The delegation prompt, which is the only field scanned for identity.
    .OUTPUTS
        The target result object from Resolve-WorktreeItemTarget.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $PromptText
    )

    return (Resolve-WorktreeItemTarget -Text $PromptText -SessionRoot (Get-Location).Path)
}

function Get-ModelRoutingTargetCheckpointResolution {
    <#
    .SYNOPSIS
        Resolve the checkpoint path this delegation must be checked against.
    .DESCRIPTION
        Maps the four resolution states onto a checkpoint path or a deny reason. The two
        resolved states share one branch because the path is composed the same way for
        both: identity selects the worktree, and the checkpoint is read beneath it. The two
        unresolved states deny, carrying the code the worktree-resolution accessors supply;
        neither literal appears in this file.
    .PARAMETER PromptText
        The delegation prompt to resolve identity from.
    .OUTPUTS
        System.Collections.Specialized.OrderedDictionary with CheckpointPath and Reason.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $PromptText
    )

    $target = Resolve-ModelRoutingWorktreeTarget -PromptText $PromptText

    switch ($target.Status) {
        { $_ -in @('SessionRoot', 'OtherWorktree') } {
            return [ordered]@{ CheckpointPath = (Get-WorktreeItemCheckpointPath -WorktreeRoot $target.WorktreeRoot); Reason = $null }
        }
        default {
            $reason = "$($target.ReasonCode): $($target.Detail) The model-routing gate will not check this delegation against a checkpoint that may belong to a different item. Put the line 'Canonical issue number for this feature is <N>.' and a 'branch: <item branch>' label in the delegation prompt so the gate can identify the item."
            return [ordered]@{ CheckpointPath = $null; Reason = $reason }
        }
    }
}

function Invoke-ModelRoutingReceiptDecision {
    <#
    .SYNOPSIS
        Parses the envelope's nested tool_input and returns an allow-or-block decision object.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [string] $ToolInputRaw
    )

    $allow = [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }

    # Envelope-level anomalies fail closed (issue #501): an unreadable envelope means
    # the PreToolUse contract drifted, which is exactly the condition that made every
    # gate in this runtime inert, so it must deny rather than pass through.
    $envelope = Resolve-ClaudeHookToolInput -Raw $ToolInputRaw
    if (-not $envelope.IsValid) {
        return [ordered]@{
            hookSpecificOutput = [ordered]@{
                hookEventName            = 'PreToolUse'
                permissionDecision       = 'deny'
                permissionDecisionReason = 'MODEL_ROUTING_RECEIPT_BLOCKED: payload anomaly - ' +
                (Get-ClaudeHookPayloadAnomalyReason -Anomaly $envelope.Anomaly) +
                '. The gate fails closed on an envelope it cannot read.'
            }
        }
    }

    $subagent = Get-ClaudeHookToolInputString -ToolInput $envelope.Value -Name 'subagent_type'

    # Only the gated Agent-tool delegates are receipt-checked; any other
    # subagent_type (including orchestrator) passes through.
    if (-not $subagent -or ((Get-ModelRoutingGatedAgent) -notcontains $subagent)) {
        return $allow
    }

    # Identity resolution sits after the scope filter and before the checkpoint read, so a
    # delegation outside the gated set is still allowed however unresolvable its target is,
    # and a gated delegation the gate cannot identify is refused rather than answered from
    # unrelated state. An absent prompt is passed as an empty string, which resolves to no
    # identity and therefore to the same deny.
    $prompt = [string](Get-ClaudeHookToolInputString -ToolInput $envelope.Value -Name 'prompt')

    # Decision: a prompt whose branch label equals the epic checkpoint's integration_branch is
    # epic scope (issue #663), and the receipt is looked up in the epic checkpoint without
    # running per-feature identity resolution. Any other prompt falls through unchanged.
    $epicScope = Resolve-EpicScopeCheckpoint -Text $prompt -SessionRoot (Get-Location).Path
    if ($epicScope.IsEpicScope) {
        if (Test-ModelRoutingReceiptPresent -Checkpoint $epicScope.Checkpoint -Subagent $subagent) {
            return $allow
        }
        return [ordered]@{
            hookSpecificOutput = [ordered]@{
                hookEventName            = 'PreToolUse'
                permissionDecision       = 'deny'
                permissionDecisionReason = "MODEL_ROUTING_RECEIPT_BLOCKED: cannot delegate to '$subagent' in epic scope before a model_routing_receipts entry for it is recorded in $($epicScope.CheckpointPath); the failed readiness predicate is 'model_routing_receipts'."
            }
        }
    }

    $resolution = Get-ModelRoutingTargetCheckpointResolution -PromptText $prompt
    if ($resolution.Reason) {
        return [ordered]@{
            hookSpecificOutput = [ordered]@{
                hookEventName            = 'PreToolUse'
                permissionDecision       = 'deny'
                permissionDecisionReason = $resolution.Reason
            }
        }
    }

    $checkpoint = Get-ModelRoutingCheckpoint -CheckpointPath $resolution.CheckpointPath
    if (Test-ModelRoutingReceiptPresent -Checkpoint $checkpoint -Subagent $subagent) {
        return $allow
    }

    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = "MODEL_ROUTING_RECEIPT_BLOCKED: cannot delegate to '$subagent' before a model_routing_receipts entry for it is recorded in the orchestrator checkpoint. Perform Model Selection (record the complexity assessment and routing receipt) before delegating."
        }
    }
}

# Guard allows dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}

$decision = Invoke-ModelRoutingReceiptDecision -ToolInputRaw (Read-ClaudeHookRawPayload)

$decision | ConvertTo-Json -Compress -Depth 5 | Write-Output

exit 0
