<#
.SYNOPSIS
    Pre-tool-use hook that enforces the pr-author skill is used before gh pr create or gh pr edit.

.DESCRIPTION
    Invoked by the Claude Code PreToolUse hook before any Bash command runs. Reads
    the hook payload through the shared reader, extracts the attempted command from the
    envelope's nested tool_input, and blocks gh pr create / gh pr edit commands that
    bypass the pr-author skill workflow.

    Required sequence:
      1. mcp__drm-copilot__collect_pr_context writes artifacts/pr_context.summary.txt
      2. pr-author skill produces the body text; the pr-author agent writes
         artifacts/pr_body_<N>.md and a sibling integrity receipt
         artifacts/pr_body_<N>.receipt.json
      3. gh pr create --body-file artifacts/pr_body_<N>.md
         (or gh pr edit --body-file ...)

    Block cases:
      Case A - gh pr create or gh pr edit with --body (inline, no --body-file): blocked.
      Case B - gh pr create with neither --body nor --body-file: blocked.
      Case C - gh pr create or gh pr edit with --body-file but context artifact absent: blocked.
      Preflight - --body-file/context present: orchestrator-state checkpoint must pass
                  --require-pr-creation-ready before receipt verification runs, else blocked.
      Receipt - preflight passed: the SHA-256 receipt is verified in five ordered checks
                (Section below). The first failing check blocks.

    Receipt verification decision order on the --body-file-with-context path:
      PR_BODY_PATH_NONCANONICAL -> PR_AUTHOR_RECEIPT_MISSING -> PR_AUTHOR_RECEIPT_NUMBER_MISMATCH
      -> PR_AUTHOR_RECEIPT_HASH_MISMATCH -> PR_AUTHOR_RECEIPT_STALE -> allow.

.NOTES
    Compatible with PowerShell 7+. No external module dependencies.

    Enforcement strength: the SHA-256 receipt is a policy-level integrity check that binds the
    PR body bytes to the receipt the pr-author agent wrote. It is not a cryptographic or security
    boundary: any actor with Write access to artifacts/ can replace both the body file and the
    receipt together, because all agents share the same filesystem and the runtime exposes no
    native agent-identity signal at Bash PreToolUse time. The mechanism prevents accidental bypass
    and requires a deliberate, documented act to circumvent. It MUST NOT be described as
    tamper-proof or as a security boundary.
#>
[CmdletBinding()]
param()


Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force
$script:PrContextArtifactPath = 'artifacts/pr_context.summary.txt'
$script:OrchestratorStateCheckpointPath = 'artifacts/orchestration/orchestrator-state.json'

Import-Module (Join-Path $PSScriptRoot '../lib/orchestrator-state/OrchestratorState.psm1') -Force

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

function Get-PrBodyFileBytes {
    <#
    .SYNOPSIS
        Read the raw bytes of the PR body file. Tests mock this function (read seam).
    .DESCRIPTION
        Returns the byte content of the supplied body-file path, or $null when the file is absent.
        This is the injectable boundary for body-file bytes in tests; no test writes the body file
        to disk. The bytes are hashed inline by the receipt verification function.
    .PARAMETER BodyFilePath
        The relative path to the PR body file (for example artifacts/pr_body_5.md).
    .OUTPUTS
        System.Byte[] or $null
    #>
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseSingularNouns', '', Justification = 'The plural noun names the byte-array return; the seam name is fixed by the receipt contract.')]
    [CmdletBinding()]
    [OutputType([byte[]])]
    param(
        [Parameter(Mandatory)]
        [string] $BodyFilePath
    )

    if (-not (Test-Path -LiteralPath $BodyFilePath)) {
        return $null
    }

    return [System.IO.File]::ReadAllBytes($BodyFilePath)
}

function Get-PrAuthorReceiptContent {
    <#
    .SYNOPSIS
        Read the raw JSON text of the PR body receipt. Tests mock this function (read seam).
    .DESCRIPTION
        Returns the raw text content of the sibling receipt file artifacts/pr_body_<N>.receipt.json,
        or $null when the receipt file is absent. This is the injectable boundary for receipt
        content in tests; no test writes the receipt file to disk.
    .PARAMETER ReceiptFilePath
        The relative path to the receipt file (for example artifacts/pr_body_5.receipt.json).
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [string] $ReceiptFilePath
    )

    if (-not (Test-Path -LiteralPath $ReceiptFilePath)) {
        return $null
    }

    return (Get-Content -LiteralPath $ReceiptFilePath -Raw)
}

function Get-PrContextSummaryLastWriteUtc {
    <#
    .SYNOPSIS
        Return the UTC last-write time of the PR context summary. Tests mock this function (seam).
    .DESCRIPTION
        Returns the LastWriteTimeUtc of artifacts/pr_context.summary.txt, or $null when the file is
        absent. The staleness check compares the receipt's created_at against this value; both are
        artifact metadata, so no wall-clock seam is required.
    .OUTPUTS
        System.DateTime or $null
    #>
    [CmdletBinding()]
    [OutputType([datetime])]
    param()

    if (-not (Test-Path -LiteralPath $script:PrContextArtifactPath)) {
        return $null
    }

    return (Get-Item -LiteralPath $script:PrContextArtifactPath).LastWriteTimeUtc
}

. (Join-Path $PSScriptRoot 'enforce-pr-author-skill.epic-base-branch.ps1')

# Dot-source the receipt-verification and bypass-reason helpers. Guarded so dot-sourcing this
# hook in tests loads the helpers too (issue #501 headroom split).
. (Join-Path $PSScriptRoot 'enforce-pr-author-skill-helpers.ps1')

function Invoke-PrAuthorSkillDecision {
    <#
    .SYNOPSIS
        Parse the PreToolUse envelope and return an allow-or-block decision.
    .DESCRIPTION
        Envelope-level anomalies fail closed as a deny; missing command text inside a
        well-formed tool_input remains an allow, because that is this gate's scope filter.
    .PARAMETER ToolInputRaw
        The raw JSON hook payload acquired by Read-ClaudeHookRawPayload.
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

    $payload = Resolve-ClaudeHookToolInput -Raw $ToolInputRaw
    if (-not $payload.IsValid) {
        return Get-PrAuthorSkillBlockDecision -Reason (
            'PR_AUTHOR_SKILL_BLOCKED: payload anomaly - ' +
            (Get-ClaudeHookPayloadAnomalyReason -Anomaly $payload.Anomaly) +
            '. The gate fails closed on an envelope it cannot read.')
    }

    $commandText = Get-ClaudeHookToolInputString -ToolInput $payload.Value -Name 'command'
    if (-not $commandText) {
        return Get-PrAuthorSkillAllowDecision
    }

    $contextExists = Get-PrContextArtifactExistence
    $reason = Get-PrAuthorBypassReason -CommandText $commandText -ContextExists $contextExists

    if ($reason) {
        return Get-PrAuthorSkillBlockDecision -Reason $reason
    }

    return Get-PrAuthorSkillAllowDecision
}

function Get-PrAuthorSkillAllowDecision {
    <#
    .SYNOPSIS
        Construct the PreToolUse allow decision for a permitted Bash command.
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

function Get-PrAuthorSkillBlockDecision {
    <#
    .SYNOPSIS
        Construct the PreToolUse deny decision for a forbidden Bash command.
    .PARAMETER Reason
        The specific deny reason to surface in the decision.
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

function Test-PrAuthorBypassRequired {
    <#
    .SYNOPSIS
        Return $true when a Bash command requires the pr-author skill to run first.
    .PARAMETER CommandText
        The Bash command text extracted from the envelope's tool_input.
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

function Invoke-PrAuthorSkillEntryPoint {
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

    $decision = Invoke-PrAuthorSkillDecision -ToolInputRaw $ToolInputRaw
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
$entryPointResult = @(Invoke-PrAuthorSkillEntryPoint)
if ($entryPointResult.Count -gt 1) {
    $entryPointResult[0..($entryPointResult.Count - 2)] | Write-Output
}

exit ([int]$entryPointResult[-1])