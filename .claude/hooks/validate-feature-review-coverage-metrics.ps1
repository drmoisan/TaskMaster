<#
.SYNOPSIS
    Artifact reader and coverage-report parsers for the feature-review coverage hook.

.DESCRIPTION
    Dot-sourced by validate-feature-review-coverage.ps1. Defines the review-artifact file reader
    (Get-ArtifactFileContent), the LCOV and JaCoCo line and branch coverage parsers, and the
    per-language dispatchers that select a coverage report for a changed language.

    The file has no param() block, writes nothing to the output stream, and runs no top-level
    statement other than function definitions.

.NOTES
    Requires PowerShell 7+.
#>

function Get-ArtifactFileContent {
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return @{ Exists = $false; Text = $null; Lines = @() }
    }

    $text = Get-Content -LiteralPath $Path -Raw -ErrorAction Stop
    $lines = Get-Content -LiteralPath $Path -ErrorAction Stop
    if ($null -eq $lines) {
        $lines = @()
    }
    elseif ($lines -isnot [array]) {
        $lines = @($lines)
    }

    return @{ Exists = $true; Text = $text; Lines = $lines }
}

function Get-FeatureReviewClaudeMdText {
    <#
    .SYNOPSIS
        Reads the repository's root CLAUDE.md for the coverage-floor lookup (CR-4).
    .DESCRIPTION
        Returns a hashtable with Text and ReadError. An absent file yields Text = $null and
        ReadError = $null, so the caller applies the default floors. A file that exists but
        cannot be read yields Text = $null and ReadError set to the first line of the
        exception message, so the caller fails closed. The function never throws.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param()

    try {
        $file = Get-ArtifactFileContent -Path 'CLAUDE.md'
        return @{ Text = $file.Text; ReadError = $null }
    }
    catch {
        return @{ Text = $null; ReadError = ([string]$_.Exception.Message -split "`r?`n")[0] }
    }
}

function Get-LcovRepoCoverage {
    [OutputType([Nullable[double]])]
    param([string]$Path)

    $file = Get-ArtifactFileContent -Path $Path
    if (-not $file.Exists) { return $null }

    $totalFound = 0
    $totalHit = 0
    foreach ($line in $file.Lines) {
        if ($line.StartsWith('LF:')) {
            $totalFound += [int]($line.Substring(3))
        }
        elseif ($line.StartsWith('LH:')) {
            $totalHit += [int]($line.Substring(3))
        }
    }
    if ($totalFound -le 0) { return $null }
    return [math]::Round(($totalHit * 100.0) / $totalFound, 2)
}

function Get-LcovBranchCoverage {
    # Parses LCOV branch counters (BRF: branches found, BRH: branches hit) and returns a percent.
    # LCOV emits one BRF/BRH line per source file in the report; we sum across the file to compute
    # repo-wide branch coverage. Returns $null when the artifact is absent or when no branch
    # information is recorded (BRF total = 0).
    [OutputType([Nullable[double]])]
    param([string]$Path)

    $file = Get-ArtifactFileContent -Path $Path
    if (-not $file.Exists) { return $null }

    $totalFound = 0
    $totalHit = 0
    foreach ($line in $file.Lines) {
        if ($line.StartsWith('BRF:')) {
            $totalFound += [int]($line.Substring(4))
        }
        elseif ($line.StartsWith('BRH:')) {
            $totalHit += [int]($line.Substring(4))
        }
    }
    if ($totalFound -le 0) { return $null }
    return [math]::Round(($totalHit * 100.0) / $totalFound, 2)
}

function Get-JacocoBranchCoverage {
    [OutputType([Nullable[double]])]
    param([string]$Path)

    $file = Get-ArtifactFileContent -Path $Path
    if (-not $file.Exists) { return $null }

    [xml]$doc = $file.Text
    $counters = $doc.SelectNodes('//counter[@type="BRANCH"]')
    if (-not $counters -or $counters.Count -eq 0) { return $null }

    $missed = 0
    $covered = 0
    foreach ($counter in $counters) {
        $missed += [int]$counter.missed
        $covered += [int]$counter.covered
    }
    $total = $missed + $covered
    if ($total -le 0) { return $null }
    return [math]::Round(($covered * 100.0) / $total, 2)
}

function Get-LanguageBranchCoverage {
    [OutputType([Nullable[double]])]
    param([string]$Language)

    switch ($Language) {
        'TypeScript' { return Get-LcovBranchCoverage -Path 'coverage/lcov.info' }
        'Python' { return Get-LcovBranchCoverage -Path 'artifacts/python/lcov.info' }
        'PowerShell' { return Get-JacocoBranchCoverage -Path 'artifacts/pester/powershell-coverage.xml' }
        'CSharp' { return Get-JacocoBranchCoverage -Path 'artifacts/csharp/coverage.xml' }
    }
    return $null
}

function Get-JacocoRepoCoverage {
    [OutputType([Nullable[double]])]
    param([string]$Path)

    $file = Get-ArtifactFileContent -Path $Path
    if (-not $file.Exists) { return $null }

    [xml]$doc = $file.Text
    $counters = $doc.SelectNodes('//counter[@type="LINE"]')
    if (-not $counters -or $counters.Count -eq 0) { return $null }

    $missed = 0
    $covered = 0
    foreach ($counter in $counters) {
        $missed += [int]$counter.missed
        $covered += [int]$counter.covered
    }
    $total = $missed + $covered
    if ($total -le 0) { return $null }
    return [math]::Round(($covered * 100.0) / $total, 2)
}

function Get-LanguageRepoCoverage {
    [OutputType([Nullable[double]])]
    param(
        [string]$Language
    )

    switch ($Language) {
        'TypeScript' { return Get-LcovRepoCoverage -Path 'coverage/lcov.info' }
        'Python' { return Get-LcovRepoCoverage -Path 'artifacts/python/lcov.info' }
        'PowerShell' { return Get-JacocoRepoCoverage -Path 'artifacts/pester/powershell-coverage.xml' }
        'CSharp' { return Get-JacocoRepoCoverage -Path 'artifacts/csharp/coverage.xml' }
    }
    return $null
}
