# Baseline coverage summary (issue #968, task P0-T17)

Timestamp: 2026-10-03T02-53
Command: pwsh -NoProfile -Command '<CMD-COVERAGE-DIRECT payload>' with STAGE baseline (run with the Bash tool's run_in_background option, no redirection), then pwsh -NoProfile -Command '<CMD-COVERAGE-POST payload>' with STAGE baseline, RAW True and NAMES-TARGETS; both are the Command Reference macros executed verbatim with PREFIX expanded and WORKTREE substituted
Canonical command: dotnet-coverage collect --output coverage\baseline-968.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-968.config -- vstest.console.exe <discovered test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\968\baseline" "/Logger:trx;LogFileName=baseline-968.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229 (both payloads)
- COVERAGE-ROUTE: DIRECT
- RAW: True
- COLLECT_EXIT_CODE: 0
- ASSEMBLY_COUNT: 9
- ASSEMBLY: \QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
- ASSEMBLY: \SVGControl.Test\bin\Debug\SVGControl.Test.dll
- ASSEMBLY: \Tags.Test\bin\Debug\Tags.Test.dll
- ASSEMBLY: \TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
- ASSEMBLY: \TaskTree.Test\bin\Debug\TaskTree.Test.dll
- ASSEMBLY: \TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
- ASSEMBLY: \ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
- ASSEMBLY: \UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
- ASSEMBLY: \VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
- SEQUENCE_FILES: 0
- TRX_PRESENT: True
- DOCUMENT_PRESENT: True
- PAYLOAD-COMPLETE printed
- LINE-FLOOR: MET
- BRANCH-FLOOR: MET
- First-party coverage: lines 56206/65855 (85.35%), branches 13617/17078 (79.73%)
- ROOT line-rate=0.853481 branch-rate=0.797342 lines-covered=56206 lines-valid=65855 branches-covered=13617 branches-valid=17078
- TEST_ASSEMBLY_PACKAGES: 0
- QFCDATAMODEL_CLASS_ENTRIES: 0 (recorded)
- CHANGED-CODE-COVERAGE: NOT MEASURED (TEST ASSEMBLY EXCLUDED; QFCDATAMODEL EXCLUDED BY ATTRIBUTE)
- BASELINE-FAILED-SET: (empty)
- BASELINE-STATE: GREEN

Test-result summary (trx-derived, verbatim between the markers):

SUMMARY-BEGIN
Test run outcome: Completed
Total 7361, executed 7361, passed 7361, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
SUMMARY-END

RESULT lines (twelve at baseline; the four pin-count tests do not exist yet):

- RESULT TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition = Passed
- RESULT DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive = Passed
- RESULT EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt = Passed
- RESULT SetThemeLight_FromNormal_SelectsLightNormalTheme = Passed
- RESULT SetThemeDark_FromNormal_SelectsDarkNormalTheme = Passed
- RESULT Transaction_SecondCallerCannotInstallUntilTheFirstRestores = Passed
- RESULT Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException = Passed
- RESULT EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose = Passed
- RESULT BeginTransactionAsync_ZeroBoundWhileThisTestHoldsThePermit_ThrowsTimeoutExceptionAndReleasesNothing = Passed
- RESULT DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle = Passed
- RESULT Transaction_DisposedTwice_DoesNotOverReleaseTheGate = Passed
- RESULT EnsureDispatcher_ScopeDisposedTwice_IsIdempotent = Passed

MESSAGE lines: none (no non-passed, executed test).

Branch evaluation (in order): (d) not taken (TRX_PRESENT True, SEQUENCE_FILES 0, exit 0); (c) not taken (TEST_ASSEMBLY_PACKAGES 0); (b) not taken; (a) taken: exit 0 with both floors met, BASELINE-STATE: GREEN.
