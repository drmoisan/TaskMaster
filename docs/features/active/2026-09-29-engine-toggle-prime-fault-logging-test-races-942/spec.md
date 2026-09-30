# 2026-09-29-engine-toggle-prime-fault-logging-test-races (Spec)

- **Issue:** #942
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-29T23-40
- **Status:** Ready for planning
- **Version:** 0.2

> Formatting note for later editors: inline code spans around repository paths in this document are the change footprint. Only files listed under "Write Set" in the Scope section, and the evidence projections named in the Test Strategy section (which sit under that same feature folder), are backticked. Out-of-scope files are cited in plain prose on purpose; do not add backticks to them. Acceptance-criterion lines deliberately contain no digits, no angle brackets, and no percent signs; counts are written as words.

## Context

`EngineToggleStateCoordinatorTests.GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` failed once in CI and passed on rerun. The fault logging it asserts appeared to race the task the test awaits. The research record under this feature folder's research directory established the actual mechanism: it is not an unawaited continuation but a statement-ordering defect inside the coordinator, which lets a post-trigger call to `GetPrimeTask` return an already-completed task while the fault report is still pending on a thread-pool thread.

Environment:
- OS/version: windows-latest (GitHub Actions)
- Python version: n/a (C# / MSTest)
- Command/flags used: required check MSTest with coverage
- Data source or fixture: n/a

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

The severity is High because the failure lands on a required CI check and blocks unrelated pull requests (the first observed failure was on PR #939, which does not touch this code).

## Repro & Evidence

Steps to Reproduce:
1. Run the TaskMaster.Test project under the parallel regime configured by the repository run settings (Workers set to zero, class-level scope).
2. Observe `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` in `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` (the test begins at line 213). It fails intermittently.

Expected:
The test is deterministic: the error log it asserts is written before the awaited task completes, so any caller that obtains the prime handle after the trigger still awaits the report.

Actual:
It failed once on PR #939 head `9624376dc`, passed on a single rerun, and passed in two local runs. PR #939 does not touch this code. The failure is timing-dependent and cannot be reproduced on demand with the existing test; the research record (section 4) gives the exact interleaving and explains why the window is ordinarily microseconds wide but is widened on first execution by JIT compilation and culture-data initialisation inside the reporting path.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: CI run for PR #939 at head `9624376dc` (first attempt).
- Research record: the file under this feature folder's research directory dated 2026-09-29T23-20 (read in full; its verified line citations were re-checked against this worktree while authoring this spec).

## Scope & Non-Goals

- In scope:
  - Reorder two statements in `CompletePrime` in `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` so the fault is reported through the injected error-log delegate before the in-flight marker is removed, and update the method's documentation to describe and justify that order.
  - Add an `OnLogError` observer hook to the private `Harness` fixture in `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs`, mirroring the existing `OnInvalidate` hook.
  - Add one deterministic regression test in a new third partial, `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs`, registered with an explicit compile item in `TaskMaster.Test/TaskMaster.Test.csproj`.
  - Capture fail-before and pass-after evidence projections, a coverage baseline and post-change comparison, and the final toolchain pass under this feature folder's evidence tree.
- Out of scope / non-goals:
  - Hazard B from the research record (registration of the prime marker racing its removal when the prime completes synchronously in a non-success state; recorded as NB-2 in the issue #735 code review). It is promoted separately as its own issue. This fix neither worsens nor addresses it, and no lock is added to CompletePrime.
  - Any change to the existing test `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse`. It is left unmodified so it remains the original reproduction the bugfix workflow asks to re-run.
  - Any change to the second existing partial, EngineToggleStateCoordinatorTests.Race.cs, or to the production wiring in RibbonController.EngineCommands.cs.
  - Any change to the parallel run settings (TaskMaster.runsettings, scripts/vscode/TaskMaster.cli.runsettings).
  - Retries, sleeps, `Task.Delay`, `Thread.Sleep`, `[DoNotParallelize]`, a single-worker setting, timeouts, blocking gates inside the sink, temporary files, or an injected scheduler seam. Each is either prohibited by repository policy or rejected in the research record with a stated reason.
  - The optional try-finally hardening around the log call discussed in the research record (section 6). The production sink is log4net's error method, which does not throw on appender failure, so the hardening is not required for this issue.
  - The two other production sites that discard fault-observing continuations (AppEvents.ReadinessHookup.cs and OutlookFolderTreeService.cs). No test asserts on their logs, so they are not part of this defect.
- Explicitly excluded systems, integrations, or datasets: Outlook host process, Office ribbon callbacks, log4net configuration.

### Write Set

- `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs`
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs` (new)
- `TaskMaster.Test/TaskMaster.Test.csproj`
- `docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/spec.md`
- Evidence projections under `docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/` as named in the Test Strategy section

## Root Cause Analysis

### Confirmed mechanism

`CompletePrime` in `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (lines 341 to 355 in this worktree) runs as a `ContinueWith` continuation on `TaskScheduler.Default` with `TaskContinuationOptions.None`. On any outcome other than ran-to-completion it executes, in this order:

1. `_primeTasks.TryRemove(engineName, out _)` (line 348) — clears the in-flight marker.
2. Unwraps or synthesizes the failure exception (lines 350 to 352).
3. `_logError(BuildPrimeFailedMessage(engineName), failure)` (line 354) — reports the fault.

`GetPrimeTask` (lines 247 to 255) returns the stored continuation when the marker is present and `Task.CompletedTask` when it is absent. The flaky test fetches the handle after the trigger: it calls `probe.SetException(failure)` at line 223 and only then calls `GetPrimeTask` at line 224.

Because `SetException` runs the awaiting state machine inline on the test thread and the state machine then queues the continuation to the thread pool, the test thread and a pool thread run concurrently from that point. If the pool thread reaches step 1 before the test thread's `TryGetValue`, the test awaits `Task.CompletedTask`, returns synchronously, and asserts on `harness.Errors` before (or while) the pool thread performs step 3. The assertion `Errors.Should().ContainSingle()` then fails on an empty list, or, if the read overlaps the list append, the indexer returns null and the message assertion throws. That is the CI failure.

### Contract violated

The documentation on `GetPrimeTask` (lines 238 to 246) states that the handle is "exposed so tests can await the prime deterministically instead of polling or sleeping" and that a prime fault "is observed inside the prime itself and reported through logError". The second sentence is true only for a caller that already holds the continuation. A caller that observes the marker absent has no guarantee the report has happened, because removal precedes the report in program order. The invariant the fix restores is stated in one sentence: **for a key whose prime did not run to completion, the in-flight marker is present until the fault report has returned, so any caller that observes the marker absent observes a report that has already completed.**

### Original hypothesis refuted

The issue's suspected cause ("the prime fault is probably observed and logged in a continuation that is not part of the awaited task") is refuted. `StartObservedPrime` (lines 290 to 303) returns the `ContinueWith` continuation itself, `StartPrimeIfNeeded` stores exactly that at line 276, and `GetPrimeTask` returns the stored value. The fault observer is inside the awaited task. The defect is the order of two statements inside that observer, not a missing await.

### Why the window is real

The window between removal and the completed list append is ordinarily microseconds, but on the first execution in a process it includes JIT compilation of the message builder, the injected log lambda, the `LoggedError` constructor and the list append, plus culture-data initialisation for the current-culture string format. Under the parallel run regime with other classes on the same runner, the test thread can be preempted after `SetException` and the pool thread can be preempted inside that window. One failure with passes on rerun and locally is consistent with this.

### Why this cannot be made deterministic from the test side alone

Capturing the handle before the trigger (the pattern already used by `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker` in the Race partial) makes that one test immune, but it leaves the documented contract false on the failure path: every other post-trigger call, including the cleanup awaits already present at line 242 and in the Race partial, can still return early. It also cannot serve as a fails-before, passes-after regression test, because it changes the observation rather than the behavior.

## Proposed Fix

### Design summary (what changes where):

1. **Production, `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, `CompletePrime`.** Move `_primeTasks.TryRemove(engineName, out _)` to after the `_logError(...)` call, so the order becomes: early return on ran-to-completion; compute `failure`; report through `_logError`; remove the marker. Update the `summary` element so its prose lists the report before the marker removal, and add a one-sentence comment stating why the order is load-bearing: the report precedes the removal so that any holder of the prime handle, and any caller that observes the marker gone, is guaranteed the fault has already been reported. Tighten the `returns` element of `GetPrimeTask` with one sentence stating that guarantee. No other member changes.
2. **Test fixture, `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs`, `Harness`.** Add an internal settable `OnLogError` property typed as an action taking the message string and the exception, documented as an optional extra observer invoked from inside the error-log sink. Invoke it from the injected `logError` lambda immediately after the `Errors.Add(...)` call, using the null-conditional invoke form already used for `OnInvalidate`. No existing test method in the file changes.
3. **New regression test, `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs`.** A third partial of the same `[TestClass]` (the class attribute stays on the first partial only, as the Race partial already does), containing `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged`. Design in the Test Strategy section.
4. **Registration, `TaskMaster.Test/TaskMaster.Test.csproj`.** Add a `Compile Include` item for the new file next to the two existing coordinator test entries (lines 352 and 359). The project uses explicit compile items; without the entry the file is not compiled and the test silently does not exist.

### Boundaries and invariants to preserve:

- The type keeps exactly one `catch` (the click boundary). `CompletePrime` remains a continuation, not a `catch`, and gains no `try` block.
- The success path is untouched: on ran-to-completion `CompletePrime` still returns before touching the marker or the sink, so the handle for a successful prime stays registered and `GetPrimeTask` keeps returning it.
- The cancellation path still synthesizes a `TaskCanceledException` when the completed task carries no exception, and the faulted path still reports the unwrapped base exception.
- The returned continuation still always completes successfully; the message text and the invalidation behavior are unchanged.
- `StartPrimeIfNeeded`, `_primeGate`, and the at-most-one-prime guard are not modified. No lock is added to `CompletePrime` (that would address hazard B, which is out of scope).
- No new constructor parameter, interface, scheduler seam, or public surface is introduced.

### Dependencies or blocked work:

- None. `GetPrimeTask` has no production caller (verified by a repository-wide search over C# sources: the only non-test hits are its declaration and a `see cref` in the `_primeTasks` documentation). The production error sink is a log4net error call that does not re-enter the coordinator.
- Hazard B is promoted as a separate issue and is not a dependency of this fix.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

- `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` — `CompletePrime` body and its documentation; `GetPrimeTask` `returns` documentation.
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` — `Harness.OnLogError` and its invocation from the log lambda.
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs` — new partial with the regression test.
- `TaskMaster.Test/TaskMaster.Test.csproj` — one new compile item.

#### Functions/classes/CLI commands impacted:

- `EngineToggleStateCoordinator.CompletePrime` (private): statement order and documentation.
- `EngineToggleStateCoordinator.GetPrimeTask` (internal): documentation only; body unchanged.
- `EngineToggleStateCoordinatorTests.Harness` (private nested): one new property; constructor lambda gains one invoke.
- `EngineToggleStateCoordinatorTests` (partial test class): one new test method in a new partial.

#### Data flow and validation changes:

- None in data flow. The only semantic change is the order in which the sink observes the fault relative to the marker removal.
- After the change, a `getPressed` poll that arrives between the report and the removal sees the marker still present and does not re-prime on that poll; the next poll re-primes. Before the change the opposite window existed (a re-prime could start, and even log, before the first fault's report), so log order could invert. Neither window is asserted by any test, and the new window is bounded by the duration of the log call.

#### Error handling and logging updates:

- No new log messages. The existing prime-failed message is emitted exactly once per failed prime, as before, but now before the marker is cleared.
- A throwing error sink would now leave the marker registered as well as faulting the continuation (before the change it only faulted the continuation). The production sink does not throw; the optional `finally` hardening is recorded as a non-goal.

#### Rollback/feature-flag considerations (if applicable):

- Not applicable. The change is two reordered statements and can be reverted by a single commit revert. No flag.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

- `GetPrimeTask(engineName)` — unchanged signature. Post-condition strengthened: when it returns `Task.CompletedTask` for a key whose prime did not run to completion, the fault report for that prime has already returned.
- `Harness.OnLogError` (test-only) — an optional action receiving the message and the exception, invoked from inside the log sink after the error has been appended to `Errors`, on whatever thread the sink runs.

#### Required configuration keys and defaults:

- None.

#### Backward-compatibility expectations:

- No public API change. `CompletePrime` and `Harness` are private; `GetPrimeTask` is internal and its signature is unchanged. The #735 acceptance wording ("the marker removal and the log call are on the non-completed path") remains satisfied because both statements stay on that path.

#### Performance constraints (latency/throughput/memory):

- No measurable change. The marker is held for the additional duration of one log call on the failure path only.

## Assumptions, Constraints, Dependencies

- Assumptions (environment, data, access): .NET Framework TPL semantics as documented — `TaskCompletionSource.SetException` runs await continuations inline on the completing thread; a `ContinueWith` registered with `TaskContinuationOptions.None` is queued to the default scheduler; `ConcurrentDictionary.TryRemove` publishes under a lock with volatile writes and `TryGetValue` performs a volatile read, so observing the removal acquires all writes the removing thread made before it (including the list append performed by the sink). The MSTest version pinned by the test project's packages.config runs test bodies without a synchronization context.
- Constraints (budget, performance, compatibility): The main fixture file is at 459 of the 500-line ceiling in this worktree; the harness hook adds roughly eight lines. The new test goes in a separate partial for that reason. All C# work must pass the toolchain in CLAUDE.md order. Tests must use MSTest, Moq, and FluentAssertions, and must not add sleeps, delays, retries, timeouts, blocking waits, `[DoNotParallelize]`, or temporary files.
- External dependencies (services, libraries, releases): none beyond packages already referenced by the test project.

## Data / API / Config Impact

- User-facing or API changes: none.
- Data or migration considerations: none.
- Logging/telemetry updates (if any): none in content; ordering of the existing prime-failed log relative to internal marker removal changes as described.
- Compatibility notes (CLI flags, config schemas, versioning): none.

## Test Strategy

Seeded from issue:

- [x] Write a regression test that forces the ordering deterministically (for example a controllable scheduler or a `TaskCompletionSource` gate), then fix the coordinator or the test seam. — Resolved by the in-sink probe design below; a scheduler seam and a blocking gate were both considered and rejected (research record, sections 6 and 8.4).
- [x] Negative control: show that the test fails when the ordering is inverted. — Resolved by the fail-before run described below; the ordering under test is the statement order inside `CompletePrime`.

- Regression tests to add or update:
  - Add `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged` in `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs`. Discriminator: the state of the prime marker at the moment the sink is invoked. That moment is after the removal in the current code and before the removal in the fixed code, on the same thread, so the outcome is a function of program order, not scheduling.
    - Arrange: `new Harness()`; a held `TaskCompletionSource<bool>` named `probe`; `InvalidOperationException` named `failure`; strict mock setup `EngineActiveAsync(SpamEngine)` returning `probe.Task`; `harness.Coordinator.GetPressed(SpamEngine)` to start the prime; `var prime = harness.Coordinator.GetPrimeTask(SpamEngine)` captured before the trigger so the await itself is deterministic; a `Task` local `handleSeenBySink` initialised to null; `harness.OnLogError = (_, _) => handleSeenBySink = harness.Coordinator.GetPrimeTask(SpamEngine)`.
    - Act: `probe.SetException(failure)`; `await prime`.
    - Assert: `handleSeenBySink.Should().BeSameAs(prime, ...)` with a reason stating that while the fault is being reported the prime handle must still be registered so that a caller fetching it after the trigger awaits the report; `harness.Errors.Should().ContainSingle()`; `harness.Errors[0].Message.Should().Contain(SpamEngine)`; `harness.Errors[0].Exception.Should().BeSameAs(failure)`; `harness.Invalidations.Should().BeEmpty()`; `harness.Coordinator.GetPrimeTask(SpamEngine).Should().BeSameAs(Task.CompletedTask, ...)` with a reason stating that once the handle has completed the marker has been cleared so a later read may re-prime.
    - Thread safety of the test itself: `handleSeenBySink` is written on the pool thread before the continuation completes and read on the test thread after `await prime`; task completion supplies the happens-before edge. No blocking, gate, timer, sleep, delay, temporary file, or parallelism attribute.
    - Documentation: an XML summary on the test naming issue #942 and stating the invariant, plus an in-file comment stating that if this test passes without the production reorder, isolation of the negative control has been lost and the run must be investigated rather than accepted.
  - Modify the `Harness` in `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` to expose `OnLogError`; no existing test method is edited.
  - Leave `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` unchanged. Under the fixed order it is deterministic: if it obtains the continuation it awaits the report; if it obtains `Task.CompletedTask` it has observed a removal that the report preceded in program order, and the volatile read acquires the list append.
- Unit tests (pytest) for the fixed behavior and boundaries: not applicable (C# / MSTest). The new test, the existing fault test, and the two existing cancellation tests in the Race partial together exercise every statement of the changed method on both the faulted and canceled outcomes.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
  - Cancellation outcome: covered by the existing `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker` and `GetPressed_WhenPrimeIsCanceled_LeavesToggleReportingUnchecked`, which pass unchanged because `CompletePrime` is a single path for both outcomes. A cancellation twin of the new test is optional and not required.
  - Success outcome: covered by the existing prime-success tests; the early return is untouched.
  - Null or empty engine key on `GetPrimeTask`: body unchanged; existing constructor and key tests are unaffected.
- Error handling and logging verification: the new test asserts exactly one logged error carrying the injected exception and a message containing the engine name, with no invalidation. The existing fault test asserts the same on the original observation path.
- Coverage impact and targets for changed lines/modules: the reorder changes no branch structure. The changed lines in `CompletePrime` are exercised by the new test, the existing fault test, and the two cancellation tests, so line coverage of the changed lines must not decrease relative to the baseline and the method is expected to remain fully covered. Test files are excluded from the coverage denominator. Record a baseline projection before the change and a post-change projection after it, with an explicit comparison of the coordinator file's line and branch figures:
  - `docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/baseline/coverage-baseline.md`
  - `docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/qa-gates/coverage-post-change.md` (includes the comparison against the baseline for the coordinator file and the first-party total)
- Toolchain commands to run (format → lint → type-check → test), exactly as in CLAUDE.md, restarting from the first step if any step changes files or fails:
  1. `dotnet tool run csharpier format .` then verify with `dotnet tool run csharpier check .`
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. Run the `test: MSTest with Coverage (Koverage)` VS Code task, or invoke Invoke-MSTestWithCoverage.ps1 under scripts/vscode directly.
  The final passing pass is recorded as a projection with Timestamp, Command, and EXIT_CODE fields at `docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/qa-gates/toolchain-final-pass.md`. For the two msbuild steps the projection must also state that no project reported a skipped CoreCompile target, so the analyzer and nullable gates are known to have compiled rather than short-circuited.
- Fail-before / pass-after evidence (bugfix workflow, regression test first):
  - Fail-before: with the harness hook, the new partial, and its csproj entry in place and the production reorder NOT applied, run the coordinator test class (a vstest invocation filtered to the fully qualified test class name is sufficient). The new test fails on the `BeSameAs(prime)` assertion every time, because the sink runs after the removal and observes `Task.CompletedTask`. Record the projection with a non-zero EXIT_CODE and `ExpectedExitCode` set to that non-zero value at `docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/regression-testing/prime-fault-ordering-fail-before.md`. The projection must state the environment of the control explicitly: production file unchanged from the merge base (cite the commit), hook present, new test present, run settings unchanged.
  - Pass-after: apply the reorder and rerun the same command. The new test and `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` both pass, and every other test in the class passes. Record the projection with EXIT_CODE zero at `docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/evidence/regression-testing/prime-fault-ordering-pass-after.md`, including a statement that the only production difference between the two runs is the statement reorder in `CompletePrime` (the harness hook and the new test are test-side and were present in both runs).
  - An equivalent additional demonstration, if wanted, is to temporarily invert the two statements after the fix and observe the same deterministic failure; it does not replace the fail-before run.
  - Per CLAUDE.md "Committed Test Evidence Format", only Markdown projections are committed. No raw TRX, Cobertura XML, or coverage file is added to the repository under any path.
- Manual validation steps (if required): none. The defect is not observable from the Outlook UI.

## Acceptance Criteria

- [ ] AC1 — In `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, `CompletePrime` invokes the injected error-log delegate before it removes the engine key from the in-flight prime dictionary on every outcome other than ran-to-completion; the early return on ran-to-completion, the base-exception unwrap, and the synthesized `TaskCanceledException` for a canceled prime are unchanged; and no `catch`, `try`, or lock is added to the method.
- [ ] AC2 — The `summary` documentation on `CompletePrime` describes the report-then-clear order, a comment adjacent to the two statements states why the order is load-bearing (a caller that observes the marker absent is guaranteed the fault has already been reported), and the `returns` documentation on `GetPrimeTask` states that guarantee in one sentence.
- [ ] AC3 — The private `Harness` in `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` exposes an internal settable `OnLogError` hook typed as an action receiving the message and the exception, documented as an optional observer invoked from inside the error-log sink, and the injected log lambda invokes it with the null-conditional form immediately after appending to `Errors`; no existing test method in that file is modified (the diff for the file contains hunks only inside the `Harness` type).
- [ ] AC4 — A new partial `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs` contains the test `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged`, which captures the prime handle from `GetPrimeTask` before triggering the fault, assigns `OnLogError` to record `GetPrimeTask` from inside the sink, faults the held completion source, awaits the captured handle, and asserts that the handle seen by the sink is the same instance as the captured handle, that `Errors` contains exactly one entry whose exception is the injected failure and whose message contains the engine name, that `Invalidations` is empty, and that `GetPrimeTask` after the await returns `Task.CompletedTask`.
- [ ] AC5 — The new test uses MSTest attributes, a strict Moq mock of the engines interface, and FluentAssertions with reason strings; it is organised as Arrange, Act, Assert; it carries an XML summary naming the issue and the invariant; and it carries an in-file comment stating that a pass without the production reorder means the negative control has lost isolation.
- [ ] AC6 — `TaskMaster.Test/TaskMaster.Test.csproj` contains an explicit `Compile Include` item for the new partial, and the pass-after projection lists `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged` as an executed, passed test (the test is not silently absent from the run).
- [ ] AC7 — Fail-before evidence exists as the projection prime-fault-ordering-fail-before.md under the feature folder's regression-testing evidence directory (full path in Test Strategy), recording a run in which the harness hook, the new partial, and its csproj entry are present and the production reorder is absent; the projection carries Timestamp, Command, a non-zero EXIT_CODE, a matching `ExpectedExitCode`, the merge-base commit of the unchanged production file, and shows the new test failing on the same-instance assertion of the sink-observed handle.
- [ ] AC8 — Pass-after evidence exists as the projection prime-fault-ordering-pass-after.md under the same directory, recording the same command after the reorder with EXIT_CODE zero, listing both `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged` and `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` as passed, and stating that the only production difference between the two runs is the statement reorder in `CompletePrime`.
- [ ] AC9 — Every test method in the coordinator fixture across all its partials passes in the pass-after run, and the text of `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` is byte-for-byte unchanged from the merge base.
- [ ] AC10 — Neither the new partial nor the `Harness` change introduces `Thread.Sleep`, `Task.Delay`, a retry loop, a wall-clock read, a timeout attribute, `[DoNotParallelize]`, a blocking wait inside the sink, a temporary file, or a scheduler seam; and the repository run-settings files that configure the parallel test regime are not modified.
- [ ] AC11 — The final toolchain pass, recorded in the projection toolchain-final-pass.md under the feature folder's qa-gates evidence directory, shows `dotnet tool run csharpier check .` reporting no differences, both CLAUDE.md msbuild rebuild commands (analyzers enabled with code style enforced; warnings treated as errors) exiting zero with no project reporting a skipped CoreCompile target, and the MSTest-with-coverage run exiting zero, all in one uninterrupted pass in the CLAUDE.md order.
- [ ] AC12 — Coverage evidence exists as the baseline projection coverage-baseline.md under the feature folder's baseline evidence directory and the post-change projection coverage-post-change.md under its qa-gates evidence directory; the post-change projection compares the coordinator file's line and branch figures against the baseline and shows that coverage of the changed lines in `CompletePrime` did not decrease and that the method remains fully covered; and the diff adds no raw test-result or coverage document (no file with a trx, xml, or coverage extension) anywhere in the repository.
- [ ] AC13 — The diff against the merge base modifies no repository file outside the four code files listed in the Write Set and this feature folder; in particular the second existing test partial, the ribbon controller engine-command wiring, and the run-settings files are untouched, and `StartPrimeIfNeeded` and the prime gate lock are unchanged (hazard B remains out of scope).
- [ ] AC14 — Each of `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs`, and `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs` is at or below the five-hundred-line ceiling after the change.

## Risks & Mitigations

- Technical or operational risks:
  - A throwing error sink would leave the marker registered (before the change it only faulted the continuation). The production sink is log4net's error method, which does not throw on appender failure. Mitigation: documented as a non-goal; the optional `finally` hardening can be added in a follow-up if a throwing sink is ever introduced.
  - A `getPressed` poll arriving during the log call now observes the marker present and does not re-prime on that poll. Mitigation: the next poll re-primes; the window is bounded by one log call and no test or production behavior depends on re-priming within it.
  - Hazard B (registration racing removal on synchronous non-success completion) remains. Mitigation: promoted separately; this change neither widens nor narrows it, and the planner must not attempt to fold it in.
  - The main fixture file is close to the line ceiling. Mitigation: the new test lives in a separate partial; AC14 gates the ceiling.
- Mitigations and rollbacks: single-commit revert restores the previous order; no configuration or data migration is involved.

## Rollout & Follow-up

- Release/rollout steps: merge through the normal PR gate; no deployment step beyond the next add-in build.
- Post-fix monitoring or clean-up tasks: watch the required MSTest-with-coverage check on subsequent PRs for any recurrence of a failure in the coordinator test class; none is expected.
- Links: issue #942; PR #939 (first observed failure); issue #735 code review note NB-2 (hazard B); the separately promoted issue for hazard B (number recorded by the coordinator at promotion time).
