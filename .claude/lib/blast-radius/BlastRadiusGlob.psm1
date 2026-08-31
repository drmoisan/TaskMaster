<#
.SYNOPSIS
    Blast-radius glob, subsumption, and overlap primitives.

.DESCRIPTION
    Destination-runtime PowerShell port of the path-pattern primitives the Python
    reference splits across scripts/dev_tools/_blast_radius_extraction.py
    (_glob_to_regex_text, matches_glob, is_path_subsumed),
    scripts/dev_tools/_blast_radius_validation.py (is_glob_entry,
    concrete_entries) and scripts/dev_tools/_blast_radius_conflicts.py
    (_literal_prefix, _entries_overlap). They are gathered here because they form
    one cohesive concern, pattern comparison over repository paths, and because
    every PowerShell file must stay within the 500-line limit.

    The Python modules remain the authoritative reference implementation. This
    module is one half of a two-language mirror; it never imports validator
    logic. Every function is pure: no filesystem, subprocess, network, or
    wall-clock access, and no input is mutated.

    Parity notes for maintainers:
      - The glob vocabulary is a deliberate fnmatch subset (**, *, ?). Character
        classes are unsupported because PowerShell's -like operator does not
        agree with fnmatch on their semantics, which is also why this module
        translates patterns to regex explicitly rather than using -like.
      - [regex]::Escape and Python's re.escape escape different punctuation sets,
        but every character either escapes to a literal or is already literal
        outside a character class, so the translated patterns match the same
        strings.
      - Full-match anchoring uses \A and \z, which reproduce Python's
        re.fullmatch exactly; ^ and $ would additionally admit a trailing
        newline in .NET.
      - Comparisons and ordering use [StringComparer]::Ordinal and
        [string]::CompareOrdinal so results do not vary with the current culture.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Wildcards that make a path entry a pattern rather than a file. The question
# mark is included because the subsumption helper treats it as a pattern;
# admitting it here keeps every comparison in the fail-closed direction.
$script:GlobWildcard = @('*', '?')


function Test-GlobEntry {
    <#
    .SYNOPSIS
        Report whether a path entry is a wildcard pattern rather than a file.

    .DESCRIPTION
        Port of is_glob_entry. Used both to classify radius path entries and, one
        character at a time, by the literal-prefix helper.

    .PARAMETER Entry
        A paths entry from a radius or an extraction, or a single character.

    .OUTPUTS
        System.Boolean. True when the entry carries any wildcard character.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $Entry
    )

    foreach ($wildcard in $script:GlobWildcard) {
        if ($Entry.IndexOf($wildcard, [System.StringComparison]::Ordinal) -ge 0) {
            return $true
        }
    }

    return $false
}

function Get-ConcreteEntry {
    <#
    .SYNOPSIS
        Select the wildcard-free entries of a path collection.

    .DESCRIPTION
        Port of concrete_entries. Only concrete entries can be compared for
        equality, so the rules that count files or enumerate surfaces use this
        subset. Input order is preserved, which is already ordinal for any radius
        or extraction result.

    .PARAMETER Entry
        Entries mixing concrete paths and globs. An empty collection is accepted.

    .OUTPUTS
        System.Object[]. The concrete entries in input order.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]] $Entry
    )

    $concrete = [System.Collections.Generic.List[string]]::new()
    foreach ($single in $Entry) {
        if (-not (Test-GlobEntry -Entry $single)) {
            $concrete.Add($single)
        }
    }

    return @($concrete.ToArray())
}

# Port of _glob_to_regex_text. Scans one character at a time so the two-character
# ** token is recognized before the single-character * rule applies; the order
# matters because only ** may cross directory separators. Every other character,
# including [ and ], is escaped to a literal.
function ConvertTo-GlobRegexText {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $Pattern
    )

    $part = [System.Text.StringBuilder]::new()
    $index = 0
    while ($index -lt $Pattern.Length) {
        if ($index + 1 -lt $Pattern.Length -and $Pattern[$index] -ceq '*' -and $Pattern[$index + 1] -ceq '*') {
            [void]$part.Append('.*')
            $index += 2
            continue
        }

        $character = $Pattern[$index]
        if ($character -ceq '*') {
            [void]$part.Append('[^/]*')
        } elseif ($character -ceq '?') {
            [void]$part.Append('[^/]')
        } else {
            [void]$part.Append([regex]::Escape([string]$character))
        }
        $index += 1
    }

    return $part.ToString()
}

function Test-GlobMatch {
    <#
    .SYNOPSIS
        Report whether a candidate path matches a glob pattern.

    .DESCRIPTION
        Port of matches_glob. Translates the supported glob subset to regex and
        applies it as a whole-string match, reproducing Python's re.fullmatch.

    .PARAMETER Pattern
        Glob using the supported **, *, ? vocabulary.

    .PARAMETER Candidate
        Concrete repository-relative path to test.

    .OUTPUTS
        System.Boolean. True when the whole candidate matches the whole pattern.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $Pattern,
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $Candidate
    )

    $regexText = '\A(?:' + (ConvertTo-GlobRegexText -Pattern $Pattern) + ')\z'
    return [regex]::IsMatch($Candidate, $regexText)
}

function Test-PathSubsumed {
    <#
    .SYNOPSIS
        Report whether a concrete path is covered by a collection of entries.

    .DESCRIPTION
        Port of is_path_subsumed. Implements the coverage relation validation rule
        V1 applies: exact match, listed-directory prefix, or glob match. The three
        rules are independent, so traversal order affects speed only, never the
        verdict. An empty collection covers nothing.

    .PARAMETER Path
        Concrete repository-relative path to test.

    .PARAMETER CoveringPath
        Declared path entries, which may mix concrete paths, directory names, and
        glob patterns. An empty collection is accepted.

    .OUTPUTS
        System.Boolean. True when at least one entry covers the path.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $Path,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]] $CoveringPath
    )

    foreach ($entry in $CoveringPath) {
        if ([string]::Equals($entry, $Path, [System.StringComparison]::Ordinal)) {
            return $true
        }

        # A wildcard entry is a pattern matched with the shared glob subset. A
        # wildcard-free entry cannot be a pattern, so it is treated as a listed
        # directory covering everything beneath it.
        if (Test-GlobEntry -Entry $entry) {
            if (Test-GlobMatch -Pattern $entry -Candidate $Path) {
                return $true
            }
        } elseif ($Path.StartsWith($entry.TrimEnd('/') + '/', [System.StringComparison]::Ordinal)) {
            return $true
        }
    }

    return $false
}

function Get-LiteralPrefix {
    <#
    .SYNOPSIS
        Return the leading portion of a path entry before its first wildcard.

    .DESCRIPTION
        Port of _literal_prefix. Scanning for the earliest wildcard of any kind
        keeps the prefix a true literal, which is what makes the glob-versus-glob
        disjointness test sound. The trailing return is the defensive
        wildcard-free fallback the Python reference carries; the overlap relation
        only reaches this helper with glob entries, so that branch is exercised
        by direct invocation.

    .PARAMETER Entry
        A path entry that may contain wildcards.

    .OUTPUTS
        System.String. The literal prefix; the whole entry when it has no
        wildcard.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $Entry
    )

    for ($index = 0; $index -lt $Entry.Length; $index++) {
        if (Test-GlobEntry -Entry ([string]$Entry[$index])) {
            return $Entry.Substring(0, $index)
        }
    }

    return $Entry
}

function Test-EntryOverlap {
    <#
    .SYNOPSIS
        Report whether two path entries can name a common file.

    .DESCRIPTION
        Port of _entries_overlap. The cases are decided by how many sides are
        patterns: two concrete entries overlap when equal or when either names a
        directory containing the other, a mixed pair overlaps on a pattern match
        or on a two-way nest between the glob's literal prefix and the concrete
        entry's directory, and a pattern pair falls back to a conservative
        literal-prefix proof. The two directory rules were added by issue #452 to
        align this relation with Test-PathSubsumed, which already honoured them.
        Glob-versus-glob containment is undecidable in
        general, so that pair overlaps unless the prefixes diverge, which no
        single path could satisfy. Any pair the test cannot separate is reported
        as overlapping, the fail-closed direction.

    .PARAMETER EntryA
        First path entry, concrete or glob.

    .PARAMETER EntryB
        Second path entry, concrete or glob.

    .OUTPUTS
        System.Boolean. True when the entries overlap; the relation is symmetric.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $EntryA,
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $EntryB
    )

    $aIsGlob = Test-GlobEntry -Entry $EntryA
    $bIsGlob = Test-GlobEntry -Entry $EntryB

    if (-not $aIsGlob -and -not $bIsGlob) {
        # Anchoring each entry with a trailing separator before the prefix test is
        # what keeps the containment sound: without the anchor, scripts/dev_tools
        # would appear to contain scripts/dev_toolsX/a.py. Trimming first makes a
        # trailing separator on either entry immaterial.
        $directoryA = $EntryA.TrimEnd('/') + '/'
        $directoryB = $EntryB.TrimEnd('/') + '/'
        return ([string]::Equals($EntryA, $EntryB, [System.StringComparison]::Ordinal) -or
            $EntryA.StartsWith($directoryB, [System.StringComparison]::Ordinal) -or
            $EntryB.StartsWith($directoryA, [System.StringComparison]::Ordinal))
    }
    # A mixed pair also overlaps when the glob's literal prefix and the concrete
    # entry's directory prefix nest. The nest is tested in both directions because
    # the glob may be rooted above the directory (scripts/ above
    # scripts/dev_tools/) or below it, and either arrangement admits a common file.
    if ($aIsGlob -and -not $bIsGlob) {
        $prefixGlob = Get-LiteralPrefix -Entry $EntryA
        $directoryConcrete = $EntryB.TrimEnd('/') + '/'
        return ((Test-GlobMatch -Pattern $EntryA -Candidate $EntryB) -or
            $prefixGlob.StartsWith($directoryConcrete, [System.StringComparison]::Ordinal) -or
            $directoryConcrete.StartsWith($prefixGlob, [System.StringComparison]::Ordinal))
    }
    if ($bIsGlob -and -not $aIsGlob) {
        $prefixGlob = Get-LiteralPrefix -Entry $EntryB
        $directoryConcrete = $EntryA.TrimEnd('/') + '/'
        return ((Test-GlobMatch -Pattern $EntryB -Candidate $EntryA) -or
            $prefixGlob.StartsWith($directoryConcrete, [System.StringComparison]::Ordinal) -or
            $directoryConcrete.StartsWith($prefixGlob, [System.StringComparison]::Ordinal))
    }

    $prefixA = Get-LiteralPrefix -Entry $EntryA
    $prefixB = Get-LiteralPrefix -Entry $EntryB
    return ($prefixA.StartsWith($prefixB, [System.StringComparison]::Ordinal) -or
        $prefixB.StartsWith($prefixA, [System.StringComparison]::Ordinal))
}

function Get-OrdinalSortedEntry {
    <#
    .SYNOPSIS
        Deduplicate and ordinally sort a string collection.

    .DESCRIPTION
        Port of the tuple(sorted(set(...))) idiom the Python reference applies to
        every collection it returns. Ordinal comparison is mandatory: PowerShell's
        default Sort-Object is culture sensitive and would order entries
        differently from Python's code-point ordering on some hosts.

    .PARAMETER Entry
        The entries to normalize. An empty collection is accepted and yields an
        empty array.

    .OUTPUTS
        System.Object[]. The distinct entries in ordinal order.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]] $Entry
    )

    $unique = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach ($item in $Entry) {
        [void]$unique.Add($item)
    }

    $sorted = [System.Collections.Generic.List[string]]::new($unique)
    $sorted.Sort([StringComparer]::Ordinal)
    return @($sorted.ToArray())
}

function Get-OrdinalSmallestEntry {
    <#
    .SYNOPSIS
        Return the ordinally smallest entry of a collection.

    .DESCRIPTION
        Port of the Python min() calls in the contention relation. Ordinal
        comparison is mandatory so the reported detail does not vary with the
        current culture.

    .PARAMETER Entry
        Candidate entries. An empty collection is accepted and yields $null,
        mirroring the Python guards that return None for an empty candidate set.

    .OUTPUTS
        System.String. The smallest entry, or $null when the collection is empty.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]] $Entry
    )

    $smallest = $null
    foreach ($candidate in $Entry) {
        if ($null -eq $smallest -or [string]::CompareOrdinal($candidate, $smallest) -lt 0) {
            $smallest = $candidate
        }
    }

    return $smallest
}

Export-ModuleMember -Function `
    Test-GlobEntry, `
    Get-ConcreteEntry, `
    Test-GlobMatch, `
    Test-PathSubsumed, `
    Get-LiteralPrefix, `
    Test-EntryOverlap, `
    Get-OrdinalSmallestEntry, `
    Get-OrdinalSortedEntry
