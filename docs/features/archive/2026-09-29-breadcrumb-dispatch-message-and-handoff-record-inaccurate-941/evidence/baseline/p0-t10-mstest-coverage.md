# P0-T10 coverage baseline (scoped to QuickFiler.Test)
Timestamp: 2026-10-01T06-40
Command: scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test (all streams captured to git-ignored coverage\logs\p0-t10-coverage.log)
EXIT_CODE: 0
Output Summary:
Discovered 1 test assemblies.
Coverage threshold assertions skipped: the run is scoped to search root '<worktree root>\QuickFiler.Test' rather than the repository root '<worktree root>'.
First-party coverage: lines 15183/62181 (24.42%), branches 3764/16224 (23.20%)
BASELINE-TOTAL: 1469
BASELINE-QF-LINE-RATE: 0.820135 (10460/12754)
BASELINE-QF-BRANCH-RATE: 0.782717 (2518/3217)
SUMMARY-BEGIN
Test run outcome: Completed
Total 1469, executed 1469, passed 1469, failed 0.
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
Note: scoped run; first-party figure is over all first-party assemblies and is reference only. Unscoped 931 reference (85.32% / 79.73%) is not compared. Stall check: no stall, run completed without retry.
