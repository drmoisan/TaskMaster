# Coverage Projection (P3-T8)

Timestamp: 2026-10-02T00-27
Command: dotnet-coverage collect --output coverage\final-948.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-948.config -- vstest.console.exe <9 test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\948\final" "/Logger:trx;LogFileName=final-948.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"; then CMD-COVERAGE-POST with STAGE final and RAW True
EXIT_CODE: 0
Output Summary:
MERGE-BASE: 59cbab04f1c854baa2a03b6cbf755c1df4f961b4
COVERAGE-ROUTE: DIRECT
STALL-PROBE: REPRODUCES (P0-T15)
COLLECT_EXIT_CODE: 0
LINE-FLOOR: MET
BRANCH-FLOOR: MET
First-party coverage: lines 56204/65855 (85.35%), branches 13618/17078 (79.74%)
ROOT line-rate=0.853451 branch-rate=0.7974 lines-covered=56204 lines-valid=65855 branches-covered=13618 branches-valid=17078
COORD-FILE-LINE-RATE: 100
METHOD CompletePrime span=405-435 elements=20 covered=20 uncovered=0 rate=100
METHOD BuildPrimeFailedMessage span=473-482 elements=9 covered=9 uncovered=0 rate=100
Branch (a): exit 0, both floors met, FAILED-SET empty; pass 1.

## Details

- MERGE-BASE: 59cbab04f1c854baa2a03b6cbf755c1df4f961b4
- COVERAGE-ROUTE: DIRECT, selected by STALL-PROBE: REPRODUCES at P0-T15 (the amended AC-N names this route)
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
- First-party coverage: lines 56204/65855 (85.35%), branches 13618/17078 (79.74%)
- ROOT line-rate=0.853451 branch-rate=0.7974 lines-covered=56204 lines-valid=65855 branches-covered=13618 branches-valid=17078

PROJECTION-BEGIN

```
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2293" covered="10461" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="4607" covered="38901" />
    <counter type="BRANCH" missed="1861" covered="9432" />
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

SUMMARY-BEGIN

```
Test run outcome: Completed
Total 7361, executed 7361, passed 7361, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
```

SUMMARY-END

- FAILED-SET: (empty)
- COORD-CLASS-NODES: 1
- COORD-LINES covered=177 valid=177
- COORD-BRANCHES covered=39 valid=40
- COORD-FILE-LINE-RATE: 100
- METHOD CompletePrime span=405-435 elements=20 covered=20 uncovered=0 rate=100
- METHOD-LINE CompletePrime 406 hits=1
- METHOD-LINE CompletePrime 407 hits=1
- METHOD-LINE CompletePrime 408 hits=1
- METHOD-LINE CompletePrime 409 hits=1
- METHOD-LINE CompletePrime 412 hits=1
- METHOD-LINE CompletePrime 413 hits=1
- METHOD-LINE CompletePrime 414 hits=1
- METHOD-LINE CompletePrime 420 hits=1
- METHOD-LINE CompletePrime 421 hits=1
- METHOD-LINE CompletePrime 422 hits=1
- METHOD-LINE CompletePrime 424 hits=1
- METHOD-LINE CompletePrime 425 hits=1
- METHOD-LINE CompletePrime 426 hits=1
- METHOD-LINE CompletePrime 427 hits=1
- METHOD-LINE CompletePrime 428 hits=1
- METHOD-LINE CompletePrime 429 hits=1
- METHOD-LINE CompletePrime 431 hits=1
- METHOD-LINE CompletePrime 432 hits=1
- METHOD-LINE CompletePrime 434 hits=1
- METHOD-LINE CompletePrime 435 hits=1
- METHOD BuildPrimeFailedMessage span=473-482 elements=9 covered=9 uncovered=0 rate=100
- METHOD-LINE BuildPrimeFailedMessage 474 hits=1
- METHOD-LINE BuildPrimeFailedMessage 475 hits=1
- METHOD-LINE BuildPrimeFailedMessage 476 hits=1
- METHOD-LINE BuildPrimeFailedMessage 477 hits=1
- METHOD-LINE BuildPrimeFailedMessage 478 hits=1
- METHOD-LINE BuildPrimeFailedMessage 479 hits=1
- METHOD-LINE BuildPrimeFailedMessage 480 hits=1
- METHOD-LINE BuildPrimeFailedMessage 481 hits=1
- METHOD-LINE BuildPrimeFailedMessage 482 hits=1
- GUARD-LINE 421 hits=1 branch=True covered=2 total=2
- SINK-LINE 425 hits=1 branch=False covered=0 total=0
- RECORD-LINE 426 hits=1 branch=False covered=0 total=0

RESULT-level proof that the new tests executed is taken from P3-T7 (FINAL-FIXTURE-RUN in evidence/regression-testing/repeat-fault-suppression-pass-after.md); the run summary carries counts and failed names only.

coverage\final-948.cobertura.xml and coverage\final-948.trx remain on disk, git-ignored, for P3-T10.

## COMPARISON: (P3-T10)

Timestamp: 2026-10-02T00-32. Sources: Output Summary and Details of evidence/baseline/coverage-baseline.md and of this artifact; CMD-CHANGED-LINES over coverage\final-948.cobertura.xml.

- COORD-LINES-BASELINE: 167/167
- COORD-LINES-FINAL: 177/177
- COORD-UNCOVERED-BASELINE: 0
- COORD-UNCOVERED-FINAL: 0
- COORD-BRANCHES-BASELINE: 37/38
- COORD-BRANCHES-FINAL: 39/40
- COORD-FILE-LINE-RATE-FINAL: 100
- METHOD-BASELINE: METHOD CompletePrime span=391-416 elements=15 covered=15 uncovered=0 rate=100
- METHOD-FINAL: METHOD CompletePrime span=405-435 elements=20 covered=20 uncovered=0 rate=100
- METHOD-BASELINE: METHOD BuildPrimeFailedMessage span=454-462 elements=8 covered=8 uncovered=0 rate=100
- METHOD-FINAL: METHOD BuildPrimeFailedMessage span=473-482 elements=9 covered=9 uncovered=0 rate=100
- GUARD-LINE-FINAL: GUARD-LINE 421 hits=1 branch=True covered=2 total=2
- SINK-LINE-FINAL: SINK-LINE 425 hits=1 branch=False covered=0 total=0
- RECORD-LINE-FINAL: RECORD-LINE 426 hits=1 branch=False covered=0 total=0
- GUARD-BRANCH-ROW: ON GUARD LINE
- FIRST-PARTY-BASELINE: First-party coverage: lines 56201/65845 (85.35%), branches 13618/17076 (79.75%)
- FIRST-PARTY-FINAL: First-party coverage: lines 56204/65855 (85.35%), branches 13618/17078 (79.74%)
- ROOT-BASELINE: ROOT line-rate=0.853535 branch-rate=0.797494 lines-covered=56201 lines-valid=65845 branches-covered=13618 branches-valid=17076
- ROOT-FINAL: ROOT line-rate=0.853451 branch-rate=0.7974 lines-covered=56204 lines-valid=65855 branches-covered=13618 branches-valid=17078
- DENOMINATOR-BRANCH: COMPARABLE (root lines-valid 65845 and 65855 differ by 10, within 1 percent of the baseline figure, 658.45); rate clause: final root line-rate 0.853451 is at least 0.853535 minus 0.005 (0.848535), so it holds.

CMD-CHANGED-LINES output:

```
COORD-CLASS-NODES: 1
CHANGED-LINE-COUNT: 35
CHANGED-LINE 83 no line element
CHANGED-LINE 84 no line element
CHANGED-LINE 85 no line element
CHANGED-LINE 86 no line element
CHANGED-LINE 87 hits=1
CHANGED-LINE 88 hits=1
CHANGED-LINE 89 hits=1
CHANGED-LINE 90 hits=1
CHANGED-LINE 91 no line element
CHANGED-LINE 268 no line element
CHANGED-LINE 376 no line element
CHANGED-LINE 377 no line element
CHANGED-LINE 398 no line element
CHANGED-LINE 399 no line element
CHANGED-LINE 400 no line element
CHANGED-LINE 401 no line element
CHANGED-LINE 402 no line element
CHANGED-LINE 403 no line element
CHANGED-LINE 416 no line element
CHANGED-LINE 417 no line element
CHANGED-LINE 418 no line element
CHANGED-LINE 419 no line element
CHANGED-LINE 420 hits=1
CHANGED-LINE 421 hits=1
CHANGED-LINE 423 no line element
CHANGED-LINE 424 hits=1
CHANGED-LINE 425 hits=1
CHANGED-LINE 426 hits=1
CHANGED-LINE 427 hits=1
CHANGED-LINE 428 hits=1
CHANGED-LINE 429 hits=1
CHANGED-LINE 430 no line element
CHANGED-LINE 431 hits=1
CHANGED-LINE 478 hits=1
CHANGED-LINE 479 hits=1
CHANGED-LINES-WITH-ELEMENT: 15
CHANGED-LINES-UNCOVERED: 0
```

Repository figures: the first-party line rate is 85.35% in both runs and the root line rate passes the COMPARABLE rule. The first-party branch rate reads 79.75% at baseline and 79.74% at final. Branches covered are equal (13618), and branches valid rose by 2 (17076 to 17078). The coordinator file accounts for both added branches, and both are covered (37/38 to 39/40). The figure is lower only because the UtilitiesCS package, which this change does not touch, lost 2 covered branches and 8 covered lines between the runs (projection: BRANCH covered 9434 to 9432, LINE covered 38909 to 38901). The TaskMaster package rose (LINE covered 2467 to 2477, BRANCH covered 517 to 519, missed counts unchanged). On the changed code this change does not lower the repository figures: every changed executable line is covered, and the only package it touches gained coverage. The 0.01-point drop in the repository branch rate comes from unrelated-package variance, and that variance is recorded here.

### Clause results

- COORD-FILE-LINE-RATE-FINAL at least 90.00: MET (100)
- RECORD-LINE-FINAL hits at least 1: MET (1)
- GUARD-LINE-FINAL hits strictly greater than RECORD-LINE-FINAL hits: NOT MET (guard hits=1, record hits=1)
- GUARD-BRANCH-ROW ON GUARD LINE with covered equal to total and total at least 2: MET (covered=2 total=2)
- SINK-LINE-FINAL hits at least 1: MET (1)
- CHANGED-LINES-UNCOVERED 0 and CHANGED-LINES-WITH-ELEMENT at least 3: MET (0 and 15)
- METHOD CompletePrime final elements strictly greater than baseline and final uncovered at most baseline: MET (20 > 15; 0 <= 0)
- METHOD BuildPrimeFailedMessage final uncovered at most baseline: MET (0 <= 0)
- COORD-LINES-FINAL covered at least baseline: MET (177 >= 167); COORD-UNCOVERED-FINAL at most baseline: MET (0 <= 0); COORD-BRANCHES-FINAL covered at least baseline: MET (39 >= 37)
- exactly one DENOMINATOR-BRANCH value, rate clause holds: MET (COMPARABLE)

P3-T10 STOPPED: the hit-count clause of D-8 cannot be satisfied with this collector. The dotnet-coverage Cobertura output records `hits` as a binary covered flag: P0-T17 read all 133464 line elements of the baseline document and found MAX-HITS=1 and GT1=0. A line executed many times therefore reports hits=1, and the guard line can never report more hits than the record line. Both guard outcomes are proved by the branch element on the guard line (`condition-coverage` covered=2 of total=2), and at test level by `GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly` (one report across five faulted polls). The task box is left unchecked, pending a plan correction from the orchestrator.

### Clause results (plan version 0.7)

Timestamp: 2026-10-02T03-52. Resume of P3-T10 under plan version 0.7 (resume rule). The `## COMPARISON: (P3-T10)` section above stands as this task's comparison, and its `CMD-CHANGED-LINES` output stands as this task's run of that command. No code file has changed since commit c4c4e7585 (`git diff --stat c4c4e7585 HEAD -- TaskMaster TaskMaster.Test` printed nothing; the scoped porcelain span printed nothing), and coverage\final-948.cobertura.xml and coverage\baseline-948.cobertura.xml are still on disk.

- FIRST-PARTY-DELTA: lines 85.35% minus 85.35% = +0.00 points; branches 79.74% minus 79.75% = -0.01 points (read from FIRST-PARTY-BASELINE and FIRST-PARTY-FINAL)
- NOT-LOWERED-STATEMENT: HOLDS: CHANGED PACKAGE NOT LOWER (the branch delta is negative, so the first-party form does not apply; TaskMaster package LINE covered 2467 to 2477 and missed 802 to 802, BRANCH covered 517 to 519 and missed 211 to 211, from the two projection blocks). Packages whose LINE or BRANCH covered value fell: UtilitiesCS (LINE covered 38909 to 38901; BRANCH covered 9434 to 9432).

Clause results:

- COORD-FILE-LINE-RATE-FINAL at least 90.00: MET (100)
- RECORD-LINE-FINAL hits at least 1: MET (hits=1)
- GUARD-BRANCH-ROW ON GUARD LINE, guard covered equals total, total at least 2: MET (ON GUARD LINE; GUARD-LINE 421 branch=True covered=2 total=2)
- SINK-LINE-FINAL hits at least 1: MET (hits=1)
- CHANGED-LINES-UNCOVERED 0 and CHANGED-LINES-WITH-ELEMENT at least 3: MET (0 and 15)
- METHOD CompletePrime final elements strictly greater than baseline, final uncovered at most baseline: MET (20 greater than 15; 0 at most 0)
- METHOD BuildPrimeFailedMessage final uncovered at most baseline: MET (0 at most 0)
- COORD-LINES-FINAL covered at least COORD-LINES-BASELINE covered: MET (177 at least 167)
- COORD-UNCOVERED-FINAL at most COORD-UNCOVERED-BASELINE: MET (0 at most 0)
- COORD-BRANCHES-FINAL covered at least baseline covered: MET (39 at least 37)
- exactly one DENOMINATOR-BRANCH value, and under COMPARABLE its rate clause holds: MET (COMPARABLE; 0.853451 at least 0.848535)
- NOT-LOWERED-STATEMENT begins HOLDS: MET (HOLDS: CHANGED PACKAGE NOT LOWER)

P3-T10 RESULT: every acceptance clause of plan version 0.7 is MET.

SUPERSEDED: the preceding Clause results subsection evaluated the version 0.5 acceptance; P3-T29 reads only this subsection.
