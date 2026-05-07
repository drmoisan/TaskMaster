Set-StrictMode -Version Latest

function Get-KoverageProjectAllowlist {
    [CmdletBinding()]
    [OutputType([System.Array])]
    param(
        [Parameter(Mandatory = $false)]
        [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
    )

    $projectFiles = Get-ChildItem -Path $RepoRoot -Recurse -File -Include '*.csproj', '*.vbproj', '*.fsproj' |
        Where-Object {
            $_.FullName -notmatch '\\bin\\' -and
            $_.FullName -notmatch '\\obj\\' -and
            $_.FullName -notmatch '\\packages\\'
        }

    $projectNames = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

    foreach ($projectFile in $projectFiles) {
        $projectContent = Get-Content -Path $projectFile.FullName -Raw -Encoding UTF8
        $assemblyNameMatch = [regex]::Match(
            $projectContent,
            '<AssemblyName>\s*(?<name>[^<]+?)\s*</AssemblyName>',
            [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
        )

        if ($assemblyNameMatch.Success) {
            $null = $projectNames.Add($assemblyNameMatch.Groups['name'].Value.Trim())
            continue
        }

        $null = $projectNames.Add([System.IO.Path]::GetFileNameWithoutExtension($projectFile.Name))
    }

    $projectNames | Sort-Object
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

function Get-CoberturaLineConditionCoverageParts {
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [System.Xml.XmlElement]$LineNode
    )

    if ($LineNode.HasAttribute('condition-coverage') -and $LineNode.'condition-coverage' -match '\(([0-9]+)/([0-9]+)\)') {
        return [pscustomobject]@{
            Covered = [int]$Matches[1]
            Total   = [int]$Matches[2]
        }
    }

    return [pscustomobject]@{
        Covered = 0
        Total   = 0
    }
}

function Merge-CoberturaClassesByFilename {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [xml]$XmlDocument
    )

    foreach ($packageNode in $XmlDocument.SelectNodes('//package')) {
        $classesNode = $packageNode.SelectSingleNode('./classes')
        if (-not $classesNode) {
            continue
        }

        $filenameGroups = @{}
        foreach ($classNode in @($classesNode.SelectNodes('./class[@filename]'))) {
            if (-not $filenameGroups.Contains($classNode.filename)) {
                $filenameGroups[$classNode.filename] = [System.Collections.ArrayList]::new()
            }

            [void]$filenameGroups[$classNode.filename].Add($classNode)
        }

        foreach ($filename in $filenameGroups.Keys) {
            $group = @($filenameGroups[$filename])
            if ($group.Count -le 1) {
                continue
            }

            $primaryNode = $group | Where-Object { $_.name -notmatch '<' } | Select-Object -First 1
            if (-not $primaryNode) {
                $primaryNode = $group[0]
            }

            $mergedClassNode = $primaryNode.CloneNode($true)

            $methodsNode = $mergedClassNode.SelectSingleNode('./methods')
            if (-not $methodsNode) {
                $methodsNode = $XmlDocument.CreateElement('methods')
                [void]$mergedClassNode.AppendChild($methodsNode)
            }

            $linesNode = $mergedClassNode.SelectSingleNode('./lines')
            if ($linesNode) {
                $linesNode.RemoveAll()
            }
            else {
                $linesNode = $XmlDocument.CreateElement('lines')
                [void]$mergedClassNode.AppendChild($linesNode)
            }

            $lineMap = @{}
            foreach ($classNode in $group) {
                foreach ($lineNode in @($classNode.SelectNodes('./lines/line'))) {
                    $lineNumber = [int]$lineNode.number
                    $candidateCoverage = Get-CoberturaLineConditionCoverageParts -LineNode $lineNode

                    if (-not $lineMap.Contains($lineNumber)) {
                        $lineMap[$lineNumber] = [pscustomobject]@{
                            Node    = $lineNode.CloneNode($true)
                            Covered = $candidateCoverage.Covered
                            Total   = $candidateCoverage.Total
                        }
                        continue
                    }

                    $existing = $lineMap[$lineNumber]
                    $existingNode = $existing.Node
                    $existingNode.SetAttribute('hits', [string]([math]::Max([int]$existingNode.GetAttribute('hits'), [int]$lineNode.GetAttribute('hits'))))

                    if ($existingNode.GetAttribute('branch') -ne 'True' -and $lineNode.GetAttribute('branch') -eq 'True') {
                        $existingNode.SetAttribute('branch', 'True')
                    }

                    if (
                        $candidateCoverage.Total -gt $existing.Total -or
                        ($candidateCoverage.Total -eq $existing.Total -and $candidateCoverage.Covered -gt $existing.Covered)
                    ) {
                        $existing.Covered = $candidateCoverage.Covered
                        $existing.Total = $candidateCoverage.Total

                        if ($lineNode.HasAttribute('condition-coverage')) {
                            $existingNode.SetAttribute('condition-coverage', $lineNode.GetAttribute('condition-coverage'))
                        }
                        elseif ($existingNode.HasAttribute('condition-coverage')) {
                            $existingNode.RemoveAttribute('condition-coverage')
                        }

                        foreach ($conditionChild in @($existingNode.SelectNodes('./conditions'))) {
                            [void]$existingNode.RemoveChild($conditionChild)
                        }

                        foreach ($conditionChild in @($lineNode.SelectNodes('./conditions'))) {
                            [void]$existingNode.AppendChild($conditionChild.CloneNode($true))
                        }
                    }
                }
            }

            $sortedLineNumbers = $lineMap.Keys | Sort-Object
            foreach ($lineNumber in $sortedLineNumbers) {
                [void]$linesNode.AppendChild($lineMap[$lineNumber].Node)
            }

            $classSummaryXml = [xml]"<coverage><packages><package><classes /></package></packages></coverage>"
            $classSummaryClasses = $classSummaryXml.SelectSingleNode('//classes')
            [void]$classSummaryClasses.AppendChild($classSummaryXml.ImportNode($mergedClassNode, $true))
            $classSummary = Get-CoberturaCoverageSummary -XmlDocument $classSummaryXml

            $mergedClassNode.SetAttribute('line-rate', $classSummary.LineRate)
            $mergedClassNode.SetAttribute('branch-rate', $classSummary.BranchRate)
            $mergedClassNode.SetAttribute('complexity', [string](
                    ($group | ForEach-Object {
                        if ($_.complexity) { [double]$_.complexity } else { 0d }
                    } | Measure-Object -Sum).Sum
                ))

            [void]$classesNode.ReplaceChild($mergedClassNode, $primaryNode)

            foreach ($classNode in $group) {
                if ($classNode -ne $primaryNode) {
                    [void]$classesNode.RemoveChild($classNode)
                }
            }
        }
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

    Merge-CoberturaClassesByFilename -XmlDocument $xml

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
    $xml.coverage.SetAttribute('line-rate', $coverageSummary.LineRate)
    $xml.coverage.SetAttribute('branch-rate', $coverageSummary.BranchRate)
    $xml.coverage.SetAttribute('lines-covered', $coverageSummary.LinesCovered)
    $xml.coverage.SetAttribute('lines-valid', $coverageSummary.LinesValid)
    $xml.coverage.SetAttribute('branches-covered', $coverageSummary.BranchesCovered)
    $xml.coverage.SetAttribute('branches-valid', $coverageSummary.BranchesValid)

    $stringWriter = [System.IO.StringWriter]::new()
    $xmlWriter = [System.Xml.XmlTextWriter]::new($stringWriter)
    $xmlWriter.Formatting = [System.Xml.Formatting]::Indented
    $xml.WriteTo($xmlWriter)
    $xmlWriter.Flush()
    $xmlWriter.Close()

    return $stringWriter.ToString()
}
