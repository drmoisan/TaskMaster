# quickfiler-tests-depend-on-wall-clock-timing (Issue #950)

- Date captured: 2026-09-30
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-tests-depend-on-wall-clock-timing/ (Issue #950)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #950
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/950
- Last Updated: 2026-09-30
## Summary

Several QuickFiler.Test tests fail intermittently under load because they wait on real wall-clock time. Seen during parallel run bugs-2026-09-28:
- `QfcDatamodelLivenessTests`, including `RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces`. Two failures stopped #944's P3-T8 gate, and one failure occurred during #929's local run.
- `QfcItemController.UiThreadDispatcherFixtureTests.Transaction_SecondCallerCannotInstallUntilTheFirstRestores`, which failed once in CI on an earlier #929 head.

## Environment

- OS/version: Windows 11 (local, with concurrent coverage runs) and windows-latest (CI)
- Python version: n/a (C# / MSTest, parallel Workers=0, Scope=ClassLevel)
- Command/flags used: `Invoke-MSTestWithCoverage.ps1`, and the CI MSTest-with-coverage required check
- Data source or fixture: n/a

## Steps to Reproduce

1. Run QuickFiler.Test under the parallel regime while the machine is loaded, for example with a second coverage run.
2. Observe intermittent failures in the tests named above.

## Expected Behavior

The tests are deterministic regardless of machine load.

## Actual Behavior

- `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs` uses `SpinWait.SpinUntil(condition, TimeSpan.FromSeconds(5))` (line ~56) and `.Task.Wait(TimeSpan.FromSeconds(5))` (lines ~103 and ~173). A slow scheduler turns these into assertion failures.
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs:206` fails intermittently. Its cause is not yet established.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: #944 evidence for P3-T8, first run; #929 PR #949 CI history.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

Real-time bounded waits violate the determinism rules: no wall-clock waits in tests, and use of `FakeTimeProvider` or a controllable scheduler. They hit a required check. The transaction test may be a different root cause. Triage it first, and split it into its own issue if its cause is not timing.

## Proposed Fix / Validation Ideas

- [ ] Replace the bounded waits with deterministic completion signals (`TaskCompletionSource` or awaited handles) or an injected `TimeProvider`.
- [ ] Use no retries, `[DoNotParallelize]`, Workers=1 or longer timeouts, and find any raced static state.
- [ ] Negative control: show that each rewritten test fails when the awaited signal is never set.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
