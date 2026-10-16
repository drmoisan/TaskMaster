<#
.SYNOPSIS
    Docs-only exemption from the checkpoint preflight of enforce-pr-author-skill.ps1 (issue #901).

.DESCRIPTION
    Dot-sourced by enforce-pr-author-skill-helpers.ps1. Get-PrAuthorBypassReason calls
    Get-PrAuthorDocsOnlyExemption only in its non-epic branch and only after
    Invoke-OrchestratorStatePreflight reports errors. The exemption lets a gh pr create call
    continue to the receipt checks when the branch changes nothing outside docs/. It fails
    closed: every condition below must hold, and the first that does not names the sub-code
    the caller appends to its ORCHESTRATOR_STATE_PREFLIGHT_FAILED denial.

      E1 DOCS_ONLY_EXEMPTION_NOT_CREATE         - the command is a structural gh pr create.
      E2 DOCS_ONLY_EXEMPTION_CHECKPOINT_PRESENT - no checkpoint file exists at the resolved path.
      E3 DOCS_ONLY_EXEMPTION_BASE_UNRESOLVED    - exactly one explicit --base, a conservative branch
                                                  spelling without '..', and no base or repository
                                                  override: no single-dash token whose letters
                                                  include B or R (-B, -R, -dB, -Rx), no --repo or
                                                  --repo= token, and no inline GH_REPO= prefix.
      E4 DOCS_ONLY_EXEMPTION_BASE_UNRESOLVED    - refs/remotes/origin/<base> resolves to a commit.
      E5 DOCS_ONLY_EXEMPTION_DIFF_FAILED        - git diff <commit>...HEAD exits 0 without throwing,
                                                  and non-empty output ends with NUL.
      E6 DOCS_ONLY_EXEMPTION_EMPTY_DIFF         - the diff lists at least one path.
      E7 DOCS_ONLY_EXEMPTION_NON_DOCS_PATH      - every path starts with docs/ (ordinal).

    The diff runs with --no-renames so a rename from outside docs/ reports its old path, with -z
    so no path is quoted, and with --no-relative, --no-color, and --no-ext-diff so repository
    configuration cannot change the listing.

    Residual limit: a GH_REPO value exported by an earlier shell command or inherited from the
    session environment is not visible in the command text, so this file cannot detect it.

.NOTES
    PowerShell 7+. Depends, at call time, on Test-CommandLineInvocation,
    Get-CommandLineStructuralMatch, and Get-CommandLineFlagValue from the command parsers the
    helpers file loads. It reads no file content. The git executable runs only through
    Invoke-PrAuthorDocsOnlyGitExe, the single executable seam, which never throws. Mirrored
    byte-identically under extensions/drm-copilot/resources/claude-customizations/.
    Dot-sources hook-docs-only-diff.ps1 (issue #927), whose Get-DocsOnlyDiffTextViolation
    performs the E5 NUL check and the E6 and E7 path classification.
#>
[CmdletBinding()]
param()
. (Join-Path $PSScriptRoot 'hook-docs-only-diff.ps1')

function Invoke-PrAuthorDocsOnlyGitExe {
    <#
    .SYNOPSIS
        Run git -C <root> with the given arguments. The single executable seam; tests mock it.
    .DESCRIPTION
        Never throws: an uncaught error ends the hook with exit 1, which PreToolUse treats as
        non-blocking, so a throw here would allow the call. A root that is not an existing
        directory, or a git that cannot start, returns ExitCode -1. Standard error is discarded
        so that it is never read as path data.
    .PARAMETER WorktreeRoot
        The resolved worktree root git runs in.
    .PARAMETER GitArgs
        The git arguments that follow -C <root>.
    .PARAMETER GitInvoker
        The scriptblock that runs git; a unit test passes a test-local stand-in so that this
        body runs without starting a process.
    .OUTPUTS
        System.Management.Automation.PSCustomObject with ExitCode and Output.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [AllowNull()][AllowEmptyString()][string] $WorktreeRoot,
        [Parameter(Mandatory)][string[]] $GitArgs,
        [ValidateNotNull()][scriptblock] $GitInvoker = { git @args 2>$null }
    )

    $failure = [pscustomobject]@{ ExitCode = -1; Output = '' }
    if ([string]::IsNullOrWhiteSpace($WorktreeRoot) -or -not (Test-Path -LiteralPath $WorktreeRoot -PathType Container)) {
        return $failure
    }
    try {
        $lines = @(& $GitInvoker -C $WorktreeRoot @GitArgs 2>$null)
        $code = $LASTEXITCODE
    }
    catch {
        return $failure
    }
    return [pscustomobject]@{ ExitCode = [int]$code; Output = ($lines -join "`n") }
}

function Test-PrAuthorDocsOnlyCheckpointPresence {
    <#
    .SYNOPSIS
        Return $true when a checkpoint file exists at the path, and also for a blank path.
    .DESCRIPTION
        A blank path cannot prove that no checkpoint exists, so it reports presence (fail closed).
    .PARAMETER CheckpointPath
        The absolute checkpoint path identity resolution composed.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()][AllowEmptyString()][string] $CheckpointPath
    )

    if ([string]::IsNullOrWhiteSpace($CheckpointPath)) {
        return $true
    }
    return (Test-Path -LiteralPath $CheckpointPath -PathType Leaf)
}

function Get-PrAuthorDocsOnlyBaseName {
    <#
    .SYNOPSIS
        Return the single explicit --base value of a gh pr create command, or $null.
    .DESCRIPTION
        Returns $null when the command is not a structural gh pr create; when a single-dash
        token has B or R among its letters (-B base and -R repository, alone or clustered as in
        -dB or -dR); when a token is --repo, starts with --repo=, or starts with GH_REPO=; when
        --base appears other than exactly once; when its value is missing or dash-leading; when
        the value is not a conservative branch spelling; or when it contains '..'.
    .PARAMETER CommandText
        The Bash command text under evaluation.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string] $CommandText
    )

    $subcommandPath = @('pr', 'create')
    $match = Get-CommandLineStructuralMatch -CommandText $CommandText -CommandWord 'gh' -SubcommandPath $subcommandPath
    if ($null -eq $match) {
        return $null
    }
    $baseTokenCount = 0
    foreach ($token in @($match.Segment.Tokens)) {
        $text = [string]$token
        if ($text.Length -ge 2 -and $text[0] -eq '-' -and $text[1] -ne '-' -and $text.Substring(1).IndexOfAny([char[]]@('B', 'R')) -ge 0) {
            return $null
        }
        if ($text -eq '--repo' -or $text.StartsWith('--repo=', [System.StringComparison]::OrdinalIgnoreCase) -or $text.StartsWith('GH_REPO=', [System.StringComparison]::OrdinalIgnoreCase)) {
            return $null
        }
        if ($text -eq '--base' -or $text.StartsWith('--base=', [System.StringComparison]::OrdinalIgnoreCase)) {
            $baseTokenCount++
        }
    }
    if ($baseTokenCount -ne 1) {
        return $null
    }
    $base = Get-CommandLineFlagValue -CommandText $CommandText -CommandWord 'gh' -SubcommandPath $subcommandPath -FlagName '--base'
    if ($null -eq $base -or $base -cnotmatch '^[A-Za-z0-9][A-Za-z0-9._/-]*$' -or $base.Contains('..')) {
        return $null
    }
    return $base
}

function Get-PrAuthorDocsOnlyExemption {
    <#
    .SYNOPSIS
        Decide whether a failed preflight is exempt because the branch changes only docs/.
    .DESCRIPTION
        Evaluates E1 to E7 of the file header in order and returns at the first failure.
        Each git call is wrapped so that a throwing seam denies rather than escapes.
    .PARAMETER CommandText
        The Bash command text under evaluation.
    .PARAMETER WorktreeRoot
        The resolved worktree root the diff is computed in.
    .PARAMETER CheckpointPath
        The resolved checkpoint path whose absence E2 requires.
    .OUTPUTS
        System.Collections.Specialized.OrderedDictionary with Exempt and Reason.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)][string] $CommandText,
        [AllowNull()][AllowEmptyString()][string] $WorktreeRoot,
        [AllowNull()][AllowEmptyString()][string] $CheckpointPath
    )

    if (-not (Test-CommandLineInvocation -CommandText $CommandText -CommandWord 'gh' -SubcommandPath @('pr', 'create'))) {
        return [ordered]@{ Exempt = $false; Reason = 'DOCS_ONLY_EXEMPTION_NOT_CREATE: only gh pr create is eligible.' }
    }
    if (Test-PrAuthorDocsOnlyCheckpointPresence -CheckpointPath $CheckpointPath) {
        return [ordered]@{ Exempt = $false; Reason = "DOCS_ONLY_EXEMPTION_CHECKPOINT_PRESENT: a checkpoint file exists at '$CheckpointPath', so its preflight governs." }
    }
    $base = Get-PrAuthorDocsOnlyBaseName -CommandText $CommandText
    if ($null -eq $base) {
        return [ordered]@{ Exempt = $false; Reason = 'DOCS_ONLY_EXEMPTION_BASE_UNRESOLVED: pass exactly one explicit --base <branch>; -B, -R, --repo, a single-dash shorthand cluster containing B or R, and an inline GH_REPO= prefix are not accepted.' }
    }

    $baseRef = 'refs/remotes/origin/' + $base + '^{commit}'
    try {
        $revParse = Invoke-PrAuthorDocsOnlyGitExe -WorktreeRoot $WorktreeRoot -GitArgs @('rev-parse', '--verify', '--quiet', $baseRef)
    }
    catch {
        $revParse = $null
    }
    $commit = ''
    if ($null -ne $revParse) {
        $commit = ([string]$revParse.Output).Trim()
    }
    if ($null -eq $revParse -or $revParse.ExitCode -ne 0 -or $commit -cnotmatch '^[0-9a-f]{40}([0-9a-f]{24})?$') {
        return [ordered]@{ Exempt = $false; Reason = "DOCS_ONLY_EXEMPTION_BASE_UNRESOLVED: refs/remotes/origin/$base does not resolve to a commit in the target worktree." }
    }

    try {
        $diff = Invoke-PrAuthorDocsOnlyGitExe -WorktreeRoot $WorktreeRoot -GitArgs @('diff', '--name-only', '-z', '--no-renames', '--no-color', '--no-ext-diff', '--no-relative', ($commit + '...HEAD'))
    }
    catch {
        $diff = $null
    }
    if ($null -eq $diff -or $diff.ExitCode -ne 0) {
        return [ordered]@{ Exempt = $false; Reason = "DOCS_ONLY_EXEMPTION_DIFF_FAILED: git diff against origin/$base could not be computed." }
    }

    $violation = Get-DocsOnlyDiffTextViolation -DiffText ([string]$diff.Output) -BaseName $base
    if ($violation) {
        return [ordered]@{ Exempt = $false; Reason = $violation }
    }
    return [ordered]@{ Exempt = $true; Reason = $null }
}
