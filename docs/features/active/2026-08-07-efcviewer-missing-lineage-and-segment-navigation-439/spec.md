# 2026-08-07-efcviewer-missing-lineage-and-segment-navigation (Spec)

- **Issue:** #439
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-24T17-30
- **Status:** Draft
- **Version:** 0.1

## Context
In EfcViewer, suggested and searched folder rows render as a single leaf name with no ancestor lineage, so the arrow-separated ancestor chain is missing. The companion behavior is also gone or non-functional: clicking a non-leaf part of the lineage should move up to that ancestor node in the tree and let the user expand that node to see all of its children.

Environment:
- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1 WinForms VSTO add-in with Microsoft WebView2
- UI path: EfcViewer folder list (`EfcViewer.FolderListBox`, exposed as `BreadcrumbWebView`), driven by `EfcFormController` through `BreadcrumbBridgeRouter` and `QuickFiler/Resources/FolderBreadcrumb.html`
- Data source or fixture: `EfcDataModel.FindMatches` search results and `FolderPredictor.Suggestions` suggestion rows, under an `ArchiveRootPath`-rooted search

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

The lineage is how the user disambiguates same-named folders across different parents, and the ancestor-click-then-expand path is how the user reaches a sibling folder that the predictor did not suggest. Without both, filing to anything other than an exact suggestion requires repeated searching, and a wrong-parent match can be selected without the user noticing.


## Repro & Evidence
Steps to Reproduce:
1. Open EfcViewer on a mail item so the folder list is populated with suggestions.
2. Observe the suggestion rows: each shows only a folder name, with no ancestor chain and no arrow separators.
3. Type a folder search string so the SEARCH RESULTS section is populated, and observe the same absence of lineage on the search-result rows.
4. On any row that does show more than one lineage segment, click a segment that is not the leaf.
5. Observe that the click does not move the selection up to that ancestor node and does not offer an expansion of that node into its children.

Expected:
- Every suggestion row and every search-result row renders its full root-to-leaf ancestor lineage, with each ancestor separated from the next by an arrow separator (the `→` separator cell in `FolderBreadcrumb.html`, written `->` in the original report).
- Clicking a non-leaf lineage segment moves the selection up to that ancestor node in the folder tree.
- The ancestor node selected that way can then be expanded to show all of its children, so the user can pick a sibling of the originally-suggested folder without retyping a search.
- Rows whose ancestor chain genuinely cannot be resolved still render and stay selectable (the existing single-segment fallback), but this must be the exception, not the normal case.

Actual:
- Suggestion and search-result rows render as one leaf-only segment; no ancestor lineage and no arrow separators appear.
- Clicking a non-leaf segment does not select that ancestor node and provides no way to expand it into its children. The only wired segment gesture is a double-click that collapses the row after the clicked segment.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Code-read evidence (2026-08-07) is recorded under Suspected Cause below; no runtime log capture yet.


## Scope & Non-Goals
- In scope:
- Out of scope / non-goals:
- Explicitly excluded systems, integrations, or datasets:

## Root Cause Analysis
Read of the current sources on 2026-08-07. There are two distinct defects.

**A. Ancestor chain never resolves, so the lineage falls back to a single segment.**

- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:894-930` — `LoopFolders` adds `folderStem` to the match list, where `folderStem` is `GetOlSubpath(f.FolderPath, olAncestor, true)` (line 934), which strips the archive-root prefix. The presented row text is therefore an archive-root-relative stem, not a full Outlook folder path.
- `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs:52-71` — `ResolveLeafKeyAsync` matches the presented text against `node.FolderPath` using exact `OrdinalIgnoreCase` equality against a full Outlook folder path (the comment at lines 64-65 states real Outlook full paths embed the store name). A relative stem cannot match, so the method returns `null`.
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:333-352` — `FetchChainAsync` returns `null` when `ResolveLeafKeyAsync` returns `null`, without calling `GetAncestorChainAsync`.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:119-129` — with a null chain, `BuildRow` takes the documented fallback and emits one leaf-only segment. The row stays visible and selectable, which is why the failure presents as missing lineage rather than as missing rows.

Confirm during planning whether the suggestion rows (`FolderPredictor.AddSuggestions`, line 804, via `Suggestions.ToArray(5)`) use the same relative-stem form as the search matches. The reported symptom covers both sections, which is consistent with a single shared path-form mismatch, but the suggestion path was not traced end to end for this write-up.

Note the same key-form question applies to the probability join in `BreadcrumbRowBuilder.BuildProbabilityIndex` (keyed on `FolderScore.FolderPath`); if the forms disagree there too, the percentage would also be dropped. That is adjacent to issue #400 and should be checked, not assumed.

**B. Non-leaf segment click does not navigate to the ancestor or expand it.**

- `QuickFiler/Resources/FolderBreadcrumb.html:250-257` — a segment cell wires only a `dblclick` handler, which posts `segmentDoubleClick`. There is no single-click ancestor-navigation gesture.
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:168-172` — `SegmentDoubleClick` calls `row.CollapseAfter(segmentIndex)` and re-renders. It collapses the row's trailing segments; it does not change the selected node and does not request the ancestor's children.
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:225-250` — the only expansion path is `ExpandLeafAsync(row)`, reached from `Right` or the leaf toggle. There is no code path that expands an arbitrary non-leaf ancestor segment into its children.

`FolderBreadcrumb.html:258-261` confirms the intended separator glyph is `→`, so the separator itself is implemented; only the multi-segment chain that would use it is absent.


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

Design direction (to be confirmed during planning):

- Establish one canonical folder-path form at the boundary between the predictor's presented rows and `IFolderHierarchyProvider`. Either present full folder paths, or normalize the archive-root-relative stem back to a full path before resolution. Do not add prefix-matching heuristics inside `BreadcrumbRowBuilder`; the builder's contract explicitly derives no hierarchy from row text.
- Treat an unresolved chain as a diagnosable condition, not a silent fallback: log at a level that makes a systematic resolution failure visible rather than presenting as a cosmetic omission.
- Add an ancestor-navigation message for a non-leaf segment gesture, distinct from the existing `segmentDoubleClick` collapse, that selects the ancestor node and makes its children expandable.

Validation:

- [ ] Unit coverage areas: MSTest coverage over the path-form normalization (relative stem to full path and the identity case), over `BreadcrumbRowBuilder.BuildRow` asserting a resolved chain yields multiple segments in root-to-leaf order, over the renderer asserting arrow separator cells appear between segments, and over the router asserting a non-leaf segment gesture selects the ancestor and requests its children. Use Moq for `IFolderHierarchyProvider`; no live Outlook dependency.
- [ ] Integration scenario to retest: bind a presented row set containing a search result, a suggestion, a `====` banner, and the `Trash to Delete` pseudo-row, and assert lineage is present on the folder rows and absent (correctly) on the banner and trash rows.
- [ ] Manual verification notes: in EfcViewer, confirm suggestion and search rows show the full arrow-separated chain, then click a middle ancestor and confirm the selection moves there and its children can be expanded.

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
