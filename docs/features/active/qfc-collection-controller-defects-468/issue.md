# qfc-collection-controller-defects (Issue #468)

- Issue: #468
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/468
- Work Mode: full-bug
- Type: bug
- Owner: drmoisan
- Epic: quickfiler-bug-family (integration branch `epic/quickfiler-bug-family-integration`)
- Last Updated: 2026-08-24

## Summary

This feature closes seven pre-existing bug issues concentrated in
`QuickFiler/Controllers/QfcCollectionController.cs`. All seven were filed from a single
solution-wide review of that controller and share one file, so they are remediated together
rather than as seven independent changes to the same 2,349-line source file.

The authoritative requirement text for each defect is the promoted potential document listed
below. Those documents carry `file:line`, the offending code block, root cause, suggested fix,
and severity, and they are richer than the corresponding GitHub issue bodies.

## Closed Issues and Authoritative Sources

| Issue | Title | Authoritative potential document |
|---|---|---|
| #286 | `RemoveSpecificControlGroupAsync` reentrancy-counter leak | `docs/features/potential/promoted/2026-07-09-qfc-collectioncontroller-removespecificcontrolgroup-counter-leak.md` |
| #468 | Unreachable load paths (dead code) | `docs/features/potential/promoted/2026-08-07-qfc-collection-controller-unreachable-load-paths.md` |
| #469 | Move-diagnostics defects (4) | `docs/features/potential/promoted/2026-08-07-qfc-collection-move-diagnostics-defects.md` |
| #470 | Conversation index and null-guard defects (3) | `docs/features/potential/promoted/2026-08-07-qfc-collection-conversation-index-defects.md` |
| #471 | `EliminateSpaceForItems` sign error | `docs/features/potential/promoted/2026-08-07-qfc-collection-eliminate-space-sign-error.md` |
| #473 | Background-task reset race and double-catch defects (2) | `docs/features/potential/promoted/2026-08-07-qfc-collection-background-task-and-catch-defects.md` |
| #474 | Controller coupling and modal property getter (2) | `docs/features/potential/promoted/2026-08-07-qfc-collection-controller-coupling-and-modal-getter.md` |

Note on promotion lifecycle: all seven issues were already open on GitHub and their potential
entries were already promoted before this feature folder was created. No new potential entry and
no new issue were created for this work.

## Files This Feature Owns

- `QuickFiler/Controllers/QfcCollectionController.cs`
- `QuickFiler/Interfaces/IQfcCollectionController.cs`
- `QuickFiler/Controllers/IQfcFormController.cs`
- `QuickFiler/Interfaces/IFilerFormController.cs`
- `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` (existing test file)
- `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs` (existing test file)

## Files This Feature Must Not Write

- `QuickFiler/Controllers/KbdActions.cs` — owned by sibling epic child issue #444, which depends
  on this feature and merges after it. Any change this feature's analysis concludes is required
  in that file is recorded as a downstream note in `spec.md` for #444 and kept out of the plan.

## Acceptance Criteria

The authoritative acceptance criteria for this `full-bug` work mode live in `spec.md`. This
section is a pointer only and is not the acceptance-criteria source.

- See `spec.md` section `## Acceptance Criteria`.
