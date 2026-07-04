# 2026-07-03-quickfiler-high-confidence-dequeue-streaming — Spec

- **Issue:** #233
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-07-03T16-57
- **Status:** Draft
- **Version:** 0.1

## Overview

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


## Behavior

Redesign high-confidence mode so filtering is the sole responsibility of the queue/dequeue layer, applied at dequeue time, streaming, with backfill. The four required behaviors:

1. **Single filtering location — the Queue only.** High-confidence filtering must live only in the queue/dequeue layer. Remove the high-confidence filter from every other location: the post-hoc `ApplyHighConfidenceFilterAsync`/`RemoveBelowThresholdAsync` removal pass in the UI load path (Issue #169), and any first-screen special-casing that loads an unfiltered fixed batch. The UI/collection layer must not add, remove, or re-filter items on the basis of confidence. After this change there is exactly one place in the code where the high-confidence threshold gates which items reach the user.

2. **Filter at Dequeue time, not at entry.** Today, admission to `_masterQueue` applies the threshold when an item enters the queue (`QfcRemainingQueueAdmission.TryQueueAsync`). Move the confidence gate so it is evaluated at the moment of Dequeue rather than at entry. Rationale: an item's probability can change substantially as earlier items are filed and the classifier retrains, so the score that matters is the one computed when the item is about to be surfaced, not when it was first admitted. (The implementing orchestrator must decide whether the master queue should hold all candidates unfiltered and gate at dequeue, or retain admission but re-score at dequeue; the required observable outcome is that the threshold decision reflects a dequeue-time score.)

3. **Streaming dequeue with backfill until the requested count is satisfied.** When the dequeue function is asked for N items in high-confidence mode, it must return N items that meet the threshold (or exhaust the source trying). It streams: dequeue a candidate, score it at dequeue time, discard it if below threshold, and continue pulling candidates until N qualifying items have been collected or no candidates remain. Example: to return 7 high-confidence items it may need to dequeue and evaluate 47 candidates, discarding 40. The extra processing time is acceptable because this runs in the background for the hidden/cached panel, not the live panel the user is interacting with. Discarded (below-threshold) items are removed from the high-confidence stream; they are not surfaced to the user.

4. **No removal after an item is surfaced to the UX.** Once an item has been returned by the dequeue and placed on a page shown (or cached for showing) to the user, it must remain on that page even if its recomputed confidence later drops below the threshold at filing time. The confidence gate applies exactly once, at dequeue time. There is no post-display re-check that can make an already-surfaced item disappear. This eliminates the "item vanished from the page" class of behavior and makes each page stable once built.

Net effect: in high-confidence mode, every page the user sees is full (up to the per-iteration size) of items that met the threshold at the time they were dequeued, with no empty pages while qualifying items remain in the inbox, and no items disappearing from a page after it is shown. When high-confidence mode is disabled, behavior is unchanged.


## Inputs / Outputs

- User inputs:
  - QuickFiler high-confidence mode setting from `_globals.QfSettings.HighConfidenceModeEnabled`.
  - QuickFiler high-confidence threshold from `_globals.QfSettings.HighConfidenceThreshold`, defaulting to the existing 90% behavior when configured that way.
  - Existing per-iteration page size passed to `DequeueNextItemGroupAsync(int quantity, int timeOut)` and related queue-loading paths.
- Data inputs:
  - Outlook `MailItem` candidates retrieved from the current QuickFiler inbox frame.
  - Folder scoring data from the existing trained classifier and folder scoring services.
  - Cancellation state from the existing QuickFiler cancellation token.
  - Background-worker/source-completion state used by `WaitForQueue` and queue-draining behavior.
- Outputs:
  - A page candidate list containing up to the requested quantity of items that met the high-confidence threshold at dequeue time.
  - Empty or partial candidate lists only when the candidate source is exhausted before the requested number of qualifying items is found.
  - Debug log lines for dequeue-time scoring with item summary, score, and caller context, matching the probability logging expectations introduced by issue #232.
  - QA, regression, and coverage evidence under `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/<kind>/` during implementation and validation.
- Configuration keys and defaults:
  - `HighConfidenceModeEnabled == false` preserves current non-filtering dequeue behavior.
  - `HighConfidenceModeEnabled == true` enables the single dequeue-layer gate.
  - Threshold cutoff remains `(long)Math.Round(threshold * 1000, 0)` unless implementation records and justifies a change.
- Backward-compatibility constraints:
  - No CLI, environment variable, database schema, or external API changes are expected.
  - Public signatures should remain stable where practical, especially `IQfcDatamodel.DequeueNextItemGroupAsync(int quantity, int timeOut)`.
  - Any interface contract change must update every in-repo caller and be documented in the change summary.

## API / CLI Surface

- CLI surface: not applicable. QuickFiler is a VSTO Outlook add-in and this feature has no command-line interface.
- Primary in-repo API surface:
  - `IQfcDatamodel.DequeueNextItemGroupAsync(int quantity, int timeOut)` remains the observable async dequeue contract for page candidates.
  - `IQfcDatamodel.DequeueNextItemGroup(int quantity)` remains the synchronous counterpart unless implementation proves it must be updated.
  - `QfcQueue.EnqueueAsync` continues receiving already-qualified page candidates for hidden/cached panel construction.
  - `IQfcCollectionController.RemoveBelowThresholdAsync(double threshold)` must no longer be invoked by the live high-confidence enforcement path. Its final disposition must be documented for AC8.
- Expected contract examples:
  - High-confidence enabled, request `7`, candidate source has `47` items with `7` qualifying scores interleaved: returns exactly `7` qualifying items after discarding below-threshold candidates.
  - High-confidence enabled, request `7`, source has `3` qualifying items and then exhausts: returns `3` qualifying items without throwing or blocking indefinitely.
  - High-confidence enabled, request `7`, source has no qualifying items and then exhausts: returns an empty list without throwing or blocking indefinitely.
  - High-confidence disabled, request `7`, queue has `5` available items: returns the same count and ordering as the current direct dequeue behavior.
- Validation rules:
  - Invalid or zero requested quantities must preserve current queue behavior unless implementation deliberately records a narrower contract.
  - Cancellation must be observed inside the streaming loop by the existing cancellation token.
  - Dequeue-time threshold comparison is inclusive: `score >= cutoff` qualifies.

## Data & State

- Data flow:
  - Remaining inbox candidates enter the queue without being rejected by high-confidence threshold at admission time.
  - The dequeue layer consumes one candidate at a time, computes the current folder score at dequeue time, compares the score to the configured cutoff, and either returns the candidate or discards it from the high-confidence stream.
  - First-page loading must use the same dequeue-layer confidence decision as subsequent pages. It must not use a fixed unscored batch followed by UI trimming.
  - UI/controller construction receives only candidates already accepted by the dequeue gate when high-confidence mode is enabled.
- State invariants:
  - Exactly one live location gates user-visible items by high-confidence threshold.
  - A surfaced page is stable. After an item is returned from dequeue and placed on a displayed or cached page, later score recomputation must not remove it from that page.
  - Below-threshold candidates discarded by the dequeue gate are not surfaced in the high-confidence stream.
  - Disabled mode preserves current queue item counts and avoids confidence scoring/filtering.
- Caching and persistence:
  - No new persisted data store is required.
  - Existing hidden/cached panel behavior remains the page construction mechanism.
  - Debug logs are diagnostic output only and must not change filing or scoring state.
- Migration or backfill requirements:
  - No data migration is required.
  - Planning must account for prerequisite branch drift: issue #232 commit `90e75ec1` is not present on the current #233 branch according to research. The #232 navigation fix and probability debug logging must be integrated or otherwise preserved before #233 can satisfy AC11 and the navigation constraint.

## Constraints & Risks

- **COM/WinForms boundary and testability.** `QfcDatamodel`, `QfcCollectionController`, and `QfcItemController` are `[ExcludeFromCodeCoverage]` COM/WinForms-bound classes. The redesign must introduce a testable seam for the streaming dequeue/scoring decision (per repo DI-seam guidance: interface seam preferred, then injectable delegate, then adapter) so the filtering/streaming/backfill logic is unit-testable with Moq + FluentAssertions without a live Outlook process. Testable seams within COM-bound assemblies are explicitly NOT coverage-exempt in this repo and must meet the >= 90% new-code target. `QfcRemainingQueueAdmission` is already a small injectable-delegate seam (`scoreLoader`, `addToQueue`, `hookItem`, `removeFromQueue`) and is a natural anchor for the redesign.
- **Performance of scan-many-to-yield-few.** Streaming may score many candidates to yield few. This is acceptable only because it runs on the background/hidden panel, not the live panel. The design must keep the expensive scan off the UI thread (the existing background worker / `Task.Run` pattern in `LoadRemainingEmailsToQueueAsync` and `DequeueNextItemGroupAsync` is the reference) and must not block the user-facing panel while backfilling.
- **Score mutation is real and intentional to embrace.** The classifier retrains as items are filed; dequeue-time scores can differ from admission-time scores. The design intentionally uses the dequeue-time score as authoritative (Proposed Behavior 2 and 4). Do not attempt to "stabilize" scores; the requirement is a single dequeue-time decision, then immutability once surfaced.
- **Do not weaken the crash fix from #232.** #232 fixes the KbdActions navigation-key collision in `QfcCollectionController.LoadControlsAndHandlers_01(TableLayoutPanel, List<QfcItemGroup>)`. This work must not reintroduce or bypass that fix. If the dequeue redesign changes how pages are swapped in, it must preserve the register/unregister navigation pairing #232 established.
- **Termination / liveness.** `WaitForQueue` and any new streaming loop must terminate deterministically when the background worker completes and the source is exhausted; avoid busy-wait or unbounded blocking. Preserve `CancellationToken` propagation (`_token.ThrowIfCancellationRequested()` is used throughout the queue paths).
- **Public surface.** `DequeueNextItemGroupAsync(int quantity, int timeOut)` and `IQfcCollectionController.RemoveBelowThresholdAsync(double)` are on interfaces (`IQfcFormController`, `IQfcCollectionController`). Interface changes require updating all in-repo callers and should be called out. Prefer additive/internal seams over breaking public signatures where practical.
- **Banned APIs.** Repo bans `DateTime.Now`/`UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay` in touched files (BannedApiAnalyzers, `BannedSymbols.txt`). The existing queue code already uses `TimeProvider.Delay` in `WaitForQueue`; keep timing on the injected `TimeProvider` seam.
- **Scope boundary.** This work does NOT include the #232 crash fix or the #232 additive logging (already delivered under #232). It also does not include unrelated queue refactors beyond what the four Proposed Behaviors require.


## Implementation Strategy

- Implementation scope:
  - Create or refactor a focused internal dequeue-layer seam for streaming high-confidence filtering and backfill.
  - Move the live confidence gate out of `QfcRemainingQueueAdmission.TryQueueAsync` so admission does not permanently reject items that could qualify later at dequeue time.
  - Update `QfcDatamodel.DequeueNextItemGroupAsync` to use the streaming gate when `HighConfidenceModeEnabled` is true and preserve current direct dequeue behavior when false.
  - Route the initial page through the same dequeue-layer gate so there is no first-screen fixed batch later trimmed by the UI layer.
  - Remove live threshold enforcement from `QfcFormController.Actions.ApplyHighConfidenceFilterAsync` / `QfcCollectionController.RemoveBelowThresholdAsync` while recording the disposition of the dormant #171 pre-filter path.
  - Preserve issue #232 navigation behavior and probability debug logging; if the branch does not contain issue #232, planning must integrate that prerequisite before final #233 validation.
- Candidate files and functions:
  - `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` for async and sync dequeue behavior.
  - `QuickFiler/Controllers/QfcDatamodel.cs` for queue admission wiring and score-loading integration.
  - `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs` or a successor seam for admission/dequeue responsibilities.
  - A new focused internal seam file under `QuickFiler/Controllers/` if needed to keep controller files cohesive and under policy limits.
  - `QuickFiler/Controllers/QfcFormController.Actions.cs` and `QuickFiler/Controllers/QfcCollectionController.cs` only to remove live post-display threshold trimming and update contracts.
  - Tests under `QuickFiler.Test/Controllers/` using MSTest, Moq, and FluentAssertions.
- Dependency changes:
  - No new NuGet package is expected.
  - Existing TimeProvider support should be reused for waits.
- Logging and telemetry:
  - Preserve issue #232 log lines at `QfcDatamodel.ScoreRemainingQueueMailItemAsync`, `QfcItemController.FolderHandling` scoring points, and `QfcHighConfidencePreFilter.FilterAsync`.
  - Add an equivalent dequeue-time score log with item summary, score, and caller context.
- Rollout plan:
  - Behavior is controlled by the existing high-confidence mode setting.
  - Disabled mode is the fallback path and must match existing ordinary queue behavior.
  - Manual validation should cover a sparse-qualifying inbox and ordinary non-high-confidence OK/Skip/pop-out transitions.

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
- [x] AC10 — Full C# toolchain passes on the final pass (CSharpier → .NET analyzers → nullable/warnings-as-errors → MSTest with coverage). New/changed non-COM-bound code meets the >= 90% coverage target; repository-wide coverage does not regress below 80% on the testable denominator. COM/WinForms-bound touched surfaces rely on the ratified `[ExcludeFromCodeCoverage]` exemption only where already applicable.
- [x] AC11 — The probability debug logging introduced by issue #232 (item summary, score, caller, at `QfcDatamodel.ScoreRemainingQueueMailItemAsync`, `QfcItemController.FolderHandling` scoring points, and `QfcHighConfidencePreFilter.FilterAsync`) remains intact; any new dequeue-time scoring introduced by this work emits an equivalent debug log line (item summary, dequeue-time score, caller context) so score evolution across admission→dequeue is observable.
- [x] AC12 — No unhandled behavioral regression in the ordinary (non-high-confidence) bulk-processing flow: OK/Skip/pop-out page transitions still function; queue draining, `WaitForQueue` termination, and move-monitor hook/unhook semantics are preserved.

## Definition of Done

- [ ] AC1-AC12 remain documented in `spec.md` and `user-story.md` and are mapped to implementation tests, repo-wide search evidence, or manual validation evidence.
- [ ] The live high-confidence path uses a single dequeue-layer threshold gate; evidence records the search proving no other live threshold gate remains.
- [ ] Unit tests cover dequeue-time score selection, scan-many-to-yield-few backfill, zero and partial source exhaustion, exact-threshold inclusivity, disabled-mode parity, cancellation, and no post-display removal.
- [ ] Manual or integration validation covers sparse qualifying items, stable cached/displayed pages, ordinary OK/Skip/pop-out transitions, queue draining, `WaitForQueue` termination, and move-monitor hook/unhook behavior.
- [ ] Issue #232 prerequisite drift is resolved in planning and implementation: navigation changes and probability debug logs are present before final #233 validation.
- [ ] Dequeue-time score logging is added or verified with item summary, score, and caller context.
- [ ] Dormant #171 pre-filter disposition is recorded in the implementation change summary and does not introduce a second live confidence gate.
- [ ] Required C# final pass succeeds in order: CSharpier, .NET analyzers, nullable/warnings-as-errors build, and MSTest with coverage.
- [ ] QA, regression, and coverage evidence is stored under `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/<kind>/`.

## Seeded Test Conditions (from potential)
- [ ] Unit coverage areas:
  - Streaming dequeue decision logic at an injectable seam: request N, mixed above/below-threshold candidate sequence, assert exactly N qualifying returned and below-threshold discarded (positive + the scan-many-to-yield-few case).
  - Boundary: score exactly at threshold qualifies (inclusive); score just below is discarded.
  - Source exhaustion: zero qualifying remaining returns empty without hang/throw; partial (<N) returns all remaining qualifying.
  - Dequeue-time evaluation: same item flips decision based on dequeue-time score vs. an earlier score.
  - No post-display removal: surfaced item survives a below-threshold rescore.
  - Disabled mode: `HighConfidenceModeEnabled == false` yields unchanged counts and no filtering.
  - Cancellation: token cancellation mid-stream is observed and propagates.
- [ ] Integration scenarios (manual, documented; no live Outlook in unit tests):
  - High-confidence run over an inbox where qualifying items are sparse and interleaved: confirm full pages, no empty pages while qualifying items remain, and stable pages (no disappearing items) across OK/Skip/pop-out transitions.
  - Confirm the #232 debug logging shows admission-time vs. dequeue-time scores so drift is observable.
  - Confirm issue #232 navigation behavior remains present when pages are swapped.
- [ ] CLI/API examples: not applicable (VSTO Outlook add-in; no CLI surface).
