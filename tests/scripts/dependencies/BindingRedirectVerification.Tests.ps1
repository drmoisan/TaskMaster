Set-StrictMode -Version Latest

BeforeAll {
    $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
    Import-Module (Join-Path $script:RepoRoot 'scripts/dependencies/PackageGraph.psm1') -Force
    Import-Module (Join-Path $script:RepoRoot 'scripts/dependencies/BindingRedirectVerification.psm1') -Force

    # Every fixture below is an in-memory string. The repository-level tests read tracked
    # files through ReadAllText on paths derived from the repository root and write nothing.
    function ConvertTo-CrLf {
        param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text)
        return ($Text -replace "`r?`n", "`r`n")
    }

    function ConvertTo-DependentAssemblyXml {
        param(
            [Parameter(Mandatory = $true)][string]$Name,
            [string]$OldVersion = '',
            [string]$NewVersion = ''
        )
        $redirect = ''
        if ($NewVersion -ne '') {
            $redirect = '        <bindingRedirect oldVersion="{0}" newVersion="{1}" />' -f $OldVersion, $NewVersion
        }
        $lines = @(
            '      <dependentAssembly>',
            ('        <assemblyIdentity name="{0}" publicKeyToken="4ebff4844e382110" culture="neutral" />' -f $Name)
        )
        if ($redirect -ne '') { $lines += $redirect }
        $lines += '      </dependentAssembly>'
        return ($lines -join "`r`n")
    }

    function ConvertTo-AppConfigFixture {
        param([Parameter(Mandatory = $true)][string[]]$Block)
        $head = @(
            '<?xml version="1.0" encoding="utf-8"?>',
            '<configuration>',
            '  <runtime>',
            '    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">'
        ) -join "`r`n"
        $tail = @(
            '    </assemblyBinding>',
            '  </runtime>',
            '</configuration>'
        ) -join "`r`n"
        return ($head + "`r`n" + ($Block -join "`r`n") + "`r`n" + $tail + "`r`n")
    }

    $script:BlockFizzlerStale = ConvertTo-DependentAssemblyXml -Name 'Fizzler' -OldVersion '0.0.0.0-1.3.0.0' -NewVersion '1.3.0.0'
    $script:BlockFizzlerCurrent = ConvertTo-DependentAssemblyXml -Name 'Fizzler' -OldVersion '0.0.0.0-1.3.1.0' -NewVersion '1.3.1.0'
    $script:BlockFizzlerWideRange = ConvertTo-DependentAssemblyXml -Name 'Fizzler' -OldVersion '0.0.0.0-9.9.9.9' -NewVersion '1.3.1.0'
    $script:BlockUnsafe = ConvertTo-DependentAssemblyXml -Name 'System.Runtime.CompilerServices.Unsafe' -OldVersion '0.0.0.0-6.0.3.0' -NewVersion '6.0.3.0'
    $script:BlockUnknown = ConvertTo-DependentAssemblyXml -Name 'Contoso.Unknown' -OldVersion '0.0.0.0-1.0.0.0' -NewVersion '1.0.0.0'
    $script:BlockNoRedirect = ConvertTo-DependentAssemblyXml -Name 'Contoso.NoRedirect'

    # Provider that deploys one version per known assembly and nothing for any other name.
    $script:ProviderSingle = {
        param($Name)
        switch ($Name) {
            'Fizzler' { @('1.3.1.0') }
            'System.Runtime.CompilerServices.Unsafe' { @('6.0.3.0') }
            default { @() }
        }
    }

    # Provider that deploys two versions of Fizzler.
    $script:ProviderTwoVersions = {
        param($Name)
        switch ($Name) {
            'Fizzler' { @('1.3.0.0', '1.3.1.0') }
            default { @() }
        }
    }

    $script:ProjectA = ConvertTo-CrLf -Text (@'
<Project ToolsVersion="15.0">
  <ItemGroup>
    <Reference Include="Fizzler, Version=1.3.1.0, Culture=neutral, PublicKeyToken=4ebff4844e382110">
      <HintPath>..\packages\Fizzler.1.3.1\lib\net47\Fizzler.dll</HintPath>
    </Reference>
    <Reference Include="System.Xml" />
  </ItemGroup>
</Project>

'@)

    $script:ProjectB = ConvertTo-CrLf -Text (@'
<Project ToolsVersion="15.0">
  <ItemGroup>
    <Reference Include="Fizzler, Version=1.3.0.0, Culture=neutral">
      <HintPath>..\packages\Fizzler.1.3.0\lib\net47\Fizzler.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>

'@)
}

Describe 'Find-StaleBindingRedirect (in-memory fixtures)' {

    It 'reports one finding when the Fizzler redirect names 1.3.0.0 and the provider deploys 1.3.1.0' {
        # Arrange: negative control, a stale Fizzler redirect beside a correct Unsafe redirect.
        $text = ConvertTo-AppConfigFixture -Block @($script:BlockFizzlerStale, $script:BlockUnsafe)

        # Act
        $result = Find-StaleBindingRedirect -AppConfigText $text -DeployedVersionProvider $script:ProviderSingle

        # Assert
        $result.Finding.Count | Should -Be 1
        $result.Finding[0].AssemblyName | Should -BeExactly 'Fizzler'
        $result.Finding[0].NewVersion | Should -BeExactly '1.3.0.0'
        $result.Finding[0].DeployedVersions.Count | Should -Be 1
        $result.Finding[0].DeployedVersions[0] | Should -BeExactly '1.3.1.0'
        $result.ExaminedCount | Should -Be 2
    }

    It 'reports no finding when the Fizzler redirect names the deployed 1.3.1.0' {
        # Arrange: positive control, the same shape with the correct Fizzler version.
        $text = ConvertTo-AppConfigFixture -Block @($script:BlockFizzlerCurrent, $script:BlockUnsafe)

        # Act
        $result = Find-StaleBindingRedirect -AppConfigText $text -DeployedVersionProvider $script:ProviderSingle

        # Assert
        $result.Finding.Count | Should -Be 0
        $result.ExaminedCount | Should -Be 2
        $result.Unverifiable.Count | Should -Be 0
    }

    It 'compares newVersion only and ignores the oldVersion range' {
        # Arrange: a wide oldVersion range with a newVersion equal to the deployed version.
        $text = ConvertTo-AppConfigFixture -Block @($script:BlockFizzlerWideRange)

        # Act
        $result = Find-StaleBindingRedirect -AppConfigText $text -DeployedVersionProvider $script:ProviderSingle

        # Assert
        $result.Finding.Count | Should -Be 0
    }

    It 'lists an assembly the provider knows nothing about as unverifiable and not as a finding' {
        # Arrange
        $text = ConvertTo-AppConfigFixture -Block @($script:BlockUnknown)

        # Act
        $result = Find-StaleBindingRedirect -AppConfigText $text -DeployedVersionProvider $script:ProviderSingle

        # Assert
        $result.Unverifiable.Count | Should -Be 1
        $result.Unverifiable[0] | Should -BeExactly 'Contoso.Unknown'
        $result.Finding.Count | Should -Be 0
        $result.ExaminedCount | Should -Be 1
    }

    It 'skips a dependentAssembly block that carries no bindingRedirect' {
        # Arrange: one block with no redirect beside one block with a correct redirect.
        $text = ConvertTo-AppConfigFixture -Block @($script:BlockNoRedirect, $script:BlockFizzlerCurrent)

        # Act
        $result = Find-StaleBindingRedirect -AppConfigText $text -DeployedVersionProvider $script:ProviderSingle

        # Assert
        $result.ExaminedCount | Should -Be 1
        $result.Finding.Count | Should -Be 0
        $result.Unverifiable.Count | Should -Be 0
    }

    It 'counts one examined entry per bindingRedirect-bearing block' {
        # Arrange
        $text = ConvertTo-AppConfigFixture -Block @($script:BlockFizzlerCurrent, $script:BlockUnsafe, $script:BlockUnknown)

        # Act
        $result = Find-StaleBindingRedirect -AppConfigText $text -DeployedVersionProvider $script:ProviderSingle

        # Assert
        $result.ExaminedCount | Should -Be 3
    }

    It 'examines zero entries and reports nothing for empty text' {
        # Arrange
        $act = { Find-StaleBindingRedirect -AppConfigText '' -DeployedVersionProvider $script:ProviderSingle }

        # Act
        $result = & $act

        # Assert
        $act | Should -Not -Throw
        $result.ExaminedCount | Should -Be 0
        $result.Finding.Count | Should -Be 0
        $result.Unverifiable.Count | Should -Be 0
    }

    It 'accepts a newVersion equal to any one of several deployed versions' {
        # Arrange: the provider deploys both 1.3.0.0 and 1.3.1.0.
        $text = ConvertTo-AppConfigFixture -Block @($script:BlockFizzlerStale)

        # Act
        $result = Find-StaleBindingRedirect -AppConfigText $text -DeployedVersionProvider $script:ProviderTwoVersions

        # Assert
        $result.Finding.Count | Should -Be 0
    }

    It 'throws when the text is not an application configuration document' {
        # Arrange
        $act = { Find-StaleBindingRedirect -AppConfigText '<packages />' -DeployedVersionProvider $script:ProviderSingle }

        # Act and Assert: the parser rejects text with no configuration root.
        $act | Should -Throw
    }
}

Describe 'ConvertTo-ReferenceVersionMap (in-memory fixtures)' {

    It 'maps a Reference Include with a Version to its assembly name' {
        # Arrange and Act
        $map = ConvertTo-ReferenceVersionMap -ProjectText @($script:ProjectA)

        # Assert
        $map.ContainsKey('Fizzler') | Should -BeTrue
        $map['Fizzler'].Count | Should -Be 1
        $map['Fizzler'][0] | Should -BeExactly '1.3.1.0'
    }

    It 'omits a Reference Include that declares no Version' {
        # Arrange and Act
        $map = ConvertTo-ReferenceVersionMap -ProjectText @($script:ProjectA)

        # Assert
        $map.ContainsKey('System.Xml') | Should -BeFalse
        $map.Keys.Count | Should -Be 1
    }

    It 'unions the versions of one assembly across several project texts' {
        # Arrange and Act
        $map = ConvertTo-ReferenceVersionMap -ProjectText @($script:ProjectA, $script:ProjectB)

        # Assert
        (@($map['Fizzler'] | Sort-Object) -join ',') | Should -BeExactly '1.3.0.0,1.3.1.0'
    }
}

Describe 'Repository binding redirects (issue 953)' {

    It 'names 1.3.1.0 in every Fizzler binding redirect across the repository app.config files' {
        # Arrange: every app.config directly under a root-level directory.
        $configPath = @(
            Get-ChildItem -LiteralPath $script:RepoRoot -Directory |
                ForEach-Object { Join-Path $_.FullName 'app.config' } |
                Where-Object { Test-Path -LiteralPath $_ }
        )
        $configPath.Count | Should -BeGreaterThan 9

        # Act
        $fizzler = @(
            foreach ($path in $configPath) {
                $text = [System.IO.File]::ReadAllText($path)
                foreach ($record in @(ConvertFrom-AppConfigText -Text $text | Where-Object { $_.Name -eq 'Fizzler' })) {
                    [pscustomobject]@{
                        Config     = Split-Path -Leaf (Split-Path -Parent $path)
                        NewVersion = $record.NewVersion
                        OldVersion = $record.OldVersion
                    }
                }
            }
        )
        $stale = @($fizzler | Where-Object { $_.NewVersion -ne '1.3.1.0' -or $_.OldVersion -ne '0.0.0.0-1.3.1.0' })

        # Assert
        $fizzler.Count | Should -Be 13 -Because 'thirteen configs carry a Fizzler redirect'
        $stale.Count | Should -Be 0 -Because ('these configs redirect Fizzler to another version: ' + (($stale | ForEach-Object { $_.Config + '=' + $_.NewVersion }) -join '; '))
    }

    It 'reports exactly the recorded known-debt set and unverifiable set over every app.config against every csproj Reference' {
        # Arrange: every app.config and every csproj directly under a root-level directory.
        $rootDirectory = @(Get-ChildItem -LiteralPath $script:RepoRoot -Directory)
        $configPath = @(
            $rootDirectory |
                ForEach-Object { Join-Path $_.FullName 'app.config' } |
                Where-Object { Test-Path -LiteralPath $_ }
        )
        $projectPath = @(
            $rootDirectory |
                ForEach-Object { Get-ChildItem -LiteralPath $_.FullName -Filter '*.csproj' -File } |
                ForEach-Object { $_.FullName }
        )
        $configPath.Count | Should -BeGreaterThan 9
        $projectPath.Count | Should -BeGreaterThan 9

        $map = ConvertTo-ReferenceVersionMap -ProjectText @($projectPath | ForEach-Object { [System.IO.File]::ReadAllText($_) })
        $provider = { param($Name) $map[$Name] }.GetNewClosure()
        $expectedDebt = @(
            'Azure.Core|1.62.0.0',
            'Microsoft.Bcl.Memory|10.0.0.7',
            'Microsoft.Bcl.Numerics|10.0.0.5',
            'Microsoft.Extensions.Diagnostics.Abstractions|10.0.0.5',
            'Microsoft.Identity.Client|4.89.0.0',
            'Microsoft.Identity.Client.Extensions.Msal|4.89.0.0',
            'Microsoft.IdentityModel.Abstractions|8.22.0.0',
            'Microsoft.IdentityModel.JsonWebTokens|8.22.0.0',
            'Microsoft.IdentityModel.Logging|8.22.0.0',
            'Microsoft.IdentityModel.Protocols|8.22.0.0',
            'Microsoft.IdentityModel.Protocols.OpenIdConnect|8.22.0.0',
            'Microsoft.IdentityModel.Tokens|8.22.0.0',
            'Microsoft.IdentityModel.Validators|8.22.0.0',
            'System.IdentityModel.Tokens.Jwt|8.22.0.0',
            'System.ClientModel|1.3.0.0'
        )
        $expectedUnverifiable = @('Microsoft.IdentityModel.Clients.ActiveDirectory', 'System.Linq.AsyncEnumerable', 'netstandard')

        # Act
        $redirectElement = 0
        $examined = 0
        $pair = [System.Collections.Generic.List[string]]::new()
        $name = [System.Collections.Generic.List[string]]::new()
        foreach ($path in $configPath) {
            $text = [System.IO.File]::ReadAllText($path)
            $redirectElement += [regex]::Matches($text, '<bindingRedirect\b').Count
            $result = Find-StaleBindingRedirect -AppConfigText $text -DeployedVersionProvider $provider
            $examined += $result.ExaminedCount
            foreach ($finding in $result.Finding) { $pair.Add($finding.AssemblyName + '|' + $finding.NewVersion) }
            foreach ($unverifiable in $result.Unverifiable) { $name.Add($unverifiable) }
        }
        $actualDebt = @($pair | Sort-Object -Unique)
        $actualUnverifiable = @($name | Sort-Object -Unique)

        # Assert
        $redirectElement | Should -BeGreaterThan 0
        $examined | Should -Be $redirectElement -Because 'every bindingRedirect element must be examined'
        $actualDebt | Should -Be @($expectedDebt | Sort-Object -Unique) -Because ('the stale redirect set must equal the recorded known debt; observed: ' + ($actualDebt -join '; '))
        $actualUnverifiable | Should -Be @($expectedUnverifiable | Sort-Object -Unique)
        @($actualDebt | Where-Object { $_ -like 'Fizzler|*' -or $_ -like 'System.Runtime.CompilerServices.Unsafe|*' }).Count | Should -Be 0
    }
}
