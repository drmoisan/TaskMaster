# Coverage Baseline (P0-T11): repository-wide test and coverage run before any edit

Timestamp: 2026-09-29T09-05
Command:
- Route: COVERAGE-ROUTE: DIRECT (fixed by P0-T9, STALL-PROBE: REPRODUCES)
- Run payload (CMD-COVERAGE-DIRECT, STAGE baseline): dotnet-coverage collect --output coverage\baseline-931.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-931.config -- vstest.console.exe <9 test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\931\baseline" "/Logger:trx;LogFileName=baseline-931.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (vstest.console.exe resolved through vswhere; effective settings derived with ConvertTo-DerivedCoverageSettingsXml from coverage.config)
- Post payload (CMD-COVERAGE-POST, STAGE baseline, RAW True): Get-TrxRunSummary / Format-TrxRunSummary, ConvertTo-KoverageCoberturaXml, Assert-CoberturaLineCoverageThreshold, Assert-CoberturaBranchCoverageThreshold, Get-CoberturaFirstPartyCoverageReport, ConvertTo-JacocoPackageProjection, Assert-JacocoProjectionReconciliation (the runner's own helpers, dot-sourced)
- Filter applied by the route: the LiveOutlook category exclusion plus the four-class shell-icon exclusion shown above
EXIT_CODE: 0

Output Summary:
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
- LINE-FLOOR: MET
- BRANCH-FLOOR: MET
- First-party coverage: lines 56085/65737 (85.32%), branches 13595/17052 (79.73%)
- FAILED-SET: (empty; no test failed)
- Branch decided: (a) exit 0 with both floors met; the task completes.

SUMMARY-BEGIN
Test run outcome: Completed
Total 7320, executed 7320, passed 7320, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
SUMMARY-END

PROJECTION-BEGIN
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2293" covered="10461" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="4608" covered="38816" />
    <counter type="BRANCH" missed="1858" covered="9411" />
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
    <counter type="LINE" missed="802" covered="2443" />
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

coverage\baseline-931.jacoco.xml and coverage\baseline-931.cobertura.xml remain on disk, git-ignored, for P4-T8. The raw Cobertura document and the TRX are not copied into the feature folder.
