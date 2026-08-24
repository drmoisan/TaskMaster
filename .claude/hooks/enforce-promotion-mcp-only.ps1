<#
.SYNOPSIS
    Pre-tool-use hook that blocks Bash promotion-script bypass attempts.

.DESCRIPTION
    This script is invoked by the Claude Code PreToolUse hook before any Bash
    command runs. It acquires the hook payload through the shared reader and reads
    the command from the envelope's nested tool_input, then blocks direct promotion
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


Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force
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
        The Bash command text extracted from the envelope's tool_input.
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

    foreach ($token in $forbiddenTokens) {
        if ($CommandText.IndexOf($token, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            return (Get-PromotionMcpOnlyBlockedReason)
        }
    }

    # `gh issue create` and `gh issue new` are direct bypasses of the MCP
    # promotion path. Tolerate any flags after the subcommand.
    if ($CommandText -match '(?i)\bgh\s+issue\s+(?:create|new)\b') {
        return (Get-PromotionMcpOnlyGhIssueBlockedReason)
    }

    # `gh api repos/<owner>/<repo>/issues` is a write surface only when an
    # explicit POST method is supplied. `gh api` defaults to GET, so we only
    # block when -X POST or --method POST is present, to avoid false positives
    # on issue read operations. Use a single regex with lookaheads against the
    # whole command string.
    $ghApiIssuesPostPattern = '(?i)(?=.*\bgh\s+api\b)(?=.*repos/[^/\s]+/[^/\s]+/issues(?:\b|/[^/\s]*$))(?=.*(?:-X\s+POST|--method\s+POST))'
    if ($CommandText -match $ghApiIssuesPostPattern) {
        return (Get-PromotionMcpOnlyGhIssueBlockedReason)
    }

    return $null
}

function Test-PromotionBypassToken {
    <#
    .SYNOPSIS
        Return $true when a Bash command contains a forbidden promotion bypass pattern.
    .PARAMETER CommandText
        The Bash command text extracted from the envelope's tool_input.
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
        Parse the PreToolUse envelope and return an allow-or-block decision.
    .PARAMETER ToolInputRaw
        The raw JSON tool payload supplied by Claude Code.
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

    $payload = Resolve-ClaudeHookToolInput -Raw $ToolInputRaw
    if (-not $payload.IsValid) {
        return Get-PromotionMcpOnlyBlockDecision -Reason (
            'PROMOTION_MCP_ONLY_BLOCKED: payload anomaly - ' +
            (Get-ClaudeHookPayloadAnomalyReason -Anomaly $payload.Anomaly) +
            '. The gate fails closed on an envelope it cannot read.')
    }

    $commandText = Get-ClaudeHookToolInputString -ToolInput $payload.Value -Name 'command'
    if (-not $commandText) {
        return Get-PromotionMcpOnlyAllowDecision
    }

    $reason = Get-PromotionBypassReason -CommandText $commandText
    if ($reason) {
        return Get-PromotionMcpOnlyBlockDecision -Reason $reason
    }

    return Get-PromotionMcpOnlyAllowDecision
}

function Invoke-PromotionMcpOnlyEntryPoint {
    <#
    .SYNOPSIS
        Runs the hook decision and returns the process exit code.
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

    $decision = Invoke-PromotionMcpOnlyDecision -ToolInputRaw $ToolInputRaw
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
$entryPointResult = @(Invoke-PromotionMcpOnlyEntryPoint)
if ($entryPointResult.Count -gt 1) {
    $entryPointResult[0..($entryPointResult.Count - 2)] | Write-Output
}

exit ([int]$entryPointResult[-1])