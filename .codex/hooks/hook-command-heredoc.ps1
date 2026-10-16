<#
.SYNOPSIS
    Heredoc header and body readers for the Bash command-line segment scanner.
.DESCRIPTION
    Holds Read-CommandLineHeredocHeader and Read-CommandLineHeredocBody, moved verbatim
    from hook-command-scanner.ps1 (issue #824) so the scanner stays under the 500-line
    limit once it records each segment's Delimiter and TokenText. The functions read
    $script:CommandLineSegmentDelimiters, which the scanner declares before it
    dot-sources this file.

    Pure string logic only: no disk, process, network, clock, or environment access. It is
    dot-sourced by hook-command-scanner.ps1 as:
    . (Join-Path $PSScriptRoot 'hook-command-heredoc.ps1')
#>

function Read-CommandLineHeredocHeader {
    <#
    .SYNOPSIS
        Read a heredoc header beginning at a '<<' operator and report its delimiter.
    .DESCRIPTION
        Handles the plain '<<' form, the tab-stripping '<<-' form, and an optionally quoted
        delimiter word. A delimiter produced by expansion is reported as non-literal, which
        makes the segment unresolvable under R3. Returns NextIndex, Delimiter, StripTabs,
        and IsLiteral.
    .OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)][string] $Text,
        [Parameter(Mandatory)][int] $StartIndex
    )

    $cursor = $StartIndex + 2
    $stripTabs = $false
    if ($cursor -lt $Text.Length -and $Text[$cursor] -eq '-') {
        $stripTabs = $true
        $cursor++
    }

    while ($cursor -lt $Text.Length -and ($Text[$cursor] -eq ' ' -or $Text[$cursor] -eq "`t")) { $cursor++ }

    $word = [System.Text.StringBuilder]::new()
    $isLiteral = $true
    $openQuote = [char]0
    while ($cursor -lt $Text.Length) {
        $character = $Text[$cursor]
        if ($openQuote -ne [char]0) {
            if ($character -eq $openQuote) {
                $openQuote = [char]0
            } else {
                if ($openQuote -eq '"' -and ($character -eq '$' -or $character -eq '`')) { $isLiteral = $false }
                [void]$word.Append($character)
            }
            $cursor++
            continue
        }

        if ($character -eq '"' -or $character -eq "'") {
            $openQuote = $character
            $cursor++
            continue
        }

        if ([char]::IsWhiteSpace($character) -or $script:CommandLineSegmentDelimiters -contains $character) { break }
        if ($character -eq '$' -or $character -eq '`') { $isLiteral = $false }
        [void]$word.Append($character)
        $cursor++
    }

    $delimiter = $word.ToString()
    if ($openQuote -ne [char]0 -or [string]::IsNullOrEmpty($delimiter)) { $isLiteral = $false }

    return [pscustomobject]@{
        NextIndex = $cursor
        Delimiter = $delimiter
        StripTabs = $stripTabs
        IsLiteral = $isLiteral
    }
}

function Read-CommandLineHeredocBody {
    <#
    .SYNOPSIS
        Consume one heredoc body from a start index and report where it ends.
    .DESCRIPTION
        Consumes lines until a line whose content - after optional leading tabs for the
        '<<-' form - equals the delimiter exactly, case-sensitively as in the shell. An
        unterminated body is consumed to end of text and reported through Unbalanced.
    .OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)][string] $Text,
        [Parameter(Mandatory)][int] $StartIndex,
        [Parameter(Mandatory)][AllowEmptyString()][string] $Delimiter,
        [Parameter(Mandatory)][bool] $StripTabs
    )

    $cursor = $StartIndex
    $length = $Text.Length
    while ($cursor -lt $length) {
        $lineEnd = $Text.IndexOf("`n", $cursor)
        if ($lineEnd -lt 0) { $lineEnd = $length }

        $line = $Text.Substring($cursor, $lineEnd - $cursor).TrimEnd("`r")
        if ($StripTabs) { $line = $line.TrimStart("`t") }

        $cursor = if ($lineEnd -lt $length) { $lineEnd + 1 } else { $length }
        if ($line -ceq $Delimiter) {
            return [pscustomobject]@{ NextIndex = $cursor; Terminated = $true }
        }
    }

    return [pscustomobject]@{ NextIndex = $length; Terminated = $false }
}
