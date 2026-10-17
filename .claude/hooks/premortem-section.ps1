<#
.SYNOPSIS
    Shared Pre-Mortem section check for the planner and researcher SubagentStop hooks (issue #964).

.DESCRIPTION
    Test-PreMortemSection reads Markdown text and requires a `## Pre-Mortem` heading followed, before
    the next level-one or level-two heading, by one bullet that begins with each of the seven labels:
    Review checklists, Coverage, Mirror parity, File size, Test isolation, CI-only behavior, and
    Evidence. Labels match case-insensitively and may be bold. Each bullet must carry text after its
    label; a bullet whose text is "not applicable" must also state a reason.

.NOTES
    Pure functions: they read no file, environment variable, or clock, and keep no script-scope state. The calling hooks dot-source this
    file under the guarded Add-HookDependencyFailure pattern and pass the document text they read
    through their own file seams.
#>

Set-StrictMode -Version Latest

function Get-PreMortemLabel {
    <#
    .SYNOPSIS
        Returns the seven required Pre-Mortem labels in their canonical order.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param()

    return [string[]]@(
        'Review checklists',
        'Coverage',
        'Mirror parity',
        'File size',
        'Test isolation',
        'CI-only behavior',
        'Evidence'
    )
}

function Get-PreMortemSectionLine {
    <#
    .SYNOPSIS
        Returns the lines of the `## Pre-Mortem` section, or $null when the heading is absent.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $Content
    )

    $lines = @($Content -split "`r?`n")
    $start = -1
    for ($index = 0; $index -lt $lines.Count; $index++) {
        if ($lines[$index] -match '^##[ \t]+Pre-Mortem[ \t]*$') {
            $start = $index
            break
        }
    }
    if ($start -lt 0) {
        return $null
    }

    $section = [System.Collections.Generic.List[string]]::new()
    for ($index = $start + 1; $index -lt $lines.Count; $index++) {
        if ($lines[$index] -match '^#{1,2}[ \t]') {
            break
        }
        $section.Add($lines[$index])
    }
    return [string[]]@($section)
}

function Test-PreMortemBulletText {
    <#
    .SYNOPSIS
        Returns $true when the text after a label is substantive or is "not applicable" with a reason.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $Text
    )

    $body = ($Text -replace '^[\s:*\-\u2013\u2014]+', '').Trim()
    if ($body -notmatch '[A-Za-z0-9]') {
        return $false
    }
    $notApplicable = [regex]::Match($body, '^not applicable\b(?<Reason>.*)$', 'IgnoreCase')
    if (-not $notApplicable.Success) {
        return $true
    }
    return $notApplicable.Groups['Reason'].Value -match '[A-Za-z0-9]'
}

function Test-PreMortemSection {
    <#
    .SYNOPSIS
        Validates the `## Pre-Mortem` section of a plan or research document.
    .OUTPUTS
        Hashtable with Ok (bool), Message (string or $null), and MissingLabels (string[]): the labels
        that have no bullet, or whose bullet has no text or says "not applicable" without a reason.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $Content
    )

    $labels = Get-PreMortemLabel
    $section = Get-PreMortemSectionLine -Content $Content
    if ($null -eq $section) {
        return @{
            Ok            = $false
            Message       = 'document has no `## Pre-Mortem` section; add one bullet for each of: ' + ($labels -join ', ') + '.'
            MissingLabels = $labels
        }
    }

    $missing = [System.Collections.Generic.List[string]]::new()
    foreach ($label in $labels) {
        $pattern = '^\s*[-*+][ \t]+(?:\*\*)?' + [regex]::Escape($label) + '(?![A-Za-z])(?<Rest>.*)$'
        $valid = $false
        foreach ($line in $section) {
            $match = [regex]::Match($line, $pattern, 'IgnoreCase')
            if ($match.Success -and (Test-PreMortemBulletText -Text $match.Groups['Rest'].Value)) {
                $valid = $true
                break
            }
        }
        if (-not $valid) {
            $missing.Add($label)
        }
    }

    if ($missing.Count -gt 0) {
        return @{
            Ok            = $false
            Message       = '`## Pre-Mortem` section lacks a bullet with text (or "not applicable" with a reason) for: ' + ($missing -join ', ') + '.'
            MissingLabels = [string[]]@($missing)
        }
    }
    return @{ Ok = $true; Message = $null; MissingLabels = [string[]]@() }
}
