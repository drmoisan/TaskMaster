# quickfiler-breadcrumb-webview2 — Spec

- **Issue:** #351
- **Epic:** folder-tree-breadcrumb-redesign (integration branch
  `epic/folder-tree-breadcrumb-redesign-integration`), decomposition item 3
  (manifest placeholder 9103, wave 1, complexity band C4)
- **Depends on:** issue 9101 (live Outlook folder-hierarchy provider), merged before this
  feature executes
- **Independent of:** issue 9102 (EfcViewer breadcrumb); executes in parallel
- **Owner:** drmoisan
- **Last Updated:** 2026-07-16
- **Status:** Ready for planning
- **Version:** 1.0
- **Work Mode:** full-feature
- **Research:** `research/2026-07-16T22-30-quickfiler-breadcrumb-webview2-research.md`

## Overview

The `folder-tree-percentage-ui` epic (issues 324, 325, 326, 327) delivered a QuickFiler folder
dropdown whose behavior does not match the intended design. The dropdown is still a stock
`System.Windows.Forms.ComboBox` (`CboFolders`, `DrawMode=OwnerDrawFixed`) in the live
`ItemViewer` variant. Its hierarchy is synthesized by `FolderHierarchyBuilder.Build`, which
splits the same set of at most five suggestion paths on `\`; it does not query the real Outlook
subfolder structure. The prediction percentage is not reliably fully visible.

This feature replaces that control with a WebView2-hosted HTML/CSS/JS breadcrumb control and a
new JS<->.NET event bridge, following the pattern QuickFiler already uses for its WebView2
message-body pane (`QfcItemController.ViewerSetup.cs`, `WebView2CoreInitializer` /
`IWebViewCoreInitializer`, `ItemViewer.WebViewThread.cs`). It is the QuickFiler-surface
counterpart to the EfcViewer breadcrumb (9102) and consumes the shared live folder-hierarchy
provider (9101).

## Scope

### Scope decision: live viewer variant = `ItemViewer` only

Research re-verified viewer-variant liveness on this branch (research §1, §9): the only
production construction site is `QuickFiler\Helper Classes\ItemViewerQueue.cs:105`
(`return new ItemViewer();` inside `CreateProductionViewer()`, wired into the hard-bound
`ViewerQueueCore<ItemViewer>` pool). No DI registration, factory, or reflection path constructs
any other variant. The nine other declared viewer variants (`Form1`, `ItemViewerExpanded`,
`QfcItemViewer`, `QFCItemViewerDarkNew`, `QfcItemViewerExpanded`, `QfcItemViewerExpandedLight`,
`QFCItemViewerLightNew`, `QfcItemViewerLightSelected`, `QfcItemViewerV1`) are Designer-only dead
types.

**Decision:** the WebView2 breadcrumb replaces `CboFolders` in the single live `ItemViewer`
variant only. The nine dead variants keep their Designer `CboFolders` fields untouched; deleting
them is out of scope per the epic's non-goals. Required-change viewer count = 1.

### In scope

- Replace `CboFolders` in `ItemViewer` with a Designer-declared `WebView2` control occupying the
  same `_l0vh_Tlp` cell (ColumnSpan 2), initialized through the existing `IWebViewCoreInitializer`
  seam and, where practical, the same `CoreWebView2Environment` already created for the
  message-body pane.
- A self-contained HTML/CSS/JS breadcrumb page loaded via `NavigateToString` from an
  embedded/`Resources` string (precedent: `Resources\EmailHeader.html`).
- A new JS<->.NET bridge: JSON messages over `CoreWebView2.WebMessageReceived` /
  `PostWebMessageAsJson`, behind a narrow messenger seam (e.g.
  `IWebViewMessenger { event MessageReceived; PostJson(string); }`). No such bridge exists in the
  repo today; this is entirely new code.
- Host-neutral, fully unit-testable core types: `BreadcrumbStateModel` (collapse/expand state
  machine), a rendering projection (state -> segment/affordance/percentage DTOs), a bridge
  message router (JSON in -> typed message -> state transition and/or provider call -> JSON out),
  and a selection mapping (visible row/segment -> full folder path), mirroring the tested
  `FolderTreeStateModel` precedent.
- Re-implementation of the `IItemViewer` folder members (`SetFolderItems`,
  `SetFolderSuggestions`, `GetSelectedFolder`, `SetFolderSelectedIndex`, `SetFolderSelectedItem`,
  `SetFolderDroppedDown`, `ClearFolderItems`, `FocusFolderDropDown`, `FolderContains`,
  `GetFolderItems`, `FolderSelectionChanged`, `FolderKeyDown`) over the breadcrumb, preserving
  the controller-facing contract.
- Rerouting of the ComboBox-shaped keyboard handling (`KeyboardHandler.CboFolders_KeyDownAsync`,
  `DdOpen_KeyDownAsync`/`DdClosed_KeyDownAsync` Left/Right branches) through the bridge,
  preserving the legacy fall-through behaviors (Right -> Pop Out/Enumerate dialog when nothing
  expands; Left -> close/collapse).
- Runtime reproduction of the percentage-obscuring defect, then a CSS-based fix.
- Dark/light theme participation for the breadcrumb page (CSS custom properties driven by a
  theme bridge message or `PreferredColorScheme`/`prefers-color-scheme`), consistent with the
  existing message-body WebView2 theming (`Theme.Rendering.cs:112-116`).

### Out of scope / non-goals

- No third-party WinForms tree/list control (e.g., `BrightIdeasSoftware.TreeListView`) and no
  WPF/`ElementHost`. The control technology is WebView2 (HTML/CSS/JS).
- No change to the scoring/ranking algorithm or model output. The surfaced percentage is the
  score already computed for internal ranking (feature 324 plumbing —
  `FolderScore.Probability` -> `FolderRow.Score` -> `PercentageFormatter.FormatPercent` — is
  reused as-is).
- No changes to the nine dead viewer variants; no deletion of dead Designer types.
- No changes to the EfcViewer surface (feature 9102 owns it; no shared UI base class).
- No re-implementation of the 9101 provider itself; this feature consumes it.
- No fix for the `CoreWebView2EnvironmentOptions("–incognito ")` en-dash argument
  (`ViewerSetup.cs:49`); the breadcrumb reuses the same options object verbatim for environment
  compatibility.
- No new NuGet packages. WebView2 SDK 1.0.3912.50 is already referenced in `QuickFiler`,
  `UtilitiesCS`, and both test projects.

## Upstream Dependency: Issue 9101 (live folder-hierarchy provider)

This feature cites the 9101 provider as the source of **both** the ancestor chain and the
on-demand subfolder listing:

- Given a selected leaf folder, the provider returns the ordered ancestor chain
  `Folder -> ... -> Leaf` (root-to-leaf segments) for breadcrumb rendering.
- On demand, it returns the real immediate Outlook subfolders of a given segment via a live
  Outlook query behind an injectable seam.
- It replaces the prefix-matching logic in `FolderSuggestionTree.BuildFromRows` and
  `FolderHierarchyBuilder.Build`. Within this feature,
  `ItemViewer.FolderSearch.cs` `SetFolderSuggestions` — the sole production caller of
  `FolderHierarchyBuilder.Build` — is rewired to the provider.

**Assumption (marker: `ASSUMED-PENDING-9101-MERGE`):** the concrete 9101 contract is not present
on this branch. Research §4.4 documents the assumed interface
(`IFolderHierarchyProvider.GetAncestorChainAsync` / `GetImmediateSubfoldersAsync` returning
segment DTOs with `DisplayName`, `FolderPath`, `HasSubfolders`), with
`IOutlookFolderHierarchyReader` + `FolderTreeSnapshotNode` in
`UtilitiesCS.OutlookObjects.Folder` as the probable substrate. Names and namespace must be
reconciled against the merged 9101 contract on the epic integration branch before this feature's
atomic plan executes. If 9101 ships only a raw reader extension, this feature introduces a thin
QuickFiler-facing adapter conforming to the assumed shape so the breadcrumb model codes against
one narrow interface. Segment DTOs crossing the bridge must be JSON-serializable (plain
strings/bools; no COM handles).

This feature has no dependency on 9102 (EfcViewer breadcrumb) and executes in parallel with it.

## Functional Requirements

### FR-1: Single-line breadcrumb

Each folder suggestion renders as a single-line breadcrumb `Folder -> SubFolder -> Leaf`,
anchored at the selected/predicted leaf. Segment order is the 9101 ancestor chain, root to leaf.
Long paths truncate in middle segments (ellipsis) rather than wrapping or pushing the percentage
out of view.

### FR-2: Leaf-only expand affordance

The leaf segment carries an expand affordance — plus when collapsed, minus when expanded —
shown only when the leaf has subfolders (`HasSubfolders` from the 9101 segment DTO). Leaves
without subfolders show no affordance.

### FR-3: Non-leaf double-click collapse

Double-clicking a non-leaf segment collapses the row after that segment: all arrows, downstream
segments, and the original leaf after it are hidden, and a plus appears to the left of the
now-terminal segment. Activating that plus re-expands the full breadcrumb, restoring the
complete chain.

### FR-4: Live subfolder listing via the 9101 provider

Expanding a segment lists every real immediate Outlook subfolder of that folder, obtained from
the shared 9101 provider (live Outlook query behind an injectable seam) — not only the
subfolders that appear among the top-ranked suggestions. This replaces
`FolderHierarchyBuilder.Build` as the hierarchy source for the QuickFiler surface. The subfolder
query is routed across the JS<->.NET bridge (JS request message -> .NET router -> provider ->
JSON response -> JS render).

### FR-5: Always-visible percentage (runtime reproduction first, then CSS fix)

The prediction percentage is always fully visible and unobstructed.

- **Reproduction precondition:** a runtime reproduction of the current obscuring defect must be
  captured **before** the fix is applied. Environment: live Outlook with the add-in, QuickFiler
  open, suggestions populated; capture the dropped-down list under dark and light themes, with
  enough rows to force a scrollbar, at 100% and 150% display scaling. Evidence (screenshots plus
  an observation log recording theme, row count, scaling, and which hypothesis the capture
  confirms) is stored at
  `docs/features/active/2026-07-16-quickfiler-breadcrumb-webview2-351/evidence/repro/`.
- **Hypotheses (ordered by research §5.2):** (1) theme color contrast — `Theme.Rendering.cs:96-98`
  sets `CboFolders` colors under an explicit `TODO` that the colors do not work as expected;
  (2) dropdown vertical-scrollbar overlay of the rightmost 46 px column; (3) DPI/font clipping of
  the fixed-width column. The capture decides; the root cause is unconfirmed by design.
- **Fix:** CSS/flexbox in the breadcrumb page. Percentage cell:
  `flex: 0 0 auto; margin-left: auto;` with a `ch`-based `min-width` so it cannot be compressed
  or overlapped (`flex-shrink: 0`). Segment container:
  `flex: 1 1 auto; min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;`.
  Theme colors come from CSS custom properties. Post-fix evidence: screenshots in the same
  evidence folder showing the percentage fully visible in both themes with a maximal-length path.

### FR-6: JS<->.NET event bridge

A new JS<->.NET bridge handles all breadcrumb interaction. No
`WebMessageReceived`/`AddHostObjectToScript`/`ExecuteScriptAsync` usage exists in the repo
today; the bridge is entirely new code.

- Transport: JSON messages over `CoreWebView2.WebMessageReceived` (JS -> .NET) and
  `PostWebMessageAsJson` (.NET -> JS), behind a narrow messenger seam whose production adapter
  over `CoreWebView2` is a thin `[ExcludeFromCodeCoverage]` forwarder (matching the
  `WebView2CoreInitializer` pattern).
- Message families (minimum): segment double-click, plus/minus activation, Left/Right arrow key,
  selection change, subfolder query request/response, theme change, full re-render payload.
- Keyboard: Left/Right arrows are captured by JS `keydown` inside the WebView2 (the WinForms
  host `KeyDown` does not fire for browser-consumed keys) and surfaced as bridge messages. When
  an arrow is not handled by the breadcrumb (nothing to expand/collapse), JS reports
  "unhandled arrow" and the .NET handler invokes the legacy fall-through behavior
  (Right -> Pop Out/Enumerate dialog, `KeyboardHandler.cs:559-564`; Left -> close/collapse,
  `:579`). The `FolderKeyDown` interface event may be raised synthetically from the bridge to
  preserve the seam for existing consumers.
- The .NET side of the bridge is a pure, unit-testable message router
  (parse -> `BreadcrumbStateModel` transition and/or provider call -> response payload).

### FR-7: Selection-output contract preserved

The control must still yield the full folder path string that `GetSelectedFolder()` yields
today.

- Output contract: a full folder path string for suggestion rows, or the verbatim
  non-suggestion string for plain rows (`ItemViewer.FolderSearch.cs:107-114`).
- Downstream consumers preserved bit-for-bit: `QfcItemController.cs:237-240`
  (`_selectedFolder`/`SelectedFolder` caching), `QfcItemController.MailActions.cs:90`
  (the literal `"Trash to Delete"` comparison gating attachment saving) and `:103`
  (`DestinationOlStem = SelectedFolder`, the actual move target), `QfcCollectionController.cs`
  (`:170-171` dialog-skip check, `:2308` CSV export), and
  `QfcItemController.EventHandlers.cs:209-212` (selection-change refresh). String identity of
  non-suggestion rows, including `"Trash to Delete"`, is contract.
- **Both population paths survive:**
  - **Path A — suggestion rows:** `AssignFolderComboBox`
    (`QfcItemController.FolderHandling.cs:161-200`) populating `FolderRow` suggestions via
    `SetFolderSuggestions` (the sole production caller of `FolderHierarchyBuilder.Build`,
    rewired to the 9101 provider) plus `SetFolderItems`, preselect, and
    `GetSelectedFolder()` readback.
  - **Path B — search results:** `TextBoxSearch_TextChanged`
    (`QfcItemController.EventHandlers.cs:164-178`) populating plain `string[]` rows via
    `ClearFolderItems()` / `SetFolderItems(folders)` / `SetFolderSelectedIndex(1)` /
    `SetFolderDroppedDown(true)`. These rows carry no probability; the breadcrumb renders them
    (as ancestor-split chains with an empty percentage cell) and returns them from
    `GetSelectedFolder()` verbatim.
- Intent members with no literal breadcrumb equivalent (`SetFolderDroppedDown`,
  `FocusFolderDropDown`) remain on `IItemViewer` and map to breadcrumb focus / expansion-popup
  visibility (callers: `QfcItemController.Navigation.cs:33-46`, `EventHandlers.cs:177,184-185`).

## Architecture and Testability Requirements

- **Host-neutral core (NOT coverage-exempt):** `BreadcrumbStateModel`, the rendering
  projection, the bridge message router, and the selection mapping perform no WinForms, COM,
  WebView2, or I/O work, mirroring the placement and style of the tested
  `FolderTreeStateModel` (`UtilitiesCS\OutlookObjects\Folder\FolderTreeStateModel.cs`).
- **Seams (thin, exempt glue):** WebView2 init through the existing `IWebViewCoreInitializer`;
  post-init messaging through the new narrow messenger seam; Outlook I/O entirely behind the
  9101 provider (breadcrumb code never touches interop); WinForms glue confined to
  `[ExcludeFromCodeCoverage]` `ItemViewer` partials, matching the current
  `ItemViewer.FolderSearch.cs` split.
- **Tests:** MSTest + Moq + FluentAssertions. Provider and messenger mocked with Moq; router
  tested string-in/string-out; async provider calls tested with completed tasks (no timers, no
  temp files). New test files require explicit `<Compile Include>` entries in the non-SDK test
  `.csproj` files.
- **Coverage:** new host-neutral code targets >= 90% line coverage (the stricter of the
  documented bars, per prior epic practice); changed code must not reduce coverage on changed
  lines; full toolchain (csharpier -> analyzers -> nullable -> vstest) green.
- **Platform:** all touched projects are non-SDK net4.8.1 — no `record`/`record struct`/`init`
  (no `IsExternalInit`); use plain classes/readonly structs with explicit constructors.

## Constraints & Risks

- No third-party WinForms tree/list control; no WPF/`ElementHost`. WebView2 (HTML/CSS/JS) only.
- No change to scoring/ranking or model output.
- 9101 contract is assumed pending merge (`ASSUMED-PENDING-9101-MERGE`); reconcile before plan
  execution.
- Second WebView2 per pooled viewer: memory/init-latency impact is unknown; environment reuse
  mitigates process count. Observe at runtime during implementation; fallback is lazy
  initialization of the breadcrumb WebView2 when a viewer becomes active.
- Focus semantics for `FocusFolderDropDown`/`SetFolderDroppedDown(true)` require runtime
  verification of WebView2 focus hand-off (`WebView2.Focus()` + JS `focus()`) in the live add-in.
- KeyboardHandler decommissioning scope: prefer rerouting the existing `CboFolders_KeyDownAsync`
  / `Dd*_KeyDownAsync` families over removal (smaller diff); the atomic plan decides.

## Acceptance Criteria

- [ ] AC-1: In the single live `ItemViewer` variant (scope decision recorded above), the
  `CboFolders` `ComboBox` is replaced by a WebView2-hosted HTML/CSS/JS breadcrumb control
  following the QuickFiler WebView2 message-body pane pattern (`IWebViewCoreInitializer` init,
  `NavigateToString` content delivery); the nine dead viewer variants are unchanged.
- [ ] AC-2: Each suggestion renders as a single-line breadcrumb `Folder -> SubFolder -> Leaf`
  anchored at the selected/predicted leaf, with segment order supplied by the 9101 provider's
  ancestor chain.
- [ ] AC-3: The leaf carries a plus (collapsed) / minus (expanded) affordance only when the leaf
  has subfolders; leaves without subfolders show no affordance.
- [ ] AC-4: Double-clicking a non-leaf segment collapses the row after that segment and shows a
  plus to the left of the now-terminal segment; activating the plus re-expands the full
  breadcrumb.
- [ ] AC-5: Expanding a segment lists every real immediate Outlook subfolder of that folder via
  the shared 9101 provider (live query behind an injectable seam), not only subfolders present
  among the top-ranked suggestions; `FolderHierarchyBuilder.Build` is no longer the hierarchy
  source for this surface.
- [ ] AC-6: A runtime reproduction of the percentage-obscuring defect is captured and stored
  under `evidence/repro/` (theme, row count, scaling, confirmed hypothesis recorded) **before**
  the fix is applied; the CSS-based fix (percentage cell `margin-left: auto; flex-shrink: 0`
  with `min-width`; truncating segment container) is then applied, with post-fix evidence
  showing the percentage fully visible in both themes with a maximal-length path.
- [ ] AC-7: A new JS<->.NET bridge (JSON over `WebMessageReceived`/`PostWebMessageAsJson`
  behind a narrow messenger seam) handles double-click, plus/minus, and Left/Right arrow
  interaction, routes the live subfolder query, and preserves the legacy Right/Left
  fall-through behaviors when the breadcrumb does not consume the arrow.
- [ ] AC-8: The selection-output contract is preserved: `GetSelectedFolder()` yields the full
  folder path string (or verbatim non-suggestion string, including `"Trash to Delete"`) exactly
  as consumed at `QfcItemController.MailActions.cs:90,103` and `QfcCollectionController.cs`;
  both Path A (`AssignFolderComboBox` `FolderRow` suggestions) and Path B
  (`TextBoxSearch_TextChanged` plain `string[]` search results) populate and select correctly.
- [ ] AC-9: No third-party WinForms tree/list control and no WPF/`ElementHost` are introduced;
  no new NuGet packages are added.
- [ ] AC-10: The scoring/ranking algorithm and model output are unchanged; the surfaced
  percentage is the score already computed (feature 324 plumbing reused as-is).
- [ ] AC-11: Breadcrumb core logic (`BreadcrumbStateModel`, rendering projection, bridge message
  router, selection mapping) is host-neutral and unit-tested with MSTest + Moq +
  FluentAssertions without a live Outlook process or a live WebView2; live Outlook I/O is
  reachable only through the injectable 9101 provider seam.
- [ ] AC-12: The full C# toolchain (csharpier, .NET analyzers, nullable, MSTest via vstest) is
  green; new host-neutral code meets >= 90% line coverage; changed lines do not lose coverage;
  new test files have explicit `<Compile Include>` entries.
- [ ] AC-13: The 9101 dependency is reconciled: the assumed contract
  (`ASSUMED-PENDING-9101-MERGE`) is replaced with the actual merged provider surface (directly
  or via a thin QuickFiler-facing adapter) before the breadcrumb consumes it.

## Definition of Done

- [ ] All acceptance criteria above checked off with evidence.
- [ ] Tests added for positive, negative, edge, and error scenarios of the state model, router,
  projection, and selection mapping.
- [ ] Runtime evidence (reproduction and post-fix) committed under the feature `evidence/` tree.
- [ ] Docs updated (this spec, user-story, issue AC alignment).
- [ ] Toolchain pass completed (format -> analyzers -> nullable -> test) with commands reported.

## Seeded Test Conditions (from potential)

- [ ] Unit coverage: pure breadcrumb-model logic (ancestor-chain rendering, collapse/expand
  state transitions) without a live Outlook process.
- [ ] Integration scenarios: JS<->.NET bridge message routing for double-click, keyboard, and
  live subfolder query.
- [ ] Runtime reproduction of the percentage-obscuring defect, then verification the CSS fix
  resolves it.
