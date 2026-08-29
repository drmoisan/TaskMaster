# P3-T6 — Green Run A (host seam)

Timestamp: 2026-08-28T15-57

Command (DR-1 runner resolution):

```
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbDropDownHostTests" /Logger:"trx;LogFileName=p3-t6.trx" "/ResultsDirectory:docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p3-t6"
```

EXIT_CODE: 0

Output Summary:

- `Test Run Successful.` Total tests: **27**; Passed: **27**; Failed: **0**. Total time 1.6094 seconds.
- The run total of 27 equals P2-T3's total of 27 — same filter, same population. The two formerly-red
  tests now pass and no test was added, removed, or filtered out between the two runs:
  - `ShowPopup_NonFocusingOpen_RunsTheShowDelegateWithAutoCloseFalse` — **Passed** (was Failed in P2-T3)
  - `ShowPopup_TwoConsecutiveNonFocusingOpens_ShowOnceWithAutoCloseFalse` — **Passed** (was Failed in P2-T3)
- The four #680 control/guard tests remain green and are now non-vacuous:
  - `ShowPopup_GestureOpen_RunsTheShowDelegateWithAutoCloseTrue`
  - `Close_AfterANonFocusingOpen_RestoresAutoCloseTrue`
  - `OpenAsync_TakeFocusReopenOnANonFocusingOpen_RestoresAutoCloseTrue`
  - `ShowPopup_GestureOpenAfterANonFocusingCycle_RunsTheShowDelegateWithAutoCloseTrue`
- Every pre-existing `BreadcrumbDropDownHostTests` case (including the #438 Part2 suite) is green.
- TRX: the `p3-t6` results subdirectory holds exactly one file, named exactly `p3-t6.trx` (DR-1).

Acceptance: satisfied — `EXIT_CODE: 0`, zero failures, and the run total equals P2-T3's total.
