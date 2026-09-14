# AC1 discriminating observable — declared in advance of any instrumentation (P0-T11)

Task: [P0-T11]
Timestamp: 2026-09-13T02-36
Declared before P1-T6 and P1-T7 add any counter or test to the fixture files. At the time of writing, `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` (278 lines) contains zero occurrences of `Interlocked.Increment`, and `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` (353 lines) contains zero occurrences of `GATECOUNTERS`.

## (a) The observable

The observable is whether any acquisition of the one-permit `TransactionGate` (the `SemaphoreSlim(1, 1)` declared at line 32 of the fixture file) finds the permit held with no live holder. A live holder is a transaction obtained from `BeginTransactionAsync` (fixture lines 122-126) that has not yet run `ReleaseTransactionGate` (fixture lines 88-91) through its `Dispose`. A permit found held with no live holder can only be the result of a leaked or late-released transaction, which is the H-LEAK hypothesis; a permit that is never found held in a serial run leaves elapsed fixture cost (H-COST) as the only surviving mechanism.

## (b) Operationalisation

Three monotonic counters are added to `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`:

1. acquisitions — incremented immediately after `await TransactionGate.WaitAsync()` returns in `BeginTransactionAsync`;
2. releases — incremented immediately before the existing `TransactionGate.Release()` in `ReleaseTransactionGate`;
3. contended acquisitions — incremented immediately before the wait in `BeginTransactionAsync` when, and only when, `TransactionGate.CurrentCount == 0` was observed at that instant.

All increments use `Interlocked.Increment`; the accessors return `Volatile.Read` of the respective field. No wait, sleep, `Stopwatch` or timeout is added. A balance test (P1-T7) begins a transaction and, while holding the sole permit, asserts `TransactionAcquisitions - TransactionReleases == 1`, then writes one line of the exact form `GATECOUNTERS acquisitions=<n> releases=<n> contended=<n>` through `TestContext.WriteLine`. The assertion is order-independent: every predecessor transaction that was released contributes equally to both counters, so the difference is 1 if and only if no predecessor leaked.

## (c) Decision rule (reproduced verbatim from the table in P1-T9 of the plan)

| Serial-run contended count | Serial-run balance test | Verdict |
|---|---|---|
| 0 | passed (difference equals 1) | H-LEAK REJECTED by direct observation; H-COST is the surviving mechanism |
| greater than 0 | any | H-LEAK OPERATIVE: a serial run found the permit held, which requires a leaked or late-released transaction |
| 0 | failed (difference greater than 1) | H-LEAK OPERATIVE: acquisitions exceed releases |

Only the SERIAL-regime figures (no `/Settings:` argument, which is CI's regime) select a row. In the PARALLEL regime (`/Settings:TaskMaster.runsettings`, Workers 0, Scope ClassLevel) distinct test classes genuinely queue on the one permit, so a contended count greater than zero there is the expected consequence of live-holder queueing and does not by itself indicate a leak.

## (d) Load condition

The measurement will be taken on an otherwise-idle machine with no induced load, with Outlook closed, and with msbuild and vstest serialized across concurrently in-flight items by the shared machine build lock so that no other item's build or test run overlaps the measurement. The one existing test that starts a second transaction while the first is held, `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` (fixture test file lines 202-262; the second `BeginTransactionAsync` inside `Task.Run` at lines 220-234 with the first transaction still held until line 238), is excluded from the serial-regime run by test-case filter (`FullyQualifiedName!~Transaction_SecondCallerCannotInstallUntilTheFirstRestores`), because its designed contention has a live holder and lies outside the observable. It remains included in the parallel-regime run.

## (e) Invalid mechanism names

The mechanism names UiThreadDispatcherGate and SwapUiThreadDispatcher are invalid: correction C1 of spec.md records that both exist in zero .cs files in this tree.

(Re-verified at the time of this declaration: a repository-wide search of `*.cs` for either identifier returns zero matches.)
