# P3-T9 — Green Run B (dismissal ownership, host suite, and the #438 controller suite)

Timestamp: 2026-08-28T16-02

Command (DR-1 runner resolution; the exact P2-T10 filter plus the host and #438 controller suites):

```
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcItemController_SearchDismissalTests|FullyQualifiedName~WireIntentEvents_SubscribesSearchLeave|FullyQualifiedName~UnwireIntentEvents_DetachesSearchLeave|FullyQualifiedName~ItemViewerSearchDismissalContractTests|FullyQualifiedName~BreadcrumbDropDownHostTests|FullyQualifiedName~QfcItemController_SearchFocusRegressionTests" /Logger:"trx;LogFileName=p3-t9.trx" "/ResultsDirectory:docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p3-t9"
```

EXIT_CODE: 0

Output Summary:

- `Test Run Successful.` Total tests: **47**; Passed: **47**; Failed: **0**. Total time 1.7087 seconds.
- The three tests that were red in P2-T10 now pass:
  - `TextBoxSearchKeyDown_EscapeWhileDropDownOpen_RoutesExactlyOneCloseIntent`
  - `TextBoxSearchLeave_WhileDropDownOpen_RoutesExactlyOneCloseIntent`
  - `TextBoxSearchLeave_AfterDownArrowHandoff_SuppressesExactlyOneClose`
- `QfcItemController_SearchFocusRegressionTests` (#438) is green and byte-unmodified. It pins
  `SetFolderDroppedDown` `Times.Never()` on the TextChanged path only, which the new KeyDown and
  Leave intents do not touch.
- The full `BreadcrumbDropDownHostTests` suite (27 tests, including the six #680 host-seam tests) is
  green, consistent with P3-T6.
- TRX: the `p3-t9` results subdirectory holds exactly one file, named exactly `p3-t9.trx` (DR-1).

Acceptance: satisfied — `EXIT_CODE: 0` and zero failures.
