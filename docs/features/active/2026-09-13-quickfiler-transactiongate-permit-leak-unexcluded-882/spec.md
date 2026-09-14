# 2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded (Spec)

- **Issue:** #882
- **Parent (optional):** originates from issue #743
- **Owner:** drmoisan
- **Last Updated:** 2026-09-13T19-40
- **Status:** Approved for planning
- **Version:** 1.0
- **Work Mode:** full-bug (this file is the sole acceptance-criteria source per `acceptance-criteria-tracking`)
- **Research:** `docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/research/2026-09-13T19-00-transactiongate-bounded-acquisition-research.md`

## Context

`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` declares a process-wide one-permit gate at line 32:

```
private static readonly SemaphoreSlim TransactionGate = new SemaphoreSlim(1, 1);
```

`BeginTransactionAsync` (lines 122-126) acquires it at line 124 with `await TransactionGate.WaitAsync().ConfigureAwait(false);` — the parameterless overload, which takes no timeout and no `CancellationToken` and therefore cannot fail to acquire and cannot be cancelled. The permit is released only by `ReleaseTransactionGate` (line 88), whose sole caller is `UiThreadDispatcherTransaction.Dispose` (line 275).

Issue #743 measured this area and did not settle it. Both instrumented runs recorded `timeout=0`, so no test was abandoned, so the discriminating observation never occurred. The identical unbounded acquisition remains on the #743 branch at line 149 of the same file, so #743 delivers no change here.

- Observed environment: Windows 11 Pro 10.0.26200; .NET Framework 4.8 / `net481`; MSTest 4.4.0 (`QuickFiler.Test/packages.config` lines 123-124).
- Impact: confined to the `QuickFiler.Test` assembly. A lost or late-released permit presents as a hung or timed-out run whose cause is not local to the failing test.
- Severity: Medium.

## Repro & Evidence

The defect is a reachability property of the current code, not a stochastic event, and the following facts were each read directly in this worktree at base `origin/main` `e6d86049e`:

1. The acquisition at line 124 is unbounded and token-blind.
2. Fourteen acquisition statements exist across five files (research §4.1). Every one routes its release through `UiThreadDispatcherTransaction.Dispose`; no site calls `ReleaseTransactionGate` directly.
3. **Two consuming classes carry no `[Timeout]` at all.** `git grep -c Timeout` returns no match for `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs` and none for `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs`, while returning 2 for `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`. Four test methods in those two classes therefore acquire the process-wide gate with **no bound of any kind** — neither MSTest's nor the gate's. For those four, a permit held by a finished-but-still-running background task produces an unbounded hang, terminated only by the runner-level `/Blame:...;TestTimeout=4min` guard.

This third fact is the load-bearing one and it does not depend on any hypothesis being confirmed.

### Corrected statement of the runner's behaviour

`issue.md` states that MSTest "abandons a timed-out `async` test rather than unwinding it, so the `finally` that would release the permit is no longer observed." The documented behaviour for MSTest 4.4.0 is narrower, and this spec supersedes that wording. With `CooperativeCancellation` at its default of `false` — and it is at the default here: no `CooperativeCancellation` occurrence and no `testconfig.json` exists anywhere in the tree, and `TaskMaster.runsettings` sets no timeout or cancellation key — the documented behaviour is that "the cancellation token is canceled on timeout, timeout result is reported and the method task will continue running on background."

The task is therefore **not** torn down and its `finally` blocks **do** eventually run. Three distinct mechanisms follow, and they must be kept apart:

- **H-LEAK-strong — the permit is never released.** Requires the abandoned background task to never reach `Dispose`. The documented semantics do not produce this on their own. **Not established, and this change does not claim to establish it.**
- **H-LEAK-weak — the permit is released late.** Between expiry and the background task's eventual `Dispose`, the permit is held by a test the runner has already reported as finished. Any later acquirer waits on it with no bound. **Established directly from the documented semantics.**
- **H-TOKEN-BLIND — the acquisition cannot observe cancellation.** MSTest cancels the `CancellationToken` on expiry in both modes. The parameterless `WaitAsync()` at line 124 takes no token and so is structurally incapable of observing it. **Established from the code and the documentation together, and it is unconditional: it does not depend on any timeout having occurred.**

**Delivery is justified by H-LEAK-weak and H-TOKEN-BLIND alone and is not conditional on H-LEAK-strong reproducing.** That conditionality is what left #743's residual open, and this spec forecloses it.

## Scope & Non-Goals

**In scope.**
- Convert the acquisition in `BeginTransactionAsync` from unbounded to bounded, so that failure to acquire surfaces as a prompt, named, diagnosable failure instead of an unbounded wait.
- Add an internal acquisition entry point that accepts the bound, so the failure branch is reachable deterministically from a test.
- Add regression coverage for the failure branch and for the gate's integrity after a failed acquisition.

**Out of scope / non-goals.**
- Demonstrating or refuting H-LEAK-strong. A deterministic demonstration is not available (see Test Strategy, construction C4) and delivery does not depend on one.
- Adding a `CancellationToken`-observing overload that flows `TestContext.CancellationTokenSource.Token`. This addresses H-TOKEN-BLIND more directly but changes the signature at all fourteen acquisition statements across five files, and the two classes that most need a bound carry no `[Timeout]` so their token is never cancelled and they would gain nothing. Record as a follow-up issue.
- Any change to shipped add-in production code. The subject is test-assembly infrastructure.
- Stabilising `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` (R4). Its intermittency is tracked by issue #823, whose flake-watch log forbids introducing a sleep, retry attribute or timing tolerance to stabilise it. This change is not a fix for R4 and must not be presented as one.
- Changing `FieldLock`, `EnsureDispatcher`, or the documented lock ordering.
- `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs`. It is a different type in a different assembly with no semaphore; its only link is a documentation cross-reference.

## Root Cause Analysis

The gate's shape — one permit, unbounded acquisition, held from acquisition to disposal — is unchanged since before issue #493, which moved the ownership of the serialization without changing its shape. Every precondition H-LEAK-weak needs is present, and H-TOKEN-BLIND is present unconditionally.

Affected components:
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` — the gate, its acquisition, and its release.
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` — the six existing regression tests for this fixture.

## Proposed Fix

### Design summary

Make the acquisition bounded inside `BeginTransactionAsync`, and expose the bound to tests through an internal overload. No call site changes.

### The control-flow invariant

> **The object that owns the release must be constructed only on the branch where the acquisition returned `true`. It must never be constructed first and then discarded.**

The current code already satisfies the single-releaser half by construction: `UiThreadDispatcherTransaction` is the only type that calls `ReleaseTransactionGate`, and it is constructed in exactly one place (line 125). Making the acquisition bounded is therefore confined to `BeginTransactionAsync`, provided the failure branch leaves the method **before** `new UiThreadDispatcherTransaction()` exists.

Shape:
- acquire with a bounded overload into a `bool`;
- on `false`, throw, before any transaction object exists;
- on `true`, fall through to the existing `return new UiThreadDispatcherTransaction();`.

Because no releasing object exists on the failure path there is no release to omit, no `finally` to get wrong, and no way for a caller to dispose something it never received. Every existing `using` and `try`/`finally` at the fourteen acquisition statements stays correct unchanged, because a throw from `BeginTransactionAsync` happens before the `using` scope is entered or the assignment completes.

### Shapes that are wrong, named so review can reject them

- `try { acquired = await Wait(...); } finally { Release(); }` — releases on the failure path. A second `Release` on a `SemaphoreSlim(1, 1)` throws `SemaphoreFullException`, which permanently raises the count above the maximum and destroys mutual exclusion for every later test in the process. That is strictly worse than the defect being fixed.
- Constructing the transaction first and disposing it on failure — the same defect routed through `Dispose`.
- Returning `null` on failure — every call site immediately dereferences the result, and the three `using (var transaction = await ...)` sites in `QfcFormControllerUndoHandoffTests.cs` would silently no-op instead of failing.

### The bound

**120000 ms (two minutes).** The value is fixed by this spec because research recorded the two anchors as being in tension and required explicit reconciliation.

- It must **exceed the longest legitimate hold.** The longest is `PumpHarness`, which holds the permit for a whole pump-hosted test body; that body is itself bounded by `[Timeout(PumpTimeoutMs)]` with `PumpTimeoutMs = 60000` (`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` line 38). 120000 gives a factor of two of headroom over the longest hold the suite can legitimately produce, so the bound cannot manufacture a false failure.
- It must sit **below the runner-level hang guard**, which the #823 flake-watch log records as `TestTimeout=4min`. 120000 is half of it, so a gate failure is reported as a test failure naming its cause rather than as a hang dump.
- It is deliberately **above** the local `[Timeout]` convention of 60000 ms. For the classes that carry `[Timeout]`, MSTest continues to report first and their behaviour is unchanged — this change introduces no new failure mode for them. For the two classes that carry no `[Timeout]`, the gate bound is the only bound in existence, and it converts an unbounded hang into a named failure. The change is therefore a strict improvement at every call site and a regression risk at none.

### Failure type

Throw `System.TimeoutException`. No new exception type is introduced. The message must:
- name `TransactionGate` and `UiThreadDispatcherFixture`;
- state the bound that elapsed;
- state that the probable cause is a permit held by a test the runner has already reported as finished, so that the reader looks outside the failing test;
- contain a fixed, greppable token so the failure can be found in a log.

### Files to change

- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` (278 lines; ample headroom under the 500-line ceiling).
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` (353 lines; headroom for the new tests).

Adding the tests to the existing test file rather than a new file avoids touching `QuickFiler.Test/QuickFiler.Test.csproj`, which uses explicit `<Compile Include>` items exclusively (173 occurrences, no globbing and no SDK-style default include) and would otherwise require an edit to a shared project file. **Keeping the project file untouched is a requirement of this spec, not an optimisation** — see AC7.

## Determinism Ruling (record verbatim; do not relitigate)

`.claude/rules/general-unit-test.md` prohibits "real wall-clock waits" in test code. The subject compiles into `QuickFiler.Test`, so the question is whether a bounded acquisition inside fixture infrastructure is such a wait.

**It is not**, and the repository has already settled this reading in code and written down why:

- `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs` lines 48-49: "Bounded, event-driven wait for a state transition. This is not a fixed sleep: it returns as soon as the condition holds, and fails the test with a clear message if it never does."
- `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs` line 29: "The outer MSTest `[Timeout]` is a deadlock bound, not a wait." The same file's class doc states "no elapsed-time measurement and no real wall-clock wait is used" about a test that nonetheless carries `[Timeout(5000)]`.

The criterion those establish, which this spec adopts:

> A real-time bound is permitted in test code when (i) it returns immediately once the awaited condition holds, so it contributes nothing to the duration of a passing run, and (ii) it is a failure bound whose expiry is reported as a failure, never the mechanism by which the expected state is reached. A construct that consumes time in order to let something else happen is banned regardless of where it is written.

A bounded `WaitAsync(TimeSpan)` in `BeginTransactionAsync` satisfies both clauses: on the success path it returns the instant the permit is available, exactly as the unbounded form does. The contrary reading would additionally condemn the sixteen pre-existing `[Timeout(...)]` attributes in this same assembly, which is not the repository's settled position.

**Corollary that constrains the tests:** a test observing the `false` branch must not reach it by letting the bound elapse, because that would breach clause (i).

## Test Strategy

### The deterministic construction

Hold a real transaction and probe with a zero-length bound. `SemaphoreSlim.WaitAsync` is documented: "If the timeout is set to zero milliseconds, the method doesn't block. It tests the state of the wait handle and returns immediately." So:

1. Acquire a transaction through the public entry point and do not dispose it yet.
2. Perform a second acquisition through the internal overload with `TimeSpan.Zero`. It returns the failure outcome immediately, with zero elapsed time and zero scheduling dependence.
3. Dispose the held transaction in a `finally`.
4. Acquire again and observe success, proving the gate survived the failed acquisition.

This exercises the real failure branch of the real acquisition and satisfies both clauses of the determinism criterion. The release in step 3 must be in a `finally` so that a failing assertion cannot itself leak the process-wide permit and poison the remainder of the run.

### Rejected constructions

- Reflecting onto the private `TransactionGate` field and calling `Wait()` directly: deterministic, but it couples a test to a private field name in its own assembly and a failure between the raw `Wait()` and the raw `Release()` corrupts the gate for the whole process with no `Dispose` to recover it. The same effect is reachable without reflection.
- Asserting on `SemaphoreSlim.CurrentCount` only: deterministic but never drives the failure branch, so it cannot be the regression test.
- Reproducing genuine MSTest abandonment with a very small `[Timeout]`: **not deterministic** and rejected. It depends on cross-class ordering that MSTest does not guarantee, requires a deliberately failing test in the suite, and reaches the expected state by elapsed time, breaching clause (ii).
- Making `TransactionGate` injectable: defeats the file's stated single-owner property.

### Fail-before framing

The failure branch does not exist on the current tree, so a test written against it will not **compile** before the fix rather than fail at runtime. The plan must record this explicitly as a compile-level fail-before with a `fail-before-exception` dossier under `evidence/regression-testing/`, per `evidence-and-timestamp-conventions`, rather than discovering it at execution time.

### Guards on the new tests

- Carry `[Timeout(GateTimeoutMs)]`, matching the local convention (60000 ms, declared at line 33 of the test file).
- Apply `[DoNotParallelize]` to the class holding the process-wide permit, following the established local precedent in `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` and `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs`. The assembly otherwise inherits `TaskMaster.runsettings` (`Workers` 0, `Scope` ClassLevel), under which other classes run concurrently against the same gate.
- MSTest, Moq where mocking is needed, FluentAssertions for assertions, per the C# Unit Test Policy.

### Existing tests to watch

- `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` (lines 202-262) is the only existing test with a genuinely contended acquisition. A bound of 120000 ms is far above its hold window, so it is not at risk, but any shorter bound would convert it into a flake. Its behaviour must be unchanged.
- `Transaction_DisposedTwice_DoesNotOverReleaseTheGate` (lines 269-310) is the existing guard against an over-release. Its round-trip acquisition is precisely the assertion that catches a failure path that wrongly released. It must keep passing and must not be weakened.

### Coverage

The change is confined to test-assembly infrastructure. `QuickFiler.Test` is a test project and is excluded from the coverage denominator, so no production-coverage movement is expected. This is stated rather than measured, and the plan must record the statement rather than assert a coverage delta it cannot produce.

## Acceptance Criteria

- [ ] AC1 — The acquisition in `BeginTransactionAsync` is bounded: the parameterless `SemaphoreSlim.WaitAsync()` call no longer appears in `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`, and the acquisition uses an overload returning a boolean outcome that the method branches on.
- [ ] AC2 — On a failed acquisition the method throws `System.TimeoutException` whose message names `TransactionGate`, names the elapsed bound, and carries the fixed greppable token declared by the plan; and no `UiThreadDispatcherTransaction` instance is constructed on that path.
- [ ] AC3 — The production default bound is 120000 ms, and an internal acquisition entry point accepting the bound exists so that a test can supply `TimeSpan.Zero`.
- [ ] AC4 — A new regression test in `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` observes the failure branch deterministically by holding a transaction and probing with `TimeSpan.Zero`, asserting `TimeoutException` is thrown. The test contains no `Thread.Sleep`, no `Task.Delay`, and no elapsed-time assertion.
- [ ] AC5 — A companion assertion proves the gate survives a failed acquisition: after the failure branch is taken, releasing the held transaction and acquiring again succeeds, and no `SemaphoreFullException` is thrown on any path.
- [ ] AC6 — The six pre-existing tests in `QfcItemController.UiThreadDispatcherFixtureTests.cs` are unmodified in intent and all pass, and no test in any of the five consuming files listed in the research blast radius required an edit.
- [ ] AC7 — `QuickFiler.Test/QuickFiler.Test.csproj` is not modified, and no file is added to or removed from the project. The anchored diff against the plan's recorded base lists exactly the write-set paths the plan declares.
- [ ] AC8 — The spec's determinism criterion is reproduced in the plan or in an evidence artifact, and the change is shown to satisfy both of its clauses.
- [ ] AC9 — A compile-level `fail-before-exception` dossier exists under `evidence/regression-testing/` recording why a runtime fail-before run is structurally impossible for AC4.
- [ ] AC10 — The full C# toolchain passes in a single pass in the required order: `dotnet tool run csharpier check .` clean, analyzer rebuild with zero warnings and zero errors, nullable rebuild with zero warnings and zero errors, and the `QuickFiler.Test` suite passing with no fewer tests than the recorded baseline plus the tests this change adds.
- [ ] AC11 — No shipped add-in production file is modified. The write set contains no path outside `QuickFiler.Test/` and the feature folder.

## Risks & Mitigations

| Risk | Mitigation |
|---|---|
| A failure path that releases the permit raises the count above the maximum and destroys mutual exclusion for the whole process, silently. | The control-flow invariant above, plus AC5's explicit `SemaphoreFullException` assertion, plus the pre-existing `Transaction_DisposedTwice_DoesNotOverReleaseTheGate` guard. |
| A bound shorter than the longest legitimate hold manufactures false failures. | The bound is fixed at twice the 60000 ms `[Timeout]` that bounds the longest hold, and the rationale is recorded so a later reduction has to argue against it. |
| The new test holds a process-wide permit while other classes run concurrently under `Scope ClassLevel`. | `[DoNotParallelize]` on the holding class, the hold window kept to the probe only, and release in a `finally`. |
| The change is mistaken for a fix for the issue #823 R4 flake. | Declared a non-goal above; the plan must not cite R4 stability as evidence. |
| The bounded wait is read as a banned wall-clock wait. | The determinism ruling above, with the in-tree precedent and the two-clause criterion, recorded so review does not relitigate it. |

## Rollout & Follow-up

- Follow-up issue to consider: add a `CancellationToken`-observing acquisition overload flowing `TestContext.CancellationTokenSource.Token`, addressing H-TOKEN-BLIND directly. Deferred here for blast-radius reasons.
- Follow-up issue to consider: `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs` contains a second unbounded `SemaphoreSlim.WaitAsync()`. It is a per-instance test-local semaphore rather than a process-wide static, so its exposure is confined to one test, but it is the same shape.
- Links: issue #882; originating issue #743; related flake reports #592, #511, #571, #823; frequently miscited as closing this area, #493.
