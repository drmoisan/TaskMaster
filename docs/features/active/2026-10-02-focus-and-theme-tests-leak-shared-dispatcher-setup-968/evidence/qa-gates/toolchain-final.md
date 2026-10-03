# Final toolchain pass (issue #968, task P8-T7)

Timestamp: 2026-10-03T03-31
ITERATION: 1

| Step | Exact command | EXIT_CODE | Key observations | ITERATION | Artifact |
|---|---|---|---|---|---|
| 1. csharpier format | `dotnet tool run csharpier format .` | 0 | REWRITTEN-WRITESET: NONE; REWRITTEN-OTHER: NONE | 1 | FEATURE/evidence/qa-gates/csharpier-format-final.md |
| 2. csharpier check | `dotnet tool run csharpier check .` | 0 | `Checked 1640 files in 4966ms.` | 1 | FEATURE/evidence/qa-gates/csharpier-check-final.md |
| 3. analyzer rebuild | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 | ERRORS 0; WARNINGS 0; SKIP_CORECOMPILE_LINES: 0 | 1 | FEATURE/evidence/qa-gates/msbuild-analyzer-final.md |
| 4. TreatWarningsAsErrors rebuild | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0 | ERRORS 0; SKIP_CORECOMPILE_LINES: 0 | 1 | FEATURE/evidence/qa-gates/msbuild-nullable-final.md |
| 5. coverage run (route DIRECT) | `dotnet-coverage collect --output coverage\final-968.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-968.config -- vstest.console.exe <discovered test assemblies> ... "/TestCaseFilter:TestCategory!=LiveOutlook&<four shell-icon class exclusions>"` then CMD-COVERAGE-POST | 0 | COVERAGE-ROUTE: DIRECT; RUNNER-GREEN: NO (COVERAGE-ROUTE DIRECT); Total 7365 passed 7365 failed 0; First-party coverage: lines 56211/65855 (85.36%), branches 13620/17078 (79.75%) | 1 | FEATURE/evidence/qa-gates/coverage-summary.md |

SINGLE-PASS: YES (all five rows come from ITERATION 1 with no restart after P8-T1)

AC22-STATUS: NOT MET (ENVIRONMENTAL: COVERAGE-ROUTE DIRECT). Every step passed in one uninterrupted pass, but the coverage step ran by the DIRECT route that P0-T16 selected (STALL-PROBE: REPRODUCES, one fast shell-icon test failure on this host), not the runner `scripts/vscode/Invoke-MSTestWithCoverage.ps1` verbatim, so RUNNER-GREEN is NO and AC22 cannot be met as worded. The decision belongs to the orchestrator (D-6).
