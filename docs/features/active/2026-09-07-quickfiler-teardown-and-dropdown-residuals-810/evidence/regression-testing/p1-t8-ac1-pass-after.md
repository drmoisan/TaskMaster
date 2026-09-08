# [P1-T8] AC1 Regression Test — Pass-After

Timestamp: 2026-09-08T09-48
Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\810-p1-t8' '/TestCaseFilter:FullyQualifiedName~ActionCancelAsync_SelfInflictedByOwnPopup_StillCancelsEverySelector'`, with `$vstest` re-bound per D3 from the two `vswhere` lines pinned by [P0-T7]
EXIT_CODE: 0
Output Summary: The AC1 regression test passes against the fixed production code. One test was selected, one passed, none failed. Together with the [P1-T3] fail-before this is the fail-before / pass-after pair for AC1: the same test, unchanged between the two runs, failed before the production change and passes after it.

TOTAL: 1
PASSED: 1
FAILED: 0

## What changed between the two runs

The test file was not edited between [P1-T3] and this run. The three production edits are [P1-T4] (the `honourSelfInflictedGuard` parameter and the guard conjunct), [P1-T5] (the deactivation caller passing true) and [P1-T6] (the teardown call site passing false through an explicit lambda). The teardown path now cancels every breadcrumb selector whatever the state of `IsDeactivationSelfInflictedByOwnPopup`, while the `Form.Deactivate` path continues to honour the issue-677 contract.

## D13

No TRX content is reproduced here. The three counters above were parsed from the run's `ResultSummary/Counters` element.
