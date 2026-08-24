<#
.SYNOPSIS
    Pre-tool-use hook that blocks a delegation to a gated subagent when the
    orchestrator checkpoint records no model-routing receipt for that agent.

.DESCRIPTION
    Invoked by the Claude Code PreToolUse hook on the Agent (Task) tool. Reads
    tool input JSON from the the envelope's nested tool_input environment variable and the
    orchestrator checkpoint from artifacts/orchestration/orchestrator-state.json.

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
    inject a synthetic checkpoint without touching disk.

.NOTES
    Compatible with PowerShell 7+. Read-only presence-gating deterrent.
#>
[CmdletBinding()]
param()


Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force
function Get-ModelRoutingCheckpoint {
    <#
    .SYNOPSIS
        Returns the parsed orchestrator checkpoint object, or $null when the
        file is missing or not valid JSON. Tests mock this seam.
    #>
    [CmdletBinding()]
    [OutputType([object])]
    param(
        [string] $CheckpointPath = 'artifacts/orchestration/orchestrator-state.json'
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

    $checkpoint = Get-ModelRoutingCheckpoint
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
