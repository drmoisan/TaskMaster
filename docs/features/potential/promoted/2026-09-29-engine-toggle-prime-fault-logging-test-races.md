# engine-toggle-prime-fault-logging-test-races (Issue #942)

- Date captured: 2026-09-29
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/engine-toggle-prime-fault-logging-test-races/ (Issue #942)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #942
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/942
- Last Updated: 2026-09-30
## Summary

`EngineToggleStateCoordinatorTests.GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` failed once in CI and passed on rerun. The fault logging it asserts appears to race the task the test awaits.

## Environment

- OS/version: windows-latest (GitHub Actions)
- Python version: n/a (C# / MSTest)
- Command/flags used: required check MSTest with coverage
- Data source or fixture: n/a

## Steps to Reproduce

1. Run `TaskMaster.Test` under the parallel regime (Workers=0, Scope=ClassLevel).
2. Observe `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` (`TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs:213`). It fails intermittently.

## Expected Behavior

The test is deterministic: the error log it asserts is written before the awaited task completes, or the test awaits the logging continuation itself.

## Actual Behavior

It failed once on PR #939 head `9624376dc`, passed on a single rerun, and passed in two local runs. PR #939 does not touch this code.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: CI run for PR #939 at head `9624376dc` (first attempt).

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

The prime fault is probably observed and logged in a continuation that is not part of the awaited task, so the assertion can run before the log call. This is a determinism defect that hits a required check. Fix it by awaiting or injecting the continuation, not by retries, sleeps, `[DoNotParallelize]`, or Workers=1.

## Proposed Fix / Validation Ideas

- [ ] Write a regression test that forces the ordering deterministically (for example a controllable scheduler or a `TaskCompletionSource` gate), then fix the coordinator or the test seam.
- [ ] Negative control: show that the test fails when the ordering is inverted.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
