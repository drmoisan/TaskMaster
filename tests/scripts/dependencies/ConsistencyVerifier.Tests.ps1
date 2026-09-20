Set-StrictMode -Version Latest

BeforeAll {
    $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
    Import-Module (Join-Path $script:RepoRoot 'scripts/dependencies/ConsistencyVerifier.psm1') -Force

    # Every fixture is an in-memory string, hashtable or scriptblock. No temporary file is
    # created and no file on disk is read except the module itself. No block or test name in
    # this file matches the regex AC followed by a digit: these are module-level cases and
    # must never enter a criterion-filtered population.

    # An agreeing project: the manifest version appears in all four folder-bearing kinds,
    # and the reference and hint path pair is complete.
    $script:AgreeingProject = @'
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0">
  <Import Project="..\packages\Contoso.Widgets.2.0.0\build\Contoso.Widgets.props" Condition="Exists('..\packages\Contoso.Widgets.2.0.0\build\Contoso.Widgets.props')" />
  <ItemGroup>
    <Reference Include="Contoso.Widgets, Version=2.0.0, Culture=neutral, processorArchitecture=MSIL">
      <HintPath>..\packages\Contoso.Widgets.2.0.0\lib\net472\Contoso.Widgets.dll</HintPath>
    </Reference>
    <Analyzer Include="..\packages\Contoso.Widgets.2.0.0\analyzers\dotnet\cs\Contoso.Widgets.dll" />
  </ItemGroup>
  <Target Name="EnsureNuGetPackageBuildImports" BeforeTargets="PrepareForBuild">
    <Error Condition="!Exists('..\packages\Contoso.Widgets.2.0.0\build\Contoso.Widgets.props')" Text="Missing ..\packages\Contoso.Widgets.2.0.0\build\Contoso.Widgets.props" />
  </Target>
</Project>
'@

    # The same project with the analyzer item left behind at an earlier version, which is
    # the shape issue #898 reports.
    $script:StaleAnalyzerProject = $script:AgreeingProject.Replace(
        'Contoso.Widgets.2.0.0\analyzers', 'Contoso.Widgets.1.0.0\analyzers')

    # The same project with the hint path removed, so the resolved asset has no binding.
    $script:MissingHintProject = @'
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0">
  <ItemGroup>
    <Reference Include="Contoso.Widgets, Version=2.0.0, Culture=neutral, processorArchitecture=MSIL" />
  </ItemGroup>
</Project>
'@

    # A hint path whose package the manifest does not declare.
    $script:OrphanProject = @'
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0">
  <ItemGroup>
    <Reference Include="Fabrikam.Core, Version=3.1.0, Culture=neutral, processorArchitecture=MSIL">
      <HintPath>..\packages\Fabrikam.Core.3.1.0\lib\net472\Fabrikam.Core.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
'@

    # Reproduces QuickFiler.Test/QuickFiler.Test.csproj lines 8 and 514: two Exists() guarded
    # Import elements naming a package no manifest declares, with no matching Error guard, so
    # the build is unaffected. Tracked separately; no exception is hard-coded for it here.
    $script:GuardedUnmanifestedProject = @'
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0">
  <Import Project="..\packages\altcover.8.6.45\build\netstandard2.0\AltCover.props" Condition="Exists('..\packages\altcover.8.6.45\build\netstandard2.0\AltCover.props')" />
  <ItemGroup>
    <Reference Include="Contoso.Widgets, Version=2.0.0, Culture=neutral, processorArchitecture=MSIL">
      <HintPath>..\packages\Contoso.Widgets.2.0.0\lib\net472\Contoso.Widgets.dll</HintPath>
    </Reference>
  </ItemGroup>
  <Import Project="..\packages\altcover.8.6.45\build\netstandard2.0\AltCover.targets" Condition="Exists('..\packages\altcover.8.6.45\build\netstandard2.0\AltCover.targets')" />
</Project>
'@

    $script:Manifest = @'
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="Contoso.Widgets" version="2.0.0" targetFramework="net481" />
</packages>
'@

    # The delegate signature is the caller's contract, so both parameters are declared even
    # where a fixture consults only one of them.
    $script:AssetProvider = {
        param($PackageId, $PackageVersion)
        $null = $PackageVersion
        if ($PackageId -eq 'Contoso.Widgets') { return @('Contoso.Widgets.dll') }
        return @()
    }

    # Stands in for the records the analyzer repair returns when a preserved folder segment
    # is absent from the new version's listing. The derivation itself is owned by
    # AnalyzerItemRepair.Tests.ps1; these cases assert the aggregation and the count only.
    $script:SegmentRecord = @(
        [pscustomobject]@{
            ProjectName    = 'Contoso.Test'
            LineNumber     = 12
            Item           = '..\packages\Contoso.Widgets.2.0.0\analyzers\dotnet\roslyn5.0\cs\Contoso.Widgets.dll'
            MissingSegment = 'dotnet\roslyn5.0\cs'
            OfferedSegment = @('dotnet\roslyn4.8\cs', 'dotnet\roslyn5.9\cs')
        },
        [pscustomobject]@{
            ProjectName    = 'Contoso.Test'
            LineNumber     = 13
            Item           = '..\packages\Contoso.Widgets.2.0.0\analyzers\dotnet\roslyn5.0\cs\Contoso.Widgets.Extra.dll'
            MissingSegment = 'dotnet\roslyn5.0\cs'
            OfferedSegment = @('dotnet\roslyn4.8\cs', 'dotnet\roslyn5.9\cs')
        }
    )
}

Describe 'ConsistencyVerifier detection surfaces' {

    Context 'Version disagreement' {

        It 'reports no disagreement and a non-zero examined count for an agreeing project' {
            # Arrange
            $project = $script:AgreeingProject

            # Act
            $detection = Find-VersionDisagreement -ProjectText $project -ManifestText $script:Manifest

            # Assert: the examined count guards the zero finding count.
            $detection.FindingCount | Should -Be 0 -Because 'every dependent element names the manifest version'
            $detection.ExaminedCount | Should -BeGreaterThan 0 -Because 'a zero examined count would mean the detector never fired'
            $detection.ExaminedAnalyzerCount | Should -Be 1 -Because 'the project carries one analyzer item'
        }

        It 'reports a disagreement for a project whose analyzer item names a stale version' {
            # Arrange: only the analyzer item was left behind, which is the issue 898 shape.
            $project = $script:StaleAnalyzerProject

            # Act
            $detection = Find-VersionDisagreement -ProjectText $project -ManifestText $script:Manifest

            # Assert
            $detection.FindingCount | Should -Be 1
            $detection.Finding[0].Kind | Should -BeExactly 'Analyzer'
            $detection.Finding[0].FoundVersion | Should -BeExactly '1.0.0'
            $detection.Finding[0].ExpectedVersion | Should -BeExactly '2.0.0'
        }
    }

    Context 'Orphaned hint path' {

        It 'reports no orphan and a non-zero examined count when the manifest declares the package' {
            # Arrange
            $project = $script:AgreeingProject

            # Act
            $detection = Find-OrphanedHintPath -ProjectText $project -ManifestText $script:Manifest

            # Assert
            $detection.FindingCount | Should -Be 0
            $detection.ExaminedCount | Should -BeGreaterThan 0 -Because 'the hint path population must be non-empty for the zero to mean anything'
        }

        It 'reports an orphan when the manifest declares no matching package' {
            # Arrange
            $project = $script:OrphanProject

            # Act
            $detection = Find-OrphanedHintPath -ProjectText $project -ManifestText $script:Manifest

            # Assert
            $detection.FindingCount | Should -Be 1
            $detection.Finding[0].PackageFolder | Should -BeExactly 'Fabrikam.Core.3.1.0'
        }
    }

    Context 'Reference completeness' {

        It 'reports no missing reference and a non-zero examined count for a complete project' {
            # Arrange
            $project = $script:AgreeingProject

            # Act
            $detection = Test-ReferenceCompleteness -ProjectText $project -ManifestText $script:Manifest -AssetProvider $script:AssetProvider

            # Assert
            $detection.FindingCount | Should -Be 0
            $detection.ExaminedCount | Should -Be 1 -Because 'one resolved asset was checked'
        }

        It 'reports a missing reference when the hint path for a resolved asset is absent' {
            # Arrange: the Reference survives but its HintPath does not, so the asset has no
            # binding on disk.
            $project = $script:MissingHintProject

            # Act
            $detection = Test-ReferenceCompleteness -ProjectText $project -ManifestText $script:Manifest -AssetProvider $script:AssetProvider

            # Assert
            $detection.FindingCount | Should -Be 1
            $detection.Finding[0].AssetFileName | Should -BeExactly 'Contoso.Widgets.dll'
            $detection.Finding[0].HasHintPath | Should -BeFalse
        }
    }

    Context 'Package absent from the manifest' {

        It 'reports nothing absent and a non-zero examined count when every element has an entry' {
            # Arrange
            $project = $script:AgreeingProject

            # Act
            $detection = Find-PackageAbsentFromManifest -ProjectText $project -ManifestText $script:Manifest

            # Assert
            $detection.FindingCount | Should -Be 0
            $detection.ExaminedCount | Should -BeGreaterThan 0
        }

        It 'reports an element whose package the manifest does not declare' {
            # Arrange
            $project = $script:OrphanProject

            # Act
            $detection = Find-PackageAbsentFromManifest -ProjectText $project -ManifestText $script:Manifest

            # Assert
            $detection.FindingCount | Should -Be 1
            $detection.Finding[0].PackageFolder | Should -BeExactly 'Fabrikam.Core.3.1.0'
        }
    }

    Context 'Missing Roslyn segment aggregation' {

        It 'aggregates no finding and a non-zero examined item count when the repair returned no record' {
            # Arrange: the repair examined items and found every preserved segment present.
            $record = @()

            # Act
            $detection = Get-MissingRoslynSegmentFinding -MissingSegmentRecord $record -ExaminedItemCount 3

            # Assert: the examined count is what separates a clean aggregation from an
            # aggregation over nothing.
            $detection.FindingCount | Should -Be 0
            $detection.ExaminedCount | Should -Be 3
        }

        It 'aggregates and counts the records the analyzer repair returned' {
            # Arrange: two records, supplied as if returned on an AnalyzerItemRepair result.
            # This asserts the aggregation and the count, not the derivation.
            $record = $script:SegmentRecord

            # Act
            $detection = Get-MissingRoslynSegmentFinding -MissingSegmentRecord $record -ExaminedItemCount 4

            # Assert
            $detection.FindingCount | Should -Be 2
            $detection.ExaminedCount | Should -Be 4
            @($detection.Finding | ForEach-Object { $_.MissingSegment }) | Should -Contain 'dotnet\roslyn5.0\cs'
            $detection.Finding[0].OfferedSegment | Should -Contain 'dotnet\roslyn5.9\cs'
        }
    }

    Context 'Guarded import of a package no manifest declares' {

        It 'reports exactly 2 guarded imports of an unmanifested package and still succeeds' {
            # Arrange: the live shape from QuickFiler.Test, reproduced in memory. Both imports
            # are Exists() guarded and carry no matching Error element, so the build is
            # unaffected, but the invariant this tooling enforces is still violated.
            $project = $script:GuardedUnmanifestedProject

            # Act
            $result = Invoke-ProjectConsistencyRepair -ProjectName 'Contoso.Test' -ProjectText $project -ManifestText $script:Manifest

            # Assert: the class is reported, named and counted, and it is not fatal.
            $result.Report.AbsentFromManifestCount | Should -Be 2 -Because 'both guarded imports name the unmanifested package'
            @($result.Report.AbsentFromManifest | ForEach-Object { $_.PackageFolder }) |
                Should -Contain 'altcover.8.6.45'
            $result.IsSuccess | Should -BeTrue -Because 'the class is reported rather than fatal, and no exception is hard-coded for the package'
            @($result.Failure).Count | Should -Be 0
        }
    }
}
