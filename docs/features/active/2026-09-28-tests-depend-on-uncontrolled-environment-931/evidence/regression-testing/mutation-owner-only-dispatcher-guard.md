# Negative Control M1: Owner-Only Dispatcher Guard Disabled

Timestamp: 2026-09-29T09-22
Command: (1) CMD-CENSUS with PATH = QuickFiler/Viewers/BreadcrumbUiDispatcher.cs; (2) msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (CMD-BUILD-QF, TASKID p3-t1), resolved through vswhere, plus /nodeReuse:false; (3) vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll "/Settings:TaskMaster.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction" "/ResultsDirectory:coverage\test-results\931\p3-t1" "/Logger:trx;LogFileName=p3-t1.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (CMD-VSTEST, resolved through vswhere)
EXIT_CODE: 1
ExpectedExitCode: 1

Output Summary:
- The EXIT_CODE row is scoped to the mutated run (the VSTEST_EXIT_CODE of step 3); the expected failure is the control's purpose.
- Predicted failure (D-6) observed: the test Failed with a MESSAGE containing "to be 0, but found 1".
- Acceptance: census transitions hold; MSBUILD_EXIT_CODE 0 and PROD_CSC_OUT_LINES 2; total 1, failed 1; MESSAGE contains the predicted phrase; RUNSETTINGS-HASH-NOW equals RUNSETTINGS-HASH - HOLD. MUTATION PREDICTION MISMATCH did not fire.

## Mutated run

- Mutated file: QuickFiler/Viewers/BreadcrumbUiDispatcher.cs
- Hunk: lines 276 to 277 (return _ownerThreadId.HasValue / && Environment.CurrentManagedThreadId == _ownerThreadId.Value;) replaced by the single line return true; (applied with the Edit tool)
- TOKEN return true; = 2 (N1 plus 1; P0-T12 N1 = 1)
- TOKEN _ownerThreadId.HasValue = 0 (N2 minus 1; P0-T12 N2 = 1)
- Mutated-file census, other non-zero tokens: action(); 2, ownerThreadId 5, cannot marshal 2; LINES = 284; SHA256 = 847DBDC8B416FF98C3D3F8599FCFFE6CE1CB106F8FF75C623C7DB06F47E0FF6D
- MSBUILD_EXIT_CODE: 0
- CSC_OUT_LINES: 2
- PROD_CSC_OUT_LINES: 2
- ZERO_ERRORS_LINES: 1
- DLL_ADVANCED: True
- Runsettings file: TaskMaster.runsettings
- Isolation switch: /InIsolation
- Filter: "/TestCaseFilter:FullyQualifiedName~Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction"
- RUNSETTINGS-HASH-NOW: 199408CA53CE4E12AE1A894FC66A0926124F3AC0D6447BD93B0C121338297FFA
- VSTEST_EXIT_CODE: 1
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COLLECTOR_LINES: 0

    COUNTERS total=1 executed=1 passed=0 failed=1
    RESULT_COUNT: 1
    RESULT Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction = Failed
    MESSAGE Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction :: Expected executions to be 0, but found 1 (difference of 1).

## Revert and confirming run

- Recorded at: 2026-09-29T09-23
- Commands: git checkout -- QuickFiler\Viewers\BreadcrumbUiDispatcher.cs; git diff --exit-code HEAD -- QuickFiler\Viewers\BreadcrumbUiDispatcher.cs; git status --porcelain -- QuickFiler\Viewers\BreadcrumbUiDispatcher.cs; CMD-CENSUS on the file; CMD-BUILD-QF (TASKID p3-t2); CMD-VSTEST with ASSEMBLY-QF, FILTER-DISPATCHER, NAMES-FOUR, TASKID p3-t2
- REVERT-DIFF-EXIT: 0
- Porcelain output: EMPTY
- TOKEN return true; = 1 (P0-T12 N1 = 1)
- TOKEN _ownerThreadId.HasValue = 1 (P0-T12 N2 = 1)
- SHA256: 0764D49C8747276722853BF30FE32ACA133CB19A3D634A9CDA351217FD49017E (P0-T12 SHA256 of the same file: 0764D49C8747276722853BF30FE32ACA133CB19A3D634A9CDA351217FD49017E; equal)
- MSBUILD_EXIT_CODE: 0; CSC_OUT_LINES: 2; PROD_CSC_OUT_LINES: 2; ZERO_ERRORS_LINES: 1; DLL_ADVANCED: True
- CONFIRMING-RUN-EXIT: 0
- RUNSETTINGS-HASH-NOW: 199408CA53CE4E12AE1A894FC66A0926124F3AC0D6447BD93B0C121338297FFA
- Acceptance: REVERT-DIFF-EXIT 0; porcelain empty; both census values equal P0-T12; SHA256 equal to P0-T12; PROD_CSC_OUT_LINES at least 1; CONFIRMING-RUN-EXIT 0 with total 1, passed 1, RESULT Passed - HOLD.

    COUNTERS total=1 executed=1 passed=1 failed=0
    RESULT_COUNT: 1
    RESULT Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction = Passed
