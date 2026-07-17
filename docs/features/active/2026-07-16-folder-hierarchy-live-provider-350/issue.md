# folder-hierarchy-live-provider (Issue #350)

- Date captured: 2026-07-16
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/folder-hierarchy-live-provider/ (Issue #350)
- Epic: folder-tree-breadcrumb-redesign (manifest issue placeholder 9101, wave 0, complexity band C3)
- Integration branch: epic/folder-tree-breadcrumb-redesign-integration

- Issue: #350
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/350
- Last Updated: 2026-07-17
- Work Mode: full-feature

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
not appear among the top-ranked suggestions are never shown. This is the epic's single shared upstream
contract; two UI consumers (issues 9102 EfcViewer and 9103 QuickFiler) depend on it.

## Proposed Behavior

Introduce a live Outlook folder-hierarchy provider with a clear public contract. Given a selected leaf
folder it must return:

1. The ordered ancestor chain `Folder -> ... -> Leaf` (root-to-leaf segments) for breadcrumb rendering.
2. On demand, the real immediate subfolders of a given segment, queried live against the real Outlook
   hierarchy (`MAPIFolder.Folders` or the equivalent existing Outlook interop/adapter seam).

This replaces the prefix-matching-over-suggestion-rows logic in `FolderSuggestionTree.BuildFromRows`
and `FolderHierarchyBuilder.Build`.

Per repository I/O-boundary policy, isolate the live Outlook query behind an injectable seam so the
pure ancestor-chain and segment-children logic is unit-testable without a live Outlook process
(MSTest/Moq/FluentAssertions). Define a clear public contract with documented inputs, outputs, and
invariants so both downstream UI consumers can depend on it.

## Acceptance Criteria (early draft)

- [ ] A public folder-hierarchy provider contract returns the ordered root-to-leaf ancestor chain for a
      selected leaf folder.
- [ ] The provider returns, on demand, the real immediate subfolders of a given segment queried live
      against the real Outlook hierarchy (not from suggestion rows).
- [ ] The live Outlook query is isolated behind an injectable seam; the pure ancestor-chain and
      segment-children logic is unit-testable without a live Outlook process.
- [ ] The new provider supersedes the prefix-matching-over-suggestion-rows approach used by
      `FolderSuggestionTree.BuildFromRows` and `FolderHierarchyBuilder.Build`: it delivers the live
      ancestor-chain and real-immediate-subfolder queries those methods lack. Removal of the old
      methods and rewiring of their UI callers (`EfcFormController.BindFolderRows` for 9102,
      `ItemViewer.SetFolderSuggestions` for 9103) is executed by the consuming UI features when they
      adopt the provider, so wave-0 feature 9101 stays independently mergeable and does not break the
      build by deleting methods still referenced by unmigrated UI code.
- [ ] The scoring/ranking algorithm and the feature 324 probability plumbing
      (`FolderScore.Probability` -> `FolderRow.Score` -> `PercentageFormatter.FormatPercent`) are
      unchanged.
- [ ] Full C# toolchain green (csharpier, .NET analyzers, nullable, MSTest); changed and new code meets
      repository coverage thresholds.

## Constraints & Risks

- Constraint: do not change the scoring/ranking algorithm or the feature 324 probability plumbing.
- Constraint: repository I/O-boundary policy requires the live Outlook interop query to be isolated
  behind an injectable seam.
- Constraint: this is a shared public contract consumed across module boundaries by two UI features
  (9102, 9103); it must have documented inputs, outputs, and invariants and remain stable.
- Risk: existing seams (`IOutlookFolderHierarchyReader`, `OutlookFolderHierarchyReader`,
  `IOutlookFolderTreeService`, `OutlookFolderTreeService`) may already provide part of the live-query
  surface; research must reconcile against them before introducing a new seam.
- Risk (COM/Outlook interop): live `MAPIFolder.Folders` enumeration must not be exercised in unit tests.

## Test Conditions to Consider

- [ ] Unit coverage: ancestor-chain computation for single-level, multi-level, and root-only folders.
- [ ] Unit coverage: segment-children retrieval via a mocked live-hierarchy seam.
- [ ] Negative flows: missing/null selected folder, unresolved handle, empty subfolder set.
- [ ] Edge cases: leaf equals root; duplicate segment names at different depths.
- [ ] Isolation: no test touches a live Outlook process or COM interop.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/2026-07-16-folder-hierarchy-live-provider/` folder from the template
