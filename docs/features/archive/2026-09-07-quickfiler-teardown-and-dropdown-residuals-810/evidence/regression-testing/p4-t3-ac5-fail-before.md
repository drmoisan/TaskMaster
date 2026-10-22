# [P4-T3] AC5 Regression Cases — Fail-Before

Timestamp: 2026-09-08T10-01
Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\810-p4-t3' '/TestCaseFilter:FullyQualifiedName~RestoreAfterOpenFailure_WithStaleCommitPending_StillCancelsAndClearsLatch|FullyQualifiedName~NativeCloseWhileCommitPending_DoesNotCancelSelection'`, with `$vstest` re-bound per D3 from the two `vswhere` lines pinned by [P0-T7]
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: Both AC5 cases fail against the unmodified production code, which is the required fail-before observation for this `[expect-fail]` task. Two tests were selected, zero passed, two failed. vstest 18.x rejects `OR` inside a filter, so the two clauses are joined with a vertical bar.

TOTAL: 2
PASSED: 0
FAILED: 2

## Outcome per case

```
Failed RestoreAfterOpenFailure_WithStaleCommitPending_StillCancelsAndClearsLatch
Failed NativeCloseWhileCommitPending_DoesNotCancelSelection
```

FAILURE-REASON: in both cases the latch survived the close that consumed it. `FinishClose` at `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:432-453` reads `IsCommitPending` at `:447` to decide whether to cancel, and nothing in the operation list clears it afterwards.

- `NativeCloseWhileCommitPending_DoesNotCancelSelection` — after the native close that read and honoured the latch, `IsCommitPending` was still observed to be true where the appended AC5 assertion requires false.
- `RestoreAfterOpenFailure_WithStaleCommitPending_StillCancelsAndClearsLatch` — because the latch survived the earlier close, the later `RestoreAfterOpenFailure` still suppressed the cancel and `CancelCount` stayed 0 where 1 is required. The observed value was 0 against a required 1.

## Why the new case drives two closes

The latch read at `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:447` is an earlier operation of the same `CompleteAll` list than the clear D19 specifies, so the clear cannot make the cancel run on the call that read a stale latch. A single-close version of this case asserting a cancel would therefore be unsatisfiable even after the fix. The defect the fix closes is that the latch survives the close that consumed it, and only a second close can observe that.

## Expectation

This task is tagged `[expect-fail]`. A failing run is its required outcome and `ExpectedExitCode: 1` declares that expectation, so the observed `EXIT_CODE: 1` normalizes to a pass. The pass-after counterpart is [P4-T12].

## D13

No TRX content is reproduced here. The counters were parsed from the run's `ResultSummary/Counters` element, the outcome list from its `UnitTestResult` elements, and the two failure causes are stated in prose.
