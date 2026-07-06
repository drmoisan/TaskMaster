---
name: feedback-tests-must-not-trigger-ux-or-live-worker
description: Unit tests must never start a live BackgroundWorker/form that runs production UX (MessageBox) or COM; seam the worker body
metadata:
  type: feedback
---

Unit tests must not trigger real UX (MessageBox/WinForms dialogs) or start a live `BackgroundWorker` that executes production COM/UX code.

**Why:** On #244 the maintainer received repeated modal "Email Frame is empty" pop-ups while tests ran. The new QfcDatamodel tests called `InitEmailQueue(..., new BackgroundWorker())`, which runs the real `SetupWorker` + `RunWorkerAsync()`; the started production `Worker_DoWork` -> `LoadRemainingEmailsToQueueAsync` calls `MessageBox.Show("Email Frame is empty")` (and touches Outlook COM via `_olApp.GetNamespace("MAPI")`). A drained/empty `_frame` guarantees the modal dialog. This blocks the run and violates determinism/no-external-UX rules.

**How to apply:** When a method under test starts a background worker or shows a dialog, introduce the smallest DI seam (an injectable `Func<>`/`Action<>` delegate defaulting to the production behavior) so the worker body / notification is inert in tests. Tests inject a recording no-op and assert AC by observing the seam invocation (with a bounded `TaskCompletionSource` signal, never a fixed sleep) plus deterministic synchronous side effects (e.g. `WorkerSupportsCancellation`). Do not assert on a racing `IsBusy` immediately after `RunWorkerAsync()` — `async void` DoWork completes at its first await and the check is flaky. Verify no test path can reach a real `MessageBox.Show`/form or a live COM call.
