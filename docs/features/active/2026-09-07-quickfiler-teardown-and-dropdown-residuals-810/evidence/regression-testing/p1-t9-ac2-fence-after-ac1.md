# [P1-T9] AC2 Fence After the AC1 Change

Timestamp: 2026-09-08T09-48
Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\810-p1-t9' '/TestCaseFilter:FullyQualifiedName~QfcFormControllerDeactivateTests'`, with `$vstest` re-bound per D3; `git diff --name-only origin/main -- QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs`; `git status --porcelain --untracked-files=all -- QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs`
EXIT_CODE: 0
Output Summary: All 9 cases in `QfcFormControllerDeactivateTests` still pass after the AC1 production change, matching the `FENCE-PASSED: 9` recorded by [P0-T14], and the fence file remains byte-unmodified relative to `origin/main` with no porcelain entry. The issue-677 contract on the genuine `Form.Deactivate` path is therefore intact.

FENCE-TOTAL: 9
FENCE-PASSED: 9
FENCE-FAILED: 0
FENCE-DIFF-LINES: 0
FENCE-PORCELAIN-LINES: 0

## Comparison against the baseline

`FENCE-PASSED` equals `FENCE-TOTAL` and equals the `FENCE-PASSED` value of 9 recorded in [P0-T14]. No case was added, removed or renamed.

## The AC2 case specifically

`FormDeactivated_SelfInflictedByOwnPopup_DoesNotCancelAnySelector` passed. That case drives the `Form.Deactivate` path, which [P1-T5] made pass `honourSelfInflictedGuard: true`, so the guard still suppresses cancellation for a deactivation this form's own breadcrumb popup caused. `FormDeactivated_CancelsSelectorOnEveryItemController` also passed, so a deactivation that is not self-inflicted still cancels every selector.

## Full passing set

```
Passed RegisterFormEventHandlers_SubscribesFormDeactivated
Passed FormatDeactivationDiagnostics_IncludesEveryDiscriminatingField
Passed UnregisterFormEventHandlers_UnsubscribesFormDeactivated
Passed FormDeactivated_CancelsSelectorOnEveryItemController
Passed FormDeactivated_ItemCancelThrows_DoesNotPropagateAndContinues
Passed FormDeactivated_NoWebView2Focus_DoesNotPark
Passed FormDeactivated_NullGroupsOrNullItemGroups_DoesNotThrow
Passed FormDeactivated_SelfInflictedByOwnPopup_DoesNotCancelAnySelector
Passed FormDeactivated_WebView2Focused_ParksFocusOnce
```

## D9 fence

The file is unmodified and untouched. Both git spans report zero lines: the name-listing diff establishes the tracked file is unchanged relative to `origin/main`, and the porcelain span establishes there is no untracked replacement it could not see.

## D13

No TRX content is reproduced here. The counters were parsed from the run's `ResultSummary/Counters` element and the outcome list from the run's `UnitTestResult` elements.
