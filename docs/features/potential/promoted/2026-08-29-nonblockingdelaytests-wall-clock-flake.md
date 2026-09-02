# nonblockingdelaytests-wall-clock-flake (Issue #694)

- Date captured: 2026-08-29
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/nonblockingdelaytests-wall-clock-flake/ (Issue #694)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #694
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/694
- Last Updated: 2026-08-29
## Summary

`NonBlockingDelayTests.WaitAsync_WithNoDispatcher_CompletesAfterInterval` in `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs` waits on a real wall-clock interval instead of a fake/virtual clock, violating this repository's own determinism rule (`.claude/rules/general-unit-test.md`: no `Thread.Sleep`/`Task.Delay`/real wall-clock waits in tests; async tests must use a fake-timer/`FakeTimeProvider` facility).

## Environment

- OS/version: Windows, MSTest
- Command/flags used: `vstest.console.exe` against `TaskMaster.Test.dll`
- Data source or fixture: `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs`

## Steps to Reproduce

1. Run `NonBlockingDelayTests.WaitAsync_WithNoDispatcher_CompletesAfterInterval` repeatedly, or under load/CI contention.
2. Observe the test waits on a real wall-clock interval to elapse rather than advancing a fake/virtual timer.
3. Under scheduler contention or a slow CI runner, the wait can complete later or less predictably than intended, producing flaky pass/fail behavior.

## Expected Behavior

The test should use a controllable clock or fake-timer facility (per `.claude/rules/general-unit-test.md`'s Determinism Infrastructure section) so elapsed-time behavior is simulated deterministically rather than waiting on the real clock.

## Actual Behavior

The test performs a real wall-clock wait, which is a banned pattern under this repository's determinism rules and is a source of flakiness under load.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: parallel-run bugs-635-440 final report: "`NonBlockingDelayTests.WaitAsync_WithNoDispatcher_CompletesAfterInterval` is a wall-clock flake violating the repo's own determinism rule; fix patterns exist at `d208fa68` and `2b8ff3ef`."

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

## Suspected Cause / Notes

The test predates, or was not updated to use, an injectable clock/`FakeTimeProvider` seam for the delay path under test. Two prior commits in this repository (`d208fa68` and `2b8ff3ef`) already establish the fix pattern for equivalent wall-clock-wait defects and can likely be adapted directly.

## Proposed Fix / Validation Ideas

- [ ] Apply the fix pattern from commits `d208fa68` and `2b8ff3ef` to route this test's timing through a fake/virtual clock
- [ ] Confirm the test still exercises the "no dispatcher" code path meaningfully after the clock is faked
- [ ] Run the test repeatedly (e.g. in a tight loop) before and after the fix to confirm the flake source is eliminated

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
