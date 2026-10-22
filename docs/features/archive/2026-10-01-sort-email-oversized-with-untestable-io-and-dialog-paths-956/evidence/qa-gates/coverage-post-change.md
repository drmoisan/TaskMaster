# P4-T7 Final repository-wide test and coverage run

Timestamp: 2026-10-01T21-24
ITERATION: 1
Command: CMD-COVERAGE-DIRECT (STAGE final, route DIRECT, the P0-T11 EXCLUSION, background invocation polled until its final TRX_PRESENT line): dotnet-coverage collect --output coverage\final-956.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-956.config -- vstest.console.exe <9 test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\956\final" "/Logger:trx;LogFileName=final-956.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"; then CMD-COVERAGE-POST (STAGE final): Get-TrxRunSummary and Format-TrxRunSummary, ConvertTo-KoverageCoberturaXml, Assert-CoberturaLineCoverageThreshold, Assert-CoberturaBranchCoverageThreshold, Get-CoberturaFirstPartyCoverageReport, ConvertTo-JacocoPackageProjection with Assert-JacocoProjectionReconciliation. The collect console stream was teed to coverage\logs\final-956.collect.log (git-ignored) and not echoed to the tool output.
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
First-party coverage: lines 56202/65845 (85.36%), branches 13618/17076 (79.75%)
FIRST-PARTY-LINE-PERCENT: 85.36
FIRST-PARTY-BRANCH-PERCENT: 79.75
SUMMARY-BEGIN
Test run outcome: Completed
Total 7354, executed 7354, passed 7354, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
SUMMARY-END
FAILED-SET: (empty)
NEWLY-FAILING: NONE
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
FINAL-UCS-LINE: 38909/43508
FINAL-UCS-BRANCH: 9434/11293
Note (recorded): one statement appended by the executor after the plan payload, `Write-Output ("POST_EXIT: " + $LASTEXITCODE)`, raised a StrictMode InvalidOperation because the Helpers script enables `Set-StrictMode -Version Latest` and no native command had set `$LASTEXITCODE` in that session. It ran after every plan statement had printed and is not a plan statement; it affects no field above.
Branch: (a) exit 0, both floors met, empty FAILED-SET.
Acceptance: the projection holds a UtilitiesCS package with LINE and BRANCH counters; FIRST-PARTY-LINE-PERCENT 85.36 (at least 80) and FIRST-PARTY-BRANCH-PERCENT 79.75 (at least 75) with LINE-FLOOR MET and BRANCH-FLOOR MET; FAILED-SET contains none of NAMES-TST, NAMES-T or NAMES-S (it is empty); NEWLY-FAILING NONE; EXIT_CODE 0 equals its declared expectation (0); the artifact contains no absolute path. All six hold. coverage\final-956.cobertura.xml and coverage\final-956.jacoco.xml remain on disk, git-ignored, for P4-T8 and P4-T9.
