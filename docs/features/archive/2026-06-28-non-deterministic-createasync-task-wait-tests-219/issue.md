# non-deterministic-createasync-task-wait-tests (Issue #219)

- Date captured: 2026-06-28
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/non-deterministic-createasync-task-wait-tests/ (Issue #219)

- Issue: #219
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/219
- Last Updated: 2026-06-28
- Work Mode: minor-audit

## Problem / Why

Two MSTest methods in `UtilitiesCS.Test/HelperClasses/QfcTipsDetails_Tests.cs` use the
forbidden blocking-timeout pattern `bool completed = task.Wait(TimeSpan.FromSeconds(10));`:

- `CreateAsync_HiddenLabel_WithMatchingSyncContext_ReturnsInitializedDetails()`
- `CreateAsync_VisibleLabel_WithMatchingSyncContext_ReturnsOnState()`

This violates the repository C# policy in `.claude/rules/csharp.md` ("Prohibited Behaviors:
Adding sleeps, retries, or timing hacks to mask flaky behavior") and the General Unit Test
Policy determinism requirement. The arbitrary 10-second timeout introduces a timing
dependency that makes the tests non-deterministic and can diverge between the IDE runner and
CI. This is a test-policy violation, not a defect in production code; production behavior is
unchanged.

## Proposed Behavior

Both test methods are converted to `async Task` MSTest methods that `await` the
`Task.Run(...)` work to completion and assert directly on the returned result. The forbidden
`Task.Wait(TimeSpan)` call and the timeout-based `completed` assertion are removed. Test
intent, scenario coverage, and the existing `Task.Run` / `SynchronizationContext` setup are
preserved; only the wait mechanism changes.

## Acceptance Criteria

- [x] AC1: `CreateAsync_HiddenLabel_WithMatchingSyncContext_ReturnsInitializedDetails` no
      longer uses `Task.Wait(TimeSpan)` (or any timeout-based wait) and is an awaited
      `async Task` test.
- [x] AC2: `CreateAsync_VisibleLabel_WithMatchingSyncContext_ReturnsOnState` no longer uses
      `Task.Wait(TimeSpan)` (or any timeout-based wait) and is an awaited `async Task` test.
- [x] AC3: No other test or production file is modified; documented test intent and scenario
      coverage are preserved.
- [x] AC4: The full C# toolchain (CSharpier → .NET analyzers → nullable → MSTest) passes,
      and both methods pass under `vstest.console.exe`.

## Constraints & Risks

- Test-only change; no production code is touched and no regression test is required because
  this is a policy/determinism fix rather than a behavioral defect.
- Both methods must keep running the `CreateAsync` work inside `Task.Run` to avoid the
  documented `CoWaitForMultipleHandles` STA deadlock on .NET Framework 4.8.
- Small path: 1 test file, no new modules; coverage of the changed lines must not regress.

## Test Conditions to Consider

- [ ] Hidden-label path (Visible=false else-branch) returns a non-null initialised instance.
- [ ] Visible-label path (Visible=true On-branch) returns a non-null instance in the On state.
- [ ] An exception inside `CreateAsync` propagates out of the awaited task and fails the test.

## Next Step

- [ ] Promote to GitHub issue (bug/refactor template)
- [ ] Create `docs/features/active/non-deterministic-createasync-task-wait-tests/` folder from the template
