# Research — `QfcCollectionController.cs` (Issue #454, epic #136 child F11)

- Date: 2026-08-07
- Target production file: `QuickFiler/Controllers/QfcCollectionController.cs`
- Contract file: `QuickFiler/Interfaces/IQfcCollectionController.cs`
- Existing tests: `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`,
  `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs`
- Method: every claim below is cited to `file:line` from a direct read of the working tree.
  All paths are repo-relative.

---

## 0. Verification summary

| Claim under test | Verdict | Evidence |
| --- | --- | --- |
| File is 2,349 lines | Confirmed | `QuickFiler/Controllers/QfcCollectionController.cs:2349` is the final `}`; no line 2350 |
| `[ExcludeFromCodeCoverage]` present | Confirmed | `QuickFiler/Controllers/QfcCollectionController.cs:21`, immediately above `public class QfcCollectionController : IQfcCollectionController` at `:22` |
| File is absent from Cobertura (unmeasured) | Consistent with the attribute; no report read this cycle | epic.md:180-187 states exempted files are absent from instrumentation |
| At least five partials needed | Confirmed and exceeded — 13 partials recommended | §A |
| Existing tests contribute zero measured coverage | Confirmed as to *measured* coverage | The attribute at `:21` removes the type from instrumentation entirely; the tests still execute and still assert |
| Region boundaries as stated in the brief | Confirmed exactly | `#region`/`#endregion` at lines 28/55, 57/82, 84/249, 251/1250, 1252/1387, 1389/1704, 1706/1987, 1989/2109, 2111/2174, 2176/2347 |
| `System.Net.NetworkInformation` unused | Confirmed | `:6`; no `Ping`/`NetworkInterface`/`IPGlobal*`/`PhysicalAddress`/`NetworkChange` token anywhere in the file |
| Banned determinism-hostile APIs in production file | None found | No `DateTime.Now`, `DateTime.UtcNow`, `Thread.Sleep`, `Task.Delay`, `Random` in the file |
| Banned APIs in the two existing test files | None found | Same search across both test files |

---

## G. Documented Deviations

These correct statements in the delegation brief or in inherited context. The spec must carry the
correction.

### G1. `QuickFiler` **does** grant `InternalsVisibleTo("QuickFiler.Test")`

`QuickFiler/Properties/AssemblyInfo.cs:5` contains `[assembly: InternalsVisibleTo("QuickFiler.Test")]`.
Therefore **`internal` seams on `QfcCollectionController` are directly testable** and the brief's
fallback ("if it does not, propose the alternative") is not needed. The separate epic constraint
still holds and is unaffected: `UtilitiesCS` grants internals to `DynamicProxyGenAssembly2`,
`UtilitiesCS.Test`, and `ToDoModel.Test` only (epic.md:619-631), so `UtilitiesCS` internals remain
unreachable. Every seam proposed in §B is `private` or `internal` and none is reachable through
`UtilitiesCS`.

Note a second, load-bearing grant: `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` —
which is what allows Moq to create proxies for `internal` types such as `IEmailMoveMonitor`
(`QuickFiler/Interfaces/IEmailMoveMonitor.cs:13`, mocked at
`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:351`) and for the two new `internal`
seam interfaces proposed in §A4 — is **not** in `Properties/AssemblyInfo.cs`. It is declared in
`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:11`, an **F2-owned** file (epic.md:331). F11
must not remove or relocate it, and must not assume it lives with the other assembly attributes.
(A second copy exists at `QuickFiler/Legacy/IAcceleratorCallbacks.cs:5`, which is not compiled.)

### G2. `QuickFiler.Test` already has working STA infrastructure

Inherited context (F4/#434 research) recorded "QuickFiler.Test has zero STA infra". That is now
false. `QuickFiler.Test` contains three manual STA-thread helpers:

- `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:267-278` (parked STA dispatcher)
- `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:302-313` (`StartRunningDispatcher`,
  with `ShutdownDispatcher` at `:323-326`)
- `QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs:21-45`

There is no `[STATestClass]`/`MSTest.STAExtensions` package in `QuickFiler.Test`
(no match for `STATestClass`, `STATestMethod`, `STAThread`, or `MSTest.STAExtensions` anywhere under
`QuickFiler.Test`). **Consequence: the epic's STA last-resort clause (epic.md:234-241) can be
satisfied in this child without adding any NuGet package** — by a dedicated STA-thread runner helper
plus `*.StaTests.cs` file naming. No `packages.config` edit is required. This materially lowers the
risk of the STA path.

### G3. `[ExcludeFromCodeCoverage]` does not mean "untested"

The two existing test files are not dead. They construct the controller (one via the real
constructor, one via `FormatterServices.GetUninitializedObject`) and assert real behavior. Removing
the attribute will therefore *not* reveal 0%; see §C5 for the estimated floor (12%-20%).

### G4. `QfcItemGroup.ItemViewer` is typed as the concrete `ItemViewer` and is **out of this child's
file scope**

`QuickFiler/Controllers/QfcItemGroup.cs:32-37` types `ItemViewer` as the concrete WinForms control,
not `IItemViewer`. Retyping it would be the single highest-leverage testability change, but
`Controllers/QfcItemGroup.cs` is assigned to **F2** (epic.md:334). F11 must not edit it. This is why
§B recommends a viewer-surface adapter rather than retyping the group.

### G5. `System.Windows` (`:10`) is also an unused-but-load-bearing using

`QuickFiler/QuickFiler.csproj:287` references `WindowsBase`, so `using System.Windows;` imports
`System.Windows.Size`/`Point`, which is why every `System.Drawing.Size`/`Point` construction in the
file is fully qualified (`:1526`, `:1535`, `:1575`, `:2018`, `:2023`, `:2031`, `:2086`, `:2102`).
`PresentationFramework` is **not** referenced, so `MessageBox` at `:186` resolves unambiguously to
`System.Windows.Forms.MessageBox`. No `System.Windows` type is used unqualified, so the directive
is droppable; keep the fully-qualified `System.Drawing.*` call sites as-is to avoid churn, and do
not propagate `using System.Windows;` into new partials.

### G6. A significant fraction of the file is unreachable

Twelve members have **no caller anywhere in the repository** (production or test). See §A partial
`LegacyLoadPaths` and §E1. This changes the coverage arithmetic: those members are in the
denominator and must be covered by direct-call tests or removed. The epic's no-behavior-change NFR
argues against deletion in this child (deleting `public` members changes the public API surface),
so they are isolated into one partial and promoted as a follow-up.

---

## A. Partial-split design

### A0. Design rules applied

1. **Responsibility, not line count.** The `#region` markers were used as a hypothesis and then
   overridden in four places (see A2).
2. **Every partial must have a testable entry point.** A partial whose members can only be reached
   through a WinForms control is not independently testable and was either merged into a partial
   that has an entry point, or reduced to thin delegation over a seam.
3. **Precedent.** `QfcItemController` is already split into 10 partials for 3,073 lines
   (epic.md:395-398). 13 partials for 2,349 lines is the same density.
4. **Projection method.** Each projection = sum of the member's current line span + a per-file
   overhead of **20 lines** (using block sized to that partial + blank + `namespace` + `{` +
   `public partial class QfcCollectionController` + `{` + `}` + `}` + a 4-line file-header XML
   comment). Where a member is reduced to thin delegation over a seam, the reduced figure is used
   and marked "thin".

### A1. Root file — members that must live in exactly one file

The class declaration, the `log4net` logger, the constructor, and every instance field live in
**`QuickFiler/Controllers/QfcCollectionController.cs`** (retained; already listed at
`QuickFiler/QuickFiler.csproj:311`). `public class` becomes `public partial class` at `:22`, and
`[ExcludeFromCodeCoverage]` at `:21` plus `using System.Diagnostics.CodeAnalysis;` at `:4` are
removed.

### A2. Where the proposed boundaries differ from the `#region` boundaries

| Region | Deviation | Rationale |
| --- | --- | --- |
| `UI Add and Remove QfcItems` (251-1250) | Split five ways: `LoadSync`, `LoadAsync`, `GroupFactory`, `Removal`, `LegacyLoadPaths` | ~1,000 lines mixing three unrelated responsibilities (loading, per-group construction, removal) plus dead code |
| `Event Wiring` (1252-1387) | `WireUpKeyboardHandler` (1254-1273) and `AnyOpenDropDownsAsync` (1324-1328) moved out to `LegacyLoadPaths` | Both are unreachable (§E1); keeping them here would put the #444 defect in a live-code file and distort its coverage |
| `Helper Functions` (1989-2109) | `CaptureTlpTemplate` (1991-1996) moved out to `LegacyLoadPaths` | Unreachable; `_templateTlp` (`:70`) is written nowhere else |
| `UI Conversation Expansion` (1706-1987) | `InitializeGroup` (1849-1864) and `AddItemGroup` (1924-1968) moved to `GroupFactory` | `InitializeGroup` is per-group construction shared with the load path; `AddItemGroup` adds one group and is not conversation logic |
| `Major Actions` (2176-2347) | `Cleanup` (2192-2204) and `CleanupAsync` (2178-2190) moved to the root file | Lifecycle pairs with the constructor; leaves `Move` as a single-responsibility file |
| `UI Select QfcItems` (1389-1704) | Split into `Selection` and `NavigationToggle` | Item activation/index arithmetic is pure-ish; navigation/expansion toggling is fan-out over `IQfcItemController` — different test shapes |

### A3. Proposed file list

Overhead constant 20 lines/file. "Thin" marks a member reduced to delegation over the §B seams.

---

#### 1. `QuickFiler/Controllers/QfcCollectionController.cs` (root, retained) — **~202 lines**

| Member | Current lines |
| --- | --- |
| `logger` | 24-26 |
| ctor `QfcCollectionController(...)` | 30-53 |
| all private fields (`_formViewer`.. `BackgroundLoadingTasks`) | 60-80 |
| new `#region Seams` (seam fields + production defaults, §B) | new, ~90 |
| `CleanupAsync` | 2178-2190 |
| `Cleanup` | 2192-2204 |

`using`s: `System`, `System.Collections.Concurrent`, `System.Collections.Generic`,
`System.Threading`, `System.Threading.Tasks`, `System.Windows.Forms`,
`Microsoft.Office.Interop.Outlook`, `QuickFiler.Helper_Classes`, `QuickFiler.Interfaces`,
`UtilitiesCS`, `UtilitiesCS.Threading`. **Removed:** `System.Diagnostics.CodeAnalysis` (`:4`),
`System.Net.NetworkInformation` (`:6`), `System.Windows` (`:10`).

Responsibility: construction, dependency capture, seam defaults, teardown.
Independently testable: the constructor is already driven end-to-end by
`QfcCollectionControllerDarkModeTests.cs:50-59` with every collaborator mocked;
`Cleanup`/`CleanupAsync` are already driven at `:107` and `:136` of that file.

---

#### 2. `QuickFiler/Controllers/QfcCollectionController.State.cs` — **~180 lines**

Members: `_activeIndex`/`ActiveIndex` 86-91, `ActiveSelection` 92-96, `_token`/`Token` 98-103,
`_tokenSource`/`TokenSource` 105-110, `_digitRefreshNeeded`/`_digits`/`Digits` 112-128,
`SetVisualDigits` 130-146 (thin), `EmailsLoaded` 148, `EmailsToMove` 150, `ReadyForMove` 152-194,
`_tlpLayout`/`TlpLayout` 196-231 (thin), `SafeSetTlpLayout` 233-238, `_itemGroups`/`ItemGroups`
240-247.

`using`s: `System`, `System.Collections.Generic`, `System.Linq`,
`System.Runtime.CompilerServices` (for `MethodImpl` at 116/199/201/243/245), `System.Threading`,
`UtilitiesCS.Extensions`.

Responsibility: observable controller state and the layout-suspension gate.
Independently testable: every member is field-driven; `Digits` (114-128) and `TlpLayout` (197-231)
are already reachable in the existing swap tests. `ReadyForMove` becomes testable once the
`MessageBox.Show` at 186-191 is behind seam **S6**; `TlpLayout`'s `_itemTlp.InvokeRequired`/`Invoke`
(209-227) and `SetVisualDigits`'s `grp.ItemViewer.LblItemNumber.Text` (141) go behind **S1**.

---

#### 3. `QuickFiler/Controllers/QfcCollectionController.LoadSync.cs` — **~113 lines**

Members: `LoadControlsAndHandlers_01(TableLayoutPanel, List<QfcItemGroup>)` 253-266,
`LoadControlsAndHandlers_01(IList<MailItem>, RowStyle, RowStyle)` 268-296,
`LoadItemGroupsAndViewers_02` 740-754, `LoadConversationsAndFolders_04` 756-759,
`LoadSequential_5` 798-825.

`using`s: `System.Collections.Generic`, `System.Windows.Forms`,
`Microsoft.Office.Interop.Outlook`, `UtilitiesCS`, `UtilitiesCS.Extensions`.

Responsibility: the synchronous load pipeline and the cached-page swap entry point.
Independently testable: the `(tlp, itemGroups)` overload is **already** exercised end-to-end at
`QfcCollectionControllerTests.cs:409-423`, `:430-445`, `:474-492` with a mocked `IQfcFormViewer`,
mocked `IEmailMoveMonitor`, and a real `KbdActions`.

---

#### 4. `QuickFiler/Controllers/QfcCollectionController.LoadAsync.cs` — **~293 lines**

Members: `GetPartiallyInitializedHelperAsync` 298-318, `ValidateParams` 320-339,
`LoadControlsAndHandlers_01Async(IList<MailItem>, ...)` 341-418,
`LoadControlsAndHandlers_01Async(IList<QfcPreScoredItem>, ...)` 420-507,
`LoadSecondaryAsync` 525-579, `CreateEmptyKbdHandlerCharActions` 581-585.
The commented-out block at 509-523 is deleted (dead comment, no behavior).

`using`s: `System`, `System.Collections.Generic`, `System.Linq`, `System.Threading`,
`System.Threading.Tasks`, `System.Windows.Forms`, `Microsoft.Office.Interop.Outlook`,
`UtilitiesCS`, `UtilitiesCS.Extensions`.

Responsibility: the two production async load paths (standard + issue-#171 carrier list) and the
secondary conversation/folder load.
Independently testable: with **S5** (helper factory), **S3**/**S4** (viewer + item-controller
factories) and **S1** the whole body is reachable; `ValidateParams` (320-339) is testable today
against a mocked `IQfcFormViewer` because it only reads `InvokeRequired` and the token.

---

#### 5. `QuickFiler/Controllers/QfcCollectionController.GroupFactory.cs` — **~134 lines**

Members: `EncapsulateItemGroup` 607-633, `LoadItemToTlp` 904-949 (thin), `LoadItemViewer_03`
951-962, `InitializeGroup` 1849-1864, `AddItemGroup` 1924-1968.

`using`s: `System.Windows.Forms`, `Microsoft.Office.Interop.Outlook`,
`QuickFiler.Helper_Classes`, `QuickFiler.Interfaces`, `UtilitiesCS`.

Responsibility: create one `QfcItemGroup` (viewer + controller) and place it in the layout.
Independently testable: `ItemViewerQueue.Dequeue` (617, 958) is already replaceable from
`QuickFiler.Test` — `ItemViewerQueue.SetCoreForTesting` is `internal`
(`QuickFiler/Helper Classes/ItemViewerQueue.cs:69`) and is already used with an uninitialized
`ItemViewer` at `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs:128,143`. Seam
**S3** makes this local rather than global static mutation; **S4** replaces the
`new QfcItemController(...)` at 620-630 and 1853-1862.

---

#### 6. `QuickFiler/Controllers/QfcCollectionController.Removal.cs` — **~348 lines**

Members: `ActivateQueuedTlp` 859-863, `CacheItemGroupsForMove` 876-881,
`ActivateQueuedItemGroups` 883-886, `SwapItemGroups` 888-896, `CacheMoveObjects` 898-902,
`PopOutControlGroup` 964-974 (thin), `PopOutControlGroupAsync` 976-989 (thin), `RemoveControls`
991-1011, `CleanupBackground` 1013-1022, `RemoveControlsAsync` 1024-1044, `RemovedItemMonitor`
1046-1051, `RemoveSpecificControlGroup(string)` 1053-1058, `_removeGroupByEntryId` +
`RemoveGroupByEntryId` 1060-1074, `RemoveBelowThresholdAsync` 1076-1097,
`RemoveSpecificControlGroup(int)` 1099-1155, `removespecificcontrolgroupcounter` 1157,
`RemoveSpecificControlGroupAsync` 1159-1248.

`using`s: `System`, `System.Collections.Generic`, `System.Linq`, `System.Threading`,
`System.Threading.Tasks`, `System.Windows.Forms`, `Microsoft.Office.Interop.Outlook`,
`QuickFiler.Interfaces`, `UtilitiesCS`, `UtilitiesCS.Extensions`, `UtilitiesCS.Threading`.

Responsibility: removing groups from the page, caching the outgoing page, and the page swap.
This is the largest partial at 348 projected lines; if seam work pushes it past ~430, split
`RemoveSpecificControlGroup(int)` / `RemoveSpecificControlGroupAsync` / the counter (1099-1248,
150 lines) into `QfcCollectionController.RemoveGroup.cs`. That split is pre-authorized here so the
planner does not have to re-derive it.
Independently testable: `RemoveBelowThresholdAsync` is already fully driven through the existing
`_removeGroupByEntryId` seam (`QfcCollectionControllerTests.cs:185-288`). The rest becomes testable
with **S1** (TLP row removal, panel height), **S2** (`UiThread.Dispatcher` at 1195 and 1226) and
**S8** (`(QfcFormController)_parent).SkipGroupAsync()` at 1232).

---

#### 7. `QuickFiler/Controllers/QfcCollectionController.KeyboardWiring.cs` — **~129 lines**

Members: `WireUpAsyncKeyboardHandler` 1275-1280, `RegisterAsyncKeyActions` 1282-1291,
`RegisterAlwaysOnAsyncKeyActions` 1293-1305, `CustomReturnKeyHandler` 1307-1314,
`AnyOpenDropDowns` 1316-1322, `RegisterNavigation` 1330-1341, `UnregisterNavigation` 1343-1356,
`RegisterNavigationAsyncAction` 1358-1361, `GenerateStringKbdAction` 1363-1385.

`using`s: `System`, `System.Collections.Generic`, `System.Threading`, `System.Threading.Tasks`,
`System.Windows.Forms` (`Keys`), `QuickFiler.Interfaces`.

Responsibility: registering/unregistering the "Collection" keyboard action set.
Independently testable: **already is** — `QfcCollectionControllerTests.cs:452-466` and `:474-492`
drive `RegisterNavigation`/`UnregisterNavigation` against a real
`KbdActions<string, KaStringAsync, Func<string, Task>>` behind a Loose `IQfcKeyboardHandler`. This
partial needs **no new seam at all**.

---

#### 8. `QuickFiler/Controllers/QfcCollectionController.Selection.cs` — **~184 lines**

Members: `ActivateByIndex` 1391-1394, `ActivateByIndexAsync` 1396-1399, `ActivateBySelection`
1401-1424, `ActivateBySelectionAsync` 1426-1448, `ChangeByIndex` 1450-1464, `ChangeByIndexAsync`
1466-1484, `SelectNextItem` 1486-1496, `SelectNextItemAsync` 1498-1501, `SelectPreviousItem`
1503-1514, `SelectPreviousItemAsync` 1516-1519, `ScrollIntoView` 1521-1541 (thin),
`ToggleOffActiveItem` 1667-1685, `ToggleOffActiveItemAsync` 1687-1702.

`using`s: `System.Threading.Tasks`, `QuickFiler.Interfaces`, `UtilitiesCS`,
`UtilitiesCS.Threading`.

Responsibility: which item is active, and index/selection arithmetic (the `ActiveSelection =
ActiveIndex + 1` convention).
Independently testable: all fan-out is through the mockable `IQfcItemController`
(`QuickFiler/Interfaces/IQfcItemController.cs:36-49`). Barriers: `itemViewer.LblSubject.Focus()`
(1417) and `ScrollIntoView` (1521-1541) go behind **S1**; the four `UiThread.Dispatcher.InvokeAsync`
sites (1472, 1482, 1500, 1518) go behind **S2**.

---

#### 9. `QuickFiler/Controllers/QfcCollectionController.NavigationToggle.cs` — **~140 lines**

Members: `ToggleExpansionStyle` 1543-1589 (thin), `ToggleExpansionStyleAsync` 1591-1598,
`ToggleOffNavigation` 1600-1613, `ToggleOffNavigationAsync` 1615-1632, `ToggleOnNavigation`
1634-1646, `ToggleOnNavigationAsync` 1648-1665.

`using`s: `System`, `System.Collections.Generic`, `System.Linq`, `System.Threading.Tasks`,
`System.Windows.Forms` (`RowStyle`), `UtilitiesCS`, `UtilitiesCS.Extensions`,
`UtilitiesCS.Threading`.

Responsibility: bulk on/off toggling of navigation across all groups, and the row-height expansion
style.
Independently testable: `ToggleExpansionStyle`'s two guard clauses (1545-1561) are pure and testable
today; the `_itemTlp.RowStyles`/`MinimumSize`/`Invoke` block (1563-1588) goes behind **S1**.

---

#### 10. `QuickFiler/Controllers/QfcCollectionController.Conversation.cs` — **~235 lines**

Members: `ChangeConversationSilently(int, bool)` 1714-1717,
`ChangeConversationSilently(QfcItemGroup, bool)` 1725-1731 (thin), `ToggleGroupConv(string)`
1733-1766, `ToggleGroupConv(int, int)` 1768-1798, `ToggleUnGroupConv` 1808-1847,
`EnumerateConversationMembers` 1875-1922, `PromoteFirstChild` 1970-1985 (thin).

`using`s: `System.Collections.Generic`, `System.Linq`, `Microsoft.Office.Interop.Outlook`,
`QuickFiler.Helper_Classes` (`ConversationResolver`), `UtilitiesCS`, `UtilitiesCS.Extensions`.

Responsibility: collapsing and expanding a conversation into/out of individual item groups.
Independently testable: index arithmetic (`childCount`, `indexOriginal`, `insertionIndex`,
`insertCount`) is pure; the only control touches are the checkbox write at 1729 and the
`SetCellPosition`/`SetColumnSpan` at 1976-1980, both of which go behind **S1**.
`PromoteFirstChild`'s `ref int childCount` is testable directly.

---

#### 11. `QuickFiler/Controllers/QfcCollectionController.Layout.cs` — **~122 lines**

Members: `InsertItemGroups` 2004-2011, `EliminateSpaceForItems` 2013-2027 (thin),
`MakeSpaceForItems` 2029-2042 (thin), `UpdateSelectionNumberForRemoval` 2044-2062,
`RenumberGroups()` 2064-2070, `RenumberGroups(int)` 2072-2078, `ResetPanelHeightAsync` 2080-2090
(thin), `ResetPanelHeight` 2092-2107 (thin).

`using`s: `System`, `System.Linq`, `System.Threading.Tasks`, `System.Windows.Forms` (`RowStyle`),
`UtilitiesCS`.

Responsibility: row-space arithmetic, group renumbering, and selection-index fix-up on removal.
Independently testable: `InsertItemGroups`, `UpdateSelectionNumberForRemoval`, and both
`RenumberGroups` overloads are pure list/index logic reachable with a reflection-injected
`_itemGroups` today — **this is the highest-value, lowest-cost partial in the whole child**.

---

#### 12. `QuickFiler/Controllers/QfcCollectionController.Theme.cs` — **~82 lines**

Members: `SetupLightDark` 2113-2118, `DarkMode_CheckedChanged` 2120-2156, `SetDarkMode` 2158-2164,
`SetLightMode` 2166-2172.

`using`s: `System`, `QuickFiler.Interfaces`, `UtilitiesCS`.

Responsibility: dark/light propagation and the `IOlObjects.PropertyChanged` subscription lifecycle.
Independently testable: **already is** — `QfcCollectionControllerDarkModeTests.cs` raises
`PropertyChanged` on a `Mock<IOlObjects>` at `:112-116` and `:141-145`. Only the post-cleanup
early-return (2125-2128) is currently reached; the sender-typed and `_globals`-fallback branches
(2134-2145) and both `SetDarkMode`/`SetLightMode` fan-outs are one test each away.

---

#### 13. `QuickFiler/Controllers/QfcCollectionController.Move.cs` — **~163 lines**

Members: `MoveEmailsAsync` 2206-2228, `TryMoveEmailByGroupIndexAsync` 2230-2234,
`TryMoveEmailByGroupAsync` 2236-2258, `TryGetItemGroupByIndex` 2260-2270, `GetMoveDiagnostics`
2272-2328, `xComma` 2330-2345.

`using`s: `System`, `System.Linq`, `System.Threading.Tasks`,
`Microsoft.Office.Interop.Outlook` (`AppointmentItem`), `UtilitiesCS`, `UtilitiesCS.Extensions`,
`UtilitiesCS.ReusableTypeClasses.SerializableNew.Concurrent.Observable` (`SloStack`).

Responsibility: executing the batched move and producing the CSV diagnostic lines.
Independently testable: **already is** — `QfcCollectionControllerTests.cs:83-132` drives
`GetMoveDiagnostics`/`TryGetItemGroupByIndex`/`xComma` with a reflection-injected
`_itemGroupsToMove`. `xComma` is a pure static string function.
**Cross-file constraint:** `xComma` must stay `public static` on the type —
`QuickFiler/Controllers/EfcHomeController.Metrics.cs:79` calls
`QfcCollectionController.xComma(itemInfo.Subject)`, and that file belongs to **F8**.

---

#### 14. `QuickFiler/Controllers/QfcCollectionController.LegacyLoadPaths.cs` — **~249 lines**

Members with **no caller anywhere in the repository** (§E1):
`WireUpKeyboardHandler` 1254-1273, `AnyOpenDropDownsAsync` 1324-1328, `LoadGroups_02cAsync`
587-605, `LoadGroups_02bAsync` 635-652, `LoadGroup_03bAsync` 654-738,
`LoadConversationsAndFoldersAsync` 761-774, `LoadItemGroup` 776-796, `LoadSequentialAsync` 827-840,
`LoadGroupSequential` 842-857, `CacheTlpForMove` 865-868, `SwapTlp` 870-874, `CaptureTlpTemplate`
1991-1996.

`using`s: `System`, `System.Collections.Generic`, `System.Linq`, `System.Threading`,
`System.Threading.Tasks`, `System.Windows.Forms`, `Microsoft.Office.Interop.Outlook`,
`QuickFiler.Helper_Classes`, `QuickFiler.Interfaces`, `UtilitiesCS`.

Responsibility: superseded load/wiring paths retained for API compatibility. The file header must
state that no member has a production caller and cite the follow-up removal issue.
Independently testable: each member is called directly from a test; `LoadGroup_03bAsync` (654-738)
is the only hard one because it uses `TaskScheduler.FromCurrentSynchronizationContext()` (662) —
see §F14.

---

### A4. New non-partial seam files

| File | Kind | Projected | Ledger bucket |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/IQfcTlpSurface.cs` | `internal interface` | ~55 | `interface-only / not-measured` (epic.md:509-522) |
| `QuickFiler/Controllers/QfcTlpSurface.cs` | thin WinForms adapter over `TableLayoutPanel`/`Panel` | ~115 | `testable`, >= 90% via STA tests |
| `QuickFiler/Controllers/IQfcItemViewerSurface.cs` | `internal interface` | ~40 | `interface-only / not-measured` |
| `QuickFiler/Controllers/QfcItemViewerSurface.cs` | thin adapter over `ItemViewer` members | ~70 | see §B10 — the only ratified-exemption candidate in this child |

### A5. `QuickFiler.csproj` impact

`QuickFiler/QuickFiler.csproj:311` already carries
`<Compile Include="Controllers\QfcCollectionController.cs" />`. **17 new `<Compile Include>` entries
are required** (13 partials + 4 seam files). Per epic.md:604-612: only this child's entries, minimal
adjacent hunks, CRLF preserved, inserted contiguously immediately after line 311 so the conflict
surface with siblings is a single hunk.

---

## B. Seam inventory

### B0. Barrier taxonomy applied to every member

| Barrier class | Members (line spans) | Count |
| --- | --- | --- |
| **B-COM** — `MailItem`/`AppointmentItem` member access | 283-285, 364, 392, 451, 487, 967, 981, 1055, 1090, 1124, 1187, 1739, 1820, 1884-1885, 1900-1902, 1942, 2247, 2277-2308 | 19 sites |
| **B-CTRL-R** — *reads/writes a control instance* (seamable) | `_itemTlp` (209-227, 929-941, 999, 1034, 1121, 1183, 1566-1588, 1976-1980, 2015-2026, 2031-2041, 2084-2106), `_itemPanel` (1524-1537), `grp.ItemViewer.*` (141-142, 1417, 1729), `itemViewer.*` (926-947), `_itemTlpToMove` (1021) | ~45 sites |
| **B-CTRL-C** — *constructs or shows* a control/window | `ItemViewerQueue.Dequeue` (617, 958), `new EfcHomeController(...).Run()` (972-973), `.RunAsync()` (986-988), `MessageBox.Show` (186-191), `_formViewer.WindowState = Maximized` (289) | 5 sites |
| **B-UI-THREAD** — static `UiThread.Dispatcher` | 1195, 1226, 1472, 1482, 1500, 1518, 1595 | 7 sites |
| **B-SYNCCTX** — `await _formViewer.UiSyncContext` | 1028, 2082 | 2 sites |
| **B-STATIC** — mutable static state | `removespecificcontrolgroupcounter` 1157/1161/1237/1247; `xComma` 2330 (pure static, **no barrier**) | 2 |
| **B-LOG** — `log4net` logger 24-26 | 1239, 2251, 2253 | **no barrier** — `log4net` with no configured appender is a no-op; the existing tests already construct the type |
| **B-NONE** — reachable today | `ActiveIndex`/`ActiveSelection` 86-96, `Token`/`TokenSource` 98-110, `EmailsLoaded` 148, `EmailsToMove` 150, `ItemGroups` 240-247, `SafeSetTlpLayout` 233-238, `ValidateParams` 320-339, `CreateEmptyKbdHandlerCharActions` 581-585, `ActivateQueuedItemGroups` 883-886, `RemoveBelowThresholdAsync` 1076-1097, all of `KeyboardWiring` except nothing, `UpdateSelectionNumberForRemoval` 2044-2062, `RenumberGroups` x2 2064-2078, `InsertItemGroups` 2004-2011, `TryGetItemGroupByIndex` 2260-2270, `xComma` 2330-2345, `SetDarkMode`/`SetLightMode` 2158-2172 | ~30 members |

`log4net` deserves an explicit note: it is **not** a barrier. `QfcCollectionControllerDarkModeTests`
already constructs the type at `:50`, which forces the static initializer at `:24-26` to run, and the
suite passes. No seam is proposed for it.

### B1-B10. Proposed seams

Applied in the mandated order: **interface seam > injectable delegate > adapter**
(`.claude/rules/csharp.md:49-53`).

| # | Seam | Tier | Replaces | Visibility |
| --- | --- | --- | --- | --- |
| **S1** | `IQfcTlpSurface _tlpSurface` + `IQfcItemViewerSurface _viewerSurface` | interface | all **B-CTRL-R** sites | `internal` interfaces, `private` fields |
| **S2** | `UtilitiesCS.Threading.IUiDispatcher _uiDispatcher` | interface (**already exists**) | all 7 **B-UI-THREAD** sites | `private` field |
| **S3** | `Func<CancellationToken, ItemViewer> _itemViewerFactory` | delegate | `ItemViewerQueue.Dequeue` 617, 958 | `private` |
| **S4** | `Func<QfcItemGroup, int, int, TlpCellStates, string, IQfcItemController> _itemControllerFactory` | delegate | `new QfcItemController(...)` 620-630, 681-690, 778-787, 803-812, 844-853, 1853-1862 | `private` |
| **S5** | `Func<MailItem, Task<MailItemHelper>> _helperFactory` | delegate | `MailItemHelper.FromMailItemAsync` 300-305 | `private` |
| **S6** | `Action<string, string> _showError` | delegate | `MessageBox.Show` 186-191 | `private` |
| **S7** | `Func<MailItem, bool, Task> _popOutAsync` | delegate | `new EfcHomeController(...)` 972-973, 986-988 | `private` |
| **S8** | `Func<Task> _skipGroupAsync` | delegate | `((QfcFormController)_parent).SkipGroupAsync()` 1232 | `private` |
| **S9** | `IEmailMoveMonitor` via optional ctor parameter | interface (**already exists**) | field initializer at `:78` | field already `private` |
| **S10** | `Func<string, Task> _removeGroupByEntryId` | delegate (**already present**) | `RemoveSpecificControlGroup(string)` from `RemoveBelowThresholdAsync` | `private`, `:1067` |

**S2 detail.** `UtilitiesCS.Threading.IUiDispatcher` is `public`
(`UtilitiesCS/Threading/IUiDispatcher.cs:15`) with a production adapter
`UtilitiesCS.Threading.WpfUiDispatcher` (`UtilitiesCS/Threading/WpfUiDispatcher.cs:17`) whose
parameterless constructor forwards to `UiThread.Dispatcher` (`:24-25`). The sibling
`QfcItemController` already takes it as an optional constructor parameter
(`QuickFiler/Controllers/QfcItemController.Initialization.cs:38`), and `QuickFiler.Test` already
proves both the mocked (`QfcItemController.TestSupport.cs:102-120`) and real-dispatcher
(`QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs:26-29`) shapes. **No new file, no new
dependency, no contract widening.** This seam is mandatory, not optional: `UiThread.Dispatcher` is a
static `Dispatcher` with a `private set` initialized to `null!`
(`UtilitiesCS/Threading/UiThread.cs:135-140`), assigned only inside `Init()` (`:61`), so in a unit
test every `UiThread.Dispatcher.InvokeAsync(...)` call throws `NullReferenceException`.

**B-SYNCCTX is already seamed.** `IQfcFormViewer.UiSyncContext`
(`QuickFiler/Interfaces/IQfcFormViewer.cs:17`) is an interface member. Note that a `Moq` default of
`null` will throw: `SynchronizationContextAwaiter`'s constructor throws `ArgumentNullException` on a
null context (`UtilitiesCS/Threading/UiThread.cs:93-96`). Tests must set up `UiSyncContext` to
return a real `SynchronizationContext` whose `Post` executes inline. No production change required.

**S1 detail — why two interfaces, not one.** Splitting the adapter along the
`TableLayoutPanel`/`Panel` boundary versus the `ItemViewer` boundary matters for the coverage gate:

- `QfcTlpSurface` operates on plain `TableLayoutPanel`/`Panel`/`RowStyle`. `UtilitiesCS.Test`
  already proves `new TableLayoutPanel()` plus `InsertSpecificRow`/`RemoveSpecificRow` work on an
  STA thread (`UtilitiesCS.Test/HelperClasses/TableLayoutHelper_Tests.cs:11-23`,
  `UtilitiesCS.Test/HelperClasses/WindowsForms/ScreenAndTableLayoutTests.cs:41-54`), so >= 90% is
  achievable with the STA-thread helper described in §G2.
- `QfcItemViewerSurface` operates on `ItemViewer` members (`LblItemNumber`, `LblSubject`,
  `ConversationMenuItem`, `Parent`, `Dock`, `AutoSize`, `BorderStyle`). A designer-initialized
  `ItemViewer` is 6,224 lines of generated code (epic.md:113-114) carrying a WebView2 surface. This
  is the **only** genuine irreducible-remainder candidate in the child.

**Non-seam noted for completeness.** `TableLayoutHelper.InsertSpecificRow`/`RemoveSpecificRow` are
`public static` extension methods on `TableLayoutPanel`
(`UtilitiesCS/HelperClasses/Windows Forms/TableLayoutHelper.cs:13, 55`), and both begin with
`panel.InvokeRequired` (`:21`, `:62`), so they dereference a null panel. They are called from the
controller at 999, 1034, 1121, 1183, 2015, 2036 — all six sites move behind **S1**.

### B11. Public-surface impact — F7's conclusion is preserved

**Confirmed: the proposed seam set adds nothing to `IQfcCollectionController`.**

- No member is added to `QuickFiler/Interfaces/IQfcCollectionController.cs` (118 lines, unchanged).
- All ten seams are `private` fields on the class or `internal` interfaces in
  `QuickFiler.Controllers`.
- The only public-surface change is **optional trailing constructor parameters**. All three
  production construction sites — `QuickFiler/Controllers/QfcFormController.Actions.cs:49`, `:83`,
  `:139` — pass seven named arguments plus one positional (`_states`) and therefore **compile
  unchanged**. Those files are F6-owned and are not edited.
- `QuickFiler/Controllers/QfcItemGroup.cs:12` carries a vestigial
  `using static QuickFiler.Controllers.QfcCollectionController;`. A `partial` split does not affect
  it (a `using static` binds to the type, not the file). No F2 edit required.
- `xComma` stays `public static` for `EfcHomeController.Metrics.cs:79` (F8).

**Cross-child note to record:** F11 introduces two new `internal` interfaces in the
`QuickFiler.Controllers` namespace (`IQfcTlpSurface`, `IQfcItemViewerSurface`). No sibling consumes
them. F7 (#433) needs no contract additions from this controller, and that conclusion **remains
true**.

---

## C. What the existing tests actually reach

### C1. `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` (500 lines)

Construction strategy: `FormatterServices.GetUninitializedObject(typeof(QfcCollectionController))`
(`:37`, `:148`, `:255`, `:344`) — bypasses the constructor **and every field initializer**, then
injects private fields by reflection (`:69`, `:167`, `:178`, `:262`, `:380-383`). No STA, no real
form. Note the consequence at `:334-336`: because field initializers are bypassed, `_digits` must be
manually set to 1 or `Digits` (`:114-128`) flips `_digitRefreshNeeded` and drags
`RegisterNavigation` into the WinForms-bound `SetVisualDigits`.

Production members exercised, by name:

| Member | Lines | Depth |
| --- | --- | --- |
| `GetMoveDiagnostics` | 2272-2328 | one group, null appointment: covers the loop body and the `olAppointment is null` path; the appointment-non-null branches 2297-2309 and the `else` at 2318-2322 are **not** reached |
| `TryGetItemGroupByIndex` | 2260-2270 | success path only |
| `xComma` | 2330-2345 | non-empty path only (via 2312/2316) |
| `RemoveBelowThresholdAsync` | 1076-1097 | fully, all branches including the null-`_itemGroups` guard |
| `RemoveGroupByEntryId` | 1069-1074 | the `??=` already-assigned path only |
| `LoadControlsAndHandlers_01(TableLayoutPanel, List)` | 253-266 | fully |
| `ActivateQueuedTlp` | 859-863 | fully |
| `SwapItemGroups` | 888-896 | fully |
| `CacheItemGroupsForMove` | 876-881 | fully |
| `ActivateQueuedItemGroups` | 883-886 | fully |
| `RegisterNavigation` | 1330-1341 | `_digitRefreshNeeded == false` path |
| `UnregisterNavigation` | 1343-1356 | `Digits == 1` path only |
| `RegisterNavigationAsyncAction` | 1358-1361 | fully |
| `GenerateStringKbdAction` | 1363-1385 | `digits == 1` path only |
| `Digits` | 114-128 | the no-change path |
| `ActiveIndex` setter | 87-91 | via `:265` |
| `ItemGroups`/`_itemGroups` | 240-247 | field only, not the property |

### C2. `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs` (155 lines)

Construction strategy: the **real** constructor (`:50-59`) with `Mock<IApplicationGlobals>`,
`Mock<IOlObjects>`, `Mock<IQfcFormViewer>`, `Mock<IQfcKeyboardHandler>`,
`Mock<IFilerHomeController>`, `Mock<IFilerFormController>`, a real `CancellationTokenSource`, and a
real `new TlpCellStates()`. This proves the constructor is already fully mockable. No STA.

Members exercised: ctor 30-53, `SetupLightDark` 2113-2118, `Cleanup` 2192-2204, `CleanupAsync`
2178-2190, `RemoveControls` 991-1011 (null-`_itemGroups` early exit only), `RemoveControlsAsync`
1024-1044 (same), `DarkMode_CheckedChanged` 2120-2156 (**only** the `_formViewer is null`
early-return at 2125-2128).

### C3. Source compatibility with the partial split

A partial-class split is source-compatible with both test files and **neither will need editing**:

- Both address the type by name (`typeof(QfcCollectionController)`), never by file.
- All reflection lookups use `BindingFlags.NonPublic | BindingFlags.Instance` against
  `typeof(QfcCollectionController)`; field/method location within partial files is irrelevant to
  reflection.
- `FormatterServices.GetUninitializedObject` operates on the type.
- The seam work in §B adds **optional** constructor parameters only, so
  `QfcCollectionControllerDarkModeTests.cs:50-59` (8 positional arguments) still binds.

One caveat the planner must honor: `QfcCollectionControllerTests.cs:359` injects `_moveMonitor` by
reflection. Seam **S9** must keep the field named `_moveMonitor` (adding constructor injection that
assigns the same field), or that test breaks.

### C4. Test-policy audit of the two existing files

| Check | `QfcCollectionControllerTests.cs` | `QfcCollectionControllerDarkModeTests.cs` |
| --- | --- | --- |
| 500-line limit | **500 lines exactly — compliant, zero headroom** | 155 — compliant |
| `DateTime.Now`/`UtcNow` | none (only `new DateTime(2026,1,1)` literals at `:44`, `:100`, `:126`) | none |
| `Thread.Sleep` / `Task.Delay` | none | none |
| Unseeded `Random` | none | none |
| Temporary files | none | none |
| Live forms / popups | none (`IQfcFormViewer` mocked; `L1v0L2L3v_TableLayout` returns `null` at `:354-356`) | none |
| MSTest + Moq + FluentAssertions | yes | yes |
| Arrange-Act-Assert | yes | yes |

**No violation found. One in-scope obligation:** `QfcCollectionControllerTests.cs` is at exactly the
500-line ceiling, so **no new test may be added to it**. All new tests go in new per-partial files
(§F). If the planner wants headroom, splitting the existing file along the same partial boundaries
is optional and low-risk — but it is not required by policy today.

### C5. Realistic coverage floor once the exemption is removed

**Estimate: 12%-20% line coverage, most likely near 15%.**

Basis:
- Roughly 95 members exist on the type. The existing tests touch **24** of them (§C1, §C2), and
  eight of those only along one branch or one early-return.
- The touched members are disproportionately small: `ActivateQueuedTlp` (5 lines),
  `ActivateQueuedItemGroups` (4), `CacheItemGroupsForMove` (6), `RegisterNavigationAsyncAction` (4).
- The untouched members are disproportionately large: `LoadGroup_03bAsync` (85),
  `RemoveSpecificControlGroupAsync` (90), `LoadControlsAndHandlers_01Async` x2 (78 + 88),
  `GetMoveDiagnostics`'s unreached branches, `LoadSecondaryAsync` (55),
  `ToggleExpansionStyle` (47), `EnumerateConversationMembers` (48).
- Summing the covered spans conservatively gives ~150-200 executed statements against a plausible
  ~1,100-1,250 sequence points for a 2,349-line file with this comment/brace density.

The number that matters for planning: **the child must add roughly 65-80% of line coverage from
zero-ish**, across 13 partials, to clear the 80% per-file gate.

---

## D. Concurrency and ordering invariants

### D1. Invariants the split must not disturb

| # | Invariant | Evidence |
| --- | --- | --- |
| D1.1 | `_formViewer.SuspendLayout()` precedes group mutation and `ResumeLayout()` follows it | 258/264, 275/292, 353/409-413, 440/499-505 |
| D1.2 | `TlpLayout` is saved, set `false`, and restored to the **saved** value (not `true`) | `SafeSetTlpLayout` 233-238 used at 276, 354, 441, 1490, 1617, 1650, 1815, 1927; restore at 290, 406, 498, 1149, 1228, 1462, 1494, 1512, 1631, 1664, 1797, 1846, 1967 |
| D1.3 | `UnregisterNavigation()` precedes any `_itemGroups` mutation and `RegisterNavigation()` follows it | `SwapItemGroups` 890/895, `RemovedItemMonitor` 1048/1050, `ToggleGroupConv` 1773/1796, `ToggleUnGroupConv` 1817/1845, `AddItemGroup` 1926/1966 |
| D1.4 | Below-threshold removal captures all `EntryID`s **before** removing any, so renumbering cannot cause index drift mid-iteration | 1086-1096 (comment at 1086-1087 is explicit) |
| D1.5 | Removal order: toggle off active -> update selection number -> suspend layout -> remove TLP row -> unhook move monitor -> remove from list -> renumber -> restore | 1108-1149 and 1165-1228 |
| D1.6 | Renumbering happens **after** removal, and `Digits` is re-evaluated before renumbering on the async path only | 1195-1203 vs 1132 (sync path does not refresh digits) |
| D1.7 | The trailing `RegisterNavigation()` is skipped when the zero-item branch already re-registered through `SkipGroupAsync -> LoadControlsAndHandlers_01 -> SwapItemGroups` (issue #232) | 1221-1225, 1233, 1243-1246 |
| D1.8 | `BackgroundLoadingTasks` is awaited to completion and then reset before keyboard wiring | 398-399 and 492-493, both before `WireUpAsyncKeyboardHandler` at 403/495 |
| D1.9 | `LoadSecondaryAsync` correlates each completed task back to its group **by index into the original task list**, not by completion order | 556-573 |
| D1.10 | `PopOutControlGroup` reads the `MailItem` **before** removing the group | 967 then 970; 981 then 984 |
| D1.11 | Item numbers are 1-based; indexes are 0-based; `ActiveSelection == ActiveIndex + 1` | 92-96, and every `selection - 1` at 967, 981, 1108-1109, 1124, 1127, 1165-1166, 1187, 1190 |

### D2. Sync-over-async and fire-and-forget patterns

| Pattern | Location | Note |
| --- | --- | --- |
| `Task.Run` fire-and-forget into a `ConcurrentBag` | 361-367, 370, 448-454, 457 | Awaited later at 398/492, so not truly orphaned, but see §E16 |
| `Task.Factory.StartNew(...).ContinueWith(...).Unwrap()` chain on the UI `TaskScheduler` | 654-738 (`LoadGroup_03bAsync`) | Dead code; uses `TaskScheduler.FromCurrentSynchronizationContext()` at 662, which throws `InvalidOperationException` when `SynchronizationContext.Current` is null |
| `Task.Factory.StartNew` returning a `Task` that is never unwrapped (`AttachedToParent`) | 709-725 | `PopulateConversationAsync`/`PopulateFolderComboBoxAsync` return `Task`s that `StartNew` wraps as `Task<Task>`; `Task.WhenAll(subTasks)` at 727 therefore completes when the *outer* task completes, not the inner one. Dead code, but a real ordering bug if ever revived |
| `async` lambda passed to `Dispatcher.InvokeAsync` | 1226-1236 | `InvokeAsync(async () => ...)` returns as soon as the lambda's first `await` yields; the `await` on line 1226 does **not** wait for `SkipGroupAsync` (1232) to complete. Directly relevant to invariant D1.7 |
| `Task.Run(() => grp.ItemController.PopulateConversation())` | 791-792, 855-856 | Marshals COM work off the UI thread; dead code |
| `Interlocked` counter without `finally` | 1161 / 1247 | See §E6 |
| `[MethodImpl(MethodImplOptions.Synchronized)]` (lock on `this`) | 116, 199, 201, 243, 245 | `Digits`, `TlpLayout` get/set, `ItemGroups` get/set. Public lock object; a caller holding a lock on the controller instance can deadlock against these |

### D3. Banned determinism-hostile APIs in the production file

**None.** No `DateTime.Now`, `DateTime.UtcNow`, `DateTimeOffset.Now`, `Thread.Sleep`, `Task.Delay`,
or `Random` appears in `QuickFiler/Controllers/QfcCollectionController.cs`. Nothing to remediate
under the banned-API rule.

### D4. Ordering constraints on the tests themselves

- Tests that use `ItemViewerQueue.SetCoreForTesting` mutate process-global static state and must
  carry `[DoNotParallelize]` plus a `[TestCleanup]` calling
  `ItemViewerQueue.ResetProductionCoreDefaultsForTesting()` and `ResetCoreForTesting()` — the exact
  pattern at `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs:11-22`. Seam **S3**
  exists precisely so most tests do not need this.
- `removespecificcontrolgroupcounter` (1157) is `static` and therefore shared across test methods.
  Any test that drives `RemoveSpecificControlGroupAsync` must reset it by reflection in
  `[TestInitialize]`, or the `> 1` log branch at 1237-1242 becomes order-dependent.

---

## E. Latent defect inventory

### E1. Twelve unreachable members — **PROMOTE TO ISSUE**

No caller anywhere in `QuickFiler`, `QuickFiler.Test`, or any other project:
`WireUpKeyboardHandler` (1254), `AnyOpenDropDownsAsync` (1324), `LoadGroups_02cAsync` (587),
`LoadGroups_02bAsync` (635), `LoadGroup_03bAsync` (654), `LoadConversationsAndFoldersAsync` (761),
`LoadItemGroup` (776), `LoadSequentialAsync` (827), `LoadGroupSequential` (842), `CacheTlpForMove`
(865), `SwapTlp` (870), `CaptureTlpTemplate` (1991). `LoadGroups_02bAsync` is referenced only from a
commented-out line (402). `_templateTlp` (`:70`) is written only by the dead `CaptureTlpTemplate`.
Severity: low functional risk, **high coverage cost** — ~227 lines of the denominator that no
production path exercises. Disposition: **PROMOTE** (removal is a public-API change, out of scope
under the no-behavior-change NFR); isolate in `LegacyLoadPaths.cs` this cycle.

### E2. Issue #444 — duplicate `KaKey` registration (1265-1272) — **CHARACTERIZE ONLY**

Precise current behavior, for a characterization test:

1. `WireUpKeyboardHandler` builds `new KbdActions<Keys, KaKey, Action<Keys>>(new List<KaKey> {...})`
   with three entries: `("Collection", Keys.Up)`, `("Collection", Keys.Down)`,
   `("Collection", Keys.Down)` (1265-1272).
2. The `IEnumerable` constructor `KbdActions(IEnumerable<UClass> list)`
   (`QuickFiler/Controllers/KbdActions.cs:26-29`) performs **no duplicate check** — it is a plain
   `new List<UClass>(list)`. **Construction therefore succeeds**; the collection holds three entries.
   (The duplicate check lives only in the two `Add` overloads at `KbdActions.cs:90-104` and
   `:106-121`.)
3. `Find(Keys.Down)` (`KbdActions.cs:53-69`) matches two entries, falls to `default:`, and throws
   `InvalidOperationException` with the message
   `"Multiple sources have registered actions for Key Down. SourceId list [Collection and Collection]"`
   (message built at `:64-66` using `SentenceJoin`).
4. The indexer `this[Keys.Down]` (`:36-38`) throws through `Find`.
5. `FindIndex(Keys.Down)` (`:71-88`) logs the same message via `logger.Error` at `:85` and throws.
6. `ContainsKey(Keys.Down)` (`:49`) returns `true`; `FilterKeys(Keys.Down)` (`:51`) returns **both**
   entries without throwing.
7. **New finding to add to #444:** `WireUpKeyboardHandler` has **no caller** (§E1). The defect is
   dormant, not live. Production wires keys through `WireUpAsyncKeyboardHandler` (1275-1280) ->
   `RegisterAsyncKeyActions` (1282-1291), which registers `Keys.Up`/`Keys.Down` exactly once each.
   Severity should be downgraded accordingly, and the fix folded into the dead-code removal in E1.

Do **not** fix. Characterize with a test asserting (a) the constructor does not throw, (b)
`FilterKeys(Keys.Down).Length == 2`, (c) `Find(Keys.Down)` throws `InvalidOperationException`.

### E3. `EliminateSpaceForItems` sign error (2013-2027) — **PROMOTE TO ISSUE**

`heightChange` is assigned a **negative** value at 2017
(`-(int)Math.Round(_template.Height * removalCount, 0)`), then subtracted at 2020 and 2025
(`MinimumSize.Height - heightChange`, `Size.Height - heightChange`). Subtracting a negative
**increases** the height when rows are removed. The sibling `MakeSpaceForItems` (2029-2042) uses a
positive value and `+`. Failure mode: the item table grows every time a conversation is collapsed.
Severity: medium (visual layout defect on the `ToggleGroupConv` path at 1779).

### E4. `GetMoveDiagnostics` null-guard is unreachable (2288-2322) — **PROMOTE TO ISSUE**

`var qf = TryGetItemGroupByIndex(k)?.ItemController;` (2288) may yield `null`, but `qf` is
dereferenced immediately at 2289 (`qf.ItemHelper`) and again at 2312
(`xComma(qf.ItemHelper.Subject)`), **before** the `if (qf is not null)` check at 2313. The `else`
branch at 2318-2322 is therefore dead. The issue-#97 guard documented in
`QfcCollectionControllerTests.cs:77-80` protects the `olAppointment` parameter, not this path.
Severity: medium.

### E5. `GetMoveDiagnostics` returns a trailing null element (2284) — **PROMOTE TO ISSUE**

`new string[_itemGroupsToMove.Count + 1]` allocates one extra slot; the loop at 2286 fills indices
`0..Count-1`, leaving `strOutput[Count] == null`. Consumers are
`QuickFiler/Controllers/QfcHomeController.Metrics.cs:75` and `:144` (F7-owned). Severity:
low-medium; depends on whether the consumer writes the null line.

### E6. Positional access into a `ConcurrentDictionary` (2260-2270) — **PROMOTE TO ISSUE**

`TryGetItemGroupByIndex` does `_itemGroupsToMove.ElementAt(index).Key` (2264). `_itemGroupsToMove`
is a `ConcurrentDictionary<QfcItemGroup, int>` (`:71`) whose enumeration order is unspecified and
not stable across mutations. `MoveEmailsAsync` (2220-2223) and `GetMoveDiagnostics` (2286-2288) each
walk `0..Count-1` independently, so a diagnostic line can be attributed to the wrong message.
Severity: medium.

### E7. Static counter leak and false race log (1157, 1161, 1237-1247) — **PROMOTE TO ISSUE**

`private static int removespecificcontrolgroupcounter` (1157) is **process-global across all
controller instances**, so two independent controllers running concurrently trip the
`> 1` check at 1237 and log `"RemoveSpecificControlGroupAsync: Counter is greater than 1. Race
Condition Exists"` (1239-1241) with no actual race. Worse, `Interlocked.Decrement` at 1247 is not in
a `finally`: any throw between 1161 and 1247 leaks the counter permanently, after which **every**
subsequent call logs the error. The read at 1237 is also a plain, non-volatile read of a field
mutated by `Interlocked`. Severity: medium.

### E8. Concrete-type cast to a sibling controller (1232) — **IN SCOPE (seam) + PROMOTE (design)**

`await ((QfcFormController)_parent).SkipGroupAsync();` casts `_parent`, declared as
`IFilerFormController` (`:64`), to the concrete `QfcFormController`. `SkipGroupAsync` is declared on
`QuickFiler.Controllers.IQfcFormController` (`QuickFiler/Controllers/IQfcFormController.cs:38`), a
**different** interface from `QuickFiler.Interfaces.IFilerFormController`
(`QuickFiler/Interfaces/IFilerFormController.cs:9-24`), which does not declare it. Any other
implementation throws `InvalidCastException`. **In scope:** seam **S8** wraps the call in a delegate
whose default body keeps the cast verbatim, so production behavior is bit-identical and tests can
inject. **Promote:** the underlying two-interface split. `IQfcFormController.cs` is F6-owned; do not
edit it.

### E9. Hard-wired `EmailMoveMonitor` construction (`:78`) — **IN SCOPE**

`private IEmailMoveMonitor _moveMonitor = new EmailMoveMonitor();` is a field initializer and the
field is **never reassigned anywhere in the file**. The existing test reaches it only by reflection
(`QfcCollectionControllerTests.cs:359`). The interface already exists
(`QuickFiler/Interfaces/IEmailMoveMonitor.cs:13`), so seam **S9** is a one-line optional constructor
parameter. Keep the field name `_moveMonitor` (see §C3).

### E10. Modal `MessageBox` inside a property getter (152-194) — **IN SCOPE (seam) + PROMOTE (design)**

`ReadyForMove`'s getter shows a modal dialog at 186-191 when any group lacks a folder. A property
getter with a modal side effect is both a design defect and a hard unit-test-policy barrier
("never show popups", epic.md:230-232). **In scope:** seam **S6**. **Promote:** converting the
side-effecting getter to a method returning a result object is a behavior change and is deferred.

### E11. `ToggleGroupConv(string)` can index `_itemGroups[-1]` (1743-1749) — **PROMOTE TO ISSUE**

When the original message has been removed, `indexOriginal == -1` at 1743 and `PromoteFirstChild`
(1970) runs. If **no** group carries `ConvOriginID == originalId`, `FindIndex` at 1972 also returns
`-1` and line 1975 evaluates `_itemGroups[-1].ItemViewer`, throwing
`ArgumentOutOfRangeException`. `ChangeConversationSilently(indexOriginal, true)` at 1749 would fail
the same way. Severity: medium.

### E12. `EnumerateConversationMembers` count mismatch (1875-1922) — **PROMOTE TO ISSUE**

`ToggleUnGroupConv` reserves `insertCount = conversationCount - 1` slots (1823, 1827-1829), but
`EnumerateConversationMembers` iterates `insertions.Count` (1888-1889), derived independently from
`resolver.ConversationItems.SameFolder` (1883-1886). If the resolver returns more members than
`conversationCount - 1`, `_itemGroups[i + insertionIndex]` (1893) walks past the reserved slots and
re-initializes existing groups; if it returns fewer, empty placeholder `QfcItemGroup`s (created at
2008) are left with a `null` `ItemController`, which the next `RenumberGroups` (2068) dereferences.
Severity: medium.

### E13. `SetVisualDigits` inconsistent null handling (138-143) — **PROMOTE TO ISSUE**

Line 140 dereferences `grp.ItemController.ItemNumberDigits` unguarded, while lines 141-142 use
`grp.ItemController?.ItemNumber... ?? 0.ToString(format)`. If `ItemController` can be null (it can —
see E12), line 140 throws before the guard at 141 is ever reached. Same defect shape as E4.
Severity: low-medium.

### E14. `Digits` evaluated per-iteration in `UnregisterNavigation` but once in `RegisterNavigation` — **PROMOTE TO ISSUE**

`RegisterNavigation` (1330-1341) captures `var digits = Digits;` once at 1332 and passes it to every
`RegisterNavigationAsyncAction`. `UnregisterNavigation` (1343-1356) re-evaluates the
side-effecting `Digits` **inside** the loop at 1347. `Digits` (114-128) reads `_itemGroups?.Count`
live and flips between 1 and 2 at the 10-item boundary. If the count crosses 10 between register and
unregister, keys registered as `"01".."09"` are removed as `"1".."9"`; `KbdActions.Remove`
(`KbdActions.cs:123-135`) returns `false` **silently**, leaving orphaned keys that later collide in
`Add` (`:90-98`) with `ArgumentException`. This is the same failure family as issue #232.
Severity: medium.

### E15. `MoveEmailsAsync` ignores its parameter (2206-2228) — **PROMOTE TO ISSUE**

`stackMovedItems` (`SloStack<IMovedMailInfo>`) is declared on the interface
(`QuickFiler/Interfaces/IQfcCollectionController.cs:50`) and supplied by
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs:225` as `_movedItems`, but the method
body never reads it. Either the undo record is populated elsewhere or it is silently dropped.
Severity: needs triage before a severity can be assigned.

### E16. `BackgroundLoadingTasks` reset race (398-399, 492-493) — **PROMOTE TO ISSUE**

`await Task.WhenAll(BackgroundLoadingTasks); BackgroundLoadingTasks = [];` replaces the
`ConcurrentBag` **reference**. Any `Add` performed by a concurrently-running load between the
`WhenAll` and the assignment is discarded and never awaited. The field is `internal` (`:80`) but has
no consumer outside this file, so a fix is locally contained. Severity: low-medium.

### E17. `TryMoveEmailByGroupAsync` double-logs and swallows cancellation (2236-2258) — **NOTE ONLY**

A `null` `group` (returned by `TryGetItemGroupByIndex` at 2268) causes an NRE at 2240, caught by the
broad `catch (System.Exception)` at 2242, then a second NRE at 2247 caught at 2249 — producing two
misleading error logs instead of one clear failure. The broad catch also swallows
`OperationCanceledException`. Severity: low.

### E18. Test-policy findings — **IN SCOPE**

`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` is at exactly 500 lines: compliant,
but with zero headroom. No new test may be added to it. This is an in-scope constraint on the plan,
not a violation to remediate.

### E19. Unused `using` directives — **IN SCOPE**

`using System.Net.NetworkInformation;` (`:6`) and `using System.Windows;` (`:10`) are unused (§G5).
Both are dropped as part of the split, which is a formatting/cleanliness change with no behavior
impact.

---

## F. Test strategy per proposed partial

Conventions for every case: MSTest `[TestClass]`/`[TestMethod]`, Moq, FluentAssertions, AAA, no
temporary files, no live forms, no popups. Test files mirror the partial names under
`QuickFiler.Test/Controllers/`. **All new tests go in new files** (§E18).

The STA set is deliberately minimized to **two** files (F15a, F15b); everything else is plain
`[TestClass]` with Moq.

### F1. `QfcCollectionControllerTests.Construction.cs` (root file)
Pure MSTest+Moq. Extends the existing `CreateController` shape from
`QfcCollectionControllerDarkModeTests.cs:31-60`.
- ctor stores each collaborator (assert by reflection on `_globals`, `_formViewer`, `_homeController`,
  `_parent`, `_tlpStates`, `_token`, `_tokenSource`, `_initType`).
- ctor reads `_formViewer.L1v0L2L3v_TableLayout` into `_itemTlp` and `L1v0L2_PanelMain` into
  `_itemPanel` (44-45).
- ctor takes `_kbdHandler` from `_homeController.KeyboardHandler` (49).
- ctor calls `SetupLightDark(_globals.Ol.DarkMode)` for both `true` and `false` (52).
- Each of the 10 seams: default is non-null when not injected; injected instance is used.
- `Cleanup` / `CleanupAsync`: nulls `_formViewer`/`_globals`/`_parent`/`_itemTlp`/`_itemGroups`;
  unsubscribes `PropertyChanged`; the `_globals?.Ol is null` branch (2182, 2196).

### F2. `QfcCollectionControllerStateTests.cs`
Pure MSTest+Moq (mocked `IQfcTlpSurface`, `IQfcItemViewerSurface`).
- `ActiveIndex`/`ActiveSelection` round-trip and the `+1/-1` relationship (86-96).
- `Token`/`TokenSource` round-trip (98-110).
- `Digits`: `Count < 10 -> 1`; `Count >= 10 -> 2`; `_itemGroups == null -> 1` (119); the
  change path sets `_digitRefreshNeeded` (122-124); the no-change path does not.
- `SetVisualDigits`: `EmailsLoaded == 0` skips the loop (132); format string is `"0"` for 1 digit
  and `"00"` for 2 (134-137); `_digitRefreshNeeded` is cleared (145); verify the surface receives
  the formatted text per group.
- `EmailsLoaded` / `EmailsToMove`: null and non-null (148, 150).
- `ReadyForMove`: all-assigned -> `true`, no error shown; one `null` `SelectedFolder` -> `false` +
  one `_showError` call; each of the three header sentinels (164-168) -> `false`; assert the
  notification text contains item number, date and subject (176-182).
- `TlpLayout`: no-op when unchanged (205); `true` -> `ResumeLayout` via surface; `false` ->
  `SuspendLayout`; both `InvokeRequired` branches (209-227).
- `SafeSetTlpLayout` returns the **previous** value (233-238).
- `ItemGroups` get/set (241-247).

### F3. `QfcCollectionControllerLoadSyncTests.cs`
Pure MSTest+Moq. The `(tlp, itemGroups)` overload is already proven testable
(`QfcCollectionControllerTests.cs:409-492`).
- `LoadControlsAndHandlers_01(tlp, groups)`: hooks every incoming group's mail into the move monitor
  (255-257); suspends/resumes the viewer; routes through `SwapItemGroups`; sets `ActiveIndex = -1`
  (265).
- `LoadControlsAndHandlers_01(items, template, templateExpanded)`: saves `_template`/
  `_templateExpanded` (279-280); hooks each mail (283-285); calls `LoadItemGroupsAndViewers_02` and
  `LoadConversationsAndFolders_04`; sets `WindowState = Maximized` (289); restores `TlpLayout`.
- `LoadItemGroupsAndViewers_02`: creates one group per item; resets both `CharActions` collections
  (743-744); empty input -> empty list.
- `LoadSequential_5`: 1-based `++i` numbering (808); dark vs light branch (816-823).

### F4. `QfcCollectionControllerLoadAsyncTests.cs`
Pure MSTest+Moq, with **S5** returning a completed `MailItemHelper` mock (the existing tests already
mock `MailItemHelper` at `QfcCollectionControllerTests.cs:40-44`).
- `ValidateParams`: null `items`/`template`/`templateExpanded` each throw; `InvokeRequired == true`
  throws `InvalidOperationException` whose message names `LoadControlsAndHandlers_01Async` (334);
  a cancelled token throws `OperationCanceledException` (338).
- `GetPartiallyInitializedHelperAsync`: null `mailItem` throws; the returned helper is the factory's
  value; the seven property touches at 308-314 do not throw for a loose mock.
- `LoadControlsAndHandlers_01Async(IList<MailItem>)`: digits 1 vs 2 at the 10-item boundary (373);
  one group per item; `InitializeGraphicsAsync` awaited once per group (384); helper-to-group
  correlation by `EntryID` (392); `BackgroundLoadingTasks` empty after completion (399);
  `WireUpAsyncKeyboardHandler` called (403); both `InvokeRequired` branches on resume (407-414).
- `LoadControlsAndHandlers_01Async(IList<QfcPreScoredItem>)`: `PredeterminedFolder` reaches
  `EncapsulateItemGroup` (471) — this generalizes the shape-only assertion currently at
  `QfcCollectionControllerTests.cs:303-326` into a real call-path assertion.
- `LoadSecondaryAsync`: cancelled token throws (528); conversation completion calls
  `RenderConversationCount` (565); folder completion calls `AssignFolderComboBox` (572); a foreign
  task throws `InvalidOperationException` (576) — reachable by injecting the group list such that
  neither list contains the completed task.
- `CreateEmptyKbdHandlerCharActions`: both collections replaced (583-584).

### F5. `QfcCollectionControllerGroupFactoryTests.cs`
Pure MSTest+Moq using seams **S3**/**S4**; no static mutation needed.
- `EncapsulateItemGroup`: `PredeterminedFolder` carried (616); viewer taken from **S3**;
  `LoadItemToTlp` invoked with `(i, template, true, 0)` (618); controller built with
  `viewerPosition == i + 1` (625); `Token` propagated (631).
- `LoadItemViewer_03`: returns the dequeued viewer and places it (958-961).
- `LoadItemToTlp` (thin): forwards `columnNumber == 0` and `columnNumber != 0` to the surface.
- `InitializeGroup`: `child: true` -> column 1 and `IsChild == true` (1851, 1863); `child: false` ->
  column 0.
- `AddItemGroup`: unregisters then registers navigation (1926, 1966); appends at `_itemGroups.Count`
  (1929); hooks the move monitor (1942); `_digitRefreshNeeded` branch (1936-1939); dark and light
  branches (1957-1964); `KbdActive` true/false branch (1950).

### F6. `QfcCollectionControllerRemovalTests.cs`
Pure MSTest+Moq using **S1**, **S2** (`Mock<IUiDispatcher>` executing inline, per
`QfcItemController.TestSupport.cs:102-120`), **S7**, **S8**.
- `RemoveBelowThresholdAsync`: already covered; port the six existing cases verbatim into the new
  file only if the existing file is split — otherwise leave them where they are.
- `RemoveSpecificControlGroup(string)`: match found -> delegates with the group's `ItemNumber`;
  no match -> no-op (1056-1057).
- `RemovedItemMonitor`: unregister -> remove -> register ordering (1048-1050).
- `RemoveSpecificControlGroup(int)`: active vs inactive (1110); expanded vs collapsed (1139);
  `Count > 0` renumber path (1129-1143); `Count == 0 && KbdActive` toggles the keyboard dialog
  (1144-1147); `Count == 0` calls `_parent.ActionOkAsync()` (1151-1154); move monitor unhooked
  (1124).
- `RemoveSpecificControlGroupAsync`: same matrix, plus the digit-refresh branch (1197-1201), the
  zero-item `SkipGroupAsync` branch setting `swapAlreadyRegistered` (1230-1235), the guarded
  trailing register (1243-1246), and the counter `> 1` log branch (1237-1242) driven by
  pre-seeding the static counter by reflection.
- `RemoveControls` / `RemoveControlsAsync`: null `_itemGroups` early exit (already covered);
  non-null path calls `Cleanup()` per group, clears the list, calls `UnhookAll` (1007 — note the
  async variant at 1024-1044 does **not** call `UnhookAll`; assert that asymmetry as a
  characterization).
- `CleanupBackground`: null and non-null `_itemGroupsToMove` and `_itemTlpToMove` (1015-1021).
- `PopOutControlGroup`/`Async`: reads the mail before removing (967/970); delegates to **S7**;
  cancelled token throws (978).
- `SwapItemGroups`/`CacheMoveObjects`/`ActivateQueuedTlp`: already covered; add
  `CacheMoveObjects` (898-902).

### F7. `QfcCollectionControllerKeyboardWiringTests.cs`
Pure MSTest+Moq with a **real** `KbdActions` — no seam needed at all (the existing tests prove this
at `QfcCollectionControllerTests.cs:338-365`).
- `WireUpAsyncKeyboardHandler` calls all three registrars (1277-1279).
- `RegisterAsyncKeyActions`: exactly two entries, `Keys.Up` -> `SelectPreviousItemAsync`,
  `Keys.Down` -> `SelectNextItemAsync` (1287-1288).
- `RegisterAlwaysOnAsyncKeyActions`: one `Keys.Return` entry (1302).
- `CustomReturnKeyHandler`: `AnyOpenDropDowns` returns `false` (1321) so `ActionOkAsync` is always
  called (1312) — characterize the #351 always-clear gate.
- `AnyOpenDropDowns` returns `false` for every input (1319-1322).
- `RegisterNavigation`: `_digitRefreshNeeded` true and false (1333-1336); one action per group.
- `UnregisterNavigation`: `Digits == 1` and `Digits == 2` key formats (1349, 1353).
- `GenerateStringKbdAction`: `digits == 1`, `digits == 2`, and the **unhandled `digits == 3`** case
  leaving `key == ""` (1366-1374) — a branch-coverage requirement.
- `RegisterNavigation` twice without unregister throws `ArgumentException` (already covered).

### F8. `QfcCollectionControllerSelectionTests.cs`
Pure MSTest+Moq (**S1**, **S2**).
- `ActivateBySelection`: below range (`<= 0`), in range, above range (1403); `blExpanded` true/false
  (1412); `ActiveSelection` updated (1419); `TlpLayout` restored (1421); returns `ActiveSelection`.
- `ActivateBySelectionAsync`: same matrix; note it does **not** call `LblSubject.Focus()` (compare
  1417 with 1441) — assert the asymmetry.
- `ActivateByIndex`/`ActivateByIndexAsync`: `index + 1` delegation (1393, 1398).
- `ChangeByIndex`: same-index no-op (1453); `ActiveIndex == -1` skips `ToggleOffActiveItem` (1458);
  out-of-range no-op.
- `ChangeByIndexAsync`: same matrix through the dispatcher seam (1472, 1482).
- `SelectNextItem`: at the last item -> no-op (1488); otherwise advances.
- `SelectPreviousItem`: at index 0 -> no-op (1505); otherwise retreats.
- `SelectNextItemAsync`/`SelectPreviousItemAsync`: dispatch delegation (1500, 1518).
- `ScrollIntoView` (thin): forwards the viewer.
- `ToggleOffActiveItem`: `ActiveIndex == -1` -> returns the parameter unchanged (1670);
  `KbdActive == false` -> unchanged; expanded -> `ToggleExpansion` + returns `true` (1675-1681).
- `ToggleOffActiveItemAsync`: `ToggleFocusAsync(Off)` only; the expansion block is commented out
  (1694-1698) — assert `ToggleExpansionAsync` is **never** called (characterization).

### F9. `QfcCollectionControllerNavigationToggleTests.cs`
Pure MSTest+Moq (**S1**, **S2**).
- `ToggleExpansionStyle`: `itemIndex < 0` and `>= Count` throw `ArgumentOutOfRangeException` with
  the range in the message (1545-1551); `IsActiveUI == false` throws `InvalidOperationException`
  whose message includes subject/sender/date (1553-1561); `On` uses `_templateExpanded` (1566-1567);
  `Off` uses `_template` (1571-1572); the `heightChange < 0` invoke branch (1580-1585); `On` scrolls
  into view (1587-1588).
- `ToggleExpansionStyleAsync`: cancelled token throws (1593); otherwise dispatches (1595).
- `ToggleOffNavigation`: `ActiveIndex == -1` skips (1602); fan-out with `desiredState = Off` (1607).
- `ToggleOffNavigationAsync`: `TlpLayout` saved/restored (1617, 1631); `ActiveIndex == -1` branch.
- `ToggleOnNavigation` / `ToggleOnNavigationAsync`: fan-out `On`; `ActiveIndex != -1` reactivates
  (1642-1645, 1659-1662).

### F10. `QfcCollectionControllerConversationTests.cs`
Pure MSTest+Moq (**S1**).
- `ChangeConversationSilently(int, bool)` delegates to the group overload (1716).
- `ChangeConversationSilently(QfcItemGroup, bool)`: saves and restores `SuppressEvents` around the
  write (1727-1730), for both prior states.
- `ToggleGroupConv(string)`: original present; original absent with a child present (promotion
  path, 1743-1746); **original absent and no child** -> assert the current
  `ArgumentOutOfRangeException` (E11, characterization); `childCount == 0` skips the collapse
  (1752); `reactivate` true and false (1755-1764).
- `ToggleGroupConv(int, int)`: removes exactly `childCount` groups starting at `indexOriginal + 1`
  (1775-1785); calls `Cleanup()` on each removed controller (1783); renumbers (1787); unregisters
  and re-registers navigation (1773, 1796).
- `ToggleUnGroupConv`: `insertCount <= 0` skips the whole block (1825); `insertCount > 0` reserves
  space, inserts, renumbers from `insertionIndex + insertCount` (1827-1830); the
  `_digitRefreshNeeded` branch (1839-1842).
- `EnumerateConversationMembers`: excludes the base `entryID` (1884); orders by `SentOn` descending
  (1885); sets `ConvOriginID` from the group before the insertion point (1900-1902); `KbdActive`
  true/false (1905); dark/light (1912-1919); unchecks the conversation box (1920); plus the
  **count-mismatch characterization** from E12.
- `PromoteFirstChild`: decrements `childCount` through the `ref` parameter (1983); clears
  `ConvOriginID` and `IsChild` (1981-1982); returns the index; the `-1` case (E11).

### F11. `QfcCollectionControllerLayoutTests.cs`
**Pure MSTest, no mocks needed for four of the eight members** — the highest value-per-effort file.
- `InsertItemGroups`: inserts `insertCount` empty groups at `insertionIndex`; zero count is a no-op;
  insertion at the end.
- `UpdateSelectionNumberForRemoval`: `ActiveSelection == selection && selection == Count` decrements
  (2049-2052); `ActiveSelection == selection && selection < Count` leaves it unchanged;
  `ActiveSelection > selection` decrements `ActiveIndex` (2056-2060); `ActiveSelection < selection`
  is a no-op. All four branches.
- `RenumberGroups()`: assigns `i + 1` to every group.
- `RenumberGroups(int)`: only from `beginningIndex`; `beginningIndex >= Count` is a no-op.
- `EliminateSpaceForItems` / `MakeSpaceForItems` (thin): forward the computed height delta —
  **assert the current sign at 2017-2026 as a characterization of E3, do not correct it**.
- `ResetPanelHeight` / `ResetPanelHeightAsync` (thin): forward; assert the `RowStyles.Count - 1` vs
  full-sum asymmetry between 2097 and 2084 as a characterization.

### F12. `QfcCollectionControllerThemeTests.cs`
Pure MSTest+Moq; extends the existing `QfcCollectionControllerDarkModeTests` shape.
- `SetupLightDark`: sets `_darkMode` for both inputs; subscribes exactly once (2117).
- `DarkMode_CheckedChanged`: `_formViewer is null` early return (already covered); `sender is
  IOlObjects` with `DarkMode` true and false (2134-2136); `sender` not `IOlObjects` and `_globals`
  non-null (2138-2141); `sender` not `IOlObjects` and `_globals` null -> return (2142-2145);
  `_darkMode` updated (2155).
- `SetDarkMode` / `SetLightMode`: fan-out over every group with the supplied `async` flag
  (2160-2171); empty list is a no-op.

### F13. `QfcCollectionControllerMoveTests.cs`
Pure MSTest+Moq; extends the existing shape.
- `MoveEmailsAsync`: `_itemGroupsToMove` null and empty -> early return (2209-2213); N groups ->
  N move attempts; a throwing `MoveMailAsync` is swallowed and iteration continues (2242-2257);
  the inner `Subject` throw path (2245-2252).
- `TryGetItemGroupByIndex`: valid index; out-of-range returns `null` (2266-2269).
- `GetMoveDiagnostics`: `olAppointment` null (already covered); non-null with empty `Body`
  (2299-2303); non-null with non-empty `Body` (2304-2308); N > 1 groups; the trailing-null element
  (E5, characterization); the unreachable `else` at 2318-2322 documented as unreachable (E4) rather
  than tested.
- `xComma`: null, empty, `", "`, `","`, no comma, accented text (2332-2343). Pure function; six
  cases.

### F14. `QfcCollectionControllerLegacyLoadPathsTests.cs`
Pure MSTest+Moq. Every member is invoked directly; each test's docstring must state that the member
has no production caller and cite the E1 promotion issue.
- `WireUpKeyboardHandler`: the #444 characterization (§E2, three assertions).
- `AnyOpenDropDownsAsync`: cancelled token throws (1326); otherwise returns `false` (1327).
- `LoadGroups_02cAsync` / `LoadGroups_02bAsync`: cancelled token throws (593, 641); digits boundary
  (595, 643); one group per item.
- `LoadGroup_03bAsync`: requires a non-null `SynchronizationContext.Current` because of
  `TaskScheduler.FromCurrentSynchronizationContext()` at 662. Use
  `SynchronizationContext.SetSynchronizationContext(new SynchronizationContext())` — the exact
  pattern already used at `QfcItemController.TestSupport.cs:87-93`. **No STA required.**
- `LoadConversationsAndFoldersAsync` / `LoadItemGroup` / `LoadSequentialAsync` /
  `LoadGroupSequential`: one call per group; the digits boundary at 784, 850.
- `CacheTlpForMove` / `SwapTlp` / `CaptureTlpTemplate` (thin over **S1**): forward and assign.

### F15. STA files — the minimized set

Only the two production adapter files require STA. Both live in dedicated `*.StaTests.cs` files
using the manual STA-thread helper pattern proven at
`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:302-317` (§G2), not `[STATestClass]`.

**F15a. `QfcTlpSurface.StaTests.cs`** — justified because `TableLayoutPanel.SetCellPosition`,
`SetColumnSpan`, `RowStyles`, `MinimumSize`, `Size`, `Height`, `Parent.Height`,
`Panel.AutoScrollPosition`, and `TableLayoutHelper.InsertSpecificRow`/`RemoveSpecificRow`
(`UtilitiesCS/HelperClasses/Windows Forms/TableLayoutHelper.cs:13, 55`) have no interface and no
in-memory substitute. Precedent for exactly this on an in-memory, never-shown
`new TableLayoutPanel()`: `UtilitiesCS.Test/HelperClasses/TableLayoutHelper_Tests.cs:11-23` and
`UtilitiesCS.Test/HelperClasses/WindowsForms/ScreenAndTableLayoutTests.cs:41-54`. Target >= 90%
(new file). Every control is created in memory and never shown.

**F15b. `QfcItemViewerSurface.StaTests.cs`** — covers only the members reachable on an
`ItemViewer` obtained by `FormatterServices.GetUninitializedObject`, the technique already used for
this exact type at `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs:97, 128, 143,
208, 280`. **Risk flag:** an uninitialized `ItemViewer` has no designer-assigned child controls, so
`LblItemNumber`, `LblSubject`, and `ConversationMenuItem` are `null` and any write throws. If a
full `new ItemViewer()` proves unsafe or slow in the runner, this file is the **single** ratified
exemption request for the child, with a file-specific rationale under epic.md:205-225. It must not
exceed ~70 lines, and the request must be recorded in F1's ledger with the exact uncovered member
list — not a blanket file exemption.

### F16. Branch-coverage attention list (the 75% gate is independent of the 80% line gate)

Members whose branch count materially exceeds their line count, and which therefore need explicit
both-ways cases: `ReadyForMove` (169-172, three sentinels plus null), `TlpLayout` (205-228, four
paths), `Digits` (119-125), `GenerateStringKbdAction` (1366-1374, three-way with an unhandled
default), `UpdateSelectionNumberForRemoval` (2047-2061, four paths),
`DarkMode_CheckedChanged` (2125-2154, five paths), `RemoveSpecificControlGroup(int)` (1110-1154,
six paths), `RemoveSpecificControlGroupAsync` (1167-1246, eight paths), `GetMoveDiagnostics`
(2297-2322, four paths of which one is unreachable), `ToggleGroupConv(string)` (1743-1765, four
paths), `LoadItemToTlp` (912-942, three paths).

---

## H. Open risks and cross-child notes

1. **17 new `<Compile Include>` entries** in `QuickFiler/QuickFiler.csproj` (§A5) — the largest
   csproj delta of any wave-1 child. Insert contiguously after `:311`, preserve CRLF, expect an
   additive fan-in conflict (epic.md:604-617).
2. **F1 dependency.** Four new files need ledger rows: two `interface-only / not-measured`
   (epic.md:509-522) and two `testable` at >= 90%. Thirteen new partials also need rows, each
   defaulting to `testable` at >= 90% per epic.md:583-585. **Note the tension:** newly created
   partials carrying *pre-existing* code are extractions, not new logic, yet the epic's rule 4 puts
   them at the 90% new-file bar. F11 should raise this with F1 explicitly rather than assume the
   80% figure applies.
3. **F7 (#433) is unaffected** — no `IQfcCollectionController` change (§B11).
4. **F6 (#454's upstream constructor callers)** — `QfcFormController.Actions.cs:49, 83, 139` compile
   unchanged; no F6 file is edited.
5. **F8** — `xComma` must remain `public static` for `EfcHomeController.Metrics.cs:79`.
6. **F2** — `QfcItemGroup.cs` is F2-owned; `ItemViewer` cannot be retyped to `IItemViewer` (§G4).
   The `using static` at `QfcItemGroup.cs:12` is unaffected by the split.
7. **Issues to promote** (via the MCP promotion lifecycle, per epic.md:538-543): E1, E3, E4, E5, E6,
   E7, E8 (design half), E10 (design half), E11, E12, E13, E14, E15, E16. That is 14 promotions —
   large but expected for a 2,349-line COM-bound controller. E2 (#444) already exists and needs only
   a comment recording the dormancy finding.
8. **`ItemViewerQueue` static mutation** — seam **S3** is what keeps most tests off the global
   static core. Any test that still uses `SetCoreForTesting` must carry `[DoNotParallelize]` and the
   `[TestCleanup]` reset shown at `ViewerQueueStaticWrapperTests.cs:11-22`.
