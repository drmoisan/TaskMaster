# 2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded (Spec)

- **Issue:** #882
- **Parent (optional):** originates from issue #743
- **Owner:** drmoisan
- **Last Updated:** 2026-09-28T00-30
- **Status:** Approved for planning
- **Version:** 1.1
- **Work Mode:** full-bug (this file is the sole acceptance-criteria source per `acceptance-criteria-tracking`)
- **Research:** docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/research/2026-09-13T19-00-transactiongate-bounded-acquisition-research.md (original, reasoning in its sections 1, 2 and 3a still governs) and docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/research/2026-09-28T00-10-transactiongate-research-refresh-research.md (refresh; authoritative for every line citation, the acquisition inventory, the parallel-regime design and the `SemaphoreFullException` correction)

## Context

QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs (the fixture file, 304 lines) declares a process-wide one-permit gate at line 32:

```
private static readonly SemaphoreSlim TransactionGate = new SemaphoreSlim(1, 1);
```

`BeginTransactionAsync` (lines 142-152) acquires it at line 149 with `await TransactionGate.WaitAsync().ConfigureAwait(false);` — the parameterless overload, which takes no timeout and no `CancellationToken` and therefore cannot fail to acquire and cannot be cancelled. Since the merge of issue #743 the method also carries a contended pre-check before the wait (lines 144-147, incrementing `_contendedAcquisitions` when `CurrentCount` reads zero) and an acquisitions increment after the wait (line 150, `_transactionAcquisitions`); the transaction is constructed at line 151, still the only `new UiThreadDispatcherTransaction()` in the file. The permit is released only by `ReleaseTransactionGate` (lines 107-111, which increments `_transactionReleases` at line 109 before calling `Release()` at line 110), whose sole caller is `UiThreadDispatcherTransaction.Dispose` (line 301).

Issue #743 measured this area and did not settle it. Both instrumented runs recorded `timeout=0`, so no test was abandoned, so the discriminating observation never occurred. Issue #743 has since merged into this branch; it added the three counters and one counter-balance test, and left the acquisition itself unbounded and token-blind. The refreshed research (section 8) confirmed that no merged commit bounds this acquisition, so the change this spec describes is still outstanding and duplicates nothing.

- Observed environment: Windows 11 Pro 10.0.26200; .NET Framework 4.8 / `net481`; MSTest 4.4.1 (QuickFiler.Test/packages.config lines 43-45; a patch bump from the 4.4.0 cited in version 1.0, within the same documentation moniker, so the runner-behaviour statements below are unaffected).
- Impact: confined to the `QuickFiler.Test` assembly. A lost or late-released permit presents as a hung or timed-out run whose cause is not local to the failing test.
- Severity: Medium.

## Repro & Evidence

The defect is a reachability property of the current code, not a stochastic event. The following facts were first read in this worktree at base origin/main `e6d86049e` on 2026-09-13 and were re-read on 2026-09-28 against the branch tree with issue #743 merged (the refreshed research records that merge as two hundred thirty-eight commits past the original base):

1. The acquisition at line 149 is unbounded and token-blind. No `TimeSpan` and no `CancellationToken` token appears anywhere in the fixture file.
2. Fifteen acquisition statements exist across five files (refreshed research section 2, which carries the full numeric derivation with two independent search strategies and an explicit member-set comparison; the fifteenth is the issue #743 counter-balance test). Every one routes its release through `UiThreadDispatcherTransaction.Dispose`; no site calls `ReleaseTransactionGate` directly.
3. **Two consuming classes carry no `[Timeout]` at all.** A search for `Timeout` and `DoNotParallelize` over QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs and over all four partial-class files of `QfcHomeControllerRunAsyncTests` (glob QfcHomeControllerRunAsync*.cs) returns no match, while QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs returns two. Four test methods in those two classes therefore acquire the process-wide gate with **no bound of any kind** — neither MSTest's nor the gate's. For those four, a permit held by a finished-but-still-running background task produces an unbounded hang, terminated only by the runner-level `/Blame:...;TestTimeout=4min` guard.

This third fact is the load-bearing one and it does not depend on any hypothesis being confirmed.

### Corrected statement of the runner's behaviour

issue.md states that MSTest "abandons a timed-out `async` test rather than unwinding it, so the `finally` that would release the permit is no longer observed." The documented behaviour for MSTest 4.4.x is narrower, and this spec supersedes that wording. With `CooperativeCancellation` at its default of `false` — and it is at the default here: no `CooperativeCancellation` occurrence exists in any source or configuration file, no testconfig.json file exists anywhere in the tree, and TaskMaster.runsettings sets no timeout or cancellation key — the documented behaviour is that "the cancellation token is canceled on timeout, timeout result is reported and the method task will continue running on background."

The task is therefore **not** torn down and its `finally` blocks **do** eventually run. Three distinct mechanisms follow, and they must be kept apart:

- **H-LEAK-strong — the permit is never released.** Requires the abandoned background task to never reach `Dispose`. The documented semantics do not produce this on their own. **Not established, and this change does not claim to establish it.**
- **H-LEAK-weak — the permit is released late.** Between expiry and the background task's eventual `Dispose`, the permit is held by a test the runner has already reported as finished. Any later acquirer waits on it with no bound. **Established directly from the documented semantics.**
- **H-TOKEN-BLIND — the acquisition cannot observe cancellation.** MSTest cancels the `CancellationToken` on expiry in both modes. The parameterless `WaitAsync()` at line 149 takes no token and so is structurally incapable of observing it. **Established from the code and the documentation together, and it is unconditional: it does not depend on any timeout having occurred.**

**Delivery is justified by H-LEAK-weak and H-TOKEN-BLIND alone and is not conditional on H-LEAK-strong reproducing.** That conditionality is what left #743's residual open, and this spec forecloses it.

## Scope & Non-Goals

**In scope.**
- Convert the acquisition in `BeginTransactionAsync` from unbounded to bounded, so that failure to acquire surfaces as a prompt, named, diagnosable failure instead of an unbounded wait.
- Add an internal acquisition entry point that accepts the bound, so the failure branch is reachable deterministically from a test.
- Add regression coverage for the failure branch and for the gate's integrity after a failed acquisition, running under the existing parallel test regime.

**Out of scope / non-goals.**
- Demonstrating or refuting H-LEAK-strong. A deterministic demonstration is not available (see Test Strategy, rejected constructions) and delivery does not depend on one.
- Adding a `CancellationToken`-observing overload that flows `TestContext.CancellationTokenSource.Token`. This addresses H-TOKEN-BLIND more directly but changes the signature at all fifteen acquisition statements across five files, and the two classes that most need a bound carry no `[Timeout]` so their token is never cancelled and they would gain nothing. Record as a follow-up issue.
- Any change to shipped add-in production code. The subject is test-assembly infrastructure.
- Stabilising `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` (R4). Its intermittency is tracked by issue #823, whose flake-watch log forbids introducing a sleep, retry attribute or timing tolerance to stabilise it. This change is not a fix for R4 and must not be presented as one.
- Changing `FieldLock`, `EnsureDispatcher`, the three issue #743 counters' meaning, or the documented lock ordering.
- UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs. It is a different type in a different assembly with no semaphore; its only link is a documentation cross-reference.
- Serialising the test run. TaskMaster.runsettings (`Workers` 0, `Scope` ClassLevel) stays in force; see the binding constraint under Test Strategy.

## Root Cause Analysis

The gate's shape — one permit, unbounded acquisition, held from acquisition to disposal — is unchanged since before issue #493, which moved the ownership of the serialization without changing its shape, and issue #743 added observability around it without changing it either. Every precondition H-LEAK-weak needs is present, and H-TOKEN-BLIND is present unconditionally.

Affected components:
- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs — the gate, its acquisition, its release, and the three counters.
- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs (the fixture test file, 396 lines) — the seven existing regression tests for this fixture (R1 to R6 plus the issue #743 counter-balance test at lines 355-394).

## Proposed Fix

### Design summary

Make the acquisition bounded inside `BeginTransactionAsync`, and expose the bound to tests through an internal overload. No call site changes. The parameterless overload delegates to the bounded one with the production default. The XML documentation on `BeginTransactionAsync` (lines 137-141) and the class documentation sentence that the gate "is held from transaction start until `Dispose`" (lines 17-19) must be updated to state the bound and the failure type.

### The control-flow invariant

> **The object that owns the release must be constructed only on the branch where the acquisition returned `true`. It must never be constructed first and then discarded.**

The current code already satisfies the single-releaser half by construction: `UiThreadDispatcherTransaction` is the only type that calls `ReleaseTransactionGate`, and it is constructed in exactly one place (line 151). Making the acquisition bounded is therefore confined to `BeginTransactionAsync`, provided the failure branch leaves the method **before** `new UiThreadDispatcherTransaction()` exists.

Shape:
- keep the contended pre-check (lines 144-147) where it is, before the wait;
- acquire with a bounded overload into a `bool`;
- on `false`, throw, before any transaction object exists and without touching `_transactionAcquisitions` or `_transactionReleases`;
- on `true`, increment `_transactionAcquisitions` and fall through to the existing `return new UiThreadDispatcherTransaction();`.

Because no releasing object exists on the failure path there is no release to omit, no `finally` to get wrong, and no way for a caller to dispose something it never received. Every existing `using` and `try`/`finally` at the fifteen acquisition statements stays correct unchanged, because a throw from `BeginTransactionAsync` happens before the `using` scope is entered or the assignment completes.

### Counter placement (issue #743 interaction)

The three counters introduced by issue #743 add a second reason for the invariant, and they fix where the increments may sit:

1. **The contended pre-check stays before the wait.** A zero-bound probe issued while another transaction holds the permit genuinely "observed `CurrentCount == 0` immediately before waiting" (the documented meaning at lines 37-40), so incrementing `_contendedAcquisitions` for it is consistent with the definition. `ContendedAcquisitions` is written to test output and never asserted anywhere in the assembly (refreshed research section 4.3), so the increment can perturb no existing assertion.
2. **`_transactionAcquisitions` is incremented only on the successful branch**, after the boolean result has been tested. Whether the increment precedes or follows the constructor is immaterial; the constructor touches no counter.
3. **The failure branch increments no counter and constructs no `UiThreadDispatcherTransaction`.** A failed bounded wait is plus-zero on acquisitions and plus-zero on releases.

Any other placement breaks the existing test `TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition` (fixture test file lines 355-394). That test asserts `TransactionAcquisitions - TransactionReleases == 1` while holding the sole permit; the assertion is sound because an acquisition is counted only once the permit is held and a release is counted exactly once per transaction. If the acquisitions increment were moved before the wait, or executed unconditionally, the first failed probe in a process would raise acquisitions without a matching release and that test would read two the next time it ran in the same process. The counter-balance test is therefore both a consumer of this invariant and a deterministic detector for the most likely wrong placement.

### Shapes that are wrong, named so review can reject them

- `try { acquired = await Wait(...); } finally { Release(); }` — releases on the failure path. This is worse than the defect being fixed, and the mechanism must be stated precisely because version 1.0 of this spec stated it incorrectly. While the legitimate holder still holds the permit the count is zero, so a wrong `Release()` on the failure branch **succeeds silently**, raises the count to one, and breaks mutual exclusion from that instant. Nothing throws at the release point. The `SemaphoreFullException` surfaces later, at the legitimate holder's `Dispose`, when its own `Release()` finds the count already at the maximum — and under the parallel regime a parked acquirer may take the wrongly-released permit first, in which case the exception surfaces at whichever holder disposes second. A wrong release is therefore detected downstream, not at the site of the error, which is why the Test Strategy places its `SemaphoreFullException` assertion on the probing test's own `Dispose` and relies on the counter-difference assertion and code review as the deterministic guards.
- Constructing the transaction first and disposing it on failure — the same defect routed through `Dispose`, with the additional effect that `_transactionReleases` is incremented for a permit that was never counted as acquired.
- Returning `null` on failure — every call site immediately dereferences the result, and the three `using (var transaction = await ...)` sites in QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs (lines 230, 281, 337) would silently no-op instead of failing.
- Incrementing `_transactionAcquisitions` before the wait or unconditionally — breaks the counter-balance test as described above.

### The bound

**120000 ms (two minutes).** The value is fixed by this spec because research recorded the two anchors as being in tension and required explicit reconciliation.

- It must **exceed the longest legitimate hold.** The longest is `PumpHarness`, which holds the permit for a whole pump-hosted test body; that body is itself bounded by `[Timeout(PumpTimeoutMs)]` with `PumpTimeoutMs = 60000` (QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs line 38, re-verified 2026-09-28). 120000 gives a factor of two of headroom over the longest hold the suite can legitimately produce, so the bound cannot manufacture a false failure.
- It must sit **below the runner-level hang guard**, which the #823 flake-watch log records as `TestTimeout=4min` (four occurrences in that log, re-verified). 120000 is half of it, so a gate failure is reported as a test failure naming its cause rather than as a hang dump.
- It is deliberately **above** the local `[Timeout]` convention of 60000 ms. For the classes that carry `[Timeout]`, MSTest continues to report first and their behaviour is unchanged — this change introduces no new failure mode for them. For the two classes that carry no `[Timeout]`, the gate bound is the only bound in existence, and it converts an unbounded hang into a named failure. The change is therefore a strict improvement at every call site and a regression risk at none.

### Failure type

Throw `System.TimeoutException`. No new exception type is introduced. The message must:
- name `TransactionGate` and `UiThreadDispatcherFixture`;
- state the bound that elapsed;
- state that the probable cause is a permit held by a test the runner has already reported as finished, so that the reader looks outside the failing test;
- contain the fixed, greppable token **`TRANSACTIONGATE_ACQUIRE_TIMEOUT`**, verbatim and whitespace-free, so the failure can be found in a log. This token was verified on 2026-09-28 to occur nowhere in the repository, so a search for it returns zero hits before the change and at least one hit in the fixture file after it; the plan must assert the token by this literal.

### Files to change

- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` (304 lines; the overload, the constant, the message and the documentation updates are estimated at twenty-five to thirty-five lines, leaving ample headroom under the 500-line ceiling).
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` (396 lines; 104 lines of headroom under the 500-line ceiling. The new test is budgeted at no more than eighty lines including its XML documentation, and the file must remain at or under 500 lines after the addition — see AC7).

Adding the test to the existing test file rather than a new file avoids touching QuickFiler.Test/QuickFiler.Test.csproj, which uses explicit `<Compile Include>` items exclusively (185 occurrences, re-counted 2026-09-28; no globbing, no `EnableDefaultCompileItems`, and no SDK-style default include) and would otherwise require an edit to a shared project file. **Keeping the project file untouched is a requirement of this spec, not an optimisation** — see AC7.

## Determinism Ruling (record verbatim; do not relitigate)

.claude/rules/general-unit-test.md prohibits "real wall-clock waits" in test code. The subject compiles into `QuickFiler.Test`, so the question is whether a bounded acquisition inside fixture infrastructure is such a wait.

**It is not**, and the repository has already settled this reading in code and written down why. Both precedent quotations were re-read on 2026-09-28 and match the tree verbatim:

- QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs lines 48-49: "Bounded, event-driven wait for a state transition. This is not a fixed sleep: it returns as soon as the condition holds, and fails the test with a clear message if it never does."
- TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs line 29: "The outer MSTest `[Timeout]` is a deadlock bound, not a wait." The same file's class doc (lines 15-17) states "no elapsed-time measurement and no real wall-clock wait is used" about a test that nonetheless carries `[Timeout(5000)]` (line 32).

The criterion those establish, which this spec adopts:

> A real-time bound is permitted in test code when (i) it returns immediately once the awaited condition holds, so it contributes nothing to the duration of a passing run, and (ii) it is a failure bound whose expiry is reported as a failure, never the mechanism by which the expected state is reached. A construct that consumes time in order to let something else happen is banned regardless of where it is written.

A bounded `WaitAsync(TimeSpan)` in `BeginTransactionAsync` satisfies both clauses: on the success path it returns the instant the permit is available, exactly as the unbounded form does. The contrary reading would additionally condemn the sixteen pre-existing `[Timeout(...)]` attributes carried by the three gate-consuming classes in this same assembly (seven in the fixture test file, one in WpfUiDispatcherTests.cs, eight in QfcItemController.InitializationTests.Part3.cs; re-counted 2026-09-28), which is not the repository's settled position.

**Corollary that constrains the tests:** a test observing the `false` branch must not reach it by letting the bound elapse, because that would breach clause (i).

## Test Strategy

### Binding operator constraint: the parallel regime stays in force

The tests must keep running in the parallel regime defined by TaskMaster.runsettings (`Workers` 0, `Scope` ClassLevel; re-read 2026-09-28). The new test may not carry `[DoNotParallelize]`, may not use a retry attribute, may not sleep, and may not be serialised by any other means. Version 1.0 of this spec prescribed `[DoNotParallelize]` on the holding class; that guard is withdrawn, and the construction below is shown to be parallel-safe without it.

### The deterministic construction

`SemaphoreSlim.WaitAsync` is documented: "If the timeout is set to zero milliseconds, the method doesn't block. It tests the state of the wait handle and returns immediately." One new test is added to the existing class `QfcItemController_UiThreadDispatcherFixtureTests`, carrying `[TestMethod]` and `[Timeout(GateTimeoutMs)]` (the constant is declared at line 33 of the fixture test file) and using only members already reachable through the file's existing `using` set (`System` is imported, so `TimeoutException`, `TimeSpan`, `Func<>` and `Action` need no new directive). Its shape, in order:

1. **Arrange — hold the permit.** Acquire through the production entry point, `BeginTransactionAsync()` with the production bound. Do **not** call `Install`: the test needs no dispatcher, no `StartRunningDispatcher`, and no write to `UiThread._dispatcher`, so the hold window is the probe plus assertions only, and `Dispose` takes the not-installed path (fixture file lines 296-299) that skips `CompareExchange`. This also keeps the hold from perturbing any test that reads `UiThreadDispatcherFixture.Current`.
2. **Act — probe with a zero bound while holding.** Call the internal overload with `TimeSpan.Zero` through a `Func<Task>` and assert with FluentAssertions `ThrowAsync<TimeoutException>()` that the message contains `TRANSACTIONGATE_ACQUIRE_TIMEOUT`. FluentAssertions 8.11.0 (QuickFiler.Test/packages.config line 8) supports `ThrowAsync`, with in-assembly precedent at QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs line 240. The probe returns the failure outcome immediately, with zero elapsed time and zero scheduling dependence. This is AC4.
3. **Assert — no transaction was constructed on the failure path.** Implied by the throw (the method has no other return) and evidenced independently by step 4.
4. **Assert — counters, still holding.** `TransactionAcquisitions - TransactionReleases` equals one. Only the holder can move either side of the difference, so this is parallel-safe by the same argument as the existing counter-balance test, and it proves the failed probe was not counted as an acquisition. Optionally, capture `ContendedAcquisitions` before the probe and assert the value after is greater than or equal to the value before plus one — the counter is monotonic, so other classes can only add to it. A strict equality on `ContendedAcquisitions` is non-deterministic under parallelism and must not be written.
5. **Assert — the failed probe released nothing.** Still inside the `try`, dispose the held transaction through an `Action` and assert it does not throw `SemaphoreFullException`. Keep an unconditional `transaction.Dispose()` in the `finally` as the safety net; `Dispose` is idempotent (fixture file lines 289-294, proven by R5), so the double call is safe and cannot leak the process-wide permit even if an assertion fails.
6. **Assert — the gate is still usable (round trip).** After the `finally`, acquire again through the **production** entry point and dispose the result, mirroring R5 (fixture test file lines 296-299). Never use `TimeSpan.Zero` here.

The test contains no `Thread.Sleep`, no `Task.Delay`, no stopwatch, no elapsed-time assertion, no `[DoNotParallelize]`, and no retry attribute. Its estimated size is fifty-five to seventy-five lines including XML documentation.

### The rule that makes it parallel-safe

> A `TimeSpan.Zero` acquisition may be used only to assert **failure**, and only while the asserting test itself holds the permit. Success is asserted only through the production entry point.

Why this holds under `Workers` 0 / `Scope` ClassLevel:

- **The probe's observation is deterministic.** `SemaphoreSlim(1, 1)` has exactly one permit. While this test holds it, `CurrentCount` is zero and can be raised only by a `Release()`. The only `Release()` in the assembly is at fixture file line 110, reachable only through this test's own transaction's `Dispose` (single-caller property, line 301). Concurrent acquirers from other classes are parked in the semaphore's wait queue and do not change `CurrentCount`. A zero-bound probe issued by the holder therefore returns `false` immediately, irrespective of how many other classes are running or waiting, and contributes zero time to the run (clause (i) of the determinism criterion).
- **The test's own initial acquisition may wait on another class's hold, and that wait is bounded.** It goes through the production entry point and is bounded by `[Timeout(GateTimeoutMs)]` exactly as the seven existing tests in the class are; the new test adds no exposure category that R1 to R6 and the counter-balance test do not already carry. With the 120000 ms gate bound, MSTest's 60000 ms reports first for this class, so the gate bound never changes this class's observable failure mode.
- **A zero-bound success probe after release would be non-deterministic and is forbidden.** After this test releases its transaction, any other class may acquire the permit at once. The success-path ("gate survives") assertion therefore goes through the production entry point, which waits (bounded by `[Timeout]`) rather than fails when another class holds the permit.
- **No assertion depends on another class not running.** Every assertion is either made while this test holds the sole permit, where no other party can change the observed state, or made through the production entry point, which waits rather than fails under contention.

### Where the `SemaphoreFullException` is detectable

Per the corrected reasoning under Proposed Fix, a wrong-shape release on the failure branch is silent at the release point while the probing test holds the permit. The companion assertion is therefore placed on the probing test's **own `Dispose`** (step 5), which is a deterministic detector in a serial run and a probabilistic one under parallelism, because a parked acquirer may take the wrongly-released permit first. That is acceptable: the regression assertion for AC4 is the `TimeoutException` on the probe, and the **deterministic guards** are (a) the counter-difference assertion in step 4, which reads two if the acquisitions increment is unconditional and reads zero if a wrong release is routed through `ReleaseTransactionGate`, and (b) the code-reviewed control-flow invariant, which is the only guard that catches a raw `TransactionGate.Release()` on the failure branch, since that call bypasses the counters.

### Rejected constructions

- `[DoNotParallelize]` on the holding class: rejected by the operator constraint and shown unnecessary above.
- A `TimeSpan.Zero` success probe after release: non-deterministic under the parallel regime; must not be written.
- A strict `ContendedAcquisitions` equality: non-deterministic under parallelism; use greater-than-or-equal or omit.
- Moving the acquisitions increment before the wait: breaks the counter-balance test on the first failed probe.
- Reflecting onto the private `TransactionGate` field and calling `Wait()` directly: deterministic, but it couples a test to a private field name in its own assembly and a failure between the raw `Wait()` and the raw `Release()` corrupts the gate for the whole process with no `Dispose` to recover it. The same effect is reachable without reflection.
- Asserting on `SemaphoreSlim.CurrentCount` only: deterministic but never drives the failure branch, so it cannot be the regression test.
- Reproducing genuine MSTest abandonment with a very small `[Timeout]`: **not deterministic** and rejected. It depends on cross-class ordering that MSTest does not guarantee, requires a deliberately failing test in the suite, and reaches the expected state by elapsed time, breaching clause (ii).
- Making `TransactionGate` injectable: defeats the file's stated single-owner property.

### Fail-before framing

The `TimeSpan` overload does not exist on the current tree, so a test written against it will not **compile** before the fix rather than fail at runtime; nothing merged since 2026-09-13 changes this. The plan must record this explicitly as a compile-level fail-before with a fail-before-exception dossier under docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/regression-testing/ (filename prefix fail-before-exception. followed by the run timestamp, per `evidence-and-timestamp-conventions`), rather than discovering it at execution time.

### Guards on the new test

- Carry `[Timeout(GateTimeoutMs)]`, matching the local convention (60000 ms, declared at line 33 of the fixture test file).
- No `[DoNotParallelize]`, no retry attribute, no sleep, no delay.
- MSTest, Moq where mocking is needed (none is expected), FluentAssertions for assertions, per the C# Unit Test Policy.

### Existing tests to watch

- `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` (R4, fixture test file lines 204-264) is the only existing test with a genuinely contended acquisition. A bound of 120000 ms is far above its hold window (closed by `transactionA.Dispose()` at line 240 immediately after `secondCallerStarted.Wait()` at line 239), so it is not at risk, but any shorter bound would convert it into a flake. Its behaviour must be unchanged.
- `Transaction_DisposedTwice_DoesNotOverReleaseTheGate` (R5, lines 271-312) is the existing guard against an over-release. Its round-trip acquisition is precisely the assertion that catches a failure path that wrongly released. It must keep passing and must not be weakened.
- `TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition` (lines 355-394) depends only on the counter placement stated under Proposed Fix. It must keep passing unmodified.

### Coverage

The change is confined to test-assembly infrastructure. `QuickFiler.Test` is a test project and is excluded from the coverage denominator, so no production-coverage movement is expected. This is stated rather than measured, and the plan must record the statement rather than assert a coverage delta it cannot produce.

### Committed evidence (referenced by AC10 and AC12)

Committed evidence follows the CLAUDE.md section "Committed Test Evidence Format" exactly:

- For the test run: a test-result summary derived from the trx document, committed as a Markdown projection at docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/mstest-test-result-summary.md, carrying the run timestamp, the command, the exit code, the total, passed, failed and skipped counts, and the names of the tests this change adds. The summary states which figures are derived rather than reported.
- For the coverage run: the package-level JaCoCo projection of the post-processed Cobertura document and the one-line first-party coverage summary, committed as Markdown at docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/coverage-jacoco-projection.md and docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates/coverage-summary.md respectively.
- A raw trx document and a raw coverage collector document (any `.trx`, Cobertura or other coverage `.xml`, or `.coverage` file) are prohibited and must not be added to git in any form, including under the feature folder's evidence tree.
- No committed text — evidence projections, dossiers, plan, or spec — may contain an absolute host path, the developer account name, or the host name. Where a path or identity must be recorded, the placeholders `<repo-root>`, `<user-profile>`, `<user>` and `<host>` are used instead.
- Filenames are fixed (no timestamp in the name) so that acceptance criteria and the plan can name them; the run timestamp is recorded inside each artifact.

## Acceptance Criteria

- [ ] AC1 — The acquisition in `BeginTransactionAsync` is bounded: the parameterless `SemaphoreSlim.WaitAsync()` call no longer appears in the fixture file QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs, and the acquisition uses an overload returning a boolean outcome that the method branches on.
- [ ] AC2 — On a failed acquisition the method throws `System.TimeoutException` whose message names `TransactionGate`, names the elapsed bound, and contains the literal token `TRANSACTIONGATE_ACQUIRE_TIMEOUT`; on that path no `UiThreadDispatcherTransaction` instance is constructed and neither the acquisitions counter nor the releases counter is incremented.
- [ ] AC3 — The production default bound is one hundred twenty thousand milliseconds; an internal acquisition entry point accepting a `TimeSpan` bound exists so that a test can supply `TimeSpan.Zero`; the contended pre-check remains before the wait; and the acquisitions counter is incremented only on the successful branch.
- [ ] AC4 — A new regression test in the existing class of the fixture test file QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs, carrying the MSTest timeout attribute with the class's existing `GateTimeoutMs` constant and carrying no `DoNotParallelize` attribute, acquires a transaction through the production entry point without calling `Install`, probes the internal overload with `TimeSpan.Zero` while still holding that transaction, and asserts that `TimeoutException` is thrown with a message containing `TRANSACTIONGATE_ACQUIRE_TIMEOUT`. The test contains no `Thread.Sleep`, no `Task.Delay`, no retry attribute, and no elapsed-time assertion.
- [ ] AC5 — While still holding the transaction, the same test asserts that `TransactionAcquisitions` minus `TransactionReleases` equals exactly one; it then disposes its own transaction inside the `try` and asserts that the disposal does not throw `SemaphoreFullException`, keeps an unconditional disposal in the `finally` as an idempotent safety net, and afterwards acquires and disposes a further transaction through the production entry point, never through a `TimeSpan.Zero` probe.
- [ ] AC6 — The seven pre-existing tests in the fixture test file, including `TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition`, are unmodified and all pass, and no test in any of the five consuming files listed in the research blast radius required an edit.
- [ ] AC7 — QuickFiler.Test/QuickFiler.Test.csproj is not modified, and no file is added to or removed from the project; the anchored diff against the plan's recorded base lists exactly the write-set paths the plan declares; and the fixture test file remains at or under five hundred total lines after the addition.
- [ ] AC8 — The spec's determinism criterion is reproduced in the plan or in an evidence artifact, and the change is shown to satisfy both of its clauses.
- [ ] AC9 — A compile-level fail-before-exception dossier exists under the feature folder's regression-testing evidence directory, recording why a runtime fail-before run is structurally impossible for AC4.
- [ ] AC10 — The full C# toolchain passes in a single pass in the required order: `dotnet tool run csharpier check .` clean, analyzer rebuild with zero warnings and zero errors, nullable rebuild with zero warnings and zero errors, and the `QuickFiler.Test` suite passing with no fewer tests than the recorded baseline plus the one test this change adds; the run is evidenced only by the Markdown projections named under "Committed evidence" in the Test Strategy.
- [ ] AC11 — No shipped add-in production file is modified. The write set contains no path outside the QuickFiler.Test project directory and the feature folder.
- [ ] AC12 — The diff adds no raw test-platform or coverage-collector document (no trx, no coverage XML, no coverage binary) anywhere in the repository, and no committed text in the feature folder contains an absolute host path, the developer account name, or the host name; the placeholder set listed under "Committed evidence" in the Test Strategy is used wherever such a value would otherwise appear.

## Risks & Mitigations

| Risk | Mitigation |
|---|---|
| A failure path that releases the permit breaks mutual exclusion silently at the release point, and the `SemaphoreFullException` surfaces only later at a legitimate holder's `Dispose`. | The control-flow invariant and counter placement above, enforced by review; the counter-difference assertion in AC5 as the deterministic detector for a wrong release routed through `ReleaseTransactionGate` or an unconditional acquisitions increment; the `SemaphoreFullException` assertion on the probing test's own `Dispose`; and the pre-existing `Transaction_DisposedTwice_DoesNotOverReleaseTheGate` guard. |
| A bound shorter than the longest legitimate hold manufactures false failures. | The bound is fixed at twice the 60000 ms `[Timeout]` that bounds the longest hold, and the rationale is recorded so a later reduction has to argue against it. |
| The new test holds a process-wide permit while other classes run concurrently under `Scope` ClassLevel. | The parallel-safety rule: zero-bound probes assert only failure and only while this test holds the permit; success is asserted only through the production entry point; the hold window is the probe plus assertions with no `Install`; release in a `finally`. No serialisation, retry or sleep is used. |
| An unconditional or pre-wait acquisitions increment silently breaks the issue #743 counter-balance test. | Counter placement fixed under Proposed Fix and asserted by AC3; the counter-balance test must keep passing unmodified (AC6). |
| A test in a `[Timeout]`-carrying class is abandoned while parked on the gate, and the 120000 ms bound later elapses inside the abandoned continuation, throwing into a task nobody observes. | Recorded so it is not mistaken for a new hazard: no `UnobservedTaskException` subscriber exists in any test assembly (refreshed research section 5.2) and the .NET Framework default does not fail the process on an unobserved task fault, so nothing observable changes. |
| The change is mistaken for a fix for the issue #823 R4 flake. | Declared a non-goal above; the plan must not cite R4 stability as evidence. |
| The bounded wait is read as a banned wall-clock wait. | The determinism ruling above, with the in-tree precedent and the two-clause criterion, recorded so review does not relitigate it. |
| Raw tool output or host-identifying text is committed as evidence. | AC12 and the "Committed evidence" subsection: projections only, fixed filenames, placeholders for paths and identities. |

## Rollout & Follow-up

- Follow-up issue to consider: add a `CancellationToken`-observing acquisition overload flowing `TestContext.CancellationTokenSource.Token`, addressing H-TOKEN-BLIND directly. Deferred here for blast-radius reasons.
- Follow-up candidates only (not in scope, no change proposed here): two further unbounded `SemaphoreSlim.WaitAsync()` calls exist in the same assembly, both on per-instance test-local semaphores rather than a process-wide static, so each exposure is confined to one test, but each is the same shape. They are at QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs line 391 and QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs line 305 (the latter inside a `Task.WhenAny`; both re-verified 2026-09-28).
- Links: issue #882; originating issue #743 (now merged); related flake reports #592, #511, #571, #823; frequently miscited as closing this area, #493.

## Revision Log

- **1.1 (2026-09-28T00-30).** Every line and identifier citation re-derived against the branch tree after the merge of issue #743 (fixture file 304 lines, fixture test file 396 lines; `BeginTransactionAsync` lines 142-152; `ReleaseTransactionGate` lines 107-111; sole `Dispose` caller line 301; construction line 151). MSTest version corrected to 4.4.1; acquisition inventory corrected from fourteen to fifteen statements across five files; `<Compile Include>` count corrected from 173 to 185; pre-existing test count corrected from six to seven (AC6). Issue #743 counter placement added to the control-flow invariant and to AC2 and AC3. The `[DoNotParallelize]` guard and its Risks row withdrawn under the binding parallel-regime constraint and replaced by the refreshed research section 5 design and its parallel-safety rule (AC4, AC5). The `SemaphoreFullException` reasoning corrected: a wrong release on the failure branch is silent at the release point while the probing test holds the permit and surfaces at the legitimate holder's `Dispose`; AC5's assertion moved to the test's own `Dispose`, with the counter-difference assertion and the reviewed invariant named as the deterministic guards. Greppable token declared literally as `TRANSACTIONGATE_ACQUIRE_TIMEOUT`. Five-hundred-line headroom requirement added to AC7. Committed-evidence requirements added (Test Strategy subsection, AC10, new AC12). Third unbounded `WaitAsync()` (BreadcrumbPopupBoundaryCoverageTests) added to Rollout as a follow-up candidate. Context-only repository paths converted from code spans to plain text so that only the two written files appear as backticked path tokens.
- **1.0 (2026-09-13T19-40).** Initial approved-for-planning version.
