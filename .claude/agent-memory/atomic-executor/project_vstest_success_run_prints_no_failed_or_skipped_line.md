---
name: vstest-success-run-prints-no-failed-or-skipped-line
description: vstest.console.exe prints only "Total tests:" and "Passed:" on an all-green run (no "Failed:"/"Skipped:" line), and the TRX Counters element supplies only failed - notExecuted is hard-coded 0 and is NOT the skipped count
metadata:
  type: project
---

On a fully passing run, `vstest.console.exe` (VS18 / net481) prints exactly three summary lines:

```
Test Run Successful.
Total tests: 4783
     Passed: 4783
 Total time: 12.8452 Seconds
```

There is NO `Failed:` line and NO `Skipped:` line. Measured 2026-09-02 on #584 preflight round 5
across four separate green runs (4783, 1312, 41, and 2 tests), with and without
`/Settings:...runsettings`, and under `dotnet-coverage collect`. `grep -c "Failed:\|Skipped:"`
returned 0 on every one. Both lines DO appear when the run has failures.

**Why:** a plan acceptance of the form "record the numeric `Total tests`, `Passed`, `Failed`, and
`Skipped` counts printed by vstest ... as concrete numbers, not placeholders" names two values the
tool never emits on its success path, so the executor must either derive them or record a
placeholder and fail the task. This is the `atomic-plan-contract` mandatory rule "Observe a
command's success-case output before asserting over that output" — the same shape as demanding
separate line and branch percentages from a coverage run that prints one combined column.

It survived four earlier preflight rounds on #584 because every vstest invocation was broken by
MSYS `/InIsolation` mangling (see [[project_msys_slash_switch_conversion_rule]]) and ran zero
tests, so no round could observe a SUCCESSFUL summary block until the prefix fix landed. A defect
hidden behind another defect only becomes visible after the first is fixed — budget a round for it.

**How to apply:** when a plan asks for a Failed/Skipped count, point it at the TRX — but only
`Failed`. Every one of these commands already passes `/Logger:trx`, and the TRX carries the run
totals in one element:

```
<Counters total="3" executed="2" passed="1" failed="1" error="0" ... notExecuted="0" ... />
```

`failed` is the console's `Failed:`. **`notExecuted` is NOT the console's `Skipped:`.** The vstest
TrxLogger populates only `total`, `executed`, `passed`, and `failed`; every other counter attribute
(`notExecuted`, `error`, `timeout`, `aborted`, `inconclusive`, ...) is hard-coded to `0`. Measured
2026-09-02 on #584 preflight round 6 with a purpose-built 3-test probe assembly (1 pass, 1 fail,
1 `[Ignore]`): the console printed `Skipped: 1` while the same run's TRX reported `notExecuted="0"`
even though that test's own `<UnitTestResult ... outcome="NotExecuted">` was present.

Derive `Skipped` as `total` minus `executed` (3-2=1 on the red probe, 2-2=0 on a green run), or
count `outcome="NotExecuted"` results. Sourcing `Skipped` from `notExecuted` yields a constant `0` —
an acceptance value that cannot fail, which is the same class of defect as reading a count the
console never prints.

Note also: MSTest 4.3 renders a method-level `[Ignore]` in this repo's suites as a PASSING test
named `Disabled_<original>`, so the real UtilitiesCS.Test / QuickFiler.Test runs report zero skips
and the wrong derivation happens to agree there. Do not let that coincidence validate the mechanism.

Re-measured independently 2026-09-02 on #584 preflight round 7 with the same probe:
`total=3 executed=2 notExecuted=0` on the mixed run (console `Skipped: 1`), `total=1 executed=1` on
a green run, so `total - executed` gives 1 and 0. Also measured, on a run with a failure and NO
skip: console prints `Failed: 1` and no `Skipped:` line at all — the two aggregate lines really are
independent per-counter, so an `[expect-fail]` task whose run has a non-zero failure count CAN read
`Failed:` from the console.

Two operational facts from the same runs:
- The default TRX filename is `<account>_<machine>_<timestamp>_net<tfm>.trx`, and vstest also prints
  a `Results File: <absolute host path>\<that name>` console line on green AND red runs. Both
  disclose the account and machine name; redact both from committed evidence.
- The timestamp is per-second and vstest never overwrites, so re-running the same command into the
  same `/ResultsDirectory:` leaves TWO `.trx` files with distinct mtimes. A plan that reads "the TRX"
  from a task's own directory needs an explicit tie-break (most recently modified) once any task can
  be re-run; per-task directories bound collisions ACROSS tasks only, never across re-runs.
