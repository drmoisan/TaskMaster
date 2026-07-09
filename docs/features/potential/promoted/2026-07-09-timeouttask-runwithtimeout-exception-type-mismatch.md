# timeouttask-runwithtimeout-exception-type-mismatch (Issue #285)

- Date captured: 2026-07-09
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/timeouttask-runwithtimeout-exception-type-mismatch/ (Issue #285)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #285
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/285
- Last Updated: 2026-07-09
## Summary

`TimeOutTask.RunWithTimeout<T1, TResult>` (`UtilitiesCS/Threading/TimeOutTask.cs:164-199`) catches `TimeoutException` in its retry/timeout handling ladder, but the wrapped call is actually cancelled via a `CancellationTokenSource`-driven timeout, which surfaces as `TaskCanceledException`, not `TimeoutException`. The catch clause can never match a genuine timer-driven timeout in this overload.

## Environment

- OS/version: n/a (pure C#/.NET Framework logic defect, reproducible on any platform)
- Python version: n/a
- Command/flags used: n/a
- Data source or fixture: `UtilitiesCS/Threading/TimeOutTask.cs`

## Steps to Reproduce

1. Call `RunWithTimeout<T1, TResult>` (the `Func<T1, TResult>` overload declared at `TimeOutTask.cs:164`) with a `milliseconds` value short enough that the internal `CancellationTokenSource`-linked `Task.Run` is cancelled by the timer before the delegate completes.
2. Observe that the timer-driven cancellation raises `System.Threading.Tasks.TaskCanceledException`, not `System.TimeoutException`.
3. The `catch (TimeoutException)` clause at line 199 does not match, so the intended retry/timeout-handling branch never executes for a real timeout; the exception instead propagates unhandled to the caller.
4. Contrast with every sibling overload in the same file, which correctly catches `TaskCanceledException` for the identical `CancellationTokenSource`-driven pattern: `RunWithTimeout<TResult>` (`Func<TResult>` overload) at line 64, `RunWithTimeout<TResult>` (`Func<CancellationToken,Task<TResult>>` overload) at line 129, `RunWithTimeout<T1,T2,TResult>` at line 350, `RunWithTimeout<T1,T2,T3,TResult>` at line 580.

## Expected Behavior

A genuine timer-driven timeout in `RunWithTimeout<T1, TResult>` should be caught and handled by the same retry ladder as every sibling overload (`catch (TaskCanceledException)`), so `maxAttempts`/`strict` retry behavior applies consistently across all `RunWithTimeout` overloads.

## Actual Behavior

`catch (TimeoutException)` at line 199 only matches a `TimeoutException` thrown directly by the wrapped delegate itself (an unusual, non-default case). It never matches the actual timer-driven `TaskCanceledException` that `Task.Run(..., combinedToken)` raises when the internal `CancellationTokenSource` fires. `TaskCanceledException` and `TimeoutException` both derive independently from `SystemException` and are unrelated types. As a result, the overload's own existing test coverage simulates "timeout" by having the wrapped delegate throw `TimeoutException` directly, rather than via a genuinely short `milliseconds` value — masking the defect in the existing test suite.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: Confirmed directly against source at `UtilitiesCS/Threading/TimeOutTask.cs:164` (declaration) and `:199` (`catch (TimeoutException)`), compared against sibling `catch (TaskCanceledException)` clauses at lines 64, 129, 350, 580. Originally identified and documented in `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/research/2026-07-07T13-00-onedrive-writer-timeout-research.md` (Section 2.1) and `docs/features/archive/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/other/follow-up-issue-note.2026-07-07T14-05.md`, both of which explicitly deferred this defect as out of scope for issue #253 and recommended filing a new issue. No open GitHub issue currently references `TimeOutTask` (verified via `gh issue list --search "TimeOutTask"`).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

The retry/timeout-handling contract for this one overload silently does not work for its primary intended case (a real timer-driven timeout); callers relying on `maxAttempts`/`strict` retry-on-timeout behavior for `RunWithTimeout<T1, TResult>` get an unhandled `TaskCanceledException` instead of a retry.

## Suspected Cause / Notes

- A prior proposed production fix (changing line 199 to `catch (TaskCanceledException)`) was shown, during issue #253's research, to break other existing `TimeOutTask` retry tests — confirming this requires its own dedicated investigation and test updates, not a one-line swap.
- Two other `catch (TimeoutException)` clauses exist elsewhere in the file (lines 817, 907); this entry is scoped only to the confirmed sibling-inconsistent case at line 199 (the `RunWithTimeout<T1, TResult>` overload) per the original research citation. The other two sites were not part of the original citation and should be re-examined for the same pattern during triage, not assumed identical.

## Proposed Fix / Validation Ideas

- [ ] Add a test that exercises a genuinely short `milliseconds` value (not a directly-thrown `TimeoutException`) against `RunWithTimeout<T1, TResult>` to prove the current defect (test should fail before fix).
- [ ] Change the catch clause at line 199 to `catch (TaskCanceledException)` to match all four sibling overloads, then re-run the full `TimeOutTask` test suite and fix any retry-test regressions surfaced (per the prior investigation's finding that this swap breaks existing tests).
- [ ] Re-audit the other two `catch (TimeoutException)` sites (lines 817, 907) for the same sibling-inconsistency pattern.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
