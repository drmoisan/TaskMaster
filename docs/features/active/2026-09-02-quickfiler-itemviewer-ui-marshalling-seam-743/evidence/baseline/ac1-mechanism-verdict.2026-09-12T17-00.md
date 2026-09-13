# AC1 mechanism verdict (P1-T11)

Task: [P1-T11]
Timestamp: 2026-09-13T02-50
Command: none (verdict authored from the P0-T11 declaration and the P1-T9 and P1-T10 measurement artifacts in this folder)
EXIT_CODE: 0
Output Summary: the serial-regime figures select row 1 of the pre-declared decision rule; H-LEAK is REJECTED by direct observation and H-COST (elapsed fixture cost) is the single operative mechanism. No expiry was observed in either instrumented run.

## (i) The discriminating observable (restated verbatim from the P0-T11 declaration)

The observable is whether any acquisition of the one-permit `TransactionGate` (the `SemaphoreSlim(1, 1)` declared at line 32 of the fixture file) finds the permit held with no live holder. A live holder is a transaction obtained from `BeginTransactionAsync` (fixture lines 122-126) that has not yet run `ReleaseTransactionGate` (fixture lines 88-91) through its `Dispose`. A permit found held with no live holder can only be the result of a leaked or late-released transaction, which is the H-LEAK hypothesis; a permit that is never found held in a serial run leaves elapsed fixture cost (H-COST) as the only surviving mechanism.

The declaration's item (e) is also restated verbatim: The mechanism names UiThreadDispatcherGate and SwapUiThreadDispatcher are invalid: correction C1 of spec.md records that both exist in zero .cs files in this tree.

## (ii) Measured counter triples

| Regime | Exact command | Load condition | acquisitions | releases | contended | Balance test |
|---|---|---|---|---|---|---|
| SERIAL | `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/Logger:trx;LogFileName=p1-t9-ac1-serial.trx" /ResultsDirectory:coverage\trx\p1-t9 "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~Transaction_SecondCallerCannotInstallUntilTheFirstRestores"` | otherwise-idle machine, no induced load, Outlook closed, machine build lock held so no sibling item's build or test run overlapped; R4 excluded by filter (live-holder contention by design) | 11 | 10 | 0 | Passed (11 - 10 = 1) |
| PARALLEL | `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:TaskMaster.runsettings /InIsolation "/Logger:trx;LogFileName=p1-t10-ac1-parallel.trx" /ResultsDirectory:coverage\trx\p1-t10 "/TestCaseFilter:TestCategory!=LiveOutlook"` | same machine state; Workers 0 / Scope ClassLevel so distinct test classes ran concurrently; R4 included | 19 | 18 | 14 | Passed (19 - 18 = 1) |

Source artifacts: `evidence/baseline/ac1-serial-measurement.2026-09-12T17-00.md` (P1-T9) and `evidence/baseline/ac1-parallel-measurement.2026-09-12T17-00.md` (P1-T10). Both runs were taken from the same instrumented assembly built in P1-T8, on 2026-09-13 between 02:47 and 02:50 local time.

## (iii) Verdict by the pre-declared decision rule

The P1-T9 table, fixed in advance and reproduced in the P0-T11 declaration, is applied to the serial-regime figures only:

| Serial-run contended count | Serial-run balance test | Verdict |
|---|---|---|
| 0 | passed (difference equals 1) | H-LEAK REJECTED by direct observation; H-COST is the surviving mechanism |

Measured: serial contended count = 0; serial balance test = passed with difference exactly 1. Row 1 is selected.

**REJECTED hypothesis: H-LEAK** (a leaked or late-released transaction leaving the one-permit gate held with no live holder). **Observation that rejects it:** across the whole serial-regime run of 1394 tests, with the only designed live-holder contention test excluded, every one of the 11 acquisitions found the permit free (`contended=0`), and at the moment the balance test held the permit the acquisition and release counters differed by exactly 1, so no earlier transaction in the run had been left unreleased. A serial run cannot queue a second live holder; therefore a contended count of zero in that regime is a direct observation that the permit was never found held by a leaked transaction.

**Operative mechanism (exactly one): H-COST** — the elapsed cost of the pump-hosted fixtures themselves, elongated under load, is what consumes the per-test bound. The parallel-regime contended count of 14 does not contradict this: in that regime distinct test classes genuinely queue on the gate with a live holder, and its balance test also passed with difference 1, so those 14 contended acquisitions were live-holder queueing rather than leaks. The two hypotheses are not both supported; only H-COST survives.

## (iv) Elapsed durations bounding the construction-cost contribution

Serial-regime (P1-T9) `ThroughThePumpHost` durations, transcribed:

| Test | duration (ms) |
|---|---|
| InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme | 85.2421 |
| InitializeBool_ThroughThePumpHost_CompletesAndInitializesState | 81.7954 |
| InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates | 85.7493 |
| InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults | 124.5081 |
| InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState | 111.1364 |
| ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups | 68.1703 |

Largest serial duration: 124.5081 ms. The recorded load multiplier is 6x to 26x the unloaded duration under sustained CPU saturation (spec.md line 99, from the ten-run #511 determinism evidence). Arithmetic against the 60,000 ms bound:

- 124.5081 ms x 6 = 747.0 ms, which is 1.25 percent of 60,000 ms.
- 124.5081 ms x 26 = 3,237.2 ms, which is 5.40 percent of 60,000 ms.
- The bound divided by the largest serial duration is 60,000 / 124.5081 = 481.9x; an expiry from construction cost alone therefore requires an elongation roughly 18.5 times larger than the upper recorded multiplier when the machine is otherwise idle.

Supplementary observation from the parallel-regime run (P1-T10): the same six tests elongated to between 123.9 ms and 6,460.4 ms (InitializeSequentialAsync, 58x its serial figure) with only class-level parallelism and no external load. Under that regime the largest measured duration times the upper multiplier, 6,460.4 ms x 26 = 167,970 ms, exceeds the 60,000 ms bound; this is the reachable path by which elapsed fixture cost, not a gate leak, can consume the per-test timeout when class-level parallelism and CPU saturation coincide. It is consistent with, and does not replace, the counter-based identification above.

## Expiry statement (recorded negative result)

Neither instrumented run produced any expiry: the serial run recorded `timeout=0` and `failed=0` over 1394 tests, and the parallel run recorded `timeout=0` and `failed=0` over 1395 tests. No `PumpTimeoutMs` or `GateTimeoutMs` expiry was reproduced in this session. This is stated plainly as a negative result. The mechanism identification in section (iii) rests on the counter observable declared in advance in P0-T11, namely the serial-regime contended count and the balance-test difference, and not on an observed expiry.
