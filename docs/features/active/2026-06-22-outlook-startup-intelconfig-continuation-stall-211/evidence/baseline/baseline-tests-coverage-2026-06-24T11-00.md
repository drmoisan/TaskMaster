# Phase 0 — MSTest + Coverage Baseline (issue #211, Phase 3.3)

Timestamp: 2026-06-24T11-00

Command: `vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /InIsolation /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook"`

EXIT_CODE: 0

Output Summary:
- Test Run Successful. Total tests: 134; Passed: 134; Failed: 0. Total time ~5.06 s.
- `/InIsolation` is required for this Moq-using assembly (otherwise STTE 4.2.0.1 Setup FileNotFound).
- Coverage file converted to Cobertura via `dotnet-coverage merge ... -f cobertura`.
- `TaskMaster.StartupDiagnosticsProbe` class line-rate: 1.0 (100%) at baseline — all existing
  EmitHeartbeat / EmitGcDelta members are fully covered by the existing tests.
- Single-assembly coverage run instruments all modules referenced by TaskMaster.Test (including
  vendored Deedle / Apache.Arrow), so the raw cobertura `coverage/@line-rate` is 0.1162
  (9,473 of 81,548 lines). This raw figure is NOT the repository-wide first-party floor metric; the
  policy 80% floor is measured against the first-party production denominator per the established
  coverage-denominator method, not against this single-assembly multi-module instrumentation total.
  The relevant new-code baseline is the StartupDiagnosticsProbe class at 100%.

Numeric baseline values (recorded, no UNVERIFIED placeholder):
- StartupDiagnosticsProbe class line-rate: 100.0%
- Raw single-assembly cobertura line-rate: 11.62% (9473/81548) — instrumentation artifact, not the
  first-party floor.
