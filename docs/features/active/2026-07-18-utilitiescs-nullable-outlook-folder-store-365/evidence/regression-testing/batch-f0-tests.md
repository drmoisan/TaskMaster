# Batch F0 Tests With Coverage (P1-T4)

Timestamp: 2026-07-19T11-24

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot UtilitiesCS.Test -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-outlook-folder-store-365/evidence/regression-testing/batch-f0-coverage.cobertura.xml`

EXIT_CODE: 0

Output Summary:
- Test Run Successful. Total tests: **4511**, Passed: **4511**, Failed: **0**. No regression (AC3).
- Line coverage: **65.30%** (line-rate 0.652952); Branch coverage: **61.32%** (branch-rate 0.613196).
  Identical to the P0-T6 baseline, as expected for annotation-only interface/DTO changes.

Flakiness note: the first invocation aborted at 4145/4511 with dotnet-coverage exit -1 and produced no XML
(known full-suite coverage-run nondeterminism under parallel instrumentation). A deterministic re-run
passed 4511/4511 with the XML produced. The abort is pre-existing infrastructure flakiness unrelated to the
F0 annotation changes (interface/DTO annotations do not alter runtime behavior).
