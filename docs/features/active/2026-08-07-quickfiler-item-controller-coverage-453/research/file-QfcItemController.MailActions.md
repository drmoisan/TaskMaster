# Per-File Research: `QuickFiler/Controllers/QfcItemController.MailActions.cs`

- Feature: F10 `quickfiler-item-controller-coverage` (issue #453), epic #136
- Branch: `feature/quickfiler-item-controller-coverage`
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a359b62de7a79b16e`
- Production file: `QuickFiler/Controllers/QfcItemController.MailActions.cs` — **224 lines**, no
  `[ExcludeFromCodeCoverage]` attribute anywhere in the file (verified by full read).
- Primary test file: `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` (184 lines)
- Shared harness: `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` (365 lines)

---

## 0. Headline conclusions

1. **Both gates are clearable with tests only, no production change.** The nine uncovered lines in the
   two `RightKeyActions*` getters are pure delegate bodies over already-mocked collaborators
   (`IQfcCollectionController`, `IItemViewer`, `IUiDispatcher`). Adding those, plus two branch-only
   tests, takes the file to **105/125 = 84.0% line and 18/22 = 81.8% branch**.
2. **Two uncovered blocks genuinely need a seam, and both are policy-relevant, not merely
   coverage-relevant.** `FlagAsTask`/`FlagAsTaskAsync` reach `FlagTasks.Run(modal: true)` (a live modal
   dialog) and `MoveMailAsync`'s catch block reaches `MessageBox.Show` (a popup). Both are unit-test-policy
   violations if a test ever reaches them, which is exactly why the existing tests deliberately stop
   short by throwing from the injected factory (`SeamFactoryTests.cs:110`, `:135`). Recommended minimum
   seam for each: an **injectable delegate**, mirroring the `_flagTasksFactory` / `_emailFilerFactory`
   pattern already established in this same class.
3. **The measured starting point is slightly worse than the brief states.** Recomputed from the
   per-line hit map, the file is at **96/125 = 76.8% line** and **16/22 = 72.7% branch**, not 77.8%/75%.
   Section 2 explains the discrepancy. Branch is currently **below** the 75% floor.
4. `IMailItemActions` (F3 / #430) already exists and is already consumed by this file
   (`_mailActions.EntryID`); **no upstream change is requested of F3.** The Outlook COM surface this
   file still touches directly is narrow and is enumerated in §5.1.
5. **The STA last-resort clause does not apply to this file.**

---

## 1. Member inventory

All members are instance members of `internal partial class QfcItemController`
(`QfcItemController.MailActions.cs:25`). No constructors, no fields, no events, no nested types.

| # | Member | Lines | Accessibility | Returns |
| --- | --- | --- | --- | --- |
| N1 | `CollapseConversation()` | 27-34 | `internal` | `void` |
| N2 | `EnumerateConversation()` | 36-47 | `internal` | `void` |
| N3 | `EnumerateConversationAsync()` | 49-52 | `internal` | `Task` |
| N4 | `RightKeyActions` (get-only property) | 54-70 | `public` | `Dictionary<string, System.Action>` |
| N5 | `RightKeyActionsAsync` (get-only property) | 72-81 | `public` | `Dictionary<string, Func<Task>>` |
| N6 | `MoveMailAsync()` | 83-126 | `public` | `Task` |
| — | (commented-out former `MoveMailAsync`) | 128-158 | — | dead code, 31 lines |
| N7 | `PackageItems()` | 160-165 | `internal` | `IList<MailItemHelper>` |
| N8 | `FlagAsTask()` | 167-181 | `public` | `void` |
| N9 | `FlagAsTaskAsync()` | 183-200 | `public` | `Task` |
| N10 | `MarkItemForDeletion()` | 202-209 | `public` | `void` |
| N11 | `MarkItemForDeletionAsync()` | 211-222 | `public` | `Task` |

Compiler-generated closures attributed to this file: `<get_RightKeyActions>b__*_0/_1` (lines 59, 63-66),
`<get_RightKeyActionsAsync>b__*_0/_1` (lines 77, 78), `<MarkItemForDeletionAsync>b__*_0` (lines 215-221),
plus async state machines for N3, N6, N9, N11 and the display-class lambda inside `FlagAsTaskAsync`
(lines 187-199).

Collaborators referenced (all declared in `QfcItemController.cs`, F10-owned):
`_itemViewer` (`IItemViewer`, line 51), `_parent` (`IQfcCollectionController`, line 44),
`_uiDispatcher` (`IUiDispatcher`, line 66), `_mailActions` (`IMailItemActions`, line 68),
`_globals` (`IApplicationGlobals`, line 42), `_homeController` (`IFilerHomeController`, line 48),
`_themes`/`_activeTheme` (lines 40, 52), `_convOriginID` (line 102),
`_optionConversationChecked`/`_optionEmailCopy`/`_optionAttachments`/`_optionsPictures` (lines 54-57),
`_flagTasksFactory` (lines 70-76), `_emailFilerFactory` (line 77), `_selectedFolder` (line 237),
`ItemHelper` (line 135), `Mail` (`MailItem`, line 180), `ConversationResolver` (line 110),
`ItemNumber` (line 193), `Token` (line 267), `logger` (line 30).

---

## 2. Measured coverage baseline and a correction to the epic's numbers

Source: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`,
class element at line 26662, `filename="QuickFiler\Controllers\QfcItemController.MailActions.cs"`.
The per-line map aligns exactly with the current source (spot-checked at `MarkItemForDeletion` 203-209,
`PackageItems` 161-165, the `MoveMailAsync` catch at 115-122), so the file content did not change
between the two branches.

**Correction to epic.md.** The epic's baseline table lists "Lines 189 / 77.8%" for a 224-line file. The
emitted `line-rate="0.777778"` is exactly `147/189`, where `189 = 125 (class-level <lines>) + 64 (sum of
the per-method <lines>)`. The tool emits each method's lines twice and computes the class rate over the
concatenation. Branch likewise: emitted `branch-rate="0.75"` is exactly `27/36 = (16 + 11)/(22 + 14)`.
The same inflation was independently confirmed on `QfcItemController.FocusAndTheme.cs`.

**Authoritative distinct-line figures for this file (class-level `<lines>` union):**

| Metric | Covered | Total | Rate | Gate | Shortfall |
| --- | --- | --- | --- | --- | --- |
| Line | 96 | 125 | **76.80%** | >= 80% | **+4 lines** |
| Branch (conditions) | 16 | 22 | **72.73%** | >= 75% | **+1 condition** |

Note that the emitted `branch-rate="0.75"` sits exactly on the floor while the true distinct-branch rate
is **below** it. A harness that reports the emitted figure would pass this file's branch gate
incorrectly. This is a second, independent instance of the epic's Directive-B harness requirement.

---

## 3. Coverage status per member, with the covering test named

| # | Member | Status | Covering test(s) | Uncovered lines | Uncovered conditions |
| --- | --- | --- | --- | --- | --- |
| N1 | `CollapseConversation` | COVERED (both arms) | `MailActionsTests.cs:137` `CollapseConversation_WhenConvOriginIdSet_TogglesGroupWithThatId`; `SeamCoreTests.cs:81` `CollapseConversation_WhenConvOriginIdEmpty_UsesMailActionsEntryId` | — | — |
| N2 | `EnumerateConversation` | COVERED | `MailActionsTests.cs:157` `EnumerateConversation_TogglesUnGroupWithResolverEntryIdAndCount` | — | — |
| N3 | `EnumerateConversationAsync` | COVERED | `SeamDispatcherTests.cs:155` `EnumerateConversationAsync_RunsEnumerateThroughDispatcher` | — | — |
| N4 | `RightKeyActions` getter | PARTIAL (dictionary built, **no lambda body executed**) | `MailActionsTests.cs:107` `RightKeyActions_Getter_ContainsExpectedMenuKeys` | 59, 63, 64, 65, 66, 68 | — |
| N5 | `RightKeyActionsAsync` getter | PARTIAL (same) | `MailActionsTests.cs:122` `RightKeyActionsAsync_Getter_ContainsExpectedMenuKeys` | 77, 78, 79 | — |
| N6 | `MoveMailAsync` | PARTIAL (happy path + both guards; **catch block uncovered**) | `SeamFactoryTests.cs:150` `..._WhenItemHelperNull_DoesNotInvokeFactory`; `:167` `..._WhenOneDriveMissing_ReturnsWithoutInvokingFactory`; `:190` `..._WhenOneDrivePresent_InvokesFactoryWithConfigAndEnqueues` | 115, 116, 118, 119, 120, 121, 122 | L90 1/2 |
| N7 | `PackageItems` | line-COVERED, branch PARTIAL | `MailActionsTests.cs:35` `PackageItems_WhenConversationUnchecked_ReturnsSingleItem` | — | L162 1/2 |
| N8 | `FlagAsTask` | PARTIAL (stops at the factory call) | `SeamFactoryTests.cs:93` `FlagAsTask_InvokesFactoryWithExpectedArguments` (factory throws `SentinelException` deliberately) | 176, 177, 178, 179, 180, 181 | L177 0/2 |
| N9 | `FlagAsTaskAsync` | PARTIAL (same) | `SeamFactoryTests.cs:124` `FlagAsTaskAsync_InvokesFactoryThroughDispatcher` (factory throws) | 194, 195, 196, 197, 198, 199, 200 | L195 0/2 |
| N10 | `MarkItemForDeletion` | COVERED (both arms) | `MailActionsTests.cs:51` `..._WhenTrashFolderAbsent_AddsAndSelectsIt`; `:75` `..._WhenTrashFolderPresent_SelectsWithoutAdding` | — | — |
| N11 | `MarkItemForDeletionAsync` | COVERED | `SeamDispatcherTests.cs:183` `MarkItemForDeletionAsync_AddsAndSelectsTrashThroughDispatcher` | — | — |

Total: 29 uncovered lines, 6 uncovered conditions.

The two `Getter_ContainsExpectedMenuKeys` tests assert dictionary membership only; their own doc comment
(`MailActionsTests.cs:92-94`) records that "the lambda bodies are not invoked so no COM is touched".
That was the correct cycle-2 decision when no collaborator mocks were wired; §8 shows the bodies are now
reachable without COM, so invoking them is gap closure, not duplication.

---

## 4. Shared test harness — what to reuse, not re-create

The full inventory of `QfcItemController.TestSupport.cs` is given in the sibling artifact
`file-QfcItemController.FocusAndTheme.md` §4. The members relevant to this file are:

| Helper | Line | Relevance here |
| --- | --- | --- |
| `HarnessController` | 25-29 | Base fixture; `QfcItemController` is `internal` and `QuickFiler/Properties/AssemblyInfo.cs:5` grants `InternalsVisibleTo("QuickFiler.Test")`. |
| `SetField` / `GetField` | 37-59 | Injecting `_parent`, `_itemViewer`, `_mailActions`, `_conversationResolver`, `_globals`, `_homeController`, `_flagTasksFactory`, `_emailFilerFactory`, `_optionConversationChecked`, `_optionAttachments`, `_selectedFolder`. |
| `BuildSyncDispatcher()` | 102-137 | `Mock<IUiDispatcher>` executing `Invoke`/`InvokeAsync`/`BeginInvoke` synchronously — required by N3, N9, N11 and by MA-05. |
| `BuildColorTheme` / `BuildThemeDictionary` | 166-192 | Supplies `ButtonClickedColor` for the `FlagAsTask` OK path (line 179). |

`QfcItemController.MailActionsTests.cs` additionally defines, file-locally:

| Helper | Line | Notes |
| --- | --- | --- |
| `private sealed class MailController : QfcItemController` | 23-27 | Duplicates `HarnessController`. New files should use `HarnessController` from the shared support file rather than adding a third subclass. |
| `SetField` | 29-32 | A local re-implementation of the shared helper. Do not copy it again. |
| `BuildResolverWithCount(int sameFolder)` | 97-104 | Builds a `ConversationResolver` via its **positional** constructor `(IApplicationGlobals, MailItem)` (`ConversationResolver.cs:64`) and sets `Count = new Pair<int>(sameFolder:, expanded:)`. Reuse verbatim; `Count` has an `internal set` (`ConversationResolver.Loading.cs:270`). |

---

## 5. Mail-action semantics and seam analysis

### 5.1 Action-to-Outlook-surface map

| Member | Outlook / COM surface touched | Via a seam? | Controller state mutated |
| --- | --- | --- | --- |
| N1 `CollapseConversation` | `MailItem.EntryID` | **Yes** — `IMailItemActions.EntryID` (`QuickFiler/Interfaces/IMailItemActions.cs:33`), F3-owned | none (delegates to `_parent.ToggleGroupConv`) |
| N2 `EnumerateConversation` | `MailItem.EntryID`; conversation counts | **Yes** — `IMailItemActions.EntryID`; `ConversationResolver.Count` | none |
| N3 `EnumerateConversationAsync` | as N2 | plus `IUiDispatcher.InvokeAsync` | none |
| N4/N5 `RightKeyActions*` | none directly; the `&Expand` body reaches N2/N3 | n/a | none |
| N6 `MoveMailAsync` | **no direct `MailItem`/`MAPIFolder`/`Store` call.** It packages `MailItemHelper` objects and hands them to `_homeController.FilerQueue.Enqueue(filer, helpers)` (line 111). The actual move is the queue's/`EmailFiler`'s job. Reads `_globals.FS.SpecialFolders` and `_globals.Ol.ArchiveRootPath`. | **Yes** — `_emailFilerFactory` delegate + `IApplicationGlobals` | none; enqueues onto `FilerQueue` |
| N7 `PackageItems` | `ConversationResolver.ConversationInfo.SameFolder` | resolver is constructible with mocks | none |
| N8/N9 `FlagAsTask*` | `List<MailItem> itemList = [Mail]` — a raw `MailItem` reference is captured but **never dereferenced here**; it is passed to `FlagTasks`. `_homeController.FormController.FormHandle` (an `IntPtr`). | Factory is seamed; **`FlagTasks.Run(modal: true)` is NOT** | writes `_itemViewer.FlagTaskDialogResult` and, on OK, `_itemViewer.FlagTaskBackColor` |
| N10/N11 `MarkItemForDeletion*` | none — operates on the `"Trash to Delete"` pseudo-folder string through `IItemViewer` | **Yes** — `IItemViewer.FolderContains` / `SetFolderItems` / `SetFolderSelectedItem` (`QuickFiler/Viewers/IItemViewer.cs:93-94`) | none directly |

**Finding:** this file contains **no direct `MAPIFolder`, `Store`, or folder-move COM call at all.** The
`Microsoft.Office.Interop.Outlook` dependency is limited to holding `MailItem` references
(`List<MailItem> itemList = [Mail]`, lines 169 and 185) which are never dereferenced in this file. The
brief's framing of "map each action to the Outlook Interop surface (move/delete/flag)" resolves to: the
move is queue-mediated, the delete is a combo-box selection, and the flag hands a `MailItem` to
`TaskVisualization.FlagTasks`. That is why the file already sits at 76.8% without a COM harness.

### 5.2 What blocks each uncovered member, and the minimum seam

| Uncovered block | Blocker | Minimum seam (hierarchy: interface > delegate > adapter) |
| --- | --- | --- |
| N4 lines 59, 63-66, 68 | **Nothing.** `_parent.PopOutControlGroup(int)` and `PopOutControlGroupAsync(int)` are both on `IQfcCollectionController` (`QuickFiler/Interfaces/IQfcCollectionController.cs:43-44`); `_itemViewer.FocusSubject()` is on `IItemViewer` (`IItemViewer.cs:54`); `EnumerateConversation` is already testable. | **None.** Tests only. |
| N5 lines 77-79 | **Nothing.** Same collaborators plus `IUiDispatcher.InvokeAsync` (already stubbed by `BuildSyncDispatcher`). | **None.** Tests only. |
| N6 lines 115-122 (catch) | `MessageBox.Show(...)` at line 119 is a modal popup — a unit-test-policy violation if reached. Reaching the catch also requires the try body to throw, which the `_emailFilerFactory` delegate can already do. | **Injectable delegate**: `private Action<string> _showUserMessage;` on `QfcItemController.cs`, defaulted in `SaveParameters` with `_showUserMessage ??= m => MessageBox.Show(m);` and called at line 119. Interface seam rejected: single method, no second implementation, and the sibling seams in this exact class (`_flagTasksFactory`, `_emailFilerFactory`, `_conversationResolverFactory`, `_folderPredictorFactory`) are all delegates — matching repo convention (`.claude/rules/general-code-change.md`, "match the existing style"). Precedent: F3's K1 dialog seam (epic.md, "Cross-Child Constraints" 2). |
| N8 lines 176-181; N9 lines 194-200 | `flagTask.Run(modal: true)` shows a live modal. `TaskVisualization.FlagTasks` is `public class FlagTasks` (`TaskVisualization/FlagTasks.cs:20`) and **`public DialogResult Run(bool modal = false)` is non-virtual** (`FlagTasks.cs:89`), so Moq cannot intercept it and returning a `Mock<FlagTasks>.Object` does not help. | **Injectable delegate**: `private Func<FlagTasks, bool, DialogResult> _flagTasksRunner;` defaulted in `SaveParameters` with `_flagTasksRunner ??= (ft, modal) => ft.Run(modal);`, called as `_flagTasksRunner(flagTask, true)` at lines 176 and 194. Tests then return `DialogResult.OK` / `DialogResult.Cancel` and can have the factory return `null` (the runner never dereferences it). Interface seam (`IFlagTaskRunner`) rejected: it would require either a new file in `QuickFiler` or an edit to `TaskVisualization`, which is outside every epic child's assignment, for no additional testability. |

### 5.3 Error-handling paths the brief asks about

- **Action fails.** `MoveMailAsync` wraps everything after the OneDrive lookup in `try/catch (System.Exception e)` (lines 91-122). It logs at Error and shows a message box; **it does not rethrow**, so the caller cannot observe the failure. See defect D-3.
- **Item already moved / helper absent.** `MoveMailAsync` is a whole-body no-op when `ItemHelper is null` (line 87). Already covered.
- **Target folder null / missing.** The OneDrive special folder is looked up with `TryGetValue` and the method returns early with a Debug log when absent (lines 93-99). Already covered. `SelectedFolder` itself is never null-checked; it is copied into `EmailFilerConfig.DestinationOlStem` (line 103) and compared to `"Trash to Delete"` (line 90) — a null `SelectedFolder` therefore yields `attachments == false` and a null destination stem passed to `EmailFiler`. See defect D-4.
- **Flag dialog cancelled.** `FlagAsTask` writes `FlagTaskDialogResult` unconditionally and `FlagTaskBackColor` only when the result is `OK` (lines 176-180). Currently unreachable in tests; MA-11/MA-12 pin it once the runner seam exists.

### 5.4 Why the STA clause does not apply

Every UI touch in this file is behind `IItemViewer` or `IUiDispatcher`. The only concrete WinForms types
named are `DialogResult` (an enum) and `MessageBox` (static). No control is constructed. **No
`*.StaTests.cs` file should be created for this file.**

### 5.5 Sibling boundaries — dependencies recorded, no edits proposed

- **F3 (#430)** owns `Interfaces/IMailItemActions.cs` and `Interfaces/MailItemActionsAdapter.cs`. This
  file consumes `IMailItemActions.EntryID` only (lines 32, 43). The interface is sufficient as it stands;
  **no upstream change is requested of F3.** `KeyboardHandler.cs` is not referenced by this file.
- **F4 (#434)** owns `Helper Classes/ConversationResolver*.cs`. This file reads
  `ConversationResolver.Count.SameFolder` (line 44) and `ConversationResolver.ConversationInfo.SameFolder`
  (line 163). Tests must depend on the **current positional constructor**
  `ConversationResolver(IApplicationGlobals, MailItem)` (`ConversationResolver.cs:64`), the `internal set`
  on `Count` (`ConversationResolver.Loading.cs:270`) and the public set on `ConversationInfo`
  (`ConversationResolver.Loading.cs:30`). Cross-child contract note: if F4 changes any of those three,
  F10's `EnumerateConversation` / `PackageItems` tests break at fan-in. `QfcThemeHelper.cs`,
  `QfcThemeControlSet.cs` are not referenced by this file.
- **F5** owns `IQfcDatamodel`; not referenced by this file.
- **`TaskVisualization/FlagTasks.cs`** is outside epic #136 entirely (it is not compiled by
  `QuickFiler.csproj`). The recommended runner delegate keeps the change wholly inside F10-owned files.
- Files this child would edit for the recommended seams, all F10-owned:
  `QfcItemController.cs` (323 lines, field declarations), `QfcItemController.Initialization.cs`
  (466 lines, the `??=` default block at `:389-395`), `QfcItemController.MailActions.cs` (call sites).

---

## 6. State-transition invariants and the tests that pin them

| ID | Invariant | Evidence | Pinning test |
| --- | --- | --- | --- |
| J1 | `CollapseConversation` prefers `_convOriginID` when non-empty and falls back to `_mailActions.EntryID` otherwise. | 32 | already pinned (`MailActionsTests.cs:137`, `SeamCoreTests.cs:81`) |
| J2 | `RightKeyActions` and `RightKeyActionsAsync` expose exactly the three keys `&Pop Out`, `&Expand`, `&Cancel`, and the two dictionaries are behaviourally parallel: same keys, same targets, sync vs async. | 54-81 | already pinned for keys; MA-01..MA-06 pin the parallel behaviour |
| J3 | `&Cancel` is a guaranteed no-op: it must not touch `_parent` or `_itemViewer`. | 68, 79 | MA-03, MA-06 |
| J4 | `&Expand` ordering: `FocusSubject()` runs **before** `EnumerateConversation()`. | 64-65 | MA-02 (ordered verification) |
| J5 | `MoveMailAsync` is a whole-body no-op when `ItemHelper is null`, and aborts without enqueueing when the OneDrive special folder is absent. | 87, 93-99 | already pinned (`SeamFactoryTests.cs:150`, `:167`) |
| J6 | Attachments are never saved when the destination is the delete pseudo-folder: `attachments = SelectedFolder != "Trash to Delete" && _optionAttachments`. | 90 | MA-08, MA-09 |
| J7 | `MoveMailAsync` enqueues rather than sorting inline; exactly one item is enqueued per call on the happy path. | 111 | already pinned (`SeamFactoryTests.cs:190`) |
| J8 | `PackageItems` returns the resolver's same-folder list in conversation mode and a single-element list otherwise. | 162-164 | already pinned for the false arm; MA-07 pins the true arm |
| J9 | `FlagAsTask` writes `FlagTaskDialogResult` unconditionally and `FlagTaskBackColor` **only** on `DialogResult.OK`. | 176-180 | MA-11, MA-12 |
| J10 | `FlagAsTaskAsync` builds `itemList` on the calling thread and performs factory + run + colour write entirely inside one `IUiDispatcher.InvokeAsync` callback (single marshal, correct ordering). | 185-199 | MA-13, MA-14 |
| J11 | `MarkItemForDeletion` is idempotent: it adds the pseudo-folder only when absent and always selects it. | 204-208 | already pinned (both arms) |
| J12 | `MarkItemForDeletionAsync` observes cancellation **before** marshalling to the dispatcher. | 213 | MA-16 |

There is no dispose/teardown guard in this file; "action-after-dispose" is not an invariant this partial
holds. Note the asymmetry that `MarkItemForDeletionAsync` checks `Token` (line 213) while
`MoveMailAsync`, `FlagAsTaskAsync` and `EnumerateConversationAsync` do not — recorded as defect D-5.

---

## 7. Determinism requirements

Verified by full read of the file:

- **No wall-clock read.** No `DateTime.Now`/`UtcNow`, no `Stopwatch`, no `TimeProvider`. The only
  date-shaped expression is `ItemHelper.SentDate` interpolated into the error message at line 120 — a
  data read from the mail item, not a clock read.
- **No randomness**, no `Guid.NewGuid`.
- **No `Thread.Sleep`, `Task.Delay`, or real wall-clock wait.** Line 112 is `await Task.CompletedTask;`,
  which completes synchronously (and is dead — defect D-6).
- **No thread-pool scheduling** originates here. `FilerQueue.Enqueue` (line 111) starts a background
  consumer in production; the existing test pre-trips the queue's `ThreadSafeSingleShotGuard` via
  reflection so no consumer thread starts (`SeamFactoryTests.cs:209-217`). New `MoveMailAsync` tests
  **must reuse that exact pattern** or avoid reaching the enqueue.
- **UI-thread marshalling** occurs at lines 51, 186, 214 (`IUiDispatcher.InvokeAsync`) — all behind the
  mockable seam. `MessageBox.Show` (line 119) and `FlagTasks.Run(modal: true)` (lines 176, 194) are the
  two unmarshalled UI operations, and both are the subject of the seams in §5.2.
- **No banned-API finding to report** for this file. The proposed tests use no sleeps, no timers, and no
  temporary files.

---

## 8. Proposed test cases

Each row is one atomic task. "Lines"/"Conds" are projected first-time-covered counts against the §2
baseline (96/125 line, 16/22 branch). MSTest, Moq, FluentAssertions, Arrange-Act-Assert.

### Tier A — required to clear both gates (no production change)

| ID | Target | Scenario | Fixture | Lines | Conds |
| --- | --- | --- | --- | --- | --- |
| MA-01 | N4 `RightKeyActions["&Pop Out"]` | positive: invoking the delegate calls `_parent.PopOutControlGroup(ItemNumber)` with this controller's number | `HarnessController`, `Mock<IQfcCollectionController>`, `controller.ItemNumber = 5` (the setter is null-safe when `_itemViewer` is null — `QfcItemController.cs:201`) | 59 (1) | — |
| MA-02 | N4 `RightKeyActions["&Expand"]` | positive + ordering (J4): `FocusSubject()` then `EnumerateConversation()`; verify with an ordered/`MockSequence` assertion | `Mock<IItemViewer>` (`GetFolderItems`), `Mock<IQfcCollectionController>`, `Mock<IMailItemActions>`, `BuildResolverWithCount(2)` | 63, 64, 65, 66 (4) | — |
| MA-03 | N4 `RightKeyActions["&Cancel"]` | negative/no-op (J3): invoking it touches no collaborator | strict mocks or `VerifyNoOtherCalls()` | 68 (1) | — |
| MA-04 | N5 `RightKeyActionsAsync["&Pop Out"]` | positive: awaiting the delegate calls `_parent.PopOutControlGroupAsync(ItemNumber)` | as MA-01, with `Returns(Task.CompletedTask)` | 77 (1) | — |
| MA-05 | N5 `RightKeyActionsAsync["&Expand"]` | positive: awaiting routes through `IUiDispatcher.InvokeAsync` into `EnumerateConversation` | as MA-02 plus `BuildSyncDispatcher()` | 78 (1) | — |
| MA-06 | N5 `RightKeyActionsAsync["&Cancel"]` | negative/no-op: returns an already-completed task, touches no collaborator | as MA-03 | 79 (1) | — |
| MA-07 | N7 `PackageItems` | positive/edge (J8): `_optionConversationChecked = true` -> returns the resolver's `ConversationInfo.SameFolder` instance | `BuildResolverWithCount` + `ConversationInfo = new Pair<List<MailItemHelper>>(sameFolder:, expanded:)` | 0 | L162 (1) |
| MA-08 | N6 `MoveMailAsync` | edge (J6): `_selectedFolder = "Trash to Delete"`, `_optionAttachments = true` -> captured `EmailFilerConfig.SaveAttachments` is `false` | the `SeamFactoryTests.cs:190` fixture (OneDrive present, pre-tripped `FilerQueue` guard) | 0 | L90 (1) |

**Tier A projection: 96 + 9 = 105/125 = 84.0% line; 16 + 2 = 18/22 = 81.8% branch.** Both gates clear
with zero production edits.

### Tier B — recommended: `FlagTasks` runner delegate seam

Production change (all F10-owned): add `private Func<FlagTasks, bool, DialogResult> _flagTasksRunner;`
to `QfcItemController.cs`; add `_flagTasksRunner ??= (ft, modal) => ft.Run(modal);` to the `??=` default
block in `QfcItemController.Initialization.cs:389-395`; replace `flagTask.Run(modal: true)` at
`MailActions.cs:176` and `:194` with `_flagTasksRunner(flagTask, true)` (1:1 line replacement, so the
`MailActions.cs` line count and coverage denominator are unchanged).

| ID | Target | Scenario | Fixture | Lines | Conds |
| --- | --- | --- | --- | --- | --- |
| MA-10 | seam default | positive: `SaveParameters` leaves `_flagTasksRunner` non-null when no runner is injected (guards the production default) | `SaveParameters` invocation as used by existing initialization tests | 0 (lines land in `Initialization.cs`) | — |
| MA-11 | N8 `FlagAsTask` | positive (J9): runner returns `DialogResult.OK` -> `FlagTaskDialogResult` set to OK **and** `FlagTaskBackColor` set to `_themes[_activeTheme].ButtonClickedColor` | `BuildColorTheme`/`BuildThemeDictionary`, factory returning `null`, runner returning `OK` | 176, 178, 179, 180, 181 (5) | L177 (1) |
| MA-12 | N8 `FlagAsTask` | negative (J9): runner returns `DialogResult.Cancel` -> result recorded, back colour **not** written | as MA-11 | 177 (1) | L177 (1) |
| MA-13 | N9 `FlagAsTaskAsync` | positive (J10): OK path executes entirely inside one `InvokeAsync` callback | as MA-11 plus `BuildSyncDispatcher()` | 194, 196, 197, 198, 199, 200 (6) | L195 (1) |
| MA-14 | N9 `FlagAsTaskAsync` | negative: Cancel path writes no back colour | as MA-13 | 195 (1) | L195 (1) |

**Tier A + B projection: 118/125 = 94.4% line; 20/22 = 90.9% branch.**

### Tier C — optional: user-message delegate seam (closes the last block)

Production change: `private Action<string> _showUserMessage;` on `QfcItemController.cs`, defaulted with
`_showUserMessage ??= m => MessageBox.Show(m);`, called at `MailActions.cs:119`.

| ID | Target | Scenario | Fixture | Lines | Conds |
| --- | --- | --- | --- | --- | --- |
| MA-15 | N6 `MoveMailAsync` | error path: the `_emailFilerFactory` throws -> exception is logged and surfaced through the injected presenter, and does **not** propagate to the caller | OneDrive-present fixture; factory throws; captured message asserted to contain the subject | 115, 116, 118, 119, 120, 121, 122 (7) | — |
| MA-16 | N11 `MarkItemForDeletionAsync` | error path (J12): pre-cancelled `Token` -> throws before any `IUiDispatcher` call | `Token` setter with a cancelled token | 0 | — |

**Tier A + B + C projection: 125/125 = 100% line; 22/22 = 100% branch.**

### Sequencing note for the plan

Tier A alone satisfies epic AC1 and the branch floor and requires no production edit; it should be its
own plan phase with an independent exit gate. Tier B is recommended because it removes a real
"a test that goes one line further shows a modal dialog" hazard rather than merely adding coverage.
Tier C is optional and can be deferred to the capstone if the change budget is tight.

---

## 9. File-size and file-creation impact

| File | Current | Limit | Change proposed | Projected |
| --- | --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcItemController.MailActions.cs` | 224 | 500 | Tier B/C replace 3 call-site lines 1:1 | 224 |
| `QuickFiler/Controllers/QfcItemController.cs` | 323 | 500 | Tier B/C add 2 field declarations (+ ~4 comment lines) | ~329 |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs` | **466** | 500 | Tier B/C add 2 `??=` default lines | ~468 (**34 lines of headroom — verify before editing**) |
| `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` | 184 | 500 | Tier A can extend this file (~+180 lines) | ~364 |

**Obligations the plan must encode.**

1. **No new production `.cs` file is required** under any tier, because both seams are fields on an
   existing partial. Therefore **no `<Compile Include=...>` edit to `QuickFiler/QuickFiler.csproj`** and
   **no new ledger row** under epic.md "Mid-Wave File Creation". If a later decision splits the seams
   into their own file, that file defaults to `testable` at **>= 90%** per rule 4 of that section, and the
   csproj entry and ledger row must be added in the same change.
2. If Tier A is split across more than one test file (for example to keep the menu-delegate tests
   separate), **each new test file needs an explicit `<Compile Include="Controllers\....cs" />` entry in
   `QuickFiler.Test/QuickFiler.Test.csproj`** — that project also uses explicit includes with no
   globbing (`QuickFiler.Test.csproj:58-128`). Recommended: keep Tier A inside the existing
   `QfcItemController.MailActionsTests.cs` (it has ~316 lines of headroom) and put Tier B/C in a new
   `QuickFiler.Test/Controllers/QfcItemController.MailActionsSeamTests.cs`.
3. **CRLF preservation** on any csproj edit: use the Edit tool or `perl -0777` with explicit `\r\n`,
   never a git-bash `sed -i` (epic.md, "Cross-Child Constraints" 1). Keep the edit to one minimal
   adjacent hunk near the existing `Controllers\QfcItemController*` entries.
4. `QfcItemController.Initialization.cs` at 466 lines is the tightest file in the change set. Verify its
   line count immediately before editing; if a sibling task in this same child has already grown it, the
   seam defaults must move rather than push it over 500.

---

## 10. Latent defects for promotion

Report only; do **not** fix under this child. Promote via the MCP promotion lifecycle.

| ID | Location | Description | Severity |
| --- | --- | --- | --- |
| D-1 | `QfcItemController.MailActions.cs:31` | `var folderList = _itemViewer.GetFolderItems();` in `CollapseConversation` is assigned and **never used**. It is a wasted round-trip to the viewer on every collapse. (The identical call at line 40 in `EnumerateConversation` **is** used, at line 45.) | Low — dead call, wasted UI work. |
| D-2 | `QfcItemController.MailActions.cs:119-121` | `MessageBox.Show` is invoked from inside a `catch` in an `async` method with no dispatcher marshalling. On a non-UI thread this either blocks that thread with a modal window or throws. It is also unreachable to any test without a seam. | **Medium** — modal dialog from a potentially non-UI context. |
| D-3 | `QfcItemController.MailActions.cs:115-122` | The catch swallows every `System.Exception` and does not rethrow, so `MoveMailAsync` returns normally after a failed move. The caller cannot distinguish success from failure, and the item is silently not filed. This contradicts `.claude/rules/general-code-change.md` ("do not use broad catch-all handlers unless you immediately re-raise or propagate with added context"). | **Medium** — silent failure. |
| D-4 | `QfcItemController.MailActions.cs:90`, `:103` | `SelectedFolder` is never null-checked. A null value yields `attachments == false` (because `null != "Trash to Delete"` is true but `_optionAttachments` gates it) and a null `EmailFilerConfig.DestinationOlStem` handed to `EmailFiler`. | Low — deferred null failure. |
| D-5 | `QfcItemController.MailActions.cs:83`, `:183`, `:49` | Cancellation is inconsistent across the async members: `MarkItemForDeletionAsync` calls `Token.ThrowIfCancellationRequested()` (line 213) but `MoveMailAsync`, `FlagAsTaskAsync` and `EnumerateConversationAsync` do not. A cancelled QuickFiler session can still enqueue a move or open a flag dialog. | **Medium** — cancellation is not honoured uniformly. |
| D-6 | `QfcItemController.MailActions.cs:112` | `await Task.CompletedTask;` is a residual no-op left behind when the inline `filer.SortAsync(helpers)` call (still present, commented, at line 113) was replaced by the queue enqueue. | Low — dead code. |
| D-7 | `QfcItemController.MailActions.cs:128-158` | 31 lines of commented-out dead code (the previous `MoveMailAsync` implementation). | Low — maintenance. |
| D-8 | `QfcItemController.MailActions.cs:54-70`, `:72-81` | `RightKeyActions` and `RightKeyActionsAsync` allocate a **new** `Dictionary` on every property read, and the two dictionaries duplicate the same key set and targets. A caller that reads the property twice gets two different dictionaries with two different delegate instances, so unsubscribe-by-reference is impossible. | Low — allocation and identity surprise. |

---

## 11. Rejected alternatives

- **Mock `TaskVisualization.FlagTasks` with Moq and stub `Run`.** Rejected on evidence:
  `public DialogResult Run(bool modal = false)` (`TaskVisualization/FlagTasks.cs:89`) is non-virtual, so
  Moq cannot intercept it; a `Mock<FlagTasks>.Object` would execute the real modal body.
- **Introduce `IFlagTasks` in `TaskVisualization` and change `_flagTasksFactory` to return it.**
  Rejected: `TaskVisualization` is outside every epic-#136 child's file assignment, the change ripples
  through `QfcItemController.Initialization.cs`'s optional-parameter constructor signature and three
  existing tests, and it buys nothing the two-argument runner delegate does not.
- **Leave `FlagAsTask`/`FlagAsTaskAsync` uncovered and record a `ratified-exempt` note.** Rejected:
  epic.md §"Shared Design" 1 reads the CLAUDE.md exemption qualifier "without an injectable seam" as a
  live obligation. A seam is demonstrably feasible here (§5.2), so the exemption does not apply and
  `[ExcludeFromCodeCoverage]` on these members would be a Blocking finding.
- **Cover the `MoveMailAsync` catch block by letting the real `MessageBox.Show` run under a suppressed
  window.** Rejected outright: a popup requiring human interaction is a unit-test-policy violation
  (epic.md §"Shared Design" 2), and there is no supported suppression mechanism.
- **Extend `IMailItemActions` (F3) with a `Move`/`Delete` operation to "seam the mail actions".**
  Rejected as unnecessary: §5.1 establishes that this file performs no direct folder-move or delete COM
  call — the move is queue-mediated and the delete is a combo-box selection through `IItemViewer`.
