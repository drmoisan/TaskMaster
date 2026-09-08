# [P2-T2] Test-Assembly Build (AC3 fail-before)

Timestamp: 2026-09-08T09-51
Command: `msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU` (the [P1-T2] command)
EXIT_CODE: 0
Output Summary: The rebuild carrying the two AC3 test changes made by [P2-T1] succeeded with zero warnings and zero errors. The assembly now contains the new case `Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource` and the second-pass assertion added to `Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted`, so [P2-T3] can run the fail-before.

Verbatim `Build succeeded.` block:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:12.91
```

## D5 file-lock check

The build output was searched for `MSB3061` and `MSB3021`. Match count: 0. The D5 stop condition did not fire and no process was terminated.
