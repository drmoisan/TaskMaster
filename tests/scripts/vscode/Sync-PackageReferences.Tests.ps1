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

Describe 'Sync-PackageReferences negative and error paths' {

    Context 'Manifest identifier resolution' {

        It 'R2- returns no identifier when the restore folder matches no manifest package' {
            # Arrange: the manifest declares one package and the restore folder names a
            # different one, so no declared identifier prefixes the folder name.
            $versionMap = @{ 'Contoso.Widgets' = '2.0.0' }

            # Act
            $identifier = Resolve-ManifestPackageId -FolderName 'Fabrikam.Core.1.0.0' -VersionMap $versionMap

            # Assert
            $identifier | Should -BeNullOrEmpty -Because 'a restore folder no manifest key prefixes belongs to no declared package'
        }
    }

    Context 'Asset folder resolution against an absent library directory' {

        It 'R2- returns no asset folder when the library directory is absent' {
            # Arrange: every probe reports the path absent, and the enumerator records
            # whether it was reached at all.
            $call = @{ ListAssetFolder = 0; LastLibraryDirectory = '' }
            $seam = @{
                TestPath        = { param([string]$Path) $Path.Length -lt 0 }
                ListAssetFolder = {
                    param([string]$LibraryDirectory)
                    $call['ListAssetFolder'] = $call['ListAssetFolder'] + 1
                    $call['LastLibraryDirectory'] = $LibraryDirectory
                    return @('net481')
                }.GetNewClosure()
            }

            # Act
            $selected = Resolve-PackageAssetFolder -LibraryDirectory 'C:\fake\packages\Contoso.2.0.0\lib' -Seam $seam

            # Assert
            $selected | Should -BeNullOrEmpty -Because 'an absent library directory offers no asset folder to select'
            $call['ListAssetFolder'] | Should -Be 0 -Because 'the function must not enumerate a directory it has not confirmed exists'
        }
    }

    Context 'Compatibility rejection reaching the issue 902 handler' {

        It 'R2- warns and records no repair when no asset folder the target framework can consume ships the file' {
            # Arrange: the project is bound to version 1.0.0 of the package and the manifest
            # declares 2.0.0, so the hint path needs repairing; but 2.0.0 ships only an asset
            # folder .NET Framework 4.8.1 cannot consume.
            $projectText = '<Project><ItemGroup><Reference Include="Contoso.Widgets, Version=1.0.0.0"><HintPath>..\packages\Contoso.Widgets.1.0.0\lib\netstandard2.1\Contoso.Widgets.dll</HintPath></Reference></ItemGroup></Project>'
            $versionMap = @{ 'Contoso.Widgets' = '2.0.0' }

            # Every relative probe reports absent, so neither the current hint path nor the
            # candidate at the corrected version resolves. Every absolute probe reports
            # present, so the library directory and the required file inside the single
            # offered asset folder are both found and the rejection is the compatibility
            # gate's decision rather than a missing file.
            $seam = @{
                TestPath        = { param([string]$Path) -not $Path.Contains('..') }
                ListAssetFolder = {
                    param([string]$LibraryDirectory)
                    if ([string]::IsNullOrEmpty($LibraryDirectory)) { return @() }
                    return @('netstandard2.1')
                }
            }

            # Act
            $repair = @(Get-HintPathRepair -ProjectText $projectText -ProjectDirectory 'C:\fake\Proj' `
                    -PackagesDirectory 'C:\fake\packages' -VersionMap $versionMap -Seam $seam `
                    -WarningVariable rejection -WarningAction SilentlyContinue)

            # Assert
            $repair.Count | Should -Be 0 -Because 'an unconsumable asset set must yield no repair record rather than a guessed one'
            @($rejection).Count | Should -BeGreaterThan 0 -Because 'an empty warning set would satisfy the text assertion below vacuously'
            (@($rejection) -join ' ') | Should -BeLike '*no asset folder the target framework can consume ships it*' -Because 'the rejection must be reported in the run log, not skipped silently'
        }
    }

    Context 'Reference version rewriting that must not fire' {

        It 'R2- returns the project text unchanged when no Reference names the assembly' {
            # Arrange: the assembly name appears in no Include attribute of the text.
            $projectText = '<Project><ItemGroup><Reference Include="Contoso.Widgets, Version=1.0.0.0" /></ItemGroup></Project>'

            # Act
            $result = Repair-ProjectReferenceVersion -ProjectText $projectText `
                -AssemblyName 'Fabrikam.Core' -AssemblyVersion '9.9.9.9'

            # Assert
            $result | Should -BeExactly $projectText -Because 'an assembly the text never names must not cause any rewrite'
        }

        It 'R2- returns the project text unchanged when the Reference already names the resolved version' {
            # Arrange: the Include attribute already declares the four-part version that will
            # be supplied as the resolved one.
            $projectText = '<Project><ItemGroup><Reference Include="Contoso.Widgets, Version=2.0.0.0" /></ItemGroup></Project>'

            # Act
            $result = Repair-ProjectReferenceVersion -ProjectText $projectText `
                -AssemblyName 'Contoso.Widgets' -AssemblyVersion '2.0.0.0'

            # Assert
            $result | Should -BeExactly $projectText -Because 'an already-correct version must not be rewritten, so applying the repair twice is a no-op'
        }
    }

    Context 'Per-project sync outcomes that produce no write' {

        It 'R2- skips the manifest directory when no project file sits beside it' {
            # Arrange: the directory holds a manifest and no project file, and the reader
            # records whether it was reached at all.
            $call = @{ ReadText = 0; LastPath = '' }
            $seam = @{
                ListProjectPath = {
                    param([string]$Directory)
                    # The directory under test holds no project file; any other directory
                    # would, so the empty result is a property of this fixture rather than
                    # of an enumerator that returns nothing whatever it is asked.
                    if ($Directory -eq 'C:\fake\Proj') { return @() }
                    return @('C:\fake\Other\Other.csproj')
                }
                ReadText        = {
                    param([string]$Path)
                    $call['ReadText'] = $call['ReadText'] + 1
                    $call['LastPath'] = $Path
                    return ''
                }.GetNewClosure()
            }

            # Act
            $result = Invoke-ProjectReferenceSync -ManifestPath 'C:\fake\Proj\packages.config' `
                -PackagesDirectory 'C:\fake\packages' -Seam $seam

            # Assert
            $result.Skipped | Should -BeTrue -Because 'a directory with no project file is skipped rather than examined'
            $result.FixedCount | Should -Be 0 -Because 'nothing can be repaired where nothing was read'
            $call['ReadText'] | Should -Be 0 -Because 'the function must not read a project file it never found'
        }

        It 'R2- skips the project with a warning when merge conflict markers are present' {
            # Arrange: the project file carries a seven-character conflict marker.
            $conflicted = ('<' * 7) + " HEAD`n<Project></Project>"
            $seam = @{
                ListProjectPath = {
                    param([string]$Directory)
                    if ([string]::IsNullOrEmpty($Directory)) { return @() }
                    return @('C:\fake\Proj\Proj.csproj')
                }
                ReadText        = {
                    param([string]$Path)
                    # The conflicted text belongs to the project file. The manifest branch
                    # exists so the delegate answers by path rather than unconditionally; the
                    # skip returns before the manifest is ever read.
                    if ($Path -like '*packages.config') { return '' }
                    return $conflicted
                }.GetNewClosure()
            }

            # Act
            $result = Invoke-ProjectReferenceSync -ManifestPath 'C:\fake\Proj\packages.config' `
                -PackagesDirectory 'C:\fake\packages' -Seam $seam `
                -WarningVariable conflictWarning -WarningAction SilentlyContinue

            # Assert
            $result.Skipped | Should -BeTrue -Because 'a conflicted project must not be rewritten, which would corrupt an in-progress merge'
            $result.FixedCount | Should -Be 0 -Because 'a skipped project repairs nothing'
            @($conflictWarning).Count | Should -BeGreaterThan 0 -Because 'an empty warning set would satisfy the text assertion below vacuously'
            (@($conflictWarning) -join ' ') | Should -BeLike '*Merge conflict markers detected, skipping*' -Because 'the skip must be reported rather than silent'
        }

        It 'R2- returns an unskipped result with no fix when no hint path needs repair' {
            # Arrange: one project file whose every hint path already resolves, so the repair
            # set is empty and nothing is written.
            $call = @{ WriteText = 0; WrittenPath = ''; WrittenText = '' }
            $clean = '<Project><ItemGroup><Reference Include="Contoso.Widgets, Version=2.0.0.0"><HintPath>..\packages\Contoso.Widgets.2.0.0\lib\net481\Contoso.Widgets.dll</HintPath></Reference></ItemGroup></Project>'
            $manifest = '<?xml version="1.0" encoding="utf-8"?><packages><package id="Contoso.Widgets" version="2.0.0" targetFramework="net481" /></packages>'
            $seam = @{
                ListProjectPath = {
                    param([string]$Directory)
                    if ([string]::IsNullOrEmpty($Directory)) { return @() }
                    return @('C:\fake\Proj\Proj.csproj')
                }
                TestPath        = { param([string]$Path) $Path.Length -gt 0 }
                ReadText        = {
                    param([string]$Path)
                    if ($Path -like '*packages.config') { return $manifest }
                    return $clean
                }.GetNewClosure()
                WriteText       = {
                    param([string]$Path, [string]$Text)
                    $call['WriteText'] = $call['WriteText'] + 1
                    $call['WrittenPath'] = $Path
                    $call['WrittenText'] = $Text
                }.GetNewClosure()
            }

            # Act
            $result = Invoke-ProjectReferenceSync -ManifestPath 'C:\fake\Proj\packages.config' `
                -PackagesDirectory 'C:\fake\packages' -Seam $seam

            # Assert: this is the second of the two zero-fix outcomes, and it is distinguished
            # from the skipped one by Skipped being false.
            $result.Skipped | Should -BeFalse -Because 'the project was examined, not skipped'
            $result.FixedCount | Should -Be 0 -Because 'every hint path already resolved, so nothing needed repairing'
            $call['WriteText'] | Should -Be 0 -Because 'a clean project must not be written back, which would dirty the tree on every run'
        }
    }

    Context 'Aggregate summary when the run repairs nothing' {

        It 'R2- reports the up-to-date outcome for the whole run when every hint path already resolves' {
            # Arrange: one manifest whose sibling project needs no repair, driven through the
            # top-level entry point rather than the per-project one. This is the zero-fix arm of
            # the aggregate summary. Its non-zero counterpart is already driven by the
            # end-to-end repair test, which returns a FixedCount of 1; nothing reached this arm.
            $call = @{ WriteText = 0; WrittenPath = ''; WrittenText = '' }
            $clean = '<Project><ItemGroup><Reference Include="Contoso.Widgets, Version=2.0.0.0"><HintPath>..\packages\Contoso.Widgets.2.0.0\lib\net481\Contoso.Widgets.dll</HintPath></Reference></ItemGroup></Project>'
            $manifest = '<?xml version="1.0" encoding="utf-8"?><packages><package id="Contoso.Widgets" version="2.0.0" targetFramework="net481" /></packages>'
            $seam = @{
                ListManifestPath = {
                    param([string]$Root)
                    if ([string]::IsNullOrEmpty($Root)) { return @() }
                    return @('C:\fake\Proj\packages.config')
                }
                ListProjectPath  = {
                    param([string]$Directory)
                    if ([string]::IsNullOrEmpty($Directory)) { return @() }
                    return @('C:\fake\Proj\Proj.csproj')
                }
                TestPath         = { param([string]$Path) $Path.Length -gt 0 }
                ReadText         = {
                    param([string]$Path)
                    if ($Path -like '*packages.config') { return $manifest }
                    return $clean
                }.GetNewClosure()
                WriteText        = {
                    param([string]$Path, [string]$Text)
                    $call['WriteText'] = $call['WriteText'] + 1
                    $call['WrittenPath'] = $Path
                    $call['WrittenText'] = $Text
                }.GetNewClosure()
            }

            # Act: the information stream is captured because the summary line is the only
            # observable this arm produces; the returned counts alone cannot distinguish it
            # from a run that examined nothing at all.
            $summary = Invoke-PackageReferenceSync -SolutionRoot 'C:\fake' -Seam $seam -InformationVariable record

            # Assert
            $summary.ExaminedCount | Should -Be 1 -Because 'the one manifest the seam offered was examined'
            $summary.FixedCount | Should -Be 0 -Because 'every hint path already resolved, so the run repaired nothing'
            $call['WriteText'] | Should -Be 0 -Because 'a clean tree must not be written back, which would dirty it on every run'
            (@($record | ForEach-Object { [string]$_ }) -join "`n") | Should -BeLike '*All HintPaths are up to date*' -Because 'the run must report the up-to-date outcome rather than finish silently'
        }
    }
}
