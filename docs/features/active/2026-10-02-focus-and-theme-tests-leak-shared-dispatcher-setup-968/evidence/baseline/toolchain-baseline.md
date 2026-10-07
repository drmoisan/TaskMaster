# Baseline toolchain index (issue #968, task P0-T18)

Timestamp: 2026-10-03T02-53

This file is an index over the per-step baseline artifacts, not a substitute for them.

| Step | Canonical command | EXIT_CODE | Artifact |
|---|---|---|---|
| 1. csharpier check | `dotnet tool run csharpier check .` | 0 | FEATURE/evidence/baseline/csharpier-check-baseline.md |
| 2. analyzer rebuild | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 | FEATURE/evidence/baseline/msbuild-analyzer-baseline.md |
| 3. TreatWarningsAsErrors rebuild | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0 | FEATURE/evidence/baseline/msbuild-nullable-baseline.md |
| 4. coverage run (route DIRECT) | `dotnet-coverage collect ... -- vstest.console.exe <discovered test assemblies> ... "/TestCaseFilter:TestCategory!=LiveOutlook&<four shell-icon class exclusions>"` (CMD-COVERAGE-DIRECT, then CMD-COVERAGE-POST) | 0 | FEATURE/evidence/baseline/coverage-summary.md |

BASELINE-STATE: GREEN
First-party coverage: lines 56206/65855 (85.35%), branches 13617/17078 (79.73%)
