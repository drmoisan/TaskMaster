Set-StrictMode -Version Latest

# Added for issue #928. This file is pure: the predicate below performs no filesystem or process
# I/O. It is dot-sourced by the entry point rather than through the helpers chain, because adding a
# line to the helpers file would put a fourth production file in this change, over the three-file
# cap.

function Test-CoverageRunIsScoped {
    <#
        .SYNOPSIS
        Returns true when a coverage run's resolved search root is not the repository root.

        .DESCRIPTION
        Pure predicate; no I/O. Both paths are normalised with GetFullPath, trailing directory
        separators are trimmed, and the comparison is ordinal case-insensitive, so an omitted
        search root, a lone dot and a dot followed by a backslash all read as the repository root.
        The entry point skips the document-level threshold assertions when this returns true,
        because those assertions compare a rate taken across every instrumented assembly against
        floors set for the whole solution, and a single-assembly run fails them on a healthy
        tree (issue #928).

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
        [string]$RepoRoot,

        [Parameter(Mandatory = $true)]
        [string]$ResolvedSearchRoot
    )

    $separators = [char[]]@([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar)
    $normalizedRepoRoot = [IO.Path]::GetFullPath($RepoRoot).TrimEnd($separators)
    $normalizedSearchRoot = [IO.Path]::GetFullPath($ResolvedSearchRoot).TrimEnd($separators)

    return -not [string]::Equals(
        $normalizedRepoRoot,
        $normalizedSearchRoot,
        [StringComparison]::OrdinalIgnoreCase)
}
