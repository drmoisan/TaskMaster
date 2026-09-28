Set-StrictMode -Version Latest

BeforeAll {
    $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
    $script:ModulePath = Join-Path $script:RepoRoot 'scripts/dependencies/PackageCompatibility.psm1'
    Import-Module $script:ModulePath -Force

    # Every fixture below is an in-memory array of folder names. No temporary file is
    # created anywhere in this suite, and no filesystem path is read except the module
    # itself. The arrays stand in for a directory listing of a package's lib folder, which
    # is the asset-level evidence the gate decides from.
    $script:OfferedWithTargetFramework = @('net45', 'netstandard2.0', 'net481')
    $script:OfferedWithoutTargetFramework = @('net45', 'net48', 'netstandard2.0')
    $script:OfferedBothNetStandard = @('netstandard2.1', 'netstandard2.0')
    $script:OfferedOnlyExcludedNetStandard = @('netstandard2.1')
    $script:OfferedOnlyCoreEra = @('net6.0', 'net8.0')
    $script:OfferedNothing = @()
    $script:OfferedOnlyUnconsumable = @('netstandard2.1', 'net6.0', 'netcoreapp3.1')
    $script:OfferedWithConsumable = @('netstandard2.1', 'net6.0', 'net472')
}

Describe 'PackageCompatibility asset selection and gate decisions' {

    Context 'Selector over the asset folders a package ships' {

        It 'returns net481 when net481 is present' {
            # Arrange: the offered set contains the target framework alongside older assets.
            $offered = $script:OfferedWithTargetFramework

            # Act
            $selected = Select-CompatibleAssetFolder -AssetFolder $offered

            # Assert: the most specific consumable folder wins regardless of offered order.
            $selected | Should -BeExactly 'net481' -Because 'net481 is offered and is the most preferred consumable asset folder'
        }

        It 'returns net48 when net481 is absent' {
            # Arrange: the target framework itself is not shipped by the package.
            $offered = $script:OfferedWithoutTargetFramework

            # Act
            $selected = Select-CompatibleAssetFolder -AssetFolder $offered

            # Assert: selection falls back to the next consumable folder in preference order.
            $selected | Should -BeExactly 'net48' -Because 'net48 is the most preferred consumable folder once net481 is unavailable'
        }

        It 'returns netstandard2.0 when offered netstandard2.1 and netstandard2.0 together' {
            # Arrange: the excluded framework is offered first, which a ranking would not prevent.
            $offered = $script:OfferedBothNetStandard

            # Act
            $selected = Select-CompatibleAssetFolder -AssetFolder $offered

            # Assert
            $selected | Should -BeExactly 'netstandard2.0' -Because 'net481 implements no .NET Standard above 2.0, so only the 2.0 asset is loadable'
        }

        It 'returns no selection when offered only netstandard2.1' {
            # Arrange: the single-member set is the case a demotion cannot handle, because a
            # framework ranked last is still selected when it is the only candidate.
            $offered = $script:OfferedOnlyExcludedNetStandard

            # Act
            $selected = Select-CompatibleAssetFolder -AssetFolder $offered

            # Assert
            $selected | Should -BeNullOrEmpty -Because 'the framework is excluded outright rather than ranked below netstandard2.0 (issue #902)'
        }

        It 'returns no selection when offered only a .NET-Core-era framework' {
            # Arrange
            $offered = $script:OfferedOnlyCoreEra

            # Act
            $selected = Select-CompatibleAssetFolder -AssetFolder $offered

            # Assert: exclusion is by non-membership, so it reaches frameworks no deny list names.
            $selected | Should -BeNullOrEmpty -Because 'no .NET-Core-era asset is loadable by .NET Framework 4.8.1'
        }

        It 'returns no selection for an empty set' {
            # Arrange: a package that ships no lib assets at all.
            $offered = $script:OfferedNothing

            # Act
            $selected = Select-CompatibleAssetFolder -AssetFolder $offered

            # Assert
            $selected | Should -BeNullOrEmpty -Because 'an empty asset set offers nothing to select'
        }
    }

    Context 'Gate decision records over the same asset evidence' {

        It 'AC9- returns a rejection carrying a non-empty reason when only unconsumable frameworks are offered' {
            # Arrange: the asset set contains only frameworks net481 cannot consume. The decision
            # is taken from these folder names, not from any declared framework attribute.
            $offered = $script:OfferedOnlyUnconsumable

            # Act
            $decision = Test-PackageAssetCompatibility -PackageId 'Contoso.Widgets' -AssetFolder $offered

            # Assert
            $decision.IsCompatible | Should -BeFalse -Because 'none of the offered asset folders is loadable by net481'
            $decision.Reason | Should -Not -BeNullOrEmpty -Because 'a skipped package must report why it was skipped'
            $decision.Reason | Should -BeLike '*Contoso.Widgets*' -Because 'the reason names the package it rejected'
            $decision.SelectedAssetFolder | Should -BeNullOrEmpty -Because 'a rejection selects nothing'
        }

        It 'AC9- returns an acceptance naming the selected asset folder when a consumable asset is present' {
            # Arrange: the same unconsumable frameworks, plus one asset net481 can load.
            $offered = $script:OfferedWithConsumable

            # Act
            $decision = Test-PackageAssetCompatibility -PackageId 'Contoso.Widgets' -AssetFolder $offered

            # Assert
            $decision.IsCompatible | Should -BeTrue -Because 'net472 is a consumable asset folder'
            $decision.SelectedAssetFolder | Should -BeExactly 'net472' -Because 'the acceptance names the asset folder the caller should bind to'
            $decision.Reason | Should -BeNullOrEmpty -Because 'an acceptance carries no rejection reason'
        }
    }
}
