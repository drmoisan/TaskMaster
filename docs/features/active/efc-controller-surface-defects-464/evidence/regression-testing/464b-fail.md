# [P6-T5] #464 B fail-before evidence — the five `async void` boundaries still rethrow

Timestamp: 2026-08-28T01-06
Task: [P6-T5] [expect-fail]
Command: `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow" "/Logger:trx;LogFileName=464b-fail.trx" /ResultsDirectory:docs\features\active\efc-controller-surface-defects-464\evidence\regression-testing\p6-t5` under `pwsh -NoProfile` from the worktree root
EXIT_CODE: 1
ExpectedExitCode: 1

## Preceding intermediate build

Command: `& "<resolved MSBuild.exe>" "QuickFiler.Test\QuickFiler.Test.csproj" /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo /v:m`
EXIT_CODE: 0

## Result — TRX `<Counters>`, verbatim

```
total="5" executed="5" passed="0" failed="5" error="0" timeout="0" aborted="0"
inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0"
warning="0" completed="0" inProgress="0" pending="0"
```

Total executed: **5** (non-zero, per the non-vacuity rule). Failed: **5**.

## Enumerated distinct row result names and outcomes

Under decision D9 each `[DataRow]` is a distinct named test result with its own name and outcome. The
five distinct result names are:

| # | Result name | Outcome |
|---|---|---|
| 1 | `AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow ("ButtonCancelClickAsync")` | **Failed** |
| 2 | `AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow ("ButtonOkClickAsync")` | **Failed** |
| 3 | `AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow ("ButtonRefreshClickAsync")` | **Failed** |
| 4 | `AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow ("ButtonCreateClickAsync")` | **Failed** |
| 5 | `AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow ("ButtonDeleteClickAsync")` | **Failed** |

Every row failed on the `NotThrowAsync` assertion: the awaited call threw rather than completing,
because each extracted member's `catch` still ends in `throw;`. That is the fail-before evidence that
each boundary still rethrows.

The fault is injected by the all-fields-null state itself. The test nulls the ambient
`SynchronizationContext` inside a `try` whose `finally` restores it, so the first statement each
extracted member reaches dereferences the null `_formViewer` (or awaits its null `UiSyncContext`) and
throws.

## Artifacts

TRX: `docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/p6-t5/464b-fail.trx`,
sanitised (worktree path to `<repo-root>`, account to `<user>`, machine to `<host>`). The `/InIsolation`
`Deploy_*` scratch tree written into that directory was deleted.

Output Summary: EXPECT-FAIL SATISFIED. 5 executed, 5 failed, EXIT_CODE 1 against ExpectedExitCode 1. All
five distinct `[DataRow]` result names are red because the extracted boundary members still carry the
`throw;` the defect-preserving extraction retained.
