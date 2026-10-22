# Finding 2 — undo-queue disposal ordering, passing run after the fix

Timestamp: 2026-09-03T14-19

Task: [P2-T5]
Issue: #731

## Command

1. Rebuild:

```
msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

MSBuild executable actually invoked: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`, recorded in full under the Evidence path-hygiene rule's stated exception for an external build-tool executable.

2. The same filtered test run as [P2-T3]:

```
<vstest> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcFormControllerCleanupTests"
```

vstest console: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe` (VSTest version 18.9.0, x64).

EXIT_CODE: 0

## Output Summary

Build summary lines, as observed:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

Test run output, as observed:

```
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
  Passed Cleanup_WithRunningConsumer_ConsumerReachesRanToCompletion [221 ms]
  Passed Cleanup_WithRunningConsumer_CompletesAddingBeforeDisposing [3 ms]
  Passed Cleanup_WithNullConsumerTask_DisposesQueueAndDoesNotThrow [3 ms]
  Passed Cleanup_WithParkedConsumer_ReturnsWithoutWaiting [< 1 ms]
  Passed Cleanup_CalledTwice_DoesNotThrow [< 1 ms]
  Passed Cleanup_WithFaultedConsumer_ObservesAndLogsTheFault [2 ms]
  Passed Cleanup_SourceContainsNoSynchronousWait [1 ms]

Test Run Successful.
Total tests: 7
     Passed: 7
 Total time: 1.4502 Seconds
```

- Total tests: **7**
- Passed: **7**
- Failed: **0**

The three tests that failed in `EVIDENCE/regression-testing/finding2-cleanup-fail-before.md` — `Cleanup_WithRunningConsumer_ConsumerReachesRanToCompletion`, `Cleanup_WithRunningConsumer_CompletesAddingBeforeDisposing` and `Cleanup_WithFaultedConsumer_ObservesAndLogsTheFault` — now pass, and the four forward guards that passed before continue to pass.

`Cleanup_SourceContainsNoSynchronousWait` and `Cleanup_WithParkedConsumer_ReturnsWithoutWaiting` are the two results that evidence AC6: the first confirms that `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` contains none of the four literals `.Wait(`, `.Result`, `Thread.Sleep` and `Task.Delay` after the fix, and the second confirms at run time that `Cleanup()` returns while the consumer is still parked, so the teardown path did not block on it.

## Frozen file check

Task: [P2-T6]

Commands:

```
git diff --name-only 35583f7c7e1f1c9b97e4f6f1e7846a3f2693c17e -- QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs
(Get-Content -LiteralPath 'QuickFiler.Test\Controllers\QfcFormControllerSeamTests.cs').Count
```

The `<DIFF-BASE>` operand is the value recorded on the `Diff base:` line of `EVIDENCE/baseline/tree-invariants.md` by [P0-T2], substituted verbatim. The literal ref `origin/main` was not used, because `HEAD` is a merge commit whose second parent is that remote ref and an advance of it mid-run would silently change what this gate measures.

EXIT_CODE: 0

Results:

- Anchored `git diff --name-only` output: **zero lines**. The frozen file is unchanged against the diff base.
- Recorded line count: **496**, which equals the value `[P0-T3]` recorded for that path in `EVIDENCE/baseline/tree-invariants.md`.

Rule G8b does not apply to this gate: the path is tracked and pre-existing, so the untracked-file blindness that rule detects is unreachable, and the assertion here is that the diff is empty rather than that it lists a newly created file.
