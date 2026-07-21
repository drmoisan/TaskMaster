# Baseline Test Run with Coverage

Timestamp: 2026-07-19T01-05

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-email-parsing-370/evidence/baseline/baseline-coverage.cobertura.xml`

EXIT_CODE: 0

Output Summary:
- Total tests: 5702
- Passed: 5702
- Failed: 0
- Total time: 35.0405 seconds
- Baseline overall line-coverage: 83.7834%
- Baseline overall branch-coverage: 76.3407%
- Cobertura XML written to `docs/features/active/2026-07-18-utilitiescs-nullable-email-parsing-370/evidence/baseline/baseline-coverage.cobertura.xml`.

Note: this run required a one-time environment step on this fresh worktree: the repo-local .NET
SDK and NuGet restore were already established for P0-T4/P0-T5 (see those artifacts), but the
Invoke-MSTestWithCoverage.ps1 script requires pre-built `*.Test.dll` assemblies under
`bin\Debug\`; a plain `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`
(no special flags) was run first to produce all 7 test assemblies (`QuickFiler.Test`,
`Tags.Test`, `TaskMaster.Test`, `TaskTree.Test`, `TaskVisualization.Test`, `ToDoModel.Test`,
`UtilitiesCS.Test`, `VBFunctions.Test`).

The two duplicate-named test-file pairs flagged in the plan's Scope Invariants (e.g.
`EmailFiler_Tests.cs`, `EmailTokenizer(Tests|_Tests).cs`) did not cause a build or test-discovery
ambiguity at baseline: all 5702 tests discovered and ran, all passed. This baseline pass/fail
state (5702/5702) and coverage percentages (83.7834% line / 76.3407% branch) are the reference
point for per-batch and final regression comparisons.
