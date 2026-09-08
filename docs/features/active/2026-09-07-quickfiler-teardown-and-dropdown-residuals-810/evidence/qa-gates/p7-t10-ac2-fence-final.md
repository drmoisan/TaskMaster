# [P7-T10] Final AC2 Fence State

Timestamp: 2026-09-08T10-28
Command: `git diff --name-only origin/main -- QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs`; `git status --porcelain --untracked-files=all -- QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs`; and `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\810-p7-t10' '/TestCaseFilter:FullyQualifiedName~QfcFormControllerDeactivateTests'`, the [P0-T14] scoped test command, with `$vstest` re-bound per D3
EXIT_CODE: 0
Output Summary: The AC2 fence holds at the end of the plan. `QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs` is byte-unmodified relative to `origin/main` with no porcelain entry, and all 9 of its cases pass, matching the `FENCE-PASSED: 9` recorded by [P0-T14].

FENCE-TOTAL: 9
FENCE-PASSED: 9
FENCE-FAILED: 0
FENCE-DIFF-LINES: 0
FENCE-PORCELAIN-LINES: 0

`FENCE-PASSED` equals the [P0-T14] value of 9.

## The two named cases

Both are recorded as passed in this run:

- `FormDeactivated_SelfInflictedByOwnPopup_DoesNotCancelAnySelector` — passed
- `FormDeactivated_CancelsSelectorOnEveryItemController` — passed

## Full passing set

```
Passed FormDeactivated_CancelsSelectorOnEveryItemController
Passed FormatDeactivationDiagnostics_IncludesEveryDiscriminatingField
Passed UnregisterFormEventHandlers_UnsubscribesFormDeactivated
Passed FormDeactivated_NullGroupsOrNullItemGroups_DoesNotThrow
Passed FormDeactivated_SelfInflictedByOwnPopup_DoesNotCancelAnySelector
Passed FormDeactivated_NoWebView2Focus_DoesNotPark
Passed RegisterFormEventHandlers_SubscribesFormDeactivated
Passed FormDeactivated_ItemCancelThrows_DoesNotPropagateAndContinues
Passed FormDeactivated_WebView2Focused_ParksFocusOnce
```

## Why this is the AC2 evidence

D9 requires this file to stay byte-unmodified for the whole plan, and its tests remaining green while the file is unmodified is itself the AC2 evidence. Both halves hold here at the end of the plan, as they did at [P0-T14] before any edit and at [P1-T9] after the AC1 change.

The substance is that the issue-677 contract on the genuine `Form.Deactivate` path survived every change made here. `FormDeactivated_SelfInflictedByOwnPopup_DoesNotCancelAnySelector` still passes because [P1-T5] made the deactivation caller pass `honourSelfInflictedGuard: true`, so the guard still suppresses cancellation for a deactivation this form's own breadcrumb popup caused. `FormDeactivated_CancelsSelectorOnEveryItemController` still passes because a deactivation that is not self-inflicted still cancels every selector. Neither outcome was reached by editing the tests that assert them.

## Both git spans

The name-listing diff reports zero lines, which establishes that the tracked file is unchanged relative to `origin/main`. It cannot report an untracked path, so the porcelain span is run beside it; it likewise reports zero lines. Together the two establish that the file is neither modified nor replaced by an untracked variant.

## D13

No TRX content is reproduced here. The counters were parsed from the run's `ResultSummary/Counters` element and the outcome list from its `UnitTestResult` elements.
