Set-StrictMode -Version Latest

BeforeAll {
    $script:repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
    $script:mstestScript = Join-Path $script:repoRoot 'scripts\vscode\Invoke-MSTest.ps1'
    $script:coverageScript = Join-Path $script:repoRoot 'scripts\vscode\Invoke-MSTestWithCoverage.ps1'
    $script:scriptDir = Join-Path $script:repoRoot 'scripts\vscode'

    # Import the existing non-coverage script's definitions. Its top-level wiring is guarded
    # by an InvocationName check, so dot-sourcing imports definitions without running the body.
    . $script:mstestScript

    # Parse and dot-source the coverage scriptblock. The production entrypoint checks
    # dot-source invocation, so only definitions are imported for these in-process tests.
    $tokens = $null
    $parseErrors = $null
    $coverageAst = [System.Management.Automation.Language.Parser]::ParseFile(
        $script:coverageScript,
        [ref]$tokens,
        [ref]$parseErrors)
    $parseErrors | Should -BeNullOrEmpty
    . $coverageAst.GetScriptBlock()
    . (Join-Path $script:scriptDir 'Invoke-MSTestWithCoverage.Helpers.ps1')

    $script:expectedRunSettings = Join-Path $script:scriptDir 'TaskMaster.cli.runsettings'
}

Describe 'Resolve-RunSettingsPath' {
    It 'resolves the off-root CLI TaskMaster.cli.runsettings path when present' {
        $resolved = Resolve-RunSettingsPath -ScriptRoot $script:scriptDir

        $resolved | Should -Be $script:expectedRunSettings
    }

    It 'fails fast with a specific error naming the missing path when absent' {
        $missingRoot = Join-Path $script:repoRoot 'does-not-exist-runsettings-root'
        $expectedMissing = Join-Path $missingRoot 'TaskMaster.cli.runsettings'

        { Resolve-RunSettingsPath -ScriptRoot $missingRoot } |
            Should -Throw -ExpectedMessage "Runsettings file not found: $expectedMissing"
    }
}

Describe 'Get-VsTestArgumentList (Invoke-MSTest.ps1)' {
    It 'includes /Settings: pointing at the off-root CLI TaskMaster.cli.runsettings' {
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
            '/InIsolation',
            '/TestCaseFilter:TestCategory!=LiveOutlook'
        )
    }

    It 'appends the /TestCaseFilter excluding the LiveOutlook category' {
        $arguments = Get-VsTestArgumentList `
            -TestAssembly @('C:\repo\A.Test.dll') `
            -RunSettingsPath $script:expectedRunSettings

        $arguments | Should -Contain '/TestCaseFilter:TestCategory!=LiveOutlook'
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
    It 'includes the inner vstest /Settings: pointing at the off-root CLI TaskMaster.cli.runsettings' {
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

    It 'appends the /TestCaseFilter excluding the LiveOutlook category to the inner vstest args' {
        $arguments = Get-DotnetCoverageArgumentList `
            -OutputPath 'C:\repo\coverage\coverage.cobertura.xml' `
            -CoverageConfig 'C:\repo\coverage.config' `
            -VsTestPath 'C:\vstest.console.exe' `
            -TestAssembly @('C:\repo\A.Test.dll') `
            -RunSettingsPath $script:expectedRunSettings

        $arguments | Should -Contain '/TestCaseFilter:TestCategory!=LiveOutlook'
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

Describe 'Invoke-MSTestWithCoverage derived settings' {
    Context 'Derived coverage settings lifecycle' {
        BeforeEach {
            $script:canonicalCoverageConfig = 'C:\repo\coverage.config'
            $script:coverageOutput = 'C:\repo\coverage\coverage.cobertura.xml'
            $script:fakeVsTestPath = 'C:\vstest.console.exe'
            $script:fakeRunSettingsPath = 'C:\repo\scripts\vscode\TaskMaster.cli.runsettings'
            $script:fakeTestAssemblies = @(
                'C:\repo\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll'
                'C:\repo\Tags.Test\bin\Debug\Tags.Test.dll'
                'C:\repo\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll'
                'C:\repo\TaskTree.Test\bin\Debug\TaskTree.Test.dll'
                'C:\repo\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll'
                'C:\repo\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll'
                'C:\repo\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll'
                'C:\repo\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll'
            )
            $script:canonicalCoverageXml = @'
<?xml version="1.0" encoding="utf-8"?>
<Configuration>
  <CodeCoverage>
    <ModulePaths>
      <Exclude>
        <ModulePath>.*Deedle.*</ModulePath>
        <ModulePath>.*FSharp.*</ModulePath>
        <ModulePath>.*Castle\.Core.*</ModulePath>
        <ModulePath>.*FluentAssertions.*</ModulePath>
        <ModulePath>.*Moq.*</ModulePath>
        <ModulePath>.*Microsoft\.Testing.*</ModulePath>
        <ModulePath>.*MSTest.*</ModulePath>
      </Exclude>
    </ModulePaths>
  </CodeCoverage>
</Configuration>
'@
            $script:writtenCoverageSettingsPath = $null
            $script:writtenCoverageSettingsContent = $null
            $script:removedCoverageSettingsPaths = @()
            $script:capturedDerivedCoverageArgs = $null

            Mock -CommandName Get-Content -MockWith {
                $script:canonicalCoverageXml
            }
            Mock -CommandName Set-Content -MockWith {
                $script:writtenCoverageSettingsPath = $LiteralPath
                $script:writtenCoverageSettingsContent = [string]$Value
            }
            Mock -CommandName Remove-Item -MockWith {
                $script:removedCoverageSettingsPaths += $LiteralPath
            }
            Mock -CommandName Invoke-DotnetCoverageExe -MockWith {
                param([string[]]$DotnetCoverageArgs)
                $script:capturedDerivedCoverageArgs = @($DotnetCoverageArgs)
            }
        }

        It 'retains canonical module exclusions and adds the test assembly exclusion exactly once' {
            [xml]$canonicalSettings = $script:canonicalCoverageXml
            $canonicalExclusions = @(
                $canonicalSettings.Configuration.CodeCoverage.ModulePaths.Exclude.ModulePath |
                    ForEach-Object { [string]$_ }
            )

            $derivedContent = ConvertTo-DerivedCoverageSettingsXml `
                -CanonicalSettingsXml $script:canonicalCoverageXml

            [xml]$derivedSettings = $derivedContent
            $derivedExclusions = @(
                $derivedSettings.Configuration.CodeCoverage.ModulePaths.Exclude.ModulePath |
                    ForEach-Object { [string]$_ }
            )
            $testAssemblyExclusions = @(
                $derivedExclusions | Where-Object { $_ -eq '.*\.Test\.dll$' }
            )
            $retainedCanonicalExclusions = @(
                $derivedExclusions | Where-Object { $_ -ne '.*\.Test\.dll$' }
            )

            $retainedCanonicalExclusions | Should -Be $canonicalExclusions
            $testAssemblyExclusions.Count | Should -Be 1
        }

        It 'uses the derived settings path and preserves all eight test assemblies after the vstest boundary' {
            Invoke-DotnetCoverageCollection `
                -OutputPath $script:coverageOutput `
                -CoverageConfig $script:canonicalCoverageConfig `
                -VsTestPath $script:fakeVsTestPath `
                -TestAssembly $script:fakeTestAssemblies `
                -RunSettingsPath $script:fakeRunSettingsPath

            $settingsIndex = [array]::IndexOf($script:capturedDerivedCoverageArgs, '--settings')
            $separatorIndex = [array]::IndexOf($script:capturedDerivedCoverageArgs, '--')
            $assemblyStartIndex = $separatorIndex + 2
            $assemblyEndIndex = $assemblyStartIndex + $script:fakeTestAssemblies.Count - 1
            $forwardedTestAssemblies = @(
                $script:capturedDerivedCoverageArgs[$assemblyStartIndex..$assemblyEndIndex]
            )

            $script:writtenCoverageSettingsPath | Should -Not -BeNullOrEmpty
            $script:writtenCoverageSettingsPath | Should -Not -Be $script:canonicalCoverageConfig
            (Split-Path $script:writtenCoverageSettingsPath -Parent) |
                Should -Be (Split-Path $script:coverageOutput -Parent)
            $settingsIndex | Should -BeGreaterThan -1
            $script:capturedDerivedCoverageArgs[$settingsIndex + 1] |
                Should -Be $script:writtenCoverageSettingsPath
            $script:capturedDerivedCoverageArgs[$separatorIndex + 1] |
                Should -Be $script:fakeVsTestPath
            $forwardedTestAssemblies | Should -Be $script:fakeTestAssemblies
        }

        It 'removes the derived settings after successful collection without writing the canonical file' {
            Invoke-DotnetCoverageCollection `
                -OutputPath $script:coverageOutput `
                -CoverageConfig $script:canonicalCoverageConfig `
                -VsTestPath $script:fakeVsTestPath `
                -TestAssembly $script:fakeTestAssemblies `
                -RunSettingsPath $script:fakeRunSettingsPath

            Should -Invoke -CommandName Get-Content -Times 1 -Exactly -ParameterFilter {
                $LiteralPath -eq $script:canonicalCoverageConfig
            }
            Should -Invoke -CommandName Set-Content -Times 1 -Exactly
            Should -Invoke -CommandName Set-Content -Times 0 -Exactly -ParameterFilter {
                $LiteralPath -eq $script:canonicalCoverageConfig
            }
            Should -Invoke -CommandName Remove-Item -Times 1 -Exactly -ParameterFilter {
                $LiteralPath -eq $script:writtenCoverageSettingsPath
            }
            $script:removedCoverageSettingsPaths |
                Should -Be @($script:writtenCoverageSettingsPath)
        }

        It 'removes the derived settings after failed collection without writing the canonical file' {
            Mock -CommandName Invoke-DotnetCoverageExe -MockWith {
                throw 'Simulated dotnet-coverage collection failure.'
            }

            {
                Invoke-DotnetCoverageCollection `
                    -OutputPath $script:coverageOutput `
                    -CoverageConfig $script:canonicalCoverageConfig `
                    -VsTestPath $script:fakeVsTestPath `
                    -TestAssembly $script:fakeTestAssemblies `
                    -RunSettingsPath $script:fakeRunSettingsPath
            } | Should -Throw -ExpectedMessage 'Simulated dotnet-coverage collection failure.'

            Should -Invoke -CommandName Set-Content -Times 1 -Exactly
            Should -Invoke -CommandName Set-Content -Times 0 -Exactly -ParameterFilter {
                $LiteralPath -eq $script:canonicalCoverageConfig
            }
            Should -Invoke -CommandName Remove-Item -Times 1 -Exactly -ParameterFilter {
                $LiteralPath -eq $script:writtenCoverageSettingsPath
            }
            $script:removedCoverageSettingsPaths |
                Should -Be @($script:writtenCoverageSettingsPath)
        }
    }
}

Describe 'Invoke-MSTestWithCoverage main wrapper seam' {
    It 'exposes a callable main entrypoint for isolated mocked execution' {
        Get-Command -Name Invoke-MSTestWithCoverageMain -ErrorAction SilentlyContinue |
            Should -Not -BeNullOrEmpty
    }

    It 'exposes a callable vswhere wrapper for executable-free tests' {
        Get-Command -Name Invoke-VsWhereExe -ErrorAction SilentlyContinue |
            Should -Not -BeNullOrEmpty
    }
}

Describe 'Invoke-MSTestWithCoverageMain' {
    BeforeEach {
        $script:coverageCallCount = 0
        $script:vsWhereArgs = $null
        $script:canonicalCoverageXml = '<Configuration><CodeCoverage><ModulePaths><Exclude /></ModulePaths></CodeCoverage></Configuration>'

        Mock Resolve-Path {
            [pscustomobject]@{ Path = 'C:\repo' }
        }
        Mock Test-Path { $true }
        Mock Resolve-RunSettingsPath { 'C:\repo\scripts\vscode\TaskMaster.cli.runsettings' }
        Mock Invoke-VsWhereExe {
            param([string]$VsWherePath, [string[]]$VsWhereArgs)
            $null = $VsWherePath
            $script:vsWhereArgs = $VsWhereArgs
            'C:\repo\vstest.console.exe'
        }
        Mock Get-Command { [pscustomobject]@{ Name = 'dotnet-coverage' } }
        Mock Get-ChildItem {
            [pscustomobject]@{ FullName = 'C:\repo\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' }
        }
        Mock Invoke-DotnetCoverageCollection {
            $script:coverageCallCount++
        }
        Mock Get-Content { '<coverage />' }
        Mock ConvertTo-KoverageCoberturaXml { '<coverage line-rate="0.8" />' }
        Mock Set-Content {}
    }

    It 'uses only mocked discovery and builds the vswhere command for the main happy path' {
        Invoke-MSTestWithCoverageMain `
            -SearchRoot 'QuickFiler.Test' `
            -NoExecute `
            -ScriptRoot $script:scriptDir

        $script:vsWhereArgs | Should -Be @(
            '-latest', '-products', '*', '-find', 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'
        )
        Should -Invoke Invoke-DotnetCoverageCollection -Times 0 -Exactly
    }

    It 'does not start coverage collection when NoExecute is supplied' {
        Invoke-MSTestWithCoverageMain -NoExecute -ScriptRoot $script:scriptDir

        $script:coverageCallCount | Should -Be 0
    }

    It 'collects and post-processes coverage on the fully mocked main happy path' {
        Invoke-MSTestWithCoverageMain -ScriptRoot $script:scriptDir

        Should -Invoke Invoke-DotnetCoverageCollection -Times 1 -Exactly
        Should -Invoke ConvertTo-KoverageCoberturaXml -Times 1 -Exactly
        Should -Invoke Set-Content -Times 1 -Exactly
    }

    It 'passes the generated Cobertura result to the threshold evaluator before completing successfully' {
        $script:evaluatedCoberturaXml = $null
        Mock Assert-CoberturaLineCoverageThreshold { param([string]$CoberturaXml) $script:evaluatedCoberturaXml = $CoberturaXml }
        Mock ConvertTo-KoverageCoberturaXml { '<coverage line-rate="0.8" />' }
        Invoke-MSTestWithCoverageMain -ScriptRoot $script:scriptDir
        $script:evaluatedCoberturaXml | Should -Be '<coverage line-rate="0.8" />'
    }

    It 'fails when the search root cannot be found' {
        Mock Test-Path { $false }

        { Invoke-MSTestWithCoverageMain -NoExecute -ScriptRoot $script:scriptDir } |
            Should -Throw -ExpectedMessage 'Search root not found: C:\repo\.'
    }

    It 'excludes assemblies discovered under a .claude worktree segment' {
        # Issue #733 finding 3: agent worktrees under .claude carry their own built
        # copy of every test assembly, so discovery must drop them before collection.
        $script:capturedTestAssembly = $null
        Mock Get-ChildItem {
            @(
                [pscustomobject]@{ FullName = 'C:\repo\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' },
                [pscustomobject]@{ FullName = 'C:\repo\.claude\worktrees\agent-1\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' }
            )
        }
        Mock Invoke-DotnetCoverageCollection {
            param(
                [string]$OutputPath,
                [string]$CoverageConfig,
                [string]$VsTestPath,
                [string[]]$TestAssembly,
                [string]$RunSettingsPath
            )
            $null = $OutputPath, $CoverageConfig, $VsTestPath, $RunSettingsPath
            $script:capturedTestAssembly = $TestAssembly
        }

        Invoke-MSTestWithCoverageMain -ScriptRoot $script:scriptDir

        $script:capturedTestAssembly |
            Should -Be @('C:\repo\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll')
    }
}

Describe 'Invoke-MSTestWithCoverage isolated error paths' {
    It 'fails when coverage settings have no module exclusion node' {
        {
            ConvertTo-DerivedCoverageSettingsXml -CanonicalSettingsXml '<Configuration />'
        } | Should -Throw -ExpectedMessage 'Coverage settings do not contain Configuration/CodeCoverage/ModulePaths/Exclude.'
    }

    It 'fails when coverage settings repeat the test assembly exclusion' {
        $settings = '<Configuration><CodeCoverage><ModulePaths><Exclude><ModulePath>.*\.Test\.dll$</ModulePath><ModulePath>.*\.Test\.dll$</ModulePath></Exclude></ModulePaths></CodeCoverage></Configuration>'

        {
            ConvertTo-DerivedCoverageSettingsXml -CanonicalSettingsXml $settings
        } | Should -Throw -ExpectedMessage 'Coverage settings contain the test-assembly exclusion more than once: .*\.Test\.dll$'
    }

    It 'fails when the derived path equals the canonical coverage path' {
        Mock Get-DerivedCoverageSettingsPath { 'C:\repo\coverage.config' }

        {
            Invoke-DotnetCoverageCollection `
                -OutputPath 'C:\repo\coverage.config' `
                -CoverageConfig 'C:\repo\coverage.config' `
                -VsTestPath 'C:\repo\vstest.console.exe' `
                -TestAssembly @('C:\repo\A.Test.dll') `
                -RunSettingsPath 'C:\repo\TaskMaster.cli.runsettings'
        } | Should -Throw -ExpectedMessage 'Derived coverage settings path must differ from the canonical settings path.'
    }

    It 'fails when dotnet coverage returns a nonzero exit code' {
        Mock Get-Content { '<Configuration><CodeCoverage><ModulePaths><Exclude /></ModulePaths></CodeCoverage></Configuration>' }
        Mock Set-Content {}
        Mock Remove-Item {}
        Mock Invoke-DotnetCoverageExe { $global:LASTEXITCODE = 7 }

        {
            Invoke-DotnetCoverageCollection `
                -OutputPath 'C:\repo\coverage\coverage.cobertura.xml' `
                -CoverageConfig 'C:\repo\coverage.config' `
                -VsTestPath 'C:\repo\vstest.console.exe' `
                -TestAssembly @('C:\repo\A.Test.dll') `
                -RunSettingsPath 'C:\repo\TaskMaster.cli.runsettings'
        } | Should -Throw -ExpectedMessage 'MSTest with coverage failed with exit code 7'
    }
}
