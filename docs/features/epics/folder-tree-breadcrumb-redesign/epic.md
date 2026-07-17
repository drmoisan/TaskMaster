---
epic: folder-tree-breadcrumb-redesign
integration_branch: epic/folder-tree-breadcrumb-redesign-integration
created_at: 2026-07-17T01-47
intent:
  epic_type: business
  business_outcome_hypothesis: Rendering each folder suggestion as a single-line breadcrumb (Folder -> SubFolder -> Leaf) with per-segment double-click collapse, a leaf-anchored expand affordance that lists the real Outlook subfolders of a folder, and an always-visible prediction percentage — delivered in both the EfcViewer matching-folders list and the QuickFiler folder dropdown via a WebView2-hosted HTML/CSS/JS control — lets a user judge and navigate suggested filing targets faster and with more confidence than the current indented tree over top-ranked suggestions, while choosing WebView2 as the control technology preserves the UI investment across the planned VSTO migration.
  leading_indicators:
    - Each suggestion renders as a single-line breadcrumb anchored at the selected leaf in both the EfcViewer matching-folders list and the QuickFiler folder dropdown.
    - Expanding a folder segment lists every real immediate Outlook subfolder of that folder, not only the subfolders that appear among the top-ranked suggestions.
    - Double-clicking a non-leaf segment collapses the row after that segment and shows a plus affordance that re-expands to the full breadcrumb.
    - The prediction percentage is fully visible and unobstructed in both controls.
  nfrs:
    - The live Outlook subfolder and ancestor-chain query is isolated behind an injectable seam so the breadcrumb core logic is unit-testable without a live Outlook process, per the repository I/O-boundary policy.
    - No change to the scoring or ranking algorithm or to model output; the percentage surfaced is the score already computed for internal ranking (feature 324 plumbing is reused as-is).
    - No third-party WinForms tree/list control (for example BrightIdeasSoftware.TreeListView) and no WPF/ElementHost are introduced; the breadcrumb control technology is WebView2 (HTML/CSS/JS) in both surfaces.
    - Full C# toolchain (csharpier, .NET analyzers, nullable, MSTest) green for every child feature; changed and new code meets repository coverage thresholds.
features:
  - issue_num: 9101
    feature_folder: 2026-07-16-folder-hierarchy-live-provider
    depends_on: []
  - issue_num: 9102
    feature_folder: 2026-07-16-efcviewer-breadcrumb-webview2
    depends_on: [9101]
  - issue_num: 9103
    feature_folder: 2026-07-16-quickfiler-breadcrumb-webview2
    depends_on: [9101]
---

# Epic: Folder-Tree Breadcrumb Redesign (WebView2)

- Integration branch: `epic/folder-tree-breadcrumb-redesign-integration`
- Status: Manifest authored by `epic-planner`. `issue_num` values `9101`/`9102`/`9103` are
  provisional placeholders and are back-filled with the real GitHub issue numbers as each child
  feature is promoted during preparation. Child preparation (research, spec/user-story, atomic
  plan, preflight clearance) fans in to this integration branch per feature as it completes.
- Source objective:
  `docs/features/potential/2026-07-16-folder-tree-breadcrumb-redesign-epic-request.md`.

This epic is a design-correction follow-up to the merged `folder-tree-percentage-ui` epic
(issues 324, 325, 326, 327). The delivered tree/percentage behavior for the EfcViewer and
QuickFiler surfaces does not match the intended design; this epic replaces it with a
breadcrumb layout backed by a live Outlook folder-hierarchy query and a WebView2-hosted control.

## Goal

Replace the current multi-row indented tree (EfcViewer `TreeListView`) and the stock
`ComboBox` folder dropdown (QuickFiler) with a single-line breadcrumb control in both surfaces:

1. Each suggestion renders as `Folder -> SubFolder -> SubFolder -> Leaf`, anchored at the
   actually-selected/predicted leaf folder.
2. The leaf carries an expand affordance (plus when collapsed, minus when expanded) only when
   the leaf has subfolders.
3. Double-clicking any non-leaf segment collapses the row so everything after that segment
   (arrows, downstream segments, the original leaf) is hidden, with a plus to the left of the
   now-terminal segment that re-expands the full breadcrumb.
4. Expanding a segment lists every real immediate Outlook subfolder of that folder — via a live
   query against the real Outlook hierarchy — not only the subfolders that happen to appear
   among the top-ranked suggestions.
5. The percentage is always fully visible and unobstructed.

Both surfaces use a WebView2-hosted HTML/CSS/JS breadcrumb control (flexbox row layout,
per-segment click handlers, CSS-based percentage-visibility fix), following the pattern
QuickFiler already uses for its WebView2 message-body pane.

## Scope

- Introduce a live Outlook folder-hierarchy provider (the epic's single shared contract) that,
  given a selected leaf folder, returns its `Folder -> ... -> Leaf` ancestor chain and, on
  demand, a segment's real immediate subfolders. This replaces the prefix-matching-over-
  suggestion-rows logic in `FolderSuggestionTree.BuildFromRows` and `FolderHierarchyBuilder.Build`.
- Replace the EfcViewer matching-folders control (`BrightIdeasSoftware.TreeListView` in
  `EfcViewer` / `EfcViewer3`) with a WebView2-hosted breadcrumb control, including the JS<->.NET
  event bridge for double-click and keyboard (left/right arrow) interaction and the routing of
  the live subfolder query across that bridge, plus the CSS percentage-visibility fix.
- Replace the QuickFiler folder dropdown control (`CboFolders` stock `ComboBox` in the live
  `ItemViewer` variant) with the same WebView2-hosted breadcrumb control and bridge, re-verifying
  which viewer variant(s) are actually live before fixing scope.
- Reproduce and fix the percentage-obscuring defect at runtime; static review of the current
  column/rect math found no layout-level overlap, so each UI child first captures a runtime
  reproduction before applying the CSS-based fix.

## Non-Goals

- No change to the scoring/ranking algorithm itself or to model output; the percentage surfaced
  is the score already computed for internal ranking (the feature 324 probability plumbing is
  reused as-is and is not re-plumbed here).
- No unification of the two EfcViewer implementations or of the QuickFiler viewer variants into
  a shared base control beyond what the WebView2 control replacement in requirement 5 requires.
- No third-party WinForms tree/list control and no WPF/`ElementHost`; both are ruled out because
  they are VSTO/WinForms-hosting-specific investments that would not carry forward to a
  post-VSTO UI stack.

## Shared Design

The single shared contract across this epic is the **live Outlook folder-hierarchy provider**
(feature 9101). Given a selected leaf folder it returns:

- the ordered ancestor chain `Folder -> ... -> Leaf` (root-to-leaf segments) for breadcrumb
  rendering, and
- on demand, the real immediate subfolders of a given segment, queried live against the Outlook
  hierarchy (`MAPIFolder.Folders` or the equivalent existing interop/adapter seam).

The provider isolates the Outlook I/O behind an injectable seam so the pure ancestor-chain and
segment-children logic is unit-testable without a live Outlook process, per repository policy.
It replaces the prefix-matching-over-suggestion-rows approach that both `FolderSuggestionTree`
and `FolderHierarchyBuilder` use today. Features 9102 and 9103 both consume this contract to
populate their WebView2 breadcrumb controls; they share the provider but share no UI base class
with each other (EfcViewer hosts a `TreeListView` today, QuickFiler hosts a `ComboBox`), so each
builds its own WebView2 host and JS<->.NET bridge.

## Decomposition Rationale

Current state verified in the source objective against current code:

- EfcViewer (`EfcViewer.cs` / `EfcViewer3.cs`, two parallel non-shared implementations) renders
  matching folders in a `BrightIdeasSoftware.TreeListView` as a conventional indented multi-row
  tree. Hierarchy comes from `FolderSuggestionTree.BuildFromRows`, which derives parent/child
  edges by prefix-matching among the already-presented top-5-plus-recents suggestion rows; it
  never queries Outlook's real subfolder structure.
- QuickFiler's folder dropdown (`CboFolders`) is still a stock `System.Windows.Forms.ComboBox`
  (`DrawMode=OwnerDrawFixed`) in the single live `ItemViewer` variant. Hierarchy is synthesized
  by `FolderHierarchyBuilder.Build` splitting the same <=5 suggestion paths on `\`; it does not
  query real Outlook subfolders. Nine other declared viewer variants were left unchanged by the
  prior epic and their liveness must be re-verified.
- Probability/percentage plumbing (feature 324) is sound and reused as-is: `FolderScore.Probability`
  flows through `FolderPredictor.FolderRowArray` as `FolderRow.Score` and is rendered by
  `PercentageFormatter.FormatPercent(double?)`.

This decomposes into three independently mergeable child features:

- **9101 — Live Outlook folder-hierarchy provider (wave 0, C3).** Introduces the shared
  ancestor-chain + live-subfolder provider contract and replaces the prefix-matching logic in
  `FolderSuggestionTree.BuildFromRows` and `FolderHierarchyBuilder.Build`. Complexity floor is
  forced to C3 by the `cross_module_contract_change` signal (a new public contract consumed
  across module boundaries by both UI consumers). The folder module is scoring-adjacent (T1/T2)
  and the change adds a live-I/O seam that must be isolated from pure logic. No `depends_on`.
- **9102 — EfcViewer WebView2 breadcrumb control (wave 1, C4).** Replaces the `TreeListView`
  with a WebView2-hosted HTML/CSS/JS breadcrumb control across both `EfcViewer` implementations:
  single-line breadcrumb, leaf-anchored expand affordance, per-segment double-click collapse, a
  JS<->.NET event bridge for double-click and keyboard interaction, routing of the live subfolder
  query (from 9101) across that bridge, and a CSS-based percentage-visibility fix preceded by a
  runtime reproduction. Banded C4 by judgment: this is a novel control technology for this
  surface with a new bidirectional JS<->.NET interaction model and live-query routing, exceeding
  the localized-change bands. Consumes 9101's provider contract. `depends_on: [9101]`.
- **9103 — QuickFiler WebView2 breadcrumb control (wave 1, C4).** Replaces the `CboFolders`
  `ComboBox` with the same WebView2-hosted breadcrumb control and bridge in the live viewer
  variant, after re-verifying viewer-variant liveness; same interaction model, live-query
  routing, and CSS percentage-visibility fix. Banded C4 by judgment for the same novel WebView2
  bridge surface, in a different host control with no shared base class, plus the viewer-variant
  liveness re-verification. Consumes 9101's provider contract. `depends_on: [9101]`.

## Waves

Wave assignment by longest-path layering over the dependency DAG
(`wave(f) = 0` when `depends_on` is empty, else `1 + max(wave(d))`):

- **Wave 0:** 9101 (live folder-hierarchy provider).
- **Wave 1:** 9102 (EfcViewer WebView2 breadcrumb), 9103 (QuickFiler WebView2 breadcrumb).

The DAG is cycle-free (verified manually; the reference wave-computation script is not vendored
in this repository). 9102 and 9103 have no interdependency and execute in parallel within
wave 1 once 9101 merges.
