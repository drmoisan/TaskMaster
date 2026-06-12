Set-StrictMode -Version Latest

BeforeAll {
    $script:repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
    $script:mstestScript = Join-Path $script:repoRoot 'scripts\vscode\Invoke-MSTest.ps1'
    $script:coverageScript = Join-Path $script:repoRoot 'scripts\vscode\Invoke-MSTestWithCoverage.ps1'

    # Dot-source the scripts to import their functions into the test scope. The
    # top-level body resolves vswhere/assemblies after the functions are defined,
    # so any body error does not prevent the function definitions from loading.
    # Functions under test (Resolve-RunSettingsPath, Get-VsTestArgumentList,
    # Get-DotnetCoverageArgumentList, and the wrapper seams) are declared before the
    # body, making them available regardless of body execution.
    # The body may throw in environments without vswhere/dotnet-coverage; the
    # function definitions are already loaded by that point, so the body error is
    # intentionally tolerated and recorded for diagnostics rather than rethrown.
    try { . $script:mstestScript -NoExecute } catch { Write-Verbose "Invoke-MSTest body skipped: $_" }
    try { . $script:coverageScript -NoExecute } catch { Write-Verbose "Invoke-MSTestWithCoverage body skipped: $_" }

    $script:expectedRunSettings = Join-Path $script:repoRoot 'TaskMaster.runsettings'
}

Describe 'Resolve-RunSettingsPath' {
    It 'resolves the repo-root TaskMaster.runsettings path when present' {
        $resolved = Resolve-RunSettingsPath -RepoRoot $script:repoRoot

        $resolved | Should -Be $script:expectedRunSettings
    }

    It 'fails fast with a specific error naming the missing path when absent' {
        $missingRoot = Join-Path $script:repoRoot 'does-not-exist-runsettings-root'
        $expectedMissing = Join-Path $missingRoot 'TaskMaster.runsettings'

        { Resolve-RunSettingsPath -RepoRoot $missingRoot } |
            Should -Throw -ExpectedMessage "Runsettings file not found: $expectedMissing"
    }
}

Describe 'Get-VsTestArgumentList (Invoke-MSTest.ps1)' {
    It 'includes /Settings: pointing at the repo-root TaskMaster.runsettings' {
        $arguments = Get-VsTestArgumentList `
            -TestAssembly @('C:\repo\A.Test.dll', 'C:\repo\B.Test.dll') `
            -RunSettingsPath $script:expectedRunSettings

        $arguments | Should -Contain "/Settings:$($script:expectedRunSettings)"
    }

    It 'preserves the test assemblies and /InIsolation alongside /Settings:' {
        $arguments = Get-VsTestArgumentList `
            -TestAssembly @('C:\repo\A.Test.dll') `
            -RunSettingsPath $script:expectedRunSettings

        $arguments | Should -Be @(
            'C:\repo\A.Test.dll',
            "/Settings:$($script:expectedRunSettings)",
            '/InIsolation'
        )
    }
}

Describe 'Invoke-VsTestExe wrapper seam (Invoke-MSTest.ps1)' {
    It 'passes the constructed argument list through the mockable seam' {
        # Mock the wrapper seam only (never the real vstest.console.exe). The mock
        # signature matches production exactly: param([string]$VsTestPath,[string[]]$VsTestArgs).
        $script:capturedVsTestArgs = $null
        $script:capturedVsTestPath = $null
        Mock Invoke-VsTestExe {
            param([string]$VsTestPath, [string[]]$VsTestArgs)
            $script:capturedVsTestPath = $VsTestPath
            $script:capturedVsTestArgs = $VsTestArgs
        }

        $arguments = Get-VsTestArgumentList `
            -TestAssembly @('C:\repo\A.Test.dll') `
            -RunSettingsPath $script:expectedRunSettings

        Invoke-VsTestExe -VsTestPath 'C:\vstest.console.exe' -VsTestArgs $arguments

        Should -Invoke Invoke-VsTestExe -Times 1 -Exactly
        $script:capturedVsTestPath | Should -Be 'C:\vstest.console.exe'
        $script:capturedVsTestArgs | Should -Contain "/Settings:$($script:expectedRunSettings)"
    }
}

Describe 'Get-DotnetCoverageArgumentList (Invoke-MSTestWithCoverage.ps1)' {
    It 'includes the inner vstest /Settings: pointing at the repo-root TaskMaster.runsettings' {
        $arguments = Get-DotnetCoverageArgumentList `
            -OutputPath 'C:\repo\coverage\coverage.cobertura.xml' `
            -CoverageConfig 'C:\repo\coverage.config' `
            -VsTestPath 'C:\vstest.console.exe' `
            -TestAssembly @('C:\repo\A.Test.dll') `
            -RunSettingsPath $script:expectedRunSettings

        $arguments | Should -Contain "/Settings:$($script:expectedRunSettings)"
    }

    It 'preserves the distinct outer --settings coverage.config (instrumentation excludes)' {
        $arguments = Get-DotnetCoverageArgumentList `
            -OutputPath 'C:\repo\coverage\coverage.cobertura.xml' `
            -CoverageConfig 'C:\repo\coverage.config' `
            -VsTestPath 'C:\vstest.console.exe' `
            -TestAssembly @('C:\repo\A.Test.dll') `
            -RunSettingsPath $script:expectedRunSettings

        $settingsIndex = [array]::IndexOf($arguments, '--settings')
        $settingsIndex | Should -BeGreaterThan -1
        $arguments[$settingsIndex + 1] | Should -Be 'C:\repo\coverage.config'
    }

    It 'places the inner /Settings: after the -- separator and the vstest path' {
        $arguments = Get-DotnetCoverageArgumentList `
            -OutputPath 'C:\repo\coverage\coverage.cobertura.xml' `
            -CoverageConfig 'C:\repo\coverage.config' `
            -VsTestPath 'C:\vstest.console.exe' `
            -TestAssembly @('C:\repo\A.Test.dll') `
            -RunSettingsPath $script:expectedRunSettings

        $separatorIndex = [array]::IndexOf($arguments, '--')
        $vsTestSettingsIndex = [array]::IndexOf($arguments, "/Settings:$($script:expectedRunSettings)")

        $separatorIndex | Should -BeGreaterThan -1
        $vsTestSettingsIndex | Should -BeGreaterThan $separatorIndex
    }
}

Describe 'Invoke-DotnetCoverageExe wrapper seam (Invoke-MSTestWithCoverage.ps1)' {
    It 'passes the constructed argument list through the mockable seam' {
        # Mock the wrapper seam only (never the real dotnet-coverage / vstest.console.exe).
        # Mock signature matches production exactly: param([string[]]$DotnetCoverageArgs).
        $script:capturedCoverageArgs = $null
        Mock Invoke-DotnetCoverageExe {
            param([string[]]$DotnetCoverageArgs)
            $script:capturedCoverageArgs = $DotnetCoverageArgs
        }

        $arguments = Get-DotnetCoverageArgumentList `
            -OutputPath 'C:\repo\coverage\coverage.cobertura.xml' `
            -CoverageConfig 'C:\repo\coverage.config' `
            -VsTestPath 'C:\vstest.console.exe' `
            -TestAssembly @('C:\repo\A.Test.dll') `
            -RunSettingsPath $script:expectedRunSettings

        Invoke-DotnetCoverageExe -DotnetCoverageArgs $arguments

        Should -Invoke Invoke-DotnetCoverageExe -Times 1 -Exactly
        $script:capturedCoverageArgs | Should -Contain "/Settings:$($script:expectedRunSettings)"
    }
}
