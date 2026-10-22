# P2-T3 [expect-fail] — Red Run A (host seam)

Timestamp: 2026-08-28T15-32

Command (DR-1 runner resolution; `$vstest` is resolved by
`& $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1`):

```
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbDropDownHostTests" /Logger:"trx;LogFileName=p2-t3.trx" "/ResultsDirectory:docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p2-t3"
```

EXIT_CODE: 1

ExpectedExitCode: 1

Output Summary:

- Total tests: **27**; Passed: **25**; Failed: **2**. `Test Run Failed.` Total time 1.4981 seconds.
- Exactly two tests failed, and they are exactly the two the plan predicts:

1. `QuickFiler.Test.Viewers.BreadcrumbDropDownHostTests.ShowPopup_NonFocusingOpen_RunsTheShowDelegateWithAutoCloseFalse`

   Verbatim FluentAssertions message:

   ```
   Expected observed to be equal to {False}, but {True} differs at index 0.
   ```

   Failing frame: `BreadcrumbDropDownHostTests.Part2.cs:line 238`.

2. `QuickFiler.Test.Viewers.BreadcrumbDropDownHostTests.ShowPopup_TwoConsecutiveNonFocusingOpens_ShowOnceWithAutoCloseFalse`

   Verbatim FluentAssertions message:

   ```
   Expected observed to be equal to {False}, but {True} differs at index 0.
   ```

   Failing frame: `BreadcrumbDropDownHostTests.Part2.cs:line 358`.

  Both messages show the observed `True` where `False` was expected — the defect this plan fixes:
  the popup is currently shown with `AutoClose == true`, which is what engages WinForms menu mode
  and retargets every keystroke away from the search textbox.

- Zero other tests in the filtered run failed. The four #680 control/guard tests
  (`ShowPopup_GestureOpen_RunsTheShowDelegateWithAutoCloseTrue`,
  `Close_AfterANonFocusingOpen_RestoresAutoCloseTrue`,
  `OpenAsync_TakeFocusReopenOnANonFocusingOpen_RestoresAutoCloseTrue`,
  `ShowPopup_GestureOpenAfterANonFocusingCycle_RunsTheShowDelegateWithAutoCloseTrue`) passed, as did
  every pre-existing `BreadcrumbDropDownHostTests` case.

- TRX: the `p2-t3` results subdirectory holds exactly one TRX file, named exactly `p2-t3.trx`
  (DR-1). vstest additionally created empty `Deploy_*/In/<host>` and `Deploy_*/Out` scaffold
  directories under the same results directory; they contain no files, are not tracked by git, and
  are removed by P7-T3 because their directory names embed the account and machine name.

Acceptance: satisfied — exactly the two predicted tests fail, each with the recorded verbatim
message showing observed `True`, and zero other tests in the filtered run fail.
