# Fail-Before Evidence — InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing (Issue #244, P1-T2)

Timestamp: 2026-07-06T11-58

Command: & "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~InitEmailQueue_ZeroBatchSize|FullyQualifiedName~InitEmailQueue_PositiveBatchSize"

(Note: `|` used as the OR operator per the tooling note recorded in `evidence/baseline/baseline-test-filter.md` — this vstest 18.7.0 build rejects the literal `OR` keyword. Same target test names as specified by the plan.)

EXIT_CODE: 1

Output Summary: `Failed InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing [378 ms]`. Error message: "Did not expect any exception, but found System.Exception: The interface member 'EntryId' does not exist in the column index." Stack trace confirms the exception originates at `Deedle.Frame\`2.GetRowsAs[TRow]()` called from `QuickFiler.Controllers.QfcDatamodel.InitEmailQueue(Int32 batchSize, BackgroundWorker worker)` in `QfcDatamodel.cs:line 225` — the exact pre-fix defect described in the issue. Total tests: 1, Failed: 1. The test is confirmed red before the fix.
