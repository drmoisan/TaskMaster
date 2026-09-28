# AC1 mechanism verdict (P1-T11)

Task: [P1-T11]
Timestamp: 2026-09-13T02-50
Command: none (verdict authored from the P0-T11 declaration and the P1-T9 and P1-T10 measurement artifacts in this folder)
EXIT_CODE: 0
Output Summary: both instrumented runs recorded `timeout=0`. No expiry occurred, and therefore NEITHER H-COST NOR H-LEAK WAS DISCRIMINATED by this measurement. There was no expiry event in which to observe whether the one-permit `TransactionGate` was held with no live holder, which is the observable the P0-T11 declaration named. A hypothesis cannot be rejected by the absence of observations. This artifact records a negative result: the mechanism was not identified by measurement. AC1 is NOT marked PASS and its checkbox in `spec.md` is unchecked.

## Correction history (this artifact has been corrected twice; both superseded texts are retained below)

**CORRECTION 1 (review finding R-2, applied 2026-09-13, ITSELF SUPERSEDED BY CORRECTION 2).** Its text was:

> AMENDMENT (review finding R-2, applied 2026-09-13). The phrase "rejected by direct observation" above originally stood without its object, which read as though an expiry had been observed and attributed. It was not. Nothing in this artifact rests on a reproduced expiry: the serial run recorded `timeout=0` over 1394 tests and the parallel run `timeout=0` over 1395, as section (v) states. The rejection of H-LEAK rests entirely on the counter observable, which is a legitimate basis and was declared before the measurement rather than chosen after it.

Correction 1 identified the right defect and then defended the wrong claim. Having established that no expiry was reproduced, it went on to assert that the rejection of H-LEAK "rests entirely on the counter observable, which is a legitimate basis". That assertion is withdrawn. It is not a legitimate basis, for the reason Correction 2 states.

**CORRECTION 2 (maintainer ratification of R-2, applied 2026-09-13).** The superseded Output Summary text was:

> Output Summary: the serial-regime figures select row 1 of the pre-declared decision rule; H-LEAK is REJECTED by direct observation OF THE PRE-DECLARED COUNTER OBSERVABLE — the serial-regime contended count and the balance-test difference declared in advance at P0-T11 — and H-COST (elapsed fixture cost) is the single surviving mechanism. No expiry was observed in either instrumented run.

Why both superseded texts are wrong. Section 4.2 of `spec.md` defines H-LEAK as a leak that FOLLOWS a timed-out `async` test whose `finally` MSTest has stopped observing. H-LEAK is therefore conditional on an expiry having already occurred. Both instrumented runs recorded `timeout=0`, so no test was abandoned, so under EITHER hypothesis no leak could have occurred in these runs. The serial reading `contended=0` was consequently PREDETERMINED by the absence of expiry: it would have read zero whether H-LEAK is true of this codebase or false. A reading that is fixed in advance by a condition independent of the hypothesis carries no information about the hypothesis, so it cannot reject it. The counter observable was declared in advance and was measured honestly; what failed is that the run never entered the regime in which the observable becomes discriminating.

The correct statement of what these runs establish: no expiry was reproduced, so the discriminating experiment did not take place. H-COST remains the only available ORIGINATING mechanism within the spec's two-hypothesis frame, because H-LEAK is a cascade conditional on an initial expiry rather than an originating cause; the sufficiency evidence for H-COST is the measured parallel-regime elongation in section (iv). That is a narrower claim than "H-LEAK is rejected". H-LEAK IS NOT REJECTED. Spec unknown U2 — whether a first expiry cascades through a leaked permit — remains OPEN and UNTESTED by this item. It is carried forward as a separate issue; see the ratification record at `evidence/other/maintainer-ratification-ac1.2026-09-13T18-00.md`.

CONSEQUENCE FOR AC1, STATED EXPLICITLY SO IT IS NOT INFERRED EITHER WAY. AC1's no-expiry clause is NOT satisfied by this evidence, so AC1 is NOT marked PASS and its checkbox in `spec.md` is unchecked. The defect did not reproduce in 62 targeted runs or in 1394 serial runs.

ESCALATION OUTCOME (updated by Correction 2). Whether a non-reproducing negative result of this shape discharges AC1 was a judgment reserved to the maintainer. It is no longer unresolved: THE MAINTAINER HAS RULED, on 2026-09-13, ratifying the negative result and accepting the item subject to four conditions recorded in `evidence/other/maintainer-ratification-ac1.2026-09-13T18-00.md`. The ratification is the maintainer accepting the item DESPITE a negative result. It is not a finding that the result was positive, and it does not convert AC1 into a pass. The checkbox stays unchecked because the checkbox records what was measured; the ratification records the maintainer's acceptance. Both statements belong in the record and neither replaces the other.

## (i) The discriminating observable (restated verbatim from the P0-T11 declaration)

The observable is whether any acquisition of the one-permit `TransactionGate` (the `SemaphoreSlim(1, 1)` declared at line 32 of the fixture file) finds the permit held with no live holder. A live holder is a transaction obtained from `BeginTransactionAsync` (fixture lines 122-126) that has not yet run `ReleaseTransactionGate` (fixture lines 88-91) through its `Dispose`. A permit found held with no live holder can only be the result of a leaked or late-released transaction, which is the H-LEAK hypothesis; a permit that is never found held in a serial run leaves elapsed fixture cost (H-COST) as the only surviving mechanism.

The declaration's item (e) is also restated verbatim: The mechanism names UiThreadDispatcherGate and SwapUiThreadDispatcher are invalid: correction C1 of spec.md records that both exist in zero .cs files in this tree.

CITATION NOTE (added 2026-09-13, after merging `main` at e6d86049e). The line numbers in the verbatim restatement above are PRE-INSTRUMENTATION and no longer resolve. They were correct when the P0-T11 declaration was authored and were shifted by this item's own P1 instrumentation, which inserted the three monotonic counters into the same file. They were NOT shifted by the merge: the merge changed no file this artifact cites. The restatement is left exactly as declared, because the in-advance declaration is the record of what was declared in advance and must not be rewritten after the fact. The current locations, re-derived against the merged tree on 2026-09-13, are:

| Element | Cited above (pre-instrumentation) | Current line(s) in `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` |
|---|---|---|
| `SemaphoreSlim(1, 1)` declaration of `TransactionGate` | 32 | 32 (unchanged) |
| `ReleaseTransactionGate` | 88-91 | declared at 107; releases the permit at 110 |
| `BeginTransactionAsync` | 122-126 | declared at 142; awaits `TransactionGate.WaitAsync()` at 149 |

Line 32 is unchanged and is the citation that matters for correction C2 of `spec.md`: `TransactionGate` is still `new SemaphoreSlim(1, 1)`, and line 149 confirms it is still awaited with no timeout argument and no `CancellationToken` overload. Both were re-verified against the merged tree.

## (ii) Measured counter triples

| Regime | Exact command | Load condition | acquisitions | releases | contended | Balance test |
|---|---|---|---|---|---|---|
| SERIAL | `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/Logger:trx;LogFileName=p1-t9-ac1-serial.trx" /ResultsDirectory:coverage\trx\p1-t9 "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~Transaction_SecondCallerCannotInstallUntilTheFirstRestores"` | otherwise-idle machine, no induced load, Outlook closed, machine build lock held so no sibling item's build or test run overlapped; R4 excluded by filter (live-holder contention by design) | 11 | 10 | 0 | Passed (11 - 10 = 1) |
| PARALLEL | `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:TaskMaster.runsettings /InIsolation "/Logger:trx;LogFileName=p1-t10-ac1-parallel.trx" /ResultsDirectory:coverage\trx\p1-t10 "/TestCaseFilter:TestCategory!=LiveOutlook"` | same machine state; Workers 0 / Scope ClassLevel so distinct test classes ran concurrently; R4 included | 19 | 18 | 14 | Passed (19 - 18 = 1) |

Source artifacts: `evidence/baseline/ac1-serial-measurement.2026-09-12T17-00.md` (P1-T9) and `evidence/baseline/ac1-parallel-measurement.2026-09-12T17-00.md` (P1-T10). Both runs were taken from the same instrumented assembly built in P1-T8, on 2026-09-13 between 02:47 and 02:50 local time.

## (iii) Verdict by the pre-declared decision rule — SUPERSEDED BY CORRECTION 2

The whole of section (iii) as originally written is retained verbatim below for the audit trail and is WITHDRAWN as a verdict. Its conclusion "H-LEAK REJECTED by direct observation" is the claim Correction 2 withdraws. Read it as a record of what was claimed on 2026-09-13, not as a finding. Two specific sentences below do not survive: "A serial run cannot queue a second live holder; therefore a contended count of zero in that regime is a direct observation that the permit was never found held by a leaked transaction" is unsound because with `timeout=0` no transaction could have leaked under either hypothesis, so the zero was predetermined; and "The two hypotheses are not both supported; only H-COST survives" overstates a result that discriminated neither.

A further structural point, from the code review (finding N-3), explains why the decision rule could not have discriminated as written: under H-LEAK the serial-regime signature is the balance test BLOCKING on `WaitAsync` and expiring under its own `[Timeout]` with no `GATECOUNTERS` line printed at all. Rows 2 and 3 of the rule are therefore not observable as printed counter values. Row 1 is the only row that can ever appear as a printed triple, so selecting row 1 is not evidence that rows 2 and 3 were ruled out.

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
