# 2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll (Spec)

- **Issue:** #948
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-10-01T12-30
- **Status:** Ready for planning
- **Version:** 1.2 (planner amendments: version 1.1 admitted the coverage runner's own inner collector invocation, with the four known local shell-icon test classes excluded, when the plan's baseline stall probe reproduces the local shell-icon failure (plan decision D-9); version 1.2 reworded AC-L, the landing-order sentence under Dependencies, the catch-count invariant row, the merge-conflict mitigation and the Rollout constraint so that the sibling throwing-sink fix landing first is the expected state: this fix adds no try, catch or finally beyond the merge base, keeps the sibling's sink guard, and places the record immediately after the sink call inside that guard (plan decision D-14))
- **Work Mode:** full-bug. This file is the sole authoritative acceptance-criteria source. No user-story.md exists for this feature, by design.

> Change-footprint note (do not "fix" this formatting): the only repository paths written as inline code spans in this document are the three files the fix creates or modifies and the evidence artifacts under this feature folder. Every other file is cited in bare prose so that the backtick-harvesting footprint tool does not read it as a write target.

## Context

The Spam and Triage engine toggles on the ribbon answer Office's synchronous `getPressed` poll from a cache inside `EngineToggleStateCoordinator`. On a cache miss the coordinator starts one asynchronous prime per engine key; the prime awaits `IAppItemEngines.EngineActiveAsync`, which in production awaits the classifier configuration held in an `AsyncLazy` on `ManagerAsyncLazy`. `AsyncLazy` wraps a `Lazy` of a task, so once the configuration load faults the same faulted task is re-awaited by every later call for the lifetime of the lazy, and nothing in production ever resets it (the only reset sites are the `ManagerAsyncLazy` constructor and a null-guarded branch that cannot run after construction).

A failed prime leaves the cache unset, reports the failure through the injected `logError` sink, and clears its registration marker so a later poll may re-prime. Against a permanently faulted configuration that sequence repeats on every cache-miss poll: re-prime, synchronous rethrow, one error log entry, marker cleared, next poll. Before issue #944 a stale marker intermittently blocked re-primes and so masked the repetition; #944 removed the stale marker, and now one error entry is written per completed prime cycle without bound.

Environment:
- OS/version: Windows 11 (Outlook VSTO add-in)
- Runtime: C#, .NET Framework 4.8 (net481), legacy non-SDK project files
- Command/flags used: n/a (ribbon polling drives the path)
- Data source or fixture: the production coordinator in the TaskMaster Ribbon folder; the configuration `AsyncLazy` owned by ManagerAsyncLazy in UtilitiesCS with its `ResetConfigAsyncLazy` method

Impact / Severity:
- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

## Repro & Evidence

Steps to Reproduce (production):
1. Cause the engine configuration load to fault persistently (for example, make the classifier configuration unreadable so `ReadConfiguration` throws).
2. Let the ribbon repeatedly invalidate or poll the engine toggle `getPressed` callback.
3. Observe the add-in log file under the TaskMaster bin Debug logs folder (one file per date).

Expected (from the issue):
A permanent configuration fault is logged once, or at a bounded rate, and recovery is either explicit or backed off.

Actual:
After #944, one "Reading the activation state for engine '...' failed" entry is logged per cache-miss poll, without bound.

Deterministic reproduction (test harness): wire the coordinator to a strict `Mock<IAppItemEngines>` whose `EngineActiveAsync` returns one already-faulted task instance (`Task.FromException<bool>(failure)`), which models the cached `AsyncLazy` fault exactly. Each poll is `GetPressed` followed by awaiting `GetPrimeTask`. Five such polls produce five entries in the harness `Errors` list on the unchanged production file, decided by program order rather than by scheduling. This is the fail-before observation that the regression test in Test Strategy carries.

Evidence trail:
- Research record (not a write target, so cited in prose): the file 2026-10-01T07-20-engine-toggle-permanent-config-fault-logs-every-poll-research.md under this feature folder's research subfolder (sections 1.1 to 1.5 for the mechanism, section 7 for the test mechanics).
- Prior art: the #944 spec's Rollout and Follow-up item 2 and the #944 research follow-up item 2 both name this behaviour as the next defect.

## Scope & Non-Goals

- In scope:
  - A repeat-report suppression policy in `CompletePrime` of `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`: a prime failure is reported through `logError` once per (engine key, base-exception type) pair; later failures of an already-reported pair for the same key are not reported again. Re-priming is unchanged, so recovery stays automatic.
  - One appended sentence in the prime-failure message stating that further failures of this kind for this engine are not logged again.
  - XML documentation updates on `CompletePrime`, the new field, and the `GetPrimeTask` return contract to describe the suppressed case.
  - A new test partial `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs`, registered as an explicit compile item in `TaskMaster.Test/TaskMaster.Test.csproj`.
  - Evidence projections (Markdown only) for the fail-before run, the pass-after run, and the final toolchain pass.

- Out of scope / non-goals (paths deliberately unbackticked):
  - The sibling defect in which a throwing `logError` sink leaves a stale prime marker (issue 947) is out of scope. Its behaviour must not change: a throwing sink on the first report still leaves the marker registered exactly as today, and this fix adds no try, catch, or finally to `CompletePrime`.
  - No change to `StartPrimeIfNeeded`, `StartObservedPrime`, `ApplyPrimeAsync`, `GetPressed`, `ExecuteToggleAsync`, `HandleToggleClickAsync` (the toggle path), the coordinator constructor, or RibbonController.EngineCommands.cs in the TaskMaster Ribbon folder.
  - No time-based back-off and no clock or `TimeProvider` injection. The design reads no clock, so the C# rules' time seam is not engaged.
  - No new package dependency and no project-file change beyond the one compile-item registration.
  - No explicit reset or "clear suppression" API on the coordinator, and no production caller of `ResetConfigAsyncLazy`.
  - No change to the pressed-state cache type (EngineTogglePressedStateCache.cs in the TaskMaster Ribbon folder), to `AsyncLazy`, to ManagerAsyncLazy, or to AppItemEngines.
  - No edit to the four existing test partials (the primary fixture file, the Race partial, the PrimeFaultOrdering partial, and the PrimeRegistration partial under the TaskMaster.Test Ribbon folder). The stale remark in the Race partial is recorded under Rollout & Follow-up rather than edited here.
  - No lowering of the report level (for example, debug instead of error) for repeats; the sink is a single delegate and a level parameter would change the constructor and the production wiring.

- Explicitly excluded systems, integrations, or datasets: Outlook, the Office ribbon runtime, the on-disk classifier configuration, and the add-in log file. The fix is host-neutral and is verified entirely through the injected delegates.

## Root Cause Analysis

Two properties combine to produce the unbounded log volume:

1. **The fault is cached upstream.** `ManagerAsyncLazy.Configuration` is an `AsyncLazy` whose underlying `Lazy` stores the first task produced by `Task.Run(ReadConfiguration)`. When that task faults, every later `await Configuration` re-awaits the same faulted task and rethrows synchronously. `AppItemEngines.EngineActiveAsync` therefore returns an already-faulted task on every call, and no production code ever calls `ResetConfigAsyncLazy` after construction. The coordinator cannot reach the reset because `IAppItemEngines` does not expose it.

2. **The coordinator re-primes on every cache miss and reports on every completed prime.** `GetPressed` on a cache miss calls `StartPrimeIfNeeded`; the at-most-one-prime guard only coalesces polls that arrive while a marker is registered. `CompletePrime` reports through `logError` and then removes the marker. With a synchronously faulted task the whole cycle completes before the next poll, so each poll is a fresh prime and a fresh report. The pressed-state cache gains an entry only on success, so the key stays a cache miss indefinitely.

This is a missing policy rather than an ordering defect: #944 made the re-prime reliable (which is correct and is what makes recovery possible), and in doing so removed the accidental suppression a stale marker had provided. The issue's own assessment stands: a design decision about the retry and reporting policy, not a correctness defect in #944.

A per-key-only suppression was considered and rejected because a transient first fault is structurally possible: the globals constructor creates `AppItemEngines` before `AppAutoFileObjects.Manager` is assigned, so an early prime can fault with `NullReferenceException` before the configuration fault ever appears. A flag keyed on the engine name alone would then hide the later, permanent configuration fault. Keying on the base-exception type as well logs each distinct failure kind once.

## Proposed Fix

### Design summary (what changes where):

Policy (a) from the research record: suppress repeat prime-failure reports per (engine key, base-exception type) and keep re-priming.

**Invariant (one sentence):** for a given engine key and a given base-exception type, `logError` is invoked at most once per coordinator lifetime for prime failures, every prime failure still clears its marker so the next cache-miss poll re-primes, and the (key, type) pair is recorded only after the sink has returned.

Changes, all in `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`:

1. **New field** next to `_primeTasks`: a `ConcurrentDictionary` keyed by the value tuple `(string EngineName, Type FaultType)` with a `byte` value, named `_reportedPrimeFaults`. Its summary states: prime failures already reported, keyed by engine and base-exception type; presence means a later failure of the same kind for the same key is not reported again; never cleared, because a key that gains a cached value never primes again (issue #948). `System` and `System.Collections.Concurrent` are already imported; `ValueTuple` is available on net481 without an extra reference.
2. **`CompletePrime`**: keep the ran-to-completion early return and the `failure` computation as they are. Replace the unconditional report with a guarded block:
   - compute the report key as the tuple of `engineName` and `failure.GetType()`;
   - if `_reportedPrimeFaults` does not contain that key: call `_logError(BuildPrimeFailedMessage(engineName), failure)`, then record the key through the indexer (so no discarded `TryAdd` result exists for an analyzer to flag);
   - keep `_primeTasks.TryRemove(engineName, out _)` as the last statement.
   The record is placed after the sink returns, deliberately: if the sink throws (the sibling issue), the pair is not recorded and the report is still owed.
3. **`BuildPrimeFailedMessage`**: append one sentence to the existing text: "Further failures of this kind for this engine are not logged again." Every existing assertion on this message is a `Contain(SpamEngine)` check, so the text change breaks no test.
4. **Documentation**: the `CompletePrime` summary and remarks gain the suppression rule, the reason the record follows the report, and two constraints the sibling fix must respect (the record stays after the sink returns, and the record must not move into a finally). The `GetPrimeTask` return contract is reworded so that a caller receiving `Task.CompletedTask` can rely on the fault having been reported, or deliberately suppressed as a repeat of a failure kind already reported for that key. The "Report-then-clear is load-bearing" comment gains "(if any)" after "report".

Expected size: the file is 442 lines today and grows by roughly twenty to twenty-five lines, below the five-hundred-line ceiling.

**Trace of one accepted value through the current and fixed code** (the path that has no guard between the poll and the sink):

1. Accept point: `GetPressed("Spam")` maps the key, misses the cache, and calls `StartPrimeIfNeeded`. The guard there checks only engines availability and marker presence; it does not consult any prior failure, so the poll proceeds.
2. Throw point: `ApplyPrimeAsync` awaits `engines.EngineActiveAsync("Spam")`, which returns the cached faulted task; the await rethrows and the prime task faults. `ApplyPrimeAsync` has no catch, by design.
3. Current absorption point: the continuation in `StartObservedPrime` runs `CompletePrime`, which unwraps the base exception, calls `_logError` unconditionally, then removes the marker. There is no memory of a prior report, so the next poll repeats steps 1 to 3 and produces another log entry.
4. Fixed behaviour: `CompletePrime` computes the (key, type) pair; on the first cycle it is absent, so the sink is called and the pair is recorded after the sink returns; on every later cycle the pair is present, the sink is skipped, and the marker is still removed. Step 1 is unchanged, so a later successful read (if the configuration ever loads) still caches the value and invalidates the control once.

Why neither half suffices alone: suppressing without re-priming (policy (c)) freezes the toggle at unchecked for the session after a transient fault and fails four existing tests; re-priming without suppression is the current, unbounded behaviour.

### Boundaries and invariants to preserve:

| Invariant | Preserved because | Pinned by (existing tests, unchanged) |
|---|---|---|
| At most one prime per key; registration precedes start | `StartPrimeIfNeeded` and `StartObservedPrime` are untouched | `GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime`; `GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns` |
| Report-then-clear in `CompletePrime` | the guarded report (or its deliberate skip) still precedes `TryRemove`, which stays the last statement | `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged` |
| `GetPrimeTask` never faults; `Task.CompletedTask` implies the report has returned or was deliberately suppressed | the marker completes only in the continuation's finally after `CompletePrime` exits; contract reworded | the same test; `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` |
| A failed or canceled prime clears its marker so a later read re-primes | suppression never prevents `TryRemove` | `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker`; `GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime`; `GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime` |
| No catch, try or finally beyond the merge base (the click boundary, plus the sibling throwing-sink fix's sink guards once that fix has merged) | this fix adds no catch, try or finally to `CompletePrime`; a sink guard already present at the merge base is kept as it is | `ExecuteToggleAsync_WhenToggleFaults_PropagatesUnchanged` (the toggle core must keep propagating; its caller's catch must not be widened or duplicated) |
| No lock in `CompletePrime`; no await under `_primeGate` | the new logic is two `ConcurrentDictionary` operations | structural |
| Toggle-path faults are reported on every click | `HandleToggleClickAsync` is untouched and does not consult `_reportedPrimeFaults` | `HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate` plus new test F below |

Why the check-then-record needs no lock: for one key, `CompletePrime` runs once per prime, and the next prime for that key cannot be registered until the `TryRemove` at the end of the current `CompletePrime` has run, because `StartPrimeIfNeeded` refuses while the marker is present. The record precedes `TryRemove`, so two `CompletePrime` invocations for the same key are strictly ordered. Different keys touch different dictionary entries, and the toggle path never touches the dictionary.

### Dependencies or blocked work:

- Not blocked. Builds on #944 (merged; the registration-before-start fix is what makes the re-prime reliable).
- Interacts with the sibling throwing-sink issue (947), which edits the same failure branch of `CompletePrime`. Landing order as executed: the sibling merges first, and this fix reconciles with it at its plan's Phase 0, keeping the sibling's sink guard and placing the record immediately after the sink call inside that guard. See Rollout & Follow-up for the two constraints that placement respects.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

- `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (modify: field, `CompletePrime`, `BuildPrimeFailedMessage`, XML docs).
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs` (create: fifth partial of the existing fixture).
- `TaskMaster.Test/TaskMaster.Test.csproj` (modify: one new compile item adjacent to the PrimeRegistration partial entry; the test project is a legacy non-SDK project, so a file without an explicit compile item does not compile and its tests silently do not exist).

#### Functions/classes/CLI commands impacted:

- `EngineToggleStateCoordinator.CompletePrime` (private): guarded report and record.
- `EngineToggleStateCoordinator.BuildPrimeFailedMessage` (private static): appended sentence.
- `EngineToggleStateCoordinator.GetPrimeTask` (internal): documentation only; code unchanged.
- New private readonly field `_reportedPrimeFaults`.
- No CLI, ribbon XML, or public API change.

#### Data flow and validation changes:

- State model per engine key: absent (never primed) to priming (marker registered); on success, cached (permanent, marker retained and complete); on failure, absent again with the (key, fault type) pair added to the reported set. The reported set only grows and is bounded by the number of mapped keys (two) times the number of distinct base-exception types a prime can produce.
- No input validation changes: null or whitespace keys never reach `CompletePrime` because `GetPressed` rejects them first.

#### Error handling and logging updates:

- The first prime failure of each (key, type) is reported with the full exception, exactly as today, plus the appended sentence. Repeats of an already-reported pair are not reported. A canceled prime synthesizes a fresh `TaskCanceledException` per cycle, which is one type and is therefore also reported once.
- The toggle path's reporting is unchanged: a toggle fault is logged on every click (user-driven and bounded by clicks).
- Nothing is swallowed: the fault is still observed (reading `Task.Exception` in `CompletePrime` marks it observed) and the marker is still cleared. This conforms to the fail-fast rule because the suppression removes a duplicate of an already-delivered report, not the report itself.

#### Rollback/feature-flag considerations (if applicable):

- No feature flag. Rollback is a revert of the single production file plus removal of the test partial and its compile item.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

- Inputs: unchanged (engine key strings "Spam" and "Triage"; the injected `logError` delegate).
- Output: the prime-failure message becomes "Reading the activation state for engine '{0}' failed; its toggle continues to report unchecked. Further failures of this kind for this engine are not logged again." The toggle-failure and unavailable messages are unchanged.

#### Required configuration keys and defaults:

- None.

#### Backward-compatibility expectations:

- The constructor signature, `GetPressed`, `HandleToggleClickAsync`, `ExecuteToggleAsync`, and `GetPrimeTask` signatures are unchanged. The production wiring in RibbonController.EngineCommands.cs compiles unchanged.
- Log consumers that match on the leading "Reading the activation state for engine" text continue to match; the sentence is appended, not inserted.

#### Performance constraints (latency/throughput/memory):

- Per failed prime: one `ConcurrentDictionary` probe and, on the first occurrence only, one insert. Memory is bounded by (mapped keys) times (distinct failure types). A re-prime against a cached fault remains one dictionary probe plus one synchronous rethrow; the re-prime cost itself is not changed by this fix and is not the subject of the issue.

## Assumptions, Constraints, Dependencies

- Assumptions (environment, data, access):
  - The cached `AsyncLazy` fault always carries the same exception instance and therefore the same base-exception type, so the production scenario in the issue yields exactly one entry per key. Verified by reading `AsyncLazy` and ManagerAsyncLazy in this worktree (research section 1.1).
  - The pressed-state cache has no remove or clear member, so once a key has a cached value no further prime, and no further prime fault, can occur for it. Any "reset suppression on success" logic would be unobservable and is therefore omitted (research section 1.5).
  - Visual Studio Build Tools with the Test Platform and the `dotnet-coverage` tool are present on the build machine, as they were for the #944 run.
- Constraints (budget, performance, compatibility):
  - net481: no `init` accessors or record structs; the value-tuple dictionary key is the chosen shape.
  - The production file carries no `#nullable enable` directive, so it is outside the nullable-as-error set; the new field must still pass the analyzer rebuild.
  - The primary test fixture file is 470 lines and must not grow; the new tests live in a new partial.
  - MSTest, Moq (strict harness), FluentAssertions; no sleep, delay, timer, wall clock, temporary file, parallelism attribute, or worker-count change in any new test.
- External dependencies (services, libraries, releases):
  - None added. `Microsoft.Bcl.TimeProvider` and `Microsoft.Extensions.TimeProvider.Testing` are already referenced but are not used by this fix.

## Data / API / Config Impact

- User-facing or API changes: none on the ribbon. The add-in log receives one prime-failure entry per (engine key, failure kind) instead of one per poll, and that entry states the suppression.
- Data or migration considerations: none.
- Logging/telemetry updates (if any): the appended sentence in the prime-failure message; no new log level, sink, or category.
- Compatibility notes (CLI flags, config schemas, versioning): none.

## Test Strategy

Policy decision carried from the issue's "Proposed Fix / Validation Ideas": suppress repeat reports per (engine key, failure kind) and keep re-priming (policy (a)); no `TimeProvider` or fake clock is required because the design reads no clock, so the issue's "under a fake TimeProvider" idea is replaced by a program-order test against an already-faulted task.

- Regression tests to add or update:
  - New partial `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs`, registered in `TaskMaster.Test/TaskMaster.Test.csproj`. Reuses the private `Harness`, `LoggedError`, `SpamEngine`, and `SpamToggleControlId` members of the primary fixture; adds a local `private const string TriageEngine = "Triage";` for test E. Estimated 170 to 220 lines.
  - Shared mechanics: `harness.Engines.Setup(x => x.EngineActiveAsync(SpamEngine)).Returns(faulted)` where `faulted` is one `Task.FromException<bool>(failure)` instance; each poll is `harness.Coordinator.GetPressed(SpamEngine)` followed by `await harness.Coordinator.GetPrimeTask(SpamEngine)`. After the await the marker has been removed, so the next `GetPressed` starts a new prime and `EngineActiveAsync` is called exactly once per poll. Cycles are sequential, so `Errors` is never mutated concurrently.
  - Tests (MSTest `[TestMethod]`, Arrange-Act-Assert, FluentAssertions, Moq strict):
    - A. `GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly` (carries the fail-before obligation): five polls against one faulted task; assert `Errors` contains a single entry whose `Exception` is the same instance as `failure` and whose `Message` contains `SpamEngine`; verify `EngineActiveAsync(SpamEngine)` was called exactly five times; `GetPressed(SpamEngine)` is false; `Invalidations` is empty. On the unchanged production file `Errors` has five entries, so the first assertion fails deterministically.
    - B. `GetPressed_WhenPrimeIsCanceledRepeatedly_LogsFirstCancellationOnly`: same shape with `Task.FromCanceled<bool>(new CancellationToken(true))`; assert one error assignable to `OperationCanceledException` and five activation reads.
    - C. `GetPressed_AfterSuppressedFaults_LaterSuccessfulPrimeCachesStateAndInvalidatesOnce` (recovery): `SetupSequence` faulted, faulted, then `Task.FromResult(true)`; three polls; assert a single error, `GetPressed(SpamEngine)` true, `Invalidations` equal to exactly `[SpamToggleControlId]`, three activation reads. A fourth read cannot occur because the post-success read is a cache hit.
    - D. `GetPressed_WhenFailureKindChanges_LogsNewKindOnce`: `SetupSequence` with an `InvalidOperationException` task twice, then an `IOException` task twice; four polls; assert two errors whose exceptions are the two injected instances in order, four activation reads.
    - E. `GetPressed_WhenSpamFaultIsSuppressed_FirstTriageFaultIsStillLogged`: two Spam polls, then one Triage poll with its own faulted setup; assert two errors whose messages contain `SpamEngine` and `TriageEngine` respectively.
    - F. `HandleToggleClickAsync_AfterSuppressedPrimeFault_StillLogsToggleFault`: two Spam polls (one prime report), then `ToggleEngineAsync(SpamEngine)` set up to throw and `await HandleToggleClickAsync(SpamEngine)`; assert two errors with the second's exception the same instance as the toggle failure.
    - G. `GetPressed_WhenPrimeFaults_FirstReportStatesRepeatsAreNotLoggedAgain`: one faulted poll; assert the single error's message contains "not logged again".
  - Scenario coverage: positive (C), negative/error (A, B, G), edge (D, E), boundary interaction (F), concurrency (sequential by construction; the invariant tests in the Proposed Fix table remain the concurrency pins). State transitions: absent to priming to absent-with-record (A, B), then to cached (C).
- Unit tests (MSTest) for the fixed behavior and boundaries: the seven tests above; all existing coordinator tests (four partials) run unchanged and remain green. No existing test asserts that a second prime failure for the same key is logged again; the three tests that re-prime after a failure assert the single error before the re-prime or follow the failure with a success.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values): null, empty, and whitespace keys are rejected by `GetPressed` before any prime (existing data-row test); an unmapped key starts no prime (existing test); a canceled prime is one failure kind (B); a different failure kind for the same key is reported once more (D); per-key independence (E).
- Error handling and logging verification: A, B, D, E, G verify the sink; F verifies the toggle path still reports every click; the existing PrimeFaultOrdering test verifies report-then-clear on the first (reported) failure.
- Coverage impact and targets for changed lines/modules: the changed lines in `CompletePrime` are exercised on both branches (report taken by A's first cycle; report skipped by A's later cycles). The new field and the message change are exercised by every failure test. Target ninety percent or better on the coordinator file and full coverage of the changed lines; the repository-wide figure is recorded in the coverage projection as record-and-report against the testable denominator defined in CLAUDE.md, and this change must not lower it.
- Toolchain commands to run (format, lint, type-check, test), from CLAUDE.md, in this order, restarting from the first on any failure or file change:
  1. `dotnet tool restore` once per worktree, then `dotnet tool run csharpier format .` and `dotnet tool run csharpier check .`
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. The MSTest-with-coverage route (the Invoke-MSTestWithCoverage script under scripts/vscode or the matching VS Code task). The script accepts no test filter; a scoped `-SearchRoot TaskMaster.Test` run skips the solution-wide threshold assertions and warns, an unscoped run enforces them. When the plan's baseline stall probe observes the known local shell-icon test failure or stall in UtilitiesCS.Test, the route is the script's own inner collector invocation with those four shell-icon test classes excluded and the vstest hang-blame switch appended, post-processed by the script's own helpers and floor checks, and the toolchain projection records the route taken and the probe result.
  - Per-class runs for the fail-before and pass-after evidence: direct vstest.console.exe on the TaskMaster.Test Debug assembly with the TaskMaster.cli.runsettings file, the InIsolation switch, a TestCaseFilter switch whose value is FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests, an explicit results directory, and an explicit TRX log file name (so the TRX name is predictable and host-path-free), as the #944 baseline did. The Tests switch and the TestCaseFilter switch are mutually exclusive on the vstest command line; use the filter form.
- Evidence (Markdown projections only, per the Committed Test Evidence Format in CLAUDE.md; no raw TRX, Cobertura, or .coverage file is added to the repository). Fixed filenames; the run timestamp goes in each artifact's Timestamp field:
  - Baseline (coordinator class on the unchanged branch head): `docs/features/active/2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll-948/evidence/baseline/coordinator-tests-baseline.md`
  - Fail-before (new partial compiled against the unchanged production file; test A fails, the `Errors` count observed is recorded): `docs/features/active/2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll-948/evidence/regression-testing/repeat-fault-suppression-fail-before.md`
  - Pass-after (same filter after the fix; all coordinator tests pass): `docs/features/active/2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll-948/evidence/regression-testing/repeat-fault-suppression-pass-after.md`
  - Final toolchain pass (the four commands, exit codes, and the MSBuild non-vacuity check that no project reported a skipped CoreCompile): `docs/features/active/2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll-948/evidence/qa-gates/toolchain-final-pass.md`
  - Coverage projection (package-level JaCoCo projection plus the one-line first-party summary, with the coordinator file's line and branch figures and the changed-line figure called out): `docs/features/active/2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll-948/evidence/qa-gates/coverage-projection.md`
- Manual validation steps (if required): none required. Optional operator check after release: with the classifier configuration made unreadable, open the ribbon and confirm the debug log shows one prime-failure entry per engine key, each ending with the appended sentence, and that the toggle re-checks itself once the configuration becomes readable and a poll occurs.

## Acceptance Criteria

Every criterion below is proved by the named test or command. No criterion line contains a digit; counts are written as words. "The regression partial" means the new RepeatFaultSuppression test partial named in Test Strategy; "the production file" means the coordinator source file named in Scope.

- [ ] AC-A. Repeated faulted polls report once while every poll still re-primes: `GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly` in the regression partial passes, asserting a single logged error whose exception is the injected instance, an activation-read count equal to the poll count, `GetPressed` returning false, and no invalidation.
- [ ] AC-B. Repeated canceled primes report once: `GetPressed_WhenPrimeIsCanceledRepeatedly_LogsFirstCancellationOnly` passes, asserting a single logged error assignable to `OperationCanceledException` and an activation-read count equal to the poll count.
- [ ] AC-C. A later successful prime recovers: `GetPressed_AfterSuppressedFaults_LaterSuccessfulPrimeCachesStateAndInvalidatesOnce` passes, asserting a single logged error, `GetPressed` returning true after the success, and an invalidation list equal to exactly the Spam toggle control id.
- [ ] AC-D. A different failure kind for the same key is reported once more: `GetPressed_WhenFailureKindChanges_LogsNewKindOnce` passes, asserting two logged errors carrying the two injected exception instances in order.
- [ ] AC-E. Suppression is per key: `GetPressed_WhenSpamFaultIsSuppressed_FirstTriageFaultIsStillLogged` passes, asserting two logged errors whose messages name the Spam and Triage engines respectively.
- [ ] AC-F. Toggle-path faults are still reported on every click: `HandleToggleClickAsync_AfterSuppressedPrimeFault_StillLogsToggleFault` passes, asserting that the toggle fault is logged after a suppressed prime fault, and the existing `HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate` still passes unchanged.
- [ ] AC-G. The first prime-failure message states that repeats are not logged again: `GetPressed_WhenPrimeFaults_FirstReportStatesRepeatsAreNotLoggedAgain` passes, asserting the single logged message contains the phrase "not logged again", and the message text in `BuildPrimeFailedMessage` ends with the sentence quoted in the Proposed Fix.
- [ ] AC-H. Fail-before is demonstrated: with the regression partial compiled against the unchanged production file, a per-class run shows `GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly` failing on its single-error assertion, and the Markdown fail-before projection named in Test Strategy records the command, exit code, failing test name, and observed error count under the feature's regression-testing evidence folder.
- [ ] AC-I. Pass-after is demonstrated: the same per-class run after the fix shows every test in the coordinator fixture passing, recorded in the Markdown pass-after projection named in Test Strategy under the feature's regression-testing evidence folder.
- [ ] AC-J. All existing coordinator tests pass unchanged: the four existing test partials are byte-identical to the branch base (a diff of each against the merge base is empty) and every test in them passes in the pass-after run.
- [ ] AC-K. The invariants are preserved: `GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime`, `GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns`, `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged`, and `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker` pass unchanged, and in the production file the `TryRemove` call remains the last statement of `CompletePrime` with the guarded report block placed before it.
- [ ] AC-L. The throwing-sink behaviour is unchanged: the `CompletePrime` body contains no try, catch, or finally keyword beyond those already present at the merge base (the sibling throwing-sink fix's sink guard, when merged, is kept as it is), the count of catch keywords on code lines of the production file equals its merge-base count, and the record into the reported-faults dictionary is the statement immediately after the `_logError` call, inside the guarded block and inside any pre-existing sink guard, not before the call and not in a catch or finally.
- [ ] AC-M. Scope is held: the diff against the merge base touches only the production file, the regression partial, the test project file (one added compile item), and Markdown files under the feature folder; `StartPrimeIfNeeded`, `StartObservedPrime`, `ApplyPrimeAsync`, `GetPressed`, the toggle path, and the constructor are textually unchanged; no package manifest changes; no file with an xml, trx, or coverage extension is added.
- [ ] AC-N. The full C# toolchain from CLAUDE.md passes in one final pass in order: csharpier format then check, the analyzer Rebuild, the nullable Rebuild, and the MSTest-with-coverage route, with exit codes and the no-skipped-CoreCompile check recorded in the Markdown toolchain projection named in Test Strategy under the feature's qa-gates evidence folder. When the plan's baseline stall probe observes the known local shell-icon test failure or stall, the MSTest-with-coverage route is the coverage script's own inner collector invocation with those four shell-icon test classes excluded and the vstest hang-blame switch appended, post-processed by the script's own helpers and floor checks, and the toolchain projection records that route and the probe result.
- [ ] AC-O. Coverage: every changed line in `CompletePrime` and `BuildPrimeFailedMessage` is covered on both branches of the new guard, the coordinator file reports line coverage of at least ninety percent in the Markdown coverage projection named in Test Strategy, and the repository-wide first-party figures are recorded there against the testable denominator with a statement that this change does not lower them.
- [ ] AC-P. File-size ceiling: the production file and the regression partial each remain under five hundred lines, measured by a line count of each file after formatting; the primary fixture file is unchanged in length.

## Risks & Mitigations

- Technical or operational risks:
  - Merge conflict with the sibling throwing-sink fix, which edits the same ten-line region of `CompletePrime`. Mitigation: the sibling lands first; the plan locates the region by content anchors rather than line numbers, keeps the sibling's sink guard, places the guarded report and its record so that `TryRemove` stays the last statement, and documents the two constraints in the `CompletePrime` remarks.
  - A later permanent fault hidden behind an earlier transient one. Mitigation: the key includes the base-exception type, and test D pins that a new kind is reported.
  - An operator reading the log expects repeats and concludes the fault cleared. Mitigation: the appended sentence in the first entry, pinned by test G.
  - An analyzer diagnostic on the value-tuple dictionary key or on the probe-then-indexer sequence. Mitigation: the indexer is used for the record so no result is discarded; if an analyzer prefers `TryAdd` over probe-then-set, use `TryAdd` and consume its result in the condition (report only when it returns true, which also keeps the record after the sink only if written as probe-then-report-then-set; prefer the probe-then-set form and resolve any diagnostic by the narrowest in-code means allowed by the C# policy).
  - Memory growth of the reported set. Mitigation: bounded by two mapped keys times the finite set of base-exception types; no clearing is needed because a cached key never primes again.
- Mitigations and rollbacks: revert the single production file, the new partial, and the compile item. No data or configuration migration is involved.

## Rollout & Follow-up

- Release/rollout steps: ships with the add-in build; no flag, configuration, or migration. Verify in the first session after release that the debug log shows at most one prime-failure entry per engine key per failure kind.
- Post-fix monitoring or clean-up tasks:
  - Follow-up (documentation only, deferred to keep the existing partials byte-identical): the remarks on `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker` in the Race partial under the TaskMaster.Test Ribbon folder state that the re-prime "re-enters that same canceled task and logs a second error." Under this policy the second cycle is a suppressed report; the test's assertions are unaffected because the single-error assertion is taken before the re-prime. Correct the remark in the sibling throwing-sink fix or in the next edit that touches that partial.
  - Constraint carried into the sibling throwing-sink fix's sink guard (issue 947, which lands first): the record into the reported-faults dictionary stays the statement immediately after the sink call, inside the sibling's try block and outside any catch or finally arm, so that a sink that throws leaves the pair unrecorded. Recording before the sink, or in a catch or finally arm, would suppress the report for the session after a sink that threw on the first attempt. Any later change to that region must keep this placement.
  - If a cache-invalidation feature is ever added to the pressed-state cache, the reset of the reported-faults set belongs with it; today it is unobservable.
- Links: issue, PRs, related docs
  - Issue #948: https://github.com/drmoisan/TaskMaster/issues/948
  - Research: the file 2026-10-01T07-20-engine-toggle-permanent-config-fault-logs-every-poll-research.md under this feature folder's research subfolder (cited in prose because it is not a write target).
  - Predecessor: #944 (prime marker registration races removal); sibling: issue 947 (throwing logError sink leaves a stale prime marker); origin of the toggle coordinator: #505, #506, #518; CR-2 canceled prime: #735; report-then-clear: #942.
