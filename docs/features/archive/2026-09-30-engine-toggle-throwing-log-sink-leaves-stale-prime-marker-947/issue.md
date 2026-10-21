# engine-toggle-throwing-log-sink-leaves-stale-prime-marker (Issue #947)

- Date captured: 2026-09-30
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/engine-toggle-throwing-log-sink-leaves-stale-prime-marker/ (Issue #947)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #947
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/947
- Last Updated: 2026-10-01
- Work Mode: minor-audit

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

## Scope Consolidation

- Source: maintainer comment by drmoisan on https://github.com/drmoisan/TaskMaster/issues/947, posted 2026-10-01T15:57:04Z, recorded here on 2026-10-01.
- Comment text: "Same root cause, second call site, found during #947 preparation and consolidated here instead of filed separately. `HandleToggleClickAsync` in `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` calls `_logError` without a guard (line ~184). A sink that throws can therefore escape a method documented as never throwing. The fix for #947 should cover both sites: `CompletePrime` and `HandleToggleClickAsync`. Each site needs its own regression test using a throwing sink."
- Effect on this issue: the sixth and seventh acceptance criteria below cover the second call site. The first five criteria are unchanged.

## Acceptance Criteria

- [x] When the `logError` sink throws while `CompletePrime` reports a faulted or canceled prime, the engine's prime marker is still removed, so a later `GetPressed` for the same engine starts a new prime (`EngineActiveAsync` is invoked a second time).
- [x] The report-then-clear ordering in `CompletePrime` is preserved: the sink is invoked before the marker is removed (the clear is not reordered ahead of the report), and the existing tests in `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs` pass without modification.
- [x] A throwing `logError` sink leaves no faulted task unobserved: the sink exception is contained inside `CompletePrime`, so the prime continuation has no remaining throw source, and a deterministic test asserts that the task returned by `GetPrimeTask` for the first prime ends in `RanToCompletion` (not `Faulted`) after the sink has thrown.
- [x] A regression test reproducing the Steps to Reproduce fails on the pre-fix code and passes after the fix, with the failing run recorded under the feature folder's `evidence/regression-testing/`.
- [x] New tests use MSTest, Moq, and FluentAssertions, create no temporary files, and use no `Thread.Sleep` or `Task.Delay`; the C# toolchain (CSharpier, analyzers, nullable type-check, MSTest with coverage) passes, and the changed lines in `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` are covered.
- [x] When the `logError` sink throws while `HandleToggleClickAsync` reports a faulted toggle, `HandleToggleClickAsync` does not throw and still attempts the report: the sink exception is contained inside the click boundary by its own guarded sink call, the sink receives the toggle fault unchanged, and no control is invalidated.
- [x] A separate regression test for the `HandleToggleClickAsync` call site, using a throwing sink on a faulted toggle, fails on the pre-fix code and passes after the fix, with the failing run recorded under the feature folder's `evidence/regression-testing/`.

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
