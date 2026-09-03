# terminal-notification-hook-test-lacks-sync-barrier (Issue #751)

- Date captured: 2026-09-03
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/ (Issue #751)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #751
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/751
- Last Updated: 2026-09-03
- Work Mode: full-bug

## Summary

`AppOlObjectsFolderTreeServiceLifecycleTests.TerminalNotificationHookFailure_DoesNotReplaceDispatchFault`
in `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs` asserts
`sut.InvokedTerminalHookCount.Should().Be(1)` immediately after `await run.Operation.ReleaseAsync()`
with no synchronization barrier guaranteeing the terminal hook has actually executed by that point.
The test is non-deterministic: it failed once on CI (PR #746, job `mstest-coverage`) with
`Expected sut.InvokedTerminalHookCount to be 1, but found 0`, then passed on an identical-code re-run.

## Environment

- OS/version: GitHub Actions `windows-latest` runner (CI job `mstest-coverage / Run MSTest suite with
  coverage`); also reproducible in principle on any Windows host running the MSTest suite.
- Python version: n/a (C# / MSTest).
- Command/flags used: `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage /InIsolation`
  (the CI coverage step).
- Data source or fixture: `ControlledUiDispatcher(DispatchMode.Pending, fault: ...)` with
  `CreateSut(dispatcher, throwFromTerminalHook: true)`; no external data.

## Steps to Reproduce

1. Check out `main` at or after merge commit `a679cd08` (PR #746).
2. Run the `TaskMaster.Test` assembly repeatedly under vstest (loop or CI re-runs).
3. Observe `TerminalNotificationHookFailure_DoesNotReplaceDispatchFault` intermittently fail with
   `Expected sut.InvokedTerminalHookCount to be 1, but found 0`.

## Expected Behavior

The test deterministically observes the terminal hook invocation before asserting on
`InvokedTerminalHookCount`, and passes on every run.

## Actual Behavior

On PR #746 the `mstest-coverage` required check failed once asserting `InvokedTerminalHookCount` to
be 1 and finding 0, then passed on re-run. The same check had already passed against head `e6c488bf`,
which contained every production and test change; the only delta to the failing head was three
Markdown audit files, so identical code produced opposite outcomes.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: `Expected sut.InvokedTerminalHookCount to be 1, but found 0.` (FluentAssertions message from
  the failed CI run on PR #746; recorded in `artifacts/orchestration/parallel-orchestrator-state.json`
  under `latent_defects_pending_promotion`).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: a flaky required CI check blocks unrelated pull requests and forces re-runs; it also violates
the determinism requirement in the General Unit Test Policy (UT1) and the test-determinism objective
of issue #729.

## Suspected Cause / Notes

Root cause is likely a missing await/synchronization barrier on an async dispatch/terminal-hook
completion path in the test or its fixture (`ControlledUiDispatcher`), not a production defect --
this hypothesis is to be verified in research rather than assumed. If research finds the race is
actually reachable in production code (not just the test double), scope should broaden accordingly.

## Proposed Fix / Validation Ideas

- [x] Trace `ControlledUiDispatcher` and `AppOlObjectsFolderTreeService`'s terminal-hook dispatch path
  to determine whether `ReleaseAsync()` can legitimately return before the terminal hook has run.
  Answered by research §2.2-§2.3: the worker is released by `TrySetException` at
  `AppOlObjects.FolderTreeService.cs:261` while the terminal notification is dispatched afterwards at
  `:269-272`, and `ReleaseAsync()` is a complete no-op on this path (the identity guard at `:177-183`
  returns immediately because the initialization field was already nulled at `:262`). So yes,
  `ReleaseAsync()` legitimately returns before the terminal hook has run, and it is not a barrier.
- [x] Add a deterministic synchronization barrier (e.g., await a completion signal/TaskCompletionSource
  the terminal hook sets) so the assertion cannot race the hook's execution.
  Answered by the delivered change and P3-T3: the test now awaits the terminal signal the fixture already
  captures, via the inserted assertion
  `(await GetExceptionAsync(await run.Terminal)).Should().BeSameAs(fault);`, and the counter is hardened
  with `Interlocked.Increment` and `Volatile.Read`. No new primitive was introduced. The five-run
  repeat-run series recorded by P3-T3 shows the repaired test passing on every run.
- [x] Confirm no production code path shares the same unguarded race; broaden scope if it does.
  Answered by research §3: the defect is test-only. `OnFolderTreeServiceInitializationTerminal` is
  overridden only in test code, `AppOlObjects` has no production subclass at all, and the
  notify-after-publish ordering is deliberate. Scope was therefore not broadened, and
  `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs` is byte-identical to branch point `f8414ee9`
  (P4-T8).

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch

## Source

From: docs/features/potential/2026-09-03-terminal-notification-hook-test-lacks-sync-barrier.md
