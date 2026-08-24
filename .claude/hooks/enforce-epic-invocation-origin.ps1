<#
.SYNOPSIS
    Pre-tool-use hook that blocks epic-planner, epic-orchestrator,
    parallel-planner, and parallel-orchestrator delegations originating from an
    orchestrator agent.

.DESCRIPTION
    Invoked by the Claude Code PreToolUse hook on the "Agent" matcher before any
    Agent (Task) call runs. Activates only when the delegation target
    subagent_type is 'epic-planner', 'epic-orchestrator', 'parallel-planner',
    or 'parallel-orchestrator'.

    The payload is acquired through the shared reader
    (.claude/lib/hook-payload/HookPayload.psm1: stdin first, then the two
    environment-variable fallbacks).

    Caller identity resolution:
      - The PreToolUse envelope carries a top-level 'agent_type' field only when
        the tool call is made from inside a subagent context. A main-thread call
        carries no 'agent_type'.
      - The envelope's nested 'tool_input' object carries the delegation target
        'subagent_type'.

    Decision procedure:
      0. An envelope this hook cannot read at all -- empty on every transport,
         unparseable, or carrying no usable tool_input -- denies (fail closed).
      1. Resolve the target subagent_type from the nested tool_input. A non-gated
         target allows.
      2. Resolve the calling agent_type from the payload. An absent or empty
         agent_type indicates a main-thread invocation, which allows.
      3. Deny when the calling agent_type is exactly 'orchestrator', with the
         reason variant selected by target: EPIC_INVOCATION_ORIGIN_BLOCKED for
         an epic target, PARALLEL_INVOCATION_ORIGIN_BLOCKED for a parallel
         target. All four gated agents delegate to Agent(orchestrator); an
         orchestrator-originated invocation would nest orchestrator inside its
         own delegation chain.

.NOTES
    Compatible with PowerShell 7+. No external module dependencies. Read-only
    validation gate; malformed JSON in either payload throws so the entrypoint
    exits 1.
#>
[CmdletBinding()]
param()


Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force
$script:GatedSubagentTypes = @('epic-planner', 'epic-orchestrator', 'parallel-planner', 'parallel-orchestrator')
$script:ParallelSubagentTypes = @('parallel-planner', 'parallel-orchestrator')
$script:ProhibitedCallerAgentType = 'orchestrator'
$script:EmptyPayloadAnomaly = 'EmptyPayload'

function Get-EpicInvocationOriginAllowDecision {
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

function Get-EpicInvocationOriginBlockDecision {
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

function ConvertFrom-EpicInvocationOriginPayload {
    <#
    .SYNOPSIS
        Parses a raw JSON payload string, returning $null for a blank payload
        and throwing a named error for malformed JSON.
    .PARAMETER RawPayload
        The raw JSON text under evaluation.
    .PARAMETER PayloadName
        The payload's environment-variable name, used in the error message.
    .OUTPUTS
        System.Object or $null
    #>
    [CmdletBinding()]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $RawPayload,

        [Parameter(Mandatory)]
        [string] $PayloadName
    )

    if ([string]::IsNullOrWhiteSpace($RawPayload)) {
        return $null
    }

    try {
        return $RawPayload | ConvertFrom-Json -ErrorAction Stop
    } catch {
        throw "enforce-epic-invocation-origin hook received malformed JSON in ${PayloadName}: $_"
    }
}

function Get-EpicInvocationOriginTargetSubagent {
    <#
    .SYNOPSIS
        Resolves the delegation target subagent_type from the tool input,
        falling back to the full payload's tool_input object.
    .PARAMETER ToolInput
        Parsed legacy tool-input object, or $null when absent.
    .PARAMETER HookInput
        Parsed PreToolUse envelope object, or $null when absent.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowNull()]
        $ToolInput,

        [AllowNull()]
        $HookInput
    )

    if ($null -ne $ToolInput -and
        (@($ToolInput.PSObject.Properties.Name) -contains 'subagent_type') -and
        -not [string]::IsNullOrWhiteSpace([string]$ToolInput.subagent_type)) {
        return [string]$ToolInput.subagent_type
    }

    if ($null -ne $HookInput -and
        (@($HookInput.PSObject.Properties.Name) -contains 'tool_input')) {
        $nested = $HookInput.tool_input
        if ($null -ne $nested -and
            (@($nested.PSObject.Properties.Name) -contains 'subagent_type') -and
            -not [string]::IsNullOrWhiteSpace([string]$nested.subagent_type)) {
            return [string]$nested.subagent_type
        }
    }

    return $null
}

function Get-EpicInvocationOriginCallerAgentType {
    <#
    .SYNOPSIS
        Resolves the calling agent_type from the full hook payload. Returns
        $null for a main-thread invocation (no agent_type field).
    .PARAMETER HookInput
        Parsed PreToolUse envelope object, or $null when absent.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowNull()]
        $HookInput
    )

    if ($null -eq $HookInput) {
        return $null
    }
    if (@($HookInput.PSObject.Properties.Name) -notcontains 'agent_type') {
        return $null
    }

    $agentType = [string]$HookInput.agent_type
    if ([string]::IsNullOrWhiteSpace($agentType)) {
        return $null
    }
    return $agentType
}

function Invoke-EpicInvocationOriginDecision {
    <#
    .SYNOPSIS
        Parses the hook payloads and returns an allow-or-block decision.
    .DESCRIPTION
        The caller's agent_type is read off the envelope root and the delegation
        target's subagent_type off the nested tool_input. An envelope this hook cannot
        read at all -- empty on every transport, unparseable, or carrying no usable
        tool_input -- is a fail-closed deny, because an unreadable envelope means the
        PreToolUse contract drifted (issue #501).
    .PARAMETER HookInputRaw
        The raw PreToolUse envelope JSON acquired by Read-ClaudeHookRawPayload.
    .PARAMETER ToolInputRaw
        Optional legacy direct tool-input payload, retained so an undocumented wrapper
        that supplies the bare tool input keeps working.
    .OUTPUTS
        System.Collections.Specialized.OrderedDictionary
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $HookInputRaw,

        [AllowNull()]
        [AllowEmptyString()]
        [string] $ToolInputRaw
    )

    # The envelope carries both halves: the caller's agent_type at the root and the
    # delegation target's subagent_type inside tool_input.
    $anomaly = $script:EmptyPayloadAnomaly
    $hookInput = $null
    if (-not [string]::IsNullOrWhiteSpace($HookInputRaw)) {
        $resolved = Resolve-ClaudeHookToolInput -Raw $HookInputRaw
        $hookInput = $resolved.Envelope
        $anomaly = if ($resolved.IsValid) { $null } else { $resolved.Anomaly }
    }

    # Legacy direct tool-input payload, retained for an undocumented wrapper that
    # supplies the bare tool input rather than the documented envelope.
    $toolInput = $null
    if (-not [string]::IsNullOrWhiteSpace($ToolInputRaw)) {
        $legacy = ConvertFrom-ClaudeHookEnvelope -Raw $ToolInputRaw
        if ($legacy.IsValid) {
            $toolInput = $legacy.Value
            $anomaly = $null
        }
    }

    $target = Get-EpicInvocationOriginTargetSubagent -ToolInput $toolInput -HookInput $hookInput

    if (-not $target -and $anomaly) {
        return Get-EpicInvocationOriginBlockDecision -Reason (
            'EPIC_INVOCATION_ORIGIN_BLOCKED: payload anomaly - ' +
            (Get-ClaudeHookPayloadAnomalyReason -Anomaly $anomaly) +
            '. The gate fails closed on an envelope it cannot read.')
    }

    if (-not $target -or $script:GatedSubagentTypes -notcontains $target) {
        return Get-EpicInvocationOriginAllowDecision
    }

    # An absent agent_type marks a main-thread invocation, which is the
    # intended entry point for every gated agent; only an orchestrator-context
    # invocation is prohibited.
    $caller = Get-EpicInvocationOriginCallerAgentType -HookInput $hookInput
    if ($caller -ne $script:ProhibitedCallerAgentType) {
        return Get-EpicInvocationOriginAllowDecision
    }

    # The parallel family carries its own reason variant because the epic reason
    # names the two epic agents literally; the deny prose is selected by target.
    if ($script:ParallelSubagentTypes -contains $target) {
        $parallelReason = "PARALLEL_INVOCATION_ORIGIN_BLOCKED: Agent($target) must not be invoked from an orchestrator agent. Both parallel-planner and parallel-orchestrator delegate to Agent(orchestrator), so an orchestrator-originated invocation would nest orchestrator inside its own delegation chain. Invoke $target from the main session instead."
        return Get-EpicInvocationOriginBlockDecision -Reason $parallelReason
    }

    $reason = "EPIC_INVOCATION_ORIGIN_BLOCKED: Agent($target) must not be invoked from an orchestrator agent. Both epic-planner and epic-orchestrator delegate to Agent(orchestrator), so an orchestrator-originated invocation would nest orchestrator inside its own delegation chain. Invoke $target from the main session instead."
    return Get-EpicInvocationOriginBlockDecision -Reason $reason
}

# Guard allows dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}

$decision = Invoke-EpicInvocationOriginDecision -HookInputRaw (Read-ClaudeHookRawPayload)

$decision | ConvertTo-Json -Compress -Depth 5 | Write-Output

exit 0
