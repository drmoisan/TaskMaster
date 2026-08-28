# [P6-T12] #464 E fail-before evidence — the plain rethrow resets the stack trace

Timestamp: 2026-08-28T01-09
Task: [P6-T12] [expect-fail]
Command: `& "<resolved vstest.console.exe>" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~ThrowInitializationFailure_PreservesOriginalStackTrace" "/Logger:trx;LogFileName=464e-fail.trx" /ResultsDirectory:docs\features\active\efc-controller-surface-defects-464\evidence\regression-testing\p6-t12` under `pwsh -NoProfile` from the worktree root
EXIT_CODE: 1
ExpectedExitCode: 1

## Preceding intermediate build

Command: `& "<resolved MSBuild.exe>" "QuickFiler.Test\QuickFiler.Test.csproj" /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo /v:m`
EXIT_CODE: 0

## Result — TRX `<Counters>`, verbatim

```
total="1" executed="1" passed="0" failed="1" error="0" timeout="0" aborted="0"
inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0"
warning="0" completed="0" inProgress="0" pending="0"
```

Total executed: **1** (non-zero, per the non-vacuity rule). Failed: **1**.

## Enumerated result name and outcome

| # | Result name | Outcome | Failure reason |
|---|---|---|---|
| 1 | `ThrowInitializationFailure_PreservesOriginalStackTrace` | **Failed** | **the plain rethrow reset the stack trace.** The rethrown exception's `StackTrace` no longer contains the originating frame `ThrowFromOriginatingHelper`; its topmost frame is `EfcItemController.ThrowInitializationFailure`, the rethrow site itself |

The test's arrange step asserts the originating frame **is** present before the rethrow, so the failure
is specifically the loss of that frame and not an absent frame to begin with. The rethrown instance is
the same object; only its trace was overwritten.

This is the fail-before evidence for #464 E: `throw (expression);` overwrites `StackTrace` with the
rethrow site, discarding the frames that identify where the WebView2 core initialization actually
failed.

## Artifacts

TRX: `docs/features/active/efc-controller-surface-defects-464/evidence/regression-testing/p6-t12/464e-fail.trx`,
sanitised (worktree path to `<repo-root>`, account to `<user>`, machine to `<host>`). The `/InIsolation`
`Deploy_*` scratch tree written into that directory was deleted.

Output Summary: EXPECT-FAIL SATISFIED. 1 executed, 1 failed, EXIT_CODE 1 against ExpectedExitCode 1. The
defect-preserving `ThrowInitializationFailure` reset the stack trace, losing the originating frame.
