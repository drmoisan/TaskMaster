# QuickFiler.Test Full-Assembly Baseline (P0-T11)

Timestamp: 2026-09-01T15-44

Command: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:TaskMaster.runsettings /EnableCodeCoverage /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=quickfiler-baseline.trx" /ResultsDirectory:docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662/evidence/baseline/p0-t11`

EXIT_CODE: 0

Output Summary:

`<Counters ... />` line read from the produced TRX with `Select-String`:

```
<Counters total="1286" executed="1286" passed="1286" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
```

AC8 baseline figures for `QuickFiler.Test`:

- total: 1286
- executed: 1286
- passed: 1286
- failed: 0

Console summary, transcribed:

```
Test Run Successful.
Total tests: 1286
     Passed: 1286
 Total time: 14.4751 Seconds
```

The baseline `failed` value is 0, so the AC8 gate is not pre-blocked by
pre-existing failures in this assembly.

The runsettings used is the repository-root `TaskMaster.runsettings`, not the
`scripts\vscode` CLI variant, because `/EnableCodeCoverage` activates the Code
Coverage collector and only the repository-root file supplies that collector's
`Deedle` and `FSharp.Core` module exclusions. `/InIsolation` was supplied, per
Decisions Record D10.
