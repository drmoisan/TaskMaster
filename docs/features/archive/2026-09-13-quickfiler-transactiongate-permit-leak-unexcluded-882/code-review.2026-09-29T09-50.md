# Code Review — Issue #882 (QuickFiler `TransactionGate` bounded acquisition)

- Date: 2026-09-29 (artifact stamp `2026-09-29T09-50`, authoring stamp; no shell clock was available to this review)
- Branch: `bug/quickfiler-transactiongate-permit-leak-unexcluded-882`
- Head: `865a473f9e3b0f6322859d0e8e3ccd776ffb40d3`
- Base: `177b6d78e1b2408e5aedbd794cef3aad6b7fb372`
- Reviewer verdict: **ACCEPT** — 0 blocking findings, 0 non-blocking code findings, 4 informational observations

## Executive Summary

Two C# files changed, both in the `QuickFiler.Test` project and both read in full by the reviewer with the Read tool (the Bash tool was not used, at the caller's direction). The fixture change is a 38-line net addition that bounds a process-wide `SemaphoreSlim(1, 1)` acquisition and exposes the bound through an `internal` `TimeSpan` overload; the test change is one 62-line regression test appended to the existing fixture test class. The control-flow invariant the spec makes load-bearing — the releasing object is constructed only after the acquisition returned `true`, and nothing on the failure branch releases, constructs, or counts — holds by inspection at fixture lines 173-189. Every existing caller compiles unchanged because all 18 acquisition sites in the assembly use the parenthesised form. The new test is deterministic and parallel-safe under `Workers 0 / Scope ClassLevel` by the argument the spec sets out, and the reviewer found no gap in that argument. No change is recommended; the four observations below are recorded for a future maintainer.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Informational | `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` | lines 440-445 | `NotThrow<SemaphoreFullException>()` is the generic form, which fails only when an exception assignable to `SemaphoreFullException` is thrown; any other exception from `transaction.Dispose()` would not fail this assertion and would surface only through the later round-trip timeout. | None. Spec AC5 prescribes exactly this assertion shape, and `Dispose` has no realistic alternative throw path (`CompareExchange` under a lock, then counter increment and `Release()`). | The shape is spec-mandated and a broader `NotThrow()` would not change the test's discriminating power for the defect class it guards. | Reviewer read of `UiThreadDispatcherTransaction.Dispose` (fixture lines 325-340). |
| Informational | `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` | lines 173-176 | The contended pre-check reads `CurrentCount` and then waits; the two are not atomic, so a release between them counts an immediate acquisition as contended. | None. This is the issue #743 definition ("observed `CurrentCount == 0` immediately before waiting"), unchanged by this change, and the counter is only ever asserted with `>=`. | The spec explicitly keeps the pre-check before the wait so that a failed probe is counted as contended; changing the placement would break the counter-balance test. | Fixture doc comment lines 40-43; spec "Counter placement". |
| Informational | `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` | lines 178-186 | A caller abandoned by MSTest's `[Timeout]` while parked keeps waiting in the background. If the permit frees within the bound the continuation acquires and constructs a transaction no one disposes; if the bound expires first the `TimeoutException` faults an unobserved task. | None in this change. The `CancellationToken`-observing overload is the recorded follow-up. | The change converts the downstream symptom from an unbounded hang into a named failure after at most 120000 ms; it does not claim to remove the leak class, and the spec's Risks table records both outcomes. | Spec "Risks & Mitigations" rows 5 and "Rollout & Follow-up". |
| Informational | `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` | line 146 | The bound is a `const int` in milliseconds converted with `TimeSpan.FromMilliseconds` at each parameterless call. | None. A `static readonly TimeSpan` would remove the per-call conversion but would not be usable in the cref-bearing XML documentation as a compile-time constant, and the conversion cost is negligible on a path that awaits a semaphore. | Deliberate simplicity; the constant's doc comment records both anchors of the value. | Fixture lines 140-160. |

## 1. Fixture change — `QfcItemController.UiThreadDispatcherFixture.cs` (304 → 342 lines)

### The bounded acquisition

```csharp
internal const int TransactionGateAcquireTimeoutMs = 120000;

internal static Task<UiThreadDispatcherTransaction> BeginTransactionAsync()
{
    return BeginTransactionAsync(
        TimeSpan.FromMilliseconds(TransactionGateAcquireTimeoutMs)
    );
}

internal static async Task<UiThreadDispatcherTransaction> BeginTransactionAsync(
    TimeSpan bound
)
{
    if (TransactionGate.CurrentCount == 0)
    {
        Interlocked.Increment(ref _contendedAcquisitions);
    }

    bool acquired = await TransactionGate.WaitAsync(bound).ConfigureAwait(false);
    if (!acquired)
    {
        throw new TimeoutException(
            "TRANSACTIONGATE_ACQUIRE_TIMEOUT: UiThreadDispatcherFixture.TransactionGate was not acquired within "
                + bound.TotalMilliseconds.ToString("0", CultureInfo.InvariantCulture)
                + " ms. The probable cause is a permit held by a test the runner has already reported as finished (issue #882)."
        );
    }

    Interlocked.Increment(ref _transactionAcquisitions);
    return new UiThreadDispatcherTransaction();
}
```

**Assessment: correct, and matches the spec's prescribed shape exactly.**

- **Counter ordering is right.** The contended pre-check (175) precedes the wait (178). The acquisitions increment (188) sits after the `if (!acquired) throw` (179-186), so it executes only when the permit is held. The reviewer checked the placement against the two wrong shapes the spec names: an increment before the wait, or an unconditional one, would make `TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition` read 2 after the first failed probe in a process. That test Passed in the full run after the new test had already run in the same process (the scoped run executed both in one process as well), which is a live confirmation of the placement, not only an inspection.
- **Nothing on the failure path releases or constructs.** `TransactionGate.Release()` occurs once in the file (line 113, inside `ReleaseTransactionGate`), whose only caller is `UiThreadDispatcherTransaction.Dispose` (339); `new UiThreadDispatcherTransaction()` occurs once (189), after the increment. No `try`/`finally` wraps the wait, so the "release on failure" shape the spec calls out as worse than the defect is absent. `_transactionReleases` is untouched on the failure path.
- **Overload resolution for existing callers.** The reviewer enumerated every `BeginTransactionAsync` reference in `QuickFiler.Test`: 3 in `QfcFormControllerUndoHandoffTests.cs` (`using (var transaction = await …BeginTransactionAsync())`), 1 in `QfcHomeControllerRunAsyncTests.cs`, 1 in `QfcItemController.InitializationTests.Part2.cs`, 1 in `WpfUiDispatcherTests.cs`, 12 in the fixture test file (9 pre-existing plus 3 in the new test), all with an argument list. No method-group conversion (`Func<Task<…>> f = UiThreadDispatcherFixture.BeginTransactionAsync;`) exists anywhere, so introducing the overload cannot have created an ambiguity, and the two solution-wide rebuilds confirm it. The three `using (var transaction = await …)` sites remain correct because a throw from the awaited call happens before the `using` scope is entered.
- **The parameterless overload is a plain `Task`-returning delegation, not `async`.** That is the right choice: it adds no state machine and preserves the exception-on-the-task semantics callers already have (every caller awaits the result).
- **The `TimeoutException` message meets all four spec requirements**: it names `TransactionGate` and `UiThreadDispatcherFixture`, states the elapsed bound (`bound.TotalMilliseconds` formatted with `"0"` under the invariant culture, so `120000` or `0`, never a localised separator), directs the reader outside the failing test, and carries the whitespace-free token `TRANSACTIONGATE_ACQUIRE_TIMEOUT` at the start of the message so that the FluentAssertions wildcard `*TRANSACTIONGATE_ACQUIRE_TIMEOUT*` and any log grep both find it.
- **`SemaphoreSlim.WaitAsync(TimeSpan)` contract.** For `TimeSpan.Zero` the method tests state and returns immediately without blocking, which is what makes the test's probe deterministic; for 120000 ms the value is inside the accepted range (`-1` to `Int32.MaxValue` milliseconds), so no `ArgumentOutOfRangeException` is possible from the production default.
- **The bound is justified in-code.** Twice the 60000 ms `[Timeout]` that bounds the longest legitimate hold (`PumpHarness`), and half the four-minute `/Blame` hang guard, so an expired bound is a named test failure rather than a hang dump. R4 (`Transaction_SecondCallerCannotInstallUntilTheFirstRestores`) closes its hold window immediately after `secondCallerStarted.Wait()`, so it is far inside the bound; the reviewer confirms the test is unmodified and Passed.

### Documentation changes

- The class summary now states the acquisition is bounded by `TransactionGateAcquireTimeoutMs` and throws `TimeoutException` on expiry, immediately before the unchanged lock-ordering sentence (`TransactionGate` then `FieldLock`).
- The `UiThreadDispatcherTransaction` cref was changed from `UiThreadDispatcherFixture.BeginTransactionAsync` to `UiThreadDispatcherFixture.BeginTransactionAsync()`. This is required once the method group is overloaded: the bare cref would produce CS0419 (ambiguous reference) under the nullable gate's `TreatWarningsAsErrors`. The gate is clean, so the cref resolves.
- `using System.Globalization;` is the only new directive and is used exactly once.

## 2. Test change — `QfcItemController.UiThreadDispatcherFixtureTests.cs` (396 → 458 lines)

One method, `BeginTransactionAsync_ZeroBoundWhileThisTestHoldsThePermit_ThrowsTimeoutExceptionAndReleasesNothing`, lines 396-456, in the existing class so that `QuickFiler.Test.csproj` (explicit `<Compile Include>` items) is untouched.

### Determinism and parallel safety

The reviewer checked each element of the spec's parallel-safety rule ("a `TimeSpan.Zero` acquisition may be used only to assert failure, and only while the asserting test itself holds the permit; success is asserted only through the production entry point") against the delivered code:

1. **Arrange (410-412).** Acquires through `BeginTransactionAsync()` with the production bound, bounded externally by `[Timeout(GateTimeoutMs)]` (60000 ms) like the other seven tests. No `Install`, so the hold touches no dispatcher state and `Dispose` takes the not-installed path.
2. **Act (418-419).** `Func<Task> probe = () => UiThreadDispatcherFixture.BeginTransactionAsync(TimeSpan.Zero);` inside the `try`, while the permit is held. `SemaphoreSlim(1, 1)` has one permit; while this test holds it, `CurrentCount` is 0 and can only be raised by a `Release()` reachable through this test's own transaction. Parked acquirers from other classes queue and do not change `CurrentCount`. The probe therefore returns `false` immediately, regardless of what other classes are doing.
3. **Assert, failure (422-427).** `ThrowAsync<TimeoutException>().WithMessage("*TRANSACTIONGATE_ACQUIRE_TIMEOUT*")`.
4. **Assert, counters (428-439).** `TransactionAcquisitions − TransactionReleases` is asserted equal to 1 while holding — only the holder can move either side, because an acquisition is counted only after the wait returns `true` and a release is counted only by a transaction's `Dispose`, and `ReleaseTransactionGate` increments `_transactionReleases` before calling `Release()`, so a predecessor's release is always counted before this test can acquire. `ContendedAcquisitions` is asserted `>= contendedBefore + 1`, which is monotonic-safe; a strict equality would be non-deterministic under parallelism and was correctly not written.
5. **Assert, no over-release (440-445).** The holder's own `Dispose` is invoked through an `Action` and asserted not to throw `SemaphoreFullException`. Because `_disposed` is set before the release, the unconditional `finally` disposal (449) is a no-op after this and cannot double-release; R5 already proves that idempotence.
6. **Round trip (452-455).** A second acquisition through the production entry point, then disposal. Never `TimeSpan.Zero` here, which is correct: after release any other class may hold the permit, and the production form waits rather than fails.

The test contains no `Thread.Sleep`, `Task.Delay`, `Stopwatch`, elapsed-time assertion, retry attribute or `DoNotParallelize` (reviewer full read; the executor's token counts agree). Every FluentAssertions call carries a `because` string. The XML summary states the scenario, the three properties asserted, and why no wall-clock time is consumed.

### Fail-before

The `TimeSpan` overload did not exist on the base tree, so the test could not compile before the fix. The executor recorded a compile-level dossier: `Build FAILED`, one `error CS1501: No overload for method 'BeginTransactionAsync' takes 1 arguments` at the probe line, exit 1. That is the correct fail-before shape here and the only one structurally available; a runtime red run would require the overload to exist.

## 3. Cross-cutting quality

| Dimension | Assessment |
|---|---|
| Simplicity | One constant, two overloads, one `if`. No new type or abstraction. |
| Reusability | The bound is a single named constant; the parameterless overload delegates. |
| Separation of concerns | Fixture infrastructure only; no production code touched, no runsettings or project file touched. |
| Error handling | Fail-fast `TimeoutException` with a greppable token and a cause hint; no swallow. |
| Naming | `TransactionGateAcquireTimeoutMs`, `bound`, `acquired`, `probe`, `roundTrip`, `contendedBefore`. |
| Comments | Explain why: the two anchors of the bound, why the pre-check stays before the wait, why no counter moves on failure, why the test consumes no time. |
| Public API stability | Both members `internal`; parameterless signature unchanged. |
| File sizes | 342 and 458 lines, both under 500. |
| Formatting | CSharpier 1.2.6 output (the split `TimeSpan bound` signature and the wrapped delegating return are the formatter's rewraps, which the plan anticipated); repository-wide `check .` clean over 1623 files. |
| Analyzers / nullable | Both solution-wide `/t:Rebuild` gates `0 Warning(s) 0 Error(s)` with the test DLL observed fresh. |

## 4. Verdict

**ACCEPT. 0 blocking findings, 0 non-blocking findings, 4 informational observations.**

The change is the minimal shape the spec prescribes, the invariant that makes it safe is verified by inspection and corroborated by the counter-balance test passing in the same process as the new test, every caller is unaffected, and the regression test is deterministic under the repository's parallel regime without any of the prohibited stabilisers.
