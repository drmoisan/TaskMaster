<#
.SYNOPSIS
    Pre-tool-use hook that gates the destructive parallel abandon disposition behind an
    explicit confirmation marker.

.DESCRIPTION
    Invoked by the Claude Code PreToolUse hook on the "Bash" matcher before any Bash
    command runs. Reads the hook payload through the shared reader, extracts the
    command text from the envelope's nested tool_input, and matches it
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


Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force
# The two tokens this gate matches on. These are the ONLY places either token literal
# appears in this file; the seam test extracts the consumer-side values from exactly
# these two named assignments.
$script:AbandonDispositionToken = '--disposition abandon'
$script:AbandonConfirmToken = '--confirm-abandon'

# Literal prefix on the deny reason, so the reason code is greppable in transcripts.
$script:AbandonBlockedReasonCode = 'PARALLEL_ABANDON_BLOCKED'

# Shared command-line parser (issue #545). Dot-sourced BELOW the two token assignments
# rather than above them, deliberately: the assignments keep their existing line numbers,
# so the diff shows no change at either line. Both are still declared before any function
# body runs, because the file executes top to bottom before any of its functions is called.
. (Join-Path $PSScriptRoot 'hook-command-scanner.ps1')
. (Join-Path $PSScriptRoot 'hook-command-invocation.ps1')

function Get-ParallelAbandonGateToolInput {
    <#
    .SYNOPSIS
        Read the raw JSON hook payload through the shared reader (stdin first, then
        the two environment fallbacks). Tests mock this function (read seam).
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param()

    return Read-ClaudeHookRawPayload
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

function Test-ParallelAbandonSegmentDisposition {
    <#
    .SYNOPSIS
        Report whether one segment's tokens request the abandon disposition.
    .DESCRIPTION
        The option name and its value are derived from $script:AbandonDispositionToken by
        splitting it on whitespace, so this function restates neither literal and the seam
        test's single-source-of-truth property is preserved. Both spellings the producer's
        argparse registration accepts are recognized: the separated pair
        '--disposition' 'abandon' as two adjacent tokens, and the equals-joined single token
        '--disposition=abandon'.

        Comparison is by whole-token equality, which is case-insensitive in PowerShell and
        therefore preserves the OrdinalIgnoreCase behaviour of the containment test it
        replaces. Because a quoted span is one token, a grep whose quoted search term is the
        disposition token no longer requests an abandon.

        A raw-scan leg runs after the token loop, on the segments the scanner itself scans
        raw, so a disposition carried inside a wrapper's quoted argument is still recognized.
    .PARAMETER Segment
        One segment record produced by Read-CommandLineSegment.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowNull()] $Segment)

    $parts = @($script:AbandonDispositionToken -split '\s+' | Where-Object { $_ })
    if ($parts.Count -lt 2) {
        return $false
    }
    $optionName = $parts[0]
    $optionValue = $parts[1]
    $joined = $optionName + '=' + $optionValue

    $token = @($Segment.Tokens)

    for ($index = 0; $index -lt $token.Count; $index++) {
        if ($token[$index] -eq $joined) {
            return $true
        }
        if ($token[$index] -eq $optionName -and
            $index + 1 -lt $token.Count -and
            $token[$index + 1] -eq $optionValue) {
            return $true
        }
    }

    if (-not (Test-CommandLineSegmentRawScan -Segment $Segment)) {
        return $false
    }

    # A wrapper's quoted argument is a nested command line and collapses into ONE token, so
    # neither the adjacent pair nor the equals-joined token can form inside it. Both accepted
    # spellings are reconstructed from the split constant rather than restated, so the
    # single-source-of-truth seam test still finds each literal exactly once in this file.
    $scanText = [string]$Segment.ScanText
    $comparison = [System.StringComparison]::OrdinalIgnoreCase
    return ($scanText.IndexOf($joined, $comparison) -ge 0 -or
        $scanText.IndexOf(($optionName + ' ' + $optionValue), $comparison) -ge 0)
}

function Test-ParallelAbandonCommandInScope {
    <#
    .SYNOPSIS
        Report whether a command carries the abandon disposition token at all.
    .DESCRIPTION
        Evaluated per segment through the shared parser rather than as a containment test
        over the whole command string. In scope when ANY segment requests the abandon
        disposition in either accepted spelling.
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

    foreach ($segment in @(Read-CommandLineSegment -CommandText $NormalizedCommand)) {
        if (Test-ParallelAbandonSegmentDisposition -Segment $segment) {
            return $true
        }
    }

    return $false
}

function Test-ParallelAbandonCommandConfirmed {
    <#
    .SYNOPSIS
        Report whether a command carries the confirmation marker.
    .DESCRIPTION
        Confirmed only when a SINGLE segment carries both the abandon disposition and the
        confirmation marker. The confirmation is the caller's deliberate acknowledgement of
        this abandon, so a marker echoed in an unrelated segment is not an acknowledgement
        of the command that follows it.
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

    foreach ($segment in @(Read-CommandLineSegment -CommandText $NormalizedCommand)) {
        $tokens = @($segment.Tokens)
        if (-not (Test-ParallelAbandonSegmentDisposition -Segment $segment)) {
            continue
        }
        if ($tokens -contains $script:AbandonConfirmToken) {
            return $true
        }
        if ((Test-CommandLineSegmentRawScan -Segment $segment) -and
            ([string]$segment.ScanText).IndexOf($script:AbandonConfirmToken, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            return $true
        }
    }

    return $false
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
        Parses the PreToolUse envelope and returns an allow-or-block decision.
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
    $payload = Resolve-ClaudeHookToolInput -Raw $ToolInputRaw
    if (-not $payload.IsValid) {
        return Get-ParallelAbandonGateBlockDecision -Reason (
            $script:AbandonBlockedReasonCode + ': payload anomaly - ' +
            (Get-ClaudeHookPayloadAnomalyReason -Anomaly $payload.Anomaly) +
            '. The gate fails closed on an envelope it cannot read.')
    }

    $commandText = Get-ClaudeHookToolInputString -ToolInput $payload.Value -Name 'command'
    $normalizedCommand = Get-ParallelAbandonNormalizedCommand -CommandText $commandText
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

$decision = Invoke-ParallelAbandonGateDecision -ToolInputRaw (Get-ParallelAbandonGateToolInput)

$decision | ConvertTo-Json -Compress -Depth 5 | Write-Output

exit 0
