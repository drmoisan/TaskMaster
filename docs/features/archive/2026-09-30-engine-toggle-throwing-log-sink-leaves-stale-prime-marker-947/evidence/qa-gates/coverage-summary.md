# QA Gate: Repository-Wide Test and Coverage Run, Final (P2-T8)

Timestamp: 2026-10-01T18-09
Task: P2-T8
Command: dotnet-coverage collect --output coverage\final-947.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-947.config -- vstest.console.exe <9 test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\947\final" "/Logger:trx;LogFileName=final-947.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"; then CMD-COVERAGE-POST (STAGE final, RAW True)
EXIT_CODE: 0

Output Summary:
- BASE-SHA: 2e6ce2cabe7136bbbc8897fcb39d3e13d654ff85
- COVERAGE-ROUTE: DIRECT (STALL-PROBE: REPRODUCES at P0-T12; same EXCLUSION as P0-T14)
- COLLECT_EXIT_CODE: 0
- Branch: (a) exit 0 with both floors met; first attempt, no issue 780 re-run
- LINE-FLOOR: MET
- BRANCH-FLOOR: MET
- First-party coverage: lines 56120/65760 (85.34%), branches 13597/17054 (79.73%)
- ROOT line-rate=0.853406 branch-rate=0.797291 lines-covered=56120 lines-valid=65760 branches-covered=13597 branches-valid=17054
- METHOD HandleToggleClickAsync span=173-196 elements=18 covered=18 uncovered=0 rate=100
- METHOD StartObservedPrime span=316-340 elements=19 covered=19 uncovered=0 rate=100
- METHOD CompletePrime span=391-416 elements=15 covered=15 uncovered=0 rate=100
- CATCH-ARM-COUNT: 2 (CATCH-ARM 1 owner=HandleToggleClickAsync, CATCH-ARM 2 owner=CompletePrime; each 3 elements, 0 uncovered)
- Tests: total 7336, executed 7336, passed 7336, failed 0
- STRAY_TEST_PROCESSES: 0 before the collection.

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
    <counter type="LINE" missed="2293" covered="10461" />
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
Total 7336, executed 7336, passed 7336, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
```
SUMMARY-END

FAILED-SET: (empty)
COORD-CLASS-NODES: 1
COORD-LINES covered=167 valid=167
COORD-BRANCHES covered=37 valid=38

```
METHOD-LINE HandleToggleClickAsync 174 hits=1
METHOD-LINE HandleToggleClickAsync 175 hits=1
METHOD-LINE HandleToggleClickAsync 176 hits=1
METHOD-LINE HandleToggleClickAsync 177 hits=1
METHOD-LINE HandleToggleClickAsync 178 hits=1
METHOD-LINE HandleToggleClickAsync 182 hits=1
METHOD-LINE HandleToggleClickAsync 183 hits=1
METHOD-LINE HandleToggleClickAsync 184 hits=1
METHOD-LINE HandleToggleClickAsync 185 hits=1
METHOD-LINE HandleToggleClickAsync 186 hits=1
METHOD-LINE HandleToggleClickAsync 188 hits=1
METHOD-LINE HandleToggleClickAsync 189 hits=1
METHOD-LINE HandleToggleClickAsync 190 hits=1
METHOD-LINE HandleToggleClickAsync 191 hits=1
METHOD-LINE HandleToggleClickAsync 192 hits=1
METHOD-LINE HandleToggleClickAsync 194 hits=1
METHOD-LINE HandleToggleClickAsync 195 hits=1
METHOD-LINE HandleToggleClickAsync 196 hits=1
METHOD-LINE StartObservedPrime 322 hits=1
METHOD-LINE StartObservedPrime 323 hits=1
METHOD-LINE StartObservedPrime 324 hits=1
METHOD-LINE StartObservedPrime 325 hits=1
METHOD-LINE StartObservedPrime 326 hits=1
METHOD-LINE StartObservedPrime 327 hits=1
METHOD-LINE StartObservedPrime 328 hits=1
METHOD-LINE StartObservedPrime 329 hits=1
METHOD-LINE StartObservedPrime 330 hits=1
METHOD-LINE StartObservedPrime 331 hits=1
METHOD-LINE StartObservedPrime 332 hits=1
METHOD-LINE StartObservedPrime 333 hits=1
METHOD-LINE StartObservedPrime 334 hits=1
METHOD-LINE StartObservedPrime 335 hits=1
METHOD-LINE StartObservedPrime 336 hits=1
METHOD-LINE StartObservedPrime 337 hits=1
METHOD-LINE StartObservedPrime 338 hits=1
METHOD-LINE StartObservedPrime 339 hits=1
METHOD-LINE StartObservedPrime 340 hits=1
METHOD-LINE CompletePrime 392 hits=1
METHOD-LINE CompletePrime 393 hits=1
METHOD-LINE CompletePrime 394 hits=1
METHOD-LINE CompletePrime 395 hits=1
METHOD-LINE CompletePrime 398 hits=1
METHOD-LINE CompletePrime 399 hits=1
METHOD-LINE CompletePrime 400 hits=1
METHOD-LINE CompletePrime 407 hits=1
METHOD-LINE CompletePrime 408 hits=1
METHOD-LINE CompletePrime 409 hits=1
METHOD-LINE CompletePrime 410 hits=1
METHOD-LINE CompletePrime 411 hits=1
METHOD-LINE CompletePrime 413 hits=1
METHOD-LINE CompletePrime 415 hits=1
METHOD-LINE CompletePrime 416 hits=1
CATCH-ARM 1 owner=HandleToggleClickAsync span=191-194 CATCH-ARM-ELEMENTS: 3 CATCH-ARM-UNCOVERED: 0
CATCH-ARM-LINE 1 191 hits=1
CATCH-ARM-LINE 1 192 hits=1
CATCH-ARM-LINE 1 194 hits=1
CATCH-ARM 2 owner=CompletePrime span=410-413 CATCH-ARM-ELEMENTS: 3 CATCH-ARM-UNCOVERED: 0
CATCH-ARM-LINE 2 410 hits=1
CATCH-ARM-LINE 2 411 hits=1
CATCH-ARM-LINE 2 413 hits=1
CATCH-ARM-COUNT: 2
```

Notes:
- The THRESHOLD_MESSAGE and COLLECT_FAILURE_MESSAGE fields are RUNNER-route fields and do not apply to this DIRECT run.
- coverage\final-947.cobertura.xml (post-processed in place) and coverage\final-947.trx remain on disk under the git-ignored coverage directory for P2-T10; neither raw document is copied into the feature folder.

## COMPARISON:

Timestamp: 2026-10-01T18-10
Task: P2-T10
Command: CMD-CHANGED-LINES (git diff -U0 2e6ce2cabe7136bbbc8897fcb39d3e13d654ff85 -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs, hits read from coverage\final-947.cobertura.xml); figures read from FEATURE/evidence/baseline/coverage-baseline.md and the P2-T8 section above
EXIT_CODE: 0

Output Summary:
- Coordinator file lines 157/157 to 167/167; branches 37/38 to 37/38.
- CompletePrime: elements 10 to 15, rate 100 to 100 (at least 90.00; elements grew).
- HandleToggleClickAsync: elements 13 to 18; uncovered 0 to 0.
- StartObservedPrime: elements 19 to 19; uncovered 0 to 0.
- CATCH-ARM-COUNT 0 (negative control) to 2; both arms 3 elements, 0 uncovered.
- CHANGED-LINES-WITH-ELEMENT: 12; CHANGED-LINES-UNCOVERED: 0.
- DENOMINATOR-BRANCH: COMPARABLE; final root line-rate 0.853406 is at least 0.853369 - 0.005.
- Result: every P2-T10 acceptance clause holds.

Rows:

```
COORD-LINES-BASELINE: covered=157 valid=157
COORD-LINES-FINAL: covered=167 valid=167
COORD-BRANCHES-BASELINE: covered=37 valid=38
COORD-BRANCHES-FINAL: covered=37 valid=38
METHOD-BASELINE: METHOD HandleToggleClickAsync span=170-186 elements=13 covered=13 uncovered=0 rate=100
METHOD-FINAL: METHOD HandleToggleClickAsync span=173-196 elements=18 covered=18 uncovered=0 rate=100
METHOD-BASELINE: METHOD StartObservedPrime span=303-327 elements=19 covered=19 uncovered=0 rate=100
METHOD-FINAL: METHOD StartObservedPrime span=316-340 elements=19 covered=19 uncovered=0 rate=100
METHOD-BASELINE: METHOD CompletePrime span=366-382 elements=10 covered=10 uncovered=0 rate=100
METHOD-FINAL: METHOD CompletePrime span=391-416 elements=15 covered=15 uncovered=0 rate=100
CATCH-ARM-COUNT-BASELINE: 0
CATCH-ARM-COUNT-FINAL: 2
CATCH-ARM 1 owner=HandleToggleClickAsync span=191-194 CATCH-ARM-ELEMENTS: 3 CATCH-ARM-UNCOVERED: 0
CATCH-ARM 2 owner=CompletePrime span=410-413 CATCH-ARM-ELEMENTS: 3 CATCH-ARM-UNCOVERED: 0
FIRST-PARTY-BASELINE: First-party coverage: lines 56109/65750 (85.34%), branches 13597/17054 (79.73%)
FIRST-PARTY-FINAL: First-party coverage: lines 56120/65760 (85.34%), branches 13597/17054 (79.73%)
ROOT-BASELINE: ROOT line-rate=0.853369 branch-rate=0.797291 lines-covered=56109 lines-valid=65750 branches-covered=13597 branches-valid=17054
ROOT-FINAL: ROOT line-rate=0.853406 branch-rate=0.797291 lines-covered=56120 lines-valid=65760 branches-covered=13597 branches-valid=17054
DENOMINATOR-BRANCH: COMPARABLE (lines-valid 65750 vs 65760 differ by 10, within 1 percent of the baseline figure, 657.5; rate clause: 0.853406 >= 0.848369 holds)
```

CMD-CHANGED-LINES output (verbatim):

```
COORD-CLASS-NODES: 1
CHANGED-LINE-COUNT: 48
CHANGED-LINE 154 no line element
CHANGED-LINE 155 no line element
CHANGED-LINE 167 no line element
CHANGED-LINE 168 no line element
CHANGED-LINE 169 no line element
CHANGED-LINE 170 no line element
CHANGED-LINE 171 no line element
CHANGED-LINE 187 no line element
CHANGED-LINE 188 hits=1
CHANGED-LINE 189 hits=1
CHANGED-LINE 190 hits=1
CHANGED-LINE 191 hits=1
CHANGED-LINE 192 hits=1
CHANGED-LINE 193 no line element
CHANGED-LINE 194 hits=1
CHANGED-LINE 257 no line element
CHANGED-LINE 258 no line element
CHANGED-LINE 259 no line element
CHANGED-LINE 306 no line element
CHANGED-LINE 307 no line element
CHANGED-LINE 308 no line element
CHANGED-LINE 313 no line element
CHANGED-LINE 314 no line element
CHANGED-LINE 368 no line element
CHANGED-LINE 369 no line element
CHANGED-LINE 372 no line element
CHANGED-LINE 380 no line element
CHANGED-LINE 381 no line element
CHANGED-LINE 382 no line element
CHANGED-LINE 383 no line element
CHANGED-LINE 384 no line element
CHANGED-LINE 385 no line element
CHANGED-LINE 386 no line element
CHANGED-LINE 387 no line element
CHANGED-LINE 388 no line element
CHANGED-LINE 389 no line element
CHANGED-LINE 403 no line element
CHANGED-LINE 404 no line element
CHANGED-LINE 405 no line element
CHANGED-LINE 406 no line element
CHANGED-LINE 407 hits=1
CHANGED-LINE 408 hits=1
CHANGED-LINE 409 hits=1
CHANGED-LINE 410 hits=1
CHANGED-LINE 411 hits=1
CHANGED-LINE 412 no line element
CHANGED-LINE 413 hits=1
CHANGED-LINE 414 no line element
CHANGED-LINES-WITH-ELEMENT: 12
CHANGED-LINES-UNCOVERED: 0
```

The lines without a line element are documentation comments, blank lines, the discard comments and brace-only lines that the compiler emits no sequence point for.
