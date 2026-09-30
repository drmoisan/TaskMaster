# Baseline: repository-wide test and coverage run (issue 942)

Timestamp: 2026-09-30T07-31
Task: P0-T14
Command: dotnet-coverage collect --output coverage\baseline-942.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-942.config -- vstest.console.exe <9 test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\942\baseline" "/Logger:trx;LogFileName=baseline-942.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"; then CMD-COVERAGE-POST (STAGE baseline, RAW True)
EXIT_CODE: 0

Output Summary:
- COVERAGE-ROUTE: DIRECT (selected by STALL-PROBE: REPRODUCES in evidence/baseline/stall-probe.md)
- RAW: True (the DIRECT route writes a raw document; CMD-COVERAGE-POST post-processed it in place with the runner's own ConvertTo-KoverageCoberturaXml, using the native backslash spelling of the repository root; AUDIT-ABSOLUTE-FILENAMES: 0 after post-processing)
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
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- DOCUMENT_PRESENT: True
- LINE-FLOOR: MET
- BRANCH-FLOOR: MET
- First-party coverage: lines 56083/65736 (85.32%), branches 13597/17054 (79.73%)
- ROOT line-rate=0.853155 branch-rate=0.797291 lines-covered=56083 lines-valid=65736 branches-covered=13597 branches-valid=17054

Trx-derived summary:

SUMMARY-BEGIN
Test run outcome: Completed
Total 7323, executed 7323, passed 7323, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
SUMMARY-END

FAILED-SET: (empty)

JaCoCo package projection:

PROJECTION-BEGIN
```xml
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2294" covered="10460" />
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
```
PROJECTION-END

Coordinator file figures (TaskMaster/Ribbon/EngineToggleStateCoordinator.cs):

- COORD-CLASS-NODES: 1
- COORD-LINES covered=143 valid=143
- COORD-BRANCHES covered=37 valid=38
- COMPLETEPRIME-SPAN: 341-355
- COMPLETEPRIME-LINE-ELEMENTS: 10
- COMPLETEPRIME-LINE 342 hits=1
- COMPLETEPRIME-LINE 343 hits=1
- COMPLETEPRIME-LINE 344 hits=1
- COMPLETEPRIME-LINE 345 hits=1
- COMPLETEPRIME-LINE 348 hits=1
- COMPLETEPRIME-LINE 350 hits=1
- COMPLETEPRIME-LINE 351 hits=1
- COMPLETEPRIME-LINE 352 hits=1
- COMPLETEPRIME-LINE 354 hits=1
- COMPLETEPRIME-LINE 355 hits=1
- COMPLETEPRIME-UNCOVERED: 0

Branch outcome: (a) exit 0 with both floors met.

Prospective statement: the planned change adds no executable statement to the coordinator (it reorders two statements and edits documentation), so `COORD-LINES valid=` is expected to be unchanged at P3-T10.

The stage documents coverage\baseline-942.cobertura.xml and coverage\baseline-942.trx remain on disk under the git-ignored coverage directory for P3-T10; neither is committed.
