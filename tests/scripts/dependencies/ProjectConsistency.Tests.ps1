Set-StrictMode -Version Latest

BeforeAll {
    $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
    Import-Module (Join-Path $script:RepoRoot 'scripts/dependencies/ProjectConsistency.psm1') -Force
    Import-Module (Join-Path $script:RepoRoot 'scripts/dependencies/ConsistencyVerifier.psm1') -Force

    # Every fixture below is an in-memory string or scriptblock. No temporary file is created
    # anywhere in this suite and no project file on disk is read, so the suite is independent
    # of the state of the working tree.

    # Four dependent element kinds, each naming a different version, against a manifest that
    # declares 2.0.0. This is the AC11 fixture and is reused as the AC16 repairable fixture.
    $script:StaleProject = @'
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0">
  <Import Project="..\packages\Contoso.Widgets.1.0.1\build\Contoso.Widgets.props" Condition="Exists('..\packages\Contoso.Widgets.1.0.1\build\Contoso.Widgets.props')" />
  <ItemGroup>
    <Reference Include="Contoso.Widgets, Version=1.0.2, Culture=neutral, processorArchitecture=MSIL">
      <HintPath>..\packages\Contoso.Widgets.1.0.3\lib\net472\Contoso.Widgets.dll</HintPath>
    </Reference>
  </ItemGroup>
  <Target Name="EnsureNuGetPackageBuildImports" BeforeTargets="PrepareForBuild">
    <Error Condition="!Exists('..\packages\Contoso.Widgets.1.0.4\build\Contoso.Widgets.props')" Text="Missing ..\packages\Contoso.Widgets.1.0.4\build\Contoso.Widgets.props" />
  </Target>
</Project>
'@

    # One agreeing Reference and HintPath pair, for the package the one-package manifest
    # declares. Against the two-package manifest the Fabrikam pair is missing.
    $script:PairProject = @'
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0">
  <ItemGroup>
    <Reference Include="Contoso.Widgets, Version=2.0.0, Culture=neutral, processorArchitecture=MSIL">
      <HintPath>..\packages\Contoso.Widgets.2.0.0\lib\net472\Contoso.Widgets.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
'@

    $script:BothPairProject = @'
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0">
  <ItemGroup>
    <Reference Include="Contoso.Widgets, Version=2.0.0, Culture=neutral, processorArchitecture=MSIL">
      <HintPath>..\packages\Contoso.Widgets.2.0.0\lib\net472\Contoso.Widgets.dll</HintPath>
    </Reference>
    <Reference Include="Fabrikam.Core, Version=3.1.0, Culture=neutral, processorArchitecture=MSIL">
      <HintPath>..\packages\Fabrikam.Core.3.1.0\lib\net472\Fabrikam.Core.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
'@

    # Reproduces the issue #903 pre-fix pair: two HintPath entries whose packages the sibling
    # manifest does not declare.
    $script:OrphanProject = @'
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0">
  <ItemGroup>
    <Reference Include="Deedle, Version=3.0.0.0, Culture=neutral, processorArchitecture=MSIL">
      <HintPath>..\packages\Deedle.3.0.0\lib\netstandard2.0\Deedle.dll</HintPath>
    </Reference>
    <Reference Include="FSharp.Core, Version=11.0.0.0, Culture=neutral, processorArchitecture=MSIL">
      <HintPath>..\packages\FSharp.Core.11.0.100\lib\netstandard2.0\FSharp.Core.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
'@

    $script:OnePackageManifest = @'
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="Contoso.Widgets" version="2.0.0" targetFramework="net481" />
</packages>
'@

    $script:TwoPackageManifest = @'
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="Contoso.Widgets" version="2.0.0" targetFramework="net481" />
  <package id="Fabrikam.Core" version="3.1.0" targetFramework="net481" />
</packages>
'@

    $script:RedirectConfig = @'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="Contoso.Widgets" publicKeyToken="1234567890abcdef" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.0.3.0" newVersion="1.0.3.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
'@

    $script:OtherRedirectConfig = @'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="Fabrikam.Core" publicKeyToken="fedcba0987654321" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-3.1.0.0" newVersion="3.1.0.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
'@

    $script:SingleVersionRedirectConfig = @'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="Contoso.Widgets" publicKeyToken="1234567890abcdef" culture="neutral" />
        <bindingRedirect oldVersion="1.0.3.0" newVersion="1.0.3.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
'@

    # The issue #908 three-way divergence: the manifest declares 3.0.235, the Import and Error
    # guards name 3.0.259 and the Analyzer item names 3.0.203.
    $script:DivergentProject = @'
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0">
  <Import Project="..\packages\Meziantou.Analyzer.3.0.259\build\Meziantou.Analyzer.props" Condition="Exists('..\packages\Meziantou.Analyzer.3.0.259\build\Meziantou.Analyzer.props')" />
  <ItemGroup>
    <!-- Issue #181: analyzer-only references -->
    <Analyzer Include="..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll" />
    <AdditionalFiles Include="$(MSBuildThisFileDirectory)..\BannedSymbols.txt" />
  </ItemGroup>
  <Target Name="EnsureNuGetPackageBuildImports" BeforeTargets="PrepareForBuild">
    <Error Condition="!Exists('..\packages\Meziantou.Analyzer.3.0.259\build\Meziantou.Analyzer.props')" Text="Missing ..\packages\Meziantou.Analyzer.3.0.259\build\Meziantou.Analyzer.props" />
  </Target>
</Project>
'@

    $script:DivergentManifest = @'
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="Meziantou.Analyzer" version="3.0.235" targetFramework="net481" developmentDependency="true" />
</packages>
'@

    # Stands in for a listing of the restored package directory. It offers three
    # Roslyn-qualified folders, of which roslyn5.0 is neither the first nor the highest.
    # The delegate signature is the caller's contract, so both parameters are declared even
    # where a fixture consults neither.
    $script:MeziantouListing = {
        param($PackageId, $PackageVersion)
        $null = $PackageId, $PackageVersion
        return @(
            'analyzers\dotnet\roslyn4.14\cs\Meziantou.Analyzer.dll',
            'analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll',
            'analyzers\dotnet\roslyn5.9\cs\Meziantou.Analyzer.dll'
        )
    }

    # Stands in for the consumable library assets each package resolves for net481.
    $script:AssetProvider = {
        param($PackageId, $PackageVersion)
        $null = $PackageVersion
        switch ($PackageId) {
            'Contoso.Widgets' { return @('Contoso.Widgets.dll') }
            'Fabrikam.Core' { return @('Fabrikam.Core.dll') }
            default { return @() }
        }
    }
}

Describe 'Project consistency reconciliation and verification' {

    Context 'Version reconciliation across dependent element kinds' {

        It 'AC11- reconciles the Import guard to the manifest version' {
            # Arrange: the Import guard names 1.0.1 while the manifest declares 2.0.0.
            $project = $script:StaleProject

            # Act
            $result = Invoke-VersionReconciliation -ProjectText $project -PackageId 'Contoso.Widgets' -ManifestVersion '2.0.0'

            # Assert: the assertion is per kind, so a reconciler handling only two kinds fails
            # here rather than passing on an aggregate.
            $line = @($result.Text -split '\r?\n' | Where-Object { $_ -like '*<Import *' })[0]
            $line | Should -BeLike '*packages\Contoso.Widgets.2.0.0\build\Contoso.Widgets.props*' -Because 'the Import guard must name the version the manifest declares'
            $line | Should -Not -BeLike '*Contoso.Widgets.1.0.1*' -Because 'no residual of the stale version may remain on the line'
        }

        It 'AC11- reconciles the Error guard to the manifest version' {
            # Arrange: the Error guard names 1.0.4, a different version again.
            $project = $script:StaleProject

            # Act
            $result = Invoke-VersionReconciliation -ProjectText $project -PackageId 'Contoso.Widgets' -ManifestVersion '2.0.0'

            # Assert
            $line = @($result.Text -split '\r?\n' | Where-Object { $_ -like '*<Error *' })[0]
            $line | Should -BeLike '*packages\Contoso.Widgets.2.0.0\build\Contoso.Widgets.props*' -Because 'the Error guard must name the version the manifest declares'
            $line | Should -Not -BeLike '*Contoso.Widgets.1.0.4*' -Because 'no residual of the stale version may remain on the line'
        }

        It 'AC11- reconciles the Reference assembly version to the manifest version' {
            # Arrange: the Reference Include names Version=1.0.2.
            $project = $script:StaleProject

            # Act
            $result = Invoke-VersionReconciliation -ProjectText $project -PackageId 'Contoso.Widgets' -ManifestVersion '2.0.0'

            # Assert
            $line = @($result.Text -split '\r?\n' | Where-Object { $_ -like '*<Reference Include=*' })[0]
            $line | Should -BeLike '*Version=2.0.0,*' -Because 'the Reference must name the version the manifest declares'
            $line | Should -Not -BeLike '*Version=1.0.2,*' -Because 'no residual of the stale version may remain on the line'
        }

        It 'AC11- reconciles the HintPath to the manifest version' {
            # Arrange: the HintPath names 1.0.3.
            $project = $script:StaleProject

            # Act
            $result = Invoke-VersionReconciliation -ProjectText $project -PackageId 'Contoso.Widgets' -ManifestVersion '2.0.0'

            # Assert
            $line = @($result.Text -split '\r?\n' | Where-Object { $_ -like '*<HintPath>*' })[0]
            $line | Should -BeLike '*packages\Contoso.Widgets.2.0.0\lib\net472\Contoso.Widgets.dll*' -Because 'the HintPath must name the version the manifest declares'
            $line | Should -Not -BeLike '*Contoso.Widgets.1.0.3*' -Because 'no residual of the stale version may remain on the line'
        }
    }

    Context 'Binding redirect reconciliation' {

        It 'AC14- writes the resolved version into both the oldVersion upper bound and newVersion' {
            # Arrange: the redirect names an older assembly version than the resolved one.
            $config = $script:RedirectConfig

            # Act
            $result = Invoke-BindingRedirectReconciliation -AppConfigText $config -AssemblyName 'Contoso.Widgets' -AssemblyVersion '2.0.0.0'

            # Assert: both halves of the redirect move, not only newVersion.
            $line = @($result.Text -split '\r?\n' | Where-Object { $_ -like '*<bindingRedirect *' })[0]
            $line | Should -BeLike '*oldVersion="0.0.0.0-2.0.0.0"*' -Because 'the upper bound of the range must cover the resolved assembly version'
            $line | Should -BeLike '*newVersion="2.0.0.0"*' -Because 'the redirect target must be the resolved assembly version'
        }

        It 'AC14- returns an app.config carrying no redirect for the assembly unchanged' {
            # Arrange: the only redirect present names a different assembly.
            $config = $script:OtherRedirectConfig

            # Act
            $result = Invoke-BindingRedirectReconciliation -AppConfigText $config -AssemblyName 'Contoso.Widgets' -AssemblyVersion '2.0.0.0'

            # Assert: adding a redirect the project never declared is a new decision, not a
            # reconciliation, so the input is returned byte-identical.
            $result.Text | Should -BeExactly $config -Because 'an absent redirect is left absent rather than synthesised'
            @($result.Repair).Count | Should -Be 0 -Because 'no repair was performed'
        }
    }

    Context 'Orphaned hint path detection' {

        It 'AC8- reports a non-empty orphan set for the issue 903 pre-fix pair' {
            # Arrange: two HintPath entries whose packages the manifest does not declare.
            $project = $script:OrphanProject

            # Act
            $detection = Find-OrphanedHintPath -ProjectText $project -ManifestText $script:OnePackageManifest

            # Assert
            $detection.FindingCount | Should -Be 2 -Because 'both hint paths are orphaned by the pre-fix manifest'
            $detection.ExaminedCount | Should -Be 2 -Because 'both hint paths were examined'
            @($detection.Finding | ForEach-Object { $_.PackageFolder }) | Should -Contain 'Deedle.3.0.0'
            @($detection.Finding | ForEach-Object { $_.PackageFolder }) | Should -Contain 'FSharp.Core.11.0.100'
        }

        It 'AC8- reports no orphan and a non-zero examined count when the manifest declares every hint path' {
            # Arrange: the post-fix direction, in which every hint path has a manifest entry.
            $project = $script:BothPairProject

            # Act
            $detection = Find-OrphanedHintPath -ProjectText $project -ManifestText $script:TwoPackageManifest

            # Assert: the examined count guards the zero, so a detector that matched nothing
            # is distinguishable from a clean project.
            $detection.FindingCount | Should -Be 0 -Because 'every hint path has a declaring manifest entry'
            $detection.ExaminedCount | Should -BeGreaterThan 0 -Because 'a zero examined count would mean the detector never fired'
        }
    }

    Context 'Reference completeness' {

        It 'AC23- reports a missing reference when one Reference and HintPath pair is removed' {
            # Arrange: the manifest declares two packages and the project carries the pair for
            # one of them only.
            $project = $script:PairProject

            # Act
            $detection = Test-ReferenceCompleteness -ProjectText $project -ManifestText $script:TwoPackageManifest -AssetProvider $script:AssetProvider

            # Assert: the detector must be demonstrable firing; a check that cannot be made to
            # fail tests nothing.
            $detection.FindingCount | Should -Be 1 -Because 'the Fabrikam.Core asset has no Reference and HintPath pair'
            $detection.Finding[0].PackageId | Should -BeExactly 'Fabrikam.Core'
            $detection.Finding[0].AssetFileName | Should -BeExactly 'Fabrikam.Core.dll'
        }

        It 'AC23- reports no missing reference and a non-zero examined count for a complete fixture' {
            # Arrange: every declared package has its Reference and HintPath pair.
            $project = $script:BothPairProject

            # Act
            $detection = Test-ReferenceCompleteness -ProjectText $project -ManifestText $script:TwoPackageManifest -AssetProvider $script:AssetProvider

            # Assert
            $detection.FindingCount | Should -Be 0 -Because 'every resolved asset has a matching reference'
            $detection.ExaminedCount | Should -BeGreaterThan 0 -Because 'a zero examined count would mean no asset was checked'
        }
    }

    Context 'Repair entry point' {

        It 'AC16- returns a success result whose report enumerates the repairs performed' {
            # Arrange: every divergence in this fixture is repairable by reconciliation.
            $project = $script:StaleProject

            # Act
            $result = Invoke-ProjectConsistencyRepair -ProjectName 'Contoso.Test' -ProjectText $project -ManifestText $script:OnePackageManifest -AssetProvider $script:AssetProvider

            # Assert
            $result.IsSuccess | Should -BeTrue -Because 'no divergence survives the repair passes'
            @($result.Failure).Count | Should -Be 0 -Because 'a success result carries no failure'
            $result.Report.RepairCount | Should -BeGreaterThan 0 -Because 'the report enumerates the repairs performed'
            @($result.Report.Repair | ForEach-Object { $_.Kind }) | Should -Contain 'Import'
            @($result.Report.Repair | ForEach-Object { $_.Kind }) | Should -Contain 'HintPath'
        }

        It 'AC16- returns a failure result naming the condition and the project for an unrepairable divergence' {
            # Arrange: the same project against a manifest declaring a second package for which
            # no Reference and HintPath pair exists. The repair pass never edits a manifest and
            # never synthesises a reference, so this divergence cannot be resolved.
            $project = $script:StaleProject

            # Act
            $result = Invoke-ProjectConsistencyRepair -ProjectName 'Contoso.Test' -ProjectText $project -ManifestText $script:TwoPackageManifest -AssetProvider $script:AssetProvider

            # Assert: the failing direction is what proves the verifier is not a pass-through.
            $result.IsSuccess | Should -BeFalse -Because 'a divergence no repair can resolve remains'
            @($result.Failure).Count | Should -Be 1
            $result.Failure[0].Condition | Should -BeExactly 'MissingReference' -Because 'the failure names the specific condition'
            $result.Failure[0].ProjectName | Should -BeExactly 'Contoso.Test' -Because 'the failure names the project'
        }
    }

    Context 'Three-way divergence from pull request 908' {

        It 'AC21- reports separate guard and analyzer disagreements before repair and reconciles all three locations after' {
            # Arrange: the manifest declares 3.0.235, the guards name 3.0.259 and the analyzer
            # item names 3.0.203.
            $project = $script:DivergentProject

            # Act: detection before repair.
            $before = Find-VersionDisagreement -ProjectText $project -ManifestText $script:DivergentManifest

            # Assert: the two disagreements are reported separately, not folded together.
            $guard = @($before.Finding | Where-Object { $_.Kind -eq 'Import' -or $_.Kind -eq 'Error' })
            $analyzer = @($before.Finding | Where-Object { $_.Kind -eq 'Analyzer' })
            $guard.Count | Should -BeGreaterThan 0 -Because 'the Import and Error guards disagree with the manifest'
            @($guard | ForEach-Object { $_.FoundVersion }) | Should -Contain '3.0.259'
            $analyzer.Count | Should -Be 1 -Because 'one analyzer item disagrees with the manifest'
            $analyzer[0].FoundVersion | Should -BeExactly '3.0.203'

            # Act: repair.
            $result = Invoke-ProjectConsistencyRepair -ProjectName 'Contoso.Test' -ProjectText $project -ManifestText $script:DivergentManifest -AnalyzerListingProvider $script:MeziantouListing

            # Assert: all three locations name the manifest version afterwards.
            $result.IsSuccess | Should -BeTrue
            $after = Find-VersionDisagreement -ProjectText $result.ProjectText -ManifestText $script:DivergentManifest
            $after.FindingCount | Should -Be 0 -Because 'no location disagrees with the manifest after repair'
            $importLine = @($result.ProjectText -split '\r?\n' | Where-Object { $_ -like '*<Import *' })[0]
            $errorLine = @($result.ProjectText -split '\r?\n' | Where-Object { $_ -like '*<Error *' })[0]
            $analyzerLine = @($result.ProjectText -split '\r?\n' | Where-Object { $_ -like '*<Analyzer Include=*' })[0]
            $importLine | Should -BeLike '*Meziantou.Analyzer.3.0.235*'
            $errorLine | Should -BeLike '*Meziantou.Analyzer.3.0.235*'
            $analyzerLine | Should -BeLike '*Meziantou.Analyzer.3.0.235\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll*' -Because 'the preserved folder segment is unchanged and only the version segment moved'
        }
    }

    Context 'Guard clauses and explicit overrides in the reconciliation surface' {

        It 'returns empty project text unchanged with a zero examined count' {
            # Arrange: a caller that handed over a project file it could not read.
            $project = ''

            # Act
            $result = Invoke-VersionReconciliation -ProjectText $project -PackageId 'Contoso.Widgets' -ManifestVersion '2.0.0'

            # Assert: the guard returns rather than throwing, and reports nothing examined.
            $result.Text | Should -BeExactly ''
            @($result.Repair).Count | Should -Be 0
            $result.ExaminedCount | Should -Be 0 -Because 'no element was examined, and the count says so'
        }

        It 'writes the supplied assembly version into the Reference rather than the manifest version' {
            # Arrange: the resolved assembly version differs from the package version, which
            # is the ordinary case for packages such as Castle.Core and FSharp.Core.
            $project = $script:StaleProject

            # Act
            $result = Invoke-VersionReconciliation -ProjectText $project -PackageId 'Contoso.Widgets' `
                -ManifestVersion '2.0.0' -AssemblyVersion '9.9.9.9'

            # Assert: the Reference takes the supplied assembly version and the restore paths
            # still take the manifest version. The two are different quantities.
            $referenceLine = @($result.Text -split '\r?\n' | Where-Object { $_ -like '*<Reference Include=*' })[0]
            $hintLine = @($result.Text -split '\r?\n' | Where-Object { $_ -like '*<HintPath>*' })[0]
            $referenceLine | Should -BeLike '*Version=9.9.9.9,*'
            $hintLine | Should -BeLike '*Contoso.Widgets.2.0.0*'
        }

        It 'returns text that is not an application configuration unchanged with a zero examined count' {
            # Arrange: a document with no configuration root element.
            $config = '<notAConfiguration />'

            # Act
            $result = Invoke-BindingRedirectReconciliation -AppConfigText $config -AssemblyName 'Contoso.Widgets' -AssemblyVersion '2.0.0.0'

            # Assert
            $result.Text | Should -BeExactly $config
            @($result.Repair).Count | Should -Be 0
            $result.ExaminedCount | Should -Be 0
        }

        It 'replaces an oldVersion written as a single version outright' {
            # Arrange: a redirect whose oldVersion is a single version rather than a range,
            # so there is no lower bound to preserve.
            $config = $script:SingleVersionRedirectConfig

            # Act
            $result = Invoke-BindingRedirectReconciliation -AppConfigText $config -AssemblyName 'Contoso.Widgets' -AssemblyVersion '2.0.0.0'

            # Assert
            $line = @($result.Text -split '\r?\n' | Where-Object { $_ -like '*<bindingRedirect *' })[0]
            $line | Should -BeLike '*oldVersion="2.0.0.0"*'
            $line | Should -BeLike '*newVersion="2.0.0.0"*'
        }
    }
}
