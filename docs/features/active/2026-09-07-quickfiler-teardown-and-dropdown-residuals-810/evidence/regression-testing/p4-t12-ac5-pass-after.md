# [P4-T12] AC5 Pass-After Across the Three Breadcrumb Host Classes

Timestamp: 2026-09-08T10-08
Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\810-p4-t12' '/TestCaseFilter:FullyQualifiedName~BreadcrumbDropDownCloseOrderingTests|FullyQualifiedName~BreadcrumbDropDownHostTests|FullyQualifiedName~BreadcrumbPendingOpenCloseTests'`, with `$vstest` re-bound per D3 from the two `vswhere` lines pinned by [P0-T7]
EXIT_CODE: 0
Output Summary: All 48 cases across `BreadcrumbDropDownCloseOrderingTests`, `BreadcrumbDropDownHostTests` and `BreadcrumbPendingOpenCloseTests` pass. The two cases that failed in [P4-T3] now pass, and the 46 pre-existing cases are unaffected by both the AC5 latch clear and the [P4-T8] relocation of `FinishClose` and `RestoreAfterOpenFailure` to the sibling partial-class part.

TOTAL: 48
PASSED: 48
FAILED: 0

## The three cases the task names, and one more

```
Passed RestoreAfterOpenFailure_WithStaleCommitPending_StillCancelsAndClearsLatch
Passed NativeCloseWhileCommitPending_DoesNotCancelSelection
Passed NativeCloseWithNoCommitPending_StillCancelsSelection
Passed CloseWhilePendingOpenAndCommitPending_DoesNotCancelSelection
```

The first two are the AC5 pair that failed in [P4-T3]. The third is the scoping half of the issue-796 pair and its passing is what establishes that the AC5 clear did not make the suppression global: a close with no commit in flight still cancels. The fourth is listed because it is the pending-open sibling of the same latch behaviour and it also passes, so the clear did not disturb the pending-open path.

## What changed between the two runs

The test file was not edited between [P4-T3] and this run. The production changes are [P4-T4], which added `() => IsCommitPending = false` as the fourth operation of `FinishClose`'s `CompleteAll` list and replaced the superseded issue-677 comment line, [P4-T5], which corrected the latch-lifetime XML doc, and [P4-T8], which relocated `FinishClose` and `RestoreAfterOpenFailure` verbatim into `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs`.

Together with [P4-T3] this is the fail-before / pass-after pair for AC5.

## D13

No TRX content is reproduced here. The counters were parsed from the run's `ResultSummary/Counters` element and the outcome list from its `UnitTestResult` elements.
