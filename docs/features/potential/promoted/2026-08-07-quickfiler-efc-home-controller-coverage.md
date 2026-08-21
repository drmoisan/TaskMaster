# quickfiler-efc-home-controller-coverage (Potential — Promoted)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> [Issue #437](https://github.com/drmoisan/TaskMaster/issues/437)
- Active folder: `docs/features/active/2026-08-07-quickfiler-efc-home-controller-coverage-437/`
- Parent epic issue: [#136](https://github.com/drmoisan/TaskMaster/issues/136) — QuickFiler per-file 80% coverage
- Epic manifest: `docs/features/epics/quickfiler-per-file-coverage/epic.md` (child F8, wave 1, band C3)
- Integration branch: `epic/quickfiler-per-file-coverage-integration`
- Depends on: F1 `quickfiler-coverage-denominator-and-exemption-ledger` (per-file coverage harness and ratified exemption ledger)

> This file is an audit-trail reconstruction. `mcp__drm-copilot__potential_to_issue` reported
> `destination_path` as `docs/features/potential/promoted/2026-08-07-quickfiler-efc-home-controller-coverage.md`
> but did not leave the file on disk. The GitHub issue and the active feature folder are the
> authoritative records; this file preserves the pre-promotion content.

## Problem / Why

Epic #136 requires every testable production file compiled by `QuickFiler/QuickFiler.csproj` to
reach at least 80% line coverage, measured per file rather than per assembly. Child F8 owns the
`EfcHomeController` partial-class family and its dependency-injection factories — six files
totalling approximately 1,411 lines:

| File | Lines |
| --- | --- |
| `QuickFiler/Controllers/EfcHomeController.cs` | 441 |
| `QuickFiler/Controllers/EfcHomeControllerDependencies.cs` | 428 |
| `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs` | 268 |
| `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs` | 144 |
| `QuickFiler/Controllers/EfcHomeController.Metrics.cs` | 87 |
| `QuickFiler/Controllers/EfcHomeController.Timing.cs` | 43 |

Existing test files cover parts of this surface, but `EfcHomeControllerDependencyFactories.cs` has
no dedicated test file and the actual per-file line coverage of each of the six files is unmeasured.
Aggregate assembly coverage does not satisfy issue #136.

## Proposed Behavior

No change to observable QuickFiler behavior. Add deterministic MSTest coverage — and, only where a
seam is genuinely required to reach otherwise-unreachable logic, add an additive injectable seam —
so that every file classified `testable` by F1's ledger reaches at least 80% line coverage,
verified with F1's per-file coverage harness and recorded as numeric evidence.

## Acceptance Criteria (early draft)

The authoritative acceptance criteria live in
`docs/features/active/2026-08-07-quickfiler-efc-home-controller-coverage-437/spec.md` and
`user-story.md` (work mode `full-feature`). The early draft is preserved in `issue.md` of the same
folder.

## Constraints & Risks

- `EfcHomeControllerDependencies` and `EfcHomeControllerDependencyFactories` form the injection-seam
  contract for the whole EFC controller family, including `EfcFormController` and
  `EfcItemController`, which belong to sibling child F9. Any change to the dependency contract must
  be additive so F9 needs no edit.
- `ExecuteMoves` performs Outlook `MailItem`/`MAPIFolder` moves and must be exercised only through
  an injected move seam.
- `EfcHomeController.Timing.cs` requires an injected clock; `Thread.Sleep`, `Task.Delay`, and real
  wall-clock waits are prohibited in tests.
- This child must not modify `coverage.config` or any shared build property file.

## Next Step

- [x] Promote to GitHub issue (feature request template) — Issue #437
- [x] Create the active feature folder from the template
