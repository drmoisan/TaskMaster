Set-StrictMode -Version Latest

BeforeAll {
    $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
    $script:EntryPoint = Join-Path $script:RepoRoot 'scripts/dependencies/Repair-PackageManifestConsistency.ps1'

    # Every fixture is an in-memory string or hashtable and every filesystem dependency is
    # injected, so no temporary file is created and nothing on disk is read except the entry
    # point itself and, in the last context, the repository's own tracked files. Only the three
    # skip-and-proceed cases carry a criterion token in their It name; no Describe or Context
    # name in this file matches the regex AC followed by a digit.

    $script:ManifestText = @'
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="Contoso.Widgets" version="1.0.0" targetFramework="net481" />
  <package id="Fabrikam.Core" version="1.0.0" targetFramework="net481" />
</packages>
'@

    $script:ProjectText = @'
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0">
  <Import Project="..\packages\Contoso.Widgets.1.0.0\build\Contoso.Widgets.props" Condition="Exists('..\packages\Contoso.Widgets.1.0.0\build\Contoso.Widgets.props')" />
  <ItemGroup>
    <Reference Include="Contoso.Widgets, Version=1.0.0.0, Culture=neutral, processorArchitecture=MSIL">
      <HintPath>..\packages\Contoso.Widgets.1.0.0\lib\net472\Contoso.Widgets.dll</HintPath>
    </Reference>
    <Reference Include="Fabrikam.Core, Version=1.0.0.0, Culture=neutral, processorArchitecture=MSIL">
      <HintPath>..\packages\Fabrikam.Core.1.0.0\lib\net472\Fabrikam.Core.dll</HintPath>
    </Reference>
    <Analyzer Include="..\packages\Contoso.Widgets.1.0.0\analyzers\dotnet\cs\Contoso.Widgets.dll" />
  </ItemGroup>
  <Target Name="EnsureNuGetPackageBuildImports" BeforeTargets="PrepareForBuild">
    <Error Condition="!Exists('..\packages\Contoso.Widgets.1.0.0\build\Contoso.Widgets.props')" Text="Missing package" />
  </Target>
</Project>
'@

    $script:AppConfigText = @'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="Contoso.Widgets" publicKeyToken="abcdef0123456789" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.0.0.0" newVersion="1.0.0.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
'@

    # The upgraded package ships a consumable asset; the other ships only a framework net481
    # cannot load, which is what makes the skip decision an asset-level one.
    # Rendered with the line ending the repository's own manifests carry: the renderers emit that
    # ending, so a fixture written with any other one is rewritten by the normalisation pass and
    # the run reads as non-idempotent for a reason belonging to the fixture rather than the code.
    $script:ManifestText = ($script:ManifestText -replace "`r?`n", "`r`n") + "`r`n"
    $script:ProjectText = $script:ProjectText -replace "`r?`n", "`r`n"
    $script:AppConfigText = $script:AppConfigText -replace "`r?`n", "`r`n"

    $script:AssetFolder = @{
        'Contoso.Widgets|2.0.0' = @('net472', 'netstandard2.0')
        'Fabrikam.Core|2.0.0'   = @('netstandard2.1')
    }

    $script:AssemblyIdentity = @{
        'Contoso.Widgets|1.0.0' = @([pscustomobject]@{ AssetFolder = 'net472'; Version = '1.0.0.0' })
        'Contoso.Widgets|2.0.0' = @([pscustomobject]@{ AssetFolder = 'net472'; Version = '2.0.0.0' })
        'Fabrikam.Core|1.0.0'   = @([pscustomobject]@{ AssetFolder = 'net472'; Version = '1.0.0.0' })
    }

    $script:AnalyzerListing = @{
        'Contoso.Widgets|1.0.0' = @('analyzers\dotnet\cs\Contoso.Widgets.dll')
        'Contoso.Widgets|2.0.0' = @(
            'analyzers\dotnet\cs\Contoso.Widgets.dll',
            'analyzers\dotnet\roslyn4.7\cs\Contoso.Widgets.dll',
            'lib\net472\Contoso.Widgets.dll')
    }

    $script:ManifestPath = 'X:\fixture\App\packages.config'
    $script:ProjectPath = 'X:\fixture\App\App.csproj'
    $script:AppConfigPath = 'X:\fixture\App\app.config'

    function Get-RepairFixture {
        <#
        .SYNOPSIS
            Builds an in-memory file store and the delegate set the entry point consumes.
        #>
        param(
            [hashtable]$File,
            [hashtable]$Asset = @{},
            [hashtable]$Identity = @{},
            [hashtable]$Listing = @{}
        )

        $store = @{}
        foreach ($key in $File.Keys) { $store[$key] = $File[$key] }

        # The three maps are read into locals so the closures below capture the locals.
        # PSReviewUnusedParameter cannot see a parameter referenced only inside a nested
        # scriptblock, so a body-level read is what keeps the analyzer clean here.
        $assetMap = $Asset
        $identityMap = $Identity
        $listingMap = $Listing

        return @{
            Store    = $store
            Argument = @{
                RepositoryRoot           = $script:RepoRoot
                DirectoryLister          = { @($store.Keys) }.GetNewClosure()
                TextReader               = { param($Path) [string]$store[$Path] }.GetNewClosure()
                TextWriter               = { param($Path, $Text) $store[$Path] = $Text }.GetNewClosure()
                AssetFolderProvider      = { param($Id, $Version) @($assetMap[($Id + '|' + $Version)]) }.GetNewClosure()
                AssemblyIdentityProvider = { param($Id, $Version) @($identityMap[($Id + '|' + $Version)]) }.GetNewClosure()
                AnalyzerListingProvider  = { param($Id, $Version) @($listingMap[($Id + '|' + $Version)]) }.GetNewClosure()
            }
        }
    }

    function Get-StandardFixture {
        <#
        .SYNOPSIS
            Builds the two-package fixture: one project, its manifest and its application
            configuration, all naming version 1.0.0.
        #>
        param([string]$Project = $script:ProjectText)

        return Get-RepairFixture -File @{
            $script:ManifestPath  = $script:ManifestText
            $script:ProjectPath   = $Project
            $script:AppConfigPath = $script:AppConfigText
        } -Asset $script:AssetFolder -Identity $script:AssemblyIdentity -Listing $script:AnalyzerListing
    }
}

Describe 'Repair-PackageManifestConsistency' {

    Context 'Two candidate upgrades, one of them incompatible' {
        BeforeAll {
            $script:Fixture = Get-StandardFixture
            $argument = $script:Fixture.Argument
            $script:Result = & $script:EntryPoint @argument `
                -CandidateUpgrade @{ 'Contoso.Widgets' = '2.0.0'; 'Fabrikam.Core' = '2.0.0' }
            $script:Manifest = $script:Fixture.Store[$script:ManifestPath]
            $script:Project = $script:Fixture.Store[$script:ProjectPath]
            $script:AppConfig = $script:Fixture.Store[$script:AppConfigPath]
        }

        It 'AC10-1 leaves the incompatible package at the version its manifest already declared' {
            $script:Manifest | Should -Match '<package id="Fabrikam\.Core" version="1\.0\.0"'
            $script:Manifest | Should -Not -Match 'id="Fabrikam\.Core" version="2\.0\.0"'
        }

        It 'AC10-2 writes the target version for the compatible package, so the run proceeded' {
            $script:Manifest | Should -Match '<package id="Contoso\.Widgets" version="2\.0\.0"'
        }

        It 'AC10-3 returns a skip record naming the incompatible package and a reason' {
            $skip = @($script:Result.Skipped)
            $skip.Count | Should -Be 1
            $skip[0].PackageId | Should -Be 'Fabrikam.Core'
            $skip[0].ToVersion | Should -Be '2.0.0'
            [string]::IsNullOrWhiteSpace($skip[0].Reason) | Should -BeFalse
            $skip[0].Reason | Should -Match 'netstandard2\.1'
        }

        It 'records the upgrade it applied, with the asset folder the gate selected' {
            $applied = @($script:Result.Upgraded)
            $applied.Count | Should -Be 1
            $applied[0].PackageId | Should -Be 'Contoso.Widgets'
            $applied[0].FromVersion | Should -Be '1.0.0'
            $applied[0].AssetFolder | Should -Be 'net472'
        }

        It 'reconciles the import guard to the upgraded version' {
            $script:Project | Should -Match 'packages\\Contoso\.Widgets\.2\.0\.0\\build'
        }

        It 'reconciles the error guard to the upgraded version' {
            $script:Project | Should -Match 'Exists\(''\.\.\\packages\\Contoso\.Widgets\.2\.0\.0\\build'
        }

        It 'reconciles the hint path to the upgraded version' {
            $script:Project | Should -Match 'packages\\Contoso\.Widgets\.2\.0\.0\\lib\\net472'
        }

        It 'reconciles the reference to the assembly version the restored package declares' {
            $script:Project | Should -Match 'Include="Contoso\.Widgets, Version=2\.0\.0\.0'
        }

        It 'regenerates the analyzer item, preserving its existing folder segment' {
            $script:Project | Should -Match 'Analyzer Include="\.\.\\packages\\Contoso\.Widgets\.2\.0\.0\\analyzers\\dotnet\\cs\\'
            $script:Project | Should -Not -Match 'roslyn4\.7'
        }

        It 'reconciles the binding redirect of the upgraded package in both positions' {
            $script:AppConfig | Should -Match 'oldVersion="0\.0\.0\.0-2\.0\.0\.0"'
            $script:AppConfig | Should -Match 'newVersion="2\.0\.0\.0"'
        }

        It 'leaves every dependent element of the skipped package at its manifest version' {
            $script:Project | Should -Match 'packages\\Fabrikam\.Core\.1\.0\.0\\lib'
            $script:Project | Should -Match 'Include="Fabrikam\.Core, Version=1\.0\.0\.0'
        }

        It 'reports the paths it wrote' {
            $written = @($script:Result.WrittenPath)
            $written | Should -Contain $script:ManifestPath
            $written | Should -Contain $script:ProjectPath
            $written | Should -Contain $script:AppConfigPath
        }

        It 'reports a repair count matching the repairs its report enumerates' {
            $script:Result.RepairCount | Should -BeGreaterThan 0
            $script:Result.RepairCount | Should -Be @($script:Result.Verification[0].Report.Repair).Count
        }

        It 'carries a skipped block in the body, naming the package and its reason' {
            $script:Result.Body | Should -Match '## Packages skipped'
            $script:Result.Body | Should -Match 'Fabrikam\.Core 1\.0\.0 to 2\.0\.0'
            $script:Result.Body | Should -Match '## Repairs applied'
        }

        It 'returns a success result, a skip not being a failure' {
            $script:Result.IsSuccess | Should -BeTrue
            @($script:Result.Failure).Count | Should -Be 0
        }
    }

    Context 'A tree that already agrees with its manifests' {
        BeforeAll {
            $script:Agreeing = Get-StandardFixture
            $argument = $script:Agreeing.Argument
            $script:AgreeingResult = & $script:EntryPoint @argument
        }

        It 'applies no repair' {
            $script:AgreeingResult.RepairCount | Should -Be 0
            @($script:AgreeingResult.WrittenPath).Count | Should -Be 0
        }

        It 'examines a non-zero element population, so the zero is a measurement' {
            $script:AgreeingResult.ExaminedElementCount | Should -BeGreaterThan 0
            $script:AgreeingResult.ExaminedProjectCount | Should -Be 1
            $script:AgreeingResult.ExaminedManifestCount | Should -Be 1
            $script:AgreeingResult.ExaminedAppConfigCount | Should -Be 1
        }

        It 'confirms the analyzer item against the listing and reports no missing segment' {
            $script:AgreeingResult.ExaminedAnalyzerItemCount | Should -Be 1
            $script:AgreeingResult.AnalyzerProjectCount | Should -Be 1
            $script:AgreeingResult.MissingRoslynSegmentCount | Should -Be 0
        }

        It 'reports no version disagreement' {
            $script:AgreeingResult.VersionDisagreementCount | Should -Be 0
            $script:AgreeingResult.IsSuccess | Should -BeTrue
        }

        It 'carries no skipped block in the body, no package having been skipped' {
            $script:AgreeingResult.Body | Should -Match 'No repairs were applied\.'
            $script:AgreeingResult.Body | Should -Not -Match '## Packages skipped'
        }
    }

    Context 'A run asked what it would change' {
        It 'writes nothing and leaves the store byte-identical' {
            $fixture = Get-StandardFixture
            $before = $fixture.Store[$script:ProjectPath]
            $argument = $fixture.Argument
            $result = & $script:EntryPoint @argument `
                -CandidateUpgrade @{ 'Contoso.Widgets' = '2.0.0' } -WhatIf
            $fixture.Store[$script:ProjectPath] | Should -BeExactly $before
            $fixture.Store[$script:ManifestPath] | Should -BeExactly $script:ManifestText
            @($result.WrittenPath).Count | Should -Be 0
        }
    }

    Context 'An analyzer item whose preserved segment the new listing does not offer' {
        BeforeAll {
            $script:MovedListing = @{
                'Contoso.Widgets|2.0.0' = @('analyzers\dotnet\roslyn4.7\cs\Contoso.Widgets.dll')
            }
            $script:Moved = Get-RepairFixture -File @{
                $script:ManifestPath = $script:ManifestText
                $script:ProjectPath  = $script:ProjectText
            } -Asset $script:AssetFolder -Identity $script:AssemblyIdentity -Listing $script:MovedListing
            $argument = $script:Moved.Argument
            $script:MovedResult = & $script:EntryPoint @argument `
                -CandidateUpgrade @{ 'Contoso.Widgets' = '2.0.0' }
        }

        It 'leaves the analyzer item unmodified rather than guessing a replacement path' {
            $script:Moved.Store[$script:ProjectPath] |
                Should -Match 'Analyzer Include="\.\.\\packages\\Contoso\.Widgets\.1\.0\.0\\analyzers\\dotnet\\cs\\'
        }

        It 'reports the item as a missing-segment finding over a non-zero examined population' {
            $script:MovedResult.MissingRoslynSegmentCount | Should -BeGreaterThan 0
            $script:MovedResult.ExaminedAnalyzerItemCount | Should -Be 1
        }

        It 'does not fail the run, the class being non-fatal' {
            $script:MovedResult.IsSuccess | Should -BeTrue
        }
    }

    Context 'A manifest with no project file beside it' {
        It 'repairs the manifest alone and reports no project' {
            $fixture = Get-RepairFixture -File @{ $script:ManifestPath = $script:ManifestText } `
                -Asset $script:AssetFolder
            $argument = $fixture.Argument
            $result = & $script:EntryPoint @argument `
                -CandidateUpgrade @{ 'Contoso.Widgets' = '2.0.0' }
            $fixture.Store[$script:ManifestPath] | Should -Match 'id="Contoso\.Widgets" version="2\.0\.0"'
            $result.ExaminedProjectCount | Should -Be 0
            $result.ExaminedManifestCount | Should -Be 1
        }
    }

    Context 'An element naming a package no manifest declares' {
        BeforeAll {
            $script:UnmanifestedProject = @'
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0">
  <Import Project="..\packages\altcover.8.6.45\build\netstandard2.0\AltCover.props" Condition="Exists('..\packages\altcover.8.6.45\build\netstandard2.0\AltCover.props')" />
  <ItemGroup>
    <Reference Include="Fabrikam.Core, Version=1.0.0.0, Culture=neutral, processorArchitecture=MSIL">
      <HintPath>..\packages\Fabrikam.Legacy.9.9.9\lib\net472\Fabrikam.Core.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
'@ -replace "`r?`n", "`r`n"
            $script:Unmanifested = Get-RepairFixture -File @{
                $script:ManifestPath = $script:ManifestText
                $script:ProjectPath  = $script:UnmanifestedProject
            } -Identity $script:AssemblyIdentity
            $argument = $script:Unmanifested.Argument
            $script:UnmanifestedResult = & $script:EntryPoint @argument
        }

        It 'reports the guarded import in the absent-from-manifest class' {
            $script:UnmanifestedResult.AbsentFromManifestCount | Should -Be 1
            $report = $script:UnmanifestedResult.Verification[0].Report
            @($report.AbsentFromManifest)[0].Kind | Should -Be 'Import'
        }

        It 'reports the orphaned hint path in its own class rather than in both' {
            $script:UnmanifestedResult.OrphanedHintPathCount | Should -Be 1
            @($script:UnmanifestedResult.Verification[0].Report.AbsentFromManifest).Count | Should -Be 1
        }
    }

    Context 'The default filesystem delegates against the repository itself' {
        BeforeAll {
            # Scoped to one project directory and run with -WhatIf, so the delegates are
            # exercised against real files while the tree is left untouched.
            $script:ScopedRoot = Join-Path $script:RepoRoot 'SVGControl'
            $script:ScopedResult = & $script:EntryPoint -RepositoryRoot $script:ScopedRoot `
                -CandidateUpgrade @{ 'System.Memory' = '99.0.0' } -WhatIf
        }

        It 'discovers the manifest, the project file and the application configuration' {
            $script:ScopedResult.ExaminedManifestCount | Should -Be 1
            $script:ScopedResult.ExaminedAppConfigCount | Should -Be 1
            $script:ScopedResult.ExaminedProjectCount | Should -Be 1
        }

        It 'skips a candidate whose restored package offers no consumable asset' {
            @($script:ScopedResult.Skipped).Count | Should -Be 1
            @($script:ScopedResult.Skipped)[0].PackageId | Should -Be 'System.Memory'
        }

        It 'reports the tree as already consistent and writes nothing' {
            $script:ScopedResult.VersionDisagreementCount | Should -Be 0
            @($script:ScopedResult.WrittenPath).Count | Should -Be 0
            $script:ScopedResult.IsSuccess | Should -BeTrue
        }
    }

    Context 'A run whose only change is a manifest normalisation' {
        BeforeAll {
            # A manifest in the wrapped multi-line form CSharpier produces, beside a project
            # that already agrees with it. The normalisation pass rewrites the manifest to the
            # inline form the NuGet CLI writes; nothing in the project needs repairing.
            $script:WrappedManifest = (@'
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package
    id="Contoso.Widgets"
    version="1.0.0"
    targetFramework="net481" />
</packages>
'@ -replace "`r?`n", "`r`n") + "`r`n"

            $script:AgreeingProject = @'
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0">
  <ItemGroup>
    <Reference Include="Contoso.Widgets, Version=1.0.0.0, Culture=neutral, processorArchitecture=MSIL">
      <HintPath>..\packages\Contoso.Widgets.1.0.0\lib\net472\Contoso.Widgets.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
'@ -replace "`r?`n", "`r`n"

            $script:NormalisationFixture = Get-RepairFixture -File @{
                $script:ManifestPath = $script:WrappedManifest
                $script:ProjectPath  = $script:AgreeingProject
            } -Identity $script:AssemblyIdentity
            $argument = $script:NormalisationFixture.Argument
            $script:NormalisationResult = & $script:EntryPoint @argument
        }

        It 'R3- reports a non-zero write count for a run whose only change is a normalisation' {
            # Assert: the two quantities disagree, which is exactly the silent-discard case.
            # The old push gate read RepairCount and would have seen 0; the new one reads the
            # write-set count and sees 1.
            $script:NormalisationResult.RepairCount |
                Should -Be 0 -Because 'a normalisation produces no per-project repair record'
            @($script:NormalisationResult.WrittenPath).Count |
                Should -Be 1 -Because 'the manifest was rewritten, so the write set must not be empty'
            @($script:NormalisationResult.WrittenPath)[0] |
                Should -BeExactly $script:ManifestPath -Because 'the single written path is the manifest the normalisation reflowed'
        }
    }
}
