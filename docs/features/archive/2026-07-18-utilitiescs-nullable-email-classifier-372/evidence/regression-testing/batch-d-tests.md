# Batch D — Test Run with Coverage

Timestamp: 2026-07-19T03-00

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-email-classifier-372/evidence/regression-testing/batch-d-coverage.cobertura.xml`

EXIT_CODE: 0

Output Summary:
- Total tests: 5702. Passed: 5702. Failed: 0. Test Run Successful.
- Line coverage: 83.79% (line-rate 0.83794) vs baseline 0.837795 — no regression.
- Branch coverage: 76.34% (branch-rate 0.763446) vs baseline 0.763329 — no regression.
- No test regression on the engine classes; changed-line coverage does not regress versus baseline (AC3, AC4).

Operational note: one run timed out mid-suite (single pipeline, hung after ~522 logged tests) — the known STA/24-worker-parallelism flakiness in the full suite. After a full process-clean and retry, all 5702 tests passed. The changes are annotation-only; Batches A–C also passed the same suite at ~37s.
