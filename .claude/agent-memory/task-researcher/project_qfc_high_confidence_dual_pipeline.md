---
name: qfc-high-confidence-dual-pipeline
description: QuickFiler has two independent high-confidence filter implementations (#169 post-hoc removal, live; #171 pre-filter, dead in production) plus a fixed-batch-without-backfill pattern that explains "subset of items on navigation" reports
metadata:
  type: project
---

QuickFiler high-confidence mode has two separately-implemented filtering pipelines:
1. Issue #169 post-hoc removal (`QfcFormController.Actions.cs`, `QfcCollectionController.RemoveBelowThresholdAsync`) -- the only path reachable from `QfcHomeController.RunAsync()`/`Run()`. Loads a fixed `ItemsPerIteration`-size batch unconditionally, scores during `LoadSecondaryAsync`, then strips below-threshold groups after the fact, with no backfill from later candidates.
2. Issue #171 pre-filter (`QfcHighConfidencePreFilter.cs`, `QfcHomeController.HighConfidencePreFilterLoader`, `LoadItemsAsync(IList<QfcPreScoredItem>)`) -- scores before building UI, only survivors get controllers. Confirmed (2026-07-03, via repo-wide grep for call sites) to have **zero production callers** -- only exercised in `QfcHomeControllerRunAsyncTests.cs` / `QfcHomeControllerIssue218Tests.cs` / `QfcFormControllerTests.cs`. Fully implemented and unit-tested but never wired into the live startup path.

Separately, subsequent screens draw from `_masterQueue`, gated by a live per-item score check at admission time (`QfcRemainingQueueAdmission.TryQueueAsync`), but `DequeueNextItemGroupAsync` returns however many items are actually queued (`TryTakeFirst(quantity)`, no padding) -- so any screen can legitimately show fewer than `ItemsPerIteration` items even when the >90% filter is working correctly.

**Why this matters**: reports of "QuickFiler high-confidence mode shows only a subset of items" are very likely explained by the fixed-batch-without-backfill pattern above, not a threshold-comparison bug (checked: `>=`/`<` boundaries are consistent and correct across `QfcHighConfidencePreFilter.FilterAsync`, `QfcRemainingQueueAdmission.TryQueueAsync`, and `RemoveBelowThresholdAsync`).

**How to apply**: before assuming a new high-confidence-mode bug report is a scoring defect, check (a) whether it's actually the dormant #171 pre-filter path being expected but not running, and (b) whether the visible count is explained by batch-size-then-filter with no backfill, before looking for score-calculation errors. Full research: `artifacts/research/2026-07-03T00-00-quickfiler-kbdactions-duplicate-key-research.md` (Investigation 2 section).

Related: [[project_qfc227_headless_itemviewer_and_tlpcellsnapshot]] (same QuickFiler controller family, different issue).
