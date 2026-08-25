<#
.SYNOPSIS
    Blast-radius token-shape predicates, ported from the Python reference.

.DESCRIPTION
    Destination-runtime PowerShell port of
    scripts/dev_tools/_blast_radius_token_shapes.py. Holds the pure,
    context-free shape tests that reject an inline-code token before
    Get-PathTokenKind can record it as a repository path. A token can look like
    a path and still name no file: a placeholder or interpolation marker makes it
    a command or artifact shape, and a corpus-wide documentation glob makes it a
    cross-corpus claim. Neither is evidence that a work item will write anything.

    Test-PlaceholderMarker is new in issue #502.
    Test-MultipleFeatureFolderSpan was relocated here from
    BlastRadiusExtraction.psm1 in the same change, and the two module-scoped
    documentation-corpus variables it reads travelled with it because $script:
    scope is per module. The relocation exists because the extraction module had
    two lines of headroom against the 500-line limit when the marker guard was
    added, so an in-place guard was arithmetically impossible.

    The Python module remains the authoritative reference implementation. This
    module is a LEAF: it imports no sibling blast-radius module, so the
    extraction module can import it with no possibility of a cycle. That
    constraint is why the predicate does not live in BlastRadiusNormalization.psm1,
    which already imports the extraction module.

    Every function is pure: no filesystem, subprocess, network, or wall-clock
    access, and no input is mutated.

    Parity notes for maintainers:
      - The marker array is character-identical to PLACEHOLDER_MARKERS in
        scripts/dev_tools/_blast_radius_token_shapes.py and to the tuple of the
        same name in scripts/dev_tools/plan_gate_coverage.py, whose origin is
        the checkable-literal placeholder guard recorded in
        .claude/rules/plan-acceptance-gates.md. The marker set is a module
        constant, not a truth-table key: it describes what a path can never
        contain rather than a policy a repository could tune.
      - Every marker literal is single-quoted, and the two dollar forms are
        built by character concatenation. A double-quoted PowerShell string
        expands the subexpression form and the delimited-variable form, so a
        double-quoted marker literal would silently define a DIFFERENT
        vocabulary than the Python reference and the parity would fail on
        exactly the two members that matter most.
      - Substring search uses [System.StringComparison]::Ordinal so the result
        is culture-independent and matches Python's byte-wise 'in' operator.
      - Both predicates are total on every string, including the empty string, a
        token consisting only of a marker, and a bare bracket pair. Neither
        throws for any input, because the classifier that calls them runs over
        every inline-code span in a document and a throw would abort an entire
        derivation over one stray span.
#>

Set-StrictMode -Version Latest

# Placeholder and interpolation markers. A token carrying any of these was
# written to document a shape, not to name a file, so it can never be a write
# claim.
#
# The angle brackets are the dominant corpus shape and are also the strongest
# case: Windows forbids both characters in a filename outright, so an
# angle-bracketed token cannot name a file on the platform this repository is
# developed on. The two dollar forms are shell and PowerShell interpolation, and
# the percent form is the Windows shell's environment-variable syntax; each
# resolves at run time to text that is not in the token.
#
# The two dollar forms are assembled from single characters deliberately. Written
# as one single-quoted literal they would be correct, but assembling them makes
# the intent explicit at the definition site and removes any question of what a
# future edit to the quoting style would do.
$script:PlaceholderMarker = [string[]]@(
    '<',
    '>',
    '$' + '{',
    '$' + '(',
    '%'
)

# Documentation-corpus root and the index, counted after that prefix, of the
# segment that names one feature folder. A glob whose wildcard reaches this
# segment or any earlier one claims every feature folder in the corpus.
$script:FeatureCorpusPrefix = 'docs/features/'
$script:FeatureFolderSegmentIndex = 1

function Test-PlaceholderMarker {
    <#
    .SYNOPSIS
        Report whether a token carries a placeholder or interpolation marker.

    .DESCRIPTION
        Port of contains_placeholder_marker. A marker-bearing token documents a
        shape rather than naming a file. Two work items that cite the same
        mandated artifact shape therefore acquired a path-level conflict edge on
        a string that resolves to nothing, which made thematically unrelated
        items contend and serialized runs that had no reason to serialize
        (issue #502).

        The test is a plain substring scan over a fixed vocabulary, deliberately
        context-free: it needs no repository lookup, no configuration, and no
        knowledge of which segment the marker sits in. A marker anywhere in the
        token is disqualifying, including in the filename position, because an
        interpolated filename is as unresolvable as an interpolated directory.

    .PARAMETER Token
        A single whitespace-free inline-code token. The empty string is accepted
        and reports false.

    .OUTPUTS
        System.Boolean. True when any configured marker appears anywhere in the
        token, otherwise false.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $Token
    )

    # Ordinal comparison keeps the result culture-independent and byte-wise, so
    # it agrees with Python's 'in' operator on every input.
    foreach ($marker in $script:PlaceholderMarker) {
        if ($Token.IndexOf($marker, [System.StringComparison]::Ordinal) -ge 0) {
            return $true
        }
    }

    return $false
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

Export-ModuleMember -Function `
    Test-PlaceholderMarker, `
    Test-MultipleFeatureFolderSpan
