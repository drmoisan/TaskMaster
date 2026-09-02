<#
.SYNOPSIS
    Markdown fenced-block tracker and opt-out marker detector for the Mermaid gate.

.DESCRIPTION
    Extracts ```` ```mermaid ```` blocks from Markdown text without parsing
    CommonMark. The rule set is deliberately small and each rule exists for a
    stated reason:

      - An opening fence is a run of three or more backticks or three or more
        tildes, indented up to three spaces, optionally prefixed by blockquote
        markers, whose info string's first word is `mermaid` (case-insensitive).
      - A closing fence uses the same fence character, is at least as long as the
        opening run, carries no info string, and sits at the same blockquote
        depth.
      - A fence stack is maintained so a `mermaid` fence nested inside an outer
        open fence is reported as nested. Documentation showing example Mermaid is
        not a diagram, so the validator must skip it (fail-open item 6).
      - An unclosed fence is tolerated: its collected body is still reported, so a
        diagram at the end of a truncated document is not silently dropped.
      - Body lines of a fence opened inside a blockquote are stripped of their
        `> ` prefix, so a blockquoted diagram validates as its unquoted twin.
      - Tilde fences may contain backtick runs and vice versa, which the
        same-character close rule handles without special cases.

    The opt-out marker is `<!-- mermaid-validator: ignore -->` on the line
    immediately preceding the opening fence, with no intervening line. Its scope
    is exactly that one block; a later block needs its own marker. The marker
    exists so that documentation deliberately quoting invalid Mermaid is never
    blocked, which would be worse than having no gate at all.

    Pinned to Mermaid 11.17.0 through MermaidGrammar.psm1. Every function is pure:
    no filesystem, subprocess, network, or wall-clock access, and no input is
    mutated.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# The marker text must be exactly `mermaid-validator: ignore` and is
# case-sensitive. Whitespace is permitted around the line, around the comment
# delimiters, and a blockquote prefix is tolerated so the marker works inside a
# quoted passage.
$script:OptOutMarkerPattern = '^(?:\s{0,3}>\s?)*\s*<!--\s*mermaid-validator: ignore\s*-->\s*$'

# Fence line shape: optional blockquote prefixes, up to three spaces of
# indentation, then the fence run, then the info string.
$script:FenceLinePattern = '^(?<quote>(?:\s{0,3}>\s?)*)(?<indent>\s{0,3})(?<fence>`{3,}|~{3,})(?<info>.*)$'

function Split-MermaidTextLine {
    <#
    .SYNOPSIS
        Splits text into lines, normalizing CRLF, CR, and LF endings.
    .DESCRIPTION
        Line-ending normalization happens once, here, so every downstream rule
        sees the same line array and a CRLF document produces a byte-identical
        verdict to its LF twin.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [AllowEmptyString()]
        [AllowNull()]
        [string] $Text
    )

    if ([string]::IsNullOrEmpty($Text)) {
        return [string[]]@()
    }

    return [string[]]($Text -split '\r\n|\n|\r')
}

function Test-MermaidOptOutMarker {
    <#
    .SYNOPSIS
        Returns $true when a line is the documented Mermaid validator opt-out marker.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowEmptyString()]
        [AllowNull()]
        [string] $Line
    )

    if ([string]::IsNullOrWhiteSpace($Line)) {
        return $false
    }

    return [bool]($Line -cmatch $script:OptOutMarkerPattern)
}

function Get-MermaidFenceLine {
    <#
    .SYNOPSIS
        Parses a candidate Markdown fence line.
    .DESCRIPTION
        Returns $null when the line is not a fence line. Otherwise returns an
        ordered dictionary with FenceCharacter, FenceLength, QuoteDepth, Info, and
        IsMermaid (the info string's first word is `mermaid`, case-insensitive).
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [AllowEmptyString()]
        [AllowNull()]
        [string] $Line
    )

    if ([string]::IsNullOrEmpty($Line)) {
        return $null
    }

    $match = [regex]::Match($Line, $script:FenceLinePattern)
    if (-not $match.Success) {
        return $null
    }

    $fence = $match.Groups['fence'].Value
    $info = $match.Groups['info'].Value.Trim()
    $quoteDepth = ([regex]::Matches($match.Groups['quote'].Value, '>')).Count

    $firstWord = ''
    if (-not [string]::IsNullOrWhiteSpace($info)) {
        $firstWord = ($info -split '[\s{,]+', 2)[0]
    }

    return [ordered]@{
        FenceCharacter = [string]$fence[0]
        FenceLength    = $fence.Length
        QuoteDepth     = $quoteDepth
        Info           = $info
        IsMermaid      = [bool]($firstWord -imatch '^mermaid$')
    }
}

function Get-MermaidUnquotedLine {
    <#
    .SYNOPSIS
        Strips up to QuoteDepth blockquote markers from the start of a body line.
    .DESCRIPTION
        A fence opened inside a blockquote carries a `> ` prefix on every body
        line. Leaving the prefix in place would make the first body line start
        with `>`, which the validator would read as a non-keyword first line and
        reject. Stripping it is what makes a blockquoted diagram validate the same
        as its unquoted twin.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowEmptyString()]
        [AllowNull()]
        [string] $Line,

        [Parameter(Mandatory)]
        [int] $QuoteDepth
    )

    if ($QuoteDepth -le 0 -or [string]::IsNullOrEmpty($Line)) {
        return $Line
    }

    $result = $Line
    for ($count = 0; $count -lt $QuoteDepth; $count++) {
        $stripped = $result -replace '^\s{0,3}>\s?', ''
        if ($stripped -eq $result) {
            break
        }
        $result = $stripped
    }

    return $result
}

function Test-MermaidFenceClose {
    <#
    .SYNOPSIS
        Returns $true when a parsed fence line closes the supplied open fence.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)]
        [System.Collections.Specialized.OrderedDictionary] $Fence,

        [Parameter(Mandatory)]
        [System.Collections.Specialized.OrderedDictionary] $OpenFence
    )

    if ($Fence.FenceCharacter -ne $OpenFence.FenceCharacter) {
        return $false
    }
    if ($Fence.FenceLength -lt $OpenFence.FenceLength) {
        return $false
    }
    if ($Fence.QuoteDepth -ne $OpenFence.QuoteDepth) {
        return $false
    }

    return [string]::IsNullOrEmpty($Fence.Info)
}

function Get-MermaidFenceBlock {
    <#
    .SYNOPSIS
        Extracts every Mermaid fenced block from Markdown text.
    .DESCRIPTION
        Returns an array of ordered dictionaries, one per ```` ```mermaid ````
        block, each with:
          Content        the block body, joined with newlines
          StartLine      1-based line number of the opening fence
          BodyStartLine  1-based line number of the first body line
          IsNested       the fence opened while another fence was already open
          IsOptedOut     the immediately preceding line carried the opt-out marker
          IsClosed       a matching closing fence was found
        A document with no Mermaid fence returns an empty array.
    .PARAMETER Content
        The full Markdown text.
    #>
    [CmdletBinding()]
    [OutputType([object[]])]
    param(
        [AllowEmptyString()]
        [AllowNull()]
        [string] $Content
    )

    $blocks = [System.Collections.Generic.List[object]]::new()
    $lines = @(Split-MermaidTextLine -Text $Content)
    if ($lines.Count -eq 0) {
        return [object[]]@()
    }

    $stack = [System.Collections.Generic.List[object]]::new()

    for ($index = 0; $index -lt $lines.Count; $index++) {
        $line = $lines[$index]
        $fence = Get-MermaidFenceLine -Line $line

        if ($null -ne $fence) {
            if ($stack.Count -gt 0 -and (Test-MermaidFenceClose -Fence $fence -OpenFence $stack[$stack.Count - 1].Fence)) {
                $entry = $stack[$stack.Count - 1]
                $stack.RemoveAt($stack.Count - 1)
                if ($entry.Fence.IsMermaid) {
                    $entry.Block.Content = ($entry.Body -join "`n")
                    $entry.Block.IsClosed = $true
                    $blocks.Add($entry.Block)
                }
                continue
            }

            $marker = $false
            if ($index -gt 0) {
                $marker = Test-MermaidOptOutMarker -Line $lines[$index - 1]
            }

            $stack.Add([ordered]@{
                    Fence = $fence
                    Body  = [System.Collections.Generic.List[string]]::new()
                    Block = [ordered]@{
                        Content       = ''
                        StartLine     = $index + 1
                        BodyStartLine = $index + 2
                        IsNested      = ($stack.Count -gt 0)
                        IsOptedOut    = $marker
                        IsClosed      = $false
                    }
                })
            continue
        }

        if ($stack.Count -gt 0) {
            $top = $stack[$stack.Count - 1]
            $stack[$stack.Count - 1].Body.Add((Get-MermaidUnquotedLine -Line $line -QuoteDepth $top.Fence.QuoteDepth))
        }
    }

    # Unclosed-fence tolerance: report whatever body was collected so a diagram at
    # the end of a truncated document is not silently dropped.
    foreach ($entry in $stack) {
        if ($entry.Fence.IsMermaid) {
            $entry.Block.Content = ($entry.Body -join "`n")
            $blocks.Add($entry.Block)
        }
    }

    # Unclosed blocks are appended after the closed ones, so sort back into
    # document order to keep the caller's line-number reporting monotonic.
    return [object[]]@($blocks.ToArray() | Sort-Object -Property { $_.StartLine })
}

Export-ModuleMember -Function `
    Split-MermaidTextLine, `
    Test-MermaidOptOutMarker, `
    Get-MermaidFenceLine, `
    Get-MermaidUnquotedLine, `
    Test-MermaidFenceClose, `
    Get-MermaidFenceBlock
