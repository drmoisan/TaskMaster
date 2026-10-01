# engine-toggle-throwing-log-sink-leaves-stale-prime-marker (Issue #947)

- Date captured: 2026-09-30
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/engine-toggle-throwing-log-sink-leaves-stale-prime-marker/ (Issue #947)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #947
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/947
- Last Updated: 2026-09-30
## Summary

After #942 moved `CompletePrime` to report the fault first and clear the marker second, a `logError` sink that throws skips the `_primeTasks.TryRemove`. The stale prime marker then blocks later re-primes for that engine, which is the #944 symptom by a different cause, and the continuation task faults unobserved.

## Environment

- OS/version: Windows 11 / windows-latest
- Python version: n/a (C#, .NET Framework 4.8)
- Command/flags used: n/a
- Data source or fixture: `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, `CompletePrime`

## Steps to Reproduce

1. Construct the coordinator with a `logError` delegate that throws.
2. Fault a prime, so that `EngineActiveAsync` faults.
3. Call `GetPressed` again for the same engine.

## Expected Behavior

The marker is cleared whether or not the log sink throws, a later `GetPressed` starts a new prime, and no task faults unobserved.

## Actual Behavior

The marker stays registered, no re-prime starts, and the continuation task faults unobserved.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: #944 spec, Rollout and Follow-up item 1; #944 research, follow-up item 1.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

## Suspected Cause / Notes

The report-then-clear ordering puts `_logError` before `TryRemove` with no `try`/`finally`. The production sink is `logger.Error`, so the likelihood is low, but the ordering change came from #942 and the lifecycle is the one #944 hardens.

## Proposed Fix / Validation Ideas

- [ ] Clear the marker in a `finally` after the report, while keeping the #942 guarantee that the log precedes the observable completion. Alternatively, guard the sink call.
- [ ] Regression test: a throwing sink followed by a second `GetPressed` starts a new prime. It must fail before the fix.
- [ ] Sequence this after #944 merges, because the same method is touched.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
