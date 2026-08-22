# P4-T5 — The Two Regression Tests Across the Ten Runs

Timestamp: 2026-08-22T14-35

Command:
```powershell
Get-ChildItem docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/p4-t2/*.trx |
  Sort-Object Name |
  ForEach-Object {
      [xml]$x = Get-Content -Raw $_.FullName
      # map TestDefinitions/UnitTest id -> TestMethod name, then read Results/UnitTestResult outcome
  }
```

Each cell was read from that run's own TRX by resolving the result's `testId` against the run's
`TestDefinitions`.

EXIT_CODE: 0

Output Summary:

Column abbreviations: **Forces** = `BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread`;
**NoChildHandles** = `BuildPumpHarness_DoesNotCreateTheWebViewChildHandles`.

| # | TRX | Forces | NoChildHandles |
| --- | --- | --- | --- |
| 1 | `DanMoisan_MEGALODON4_2026-08-22_11_53_56_net481.trx` | passed | passed |
| 2 | `DanMoisan_MEGALODON4_2026-08-22_12_12_50_net481.trx` | passed | passed |
| 3 | `DanMoisan_MEGALODON4_2026-08-22_12_30_40_net481.trx` | passed | passed |
| 4 | `DanMoisan_MEGALODON4_2026-08-22_12_37_28_net481.trx` | passed | passed |
| 5 | `DanMoisan_MEGALODON4_2026-08-22_12_53_39_net481.trx` | passed | passed |
| 6 | `DanMoisan_MEGALODON4_2026-08-22_13_10_45_net481.trx` | passed | passed |
| 7 | `DanMoisan_MEGALODON4_2026-08-22_13_17_44_net481.trx` | passed | passed |
| 8 | `DanMoisan_MEGALODON4_2026-08-22_13_32_03_net481.trx` | passed | passed |
| 9 | `DanMoisan_MEGALODON4_2026-08-22_13_39_20_net481.trx` | passed | passed |
| 10 | `DanMoisan_MEGALODON4_2026-08-22_14_03_59_net481.trx` | passed | passed |

Acceptance: the table has exactly ten rows and every cell reads passed.

Both regression tests carry `[Timeout(PumpTimeoutMs)]` with `PumpTimeoutMs = 60000`, and neither was
recorded as a timeout in any of the ten runs, despite every run being between 6x and 26x slower than
the unloaded baseline.

`BuildPumpHarness_DoesNotCreateTheWebViewChildHandles` is the corrected form described in
`webview-child-handle-measurement.2026-08-21T18-10.md`; it asserts the measured handle state of both
named `Microsoft.Web.WebView2.WinForms.WebView2` children rather than the unmeasured prediction the
plan's P3-T1 body stated.
