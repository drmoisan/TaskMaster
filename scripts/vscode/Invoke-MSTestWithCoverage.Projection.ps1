Set-StrictMode -Version Latest

# This is a new part file rather than an addition to Invoke-MSTestWithCoverage.Helpers.ps1 because
# that file measures 470 content lines and therefore has only thirty lines of headroom against the
# repository's 500-line ceiling, which the three functions below would exceed on their own.
# Invoke-MSTestWithCoverage.Helpers.ps1 dot-sources this file, so a caller that dot-sources the
# helpers file alone still resolves every function declared here.
#
# Every function in this file is pure: none performs filesystem or process I/O, and none mutates the
# document it is handed. The conditional discard of the raw collector document is deliberately split
# into a predicate here and the discard itself at the entry point, so the decision is unit-testable
# without any file existing.

function ConvertTo-JacocoPackageProjection {
    <#
        .SYNOPSIS
        Projects a post-processed Cobertura document onto the package-level JaCoCo report shape.

        .DESCRIPTION
        Emits the committed-evidence projection: a root report element carrying a single name
        attribute, one package child per source package in document order, and exactly two counter
        children per package in the order LINE then BRANCH. No class, method, source-file or line
        element survives the projection, and no INSTRUCTION, METHOD or CLASS counter is emitted, so
        the result carries aggregate figures only and cannot leak a source path.

        The per-package counting rule is not re-derived here. Each package's covered and valid
        figures come from Get-CoberturaPackageLineSummary, which reduces every class through the one
        de-duplicating summariser the repository already has, so exactly one implementation of the
        counting rule exists and every caller applies that same one (issue #815). JaCoCo reports
        missed rather than valid, so missed is computed as valid minus covered, independently for
        lines and for branches.

        The function is pure: it performs no I/O and mutates nothing in the source document.

        .PARAMETER XmlDocument
        A loaded Cobertura document, normally the post-processed one rather than raw collector
        output. A document carrying no packages node is rejected with the wording
        Get-CoberturaFirstPartyCoverageSummary and Get-CoberturaCoverageSummary already use, so one
        rejection has one wording.

        .OUTPUTS
        One string holding the serialised projection, two-space indented per level, with a space
        before each self-closing slash, no XML declaration and no DOCTYPE. The string carries no
        trailing newline; the caller that persists it supplies the file's final newline.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [xml]$XmlDocument
    )

    $packagesNode = $XmlDocument.SelectSingleNode('//packages')
    if (-not $packagesNode) {
        throw 'Cobertura XML does not contain a <packages> node.'
    }

    $projectionLines = [System.Collections.Generic.List[string]]::new()
    $projectionLines.Add('<report name="TaskMaster">')

    foreach ($packageNode in @($packagesNode.SelectNodes('./package'))) {
        $packageSummary = Get-CoberturaPackageLineSummary -PackageNode $packageNode
        $coveredLines = [int]$packageSummary.LinesCovered
        $validLines = [int]$packageSummary.LinesValid
        $coveredBranches = [int]$packageSummary.BranchesCovered
        $validBranches = [int]$packageSummary.BranchesValid

        # GetAttribute is used rather than bare property access because Set-StrictMode -Version
        # Latest makes a missing XML attribute throw instead of returning $null.
        $packageName = $packageNode.GetAttribute('name')

        $projectionLines.Add('  <package name="' + $packageName + '">')
        $projectionLines.Add('    <counter type="LINE" missed="' + ($validLines - $coveredLines) + '" covered="' + $coveredLines + '" />')
        $projectionLines.Add('    <counter type="BRANCH" missed="' + ($validBranches - $coveredBranches) + '" covered="' + $coveredBranches + '" />')
        $projectionLines.Add('  </package>')
    }

    $projectionLines.Add('</report>')

    return ($projectionLines -join [System.Environment]::NewLine)
}

function Assert-JacocoProjectionReconciliation {
    <#
        .SYNOPSIS
        Throws unless the projection's summed LINE figures equal the source document's root totals.

        .DESCRIPTION
        The projection is committed as evidence in place of the document it summarises, so a silent
        arithmetic divergence between the two would be undetectable after the raw document is
        discarded. This assertion closes that gap: it sums the projection's per-package LINE
        counters and requires the covered total to equal the source root lines-covered attribute and
        the missed-plus-covered total to equal the source root lines-valid attribute. Both
        comparisons are exact; no tolerance is applied.

        The thrown message names the expected total and the observed total so a failure is
        diagnosable from the message alone.

        The function is pure: it performs no I/O and mutates nothing in either document.

        .PARAMETER XmlDocument
        The source Cobertura document whose root carries lines-covered and lines-valid.

        .PARAMETER ProjectionXml
        The serialised projection ConvertTo-JacocoPackageProjection returned.

        .OUTPUTS
        None. The function throws on a failed reconciliation and returns nothing otherwise.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [xml]$XmlDocument,

        [Parameter(Mandatory = $true)]
        [string]$ProjectionXml
    )

    $coverageNode = $XmlDocument.SelectSingleNode('/coverage')
    if (-not $coverageNode) {
        throw 'Cobertura XML has no root <coverage> element, so the projection cannot be reconciled.'
    }

    [xml]$projectionDocument = $ProjectionXml
    $observedCovered = 0
    $observedMissed = 0

    foreach ($counterNode in @($projectionDocument.SelectNodes('/report/package/counter[@type="LINE"]'))) {
        $observedCovered += [int]$counterNode.GetAttribute('covered')
        $observedMissed += [int]$counterNode.GetAttribute('missed')
    }

    $expectedCovered = [int]$coverageNode.GetAttribute('lines-covered')
    $expectedValid = [int]$coverageNode.GetAttribute('lines-valid')
    $observedValid = $observedMissed + $observedCovered

    if ($observedCovered -ne $expectedCovered) {
        throw ('JaCoCo projection LINE covered total does not reconcile: expected ' +
            $expectedCovered + ', observed ' + $observedCovered + '.')
    }

    if ($observedValid -ne $expectedValid) {
        throw ('JaCoCo projection LINE valid total does not reconcile: expected ' +
            $expectedValid + ', observed ' + $observedValid + '.')
    }
}

function Test-RawCoverageDocumentRetained {
    <#
        .SYNOPSIS
        Decides whether the raw collector document at the supplied output path is kept.

        .DESCRIPTION
        The raw document is retained only when it was written into the repository coverage
        directory itself, which is already ignored by this repository, so no raw document can be
        staged. Any other destination is discarded once the projection and the reconciliation have
        completed, because a raw document outside the ignored tree is a candidate for accidental
        commit and carries absolute source paths.

        The comparison is an equality between the output path's full parent directory and the
        repository root joined with coverage, case-insensitive with a trailing separator trimmed.
        Equality rather than containment is deliberate: a subdirectory of the coverage tree is an
        externally supplied destination, so it is discarded.

        The function is pure: it performs no I/O and touches no file. Split-Path and Join-Path are
        path arithmetic only and do not require either path to exist.

        .PARAMETER OutputPath
        The full path the raw collector document was written to.

        .PARAMETER RepoRoot
        The repository root the coverage directory is resolved against.

        .OUTPUTS
        True when the document is retained, false when it is to be discarded.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [string]$OutputPath,

        [Parameter(Mandatory = $true)]
        [string]$RepoRoot
    )

    $parentDirectory = Split-Path -Path $OutputPath -Parent
    if (-not $parentDirectory) {
        return $false
    }

    $retainedDirectory = Join-Path -Path $RepoRoot -ChildPath 'coverage'

    return $parentDirectory.TrimEnd('\', '/').Equals(
        $retainedDirectory.TrimEnd('\', '/'),
        [System.StringComparison]::OrdinalIgnoreCase)
}
