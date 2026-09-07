# P10-T16 — Test-framework compliance for every test this feature added or edited

Timestamp: 2026-08-28T02-01
Command: git diff cecd78130a489fcfdc2ddac7970f344256f4a75a -- <each test file> (to identify added and edited tests) ; grep for using directives, .Should(), Assert., Mock<, Verify* in each file
EXIT_CODE: 0

`BASELINE_SHA` is `cecd78130a489fcfdc2ddac7970f344256f4a75a`.

## Scope of this check

Two populations are covered:

- **Added tests** — 22 new `[TestMethod]` members across four new files and one appended file.
- **Edited tests** — 13 pre-existing tests whose bodies received the one-token
  `SetFolderItems` to `AddFolderItems` invocation rename. No test method was renamed and no assertion
  semantics changed in any of them.

Two further edited files, `QfcItemController.EventWiringTests.cs` and
`QfcItemController.MailActionsTests.cs`, received only a `partial` modifier on the class declaration
(`1 added / 1 deleted` and one of the three lines respectively). Those edits touch no test method, so
they contribute no row.

## Added tests

### `QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs` — new file, 5 tests

Framework `using`s: `FluentAssertions`, `FluentAssertions.Execution`,
`Microsoft.VisualStudio.TestTools.UnitTesting`. `.Should()` occurrences: **10**. Bare `Assert.`
occurrences: **0**. `Mock<` occurrences: **0**.

| # | Test | Framework | Mocking | Assertions |
|---|---|---|---|---|
| 1 | `Checked_WhenSetTrue_AssignsCheckedCheckBoxImage` (`:37`) | MSTest `[TestMethod]` | none needed (real `ToolStripMenuItemCb`, no external dependency) | FluentAssertions |
| 2 | `Checked_WhenSetFalse_AssignsNullImage` (`:63`) | MSTest `[TestMethod]` | none needed | FluentAssertions |
| 3 | `Checked_WhenSetTrue_RaisesShadowedCheckedChangedExactlyOnce` (`:82`) | MSTest `[TestMethod]` | none needed | FluentAssertions |
| 4 | `ToolStripMenuItemCb_IsNotDerivedFromControl` (`:104`) | MSTest `[TestMethod]` | none needed (reflection over the type) | FluentAssertions |
| 5 | `ItemViewerExpanded_DeclaresNoMenuItemCheckedChangedHandler` (`:127`) | MSTest `[TestMethod]` | none needed (reflection over the type) | FluentAssertions |

### `QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs` — new file, 3 tests

Framework `using`s: `FluentAssertions`, `Microsoft.VisualStudio.TestTools.UnitTesting`, `Moq`.
`Mock<` occurrences: **5**. Bare `Assert.` occurrences: **0**.

| # | Test | Framework | Mocking | Assertions |
|---|---|---|---|---|
| 6 | `HtmlDarkConverter_WhenInvokeRequired_MarshalsThroughInvoke` (`:72`) | MSTest `[TestMethod]` | Moq — `Mock<IItemViewer>` plus the synchronous `Mock<IUiDispatcher>` from `QfcItemControllerTestSupport.BuildSyncDispatcher()` | Moq `Verify` (`:81`) |
| 7 | `HtmlDarkConverter_WhenInvokeRequired_DoesNotNavigateWithoutMarshalling` (`:89`) | MSTest `[TestMethod]` | Moq | Moq `Verify` (`:100`) |
| 8 | `HtmlDarkConverter_WhenNotInvokeRequired_NavigatesDirectly` (`:108`) | MSTest `[TestMethod]` | Moq | Moq `Verify` (`:117`, `:122`) |

These three assert **interaction**, not state: the behaviour under test is whether the write is
marshalled through `_itemViewer.Invoke` or issued directly. Moq's `Verify` with a `Times` constraint
is the idiomatic and only practical expression of that; FluentAssertions has no equivalent for a mock
call-count assertion, so its absence here is not a case of an MSTest `Assert` API used where
FluentAssertions was practical. No bare MSTest `Assert` API is used.

### `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs` — new file, 2 tests

Framework `using`s: `FluentAssertions`, `Microsoft.VisualStudio.TestTools.UnitTesting`, `Moq`.
`.Should()`: **1**. Bare `Assert.`: **0**. `Mock<`: **4**.

| # | Test | Framework | Mocking | Assertions |
|---|---|---|---|---|
| 9 | `WireIntentEvents_SubscribesToPicturesChanged` (`:34`) | MSTest `[TestMethod]` | Moq — `Mock<IItemViewer>` | Moq `VerifyAdd` (`:47`) |
| 10 | `PicturesChanged_WhenRaised_RefreshesOptionsPictures` (`:56`) | MSTest `[TestMethod]` | Moq | FluentAssertions (`:74`) |

### `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.Part2.cs` — new file, 3 tests

Framework `using`s: `FluentAssertions`, `Microsoft.VisualStudio.TestTools.UnitTesting`, `Moq`.
Bare `Assert.`: **0**. `Mock<`: **10**.

| # | Test | Framework | Mocking | Assertions |
|---|---|---|---|---|
| 11 | `FlagAsTask_DoesNotReadBackFlagTaskDialogResult` (`:71`) | MSTest `[TestMethod]` | Moq | Moq `VerifyGet(..., Times.Never())` (`:81`) |
| 12 | `FlagAsTaskAsync_DoesNotReadBackFlagTaskDialogResult` (`:89`, `async Task`) | MSTest `[TestMethod]` | Moq | Moq `VerifyGet(..., Times.Never())` (`:104`) |
| 13 | `Expand_WhenFocusSubjectReturnsFalse_StillEnumeratesConversation` (`:114`) | MSTest `[TestMethod]` | Moq | Moq `Verify` (`:135`) |

The first two assert the **absence** of a property read, which only a mock can observe;
`VerifyGet(..., Times.Never())` is the sole practical expression.

### `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs` — appended, 9 new tests

Framework `using`s: `FluentAssertions`, `FluentAssertions.Execution`,
`Microsoft.VisualStudio.TestTools.UnitTesting`. `.Should()`: **24** across the file. Bare `Assert.`:
**0**. `Mock<`: **0**.

| # | Test | Framework | Mocking | Assertions |
|---|---|---|---|---|
| 14 | `ItemViewer_DeclaresNoMenuItemCheckedChangedMembers` (`:133`) | MSTest `[TestMethod]` | none needed (reflection over the type) | FluentAssertions |
| 15 | `ItemViewer_DeclaresNoMoveOptionsMenuClickHandler` (`:166`) | MSTest `[TestMethod]` | none needed | FluentAssertions |
| 16 | `ItemViewer_DeclaresNoParentChangedHandler` (`:190`) | MSTest `[TestMethod]` | none needed | FluentAssertions |
| 17 | `ItemViewerExpanded_DeclaresNoParentChangedHandler` (`:211`) | MSTest `[TestMethod]` | none needed | FluentAssertions |
| 18 | `IItemViewer_DeclaresNoUiSchedulerMember` (`:232`) | MSTest `[TestMethod]` | none needed | FluentAssertions |
| 19 | `IItemViewer_StillDeclaresUiDispatcher` (`:248`) | MSTest `[TestMethod]` | none needed | FluentAssertions |
| 20 | `IItemViewer_StillDeclaresUiSyncContext` (`:264`) | MSTest `[TestMethod]` | none needed | FluentAssertions |
| 21 | `IItemViewer_DeclaresAddFolderItemsAndNotSetFolderItems` (`:280`) | MSTest `[TestMethod]` | none needed | FluentAssertions |
| 22 | `IItemViewer_FocusSubjectReturnsBool` (`:308`) | MSTest `[TestMethod]` | none needed | FluentAssertions |

These nine are reflection-based contract assertions over `System.Type` metadata. There is no
collaborator to mock, so "none needed" is the correct entry rather than a policy gap.

## Edited tests — the `AddFolderItems` invocation rename

Each row below is a pre-existing test whose body received the one-token rename. Every one keeps its
original framework, mocking library and assertion library; none was renamed and none had its
assertion semantics changed.

| # | Test | File | Changed line | Framework | Mocking | Assertions |
|---|---|---|---|---|---|---|
| 23 | `MarkItemForDeletionAsync_AddsAndSelectsTrashThroughDispatcher` (`:183`) | `QfcItemController.SeamDispatcherTests.cs` | `:193` | MSTest | Moq | Moq `Verify` |
| 24 | `AssignFolderComboBox_RetainsSetFolderItemsAndIndexOneSelection` (`:111`) | `QfcItemController.FolderSuggestionsTests.cs` | `:131` | MSTest | Moq | Moq `Verify` |
| 25 | `AssignFolderComboBox_PredeterminedFolder_PreselectsByNameAndStillPopulates` (`:137`) | `QfcItemController.FolderSuggestionsTests.cs` | `:159` | MSTest | Moq | Moq `Verify` |
| 26 | `MarkItemForDeletion_StillAppendsTrashToDeleteViaSetFolderItems` (`:169`) | `QfcItemController.FolderSuggestionsTests.cs` | `:183` | MSTest | Moq | Moq `Verify` |
| 27 | `PopulateFolderComboBox_WhenFactorySucceeds_LoadsHandlerAndAssignsComboFromViewer` (`:329`) | `QfcItemController.FolderHandlingTests.cs` | `:349` | MSTest | Moq | Moq `Verify` |
| 28 | `PopulateFolderComboBoxAsync_WhenFactorySucceeds_DispatchesAssignFolderComboBoxThroughViewerDispatcher` (`:377`) | `QfcItemController.FolderHandlingTests.cs` | `:407` | MSTest | Moq | Moq `Verify` |
| 29 | `AssignFolderComboBox_WhenNoPredeterminedFolder_SelectsTopSuggestionViaViewer` (`:416`) | `QfcItemController.FolderHandlingTests.cs` | `:433` | MSTest | Moq | Moq `Verify` |
| 30 | `AssignFolderComboBox_WhenFolderHandlerNull_DoesNotTouchViewer` (`:465`) | `QfcItemController.FolderHandlingTests.cs` | `:476` | MSTest | Moq | Moq `Verify(..., Times.Never())` |
| 31 | `MarkItemForDeletion_WhenTrashFolderAbsent_AddsAndSelectsIt` (`:52`) | `QfcItemController.MailActionsTests.cs` | `:67` | MSTest | Moq | Moq `Verify` |
| 32 | `MarkItemForDeletion_WhenTrashFolderPresent_SelectsWithoutAdding` (`:76`) | `QfcItemController.MailActionsTests.cs` | `:88` | MSTest | Moq | Moq `Verify(..., Times.Never())` |
| 33 | `Dispose_WhenResetAndOpenWorkAreQueued_HasNoLateActivity` (`:156`) | `BreadcrumbSelectorOpenRetryTests.cs` | `:261` | MSTest | headless viewer harness | FluentAssertions |
| 34 | `ClosedSurfaceReadyBoundary_DefersPopupReplayAndReopenDoesNotDuplicateSubscriptions` (`:165`) | `BreadcrumbDropDownIntegrationTests.cs` | `:170` | MSTest | headless viewer harness | FluentAssertions |
| 35 | `ResetAndPooledReuse_DetachPopupAndDoNotDuplicateCallbacks` (`:227`) | `BreadcrumbDropDownIntegrationTests.cs` | `:248` | MSTest | headless viewer harness | FluentAssertions |
| 36 | `ItemViewerDisposal_OwnsHostAndDetachesBothSurfaces` (`:297`) | `BreadcrumbDropDownIntegrationTests.cs` | `:341` | MSTest | headless viewer harness | FluentAssertions |

Two test method names retain the word `SetFolderItems` — rows 24 and 26,
`AssignFolderComboBox_RetainsSetFolderItemsAndIndexOneSelection` and
`MarkItemForDeletion_StillAppendsTrashToDeleteViaSetFolderItems`. That is deliberate: the spec's hard
constraint on the rename is "Rename **member invocations only**. Do **not** rename the two existing
test method names." Both are named pins in the P0-T13 `BaselineNamedPins:` block, and renaming them
would break the baseline comparison.

## Acceptance

| P10-T16 condition | Result |
|---|---|
| Every added or edited test has a row | Met — 36 rows: 22 added, 14 edited |
| No row names xUnit or NUnit | Met — a case-insensitive `git grep` for `xunit` or `nunit` across the four created files and the appended contract file returns **zero** matches; every file imports `Microsoft.VisualStudio.TestTools.UnitTesting` |
| No row records an MSTest `Assert` API used where FluentAssertions was practical | Met — the bare `Assert.` count is **0** in every one of the five files. Where FluentAssertions does not appear, the assertion is a Moq `Verify`, `VerifyAdd` or `VerifyGet` on a mock interaction, for which FluentAssertions offers no equivalent |

Output Summary: All **36** tests this feature added or edited comply. 22 tests were added across
`ToolStripMenuItemCbTests.cs` (5), `QfcItemController.ThemeMarshallingTests.cs` (3),
`QfcItemController.EventWiringTests.Part2.cs` (2), `QfcItemController.MailActionsTests.Part2.cs` (3)
and `ItemViewerBreadcrumbDropDownContractTests.cs` (9); 14 pre-existing tests were edited by the
one-token `AddFolderItems` invocation rename, with no test renamed and no assertion semantics changed.
Every file uses **MSTest** (`Microsoft.VisualStudio.TestTools.UnitTesting`, `[TestMethod]`), **Moq**
wherever a collaborator needs doubling, and **FluentAssertions** for state assertions. A
case-insensitive search for `xunit` or `nunit` returns **zero** matches, and the bare MSTest `Assert.`
count is **zero** in all five files; the only non-FluentAssertions assertions are Moq `Verify`,
`VerifyAdd` and `VerifyGet` calls asserting mock interactions, for which FluentAssertions has no
equivalent.
