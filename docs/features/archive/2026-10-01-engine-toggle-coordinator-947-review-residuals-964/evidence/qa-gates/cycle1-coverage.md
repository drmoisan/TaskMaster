# Test and coverage gate (P2-T5)

Timestamp: 2026-10-03T09-30
Command: pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1
EXIT_CODE: 0
Output Summary:
- RUNNER_EXIT_CODE: 0 (empty COLLECT_FAILURE_MESSAGE and THRESHOLD_MESSAGE)
- LINE-FLOOR: MET
- BRANCH-FLOOR: MET
- First-party coverage: lines 56639/65881 (85.97%), branches 13687/17082 (80.13%)
- ROOT line-rate=0.859717 branch-rate=0.801253 lines-covered=56639 lines-valid=65881
- COORD-LINES covered=203 valid=203
- COORD-BRANCHES covered=44 valid=44
- COORD-LINE-RATE: 100
- COORD-BRANCH-RATE: 100
- CLASS-NODE TaskMaster/Ribbon/EngineToggleStateCoordinator.cs line-rate=1 branch-rate=1
- CLASS-NODE TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs line-rate=1 branch-rate=1
- CLASS-NODE TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs line-rate=1 branch-rate=1
- FINAL-FAILED-FQN-COUNT: 0
- FINAL-TEST-TOTAL: 7390

Details:
DISCOVERED_LINE: Discovered 9 test assemblies.
THRESHOLD_MESSAGE: (empty)
COLLECT_FAILURE_MESSAGE: (empty)
DOCUMENT_PRESENT: True
TRX_PRESENT: True
SUMMARY_FILE_PRESENT: True
TEST-DEFINITIONS: 7380

SUMMARY-BEGIN
Test run outcome: Completed
Total 7390, executed 7390, passed 7390, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
SUMMARY-END

PROJECTION-BEGIN
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2293" covered="10461" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="4198" covered="39310" />
    <counter type="BRANCH" missed="1797" covered="9496" />
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
    <counter type="LINE" missed="802" covered="2503" />
    <counter type="BRANCH" missed="210" covered="524" />
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

COORD-FILE TaskMaster/Ribbon/EngineToggleStateCoordinator.cs nodes=1 covered=89 valid=89 branches-covered=22 branches-valid=22
COORD-FILE TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs nodes=1 covered=72 valid=72 branches-covered=20 branches-valid=20
COORD-FILE TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs nodes=1 covered=42 valid=42 branches-covered=2 branches-valid=2
COORD-CLASS-NODES: 3

Class-node Grep read-outs (explicit file paths, git-ignored documents; pattern NODE-RATE-ONE = Messages class node with branch-rate 1, NODE-RATE-HALF = same node with branch-rate 0.5):
- coverage/remediation-964.cobertura.xml: NODE-RATE-ONE = 1, NODE-RATE-HALF = 0
- coverage/final-964.cobertura.xml (pre-change control): NODE-RATE-ONE = 0, NODE-RATE-HALF = 1

The raw Cobertura and trx documents remain under the git-ignored coverage directory and are not committed.
