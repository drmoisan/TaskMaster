<#
.SYNOPSIS
    PreToolUse hook that limits the pr-author subagent's Bash calls to an allowlist of forms.

.DESCRIPTION
    Registered in the frontmatter of .claude/agents/pr-author.md on the "Bash" matcher
    (issue #824, #733). Every command is read with Read-CommandLineInvocationSegment, so a
    wrapper payload, a substitution body, or an xargs-injected command is a record of its own.
    The command is allowed only when every record is a literal, top-level, unwrapped command
    in one of these forms, matched by token position:

      - git log ...            and git rev-parse ...        (no git global option)
      - gh pr create ...       and gh pr edit ...           (no gh global option)
      - sha256sum artifacts/pr_body_<N>.md                  (only as the whole command)
      - date -u [+<format>]                                 (only as the whole command)

    Segments may be joined only by ';', '&&', '||', '|', or a newline. A record that is
    nested, unbalanced, opaque, non-literal, wrapper-led, led by a VAR=value assignment, or
    whose scanner segment carries a redirection is denied. Every deny reason begins
    PR_AUTHOR_COMMAND_NOT_ALLOWED: and names the first rejected segment.

    The hook is read-only: it inspects the attempted command and emits a JSON
    allow-or-deny decision without mutating the command text.

.NOTES
    Compatible with PowerShell 7+. Pure string logic over the shared command-line parser;
    no disk, process, or network access beyond payload acquisition.
#>
[CmdletBinding()]
param()


Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force
# Shared command-line parser (issues #545 and #824).
. (Join-Path $PSScriptRoot 'hook-command-scanner.ps1')
. (Join-Path $PSScriptRoot 'hook-command-invocation.ps1')

$script:PrAuthorAllowlistReasonPrefix = 'PR_AUTHOR_COMMAND_NOT_ALLOWED: '
$script:PrAuthorAllowlistDelimiters = @('', ';', '&&', '||', '|', "`n")
$script:PrAuthorAllowlistWrapperWords = @(
    'sh', 'bash', 'zsh', 'dash', 'ksh', 'pwsh', 'powershell', 'eval', 'xargs',
    'env', 'timeout', 'time', 'command', 'nohup'
)
$script:PrAuthorAllowlistFormText = 'git log, git rev-parse, gh pr create, gh pr edit, sha256sum artifacts/pr_body_<N>.md, date -u [+<format>]'

function Test-PrAuthorAllowlistForm {
    <#
    .SYNOPSIS
        Report whether one record's tokens match an allowed command form by token position.
    .PARAMETER Token
        The record's tokens, quotes already stripped.
    .PARAMETER IsWholeCommand
        Whether the command has exactly one top-level record; sha256sum and date require it.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][string[]] $Token,
        [Parameter(Mandatory)][bool] $IsWholeCommand
    )

    if ($Token.Count -eq 0) {
        return $false
    }
    switch -CaseSensitive ($Token[0]) {
        'git' {
            return ($Token.Count -ge 2 -and @('log', 'rev-parse') -ccontains $Token[1])
        }
        'gh' {
            return ($Token.Count -ge 3 -and $Token[1] -ceq 'pr' -and @('create', 'edit') -ccontains $Token[2])
        }
        'sha256sum' {
            return ($IsWholeCommand -and $Token.Count -eq 2 -and $Token[1] -cmatch '^artifacts/pr_body_\d+\.md$')
        }
        'date' {
            if (-not $IsWholeCommand -or $Token.Count -lt 2 -or $Token[1] -cne '-u') {
                return $false
            }
            return ($Token.Count -eq 2 -or ($Token.Count -eq 3 -and $Token[2].StartsWith('+')))
        }
    }
    return $false
}

function Get-PrAuthorAllowlistRecordProblem {
    <#
    .SYNOPSIS
        Return the reason one record is rejected, or $null when the record is allowed.
    .PARAMETER Record
        One Read-CommandLineInvocationSegment record.
    .PARAMETER MaskedText
        The masked text of the record's depth-0 scanner segment, or '' for a nested record.
    .PARAMETER IsWholeCommand
        Whether the command has exactly one top-level record.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)][pscustomobject] $Record,
        [Parameter(Mandatory)][AllowEmptyString()][string] $MaskedText,
        [Parameter(Mandatory)][bool] $IsWholeCommand
    )

    $tokens = [string[]]@($Record.Tokens)
    if ($Record.Depth -gt 0) { return "it is a nested $($Record.Origin) command" }
    if ($Record.Unbalanced) { return 'its quoting or grouping is unbalanced' }
    if ($Record.Opaque) { return "its payload cannot be read ($($Record.OpaqueReason))" }
    if (-not $Record.Literal) { return 'it carries a variable, substitution, or other non-literal token' }
    if ($script:PrAuthorAllowlistWrapperWords -contains $Record.CommandWord) { return "it is led by the wrapper '$($Record.CommandWord)'" }
    if ($script:PrAuthorAllowlistDelimiters -notcontains $Record.Delimiter) { return "it is joined by the delimiter '$($Record.Delimiter)'" }
    if ($MaskedText.Contains('<') -or $MaskedText.Contains('>')) { return 'it carries a redirection' }
    if ($tokens.Count -gt 0 -and $tokens[0] -match '^[A-Za-z_][A-Za-z0-9_]*=') { return 'it is led by a VAR=value assignment' }
    if (-not (Test-PrAuthorAllowlistForm -Token $tokens -IsWholeCommand $IsWholeCommand)) { return 'it is not an allowed command form' }
    return $null
}

function Get-PrAuthorCommandAllowlistReason {
    <#
    .SYNOPSIS
        Return the deny reason for a command outside the pr-author allowlist, or $null.
    .PARAMETER CommandText
        The Bash command text extracted from the envelope's tool_input.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $CommandText
    )

    if ([string]::IsNullOrWhiteSpace($CommandText)) {
        return $null
    }

    $records = @(Read-CommandLineInvocationSegment -CommandText $CommandText)
    $segments = @(Read-CommandLineSegment -CommandText (ConvertTo-CommandLineNormalizedText -CommandText $CommandText))
    $topLevel = @($records | Where-Object { $_.Depth -eq 0 })
    $isWholeCommand = $topLevel.Count -eq 1
    $topLevelPosition = 0

    foreach ($record in $records) {
        $maskedText = ''
        if ($record.Depth -eq 0) {
            if ($topLevelPosition -lt $segments.Count) {
                $maskedText = [string]$segments[$topLevelPosition].MaskedText
            }
            $topLevelPosition++
        }
        $problem = Get-PrAuthorAllowlistRecordProblem -Record $record -MaskedText $maskedText -IsWholeCommand $isWholeCommand
        if ($null -ne $problem) {
            return ($script:PrAuthorAllowlistReasonPrefix +
                "the segment '$($record.RawText.Trim())' is rejected because $problem. The pr-author agent may run only: $($script:PrAuthorAllowlistFormText), each without wrappers, substitutions, or redirections.")
        }
    }

    return $null
}

function Get-PrAuthorCommandAllowlistDecision {
    <#
    .SYNOPSIS
        Build an ordered allow or deny decision.
    .PARAMETER Reason
        The deny reason, or $null for an allow.
    .OUTPUTS
        System.Collections.Specialized.OrderedDictionary
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [AllowNull()]
        [string] $Reason
    )

    if ([string]::IsNullOrEmpty($Reason)) {
        return [ordered]@{
            hookSpecificOutput = [ordered]@{
                hookEventName      = 'PreToolUse'
                permissionDecision = 'allow'
            }
        }
    }
    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = $Reason
        }
    }
}

function Invoke-PrAuthorCommandAllowlistDecision {
    <#
    .SYNOPSIS
        Return the ordered allow-or-deny decision for one command text.
    .PARAMETER CommandText
        The Bash command text.
    .OUTPUTS
        System.Collections.Specialized.OrderedDictionary
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $CommandText
    )

    return Get-PrAuthorCommandAllowlistDecision -Reason (Get-PrAuthorCommandAllowlistReason -CommandText $CommandText)
}

function Invoke-PrAuthorCommandAllowlistEntryPoint {
    <#
    .SYNOPSIS
        Runs the hook decision and returns the process exit code.
    .DESCRIPTION
        Acquires the payload through the shared reader unless the caller supplies one, emits
        the compact decision JSON, and returns 0. It never returns 1: exit 1 is non-blocking
        for PreToolUse, so every anomaly is already a deny decision by the time control
        reaches here. The function does not call exit; the thin tail converts the returned
        code into a process exit.
    .PARAMETER ToolInputRaw
        Optional pre-acquired payload text. When omitted the ReadPayload seam runs.
    .PARAMETER ReadPayload
        Seam for payload acquisition, so tests can drive the empty-on-all-transports case
        without touching a console.
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

    $payload = Resolve-ClaudeHookToolInput -Raw $ToolInputRaw
    if (-not $payload.IsValid) {
        $decision = Get-PrAuthorCommandAllowlistDecision -Reason (
            $script:PrAuthorAllowlistReasonPrefix + 'payload anomaly - ' +
            (Get-ClaudeHookPayloadAnomalyReason -Anomaly $payload.Anomaly) +
            '. The hook fails closed on an envelope it cannot read.')
    } else {
        $commandText = [string](Get-ClaudeHookToolInputString -ToolInput $payload.Value -Name 'command')
        $decision = Invoke-PrAuthorCommandAllowlistDecision -CommandText $commandText
    }
    $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output

    return 0
}

# Allow dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}

# The entry point returns its [int] exit code as the last pipeline element and the
# decision JSON before it. `exit (<call>)` would capture BOTH into the exit
# expression and emit nothing, so the decision is written explicitly here first.
$entryPointResult = @(Invoke-PrAuthorCommandAllowlistEntryPoint)
if ($entryPointResult.Count -gt 1) {
    $entryPointResult[0..($entryPointResult.Count - 2)] | Write-Output
}

exit ([int]$entryPointResult[-1])
