# uithread-init-accepts-non-sta-callers (Issue #787)

- Date captured: 2026-09-05
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/uithread-init-accepts-non-sta-callers/ (Issue #787)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #787
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/787
- Last Updated: 2026-09-06
## Summary

`UtilitiesCS.UiThread.Init()` accepts a call from any thread. It performs no apartment-state check, and neither does the `Initialize()` it guards. A worker-thread call therefore succeeds silently and installs that worker's non-pumping `Dispatcher`, `SynchronizationContext`, and managed thread id into set-once process-global state, after which every consumer of `UiThread.Dispatcher`, `UiThread.UiSyncContext`, `UiThread.AutoScaleFactor`, and `UiThread.UiThreadId` marshals onto a thread that never runs a message loop.

Raised as the behavioral half of finding C09 in the three-phase post-merge review of PR #778 (issue #584). The message-text half of C09 is delivered in issue #782; this entry is the behavior change that #782 explicitly placed out of scope.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8, VSTO add-in hosted by Outlook desktop
- Command/flags used: `vstest.console.exe <nine test assemblies> /InIsolation`
- Data source or fixture: `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs`

## Steps to Reproduce

1. Call `UtilitiesCS.UiThread.Init(false)` from a thread whose apartment state is MTA. The in-repo instance is `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs:329`, inside `Worker_RunWorkerCompleted_HandlesCompletionCorrectly` at `:326`, which is a plain `[TestMethod]` on a class carrying `[TestClass]` only.
2. Observe that the call returns normally rather than rejecting the caller.
3. Read `UiThread.Dispatcher`, `UiThread.UiSyncContext`, or `UiThread.UiThreadId` from any later code in the same process.

## Expected Behavior

`Init()` rejects a non-STA caller with a named `InvalidOperationException` before it captures anything, so the process-global UI context can only ever be populated from a thread that runs a message loop.

## Actual Behavior

The call succeeds. `Initialize()` constructs and shows a WinForms `SyncContextForm`, and `CaptureUiVariables()` reads `SynchronizationContext.Current`, `this.AutoScaleFactor`, `Dispatcher.CurrentDispatcher`, and `Thread.CurrentThread.ManagedThreadId` from the calling thread unconditionally. Because the latch at `UiThread.cs:36` is single-shot, the first caller wins permanently, so a worker-thread `Init()` that happens to run first poisons the globals for the process lifetime. The exception message added by issue #782 names `Init()` as the remedy, which offers nothing in this state because `Init()` has already run.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: not captured. The defect is a missing precondition rather than a failure, so it produces no diagnostic; it is established by reading the call chain below.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium rather than High because the hazard is presently reachable only from test code. In production `TaskMaster/ThisAddIn.cs:35-40` is the only direct caller and runs on the Outlook STA during `ThisAddIn_Startup`. The severity would rise if any worker-thread code path began reading the lazy accessors before startup completed.

## Suspected Cause / Notes

- `UtilitiesCS/Threading/UiThread.cs:19-40` — `Init(...)` validates none of its callers' context. Its only gate is the single-shot latch at `:36`, `if (_loaded.CheckAndSetFirstCall)`.
- `UtilitiesCS/Threading/UiThread.cs:59-90` — `Initialize()` constructs and shows the `SyncContextForm` and then calls `CaptureUiVariables()`. No apartment check.
- `QuickFiler/Viewers/SyncContextForm.cs:34-40` — `CaptureUiVariables()` reads the four values from the calling thread unconditionally.
- Two latent entry points exist beyond the direct callers: the `UiSyncContext` getter at `UiThread.cs:128-131` and the `AutoScaleFactor` getter at `UiThread.cs:194-197` both call `Init()` when their backing field is null, so any reader of either property on a non-STA thread is an implicit `Init()` caller. Production readers of `UiSyncContext` are `UtilitiesCS/Threading/ThreadMonitor.cs:143`, `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:178`, and `TaskMaster/AppGlobals/AppOlObjects.cs:367`; the `ThreadMonitor` reader runs on a watchdog thread and is the one production path worth re-checking during implementation.
- Blast radius, measured: three textual `UiThread.Init` call sites, of which two are live. `TaskMaster/ThisAddIn.cs:35-40` is STA and unaffected; `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs:329` is MTA and is the single in-repo caller the change breaks; `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs:170` is commented out.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: `UiThread.Init` apartment rejection, latch preservation on rejection, and both branches of each lazy accessor.
- [x] Integration scenario to retest: `QfcHomeControllerRunAsyncTests.Worker_RunWorkerCompleted_HandlesCompletionCorrectly` on an STA context.
- [ ] Manual verification notes: none required; the change is fully covered by deterministic tests.

Proposed behavior:

- `UiThread.Init(...)` throws `InvalidOperationException` when `Thread.CurrentThread.GetApartmentState() != ApartmentState.STA`, with a message naming the requirement and the caller's observed apartment state.
- The check runs before the single-shot latch at `UiThread.cs:36` is consumed, so a rejected call does not burn the one-shot and a subsequent correct call still initializes.
- The two lazy accessors keep their current self-healing behavior on the STA and surface the same named exception off it, instead of silently capturing a worker thread's context.
- `QfcHomeControllerRunAsyncTests.Worker_RunWorkerCompleted_HandlesCompletionCorrectly` is migrated to an STA context, which is the only in-repo caller the change breaks.

Acceptance criteria for the resulting issue:

- [ ] AC1: `UiThread.Init()` called from an MTA thread throws `InvalidOperationException` whose message names the STA requirement and the observed apartment state. Covered by a deterministic test that runs the Act on a dedicated MTA thread and joins it.
- [ ] AC2: `UiThread.Init()` called from an STA thread behaves exactly as before. Covered by a test that asserts the single-shot latch, the captured dispatcher, and the captured `UiThreadId` are unchanged.
- [ ] AC3: A rejected non-STA call does not consume the single-shot latch: a subsequent STA call in the same process still runs `Initialize()`.
- [ ] AC4: `QfcHomeControllerRunAsyncTests.Worker_RunWorkerCompleted_HandlesCompletionCorrectly` passes on an STA context, and a repository-wide grep confirms no remaining `UiThread.Init` call site executes off the STA.
- [ ] AC5: The `UiSyncContext` and `AutoScaleFactor` lazy-`Init()` branches are covered for both the STA (self-heals) and non-STA (throws) cases.
- [ ] AC6: The full C# toolchain (csharpier, analyzers, nullable, vstest with coverage) passes and changed-line coverage does not decrease.

Interaction with the sibling entry: issue #782 considered and withdrew finding C03, which would have re-armed the single-shot latch when `Initialize()` throws. That withdrawal is tracked separately as `uithread-init-latch-not-rearmed-after-failed-initialize`. AC3 above is deliberately narrower than C03 was: it requires only that a rejected non-STA call leave the latch unconsumed, which is achievable by checking the apartment state before the latch is read and does not depend on the withdrawn re-arm.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
