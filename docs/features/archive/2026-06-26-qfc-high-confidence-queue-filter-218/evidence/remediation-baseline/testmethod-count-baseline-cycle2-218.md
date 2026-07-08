# Pre-Trim TestMethod Count Baseline — Cycle 2, Issue #218

Timestamp: 2026-06-28T17-31

Command: `Select-String -Path '...QfcHomeControllerTests.cs','...RunAsyncTests.cs','...IterationTests.cs','...MetricsTests.cs','...PropertyTests.cs','...Issue218Tests.cs' -Pattern '^\s*\[TestMethod\]'` (active) and `'^\s*//\s*\[TestMethod\]'` (commented).

EXIT_CODE: 0

| File | Active [TestMethod] | Commented [TestMethod] |
|------|---------------------|------------------------|
| QfcHomeControllerTests.cs | 30 | 3 |
| QfcHomeControllerRunAsyncTests.cs | 6 | 0 |
| QfcHomeControllerIterationTests.cs | 6 | 0 |
| QfcHomeControllerMetricsTests.cs | 2 | 0 |
| QfcHomeControllerPropertyTests.cs | 13 | 0 |
| QfcHomeControllerIssue218Tests.cs | 2 | 0 |

Anchors (all CONFIRMED):
- QfcHomeControllerTests.cs = 30 active (3 commented excluded). MATCH.
- Currently-compiled QfcHomeController suite active count = Tests 30 + Issue218 2 = 32 (the four split files are not yet wired into the test csproj, so not compiled). MATCH.
- The four split files contribute 6 + 6 + 2 + 13 = 27 (currently uncompiled). MATCH.
- Post-trim target: Tests 3 + RunAsync 6 + Iteration 6 + Metrics 2 + Property 13 + Issue218 2 = 32 compiled active. (27 moved + 3 residual = 30 in Tests pre-trim.)

Output Summary: All TestMethod-count anchors confirmed. 27 moved tests live in the four uncompiled split files; 30 active tests (27 moved + 3 residual) still in QfcHomeControllerTests.cs pre-trim. Currently-compiled suite = 32. End-state compiled active count must remain 32.
