# Research: breadcrumb-bridge-keyboard-navigation-defects (#737)

- **Issue:** #737
- **Feature folder:** `docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737`
- **Scope:** Read-only research supporting `spec.md` / a future `plan.md` for Findings #640 (scroll-into-view), #641 (Enter key), #693 (discarded test assertions).
- **Method:** Direct file reads and greps against the working tree on branch `bug/breadcrumb-bridge-keyboard-navigation-defects-737` (branched from `origin/main`). No files were modified.

## 0. Headline correction to the issue's framing — TWO parallel breadcrumb pipelines exist, not one

The issue text describes a single chain: "WebView2 -> inline JS bridge in `BreadcrumbDocumentAssets.cs` -> `FolderBreadcrumbBridgeRouter.cs`". Verification shows this chain is **inaccurate as a single path** — the repository contains two structurally independent breadcrumb implementations that share only the row/segment domain types and the `IFolderHierarchyProvider` seam:

| | **Efc pipeline** (consumes `BreadcrumbDocumentAssets.cs`) | **Qfc pipeline** (`ItemViewer`) |
|---|---|---|
| JS asset | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` `BridgeJs` constant, embedded into HTML by `BreadcrumbHtmlRenderer.RenderDocument` | Static file `QuickFiler/Resources/FolderBreadcrumb.html` (own inline `<script>`), loaded via `Properties.Resources.FolderBreadcrumb` |
| C# router | `QuickFiler.Controllers.BreadcrumbBridgeRouter` (+ `.Arrows.cs`, `.Selection.cs` partials) | `UtilitiesCS.OutlookObjects.Folder.FolderBreadcrumbBridgeRouter` |
| Wire codec | `UtilitiesCS.OutlookObjects.Folder.BreadcrumbMessageCodec` (`BreadcrumbMessages.cs`) | `UtilitiesCS.OutlookObjects.Folder.BreadcrumbBridgeSerializer` (`BreadcrumbBridgeMessages.cs`) |
| Row model | `BreadcrumbRow` (`BreadcrumbRow.cs`) | `BreadcrumbStateRow` (`BreadcrumbStateModel.Row.cs`) |
| Wired from | `EfcFormController.ConfigureBreadcrumbControl()` (`QuickFiler/Controllers/EfcFormController.cs:943-963`) | `ItemViewer.InitializeBreadcrumbPipeline` (`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:46-85`) via `BreadcrumbBridgeCoordinator` (`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`) |
| Render payload shape | Pre-rendered HTML fragment: `{type:'render', rowId, html}` | Structured row JSON: `{type:'render', rows:[...], selectedSubfolderIndex, selectedFolder}` |

Both pipelines independently implement the #440 ancestor-walk (Left/Right tree navigation): the Efc pipeline in `BreadcrumbRow.LeftArrow()`/`ActivateSegment()` + `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` (comments citing "#440" at lines 47, 69-72, 100-105), and the Qfc pipeline in `BreadcrumbStateModel.cs` / `BreadcrumbStateModel.Row.cs` / `FolderBreadcrumbBridgeRouter.cs` (comment at `FolderBreadcrumbBridgeRouter.cs:418`).

**Consequence for scoping this issue:**
- Findings 1 (#640) and 2 (#641) are about `BreadcrumbDocumentAssets.cs`'s `BridgeJs` — this file is loaded **only** by the Efc pipeline. The Qfc pipeline never executes this JS.
- Finding 3 (#693) is about `FolderBreadcrumbBridgeRouterTests.cs`, which tests `UtilitiesCS.OutlookObjects.Folder.FolderBreadcrumbBridgeRouter` — the **Qfc** pipeline's router, and the file that literally carries the `#440` regression-context comment cited in the delegation prompt.
- The three findings therefore sit on **two different surfaces**: 1 and 2 are Efc-only JS/wiring work; 3 is a Qfc-only test-quality fix. A plan should treat them as independently scoped work items inside one issue, not as one code change.
- The Qfc pipeline's own JS (`FolderBreadcrumb.html`) already has both a scroll-into-view call (line 391: `activeRow.scrollIntoView({ block: "nearest" })`, gated to `state.viewMode === "expanded"`) and an Enter-key binding (`selectorKeys.Enter = "enter"` at line 410, routed to `BreadcrumbSelectorKey.Enter -> CommitSelector()` in `BreadcrumbBridgeCoordinator.HandleSelectorKey`, `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:159-170`). Findings 1 and 2 describe a real gap only in the Efc surface; the Qfc surface already has materially more mature keyboard/scroll behavior for its own equivalent interactions. Nothing in this research proposes changing `FolderBreadcrumb.html`.

## 1. `BreadcrumbDocumentAssets.cs` — exact current content (task 1)

Full file: `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` (138 lines). Confirmed against `origin/main`-branched working tree, matches the issue's citations exactly:

- **Keydown map** — lines 101-109:
  ```
  101	  document.addEventListener('keydown', function (e) {
  102	    var map = { ArrowLeft: 'Left', ArrowRight: 'Right', ArrowUp: 'Up', ArrowDown: 'Down' };
  103	    var key = map[e.key];
  104	    if (!key) { return; }
  105	    var selected = document.querySelector('.rowwrap.selected');
  106	    var id = selected ? selected.getAttribute('data-row-id') : '';
  107	    post({ type: 'arrowKey', rowId: id || '', key: key });
  108	    e.preventDefault();
  109	  });
  ```
  No `Enter` key is present in `map`; any other key value is a no-op return at line 104.

- **Inbound message listener / `render` handler** — lines 110-135, `render` case at 114-121:
  ```
  114	      if (msg.type === 'render') {
  115	        if (msg.rowId) {
  116	          var target = document.querySelector('[data-row-id="' + msg.rowId + '"]');
  117	          if (target) { target.outerHTML = msg.html; }
  118	        } else {
  119	          var list = document.getElementById('rows');
  120	          if (list) { list.innerHTML = msg.html; }
  121	        }
  122	      } else if (msg.type === 'subfolderResult') { ... }
  ```
  No `scrollIntoView` (or any other viewport-related) call anywhere in the file — confirmed by reading the file in full; the string does not occur.

- Other JS emitters in the same file (for context on the message-type family a new binding should join): `segmentDoubleClick` (59-67), `segmentActivate` (68-78), `renderedChildActivate` (79-88), `leafExpandToggle` (89-94), `rowSelected` (95-99), `arrowKey` (101-109).

## 2. C#-side inbound message handling and reusable infrastructure (task 2)

The router that actually receives `BreadcrumbDocumentAssets.cs`'s postMessage calls is `QuickFiler.Controllers.BreadcrumbBridgeRouter` (doc comment: "Non-exempt bridge router for the EfcViewer breadcrumb control (#349)", `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:12-18`), wired from `EfcFormController.ConfigureBreadcrumbControl()` at `QuickFiler/Controllers/EfcFormController.cs:952-958`:
```
952	            _router = new BreadcrumbBridgeRouter(
953	                provider,
954	                _breadcrumbHost,
955	                new UtilitiesCS.OutlookObjects.Folder.BreadcrumbMessageCodec(),
956	                new UtilitiesCS.OutlookObjects.Folder.BreadcrumbHtmlRenderer(),
957	                new BreadcrumbOutboundQueue(_breadcrumbHost)
958	            );
```

Dispatch is `BreadcrumbBridgeRouter.ProcessInboundAsync(string json)`, `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:233-289`. It decodes via `_codec.DeserializeInbound(json)` (`BreadcrumbMessageCodec.DeserializeInbound`, `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs:73-126`) and switches on `message.Type` (lines 243-288):

```
245  case BreadcrumbMessageTypes.SegmentDoubleClick: ... row.CollapseAfter(...); PostRowRender(row);
273  case BreadcrumbMessageTypes.SegmentActivate:      ActivateSegment(row, message.SegmentIndex!.Value);
276  case BreadcrumbMessageTypes.RenderedChildActivate: ActivateChild(row, message.ChildIndex!.Value);
279  case BreadcrumbMessageTypes.LeafExpandToggle:      await HandleLeafToggleAsync(row);
282  case BreadcrumbMessageTypes.ArrowKey:               await HandleArrowKeyAsync(row, message.Key!);
285  case BreadcrumbMessageTypes.RowSelected:            SelectRow(row);
```

Every JS-emitted message type in `BreadcrumbDocumentAssets.cs` has a matching `case` here — the codec's wire-type constants (`BreadcrumbMessageTypes` in `BreadcrumbMessages.cs:10-38`) were checked against the exact strings the JS posts (`segmentDoubleClick`, `segmentActivate`, `renderedChildActivate`, `leafExpandToggle`, `arrowKey`, `rowSelected`) and all six line up one-to-one. (An earlier pass of this research mis-attributed a message-type mismatch to a *different*, unrelated router — `UtilitiesCS.OutlookObjects.Folder.FolderBreadcrumbBridgeRouter`/`BreadcrumbBridgeSerializer` — which is the Qfc pipeline's router and never receives this JS's messages at all; that mismatch claim is retracted and is not a defect in the Efc path this issue concerns.)

**Reusable infrastructure for Enter:** `BreadcrumbMessageTypes.RowSelected` ("rowSelected") already exists and its handler is exactly the semantic Enter needs — "commit the highlighted suggestion":
```
285  case BreadcrumbMessageTypes.RowSelected:
286      SelectRow(row);
287      break;
```
`SelectRow(BreadcrumbRow)` (`QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs:83-119`) is the same method the JS's mouse-click handler already triggers (`post({ type: 'rowSelected', rowId: rowId })`, `BreadcrumbDocumentAssets.cs:98`). No new C#-side message type or router case is required to add Enter: the JS keydown map only needs a new `Enter` entry that posts `{ type: 'rowSelected', rowId: id }` using the exact same `document.querySelector('.rowwrap.selected')` lookup the arrow-key handler already performs (lines 105-106). This is the minimal, infrastructure-reusing design spec.md's Test Strategy checklist already anticipates ("posting a select/activate-style message the C# side already has infrastructure to handle").

If a future design wants Enter to behave differently from a plain row click (e.g. suppress in some state), a dedicated message type would need a new `BreadcrumbMessageTypes` constant, a new `IsKnownInboundType` branch (`BreadcrumbMessageCodec.cs:128-136`), and a new `case` in `ProcessInboundAsync`. The evidence above shows the reuse path is available without that additional surface.

## 3. Render message origin and scroll-into-view hook point (task 3)

Two call sites in `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` construct outbound `render` messages via `BreadcrumbRenderMessage(html, rowId)` (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs:108-126`):

- **Full-list re-render, `rowId: null`** — `CommitSelection`, lines 143-151:
  ```
  143	        private void CommitSelection(BreadcrumbRow row, string selection)
  144	        {
  145	            _selectedRowId = row.RowId;
  146	            SelectedFolderPath = selection;
  147	            PostOutbound(
  148	                new BreadcrumbRenderMessage(_renderer.RenderRows(_rows, _selectedRowId), null)
  149	            );
  150	            SelectedFolderPathChanged?.Invoke(this, SelectedFolderPath);
  151	        }
  ```
  This is the path Up/Down arrow-key navigation uses (`MoveSelection`/`HandleUpArrow` in `BreadcrumbBridgeRouter.Arrows.cs:88-163` call `SelectRow(next/previous)` at `BreadcrumbBridgeRouter.Selection.cs:83-119`, which ends in `CommitSelection`). Because `rowId` is `null`, the JS's `render` handler takes the `list.innerHTML = msg.html` branch (`BreadcrumbDocumentAssets.cs:118-121`), i.e. the exact branch Finding 1 describes as having no scroll logic.

- **Single-row fragment re-render, `rowId` set** — `PostRowRender`, lines 153-161:
  ```
  153	        private void PostRowRender(BreadcrumbRow row)
  154	        {
  155	            PostOutbound(
  156	                new BreadcrumbRenderMessage(
  157	                    _renderer.RenderRowFragment(row, row.RowId == _selectedRowId),
  158	                    row.RowId
  159	                )
  160	            );
  161	        }
  ```
  Used by segment-collapse, leaf-toggle, and the #440 Left/Right tree-transition branches in `BreadcrumbBridgeRouter.Arrows.cs`.

**No wire-format change is needed to add a scroll target.** `BreadcrumbHtmlRenderer.RenderRowFragment` already stamps the selected row with a `selected` CSS class in the wrapper it emits (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs:94-95`: `string wrapClass = "rowwrap" + (isSelected ? " selected" : string.Empty);`), and the existing keydown handler in `BreadcrumbDocumentAssets.cs` already queries `document.querySelector('.rowwrap.selected')` (line 105) after every render. A scroll-into-view fix can therefore run entirely client-side: after either branch of the `render` handler (lines 114-121) finishes updating the DOM, re-query `.rowwrap.selected` and call `.scrollIntoView({ block: 'nearest' })` on it if found — this covers both the full-list (`CommitSelection`) and per-row-fragment (`PostRowRender`) origins uniformly, without touching `BreadcrumbBridgeRouter.cs`, `BreadcrumbMessages.cs`, or `BreadcrumbHtmlRenderer.cs`.

## 4. `EfcFormController`'s `Keys.Return` binding and its relationship to the WebView2 popup (task 4)

`EfcFormController.RegisterAlwaysOnAsyncKeyActions`, `QuickFiler/Controllers/EfcFormController.cs:410-422`:
```
410	        internal void RegisterAlwaysOnAsyncKeyActions()
411	        {
412	            _formViewer.KeyboardHandler.AlwaysOnKeyActionsAsync = new KbdActions<
413	                Keys,
414	                KaKeyAsync,
415	                Func<Keys, Task>
416	            >(
417	                new List<KaKeyAsync>
418	                {
419	                    new KaKeyAsync("Collection", Keys.Return, (k) => ActionOkAsync()),
420	                }
421	            );
422	        }
```
This binding is dispatched by `KeyboardHandler.KeyboardHandler_KeyDownAsync` (`QuickFiler/Controllers/KeyboardHandler.cs:133-159`), which only fires when a **WinForms `KeyDown`** event reaches a control the handler is wired to. `EfcFormController.WireEventHandlers` (`QuickFiler/Controllers/EfcFormController.cs:424-438`) attaches that handler via `_formViewer.ForAllControls(x => { x.KeyDown += ...; x.PreviewKeyDown += ...; }, ...)` — a recursive walk over the form's WinForms `Control` tree.

The breadcrumb selector is hosted in a `Microsoft.Web.WebView2.WinForms.WebView2` control (`L0vhBreadcrumb_WebView2`, `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:25-29`; the Efc-side equivalent is the `BreadcrumbWebView` referenced in `EfcFormController.ConfigureBreadcrumbControl`, line 946). Once the WebView2 browser process has input focus, keystrokes are consumed inside the Chromium renderer and routed through `window.chrome.webview` message-passing — they do not surface as WinForms `KeyDown`/`PreviewKeyDown` events on the hosting control while the Chromium document itself has focus. `BreadcrumbDocumentAssets.cs`'s own `keydown` listener (line 101) is what actually receives keystrokes at that point, and it has no `Enter` entry (§1). This confirms Finding 2's claim precisely: `EfcFormController`'s `Keys.Return -> ActionOkAsync()` binding is real, wired, and reachable when a plain WinForms control (e.g., the item list) has focus, but it is a **different keyboard-scope owner** than the WebView2 popup and does not receive Enter while the popup's Chromium document owns focus.

## 5. `FolderBreadcrumbBridgeRouterTests.cs` — `ArrowAsync` helper and surrounding structure (task 5)

File: `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` (492 lines), `[TestClass] public sealed partial class FolderBreadcrumbBridgeRouterTests` (line 24; a sibling partial class exists at `FolderBreadcrumbBridgeRouterInFlightTests.cs` per the class doc comment, lines 14-21).

Helper, lines 429-436:
```
429	        private static Task<IReadOnlyList<string>> ArrowAsync(
430	            FolderBreadcrumbBridgeRouter router,
431	            string direction
432	        ) =>
433	            router.RouteAsync(
434	                "{\"type\":\"arrowKey\",\"direction\":\"" + direction + "\"}",
435	                CancellationToken.None
436	            );
```
It is a thin wrapper over `FolderBreadcrumbBridgeRouter.RouteAsync` (the Qfc-pipeline router, `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs:284-354`) that builds one `arrowKey` JSON payload and returns the awaited output list — no filtering or interpretation of the result, so callers are responsible for asserting on the returned `IReadOnlyList<string>`.

Target test, `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft`, lines 367-385:
```
367	        [TestMethod]
368	        public async Task Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft()
369	        {
370	            // Arrange: #440 Left walks the ancestor chain, so on this three-segment fixture two
371	            // presses are needed to reach the root. Only once the root is active does nothing
372	            // remain to collapse and no further tree transition apply.
373	            var router = await PopulatedRouterAsync(ProviderMock());
374	            await ArrowAsync(router, "left");
375	            await ArrowAsync(router, "left");
376	
377	            // Act
378	            var outputs = await ArrowAsync(router, "left");
379	
380	            // Assert
381	            outputs.Should().ContainSingle();
381	            ((UnhandledArrowMessage)BreadcrumbBridgeSerializer.Parse(outputs[0]))
382	                .Direction.Should()
384	                .Be(BreadcrumbArrowDirection.Left);
385	        }
```
The router is populated with a three-segment fixture (`Inbox -> Projects -> Apollo`, `LeafChain()`, `PopulatedRouterAsync`, lines 78-91) whose active segment starts at the leaf. Per the #440 comment, each Left press moves the active segment one step toward the root (`_model.LeftArrow()`), so:
- **Press 1** (line 374, discarded): active segment moves from `Apollo` (leaf, index 2) to `Projects` (index 1). Handled — expected to be a `RenderMessage`, not `UnhandledArrowMessage`.
- **Press 2** (line 375, discarded): active segment moves from `Projects` (index 1) to `Inbox` (root, index 0). Also handled — expected to be a `RenderMessage`.
- **Press 3** (line 378, asserted): now at the root with nothing left to collapse or walk toward — correctly asserted as `UnhandledArrowMessage`.

A real-assertion fix should capture each of the two discarded results and assert `BreadcrumbBridgeSerializer.Parse(outputsN[0])` is a `RenderMessage` (not `UnhandledArrowMessage`) for presses 1 and 2, matching the pattern already established by the sibling test `ArrowAsync_QfcLeftOnMultiSegmentRow_RoutesParentSelectTransition` (lines 442-467), which asserts a single Left press on this same kind of fixture "produced a render, not an unhandled-arrow fall-through" and checks `outputs.Should().ContainSingle()` + `BeOfType<RenderMessage>()`. Reusing `ArrowAsync`'s existing signature (`Task<IReadOnlyList<string>>`) is sufficient; no helper signature change is required — only capturing and asserting on what the two Arrange-phase calls already return.

Other structural notes relevant to not breaking sibling tests: the file uses three private factory helpers for provider mocks (`ProviderMock`, `StemProviderMock`, `ParentSubfolderProviderMock`, each `MockBehavior.Strict`) and one router factory (`PopulatedRouterAsync`, lines 78-91) that all other tests in the file, including the two `#440` Qfc tree-navigation tests at the bottom (lines 407-490), depend on unchanged.

## 6. Efc vs Qfc JS asset sharing (task 6)

**Not shared.** Confirmed by production wiring, not just by file existence:
- Efc: `EfcFormController.ConfigureBreadcrumbControl()` (`EfcFormController.cs:943-963`) builds `QuickFiler.Controllers.BreadcrumbBridgeRouter` with `new UtilitiesCS.OutlookObjects.Folder.BreadcrumbHtmlRenderer()` (line 956), whose `RenderDocument` embeds `BreadcrumbDocumentAssets.BaseCss/LightThemeCss/DarkThemeCss/BridgeJs` (`BreadcrumbHtmlRenderer.cs:38-52`) and delivers the result via `_host.NavigateToString(document)` (`BreadcrumbBridgeRouter.Selection.cs:168-180`).
- Qfc: `ItemViewer.Breadcrumb.cs:107-123` (`CreateCollapsedBreadcrumbCandidate`) calls `_l0vhBreadcrumb_WebView2.NavigateToString(Properties.Resources.FolderBreadcrumb)` — a static resource (`QuickFiler/Properties/Resources.resx:133-135`) backed by the physical file `QuickFiler/Resources/FolderBreadcrumb.html`, which contains its own complete, independently-authored inline `<style>`/`<script>` (491 lines total) with a materially different (structured-JSON, "selector" open/collapsed view-mode) protocol.

`BreadcrumbHtmlRenderer` itself is a shared, host-neutral class (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs`) — it is unit-tested independent of any consumer (`UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs`) and is also referenced from several `QuickFiler.Test` files under `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue*Tests.cs` — but those all construct the same `QuickFiler.Controllers.BreadcrumbBridgeRouter` (Efc pipeline; the "queue"/"issue439"/"issue614"/"issue637" test files exercise this router's Left/Right/segment/selection logic against the Efc row model). No test or production file was found that feeds `BreadcrumbDocumentAssets`/`BreadcrumbHtmlRenderer` output into the Qfc `ItemViewer`/`BreadcrumbBridgeCoordinator` pipeline.

**Conclusion:** a fix scoped to `BreadcrumbDocumentAssets.cs` affects only the Efc (`EfcFormController`/`EfcViewer`) surface. It has zero effect on the Qfc (`ItemViewer`) surface, which already has its own scroll-into-view and Enter-key behavior in `FolderBreadcrumb.html`.

## 7. Existing test coverage of `BreadcrumbDocumentAssets.cs`'s JS content (task 7)

No test anywhere references `BreadcrumbDocumentAssets`, `BridgeJs`, `BaseCss`, `LightThemeCss`, or `DarkThemeCss` by name (`grep` across `UtilitiesCS.Test/` and `QuickFiler.Test/` returned zero matches for all five identifiers). There is no JS-execution test harness (no headless browser / JS engine dependency) anywhere in the two test projects for this feature area.

Coverage of the JS *content* is indirect and shallow, through one test: `BreadcrumbHtmlRendererTests.Issue439ActiveAncestorChildrenAndEmbeddedBridgeUseTypedStoppedActivation` (`UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs:66-94`), which calls `_renderer.RenderDocument(...)` (embedding the full `BridgeJs` string) and asserts plain substring containment against the resulting document string, e.g.:
```
90	            document.Should().Contain("type: 'segmentActivate'");
91	            document.Should().Contain("type: 'renderedChildActivate'");
92	            document.Should().Contain("e.stopPropagation();");
93	            document.Should().NotContain("ItemViewer");
```
This establishes the precedent and the only available test technique for this file: string-containment assertions against the rendered document (or, more directly, against the `BreadcrumbDocumentAssets.BridgeJs` constant itself, which is public and can be asserted on directly without going through `BreadcrumbHtmlRenderer`). A test addition for Finding 1/2 should follow this same pattern — e.g. `BreadcrumbDocumentAssets.BridgeJs.Should().Contain("Enter: 'select'")` (or whatever literal the implementation emits) and `.Should().Contain("scrollIntoView")` — rather than attempting to execute or simulate the JS. All remaining coverage of this feature area is exercised through the C#-side router tests (`FolderBreadcrumbBridgeRouterTests.cs` for Qfc; `BreadcrumbBridgeRouterTests.cs`/`BreadcrumbBridgeRouterQueueTests.cs`/`BreadcrumbBridgeRouterIssue*Tests.cs` for Efc), which test the C# state machine driven by hand-built JSON strings simulating what the JS would send — they never execute the JS itself.

## 8. #440 regression-risk check (explicit section per delegation prompt)

**Conclusion: no overlap between Findings 1/2's proposed JS changes and the #440 ancestor-walk logic; Finding 3 is directly on the #440 test surface and must preserve its existing semantics.**

Evidence:

1. **Findings 1 and 2 touch only `BreadcrumbDocumentAssets.cs` (Efc pipeline JS).** Per §0/§6, this file is never loaded by the Qfc `ItemViewer` pipeline, and the #440 fix's Qfc-side code (`BreadcrumbStateModel.cs`, `BreadcrumbStateModel.Row.cs`, `FolderBreadcrumbBridgeRouter.cs`) lives entirely outside the Efc pipeline. There is no shared file, class, or JS asset between "add an Enter key mapping to `BridgeJs`" / "add scroll-into-view to `BridgeJs`'s render handler" and the Qfc ancestor-walk implementation.

2. **The #440 fix also has an Efc-pipeline copy** (`QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs`, comments at lines 47, 69-72, 100-105), which *does* post `render` messages that would be received by the same `BreadcrumbDocumentAssets.cs` JS a scroll-into-view fix would modify (`PostRowRender` calls from `TryRightTreeTransitionAsync`/the Left-arrow branch, `BreadcrumbBridgeRouter.Arrows.cs:78, 84, 125`). A JS-only scroll-into-view addition — re-querying `.rowwrap.selected` and calling `scrollIntoView` after every `render` message is applied (§3) — is purely additive DOM behavior: it does not read, write, or branch on any state the #440 tree-walk logic owns (`ActiveSegmentIndex`, `ActivateSegment`, `LeftArrow`/`TryRightTreeTransitionAsync`). It would also fire for #440-triggered renders in the Efc pipeline, which is a cosmetic improvement (the row the #440 transition just selected becomes visible), not a semantic change to which row gets selected or how the tree walk decides. No regression risk was found.

3. **Finding 2 (Enter key) reuses the existing `rowSelected`/`SelectRow` path** (§2), which is unrelated to `ArrowKey`/`HandleArrowKeyAsync`/the `#440` tree-walk switch cases in `BreadcrumbBridgeRouter.Arrows.cs:42-98`. Adding an `Enter` JS mapping does not add a new C# case to that switch and cannot alter Left/Right routing.

4. **Finding 3 is the one item genuinely inside the #440 surface**: it modifies `FolderBreadcrumbBridgeRouterTests.cs`, the same file whose `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` test carries the `#440` comment cited in the delegation prompt (confirmed present at lines 370-372, verbatim: "#440 Left walks the ancestor chain, so on this three-segment fixture two presses are needed to reach the root"). A fix here must assert `RenderMessage` (not `UnhandledArrowMessage`) for the first two `ArrowAsync(router, "left")` calls (§5) — asserting the opposite would directly contradict both the #440 ancestor-walk contract and the sibling test `ArrowAsync_QfcLeftOnMultiSegmentRow_RoutesParentSelectTransition` (lines 442-467), which already proves a single Left press on an equivalent multi-segment fixture yields a `RenderMessage`. Getting this backwards would be a genuine, self-inflicted regression of the #440 fix's test coverage; getting it right (per the derivation in §5) reinforces it.

## Candidate approaches (Findings 1 and 2 — Efc-only JS changes)

**Approach A — JS-only, reuse existing message types (recommended).**
- Enter: add `Enter: 'select'` is unnecessary — simplest form is a second `keydown` branch (or extending the existing `map`) that, on `Enter`, posts `{ type: 'rowSelected', rowId: id }` using the same `document.querySelector('.rowwrap.selected')` lookup already at lines 105-106, reusing the existing `BreadcrumbMessageTypes.RowSelected -> SelectRow(row)` C# path (§2).
- Scroll: after the `render`/`subfolderResult` DOM mutation in the inbound listener (lines 114-134), re-query `.rowwrap.selected` and call `.scrollIntoView({ block: 'nearest' })` if found (§3).
- Advantages: no C# router/message/test changes required for Enter; no wire-format change for scroll; both fixes are localized entirely to `BreadcrumbDocumentAssets.cs`, matching the file's existing single-responsibility scope (inline CSS/JS constants only) and the repo's 500-line file-size ceiling (file is 138 lines; ample headroom).
- Limitations: `rowSelected`'s semantics ("select this row") are being reused for Enter's semantics ("commit the highlighted suggestion") — verified identical by inspection (§2) but should be confirmed against product intent if Enter is ever expected to differ from a click (e.g., during an open leaf-expansion child list, `renderedChildActivate` might be the more correct target for Enter — out of scope for this research to decide; a plan should state which target row Enter commits when both a row and an expanded child are visually highlighted, since the JS currently only tracks one `.selected` class on the row wrapper, not on `.child` elements).

**Approach B — new dedicated C#-side message type for Enter (rejected as the primary approach).**
- Would add a new `BreadcrumbMessageTypes` constant, a new `IsKnownInboundType` branch, and a new `ProcessInboundAsync` case that behaves identically to `RowSelected` today.
- Rejected because it duplicates existing, already-correct infrastructure (§2) with no behavioral difference demonstrated as necessary, and would require touching `BreadcrumbMessages.cs`, `BreadcrumbMessageCodec.cs`, and `BreadcrumbBridgeRouter.cs` in addition to `BreadcrumbDocumentAssets.cs` — more surface than the fix needs on current evidence.

**Rejected alternative for Finding 1** — driving the scroll target from the C# render payload (adding a `scrollRowId` field to `BreadcrumbRenderMessage`) was considered and rejected: it requires a wire-format/serializer change (`BreadcrumbMessages.cs`) and a `BreadcrumbHtmlRenderer` change, for no behavioral gain over the JS-only DOM re-query approach, which already has the necessary `data-row-id`/`selected` markers available post-render (§3).

## Testing implications

- Finding 1/2 fixes are confined to a single file with no existing direct test coverage (§7); the available, precedented technique is string-containment assertions against `BreadcrumbDocumentAssets.BridgeJs` (or against `BreadcrumbHtmlRenderer.RenderDocument`'s output, per the `Issue439...` precedent) — e.g. asserting the map contains an `Enter` key and the render handler contains a `scrollIntoView` call. This does not exercise real DOM/JS behavior (no JS engine in the test suite) and should be documented as a known limitation, consistent with repo convention (`BreadcrumbHtmlRendererTests.cs`'s existing tests carry the same limitation).
- The C#-side reuse path for Enter (`RowSelected` -> `SelectRow`) already has coverage through existing Efc router tests exercising `rowSelected` — a plan should confirm/extend that coverage for the Enter-triggered case if the fix posts through a different code path than a literal mouse click (on current evidence it is the identical message, so no new C# test should be strictly required beyond the JS-string assertion, but the deciding factor is the exact wire payload chosen during implementation).
- Finding 3's fix is a pure test-quality change to `FolderBreadcrumbBridgeRouterTests.cs`; per repo Bugfix Workflow it should start from a red assertion (assert `RenderMessage` on outputs 1 and 2, confirm it currently passes trivially only because the prior code discarded the value — i.e., there is no "red" state to reproduce here since the underlying #440 behavior is already correct; the test change is additive verification, not a behavior fix) and finish by running the full C# toolchain (csharpier, analyzer rebuild, nullable rebuild, vstest) per `CLAUDE.md`.
- No production behavior changes are proposed for the Qfc pipeline; no Qfc-side tests need updating for Findings 1-3 as scoped by this research.
