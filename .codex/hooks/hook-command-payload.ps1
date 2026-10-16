<#
.SYNOPSIS
    Payload-aware command-line iterator shared by the Claude and Codex enforcement hooks.
.DESCRIPTION
    Issue #824. Read-CommandLineInvocationSegment returns one record per command visible in a
    Bash command line, in pre-order source order: each top-level scanner segment, each command
    inside a wrapper payload (sh, bash, zsh, dash, ksh, pwsh, powershell, eval), each command
    inside a double-quoted command substitution, and the command xargs injects arguments into.
    PowerShell payloads are parsed by hook-command-payload-powershell.ps1. This file also holds
    the whole-token presence predicate and the inert-payload proof.

    Pure string logic only: no disk, process, network, clock, or environment access. It is
    dot-sourced by hook-command-invocation.ps1, after hook-command-scanner.ps1.
#>

# Wrapper words whose argument is itself a command line (design item 4).
$script:CommandLinePosixShellNames = @('sh', 'bash', 'zsh', 'dash', 'ksh')
$script:CommandLinePowerShellHostNames = @('pwsh', 'powershell')

# The fixed sink allowlist of the inert proof (spec design item 8). Not extended here.
$script:CommandLineInertSinkNames = @(
    'Write-Output', 'Write-Host', 'Write-Verbose', 'Write-Information',
    'echo', 'printf', 'Select-String', 'grep', 'rg'
)

# xargs options that consume the following token when no value is attached.
$script:CommandLineXargsValueOptions = @('-n', '-I', '-L', '-d', '-P', '-s', '-E', '-a')

function ConvertTo-CommandLineLeafWord {
    <#
    .SYNOPSIS
        Return the text after the last '/' or '\', with a trailing '.exe' removed
        case-insensitively. Other case is preserved; callers compare case-insensitively.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string] $Word)

    if ([string]::IsNullOrEmpty($Word)) { return '' }
    $leaf = $Word.Substring($Word.LastIndexOfAny([char[]]@('/', '\')) + 1)
    if ($leaf.EndsWith('.exe', [System.StringComparison]::OrdinalIgnoreCase)) {
        $leaf = $leaf.Substring(0, $leaf.Length - 4)
    }
    return $leaf
}

function Test-CommandLineWordPresent {
    <#
    .SYNOPSIS
        Report whether a word occurs in a text as a whole token.
    .DESCRIPTION
        Matches (?<![\w-])<word>(?![\w-]) with IgnoreCase and CultureInvariant, so 'gh' is not
        present in 'through' and 'new' is not present in 'New-Object'. This is the only
        presence implementation the classifier uses.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string] $RawText,
        [Parameter(Mandatory)][ValidateNotNullOrEmpty()][string] $Word
    )

    if ([string]::IsNullOrEmpty($RawText)) { return $false }
    $pattern = '(?<![\w-])' + [regex]::Escape($Word) + '(?![\w-])'
    $options = [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [System.Text.RegularExpressions.RegexOptions]::CultureInvariant
    return [regex]::IsMatch($RawText, $pattern, $options)
}

function ConvertTo-CommandLineNormalizedText {
    <#
    .SYNOPSIS
        Pre-normalize POSIX command text before it is segmented.
    .DESCRIPTION
        Replaces each backslash + optional CR + LF with one space (design item 1), and quotes an
        unquoted standalone '{}' word as '{}': the shell reads that word as a literal (the
        xargs -I and find -exec placeholder), but the scanner treats every brace as a group
        delimiter, which would detach an xargs payload from its xargs word.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string] $CommandText)

    if ([string]::IsNullOrEmpty($CommandText)) { return '' }
    $text = [regex]::Replace($CommandText, '\\\r?\n', ' ')
    $builder = [System.Text.StringBuilder]::new()
    $quote = [char]0
    for ($index = 0; $index -lt $text.Length; $index++) {
        $character = $text[$index]
        $hasNext = $index + 1 -lt $text.Length
        if ($character -eq '\' -and $hasNext -and $quote -ne "'") {
            [void]$builder.Append($character).Append($text[$index + 1])
            $index++
            continue
        }
        if ($quote -ne [char]0) {
            if ($character -eq $quote) { $quote = [char]0 }
            [void]$builder.Append($character)
            continue
        }
        if ($character -eq '"' -or $character -eq "'") { $quote = $character }
        $standalone = $character -eq '{' -and $hasNext -and $text[$index + 1] -eq '}' -and
        ($index -eq 0 -or [char]::IsWhiteSpace($text[$index - 1])) -and
        ($index + 2 -ge $text.Length -or $text[$index + 2] -match '[\s;|&)\\]')
        if ($standalone) {
            [void]$builder.Append("'{}'")
            $index++
            continue
        }
        [void]$builder.Append($character)
    }
    return $builder.ToString()
}

function Split-CommandLinePosixWord {
    <#
    .SYNOPSIS
        Split POSIX text into shell words, removing quotes and backslash escapes.
    .DESCRIPTION
        Single quotes are literal; inside double quotes a backslash escapes '"', '\', '$', and
        the backtick; outside quotes it escapes any character. Used to read a wrapper's
        arguments, so bash -c "\"gh\" issue create" yields the payload '"gh" issue create'.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Text)

    $words = [System.Collections.Generic.List[string]]::new()
    $current = [System.Text.StringBuilder]::new()
    $hasWord = $false
    $quote = [char]0
    for ($index = 0; $index -lt $Text.Length; $index++) {
        $character = $Text[$index]
        $next = if ($index + 1 -lt $Text.Length) { $Text[$index + 1] } else { [char]0 }
        if ($quote -eq "'") {
            if ($character -eq "'") { $quote = [char]0 } else { [void]$current.Append($character) }
            continue
        }
        if ($quote -eq '"') {
            if ($character -eq '\' -and $next -ne [char]0 -and '"\$`'.IndexOf($next) -ge 0) { [void]$current.Append($next); $index++ }
            elseif ($character -eq '"') { $quote = [char]0 }
            else { [void]$current.Append($character) }
            continue
        }
        if ($character -eq '\' -and $next -ne [char]0) {
            [void]$current.Append($next)
            $hasWord = $true
            $index++
            continue
        }
        if ($character -eq '"' -or $character -eq "'") {
            $quote = $character
            $hasWord = $true
            continue
        }
        if ([char]::IsWhiteSpace($character)) {
            if ($hasWord) { $words.Add($current.ToString()); [void]$current.Clear(); $hasWord = $false }
            continue
        }
        [void]$current.Append($character)
        $hasWord = $true
    }
    if ($hasWord) { $words.Add($current.ToString()) }
    return [string[]]$words.ToArray()
}

function Skip-CommandLineTransparentWrapper {
    <#
    .SYNOPSIS
        Return Index, the position of the command word after VAR=value prefixes and transparent
        wrappers, and SkippedWrapper.
    .DESCRIPTION
        Skips env with its dash and VAR=value tokens, timeout with its dash tokens and one
        duration token, time -p, command -p|-v|-V, and nohup (design item 4). Except names a
        word that is never skipped, so a query for that word can match it.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Token,
        [AllowEmptyString()][string] $Except = ''
    )

    $index = 0
    $skipped = $false
    while ($index -lt $Token.Count) {
        if ($Token[$index] -match '^[A-Za-z_][A-Za-z0-9_]*=') { $index++; continue }
        $leaf = (ConvertTo-CommandLineLeafWord -Word $Token[$index]).ToLowerInvariant()
        if ($leaf -eq $Except.ToLowerInvariant() -or $script:CommandLineTransparentWrapperNames -notcontains $leaf) { break }
        $skipped = $true
        $index++
        switch ($leaf) {
            'env' {
                while ($index -lt $Token.Count -and ($Token[$index].StartsWith('-') -or $Token[$index] -match '^[A-Za-z_][A-Za-z0-9_]*=')) { $index++ }
            }
            'timeout' {
                while ($index -lt $Token.Count -and $Token[$index].StartsWith('-')) { $index++ }
                if ($index -lt $Token.Count) { $index++ }
            }
            'time' { if ($index -lt $Token.Count -and $Token[$index] -eq '-p') { $index++ } }
            'command' { while ($index -lt $Token.Count -and @('-p', '-v', '-V') -ccontains $Token[$index]) { $index++ } }
        }
    }
    return [pscustomobject]@{ Index = $index; SkippedWrapper = $skipped }
}

function Get-CommandLineWrapperPayload {
    <#
    .SYNOPSIS
        Identify a payload wrapper in a word list and extract its payload.
    .DESCRIPTION
        Returns Wrapper ('' when the words are not led by a payload wrapper), Kind ('Posix',
        'PowerShell', or 'Words'), Payload, Words (the xargs-injected command), Opaque, and
        OpaqueReason. A wrapper with no extractable payload is Opaque with reason NoPayload.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param([Parameter(Mandatory)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Word)

    $result = [pscustomobject]@{ Wrapper = ''; Kind = ''; Payload = ''; Words = [string[]]@(); Opaque = $false; OpaqueReason = '' }
    $start = (Skip-CommandLineTransparentWrapper -Token $Word).Index
    if ($start -ge $Word.Count) { return $result }
    $wrapper = (ConvertTo-CommandLineLeafWord -Word $Word[$start]).ToLowerInvariant()
    $rest = [string[]]@($Word | Select-Object -Skip ($start + 1))

    if ($script:CommandLinePosixShellNames -contains $wrapper) {
        $result.Wrapper = $wrapper
        for ($index = 0; $index -lt $rest.Count -and $rest[$index].StartsWith('-'); $index++) {
            if ($rest[$index] -cmatch '^-[A-Za-z]*c[A-Za-z]*$' -and $index + 1 -lt $rest.Count) {
                $result.Kind = 'Posix'; $result.Payload = $rest[$index + 1]; return $result
            }
        }
    } elseif ($script:CommandLinePowerShellHostNames -contains $wrapper) {
        # An Opaque result carries NoPayload or DecodeFailure from the PowerShell adapter.
        $powerShell = Get-CommandLinePowerShellPayload -Word $rest
        $result.Wrapper = $wrapper; $result.Kind = 'PowerShell'; $result.Payload = $powerShell.Payload
        $result.Opaque = $powerShell.Opaque; $result.OpaqueReason = $powerShell.OpaqueReason
        return $result
    } elseif ($wrapper -eq 'eval') {
        $result.Wrapper = $wrapper
        if ($rest.Count -gt 0) { $result.Kind = 'Posix'; $result.Payload = $rest -join ' '; return $result }
    } elseif ($wrapper -eq 'xargs') {
        $result.Wrapper = $wrapper
        $index = 0
        while ($index -lt $rest.Count -and $rest[$index].StartsWith('-')) {
            $index += if ($script:CommandLineXargsValueOptions -ccontains $rest[$index]) { 2 } else { 1 }
        }
        if ($index -lt $rest.Count) {
            $result.Kind = 'Words'; $result.Words = [string[]]@($rest | Select-Object -Skip $index); return $result
        }
    } else {
        return $result
    }

    $result.Opaque = $true
    $result.OpaqueReason = 'NoPayload'
    return $result
}

function Get-CommandLineSubstitutionBody {
    <#
    .SYNOPSIS
        Return the content of each double-quoted span that contains '$(' or a backtick.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Text)

    $bodies = [System.Collections.Generic.List[string]]::new()
    $span = [System.Text.StringBuilder]::new()
    $quote = [char]0
    for ($index = 0; $index -lt $Text.Length; $index++) {
        $character = $Text[$index]
        if ($quote -eq "'") {
            if ($character -eq "'") { $quote = [char]0 }
            continue
        }
        if ($character -eq '\' -and $index + 1 -lt $Text.Length) {
            if ($quote -eq '"') { [void]$span.Append($character).Append($Text[$index + 1]) }
            $index++
            continue
        }
        if ($quote -eq '"') {
            if ($character -ne '"') { [void]$span.Append($character); continue }
            $quote = [char]0
            $body = $span.ToString()
            if ($body.Contains('$(') -or $body.Contains('`')) { $bodies.Add($body) }
            continue
        }
        if ($character -eq "'") { $quote = $character }
        if ($character -eq '"') { $quote = $character; [void]$span.Clear() }
    }
    return [string[]]$bodies.ToArray()
}

function Add-CommandLineInvocationRecord {
    <#
    .SYNOPSIS
        Build one iterator record, append it to the context's record list, and return it.
        PresenceText follows design item DC-9: TopLevel uses RawText, WrapperPayload uses
        RootRawText, Substitution uses SubstitutionPresence, ArgumentInjector uses the whole
        normalized command text.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)][hashtable] $Context, [Parameter(Mandatory)][int] $Depth,
        [Parameter(Mandatory)][string] $Origin, [Parameter(Mandatory)][AllowEmptyString()][string] $Wrapper,
        [Parameter(Mandatory)][string] $Dialect, [Parameter(Mandatory)][int] $RootIndex,
        [Parameter(Mandatory)][AllowEmptyString()][string] $RootRawText, [Parameter(Mandatory)][AllowEmptyString()][string] $RawText,
        [Parameter(Mandatory)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Tokens,
        [Parameter(Mandatory)][AllowEmptyString()][string] $CommandWord, [Parameter(Mandatory)][bool] $Literal,
        [Parameter(Mandatory)][bool] $Unbalanced, [Parameter(Mandatory)][AllowEmptyString()][string] $Delimiter,
        [Parameter(Mandatory)][bool] $ArgumentInjected, [AllowEmptyString()][string] $SubstitutionPresence = ''
    )

    $index = $Context.Records.Count
    $rootIndexValue = if ($RootIndex -lt 0) { $index } else { $RootIndex }
    $rootRaw = if ($RootIndex -lt 0) { $RawText } else { $RootRawText }
    $presence = switch ($Origin) {
        'TopLevel' { $RawText }
        'WrapperPayload' { $rootRaw }
        'Substitution' { $SubstitutionPresence }
        default { $Context.NormalizedText }
    }
    $record = [pscustomobject]@{
        Index = $index; RootIndex = $rootIndexValue; Depth = $Depth; Origin = $Origin; Wrapper = $Wrapper
        Dialect = $Dialect; RawText = $RawText; RootRawText = $rootRaw; Tokens = [string[]]$Tokens
        CommandWord = $CommandWord; Literal = $Literal; Unbalanced = $Unbalanced; Opaque = $false
        OpaqueReason = ''; Delimiter = $Delimiter; PresenceText = $presence; ArgumentInjected = $ArgumentInjected
    }
    $Context.Records.Add($record)
    return $record
}

function Add-CommandLineWrapperExpansion {
    <#
    .SYNOPSIS
        Expand the wrapper payload of one record into child records, or mark the record Opaque.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][hashtable] $Context,
        [Parameter(Mandatory)][pscustomobject] $Record,
        [Parameter(Mandatory)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Word
    )

    $payload = Get-CommandLineWrapperPayload -Word $Word
    if ([string]::IsNullOrEmpty($payload.Wrapper)) { return }
    if ($payload.Opaque -or $Record.Depth + 1 -gt $Context.MaxDepth) {
        $Record.Opaque = $true
        $Record.OpaqueReason = if ($payload.Opaque) { $payload.OpaqueReason } else { 'DepthLimit' }
        return
    }

    $child = @{
        Context = $Context; Depth = $Record.Depth + 1; Wrapper = $payload.Wrapper; RootIndex = $Record.RootIndex
        RootRawText = $Record.RootRawText; ArgumentInjected = [bool]$Record.ArgumentInjected
    }
    switch ($payload.Kind) {
        'Posix' { Add-CommandLinePosixRecord @child -Origin 'WrapperPayload' -Text $payload.Payload }
        'PowerShell' { Add-CommandLinePowerShellRecord @child -Text $payload.Payload }
        'Words' {
            $words = [string[]]$payload.Words
            $child.ArgumentInjected = $true
            $injected = Add-CommandLineInvocationRecord @child -Origin 'ArgumentInjector' -Dialect 'Posix' -RawText ($words -join ' ') `
                -Tokens $words -CommandWord (ConvertTo-CommandLineLeafWord -Word $words[0]) -Literal (Test-CommandLinePosixTokenLiteral -Token $words) `
                -Unbalanced $false -Delimiter ''
            Add-CommandLineWrapperExpansion -Context $Context -Record $injected -Word $words
        }
    }
}

function Test-CommandLinePosixTokenLiteral {
    <#
    .SYNOPSIS
        Report whether no token carries a '$' or a backtick.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Token)

    foreach ($current in $Token) {
        if ($current.Contains('$') -or $current.Contains('`')) { return $false }
    }
    return $true
}

function Add-CommandLinePosixRecord {
    <#
    .SYNOPSIS
        Segment POSIX text and append one record per segment, expanding wrappers and
        substitution bodies in pre-order.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][hashtable] $Context, [Parameter(Mandatory)][AllowEmptyString()][string] $Text,
        [Parameter(Mandatory)][int] $Depth, [Parameter(Mandatory)][string] $Origin,
        [Parameter(Mandatory)][AllowEmptyString()][string] $Wrapper, [Parameter(Mandatory)][int] $RootIndex,
        [Parameter(Mandatory)][AllowEmptyString()][string] $RootRawText, [Parameter(Mandatory)][bool] $ArgumentInjected
    )

    $segments = @(Read-CommandLineSegment -CommandText (ConvertTo-CommandLineNormalizedText -CommandText $Text))
    $presence = (@($segments | ForEach-Object { $_.TokenText }) -join ' ')
    foreach ($segment in $segments) {
        $tokens = [string[]]@($segment.Tokens)
        $record = Add-CommandLineInvocationRecord -Context $Context -Depth $Depth -Origin $Origin -Wrapper $Wrapper -Dialect 'Posix' `
            -RootIndex $RootIndex -RootRawText $RootRawText -RawText $segment.RawText -Tokens $tokens `
            -CommandWord (ConvertTo-CommandLineLeafWord -Word $segment.CommandWord) -Literal (Test-CommandLinePosixTokenLiteral -Token $tokens) `
            -Unbalanced ([bool]$segment.Unbalanced) -Delimiter $segment.Delimiter -ArgumentInjected $ArgumentInjected -SubstitutionPresence $presence
        Add-CommandLineWrapperExpansion -Context $Context -Record $record -Word (Split-CommandLinePosixWord -Text $segment.TokenText)
        if (-not $segment.HasLiveSubstitution) { continue }
        foreach ($body in @(Get-CommandLineSubstitutionBody -Text $segment.TokenText)) {
            if ($Depth + 1 -gt $Context.MaxDepth) {
                $record.Opaque = $true
                $record.OpaqueReason = 'DepthLimit'
                break
            }
            Add-CommandLinePosixRecord -Context $Context -Text $body -Depth ($Depth + 1) -Origin 'Substitution' -Wrapper '' `
                -RootIndex $record.RootIndex -RootRawText $record.RootRawText -ArgumentInjected ([bool]$record.ArgumentInjected)
        }
    }
}

function Read-CommandLineInvocationSegment {
    <#
    .SYNOPSIS
        Return one record per command visible in a command line, including wrapper payloads,
        substitution bodies, and xargs-injected commands, in pre-order source order.
    .PARAMETER CommandText
        The raw Bash command text.
    .PARAMETER MaxDepth
        The deepest payload level expanded. A wrapper whose payload would exceed it is Opaque
        with reason DepthLimit and yields no payload records.
    .OUTPUTS
        System.Management.Automation.PSCustomObject[] - each record carries Index, RootIndex,
        Depth, Origin ('TopLevel', 'WrapperPayload', 'Substitution', 'ArgumentInjector'),
        Wrapper, Dialect ('Posix', 'PowerShell'), RawText, RootRawText, Tokens, CommandWord
        (leaf-normalized), Literal, Unbalanced, Opaque, OpaqueReason ('', 'NoPayload',
        'DecodeFailure', 'DepthLimit'), Delimiter, PresenceText, and ArgumentInjected.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject[]])]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string] $CommandText,
        [ValidateRange(0, 64)][int] $MaxDepth = 4
    )

    $records = [System.Collections.Generic.List[pscustomobject]]::new()
    if ([string]::IsNullOrWhiteSpace($CommandText)) { return $records.ToArray() }
    $normalized = ConvertTo-CommandLineNormalizedText -CommandText $CommandText
    $context = @{ Records = $records; MaxDepth = $MaxDepth; NormalizedText = $normalized }
    Add-CommandLinePosixRecord -Context $context -Text $normalized -Depth 0 -Origin 'TopLevel' -Wrapper '' -RootIndex -1 -RootRawText '' -ArgumentInjected $false
    return $records.ToArray()
}

function Test-CommandLinePayloadInert {
    <#
    .SYNOPSIS
        Report whether the payload of one root is proven inert (design item 8, D3).
    .DESCRIPTION
        True only when every record with that RootIndex and Depth >= 1 is Literal, neither
        Unbalanced nor Opaque, led by a command in the fixed sink allowlist, and carries no
        token containing '<' or '>'. A root with no payload records is not proven inert.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][object[]] $Record,
        [Parameter(Mandatory)][int] $RootIndex
    )

    $payload = @($Record | Where-Object { $_.RootIndex -eq $RootIndex -and $_.Depth -ge 1 })
    if ($payload.Count -eq 0) { return $false }
    foreach ($current in $payload) {
        if (-not $current.Literal -or $current.Unbalanced -or $current.Opaque) { return $false }
        if ($script:CommandLineInertSinkNames -notcontains (ConvertTo-CommandLineLeafWord -Word $current.CommandWord)) { return $false }
        if (@($current.Tokens | Where-Object { $_.Contains('<') -or $_.Contains('>') }).Count -gt 0) { return $false }
    }
    return $true
}
