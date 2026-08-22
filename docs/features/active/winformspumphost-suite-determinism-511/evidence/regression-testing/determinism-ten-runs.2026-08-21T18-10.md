# P4-T6 — Consolidated Determinism Record (Ten Runs Under Load)

Timestamp: 2026-08-22T14-40

## The ten TRX paths

All ten sit in the task-private subdirectory
`docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/p4-t2/`,
which holds exactly ten `.trx` files and no other file. (`vstest.console.exe /EnableCodeCoverage`
additionally creates one attachment directory per run alongside them; the same structure is present
in the Phase 1 `p1-t3` and `p1-t4` subdirectories.)

1. `docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/p4-t2/DanMoisan_MEGALODON4_2026-08-22_11_53_56_net481.trx`
2. `docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/p4-t2/DanMoisan_MEGALODON4_2026-08-22_12_12_50_net481.trx`
3. `docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/p4-t2/DanMoisan_MEGALODON4_2026-08-22_12_30_40_net481.trx`
4. `docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/p4-t2/DanMoisan_MEGALODON4_2026-08-22_12_37_28_net481.trx`
5. `docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/p4-t2/DanMoisan_MEGALODON4_2026-08-22_12_53_39_net481.trx`
6. `docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/p4-t2/DanMoisan_MEGALODON4_2026-08-22_13_10_45_net481.trx`
7. `docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/p4-t2/DanMoisan_MEGALODON4_2026-08-22_13_17_44_net481.trx`
8. `docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/p4-t2/DanMoisan_MEGALODON4_2026-08-22_13_32_03_net481.trx`
9. `docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/p4-t2/DanMoisan_MEGALODON4_2026-08-22_13_39_20_net481.trx`
10. `docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/p4-t2/DanMoisan_MEGALODON4_2026-08-22_14_03_59_net481.trx`

## Per-run record

Durations are the TRX `Times` finish-minus-start span. MSBuild node counts were sampled immediately
before and immediately after each `vstest.console.exe` invocation.

| # | Total | Passed | Failed | Not executed | Duration (s) | MSBuild nodes before / after |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 6439 | 6439 | 0 | 0 | 1153.8 | 0 / 0 |
| 2 | 6439 | 6439 | 0 | 0 | 1063.8 | 0 / 0 |
| 3 | 6439 | 6439 | 0 | 0 | 399.7 | 0 / 0 |
| 4 | 6439 | 6439 | 0 | 0 | 960.0 | 0 / 0 |
| 5 | 6439 | 6438 | **1** | 0 | 1021.7 | 0 / 0 |
| 6 | 6439 | 6439 | 0 | 0 | 414.4 | 0 / 0 |
| 7 | 6439 | 6439 | 0 | 0 | 849.5 | 0 / 0 |
| 8 | 6439 | 6439 | 0 | 0 | 430.2 | 0 / 0 |
| 9 | 6439 | 6439 | 0 | 0 | 1471.9 | 0 / 0 |
| 10 | 6439 | 6439 | 0 | 0 | 1273.2 | 0 / 0 |

Total is 6439 in every run: the 6437 of the P0-T15 baseline plus the two regression tests added by
P1-T1 and P3-T1.

## Sustained CPU utilization

| Point | Timestamp | Samples | Mean |
| --- | --- | --- | --- |
| P4-T1, before run 1 | 2026-08-22T11-53 | `100`, `100`, `100`, `100`, `100` | **100.00** |
| P4-T3, after run 10 | 2026-08-22T14-24 | `99.94`, `100`, `100`, `100`, `100` | **99.99** |

23 load jobs (`ProcessorCount - 1`, `ProcessorCount = 24`) ran for the whole window. Every run took
between 399.7 s and 1471.9 s against a measured unloaded baseline of 55.4 s to 70.0 s for the same
command in P1-T4, that is between 6x and 26x slower, which corroborates sustained contention across
the window rather than only at its ends.

## Pre-fix and post-fix, side by side, measured values only

| Test | Pre-fix (P1-T5, 20 runs, unloaded) | Post-fix (P4-T2, 10 runs, 100% CPU load) |
| --- | --- | --- |
| `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` | Passed 20 / 20 | Passed **10 / 10** |
| `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` | Passed 20 / 20 | Passed **10 / 10** |
| `BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` | Passed 20 / 20 | Passed **10 / 10** |
| `BuildPumpHarness_DoesNotCreateTheWebViewChildHandles` | not authored until P3-T1 | Passed **10 / 10** |
| Suite-wide failed count | 0 in each of the 20 runs | 0 in 9 of 10 runs; **1** in run 5 |

The pre-fix column is drawn from `prefix-baseline.2026-08-21T18-10.md`; the post-fix column from
`named-tests-ten-runs.2026-08-21T18-10.md` and `regression-tests-ten-runs.2026-08-21T18-10.md`.

### What the comparison does and does not establish

It does **not** establish a fail-before / pass-after transition for the two named tests, because the
pre-fix measurement recorded no failure in either of them across its twenty runs. That was recorded
at the time and is not restated here as anything stronger.

What it does establish is that both named tests, and both regression tests, held under sustained
100% CPU saturation for ten consecutive full-suite runs totalling roughly two and a half hours, at
6x to 26x the unloaded run duration, with `PumpTimeoutMs = 60000` unchanged and no sleep, retry, or
timing tolerance anywhere in the change.

One genuine pre-fix failure of both named tests was observed in this execution, outside the
twenty-run pre-fix table: the second P0-T16 coverage invocation reported 6430 / 6437 with seven
60,000 ms `PumpTimeoutMs` expiries including both named tests. Its distinguishing condition was
17 idle MSBuild node-reuse processes, which the ten runs above did not carry. That gap is closed
separately in `supplementary-msbuild-node-contention-ten-runs.2026-08-21T18-10.md`.

## Recorded failure — P4-T2's acceptance condition is not met

P4-T2 requires that each of the ten TRX files record a failed count of exactly 0. **Run 5 records
`failed="1"`, so P4-T2's acceptance condition is not met and the task is not checked off.**

The failing test is:

```
UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests.GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform
System.NullReferenceException: Object reference not set to an instance of an object.
duration 00:00:26.1779280
```

Four facts about it are recorded rather than argued:

1. It is in `UtilitiesCS.Test`, a different assembly from the one this change touches. Nothing in
   this change's diff reaches it: the diff is confined to three files under
   `QuickFiler.Test/Controllers/`.
2. It is not a `[Timeout]` expiry and is not a pump-harness test. It is a `NullReferenceException`
   in a Deedle data-frame test, unrelated to window handles, the WinForms pump, or the dispatcher
   gate.
3. It passed in the other nine runs of this window, in all twenty pre-fix runs of P1-T3 and P1-T4,
   and in the P0-T15 baseline.
4. All four tests this plan tracks passed in run 5.

The run was **not** repeated to obtain a tenth green result. The plan forbids doing so without
recording every attempt, and the honest record is that ten runs were executed and one of them
recorded one failure in an out-of-scope assembly.

The disposition of that failure — whether it is a pre-existing latent defect in
`DfDeedle_COM_Tests` that saturation exposes, and whether P4-T2's absolute-zero condition over the
whole nine-assembly suite is the right gate for a change scoped to three `QuickFiler.Test` files —
is escalated to the caller rather than decided here.
