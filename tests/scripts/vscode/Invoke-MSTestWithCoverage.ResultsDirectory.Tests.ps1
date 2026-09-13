Set-StrictMode -Version Latest

BeforeAll {
    $script:repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
    $script:scriptDir = Join-Path $script:repoRoot 'scripts\vscode'
    $script:coverageScript = Join-Path $script:scriptDir 'Invoke-MSTestWithCoverage.ps1'

    # The production entry point guards its top-level wiring with an invocation-name check, so
    # parsing it and dot-sourcing the resulting scriptblock imports definitions without running the
    # body. The parsed tree is kept because the abstract-syntax-tree tests below read it directly.
    $tokens = $null
    $parseErrors = $null
    $script:coverageAst = [System.Management.Automation.Language.Parser]::ParseFile(
        $script:coverageScript,
        [ref]$tokens,
        [ref]$parseErrors)
    $parseErrors | Should -BeNullOrEmpty
    . $script:coverageAst.GetScriptBlock()
    . (Join-Path $script:scriptDir 'Invoke-MSTestWithCoverage.Helpers.ps1')

    # The builder is invoked by splatting so an added parameter costs one key rather than one line
    # at every call site. Both new values are fixed here so every assertion below quotes the same
    # switch text rather than deriving it.
    $script:resultsDirectory = 'C:\repo\coverage\test-results'
    $script:logFileName = 'mstest-coverage-run.trx'
    $script:builderArgument = @{
        OutputPath       = 'C:\repo\coverage\coverage.cobertura.xml'
        CoverageConfig   = 'C:\repo\coverage.config'
        VsTestPath       = 'C:\vstest.console.exe'
        TestAssembly     = @('C:\repo\A.Test.dll')
        RunSettingsPath  = 'C:\repo\scripts\vscode\TaskMaster.cli.runsettings'
        ResultsDirectory = $script:resultsDirectory
        LogFileName      = $script:logFileName
    }
    $script:expectedResultsDirectorySwitch = '/ResultsDirectory:' + $script:resultsDirectory
    $script:expectedLoggerSwitch = '/Logger:trx;LogFileName=' + $script:logFileName

    # The two documents the ordering test's mocks answer with. The post-processed document carries
    # root lines-covered and lines-valid attributes over one package whose figures sum to them, so
    # the reconciliation the entry point runs holds exactly. The test-result document declares the
    # default TeamTest namespace, as a real one does. Both are in-memory here-strings.
    $script:postProcessedCoverageXml = @'
<coverage line-rate="0.8" lines-covered="4" lines-valid="5"><packages><package name="Alpha.Core"><classes><class name="Alpha.Core.Widget" filename="Alpha.Core\Widget.cs"><lines><line number="10" hits="1" /><line number="11" hits="2" /><line number="12" hits="3" /><line number="13" hits="4" /><line number="14" hits="0" /></lines></class></classes></package></packages></coverage>
'@
    $script:coverageTrxFixture = @'
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Results><UnitTestResult testName="Contoso.Alpha.PassesCleanly" outcome="Passed" /></Results>
  <ResultSummary outcome="Completed"><Counters total="1" executed="1" passed="1" failed="0" /></ResultSummary>
</TestRun>
'@

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

    function Get-NamedArgumentAst {
        <#
            .SYNOPSIS
            Returns the element a command binds to one named parameter.

            .DESCRIPTION
            A test-only reader so an argument assertion names the parameter it is interested in
            rather than repeating the walk over the command's elements. The element immediately
            following the parameter element is the bound argument for a non-switch parameter.
        #>
        [CmdletBinding()]
        [OutputType([System.Management.Automation.Language.ExpressionAst])]
        param(
            [Parameter(Mandatory = $true)]
            [System.Management.Automation.Language.CommandAst]$CommandAst,

            [Parameter(Mandatory = $true)]
            [string]$ParameterName
        )

        $element = @($CommandAst.CommandElements)
        for ($index = 0; $index -lt $element.Count - 1; $index++) {
            if ($element[$index] -is [System.Management.Automation.Language.CommandParameterAst] -and
                $element[$index].ParameterName -eq $ParameterName) {
                return $element[$index + 1]
            }
        }

        return $null
    }
}

Describe 'Get-DotnetCoverageArgumentList results directory and log file name' {
    It 'includes the explicit results directory and trx log file name in the coverage argument list' {
        # AC12. The log file name is supplied explicitly rather than left to the test console, which
        # otherwise derives a machine-and-timestamp name no later step could read.
        $arguments = Get-DotnetCoverageArgumentList @script:builderArgument

        $arguments | Should -Contain $script:expectedResultsDirectorySwitch
        $arguments | Should -Contain $script:expectedLoggerSwitch
    }

    It 'places both new switches after the argument separator' {
        # Both switches configure the inner test console rather than the outer collector, so both
        # must sit after the separator element. The comparison is by index, not by presence.
        $arguments = Get-DotnetCoverageArgumentList @script:builderArgument

        $separatorIndex = [array]::IndexOf($arguments, '--')
        $resultsDirectoryIndex = [array]::IndexOf($arguments, $script:expectedResultsDirectorySwitch)
        $loggerIndex = [array]::IndexOf($arguments, $script:expectedLoggerSwitch)

        $separatorIndex | Should -BeGreaterThan -1
        $resultsDirectoryIndex | Should -BeGreaterThan $separatorIndex
        $loggerIndex | Should -BeGreaterThan $separatorIndex
    }
}

Describe 'Invoke-MSTestWithCoverageMain results-directory default' {
    It 'defaults the coverage entry-point results directory beneath the repository coverage directory' {
        # AC13. Read from the parameter default rather than by invoking the entry point, so the
        # assertion depends on the declared default and not on any mock arrangement.
        $resultsDirectoryParameter = @(
            (Get-CoverageEntryPointAst).Body.ParamBlock.Parameters |
                Where-Object { $_.Name.VariablePath.UserPath -eq 'ResultsDirectory' }
        )

        $resultsDirectoryParameter.Count | Should -Be 1
        $resultsDirectoryParameter[0].DefaultValue.Extent.Text |
            Should -BeExactly "'coverage\test-results'"
    }
}

Describe 'Test-RawCoverageDocumentRetained' {
    It 'retains the raw document when the output directory is the repository coverage directory' {
        # AC14. The coverage directory is already ignored by this repository, so a raw document
        # written there cannot be staged and is kept.
        $outputPath = Join-Path (Join-Path $script:repoRoot 'coverage') 'coverage.cobertura.xml'

        Test-RawCoverageDocumentRetained -OutputPath $outputPath -RepoRoot $script:repoRoot |
            Should -BeTrue
    }

    It 'discards the raw document for any other output directory' {
        $outputPath = Join-Path (Join-Path $script:repoRoot 'tm873-unrelated-output') 'coverage.cobertura.xml'

        Test-RawCoverageDocumentRetained -OutputPath $outputPath -RepoRoot $script:repoRoot |
            Should -BeFalse
    }

    It 'discards the raw document for a subdirectory of the repository coverage directory' {
        # Not redundant with the case above. The predicate is specified as an equality between the
        # output path's full parent directory and the repository coverage directory, and a
        # containment implementation would return true here while still returning false for the
        # unrelated directory, so without this case a containment implementation passes both others.
        $outputPath = Join-Path `
        (Join-Path $script:repoRoot 'coverage\tm873-external-output') `
            'coverage.cobertura.xml'

        Test-RawCoverageDocumentRetained -OutputPath $outputPath -RepoRoot $script:repoRoot |
            Should -BeFalse
    }
}

Describe 'Invoke-MSTestWithCoverageMain discard ordering' {
    BeforeEach {
        # Call order is captured through the named wrapper seams into a script-scoped collection.
        # Every filesystem command the entry point reaches is mocked, so no test here creates,
        # writes or deletes a file, and every fixture is an in-memory string.
        $script:callOrder = [System.Collections.Generic.List[string]]::new()

        Mock Resolve-Path { [pscustomobject]@{ Path = 'C:\repo' } }
        Mock Test-Path { $true }
        Mock Resolve-RunSettingsPath { 'C:\repo\scripts\vscode\TaskMaster.cli.runsettings' }
        Mock Invoke-VsWhereExe { 'C:\repo\vstest.console.exe' }
        Mock Get-Command { [pscustomobject]@{ Name = 'dotnet-coverage' } }
        Mock Get-ChildItem {
            [pscustomobject]@{ FullName = 'C:\repo\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' }
        }
        Mock Invoke-DotnetCoverageCollection {}
        Mock Get-Content { '<coverage />' }
        Mock Get-Content -ParameterFilter { $LiteralPath -like '*mstest-coverage-run.trx' } -MockWith { $script:coverageTrxFixture }
        Mock ConvertTo-KoverageCoberturaXml { $script:postProcessedCoverageXml }
        Mock Set-Content { $script:callOrder.Add('other-write') }
        Mock Set-Content -ParameterFilter { $Path -like '*.jacoco.xml' } -MockWith { $script:callOrder.Add('projection-write') }
        Mock Assert-CoberturaLineCoverageThreshold { $script:callOrder.Add('threshold') }
        Mock Assert-JacocoProjectionReconciliation { $script:callOrder.Add('reconciliation') }
        Mock Remove-Item { $script:callOrder.Add('discard') }
    }

    It 'discards only after the threshold assertion, the projection write and the reconciliation assertion' {
        # The output directory is a subdirectory of the coverage tree, which the predicate above
        # reports as not retained, so the discard is reached on this path.
        Invoke-MSTestWithCoverageMain `
            -CoverageOutput 'coverage\tm873-external-output\coverage.cobertura.xml' `
            -ScriptRoot $script:scriptDir

        $discardIndex = $script:callOrder.IndexOf('discard')
        $thresholdIndex = $script:callOrder.IndexOf('threshold')
        $projectionWriteIndex = $script:callOrder.IndexOf('projection-write')
        $reconciliationIndex = $script:callOrder.IndexOf('reconciliation')

        $thresholdIndex | Should -BeGreaterThan -1
        $projectionWriteIndex | Should -BeGreaterThan -1
        $reconciliationIndex | Should -BeGreaterThan -1
        $discardIndex | Should -BeGreaterThan $thresholdIndex
        $discardIndex | Should -BeGreaterThan $projectionWriteIndex
        $discardIndex | Should -BeGreaterThan $reconciliationIndex
    }
}

Describe 'Invoke-MSTestWithCoverageMain projection wiring' {
    It 'builds the projection from the post-processed content rather than the raw collector string' {
        # AC4. The raw collector document still carries test and third-party packages and absolute
        # source paths, so a projection built from it would neither reconcile nor be committable.
        # The assertion is two-part: the projection writer's document argument is a variable, and
        # that same variable is the one assigned from the post-processor.
        $entryPointAst = Get-CoverageEntryPointAst
        $projectionCall = $entryPointAst.Find(
            {
                $args[0] -is [System.Management.Automation.Language.CommandAst] -and
                $args[0].GetCommandName() -eq 'ConvertTo-JacocoPackageProjection'
            },
            $true)
        $projectionCall | Should -Not -BeNullOrEmpty

        $documentArgument = Get-NamedArgumentAst `
            -CommandAst $projectionCall `
            -ParameterName 'XmlDocument'
        $documentArgument |
            Should -BeOfType ([System.Management.Automation.Language.VariableExpressionAst])

        $postProcessorAssignment = $entryPointAst.Find(
            {
                $args[0] -is [System.Management.Automation.Language.AssignmentStatementAst] -and
                $args[0].Right.Extent.Text -like '*ConvertTo-KoverageCoberturaXml*'
            },
            $true)
        $postProcessorAssignment | Should -Not -BeNullOrEmpty
        $documentArgument.VariablePath.UserPath |
            Should -BeExactly $postProcessorAssignment.Left.VariablePath.UserPath
    }

    It 'invokes the reconciliation assertion on the coverage path' {
        # AC5. The projection is committed in place of the document it summarises, so the entry
        # point must reconcile the two rather than leaving the arithmetic unchecked.
        $reconciliationCall = (Get-CoverageEntryPointAst).Find(
            {
                $args[0] -is [System.Management.Automation.Language.CommandAst] -and
                $args[0].GetCommandName() -eq 'Assert-JacocoProjectionReconciliation'
            },
            $true)

        $reconciliationCall | Should -Not -BeNullOrEmpty
    }
}
