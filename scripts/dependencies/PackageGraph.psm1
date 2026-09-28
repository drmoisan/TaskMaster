<#
.SYNOPSIS
    Parses and renders the NuGet package manifests, project dependent elements and
    application binding redirects that the dependency-consistency tooling reads.

.DESCRIPTION
    PackageGraph is the text layer of the dependency-consistency tooling for issue #911.
    Every function except Get-PackageManifestPath and Invoke-ManifestNormalization is pure
    over text: it takes a string and returns records, or takes records and returns a string.
    Those two reach the outside world only through injected delegates, so the whole module
    is exercisable in memory, with no temporary file and no filesystem dependency.

    The canonical form this module renders is the inline form the NuGet CLI writes: one
    element per line, attributes separated by a single space, a space before the
    self-closing slash, two-space indentation and CRLF line endings.

    Exported functions:
      - Get-PackageManifestPath
      - ConvertFrom-PackagesConfigText
      - ConvertTo-PackagesConfigText
      - ConvertFrom-ProjectFileText
      - ConvertFrom-AppConfigText
      - ConvertTo-AppConfigText
      - Invoke-ManifestNormalization
#>

Set-StrictMode -Version Latest

$script:AttributePattern = '(?<name>[A-Za-z_][A-Za-z0-9_.:-]*)\s*=\s*"(?<value>[^"]*)"'
$script:DirectorySeparator = [char]92
$script:PathSeparator = [char]47
$script:NewLine = "`r`n"

function ConvertTo-AttributeMap {
    <#
    .SYNOPSIS
        Converts the attribute region of an XML start tag into an ordered name/value map.
    .PARAMETER AttributeText
        The text between the element name and the tag terminator.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$AttributeText
    )

    $map = [ordered]@{}
    foreach ($match in [regex]::Matches($AttributeText, $script:AttributePattern)) {
        $map[$match.Groups['name'].Value] = $match.Groups['value'].Value
    }
    return $map
}

function ConvertTo-DependentElementRecord {
    <#
    .SYNOPSIS
        Builds one dependent-element record for ConvertFrom-ProjectFileText.
    .PARAMETER Kind
        One of Import, Error, Reference, HintPath or Analyzer.
    .PARAMETER LineNumber
        The one-based line number the element was found on.
    .PARAMETER Value
        The element's primary value: project path, error text, include or hint path.
    .PARAMETER Attribute
        The ordered attribute map, empty for elements that carry none.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][string]$Kind,
        [Parameter(Mandatory = $true)][int]$LineNumber,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Value,
        [Parameter(Mandatory = $true)][object]$Attribute
    )

    return [pscustomobject]@{
        PSTypeName = 'PackageGraph.DependentElement'
        Kind       = $Kind
        Value      = $Value
        Attribute  = $Attribute
        LineNumber = $LineNumber
    }
}

function Get-PackageManifestPath {
    <#
    .SYNOPSIS
        Selects the manifest or application-configuration paths from an injected listing.
    .DESCRIPTION
        The only I/O this module performs on its own behalf is the call to the supplied
        delegate. The delegate returns candidate paths as strings; this function filters
        them by leaf name and discards anything under a restore output directory, so a
        listing taken over a restored tree does not pull package-internal copies in.
    .PARAMETER Kind
        PackagesConfig selects packages.config; AppConfig selects app.config.
    .PARAMETER DirectoryLister
        A delegate returning candidate paths. Separators may be either form.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('PackagesConfig', 'AppConfig')]
        [string]$Kind,

        [Parameter(Mandatory = $true)]
        [scriptblock]$DirectoryLister
    )

    $fileName = if ($Kind -eq 'PackagesConfig') { 'packages.config' } else { 'app.config' }
    $excluded = @('packages', 'bin', 'obj', 'node_modules')
    $selected = [System.Collections.Generic.List[string]]::new()

    foreach ($candidate in @(& $DirectoryLister)) {
        if ($null -eq $candidate) { continue }
        $original = [string]$candidate
        $normalised = $original.Replace($script:DirectorySeparator, $script:PathSeparator)
        $segments = $normalised.Split($script:PathSeparator)
        if ($segments[-1] -ne $fileName) { continue }
        $parents = $segments[0..($segments.Count - 2)]
        if (@($parents | Where-Object { $excluded -contains $_ }).Count -gt 0) { continue }
        $selected.Add($original)
    }

    return [string[]]@($selected | Sort-Object)
}

function ConvertFrom-PackagesConfigText {
    <#
    .SYNOPSIS
        Parses packages.config text into ordered package records.
    .DESCRIPTION
        Parsing is insensitive to whether an element is written inline or reflowed across
        several lines: both yield the same record, which is what makes the normalisation
        at issue #911 verifiable rather than merely plausible.
    .PARAMETER Text
        The manifest text. A document with no packages root element is rejected.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    if ($Text -notmatch '<packages\b') {
        throw 'The supplied text is not a packages.config document: no packages root element was found.'
    }

    $records = [System.Collections.Generic.List[pscustomobject]]::new()
    $index = 0
    foreach ($match in [regex]::Matches($Text, '(?s)<package\b(?<attrs>[^<>]*?)/>')) {
        $map = ConvertTo-AttributeMap -AttributeText $match.Groups['attrs'].Value
        if (-not $map.Contains('id')) {
            throw "The package element at index $index declares no id attribute."
        }
        if (-not $map.Contains('version')) {
            throw "The package element for id '$($map['id'])' declares no version attribute."
        }
        $framework = if ($map.Contains('targetFramework')) { [string]$map['targetFramework'] } else { '' }
        $records.Add([pscustomobject]@{
                PSTypeName      = 'PackageGraph.PackageRecord'
                Id              = [string]$map['id']
                Version         = [string]$map['version']
                TargetFramework = $framework
                Attribute       = $map
                Index           = $index
            })
        $index++
    }

    return $records.ToArray()
}

function ConvertTo-PackagesConfigText {
    <#
    .SYNOPSIS
        Renders package records as a canonical inline packages.config document.
    .DESCRIPTION
        Attributes are emitted in the order the parser recorded them, so a round trip
        through ConvertFrom-PackagesConfigText and back preserves attribute order and is
        byte-identical for a manifest already in canonical form.
    .PARAMETER Package
        The package records to render.
    .PARAMETER Indent
        The indentation prefix for each element. Two spaces by default.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [object[]]$Package,

        [ValidateNotNull()]
        [string]$Indent = '  '
    )

    $builder = [System.Text.StringBuilder]::new()
    [void]$builder.Append('<?xml version="1.0" encoding="utf-8"?>').Append($script:NewLine)
    [void]$builder.Append('<packages>').Append($script:NewLine)
    foreach ($record in $Package) {
        $pairs = foreach ($name in $record.Attribute.Keys) {
            '{0}="{1}"' -f $name, $record.Attribute[$name]
        }
        [void]$builder.Append($Indent).Append('<package ').Append(($pairs -join ' ')).Append(' />').Append($script:NewLine)
    }
    [void]$builder.Append('</packages>').Append($script:NewLine)
    return $builder.ToString()
}

function ConvertFrom-ProjectFileText {
    <#
    .SYNOPSIS
        Parses project-file text into records for the five dependent element kinds.
    .DESCRIPTION
        The five kinds are the ones a package upgrade can strand: Import, Error,
        Reference, HintPath and Analyzer. Each record carries its one-based line number so
        a caller can report a finding against a reviewable location.
    .PARAMETER Text
        The project file text. Empty or whitespace-only input is rejected.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    if ([string]::IsNullOrWhiteSpace($Text)) {
        throw 'The supplied project-file text is empty, so no dependent elements can be parsed.'
    }

    $records = [System.Collections.Generic.List[pscustomobject]]::new()
    $lines = $Text -split "`r?`n"
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        $number = $i + 1

        $import = [regex]::Match($line, '<Import\b(?<attrs>[^<>]*?)/?>')
        if ($import.Success) {
            $map = ConvertTo-AttributeMap -AttributeText $import.Groups['attrs'].Value
            $value = if ($map.Contains('Project')) { [string]$map['Project'] } else { '' }
            $records.Add((ConvertTo-DependentElementRecord -Kind 'Import' -LineNumber $number -Value $value -Attribute $map))
            continue
        }

        $errorElement = [regex]::Match($line, '<Error\b(?<attrs>[^<>]*?)/?>')
        if ($errorElement.Success) {
            $map = ConvertTo-AttributeMap -AttributeText $errorElement.Groups['attrs'].Value
            $value = if ($map.Contains('Text')) { [string]$map['Text'] } else { '' }
            $records.Add((ConvertTo-DependentElementRecord -Kind 'Error' -LineNumber $number -Value $value -Attribute $map))
            continue
        }

        $analyzer = [regex]::Match($line, '<Analyzer\b(?<attrs>[^<>]*?)/?>')
        if ($analyzer.Success) {
            $map = ConvertTo-AttributeMap -AttributeText $analyzer.Groups['attrs'].Value
            $value = if ($map.Contains('Include')) { [string]$map['Include'] } else { '' }
            $records.Add((ConvertTo-DependentElementRecord -Kind 'Analyzer' -LineNumber $number -Value $value -Attribute $map))
            continue
        }

        $reference = [regex]::Match($line, '<Reference\b(?<attrs>[^<>]*?)/?>')
        if ($reference.Success) {
            $map = ConvertTo-AttributeMap -AttributeText $reference.Groups['attrs'].Value
            $value = if ($map.Contains('Include')) { [string]$map['Include'] } else { '' }
            $records.Add((ConvertTo-DependentElementRecord -Kind 'Reference' -LineNumber $number -Value $value -Attribute $map))
            continue
        }

        $hintPath = [regex]::Match($line, '<HintPath>(?<value>[^<]*)</HintPath>')
        if ($hintPath.Success) {
            $records.Add((ConvertTo-DependentElementRecord -Kind 'HintPath' -LineNumber $number -Value $hintPath.Groups['value'].Value -Attribute ([ordered]@{})))
        }
    }

    return $records.ToArray()
}

function ConvertFrom-AppConfigText {
    <#
    .SYNOPSIS
        Parses application-configuration text into binding-redirect records.
    .DESCRIPTION
        One record is produced per dependentAssembly block, carrying the assembly identity
        and the redirect range. A block declaring no assembly identity is rejected, because
        a redirect with no identity cannot be reconciled against a resolved assembly.
    .PARAMETER Text
        The application configuration text.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    if ($Text -notmatch '<configuration\b') {
        throw 'The supplied text is not an application configuration document: no configuration root element was found.'
    }

    $records = [System.Collections.Generic.List[pscustomobject]]::new()
    foreach ($block in [regex]::Matches($Text, '(?s)<dependentAssembly>(?<body>.*?)</dependentAssembly>')) {
        $body = $block.Groups['body'].Value
        $identity = [regex]::Match($body, '(?s)<assemblyIdentity\b(?<attrs>[^<>]*?)/>')
        if (-not $identity.Success) {
            throw 'A dependentAssembly block declares no assemblyIdentity element.'
        }
        $identityMap = ConvertTo-AttributeMap -AttributeText $identity.Groups['attrs'].Value
        $redirect = [regex]::Match($body, '(?s)<bindingRedirect\b(?<attrs>[^<>]*?)/>')
        $redirectMap = if ($redirect.Success) {
            ConvertTo-AttributeMap -AttributeText $redirect.Groups['attrs'].Value
        }
        else {
            [ordered]@{}
        }
        $records.Add([pscustomobject]@{
                PSTypeName     = 'PackageGraph.BindingRedirect'
                Name           = if ($identityMap.Contains('name')) { [string]$identityMap['name'] } else { '' }
                PublicKeyToken = if ($identityMap.Contains('publicKeyToken')) { [string]$identityMap['publicKeyToken'] } else { '' }
                Culture        = if ($identityMap.Contains('culture')) { [string]$identityMap['culture'] } else { '' }
                OldVersion     = if ($redirectMap.Contains('oldVersion')) { [string]$redirectMap['oldVersion'] } else { '' }
                NewVersion     = if ($redirectMap.Contains('newVersion')) { [string]$redirectMap['newVersion'] } else { '' }
                Identity       = $identityMap
            })
    }

    return $records.ToArray()
}

function ConvertTo-AppConfigText {
    <#
    .SYNOPSIS
        Renders application-configuration text in canonical inline form.
    .DESCRIPTION
        A start tag whose attributes have been reflowed across several lines is collapsed
        onto one line. Every other line is emitted unchanged, so the transform touches
        formatting alone. Applying it to its own output is a no-op, because the collapsed
        output contains no reflowed start tag for the second pass to find.
    .PARAMETER Text
        The application configuration text.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    $lines = $Text -split "`r?`n"
    $rendered = [System.Collections.Generic.List[string]]::new()
    $index = 0

    while ($index -lt $lines.Count) {
        $line = $lines[$index]
        $opening = [regex]::Match($line, '^(?<indent>\s*)(?<tag><[A-Za-z_][A-Za-z0-9_.:-]*)\s*$')
        if (-not $opening.Success) {
            $rendered.Add($line)
            $index++
            continue
        }

        $parts = [System.Collections.Generic.List[string]]::new()
        $parts.Add($opening.Groups['tag'].Value)
        $cursor = $index + 1
        $terminated = $false
        while ($cursor -lt $lines.Count) {
            $fragment = $lines[$cursor].Trim()
            if ($fragment -ne '') { $parts.Add($fragment) }
            if ($fragment.EndsWith('>')) { $terminated = $true; break }
            $cursor++
        }
        if (-not $terminated) {
            throw "The start tag beginning on line $($index + 1) is never terminated."
        }

        $joined = $opening.Groups['indent'].Value + ($parts -join ' ')
        if ($joined.EndsWith(' >')) {
            $joined = $joined.Substring(0, $joined.Length - 2) + '>'
        }
        $rendered.Add($joined)
        $index = $cursor + 1
    }

    return ($rendered -join $script:NewLine)
}

function Invoke-ManifestNormalization {
    <#
    .SYNOPSIS
        Rewrites every discovered manifest and application configuration in canonical form.
    .DESCRIPTION
        Discovery, reading and writing are all supplied as delegates, so the function is
        exercisable in memory. The returned summary reports the examined count per kind
        alongside the changed count per kind. The two legitimately differ: a file already
        in canonical form is examined and left byte-identical, so a changed count below the
        examined count is not a discovery shortfall.
    .PARAMETER DirectoryLister
        A delegate returning candidate paths, passed through to Get-PackageManifestPath.
    .PARAMETER TextReader
        A delegate taking a path and returning that file's text.
    .PARAMETER TextWriter
        A delegate taking a path and the replacement text.
    #>
    [CmdletBinding(SupportsShouldProcess = $true)]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][scriptblock]$DirectoryLister,
        [Parameter(Mandatory = $true)][scriptblock]$TextReader,
        [Parameter(Mandatory = $true)][scriptblock]$TextWriter
    )

    $examined = [ordered]@{ PackagesConfig = 0; AppConfig = 0 }
    $changed = [ordered]@{ PackagesConfig = 0; AppConfig = 0 }
    $changedPath = [System.Collections.Generic.List[string]]::new()
    $examinedPath = [System.Collections.Generic.List[string]]::new()

    foreach ($kind in @('PackagesConfig', 'AppConfig')) {
        foreach ($path in (Get-PackageManifestPath -Kind $kind -DirectoryLister $DirectoryLister)) {
            $examined[$kind] = $examined[$kind] + 1
            $examinedPath.Add($path)
            $original = [string](& $TextReader $path)
            if ($kind -eq 'PackagesConfig') {
                $rendered = ConvertTo-PackagesConfigText -Package @(ConvertFrom-PackagesConfigText -Text $original)
            }
            else {
                $rendered = ConvertTo-AppConfigText -Text $original
            }
            if ($rendered -ceq $original) { continue }
            if ($PSCmdlet.ShouldProcess($path, 'Rewrite in canonical inline form')) {
                & $TextWriter $path $rendered
                $changed[$kind] = $changed[$kind] + 1
                $changedPath.Add($path)
            }
        }
    }

    return [pscustomobject]@{
        PSTypeName             = 'PackageGraph.NormalizationSummary'
        ExaminedPackagesConfig = $examined['PackagesConfig']
        ExaminedAppConfig      = $examined['AppConfig']
        ExaminedTotal          = $examined['PackagesConfig'] + $examined['AppConfig']
        ChangedPackagesConfig  = $changed['PackagesConfig']
        ChangedAppConfig       = $changed['AppConfig']
        ChangedPath            = $changedPath.ToArray()
        ExaminedPath           = $examinedPath.ToArray()
    }
}

Export-ModuleMember -Function @(
    'Get-PackageManifestPath',
    'ConvertFrom-PackagesConfigText',
    'ConvertTo-PackagesConfigText',
    'ConvertFrom-ProjectFileText',
    'ConvertFrom-AppConfigText',
    'ConvertTo-AppConfigText',
    'Invoke-ManifestNormalization'
)
