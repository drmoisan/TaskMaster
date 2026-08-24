<#
.SYNOPSIS
    Pre-tool-use hook for Claude Code that blocks dangerous Bash commands.

.DESCRIPTION
    This script is invoked by the Claude Code PreToolUse hook before any Bash
    command is executed. It acquires the hook payload through the shared reader
    (.claude/lib/hook-payload/HookPayload.psm1: stdin first, then the two
    environment-variable fallbacks) and reads the proposed command string from the
    envelope's nested tool_input.command, or falls back to the first positional
    argument. If the command matches any blocked pattern (destructive operations
    such as forced deletions, forced pushes, or hard resets), the script writes a
    PreToolUse deny decision to stdout and exits with code 0. The deny decision
    uses the Claude Code PreToolUse schema:

        {"hookSpecificOutput":{"hookEventName":"PreToolUse",
         "permissionDecision":"deny","permissionDecisionReason":"<reason>"}}

    Safe commands emit no decision and exit with code 0 (an absent decision is a
    valid allow at PreToolUse). The legacy top-level decision/block form and the
    deny-path 'exit 1' are intentionally NOT used: PreToolUse fail-opens on both,
    so they would silently fail to block.

    Deliberate exception to the fail-closed envelope policy (issue #501, AC-5): this
    hook is a dangerous-command denylist, not a receipt gate, so an empty payload
    remains an allow and unparseable raw text is still treated as the command text
    for denylist matching. Both behaviours preserve the documented manual/CLI usage
    'pwsh -NoProfile -File validate-bash.ps1 "<command>"'. Every other PreToolUse
    hook denies on those two conditions.

.NOTES
    Compatible with PowerShell 7+.
    This script must not modify any state; it is a read-only validation gate.
#>
[CmdletBinding()]
param(
    [Parameter(Position = 0, Mandatory = $false)]
    [string]$CommandInput
)

Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force

function Get-BlockedBashPattern {
    [CmdletBinding()]
    [OutputType([string[]])]
    param()

    return [string[]]@(
        'rm -rf',
        'git push --force',
        'git push origin --force',
        'Remove-Item -Recurse -Force',
        'git reset --hard',
        'git push -f'
    )
}

function Get-BlockedPatternMatch {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $false)]
        [AllowEmptyString()]
        [AllowNull()]
        [string] $Command
    )

    if (-not $Command) {
        return $null
    }

    foreach ($pattern in (Get-BlockedBashPattern)) {
        if ($Command.Contains($pattern)) {
            return $pattern
        }
    }

    return $null
}

function Get-BashBlockReason {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $false)]
        [AllowEmptyString()]
        [AllowNull()]
        [string] $Command
    )

    $pattern = Get-BlockedPatternMatch -Command $Command
    if (-not $pattern) {
        return $null
    }

    return "Blocked dangerous command pattern detected: '$pattern'"
}

function Get-BashDenyDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [string] $Reason
    )

    [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = $Reason
        }
    }
}

function Get-BashCommandToCheck {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $false)]
        [AllowEmptyString()]
        [AllowNull()]
        [string] $ToolInputRaw,

        [Parameter(Mandatory = $false)]
        [AllowEmptyString()]
        [AllowNull()]
        [string] $PositionalInput
    )

    if ($ToolInputRaw) {
        $parsed = ConvertFrom-ClaudeHookEnvelope -Raw $ToolInputRaw
        if (-not $parsed.IsValid) {
            # AC-5 exception: unparseable raw text is still treated as the command
            # text, which is what the documented manual/CLI usage supplies.
            if ($parsed.Anomaly -eq 'UnparseableJson') {
                return [string]$ToolInputRaw
            }
        } else {
            $extracted = Get-ClaudeHookToolInput -Envelope $parsed.Value
            if ($extracted.IsValid) {
                $command = Get-ClaudeHookToolInputString -ToolInput $extracted.Value -Name 'command'
                if ($command) {
                    return $command
                }
            }
        }
    }

    if ($PositionalInput) {
        return $PositionalInput
    }

    return ''
}

function Invoke-ValidateBashDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory = $false)]
        [AllowEmptyString()]
        [AllowNull()]
        [string] $ToolInputRaw,

        [Parameter(Mandatory = $false)]
        [AllowEmptyString()]
        [AllowNull()]
        [string] $PositionalInput
    )

    $commandToCheck = Get-BashCommandToCheck -ToolInputRaw $ToolInputRaw -PositionalInput $PositionalInput

    $reason = Get-BashBlockReason -Command $commandToCheck
    if ($reason) {
        return Get-BashDenyDecision -Reason $reason
    }

    return $null
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

$toolInputRaw = Read-ClaudeHookRawPayload

$decision = Invoke-ValidateBashDecision -ToolInputRaw $toolInputRaw -PositionalInput $CommandInput
if ($null -ne $decision -and $decision.hookSpecificOutput.permissionDecision -eq 'deny') {
    $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output
}

exit 0
