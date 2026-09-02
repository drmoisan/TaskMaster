# quickfiler-review-sweep-code-defects (Issue #726)

- Date captured: 2026-09-02
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-review-sweep-code-defects/ (Issue #726)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #726
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/726
- Last Updated: 2026-09-02
## Summary

Eight distinct code-correctness findings surfaced across the `bugs-638-644-647` parallel-orchestration run's feature reviews, each deliberately left unfixed by the item that found it because fixing it would have exceeded that item's declared file-footprint scope. Consolidated here as one issue rather than eight, since each is small in isolation and the orchestration overhead of running eight separate bug-fix cycles is disproportionate to the size of the fixes.

## Environment

- OS/version: Windows 11 Pro (repo default)
- Python version: n/a — C#/.NET Framework 4.8.1 WinForms VSTO add-in
- Command/flags used: n/a — findings are from code review, not a repro command
- Data source or fixture: n/a

## Steps to Reproduce

Not applicable in the usual sense — each sub-finding below is a static code-review finding with its own reachability note. See "Actual Behavior" for the reachability of each.

## Expected Behavior

Each sub-finding's expected behavior is stated inline below.

## Actual Behavior

**1. `FilerQueue` drain-barrier hardening (Major).** `QuickFiler/Controllers/FilerQueue.cs` clears its consumer-running flag only on the normal loop-exit path. An exception raised inside the diagnostic branch of the `catch` handler — e.g. `Helpers.First()` on an empty list, which `FilerQueueItem`'s constructor currently permits — escapes the worker loop and leaves the flag permanently set, hanging the background mover. `Enqueue((FilerQueueItem)null)` is a second, separate route to the same state. Neither is reachable from the single current production call site, but both are one caller away from live. Suggested fix: wrap the loop body in `try`/`finally` clearing the flag under the monitor, add a null-and-empty-safe diagnostic path, and a null guard on the item overload. Additionally: `ConsumeAsync` is `public` and now participates in the drain-barrier invariant without establishing it itself (the sibling queue in `TaskVisualization` declares the analogous method `internal`); the barrier is queue-wide rather than per-batch (correct today, but would silently break under a second producer) and takes no `CancellationToken`. The identical latent handshake window exists in the separate type `TaskVisualization/FlagChangeTrainingQueue.cs`. *(Source: item #633 review, PR #717.)*

**2. `QfcHomeController` null-guard and calendar-write ordering (Minor x2).** `GetMoveDiagnostics` has no null-array guard, a narrower null-handling asymmetry with the EFC reference implementation — not reachable today (needs an `IQfcCollectionController` implementation returning `null`, and the sole production implementation does not), but the asymmetry is real. Separately, `WriteMoveToCalendar` runs *before* the empty-diagnostics guard added by item #646, so a session that produces no diagnostic content still creates an Outlook calendar appointment even though the metrics-file write is now correctly skipped — reachable on every occurrence of the reported input; whether the appointment should also be suppressed is a product decision. *(Source: item #646 review, PR #718.)*

**3. `StoreWrapperController.EvaluateLaunchReadiness` conflates two distinct causes.** `StoreLaunchReadinessState.ModelUnavailable` conflates a genuinely failed load with an undefined/future readiness state behind one dialog message. Resolving this needs a new readiness state and crosses the `UtilitiesCS`/`TaskMaster` assembly boundary — recorded as an open item in `docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/spec.md:297-300`. *(Source: item #287 review, PR #716.)*

**4. `EfcItemController` fire-and-forget `InitializeWebViewAsync` calls are unguarded (LIVE).** Item #670 fixed the identical defect for `QfcItemController` (three discarding call sites, now routed through a fault boundary that logs instead of silently dropping the exception). The same pattern exists, unfixed, in `EfcItemController` at `:97` and `:153`, both calling `Task.Run(() => InitializeWebViewAsync())` against that class's own same-named member and discarding the result — this is live, not latent, by the same reasoning item #670 used to justify its own fix (a discarded faulted task under .NET Framework 4.5+ is silently finalized away with no diagnostic). Blocker to a direct copy of #670's fix: `EfcItemController` carries a class-level `[ExcludeFromCodeCoverage]` and has no injectable initializer seam, so extracting one is real work. *(Source: item #670 review, PR #723.)*

**5. Boundary error sinks unguarded against a null or throwing delegate (latent).** Both the sink item #670 just added and the pre-existing `EfcFormController.BoundaryErrorSink` precedent it was modeled on are settable and unguarded: a null or throwing sink delegate would fault the guard's task and silently reinstate the exact unobserved-fault behavior these boundaries exist to prevent. No production code currently assigns either sink, so this is latent — but should be addressed for both types together, since they share the same contract shape. *(Source: item #670 review, PR #723.)*

**6. Post-#678 QuickFiler residuals (2 live, 2 latent-docs).** LIVE: the synchronous `QfcItemController.LoadFolderHandler` path never calls `InitAsync`, unlike its async counterpart. LIVE: `MailItemHelper.FromMailItemAsync` is called redundantly (duplicated calls) somewhere in the affected path — needs tracing to the exact call sites. Latent: the dormant post-display pre-filter (`QfcHighConfidencePreFilter.FilterAsync`, already established as dead/dormant code per the landed decision of issue #233) is confirmed dead code and could be removed. Latent, documentation-only: two stale comments and one stale test docstring left over from item #678's remediation cycle. *(Source: item #678 review, PR #724.)*

**7. `TaskVisualization/TaskViewer.cs` inconsistently discards a `bool`.** `TaskViewer.cs` discards the `bool` returned by `TaskController.KeyboardHandler_KeyDown` at one call site while consuming it at another. Whether this inconsistency is a live defect is unresolved — flagged in passing by an item focused on `QuickFiler`, `TaskVisualization` being out of that item's scope. *(Source: item #663 review, PR #722.)*

**8. `QfcFormViewer.ProcessCmdKey` unused locals; missing EFC Alt-chord positive test.** `ProcessCmdKey`'s claimed branch retains two unused locals (`object sender = FromHandle(msg.HWnd)` and `var e = new KeyEventArgs(keyData)`) — pre-existing, unrelated to item #663's fix, deliberately not removed there under the minimal-targeted-fix bugfix policy. Separately, the Email Filer suite (delivered under issue #467) is missing a positive test case for `Keys.Menu | Keys.Alt`; item #663's own test suite demonstrates this is worth pinning — the `Keys.Menu` arm of #467's delivered predicate is currently deletable without failing any existing test. *(Source: item #663 review, PR #722.)*

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: n/a — see file/method citations inline above; each finding was verified by the originating item's feature-review agent against the relevant guard site.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High: finding 1 (FilerQueue flag leak) can hang the background mover once the narrow trigger condition is hit, and finding 4 (EfcItemController unguarded fire-and-forget) is classified LIVE by the same reasoning that made the sibling fix in #670 necessary. The remaining findings are Medium/Low individually; the bundle is rated at the severity of its most severe member per this repo's established practice for consolidated multi-defect issues (see #451, #619's five-defect PR).

## Suspected Cause / Notes

Each finding traces to a specific PR/item, cited inline above. All eight were deliberately deferred rather than fixed in-branch because fixing them would have exceeded that item's declared file-footprint/acceptance-criteria scope — this is expected behavior of the parallel-orchestration surface, not a process failure.

## Proposed Fix / Validation Ideas

- [ ] `FilerQueue`: `try`/`finally` around the drain loop; null/empty-safe diagnostic branch; null guard on `Enqueue(FilerQueueItem)`; consider `internal` for `ConsumeAsync`; port the identical fix to `TaskVisualization/FlagChangeTrainingQueue.cs`
- [ ] `QfcHomeController`: null-array guard in `GetMoveDiagnostics`; product decision + fix for `WriteMoveToCalendar` ordering relative to the empty-diagnostics guard
- [ ] `StoreWrapperController`/`AppOlObjects`: design a new readiness state to split `ModelUnavailable`'s two causes (crosses `UtilitiesCS`/`TaskMaster` boundary)
- [ ] `EfcItemController`: extract an injectable web-view initializer seam (mirroring #670's `QfcItemController` fix) and route both call sites through a fault boundary
- [ ] Add a null/throwing-delegate guard shared by `WebViewInitializationErrorSink` and `EfcFormController.BoundaryErrorSink`
- [ ] `QfcItemController.LoadFolderHandler` (sync path): call `InitAsync`; trace and de-duplicate the redundant `MailItemHelper.FromMailItemAsync` calls; remove dormant `QfcHighConfidencePreFilter.FilterAsync`; correct the two stale comments + one stale test docstring
- [ ] `TaskVisualization/TaskViewer.cs`: resolve the inconsistent `bool` discard from `KeyboardHandler_KeyDown`
- [ ] `QfcFormViewer.ProcessCmdKey`: remove the two unused locals; add a positive `Keys.Menu | Keys.Alt` test case to the Email Filer suite (issue #467's delivered predicate)

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
