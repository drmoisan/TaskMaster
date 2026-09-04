# tryaddvaluesasync-wall-clock-timeout-flaky (Issue #780)

- Date captured: 2026-09-04
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/tryaddvaluesasync-wall-clock-timeout-flaky/ (Issue #780)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #780
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/780
- Last Updated: 2026-09-04
## Summary

`DictionaryExtensions.TryAddValuesAsync` cancels its inner `Task.Run` after a hard-coded 500 ms wall-clock window. Under parallel test execution with coverage instrumentation the thread pool can delay the task start past that window, so `DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` fails intermittently with `TaskCanceledException` even though the operation itself completes in about 2 ms when run alone.

## Environment

- OS/version: Windows 11 Pro 10.0.26200, .NET Framework 4.8.1 test host (VSTest 18.9.0, MSTest 4.4.0)
- Python version: n/a
- Command/flags used: `vstest.console.exe UtilitiesCS.Test.dll VBFunctions.Test.dll /EnableCodeCoverage /InIsolation /Logger:trx "/TestCaseFilter:TestCategory!=LiveOutlook"` (MSTest assembly-level parallelization enabled: 24 workers, class-level scope)
- Data source or fixture: none; in-memory `ConcurrentDictionary<string,int>`

## Steps to Reproduce

1. Build the solution in Debug.
2. Run `UtilitiesCS.Test.dll` together with another assembly under `/EnableCodeCoverage` on a machine where the thread pool is saturated (24 parallel class workers).
3. Observe `TryAddValuesAsync_UpdatesExistingValue` occasionally reported as failed after roughly 20 s.
4. Re-run `/TestCaseFilter:FullyQualifiedName~DictionaryExtensions_Tests` alone: all 14 tests pass, the affected test in about 2 ms.

## Expected Behavior

`TryAddValuesAsync` returns `true` and updates the value regardless of thread-pool scheduling latency, and the unit test is deterministic under parallel execution.

## Actual Behavior

Intermittent failure (observed 2026-09-04 during PR #779 verification):

```
Test method UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue threw exception:
System.Threading.Tasks.TaskCanceledException: A task was canceled.
   at UtilitiesCS.DictionaryExtensions.<TryAddValuesAsync>d__10`2.MoveNext() in UtilitiesCS\Extensions\DictionaryExtensions.cs:line 179
```

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: see Actual Behavior. Local run summary: 4767 tests, 4766 passed, 1 failed (this test), total time 35 s; the failing test alone took 21 s.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Flaky CI failures on unrelated pull requests; production callers may receive a spurious `TaskCanceledException` under thread-pool starvation.

## Suspected Cause / Notes

- `UtilitiesCS/Extensions/DictionaryExtensions.cs` lines 169-180: `linkedTS.CancelAfter(500)` applies a wall-clock deadline to a `Task.Run` whose start time depends on thread-pool availability, and the token is passed only to `Task.Run` (so it cancels before the work is scheduled, never during it).
- The repository test policy prohibits wall-clock dependence in tests and prefers an injected `TimeProvider` for time-dependent code (`.claude/rules/csharp.md`, Time seam guidance).
- Not related to the NuGet update in PR #779; the production code and test are unchanged on `main` at 1c3b210c.

## Proposed Fix / Validation Ideas

- [ ] Remove the fixed 500 ms deadline or make the timeout an explicit parameter with a `TimeProvider`-backed cancellation so tests can use `FakeTimeProvider`.
- [ ] Have the caller-supplied token, not an internal timer, govern cancellation; the underlying `TryAddValues` is a bounded compare-and-swap loop that does not need a watchdog.
- [ ] Update `TryAddValuesAsync_UpdatesExistingValue` and add a test proving a pre-cancelled token is honoured and that scheduling latency alone does not cancel.
- [ ] Manual verification: run `UtilitiesCS.Test` with coverage and 24 workers ten times; expect zero failures.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
