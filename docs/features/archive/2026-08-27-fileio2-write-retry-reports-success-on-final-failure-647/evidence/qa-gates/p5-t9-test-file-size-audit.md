# P5-T9 — Post-Format Line-Count Audit of the Two Changed Test Files

Timestamp: 2026-08-31T20-13
Command: Get-Content -LiteralPath <path> and read the returned array's Count property, once per path, after the P5-T8 format
EXIT_CODE: 0

## Counts, against the 500-line limit

| Path | Pre-change (P0-T8) | Post-format | Limit | Within |
|---|---|---|---|---|
| `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` | 116 | 335 | 500 | Yes |
| `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` | 453 | 454 | 500 | Yes |

Both recorded counts are at most 500.

`UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` grew by 219 lines. That is the net of six new seam-driven test methods plus the private `ThrowingOnWriteTextWriter` fake, less the 19-line locked-fixture test that P5-T7 deleted. It has 165 lines of headroom.

`QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` grew by 1 line, the `return true;` statement that P4-T6 added to the async test double. Its five other doubles changed expression in place and its seam comment was reworded within its existing five lines. It has 46 lines of headroom.

Output Summary: Both changed test files are within the 500-line limit after formatting.
