Set-StrictMode -Version Latest

BeforeAll {
    $script:repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
    $script:scriptDir = Join-Path $script:repoRoot 'scripts\vscode'
    $script:mstestScript = Join-Path $script:scriptDir 'Invoke-MSTest.ps1'

    # The entry point's top-level wiring is guarded by an invocation-name check, so dot-sourcing
    # the file imports definitions without running the body. The file is parsed as well because
    # the parameter-default test below reads the declared default from the tree rather than from
    # any mock arrangement.
    $tokens = $null
    $parseErrors = $null
    $script:mstestAst = [System.Management.Automation.Language.Parser]::ParseFile(
        $script:mstestScript,
        [ref]$tokens,
        [ref]$parseErrors)
    $parseErrors | Should -BeNullOrEmpty
    . $script:mstestScript

    # The builder is invoked by splatting so an added parameter costs one key rather than one line
    # at every call site. Both new values are fixed here so every assertion below quotes the same
    # switch text rather than deriving it.
    $script:resultsDirectory = 'C:\repo\coverage\test-results'
    $script:logFileName = 'mstest-run.trx'
    $script:vsTestArgument = @{
        TestAssembly     = @('C:\repo\A.Test.dll')
        RunSettingsPath  = 'C:\repo\scripts\vscode\TaskMaster.cli.runsettings'
        ResultsDirectory = $script:resultsDirectory
        LogFileName      = $script:logFileName
    }
    $script:expectedResultsDirectorySwitch = '/ResultsDirectory:' + $script:resultsDirectory
    $script:expectedLoggerSwitch = '/Logger:trx;LogFileName=' + $script:logFileName

    function Get-PlainEntryPointAst {
        <#
            .SYNOPSIS
            Returns the parsed definition of the plain entry point.

            .DESCRIPTION
            A test-only reader so the parameter-default assertion names what it is looking for
            rather than repeating the search for the enclosing function definition. The tree is
            the one parsed once in this block, so no test re-reads the file from disk.
        #>
        [CmdletBinding()]
        [OutputType([System.Management.Automation.Language.FunctionDefinitionAst])]
        param()

        return $script:mstestAst.Find(
            {
                $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $args[0].Name -eq 'Invoke-MSTestMain'
            },
            $true)
    }
}

Describe 'Get-VsTestArgumentList results directory and log file name' {
    It 'includes the explicit results directory and trx log file name in the vstest argument list' {
        # AC12. The log file name is supplied explicitly rather than left to the test console, which
        # otherwise derives a machine-and-timestamp name no later step could read. Membership is
        # asserted by index so the assertion also pins the order the two switches are emitted in.
        $arguments = Get-VsTestArgumentList @script:vsTestArgument

        $resultsDirectoryIndex = [array]::IndexOf($arguments, $script:expectedResultsDirectorySwitch)
        $loggerIndex = [array]::IndexOf($arguments, $script:expectedLoggerSwitch)

        $resultsDirectoryIndex | Should -BeGreaterThan -1
        $loggerIndex | Should -BeGreaterThan $resultsDirectoryIndex
    }
}

Describe 'Invoke-MSTestMain results-directory default' {
    It 'defaults the entry-point results directory beneath the repository coverage directory' {
        # AC13. Read from the parameter default rather than by invoking the entry point, so the
        # assertion depends on the declared default and not on any mock arrangement. The coverage
        # directory is already ignored by this repository, so the default needs no ignore change.
        $resultsDirectoryParameter = @(
            (Get-PlainEntryPointAst).Body.ParamBlock.Parameters |
                Where-Object { $_.Name.VariablePath.UserPath -eq 'ResultsDirectory' }
        )

        $resultsDirectoryParameter.Count | Should -Be 1
        $resultsDirectoryParameter[0].DefaultValue.Extent.Text |
            Should -BeExactly "'coverage\test-results'"
    }
}

Describe 'Invoke-MSTestMain non-fatal summary path' {
    BeforeEach {
        # Every seam the entry point reaches is mocked, so this test starts no process and touches
        # no file. The content reader throws, which is what a missing test-result document does, so
        # the non-fatal branch is the one under test.
        $script:emittedWarning = [System.Collections.Generic.List[string]]::new()

        Mock Resolve-Path { [pscustomobject]@{ Path = 'C:\repo' } }
        Mock Test-Path { $true }
        Mock Resolve-RunSettingsPath { 'C:\repo\scripts\vscode\TaskMaster.cli.runsettings' }
        Mock Get-VsTestConsolePath { 'C:\repo\vstest.console.exe' }
        Mock Get-MSTestAssemblyPathList { , @('C:\repo\A.Test\bin\Debug\A.Test.dll') }
        Mock Invoke-VsTestExe { $global:LASTEXITCODE = 0 }
        Mock Get-Content { throw 'Simulated missing test-result document.' }
        Mock Write-Warning { param([string]$Message) $script:emittedWarning.Add($Message) }
        Mock Set-Content {}
        Mock Remove-Item {}
    }

    It 'warns without writing a summary when the test-result document cannot be read' {
        # AC12 companion. The run itself already succeeded, so an unreadable test-result document
        # must not fail the run; it must also not leave a summary behind and must not discard the
        # document whose summary was never written.
        Invoke-MSTestMain -ScriptRoot $script:scriptDir

        Should -Invoke Set-Content -Times 0 -Exactly
        Should -Invoke Remove-Item -Times 0 -Exactly
        $script:emittedWarning.Count | Should -Be 1
        $script:emittedWarning[0] | Should -BeLike 'Test-result summary was not written:*'
    }
}
