<#
.SYNOPSIS
    Write-intent extraction for the blast-radius library (issue #722).

.DESCRIPTION
    Destination-runtime PowerShell mirror of
    scripts/dev_tools/_blast_radius_write_intent.py. The rules narrow the text
    harvest of derivation and validation so that only write-intent tokens become
    radius entries, and they are active only when the truth table carries
    write_intent_extraction set to true:

      - W1 glob mention: a token containing * or ? is dropped.
      - W2 command span: every token of an inline span that splits into more
        than one word is dropped.
      - W3 read task: the tokens of a task attribution window (a task line plus
        the following non-task lines up to the next ATX heading) are dropped
        when the title starts with a read verb, after an optional bold label,
        and carries no write verb anywhere.
      - W4 root anchoring: a concrete token whose first segment, after a
        leading ./ is stripped, is not in path_roots and which is not a
        configured root surface is dropped; an empty path_roots disables W4.
      - W5 spec contracts only: the spec contributes contracts and no paths,
        and contract harvesting applies W1 and W2.
      - W6 placeholder stem: a concrete token whose final-component stem is in
        the placeholder-stem set is dropped.

    Each exported function names its Python counterpart:

      - Test-WriteIntentExtractionEnabled  config_write_intent_extraction
      - Get-ConfigPathRoot                 config_path_roots
      - Get-WriteIntentPlanPath            extract_write_intent_plan_paths
      - Get-WriteIntentSpecContract        extract_write_intent_contracts
      - Select-WriteIntentPathEntry        select_write_intent_path_entries
      - Get-PlanPathForConfig              select_plan_paths

    Parity notes for maintainers:
      - The three vocabularies below are pinned equal, member for member and in
        order, to the Python constants READ_VERBS, WRITE_VERBS, and
        PLACEHOLDER_STEMS by a Pester parity test that reads the Python source.
      - With the flag absent or false, Get-PlanPathForConfig returns exactly
        Get-PlanPaths, so every caller behaves as before (fail closed).
      - The rules only drop tokens the current classifier accepts; they never
        add one. Every function is pure: no filesystem, subprocess, network, or
        clock access, and no input is mutated.
      - This module imports the extraction, glob, and config leaves only, never
        the validation module or the facade, so no import cycle can form.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusExtraction.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusGlob.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'BlastRadiusConfig.psm1') -Force -ErrorAction Stop

# Truth-table keys read by this module.
$script:WriteIntentKey = 'write_intent_extraction'
$script:PathRootKey = 'path_roots'

# The three rule vocabularies, in the same order as the Python constants.
$script:WriteIntentReadVerb = @('Read', 'Verify', 'Confirm', 'Inspect', 'Review', 'Baseline')
$script:WriteIntentWriteVerb = @(
    'Fix', 'Write', 'Update', 'Edit', 'Add', 'Create', 'Delete', 'Remove',
    'Rename', 'Author', 'Append', 'Replace'
)
$script:WriteIntentPlaceholderStem = @(
    'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o',
    'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
    'foo', 'bar', 'baz', 'example', 'sample', 'placeholder'
)

# Plan and spec structure patterns, mirroring the extraction module's regexes.
$script:TaskPattern = [regex]::new('^- \[(?<state>[ xX])\] \[P(?<phase>\d+)-T(?<task>\d+)\] (?<title>.+)$')
$script:HeadingPattern = [regex]::new('^(?<hashes>#{1,6}) (?<title>.+)$')
$script:InlineCodeSpanPattern = [regex]::new('`([^`]+)`')
$script:LineSuffixPattern = [regex]::new(':\d+$')
$script:ContractLetterPattern = [regex]::new('[A-Za-z]')
$script:ContractHeadingKeyword = @('API', 'Interface', 'Contract', 'Surface')

# W3 title parsing: an optional leading bold label is skipped before the first
# word is read; write verbs match as whole words anywhere, case-insensitively.
$script:BoldLabelPattern = [regex]::new('^\*\*[^*]+\*\*\s*')
$script:FirstWordPattern = [regex]::new('^[A-Za-z]+')
$script:WriteVerbPattern = [regex]::new(
    '\b(?:' + ($script:WriteIntentWriteVerb -join '|') + ')\b',
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
$script:ReadVerbSet = [System.Collections.Generic.HashSet[string]]::new(
    [string[]]@($script:WriteIntentReadVerb | ForEach-Object { $_.ToLowerInvariant() }),
    [StringComparer]::Ordinal)
$script:PlaceholderStemSet = [System.Collections.Generic.HashSet[string]]::new(
    [string[]]$script:WriteIntentPlaceholderStem, [StringComparer]::Ordinal)


function Test-WriteIntentExtractionEnabled {
    <#
    .SYNOPSIS
        Read the write_intent_extraction flag strictly.

    .DESCRIPTION
        Port of config_write_intent_extraction. An absent or null key reads as
        false, which keeps current extraction.

    .PARAMETER Config
        Parsed config/blast-radius.json.

    .OUTPUTS
        System.Boolean. The flag value. Throws, naming the key, when the value is
        present and is not a boolean.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Config
    )

    $mapping = Get-RequiredMapping -Value $Config -FieldName 'config'
    if (-not $mapping.ContainsKey($script:WriteIntentKey) -or $null -eq $mapping[$script:WriteIntentKey]) {
        return $false
    }
    $value = $mapping[$script:WriteIntentKey]
    if ($value -isnot [bool]) {
        throw "config[""$($script:WriteIntentKey)""] must be a boolean, got $($value.GetType().Name)."
    }
    return $value
}

function Get-ConfigPathRoot {
    <#
    .SYNOPSIS
        Read the path_roots list strictly.

    .DESCRIPTION
        Port of config_path_roots. An absent or null key yields an empty list,
        which disables rule W4.

    .PARAMETER Config
        Parsed config/blast-radius.json.

    .OUTPUTS
        System.Object[]. First path segments, deduplicated and ordinally sorted.
        Throws, naming the key, when the value is not a list of non-blank
        strings.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Config
    )

    $mapping = Get-RequiredMapping -Value $Config -FieldName 'config'
    if (-not $mapping.ContainsKey($script:PathRootKey) -or $null -eq $mapping[$script:PathRootKey]) {
        return @()
    }
    return @(Get-RequiredStringList -Value $mapping[$script:PathRootKey] -FieldName "config[""$($script:PathRootKey)""]")
}

# Report whether a task title makes its attribution window a read task (W3).
function Test-ReadTaskTitle {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory = $true)] [AllowEmptyString()] [string] $Title)

    $unlabeled = $script:BoldLabelPattern.Replace($Title.Trim(), '', 1)
    $firstWord = $script:FirstWordPattern.Match($unlabeled)
    if (-not $firstWord.Success -or -not $script:ReadVerbSet.Contains($firstWord.Value.ToLowerInvariant())) {
        return $false
    }
    return -not $script:WriteVerbPattern.IsMatch($Title)
}

# Report whether an entry is a feature-folder glob, which is never dropped.
function Test-FeatureFolderGlob {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory = $true)] [string] $Entry)

    if (-not ($Entry.StartsWith('docs/features/', [System.StringComparison]::Ordinal) -and
            $Entry.EndsWith('/**', [System.StringComparison]::Ordinal))) {
        return $false
    }
    $body = $Entry.Substring(0, $Entry.Length - 3)
    return ($body.IndexOf('*') -lt 0 -and $body.IndexOf('?') -lt 0)
}

# Apply the token-level rules W1, W4, and W6 to one token; true means it survives.
function Test-WriteIntentToken {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)] [string] $Token,
        [AllowEmptyCollection()] [string[]] $RootSurface = @(),
        [AllowEmptyCollection()] [string[]] $PathRoot = @()
    )

    # W1: a wildcard marks a glob mention rather than a write claim.
    if ($Token.IndexOf('*') -ge 0 -or $Token.IndexOf('?') -ge 0) {
        return $false
    }

    # W4: enabled only by a non-empty root list; a configured root surface is a
    # repository-root file with no first directory segment to test.
    if ($PathRoot.Count -gt 0 -and $RootSurface -cnotcontains $Token) {
        $stripped = if ($Token.StartsWith('./', [System.StringComparison]::Ordinal)) { $Token.Substring(2) } else { $Token }
        if ($PathRoot -cnotcontains $stripped.Split('/', 2)[0]) {
            return $false
        }
    }

    # W6: the final component's stem, compared case-insensitively.
    $finalComponent = $script:LineSuffixPattern.Replace($Token.Substring($Token.LastIndexOf('/') + 1), '')
    $stem = if ($finalComponent.Contains('.')) { $finalComponent.Substring(0, $finalComponent.LastIndexOf('.')) } else { $finalComponent }
    return -not $script:PlaceholderStemSet.Contains($stem.ToLowerInvariant())
}

# Yield the token of every inline span that holds exactly one word (W2).
function Get-SingleWordToken {
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param([Parameter(Mandatory = $true)] [AllowEmptyString()] [string] $Line)

    $token = [System.Collections.Generic.List[string]]::new()
    foreach ($match in $script:InlineCodeSpanPattern.Matches($Line)) {
        $word = @($match.Groups[1].Value -split '\s+' | Where-Object { $_.Length -gt 0 })
        if ($word.Count -eq 1) {
            $token.Add($word[0])
        }
    }
    return @($token.ToArray())
}

function Get-WriteIntentPlanPath {
    <#
    .SYNOPSIS
        Extract plan paths under rules W1 through W4 and W6.

    .DESCRIPTION
        Port of extract_write_intent_plan_paths. A task line opens a window
        whose read status comes from its title; an ATX heading closes any
        window; every other line belongs to the open window. Tokens of a read
        window are dropped (W3); every other line is harvested under W1, W2, W4,
        and W6 after the current classifier accepts the token.

    .PARAMETER PlanText
        Full atomic-plan document text; may be empty.

    .PARAMETER RootSurface
        Configured separator-free root surfaces, forwarded to the classifier and
        exempt from W4.

    .PARAMETER PathRoot
        Configured first segments for W4; empty disables W4.

    .OUTPUTS
        System.Object[]. Surviving tokens, deduplicated and ordinally sorted.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $PlanText,
        [AllowEmptyCollection()] [string[]] $RootSurface = @(),
        [AllowEmptyCollection()] [string[]] $PathRoot = @()
    )

    $accepted = [System.Collections.Generic.List[string]]::new()
    $inReadWindow = $false
    foreach ($line in @(ConvertTo-NormalizedLine -Text $PlanText)) {
        $taskMatch = $script:TaskPattern.Match($line)
        if ($taskMatch.Success) {
            $inReadWindow = Test-ReadTaskTitle -Title $taskMatch.Groups['title'].Value
        } elseif ($script:HeadingPattern.IsMatch($line)) {
            $inReadWindow = $false
        }
        if ($inReadWindow) {
            continue
        }

        foreach ($token in @(Get-SingleWordToken -Line $line)) {
            if ($null -ne (Get-PathTokenKind -Token $token -RootSurface $RootSurface) -and
                (Test-WriteIntentToken -Token $token -RootSurface $RootSurface -PathRoot $PathRoot)) {
                $accepted.Add($token)
            }
        }
    }
    return @(Get-OrdinalSortedEntry -Entry $accepted.ToArray())
}

function Get-WriteIntentSpecContract {
    <#
    .SYNOPSIS
        Extract spec contract identifiers with W1 and W2 applied (W5).

    .DESCRIPTION
        Port of extract_write_intent_contracts. Identifiers are harvested only
        inside a section whose heading, or an ancestor heading, names an API,
        interface, contract, or surface; a multi-word span and a wildcard token
        contribute nothing.

    .PARAMETER SpecText
        Full feature spec.md document text; may be empty.

    .OUTPUTS
        System.Object[]. Identifiers carrying an ASCII letter and no separator,
        deduplicated and ordinally sorted.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $SpecText
    )

    $identifier = [System.Collections.Generic.List[string]]::new()
    $qualifyingDepth = 0
    foreach ($line in @(ConvertTo-NormalizedLine -Text $SpecText)) {
        $headingMatch = $script:HeadingPattern.Match($line)

        # A deeper heading stays inside the qualifying section; a heading at or
        # above that level ends it and is judged on its own title.
        if ($headingMatch.Success) {
            $level = $headingMatch.Groups['hashes'].Value.Length
            if ($qualifyingDepth -ne 0 -and $level -gt $qualifyingDepth) {
                continue
            }
            $title = $headingMatch.Groups['title'].Value
            $qualifyingDepth = 0
            foreach ($keyword in $script:ContractHeadingKeyword) {
                if ($title.IndexOf($keyword, [System.StringComparison]::Ordinal) -ge 0) {
                    $qualifyingDepth = $level
                    break
                }
            }
            continue
        }
        if ($qualifyingDepth -eq 0) {
            continue
        }

        foreach ($token in @(Get-SingleWordToken -Line $line)) {
            if ($token.IndexOf('/') -lt 0 -and $token.IndexOf('*') -lt 0 -and
                $token.IndexOf('?') -lt 0 -and $script:ContractLetterPattern.IsMatch($token)) {
                $identifier.Add($token)
            }
        }
    }
    return @(Get-OrdinalSortedEntry -Entry $identifier.ToArray())
}

function Select-WriteIntentPathEntry {
    <#
    .SYNOPSIS
        Apply the token-level rules W1, W4, and W6 to recorded radius entries.

    .DESCRIPTION
        Port of select_write_intent_path_entries. Normalization calls this
        before the mandate-read filter when the flag is true. The feature-folder
        glob is always kept.

    .PARAMETER Entry
        Recorded entries already accepted by the current classifier.

    .PARAMETER RootSurface
        Configured separator-free root surfaces.

    .PARAMETER PathRoot
        Configured first segments for W4.

    .OUTPUTS
        System.Object[]. Surviving entries, deduplicated and ordinally sorted.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]] $Entry,
        [AllowEmptyCollection()] [string[]] $RootSurface = @(),
        [AllowEmptyCollection()] [string[]] $PathRoot = @()
    )

    $kept = [System.Collections.Generic.List[string]]::new()
    foreach ($item in $Entry) {
        if ((Test-FeatureFolderGlob -Entry $item) -or
            (Test-WriteIntentToken -Token $item -RootSurface $RootSurface -PathRoot $PathRoot)) {
            $kept.Add($item)
        }
    }
    return @(Get-OrdinalSortedEntry -Entry $kept.ToArray())
}

function Get-PlanPathForConfig {
    <#
    .SYNOPSIS
        Select the plan-side extractor from the truth table's flag.

    .DESCRIPTION
        Port of select_plan_paths, the single selector shared by derivation and
        by validation rules V1 and V2, which keeps a derived radius passing V1
        and V2 against its own plan in both modes.

    .PARAMETER PlanText
        Full atomic-plan document text; may be empty.

    .PARAMETER Config
        Parsed config/blast-radius.json.

    .PARAMETER RootSurface
        Configured separator-free root surfaces.

    .OUTPUTS
        System.Object[]. The write-intent plan paths when the flag is true, and
        exactly Get-PlanPaths otherwise.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $PlanText,
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Config,
        [AllowEmptyCollection()] [string[]] $RootSurface = @()
    )

    if (Test-WriteIntentExtractionEnabled -Config $Config) {
        return @(Get-WriteIntentPlanPath -PlanText $PlanText -RootSurface $RootSurface `
                -PathRoot ([string[]]@(Get-ConfigPathRoot -Config $Config)))
    }
    return @(Get-PlanPaths -PlanText $PlanText -RootSurface $RootSurface)
}

Export-ModuleMember -Function `
    Test-WriteIntentExtractionEnabled, `
    Get-ConfigPathRoot, `
    Get-WriteIntentPlanPath, `
    Get-WriteIntentSpecContract, `
    Select-WriteIntentPathEntry, `
    Get-PlanPathForConfig
