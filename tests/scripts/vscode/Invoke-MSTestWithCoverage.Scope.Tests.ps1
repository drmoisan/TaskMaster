Set-StrictMode -Version Latest

BeforeAll {
    $script:repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
    $script:scriptDir = Join-Path $script:repoRoot 'scripts\vscode'
    $script:coverageScript = Join-Path $script:scriptDir 'Invoke-MSTestWithCoverage.ps1'
    $script:scopeScript = Join-Path $script:scriptDir 'Invoke-MSTestWithCoverage.Scope.ps1'

    # The production entry point guards its top-level wiring with an invocation-name check, so
    # parsing it and dot-sourcing the resulting scriptblock imports definitions without running the
    # body. The parsed tree is kept because the comment-based help test below reads it directly.
    $tokens = $null
    $parseErrors = $null
    $script:coverageAst = [System.Management.Automation.Language.Parser]::ParseFile(
        $script:coverageScript,
        [ref]$tokens,
        [ref]$parseErrors)
    $parseErrors | Should -BeNullOrEmpty
    . $script:coverageAst.GetScriptBlock()
    . (Join-Path $script:scriptDir 'Invoke-MSTestWithCoverage.Helpers.ps1')

    # The part file is dot-sourced through an existence guard so that, before the part file is
    # created, each predicate case fails on its own with an unrecognised-command error instead of
    # the whole file reporting a single block-setup error that would hide the fail-before signal.
    if (Test-Path -LiteralPath $script:scopeScript) {
        . $script:scopeScript
    }

    # The neutral repository root the entry-point mocks resolve to, shared by the predicate cases.
    $script:fixtureRoot = 'C:\repo'

    # Post-processed documents the post-processor mock answers with. Each carries root
    # lines-covered and lines-valid attributes that equal the per-package line figures, so the
    # reconciliation assertion the entry point runs holds exactly. The first is below both floors;
    # the second is at the line floor and below the branch floor.
    $script:belowBothFloorsXml = '<coverage line-rate="0.4" branch-rate="0.5" lines-covered="2" lines-valid="5" branches-valid="10"><packages><package name="Alpha.Core"><classes><class name="Alpha.Core.Widget" filename="Alpha.Core\Widget.cs"><lines><line number="10" hits="1" /><line number="11" hits="2" /><line number="12" hits="0" /><line number="13" hits="0" /><line number="14" hits="0" /></lines></class></classes></package></packages></coverage>'
    $script:branchBelowFloorXml = '<coverage line-rate="0.8" branch-rate="0.5" lines-covered="4" lines-valid="5" branches-valid="10"><packages><package name="Alpha.Core"><classes><class name="Alpha.Core.Widget" filename="Alpha.Core\Widget.cs"><lines><line number="10" hits="1" /><line number="11" hits="2" /><line number="12" hits="3" /><line number="13" hits="4" /><line number="14" hits="0" /></lines></class></classes></package></packages></coverage>'

    # The test-result document the filtered reader mock answers with, and the path that mock is
    # filtered to. Both are in-memory values; no test in this file creates, writes or deletes a file.
    $script:coverageTrxPath = 'C:\repo\coverage\test-results\mstest-coverage-run.trx'
    $script:coverageTrxFixture = @'
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Results><UnitTestResult testName="Contoso.Alpha.PassesCleanly" outcome="Passed" /></Results>
  <ResultSummary outcome="Completed"><Counters total="1" executed="1" passed="1" failed="0" /></ResultSummary>
</TestRun>
'@

    # The canonical coverage settings the real collection wrapper reads and derives from.
    $script:canonicalSettingsXml = '<Configuration><CodeCoverage><ModulePaths><Exclude /></ModulePaths></CodeCoverage></Configuration>'

    function Get-CoverageEntryPointAst {
        <#
            .SYNOPSIS
            Returns the parsed definition of the coverage entry point.

            .DESCRIPTION
            A test-only reader so each abstract-syntax-tree assertion names what it is looking for
            rather than repeating the search for the enclosing function definition. The tree is the
            one parsed once in this block, so no test re-reads the file from disk.
        #>
        [CmdletBinding()]
        [OutputType([System.Management.Automation.Language.FunctionDefinitionAst])]
        param()

        return $script:coverageAst.Find(
            {
                $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $args[0].Name -eq 'Invoke-MSTestWithCoverageMain'
            },
            $true)
    }
}

Describe 'Test-CoverageRunIsScoped' {
    It 'returns false when the resolved search root equals the repository root' {
        Test-CoverageRunIsScoped -RepoRoot $script:fixtureRoot -ResolvedSearchRoot $script:fixtureRoot |
            Should -BeFalse
    }

    It 'returns false for a dot search root beneath the repository root' {
        # The default search root joined to the repository root.
        $searchRoot = $script:fixtureRoot + '\.'

        Test-CoverageRunIsScoped -RepoRoot $script:fixtureRoot -ResolvedSearchRoot $searchRoot |
            Should -BeFalse
    }

    It 'returns false for a dot-backslash search root beneath the repository root' {
        $searchRoot = $script:fixtureRoot + '\.\'

        Test-CoverageRunIsScoped -RepoRoot $script:fixtureRoot -ResolvedSearchRoot $searchRoot |
            Should -BeFalse
    }

    It 'returns false when only a trailing separator differs' {
        $searchRoot = $script:fixtureRoot + '\'

        Test-CoverageRunIsScoped -RepoRoot $script:fixtureRoot -ResolvedSearchRoot $searchRoot |
            Should -BeFalse
    }

    It 'returns false when only letter case differs' {
        $searchRoot = $script:fixtureRoot.ToUpperInvariant()

        Test-CoverageRunIsScoped -RepoRoot $script:fixtureRoot -ResolvedSearchRoot $searchRoot |
            Should -BeFalse
    }

    It 'returns true for a subdirectory search root' {
        $searchRoot = $script:fixtureRoot + '\QuickFiler.Test'

        Test-CoverageRunIsScoped -RepoRoot $script:fixtureRoot -ResolvedSearchRoot $searchRoot |
            Should -BeTrue
    }

    It 'returns true for a sibling directory whose name extends the repository root name' {
        # A prefix comparison would wrongly report this sibling as the repository root.
        $searchRoot = $script:fixtureRoot + 'sitory'

        Test-CoverageRunIsScoped -RepoRoot $script:fixtureRoot -ResolvedSearchRoot $searchRoot |
            Should -BeTrue
    }

    It 'returns true for the parent directory of the repository root' {
        $searchRoot = [IO.Path]::GetPathRoot($script:fixtureRoot)

        Test-CoverageRunIsScoped -RepoRoot $script:fixtureRoot -ResolvedSearchRoot $searchRoot |
            Should -BeTrue
    }

    It 'throws when the repository root is an empty string' {
        # CR-2 control. An empty repository root is rejected by parameter binding and validation.
        { Test-CoverageRunIsScoped -RepoRoot '' -ResolvedSearchRoot $script:fixtureRoot } |
            Should -Throw -ExpectedMessage "*parameter 'RepoRoot'*"
    }

    It 'throws when the resolved search root is an empty string' {
        # CR-2 control. An empty search root is rejected by parameter binding and validation.
        { Test-CoverageRunIsScoped -RepoRoot $script:fixtureRoot -ResolvedSearchRoot '' } |
            Should -Throw -ExpectedMessage "*parameter 'ResolvedSearchRoot'*"
    }

    It 'throws when the repository root is a relative path' {
        # CR-2. A relative input would resolve against the process working directory, so it fails fast.
        { Test-CoverageRunIsScoped -RepoRoot 'repo' -ResolvedSearchRoot $script:fixtureRoot } |
            Should -Throw -ExpectedMessage 'RepoRoot must be an absolute path: repo'
    }

    It 'throws when the resolved search root is a relative path' {
        # CR-2. A relative search root fails fast for the same reason.
        { Test-CoverageRunIsScoped -RepoRoot $script:fixtureRoot -ResolvedSearchRoot 'QuickFiler.Test' } |
            Should -Throw -ExpectedMessage 'ResolvedSearchRoot must be an absolute path: QuickFiler.Test'
    }
}

Describe 'Assert-CoberturaCoverageThresholdForRun' {
    BeforeEach {
        # The warning stream is the scoped arm's only side effect, so it is the only mock.
        Mock Write-Warning {}
    }

    It 'does not throw on a scoped run whose document is below both floors' {
        # AC1. The scoped arm returns before either floor is asserted.
        {
            Assert-CoberturaCoverageThresholdForRun `
                -CoberturaXml $script:belowBothFloorsXml `
                -RepoRoot $script:fixtureRoot `
                -ResolvedSearchRoot ($script:fixtureRoot + '\QuickFiler.Test')
        } | Should -Not -Throw
    }

    It 'writes exactly one warning naming the scoped search root' {
        # AC1. The skip is announced once, and the announcement names the search root.
        Assert-CoberturaCoverageThresholdForRun `
            -CoberturaXml $script:belowBothFloorsXml `
            -RepoRoot $script:fixtureRoot `
            -ResolvedSearchRoot ($script:fixtureRoot + '\QuickFiler.Test')

        Should -Invoke Write-Warning -Times 1 -Exactly
        Should -Invoke Write-Warning -Times 1 -Exactly -ParameterFilter {
            $Message -like 'Coverage threshold assertions skipped*' -and $Message -like '*QuickFiler.Test*'
        }
    }

    It 'throws the line threshold message on an unscoped run below the line floor' {
        # AC3. The unscoped arm keeps enforcing the line floor.
        {
            Assert-CoberturaCoverageThresholdForRun `
                -CoberturaXml $script:belowBothFloorsXml `
                -RepoRoot $script:fixtureRoot `
                -ResolvedSearchRoot $script:fixtureRoot
        } | Should -Throw -ExpectedMessage 'Cobertura line coverage 40% is below the required 80% threshold.'
    }

    It 'throws the branch threshold message on an unscoped run at the line floor and below the branch floor' {
        # AC3. The unscoped arm keeps enforcing the branch floor once the line floor is met.
        {
            Assert-CoberturaCoverageThresholdForRun `
                -CoberturaXml $script:branchBelowFloorXml `
                -RepoRoot $script:fixtureRoot `
                -ResolvedSearchRoot $script:fixtureRoot
        } | Should -Throw -ExpectedMessage 'Cobertura branch coverage 50% is below the required 75% threshold.'
    }
}

Describe 'Invoke-MSTestWithCoverageMain threshold gating by search root' {
    BeforeEach {
        # Every filesystem command the entry point reaches is mocked, and the executable seam is
        # mocked beneath the real collection wrapper, so the real threshold assertions and the real
        # wrapper both run while no test creates, writes or deletes a file.
        Mock Resolve-Path { [pscustomobject]@{ Path = 'C:\repo' } }
        Mock Test-Path { $true }
        Mock Resolve-RunSettingsPath { 'C:\repo\scripts\vscode\TaskMaster.cli.runsettings' }
        Mock Invoke-VsWhereExe {
            param([string]$VsWherePath, [string[]]$VsWhereArgs)
            $null = $VsWherePath, $VsWhereArgs
            'C:\repo\vstest.console.exe'
        }
        Mock Get-Command { [pscustomobject]@{ Name = 'dotnet-coverage' } }
        Mock Get-ChildItem {
            [pscustomobject]@{ FullName = 'C:\repo\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' }
        }
        Mock Invoke-DotnetCoverageExe {
            param([string[]]$DotnetCoverageArgs)
            $null = $DotnetCoverageArgs
            $global:LASTEXITCODE = 0
        }
        Mock Get-Content { '<coverage />' }
        Mock Get-Content -ParameterFilter { $LiteralPath -like '*coverage.config' } -MockWith { $script:canonicalSettingsXml }
        Mock Get-Content -ParameterFilter { $LiteralPath -eq $script:coverageTrxPath } -MockWith { $script:coverageTrxFixture }
        Mock ConvertTo-KoverageCoberturaXml { $script:belowBothFloorsXml }
        Mock Set-Content {}
        Mock Remove-Item {}
        Mock Write-Warning {}
    }

    Context 'scoped run' {
        It 'completes without error on a scoped run whose post-processed document is below both floors' {
            # AC1. A single-assembly run is judged on its tests, not on solution-wide floors.
            {
                $null = Invoke-MSTestWithCoverageMain -SearchRoot 'QuickFiler.Test' -ScriptRoot $script:scriptDir
            } | Should -Not -Throw

            # Derived settings, post-processed document, projection, test-result summary.
            Should -Invoke Set-Content -Times 4 -Exactly
        }

        It 'writes exactly one warning naming the skipped assertions and the scoped search root' {
            # AC1. The skip is announced once, and the announcement names the search root.
            $null = Invoke-MSTestWithCoverageMain -SearchRoot 'QuickFiler.Test' -ScriptRoot $script:scriptDir

            Should -Invoke Write-Warning -Times 1 -Exactly
            Should -Invoke Write-Warning -Times 1 -Exactly -ParameterFilter {
                $Message -like 'Coverage threshold assertions skipped*' -and $Message -like '*QuickFiler.Test*'
            }
        }

        It 'still terminates with an error when collection returns a non-zero exit code on a scoped run' {
            # AC2. The collector exit code flows through the real wrapper and still fails the run.
            Mock Invoke-DotnetCoverageExe {
                param([string[]]$DotnetCoverageArgs)
                $null = $DotnetCoverageArgs
                $global:LASTEXITCODE = 7
            }

            {
                $null = Invoke-MSTestWithCoverageMain -SearchRoot 'QuickFiler.Test' -ScriptRoot $script:scriptDir
            } | Should -Throw -ExpectedMessage 'MSTest with coverage failed with exit code 7'

            Should -Invoke ConvertTo-KoverageCoberturaXml -Times 0 -Exactly
        }
    }

    Context 'unscoped run' {
        It 'throws the line threshold message when the search root is omitted and the line rate is below 80 percent' {
            # AC3. The unscoped run keeps enforcing the line floor.
            {
                $null = Invoke-MSTestWithCoverageMain -ScriptRoot $script:scriptDir
            } | Should -Throw -ExpectedMessage 'Cobertura line coverage 40% is below the required 80% threshold.'
        }

        It 'throws the branch threshold message for a dot search root when the branch rate is below 75 percent' {
            # AC3. The unscoped run keeps enforcing the branch floor.
            Mock ConvertTo-KoverageCoberturaXml { $script:branchBelowFloorXml }

            {
                $null = Invoke-MSTestWithCoverageMain -SearchRoot '.' -ScriptRoot $script:scriptDir
            } | Should -Throw -ExpectedMessage 'Cobertura branch coverage 50% is below the required 75% threshold.'
        }
    }
}

Describe 'Invoke-MSTestWithCoverageMain comment-based help' {
    It 'documents the scoped-run behavior on the SearchRoot parameter' {
        # AC4. Read from the parsed help rather than by invoking Get-Help, so no module load is needed.
        $help = (Get-CoverageEntryPointAst).GetHelpContent()
        $key = @($help.Parameters.Keys | Where-Object { $_ -ieq 'SearchRoot' })

        $key.Count | Should -Be 1
        $text = $help.Parameters[$key[0]]
        $text | Should -Match '(?i)scoped'
        $text | Should -Match '(?i)repository root'
        $text | Should -Match '(?i)skip'
    }
}
