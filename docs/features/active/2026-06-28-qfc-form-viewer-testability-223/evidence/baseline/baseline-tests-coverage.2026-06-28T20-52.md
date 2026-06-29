# Baseline — Tests + Code Coverage (QuickFiler.Test) (Issue #223)

Timestamp: 2026-06-28T20-52
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation
EXIT_CODE: 0

Output Summary:
- Total tests: 181. Passed: 181. Failed: 0. (This passing count is the baseline to preserve across all later phases.)
- QfcFormController aggregate line coverage (production type, all partial/async-state-machine classes, deduped by source line): 301 / 767 = 39.24%. This is the changed-type baseline for the no-regression comparison (AC5).
- QfcFormKeyHandler: does not exist yet (new in Phase 2); baseline coverage N/A.
- Process-wide line coverage for the QuickFiler.Test run: 12.52% (lines-covered 9524 / lines-valid 76066). NOTE: this command instruments ALL loaded modules (vendored + third-party) and runs only the QuickFiler.Test assembly, so this process-wide figure is not the repo-wide first-party >= 80% gate; it is recorded as a consistent apples-to-apples reference for the final-phase delta (same single-assembly command). The repo-wide >= 80% first-party policy gate is unaffected by this structural refactor.

Coverage conversion: dotnet-coverage merge -f cobertura on the emitted .coverage attachment.
