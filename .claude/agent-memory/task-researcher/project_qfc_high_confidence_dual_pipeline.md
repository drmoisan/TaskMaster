---
name: qfc-high-confidence-dual-pipeline
description: QuickFiler now has THREE high-confidence implementations; the live one (since #233) is the dequeue-time streaming gate. #169 post-hoc removal and #171 pre-filter are both dormant. Admission never scores (test-pinned).
metadata:
  type: project
---

QuickFiler high-confidence filtering has three separately-implemented pipelines (state verified 2026-08-06 during #424 research):

1. **Issue #233 dequeue-time gate — LIVE.** `QfcDatamodel.QueueProcessing.DequeueWithHighConfidenceGateAsync` -> `QfcStreamingDequeueConfidenceGate.DequeueAsync`. In high-confidence mode `RunAsync` sets `initializationBatchSize = 0` and takes the first UI batch from the gate. Scores each streamed candidate serially; rejects are permanently dropped (and never unhooked from `EmailMoveMonitor` — session-scoped COM-ref retention).
2. **Issue #169 post-hoc removal — DORMANT.** `QfcFormController.ApplyHighConfidenceFilterAsync` / `RemoveBelowThresholdAsync` carries doc comments stating #233 replaced it ("live filtering occurs in the datamodel dequeue layer").
3. **Issue #171 pre-filter carrier path — DORMANT.** `QfcHighConfidencePreFilter.FilterAsync` + `LoadItemsAsync(IList<QfcPreScoredItem>)`; still zero production callers.

`QfcRemainingQueueAdmission.TryQueueAsync` accepts a `scoreLoader` ctor arg but never invokes it — intentional (#233 design), pinned by `QfcDatamodelTests` failure messages ("Threshold scoring belongs to dequeue-time enforcement"). Not a regression.

**How to apply**: for any high-confidence behavior/latency report, start at the #233 gate, not the dormant paths. The gate's contract (partial results legal, below-threshold discarded, inclusive `>=` cutoff = threshold*1000) is pinned in `QfcStreamingDequeueConfidenceGateTests.cs`.

Related: [[qfc424-high-confidence-startup-stall]] (latency defect in this gate), [[project_qfc227_headless_itemviewer_and_tlpcellsnapshot]].
