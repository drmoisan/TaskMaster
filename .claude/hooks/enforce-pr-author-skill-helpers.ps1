<#
.SYNOPSIS
    Dot-sourced receipt-verification and bypass-reason helpers for enforce-pr-author-skill.ps1.

.DESCRIPTION
    Holds the two decision helpers the pr-author gate uses once a Bash command has been
    identified as a gh pr create or gh pr edit invocation:

      - Test-PrAuthorReceiptVerification: the six ordered SHA-256 receipt checks.
      - Get-PrAuthorBypassReason: cases A, B, and C plus the orchestrator-state preflight
        and the receipt-verification call.

    They are split out of the parent hook, .claude/hooks/enforce-pr-author-skill.ps1,
    because that file had no headroom left under the 500-line limit once the shared
    payload-reader migration (issue #501) added the module import, the envelope-anomaly
    mapping, and the entry-point seam. The split is a pure move with no behaviour change.

    Both helpers read the script-scoped configuration the parent hook declares and call the
    parent hook's injectable read seams, so this file is only ever dot-sourced from that hook
    and never invoked on its own. $script:PrContextArtifactPath is the repository-relative
    literal used in messages and as the composition input; every artifact read is composed
    beneath the root Resolve-PrAuthorArtifactRoot selects (issue #850).
    $script:OrchestratorStateCheckpointPath is different after
    issue #673: the parent declares it null, and Get-PrAuthorBypassReason assigns it the
    absolute checkpoint path of the worktree identity resolution selected, so every later
    reader receives one resolved path rather than deriving its own.

.NOTES
    Compatible with PowerShell 7+. Dependencies are in-repository only: the command parsers
    and worktree-resolution modules loaded below, and the artifact-root and docs-only siblings.
    No PowerShell Gallery module is required.
    The docs-only sibling runs the git executable at call time (issue #901).
#>
[CmdletBinding()]
param()

# Shared command-line parser (issue #545). Dot-sourced here as well as from the parent hook
# so this file keeps working when a test dot-sources it directly.
. (Join-Path $PSScriptRoot 'hook-command-scanner.ps1')
. (Join-Path $PSScriptRoot 'hook-command-invocation.ps1')
# Worktree target resolution (issue #687). The checkpoint this gate validates must belong to
# the item the call is about. Reading a session-root-relative path made that the item whose
# worktree happened to occupy the session root, so in a parallel or epic topology the gate
# could return allow on the strength of a sibling item's state -- a false green, which is
# worse than a false denial because it reports success for a lifecycle it never checked.
Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/WorktreeResolution.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/WorktreeTargetResolution.psm1') -Force -ErrorAction Stop
# Portable-identity resolution (issue #673). A path signal cannot select a worktree: a merged
# feature folder exists in every checkout branched from main, so it places a call in many at
# once. The item is identified by its canonical issue number and its branch instead.
Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/WorktreeItemResolution.psm1') -Force -ErrorAction Stop
# Epic scope (issue #663): the epic integration pull request is gated against the epic checkpoint.
Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/EpicScopeResolution.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/EpicScopeReadiness.psm1') -Force -ErrorAction Stop
# Artifact-root resolution and the Check 1 body-path binding (issue #850).
. (Join-Path $PSScriptRoot 'enforce-pr-author-skill.artifact-root.ps1')
# Docs-only exemption from the orchestrator-state preflight (issue #901).
. (Join-Path $PSScriptRoot 'enforce-pr-author-skill.docs-only.ps1')

function Resolve-PrAuthorWorktreeTarget {
    <#
    .SYNOPSIS
        Resolve the call's target worktree. Tests mock this function (resolution seam).
    .DESCRIPTION
        A seam rather than a direct call because the resolver reads real git state: the
        worktree list, each registration's branch, and the filesystem. A test that drove it
        directly would depend on the machine's live worktrees and would pass or fail
        according to what other sessions happen to have checked out, which the repository's
        determinism policy forbids. Mocking here lets a test state the resolution outcome it
        is exercising and nothing else.
    .OUTPUTS
        The target result object from Resolve-WorktreeItemTarget.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string] $CommandText
    )

    return (Resolve-WorktreeItemTarget -Text $CommandText -SessionRoot (Get-Location).Path)
}

function Get-PrAuthorTargetCheckpointResolution {
    <#
    .SYNOPSIS
        Resolve the checkpoint path this call must be validated against.
    .DESCRIPTION
        Resolves the target worktree from the command text by portable identity and maps the
        four resolution states onto a checkpoint path or a deny reason:

          SessionRoot   -> that worktree's own checkpoint, composed absolutely.
          OtherWorktree -> that worktree's own checkpoint, composed the same way.
          NoTarget      -> deny, carrying the code Get-WorktreeResolutionNoTargetReasonCode
                           returns. pr-author passes --head on every gh pr create, so a call
                           reaching here omitted the explicit target; the remedy is to supply
                           it, never to fall back to whichever checkpoint occupies the
                           session root.
          Ambiguous     -> deny, carrying the code
                           Get-WorktreeResolutionAmbiguityReasonCode returns.

        The two resolved states share one branch because the path is now composed the same
        way for both: identity selects the worktree, and the checkpoint is read beneath it.
        A session-root result is not a special case, it is the case where identity happened
        to select the worktree the process is already running in.

        Returning a deny for the two unresolved states is what keeps the gate honest: it
        refuses to answer rather than answering from unrelated state. Neither reason-code
        literal appears in this file; both come from the accessors named above.
    .OUTPUTS
        System.Collections.Specialized.OrderedDictionary with CheckpointPath, Reason,
        WorktreeRoot (the resolved root, or $null when unresolved), and Status (the target's
        status), so the artifact root is taken from the same resolution (issue #850).
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [string] $CommandText
    )

    $target = Resolve-PrAuthorWorktreeTarget -CommandText $CommandText

    switch ($target.Status) {
        { $_ -in @('SessionRoot', 'OtherWorktree') } {
            return [ordered]@{ CheckpointPath = (Get-WorktreeItemCheckpointPath -WorktreeRoot $target.WorktreeRoot); Reason = $null; WorktreeRoot = $target.WorktreeRoot; Status = $target.Status }
        }
        default {
            $reason = "$($target.ReasonCode): $($target.Detail) The pr-author gate will not validate this call against the session root's checkpoint, because that checkpoint may belong to a different item. Pass --head <branch> on the gh pr create command so the call names its own target."
            return [ordered]@{ CheckpointPath = $null; Reason = $reason; WorktreeRoot = $null; Status = $target.Status }
        }
    }
}

function Get-PrAuthorBodyFileValue {
    <#
    .SYNOPSIS
        Return the normalized --body-file value of the matched gh pr create or gh pr edit, or $null.
    .DESCRIPTION
        Reads the flag value from the first Structural gh pr create match, else gh pr edit.
        Backslashes become forward slashes and one leading './' is removed (issue #824). A
        rooted value is returned as written, with separators normalized, and is bound to the
        resolved worktree by Get-PrAuthorBodyPathBindingReason (issue #850).
    .PARAMETER CommandText
        The Bash command text under evaluation.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [string] $CommandText
    )

    $subcommandPath = @('pr', 'edit')
    if (Test-CommandLineInvocation -CommandText $CommandText -CommandWord 'gh' -SubcommandPath @('pr', 'create')) {
        $subcommandPath = @('pr', 'create')
    }
    $value = Get-CommandLineFlagValue -CommandText $CommandText -CommandWord 'gh' -SubcommandPath $subcommandPath -FlagName '--body-file'
    if ($null -eq $value) {
        return $null
    }
    $value = $value.Replace('\', '/')
    if ($value.StartsWith('./')) {
        $value = $value.Substring(2)
    }
    return $value
}

function Test-PrAuthorReceiptVerification {
    <#
    .SYNOPSIS
        Verify the SHA-256 receipt and return a block-reason string, or $null when verified.
    .DESCRIPTION
        Runs the six ordered receipt checks on the --body-file-with-context path. Each check is its
        own short-circuiting branch; the first failure returns its reason code:
          1. PR_BODY_PATH_NONCANONICAL        - --body-file path is not the canonical
                                                 artifacts/pr_body_<N>.md beneath the artifact
                                                 root (relative only for the session worktree).
          2. PR_AUTHOR_RECEIPT_MISSING        - sibling artifacts/pr_body_<N>.receipt.json absent.
          3. PR_AUTHOR_RECEIPT_NUMBER_MISMATCH- receipt.number (integer) != <N> from the path.
          4. PR_AUTHOR_RECEIPT_HASH_MISMATCH  - inline SHA-256 (lowercase hex) of the body bytes
                                                 != receipt.sha256.
          5. PR_AUTHOR_RECEIPT_STALE          - receipt.created_at (UTC) not strictly newer than the
                                                 context summary last-write time.
          6. EPIC_BASE_BRANCH_MISMATCH        - under epic_mode, gh pr create does not carry a
                                                 matching --base <epic_context.integration_branch>.
        Returns $null when all six checks pass (allow). All disk access flows through the four
        injectable seams (Get-PrBodyFileBytes, Get-PrAuthorReceiptContent,
        Get-PrContextSummaryLastWriteUtc, Get-PrAuthorCheckpointContent), each given an absolute
        path beneath the artifact root (issue #850); SHA-256 is computed inline. This is a
        policy-level integrity check, not a cryptographic control: any actor with Write access
        to artifacts/ can replace the body file and the receipt together.
    .PARAMETER CommandText
        The Bash command text containing the --body-file argument.
    .PARAMETER CheckpointPath
        The absolute checkpoint path identity resolution selected, passed through to check 6
        so that check reads the resolved worktree's checkpoint rather than deriving its own.
    .PARAMETER EpicScope
        Optional. The epic-scope result Get-PrAuthorBypassReason resolved (issue #663), passed
        through to check 6 so the call is resolved once; $null outside epic scope.
    .PARAMETER ArtifactRoot
        The resolved worktree root the body, receipt, and summary are read beneath.
    .PARAMETER RelativeBodyAllowed
        Whether a relative --body-file is read from the resolved worktree (the session worktree).
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [string] $CommandText,

        [Parameter(Mandatory)]
        [string] $CheckpointPath,

        [AllowNull()]
        [object] $EpicScope,

        [Parameter(Mandatory)]
        [string] $ArtifactRoot,

        [bool] $RelativeBodyAllowed
    )

    # Check 1: the --body-file value must be the canonical artifacts/pr_body_<N>.md beneath the
    # artifact root (issue #850); C1a's structural read and spelling normalization are reused.
    $binding = Get-PrAuthorBodyPathBindingReason -CommandText $CommandText -ArtifactRoot $ArtifactRoot -RelativeBodyAllowed $RelativeBodyAllowed
    if ($binding.Reason) {
        return $binding.Reason
    }

    $bodyNumber = $binding.BodyNumber
    $bodyFilePath = Join-WorktreeResolutionPath -WorktreeRoot $ArtifactRoot -RepoRelativePath "artifacts/pr_body_$bodyNumber.md"
    $receiptFilePath = Join-WorktreeResolutionPath -WorktreeRoot $ArtifactRoot -RepoRelativePath "artifacts/pr_body_$bodyNumber.receipt.json"
    $summaryPath = Join-WorktreeResolutionPath -WorktreeRoot $ArtifactRoot -RepoRelativePath $script:PrContextArtifactPath

    # Check 2: the sibling receipt file must exist (read via the injectable seam).
    $receiptRaw = Get-PrAuthorReceiptContent -ReceiptFilePath $receiptFilePath
    if ([string]::IsNullOrWhiteSpace($receiptRaw)) {
        return "PR_AUTHOR_RECEIPT_MISSING: ``$receiptFilePath`` is absent. The pr-author agent must write the SHA-256 receipt alongside ``$bodyFilePath`` before issuing ``gh pr create``/``gh pr edit --body-file``."
    }

    # A receipt that is present but not valid JSON cannot be verified; treat it as missing content.
    try {
        $receipt = $receiptRaw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        return "PR_AUTHOR_RECEIPT_MISSING: ``$receiptFilePath`` is not valid JSON. The pr-author agent must write a well-formed receipt with ``number``, ``sha256``, and ``created_at``."
    }

    # Check 3: the receipt number must equal the <N> extracted from the canonical path.
    $receiptNumber = $null
    $parsedNumber = 0
    if ([int]::TryParse([string]$receipt.number, [ref] $parsedNumber)) {
        $receiptNumber = $parsedNumber
    }
    if ($receiptNumber -ne $bodyNumber) {
        return "PR_AUTHOR_RECEIPT_NUMBER_MISMATCH: ``$receiptFilePath`` ``number`` ($($receipt.number)) does not equal the body-file number ($bodyNumber). The receipt must bind to ``$bodyFilePath``."
    }

    # Check 4: the inline SHA-256 (lowercase hex) of the body bytes must equal receipt.sha256.
    $bodyBytes = Get-PrBodyFileBytes -BodyFilePath $bodyFilePath
    if ($null -eq $bodyBytes) {
        return "PR_AUTHOR_RECEIPT_HASH_MISMATCH: ``$bodyFilePath`` could not be read to verify its SHA-256 against ``$receiptFilePath``."
    }

    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try {
        $hashBytes = $sha256.ComputeHash($bodyBytes)
    } finally {
        $sha256.Dispose()
    }
    $computedHash = ([System.BitConverter]::ToString($hashBytes) -replace '-', '').ToLowerInvariant()

    if ($computedHash -ne ([string]$receipt.sha256).ToLowerInvariant()) {
        return "PR_AUTHOR_RECEIPT_HASH_MISMATCH: the SHA-256 of ``$bodyFilePath`` does not equal ``sha256`` in ``$receiptFilePath``. The body file was modified after the receipt was written."
    }

    # Check 5: receipt.created_at (UTC) must be strictly newer than the context summary last-write.
    $createdAt = [DateTime]::MinValue
    $createdParsed = [DateTime]::TryParse(
        [string]$receipt.created_at,
        [System.Globalization.CultureInfo]::InvariantCulture,
        [System.Globalization.DateTimeStyles]::AdjustToUniversal -bor [System.Globalization.DateTimeStyles]::AssumeUniversal,
        [ref] $createdAt)
    if (-not $createdParsed) {
        return "PR_AUTHOR_RECEIPT_STALE: ``$receiptFilePath`` has a missing or unparseable ``created_at``. The pr-author agent must record a UTC ISO-8601 ``created_at`` strictly newer than ``$script:PrContextArtifactPath``."
    }

    $contextLastWrite = Get-PrContextSummaryLastWriteUtc -Path $summaryPath
    if (($null -eq $contextLastWrite) -or ($createdAt -le $contextLastWrite)) {
        return "PR_AUTHOR_RECEIPT_STALE: ``$receiptFilePath`` ``created_at`` is not strictly newer than the last-write time of ``$script:PrContextArtifactPath``. The pr-author agent must regenerate the body and receipt after refreshing the PR context."
    }

    # Check 6: under epic_mode, gh pr create must carry a matching --base override.
    $epicBaseBranchReason = Test-EpicBaseBranchOverride -CommandText $CommandText -CheckpointPath $CheckpointPath -EpicScope $EpicScope
    if ($epicBaseBranchReason) {
        return $epicBaseBranchReason
    }

    return $null
}

function Get-PrAuthorBypassReason {
    <#
    .SYNOPSIS
        Inspect the command text and return a block reason string, or $null when the command is allowed.
    .DESCRIPTION
        Returns PR_AUTHOR_SKILL_BLOCKED when gh pr create or gh pr edit is run with --body (inline,
        no --body-file), or when gh pr create is run with no body flag at all; these cases and the
        gh pr edit no-body allow perform no resolution. For a --body-file command the artifact root
        is resolved once (issue #850), and the deny precedence is: the resolution reason
        (no-target or ambiguity code, or an underivable epic worktree), PR_CONTEXT_MISSING when the
        context summary is absent beneath the resolved worktree, ORCHESTRATOR_STATE_PREFLIGHT_FAILED
        unless Get-PrAuthorDocsOnlyExemption exempts a docs-only gh pr create (issue #901), then
        receipt checks 1-6 via Test-PrAuthorReceiptVerification. Returns $null for all allowed
        patterns.
    .PARAMETER CommandText
        The Bash command text extracted from the envelope's tool_input.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [string] $CommandText
    )

    # Only act on gh pr create or gh pr edit subcommands. The test is structural: a segment
    # must INVOKE gh with the subcommand path, so quoted prose that merely mentions the
    # phrase no longer triggers the gate, and a relocating spelling that carries a gh global
    # option between the command word and the subcommand now does.
    $isPrCreate = Test-CommandLineInvocation -CommandText $CommandText -CommandWord 'gh' -SubcommandPath @('pr', 'create')
    $isPrEdit = Test-CommandLineInvocation -CommandText $CommandText -CommandWord 'gh' -SubcommandPath @('pr', 'edit')

    if (-not $isPrCreate -and -not $isPrEdit) {
        return $null
    }

    # Flag presence comes from the matched segment's tokens. Exact token comparison is what
    # keeps '--body' from matching '--body-file'; the previous negative lookahead expressed
    # the same distinction over raw text.
    $subcommandPath = if ($isPrCreate) { @('pr', 'create') } else { @('pr', 'edit') }
    $hasBodyFile = Test-CommandLineFlag -CommandText $CommandText -CommandWord 'gh' -SubcommandPath $subcommandPath -FlagName '--body-file'
    $hasInlineBody = Test-CommandLineFlag -CommandText $CommandText -CommandWord 'gh' -SubcommandPath $subcommandPath -FlagName '--body'

    # Raw-scan fallback (issue #824). Wrapper payloads are now read structurally, so the flags
    # above already come from a payload's own tokens. The raw text is scanned only when the
    # invocation could not be read structurally at all: no Structural match exists for the
    # subcommand path and at least one Indeterminate match does. The --body-file test runs
    # first and wins, so a --body-file is never misread as an inline body.
    if (-not $hasBodyFile -and -not $hasInlineBody) {
        $matchList = @(Get-CommandLineInvocation -CommandText $CommandText -CommandWord 'gh' -SubcommandPath $subcommandPath)
        if (@($matchList | Where-Object { $_.Status -eq 'Structural' }).Count -eq 0) {
            $comparison = [System.StringComparison]::OrdinalIgnoreCase
            foreach ($match in @($matchList | Where-Object { $_.Status -eq 'Indeterminate' })) {
                if ($match.Segment.RawText.IndexOf('--body-file', $comparison) -ge 0) {
                    $hasBodyFile = $true
                } elseif ($match.Segment.RawText.IndexOf('--body', $comparison) -ge 0) {
                    $hasInlineBody = $true
                }
            }
        }
    }

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

    # Issue #850: the worktree whose artifacts are read is resolved once, before Case C. Epic
    # scope (issue #663) selects the epic checkpoint's worktree and reads no per-feature
    # checkpoint; otherwise identity resolution selects the item worktree, and an unresolved
    # target denies here, ahead of PR_CONTEXT_MISSING.
    $artifact = Resolve-PrAuthorArtifactRoot -CommandText $CommandText
    if ($artifact.Reason) {
        return $artifact.Reason
    }

    # Case C: --body-file present but the context artifact is absent beneath the resolved worktree.
    $summaryPath = Join-WorktreeResolutionPath -WorktreeRoot $artifact.ArtifactRoot -RepoRelativePath $script:PrContextArtifactPath
    if (-not (Get-PrContextArtifactExistence -Path $summaryPath)) {
        return "PR_CONTEXT_MISSING: '$script:PrContextArtifactPath' in worktree '$($artifact.ArtifactRoot)' is absent. Run ``mcp__drm-copilot__collect_pr_context`` in that worktree before creating or editing the PR body."
    }

    # Orchestrator-state preflight: runs inside this same PreToolUse hook (so it cannot be
    # bypassed by invoking gh pr create/edit directly) before receipt verification. The
    # checkpoint path is assigned once here and read by every later consumer, so the
    # preflight, the receipt verifier, and the epic base-branch check gate one item.
    if ($null -ne $artifact.EpicScope) {
        $epicScope = $artifact.EpicScope
        $failure = Get-EpicPrCreationReadinessFailure -Checkpoint $epicScope.Checkpoint -HeadBranch $epicScope.Branch
        if ($failure) {
            return "ORCHESTRATOR_STATE_PREFLIGHT_FAILED: this epic-scope pull request was evaluated against $($epicScope.CheckpointPath), and the failed readiness predicate is '$failure'."
        }
        $script:OrchestratorStateCheckpointPath = $artifact.CheckpointPath
    } else {
        $script:OrchestratorStateCheckpointPath = $artifact.CheckpointPath
        $checkpointPath = $script:OrchestratorStateCheckpointPath
        $preflightResult = Invoke-OrchestratorStatePreflight -CheckpointPath $checkpointPath
        if ($preflightResult.HasErrors) {
            $preflightSummary = if ([string]::IsNullOrWhiteSpace($preflightResult.ErrorText)) {
                "checkpoint missing at $checkpointPath"
            } else {
                $preflightResult.ErrorText
            }
            $exemption = Get-PrAuthorDocsOnlyExemption -CommandText $CommandText -WorktreeRoot $artifact.ArtifactRoot -CheckpointPath $checkpointPath
            if (-not $exemption.Exempt) {
                return "ORCHESTRATOR_STATE_PREFLIGHT_FAILED: $preflightSummary (docs-only exemption not applied: $($exemption.Reason))"
            }
        }
    }

    # Receipt verification: extends, and does not replace, the previously-allowed path.
    $receiptReason = Test-PrAuthorReceiptVerification -CommandText $CommandText -CheckpointPath $script:OrchestratorStateCheckpointPath -EpicScope $artifact.EpicScope -ArtifactRoot $artifact.ArtifactRoot -RelativeBodyAllowed $artifact.RelativeBodyAllowed
    if ($receiptReason) {
        return $receiptReason
    }

    return $null
}
