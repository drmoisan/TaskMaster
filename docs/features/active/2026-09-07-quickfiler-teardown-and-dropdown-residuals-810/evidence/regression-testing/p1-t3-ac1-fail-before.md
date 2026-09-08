# [P1-T3] AC1 Regression Test — Fail-Before

Timestamp: 2026-09-08T09-45
Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\810-p1-t3' '/TestCaseFilter:FullyQualifiedName~ActionCancelAsync_SelfInflictedByOwnPopup_StillCancelsEverySelector'`, with `$vstest` re-bound per D3 from the two `vswhere` lines pinned by [P0-T7]
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: The AC1 regression test added by [P1-T1] fails against the unmodified production code, which is the required fail-before observation for this `[expect-fail]` task. One test was selected, zero passed, one failed. The failure is the Moq verification of `CancelBreadcrumbSelector`, which was never invoked.

TOTAL: 1
PASSED: 0
FAILED: 1

FAILURE-REASON: `CancelBreadcrumbSelector` was invoked zero times on the item-controller mocks. The test arranges `IsDeactivationSelfInflictedByOwnPopup` to return true, and the guard at `QuickFiler/Controllers/QfcFormController.Deactivate.cs:118` therefore returned from `ParkFocusAndCancelSelectors` before reaching the per-item loop, so no selector was cancelled. This is the defect AC1 describes: the issue-677 self-inflicted-deactivation guard, which is meaningful only for a genuine `Form.Deactivate`, is also consulted on the teardown path, where it suppresses cancellation that must always happen.

## Expectation

This task is tagged `[expect-fail]`. A failing run is its required outcome and `ExpectedExitCode: 1` declares that expectation, so the observed `EXIT_CODE: 1` normalizes to a pass. The pass-after counterpart is [P1-T8].

## D13

No TRX content is reproduced here. The three counters above were parsed from the run's `ResultSummary/Counters` element and the failure reason is stated in prose.
