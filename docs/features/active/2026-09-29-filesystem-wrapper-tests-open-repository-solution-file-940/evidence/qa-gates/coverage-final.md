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

## Coordinator ruling (2026-09-30)

Timestamp: 2026-09-30T11-25

The coordinator ruling of 2026-09-30 (option 1; plan D-7 and D-8, plan version 1.3) replaced the `PACKAGE` and `REPO-LINE` not-lower comparison and the second-measurement rule recorded above with a per-file rule: for each of the three changed-coverage files PhysicalDirectoryInfoAdapter.cs, PhysicalFileInfoAdapter.cs and DirectoryInfoWrapper.cs, the covered-line count and the covered-branch count of the gating measurement are not lower than this item's Phase 0 baseline, plus the CLAUDE.md first-party floors (lines at least 80 percent, branches at least 75 percent). MEASUREMENT 1 and MEASUREMENT 2 above, their Outcome section included, remain as the record of the superseded package-level comparison. MEASUREMENT 3 below is the gating measurement, taken on the merged head that contains the orchestrator's post-P2-T6 merge of origin/main; no re-measurement follows it.

## Post-merge anchor

Timestamp: 2026-09-30T11-19
Command: git rev-parse HEAD; git merge-base --is-ancestor 40e587ce20bbd41cd6915707271faae937f1a8e2 HEAD; git rev-parse 40e587ce20bbd41cd6915707271faae937f1a8e2^2; git merge-base HEAD origin/main; git rev-parse origin/main; git diff --exit-code ANCHOR-SHA ANCHOR-SHA-2 -- UtilitiesCS UtilitiesCS.Test (ANCHOR-SHA 231e1c0b55105aeb626bf5a6e8d0266a567cacad, ANCHOR-SHA-2 66afa6372fd82fc1ffd7c81f85a1ad65eebc5817); git diff --name-only ANCHOR-SHA ANCHOR-SHA-2 -- UtilitiesCS TaskMaster.runsettings scripts/vscode/TaskMaster.cli.runsettings coverage.config UtilitiesCS.Test/UtilitiesCS.Test.csproj TaskMaster.Test/Ribbon/RibbonControllerTests.cs TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs; git status --porcelain -- UtilitiesCS UtilitiesCS.Test
EXIT_CODE: 0 (scoped to the `git diff --exit-code ANCHOR-SHA ANCHOR-SHA-2` span)
Output Summary: the merge commit is in HEAD's history, the merge base equals the merge commit's second parent, and the two anchors agree on both source trees, so every PRE-EDIT-HASH- and FIX-HASH- anchor, the Token Census Expectations and the Target Source C citations remain valid after the merge.
- P2-T7-HEAD: d3f01551991a93ce2038db79415540992dc8b5fe
- MERGE-COMMIT-IN-HEAD-EXIT: 0
- ANCHOR-SHA-2: 66afa6372fd82fc1ffd7c81f85a1ad65eebc5817
- MERGE-BASE-NOW: 66afa6372fd82fc1ffd7c81f85a1ad65eebc5817 (equals ANCHOR-SHA-2)
- ORIGIN-MAIN-NOW: 66afa6372fd82fc1ffd7c81f85a1ad65eebc5817 (observation)
- MERGE-UCS-DIFF-EXIT: 0
- MERGE-OUT-OF-SET-DELTA: NONE
- SOURCE-PORCELAIN-P2-T7: EMPTY

## Foreign test-process probe

Timestamp: 2026-09-30T11-20
Command: CMD-FOREIGN-PROCESS-PROBE (the Command Reference block, with one appended statement printing the probe time as `PROBE-TIME:`; the appended line prints no path)
EXIT_CODE: 0
Output Summary: no vstest.console, testhost or dotnet-coverage process was running on the workstation at either invocation; the first invocation preceded step (d) and the second immediately preceded the step (e) coverage run, as the delegation's concurrency rule requires before every coverage run.
- FOREIGN-PROBE-INVOCATIONS: 2
- First probe output (2026-09-30T11-19, before step d), verbatim:
  - TEST-PROCESSES: 0
  - FOREIGN-TEST-PROCESSES: 0
- Last probe output (2026-09-30T11-20, before step e), verbatim:
  - TEST-PROCESSES: 0
  - FOREIGN-TEST-PROCESSES: 0

## Post-merge rebuilds

Timestamp: 2026-09-30T11-20
Command: CMD-REBUILD with the analyzer GATEARGS and TASKID p2-t7a; CMD-REBUILD with the nullable GATEARGS and TASKID p2-t7b (both msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" plus the gate properties, resolved through vswhere, plus /nodeReuse:false)
EXIT_CODE: 0 (scoped to the nullable rebuild; the analyzer rebuild exit is recorded in its own sub-record)
Output Summary: both post-merge rebuilds are clean; sub-records FEATURE/evidence/qa-gates/p2-t7-msbuild-analyzers.2026-09-30T11-19.md (MSBUILD_EXIT_CODE 0, ERRORS 0, SKIP_CORECOMPILE_LINES 0, UCS_TEST_CSC_OUT_LINES 2, UCS_CSC_OUT_LINES 2, WRITESET_DIAGNOSTIC_LINES 0) and FEATURE/evidence/qa-gates/p2-t7-msbuild-nullable.2026-09-30T11-20.md (the same values).

## Post-merge measurement (MEASUREMENT: 3, STAGE final3)

Timestamp: 2026-09-30T11-22
MEASUREMENT: 3
Command: CMD-COVERAGE-DIRECT with STAGE final3: dotnet-coverage collect --output coverage\final3-940.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-940.config -- vstest.console.exe <9 test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\940\final3" "/Logger:trx;LogFileName=final3-940.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (vstest resolved through vswhere; the runner script dot-sourced by absolute path for ConvertTo-DerivedCoverageSettingsXml; started as a background invocation and polled; the collector console stream went to the git-ignored Tee-Object log coverage\logs\final3-940.collect.log only, as in MEASUREMENT 1); then CMD-COVERAGE-POST with STAGE final3 and RAW True.
EXIT_CODE: 0 (scoped to the collector invocation)
Output Summary: DIRECT route on the merged head, 9 test assemblies, 7327 tests executed and passed (one more than MEASUREMENT 1 and 2; the additional test arrives with the merged origin/main tree, since this item's two Write Set files are unchanged since P1-T8); collector console `Total time: 54.7679 Seconds`; no hang document; both CLAUDE.md floors met; no failure; branch (a) applies.
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
- First-party coverage: lines 56092/65736 (85.33%), branches 13597/17054 (79.73%)
- FIRST-PARTY-LINE-PERCENT: 85.33
- FIRST-PARTY-BRANCH-PERCENT: 79.73
- FAILED-SET: (empty)
- NEWLY-FAILING: NONE
- BRANCH-TAKEN: (a) exit 0 with FAILED-SET empty and both floors met

### TRX-derived summary (final3)

SUMMARY-BEGIN
Test run outcome: Completed
Total 7327, executed 7327, passed 7327, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
SUMMARY-END

### JaCoCo package projection (final3)

PROJECTION-BEGIN
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2293" covered="10461" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="4600" covered="38823" />
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

coverage\final3-940.jacoco.xml and coverage\final3-940.cobertura.xml remain on disk under the git-ignored coverage tree; neither the Cobertura document nor the TRX is copied into the feature folder.

## Comparison against coverage-baseline.md (MEASUREMENT: 3, STAGE final3)

Timestamp: 2026-09-30T11-23
Command: CMD-PACKAGE-COMPARE with STAGE final3 (observational); CMD-FILE-COMPARE with STAGE final3 (the gate), then with STAGE final and STAGE final2 (observational re-reads, issued in one payload that ran the Command Reference block unchanged once per stage); CMD-UNTOUCHED-LINES once
EXIT_CODE: 0 (scoped to the CMD-FILE-COMPARE payload)
Output Summary: all three final3 FILE lines read NOT-LOWER=True on covered lines and covered branches with non-zero denominators on both sides, so the per-file rule of the coordinator ruling is met; the observational PACKAGE and REPO-LINE lines for final3 also read NOT-LOWER=True.

Observations (no acceptance reads their NOT-LOWER= field):

- PACKAGE UtilitiesCS LINE baseline=38815/43423 rate=0.893881 final=38823/43423 rate=0.894065 NOT-LOWER=True
- PACKAGE UtilitiesCS BRANCH baseline=9413/11271 rate=0.835152 final=9413/11271 rate=0.835152 NOT-LOWER=True
- REPO-LINE baseline=56084/65736 rate=0.85317 final=56092/65736 rate=0.853292 NOT-LOWER=True

Gate lines (final3):

- FILE PhysicalDirectoryInfoAdapter.cs LINES baseline=81/91 final=91/91 BRANCHES baseline=36/42 final=36/42 NOT-LOWER=True
- FILE PhysicalFileInfoAdapter.cs LINES baseline=69/75 final=71/75 BRANCHES baseline=6/12 final=6/12 NOT-LOWER=True
- FILE DirectoryInfoWrapper.cs LINES baseline=123/123 final=123/123 BRANCHES baseline=3/4 final=3/4 NOT-LOWER=True

OBSERVATIONAL-FILE-LINES:

- final: FILE PhysicalDirectoryInfoAdapter.cs LINES baseline=81/91 final=91/91 BRANCHES baseline=36/42 final=36/42 NOT-LOWER=True
- final: FILE PhysicalFileInfoAdapter.cs LINES baseline=69/75 final=71/75 BRANCHES baseline=6/12 final=6/12 NOT-LOWER=True
- final: FILE DirectoryInfoWrapper.cs LINES baseline=123/123 final=123/123 BRANCHES baseline=3/4 final=3/4 NOT-LOWER=True
- final2: FILE PhysicalDirectoryInfoAdapter.cs LINES baseline=81/91 final=91/91 BRANCHES baseline=36/42 final=36/42 NOT-LOWER=True
- final2: FILE PhysicalFileInfoAdapter.cs LINES baseline=69/75 final=71/75 BRANCHES baseline=6/12 final=6/12 NOT-LOWER=True
- final2: FILE DirectoryInfoWrapper.cs LINES baseline=123/123 final=123/123 BRANCHES baseline=3/4 final=3/4 NOT-LOWER=True

UNTOUCHED lines:

- UNTOUCHED baseline Interfaces\IWinForm\PropertyStore.cs covered=565/663 branches=168
- UNTOUCHED baseline OutlookObjects\Table\OlTableExtensions.Etl.cs covered=268/273 branches=37
- UNTOUCHED baseline EmailIntelligence\SubjectMap\SubjectMapSco.Orchestration.cs covered=127/131 branches=30
- UNTOUCHED final Interfaces\IWinForm\PropertyStore.cs covered=561/663 branches=166
- UNTOUCHED final OutlookObjects\Table\OlTableExtensions.Etl.cs covered=268/273 branches=37
- UNTOUCHED final EmailIntelligence\SubjectMap\SubjectMapSco.Orchestration.cs covered=127/131 branches=30
- UNTOUCHED final2 Interfaces\IWinForm\PropertyStore.cs covered=559/663 branches=165
- UNTOUCHED final2 OutlookObjects\Table\OlTableExtensions.Etl.cs covered=264/273 branches=37
- UNTOUCHED final2 EmailIntelligence\SubjectMap\SubjectMapSco.Orchestration.cs covered=123/131 branches=30
- UNTOUCHED final3 Interfaces\IWinForm\PropertyStore.cs covered=565/663 branches=168
- UNTOUCHED final3 OutlookObjects\Table\OlTableExtensions.Etl.cs covered=268/273 branches=37
- UNTOUCHED final3 EmailIntelligence\SubjectMap\SubjectMapSco.Orchestration.cs covered=123/131 branches=30

First-party lines:

- BASELINE-FIRST-PARTY: First-party coverage: lines 56084/65736 (85.32%), branches 13597/17054 (79.73%) (equal, figure for figure, to the line coverage-baseline.md transcribed)
- FINAL-FIRST-PARTY: First-party coverage: lines 56092/65736 (85.33%), branches 13597/17054 (79.73%) (equal, figure for figure, to the MEASUREMENT 3 line transcribed above from CMD-COVERAGE-POST)
- COMPARABILITY-MEASUREMENT-3: A (both first-party `lines` denominators are 65736)
- CHANGED-PRODUCTION-LINES: 0 (the P1-T8 `git show` path list contains no path under UtilitiesCS/, P1-T31 recorded `PRODUCTION-DIFF-EXIT: 0`, and this task recorded `MERGE-UCS-DIFF-EXIT: 0`, so every changed line of this item is test code outside the coverage denominator and the policy's no-regression-on-changed-lines clause is discharged by the per-file rule over the three files whose coverage the rewritten tests carry)

## Per-file comparator negative control (in memory)

Timestamp: 2026-09-30T11-24
Command: CMD-FILE-COMPARE-CONTROL with STAGE final3
EXIT_CODE: 0
Output Summary: the comparator reads NOT-LOWER=True when a document is compared with itself and NOT-LOWER=False when one covered line or one covered branch is removed in memory, for each of the three files; the document on disk is unchanged (hashes equal), so the comparator is shown able to fail.

- CONTROL-DOC-HASH-BEFORE: E806131BE302EFB40A7EFEF078DAC420DCBD8FE948077D1F90B8ED3A0D41D7F9
- FILE PhysicalDirectoryInfoAdapter.cs CONTROL-SELF LINES baseline=91/91 final=91/91 BRANCHES baseline=36/42 final=36/42 NOT-LOWER=True
- FILE PhysicalDirectoryInfoAdapter.cs CONTROL-LINE LINES baseline=91/91 final=90/91 BRANCHES baseline=36/42 final=36/42 NOT-LOWER=False
- FILE PhysicalDirectoryInfoAdapter.cs CONTROL-BRANCH LINES baseline=91/91 final=91/91 BRANCHES baseline=36/42 final=35/42 NOT-LOWER=False
- FILE PhysicalFileInfoAdapter.cs CONTROL-SELF LINES baseline=71/75 final=71/75 BRANCHES baseline=6/12 final=6/12 NOT-LOWER=True
- FILE PhysicalFileInfoAdapter.cs CONTROL-LINE LINES baseline=71/75 final=70/75 BRANCHES baseline=6/12 final=6/12 NOT-LOWER=False
- FILE PhysicalFileInfoAdapter.cs CONTROL-BRANCH LINES baseline=71/75 final=71/75 BRANCHES baseline=6/12 final=5/12 NOT-LOWER=False
- FILE DirectoryInfoWrapper.cs CONTROL-SELF LINES baseline=123/123 final=123/123 BRANCHES baseline=3/4 final=3/4 NOT-LOWER=True
- FILE DirectoryInfoWrapper.cs CONTROL-LINE LINES baseline=123/123 final=122/123 BRANCHES baseline=3/4 final=3/4 NOT-LOWER=False
- FILE DirectoryInfoWrapper.cs CONTROL-BRANCH LINES baseline=123/123 final=123/123 BRANCHES baseline=3/4 final=2/4 NOT-LOWER=False
- CONTROL-DOC-HASH-AFTER: E806131BE302EFB40A7EFEF078DAC420DCBD8FE948077D1F90B8ED3A0D41D7F9

## Two-measurement variance table (observation, not a gate)

Every cell is `<covered>/<valid>`. The baseline row is transcribed from coverage-baseline.md and from the `baseline=` halves of the FILE lines; the measurement 1 and measurement 2 rows from the PACKAGE and First-party lines of the MEASUREMENT 1 and MEASUREMENT 2 sections above and from the OBSERVATIONAL-FILE-LINES; the measurement 3 row from the MEASUREMENT 3 sections above. PDA is PhysicalDirectoryInfoAdapter.cs, PFA is PhysicalFileInfoAdapter.cs, DIW is DirectoryInfoWrapper.cs.

| Row | UtilitiesCS lines | UtilitiesCS branches | first-party lines | first-party branches | PDA lines | PDA branches | PFA lines | PFA branches | DIW lines | DIW branches |
|---|---|---|---|---|---|---|---|---|---|---|
| baseline | 38815/43423 | 9413/11271 | 56084/65736 | 13597/17054 | 81/91 | 36/42 | 69/75 | 6/12 | 123/123 | 3/4 |
| measurement 1 (final) | 38823/43423 | 9411/11271 | 56092/65736 | 13595/17054 | 91/91 | 36/42 | 71/75 | 6/12 | 123/123 | 3/4 |
| measurement 2 (final2) | 38813/43423 | 9410/11271 | 56082/65736 | 13594/17054 | 91/91 | 36/42 | 71/75 | 6/12 | 123/123 | 3/4 |
| measurement 3 (final3, post-merge) | 38823/43423 | 9413/11271 | 56092/65736 | 13597/17054 | 91/91 | 36/42 | 71/75 | 6/12 | 123/123 | 3/4 |

Untouched UtilitiesCS files (covered lines, transcribed from the UNTOUCHED lines):

| File | baseline | final | final2 | final3 |
|---|---|---|---|---|
| Interfaces\IWinForm\PropertyStore.cs | covered=565/663 | covered=561/663 | covered=559/663 | covered=565/663 |
| OutlookObjects\Table\OlTableExtensions.Etl.cs | covered=268/273 | covered=268/273 | covered=264/273 | covered=268/273 |
| EmailIntelligence\SubjectMap\SubjectMapSco.Orchestration.cs | covered=127/131 | covered=127/131 | covered=123/131 | covered=123/131 |

### MEASUREMENT 3 outcome

- AC8 (amended, D-7): the three final3 FILE lines read NOT-LOWER=True on covered lines and covered branches; FIRST-PARTY-LINE-PERCENT 85.33 (at least 80) and FIRST-PARTY-BRANCH-PERCENT 79.73 (at least 75); NEWLY-FAILING: NONE.
- AC4 (D-8): the same three FILE lines read NOT-LOWER=True.
