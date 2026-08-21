<#
.SYNOPSIS
    Structural Mermaid diagram validator (issue #491).

.DESCRIPTION
    Public entry point `Test-MermaidDiagram -Content <string>` returns a structured
    result so a future CI-side deep check can be layered without changing the hook
    contract:

      Verdict      Valid | Invalid | NotJudged
      DiagramType  the resolved canonical type, the declared token when the token
                   did not resolve, or $null
      Findings     array of { Class; Line; Message } for each detected defect
      Warnings     array of strings, for example the keyword-drift warning

    The gate's contract is "rejects the named defect classes", NOT "proves
    validity". `Valid` means no defect of a checked class was found. The checked
    classes are: missing or clearly non-keyword first line, misspelled diagram
    keyword, malformed YAML frontmatter, empty or whitespace-only body,
    unbalanced `[]` / `()` / `{}`, unterminated double-quoted string, arrow token
    invalid for the declared type, and `subgraph`/`end` imbalance. Semantic and
    deep-grammar errors are outside its reach; see the feature research for the
    full "cannot catch" list.

    Fail-open policy, all of which allow rather than reject:
      1. A first-line token outside the allowlist but shaped like a plausible
         keyword warns and allows. This is the Mermaid version-drift safety valve.
         Only a missing or clearly non-keyword first line blocks.
      2. Diagram types outside the deep-checked set are keyword-checked only.
      3. A line the classifier cannot categorize is skipped, never rejected.
      4. A statement-keyword line is exempt from arrow and bracket judgement.
      5. A block-opening keyword line is exempt from arrow judgement.
      6. An unverified keyword-accept row warns and declines to judge the body.
      7. ZenUML bodies use an external plugin grammar and are keyword-checked only.

    Pinned to Mermaid 11.17.0 through MermaidGrammar.psm1. Every function is pure:
    no filesystem, subprocess, network, or wall-clock access, and no input is
    mutated. CRLF, CR, and LF inputs produce identical verdicts because line
    splitting is normalized once in MermaidMarkdownFences.psm1.
#>

Set-StrictMode -Version Latest

Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'MermaidGrammar.psm1') -Force
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'MermaidLineScanner.psm1') -Force
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'MermaidMarkdownFences.psm1') -Force

function Get-MermaidFinding {
    <#
    .SYNOPSIS
        Builds one finding record for the structured validation result.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [string] $Class,

        [Parameter(Mandatory)]
        [int] $Line,

        [Parameter(Mandatory)]
        [string] $Message
    )

    return [ordered]@{ Class = $Class; Line = $Line; Message = $Message }
}

function Get-MermaidFrontmatter {
    <#
    .SYNOPSIS
        Extracts a leading YAML frontmatter block from a diagram's lines.
    .DESCRIPTION
        Returns HasFrontmatter, IsMalformed (an opening `---` with no closing
        `---`), Keys (the top-level `key:` names found), and BodyStartIndex (the
        zero-based index of the first line after the frontmatter).

        Frontmatter is only recognized when the first non-blank line is exactly
        `---`, which is the documented Mermaid form.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]] $Line
    )

    $result = [ordered]@{
        HasFrontmatter = $false
        IsMalformed    = $false
        Keys           = [string[]]@()
        BodyStartIndex = 0
    }

    $first = 0
    while ($first -lt $Line.Count -and [string]::IsNullOrWhiteSpace($Line[$first])) {
        $first++
    }
    if ($first -ge $Line.Count -or $Line[$first].Trim() -ne '---') {
        return $result
    }

    $result.HasFrontmatter = $true
    $keys = [System.Collections.Generic.List[string]]::new()
    for ($index = $first + 1; $index -lt $Line.Count; $index++) {
        if ($Line[$index].Trim() -eq '---') {
            $result.Keys = [string[]]@($keys.ToArray())
            $result.BodyStartIndex = $index + 1
            return $result
        }
        $match = [regex]::Match($Line[$index], '^(?<key>[A-Za-z_][A-Za-z0-9_-]*)\s*:')
        if ($match.Success) {
            $keys.Add($match.Groups['key'].Value)
        }
    }

    $result.IsMalformed = $true
    $result.BodyStartIndex = $Line.Count
    return $result
}

function Test-MermaidManagedDiagram {
    <#
    .SYNOPSIS
        Returns $true when a diagram's frontmatter carries the Mermaid Chart `id:` marker.
    .DESCRIPTION
        `id:` in the frontmatter is what the Mermaid Chart extension writes when a
        diagram is connected to the cloud sync workflow. A diagram carrying it must
        not be hand-edited, so the hook uses this detector as its managed-diagram
        guard. An `id:` key with an empty value is not treated as a marker, because
        an unconnected placeholder should not lock the file.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowEmptyString()]
        [AllowNull()]
        [string] $Content
    )

    if ([string]::IsNullOrWhiteSpace($Content)) {
        return $false
    }

    $lines = @(Split-MermaidTextLine -Text $Content)
    $frontmatter = Get-MermaidFrontmatter -Line $lines
    if (-not $frontmatter.HasFrontmatter) {
        return $false
    }
    if ($frontmatter.Keys -notcontains 'id') {
        return $false
    }

    $limit = if ($frontmatter.IsMalformed) { $lines.Count } else { $frontmatter.BodyStartIndex - 1 }
    for ($index = 0; $index -lt $limit; $index++) {
        $match = [regex]::Match($lines[$index], '^\s*id\s*:\s*(?<value>.*)$')
        if ($match.Success -and -not [string]::IsNullOrWhiteSpace($match.Groups['value'].Value)) {
            return $true
        }
    }

    return $false
}

function Get-MermaidKeywordLineIndex {
    <#
    .SYNOPSIS
        Finds the index of the diagram keyword line within a body line range.
    .DESCRIPTION
        Skips blank lines, `%%` comments, and `%%{...}%%` directives, which may all
        precede the keyword. Returns -1 when no candidate line exists.
    #>
    [CmdletBinding()]
    [OutputType([int])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]] $Line,

        [Parameter(Mandatory)]
        [int] $StartIndex
    )

    for ($index = $StartIndex; $index -lt $Line.Count; $index++) {
        $scan = Get-MermaidLineScan -Line $Line[$index]
        if ($scan.IsBlank -or $scan.IsComment -or $scan.IsDirective) {
            continue
        }
        return $index
    }

    return -1
}

function Get-MermaidArrowFinding {
    <#
    .SYNOPSIS
        Returns arrow findings for one scanned line of a deep-checked diagram.
    #>
    [CmdletBinding()]
    [OutputType([object[]])]
    param(
        [Parameter(Mandatory)]
        [System.Collections.Specialized.OrderedDictionary] $Scan,

        [Parameter(Mandatory)]
        [string] $DiagramType,

        [Parameter(Mandatory)]
        [int] $LineNumber
    )

    $findings = [System.Collections.Generic.List[object]]::new()
    $pattern = Get-MermaidArrowPattern -DiagramType $DiagramType
    if ([string]::IsNullOrEmpty($pattern)) {
        return [object[]]@()
    }

    # Statement-keyword and block-opening lines carry URLs, CSS, and free text.
    if ($Scan.Class -eq 'StatementKeyword' -or (Test-MermaidNonEdgeKeyword -Token $Scan.FirstToken)) {
        return [object[]]@()
    }

    # For the types whose statements put free text after the first colon, arrow
    # judgement stops at the colon.
    $text = $Scan.ArrowText
    if ((Test-MermaidPostColonLabelType -DiagramType $DiagramType) -and $Scan.ColonIndex -ge 0) {
        $cut = [Math]::Min($Scan.ColonIndex, $text.Length)
        $text = $text.Substring(0, $cut)
    }

    foreach ($token in @(Get-MermaidArrowCandidate -Text $text)) {
        if ($token -match $pattern) {
            continue
        }
        $findings.Add((Get-MermaidFinding -Class 'InvalidArrowToken' -Line $LineNumber -Message "the token '$token' is not a valid edge form for a '$DiagramType' diagram"))
    }

    return [object[]]@($findings.ToArray())
}

function Get-MermaidBodyFinding {
    <#
    .SYNOPSIS
        Runs the deep structural checks over a deep-checked diagram's body.
    .DESCRIPTION
        Bracket balance is aggregated across the body rather than judged per line,
        because legal Mermaid opens a brace block on one line and closes it on
        another. Closers are clamped at zero and a closer without an opener is never
        a finding, because a statement-keyword line such as `class Animal {` is
        exempt from bracket counting while its closing `}` is not, and reporting
        that as a defect would reject valid class diagrams.
    #>
    [CmdletBinding()]
    [OutputType([object[]])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]] $Line,

        [Parameter(Mandatory)]
        [int] $BodyStartIndex,

        [Parameter(Mandatory)]
        [string] $DiagramType,

        [Parameter(Mandatory)]
        [int] $LineOffset
    )

    $findings = [System.Collections.Generic.List[object]]::new()
    $bracketStructural = Test-MermaidBracketStructuralType -DiagramType $DiagramType
    $postColon = Test-MermaidPostColonLabelType -DiagramType $DiagramType
    $openers = [ordered]@{
        Square = [System.Collections.Generic.List[int]]::new()
        Round  = [System.Collections.Generic.List[int]]::new()
        Curly  = [System.Collections.Generic.List[int]]::new()
    }
    $subgraphOpen = [System.Collections.Generic.List[int]]::new()

    for ($index = $BodyStartIndex; $index -lt $Line.Count; $index++) {
        $lineNumber = $index + 1 + $LineOffset
        $scan = Get-MermaidLineScan -Line $Line[$index]
        if ($scan.IsBlank -or $scan.IsComment -or $scan.IsDirective) {
            continue
        }

        foreach ($finding in @(Get-MermaidArrowFinding -Scan $scan -DiagramType $DiagramType -LineNumber $lineNumber)) {
            $findings.Add($finding)
        }

        if ($scan.Class -ne 'StatementKeyword') {
            $quoteText = $scan.Raw
            if ($postColon -and $scan.ColonIndex -ge 0) {
                $quoteText = $scan.Raw.Substring(0, [Math]::Min($scan.ColonIndex, $scan.Raw.Length))
            }
            if ((Get-MermaidLineScan -Line $quoteText).HasUnterminatedQuote) {
                $findings.Add((Get-MermaidFinding -Class 'UnterminatedQuote' -Line $lineNumber -Message 'a double-quoted label is not closed on this line'))
            }
        }

        if ($DiagramType -eq 'flowchart') {
            if ($scan.FirstToken -eq 'subgraph') {
                $subgraphOpen.Add($lineNumber)
            } elseif ($scan.FirstToken -eq 'end' -and $subgraphOpen.Count -gt 0) {
                $subgraphOpen.RemoveAt($subgraphOpen.Count - 1)
            }
        }

        if (-not $bracketStructural -or $scan.Class -eq 'StatementKeyword') {
            continue
        }

        foreach ($kind in @('Square', 'Round', 'Curly')) {
            $delta = $scan.BracketDelta[$kind]
            for ($count = 0; $count -lt $delta; $count++) {
                $openers[$kind].Add($lineNumber)
            }
            for ($count = 0; $count -gt $delta; $count--) {
                if ($openers[$kind].Count -gt 0) {
                    $openers[$kind].RemoveAt($openers[$kind].Count - 1)
                }
            }
        }
    }

    $bracketName = [ordered]@{ Square = '[]'; Round = '()'; Curly = '{}' }
    foreach ($kind in @('Square', 'Round', 'Curly')) {
        if ($openers[$kind].Count -gt 0) {
            $findings.Add((Get-MermaidFinding -Class 'UnbalancedBracket' -Line $openers[$kind][0] -Message "a '$($bracketName[$kind])' bracket opened here is never closed"))
        }
    }

    foreach ($lineNumber in $subgraphOpen) {
        $findings.Add((Get-MermaidFinding -Class 'UnclosedSubgraph' -Line $lineNumber -Message "the 'subgraph' opened here has no matching 'end'"))
    }

    return [object[]]@($findings.ToArray())
}

function Get-MermaidResult {
    <#
    .SYNOPSIS
        Builds the structured validation result from its parts.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [string] $Verdict,

        [AllowNull()]
        [AllowEmptyString()]
        [string] $DiagramType,

        [AllowEmptyCollection()]
        [object[]] $Findings = @(),

        [AllowEmptyCollection()]
        [string[]] $Warnings = @()
    )

    return [ordered]@{
        Verdict     = $Verdict
        DiagramType = $DiagramType
        Findings    = [object[]]@($Findings)
        Warnings    = [string[]]@($Warnings)
    }
}

function Test-MermaidDiagram {
    <#
    .SYNOPSIS
        Validates one Mermaid diagram and returns the structured result.
    .PARAMETER Content
        The full diagram text: optional YAML frontmatter, optional directives and
        comments, the diagram keyword line, and the body.
    .PARAMETER LineOffset
        Added to every reported line number. Callers validating a fenced block
        inside a larger document pass the block's body start line minus one so the
        reported numbers are file-relative.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [AllowEmptyString()]
        [AllowNull()]
        [string] $Content,

        [int] $LineOffset = 0
    )

    if ([string]::IsNullOrWhiteSpace($Content)) {
        return Get-MermaidResult -Verdict 'Invalid' -DiagramType $null -Findings @(
            (Get-MermaidFinding -Class 'EmptyDiagram' -Line (1 + $LineOffset) -Message 'the diagram is empty or contains only whitespace')
        )
    }

    $lines = @(Split-MermaidTextLine -Text $Content)
    $frontmatter = Get-MermaidFrontmatter -Line $lines
    if ($frontmatter.IsMalformed) {
        return Get-MermaidResult -Verdict 'Invalid' -DiagramType $null -Findings @(
            (Get-MermaidFinding -Class 'MalformedFrontmatter' -Line (1 + $LineOffset) -Message 'the YAML frontmatter opens with --- but is never closed by a matching ---')
        )
    }

    $keywordIndex = Get-MermaidKeywordLineIndex -Line $lines -StartIndex $frontmatter.BodyStartIndex
    if ($keywordIndex -lt 0) {
        return Get-MermaidResult -Verdict 'Invalid' -DiagramType $null -Findings @(
            (Get-MermaidFinding -Class 'MissingDiagramType' -Line (1 + $LineOffset) -Message 'no diagram-type keyword line was found after the frontmatter, directives, and comments')
        )
    }

    $keywordLineNumber = $keywordIndex + 1 + $LineOffset
    $resolved = Resolve-MermaidDiagramType -FirstLine $lines[$keywordIndex]

    if (-not $resolved.IsKnown) {
        if (-not $resolved.IsPlausibleKeyword) {
            return Get-MermaidResult -Verdict 'Invalid' -DiagramType $null -Findings @(
                (Get-MermaidFinding -Class 'MissingDiagramType' -Line $keywordLineNumber -Message "the first line must declare a diagram type, but it begins with '$($resolved.Token)', which is not a diagram-type keyword")
            )
        }

        # A near miss is a typo, not version drift, so it is named as a defect. The
        # single-edit radius is what keeps this from swallowing genuinely new
        # diagram types; see Resolve-MermaidMisspelledKeyword.
        if (-not [string]::IsNullOrEmpty($resolved.MisspelledOf)) {
            return Get-MermaidResult -Verdict 'Invalid' -DiagramType $resolved.Token -Findings @(
                (Get-MermaidFinding -Class 'MisspelledDiagramType' -Line $keywordLineNumber -Message "the first line declares '$($resolved.Token)', which is one character away from the diagram keyword '$($resolved.MisspelledOf)'. Correct the keyword spelling.")
            )
        }

        # Fail-open item 1: the version-drift safety valve. An out-of-date keyword
        # allowlist costs a warning, never a false rejection.
        return Get-MermaidResult -Verdict 'NotJudged' -DiagramType $resolved.Token -Warnings @(
            "'$($resolved.Token)' is not in the Mermaid $(Get-MermaidGrammarVersion) diagram-type allowlist. It is shaped like a diagram keyword, so the body was not judged. Confirm the keyword against the Mermaid documentation and update .claude/lib/mermaid/MermaidGrammar.psm1 if it is a newer diagram type."
        )
    }

    $diagramType = $resolved.Type
    if (-not $resolved.IsVerified) {
        # Fail-open item 6: the keyword resolves but its exact form was never
        # verified against the pinned documentation, so the body is not judged.
        return Get-MermaidResult -Verdict 'NotJudged' -DiagramType $diagramType -Warnings @(
            "'$($resolved.Token)' is a keyword-accept entry whose exact first-line form was not verified against the pinned Mermaid $(Get-MermaidGrammarVersion) documentation, so the body was not judged."
        )
    }

    if (-not (Test-MermaidDeepCheckedType -DiagramType $diagramType)) {
        # Fail-open items 2 and 7: free-text and plugin-backed grammars are
        # keyword-checked only.
        return Get-MermaidResult -Verdict 'Valid' -DiagramType $diagramType
    }

    $bodyStartIndex = $keywordIndex + 1
    $hasBody = $false
    for ($index = $bodyStartIndex; $index -lt $lines.Count; $index++) {
        $scan = Get-MermaidLineScan -Line $lines[$index]
        if (-not ($scan.IsBlank -or $scan.IsComment -or $scan.IsDirective)) {
            $hasBody = $true
            break
        }
    }
    if (-not $hasBody) {
        return Get-MermaidResult -Verdict 'Invalid' -DiagramType $diagramType -Findings @(
            (Get-MermaidFinding -Class 'EmptyDiagramBody' -Line $keywordLineNumber -Message "the '$diagramType' diagram declares a type but has no statements after the keyword line")
        )
    }

    $findings = @(Get-MermaidBodyFinding -Line $lines -BodyStartIndex $bodyStartIndex -DiagramType $diagramType -LineOffset $LineOffset)
    if ($findings.Count -gt 0) {
        return Get-MermaidResult -Verdict 'Invalid' -DiagramType $diagramType -Findings $findings
    }

    return Get-MermaidResult -Verdict 'Valid' -DiagramType $diagramType
}

# Get-MermaidFenceBlock and Split-MermaidTextLine are re-exported from the nested
# MermaidMarkdownFences module so a consumer that imports this one module gets the whole
# diagram-extraction surface. A nested module's commands are visible to this module only
# unless they are named here explicitly.
Export-ModuleMember -Function `
    Get-MermaidFenceBlock, `
    Split-MermaidTextLine, `
    Get-MermaidFinding, `
    Get-MermaidFrontmatter, `
    Test-MermaidManagedDiagram, `
    Get-MermaidKeywordLineIndex, `
    Get-MermaidArrowFinding, `
    Get-MermaidBodyFinding, `
    Get-MermaidResult, `
    Test-MermaidDiagram
