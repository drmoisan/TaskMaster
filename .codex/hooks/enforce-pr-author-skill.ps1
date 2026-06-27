# Converted hook
# Review the generated hook behavior before enabling it.

<#
.SYNOPSIS
    Pre-tool-use hook that enforces the pr-author skill is used before gh pr create or gh pr edit.

.DESCRIPTION
    Invoked by the Claude Code PreToolUse hook before any Bash command runs. Reads
    tool input JSON from the CLAUDE_TOOL_INPUT environment variable, inspects the
    attempted command, and blocks gh pr create / gh pr edit commands that bypass the
    pr-author skill workflow.

    Required sequence:
      1. mcp__drm-copilot__collect_pr_context writes artifacts/pr_context.summary.txt
      2. pr-author skill reads that file and writes artifacts/pr_body_<N>.md
      3. gh pr create --body-file artifacts/pr_body_<N>.md
         (or gh pr edit --body-file ...)

    Block cases:
      Case A - gh pr create or gh pr edit with --body (inline, no --body-file): blocked.
      Case B - gh pr create with neither --body nor --body-file: blocked.
      Case C - gh pr create or gh pr edit with --body-file but context artifact absent: blocked.
      Case D - --body-file present, context artifact present, but the authorization sentinel
               artifacts/pr_author_authorization.json is absent or empty: blocked
               (PR_AGENT_AUTHORIZATION_MISSING).
      Malformed - sentinel present but not valid JSON, or issued_at missing/unparseable: blocked
               (PR_AGENT_AUTHORIZATION_MALFORMED).
      Case E - sentinel valid JSON but issued_by != "pr-author": blocked
               (PR_AGENT_AUTHORIZATION_INVALID).
      Case F - sentinel issued_by == "pr-author" but elapsed time since issued_at exceeds
               ttl_seconds: blocked (PR_AGENT_AUTHORIZATION_EXPIRED).

    Authorization sentinel decision order on the --body-file-with-context path:
      missing -> malformed -> invalid issuer -> expired -> allow.

.NOTES
    Compatible with PowerShell 7+. No external module dependencies.

    Enforcement strength: the authorization sentinel is a policy guardrail, not a cryptographic
    or security control. Any actor with Write access to artifacts/ can forge
    artifacts/pr_author_authorization.json, because all agents share the same filesystem and the
    runtime exposes no native agent-identity signal at Bash PreToolUse time. The mechanism prevents
    accidental bypass and requires a deliberate, documented act to circumvent. It MUST NOT be
    described as tamper-proof or as a security boundary.
#>
[CmdletBinding()]
param()

$script:PrContextArtifactPath = 'artifacts/pr_context.summary.txt'
$script:PrAuthorAuthorizationPath = 'artifacts/pr_author_authorization.json'
$script:PrAuthorAuthorizationTtlSeconds = 120

function Get-PrContextArtifactExistence {
    <#
    .SYNOPSIS
        Wrapper around Test-Path for the PR context artifact. Tests mock this function.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param()

    return [bool](Test-Path -LiteralPath $script:PrContextArtifactPath)
}

function Get-PrAuthorAuthorizationContent {
    <#
    .SYNOPSIS
        Read the raw text of the authorization sentinel. Tests mock this function (read seam).
    .DESCRIPTION
        Returns the raw text content of artifacts/pr_author_authorization.json, or $null when the
        sentinel file is absent. This is the injectable boundary for sentinel content in tests; no
        test writes the sentinel file to disk.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param()

    if (-not (Test-Path -LiteralPath $script:PrAuthorAuthorizationPath)) {
        return $null
    }

    return (Get-Content -LiteralPath $script:PrAuthorAuthorizationPath -Raw)
}

function Get-CurrentDateTimeUtc {
    <#
    .SYNOPSIS
        Return the current UTC time. Tests mock this function (clock seam) for time-travel scenarios.
    .OUTPUTS
        System.DateTime
    #>
    [CmdletBinding()]
    [OutputType([datetime])]
    param()

    return [DateTime]::UtcNow
}

function Test-PrAuthorAuthorization {
    <#
    .SYNOPSIS
        Validate the authorization sentinel and return a block-reason string, or $null when authorized.
    .DESCRIPTION
        Reads the sentinel via Get-PrAuthorAuthorizationContent and computes elapsed time via
        Get-CurrentDateTimeUtc. Applies the decision order missing -> malformed -> invalid issuer ->
        expired -> allow (spec FR-2 step 3):
          - $null/empty contents              -> PR_AGENT_AUTHORIZATION_MISSING
          - not valid JSON, or issued_at
            missing/unparseable               -> PR_AGENT_AUTHORIZATION_MALFORMED
          - issued_by != "pr-author"          -> PR_AGENT_AUTHORIZATION_INVALID
          - elapsed seconds > ttl_seconds     -> PR_AGENT_AUTHORIZATION_EXPIRED
          - all checks pass                   -> $null (allow)
        The ttl_seconds value defaults to the named constant when absent from the sentinel.
        This is a policy guardrail, not a cryptographic control; the sentinel is forgeable by any
        actor with Write access to artifacts/.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param()

    $contents = Get-PrAuthorAuthorizationContent

    if ([string]::IsNullOrWhiteSpace($contents)) {
        return "PR_AGENT_AUTHORIZATION_MISSING: ``$script:PrAuthorAuthorizationPath`` is absent or empty. The pr-author agent must write the authorization sentinel immediately before issuing ``gh pr create``/``gh pr edit --body*``."
    }

    try {
        $sentinel = $contents | ConvertFrom-Json -ErrorAction Stop
    } catch {
        return "PR_AGENT_AUTHORIZATION_MALFORMED: ``$script:PrAuthorAuthorizationPath`` is not valid JSON. The pr-author agent must write a well-formed sentinel with ``issued_by``, ``issued_at``, and ``ttl_seconds``."
    }

    $issuedAtRaw = $sentinel.issued_at
    if ([string]::IsNullOrWhiteSpace([string]$issuedAtRaw)) {
        return "PR_AGENT_AUTHORIZATION_MALFORMED: ``$script:PrAuthorAuthorizationPath`` is missing ``issued_at``. The pr-author agent must record a UTC ISO-8601 ``issued_at`` timestamp."
    }

    $issuedAt = [DateTime]::MinValue
    $parsed = [DateTime]::TryParse(
        [string]$issuedAtRaw,
        [System.Globalization.CultureInfo]::InvariantCulture,
        [System.Globalization.DateTimeStyles]::AdjustToUniversal -bor [System.Globalization.DateTimeStyles]::AssumeUniversal,
        [ref] $issuedAt)
    if (-not $parsed) {
        return "PR_AGENT_AUTHORIZATION_MALFORMED: ``$script:PrAuthorAuthorizationPath`` has an unparseable ``issued_at``. The pr-author agent must record a UTC ISO-8601 ``issued_at`` timestamp."
    }

    if ($sentinel.issued_by -ne 'pr-author') {
        return "PR_AGENT_AUTHORIZATION_INVALID: ``$script:PrAuthorAuthorizationPath`` ``issued_by`` is not ``pr-author``. Only the pr-author agent may authorize PR creation or body edits."
    }

    $ttlSeconds = $script:PrAuthorAuthorizationTtlSeconds
    if ($null -ne $sentinel.ttl_seconds) {
        $candidateTtl = 0
        if ([int]::TryParse([string]$sentinel.ttl_seconds, [ref] $candidateTtl)) {
            $ttlSeconds = $candidateTtl
        }
    }

    $elapsedSeconds = ((Get-CurrentDateTimeUtc) - $issuedAt).TotalSeconds
    if ($elapsedSeconds -gt $ttlSeconds) {
        return "PR_AGENT_AUTHORIZATION_EXPIRED: ``$script:PrAuthorAuthorizationPath`` issued $([math]::Round($elapsedSeconds)) s ago, exceeding the ${ttlSeconds}s TTL. The pr-author agent must write a fresh sentinel immediately before the ``gh`` command."
    }

    return $null
}

function Get-PrAuthorBypassReason {
    <#
    .SYNOPSIS
        Inspect the command text and return a block reason string, or $null when the command is allowed.
    .DESCRIPTION
        Returns PR_AUTHOR_SKILL_BLOCKED when gh pr create or gh pr edit is run with --body (inline,
        no --body-file), or when gh pr create is run with no body flag at all. Returns
        PR_CONTEXT_MISSING when --body-file is present but the context artifact
        does not exist on disk. When --body-file is present and the context artifact exists, evaluates
        the authorization sentinel via Test-PrAuthorAuthorization and returns its block reason
        (PR_AGENT_AUTHORIZATION_*) when authorization fails. Returns $null for all allowed patterns.
        Cases A, B, and C are evaluated first and unchanged; the sentinel check only extends the
        previously-allowed --body-file-with-context path.
    .PARAMETER CommandText
        The Bash command text extracted from CLAUDE_TOOL_INPUT.
    .PARAMETER ContextExists
        Whether artifacts/pr_context.summary.txt currently exists on disk.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [string] $CommandText,

        [Parameter(Mandatory)]
        [bool] $ContextExists
    )

    # Only act on gh pr create or gh pr edit subcommands.
    $isPrCreate = $CommandText -match '(?i)\bgh\s+pr\s+create\b'
    $isPrEdit = $CommandText -match '(?i)\bgh\s+pr\s+edit\b'

    if (-not $isPrCreate -and -not $isPrEdit) {
        return $null
    }

    $hasBodyFile = $CommandText -match '(?i)--body-file\b'
    $hasInlineBody = $CommandText -match '(?i)--body(?!-file)\b'

    # Case A: gh pr create OR gh pr edit with inline --body (not --body-file). Evaluated before the
    # gh pr edit no-body allow short-circuit so inline-body edits are blocked, not allowed.
    if (($isPrCreate -or $isPrEdit) -and $hasInlineBody -and -not $hasBodyFile) {
        return "PR_AUTHOR_SKILL_BLOCKED: ``gh pr create`` and ``gh pr edit`` must use ``--body-file`` with a file produced by the pr-author skill from ``$script:PrContextArtifactPath``. Run ``mcp__drm-copilot__collect_pr_context`` to generate the context file, apply the pr-author skill to produce ``artifacts/pr_body_<N>.md``, then pass that file via ``--body-file``."
    }

    if ($isPrCreate) {
        # Case B: gh pr create with no body flag at all.
        if (-not $hasInlineBody -and -not $hasBodyFile) {
            return "PR_AUTHOR_SKILL_BLOCKED: New PRs require ``--body-file``. Run ``mcp__drm-copilot__collect_pr_context`` to generate ``$script:PrContextArtifactPath``, apply the pr-author skill to produce ``artifacts/pr_body_<N>.md``, then pass that file via ``--body-file``."
        }
    }

    if ($isPrEdit) {
        # gh pr edit with no --body or --body-file (e.g., --title, --add-label, --reviewer) is allowed.
        if (-not $hasInlineBody -and -not $hasBodyFile) {
            return $null
        }
    }

    # Case C: --body-file present but context artifact is absent.
    if ($hasBodyFile -and -not $ContextExists) {
        return "PR_CONTEXT_MISSING: ``$script:PrContextArtifactPath`` is absent. Run ``mcp__drm-copilot__collect_pr_context`` before creating or editing the PR body."
    }

    # Cases D/E/F and malformed: --body-file present and context artifact exists; verify the
    # authorization sentinel. This extends, and does not replace, the previously-allowed path.
    if ($hasBodyFile -and $ContextExists) {
        $authorizationReason = Test-PrAuthorAuthorization
        if ($authorizationReason) {
            return $authorizationReason
        }
    }

    return $null
}

function Invoke-PrAuthorSkillDecision {
    <#
    .SYNOPSIS
        Parse CLAUDE_TOOL_INPUT and return an allow-or-block decision.
    .PARAMETER ToolInputRaw
        The raw JSON tool payload supplied by Claude Code.
    .OUTPUTS
        System.Collections.Specialized.OrderedDictionary
    .NOTES
        Missing tool input or missing command text is treated as allow.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [string] $ToolInputRaw
    )

    if (-not $ToolInputRaw) {
        return [ordered]@{ decision = 'allow' }
    }

    try {
        $toolInput = $ToolInputRaw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        throw "enforce-pr-author-skill hook received malformed JSON in CLAUDE_TOOL_INPUT: $_"
    }

    $commandText = $toolInput.command
    if (-not $commandText) {
        return [ordered]@{ decision = 'allow' }
    }

    $contextExists = Get-PrContextArtifactExistence
    $reason = Get-PrAuthorBypassReason -CommandText $commandText -ContextExists $contextExists

    if ($reason) {
        return [ordered]@{
            decision = 'block'
            reason   = $reason
        }
    }

    return [ordered]@{ decision = 'allow' }
}

function Test-PrAuthorBypassRequired {
    <#
    .SYNOPSIS
        Return $true when a Bash command requires the pr-author skill to run first.
    .PARAMETER CommandText
        The Bash command text extracted from CLAUDE_TOOL_INPUT.
    .PARAMETER ContextExists
        Whether artifacts/pr_context.summary.txt currently exists on disk.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)]
        [string] $CommandText,

        [Parameter(Mandatory)]
        [bool] $ContextExists
    )

    return ($null -ne (Get-PrAuthorBypassReason -CommandText $CommandText -ContextExists $ContextExists))
}

# Allow dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}

try {
    $decision = Invoke-PrAuthorSkillDecision -ToolInputRaw $env:CLAUDE_TOOL_INPUT
} catch {
    Write-Error $_
    exit 1
}

$decision | ConvertTo-Json -Compress | Write-Output

exit 0
