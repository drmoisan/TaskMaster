# Coverage Baseline (P0-T17)

Timestamp: 2026-10-01T23-31
Command: dotnet-coverage collect --output coverage\baseline-948.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-948.config -- vstest.console.exe <9 test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\948\baseline" "/Logger:trx;LogFileName=baseline-948.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"; then CMD-COVERAGE-POST with STAGE baseline and RAW True
EXIT_CODE: 0
Output Summary:
MERGE-BASE: 59cbab04f1c854baa2a03b6cbf755c1df4f961b4
COVERAGE-ROUTE: DIRECT
COLLECT_EXIT_CODE: 0
LINE-FLOOR: MET
BRANCH-FLOOR: MET
First-party coverage: lines 56201/65845 (85.35%), branches 13618/17076 (79.75%)
ROOT line-rate=0.853535 branch-rate=0.797494 lines-covered=56201 lines-valid=65845 branches-covered=13618 branches-valid=17076
COORD-FILE-LINE-RATE: 100
METHOD CompletePrime span=391-416 elements=15 covered=15 uncovered=0 rate=100
METHOD BuildPrimeFailedMessage span=454-462 elements=8 covered=8 uncovered=0 rate=100
Branch (a): exit 0, both floors met, FAILED-SET empty.

## Details

- MERGE-BASE: 59cbab04f1c854baa2a03b6cbf755c1df4f961b4
- COVERAGE-ROUTE: DIRECT (selected by STALL-PROBE: REPRODUCES at P0-T15)
- Local filter: the four UtilitiesCS.Test shell-icon classes are excluded through the TestCaseFilter above (spec v1.3 AC-N); blame hang timeout: TestTimeout=4min with HangDumpType=None. CI runs these classes unfiltered.
- RAW: True
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
- Pre-run process check: FOREIGN_CANDIDATES: 0; STRAY_TEST_PROCESSES: 0
- LINE-FLOOR: MET
- BRANCH-FLOOR: MET
- First-party coverage: lines 56201/65845 (85.35%), branches 13618/17076 (79.75%)
- ROOT line-rate=0.853535 branch-rate=0.797494 lines-covered=56201 lines-valid=65845 branches-covered=13618 branches-valid=17076

PROJECTION-BEGIN

```
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2294" covered="10460" />
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
    <counter type="LINE" missed="802" covered="2467" />
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

SUMMARY-BEGIN

```
Test run outcome: Completed
Total 7354, executed 7354, passed 7354, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
```

SUMMARY-END

- FAILED-SET: (empty)
- COORD-CLASS-NODES: 1
- COORD-LINES covered=167 valid=167
- COORD-BRANCHES covered=37 valid=38
- COORD-FILE-LINE-RATE: 100
- METHOD CompletePrime span=391-416 elements=15 covered=15 uncovered=0 rate=100
- METHOD-LINE CompletePrime 392 hits=1
- METHOD-LINE CompletePrime 393 hits=1
- METHOD-LINE CompletePrime 394 hits=1
- METHOD-LINE CompletePrime 395 hits=1
- METHOD-LINE CompletePrime 398 hits=1
- METHOD-LINE CompletePrime 399 hits=1
- METHOD-LINE CompletePrime 400 hits=1
- METHOD-LINE CompletePrime 407 hits=1
- METHOD-LINE CompletePrime 408 hits=1
- METHOD-LINE CompletePrime 409 hits=1
- METHOD-LINE CompletePrime 410 hits=1
- METHOD-LINE CompletePrime 411 hits=1
- METHOD-LINE CompletePrime 413 hits=1
- METHOD-LINE CompletePrime 415 hits=1
- METHOD-LINE CompletePrime 416 hits=1
- METHOD BuildPrimeFailedMessage span=454-462 elements=8 covered=8 uncovered=0 rate=100
- METHOD-LINE BuildPrimeFailedMessage 455 hits=1
- METHOD-LINE BuildPrimeFailedMessage 456 hits=1
- METHOD-LINE BuildPrimeFailedMessage 457 hits=1
- METHOD-LINE BuildPrimeFailedMessage 458 hits=1
- METHOD-LINE BuildPrimeFailedMessage 459 hits=1
- METHOD-LINE BuildPrimeFailedMessage 460 hits=1
- METHOD-LINE BuildPrimeFailedMessage 461 hits=1
- METHOD-LINE BuildPrimeFailedMessage 462 hits=1
- GUARD-LINE 0 no line element
- SINK-LINE 408 hits=1 branch=False covered=0 total=0
- RECORD-LINE 0 no line element

Prospective statement: the planned change adds executable statements only inside CompletePrime (the report key, the guard and the record), so the METHOD CompletePrime `elements=` figure is expected to rise at P3-T10 while METHOD BuildPrimeFailedMessage is expected to keep its element count.

Observation recorded for later tasks (not a gate of this task): every line element of the post-processed document carries hits 0 or 1 (a read of all 133464 line elements gave MAX-HITS=1, GT1=0), so this collector's Cobertura output reports hits as a binary covered flag rather than an execution count. The D-8 guard proof at P3-T10 (guard-line hits strictly greater than record-line hits) reads these values.

coverage\baseline-948.cobertura.xml and coverage\baseline-948.trx remain on disk, git-ignored, for P3-T10.
