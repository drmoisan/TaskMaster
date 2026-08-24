# efcviewer-missing-lineage-and-segment-navigation (Issue #439)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/efcviewer-missing-lineage-and-segment-navigation/ (Issue #439)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #439
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/439
- Last Updated: 2026-08-08
- Work Mode: full-bug

## Summary

In EfcViewer, suggested and searched folder rows render as a single leaf name with no ancestor lineage, so the arrow-separated ancestor chain is missing. The companion behavior is also gone or non-functional: clicking a non-leaf part of the lineage should move up to that ancestor node in the tree and let the user expand that node to see all of its children.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1 WinForms VSTO add-in with Microsoft WebView2
- UI path: EfcViewer folder list (`EfcViewer.FolderListBox`, exposed as `BreadcrumbWebView`), driven by `EfcFormController` through `BreadcrumbBridgeRouter` and the generated `BreadcrumbHtmlRenderer` document
- Data source or fixture: `EfcDataModel.FindMatches` search results and `FolderPredictor.Suggestions` suggestion rows, under an `ArchiveRootPath`-rooted search

## Steps to Reproduce

1. Open EfcViewer on a mail item so the folder list is populated with suggestions.
2. Observe the suggestion rows: each shows only a folder name, with no ancestor chain and no arrow separators.
3. Type a folder search string so the SEARCH RESULTS section is populated, and observe the same absence of lineage on the search-result rows.
4. On any row that does show more than one lineage segment, click a segment that is not the leaf.
5. Observe that the click does not move the selection up to that ancestor node and does not offer an expansion of that node into its children.

## Expected Behavior

- Every suggestion row and every search-result row renders its full root-to-leaf ancestor lineage in the Efc-generated document, with each ancestor separated from the next by the `→` glyph.
- Clicking a non-leaf lineage segment moves the selection up to that ancestor node in the folder tree.
- The ancestor node selected that way can then be expanded to show all of its children, so the user can pick a sibling of the originally-suggested folder without retyping a search.
- Rows whose ancestor chain genuinely cannot be resolved still render and stay selectable (the existing single-segment fallback), but this must be the exception, not the normal case.

## Actual Behavior

- Suggestion and search-result rows render as one leaf-only segment; no ancestor lineage and no arrow separators appear.
- Clicking a non-leaf segment does not select that ancestor node and provides no way to expand it into its children. The only wired segment gesture is a double-click that collapses the row after the clicked segment.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Code-read evidence (2026-08-07) is recorded under Suspected Cause below; no runtime log capture yet.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

The lineage is how the user disambiguates same-named folders across different parents, and the ancestor-click-then-expand path is how the user reaches a sibling folder that the predictor did not suggest. Without both, filing to anything other than an exact suggestion requires repeated searching, and a wrong-parent match can be selected without the user noticing.

## Suspected Cause / Notes

Read of the current sources on 2026-08-07. There are two distinct defects.

**A. Ancestor chain never resolves, so the lineage falls back to a single segment.**

- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:894-930` — `LoopFolders` adds `folderStem` to the match list, where `folderStem` is `GetOlSubpath(f.FolderPath, olAncestor, true)` (line 934), which strips the archive-root prefix. The presented row text is therefore an archive-root-relative stem, not a full Outlook folder path.
- `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs:52-71` — `ResolveLeafKeyAsync` matches the presented text against `node.FolderPath` using exact `OrdinalIgnoreCase` equality against a full Outlook folder path (the comment at lines 64-65 states real Outlook full paths embed the store name). A relative stem cannot match, so the method returns `null`.
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:333-352` — `FetchChainAsync` returns `null` when `ResolveLeafKeyAsync` returns `null`, without calling `GetAncestorChainAsync`.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:119-129` — with a null chain, `BuildRow` takes the documented fallback and emits one leaf-only segment. The row stays visible and selectable, which is why the failure presents as missing lineage rather than as missing rows.

Research on 2026-08-24 confirmed that suggestions use the same archive-relative scorer key form as search matches. `FolderScore.Probability` is also keyed by that original relative form, so a hierarchy resolution to a full Outlook path must not replace the filing target or the score-lookup key. This is not an Issue #400 change.

**B. Non-leaf segment click does not navigate to the ancestor or expand it.**

- `BreadcrumbDocumentAssets.cs` emits the generated Efc document's segment and row event handlers. A segment-specific gesture posts only `segmentDoubleClick`; ordinary clicks bubble to whole-row selection. There is no typed non-leaf activation message.
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` handles `segmentDoubleClick` by calling `row.CollapseAfter(segmentIndex)` and re-rendering. It does not change the active selection or request the clicked ancestor's children. Its expansion path uses `row.LeafSegment`, so it cannot expand an arbitrary ancestor.
- `BreadcrumbHtmlRenderer.cs` is the active Efc renderer and writes `&gt;` between segments. `QuickFiler/Resources/FolderBreadcrumb.html` has a `→` glyph but belongs to the separate ItemViewer path and cannot fix EfcViewer.

## Proposed Fix / Validation Ideas

Design direction (to be confirmed during planning):

- At the Efc binding boundary, carry both the original archive-relative filing target and a hierarchy-only path. Expand a target with `ArchiveRootPath` only when it is not already rooted there; use the resulting full path only for `IFolderHierarchyProvider` exact resolution. Do not add fuzzy or prefix provider matching, and do not reconstruct paths in `BreadcrumbRowBuilder`.
- Keep the original filing target as the normal selection output and as the `FolderScore.Probability` lookup key. When an ancestor or returned child is activated, derive its selection target by removing the same archive-root prefix from that verified full hierarchy path.
- Retain the selectable one-segment fallback only for a null resolution key, an empty ancestor chain, or a provider failure. Make each such fallback diagnosable through the existing logging pattern; it must not be used for ordinary archive-relative targets.
- Extend the generated-document bridge with typed, validated non-leaf `segmentActivate` and rendered-child activation messages. Stop propagation for segment activation, select the indexed ancestor, query that ancestor's immediate children on expansion, and allow child or sibling activation. Keep `segmentDoubleClick` as the separate collapse gesture.
- Render `→` in `BreadcrumbHtmlRenderer` between visible Efc lineage segments. Do not modify ItemViewer's `FolderBreadcrumb.html`; keyboard Left/Right navigation, Issue #400 behavior, banners, and `Trash to Delete` remain outside this issue.

Validation:

- [ ] Unit coverage areas: MSTest coverage over the path-form normalization (relative stem to full path and the identity case), over `BreadcrumbRowBuilder.BuildRow` asserting a resolved chain yields multiple segments in root-to-leaf order, over the renderer asserting arrow separator cells appear between segments, and over the router asserting a non-leaf segment gesture selects the ancestor and requests its children. Use Moq for `IFolderHierarchyProvider`; no live Outlook dependency.
- [ ] Integration scenario to retest: bind a presented row set containing a search result, a suggestion, a `====` banner, and the `Trash to Delete` pseudo-row, and assert lineage is present on the folder rows and absent (correctly) on the banner and trash rows.
- [ ] Manual verification notes: in EfcViewer, confirm suggestion and search rows show the full arrow-separated chain, then click a middle ancestor and confirm the selection moves there and its children can be expanded.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
