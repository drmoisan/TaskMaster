# 2026-09-06-quickfiler-high-confidence-cancel-teardown-and-deadline-defects (Spec)

- **Issue:** #791
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-06T12-57
- **Status:** Ready for implementation
- **Version:** 1.0
- **Work Mode:** full-bug. This spec is the sole authoritative acceptance-criteria source. `user-story.md` in this folder is narrative operator context only and carries no criteria.

> Path-formatting note: the backticked repository paths in the Write Set below are the change
> footprint for this fix. Everywhere else in this document, file paths and File.cs:123 line citations are
> written as plain prose on purpose. Do not add backticks to them. The single exception is the runbook
> path inside AC2, which is quoted verbatim from issue.md.

## Context
Two defects observed while running QuickFiler in High Confidence mode on 2026-09-06 against the build of `7c8ac9ae`. (1) A High Confidence run whose first 12 seconds of scanning finds no item at or above the cutoff opens an empty dialog, and because scan order follows the Explorer view the same view produces the same empty dialog on every rerun. (2) The Cancel teardown does not shut QuickFiler down cleanly: the background queue loader outlives Cancel and crashes on fields that cleanup has already nulled, the keyboard-active flag and WebView2 focus are never reset on the Cancel path, the teardown chain has no `try`/`finally`, and the whole path emits no log output, which left a 37 minute unexplained gap during which the Outlook keyboard was locked.

Environment:
- OS/version: Windows 11 Pro 10.0.26200
- Runtime: C# / .NET Framework 4.8 VSTO add-in (no Python component)
- Command/flags used: QuickFiler launched from the ribbon High Confidence button; HighConfidenceThreshold at the designer default 0.9 (never changed in any user.config on the machine); HighConfidenceModeEnabled toggled by the ribbon launch path
- Data source or fixture: live Outlook Inbox view; add-in loaded from TaskMaster\bin\Debug built 2026-09-06 08:51 from `7c8ac9ae`

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High: the deadline defect makes High Confidence mode unusable for any view whose top-scoring items are not near the front, with no message and no recovery other than filing items some other way. The teardown defect can leave the whole Outlook keyboard unusable until Outlook is restarted, and the surviving background loader crashes against the next launch's state.

## Repro & Evidence
Steps to Reproduce — Defect 1 (deadline policy, deterministic for a given view):
1. Arrange an Explorer view whose first roughly 40 items in view order all score below 900 per-mille while later items score above it.
2. Launch QuickFiler via the High Confidence ribbon button.
3. Observe the dialog open with zero rows after roughly 20 seconds.
4. Cancel and relaunch via the same button; observe the same empty dialog.

Steps to Reproduce — Defect 2 (Cancel teardown, sporadic):
1. Launch QuickFiler via the High Confidence ribbon button and file one round of suggestions.
2. File a second round, then press Undo repeatedly (24 undo clicks were logged between 09:04:05 and 09:05:53).
3. Press Cancel.
4. Observe the Outlook keyboard is unusable in the native Outlook window. In a separate run the same Cancel left the background loader running until it crashed 4 seconds after the next launch.

Expected:
- A High Confidence run that has scored items but found none at or above the cutoff within the first-batch deadline keeps scanning until the first acceptance or until the candidate queue is exhausted, subject to a hard cap on scanned items, and reports progress. It never opens an empty dialog while unscanned candidates remain.
- The cutoff in effect and the scan progress are logged at launch and at every deadline decision.
- Cancel performs a complete, ordered teardown: cancellation is signalled, the background loader is stopped and awaited before any datamodel field is nulled, form and item keyboard handlers are unregistered before the item rows are removed, the keyboard-active flag is reset, WebView2 focus is parked and any open breadcrumb dropdown is cancelled (the same routine that FormViewer_Deactivated runs), and the ribbon release callback runs even if an earlier step throws.
- Every stage of the Cancel teardown writes a log line through the existing log4net pattern, including any exception.

Actual:
- QfcStreamingDequeueConfidenceGate.DequeueAsync returns DeadlineExpired with an empty accepted list when accepted.Count == 0 after 12 seconds, and QfcHomeController.RunAsync loads zero rows. Scores were real, not zero: the three zero-accepted runs peaked at 928 and 960 *after* the deadline had already expired, while accepting runs peaked at 997 to 1000. The cutoff (900) is never logged.
- After Cancel, QfcDatamodel.Cleanup() cancels the token and calls worker.CancelAsync() but does not await LoadRemainingEmailsToQueueAsync, then nulls _moveMonitor, _globals, _masterQueue and _worker. The still-running loader then throws at QfcDatamodel.cs:355-358 while constructing QfcRemainingQueueAdmission from those fields.
- ActionCancelAsync (QfcFormController.EventHandlers.cs:84-93) does not reset KbdActive, does not call ParkFocusOffWebView2() or CancelBreadcrumbSelector(), and has no `try`/`finally`. ButtonCancel_Click is `async void` and rethrows, so an escaping exception becomes an unhandled Outlook UI-thread failure.
- QfcFormController.Cleanup() (QfcFormController.SetupDisposal.cs:213-261) unregisters form event handlers after _groups.Cleanup() has already removed the item rows, so the recursive unsubscribe no longer reaches the item controls' PreviewKeyDown/KeyDown subscriptions.
- QfcHomeController.Cleanup() (QfcHomeController.cs:370-379) invokes ParentCleanup with no `try`/`finally`; if the datamodel cleanup throws, RibbonController.ReleaseQuickFiler() never runs and both ribbon buttons become no-ops. _tokenSource is never disposed and Worker_RunWorkerCompleted is never detached.
- The Cancel path, QfcDatamodel.Cleanup() and ParkFocusOffWebView2() contain no logging. After the 09:05:53 undo burst the log is silent for 37 minutes 39 seconds until the next launch at 09:43:32.

Logs / Screenshots:
- [x] Attached minimal logs or screenshot
- Snippet (from TaskMaster\bin\Debug\logs\debug_2026-09-06.log):

```
2026-09-06 09:43:54,214 [44] DEBUG QfcStreamingDequeueConfidenceGate - First-batch deadline expired [DequeueAsync] Accepted=0 Scanned=38 Deadline=00:00:12
2026-09-06 09:45:36,727 [53] DEBUG QfcStreamingDequeueConfidenceGate - First-batch deadline expired [DequeueAsync] Accepted=0 Scanned=44 Deadline=00:00:12
2026-09-06 10:08:06,149 [29] DEBUG QfcStreamingDequeueConfidenceGate - First-batch deadline expired [DequeueAsync] Accepted=0 Scanned=42 Deadline=00:00:12
2026-09-06 10:08:10,910 [5] ERROR QuickFiler.Controllers.QfcDatamodel - LoadRemainingEmailsToQueue Error.
      Delegate to an instance method cannot have null 'this'.
   at System.MulticastDelegate.CtorClosed(Object target, IntPtr methodPtr)
   at QuickFiler.Controllers.QfcDatamodel.<TryQueueRemainingMailItemAsync>d__41.MoveNext() ... QfcDatamodel.cs:line 355
   at QuickFiler.Controllers.QfcDatamodel.<LoadRemainingEmailsToQueueAsync>d__40.MoveNext() ... QfcDatamodel.cs:line 330
2026-09-06 10:08:10,985 [5] ERROR QuickFiler.Controllers.QfcDatamodel - Error in Worker_DoWork Delegate to an instance method cannot have null 'this'.
```

Timeline evidence (same log): launches at 08:52:09 (accepted, rows at 08:53:39), 09:43:32 (Accepted=0), 09:45:14 (Accepted=0), 10:04:26 (accepted), 10:05:12 (accepted), 10:07:45 (Accepted=0). Undo burst 09:04:05 to 09:05:53, then no log output until 09:43:32.

## Scope & Non-Goals
- In scope: the production files listed in the Write Set (gate, IQfcDatamodel, both QfcDatamodel partials, the QfcFormController EventHandlers and Deactivate partials, QfcHomeController), the new and retargeted MSTest files, and the `<Compile Include>` entries required for new files.
- Out of scope / non-goals (paths below are deliberately unbackticked; they are not part of the change footprint):
  - QuickFiler/Controllers/QfcCollectionController.cs — 2329 lines, a pre-existing violation of the 500-line limit. UnregisterNavigation is already on IQfcCollectionController (line 109) and is called from the Cancel path instead of being added to that file's Cleanup.
  - QuickFiler/Controllers/QfcHomeController.Iteration.cs — the SourceExhausted-only CompleteAddingAsync branch is already correct and is preserved unchanged (#446 AC-6).
  - TaskMaster/Ribbon/RibbonController.cs — ReleaseQuickFiler stays private with no test seam; the guarantee is expressed at the QfcHomeController.ParentCleanup boundary.
  - TaskMaster/Properties/Settings.Designer.cs and TaskMaster/AppGlobals/AppQuickFilerSettings.cs — no new user-facing setting is introduced.
  - QuickFiler/Controllers/QfcFormController.SetupDisposal.cs — the existing Cleanup body and the #731 deferred undo-queue disposal are untouched; the ordering defect is corrected by calling the existing unregister methods earlier from the Cancel path.
- Explicitly excluded systems, integrations, or datasets: the breadcrumb WebView2 initialization failure (`Breadcrumb CoreWebView2 initialization failed ... 0x8007139F`, observed 08:55:22 and 10:06:51), filed separately as issue #792; the SpamBayes/Triage scoring engines; Outlook COM automation in tests.

## Root Cause Analysis
- Gate loop QfcStreamingDequeueConfidenceGate.cs:168-237: the deadline is evaluated only while accepted.Count == 0 (:172-176) and returns DeadlineExpired at :179. scanned++ at :205 runs only after the score loader returns, so `Scanned=38 Accepted=0` means 38 completed scores all strictly below _cutoff (:129, per-mille). Rejected items leave the session queue permanently (:182, :215-232), so a rerun rescans the same view prefix — hence the reported determinism.
- The empty-queue wait path (:185-196) does not increment scanned, so an item cap alone cannot bound the pre-UI wait while the loader is still refilling. A time ceiling is required in addition.
- The 12 second first-batch deadline was introduced by #424 and adjusted by #446 and #608; those changes handled the post-UI iteration and the undersized-batch cases, not the zero-accepted first batch.
- Worker_DoWork (QfcDatamodel.cs:175-213) is `async void` and retains no handle to the loader task, so nothing can await it. LoadRemainingEmailsToQueueAsync observes the token only at :322 and :324; TryQueueRemainingMailItemAsync then dereferences _masterQueue and _moveMonitor at :355-359 with no null guard.
- Keyboard mechanism: no SetWindowsHookEx, AddMessageFilter or KeyPreview exists anywhere in the repo. #677 identified WebView2 focus retention and an open breadcrumb ToolStripDropDown as the mechanism and fixed it on the Form.Deactivate path only; the Cancel path unsubscribes that event.
- Related closed issues: #424, #446, #608 (deadline lineage), #677 (Deactivate focus fix), #731 (controller lifecycle disposal), #737 (breadcrumb keyboard navigation). All are on `7c8ac9ae`.
- Unknown: whether the 09:05 keyboard lock cleared on Escape, on focus change, or only on restart. The Cancel-stage logging added here is what makes a future occurrence diagnosable.

## Proposed Fix

### Design summary (what changes where):

**AC1 — advisory checkpoint plus a hard scan bound.** The zero-acceptance branch at gate:172-180 becomes a checkpoint instead of a return. _firstBatchDeadline is re-purposed as the checkpoint interval: on expiry the gate logs the cutoff, scanned, accepted count, elapsed time and the remaining bounds, resets the interval origin, and continues scanning. Two bounds terminate the extended scan: maxScanWithoutAcceptance (default 250 scored candidates) and zeroAcceptanceCeiling (default 120 seconds). Both are gate-internal `internal static readonly` defaults with an optional constructor parameter as the test seam — no new setting. A new stop reason `QfcDequeueStop.ScanCapReached` reports the bounded exit and is treated exactly as DeadlineExpired is treated today (the queue stays open). `DeadlineExpired` is retained as an enum member with its XML doc updated to record that #791 made the deadline advisory. A launch log line at the top of DequeueAsync carries the cutoff, quantity, checkpoint interval and both bounds. IterateQueueAsync still calls CompleteAddingAsync only under SourceExhausted.

Superseded prior criteria, stated deliberately rather than regressed silently:
- #424 spec acceptance criterion at docs/features/archive/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/spec.md:231 ("When zero candidates reach the cutoff before the deadline, `DequeueAsync` returns an empty list at the deadline bound...") is **superseded by #791 AC1**.
- #608 spec acceptance criterion at docs/features/active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/spec.md:184 ("Deadline expiry with `accepted.Count == 0` retains the current empty-result behavior...") is **superseded by #791 AC1**. #608's other criteria (:181-183, :185) concern the non-empty prefix and must remain green.

**AC2 — ordered, logged, exception-safe teardown.**
1. Worker_DoWork captures the loader task in a `_remainingLoadTask` field before awaiting it. A new `IQfcDatamodel.QuiesceLoaderAsync(TimeSpan)` cancels, then awaits the loader against a TimeProvider delay of the supplied bound and logs whether the loader completed or the bound expired. The field and method live in the QueueProcessing partial. It is awaited from ActionCancelAsync through _parent.DataModel — **never** a blocking wait inside Cleanup(), which #731 established runs on the UI thread.
2. TryQueueRemainingMailItemAsync is relocated to the QueueProcessing partial, snapshots _masterQueue and _moveMonitor into locals, and returns false when either is null or cancellation is requested.
3. QfcDatamodel.Cleanup() null-guards its _globals / _moveMonitor dereferences so a second Cancel cannot throw before the fields are released.
4. FormViewer_Deactivated is split into the event handler plus `internal void ParkFocusAndCancelSelectors()`, called from both the event and the Cancel path, with the per-item boundary catch intact.
5. ActionCancelAsync is reordered to: (1) log entry; (2) signal cancellation; (3) marshal to the UI sync context; (4) reset KbdActive, toggling only when active; (5) ParkFocusAndCancelSelectors() while the item groups still exist; (6) _groups?.UnregisterNavigation() and UnregisterFormEventHandlers() before rows are removed; (7) Hide(); (8) await QuiesceLoaderAsync; (9) _groups?.Cleanup(); (10) Cleanup(), which reaches ParentCleanup. Each stage-group is wrapped so a throwing stage cannot skip a later one, and the release callback runs under `finally`. Repeat invocation (double Cancel, or Cancel after the MoveAndIterate completion path, which calls the same method) is inert.
6. QfcHomeController.Cleanup() wraps the datamodel cleanup, the field nulling and the Worker_RunWorkerCompleted detach in guarded blocks with logging, disposes _tokenSource, and invokes ParentCleanup in a `finally`.
7. ButtonCancel_Click no longer rethrows. This is a deliberate behavior change: an `async void` rethrow becomes an unhandled Outlook UI-thread exception, which is the failure mode the logging requirement replaces.

**Invariant established by this fix (single sentence):** after ActionCancelAsync returns, no background loader work can observe a nulled QfcDatamodel field — the loader has either completed or been bounded out and its admission path returns false rather than constructing a delegate over a null instance — and RibbonController.ReleaseQuickFiler has been invoked exactly once regardless of which teardown stage threw.

**Trace of one accepted value (the reported crash):**
1. *Accept point* — LoadRemainingEmailsToQueueAsync (QfcDatamodel.cs:322, :324) checks only the cancellation token and passes a MailItem to TryQueueRemainingMailItemAsync. It does not validate _masterQueue or _moveMonitor, and no guard exists anywhere between here and the throw.
2. *Throw point* — QfcDatamodel.cs:355-359 constructs QfcRemainingQueueAdmission over _masterQueue.AddLast and _moveMonitor.HookItem; once Cleanup() has nulled either field, delegate construction raises ArgumentException "Delegate to an instance method cannot have null 'this'".
3. *Current absorption point* — the exception is caught and logged as "LoadRemainingEmailsToQueue Error." and again as "Error in Worker_DoWork". Because Worker_DoWork is `async void` and the form is already hidden, neither location can report to the operator, abort the teardown, or prevent the loader from surviving into the next launch (the logged crash occurred 4 seconds after a relaunch).
4. *Where the fix moves the decision* — the awaited QuiesceLoaderAsync in ActionCancelAsync is an `async` boundary that can both wait and report before any field is nulled, and the relocated guard in TryQueueRemainingMailItemAsync returns false at the accept point instead of throwing at the throw point.

Why neither half suffices alone: the quiesce await alone leaves the crash reachable on any future path that nulls fields without awaiting (for example the MoveAndIterate completion path, or a partially-failed launch), and the null guard alone silently truncates queue loading that was still legitimately in flight while giving the teardown no completion point to observe. Both are required, and both are pinned by tests.

### Boundaries and invariants to preserve:
- #446 AC-6: CompleteAddingAsync is invoked only under SourceExhausted. The new ScanCapReached stop reason must not be routed into that branch, and QfcHomeController.Iteration.cs is not modified.
- #608's non-empty-prefix criteria (:181-183, :185): the deadline remains inert once accepted.Count > 0; inclusive `score >= _cutoff` qualification, below-cutoff discard, accepted-message ordering and cancellation propagation are unchanged.
- #731 disposal design, untouched: the deferred undo-queue disposal via _undoQueueDisposal in QfcFormController.SetupDisposal.cs:207-249; the one-monitor-per-owner design and comment at QfcDatamodel.cs:104-105; the three-delegate QfcRemainingQueueAdmission constructor. Do not convert the quiesce into a blocking wait inside Cleanup() — that is the deadlock #731 finding 4 rejected.
- #677: ParkFocusOffWebView2 and the per-item CancelBreadcrumbSelector loop keep their existing bodies and their existing Form.Deactivate wiring; only the extraction of the shared routine is new.
- Cancellation semantics: cancelling during the extended scan still surfaces OperationCanceledException; `quantity <= 0` still short-circuits.
- Which catches must not be widened: the per-item boundary catch inside the deactivate routine stays per-item (a broader catch would hide a single failing selector); the gate's score-loader call site keeps propagating OperationCanceledException, pinned by the existing gate cancellation tests.

### Dependencies or blocked work:
None. All prerequisite work (#424, #446, #608, #677, #731, #737) is closed and present on `7c8ac9ae`. The live-Outlook confirmation is a human follow-up and does not block the automated review.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

Write Set — production:
- `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`
- `QuickFiler/Interfaces/IQfcDatamodel.cs`
- `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`
- `QuickFiler/Controllers/QfcDatamodel.cs`
- `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`
- `QuickFiler/Controllers/QfcFormController.Deactivate.cs`
- `QuickFiler/Controllers/QfcHomeController.cs`

Write Set — tests (new):
- `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs`
- `QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs`
- `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs`
- `QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs`

Write Set — tests (retargeted):
- `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs`
- `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs`
- `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs`
- `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs`

Write Set — project files:
- `QuickFiler.Test/QuickFiler.Test.csproj` — `<Compile Include>` entries for the four new test files
- `QuickFiler/QuickFiler.csproj` — an entry is required only if implementation introduces a new production file; the mapping above adds none

#### Functions/classes/CLI commands impacted:
DequeueAsync, LogDeadlineExpiry and the new launch-log helper on the gate; the QfcDequeueStop enum and the IQfcDatamodel contract; Worker_DoWork, Cleanup, TryQueueRemainingMailItemAsync and the new QuiesceLoaderAsync on QfcDatamodel; ActionCancelAsync and ButtonCancel_Click on QfcFormController; FormViewer_Deactivated split into the handler plus ParkFocusAndCancelSelectors; QfcHomeController.Cleanup. No CLI surface exists in this component.

#### Data flow and validation changes:
The gate's scan loop gains two counters checked at the same point as the existing checkpoint, before the take, so a bounded scan cannot take an extra item. The queue take, admission, scoring and progress-callback contracts are unchanged. TryQueueRemainingMailItemAsync gains a null/cancellation precondition that returns false instead of throwing. No persisted data, schema or file format changes.

#### Error handling and logging updates:
All lines use the existing log4net ILog idiom on each class; no new logger shape. Levels are chosen so a normal Cancel is readable at INFO and diagnosis is available at DEBUG.

| Stage | Level | Content |
| --- | --- | --- |
| Gate launch | DEBUG | cutoff (per-mille and fraction), quantity, checkpoint interval, scan cap, ceiling |
| Gate checkpoint | DEBUG | accepted, scanned, cutoff, elapsed, remaining cap/ceiling, decision (continue / stop) |
| Cancel entry | INFO | trigger (button vs. completion path), token already cancelled? |
| Token cancelled | DEBUG | — |
| Keyboard flag reset | DEBUG | previous KbdActive value |
| Focus parked / selectors cancelled | DEBUG | whether a WebView2 held focus; item count cancelled |
| Handlers unregistered | DEBUG | navigation ledger drained, form handlers removed |
| Loader quiesce | INFO | completed vs. timed out, elapsed, bound |
| Datamodel cleanup | DEBUG | — |
| Groups cleanup | DEBUG | rows removed |
| Release callback invoked | INFO | — |
| Any stage exception | ERROR | stage name + exception (logger.Error(message, e)) |

#### Rollback/feature-flag considerations (if applicable):
No feature flag. Rollback is a revert of the branch. The bounds are constructor-seamed constants, so behavior can be tuned without a settings migration.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:
DequeueAsync keeps its existing parameters and result shape; the outcome's stop reason may now be ScanCapReached in addition to the existing members. QuiesceLoaderAsync takes a TimeSpan bound and returns a Task that completes when the loader finishes or the bound expires; it never throws for the timeout case. ParkFocusAndCancelSelectors takes no arguments and returns void.

#### Required configuration keys and defaults:
None. maxScanWithoutAcceptance (250) and zeroAcceptanceCeiling (120 seconds) are internal gate defaults with optional constructor parameters, following the ratified #424 precedent that the deadline is an internal constant with an internal test seam and no settings surface. The quiesce bound is likewise a constant supplied by the caller.

#### Backward-compatibility expectations:
Additive only at the type level: one new enum member, one new interface method, one new internal method, two new optional constructor parameters. DeadlineExpired is retained. Existing callers compile unchanged. The behavioral changes that are not backward compatible, and are intended, are the superseded #424/#608 empty-result criteria and the non-rethrowing ButtonCancel_Click.

#### Performance constraints (latency/throughput/memory):
The extended scan is bounded by the item cap and the time ceiling; no unbounded wait is introduced. No performance threshold is asserted as an acceptance criterion, because no measured baseline exists for scan throughput (observed 2-3 items/s is a field observation, not a benchmark). Real-world tolerability of the extended scan is an observation recorded during the live-Outlook verification.

## Assumptions, Constraints, Dependencies
- Assumptions: the scoring throughput and score distributions observed on 2026-09-06 are representative; the recommended bounds (250 items, 120 seconds) are engineering proposals confirmed during live verification, not measured values.
- Constraints: .NET Framework 4.8 / legacy non-SDK projects, so every new file needs an explicit Compile entry; the 500-line file limit; no temporary files and no wall-clock waits in tests; Cleanup runs on the UI thread and must not block.
- External dependencies: MSTest, Moq, FluentAssertions, Microsoft.Extensions.Time.Testing (FakeTimeProvider), log4net — all already referenced. No new package.

## Data / API / Config Impact
- User-facing or API changes: one new QfcDequeueStop member (ScanCapReached); one new IQfcDatamodel method (QuiesceLoaderAsync); one new internal method on QfcFormController (ParkFocusAndCancelSelectors); two new optional constructor parameters on the internal gate class. Operator-visible change: a High Confidence run may now scan longer before the dialog opens, and opens empty only on exhaustion or at a bound.
- Superseded criteria recorded here for the reviewer: the #424 spec criterion at archive/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/spec.md:231 and the #608 spec criterion at active/2026-08-25-quickfiler-high-confidence-partial-screen-backfill-608/spec.md:184 are both superseded by #791 AC1. #446 AC-6 is preserved.
- Data or migration considerations: none. No settings surface: Settings.Designer.cs, AppQuickFilerSettings and IAppQuickFilerSettings are unchanged.
- Logging/telemetry updates: the Logging Plan table above.
- Compatibility notes: both QuickFiler projects are legacy non-SDK, so new files require `<Compile Include>` entries; no CLI flags or config schemas exist for this component.

## Test Strategy
MSTest with Moq and FluentAssertions. FakeTimeProvider is the clock seam for both the gate and the quiesce bound. No temporary files, no Thread.Sleep, no Task.Delay, no wall-clock waits, no live Outlook COM.

AC1 — new tests in `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs` (a new file; the existing gate test files are 477 and 465 lines):
- DequeueAsync_ZeroAcceptedAtCheckpoint_ContinuesUntilFirstAcceptance — 40 below-cutoff candidates then one at 950, fake clock advancing 1 s per score against a 12 s checkpoint. Fail-before evidence required.
- DequeueAsync_ZeroAcceptedAndSourceDrained_ReportsSourceExhausted — cap not reached, producer dead.
- DequeueAsync_ZeroAcceptedAndCapReached_StopsAndReportsScanCapReached — small injected cap; asserts no take occurs after the cap.
- DequeueAsync_ZeroAcceptedAndCeilingReached_StopsWhileSourceStillRefilling — sourceActive true and tryTakeNext always null; asserts the ceiling terminates the wait loop.
- DequeueAsync_CheckpointExpiry_LogsCutoffAndCounts and DequeueAsync_Launch_LogsCutoffQuantityAndBounds — asserted through the injected debugLog delegate, not a log4net appender (existing convention).
- DequeueAsync_NonEmptyPrefix_UnchangedByCheckpoint — #608 regression pin.

AC1 retargeting obligations (these encode the superseded behavior; retarget, do not delete):
- `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs` lines 174-208, DequeueAsync_DeadlineExpiresWithZeroAccepted_ReportsDeadlineExpiredStop.
- `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` lines 201-260, DequeueNextItemGroupWithOutcomeAsync_DeadlineExpiredGate_ReportsDeadlineExpiredStop.
- `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` lines 27-92, the fail-closed reflection helper that asserts an exact nine-parameter constructor and must be updated for the two new optional parameters.
- `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` gains a sibling of the existing pin asserting that ScanCapReached also leaves the queue open (CompleteAddingAsync not called).

AC2 — new tests:
- `QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs`: ActionCancelAsync_ResetsKbdActive_WhenKeyboardDialogActive and _DoesNotToggle_WhenInactive; ActionCancelAsync_ParksFocusAndCancelsBreadcrumbSelectors; ActionCancelAsync_UnregistersHandlersBeforeGroupsCleanup (invocation-order assertion; fails before, order inverted); ActionCancelAsync_AwaitsLoaderQuiesceBeforeGroupsCleanup, including that a timed-out quiesce still proceeds; ActionCancelAsync_GroupsCleanupThrows_StillInvokesParentCleanup; ButtonCancel_Click_ActionThrows_DoesNotRethrow.
- `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs`: Cleanup_DatamodelCleanupThrows_StillInvokesParentCleanup; Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted.
- `QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs`: TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_ReturnsFalseWithoutThrowing (fail-before evidence required; fails today with the exact ArgumentException from the log); QuiesceLoaderAsync_LoaderCompletes_ReturnsBeforeTimeout; QuiesceLoaderAsync_LoaderHangs_ReturnsAtBoundAndLogs; Cleanup_CalledTwice_DoesNotThrow.
- Not proposed: any test of RibbonController.ReleaseQuickFiler. It is private with no seam; the guarantee is asserted at the ParentCleanup boundary.

Edge cases and negative scenarios: quantity <= 0 short-circuit; cancellation during the extended scan; transient empty queue while the loader refills; double Cancel; Cancel after a partially-failed launch; a throwing stage in each teardown stage-group.

Coverage impact and targets: changed lines must not regress, and the new and changed methods target >= 90% per the repository unit-test policy. The repository-wide figure is reported against the testable denominator per CLAUDE.md UT2 (COM/VSTO/WinForms/Outlook-Interop exemptions); this change must not lower it. Coverage XML is produced at artifacts/csharp/coverage.xml for the feature review (a permitted non-evidence artifacts path), and the baseline and final-QC coverage notes are recorded under this feature folder's canonical evidence/baseline/ and evidence/qa-gates/ directories. Fail-before/pass-after evidence is recorded under this feature folder's evidence/regression-testing/ directory.

Toolchain commands to run, in this order, restarting from the first on any failure or auto-fix (per CLAUDE.md):
1. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .`
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. vstest.console.exe over the QuickFiler.Test and dependent test assemblies with /EnableCodeCoverage

Manual validation steps: performed by a human per runbooks/live-outlook-cancel-teardown-verification.runbook.md; see Rollout & Follow-up.

## Acceptance Criteria
- [ ] AC1: A High Confidence run that has found no item at or above the cutoff when the first-batch deadline expires continues scanning until the first acceptance, until the candidate queue is genuinely exhausted, or until a hard bound is reached (a cap on items scanned without acceptance, plus a time ceiling that bounds the wait while the background loader is still refilling). An empty dialog is permitted only on exhaustion or at the bound, and the bound decision is logged. The cutoff in effect and the scanned/accepted counts are logged at launch and at each deadline decision. Covered by deterministic MSTest regression tests using a fake time provider.
- [ ] AC2: The Cancel teardown completes cleanly and in order: the background loader is stopped and awaited before any datamodel field is nulled; form and item keyboard handlers are unregistered before item rows are removed; the keyboard-active flag is reset; WebView2 focus is parked and any open breadcrumb dropdown is cancelled on the Cancel path; the ribbon release callback runs under a `finally`; and every stage, including any exception, is logged. Covered by deterministic MSTest regression tests. The live-Outlook confirmation (keyboard usable after Cancel, new log lines present, no null-`this` loader error) is a human follow-up performed per `runbooks/live-outlook-cancel-teardown-verification.runbook.md`, recorded as human-interaction exception HI-1, and does not gate the automated review.
- [ ] AC3: Every regression test named in Test Strategy exists in the file listed for it and passes, and fail-before/pass-after evidence is recorded under this feature folder's evidence/regression-testing/ directory for at least DequeueAsync_ZeroAcceptedAtCheckpoint_ContinuesUntilFirstAcceptance and TryQueueRemainingMailItemAsync_AfterCleanupNulledFields_ReturnsFalseWithoutThrowing.
- [ ] AC4: The C# toolchain passes in the CLAUDE.md order (csharpier format then check, the analyzer msbuild /t:Rebuild, the nullable msbuild /t:Rebuild, vstest with /EnableCodeCoverage) with no failures in the final pass; coverage XML is produced at artifacts/csharp/coverage.xml; and coverage on the changed files is at or above the policy target with no regression on changed lines.
- [ ] AC5: The branch diff touches no file outside the Write Set, other than test files under QuickFiler.Test/Controllers and `<Compile Include>` entries in the QuickFiler project files; in particular QfcCollectionController.cs, QfcHomeController.Iteration.cs, RibbonController.cs, Settings.Designer.cs and AppQuickFilerSettings.cs are unmodified.
- [ ] AC6: The superseded #424 criterion (spec.md:231 in the archived #424 feature folder) and the superseded #608 criterion (spec.md:184 in the active #608 feature folder) are both recorded as superseded in this spec, under Proposed Fix and under Data / API / Config Impact, and #446 AC-6 is verifiably preserved by an unmodified QfcHomeController.Iteration.cs.

## Risks & Mitigations
- Risk: the extended scan makes the pre-UI wait feel longer to the operator when no item qualifies. Mitigation: progress reporting continues during the extended scan, checkpoint decisions are logged, and both bounds terminate it; the bounds are confirmed during live verification.
- Risk: the retargeted gate tests are rewritten to assert the new behavior in a way that no longer pins anything. Mitigation: the superseded assertions are replaced by explicit ScanCapReached and continuation assertions, and the #608 non-empty-prefix pin is added as its own test.
- Risk: the non-rethrowing ButtonCancel_Click hides a real failure. Mitigation: every stage exception is logged at ERROR with its stage name, which is strictly more diagnosable than the current unhandled UI-thread rethrow.
- Risk: the quiesce bound expires while the loader is genuinely mid-work. Mitigation: the relocated null/cancellation guard makes the post-bound continuation harmless, and the timeout case is logged at INFO.
- Rollback: revert the branch; no data or configuration migration is involved.

## Rollout & Follow-up
- Release/rollout steps: merge to main after review; the add-in is picked up by rebuilding the registered checkout, with no re-registration step.
- Post-fix manual verification: a human performs the live-Outlook confirmation per runbooks/live-outlook-cancel-teardown-verification.runbook.md after the fix is built, following the #677 precedent, and records the evidence note in this feature folder at evidence/other/manual-verification.yyyy-MM-ddTHH-mm.md with the timestamp format from the evidence-and-timestamp-conventions skill. This is human-interaction exception HI-1 and does not gate the automated review.
- Post-fix monitoring: after the next live High Confidence runs, confirm the Cancel-stage log lines appear and that no "Delegate to an instance method cannot have null 'this'" error follows a Cancel.
- Follow-up: issue #792 tracks the breadcrumb WebView2 initialization failure (0x8007139F), which is out of scope here.
- Links: issue https://github.com/drmoisan/TaskMaster/issues/791; research note in this feature folder under research/; runbook under runbooks/; follow-up issue #792.
