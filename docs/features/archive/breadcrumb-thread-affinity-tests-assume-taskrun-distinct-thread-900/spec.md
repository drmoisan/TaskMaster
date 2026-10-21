# breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread (Spec)

- **Issue:** #900
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-17 (date only: the planning session that applied version 0.3 had no shell clock; the four in-place amendments it made are each marked `amended 2026-09-17 during planning`)
- **Status:** Draft
- **Version:** 0.3

## Context
- Two MSTest tests in `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` assert that a
  boundary guard throws `InvalidOperationException` when a guarded `ItemViewer` member is called from a
  "worker thread." Both tests obtain that worker thread from `Task.Run(...).GetAwaiter().GetResult()`,
  which does not guarantee a thread distinct from the one currently executing.
- Observed environment(s): local and CI test execution under `scripts/vscode/TaskMaster.cli.runsettings`
  (`Workers=0`, `Scope=ClassLevel`), where MSTest 4.4.0 runs every non-`[Timeout]` test body directly on a
  `Task.Run` thread-pool worker (`DefaultFactoryAsync` in `microsoft/testfx` v4.4.0). The same underlying
  mechanism is present under a serial run too (see Root Cause Analysis) but is less likely to be observed
  because more pool workers are idle and available to steal the queued work item.
- Customer impact and severity: developer/CI-facing only. No production behavior is affected. Impact is an
  intermittent, spurious red test that erodes trust in the suite and can mask a genuine regression on a
  retry.
- First observed date and version(s) impacted: reported in issue #900 (2026-09-14 potential entry,
  promoted to issue #900, already OPEN before this run). No specific commit range identified; the tests
  were introduced for issue #781 and have carried this defect since.

## Repro & Evidence
- Steps to reproduce: run `ItemViewerBreadcrumbThreadAffinityTests.InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic`
  or `...ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic` under `Workers=0`/`ClassLevel`
  while the thread pool is not saturated with other work; the `Task.Run` delegate can be "wait-inlined"
  back onto the same `Thread` object that constructed the `ItemViewer` under test, in which case the guard
  never throws and the `.Should().Throw<InvalidOperationException>()` assertion fails.
- Expected vs actual behavior: expected — a genuinely cross-thread call always throws
  `InvalidOperationException` naming the guarded operation. Actual — the call sometimes does not throw at
  all, because it did not, in fact, cross a thread boundary.
- Logs/screenshots/error snippets: none captured yet; a deterministic reproduction of the *original*
  (unfixed) two tests failing is not achievable without mutating process-global `ThreadPool` state (see
  Root Cause Analysis and the plan's fail-before-exception dossier). The mechanism itself is reproduced
  deterministically in the plan's mutation-testing phase (Phase 3 of `plan.2026-09-16T23-27.md`;
  amended 2026-09-17 during planning from "Phase 2") via a guard-disabled run of the *replacement* tests.
- Frequency / determinism: intermittent, decided by a microsecond-scale race between the CLR's local-queue
  pop (inlining) and a remote worker's steal of the same work item. Not data-dependent.

## Scope & Non-Goals
- In scope: rewriting the Act phase of the two named tests in
  `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` so that the "worker thread" is a
  dedicated `System.Threading.Thread` the test itself creates and controls, plus the vacuity/precondition
  proof, the mutation-testing non-vacuity evidence, and the fail-before-exception dossier described below.
- Out of scope / non-goals:
  - The production guard `ItemViewer.ThrowIfOffUiBoundary` (`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:432-447`)
    and its owner-thread-identity design (ratified by issue #781, AC3). This issue does not question or
    change the guard's design intent.
  - `scripts/vscode/TaskMaster.cli.runsettings` (`Workers=0`, `Scope=ClassLevel` stays exactly as-is; see
    the TEST PARALLELISM constraint below).
  - `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` (line 280): also uses
    `Task.Run(...).GetAwaiter().GetResult()`, but asserts `NotThrow()`, which is not defeated by thread
    reuse/inlining the way `Throw()` is (both the inlined and stolen branches reach the same early return
    once `_uiDispatcher` is null). Confirmed not flaky; left unchanged.
  - `BreadcrumbPopupBoundaryCoverageTests.cs:58-61` and `BreadcrumbUiThreadDispatchTests.cs:298-307`, which
    share the same `Task.Run`-as-different-thread assumption against `BreadcrumbUiDispatcher`'s owner-id
    check rather than against `ItemViewer`'s guard. Same defect class, different guard, different file;
    out of scope for #900. To be filed as separate potential entries by the orchestrator through
    `mcp__drm-copilot__new_potential_bug_entry`, from the follow-up handoff record the executor writes
    (see Rollout & Follow-up; amended 2026-09-17 during planning: the earlier wording assigned the
    filing to the executor, but `.claude/skills/feature-promotion-lifecycle/SKILL.md` permits no
    non-MCP route for potential-entry creation and the executor has no MCP tool surface).
  - Explicitly excluded systems/integrations: none (no I/O, no external services involved).

## Root Cause Analysis
- Confirmed root cause (research artifact `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/research/2026-09-16T23-50-breadcrumb-thread-affinity-tests-taskrun-distinct-thread-research.md`,
  Section 2): the failing branch is **wait-inlining**, not idle-thread reuse. `TaskAwaiter.GetResult()` →
  `Task.InternalWait(Timeout.Infinite, default)` attempts to run the target task inline
  (`WrappedTryRunInline`) before blocking. `ThreadPoolTaskScheduler.QueueTask` places a non-`LongRunning`
  `Task.Run` work item on the *calling* thread's own local work-stealing queue when the caller is itself a
  pool worker (`ThreadPoolWorkQueueThreadLocals`); `GetAwaiter().GetResult()` then pops that same item back
  off the local queue and runs the delegate inline **unless another worker steals it first**. When inlined,
  the constructing `Thread` object (captured by `ItemViewer`'s constructor as
  `_uiDispatcher = Dispatcher.CurrentDispatcher`) and the calling `Thread` object are identical, so
  `Dispatcher.CheckAccess()` (`return Thread == Thread.CurrentThread;`, object-identity comparison) returns
  `true`, and the guard never throws.
- Signals/evidence supporting it: `microsoft/referencesource` `TaskAwaiter.cs`, `ThreadPoolTaskScheduler.cs`,
  `threadpool.cs` (quoted in the research artifact, Section 2); `dotnet/wpf`
  `WindowsBase/System/Windows/Threading/Dispatcher.cs` (`CheckAccess`); `microsoft/testfx` v4.4.0
  `TestExecutionManager.cs` (`DefaultFactoryAsync` uses `Task.Run(taskGetter)` for every parallel worker,
  and a non-`[Timeout]` test body runs directly on that worker thread — confirmed neither target test
  carries `[Timeout]`).
- Affected components/modules: test-only. `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`.
  The production guard and its call sites are correct and unaffected.

## Proposed Fix

### Design summary (what changes where)
Replace the `Task.Run(...).GetAwaiter().GetResult()` Act phase in both
`InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic` (currently line 204) and
`ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic` (currently line 237) with a call
through a new private static helper that runs the guarded member on a dedicated `System.Threading.Thread`
created and joined by the test, and captures any exception the delegate throws. A `Thread` object the test
constructs can never be the same `Thread` object that constructed the `ItemViewer` under test — this is
what `Dispatcher.CheckAccess()` compares — so the distinct-thread precondition holds **by construction**,
independent of ThreadPool scheduling, and is correct under `Workers=0`/`ClassLevel` parallel execution by
the same reasoning that makes it correct serially: no other live thread can ever be object-identical to a
brand-new `Thread`.

Candidate approaches considered (full analysis in the research artifact, Section 4):
- **Adopted — dedicated `new Thread` per act.** Sound under any scheduler, matches three in-repo
  precedents (`EmailMoveMonitorTests.cs:281-288`, `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs`
  `ApartmentThreadRunner`, `BayesianPerformanceController.TestSupport.cs`), needs no process-global state,
  no message pump, no sleep or timeout.
- **Rejected — `Task.Factory.StartNew(..., TaskCreationOptions.LongRunning)`.** The default scheduler's
  dedicated-thread behavior for `LongRunning` is an implementation detail described by Microsoft's own docs
  as merely "a hint," not a guaranteed contract; relying on it would trade one undocumented scheduling
  assumption for another, which is the exact failure mode #900 exists to remove.
- **Rejected — retry/tolerance until a distinct thread id is observed.** Prohibited outright by the TEST
  PARALLELISM constraint and the repository's determinism policy (`.claude/rules/general-unit-test.md`);
  retries mask, rather than fix, an assumption the test does not control.

### Boundaries and invariants to preserve
- `scripts/vscode/TaskMaster.cli.runsettings` (`Workers=0`, `Scope=ClassLevel`) is unchanged. The fix must
  be correct under full parallel execution; it must not rely on, or be improved by, running serially.
- The production guard `ThrowIfOffUiBoundary` and its message template
  (`"{operation} must be called on the thread that owns this ItemViewer. The calling thread is not the
  thread the viewer was constructed on."`) are unchanged.
- The existing AC ratified for issue #781 (a guarded member called from a different thread throws
  `InvalidOperationException` naming the operation, and the exception is not `ObjectDisposedException`)
  continues to be exercised — genuinely, this time — by the replacement tests.
- No other test in the file is modified. `InertOperations()`, `ClearViewerDispatcher`, `ViewerScope`,
  `InertDropDownHost`, `DrainableSynchronizationContext` keep their current contracts (research artifact,
  Q4); the new helper is additive.

### Dependencies or blocked work
None. This is a self-contained test-file change with no production dependency.

### Implementation strategy (what changes, not sequencing)

#### Files/modules to change
- `` `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` `` — the only file with a
  persisted code change. No `.csproj` edit is needed (the file is already listed).

#### Functions/classes/CLI commands impacted
- New private static helper, e.g. `RunOnDedicatedWorkerThread(Action action)`, added to
  `ItemViewerBreadcrumbThreadAffinityTests`: starts a `new Thread(...)` with `IsBackground = true`, runs
  `action` inside a `try`/`catch (Exception)` that records the exception into a local, `Join()`s
  synchronously (no timeout — this is a deterministic wait for a fast, dedicated, non-pool thread to finish
  a synchronous call, not a sleep or a wall-clock wait), and returns the captured exception (or `null`).
- `InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic` and
  `ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic`: Act phase rewritten to call the new
  helper. Inside the delegate passed to the helper, first assert the precondition
  (`scope.Viewer.UiDispatcher.CheckAccess().Should().BeFalse(...)`) — the guard's *exact* predicate,
  proving the call is genuinely off the owning thread before invoking the guarded member — then invoke the
  guarded member. Assert on the returned exception: not null, exact type `InvalidOperationException`
  (`BeOfType<InvalidOperationException>()` is an exact-type check in FluentAssertions 8.10.0 and already
  excludes the `InvalidOperationException`-derived `ObjectDisposedException`; keep or drop an explicit
  `NotBeOfType<ObjectDisposedException>()` as documentation — this is an implementation-level choice, not a
  behavioral one), and `Message` contains the guarded operation's name.
- XML doc remarks on both tests updated to describe the dedicated-thread mechanism instead of the
  `Task.Run` assumption, and a short remark on why `Thread.Join()` here is safe: the wait is on the *test's*
  thread for a *dedicated, single-purpose* worker thread to finish a synchronous, bounded call — unlike the
  original `Task.Run(...).GetAwaiter().GetResult()`, no ThreadPool slot is blocked waiting on another
  ThreadPool slot, so there is no shared-pool contention or starvation risk under `Workers=0` parallel
  execution.

#### Data flow and validation changes
None (test-only; no data flow or validation logic changes).

#### Error handling and logging updates
None in production code. The new test helper must not swallow a `null` exception silently: the assertion
that the captured exception is not null must carry an explicit failure reason.

#### Rollback/feature-flag considerations (if applicable)
None; the change is confined to two test bodies and one new private helper, trivially revertible via git.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs and formats
Not applicable (no public API change).

#### Required configuration keys and defaults
None. `scripts/vscode/TaskMaster.cli.runsettings` is read, never written, by this change.

#### Backward-compatibility expectations
Not applicable; test-only change with no consumer.

#### Performance constraints (latency/throughput/memory)
Negligible: two additional OS threads are created and joined per test run, each doing a fast, synchronous
call. No measurable effect on suite runtime.

## Assumptions, Constraints, Dependencies
- Assumptions: `System.Threading.Thread`'s `ManagedThreadId` is unique only among currently-live threads
  (Microsoft Learn); a dedicated `Thread` object is never object-identical to the `Thread` that constructed
  the `ItemViewer` while both are simultaneously live (the constructing thread is alive and blocked in
  `Join()` for the duration of the dedicated thread's run), which is what `Dispatcher.CheckAccess()`
  compares. Both `QuickFiler` and `QuickFiler.Test` target `.NET Framework 4.8.1`
  (`<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>`), and `ObjectDisposedException` derives from
  `InvalidOperationException` on that framework (Object → Exception → SystemException →
  InvalidOperationException → ObjectDisposedException).
- Constraints: no production file may change (item-specific scope constraint); no change to
  `scripts/vscode/TaskMaster.cli.runsettings`; no `Thread.Sleep`, `Task.Delay`, `[Timeout]`,
  `Join(timeout)`, retries, or tolerance of any kind (determinism policy); no temporary files.
- External dependencies: `FluentAssertions` 8.10.0 (pinned, `QuickFiler.Test/packages.config`), `MSTest`
  (`MSTest.TestFramework`/`MSTest.TestAdapter` 4.4.0, pinned), `Moq` (already used in this file). No new
  package.

## Data / API / Config Impact
- User-facing or API changes: none.
- Data or migration considerations: none.
- Logging/telemetry updates: none.
- Compatibility notes: none.

## Test Strategy
- Regression tests to add or update: rewrite the Act phase of
  `InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic` (line 204) and
  `ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic` (line 237) in
  `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`, per the Proposed Fix above.
- Unit tests (MSTest) for the fixed behavior and boundaries: the two rewritten tests themselves are the
  unit tests under repair; no additional test method is required, since the existing pair already covers
  both guarded members named in the issue.
- Edge cases and negative scenarios:
  1. **Fail-before evidence for the *original*, unfixed tests.** A fully deterministic reproduction of the
     original tests' intermittent failure is not achievable without mutating process-global `ThreadPool`
     state (`SetMinThreads`/`SetMaxThreads`), which is itself prohibited in committed tests and would
     perturb every class running in parallel. The plan must therefore record a
     `fail-before-exception.<timestamp>.md` dossier under
     `<FEATURE>/evidence/regression-testing/`, in the shape of the precedent at
     `docs/features/archive/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/regression-testing/fail-before-exception.2026-07-07T14-05.md`
     (`Timestamp:`, `WhyFailingRunImpossible:`, `## Alternative Proof`, `## Output Summary`), citing the
     wait-inlining mechanism chain from the research artifact as the alternative proof.
  2. **Non-vacuity evidence for the *replacement* tests (mandatory, deterministic, test-only).** Before
     finalizing, temporarily insert a call to the existing private helper `ClearViewerDispatcher(scope.Viewer)`
     inside the delegate passed to the dedicated-thread helper, immediately after the precondition
     assertion and immediately before the guarded call, in both rewritten tests
     (amended 2026-09-17 during planning: the earlier wording placed the insertion
     immediately after constructing each `ViewerScope`, which nulls the owning dispatcher before the precondition reads
     `scope.Viewer.UiDispatcher.CheckAccess()` and would fail the precondition with a
     `NullReferenceException` instead of exercising the boundary assertions), so the precondition still
     observes a non-null owner while the guarded call takes the guard's null-owner escape
     (`ItemViewer.Breadcrumb.cs:435-438`) and never reaches the boundary throw. Run the two tests scoped
     and observe that each now **fails** — expected failure is the message assertion
     `captured.Message.Should().Contain(...)` (the actual exception is
     `InvalidOperationException("Breadcrumb UI components must be constructed on an owning UI
     synchronization context.")`, thrown from `BreadcrumbUiDispatcher.CaptureCurrent()`, which
     `InitializeBreadcrumbPipeline` reaches at `ItemViewer.Breadcrumb.cs:80` while building the bridge
     coordinator and `ConfigureBreadcrumbDropDown` reaches through `:241-243`, `:364` and
     `BreadcrumbPopupUiOperations.cs:80-81` — not the boundary-guard message, so the assertion that the
     message contains the operation name fails). Capture this failing run as evidence under
     `<FEATURE>/evidence/regression-testing/`, then remove the temporary `ClearViewerDispatcher(...)` call
     from both tests (git diff must show the file identical to the intended fixed version before the final
     commit) and re-run to confirm both tests pass again. This is a test-only, fully reverted mutation; no
     production file is touched at any point.
  3. Precondition edge case: the dedicated worker thread's delegate must itself assert
     `scope.Viewer.UiDispatcher.CheckAccess()` is `false` before calling the guarded member, so a future
     change that accidentally makes the helper run inline (e.g. a refactor that removes the `Join()` or
     calls `action()` directly) fails loudly at the precondition rather than silently passing or silently
     producing a confusing downstream failure.
- Error handling and logging verification: not applicable (test-only; the helper's own `catch` must not
  swallow an unexpected exception type silently — the type/message assertions below already require
  seeing whatever was captured).
- Coverage impact and targets for changed lines/modules: no production line changes. `ItemViewer` carries
  `[ExcludeFromCodeCoverage]` (`ItemViewer.cs:20`); `QuickFiler.Test` is outside the coverage denominator by
  policy. No coverage delta is expected or required by this change; the plan's final-QC coverage task
  should confirm the repository-wide coverage figure is unchanged (not regressed) rather than target a new
  percentage.
- Toolchain commands to run (format → lint → type-check → test), per CLAUDE.md's C# Toolchain:
  1. `dotnet tool run csharpier format .` (verify: `dotnet tool run csharpier check .`)
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /Settings:scripts/vscode/TaskMaster.cli.runsettings /EnableCodeCoverage`
  Every command task in the plan must record a two-rung channel probe (`pwsh -NoProfile -Command` then
  `pwsh -NoProfile -File`) rather than assuming either shape is available, per the environment notes; both
  were refused under this preparation session's worktree isolation, and that refusal is session-dependent.
- Manual validation steps: none required.

## Acceptance Criteria
- [x] AC1. `InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic` and
  `ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic` in
  `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` no longer obtain their worker thread
  from `Task.Run(...).GetAwaiter().GetResult()`; they use a dedicated `System.Threading.Thread` the test
  creates and joins.
- [x] AC2. Each rewritten test explicitly establishes the distinct-thread precondition
  (`scope.Viewer.UiDispatcher.CheckAccess()` is `false` on the dedicated thread) before asserting the
  boundary diagnostic, so the boundary assertion cannot pass vacuously.
- [x] AC3. Each rewritten test asserts the captured exception is exactly `InvalidOperationException`
  (excluding `ObjectDisposedException`), with a message containing the guarded operation's name
  (`"InitializeBreadcrumbPipeline"` / `"ConfigureBreadcrumbDropDown"` respectively), preserving the AC3
  contract ratified for issue #781.
- [x] AC4. A `fail-before-exception.<timestamp>.md` dossier is recorded under
  `<FEATURE>/evidence/regression-testing/`, documenting why a deterministic failing run of the *original*
  two tests is not achievable, with the wait-inlining mechanism chain as the alternative proof.
- [x] AC5. A deterministic guard-disabled failing run of the two *replacement* tests (via a temporary,
  fully reverted `ClearViewerDispatcher(scope.Viewer)` insertion, no production edit) is captured as
  evidence under `<FEATURE>/evidence/regression-testing/`, proving the new assertions are not vacuous; the
  temporary insertion is confirmed removed (clean `git diff`) before the final pass-after run.
- [x] AC6. Both rewritten tests pass under `scripts/vscode/TaskMaster.cli.runsettings` (`Workers=0`,
  `Scope=ClassLevel`) — i.e. under full parallel execution, not serially — with no change to that
  runsettings file.
- [x] AC7. No sibling test in `ItemViewerBreadcrumbThreadAffinityTests.cs` regresses (all 7
  `[TestMethod]`s in the file pass), and no production file is modified in the final committed diff.
- [x] AC8. Full C# toolchain pass completed in order (CSharpier format → .NET analyzers/EnforceCodeStyleInBuild
  rebuild → nullable/TreatWarningsAsErrors rebuild → vstest with the CLI runsettings), restarting from step
  1 on any failure or file change, with numeric coverage recorded and confirmed not regressed.

## Risks & Mitigations
- Technical risk: a future refactor could reintroduce a `Task.Run`-based "different thread" assumption
  elsewhere. Mitigation: the new helper and its precondition assertion are self-documenting (XML remarks +
  inline comment explaining why `Join()` is safe here), and the research artifact identifies two sibling
  call sites (`BreadcrumbPopupBoundaryCoverageTests.cs:58-61`,
  `BreadcrumbUiThreadDispatchTests.cs:298-307`) with the same latent assumption, to be filed as separate
  potential entries rather than silently left for a future incident.
- Operational risk: none (test-only, no rollout).
- Mitigations and rollbacks: the change is two test methods plus one private helper; revertible via git
  with no downstream impact.

## Rollout & Follow-up
- Release/rollout steps: normal PR merge; no feature flag, no migration.
- Post-fix monitoring or clean-up tasks: the orchestrator files separate potential entries through
  `mcp__drm-copilot__new_potential_bug_entry` (not fixed under #900), from the follow-up handoff record
  the executor writes (amended 2026-09-17 during planning; see Scope & Non-Goals), for:
  (a) `BreadcrumbPopupBoundaryCoverageTests.cs:58-61` and `BreadcrumbUiThreadDispatchTests.cs:298-307`,
  which share the `Task.Run`-as-different-thread assumption against `BreadcrumbUiDispatcher`'s owner-id
  check; (b) the discrimination-claim remark on `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow`
  (lines 273-277), which only fully discriminates against the pre-#781 guard in the "stolen" branch, per
  research artifact Q4.
- Links: issue #900; research artifact
  `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/research/2026-09-16T23-50-breadcrumb-thread-affinity-tests-taskrun-distinct-thread-research.md`;
  prior issue #781 (`docs/features/potential/promoted/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers.md`,
  `docs/features/active/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781/`).
