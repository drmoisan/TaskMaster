# Test + Coverage Baseline

Timestamp: 2026-07-08T01-27

Command (coverage): dotnet-coverage collect -f cobertura -o baseline-cov.cobertura.xml "vstest.console.exe" UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"

Command (non-instrumented cross-check): vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"

(dotnet-coverage collect wraps vstest to emit a numeric Cobertura XML; this is the reliable numeric-coverage path in this repo. /InIsolation is required for the Moq test assemblies. The plan's `/EnableCodeCoverage` is a coverage-collection mechanism; dotnet-coverage collect is the equivalent mechanism that yields the numeric value the plan requires.)

EXIT_CODE: 1 (coverage run — due to pre-existing coverage-instrumentation-induced failures, see below)

Output Summary:
- Coverage-instrumented run: Total tests 4412, Passed 4395, Failed 17.
- Non-instrumented cross-check run: Total tests 4412, Passed 4411, Failed 1.
- The 17 coverage-run failures are ALL Deedle/DataFrame tests (DeedleDoodles, DropFirstN_DropsFirstNRows, Email2dArrayToDf_*, Exclude_*, FromArray2D_*, FromDefaultFolder_*, GetColumnEid_*, GetDuplicateEntriesByColumn_*, GetEmailDataFromTable_*, GetEmailDataInView*, PrintToLog_*): the Deedle/FSharp.Core code path fails under coverage instrumentation. Pre-existing and unrelated to F3 (which touches store-rehook logic in UtilitiesCS store/folder/AppEvents and TaskMaster AppGlobals).
- The single non-instrumented failure is TryAddValuesAsync_UpdatesExistingValue (a ~22s timing-sensitive test), a known flaky timing test.
- "Failed loading language 'eng'" lines are Tesseract OCR stderr noise, not test failures.

Numeric coverage (Cobertura line-rate):
- Overall (all packages, incl. test + vendored): 61.94% (lines-covered 102707 / lines-valid 165825).
- First-party production packages touched by F3:
  - UtilitiesCS: 87.98%
  - TaskMaster: 64.08%
- Near-zero first-party packages (QuickFiler 0%, Tags 0%, ToDoModel 2.3%, TaskVisualization 18.3%) are COM/VSTO/WinForms-bound; their dedicated test assemblies are NOT part of this two-assembly (UtilitiesCS.Test + TaskMaster.Test) run, so their figures here are a test-scope artifact, not the testable denominator.
- Testable-denominator no-regression is verified as a baseline-vs-post-change delta on the identical command (P6-T5), which is robust to the absolute-denominator definition.
