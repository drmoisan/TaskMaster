# Scoped Controller Regression Run (P4-T2)

Timestamp: 2026-08-28T16-04
Command (CR-VSTEST, fully expanded):

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~QfcFormControllerDeactivateTests|FullyQualifiedName~CancelBreadcrumbSelectorTests" /Logger:trx "/ResultsDirectory:docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/regression-testing/p4-t2"'
```

The results directory was deleted before the run so exactly one timestamp-named TRX can exist
under it.

EXIT_CODE: 0

## Output Summary

```
Test Run Successful.
Total tests: 9
     Passed: 9
 Total time: 1.4123 Seconds
```

Total / passed / failed triple: **9 / 9 / 0**. TRX `<Counters>`: `total="9" passed="9" failed="0"
notExecuted="0"`.

The filter selected exactly the nine tests authored by P1-T2 and P1-T3 and nothing else, all
passed:

Seven from `QfcFormControllerDeactivateTests` (P1-T2):

1. `RegisterFormEventHandlers_SubscribesFormDeactivated`
2. `UnregisterFormEventHandlers_UnsubscribesFormDeactivated`
3. `FormDeactivated_WebView2Focused_ParksFocusOnce`
4. `FormDeactivated_NoWebView2Focus_DoesNotPark`
5. `FormDeactivated_CancelsSelectorOnEveryItemController`
6. `FormDeactivated_NullGroupsOrNullItemGroups_DoesNotThrow`
7. `FormDeactivated_ItemCancelThrows_DoesNotPropagateAndContinues`

Two from `QfcItemControllerCancelBreadcrumbSelectorTests` (P1-T3):

8. `CancelBreadcrumbSelector_ForwardsToViewer`
9. `CancelBreadcrumbSelector_NullViewer_DoesNotThrow`

## TRX artifact

Exactly one TRX exists under
`docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/regression-testing/p4-t2/`:

- `p4-t2-controller-deactivate-and-cancel.trx`

Renamed from the vstest default name and sanitised in binary mode with case-insensitive
substitutions (34 applied) over the workspace-root prefix, user-profile prefix, host identifier and
account identifier, per the repository-wide "never embed absolute host paths" rule. Post-condition
sweeps (case-insensitive, fixed-string) return 0 hits for the account identifier and the host
identifier.
