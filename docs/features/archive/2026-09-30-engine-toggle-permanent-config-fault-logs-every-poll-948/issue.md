# engine-toggle-permanent-config-fault-logs-every-poll (Issue #948)

- Date captured: 2026-09-30
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll-948/ (Issue #948)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #948
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/948
- Last Updated: 2026-10-01
- Work Mode: full-bug

## Summary

When the engine configuration load faults permanently, `AsyncLazy` caches the fault, so every cache-miss `getPressed` poll re-primes and logs the same error again. A stale marker used to suppress repeats intermittently. Once #944 removes the stale marker, every poll that reaches `StartPrimeIfNeeded` logs.

## Environment

- OS/version: Windows 11 (Outlook VSTO add-in)
- Python version: n/a (C#, .NET Framework 4.8)
- Command/flags used: n/a
- Data source or fixture: `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, and the configuration `AsyncLazy` with `ResetConfigAsyncLazy`

## Steps to Reproduce

1. Cause the engine configuration load to fault persistently.
2. Let the ribbon repeatedly invalidate or poll the engine toggle `getPressed`.
3. Observe the log file (`TaskMaster\bin\Debug\logs\debug_<date>.log`).

## Expected Behavior

A permanent configuration fault is logged once, or at a bounded rate, and recovery is either explicit or backed off.

## Actual Behavior

After #944, one error is logged per cache-miss poll, without bound.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: #944 spec, Rollout and Follow-up item 2; #944 research, follow-up item 2.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

## Suspected Cause / Notes

The fault is cached in `AsyncLazy` with no back-off or reset policy in the coordinator. This is a design decision about the retry policy, not a correctness defect in #944.

## Proposed Fix / Validation Ideas

- [ ] Decide on a policy: back off re-primes after a fault, log only on a state change, or call `ResetConfigAsyncLazy` to recover.
- [ ] Deterministic test: under a fake `TimeProvider`, N polls against a cached fault produce a bounded number of log entries.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
