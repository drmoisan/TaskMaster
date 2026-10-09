Set-StrictMode -Version Latest

BeforeAll {
    $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
    $script:EntryPoint = Join-Path $script:RepoRoot 'scripts/dependencies/Repair-PackageManifestConsistency.ps1'

    # Every fixture is an in-memory string or hashtable and every filesystem dependency is
    # injected, so no temporary file is created and nothing on disk is read except the entry
    # point itself. No block or test name in this file matches the regex AC followed by a digit.
    # The tree models issue 985: a production project references log4net 3.5.0.0, while a test
    # project that neither declares nor references log4net keeps a transitive redirect to
    # 3.4.0.0, which only a solution-wide pass can reach.

    $script:ProdManifest = @'
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="log4net" version="3.5.0" targetFramework="net481" />
</packages>
'@

    $script:ProdProject = @'
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0">
  <ItemGroup>
    <Reference Include="log4net, Version=3.5.0.0, Culture=neutral, PublicKeyToken=669e0ddf0bb1aa2a, processorArchitecture=MSIL">
      <HintPath>..\packages\log4net.3.5.0\lib\net462\log4net.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
'@

    $script:TestManifest = @'
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="Fabrikam.Core" version="1.0.0" targetFramework="net481" />
</packages>
'@

    $script:TestProject = @'
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0">
  <ItemGroup>
    <Reference Include="Fabrikam.Core, Version=1.0.0.0, Culture=neutral, processorArchitecture=MSIL">
      <HintPath>..\packages\Fabrikam.Core.1.0.0\lib\net472\Fabrikam.Core.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
'@

    $script:TestAppConfig = @'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="log4net" publicKeyToken="669e0ddf0bb1aa2a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-3.4.0.0" newVersion="3.4.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Fabrikam.Core" publicKeyToken="abcdef0123456789" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-1.0.0.0" newVersion="1.0.0.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
'@

    # Rendered with the line ending the repository's own files carry, as the sibling suite
    # does, so the normalisation pass leaves the fixture byte-identical.
    $script:ProdManifest = ($script:ProdManifest -replace "`r?`n", "`r`n") + "`r`n"
    $script:TestManifest = ($script:TestManifest -replace "`r?`n", "`r`n") + "`r`n"
    $script:ProdProject = $script:ProdProject -replace "`r?`n", "`r`n"
    $script:TestProject = $script:TestProject -replace "`r?`n", "`r`n"
    $script:TestAppConfig = $script:TestAppConfig -replace "`r?`n", "`r`n"

    $script:AssemblyIdentity = @{
        'log4net|3.5.0'       = @([pscustomobject]@{ AssetFolder = 'net462'; Version = '3.5.0.0' })
        'Fabrikam.Core|1.0.0' = @([pscustomobject]@{ AssetFolder = 'net472'; Version = '1.0.0.0' })
    }

    $script:TestAppConfigPath = 'X:\fixture\Test\app.config'

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

    function Get-TransitiveFixture {
        <#
        .SYNOPSIS
            Builds the two-project store: a production project referencing log4net 3.5.0.0 and a
            test project whose application configuration redirects log4net to 3.4.0.0.
        #>
        return Get-RepairFixture -File @{
            'X:\fixture\Prod\packages.config' = $script:ProdManifest
            'X:\fixture\Prod\Prod.csproj'     = $script:ProdProject
            'X:\fixture\Test\packages.config' = $script:TestManifest
            'X:\fixture\Test\Test.csproj'     = $script:TestProject
            $script:TestAppConfigPath         = $script:TestAppConfig
        } -Identity $script:AssemblyIdentity
    }
}

Describe 'Repair-PackageManifestConsistency redirect synchronisation (issue 985)' {

    Context 'A transitive redirect left stale by an upgrade elsewhere, with no candidate upgrade supplied' {
        BeforeAll {
            $script:Transitive = Get-TransitiveFixture
            $argument = $script:Transitive.Argument
            $script:FirstResult = & $script:EntryPoint @argument
            $script:SecondResult = & $script:EntryPoint @argument
            $script:SyncedConfig = $script:Transitive.Store[$script:TestAppConfigPath]
        }

        It 'rewrites the transitive redirect in both positions' {
            $script:SyncedConfig | Should -Match 'oldVersion="0\.0\.0\.0-3\.5\.0\.0"'
            $script:SyncedConfig | Should -Match 'newVersion="3\.5\.0\.0"'
            $script:SyncedConfig | Should -Match 'newVersion="1\.0\.0\.0"' -Because 'the Fabrikam redirect already names a deployed version'
        }

        It 'reports the rewritten application configuration in the written paths' {
            @($script:FirstResult.WrittenPath) | Should -Contain $script:TestAppConfigPath
        }

        It 'carries the synchronised-redirect block in the body' {
            $script:FirstResult.Body | Should -Match '## Binding redirects synchronised'
            $script:FirstResult.Body | Should -Match 'log4net 3\.4\.0\.0 to 3\.5\.0\.0'
        }

        It 'exposes the synchronisation in the RedirectSync field and not in the per-project repair records' {
            $repair = @($script:FirstResult.RedirectSync.Repair)
            $repair.Count | Should -Be 1
            $repair[0].AssemblyName | Should -Be 'log4net'
            $repair[0].From | Should -Be '3.4.0.0'
            $repair[0].To | Should -Be '3.5.0.0'
            $repair[0].Rule | Should -Be 'HighestDeployed'
            $kind = @($script:FirstResult.Verification | ForEach-Object { @($_.Report.Repair) } |
                    Where-Object { $null -ne $_ } | ForEach-Object { $_.Kind })
            $kind | Should -Not -Contain 'BindingRedirectSync'
        }

        It 'writes nothing on a second run over the repaired store' {
            @($script:SecondResult.WrittenPath).Count | Should -Be 0
        }
    }

    Context 'The same tree run with -WhatIf' {
        It 'leaves the stale redirect unchanged and writes nothing' {
            $fixture = Get-TransitiveFixture
            $argument = $fixture.Argument
            $result = & $script:EntryPoint @argument -WhatIf
            $fixture.Store[$script:TestAppConfigPath] | Should -Match 'newVersion="3\.4\.0\.0"'
            @($result.WrittenPath).Count | Should -Be 0
        }
    }
}
