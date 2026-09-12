# quickfiler-item-controller-coverage (Issue #453)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-item-controller-coverage/ (Issue #453)
- Parent epic: #136 (QuickFiler Per-File 80% Coverage)
- Epic child: F10 (wave 1, complexity band C3)
- Integration branch: `epic/quickfiler-per-file-coverage-integration`

- Issue: #453
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/453
- Last Updated: 2026-08-08
- Work Mode: full-feature

## Problem / Why

Epic #136 requires every production `.cs` file compiled by `QuickFiler/QuickFiler.csproj` to reach
at least 80% line coverage and 75% branch coverage, or to sit on an explicitly ratified exemption
ledger. The `QfcItemController` partial-class family is one of the largest single clusters in that
denominator: 10 production partials plus one interface file, 3,180 lines in total.

The family is already substantially tested — 16 existing test files exist under
`QuickFiler.Test/Controllers/` — so this is a **gap-closure and exemption-removal** exercise rather
than a build-from-zero effort. Four partials measured below the 80% line floor on the most recent
committed Cobertura report, and 19 method-level `[ExcludeFromCodeCoverage]` attributes across six
partials remove real production logic from instrumentation entirely.

## Proposed Behavior

Raise every `testable` file in the `QfcItemController` family to at least 80% line and 75% branch
coverage, verified with the per-file coverage harness delivered by epic child F1, and remove every
`[ExcludeFromCodeCoverage]` attribute whose covered code can be reached through a seam. Retain an
attribute only where F1's ledger ratifies it as irreducible with a file-specific rationale.

No observable behavior change to QuickFiler flows. Testability refactors follow the epic's seam
hierarchy: interface seam, then injectable delegate, then adapter.

## Acceptance Criteria (early draft)

- [ ] Every `testable` file in scope reaches >= 80% line and >= 75% branch coverage.
- [ ] Every `[ExcludeFromCodeCoverage]` in scope is removed with the covered code tested, or
      retained only under an F1-ledger-ratified, file-specific rationale.
- [ ] No production file in scope exceeds 500 lines; any newly created file reaches >= 90% line
      coverage.
- [ ] Tests use MSTest, Moq, and FluentAssertions; deterministic, isolated, no temporary files,
      no external services, no live forms, no popups.
- [ ] Full C# toolchain green in final form; repository-wide coverage retained or improved.
- [ ] No behavior change to observable QuickFiler flows.

## Constraints & Risks

- **One partial-class family.** All 10 partials declare the same type. The family is deliberately
  kept in a single epic child so no two children share a type or a test fixture.
- **Sibling boundaries.** `ConversationResolver` constructors belong to F4 (#434) and are invoked
  positionally from this family. `KeyboardHandler.cs` belongs to F3 (#430). `IQfcDatamodel` belongs
  to F5. Required upstream changes are recorded as cross-child contract notes, not sibling edits.
- **State-transition invariants.** Event wiring, navigation, and initialization carry ordering,
  re-entrancy, and dispose-before-setup invariants that must be covered explicitly.
- **Determinism.** Injected clock and fake timers only; `Thread.Sleep`, `Task.Delay`, and real
  wall-clock waits are prohibited in tests.
- **Upstream dependency.** F1 delivers the per-file coverage harness and the exemption ledger. A
  Phase 0 halt gate on those deliverables is required.
- **Branch coverage is a separate gate.** Line coverage at or above 80% does not imply branch
  coverage at or above 75%; both are reported independently.

## Test Conditions to Consider

- [ ] Unit coverage of the four sub-floor partials: `ViewerSetup`, `FocusAndTheme`, `MailActions`,
      `EventHandlers`.
- [ ] Coverage of the 19 currently method-exempted members across six partials.
- [ ] Event-wiring subscribe/unsubscribe symmetry, re-entrancy, and idempotent dispose.
- [ ] Navigation state transitions and boundary conditions.
- [ ] Initialization ordering, including dispose-before-setup.
- [ ] Negative and error-handling paths for each covered member.

## Next Step

- [x] Promote to GitHub issue (feature request template)
- [x] Create `docs/features/active/quickfiler-item-controller-coverage/` folder from the template
