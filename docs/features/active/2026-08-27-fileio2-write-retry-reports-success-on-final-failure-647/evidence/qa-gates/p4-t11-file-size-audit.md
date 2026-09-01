# P4-T11 — Post-Format File-Size Audit

Timestamp: 2026-08-31T20-03
Command: Get-Content -LiteralPath <path> and read the returned array's Count property, once per footprint path, after the P4-T7 format
EXIT_CODE: 0

## Counts, against the 500-line limit

| Path | Pre-change (P0-T8) | Post-format | Limit | Within |
|---|---|---|---|---|
| `UtilitiesCS/To Depricate/FileIO2.cs` | 232 | 293 | 500 | Yes |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | 215 | 227 | 500 | Yes |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | 467 | 494 | 500 | Yes |
| `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` | 116 | 203 | 500 | Yes |
| `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` | 453 | 454 | 500 | Yes |

Every one of the five recorded counts is at most 500.

## The constrained file

`TaskMaster/AppGlobals/AppOlObjects.cs` is the file the plan's risk register flags for headroom. It stood at 467 lines before the change, leaving 33 lines under the limit, and now stands at 494, an increase of 27 lines and 6 lines of remaining headroom. The increase decomposes as 23 lines for the block-bodied lambda that P4-T5 introduced, plus 4 lines for the `using Exception = System.Exception;` alias and its three-line explanatory comment that the P4-T8 CS0104 remediation required.

The margin is real but small. P6-T1 re-audits all five counts after the final repository-wide format, so this audit cannot go stale: if the closing format reflows anything in that file, the re-audit observes it.

Output Summary: All five footprint files are within the 500-line limit after formatting. The narrowest margin is 6 lines, on `TaskMaster/AppGlobals/AppOlObjects.cs`.
