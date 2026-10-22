# Final coverage summary (issue #968, task P8-T5)

Timestamp: 2026-10-03T03-31
Command: pwsh -NoProfile -Command '<CMD-COVERAGE-DIRECT payload>' with STAGE final (run with the Bash tool's run_in_background option, no redirection), then pwsh -NoProfile -Command '<CMD-COVERAGE-POST payload>' with STAGE final, RAW True and NAMES-TARGETS; both are the Command Reference macros executed verbatim with PREFIX expanded and WORKTREE substituted
Canonical command: dotnet-coverage collect --output coverage\final-968.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-968.config -- vstest.console.exe <discovered test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\968\final" "/Logger:trx;LogFileName=final-968.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0
Output Summary:
- ITERATION: 1
- WORKTREE-LEAF: agent-a291a7fbabf9d0229 (both payloads)
- COVERAGE-ROUTE: DIRECT (equal to the P0-T16 route)
- RAW: True
- COLLECT_EXIT_CODE: 0
- ASSEMBLY_COUNT: 9 (QuickFiler.Test, SVGControl.Test, Tags.Test, TaskMaster.Test, TaskTree.Test, TaskVisualization.Test, ToDoModel.Test, UtilitiesCS.Test, VBFunctions.Test; each `\<Project>.Test\bin\Debug\<Project>.Test.dll`)
- SEQUENCE_FILES: 0
- TRX_PRESENT: True
- DOCUMENT_PRESENT: True
- PAYLOAD-COMPLETE printed
- LINE-FLOOR: MET
- BRANCH-FLOOR: MET
- First-party coverage: lines 56211/65855 (85.36%), branches 13620/17078 (79.75%) (the numeric post-change headline)
- ROOT line-rate=0.853557 branch-rate=0.797517 lines-covered=56211 lines-valid=65855 branches-covered=13620 branches-valid=17078
- TEST_ASSEMBLY_PACKAGES: 0
- QFCDATAMODEL_CLASS_ENTRIES: 0 (recorded; P0-T17 value 0)
- CHANGED-CODE-COVERAGE: NOT MEASURED (TEST ASSEMBLY EXCLUDED; QFCDATAMODEL EXCLUDED BY ATTRIBUTE)
- FINAL-FAILED-SET: (empty)
- NEW-FAILURES: NONE (no name in FINAL-FAILED-SET absent from BASELINE-FAILED-SET, which is also empty)
- FIGURES-COMPARED: baseline (P0-T17) Total 7361, executed 7361, error 0, timeout 0, aborted 0, notExecuted 0; final Total 7365, executed 7365, error 0, timeout 0, aborted 0, notExecuted 0. Total equals the baseline plus 4 (the four pin-count tests; the fold rewrites two tests and adds none), executed is not less than baseline plus 4, and error, timeout, aborted and notExecuted are each not greater than baseline.
- RUNNER-GREEN: NO (COVERAGE-ROUTE DIRECT)

Test-result summary (trx-derived, verbatim between the markers):

SUMMARY-BEGIN
Test run outcome: Completed
Total 7365, executed 7365, passed 7365, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
SUMMARY-END

RESULT lines (sixteen, NAMES-TARGETS, all Passed; the four pin-count tests appear as passed in the coverage route's test-result summary, AC21; the two rewritten liveness tests pass in the full parallel run):

- RESULT BeginTransactionAsync_ZeroBoundWhileThisTestHoldsThePermit_ThrowsTimeoutExceptionAndReleasesNothing = Passed
- RESULT EnsureDispatcher_ScopeDisposedTwice_IsIdempotent = Passed
- RESULT EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease = Passed
- RESULT DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle = Passed
- RESULT Transaction_DisposedTwice_DoesNotOverReleaseTheGate = Passed
- RESULT DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive = Passed
- RESULT TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition = Passed
- RESULT EnsureDispatcher_UnderATransactionHoldingALiveDispatcher_ReleasingAllPinsLeavesTheLiveDispatcher = Passed
- RESULT SetThemeLight_FromNormal_SelectsLightNormalTheme = Passed
- RESULT EnsureDispatcher_TwoPinsHeld_ReleaseOrderDoesNotChangeTheOutcome = Passed
- RESULT EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt = Passed
- RESULT EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose = Passed
- RESULT Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException = Passed
- RESULT EnsureDispatcher_AfterAFullPinCycle_AFreshSinglePinStillInstallsAndRestores = Passed
- RESULT SetThemeDark_FromNormal_SelectsDarkNormalTheme = Passed
- RESULT Transaction_SecondCallerCannotInstallUntilTheFirstRestores = Passed

MESSAGE lines: none (no non-passed, executed test; no LEAK-DEPENDENT TEST EXPOSED).
