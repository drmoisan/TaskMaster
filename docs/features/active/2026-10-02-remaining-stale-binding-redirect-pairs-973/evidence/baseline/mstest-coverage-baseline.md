# P0-T19 MSTest coverage baseline (issue #973)

Timestamp: 2026-10-03T10-54
Command: dotnet-coverage collect --output coverage\baseline-973.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-973.config -- vstest.console.exe <9 test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\973\baseline" "/Logger:trx;LogFileName=baseline-973.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (CMD-COVERAGE-DIRECT baseline: the inner invocation of scripts/vscode/Invoke-MSTestWithCoverage.ps1 with the same collector, vstest, runsettings and ConvertTo-DerivedCoverageSettingsXml settings; route per D6 and spec Planner Amendment 4), then CMD-COVERAGE-POST baseline (Get-TrxRunSummary, Format-TrxRunSummary, ConvertTo-KoverageCoberturaXml, the two floor assertions, Get-CoberturaFirstPartyCoverageReport, ConvertTo-JacocoPackageProjection, Assert-JacocoProjectionReconciliation)
EXIT_CODE: 0
Output Summary: baseline coverage on the unchanged tree (before any edit): first-party lines 85.36%, branches 79.75%; 7361 tests executed, 7361 passed, 0 failed; both floors MET; nine test assemblies; no hang (SEQUENCE_FILES 0).

COVERAGE-ROUTE: DIRECT (Planner Amendment 4)
ROUTE-FILTER: HelperClasses.ShellUtilities_Tests, HelperClasses.ShellUtilitiesStatic_Tests, HelperClasses.SysImageListHelperTests, EmailIntelligence.OSBrowser_Tests
CI-COVERS-EXCLUDED: .github/workflows/_mstest-coverage.yml

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

SUMMARY-BEGIN
Test run outcome: Completed
Total 7361, executed 7361, passed 7361, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
SUMMARY-END
FAILED-SET: (empty)
LINE-FLOOR: MET
BRANCH-FLOOR: MET
First-party coverage: lines 56212/65855 (85.36%), branches 13620/17078 (79.75%)
FIRST-PARTY-LINE-PERCENT: 85.36
FIRST-PARTY-BRANCH-PERCENT: 79.75
FIRST-PARTY-LINES-VALID: 65855

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

The raw documents coverage\baseline-973.cobertura.xml, coverage\baseline-973.trx and coverage\baseline-973.jacoco.xml remain under the git-ignored coverage tree for P4-T10; none is copied into the feature folder.
