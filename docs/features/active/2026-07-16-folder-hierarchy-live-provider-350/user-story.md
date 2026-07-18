# `2026-07-16-folder-hierarchy-live-provider` — User Story

- Issue: #350
- Epic: `folder-tree-breadcrumb-redesign` (manifest issue 9101, wave 0, complexity band C3)
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-16T21-52
- Work Mode: full-feature

## Story Statement

- As the **9102 EfcViewer breadcrumb feature**, I want a provider that returns the ordered
  root-to-leaf ancestor chain for a selected leaf folder, so that I can render a single-line
  `Folder -> ... -> Leaf` breadcrumb without deriving hierarchy from the top-ranked suggestion rows.
- As the **9103 QuickFiler breadcrumb feature**, I want a provider that returns, on demand, the real
  immediate Outlook subfolders of a given segment, so that expanding a segment lists every real
  subfolder of that folder, not only the subfolders that happen to appear among the suggestions.
- As either **downstream UI feature**, I want to resolve a UI-selected folder path to a stable node
  key, so that I can anchor the breadcrumb at the correct leaf and route subsequent expand calls by
  identity rather than by ambiguous display name.
- As the **end user judging and navigating suggested filing targets**, I want the breadcrumb to show
  the true folder hierarchy and the real subfolders of any segment, so that I can navigate to a
  filing target that is not among the top-ranked suggestions with confidence.

## Problem / Why

The `folder-tree-breadcrumb-redesign` epic replaces the EfcViewer indented tree and the QuickFiler
folder dropdown with a single-line breadcrumb control in both surfaces. Both surfaces need to render
a `Folder -> ... -> Leaf` ancestor chain for a selected folder and, on demand, list the real
immediate Outlook subfolders of a given segment.

Today the hierarchy is synthesized only from the already-presented top-ranked suggestion rows:

- `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs` (`BuildFromRows`) derives parent/child
  edges by prefix-matching among the top-5-plus-recents suggestion rows.
- `UtilitiesCS/OutlookObjects/Folder/FolderHierarchyBuilder.cs` (`Build`) splits the same <=5
  suggestion paths on `\`.

Neither queries Outlook's real subfolder structure, so a segment's real immediate subfolders that do
not appear among the top-ranked suggestions are never shown. This feature introduces the epic's single
shared upstream contract; the two UI consumers (9102 EfcViewer and 9103 QuickFiler) depend on it, and
through them the end user judging and navigating suggested filing targets.

## Personas & Scenarios

### Persona: 9102 EfcViewer breadcrumb feature (primary consumer)

- Who: the wave-1 feature that replaces the EfcViewer `BrightIdeasSoftware.TreeListView` with a
  WebView2-hosted breadcrumb control.
- What it cares about: an ordered, root-to-leaf ancestor chain it can render as breadcrumb segments,
  and a stable per-segment identity it can pass across the JS<->.NET bridge when a segment expands.
- Constraints: it must not depend on a live Outlook process in its unit tests, and it must join the
  prediction percentage from the existing feature-324 plumbing rather than from the provider.
- Goals and frustrations: it wants the provider to return breadcrumb-shaped data directly. Today it
  would have to re-derive hierarchy from suggestion rows, which cannot express the real folder tree.

- Scenario: A user opens the EfcViewer for a message. For a predicted suggestion, the feature calls
  `ResolveLeafKeyAsync(selectedFolderPath)` to get the leaf key, then `GetAncestorChainAsync(leafKey)`
  to get the ordered segments, and renders `Seg -> Seg -> ... -> Leaf`. The leaf shows the expand
  affordance only when its segment reports `HasChildren`. When the user double-clicks a non-leaf
  segment to expand it, the feature routes `GetImmediateSubfoldersAsync(segment.Key)` across the
  bridge and renders the real immediate subfolders. The percentage is joined by `FolderPath` from the
  existing probability source.

### Persona: 9103 QuickFiler breadcrumb feature (primary consumer)

- Who: the wave-1 feature that replaces the QuickFiler `CboFolders` stock `ComboBox` with the same
  WebView2-hosted breadcrumb control in the live `ItemViewer` variant.
- What it cares about: the same ancestor-chain and immediate-subfolder queries as 9102, from the same
  shared contract, so both surfaces behave consistently without a shared UI base class.
- Constraints: same testability and probability constraints as 9102; it also re-verifies which viewer
  variant is live before wiring the provider.
- Goals and frustrations: it wants one contract shared with 9102 so the two surfaces do not diverge.
  Today its `FolderHierarchyBuilder.Build` path can only split the <=5 suggestion paths, so real
  subfolders outside the suggestions are invisible in the dropdown.

- Scenario: In QuickFiler, a user reviews a suggested filing target. The feature resolves the leaf
  key, requests the ancestor chain, and renders the breadcrumb. When the user expands a segment, the
  feature requests the real immediate subfolders for that segment key and renders them, letting the
  user file into a real subfolder that was never among the top-ranked suggestions.

### Persona: end user judging and navigating suggested filing targets (indirect consumer)

- Who: an Outlook user filing a message using EfcViewer or QuickFiler.
- What they care about: seeing the true folder hierarchy for a suggestion and being able to reach any
  real subfolder of a segment, not only the subfolders that made the top-ranked list.
- Constraints: they interact only through the two UI surfaces; they never call the provider directly.
- Goals and frustrations: with the current prefix-matching hierarchy, a correct filing target that is
  not among the top suggestions cannot be reached from the breadcrumb.

- Scenario: The user sees a breadcrumb `Inbox -> Clients -> Acme`, expands `Clients`, and sees all
  real immediate subfolders of `Clients` (including ones not suggested), then navigates to the correct
  one. The prediction percentage remains fully visible throughout.

## Consumer contract expectations

- The provider is obtained from production globals (backed by `IOlObjects.FolderTreeService`); no new
  plumbing is required in either consumer to reach it.
- `GetAncestorChainAsync` returns segments ordered root-first / leaf-last, with the last element equal
  to the requested leaf; `HasChildren` on each segment tells the UI whether to render the expand
  affordance.
- `GetImmediateSubfoldersAsync` returns the real immediate subfolders of a segment key, or an empty
  list (never null) when the segment has none or the key is unknown.
- `ResolveLeafKeyAsync` returns a stable `FolderTreeNodeKey` for a folder path, or `null` when no
  matching node exists; identity is by key so duplicate segment names at different depths are
  distinguished.
- The segment DTO is probability-free by design; each consumer joins the percentage from the existing
  feature-324 source by `FolderPath`.

## Acceptance Criteria

- [x] A consuming UI feature can obtain the ordered root-to-leaf ancestor chain for a selected leaf
      folder from `IFolderHierarchyProvider.GetAncestorChainAsync`, with the last segment equal to the
      requested leaf and `HasChildren` set correctly for rendering the expand affordance.
- [x] A consuming UI feature can obtain, on demand, the real immediate Outlook subfolders of a given
      segment from `IFolderHierarchyProvider.GetImmediateSubfoldersAsync` (queried live via the cached
      snapshot, not from suggestion rows), receiving an empty list rather than null when there are none.
- [x] A consuming UI feature can resolve a UI-selected folder path to a stable `FolderTreeNodeKey` via
      `ResolveLeafKeyAsync`, receiving `null` when no matching node exists, and can route subsequent
      expand calls by that key so duplicate segment names at different depths are distinguished.
- [x] The segment DTO (`FolderBreadcrumbSegment`) is probability-free; each consumer joins the
      percentage from the existing feature-324 plumbing by `FolderPath`, and the scoring/ranking
      algorithm and probability plumbing are unchanged.
- [x] The provider and its pure ancestor-chain helper are unit-testable without a live Outlook process
      (the live query is isolated behind the reused `IOutlookFolderTreeService` seam), so a consumer's
      tests can depend on the contract without COM.
- [x] Wave-0 mergeability is preserved: this feature ADDS the provider and does not delete
      `FolderSuggestionTree.BuildFromRows` or `FolderHierarchyBuilder.Build` or rewire their UI callers
      (`EfcFormController.BindFolderRows`, `ItemViewer.SetFolderSuggestions`); that removal and rewiring
      is performed by 9102 and 9103 when they adopt the provider.
- [x] Full C# toolchain green (csharpier, .NET analyzers, nullable, MSTest); changed and new code meets
      repository coverage thresholds.

## Non-Goals

- No WebView2 control, HTML/CSS/JS, or JS<->.NET bridge work; those are owned by 9102 and 9103.
- No change to the scoring/ranking algorithm or the feature-324 probability plumbing.
- No deletion of the legacy prefix-matching methods and no rewiring of their UI callers within this
  feature.
- No new live-COM seam; the existing snapshot/reader infrastructure is reused.
