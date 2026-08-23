# [P5-T2] Fail-Before Controller Run (expect-fail, second observation)

- **Issue:** #438
- **Task:** [P5-T2] `[expect-fail]`
- **Timestamp:** 2026-08-08T11-41
- **Tree state:** the new surface (`IItemViewer.PresentFolderSearchResults`, the presentation composite, the `takeFocus` pipeline) is fully in place, but `QfcItemController.EventHandlers.cs` has **not** been flipped — `TextBoxSearch_TextChanged` still issues the defective composition.

## Command

`pwsh -NoProfile -Command "& 'C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:\"FullyQualifiedName~QfcItemController_SearchFocusRegressionTests|FullyQualifiedName~QfcItemController_EventHandlersTests\" ; exit $LASTEXITCODE"`

(Filter deviation D10 applies — see `fail-before.2026-08-08T11-41.md`. The plan's dotted form selects zero tests and exits 0.)

- **EXIT_CODE:** 1 (expected — this is the second fail-before observation required by plan D5)

## Result

```
Total tests: 22
     Passed: 16
     Failed: 6
```

| Failing test | Assertion that fails pre-flip |
|---|---|
| `QfcItemController_EventHandlersTests.TextBoxSearch_TextChanged_UsesInjectedFolderSearchHandler_PresentsSearchResultsWithoutFocusOrCommit` | `PresentFolderSearchResults` never called |
| `QfcItemController_SearchFocusRegressionTests.TextBoxSearch_TextChanged_IssuesThePresentationIntentExactlyOnce` | `PresentFolderSearchResults` never called |
| `..._NeverRequestsADropDownStateChange` | `SetFolderDroppedDown` called once |
| `..._NeverCommitsAFolderSelection` | `SetFolderSelectedIndex` called once |
| `..._SingleResult_StillTransfersNoFocusAndCommitsNothing` | `SetFolderDroppedDown` called once |
| `..._EmptyResult_TransfersNoFocusAndDoesNotThrow` | `SetFolderDroppedDown` called once |

## Captured failure output

```
Test method ...QfcItemController_EventHandlersTests.TextBoxSearch_TextChanged_UsesInjectedFolderSearchHandler_PresentsSearchResultsWithoutFocusOrCommit threw exception:
Moq.MockException:
Expected invocation on the mock once, but was 0 times: v => v.PresentFolderSearchResults(["\\A\one", "\\A\two"])

Test method ...QfcItemController_SearchFocusRegressionTests.TextBoxSearch_TextChanged_IssuesThePresentationIntentExactlyOnce threw exception:
Moq.MockException:
Expected invocation on the mock once, but was 0 times: v => v.PresentFolderSearchResults(["\\A\one", "\\A\two"])

Test method ...TextBoxSearch_TextChanged_NeverRequestsADropDownStateChange threw exception:
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

## Significance

This is the observation plan D5 requires in addition to the P1-T2 run: the finalized assertions — including the **positive** intent assertion, which could not compile before Phase 4 — are now compiled against the delivered surface and are observed failing **solely** because the controller handler has not yet been flipped. It isolates the remaining defect to exactly one file, `QuickFiler/Controllers/QfcItemController.EventHandlers.cs:164-178`.

The 16 passing tests in the same run include the explicit-gesture guards that must not regress:

- `TextBoxSearch_KeyDown_WhenDownArrow_DropsDownAndFocusesFolder` — Passed
- `TextBoxSearch_KeyDown_WhenNotDownArrow_DoesNothing` — Passed
- `CboFolders_SelectedIndexChanged_StoresSelectedFolder` — Passed

`TextBoxSearch_TextChanged_NeverFocusesTheFolderDropDown` passes, as it did at P1-T2, because the handler never calls `FocusFolderDropDown()` directly; that transfer lives downstream in the open pipeline and is covered by the P3-T6 and P4-T4 suites.

## Result

- **Output Summary:** EXIT_CODE 1. Six tests failed before the behavior flip — the two positive `PresentFolderSearchResults` intent assertions (never invoked) and the four negative focus/commit assertions (`SetFolderDroppedDown` and `SetFolderSelectedIndex` each invoked once). All explicit-gesture tests in the same run passed unmodified. This satisfies the D5 second fail-before observation. Accept criteria met.
