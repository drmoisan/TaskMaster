Set-StrictMode -Version Latest

BeforeAll {
    $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
    Import-Module (Join-Path $script:RepoRoot 'scripts/dependencies/ConsistencyVerifier.psm1') -Force

    # These tests read tracked repository files resolved from $PSScriptRoot and hold every
    # other value in memory. Nothing is written to disk. No block or test name in this file
    # matches the regex AC followed by a digit.
    $script:WorkflowPath = Join-Path $script:RepoRoot '.github/workflows/dependabot-repair.yml'
    $script:RunbookPath = Join-Path $script:RepoRoot 'docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/runbooks/github-app-installation-token.runbook.md'
    $script:ClientIdPattern = '^\s+client-id:\s*\$\{\{\s*secrets\.([A-Z0-9_]+)\s*\}\}\s*$'

    function Get-TokenStepBlock {
        <#
        .SYNOPSIS
            Returns the lines of the workflow step whose uses line contains the given token.
        .DESCRIPTION
            Re-implements the step-parsing approach of Get-WorkflowStepBlock in
            DependabotConfig.Tests.ps1: a step runs from its name line to the next name line.
        #>
        param(
            [Parameter(Mandatory = $true)][AllowEmptyString()][string[]]$Line,
            [Parameter(Mandatory = $true)][string]$UsesToken
        )

        $steps = [System.Collections.Generic.List[object]]::new()
        $current = $null
        foreach ($text in $Line) {
            if ($text -match '^\s+-\s+name:\s*(.+?)\s*$') {
                $current = [System.Collections.Generic.List[string]]::new()
                $steps.Add($current)
            }
            if ($null -ne $current) { $current.Add($text) }
        }

        foreach ($step in $steps) {
            if (@($step | Where-Object { $_ -match '^\s+uses:' -and $_.Contains($UsesToken) }).Count -gt 0) {
                return , $step.ToArray()
            }
        }
        return , [string[]]@()
    }

    function Get-DependentAssemblyBlock {
        <#
        .SYNOPSIS
            Returns the dependentAssembly element text whose assemblyIdentity names the assembly.
        #>
        param(
            [Parameter(Mandatory = $true)][string]$ConfigText,
            [Parameter(Mandatory = $true)][string]$AssemblyName
        )

        $blocks = [regex]::Matches($ConfigText, '(?s)<dependentAssembly>.*?</dependentAssembly>')
        foreach ($block in $blocks) {
            if ($block.Value -match ('name="' + [regex]::Escape($AssemblyName) + '"')) { return $block.Value }
        }
        return ''
    }
}

Describe 'Repository tree consistency (issue 929)' {

    It 'reports no Import element whose package the sibling manifest omits, for every project directory that carries a manifest' {
        # Arrange: every top-level directory holding a packages.config and exactly one project file.
        $pair = @(Get-ChildItem -LiteralPath $script:RepoRoot -Directory | ForEach-Object {
                $manifest = Join-Path $_.FullName 'packages.config'
                $project = @(Get-ChildItem -LiteralPath $_.FullName -File | Where-Object { $_.Extension -eq '.csproj' })
                if ((Test-Path -LiteralPath $manifest) -and $project.Count -eq 1) {
                    [pscustomobject]@{ Project = $project[0].FullName; Manifest = $manifest }
                }
            })
        $pair.Count | Should -BeGreaterThan 9 -Because 'the repository carries more than nine project directories with a manifest'

        # Act
        $examined = 0
        $finding = [System.Collections.Generic.List[string]]::new()
        foreach ($entry in $pair) {
            $detection = Find-PackageAbsentFromManifest -ProjectText ([System.IO.File]::ReadAllText($entry.Project)) -ManifestText ([System.IO.File]::ReadAllText($entry.Manifest))
            $examined += $detection.ExaminedCount
            foreach ($item in @($detection.Finding | Where-Object { $_.Kind -eq 'Import' })) {
                $finding.Add(('{0}: line {1} {2}' -f (Split-Path -Leaf $entry.Project), $item.LineNumber, $item.PackageFolder))
            }
        }

        # Assert: the examined count guards the zero finding count.
        $examined | Should -BeGreaterThan 0 -Because 'a zero examined count would mean the detector never fired'
        $finding.Count | Should -Be 0 -Because ('these Import elements name a package the sibling manifest omits: ' + ($finding -join '; '))
    }

    It 'names in the SVGControl binding redirects the assembly version the SVGControl project reference declares' {
        # Arrange
        $projectText = [System.IO.File]::ReadAllText((Join-Path $script:RepoRoot 'SVGControl/SVGControl.csproj'))
        $configText = [System.IO.File]::ReadAllText((Join-Path $script:RepoRoot 'SVGControl/app.config'))

        foreach ($name in @('Fizzler', 'System.Runtime.CompilerServices.Unsafe')) {
            # Act
            $referenceMatch = [regex]::Match($projectText, ('Include="' + [regex]::Escape($name) + ', Version=([^,"]+)'))
            $referenceVersion = if ($referenceMatch.Success) { $referenceMatch.Groups[1].Value } else { '' }
            $block = Get-DependentAssemblyBlock -ConfigText $configText -AssemblyName $name
            $newVersion = [regex]::Match($block, 'newVersion="([^"]+)"').Groups[1].Value
            $oldVersion = [regex]::Match($block, 'oldVersion="([^"]+)"').Groups[1].Value

            # Assert
            $referenceVersion | Should -Not -BeNullOrEmpty -Because "the SVGControl project must declare a $name reference version"
            $newVersion | Should -Be $referenceVersion -Because "the $name redirect must name the referenced assembly version"
            $oldVersion.EndsWith('-' + $referenceVersion) | Should -BeTrue -Because "the $name oldVersion range '$oldVersion' must end at $referenceVersion"
        }
    }

    It 'passes client-id and not app-id to the create-github-app-token step of the repair workflow' {
        # Arrange
        $line = [System.IO.File]::ReadAllLines($script:WorkflowPath)

        # Act
        $block = Get-TokenStepBlock -Line $line -UsesToken 'actions/create-github-app-token@'
        $clientId = @($block | Where-Object { $_ -match $script:ClientIdPattern })
        $appId = @($block | Where-Object { $_ -match '^\s+app-id:' })

        # Assert
        $block.Count | Should -BeGreaterThan 0 -Because 'the repair workflow must carry a create-github-app-token step'
        $clientId.Count | Should -Be 1 -Because 'the token step must pass exactly one client-id input read from a secret'
        $appId.Count | Should -Be 0 -Because 'the token step must no longer pass the deprecated app-id input'
    }

    It 'instructs the maintainer to store the Client ID in the secret the repair workflow reads by name' {
        # Arrange
        $workflowLine = [System.IO.File]::ReadAllLines($script:WorkflowPath)
        $runbookLine = [System.IO.File]::ReadAllLines($script:RunbookPath)

        # Act: the secret name comes from the workflow, so the runbook is held to what it reads.
        $secretName = ''
        foreach ($text in $workflowLine) {
            if ($text -match $script:ClientIdPattern) { $secretName = $Matches[1]; break }
        }
        $runbookClientId = @($runbookLine | Where-Object { $_ -match ('^\s*client-id:\s*\$\{\{\s*secrets\.' + [regex]::Escape($secretName) + '\s*\}\}') })
        $runbookAppId = @($runbookLine | Where-Object { $_ -match '^\s*app-id:' })
        $partD = [array]::IndexOf(@($runbookLine | ForEach-Object { $_.StartsWith('### Part D') }), $true)
        $partE = [array]::IndexOf(@($runbookLine | ForEach-Object { $_.StartsWith('### Part E') }), $true)

        # Assert
        $secretName | Should -Not -BeNullOrEmpty -Because 'the workflow client-id line must name the secret name the runbook has to match'
        $runbookClientId.Count | Should -Be 1 -Because "the runbook sample must pass client-id from secrets.$secretName"
        $runbookAppId.Count | Should -Be 0 -Because 'the runbook sample must not pass the deprecated app-id input'
        $partD | Should -BeGreaterThan -1 -Because 'the runbook must carry a Part D heading'
        $partE | Should -BeGreaterThan $partD -Because 'the runbook Part E heading must follow Part D'
        $partDText = ($runbookLine[$partD..$partE]) -join "`n"
        $partDText.Contains('Client ID') | Should -BeTrue -Because 'Part D must instruct the maintainer to store the Client ID'
        $partDText.Contains($secretName) | Should -BeTrue -Because "Part D must name the secret name $secretName exactly as the workflow reads it"
    }
}
