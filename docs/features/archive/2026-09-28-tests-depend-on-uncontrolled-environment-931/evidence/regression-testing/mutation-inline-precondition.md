# Negative Control M3: Precondition Run Inline

Timestamp: 2026-09-29T09-26
Command: (1) CMD-CENSUS with PATH = QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs; (2) msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (CMD-BUILD-QF, TASKID p3-t5), resolved through vswhere, plus /nodeReuse:false; (3) vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll "/Settings:TaskMaster.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic|FullyQualifiedName~ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic|FullyQualifiedName~InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow|FullyQualifiedName~Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction" "/ResultsDirectory:coverage\test-results\931\p3-t5" "/Logger:trx;LogFileName=p3-t5.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (CMD-VSTEST, resolved through vswhere)
EXIT_CODE: 1
ExpectedExitCode: 1

Output Summary:
- The EXIT_CODE row is scoped to the mutated run (the VSTEST_EXIT_CODE of step 3); the expected failure is the control's purpose.
- Predicted failure (D-6) observed: all four tests Failed at their in-thread distinctness precondition, each MESSAGE containing "dedicated worker thread must not be".
- Acceptance: TOKEN action(); = 2; MSBUILD_EXIT_CODE 0 and CSC_OUT_LINES 2; total 4, failed 4; each of the four MESSAGE lines contains the precondition phrase; RUNSETTINGS-HASH-NOW equals RUNSETTINGS-HASH - HOLD. MUTATION PREDICTION MISMATCH did not fire.

## Mutated run

- Mutated file: QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs
- Hunk: action(); inserted as the first statement of Run, immediately before Exception captured = null; (applied with the Edit tool)
- TOKEN action(); = 2 (post-fix value 1)
- Mutated-file census, other non-zero tokens: new Thread( 1, IsBackground = true 1, thread.Join(); 1, Join( 2, Join() 2, namespace QuickFiler.Test.TestSupport 1, internal static class DedicatedWorkerThread 1, internal static Exception Run(Action action) 1, distinct from every live thread by construction 1; LINES = 49; SHA256 = 20CAEB70471B0F8617F372F31271BBBC70780E52FA3E909645DD5F41F4E6B4D3
- MSBUILD_EXIT_CODE: 0
- CSC_OUT_LINES: 2
- PROD_CSC_OUT_LINES: 0 (observation; no production file changed)
- ZERO_ERRORS_LINES: 1
- DLL_ADVANCED: True
- Runsettings file: TaskMaster.runsettings
- Isolation switch: /InIsolation
- Filter: "/TestCaseFilter:FullyQualifiedName~InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic|FullyQualifiedName~ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic|FullyQualifiedName~InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow|FullyQualifiedName~Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction"
- RUNSETTINGS-HASH-NOW: 199408CA53CE4E12AE1A894FC66A0926124F3AC0D6447BD93B0C121338297FFA
- VSTEST_EXIT_CODE: 1
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COLLECTOR_LINES: 0

    COUNTERS total=4 executed=4 passed=0 failed=4
    RESULT_COUNT: 4
    RESULT ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic = Failed
    RESULT InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow = Failed
    RESULT Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction = Failed
    RESULT InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic = Failed
    MESSAGE ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic :: Expected isOwnerThread to be False because the dedicated worker thread must not be the thread that constructed the viewer, or the boundary assertion would pass vacuously, but found True.
    MESSAGE InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow :: Expected isOwnerThread to be False because the dedicated worker thread must not be the owner thread, or the null-owner escape would be witnessed on the owner thread and the test would pass vacuously, but found True.
    MESSAGE Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction :: Did not expect Environment.CurrentManagedThreadId to be 29 because the dedicated worker thread must not be the owner thread the dispatcher was built for, or the rejection path would never be reached.
    MESSAGE InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic :: Expected isOwnerThread to be False because the dedicated worker thread must not be the thread that constructed the viewer, or the boundary assertion would pass vacuously, but found True.

## Revert and confirming run

- Recorded at: 2026-09-29T09-28
- Commands: git checkout -- QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs; git diff --exit-code HEAD -- QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs; git status --porcelain -- QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs; CMD-CENSUS on the file; CMD-BUILD-QF (TASKID p3-t6); CMD-VSTEST with ASSEMBLY-QF, FILTER-FOUR, NAMES-FOUR, TASKID p3-t6
- REVERT-DIFF-EXIT: 0
- Porcelain output: EMPTY
- TOKEN action(); = 1
- SHA256: 986838E4FD72A7E233B26A1BE03B5912DFBB4016980F515F5C01680A0ABF688E (FIX-HASH-HELPER: 986838E4FD72A7E233B26A1BE03B5912DFBB4016980F515F5C01680A0ABF688E; equal)
- LINES = 48
- MSBUILD_EXIT_CODE: 0; CSC_OUT_LINES: 2; PROD_CSC_OUT_LINES: 0; ZERO_ERRORS_LINES: 1; DLL_ADVANCED: True
- CONFIRMING-RUN-EXIT: 0
- RUNSETTINGS-HASH-NOW: 199408CA53CE4E12AE1A894FC66A0926124F3AC0D6447BD93B0C121338297FFA
- Acceptance: REVERT-DIFF-EXIT 0; porcelain empty; TOKEN action(); = 1 and SHA256 equal to FIX-HASH-HELPER; CSC_OUT_LINES at least 1 with DLL_ADVANCED True; CONFIRMING-RUN-EXIT 0 with total 4, passed 4, all four RESULT lines Passed - HOLD.

    COUNTERS total=4 executed=4 passed=4 failed=0
    RESULT_COUNT: 4
    RESULT InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow = Passed
    RESULT ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic = Passed
    RESULT Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction = Passed
    RESULT InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic = Passed
