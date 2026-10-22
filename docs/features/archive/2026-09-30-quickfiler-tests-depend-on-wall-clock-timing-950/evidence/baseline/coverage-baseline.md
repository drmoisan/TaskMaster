# Baseline repository-wide test-and-coverage run (P0-T16)

Timestamp: 2026-10-02T00-56
Command: (1) CMD-COVERAGE-DIRECT with STAGE baseline, executed as one pwsh -NoProfile -Command payload started with run_in_background: PREFIX (Set-Location -LiteralPath "WORKTREE"; SetCurrentDirectory; WORKTREE-LEAF echo), then the CMD-COVERAGE-DIRECT body verbatim (dot-source scripts\vscode\Invoke-MSTestWithCoverage.ps1; derive coverage\effective-coverage-950.config from coverage.config with ConvertTo-DerivedCoverageSettingsXml; discover *.Test.dll under bin\Debug excluding obj, ref and .claude; invoke dotnet-coverage collect with the four-class exclusion filter; end with PAYLOAD-COMPLETE). CLOCK and CLOCK-END echo lines were added. (2) CMD-COVERAGE-POST with STAGE baseline and RAW True (DIRECT route), PREFIX then the body verbatim with NAMES-TARGETS substituted.
Canonical command: dotnet-coverage collect --output coverage\baseline-950.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-950.config -- vstest.console.exe <discovered test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\950\baseline" "/Logger:trx;LogFileName=baseline-950.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
COVERAGE-ROUTE: DIRECT (from P0-T15 STALL-PROBE: REPRODUCES)
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
PAYLOAD-COMPLETE observed (collection payload ran from 2026-10-02T00-54 to 2026-10-02T00-55)

LINE-FLOOR: MET
BRANCH-FLOOR: MET
First-party coverage: lines 56204/65855 (85.35%), branches 13618/17078 (79.74%)
ROOT line-rate=0.853451 branch-rate=0.7974 lines-covered=56204 lines-valid=65855 branches-covered=13618 branches-valid=17078

PROJECTION-BEGIN
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2293" covered="10461" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="4607" covered="38901" />
    <counter type="BRANCH" missed="1861" covered="9432" />
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

BASELINE-FAILED-SET: (empty)
RESULT InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing = Passed
RESULT DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle = Passed
RESULT RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces = Passed
RESULT RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally = Passed
RESULT Transaction_SecondCallerCannotInstallUntilTheFirstRestores = Passed
RESULT InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker = Passed
RESULT RemainingLoadActive_AfterLoaderCompletes_BecomesFalse = Passed
RESULT Worker_DoWork_CapturesRemainingLoadTask = Passed
RESULT InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop = Passed
QFCDATAMODEL-CLASS-NODES: 0

Branch evaluation: (d) not met (trx present, no Sequence file, exit 0); (c) not met (class nodes 0); (b) not met; (a) exit 0 with both floors met.
BASELINE-STATE: GREEN

The raw collector document and the trx stay under the ignored coverage directory; only the projection, the first-party line and the trx-derived summary are committed here.
