# Issue 995 csharp-tests-coverage

Timestamp: 2026-10-17T23-12
Command: pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/issue-995-final.cobertura.xml
EXIT_CODE: 0
Output Summary:   Passed BtnSave_Click_WithNullController_DoesNotThrow [7 ms]
Test Parallelization enabled for .\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll (Workers: 24, Scope: ClassLevel)
  Passed TlvOriginal_ModelDropped_ForwardsEventToController_DoesNotThrow [9 ms]
  Passed SetController_WithSyntheticController_ConfiguresTreeDelegates [10 ms]
  Passed FormatFileSize_ReturnsExpectedStringForSampleInputs [10 ms]
  Passed AfterDeserialize_WithConfiguredGlobals_LoadsClassifiersAndReportsCompletion [4 ms]
  Passed HeavyParallelizationAsync_WithNullProbabilities_RebuildsLegacyClassifierState [9 ms]
  Passed HeavyParallelizationAsync_WithFewerClassifiersThanProcessors_DoesNotThrow [1 ms]
  Passed ComputerInfo_PropertyTests [27 ms]
WARNING: Overwriting results file: .\coverage\test-results\mstest-coverage-run.trx
Results File: .\coverage\test-results\mstest-coverage-run.trx

Test Run Successful.
Total tests: 7470
     Passed: 7470
 Total time: 47.4487 Seconds
Code coverage results: .\coverage\issue-995-final.cobertura.xml.
Post-processing coverage XML for Koverage compatibility...
First-party coverage: lines 56926/66178 (86.02%), branches 13791/17195 (80.20%)
Coverage projection: .\coverage\issue-995-final.cobertura.jacoco.xml
Test-result summary: .\coverage\test-results\mstest-coverage-run.summary.txt
Done. Coverage artifact: .\coverage\issue-995-final.cobertura.xml
