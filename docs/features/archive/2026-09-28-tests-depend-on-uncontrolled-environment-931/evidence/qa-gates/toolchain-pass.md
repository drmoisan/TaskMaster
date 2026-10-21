# Toolchain Pass (P4-T10)

Timestamp: 2026-09-29T09-43
Command: reconciliation of P4-T1 through P4-T9
EXIT_CODE: 0
ITERATION: 1

Output Summary:

Toolchain commands of the final clean iteration, in CLAUDE.md order:
- 1 Format: dotnet tool run csharpier check . - exit 0 - evidence/qa-gates/p4-t2-csharpier-check.2026-09-29T09-34.md (preceded by dotnet tool run csharpier format . exit 0, REWRITTEN: 0, evidence/qa-gates/p4-t1-csharpier-format.2026-09-29T09-33.md)
- 2 Analyze: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true (plus /nodeReuse:false) - exit 0 - SKIP_CORECOMPILE_LINES: 0 - evidence/qa-gates/p4-t3-msbuild-analyzers.2026-09-29T09-34.md
- 3 Type-check: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true (plus /nodeReuse:false) - exit 0 - SKIP_CORECOMPILE_LINES: 0 - evidence/qa-gates/p4-t4-msbuild-nullable.2026-09-29T09-35.md
- 4 Test with coverage: the MSTest-with-coverage route, COVERAGE-ROUTE: DIRECT (dotnet-coverage collect around vstest.console.exe with the runner's own functions and the four-class exclusion) - exit 0 (COLLECT_EXIT_CODE 0 in MEASUREMENT 1 and MEASUREMENT 2; LINE-FLOOR MET and BRANCH-FLOOR MET) - evidence/qa-gates/coverage-final.md

Parallel-suite runs:
- P4-T5 QuickFiler.Test full assembly - exit 0, total 1468, failed 0 - evidence/regression-testing/parallel-suite-quickfiler-test.md
- P4-T6 UtilitiesCS.Test full assembly with UCS-FILTERARG - exit 0, total 4922, failed 0 - evidence/regression-testing/parallel-suite-utilitiescs-test.md

ITERATIONS: 1

EXPECTATION-MET: P4-T1 YES (EXIT_CODE 0, REWRITTEN 0, FORMAT_CHANGED_TREE NO, porcelain empty)
EXPECTATION-MET: P4-T2 YES (EXIT_CODE 0, CHECKED-DELTA 2)
EXPECTATION-MET: P4-T3 YES (EXIT_CODE 0, ERRORS 0, SKIP_CORECOMPILE_LINES 0, compiler echoes 2 and 2, WRITESET_DIAGNOSTIC_LINES 0)
EXPECTATION-MET: P4-T4 YES (EXIT_CODE 0, ERRORS 0, SKIP_CORECOMPILE_LINES 0, compiler echoes 2 and 2, WRITESET_DIAGNOSTIC_LINES 0)
EXPECTATION-MET: P4-T5 YES (all six acceptance conditions)
EXPECTATION-MET: P4-T6 YES (all six acceptance conditions)
EXPECTATION-MET: P4-T7 YES (all eight acceptance conditions, branch (a))
EXPECTATION-MET: P4-T8 YES (four PACKAGE lines NOT-LOWER=True on MEASUREMENT 2, COMPARABILITY A, CHANGED-PRODUCTION-LINES 0; MEASUREMENT 1 read QuickFiler LINE one line lower and was re-measured once per D-7)
EXPECTATION-MET: P4-T9 YES (all LINES at most 500, every post value holds, hashes equal P4-T1 and FIX-HASH anchors)

LOOP: CLEAN PASS

Acceptance: the four toolchain lines each record exit 0; both SKIP_CORECOMPILE_LINES values are 0; nine EXPECTATION-MET lines each read YES; LOOP: CLEAN PASS is present. All four hold.
