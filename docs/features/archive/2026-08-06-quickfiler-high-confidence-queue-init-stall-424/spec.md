# 2026-08-06-quickfiler-high-confidence-queue-init-stall (Spec)

- **Issue:** #424
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/424
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-06T23-45
- **Status:** Ready for Planning
- **Version:** 1.2 (see Correction Log)
- **Work Mode:** full-bug (AC source: this file only, per `.claude/skills/acceptance-criteria-tracking/SKILL.md`)
- **Research basis:** `research/2026-08-06T22-00-quickfiler-high-confidence-queue-init-stall-research.md` (read in full; all `file:line` citations below are taken from that artifact)

## Context

When QuickFiler runs with High Confidence mode enabled, the ProgressViewer stops at "Initializing Email Queue" for an extended period before the first QuickFiler screen appears. The same startup in normal mode presents its first screen promptly.

Environment:
- OS/version: Windows 11 Pro 10.0.26200
- Runtime: C# / .NET Framework VSTO add-in (no Python involvement)
- Command/flags used: QuickFiler launched from the TaskMaster ribbon with `QfSettings.HighConfidenceModeEnabled = true`
- Data source or fixture: Live Outlook mailbox; the configured QuickFiler source folder with a real message volume

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High: the feature remains functional but the startup latency in High Confidence mode makes the mode impractical for routine use.

## Repro & Evidence

Steps to Reproduce:
1. Enable High Confidence mode in QuickFiler settings (`HighConfidenceModeEnabled = true`, `HighConfidenceThreshold` at its configured value).
2. Launch QuickFiler against a mailbox folder containing a realistic number of messages.
3. Observe the ProgressViewer.

Expected:
The first QuickFiler screen appears within a short, bounded time. Progress reporting advances or otherwise reflects ongoing work, and the pre-UI wait does not grow without limit as the proportion of high-confidence items falls.

Actual:
The ProgressViewer displays "Initializing Email Queue" and stays there for an extended time. Progress remains static for the entire wait; the label only changes once the full first batch of high-confidence items has been assembled.

Logs / Screenshots:
- The log4net `Probability debug [QfcStreamingDequeueConfidenceGate.DequeueAsync]` lines (`QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs:96-104`) emit one entry per scored candidate during the stall, matching the observed log volume.
- Deterministic reproduction for regression evidence does not require a live mailbox: a unit test streaming a low-yield candidate sequence through the real gate with `FakeTimeProvider` reproduces the unbounded scan (research, "Automation Feasibility").

## Scope & Non-Goals

- In scope:
  1. A `TimeProvider`-based first-batch deadline inside `QfcStreamingDequeueConfidenceGate.DequeueAsync` that returns the accepted-so-far set when the deadline elapses.
  2. An incremental progress callback surfaced from the gate loop and mapped by `QfcHomeController.RunAsync` into its 0→30 progress band.
  3. Replacing the `_worker.IsBusy`-based producer-liveness signal (`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:87`) with a datamodel-owned `volatile bool` flag that truthfully tracks `LoadRemainingEmailsToQueueAsync`.
  4. Optional: reducing the fixed 1000 ms empty-queue poll interval at the pre-UI call site (`QuickFiler/Controllers/QfcHomeController.cs:294`) to 200 ms, matching the existing `WaitForQueue` cadence (`QfcDatamodel.QueueProcessing.cs:135`).
  5. Test additions and updates listed in the Test Strategy section.
- Out of scope / non-goals (record only; no fix specified here):
  - **`EmailMoveMonitor` hook retention for gate-rejected items.** Rejected candidates are taken via the bare `TryTakeFirst` delegate and never unhooked; their `EmailMoveAction` entries hold live `MailItem` COM references until session `Cleanup()` (`QuickFiler/Helper Classes/EmailMoveMonitor.cs:46-58`; `QfcDatamodel.cs:75-91`; research §5.2). Follow-up candidate: track as a separate small defect.
  - **Double-scoring of accepted items by the item controller after `Show()`.** The gate discards the computed `TopFolder` and `QfcItemController` re-runs the identical `MailItemHelper` + `FolderPredictor` sequence post-UI (`QfcDatamodel.cs:346-360`; `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:57-131`); the dormant `QfcPreScoredItem` carrier overload (`QuickFiler/Controllers/QfcFormController.Actions.cs:107-164`) exists for future reuse. This occurs after `Show()` and does not extend the pre-UI stall (research §4). Follow-up candidate: separate optimization issue.
  - `Worker_RunWorkerCompleted` early UI enablement and any `BackgroundWorker` lifecycle rework beyond the shared liveness-flag fix (research §2.8).
  - The legacy synchronous `Run()`/`Iterate()`/`DequeueNextItemGroup` paths (`QfcHomeController.cs:248-272`, `QfcHomeController.Iteration.cs:55-68`, `QfcDatamodel.QueueProcessing.cs:94-105`) — not the ribbon path.
  - Frame building (`InitDf*`/`DfDeedle`) — verified not part of the stalled label (research §2.2).
  - Bounded-parallel scoring, arrival signaling on the queue, producer restructuring, and any change to the post-UI iteration call site `QfcHomeController.Iteration.cs:23` (rejected alternatives, research §7).
  - Any new `QfSettings`/`IAppQuickFilerSettings` member (see Technical Specifications).
- Explicitly excluded systems, integrations, or datasets: live Outlook COM in tests; no changes to `UtilitiesCS` scoring/prediction internals (`FolderPredictor`, `MailItemHelper`), the mailbox, or settings persistence.

## Root Cause Analysis

Validated by research (supersedes the preliminary intake in `issue.md`):

- The stall is the segment between `progress.Report(0, "Initializing Email Queue")` at `QuickFiler/Controllers/QfcHomeController.cs:277` and `progress.Report(30, "Initializing Qfc Items")` at `QfcHomeController.cs:297`. Zero progress reports occur between those lines (research §2.2, §3).
- In High Confidence mode `RunAsync` sets `initializationBatchSize = 0` (`QfcHomeController.cs:279-281`); `InitEmailQueue` short-circuits on `batchSize <= 0` and starts the producer (`QfcDatamodel.cs:238-243`). The first UI batch comes entirely from `await _datamodel.DequeueNextItemGroupAsync(itemsPerIteration, 1000)` (`QfcHomeController.cs:292-295`) — that await is the stall.
- `DequeueWithHighConfidenceGateAsync` (`QfcDatamodel.QueueProcessing.cs:55-92`) delegates to `QfcStreamingDequeueConfidenceGate.DequeueAsync` (`QfcStreamingDequeueConfidenceGate.cs:48-94`), which loops until `accepted.Count == quantity` (line 63), awaiting a full per-candidate score (line 83) for every candidate including every rejected one (line 87), strictly serially, with no deadline and no progress reporting.
- Per-candidate cost is COM-backed materialization plus (frequently) full Bayesian classification: `MailItemHelper.FromMailItemAsync` (`UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Loading.cs:132-167`) plus `FolderPredictor.InitAsync`/`RefreshSuggestions` (`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:50-69,141-147`), invoked via a new `FolderScoringService` per call (`QfcDatamodel.cs:346-360`; `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:170-189`).
- Empty-queue polls cost the full 1000 ms `timeOut` each (`QfcStreamingDequeueConfidenceGate.cs:76-78`); there is no arrival signal.
- Latency model (research §3): `E[|scanned|] ≈ min(N, ItemsPerIteration / p)` serial scoring operations, where `p` is the fraction of items scoring at or above `HighConfidenceThreshold × 1000`. The wait is bounded only by folder size; as `p → 0` the scan approaches the whole folder.
- Latent defect that the fix must repair to be sound: `sourceActive = () => _worker?.IsBusy == true` (`QfcDatamodel.QueueProcessing.cs:87`) is dishonest. `Worker_DoWork` is `async void` (`QfcDatamodel.cs:173`) and returns at its first yielding await (`QfcDatamodel.cs:297`), so `IsBusy` goes false near the start of the load while `LoadRemainingEmailsToQueueAsync` (`QfcDatamodel.cs:288-329`) is still producing (research §2.8). Deadline and exhaustion exits both depend on this signal.
- Confirmed non-causes: admission does not score (intentional issue #233 design, test-pinned; `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs:15-46`, research §2.7); frame building completes before the stalled label appears (research §2.2).

## Proposed Fix

### Design summary (what changes where):

1. **First-batch deadline in the gate.** `QfcStreamingDequeueConfidenceGate.DequeueAsync` gains an overall first-batch budget measured through the already-injected `TimeProvider` (`GetTimestamp()`/`GetElapsedTime`). When the budget expires, the method returns `accepted` as-is — the same partial-result shape it already produces on source exhaustion (pinned at `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs:177-190`), so no new result semantics are introduced. Candidates not yet scanned remain in `_masterQueue` and are scanned by later dequeues; they are not lost.
2. **Incremental progress callback.** The gate loop invokes an optional callback (alongside the existing `_debugLog` seam) reporting `(scanned, accepted, quantity)` once per scanned candidate. `QfcHomeController.RunAsync` maps it into the 0→30 band of its progress child so the ProgressViewer advances during the scan (for example, "Scanning for high-confidence items (12 scanned, 3 accepted)").
3. **Honest producer-liveness signal.** `QfcDatamodel` owns a `volatile bool` producer-liveness flag: set `true` before `RunWorkerAsync`, cleared `false` in a `finally` at the end of `LoadRemainingEmailsToQueueAsync(CancellationToken)`. `sourceActive` at `QfcDatamodel.QueueProcessing.cs:87` consumes this flag instead of `_worker?.IsBusy`. `WaitForQueue` (`QueueProcessing.cs:130-137`) benefits from the same flag at no extra cost; no further `BackgroundWorker` lifecycle rework.
4. **Optional poll reduction (O1).** Reduce the pre-UI empty-queue poll from 1000 ms to 200 ms at `QfcHomeController.cs:294` only. The post-UI iteration call site (`QfcHomeController.Iteration.cs:23`, pinned by the exact-argument test at `QfcHomeControllerIterationTests.cs:268`) is left unchanged. If the planner drops O1, no acceptance criterion is affected.

### Boundaries and invariants to preserve:

- **High-confidence selection contract unchanged in kind:** only items with `score ≥ cutoff` (`threshold × 1000`, `QfcStreamingDequeueConfidenceGate.cs:42`) are ever accepted; the inclusive-threshold and discard-below-threshold behaviors (`QfcStreamingDequeueConfidenceGateTests.cs:226-237`) hold. Accepted items preserve master-queue order (triage/date sort).
- **Partial batches remain legal:** a batch may contain fewer than `quantity` items, exactly as on source exhaustion today. The only observable change is that a slow scan yields a possibly-partial first screen at the deadline instead of an arbitrarily late full one; background iteration (`QfcHomeController.cs:313` → `IterateQueueAsync`) continues assembling subsequent groups.
- **Admission never scores:** the issue #233 contract (`QfcDatamodelTests.cs:49-100,139-217`) is untouched.
- **Cancellation semantics unchanged:** `token.ThrowIfCancellationRequested()` at loop top and post-score (`QfcStreamingDequeueConfidenceGate.cs:65,84`) is preserved.
- **STA thread-affinity (#214/#420):** no new threads, no new COM call sites. The progress callback must not touch UI directly from the gate; reports route through the existing `ProgressTracker`, whose root `Progress<T>` is created on the UI thread and marshals for us (`UtilitiesCS/Threading/ProgressTracker.cs:47-53`).
- **Coverage boundary:** `QfcDatamodel` is `[ExcludeFromCodeCoverage]` (`QfcDatamodel.cs:25`). All new decision logic lives in the gate (or a small testable helper), not in `QfcDatamodel`; the datamodel receives only the flag set/clear and wiring.

### Dependencies or blocked work:

- None blocking. `TimeProvider` is already injected into the gate and `QfcDatamodel` (existing seams). No new packages, no settings regeneration, no schema changes.
- Follow-up candidates to file separately (not blocking): rejected-item move-monitor hook retention; post-`Show()` duplicate scoring / `QfcPreScoredItem` carrier reuse.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

| File | Change | Test seam |
|---|---|---|
| `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | `TimeProvider`-based first-batch deadline + optional progress callback in `DequeueAsync` | Existing ctor injection (`TimeProvider`, delegates); extend the reflection-based `CreateGate` helper (`QfcStreamingDequeueConfidenceGateTests.cs:26-110`) |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | Pass deadline/progress into the gate; swap `sourceActive` (line 87) to the liveness flag; (O1) 200 ms poll pass-through | `TimeProvider` property seam; uninitialized-object + `SetPrivateField` pattern (`QfcDatamodelTests.cs:219-311`) |
| `QuickFiler/Controllers/QfcDatamodel.cs` | `volatile bool` producer-liveness flag set before `RunWorkerAsync`, cleared in `finally` at end of `LoadRemainingEmailsToQueueAsync` | Existing `RemainingEmailLoader` seam (`QfcDatamodel.cs:128`) |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | New `DequeueNextItemGroupAsync` overload carrying deadline/progress; existing overload delegates to it | Moq mocks in home-controller tests |
| `QuickFiler/Controllers/QfcHomeController.cs` | Wire deadline + progress mapping into `RunAsync` (0→30 band); (O1) poll argument at line 294 | Existing `SetupMockProgressTracker` / mock form-controller pattern |

#### Functions/classes/CLI commands impacted:

- `QfcStreamingDequeueConfidenceGate.DequeueAsync` — deadline exit condition and per-iteration progress invocation.
- `QfcDatamodel.DequeueWithHighConfidenceGateAsync`, `QfcDatamodel.WaitForQueue` — consume the new liveness flag.
- `QfcDatamodel.InitEmailQueue` / `Worker_DoWork` / `LoadRemainingEmailsToQueueAsync` — flag set/clear only; no behavioral rework.
- `QfcHomeController.RunAsync` — new dequeue overload call and progress mapping. No CLI surface exists.

#### Data flow and validation changes:

- Deadline flows: internal constant in the gate (with an internal seam) → `DequeueAsync` exit condition. It is not read from settings and is not user-visible data.
- Progress flows: gate loop → callback `(scanned, accepted, quantity)` → `RunAsync` mapping → `ProgressTracker.Report` (`ProgressTracker.cs:141-178`) → ProgressViewer. Mapped values are clamped to the 0→30 band and must be monotonically non-decreasing.
- Constructor validation follows the gate's existing guard-clause pattern (reject non-positive deadline unless the disabled sentinel is used; null callback means no reporting).

#### Error handling and logging updates:

- Progress-callback exceptions **propagate** (fail fast per policy); they are not swallowed. This choice is pinned by a test.
- Existing per-candidate debug logging (`QfcStreamingDequeueConfidenceGate.cs:96-104`) is retained unchanged. A single debug-level log line on deadline expiry (accepted count, scanned count) uses the existing `_debugLog` seam; no ad-hoc console output.
- Cancellation continues to surface as `OperationCanceledException`; no broad catches are added.

#### Rollback/feature-flag considerations (if applicable):

- No feature flag. The deadline-disabled sentinel (see Technical Specifications) preserves current behavior exactly, providing a trivial in-code rollback and a test baseline. `HighConfidenceModeEnabled = false` remains the user-level escape hatch and is untouched.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

- `DequeueAsync` contract after the change: returns `accepted` when (a) `accepted.Count == quantity` (unchanged fast path), (b) the source is exhausted per the honest liveness signal (unchanged partial path), or (c) the first-batch deadline elapses (new partial path, same result shape). **Deadline expiry mid-score:** the in-flight `scoreLoader` await completes (or is cancelled via the token) before return; an item accepted by that final score **is included** in the returned batch (pinned choice, research §8).
- Progress callback: invoked once per scanned candidate with `(scanned, accepted, quantity)`; `scanned` and `accepted` are monotonically non-decreasing; no invocation occurs after `DequeueAsync` returns.
- `IQfcDatamodel.DequeueNextItemGroupAsync`: existing overload keeps its exact signature and delegates to the new overload with the default deadline and a null progress sink.

#### Required configuration keys and defaults:

- **None.** The deadline is an `internal const` (with an internal property or parameter-default seam for tests), **not** a `QfSettings`/`IAppQuickFilerSettings` member. Rationale (research §9): it is an implementation quality bound, not a user preference; a setting would cost `Settings.Designer.cs` regeneration, interface surface, and ribbon plumbing across three projects; the internal seam keeps it testable and promotable to a setting later. Default value: within the 10–15 second range; the exact constant is selected at planning time and recorded in the plan. A disabled sentinel (for example `Timeout.InfiniteTimeSpan` or an internal opt-out) must reproduce current behavior for baseline tests.

#### Backward-compatibility expectations:

- Existing `IQfcDatamodel.DequeueNextItemGroupAsync(int, int)` callers compile and behave unchanged apart from the bounded wait. No public API removal. `QfSettings` schema unchanged. Normal (non-high-confidence) mode is untouched (`QfcDatamodel.cs:245-267` path).

#### Performance constraints (latency/throughput/memory):

- Bound: `T(label "Initializing Email Queue" → Show()) ≤ D + ε`, where `D` is the deadline and `ε` covers one in-flight score completing (research §8). Verified deterministically with `FakeTimeProvider`, not wall-clock measurement.
- The fast path (quantity satisfied before deadline) must not regress: no added awaits, allocations proportional to scan count only.
- No new threads; no additional COM traffic per candidate.

## Assumptions, Constraints, Dependencies

- Assumptions: `FakeTimeProvider` (Microsoft.Extensions.TimeProvider.Testing) remains available to the test project, as used by existing gate tests (`QfcStreamingDequeueConfidenceGateTests.cs:240-298`); the ribbon `LaunchAsync` path is the delivery target (`TaskMaster/Ribbon/RibbonController.cs:118,139`).
- Constraints: policy order per `CLAUDE.md` and `.claude/rules/` (`general-code-change.md`, `general-unit-test.md`, `csharp.md`, `architecture-boundaries.md`); bugfix workflow (failing regression test first, minimal targeted fix); MSTest + Moq + FluentAssertions only; no live Outlook COM, no temporary files, no wall-clock sleeps in tests; file size ≤ 500 lines; no new dependencies.
- External dependencies: none added.

## Data / API / Config Impact

- User-facing or API changes: first screen in High Confidence mode may appear with fewer than `ItemsPerIteration` items (or zero) when the deadline fires; the ProgressViewer advances during scanning. Internal interface `IQfcDatamodel` gains one overload.
- Data or migration considerations: none. No mailbox mutation changes; rejected candidates continue to be session-dropped per the pinned mode contract (`QfcStreamingDequeueConfidenceGateTests.cs:226-237`); unscanned candidates remain queued.
- Logging/telemetry updates: one debug-level deadline-expiry line via the existing `_debugLog` seam; existing per-candidate probability logging unchanged.
- Compatibility notes: no CLI flags, no config schema, no versioning impact.

## Test Strategy

All tests: MSTest (`[TestClass]`/`[TestMethod]`), Moq, FluentAssertions, `FakeTimeProvider` for all time, mocked `MailItem` (existing pattern `QfcStreamingDequeueConfidenceGateTests.cs:18-24`), AAA structure, no temp files, no live COM, no wall-clock waits.

### Existing tests that pin current behavior (part of this spec)

| Test file | Pins | Spec expectation |
|---|---|---|
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` (lines 135-298) | Gate accept/reject/backfill, inclusive threshold, partial results on exhaustion (177-190), discard-below-threshold (226-237), cancellation, `FakeTimeProvider` empty-poll waits, `sourceActive` continue-polling | **Extend.** Existing assertions remain valid and passing. Add deadline and progress-callback tests; the reflection-based `CreateGate` helper (26-110) learns the new constructor/parameter shape. |
| `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs` (lines 100-182) | High-confidence `RunAsync` startup: `InitEmailQueueAsync(0, ...)` once; first page from `DequeueNextItemGroupAsync(itemsPerIteration, 1000)` exactly; disabled-mode overload discipline | **Update.** The exact-argument mock/verify changes to the new overload (deadline + progress sink; 200 ms if O1 adopted). The "no unfiltered first page" assertions are kept. |
| `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` | Admission-never-scores contract (49-100, 139-217); `DequeueNextItemGroupAsync` keeps polling while worker active, pinned via reflection on `BackgroundWorker.isRunning` (103-136); `WaitForQueue`/`ToggleOfflineMode` TimeProvider seams (247-309) | **Update lines 103-136 only** — replace the `BackgroundWorker.isRunning` reflection pin with the new liveness flag, and add a test that the flag is false only after the loader's final item is admitted (driven via the `RemainingEmailLoader` seam). **Admission tests (49-100, 139-217) must NOT change.** |
| `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` (line 268) | `IterateQueueAsync`/`Iterate` routing; exact-arg pin `DequeueNextItemGroupAsync(8, 2000)` | **Unchanged.** The post-UI iteration call site is out of scope; this pin must keep passing as-is. |
| `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs` | Issue #244 zero-batch short-circuit + worker start via inert `RemainingEmailLoader` seam | **Unchanged.** |
| `QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs` | Issue #218 intent: `RunAsync_HighConfidenceEnabled_DoesNotPreFilterInitialGuiBatch` and `RunAsync_HighConfidence_LoadsInitialBatchWithoutPreFilter` assert directly on the in-scope `RunAsync` high-confidence dequeue call — they `Setup` (lines 101, 192) and `Verify` (lines 160, 226) the two-argument `DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>())` overload | **Update — overload shape only.** The `Setup` and `Verify` argument matchers change from the two-argument overload to `DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<System.Action<int, int, int>>())`, and nothing else in the file changes: the `preFilterInvoked` assertion, both `LoadItemsAsync` overload-discipline assertions, and the `Times.Once` counts are preserved, so the issue #218 intent the file exists to pin is unchanged. |
| `QuickFiler.Test/Controllers/QfcHighConfidencePreFilterTests.cs`, `QfcFormControllerTests.cs` (dormant #171/#169 paths) | Dormant pre-filter and post-hoc removal paths | **Unchanged.** These two files are genuinely dormant and pass unmodified. |

### Regression tests to add or update

- **Gate deadline (new, in `QfcStreamingDequeueConfidenceGateTests`):** score loader gated on test-controlled `TaskCompletionSource`s; advance `FakeTimeProvider` past the budget; assert the returned list equals the accepted-so-far set, no further `tryTakeNext` calls occur after expiry, and unscanned items remain takeable from the source. Cases: expiry with zero accepted; expiry mid-scan (final in-flight accept included); quantity satisfied before expiry (deadline must not truncate); deadline-disabled sentinel preserving current behavior.
- **Gate progress (new):** callback invoked once per scanned candidate with monotonically non-decreasing `(scanned, accepted)`; no callback after return; a throwing callback propagates (fail fast).
- **Producer-liveness flag (update `QfcDatamodelTests`):** flag true across the `async void` first-await boundary while the loader still runs; false only after loader completion; `sourceActive` consumes the flag.
- **RunAsync wiring (update `QfcHomeControllerRunAsyncHighConfidenceTests`):** new overload called with deadline and progress sink; mapped reports land within the 0→30 band, monotonically non-decreasing, between the two existing label reports; empty-batch deadline result still reaches `LoadItemsAsync` and the form path (empty-list guard at `QfcFormController.Actions.cs:69-79`).
- Bugfix workflow: at least one deadline regression test must be demonstrated failing before the fix (low-yield stream through the current gate, `FakeTimeProvider` advanced past the intended budget, asserting bounded return) and passing after.

### Edge cases and negative scenarios

- Zero qualifying items before deadline → empty list, form still shown, background iteration continues.
- Deadline expiry during an in-flight score → completes/cancels deterministically; accepted final item included.
- Producer exhaustion before deadline → existing partial-return path, now driven by the honest flag.
- Cancellation during scan and during empty-poll wait → `OperationCanceledException`, unchanged.
- Invalid deadline construction input → guard-clause rejection.

### Error handling and logging verification

- Test that a progress-callback exception propagates out of `DequeueAsync` without corrupting gate state observable to the caller.
- Deadline-expiry debug line emitted through the `_debugLog` seam (assert via injected delegate, not log capture).

### Coverage impact and targets

- Blocking, per `.claude/rules/csharp.md` scoped to what this change controls: new/changed modules and methods ≥ 90%; no coverage regression on changed lines. The repository-wide 80% floor applies to the testable denominator per `CLAUDE.md` § UT2 (after the ratified COM/VSTO/WinForms/Outlook-Interop exemptions); the raw repo-wide rate was already below that floor at the merge-base (70.19% line / 58.30% branch, `evidence/baseline/test-coverage-baseline.2026-08-06T22-31.md`) and is recorded-and-reported rather than blocking — this change must not lower it. All new decision logic is placed in the gate/helper specifically because `QfcDatamodel` is `[ExcludeFromCodeCoverage]` (`QfcDatamodel.cs:25`); datamodel flag correctness is tested via the established uninitialized-object + seam pattern (`QfcDatamodelTests.cs:219-311`).

### Toolchain commands (format → lint → type-check → test)

1. `dotnet tool run csharpier .` (or `csharpier .`)
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

Restart from step 1 if any step fails or changes files.

### Manual validation steps (optional, not required for merge)

- Enable High Confidence mode from the ribbon, launch QuickFiler on a low-yield folder, confirm the first screen appears within the configured bound and the progress label advances during scanning (~5 minutes; research "Automation Feasibility").

## Acceptance Criteria

All criteria are verifiable by deterministic MSTest tests using Moq, FluentAssertions, and `FakeTimeProvider` — no live Outlook COM, no temporary files, no wall-clock sleeps.

- [x] `QfcStreamingDequeueConfidenceGate.DequeueAsync` enforces a first-batch deadline measured through the injected `TimeProvider`: with score completion held open via `TaskCompletionSource` and `FakeTimeProvider` advanced past the budget, the call returns the accepted-so-far set instead of continuing to scan, no `tryTakeNext` calls occur after expiry, and unscanned candidates remain takeable from the source.
- [x] When zero candidates reach the cutoff before the deadline, `DequeueAsync` returns an empty list at the deadline bound, and the `RunAsync` path proceeds to show the form with an empty first group (empty-list guard path at `QfcFormController.Actions.cs:69-79`), with background iteration still initiated.
- [x] When `quantity` items are accepted before the deadline elapses, the returned batch is identical in content and order to pre-change behavior; the deadline neither truncates nor delays the satisfied fast path, and the deadline-disabled sentinel reproduces current behavior on the existing gate test scenarios.
- [x] A score in flight when the deadline expires completes (or is cancelled via the token) before return, and an item accepted by that final score is included in the returned batch.
- [x] The gate invokes the progress callback once per scanned candidate with `(scanned, accepted, quantity)` where `scanned` and `accepted` are monotonically non-decreasing, no callback invocation occurs after `DequeueAsync` returns, and a throwing callback propagates its exception.
- [x] `QfcHomeController.RunAsync` maps gate progress into its 0→30 band: reports emitted between the "Initializing Email Queue" and "Initializing Qfc Items" reports are within [0, 30] and monotonically non-decreasing.
- [x] The producer-liveness signal is a datamodel-owned `volatile bool` that remains true across the `async void` `Worker_DoWork` first-await boundary while `LoadRemainingEmailsToQueueAsync` is still producing, becomes false only after the loader completes (cleared in a `finally`), and is the signal consumed by `sourceActive` in `QfcDatamodel.QueueProcessing.cs` in place of `_worker?.IsBusy`.
- [x] Cancellation semantics are preserved: cancelling the token during scanning or during an empty-queue wait surfaces `OperationCanceledException`, and the existing gate cancellation tests pass unchanged.
- [x] The high-confidence selection contract is unchanged: items scoring below the cutoff are never accepted, the inclusive-threshold and discard-below-threshold tests pass unchanged, accepted items preserve master-queue order, and the admission-never-scores tests (`QfcDatamodelTests.cs:49-100,139-217`) pass without modification.
- [x] The deadline is an internal constant with an internal test seam; no new `QfSettings`/`IAppQuickFilerSettings` member, no `Settings.Designer.cs` change, and no ribbon plumbing are introduced.
- [x] At least one new deadline regression test is evidenced failing before the fix and passing after (fail-before/pass-after evidence under `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/regression-testing/`).
- [x] The genuinely unchanged pins remain byte-unmodified and passing: `QfcHomeControllerIterationTests.cs` exact-arg pin (`DequeueNextItemGroupAsync(8, 2000)`, line 268), `QfcInitEmailQueueZeroBatchTests.cs`, `QfcHighConfidencePreFilterTests.cs`, and `QfcFormControllerTests.cs`; and `QfcHomeControllerIssue218Tests.cs` passes with its diff limited to the four overload-shape hunks (the `Setup`/`Verify` matchers at lines 101, 192, 160, 226 moving to `DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<System.Action<int, int, int>>())`), preserving the `preFilterInvoked` assertion, both `LoadItemsAsync` overload-discipline assertions, and the `Times.Once` counts.
- [x] The full C# toolchain passes in order without errors — CSharpier, .NET analyzers (`EnableNETAnalyzers`/`EnforceCodeStyleInBuild`), nullable build (`Nullable=enable`, `TreatWarningsAsErrors=true`), and MSTest via `vstest.console.exe /EnableCodeCoverage` — with **no coverage regression on changed lines** and **≥ 90% coverage on the new and changed modules and methods**: `QfcScanProgressBandMapper.cs`, `QfcStreamingDequeueConfidenceGate.cs`, and the changed methods in `QfcDatamodel.cs`, `QfcDatamodel.QueueProcessing.cs`, `QfcHomeController.cs`, and `IQfcDatamodel.cs`. Additionally, the repository-wide line and branch rates are recorded and reported as part of this criterion — the merge-base baseline (70.19% line / 58.30% branch, per `evidence/baseline/test-coverage-baseline.2026-08-06T22-31.md`) alongside the post-change figures — together with an explicit statement that the raw repo-wide figure was already below the 80% policy floor at the merge-base (the `CLAUDE.md` § UT2 floor applies to the testable denominator after the ratified COM/VSTO/WinForms/Outlook-Interop exemptions, not to the raw uninstrumented denominator) and that this change does not lower it.

## Risks & Mitigations

- **Partial or empty first screen on low-yield folders.** Inherent to bounding the wait under the mode's contract. Mitigated by incremental progress reporting (the user sees scanning continue) and by existing background streaming of subsequent groups (`QfcHomeController.cs:313`). Pinned by the zero-accepted AC.
- **Liveness-flag change alters exit timing of existing paths.** The current `IsBusy` signal is already dishonest (research §2.8); making it truthful removes an accidental early-exit mode. Mitigated by updating the pinned polling test (`QfcDatamodelTests.cs:103-136`) deliberately and adding flag-transition tests.
- **Progress callback touching UI off-thread.** Prohibited by design; reports route through `ProgressTracker`, which marshals via the UI-thread `Progress<T>` (`ProgressTracker.cs:47-53`). No new threads or COM call sites are added, avoiding the #214/#420 defect class.
- **Test-helper churn.** The reflection-based `CreateGate` helper must track the new constructor shape; contained within the gate test file.
- Rollback: the deadline-disabled sentinel restores current behavior in code; `HighConfidenceModeEnabled = false` restores normal-mode startup for users.

## Rollout & Follow-up

- Release/rollout steps: standard branch flow on `bug/quickfiler-high-confidence-queue-init-stall-424`; no migration, no configuration rollout.
- Post-fix monitoring or clean-up tasks: optional single manual launch comparing before/after wall-clock via the existing `Probability debug` and `[MailItem timing]` log lines; file follow-up issues for (a) rejected-item `EmailMoveMonitor` hook retention (research §5.2) and (b) post-`Show()` duplicate scoring / `QfcPreScoredItem` carrier reuse (research §4).
- Links: issue https://github.com/drmoisan/TaskMaster/issues/424; `issue.md` (this folder); research `research/2026-08-06T22-00-quickfiler-high-confidence-queue-init-stall-research.md`; promoted intake `docs/features/potential/promoted/2026-08-06-quickfiler-high-confidence-queue-init-stall.md`.

## Correction Log

- **2026-08-06T23-00 (Version 1.0 → 1.1, corrected during execution).** Version 1.0 of this spec misclassified `QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs` as "Unchanged (dormant)", grouping it with the genuinely dormant #171/#169 suites. That classification was wrong: both of its tests assert directly on the in-scope `RunAsync` high-confidence dequeue call via `Setup`/`Verify` of the two-argument `DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>())` overload, which this spec's own Technical Specifications retire from the pre-UI call site in favor of the four-argument overload. Because the mock is loose, the contradiction surfaced at run time — both tests failed (`Moq.MockException: Expected invocation on the mock once, but was 0 times`) at the Phase 5 pinned-suite gate during plan execution, while the production behavior was verified correct by `RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue`. Corrections applied in place: the Test Strategy table now carries a dedicated "Update — overload shape only" row for `QfcHomeControllerIssue218Tests.cs` (matcher shape change only; issue #218 intent preserved), the remaining two files of the former dormant group are recorded as genuinely dormant and unmodified, and AC 12 was reworded so it remains a verifiable gate (byte-unmodified pins plus a diff-limited pass for the reclassified file) rather than an unsatisfiable one. The AC count is unchanged at 13 and no criterion was checked off. This entry records that the spec was corrected deliberately in response to the execution finding, not retrofitted silently to match the implementation.
- **2026-08-06T23-45 (Version 1.1 → 1.2, corrected during execution).** AC 13 originally read: "The full C# toolchain passes in order without errors — CSharpier, .NET analyzers (`EnableNETAnalyzers`/`EnforceCodeStyleInBuild`), nullable build (`Nullable=enable`, `TreatWarningsAsErrors=true`), and MSTest via `vstest.console.exe /EnableCodeCoverage` — with repository line coverage ≥ 80%, ≥ 90% coverage on new/changed modules, and no coverage regression on changed lines per `.claude/rules/csharp.md`." The repository-wide clause was unsatisfiable before this branch existed: the captured merge-base baseline is **70.19% line / 58.30% branch** (`evidence/baseline/test-coverage-baseline.2026-08-06T22-31.md`), so the criterion as written could never be checked off and would have been a dead gate. The orchestrator's resolution, encoded here: the repository-wide 80% floor in `CLAUDE.md` and `.claude/rules/csharp.md` applies to the **testable denominator** after the ratified COM/VSTO/WinForms/Outlook-Interop exemptions in `CLAUDE.md` § UT2; the raw uninstrumented repo-wide figure is not that denominator, and the shortfall is pre-existing debt at the merge-base that a bug fix is not the vehicle for retiring. AC 13 was reworded so that the conditions this change actually controls remain strict and blocking — full toolchain pass in order, no coverage regression on changed lines, and ≥ 90% on the new and changed modules and methods (`QfcScanProgressBandMapper.cs`, `QfcStreamingDequeueConfidenceGate.cs`, and the changed methods in `QfcDatamodel.cs`, `QfcDatamodel.QueueProcessing.cs`, `QfcHomeController.cs`, `IQfcDatamodel.cs`) — while the repository-wide line and branch rates (baseline and post-change) become a record-and-report obligation within the criterion itself, including the explicit statement that the repo-wide figure was already below the 80% floor at the merge-base and that this change does not lower it. The "Coverage impact and targets" bullet in Test Strategy was aligned to the same scoping so the spec does not contradict itself. This is a deliberate, reasoned correction of a mis-scoped clause, not a lowering of the quality bar: every gate within this change's control is unchanged or stated more precisely, and the pre-existing shortfall is made visible in the delivered artifact rather than hidden. The AC count remains 13; no criterion other than AC 13 was changed; no check-off state was altered (AC 1-8 and 11 were already `[x]` before this edit and remain so).

## Acceptance Criteria Status

Recorded by `atomic-executor` at plan completion per `.claude/skills/acceptance-criteria-tracking/SKILL.md`.

- Source: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/spec.md`
- Total AC items: 13
- Checked off (delivered): 13
- Remaining (unchecked): 0
- Items remaining: none

Full per-criterion traceability to plan tasks, verifying test methods, and evidence artifacts is recorded in `evidence/qa-gates/ac-mapping.2026-08-07T00-52.md`. Each criterion was checked off individually as its mapped tasks passed verification, not batched at the end: AC 11 after [P1-T4], AC 4 after [P1-T5], AC 3 after [P1-T6], AC 1 and AC 8 after [P1-T9], AC 5 after [P2-T5], AC 7 after [P3-T8], AC 2 and AC 6 after [P4-T9], AC 12 after [P5-T1], AC 9 after [P5-T1], AC 10 after [P5-T3], AC 13 after [P6-T5].
