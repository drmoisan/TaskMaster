# 2026-07-04-coverage-gaps-test-seams-236 - Refactor Spec

- **Issue:** #236
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-07-04T13-15
- **Status:** Draft
- **Version:** 0.1

## Intent & Outcomes

TaskMaster has strong test coverage overall, but several QuickFiler elements still
have uncovered logic because their current implementations couple directly to
Outlook COM, WinForms viewers, static queues, and UI control state. The target
elements are:

- `EfcViewerQueue`
- `ItemViewerQueue`
- `QfcThemeHelper`
- `EfcHomeController`
- `TlpCellStates`

The coverage gap should be closed by adding testable seams or isolating logic
into testable methods. Coverage exemptions are not permitted.

The provided research artifact
`artifacts/research/2026-07-04T13-19-issue-236-coverage-gaps-test-seams-research.md`
is sufficient to complete this spec. It identifies the target files, current
test barriers, existing repository seam patterns, required C# toolchain order,
and rejected alternatives.

## Invariants (must not change)

- The canonical issue number is #236, and all feature references use
  `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/`.
- The work mode remains `full-feature`; `spec.md` and `user-story.md` are the
  authoritative acceptance-criteria sources for planning and review.
- `EfcViewerQueue` and `ItemViewerQueue` retain their existing public static
  method names and production behavior for current callers.
- `QfcThemeHelper` keeps existing production overloads and theme names while
  adding or exposing testable construction paths only where needed.
- `EfcHomeController` keeps its current public constructor and static factory
  methods source-compatible for production callers.
- `TlpCellStates` remains usable by existing QuickFiler controllers, queues,
  viewers, and tests.
- Unit tests do not require live Outlook COM, external services, mutable
  machine state, temporary files, full `EfcViewer` construction, full
  `ItemViewer` construction, or live QuickFiler form windows.
- No `[ExcludeFromCodeCoverage]`, runsettings exclusion, coverage configuration
  exclusion, or policy workaround is added for the five issue #236 targets.
- Evidence artifacts produced during implementation and validation are stored
  only under
  `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/<kind>/`.
- Performance characteristics to preserve: queue prebuild and dequeue behavior
  remains suitable for existing QuickFiler startup and page-loading flows; any
  additional indirection is local and should not add user-visible blocking.
- Compatibility guarantees: no CLI, external API, serialized configuration,
  database schema, or user-facing setting changes are expected.

## Scope (structural changes)

Refactor the target elements so deterministic unit tests can exercise their
decision logic without launching Outlook, dereferencing live COM objects, or
requiring live WinForms windows. Use narrow seams where external boundaries must
remain, and keep default production behavior compatible with existing callers.

Where seam injection is insufficient, move pure logic into focused methods or
small collaborators so minimal code remains untestable.

Expected in-scope production touchpoints:

- `QuickFiler/Helper Classes/EfcViewerQueue.cs`
- `QuickFiler/Helper Classes/ItemViewerQueue.cs`
- `QuickFiler/Helper Classes/QfcThemeHelper.cs`
- `QuickFiler/Helper Classes/TlpCellSnapShot.cs` for `TlpCellStates`
- `QuickFiler/Controllers/EfcHomeController.cs`
- Narrow internal helper files only if needed to keep files cohesive and under
  repository size limits.

Expected in-scope test touchpoints:

- `QuickFiler.Test` MSTest files for queue services, theme helper behavior,
  `TlpCellStates`, and `EfcHomeController` seam behavior.
- Existing test patterns using MSTest, Moq, FluentAssertions,
  `InternalsVisibleTo("QuickFiler.Test")`, and fake/synchronous dispatcher or
  factory delegates.

## Non-Goals

- Adding coverage exemptions or broadening existing coverage exclusions.
- Replacing MSTest, Moq, or FluentAssertions, or adding a new test framework.
- Introducing a broad dependency-injection container conversion.
- Using Microsoft Fakes/Shims, static interception, or reflection-only tests as
  the primary coverage strategy.
- Changing QuickFiler user-facing behavior, Outlook filing behavior, theme
  visuals, ribbon actions, page navigation, high-confidence filtering behavior,
  or queue sizing semantics except where a local test seam requires production
  adapters.
- Adding CLI flags, environment variables, persisted settings, database
  migrations, telemetry systems, or external service dependencies.
- Retiring existing public APIs or changing caller contracts unless no additive
  seam can satisfy issue #236; any unavoidable contract change must be
  documented and updated for all in-repo callers.

## Dependencies / Touchpoints

- Existing seam patterns:
  - `UtilitiesCS/Threading/IUiDispatcher.cs` for dispatcher abstraction.
  - `QuickFiler/Viewers/IItemViewer.cs` and
    `QuickFiler/Interfaces/IQfcFormViewer.cs` for viewer/UI boundaries.
  - Existing `QfcItemController` tests that use synchronous dispatcher mocks
    and factory delegates.
- Downstream callers:
  - `QfcCollectionController`, `QfcFormController`, `QfcItemController`,
    `QfcQueue`, `BayesianPerformanceController`, and
    `TaskMaster/Ribbon/RibbonController` rely on current QuickFiler surfaces.
- Test project dependencies:
  - `QuickFiler.Test` targets .NET Framework 4.8.1 and already references
    `QuickFiler`, MSTest, Moq, and FluentAssertions.
  - `QuickFiler/Properties/AssemblyInfo.cs` already grants
    `InternalsVisibleTo("QuickFiler.Test")`, so internal seams can remain
    non-public.
- Required coordination:
  - Atomic planning must include baseline coverage, post-change coverage, and
    changed-code/per-target coverage evidence under the canonical feature
    evidence directory.
  - Tests that override static queue defaults must restore production defaults
    and account for class-level parallelization in `TaskMaster.runsettings`.

## Risks & Mitigations

- Preserve existing production behavior for Outlook and QuickFiler workflows.
- Do not add coverage exemptions or weaken coverage policy.
- Keep seams narrow and local to COM, WinForms, and static-construction
  boundaries.
- Avoid broad refactors outside the named coverage targets unless required to
  expose a testable boundary.
- Unit tests must remain deterministic and must not depend on Outlook, external
  services, mutable machine state, or temporary files.
- Risk: static queue state can make tests order-dependent. Mitigation: move
  queue mechanics into instance-owned internal services and keep static wrappers
  as production adapters; add reset/restore support only where required.
- Risk: `EfcHomeController` construction allocates live collaborators.
  Mitigation: use an internal dependency bundle or factory seams while keeping
  public constructors and static factories compatible.
- Risk: `QfcThemeHelper` reads concrete `ItemViewer` controls directly.
  Mitigation: add a pure internal control-set/input model used by tests and
  retain the production overload as an adapter.
- Risk: changing null handling in `TlpCellStates` could affect callers.
  Mitigation: test current behavior first; if production behavior changes,
  prefer explicit `ArgumentNullException` and document the contract.
- Risk: file-size limits may constrain test placement. Mitigation: add focused
  test files rather than expanding already large test classes.


## Technical Specifications

- `EfcViewerQueue`
  - Keep `BuildQueue(int)` and `Dequeue()` source-compatible.
  - Move queue mechanics into an internal service or equivalent core that owns
    queue state and accepts an `EfcViewer` factory plus dispatcher/scheduler
    seam.
  - Default production adapters continue to construct `EfcViewer` on the
    appropriate UI dispatcher path.
  - Tests cover build count, cached dequeue, empty dequeue, replacement
    scheduling, cancellation/disposal boundaries, and reset behavior without
    live `EfcViewer` construction.
- `ItemViewerQueue`
  - Keep `BuildQueueWhenIdle(int)`, `BuildQueueBackground(int)`,
    `BuildQueue(int)`, `Dequeue(CancellationToken)`, and `DequeueChunk(int)`
    source-compatible.
  - Move queue mechanics into an internal service or equivalent core that owns
    queue state and accepts an `ItemViewer` factory plus dispatcher/scheduler
    seam.
  - Preserve current cancellation semantics unless implementation explicitly
    documents and tests a stricter contract.
  - Tests cover idle/background/synchronous build paths, cached and empty
    dequeue, chunk dequeue when the queue is short, replacement scheduling,
    cancellation boundaries, and reset behavior without live `ItemViewer`
    construction.
- `QfcThemeHelper`
  - Keep existing production extension methods and `SetupThemes(...)` /
    `SetupFormThemes(...)` contracts.
  - Add a focused internal input model or overload that accepts test-controlled
    controls, menu items, WebView2 references or adapters where required,
    mail-read delegates, and dispatcher seam inputs.
  - Tests verify the expected theme keys, representative color assignments,
    control-group names, direct `SetTheme` extension behavior, and
    `SetupFormThemes` behavior with handleless controls.
- `EfcHomeController`
  - Keep existing public constructor and static factory methods as production
    adapters.
  - Add an internal dependency bundle, internal overload, or equivalent factory
    seams for viewer dequeue/construction, data model creation, keyboard
    handler creation, explorer-controller creation, form-controller creation,
    and Outlook selection traversal.
  - Tests cover controller decision paths that can be exercised through
    injected collaborators, including explicit-mail input, empty selection,
    selection snapshot handling, initialization sequencing, and no live COM
    traversal.
- `TlpCellStates`
  - Add direct tests before changing production behavior.
  - Cover empty construction, typed-list construction, raw-list construction
    and conversion to `TlpCellSnapShotList`, duplicate-key behavior,
    `TryAddState` success/failure, empty input collections, and null input
    behavior.
  - If changed, null collection inputs should throw a specific
    `ArgumentNullException`.
- Public interfaces/contracts affected:
  - Prefer additive internal seams and adapters. Any public or internal
    interface change must update all in-repo callers and must be called out in
    implementation notes.
- Data flow or validation adjustments:
  - Queue state becomes instance-owned in testable cores while static wrappers
    retain production access.
  - Theme construction separates pure mapping inputs from live viewer access.
  - `EfcHomeController` separates collaborator construction from decision
    logic.
- Logging/telemetry updates:
  - None required for issue #236 unless implementation discovers a required
    production diagnostic at a new seam boundary.
- Migration or backfill needs:
  - None. This feature changes testability boundaries and unit coverage only.

## Test Strategy

- Regression tests to add or update:
  - `EfcViewerQueue` service/wrapper tests for queue build and dequeue paths.
  - `ItemViewerQueue` service/wrapper tests for all build variants, dequeue,
    and chunk behavior.
  - `QfcThemeHelper` tests for pure theme construction and form theme mapping.
  - `EfcHomeController` tests for construction and initialization decisions
    through injected seams.
  - `TlpCellStates` tests for constructors, conversions, duplicates, `TryAdd`,
    empty input, and null input.
- Invariant validation tests:
  - Production default adapters still invoke real viewer factories through the
    intended dispatcher path.
  - Existing public/static method names remain callable by current callers.
  - Theme names and representative control-group mappings remain consistent.
- Edge cases and negative scenarios:
  - Empty queue, non-empty queue, queue shorter than requested chunk, and
    cancellation token already canceled or canceled during dispatched work where
    applicable.
  - Empty theme control collections where supported by existing constructors.
  - Empty and duplicate `TlpCellStates` keys.
  - Null `TlpCellStates` collection inputs, with current behavior documented or
    explicit `ArgumentNullException` introduced and tested.
- Error handling and logging verification:
  - Verify explicit exceptions for any newly validated null inputs.
  - No new production logging is required unless implementation changes a
    boundary where existing diagnostics need preservation.
- Coverage impact and targets:
  - No coverage exemptions for issue #236 targets.
  - Repository-wide line coverage remains at least 80%.
  - Changed or newly introduced non-exempt code reaches at least 90% coverage.
  - Baseline, post-change, regression, and coverage evidence is written under
    `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/<kind>/`.
- Toolchain commands to run in final pass:
  - `csharpier .`
  - `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
- Manual validation:
  - No live Outlook manual validation is required for issue #236 unless
    implementation changes production behavior beyond testability seams.

## Definition of Done

- [x] AC1 - `EfcViewerQueue` deterministic coverage is added without live
      `EfcViewer` construction in unit tests.
- [x] AC2 - `ItemViewerQueue` deterministic coverage is added without live
      `ItemViewer` construction in unit tests.
- [x] AC3 - `QfcThemeHelper` deterministic coverage is added for theme
      construction and control-group mapping without live QuickFiler form
      instances.
- [x] AC4 - `EfcHomeController` deterministic coverage is added through
      Outlook COM, data model, viewer, keyboard, explorer-controller, and
      form-controller seams.
- [x] AC5 - `TlpCellStates` deterministic coverage is added for constructors,
      conversion, duplicates, `TryAddState`, empty inputs, and null-input
      behavior.
- [x] AC6 - Existing public/static production entry points remain
      source-compatible and all in-repo callers compile.
- [x] AC7 - No coverage exemptions or coverage-policy weakenings are added for
      the issue #236 targets.
- [x] AC8 - Repository-wide coverage remains at or above 80%, and changed or
      newly introduced non-exempt issue #236 code reaches at least 90% coverage.
- [x] AC9 - Baseline, QA, regression, and coverage evidence is stored under
      `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/<kind>/`.
- [x] AC10 - Final C# toolchain pass succeeds in order: CSharpier, .NET
      analyzers, nullable analysis with warnings as errors, and MSTest with
      coverage.

## Acceptance Evidence

| AC | Status | Evidence |
| --- | --- | --- |
| AC1 | PASS | `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/regression-testing/queue-tests.2026-07-04T13-15.md`; `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/final-coverage-targets.2026-07-04T13-15.md` |
| AC2 | PASS | `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/regression-testing/queue-tests.2026-07-04T13-15.md`; `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/final-coverage-targets.2026-07-04T13-15.md` |
| AC3 | PASS | `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/regression-testing/theme-tests.2026-07-04T13-15.md`; `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/final-coverage-targets.2026-07-04T13-15.md` |
| AC4 | PASS | `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/regression-testing/efc-home-controller-tests.2026-07-04T13-15.md`; `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/final-coverage-targets.2026-07-04T13-15.md` |
| AC5 | PASS | `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/regression-testing/tlp-cell-states-tests.2026-07-04T13-15.md`; `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/final-coverage-targets.2026-07-04T13-15.md` |
| AC6 | PASS | `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/final-analyzer-build.2026-07-04T13-15.md`; `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/final-nullable-build.2026-07-04T13-15.md` |
| AC7 | PASS | `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/final-no-coverage-exemptions.2026-07-04T13-15.md` |
| AC8 | PASS | `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/remediation-cycle2-current-coverage-thresholds.2026-07-04T18-52.md` reports repository coverage 81.08% against the 80.00% threshold, issue #236 changed/new coverage 95.74% against the 90.00% threshold, and per-file and target coverage gates passing. |
| AC9 | PASS | `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/final-evidence-location-audit.2026-07-04T13-15.md` |
| AC10 | PASS | `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/final-toolchain-loop.2026-07-04T13-15.md` |

## Seeded Test Conditions (from potential)

- [ ] Queue creation and dequeue behavior with injected factories or providers.
- [ ] Cancellation and disposal behavior around viewer acquisition.
- [ ] Theme group construction with representative buttons and panels.
- [ ] `TlpCellStates` constructors from both typed snapshot lists and raw
      snapshot lists.
- [ ] `EfcHomeController` paths that can be covered through injected
      collaborators rather than live COM or live forms.
- [x] Full MSTest run with code coverage enabled.
