# Pre-Fix Mixed Test-Run Evidence (Issue #244, P1-T5)

Timestamp: 2026-07-06T12-05

Command: & "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~InitEmailQueue_ZeroBatchSize|FullyQualifiedName~InitEmailQueue_PositiveBatchSize"

(Note: `|` used as the OR operator per the tooling note recorded in `evidence/baseline/baseline-test-filter.md`.)

EXIT_CODE: 1

Output Summary:
- `Failed InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing [432 ms]`
- `Failed InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker [4 ms]`
- `Passed InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop [187 ms]`
- Total tests: 3, Passed: 1, Failed: 2

This matches the plan's expected pre-fix mixed baseline exactly (1 passed, 2 failed).

Tooling observation (not a test defect, no action taken — pre-existing production behavior in `LoadRemainingEmailsToQueueAsync(CancellationToken)`): the positive-batch characterization test drains `_frame` to `RowCount == 0` as part of exercising the existing `batchSize > 0` frame-drop behavior. The real `BackgroundWorker.RunWorkerAsync()` call this triggers (pre-existing `SetupWorker`/`RunWorkerAsync` side effect, unchanged by this fix) then runs `Worker_DoWork` on a background thread, which reaches the pre-existing `if ((_frame is null) || (_frame.RowCount == 0)) { MessageBox.Show("Email Frame is empty"); return false; }` guard in `LoadRemainingEmailsToQueueAsync`. This creates a real (untitled) Win32 dialog (`#32770` window class) on the interactive desktop for several seconds before the vstest test host tears down, which was confirmed via a `user32.dll` `EnumWindows` probe during a diagnostic run. It does not fail the test (the test's own assertions already complete synchronously before the background thread reaches this code) and it does not hang the process — the vstest run consistently completed within the command's own reported `Total time` (single-digit to ~17 seconds) and returned control normally in every run performed for this plan. This is existing production behavior in code the plan explicitly instructs not to modify (`QfcHomeController.cs` is out of scope, and the `LoadRemainingEmailsToQueueAsync` empty-frame guard is unrelated to and unmodified by the `InitEmailQueue` zero-batch fix); it is called out here for audit transparency.
