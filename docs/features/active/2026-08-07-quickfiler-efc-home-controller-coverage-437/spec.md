# quickfiler-efc-home-controller-coverage — Spec

- **Issue:** #437
- **Parent epic:** #136 `quickfiler-per-file-coverage` (child F8, wave 1, band C3)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-07
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** `full-feature` (acceptance criteria are authoritative in `spec.md` **and**
  `user-story.md`; `issue.md` is not the AC source for this mode)
- **Research inputs:** the six per-file artifacts under `<FEATURE>/research/`

## Overview

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

None of the six carries `[ExcludeFromCodeCoverage]`, so all six are already inside the coverage
denominator and none requires an attribute disposition from F1.

### Central finding — this is gap closure, not rescue work

Per-file research (2026-08-07) establishes that **all six files already exceed the 80% per-file line
floor at baseline.** The value F8 delivers is therefore not bulk coverage. It is four things:

1. **Retain** >= 80% line coverage per file, re-verified on F8's branch with F1's per-file harness.
2. **Close** the specific behaviorally-important uncovered lines and half-covered branches
   enumerated per file in the research artifacts — re-entrancy reset on the exception path,
   production metrics fallback, `LoadSelection`'s Outlook path, the Finder `Run`/`RunAsync` arm, and
   the binding-time asymmetry of the default factories.
3. **Remove** a coverage-reproducibility hazard that makes the per-file number order-dependent, and
   therefore makes F1's harness output non-reproducible for `EfcHomeController.cs`.
4. **Raise** `EfcHomeController.Timing.cs` branch coverage above the >= 75% branch floor in
   `.claude/rules/general-unit-test.md`.

Any plan that proposes broad new test authoring to "reach 80%" for these files duplicates work that
already exists. Each research artifact carries a "Do not duplicate" section listing the scenarios
already covered.

## Baseline Evidence and Its Provenance

The following per-file figures come from a Cobertura report **committed by a sibling in-flight
feature**, read read-only:

```
docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml
```

| File | line-rate | branch-rate |
| --- | --- | --- |
| `EfcHomeController.cs` | 0.968481 | 0.890625 |
| `EfcHomeController.ExecuteMoves.cs` | 0.931624 | 0.833333 |
| `EfcHomeController.Metrics.cs` | 0.975904 | 0.916667 |
| `EfcHomeController.Timing.cs` | 1.0 | 0.666667 |
| `EfcHomeControllerDependencyFactories.cs` | 0.957895 | 1.0 |
| `EfcHomeControllerDependencies.cs` | 0.94431 | 0.93617 |

**Provenance caveat — stated explicitly.** This artifact was captured on the `...-424` feature
branch, not on F8's branch. It is strong **indicative** evidence that the 80% floor is already met
and that the acceptance criteria below are satisfiable. It is **not** the acceptance authority.

**Acceptance authority.** F1 (`quickfiler-coverage-denominator-and-exemption-ledger`) delivers the
per-file coverage harness that is the sole per-file evidence mechanism for epic #136. F8 must
re-measure all six files with that harness **on F8's own branch**, after F1 merges to the
integration branch, and commit the numeric result under `<FEATURE>/evidence/qa-gates/`. Aggregate
assembly coverage does not satisfy issue #136, and the `...-424` figures above must not be presented
as F8's evidence.

The one baseline figure that is **below** a repository floor is `EfcHomeController.Timing.cs` branch
coverage at 0.666667 against the >= 75% branch floor. The three uncovered conditional arms
enumerated in `EfcHomeController.Timing.research.md` § 4 are sufficient to clear that floor when
closed.

## Behavior

No change to observable QuickFiler behavior. F8 adds deterministic MSTest coverage and makes a small
number of behavior-preserving production edits (enumerated in "Production Edits Proposed") whose
purpose is coverage reproducibility and test-observability, not functional change. Every edit is
additive or line-count-neutral, confined to F8-owned files, and requires no edit to any
sibling-owned file.

## Corrections to Seeded Assumptions

The seeded `spec.md` and `issue.md` carried five assumptions that research disproved. Each is
recorded here as a documented deviation. The seeded text they replace is quoted so the change is
visibly deliberate.

### C1 — `Timing.cs` needs no injected clock; the real gap is branch coverage

Seeded text: *"`EfcHomeController.Timing.cs` is covered through an injected clock."*

`EfcHomeController.Timing.cs` reads **no clock**. It contains no `DateTime`, no `Stopwatch`, no
`Environment.TickCount`, and no `TimeProvider`. It is four `private static` diagnostic-logging
helpers (`DescribeSynchronizationContext`, `DescribeStartupOverlapState`,
`BuildFirstSelectionTimingContext`, `LogFirstSelectionTiming`). An injected clock here would be an
unused abstraction and is retired as inapplicable.

The real gap is **branch coverage at 66.67%, below the >= 75% floor** in
`.claude/rules/general-unit-test.md`. The uncovered arms are: `DescribeStartupOverlapState`'s
`"correlated"` arm, `DescribeSynchronizationContext`'s non-null arm asserted deterministically, and
`LogFirstSelectionTiming`'s null/whitespace-`details` arm.

The prohibition on `Thread.Sleep`, `Task.Delay`, and real wall-clock waits in tests remains a live
repo-wide constraint and is unaffected by this correction.

### C2 — There is no batch loop in `ExecuteMoves.cs`

Seeded text: *"`ExecuteMoves` happy path, **partial failure mid-batch**, cancellation mid-batch,
empty batch."*

The move seam is a single `Func<string, bool, bool, bool, bool, Task<bool>>` returning one boolean.
`ExecuteMovesCoreAsync` issues exactly one call to it. Per-item iteration lives downstream in
`EmailFiler.SortAsync`, outside F8's file set, and a partial downstream failure is flattened by
`EfcDataModel.MoveToFolderAsync` into `false`. **"Partial failure mid-batch" as a per-item
collect-and-continue behavior does not exist in this file** and cannot be tested here.

It is replaced by the real untested invariant: **exception propagation from the move seam with a
guaranteed `_isExecuting` reset via the existing `try/finally`** (lines 39-45, entirely unexecuted
today), plus the **pre-await capture ordering of `_globals`** that exists to prevent a
`NullReferenceException` when `Cleanup()` runs during the await.

### C3 — `ExecuteMovesAsync` observes no `CancellationToken` — SCOPE AMENDMENT

Seeded text: *"including partial-failure and **mid-batch-cancellation** behavior."*

`EfcHomeController.ExecuteMoves.cs` contains zero `CancellationToken` parameters, zero
`ThrowIfCancellationRequested()` calls, and zero `IsCancellationRequested` reads. The controller
owns `Token`/`TokenSource` and passes them to the data-model factories, but `ExecuteMovesAsync`
neither accepts nor observes a token, and the move seam's signature has no token parameter.

Covering "mid-batch cancellation" would require **both** a production behavior change (adding a
cancellation checkpoint) **and** a breaking change to the seam signature. The first is barred by the
epic NFR "no behavior change to end-user QuickFiler flows"; the second is barred by the additive-only
constraint on the shared EFC surface.

**Disposition:** removed from F8's scope as an explicit scope amendment, and promoted as a separate
follow-up GitHub issue via the MCP promotion lifecycle. It is not silently dropped.

### C4 — `EfcHomeControllerDependencyFactories.cs` is not untested

Seeded text: *"`EfcHomeControllerDependencyFactories.cs` has no dedicated test file."*

True of the file name, false of the effect. Its de-facto dedicated test class is
**`EfcHomeControllerDependenciesTestsProductionFactory`**, which lives in
`QuickFiler.Test/Controllers/EfcHomeControllerDependenciesProductionFactoryTests.cs` (473 lines).
The class name does not match the file name, which is why a name-based search misses the
association. All four of its test methods target members declared in the factories partial, and
**26 of 28 members are already exercised**. The file measures 95.79% line / 100% branch at baseline.

### C5 — The cross-child dependency direction is the reverse of the seeded assumption

Seeded text: *"`EfcHomeControllerDependencies` and `EfcHomeControllerDependencyFactories` form the
injection-seam contract for the whole EFC controller family, including `EfcFormController` and
`EfcItemController`, which belong to sibling child F9."*

A repository-wide grep confirms that `QuickFiler/Controllers/EfcFormController.cs` and
`QuickFiler/Controllers/EfcItemController.cs` reference `EfcHomeControllerDependencies` and every
`Production*` member **zero times**. The dependency runs the other way: **F8's files consume F9's
surface** (the two `EfcFormController` constructors, `Initialize()`, `InitializeWithoutData()`,
`InitializeDataFields(EfcDataModel)`, and the `EfcViewer` type), not the reverse.

The practical consequence is unchanged in one respect — F8 still must not edit F9's files — but the
stated reason is corrected: the risk is not that an F8 contract change breaks F9's consumption; it is
that F8's construction paths depend on F9's constructors and initializers remaining as they are.

## Scope Amendments

| Amendment | Reason | Disposition |
| --- | --- | --- |
| Mid-batch cancellation coverage removed (C3) | Requires a production behavior change plus a breaking seam-signature change; both barred | Promoted as a separate GitHub issue via the MCP promotion lifecycle |
| Partial-failure-mid-batch coverage replaced (C2) | The behavior does not exist in this file | Replaced by exception-propagation / `finally`-reset and pre-await capture coverage |
| Injected-clock requirement for `Timing.cs` retired (C1) | The file reads no clock | Replaced by a branch-coverage >= 75% requirement |

## Cross-Child Contract (F9 boundary)

**Headline verdict: every proposed change is ADDITIVE and TEST-ONLY with respect to the shared EFC
surface — no new overload, property, interface, delegate type, signature change, or removal on
either `EfcHomeControllerDependencies.cs` or `EfcHomeControllerDependencyFactories.cs`. F9 requires
no edit.**

### CCN-1 — five initializer closure bodies (residual; recommendation: no action)

Five one-statement initializer closure bodies in `EfcHomeControllerDependencyFactories.cs`
(lines 80, 92, 105, 125, 128) invoke F9-owned methods:

- L80 / L120 → `EfcFormController.Initialize()`
- L92 / L125 → `EfcFormController.InitializeWithoutData()`
- L105 / L128 → `EfcFormController.InitializeDataFields(EfcDataModel)`

`Initialize()` and `InitializeWithoutData()` both begin with `LoadUserSettings()`
(`EfcFormController.cs` L1009-1022), which reads disk-backed user-scope `Settings.Default` and then
dereferences designer-generated menu-item controls. They are not deterministically reachable under
the repository's unit-test policy.

The minimal F9 edit that would begin to address this is **one line** — `EfcFormController.cs`
line 28, adding `IEfcFormControllerInitialization` to the class declaration; the three method
signatures already match. **Honest caveat: the interface alone does not close the lines.** Those
closures are typed `Func<EfcFormController, EfcFormController>` and would still call the concrete
method; full closure would additionally require changing the `Production*Initializer` property types
to accept the interface, which cascades into the return types of
`CreateProductionFormControllerWithData` (L237) and `CreateProductionFormControllerWithoutData`
(L257). That cascade is a real design change, not a one-line addition.

**Recommendation: leave the approximately five lines uncovered, record the residual, and do not edit
F9.** They are single-statement pass-throughs into `[ExcludeFromCodeCoverage]`-marked F9 members,
and the file clears the 80% floor without them (95.79% at baseline).

### CCN-2 — `QfcExplorerController` construction (informational, non-blocking)

Closing `CreateProductionExplorerControllerInstance` (L149-156) requires constructing
`QfcExplorerController`, which is owned by sibling child **F6**
(`quickfiler-qfc-form-explorer-controller-coverage`). This is **read-only construction inside a
test**; no F6 file is edited. Residual coupling: `QfcExplorerController.cs` L35 calls
`globals.Ol.App.ActiveExplorer()`, so if F6 changes that constructor's signature or removes the
`ActiveExplorer()` call, F8's mock setup needs a one-line update. Not a blocker; does not affect
merge order.

## Scope of Work Per File

Exact uncovered line and branch lists live in the research artifacts and are not restated here.

### `EfcHomeController.cs` (441 lines; 96.85% line / 89.06% branch indicative)

Close, per `EfcHomeController.research.md` § 3: the public three-argument constructor (G1); the
`SetDefaultDependenciesFactory(null)` rejection contract (G3); the `CaptureSelectionSnapshot(null)`
arm reached through `HandleSelectionChangedAsync` (G4); the `Run()` / `RunAsync()` Finder arm where
`_dataModel?.Mail is null && InitType.HasFlag(Find)` (G5) — the Finder flow's `Run` path has never
been exercised; the `StopWatch` getter as a state-transition assertion (G6); the `UiSyncContext`
getter as a propagation assertion (G7); the `ParentCleanup` getter (G8); the `ThrowIfNull`
rejection contracts on the three static entry points (G9); and the constructor-path factory call
order `viewer, keyboard, explorer, form-with-data` (G10).

Also remove the duplicate-default-lambda hazard (G2) — see "Production Edits Proposed".

### `EfcHomeController.ExecuteMoves.cs` (144 lines; 93.16% line / 83.33% branch indicative)

Close the uncovered line set and the three half-covered branches enumerated in
`EfcHomeController.ExecuteMoves.research.md` § 3 and § 6: `ExecuteMovesAsync`'s success path driving
the core and resetting the guard (T1a); **`ExecuteMovesAsync` resetting the guard when the move seam
faults** (T1b — the single most important untested invariant in the file, and the entire reason the
`try/finally` exists); the `HandleMoveResult` fallback to `QuickFileMetrics_WRITE` when no metrics
action is injected (T2); the pre-await `_globals` capture under a `TaskCompletionSource`-controlled
suspension (T3); the pre-await form-option capture (T4, optional hardening); and the
`MoveToFolderAsync` production-fallback arm (T5), whose safety rests on
`EfcDataModel.MoveToFolderAsync` returning `false` on its first statement when `MailInfo` is null, so
no COM object is touched, no store is opened, and no file is written.

### `EfcHomeController.Metrics.cs` (87 lines; 97.59% line / 91.67% branch indicative)

One uncovered line — line 23, the delegation
`QuickFileMetrics_WRITE(filename, selectedFolder, moved, _stopWatch.Elapsed.Seconds)` — and the
line-18 `moved.Count != 0` branch outcome. One test closes both
(`EfcHomeController.Metrics.research.md` § 5, T1). Determinism comes from a never-started
`Stopwatch`, whose `Elapsed` is `TimeSpan.Zero` unconditionally; no timer, sleep, delay, or
wall-clock read is required. T2 (multi-item ordering invariants O1-O3) is optional hardening.

### `EfcHomeController.Timing.cs` (43 lines; 100% line / 66.67% branch indicative)

Line coverage is already complete; the work is branch coverage and assertion quality. Close
`DescribeStartupOverlapState`'s `"correlated"` arm plus both `"unknown"` arms (T1);
`DescribeSynchronizationContext`'s two arms **with the context supplied explicitly by the test**
rather than read from the ambient `SynchronizationContext.Current` (T2); and
`LogFirstSelectionTiming`'s null/whitespace-`details` arm together with the already-prefixed-phase
arm (T3).

Determinism rules specific to this file: assert `Contains("threadId=")` and never an exact managed
thread id; never assert an exact `elapsedMs` value; never depend on the ambient synchronization
context.

### `EfcHomeControllerDependencies.cs` (428 lines; 94.43% line / 93.62% branch indicative)

Close, per `EfcHomeControllerDependencies.research.md` § 3: `LoadSelection`'s null-`globals`
rejection (G-D1); the Outlook-selection path including the `x is MailItem` filter lambda, driven
through the fully-mockable `IApplicationGlobals` → `IOlObjects` → `Application` → `Explorer` →
`Selection` interface chain (G-D2); the empty-selection boundary with a
`Verify(GetEnumerator, Times.Never)` assertion that makes it a real branch test (G-D3); the
single-item boundary (G-D4); the default `MetricsNowFactory` closure body under a bounded-interval
assertion (G-D5, with the rationale recorded in the test comment); and the binding-time invariants
G-D6/G-D7/G-D8.

The binding-time asymmetry is the highest-value item: six of the eleven defaults read their
`Production*` static at **invocation** time, while `AsyncDataModelFactory` (L67) and `ViewerFactory`
(L68) bind **eagerly** at construction. Tests are required on **both** sides of the asymmetry so a
future refactor cannot change it silently.

### `EfcHomeControllerDependencyFactories.cs` (268 lines; 95.79% line / 100% branch indicative)

Close, per `EfcHomeControllerDependencyFactories.research.md` § 4:
`CreateProductionExplorerControllerInstance`, the one uncovered method (G-F1, see CCN-2);
`ResetProductionFactoriesForTesting` asserted as a first-class restoration contract rather than
incidentally executed by `[TestCleanup]` (G-F2), using `.Method.Name` identity checks that never
invoke the defaults; composition-layer ordering and result propagation for the with-data and
without-data form-controller paths and for `CreateProductionDataFields` (G-F3, G-F4, G-F5);
late-binding of the composition layer (G-F6); and the "no memoization" invariant (§ 5 item 2).

G-F7 (the five initializer closure bodies) is **not closable from F8** — see CCN-1.

## Production Edits Proposed

Every edit below is behavior-preserving, confined to F8-owned files, and requires no F9 edit. F8 is
predominantly a test-authoring child; these are the only production changes in scope.

| Edit | File | Net lines | Why it is in scope |
| --- | --- | --- | --- |
| **Required.** Consolidate the two duplicate default-dependency lambdas into a single `private static readonly` default referenced by both the field initializer and `ResetDefaultDependenciesFactory` | `EfcHomeController.cs` L24-25, L37 | +2 to +3 | Removes the coverage-reproducibility hazard (see Constraints). Because F1's harness output is F8's acceptance evidence, reproducibility is itself acceptance-relevant |
| **Recommended.** Widen the four `Timing.cs` helpers from `private static` to `internal static` | `EfcHomeController.Timing.cs` | 0 | `QuickFiler/Properties/AssemblyInfo.cs` already declares `[assembly: InternalsVisibleTo("QuickFiler.Test")]`. Zero runtime behavior change, zero public-API change; replaces brittle reflection with compile-checked access |
| **Recommended.** Extract `BuildFirstSelectionTimingMessage` as a pure function and reduce `LogFirstSelectionTiming` to a single `logger.Debug(...)` call | `EfcHomeController.Timing.cs` | +10 to +14 | The `details` and `phaseLabel` arms currently admit only a "does not throw" assertion because composition and emission are fused. Rejected alternatives: an injectable static log sink (adds mutable global state) and a log4net memory appender (process-global, not isolated) |
| **Optional.** Collapse `ParentCleanup` to an expression-bodied get-only property | `EfcHomeController.cs` L285-290 | −4 | The `private set` has zero in-repo callers and is unreachable. Dead-code removal on an `internal` member, not an API change |
| **Recommended (test-side).** Add `[DoNotParallelize]` to the existing `EfcHomeControllerDependenciesTestsProductionFactory` | test file | +1 | Required once a second static-mutating class exists; see Constraints |

Post-change production sizes remain approximately 441-444 / 428 / 268 / 144 / 87 / 53-57 — all far
below the 500-line ceiling. **The real 500-line risk is test-side**, not production-side.

## Constraints & Risks

### Mandatory test-safety constraints

- **Modal-popup hazard (hard safety rule).** `MoveFailureMessageAction` defaults to
  `text => MessageBox.Show(text)`. Any test that reaches `result == false` **without overriding that
  seam** produces a modal popup and hangs CI. Overriding it is **mandatory in every affected test**,
  including tests that expect success, as defence in depth.
- **Parallelization hazard.** `scripts/vscode/TaskMaster.cli.runsettings` L4-7 sets
  `<Scope>ClassLevel</Scope>` with `<Workers>0</Workers>`, so test **classes** run in parallel. The
  16 `Production*` delegate statics are unsynchronized process-global state, and the existing
  mutating class is not `[DoNotParallelize]`. That is safe today only because it is the sole mutator;
  **a second mutating class makes the flakiness live.** Any new test class that mutates these statics
  MUST be `[DoNotParallelize]` with a `[TestCleanup]` calling
  `EfcHomeControllerDependencies.ResetProductionFactoriesForTesting()`. In-repo precedent:
  `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs` L11. The same rule applies to any
  class mutating `EfcHomeController._defaultDependenciesFactory`, which must reset via
  `ResetDefaultDependenciesFactory()`.
- **Coverage-reproducibility hazard.** `EfcHomeController.cs` installs **two separate default-lambda
  instances with identical bodies** — the field initializer at L24-25 and a distinct lambda in
  `ResetDefaultDependenciesFactory` at L37. Exactly one body is covered in any given run, depending
  on test-class execution order. Per-file coverage for this file is therefore **not reproducible**
  until both sites share a single `static readonly` default.
- **Never invoke these defaults.** `EfcViewerQueue.Dequeue` (constructs a real `EfcViewer` form),
  `EfcDataModel.CreateAsync` (starts a real async Outlook data load), `FileIO2.WriteTextFile`
  (writes to disk), and the three `Production*Initializer` closures (CCN-1). Assert delegate identity
  via `.Method.Name` only.
- **Test conventions.** MSTest, Moq, FluentAssertions, Arrange-Act-Assert; independent, isolated,
  fast, deterministic; no temporary files; no external services; no live or shown WinForms forms; no
  popups; no live Outlook store; no `Thread.Sleep`, `Task.Delay`, or real wall-clock waits.
  Suspension points are controlled with `TaskCompletionSource` only.
- **Seam hierarchy** per `.claude/rules/csharp.md`: interface seam > injectable delegate > adapter.
  Every remaining gap in this file set is reachable with existing seams; no new production seam is
  required for coverage purposes.

### Structural constraints

- No production file may exceed 500 lines, **and the 500-line rule applies to test files too**.
  `QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs` is at 340 lines and the new
  tests will plausibly breach 500. A second test file (for example
  `EfcHomeControllerExecuteMovesStateTests.cs` holding the async state-transition and ordering tests)
  plus extraction of the triplicated reflection helpers (`SetPrivateField` / `SetField`) and the
  triplicated `FakeApplicationGlobals` / `FakeFileSystemFolderPaths` fakes into a shared internal
  test-support class is **required**, not optional.
- Must **not** modify `coverage.config` or any shared build property file.
- Must **not** edit any sibling-owned file, including F9's `EfcFormController.cs` /
  `EfcItemController.cs` / `EfcViewer.cs` and F6's `QfcExplorerController.cs`.
- F1's harness and ledger do not exist on disk at preparation time. The plan consumes them as an
  upstream contract; F1 merges to the integration branch before F8 executes.

## Latent Defects — documented, out of scope for fix

The epic NFR forbids behavior change, so none of the following is fixed in F8. Each was verified by
direct reading or grep, not inferred, and each is promoted to its own GitHub issue via the MCP
promotion lifecycle so it does not disappear at merge.

> **Promotion status (2026-08-07): already done during F8 preparation.** Defects 1-6 below were
> promoted as a single bug workflow to
> [issue #451](https://github.com/drmoisan/TaskMaster/issues/451)
> (`efc-home-controller-metrics-inert-duration`, work mode `full-bug`) before this plan was
> committed, so the record is durable independently of F8's merge. Plan task `[P8-T9]` should
> therefore VERIFY and, if needed, extend #451 rather than open a duplicate issue. The cancellation
> scope amendment (§ Scope Amendments) is the one deferred item still awaiting its own promotion.

1. **`_stopWatch` is never started.** It is constructed at `EfcHomeController.cs` L76 and L225 but
   `.Start()` is never called anywhere in the family — contrast `QfcHomeController.cs` L267-268,
   which does. `Metrics.cs` L23 therefore always reads `_stopWatch.Elapsed.Seconds == 0`: the
   duration metric is inert in production.
2. **`Metrics.cs` L23 uses `.Seconds`, not `.TotalSeconds`** — the 0-59 component, so any duration
   beyond a minute is truncated (90 seconds would report as 30).
3. **`TryBeginExecuteMoves` performs a non-atomic check-then-set** despite `_isExecuting` being
   `volatile`. `volatile` provides visibility, not atomicity. A deterministic unit test cannot prove
   or disprove the race; do not attempt a threading test.
4. **Missing CSV field separator** between `ToRecipientsName` and `SenderName` in the emitted metrics
   line, so the two collapse into one column. The defect is currently **pinned** by an existing
   assertion expecting the concatenated `"RecipientSender"`.
5. **Inconsistent `xComma` sanitization** — applied only to `Subject`, while three other interpolated
   CSV fields are unsanitized. `QfcCollectionController` sanitizes all four.
6. **`QuickFileMetrics_WRITE(string filename)` throws `NotImplementedException`** on a public
   surface. It exists to satisfy an interface obligation; an existing test pins the contract
   deliberately.
7. **Eager-versus-invocation-time binding asymmetry.** Six of eleven factory defaults read their
   `Production*` static at invocation time, while `AsyncDataModelFactory` (L67) and `ViewerFactory`
   (L68) bind eagerly at construction. Real, undocumented, and untested. F8 does **not** change the
   behavior but **does** add tests on both sides so the asymmetry is pinned.

## Evidence and Measurement

- All coverage, QA-gate, and regression evidence for this child is written to
  `<FEATURE>/evidence/qa-gates/` per
  `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. Non-canonical paths
  (`artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, `artifacts/evidence/`) are
  rejected.
- The committed evidence must be F1-harness output produced on F8's branch and must state, per file,
  the line rate and the branch rate.
- The `...-424` figures reproduced above may be cited in the plan as an indicative baseline. They may
  not be committed as F8's acceptance evidence.
- The full C# toolchain order is `csharpier .` → analyzer msbuild → nullable msbuild →
  `vstest.console.exe ... /EnableCodeCoverage`, restarting from step 1 on any failure or auto-fix.

## Acceptance Criteria

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
      promotion lifecycle, with issue numbers recorded here.
- [ ] **AC14 — No behavior change and no sibling edits.** No observable QuickFiler flow changes; every
      production edit is behavior-preserving and confined to F8-owned files; F9 requires no edit; and
      `coverage.config` and all shared build property files are unmodified.
- [ ] **AC15 — Toolchain green.** The full C# toolchain passes in final form in a single pass:
      `csharpier .`, the analyzer msbuild, the nullable msbuild, and coverage-enabled
      `vstest.console.exe`.

## Definition of Done

- [ ] Acceptance criteria above are individually verified and checked off in both `spec.md` and
      `user-story.md`
- [ ] Behavior matches acceptance criteria in all documented environments
- [ ] Tests added/updated per the per-file scope sections
- [ ] Edge cases and error handling covered by tests
- [ ] F1-harness per-file coverage evidence committed under `<FEATURE>/evidence/qa-gates/`
- [ ] Deferred items promoted to GitHub issues and their numbers recorded in AC13
- [ ] Toolchain pass completed (format → analyze → type-check → test)

## Non-Goals

- No behavior change to end-user QuickFiler flows.
- No fix for any latent defect listed above; each is promoted separately.
- No cancellation support in `ExecuteMovesAsync` (C3).
- No edit to any F9- or F6-owned file, and no closure of CCN-1's five residual lines.
- No conversion of the delegate seams to interface seams; that would be a non-additive change to a
  shared surface for zero coverage benefit.
- No change to `coverage.config`, shared build property files, or repository-wide coverage
  thresholds.
