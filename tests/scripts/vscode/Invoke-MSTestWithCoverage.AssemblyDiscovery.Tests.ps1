Set-StrictMode -Version Latest

BeforeAll {
    $script:repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
    $script:mstestScript = Join-Path $script:repoRoot 'scripts\vscode\Invoke-MSTest.ps1'
    $script:coverageScript = Join-Path $script:repoRoot 'scripts\vscode\Invoke-MSTestWithCoverage.ps1'
    $script:scriptDir = Join-Path $script:repoRoot 'scripts\vscode'

    . $script:mstestScript

    $tokens = $null
    $parseErrors = $null
    $coverageAst = [System.Management.Automation.Language.Parser]::ParseFile(
        $script:coverageScript,
        [ref]$tokens,
        [ref]$parseErrors)
    $parseErrors | Should -BeNullOrEmpty
    . $coverageAst.GetScriptBlock()
    . (Join-Path $script:scriptDir 'Invoke-MSTestWithCoverage.Helpers.ps1')
}

Describe 'Invoke-MSTestWithCoverage assembly discovery' {
    BeforeEach {
        $script:capturedTestAssembly = $null

        Mock Resolve-Path { [pscustomobject]@{ Path = 'C:\repo' } }
        Mock Test-Path { $true }
        Mock Resolve-RunSettingsPath { 'C:\repo\scripts\vscode\TaskMaster.cli.runsettings' }
        Mock Invoke-VsWhereExe {
            param([string]$VsWherePath, [string[]]$VsWhereArgs)
            $null = $VsWherePath, $VsWhereArgs
            'C:\repo\vstest.console.exe'
        }
        Mock Get-Command { [pscustomobject]@{ Name = 'dotnet-coverage' } }
        Mock Get-ChildItem { @() }
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
        Mock Get-Content { '<coverage />' }
        Mock ConvertTo-KoverageCoberturaXml { '<coverage line-rate="0.8" />' }
        Mock Set-Content {}
    }

    It 'includes an assembly directly beneath a search root that is itself under a .claude worktree segment' {
        # Issue #752: discovery must not exclude the worktree it is running in.
        Mock Resolve-Path { [pscustomobject]@{ Path = 'C:\repo\.claude\worktrees\agent-7' } }
        Mock Get-ChildItem {
            @(
                [pscustomobject]@{ FullName = 'C:\repo\.claude\worktrees\agent-7\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' }
            )
        }

        Invoke-MSTestWithCoverageMain -ScriptRoot $script:scriptDir

        $script:capturedTestAssembly |
            Should -Be @('C:\repo\.claude\worktrees\agent-7\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll')
    }

    It 'excludes a nested sibling worktree beneath a non-dot-claude search root' {
        # Issue #733 finding 3, preserved: a sibling agent worktree nested under the
        # search root carries its own built copy and must still be dropped.
        Mock Get-ChildItem {
            @(
                [pscustomobject]@{ FullName = 'C:\repo\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' },
                [pscustomobject]@{ FullName = 'C:\repo\.claude\worktrees\agent-1\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' }
            )
        }

        Invoke-MSTestWithCoverageMain -ScriptRoot $script:scriptDir

        $script:capturedTestAssembly |
            Should -Be @('C:\repo\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll')
    }

    It 'retains the root-level assembly and excludes a further-nested worktree beneath a dot-claude search root' {
        # Both behaviours must hold for the same search root, so a fix that simply
        # disables the exclusion whenever the root is under .claude cannot pass.
        Mock Resolve-Path { [pscustomobject]@{ Path = 'C:\repo\.claude\worktrees\agent-7' } }
        Mock Get-ChildItem {
            @(
                [pscustomobject]@{ FullName = 'C:\repo\.claude\worktrees\agent-7\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' },
                [pscustomobject]@{ FullName = 'C:\repo\.claude\worktrees\agent-7\.claude\worktrees\agent-9\Nested.Test\bin\Debug\Nested.Test.dll' }
            )
        }

        Invoke-MSTestWithCoverageMain -ScriptRoot $script:scriptDir

        $script:capturedTestAssembly |
            Should -Be @('C:\repo\.claude\worktrees\agent-7\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll')
    }
}
