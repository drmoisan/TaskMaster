# P4-T9 MSTest coverage, final stage (final C# pass, iteration 1)

Timestamp: 2026-10-06T18-30
Command: dotnet-coverage collect --output coverage\final-973.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-973.config -- vstest.console.exe <9 test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\973\final" "/Logger:trx;LogFileName=final-973.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (CMD-COVERAGE-DIRECT final: the inner invocation of scripts/vscode/Invoke-MSTestWithCoverage.ps1 with the same collector, vstest, runsettings and ConvertTo-DerivedCoverageSettingsXml settings; route per D6 and spec Planner Amendment 4), then CMD-COVERAGE-POST final (Get-TrxRunSummary, Format-TrxRunSummary, ConvertTo-KoverageCoberturaXml, the two floor assertions, Get-CoberturaFirstPartyCoverageReport, ConvertTo-JacocoPackageProjection, Assert-JacocoProjectionReconciliation)
EXIT_CODE: 0
Output Summary: post-change coverage: first-party lines 85.35% (56207/65855), branches 79.74% (13618/17078); both floors MET (80 line, 75 branch); 7361 tests executed, 7361 passed, 0 failed; nine test assemblies; no hang (SEQUENCE_FILES 0); COLLECT_EXIT_CODE 0.

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

LINE-FLOOR: MET
BRANCH-FLOOR: MET
First-party coverage: lines 56207/65855 (85.35%), branches 13618/17078 (79.74%)
FIRST-PARTY-LINE-PERCENT: 85.35
FIRST-PARTY-BRANCH-PERCENT: 79.74
FIRST-PARTY-LINES-VALID: 65855

PROJECTION-BEGIN
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2294" covered="10460" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="4603" covered="38905" />
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

The raw documents coverage\final-973.cobertura.xml, coverage\final-973.trx and coverage\final-973.jacoco.xml remain under the git-ignored coverage tree for P4-T10; none is copied into the feature folder.
