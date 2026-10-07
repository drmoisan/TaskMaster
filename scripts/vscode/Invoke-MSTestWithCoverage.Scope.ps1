Set-StrictMode -Version Latest

# Added for issue #928. This file holds the coverage runner's scoped-run gate: a pure predicate that
# decides whether a run is scoped, and the assertion wrapper the entry point calls once, so the whole
# gate lives in one file that is dot-sourced by path. Every test file shares one compiled copy of a
# path-loaded file, so breakpoint coverage credits every line here from any test file.

function Test-CoverageRunIsScoped {
    <#
        .SYNOPSIS
        Returns true when a coverage run's resolved search root is not the repository root.

        .DESCRIPTION
        Pure predicate; no I/O. Both paths are normalised with GetFullPath, trailing directory
        separators are trimmed, and the comparison is ordinal case-insensitive, so an omitted
        search root, a lone dot and a dot followed by a backslash all read as the repository root.
        Both inputs must be absolute: a relative path would otherwise be resolved against the
        process working directory, which is ambient state, so a relative input is rejected.

        .PARAMETER RepoRoot
        The repository root as an absolute path.

        .PARAMETER ResolvedSearchRoot
        The search root after it has been joined to the repository root, as an absolute path.

        .OUTPUTS
        True when the run is scoped, false when it is unscoped.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$RepoRoot,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$ResolvedSearchRoot
    )

    if (-not [IO.Path]::IsPathRooted($RepoRoot)) {
        throw "RepoRoot must be an absolute path: $RepoRoot"
    }

    if (-not [IO.Path]::IsPathRooted($ResolvedSearchRoot)) {
        throw "ResolvedSearchRoot must be an absolute path: $ResolvedSearchRoot"
    }

    $separators = [char[]]@([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar)
    $normalizedRepoRoot = [IO.Path]::GetFullPath($RepoRoot).TrimEnd($separators)
    $normalizedSearchRoot = [IO.Path]::GetFullPath($ResolvedSearchRoot).TrimEnd($separators)

    return -not [string]::Equals(
        $normalizedRepoRoot,
        $normalizedSearchRoot,
        [StringComparison]::OrdinalIgnoreCase)
}

function Assert-CoberturaCoverageThresholdForRun {
    <#
        .SYNOPSIS
        Enforces the document-level coverage floors on an unscoped run and skips them on a scoped run.

        .DESCRIPTION
        On a scoped run (Test-CoverageRunIsScoped returns true) writes exactly one warning naming
        the search root and the repository root and returns without asserting, because the two
        document-level assertions compare a rate taken across every instrumented assembly against
        floors set for the whole solution, and a single-assembly run fails them on a healthy tree
        (issue #928). On an unscoped run calls Assert-CoberturaLineCoverageThreshold and then
        Assert-CoberturaBranchCoverageThreshold, unchanged, in that order.

        .PARAMETER CoberturaXml
        The post-processed Cobertura document as a string.

        .PARAMETER RepoRoot
        The repository root as an absolute path.

        .PARAMETER ResolvedSearchRoot
        The search root after it has been joined to the repository root, as an absolute path.

        .OUTPUTS
        None. Throws on a failed threshold check of an unscoped run and returns nothing otherwise.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$CoberturaXml,

        [Parameter(Mandatory = $true)]
        [string]$RepoRoot,

        [Parameter(Mandatory = $true)]
        [string]$ResolvedSearchRoot
    )

    if (Test-CoverageRunIsScoped -RepoRoot $RepoRoot -ResolvedSearchRoot $ResolvedSearchRoot) {
        Write-Warning ("Coverage threshold assertions skipped: the run is scoped to search root " +
            "'$ResolvedSearchRoot' rather than the repository root '$RepoRoot'.")
        return
    }

    Assert-CoberturaLineCoverageThreshold -CoberturaXml $CoberturaXml
    Assert-CoberturaBranchCoverageThreshold -CoberturaXml $CoberturaXml
}
