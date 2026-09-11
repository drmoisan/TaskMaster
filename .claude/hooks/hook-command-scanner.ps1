<#
.SYNOPSIS
    Bash command-line segment scanner shared by the Claude and Codex enforcement hooks.
.DESCRIPTION
    Realizes Piece 1 and Piece 2 of the D2 behavior contract in
    docs/features/active/2026-08-25-enforcement-hook-trigger-matches-whole-command-text-545/spec.md.

    A single left-to-right scan of a raw Bash command line produces an ordered list of
    segment records. Each record carries the segment's raw text, a masked text in which
    every quoted span and every attached heredoc body has been replaced by spaces, a
    quote-stripped token list, the leading command word, and the flags a caller needs in
    order to decide whether the masked text is safe to scan.

    Pure string logic only: no disk, process, network, clock, or environment access. It is
    dot-sourced as: . (Join-Path $PSScriptRoot 'hook-command-scanner.ps1')
#>

# The wrapper carve-out set (D2 Piece 2). A segment led by one of these names keeps its raw
# text as scan text, because a wrapper's quoted argument is a nested command line rather
# than inert data. Pinned by test through Get-CommandLineWrapperName (rule R6).
$script:CommandLineWrapperNames = @(
    'sh', 'bash', 'zsh', 'dash', 'ksh', 'pwsh', 'powershell',
    'xargs', 'env', 'command', 'eval', 'nohup', 'time', 'timeout'
)

# Characters that end a segment when they occur outside a quoted span and outside a heredoc
# body (D2 Piece 1). The subshell, group, and substitution openers and closers are
# delimiters by design: treating them as such is what closes the '(git add .)' and
# '$(git add .)' non-match bypasses. The two-character '$(' opener is handled alongside
# this table rather than in it.
$script:CommandLineSegmentDelimiters = [char[]]@(';', '&', '|', "`n", '(', ')', '{', '}', '`')

function Get-CommandLineWrapperName {
    <#
    .SYNOPSIS
        Return the wrapper carve-out set of D2 Piece 2.
    .DESCRIPTION
        A getter rather than a bare variable, so the R6 membership test pins a public surface.
    .OUTPUTS
        System.String[]
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param()

    return [string[]]$script:CommandLineWrapperNames
}

function ConvertTo-CommandLineToken {
    <#
    .SYNOPSIS
        Split one segment into whitespace-delimited tokens with balanced quotes stripped.
    .DESCRIPTION
        The issue #539 ConvertTo-OrchestrationCommandToken idiom, reproduced here so the
        parser carries no dependency on the orchestration gate's helper file. A quoted span
        contributes to the token it sits in and its delimiting quotes are removed, and
        whitespace inside it does not split a token, so `-m "rm -rf x"` yields the two
        tokens `-m` and `rm -rf x` and never an adjacent token pair equal to a literal.
    .OUTPUTS
        System.String[]
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Segment)

    $tokens = [System.Collections.Generic.List[string]]::new()
    $current = [System.Text.StringBuilder]::new()
    $hasToken = $false
    $openQuote = [char]0

    foreach ($character in $Segment.ToCharArray()) {
        if ($openQuote -ne [char]0) {
            if ($character -eq $openQuote) { $openQuote = [char]0 } else { [void]$current.Append($character) }
            continue
        }

        if ($character -eq '"' -or $character -eq "'") {
            $openQuote = $character
            $hasToken = $true
            continue
        }

        if ([char]::IsWhiteSpace($character)) {
            if ($hasToken) {
                $tokens.Add($current.ToString())
                [void]$current.Clear()
                $hasToken = $false
            }
            continue
        }

        $hasToken = $true
        [void]$current.Append($character)
    }
    if ($hasToken) { $tokens.Add($current.ToString()) }
    return $tokens.ToArray()
}

function Get-CommandLineSegmentCommandWord {
    <#
    .SYNOPSIS
        Return the leading command word of a token list, skipping VAR=value prefixes.
    .DESCRIPTION
        Realizes step 1 of D2 Piece 3 for the scanner's own CommandWord property. Returns
        the empty string when the segment carries no command word.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][AllowEmptyCollection()][AllowEmptyString()][AllowNull()][string[]] $Token)

    foreach ($candidate in $Token) {
        if ($candidate -match '^[A-Za-z_][A-Za-z0-9_]*=') { continue }
        return $candidate
    }
    return ''
}

function ConvertTo-CommandLineSegmentRecord {
    <#
    .SYNOPSIS
        Assemble one segment record and apply the three ordered ScanText clauses.
    .DESCRIPTION
        ScanText selection follows D2 Piece 2 in order: RawText when the segment is
        Unbalanced or carries a live substitution; RawText when it is wrapper-led;
        MaskedText otherwise. TokenText is the segment text with heredoc bodies replaced by
        spaces and quoted spans intact. Unbalanced also carries the unmaskable-delimiter
        case of rule R3, because such a heredoc cannot be shown to close.
    .OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string] $RawText,
        [Parameter(Mandatory)][AllowEmptyString()][string] $MaskedText,
        [Parameter(Mandatory)][AllowEmptyString()][string] $TokenText,
        [Parameter(Mandatory)][bool] $HasLiveSubstitution,
        [Parameter(Mandatory)][bool] $Unbalanced
    )

    $tokens = @(ConvertTo-CommandLineToken -Segment $TokenText)
    $commandWord = Get-CommandLineSegmentCommandWord -Token $tokens
    $isWrapperLed = $false
    if (-not [string]::IsNullOrEmpty($commandWord)) {
        $isWrapperLed = $script:CommandLineWrapperNames -contains $commandWord
    }

    $scanText = $MaskedText
    if ($Unbalanced -or $HasLiveSubstitution -or $isWrapperLed) { $scanText = $RawText }

    return [pscustomobject]@{
        RawText             = $RawText
        MaskedText          = $MaskedText
        Tokens              = $tokens
        CommandWord         = $commandWord
        IsWrapperLed        = $isWrapperLed
        HasLiveSubstitution = $HasLiveSubstitution
        Unbalanced          = $Unbalanced
        ScanText            = $scanText
    }
}

function Test-CommandLineSegmentRawScan {
    <#
    .SYNOPSIS
        Report whether the scanner selected RawText as this segment's ScanText.
    .DESCRIPTION
        The three disjuncts below are the same three clauses ConvertTo-CommandLineSegmentRecord
        applies when it selects ScanText, so a caller that guards a raw-text read with this
        predicate reads raw on exactly the segments the scanner already scans raw, and on no
        others.

        It exists for one fail-open class the token readers cannot close by themselves.
        ConvertTo-CommandLineToken collapses a balanced quoted span into ONE token, so a flag
        carried inside a wrapper's quoted argument never appears as a token and every
        token-based flag read reports it absent. A call site that reads flag absence as OUT OF
        SCOPE therefore allows a command the pre-parser whole-text scan denied. A call site
        that reads flag absence as MISSING AUTHORIZATION is already fail-closed and must not
        use this predicate.
    .PARAMETER Segment
        One segment record produced by Read-CommandLineSegment.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowNull()] $Segment)

    if ($null -eq $Segment) {
        return $false
    }

    return ([bool]$Segment.IsWrapperLed -or [bool]$Segment.HasLiveSubstitution -or [bool]$Segment.Unbalanced)
}

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

function Read-CommandLineSegment {
    <#
    .SYNOPSIS
        Scan a raw Bash command line into an ordered list of segment records.
    .DESCRIPTION
        A single left-to-right scan tracking single-quote spans, double-quote spans,
        backslash escapes, and a heredoc delimiter state machine. Segment delimiters,
        recognized only outside quoted spans and heredoc bodies: ';', '&', '|', newline,
        and the subshell/group/substitution openers and closers '(', ')', '{', '}',
        '$(' and the backtick.

        Pure: reads no file, starts no process, reads no clock, mutates no input.
    .PARAMETER CommandText
        The raw Bash command text, exactly as delivered by
        Get-ClaudeHookToolInputString -Name 'command'.
    .OUTPUTS
        System.Management.Automation.PSCustomObject[] - one record per segment, in
        source order. Each record carries:
          RawText             [string]   the segment's original text, unmodified
          MaskedText          [string]   quoted spans and attached heredoc bodies
                                         replaced by single spaces
          Tokens              [string[]] quote-stripped, whitespace-delimited tokens
          CommandWord         [string]   the first token after any VAR=value prefixes,
                                         or '' when the segment has no command word
          IsWrapperLed        [bool]     $true when CommandWord is a member of the
                                         wrapper carve-out set
          HasLiveSubstitution [bool]     $true when '$(' or a backtick occurred inside
                                         one of the segment's double-quoted spans
          Unbalanced          [bool]     $true when a quote span or heredoc did not
                                         close before end of text
          ScanText            [string]   the clause-ordered selection: RawText when
                                         Unbalanced or HasLiveSubstitution or
                                         IsWrapperLed; MaskedText otherwise
        Returns an empty array for null, empty, or whitespace-only input.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject[]])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [AllowNull()]
        [string] $CommandText
    )

    $records = [System.Collections.Generic.List[pscustomobject]]::new()
    if ([string]::IsNullOrWhiteSpace($CommandText)) { return $records.ToArray() }

    $raw = [System.Text.StringBuilder]::new()
    $masked = [System.Text.StringBuilder]::new()
    $tokenText = [System.Text.StringBuilder]::new()
    $pending = [System.Collections.Generic.List[pscustomobject]]::new()
    $hasLiveSubstitution = $false
    $unbalanced = $false
    $inSingle = $false
    $inDouble = $false
    $index = 0
    $length = $CommandText.Length

    while ($index -lt $length) {
        $character = $CommandText[$index]
        $next = if ($index + 1 -lt $length) { $CommandText[$index + 1] } else { [char]0 }

        if ($inSingle) {
            if ($character -eq "'") { $inSingle = $false }
            [void]$raw.Append($character)
            [void]$tokenText.Append($character)
            [void]$masked.Append(' ')
            $index++
            continue
        }

        if ($inDouble) {
            if ($character -eq '\' -and $next -ne [char]0) {
                [void]$raw.Append($character).Append($next)
                [void]$tokenText.Append($character).Append($next)
                [void]$masked.Append('  ')
                $index += 2
                continue
            }
            if ($character -eq '`' -or ($character -eq '$' -and $next -eq '(')) { $hasLiveSubstitution = $true }
            if ($character -eq '"') { $inDouble = $false }
            [void]$raw.Append($character)
            [void]$tokenText.Append($character)
            [void]$masked.Append(' ')
            $index++
            continue
        }

        if ($character -eq '\' -and $next -ne [char]0) {
            [void]$raw.Append($character).Append($next)
            [void]$tokenText.Append($character).Append($next)
            [void]$masked.Append($character).Append($next)
            $index += 2
            continue
        }

        if ($character -eq "'" -or $character -eq '"') {
            if ($character -eq "'") { $inSingle = $true } else { $inDouble = $true }
            [void]$raw.Append($character)
            [void]$tokenText.Append($character)
            [void]$masked.Append(' ')
            $index++
            continue
        }

        if ($character -eq '<' -and $next -eq '<') {
            if ($index + 2 -lt $length -and $CommandText[$index + 2] -eq '<') {
                # '<<<' is a here-string. It has no body and is not a heredoc.
                [void]$raw.Append('<<<')
                [void]$tokenText.Append('<<<')
                [void]$masked.Append('<<<')
                $index += 3
                continue
            }

            $header = Read-CommandLineHeredocHeader -Text $CommandText -StartIndex $index
            $headerText = $CommandText.Substring($index, $header.NextIndex - $index)
            [void]$raw.Append($headerText)
            [void]$tokenText.Append($headerText)
            [void]$masked.Append($headerText)
            if ($header.IsLiteral) {
                $pending.Add($header)
            } else {
                # A delimiter produced by expansion is unmaskable, so the containing
                # segment is unresolvable and falls back to a raw scan (rule R3).
                $unbalanced = $true
            }
            $index = $header.NextIndex
            continue
        }

        $isSubstitutionOpener = $character -eq '$' -and $next -eq '('
        if ($isSubstitutionOpener -or $script:CommandLineSegmentDelimiters -contains $character) {
            if ($character -eq "`n" -and $pending.Count -gt 0) {
                [void]$raw.Append($character)
                [void]$tokenText.Append($character)
                [void]$masked.Append($character)
                $index++
                foreach ($heredoc in $pending) {
                    $body = Read-CommandLineHeredocBody -Text $CommandText -StartIndex $index -Delimiter $heredoc.Delimiter -StripTabs $heredoc.StripTabs
                    $bodyText = $CommandText.Substring($index, $body.NextIndex - $index)
                    [void]$raw.Append($bodyText)
                    [void]$tokenText.Append(' ' * $bodyText.Length)
                    [void]$masked.Append(' ' * $bodyText.Length)
                    if (-not $body.Terminated) { $unbalanced = $true }
                    $index = $body.NextIndex
                }
                $pending.Clear()
            } elseif ($isSubstitutionOpener) {
                $index += 2
            } else {
                $index++
            }

            $rawText = $raw.ToString()
            if (-not [string]::IsNullOrWhiteSpace($rawText)) {
                $records.Add((ConvertTo-CommandLineSegmentRecord -RawText $rawText -MaskedText $masked.ToString() -TokenText $tokenText.ToString() -HasLiveSubstitution $hasLiveSubstitution -Unbalanced $unbalanced))
            }
            [void]$raw.Clear()
            [void]$masked.Clear()
            [void]$tokenText.Clear()
            $hasLiveSubstitution = $false
            $unbalanced = $false
            continue
        }

        [void]$raw.Append($character)
        [void]$tokenText.Append($character)
        [void]$masked.Append($character)
        $index++
    }

    if ($inSingle -or $inDouble -or $pending.Count -gt 0) { $unbalanced = $true }

    $rawText = $raw.ToString()
    if (-not [string]::IsNullOrWhiteSpace($rawText)) {
        $records.Add((ConvertTo-CommandLineSegmentRecord -RawText $rawText -MaskedText $masked.ToString() -TokenText $tokenText.ToString() -HasLiveSubstitution $hasLiveSubstitution -Unbalanced $unbalanced))
    }

    return $records.ToArray()
}
