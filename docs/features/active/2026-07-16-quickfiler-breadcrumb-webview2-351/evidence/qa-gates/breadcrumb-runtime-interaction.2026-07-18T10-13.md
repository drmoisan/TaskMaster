# Breadcrumb Runtime Interaction Evidence (P6-T2, AC-3/AC-4/AC-5/AC-7, US-4/US-6) — STRUCTURAL-IMPOSSIBILITY DOSSIER

Timestamp: 2026-07-18T10-13

WhyRuntimeCaptureImpossible: No live Outlook host, VSTO add-in process, or interactive desktop
session exists in this execution environment; keyboard interaction, focus hand-off, and live
subfolder listing against a real mailbox cannot be exercised here. No runtime pass is claimed
for any item below; each is instead pinned by deterministic unit tests of the exact code path
the live interaction drives.

## Alternative proof — per-item mapping to deterministic evidence

(a) Keyboard Left/Right — Right expands when expandable, otherwise Pop Out/Enumerate dialog;
Left collapses, otherwise closes the folder control (AC-7, US-6): NOT RUNTIME-VERIFIED —
pinned by:
- `BreadcrumbBridgeRouterTests.Route_RightArrow_ExpandsWhenExpandable`,
  `Route_RightArrow_NothingToExpand_ReportsUnhandledRight`,
  `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` (string-in/string-out routing).
- `BreadcrumbStateModelTests.Arrows_RightExpandsThenLeftCollapses_UnhandledWhenNothingChanges`,
  `RightArrow_OnCollapsedRow_ReExpandsBeforeLeafExpansion`.
- `BreadcrumbBridgeCoordinatorTests.UnhandledRightArrow_RaisesUnhandledArrowRight`,
  `UnhandledLeftArrow_RaisesUnhandledArrowLeft`, `ArrowMessages_RaiseSyntheticFolderKeyDown`.
- The legacy fall-through wiring is code-pinned: `QfcItemController.ViewerSetup.cs`
  `EnsureBreadcrumbPipeline()` subscribes `BreadcrumbUnhandledArrow` to
  `KeyboardHandler.BreadcrumbArrowFallThrough`, whose Right branch invokes the identical
  `MyBox.ShowDialog("Pop Out Item or Enumerate Conversation?", ..., viewer.Controller.RightKeyActions)`
  call the legacy dropdown-open Right used, and whose Left branch invokes
  `viewer.SetFolderDroppedDown(false)` (the close-the-folder-control intent).

(b) Focus hand-off — `FocusFolderDropDown()`/`SetFolderDroppedDown(true)` land keyboard focus in
the breadcrumb WebView2 (spec risk item): NOT RUNTIME-VERIFIED — the glue is code-pinned
(`ItemViewer.FolderSearch.cs` delegations -> `ItemViewer.Breadcrumb.cs` `FocusBreadcrumb()` ->
`WebView2.Focus()`, plus the page's `window.addEventListener("focus", ...)` focusing the list
container in `FolderBreadcrumb.html`); WebView2 focus behavior itself cannot be unit-tested.

(c) Double-click collapse and plus re-expand on a non-leaf segment (AC-4): NOT RUNTIME-VERIFIED —
pinned by `BreadcrumbBridgeRouterTests.Route_SegmentDoubleClick_ProducesCollapsedRenderPayload`,
`Sequence_ExpandCollapseViaMessages_TransitionsDeterministically`,
`BreadcrumbStateModelTests.CollapseAfter_NonLeafSegment_HidesDownstreamAndClosesLeafExpansion`,
`ReExpand_AfterCollapse_RestoresTheFullChain`, and the page's `dblclick`/affordance `click`
handlers posting `segmentDoubleClick`/`affordanceToggle` (FolderBreadcrumb.html).

(d) Leaf affordance shown only when the leaf has subfolders (AC-3): NOT RUNTIME-VERIFIED —
pinned by `BreadcrumbRenderProjectionTests.Project_LeafWithoutSubfolders_RendersNoAffordance`,
`Project_LeafAffordance_PlusWhenClosedMinusWhenOpen`, and
`BreadcrumbStateModelTests.LeafHasSubfolders_TrueOnlyWhenLeafSegmentHasChildren` (driven by the
merged provider's `HasChildren` derivation pinned in
`FolderHierarchyProviderAdapterTests.ExpandComposition_SegmentKey_ListsRealImmediateSubfolders`).

(e) Expanding a segment lists real immediate Outlook subfolders not present among suggestions
(AC-5, US-4): NOT RUNTIME-VERIFIED against a live mailbox — pinned by
`BreadcrumbBridgeRouterTests.Route_AffordanceToggleExpand_QueriesProviderAndReturnsRenderPlusResponse`
(provider queried by segment key; children returned are the provider's, not the suggestion set)
and the merged #350 provider's own tests
(`OutlookFolderHierarchyProviderTests.GetImmediateSubfoldersAsync_PopulatedSegment_ReturnsRealChildren`)
which bind the provider to the live snapshot service seam; the live Outlook query itself is
behind `IOutlookFolderTreeService` (#350 scope).

Per-item verdict lines: (a) DOSSIER-PINNED, (b) DOSSIER-PINNED (focus glue code-pinned only),
(c) DOSSIER-PINNED, (d) DOSSIER-PINNED, (e) DOSSIER-PINNED. No item is recorded as a runtime pass.

MANUAL-VERIFICATION-REQUIRED: yes — the maintainer must verify in the live add-in: (a) Right
expands / opens the Pop Out dialog appropriately and Left collapses / closes appropriately;
(b) `F`-key/search-down focus lands in the breadcrumb and arrows are received by the page;
(c) double-click collapse and plus re-expand; (d) affordance only on leaves with subfolders;
(e) expansion lists real immediate subfolders of the segment from the live mailbox.
