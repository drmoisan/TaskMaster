# 2026-09-30-engine-toggle-prime-marker-registration-races-removal (Spec)

- **Issue:** #944
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-30T12-00
- **Status:** Ready for planning
- **Version:** 1.2 (planner amendments: AC14 and AC15 and the Test Strategy toolchain step 4 sentence at 1.1; AC14 and the same Test Strategy sentence again at 1.2, naming the vstest hang-blame switch on the DIRECT route; see plan decision D-12)
- **Work Mode:** full-bug (this file is the sole acceptance-criteria source; no user-story.md is produced)
- **Design record:** `docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/research/2026-09-30T08-00-engine-toggle-prime-marker-registration-research.md` (sections 3 (iii), 4, 5 and 6 are adopted as written)

> Formatting note for later editors: backticked repository paths in this document are the change footprint read by downstream tooling. Only files listed in the Write Set, plus evidence paths inside this feature folder, are backticked. Files that must NOT change are named in plain prose on purpose; do not add backticks to them. Code is cited by unique source tokens rather than line numbers, because the upstream prime-fault-logging fix (issue #942) shifts line numbers in the same files.

## Context

In `EngineToggleStateCoordinator`, a prime whose antecedent task is already complete (or completes on another thread) in a non-success state can run the marker removal inside `CompletePrime` before `StartPrimeIfNeeded` has stored that marker. The registering thread then stores a handle for a prime that has already finished. `_primeTasks.ContainsKey(engineName)` stays true for the rest of the session, and no later `GetPressed` read can start a new prime for that engine. This is hazard B from the issue #942 research, first recorded as NB-2 in the issue #735 code review, and not previously promoted.

Environment:
- OS/version: Windows 11 / windows-latest
- Runtime: C#, .NET Framework v4.8.1 (all TaskMaster projects)
- Reachability: production path (`RibbonController` `getPressed` polling through `IsEngineToggleActive` to `GetPressed`); also reachable from the re-prime at the end of the existing test `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` and the second read in `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker`
- Data source or fixture: n/a

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Repro & Evidence

Steps to Reproduce:
1. Arrange for `EngineActiveAsync` to return an already-faulted task. In production this occurs after a cached configuration-load fault: `AppItemEngines.EngineActiveAsync` begins with `await Globals.AF.Manager.Configuration`, which is an `AsyncLazy` over a `Lazy` of a task, so a faulted load is cached and every later await rethrows synchronously.
2. Call `GetPressed` for that engine. On a cache miss it calls `StartPrimeIfNeeded`.
3. Inside `lock (_primeGate)`, the statement `_primeTasks[engineName] = StartObservedPrime(engines, engineName, controlId);` first evaluates `StartObservedPrime`, which calls `ContinueWith` on the already-faulted task. With `TaskContinuationOptions.None` the continuation is queued to the thread pool immediately. A pool thread can dequeue it and execute `_primeTasks.TryRemove(engineName, out _);` inside `CompletePrime` before the registering thread performs the dictionary store.

Expected:
A finished failed or canceled prime never leaves a marker in `_primeTasks`, so a later `GetPressed` can start a new prime.

Actual:
The removal can precede the registration. The stored handle then belongs to a prime that has already completed, `ContainsKey` returns true, and no later prime starts for that engine for the rest of the session.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Source: research record for issue #942 (docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/research/2026-09-29T23-20-engine-toggle-prime-fault-race-research.md), conclusion 3; code review for issue #735 (docs/features/active/2026-09-02-ribbon-engine-toggle-defects-735/code-review.2026-09-03T06-19.md), NB-2.

## Scope & Non-Goals

- In scope:
  - Register the prime marker before the prime starts, inside the existing `lock (_primeGate)` block of `StartPrimeIfNeeded`, in `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`.
  - Change `StartObservedPrime` to return `void`, accept the marker, and complete the marker after `CompletePrime` returns.
  - Update the XML documentation that describes the stored value (`_primeTasks` field summary, `_primeGate` field summary, `StartObservedPrime` remarks).
  - Add three regression tests in a new partial, `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs`, and register it in `TaskMaster.Test/TaskMaster.Test.csproj`.
- Out of scope / non-goals:
  - Any change to the `CompletePrime` method, including its report-then-clear ordering and its `_primeTasks.TryRemove(engineName, out _);` statement. That ordering is owned by issue #942.
  - Identity-conditional removal (the `ICollection` of `KeyValuePair` `Remove` overload). Research section 4.4 shows it is unnecessary under the chosen design and it would edit the issue #942-owned `TryRemove` line.
  - A scheduler constructor seam, `TaskContinuationOptions.ExecuteSynchronously`, `await Task.Yield()` in `ApplyPrimeAsync`, a cold `Task` with `RunSynchronously`, or a lock inside `CompletePrime` (rejected in research section 3).
  - The throwing-`logError`-sink hazard and the log volume after a permanent configuration fault (recorded under Rollout and Follow-up; no issue is filed by this item).
- Explicitly excluded files (named in plain prose on purpose; these must be byte-identical to the re-anchored origin/main after this change):
  - TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs (main fixture, including the private `Harness` and its issue #942 `OnLogError` hook)
  - TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs
  - TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs (added by issue #942)
  - TaskMaster/Ribbon/RibbonController.EngineCommands.cs (the only production caller; unaffected)
  - TaskMaster/TaskMaster.csproj (the production compile item already exists)

## Root Cause Analysis

`StartPrimeIfNeeded` evaluates the right-hand side of `_primeTasks[engineName] = StartObservedPrime(engines, engineName, controlId);` before it performs the store. That evaluation calls `ApplyPrimeAsync` and attaches the continuation `completed => CompletePrime(completed, engineName),` with `TaskContinuationOptions.None,` and `TaskScheduler.Default`. On .NET Framework 4.8 the continuation is never run inline on the registering thread with those options; it is queued to the thread pool (research section 2, items 2 to 4, verified against the .NET Framework reference source). The race window therefore runs from the queueing inside `ContinueWith` to the completion of the dictionary store on the registering thread.

`CompletePrime` removes the marker without taking `_primeGate`. A pool thread that runs `CompletePrime` inside the window finds no entry to remove (first prime) and returns; the registering thread then stores a handle for the finished prime. Because `_primeTasks` is written only by `StartPrimeIfNeeded` and removed only by `CompletePrime`, nothing removes that handle afterwards.

The window is not limited to synchronously failed primes: any antecedent that completes on another thread between `ContinueWith` registration and the store has the same exposure. A synchronously failed prime opens the window at the earliest point. No current test fails on this, because the existing re-prime assertions hold whether `GetPrimeTask` returns the stale handle or `Task.CompletedTask` (research section 1.3).

The defect is an ordering property: the marker is registered after the prime can complete. The fix restores the invariant "the marker is registered before the prime can complete, on any thread".

## Proposed Fix

### Design summary (what changes where):

Adopt research design 3 (iii), register-before-start, in `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` only:

1. In `StartPrimeIfNeeded`, inside the existing `lock (_primeGate)` block and after the `if (_primeTasks.ContainsKey(engineName))` check, create `var marker = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);`, store `_primeTasks[engineName] = marker.Task;`, and only then call `StartObservedPrime(engines, engineName, controlId, marker);`. Add a short why-comment stating that registration precedes the start so that a prime completing on any thread, including before `StartObservedPrime` returns, always finds its own marker to remove (issue #944).
2. `StartObservedPrime` becomes `private void StartObservedPrime(IAppItemEngines engines, string engineName, string controlId, TaskCompletionSource<bool> marker)`. It discards the continuation task (`_ = ApplyPrimeAsync(...).ContinueWith(...)`, which satisfies MA0134) and the continuation body becomes `try { CompletePrime(completed, engineName); } finally { marker.SetResult(true); }`. The arguments `CancellationToken.None`, `TaskContinuationOptions.None` and `TaskScheduler.Default` are unchanged.
3. `CompletePrime` is not edited.

State model (research section 4.1): `_primeTasks[engine]` is absent, then registered-incomplete (under `_primeGate`, before the start), then either registered-complete for the session (success) or, on failure or cancellation, report, then removed, then marker complete.

### Boundaries and invariants to preserve:

- **Report-then-clear (issue #942).** `CompletePrime` reports through `_logError` and only then executes `_primeTasks.TryRemove(engineName, out _);`. The method body is unchanged. Because the marker is completed in the `finally` that follows the `CompletePrime` call, an awaiter of the handle resumes only after both the report and the removal.
- **Handle identity (issue #942 test).** The handle captured before a prime fault is triggered is `marker.Task`; inside the `logError` sink, before the removal, `GetPrimeTask` returns the same `marker.Task`; after the handle completes, `GetPrimeTask` returns `Task.CompletedTask`.
- **Never-faults contract of `GetPrimeTask`.** The marker is completed only through `SetResult`, so the returned task cannot fault or cancel.
- **At most one concurrent prime per engine.** The check-and-register pair stays atomic under `_primeGate`. `_primeTasks[engineName]` has exactly one writer, so while a marker is registered no other value can replace it, and the only removal of it is its own `CompletePrime`.
- **Single `catch` in the type.** `try`/`finally` adds no `catch`; the click boundary in `HandleToggleClickAsync` remains the only `catch` clause.
- **No new lock and no deadlock path.** `_primeGate` is still held across a dictionary probe, a store, and the synchronous prefix of the prime start, as today. `CompletePrime` takes no lock. `GetPrimeTask` takes no lock, so calling it from inside `EngineActiveAsync` while `_primeGate` is held (as test 1 does) cannot deadlock.
- **Synchronous start.** `ApplyPrimeAsync` is still invoked directly on the calling thread, inside the lock. `GetPressed` still never awaits, blocks, or throws.
- **Success path.** A successful prime's marker stays registered, and complete, for the session, exactly as the continuation handle does today.

### Dependencies or blocked work:

**Binding sequencing constraint: issue #942 must merge first.** Issue #942 (branch bug/engine-toggle-prime-fault-logging-test-races-942) edits the same production file and the same fixture and merges into main before this item executes.

- (a) Execution of this item must not begin until issue #942 has merged into main. The first execution step re-anchors this branch on the then-current origin/main (rebase or merge) and records the origin/main commit it anchored on in the execution record.
- (b) Post-#942 baseline this spec is written against: `CompletePrime` reports through `_logError` and only then calls `_primeTasks.TryRemove(engineName, out _);`; its summary and the `GetPrimeTask` returns element were rewritten by issue #942. This item preserves report-then-clear and leaves the `CompletePrime` body and the `TryRemove` statement unchanged.
- (c) Issue #942 adds an `OnLogError` hook to the private `Harness`, a third partial TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs containing `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged`, and a matching compile entry. Every assertion of that test must keep passing.
- (d) This item does not modify the main fixture file, the Race partial, the PrimeFaultOrdering partial, or RibbonController.EngineCommands.cs.
- (e) All code citations in this spec, the plan, and the execution record use unique source tokens (research section 7), never line numbers.
- If, after re-anchoring, the `CompletePrime` body does not match the report-then-clear shape described in (b), execution halts and the discrepancy is reported rather than worked around.

No other blocking work.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

| File | Change | Size budget |
|---|---|---|
| `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | modify: marker registration in `StartPrimeIfNeeded`; `StartObservedPrime` signature, body and remarks; `_primeTasks` and `_primeGate` summaries | 415 total lines before issue #942; expected roughly 430 to 445 after both issues; must stay at or below 500 total lines |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs` | create: `public partial class EngineToggleStateCoordinatorTests` with three `[TestMethod]` tests | roughly 170 to 200 total lines; must stay at or below 500 |
| `TaskMaster.Test/TaskMaster.Test.csproj` | add `<Compile Include="Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs" />` adjacent to the existing `<Compile Include="Ribbon\EngineToggleStateCoordinatorTests.Race.cs" />` entry (the issue #942 PrimeFaultOrdering entry is expected nearby) | n/a (project file; the 500-line ceiling does not apply) |

The legacy test project uses explicit `<Compile Include>` items; without the entry the new tests would silently not compile or run.

#### Functions/classes/CLI commands impacted:

- `EngineToggleStateCoordinator.StartPrimeIfNeeded(string engineName, string controlId)`: registers the marker before starting the prime.
- `EngineToggleStateCoordinator.StartObservedPrime(...)`: return type `Task` becomes `void`; new parameter `TaskCompletionSource<bool> marker`; continuation body wraps `CompletePrime` in `try`/`finally` that calls `marker.SetResult(true)`.
- `EngineToggleStateCoordinator.GetPrimeTask(string engineName)`: signature and body unchanged; the value it returns is now the registered marker task instead of the continuation task (see Technical specifications).
- `EngineToggleStateCoordinator.CompletePrime(Task completed, string engineName)`: verify-only; not edited.
- `EngineToggleStateCoordinator._primeTasks`, `EngineToggleStateCoordinator._primeGate`: XML summaries updated; field types unchanged (`ConcurrentDictionary<string, Task>`, `object`).
- No public or cross-assembly API changes. All touched members are `private` or `internal`.

#### Data flow and validation changes:

`GetPressed` cache miss, then `StartPrimeIfNeeded`: accessor null returns; otherwise under `_primeGate` the `ContainsKey` probe, marker creation, marker store, then `StartObservedPrime`, which calls `ApplyPrimeAsync` synchronously and attaches the continuation. The continuation runs `CompletePrime` and then completes the marker. No input validation changes.

#### Error handling and logging updates:

None to behavior. Faults and cancellations are still observed by `CompletePrime` through the continuation, reported through `_logError` with the unwrapped base exception or a synthesized `TaskCanceledException`, and are not rethrown. No `catch` clause is added.

#### Rollback/feature-flag considerations (if applicable):

No feature flag. Rollback is a revert of this item's commits; the issue #942 changes are independent and remain in place.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

- **`GetPrimeTask` return contract (changed).** Returns the registration marker, `TaskCompletionSource<bool>.Task` created with `TaskCreationOptions.RunContinuationsAsynchronously`, which is registered in `_primeTasks` before the prime starts and completes only after `CompletePrime` has returned (on every outcome: success, fault, cancellation). Returns `Task.CompletedTask` for a null or empty key, or when no marker is registered. The returned task never faults or cancels. The issue #942 returns element (the handle stays incomplete while the marker is registered on the failure path) remains true; after re-anchoring, the `GetPrimeTask` XML documentation is re-read and edited only if a sentence has become false (for example a sentence that names the continuation task as the returned value).
- **`StartObservedPrime` contract (changed).** `void`; receives the already-registered marker; completes it in a `finally` after `CompletePrime` returns. Its remarks replace the sentence beginning "The returned continuation task always completes successfully" with a description of the marker: the continuation task is discarded, and the marker, completed only by `SetResult` after `CompletePrime`, is the value a test awaits.
- **`_primeTasks` field summary (changed).** Replace "The in-flight — or most recently completed — prime per engine key." with wording stating that the value is the registration marker for the engine's prime, registered before the prime starts, removed by `CompletePrime` on failure or cancellation, and retained after success; its presence remains the at-most-one-prime guard and its value remains the handle returned by `GetPrimeTask`.
- **`_primeGate` field summary (changed).** "Held only across a dictionary probe and a task start" becomes "held only across a dictionary probe, the marker registration, and the start of the prime"; "no await occurs inside it" is kept.
- `RunContinuationsAsynchronously` is already used in production TaskMaster code (`NonBlockingDelay`, `AppOlObjects.FolderTreeService`) and exists in the .NET Framework v4.8.1 reference source. It is chosen so that awaiters of the marker resume on their own pool work item rather than inside the coordinator's `finally`; it is not required for correctness.

#### Required configuration keys and defaults:

None.

#### Backward-compatibility expectations:

- The only production caller, RibbonController.EngineCommands.cs, never calls `GetPrimeTask` and is unaffected.
- The ten existing `GetPrimeTask` call sites in test code (five in the main fixture, five in the Race partial, per research Numeric Derivation Evidence; issue #942 adds more) all await the returned handle; the marker completes after `CompletePrime` on every outcome, so they remain valid. The existing baseline is 22 test methods and 24 test cases before issue #942 (research section 5.3).

#### Performance constraints (latency/throughput/memory):

One additional `TaskCompletionSource<bool>` allocation per prime. Primes occur at most once per engine per session on success, and once per cache-miss read after a failure. No measurable effect on the synchronous `getPressed` path is expected; no latency target is set.

## Assumptions, Constraints, Dependencies

- Assumptions:
  - Issue #942 merges with the shape described in Dependencies (b) and (c). If it does not, execution halts per Dependencies.
  - `ApplyPrimeAsync` remains an `async` method with no synchronous throw path, so `StartObservedPrime` cannot throw after the marker is registered.
- Constraints:
  - Single production file. No scheduler seam, no `ExecuteSynchronously`, no new lock, no new `catch`.
  - Tests use MSTest, Moq (the fixture's strict `Mock<IAppItemEngines>`), and FluentAssertions. No `Thread.Sleep`, `Task.Delay`, wall-clock waits, retries, temporary files, `[DoNotParallelize]`, `Workers=1`, or scheduler seam.
  - The 500-line ceiling applies to both C# files, measured as total lines.
- External dependencies: none beyond the .NET Framework v4.8.1 TPL.

## Data / API / Config Impact

- User-facing or API changes: none. After a failed or canceled prime, a later `getPressed` poll now reliably starts a new prime instead of being blocked for the session.
- Data or migration considerations: none.
- Logging/telemetry updates: none to code. Operational effect: after a permanent configuration fault, each cache-miss poll that reaches `StartPrimeIfNeeded` now logs again (previously the stale marker intermittently suppressed repeats). See Rollout and Follow-up.
- Compatibility notes: none.

## Test Strategy

Framework: MSTest (`[TestClass]` via the existing partial, `[TestMethod]`), Moq (the existing strict `Harness.Engines`), FluentAssertions. Each test constructs its own `Harness`, so the tests are independent under the repository's class-level parallel execution. All three tests live in `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs` as members of `public partial class EngineToggleStateCoordinatorTests` and reuse the existing private `Harness`, `LoggedError`, `SpamEngine` and `SpamToggleControlId` without adding harness members.

Bugfix workflow order: the new partial and its csproj entry are added and run against the unchanged (re-anchored, post-#942) production file first, to capture the fail-before evidence; only then is the production file edited.

### Test 1 — program-order discriminator (carries the fail-before obligation)

`GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns`

- Arrange: `harness.Engines.Setup(x => x.EngineActiveAsync(SpamEngine)).Returns(() => { handleSeenDuringRead = harness.Coordinator.GetPrimeTask(SpamEngine); handleCompletedDuringRead = handleSeenDuringRead.IsCompleted; return Task.FromException<bool>(failure); });`. The callback runs synchronously inside `ApplyPrimeAsync` on the test thread. It only records; no assertion runs inside it, because an assertion thrown there would become a prime fault.
- Act: `harness.Coordinator.GetPressed(SpamEngine);`
- Assert: `handleCompletedDuringRead.Should().BeFalse(...)` with a message stating that the prime handle must be registered before the activation read runs; then `await handleSeenDuringRead;`; `harness.Errors` contains exactly one entry whose `Exception` is the same instance as `failure`; `harness.Coordinator.GetPrimeTask(SpamEngine)` is not the same instance as `handleSeenDuringRead`, and its `IsCompleted` is true.
- Before the fix: inside the callback no entry exists, so `GetPrimeTask` returns `Task.CompletedTask` and `handleCompletedDuringRead` is true. The first assertion fails on every run by program order alone. This is the deterministic fail-before test.
- After the fix: the marker is registered and cannot yet be complete, because completing it requires the continuation, which requires the task `ApplyPrimeAsync` has not yet returned. The remaining assertions are deterministic because awaiting the marker resumes only after the report and the removal.
- Do not assert `GetPrimeTask` identity immediately after `GetPressed` returns (the queued continuation may already have removed the marker). Do not use `BeSameAs(Task.CompletedTask)` as the discriminator; `IsCompleted` is sufficient.

### Tests 2 and 3 — behavioral re-prime guards (deterministic pass after the fix; pre-fix failure is timing-dependent)

`GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime` and `GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime`

- Arrange: `harness.Engines.SetupSequence(x => x.EngineActiveAsync(SpamEngine)).Returns(Task.FromException<bool>(failure)).Returns(Task.FromResult(true));`. The canceled variant uses `Task.FromCanceled<bool>(new CancellationToken(true))` as the first return (requires `using System.Threading;`).
- Act: `GetPressed(SpamEngine)`; `await harness.Coordinator.GetPrimeTask(SpamEngine);`; second `GetPressed(SpamEngine)`; `var secondPrime = harness.Coordinator.GetPrimeTask(SpamEngine); await secondPrime;`.
- Assert: `harness.Engines.Verify(x => x.EngineActiveAsync(SpamEngine), Times.Exactly(2), ...)`; a third `GetPressed(SpamEngine)` returns true; `harness.Invalidations` equals a single-element list containing `SpamToggleControlId`; `harness.Errors` contains exactly one entry (faulted: `Exception` is the same instance as `failure`; canceled: `Exception` is assignable to `OperationCanceledException`).
- After the fix these are deterministic: the first `GetPrimeTask` returns either the marker (awaiting it resumes after removal) or `Task.CompletedTask` (removal already happened); either way the second read starts a new prime, which completes synchronously on the test thread.
- Before the fix these fail only when a pool thread wins the race, so they are regression guards for the user-visible outcome and do not carry the fail-before obligation. Their pre-fix outcome is recorded informationally in the fail-before projection.
- Strict-mock note: a further `EngineActiveAsync` call would return null from the exhausted sequence; none occurs because the read after success is a cache hit.

### Existing tests

All existing coordinator tests stay unmodified and must pass: the main fixture (16 methods, 18 cases), the Race partial (6 methods), and the issue #942 PrimeFaultOrdering partial, including `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged`. `GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime` continues to guard the at-most-one-prime invariant.

### Edge cases and negative scenarios

- Faulted synchronous prime (tests 1 and 2), canceled synchronous prime (test 3), success after failure (tests 2 and 3), null/whitespace/unmapped keys and null engines (existing main-fixture tests, unchanged).

### Error handling and logging verification

- Tests 1 to 3 assert exactly one `logError` report per failed prime with the correct exception; the issue #942 test asserts the report precedes the removal.

### Coverage impact and targets

- Changed lines must not lose coverage relative to the pre-change baseline.
- `StartPrimeIfNeeded` and `StartObservedPrime` (changed methods, including the new `finally`) target at least 90 percent line coverage; all three tests and the existing prime tests exercise them.
- Baseline and after figures are recorded as Markdown projections (no raw coverage or TRX document is committed).

### Evidence artifacts (Markdown projections only; fixed filenames; run timestamp in each artifact's Timestamp field)

- `docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/coverage-baseline.md` — pre-change coverage figures for `EngineToggleStateCoordinator` and the origin/main commit anchored on.
- `docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/regression-testing/prime-registration-fail-before.md` — the three new tests run against the unchanged production file: test 1 failing on the `handleCompletedDuringRead` assertion with its assertion message (not a compile or assembly-load failure), plus the informational outcome of tests 2 and 3.
- `docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/regression-testing/prime-registration-pass-after.md` — the three new tests, the issue #942 test, and every existing coordinator test passing after the fix.
- `docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/qa-gates/toolchain-final-pass.md` — the final four-step toolchain pass: commands, exit codes, output summaries.
- `docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/qa-gates/coverage-summary.md` — after figures for the changed methods and file, compared against the baseline, plus the repository summary line.

### Toolchain commands to run (format, lint, type-check, test), in order, restarting from step 1 on any failure or auto-fix

1. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .`
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. The `test: MSTest with Coverage (Koverage)` VS Code task, or Invoke-MSTestWithCoverage.ps1 under scripts/vscode invoked directly (named in plain prose because it is not modified); when the plan's Phase 0 stall probe observes the known local shell-icon test stall, the runner's own inner collector invocation with those four test classes excluded and the vstest hang-blame switch appended, post-processed by the runner's own helpers, as AC14 states.

### Manual validation steps

None required. The defect has no reliable manual reproduction; the program-order test is the reproduction.

## Acceptance Criteria

- [x] AC1 — Execution began only after the report-then-clear fix for the engine-toggle prime fault-logging test races had merged into main; the branch was re-anchored on the then-current origin/main before any code change, and the execution record names the origin/main commit it anchored on and confirms that `CompletePrime` reported through the error sink before its marker removal at that commit.
- [x] AC2 — `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs` contains the test `GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns`, which records from inside the `EngineActiveAsync` setup callback the handle returned by `GetPrimeTask` and its `IsCompleted` value without asserting inside the callback, then asserts outside the callback that the recorded handle was not completed, awaits it, asserts exactly one logged error whose exception is the injected failure instance, and asserts that `GetPrimeTask` afterwards returns a different, completed task.
- [x] AC3 — Fail-before evidence: the fail-before projection named in the Test Strategy records a run of the program-order test against the unchanged production file in which it fails on the not-completed-during-read assertion with that assertion's message, and not by a compile error, an assembly-load error, or a timeout.
- [x] AC4 — Pass-after evidence: the pass-after projection named in the Test Strategy records the program-order test and both re-prime tests passing after the production change.
- [x] AC5 — The test `GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime` passes: after a first activation read that returns an already-faulted task, a later `GetPressed` starts a new prime, `EngineActiveAsync` is verified as called exactly twice, the toggle then reads as pressed, the mapped control is invalidated exactly once, and exactly one error carrying the injected failure instance is logged.
- [x] AC6 — The test `GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime` passes with the same assertions as the faulted re-prime test, except that the first activation read returns an already-canceled task and the single logged exception is assignable to `OperationCanceledException`.
- [x] AC7 — In `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, `StartPrimeIfNeeded` creates a boolean `TaskCompletionSource` with `TaskCreationOptions.RunContinuationsAsynchronously` and stores its task in `_primeTasks` inside the existing `_primeGate` lock block, after the `ContainsKey` check and before `StartObservedPrime` is called; `StartObservedPrime` returns `void`, receives that completion source as a parameter, keeps `CancellationToken.None`, `TaskContinuationOptions.None` and `TaskScheduler.Default`, and completes the marker only through `SetResult` inside a `finally` block that follows the `CompletePrime` call in the continuation.
- [x] AC8 — Report-then-clear is preserved: the `CompletePrime` method, including its `_primeTasks.TryRemove(engineName, out _);` statement and its XML documentation, is identical to the re-anchored origin/main, and `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged` passes with no assertion changed.
- [x] AC9 — At most one concurrent prime per engine: the `ContainsKey` check and the marker store occur within a single `_primeGate` lock block with no other writer to `_primeTasks` in the type, and `GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime` passes unmodified.
- [x] AC10 — The diff of `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` against the re-anchored origin/main adds no `catch` clause, no `lock` statement, and no `Monitor`, `SemaphoreSlim`, `Mutex` or `ReaderWriterLockSlim` usage.
- [x] AC11 — Every test method in the main coordinator fixture, the Race partial and the PrimeFaultOrdering partial passes, and those three test files and RibbonController.EngineCommands.cs are byte-identical to the re-anchored origin/main.
- [x] AC12 — The `_primeTasks` field summary describes the registration marker registered before the prime starts, the `_primeGate` field summary names the marker registration among the operations it is held across, the `StartObservedPrime` remarks describe the marker instead of the returned continuation, the production file no longer contains the phrase "The returned continuation task always completes successfully", and `StartPrimeIfNeeded` carries a why-comment explaining that registration precedes the start.
- [x] AC13 — The new partial uses MSTest, the existing strict Moq harness and FluentAssertions, constructs a fresh harness in each test, and contains no `Thread.Sleep`, `Task.Delay`, `SpinWait`, polling loop, retry, wall-clock read, temporary file, `DoNotParallelize` or other parallelism attribute, and no custom `TaskScheduler` or scheduler seam.
- [x] AC14 — A single final toolchain pass succeeds in order with no step failing or rewriting a file: `dotnet tool run csharpier check .`, the analyzer rebuild with analyzers and code-style enforcement enabled, the nullable rebuild with warnings treated as errors, and the coverage-enabled MSTest run, each exactly as named in CLAUDE.md, recorded in the toolchain projection named in the Test Strategy. When the plan's baseline stall probe observes the known local shell-icon test stall, the coverage-enabled run is the coverage runner's own inner collector invocation with those four test classes excluded and the vstest hang-blame switch appended, post-processed by the runner's own helpers and floor checks, and the toolchain projection records that route and the probe result.
- [x] AC15 — Coverage: no changed line in `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` loses coverage relative to the baseline projection, `StartPrimeIfNeeded` and `StartObservedPrime` each reach at least ninety percent line coverage in the coverage projection, and the repository summary line is recorded beside its baseline value; when the two runs' repository line denominators differ by at most one percent, the post-change repository line rate is not more than half a percentage point below the baseline rate, and when they differ by more, the merged repository figures are not comparable across runs and are recorded without a gate.
- [x] AC16 — The diff adds no `.trx`, `.xml` or `.coverage` file; all committed test and coverage evidence is Markdown projections inside this feature folder.
- [x] AC17 — The diff against the re-anchored origin/main is limited to the three code files `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs` and `TaskMaster.Test/TaskMaster.Test.csproj`, plus files inside this feature folder and the inherited promotion record for this item listed in the Write Set.
- [x] AC18 — `TaskMaster.Test/TaskMaster.Test.csproj` contains a `Compile Include` entry for the new PrimeRegistration partial, and `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` and `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs` are each at or below the repository five-hundred-line ceiling measured as total lines; the project file is exempt from the ceiling.

## Risks & Mitigations

- **Issue #942 lands in a different shape than assumed.** Mitigation: Dependencies (a) and (b) and AC1 require re-anchoring and verifying the `CompletePrime` shape before any change; a mismatch halts execution.
- **Test 1 passes vacuously before the fix** (for example, if the callback were never invoked, both recorded values would stay at defaults). Mitigation: initialize `handleCompletedDuringRead` so that a never-invoked callback fails the assertion (for example to true, or assert the recorded handle is non-null), and AC3 requires captured fail-before evidence showing the specific assertion failing.
- **Fail-before run misread.** A compile failure or an assembly-load failure (empty message, sub-millisecond duration) is not a valid fail-before. Mitigation: AC3 excludes those outcomes explicitly; the new tests use only existing `internal` API, so they compile against the unchanged production file.
- **Marker leaks if `StartObservedPrime` ever throws after registration.** `ApplyPrimeAsync` is `async`, so faults go into its task, and `ContinueWith` with valid arguments does not throw. Mitigation: record this as an assumption; any future change that adds a synchronous throw path before the continuation is attached must also remove the marker.
- **Throwing `logError` sink.** Under report-then-clear, a throwing sink skips `TryRemove`; the marker still completes (`finally`) but stays registered, and the continuation task faults unobserved. Production sink is `logger.Error`, so likelihood is low. Mitigation: out of scope; recorded as a follow-up.
- **Analyzer findings on the discarded continuation.** Mitigation: use an explicit discard (`_ = ...`), which satisfies MA0134; the analyzer rebuild in AC14 is the gate.
- Rollback: revert this item's commits; issue #942 is unaffected.

## Rollout & Follow-up

- Release/rollout steps: standard PR into main after issue #942 has merged; no configuration, migration or flag.
- Post-fix monitoring: none required beyond the add-in log.
- Out-of-scope follow-ups (recorded here only; this item files no issue):
  1. **Throwing `logError` sink leaves a stale marker.** Under report-then-clear, a sink that throws skips the removal, reproducing this symptom by a different cause. With this fix the marker still completes, but the continuation faults unobserved. Candidate for a separate bug.
  2. **Log volume after a permanent configuration fault.** `AsyncLazy` caches the configuration fault, so each cache-miss `getPressed` poll re-primes and logs again. Previously the stale marker intermittently suppressed repeats; after this fix every poll that reaches `StartPrimeIfNeeded` logs. A back-off or a `ResetConfigAsyncLazy`-based recovery is a separate decision.
- Links: issue #944 (https://github.com/drmoisan/TaskMaster/issues/944); upstream dependency issue #942; origin NB-2 in the issue #735 code review.

## Write Set

- `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (modify)
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs` (create)
- `TaskMaster.Test/TaskMaster.Test.csproj` (modify: one compile entry)
- `docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/**` (this feature folder: plan, evidence, spec updates)
- `docs/features/potential/promoted/2026-09-30-engine-toggle-prime-marker-registration-races-removal.md` (inherited promotion record)
