Set-StrictMode -Version Latest

function Get-KoverageProjectAllowlist {
    [CmdletBinding()]
    [OutputType([System.Array])]
    param()

    @(
        'QuickFiler', 'QuickFiler.Test',
        'SVGControl', 'SVGControl.Test',
        'Tags', 'Tags.Test',
        'TaskMaster', 'TaskMaster.Test',
        'TaskTree',
        'TaskVisualization', 'TaskVisualization.Test',
        'TaskVisualizer',
        'ToDoModel', 'ToDoModel.Test',
        'UtilitiesCS', 'UtilitiesCS.Test',
        'UtilitiesSwordfish', 'UtilitiesSwordfish.Test',
        'VBFunctions', 'VBFunctions.Test'
    )
}

function ConvertTo-KoverageRelativePath {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$RepoRoot,

        [Parameter(Mandatory = $false)]
        [ValidateSet('/', '\')]
        [string]$PathSeparator = [System.IO.Path]::DirectorySeparatorChar
    )

    $trimmedRepoRoot = $RepoRoot.TrimEnd('\', '/')
    $relativePath = $Path
    $prefixes = @(
        "$trimmedRepoRoot\",
        "$trimmedRepoRoot/"
    )

    foreach ($prefix in $prefixes) {
        if ($relativePath.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)) {
            $relativePath = $relativePath.Substring($prefix.Length)
            break
        }
    }

    if ($PathSeparator -eq '\') {
        return $relativePath.Replace('/', '\')
    }

    return $relativePath.Replace('\', '/')
}

function Get-CoberturaCoverageSummary {
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [xml]$XmlDocument
    )

    $totalLines = 0
    $coveredLines = 0
    $totalBranches = 0
    $coveredBranches = 0

    $packagesNode = $XmlDocument.SelectSingleNode('//packages')
    if (-not $packagesNode) {
        throw 'Cobertura XML does not contain a <packages> node.'
    }

    foreach ($pkg in $packagesNode.ChildNodes) {
        if ($pkg.NodeType -ne 'Element') {
            continue
        }

        foreach ($cls in $pkg.SelectNodes('.//class')) {
            foreach ($line in $cls.SelectNodes('.//lines/line')) {
                $totalLines++
                if ([int]$line.hits -gt 0) {
                    $coveredLines++
                }

                if ($line.branch -eq 'True' -and $line.HasAttribute('condition-coverage') -and $line.'condition-coverage' -match '\(([0-9]+)/([0-9]+)\)') {
                    $coveredBranches += [int]$Matches[1]
                    $totalBranches += [int]$Matches[2]
                }
            }
        }
    }

    [pscustomobject]@{
        LineRate        = if ($totalLines -gt 0) { [string]([math]::Round($coveredLines / $totalLines, 6)) } else { '0' }
        BranchRate      = if ($totalBranches -gt 0) { [string]([math]::Round($coveredBranches / $totalBranches, 6)) } else { '0' }
        LinesCovered    = [string]$coveredLines
        LinesValid      = [string]$totalLines
        BranchesCovered = [string]$coveredBranches
        BranchesValid   = [string]$totalBranches
    }
}

function ConvertTo-KoverageCoberturaXml {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [string]$XmlContent,

        [Parameter(Mandatory = $true)]
        [string]$RepoRoot,

        [Parameter(Mandatory = $false)]
        [string[]]$ProjectNames = (Get-KoverageProjectAllowlist),

        [Parameter(Mandatory = $false)]
        [ValidateSet('/', '\')]
        [string]$PathSeparator = [System.IO.Path]::DirectorySeparatorChar
    )

    [xml]$xml = $XmlContent
    $packagesNode = $xml.SelectSingleNode('//packages')
    if (-not $packagesNode) {
        throw 'Cobertura XML does not contain a <packages> node.'
    }

    foreach ($pkg in @($packagesNode.ChildNodes)) {
        if ($pkg.NodeType -eq 'Element' -and $pkg.name -and $pkg.name -notin $ProjectNames) {
            $packagesNode.RemoveChild($pkg) | Out-Null
        }
    }

    foreach ($classNode in $xml.SelectNodes('//class[@filename]')) {
        $classNode.filename = ConvertTo-KoverageRelativePath -Path $classNode.filename -RepoRoot $RepoRoot -PathSeparator $PathSeparator
    }

    if (-not $xml.SelectSingleNode('//sources')) {
        $sourcesNode = $xml.CreateElement('sources')
        $sourceNode = $xml.CreateElement('source')
        $sourceNode.InnerText = '.'
        $sourcesNode.AppendChild($sourceNode) | Out-Null

        $coverageNode = $xml.SelectSingleNode('/coverage')
        $packagesElement = $xml.SelectSingleNode('/coverage/packages')
        $coverageNode.InsertBefore($sourcesNode, $packagesElement) | Out-Null
    }

    $coverageSummary = Get-CoberturaCoverageSummary -XmlDocument $xml
    $xml.coverage.'line-rate' = $coverageSummary.LineRate
    $xml.coverage.'branch-rate' = $coverageSummary.BranchRate
    $xml.coverage.'lines-covered' = $coverageSummary.LinesCovered
    $xml.coverage.'lines-valid' = $coverageSummary.LinesValid
    $xml.coverage.'branches-covered' = $coverageSummary.BranchesCovered
    $xml.coverage.'branches-valid' = $coverageSummary.BranchesValid

    $stringWriter = [System.IO.StringWriter]::new()
    $xmlWriter = [System.Xml.XmlTextWriter]::new($stringWriter)
    $xmlWriter.Formatting = [System.Xml.Formatting]::Indented
    $xml.WriteTo($xmlWriter)
    $xmlWriter.Flush()
    $xmlWriter.Close()

    return $stringWriter.ToString()
}