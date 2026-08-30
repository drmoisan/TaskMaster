Set-StrictMode -Version Latest

<#
.SYNOPSIS
Counts markdown checkbox items contained by a named heading section.
#>
function Get-NamedSectionCheckboxCount {
    [CmdletBinding()]
    [OutputType([int])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string] $Document,
        [Parameter(Mandatory = $true)][string] $Heading
    )

    $headingPattern = '^(?<marks>#{1,6})\s+' + [regex]::Escape($Heading) + '\s*$'
    $inside = $false
    $level = 0
    $count = 0
    foreach ($line in ($Document -split "`r?`n")) {
        if (-not $inside) {
            $match = [regex]::Match($line, $headingPattern)
            if ($match.Success) { $inside = $true; $level = $match.Groups['marks'].Value.Length }
            continue
        }
        $nextHeading = [regex]::Match($line, '^(?<marks>#{1,6})\s+')
        if ($nextHeading.Success -and $nextHeading.Groups['marks'].Value.Length -le $level) { break }
        if ($line -match '^\s*[-*]\s+\[[ xX]\]\s+') { $count++ }
    }
    return $count
}

Export-ModuleMember -Function Get-NamedSectionCheckboxCount
