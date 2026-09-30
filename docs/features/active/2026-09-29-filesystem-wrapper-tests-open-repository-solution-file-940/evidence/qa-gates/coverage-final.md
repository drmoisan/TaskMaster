# Final Repository-Wide Test and Coverage Run (P2-T6)

Timestamp: 2026-09-30T08-14
Task: P2-T6
ITERATION: 1
MEASUREMENT: 1
Command: CMD-COVERAGE-DIRECT with STAGE final: dotnet-coverage collect --output coverage\final-940.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-940.config -- vstest.console.exe <9 test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\940\final" "/Logger:trx;LogFileName=final-940.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (vstest resolved through vswhere; the runner script dot-sourced by absolute path for ConvertTo-DerivedCoverageSettingsXml; started as a background invocation and polled; the collector console stream went to the git-ignored Tee-Object log coverage\logs\final-940.collect.log only); then CMD-COVERAGE-POST with STAGE final and RAW True (runner helpers ConvertTo-KoverageCoberturaXml, Assert-CoberturaLineCoverageThreshold, Assert-CoberturaBranchCoverageThreshold, Get-CoberturaFirstPartyCoverageReport, ConvertTo-JacocoPackageProjection, Assert-JacocoProjectionReconciliation, Get-TrxRunSummary, Format-TrxRunSummary).
EXIT_CODE: 0
Output Summary: DIRECT route, 9 test assemblies, 7326 tests executed and passed (7323 at baseline plus the three test methods the PFS rewrite adds; collector started 08-11-25, ended 08-12-27, console `Total time: 57.5790 Seconds`); no hang document; both CLAUDE.md floors met; no failure, so NEWLY-FAILING is NONE; branch (a) applies.
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
- First-party coverage: lines 56092/65736 (85.33%), branches 13595/17054 (79.72%)
- FIRST-PARTY-LINE-PERCENT: 85.33
- FIRST-PARTY-BRANCH-PERCENT: 79.72
- FAILED-SET: (empty)
- NEWLY-FAILING: NONE
- BRANCH-TAKEN: (a) exit 0 with FAILED-SET empty and both floors met

## TRX-derived summary

SUMMARY-BEGIN
Test run outcome: Completed
Total 7326, executed 7326, passed 7326, failed 0.
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
    <counter type="LINE" missed="4600" covered="38823" />
    <counter type="BRANCH" missed="1860" covered="9411" />
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

UtilitiesCS package (derived): LINE 38823/43423 covered (rate 0.894066), BRANCH 9411/11271 covered (rate 0.834975).

coverage\final-940.jacoco.xml and coverage\final-940.cobertura.xml remain on disk under the git-ignored coverage tree for P2-T7; neither the Cobertura document nor the TRX is copied into the feature folder.

## Comparison against coverage-baseline.md (MEASUREMENT: 1, STAGE final)

Timestamp: 2026-09-30T08-15
Command: CMD-PACKAGE-COMPARE with STAGE final, then CMD-FILE-COMPARE with STAGE final (issued in one payload; the per-file block read the two Cobertura documents into its own variables because the package block had already bound the two JaCoCo projections; the computation is the Command Reference block unchanged)
EXIT_CODE: 0
Output Summary: UtilitiesCS LINE rate, REPO-LINE and all three FILE lines are not lower than baseline; the UtilitiesCS BRANCH rate is lower by two covered branches (9411 against 9413 of 11271), so D-7 requires one identical second measurement (STAGE final2, MEASUREMENT: 2, below).

- PACKAGE UtilitiesCS LINE baseline=38815/43423 rate=0.893881 final=38823/43423 rate=0.894065 NOT-LOWER=True
- PACKAGE UtilitiesCS BRANCH baseline=9413/11271 rate=0.835152 final=9411/11271 rate=0.834975 NOT-LOWER=False
- REPO-LINE baseline=56084/65736 rate=0.85317 final=56092/65736 rate=0.853292 NOT-LOWER=True
- FILE PhysicalDirectoryInfoAdapter.cs baseline=81/91 final=91/91 NOT-LOWER=True
- FILE PhysicalFileInfoAdapter.cs baseline=69/75 final=71/75 NOT-LOWER=True
- FILE DirectoryInfoWrapper.cs baseline=123/123 final=123/123 NOT-LOWER=True
- BASELINE-FIRST-PARTY: First-party coverage: lines 56084/65736 (85.32%), branches 13597/17054 (79.73%) (equal, figure for figure, to the line coverage-baseline.md transcribed)
- FINAL-FIRST-PARTY: First-party coverage: lines 56092/65736 (85.33%), branches 13595/17054 (79.72%) (equal, figure for figure, to the line transcribed above from CMD-COVERAGE-POST)
- COMPARABILITY-MEASUREMENT-1: A (both first-party `lines` denominators are 65736)
- MEASUREMENT-1-OUTCOME: SECOND MEASUREMENT REQUIRED (PACKAGE UtilitiesCS BRANCH NOT-LOWER=False)

## Second measurement (MEASUREMENT: 2, STAGE final2; D-7)

Timestamp: 2026-09-30T08-19
MEASUREMENT: 2
Command: CMD-COVERAGE-DIRECT with STAGE final2 (identical to the MEASUREMENT 1 command with every `final` stage token replaced by `final2`: output coverage\final2-940.cobertura.xml, results directory coverage\test-results\940\final2, trx log file name final2-940.trx; started as a background invocation and polled), then CMD-COVERAGE-POST with STAGE final2 and RAW True, then CMD-PACKAGE-COMPARE and CMD-FILE-COMPARE with STAGE final2.
EXIT_CODE: 0 (scoped to the collector invocation)
Output Summary: collector exit 0, 9 assemblies, 7326 of 7326 passed, no hang document, both floors met; the second measurement again reads lower than baseline, now on the UtilitiesCS LINE and BRANCH rates and on REPO-LINE. Under P2-T7 a second `False` on a `PACKAGE` or `REPO-LINE` line is `AC8: NOT MET`: the run stops here and reports.

- COLLECT_EXIT_CODE: 0
- ASSEMBLY_COUNT: 9 (the same nine `ASSEMBLY:` paths as MEASUREMENT 1)
- SEQUENCE_FILES: 0
- TRX_PRESENT: True
- RAW: True
- LINE-FLOOR: MET
- BRANCH-FLOOR: MET
- First-party coverage: lines 56082/65736 (85.31%), branches 13594/17054 (79.71%)
- FIRST-PARTY-LINE-PERCENT: 85.31
- FIRST-PARTY-BRANCH-PERCENT: 79.71
- FAILED-SET: (empty)
- NEWLY-FAILING: NONE

### TRX-derived summary (final2)

SUMMARY-BEGIN
Test run outcome: Completed
Total 7326, executed 7326, passed 7326, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
SUMMARY-END

### JaCoCo package projection (final2)

PROJECTION-BEGIN
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2293" covered="10461" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="4610" covered="38813" />
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

### Comparison against coverage-baseline.md (final2)

- PACKAGE UtilitiesCS LINE baseline=38815/43423 rate=0.893881 final=38813/43423 rate=0.893835 NOT-LOWER=False
- PACKAGE UtilitiesCS BRANCH baseline=9413/11271 rate=0.835152 final=9410/11271 rate=0.834886 NOT-LOWER=False
- REPO-LINE baseline=56084/65736 rate=0.85317 final=56082/65736 rate=0.85314 NOT-LOWER=False
- FILE PhysicalDirectoryInfoAdapter.cs baseline=81/91 final=91/91 NOT-LOWER=True
- FILE PhysicalFileInfoAdapter.cs baseline=69/75 final=71/75 NOT-LOWER=True
- FILE DirectoryInfoWrapper.cs baseline=123/123 final=123/123 NOT-LOWER=True
- BASELINE-FIRST-PARTY: First-party coverage: lines 56084/65736 (85.32%), branches 13597/17054 (79.73%)
- FINAL-FIRST-PARTY: First-party coverage: lines 56082/65736 (85.31%), branches 13594/17054 (79.71%) (equal, figure for figure, to the line transcribed above from CMD-COVERAGE-POST for final2)
- COMPARABILITY: A (both first-party `lines` denominators are 65736)
- CHANGED-PRODUCTION-LINES: 0 (the P1-T8 `git show` path list contains no path under UtilitiesCS/, and P1-T31 recorded `PRODUCTION-DIFF-EXIT: 0`, so every changed line of this item is test code outside the coverage denominator and the no-regression-on-changed-lines clause has an empty subject)

### Both REPO-LINE lines, quoted

- MEASUREMENT 1: REPO-LINE baseline=56084/65736 rate=0.85317 final=56092/65736 rate=0.853292 NOT-LOWER=True
- MEASUREMENT 2: REPO-LINE baseline=56084/65736 rate=0.85317 final=56082/65736 rate=0.85314 NOT-LOWER=False

### Diagnostic observation (read-only line and branch difference of each final document against the baseline document; not a gate)

Every covered-line and covered-branch shortfall against the baseline lies in UtilitiesCS files that this item does not change and that the two Write Set test classes do not exercise; every difference in the three adapter and wrapper files is a gain.

- MEASUREMENT 1 losses: UtilitiesCS\Interfaces\IWinForm\PropertyStore.cs lines 687, 688, 695, 696 (covered at baseline, not covered) and branch line 684 (4 covered at baseline, 2 covered).
- MEASUREMENT 1 gains: UtilitiesCS\HelperClasses\FileSystem\PhysicalDirectoryInfoAdapter.cs lines 30, 31, 42, 43, 48, 49, 54, 55, 60, 61; UtilitiesCS\HelperClasses\FileSystem\PhysicalFileInfoAdapter.cs lines 52, 53.
- MEASUREMENT 2 losses: PropertyStore.cs lines 571, 573, 687, 688, 695, 696 and branch lines 570 (2 to 1) and 684 (4 to 2); UtilitiesCS\OutlookObjects\Table\OlTableExtensions.Etl.cs lines 291 to 294; UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapSco.Orchestration.cs lines 97 to 100.
- MEASUREMENT 2 gains: the same twelve adapter lines as MEASUREMENT 1.
- Reading: the loss set differs between two identical runs of the same committed tree, which is consistent with run-to-run variance in which other tests reach those files; that attribution is an inference from the two observations and was not verified further.

### Outcome

- AC8: NOT MET (PACKAGE UtilitiesCS LINE and BRANCH, and REPO-LINE, read NOT-LOWER=False on the second measurement)
- STOP: AC8: NOT MET (P2-T7). P2-T7 is left unchecked; P2-T8 onward is not executed.
