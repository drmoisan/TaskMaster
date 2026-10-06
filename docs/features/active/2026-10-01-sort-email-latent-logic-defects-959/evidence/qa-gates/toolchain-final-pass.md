# Toolchain Final Pass (P7-T14)

Timestamp: 2026-10-06T17-22
ITERATION: 2
SUPERSEDES: a7ccc8d7dd40d5824ee4df825652ba911f9aaaa4
WRITTEN-BY: P7-T14
LOOP-RESTARTS: 0
Command: (1) dotnet tool run csharpier format . then dotnet tool run csharpier check .; (2) msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; (3) msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true; (4) the scoped vstest runs and the DIRECT coverage route of Invoke-MSTestWithCoverage.ps1 (dotnet-coverage collect around vstest.console.exe). EXIT_CODE is scoped to the P7-T11 collect invocation.
EXIT_CODE: 0 (the P7-T11 COLLECT_EXIT_CODE; branch (a), so no ExpectedExitCode is declared)
Output Summary: one clean Phase 7 pass on the first iteration: format changed no Write Set file, the check passed, both rebuilds ran CoreCompile on all four projects with 0 errors, the scoped suites passed 56/56, 3/3 and 11/11, and the full coverage run passed 7394/7394 with both floors met. This pass is the first analyzer rebuild, nullable rebuild and test run after the P6-T13 documentation-comment edit of TST1 (policy-audit G-4) and covers the P7-T2 and P7-T3 edits.

| Step | Canonical command | Artifact | Exit code | Additional figures |
| --- | --- | --- | --- | --- |
| P7-T6 format | dotnet tool run csharpier format . | docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p7-t6-csharpier-format.2026-10-06T17-11.md | 0 | WRITESET-CHANGED-COUNT: 0; PORCELAIN-SAME: True |
| P7-T7 check | dotnet tool run csharpier check . | docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p7-t7-csharpier-check.2026-10-06T17-12.md | 0 | Checked 1639 files |
| P7-T8 analyzer rebuild | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true | docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p7-t8-msbuild-analyzers.2026-10-06T17-13.md | 0 | SKIP_CORECOMPILE_LINES: 0; UCS_TEST_CSC_OUT_LINES: 2; UCS_CSC_OUT_LINES: 2; QF_CSC_OUT_LINES: 2; QFT_CSC_OUT_LINES: 2 |
| P7-T9 nullable rebuild | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true | docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p7-t9-msbuild-nullable.2026-10-06T17-14.md | 0 | SKIP_CORECOMPILE_LINES: 0; UCS_TEST_CSC_OUT_LINES: 2; UCS_CSC_OUT_LINES: 2; QF_CSC_OUT_LINES: 2; QFT_CSC_OUT_LINES: 2 |
| P7-T10 scoped tests | vstest.console.exe UtilitiesCS.Test.dll with FILTER-SORTEMAIL; QuickFiler.Test.dll with FILTER-EFC-CLEANUP and FILTER-EFC-ARCHIVE | docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/pass-after-regression-tests.md | 0, 0 and 0 | COUNTERS total=56 passed=56; total=3 passed=3; total=11 passed=11 |
| P7-T11 tests with coverage | dotnet-coverage collect ... -- vstest.console.exe <9 test assemblies> (CLAUDE.md step 4, DIRECT route) | docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/coverage-post-change.md | 0 | 7394 of 7394 passed; first-party lines 85.39%, branches 79.81%; LINE-FLOOR and BRANCH-FLOOR MET |

## Acceptance (P7-T14, all four required)

1. Every row of the final iteration reads exit code 0 (P7-T11 reads 0): met.
2. Both rebuild rows read SKIP_CORECOMPILE_LINES: 0 with the four _CSC_OUT_LINES values at least 1: met.
3. The P7-T6 row reads WRITESET-CHANGED-COUNT: 0: met.
4. EXIT_CODE 0 equals its declared expectation (default 0, ExpectedExitCode omitted): met.
