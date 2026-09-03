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
