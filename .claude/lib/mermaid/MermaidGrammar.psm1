<#
.SYNOPSIS
    Mermaid grammar reference data for the dependency-free structural validator.

.DESCRIPTION
    Data-only module holding the diagram-type keyword allowlist, the per-type
    arrow/edge token sets, the deep-checked type set, and the statement-keyword
    exemption list used by MermaidLineScanner.psm1 and MermaidValidation.psm1.
    Every export is a pure accessor: no filesystem, subprocess, network, or
    wall-clock access, and no input is mutated.

    Pinned documentation version: Mermaid 11.17.0.
    Source: https://mermaid.js.org/intro/syntax-reference.html
    (per-type pages under https://mermaid.js.org/syntax/ ; fetched 2026-08-19).

    Staleness is auditable from this header. Mermaid adds diagram types several
    times per year, so the allowlist is a snapshot, not a closed set: an
    out-of-date allowlist costs a drift warning, never a false rejection. The one
    exception is a near miss, documented at Resolve-MermaidMisspelledKeyword.

    `Verified = $true` means the keyword form was read from the pinned
    documentation. `Verified = $false` means the type appears in the 11.x
    documentation sidebar but its exact first-line keyword form was not verified,
    so it resolves with a drift warning and its body is not judged.

    Only the five deep-checked types (flowchart, sequence, class, state, ER) carry
    structural judgement; every other type is keyword-checked only, the fail-open
    policy for free-text and plugin-backed grammars. Brackets are structural only
    where they delimit node shapes or attribute blocks: a gantt task named
    `Deploy (phase 1` must never be blocked as unbalanced.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:MermaidGrammarVersion = '11.17.0'
$script:MermaidGrammarSourceUrl = 'https://mermaid.js.org/intro/syntax-reference.html'

# Statement-keyword lines carry URLs, CSS declarations, and free text. They are
# exempt from BOTH the arrow rules and the bracket-balance rules; only the
# quote-termination rule may still apply. Narrowing or extending this list changes
# false-positive behavior, so it is held in one place.
$script:MermaidStatementKeyword = @(
    'click', 'style', 'classDef', 'linkStyle', 'class', 'accTitle', 'accDescr', 'title'
)

# Block and declaration keywords that are never edge statements. They are exempt
# from the arrow rules only (their bracket characters still participate in the
# balance count, which is what keeps ER and state composite blocks correct).
# A longer list here can only make the validator more permissive, never stricter.
$script:MermaidNonEdgeKeyword = @(
    'subgraph', 'end', 'direction', 'state', 'note', 'Note', 'participant',
    'actor', 'loop', 'alt', 'else', 'opt', 'par', 'and', 'critical', 'break',
    'rect', 'activate', 'deactivate', 'autonumber', 'box', 'create', 'destroy',
    'namespace', 'cssClass', 'callback', 'link', 'links', 'properties',
    'details', 'section', 'requirement', 'element'
)

# Types whose statement lines put free-text labels after the first colon. Arrow
# and quote checks apply to the pre-colon segment only for these types.
$script:MermaidPostColonLabelType = @('sequence', 'class', 'state', 'er')

<#
    Arrow patterns are anchored regexes matched against a single candidate arrow
    token produced by Get-MermaidArrowCandidate. They are deliberately more
    permissive than the documented token list so that length variants (`---->`,
    `====>`, `-...->`) and the tilde runs used by class-diagram generics never
    produce a finding. The five deep entries below carry the full shape; the
    keyword-accept entries are expanded from a compact map because they share one
    fixed shape: keyword-checked only, brackets never structural, no arrow grammar.
#>
$script:MermaidDiagramType = [ordered]@{
    flowchart = [ordered]@{
        Keywords = @('flowchart', 'graph', 'flowchart-elk'); Verified = $true; Deep = $true; BracketStructural = $true
        ArrowTokens  = @('-->', '---', '-.->', '-.-', '==>', '===', '~~~', '--o', '--x', 'o--o', 'x--x', '<-->')
        ArrowPattern = '^(?:~+|[<ox]?-{2,}[>ox]?|<?={2,}>?|-\.+-?>?|\.+-?>?)$'
    }
    sequence  = [ordered]@{
        Keywords = @('sequenceDiagram'); Verified = $true; Deep = $true; BracketStructural = $false
        ArrowTokens  = @('->', '-->', '->>', '-->>', '<<->>', '<<-->>', '-x', '--x', '-)', '--)')
        ArrowPattern = '^(?:~+|(?:<<)?-{1,2}(?:>{1,2}|x|\)|\\|/)?)$'
    }
    class     = [ordered]@{
        Keywords = @('classDiagram', 'classDiagram-v2'); Verified = $true; Deep = $true; BracketStructural = $true
        ArrowTokens  = @('<|--', '--|>', '*--', '--*', 'o--', '--o', '-->', '<--', '--', '..>', '<..', '..|>', '<|..', '..')
        ArrowPattern = '^(?:~+|(?:<\|?|\*|o)?(?:-{2,}|\.{2,})(?:\|>|>|\*|o)?)$'
    }
    state     = [ordered]@{
        Keywords = @('stateDiagram-v2', 'stateDiagram'); Verified = $true; Deep = $true; BracketStructural = $true
        ArrowTokens  = @('-->')
        ArrowPattern = '^(?:~+|-{2,}>)$'
    }
    er        = [ordered]@{
        Keywords = @('erDiagram'); Verified = $true; Deep = $true; BracketStructural = $true
        ArrowTokens  = @('|o--o|', '||--||', '}o--o{', '}|--|{', '|o..o|', '}|..|{')
        ArrowPattern = '^(?:~+|(?:\|o|\|\||\}o|\}\|)(?:-{2}|\.{2})(?:o\||\|\||o\{|\|\{))$'
    }
}

# Keyword-accept types, verified against the pinned documentation. The map value is
# the documented first-line keyword form or forms. `packet` is the 11.17 keyword;
# `packet-beta` was the earlier form, retained as an alias so neither spelling
# costs a false rejection. The three types whose documentation lists edge tokens
# carry them for reference only (see the reference-token block after the expansion
# loop); no arrow judgement is ever performed on a non-deep type.
$script:MermaidVerifiedKeywordOnlyType = [ordered]@{
    journey      = @('journey')
    gantt        = @('gantt')
    pie          = @('pie')
    quadrant     = @('quadrantChart')
    requirement  = @('requirementDiagram')
    gitgraph     = @('gitGraph')
    mindmap      = @('mindmap')
    timeline     = @('timeline')
    zenuml       = @('zenuml')
    sankey       = @('sankey-beta')
    xychart      = @('xychart-beta')
    block        = @('block-beta')
    packet       = @('packet', 'packet-beta')
    kanban       = @('kanban')
    architecture = @('architecture-beta')
    radar        = @('radar-beta')
    treemap      = @('treemap-beta')
    c4           = @('C4Context', 'C4Container', 'C4Component', 'C4Dynamic', 'C4Deployment')
    info         = @('info')
}

# 11.x documentation-sidebar additions. The diagram types are documented but their
# exact first-line keyword forms were not individually verified, so they are
# keyword-accept only and always carry a drift warning.
$script:MermaidUnverifiedKeywordOnlyType = [ordered]@{
    swimlanes     = @('swimlanes')
    eventmodeling = @('eventmodeling')
    venn          = @('venn')
    ishikawa      = @('ishikawa')
    wardley       = @('wardley')
    cynefin       = @('cynefin')
    treeview      = @('treeView')
    railroad      = @('railroad', 'railroad-beta')
}

foreach ($tier in @(
        @{ Map = $script:MermaidVerifiedKeywordOnlyType; Verified = $true },
        @{ Map = $script:MermaidUnverifiedKeywordOnlyType; Verified = $false })) {
    foreach ($name in $tier.Map.Keys) {
        $script:MermaidDiagramType[$name] = [ordered]@{
            Keywords = @($tier.Map[$name]); Verified = $tier.Verified; Deep = $false; BracketStructural = $false
            ArrowTokens  = @()
            ArrowPattern = $null
        }
    }
}

# Documented edge tokens for the keyword-accept types whose pages list them. These
# are reference data only: ArrowPattern stays $null, so no arrow is ever judged.
$script:MermaidDiagramType['requirement'].ArrowTokens = @('->', '<-')
$script:MermaidDiagramType['block'].ArrowTokens = @('-->', '--')
$script:MermaidDiagramType['architecture'].ArrowTokens = @('--', '-->')

function Get-MermaidGrammarVersion {
    <#
    .SYNOPSIS
        Returns the pinned Mermaid documentation version this table snapshots.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param()
    return $script:MermaidGrammarVersion
}

function Get-MermaidGrammarSourceUrl {
    <#
    .SYNOPSIS
        Returns the documentation URL the pinned grammar table was read from.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param()
    return $script:MermaidGrammarSourceUrl
}

function Get-MermaidDiagramTypeName {
    <#
    .SYNOPSIS
        Returns the canonical diagram-type names in table order.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param()
    return [string[]]@($script:MermaidDiagramType.Keys)
}

function Get-MermaidDiagramTypeEntry {
    <#
    .SYNOPSIS
        Returns the grammar entry for a canonical diagram-type name, or $null.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $DiagramType)

    if (-not $script:MermaidDiagramType.Contains($DiagramType)) { return $null }

    return $script:MermaidDiagramType[$DiagramType]
}

function Get-MermaidDeepCheckedType {
    <#
    .SYNOPSIS
        Returns the diagram types that receive structural judgement beyond the
        first-line keyword check.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param()

    $names = [System.Collections.Generic.List[string]]::new()
    foreach ($name in $script:MermaidDiagramType.Keys) {
        if ($script:MermaidDiagramType[$name].Deep) {
            $names.Add($name)
        }
    }

    return [string[]]@($names.ToArray())
}

function Test-MermaidDeepCheckedType {
    <#
    .SYNOPSIS
        Returns $true when the supplied canonical diagram type is deep-checked.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $DiagramType)

    $entry = Get-MermaidDiagramTypeEntry -DiagramType $DiagramType
    if ($null -eq $entry) { return $false }

    return [bool]$entry.Deep
}

function Test-MermaidBracketStructuralType {
    <#
    .SYNOPSIS
        Returns $true when brackets are structural for the supplied diagram type.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $DiagramType)

    $entry = Get-MermaidDiagramTypeEntry -DiagramType $DiagramType
    if ($null -eq $entry) { return $false }

    return [bool]$entry.BracketStructural
}

function Test-MermaidPostColonLabelType {
    <#
    .SYNOPSIS
        Returns $true when the diagram type puts free text after the first colon.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $DiagramType)

    return $script:MermaidPostColonLabelType -contains $DiagramType
}

function Get-MermaidArrowToken {
    <#
    .SYNOPSIS
        Returns the documented arrow/edge token set for a diagram type.
    .DESCRIPTION
        The documentation reference set. Validation uses Get-MermaidArrowPattern,
        which additionally admits the documented length variants.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $DiagramType)

    $entry = Get-MermaidDiagramTypeEntry -DiagramType $DiagramType
    if ($null -eq $entry) { return [string[]]@() }

    return [string[]]@($entry.ArrowTokens)
}

function Get-MermaidArrowPattern {
    <#
    .SYNOPSIS
        Returns the anchored regex accepting a valid arrow token for a type.
    .DESCRIPTION
        Returns $null when the type carries no arrow grammar, which the caller must
        treat as "do not judge arrows" rather than "reject all arrows".
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $DiagramType)

    $entry = Get-MermaidDiagramTypeEntry -DiagramType $DiagramType
    if ($null -eq $entry) { return $null }

    return $entry.ArrowPattern
}

function Get-MermaidStatementKeyword {
    <#
    .SYNOPSIS
        Returns the statement keywords exempt from arrow and bracket rules.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param()
    return [string[]]@($script:MermaidStatementKeyword)
}

function Test-MermaidStatementKeyword {
    <#
    .SYNOPSIS
        Returns $true when a first token is an exempt statement keyword.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([AllowEmptyString()][string] $Token)

    if ([string]::IsNullOrEmpty($Token)) { return $false }

    return $script:MermaidStatementKeyword -contains $Token
}

function Test-MermaidNonEdgeKeyword {
    <#
    .SYNOPSIS
        Returns $true when a first token opens a block or declaration.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([AllowEmptyString()][string] $Token)

    if ([string]::IsNullOrEmpty($Token)) { return $false }

    return $script:MermaidNonEdgeKeyword -contains $Token
}

function Test-MermaidPlausibleKeyword {
    <#
    .SYNOPSIS
        Returns $true when a token is shaped like a Mermaid diagram keyword.
    .DESCRIPTION
        The shape rule is the version-drift safety valve: a token of letters,
        digits, and hyphens beginning with a letter is treated as a plausible
        keyword newer than the pinned allowlist. Only a missing or clearly
        non-keyword first line is rejected outright.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([AllowEmptyString()][string] $Token)

    if ([string]::IsNullOrEmpty($Token)) { return $false }

    return [bool]($Token -cmatch '^[A-Za-z][A-Za-z0-9-]*$')
}

function Test-MermaidSingleEditDistance {
    <#
    .SYNOPSIS
        Returns $true when two tokens differ by exactly one character edit.
    .DESCRIPTION
        One edit means one substitution (equal lengths, exactly one differing
        position) or one insertion or deletion (lengths differing by one, where
        removing one character from the longer yields the shorter). Comparison is
        ordinal and case-sensitive, matching Mermaid keyword resolution.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string] $First,
        [Parameter(Mandatory)][AllowEmptyString()][string] $Second
    )

    if ([Math]::Abs($First.Length - $Second.Length) -gt 1) { return $false }

    if ($First.Length -eq $Second.Length) {
        $mismatch = 0
        for ($index = 0; $index -lt $First.Length; $index++) {
            if ($First[$index] -cne $Second[$index]) {
                $mismatch++
            }
            if ($mismatch -gt 1) { return $false }
        }
        return ($mismatch -eq 1)
    }

    $longer = if ($First.Length -gt $Second.Length) { $First } else { $Second }
    $shorter = if ($First.Length -gt $Second.Length) { $Second } else { $First }
    for ($index = 0; $index -lt $longer.Length; $index++) {
        if ([string]::Equals($longer.Remove($index, 1), $shorter, [System.StringComparison]::Ordinal)) { return $true }
    }

    return $false
}

function Resolve-MermaidMisspelledKeyword {
    <#
    .SYNOPSIS
        Returns the known keyword a token appears to misspell, or $null.
    .DESCRIPTION
        Only tokens of five or more characters are considered, and only a
        single-edit difference counts. The narrow radius is deliberate: a wider one
        would begin catching genuinely new diagram types and convert the
        version-drift warn-and-allow valve into a false rejection. A typo, by
        contrast, is a defect the gate is required to name (`flowchar` for
        `flowchart`).
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([AllowEmptyString()][string] $Token)

    if ([string]::IsNullOrEmpty($Token) -or $Token.Length -lt 5) { return $null }

    foreach ($name in $script:MermaidDiagramType.Keys) {
        foreach ($keyword in $script:MermaidDiagramType[$name].Keywords) {
            if ($keyword.Length -lt 5) {
                continue
            }
            if (Test-MermaidSingleEditDistance -First $Token -Second $keyword) { return $keyword }
        }
    }

    return $null
}

function Resolve-MermaidDiagramType {
    <#
    .SYNOPSIS
        Resolves the declared diagram type from a candidate first line.
    .DESCRIPTION
        Takes the first whitespace-delimited token of the trimmed line, strips a
        trailing colon or semicolon (the `gitGraph LR:` and `graph TD;` forms), and
        looks it up with ordinal, case-sensitive comparison because Mermaid
        keywords are case-sensitive (`C4Context`, `stateDiagram-v2`).

        Returns Token (the normalized first token), Type (canonical name or $null),
        IsKnown, IsVerified, IsPlausibleKeyword, and MisspelledOf (the keyword an
        unresolved token appears to misspell, or $null).
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param([AllowEmptyString()][string] $FirstLine)

    $token = ''
    if (-not [string]::IsNullOrWhiteSpace($FirstLine)) {
        $token = (($FirstLine.Trim() -split '\s+', 2)[0]) -replace '[:;]+$', ''
    }

    $result = [ordered]@{
        Token              = $token
        Type               = $null
        IsKnown            = $false
        IsVerified         = $false
        IsPlausibleKeyword = (Test-MermaidPlausibleKeyword -Token $token)
        MisspelledOf       = $null
    }

    if ([string]::IsNullOrEmpty($token)) { return $result }

    foreach ($name in $script:MermaidDiagramType.Keys) {
        $entry = $script:MermaidDiagramType[$name]
        foreach ($keyword in $entry.Keywords) {
            if ([string]::Equals($keyword, $token, [System.StringComparison]::Ordinal)) {
                $result.Type = $name
                $result.IsKnown = $true
                $result.IsVerified = [bool]$entry.Verified
                return $result
            }
        }
    }

    if ($result.IsPlausibleKeyword) {
        $result.MisspelledOf = Resolve-MermaidMisspelledKeyword -Token $token
    }

    return $result
}

Export-ModuleMember -Function @(
    'Get-MermaidGrammarVersion', 'Get-MermaidGrammarSourceUrl', 'Get-MermaidDiagramTypeName',
    'Get-MermaidDiagramTypeEntry', 'Get-MermaidDeepCheckedType', 'Test-MermaidDeepCheckedType',
    'Test-MermaidBracketStructuralType', 'Test-MermaidPostColonLabelType', 'Get-MermaidArrowToken',
    'Get-MermaidArrowPattern', 'Get-MermaidStatementKeyword', 'Test-MermaidStatementKeyword',
    'Test-MermaidNonEdgeKeyword', 'Test-MermaidPlausibleKeyword', 'Test-MermaidSingleEditDistance',
    'Resolve-MermaidMisspelledKeyword', 'Resolve-MermaidDiagramType'
)
