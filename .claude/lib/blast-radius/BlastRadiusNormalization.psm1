<#
.SYNOPSIS
    Blast-radius normalization: contract extraction, module resolution, and the
    read-by-mandate exclusion.

.DESCRIPTION
    Destination-runtime PowerShell port of
    scripts/dev_tools/_blast_radius_normalization.py, plus the two functions
    relocated here so their source modules stay inside the 500-line limit
    (issue #489): Get-ContractIdentifier from BlastRadiusExtraction.psm1 and
    Resolve-BlastRadiusModule from BlastRadiusConfig.psm1.

    The Python modules remain the authoritative reference implementation. This
    module is one half of a two-language mirror; it never imports validator
    logic. Every function is pure: no filesystem, subprocess, network, or
    wall-clock access, and no input is mutated.

    Parity notes for maintainers:
      - Get-ContractIdentifier carries the module-scoped variables it reads
        ($script:HeadingPattern, $script:ContractHeadingKeyword, and
        $script:NoQualifyingHeadingDepth). PowerShell scopes $script: per module,
        so a function-only relocation would resolve them to $null at runtime.
      - Test-MandateRead applies exact ordinal equality first, which is the only
        rule that can settle a glob entry, then glob containment for a concrete
        entry only. A glob entry is never tested for containment in another glob,
        matching matches_mandate_read.
      - An empty mandate-read collection excludes nothing, so a truth table with
        no mandate_reads key reproduces pre-change behaviour exactly.
      - Every returned collection is deduplicated and ordinally sorted.
#>

Set-StrictMode -Version Latest

Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusExtraction.psm1') -Force
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusGlob.psm1') -Force
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusConfig.psm1') -Force

# Markdown ATX heading pattern used to locate spec interface sections. Relocated
# with Get-ContractIdentifier: $script: scope is per module, so the variable must
# travel with its only consumer.
$script:HeadingPattern = [regex]::new('^(?<hashes>#{1,6}) (?<title>.+)$')

# A spec section qualifies as an interface section when its heading, or the
# heading of an ancestor section, contains one of these words.
$script:ContractHeadingKeyword = @('API', 'Interface', 'Contract', 'Surface')

# Heading depth sentinel standing in for the Python `qualifying_depth is None`
# state: markdown heading levels are 1..6, so 0 can never be a real level.
$script:NoQualifyingHeadingDepth = 0

# A contract identifier names something callable or referenceable, so it must
# carry at least one ASCII letter. Punctuation-only tokens such as -> or a bare
# digit are notation from an interface example rather than a contract, and
# admitting them made unrelated specs contend (issue #489).
$script:ContractLetterPattern = [regex]::new('[A-Za-z]')


function Get-ContractIdentifier {
    <#
    .SYNOPSIS
        Extract contract identifiers from a spec's interface sections.

    .DESCRIPTION
        Port of extract_contract_identifiers. Implements the contracts level of
        the radius model: exported symbols, schema names, and CLI identifiers
        named in inline code inside sections whose heading, or an ancestor
        heading, contains API, Interface, Contract, or Surface. Markdown sections
        nest, so a heading deeper than the innermost qualifying heading stays
        inside that section and inherits its qualification; a heading at or above
        that level ends the section and is judged on its own title.

    .PARAMETER SpecText
        Full feature spec.md document text; may be empty.

    .OUTPUTS
        System.Object[]. Identifiers, deduplicated and ordinally sorted. Tokens
        containing a separator are excluded as path references, as are tokens
        carrying no ASCII letter.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $SpecText
    )

    $identifier = [System.Collections.Generic.List[string]]::new()
    $qualifyingDepth = $script:NoQualifyingHeadingDepth

    foreach ($line in @(ConvertTo-NormalizedLine -Text $SpecText)) {
        $headingMatch = $script:HeadingPattern.Match($line)

        # A heading changes the section context and contributes no identifiers of
        # its own, so each heading is handled and the line is then skipped.
        if ($headingMatch.Success) {
            $headingLevel = $headingMatch.Groups['hashes'].Value.Length
            if ($qualifyingDepth -ne $script:NoQualifyingHeadingDepth -and
                $headingLevel -gt $qualifyingDepth) {
                continue
            }

            $headingTitle = $headingMatch.Groups['title'].Value
            $qualifyingDepth = $script:NoQualifyingHeadingDepth
            foreach ($keyword in $script:ContractHeadingKeyword) {
                if ($headingTitle.IndexOf($keyword, [System.StringComparison]::Ordinal) -ge 0) {
                    $qualifyingDepth = $headingLevel
                    break
                }
            }
            continue
        }

        if ($qualifyingDepth -eq $script:NoQualifyingHeadingDepth) {
            continue
        }

        # Inside a qualifying section an inline-code token without a separator is
        # a contract identifier; a token with one is a path reference and is
        # recorded at the paths level instead. A token carrying no ASCII letter
        # is notation, not an identifier, and is dropped (issue #489).
        foreach ($token in @(Get-InlineCodeToken -Line $line)) {
            if ($token.IndexOf('/') -lt 0 -and
                $script:ContractLetterPattern.IsMatch($token)) {
                $identifier.Add($token)
            }
        }
    }

    return @(Get-OrdinalSortedEntry -Entry $identifier.ToArray())
}

function Resolve-BlastRadiusModule {
    <#
    .SYNOPSIS
        Resolve path entries to the module names of the truth-table map.

    .DESCRIPTION
        Port of resolve_modules. A module joins the radius as soon as one of its
        globs covers one entry, so the search stops at the first hit per module.
        A path matching no glob resolves to no module.

    .PARAMETER PathEntry
        Concrete paths and globs of a radius. An empty collection is accepted.

    .PARAMETER Config
        Parsed config/blast-radius.json.

    .OUTPUTS
        System.Object[]. Matched module names, deduplicated and ordinally sorted.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]] $PathEntry,
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Config
    )

    $matched = [System.Collections.Generic.List[string]]::new()
    foreach ($pair in @(Get-ConfigModuleEntry -Config $Config)) {
        foreach ($pattern in $pair['globs']) {
            $hit = $false
            foreach ($entry in $PathEntry) {
                if (Test-GlobMatch -Pattern $pattern -Candidate $entry) {
                    $matched.Add([string]$pair['name'])
                    $hit = $true
                    break
                }
            }
            if ($hit) {
                break
            }
        }
    }

    return @(Get-OrdinalSortedEntry -Entry $matched.ToArray())
}


function Test-MandateRead {
    <#
    .SYNOPSIS
        Report whether one radius entry is a read-by-mandate citation.

    .DESCRIPTION
        Port of matches_mandate_read. Exact ordinal equality settles both a
        concrete path listed verbatim and a glob entry that repeats a configured
        glob character for character. Glob containment then covers a concrete
        path falling inside a configured subtree pattern such as artifacts/**. A
        glob entry is deliberately never tested for containment in another glob:
        deciding whether one pattern subsumes another is not a comparison this
        repository's glob vocabulary supports, and guessing would silently drop a
        genuine claim.

    .PARAMETER Entry
        One harvested or extracted radius entry: a concrete repository-relative
        path or a glob pattern.

    .PARAMETER MandateRead
        Configured mandate-read patterns from Get-ConfigMandateRead. An empty
        collection matches nothing.

    .OUTPUTS
        System.Boolean. True when the entry must be excluded from contention.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $Entry,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]] $MandateRead
    )

    foreach ($pattern in $MandateRead) {
        if ([string]::Equals($Entry, $pattern, [System.StringComparison]::Ordinal)) {
            return $true
        }
    }

    # A glob entry that did not match exactly is left alone; only a concrete path
    # is tested for containment in a configured subtree pattern.
    if (Test-GlobEntry -Entry $Entry) {
        return $false
    }

    foreach ($pattern in $MandateRead) {
        if (Test-GlobMatch -Pattern $pattern -Candidate $Entry) {
            return $true
        }
    }

    return $false
}

function Get-NonMandateReadEntry {
    <#
    .SYNOPSIS
        Drop every read-by-mandate citation from a collection of radius entries.

    .DESCRIPTION
        Port of exclude_mandate_reads. Every agent is instructed to read the
        policy rules, the tier map, and the process artifacts before doing any
        work, so a citation of one of those paths is evidence that the author
        obeyed the reading order rather than evidence that the change will write
        the file. Counting such citations as contention made thematically
        unrelated work items collide (issue #489).

    .PARAMETER Entry
        Harvested or extracted radius entries. An empty collection is accepted.

    .PARAMETER MandateRead
        Configured mandate-read patterns from Get-ConfigMandateRead. An empty
        collection excludes nothing, so the returned content equals the input
        content.

    .OUTPUTS
        System.Object[]. Surviving entries, deduplicated and ordinally sorted.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]] $Entry,
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]] $MandateRead
    )

    $survivor = [System.Collections.Generic.List[string]]::new()
    foreach ($candidate in $Entry) {
        if (-not (Test-MandateRead -Entry $candidate -MandateRead $MandateRead)) {
            $survivor.Add($candidate)
        }
    }

    return @(Get-OrdinalSortedEntry -Entry $survivor.ToArray())
}

Export-ModuleMember -Function `
    Get-ContractIdentifier, `
    Resolve-BlastRadiusModule, `
    Test-MandateRead, `
    Get-NonMandateReadEntry
