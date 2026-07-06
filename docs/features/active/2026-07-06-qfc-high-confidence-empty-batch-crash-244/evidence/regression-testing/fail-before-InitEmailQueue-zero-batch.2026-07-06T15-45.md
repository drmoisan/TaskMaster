# P1-T3 [expect-fail] — Fail-Before Evidence: InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing (Issue #244, v1.1)

Timestamp: 2026-07-06T15-45

Command: `MSYS_NO_PATHCONV=1 "/c/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/CommonExtensions/Microsoft/TestWindow/vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~InitEmailQueue_ZeroBatchSize|FullyQualifiedName~InitEmailQueue_PositiveBatchSize"`

EXIT_CODE: 1

## Procedure note (v1.1 seam-correct capture)

The `RemainingEmailLoader` seam (P1-T2) was added to `QfcDatamodel.cs` and the rewritten test file
(with `CreateInertRemainingEmailLoader` and the inert-loader assignment before every `InitEmailQueue`
call) was in place for this run. Because a prior execution cycle had already applied the
`batchSize <= 0` guard fix to the working tree, the guard block was temporarily removed
(byte-for-byte, comment-and-code) immediately before this run and restored byte-for-byte immediately
afterward (confirmed via `git diff` showing only the seam-related additions remain) — this is the
mechanical step required to reproduce a genuine pre-fix failing run rather than fabricate one. This
does not change the guard's final content, which is unchanged from the pre-existing v1.0 fix.

## Output Summary

`InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing` **Failed** in 379 ms with:

```
Did not expect any exception, but found System.Exception: The interface member 'EntryId' does not exist in the column index.
  at Deedle.Frame`2.GetRowsAs[TRow]()
  at QuickFiler.Controllers.QfcDatamodel.InitEmailQueue(Int32 batchSize, BackgroundWorker worker) in QfcDatamodel.cs:line 246
```

This confirms the test is red today (pre-guard state), reproducing the exact Deedle exception from
the Root Cause Summary. The inert `RemainingEmailLoader` was assigned before the call, but the
exception is thrown inside the `batchSize > 0` slice-and-project block, before `SetupWorker`/
`RunWorkerAsync` ever execute — so no `MessageBox.Show` pop-up and no live COM call (`_olApp`) occurred
during this run. `grep -c "MessageBox"` against the captured console output returned `0`.

Full run: 3 tests, 1 passed (`InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop`),
2 failed (this test and `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker`, evidenced
separately in `fail-before-InitEmailQueue-worker-start.2026-07-06T15-45.md`).
