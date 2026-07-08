# timeout-task-flaky-timing (Issue #191)

- Date captured: 2026-06-12
- Author: Dan Moisan

- Status: Promoted -> docs/features/active/timeout-task-flaky-timing/ (Issue #191)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #191
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/191
- Last Updated: 2026-06-13
- Work Mode: minor-audit

## Summary

`TimeOutTask_Tests.RunWithTimeout_FuncT1TResult_ShouldReturnResult` is flaky under parallel and/or coverage execution. It runs a trivial synchronous function but asserts completion within a 200 ms wall-clock window; under thread-pool starvation (24-worker class-level parallelism, aggravated by coverage instrumentation overhead) the work can miss the window and the test fails with a `TimeoutException` in strict mode.

## Environment

- OS/version: Windows; failure observed in Visual Studio "Analyze Code Coverage" and under class-level parallel runs
- Python version: n/a (C# / MSTest)
- Command/flags used: parallel MSTest (`Workers=0`, `ClassLevel`); aggravated under code-coverage instrumentation
- Data source or fixture: `UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs` (test #128), `UtilitiesCS/Threading/TimeOutTask.cs`

## Steps to Reproduce

1. Run the full `UtilitiesCS.Test` suite with class-level parallelism (and/or under code coverage) on a busy machine.
2. Observe `RunWithTimeout_FuncT1TResult_ShouldReturnResult` intermittently fail with a `TimeoutException`.
3. Run the single test in isolation and observe it passes.

## Expected Behavior

The test passes deterministically regardless of parallelism, coverage instrumentation, or machine load.

## Actual Behavior

The test calls `function.RunWithTimeout(42, CancellationToken.None, milliseconds: 200, maxAttempts: 0, strict: true)` with `function = arg => $"result-{arg}"`. The trivial function is scheduled on the thread pool and raced against a 200 ms timeout. Under thread-pool starvation the work does not complete within 200 ms, the timeout wins, and strict mode throws `TimeoutException`, failing the assertion.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: `TimeoutException` from `RunWithTimeout` (strict mode) on a function that returns immediately.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

This is the single failure common to both VS Code and Visual Studio after the parallelization/coverage parity changes (#188/#189); it is the last item between the suite and zero failures.

## Suspected Cause / Notes

- Root cause is wall-clock/thread-pool sensitivity in the TEST, not a defect in the production `RunWithTimeout` (timing out under genuine starvation is technically correct behavior).
- `UtilitiesCS/Threading/TimeOutTask.cs` is already ~775 lines (over the 500-line guideline), so a sprawling production refactor (e.g., threading `TimeProvider` through every overload) is undesirable and disproportionate.
- The repository already uses two established patterns for timing-sensitive tests: `[DoNotParallelize]` (e.g., `ApplicationIdleTimer_Tests`, `TimerWrapper_Tests`, `OlTableExtensions_Tests`) and generous timeouts (e.g., `TimeOutTask_Tests` line 75: "increased from 100 ms to 5000 ms").

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: make the affected TimeOutTask timing test(s) deterministic using the repo's established patterns — prefer the smallest test-only change (e.g., `[DoNotParallelize]` on the timing-sensitive class and/or a robust timeout/await that does not depend on a tight wall-clock window for trivially-completing work). Do not weaken the assertion's intent.
- [x] Integration scenario to retest: run the affected test(s) repeatedly under class-level parallelism and under coverage; confirm consistent pass.
- [x] Manual verification notes: prefer a test-only fix; avoid growing `TimeOutTask.cs` and avoid changing production timeout semantics.

## Acceptance Criteria

- [x] AC1: `TimeOutTask_Tests.RunWithTimeout_FuncT1TResult_ShouldReturnResult` (in `UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs`) is made deterministic so it passes consistently under class-level parallelism and under code-coverage instrumentation, not only when run in isolation.
- [x] AC2: The fix is test-only. No change to `UtilitiesCS/Threading/TimeOutTask.cs` (production timeout semantics unchanged), and the ~775-line production file is not grown. If any other `TimeOutTask` timing test shares the same wall-clock/thread-pool sensitivity, it may be stabilized in the same test-only change.
- [x] AC3: The fix uses an established repository pattern for timing-sensitive tests — `[DoNotParallelize]` on the affected test class and/or a robust timing approach that does not depend on a tight wall-clock window for trivially-completing work — consistent with `ApplicationIdleTimer_Tests`, `TimerWrapper_Tests`, and the existing generous-timeout precedent in `TimeOutTask_Tests`.
- [x] AC4: The assertion intent is preserved (the test still verifies that `RunWithTimeout` returns the function's result for the success path). Assertions are not weakened or removed.
- [x] AC5: Determinism is demonstrated: the affected test(s) pass across repeated runs under class-level parallelism (capture evidence). No other test is regressed.
- [x] AC6: C# toolchain passes in order — CSharpier -> .NET analyzers -> nullable -> MSTest (vstest) — for the changed test assembly, with no new analyzer/nullable diagnostics and no coverage regression on changed lines.

### Out of scope

- The #188/#189 parallelization and coverage-exclusion changes (separate, in PR #190).
- Any production change to `RunWithTimeout` overloads or a `TimeProvider` refactor of `TimeOutTask.cs`.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch