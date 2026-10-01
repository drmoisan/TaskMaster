# Research: throwing `logError` sink leaves a stale prime marker (Issue #947)

- Timestamp: 2026-10-01T06-47
- Branch: `bug/engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947`
- Requirements source: `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/issue.md` (`## Acceptance Criteria`, lines 39-45)
- Evidence tags: `[V]` verified by reading the current tree with the Read/Grep tools; `[D]` derived by reasoning from verified facts and documented .NET semantics.

## 1. Current state

### 1.1 Production: `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (442 lines) `[V]`

| Element | Lines | Fact |
| --- | --- | --- |
| Sink field | 56 | `private readonly Action<string, Exception> _logError;` |
| `_primeTasks` | 78-81 | `ConcurrentDictionary<string, Task>`; doc at 72-77 says the marker is "removed by `CompletePrime` when the prime faults or is canceled, and retained after a successful prime". |
| Click boundary | 170-186 | `HandleToggleClickAsync`: the only `catch (Exception ex)` in the type (182-185); it calls `_logError` at 184 without a guard. Summary at 153-156 states it is "the only place in this type that observes a fault with a `catch` clause". |
| `GetPrimeTask` | 250-258 | Returns the registered marker or `Task.CompletedTask`. The `<returns>` doc at 243-249 states "The returned task never faults" and that a caller receiving `Task.CompletedTask` "can rely on the fault having been reported". |
| `StartPrimeIfNeeded` | 264-289 | Under `_primeGate`, registers a `TaskCompletionSource<bool>` marker with `RunContinuationsAsynchronously` (283-286) before calling `StartObservedPrime` (287). |
| `StartObservedPrime` | 303-327 | `_ = ApplyPrimeAsync(...).ContinueWith(completed => { try { CompletePrime(completed, engineName); } finally { marker.SetResult(true); } }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);`. Remarks at 294-302: "The observer is a continuation rather than a `catch` clause, so this type keeps exactly one `catch` — the click boundary. ... The continuation task itself is discarded; the value a test awaits is the marker, which the continuation completes only through `SetResult` in a `finally` ... so it never faults or cancels." |
| `CompletePrime` | 366-382 | Returns early on `RanToCompletion` (368-371); unwraps the base exception or synthesizes `TaskCanceledException` (373-375); comment 377-379 "Report-then-clear is load-bearing"; `_logError(...)` at 380; `_primeTasks.TryRemove(engineName, out _)` at 381. No `try`. |

Defect mechanics `[D]` from the facts above: if `_logError` at line 380 throws, line 381 never runs, so the marker stays registered. The lambda's `finally` (318-321) still completes the marker, so `await GetPrimeTask(...)` returns, but the continuation task created at 310-326 ends `Faulted` with the sink exception and is discarded (`_ =`), so it is never observed. `StartPrimeIfNeeded` then returns at 274-277 for every later `GetPressed` of that key.

Other facts relevant to the fix:

- The file carries no `#nullable enable` directive (grep for `#nullable` in `TaskMaster/Ribbon` returned no match) `[V]`, so the nullable gate does not apply new `CS86xx` diagnostics to it.
- `.editorconfig:27` sets `dotnet_analyzer_diagnostic.severity = suggestion` for all third-party analyzer rules; no `CA1031`, `S108`, or `S2221` entry exists `[V]`. A new `catch (Exception)` therefore cannot break the analyzer or nullable builds.
- Production sink: `TaskMaster/Ribbon/RibbonController.EngineCommands.cs:76` passes `(message, exception) => logger.Error(message, exception)` `[V]`.
- In-repo precedent for containing a failure thrown by the last-resort log channel: `TaskMaster/Ribbon/RibbonCommandBoundary.cs:96-113` (`SafeLog`: `try { _logFailure(...) } catch (System.Exception) { // Intentionally discarded: see remarks. }`, remarks at 99-102: "This is the last reporting channel. If it throws there is nowhere left to report to, so the failure is discarded deliberately rather than allowed to escape the boundary.") `[V]`. A second precedent at `RibbonViewer.cs:307-320` catches a reporter failure and logs it via `logger.Error` `[V]`.
- No code in the repository subscribes to `TaskScheduler.UnobservedTaskException` (repo-wide grep over `*.cs`: no match) `[V]`, so a faulted discarded continuation is silent in production and in the test process.

### 1.2 Tests `[V]`

| File | Lines | Content |
| --- | --- | --- |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` | 470 | Primary partial: constants `SpamEngine`, `SpamToggleControlId` (25-26); constructor, `GetPressed`, `ExecuteToggleAsync`, `HandleToggleClickAsync` tests; private `Harness` (403-452) with strict `Mock<IAppItemEngines>` (423-424), `Invalidations`/`Notifications`/`Errors` recorders, `OnInvalidate` hook (438), and `OnLogError` hook (445) invoked **after** `Errors.Add` (415-419); private `LoggedError` (457-468). |
| `...Tests.PrimeFaultOrdering.cs` | 77 | #942 regression `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged` (27-73): fetches the handle before the trigger, probes `GetPrimeTask` from inside `OnLogError`, asserts `BeSameAs(prime)` and afterwards `BeSameAs(Task.CompletedTask)`. |
| `...Tests.PrimeRegistration.cs` | 175 | #944 regressions: program-order test (32-75) and two re-prime guards using `SetupSequence` with `Task.FromException`/`Task.FromCanceled` then `Task.FromResult(true)` and `Verify(..., Times.Exactly(2))` (85-171). |
| `...Tests.Race.cs` | 277 | #735 tests; canceled-prime tests (204-273) establish the "distinct handle proves re-prime" pattern (240-245). |

Established deterministic-await pattern `[V]`: every asynchronous outcome is driven by a `TaskCompletionSource<bool>` returned from `EngineActiveAsync` and awaited through `harness.Coordinator.GetPrimeTask(SpamEngine)`; when the handle must be awaited after the marker may have been cleared, the test captures `var prime = GetPrimeTask(...)` **before** triggering completion (`PrimeFaultOrdering.cs:35`, `Race.cs:211`). No test uses `Thread.Sleep`, `Task.Delay`, `[DoNotParallelize]`, or a timeout attribute in this fixture; `[Timeout(5000)]` exists elsewhere in the project only (`AppGlobals/NonBlockingDelayTests.cs:32,74,109`).

Project wiring `[V]`: `TaskMaster.Test/TaskMaster.Test.csproj` uses explicit `<Compile Include>` items; the four partials are at lines 352 and 359-361. Runsettings in force (`scripts/vscode/TaskMaster.cli.runsettings`): `Workers=0`, `Scope=ClassLevel`; no `TestTimeout` or per-test timeout, so a hanging test would only be bounded by the `/Blame:...TestTimeout=4min` switch used in the #944 evidence command (`...-944/evidence/regression-testing/prime-registration-fail-before.md:4`), not by the settings file.

### 1.3 Prior analysis of this defect `[V]`

- #944 spec `Rollout & Follow-up` item 1 (`...-944/spec.md:287`) and #944 research §8 item 1 (`...-944/research/2026-09-30T08-00-...md:175`) describe the defect exactly as stated here and name "Production sink is `logger.Error`, so likelihood is low".
- #944 spec line 94 recorded "Single `catch` in the type. `try`/`finally` adds no `catch`" as a property of **that** change; it is a historical statement about #944 and is not modified by this item.

## 2. Candidate approaches

Evaluation criteria (from the delegation): (1) can the continuation still end `Faulted`; (2) can `GetPrimeTask` ever return a faulted task; (3) does marker-absent-implies-reported still hold; (4) CLAUDE.md C#4 compliance; (5) documentation impact.

### (a) `try { _logError(...) } finally { TryRemove }` only

- (1) Yes: the sink exception leaves `CompletePrime`, the lambda's own `finally` (318-321) completes the marker, and the continuation ends `Faulted`, discarded. AC3 (issue.md:43) fails.
- (2) No: the marker is completed only by `SetResult` in a `finally`.
- (3) Holds in the weakened form "the report has been attempted and has returned or thrown".
- (4) Compliant (no catch).
- Verdict: fixes AC1 only; insufficient alone.

### (b) Contain the sink exception inside `CompletePrime`, then clear — **recommended**

Shape (illustrative; the executor formats with CSharpier):

```csharp
// Report-then-clear is load-bearing (see existing comment). The sink is this type's last
// reporting channel: a failure inside it has nowhere else to go, and letting it escape would
// skip the clear (stale marker, issue #947) and fault the discarded continuation unobserved.
try
{
    _logError(BuildPrimeFailedMessage(engineName), failure);
}
catch (Exception)
{
    // Intentionally discarded: see remarks. Follows RibbonCommandBoundary.SafeLog.
}

_primeTasks.TryRemove(engineName, out _);
```

- (1) No `[D]`: after the change every statement in the continuation lambda is non-throwing. `CompletePrime`'s remaining throw sources are `completed.Exception?.GetBaseException()` (cannot throw), `new TaskCanceledException(completed)` (cannot throw), `BuildPrimeFailedMessage` (`string.Format` with one string argument), `_primeTasks.TryRemove(engineName, ...)` (throws only for a null key; the prime path is reached only after `EngineToggleCatalog.TryGetControlId` accepted the key, and `GetPressed_WithNullOrWhitespaceKey_...` at `Tests.cs:101-125` verifies null/whitespace keys never start a prime), and `marker.SetResult(true)` (throws only if the TCS is already completed; the continuation is its sole completer and runs once). The continuation therefore ends `RanToCompletion` and the discard is harmless.
- (2) No, unchanged.
- (3) Holds in the same form as (a): the marker is absent only after the sink has been invoked and has returned or thrown. The `GetPrimeTask` doc (247-248) should say "the report has been attempted" rather than "the fault has already been reported".
- (4) The catch is at a clear boundary: the injected sink is the type's last reporting channel, and the exception originates outside the type. "Added context" is not possible because there is no further channel; the repository already accepts this exact case with a documented rationale at `RibbonCommandBoundary.cs:99-112`. The empty block must carry a comment (satisfies Sonar S108, which is at `suggestion` severity anyway).
- (5) The "exactly one catch — the click boundary" statements at lines 153-156 and 295-296 become false and must be revised to "two catch clauses, both at reporting boundaries: the click boundary and the sink guard in `CompletePrime`". The `ExecuteToggleAsync` note at 203-204 ("contains no catch") stays true. The `CompletePrime` summary (351-356) and remarks gain a sentence on the sink guard. The `_primeTasks` doc (72-77) stays true.
- Variant: place the guard in the continuation lambda instead (wrap the `CompletePrime` call). Rejected: it would require (a)'s `finally` as well to preserve the clear, and moves the catch away from the statement whose failure it contains.
- Variant: `try { log } catch { } finally { TryRemove }`. Functionally equivalent; the `finally` is redundant once the catch exists. Prefer the simpler shape above unless the planner wants the `finally` for self-documentation; either passes the gates.

### (c) Guard the sink once at construction

Wrap the injected delegate in the constructor (`_logError = (m, e) => { try { logError(m, e); } catch (Exception) { } }`), which would also cover the unguarded call at line 184 in `HandleToggleClickAsync` (a throwing sink there propagates out of the method into the `async void` Office handler, contradicting the "never throws" contract at 167-168). Rejected for this item: it widens the change beyond `CompletePrime` (CLAUDE.md Bugfix Workflow step 2, minimal fix) and makes the constructor's stored field differ from the injected delegate, which complicates reasoning about the sink in tests. The line-184 hazard is recorded as a follow-up in §7.

### (d) Reorder clear before report

Prohibited by AC2 (issue.md:42); it would also fail `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged`.

**Recommendation: (b).** Production delta is about +10 lines plus doc edits; the file stays well under 500 lines.

## 3. Behaviour semantics after the fix

- Success path: unchanged; `CompletePrime` returns at 368-371.
- Faulted or canceled prime, sink returns normally: unchanged observable behaviour; the sink is invoked once with the unwrapped base exception or a synthesized `TaskCanceledException`, then the marker is removed, then the marker completes.
- Faulted or canceled prime, sink throws: the sink is invoked once (and its side effects before the throw persist), its exception is discarded, the marker is removed, the marker completes `RanToCompletion`, the continuation completes `RanToCompletion`. A later `GetPressed` for the key starts a new prime.
- Ordering invariant (unchanged): a caller observing `GetPrimeTask(key) == Task.CompletedTask` after a failed prime is guaranteed the sink invocation has already returned or thrown.
- Edge: `_logError` throwing on the success path is impossible (the sink is not invoked there).

## 4. Requirements mapping

### 4.1 Production changes (one file)

1. `CompletePrime` (366-382): wrap line 380 in `try`/`catch (Exception)` with a rationale comment; keep line 381 after the block. Extend the comment at 377-379 with the #947 reason.
2. Doc edits: `HandleToggleClickAsync` summary (153-156); `StartObservedPrime` remarks (294-302); `CompletePrime` summary/remarks (351-365); `GetPrimeTask` returns (246-248: "reported" to "attempted").
3. No change to `StartPrimeIfNeeded`, `StartObservedPrime` code, `GetPrimeTask` code, or the constructor.

### 4.2 Test design

Fixture reuse: the `Harness.OnLogError` hook (`Tests.cs:445`) runs after `Errors.Add`, so a throwing hook both records the report and models a throwing sink without any harness change. New partial recommended (see §4.4).

**Test A — re-prime after a throwing sink (AC1, carries the fail-before obligation; faulted variant).**

- Arrange: `harness`; `probe = new TaskCompletionSource<bool>()`; `Engines.SetupSequence(x => x.EngineActiveAsync(SpamEngine)).Returns(probe.Task).Returns(Task.FromResult(true))`; `harness.OnLogError = (_, _) => throw new InvalidOperationException("sink failed")`; `harness.Coordinator.GetPressed(SpamEngine)`; `var firstPrime = harness.Coordinator.GetPrimeTask(SpamEngine)` (captured before the trigger, per the established pattern).
- Act: `probe.SetException(failure)`; `await firstPrime`; `harness.Coordinator.GetPressed(SpamEngine)`; `var secondPrime = harness.Coordinator.GetPrimeTask(SpamEngine)`.
- Assert: `Engines.Verify(x => x.EngineActiveAsync(SpamEngine), Times.Exactly(2), ...)`; `secondPrime.Should().NotBeSameAs(firstPrime)`; `await secondPrime`; `harness.Errors.Should().ContainSingle()` with `Exception.BeSameAs(failure)` (the sink was invoked before it threw, so report preceded clear); `harness.Coordinator.GetPressed(SpamEngine).Should().BeTrue()`; `Invalidations.Should().Equal(new[] { SpamToggleControlId })`.
- Fail-before `[D]`: pre-fix, the sink throws at line 380, line 381 is skipped, the lambda `finally` completes `firstPrime` (so `await firstPrime` returns; no hang), the second `GetPressed` hits `ContainsKey` at 274 and starts nothing, so `Verify(Times.Exactly(2))` fails with a Moq `MockException` reporting one invocation. `secondPrime` pre-fix is the stale completed `firstPrime`, so `NotBeSameAs` also fails and awaiting it cannot block. Every await in the test is on a task that is already complete or is completed by the `finally` at 318-321, which runs on both pre- and post-fix code; no await can block forever.
- Post-fix: `Task.FromResult(true)` completes the second prime synchronously; the continuation returns early at 368-371 and completes the marker.
- Pre-fix side effect: the discarded continuation faults unobserved; nothing in the test process subscribes to `UnobservedTaskException`, and .NET Framework 4.5+ does not terminate on unobserved task exceptions, so this does not affect the run.

**Test B — canceled variant (AC1 "faulted or canceled").** Same as A with `probe.SetCanceled()` and `Errors[0].Exception.Should().BeAssignableTo<OperationCanceledException>()`. Can be folded into A as a `[DataTestMethod]` with a `bool canceled` `DataRow` selecting `SetCanceled` vs `SetException`; two plain tests are equally acceptable and match the `PrimeRegistration.cs` style.

**Test C — no faulted task (AC3).**

- The marker: assert `firstPrime.Status.Should().Be(TaskStatus.RanToCompletion)` and the same for `secondPrime` after awaiting. Deterministic; passes pre- and post-fix (the #944 `finally` guarantees it), so it is a guard, not the fail-before carrier.
- The continuation: it is not reachable from a test without a seam, because `StartObservedPrime` discards it (line 310). Recommended: a structural proof, stated in the test's XML remarks, that after the fix every statement in the continuation lambda is non-throwing (the enumeration in §2(b)(1)), combined with Test A's observation that the sink did throw (`Errors` has one entry and `OnLogError` threw) and the marker was still cleared, which is only possible if `CompletePrime` reached line 381 after the sink threw, i.e. the exception was contained inside `CompletePrime`. Given containment, the lambda has no remaining throw source, so the continuation cannot be `Faulted`.
- Smallest seam if runtime evidence is demanded: none exists without new state. Returning the continuation from `StartObservedPrime` has no storage slot (the dictionary holds the marker), and a second dictionary plus an `internal` getter adds a field, a method, and a lifetime question for test-only use. Making the marker a `Task<bool>` carrying "reported cleanly" changes its documented semantics and requires a test-side cast. Both are rejected; recommend the structural proof.

### 4.3 Fail-before evidence

Follow the #944 artifact shape (`...-944/evidence/regression-testing/prime-registration-fail-before.md`): record the vstest command (filter `FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests`, `/Blame:CollectHangDump;TestTimeout=4min` as the hang bound), exit code 1, the failing test name, and the Moq message fragment (expected invocation on the mock exactly 2 times, but was 1 time), plus the production file hash equal to the branch baseline. Write it under `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/evidence/regression-testing/` as a derived summary, never the raw `.trx` (CLAUDE.md "Committed Test Evidence Format").

### 4.4 New partial versus existing partial

- `PrimeFaultOrdering.cs` (77 lines) has room, but AC2 requires its existing tests to pass "without modification"; keeping that file byte-identical makes the AC2 check trivial (`git diff --exit-code <base> -- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs`).
- Recommended: a fifth partial `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs` (estimated 110-150 lines) plus one `<Compile Include="Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs" />` line in `TaskMaster.Test.csproj` adjacent to line 361. This matches the #942 and #944 precedent of one partial per issue and reuses `Harness`, `LoggedError`, and the fixture constants with no harness edit.

## 5. Testing implications and coverage

Changed production lines and branches that the tests must hit:

- The `try` body (`_logError` call): hit by every existing fault/cancel test and by Tests A/B.
- The `catch (Exception)` block: hit only when the sink throws; Tests A/B are the sole coverage of this branch, so the branch-taken arm depends on them.
- `_primeTasks.TryRemove` after the block: hit by all fault paths; Tests A/B cover the reached-after-catch flow.
- Branch pairs: `catch` taken (A/B) and not taken (existing `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse`, `Race.cs` canceled tests, `PrimeRegistration.cs` re-prime tests).

Policy checks: MSTest, Moq (strict mock with `SetupSequence`), FluentAssertions; no `Thread.Sleep`, `Task.Delay`, temp files, timers, `[DoNotParallelize]`, or GC forcing. The sink exception type in the tests should differ from `failure` (for example `InvalidOperationException("sink failed")` versus the prime's `failure`) only if an assertion needs to distinguish them; `Errors[0].Exception` is the prime failure in either case because `Errors.Add` precedes the hook.

## 6. Rejected alternatives (summary)

- (a) `try`/`finally` only: leaves the continuation `Faulted` (fails AC3).
- (c) constructor-level sink wrapper: covers line 184 too but widens scope beyond the minimal fix; see follow-up.
- (d) clear-before-report: prohibited by AC2.
- Catch in the continuation lambda: needs (a) as well; worse locality.
- Any runtime seam for the discarded continuation: adds state or changes marker semantics for test-only use; structural proof preferred.

## 7. Follow-up candidates (out of scope; promote to issues rather than leave in prose)

1. `HandleToggleClickAsync` line 184 invokes `_logError` inside the click-boundary `catch` without a guard; a throwing sink propagates out of a method documented at 167-168 as never throwing, into the `async void` Office handler. Same hazard class as #947, different call site. Approach (c) or a `SafeLog`-style helper shared by both sites would address it.
2. Log volume after a permanent configuration fault (#944 spec follow-up item 2) remains open.

## 8. Numeric Derivation Evidence

No numeric count, enumeration, or population is proposed for a `spec.md` acceptance criterion by this research. The line counts in §1 describe the current tree and are not proposed acceptance values. The `Times.Exactly(2)` in Test A is the issue's own AC1 wording ("invoked a second time"), not a newly derived count.
