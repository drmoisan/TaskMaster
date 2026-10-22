# P4-T9 — AC2 pass-after run

Timestamp: 2026-09-07T14-16
Task: [P4-T9]
Issue: #796
Channel used: A

Command:

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~QfcFormControllerDeactivateTests" /ResultsDirectory:TestResults\796\p4-t9 "/Logger:trx;LogFileName=p4-t9.trx"; "EXIT_CODE=$LASTEXITCODE"'
```

EXIT_CODE: 0

Raw results (gitignored, never committed): TestResults/796/p4-t9/p4-t9.trx

## Run summary

```
Test Run Successful.
Total tests: 9
     Passed: 9
```

vstest.console.exe prints no `Failed:` line on a successful run, so the Failed count is recorded as
0 with the note NOT PRINTED ON A PASSING RUN. The run additionally prints no `Skipped:` line, and
Total minus Passed is 0, so no test was skipped.

## Per-test results

| Test | P4-T5 (fail-before) | P4-T9 (pass-after) |
|---|---|---|
| RegisterFormEventHandlers_SubscribesFormDeactivated | Passed | Passed |
| UnregisterFormEventHandlers_UnsubscribesFormDeactivated | Passed | Passed |
| FormDeactivated_WebView2Focused_ParksFocusOnce | Passed | Passed |
| FormDeactivated_NoWebView2Focus_DoesNotPark | Passed | Passed |
| FormDeactivated_CancelsSelectorOnEveryItemController | Passed | Passed |
| FormDeactivated_NullGroupsOrNullItemGroups_DoesNotThrow | Passed | Passed |
| FormDeactivated_ItemCancelThrows_DoesNotPropagateAndContinues | Passed | Passed |
| FormatDeactivationDiagnostics_IncludesEveryDiscriminatingField | Passed | Passed |
| FormDeactivated_SelfInflictedByOwnPopup_DoesNotCancelAnySelector | Failed | Passed |

FormDeactivated_CancelsSelectorOnEveryItemController, which pins the issue #677 contract with two
`Times.Once()` assertions, is Passed on both runs, so the guard did not become global.

Output Summary: 9 total, 9 passed, 0 failed; the AC2 expect-fail test transitioned Failed to Passed
and the #677 contract test stayed Passed.
