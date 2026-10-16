<#
.SYNOPSIS
    Resolve the coverage thresholds that govern the feature-review coverage validation.
.DESCRIPTION
    Implements the threshold precedence of the pushed quality-tiers rule for the
    feature-review SubagentStop hook (issue #824, follow-up FU-823-1). When the
    repository's root CLAUDE.md states a line or branch coverage threshold, that threshold
    governs; otherwise the default governs (85 percent line, 75 percent branch). Each
    metric is resolved on its own, so a root CLAUDE.md that states only a line threshold
    leaves the branch default in force.

    Pure string logic only: the caller supplies the CLAUDE.md text. No disk, process,
    network, clock, or environment access. It is dot-sourced by
    validate-feature-review-coverage.ps1 as:
    . (Join-Path $PSScriptRoot 'feature-review-coverage-thresholds.ps1')
#>


function Get-FeatureReviewCoverageThreshold {
    <#
    .SYNOPSIS
        Return the governing line and branch coverage thresholds for a root CLAUDE.md text.
    .DESCRIPTION
        For each metric M in 'line' and 'branch', a figure is read only when a comparator
        or floor phrase links it to the metric on the same line as 'M coverage'. Two forms
        are accepted, and any text without a percent sign may sit between 'M coverage' and
        the comparator or the figure:

          - Comparator before the figure: one of '>=', the U+2265 sign, 'at least',
            'minimum', 'minimum of', or 'no less than', an optional colon, and the figure
            followed by a percent sign (for example 'line coverage >= 85%').
          - Trailing floor phrase after the figure: the figure followed by a percent sign,
            then one of 'minimum', 'at least', 'or more', 'or higher', 'or above', or
            'or greater' (for example 'line coverage: 70% minimum').

        Prose such as 'line coverage was 62% last release' is not read. The first match in
        text order across both forms whose value parses (invariant culture) to a number
        from 0 to 100 sets that metric and its source to 'claude-md'. Otherwise the metric
        keeps its default (line 85, branch 75) and its source is 'default'.

        Known limitation: a combined statement such as 'line and branch coverage >= 70%'
        matches neither metric, because the metric name must be followed directly by
        'coverage' and must not be joined to the other metric name by 'and', 'or', '&',
        or '/'. Both metrics then keep their defaults, which is the stricter direction.
    .PARAMETER ClaudeMdText
        The text of the repository's root CLAUDE.md; $null or empty when it is absent.
    .OUTPUTS
        System.Management.Automation.PSCustomObject - Line, Branch, LineSource, BranchSource.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)][AllowNull()][AllowEmptyString()][string] $ClaudeMdText
    )

    $defaults = [ordered]@{ line = 85.0; branch = 75.0 }
    $resolved = @{}
    $culture = [System.Globalization.CultureInfo]::InvariantCulture
    $style = [System.Globalization.NumberStyles]::Float

    foreach ($metric in $defaults.Keys) {
        $value = [double]$defaults[$metric]
        $source = 'default'
        if (-not [string]::IsNullOrEmpty($ClaudeMdText)) {
            $pattern = '(?im)(?<!\b(?:line|branch)\s*(?:and|or|&|/)\s*)\b' + $metric + '\s+coverage\b[^\r\n%]*?(?:(?:>=|' + [char]0x2265 + '|\bat\s+least\b|\bminimum(?:\s+of)?\b|\bno\s+less\s+than\b)\s*:?\s*(?<![\d.])(?<pct>\d{1,3}(?:\.\d+)?)\s*%|(?<![\d.])(?<pct>\d{1,3}(?:\.\d+)?)\s*%\s*(?:minimum|at\s+least|or\s+(?:more|higher|above|greater))\b)'
            foreach ($match in [regex]::Matches($ClaudeMdText, $pattern)) {
                $parsed = 0.0
                $isNumber = [double]::TryParse($match.Groups['pct'].Value, $style, $culture, [ref]$parsed)
                if ($isNumber -and $parsed -ge 0 -and $parsed -le 100) {
                    $value = $parsed
                    $source = 'claude-md'
                    break
                }
            }
        }
        $resolved[$metric] = [pscustomobject]@{ Value = $value; Source = $source }
    }

    return [pscustomobject]@{
        Line         = $resolved['line'].Value
        Branch       = $resolved['branch'].Value
        LineSource   = $resolved['line'].Source
        BranchSource = $resolved['branch'].Source
    }
}
