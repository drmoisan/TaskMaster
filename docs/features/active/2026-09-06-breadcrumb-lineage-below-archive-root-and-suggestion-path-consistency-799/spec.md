# 2026-09-06-breadcrumb-lineage-below-archive-root-and-suggestion-path-consistency (Spec)

- **Issue:** #799
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-06T22-01
- **Status:** Draft
- **Version:** 0.1

## Context
In QuickFiler (ordinary and High Confidence modes) suggestion rows render their breadcrumb lineage from the store root, for example `dmoisan@realgoodfoods.com -> Archive -> _Active Projects -> Build RGF Org and Team -> Sales Lead`, while typed search-result rows render `_Active Projects -> Build RGF Org and Team -> Sales Lead`. The mailbox and Archive segments are superfluous: every filing target is under the archive root, and the lineage must begin at the first segment below it for every row kind. The full-root lineage is the behavior delivered by issue #439 ("full root-to-leaf ancestor lineage"), so this is a specification change, not a regression. The same change should unify several archive-root stripping paths that have drifted into duplicates and gaps.

Environment:
- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8 VSTO Outlook add-in with WebView2 breadcrumb, debug build from `TaskMaster\bin\Debug`, HEAD `c431dc32` (2026-09-06)
- Command/flags used: Outlook ribbon -> QuickFiler and QuickFiler High Confidence; folder drop-down in the item view (`ItemViewer` breadcrumb, `FolderBreadcrumb.html`)
- Data source or fixture: live mailbox; archive root resolves to `\\dmoisan@realgoodfoods.com\Archive` (`AppOlObjects.ArchiveRootPath`)

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Display-only: filing lands correctly. The redundant segments consume most of the row width and make same-named folders harder to distinguish, which is the problem #439 set out to solve.


## Repro & Evidence
Steps to Reproduce:
1. Launch QuickFiler on Inbox. Open the folder drop-down on an item without typing.
2. Observe the first suggestion row: `dmoisan@realgoodfoods.com -> Archive -> _Active Projects -> Build RGF Org and Team -> Sales Lead`.
3. Type a few letters into the search box. Observe the same folder as a search result: `_Active Projects -> Build RGF Org and Team -> Sales Lead`. Typing `90` shows `_Active Projects -> 90 Day Plan`.
4. Accept a suggestion: filing lands in the correct Outlook and file-system folders (the filing target is the archive-relative stem, not the displayed lineage).

Note: the maintainer's transcription of the search row showed `_ Active Projects` with a space after the underscore. The orchestrator should verify whether the renderer alters a leading underscore or whether this was a transcription artifact.

Expected:
- Every row in the folder list, suggestion or search result, renders as `_Active Projects -> Build RGF Org and Team -> Sales Lead`: the lineage begins at the first segment below the archive root, each segment is clickable for ancestor navigation, and no row shows the mailbox or Archive segments.
- A resolved ancestor chain that does not pass through the archive root node is logged as an error and rendered with the existing single-segment fallback, never with a mailbox prefix. There are no legitimate filing targets outside the archive root.
- One archive-root projection rule is used everywhere a suggestion path is prepared for display or compared against displayed entries.

Actual:
- Suggestion rows show the mailbox and Archive segments; search rows do not. Ordinary and High Confidence modes behave the same.
- Recent-folder entries are appended to the suggestion list with no projection at all.
- The Efc breadcrumb binding joins projected row text against raw scorer paths, so an archive-rooted suggestion loses its percentage.
- Several suggestion rows fail hierarchy resolution and fall back to a single segment, with errors such as `No snapshot node path ends with '\Forums\Pricing'; leaving 'Forums\Pricing' unresolved.`

Logs / Screenshots:
- [x] Attached minimal logs or screenshot
- Snippet (`TaskMaster\bin\Debug\logs\debug_2026-09-06.log`), stale-label resolution failures:

```
2026-09-06 16:45:51,908 [VSTA_Main] ERROR UtilitiesCS.OutlookObjects.Folder.OutlookFolderHierarchyProvider - No snapshot node path ends with '\Scorecards\Monthly Scans'; leaving 'Scorecards\Monthly Scans' unresolved.
2026-09-06 16:45:51,937 [VSTA_Main] ERROR UtilitiesCS.OutlookObjects.Folder.OutlookFolderHierarchyProvider - No snapshot node path ends with '\Forums\Pricing'; leaving 'Forums\Pricing' unresolved.
```


## Scope & Non-Goals
- In scope:
- Out of scope / non-goals:
- Explicitly excluded systems, integrations, or datasets:

## Root Cause Analysis
Why the two row kinds differ (verified by code read):

- Suggestion rows: `FolderBreadcrumbBridgeRouter.SetSuggestionsAsync` decorates each row with the ancestor chain from `IFolderHierarchyProvider`. `FolderTreeSnapshotQueries.GetAncestorChain` (`UtilitiesCS\OutlookObjects\Folder\FolderTreeSnapshotQueries.cs:109-140`) walks `ParentKey` until it is null, i.e. up to the store node. `BreadcrumbRowBuilder.MapSegments` (`UtilitiesCS\OutlookObjects\Folder\BreadcrumbRowBuilder.cs:178-208`) maps every node to a segment with no trimming. This is the #439 design (`docs\features\active\2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439\issue.md`, Expected Behavior).
- Search rows: `FolderBreadcrumbBridgeRouter.ReplaceItemsPreservingSession` (`FolderBreadcrumbBridgeRouter.SearchPresentation.cs:38-55`) carries the verbatim archive-relative stem produced by `FolderPredictor.GetOlSubpath` (`FolderPredictor.cs:953-971`), and `BreadcrumbRenderProjection.SplitVerbatim` (`BreadcrumbRenderProjection.cs:242-246`) splits it on path separators for rendering. No provider chain, so no store segments.

Archive-root projection paths that should be unified under `ArchiveStemContract` (`UtilitiesCS\OutlookObjects\Folder\ArchiveStemContract.cs`, the canonical ordinal, anchored, separator-terminated stripper from #614):

- `FolderPredictor.ProjectSuggestionPath` (`FolderPredictor.cs:848-861`): private, backslash-only, appends `"\\"` to the root unconditionally, silently returns the full path on no match, and with an empty root strips one leading separator from any path.
- `QfcItemController.ProjectPredeterminedFolder` (`QuickFiler\Controllers\QfcItemController.FolderHandling.cs:272-285`): a hand-copied duplicate of the above added by #678 AC12 because the original is private.
- `FolderPredictor.AddRecents` (`FolderPredictor.cs:788-795`): appends `_globals.AF.RecentsList` verbatim with no projection.
- `FolderPredictor.GetOlSubpath` (`FolderPredictor.cs:953-971`): blind `Substring` with no prefix verification.
- `FolderMinimalWrapper.ToRelativePath` (`FolderMinimalWrapper.cs:84`) and `FolderWrapper.RelativePath` (`FolderWrapper .cs:194-224`): unanchored `Replace(root + "\\", "")` with a full-path fallback when the root is null, which is how a rooted label can enter the persisted classifier corpus.
- `EfcFormController.BindBreadcrumbRowsAsync` (`QuickFiler\Controllers\EfcFormController.cs:1115-1118`): passes projected `rows` with raw `Suggestions.ToScoredArray()` scores to `BindRowsAsync`, which joins by presented-text equality, so archive-rooted suggestions lose their score. `AddSuggestionRows` (`FolderPredictor.cs:835-846`) projects the score path; this site does not.
- `ToDoModel\Email Utilities\SortItemsToExistingFolder.cs:67-109`: legacy unanchored `Contains(StrRoot)` and `Substring` root handling.

Stale labels: the resolution failures logged above come from `OutlookFolderHierarchyProvider.ResolveByUniqueSuffix` (`OutlookFolderHierarchyProvider.cs:85-112`) when a persisted suggestion label (classifier corpus, subject map, or recents) names a folder that no longer exists at that path. Those rows fall back to a single segment. The change should make the fallback visibly distinct (or filter such labels) and log once per label rather than once per render.

Runtime facts that constrain the design: `AppOlObjects.ArchiveRootPath` (`TaskMaster\AppGlobals\AppOlObjects.cs:260-270`, `AppOlObjects.ArchiveRoot.cs:86-93`) is derived from the default store root plus the literal folder `Archive`, validated by `ArchiveRootPathGuard`, and throws rather than returning null. Persisted suggestion labels are archive-relative when written correctly (`OlFolderClassifierGroup.cs:205`, `SubjectMapSco.Orchestration.cs:19-33`, `AppAutoFileObjects.cs:211-229`).


## Proposed Fix

### Design summary (what changes where):

### Boundaries and invariants to preserve:

### Dependencies or blocked work:

### Implementation strategy (what changes, not sequencing):
	
#### Files/modules to change:

#### Functions/classes/CLI commands impacted:

#### Data flow and validation changes:

#### Error handling and logging updates:

#### Rollback/feature-flag considerations (if applicable):

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

#### Required configuration keys and defaults:

#### Backward-compatibility expectations:

#### Performance constraints (latency/throughput/memory):

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
- Constraints (budget, performance, compatibility):
- External dependencies (services, libraries, releases):

## Data / API / Config Impact
- User-facing or API changes:
- Data or migration considerations:
- Logging/telemetry updates (if any):
- Compatibility notes (CLI flags, config schemas, versioning):

## Test Strategy
Seeded from issue:

Acceptance criteria settled with the maintainer on 2026-09-06:

- [ ] AC1: Suggestion rows and search-result rows in both the QuickFiler item view and the Efc view render the lineage starting at the first segment below the archive root, with the same arrow rendering and clickable ancestor segments for both row kinds. Example: `_Active Projects -> Build RGF Org and Team -> Sales Lead`.
- [ ] AC2: A resolved chain that does not pass through the archive root node is logged as an error and rendered with the existing single-segment fallback; no row ever shows the mailbox or Archive segment.
- [ ] AC3: Filing target and score-lookup key remain the archive-relative stem (unchanged #439 constraint); filing to the selected folder still lands correctly.
- [ ] AC4: `ProjectSuggestionPath` and `ProjectPredeterminedFolder` are replaced by one shared projection built on `ArchiveStemContract.TryMakeArchiveRelative`, and the empty-root one-separator strip is eliminated.
- [ ] AC5: Recent-folder entries pass through the same projection before display.
- [ ] AC6: `EfcFormController.BindBreadcrumbRowsAsync` projects the score paths the same way as the rows, so archive-rooted suggestions retain their percentage.
- [ ] AC7: Persisted suggestion labels that fail hierarchy resolution are rendered distinguishably (or filtered) and logged once per label per session, not once per render.
- [ ] AC8: The leading-underscore rendering question (`_Active Projects` vs `_ Active Projects`) is verified and, if the renderer alters it, corrected.

Validation:

- [ ] Unit coverage areas: chain trimming below the archive root (chain through root, chain not through root, chain equal to root); shared projection against the #614 contract cases (case-insensitive, trailing separators, `Archive2` boundary); recents projection; Efc score join with rooted and relative paths; renderer segment text preservation for leading underscores.
- [ ] Integration scenario to retest: bind a row set with a suggestion, a search result, a `====` banner, the trash pseudo-row, and a stale label; assert lineage on both folder row kinds, fallback on the stale label, none on banner/trash.
- [ ] Manual verification notes: QuickFiler ordinary and High Confidence, plus Efc view: confirm no row begins with the mailbox or Archive, and that clicking a middle segment navigates to that ancestor.

- Regression tests to add or update:
- Unit tests (pytest) for the fixed behavior and boundaries:
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
- Error handling and logging verification:
- Coverage impact and targets for changed lines/modules:
- Toolchain commands to run (format → lint → type-check → test):
- Manual validation steps (if required):


## Acceptance Criteria
- [ ] Repro steps now produce the expected behavior in all documented environments.
- [ ] Regression test(s) added and passing (list file path and test name).
- [ ] Edge cases and invalid inputs are handled with correct errors or fallbacks.
- [ ] No unintended behavior changes outside the defined scope.
- [ ] Required logs/telemetry updated and validated (if applicable).
- [ ] Performance constraints met or explicitly waived with rationale.
- [ ] Full toolchain pass completed (format → lint → type-check → test).
- [ ] Docs/config references updated to match the new behavior.

## Risks & Mitigations
- Technical or operational risks:
- Mitigations and rollbacks:

## Rollout & Follow-up
- Release/rollout steps:
- Post-fix monitoring or clean-up tasks:
- Links: issue, PRs, related docs
