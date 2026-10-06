# Toolchain Final Pass (P8-T11)

Timestamp: 2026-10-06T18-07
ITERATION: 3
SUPERSEDES: 039825cd2385f5f30dd56b32252b5bbd0f71b9c9
WRITTEN-BY: P8-T11
LOOP-RESTARTS: 0
MERGE-HEAD-SHA: 9163994569e24c5c539a285724f9c8f9f6fd8a0e
ORIGIN-MAIN-SHA: f8ea1b5dcc6514bc0088bc80965c188bfd717557
Command: (1) dotnet tool run csharpier format . then dotnet tool run csharpier check .; (2) msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; (3) msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true; (4) the scoped vstest runs and the DIRECT coverage route of Invoke-MSTestWithCoverage.ps1 (dotnet-coverage collect around vstest.console.exe). EXIT_CODE is scoped to the P8-T9 collect invocation.
EXIT_CODE: 0 (the P8-T9 COLLECT_EXIT_CODE; branch (a), so no ExpectedExitCode is declared)
Output Summary: one clean Phase 8 pass on the first iteration over the merged tree: format changed no Write Set file and no other file, the check passed over 1645 files, both rebuilds ran CoreCompile on all four projects with 0 warnings and 0 errors, the scoped suites passed 56/56, 3/3 and 11/11, and the full coverage run passed 7404/7404 with both floors met (first-party lines 85.40%, branches 79.83%). This pass ran on the merged tree whose merge base with origin/main is f8ea1b5dcc6514bc0088bc80965c188bfd717557 (ORIGIN-MAIN-SHA). This rewrite supersedes the ITERATION 2 content written by P7-T14, which remains in git history.

| Step | Canonical command | Artifact | Exit code | Additional figures |
| --- | --- | --- | --- | --- |
| P8-T4 format | dotnet tool run csharpier format . | docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p8-t4-csharpier-format.2026-10-06T17-56.md | 0 | WRITESET-CHANGED-COUNT: 0; PORCELAIN-SAME: True |
| P8-T5 check | dotnet tool run csharpier check . | docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p8-t5-csharpier-check.2026-10-06T17-57.md | 0 | Checked 1645 files |
| P8-T6 analyzer rebuild | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true | docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p8-t6-msbuild-analyzers.2026-10-06T17-58.md | 0 | SKIP_CORECOMPILE_LINES: 0; UCS_TEST_CSC_OUT_LINES: 2; UCS_CSC_OUT_LINES: 2; QF_CSC_OUT_LINES: 2; QFT_CSC_OUT_LINES: 2 |
| P8-T7 nullable rebuild | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true | docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p8-t7-msbuild-nullable.2026-10-06T17-59.md | 0 | SKIP_CORECOMPILE_LINES: 0; UCS_TEST_CSC_OUT_LINES: 2; UCS_CSC_OUT_LINES: 2; QF_CSC_OUT_LINES: 2; QFT_CSC_OUT_LINES: 2 |
| P8-T8 scoped tests | vstest.console.exe UtilitiesCS.Test.dll with FILTER-SORTEMAIL; QuickFiler.Test.dll with FILTER-EFC-CLEANUP and FILTER-EFC-ARCHIVE | docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/pass-after-regression-tests.md | 0, 0 and 0 | COUNTERS total=56 passed=56; total=3 passed=3; total=11 passed=11 |
| P8-T9 tests with coverage | dotnet-coverage collect ... -- vstest.console.exe <9 test assemblies> (CLAUDE.md step 4, DIRECT route) | docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/coverage-post-change.md | 0 | 7404 of 7404 passed; first-party lines 85.40%, branches 79.83%; LINE-FLOOR and BRANCH-FLOOR MET |

## Acceptance (P8-T11, all four required)

1. Every row reads exit code 0 (P8-T9 reads 0): met.
2. Both rebuild rows read SKIP_CORECOMPILE_LINES: 0 with the four _CSC_OUT_LINES values at least 1: met.
3. The P8-T4 row reads WRITESET-CHANGED-COUNT: 0: met.
4. EXIT_CODE 0 equals its declared expectation (default 0, ExpectedExitCode omitted): met.
