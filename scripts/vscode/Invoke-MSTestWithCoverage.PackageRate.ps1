Set-StrictMode -Version Latest

function Get-CoberturaPackageLineSummary {
    <#
        .SYNOPSIS
        Reduces one Cobertura <package> element to a deduplicated line and branch summary.

        .DESCRIPTION
        Accumulates Get-CoberturaClassLineSummary over every <class> descendant of the supplied
        package and returns the same object shape Get-CoberturaCoverageSummary produces for a
        whole document, using the identical rounding and the identical '0' zero-denominator
        fallback, so a package-level rate and a document-level rate are always computed by one
        rule rather than by two that can drift apart.

        Two callers share it: Get-CoberturaCoverageSummary, which sums one summary per package
        into the document totals, and Merge-CoberturaClassesByFilename, which recomputes a
        package's line-rate and branch-rate after the merge has changed that package's class set
        (issue #733, finding 1).

        This function lives in its own file rather than alongside its callers in
        Invoke-MSTestWithCoverage.Helpers.ps1 because that file is already within a few lines of
        the repository's 500-line ceiling. Helpers.ps1 dot-sources this file, so a caller that
        dot-sources Helpers.ps1 alone still resolves this function.

        The function is pure: it performs no I/O and mutates nothing in the source document.

        .PARAMETER PackageNode
        A Cobertura <package> element. A package with no <class> descendant, or one whose classes
        carry no <lines> and no <methods>, is valid input and yields a LineRate and BranchRate of
        '0'.

        .OUTPUTS
        A pscustomobject carrying LineRate, BranchRate, LinesCovered, LinesValid, BranchesCovered
        and BranchesValid. Every value is a string, matching Get-CoberturaCoverageSummary, so a
        caller can assign it straight to an XML attribute.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [System.Xml.XmlElement]$PackageNode
    )

    $totalLines = 0
    $coveredLines = 0
    $totalBranches = 0
    $coveredBranches = 0

    foreach ($classNode in @($PackageNode.SelectNodes('.//class'))) {
        $classSummary = Get-CoberturaClassLineSummary -ClassNode $classNode
        $totalLines += $classSummary.TotalLines
        $coveredLines += $classSummary.CoveredLines
        $totalBranches += $classSummary.TotalBranches
        $coveredBranches += $classSummary.CoveredBranches
    }

    [pscustomobject]@{
        LineRate        = if ($totalLines -gt 0) { [string]([math]::Round($coveredLines / $totalLines, 6)) } else { '0' }
        BranchRate      = if ($totalBranches -gt 0) { [string]([math]::Round($coveredBranches / $totalBranches, 6)) } else { '0' }
        LinesCovered    = [string]$coveredLines
        LinesValid      = [string]$totalLines
        BranchesCovered = [string]$coveredBranches
        BranchesValid   = [string]$totalBranches
    }
}
