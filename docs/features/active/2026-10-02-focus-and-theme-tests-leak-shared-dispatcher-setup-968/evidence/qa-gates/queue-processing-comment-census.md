# Queue-processing comment census (issue #968, tasks P5-T11 and P5-T12)

Timestamp: 2026-10-03T03-15
Command: pwsh -NoProfile -Command '<CMD-LINECOUNT payload>' with FILES `"QuickFiler\Controllers\QfcDatamodel.QueueProcessing.cs"` (the first of three payloads; then CMD-TOKEN-COUNT on QQP with the P0-T13 list and CMD-HUNKS on QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs; each the Command Reference macro executed verbatim with PREFIX expanded and WORKTREE substituted), followed by two git calls
Canonical command: as listed; git -C WORKTREE diff 94287369908cc920b21b0e3256314f988ad7d2f5 -- QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs; git -C WORKTREE status --porcelain -- QuickFiler
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229 (every payload)
- LINES QuickFiler\Controllers\QfcDatamodel.QueueProcessing.cs = 413
- QQP tokens (`RunWorkerAsync`, `written on the worker thread and read`, `written on the worker thread`, `(:31-66)`, `TryUnhookOrReplace`, `WorkerStarter`, `share no other fence`, `honest producer-liveness signal`, `() => _remainingLoadActive,`, `() => false,`, `private volatile bool _remainingLoadActive;`): 0, 0, 1, 0, 3, 1, 1, 0, 1, 0, 1
- CMD-HUNKS: GIT_DIFF_EXIT_CODE: 0; `HUNK @@ -13,13 +13,13 @@ namespace QuickFiler.Controllers` (old range 13 to 25: starts at or above 10 and ends at or below 30); `HUNK @@ -282,7 +282,7 @@ namespace QuickFiler.Controllers` (old range 282 to 288: starts at or above 280 and ends at or below 292); HUNK_COUNT: 2
- Every changed line in the transcribed diff below begins with `///` after its indentation (comment-only: the AC26 and AC20 reading); the declaration `private volatile bool _remainingLoadActive;` and every statement are unchanged.
- porcelain (exit 0): ` M QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` and ` M QuickFiler/Controllers/QfcDatamodel.cs`

QUIESCE-COMMENT-DECISION: UNCHANGED. Reason (D-20): the `QuiesceLoaderAsync` comment at line 52 (`the field is written on the worker thread`) concerns `_remainingLoadTask`, which `Worker_DoWork` writes at QfcDatamodel.cs (pre-edit line 218) on the thread that runs the handler; in production that is still the BackgroundWorker thread, because the production `WorkerStarter` calls `RunWorkerAsync()`, and the comment's snapshot rationale holds for any cross-thread writer. The comment is therefore accurate post-#950 and is left unchanged; `written on the worker thread` reads 1 (line 52) while the AC26 token `written on the worker thread and read` reads 0.

## Transcribed diff (git -C WORKTREE diff 94287369908cc920b21b0e3256314f988ad7d2f5 -- QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs, exit 0)

```diff
diff --git a/QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs b/QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs
index ce2bc0428..654a0bf4b 100644
--- a/QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs
+++ b/QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs
@@ -13,13 +13,13 @@ namespace QuickFiler.Controllers
     public partial class QfcDatamodel
     {
         /// <summary>
-        /// Issue #424: honest producer-liveness signal. Set <see langword="true"/> immediately before
-        /// each <c>RunWorkerAsync()</c> call and cleared in a <c>finally</c> once the awaited
-        /// <c>RemainingEmailLoader</c> completes. <c>BackgroundWorker.IsBusy</c> cannot serve this
-        /// role: <c>Worker_DoWork</c> is <c>async void</c>, so it returns at its first yielding await
-        /// and reports idle while the loader is still producing. Both the dequeue gate's
-        /// <c>sourceActive</c> signal and <see cref="WaitForQueue"/> consume this flag. Declared
-        /// <c>volatile</c> because it is written on the worker thread and read by dequeue callers.
+        /// Issue #424 producer-liveness signal, read by the dequeue gate's <c>sourceActive</c>
+        /// delegate and by <see cref="WaitForQueue"/>. <c>InitEmailQueue</c> sets it just before
+        /// handing the worker to <see cref="WorkerStarter"/>, and <c>Worker_DoWork</c> clears it in
+        /// a <c>finally</c> when the awaited <see cref="RemainingEmailLoader"/> task completes,
+        /// because <c>BackgroundWorker.IsBusy</c> already reads idle at that handler's first
+        /// incomplete await (issue #950 made the start synchronous in tests, so no particular thread
+        /// owns either write). Volatile: the writers and the readers share no other fence.
         /// </summary>
         private volatile bool _remainingLoadActive;
 
@@ -282,7 +282,7 @@ namespace QuickFiler.Controllers
         /// <see cref="QfcDequeueBatch.Items"/> is taken from the same accepted set as
         /// <see cref="QfcDequeueBatch.PreScored"/>, after <see cref="UnhookDequeuedNodes"/> has run
         /// over it. #678 R1: that correspondence holds on the happy path only. On the
-        /// <c>UnhookItem</c> throw path <see cref="TryUnhookOrReplace"/> (:31-66) removes the failed
+        /// <c>UnhookItem</c> throw path <see cref="TryUnhookOrReplace"/> removes the failed
         /// item and inserts a substitute pulled from the master queue, so <c>PreScored</c> can name
         /// an item absent from <c>Items</c> and <c>Items</c> can name an item absent from
         /// <c>PreScored</c>. Leg A reconciles the two at the load boundary through
```
