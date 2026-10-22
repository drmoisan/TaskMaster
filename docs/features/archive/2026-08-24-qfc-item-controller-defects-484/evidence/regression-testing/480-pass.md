# Issue #480 — Both Exact-Count Tests Pass After the Fix

Timestamp: 2026-08-26T08-54
Task: [P1-T6]

## Build step

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved MSBuild):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU
```

EXIT_CODE: 0 (3 warnings, 0 errors). Not an analyzer or nullable gate (decision D2).

## Test step

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved `vstest.console.exe`):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~ToggleNavigation" "/Logger:trx;LogFileName=480-pass.trx" /ResultsDirectory:docs\features\active\qfc-item-controller-defects-484\evidence\regression-testing\480-pass
```

EXIT_CODE: **0**

## Counts

| Metric | Value |
|---|---|
| Total | 4 |
| Passed | 4 |
| **Failed** | **0** |

```
Test Run Successful.
Total tests: 4
     Passed: 4
```

## Result rows

| Test | Outcome |
|---|---|
| `QfcItemController_FocusAndThemeTests.ToggleNavigation_Synchronous_TogglesPositionTips` | **Passed** |
| `QfcItemController_MailActionsTests.ToggleNavigation_Asynchronous_TogglesPositionTipsExactlyOnce` | **Passed** |
| `QfcItemController_FocusAndThemeTests.ToggleNavigation_WithState_TogglesPositionTipsWithState` | Passed |
| `QfcItemController_FocusAndThemeTests.ToggleNavigationAsync_AwaitsPositionTipsToggleAsync` | Passed |

Both tests the acceptance text names are recorded with outcome `Passed`. The two sibling
`ToggleNavigation` overload tests that the filter also selected remain green, confirming the deletion did
not disturb the two-argument overload or the async overload.

## Interpretation

`[P1-T5]` deleted the unconditional
`_itemViewer.BeginInvoke(new System.Action(() => _itemPositionTips.Toggle(false)));` statement that
preceded the `if (async)` branch in `ToggleNavigation(bool async)`. The method now contains exactly two
dispatch statements, one per branch, so each branch produces exactly one `Toggle(false)` invocation and
both `Times.Once()` assertions hold.

TRX:
`docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/480-pass/480-pass.trx`

Output Summary: `EXIT_CODE: 0`, 4 total, 4 passed, 0 failed. Both
`ToggleNavigation_Synchronous_TogglesPositionTips` and
`ToggleNavigation_Asynchronous_TogglesPositionTipsExactlyOnce` pass after the fix.
