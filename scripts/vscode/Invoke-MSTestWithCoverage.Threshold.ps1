Set-StrictMode -Version Latest

function Assert-CoberturaLineCoverageThreshold {
    <#
        .SYNOPSIS
        Throws unless a Cobertura document's document-level line-rate is at or above 80 percent.

        .DESCRIPTION
        Reads the line-rate attribute of the /coverage element, rejects a missing, non-numeric,
        or out-of-range value with a distinct message for each, and throws when the resulting
        percentage is below the 80 percent threshold. The function has no return value: reaching
        its end is the success signal.

        This function lives in its own file rather than alongside its caller in
        Invoke-MSTestWithCoverage.Helpers.ps1 because that file reached the repository's 500-line
        ceiling once issue #733's fixes landed. Helpers.ps1 dot-sources this file, so a caller
        that dot-sources Helpers.ps1 alone still resolves this function.

        .PARAMETER CoberturaXml
        A Cobertura document as a string.

        .OUTPUTS
        None. The function throws on a failed threshold check and returns nothing otherwise.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$CoberturaXml
    )

    [xml]$coverageDocument = $CoberturaXml
    $coverageNode = $coverageDocument.SelectSingleNode('/coverage')
    $lineRateText = if ($coverageNode) { $coverageNode.GetAttribute('line-rate') } else { $null }
    if ([string]::IsNullOrWhiteSpace($lineRateText)) {
        throw 'Cobertura line-rate is missing.'
    }

    [decimal]$lineRate = 0
    if (-not [decimal]::TryParse(
            $lineRateText,
            [System.Globalization.NumberStyles]::Float,
            [System.Globalization.CultureInfo]::InvariantCulture,
            [ref]$lineRate)) {
        throw 'Cobertura line-rate must be numeric.'
    }

    if ($lineRate -lt 0 -or $lineRate -gt 1) {
        throw 'Cobertura line-rate must be between 0 and 1.'
    }

    $percentage = $lineRate * 100
    if ($percentage -lt 80) {
        $formattedPercentage = $percentage.ToString('0.####', [System.Globalization.CultureInfo]::InvariantCulture)
        throw "Cobertura line coverage $formattedPercentage% is below the required 80% threshold."
    }
}

function Assert-CoberturaBranchCoverageThreshold {
    <#
        .SYNOPSIS
        Throws unless a Cobertura document's document-level branch-rate is at or above 75 percent.

        .DESCRIPTION
        Reads the branch-rate attribute of the /coverage element, rejects a missing, non-numeric,
        or out-of-range value with a distinct message for each, rejects a projection whose
        branches-valid attribute is not a positive integer, and throws when the resulting
        percentage is below the 75 percent threshold. The function has no return value: reaching
        its end is the success signal.

        The zero-branch rejection is deliberate and fails closed. A first-party projection that
        carries no valid branches has measured nothing, and the post-processor emits a branch rate
        of 0 in that case, so without the guard a pipeline that produced nothing to measure would
        be distinguishable from a genuinely sub-threshold run only by its message.

        .PARAMETER CoberturaXml
        A Cobertura document as a string. The caller must pass the POST-PROCESSED document
        produced by ConvertTo-KoverageCoberturaXml, never raw collector output. The post-processor
        strips non-allowlisted packages and rewrites the document-root counters from the
        survivors, so only on that document does the root branch-rate describe first-party code.
        Raw collector output aggregates test and third-party modules and reads far lower.

        .OUTPUTS
        None. The function throws on a failed threshold check and returns nothing otherwise.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$CoberturaXml
    )

    [xml]$coverageDocument = $CoberturaXml
    $coverageNode = $coverageDocument.SelectSingleNode('/coverage')
    $branchRateText = if ($coverageNode) { $coverageNode.GetAttribute('branch-rate') } else { $null }
    if ([string]::IsNullOrWhiteSpace($branchRateText)) {
        throw 'Cobertura branch-rate is missing.'
    }

    [decimal]$branchRate = 0
    if (-not [decimal]::TryParse(
            $branchRateText,
            [System.Globalization.NumberStyles]::Float,
            [System.Globalization.CultureInfo]::InvariantCulture,
            [ref]$branchRate)) {
        throw 'Cobertura branch-rate must be numeric.'
    }

    if ($branchRate -lt 0 -or $branchRate -gt 1) {
        throw 'Cobertura branch-rate must be between 0 and 1.'
    }

    $branchesValidText = $coverageNode.GetAttribute('branches-valid')
    [int]$branchesValid = 0
    if (-not [int]::TryParse(
            $branchesValidText,
            [System.Globalization.NumberStyles]::Integer,
            [System.Globalization.CultureInfo]::InvariantCulture,
            [ref]$branchesValid) -or $branchesValid -le 0) {
        throw 'Cobertura branch coverage has no valid branches.'
    }

    $percentage = $branchRate * 100
    if ($percentage -lt 75) {
        $formattedPercentage = $percentage.ToString('0.####', [System.Globalization.CultureInfo]::InvariantCulture)
        throw "Cobertura branch coverage $formattedPercentage% is below the required 75% threshold."
    }
}
