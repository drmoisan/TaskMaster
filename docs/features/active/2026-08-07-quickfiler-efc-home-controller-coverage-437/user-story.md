# `quickfiler-efc-home-controller-coverage` — User Story

- **Issue:** #437
- **Parent epic:** #136 `quickfiler-per-file-coverage` (child F8, wave 1, band C3)
- **Owner:** drmoisan
- **Status:** Draft
- **Last Updated:** 2026-08-07
- **Work Mode:** `full-feature` — this file is a co-authoritative acceptance-criteria source
  alongside `spec.md`

## Story Statement

- **As a QuickFiler maintainer**, I want the six `EfcHomeController` production files to have their
  per-file coverage re-measured on this branch with the epic's own harness and pinned by acceptance
  criteria, **so that** the coverage level they already hold cannot silently regress the next time
  someone edits the controller family.
- **As a QuickFiler maintainer**, I want the specific behaviorally-important paths that no test
  reaches today — the re-entrancy guard reset when a move faults, the Finder `Run` path, the
  production metrics fallback, and the Outlook selection traversal — covered by named deterministic
  tests, **so that** a regression in any of them fails a test rather than reaching a user.
- **As a maintainer who delegates work to coding agents**, I want the per-file coverage number for
  `EfcHomeController.cs` to be reproducible run to run, **so that** the number an agent reports as
  evidence means the same thing on every machine and in every test-class ordering.
- **As a maintainer**, I want the seeded assumptions that research disproved to be corrected in
  writing and the genuinely-untestable items promoted to their own issues, **so that** the next
  person to read this folder is not misled into implementing work that does not apply, and nothing
  real is quietly dropped.

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

Per-file research completed on 2026-08-07 established that **all six files already exceed the 80%
per-file line floor**. Indicative figures from a Cobertura report committed by a sibling in-flight
feature (`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`)
range from 93.16% to 100% line coverage. That artifact was captured on the `...-424` branch, not on
F8's, so it is strong indicative evidence and **not** the acceptance authority; F1's per-file harness
remains the authority and the numbers must be re-measured on F8's branch.

This changes what the child is for. It is not rescue work. It is gap closure and invariant pinning,
with one measurable floor genuinely unmet (`EfcHomeController.Timing.cs` at 66.67% branch coverage,
below the 75% floor in `.claude/rules/general-unit-test.md`) and one reproducibility hazard that
undermines the evidence mechanism itself.

## Value Framing — why this matters when coverage is already above the floor

The epic's business-outcome hypothesis is that bringing every testable QuickFiler file to at least
80% line coverage "reduces regression escapes in QuickFiler and makes the project safe for autonomous
agentic maintenance." For a file set that is already above the floor, the value accrues in three
specific ways rather than as a percentage increase.

**Regression safety on paths that percentage does not see.** A line metric counts statements, not
guarantees. The `try/finally` in `ExecuteMovesAsync` exists solely so that `_isExecuting` is restored
when the move seam throws — and that path has never been executed by a test. The Finder flow's
`Run()`/`RunAsync()` arm has never been exercised, because every existing test supplies a non-null
`Mail` and short-circuits before the flag check. `LoadSelection`'s Outlook traversal, the only
COM-adjacent path in the dependency file, is untested despite being fully mockable. These are the
paths where a regression escapes, and closing them is the concrete regression-safety deliverable.

**Evidence that means what it says.** `EfcHomeController.cs` installs two separate default-lambda
instances with identical bodies. Exactly one is covered per run, and which one depends on test-class
execution order. Because F1's harness output is this child's acceptance evidence, an
order-dependent number is a defect in the evidence mechanism, not a cosmetic issue. Consolidating
the two into one shared default is what makes the committed number reproducible.

**Confidence for autonomous maintenance.** An agent editing this controller family needs to know
which behaviors are load-bearing. Several are currently undocumented and unpinned: that the default
factories read their `Production*` statics at invocation time for six of eleven delegates but bind
eagerly for two others; that `_globals` is captured before the await specifically because `Cleanup()`
nulls it; that `ResetProductionFactoriesForTesting` actually restores every default the rest of the
suite depends on. Pinning these with tests converts tribal knowledge into an executable contract.

## Personas & Scenarios

### Persona — QuickFiler maintainer

- **Who:** the engineer (or delegated coding agent) responsible for QuickFiler's Outlook add-in
  behavior, working in a VSTO/WinForms codebase with an active long-term goal of migrating away from
  VSTO.
- **What they care about:** that a change to the EFC controller family is safe to merge without a
  manual Outlook smoke test, and that the test suite tells them so quickly and without flakiness.
- **Constraints:** unit tests must never construct a live form, show a popup, touch a live Outlook
  store, or write to disk; test classes run in parallel, so process-global statics are a live
  flakiness risk; no production file or test file may exceed 500 lines.
- **Frustrations:** coverage numbers that shift between runs; tests that assert only "does not
  throw"; reflection-based tests that degrade silently after a rename; a CI job that hangs on a
  modal message box.
- **Goals:** merge with confidence, and hand work to an agent without hand-holding.

### Scenario — a later change to the move path

A maintainer changes `ExecuteMovesCoreAsync` to read the selected folder after the await instead of
before it, believing the read is equivalent.

1. **Trigger:** a refactor that looks locally harmless.
2. **Today:** every existing test uses a synchronously-completing move seam, so the await never
   actually suspends. The ordering invariant is line-covered but never exercised. The suite stays
   green, and the `NullReferenceException` that the pre-await capture was written to prevent returns
   in production the next time `Cleanup()` races the await.
3. **After this child:** the `TaskCompletionSource`-controlled test suspends at the await, nulls
   `_globals`, then completes the task and asserts that the metrics recorder received the original
   globals instance. The refactor fails a named test with an actionable message.
4. **Expected outcome:** the maintainer sees why the code was written that way, and either preserves
   the ordering or makes a deliberate, documented decision to change it.

### Scenario — a maintainer inherits the seeded assumptions

An engineer picks up this folder to plan implementation and reads the seeded acceptance criteria.

1. **Trigger:** seeded criteria that require an injected clock in `Timing.cs`, partial-failure
   mid-batch coverage in `ExecuteMoves.cs`, and mid-batch cancellation coverage.
2. **Obstacle:** none of the three exists. `Timing.cs` reads no clock at all — it is four logging
   helpers. There is no batch loop; the move seam returns a single boolean and iteration happens
   downstream in `EmailFiler.SortAsync`. `ExecuteMovesAsync` has no `CancellationToken` parameter and
   no cancellation checkpoint, so covering cancellation would require both a production behavior
   change (barred by the epic NFR) and a breaking seam-signature change (barred by the additive-only
   constraint).
3. **After this child:** `spec.md` records all five corrections as documented deviations with the
   superseded wording quoted, and the cancellation gap is promoted as its own GitHub issue rather
   than dropped.
4. **Expected outcome:** the implementer builds the tests that exist to be built, and the deferred
   work survives the merge as a tracked issue.

## Acceptance Criteria

These criteria are identical in substance to the `## Acceptance Criteria` section of `spec.md`; both
files are authoritative for `full-feature` work mode and must be checked off in step.

- [ ] **AC1 — Per-file line coverage floor retained.** All six F8 production files
      (`EfcHomeController.cs`, `EfcHomeController.ExecuteMoves.cs`, `EfcHomeController.Metrics.cs`,
      `EfcHomeController.Timing.cs`, `EfcHomeControllerDependencies.cs`,
      `EfcHomeControllerDependencyFactories.cs`) measure >= 80% line coverage, re-verified on F8's
      branch with F1's per-file harness, with the numeric per-file result committed under
      `<FEATURE>/evidence/qa-gates/`. The `...-424` Cobertura figures are indicative only and are not
      accepted as this evidence.
- [ ] **AC2 — `Timing.cs` branch floor cleared.** `EfcHomeController.Timing.cs` measures >= 75%
      branch coverage in the same F1-harness evidence artifact (indicative baseline: 66.67%).
- [ ] **AC3 — `EfcHomeController.cs` gaps closed.** Every gap G1 and G3-G10 in
      `research/EfcHomeController.research.md` § 3 is closed by a named test, including the
      `Run()`/`RunAsync()` Finder arm and the constructor-path factory call order.
- [ ] **AC4 — `ExecuteMoves.cs` gaps closed.** The uncovered line set and the three half-covered
      branches in `research/EfcHomeController.ExecuteMoves.research.md` § 3 are closed, explicitly
      including `ExecuteMovesAsync` resetting `_isExecuting` through the `finally` block when the
      move seam faults, and the pre-await capture of `_globals` verified under a
      `TaskCompletionSource`-controlled suspension.
- [ ] **AC5 — `Metrics.cs` gap closed.** Line 23 and the line-18 non-empty-list branch outcome are
      covered deterministically via a never-started `Stopwatch`, with no timer, sleep, delay, or
      wall-clock read.
- [ ] **AC6 — `EfcHomeControllerDependencies.cs` gaps closed.** `LoadSelection`'s null-`globals`
      guard, its Outlook-selection path including the `x is MailItem` filter, and its empty and
      single-item boundaries are covered through the mocked `IApplicationGlobals`/`IOlObjects`
      interface chain; the invocation-time versus eager binding asymmetry is pinned by tests on both
      sides.
- [ ] **AC7 — `EfcHomeControllerDependencyFactories.cs` gaps closed.**
      `CreateProductionExplorerControllerInstance` is covered;
      `ResetProductionFactoriesForTesting` is asserted as a restoration contract using `.Method.Name`
      identity checks that never invoke a default; and composition-layer ordering, result
      propagation, late binding, and the no-memoization invariant are pinned. CCN-1's five
      initializer closure bodies are recorded as an accepted residual, not closed.
- [ ] **AC8 — Coverage reproducibility.** The duplicate default-dependency-lambda hazard in
      `EfcHomeController.cs` (L24-25 and L37) is removed so that both sites share one
      `static readonly` default and the per-file coverage number is order-independent.
- [ ] **AC9 — File-size compliance.** No production file and no test file in scope exceeds 500 lines,
      including after the `EfcHomeControllerExecuteMovesTests.cs` split and the extraction of the
      shared reflection and fake-globals helpers.
- [ ] **AC10 — Test safety.** `MoveFailureMessageAction` is overridden in every test that can reach a
      failure path; and the `EfcViewerQueue.Dequeue`, `EfcDataModel.CreateAsync`,
      `FileIO2.WriteTextFile`, and `Production*Initializer` defaults are never invoked — identity is
      asserted via `.Method.Name` only.
- [ ] **AC11 — Parallelization safety.** Every new or modified test class that mutates the
      `Production*` statics or `_defaultDependenciesFactory` is marked `[DoNotParallelize]` and
      restores state in `[TestCleanup]`; the existing
      `EfcHomeControllerDependenciesTestsProductionFactory` is marked `[DoNotParallelize]`.
- [ ] **AC12 — Test conventions.** All new and modified tests use MSTest, Moq, and FluentAssertions
      in Arrange-Act-Assert form; are deterministic and isolated; and use no temporary files, external
      services, live forms, popups, live Outlook store, `Thread.Sleep`, `Task.Delay`, or real
      wall-clock waits.
- [ ] **AC13 — Corrections and amendments recorded.** The five corrected seeded assumptions (C1-C5)
      and the three scope amendments are documented in `spec.md`, and the deferred items — mid-batch
      cancellation and the seven latent defects — are promoted to their own GitHub issues via the MCP
      promotion lifecycle, with issue numbers recorded there.
- [ ] **AC14 — No behavior change and no sibling edits.** No observable QuickFiler flow changes; every
      production edit is behavior-preserving and confined to F8-owned files; F9 requires no edit; and
      `coverage.config` and all shared build property files are unmodified.
- [ ] **AC15 — Toolchain green.** The full C# toolchain passes in final form in a single pass:
      `csharpier .`, the analyzer msbuild, the nullable msbuild, and coverage-enabled
      `vstest.console.exe`.

## Non-Goals

- Raising the observable capability of QuickFiler. This child is an enabler; end-user behavior is
  unchanged.
- Fixing the seven latent defects recorded in `spec.md` (inert stopwatch, `.Seconds` truncation,
  non-atomic check-then-set, missing CSV separator, inconsistent `xComma` sanitization, the
  `NotImplementedException` overload, and the binding-time asymmetry). Each is promoted separately.
- Adding cancellation support to `ExecuteMovesAsync`.
- Editing any file owned by sibling child F9 (`EfcFormController.cs`, `EfcItemController.cs`,
  `EfcViewer.cs`) or F6 (`QfcExplorerController.cs`), and closing CCN-1's five residual lines.
- Converting the existing delegate seams into interface seams, which would be a non-additive change
  to a shared surface for no coverage benefit.
- Changing `coverage.config`, any shared build property file, or the repository-wide coverage
  thresholds.
