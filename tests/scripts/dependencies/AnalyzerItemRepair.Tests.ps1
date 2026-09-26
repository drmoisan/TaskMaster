Set-StrictMode -Version Latest

BeforeAll {
    $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
    Import-Module (Join-Path $script:RepoRoot 'scripts/dependencies/AnalyzerItemRepair.psm1') -Force

    # Every listing below is an in-memory array standing in for the contents of a restored
    # package directory, supplied through the injected delegate. No temporary file is created
    # and no directory on disk is enumerated anywhere in this suite.

    $script:PlainListing = { @('lib\net472\Contoso.Analyzers.dll', 'analyzers\dotnet\cs\Contoso.Analyzers.dll') }

    $script:RoslynListing = { @('analyzers\dotnet\roslyn5.0\cs\Contoso.Analyzers.dll') }

    # Four assemblies, none of them named after the package, which is the Roslynator shape.
    $script:MultiAssemblyListing = {
        @(
            'analyzers\dotnet\roslyn4.7\cs\Roslynator.Core.dll',
            'analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.dll',
            'analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll',
            'analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.CodeFixes.dll'
        )
    }

    # A bare analyzer directory with no intermediate folders, which is the Sonar shape.
    $script:BareListing = { @('analyzers\SonarAnalyzer.CSharp.dll') }

    # One C-sharp assembly beside a Visual Basic one and a satellite resource assembly.
    $script:MixedLanguageListing = {
        @(
            'analyzers\dotnet\cs\Contoso.Analyzers.dll',
            'analyzers\dotnet\vb\Contoso.Analyzers.dll',
            'analyzers\dotnet\cs\de\Contoso.Analyzers.resources.dll'
        )
    }

    $script:NoAnalyzerListing = { @('lib\net472\Contoso.Analyzers.dll', 'build\Contoso.Analyzers.props') }

    # Meziantou ships five Roslyn folders and the committed item names roslyn5.0, which is
    # neither the first nor the highest. A selection implementation fails against this.
    $script:MeziantouListing = {
        @(
            'analyzers\dotnet\roslyn4.14\cs\Meziantou.Analyzer.dll',
            'analyzers\dotnet\roslyn4.8\cs\Meziantou.Analyzer.dll',
            'analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll',
            'analyzers\dotnet\roslyn5.6\cs\Meziantou.Analyzer.dll',
            'analyzers\dotnet\roslyn5.9\cs\Meziantou.Analyzer.dll'
        )
    }

    # Roslynator ships three and the committed item names roslyn4.7, again not the highest.
    $script:RoslynatorListing = {
        @(
            'analyzers\dotnet\roslyn3.8\cs\Roslynator.CSharp.Analyzers.dll',
            'analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll',
            'analyzers\dotnet\roslyn5.0\cs\Roslynator.CSharp.Analyzers.dll'
        )
    }

    # The same package at a version whose listing no longer offers the preserved segment.
    $script:SegmentDroppedListing = {
        @(
            'analyzers\dotnet\roslyn4.8\cs\Meziantou.Analyzer.dll',
            'analyzers\dotnet\roslyn5.9\cs\Meziantou.Analyzer.dll'
        )
    }

    $script:AbsentPackageListing = { @() }

    $script:MeziantouProject = @'
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0">
  <ItemGroup>
    <!-- Issue #181: analyzer-only references, kept out of the compile closure -->
    <Analyzer Include="..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll" />
    <AdditionalFiles Include="$(MSBuildThisFileDirectory)..\BannedSymbols.txt" />
  </ItemGroup>
</Project>
'@

    $script:RoslynatorProject = @'
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0">
  <ItemGroup>
    <Analyzer Include="..\packages\Roslynator.Analyzers.4.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll" />
  </ItemGroup>
</Project>
'@

    # Two separate analyzer item groups, the shape VBFunctions.Test carries at lines 263-265
    # and 287-294. A single-group assumption silently drops one of them.
    $script:TwoGroupProject = @'
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0">
  <ItemGroup>
    <Analyzer Include="..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll" />
  </ItemGroup>
  <ItemGroup Label="Second analyzer group">
    <Analyzer Include="..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.CodeFixes.dll" />
  </ItemGroup>
</Project>
'@

    # The SVGControl shape: no analyzer item group at all.
    $script:NoItemGroupProject = @'
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0">
  <ItemGroup>
    <Compile Include="Program.cs" />
  </ItemGroup>
</Project>
'@
}

Describe 'Analyzer item derivation and repair' {

    Context 'Derivation from an injected directory listing' {

        It 'AC12- derives the item path for a plain language-folder shape' {
            # Arrange: a package whose analyzers sit directly under a language folder.
            $lister = $script:PlainListing

            # Act
            $derived = Get-AnalyzerAssemblyPath -PackageId 'Contoso.Analyzers' -PackageVersion '2.0.0' -DirectoryLister $lister

            # Assert: only the analyzer asset is derived; the lib asset is not an analyzer.
            # The result is wrapped in @() before indexing because PowerShell unrolls a
            # single-element array return to a scalar, and indexing a scalar string yields
            # its first character rather than the path.
            @($derived).Count | Should -Be 1
            @($derived)[0] | Should -BeExactly '..\packages\Contoso.Analyzers.2.0.0\analyzers\dotnet\cs\Contoso.Analyzers.dll'
        }

        It 'AC12- derives the item path for a Roslyn-qualified shape' {
            # Arrange
            $lister = $script:RoslynListing

            # Act
            $derived = Get-AnalyzerAssemblyPath -PackageId 'Contoso.Analyzers' -PackageVersion '2.0.0' -DirectoryLister $lister

            # Assert
            @($derived).Count | Should -Be 1
            @($derived)[0] | Should -BeExactly '..\packages\Contoso.Analyzers.2.0.0\analyzers\dotnet\roslyn5.0\cs\Contoso.Analyzers.dll'
        }

        It 'AC12- derives every assembly for a multi-assembly shape whose names differ from the package' {
            # Arrange: four assemblies, none of them named after the package.
            $lister = $script:MultiAssemblyListing

            # Act
            $derived = Get-AnalyzerAssemblyPath -PackageId 'Roslynator.Analyzers' -PackageVersion '5.0.0' -DirectoryLister $lister

            # Assert: an implementation computing the path from the package identifier fails
            # here, because no assembly is called Roslynator.Analyzers.dll.
            @($derived).Count | Should -Be 4
            $derived | Should -Contain '..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.Core.dll'
            $derived | Should -Contain '..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.CodeFixes.dll'
        }

        It 'AC12- derives the item path for a shape with no intermediate folders' {
            # Arrange: the assembly sits directly in the analyzers directory.
            $lister = $script:BareListing

            # Act
            $derived = Get-AnalyzerAssemblyPath -PackageId 'SonarAnalyzer.CSharp' -PackageVersion '10.34.0.3385' -DirectoryLister $lister

            # Assert: an implementation assuming an intermediate directory fails here.
            @($derived).Count | Should -Be 1
            @($derived)[0] | Should -BeExactly '..\packages\SonarAnalyzer.CSharp.10.34.0.3385\analyzers\SonarAnalyzer.CSharp.dll'
        }

        It 'AC12- excludes non-C-sharp language folders and satellite resource assemblies' {
            # Arrange: one C-sharp assembly beside a Visual Basic one and a satellite.
            $lister = $script:MixedLanguageListing

            # Act
            $derived = Get-AnalyzerAssemblyPath -PackageId 'Contoso.Analyzers' -PackageVersion '2.0.0' -DirectoryLister $lister

            # Assert
            @($derived).Count | Should -Be 1
            @($derived)[0] | Should -BeLike '*analyzers\dotnet\cs\Contoso.Analyzers.dll'
        }

        It 'AC12- contributes no items for a package whose listing has no analyzer directory' {
            # Arrange: a library-only package.
            $lister = $script:NoAnalyzerListing

            # Act
            $derived = Get-AnalyzerAssemblyPath -PackageId 'Contoso.Analyzers' -PackageVersion '2.0.0' -DirectoryLister $lister

            # Assert: an empty derivation, not a throw. The package simply ships no analyzer.
            @($derived).Count | Should -Be 0
        }
    }

    Context 'Repair under the preserve rule' {

        It 'AC12- preserves roslyn5.0 rather than selecting the highest offered folder' {
            # Arrange: the listing offers roslyn5.6 and roslyn5.9 above the roslyn5.0 the
            # existing item names, so a selection implementation moves the folder.
            $project = $script:MeziantouProject

            # Act
            $result = Invoke-AnalyzerItemRepair -ProjectName 'Contoso.Test' -ProjectText $project `
                -PackageId 'Meziantou.Analyzer' -ManifestVersion '3.0.235' -DirectoryLister $script:MeziantouListing

            # Assert: only the version segment moved.
            $line = @($result.Text -split '\r?\n' | Where-Object { $_ -like '*<Analyzer Include=*' })[0]
            $line | Should -BeLike '*..\packages\Meziantou.Analyzer.3.0.235\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll*'
            @($result.RepairedItem).Count | Should -Be 1
            @($result.MissingSegmentRecord).Count | Should -Be 0
        }

        It 'AC12- preserves roslyn4.7 rather than selecting the highest offered folder' {
            # Arrange: the same property for the second analyzer family in use, whose
            # committed items sit at roslyn4.7 while the package ships roslyn5.0.
            $project = $script:RoslynatorProject

            # Act
            $result = Invoke-AnalyzerItemRepair -ProjectName 'Contoso.Test' -ProjectText $project `
                -PackageId 'Roslynator.Analyzers' -ManifestVersion '5.0.0' -DirectoryLister $script:RoslynatorListing

            # Assert
            $line = @($result.Text -split '\r?\n' | Where-Object { $_ -like '*<Analyzer Include=*' })[0]
            $line | Should -BeLike '*..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll*'
            $line | Should -Not -BeLike '*roslyn5.0*'
            @($result.RepairedItem).Count | Should -Be 1
        }

        It 'AC12- leaves the item unmodified and records the missing segment when it is absent' {
            # Arrange: the new version's listing no longer offers the preserved segment.
            $project = $script:MeziantouProject

            # Act
            $result = Invoke-AnalyzerItemRepair -ProjectName 'Contoso.Test' -ProjectText $project `
                -PackageId 'Meziantou.Analyzer' -ManifestVersion '3.0.235' -DirectoryLister $script:SegmentDroppedListing

            # Assert: no guessed path is emitted. The item is returned untouched and a record
            # is raised instead, which ConsistencyVerifier aggregates into a non-fatal class.
            $result.Text | Should -BeExactly $project -Because 'a repair that cannot be derived guesses nothing'
            @($result.RepairedItem).Count | Should -Be 0
            @($result.MissingSegmentRecord).Count | Should -Be 1
            $record = $result.MissingSegmentRecord[0]
            $record.ProjectName | Should -BeExactly 'Contoso.Test'
            $record.Item | Should -BeLike '*Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll'
            $record.MissingSegment | Should -BeExactly 'dotnet\roslyn5.0\cs'
            $record.OfferedSegment | Should -Contain 'dotnet\roslyn4.8\cs'
            $record.OfferedSegment | Should -Contain 'dotnet\roslyn5.9\cs'
        }

        It 'AC12- throws rather than guessing when the restored package directory is absent' {
            # Arrange: the listing delegate returns nothing at all, which means the restored
            # directory for the manifest version does not exist.
            $project = $script:MeziantouProject

            # Act and Assert
            { Invoke-AnalyzerItemRepair -ProjectName 'Contoso.Test' -ProjectText $project `
                    -PackageId 'Meziantou.Analyzer' -ManifestVersion '3.0.235' `
                    -DirectoryLister $script:AbsentPackageListing } |
                Should -Throw -Because 'no path can be derived from an absent directory and a guess is prohibited'
        }
    }

    Context 'Survival of sibling elements and untouched projects' {

        It 'AC13- leaves the AdditionalFiles element and the preceding comment in place' {
            # Arrange: the banned-symbols list and the explanatory comment share the item
            # group with the analyzer item. Dropping the former silently disables an analyzer.
            $project = $script:MeziantouProject

            # Act
            $result = Invoke-AnalyzerItemRepair -ProjectName 'Contoso.Test' -ProjectText $project `
                -PackageId 'Meziantou.Analyzer' -ManifestVersion '3.0.235' -DirectoryLister $script:MeziantouListing

            # Assert
            $result.Text | Should -BeLike '*<AdditionalFiles Include="$(MSBuildThisFileDirectory)..\BannedSymbols.txt" />*'
            $result.Text | Should -BeLike '*<!-- Issue #181: analyzer-only references, kept out of the compile closure -->*'
        }

        It 'AC13- returns a project with no analyzer item group byte-identical' {
            # Arrange: the SVGControl shape, which carries no analyzer item group at all.
            $project = $script:NoItemGroupProject

            # Act
            $result = Invoke-AnalyzerItemRepair -ProjectName 'SVGControl' -ProjectText $project `
                -PackageId 'Meziantou.Analyzer' -ManifestVersion '3.0.235' -DirectoryLister $script:MeziantouListing

            # Assert: nothing is synthesised for a project that never had an item group.
            $result.Text | Should -BeExactly $project
            $result.Text | Should -Not -BeLike '*<Analyzer *'
            $result.ExaminedItemCount | Should -Be 0
        }

        It 'AC13- repairs the items in every analyzer item group rather than the first' {
            # Arrange: two separate analyzer item groups, the VBFunctions.Test shape.
            $project = $script:TwoGroupProject

            # Act
            $result = Invoke-AnalyzerItemRepair -ProjectName 'VBFunctions.Test' -ProjectText $project `
                -PackageId 'Meziantou.Analyzer' -ManifestVersion '3.0.235' -DirectoryLister $script:MeziantouListing

            # Assert: a single-group assumption leaves the second item stale and fails here.
            @($result.RepairedItem).Count | Should -Be 2
            $result.ExaminedItemCount | Should -Be 2
            @($result.Text -split '\r?\n' | Where-Object { $_ -like '*Meziantou.Analyzer.3.0.203*' }) |
                Should -BeNullOrEmpty -Because 'no item may be left at the stale version'
            @($result.Text -split '\r?\n' | Where-Object { $_ -like '*Meziantou.Analyzer.3.0.235*' }).Count |
                Should -Be 2
        }
    }
}
