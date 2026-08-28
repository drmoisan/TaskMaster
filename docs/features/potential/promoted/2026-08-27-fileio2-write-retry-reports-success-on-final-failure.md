# fileio2-write-retry-reports-success-on-final-failure (Issue #647)

- Date captured: 2026-08-27
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/fileio2-write-retry-reports-success-on-final-failure/ (Issue #647)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #647
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/647
- Last Updated: 2026-08-27
## Summary

`FileIO2.WriteTextFileAsync` in `UtilitiesCS/To Depricate/FileIO2.cs` retries on `IOException` up to
100 times with a 100 millisecond delay between attempts, roughly a ten-second bounded window. When
the final attempt still fails it logs the exception and then sets its success flag to `true` and
returns, so the caller cannot distinguish a completed write from a write that never happened.

Two consequences:

1. **A persistently failed write is silent.** Any caller that awaits this method and treats normal
   return as success is wrong, and there is no return value or exception that would let it behave
   otherwise.
2. **The retry window is not cancellable.** The loop's delay does not observe a `CancellationToken`,
   so a caller that awaits the method while the target file is locked is stalled for the whole
   bounded window regardless of what its own token does.

The second consequence became reachable in a new place through issue #442. `QfcHomeController.WriteMetricsAsync`
now awaits this writer directly, and it deliberately passes `CancellationToken.None` so that a
session cancellation cannot destroy the metrics write. That choice is correct for its own purpose,
but it means a locked session-metrics file stalls the awaiting continuation for the full window with
no cancellation path.

`FileIO2.cs` was **not** modified by #442 and is outside that feature's owned files. This is recorded
as a pre-existing defect in a module already marked for deprecation, surfaced by that work rather
than caused by it. Feature-review raised it as finding CR-2 (Minor, pre-existing, non-blocking) and
explicitly recommended the promotion lifecycle rather than an in-scope fix.

## Environment

- OS/version: Windows 11, Outlook VSTO add-in host
- Python version: not applicable (C# / .NET Framework 4.8)
- Command/flags used: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`
- Data source or fixture: any file held open exclusively by another process while the write is attempted

## Steps to Reproduce

1. Open the intended target file exclusively in another process and keep the handle.
2. Call `FileIO2.WriteTextFileAsync` against that path.
3. Wait for the retry window to expire, then observe the method's return and the file's contents.

## Expected Behavior

Exhausting the retry budget is a failure and must be reported as one: either by throwing, or by
returning a result the caller can inspect. The retry delay should also observe a supplied
`CancellationToken` so a caller can abandon the attempt.

## Actual Behavior

The method logs and returns normally. The caller has no way to learn the write did not happen, and
the delay is uncancellable for the duration of the window.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet: the retry loop sits at `UtilitiesCS/To Depricate/FileIO2.cs:50-89`; the final-failure path
  logs the exception and then assigns the success flag `true` before returning.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium. Silent data loss on a genuinely contended file, and an uncancellable multi-second stall in
any `await` path that reaches it. Both are bounded and neither is reachable from unit tests, because
every current in-repo caller of consequence writes through an injectable seam that tests substitute.

## Suspected Cause / Notes

The success flag appears to have been intended as "stop retrying" rather than "the write succeeded",
and the two meanings were conflated. The module lives under `UtilitiesCS/To Depricate/`, which
suggests the defect has survived because the file is slated for removal rather than repair.

Raised as finding CR-2 in
`docs/features/active/quickfiler-home-controller-metrics-442/code-review.2026-08-27T14-35.md`.

Related: `UtilitiesCS.Test.HelperClasses.FileIO2_Tests` already contains
`WriteTextFileAsync_WhenTargetIsLocked_ShouldRetryAndExitWithoutThrowing`, whose name records the
current contract as intentional. Fixing this defect requires deciding whether that contract is the
one wanted, and updating that test accordingly.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: decide the contract first. If failure must surface, change the signature
  to return a success indicator, or throw after the budget is exhausted, and update
  `WriteTextFileAsync_WhenTargetIsLocked_ShouldRetryAndExitWithoutThrowing` to assert the new
  contract. Add a `CancellationToken` parameter that the retry delay observes, and a test that
  cancels mid-window and asserts prompt return.
- [x] Integration scenario to retest: every in-repo caller of `WriteTextFileAsync` must be reviewed
  for how it should react to a reported failure, including
  `QfcHomeController.WriteMetricsAsync`, which currently passes `CancellationToken.None` deliberately
  and would need to keep doing so while still learning about failure.
- [x] Manual verification notes: banned-API check — the existing loop uses `Task.Delay`, which is
  prohibited in test code by `.claude/rules/general-unit-test.md`; any new test must drive the delay
  through an injected seam or `FakeTimeProvider` rather than a real wall-clock wait.

Consider whether the correct disposition is to fix `FileIO2` or to complete its deprecation and move
its remaining callers to a supported writer. The `To Depricate` folder placement argues for the
latter.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
