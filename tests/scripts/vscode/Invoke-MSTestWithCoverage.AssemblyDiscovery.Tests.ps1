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
                [string]$RunSettingsPath,
                [string]$ResultsDirectory,
                [string]$LogFileName
            )
            $null = $OutputPath, $CoverageConfig, $VsTestPath, $RunSettingsPath, $ResultsDirectory, $LogFileName
            $script:capturedTestAssembly = $TestAssembly
        }
        # The reader mock is deliberately left unfiltered. It answers the entry point's test-result
        # read with a document that parses but carries no result-summary node, so the summary write
        # and the discard both take the non-fatal warning branch and this file needs no further
        # change. The post-processor value carries the root lines-covered and lines-valid attributes
        # the reconciliation assertion reads, over one package whose figures sum to them.
        Mock Get-Content { '<coverage />' }
        Mock ConvertTo-KoverageCoberturaXml { '<coverage line-rate="0.8" branch-rate="0.8" lines-covered="4" lines-valid="5" branches-valid="10"><packages><package name="Alpha.Core"><classes><class name="Alpha.Core.Widget" filename="Alpha.Core\Widget.cs"><lines><line number="10" hits="1" /><line number="11" hits="2" /><line number="12" hits="3" /><line number="13" hits="4" /><line number="14" hits="0" /></lines></class></classes></package></packages></coverage>' }
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

Describe 'Invoke-MSTestWithCoverage threshold call-site wiring' {
    BeforeEach {
        $script:thresholdCallOrder = @()
        $script:lineAssertionArgument = $null
        $script:branchAssertionArgument = $null
        $script:expectedPostProcessed = '<coverage line-rate="0.8" branch-rate="0.8" lines-covered="4" lines-valid="5" branches-valid="10"><packages><package name="Alpha.Core"><classes><class name="Alpha.Core.Widget" filename="Alpha.Core\Widget.cs"><lines><line number="10" hits="1" /><line number="11" hits="2" /><line number="12" hits="3" /><line number="13" hits="4" /><line number="14" hits="0" /></lines></class></classes></package></packages></coverage>'

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
            @([pscustomobject]@{ FullName = 'C:\repo\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' })
        }
        Mock Invoke-DotnetCoverageCollection {}
        Mock Get-Content { '<coverage />' }
        Mock ConvertTo-KoverageCoberturaXml { $script:expectedPostProcessed }
        Mock Set-Content {}
    }

    # Both threshold mocks are registered inside each It body rather than in a setup block.
    # A mock of a command that does not yet exist, evaluated in a block-level setup, makes
    # Pester report a single block-setup error instead of two discrete failing cases, which
    # would hide the fail-before signal these two cases are written to produce.
    It 'invokes the branch assertion once with the post-processed document' {
        Mock Assert-CoberturaLineCoverageThreshold {
            param([string]$CoberturaXml)
            $script:thresholdCallOrder += 'line'
            $script:lineAssertionArgument = $CoberturaXml
        }
        Mock Assert-CoberturaBranchCoverageThreshold {
            param([string]$CoberturaXml)
            $script:thresholdCallOrder += 'branch'
            $script:branchAssertionArgument = $CoberturaXml
        }

        Invoke-MSTestWithCoverageMain -ScriptRoot $script:scriptDir

        Should -Invoke Assert-CoberturaLineCoverageThreshold -Times 1 -Exactly
        Should -Invoke Assert-CoberturaBranchCoverageThreshold -Times 1 -Exactly
        $script:branchAssertionArgument | Should -Be $script:expectedPostProcessed
    }

    It 'invokes the line assertion before the branch assertion' {
        Mock Assert-CoberturaLineCoverageThreshold {
            param([string]$CoberturaXml)
            $script:thresholdCallOrder += 'line'
            $script:lineAssertionArgument = $CoberturaXml
        }
        Mock Assert-CoberturaBranchCoverageThreshold {
            param([string]$CoberturaXml)
            $script:thresholdCallOrder += 'branch'
            $script:branchAssertionArgument = $CoberturaXml
        }

        Invoke-MSTestWithCoverageMain -ScriptRoot $script:scriptDir

        Should -Invoke Assert-CoberturaLineCoverageThreshold -Times 1 -Exactly
        Should -Invoke Assert-CoberturaBranchCoverageThreshold -Times 1 -Exactly
        $script:thresholdCallOrder | Should -Be @('line', 'branch')
        $script:lineAssertionArgument | Should -Be $script:expectedPostProcessed
    }
}
