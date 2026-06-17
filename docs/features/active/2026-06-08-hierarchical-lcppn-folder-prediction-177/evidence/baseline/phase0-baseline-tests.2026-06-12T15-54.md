# Phase 0 Baseline — Tests + Coverage (#177 Cycle 1)

- Timestamp: 2026-06-12T16-20 (UTC)
- Task: [P0-T6]
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation`
- EXIT_CODE: 0
- Output Summary:
  - Test Run Successful. Total tests: 3890, Passed: 3890, Failed: 0. Total time ~41.3s.
  - `/InIsolation` is required for this Moq-using assembly (per repo memory) to avoid a TestPlatform Setup FileNotFound failure.
  - Coverage .coverage merged to XML via `Microsoft.CodeCoverage.Console.exe merge -f xml`.
  - Baseline strict per-type line coverage (strict = partially-covered lines counted as NOT covered):
    - `FolderHierarchyTree`: strict 86.42% / inclusive 91.36% (covered=70, partial=4, not-covered=7, total=81)
    - `LcppnFolderPredictor`: strict 89.14% / inclusive 91.43% (covered=156, partial=4, not-covered=15, total=175)
  - These match the reviewer-reported baseline values (FolderHierarchyTree 86.4%, LcppnFolderPredictor 89.1%), confirming the aggregation methodology.
  - Repo-wide strict total (first-party scope, as reported by the Step-8 feature-review): 85.40%. (A whole-collection figure that includes the four vendored assemblies — Swordfish.NET.General, etc. — is 53.74% strict; the vendored projects are excluded from the analyzer/coverage gate scope per `.claude/rules/csharp.md`, so 85.40% first-party is the gate metric.)

Baseline coverage XML retained at:
`docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/baseline/coverage-p0/baseline.xml`
