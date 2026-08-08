# quickfiler-efc-home-controller-coverage (Issue #437)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-efc-home-controller-coverage/ (Issue #437)
- Parent epic issue: [#136](https://github.com/drmoisan/TaskMaster/issues/136) — QuickFiler per-file 80% coverage
- Epic manifest: `docs/features/epics/quickfiler-per-file-coverage/epic.md` (child F8, wave 1, band C3)
- Integration branch: `epic/quickfiler-per-file-coverage-integration`
- Depends on: F1 `quickfiler-coverage-denominator-and-exemption-ledger` (per-file coverage harness and ratified exemption ledger)

- Issue: #437
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/437
- Last Updated: 2026-08-08
- Work Mode: full-feature

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

Existing test files cover parts of this surface (`EfcHomeControllerTests.cs`,
`EfcHomeControllerLifecycleTests.cs`, `EfcHomeControllerSeamTests.cs`,
`EfcHomeControllerMetricsTests.cs`, `EfcHomeControllerExecuteMovesTests.cs`,
`EfcHomeControllerDependenciesTests.cs`, `EfcHomeControllerDependenciesProductionFactoryTests.cs`),
but `EfcHomeControllerDependencyFactories.cs` has no dedicated test file and the actual per-file
line coverage of each of the six files is unmeasured. Aggregate assembly coverage does not satisfy
issue #136.

## Proposed Behavior

No change to observable QuickFiler behavior. Add deterministic MSTest coverage — and, only where a
seam is genuinely required to reach otherwise-unreachable logic, add an additive injectable seam —
so that every file classified `testable` by F1's ledger reaches at least 80% line coverage,
verified with F1's per-file coverage harness and recorded as numeric evidence.

## Acceptance Criteria (early draft)

- [ ] Every file in the F8 set that F1's ledger classifies `testable` reaches >= 80% line coverage,
      verified with F1's per-file harness and recorded numerically under `<FEATURE>/evidence/qa-gates/`.
- [ ] Any file F1's ledger classifies `ratified-exempt` is excluded from the target set, citing the
      ledger entry as the authority.
- [ ] No production file in scope exceeds 500 lines.
- [ ] Tests use MSTest, Moq, and FluentAssertions; they are deterministic and isolated, and use no
      temporary files, external services, live forms, or popups.
- [ ] Per-file coverage spans the positive path plus invalid-input, boundary, and error-handling
      behavior.
- [ ] `ExecuteMoves` is covered through an injected move seam, never against a live Outlook store,
      including partial-failure and mid-batch-cancellation behavior.
- [ ] `EfcHomeController.Timing.cs` is covered through an injected clock; `Thread.Sleep`,
      `Task.Delay`, and real wall-clock waits are absent from the tests.
- [ ] The full C# toolchain passes in final form: csharpier, analyzer build, nullable build,
      coverage-enabled vstest.
- [ ] No behavior change to observable QuickFiler flows.

## Constraints & Risks

- `EfcHomeControllerDependencies` and `EfcHomeControllerDependencyFactories` form the injection-seam
  contract for the whole EFC controller family, including `EfcFormController` and
  `EfcItemController`, which belong to sibling child F9. Any change to the dependency contract must
  be **additive** so F9 needs no edit. If an additive change is impossible, the required change is
  recorded in `spec.md` as a cross-child contract note rather than applied to F9's files.
- `ExecuteMoves` performs Outlook `MailItem`/`MAPIFolder` moves. It must be exercised only through
  an injected move seam.
- The move-execution and metrics paths carry ordering and state-transition invariants; partial
  failure and mid-batch cancellation must be covered explicitly.
- Seam hierarchy per `.claude/rules/csharp.md`: interface seam, then injectable delegate, then
  adapter. Never construct live forms in tests.
- This child must not modify `coverage.config` or any shared build property file, and must not edit
  files assigned to sibling children (see epic.md "Feature File Assignments").
- F1's outputs (harness and ledger) do not exist at preparation time; the plan consumes them as an
  upstream contract and F1 merges to the integration branch before F8 executes.

## Test Conditions to Consider

- [ ] Per-file line coverage measurement for each of the six production files
- [ ] `ExecuteMoves` happy path, partial failure mid-batch, cancellation mid-batch, empty batch
- [ ] Metrics accumulation ordering and state transitions
- [ ] Timing behavior under an injected clock
- [ ] Dependency-factory construction, null-argument rejection, and lazy/cached resolution behavior
- [ ] Controller lifecycle: initialize, run, dispose, double-dispose

## Next Step

- [x] Promote to GitHub issue (feature request template)
- [x] Create `docs/features/active/quickfiler-efc-home-controller-coverage/` folder from the template
