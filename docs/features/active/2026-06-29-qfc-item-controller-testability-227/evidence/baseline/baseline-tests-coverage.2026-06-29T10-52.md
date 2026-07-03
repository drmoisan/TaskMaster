# Baseline — Tests + Code Coverage (P0-T5)

Timestamp: 2026-06-29T10-52
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation
EXIT_CODE: 0

Coverage conversion command: dotnet-coverage merge -o scratch-baseline.cobertura.xml -f cobertura <TestResults .coverage>

Output Summary:
- Total tests: 201; Passed: 201; Failed: 0. This passing count (201) is the baseline to preserve across all later phases for the QuickFiler.Test assembly.
- QfcItemController.cs production line coverage (Cobertura sequence-point basis): 246 / 3261 = 7.54%. (The research §6.2 figure of 74/1288 = ~5.7% uses a different physical-line counting basis; the Cobertura number is the authoritative measured baseline for delta computation in this plan.)
- Whole-process line-rate for this single-assembly run (includes all vendored/third-party modules loaded by QuickFiler.Test): lines-covered 10003 / lines-valid 76355 = 13.10%. This is NOT the first-party testable denominator; the authoritative repo-wide first-party testable-denominator figure is the #223-measured 73.35%-74.11% (2026-06-28T21-50), accepted below the 80% floor under the authority-scoped exception in maintainer-decision.2026-06-29.md and tracked under #197.

Numeric headline: 201 passed; QfcItemController production coverage 7.54% (246/3261).
