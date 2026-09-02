# Spec: breadcrumb-bridge-keyboard-navigation-defects (Issue #737)

- Issue: #737
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/737
- Feature folder: docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737
- Work Mode: full-bug
- Research consulted: docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/research/2026-09-02T14-20-breadcrumb-bridge-keyboard-defects-research.md

## Summary

Issue #737 consolidates three code-review findings into one issue:

1. **Finding 1 (#640)** — arrow-key navigation in the breadcrumb selector has no viewport/scroll logic, so keyboard-driven selection can move off-screen with no visual feedback.
2. **Finding 2 (#641)** — the same keydown handling has no Enter binding, so a keyboard-only user cannot commit a highlighted suggestion.
3. **Finding 3 (#693)** — a router test discards two Arrange-phase results without assertion, masking a gap in left-arrow collapse coverage.

The issue's own framing describes these three findings as living on "one chain": WebView2 through the inline JS bridge through to `FolderBreadcrumbBridgeRouter`. Research completed for this feature folder disproves that framing and this spec corrects it (see "Correction to Issue Framing" below). The three findings sit on two structurally independent breadcrumb pipelines and must be treated, and delivered, as two separately-scoped fixes inside one issue.

## Correction to Issue Framing: Two Independent Breadcrumb Pipelines

The repository contains two structurally independent breadcrumb implementations that share only the row/segment domain concept, not any file:

- **Efc pipeline.** JavaScript asset in the file UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs (the `BridgeJs` constant, embedded into HTML by the file UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs), consumed by the router class in the file QuickFiler/Controllers/BreadcrumbBridgeRouter.cs, wired from the file QuickFiler/Controllers/EfcFormController.cs. This is the surface Findings 1 and 2 live on, and the only surface they live on.
- **Qfc pipeline.** A separate static HTML/JS file, QuickFiler/Resources/FolderBreadcrumb.html, with its own independently authored inline script, consumed by the router class in the file UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs, wired from the file QuickFiler/Viewers/ItemViewer.Breadcrumb.cs through the file QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs. Finding 3 touches `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs`, which tests this pipeline's router. Notably, the Qfc pipeline's own JS in QuickFiler/Resources/FolderBreadcrumb.html already implements both a `scrollIntoView({ block: "nearest" })` call and an Enter-key binding routed through the file QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs — the Qfc surface already has materially more mature keyboard/scroll behavior than the Efc surface for its own equivalent interactions.

Findings 1 and 2 (Efc-only JS) and Finding 3 (Qfc-only test) share no file, class, wire format, or row model. There is no single "chain" running through both. This spec's design and acceptance criteria treat them as two independently-scoped fixes delivered together inside this one issue, not as one code change.

## Prior-Regression Check (#440 Left/Right Ancestor-Walk)

Issue #440 added Left/Right ancestor-walk tree navigation to the breadcrumb selectors, merged via PR #689 (commit ecdb1c84). Issue #690 separately recorded and resolved a risk where a stale sibling branch could have silently reverted #440's behavior on merge with no conflict.

As part of this feature's research, the #440 fix was independently reverified as live on `origin/main`: the test `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` in `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` already contains an in-code comment citing #440 and asserts the ancestor-walk two-presses-to-root behavior on a three-segment fixture (`Inbox -> Projects -> Apollo`).

The research's conclusion, which this spec adopts as a design constraint:

- There is no overlap between the Enter/scroll-into-view fix (Findings 1 and 2) and #440's Left/Right tree-walk logic. Findings 1 and 2 touch only the Efc pipeline's `BreadcrumbDocumentAssets.cs` JS; #440's Efc-side implementation lives in the file QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs, and its Qfc-side implementation lives in the file UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs, the file UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs, and the file UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs. None of these files are part of the Findings 1/2 fix.
- Finding 3 sits directly on the #440 test surface: `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` is the exact test that carries the #440 regression-context comment. The fix for Finding 3 must preserve, not weaken, the existing "`RenderMessage` on presses 1 and 2, `UnhandledArrowMessage` on press 3" semantics of that test's ancestor walk. Asserting the two currently-discarded results as anything other than `RenderMessage` would contradict both the #440 contract and the sibling test `ArrowAsync_QfcLeftOnMultiSegmentRow_RoutesParentSelectTransition`, which already proves a single Left press on an equivalent multi-segment fixture yields a `RenderMessage`.

## Design

### Finding 1 (#640) — scroll-into-view

Add client-side-only DOM logic to the inbound message listener in the `BridgeJs` constant of the file UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs: after the `render`/`subfolderResult` DOM mutation completes (the branch currently ending around line 134), re-query `document.querySelector('.rowwrap.selected')` and, if a match is found, call `.scrollIntoView({ block: 'nearest' })` on it.

This requires no C# or wire-format change. `BreadcrumbHtmlRenderer.RenderRowFragment` (in the file UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs) already stamps the selected row wrapper with a `rowwrap selected` CSS class, and the existing keydown handler in `BreadcrumbDocumentAssets.cs` already uses the same `.rowwrap.selected` query after every render. The fix covers both outbound render origins uniformly — the full-list re-render (`rowId: null`, used by arrow-key navigation) and the single-row fragment re-render (`rowId` set, used by segment-collapse, leaf-toggle, and the Efc-side #440 Left/Right transitions) — because both land in the same inbound `render` branch of the JS listener.

### Finding 2 (#641) — Enter key binding

Add an `Enter` branch to the keydown listener in the same `BridgeJs` constant (the listener currently registered around lines 101-109), alongside the existing `ArrowLeft/ArrowRight/ArrowUp/ArrowDown` map. On `Enter`, the listener posts `{ type: 'rowSelected', rowId: id }`, using the identical `document.querySelector('.rowwrap.selected')` lookup the arrow-key handler already performs to obtain `id`.

This reuses the existing `BreadcrumbMessageTypes.RowSelected -> SelectRow(row)` C# path in the file QuickFiler/Controllers/BreadcrumbBridgeRouter.cs, which is the same path already triggered by a mouse click on a row (`post({ type: 'rowSelected', rowId: rowId })`). No new `BreadcrumbMessageTypes` constant, no new `IsKnownInboundType` branch in the file UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs, and no new `case` in `BreadcrumbBridgeRouter.ProcessInboundAsync` are required. A dedicated Enter-specific message type (Approach B in the research) was evaluated and rejected: it would touch three additional C# files (the codec, the message model, and the router) for no demonstrated behavioral difference from reusing `rowSelected`.

This design note carries forward from the research: `rowSelected`'s semantics ("select this row") are being reused for Enter's semantics ("commit the highlighted suggestion"), verified identical by inspection of the current C#-side handler. If a future state exists where a row and one of its rendered children are both visually highlighted, product intent should be revisited before Enter's target is assumed to always be the `.rowwrap.selected` row rather than an expanded child — this concern is noted for awareness and is out of scope for this fix, which targets the currently-reported gap (no Enter binding exists at all).

### Finding 3 (#693) — discarded test assertions

In `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs`, the test `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` currently calls `ArrowAsync(router, "left")` twice during Arrange (current lines approximately 373-378) and discards both results, only asserting on a third call. The fix captures both discarded results and asserts each is a `RenderMessage` (not an `UnhandledArrowMessage`), matching the pattern already established by the sibling test `ArrowAsync_QfcLeftOnMultiSegmentRow_RoutesParentSelectTransition` (current lines approximately 442-467), which asserts `outputs.Should().ContainSingle()` followed by a type assertion on the parsed message.

No change to the `ArrowAsync` helper's signature, to `PopulatedRouterAsync`, or to any provider-mock factory in this file is required; this is an additive verification-only change to one test method's Assert coverage.

## Test Strategy

- **Findings 1 and 2 (JS content).** There is no JS-execution test harness in this repository's test suite (confirmed: no headless browser or JS engine dependency exists in `UtilitiesCS.Test` or `QuickFiler.Test`). The precedented technique for verifying JS content in this repo is string-containment assertion, established by `BreadcrumbHtmlRendererTests.Issue439ActiveAncestorChildrenAndEmbeddedBridgeUseTypedStoppedActivation` in `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs`, which asserts substring containment against `BreadcrumbHtmlRenderer.RenderDocument`'s output. Add a new `[TestMethod]` to this same file following that precedent: render the document (or assert directly against the public `BreadcrumbDocumentAssets.BridgeJs` constant) and assert it contains the Enter-key branch's posted message shape (`rowSelected`) and the `scrollIntoView` call. This is an accepted, repo-conventional limitation: the test verifies the JS text is present and correctly shaped, not that it executes correctly in a real WebView2/Chromium document. Do not introduce a JS test runner or headless browser dependency to close this gap.
- **Finding 3 (router test).** This is a pure test-quality change with no production behavior change. Per the repo's Bugfix Workflow, there is no "red" state to reproduce first, because the underlying #440 behavior being asserted is already correct in production code — the change adds missing verification, it does not fix a live defect. Confirm the new assertions pass against current router behavior.
- **General.** After all three changes, run the full C# toolchain in policy order — `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`), the analyzer rebuild (`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`), the nullable rebuild (`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`), and `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` — restarting from formatting if any step fails or changes files, per CLAUDE.md and `.claude/rules/general-code-change.md`. New/modified tests must use MSTest with FluentAssertions per `.claude/rules/general-unit-test.md` and CLAUDE.md's C# Unit Test Policy, must not reduce coverage on changed lines, and must not introduce any temporary file usage.

## Write Set

- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs`
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs`

## Files Verified to Need No Change (Context / Precedent Only)

The following files were read or referenced during research as context, as precedent to follow, or as survey sites confirming the pipeline boundary described above. None of them are modified by this fix:

- QuickFiler/Resources/FolderBreadcrumb.html — the Qfc pipeline's own JS, which already has scroll-into-view and Enter-key behavior for its own protocol; must not be touched by an Efc-scoped fix.
- QuickFiler/Controllers/BreadcrumbBridgeRouter.cs — the Efc router that already has the `RowSelected -> SelectRow` case Finding 2 reuses; read as context, not modified.
- QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs — outbound render-message construction; read as context confirming both render origins reach the same JS listener branch, not modified.
- QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs — the Efc-side #440 tree-walk implementation; confirmed unaffected by the Finding 1 scroll addition, not modified.
- UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs — the Qfc pipeline's router, read only as context for Finding 3's target test file; must not be touched.
- UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs and UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs — the Qfc pipeline's #440 ancestor-walk state; must not be touched.
- QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs and QuickFiler/Viewers/ItemViewer.Breadcrumb.cs — Qfc pipeline wiring; read as context only, must not be touched.
- QuickFiler/Controllers/EfcFormController.cs — confirms how the Efc pipeline is wired and confirms the `Keys.Return` binding's keyboard-scope limitation described in Finding 2's root cause; must not be touched.
- UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs — confirms the `rowwrap selected` CSS class the scroll-into-view fix depends on; read as context, not modified.
- UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs and UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs — confirm the `RowSelected` message type already exists end-to-end; read as context confirming no new message type is needed, not modified.

## Acceptance Criteria

- [ ] In `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`'s `BridgeJs` constant, the inbound message listener scrolls the current `.rowwrap.selected` element into view (`scrollIntoView({ block: 'nearest' })`) after a `render` or `subfolderResult` DOM update, addressing Finding 1 (#640).
- [ ] In the same `BridgeJs` constant, the `keydown` listener includes an `Enter` branch that posts `{ type: 'rowSelected', rowId: id }` using the same `.rowwrap.selected` lookup the arrow-key handler already uses, addressing Finding 2 (#641), and requires no new C#-side message type, codec branch, or router case.
- [ ] A new MSTest test method in `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs`, following the existing `Issue439...` string-containment precedent, asserts the rendered document (or the `BreadcrumbDocumentAssets.BridgeJs` constant directly) contains the Enter-triggered `rowSelected` post and the `scrollIntoView` call, with the JS-execution-harness limitation documented in the test's own comment or docstring.
- [ ] In `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs`, `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft` captures both previously-discarded `ArrowAsync(router, "left")` results and asserts each parses to a `RenderMessage`, addressing Finding 3 (#693), without modifying the `ArrowAsync` helper signature or any shared provider-mock/router factory in the file.
- [ ] The fix for Finding 3 preserves the #440 ancestor-walk contract already documented in the test's in-code comment (two presses to reach the root on the three-segment fixture; `UnhandledArrowMessage` only on the third press), and is consistent with the sibling test `ArrowAsync_QfcLeftOnMultiSegmentRow_RoutesParentSelectTransition`.
- [ ] No file outside the Write Set is modified. In particular, no Qfc-pipeline file (QuickFiler/Resources/FolderBreadcrumb.html, UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs, UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs, UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs, QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs) and no #440 production logic in QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs is altered.
- [ ] The full C# toolchain (csharpier format/check, analyzer rebuild, nullable rebuild, vstest with coverage) passes cleanly in a single pass, per CLAUDE.md and `.claude/rules/general-code-change.md`, with no reduction in coverage on changed lines.

### Acceptance Criteria Status

- Source: docs/features/active/2026-09-02-breadcrumb-bridge-keyboard-navigation-defects-737/spec.md
- Total AC items: 7
- Checked off (delivered): 0
- Remaining (unchecked): 7
- Items remaining: all seven items above (no implementation has occurred yet; this spec is a planning artifact only)
