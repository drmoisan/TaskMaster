# Coverage Post-Change (P6-T7)

Timestamp: 2026-10-03T12-46
ITERATION: 1
Command: CMD-COVERAGE-DIRECT (STAGE final): dotnet-coverage collect --output coverage\final-959.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-959.config -- vstest.console.exe <9 test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\959\final" "/Logger:trx;LogFileName=final-959.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (background invocation polled until its final TRX_PRESENT: line); then CMD-COVERAGE-POST (STAGE final); route DIRECT; filter as shown (the same P0-T9 exclusion as P0-T11)
EXIT_CODE: 0 (the printed COLLECT_EXIT_CODE)
Output Summary: branch (a): exit 0, both floors met, empty FAILED-SET; 7393 of 7393 tests passed; first-party coverage lines 85.39 percent (baseline 85.36), branches 79.81 percent (baseline 79.75); no newly failing test.

- COVERAGE-ROUTE: DIRECT
- EXCLUSION: &FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests
- SANDBOX-959-EXISTS-BEFORE: False
- SANDBOX-956-EXISTS-BEFORE: False
- SANDBOX-945-EXISTS-BEFORE: False
- COLLECT_EXIT_CODE: 0
- SANDBOX-959-EXISTS-AFTER: False
- SANDBOX-956-EXISTS-AFTER: False
- SANDBOX-945-EXISTS-AFTER: False
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
- First-party coverage: lines 56410/66058 (85.39%), branches 13681/17141 (79.81%)
- FIRST-PARTY-LINE-PERCENT: 85.39
- FIRST-PARTY-BRANCH-PERCENT: 79.81
- FINAL-FIRST-PARTY-LINE-PERCENT: 85.39
- FINAL-FIRST-PARTY-BRANCH-PERCENT: 79.81
- FAILED-SET: (empty)
- NEWLY-FAILING: NONE
- FINAL-UCS-LINE: 39100/43704
- FINAL-UCS-BRANCH: 9495/11356
- FINAL-QF-LINE: 10468/12761
- FINAL-QF-BRANCH: 2518/3217

SUMMARY-BEGIN
Test run outcome: Completed
Total 7393, executed 7393, passed 7393, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
SUMMARY-END

PROJECTION-BEGIN
```xml
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2293" covered="10468" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="4604" covered="39100" />
    <counter type="BRANCH" missed="1861" covered="9495" />
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
```
PROJECTION-END

## Acceptance (P6-T7, all seven required)

1. The projection holds UtilitiesCS and QuickFiler packages with LINE and BRANCH counters: met.
2. FIRST-PARTY-LINE-PERCENT 85.39 (at least 80) and FIRST-PARTY-BRANCH-PERCENT 79.81 (at least 75), with LINE-FLOOR: MET and BRANCH-FLOOR: MET: met.
3. FAILED-SET contains none of the names of NAMES-TST1-FINAL, NAMES-T12, NAMES-TSC-FINAL, NAMES-TAS-FINAL, NAMES-TUL, NAMES-TEF or NAMES-EFC-ARCHIVE (the set is empty): met.
4. NEWLY-FAILING: NONE: met.
5. EXIT_CODE 0 equals its declared expectation (default 0, branch (a)): met.
6. Every SANDBOX- value is False: met.
7. The artifact contains no absolute path (assembly paths are repository-relative): met.

coverage\final-959.cobertura.xml and coverage\final-959.jacoco.xml remain on disk, git-ignored, for P6-T8 and P6-T9.
