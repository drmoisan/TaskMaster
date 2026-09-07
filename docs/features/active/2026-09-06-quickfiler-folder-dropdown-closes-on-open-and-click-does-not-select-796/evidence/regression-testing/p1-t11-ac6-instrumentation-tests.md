# P1-T11 — AC6 instrumentation tests

Timestamp: 2026-09-07T14-25
Task: [P1-T11]
Issue: #796
Channel used: A

Command:

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbDropDownCloseOrderingTests|FullyQualifiedName~QfcFormControllerDeactivateTests" /ResultsDirectory:TestResults\796\p1-t11 "/Logger:trx;LogFileName=p1-t11.trx"; "EXIT_CODE=$LASTEXITCODE"'
```

EXIT_CODE: 0

## Result lines, verbatim

```
  Passed FormatDropDownClosedDiagnostics_IncludesEveryDiscriminatingField [50 ms]
  Passed RegisterFormEventHandlers_SubscribesFormDeactivated [282 ms]
  Passed UnregisterFormEventHandlers_UnsubscribesFormDeactivated [2 ms]
  Passed FormDeactivated_WebView2Focused_ParksFocusOnce [6 ms]
  Passed FormDeactivated_NoWebView2Focus_DoesNotPark [< 1 ms]
  Passed FormDeactivated_CancelsSelectorOnEveryItemController [19 ms]
  Passed FormDeactivated_NullGroupsOrNullItemGroups_DoesNotThrow [3 ms]
  Passed FormDeactivated_ItemCancelThrows_DoesNotPropagateAndContinues [1 ms]
  Passed FormatDeactivationDiagnostics_IncludesEveryDiscriminatingField [< 1 ms]

Test Run Successful.
Total tests: 9
     Passed: 9
 Total time: 1.6277 Seconds
```

## Acceptance

| Condition | Observed |
|---|---|
| `EXIT_CODE: 0` | 0 |
| FormatDropDownClosedDiagnostics_IncludesEveryDiscriminatingField Passed | Passed |
| FormatDeactivationDiagnostics_IncludesEveryDiscriminatingField Passed | Passed |
| Total recorded as 9 | 9 |
| No test recorded as Failed | none; the run printed no `Failed:` line and no `Failed` result line |

The Total of 9 is the 8 methods QfcFormControllerDeactivateTests holds after P1-T7
plus the 1 method BreadcrumbDropDownCloseOrderingTests holds after P1-T5.

No expect-fail test exists in either class at this point in plan order, so no
carve-out applies to this gate. The complete expect-fail inventory for this plan
lands its first entry at P4-T4, which is three phases later.

## Incidental observation

The six pre-existing QfcFormControllerDeactivateTests methods all pass unchanged
against the instrumented handler, including the two that assert focus-parking
behaviour and the two that assert null-safety and per-item exception containment.
That is direct evidence that the two added log statements changed no behaviour those
tests exercise.

## Raw output

The TRX is written to the gitignored path TestResults/796/p1-t11/p1-t11.trx and is
never committed.
