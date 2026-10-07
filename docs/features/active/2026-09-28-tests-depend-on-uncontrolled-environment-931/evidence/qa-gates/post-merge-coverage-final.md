# Post-Merge Step 5: Repository-Wide Test and Coverage Run

Timestamp: 2026-09-29T19-56
HEAD: 55a50e9226173d39d3c169b0dc90400b430af59b
Command:
- Route: COVERAGE-ROUTE: DIRECT, the same route coverage-final.md and coverage-baseline.md used (fixed by P0-T9, STALL-PROBE: REPRODUCES). The block dot-sources scripts\vscode\Invoke-MSTestWithCoverage.ps1 by absolute path in this worktree and uses its own functions. Route substitution, recorded: the runner's entry point hard-codes "/TestCaseFilter:TestCategory!=LiveOutlook" and declares no filter parameter, so invoking the entry point directly cannot apply the shell-icon exclusion recorded by P0-T9. It would also produce figures that cannot be compared with the baseline, which was measured with the exclusion.
- Run payload (the plan's CMD-COVERAGE-DIRECT, STAGE postmerge): dotnet-coverage collect --output coverage\postmerge-931.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-931.config -- vstest.console.exe <9 test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\931\postmerge" "/Logger:trx;LogFileName=postmerge-931.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (vstest.console.exe resolved through vswhere; effective settings derived with ConvertTo-DerivedCoverageSettingsXml from coverage.config)
- Post payload (the plan's CMD-COVERAGE-POST, STAGE postmerge, RAW True): Get-TrxRunSummary / Format-TrxRunSummary, ConvertTo-KoverageCoberturaXml, Assert-CoberturaLineCoverageThreshold, Assert-CoberturaBranchCoverageThreshold, Get-CoberturaFirstPartyCoverageReport, ConvertTo-JacocoPackageProjection, Assert-JacocoProjectionReconciliation
- Stage names postmerge and postmerge2 were used so the baseline-931 and final-931 documents stay unchanged.
EXIT_CODE: 0
MEASUREMENT: 1

Output Summary:
- COLLECT_EXIT_CODE: 0
- ASSEMBLY_COUNT: 9 (QuickFiler.Test, SVGControl.Test, Tags.Test, TaskMaster.Test, TaskTree.Test, TaskVisualization.Test, ToDoModel.Test, UtilitiesCS.Test, VBFunctions.Test; each under \<Project>\bin\Debug\)
- SEQUENCE_FILES: 0
- TRX_PRESENT: True
- LINE-FLOOR: MET
- BRANCH-FLOOR: MET
- RECONCILIATION: PASSED (Assert-JacocoProjectionReconciliation did not throw)
- First-party coverage: lines 56079/65736 (85.31%), branches 13597/17054 (79.73%)
- FAILED-SET: (empty; no test failed)
- COVERAGE-COMPARISON: FINDING (UtilitiesCS LINE is lower than the baseline in both measurements; see below)

SUMMARY-BEGIN
Test run outcome: Completed
Total 7323, executed 7323, passed 7323, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
SUMMARY-END

PROJECTION-BEGIN
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2294" covered="10460" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="4612" covered="38811" />
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

## Comparison of MEASUREMENT 1 against coverage-baseline.md

Command: the plan's CMD-PACKAGE-COMPARE (coverage\baseline-931.jacoco.xml against coverage\postmerge-931.jacoco.xml)

PACKAGE UtilitiesCS LINE baseline=38816/43424 rate=0.893884 postmerge=38811/43423 rate=0.893789 NOT-LOWER=False
PACKAGE UtilitiesCS BRANCH baseline=9411/11269 rate=0.835123 postmerge=9413/11271 rate=0.835152 NOT-LOWER=True
PACKAGE QuickFiler LINE baseline=10461/12754 rate=0.820213 postmerge=10460/12754 rate=0.820135 NOT-LOWER=False
PACKAGE QuickFiler BRANCH baseline=2518/3217 rate=0.782717 postmerge=2518/3217 rate=0.782717 NOT-LOWER=True

Two PACKAGE lines read NOT-LOWER=False. Following the P4-T8 and D-7 precedent, the identical command was run once more as MEASUREMENT 2.

## MEASUREMENT 2 (identical command, STAGE postmerge2)

Timestamp: 2026-09-29T19-59
Command: identical to MEASUREMENT 1 with STAGE postmerge2 (CMD-COVERAGE-DIRECT, then CMD-COVERAGE-POST with RAW True)
EXIT_CODE: 0
MEASUREMENT: 2

- COLLECT_EXIT_CODE: 0
- ASSEMBLY_COUNT: 9
- SEQUENCE_FILES: 0
- TRX_PRESENT: True
- LINE-FLOOR: MET
- BRANCH-FLOOR: MET
- RECONCILIATION: PASSED
- First-party coverage: lines 56080/65736 (85.31%), branches 13597/17054 (79.73%)
- FAILED-SET: (empty; no test failed)

SUMMARY-BEGIN
Test run outcome: Completed
Total 7323, executed 7323, passed 7323, failed 0.
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
    <counter type="LINE" missed="4612" covered="38811" />
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

## Comparison of MEASUREMENT 2 against coverage-baseline.md

Command: the plan's CMD-PACKAGE-COMPARE (coverage\baseline-931.jacoco.xml against coverage\postmerge2-931.jacoco.xml)

PACKAGE UtilitiesCS LINE baseline=38816/43424 rate=0.893884 postmerge=38811/43423 rate=0.893789 NOT-LOWER=False
PACKAGE UtilitiesCS BRANCH baseline=9411/11269 rate=0.835123 postmerge=9413/11271 rate=0.835152 NOT-LOWER=True
PACKAGE QuickFiler LINE baseline=10461/12754 rate=0.820213 postmerge=10461/12754 rate=0.820213 NOT-LOWER=True
PACKAGE QuickFiler BRANCH baseline=2518/3217 rate=0.782717 postmerge=2518/3217 rate=0.782717 NOT-LOWER=True

BASELINE-FIRST-PARTY: First-party coverage: lines 56085/65737 (85.32%), branches 13595/17052 (79.73%)
POSTMERGE-FIRST-PARTY: First-party coverage: lines 56080/65736 (85.31%), branches 13597/17054 (79.73%)

## Finding: UtilitiesCS LINE rate is lower than the baseline

Nothing was fixed; this section records the observation only.

- QuickFiler LINE: MEASUREMENT 1 read one covered line fewer (10460) and MEASUREMENT 2 read the baseline value (10461) on an identical denominator of 12754. The merge changed only documentation comments in QuickFiler production code (BreadcrumbBridgeCoordinator.Search.cs and BreadcrumbItemViewerLifecycleCoordinator.Search.cs). This matches the one-line collector variance coverage-final.md recorded for the same package, so QuickFiler is not lower than baseline on MEASUREMENT 2.
- UtilitiesCS LINE: 38811/43423 in both measurements against a baseline of 38816/43424 (rate 0.893789 against 0.893884). A per-file comparison of the post-processed Cobertura documents against the baseline document (line numbers de-duplicated per file name) shows the following:
  - UtilitiesCS\NewtonsoftHelpers\SDIL Reader\ILGlobals.cs: baseline 38/40, post-merge 36/38 in both measurements. The merge from origin/main deleted two static-field declarations with initializers (Cache and modules); the per-file figures, with two fewer lines and two fewer covered lines, are consistent with both deleted lines having been covered.
  - UtilitiesCS\Threading\UiThread.cs: baseline 130/133, post-merge 131/134 in both measurements. The merge from origin/main added one covered line (the `_dispatcher is not null` test).
  - A further 4 covered lines are missing in each measurement, but in a different file each time. In MEASUREMENT 1 they are in UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapSco.Orchestration.cs (127/131 to 123/131). In MEASUREMENT 2 they are lines 339 to 342 of UtilitiesCS\OutlookObjects\Table\OlTableExtensions.Etl.cs (268/273 to 264/273), and those lines were covered in both baseline and final and in MEASUREMENT 1. Neither file was changed by the merge. Because the missing lines move between files from run to run on unchanged code, this part of the drop is attributed to run-to-run collector or test-scheduling variance.
  - The reproducible part of the drop comes from the merge alone: removing 2 covered lines and adding 1 covered line gives 38815/43423 = 0.893881, which is still lower than 0.893884 by 0.000003. The cause is production code that origin/main contributed. None of it comes from this item's changes, which remain test-only (CHANGED-PRODUCTION-LINES for this item: 0, per coverage-final.md).
- UtilitiesCS BRANCH and QuickFiler BRANCH: not lower in either measurement.
- Solution floors: LINE-FLOOR MET and BRANCH-FLOOR MET in both measurements.

The raw Cobertura documents and the TRX files remain under the git-ignored coverage\ tree; this file carries only the committed projection and summary forms.
