# P4-T5 — AC2 fail-before run (expect-fail)

Timestamp: 2026-09-07T14-12
Task: [P4-T5] [expect-fail]
Issue: #796
Channel used: A

Command:

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~QfcFormControllerDeactivateTests" /ResultsDirectory:TestResults\796\p4-t5 "/Logger:trx;LogFileName=p4-t5.trx"; "EXIT_CODE=$LASTEXITCODE"'
```

EXIT_CODE: 1
ExpectedExitCode: 1

Raw results (gitignored, never committed): TestResults/796/p4-t5/p4-t5.trx

## Run summary

```
Total tests: 9
     Passed: 8
     Failed: 1
```

## Per-test results

| Test | Result |
|---|---|
| RegisterFormEventHandlers_SubscribesFormDeactivated | Passed |
| UnregisterFormEventHandlers_UnsubscribesFormDeactivated | Passed |
| FormDeactivated_WebView2Focused_ParksFocusOnce | Passed |
| FormDeactivated_NoWebView2Focus_DoesNotPark | Passed |
| FormDeactivated_CancelsSelectorOnEveryItemController | Passed |
| FormDeactivated_NullGroupsOrNullItemGroups_DoesNotThrow | Passed |
| FormDeactivated_ItemCancelThrows_DoesNotPropagateAndContinues | Passed |
| FormatDeactivationDiagnostics_IncludesEveryDiscriminatingField | Passed |
| FormDeactivated_SelfInflictedByOwnPopup_DoesNotCancelAnySelector | Failed |

## Carve-out

The single Failed test is exactly FormDeactivated_SelfInflictedByOwnPopup_DoesNotCancelAnySelector,
landed by task P4-T4 as a deliberately-failing regression test and made to pass by task P4-T8. Every
one of the other 8 tests in the class is recorded Passed. No test other than that exact named test
is Failed, so the gate's explicit single-name carve-out is satisfied exactly and not merely
non-vacuously.

## Failure detail (the assertion that matters)

The failure is a runtime Moq verification failure at the assertion, not a compile or arrange
failure:

```
Moq.MockException:
Expected invocation on the mock should never have been performed, but was 1 times: x => x.CancelBreadcrumbSelector()
```

That is the defect under repair: with the seam declared and implemented but not yet consulted by
`ParkFocusAndCancelSelectors`, a self-inflicted deactivation still cancels every item's selector.

Output Summary: 9 total, 8 passed, 1 failed; the one failure is the named expect-fail test and the
failure is the intended assertion.
