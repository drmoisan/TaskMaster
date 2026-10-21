# Research — Issue #948: a permanent configuration fault is logged on every cache-miss poll

- **Issue:** #948 (https://github.com/drmoisan/TaskMaster/issues/948)
- **Feature folder:** `docs/features/active/2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll-948/`
- **Branch / anchor:** `bug/engine-toggle-permanent-config-fault-logs-every-poll-948`, cut from `origin/main` `9b3eea58447c264eae6f95a4bfee3bfcec7fb17f`
- **Researched:** 2026-10-01T07-20
- **Mode:** research only; no source file was modified
- **Evidence tags:** `[V]` verified by reading the named file in this worktree; `[D]` derived by reasoning from `[V]` facts; `[N]` not verifiable in this session (stated as such). The Bash tool was unavailable in this session, so no `git` command was run; commit-level claims are verified from file content instead.

All line numbers refer to the files in this worktree at the anchor.

## 1. Current state and defect mechanism (Q1)

### 1.1 Where `EngineActiveAsync` gets its configuration `[V]`

- `AppItemEngines.EngineActiveAsync` (`TaskMaster/AppGlobals/AppItemEngines.cs:101-109`) begins with `var configs = await Globals.AF.Manager.Configuration;` and then performs a non-throwing `TryGetValue`. The only awaited operation is the configuration load.
- `ManagerAsyncLazy.Configuration` (`UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs:54-58`) is an `AsyncLazy<ConcurrentDictionary<string, SmartSerializableLoader>>`, assigned only by `ResetConfigAsyncLazy()` (`:94`, `Configuration = new(ReadConfiguration)`).
- `AsyncLazy<T>` (`UtilitiesCS/ReusableTypeClasses/AsyncLazy/AsyncLazy.cs`) wraps a `Lazy<Task<T>>` (`:24`) whose value is `Task.Run(factory)` (`:51` for the `Func<Task<T>>` constructor used here). `GetAwaiter()` returns `instance.Value.GetAwaiter()` (`:67-70`). `Lazy<T>` caches the produced `Task<T>` object, so once `ReadConfiguration` faults, every later `await Configuration` re-awaits the same faulted task and rethrows synchronously. `EngineActiveAsync` therefore returns an already-faulted task on every call for the rest of the lazy's lifetime.
- `ResetConfigAsyncLazy()` has exactly two production invocation sites, both inside `ManagerAsyncLazy` itself: the constructor (`:41`) and `ResetLoadManagerAsyncLazy` (`:329`), where it runs only when `Configuration is null`, which cannot be true after construction. No production code resets a faulted configuration; the only other callers are tests (`UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage_Tests.ManagerAndAdditional.cs:186,199`). See Numeric Derivation Evidence, claim 2.
- The coordinator cannot reach the reset: `IAppItemEngines` (`UtilitiesCS/Interfaces/IGlobals/IAppItemEngines.cs:7-17`) exposes `InboxEngines`, `ToggleEngineAsync`, `EngineActiveAsync`, `ShowSaveInfo`, `ShowDiskDialog`, `RestartEngineAsync`, `InitAsync`, and nothing that touches `ManagerAsyncLazy`.

### 1.2 A transient first fault is structurally possible `[V]`/`[D]`

`ApplicationGlobals.cs:117-120` constructs `_autoFileObjects` and then `Engines = new AppItemEngines(this)` in the globals constructor, whereas `AppAutoFileObjects.Manager` is assigned only in the load paths (`TaskMaster/AppGlobals/AppAutoFileObjects.cs:68,86`; auto-property at `:615`). In the window where `Engines` is non-null but `Manager` is still null, `Globals.AF.Manager.Configuration` throws `NullReferenceException`. Whether a ribbon `getPressed` poll actually lands in that window was not verified `[N]`; the consequence for design is in §2.3.

### 1.3 The coordinator's re-prime loop `[V]`

`TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (442 lines):

1. `GetPressed` (`:137-151`): unmapped key returns false (`:139-142`); cache hit returns the cached value (`:144-147`); otherwise `StartPrimeIfNeeded` (`:149`) and return false.
2. `StartPrimeIfNeeded` (`:264-289`): returns when the engines accessor yields null (`:266-270`); under `lock (_primeGate)` (`:272`) returns if `_primeTasks.ContainsKey(engineName)` (`:274-277`); otherwise registers a `TaskCompletionSource<bool>` marker (`:283-286`, the #944 change, with its explanatory comment at `:279-282`) and calls `StartObservedPrime` (`:287`).
3. `StartObservedPrime` (`:303-327`): `ApplyPrimeAsync(...).ContinueWith(completed => { try { CompletePrime(completed, engineName); } finally { marker.SetResult(true); } }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default)`; the continuation task is discarded.
4. `ApplyPrimeAsync` (`:334-349`): takes a ticket, awaits `engines.EngineActiveAsync` (`:343`), stores through `TryApplyState`, invalidates on a real change. No `catch`.
5. `CompletePrime` (`:366-382`): returns on `RanToCompletion` (`:368-371`); otherwise unwraps the base exception or synthesizes `TaskCanceledException` (`:373-375`), calls `_logError(BuildPrimeFailedMessage(engineName), failure)` (`:380`), then `_primeTasks.TryRemove(engineName, out _)` (`:381`).
6. `GetPrimeTask` (`:250-258`): returns the registered marker or `Task.CompletedTask`; its `<returns>` (`:243-249`) promises that a caller receiving `Task.CompletedTask` "can rely on the fault having been reported".

Loop `[D]`: after a faulted prime, `TryRemove` (`:381`) clears the marker; the cache (`EngineTogglePressedStateCache`, `TaskMaster/Ribbon/EngineTogglePressedStateCache.cs:70-80`) still has no entry for the key, so the next `GetPressed` misses (`:144`), reaches `StartPrimeIfNeeded`, finds no marker (`:274`), starts a new prime, which re-awaits the cached faulted task and reaches `:380` again. One `logError` call per completed prime cycle, unbounded. Polls that arrive while a marker is registered (between `:286` and `:381`) are coalesced by `ContainsKey`, so the rate is bounded only by how fast the thread pool runs the continuation; with a synchronously faulted task that is effectively every poll. Before #944 a stale marker intermittently blocked re-primes and so masked this (`#944` research §8 item 2, spec "Rollout & Follow-up" item 2, `docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/spec.md:288`).

### 1.4 Production wiring `[V]`

`TaskMaster/Ribbon/RibbonController.EngineCommands.cs:67-77` constructs the coordinator lazily (`??=`), so one instance lives for the `RibbonController` lifetime; the `logError` sink is `(message, exception) => logger.Error(message, exception)` (`:76`). `IsEngineToggleActive` calls `GetPressed` (`:90`); `HandleEngineToggleClickAsync` calls `HandleToggleClickAsync` (`:105`). No production code calls `GetPrimeTask`.

### 1.5 A property that shapes the design: the pressed-state cache is never cleared `[V]`/`[D]`

`EngineTogglePressedStateCache` has `NextSequence`, `TryGetActive`, `TryApplyState` and no remove or clear member (`:57`, `:70`, `:98`). `_pressedState` in the coordinator is `readonly` (`:69-70`). Therefore once a key has a cached value (from a successful prime or a successful toggle) `GetPressed` is a cache hit for the rest of the coordinator's lifetime and `StartPrimeIfNeeded` is never reached again for that key. Consequently a "fault episode" for a key can only end in a success after which no further prime, and so no further prime fault, can occur for that key. Any "reset suppression on success" logic would be unobservable through the type's surface (§2.2, §5.2).

### 1.6 Test fixture `[V]`

Four partials of `TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests`, all registered as explicit `<Compile Include>` items in `TaskMaster.Test/TaskMaster.Test.csproj:352,359,360,361`:

| File | Lines | Methods | Content |
|---|---|---|---|
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` | 470 | 16 (18 cases; one `DataTestMethod` with 3 rows) | constructor contracts, `GetPressed`, toggle ordering, click boundary; private `Harness` (strict `Mock<IAppItemEngines>`, `Invalidations`, `Notifications`, `Errors`, `OnInvalidate`, `OnLogError`) and `LoggedError` |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs` | 277 | 6 | #735 last-writer race, CR-3 guard, CR-2 canceled prime |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs` | 77 | 1 | #942 report-then-clear |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs` | 175 | 3 | #944 register-before-start and re-prime guards |

Totals: 26 methods, 28 cases (Numeric Derivation Evidence, claim 1). The #944 baseline evidence recorded 25 cases executed before the three #944 tests existed (`.../944/evidence/baseline/coordinator-tests-baseline.md:6`), consistent with 28 now.

## 2. Candidate policies (Q2)

Evaluation criteria: satisfies "logged once, or at a bounded rate, and recovery is either explicit or backed off"; preserves the at-most-one-prime invariant, report-then-clear ordering and the `GetPrimeTask` contract; keeps recovery reachable; single production file; no new package dependency; deterministic tests without a clock; size.

| Policy | Bound on log volume | Recovery path kept | Files | New dependency | Deterministic RED test | Breaks existing tests |
|---|---|---|---|---|---|---|
| (a) suppress repeat reports per key (and failure type), keep re-priming | once per key per distinct failure type | yes, implicit: the next poll re-primes and succeeds as soon as the source succeeds | 1 production + 1 new test partial + csproj | none | yes | none |
| (b) time-based back-off of re-primes via injected `TimeProvider` | once per back-off interval, unbounded over time | yes, delayed by up to one interval | 1-2 production (ctor signature; `RibbonController.EngineCommands.cs` if the default is not supplied inside the coordinator) + tests | none (packages already present, §2.4) | yes, with `FakeTimeProvider` | none, but changes the 4-argument constructor contract |
| (c) stop re-priming after a fault until an explicit reset | once per key | no production caller exists for an explicit reset (§1.1); the toggle stays unchecked for the session even after a transient fault | 1 production + new reset API + a caller that does not exist | none | yes | yes: four tests (§5.3) |

### 2.1 Recommendation: (a), keyed by engine key and failure type

Add to the coordinator a `ConcurrentDictionary<(string EngineName, Type FaultType), byte>` of prime failures already reported. In `CompletePrime`, after computing `failure`, report through `_logError` only when the `(engineName, failure.GetType())` pair is absent, record the pair immediately after the sink returns, then clear the marker as today. Re-primes are unchanged, so recovery stays automatic: a later poll that finds a succeeding source (because the configuration eventually loads, because `ResetConfigAsyncLazy` is ever wired to a caller, or because a transient fault clears) caches the value and invalidates the control.

Why key on failure type rather than engine key alone: §1.2 shows a structural window in which the first prime fault for a key can be a transient `NullReferenceException`; with a per-key flag the later permanent configuration fault would never be logged. Keying by `(key, type)` logs each distinct failure kind once, which is still a hard bound (the set of exception types a prime can produce is finite), costs one tuple key instead of a string key, and gives the regression suite an observable "a new kind of failure logs again" path (§7, test 4). The cached `AsyncLazy` fault always carries the same exception instance and therefore the same type, so the production scenario in the issue produces exactly one log entry. A canceled prime synthesizes a fresh `TaskCanceledException` each cycle (`:375`), which is also one type and so also logged once.

Why not reset the suppression on a later success: §1.5 shows no further prime can run for a cached key, so a reset would be unobservable and untestable; it is omitted. If a cache-invalidation feature is ever added, the reset belongs with it.

Why report the first occurrence at all rather than, for example, counting: the issue's expected behaviour is satisfied by "once"; a count or a periodic summary needs a clock or a second sink and adds no information an operator can act on beyond the first entry.

### 2.2 Rejected alternatives (brief)

- **Per-key flag only** (`ConcurrentDictionary<string, byte>`): simplest, hard bound of one per key per lifetime, but hides a later permanent fault behind an earlier transient one (§1.2). Kept as the fallback if the maintainer prefers the stricter bound; the difference from the recommendation is the dictionary key type and one `GetType()` call.
- **Suppress only when the same exception instance repeats** (`ReferenceEquals` with the last reported): targets the `AsyncLazy` mechanism precisely but gives no bound for a source that produces a fresh exception per call, and the canceled path synthesizes a fresh exception every cycle, so it would log every canceled prime.
- **(b) time-based back-off**: viable and dependency-free (§2.4), but it still logs without bound over time (one entry per interval, all identical), delays recovery by up to one interval, needs a constructor change plus a policy constant, and is strictly more code than (a). Not minimal. Could be layered on later if the per-poll re-prime itself (not the logging) is ever measured to be a cost; today a re-prime against a cached fault is one dictionary probe and one synchronous rethrow.
- **(c) stop re-priming until explicit reset**: no production reset caller exists (§1.1), so a transient startup fault would freeze the toggle at unchecked for the session; it also reverses the user-visible outcome #735 CR-2 and #944 established and fails four existing tests (§5.3).
- **Lower the level of repeat reports (debug instead of error)**: the sink is a single `Action<string, Exception>`; a second sink or a level parameter changes the constructor and the production wiring for no gain over suppression.
- **Skip re-priming inside `StartPrimeIfNeeded` when a fault was reported**: equivalent to (c) without the reset; same objection.

### 2.3 Conformance notes

- Rule `.claude/rules/csharp.md` "Time seam": not engaged, because the recommended design reads no clock.
- Rule `.claude/rules/csharp.md` "DI Seams": no new seam is needed; the existing injected `logError` delegate is the observation point.
- CLAUDE.md General Code Change Policy §3 (fail fast, no silent ignoring): the first occurrence of each failure kind is still reported with the full exception; what is suppressed is a repeat of an already-reported report. The first message should state the suppression so a reader of the log is not misled (§4.2 item 3).

### 2.4 Package facts relevant to (b), recorded for completeness `[V]`

`Microsoft.Bcl.TimeProvider` 10.0.12 is referenced by `TaskMaster/TaskMaster.csproj:148-149` (`TaskMaster/packages.config:11`) and by `TaskMaster.Test/TaskMaster.Test.csproj:75-76` (`TaskMaster.Test/packages.config:13`); `Microsoft.Extensions.TimeProvider.Testing` 10.10.0 is referenced by `TaskMaster.Test/TaskMaster.Test.csproj:126-127` (`TaskMaster.Test/packages.config:30`). Production precedent: `TaskMaster/AppGlobals/NonBlockingDelay.cs:65` (`WaitAsync(TimeSpan, TimeProvider)`), `TaskMaster/ThisAddIn.cs:50` (`timeProvider: TimeProvider.System`); test precedent: `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs:5,42,78` (`FakeTimeProvider`). A back-off would therefore not be a new dependency, but it is not recommended for the reasons in §2.2.

## 3. Behaviour semantics

- **Success condition:** for one engine key and one failure type, however many cache-miss polls re-prime against a source that keeps failing with that type, `logError` is invoked exactly once; every re-prime still runs (`EngineActiveAsync` is called once per completed cycle); `GetPressed` keeps returning false; no invalidation occurs.
- **Recovery:** the first prime that runs to completion caches the value and invalidates the mapped control exactly once (unchanged behaviour, `:345-348`); from then on `GetPressed` is a cache hit and no prime runs.
- **New failure kind:** a prime failure whose base-exception type differs from every type already reported for that key is reported once.
- **Toggle path:** `HandleToggleClickAsync` (`:170-186`) is untouched; a toggle fault is reported on every click as today (user-driven, bounded by clicks).
- **Ordering:** the (possibly suppressed) report precedes `TryRemove`; `marker.SetResult(true)` follows both (`:314-321`).
- **Edge cases:** null or whitespace key never reaches `CompletePrime` (`:139-142`); the canceled prime synthesizes `TaskCanceledException` and is treated as a failure kind like any other; per-key independence (`"Spam"` and `"Triage"` are the only mapped keys, `TaskMaster/Ribbon/EngineToggleCatalog.cs:51-52`) means a suppressed `Spam` fault does not suppress the first `Triage` fault.
- **Thread-safety of the check-then-record:** `CompletePrime` for a given key cannot run concurrently with itself, because a second prime for that key cannot start until the first's `TryRemove` (`:381`) has run and the record happens before it; different keys use distinct dictionary entries. No lock is taken in `CompletePrime`, which keeps the #944 research's "`CompletePrime` takes no lock" property.

## 4. Requirements mapping and design

### 4.1 State model

Per engine key: `absent` (never primed) -> `priming` (marker registered) -> on success `cached` (permanent; marker retained and complete) | on failure `absent` again with `(key, faultType)` added to the reported set. The reported set only grows; its size is bounded by the number of mapped keys times the number of distinct failure types.

### 4.2 Production changes, all in `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`

1. **Field** (next to `_primeTasks`, after `:81`): `private readonly ConcurrentDictionary<(string EngineName, Type FaultType), byte> _reportedPrimeFaults = new ConcurrentDictionary<(string, Type), byte>();` with a summary stating: prime failures already reported, keyed by engine and base-exception type; present means a later failure of the same kind for the same key is not reported again; never cleared because a key that gains a cached value never primes again (issue #948). `System` and `System.Collections.Concurrent` are already imported (`:1-2`). `ValueTuple` is available on net481 without an extra reference.
2. **`CompletePrime`** (`:366-382`): keep `:368-375` as they are; replace `:380` with a guarded report:
   - compute `var reportKey = (engineName, failure.GetType());`
   - `if (!_reportedPrimeFaults.ContainsKey(reportKey)) { _logError(BuildPrimeFailedMessage(engineName), failure); _reportedPrimeFaults[reportKey] = 0; }`
   - keep `_primeTasks.TryRemove(engineName, out _);` (`:381`) as the last statement.
   The record uses the indexer so there is no discarded `TryAdd` result for an analyzer to flag. The record is placed after the sink returns, deliberately: if the sink throws (#947) the pair is not recorded and the report is still owed.
3. **Message** (`BuildPrimeFailedMessage`, `:420-428`): append one sentence to the existing text, for example "Further failures of this kind for this engine are not logged again." Every existing assertion on the prime-failure message is `Contain(SpamEngine)` (`EngineToggleStateCoordinatorTests.cs:232`, `.Race.cs:223`, `.PrimeFaultOrdering.cs:59`), so the text change is safe.
4. **Docs:** `CompletePrime` summary and remarks (`:351-365`) gain the suppression rule and the reason the record follows the report; `GetPrimeTask` `<returns>` (`:243-249`) becomes "a caller that receives `Task.CompletedTask` can rely on the fault having been reported, or deliberately suppressed as a repeat of a failure kind already reported for that key"; the comment at `:377-379` stays accurate and needs only "(if any)" after "report".
5. **No change** to `StartPrimeIfNeeded`, `StartObservedPrime`, `ApplyPrimeAsync`, `GetPressed`, the toggle path, the constructor, or `RibbonController.EngineCommands.cs`.

Expected size: 442 + about 20 to 25 lines, below the 500-line ceiling.

### 4.3 Test-side changes

- New partial `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs` (§7), registered in `TaskMaster.Test/TaskMaster.Test.csproj` adjacent to `:361`.
- Optional comment-only correction in `.Race.cs:195-202`: the remark says the re-prime "logs a second error"; under the new policy it does not. The test's assertions are unaffected (the single-error assertion is taken before the re-prime, `:218-222`). If the planner keeps `.Race.cs` byte-identical, record the stale remark as a follow-up instead.

## 5. Concurrency and ordering (Q3)

### 5.1 Invariants and how the design preserves them

| Invariant | Where | Preserved because | Pinned by |
|---|---|---|---|
| At most one prime per key; registration precedes start | `:272-288` | untouched | `GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime` (`EngineToggleStateCoordinatorTests.cs:160`), `GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns` (`.PrimeRegistration.cs:32`) |
| Report-then-clear in `CompletePrime` | `:377-381` | the guarded report (or its deliberate skip) still precedes `TryRemove`, which stays the last statement | `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged` (`.PrimeFaultOrdering.cs:27`; first fault, so it is reported and `OnLogError` fires) |
| `GetPrimeTask` never faults; `Task.CompletedTask` implies the report has returned | `:243-258`, `:314-321` | marker still completes only in the `finally`; contract reworded to include the suppressed case (§4.2 item 4) | same test; `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` (`EngineToggleStateCoordinatorTests.cs:213`) |
| A failed or canceled prime clears its marker so a later read re-primes | `:381`, #735 CR-2, #944 | untouched; suppression never prevents a re-prime | `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker` (`.Race.cs:204`), `GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime` (`.PrimeRegistration.cs:85`), `GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime` (`.PrimeRegistration.cs:132`) |
| Exactly one `catch` in the type (the click boundary) | `:178-185` | no `catch` added | structural; `ExecuteToggleAsync_WhenToggleFaults_PropagatesUnchanged` (`EngineToggleStateCoordinatorTests.cs:297`) |
| No lock in `CompletePrime`; no await under `_primeGate` | `:59-62`, `:366-382` | dictionary operations only | structural |

### 5.2 Why the check-then-record needs no lock `[D]`

For one key, `CompletePrime` runs once per prime, and the next prime for that key cannot be registered until `TryRemove` at the end of the current `CompletePrime` has run (the `ContainsKey` guard at `:274` blocks it while the marker is present). The record precedes `TryRemove`. Therefore two `CompletePrime` invocations for the same key are strictly ordered, and the `ContainsKey`-then-indexer sequence cannot interleave with another for the same key. Different keys touch different entries of a `ConcurrentDictionary`. A toggle completing concurrently does not touch `_reportedPrimeFaults`.

### 5.3 Existing tests under the recommended policy

- All 26 methods / 28 cases remain green `[D]`: no existing test asserts that a second prime failure for the same key is logged again. The three tests that re-prime after a failure assert the single error before the re-prime (`EngineToggleStateCoordinatorTests.cs:227-231`, `.Race.cs:218-222`) or follow the failure with a success (`.PrimeRegistration.cs:118,164`). The cleanup re-primes at `EngineToggleStateCoordinatorTests.cs:242` and `.Race.cs:272` are now suppressed reports, which no assertion observes.
- Comment drift only: `.Race.cs:195-202` (see §4.3).
- Under policy (c) the following would fail: `.Race.cs:204` (`NotBeSameAs(firstPrime)` at `:240-245`), `.Race.cs:253` (its cleanup `await` would be a stale marker that never completes if the marker were retained, or `Task.CompletedTask` if a flag were used; either way the premise of the test changes), `.PrimeRegistration.cs:85` and `:132` (`Times.Exactly(2)` at `:103-107`, `:149-153`). This is the evidence that (c) is not a compatible policy.

## 6. Interaction with sibling issue #947 (Q4)

#947 (throwing `logError` sink leaves a stale marker) is described by the #944 research §8 item 1 and spec `:278,287`; its promoted record is not present in this worktree (`docs/features/potential/promoted/` holds three `2026-09-30-*` files, none for #947, and `Issue: #947` matches nothing under `docs/features`) `[V]`. Both issues edit the failure branch of `CompletePrime` (`:373-381`).

- **No behaviour change for #947's scenario under this fix:** a throwing sink throws on the first report, which this fix never suppresses, so the marker still stays registered exactly as today and no further prime (hence no further sink call) occurs for that key. The record is placed after the sink returns, so a throwing sink leaves the `(key, type)` pair unrecorded; the report remains owed, which is the correct state for #947 to act on.
- **Keeping the later #947 fix composable:** the expected #947 shape is to guarantee `TryRemove` runs whether or not the sink throws (for example `try { report } finally { TryRemove }`, or catching and rethrowing after removal). Lay out this fix so that the guarded report and its record form one block that such a wrapper can enclose without reordering: `if (!ContainsKey) { _logError(...); record; }` followed by `TryRemove`. Two constraints for #947 to respect, which this fix should state in the `CompletePrime` remarks: the record must stay after the sink returns (not before it, or a throwing sink suppresses the report for the session), and the record must not move into a `finally`.
- **Merge mechanics:** both changes touch the same ten-line region, so a textual conflict is likely whichever lands second; the semantic merge is mechanical given the layout above. Recommend #948 land first (it is in flight) and #947 rebase.

## 7. Deterministic regression tests (Q5)

File: `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs`, a fifth partial reusing `Harness`, `LoggedError`, `SpamEngine`, `SpamToggleControlId`; add `private const string TriageEngine = "Triage";` locally if test 5 is included (the main fixture has no Triage constant). Register in `TaskMaster.Test.csproj` after `:361`. Estimated 170 to 220 lines; the main fixture (470) must not grow. MSTest, Moq strict harness, FluentAssertions; no sleep, delay, timer, wall clock, temp file, `[DoNotParallelize]` or worker count change; each test builds its own `Harness`, so ClassLevel parallelism (`scripts/vscode/TaskMaster.cli.runsettings:4-7`) is safe.

Mechanics shared by the tests: `harness.Engines.Setup(x => x.EngineActiveAsync(SpamEngine)).Returns(faulted)` where `faulted = Task.FromException<bool>(failure)` is one already-faulted task instance, which models the `AsyncLazy` cache exactly. Each poll is `harness.Coordinator.GetPressed(SpamEngine); await harness.Coordinator.GetPrimeTask(SpamEngine);`. After the `await`, the marker has been removed (`CompletePrime` runs before `marker.SetResult`, `:314-321`), or the handle was already `Task.CompletedTask` because the continuation had run; in both cases the next `GetPressed` starts a new prime, so `EngineActiveAsync` is called exactly once per poll. `Errors` is appended once per cycle on a pool thread, but cycles are sequential, so no concurrent mutation occurs.

1. **`GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly`** (carries the fail-before obligation). Five polls as above. Assert `Errors.Should().ContainSingle()`, `Errors[0].Exception.Should().BeSameAs(failure)`, `Errors[0].Message.Should().Contain(SpamEngine)`, `Engines.Verify(x => x.EngineActiveAsync(SpamEngine), Times.Exactly(5))` (re-primes still run), `GetPressed(SpamEngine)` false, `Invalidations` empty. Before the fix `Errors` has five entries on every run (program order, §1.3), so the first assertion fails deterministically.
2. **`GetPressed_WhenPrimeIsCanceledRepeatedly_LogsFirstCancellationOnly`**: same shape with `Task.FromCanceled<bool>(new CancellationToken(true))`; assert one error assignable to `OperationCanceledException`, `Times.Exactly(N)`.
3. **`GetPressed_AfterSuppressedFaults_LaterSuccessfulPrimeCachesStateAndInvalidatesOnce`** (recovery): `SetupSequence` faulted, faulted, `Task.FromResult(true)`; three polls. Assert `Errors.ContainSingle()`, `GetPressed(SpamEngine)` true, `Invalidations.Equal(new[] { SpamToggleControlId })`, `Times.Exactly(3)`. A fourth call cannot occur because the post-success read is a cache hit (the strict mock would otherwise return null from the exhausted sequence).
4. **`GetPressed_WhenFailureKindChanges_LogsNewKindOnce`** (the observable "logs again" path under the chosen policy): `SetupSequence` faulted(`InvalidOperationException`) twice, then faulted(`IOException`) twice; four polls. Assert `Errors.HaveCount(2)`, `Errors[0].Exception.BeSameAs(first)`, `Errors[1].Exception.BeSameAs(second)`, `Times.Exactly(4)`.
5. **`GetPressed_WhenSpamFaultIsSuppressed_FirstTriageFaultIsStillLogged`** (per-key independence): two Spam polls then one Triage poll with its own faulted setup; assert `Errors.HaveCount(2)` with messages containing `SpamEngine` and `TriageEngine` respectively.
6. **`HandleToggleClickAsync_AfterSuppressedPrimeFault_StillLogsToggleFault`** (scope of suppression): two Spam polls (one prime report), then `Engines.Setup(x => x.ToggleEngineAsync(SpamEngine)).ThrowsAsync(toggleFailure)` and `await HandleToggleClickAsync(SpamEngine)`; assert `Errors.HaveCount(2)`, `Errors[1].Exception.BeSameAs(toggleFailure)`.

Scenario coverage: positive (3), negative/error (1, 2), edge (4, 5), boundary interaction (6), concurrency (sequential by construction; the invariant tests of §5.1 remain the concurrency pins).

## 8. Toolchain facts for the plan (Q6)

- **Test project and assembly:** `TaskMaster.Test` (`TaskMaster.Test/TaskMaster.Test.csproj:16` `<AssemblyName>TaskMaster.Test</AssemblyName>`, `:35` `<OutputPath>bin\Debug\</OutputPath>`), so `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll`. Legacy non-SDK project: every new `.cs` needs an explicit `<Compile Include>`.
- **Runner scripts:** `scripts/vscode/Invoke-MSTestWithCoverage.ps1` accepts `-SearchRoot`, `-Configuration`, `-CoverageOutput`, `-NoExecute` (`:1-13`) and **no test filter parameter**; its inner vstest arguments are fixed (`:88-94`): `/Settings:<scripts/vscode/TaskMaster.cli.runsettings>`, `/InIsolation`, `/TestCaseFilter:TestCategory!=LiveOutlook`, `/ResultsDirectory:coverage\test-results`, `/Logger:trx;LogFileName=mstest-coverage-run.trx`. A scoped run (`-SearchRoot TaskMaster.Test`) skips the solution-wide 80/75 threshold assertions and emits one warning (`:284-291`); an unscoped run enforces them. `scripts/vscode/Invoke-MSTest.ps1` (no coverage) has the same fixed inner arguments (`:64-70`) and the same absence of a filter parameter.
- **Per-class run:** the #944 baseline used a direct invocation (`.../944/evidence/baseline/coordinator-tests-baseline.md:4`): `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:<dir>" "/Logger:trx;LogFileName=<name>.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"`, with vstest resolved through vswhere. `/Tests:` and `/TestCaseFilter:` are mutually exclusive on the vstest command line; use the filter form. Supply the results directory and log file name explicitly so the TRX name is predictable and host-path-free.
- **Exclusions:** the only category exclusion in the repository's scripts is `TestCategory!=LiveOutlook` (`Invoke-MSTestWithCoverage.ps1:91`, `Invoke-MSTest.ps1:67`); no shell-icon exclusion exists in `scripts/`, `.vscode/`, `.github/` or any `*.runsettings` `[V]`. The coordinator tests carry no category and do not touch Outlook or the shell, so neither concern applies to a `TaskMaster.Test`-scoped run.
- **Parallelism:** `TaskMaster.cli.runsettings` sets `Workers=0`, `Scope=ClassLevel` (`:4-7`); the new partial is part of the same class as the other four, so its tests run serially with them and in parallel with other classes.
- **Build gates:** the three CLAUDE.md commands (csharpier `format`/`check` via `dotnet tool run`, analyzer rebuild, nullable rebuild). The production file carries no `#nullable enable` directive (`:1-8`), so it is not in the nullable-as-error set; the new field type must still satisfy the analyzer rebuild.

## Automation Feasibility

Fully automatable; no human interaction is expected or required. Reasons: the defect, the fix and the regression tests live in host-neutral code (`EngineToggleStateCoordinator` has no COM, Office, WinForms or logger reference, `:36-43`); the tests are driven by already-completed or `TaskCompletionSource`-driven tasks and awaited through `GetPrimeTask`, so they need no Outlook process, no clock, no sleep and no filesystem; the fail-before and pass-after states are decided by program order (§7 test 1); every gate command is a non-interactive CLI (`dotnet tool run csharpier`, `msbuild`, `vstest.console.exe` or the scripts in §8); and the only environmental dependencies (Visual Studio Build Tools with the Test Platform, `dotnet-coverage`) were already present for the #944 run on this machine (`.../944/evidence/baseline/bootstrap-*.md`). No step requires a maintainer decision beyond the policy choice recorded in §2.1, which the planner can carry into `spec.md`.

## Numeric Derivation Evidence

### Claim 1: existing coordinator test methods = 26 (28 cases) across 4 partial files

- **Complete Family:** every MSTest test method of `TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests` (all partial files).
- **Exhaustive Search Scope:** `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests*.cs` (glob over the whole worktree for that stem returned exactly the four files listed in §1.6; `TaskMaster.Test.csproj:352,359,360,361` registers the same four).
- **Inclusion Rules:** a method decorated `[TestMethod]` or `[DataTestMethod]`.
- **Exclusion Rules:** `Harness`, `LoggedError` and their constructors (`internal`, not `public`); helper members.
- **Primary Search Strategy or Query Expression:** Grep regex `^\s*\[(TestMethod|DataTestMethod)\]`, count mode, glob `EngineToggleStateCoordinatorTests*.cs` under `TaskMaster.Test/Ribbon`.
- **Primary Member Set:** `.cs` 16, `.Race.cs` 6, `.PrimeRegistration.cs` 3, `.PrimeFaultOrdering.cs` 1.
- **Primary Count:** 26.
- **Cross-check Search Strategy or Query Expression:** Grep regex `^\s*public (async Task|void) [A-Za-z_]+\(`, content mode, same glob (method signatures rather than attributes).
- **Cross-check Member Set:** `.Race.cs` lines 38, 81, 122, 158, 204, 253; `.PrimeRegistration.cs` 32, 85, 132; `.PrimeFaultOrdering.cs` 27; `.cs` 31, 44, 62, 80, 105, 128, 143, 160, 187, 213, 250, 297, 316, 334, 355, 375.
- **Cross-check Count:** 26.
- **Member-set Comparison:** identical per file (16/6/3/1). Case count 28 follows from the single `[DataTestMethod]` at `.cs:101-105` carrying three `[DataRow]` attributes (`:102-104`); the #944 baseline's 25 executed cases before the three #944 tests were added (`coordinator-tests-baseline.md:6,12`) agrees (25 + 3 = 28).

### Claim 2: production invocation sites of `ResetConfigAsyncLazy()` = 2, both in `ManagerAsyncLazy.cs`

- **Complete Family:** every call of `ManagerAsyncLazy.ResetConfigAsyncLazy()` in non-test code.
- **Exhaustive Search Scope:** all `*.cs` in the worktree.
- **Inclusion Rules:** a call expression `ResetConfigAsyncLazy()` in a production project.
- **Exclusion Rules:** the declaration (`ManagerAsyncLazy.cs:94`), test projects (`*.Test/`), prose or identifiers inside strings.
- **Primary Search Strategy or Query Expression:** Grep `ResetConfigAsyncLazy|EngineActiveAsync|ToggleEngineAsync` (content mode, all `*.cs`), then filtering the `ResetConfigAsyncLazy` hits by hand: `ManagerAsyncLazy.cs:41` (call), `:53` (comment), `:94` (declaration), `:329` (call), `:331` (comment); `Triage_Tests.ManagerAndAdditional.cs:180,186,193,199,203` (test project).
- **Primary Member Set:** `ManagerAsyncLazy.cs:41`, `ManagerAsyncLazy.cs:329`.
- **Primary Count:** 2.
- **Cross-check Search Strategy or Query Expression:** Grep `ResetConfigAsyncLazy\(\);` (statement form), type `cs`, glob `!**/*.Test/**`.
- **Cross-check Member Set:** `ManagerAsyncLazy.cs:41`, `ManagerAsyncLazy.cs:329`.
- **Cross-check Count:** 2.
- **Member-set Comparison:** identical.

### Claim 3: files the fix creates or modifies = 3

- **Complete Family:** repository files that declare a member the fix changes, plus the new test partial, plus the project file that must register it.
- **Exhaustive Search Scope:** all `*.cs` and `*.csproj` in the worktree.
- **Inclusion Rules:** declares `CompletePrime`, `BuildPrimeFailedMessage` or `_primeTasks`; or is the new test file; or must list a new compile item.
- **Exclusion Rules:** files that only call `GetPressed`, `GetPrimeTask` or `HandleToggleClickAsync` without change (`RibbonController.EngineCommands.cs`, the four existing partials).
- **Primary Search Strategy or Query Expression:** Grep `EngineToggleStateCoordinator|GetPressed|HandleToggleClickAsync|logger\.Error` over `TaskMaster/Ribbon/RibbonController*.cs` (content) plus the earlier repository-wide grep for `EngineActiveAsync|ToggleEngineAsync`: the only file declaring the changed members is `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`.
- **Primary Member Set:** `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (modify), `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs` (create), `TaskMaster.Test/TaskMaster.Test.csproj` (register).
- **Primary Count:** 3.
- **Cross-check Search Strategy or Query Expression:** Grep `EngineToggleStateCoordinatorTests|LiveOutlook|ShellIcon|TestCaseFilter` over `TaskMaster.Test/TaskMaster.Test.csproj` and scripts (content): the csproj is the only registration point for fixture partials (`:352,359,360,361`), and no other project file references the fixture.
- **Cross-check Member Set:** same three files.
- **Cross-check Count:** 3.
- **Member-set Comparison:** identical. The optional `.Race.cs` comment correction (§4.3) is excluded from this count because it is a documentation-only edit the planner may decline.

## Anchor tokens (prefer these to line numbers in the plan)

Production (`TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`):
- `private readonly ConcurrentDictionary<string, Task> _primeTasks = new ConcurrentDictionary<` (insert the new field after this declaration's closing `);`)
- `private void CompletePrime(Task completed, string engineName)`
- `?? new TaskCanceledException(completed);`
- `_logError(BuildPrimeFailedMessage(engineName), failure);`
- `_primeTasks.TryRemove(engineName, out _);` (must remain the last statement of `CompletePrime`)
- `// Report-then-clear is load-bearing:`
- `"Reading the activation state for engine '{0}' failed; its toggle continues to "`
- `receives <see cref="Task.CompletedTask"/> can rely on the fault having been reported.`

Test project:
- `<Compile Include="Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs" />` (insert the new entry adjacent)
- Harness members relied on: `Engines`, `Coordinator`, `Errors`, `Invalidations`; constants `SpamEngine`, `SpamToggleControlId`; `Harness.OnLogError` if a sink-side probe is wanted.
- `.Race.cs` remark to reconsider: `canceled task and logs a second error.`
