# P0-T11 Baseline repository-wide test and coverage run

Timestamp: 2026-10-01T20-45
Command: CMD-COVERAGE-DIRECT (STAGE baseline, route DIRECT, background invocation polled until its final TRX_PRESENT line): dotnet-coverage collect --output coverage\baseline-956.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-956.config -- vstest.console.exe <9 test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\956\baseline" "/Logger:trx;LogFileName=baseline-956.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"; then CMD-COVERAGE-POST (STAGE baseline): Get-TrxRunSummary and Format-TrxRunSummary, ConvertTo-KoverageCoberturaXml, Assert-CoberturaLineCoverageThreshold, Assert-CoberturaBranchCoverageThreshold, Get-CoberturaFirstPartyCoverageReport, ConvertTo-JacocoPackageProjection with Assert-JacocoProjectionReconciliation. The collect console stream was teed to coverage\logs\baseline-956.collect.log (git-ignored).
EXIT_CODE: 0
Output Summary:
COVERAGE-ROUTE: DIRECT
EXCLUSION: &FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests
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
SEQUENCE_FILES: 0
TRX_PRESENT: True
LINE-FLOOR: MET
BRANCH-FLOOR: MET
First-party coverage: lines 56113/65760 (85.33%), branches 13594/17054 (79.71%)
FIRST-PARTY-LINE-PERCENT: 85.33
FIRST-PARTY-BRANCH-PERCENT: 79.71
SUMMARY-BEGIN
Test run outcome: Completed
Total 7336, executed 7336, passed 7336, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
SUMMARY-END
FAILED-SET: (empty)
PROJECTION-BEGIN
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2294" covered="10460" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="4602" covered="38821" />
    <counter type="BRANCH" missed="1861" covered="9410" />
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
    <counter type="LINE" missed="802" covered="2467" />
    <counter type="BRANCH" missed="211" covered="517" />
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
BASELINE-UCS-LINE: 38821/43423
BASELINE-UCS-BRANCH: 9410/11271
Branch: (a) exit 0, both floors met, empty FAILED-SET.
Acceptance: the projection holds a UtilitiesCS package with LINE and BRANCH counters; the First-party coverage line is present with FIRST-PARTY-LINE-PERCENT 85.33 (at least 80) and FIRST-PARTY-BRANCH-PERCENT 79.71 (at least 75); the summary's first line begins `Test run outcome:`; EXIT_CODE 0 equals its expectation (0); the artifact contains no absolute path (all hold). coverage\baseline-956.cobertura.xml and coverage\baseline-956.jacoco.xml remain on disk, git-ignored, for P4-T8.
