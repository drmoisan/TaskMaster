Set-StrictMode -Version Latest

# There is deliberately no file-level setup block. Each Describe owns its own setup, so the
# entry-point guard case below can be run under a name filter without any Describe dot-sourcing
# the production file. That is what makes the fail-before run for the guard safe on the pre-fix
# tree, where dot-sourcing would launch vswhere.exe and then msbuild.exe against the real
# repository root.

Describe 'Invoke-Restore.ps1 entry-point guard' {
    BeforeAll {
        $script:guardRepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
        $script:guardScriptPath = Join-Path $script:guardRepoRoot 'scripts\vscode\Invoke-Restore.ps1'
    }

    It 'defines functions only and performs no work on dot-source' {
        # The production file is parsed, never dot-sourced or executed. Its top-level statements
        # must be exactly three: strict mode, the error preference, and the invocation guard.
        # Anything else at top level runs as an uncontrolled side effect of a dot-source.
        $tokens = $null
        $parseErrors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            $script:guardScriptPath,
            [ref]$tokens,
            [ref]$parseErrors)
        $parseErrors | Should -BeNullOrEmpty

        $topLevelStatements = @(
            $ast.EndBlock.Statements |
                Where-Object { $_ -isnot [System.Management.Automation.Language.FunctionDefinitionAst] })

        $topLevelStatements.Count | Should -Be 3

        $strictModeStatements = @(
            $topLevelStatements | Where-Object { $_.Extent.Text -match '^\s*Set-StrictMode\b' })
        $strictModeStatements.Count | Should -Be 1

        $errorPreferenceStatements = @(
            $topLevelStatements |
                Where-Object {
                    $_ -is [System.Management.Automation.Language.AssignmentStatementAst] -and
                    $_.Left.Extent.Text -eq '$ErrorActionPreference'
                })
        $errorPreferenceStatements.Count | Should -Be 1

        $guardStatements = @(
            $topLevelStatements |
                Where-Object { $_ -is [System.Management.Automation.Language.IfStatementAst] })
        $guardStatements.Count | Should -Be 1

        $guardClause = $guardStatements[0].Clauses[0]
        $guardClause.Item1.Extent.Text | Should -BeLike '*InvocationName*'

        $guardBodyCommands = @(
            $guardClause.Item2.FindAll(
                { $args[0] -is [System.Management.Automation.Language.CommandAst] },
                $true))
        $guardBodyCommands.Count | Should -Be 1
        $guardBodyCommands[0].GetCommandName() | Should -Be 'Invoke-RestoreMain'
    }
}

Describe 'Invoke-RestoreMain' {
    # The extracted entry-point body. Every external dependency is reached through a named seam
    # and is mocked here: no vswhere.exe, no msbuild.exe, no disk access, no temporary files.

    BeforeAll {
        $script:mainRepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
        $script:mainScriptDir = Join-Path $script:mainRepoRoot 'scripts\vscode'
        . (Join-Path $script:mainScriptDir 'Invoke-Restore.ps1')
    }

    BeforeEach {
        $script:capturedRestorePath = $null
        $script:capturedRestoreArgs = $null

        Mock Resolve-Path { [pscustomobject]@{ Path = 'C:\repo' } }
        Mock Test-Path { $true }
        Mock Write-Host {}
        Mock Get-RestoreMSBuildPath { 'C:\repo\MSBuild.exe' }
        Mock Invoke-RestoreMSBuildExe {
            param([string]$MSBuildPath, [string[]]$MSBuildArgs)
            $script:capturedRestorePath = $MSBuildPath
            $script:capturedRestoreArgs = $MSBuildArgs
            $global:LASTEXITCODE = 0
        }
    }

    It 'fails when the solution cannot be found' {
        Mock Test-Path { $false }

        { Invoke-RestoreMain -ScriptRoot $script:mainScriptDir } |
            Should -Throw -ExpectedMessage 'Solution not found: C:\repo\TaskMaster.sln'
    }

    It 'fails when vswhere.exe is not installed' {
        Mock Test-Path { $false } -ParameterFilter { $Path -like '*vswhere.exe' }

        { Invoke-RestoreMain -ScriptRoot $script:mainScriptDir } |
            Should -Throw -ExpectedMessage 'vswhere.exe was not found. Install Visual Studio 2022 (or Build Tools) with MSBuild components.'
    }

    It 'fails when vswhere resolves no MSBuild path' {
        Mock Get-RestoreMSBuildPath { $null }

        { Invoke-RestoreMain -ScriptRoot $script:mainScriptDir } |
            Should -Throw -ExpectedMessage 'MSBuild.exe not found via vswhere. Install Visual Studio MSBuild components.'
    }

    It 'throws when MSBuild Restore reports a non-zero exit code' {
        Mock Invoke-RestoreMSBuildExe {
            param([string]$MSBuildPath, [string[]]$MSBuildArgs)
            $null = $MSBuildPath, $MSBuildArgs
            $global:LASTEXITCODE = 1
        }

        { Invoke-RestoreMain -ScriptRoot $script:mainScriptDir } |
            Should -Throw -ExpectedMessage 'MSBuild Restore failed with exit code 1'
    }
}

Describe 'Invoke-Restore.ps1 wrapper seams' {
    # These two cases execute the seam bodies rather than mocking them, so the seams themselves
    # are covered. Each passes an in-process command name in place of the executable path, in the
    # style of the splatting-seam case in tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1. No
    # external process is launched.

    BeforeAll {
        $script:seamRepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
        . (Join-Path $script:seamRepoRoot 'scripts\vscode\Invoke-Restore.ps1')

        # An in-process stand-in whose parameters bind the seam's own argument list. It is not a
        # mock of the seam: the seam body runs and calls this function through the call operator.
        function Invoke-RestoreVsWhereStandIn {
            param(
                [switch]$latest,
                [string]$requires,
                [string]$find
            )

            # The -latest switch is bound by the seam's own argument list and is read here so it
            # is not reported as a declared-but-unused parameter.
            $null = $latest
            "C:\stand-in\$requires\$find"
        }
    }

    It 'resolves the MSBuild path through the vswhere seam' {
        $resolved = Get-RestoreMSBuildPath -VsWherePath 'Invoke-RestoreVsWhereStandIn'

        $resolved | Should -Be 'C:\stand-in\Microsoft.Component.MSBuild\MSBuild\**\Bin\MSBuild.exe'
    }

    It 'forwards every argument array element as a separate positional argument' {
        # Join-Path takes two positional arguments, so the returned value proves both elements
        # arrived, in order.
        $result = Invoke-RestoreMSBuildExe -MSBuildPath 'Join-Path' -MSBuildArgs @('C:\alpha', 'beta')

        $result | Should -Be 'C:\alpha\beta'
    }
}
