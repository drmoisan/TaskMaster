# Baseline Repository-Wide Test and Coverage Run (P0-T11)

Timestamp: 2026-09-30T07-22
Task: P0-T11
Command: CMD-COVERAGE-DIRECT with STAGE baseline: dotnet-coverage collect --output coverage\baseline-940.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-940.config -- vstest.console.exe <9 test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\940\baseline" "/Logger:trx;LogFileName=baseline-940.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (vstest resolved through vswhere; the runner script dot-sourced by absolute path for ConvertTo-DerivedCoverageSettingsXml); then CMD-COVERAGE-POST with STAGE baseline and RAW True (runner helpers ConvertTo-KoverageCoberturaXml, Assert-CoberturaLineCoverageThreshold, Assert-CoberturaBranchCoverageThreshold, Get-CoberturaFirstPartyCoverageReport, ConvertTo-JacocoPackageProjection, Assert-JacocoProjectionReconciliation, Get-TrxRunSummary, Format-TrxRunSummary); then CMD-FILE-COMPARE with STAGE baseline (baseline document compared with itself).
EXIT_CODE: 0
Output Summary: DIRECT route, 9 test assemblies, 7323 tests executed and passed in about 59 seconds (collector started 07-19-34, ended 07-20-37); no hang; both CLAUDE.md floors met; branch (a) applies.
- COVERAGE-ROUTE: DIRECT
- ROUTE-FILTER: TestCategory!=LiveOutlook plus the four-class exclusion (FILTER-UCS-EXCLUDE terms)
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
- First-party coverage: lines 56084/65736 (85.32%), branches 13597/17054 (79.73%)
- FIRST-PARTY-LINE-PERCENT: 85.32
- FIRST-PARTY-BRANCH-PERCENT: 79.73
- FAILED-SET: (empty)
- BASELINE-DEFECT-CLASS-FAILURES: NONE
- BRANCH-TAKEN: (a) exit 0 with both floors met

## TRX-derived summary

SUMMARY-BEGIN
Test run outcome: Completed
Total 7323, executed 7323, passed 7323, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
SUMMARY-END

## JaCoCo package projection

PROJECTION-BEGIN
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2293" covered="10461" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="4608" covered="38815" />
    <counter type="BRANCH" missed="1858" covered="9413" />
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

UtilitiesCS package (derived): LINE 38815/43423 covered (rate 0.893881), BRANCH 9413/11271 covered (rate 0.835153).

## Per-file covered-line read (CMD-FILE-COMPARE, baseline against itself; positive control)

- FILE PhysicalDirectoryInfoAdapter.cs baseline=81/91 final=81/91 NOT-LOWER=True
- FILE PhysicalFileInfoAdapter.cs baseline=69/75 final=69/75 NOT-LOWER=True
- FILE DirectoryInfoWrapper.cs baseline=123/123 final=123/123 NOT-LOWER=True

coverage\baseline-940.jacoco.xml and coverage\baseline-940.cobertura.xml remain on disk under the git-ignored coverage tree for P2-T7; neither the Cobertura document nor the TRX is copied into the feature folder.
