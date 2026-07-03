# `2026-07-03-quickfiler-high-confidence-dequeue-streaming` — User Story

- Issue: #233
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-03T16-57

## Story Statement

- As a QuickFiler user processing inbox items in high-confidence mode, I want each page to contain the requested number of qualifying items when enough qualifying items remain, so that I can process confident filing suggestions without empty or partially populated pages caused by earlier low-confidence messages.
- As a QuickFiler maintainer, I want high-confidence filtering to be enforced once at dequeue time with testable streaming/backfill behavior, so that score changes from classifier retraining are handled consistently and ordinary non-high-confidence processing remains unchanged.

## Problem / Why

QuickFiler's "high confidence mode" is intended to present the user only with items whose top folder-suggestion probability is at or above the configured threshold (default 90%). In observed production runs it does not behave that way, in two related ways:

1. **Pages display only a subset of items.** On the first screen and on subsequent screens, fewer items appear than the per-iteration page size, even though more qualifying (>= threshold) items exist further down the inbox.
2. **Entirely empty screens appear.** Repeatedly, a screen loads with no items, and advancing to the next screen also shows nothing, despite qualifying items remaining in the inbox.

### Verified root cause (why this happens today)

There are two independently-implemented high-confidence filtering pipelines in the codebase, and the one reachable in production filters **after** a fixed-size batch has already been chosen, with **no backfill**:

- **Issue #169 — post-hoc removal (LIVE / the only production-reachable path).** `QfcHomeController.Run()`/`RunAsync()` (`QuickFiler/Controllers/QfcHomeController.cs:248-293`) call `_datamodel.InitEmailQueue(ItemsPerIteration, worker)` then `_formController.LoadItems(listEmail)` / `LoadItemsAsync(listEmail)` with a plain `IList<MailItem>`. `InitEmailQueue` (`QuickFiler/Controllers/QfcDatamodel.cs:211-238`) slices exactly `batchSize` (`ItemsPerIteration`) rows off the top of the inbox frame with **no score check**, builds full UI controllers for all of them, computes each score during `LoadSecondaryAsync`, and only afterward removes the below-threshold groups via `ApplyHighConfidenceFilterAsync` (`QuickFiler/Controllers/QfcFormController.Actions.cs:104-110, 180-191`) → `RemoveBelowThresholdAsync` (`QuickFiler/Controllers/QfcCollectionController.cs:1059-1079`). Nothing replenishes the page from later inbox items. If 8 items are loaded and 6 fall below threshold, the page shows 2. If all 8 fall below threshold, the page is empty.
- **Issue #171 — pre-filter (DEAD in production).** `QfcHighConfidencePreFilter.FilterAsync` (`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`) scores before building UI and constructs controllers only for survivors. It is fully implemented and unit-tested, but has **zero production callers**: `QfcHomeController.HighConfidencePreFilterLoader` (`QfcHomeController.cs:236-244`) and `QfcFormController.LoadItemsAsync(IList<QfcPreScoredItem>)` (`QfcFormController.Actions.cs:120-171`) are only invoked from tests. The live `Run()`/`RunAsync()` never route through it. The user may believe pre-filtering-before-display is what runs today — it is not.

Subsequent screens draw from `_masterQueue`. Items enter the queue only after a live per-item score check at admission time (`QfcRemainingQueueAdmission.TryQueueAsync`, `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs:33-56`, wired from `QfcDatamodel.TryQueueRemainingMailItemAsync`/`LoadRemainingEmailsToQueueAsync`, `QfcDatamodel.cs:258-326`). But `DequeueNextItemGroupAsync(quantity, timeOut)` (`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:55-110`) returns however many items happen to be queued via `TryTakeFirst(quantity)` — **no padding, no backfill**. `WaitForQueue` (`QfcDatamodel.QueueProcessing.cs:133-140`) only blocks `while (_worker.IsBusy && _masterQueue.Count < quantity)`; once the background scan finishes or momentarily stalls, a page is built from whatever qualified so far, which can be fewer than `ItemsPerIteration` (or zero).

Threshold comparisons themselves are correct and consistently boundary-inclusive (`>=` keep / `<` reject) across `QfcHighConfidencePreFilter.FilterAsync`, `QfcRemainingQueueAdmission.TryQueueAsync` (`:47-50`), and `RemoveBelowThresholdAsync` (`:1071`). The defect is the fixed-batch-then-filter-without-backfill structure and the split of filtering responsibility across multiple layers — not an off-by-one in the comparison.

### Score-mutation context (why placement/timing matters)

The same item's folder score is computed independently at two times by two paths, each constructing a fresh predictor against the live, mutable trained classifier (retrained as items are filed): admission-time (`QfcDatamodel.ScoreRemainingQueueMailItemAsync`, `:316-326`) and display-time (`QfcItemController.LoadFolderHandler(Async)`, `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:27-111`, exposed via `TopFolderScore`, `QfcItemController.cs:254`). These two measurements can diverge for the same item as other items are filed between admission and display. The user has explicitly decided the desired resolution of this ambiguity (see Proposed Behavior item 4).


## Personas & Scenarios

- Persona: High-volume Outlook filer
  - Uses QuickFiler to process many inbox messages through folder suggestions.
  - Enables high-confidence mode to focus only on messages whose top folder suggestion meets the configured confidence threshold.
  - Expects each page to be stable once shown and does not expect items to disappear after appearing on a page.
  - Is constrained by Outlook/WinForms responsiveness and should not be exposed to incomplete pages while qualifying messages remain.
  - Wants predictable page counts, no empty pages while qualifying candidates remain, and unchanged behavior when high-confidence mode is disabled.
- Persona: QuickFiler maintainer
  - Maintains the queue, page-building, and folder-scoring paths without relying on live Outlook in unit tests.
  - Needs one confidence gate with deterministic test coverage instead of separate admission-time, UI-removal, and dormant pre-filter pipelines.
  - Must preserve issue #232 navigation and probability logging behavior while implementing issue #233.
- Scenario: Sparse qualifying messages in high-confidence mode
  - The user starts QuickFiler with high-confidence mode enabled and a per-iteration page size of `N`.
  - The inbox contains qualifying messages, but they are interleaved with many below-threshold messages.
  - QuickFiler streams candidates through the dequeue-layer gate, computes the current dequeue-time score for each candidate, discards below-threshold candidates, and backfills until `N` qualifying items are collected or the source is exhausted.
  - The page shown or cached for the user contains only items that qualified at dequeue time and remains stable even if later filing activity changes the classifier and a later score would fall below the threshold.
  - When high-confidence mode is disabled, the same queue path returns ordinary candidates without confidence filtering or streaming discard.
- Scenario: Prerequisite drift from issue #232
  - Planning begins on the #233 branch and detects that issue #232 logging/navigation changes are expected by the issue but may not be present on the current branch.
  - The implementation plan integrates or otherwise preserves the #232 navigation fix and score logging before validating #233.
  - Final validation confirms the new dequeue-time log line exists in addition to the #232 log points and that page swapping does not regress navigation registration.


## Acceptance Criteria

- [x] AC1 — High-confidence filtering exists in exactly one location (the queue/dequeue layer). The post-hoc removal path (`ApplyHighConfidenceFilterAsync` → `RemoveBelowThresholdAsync`) is no longer invoked to enforce the confidence threshold in the live flow, and no first-screen path loads an unfiltered fixed batch that is later trimmed by confidence. A repo-wide search shows no confidence-threshold comparison outside the single dequeue-layer location (excluding the dormant #171 pre-filter, whose disposition is recorded under AC8).
- [x] AC2 — The confidence threshold is evaluated at dequeue time. A unit test demonstrates that an item whose dequeue-time score is >= threshold is returned even if a different (earlier) score would have rejected it, and an item whose dequeue-time score is < threshold is discarded, with the decision driven by the dequeue-time measurement.
- [x] AC3 — Streaming backfill: when N items are requested in high-confidence mode and the candidate source contains at least N qualifying items interleaved with below-threshold items, the dequeue returns exactly N qualifying items (all >= threshold), having discarded the below-threshold candidates it encountered. A unit test covers the "must scan many to yield few" case (e.g., request 7, source arranged so ~40 below-threshold candidates are interleaved).
- [x] AC4 — Source-exhaustion boundary: when fewer than N qualifying items remain, the dequeue returns all remaining qualifying items (0..N-1) without blocking indefinitely and without throwing. A unit test covers the zero-qualifying-remaining case (returns empty, does not hang, does not throw) and the partial case.
- [x] AC5 — No post-display removal: after an item is returned by the dequeue and placed on a page, a subsequent recomputation of its score below the threshold does not remove it from that page. A unit test demonstrates a surfaced item remains present after a simulated below-threshold rescore.
- [x] AC6 — Empty-page regression: a scenario reproducing the reported symptom (qualifying items sparse relative to page size) yields full pages of qualifying items up to the per-iteration size and no empty page while qualifying items remain. Expressed as a logical-level test at the queue/dequeue seam (no live Outlook).
- [x] AC7 — Disabled-mode parity: when `HighConfidenceModeEnabled == false`, dequeue behavior is unchanged from today (no filtering, no streaming discard, same item counts). A unit test asserts parity.
- [x] AC8 — Disposition of the two pipelines is explicit: the live path is the redesigned dequeue-layer filter; the dormant Issue #171 pre-filter (`QfcHighConfidencePreFilter`, `HighConfidencePreFilterLoader`, `LoadItemsAsync(IList<QfcPreScoredItem>)`) is either wired to the new single location or explicitly retired/left dormant with a recorded decision. The change description states which and why. No third filtering pipeline is introduced.
- [x] AC9 — Threshold semantics preserved: the boundary remains inclusive (score == threshold qualifies), matching the existing `>=` keep / `<` reject convention. Cutoff scaling (`Math.Round(threshold * 1000, 0)`, long comparison) is preserved or its change is justified.
- [ ] AC10 — Full C# toolchain passes on the final pass (CSharpier → .NET analyzers → nullable/warnings-as-errors → MSTest with coverage). New/changed non-COM-bound code meets the >= 90% coverage target; repository-wide coverage does not regress below 80% on the testable denominator. COM/WinForms-bound touched surfaces rely on the ratified `[ExcludeFromCodeCoverage]` exemption only where already applicable.
- [x] AC11 — The probability debug logging introduced by issue #232 (item summary, score, caller, at `QfcDatamodel.ScoreRemainingQueueMailItemAsync`, `QfcItemController.FolderHandling` scoring points, and `QfcHighConfidencePreFilter.FilterAsync`) remains intact; any new dequeue-time scoring introduced by this work emits an equivalent debug log line (item summary, dequeue-time score, caller context) so score evolution across admission→dequeue is observable.
- [x] AC12 — No unhandled behavioral regression in the ordinary (non-high-confidence) bulk-processing flow: OK/Skip/pop-out page transitions still function; queue draining, `WaitForQueue` termination, and move-monitor hook/unhook semantics are preserved.


## Non-Goals

- Implementing the issue #232 KbdActions duplicate-key crash fix as new #233 scope. Issue #232 is a prerequisite drift that planning must integrate or preserve, not a behavior expansion for #233.
- Changing the configured high-confidence threshold default, threshold scale, or inclusive boundary semantics unless a documented implementation decision justifies it.
- Introducing a third high-confidence filtering pipeline or moving enforcement into the UI/collection layer.
- Retiring the dormant #171 pre-filter unless planning explicitly chooses that disposition under AC8.
- Changing ordinary non-high-confidence bulk-processing behavior, page transition semantics, or move-monitor hook/unhook behavior.
- Adding a CLI, external service dependency, new persisted data store, or data migration.
- Relying on live Outlook, temporary files, or external services for unit-test coverage.
