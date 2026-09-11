<#
.SYNOPSIS
    Line grammar for the mechanically-mergeable project-file shapes.

.DESCRIPTION
    Classifies a project file into one of the three kinds the merge step admits
    and parses one side of a conflict hunk into ordered, keyed units (issue #643).

    The grammar is deliberately narrow: a side is parsed only when every one of
    its lines is an admitted shape, and anything else yields $null, which the
    caller turns into an escalation. Lines are matched as opaque text and no XML
    API is used, so attribute order, indentation, self-closing spacing, and line
    terminators all survive a merge: a kept line is copied, not re-serialised.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# File extensions sharing the MSBuild item grammar, compared lower-case.
$script:MsBuildExtension = @('.csproj', '.vbproj', '.props')

# Attribute shape shared by every admitted element: name="value".
$script:AttributePattern = '([A-Za-z_][-A-Za-z0-9_.:]*)\s*=\s*"([^"]*)"'

# The five MSBuild item elements, single-line and paired-open. The single-line
# form is tested first, because the paired pattern also matches a self-closing
# line whose attribute run happens to end with a slash.
$script:MsBuildSinglePattern = '^\s*<(Compile|Analyzer|None|Content|EmbeddedResource)\s+([^<>]*?)\s*/>\s*$'
$script:MsBuildOpenPattern = '^\s*<(Compile|Analyzer|None|Content|EmbeddedResource)\s+([^<>]*?)\s*>\s*$'

# The metadata children a paired item may carry; any other child is ungrammatical.
$script:MsBuildChildPattern = '^\s*<(DependentUpon|SubType|AutoGen|DesignTime|Link|CopyToOutputDirectory|Generator|LastGenOutput)>[^<>]*</\1>\s*$'

# packages.config carries exactly one admitted element.
$script:PackagePattern = '^\s*<package\s+([^<>]*?)\s*/>\s*$'

# app.config binding redirects are keyed on the enclosing dependentAssembly block.
$script:AppConfigOpenPattern = '^\s*<dependentAssembly>\s*$'
$script:AppConfigClosePattern = '^\s*</dependentAssembly>\s*$'
$script:AppConfigChildPattern = '^\s*<(assemblyIdentity|bindingRedirect|publisherPolicy|codeBase)\s+([^<>]*?)\s*/>\s*$'

function Get-ProjectFileKind {
    <#
    .SYNOPSIS
        Classify a repository path into a merge grammar kind.
    .DESCRIPTION
        Returns 'packages', 'appconfig', 'msbuild', or $null. The two .config
        shapes are keyed on the whole leaf name because the .config extension is
        shared with files this grammar does not admit.
    .PARAMETER Path
        A repository-relative or absolute path; only its leaf name is read.
    .OUTPUTS
        System.String, or $null when the path is not a mergeable project file.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string] $Path)

    $leaf = [System.IO.Path]::GetFileName($Path)
    # Whole-name shapes settle first: .config alone would admit every other one.
    if ([string]::Equals($leaf, 'packages.config', [System.StringComparison]::OrdinalIgnoreCase)) { return 'packages' }
    if ([string]::Equals($leaf, 'app.config', [System.StringComparison]::OrdinalIgnoreCase)) { return 'appconfig' }

    # The three project shapes share one item grammar, so they share one kind.
    $extension = [System.IO.Path]::GetExtension($leaf).ToLowerInvariant()
    if ($script:MsBuildExtension -contains $extension) { return 'msbuild' }
    return $null
}

function Get-AttributeMap {
    <#
    .SYNOPSIS
        Read the name="value" attributes of one element's attribute run.
    .DESCRIPTION
        A repeated name keeps its first value, which is the value an XML reader
        would have seen before rejecting the duplicate.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string] $Text)

    $map = @{}
    foreach ($match in [regex]::Matches($Text, $script:AttributePattern)) {
        # First value wins, so a malformed duplicate cannot change the key.
        if (-not $map.ContainsKey($match.Groups[1].Value)) { $map[$match.Groups[1].Value] = $match.Groups[2].Value }
    }
    return $map
}

function Get-MsBuildUnit {
    <#
    .SYNOPSIS
        Parse MSBuild item lines into keyed units.
    .DESCRIPTION
        Accepts the single-line self-closing form and the paired open/close form
        whose only children are metadata elements; any other line yields $null.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param([Parameter(Mandatory = $true)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Line)

    $unit = [System.Collections.Generic.List[object]]::new()
    $index = 0
    while ($index -lt $Line.Count) {
        $text = $Line[$index]
        # Blank separators inside a hunk side carry no unit and are dropped.
        if ([string]::IsNullOrWhiteSpace($text)) { $index++; continue }
        $single = [regex]::Match($text, $script:MsBuildSinglePattern)
        if ($single.Success) {
            $attribute = Get-AttributeMap -Text $single.Groups[2].Value
            # An item with no Include has no key, so it cannot be reconciled.
            if (-not $attribute.ContainsKey('Include')) { return $null }
            $unit.Add([pscustomobject]@{
                    Key        = ('{0}:{1}' -f $single.Groups[1].Value, $attribute['Include'])
                    Version    = $null
                    Lines      = @($text)
                    Attributes = $attribute
                })
            $index++
            continue
        }
        $open = [regex]::Match($text, $script:MsBuildOpenPattern)
        # Neither admitted form matched, so this whole side is ungrammatical.
        if (-not $open.Success) { return $null }
        $itemType = $open.Groups[1].Value
        $attribute = Get-AttributeMap -Text $open.Groups[2].Value
        if (-not $attribute.ContainsKey('Include')) { return $null }
        $body = [System.Collections.Generic.List[string]]::new()
        $body.Add($text)
        $index++
        $closePattern = '^\s*</{0}>\s*$' -f [regex]::Escape($itemType)
        $isClosed = $false
        while ($index -lt $Line.Count) {
            $child = $Line[$index]
            $body.Add($child)
            $index++
            # The matching close tag ends the unit and is part of its lines.
            if ([regex]::IsMatch($child, $closePattern)) { $isClosed = $true; break }
            # A child outside the metadata set means the form is not understood.
            if (-not [regex]::IsMatch($child, $script:MsBuildChildPattern)) { return $null }
        }
        # A side ending mid-element is a truncated hunk, not a mergeable unit.
        if (-not $isClosed) { return $null }
        $unit.Add([pscustomobject]@{
                Key        = ('{0}:{1}' -f $itemType, $attribute['Include'])
                Version    = $null
                Lines      = $body.ToArray()
                Attributes = $attribute
            })
    }
    return , $unit.ToArray()
}

function Get-PackageUnit {
    <#
    .SYNOPSIS
        Parse packages.config package lines into keyed units.
    .DESCRIPTION
        The key is package:<id> and the version is the version attribute, so two
        sides pinning one package at different versions are comparable.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param([Parameter(Mandatory = $true)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Line)

    $unit = [System.Collections.Generic.List[object]]::new()
    foreach ($text in $Line) {
        # Blank separators carry no unit.
        if ([string]::IsNullOrWhiteSpace($text)) { continue }
        $match = [regex]::Match($text, $script:PackagePattern)
        # Anything other than a package element is outside the grammar.
        if (-not $match.Success) { return $null }
        $attribute = Get-AttributeMap -Text $match.Groups[1].Value
        # Without an id there is no key to reconcile the two sides on.
        if (-not $attribute.ContainsKey('id')) { return $null }
        $version = if ($attribute.ContainsKey('version')) { $attribute['version'] } else { $null }
        $unit.Add([pscustomobject]@{
                Key        = ('package:{0}' -f $attribute['id'])
                Version    = $version
                Lines      = @($text)
                Attributes = $attribute
            })
    }
    return , $unit.ToArray()
}

function Get-BindingRedirectUnit {
    <#
    .SYNOPSIS
        Parse app.config dependentAssembly blocks into keyed units.
    .DESCRIPTION
        The key is bindingRedirect:<assemblyIdentity name> and the version is the
        bindingRedirect newVersion, so two redirects for one assembly compare.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param([Parameter(Mandatory = $true)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Line)

    $unit = [System.Collections.Generic.List[object]]::new()
    $index = 0
    while ($index -lt $Line.Count) {
        $text = $Line[$index]
        # Blank separators between blocks carry no unit.
        if ([string]::IsNullOrWhiteSpace($text)) { $index++; continue }
        # Only a whole dependentAssembly block is admitted; a bare child is not.
        if (-not [regex]::IsMatch($text, $script:AppConfigOpenPattern)) { return $null }
        $body = [System.Collections.Generic.List[string]]::new()
        $body.Add($text)
        $index++
        $name = $null
        $identity = @{}
        $newVersion = $null
        $isClosed = $false
        while ($index -lt $Line.Count) {
            $child = $Line[$index]
            $body.Add($child)
            $index++
            if ([regex]::IsMatch($child, $script:AppConfigClosePattern)) { $isClosed = $true; break }
            $match = [regex]::Match($child, $script:AppConfigChildPattern)
            # An unrecognised child means the block is not understood.
            if (-not $match.Success) { return $null }
            $attribute = Get-AttributeMap -Text $match.Groups[2].Value
            # The identity supplies the key; the redirect supplies the version.
            if ($match.Groups[1].Value -eq 'assemblyIdentity' -and $attribute.ContainsKey('name')) {
                $name = $attribute['name']
                $identity = $attribute
            }
            if ($match.Groups[1].Value -eq 'bindingRedirect' -and $attribute.ContainsKey('newVersion')) { $newVersion = $attribute['newVersion'] }
        }
        # A block with no close tag or no identity name cannot be keyed.
        if (-not $isClosed -or $null -eq $name) { return $null }
        $unit.Add([pscustomobject]@{
                Key        = ('bindingRedirect:{0}' -f $name)
                Version    = $newVersion
                Lines      = $body.ToArray()
                Attributes = $identity
            })
    }
    return , $unit.ToArray()
}

function Get-MergeableUnit {
    <#
    .SYNOPSIS
        Parse one side of a conflict hunk into ordered, keyed units.
    .DESCRIPTION
        Dispatches to the parser for the supplied kind. The whole side is parsed
        or none of it is: one line outside the grammar yields $null, which the
        merge turns into an escalation rather than a partial resolution.
    .PARAMETER Line
        The lines of one hunk side, each retaining its own terminator.
    .PARAMETER Kind
        'msbuild', 'packages', or 'appconfig' from Get-ProjectFileKind.
    .OUTPUTS
        System.Object[] of units carrying Key, Version, Lines, and Attributes in
        side order, or $null when the side is ungrammatical.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Line,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string] $Kind
    )

    # One parser per kind. An unrecognised kind is ungrammatical rather than
    # defaulting to a parser, so a typo cannot silently merge a file.
    if ($Kind -eq 'msbuild') {
        $unit = Get-MsBuildUnit -Line $Line
    } elseif ($Kind -eq 'packages') {
        $unit = Get-PackageUnit -Line $Line
    } elseif ($Kind -eq 'appconfig') {
        $unit = Get-BindingRedirectUnit -Line $Line
    } else {
        return $null
    }

    # The comma keeps an empty parse distinguishable from a failed parse: without
    # it an empty array returns as $null and a clean side would read as escalate.
    if ($null -eq $unit) { return $null }
    return , $unit
}

function Compare-UnitVersion {
    <#
    .SYNOPSIS
        Rank two version strings recorded for the same unit key.
    .DESCRIPTION
        Returns 'ours', 'theirs', or 'equal' when both sides parse as
        System.Version, and 'escalate' otherwise: a prerelease or non-numeric
        version such as 1.0.0-beta1 cannot be ranked mechanically.
    .PARAMETER Ours
        The version recorded on the ours side. May be absent.
    .PARAMETER Theirs
        The version recorded on the theirs side. May be absent.
    .OUTPUTS
        System.String: 'ours', 'theirs', 'equal', or 'escalate'.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)][AllowNull()][AllowEmptyString()][string] $Ours,
        [Parameter(Mandatory = $true)][AllowNull()][AllowEmptyString()][string] $Theirs
    )

    [System.Version] $oursVersion = $null
    [System.Version] $theirsVersion = $null
    # An unparseable side on either hand is unrankable, so neither is chosen.
    if (-not [System.Version]::TryParse($Ours, [ref] $oursVersion)) { return 'escalate' }
    if (-not [System.Version]::TryParse($Theirs, [ref] $theirsVersion)) { return 'escalate' }

    # Strictly higher wins; anything else is an equal pin needing no choice.
    if ($oursVersion -gt $theirsVersion) { return 'ours' }
    if ($theirsVersion -gt $oursVersion) { return 'theirs' }
    return 'equal'
}

Export-ModuleMember -Function Get-ProjectFileKind, Get-MergeableUnit, Compare-UnitVersion
