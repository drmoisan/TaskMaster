# Captured all-eight determinism attempt 1: non-passing hang

- Timestamp (UTC): 2026-07-27T04:24Z
- Task: P8-T66
- Command: required direct eight-assembly VSTest command with unchanged settings, `/InIsolation`, `TestCategory!=LiveOutlook`, detailed console logging, and a canonical TRX logger.
- Result: non-passing. The agent command reached its 120-second limit without a VSTest result or a TRX file.
- Process evidence before termination: agent-started `vstest.console` PID 261012 and `testhost` PID 275620 remained active for more than five minutes; `testhost` had 24.14 seconds CPU time and 310,882,304 bytes working set.
- Cleanup: the two identified agent-started processes were terminated after the wait period. No TRX was produced.

This attempt does not establish a passing all-eight run or a transient-harness classification. P8-T66 remains unchecked.
