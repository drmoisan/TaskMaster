# P1-T6 — Pre-Fix Combined Regression Run (Issue #244, v1.1)

Timestamp: 2026-07-06T15-45

Command: `MSYS_NO_PATHCONV=1 "/c/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/CommonExtensions/Microsoft/TestWindow/vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~InitEmailQueue_ZeroBatchSize|FullyQualifiedName~InitEmailQueue_PositiveBatchSize"`

EXIT_CODE: 1

## Procedure note

Same run as `fail-before-InitEmailQueue-zero-batch.2026-07-06T15-45.md` and
`fail-before-InitEmailQueue-worker-start.2026-07-06T15-45.md`: the `batchSize <= 0` guard was
temporarily reverted (byte-for-byte) to reproduce the genuine pre-fix state, then restored
byte-for-byte immediately after this run (confirmed via `git diff`). The v1.1 `RemainingEmailLoader`
seam and the rewritten test file (inert-loader assignment before every `InitEmailQueue` call) were
in place for the entire run.

## Output Summary

```
Total tests: 3
     Passed: 1
     Failed: 2
```

- `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing` = **Failed**
- `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` = **Failed**
- `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop` = **Passed**

This is the expected mixed red/green pre-fix baseline (1 passed, 2 failed). No `MessageBox.Show`
pop-up occurred during the run: the captured console output contains zero occurrences of the string
`MessageBox` (`grep -c "MessageBox"` against the log returned `0`), and no `_olApp`/live COM call was
reached by either failing test (both throw inside the `batchSize > 0` slice-and-project block, before
`SetupWorker`/`RunWorkerAsync` execute).
