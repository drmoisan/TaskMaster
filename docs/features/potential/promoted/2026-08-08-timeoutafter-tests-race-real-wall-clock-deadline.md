# timeoutafter-tests-race-real-wall-clock-deadline (Issue #516)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/timeoutafter-tests-race-real-wall-clock-deadline/ (Issue #516)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #516
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/516
- Last Updated: 2026-08-08
## Summary

`UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs` contains tests that race a real 100 ms wall-clock deadline against exception propagation. On a loaded CI runner the deadline can win, so the test observes `TimeoutException` instead of the expected `InvalidOperationException` and fails. This is a flaky test, not a product defect, and it fails CI on unrelated pull requests.

## Environment

- OS/version: `windows-latest` GitHub Actions runner (also reproducible in principle on any loaded machine)
- Runtime: .NET Framework 4.8.1, MSTest via `vstest.console.exe`
- Command/flags used: the CI job "Format, build, analyze, and test", which runs the MSTest suite with coverage
- Data source or fixture: `UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs`

## Steps to Reproduce

1. Open any pull request that does not touch `UtilitiesCS.Test/Threading/` and let CI run the full MSTest suite.
2. Observe the run under runner load.
3. Compare against a local run of the same test on an idle machine.

Observed on 2026-08-08 in run `31275248280` for PR #514 (issue #438): `6350` total, `6349` passed, **`1` failed**.

## Expected Behavior

The test deterministically asserts that a source exception set after `TimeoutAfter` is attached propagates as `InvalidOperationException`, independent of machine load. Per `.claude/rules/general-unit-test.md`, tests must be deterministic and **real wall-clock waits are a banned API in test code**.

## Actual Behavior

`TimeoutAfter_GenericTask_ShouldPropagateFaultedSourceException_WhenSourceFaultsLater` failed on the CI runner with:

```
System.TimeoutException: The operation has timed out.
```

The same test passes locally on an idle machine on the same commit, confirming a load-dependent race rather than a functional regression.

The test attaches a 100 ms timeout and then sets the exception:

```csharp
var source = new TaskCompletionSource<int>();
var proxy = source.Task.TimeoutAfter(100);
source.SetException(new InvalidOperationException("boom"));
Func<Task> act = async () => await proxy;
await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
```

If the runner does not schedule the continuation within 100 ms, the timeout path completes first and the assertion sees the wrong exception type.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: `Failed TimeoutAfter_GenericTask_ShouldPropagateFaultedSourceException_WhenSourceFaultsLater [174 ms]` / `Error Message: System.TimeoutException: The operation has timed out.` Full log: https://github.com/drmoisan/TaskMaster/actions/runs/31275248280/job/93147648583

Note the reported duration of 174 ms against a 100 ms deadline, which is the race made visible.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

This fails CI on pull requests that have nothing to do with the code under test, and the only available remedy today is to re-run the job. That is corrosive in two ways: it blocks unrelated work, and it normalizes re-running a red suite until it turns green — the exact habit that allows a genuine regression to be waved through. The sibling test `TimeoutAfter_NonGenericTask_ShouldPropagateFaultedSourceException_WhenSourceFaultsLater` has the identical structure and the same latent failure mode.

## Suspected Cause / Notes

Observed during the CI gate for PR #514 (issue #438) on 2026-08-08.

- The test's correctness depends on the continuation being scheduled within a real 100 ms window. Under CI load that assumption does not hold.
- The behavior under test — that a later source fault propagates rather than being masked by the timeout — is a genuine and worthwhile assertion. Only the timing mechanism is at fault.
- Same root-cause family as issue **#511** (`WinFormsPumpHost` load-flaky tests), which also covers coverage-measurement nondeterminism. Consider handling them together as a determinism cleanup.
- Both tests in this file with the `WhenSourceFaultsLater` suffix should be fixed together.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: remove the wall-clock dependency. Inject the delay mechanism behind a seam so the test can control it deterministically — `.claude/rules/general-unit-test.md` calls for a controllable clock (`TimeProvider`, with `FakeTimeProvider` in tests), and `.claude/rules/csharp.md` prefers an interface seam. Where `TimeoutAfter` cannot currently accept an injected timer, add that seam rather than lengthening the timeout.
- [ ] Integration scenario to retest: run the full suite repeatedly under induced CPU load and confirm a stable pass rate for both `WhenSourceFaultsLater` tests.
- [ ] Manual verification notes: do **not** fix this by increasing the timeout, adding a retry, or adding a sleep. `.claude/rules/csharp.md` prohibits masking flaky behavior with sleeps, retries, or timing hacks; a larger constant only makes the race rarer and harder to diagnose.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
