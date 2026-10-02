# Pure rule functions of the repository hygiene guard. Every function here takes text and returns
# a classification or line-numbered match records; none reads the disk, the environment or the
# clock, so each rule is testable in memory.

function Get-UserProfilePathPattern {
    <#
    .SYNOPSIS
        Returns the generic Windows user-profile path pattern.
    .DESCRIPTION
        The pattern is a drive letter, a colon, a separator run, the profile parent and a separator
        run followed by the first character of a user segment. The last class admits letters,
        digits, underscore, dot, tilde and hyphen, so a placeholder segment (which begins with a
        less-than sign) never matches. The caller applies it case-insensitively.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param()

    return '[a-z]:[\\/]+users[\\/]+[a-z0-9_.~-]'
}

function Find-UserProfilePathMatch {
    <#
    .SYNOPSIS
        Returns one record per line of the text that matches the user-profile path pattern.
    .DESCRIPTION
        Each record carries only a one-based LineNumber property; the matched text is never
        returned, so no caller can echo an identifier. Matching is case-insensitive.
    .PARAMETER Text
        The decoded file text to scan. Empty text yields no records.
    .OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter()]
        [string]$Text = ''
    )

    $options = [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [System.Text.RegularExpressions.RegexOptions]::CultureInvariant
    $regex = [regex]::new((Get-UserProfilePathPattern), $options)
    if ([string]::IsNullOrEmpty($Text) -or -not $regex.IsMatch($Text)) {
        return
    }

    $lines = $Text -split '\r?\n'
    for ($index = 0; $index -lt $lines.Count; $index++) {
        if ($regex.IsMatch($lines[$index])) {
            [pscustomobject]@{ LineNumber = $index + 1 }
        }
    }
}

function Get-RawEvidenceDocumentKind {
    <#
    .SYNOPSIS
        Classifies a tracked file as a raw test-platform or coverage-collector document.
    .DESCRIPTION
        The extension gate runs first: trx is trx; coverage and coveragexml are dotnet-coverage;
        any extension other than xml is none. For xml the byte-order mark is stripped, the
        declaration, comments, processing instructions and a DOCTYPE are skipped, and the first
        element name (terminated by whitespace, a greater-than sign or a slash) is mapped:
        coverage is cobertura, results is dotnet-coverage, TestRun is trx, CoverageSession is
        opencover, and report is jacoco-raw when the document carries a class, sourcefile, method
        or line element, else jacoco-projection. Anything else is none. A finding is any kind other
        than none and jacoco-projection.
    .PARAMETER RelativePath
        The repository-relative path of the tracked file.
    .PARAMETER Content
        The decoded file text. Read only for the xml extension; empty content classifies an xml
        file as none.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath,

        [Parameter()]
        [string]$Content = ''
    )

    $extension = [System.IO.Path]::GetExtension($RelativePath).ToLowerInvariant()
    switch ($extension) {
        '.trx' { return 'trx' }
        '.coverage' { return 'dotnet-coverage' }
        '.coveragexml' { return 'dotnet-coverage' }
        '.xml' { }
        default { return 'none' }
    }

    if ([string]::IsNullOrEmpty($Content)) {
        return 'none'
    }

    $singleLine = [System.Text.RegularExpressions.RegexOptions]::Singleline
    $text = $Content.TrimStart([char]0xFEFF)
    $prolog = [regex]::Match($text, '\A(?:\s+|<\?.*?\?>|<!--.*?-->|<!DOCTYPE[^>\[]*(?:\[.*?\])?\s*>)*', $singleLine)
    $root = [regex]::Match($text.Substring($prolog.Length), '\A<([^\s>/]+)')
    if (-not $root.Success) {
        return 'none'
    }

    switch -CaseSensitive ($root.Groups[1].Value) {
        'coverage' { return 'cobertura' }
        'results' { return 'dotnet-coverage' }
        'TestRun' { return 'trx' }
        'CoverageSession' { return 'opencover' }
        'report' {
            if ([regex]::IsMatch($text, '<(class|sourcefile|method|line)[\s>]')) {
                return 'jacoco-raw'
            }
            return 'jacoco-projection'
        }
    }

    return 'none'
}

function Test-BackupFilePath {
    <#
    .SYNOPSIS
        Tests whether a tracked path is a backup file.
    .DESCRIPTION
        A path is a backup file when its final extension equals .bak, compared case-insensitively.
        Only the final extension is compared, so a directory named bak, a longer extension such as
        .bakery and a path whose final extension is another value are not backup files.
    .PARAMETER RelativePath
        The repository-relative path of the tracked file.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    return [System.IO.Path]::GetExtension($RelativePath) -ieq '.bak'
}
