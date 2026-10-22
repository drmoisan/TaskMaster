# Final loop: repository-wide test and coverage run (issue 942)

Timestamp: 2026-09-30T07-50
Task: P3-T8 (creates this file); P3-T10 appends COMPARISON.
Command: dotnet-coverage collect --output coverage\final-942.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-942.config -- vstest.console.exe <9 test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\942\final" "/Logger:trx;LogFileName=final-942.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"; then CMD-COVERAGE-POST (STAGE final, RAW True)
EXIT_CODE: 0

Output Summary:
- COVERAGE-ROUTE: DIRECT (fixed by P0-T13)
- RAW: True (post-processed in place by CMD-COVERAGE-POST with the native backslash repository root; AUDIT-ABSOLUTE-FILENAMES: 0)
- Attempts: one. The first attempt's FAILED-SET is empty, so the issue 780 re-run rule did not apply.
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
- First-party coverage: lines 56080/65736 (85.31%), branches 13596/17054 (79.72%)
- ROOT line-rate=0.853109 branch-rate=0.797232 lines-covered=56080 lines-valid=65736 branches-covered=13596 branches-valid=17054

Trx-derived summary:

SUMMARY-BEGIN
Test run outcome: Completed
Total 7324, executed 7324, passed 7324, failed 0.
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
    <counter type="LINE" missed="2293" covered="10461" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="4612" covered="38811" />
    <counter type="BRANCH" missed="1859" covered="9412" />
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
- COMPLETEPRIME-SPAN: 344-360
- COMPLETEPRIME-LINE-ELEMENTS: 10
- COMPLETEPRIME-LINE 345 hits=1
- COMPLETEPRIME-LINE 346 hits=1
- COMPLETEPRIME-LINE 347 hits=1
- COMPLETEPRIME-LINE 348 hits=1
- COMPLETEPRIME-LINE 351 hits=1
- COMPLETEPRIME-LINE 352 hits=1
- COMPLETEPRIME-LINE 353 hits=1
- COMPLETEPRIME-LINE 358 hits=1
- COMPLETEPRIME-LINE 359 hits=1
- COMPLETEPRIME-LINE 360 hits=1
- COMPLETEPRIME-UNCOVERED: 0

Branch outcome: (a) exit 0, both floors met, FAILED-SET empty. The summary block's second line reports `failed 0`. RESULT-level proof that the new test executed is in the FINAL-FIXTURE-RUN section of evidence/regression-testing/prime-fault-ordering-pass-after.md (P3-T7). Under the DIRECT route the LINE-FLOOR and BRANCH-FLOOR lines are the floor gate.

The stage documents coverage\final-942.cobertura.xml and coverage\final-942.trx remain on disk under the git-ignored coverage directory; neither is committed.

## COMPARISON:

Timestamp: 2026-09-30T07-52 (P3-T10; read from evidence/baseline/coverage-baseline.md and this file; changed lines from `git diff -U0 231e1c0b55105aeb626bf5a6e8d0266a567cacad -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` against the working tree, which is the tree the final coverage document was generated from)

- COORD-LINES-BASELINE: covered=143 valid=143
- COORD-LINES-FINAL: covered=143 valid=143
- COORD-BRANCHES-BASELINE: covered=37 valid=38
- COORD-BRANCHES-FINAL: covered=37 valid=38
- COMPLETEPRIME-ELEMENTS-BASELINE: 10
- COMPLETEPRIME-ELEMENTS-FINAL: 10
- COMPLETEPRIME-UNCOVERED-FINAL: 0
- FIRST-PARTY-BASELINE: First-party coverage: lines 56083/65736 (85.32%), branches 13597/17054 (79.73%)
- FIRST-PARTY-FINAL: First-party coverage: lines 56080/65736 (85.31%), branches 13596/17054 (79.72%)
- ROOT-BASELINE: ROOT line-rate=0.853155 branch-rate=0.797291 lines-covered=56083 lines-valid=65736 branches-covered=13597 branches-valid=17054
- ROOT-FINAL: ROOT line-rate=0.853109 branch-rate=0.797232 lines-covered=56080 lines-valid=65736 branches-covered=13596 branches-valid=17054
- DENOMINATOR-BRANCH: COMPARABLE (lines-valid 65736 at both stages, a difference of 0, within 1 percent of the baseline figure). Rate clause: final root line-rate 0.853109 is at least the baseline 0.853155 minus 0.005 (0.848155). Holds.
- CHANGED-LINES (added lines of the anchored diff inside COMPLETEPRIME-SPAN 344-360):
  - 355: no line element (comment line)
  - 356: no line element (comment line)
  - 357: no line element (comment line)
  - 359: hits=1 (the moved `_primeTasks.TryRemove(engineName, out _);` statement)
- Added lines outside the span (documentation only, no line elements): 245-247 (GetPrimeTask returns element) and 331-333 (CompletePrime summary element).

Acceptance check (P3-T10):

- COORD-LINES-FINAL valid (143) equals COORD-LINES-BASELINE valid (143): no executable statement added; no COORD-LINES-VALID CHANGED.
- COORD-LINES-FINAL covered (143) is at least baseline covered (143).
- COORD-BRANCHES-FINAL covered (37) is at least baseline covered (37).
- COMPLETEPRIME-ELEMENTS-FINAL (10) equals COMPLETEPRIME-ELEMENTS-BASELINE (10).
- COMPLETEPRIME-UNCOVERED-FINAL: 0.
- Every CHANGED-LINES entry that has a line element has hits at least 1 (line 359, hits=1).
- Exactly one DENOMINATOR-BRANCH value is recorded, and under COMPARABLE its rate clause holds.
- The repository-wide first-party figures moved by 3 covered lines and 1 covered branch outside the coordinator file with an unchanged denominator; the coordinator file, the only production file changed, is unchanged in every figure. This variation is within the D-7 tolerance and is recorded, not attributed to this change.
- All clauses hold.
