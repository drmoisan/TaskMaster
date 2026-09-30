# Final repository-wide test and coverage run

Timestamp: 2026-09-30T12-38
ITERATION: 1
MEASUREMENT: 1
Command: CMD-COVERAGE-DIRECT (STAGE final) then CMD-COVERAGE-POST (STAGE final). Route DIRECT: dotnet-coverage collect --output coverage\final-945.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-945.config -- vstest.console.exe <9 test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\945\final" "/Logger:trx;LogFileName=final-945.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 0

Output Summary:
COVERAGE-ROUTE: DIRECT
EXCLUSION: &FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests (same as coverage-baseline.md)
COLLECT_EXIT_CODE: 0 (branch (a): exit 0, empty FAILED-SET, both floors met)
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
First-party coverage: lines 56104/65750 (85.33%), branches 13594/17054 (79.71%)
FIRST-PARTY-LINE-PERCENT: 85.33
FIRST-PARTY-BRANCH-PERCENT: 79.71
FAILED-SET: (empty)
NEWLY-FAILING: NONE
FINAL-UCS-LINE: 38821/43423
FINAL-UCS-BRANCH: 9410/11271

SUMMARY-BEGIN
Test run outcome: Completed
Total 7331, executed 7331, passed 7331, failed 0.
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
    <counter type="LINE" missed="802" covered="2457" />
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

## Comparison against coverage-baseline.md

Command: CMD-PACKAGE-COMPARE (STAGE final), over coverage\baseline-945.jacoco.xml and coverage\final-945.jacoco.xml (package figures) and the post-processed coverage\baseline-945.cobertura.xml and coverage\final-945.cobertura.xml (repository and per-file figures)

PACKAGE UtilitiesCS LINE baseline=38827/43423 rate=0.894157 final=38821/43423 rate=0.894019 NOT-LOWER=False DEFICIT-POINTS=0.0138 WITHIN-BAND=True
PACKAGE UtilitiesCS BRANCH baseline=9413/11271 rate=0.835152 final=9410/11271 rate=0.834886 NOT-LOWER=False DEFICIT-POINTS=0.0266 WITHIN-BAND=True
REPO-LINE baseline=56110/65750 rate=0.853384 final=56104/65750 rate=0.853293 NOT-LOWER=False DEFICIT-POINTS=0.0091 WITHIN-BAND=True
SORTEMAIL-FILE baseline valid=25 covered=24 uncovered=1
SORTEMAIL-FILE final valid=25 covered=24 uncovered=1
SORTEMAIL-DIR-CLASSES: 13
SORTEMAIL-UNCOVERED-DELTA: 0
BASELINE-FIRST-PARTY: First-party coverage: lines 56110/65750 (85.34%), branches 13597/17054 (79.73%)
FINAL-FIRST-PARTY: First-party coverage: lines 56104/65750 (85.33%), branches 13594/17054 (79.71%)
(Each First-party line equals, figure for figure, the line transcribed from CMD-COVERAGE-POST in coverage-baseline.md and in this file.)
COMPARABILITY: A
CHANGED-PRODUCTION-LINES: 56 (FIX-DIFF-LINES of P1-T7). Both changed production methods carry [ExcludeFromCodeCoverage], so the policy's no-regression-on-changed-lines clause is evaluated by the package-rate comparison of this section.

Reading: the two first-party line denominators are equal (65750), so COMPARABILITY is A and both PACKAGE lines and the REPO-LINE line are gated: each reads WITHIN-BAND=True (deficits 0.0138, 0.0266 and 0.0091 percentage points against the 0.10 band; NOT-LOWER is an observation and reads False for all three). SORTEMAIL-UNCOVERED-DELTA is 0 with valid=25 in both documents, so the change adds no uncovered line to SortEmail.cs. The D-7 variance rule was not triggered and no second measurement was run. The final first-party percentages (85.33 line, 79.71 branch) satisfy the 80 and 75 floors.
