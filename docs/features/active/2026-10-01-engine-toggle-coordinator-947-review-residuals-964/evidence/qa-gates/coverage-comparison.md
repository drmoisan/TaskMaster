# Coverage Comparison (P2-T6, AC8)

Timestamp: 2026-10-03T08-13
Task: P2-T6
Command: Read FEATURE/evidence/baseline/coverage-baseline.md and FEATURE/evidence/qa-gates/coverage-final.md (Read tool) and compare the recorded figures
EXIT_CODE: 0

Output Summary:
- BASELINE-FIRST-PARTY: lines 56609/65855 (85.96%), branches 13680/17078 (80.10%)
- FINAL-FIRST-PARTY: lines 56629/65881 (85.96%), branches 13683/17082 (80.10%)
- BASELINE-COORD-LINES: covered=177 valid=177 (one file)
- FINAL-COORD-LINES: covered=203 valid=203 (three files: main 89/89, Prime 72/72, Messages 42/42)
- BASELINE-COORD-LINE-RATE: 100
- FINAL-COORD-LINE-RATE: 100
- METHOD HandleToggleClickAsync file=TaskMaster/Ribbon/EngineToggleStateCoordinator.cs span=184-212 nodes=1 elements=24 covered=24 uncovered=0 rate=100
- METHOD CompletePrime file=TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs span=165-195 nodes=1 elements=22 covered=22 uncovered=0 rate=100
- METHOD TryInvokeSink file=TaskMaster/Ribbon/EngineToggleStateCoordinator.cs span=287-300 nodes=1 elements=10 covered=10 uncovered=0 rate=100
- METHOD BuildNotifyFailedMessage file=TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs span=50-58 nodes=1 elements=8 covered=8 uncovered=0 rate=100
- NEW-CODE-COVERAGE: TryInvokeSink 100; BuildNotifyFailedMessage 100
- Verdict: PASS (every clause MET; no COORDINATOR COVERAGE LOWERED).

Clauses:
- FINAL-COORD-LINE-RATE (100) at least BASELINE-COORD-LINE-RATE (100): MET
- METHOD TryInvokeSink rate (100) at least 90.00: MET
- METHOD BuildNotifyFailedMessage rate (100) at least 90.00: MET
- METHOD HandleToggleClickAsync uncovered (0) at most BASELINE-METHOD-HTC-UNCOVERED (0): MET
- METHOD CompletePrime uncovered (0) at most BASELINE-METHOD-CP-UNCOVERED (0): MET
- Final line floor (85.96% against 80%): MET (LINE-FLOOR: MET in coverage-final.md)
- Final branch floor (80.10% against 75%): MET (BRANCH-FLOOR: MET in coverage-final.md)

Observation (not a gate): the `UtilitiesCS` package line count moved from 39306 covered / 4202 missed at baseline to 39300 covered / 4208 missed at final, and its branch count from 9494 / 1799 to 9493 / 1800. This item changed no file under UtilitiesCS; the movement is run-to-run variance in code outside the Write Set. The `TaskMaster` package moved from 2477 to 2503 covered lines with 802 missed at both stages.
