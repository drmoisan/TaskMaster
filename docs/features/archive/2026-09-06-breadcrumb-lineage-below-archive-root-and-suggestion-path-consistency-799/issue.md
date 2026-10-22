# breadcrumb-lineage-below-archive-root-and-suggestion-path-consistency (Issue #799)

- Date captured: 2026-09-06
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/breadcrumb-lineage-below-archive-root-and-suggestion-path-consistency/ (Issue #799)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #799
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/799
- Last Updated: 2026-09-07
- Work Mode: full-bug

## Summary

In QuickFiler (ordinary and High Confidence modes) suggestion rows render their breadcrumb lineage from the store root, for example `dmoisan@realgoodfoods.com -> Archive -> _Active Projects -> Build RGF Org and Team -> Sales Lead`, while typed search-result rows render `_Active Projects -> Build RGF Org and Team -> Sales Lead`. The mailbox and Archive segments are superfluous: every filing target is under the archive root, and the lineage must begin at the first segment below it for every row kind. The full-root lineage is the behavior delivered by issue #439 ("full root-to-leaf ancestor lineage"), so this is a specification change, not a regression. The same change should unify several archive-root stripping paths that have drifted into duplicates and gaps.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8 VSTO Outlook add-in with WebView2 breadcrumb, debug build from `TaskMaster\bin\Debug`, HEAD `c431dc32` (2026-09-06)
- Command/flags used: Outlook ribbon -> QuickFiler and QuickFiler High Confidence; folder drop-down in the item view (`ItemViewer` breadcrumb, `FolderBreadcrumb.html`)
- Data source or fixture: live mailbox; archive root resolves to `\\dmoisan@realgoodfoods.com\Archive` (`AppOlObjects.ArchiveRootPath`)

## Steps to Reproduce

1. Launch QuickFiler on Inbox. Open the folder drop-down on an item without typing.
2. Observe the first suggestion row: `dmoisan@realgoodfoods.com -> Archive -> _Active Projects -> Build RGF Org and Team -> Sales Lead`.
3. Type a few letters into the search box. Observe the same folder as a search result: `_Active Projects -> Build RGF Org and Team -> Sales Lead`. Typing `90` shows `_Active Projects -> 90 Day Plan`.
4. Accept a suggestion: filing lands in the correct Outlook and file-system folders (the filing target is the archive-relative stem, not the displayed lineage).

Note: the maintainer's transcription of the search row showed `_ Active Projects` with a space after the underscore. The orchestrator should verify whether the renderer alters a leading underscore or whether this was a transcription artifact.

## Expected Behavior

- Every row in the folder list, suggestion or search result, renders as `_Active Projects -> Build RGF Org and Team -> Sales Lead`: the lineage begins at the first segment below the archive root, each segment is clickable for ancestor navigation, and no row shows the mailbox or Archive segments.
- A resolved ancestor chain that does not pass through the archive root node is logged as an error and rendered with the existing single-segment fallback, never with a mailbox prefix. There are no legitimate filing targets outside the archive root.
- One archive-root projection rule is used everywhere a suggestion path is prepared for display or compared against displayed entries.

## Actual Behavior

- Suggestion rows show the mailbox and Archive segments; search rows do not. Ordinary and High Confidence modes behave the same.
- Recent-folder entries are appended to the suggestion list with no projection at all.
- The Efc breadcrumb binding joins projected row text against raw scorer paths, so an archive-rooted suggestion loses its percentage.
- Several suggestion rows fail hierarchy resolution and fall back to a single segment, with errors such as `No snapshot node path ends with '\Forums\Pricing'; leaving 'Forums\Pricing' unresolved.`

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet (`TaskMaster\bin\Debug\logs\debug_2026-09-06.log`), stale-label resolution failures:

```
2026-09-06 16:45:51,908 [VSTA_Main] ERROR UtilitiesCS.OutlookObjects.Folder.OutlookFolderHierarchyProvider - No snapshot node path ends with '\Scorecards\Monthly Scans'; leaving 'Scorecards\Monthly Scans' unresolved.
2026-09-06 16:45:51,937 [VSTA_Main] ERROR UtilitiesCS.OutlookObjects.Folder.OutlookFolderHierarchyProvider - No snapshot node path ends with '\Forums\Pricing'; leaving 'Forums\Pricing' unresolved.
```

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Display-only: filing lands correctly. The redundant segments consume most of the row width and make same-named folders harder to distinguish, which is the problem #439 set out to solve.

## Suspected Cause / Notes

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

## Proposed Fix / Validation Ideas

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

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

## Outcome

Implemented on 2026-09-07 on branch `bug/breadcrumb-lineage-below-archive-root-799`, across the three phases of
`plan.2026-09-06T22-01.md`.

**This is a specification change superseding issue #439, not a regression fix against it.** Issue #439 delivered
full root-to-leaf ancestor lineage deliberately, and that behaviour was correct against its own acceptance
criteria. This item narrows the rendered lineage to begin at the first segment below the archive root because the
mailbox and Archive segments carry no information in a system where every filing target is under the archive root,
and because they consume most of the row width and defeat the row-distinguishability goal #439 itself set out to
serve. Nothing in #439 is being repaired.

**#439's filing-target and score-key constraint is preserved and carried forward as AC3.** The trim removes only
LEADING segments from the rendered chain; the filing value and the score-lookup key remain the archive-relative
stem, substituted into the LEAF segment, exactly as #439 required. AC3 pins this explicitly rather than leaving it
as an incidental consequence, via the test
`BindRowsAsync_TrimmedChain_PreservesFilingTargetAndScoreKey`. All ten tests of the #439 partial class
`BreadcrumbBridgeRouterIssue439Tests`, across both its files, pass unmodified: neither file carries a hunk in this
change, because every test in them drives a strict provider mock that sits below the trim boundary.

### What was delivered

- AC1 and AC2: the ancestor-chain trim lives in `OutlookFolderHierarchyProvider.GetAncestorChainAsync`, the single
  seam both the QuickFiler drop-down and the Efc list route through, so one change serves both surfaces. A chain
  that does not pass through the archive root, or whose leaf IS the root, is logged once and returns an empty
  segment list, which routes each surface into its existing fallback.
- AC3: filing target and score-lookup key remain the archive-relative stem.
- AC4: `ProjectSuggestionPath` and `ProjectPredeterminedFolder` now both delegate to the new shared
  `ArchiveStemProjection.ToDisplayStem`, built on `ArchiveStemContract.TryMakeArchiveRelative`. The empty-root
  one-separator strip is eliminated. Four of the seven candidate sites were converted; three were deliberately
  left, with reasons recorded in the specification's decision D-A.
- AC5: recent-folder entries are projected at both sites, the string append and the row-model mirror, preserving
  the documented text-parity contract between the two lists.
- AC6: the Efc router adds a projected score alongside each raw score, so an archive-rooted suggestion presented as
  a stem retains its percentage and a rooted-presented row does not lose its own.
- AC7: stale labels are logged once per label per provider instance rather than once per render, on both surfaces.
  Zero-candidate labels are additionally suppressed from the rendered row set on the Efc surface.
- AC8: verified as a finding, not a fix. No renderer alters a leading underscore; the reported space was a
  transcription artifact and a renderer change would have been a defect.

### Deviations from the specification's own prose

Four, each recorded by name with its reason in `spec.md` under Rollout & Follow-up, section Outcome: AC7 row
suppression is delivered on the Efc surface only; the AC6 score projection is additive rather than substitutive;
the two #439 Efc router test files carry no hunk; and the AC7 absence classification is published through a new
small public interface rather than through a fourth member on the shared hierarchy contract.

### Verification

The final toolchain loop closed clean in a single pass: CSharpier format and check both exit 0 over 1601 files,
the analyzer gate and the nullable gate each exit 0 with 0 Warning(s) and 0 Error(s), and the coverage-enabled
nine-assembly run exits 0 with 7085 tests, 7085 passed, 0 failed and `NEWLY-FAILING: NONE` against a 7048-test
baseline. First-party line coverage moved from 84.55 to 84.58 percent and branch coverage from 79.24 to 79.28
percent on the pinned comparability index; no changed line lost coverage. Both new production types reach 100
percent line and branch coverage. Evidence is under this feature folder's `evidence/qa-gates/` and
`evidence/regression-testing/` directories.

### Acceptance criteria

The authoritative acceptance-criteria source for this `full-bug` item is `spec.md`, section Acceptance Criteria.
All eight criteria AC1 through AC8 are checked off there. The mirrored list under "Proposed Fix / Validation
Ideas" above is left as captured, because it is the intake record rather than the tracked criteria source.
