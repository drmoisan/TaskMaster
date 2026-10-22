# Toolchain Final Pass (P3-T9)

Timestamp: 2026-09-30T15-12
Command: the Phase 3 loop P3-T1 through P3-T8 (format, scope re-check, line counts, read-only format check, analyzer rebuild, nullable rebuild, coordinator fixture, coverage-enabled test run), as recorded in the artifacts listed below
EXIT_CODE: 0
Output Summary: Pass number: 2. Pass 1 ran P3-T1 through P3-T7 at exit 0 and stopped at P3-T8 (exit 1) with a first-attempt failure set confined to QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests; the single pass-2 restart was admitted under the revision round 3 coordinator extension of the P3-T8 re-run rule. Pass 2 is the clean pass: every step P3-T1 through P3-T8 exited 0, with no file rewritten, the check reporting no differences, both rebuilds at SKIP_CORECOMPILE_LINES: 0 with both CSC_OUT_ counts at least 1, and the coverage run at exit 0 by COVERAGE-ROUTE: DIRECT (selected by STALL-PROBE: REPRODUCES in P0-T16) with 7327 of 7327 tests passed and both floors met.

## Pass 1 (first attempt)

| Task | Command | Exit code | Observation |
|---|---|---|---|
| P3-T1 | dotnet tool run csharpier format . | 0 | rewritten count 0; scoped porcelain before and after empty, identical line sets |
| P3-T2 | CMD-TOKEN-COUNT (TOKENS-PARTIAL, TOKENS-PROD), CMD-PRIME-SPANS, CMD-PHRASE-COUNT, P2-T6 added-lines payload, CMD-REGION-COMPARE, protected-file git diff --exit-code | 0 | every P1-T1, P2-T6 and P2-T7 clause held (POST-FORMAT: sections) |
| P3-T3 | CMD-LINECOUNT | 0 | production 442, PrimeRegistration 175, others equal to anchor |
| P3-T4 | dotnet tool run csharpier check . | 0 | no unformatted file reported |
| P3-T5 | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true | 0 | ERRORS 0, WARNINGS 0, SKIP_CORECOMPILE_LINES: 0, CSC_OUT_TASKMASTER 2, CSC_OUT_TASKMASTER_TEST 2 |
| P3-T6 | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true | 0 | ERRORS 0, WARNINGS 0, SKIP_CORECOMPILE_LINES: 0, CSC_OUT_TASKMASTER 2, CSC_OUT_TASKMASTER_TEST 2 |
| P3-T7 | vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll (FILTER-COORD) | 0 | 28 of 28 passed; seven NAMES-944 Passed |
| P3-T8 | dotnet-coverage collect ... vstest.console.exe (9 test assemblies) (CMD-COVERAGE-DIRECT), then CMD-COVERAGE-POST | 1 | FIRST-ATTEMPT-FAILED-SET: QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces, QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.RemainingLoadActive_AfterLoaderCompletes_BecomesFalse (admitted first-attempt failure set; restart approved once under revision round 3, R3-1) |

## Pass 2 (the clean pass)

| Task | Command | Exit code | Observation |
|---|---|---|---|
| P3-T1 | dotnet tool run csharpier format . | 0 | rewritten count 0 (both Write Set hashes identical before and after); scoped porcelain before and after empty, identical line sets |
| P3-T2 | CMD-TOKEN-COUNT (TOKENS-PARTIAL, TOKENS-PROD), CMD-PRIME-SPANS, CMD-PHRASE-COUNT, P2-T6 added-lines payload, CMD-REGION-COMPARE, protected-file git diff --exit-code | 0 | every clause held; PASS-2: sections appended |
| P3-T3 | CMD-LINECOUNT | 0 | production 442, PrimeRegistration 175, others equal to anchor |
| P3-T4 | dotnet tool run csharpier check . | 0 | "Checked 1627 files"; the check reported no differences |
| P3-T5 | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true | 0 | ERRORS 0, WARNINGS 0, SKIP_CORECOMPILE_LINES: 0, CSC_OUT_TASKMASTER 2, CSC_OUT_TASKMASTER_TEST 2 |
| P3-T6 | msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true | 0 | ERRORS 0, WARNINGS 0, SKIP_CORECOMPILE_LINES: 0, CSC_OUT_TASKMASTER 2, CSC_OUT_TASKMASTER_TEST 2 |
| P3-T7 | vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll (FILTER-COORD) | 0 | 28 of 28 passed; seven NAMES-944 Passed |
| P3-T8 | dotnet-coverage collect ... vstest.console.exe (9 test assemblies) (CMD-COVERAGE-DIRECT), then CMD-COVERAGE-POST | 0 | COVERAGE-ROUTE: DIRECT; STALL-PROBE: REPRODUCES (P0-T16) selected it; the run exited 0; 7327 of 7327 passed; LINE-FLOOR: MET; BRANCH-FLOOR: MET |

## Required statements

- Pass number: 2 (pass 1 is recorded above with its admitted first-attempt failure set; pass 2 is the clean pass).
- P3-T1: the rewritten count was 0 and the scoped porcelain sets were identical (both empty).
- P3-T4: the check reported no differences.
- P3-T5 and P3-T6: SKIP_CORECOMPILE_LINES: 0, and both CSC_OUT_ counts are at least 1 (2 each).
- P3-T8: COVERAGE-ROUTE: DIRECT; STALL-PROBE: REPRODUCES (P0-T16) selected it; the run exited 0.

Artifacts: evidence/qa-gates/csharpier-format.md, evidence/regression-testing/prime-registration-partial-tokens.md, evidence/qa-gates/production-edit-scope.md, evidence/qa-gates/protected-regions-unchanged.md, evidence/qa-gates/file-line-counts.md, evidence/qa-gates/csharpier-check-final.md, evidence/qa-gates/msbuild-analyzer-final.md, evidence/qa-gates/msbuild-nullable-final.md, evidence/regression-testing/prime-registration-pass-after.md, evidence/qa-gates/coverage-summary.md (each carries a PASS-2: section).
