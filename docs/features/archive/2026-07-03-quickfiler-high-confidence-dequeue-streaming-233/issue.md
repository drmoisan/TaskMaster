# quickfiler-high-confidence-dequeue-streaming (Issue #233)

- Date captured: 2026-07-03
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-high-confidence-dequeue-streaming/ (Issue #233)

> Handoff note: This entry is authored to be self-contained. A separate orchestrator will plan and execute it with no additional verbal context. All architectural claims below were verified by source read on branch `TaskMaster-wt-2026-07-03-10-11` at HEAD `00507b59` on 2026-07-03. Supporting research: `artifacts/research/2026-07-03T00-00-quickfiler-kbdactions-duplicate-key-research.md` (Investigation 2). Related project memory: `.claude/agent-memory/task-researcher/project_qfc_high_confidence_dual_pipeline.md`. This work was split out of bug issue #232 (KbdActions duplicate-key crash + additive probability logging); #232 remains scoped to the crash fix and logging only and does NOT change any filtering/dequeue behavior. The probability debug logging added by #232 (at the three scoring call sites named below) is a prerequisite diagnostic aid for this work and is expected to be already present when this work begins.

- Issue: #233
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/233
- Last Updated: 2026-07-03
- Work Mode: full-feature

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

## Proposed Behavior

Redesign high-confidence mode so filtering is the sole responsibility of the queue/dequeue layer, applied at dequeue time, streaming, with backfill. The four required behaviors:

1. **Single filtering location — the Queue only.** High-confidence filtering must live only in the queue/dequeue layer. Remove the high-confidence filter from every other location: the post-hoc `ApplyHighConfidenceFilterAsync`/`RemoveBelowThresholdAsync` removal pass in the UI load path (Issue #169), and any first-screen special-casing that loads an unfiltered fixed batch. The UI/collection layer must not add, remove, or re-filter items on the basis of confidence. After this change there is exactly one place in the code where the high-confidence threshold gates which items reach the user.

2. **Filter at Dequeue time, not at entry.** Today, admission to `_masterQueue` applies the threshold when an item enters the queue (`QfcRemainingQueueAdmission.TryQueueAsync`). Move the confidence gate so it is evaluated at the moment of Dequeue rather than at entry. Rationale: an item's probability can change substantially as earlier items are filed and the classifier retrains, so the score that matters is the one computed when the item is about to be surfaced, not when it was first admitted. (The implementing orchestrator must decide whether the master queue should hold all candidates unfiltered and gate at dequeue, or retain admission but re-score at dequeue; the required observable outcome is that the threshold decision reflects a dequeue-time score.)

3. **Streaming dequeue with backfill until the requested count is satisfied.** When the dequeue function is asked for N items in high-confidence mode, it must return N items that meet the threshold (or exhaust the source trying). It streams: dequeue a candidate, score it at dequeue time, discard it if below threshold, and continue pulling candidates until N qualifying items have been collected or no candidates remain. Example: to return 7 high-confidence items it may need to dequeue and evaluate 47 candidates, discarding 40. The extra processing time is acceptable because this runs in the background for the hidden/cached panel, not the live panel the user is interacting with. Discarded (below-threshold) items are removed from the high-confidence stream; they are not surfaced to the user.

4. **No removal after an item is surfaced to the UX.** Once an item has been returned by the dequeue and placed on a page shown (or cached for showing) to the user, it must remain on that page even if its recomputed confidence later drops below the threshold at filing time. The confidence gate applies exactly once, at dequeue time. There is no post-display re-check that can make an already-surfaced item disappear. This eliminates the "item vanished from the page" class of behavior and makes each page stable once built.

Net effect: in high-confidence mode, every page the user sees is full (up to the per-iteration size) of items that met the threshold at the time they were dequeued, with no empty pages while qualifying items remain in the inbox, and no items disappearing from a page after it is shown. When high-confidence mode is disabled, behavior is unchanged.

## Acceptance Criteria (early draft)

- [ ] AC1 — High-confidence filtering exists in exactly one location (the queue/dequeue layer). The post-hoc removal path (`ApplyHighConfidenceFilterAsync` → `RemoveBelowThresholdAsync`) is no longer invoked to enforce the confidence threshold in the live flow, and no first-screen path loads an unfiltered fixed batch that is later trimmed by confidence. A repo-wide search shows no confidence-threshold comparison outside the single dequeue-layer location (excluding the dormant #171 pre-filter, whose disposition is recorded under AC8).
- [ ] AC2 — The confidence threshold is evaluated at dequeue time. A unit test demonstrates that an item whose dequeue-time score is >= threshold is returned even if a different (earlier) score would have rejected it, and an item whose dequeue-time score is < threshold is discarded, with the decision driven by the dequeue-time measurement.
- [ ] AC3 — Streaming backfill: when N items are requested in high-confidence mode and the candidate source contains at least N qualifying items interleaved with below-threshold items, the dequeue returns exactly N qualifying items (all >= threshold), having discarded the below-threshold candidates it encountered. A unit test covers the "must scan many to yield few" case (e.g., request 7, source arranged so ~40 below-threshold candidates are interleaved).
- [ ] AC4 — Source-exhaustion boundary: when fewer than N qualifying items remain, the dequeue returns all remaining qualifying items (0..N-1) without blocking indefinitely and without throwing. A unit test covers the zero-qualifying-remaining case (returns empty, does not hang, does not throw) and the partial case.
- [ ] AC5 — No post-display removal: after an item is returned by the dequeue and placed on a page, a subsequent recomputation of its score below the threshold does not remove it from that page. A unit test demonstrates a surfaced item remains present after a simulated below-threshold rescore.
- [ ] AC6 — Empty-page regression: a scenario reproducing the reported symptom (qualifying items sparse relative to page size) yields full pages of qualifying items up to the per-iteration size and no empty page while qualifying items remain. Expressed as a logical-level test at the queue/dequeue seam (no live Outlook).
- [ ] AC7 — Disabled-mode parity: when `HighConfidenceModeEnabled == false`, dequeue behavior is unchanged from today (no filtering, no streaming discard, same item counts). A unit test asserts parity.
- [ ] AC8 — Disposition of the two pipelines is explicit: the live path is the redesigned dequeue-layer filter; the dormant Issue #171 pre-filter (`QfcHighConfidencePreFilter`, `HighConfidencePreFilterLoader`, `LoadItemsAsync(IList<QfcPreScoredItem>)`) is either wired to the new single location or explicitly retired/left dormant with a recorded decision. The change description states which and why. No third filtering pipeline is introduced.
- [ ] AC9 — Threshold semantics preserved: the boundary remains inclusive (score == threshold qualifies), matching the existing `>=` keep / `<` reject convention. Cutoff scaling (`Math.Round(threshold * 1000, 0)`, long comparison) is preserved or its change is justified.
- [ ] AC10 — Full C# toolchain passes on the final pass (CSharpier → .NET analyzers → nullable/warnings-as-errors → MSTest with coverage). New/changed non-COM-bound code meets the >= 90% coverage target; repository-wide coverage does not regress below 80% on the testable denominator. COM/WinForms-bound touched surfaces rely on the ratified `[ExcludeFromCodeCoverage]` exemption only where already applicable.
- [ ] AC11 — The probability debug logging introduced by issue #232 (item summary, score, caller, at `QfcDatamodel.ScoreRemainingQueueMailItemAsync`, `QfcItemController.FolderHandling` scoring points, and `QfcHighConfidencePreFilter.FilterAsync`) remains intact; any new dequeue-time scoring introduced by this work emits an equivalent debug log line (item summary, dequeue-time score, caller context) so score evolution across admission→dequeue is observable.
- [ ] AC12 — No unhandled behavioral regression in the ordinary (non-high-confidence) bulk-processing flow: OK/Skip/pop-out page transitions still function; queue draining, `WaitForQueue` termination, and move-monitor hook/unhook semantics are preserved.

## Constraints & Risks

- **COM/WinForms boundary and testability.** `QfcDatamodel`, `QfcCollectionController`, and `QfcItemController` are `[ExcludeFromCodeCoverage]` COM/WinForms-bound classes. The redesign must introduce a testable seam for the streaming dequeue/scoring decision (per repo DI-seam guidance: interface seam preferred, then injectable delegate, then adapter) so the filtering/streaming/backfill logic is unit-testable with Moq + FluentAssertions without a live Outlook process. Testable seams within COM-bound assemblies are explicitly NOT coverage-exempt in this repo and must meet the >= 90% new-code target. `QfcRemainingQueueAdmission` is already a small injectable-delegate seam (`scoreLoader`, `addToQueue`, `hookItem`, `removeFromQueue`) and is a natural anchor for the redesign.
- **Performance of scan-many-to-yield-few.** Streaming may score many candidates to yield few. This is acceptable only because it runs on the background/hidden panel, not the live panel. The design must keep the expensive scan off the UI thread (the existing background worker / `Task.Run` pattern in `LoadRemainingEmailsToQueueAsync` and `DequeueNextItemGroupAsync` is the reference) and must not block the user-facing panel while backfilling.
- **Score mutation is real and intentional to embrace.** The classifier retrains as items are filed; dequeue-time scores can differ from admission-time scores. The design intentionally uses the dequeue-time score as authoritative (Proposed Behavior 2 and 4). Do not attempt to "stabilize" scores; the requirement is a single dequeue-time decision, then immutability once surfaced.
- **Do not weaken the crash fix from #232.** #232 fixes the KbdActions navigation-key collision in `QfcCollectionController.LoadControlsAndHandlers_01(TableLayoutPanel, List<QfcItemGroup>)`. This work must not reintroduce or bypass that fix. If the dequeue redesign changes how pages are swapped in, it must preserve the register/unregister navigation pairing #232 established.
- **Termination / liveness.** `WaitForQueue` and any new streaming loop must terminate deterministically when the background worker completes and the source is exhausted; avoid busy-wait or unbounded blocking. Preserve `CancellationToken` propagation (`_token.ThrowIfCancellationRequested()` is used throughout the queue paths).
- **Public surface.** `DequeueNextItemGroupAsync(int quantity, int timeOut)` and `IQfcCollectionController.RemoveBelowThresholdAsync(double)` are on interfaces (`IQfcFormController`, `IQfcCollectionController`). Interface changes require updating all in-repo callers and should be called out. Prefer additive/internal seams over breaking public signatures where practical.
- **Banned APIs.** Repo bans `DateTime.Now`/`UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay` in touched files (BannedApiAnalyzers, `BannedSymbols.txt`). The existing queue code already uses `TimeProvider.Delay` in `WaitForQueue`; keep timing on the injected `TimeProvider` seam.
- **Scope boundary.** This work does NOT include the #232 crash fix or the #232 additive logging (already delivered under #232). It also does not include unrelated queue refactors beyond what the four Proposed Behaviors require.

## Test Conditions to Consider

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
- [ ] CLI/API examples: not applicable (VSTO Outlook add-in; no CLI surface).

## Current Architecture / Code References (verified 2026-07-03, HEAD 00507b59)

- Live entry: `QfcHomeController.Run()`/`RunAsync()` — `QuickFiler/Controllers/QfcHomeController.cs:248-293`.
- First-batch, no-score-check load: `QfcDatamodel.InitEmailQueue` — `QuickFiler/Controllers/QfcDatamodel.cs:211-238`; `InitEmailQueueAsync` — `:240-256`.
- Post-hoc removal (Issue #169) applied in UI load path: `QfcFormController.Actions.cs:104-110` (call), `:180-191` (`ApplyHighConfidenceFilterAsync`); `QfcCollectionController.RemoveBelowThresholdAsync` — `:1059-1079`.
- Admission-time gate (to be moved to dequeue): `QfcRemainingQueueAdmission.TryQueueAsync` — `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs:33-56`; wiring `QfcDatamodel.TryQueueRemainingMailItemAsync` — `:301-314`; `LoadRemainingEmailsToQueueAsync` — `:258-299`; admission-time scoring `ScoreRemainingQueueMailItemAsync` — `:316-326`.
- Dequeue (no backfill today): `QfcDatamodel.DequeueNextItemGroupAsync` — `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:55-110`; sync variant `:112-131`; `WaitForQueue` — `:133-140`.
- Dormant pre-filter (Issue #171): `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` (`FilterAsync`); loader `QfcHomeController.cs:236-244`; carrier load `QfcFormController.Actions.cs:120-171`.
- Threshold source: `_globals.QfSettings.HighConfidenceModeEnabled` / `HighConfidenceThreshold`; cutoff scaling `(long)Math.Round(threshold * 1000, 0)`.
- Display-time scoring (relevant to score-mutation and no-post-display-removal): `QfcItemController.FolderHandling.cs:27-111`; `TopFolderScore` — `QfcItemController.cs:254`.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/quickfiler-high-confidence-dequeue-streaming/` folder from the template
