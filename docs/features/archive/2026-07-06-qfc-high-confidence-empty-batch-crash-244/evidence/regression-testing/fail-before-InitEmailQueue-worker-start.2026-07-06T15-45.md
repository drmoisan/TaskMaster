# P1-T4 [expect-fail] — Fail-Before Evidence: InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker (Issue #244, v1.1)

Timestamp: 2026-07-06T15-45

Command: `MSYS_NO_PATHCONV=1 "/c/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/CommonExtensions/Microsoft/TestWindow/vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~InitEmailQueue_ZeroBatchSize|FullyQualifiedName~InitEmailQueue_PositiveBatchSize"`

EXIT_CODE: 1

## Procedure note

Captured from the same run as `fail-before-InitEmailQueue-zero-batch.2026-07-06T15-45.md` (guard
temporarily reverted for this evidence-capture cycle and restored byte-for-byte immediately
afterward; see that artifact for the full procedure note).

## Output Summary

`InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` **Failed** in 4 ms with:

```
System.Exception: The interface member 'EntryId' does not exist in the column index.
  at Deedle.Frame`2.GetRowsAs[TRow]()
  at QuickFiler.Controllers.QfcDatamodel.InitEmailQueue(Int32 batchSize, BackgroundWorker worker) in QfcDatamodel.cs:line 246
```

This confirms the test is red today (pre-guard state): the exception is thrown before `SetupWorker`/
`worker.RunWorkerAsync()` are ever reached, so neither the `worker.WorkerSupportsCancellation`
assertion nor the `loaderInvokedTcs.Task.Wait(...)` assertion is reached. The test method contains
no `worker.IsBusy` assertion and no `Thread.Sleep`/`Task.Delay` call (verified by inspection of
`QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs`, method
`InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker`). No `MessageBox.Show` pop-up and no live
COM call occurred during this run (`grep -c "MessageBox"` against the captured console output
returned `0`).
