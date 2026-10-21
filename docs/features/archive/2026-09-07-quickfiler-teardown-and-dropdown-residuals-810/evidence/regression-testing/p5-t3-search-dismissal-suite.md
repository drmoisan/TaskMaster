# [P5-T3] Search-Dismissal Suite After the Accessor Deletion

Timestamp: 2026-09-08T10-10
Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\810-p5-t3' '/TestCaseFilter:FullyQualifiedName~SearchDismissalTests'`, with `$vstest` re-bound per D3 from the two `vswhere` lines pinned by [P0-T7]
EXIT_CODE: 0
Output Summary: All six cases in `QfcItemController.SearchDismissalTests` pass after the deletion of the dead `SearchOwnsDropDownDismissal` accessor. The suite is unaffected because it reaches the retained backing field by reflection on its name rather than through the deleted accessor.

TOTAL: 6
PASSED: 6
FAILED: 0

## Outcome per case

```
Passed TextBoxSearchKeyDown_EscapeWhileDropDownOpen_RoutesExactlyOneCloseIntent
Passed TextBoxSearchLeave_WhileDropDownClosed_RoutesNoIntent
Passed TextBoxSearchKeyDown_EscapeWhileDropDownClosed_RoutesNoIntentAndLeavesKeyUnhandled
Passed TextBoxSearchKeyDown_DownArrow_StillOpensAndFocusesTheDropDown
Passed TextBoxSearchLeave_WhileDropDownOpen_RoutesExactlyOneCloseIntent
Passed TextBoxSearchLeave_AfterDownArrowHandoff_SuppressesExactlyOneClose
```

`PASSED` equals `TOTAL` and is greater than zero.

## Why the deletion cannot break this suite

The `#796` AC4 re-pin test at `QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs:85` reads `_searchOwnedDismissal` by reflecting on the field's name. The deleted member was an expression-bodied accessor over that field, not the field itself, and [P5-T1] left the field and every one of its five write sites and one read site untouched. The reflection path is therefore unchanged.

The complementary static evidence is in [P5-T2]: a full-solution analyzer rebuild after the deletion returned zero warnings and zero errors, so no other call site depended on the accessor either.

## D13

No TRX content is reproduced here. The counters were parsed from the run's `ResultSummary/Counters` element and the outcome list from its `UnitTestResult` elements.
