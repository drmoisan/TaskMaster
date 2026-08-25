# itemviewer-surface-defects (Issue #489)

- Issue: #489
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/489
- Type: bug
- Work Mode: full-bug
- Epic: quickfiler-bug-family
- Integration Branch: epic/quickfiler-bug-family-integration
- Wave: 2
- Owner: drmoisan
- Last Updated: 2026-08-25
- Status: Active

## Summary

This feature closes four pre-existing defect issues in the QuickFiler item-viewer surface
(`QuickFiler/Viewers/ItemViewer*.cs`, `QuickFiler/Viewers/ToolStripMenuItemCb.cs`). All four were
filed on 2026-08-07 during preparation research for epic #136 child F14 (issue #456) and were
deferred out of that child because its non-functional requirement prohibited behavior change to
observable QuickFiler flows. Each defect alters observable behavior on a UI-thread, menu, or
display-contract path and therefore requires its own regression test.

The four issues are grouped into one feature because they are confined to the same `ItemViewer` /
`ItemViewerExpanded` twin family and its `IItemViewer` contract. Splitting them would produce four
concurrent branches editing the same partial-class set.

## Issues Closed by This Feature

| Issue | Title | Primary files | Severity |
|---|---|---|---|
| #486 | `itemviewer-move-option-menu-defects` | `ToolStripMenuItemCb.cs`, `ItemViewer.cs`, `ItemViewerExpanded.cs` | Medium (user-visible) |
| #487 | `itemviewer-parentchanged-console-and-cast` | `ItemViewer.cs`, `ItemViewerExpanded.cs` (+ their `.Designer.cs`) | Low |
| #489 | `itemviewer-ui-thread-marshalling-divergence` (primary) | `ItemViewer.cs`, `ItemViewer.WebViewThread.cs`, `IItemViewer.cs` | Medium-High |
| #490 | `itemviewer-display-and-folder-contract-defects` | `ItemViewer.FolderSearch.cs`, `ItemViewer.DisplayState.cs`, `ItemViewer.Commands.cs` | Medium |

## Authoritative Requirement Sources

The promoted potential documents are the authoritative requirement source. Each carries file:line,
the offending code block, root cause, suggested disposition, and severity, and is materially richer
than the GitHub issue body (promotion retained only the `## Summary` section).

- `docs/features/potential/promoted/2026-08-07-itemviewer-move-option-menu-defects.md` (#486)
- `docs/features/potential/promoted/2026-08-07-itemviewer-parentchanged-console-and-cast.md` (#487)
- `docs/features/potential/promoted/2026-08-07-itemviewer-ui-thread-marshalling-divergence.md` (#489)
- `docs/features/potential/promoted/2026-08-07-itemviewer-display-and-folder-contract-defects.md` (#490)

## Upstream Dependencies

This is a wave-2 child. Both upstreams are already on the integration branch and their post-change
shape is authoritative for planning.

| Upstream | Folder | Closes | Contract source |
|---|---|---|---|
| 484 | `docs/features/active/qfc-item-controller-defects-484/` | #480, #481, #483, #484, #485 | `spec.md` upstream-contract table |
| 444 | `docs/features/active/quickfiler-keyboard-action-defects-444/` | #444, #472, #482 | `spec.md` |

Downstream dependent: 488.

## Acceptance Criteria

Acceptance criteria for this `full-bug` feature are authored in `spec.md`, which is the sole
acceptance-criteria source per the `acceptance-criteria-tracking` skill. This section is a pointer,
not a second source.

- See `docs/features/active/itemviewer-surface-defects-489/spec.md` § Acceptance Criteria.

## Scope Restrictions

- Scope is limited to #486, #487, #489 and #490. A deeper design problem found during this work is
  recorded in `spec.md` § Out-of-Scope Findings and opened as a new issue later, per the CLAUDE.md
  Bugfix Workflow.
- `QuickFiler.csproj` and `QuickFiler.Test.csproj` may be edited only inside the alphabetical
  `Compile Include` region this feature owns; every child in this epic touches those legacy
  non-SDK project files and region discipline is what prevents collision at execution time.
