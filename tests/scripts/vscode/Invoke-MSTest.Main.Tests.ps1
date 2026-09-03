Set-StrictMode -Version Latest

BeforeAll {
    $script:repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
    $script:mstestScript = Join-Path $script:repoRoot 'scripts\vscode\Invoke-MSTest.ps1'
    $script:scriptDir = Join-Path $script:repoRoot 'scripts\vscode'

    # Only Invoke-MSTest.ps1 is imported here. Invoke-MSTestWithCoverage.ps1 defines its own
    # same-named copies of Resolve-RunSettingsPath and of the vswhere seam, so importing both
    # into one session shadows the definitions under test. Dot-sourcing runs no host-bound
    # work: the top-level wiring is guarded by an InvocationName check.
    . $script:mstestScript
}

Describe 'Resolve-RunSettingsPath (Invoke-MSTest.ps1)' {
    It 'returns the off-root CLI runsettings path alongside the script directory' {
        # Positive flow for this file's own copy of the resolver, which is shadowed in
        # tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 by the same-named copy
        # in Invoke-MSTestWithCoverage.ps1 and is therefore only reachable from this file.
        $resolved = Resolve-RunSettingsPath -ScriptRoot $script:scriptDir

        $resolved | Should -Be (Join-Path $script:scriptDir 'TaskMaster.cli.runsettings')
    }

    It 'fails fast with a specific error naming the missing runsettings path' {
        # Negative flow for the guard that this file's own copy of the resolver enforces.
        $missingRoot = Join-Path $script:repoRoot 'does-not-exist-runsettings-root'
        $expectedMissing = Join-Path $missingRoot 'TaskMaster.cli.runsettings'

        { Resolve-RunSettingsPath -ScriptRoot $missingRoot } |
            Should -Throw -ExpectedMessage "Runsettings file not found: $expectedMissing"
    }
}

Describe 'Invoke-VsTestExe splatting seam (Invoke-MSTest.ps1)' {
    It 'forwards every argument array element as a separate positional argument' {
        # The seam is exercised against the in-process Join-Path cmdlet rather than a real
        # vstest.console.exe, so the splatting contract is asserted with no external process
        # and no filesystem access. Join-Path takes two positional arguments, so the returned
        # value proves both elements arrived, in order.
        $result = Invoke-VsTestExe -VsTestPath 'Join-Path' -VsTestArgs @('C:\alpha', 'beta')

        $result | Should -Be 'C:\alpha\beta'
    }
}

Describe 'Invoke-MSTestMain' {
    # Invoke-MSTest.ps1's entry-point body was extracted into Invoke-MSTestMain so the guards,
    # messages, and ordering below are reachable from Pester. Every external dependency is
    # reached through a named seam and is mocked here: no vswhere.exe, no vstest.console.exe,
    # no disk access, and no temporary files.

    BeforeEach {
        $script:capturedVsTestPath = $null
        $script:capturedVsTestArgs = $null
        $script:expectedRunSettings = 'C:\repo\scripts\vscode\TaskMaster.cli.runsettings'

        Mock Resolve-Path { [pscustomobject]@{ Path = 'C:\repo' } }
        Mock Test-Path { $true }
        Mock Resolve-RunSettingsPath { $script:expectedRunSettings }
        Mock Get-VsTestConsolePath { 'C:\repo\vstest.console.exe' }
        Mock Get-MSTestAssemblyPathList { , @('C:\repo\A.Test\bin\Debug\A.Test.dll') }
        Mock Invoke-VsTestExe {
            param([string]$VsTestPath, [string[]]$VsTestArgs)
            $script:capturedVsTestPath = $VsTestPath
            $script:capturedVsTestArgs = $VsTestArgs
            $global:LASTEXITCODE = 0
        }
    }

    It 'fails when the search root cannot be found' {
        Mock Test-Path { $false }

        { Invoke-MSTestMain -NoExecute -ScriptRoot $script:scriptDir } |
            Should -Throw -ExpectedMessage 'Search root not found: C:\repo\.'
    }

    It 'fails when vswhere.exe is not installed' {
        Mock Test-Path { $false } -ParameterFilter { $Path -like '*vswhere.exe' }

        { Invoke-MSTestMain -NoExecute -ScriptRoot $script:scriptDir } |
            Should -Throw -ExpectedMessage 'vswhere.exe was not found. Install Visual Studio 2022 (or Build Tools) with Test Platform components.'
    }

    It 'fails when vswhere resolves no vstest.console.exe' {
        Mock Get-VsTestConsolePath { $null }

        { Invoke-MSTestMain -NoExecute -ScriptRoot $script:scriptDir } |
            Should -Throw -ExpectedMessage 'vstest.console.exe not found via vswhere. Install Visual Studio Test Platform components.'
    }

    It 'fails when discovery finds no test assemblies, naming the search root and configuration' {
        Mock Get-MSTestAssemblyPathList { , @() }

        { Invoke-MSTestMain -SearchRoot 'QuickFiler.Test' -Configuration 'Release' -NoExecute -ScriptRoot $script:scriptDir } |
            Should -Throw -ExpectedMessage "No test assemblies found under 'C:\repo\QuickFiler.Test' for configuration 'Release'. Build first."
    }

    It 'returns before launching vstest.console.exe when NoExecute is supplied' {
        Invoke-MSTestMain -NoExecute -ScriptRoot $script:scriptDir

        Should -Invoke Invoke-VsTestExe -Times 0 -Exactly
        $script:capturedVsTestArgs | Should -BeNullOrEmpty
    }

    It 'launches vstest.console.exe with the discovered assemblies and the resolved runsettings' {
        Invoke-MSTestMain -ScriptRoot $script:scriptDir

        Should -Invoke Invoke-VsTestExe -Times 1 -Exactly
        $script:capturedVsTestPath | Should -Be 'C:\repo\vstest.console.exe'
        $script:capturedVsTestArgs | Should -Be @(
            'C:\repo\A.Test\bin\Debug\A.Test.dll',
            "/Settings:$($script:expectedRunSettings)",
            '/InIsolation',
            '/TestCaseFilter:TestCategory!=LiveOutlook'
        )
    }

    It 'defaults the search root to the repository root and the configuration to Debug' {
        # The two IsNullOrWhiteSpace fallbacks are the only source of the resolved search root
        # in the happy path above; this case pins them by asserting the discovery seam receives
        # the defaulted values rather than empty strings.
        $script:capturedSearchRoot = $null
        $script:capturedConfiguration = $null
        Mock Get-MSTestAssemblyPathList {
            param([string]$SearchRoot, [string]$Configuration)
            $script:capturedSearchRoot = $SearchRoot
            $script:capturedConfiguration = $Configuration
            , @('C:\repo\A.Test\bin\Debug\A.Test.dll')
        }

        Invoke-MSTestMain -NoExecute -ScriptRoot $script:scriptDir

        $script:capturedSearchRoot | Should -Be 'C:\repo\.'
        $script:capturedConfiguration | Should -Be 'Debug'
    }

    It 'throws naming the exit code when vstest.console.exe returns a nonzero status' {
        Mock Invoke-VsTestExe { $global:LASTEXITCODE = 3 }

        { Invoke-MSTestMain -ScriptRoot $script:scriptDir } |
            Should -Throw -ExpectedMessage 'MSTest execution failed with exit code 3'
    }
}
