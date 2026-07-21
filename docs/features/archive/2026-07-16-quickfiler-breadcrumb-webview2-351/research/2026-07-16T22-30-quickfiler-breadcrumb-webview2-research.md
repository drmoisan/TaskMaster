# Research: QuickFiler WebView2 Breadcrumb Control (Issue #351)

- **Feature:** quickfiler-breadcrumb-webview2 (epic `folder-tree-breadcrumb-redesign`, child 9103, wave 1, C4)
- **Date:** 2026-07-16T22-30
- **Author:** task-researcher agent
- **Branch researched:** `TaskMaster-wt-2026-07-16T20-52` (worktree `agent-a1d165d3cb6c7c026`, HEAD `136c0cd2`)
- **Status of upstream 9101:** NOT present on this branch (see §4)

## Executive Summary

1. **Scope = `ItemViewer` only.** Re-verified: only `ItemViewer` is constructed in production
   (`QuickFiler\Helper Classes\ItemViewerQueue.cs:105`). The nine other declared viewer variants
   remain dead Designer-only types. The spec should record a single-viewer scope decision (§1).
2. **The `CboFolders` surface is already fully seam-wrapped.** Every controller interaction goes
   through `IItemViewer` intent members (`SetFolderItems`, `SetFolderSuggestions`,
   `GetSelectedFolder`, `SetFolderDroppedDown`, `FolderSelectionChanged`, ...), so the WebView2
   replacement can be executed behind the existing interface with zero controller-contract churn
   for the selection output. Two paths must both be preserved: the `FolderRow` suggestion path and
   the plain-`string[]` search-result path (§2).
3. **The existing WebView2 pattern is one-way.** `InitializeWebViewAsync`
   (`QfcItemController.ViewerSetup.cs:36`) creates a shared environment behind the injected
   `IWebViewCoreInitializer` seam and displays in-memory HTML via `NavigateToString`. There is no
   `WebMessageReceived`, `AddHostObjectToScript`, or `ExecuteScriptAsync` usage anywhere in the
   repo — the JS<->.NET bridge is entirely new code (§3).
4. **The 9101 provider does not exist yet.** No ancestor-chain/on-demand-subfolder provider is on
   this branch; the plan must code against an assumed contract (documented in §4) and the closest
   existing live-Outlook seam family (`IOutlookFolderHierarchyReader` + `FolderTreeSnapshotNode`)
   is identified as its probable substrate.
5. **Percentage-obscuring defect:** static geometry in the owner-draw path reserves a fixed
   right-aligned 46 px column, confirming the epic's "no layout-level overlap" statement. The
   strongest runtime hypothesis is theme-driven color contrast (an explicit `TODO: Override the
   draw function because these colors do not work as expected` sits on the exact lines that set
   `CboFolders` colors), with dropdown-scrollbar overlay and DPI clipping as secondary hypotheses
   (§5).
6. **Recommended approach:** replace `CboFolders` with a second `WebView2` control inside
   `ItemViewer`, load a self-contained HTML/CSS/JS page from an embedded resource via
   `NavigateToString`, and bridge with `CoreWebView2.WebMessageReceived` +
   `PostWebMessageAsJson` JSON messages routed through a new host-neutral
   `BreadcrumbStateModel` / message-router pair that is fully unit-testable (§6, §8).

---

## 1. Viewer-Variant Liveness (scope decision)

Search: `new (ItemViewer|QfcItemViewer\w*|QFCItemViewer\w*|ItemViewerExpanded)\b` over `**/*.cs`.

Production construction sites:

| Site | Evidence |
|---|---|
| `QuickFiler\Helper Classes\ItemViewerQueue.cs:105` | `return new ItemViewer();` inside `CreateProductionViewer()`, wired as `ProductionViewerFactory` (line 85) into `ViewerQueueCore<ItemViewer>` (lines 108–121). This is the only production path. |
| `QuickFiler.Test\Form1.cs:24` | `var _controlGroup = new ItemViewer();` — test harness only. |
| `QuickFiler.Test\QfcViewer_Test.cs:29,37` | commented out. |

No DI registration, factory, or reflection-based instantiation of any other variant exists. The
nine other declarations (`Form1`, `ItemViewerExpanded`, `QfcItemViewer`, `QFCItemViewerDarkNew`,
`QfcItemViewerExpanded`, `QfcItemViewerExpandedLight`, `QFCItemViewerLightNew`,
`QfcItemViewerLightSelected`, `QfcItemViewerV1`) still declare their own `CboFolders` Designer
fields (e.g. `QfcItemViewerV1.Designer.cs:728`, `QFCItemViewerDarkNew.Designer.cs:739`,
`ItemViewerExpanded.Designer.cs:807`) but are never constructed.

Additionally, the generic type parameter of the pooled queue is hard-bound:
`ViewerQueueCore<ItemViewer>` (`ItemViewerQueue.cs:93,108`), so no variant can be substituted at
runtime without a code change.

**Scope decision the spec should record:** the WebView2 breadcrumb replaces `CboFolders` in the
single live `ItemViewer` variant only. The nine dead variants are untouched (they remain
`[ExcludeFromCodeCoverage]` Designer relics; deleting them is out of scope per the epic's
non-goals). Required-change viewer count = 1. This confirms the prior #325 assessment; nothing
has changed since 2026-07-15.

---

## 2. CboFolders Surface

### 2.1 Declaration and event wiring

- Field: `private System.Windows.Forms.ComboBox _cboFolders` exposed as public property
  `CboFolders` on `ItemViewer` (Designer-backed; `ItemViewer.Designer.cs:189–204`).
- Designer configuration (`QuickFiler\Viewers\ItemViewer.Designer.cs`):
  - `:193` `DrawMode = OwnerDrawFixed`
  - `:194` `DropDownStyle = DropDownList`
  - `:195` `FlatStyle = Flat`
  - `:196` `DrawItem += CboFolders_DrawItem`
  - `:197` `MouseDown += CboFolders_MouseDown`
  - `:191` spans 2 columns of `_l0vh_Tlp` (the viewer `TableLayoutPanel`), `Dock = Fill`.
- Owner-draw handlers live in `QuickFiler\Viewers\ItemViewer.FolderSearch.cs`:
  - `:170–249` `CboFolders_DrawItem` — indent (`Depth * 14`), glyph column (14 px), name rect,
    and a fixed right-aligned percentage column (`FolderPercentColumnWidth = 46`, `:163`),
    drawn with `TextFormatFlags.Right` at `e.Bounds.Right - 46` (`:221–234`).
  - `:254–281` `CboFolders_MouseDown` — glyph hit-test toggling `FolderTreeStateModel`.
- Runtime event subscription outside the Designer:
  - `QfcItemController.EventWiring.cs:84` `_itemViewer.FolderSelectionChanged += CboFolders_SelectedIndexChanged;`
  - `KeyboardHandler.cs:~500` `CboFolders_KeyDownAsync` throws `ArgumentException` if `sender` is
    not a `ComboBox` (`:501–505`) and dispatches to `DdOpen_KeyDownAsync` / `DdClosed_KeyDownAsync`
    on `cb.DroppedDown` (`:507–514`).
  - `KeyboardHandler.cs:543–583` — `Keys.Right` calls
    `cbo.GetAncestor<ItemViewer>().FolderTreeRightArrow()` (`:549`), falling through to the
    `MyBox.ShowDialog("Pop Out Item or Enumerate Conversation?" ...)` legacy behavior (`:559–564`);
    `Keys.Left` calls `FolderTreeLeftArrow()` (`:572`), falling through to closing the dropdown
    (`:579`). This entire routing is ComboBox-shaped and must be redesigned for the bridge.

### 2.2 Population paths (both must survive the replacement)

**Path A — suggestion rows (`FolderRow` contract, #324/#325):**
`QfcItemController.FolderHandling.cs:161–200` `AssignFolderComboBox()`:
- `:176` `_itemViewer.SetFolderItems(_folderHandler.FolderArray)` (legacy `string[]`)
- `:183–186` `_itemViewer.SetFolderSuggestions(_folderHandler.FolderRowArray)` (guarded on
  `Suggestions != null`)
- `:187–197` preselect `_predeterminedFolder` if present, else index 1
- `:198` `_selectedFolder = _itemViewer.GetSelectedFolder();`

`ItemViewer.FolderSearch.cs:24–29` `SetFolderSuggestions` is the **only production call site of
`FolderHierarchyBuilder.Build`** (`new FolderHierarchyBuilder().Build(rows)`), feeding
`FolderTreeStateModel` and `RebindFolderTree()` (`:34–54`).

**Path B — search results (plain strings):**
`QfcItemController.EventHandlers.cs:164–178` `TextBoxSearch_TextChanged` calls
`_folderHandler.FindFolder("*" + SearchText + "*", ...)`, then `ClearFolderItems()`,
`SetFolderItems(folders)`, `SetFolderSelectedIndex(1)`, `SetFolderDroppedDown(true)`. These rows
are raw strings, not `FolderRow`s; the WebView2 control must render them (breadcrumb or flat) and
return them from `GetSelectedFolder()` verbatim.

**Special row:** the injected `"Trash to Delete"` string is compared literally downstream
(`QfcItemController.MailActions.cs:90`), so string-identity of non-suggestion rows is contract.

### 2.3 Downstream consumers of the selection (must be preserved bit-for-bit)

- `ItemViewer.FolderSearch.cs:107–114` `GetSelectedFolder()` returns `vm.FolderPath` for a
  `FolderNodeViewModel` item, else `SelectedItem as string`. **The selection output contract is a
  full folder path string (or the verbatim non-suggestion string).**
- `QfcItemController.cs:237–240` caches it as `_selectedFolder` / `SelectedFolder`.
- `QfcItemController.MailActions.cs:90` (`SelectedFolder != "Trash to Delete"` gates attachment
  saving) and `:103` (`DestinationOlStem = SelectedFolder` — the actual move target).
- `QfcCollectionController.cs:170–171` (dialog-skip check) and `:2308` (CSV export line).
- `QfcItemController.EventHandlers.cs:209–212` `CboFolders_SelectedIndexChanged` refreshes
  `_selectedFolder` on every selection change.

### 2.4 `IItemViewer` members that must be re-implemented over the breadcrumb

`QuickFiler\Viewers\IItemViewer.cs:80–100`: `SetFolderItems(string[])`,
`SetFolderSuggestions(IReadOnlyList<FolderRow>)`, `GetSelectedFolder()`,
`SetFolderSelectedIndex(int)`, `SetFolderSelectedItem(string)`, `SetFolderDroppedDown(bool)`,
`ClearFolderItems()`, `FocusFolderDropDown()`, `FolderContains(string)`, `GetFolderItems()`,
`event FolderSelectionChanged`, `event KeyEventHandler FolderKeyDown`.

Notes:
- `SetFolderDroppedDown` / `FocusFolderDropDown` are also called from
  `QfcItemController.Navigation.cs:33–46` and `EventHandlers.cs:177,184–185`. A breadcrumb has no
  literal "dropped down" state; the intent members stay but map to breadcrumb focus / expansion
  popup visibility.
- `FolderKeyDown` currently forwards `CboFolders.KeyDown` (`ItemViewer.FolderSearch.cs:137–141`);
  the keyboard handler is wired against it. With WebView2, key events for Left/Right arrive
  **inside the browser**, so they must be surfaced via the JS bridge instead (see §3.3/§6).
- Theming: `UtilitiesCS\HelperClasses\ThemeHelpers\Theme.Rendering.cs:96–98` sets
  `_comboFolders.BackColor/ForeColor` per theme, and `:112–116` already sets
  `CoreWebView2.Profile.PreferredColorScheme` for the message-body WebView2 — the breadcrumb page
  must also participate in dark/light theming (CSS custom properties driven by a theme message or
  `PreferredColorScheme`/`prefers-color-scheme`).

---

## 3. WebView2 Host Pattern (existing, to mirror)

### 3.1 Initialization and threading

`QuickFiler\Controllers\QfcItemController.ViewerSetup.cs:36–107` `InitializeWebViewAsync()`:
1. `Token.ThrowIfCancellationRequested()`.
2. Cache folder `%LocalAppData%\WindowsFormsWebView2` (`:43–46`), options
   `new CoreWebView2EnvironmentOptions("–incognito ")` (`:49`).
3. **Marshal to UI thread with `await _itemViewer.UiSyncContext;`** (`:52`) — the awaitable
   SynchronizationContext pattern used throughout QuickFiler.
4. Environment creation isolated behind the injected seam:
   `_webViewInitializer.CreateEnvironmentAsync(cacheFolder, options)` (`:58–61`) then
   `_webViewInitializer.EnsureCoreWebView2Async(((ItemViewer)_itemViewer).L0v2h2_WebView2, _webViewEnvironment)`
   (`:62–65`). The concrete cast is an accepted P2-T4 residual; the whole method is
   `[ExcludeFromCodeCoverage]` with the rationale documented at `:27–35`.
5. Post-init CoreWebView2 wiring happens inline: `AddWebResourceRequestedFilter` +
   `WebResourceRequested` handler serving inline cid: images from memory (`:73–99`, issue #326).

Trigger sites: `QfcItemController.Initialization.cs:193` (`UiDispatcher.InvokeAsync`), `:255`
(awaited), `:286`, `:321` (fire-and-forget). `EfcItemController.cs:110,164,207` follows the same
shape for the EfcViewer surface.

Seam types:
- `QuickFiler\Viewers\IWebViewCoreInitializer.cs` — two-method seam
  (`CreateEnvironmentAsync`, `EnsureCoreWebView2Async`).
- `QuickFiler\Viewers\WebView2CoreInitializer.cs:15–29` — `[ExcludeFromCodeCoverage]` 1:1
  forwarding adapter over the SDK ("routing-testable" pattern; the routing method is tested with a
  mock initializer).

### 3.2 Control hosting and content delivery

- Designer-declared control: `ItemViewer.Designer.cs:231–251` (`_l0v2h2_WebView2`,
  `CreationProperties` all-null, `DefaultBackgroundColor = Transparent`, `Dock = Fill`,
  `ParentChanged += L0v2h2_WebView2_ParentChanged`), exposed via public property
  `ItemViewer.cs:309`.
- Content is **in-memory HTML via `NavigateToString`** (`ItemViewer.WebViewThread.cs:15`;
  invoked from `QfcItemController.EventHandlers.cs:200` with `MailItemHelper.Html`). There is no
  on-disk HTML asset deployment for the body pane; the only HTML asset precedent in the project is
  `QuickFiler.csproj:495` `<Content Include="Resources\EmailHeader.html" />` (an HTML template
  consumed as a resource).
- Interface exposure: `IItemViewer.cs:107–108` `NavigateToString(string html)` and
  `WebViewInitializationCompleted` event (forwarding `CoreWebView2InitializationCompleted`,
  `ItemViewer.WebViewThread.cs:17–21`).

### 3.3 JS<->.NET messaging — none exists today

Repo-wide grep for `WebMessageReceived|AddHostObjectToScript|ExecuteScriptAsync|SetVirtualHostNameToFolderMapping`
matched **zero** call sites. The bridge for double-click/keyboard/live-subfolder-query is new
code. The WebView2 SDK pinned at `1.0.3912.50` (`QuickFiler.csproj:79–86`) fully supports
`CoreWebView2.WebMessageReceived`, `PostWebMessageAsJson`, and
`WebView2.CoreWebView2.Settings.AreHostObjectsAllowed`.

### 3.4 Reusable pattern statement for the breadcrumb

Mirror exactly: Designer-declared `WebView2` control in `ItemViewer` (replacing `_cboFolders` in
the same `_l0vh_Tlp` cell, ColumnSpan 2) -> environment/init through the **same**
`IWebViewCoreInitializer` instance and (ideally) the **same `CoreWebView2Environment`** already
created for the body pane (`_webViewEnvironment`, cleared in `Cleanup()` at
`QfcItemController.ViewerSetup.cs:312`) -> content loaded once via `NavigateToString` from an
embedded/`Resources` HTML string -> messages via `WebMessageReceived` (JSON) with responses via
`PostWebMessageAsJson`. Reusing the existing environment avoids a second user-data-folder
negotiation (two environments with different options against the same folder fail).

---

## 4. 9101 Provider Contract

### 4.1 Present state: NOT on this branch

- Glob `docs/features/**/*folder-hierarchy-live-provider*/**` -> no files. The 9101 feature folder
  (`2026-07-16-folder-hierarchy-live-provider` per `epic.md:20`) has not been created.
- Grep for `IFolderHierarchyProvider|AncestorChain|ImmediateSubfolders` -> no provider interface.
- `FolderHierarchyBuilder.Build` (`UtilitiesCS\OutlookObjects\Folder\FolderHierarchyBuilder.cs:26`)
  and `FolderSuggestionTree` (`UtilitiesCS\OutlookObjects\Folder\FolderSuggestionTree.cs`) are
  both still the prefix-matching implementations the epic replaces.

### 4.2 Authoritative contract description (epic manifest)

`docs\features\epics\folder-tree-breadcrumb-redesign\epic.md:96–110` ("Shared Design"): given a
selected leaf folder the provider returns (a) the ordered ancestor chain `Folder -> ... -> Leaf`
(root-to-leaf segments) for breadcrumb rendering, and (b) on demand, the real immediate subfolders
of a given segment, queried live against the Outlook hierarchy (`MAPIFolder.Folders` or the
equivalent existing interop/adapter seam), with the Outlook I/O behind an injectable seam.
`epic.md:132–137` bands it C3 (wave 0) with the `cross_module_contract_change` signal because both
9102 and 9103 consume it across module boundaries.

### 4.3 Probable substrate already in the repo

The `UtilitiesCS.OutlookObjects.Folder` namespace already contains a live folder-hierarchy read
seam that 9101 will most plausibly extend or wrap:

- `IOutlookFolderHierarchyReader.cs:11–19` —
  `Task<IReadOnlyList<FolderTreeSnapshotNode>> ReadFoldersAsync(FolderTreeRequest, IDeadlineClock, IDispatcherYield, CancellationToken)`.
- `OutlookFolderHierarchyReader.cs:15–42` — production implementation over
  `Outlook.NameSpace.Stores` with an **internal injectable ctor**
  (`Func<IEnumerable<IOutlookStoreAdapter>> storeProvider`, `StoresWrapper`) and an internal
  `IOutlookFolderAdapter` seam (`:223`).
- `FolderTreeSnapshotNode.cs:11–58` — immutable node carrying `Key` (`FolderTreeNodeKey`),
  `DisplayName`, `StoreId`, `EntryId`, `ParentKey`, `FolderPath`, `RelativePath`,
  `ChildKeys` (`IReadOnlyList<FolderTreeNodeKey>`), `IsStale`, `StaleReason`. **`ParentKey` +
  `ChildKeys` already encode exactly the ancestor-chain and immediate-children relations the
  breadcrumb needs.**
- `FolderTreeRequest.cs:11–50` — store-scope + `AllowStaleSnapshot` request object.

### 4.4 Assumed interface the plan must code against (ASSUMED — PENDING 9101 MERGE)

Flag in the spec/plan as an assumed contract; reconcile names when 9101 merges to the epic
integration branch before this feature executes:

```csharp
namespace UtilitiesCS.OutlookObjects.Folder
{
    /// Assumed 9101 surface (names provisional).
    public interface IFolderHierarchyProvider
    {
        /// Ordered root-to-leaf segments for the given full folder path (olStem-relative).
        Task<IReadOnlyList<FolderBreadcrumbSegment>> GetAncestorChainAsync(
            string leafFolderPath, CancellationToken cancellationToken);

        /// Real immediate Outlook subfolders of the given segment (live query behind the seam).
        Task<IReadOnlyList<FolderBreadcrumbSegment>> GetImmediateSubfoldersAsync(
            string folderPath, CancellationToken cancellationToken);
    }

    /// Assumed DTO: net481-safe plain class (no record/init). Probable members:
    /// DisplayName, FolderPath (selection key), HasSubfolders (drives the plus/minus affordance).
}
```

Anchoring constraints for whatever 9101 actually ships:
- It must be consumable from `QuickFiler` (project references `UtilitiesCS`), so the
  `UtilitiesCS.OutlookObjects.Folder` namespace is the expected home.
- `HasSubfolders` (or derivable `ChildKeys.Count > 0`) is required by acceptance criterion 2
  (leaf-only affordance shown only when the leaf has subfolders).
- The DTO must be JSON-serializable for the bridge (plain strings/bools; no COM handles cross the
  bridge).
- net481 forbids `record`/`init` (no `IsExternalInit`); plain class or readonly struct with
  explicit ctor.

If 9101 ships only a raw reader extension (e.g. new methods on `IOutlookFolderHierarchyReader`),
this feature should still introduce a thin QuickFiler-facing adapter conforming to the assumed
shape so the breadcrumb model codes against one narrow interface.

---

## 5. Percentage-Obscuring Defect

### 5.1 Where the percentage is rendered today (QuickFiler surface)

- Value origin: `FolderScore.Probability` -> `FolderPredictor.FolderRowArray` (`FolderRow.Score`)
  -> `FolderHierarchyBuilder.Build` attaches probability at the leaf
  (`FolderHierarchyBuilder.cs:83`) -> `FolderNodeViewModel.FormattedPercentage`
  (`FolderNodeViewModel.cs:81–89`) -> `PercentageFormatter.FormatPercent`.
- Paint: `ItemViewer.FolderSearch.cs:220–234` — fixed 46 px column anchored at `e.Bounds.Right`,
  `TextFormatFlags.Right | VerticalCenter`, drawn with `e.ForeColor` after `e.DrawBackground()`.
  The name rect is clamped to `e.Bounds.Right - 46 - nameLeft` (`:204–210`), so **name text cannot
  geometrically overlap the percentage** — consistent with the epic's statement that static
  column/rect review found no layout-level overlap (`epic.md:79–81`).

### 5.2 Plausible runtime causes (hypotheses to test during reproduction)

1. **Theme color contrast (strongest).** `Theme.Rendering.cs:96–98` sets
   `_comboFolders.BackColor = CboFoldersBackColor; _comboFolders.ForeColor = CboFoldersForeColor;`
   under an explicit comment `// TODO: Override the draw function because these colors do not
   work as expected`. The owner-draw paints with `e.ForeColor` over `e.DrawBackground()`; a theme
   state with low/no fore-back contrast renders the percentage invisible while all geometry is
   correct. Precedent: issue #269's root cause was a Light-theme fore/back swap introduced by a
   refactor — same failure mode (text present but unreadable).
2. **Dropdown vertical scrollbar overlay.** With suggestions + separator + recents + search rows
   the drop-down list can scroll; a classic WinForms `ComboBox` quirk is the vertical scrollbar
   painting over the rightmost pixels of item content. The 46 px column touches
   `e.Bounds.Right` exactly, so any scrollbar overlap clips the percentage first.
3. **DPI/font scaling.** `CboFolders` uses a fixed 10.875 pt font (`ItemViewer.Designer.cs:198`)
   and `OwnerDrawFixed`; at >100% scaling a "100%" string may exceed the fixed 46 px column and
   clip on the left, or the fixed item height may clip vertically.

### 5.3 How to capture the runtime reproduction

- Environment: live Outlook with the add-in, QuickFiler open on a real inbox (this is inherently
  a manual/host-bound capture; no unit-test process can host the dropdown).
- Steps: populate a viewer with suggestions (Path A), open the dropdown, toggle theme
  dark/light (`QfcItemController.FocusAndTheme.cs:279–312` drives `SetQfcTheme`), and screenshot
  the dropped-down list in each state; repeat with enough rows to force a scrollbar and at 100%
  and 150% display scaling.
- Evidence location: store screenshots + a short observation log under
  `docs/features/active/2026-07-16-quickfiler-breadcrumb-webview2-351/evidence/repro/` (canonical
  `<FEATURE>/evidence/<kind>/` layout). Record theme name, row count, scaling, and which
  hypothesis the capture confirms.

### 5.4 What the CSS-based fix targets

In the WebView2 breadcrumb the row becomes a flexbox line; the fix is structural:
- Percentage element: `flex: 0 0 auto; margin-left: auto; min-width: <ch-based>;` so it can never
  be compressed or overlapped — it always wins layout.
- Segment container: `flex: 1 1 auto; min-width: 0; overflow: hidden; text-overflow: ellipsis;
  white-space: nowrap;` so long paths truncate in the middle segments instead of pushing the
  percentage out.
- Colors from CSS custom properties set per theme (bridge theme message or
  `prefers-color-scheme`), eliminating hypothesis 1 by construction; no scrollbar overlays a flex
  row, eliminating hypothesis 2.
- Acceptance evidence: post-fix screenshot in the same evidence folder demonstrating the
  percentage fully visible in both themes with a maximal-length path.

---

## 6. Candidate Approaches and Recommendation

### 6.1 Content delivery

- **A (recommended): embedded HTML string via `NavigateToString`.** Matches the only existing
  pattern (`ItemViewer.WebViewThread.cs:15`; `Resources\EmailHeader.html` precedent at
  `QuickFiler.csproj:495`). Single self-contained page (inline CSS/JS) stored as an embedded
  resource / `Resources` string; no deployment step, works under VSTO ClickOnce-style layouts,
  no virtual-host mapping. Data flows over the bridge, not into the HTML template, so the page is
  static and cacheable in memory.
- B: `SetVirtualHostNameToFolderMapping` + on-disk asset folder. Rejected: introduces a new
  build/deploy asset-copy concern with no repo precedent; `NavigateToString`'s ~2 MB limit is
  irrelevant for a static page.

### 6.2 Bridge mechanism

- **A (recommended): `WebMessageReceived` / `PostWebMessageAsJson` JSON protocol.** All
  interaction (segment double-click, plus/minus click, Left/Right key, selection change, subfolder
  query request/response, theme change, full re-render payload) becomes typed JSON messages. The
  .NET side is a pure, unit-testable message router (parse -> `BreadcrumbStateModel` transition ->
  response payload) with only a thin exempt shim touching `CoreWebView2`.
- B: `AddHostObjectToScript`. Rejected: requires a `ComVisible` host class (COM interop surface on
  net481 VSTO), synchronous reentrancy hazards, and is harder to fake in MSTest than a
  string-in/string-out router.

### 6.3 Keyboard routing

Left/Right arrows are captured by JS `keydown` inside the WebView2 (WinForms `KeyDown` on the host
control does not fire for keys the browser consumes). The existing
`KeyboardHandler.DdOpen_KeyDownAsync` Right/Left branches (`KeyboardHandler.cs:543–583`) are
replaced by bridge messages; the legacy fall-through behaviors (Right -> Pop Out/Enumerate dialog
when nothing expands, `:559–564`; Left -> close/collapse, `:579`) must be re-routed: the JS side
reports "unhandled arrow" and the .NET handler invokes the legacy action. `FolderKeyDown`
consumers must be re-audited during planning (the interface event can be raised synthetically from
the bridge to preserve the seam).

**Rejected alternatives (summary):** owner-draw ComboBox breadcrumb (violates the epic's control
technology constraint), third-party tree/list control and WPF/ElementHost (explicitly prohibited),
`ExecuteScriptAsync`-polling instead of `WebMessageReceived` (chattier, no push from JS).

---

## 7. Testability Seams

### 7.1 Pure host-neutral logic to extract (MSTest, no Outlook, no WebView2)

New (suggested home: `UtilitiesCS` or a QuickFiler non-UI folder, mirroring
`FolderTreeStateModel`'s placement and style):

1. **`BreadcrumbStateModel`** — collapse/expand state machine per suggestion row: ancestor chain
   segments, "collapsed-after segment k" state (double-click on non-leaf), leaf plus/minus
   expansion state, re-expand transition, and the invariant set (leaf-only affordance when
   `HasSubfolders`; collapse hides everything after the segment; re-expand restores the full
   chain). Direct analogue of the existing, fully unit-tested `FolderTreeStateModel`
   (`UtilitiesCS\OutlookObjects\Folder\FolderTreeStateModel.cs:12–157`, "performs no WinForms,
   COM, I/O, or timing work and is NOT coverage-exempt").
2. **Breadcrumb rendering projection** — pure function from state -> ordered segment/affordance/
   percentage DTO list (what JS renders). Percentage formatting reuses
   `PercentageFormatter.FormatPercent` and `FolderNodeViewModel`-style derivation.
3. **Bridge message protocol** — serializer/parser + router: JSON string in -> typed message ->
   state transition and/or provider call -> JSON string out. Deterministic, fully mockable
   (provider mocked with Moq), assertable with FluentAssertions.
4. **Selection mapping** — visible-row/segment id -> full folder path (the `GetSelectedFolder`
   contract), including the plain-string Path B rows and `"Trash to Delete"`.

### 7.2 Seam boundaries (thin, exempt glue)

- **WebView2 I/O:** the existing `IWebViewCoreInitializer` seam for init; a new narrow seam for
  post-init messaging (e.g. `IWebViewMessenger { event MessageReceived; PostJson(string); }`)
  whose production adapter over `CoreWebView2` is `[ExcludeFromCodeCoverage]` exactly like
  `WebView2CoreInitializer` (`WebView2CoreInitializer.cs:15`).
- **Outlook I/O:** entirely behind the 9101 provider (§4); breadcrumb code never touches interop.
- **WinForms glue:** `ItemViewer` partial re-implementations of the `IItemViewer` folder members
  over the model remain in the `[ExcludeFromCodeCoverage]` viewer, matching the current
  `ItemViewer.FolderSearch.cs` split ("host-neutral seams own all correctness; this partial holds
  only the WinForms glue", `:18–19`).
- Determinism: no timers needed; any async provider call is awaited through the router and tested
  with completed tasks. No temp files (prohibited).

### 7.3 Coverage targets

`ItemViewer`/controller glue stays exempt; every new host-neutral type is NOT exempt and must meet
the stricter documented bars (CLAUDE.md: new code >= 90% line; `.claude/rules/general-unit-test.md`:
>= 85% line / >= 75% branch — plan to the stricter of the two, as was done for #325).

---

## 8. Toolchain / Project Facts

- **All projects are legacy non-SDK `TargetFrameworkVersion v4.8.1`** (`QuickFiler.csproj:12`,
  `UtilitiesCS.csproj:24`, `QuickFiler.Test.csproj:17`, `UtilitiesCS.Test.csproj:16`). No
  `record`/`record struct`/`init` (CS0518, no `IsExternalInit`); use plain classes/readonly
  structs with explicit ctors.
- **WebView2 SDK already referenced everywhere needed:** `Microsoft.Web.WebView2 1.0.3912.50`
  (Core/WinForms/Wpf) in `QuickFiler.csproj:79–86` (+ `.targets` import `:542–546`),
  `UtilitiesCS.csproj:240–247`, `QuickFiler.Test.csproj:226–230`, `UtilitiesCS.Test.csproj:710–714`.
  No package addition required.
- **Projects touched by this feature:** `QuickFiler` (ItemViewer Designer + partials, controller
  event wiring/keyboard, bridge glue, HTML resource), `UtilitiesCS` (host-neutral breadcrumb model
  + protocol, if placed there; consumes 9101 types), `QuickFiler.Test` and/or `UtilitiesCS.Test`
  (new test classes). Both test projects are non-SDK: **new files need explicit
  `<Compile Include>` entries** in the `.csproj`.
- Toolchain loop per CLAUDE.md: `csharpier .` -> msbuild analyzers -> msbuild nullable ->
  `vstest.console.exe`. MSTest + Moq + FluentAssertions mandated.

---

## 9. Scope Decision: Live Viewer Variant(s)

**Decision for the spec:** exactly one live viewer — `ItemViewer` — verified on this branch at
`QuickFiler\Helper Classes\ItemViewerQueue.cs:103–106` via `ViewerQueueCore<ItemViewer>`
(`:93,108–121`). The nine dead variants (`Form1`, `ItemViewerExpanded`, `QfcItemViewer`,
`QFCItemViewerDarkNew`, `QfcItemViewerExpanded`, `QfcItemViewerExpandedLight`,
`QFCItemViewerLightNew`, `QfcItemViewerLightSelected`, `QfcItemViewerV1`) keep their Designer
`CboFolders` fields untouched; they are declared-only, `[ExcludeFromCodeCoverage]`, and out of
scope (their deletion is a separate concern excluded by the epic's non-goals). The functional
replacement, bridge, and CSS fix apply to `ItemViewer` only.

---

## 10. Open Questions / Assumptions

1. **9101 contract is ASSUMED (blocking dependency).** No provider exists on this branch; the plan
   codes against the §4.4 assumed interface and must reconcile exact names/namespace once 9101
   merges to `epic/folder-tree-breadcrumb-redesign-integration`. Marker:
   `ASSUMED-PENDING-9101-MERGE`.
2. **Second WebView2 per pooled viewer — resource cost unknown.** QuickFiler pools `ItemViewer`s
   (`ItemViewerQueue`); each viewer would host two Chromium-backed controls. Environment reuse
   (§3.4) mitigates process count (same user-data folder -> shared browser process), but memory
   and init-latency impact needs a runtime observation during implementation. Fallback if
   prohibitive: lazily initialize the breadcrumb WebView2 only when the viewer becomes active.
3. **`CoreWebView2EnvironmentOptions("–incognito ")`** (`ViewerSetup.cs:49`) uses an en-dash, not
   `--`; the argument is silently ignored today. The breadcrumb should reuse the same options
   object verbatim (environment-compat requirement), not fix this in-scope.
4. **Path B (search results) breadcrumb rendering** is a design question for the spec: raw search
   strings are full paths, so they can render as breadcrumbs, but they carry no probability and
   arrive as `string[]` — decide whether they render flat or as chains (recommend chains via the
   same ancestor-split, percentage cell empty).
5. **`FolderKeyDown`/`DdOpen_KeyDownAsync` decommissioning scope**: `KeyboardHandler` has
   substantial ComboBox-shaped logic (`CboFolders_KeyDownAsync` and both `Dd*_KeyDownAsync`
   families); the plan must decide between rerouting (preferred, smaller diff) and removal.
6. **Focus semantics**: `FocusFolderDropDown`/`SetFolderDroppedDown(true)` callers expect keyboard
   focus to land in the folder control (`Navigation.cs:33–46`); WebView2 focus hand-off
   (`WebView2.Focus()` + JS `focus()`) needs runtime verification in the live add-in.
7. **Percentage-obscuring root cause is unconfirmed by design** — the feature mandates runtime
   reproduction first (§5.3). The three hypotheses are ordered by likelihood; the capture decides.
8. **Coverage-threshold discrepancy** (CLAUDE.md 80/90 vs rules 85/75) persists; plan to the
   stricter bar per prior epic practice.

## 11. Key Evidence Index (absolute paths)

- `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1d165d3cb6c7c026\QuickFiler\Helper Classes\ItemViewerQueue.cs` (:105 liveness)
- `...\QuickFiler\Viewers\ItemViewer.Designer.cs` (:189–204 CboFolders; :231–251 WebView2)
- `...\QuickFiler\Viewers\ItemViewer.FolderSearch.cs` (whole file: seam members, owner-draw, hit-test)
- `...\QuickFiler\Viewers\IItemViewer.cs` (:80–100 folder seam; :107–108 WebView members)
- `...\QuickFiler\Viewers\ItemViewer.WebViewThread.cs` (:15 NavigateToString)
- `...\QuickFiler\Viewers\WebView2CoreInitializer.cs`, `...\IWebViewCoreInitializer.cs` (init seam)
- `...\QuickFiler\Controllers\QfcItemController.ViewerSetup.cs` (:36–107 InitializeWebViewAsync)
- `...\QuickFiler\Controllers\QfcItemController.FolderHandling.cs` (:161–200 AssignFolderComboBox)
- `...\QuickFiler\Controllers\QfcItemController.EventHandlers.cs` (:164–178 search path; :209–212 selection)
- `...\QuickFiler\Controllers\QfcItemController.MailActions.cs` (:90,:103 SelectedFolder consumption)
- `...\QuickFiler\Controllers\KeyboardHandler.cs` (:543–583 arrow routing)
- `...\UtilitiesCS\OutlookObjects\Folder\FolderHierarchyBuilder.cs` (:26 Build — to be replaced)
- `...\UtilitiesCS\OutlookObjects\Folder\FolderTreeStateModel.cs` (host-neutral precedent)
- `...\UtilitiesCS\OutlookObjects\Folder\FolderNodeViewModel.cs` (:65–89 Glyph/FormattedPercentage)
- `...\UtilitiesCS\OutlookObjects\Folder\IOutlookFolderHierarchyReader.cs`, `OutlookFolderHierarchyReader.cs`, `FolderTreeSnapshotNode.cs`, `FolderTreeRequest.cs` (9101 substrate)
- `...\UtilitiesCS\HelperClasses\ThemeHelpers\Theme.Rendering.cs` (:96–98 CboFolders theming TODO; :112–116 WebView2 color scheme)
- `...\docs\features\epics\folder-tree-breadcrumb-redesign\epic.md` (:96–110 shared contract; :146–151 9103 charter)
- `...\QuickFiler\QuickFiler.csproj` (:12 TFM; :79–86 WebView2 SDK; :495 EmailHeader.html precedent)
