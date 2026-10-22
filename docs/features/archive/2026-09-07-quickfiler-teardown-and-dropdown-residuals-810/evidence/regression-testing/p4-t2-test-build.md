# [P4-T2] Test-Assembly Build (AC5 fail-before)

Timestamp: 2026-09-08T10-00
Command: `msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU` (the [P1-T2] command)
EXIT_CODE: 0
Output Summary: The rebuild carrying the two AC5 test changes made by [P4-T1] succeeded with zero warnings and zero errors. The assembly now contains the new case `RestoreAfterOpenFailure_WithStaleCommitPending_StillCancelsAndClearsLatch` and the latch assertion appended to `NativeCloseWhileCommitPending_DoesNotCancelSelection`, so [P4-T3] can run the fail-before over both.

Verbatim `Build succeeded.` block:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:14.33
```

## D5 file-lock check

The build output was searched for `MSB3061` and `MSB3021`. Match count: 0. The D5 stop condition did not fire and no process was terminated.
