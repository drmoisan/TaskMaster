# [P5-T4] Pass-After Verification

- **Issue:** #438
- **Task:** [P5-T4]
- **Timestamp:** 2026-08-08T11-41
- **Tree state:** the behavior flip is applied — `QfcItemController.TextBoxSearch_TextChanged` now issues `FindFolder` plus exactly one `_itemViewer.PresentFolderSearchResults(folders)` call.

## Command 1 — controller-seam pass-after

`pwsh -NoProfile -Command "& 'C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:\"FullyQualifiedName~QfcItemController\" ; exit $LASTEXITCODE"`

- **EXIT_CODE:** 0

```
Total tests: 180
     Passed: 180
     Failed: 0
```

### Fail-before -> pass-after transition (AC-1)

The six tests observed failing at P5-T2 all pass:

| Test | P1-T2 | P5-T2 | P5-T4 |
|---|---|---|---|
| `..._EventHandlersTests.TextBoxSearch_TextChanged_UsesInjectedFolderSearchHandler_PresentsSearchResultsWithoutFocusOrCommit` | n/a | Failed | **Passed** |
| `..._SearchFocusRegressionTests.TextBoxSearch_TextChanged_IssuesThePresentationIntentExactlyOnce` | n/a | Failed | **Passed** |
| `..._NeverRequestsADropDownStateChange` | Failed | Failed | **Passed** |
| `..._NeverCommitsAFolderSelection` | Failed | Failed | **Passed** |
| `..._SingleResult_StillTransfersNoFocusAndCommitsNothing` | Failed | Failed | **Passed** |
| `..._EmptyResult_TransfersNoFocusAndDoesNotThrow` | Failed | Failed | **Passed** |

### Explicit-gesture suites, byte-unmodified, all passing (AC-7)

The 180-test run covers every `QfcItemController*` suite. Files with **zero diff** whose tests pass:

- `QfcItemController.EventHandlersTests.cs:355-388` — `TextBoxSearch_KeyDown_WhenDownArrow_DropsDownAndFocusesFolder`, `TextBoxSearch_KeyDown_WhenNotDownArrow_DoesNothing` (this file's only diff is the single sanctioned D4 method rewrite; both KeyDown methods are untouched)
- `QfcItemController.NavigationTests.cs:159-181` — `JumpToFolderDropDown` focuses and drops down
- `QfcItemController.SeamDispatcherTests.cs:94-95` — the async `JumpToFolderDropDownAsync` variant
- `QfcItemController.FolderSuggestionsTests.cs` — the suggestions path
- `QfcItemController.FolderHandlingTests.cs` — including `AssignFolderComboBox`'s `SetFolderSelectedIndex(...)`, which is out of scope and unchanged

## Command 2 — D7 loose-mock sweep across ALL test files

Multiline search for `.Setup(...)` / `.Verify(...)` expressions targeting `OpenAsync` across every `.cs` file under a test project:

Pattern: `\.(Setup|Verify)[<(][\s\S]{0,300}?OpenAsync\s*\(`

| File | Sites | Overload shape | Status |
|---|---|---|---|
| `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` | 1 `Setup` (`:348`), 1 failure-path `Setup` (`:270`), 4 `Verify` | 3-parameter only | byte-unmodified, all 10 tests pass |
| `QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs` | 1 `Setup` (`:169`) | 3-parameter only | byte-unmodified, passes |
| `QuickFiler.Test/Viewers/BreadcrumbSubfolderActivationTests.cs` | 1 `Setup` (`:310`) | 3-parameter only | byte-unmodified, passes |
| `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs` | bare `new Mock<IBreadcrumbDropDownHost>()` with no `OpenAsync` setup at all (`:132`) | n/a | byte-unmodified, passes |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownSearchIntegrationTests.cs` | 1 `Setup` (4-parameter), 10 `Verify` (both shapes) | new file | passes |

No other test file in the repository sets up or verifies `OpenAsync`.

**Zero-invocation break check (the #424 lesson):** because `BeginOpenCore` dispatches default opens through the **existing 3-parameter** overload and only search-originated opens through the 4-parameter one, every pre-existing 3-parameter `Setup` still matches its invocation and every pre-existing 3-parameter `Verify(..., Times.Once())` still counts the same invocation. No pre-existing setup became unmatched and no pre-existing verify dropped to zero.

Confirmation run for the three loose-mock suites:

`/TestCaseFilter:"FullyQualifiedName~BreadcrumbSubfolderActivationTests|FullyQualifiedName~BreadcrumbPendingOpenCloseTests|FullyQualifiedName~QfcItemControllerBreadcrumbDropDownTests"`

- **EXIT_CODE:** 0

```
Total tests: 17
     Passed: 17
     Failed: 0
```

## Result

- **Output Summary:** EXIT_CODE 0 with 180 of 180 `QfcItemController` tests passing. All six regressions that failed at P5-T2 now pass, completing the fail-before/pass-after proof for AC-1. Every explicit-gesture suite (Down arrow, `JumpToFolderDropDown` sync and async, suggestions, folder handling) passes byte-unmodified, satisfying AC-7. The D7 loose-mock sweep found four pre-existing `OpenAsync` setup/verify sites, all 3-parameter, all still matching after the change; a confirmation run of the three affected suites returned EXIT_CODE 0 with 17 of 17 passing. Accept criteria met.
