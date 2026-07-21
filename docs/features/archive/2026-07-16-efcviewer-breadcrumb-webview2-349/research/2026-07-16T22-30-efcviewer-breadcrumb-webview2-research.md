# Research: EfcViewer WebView2 Breadcrumb Control (Issue #349, epic child 9102)

- Date: 2026-07-16T22-30
- Feature folder: `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/`
- Epic: `folder-tree-breadcrumb-redesign` (wave 1, C4, `depends_on: [9101]`)
- Scope of this document: preparation research only. No production code was modified.
- All paths are repository-relative unless stated otherwise.

## 0. Summary of load-bearing findings

1. **EfcViewer3 is dead code.** The only runtime instantiation of an Efc form viewer is
   `new EfcViewer()` in `QuickFiler/Helper Classes/EfcViewerQueue.cs:83`. `EfcFormController`
   takes a concrete `EfcViewer` (fields/ctors at `QuickFiler/Controllers/EfcFormController.cs:34,44-50`);
   nothing constructs `EfcViewer3`. Its `FolderListBox` is wired only in its Designer file. The
   drafted AC says "both `EfcViewer` and `EfcViewer3`"; only `EfcViewer` has controller wiring, so
   the `EfcViewer3` conversion is a Designer-only mechanical swap (or a scope decision to remove it).
2. **The repo has a WebView2 hosting precedent but NO JS->.NET bridge precedent.** A repo-wide
   search for `ExecuteScriptAsync|PostWebMessage|WebMessageReceived|AddHostObjectToScript` returns
   zero production hits. Existing usage is one-way: `NavigateToString(html)` plus a
   `WebResourceRequested` interception (the feature 326 `cid:` fix). The bidirectional bridge is a
   new surface for this codebase and is the principal novelty of this feature.
3. **The percentage-obscuring defect has a concrete, evidence-backed candidate cause**: fixed
   pixel column widths (`olvColumnFolder.Width = 3200`, `olvColumnPercent.Width = 500`,
   `QuickFiler/Viewers/EfcViewer.Designer.cs:915,921`) authored at a high-DPI design scale
   (`AutoScaleDimensions = (12F, 25F)`, line 4250) are not rescaled by WinForms
   `AutoScaleMode.Font` (ColumnHeader widths are not `Control`s), so at ordinary runtime DPI the
   folder column alone can exceed the visible control width and push the `%` column outside the
   viewport. This explains "static column/rect math shows no overlap" (3200+500 fits the 3728-wide
   design-time control, line 905) while the runtime shows the percent hidden. See §D.
4. **9101 has not started** (no `docs/features/*/2026-07-16-folder-hierarchy-live-provider*`
   folder exists on this branch); the consumer surface in §C.3 is therefore an assumed contract
   grounded in the existing seams 9101 is mandated to build on
   (`UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyReader.cs` adapter interfaces,
   `IOutlookFolderTreeService` snapshots).
5. **WebView2 SDK 1.0.3912.50** (net462 assemblies, compatible with the project's net481) is
   already referenced by `QuickFiler/QuickFiler.csproj:79-86`; it includes
   `SetVirtualHostNameToFolderMapping`, `WebMessageReceived`, and `PostWebMessageAsJson`. No new
   package is required.

---

## A. Existing WebView2 hosting pattern in QuickFiler

### A.1 Core initialization and the DI seam

- Seam interface: `QuickFiler/Viewers/IWebViewCoreInitializer.cs` — two members:
  `CreateEnvironmentAsync(string cacheFolder, CoreWebView2EnvironmentOptions)` and
  `EnsureCoreWebView2Async(WebView2 control, CoreWebView2Environment)`.
- Production adapter: `QuickFiler/Viewers/WebView2CoreInitializer.cs` — 1:1 forwarding shim over
  the SDK, `[ExcludeFromCodeCoverage]` with an in-code justification (lines 8-15). It has a
  routing test at `QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs`.
- Consumer: `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:36-107`
  (`InitializeWebViewAsync`):
  - Cache folder: `%LocalAppData%\WindowsFormsWebView2` (line 46).
  - Options: `new CoreWebView2EnvironmentOptions("–incognito ")` (line 49; note the literal
    en-dash — a latent oddity, preserved verbatim in both controllers).
  - **Threading**: the method first awaits `_itemViewer.UiSyncContext` (line 52) to marshal onto
    the WinForms UI (STA) thread before `EnsureCoreWebView2Async`; the call is fired from
    `QfcItemController.Initialization.cs:193` via
    `_ = _itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewAsync)` (fire-and-forget on the UI
    dispatcher). WebView2 controls must be created and touched only on the UI thread.
  - The `WebView2` control itself is Designer-owned (`ItemViewer.L0v2h2_WebView2`) and reached via
    a documented concrete cast `((ItemViewer)_itemViewer).L0v2h2_WebView2` (line 63); the method is
    `[ExcludeFromCodeCoverage]` because that cast is not mockable (lines 27-35).
- A second, older copy of the same init exists in
  `QuickFiler/Controllers/EfcItemController.cs:174-240` (`InitializeWebView` /
  `InitializeWebViewAsync`) that calls the raw SDK (`CoreWebView2Environment.CreateAsync`) without
  the seam. This is the WebView2 already living inside the EfcViewer form (the embedded
  `ItemViewer` mail-body pane), invoked from `EfcItemController.cs:110,164` via
  `Task.Run(() => InitializeWebViewAsync())`. The new breadcrumb host should follow the
  seam-based `QfcItemController` pattern, not this raw one.

### A.2 Content delivery to the control

- Sole production mechanism: `NavigateToString(string html)` — forwarded by
  `QuickFiler/Viewers/ItemViewer.WebViewThread.cs:15` and exposed on the narrowed viewer interface
  `QuickFiler/Viewers/IItemViewer.cs:107` (which is what makes controller logic mockable).
  Call sites: `QfcItemController.EventWiring.cs:139-143`, `EventHandlers.cs:200`,
  `FocusAndTheme.cs:293` (dark-mode re-render), `EfcItemController.cs:764,785,791,1100`.
- No `SetVirtualHostNameToFolderMapping`, no on-disk assets, no embedded-resource web assets exist
  anywhere in the repo (verified by repo-wide grep).
- In-memory sub-resource serving precedent (feature 326 `cid:` fix),
  `QfcItemController.ViewerSetup.cs:66-99`: `CoreWebView2.AddWebResourceRequestedFilter`
  (`https://{CidImageResolver.DefaultVirtualHost}/*`, image context) plus a `WebResourceRequested`
  handler that builds a content-id map at request time and answers with
  `_webViewEnvironment.CreateWebResourceResponse(new MemoryStream(bytes), 200, "OK", ...)`.
  This proves the pattern for serving arbitrary bytes to the WebView without disk assets.

### A.3 .NET->JS, JS->.NET, serialization

- **There is no existing production usage** of `ExecuteScriptAsync`, `PostWebMessageAsJson/AsString`,
  `WebMessageReceived`, or `AddHostObjectToScript` (repo-wide grep, zero hits outside this
  research). The JS<->.NET event bridge is new. SDK 1.0.3912.50 supports all of them:
  - .NET->JS: `CoreWebView2.PostWebMessageAsJson(string)` received in JS via
    `window.chrome.webview.addEventListener('message', ...)`; or `ExecuteScriptAsync`.
  - JS->.NET: `window.chrome.webview.postMessage(obj)` raising `CoreWebView2.WebMessageReceived`
    (`e.WebMessageAsJson` / `e.TryGetWebMessageAsString()`), delivered on the UI thread.
  - `AddHostObjectToScript` is COM-marshaling based and heavier; message-passing is the better fit
    here and keeps the contract serializable/testable.
- Serialization: Newtonsoft.Json 13.0.4 is the approved repo JSON library (present in
  `UtilitiesCS/packages.config:90`, `TaskMaster/packages.config:15`, and the test projects) but is
  **not currently referenced by `QuickFiler.csproj`** (only `log4net.Ext.Json`). Two options:
  place the bridge message contract types in `UtilitiesCS` (already references Newtonsoft; also
  where the sibling 9103 feature can reuse them), or add the already-approved package reference to
  QuickFiler. Recommendation: contracts in `UtilitiesCS` (shared with 9103 per the epic).

### A.4 Lifecycle/disposal

- `QfcItemController.Cleanup()` nulls `_webViewEnvironment` (`ViewerSetup.cs:312`); the `WebView2`
  control itself is disposed with its Designer form. Viewers are pooled:
  `EfcViewer` instances come from `EfcViewerQueue` (`ViewerQueueCore<EfcViewer>`,
  `QuickFiler/Helper Classes/EfcViewerQueue.cs:71-99`), so event handlers subscribed on
  `CoreWebView2` must be unhooked (or made idempotent) if the viewer can be re-initialized; the
  cid: handler solves staleness by rebuilding its map at request time rather than unsubscribing.
- WebView2 exposes `CoreWebView2InitializationCompleted` (forwarded at
  `ItemViewer.WebViewThread.cs:17-21`); any `PostWebMessageAsJson`/`NavigateToString` issued before
  init completes must be queued or gated — a state the breadcrumb host controller must model.

### A.5 SDK/package version

- `Microsoft.Web.WebView2` **1.0.3912.50**, `QuickFiler/packages.config:16`, referenced from
  `..\packages\Microsoft.Web.WebView2.1.0.3912.50\lib\net462\*` (`QuickFiler.csproj:79-86`) with
  the native-loader `.targets` import at `QuickFiler.csproj:546`. net462 assemblies run on the
  project's `TargetFrameworkVersion v4.8.1` (`QuickFiler.csproj:12`). The WebView2 Runtime
  (Evergreen) is a machine prerequisite already assumed by the existing mail-body pane.

---

## B. Current EfcViewer control being replaced

### B.1 Control declaration (Designer)

- `QuickFiler/Viewers/EfcViewer.Designer.cs`:
  - `FolderListBox = new BrightIdeasSoftware.TreeListView()` (line 50); field declared
    `internal BrightIdeasSoftware.TreeListView FolderListBox;` (line 4280).
  - Configuration (lines 882-921): `Tlp.SetColumnSpan(FolderListBox, 14)`, two columns
    (`olvColumnFolder`, `olvColumnPercent`), `Dock = Fill`, font `Microsoft Sans Serif 10.125F`,
    `FullRowSelect = true`, `HeaderStyle = Nonclickable`, **`OwnerDraw = true`** (line 903),
    `View = Details`, design size `(3728, 1)`.
  - Columns: `olvColumnFolder.AspectName = "DisplayName"`, `Width = 3200` (lines 913-915);
    `olvColumnPercent.Text = "%"`, `TextAlign = Right`, `Width = 500` (lines 919-921).
  - Form scaling: `AutoScaleDimensions = (12F, 25F)`, `AutoScaleMode = Font`,
    `ClientSize = (3844, 1065)` (lines 4250-4252) — authored at a high-DPI design scale.
- `QuickFiler/Viewers/EfcViewer3.Designer.cs`: same shape, `FolderListBox` at lines 39/525,
  columns at 255-265 (`Folder` width 1600, `%` width 300, right-aligned).
- ObjectListView package: `ObjectListView.Official 2.9.1` (`QuickFiler/packages.config:19`,
  `QuickFiler.csproj:93-94`). After the swap, `BayesianPerformanceViewer` and `ItemViewer`'s
  `FastObjectListView TopicThread` still use it, so the package reference stays.

### B.2 Liveness: EfcViewer vs EfcViewer3

- Live: `EfcViewer` only. Evidence: the only constructor call is
  `QuickFiler/Helper Classes/EfcViewerQueue.cs:83` (`CreateProductionViewer`); `EfcFormController`
  is typed to concrete `EfcViewer`; a repo-wide grep for `new EfcViewer3` finds nothing.
- `EfcViewer3` differs only trivially (extra tips labels `LblAcAttachments`, `LblAcPictures`,
  `LblAcConversation` in `EfcViewer3.cs:57-73`; synchronous `ToggleKeyboardDialog` in
  `ProcessCmdKey`, line 81, vs async in `EfcViewer.cs:94`). Its `FolderListBox` has no controller
  wiring — the `AspectName = "DisplayName"` binding in its Designer is the only binding.
- Both forms are `[ExcludeFromCodeCoverage]` (`EfcViewer.cs:20`, `EfcViewer3.cs:17`).
- Consequence for the spec: the behavioral conversion happens once (EfcViewer + EfcFormController);
  the EfcViewer3 change is a Designer-only control swap with no reachable behavior. The spec
  author should either state that explicitly or descope EfcViewer3 to deletion/parking (the epic
  non-goal only forbids *unifying* the two implementations, not removing dead one).

### B.3 Data flow into the control (rows and percentage)

- Row source: `EfcDataModel.FolderHelper` is a `FolderPredictor`
  (`QuickFiler/Controllers/EfcDataModel.cs:168-208`); rows are the legacy sectioned `string[]`
  `FolderPredictor.FolderArray` (`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:210`), with
  the additive typed mirror `FolderRowArray` (`FolderPredictor.cs:237`) producing
  `FolderRow { Text, Kind, Score? }` (`UtilitiesCS/OutlookObjects/Folder/FolderRow.cs`), where
  `FolderScore { FolderPath, Score(long, raw), Probability(double [0,1] max-normalized display value) }`
  (`UtilitiesCS/OutlookObjects/Folder/FolderScore.cs`).
- Binding path (all in `QuickFiler/Controllers/EfcFormController.cs`, the whole class is
  `[ExcludeFromCodeCoverage]`, line 26):
  - `PopulateFolderCombobox` (1060-1074): awaits `_dataModel.InitFolderHandlerAsync`, then
    `BindFolderRows(_dataModel.FolderHelper.FolderArray)`.
  - `BindFolderRows` (876-902): `FolderSuggestionTree.BuildFromRows(rows)`; probability join via
    `BuildProbabilitySource()` (907-916) -> `FolderProbabilityAdapter(source).Apply(tree)`
    (source = `_dataModel.FolderHelper.Suggestions.ToScoredArray()` projected to
    `FolderPath -> Probability`, private `DictionaryProbabilitySource`, 920-939); then
    `tlv.SetObjects(tree.Roots)` and `SelectedIndex = 1` (first row after the banner).
  - `ConfigureFolderTreeView` (853-863): `CanExpandGetter`/`ChildrenGetter` from
    `FolderSuggestionNode.HasChildren/Children`; `olvColumnFolder.AspectGetter = DisplayName`;
    `olvColumnPercent.AspectGetter = PercentageFormatter.FormatPercent(node.Probability)`.
  - Percent formatting: `UtilitiesCS/OutlookObjects/Folder/PercentageFormatter.cs:24-33` — null ->
    empty string; else whole-number `Math.Round(p*100, AwayFromZero) + "%"` (invariant culture).

### B.4 Current event wiring (must be re-carried over the bridge)

All in `EfcFormController.cs`:
- `WireEventHandlers` (371-404): `ConfigureFolderTreeView()` (394),
  `FolderListBox.KeyDown += FolderListBox_KeyDown` (395), plus form-wide
  `PreviewKeyDown`/`KeyDown` routed to the keyboard handler (376-387).
- `FolderListBox_KeyDown` (414-436): **Up at index 0 -> focus `SearchText`** (416-419);
  **Left arrow -> `_folderTree.LeftArrow(_selectedNode)`** (collapse, 420-428);
  **Right arrow -> `_folderTree.RightArrow(_selectedNode)`** (expand, 429-435). The TreeListView
  also performs its own native expand/collapse; the calls keep the host-neutral model in sync.
- Selection: `FolderListBox_SelectionChanged` (867-870) caches `_selectedNode`;
  `SelectedFolder => _selectedNode?.FullPath` (289-296) feeds filing (`ActionOkAsync`, 722-751),
  folder creation (486-544, 772-815), and find-mode open; `IsValidSelection` (1076-1088) rejects
  `"===="`-prefixed banner rows.
- `SearchText_TextChanged` (579-582) and `SearchText_DownArrow` (406-412, Down -> focus list);
  `RefreshSuggestionsAsync` (817-826) and `ActionDeleteAsync` (762-770, prepends the
  `"Trash to Delete"` pseudo-row and rebinds) all funnel through `BindFolderRows`.
- Keyboard jump: `'F'` action focuses `FolderListBox`
  (`GetAsyncCharacterActions` line 601-605, `GetKbdActions` 662-666).
- Theming/registration touch points: `_listHighlighted` includes `FolderListBox`
  (`ResolveControlGroups`, 221-225) feeding `EfcThemeHelper.SetupFormThemes`
  (`QuickFiler/Helper Classes/EfcThemeHelper.cs:249`); dark-mode switch via `DarkMode_Changed`
  (702-716). The WebView2 breadcrumb needs its own dark/light CSS switch (precedent:
  `NavigateToString(ItemHelper.ToggleDark(...))` in `QfcItemController.FocusAndTheme.cs:293`).

### B.5 Touch-point inventory for the swap (EfcViewer surface)

| Touch point | File:line | Change |
|---|---|---|
| `FolderListBox` TreeListView field + config | `EfcViewer.Designer.cs:50,882-921,4280` | Replace with `Microsoft.Web.WebView2.WinForms.WebView2` in the same TLP cell (span 14) |
| `olvColumnFolder/olvColumnPercent` | `EfcViewer.Designer.cs:51-52,911-921,4281-4282` | Delete (rendering moves to HTML/CSS) |
| `ConfigureFolderTreeView` | `EfcFormController.cs:853-863` | Replace with breadcrumb host controller init (bridge wiring) |
| `FolderListBox_SelectionChanged` / `_selectedNode` | `EfcFormController.cs:139-141,867-870` | Selection now arrives as a bridge message; cache selected path |
| `BindFolderRows` | `EfcFormController.cs:876-902` | Build breadcrumb rows from 9101 ancestor chains + probabilities; render to WebView |
| `FolderListBox_KeyDown` | `EfcFormController.cs:414-436` | Arrow keys arrive from JS via bridge; Up-at-top must emit a "focusSearch" message |
| `WireEventHandlers` lines 394-395 | `EfcFormController.cs` | Wire `WebMessageReceived` instead |
| `_listHighlighted` theming | `EfcFormController.cs:221-225` | WebView2 has no WinForms theme; supply CSS theme + dark-mode message |
| `'F'` jump-to | `EfcFormController.cs:601-605,662-666` | Focus the WebView2 control |
| `SearchText_DownArrow` | `EfcFormController.cs:406-412` | Focus WebView2 + select-first message |
| `ActionDeleteAsync` "Trash to Delete" row | `EfcFormController.cs:762-770` | Preserve as a selectable pseudo-row in the breadcrumb list |
| `SelectedFolder`/`IsValidSelection` | `EfcFormController.cs:289-296,1076-1088` | Derive from bridge-reported selection; keep banner rejection |
| EfcViewer3 Designer duplicate | `EfcViewer3.Designer.cs:39,231-265,525-527` | Designer-only swap or removal (dead variant, §B.2) |

`EfcItemController` (mail pane) and its WebView2 init are untouched by the folder-list swap.

---

## C. Hierarchy logic being replaced and the 9101 consumer surface

### C.1 Current prefix-matching approach

- `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs` (`BuildFromRows`, 39-90): sections
  the presented `string[]` on `"===="` banners; within a section, a path Y is a child of presented
  path X when Y starts with `X + "\"` and X is the longest such presented prefix
  (`FindLongestPrefixParent`, 217-241). **No ancestor synthesis, no Outlook query** — expanding a
  node can only ever reveal other already-presented suggestion rows. State model (Expand/Collapse/
  Toggle/LeftArrow/RightArrow/VisibleRows, 98-192) is pure and fully unit-tested
  (`UtilitiesCS.Test/OutlookObjects/Folder/FolderSuggestionTreeHierarchyTests.cs`, `...StateTests.cs`).
- `UtilitiesCS/OutlookObjects/Folder/FolderHierarchyBuilder.cs` (`Build`, 26-55): the QuickFiler
  (9103-side) equivalent — splits each scored suggestion path on `\` with find-or-add ancestor
  synthesis (`AddSuggestion`, 62-105); synthesized ancestors carry no probability. Also never
  queries Outlook.
- Node model: `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionNode.cs` — pure data + expand
  state + `Probability`; `FolderSuggestionNodeKind { Folder, Banner }`.
- 9101 replaces the *edge derivation* in both builders with real ancestor chains and live
  subfolder listing. What this feature must keep from the current model: banner rows, presented
  order, probability-join semantics, and the "Trash to Delete" pseudo-row.

### C.2 Existing Outlook interop seams 9101 builds on (verified present)

- `UtilitiesCS/OutlookObjects/Folder/IOutlookFolderHierarchyReader.cs` — async read of folder
  metadata behind the COM boundary ("Unit tests must provide fake hierarchy readers").
- `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyReader.cs` — production reader with an
  injectable `Func<IEnumerable<IOutlookStoreAdapter>>` store provider (33-42) and internal adapter
  interfaces `IOutlookStoreAdapter`/`IOutlookFolderAdapter` (216-229); the `MAPIFolder.Folders`
  enumeration lives only in the exempt `OutlookFolderAdapter.Children` (270-275).
- `UtilitiesCS/OutlookObjects/Folder/IOutlookFolderTreeService.cs` — cached snapshot service
  (`GetSnapshotAsync(FolderTreeRequest, ct)`, `SnapshotChanged`, `MarkStale`).
- `FolderTreeSnapshotNode` carries `Key, DisplayName, StoreId, EntryId, ParentKey, FolderPath,
  RelativePath, ChildKeys` (constructed at `OutlookFolderHierarchyReader.cs:195-206`) — i.e., the
  snapshot already contains everything needed for ancestor chains and immediate children;
  `FolderTreeSnapshotQueries.cs` shows the query-helper idiom.

### C.3 Assumed 9101 consumer surface (dependency; confirm against 9101 spec at integration)

This feature consumes; it must not re-derive hierarchy from suggestion rows. Assumed shape,
grounded in the seams above and the epic's Shared Design section:

```csharp
// UtilitiesCS (namespace per 9101; names indicative)
public interface IFolderHierarchyProvider
{
    /// Ordered root-to-leaf segments for the suggestion's folder path.
    /// Pure lookup against provider state; no COM on the caller's thread.
    Task<IReadOnlyList<FolderSegmentInfo>> GetAncestorChainAsync(
        string leafFolderPath, CancellationToken ct);

    /// Every real immediate Outlook subfolder of the given segment (live query
    /// behind the injectable seam).
    Task<IReadOnlyList<FolderSegmentInfo>> GetImmediateSubfoldersAsync(
        string folderPath, CancellationToken ct);
}

// net48-safe readonly struct (no record/init; precedent FolderRow/FolderScore)
public readonly struct FolderSegmentInfo
{
    public FolderSegmentInfo(string fullPath, string displayName, bool hasSubfolders) {...}
    public string FullPath { get; }
    public string DisplayName { get; }
    public bool HasSubfolders { get; }
}
```

Notes for the spec author:
- `HasSubfolders` on the *leaf* segment is required by AC 2 ("expand affordance only when the leaf
  has subfolders") — the provider must answer this cheaply per rendered row (snapshot-backed, not
  a per-row COM round-trip). Route this requirement into the 9101 contract review.
- Paths in suggestion rows are the `FolderPredictor` path strings (backslash-separated, store-
  relative as used by `FolderScore.FolderPath` and filing); the provider must accept that identity
  form. `FolderTreeSnapshotNode` exposes both `FolderPath` and `RelativePath`, so mapping exists.
- If 9101 lands a different shape (e.g., snapshot-node based), this feature's only coupling point
  is the breadcrumb-row builder input (§G decomposition), so the adapter cost is one mapping class.

---

## D. Percentage-obscuring defect

### D.1 Static state (no overlap in the math)

`PercentageFormatter.FormatPercent` (`PercentageFormatter.cs:24-33`) is correct and covered by
tests. Design-time geometry: control width 3728 (`EfcViewer.Designer.cs:905`), columns 3200 + 500
= 3700 <= 3728 with the `%` column right-aligned — statically consistent, matching the epic's
"static column/rect math shows no overlap".

### D.2 Candidate runtime causes (ranked)

1. **Unscaled ColumnHeader widths under font autoscaling (primary candidate).** The form was
   designed at `AutoScaleDimensions (12F, 25F)` with `AutoScaleMode.Font`
   (`EfcViewer.Designer.cs:4250-4251`) — roughly a 250-300% design scale. WinForms font
   autoscaling rescales `Control` bounds but **not** `ColumnHeader.Width`. At runtime,
   `CaptureConfigureItemViewer` (`EfcFormController.cs:166-187`) additionally forces the form to
   75% of the explorer screen. On a typical monitor the `FolderListBox` client width lands far
   below 3700 px while `olvColumnFolder` stays 3200 px, so the `%` column begins beyond the right
   edge of the viewport and is reachable only by horizontal scroll — i.e., "always obscured".
2. **Owner-draw rendering** (`OwnerDraw = true`, `EfcViewer.Designer.cs:903`): ObjectListView 2.9.1
   owner-draw sub-item painting clips to the computed cell rect; combined with (1) the cell rect
   itself is off-viewport. Owner-draw alone would explain partial clipping if a custom renderer
   mis-measured, but no custom renderer is assigned for these columns, so this is secondary.
3. **Long-path squeeze**: `olvColumnFolder` shows leaf `DisplayName` only (post-#327), so text
   width is not the squeeze source; low likelihood.
4. **Per-monitor DPI change after form creation**: font autoscaling occurs once; moving between
   monitors would change the mismatch magnitude, not the cause.

### D.3 Runtime reproduction (must precede the fix, per AC 5)

Unit tests cannot host a live TreeListView with policy compliance, so the reproduction is a
runtime evidence capture:

1. Launch the EfcViewer against live Outlook (existing manual flow), on the user's normal display.
2. Capture (a) a screenshot of the suggestion list showing the missing/obscured percent, and
   (b) a diagnostic log line (temporary, log4net per repo pattern) emitted on `Form.Shown`:
   `FolderListBox.ClientSize.Width`, `olvColumnFolder.Width`, `olvColumnPercent.Width`,
   `CurrentAutoScaleDimensions`, and `DeviceDpi`. Expectation under candidate 1:
   `olvColumnFolder.Width (3200) > ClientSize.Width`, proving the `%` column starts off-viewport.
3. Store both artifacts under
   `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/repro/`
   (canonical `<FEATURE>/evidence/<kind>/` location), named with ISO-8601 timestamps.
4. Remove the temporary log line in the same feature branch (it exists only to produce evidence).

### D.4 How the CSS fix addresses it

In the WebView2 breadcrumb, layout is expressed in CSS pixels, which WebView2 scales with the
device scale factor automatically — the WinForms design-DPI/ColumnHeader mismatch class of defect
cannot recur. Concrete fix pattern per suggestion row (flex row):

```css
.row   { display: flex; align-items: center; }
.crumb { flex: 1 1 auto; min-width: 0; overflow: hidden;
         text-overflow: ellipsis; white-space: nowrap; }  /* path may truncate */
.pct   { flex: 0 0 auto; margin-left: auto; white-space: nowrap; } /* never clipped */
```

The percent element is a fixed-size flex item that cannot be overlapped or pushed out of the
viewport regardless of path length, DPI, or window width; the breadcrumb path is the only element
allowed to truncate (with ellipsis). The acceptance verification for AC 5 is: percent text node
fully within the row's client rect at minimum form width — assertable in a JS-side check during
manual verification and by construction in the HTML-generator unit tests (percent markup always
emitted as the trailing fixed flex item).

---

## E. Testability and repository policy

### E.1 Policy inputs (read)

- `CLAUDE.md` (embedded): MSTest + Moq + FluentAssertions; line coverage >= 80% repo-wide, new
  modules >= 90%; COM/VSTO/WinForms exemption for (a) VSTO lifecycle, (b) WinForms Form-derived +
  Designer code, (c) interop event-handler classes without an injectable seam — exemption via
  `[ExcludeFromCodeCoverage]` reviewable in PRs; testable seams are NOT exempt.
- `.claude/rules/csharp.md`: DI-seam preference order (interface > delegate > adapter); analyzer
  stack incl. BannedApiAnalyzers; severity-first ordering invariant; TimeProvider guidance.
- `.claude/rules/general-unit-test.md`: independence/determinism; **no temp files**; banned in
  tests: `Thread.Sleep`, `Task.Delay`, wall-clock reads; test files under the test project tree.
- Coverage-threshold discrepancy (known from issues #325/#349 lineage): CLAUDE.md says 80/90,
  `.claude/rules/general-unit-test.md` + `quality-tiers.md` say uniform line >= 85% / branch >= 75%.
  Plan to the stricter bar (85 line / 75 branch, 90 for new modules).

### E.2 Testable (NOT exempt — must meet floors)

All of the following are pure or Moq-isolatable and belong in the coverage denominator:

1. **Breadcrumb row model** (new, host-neutral, `UtilitiesCS` recommended): construction from an
   ancestor chain + probability; collapse-after-segment state (double-click on non-leaf segment
   i hides segments > i and shows a plus at i); leaf expand state and children list; re-expand;
   Left/Right-arrow focus/segment transitions; banner and "Trash to Delete" pseudo-rows.
   Precedent for shape and tests: `FolderSuggestionTree` + its two test files.
2. **Bridge message contracts + (de)serialization** (new): inbound
   `{ type: segmentDoubleClick | leafExpandToggle | arrowKey | rowSelected, rowId, segmentIndex, key }`;
   outbound `{ type: render | subfolderResult | focusSearch, ... , requestId }`. Round-trip tests
   with FluentAssertions; malformed-JSON negative tests.
3. **HTML/CSS/JS document generation** (new): pure string builder from the row model (analogous to
   `MailItemHelper.Html`); assert percent markup always emitted as trailing fixed flex item,
   plus/minus emitted only when `HasSubfolders`, banner rows non-interactive, HTML-encoding of
   folder names.
4. **Bridge router/controller core** (new): given a deserialized inbound message, a
   `Mock<IFolderHierarchyProvider>` (9101), and a mocked host-post seam, assert state transitions
   and outbound messages (including the subfolder query round-trip and not-yet-initialized
   queueing). Precedent: `QfcItemController` seam tests
   (`QuickFiler.Test/Controllers/QfcItemController.SeamCoreTests.cs` verify `NavigateToString`
   against `Mock<IItemViewer>`).

To make (4) mockable, introduce a narrow host seam (interface-seam tier per `.claude/rules/csharp.md`):

```csharp
public interface IBreadcrumbWebHost   // implemented by an exempt WebView2 adapter
{
    void NavigateToString(string html);
    void PostMessageJson(string json);
    event EventHandler<string> MessageReceived;   // raises WebMessageAsJson
    bool IsCoreInitialized { get; }
}
```

### E.3 Legitimately exempt (thin, justified `[ExcludeFromCodeCoverage]`)

- `EfcViewer`/`EfcViewer3` forms and Designer files (already exempt, `EfcViewer.cs:20`).
- The WebView2 adapter implementing `IBreadcrumbWebHost` (1:1 SDK forwarding; precedent
  `WebView2CoreInitializer.cs:15` with in-code justification).
- The `EnsureCoreWebView2Async` init path bound to the concrete Designer control (precedent
  `QfcItemController.ViewerSetup.cs:35`), reusing the existing `IWebViewCoreInitializer` seam.
- `EfcFormController` is wholly exempt today (`EfcFormController.cs:26`); do NOT add new logic
  there — new logic goes in the non-exempt router/model classes, with the controller reduced to
  wiring.

### E.4 net48 constraints on new types

- No `record`, `record struct`, or `init` accessors (CS0518 without `IsExternalInit`); use plain
  classes or `readonly struct` with explicit constructors. In-repo precedents that state this in
  their XML docs: `FolderRow.cs:28`, `FolderScore.cs:7-10`.
- Both `QuickFiler.Test` and `UtilitiesCS.Test` are legacy non-SDK net4.8.1 projects
  (`QuickFiler.Test.csproj:17`): every new test file needs an explicit `<Compile Include>` entry.

---

## F. Project/build

- Owner project: `QuickFiler/QuickFiler.csproj` — legacy non-SDK, `TargetFrameworkVersion v4.8.1`
  (line 12), packages.config-based. New source files require explicit `<Compile Include>` entries
  (WebView2CoreInitializer example at line 386). Shared model/contract types placed in
  `UtilitiesCS` (also non-SDK net481) likewise need `<Compile Include>` there.
- WebView2: already referenced (see §A.5); `.targets` import handles the native
  `WebView2Loader.dll` copy (line 546). No csproj package changes needed for the control itself.
- Static web assets: recommend **no packaged assets** — generate the full HTML document (inline
  `<style>` and `<script>`) in C# and deliver via `NavigateToString`, matching the only existing
  content-delivery pattern (§A.2) and avoiding new packaging mechanics in a legacy VSTO csproj
  (no `SetVirtualHostNameToFolderMapping` folder to ship, no ClickOnce/VSTO manifest additions,
  and the generator string is unit-testable). If a future need for external assets arises, the
  feature-326 `WebResourceRequested` in-memory pattern covers it without disk assets.
- JSON: Newtonsoft.Json 13.0.4 is repo-approved; not currently in `QuickFiler/packages.config`.
  Preferred: contracts + serializer in `UtilitiesCS` (already references it, shared with 9103).
- Analyzers (`QuickFiler.csproj:556-557` BannedApiAnalyzers wired): banned symbols are
  `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay` (repo-root
  `BannedSymbols.txt`, RS0030 at suggestion severity). Implications: no `Task.Delay` for
  init-waiting (gate on `CoreWebView2InitializationCompleted` + queued messages instead); any
  debounce/timing uses injected `TimeProvider` (`Microsoft.Bcl.TimeProvider` backport per
  `.claude/rules/csharp.md`); no sleeps in tests.
- Toolchain: csharpier -> msbuild analyzers -> msbuild nullable/TreatWarningsAsErrors -> vstest,
  per CLAUDE.md; new files should be `#nullable enable` (precedent: `FolderSuggestionTree.cs:1`).

---

## G. Recommended approach and testable decomposition

### G.1 Candidate approaches considered

1. **NavigateToString-generated document + postMessage bridge (RECOMMENDED).** One C# HTML
   generator produces the full document (inline CSS/JS); JS posts interaction events via
   `window.chrome.webview.postMessage`; .NET renders updates either by re-navigating
   (simple, matches existing dark-mode re-render precedent) or by posting a `render` message the
   JS applies (finer-grained). All pure parts are unit-testable strings/state machines; zero new
   packaging; follows both existing precedents (NavigateToString, WebResourceRequested).
2. **Virtual-host-mapped on-disk assets (`SetVirtualHostNameToFolderMapping`) + separate
   .html/.css/.js files.** Cleaner front-end dev ergonomics, but introduces a new asset-packaging
   concern in a legacy VSTO csproj (content-copy + deployment), has no repo precedent, and makes
   the HTML untestable as a unit (files, not generated strings). Rejected for this feature.
3. **`AddHostObjectToScript` COM-projected host object.** Bidirectional calls without JSON, but
   COM marshaling semantics, harder to mock, no serializable contract to test, and heavier
   lifetime pitfalls in a pooled viewer. Rejected.

Rejected alternatives are (2) and (3) for the reasons above; also rejected by epic mandate:
any third-party WinForms tree/list control and WPF/ElementHost.

### G.2 Decomposition (spec/planner-ready)

**Pure, coverage-bearing (new-module >= 90%):**
- `BreadcrumbRow` / `BreadcrumbSegment` model + state machine (collapse-after-segment,
  leaf-expand, arrow transitions, banner/pseudo-row kinds) — `UtilitiesCS`.
- `BreadcrumbMessage` contracts + JSON codec (Newtonsoft) — `UtilitiesCS`.
- `BreadcrumbHtmlRenderer` (document/fragment generation incl. the §D.4 CSS) — `UtilitiesCS` or
  QuickFiler pure class.
- `BreadcrumbBridgeRouter` (inbound message -> state transition + provider query -> outbound
  messages; init-pending queue) — QuickFiler, tested via `Mock<IFolderHierarchyProvider>` +
  `Mock<IBreadcrumbWebHost>`.

**Host-bound, exempt with justification:**
- `WebView2BreadcrumbHost : IBreadcrumbWebHost` (SDK forwarding adapter).
- Designer swap in `EfcViewer.Designer.cs` (and `EfcViewer3.Designer.cs` per the §B.2 scope
  decision); `EfcFormController` wiring changes (already exempt class; keep to wiring only);
  WebView2 init through the existing `IWebViewCoreInitializer`.

**Order of work:** runtime repro capture (§D.3) -> pure model + contracts + renderer with tests ->
router with mocked provider/host -> host adapter + Designer swap + controller wiring -> manual
verification incl. AC 5 percent-visibility check -> remove repro instrumentation.

**Dependency risk:** 9101 is not on this branch; the plan must sequence the provider-consuming
tasks behind 9101's merge, or code against the assumed §C.3 interface with a single adapter class
as the re-alignment point.

## Evidence-location note

EVIDENCE_LOCATION_OVERRIDE_REJECTED: none required — all evidence paths in this document already
use the canonical `<FEATURE>/evidence/<kind>/` convention.
