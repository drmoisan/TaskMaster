---
name: qfc424-high-confidence-startup-stall
description: "#424 root cause: streaming confidence gate scores serially with no deadline/progress (stall ~ ItemsPerIteration/p, bounded only by folder size); BackgroundWorker async-void makes IsBusy/sourceActive lie; accepted items scored twice (gate + post-Show item controller)"
metadata:
  type: project
---

Issue #424 (2026-08-06) — QuickFiler High Confidence startup stall on "Initializing Email Queue". Key durable findings:

- Stall is entirely `QfcHomeController.RunAsync` lines 277-297: zero progress reports around the gate await. The root ProgressViewer bar actually sits at ~86 (RunAsync child spans 86->100), even though the child reports 0.
- Latency model: `T ~ sum over scanned of score cost + empty-poll*1000ms`, scanned ~ min(N, ItemsPerIteration/p). Unbounded in folder size N; per-candidate cost = MailItemHelper COM materialization + FolderPredictor (full classification when `LoadFromField` misses).
- **BackgroundWorker + async-void trap**: `QfcDatamodel.Worker_DoWork` is `async void`, so `IsBusy` goes false at the first await while `LoadRemainingEmailsToQueueAsync` keeps producing. Both the gate's `sourceActive` (`_worker?.IsBusy == true`, QueueProcessing.cs:87) and `WaitForQueue` rely on this dishonest signal; `Worker_RunWorkerCompleted` UI enablement fires early too. Any fix leaning on producer liveness must first replace this with a datamodel-owned volatile flag.
- Rejected candidates: permanently dropped from the session (test-pinned, intended) but never unhooked from `EmailMoveMonitor` — hook-retention defect reported, not fixed, in #424 research.
- Accepted items scored twice: gate discards `TopFolder` (`ScoreRemainingQueueMailItemAsync` returns only `.Score`); item controller re-runs the identical predictor after `Show()` — wasted work but NOT pre-UI latency.
- Parallel scoring rejected: Outlook COM marshals property reads back to the single STA thread, so MTA-parallel scoring mostly serializes there and risks the #214/#420 affinity defect class.
- Recommended fix (research artifact in the #424 feature folder): TimeProvider-based first-batch deadline inside `QfcStreamingDequeueConfidenceGate` + progress callback + honest liveness flag; deadline as internal constant/seam, not a QfSettings member. `QfcDatamodel` is `[ExcludeFromCodeCoverage]` — keep new logic in the gate.
- `ItemsPerIteration` is computed from screen height (`QfcFormController.SetupDisposal.cs:120-147`), not a setting.

Related: [[qfc-high-confidence-dual-pipeline]].
