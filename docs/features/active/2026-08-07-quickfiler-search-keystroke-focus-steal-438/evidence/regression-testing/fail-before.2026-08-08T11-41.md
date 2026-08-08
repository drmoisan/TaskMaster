# [P1-T2] Fail-Before Regression Run (expect-fail)

- **Issue:** #438
- **Task:** [P1-T2] `[expect-fail]`
- **Timestamp:** 2026-08-08T11-41
- **Tree state:** HEAD `904b4c38dba0f9f41707c3c0f077e123c78de59c`; the only source change on disk is the NEW test file `QuickFiler.Test/Controllers/QfcItemController.SearchFocusRegressionTests.cs` plus its `<Compile Include>` entry. **Zero production code has been modified.**

## Command

`pwsh -NoProfile -Command "& 'C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:\"FullyQualifiedName~QfcItemController_SearchFocusRegressionTests\" ; exit $LASTEXITCODE"`

- **EXIT_CODE:** 1 (expected — this is the fail-before observation)

### Filter deviation (D10)

The plan's literal filter is `FullyQualifiedName~QfcItemController.SearchFocusRegressionTests` (with a dot). vstest's `~` operator is a plain case-insensitive **substring** match, and an MSTest fully-qualified name uses `Namespace.Type.Method`, so no FQN can contain a dot between the `QfcItemController` and `SearchFocusRegressionTests` segments of a single type name. Verified empirically against the pre-existing class:

```
/TestCaseFilter:"FullyQualifiedName~QfcItemController.EventHandlersTests"
  -> "No test matches the given testcase filter ..."  EXIT_CODE 0
/TestCaseFilter:"FullyQualifiedName~QfcItemController_EventHandlersTests"
  -> Total tests: 16, Passed: 16               EXIT_CODE 0
```

The dotted form selects **zero tests and still exits 0** — precisely the vacuous-pass hazard the plan's Environment Warning 1 forbids. The filter is therefore executed in its underscore form, matching the repository's existing test-class naming convention (`QfcItemController_EventHandlersTests`). This is a command-string correction of the same kind as Decisions Record D1; no acceptance criterion, scope boundary, or assertion is affected. The same correction applies to the `QfcItemController.*Tests` clauses in P5-T2 and P5-T5.

## Result

```
Total tests: 5
     Passed: 1
     Failed: 4
```

| Test | Pre-fix outcome |
|---|---|
| `TextBoxSearch_TextChanged_NeverRequestsADropDownStateChange` | **Failed** |
| `TextBoxSearch_TextChanged_NeverFocusesTheFolderDropDown` | Passed |
| `TextBoxSearch_TextChanged_NeverCommitsAFolderSelection` | **Failed** |
| `TextBoxSearch_TextChanged_SingleResult_StillTransfersNoFocusAndCommitsNothing` | **Failed** |
| `TextBoxSearch_TextChanged_EmptyResult_TransfersNoFocusAndDoesNotThrow` | **Failed** |

## Captured failure output

```
Test method ...QfcItemController_SearchFocusRegressionTests.TextBoxSearch_TextChanged_NeverRequestsADropDownStateChange threw exception:
Moq.MockException:
Expected invocation on the mock should never have been performed, but was 1 times: v => v.SetFolderDroppedDown(It.IsAny<bool>())

Test method ...TextBoxSearch_TextChanged_NeverCommitsAFolderSelection threw exception:
Moq.MockException:
Expected invocation on the mock should never have been performed, but was 1 times: v => v.SetFolderSelectedIndex(It.IsAny<int>())

Test method ...TextBoxSearch_TextChanged_SingleResult_StillTransfersNoFocusAndCommitsNothing threw exception:
Moq.MockException:
Expected invocation on the mock should never have been performed, but was 1 times: v => v.SetFolderDroppedDown(It.IsAny<bool>())

Test method ...TextBoxSearch_TextChanged_EmptyResult_TransfersNoFocusAndDoesNotThrow threw exception:
Moq.MockException:
Expected invocation on the mock should never have been performed, but was 1 times: v => v.SetFolderDroppedDown(It.IsAny<bool>())
```

## Attribution to the defect

The failures reproduce the defective composition exactly as cited in research §1.1 and spec § Root Cause Analysis:

- `QuickFiler/Controllers/QfcItemController.EventHandlers.cs:177` — `_itemViewer.SetFolderDroppedDown(true);` runs unconditionally on every keystroke, including for empty and single-row result sets. This is the open-side focus steal.
- `QuickFiler/Controllers/QfcItemController.EventHandlers.cs:175-176` — `if (folders.Length >= 2) _itemViewer.SetFolderSelectedIndex(1);` mutates the committed model selection per keystroke.

`TextBoxSearch_TextChanged_NeverFocusesTheFolderDropDown` passes before the fix because the handler never calls `FocusFolderDropDown()` directly; that focus transfer occurs downstream in the open pipeline (`BreadcrumbDropDownOpenLifetime.FocusCurrentSurface` -> `_host.FocusPending()`), which is covered by the P3-T6 host-level and P4-T4 integration regressions. The assertion is retained as a durable guard that the fix does not introduce a direct focus call at the controller seam.

## GUI-seam compliance

The suite is fully headless: `Mock<IItemViewer>` and `Mock<IFolderSearchHandler>` only. No `Control`, `Form`, window handle, or message pump is created, so no window can appear while these tests run.

## Result

- **Output Summary:** EXIT_CODE 1. The new regression suite executed (5 tests discovered and run) and 4 of 5 tests FAILED before any production edit, each with a Moq `MockException` proving the handler performed `SetFolderDroppedDown` / `SetFolderSelectedIndex` exactly once. This is the required fail-before observation for AC-1. Accept criteria met.
