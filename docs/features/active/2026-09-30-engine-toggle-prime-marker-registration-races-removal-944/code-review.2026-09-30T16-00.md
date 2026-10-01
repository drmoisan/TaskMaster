# Code Review — engine-toggle-prime-marker-registration-races-removal (Issue #944)

- Timestamp: 2026-09-30T16-00
- Branch: `bug/engine-toggle-prime-marker-registration-races-removal-944` at `1f3614deb5182c6b52e2bf1c625aa5b7925019ee`
- Base: `main` at `66afa6372fd82fc1ffd7c81f85a1ad65eebc5817` (merged into the branch by `7190a4bcddab8c519933d98b12ede739d4afede3`)
- Files reviewed in full: `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (442 lines), `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs` (175 lines), the `TaskMaster.Test/TaskMaster.Test.csproj` hunk, and the reused members of `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` (`Harness`, `LoggedError`, `SpamEngine`, `SpamToggleControlId`).
- Companion artifacts: `policy-audit.2026-09-30T16-00.md`, `feature-audit.2026-09-30T16-00.md`.

## Executive Summary

Verdict: **PASS** — 0 Blocking, 4 Non-blocking, 3 Follow-up.

The ordering fix is correct and minimal. Registering the marker under `_primeGate` before `ApplyPrimeAsync` is invoked closes the window in which a synchronously faulted or canceled prime's continuation could run `CompletePrime`'s `TryRemove` before the store, and it does so without a new lock, a scheduler seam or a `catch`. The keyed removal in `CompletePrime` cannot remove a newer marker, because a newer marker for the same key can only be registered after the older marker's removal has already executed (analysis in CR-A below). The three new tests are policy-compliant (MSTest, the existing strict Moq harness, FluentAssertions, no timing constructs, no temporary files) and the fail-before evidence is a genuine program-order failure rather than a compile or load failure. Both C# files are under the 500-line ceiling. The evidence tree is free of host paths and account names.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Follow-up | `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | `StartObservedPrime` continuation, lines 312-322; `CompletePrime` lines 380-381 | If the injected `_logError` sink throws, `CompletePrime` exits before `_primeTasks.TryRemove`; the `finally` still completes the marker, so awaiters resume, but the marker stays registered and the engine cannot re-prime for the session. The discarded continuation task (`_ =`, line 310) then holds an unobserved exception. | Promote spec Rollout item 1 to an issue. Candidate shapes: wrap the sink call so a sink fault is observed and the removal still runs, or move `TryRemove` into the `finally` alongside `SetResult` (would need #942's report-then-clear ordering re-argued). | Out of this item's scope by spec ("Any change to the `CompletePrime` method" is a non-goal); the production sink is `logger.Error`, so likelihood is low, but the failure mode reproduces this issue's symptom by a different cause. | spec.md Rollout & Follow-up item 1; `StartObservedPrime` remarks lines 298-301 |
| Follow-up | `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | `StartPrimeIfNeeded` lines 272-288 | After a permanently faulted configuration load (`AsyncLazy` caches the fault), every cache-miss `getPressed` poll that reaches `StartPrimeIfNeeded` now re-primes and logs again; the pre-fix stale marker intermittently suppressed the repeats. | Promote spec Rollout item 2 to an issue (back-off, or a `ResetConfigAsyncLazy`-based recovery). | Behavioural consequence of the fix that the spec records but does not file; feature-folder prose is archived at merge. | spec.md Data / API / Config Impact; Rollout item 2 |
| Follow-up | `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | `GetPrimeTask` `<returns>`, lines 243-249 | The element opens "The prime task, or `Task.CompletedTask` ..."; the returned value is now the registration marker (`TaskCompletionSource<bool>.Task`), which completes after `CompletePrime` has observed the prime's outcome, not the prime task itself. The following sentences remain accurate. | On the next touch of this file, replace "The prime task" with "The registration marker for the engine's prime" (one sentence). | Plan decision D-3 froze `GetPrimeTask` and ruled the sentence not false; it is imprecise rather than wrong, so no change is requested for this item. | production lines 243-249; plan D-3 |
| Non-blocking | `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | line 286 `_primeTasks[engineName] = marker.Task;` | The indexer store is correct under the lock and the preceding `ContainsKey` probe. `TryAdd` with a `Debug.Assert` on its result would make the single-writer invariant (AC9) self-checking at the write site. | Optional; consider at the next touch. Not requested for this item. | Style/defence-in-depth only; `_primeTasks[` count is 1 and `TryAdd`/`AddOrUpdate`/`GetOrAdd` counts are 0 (production-edit-scope tokens), so no competing writer exists today. | production lines 272-288 |
| Non-blocking | evidence: `evidence/baseline/stall-probe.md` | P0-T16 | The stall probe was classified `REPRODUCES` because one shell-icon test failed (`Win32 handle that was passed to Icon is not valid`), not because a hang occurred (`SEQUENCE_FILES: 0`). Under the plan's rule that selected the DIRECT coverage route, which excluded the four `UtilitiesCS.Test` shell-icon classes from the local coverage run. | None for this item; the excluded classes run in CI and touch no assembly this item changes. The plan rule is conservative by design. | Recorded so the route selection is understood as a failed probe rather than a reproduced hang. | `stall-probe.md` lines 7, 18-22 |
| Non-blocking | evidence: `evidence/qa-gates/coverage-summary.md` | Details (pass 1) and COORDINATOR-RULING | Pass 1 of the final coverage run failed two `QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests` tests on five-second wall-clock waits in a project this item does not modify; a single full-loop restart was coordinator-approved (R3-1) and pass 2 was clean. | None for this item. The wall-clock waits (`SpinWait.SpinUntil(..., 5 s)`, `Task.Wait(5 s)`) are the flakiness the coordinator is filing separately. | Context only; the restart began at formatting, which is the loop rule. | `coverage-summary.md` lines 60-74; plan R3-1 |
| Non-blocking | evidence: `evidence/baseline/coverage-baseline.md` vs `evidence/qa-gates/coverage-summary.md` | COMPARISON | The baseline tree is ANCHOR-SHA `b305903e` and the final tree is HEAD after merging main `66afa6372` (item 929 content), so the repository-wide delta (+14 valid, +20 covered lines) is not solely this item's. | None; the plan's comparability rule (D-9) handled it and the per-file/per-method figures carry the no-regression conclusion. | Recorded so nobody reads the +20 as this item's contribution; the coordinator class alone accounts for +14 covered lines. | `coverage-summary.md` COMPARISON section |

## Correctness Analysis of the Ordering Fix

### CR-A. Keyed removal cannot remove a newer marker

`CompletePrime` identifies the entry to remove by key only: `_primeTasks.TryRemove(engineName, out _);` (line 381). This is safe under the type's invariants for the following reason, verified by reading every write and removal site in the file:

1. `_primeTasks` has exactly one write site, line 286, executed under `lock (_primeGate)` immediately after `if (_primeTasks.ContainsKey(engineName)) return;` (line 274). A marker for key K can therefore be registered only while no entry for K exists.
2. `_primeTasks` has exactly one removal site, line 381, inside `CompletePrime`, which runs exactly once per prime as the `ContinueWith` continuation of that prime's `ApplyPrimeAsync` task.
3. Consider marker A for key K and a later marker B for K. B's registration (step 1) required `ContainsKey(K)` to be false, which — since A's registration preceded A's prime start and the only removal path is step 2 — means A's `TryRemove` had already executed. A's continuation never touches the dictionary after that statement (only `marker.SetResult(true)` in the `finally` remains). So A's removal precedes B's registration in the causal order established by the `ConcurrentDictionary` operations themselves, and A cannot remove B.
4. On the success path `CompletePrime` returns at line 370 without removing, so the marker is retained and `ContainsKey` blocks any re-prime for the session — the pre-existing intended behaviour (spec state model).

Identity-conditional removal (`ICollection<KeyValuePair<...>>.Remove`) is therefore not needed, as research section 4.4 concluded; the spec correctly excludes it.

### CR-B. The race the fix closes

At the anchor, `_primeTasks[engineName] = StartObservedPrime(...)` evaluated the right-hand side first: `ApplyPrimeAsync` ran to its first await, an already-faulted `EngineActiveAsync` task made the async method's task fault synchronously, and `ContinueWith(..., TaskContinuationOptions.None, TaskScheduler.Default)` queued the continuation to the thread pool before the store executed. A pool thread could run `TryRemove(K)` (no entry yet, no-op) before the registering thread stored the finished continuation task, leaving a permanent stale entry. With the fix, the store at line 286 precedes the call at line 287, so any continuation, on any thread, finds the marker it is meant to remove. Test 1 pins this by observing `GetPrimeTask(K).IsCompleted == false` from inside the activation read.

### CR-C. Marker completion semantics

- `marker.SetResult(true)` (line 320) is the only completion call (`SetException`/`SetCanceled`/`TrySet*` counts are 0), so the handle returned by `GetPrimeTask` can never fault or cancel; the `<returns>` contract "never faults" holds.
- The `finally` guarantees completion even if `CompletePrime` throws (see the Follow-up on a throwing sink), so no awaiter can hang on the marker.
- `TaskCreationOptions.RunContinuationsAsynchronously` prevents an awaiter's continuation from running inline inside the coordinator's `finally` on the pool thread; correctness does not depend on it, but it keeps the coordinator's continuation short and avoids re-entrancy from test code. The option is available on .NET Framework 4.8 and already used in two production files.
- Report-then-clear (#942) is preserved: the marker is completed only after `CompletePrime` returns, and `CompletePrime` still logs (line 380) before removing (line 381). The #942 test `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged` passes unchanged in all five recorded runs.
- No deadlock path: `GetPrimeTask` takes no lock, so test 1's call from inside `EngineActiveAsync` while `_primeGate` is held (the prime starts synchronously inside the lock) cannot block. `CompletePrime` takes no lock.

### CR-D. Assumption recorded by the spec

`ApplyPrimeAsync` is `async`, so it has no synchronous throw path after the marker is registered, and `ContinueWith` with valid arguments does not throw; a future change that adds a synchronous throw before the continuation is attached must also remove the marker. The spec records this under Risks; the review agrees and adds nothing.

## Test Review

| Test | Policy check | Result |
|---|---|---|
| `GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns` (lines 31-75) | Records only inside the mock callback (lines 44-46), asserts only outside it (53-74); `handleCompletedDuringRead` initialised to `true` so a never-invoked callback fails the first assertion (line 39, spec Risk 2); awaits the recorded marker rather than polling; asserts identity change and completion afterwards. Fail-before: `Failed` with the expected message; pass-after: `Passed`. | PASS |
| `GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime` (lines 84-123) | `SetupSequence` faulted-then-true on the strict mock; awaits the first handle (marker or `CompletedTask`, both deterministic), second read, awaits second handle; `Verify(Times.Exactly(2))`, pressed true, one invalidation, one error `BeSameAs(failure)`. Docstring correctly states it does not carry the fail-before obligation. | PASS |
| `GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime` (lines 131-171) | Same shape with `Task.FromCanceled<bool>(new CancellationToken(true))`; error `BeAssignableTo<OperationCanceledException>`; `using System.Threading;` present. | PASS |
| Shared | No `[TestClass]` in the partial (the main fixture carries it, line 22); no new `Harness` member or mock; each test constructs its own `Harness`; 13 FluentAssertions chains with `because` text; `// Arrange` / `// Act` / `// Assert` markers; XML summaries; no banned token (23-token scan all 0, confirmed by reading). File 175 lines. | PASS |

Strict-mock exhaustion note (spec Test Strategy): after the second `Returns` the sequence would yield `null`; no third `EngineActiveAsync` call occurs because the third `GetPressed` is a cache hit. Confirmed by `Verify(Times.Exactly(2))` passing.

## Evidence Hygiene

- Host paths / account / machine names: 0 hits on this review's Grep of the feature folder for drive-letter paths, user-profile folder segments, the developer account name, mail address and `DESKTOP-`/`LAPTOP-` prefixes; executor sweeps P3-T13 and P3-T35 also 0/0/0 over 45 files. Trx failure locations are recorded as `REDACTED-PATH\QuickFiler.Test\...`; assembly paths are root-stripped.
- Raw documents: none committed (P3-T12, `RAW-DOCS-COMMITTED: 0`); both Cobertura documents and trx files remain git-ignored under `coverage/`.
- Timestamp integrity: every artifact's `Timestamp:` label agrees to the minute with the UTC window recorded in the same artifact; the pass-2 Cobertura root epoch (1790781012 = 2026-09-30T15:10:12Z) matches the 15-10 label.
- Evidence locations: all canonical (`<FEATURE>/evidence/<kind>/`); no `artifacts/` path in the diff.

## Non-blocking and Follow-up Register

- Non-blocking: NB-A indexer store vs `TryAdd` (style); NB-B stall-probe classification by a failed test rather than a hang; NB-C R3-1 restart on untouched-project wall-clock flakiness (coordinator filing); NB-D baseline/final trees differ by the main merge.
- Follow-up: FU-1 throwing `logError` sink leaves a stale marker and an unobserved continuation fault; FU-2 log volume after a permanent configuration fault; FU-3 `GetPrimeTask` `<returns>` wording.

Blocking count contributed by this artifact: 0.
