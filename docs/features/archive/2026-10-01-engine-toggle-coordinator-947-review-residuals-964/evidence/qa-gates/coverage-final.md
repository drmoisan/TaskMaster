# Final: repository test-and-coverage run (P2-T5, pass 1)

Timestamp: 2026-10-03T08-13
Task: P2-T5
Command: pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1
EXIT_CODE: 0

Output Summary:
- RUNNER_EXIT_CODE: 0 (D-8 test-step rule: exit 0)
- RAW: False
- LINE-FLOOR: MET
- BRANCH-FLOOR: MET
- First-party coverage: lines 56629/65881 (85.96%), branches 13683/17082 (80.10%)
- ROOT line-rate=0.859565 branch-rate=0.801019 lines-covered=56629 lines-valid=65881
- COORD-LINES covered=203 valid=203
- COORD-LINE-RATE: 100
- FINAL-FAILED-FQN-COUNT: 0 (no FAILED-FQN rows)
- Verdict: PASS (COORD-CLASS-NODES: 3; every COORD-FILE row nodes=1; no PARTIAL CLASS ATTRIBUTION UNSUPPORTED, no NEW FAILING TEST).

Details:

- Pre-run check: STRAY_TEST_PROCESSES: 0.
- DISCOVERED_LINE: Discovered 9 test assemblies.
- FIRST_PARTY_LINE: First-party coverage: lines 56629/65881 (85.96%), branches 13683/17082 (80.10%)
- THRESHOLD_MESSAGE: (empty)
- COLLECT_FAILURE_MESSAGE: (empty)
- DOCUMENT_PRESENT: True
- TRX_PRESENT: True
- SUMMARY_FILE_PRESENT: True
- FAILED-SET: (empty)
- TEST-DEFINITIONS: 7378
- FAILED-FQN-COUNT: 0 (no FAILED-FQN rows)

SUMMARY-BEGIN
```
Test run outcome: Completed
Total 7388, executed 7388, passed 7388, failed 0.
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
    <counter type="LINE" missed="4208" covered="39300" />
    <counter type="BRANCH" missed="1800" covered="9493" />
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
    <counter type="BRANCH" missed="211" covered="523" />
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
COORD-FILE TaskMaster/Ribbon/EngineToggleStateCoordinator.cs nodes=1 covered=89 valid=89
COORD-FILE TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs nodes=1 covered=72 valid=72
COORD-FILE TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs nodes=1 covered=42 valid=42
COORD-CLASS-NODES: 3
COORD-LINES covered=203 valid=203
COORD-BRANCHES covered=43 valid=44
COORD-LINE-RATE: 100
METHOD HandleToggleClickAsync file=TaskMaster/Ribbon/EngineToggleStateCoordinator.cs span=184-212 nodes=1 elements=24 covered=24 uncovered=0 rate=100
METHOD-LINE HandleToggleClickAsync 185 hits=1
METHOD-LINE HandleToggleClickAsync 186 hits=1
METHOD-LINE HandleToggleClickAsync 187 hits=1
METHOD-LINE HandleToggleClickAsync 188 hits=1
METHOD-LINE HandleToggleClickAsync 189 hits=1
METHOD-LINE HandleToggleClickAsync 190 hits=1
METHOD-LINE HandleToggleClickAsync 191 hits=1
METHOD-LINE HandleToggleClickAsync 192 hits=1
METHOD-LINE HandleToggleClickAsync 193 hits=1
METHOD-LINE HandleToggleClickAsync 194 hits=1
METHOD-LINE HandleToggleClickAsync 195 hits=1
METHOD-LINE HandleToggleClickAsync 196 hits=1
METHOD-LINE HandleToggleClickAsync 197 hits=1
METHOD-LINE HandleToggleClickAsync 198 hits=1
METHOD-LINE HandleToggleClickAsync 199 hits=1
METHOD-LINE HandleToggleClickAsync 201 hits=1
METHOD-LINE HandleToggleClickAsync 205 hits=1
METHOD-LINE HandleToggleClickAsync 206 hits=1
METHOD-LINE HandleToggleClickAsync 207 hits=1
METHOD-LINE HandleToggleClickAsync 208 hits=1
METHOD-LINE HandleToggleClickAsync 209 hits=1
METHOD-LINE HandleToggleClickAsync 210 hits=1
METHOD-LINE HandleToggleClickAsync 211 hits=1
METHOD-LINE HandleToggleClickAsync 212 hits=1
METHOD CompletePrime file=TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs span=165-195 nodes=1 elements=22 covered=22 uncovered=0 rate=100
METHOD-LINE CompletePrime 166 hits=1
METHOD-LINE CompletePrime 167 hits=1
METHOD-LINE CompletePrime 168 hits=1
METHOD-LINE CompletePrime 169 hits=1
METHOD-LINE CompletePrime 172 hits=1
METHOD-LINE CompletePrime 173 hits=1
METHOD-LINE CompletePrime 174 hits=1
METHOD-LINE CompletePrime 180 hits=1
METHOD-LINE CompletePrime 181 hits=1
METHOD-LINE CompletePrime 182 hits=1
METHOD-LINE CompletePrime 183 hits=1
METHOD-LINE CompletePrime 184 hits=1
METHOD-LINE CompletePrime 185 hits=1
METHOD-LINE CompletePrime 186 hits=1
METHOD-LINE CompletePrime 187 hits=1
METHOD-LINE CompletePrime 188 hits=1
METHOD-LINE CompletePrime 189 hits=1
METHOD-LINE CompletePrime 190 hits=1
METHOD-LINE CompletePrime 191 hits=1
METHOD-LINE CompletePrime 192 hits=1
METHOD-LINE CompletePrime 194 hits=1
METHOD-LINE CompletePrime 195 hits=1
METHOD TryInvokeSink file=TaskMaster/Ribbon/EngineToggleStateCoordinator.cs span=287-300 nodes=1 elements=10 covered=10 uncovered=0 rate=100
METHOD-LINE TryInvokeSink 288 hits=1
METHOD-LINE TryInvokeSink 290 hits=1
METHOD-LINE TryInvokeSink 291 hits=1
METHOD-LINE TryInvokeSink 292 hits=1
METHOD-LINE TryInvokeSink 293 hits=1
METHOD-LINE TryInvokeSink 295 hits=1
METHOD-LINE TryInvokeSink 296 hits=1
METHOD-LINE TryInvokeSink 297 hits=1
METHOD-LINE TryInvokeSink 298 hits=1
METHOD-LINE TryInvokeSink 300 hits=1
METHOD BuildNotifyFailedMessage file=TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs span=50-58 nodes=1 elements=8 covered=8 uncovered=0 rate=100
METHOD-LINE BuildNotifyFailedMessage 51 hits=1
METHOD-LINE BuildNotifyFailedMessage 52 hits=1
METHOD-LINE BuildNotifyFailedMessage 53 hits=1
METHOD-LINE BuildNotifyFailedMessage 54 hits=1
METHOD-LINE BuildNotifyFailedMessage 55 hits=1
METHOD-LINE BuildNotifyFailedMessage 56 hits=1
METHOD-LINE BuildNotifyFailedMessage 57 hits=1
METHOD-LINE BuildNotifyFailedMessage 58 hits=1
```

The raw documents `coverage\final-964.cobertura.xml` and `coverage\final-964.trx` stay on disk under the git-ignored coverage directory.
