
<#
.SYNOPSIS
    Pre-tool-use hook that blocks Bash promotion-script bypass attempts.

.DESCRIPTION
    This script is invoked by the Codex PreToolUse hook before a shell command runs.
    It reads one Codex JSON object from stdin, inspects tool_input.command, and blocks direct promotion
    script execution that would bypass the repository's MCP-only promotion path.

    Forbidden command tokens (legacy promotion-script bypass):
      - new-potential-entry.ps1
      - new_potential_bug_entry
      - potential_to_issue
      - new_active_feature_folder

    Forbidden gh-CLI patterns (raw GitHub issue creation bypass):
      - gh issue create (with any flag suffix)
      - gh issue new
      - gh api against repos/<owner>/<repo>/issues with explicit POST method
        (-X POST or --method POST)

    The hook is read-only: it inspects the attempted command and emits a JSON
    allow-or-block decision without mutating the command text.

.NOTES
    Compatible with PowerShell 7+.
#>
[CmdletBinding()]
param()

# Shared command-line parser (issue #545). Every forbidden-token scan and both `gh`
# expressions below run against each segment's clause-ordered scan text, and the
# structural matcher reaches the relocating `gh` spellings adjacency cannot.
. (Join-Path $PSScriptRoot 'hook-command-scanner.ps1')
. (Join-Path $PSScriptRoot 'hook-command-invocation.ps1')

$script:PromotionMcpOnlyBlockedReason = 'PROMOTION_MCP_ONLY_BLOCKED: Direct Bash promotion-script execution is not allowed in agent sessions. Use the drm-copilot MCP promotion tools instead.'

$script:PromotionMcpOnlyGhIssueBlockedReason = 'PROMOTION_MCP_ONLY_BLOCKED: Direct GitHub issue creation via `gh` bypasses the approved drm-copilot MCP promotion path (`mcp__drm-copilot__new_potential_entry` -> `mcp__drm-copilot__potential_to_issue` -> `mcp__drm-copilot__new_active_feature_folder`). Use those MCP tools instead.'

function Get-PromotionMcpOnlyBlockedReason {
    <#
    .SYNOPSIS
        Return the canonical deny message for legacy promotion-script bypass attempts.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param()

    return $script:PromotionMcpOnlyBlockedReason
}

function Get-PromotionMcpOnlyGhIssueBlockedReason {
    <#
    .SYNOPSIS
        Return the deny message for raw gh-CLI issue creation bypass attempts.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param()

    return $script:PromotionMcpOnlyGhIssueBlockedReason
}

function Get-PromotionBypassReason {
    <#
    .SYNOPSIS
        Inspect the command text and return the specific deny reason, or $null when allowed.
    .DESCRIPTION
        Returns the legacy promotion-script reason when any forbidden token is present.
        Returns the gh-CLI issue creation reason when a forbidden gh pattern is matched.
        Returns $null when the command is allowed.
    .PARAMETER CommandText
        The shell command text extracted from the Codex tool_input object.
    .OUTPUTS
        System.String or $null.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [string] $CommandText
    )

    $forbiddenTokens = @(
        'new-potential-entry.ps1',
        'new_potential_bug_entry',
        'potential_to_issue',
        'new_active_feature_folder'
    )

    # Issue #545: the scan text of a segment is its raw text only when the segment is
    # wrapper-led, carries a live substitution, or could not be balanced; otherwise
    # quoted spans and attached heredoc bodies are masked. A promotion tool name that
    # appears only as a JSON receipt value is therefore a mention, not an invocation.
    $segments = @(Read-CommandLineSegment -CommandText $CommandText)

    foreach ($segment in $segments) {
        foreach ($token in $forbiddenTokens) {
            if ($segment.ScanText.IndexOf($token, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
                return (Get-PromotionMcpOnlyBlockedReason)
            }
        }
    }

    # `gh issue create` and `gh issue new` are direct bypasses of the MCP
    # promotion path. Tolerate any flags after the subcommand.
    foreach ($segment in $segments) {
        if ($segment.ScanText -match '(?i)\bgh\s+issue\s+(?:create|new)\b') {
            return (Get-PromotionMcpOnlyGhIssueBlockedReason)
        }
    }

    # The adjacency-requiring expression above cannot see a relocating spelling, in
    # which a `gh` global option such as --repo or -R sits between the command word
    # and its subcommand (D10). The structural matcher absorbs those options and
    # reads the subcommand path positionally, so both spellings classify.
    if ((Test-CommandLineInvocation -CommandText $CommandText -CommandWord 'gh' -SubcommandPath @('issue', 'create')) -or
        (Test-CommandLineInvocation -CommandText $CommandText -CommandWord 'gh' -SubcommandPath @('issue', 'new'))) {
        return (Get-PromotionMcpOnlyGhIssueBlockedReason)
    }

    # `gh api repos/<owner>/<repo>/issues` is a write surface only when an
    # explicit POST method is supplied. `gh api` defaults to GET, so we only
    # block when -X POST or --method POST is present, to avoid false positives
    # on issue read operations. Use a single regex with lookaheads against the
    # scan text of one segment.
    $ghApiIssuesPostPattern = '(?i)(?=.*\bgh\s+api\b)(?=.*repos/[^/\s]+/[^/\s]+/issues(?:\b|/[^/\s]*$))(?=.*(?:-X\s+POST|--method\s+POST))'
    foreach ($segment in $segments) {
        if ($segment.ScanText -match $ghApiIssuesPostPattern) {
            return (Get-PromotionMcpOnlyGhIssueBlockedReason)
        }
    }

    return $null
}

function Test-PromotionBypassToken {
    <#
    .SYNOPSIS
        Return $true when a Bash command contains a forbidden promotion bypass pattern.
    .PARAMETER CommandText
        The shell command text extracted from the Codex tool_input object.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)]
        [string] $CommandText
    )

    return ($null -ne (Get-PromotionBypassReason -CommandText $CommandText))
}

function Get-PromotionMcpOnlyBlockDecision {
    <#
    .SYNOPSIS
        Construct the structured block decision for a forbidden Bash command.
    .PARAMETER Reason
        The specific deny reason to surface in the block decision. Defaults to the
        legacy promotion-script reason for backward compatibility.
    .OUTPUTS
        System.Collections.Specialized.OrderedDictionary
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [string] $Reason
    )

    if (-not $Reason) {
        $Reason = Get-PromotionMcpOnlyBlockedReason
    }

    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = $Reason
        }
    }
}

function Get-PromotionMcpOnlyAllowDecision {
    <#
    .SYNOPSIS
        Construct the structured allow decision for a permitted Bash command.
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

function Invoke-PromotionMcpOnlyDecision {
    <#
    .SYNOPSIS
        Parse the mapped Codex tool_input JSON and return an allow-or-deny decision.
    .PARAMETER ToolInputRaw
        The mapped tool_input JSON supplied by Codex.
    .OUTPUTS
        System.Collections.Specialized.OrderedDictionary
    .NOTES
        Missing tool input or missing command text is treated as allow because
        non-Bash invocations or empty Bash requests cannot bypass promotion flow.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [string] $ToolInputRaw
    )

    if (-not $ToolInputRaw) {
        return Get-PromotionMcpOnlyAllowDecision
    }

    try {
        $toolInput = $ToolInputRaw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        throw "enforce-promotion-mcp-only received malformed mapped tool_input JSON: $_"
    }

    $commandText = $toolInput.command
    if (-not $commandText) {
        return Get-PromotionMcpOnlyAllowDecision
    }

    $reason = Get-PromotionBypassReason -CommandText $commandText
    if ($reason) {
        return Get-PromotionMcpOnlyBlockDecision -Reason $reason
    }

    return Get-PromotionMcpOnlyAllowDecision
}

function ConvertFrom-CodexPromotionHookPayload {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $PayloadRaw)

    if ([string]::IsNullOrWhiteSpace($PayloadRaw)) {
        throw 'enforce-promotion-mcp-only hook input is empty.'
    }
    try {
        $payload = $PayloadRaw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        throw "enforce-promotion-mcp-only hook input is malformed JSON: $_"
    }
    if ($payload.PSObject.Properties.Name -notcontains 'tool_input' -or $null -eq $payload.tool_input) {
        throw 'enforce-promotion-mcp-only hook input is missing tool_input.'
    }
    if ([string]$payload.hook_event_name -ne 'PreToolUse' -or [string]$payload.tool_name -ne 'Bash') {
        throw 'enforce-promotion-mcp-only requires a PreToolUse Bash payload.'
    }
    return $payload
}

# Allow dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}

try {
    $payload = ConvertFrom-CodexPromotionHookPayload -PayloadRaw ([Console]::In.ReadToEnd())
    $toolInputRaw = $payload.tool_input | ConvertTo-Json -Compress -Depth 20
    $decision = Invoke-PromotionMcpOnlyDecision -ToolInputRaw $toolInputRaw
    if ($decision.hookSpecificOutput.permissionDecision -eq 'deny') {
        $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output
    }
    exit 0
} catch {
    [Console]::Error.WriteLine([string]$_)
    exit 2
}
