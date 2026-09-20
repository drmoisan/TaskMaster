Set-StrictMode -Version Latest

BeforeAll {
    $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
    $script:ModulePath = Join-Path $script:RepoRoot 'scripts/dependencies/PackageGraph.psm1'
    Import-Module $script:ModulePath -Force

    # Every fixture below is an in-memory string. No temporary file is created anywhere in
    # this suite, and no filesystem path is read except the module itself.
    function ConvertTo-CrLf {
        param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text)
        return ($Text -replace "`r?`n", "`r`n")
    }

    $script:InlineManifest = ConvertTo-CrLf -Text (@'
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="ExCSS" version="4.3.2" targetFramework="net481" />
  <package id="log4net" version="3.4.0" targetFramework="net481" />
</packages>

'@)

    $script:ReflowedManifest = ConvertTo-CrLf -Text (@'
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package
    id="ExCSS"
    version="4.3.2"
    targetFramework="net481"
  />
  <package
    id="log4net"
    version="3.4.0"
    targetFramework="net481"
  />
</packages>

'@)

    $script:ReflowedAppConfig = ConvertTo-CrLf -Text (@'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <configSections>
    <sectionGroup
      name="userSettings"
      type="System.Configuration.UserSettingsGroup"
    >
      <section
        name="Sample.Properties.Settings"
        requirePermission="false"
      />
    </sectionGroup>
  </configSections>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity
          name="System.Memory"
          publicKeyToken="cc7b13ffcd2ddd51"
          culture="neutral"
        />
        <bindingRedirect oldVersion="0.0.0.0-4.0.2.0" newVersion="4.0.2.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>

'@)

    $script:ProjectFile = ConvertTo-CrLf -Text (@'
<Project ToolsVersion="15.0">
  <Import Project="..\packages\Sample.1.0.0\build\Sample.props" Condition="Exists('a')" />
  <Target Name="EnsureNuGetPackageBuildImports">
    <Error Condition="!Exists('..\packages\Sample.1.0.0\build\Sample.props')" Text="Missing package." />
  </Target>
  <ItemGroup>
    <Reference Include="Sample, Version=1.0.0.0">
      <HintPath>..\packages\Sample.1.0.0\lib\net481\Sample.dll</HintPath>
    </Reference>
    <Analyzer Include="..\packages\Meziantou.Analyzer.3.0.235\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll" />
  </ItemGroup>
</Project>

'@)
}

Describe 'PackageGraph manifest parsing' {

    It 'parses a reflowed entry and an inline entry into identical records' {
        # Arrange: the same two packages, one document inline and one reflowed.
        # Act
        $inline = @(ConvertFrom-PackagesConfigText -Text $script:InlineManifest)
        $reflowed = @(ConvertFrom-PackagesConfigText -Text $script:ReflowedManifest)

        # Assert: the wrapping is invisible to the parser.
        $reflowed.Count | Should -Be $inline.Count
        for ($i = 0; $i -lt $inline.Count; $i++) {
            $reflowed[$i].Id | Should -BeExactly $inline[$i].Id
            $reflowed[$i].Version | Should -BeExactly $inline[$i].Version
            $reflowed[$i].TargetFramework | Should -BeExactly $inline[$i].TargetFramework
            $reflowed[$i].Index | Should -Be $inline[$i].Index
        }
    }

    It 'records the declared attributes in document order' {
        # Arrange / Act
        $records = @(ConvertFrom-PackagesConfigText -Text $script:InlineManifest)

        # Assert
        $records.Count | Should -Be 2
        $records[0].Id | Should -BeExactly 'ExCSS'
        $records[0].Version | Should -BeExactly '4.3.2'
        $records[0].TargetFramework | Should -BeExactly 'net481'
        @($records[0].Attribute.Keys) | Should -Be @('id', 'version', 'targetFramework')
        $records[1].Index | Should -Be 1
    }

    It 'reports an empty target framework when the attribute is absent' {
        # Arrange
        $text = '<packages><package id="Sample" version="1.0.0" /></packages>'

        # Act
        $records = @(ConvertFrom-PackagesConfigText -Text $text)

        # Assert
        $records[0].TargetFramework | Should -BeExactly ''
    }

    It 'rejects text that carries no packages root element' {
        # Arrange
        $text = '<configuration></configuration>'

        # Act / Assert
        { ConvertFrom-PackagesConfigText -Text $text } |
            Should -Throw -ExpectedMessage '*not a packages.config document*'
    }

    It 'rejects a package element that declares no id attribute' {
        # Arrange
        $text = '<packages><package version="1.0.0" targetFramework="net481" /></packages>'

        # Act / Assert
        { ConvertFrom-PackagesConfigText -Text $text } |
            Should -Throw -ExpectedMessage '*declares no id attribute*'
    }

    It 'rejects a package element that declares no version attribute' {
        # Arrange
        $text = '<packages><package id="Sample" targetFramework="net481" /></packages>'

        # Act / Assert
        { ConvertFrom-PackagesConfigText -Text $text } |
            Should -Throw -ExpectedMessage '*declares no version attribute*'
    }
}

Describe 'PackageGraph manifest rendering' {

    It 'renders a parsed manifest in canonical inline form' {
        # Arrange
        $records = @(ConvertFrom-PackagesConfigText -Text $script:ReflowedManifest)

        # Act
        $rendered = ConvertTo-PackagesConfigText -Package $records

        # Assert: the reflowed document renders to the inline document, byte for byte.
        $rendered | Should -BeExactly $script:InlineManifest
    }

    It 'is byte-identical when applied a second time to its own output' {
        # Arrange
        $first = ConvertTo-PackagesConfigText -Package @(ConvertFrom-PackagesConfigText -Text $script:ReflowedManifest)

        # Act
        $second = ConvertTo-PackagesConfigText -Package @(ConvertFrom-PackagesConfigText -Text $first)

        # Assert
        $second | Should -BeExactly $first
    }

    It 'honours a caller-supplied indent' {
        # Arrange
        $records = @(ConvertFrom-PackagesConfigText -Text $script:InlineManifest)

        # Act
        $rendered = ConvertTo-PackagesConfigText -Package $records -Indent '    '

        # Assert
        $rendered | Should -Match '\n    <package id="ExCSS"'
    }

    It 'renders an empty document when no package records are supplied' {
        # Arrange / Act
        $rendered = ConvertTo-PackagesConfigText -Package @()

        # Assert
        $rendered | Should -BeExactly "<?xml version=`"1.0`" encoding=`"utf-8`"?>`r`n<packages>`r`n</packages>`r`n"
    }
}

Describe 'PackageGraph project-file parsing' {

    It 'parses an Import element and records its project path' {
        # Arrange / Act
        $records = @(ConvertFrom-ProjectFileText -Text $script:ProjectFile)
        $import = @($records | Where-Object { $_.Kind -eq 'Import' })

        # Assert
        $import.Count | Should -Be 1
        $import[0].Value | Should -Match 'Sample\.props$'
        $import[0].LineNumber | Should -Be 2
    }

    It 'parses an Error element and records its text' {
        # Arrange / Act
        $records = @(ConvertFrom-ProjectFileText -Text $script:ProjectFile)
        $errors = @($records | Where-Object { $_.Kind -eq 'Error' })

        # Assert
        $errors.Count | Should -Be 1
        $errors[0].Value | Should -BeExactly 'Missing package.'
    }

    It 'parses a Reference element and records its include specification' {
        # Arrange / Act
        $records = @(ConvertFrom-ProjectFileText -Text $script:ProjectFile)
        $references = @($records | Where-Object { $_.Kind -eq 'Reference' })

        # Assert
        $references.Count | Should -Be 1
        $references[0].Value | Should -BeExactly 'Sample, Version=1.0.0.0'
    }

    It 'parses a HintPath element and records its path' {
        # Arrange / Act
        $records = @(ConvertFrom-ProjectFileText -Text $script:ProjectFile)
        $hintPaths = @($records | Where-Object { $_.Kind -eq 'HintPath' })

        # Assert
        $hintPaths.Count | Should -Be 1
        $hintPaths[0].Value | Should -Match 'Sample\.dll$'
        @($hintPaths[0].Attribute.Keys).Count | Should -Be 0
    }

    It 'parses an Analyzer element and records its include specification' {
        # Arrange / Act
        $records = @(ConvertFrom-ProjectFileText -Text $script:ProjectFile)
        $analyzers = @($records | Where-Object { $_.Kind -eq 'Analyzer' })

        # Assert
        $analyzers.Count | Should -Be 1
        $analyzers[0].Value | Should -Match 'Meziantou\.Analyzer\.3\.0\.235'
    }

    It 'records an empty value for a dependent element whose primary attribute is absent' {
        # Arrange: all four attribute-bearing kinds, each stripped of its primary attribute.
        $text = "<Import Condition=`"x`" />`r`n<Error Condition=`"y`" />`r`n<Analyzer Exclude=`"z`" />`r`n<Reference Private=`"true`" />"

        # Act
        $records = @(ConvertFrom-ProjectFileText -Text $text)

        # Assert
        $records.Count | Should -Be 4
        @($records | Where-Object { $_.Value -eq '' }).Count | Should -Be 4
    }

    It 'rejects whitespace-only project-file text' {
        # Arrange / Act / Assert
        { ConvertFrom-ProjectFileText -Text "   `r`n  " } |
            Should -Throw -ExpectedMessage '*project-file text is empty*'
    }
}

Describe 'PackageGraph application-configuration parsing' {

    It 'parses a binding redirect into an identity and a redirect range' {
        # Arrange / Act
        $records = @(ConvertFrom-AppConfigText -Text $script:ReflowedAppConfig)

        # Assert
        $records.Count | Should -Be 1
        $records[0].Name | Should -BeExactly 'System.Memory'
        $records[0].PublicKeyToken | Should -BeExactly 'cc7b13ffcd2ddd51'
        $records[0].Culture | Should -BeExactly 'neutral'
        $records[0].OldVersion | Should -BeExactly '0.0.0.0-4.0.2.0'
        $records[0].NewVersion | Should -BeExactly '4.0.2.0'
    }

    It 'reports empty redirect bounds for a dependent assembly carrying no bindingRedirect' {
        # Arrange
        $text = '<configuration><dependentAssembly><assemblyIdentity name="Only" /></dependentAssembly></configuration>'

        # Act
        $records = @(ConvertFrom-AppConfigText -Text $text)

        # Assert
        $records[0].Name | Should -BeExactly 'Only'
        $records[0].PublicKeyToken | Should -BeExactly ''
        $records[0].Culture | Should -BeExactly ''
        $records[0].OldVersion | Should -BeExactly ''
        $records[0].NewVersion | Should -BeExactly ''
    }

    It 'rejects text that carries no configuration root element' {
        # Arrange / Act / Assert
        { ConvertFrom-AppConfigText -Text '<packages></packages>' } |
            Should -Throw -ExpectedMessage '*not an application configuration document*'
    }

    It 'rejects a dependent assembly that declares no assembly identity' {
        # Arrange
        $text = '<configuration><dependentAssembly><bindingRedirect oldVersion="0.0.0.0-1.0.0.0" newVersion="1.0.0.0" /></dependentAssembly></configuration>'

        # Act / Assert
        { ConvertFrom-AppConfigText -Text $text } |
            Should -Throw -ExpectedMessage '*declares no assemblyIdentity element*'
    }
}

Describe 'PackageGraph application-configuration rendering' {

    It 'collapses a reflowed self-closing start tag onto one line' {
        # Arrange / Act
        $rendered = ConvertTo-AppConfigText -Text $script:ReflowedAppConfig

        # Assert
        $rendered | Should -Match '<assemblyIdentity name="System\.Memory" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />'
        @(($rendered -split "`r`n") | Where-Object { $_.Trim() -eq '<assemblyIdentity' }).Count | Should -Be 0
    }

    It 'collapses a reflowed container start tag without leaving a space before the terminator' {
        # Arrange / Act
        $rendered = ConvertTo-AppConfigText -Text $script:ReflowedAppConfig

        # Assert
        $rendered | Should -Match '<sectionGroup name="userSettings" type="System\.Configuration\.UserSettingsGroup">'
        $rendered | Should -Not -Match 'UserSettingsGroup" >'
    }

    It 'leaves a document that carries no reflowed start tag byte-identical' {
        # Arrange
        $text = ConvertTo-AppConfigText -Text $script:ReflowedAppConfig

        # Act
        $second = ConvertTo-AppConfigText -Text $text

        # Assert
        $second | Should -BeExactly $text
    }

    It 'rejects a start tag that is never terminated' {
        # Arrange
        $text = "<configuration>`r`n  <assemblyIdentity`r`n    name=`"Unclosed`""

        # Act / Assert
        { ConvertTo-AppConfigText -Text $text } |
            Should -Throw -ExpectedMessage '*is never terminated*'
    }
}

Describe 'PackageGraph manifest discovery' {

    It 'selects manifests by leaf name and discards restore and build output paths' {
        # Arrange
        $candidates = @(
            'UtilitiesCS/packages.config',
            'UtilitiesCS/app.config',
            'packages/Sample.1.0.0/packages.config',
            'QuickFiler/bin/Debug/packages.config',
            'QuickFiler/obj/packages.config',
            'tools/node_modules/packages.config',
            'UtilitiesCS/UtilitiesCS.csproj',
            $null
        )
        $lister = { $candidates }

        # Act
        $selected = @(Get-PackageManifestPath -Kind 'PackagesConfig' -DirectoryLister $lister)

        # Assert
        $selected | Should -Be @('UtilitiesCS/packages.config')
    }

    It 'selects application configuration files when that kind is requested' {
        # Arrange
        $candidates = @('Tags/app.config', 'Tags/packages.config')
        $lister = { $candidates }

        # Act
        $selected = @(Get-PackageManifestPath -Kind 'AppConfig' -DirectoryLister $lister)

        # Assert
        $selected | Should -Be @('Tags/app.config')
    }

    It 'accepts either path separator and returns the path in its original form' {
        # Arrange: a Windows-separated candidate under a build output directory, and one not.
        $separator = [string][char]92
        $kept = 'Tags' + $separator + 'packages.config'
        $discarded = 'Tags' + $separator + 'obj' + $separator + 'packages.config'
        $lister = { @($kept, $discarded) }

        # Act
        $selected = @(Get-PackageManifestPath -Kind 'PackagesConfig' -DirectoryLister $lister)

        # Assert
        $selected | Should -Be @($kept)
    }

    It 'returns the selected paths in sorted order' {
        # Arrange
        $lister = { @('Zeta/packages.config', 'Alpha/packages.config') }

        # Act
        $selected = @(Get-PackageManifestPath -Kind 'PackagesConfig' -DirectoryLister $lister)

        # Assert
        $selected | Should -Be @('Alpha/packages.config', 'Zeta/packages.config')
    }
}

Describe 'PackageGraph normalisation over an injected tree' {

    It 'reports the examined count per kind and rewrites only the files whose form differs' {
        # Arrange: four candidates, one already canonical and one under a build output directory.
        $files = [ordered]@{
            'Alpha/packages.config'     = $script:ReflowedManifest
            'Beta/packages.config'      = $script:InlineManifest
            'Gamma/app.config'          = $script:ReflowedAppConfig
            'Delta/bin/packages.config' = $script:ReflowedManifest
        }
        $written = @{}
        $lister = { @($files.Keys) }
        $reader = { param($Path) $files[$Path] }
        $writer = { param($Path, $Text) $written[$Path] = $Text }

        # Act
        $summary = Invoke-ManifestNormalization -DirectoryLister $lister -TextReader $reader -TextWriter $writer

        # Assert: Beta is examined and left alone; Delta is never examined at all.
        $summary.ExaminedPackagesConfig | Should -Be 2
        $summary.ExaminedAppConfig | Should -Be 1
        $summary.ExaminedTotal | Should -Be 3
        $summary.ChangedPackagesConfig | Should -Be 1
        $summary.ChangedAppConfig | Should -Be 1
        @($summary.ChangedPath) | Should -Be @('Alpha/packages.config', 'Gamma/app.config')
        @($written.Keys | Sort-Object) | Should -Be @('Alpha/packages.config', 'Gamma/app.config')
        $written['Alpha/packages.config'] | Should -BeExactly $script:InlineManifest
    }

    It 'writes nothing when every examined file is already canonical' {
        # Arrange
        $files = [ordered]@{ 'Beta/packages.config' = $script:InlineManifest }
        $written = @{}
        $lister = { @($files.Keys) }
        $reader = { param($Path) $files[$Path] }
        $writer = { param($Path, $Text) $written[$Path] = $Text }

        # Act
        $summary = Invoke-ManifestNormalization -DirectoryLister $lister -TextReader $reader -TextWriter $writer

        # Assert: examined is positive while changed is zero, which is the distinction the
        # one-time normalisation depends on.
        $summary.ExaminedPackagesConfig | Should -Be 1
        $summary.ChangedPackagesConfig | Should -Be 0
        $written.Count | Should -Be 0
        @($summary.ExaminedPath) | Should -Be @('Beta/packages.config')
    }

    It 'makes no write when the caller suppresses the action' {
        # Arrange
        $files = [ordered]@{ 'Alpha/packages.config' = $script:ReflowedManifest }
        $written = @{}
        $lister = { @($files.Keys) }
        $reader = { param($Path) $files[$Path] }
        $writer = { param($Path, $Text) $written[$Path] = $Text }

        # Act
        $summary = Invoke-ManifestNormalization -DirectoryLister $lister -TextReader $reader -TextWriter $writer -WhatIf

        # Assert
        $summary.ExaminedPackagesConfig | Should -Be 1
        $summary.ChangedPackagesConfig | Should -Be 0
        $written.Count | Should -Be 0
    }
}
