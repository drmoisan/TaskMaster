# P2-T10 [expect-fail] — Red Run B (dismissal ownership)

Timestamp: 2026-08-28T15-47

Command (DR-1 runner resolution):

```
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcItemController_SearchDismissalTests|FullyQualifiedName~WireIntentEvents_SubscribesSearchLeave|FullyQualifiedName~UnwireIntentEvents_DetachesSearchLeave|FullyQualifiedName~ItemViewerSearchDismissalContractTests" /Logger:"trx;LogFileName=p2-t10.trx" "/ResultsDirectory:docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p2-t10"
```

EXIT_CODE: 1

ExpectedExitCode: 1

Output Summary:

- Total tests: **12**; Passed: **9**; Failed: **3**. `Test Run Failed.` Total time 1.2837 seconds.
- Exactly three tests failed, and they are exactly the three the plan predicts. Each failure is a
  Moq `Times.Once()` verification against the no-op seam:

1. `QuickFiler.Controllers.Tests.QfcItemController_SearchDismissalTests.TextBoxSearchKeyDown_EscapeWhileDropDownOpen_RoutesExactlyOneCloseIntent`
2. `QuickFiler.Controllers.Tests.QfcItemController_SearchDismissalTests.TextBoxSearchLeave_WhileDropDownOpen_RoutesExactlyOneCloseIntent`

   ```
   Moq.MockException:
   Expected invocation on the mock once, but was 0 times: v => v.SetFolderDroppedDown(False)

   Performed invocations:

      Mock<IItemViewer:5> (v):
      No invocations performed.
   ```

3. `QuickFiler.Controllers.Tests.QfcItemController_SearchDismissalTests.TextBoxSearchLeave_AfterDownArrowHandoff_SuppressesExactlyOneClose`

   ```
   Moq.MockException:
   Expected invocation on the mock once, but was 0 times: v => v.SetFolderDroppedDown(False)

   Performed invocations:

      Mock<IItemViewer:7> (v):

         IItemViewer.SetFolderDroppedDown(True)
         IItemViewer.FocusFolderDropDown()
   ```

   The recorded invocations show the Down-arrow gesture already behaves correctly; only the
   latch-consumption assertion fails against the no-op body.

- Zero other tests in the filtered run failed. The nine passing tests are:
  - `IItemViewer_DeclaresSearchLeaveAsPlainEventHandler`
  - `IItemViewer_DeclaresIsFolderDropDownOpenAsReadOnlyBool`
  - `IItemViewer_ExistingSearchAndDropDownMemberShapes_AreUnchanged`
  - `ItemViewer_ImplementsSearchLeaveAndIsFolderDropDownOpen`
  - `WireIntentEvents_SubscribesSearchLeave`
  - `UnwireIntentEvents_DetachesSearchLeave`
  - `TextBoxSearchKeyDown_EscapeWhileDropDownClosed_RoutesNoIntentAndLeavesKeyUnhandled`
  - `TextBoxSearchLeave_WhileDropDownClosed_RoutesNoIntent`
  - `TextBoxSearchKeyDown_DownArrow_StillOpensAndFocusesTheDropDown`

  The two wiring tests, the four contract tests, and dismissal tests 2/4/6 all pass, which proves
  the P2-T4/P2-T5 seam is behavior-preserving: it added declarations and a subscription, and changed
  no runtime behavior.

- TRX: the `p2-t10` results subdirectory holds exactly one TRX file, named exactly `p2-t10.trx`
  (DR-1). vstest additionally created an empty `Deploy_*` scaffold directory; it contains no files,
  is untracked by git, and is removed by P7-T3 because its directory name embeds the account and
  machine name.

Acceptance: satisfied — exactly the three predicted tests fail, each on a Moq `Times.Once()`
verification against the no-op seam, and zero other tests in the filtered run fail.
