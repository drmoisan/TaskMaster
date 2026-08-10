<#
.SYNOPSIS
    Pre-tool-use hook that gates the destructive parallel abandon disposition behind an
    explicit confirmation marker.

.DESCRIPTION
    Invoked by the Claude Code PreToolUse hook on the "Bash" matcher before any Bash
    command runs. Reads CLAUDE_TOOL_INPUT, extracts the command text, and matches it
    against the two tokens declared at the top of this file: the abandon disposition
    token and the confirmation marker. A command carrying the abandon disposition token
    without the confirmation marker in the SAME command is denied with reason prefix
    PARALLEL_ABANDON_BLOCKED. A command carrying both is allowed. A command carrying no
    abandon disposition token is out of scope and is allowed unchanged.

    The gate exists because the abandon disposition of /parallel-remove closes a pull
    request and removes a worktree. Both side effects are irreversible from the run's
    point of view, so the destructive path must be confirmed deliberately rather than
    reached by a command that merely looked like a removal.

.NOTES
    Compatible with PowerShell 7+. No external module dependencies. Tool-input retrieval
    goes through an injectable wrapper function so tests can mock the boundary with
    literal JSON without writing temporary files, following the read-seam pattern of
    .claude/hooks/enforce-epic-worktree-removal-gate.ps1.

    The two token values are declared ONCE EACH below and every match in this file reads
    those variables rather than repeating a literal. The producer side of the same pair is
    scripts/dev_tools/parallel_mutation_abandon_cli.py, and
    tests/scripts/dev_tools/test_parallel_abandon_token_seam.py parses both sides at run
    time so a rename on one side without the other fails.
#>
[CmdletBinding()]
param()

# The two tokens this gate matches on. These are the ONLY places either token literal
# appears in this file; the seam test extracts the consumer-side values from exactly
# these two named assignments.
$script:AbandonDispositionToken = '--disposition abandon'
$script:AbandonConfirmToken = '--confirm-abandon'

# Literal prefix on the deny reason, so the reason code is greppable in transcripts.
$script:AbandonBlockedReasonCode = 'PARALLEL_ABANDON_BLOCKED'

function Get-ParallelAbandonGateToolInput {
    <#
    .SYNOPSIS
        Read the raw JSON tool payload supplied by Claude Code. Tests mock this
        function (read seam).
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param()

    return $env:CLAUDE_TOOL_INPUT
}

function Get-ParallelAbandonNormalizedCommand {
    <#
    .SYNOPSIS
        Collapse whitespace runs in a command so token matching is not defeated by
        extra spacing or a line continuation.
    .PARAMETER CommandText
        The Bash command text under evaluation.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $CommandText
    )

    if ([string]::IsNullOrWhiteSpace($CommandText)) {
        return ''
    }
    # A single space between tokens is the shape the declared tokens are written in, so
    # normalizing here lets the tokens stay exactly as the producer declares them.
    return ($CommandText -replace '\s+', ' ').Trim()
}

function Test-ParallelAbandonCommandInScope {
    <#
    .SYNOPSIS
        Report whether a command carries the abandon disposition token at all.
    .PARAMETER NormalizedCommand
        The whitespace-normalized command text.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $NormalizedCommand
    )

    if ([string]::IsNullOrWhiteSpace($NormalizedCommand)) {
        return $false
    }
    return $NormalizedCommand.Contains(
        $script:AbandonDispositionToken,
        [System.StringComparison]::OrdinalIgnoreCase)
}

function Test-ParallelAbandonCommandConfirmed {
    <#
    .SYNOPSIS
        Report whether a command carries the confirmation marker.
    .PARAMETER NormalizedCommand
        The whitespace-normalized command text.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $NormalizedCommand
    )

    if ([string]::IsNullOrWhiteSpace($NormalizedCommand)) {
        return $false
    }
    return $NormalizedCommand.Contains(
        $script:AbandonConfirmToken,
        [System.StringComparison]::OrdinalIgnoreCase)
}

function Get-ParallelAbandonGateAllowDecision {
    <#
    .SYNOPSIS
        Build the allow decision payload.
    .OUTPUTS
        System.Collections.Specialized.OrderedDictionary
    #>
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

function Get-ParallelAbandonGateBlockDecision {
    <#
    .SYNOPSIS
        Build the deny decision payload carrying the supplied reason.
    .PARAMETER Reason
        The deny reason surfaced to the caller.
    .OUTPUTS
        System.Collections.Specialized.OrderedDictionary
    #>
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

function Get-ParallelAbandonGateBlockReason {
    <#
    .SYNOPSIS
        Build the deny reason text from the two declared tokens.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param()

    return @(
        "$($script:AbandonBlockedReasonCode): this command carries"
        "'$($script:AbandonDispositionToken)' without the explicit"
        "'$($script:AbandonConfirmToken)' confirmation marker in the same command."
        'The abandon disposition closes the item pull request and removes its worktree,'
        'so it must be confirmed deliberately. Add the confirmation marker to the same'
        'command, or use the detach disposition instead. Do not reformulate the command'
        'to evade this gate.'
    ) -join ' '
}

function Invoke-ParallelAbandonGateDecision {
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
        [AllowNull()]
        [AllowEmptyString()]
        [string] $ToolInputRaw
    )

    # An absent payload carries no command to gate, so there is nothing in scope. This is
    # allow rather than deny because the gate constrains one specific destructive command
    # shape and must not become a blanket Bash block.
    if (-not $ToolInputRaw) {
        return Get-ParallelAbandonGateAllowDecision
    }

    try {
        $toolInput = $ToolInputRaw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        throw "enforce-parallel-abandon-gate hook received malformed JSON in CLAUDE_TOOL_INPUT: $_"
    }

    $normalizedCommand = Get-ParallelAbandonNormalizedCommand -CommandText ([string]$toolInput.command)
    if (-not (Test-ParallelAbandonCommandInScope -NormalizedCommand $normalizedCommand)) {
        return Get-ParallelAbandonGateAllowDecision
    }

    # In scope: the command asks for the destructive disposition, so the confirmation
    # marker decides. Its presence is the caller's explicit acknowledgement.
    if (Test-ParallelAbandonCommandConfirmed -NormalizedCommand $normalizedCommand) {
        return Get-ParallelAbandonGateAllowDecision
    }

    return Get-ParallelAbandonGateBlockDecision -Reason (Get-ParallelAbandonGateBlockReason)
}

# Guard allows dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}

try {
    $decision = Invoke-ParallelAbandonGateDecision -ToolInputRaw (Get-ParallelAbandonGateToolInput)
} catch {
    Write-Error $_
    exit 1
}

$decision | ConvertTo-Json -Compress -Depth 5 | Write-Output

exit 0
