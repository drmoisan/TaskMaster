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
      Case C - gh pr create or gh pr edit with --body-file but context artifact absent beneath
               the resolved worktree: blocked.
      Preflight - --body-file/context present: orchestrator-state checkpoint must pass
                  --require-pr-creation-ready before receipt verification runs, else blocked.
      Docs-only - issue #901: when the preflight fails, gh pr create proceeds to receipt
                  verification only when every condition holds: no checkpoint file exists
                  beneath the resolved worktree; the command carries exactly one explicit
                  --base <branch> and no base or repository override (issue #927: no
                  single-dash token whose letters include B or R, such as -B, -R, or -dB; no
                  --repo or --repo= token; no inline GH_REPO= prefix); refs/remotes/origin/<branch>
                  resolves to a commit; git diff --name-only -z --no-renames <commit>...HEAD
                  exits 0, ends its non-empty output with NUL, and lists at least one path; and
                  every listed path starts with docs/ (ordinal, case-sensitive). Otherwise the
                  deny keeps ORCHESTRATOR_STATE_PREFLIGHT_FAILED and appends one sub-code:
                    DOCS_ONLY_EXEMPTION_NOT_CREATE         - gh pr edit is never exempt.
                    DOCS_ONLY_EXEMPTION_CHECKPOINT_PRESENT - a checkpoint file exists.
                    DOCS_ONLY_EXEMPTION_BASE_UNRESOLVED    - no single explicit --base, a base or
                                                             repository override is present, or
                                                             the base does not resolve on origin.
                    DOCS_ONLY_EXEMPTION_DIFF_FAILED        - the diff exits non-zero, throws, or
                                                             returns output not ending with NUL.
                    DOCS_ONLY_EXEMPTION_EMPTY_DIFF         - the diff lists no path.
                    DOCS_ONLY_EXEMPTION_NON_DOCS_PATH      - a listed path is outside docs/.
                  The exemption never applies under epic scope.
      Receipt - preflight passed or docs-only exemption applied: the SHA-256 receipt is
                verified in the ordered checks (Section below). The first failing check blocks.

    Receipt verification decision order on the --body-file-with-context path:
      PR_BODY_PATH_NONCANONICAL -> PR_AUTHOR_RECEIPT_MISSING -> PR_AUTHOR_RECEIPT_NUMBER_MISMATCH
      -> PR_AUTHOR_RECEIPT_HASH_MISMATCH -> PR_AUTHOR_RECEIPT_STALE -> allow.

    Issue #850: the context summary, the body, and the receipt are read beneath the worktree
    the call resolves to (the item worktree, or the epic checkpoint's worktree under epic
    scope), not beneath the process directory, so a gate running in the session worktree
    validates the artifacts of the item it gates.

.NOTES
    Compatible with PowerShell 7+. Dependencies are in-repository only: hook-dependency-guard.ps1,
    the HookPayload, OrchestratorState, and OrchestratorStateUnconditional modules, and the
    epic-base-branch and helpers siblings; the helpers load the command parsers, the
    worktree-resolution modules, and the artifact-root and docs-only siblings.
    No PowerShell Gallery module is required.
    The docs-only exemption runs the git executable at call time (issue #901).

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
$script:HookDependencyGuardLoadFailed = $false; try { . (Join-Path $PSScriptRoot 'hook-dependency-guard.ps1') } catch { $script:HookDependencyGuardLoadFailed = $true }


try { Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'HookPayload.psm1' -ErrorRecord $_ }
$script:PrContextArtifactPath = 'artifacts/pr_context.summary.txt'
# Null until the resolution step assigns the absolute path of the resolved worktree's
# checkpoint (issue #673); a relative default here was the process-directory binding.
$script:OrchestratorStateCheckpointPath = $null

try { Import-Module (Join-Path $PSScriptRoot '../lib/orchestrator-state/OrchestratorState.psm1') -Force -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'OrchestratorState.psm1' -ErrorRecord $_ }

function Get-PrContextArtifactExistence {
    <#
    .SYNOPSIS
        Wrapper around Test-Path for the PR context artifact. Tests mock this function.
    .PARAMETER Path
        The absolute path of artifacts/pr_context.summary.txt beneath the resolved worktree.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)]
        [string] $Path
    )

    return [bool](Test-Path -LiteralPath $Path)
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
        The absolute path to the PR body file beneath the resolved worktree (for example
        <worktree>/artifacts/pr_body_5.md).
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
        The absolute path to the receipt file beneath the resolved worktree (for example
        <worktree>/artifacts/pr_body_5.receipt.json).
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
    .PARAMETER Path
        The absolute path of artifacts/pr_context.summary.txt beneath the resolved worktree.
    .OUTPUTS
        System.DateTime or $null
    #>
    [CmdletBinding()]
    [OutputType([datetime])]
    param(
        [Parameter(Mandatory)]
        [string] $Path
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        return $null
    }

    return (Get-Item -LiteralPath $Path).LastWriteTimeUtc
}

try { . (Join-Path $PSScriptRoot 'enforce-pr-author-skill.epic-base-branch.ps1') } catch { Add-HookDependencyFailure -Name 'enforce-pr-author-skill.epic-base-branch.ps1' -ErrorRecord $_ }

# Dot-source the receipt-verification and bypass-reason helpers. Guarded so dot-sourcing this
# hook in tests loads the helpers too (issue #501 headroom split).
try { . (Join-Path $PSScriptRoot 'enforce-pr-author-skill-helpers.ps1') } catch { Add-HookDependencyFailure -Name 'enforce-pr-author-skill-helpers.ps1' -ErrorRecord $_ }
try { Import-Module (Join-Path $PSScriptRoot '../lib/orchestrator-state/OrchestratorStateUnconditional.psm1') -Force -Global -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'OrchestratorStateUnconditional.psm1' -ErrorRecord $_ }
try { Import-Module (Join-Path $PSScriptRoot '../lib/orchestrator-state/OrchestratorState.psm1') -Force -Global -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'OrchestratorState.psm1' -ErrorRecord $_ }

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
    $dependencyDecision = Get-HookDependencyFailureDecision -HookEvent PreToolUse -ReasonPrefix 'PR_AUTHOR_SKILL_BLOCKED:'
    if ($null -ne $dependencyDecision) { return $dependencyDecision }

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

    $reason = Get-PrAuthorBypassReason -CommandText $commandText

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
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)]
        [string] $CommandText
    )

    return ($null -ne (Get-PrAuthorBypassReason -CommandText $CommandText))
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
if ($script:HookDependencyGuardLoadFailed) { [Console]::Error.WriteLine('PR_AUTHOR_SKILL_BLOCKED: hook-dependency-guard.ps1 failed to load; the gate fails closed.'); exit 2 }
$dependencyDecision = Get-HookDependencyFailureDecision -HookEvent PreToolUse -ReasonPrefix 'PR_AUTHOR_SKILL_BLOCKED:'
if ($null -ne $dependencyDecision) { $dependencyDecision | ConvertTo-Json -Compress -Depth 5 | Write-Output; exit 0 }

# The entry point returns its [int] exit code as the last pipeline element and the
# decision JSON before it. `exit (<call>)` would capture BOTH into the exit
# expression and emit nothing, so the decision is written explicitly here first.
$entryPointResult = @(Invoke-PrAuthorSkillEntryPoint)
if ($entryPointResult.Count -gt 1) {
    $entryPointResult[0..($entryPointResult.Count - 2)] | Write-Output
}

exit ([int]$entryPointResult[-1])