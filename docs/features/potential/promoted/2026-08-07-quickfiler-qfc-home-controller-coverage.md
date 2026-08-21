# quickfiler-qfc-home-controller-coverage

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-08-07-quickfiler-qfc-home-controller-coverage-433/ (Issue #433)
- Parent epic issue: #136 (https://github.com/drmoisan/TaskMaster/issues/136)
- Parent epic manifest: docs/features/epics/quickfiler-per-file-coverage/epic.md (child F7, wave 1)

## Problem / Why

Epic #136 requires every testable production file compiled by `QuickFiler/QuickFiler.csproj` to reach
at least 80% line coverage measured **per production file**, or to be placed on the ratified exemption
ledger delivered by child F1. Child F7 owns the `QfcHomeController` partial family plus its two
interface declarations:

- `QuickFiler/Controllers/QfcHomeController.cs` (487 lines)
- `QuickFiler/Controllers/QfcHomeController.Metrics.cs` (234 lines)
- `QuickFiler/Controllers/QfcHomeController.Iteration.cs` (86 lines)
- `QuickFiler/Controllers/IQfcHomeController.cs` (20 lines)
- `QuickFiler/Interfaces/IFilerHomeController.cs` (45 lines)

This type already carries the densest existing test suite in `QuickFiler.Test`. The work is therefore
expected to be gap-closing against an existing suite rather than a from-scratch coverage effort, and
duplicating an existing test is a defect.

## Proposed Behavior

No behavior change to observable QuickFiler flows. The deliverable is per-file coverage evidence plus
the minimum set of new deterministic MSTest tests (and, only where no seam exists, the minimum
testability seam) required to bring each `testable` file in the list to >= 80% line coverage as
measured by child F1's per-file coverage harness.

## Acceptance Criteria (early draft)

- [ ] Each file classified `testable` by the F1 ledger reaches >= 80% line coverage under F1's harness.
- [ ] Numeric per-file coverage evidence is committed under `<FEATURE>/evidence/qa-gates/`.
- [ ] No production file in scope exceeds 500 lines (`QfcHomeController.cs` is at 487).
- [ ] Tests use MSTest, Moq, and FluentAssertions; deterministic, isolated, no temporary files.
- [ ] Full C# toolchain passes in final form.
- [ ] No behavior change to observable QuickFiler flows.

## Constraints & Risks

- In-flight issue #424 (`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424`)
  targets a `QfcHomeController` queue-initialization stall; new tests must not contradict its
  regression tests.
- Iteration and metrics paths carry ordering and state-transition invariants; `RunAsync` is the async
  hot path. `Thread.Sleep`, `Task.Delay`, and real wall-clock waits are prohibited in tests.
- The home controller consumes `IQfcDatamodel` (sibling F5), `IQfcQueue` (sibling F2), and the
  collection controller (sibling F11). Those files belong to siblings and must not be edited here.
- Upstream dependency on F1 (`quickfiler-coverage-ledger`), which supplies the exemption ledger and
  the per-file coverage harness.

## Test Conditions to Consider

- [ ] `RunAsync` cancellation, zero-batch, and re-entrancy scenarios.
- [ ] Iteration ordering and state transitions (`Iterate`, `Iterate2`, `IterateQueueAsync`, `SwapStopWatch`).
- [ ] Metrics accumulation, formatting, and write paths with an injected clock.
- [ ] Invalid-input, boundary, and error-handling behavior for each covered member.

## Next Step

- [x] Promote to GitHub issue (#433)
- [x] Create active feature folder `docs/features/active/2026-08-07-quickfiler-qfc-home-controller-coverage-433/`
