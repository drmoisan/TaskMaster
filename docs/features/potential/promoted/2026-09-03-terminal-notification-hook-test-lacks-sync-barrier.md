# terminal-notification-hook-test-lacks-sync-barrier (Issue #751)

- Date captured: 2026-09-03
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/terminal-notification-hook-test-lacks-sync-barrier/ (Issue #751)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #751
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/751
- Last Updated: 2026-09-03
## Summary

`AppOlObjectsFolderTreeServiceLifecycleTests.TerminalNotificationHookFailure_DoesNotReplaceDispatchFault` in `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs` asserts `sut.InvokedTerminalHookCount.Should().Be(1)` immediately after `await run.Operation.ReleaseAsync()` with no synchronization barrier guaranteeing the terminal hook has executed. The test is non-deterministic and failed once on CI with identical code passing on re-run.

## Environment

- OS/version: GitHub Actions `windows-latest` runner (CI job `mstest-coverage / Run MSTest suite with coverage`); also reproducible in principle on any Windows host running the MSTest suite.
- Python version: n/a (C# / MSTest).
- Command/flags used: `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage /InIsolation` (the CI coverage step).
- Data source or fixture: `ControlledUiDispatcher(DispatchMode.Pending, fault: ...)` with `CreateSut(dispatcher, throwFromTerminalHook: true)`; no external data.

## Steps to Reproduce

1. Check out `main` at or after merge commit `a679cd08` (PR #746).
2. Run the `TaskMaster.Test` assembly repeatedly under vstest (loop or CI re-runs).
3. Observe `TerminalNotificationHookFailure_DoesNotReplaceDispatchFault` intermittently fail with `Expected sut.InvokedTerminalHookCount to be 1, but found 0`.

## Expected Behavior

The test deterministically observes the terminal hook invocation before asserting on `InvokedTerminalHookCount`, and passes on every run.

## Actual Behavior

On PR #746 the `mstest-coverage` required check failed once asserting `InvokedTerminalHookCount` to be 1 and finding 0, then passed on re-run. The same check had already passed against head `e6c488bf`, which contained every production and test change; the only delta to the failing head was three Markdown audit files, so identical code produced opposite outcomes.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: `Expected sut.InvokedTerminalHookCount to be 1, but found 0.` (FluentAssertions message from the failed CI run on PR #746; recorded in `artifacts/orchestration/parallel-orchestrator-state.json` under `latent_defects_pending_promotion`).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: a flaky required CI check blocks unrelated pull requests and forces re-runs; it also violates the determinism requirement in the General Unit Test Policy (UT1) and the test-determinism objective of issue #729.

## Suspected Cause / Notes

- Test source on `main` (line 102 onward): after `dispatcher.Complete(run.Operation, DispatchMode.Faulted)` and awaiting the worker exception, the test calls `await run.Operation.ReleaseAsync()` and then asserts `sut.LoadCount.Should().Be(0)` and `sut.InvokedTerminalHookCount.Should().Be(1)` with no await on any signal that the terminal notification hook has run.
- The terminal hook appears to be dispatched asynchronously relative to `ReleaseAsync()` completing, so the counter can still be 0 at assertion time.
- This is distinct from the known `PhysicalFileInfoAdapter` solution-file contention flake.
- Item #729 (test determinism and hygiene debt) did not touch this file; this is the same class of debt in a file outside its scope.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: expose a completion signal for the terminal hook on the test SUT (for example a `TaskCompletionSource` set when the hook is invoked) and await it before asserting the count; or await the worker/operation completion that is guaranteed to run after the hook.
- [x] Integration scenario to retest: run the `TaskMaster.Test` assembly 20+ times locally under vstest and confirm zero failures of this test.
- [x] Manual verification notes: confirm the fix does not use `Thread.Sleep`, `Task.Delay`, or wall-clock waits (banned by the General Unit Test Policy determinism rules).

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
