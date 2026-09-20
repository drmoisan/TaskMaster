Set-StrictMode -Version Latest

BeforeAll {
    $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
    $script:ScriptPath = Join-Path $script:RepoRoot 'scripts\vscode\Sync-PackageReferences.ps1'
    $script:ModulePath = Join-Path $script:RepoRoot 'scripts\dependencies\PackageCompatibility.psm1'

    Import-Module $script:ModulePath -Force

    # Dot-sourcing defines the script's functions and performs no work: the file ends with
    # the repository's standard invocation guard, so nothing runs against the real tree.
    . $script:ScriptPath

    # Every external boundary is supplied as an in-memory delegate table at the script's own
    # wrapper-function seam. No real executable is mocked, no filesystem path is read, and no
    # temporary file is created anywhere in this suite.
    function Get-AssetSeam {
        param(
            [Parameter(Mandatory = $true)]
            [AllowEmptyCollection()]
            [string[]]$AssetFolder
        )

        $offered = $AssetFolder
        return @{
            TestPath        = { param([string]$Path) $null -ne $Path }.GetNewClosure()
            ListAssetFolder = {
                param([string]$LibraryDirectory)
                if ([string]::IsNullOrEmpty($LibraryDirectory)) { return @() }
                return $offered
            }.GetNewClosure()
        }
    }

    function Get-RepairSeam {
        param(
            [Parameter(Mandatory = $true)]
            [AllowEmptyCollection()]
            [string[]]$AssetFolder,

            [Parameter(Mandatory = $true)]
            [hashtable]$WriteSink
        )

        $offered = $AssetFolder
        $sink = $WriteSink
        $manifestText = '<?xml version="1.0" encoding="utf-8"?><packages><package id="Contoso.Widgets" version="2.0.0" targetFramework="net481" /></packages>'
        $projectText = '<Project><ItemGroup><Reference Include="Contoso.Widgets, Version=1.0.0.0"><HintPath>..\packages\Contoso.Widgets.1.0.0\lib\net45\Contoso.Widgets.dll</HintPath></Reference></ItemGroup></Project>'

        return @{
            ListManifestPath     = { param([string]$Root) if ([string]::IsNullOrEmpty($Root)) { @() } else { @('C:\fake\Proj\packages.config') } }
            ListProjectPath      = { param([string]$Directory) if ([string]::IsNullOrEmpty($Directory)) { @() } else { @('C:\fake\Proj\Proj.csproj') } }
            ListAssetFolder      = {
                param([string]$LibraryDirectory)
                if ([string]::IsNullOrEmpty($LibraryDirectory)) { return @() }
                return $offered
            }.GetNewClosure()
            # The stale hint path never resolves, and version 2.0.0 of the package no longer
            # ships the net45 asset the project was bound to, so the repair cannot simply
            # reuse the existing asset folder and must ask the shared module to select one.
            # Every other probe resolves.
            TestPath             = { param([string]$Path) $Path -notmatch 'Contoso\.Widgets\.1\.0\.0' -and $Path -notmatch 'lib\\net45\\' }
            ReadText             = {
                param([string]$Path)
                if ($Path -like '*packages.config') { return $manifestText }
                return $projectText
            }.GetNewClosure()
            WriteText            = { param([string]$Path, [string]$Text) $sink[$Path] = $Text }.GetNewClosure()
            ReadAssemblyIdentity = { param([string]$Path) if ([string]::IsNullOrEmpty($Path)) { $null } else { [pscustomobject]@{ Name = 'Contoso.Widgets'; Version = '2.0.0.0' } } }
        }
    }
}

Describe 'Sync-PackageReferences framework selection parity with the shared module' {

    Context 'Selection cases resolved through the script wrapper' {

        It 'AC7- resolves net481 through the shared module when net481 is present' {
            # Arrange
            $offered = @('net45', 'netstandard2.0', 'net481')
            $seam = Get-AssetSeam -AssetFolder $offered

            # Act
            $fromScript = Resolve-PackageAssetFolder -LibraryDirectory 'C:\fake\packages\Contoso.2.0.0\lib' -Seam $seam
            $fromModule = Select-CompatibleAssetFolder -AssetFolder $offered

            # Assert: the script agrees with the module, and the agreed answer is the expected one.
            Should -BeExactly $fromModule -ActualValue $fromScript -Because 'the script must not decide selection independently of the shared module'
            $fromScript | Should -BeExactly 'net481' -Because 'net481 is offered and is the most preferred consumable asset folder'
        }

        It 'AC7- resolves net48 through the shared module when net481 is absent' {
            # Arrange
            $offered = @('net45', 'net48', 'netstandard2.0')
            $seam = Get-AssetSeam -AssetFolder $offered

            # Act
            $fromScript = Resolve-PackageAssetFolder -LibraryDirectory 'C:\fake\packages\Contoso.2.0.0\lib' -Seam $seam
            $fromModule = Select-CompatibleAssetFolder -AssetFolder $offered

            # Assert
            Should -BeExactly $fromModule -ActualValue $fromScript -Because 'the script must not decide selection independently of the shared module'
            $fromScript | Should -BeExactly 'net48' -Because 'net48 is the most preferred consumable folder once net481 is unavailable'
        }

        It 'AC7- resolves netstandard2.0 when the package ships netstandard2.1 and netstandard2.0' {
            # Arrange: the excluded framework is offered first in the asset set.
            $offered = @('netstandard2.1', 'netstandard2.0')
            $seam = Get-AssetSeam -AssetFolder $offered

            # Act
            $fromScript = Resolve-PackageAssetFolder -LibraryDirectory 'C:\fake\packages\Contoso.2.0.0\lib' -Seam $seam
            $fromModule = Select-CompatibleAssetFolder -AssetFolder $offered

            # Assert
            Should -BeExactly $fromModule -ActualValue $fromScript -Because 'the script must not decide selection independently of the shared module'
            $fromScript | Should -BeExactly 'netstandard2.0' -Because 'net481 implements no .NET Standard above 2.0'
        }

        It 'AC7- resolves no selection for the three unconsumable asset sets' {
            # Arrange: the three sub-forms AC7 names for the no-selection case.
            $onlyExcluded = @('netstandard2.1')
            $onlyCoreEra = @('net6.0', 'netcoreapp3.1')
            $nothing = @()

            # Act
            $scriptExcluded = Resolve-PackageAssetFolder -LibraryDirectory 'C:\fake\lib' -Seam (Get-AssetSeam -AssetFolder $onlyExcluded)
            $scriptCoreEra = Resolve-PackageAssetFolder -LibraryDirectory 'C:\fake\lib' -Seam (Get-AssetSeam -AssetFolder $onlyCoreEra)
            $scriptNothing = Resolve-PackageAssetFolder -LibraryDirectory 'C:\fake\lib' -Seam (Get-AssetSeam -AssetFolder $nothing)

            # Assert: each agrees with the module and each is a non-selection.
            Should -BeExactly (Select-CompatibleAssetFolder -AssetFolder $onlyExcluded) -ActualValue $scriptExcluded -Because 'the script defers to the module for the excluded framework'
            Should -BeExactly (Select-CompatibleAssetFolder -AssetFolder $onlyCoreEra) -ActualValue $scriptCoreEra -Because 'the script defers to the module for a .NET-Core-era set'
            Should -BeExactly (Select-CompatibleAssetFolder -AssetFolder $nothing) -ActualValue $scriptNothing -Because 'the script defers to the module for an empty set'
            $scriptExcluded | Should -BeNullOrEmpty -Because 'the framework is excluded outright rather than ranked (issue #902)'
            $scriptCoreEra | Should -BeNullOrEmpty -Because 'no .NET-Core-era asset is loadable by .NET Framework 4.8.1'
            $scriptNothing | Should -BeNullOrEmpty -Because 'an empty asset set offers nothing to select'
        }
    }

    Context 'Absence of any ordering local to the script' {

        It 'AC7- declares no ordering of its own, so an asset set the deleted array would have resolved returns no selection' {
            # Arrange: the deleted $tfmPreference array listed netstandard2.1 as its
            # second-to-last member, so against this asset set a fixed ordering returns
            # netstandard2.1. The correct answer is no selection, which is where any surviving
            # local ordering would diverge from the module.
            $rankedButUnconsumable = @('net6.0', 'netstandard2.1')

            # The same set plus one consumable folder is the positive control: it proves the
            # resolver is live and selecting, not merely returning nothing for every input.
            $withConsumable = @('net6.0', 'netstandard2.1', 'net472')

            # Act
            $unconsumableResult = Resolve-PackageAssetFolder -LibraryDirectory 'C:\fake\lib' -Seam (Get-AssetSeam -AssetFolder $rankedButUnconsumable)
            $consumableResult = Resolve-PackageAssetFolder -LibraryDirectory 'C:\fake\lib' -Seam (Get-AssetSeam -AssetFolder $withConsumable)

            # Assert
            $unconsumableResult | Should -BeNullOrEmpty -Because 'a surviving fixed ordering would have returned netstandard2.1 here'
            $consumableResult | Should -BeExactly 'net472' -Because 'the same resolver selects when a consumable asset folder is offered'
        }
    }

    Context 'End-to-end repair driven through the injected seam' {

        It 'AC7- repairs a stale hint path to the asset folder the shared module selects' {
            # Arrange: the manifest declares version 2.0.0, the project still points at 1.0.0,
            # and version 2.0.0 offers an excluded framework alongside one consumable folder
            # that is not the one the project was bound to.
            $written = @{}
            $seam = Get-RepairSeam -AssetFolder @('netstandard2.1', 'net472') -WriteSink $written

            # Act
            $summary = Invoke-PackageReferenceSync -SolutionRoot 'C:\fake' -Seam $seam

            # Assert
            $summary.ExaminedCount | Should -Be 1 -Because 'one manifest was offered by the seam'
            $summary.FixedCount | Should -Be 1 -Because 'the single stale hint path was repaired'
            $written.Count | Should -Be 1 -Because 'exactly one project file was written, through the seam'
            $written['C:\fake\Proj\Proj.csproj'] | Should -BeLike '*lib\net472\Contoso.Widgets.dll*' -Because 'net472 is the asset folder the shared module selects from the offered set'
            $written['C:\fake\Proj\Proj.csproj'] | Should -Not -BeLike '*netstandard2.1*' -Because 'the excluded framework must never be bound to'
            $written['C:\fake\Proj\Proj.csproj'] | Should -BeLike '*Version=2.0.0.0*' -Because 'the reference version is reconciled to the resolved assembly'
        }
    }
}
