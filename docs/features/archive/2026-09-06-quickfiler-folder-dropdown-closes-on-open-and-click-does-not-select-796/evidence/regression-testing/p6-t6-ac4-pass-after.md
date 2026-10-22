# P6-T6 — AC4 pass-after run

Timestamp: 2026-09-07T14-43
Task: [P6-T6]
Issue: #796
Channel used: A

Command:

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~QfcItemController_SearchLeaveLatchTests|FullyQualifiedName~QfcItemController_EventHandlersTests" /ResultsDirectory:TestResults\796\p6-t6 "/Logger:trx;LogFileName=p6-t6.trx"; "EXIT_CODE=$LASTEXITCODE"'
```

EXIT_CODE: 0

Raw results (gitignored, never committed): TestResults/796/p6-t6/p6-t6.trx

## Run summary

```
Test Run Successful.
Total tests: 18
     Passed: 18
```

No test in either class is recorded as Failed. The 18 are the 2 the new
QfcItemController_SearchLeaveLatchTests declares plus the 16 QfcItemController_EventHandlersTests
declares; the split was derived by attributing each recorded per-test result line to its declaring
class, because vstest.console.exe prints one combined Total for a multi-class filter.

## Fail-before to pass-after transition

| Test | P6-T4 (fail-before) | P6-T6 (pass-after) |
|---|---|---|
| SearchLeaveAfterMouseDrivenOpen_DoesNotCloseDropDown | Failed | Passed |
| SearchLeaveAfterSearchDrivenOpen_ClosesDropDown | Passed | Passed |

The only expect-fail test in these classes was landed by P6-T2 and made to pass by P6-T5, both of
which precede this gate.

## The issue #680 contract, still pinned

Three pre-existing tests exercise the paths the AC4 change touches, and all three are Passed:

| Test | Result | What it pins |
|---|---|---|
| TextBoxSearch_KeyDown_WhenDownArrow_DropsDownAndFocusesFolder | Passed | the Down-arrow open still drops down and focuses |
| TextBoxSearch_KeyDown_WhenNotDownArrow_DoesNothing | Passed | a non-Down key still falls through untouched |
| TextBoxSearch_TextChanged_UsesInjectedFolderSearchHandler_PresentsSearchResultsWithoutFocusOrCommit | Passed | the search-typing path still presents results with no focus transfer and no committed selection |

The third is the one that could have detected an unintended viewer call added to
`TextBoxSearch_TextChanged`, because it asserts `SetFolderDroppedDown` is never called there. It is
Passed, so the producer this task added to that handler writes only the private latch.

## File-size measurement required by this task

Command:

```
pwsh -NoProfile -Command '(Get-Content -LiteralPath QuickFiler\Controllers\QfcItemController.EventHandlers.cs).Count'
```

EXIT_CODE: 0

LINE-COUNT-IDIOM: (Get-Content -LiteralPath $_).Count

| Path | Baseline | Measured | Ceiling | Verdict |
|---|---|---|---|---|
| QuickFiler/Controllers/QfcItemController.EventHandlers.cs | 263 | 317 | 500 | within |

## Analyzer state after the fix

The transient CS0649 recorded at tasks P6-T1 and P6-T3 against the not-yet-assigned
`_searchOwnedDismissal` field has cleared: the rebuild at TestResults/796/p6-t5/analyzer-rebuild.log
(gitignored) reports 0 Warning(s) and 0 Error(s), which is the baseline warning total.

Output Summary: 18 total, 18 passed, 0 failed; the AC4 expect-fail test transitioned Failed to
Passed; the #680 contract tests still pass; the handler file measures 317 lines against 500.
