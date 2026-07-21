# Red — PercentageFormatterTests (P1-T2) [expect-fail]

Timestamp: 2026-07-16T09-35
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:FullyQualifiedName~PercentageFormatterTests
EXIT_CODE: 1 (expected failure — red phase)

Output Summary: All 6 PercentageFormatterTests FAIL against the unimplemented `PercentageFormatter.Format` (stub throws NotImplementedException). This is the expected red-phase outcome before P1-T3 implements the formatter.
Total tests: 6 | Failed: 6 | Passed: 0.
Failing tests:
- Format_TypicalFraction_RoundsToNearestWholePercent (0.4267 -> "43%")
- Format_One_RendersHundredPercent (1.0 -> "100%")
- Format_Zero_RendersZeroPercent (0.0 -> "0%")
- Format_Midpoint_RoundsAwayFromZero (0.125 -> "13%", proves AwayFromZero)
- Format_InputAboveOne_ClampsToHundredPercent (1.5 -> "100%")
- Format_NegativeInput_ClampsToZeroPercent (-0.3 -> "0%")
