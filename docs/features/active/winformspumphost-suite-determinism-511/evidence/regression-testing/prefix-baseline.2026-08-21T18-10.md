# Phase 1 — Consolidated Pre-Fix Baseline (P1-T5)

Timestamp: 2026-08-22T10-26

This artifact consolidates P1-T3 (ten class-filtered runs) and P1-T4 (ten full nine-assembly suite
runs) into a single twenty-row pre-fix table. Every cell was read from that run's own TRX file. No
value in this artifact is predicted; every value was observed.

`IsHandleCreated` is derived from the probe
`BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread`, which asserts that
`harness.Viewer.IsHandleCreated` is `true` and that `harness.Viewer.InvokeRequired`, evaluated on the
pump thread, is `false`. A passing probe therefore establishes `IsHandleCreated: true` for that run.

Column abbreviations: **Bool** = `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState`;
**NineArg** = `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates`;
**Probe** = `BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread`.

| # | Run index | Scope | Bool | NineArg | Probe | `IsHandleCreated` |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 1 | class-filtered | Passed | Passed | Passed | true |
| 2 | 2 | class-filtered | Passed | Passed | Passed | true |
| 3 | 3 | class-filtered | Passed | Passed | Passed | true |
| 4 | 4 | class-filtered | Passed | Passed | Passed | true |
| 5 | 5 | class-filtered | Passed | Passed | Passed | true |
| 6 | 6 | class-filtered | Passed | Passed | Passed | true |
| 7 | 7 | class-filtered | Passed | Passed | Passed | true |
| 8 | 8 | class-filtered | Passed | Passed | Passed | true |
| 9 | 9 | class-filtered | Passed | Passed | Passed | true |
| 10 | 10 | class-filtered | Passed | Passed | Passed | true |
| 11 | 1 | full suite | Passed | Passed | Passed | true |
| 12 | 2 | full suite | Passed | Passed | Passed | true |
| 13 | 3 | full suite | Passed | Passed | Passed | true |
| 14 | 4 | full suite | Passed | Passed | Passed | true |
| 15 | 5 | full suite | Passed | Passed | Passed | true |
| 16 | 6 | full suite | Passed | Passed | Passed | true |
| 17 | 7 | full suite | Passed | Passed | Passed | true |
| 18 | 8 | full suite | Passed | Passed | Passed | true |
| 19 | 9 | full suite | Passed | Passed | Passed | true |
| 20 | 10 | full suite | Passed | Passed | Passed | true |

Row count: **20** (ten class-filtered, ten full suite). No cell is empty.

## Observed failure rate, stated as a fraction of the runs actually executed

Twenty runs were executed and twenty runs were recorded. The rates below are measurements over those
twenty runs and nothing else.

| Test | Failures / runs executed | Rate |
| --- | --- | --- |
| `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` | 0 / 20 | **0%** |
| `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` | 0 / 20 | **0%** |
| `BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` | 0 / 20 | **0%** |
| `IsHandleCreated` observed `true` | 20 / 20 | **100%** |

Split by scope: 0 / 10 in the class-filtered scope, 0 / 10 in the full-suite scope.

## Explicit statement of what was and was not measured

The measured pre-fix failure rate across these twenty runs is **zero**. This plan deliberately made no
prediction about the rate before measuring it, and this artifact claims no rate that was not observed
in these twenty runs.

Two facts constrain how far that zero generalizes, and both are recorded so no reader over-reads the
table:

1. **A pre-fix failure of both named tests was observed in this same execution, outside these twenty
   runs.** The second invocation of `scripts/vscode/Invoke-MSTestWithCoverage.ps1` during P0-T16
   reported `Total tests: 6437, Passed: 6430, Failed: 7, Test Run Failed.`, with all seven failures
   being 60,000 ms `PumpTimeoutMs` expiries, and both named tests among them. That run is recorded in
   `evidence/baseline/coverage.2026-08-21T18-10.md`. It is **not** counted in the twenty rows above,
   because it is neither a P1-T3 class-filtered run nor a P1-T4 full-suite run and was executed under
   a different harness (`dotnet-coverage` instrumentation rather than plain
   `vstest.console.exe /EnableCodeCoverage`). Counting it would misreport the denominator; omitting it
   from the analysis entirely would misreport the phenomenon. It is therefore excluded from the table
   and carried into P1-T6.

2. **The zero rate is a property of these twenty runs on this machine at this load level.** The one
   observed failing run differed from its immediately preceding and following passing runs only in
   machine load: 17 idle MSBuild node-reuse processes were resident during it and were stopped before
   the next invocation, which then passed. The twenty runs above were all executed after those
   processes were cleared. No stray `testhost`, `vstest.console`, or `dotnet-coverage` process
   belonging to another agent was present at any point during this execution.

The plan's instruction is followed exactly: the green pre-fix runs are recorded as data about the race
window, not as evidence the defect is absent, and the chosen remedy is not narrowed, widened, or
abandoned on the basis of this result. Disposition of the mechanism question is recorded separately in
`intermittency-question.2026-08-21T18-10.md` (P1-T6).

## Provenance

| Source | Path |
| --- | --- |
| Class-filtered per-run detail | `evidence/regression-testing/prefix-classfiltered.2026-08-21T18-10.md` |
| Class-filtered TRX files (10) | `evidence/regression-testing/p1-t3/` |
| Class-filtered machine-readable rows | `coverage\p1-t3-rows.json` |
| Full-suite per-run detail | `evidence/regression-testing/prefix-fullsuite.2026-08-21T18-10.md` |
| Full-suite TRX files (10) | `evidence/regression-testing/p1-t4/` |
| Full-suite machine-readable rows | `coverage\p1-t4-rows.json` |
| The out-of-table failing run | `evidence/baseline/coverage.2026-08-21T18-10.md` |
