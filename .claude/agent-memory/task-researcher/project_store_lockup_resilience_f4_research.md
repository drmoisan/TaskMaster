---
name: project-store-lockup-resilience-f4-research
description: F4 (#264) store-lockup-detect-notify research findings — AsyncLocal rejected for cross-thread attribution, MyBox has no modeless path, ThreadMonitor untested/dormant
metadata:
  type: project
---

Completed 2026-07-07: deep research for epic store-lockup-resilience (#260), feature F4
store-lockup-detect-notify (#264), depends on F1 #261 (IStoreDisableService) and F3 #263 (runtime
rehook), both still Draft specs with method lists but no final signatures. Research doc:
`docs/features/active/2026-07-07-store-lockup-detect-notify-264/research/2026-07-07-store-lockup-detect-notify-research.md`.

Key non-obvious findings that will matter when F4 is actually implemented:

1. **`AsyncLocal` cannot carry store-identity attribution to the watchdog.** The delegation prompt
   suggested AsyncLocal for the ambient "current store being processed" context, but the watchdog
   (`ThreadMonitor`) polls from an independent `Task.Run` background thread with no async/await
   relationship to the STA thread doing the per-store COM work. AsyncLocal only flows within one
   logical call chain, so it would never be visible to the watchdog thread. Correct mechanism: a
   plain `static volatile string` holder (single-writer STA thread, single-reader watchdog thread,
   both facts verified by reading the three per-store loops, which are all synchronous on the STA
   thread). See research §3.1/§3.2.

2. **`MyBox` (`UtilitiesCS\Dialogs\MyBox.cs`) has no modeless code path today.** Every
   `ShowDialog` overload calls `DialogInvoker(viewer)`, whose production default is
   `viewer.ShowDialog()` (modal/blocking); the `AsyncLocal<Func<MyBoxViewer,DialogResult>>` seam
   exists only for tests to stub the modal call, not as a prod modal/modeless switch. Worse, the
   convenience overloads wrap the viewer in a `using` block — repointing `DialogInvoker` to
   `viewer.Show()` would dispose the form immediately after `Show()` returns while still on
   screen. A genuinely modeless notification needs a new composition (owns the `MyBoxViewer`
   lifetime via `FormClosed`, injectable `Action<MyBoxViewer> showAction` defaulting to
   `viewer => viewer.Show()`, mirroring `EfcHomeController.ViewerShowAction` at
   `QuickFiler\Controllers\EfcHomeController.cs:294-297`), not reuse of `MyBox.ShowDialog(...)`.
   `ActionButton.Button_Click` invokes its delegate independent of modality, so button wiring
   itself is reusable — only the show/dispose mechanics need to change.

3. **`ThreadMonitor` (`UtilitiesCS\Threading\ThreadMonitor.cs`) is currently dormant and
   untested.** `UiThread.Init(monitorUiThread: false)` at `TaskMaster\ThisAddIn.cs:28` is the only
   call site and it's off. Zero test files exist for it. It polls via `Thread.Sleep` directly (no
   injected clock) — any touch for F4 must introduce a `TimeProvider` seam (package
   `Microsoft.Bcl.TimeProvider` already referenced by `UtilitiesCS.Test.csproj`, not yet used
   anywhere in production) to make it testable and compliant with the repo's time-seam guidance
   for touched code.

4. Cheap-`DisplayName`-before-expensive-COM-read is already an established pattern at three call
   sites (`StoreWrapper.Init():36`, `StoresWrapper.RewireOlObjectsAsync:102`,
   `AppOlObjects.EmitPerStoreInboxAttribution:211`) — all added for the #211 attribution
   diagnostics. F4's ambient-scope `Begin`/`Dispose` wraps should slot in around the existing
   Stopwatch-instrumented blocks at those exact three sites without altering existing behavior.

See [[project_onedrive_timeout_test_determinism_253]] for a related, separate finding on
`TimeOutTask.cs`'s per-overload exception-type inconsistency (not directly used by F4's design,
which relies on `ThreadMonitor`'s own polling rather than `TimeOutTask`).
