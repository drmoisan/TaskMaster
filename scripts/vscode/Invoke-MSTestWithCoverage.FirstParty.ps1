Set-StrictMode -Version Latest

function Get-CoberturaFirstPartyCoverageSummary {
    <#
        .SYNOPSIS
        Aggregates a Cobertura document into deduplicated first-party line and branch totals.

        .DESCRIPTION
        Selects /coverage/packages/package, skips any package whose name is outside ProjectNames,
        and accumulates Get-CoberturaPackageLineSummary over every retained package.

        The counting invariant this function establishes is that each (class, source line number) pair is counted exactly once, so LinesValid, LinesCovered, BranchesValid and BranchesCovered do not depend on whether a source line also appears under a method element.

        The de-duplication rule itself is not re-derived here. Per-class figures are obtained by
        calling Get-CoberturaPackageLineSummary, which reduces each class through
        Get-CoberturaClassLineSummary, so exactly one implementation of the counting rule exists
        and every caller applies that same one (issue #815).

        Get-CoberturaCoverageSummary sums every package unconditionally and offers no allowlist,
        which is safe only on a post-processed document. This function accepts raw collector output
        as well, which still carries test and third-party packages, so it applies the allowlist
        itself rather than assuming a caller has already stripped them.

        This function lives in its own file rather than alongside its callers in
        Invoke-MSTestWithCoverage.Helpers.ps1 because that file is already within a few lines of
        the repository's 500-line ceiling. Helpers.ps1 dot-sources this file, so a caller that
        dot-sources Helpers.ps1 alone still resolves this function.

        The function is pure: it performs no I/O and mutates nothing in the source document.

        .PARAMETER XmlDocument
        A loaded Cobertura document. A document carrying no <packages> node is rejected with the
        same message Get-CoberturaCoverageSummary uses, so one rejection has one wording.

        .PARAMETER ProjectNames
        The first-party package names to retain. Defaults to Get-KoverageProjectAllowlist, which
        derives the names from the tracked project files, so the allowlist stays correct when a
        project is added or renamed. Tests supply an explicit override.

        .OUTPUTS
        A pscustomobject carrying LineRate, BranchRate, LinesCovered, LinesValid, BranchesCovered
        and BranchesValid in the same string shape Get-CoberturaCoverageSummary produces, using the
        identical rounding and the identical '0' zero-denominator fallback, plus LinePercent and
        BranchPercent as two-decimal invariant-culture strings for direct quotation in an artifact.
        The invariant culture is pinned because the rendered text is asserted character for
        character by a test, and the current culture's decimal separator varies by machine.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [xml]$XmlDocument,

        [Parameter(Mandatory = $false)]
        [string[]]$ProjectNames = (Get-KoverageProjectAllowlist)
    )

    $packagesNode = $XmlDocument.SelectSingleNode('//packages')
    if (-not $packagesNode) {
        throw 'Cobertura XML does not contain a <packages> node.'
    }

    $invariant = [System.Globalization.CultureInfo]::InvariantCulture
    $totalLines = 0
    $coveredLines = 0
    $totalBranches = 0
    $coveredBranches = 0

    foreach ($packageNode in @($XmlDocument.SelectNodes('/coverage/packages/package'))) {
        # GetAttribute is used rather than bare property access because Set-StrictMode -Version
        # Latest makes a missing XML attribute throw instead of returning $null.
        if ($ProjectNames -notcontains $packageNode.GetAttribute('name')) {
            continue
        }

        $packageSummary = Get-CoberturaPackageLineSummary -PackageNode $packageNode
        $totalLines += [int]$packageSummary.LinesValid
        $coveredLines += [int]$packageSummary.LinesCovered
        $totalBranches += [int]$packageSummary.BranchesValid
        $coveredBranches += [int]$packageSummary.BranchesCovered
    }

    [pscustomobject]@{
        LineRate        = if ($totalLines -gt 0) { [string]([math]::Round($coveredLines / $totalLines, 6)) } else { '0' }
        BranchRate      = if ($totalBranches -gt 0) { [string]([math]::Round($coveredBranches / $totalBranches, 6)) } else { '0' }
        LinesCovered    = [string]$coveredLines
        LinesValid      = [string]$totalLines
        BranchesCovered = [string]$coveredBranches
        BranchesValid   = [string]$totalBranches
        LinePercent     = if ($totalLines -gt 0) { ([double](100 * $coveredLines / $totalLines)).ToString('0.00', $invariant) } else { '0.00' }
        BranchPercent   = if ($totalBranches -gt 0) { ([double](100 * $coveredBranches / $totalBranches)).ToString('0.00', $invariant) } else { '0.00' }
    }
}

function Format-CoberturaFirstPartyCoverageSummary {
    <#
        .SYNOPSIS
        Renders a first-party coverage summary as the single line the entry point prints.

        .DESCRIPTION
        Pure formatting, separated from both the aggregation and the coverage run so the reported
        text is assertable in a unit test without invoking a coverage run. It performs no I/O.

        .PARAMETER Summary
        The object Get-CoberturaFirstPartyCoverageSummary returns.

        .OUTPUTS
        One string carrying the four counts and both derived percentages.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [pscustomobject]$Summary
    )

    $lineText = "lines $($Summary.LinesCovered)/$($Summary.LinesValid) ($($Summary.LinePercent)%)"
    $branchText = "branches $($Summary.BranchesCovered)/$($Summary.BranchesValid) ($($Summary.BranchPercent)%)"

    return "First-party coverage: $lineText, $branchText"
}

function Get-CoberturaFirstPartyCoverageReport {
    <#
        .SYNOPSIS
        Aggregates a Cobertura string and returns the rendered first-party coverage line.

        .DESCRIPTION
        Composes Get-CoberturaFirstPartyCoverageSummary with
        Format-CoberturaFirstPartyCoverageSummary. It exists so that the wiring added to
        Invoke-MSTestWithCoverageMain is exactly one line: that call site sits after the early
        return no unit test can reach, so every line placed there is uncovered by construction,
        and keeping the computation and the rendering in unit-tested pure functions holds the
        uncovered surface to that single line.

        The function is pure: it performs no I/O.

        .PARAMETER CoberturaXml
        A Cobertura document as a string, matching the parameter shape
        Assert-CoberturaLineCoverageThreshold already accepts at the same call site.

        .PARAMETER ProjectNames
        The first-party package names to retain. Defaults to Get-KoverageProjectAllowlist.

        .OUTPUTS
        One string carrying the four counts and both derived percentages.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [string]$CoberturaXml,

        [Parameter(Mandatory = $false)]
        [string[]]$ProjectNames = (Get-KoverageProjectAllowlist)
    )

    [xml]$document = $CoberturaXml
    $summary = Get-CoberturaFirstPartyCoverageSummary -XmlDocument $document -ProjectNames $ProjectNames

    return Format-CoberturaFirstPartyCoverageSummary -Summary $summary
}
