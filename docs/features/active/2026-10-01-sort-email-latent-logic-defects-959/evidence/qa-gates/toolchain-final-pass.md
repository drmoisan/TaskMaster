# Toolchain Final Pass (P6-T11)

Timestamp: 2026-10-03T12-54
ITERATION: 1
LOOP-RESTARTS: 0
Command: (1) dotnet tool run csharpier format . then dotnet tool run csharpier check .; (2) msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; (3) msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true; (4) the scoped vstest runs and the DIRECT coverage route of Invoke-MSTestWithCoverage.ps1 (dotnet-coverage collect around vstest.console.exe). EXIT_CODE is scoped to the P6-T7 collect invocation.
EXIT_CODE: 0 (the P6-T7 COLLECT_EXIT_CODE; branch (a), so no ExpectedExitCode is declared)
Output Summary: one clean pass on the first iteration: format changed no Write Set file, the check passed, both rebuilds ran CoreCompile on all four projects with 0 errors, the scoped suites passed 55/55, 3/3 and 11/11, and the full coverage run passed 7393/7393 with both floors met.

| Step | Canonical command | Artifact | Exit code | Additional figures |
| --- | --- | --- | --- | --- |
| P6-T1 format | dotnet tool run csharpier format . | docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t1-csharpier-format.2026-10-03T12-34.md | 0 | WRITESET-CHANGED-COUNT: 0; PORCELAIN-SAME: True |
| P6-T2 check | dotnet tool run csharpier check . | docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t2-csharpier-check.2026-10-03T12-35.md | 0 | Checked 1639 files |
| P6-T3 analyzer rebuild | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true | docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t3-msbuild-analyzers.2026-10-03T12-36.md | 0 | SKIP_CORECOMPILE_LINES: 0; UCS_TEST_CSC_OUT_LINES: 2; UCS_CSC_OUT_LINES: 2; QF_CSC_OUT_LINES: 2; QFT_CSC_OUT_LINES: 2 |
| P6-T4 nullable rebuild | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true | docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/p6-t4-msbuild-nullable.2026-10-03T12-37.md | 0 | SKIP_CORECOMPILE_LINES: 0; UCS_TEST_CSC_OUT_LINES: 2; UCS_CSC_OUT_LINES: 2; QF_CSC_OUT_LINES: 2; QFT_CSC_OUT_LINES: 2 |
| P6-T5 scoped tests (SortEmail family) | vstest.console.exe UtilitiesCS.Test.dll with FILTER-SORTEMAIL | docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/pass-after-regression-tests.md | 0 | COUNTERS total=55 executed=55 passed=55 failed=0 |
| P6-T6 scoped tests (QuickFiler.Test) | vstest.console.exe QuickFiler.Test.dll with FILTER-EFC-CLEANUP and FILTER-EFC-ARCHIVE | docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/regression-testing/pass-after-regression-tests.md (section QuickFiler.Test (P6-T6)) | 0 and 0 | COUNTERS total=3 passed=3; COUNTERS total=11 passed=11 |
| P6-T7 tests with coverage | dotnet-coverage collect ... -- vstest.console.exe <9 test assemblies> (CLAUDE.md step 4, DIRECT route) | docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/qa-gates/coverage-post-change.md | 0 | 7393 of 7393 passed; first-party lines 85.39%, branches 79.81%; LINE-FLOOR and BRANCH-FLOOR MET |

## Acceptance (P6-T11, all four required)

1. Every row of the final iteration reads exit code 0 (P6-T7 reads 0): met.
2. Both rebuild rows read SKIP_CORECOMPILE_LINES: 0 with the four _CSC_OUT_LINES values at least 1: met.
3. The P6-T1 row reads WRITESET-CHANGED-COUNT: 0: met.
4. EXIT_CODE 0 equals its declared expectation (default 0, ExpectedExitCode omitted): met.
