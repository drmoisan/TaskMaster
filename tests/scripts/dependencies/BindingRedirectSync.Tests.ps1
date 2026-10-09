Set-StrictMode -Version Latest

BeforeAll {
    $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
    Import-Module (Join-Path $script:RepoRoot 'scripts/dependencies/BindingRedirectSync.psm1') -Force

    # Every fixture is an in-memory string or hashtable store; nothing on disk is read except the
    # module itself and nothing is written. No block or test name in this file matches the regex
    # AC followed by a digit.

    function Get-AppConfigText {
        <#
        .SYNOPSIS
            Renders an application configuration with one dependentAssembly block per entry,
            each entry being a name, an oldVersion and a newVersion, with CRLF line endings.
        #>
        param([Parameter(Mandatory = $true)][object[]]$Redirect)

        $block = foreach ($entry in $Redirect) {
            '      <dependentAssembly>'
            '        <assemblyIdentity name="' + $entry[0] + '" publicKeyToken="0123456789abcdef" culture="neutral" />'
            '        <bindingRedirect oldVersion="' + $entry[1] + '" newVersion="' + $entry[2] + '" />'
            '      </dependentAssembly>'
        }
        $line = @('<?xml version="1.0" encoding="utf-8"?>', '<configuration>', '  <runtime>',
            '    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">') + @($block) +
        @('    </assemblyBinding>', '  </runtime>', '</configuration>')
        return ($line -join "`r`n")
    }

    function Get-VersionProvider {
        <#
        .SYNOPSIS
            Returns a delegate that looks an assembly name up in the supplied map.
        #>
        param([Parameter(Mandatory = $true)][hashtable]$Map)

        $lookup = $Map
        return { param($Name) $lookup[$Name] }.GetNewClosure()
    }

    function Get-ProjectText {
        <#
        .SYNOPSIS
            Renders a project file referencing log4net at the given assembly version.
        #>
        param([Parameter(Mandatory = $true)][string]$Version)

        $line = @('<?xml version="1.0" encoding="utf-8"?>', '<Project ToolsVersion="15.0">', '  <ItemGroup>',
            ('    <Reference Include="log4net, Version=' + $Version + ', Culture=neutral, PublicKeyToken=669e0ddf0bb1aa2a, processorArchitecture=MSIL">'),
            '      <HintPath>..\packages\log4net.3.5.0\lib\net462\log4net.dll</HintPath>',
            '    </Reference>', '  </ItemGroup>', '</Project>')
        return ($line -join "`r`n")
    }

    function Get-SolutionStore {
        <#
        .SYNOPSIS
            Builds the two-project store: Prod references log4net 3.5.0.0 and redirects to it;
            Test carries only an application configuration redirecting log4net to 3.4.0.0. A
            project file under a restore directory, referencing 3.4.0.0, must be ignored.
        #>
        return @{
            'X:\fixture\Prod\Prod.csproj'                       = (Get-ProjectText -Version '3.5.0.0')
            'X:\fixture\Prod\app.config'                        = (Get-AppConfigText -Redirect @(, @('log4net', '0.0.0.0-3.5.0.0', '3.5.0.0')))
            'X:\fixture\Test\app.config'                        = (Get-AppConfigText -Redirect @(, @('log4net', '0.0.0.0-3.4.0.0', '3.4.0.0')))
            'X:\fixture\packages\log4net.3.4.0\Restored.csproj' = (Get-ProjectText -Version '3.4.0.0')
            'X:\fixture\readme.txt'                             = 'not a project'
        }
    }

    function Get-StoreDelegate {
        <#
        .SYNOPSIS
            Returns the lister, reader and writer delegates over an in-memory store.
        #>
        param([Parameter(Mandatory = $true)][hashtable]$Store)

        $file = $Store
        return @{
            DirectoryLister = { @($file.Keys) }.GetNewClosure()
            TextReader      = { param($Path) [string]$file[$Path] }.GetNewClosure()
            TextWriter      = { param($Path, $Text) $file[$Path] = $Text }.GetNewClosure()
        }
    }
}

Describe 'Invoke-BindingRedirectSync (in-memory fixtures)' {

    It 'rewrites a stale redirect in both positions and keeps the oldVersion lower bound' {
        # Arrange
        $text = Get-AppConfigText -Redirect @(, @('log4net', '1.0.0.0-3.4.0.0', '3.4.0.0'))
        $deployed = Get-VersionProvider -Map @{ 'log4net' = @('3.5.0.0') }

        # Act
        $result = Invoke-BindingRedirectSync -AppConfigText $text -DeployedVersionProvider $deployed

        # Assert
        $result.Text | Should -Match 'oldVersion="1\.0\.0\.0-3\.5\.0\.0"'
        $result.Text | Should -Match 'newVersion="3\.5\.0\.0"'
        @($result.Repair).Count | Should -Be 1
        $result.Repair[0].Kind | Should -Be 'BindingRedirectSync'
        $result.Repair[0].From | Should -Be '3.4.0.0'
        $result.Repair[0].To | Should -Be '3.5.0.0'
        $result.ExaminedCount | Should -Be 1
    }

    It 'replaces a single-version oldVersion outright' {
        # Arrange
        $text = Get-AppConfigText -Redirect @(, @('log4net', '3.4.0.0', '3.4.0.0'))
        $deployed = Get-VersionProvider -Map @{ 'log4net' = @('3.5.0.0') }

        # Act
        $result = Invoke-BindingRedirectSync -AppConfigText $text -DeployedVersionProvider $deployed

        # Assert
        $result.Text | Should -Match 'oldVersion="3\.5\.0\.0"'
        $result.Text | Should -Match 'newVersion="3\.5\.0\.0"'
    }

    It 'leaves a redirect whose newVersion is already deployed unchanged and reports no repair on a second pass' {
        # Arrange
        $current = Get-AppConfigText -Redirect @(, @('log4net', '0.0.0.0-3.5.0.0', '3.5.0.0'))
        $stale = Get-AppConfigText -Redirect @(, @('log4net', '0.0.0.0-3.4.0.0', '3.4.0.0'))
        $deployed = Get-VersionProvider -Map @{ 'log4net' = @('3.5.0.0') }

        # Act
        $unchanged = Invoke-BindingRedirectSync -AppConfigText $current -DeployedVersionProvider $deployed
        $first = Invoke-BindingRedirectSync -AppConfigText $stale -DeployedVersionProvider $deployed
        $second = Invoke-BindingRedirectSync -AppConfigText $first.Text -DeployedVersionProvider $deployed

        # Assert
        $unchanged.Text | Should -BeExactly $current
        @($unchanged.Repair).Count | Should -Be 0
        @($first.Repair).Count | Should -Be 1
        @($second.Repair).Count | Should -Be 0 -Because 'a synchronised redirect names a deployed version'
        $second.Text | Should -BeExactly $first.Text
    }

    It 'reports an assembly with no deployed version as unverifiable and leaves it unchanged' {
        # Arrange
        $text = Get-AppConfigText -Redirect @(, @('netstandard', '0.0.0.0-2.0.0.0', '2.0.0.0'))
        $deployed = Get-VersionProvider -Map @{}

        # Act
        $result = Invoke-BindingRedirectSync -AppConfigText $text -DeployedVersionProvider $deployed

        # Assert
        $result.Text | Should -BeExactly $text
        @($result.Unverifiable) | Should -Be @('netstandard')
        @($result.Repair).Count | Should -Be 0
    }

    It 'prefers the project own reference version when several versions are deployed' {
        # Arrange
        $text = Get-AppConfigText -Redirect @(, @('log4net', '0.0.0.0-3.3.0.0', '3.3.0.0'))
        $deployed = Get-VersionProvider -Map @{ 'log4net' = @('3.4.0.0', '3.5.0.0') }
        $preferred = Get-VersionProvider -Map @{ 'log4net' = @('3.4.0.0') }

        # Act
        $result = Invoke-BindingRedirectSync -AppConfigText $text -DeployedVersionProvider $deployed `
            -PreferredVersionProvider $preferred

        # Assert
        $result.Text | Should -Match 'newVersion="3\.4\.0\.0"'
        $result.Repair[0].To | Should -Be '3.4.0.0'
        $result.Repair[0].Rule | Should -Be 'OwnReference'
    }

    It 'selects the highest deployed version by numeric comparison when no own reference exists' {
        # Arrange: string ordering would select 3.9.0.0; numeric ordering selects 3.10.0.0.
        $text = Get-AppConfigText -Redirect @(, @('log4net', '0.0.0.0-3.8.0.0', '3.8.0.0'))
        $deployed = Get-VersionProvider -Map @{ 'log4net' = @('3.9.0.0', '3.10.0.0') }

        # Act
        $result = Invoke-BindingRedirectSync -AppConfigText $text -DeployedVersionProvider $deployed

        # Assert
        $result.Text | Should -Match 'newVersion="3\.10\.0\.0"'
        $result.Repair[0].To | Should -Be '3.10.0.0'
        $result.Repair[0].Rule | Should -Be 'HighestDeployed'
    }

    It 'falls back to the highest deployed version when the own reference names more than one version' {
        # Arrange
        $text = Get-AppConfigText -Redirect @(, @('log4net', '0.0.0.0-3.3.0.0', '3.3.0.0'))
        $deployed = Get-VersionProvider -Map @{ 'log4net' = @('3.4.0.0', '3.5.0.0') }
        $preferred = Get-VersionProvider -Map @{ 'log4net' = @('3.4.0.0', '3.5.0.0') }

        # Act
        $result = Invoke-BindingRedirectSync -AppConfigText $text -DeployedVersionProvider $deployed `
            -PreferredVersionProvider $preferred

        # Assert
        $result.Repair[0].To | Should -Be '3.5.0.0'
        $result.Repair[0].Rule | Should -Be 'HighestDeployed'
    }

    It 'reports an unparsable deployed version as unresolvable and leaves the redirect unchanged' {
        # Arrange
        $text = Get-AppConfigText -Redirect @(, @('log4net', '0.0.0.0-3.4.0.0', '3.4.0.0'))
        $deployed = Get-VersionProvider -Map @{ 'log4net' = @('3.5.0.0', 'not-a-version') }

        # Act
        $result = Invoke-BindingRedirectSync -AppConfigText $text -DeployedVersionProvider $deployed

        # Assert
        $result.Text | Should -BeExactly $text
        @($result.Unresolvable) | Should -Be @('log4net')
        @($result.Repair).Count | Should -Be 0
    }

    It 'skips a dependentAssembly block that carries no bindingRedirect' {
        # Arrange: one block with a codeBase only, one stale redirect.
        $codeBase = @('<?xml version="1.0" encoding="utf-8"?>', '<configuration>', '  <runtime>',
            '    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">',
            '      <dependentAssembly>',
            '        <assemblyIdentity name="Contoso" publicKeyToken="0123456789abcdef" culture="neutral" />',
            '        <codeBase version="1.0.0.0" href="Contoso.dll" />',
            '      </dependentAssembly>',
            '    </assemblyBinding>', '  </runtime>', '</configuration>') -join "`r`n"
        $deployed = Get-VersionProvider -Map @{ 'Contoso' = @('2.0.0.0') }

        # Act
        $result = Invoke-BindingRedirectSync -AppConfigText $codeBase -DeployedVersionProvider $deployed

        # Assert
        $result.ExaminedCount | Should -Be 0
        $result.Text | Should -BeExactly $codeBase
        @($result.Repair).Count | Should -Be 0
    }

    It 'examines zero entries and changes nothing for empty text' {
        # Arrange
        $deployed = Get-VersionProvider -Map @{ 'log4net' = @('3.5.0.0') }

        # Act
        $result = Invoke-BindingRedirectSync -AppConfigText '' -DeployedVersionProvider $deployed

        # Assert
        $result.ExaminedCount | Should -Be 0
        $result.Text | Should -BeExactly ''
        @($result.Repair).Count | Should -Be 0
    }

    It 'throws when the text is not an application configuration document' {
        # Arrange
        $deployed = Get-VersionProvider -Map @{}

        # Act / Assert
        { Invoke-BindingRedirectSync -AppConfigText '<packages />' -DeployedVersionProvider $deployed } |
            Should -Throw -ExpectedMessage '*not an application configuration document*'
    }

    It 'keeps every byte outside the substituted attribute values identical in CRLF text' {
        # Arrange: a current redirect beside the stale one, so untouched blocks are in the span.
        $text = Get-AppConfigText -Redirect @(
            @('Fabrikam.Core', '0.0.0.0-1.0.0.0', '1.0.0.0'),
            @('log4net', '0.0.0.0-3.4.0.0', '3.4.0.0'))
        $deployed = Get-VersionProvider -Map @{ 'log4net' = @('3.5.0.0'); 'Fabrikam.Core' = @('1.0.0.0') }

        # Act
        $result = Invoke-BindingRedirectSync -AppConfigText $text -DeployedVersionProvider $deployed
        $restored = $result.Text.Replace('oldVersion="0.0.0.0-3.5.0.0"', 'oldVersion="0.0.0.0-3.4.0.0"')
        $restored = $restored.Replace('newVersion="3.5.0.0"', 'newVersion="3.4.0.0"')

        # Assert
        $result.Text | Should -Not -BeExactly $text
        ($restored -ceq $text) | Should -BeTrue -Because 'only the two attribute values may differ'
    }
}

Describe 'Invoke-SolutionBindingRedirectSync (in-memory store)' {

    It 'synchronises the transitive redirect of a project that does not reference the assembly' {
        # Arrange
        $store = Get-SolutionStore
        $delegate = Get-StoreDelegate -Store $store

        # Act
        $result = Invoke-SolutionBindingRedirectSync @delegate

        # Assert
        $store['X:\fixture\Test\app.config'] | Should -Match 'oldVersion="0\.0\.0\.0-3\.5\.0\.0" newVersion="3\.5\.0\.0"'
        @($result.Repair).Count | Should -Be 1
        $result.Repair[0].ProjectDirectory | Should -Be 'Test'
        $result.Repair[0].Path | Should -Be 'X:\fixture\Test\app.config'
        $result.Repair[0].Rule | Should -Be 'HighestDeployed'
        $result.ExaminedAppConfig | Should -Be 2
    }

    It 'prefers the project text override over the text the reader returns' {
        # Arrange
        $store = Get-SolutionStore
        $delegate = Get-StoreDelegate -Store $store
        $override = @{ 'X:\fixture\Prod\Prod.csproj' = (Get-ProjectText -Version '3.6.0.0') }

        # Act
        $result = Invoke-SolutionBindingRedirectSync @delegate -ProjectTextOverride $override

        # Assert
        $store['X:\fixture\Test\app.config'] | Should -Match 'newVersion="3\.6\.0\.0"'
        $store['X:\fixture\Prod\app.config'] | Should -Match 'newVersion="3\.6\.0\.0"'
        @($result.Repair | Where-Object { $_.ProjectDirectory -eq 'Prod' })[0].Rule | Should -Be 'OwnReference'
    }

    It 'writes nothing when run with -WhatIf' {
        # Arrange
        $store = Get-SolutionStore
        $before = $store['X:\fixture\Test\app.config']
        $delegate = Get-StoreDelegate -Store $store

        # Act
        $result = Invoke-SolutionBindingRedirectSync @delegate -WhatIf

        # Assert
        $store['X:\fixture\Test\app.config'] | Should -BeExactly $before
        @($result.ChangedPath).Count | Should -Be 0
    }

    It 'reports exactly the changed application configuration paths' {
        # Arrange
        $store = Get-SolutionStore
        $delegate = Get-StoreDelegate -Store $store

        # Act
        $result = Invoke-SolutionBindingRedirectSync @delegate

        # Assert
        @($result.ChangedPath) | Should -Be @('X:\fixture\Test\app.config')
        @($result.ChangedPath) | Should -Not -Contain 'X:\fixture\Prod\app.config'
        @($result.Unverifiable).Count | Should -Be 0
        @($result.Unresolvable).Count | Should -Be 0
    }
}

Describe 'Format-BindingRedirectSyncReport' {

    It 'returns an empty string when there are no repairs' {
        Format-BindingRedirectSyncReport -Repair @() | Should -BeExactly ''
    }

    It 'returns the heading and one line per repair' {
        # Arrange
        $repair = @(
            [pscustomobject]@{ ProjectDirectory = 'Tags.Test'; AssemblyName = 'log4net'; From = '3.4.0.0'; To = '3.5.0.0'; Rule = 'HighestDeployed' },
            [pscustomobject]@{ ProjectDirectory = 'Prod'; AssemblyName = 'Contoso'; From = '1.0.0.0'; To = '2.0.0.0'; Rule = 'OwnReference' })

        # Act
        $line = (Format-BindingRedirectSyncReport -Repair $repair) -split [regex]::Escape([System.Environment]::NewLine)

        # Assert
        $line.Count | Should -Be 3
        $line[0] | Should -BeExactly '## Binding redirects synchronised'
        $line[1] | Should -BeExactly '- Tags.Test: log4net 3.4.0.0 to 3.5.0.0 (HighestDeployed)'
        $line[2] | Should -BeExactly '- Prod: Contoso 1.0.0.0 to 2.0.0.0 (OwnReference)'
    }
}
