# Green — FolderNodeViewModelTests (P2-T3)

Timestamp: 2026-07-16T09-55
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:FullyQualifiedName~FolderNodeViewModelTests
EXIT_CODE: 0

Output Summary: `Test Run Successful.` All 5 FolderNodeViewModelTests PASS. Total tests: 5 | Passed: 5 | Failed: 0.

Implementation:
- `Glyph`: returns null when `!HasChildren`; otherwise `'-'` when `Expanded`, `'+'` when collapsed (INV4 bijection).
- `FormattedPercentage`: `PercentageFormatter.Format(Probability.Value)` when `Probability.HasValue`, else `string.Empty`.
