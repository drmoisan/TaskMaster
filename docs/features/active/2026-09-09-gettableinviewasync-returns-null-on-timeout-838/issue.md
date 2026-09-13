# gettableinviewasync-returns-null-on-timeout (Issue #838)

- Date captured: 2026-09-09
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/gettableinviewasync-returns-null-on-timeout/ (Issue #838)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #838
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/838
- Last Updated: 2026-09-10
- Work Mode: full-bug

## Summary

`GetTableInViewAsync` returns null to its caller on the ordinary timeout path instead of throwing, so a timed-out table read is indistinguishable from a successful empty read.

## Environment

- OS/version: Windows 11, Outlook VSTO host
- Python version: n/a (C#, net48)
- Command/flags used: n/a, runtime path
- Data source or fixture: `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`

## Steps to Reproduce

1. Call `GetTableInViewAsync` against a folder whose table read exceeds the 2000 ms deadline.
2. Observe that `TimeOutTask.RunWithTimeout` is invoked with `maxAttempts: 1` and `strict: false`.
3. Observe the value the method hands back to its caller.

## Expected Behavior

A timeout surfaces as an exception, or as an explicit sentinel the caller is contractually required to handle. Failure is not silently indistinguishable from success.

## Actual Behavior

On the ordinary timeout path `RunWithTimeout` absorbs the `TaskCanceledException`, exhausts its internal retry, and returns `default(TResult)` without throwing. Neither catch block in `GetTableInViewAsync` is entered, neither retry recursion runs, and the method returns `table!` as null. The null-forgiving operator makes the null invisible to nullable analysis at the boundary.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: none. The defect is the absence of any signal.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

Same null-through-suppression class as issue 825 item 3, at a different site, and already acknowledged by a comment inside the method. Surfaced during preparation of feature 825 in the review-residuals-2026-09-08 epic and recorded as spec AC35. It was left out of that feature's blast radius deliberately.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: `RunWithTimeout` behaviour under `strict: false` with `maxAttempts: 1`, and the `GetTableInViewAsync` timeout path
- [ ] Integration scenario to retest: table read against a slow or unresponsive store
- [ ] Manual verification notes: confirm every caller of `GetTableInViewAsync` handles the chosen failure contract

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

## Delivery Note (2026-09-12)

### Delivered failure contract

`GetTableInViewAsync` no longer returns null on any failure route. It now reports each failure as an
exception, and the method carries XML documentation stating the contract:

- `TimeoutException` when the acquisition deadline is exhausted. This covers three routes: the
  retry-ceiling branch of the `TaskCanceledException` catch, the retry-ceiling branch of the
  `TimeoutException` catch, and the final guard reached when the shared time-out helper absorbed its
  own retry budget and returned its default value. The message names the retry number and the
  millisecond budget, and the originating exception is carried as the inner exception on the two
  catch routes.
- `OperationCanceledException` when the caller's token is cancelled. The cancellation branch of the
  `TaskCanceledException` catch rethrows, and the final guard checks the caller's token before it
  reports a timeout, so cancellation is never relabelled as a timeout. No catch clause for the base
  cancellation type was added.
- `InvalidOperationException` when the explorer's current view is not a table view, which is
  unchanged behaviour.

The null-forgiving suppression is gone from the return statement. The exception is constructed by a
private helper in a new partial file that returns the exception rather than throwing it, which keeps
definite-assignment and nullable flow analysis correct without a does-not-return attribute that the
net48 base class library does not provide.

Five new failure-contract tests cover the routes, all passing, and the pre-existing cancellation test
`GetTableInViewAsync_CanceledToken_PropagatesOperationCanceledException` still passes. No existing
test assertion was weakened, deleted or relaxed.

CODE-COMMIT-SHA: 3d680a4fcc498833bda95429331677b6c08d3e01

### Acceptance Criteria Status

- Source: `docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/spec.md`, section `## Acceptance Criteria`
- Total AC items: 16
- Checked off (delivered): 16
- Remaining (unchecked): 0
- Items remaining: none
