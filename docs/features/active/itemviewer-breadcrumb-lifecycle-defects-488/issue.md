# itemviewer-breadcrumb-lifecycle-defects

- Issue: #488
- Also closes: #475
- Type: bug
- Work Mode: full-bug
- Epic: quickfiler-bug-family
- Integration branch: epic/quickfiler-bug-family-integration
- Owner: drmoisan
- Last Updated: 2026-08-25
- Status: Active

> Provenance note: both issues below and their potential entries were created and promoted before
> this feature folder existed. No new potential entry and no new GitHub issue was created for this
> feature. The promoted records are
> `docs/features/potential/promoted/2026-08-07-itemviewer-breadcrumb-pipeline-lifecycle.md` (#488)
> and
> `docs/features/potential/promoted/2026-08-07-breadcrumb-capturecurrentortests-silently-degrades-in-production.md`
> (#475).

## Summary

This feature closes two pre-existing defect issues in the QuickFiler breadcrumb pipeline that
`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` owns or reaches. Both were filed on 2026-08-07 during
preparation research for epic #136 and were deferred out of that epic's children because those
children carry a hard no-behavior-change non-functional requirement. Each defect alters observable
behavior on a construction, replacement, or teardown path and therefore requires its own regression
test.

The two issues are grouped into one feature because #475's fix edits two call sites inside the same
file that #488's five defects live in (`ItemViewer.Breadcrumb.cs:156` and `:192`). Splitting them
would produce two concurrent branches editing the same file.

## Issues Closed by This Feature

| Issue | Title | Primary surface |
| --- | --- | --- |
| #488 | itemviewer-breadcrumb-pipeline-lifecycle | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` |
| #475 | breadcrumb-capturecurrentortests-silently-degrades-in-production | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`, `BreadcrumbDropDownHost.cs`, `ItemViewer.Breadcrumb.cs` |

#488 is primary. Both are `Work Mode: full-bug`, so `spec.md` is the authoritative acceptance-criteria
source and `user-story.md` is intentionally absent.

## Epic Context

- Epic feature folder: `quickfiler-bug-family`
- Integration branch: `epic/quickfiler-bug-family-integration`
- Wave: 3 (last child of the epic)
- Upstream dependency: feature `itemviewer-surface-defects-489` (issue #489; also closes #486, #487,
  #490), which is prepared concurrently with this feature and owns adjacent `ItemViewer` surface.
  Dependencies on that feature's contract are enumerated in `spec.md` under `## Dependencies on 489`.
- Sibling reference: `docs/features/active/qfc-item-controller-defects-484/spec.md` carries an
  exhaustive upstream-contract table for `QfcItemController`; cite it rather than re-deriving member
  lists or detach counts from source.

## Acceptance Criteria

The authoritative acceptance criteria for this `full-bug` feature live in `spec.md` under
`## Acceptance Criteria`, per `.claude/skills/acceptance-criteria-tracking/SKILL.md`. This section is
a pointer only and carries no criteria of its own.
