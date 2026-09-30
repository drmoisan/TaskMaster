# engine-toggle-prime-marker-registration-races-removal (Issue #944)

- Date captured: 2026-09-30
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/engine-toggle-prime-marker-registration-races-removal/ (Issue #944)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #944
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/944
- Last Updated: 2026-09-30
## Summary

In `EngineToggleStateCoordinator`, a prime that completes synchronously in a non-success state can run `CompletePrime`'s marker removal before `StartPrimeIfNeeded` registers that marker. A stale marker for a finished prime is then left in `_primeTasks`, which blocks any later re-prime for that engine. This is hazard B from the #942 research, first recorded as NB-2 in the #735 code review, and never promoted.

## Environment

- OS/version: Windows 11 / windows-latest
- Python version: n/a (C#, .NET Framework 4.8)
- Command/flags used: n/a (production code path; also reachable from the re-prime in `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse`)
- Data source or fixture: n/a

## Steps to Reproduce

1. Arrange for `EngineActiveAsync` to return an already-faulted task, for example after a cached configuration-load fault.
2. Call `GetPressed` for that engine, which triggers `StartPrimeIfNeeded`.
3. `StartObservedPrime` schedules the continuation. The continuation can run `CompletePrime` (`_primeTasks.TryRemove`, line ~348) before the assignment `_primeTasks[engineName] = ...` at line ~276 stores it.

## Expected Behavior

A finished prime never leaves a marker in `_primeTasks`, so a later `GetPressed` can start a fresh prime.

## Actual Behavior

The removal can precede the registration. The marker for the completed prime then stays registered, `ContainsKey` returns true, and no later prime starts for that engine.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: research record `docs/features/active/2026-09-29-engine-toggle-prime-fault-logging-test-races-942/research/2026-09-29T23-20-engine-toggle-prime-fault-race-research.md`, conclusion 3.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

`StartPrimeIfNeeded` holds `_primeGate` while it registers, but `CompletePrime` removes the marker without taking the gate. Registration happens after the continuation is scheduled, so a continuation that finishes quickly can remove the marker before it is written. No current test fails on this. #942 fixes a different ordering (log before removal) and leaves this one out of scope by design.

## Proposed Fix / Validation Ideas

- [ ] Write a deterministic regression test: use a pre-faulted `EngineActiveAsync` and assert that a second `GetPressed` starts a new prime. It must fail before the fix.
- [ ] Fix options: register a placeholder before scheduling, or make removal conditional on the stored task being the completing one (`TryRemove` with a `KeyValuePair` comparison), or take `_primeGate` in `CompletePrime`.
- [ ] Use no sleeps, retries, `[DoNotParallelize]` or Workers=1.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
