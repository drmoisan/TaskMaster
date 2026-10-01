# Coverage Baseline (P0-T18)

Timestamp: 2026-09-30T13-27
Command: dotnet-coverage collect --output coverage\baseline-944.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-944.config -- vstest.console.exe (9 test assemblies) /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\944\baseline" "/Logger:trx;LogFileName=baseline-944.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (CMD-COVERAGE-DIRECT, STAGE baseline), then CMD-COVERAGE-POST (STAGE baseline, RAW True)
EXIT_CODE: 0
Output Summary:
ANCHOR-SHA: b305903e275b8abf58e8e65831c189f517568fe4
COVERAGE-ROUTE: DIRECT
EXIT_CODE: 0 (COLLECT_EXIT_CODE)
LINE-FLOOR: MET
BRANCH-FLOOR: MET
First-party coverage: lines 56078/65736 (85.31%), branches 13594/17054 (79.71%)
ROOT line-rate=0.853079 branch-rate=0.797115 lines-covered=56078 lines-valid=65736 branches-covered=13594 branches-valid=17054
METHOD StartPrimeIfNeeded span=263-280 elements=13 covered=13 uncovered=0 rate=100
METHOD StartObservedPrime span=292-305 elements=9 covered=9 uncovered=0 rate=100
METHOD CompletePrime span=344-360 elements=10 covered=10 uncovered=0 rate=100
Branch outcome: (a) exit 0 with both floors met; tests 7324 total, 7324 passed, 0 failed.

## Details:

- ANCHOR-SHA: b305903e275b8abf58e8e65831c189f517568fe4 (the origin/main commit anchored on, from P0-T5)
- COVERAGE-ROUTE: DIRECT (selected by STALL-PROBE: REPRODUCES in P0-T16)
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
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- DOCUMENT_PRESENT: True
- Collection wall clock: 13-25-51 to 13-26-49 UTC
- LINE-FLOOR: MET
- BRANCH-FLOOR: MET
- First-party coverage: lines 56078/65736 (85.31%), branches 13594/17054 (79.71%)
- ROOT line-rate=0.853079 branch-rate=0.797115 lines-covered=56078 lines-valid=65736 branches-covered=13594 branches-valid=17054

Test-result summary (derived from the trx by Format-TrxRunSummary):

```
SUMMARY-BEGIN
Test run outcome: Completed
Total 7324, executed 7324, passed 7324, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
SUMMARY-END
```

FAILED-SET: (empty)

JaCoCo package projection:

```
PROJECTION-BEGIN
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2293" covered="10461" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="4614" covered="38809" />
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
```

Coordinator figures (D-8):

- COORD-CLASS-NODES: 1
- COORD-LINES covered=143 valid=143
- COORD-BRANCHES covered=37 valid=38
- METHOD StartPrimeIfNeeded span=263-280 elements=13 covered=13 uncovered=0 rate=100
- METHOD StartObservedPrime span=292-305 elements=9 covered=9 uncovered=0 rate=100
- METHOD CompletePrime span=344-360 elements=10 covered=10 uncovered=0 rate=100

METHOD-LINE rows:

```
METHOD-LINE StartPrimeIfNeeded 264 hits=1
METHOD-LINE StartPrimeIfNeeded 265 hits=1
METHOD-LINE StartPrimeIfNeeded 266 hits=1
METHOD-LINE StartPrimeIfNeeded 267 hits=1
METHOD-LINE StartPrimeIfNeeded 268 hits=1
METHOD-LINE StartPrimeIfNeeded 271 hits=1
METHOD-LINE StartPrimeIfNeeded 272 hits=1
METHOD-LINE StartPrimeIfNeeded 273 hits=1
METHOD-LINE StartPrimeIfNeeded 274 hits=1
METHOD-LINE StartPrimeIfNeeded 275 hits=1
METHOD-LINE StartPrimeIfNeeded 278 hits=1
METHOD-LINE StartPrimeIfNeeded 279 hits=1
METHOD-LINE StartPrimeIfNeeded 280 hits=1
METHOD-LINE StartObservedPrime 297 hits=1
METHOD-LINE StartObservedPrime 298 hits=1
METHOD-LINE StartObservedPrime 299 hits=1
METHOD-LINE StartObservedPrime 300 hits=1
METHOD-LINE StartObservedPrime 301 hits=1
METHOD-LINE StartObservedPrime 302 hits=1
METHOD-LINE StartObservedPrime 303 hits=1
METHOD-LINE StartObservedPrime 304 hits=1
METHOD-LINE StartObservedPrime 305 hits=1
METHOD-LINE CompletePrime 345 hits=1
METHOD-LINE CompletePrime 346 hits=1
METHOD-LINE CompletePrime 347 hits=1
METHOD-LINE CompletePrime 348 hits=1
METHOD-LINE CompletePrime 351 hits=1
METHOD-LINE CompletePrime 352 hits=1
METHOD-LINE CompletePrime 353 hits=1
METHOD-LINE CompletePrime 358 hits=1
METHOD-LINE CompletePrime 359 hits=1
METHOD-LINE CompletePrime 360 hits=1
```

Prospective statement: the planned change will add executable statements only inside `StartPrimeIfNeeded` and `StartObservedPrime`, so the `COORD-LINES valid=` figure is expected to rise at P3-T10 while the `METHOD CompletePrime` row is expected to stay unchanged.

The raw documents coverage\baseline-944.cobertura.xml and coverage\baseline-944.trx remain on disk under the git-ignored coverage directory for P3-T10 and are not committed.
