# Research — QuickFiler High-Confidence "Initializing Email Queue" Stall (Issue #424)

- Issue: #424
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/424
- Feature folder: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/`
- Timestamp: 2026-08-06T22-00
- Work mode: full-bug
- Author: task-researcher
- Read-only investigation. No production code or tests were modified.

## Evidence Classification Legend

- **[VERIFIED]** — read directly from a repository file in this session, with `file:line` citation.
- **[INFERRED]** — a conclusion drawn from verified facts plus documented .NET/COM behavior. Reasoning and limits are stated.
- **[UNVERIFIED]** — could not be established with the tools available (no build, no debugger, no live Outlook, no git history tool this session).

---

## 1. Direct Answers to the User's Questions

**"Why is it getting stuck for so long?"**
In High Confidence mode the first screen is gated behind `QfcStreamingDequeueConfidenceGate.DequeueAsync`, which scores candidates one at a time — a full COM-backed materialization plus Bayesian folder classification per candidate — and does not return until it has accepted `ItemsPerIteration` items whose score meets the threshold, or the producer has drained the entire folder. The wall-clock cost is therefore proportional to the number of candidates *scanned*, not the number displayed: approximately `ItemsPerIteration / p` serial scoring operations, where `p` is the fraction of folder items scoring at or above `HighConfidenceThreshold`. As `p` falls, the scan set approaches the whole folder. No progress is reported anywhere inside this loop, so the ProgressViewer sits frozen on "Initializing Email Queue" for the entire scan. Normal mode does none of this work before the first screen — it resolves `ItemsPerIteration` items by EntryID and shows the form; scoring happens after `Show()`.

**"Is there a way to streamline and limit the time before the first screen appears?"**
Yes. The smallest change that bounds the wait is a first-batch deadline inside the gate (measured through the already-injected `TimeProvider`), returning whatever high-confidence items were accepted when the deadline expires, plus incremental progress reporting from the gate loop. Partial first batches are already legal in this mode (the gate already returns partial results on source exhaustion, pinned by tests), and background streaming of subsequent groups already exists (`RunAsync` line 313), so the deadline composes with existing semantics rather than changing them. Details and rejected alternatives in §7.

---

## 2. Current State Analysis (whole launch path, upstream first)

### 2.1 Launch sequence and progress-label ownership

`QfcHomeController.LaunchAsync` (`QuickFiler/Controllers/QfcHomeController.cs:38-87`) **[VERIFIED]**:

1. Creates a root `ProgressTracker` (line 59) whose `Initialize()` shows the ProgressViewer (`UtilitiesCS/Threading/ProgressTracker.cs:31-58`).
2. `InitAsync(...)` receives `progress.SpawnChild(86)` (line 68) — 86% of the bar is allocated to data-model construction.
3. `RunAsync(progress.SpawnChild())` (line 72) receives the remaining allocation (14 points, starting at the root's current value).

Entry point is the ribbon: `TaskMaster/Ribbon/RibbonController.cs:118,139` call `LaunchAsync` **[VERIFIED]**. The synchronous `Run()`/`Iterate()` paths (`QfcHomeController.cs:248-272`, `QfcHomeController.Iteration.cs:55-68`) contain the same high-confidence dequeue logic but are not the ribbon path.

### 2.2 Upstream of RunAsync: frame building is *not* part of the stalled label

`InitAsync` (`QfcHomeController.cs:111-153`) starts `QfcDatamodel.LoadAsync` in the background, which reports `(0, "Initializing Data Model")` (`QuickFiler/Controllers/QfcDatamodel.cs:62`), builds the Deedle frame in `InitDfAsync` (`QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs:48-67`), delegating to `DfDeedle.GetEmailDataInViewAsync` with its own progress child (`FrameBuilding.cs:82-89`), and reports `100` on its 86-point child when the frame is filtered and sorted (`FrameBuilding.cs:65`). `DfDeedle` reports its own completion (`UtilitiesCS/Extensions/DfDeedle.cs:199`) **[VERIFIED]**.

Consequence: by the time the label switches to "Initializing Email Queue", `_frame` is fully built. The stall reported in issue #424 is entirely the segment between `QfcHomeController.cs:277` and `:297`. One reporting-contract detail: `progress.Report(0, "Initializing Email Queue")` at line 277 is a report of 0 *on the RunAsync child*; through `ProgressTracker.Report` (`ProgressTracker.cs:141-178`, `parentProgress = allocation*value/100 + startingAt`) the root bar actually displays roughly 86 at this moment, not 0 **[VERIFIED math, display value INFERRED]**. Either way, no report of any kind occurs between lines 277 and 297, which matches the observed frozen label.

### 2.3 The stalled segment, line by line

`QfcHomeController.RunAsync` (`QfcHomeController.cs:274-314`) **[VERIFIED]**:

- Line 277: `progress.Report(0, "Initializing Email Queue")`.
- Lines 279-281: with `HighConfidenceModeEnabled == true`, `initializationBatchSize = 0`.
- Lines 283-290: `InitEmailQueueAsync(0, _formViewer.Worker, ...)` → `InitEmailQueue` short-circuits on `batchSize <= 0` (`QfcDatamodel.cs:238-243`, issue #244 comment): it starts the `BackgroundWorker` producer and returns an empty list immediately. This step is fast.
- Lines 292-295: `listEmail = await _datamodel.DequeueNextItemGroupAsync(itemsPerIteration, 1000)` — **this await is the stall**.
- Line 297: `progress.Report(30, "Initializing Qfc Items")` — the next visible change.
- Line 300: `LoadItemsAsync(listEmail)` → `QfcFormController.LoadItemsAsync` builds controls, then `Show()`s the form (`QuickFiler/Controllers/QfcFormController.Actions.cs:93-102`) before `LoadSecondaryAsync` (line 104).
- Line 313: `await Task.Run(IterateQueueAsync)` — background streaming of the next group already exists (`QfcHomeController.Iteration.cs:11-53`, timeout 2000).

`itemsPerIteration` is not a stored setting; it is computed from screen real estate (`QuickFiler/Controllers/QfcFormController.SetupDisposal.cs:120-147`) — roughly "how many rows fit on screen" **[VERIFIED]**.

### 2.4 The consumer: streaming confidence gate

`DequeueNextItemGroupAsync` routes high-confidence dequeues to `DequeueWithHighConfidenceGateAsync` (`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:55-92`), which constructs a `QfcStreamingDequeueConfidenceGate` with:
- `tryTakeNext = () => _masterQueue.TryTakeFirst()` (line 82),
- `scoreLoader = ScoreRemainingQueueMailItemAsync` (line 83),
- `sourceActive = () => _worker?.IsBusy == true` (line 87) **[VERIFIED]**.

`QfcStreamingDequeueConfidenceGate.DequeueAsync` (`QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs:48-94`) **[VERIFIED]**:
- Loops until `accepted.Count == quantity` (line 63).
- Empty queue: if `timeOut <= 0`, or the queue was already empty once before and `sourceActive` is false, returns partial results (lines 69-73); otherwise waits the **full `timeOut` (1000 ms)** via `TimeProvider.Delay` (lines 76-78) and re-polls. There is no arrival signal; an item arriving 5 ms into the wait still costs 1000 ms.
- Non-empty: awaits a **full per-item score** (line 83) for every candidate, including every candidate it then rejects (line 87: `score >= _cutoff`, cutoff = `threshold * 1000`, line 42). Scoring is strictly serial — one candidate at a time.
- No progress callback exists; the only observability is the per-candidate debug log (lines 96-104), which matches the log snippet in the issue.

### 2.5 Per-candidate scoring cost

`QfcDatamodel.ScoreRemainingQueueMailItemAsync` (`QfcDatamodel.cs:346-360`) constructs a **new `FolderScoringService` per call**. `FolderScoringService.ScoreAsync` (`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:170-189`) runs, per candidate:
1. `MailItemHelper.FromMailItemAsync(...)` — synchronous COM-backed materialization: `TryProjectMailItemMembers(item)` plus `MaterializeTokenizationDependencies()` (`UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Loading.cs:132-167`) **[VERIFIED]**. This reads live Outlook COM properties (the class logs `[MailItem timing] ... COM-backed materialization`).
2. `new FolderPredictor(...)` + `InitAsync(..., FromField)` → `FromFolderKey`: `Suggestions.LoadFromField(...)` and, when no stored field is available, a full `Suggestions.RefreshSuggestions(...)` Bayesian classification (`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:50-69,141-147`) **[VERIFIED]**.

So the per-candidate cost is COM property materialization + (frequently) full classification. The class is annotated as the I/O-boundary adapter that "cannot be exercised by a unit test without live Outlook COM" (`QfcHighConfidencePreFilter.cs:157-166`).

### 2.6 The producer

`InitEmailQueue` starts `_formViewer.Worker`; its `DoWork` handler awaits `RemainingEmailLoader` (`QfcDatamodel.cs:173-200`), defaulted to `LoadRemainingEmailsToQueueAsync(CancellationToken)` (`QfcDatamodel.cs:40,51,288-329`). The producer resolves each frame row **serially**: `GetItemFromID(row.EntryId, row.StoreId)` per row via `Task.Run` (lines 305-308), then admission (`TryQueueRemainingMailItemAsync`, lines 331-344) which appends to `_masterQueue` and hooks the move monitor. `EmailMoveMonitor.HookItem` marshals per-item COM work (`mail.Parent`, `folder.EntryID`, event wiring) onto the captured STA thread (`QuickFiler/Helper Classes/EmailMoveMonitor.cs:46-58`) **[VERIFIED]**.

### 2.7 Admission does not score — confirmed intentional, not a regression

`QfcRemainingQueueAdmission` (`QuickFiler/Controllers/QfcRemainingQueueAdmission.cs:15-46`) **[VERIFIED]**: the constructor null-checks `scoreLoader` (lines 23-26) but never stores or invokes it; `TryQueueAsync` admits unconditionally (lines 34-46). This is pinned as the intended issue #233 design by tests whose failure messages state the contract explicitly: "Remaining-mail admission must not score before queue insertion" and "Threshold scoring belongs to dequeue-time enforcement" (`QuickFiler.Test/Controllers/QfcDatamodelTests.cs:49-100,139-163`) **[VERIFIED]**. The unused `scoreLoader` parameter is dead surface left from the design, not a broken wiring regression. (Git-history confirmation was not possible this session — no shell tool — so "not a regression" rests on the test-pinned contract, **[INFERRED]**.)

### 2.8 Latent defect: `sourceActive` is not a reliable producer-liveness signal

`Worker_DoWork` is `async void` (`QfcDatamodel.cs:173`) and awaits `RemainingEmailLoader` (line 187). A `BackgroundWorker` considers its work finished when the `DoWork` handler *returns*, and an `async void` handler returns at its first yielding await — here `await Task.Run(() => _frame.GetRowsAs<...>...)` (`QfcDatamodel.cs:297`), which yields almost immediately. Consequently `_worker.IsBusy` transitions to false near the start of the load while the loader continues running for the whole folder **[INFERRED from verified code plus documented BackgroundWorker/async-void semantics]**. Effects:

- The gate's `sourceCanStillProduce` (`QueueProcessing.cs:87`) reports false while the producer is still producing. The gate then exits early with a partial batch whenever the consumer finds the queue empty on two polls 1000 ms apart. During the reported stall this rarely triggers because serial scoring is slower than production (the queue is rarely empty), which is why the stall persists instead — but it means the current exit semantics are accidental, and any fix that leans on `sourceActive` (deadline, signaling) must first make this signal honest.
- The same defect affects `WaitForQueue` (`QueueProcessing.cs:130-137`) and causes `QfcHomeController.Worker_RunWorkerCompleted` (`QfcHomeController.cs:316-341`) to enable UI controls before the load is complete. Pre-existing, out of scope here, but the `sourceActive` half interacts directly with this fix.

### 2.9 Normal-mode comparison (why the divergence)

Normal mode (`RunAsync`, `highConfidenceModeEnabled == false`): `InitEmailQueueAsync(itemsPerIteration, ...)` slices the already-built frame and resolves exactly `itemsPerIteration` items by `GetItemFromID` (`QfcDatamodel.cs:245-267`), then goes straight to `LoadItemsAsync` → `Show()`. Pre-UI work ≈ `itemsPerIteration` COM resolutions, no scoring, no polling. Scoring for folder suggestions happens per item controller **after** the form is shown, inside `LoadSecondaryAsync` (`QfcFormController.Actions.cs:100-104`; `QfcItemController.FolderHandling.cs:57-131`). High-confidence mode moves an *unbounded multiple* of that scoring work in front of the first screen. This fully explains the observed divergence **[VERIFIED]**.

---

## 3. Root Cause and Latency Model

**Root cause.** The pre-UI wait in High Confidence mode is `QfcStreamingDequeueConfidenceGate.DequeueAsync` performing serial, per-candidate, COM-bound scoring of the streamed master queue until `ItemsPerIteration` candidates pass the threshold, with (a) no deadline, (b) no progress reporting, (c) a fixed 1000 ms penalty on every empty-queue poll, and (d) a producer throttled to one serial `GetItemFromID` + STA-marshaled move-monitor hook per row.

**Latency model.**

```
T_stall ≈ Σ_{i ∈ scanned} max(t_score_i, t_produce_i · [queue empty]) + n_emptyPolls × 1000 ms
E[|scanned|] ≈ min(N, ItemsPerIteration / p)
```

where `N` = frame row count (latest-email-per-conversation, `FrameBuilding.cs:56-63`), `p` = fraction of items with `score ≥ HighConfidenceThreshold × 1000`, `t_score_i` = COM materialization + classifier run per candidate (§2.5), `t_produce_i` = `GetItemFromID` + `HookItem` per row (§2.6).

**Bounded or unbounded?** The wait is *not* bounded by any constant. It is bounded only by folder size: worst case (no candidate qualifies, `p = 0`) the gate scores every one of the `N` rows before the producer drains and the gate returns a partial (possibly empty) batch after a final 1000 ms poll. It degrades linearly in `1/p` up to that ceiling, and per-candidate cost grows when `FolderScorer.LoadFromField` misses and full `RefreshSuggestions` classification runs (`FolderPredictor.cs:141-147`). With per-candidate costs plausibly in the hundreds of milliseconds (see the `[MailItem timing]` instrumentation, §2.5), scanning tens-to-hundreds of candidates produces the observed multi-minute stall **[INFERRED; magnitudes not measured this session]**.

**Progress display.** Between `QfcHomeController.cs:277` and `:297` there are zero progress reports; the label "Initializing Email Queue" and the bar value are frozen for the entire duration regardless of how much scanning work is done **[VERIFIED]**.

---

## 4. Repeated / Duplicated Work

- **Admission vs gate:** no duplication — admission does not score at all (§2.7). Each rejected candidate is scored exactly once.
- **Gate vs item-controller suggestion path:** each *accepted* item is scored twice. The gate scores it (discarding the computed `TopFolder`, since `ScoreRemainingQueueMailItemAsync` returns only `score.Score`, `QfcDatamodel.cs:346-360`), and after the form is shown the item controller re-runs the identical `MailItemHelper` + `FolderPredictor(FromField)` sequence to populate the folder combo (`QfcItemController.FolderHandling.cs:57-131`). The live path uses the plain `LoadItemsAsync(IList<MailItem>)` overload, not the dormant `QfcPreScoredItem` carrier overload that exists precisely to carry a predetermined folder (`QfcFormController.Actions.cs:107-164`) **[VERIFIED]**. This duplication occurs *after* `Show()`, so it does not extend the pre-UI stall; it is wasted work, not stall cause. Reuse is a valid follow-up, not part of the minimal fix (§7).
- **Per-call service construction:** a new `FolderScoringService` per scored candidate (`QfcDatamodel.cs:351`) and a new `QfcRemainingQueueAdmission` per admitted item (`QfcDatamodel.cs:336-343`). Cheap objects; the dominant cost is inside `ScoreAsync`, not allocation **[VERIFIED]**.

---

## 5. Fate of Rejected Items (correctness findings — report only, no fix here)

1. **Rejected candidates are permanently dropped from the session.** The gate takes them off `_masterQueue` via `TryTakeFirst()` and simply does not add them to `accepted` (`QfcStreamingDequeueConfidenceGate.cs:66,87-90`). They are not requeued and will not appear on any later screen. This is the pinned mode contract (`QfcStreamingDequeueConfidenceGateTests.cs:226-237`, `DequeueAsync_BelowThresholdItemsAreDiscarded`) — the mails themselves remain untouched in the mailbox **[VERIFIED]**.
2. **Accompanying defect — move-monitor hook retention.** Accepted items are unhooked from the move monitor on dequeue (`UnhookDequeuedNodes` → `TryUnhookOrReplace`, `QueueProcessing.cs:107-128`), but rejected items are taken through the bare `TryTakeFirst` delegate and are **never unhooked**. Their `EmailMoveAction` entries — each holding a live `MailItem` COM reference and participating in a `Folder.BeforeItemMove` subscription — remain in `EmailMoveMonitor._hookedItems` until session `Cleanup()` calls `UnhookAll` (`EmailMoveMonitor.cs:46-58`; `QfcDatamodel.cs:75-91`). If such a mail is moved while QuickFiler is open, the hook action `_masterQueue.Remove(x)` fires for an item no longer in the queue (a no-op removal). Impact: session-scoped COM-reference retention proportional to the number of rejected candidates, released at cleanup **[VERIFIED structure; runtime impact INFERRED]**. Recommend tracking as a separate small defect; not fixed by, and not blocking, the latency work.
3. **Latent early-exit defect** via the dishonest `sourceActive` signal (§2.8): under fast scoring / slow production the gate can return a partial or empty first batch long before the folder is exhausted. Distinct failure mode from the stall, same code path.

---

## 6. Existing Coverage Inventory (tests are part of the spec)

| Test file | What it pins | Impact of recommended fix |
|---|---|---|
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` | Gate accept/reject/backfill, inclusive threshold, partial results on exhaustion, discard-below-threshold, cancellation, `FakeTimeProvider`-driven empty-poll waits, `sourceActive` continue-polling (lines 135-298) | **Extend** — new deadline and progress-callback tests; the reflection-based `CreateGate` helper (lines 26-110) must learn the new constructor/parameter shape. Existing assertions remain valid. |
| `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs` | High-confidence `RunAsync` startup: `InitEmailQueueAsync(0, ...)` once; first page from `DequeueNextItemGroupAsync(itemsPerIteration, 1000)` exactly (lines 100-182); disabled-mode overload discipline | **Update** — the exact-argument mock/verify on `DequeueNextItemGroupAsync(itemsPerIteration, 1000)` changes if the call site gains a deadline argument or new overload. |
| `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` | Admission-never-scores contract (lines 49-100, 139-217 — must NOT change); `DequeueNextItemGroupAsync` keeps polling while worker active, pinned via reflection on `BackgroundWorker.isRunning` (lines 103-136); `WaitForQueue`/`ToggleOfflineMode` TimeProvider seams (lines 247-309) | **Update** lines 103-136 if the producer-liveness signal moves off `_worker.IsBusy` (it should, §2.8); admission tests unchanged. |
| `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` | `IterateQueueAsync`/`Iterate` routing; one exact-arg pin `DequeueNextItemGroupAsync(8, 2000)` (line 268) | **Possibly update** if the post-UI iteration call site changes (the minimal fix leaves it alone). |
| `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs` | Issue #244 zero-batch short-circuit + worker start via inert `RemainingEmailLoader` seam | Unchanged. |
| `QuickFiler.Test/Controllers/QfcHighConfidencePreFilterTests.cs`, `QfcHomeControllerIssue218Tests.cs`, `QfcFormControllerTests.cs` (dormant #171/#169 paths) | Dormant pre-filter and post-hoc removal paths | Unchanged (paths remain dormant). |

---

## 7. Recommended Approach

### Primary: bounded first-batch deadline inside the gate, plus incremental progress

**Mechanism.**
1. Give `QfcStreamingDequeueConfidenceGate.DequeueAsync` an overall first-batch budget measured through the already-injected `TimeProvider` (`GetTimestamp()`/`GetElapsedTime`, so `FakeTimeProvider` drives it deterministically). When the budget expires, return `accepted` as-is — exactly the partial-result shape the gate already produces on source exhaustion, so no new result semantics are introduced.
2. Add an optional progress callback to the gate loop (alongside the existing `_debugLog` seam) reporting `(scanned, accepted, quantity)` per iteration. `RunAsync` maps it into the 0→30 band of its progress child so the ProgressViewer advances during the scan and can show, e.g., "Scanning for high-confidence items (12 scanned, 3 accepted)".
3. Make the producer-liveness signal honest: replace `() => _worker?.IsBusy == true` (`QueueProcessing.cs:87`) with a datamodel-owned `volatile bool` set true before `RunWorkerAsync` and false in a `finally` at the end of `LoadRemainingEmailsToQueueAsync(CancellationToken)`. The deadline's exit conditions and the existing exhaustion exit both depend on this signal being truthful (§2.8). This also fixes `WaitForQueue`'s use of the same signal at no extra cost.
4. Optionally shorten the empty-queue poll from 1000 ms to 200 ms to match `WaitForQueue`'s existing cadence (`QueueProcessing.cs:135`) — a one-argument change at the two call sites (`QfcHomeController.cs:294` / `Iteration.cs:23`) that trims tail latency whenever the consumer outruns the producer. Low value relative to the deadline, but nearly free.

**High-confidence selection contract impact.** Unchanged in kind: only items with `score ≥ cutoff` are ever displayed; the ordering (queue order = triage/date sort) is preserved; the batch may contain fewer than `ItemsPerIteration` items, which is already legal (exhaustion partials pinned at `QfcStreamingDequeueConfidenceGateTests.cs:177-190`; no-padding behavior documented in the #233 design). The one observable change: a slow scan now yields a *partial or empty* first screen at the deadline instead of an arbitrarily late full one, with the existing background iteration (`RunAsync` line 313 → `IterateQueueAsync` → `QfcQueue`) continuing to assemble subsequent groups. Candidates not yet scanned when the deadline fires remain in `_masterQueue` — they are not lost; they are simply scanned by later dequeues.

**Deadline location and default.** Recommend an `internal const`/internal property (test seam) rather than a new `QfSettings` member — rationale in §9. A default in the 10–15 s range keeps the first screen bounded while usually admitting several items; the exact value is a planning decision, not a research finding.

**Behavioral risk.** Low. The deadline only converts "wait arbitrarily long for a full batch" into "wait at most D for a possibly-partial batch"; every other path is untouched. The main UX consideration — a first screen with few or zero items on low-yield folders — is inherent to the mode's contract and is mitigated by the progress reporting (the user sees scanning continue) and by the existing streaming of later groups.

**Testability under repo policy.** High. The gate is plain C# with injected `TimeProvider`, delegate seams, and no `[ExcludeFromCodeCoverage]`; deadline behavior is exercised with `FakeTimeProvider.Advance` exactly like the existing wait tests (`QfcStreamingDequeueConfidenceGateTests.cs:240-298`). MSTest + Moq + FluentAssertions throughout; no COM, no temp files, no wall-clock. Note `QfcDatamodel` itself is `[ExcludeFromCodeCoverage]` (`QfcDatamodel.cs:25`), so keep all new logic in the gate (or a small testable helper); the liveness-flag change in `QfcDatamodel` gets correctness-only tests through the established uninitialized-object + seam pattern (`QfcDatamodelTests.cs:219-311`, issue #222 precedent).

**STA thread-affinity interaction (#214/#420).** The fix adds no new threads and no new COM call sites; it only adds time measurement, an early return, and a callback on the existing sequential path, so it cannot reintroduce the #420 class of defect (COM traversal leaving the Outlook STA dispatcher — see `docs/features/active/2026-08-04-folder-tree-dispatcher-thread-affinity-420/issue.md:16,53`). The progress callback must not touch UI directly from the gate; `ProgressTracker.Report` already marshals through the root `Progress<T>` created on the UI thread (`ProgressTracker.cs:47-53`), so routing reports through the existing tracker is safe.

### Production files expected to change (primary + supporting)

| File | Change | Test seam |
|---|---|---|
| `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | Deadline parameter (TimeProvider-based) + optional progress callback | Existing ctor injection: `TimeProvider`, delegates; extend reflection helper in gate tests |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | Pass deadline/progress into the gate; swap `sourceActive` to the liveness flag; (optional) 200 ms poll | `TimeProvider` property seam; uninitialized-object + `SetPrivateField` pattern |
| `QuickFiler/Controllers/QfcDatamodel.cs` | `volatile bool` producer-liveness flag set/cleared around `LoadRemainingEmailsToQueueAsync` | `RemainingEmailLoader` seam (`QfcDatamodel.cs:128`) already exists |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | New `DequeueNextItemGroupAsync` overload carrying deadline/progress (existing overload delegates) | Moq mocks in home-controller tests |
| `QuickFiler/Controllers/QfcHomeController.cs` | Wire deadline + progress mapping in `RunAsync` (0→30 band) | Existing `SetupMockProgressTracker` / mock form controller pattern |

### Rejected alternatives (brief)

- **Bounded-parallel scoring.** Outlook COM objects are apartment-bound; property reads from MTA thread-pool workers marshal back to the single Outlook STA thread, so parallel scoring largely serializes at the STA anyway, gaining only the CPU-side classification overlap while multiplying cross-apartment marshaling and creating exactly the #214/#420 risk class the delegation prompt warns about. High risk, modest gain, and it still does not *bound* the wait. Could be revisited later behind the deadline, not instead of it.
- **Reuse admission-time scores / reactivate the #171 `QfcPreScoredItem` carrier path.** Admission currently does no scoring (§2.7); moving scoring into the (equally serial) producer relocates the same work without bounding it, and reactivating the dormant carrier path (`QfcFormController.Actions.cs:107-164`) is a substantially larger rewiring whose payoff (eliminating the post-`Show()` duplicate scoring, §4) does not shorten the pre-UI stall at all. Worth a follow-up issue for the duplicate-work waste; out of scope for bounding latency.
- **Arrival signal instead of polling.** Requires adding a signaling primitive to `LockingLinkedList<T>` (`UtilitiesCS/ReusableTypeClasses/Locking/LockingLinkedList.cs`) or wrapping the queue; the 1000 ms polls are a secondary cost that only accrues when the consumer outruns the producer, and the 200 ms poll reduction captures most of the benefit for a one-line change. Not worth new concurrency surface now.
- **Overlapping/batching COM resolution in the producer.** The producer is already concurrent with the gate via the worker; measurements are absent, but the scoring side is the demonstrably dominant serial cost (it includes everything the producer does per item plus classification). Batch `GetItemFromID` has no batch API in the Outlook OM; restructuring the producer buys little until scoring itself is bounded.
- **Deadline implemented in `RunAsync` (e.g., `Task.WhenAny` with a timer) instead of inside the gate.** Abandoning the await leaves the gate loop running and scoring concurrently with UI construction, with no way to hand back partially accepted items — the accepted list would be lost. The gate must own the deadline so it can return its partial state cleanly.

---

## 8. Behavior Semantics for the Fix (success/failure/edge cases)

- **Success:** first screen bound: `T(label "Initializing Email Queue" → Show()) ≤ D + ε` where ε covers one in-flight score completing. Progress reports strictly increase during scanning.
- **Deadline with zero accepted:** returns empty list; `LoadItemsAsync(empty)` builds an empty group set and shows the form; `IterateQueueAsync` continues streaming. Verify this path renders acceptably (existing `LoadItemsAsync` null/empty guards at `QfcFormController.Actions.cs:69-79` accept an empty list). Edge case to pin in tests.
- **Deadline expiry mid-score:** the in-flight `scoreLoader` await completes (or is cancelled via token) before return; an accepted item from that final score is included. Define and pin the choice (recommend: include).
- **Cancellation:** unchanged — `token.ThrowIfCancellationRequested()` at loop top and post-score (`QfcStreamingDequeueConfidenceGate.cs:65,84`).
- **Ordering:** accepted items preserve master-queue order (triage/date sort from `SortTriageDate`).
- **Producer exhaustion before deadline:** existing partial-return path unchanged, now driven by an honest liveness flag.

---

## 9. Settings Surface

| Member | Location | Role |
|---|---|---|
| `HighConfidenceModeEnabled` (default false) | `UtilitiesCS/Interfaces/IGlobals/IAppQuickFilerSettings.cs:9`; impl `TaskMaster/AppGlobals/AppQuickFilerSettings.cs:48-56`; ribbon toggle `TaskMaster/Ribbon/RibbonController.Intelligence.cs:53-61` | Routes `RunAsync`, `DequeueNextItemGroupAsync`, `Iterate` |
| `HighConfidenceThreshold` (default 0.9) | `IAppQuickFilerSettings.cs:10`; impl `AppQuickFilerSettings.cs:58-63`; ribbon text field `RibbonController.Intelligence.cs:63-81` | Gate cutoff (`threshold × 1000`) |
| Items per iteration | **Not a setting** — computed from screen space (`QfcFormController.SetupDisposal.cs:120-147`), user-adjustable via the form's items-per-load control | Batch size everywhere |

**Should the new deadline be a setting or a constant?** Recommend an **internal constant with an internal seam** (settable property or parameter default), not a `QfSettings` member. Rationale: (a) the deadline is an implementation quality bound on startup latency, not a user preference — users already control the semantically meaningful knobs (mode, threshold); (b) a new setting costs `Settings.Designer.cs` regeneration, `IAppQuickFilerSettings` surface, ribbon plumbing, and test churn across three projects for a value with no evident per-user variance; (c) an internal seam keeps it fully testable and trivially promotable to a setting later if field feedback shows folders that legitimately need a longer first-batch budget. This mirrors the repo's existing pattern of internal delegate/property seams over configuration (`QfcDatamodel.TimeProvider`, `RemainingEmailLoader`).

---

## 10. Test Strategy (description only; no test code)

- **Gate deadline (new, `QfcStreamingDequeueConfidenceGateTests`):** `FakeTimeProvider`; a score loader whose completion is gated on test-controlled `TaskCompletionSource`s; advance the fake clock past the budget; assert the returned list equals the accepted-so-far set, that no further `tryTakeNext` calls occur after expiry, and that unscanned items remain takeable from the source. Cover: expiry with zero accepted; expiry mid-scan; quantity satisfied before expiry (deadline must not truncate); deadline disabled (null/infinite) preserving current behavior byte-for-byte.
- **Gate progress (new):** callback invoked once per scanned candidate with monotonically non-decreasing `(scanned, accepted)`; no callback after return; callback exceptions must not corrupt the dequeue (decide and pin: swallow-and-log vs propagate — recommend propagate, fail fast per policy).
- **Producer-liveness flag (update `QfcDatamodelTests.cs:103-136`):** replace the `BackgroundWorker.isRunning` reflection pin with the new flag; add a test that the flag is false only after the loader's final item is admitted (drive via the `RemainingEmailLoader` seam).
- **RunAsync wiring (update `QfcHomeControllerRunAsyncHighConfidenceTests`):** verify the new dequeue overload is called with the deadline and a progress sink; verify reports land in the 0→30 band between the two existing label reports; keep the existing "no unfiltered first page" assertions.
- **Unchanged spec:** admission-never-scores tests, discard-below-threshold test, inclusive-threshold test, #244 zero-batch tests.
- All tests: MSTest + Moq + FluentAssertions, `FakeTimeProvider` for all time, mocked `MailItem` (existing pattern `QfcStreamingDequeueConfidenceGateTests.cs:18-24`), no temp files, no live COM, AAA structure.

---

## 11. Out of Scope (explicit)

- Rejected-item move-monitor hook retention (§5.2) — report as a separate defect.
- Duplicate scoring of accepted items after `Show()` (§4) and the dormant #171 carrier path — separate optimization issue.
- `Worker_RunWorkerCompleted` early UI enablement and `WaitForQueue`'s reliance on `IsBusy` beyond the shared flag fix (§2.8) — the flag fix improves both, but no further rework of the `BackgroundWorker` lifecycle.
- The legacy synchronous `Run()`/`Iterate()`/`DequeueNextItemGroup` paths (`QfcHomeController.cs:248-272`, `Iteration.cs:55-68`, `QueueProcessing.cs:94-105`) — not the ribbon path; touching them widens the blast radius without user-visible benefit.
- Any change to frame building (`InitDf*`/`DfDeedle`) — verified not part of the stalled label.

---

## Automation Feasibility

**Fix validation (regression tests): fully automatable, no human interaction required.** The gate, home-controller wiring, and datamodel seams are exercised today by deterministic MSTest suites using Moq-mocked `MailItem`s, delegate-injected scoring, and `FakeTimeProvider` (`QfcStreamingDequeueConfidenceGateTests.cs`, `QfcDatamodelTests.cs`, `QfcHomeControllerRunAsyncHighConfidenceTests.cs`). Every behavior the fix introduces (deadline expiry, partial return, progress cadence, liveness-flag transitions) is drivable from a fake clock and mocked seams; the failing-before/passing-after regression tests for this bug need no Outlook, no mailbox, and no wall-clock time.

**End-to-end reproduction of the original stall: requires human interaction; an automated substitute exists for everything except final UX confirmation.** Observing the literal symptom — ProgressViewer frozen on "Initializing Email Queue" against a real mailbox — requires launching Outlook with the VSTO add-in, enabling High Confidence mode, and starting QuickFiler on a low-yield folder (issue Steps to Reproduce). That cannot be automated under repo policy (no live COM in tests). Substitutes:
1. *Deterministic simulation (automated):* a unit test that streams a low-yield candidate sequence (e.g., 1 qualifier per 50) through the real gate with a fake clock reproduces the unbounded-scan behavior and pins the bound after the fix. This is the authoritative regression evidence.
2. *Log-based verification (semi-automated):* the existing `Probability debug [QfcStreamingDequeueConfidenceGate.DequeueAsync]` per-candidate lines and `[MailItem timing]` / `[Df timing]` instrumentation allow before/after wall-clock comparison from a single manual launch, without a debugger.

**Manual steps, if the maintainer opts to confirm live (optional, not required for merge):** enable High Confidence mode from the ribbon, launch QuickFiler on a folder known to score poorly, confirm the first screen appears within the configured bound and that the progress label advances during scanning. Approximately 5 minutes; no other human interaction is needed anywhere in the validation chain.

---

## Sources

All findings derive from repository files cited inline (read this session). External behavior relied upon: documented .NET `BackgroundWorker` completion semantics with `async void` handlers, COM apartment marshaling for Outlook interop, and `TimeProvider`/`FakeTimeProvider` timestamp APIs — all standard, uncontroversial platform behavior; no external documents were fetched.
