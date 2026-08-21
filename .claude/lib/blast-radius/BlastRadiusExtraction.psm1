<#
.SYNOPSIS
    Blast-radius text-extraction primitives, ported from the Python reference.

.DESCRIPTION
    Destination-runtime PowerShell port of the text-scanning half of
    scripts/dev_tools/_blast_radius_extraction.py. Normalizes line endings,
    partitions atomic-plan lines, extracts backtick-delimited inline-code tokens,
    and classifies those tokens as concrete repository paths or globs.
    Get-ContractIdentifier was relocated to BlastRadiusNormalization.psm1 so this
    module stays inside the 500-line limit (issue #489); the three module-scoped
    heading variables it read travelled with it, because $script: scope is per
    module.

    The Python module remains the authoritative reference implementation. This
    module is one half of a two-language mirror; it never imports validator
    logic. Every function is pure: no filesystem, subprocess, network, or
    wall-clock access, and no input is mutated.

    Parity notes for maintainers:
      - The phase and task patterns carry the same regex text as PLAN_PHASE_RE
        and PLAN_TASK_RE in scripts/dev_tools/validate_orchestration_artifacts.py
        and in the Python extraction module. Only the named-group syntax differs
        ((?<name>...) rather than (?P<name>...)), which .NET requires.
      - Matching is case sensitive, so compiled [regex] objects are used instead
        of the case-insensitive -match operator.
      - Line normalization splits on '\r\n|\r|\n' and then drops the single
        empty element that a trailing terminator produces, which reproduces the
        list Python's str.splitlines() returns for LF, CR, and CRLF documents.
        Python additionally splits on the exotic Unicode line boundaries
        (\v, \f, \x1c-\x1e, \x85, U+2028, U+2029); this port deliberately does
        not, per the approved plan for issue #447.
      - Every returned collection is deduplicated and ordinally sorted via
        [StringComparer]::Ordinal, so identical inputs produce identical output
        in both languages regardless of the current culture.
#>

Set-StrictMode -Version Latest

# Get-OrdinalSortedEntry moved to BlastRadiusGlob.psm1, where its sibling ordinal
# primitive Get-OrdinalSmallestEntry already lives, so this module stays within
# the 500-line limit (issue #452). The import keeps every pre-existing call site
# and test source-compatible, and introduces no cycle because the Glob module
# imports no sibling.
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusGlob.psm1') -Force

# Plan-structure patterns. The regex text mirrors the Python constants so radius
# derivation and the plan validator can never disagree about which lines are
# phase headings and which are tasks.
$script:PlanPhasePattern = [regex]::new('^### Phase (?<phase>\d+) — (?<title>.+)$')
$script:PlanTaskPattern = [regex]::new(
    '^- \[(?<state>[ xX])\] \[P(?<phase>\d+)-T(?<task>\d+)\] (?<title>.+)$')

# Inline code is the only accepted source of path and contract tokens. Matching
# spans per line keeps a fenced-code opening fence, which has no closing backtick
# on its own line, from producing spurious spans.
$script:InlineCodeSpanPattern = [regex]::new('`([^`]+)`')

# Top-level directories of this repository. A token starting with one of these is
# accepted without needing a recognized extension, which admits ** globs naming a
# subtree. artifacts/ is deliberately absent (issue #489): the process-artifact
# tree is read by mandate rather than written by a work item, so admitting
# artifacts/** as a subtree claim made unrelated items contend.
$script:KnownTopLevelSegment = @(
    'scripts/', 'tests/', 'docs/', 'config/', 'schemas/', 'packages/',
    'extensions/', '.claude/', '.codex/', '.github/', '.agents/'
)

# A file citation may carry a trailing line reference such as :90. The suffix is
# stripped before the extension test so a line-anchored citation keeps the
# acceptance its unanchored form has; the token itself is recorded verbatim.
$script:LineSuffixPattern = [regex]::new(':\d+$')

# Documentation-corpus root and the index, counted after that prefix, of the
# segment that names one feature folder. A glob whose wildcard reaches this
# segment or any earlier one claims every feature folder in the corpus.
$script:FeatureCorpusPrefix = 'docs/features/'
$script:FeatureFolderSegmentIndex = 1

# Fallback acceptance rule: a token shaped <segment>/.../<name>.<ext> counts as a
# repository path when its final component carries one of these extensions.
$script:RecognizedPathExtension = [System.Collections.Generic.HashSet[string]]::new(
    [string[]] @(
        'cfg', 'cs', 'csproj', 'ini', 'js', 'json', 'jsx', 'lock', 'md', 'ps1',
        'psd1', 'psm1', 'py', 'sh', 'sln', 'toml', 'ts', 'tsx', 'txt', 'xml',
        'yaml', 'yml'
    ),
    [StringComparer]::Ordinal)

# Classification vocabulary for accepted path tokens. Concrete entries take part
# in exact-match checks; glob entries cannot and are matched by pattern.
$script:PathKindConcrete = 'concrete'
$script:PathKindGlob = 'glob'


function ConvertTo-NormalizedLine {
    <#
    .SYNOPSIS
        Split document text into lines independent of line-ending style.

    .DESCRIPTION
        Port of normalize_lines. Splits on the three ASCII line terminators and
        then drops the single trailing empty element that a terminated document
        produces, so the resulting line list matches Python's str.splitlines()
        for LF, CR, and CRLF input. Empty text yields an empty list, matching
        ''.splitlines() == [].

    .PARAMETER Text
        Full document text, possibly mixing LF, CRLF, and CR endings.

    .OUTPUTS
        System.Object[]. Lines in source order without terminators.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $Text
    )

    if ($Text.Length -eq 0) {
        return @()
    }

    $lines = [string[]]($Text -split '\r\n|\r|\n')

    # A terminated document splits into one more element than it has lines. Python
    # discards exactly that trailing empty element, so the port does too; only a
    # real terminator at the very end triggers the trim.
    if ($lines.Count -ge 2 -and $lines[-1].Length -eq 0 -and $Text -cmatch '(?:\r\n|\r|\n)\z') {
        $lines = [string[]]($lines[0..($lines.Count - 2)])
    }

    return @($lines)
}

function Get-PlanLineScan {
    <#
    .SYNOPSIS
        Partition a plan's lines into task titles, phase titles, and prose.

    .DESCRIPTION
        Port of scan_plan_lines. Classifies every normalized line exactly once.
        Task lines are tested first because a task line can never also be a phase
        heading and because task bodies are the primary path signal. A line that
        resembles a task but fails the strict pattern deliberately falls through
        to prose so its path references are still collected rather than dropped.

    .PARAMETER PlanText
        Full atomic-plan document text.

    .OUTPUTS
        System.Collections.Hashtable. Keys task_titles, phase_titles, and
        other_lines, each an array of strings in source order.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $PlanText
    )

    $taskTitle = [System.Collections.Generic.List[string]]::new()
    $phaseTitle = [System.Collections.Generic.List[string]]::new()
    $otherLine = [System.Collections.Generic.List[string]]::new()

    foreach ($line in @(ConvertTo-NormalizedLine -Text $PlanText)) {
        $taskMatch = $script:PlanTaskPattern.Match($line)
        if ($taskMatch.Success) {
            $taskTitle.Add($taskMatch.Groups['title'].Value)
            continue
        }

        $phaseMatch = $script:PlanPhasePattern.Match($line)
        if ($phaseMatch.Success) {
            $phaseTitle.Add($phaseMatch.Groups['title'].Value)
            continue
        }

        $otherLine.Add($line)
    }

    return @{
        task_titles  = [string[]]$taskTitle.ToArray()
        phase_titles = [string[]]$phaseTitle.ToArray()
        other_lines  = [string[]]$otherLine.ToArray()
    }
}

function Get-InlineCodeToken {
    <#
    .SYNOPSIS
        Extract whitespace-separated tokens from a line's inline-code spans.

    .DESCRIPTION
        Port of extract_inline_code_tokens. Strips a defensive trailing carriage
        return left when upstream text was split on newline alone rather than
        normalized, then splits each span on whitespace: a span may hold a whole
        command line rather than a single path-shaped token.

    .PARAMETER Line
        A single normalized line of a plan or spec document.

    .OUTPUTS
        System.Object[]. Tokens in source order with duplicates preserved.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $Line
    )

    $token = [System.Collections.Generic.List[string]]::new()
    foreach ($match in $script:InlineCodeSpanPattern.Matches($Line)) {
        $span = $match.Groups[1].Value
        if ($span.EndsWith("`r", [System.StringComparison]::Ordinal)) {
            $span = $span.Substring(0, $span.Length - 1)
        }

        # Mirrors Python str.split() with no argument: split on whitespace runs
        # and discard the empty fragments that leading or trailing space yields.
        foreach ($piece in ($span -split '\s+')) {
            if ($piece.Length -gt 0) {
                $token.Add($piece)
            }
        }
    }

    return @($token.ToArray())
}

function Test-MultipleFeatureFolderSpan {
    <#
    .SYNOPSIS
        Report whether a glob claims more than one documentation feature folder.

    .DESCRIPTION
        Port of spans_multiple_feature_folders. The documentation corpus is laid
        out as docs/features/<bucket>/<feature-folder>/..., so a glob whose
        wildcard occupies or truncates the feature-folder segment claims every
        feature folder in the corpus. That made two unrelated work items contend
        purely because both wrote documentation (issue #489). A glob carrying a
        complete, wildcard-free feature-folder segment claims one folder and is
        retained.

    .PARAMETER Token
        A wildcard-bearing token already accepted by the shape rules of
        Get-PathTokenKind.

    .OUTPUTS
        System.Boolean. True when the token is rooted in the documentation corpus
        and its wildcard reaches the feature-folder segment or any earlier one.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $Token
    )

    if (-not $Token.StartsWith($script:FeatureCorpusPrefix,
            [System.StringComparison]::Ordinal)) {
        return $false
    }

    $segment = @($Token.Substring($script:FeatureCorpusPrefix.Length) -split '/')

    # A token that stops at or before the feature-folder segment has had that
    # segment truncated away by the wildcard, so it spans the whole corpus.
    if ($segment.Count -le $script:FeatureFolderSegmentIndex) {
        return $true
    }

    # Every segment up to and including the feature-folder name must be a literal
    # for the claim to resolve to exactly one folder.
    for ($index = 0; $index -le $script:FeatureFolderSegmentIndex; $index++) {
        if ($segment[$index].IndexOf('*') -ge 0) {
            return $true
        }
    }

    return $false
}

function Get-PathTokenKind {
    <#
    .SYNOPSIS
        Classify an inline-code token as a concrete repository path or a glob.

    .DESCRIPTION
        Port of classify_path_token. A path reference must name a separator; a
        bare word such as a function name is a contract identifier, not a path.
        It must also be repository-relative: a leading separator marks an absolute
        path and a colon in the leading segment marks a URL scheme or a Windows
        drive. Acceptance then requires one of the two documented shape rules, a
        known top-level segment or a recognized final extension.

    .PARAMETER Token
        A single whitespace-free inline-code token.

    .PARAMETER RootSurface
        Configured separator-free repository-root shared surfaces, supplied by
        the caller from Get-ConfigRootSurface. Membership is exact and ordinal.
        The empty default reproduces pre-change behavior for every existing call
        site that omits it.

    .OUTPUTS
        System.String. 'glob' for an accepted token containing an asterisk,
        'concrete' for an accepted token without one, and $null when the token is
        not a repository path reference.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $Token,
        [Parameter(Mandatory = $false)]
        [AllowEmptyCollection()]
        [string[]] $RootSurface = @()
    )

    # A separator-free token is admitted only as an exact ordinal member of the
    # configured root-surface set (issue #452). Substring, suffix, and
    # case-insensitive comparison are all rejected: anything looser would
    # desynchronize this classifier from Resolve-BlastRadiusSharedSurface, whose
    # HashSet uses [StringComparer]::Ordinal. This runs before the separator
    # guard because a configured root surface has no separator by construction.
    foreach ($surface in $RootSurface) {
        if ([string]::Equals($Token, $surface, [System.StringComparison]::Ordinal)) {
            return $script:PathKindConcrete
        }
    }

    $separatorIndex = $Token.IndexOf('/')
    if ($separatorIndex -lt 0 -or $separatorIndex -eq 0) {
        return $null
    }
    if ($Token.Substring(0, $separatorIndex).IndexOf(':') -ge 0) {
        return $null
    }

    # Read the final component's extension for the fallback acceptance rule; a
    # component with no dot (a directory name or **) has no extension. A trailing
    # line reference is stripped first so file.md:90 reads as md rather than as
    # the unrecognized extension md:90.
    $finalComponent = $script:LineSuffixPattern.Replace(
        $Token.Substring($Token.LastIndexOf('/') + 1), '')
    $extension = ''
    $dotIndex = $finalComponent.LastIndexOf('.')
    if ($dotIndex -ge 0) {
        $extension = $finalComponent.Substring($dotIndex + 1).ToLowerInvariant()
    }

    $hasExtension = $script:RecognizedPathExtension.Contains($extension)

    # A wildcard-free token must name a file, not a directory (issue #489). A
    # directory-shaped token such as scripts/dev_tools is a location reference,
    # not a write claim, and admitting it made every item touching anything under
    # that directory contend at the path level.
    if ($Token.IndexOf('*') -lt 0) {
        if ($hasExtension) {
            return $script:PathKindConcrete
        }
        return $null
    }

    $hasKnownSegment = $false
    foreach ($segment in $script:KnownTopLevelSegment) {
        if ($Token.StartsWith($segment, [System.StringComparison]::Ordinal)) {
            $hasKnownSegment = $true
            break
        }
    }

    # A wildcard-bearing token must still satisfy one of the two documented shape
    # rules; failing both means the token is prose or a non-path expression that
    # merely contains a separator, so it is dropped.
    if (-not $hasKnownSegment -and -not $hasExtension) {
        return $null
    }

    # A documentation glob spanning the whole feature corpus is a cross-corpus
    # claim rather than a write claim, so it is dropped before it can become a
    # radius entry.
    if (Test-MultipleFeatureFolderSpan -Token $Token) {
        return $null
    }

    # An accepted token carrying a wildcard names a set of files, so it cannot
    # take part in concrete exact-match comparisons and is recorded as a glob.
    return $script:PathKindGlob
}

function Get-PathFromLine {
    <#
    .SYNOPSIS
        Collect accepted path and glob tokens from already-normalized lines.

    .DESCRIPTION
        Port of extract_paths_from_lines. Shared by plan and spec extraction so
        both apply identical acceptance rules. Duplicated citations of a path
        collapse before the ordinal sort fixes the deterministic output order.

    .PARAMETER Line
        Normalized document lines to scan. An empty collection is accepted.

    .PARAMETER RootSurface
        Configured separator-free root surfaces, forwarded unchanged to
        Get-PathTokenKind. The empty default reproduces pre-change behavior.

    .OUTPUTS
        System.Object[]. Accepted tokens, deduplicated and ordinally sorted.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]] $Line,
        [Parameter(Mandatory = $false)]
        [AllowEmptyCollection()]
        [string[]] $RootSurface = @()
    )

    $accepted = [System.Collections.Generic.List[string]]::new()
    foreach ($single in $Line) {
        foreach ($token in @(Get-InlineCodeToken -Line $single)) {
            if ($null -ne (Get-PathTokenKind -Token $token -RootSurface $RootSurface)) {
                $accepted.Add($token)
            }
        }
    }

    return @(Get-OrdinalSortedEntry -Entry $accepted.ToArray())
}

function Get-PlanPaths {
    <#
    .SYNOPSIS
        Extract repository path references from an atomic plan.

    .DESCRIPTION
        Port of extract_plan_paths, the single extraction function shared by
        radius derivation and validation rule V1. Sharing it guarantees that a
        radius derived from plan P always passes V1 against P, leaving V1's force
        against hand-edited or stale declared radii and planner drift. Task bodies
        are the primary signal, but phase headings and remaining prose are scanned
        too because plans cite paths in phase preambles, guardrail clauses, and
        evidence clauses.

    .PARAMETER PlanText
        Full atomic-plan document text; may be empty.

    .PARAMETER RootSurface
        Configured separator-free root surfaces, forwarded unchanged to
        Get-PathFromLine. The empty default reproduces pre-change behavior.

    .OUTPUTS
        System.Object[]. Concrete paths and globs cited in inline code,
        deduplicated and ordinally sorted.
    #>
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseSingularNouns', '', Justification = 'The exported name is fixed by the spec PowerShell surface contract for issue #447 and mirrors extract_plan_paths.')]
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $PlanText,
        [Parameter(Mandatory = $false)]
        [AllowEmptyCollection()]
        [string[]] $RootSurface = @()
    )

    $scan = Get-PlanLineScan -PlanText $PlanText
    $allLine = [System.Collections.Generic.List[string]]::new()
    $allLine.AddRange([string[]]$scan['task_titles'])
    $allLine.AddRange([string[]]$scan['phase_titles'])
    $allLine.AddRange([string[]]$scan['other_lines'])

    return @(Get-PathFromLine -Line $allLine.ToArray() -RootSurface $RootSurface)
}

Export-ModuleMember -Function `
    Get-OrdinalSortedEntry, `
    ConvertTo-NormalizedLine, `
    Get-PlanLineScan, `
    Get-InlineCodeToken, `
    Test-MultipleFeatureFolderSpan, `
    Get-PathTokenKind, `
    Get-PathFromLine, `
    Get-PlanPaths
