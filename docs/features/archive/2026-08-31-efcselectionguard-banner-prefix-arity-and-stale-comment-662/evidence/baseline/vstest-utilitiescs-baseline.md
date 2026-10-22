# UtilitiesCS.Test Full-Assembly Baseline (P0-T12)

Timestamp: 2026-09-01T15-45

Command: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:TaskMaster.runsettings /EnableCodeCoverage /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=utilitiescs-baseline.trx" /ResultsDirectory:docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662/evidence/baseline/p0-t12`

EXIT_CODE: 0

Output Summary:

`<Counters ... />` line read from the produced TRX:

```
<Counters total="4783" executed="4783" passed="4783" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
```

AC8 baseline figures for `UtilitiesCS.Test`:

- total: 4783
- executed: 4783
- passed: 4783
- failed: 0

Console summary, transcribed:

```
Test Run Successful.
Total tests: 4783
     Passed: 4783
 Total time: 27.6355 Seconds
```

The baseline `failed` value is 0, so the AC8 gate is not pre-blocked by
pre-existing failures in this assembly. The intermittently-flaky
`PhysicalFileInfoAdapter` test known to this assembly passed on this run.

The runsettings used is the repository-root `TaskMaster.runsettings`, not the
`scripts\vscode` CLI variant, for the collector-exclusion reason recorded in the
plan. `/InIsolation` was supplied, per Decisions Record D10.
