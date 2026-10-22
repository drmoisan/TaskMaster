# Final QA step 4: repository-wide test-and-coverage run (P6-T5)

Timestamp: 2026-10-02T01-21
ITERATION: 1
Command: (1) CMD-COVERAGE-DIRECT with STAGE final (route fixed by P0-T15: DIRECT), executed as one pwsh -NoProfile -Command payload started with run_in_background after a STRAY_TEST_PROCESSES / sibling-process check (both 0): PREFIX, then the CMD-COVERAGE-DIRECT body verbatim, ending with PAYLOAD-COMPLETE; CLOCK and CLOCK-END echo lines were added. (2) CMD-COVERAGE-POST with STAGE final and RAW True, PREFIX then the body verbatim with NAMES-TARGETS substituted.
Canonical command: dotnet-coverage collect --output coverage\final-950.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-950.config -- vstest.console.exe <discovered test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\950\final" "/Logger:trx;LogFileName=final-950.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
COVERAGE-ROUTE: DIRECT (equals P0-T15)
RAW: True
COLLECT_EXIT_CODE: 0
ASSEMBLY_COUNT: 9
ASSEMBLY: \QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
ASSEMBLY: \SVGControl.Test\bin\Debug\SVGControl.Test.dll
ASSEMBLY: \Tags.Test\bin\Debug\Tags.Test.dll
ASSEMBLY: \TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
ASSEMBLY: \TaskTree.Test\bin\Debug\TaskTree.Test.dll
ASSEMBLY: \TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
ASSEMBLY: \ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
ASSEMBLY: \UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
ASSEMBLY: \VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
TRX_PRESENT: True
DOCUMENT_PRESENT: True
SEQUENCE_FILES: 0
PAYLOAD-COMPLETE observed (collection payload ran from 2026-10-02T01-20 to 2026-10-02T01-21)

LINE-FLOOR: MET
BRANCH-FLOOR: MET
First-party coverage: lines 56212/65855 (85.36%), branches 13620/17078 (79.75%)
ROOT line-rate=0.853572 branch-rate=0.797517 lines-covered=56212 lines-valid=65855 branches-covered=13620 branches-valid=17078

PROJECTION-BEGIN
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2293" covered="10461" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="4599" covered="38909" />
    <counter type="BRANCH" missed="1859" covered="9434" />
  </package>
  <package name="TaskVisualization">
    <counter type="LINE" missed="143" covered="1426" />
    <counter type="BRANCH" missed="67" covered="333" />
  </package>
  <package name="SVGControl">
    <counter type="LINE" missed="977" covered="877" />
    <counter type="BRANCH" missed="338" covered="300" />
  </package>
  <package name="ToDoModel">
    <counter type="LINE" missed="762" covered="1061" />
    <counter type="BRANCH" missed="260" covered="248" />
  </package>
  <package name="Tags">
    <counter type="LINE" missed="56" covered="702" />
    <counter type="BRANCH" missed="16" covered="174" />
  </package>
  <package name="TaskMaster">
    <counter type="LINE" missed="802" covered="2477" />
    <counter type="BRANCH" missed="211" covered="519" />
  </package>
  <package name="TaskTree">
    <counter type="LINE" missed="11" covered="295" />
    <counter type="BRANCH" missed="8" covered="94" />
  </package>
  <package name="VBFunctions">
    <counter type="LINE" missed="0" covered="4" />
    <counter type="BRANCH" missed="0" covered="0" />
  </package>
</report>
PROJECTION-END

SUMMARY-BEGIN
Test run outcome: Completed
Total 7361, executed 7361, passed 7361, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
SUMMARY-END

FINAL-FAILED-SET: (empty)
RESULT RemainingLoadActive_AfterLoaderCompletes_BecomesFalse = Passed
RESULT InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing = Passed
RESULT InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop = Passed
RESULT RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally = Passed
RESULT InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker = Passed
RESULT Transaction_SecondCallerCannotInstallUntilTheFirstRestores = Passed
RESULT Worker_DoWork_CapturesRemainingLoadTask = Passed
RESULT RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces = Passed
RESULT DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle = Passed
QFCDATAMODEL-CLASS-NODES: 0

NEW-FAILURES: NONE (FINAL-FAILED-SET is empty; BASELINE-FAILED-SET was empty)
FIGURES-COMPARED:
- Total: baseline 7361, final 7361 (equal; this plan adds and removes no test method)
- executed: baseline 7361, final 7361 (not less)
- error: baseline 0, final 0 (not greater)
- timeout: baseline 0, final 0 (not greater)
- aborted: baseline 0, final 0 (not greater)
- notExecuted: baseline 0, final 0 (not greater)
No breach.

RUNNER-GREEN: NO (reason: COVERAGE-ROUTE DIRECT; P0-T15 recorded STALL-PROBE: REPRODUCES, so the runner scripts\vscode\Invoke-MSTestWithCoverage.ps1 was not run verbatim. The collector itself exited 0 with no failed test.)

All P6-T5 acceptance conditions hold: route equals P0-T15, trx present, no Sequence file, class nodes 0, all nine targets Passed, no new failure, figures not regressed, both floors MET. The raw collector document and trx remain under the ignored coverage directory.
