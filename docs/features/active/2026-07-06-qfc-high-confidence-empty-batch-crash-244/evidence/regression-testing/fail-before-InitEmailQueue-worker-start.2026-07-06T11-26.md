# Fail-Before Evidence — InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker (Issue #244, P1-T3)

Timestamp: 2026-07-06T12-01

Command: & "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~InitEmailQueue_ZeroBatchSize|FullyQualifiedName~InitEmailQueue_PositiveBatchSize"

(Note: `|` used as the OR operator per the tooling note recorded in `evidence/baseline/baseline-test-filter.md`.)

EXIT_CODE: 1

Output Summary: `Failed InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker [3 ms]`. The test method throws `System.Exception: The interface member 'EntryId' does not exist in the column index.` before reaching `SetupWorker`/`RunWorkerAsync`, exactly as expected: the pre-guard code fails inside `InitEmailQueue` at the `GetRowsAs<IEmailSortInfo>()` call before the `SetupWorker(worker); worker.RunWorkerAsync();` lines execute. Total tests run in this filter pass: 2 (`InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing` also Failed, consistent with P1-T2). The test is confirmed red today.
