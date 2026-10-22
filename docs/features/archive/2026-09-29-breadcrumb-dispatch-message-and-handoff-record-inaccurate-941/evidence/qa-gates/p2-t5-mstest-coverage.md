# P2-T5 coverage run (scoped to QuickFiler.Test)
Timestamp: 2026-10-01T07-24
Command: scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test (all streams captured to git-ignored coverage\logs\p2-t5-coverage.log, measurement 1, and coverage\logs\p2-t5-coverage-run2.log, measurement 2)
EXIT_CODE: 0
Output Summary:
Two identical-argument measurements were taken per the P2-T7 re-run clause. Measurement 1 read one QuickFiler line and one branch below baseline; measurement 2 (the second measurement, identical arguments, no source change between) reads equal to baseline. FINAL values are those of measurement 2, which is the run whose raw Cobertura is read by P2-T6. Both exit codes were 0.

Discovered 1 test assemblies.
Coverage threshold assertions skipped: the run is scoped to search root '<worktree root>\QuickFiler.Test' rather than the repository root '<worktree root>'.
First-party coverage: lines 15183/62181 (24.42%), branches 3764/16224 (23.20%)
FINAL-TOTAL: 1470 (BASELINE-TOTAL 1469 plus 1 new test)
FINAL-QF-LINE-RATE: 0.820135 (10460/12754)
FINAL-QF-BRANCH-RATE: 0.782717 (2518/3217)
BASELINE-QF-LINE-RATE: 0.820135 (10460/12754)
BASELINE-QF-BRANCH-RATE: 0.782717 (2518/3217)
MEASUREMENT-1 first-party line: First-party coverage: lines 15182/62181 (24.42%), branches 3763/16224 (23.19%)
MEASUREMENT-1-QF-LINE-RATE: 0.820056 (10459/12754)
MEASUREMENT-1-QF-BRANCH-RATE: 0.782406 (2517/3217)
MEASUREMENT-1 total: 1470 passed 1470, failed 0
SUMMARY-BEGIN
Test run outcome: Completed
Total 1470, executed 1470, passed 1470, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none

SUMMARY-END
PROJECTION-BEGIN
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2294" covered="10460" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="38974" covered="4449" />
    <counter type="BRANCH" missed="10090" covered="1181" />
  </package>
  <package name="TaskVisualization">
    <counter type="LINE" missed="1569" covered="0" />
    <counter type="BRANCH" missed="400" covered="0" />
  </package>
  <package name="SVGControl">
    <counter type="LINE" missed="1580" covered="274" />
    <counter type="BRANCH" missed="573" covered="65" />
  </package>
  <package name="ToDoModel">
    <counter type="LINE" missed="1823" covered="0" />
    <counter type="BRANCH" missed="508" covered="0" />
  </package>
  <package name="Tags">
    <counter type="LINE" missed="758" covered="0" />
    <counter type="BRANCH" missed="190" covered="0" />
  </package>
</report>

PROJECTION-END
MEASUREMENT-1-PROJECTION-QUICKFILER: package QuickFiler LINE missed=2295 covered=10459; BRANCH missed=700 covered=2517 (all other packages identical to the projection above)
Note: scoped run; first-party figure is reference only. Stall check: no stall in either run, no retry.
Loop iteration: 1
Loop history: none
