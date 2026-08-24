# P2-T6 — Probe Outcome, Pre-Fix vs Post-Fix

Timestamp: 2026-08-22T10-22

Command:

```
pwsh -NoProfile -Command 'msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"'

vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Logger:trx `
  /ResultsDirectory:docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/p2-t6 `
  /TestCaseFilter:"FullyQualifiedName~BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread"
```

`msbuild` was invoked through its absolute resolved path
`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`;
`vstest.console.exe` through
`C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.
Both went through `pwsh -NoProfile` from the worktree root.

EXIT_CODE: 0 (rebuild), 0 (test run)

Output Summary:

| Stage | Measure | Value |
| --- | --- | --- |
| Rebuild | Exit code | **0** |
| Rebuild | `error` occurrences in `coverage\p2-t6-build.log` | **0** |
| Rebuild | `Skipping target "CoreCompile"` occurrences | **0** |
| Test run | Total tests | 1 |
| Test run | Passed | **1** |
| Test run | Failed | **0** |
| Test run | Duration | 2.78 s |
| Test run | TRX | `evidence/regression-testing/p2-t6/2026-08-22_10_22_48_net481.trx` |

## Pre-fix and post-fix side by side

| Test | Pre-fix (P1-T5 table, 20 runs) | Post-fix (this task) |
| --- | --- | --- |
| `BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` | Passed 20 / 20; `IsHandleCreated: true` on every run; measured failure rate **0%** | Passed 1 / 1 |

## Mandatory flag: this probe was already green pre-fix

The plan requires that a probe already green on every pre-fix run be recorded as such and flagged,
because in that case it proves nothing about the fix.

**The probe was green on all twenty P1-T5 pre-fix runs.** It is therefore flagged here: the
post-fix pass recorded above is a consistency check, not the fail-before/pass-after proof for the
fixture change. This task is not evidence that the fix altered behaviour.

P1-T6's disposition governs. Its two recorded facts are the load-bearing ones:

1. A genuine pre-fix failure of both named end-to-end tests **was** observed in this execution,
   outside the twenty-row table: the second P0-T16 coverage invocation reported
   `Total tests: 6437, Passed: 6430, Failed: 7`, with all seven failures being 60,000 ms
   `PumpTimeoutMs` expiries and both named tests among them.
2. That failing run differed from the passing runs either side of it only in machine load —
   17 idle MSBuild node-reuse processes were resident during it. Clearing them restored 6437 / 6437.

The remedy is unchanged, per the plan's explicit instruction: forcing the handle removes the
dependency in the passing direction under either candidate explanation, so a green pre-fix run does
not narrow, widen, or abandon it.
