# Negative Control M2: Null-Owner Escape Replaced by the Pre-#781 Context-Reference Throw

Timestamp: 2026-09-29T09-24
Command: (1) CMD-CENSUS with PATH = QuickFiler/Viewers/ItemViewer.Breadcrumb.cs; (2) msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (CMD-BUILD-QF, TASKID p3-t3), resolved through vswhere, plus /nodeReuse:false; (3) vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll "/Settings:TaskMaster.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow" "/ResultsDirectory:coverage\test-results\931\p3-t3" "/Logger:trx;LogFileName=p3-t3.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (CMD-VSTEST, resolved through vswhere)
EXIT_CODE: 1
ExpectedExitCode: 1

Output Summary:
- The EXIT_CODE row is scoped to the mutated run (the VSTEST_EXIT_CODE of step 3); the expected failure is the control's purpose.
- Predicted failure (D-6) observed: the test Failed at the captured null assertion with a MESSAGE containing both "InvalidOperationException" and "but found".
- Acceptance: census value 1; MSBUILD_EXIT_CODE 0 and PROD_CSC_OUT_LINES 2; total 1, failed 1; MESSAGE contains both phrases (no NullReferenceException); RUNSETTINGS-HASH-NOW equals RUNSETTINGS-HASH - HOLD. MUTATION PREDICTION MISMATCH did not fire.

## Mutated run

- Mutated file: QuickFiler/Viewers/ItemViewer.Breadcrumb.cs
- Hunk: lines 435 to 438 (the if (owning == null) block); line 437 (return;) replaced by the four lines if (!ReferenceEquals(System.Threading.SynchronizationContext.Current, UiSyncContext)) / { / throw new InvalidOperationException("mutation 931: pre-781 context-reference guard"); / } followed by return; (applied with the Edit tool)
- TOKEN System.Threading.SynchronizationContext.Current = 1 (P0-T12 value 0)
- Mutated-file census, other non-zero tokens: partial class 1; LINES = 464; SHA256 = 3131C371FD6AC78AC6DEFE7775F9515DAD975F984F2C5F2CAFBAED1451726F9C
- MSBUILD_EXIT_CODE: 0
- CSC_OUT_LINES: 2
- PROD_CSC_OUT_LINES: 2
- ZERO_ERRORS_LINES: 1
- DLL_ADVANCED: True
- Runsettings file: TaskMaster.runsettings
- Isolation switch: /InIsolation
- Filter: "/TestCaseFilter:FullyQualifiedName~InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow"
- RUNSETTINGS-HASH-NOW: 199408CA53CE4E12AE1A894FC66A0926124F3AC0D6447BD93B0C121338297FFA
- VSTEST_EXIT_CODE: 1
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COLLECTOR_LINES: 0

    COUNTERS total=1 executed=1 passed=0 failed=1
    RESULT_COUNT: 1
    RESULT InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow = Failed
    MESSAGE InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow :: Expected captured to be <null> because a viewer with no owning dispatcher has no boundary to enforce and must stay inert, but found System.InvalidOperationException: mutation 931: pre-781 context-reference guard
       at QuickFiler.ItemViewer.ThrowIfOffUiBoundary(String operation) in <repo-root>\QuickFiler\Viewers\ItemViewer.Breadcrumb.cs:line 441
       at QuickFiler.ItemViewer.InitializeBreadcrumbPipeline(IFolderHierarchyProvider provider, BreadcrumbPopupUiOperations operations) in <repo-root>\QuickFiler\Viewers\ItemViewer.Breadcrumb.cs:line 51
       at QuickFiler.Test.Viewers.ItemViewerBreadcrumbThreadAffinityTests.<>c__DisplayClass10_1.<InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow>b__0() in <repo-root>\QuickFiler.Test\Viewers\ItemViewerBreadcrumbThreadAffinityTests.Part2.cs:line 172
       at QuickFiler.Test.TestSupport.DedicatedWorkerThread.<>c__DisplayClass0_0.<Run>b__0() in <repo-root>\QuickFiler.Test\TestSupport\DedicatedWorkerThread.cs:line 35.

## Revert and confirming run

- Recorded at: 2026-09-29T09-25
- Commands: git checkout -- QuickFiler\Viewers\ItemViewer.Breadcrumb.cs; git diff --exit-code HEAD -- QuickFiler\Viewers\ItemViewer.Breadcrumb.cs; git status --porcelain -- QuickFiler\Viewers\ItemViewer.Breadcrumb.cs; CMD-CENSUS on the file; CMD-BUILD-QF (TASKID p3-t4); CMD-VSTEST with ASSEMBLY-QF, FILTER-NULLOWNER, NAMES-FOUR, TASKID p3-t4
- REVERT-DIFF-EXIT: 0
- Porcelain output: EMPTY
- TOKEN System.Threading.SynchronizationContext.Current = 0
- SHA256: 928E466A8C4C0D69E1CDF1577E3BF42BBC7E86BB6AA88C21B06F7A96C2F12CCF (P0-T12 SHA256 of the same file: 928E466A8C4C0D69E1CDF1577E3BF42BBC7E86BB6AA88C21B06F7A96C2F12CCF; equal)
- LINES = 460 (P0-T12: 460)
- MSBUILD_EXIT_CODE: 0; CSC_OUT_LINES: 2; PROD_CSC_OUT_LINES: 2; ZERO_ERRORS_LINES: 1; DLL_ADVANCED: True
- CONFIRMING-RUN-EXIT: 0
- RUNSETTINGS-HASH-NOW: 199408CA53CE4E12AE1A894FC66A0926124F3AC0D6447BD93B0C121338297FFA
- Acceptance: REVERT-DIFF-EXIT 0; porcelain empty; census 0; SHA256 equal to P0-T12; PROD_CSC_OUT_LINES at least 1; CONFIRMING-RUN-EXIT 0 with total 1, passed 1, RESULT Passed - HOLD.

    COUNTERS total=1 executed=1 passed=1 failed=0
    RESULT_COUNT: 1
    RESULT InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow = Passed
