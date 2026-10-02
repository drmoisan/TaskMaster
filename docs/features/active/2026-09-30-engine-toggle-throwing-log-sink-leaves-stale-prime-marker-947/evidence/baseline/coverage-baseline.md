# Baseline: Repository-Wide Test and Coverage Run (P0-T14)

Timestamp: 2026-10-01T17-42
Task: P0-T14
Command: dotnet-coverage collect --output coverage\baseline-947.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-947.config -- vstest.console.exe <9 test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\947\baseline" "/Logger:trx;LogFileName=baseline-947.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"; then CMD-COVERAGE-POST (STAGE baseline, RAW True)
EXIT_CODE: 0

Output Summary:
- BASE-SHA: 2e6ce2cabe7136bbbc8897fcb39d3e13d654ff85
- COVERAGE-ROUTE: DIRECT (STALL-PROBE: REPRODUCES at P0-T12)
- COLLECT_EXIT_CODE: 0
- Branch: (a) exit 0 with both floors met
- LINE-FLOOR: MET
- BRANCH-FLOOR: MET
- First-party coverage: lines 56109/65750 (85.34%), branches 13597/17054 (79.73%)
- ROOT line-rate=0.853369 branch-rate=0.797291 lines-covered=56109 lines-valid=65750 branches-covered=13597 branches-valid=17054
- METHOD HandleToggleClickAsync span=170-186 elements=13 covered=13 uncovered=0 rate=100
- METHOD StartObservedPrime span=303-327 elements=19 covered=19 uncovered=0 rate=100
- METHOD CompletePrime span=366-382 elements=10 covered=10 uncovered=0 rate=100
- CATCH-ARM-COUNT: 0 (no CATCH-ARM line printed; the negative control for P2-T10)
- Tests: total 7332, executed 7332, passed 7332, failed 0

## Details

RAW: True
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
FILTER: TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests
TRX_PRESENT: True
DOCUMENT_PRESENT: True
SEQUENCE_FILES: 0

PROJECTION-BEGIN
```xml
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2294" covered="10460" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="4596" covered="38827" />
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
```
PROJECTION-END

SUMMARY-BEGIN
```
Test run outcome: Completed
Total 7332, executed 7332, passed 7332, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
```
SUMMARY-END

FAILED-SET: (empty)
COORD-CLASS-NODES: 1
COORD-LINES covered=157 valid=157
COORD-BRANCHES covered=37 valid=38

```
METHOD-LINE HandleToggleClickAsync 171 hits=1
METHOD-LINE HandleToggleClickAsync 172 hits=1
METHOD-LINE HandleToggleClickAsync 173 hits=1
METHOD-LINE HandleToggleClickAsync 174 hits=1
METHOD-LINE HandleToggleClickAsync 175 hits=1
METHOD-LINE HandleToggleClickAsync 179 hits=1
METHOD-LINE HandleToggleClickAsync 180 hits=1
METHOD-LINE HandleToggleClickAsync 181 hits=1
METHOD-LINE HandleToggleClickAsync 182 hits=1
METHOD-LINE HandleToggleClickAsync 183 hits=1
METHOD-LINE HandleToggleClickAsync 184 hits=1
METHOD-LINE HandleToggleClickAsync 185 hits=1
METHOD-LINE HandleToggleClickAsync 186 hits=1
METHOD-LINE StartObservedPrime 309 hits=1
METHOD-LINE StartObservedPrime 310 hits=1
METHOD-LINE StartObservedPrime 311 hits=1
METHOD-LINE StartObservedPrime 312 hits=1
METHOD-LINE StartObservedPrime 313 hits=1
METHOD-LINE StartObservedPrime 314 hits=1
METHOD-LINE StartObservedPrime 315 hits=1
METHOD-LINE StartObservedPrime 316 hits=1
METHOD-LINE StartObservedPrime 317 hits=1
METHOD-LINE StartObservedPrime 318 hits=1
METHOD-LINE StartObservedPrime 319 hits=1
METHOD-LINE StartObservedPrime 320 hits=1
METHOD-LINE StartObservedPrime 321 hits=1
METHOD-LINE StartObservedPrime 322 hits=1
METHOD-LINE StartObservedPrime 323 hits=1
METHOD-LINE StartObservedPrime 324 hits=1
METHOD-LINE StartObservedPrime 325 hits=1
METHOD-LINE StartObservedPrime 326 hits=1
METHOD-LINE StartObservedPrime 327 hits=1
METHOD-LINE CompletePrime 367 hits=1
METHOD-LINE CompletePrime 368 hits=1
METHOD-LINE CompletePrime 369 hits=1
METHOD-LINE CompletePrime 370 hits=1
METHOD-LINE CompletePrime 373 hits=1
METHOD-LINE CompletePrime 374 hits=1
METHOD-LINE CompletePrime 375 hits=1
METHOD-LINE CompletePrime 380 hits=1
METHOD-LINE CompletePrime 381 hits=1
METHOD-LINE CompletePrime 382 hits=1
CATCH-ARM-COUNT: 0
```

Notes:
- The THRESHOLD_MESSAGE and COLLECT_FAILURE_MESSAGE fields are RUNNER-route fields and do not apply to this DIRECT run.
- coverage\baseline-947.cobertura.xml (post-processed in place) and coverage\baseline-947.trx remain on disk under the git-ignored coverage directory for P2-T10; neither raw document is copied into the feature folder.
