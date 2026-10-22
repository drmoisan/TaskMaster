# [P3-T2] Test-Assembly Build (AC4 fail-before)

Timestamp: 2026-09-08T09-56
Command: `msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU` (the [P1-T2] command)
EXIT_CODE: 0
Output Summary: The rebuild carrying the AC4 regression test added by [P3-T1] succeeded with zero warnings and zero errors. The assembly now contains `Cleanup_ViewerDisposeThrows_StillInvokesParentCleanupOnce`, so [P3-T3] can run the fail-before.

Verbatim `Build succeeded.` block:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:13.35
```

## D5 file-lock check

The build output was searched for `MSB3061` and `MSB3021`. Match count: 0. The D5 stop condition did not fire and no process was terminated.
