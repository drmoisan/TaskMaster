# Baseline: repository test-and-coverage run (P0-T14)

Timestamp: 2026-10-03T07-41
Task: P0-T14
Command: pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1
EXIT_CODE: 0

Output Summary:
- RUNNER_EXIT_CODE: 0 (branch (a): exit 0 with both floors met)
- RAW: False
- LINE-FLOOR: MET
- BRANCH-FLOOR: MET
- First-party coverage: lines 56609/65855 (85.96%), branches 13680/17078 (80.10%)
- ROOT line-rate=0.859601 branch-rate=0.801031 lines-covered=56609 lines-valid=65855
- COORD-LINES covered=177 valid=177
- COORD-LINE-RATE: 100
- BASELINE-COORD-LINE-RATE: 100
- BASELINE-FAILED-FQN-COUNT: 0
- BASELINE-METHOD-HTC-UNCOVERED: 0
- BASELINE-METHOD-CP-UNCOVERED: 0
- Verdict: PASS (COORD-CLASS-NODES: 1; main-file row nodes=1, Prime and Messages rows nodes=0; TryInvokeSink and BuildNotifyFailedMessage ABSENT as expected at baseline).

Details:

- DISCOVERED_LINE: Discovered 9 test assemblies.
- THRESHOLD_MESSAGE: (empty)
- COLLECT_FAILURE_MESSAGE: (empty)
- DOCUMENT_PRESENT: True
- TRX_PRESENT: True
- SUMMARY_FILE_PRESENT: True
- FAILED-SET: (empty)
- TEST-DEFINITIONS: 7374
- FAILED-FQN-COUNT: 0 (no FAILED-FQN rows)

SUMMARY-BEGIN
```
Test run outcome: Completed
Total 7384, executed 7384, passed 7384, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
```
SUMMARY-END

PROJECTION-BEGIN
```
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="2293" covered="10461" />
    <counter type="BRANCH" missed="699" covered="2518" />
  </package>
  <package name="UtilitiesCS">
    <counter type="LINE" missed="4202" covered="39306" />
    <counter type="BRANCH" missed="1799" covered="9494" />
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

Coordinator rows:
```
COORD-FILE TaskMaster/Ribbon/EngineToggleStateCoordinator.cs nodes=1 covered=177 valid=177
COORD-FILE TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs nodes=0 covered=0 valid=0
COORD-FILE TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs nodes=0 covered=0 valid=0
COORD-CLASS-NODES: 1
COORD-BRANCHES covered=39 valid=40
METHOD HandleToggleClickAsync file=TaskMaster/Ribbon/EngineToggleStateCoordinator.cs span=182-205 nodes=1 elements=18 covered=18 uncovered=0 rate=100
METHOD-LINE HandleToggleClickAsync 183 hits=1
METHOD-LINE HandleToggleClickAsync 184 hits=1
METHOD-LINE HandleToggleClickAsync 185 hits=1
METHOD-LINE HandleToggleClickAsync 186 hits=1
METHOD-LINE HandleToggleClickAsync 187 hits=1
METHOD-LINE HandleToggleClickAsync 191 hits=1
METHOD-LINE HandleToggleClickAsync 192 hits=1
METHOD-LINE HandleToggleClickAsync 193 hits=1
METHOD-LINE HandleToggleClickAsync 194 hits=1
METHOD-LINE HandleToggleClickAsync 195 hits=1
METHOD-LINE HandleToggleClickAsync 197 hits=1
METHOD-LINE HandleToggleClickAsync 198 hits=1
METHOD-LINE HandleToggleClickAsync 199 hits=1
METHOD-LINE HandleToggleClickAsync 200 hits=1
METHOD-LINE HandleToggleClickAsync 201 hits=1
METHOD-LINE HandleToggleClickAsync 203 hits=1
METHOD-LINE HandleToggleClickAsync 204 hits=1
METHOD-LINE HandleToggleClickAsync 205 hits=1
METHOD CompletePrime file=TaskMaster/Ribbon/EngineToggleStateCoordinator.cs span=405-435 nodes=1 elements=20 covered=20 uncovered=0 rate=100
METHOD-LINE CompletePrime 406 hits=1
METHOD-LINE CompletePrime 407 hits=1
METHOD-LINE CompletePrime 408 hits=1
METHOD-LINE CompletePrime 409 hits=1
METHOD-LINE CompletePrime 412 hits=1
METHOD-LINE CompletePrime 413 hits=1
METHOD-LINE CompletePrime 414 hits=1
METHOD-LINE CompletePrime 420 hits=1
METHOD-LINE CompletePrime 421 hits=1
METHOD-LINE CompletePrime 422 hits=1
METHOD-LINE CompletePrime 424 hits=1
METHOD-LINE CompletePrime 425 hits=1
METHOD-LINE CompletePrime 426 hits=1
METHOD-LINE CompletePrime 427 hits=1
METHOD-LINE CompletePrime 428 hits=1
METHOD-LINE CompletePrime 429 hits=1
METHOD-LINE CompletePrime 431 hits=1
METHOD-LINE CompletePrime 432 hits=1
METHOD-LINE CompletePrime 434 hits=1
METHOD-LINE CompletePrime 435 hits=1
METHOD TryInvokeSink ABSENT
METHOD BuildNotifyFailedMessage ABSENT
```

The raw documents `coverage\baseline-964.cobertura.xml` and `coverage\baseline-964.trx` stay on disk under the git-ignored coverage directory.
