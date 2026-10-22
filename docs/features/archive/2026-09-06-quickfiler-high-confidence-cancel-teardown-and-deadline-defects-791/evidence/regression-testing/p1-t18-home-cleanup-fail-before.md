# [P1-T18] [expect-fail] `QfcHomeControllerCleanupTests`, before the fix

Timestamp: 2026-09-06T14-47

Command:

```
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\791-p1t18' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:FullyQualifiedName~QfcHomeControllerCleanupTests'
```

`$vstest` was re-bound inside this command block by the two R10 resolution lines; the resolved value
reduced per R3 is `<program-files>\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.

ExpectedExitCode: 1
EXIT_CODE: 1

Output Summary: `Total tests: 2, Failed: 2. Test Run Failed. Total time: 1.7644 Seconds.` Both tests
in the class are red, which is the required outcome for this task.

FAIL-BEFORE-COUNT: 2

## Failing tests, by fully qualified name, with failure messages reduced per R3

Both names are in `QuickFiler.Controllers.Tests.QfcHomeControllerCleanupTests`.

1. `Cleanup_DatamodelCleanupThrows_StillInvokesParentCleanup`
   — `Did not expect any exception because a failing cleanup stage must be logged, not propagated,
   but found System.InvalidOperationException: datamodel cleanup failed`. `Cleanup()` calls
   `_datamodel.Cleanup()` unguarded at the top of the method, so the throw escapes and
   `ParentCleanup.Invoke()` on the last line never runs. This is the mechanism by which
   `RibbonController.ReleaseQuickFiler` is skipped and both ribbon buttons become no-ops.
2. `Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted`
   — `Expected a <System.ObjectDisposedException> to be thrown because the token source must be
   disposed during cleanup, but no exception was thrown.` Reading
   `CancellationTokenSource.Token` after `Cleanup()` succeeds, which proves the source was never
   disposed. The companion assertion on the viewer's `Worker` getter is not reached in this run
   because the disposal assertion fails first; it becomes the operative assertion once [P2-T12]
   supplies the disposal.

Both are the tests [P1-T18]'s acceptance names.
