# [P0-T14] AC2 Fence Baseline

Timestamp: 2026-09-08T09-29
Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\810-p0-t14' '/TestCaseFilter:FullyQualifiedName~QfcFormControllerDeactivateTests'`, with `$vstest` re-bound per D3; `git diff --name-only origin/main -- QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs`; `git status --porcelain --untracked-files=all -- QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs`
EXIT_CODE: 0
Output Summary: All 9 cases in `QfcFormControllerDeactivateTests` passed, and the fence file is byte-unmodified relative to `origin/main` with no porcelain entry. Both named AC2 cases are among the passing set.

FENCE-TOTAL: 9
FENCE-PASSED: 9
FENCE-FAILED: 0
FENCE-DIFF-LINES: 0
FENCE-PORCELAIN-LINES: 0

## The two named cases

Both are reported as passed in this run:

- `FormDeactivated_SelfInflictedByOwnPopup_DoesNotCancelAnySelector` — passed
- `FormDeactivated_CancelsSelectorOnEveryItemController` — passed

## Full passing set

```
Passed RegisterFormEventHandlers_SubscribesFormDeactivated
Passed UnregisterFormEventHandlers_UnsubscribesFormDeactivated
Passed FormDeactivated_WebView2Focused_ParksFocusOnce
Passed FormDeactivated_NoWebView2Focus_DoesNotPark
Passed FormDeactivated_CancelsSelectorOnEveryItemController
Passed FormDeactivated_NullGroupsOrNullItemGroups_DoesNotThrow
Passed FormDeactivated_ItemCancelThrows_DoesNotPropagateAndContinues
Passed FormatDeactivationDiagnostics_IncludesEveryDiscriminatingField
Passed FormDeactivated_SelfInflictedByOwnPopup_DoesNotCancelAnySelector
```

## Why both git spans are present

The name-listing diff reports zero lines, which establishes that the tracked file is unmodified relative to `origin/main`. It cannot, however, report an untracked path, so the porcelain span is run beside it; it likewise reports zero lines. Together the two establish that the file is neither modified nor replaced by an untracked variant.

## D9 fence

`QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs` must stay byte-unmodified for the whole plan. Its tests remaining green while the file is unmodified is itself the AC2 evidence. The state recorded here is the reference [P1-T9] and [P7-T10] compare against.
