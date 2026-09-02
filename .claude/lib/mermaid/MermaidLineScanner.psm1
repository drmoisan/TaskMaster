<#
.SYNOPSIS
    Quote-aware single-line scanner for the structural Mermaid validator.

.DESCRIPTION
    Pure string analysis for one line of Mermaid source. Every rule here exists to
    avoid a specific false positive catalogued in the feature research:

      - Characters inside a double-quoted span never participate in bracket
        balance, arrow scanning, or comment detection, so `A["foo[bar](baz)"]`
        and `A["50%% off"]` are accepted.
      - A backslash is an ordinary character. Mermaid has no escape system; the
        documented mechanism is the `#quot;` entity. Treating `\"` as an escape
        would turn a valid label into a spurious unterminated-quote finding.
      - Angle brackets are never structural, so `<br/>` in a label is inert and
        `>` participates only as part of an arrow token.
      - A `%%{...}%%` directive is recognized before comment stripping, so a
        directive is never deleted as a comment.
      - Arrow tokens are masked out before bracket counting, because `{`, `}`, and
        `|` appear inside ER cardinality tokens (`||--o{`) and class relations
        (`<|--`) where they are not brackets at all.
      - Bracket-label contents are masked before arrow scanning, so arrow-like
        text inside a node label is not read as an edge token.
      - A single-character arrow core is kept only when an affix was applied, so
        the tilde delimiters of a class generic (`List~int~`) are not arrow
        tokens while the one-dash sequence arrows (`->`, `-x`, `-)`) still are.
      - The letter-shaped affixes `o` and `x` are absorbed only across a
        non-word boundary, so the `x` in `Box--Bar` never extends the `--` core.

    Pinned to Mermaid 11.17.0 through MermaidGrammar.psm1. Every function is
    pure: no filesystem, subprocess, network, or wall-clock access, and no input
    is mutated.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'MermaidGrammar.psm1') -Force -ErrorAction Stop

# Arrow-token affix tables. An arrow candidate is a run of `-`, `=`, `.`, or `~`
# (the "core") optionally extended by one of these affixes on either side. The
# two-character forms are tried before the one-character forms so `<<-->>` and
# `||--o{` tokenize whole.
$script:ArrowLeftAffixTwo = @('<<', '<|', '}o', '}|', '|o', '||')
$script:ArrowLeftAffixOne = @('<', 'o', 'x', '*')
$script:ArrowRightAffixTwo = @('>>', '|>', 'o|', 'o{', '||', '|{')
$script:ArrowRightAffixOne = @('>', 'o', 'x', ')', '*', '\', '/')

function Test-MermaidWordCharacter {
    <#
    .SYNOPSIS
        Returns $true when the character is a letter, digit, or underscore.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)]
        [char] $Character
    )

    if ($Character -eq '_') {
        return $true
    }
    return [char]::IsLetterOrDigit($Character)
}

function Test-MermaidDirectiveLine {
    <#
    .SYNOPSIS
        Returns $true when the line is a `%%{...}%%` init directive.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowEmptyString()]
        [string] $Line
    )

    if ([string]::IsNullOrWhiteSpace($Line)) {
        return $false
    }
    return $Line.TrimStart().StartsWith('%%{', [System.StringComparison]::Ordinal)
}

function Test-MermaidCommentLine {
    <#
    .SYNOPSIS
        Returns $true when the whole line is a `%%` comment and not a directive.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowEmptyString()]
        [string] $Line
    )

    if ([string]::IsNullOrWhiteSpace($Line)) {
        return $false
    }
    if (Test-MermaidDirectiveLine -Line $Line) {
        return $false
    }
    return $Line.TrimStart().StartsWith('%%', [System.StringComparison]::Ordinal)
}

function Get-MermaidQuoteMaskedText {
    <#
    .SYNOPSIS
        Masks quoted spans and strips a trailing `%%` comment from one line.
    .DESCRIPTION
        Returns Text (every character of every double-quoted span, including the
        quote marks, replaced by a space, truncated at the first `%%` outside a
        quoted span; indices align one-for-one with the input up to that point),
        HasUnterminatedQuote, and ColonIndex (index of the first `:` outside a
        quoted span, or -1).
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [AllowEmptyString()]
        [string] $Line
    )

    $builder = [System.Text.StringBuilder]::new()
    $inQuote = $false
    $colonIndex = -1
    $index = 0

    while ($index -lt $Line.Length) {
        $character = $Line[$index]

        if ($inQuote) {
            if ($character -eq '"') {
                $inQuote = $false
            }
            [void]$builder.Append(' ')
            $index++
            continue
        }

        if ($character -eq '"') {
            $inQuote = $true
            [void]$builder.Append(' ')
            $index++
            continue
        }

        # A comment runs to end of line. The directive case is recognized by the
        # caller before this function is reached, so it is never stripped here.
        if ($character -eq '%' -and ($index + 1) -lt $Line.Length -and $Line[$index + 1] -eq '%') {
            break
        }

        if ($character -eq ':' -and $colonIndex -lt 0) {
            $colonIndex = $index
        }

        [void]$builder.Append($character)
        $index++
    }

    return [ordered]@{
        Text                 = $builder.ToString()
        HasUnterminatedQuote = $inQuote
        ColonIndex           = $colonIndex
    }
}

function Get-MermaidArrowCandidate {
    <#
    .SYNOPSIS
        Extracts the candidate arrow tokens from already-masked line text.
    .PARAMETER Text
        Masked line text, normally the ArrowText of Get-MermaidLineScan.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [AllowEmptyString()]
        [string] $Text
    )

    $candidates = [System.Collections.Generic.List[string]]::new()
    if ([string]::IsNullOrEmpty($Text)) {
        return [string[]]@()
    }

    foreach ($match in [regex]::Matches($Text, '[-=.~]+')) {
        $start = $match.Index
        $length = $match.Length

        $leftLength = 0
        if ($start -ge 2 -and $script:ArrowLeftAffixTwo -contains $Text.Substring($start - 2, 2)) {
            $leftLength = 2
        } elseif ($start -ge 1) {
            $affix = [string]$Text[$start - 1]
            if ($script:ArrowLeftAffixOne -contains $affix) {
                if ($affix -eq 'o' -or $affix -eq 'x') {
                    if ($start -lt 2 -or -not (Test-MermaidWordCharacter -Character $Text[$start - 2])) {
                        $leftLength = 1
                    }
                } else {
                    $leftLength = 1
                }
            }
        }

        $end = $start + $length
        $rightLength = 0
        if (($end + 2) -le $Text.Length -and $script:ArrowRightAffixTwo -contains $Text.Substring($end, 2)) {
            $rightLength = 2
        } elseif ($end -lt $Text.Length) {
            $affix = [string]$Text[$end]
            if ($script:ArrowRightAffixOne -contains $affix) {
                if ($affix -eq 'o' -or $affix -eq 'x') {
                    if (($end + 1) -ge $Text.Length -or -not (Test-MermaidWordCharacter -Character $Text[$end + 1])) {
                        $rightLength = 1
                    }
                } else {
                    $rightLength = 1
                }
            }
        }

        if ($length -lt 2 -and $leftLength -eq 0 -and $rightLength -eq 0) {
            continue
        }

        $candidates.Add($Text.Substring($start - $leftLength, $leftLength + $length + $rightLength))
    }

    return [string[]]@($candidates.ToArray())
}

function Get-MermaidLabelMaskedText {
    <#
    .SYNOPSIS
        Masks the contents of bracket-delimited label spans, keeping the brackets.
    .DESCRIPTION
        Indices align one-for-one with the input. Closers are clamped at depth
        zero so a sequence async arrow (`-)`) is preserved.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowEmptyString()]
        [string] $Text
    )

    $builder = [System.Text.StringBuilder]::new()
    $depth = 0

    foreach ($character in $Text.ToCharArray()) {
        if ($character -eq '[' -or $character -eq '(' -or $character -eq '{') {
            $depth++
            [void]$builder.Append($character)
        } elseif ($character -eq ']' -or $character -eq ')' -or $character -eq '}') {
            if ($depth -gt 0) {
                $depth--
            }
            [void]$builder.Append($character)
        } elseif ($depth -gt 0) {
            [void]$builder.Append(' ')
        } else {
            [void]$builder.Append($character)
        }
    }

    return $builder.ToString()
}

function Get-MermaidArrowMaskedText {
    <#
    .SYNOPSIS
        Blanks every arrow candidate so bracket counting ignores arrow tokens.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowEmptyString()]
        [string] $Text
    )

    if ([string]::IsNullOrEmpty($Text)) {
        return $Text
    }

    $characters = $Text.ToCharArray()
    $offset = 0
    foreach ($candidate in @(Get-MermaidArrowCandidate -Text $Text)) {
        $position = $Text.IndexOf($candidate, $offset, [System.StringComparison]::Ordinal)
        if ($position -lt 0) {
            continue
        }
        for ($index = $position; $index -lt ($position + $candidate.Length); $index++) {
            $characters[$index] = ' '
        }
        $offset = $position + $candidate.Length
    }

    return [string]::new($characters)
}

function Get-MermaidBracketDelta {
    <#
    .SYNOPSIS
        Counts the net square, round, and curly bracket delta of masked text.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [AllowEmptyString()]
        [string] $Text
    )

    $square = 0
    $round = 0
    $curly = 0

    foreach ($character in $Text.ToCharArray()) {
        switch ($character) {
            '[' { $square++ }
            ']' { $square-- }
            '(' { $round++ }
            ')' { $round-- }
            '{' { $curly++ }
            '}' { $curly-- }
            default { }
        }
    }

    return [ordered]@{ Square = $square; Round = $round; Curly = $curly }
}

function Get-MermaidLineScan {
    <#
    .SYNOPSIS
        Produces the full scan result for one line of Mermaid source.
    .DESCRIPTION
        Returns Raw, IsBlank, IsDirective, IsComment, Structural (quote-masked and
        comment-stripped), ArrowText (Structural with bracket-label contents
        masked), BracketText (Structural with arrow tokens masked),
        HasUnterminatedQuote, ColonIndex, FirstToken, Class (Blank | Directive |
        Comment | StatementKeyword | Edge | Unclassifiable), and BracketDelta.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [AllowEmptyString()]
        [string] $Line
    )

    $isDirective = Test-MermaidDirectiveLine -Line $Line
    $isComment = Test-MermaidCommentLine -Line $Line
    $isBlank = [string]::IsNullOrWhiteSpace($Line)

    if ($isDirective -or $isComment -or $isBlank) {
        $inertClass = if ($isBlank) { 'Blank' } elseif ($isDirective) { 'Directive' } else { 'Comment' }
        return [ordered]@{
            Raw                  = $Line
            IsBlank              = $isBlank
            IsDirective          = $isDirective
            IsComment            = $isComment
            Structural           = ''
            ArrowText            = ''
            BracketText          = ''
            HasUnterminatedQuote = $false
            ColonIndex           = -1
            FirstToken           = ''
            Class                = $inertClass
            BracketDelta         = [ordered]@{ Square = 0; Round = 0; Curly = 0 }
        }
    }

    $masked = Get-MermaidQuoteMaskedText -Line $Line
    $structural = $masked.Text
    $arrowText = Get-MermaidLabelMaskedText -Text $structural
    $bracketText = Get-MermaidArrowMaskedText -Text $structural

    # A trailing colon or semicolon is stripped from the first token because the
    # accessibility statements are written `accTitle: text` and `accDescr: text`.
    # Without this normalization those lines would miss the statement-keyword
    # exemption and their free text would be arrow-checked.
    $firstToken = ''
    if (-not [string]::IsNullOrWhiteSpace($structural)) {
        $firstToken = ($structural.Trim() -split '\s+', 2)[0] -replace '[:;]+$', ''
    }

    $class = 'Unclassifiable'
    if (Test-MermaidStatementKeyword -Token $firstToken) {
        $class = 'StatementKeyword'
    } elseif (@(Get-MermaidArrowCandidate -Text $arrowText).Count -gt 0) {
        $class = 'Edge'
    }

    return [ordered]@{
        Raw                  = $Line
        IsBlank              = $false
        IsDirective          = $false
        IsComment            = $false
        Structural           = $structural
        ArrowText            = $arrowText
        BracketText          = $bracketText
        HasUnterminatedQuote = $masked.HasUnterminatedQuote
        ColonIndex           = $masked.ColonIndex
        FirstToken           = $firstToken
        Class                = $class
        BracketDelta         = (Get-MermaidBracketDelta -Text $bracketText)
    }
}

function Get-MermaidLineClass {
    <#
    .SYNOPSIS
        Returns the classification of one line of Mermaid source.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowEmptyString()]
        [string] $Line
    )

    return (Get-MermaidLineScan -Line $Line).Class
}

function Test-MermaidLineBracketBalanced {
    <#
    .SYNOPSIS
        Returns $true when a line's own bracket deltas are all zero.
    .DESCRIPTION
        Per-line balance is a diagnostic, not the validator's verdict. Legal
        Mermaid opens a brace block on one line and closes it on another, so the
        validator aggregates deltas across the diagram body instead.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowEmptyString()]
        [string] $Line
    )

    $delta = (Get-MermaidLineScan -Line $Line).BracketDelta
    return ($delta.Square -eq 0 -and $delta.Round -eq 0 -and $delta.Curly -eq 0)
}

function Split-MermaidStatementLabel {
    <#
    .SYNOPSIS
        Splits a line at its first unquoted colon into statement and label parts.
    .DESCRIPTION
        Mermaid puts free text after the first colon on sequence, class, state, and
        ER statement lines, so arrow and quote judgement must stop at the colon.
        Returns Statement, Label, and HasLabel. With no unquoted colon, Statement
        is the whole line and Label is empty.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [AllowEmptyString()]
        [string] $Line
    )

    $scan = Get-MermaidLineScan -Line $Line
    if ($scan.ColonIndex -lt 0) {
        return [ordered]@{ Statement = $Line; Label = ''; HasLabel = $false }
    }

    $index = [Math]::Min($scan.ColonIndex, $Line.Length)
    return [ordered]@{
        Statement = $Line.Substring(0, $index)
        Label     = $Line.Substring([Math]::Min($index + 1, $Line.Length))
        HasLabel  = $true
    }
}

Export-ModuleMember -Function `
    Test-MermaidWordCharacter, `
    Test-MermaidDirectiveLine, `
    Test-MermaidCommentLine, `
    Get-MermaidQuoteMaskedText, `
    Get-MermaidArrowCandidate, `
    Get-MermaidLabelMaskedText, `
    Get-MermaidArrowMaskedText, `
    Get-MermaidBracketDelta, `
    Get-MermaidLineScan, `
    Get-MermaidLineClass, `
    Test-MermaidLineBracketBalanced, `
    Split-MermaidStatementLabel
