# uithread-init-contract-residuals-784-787-788 (Issue #809)

- Date captured: 2026-09-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/uithread-init-contract-residuals-784-787-788/ (Issue #809)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #809
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/809
- Last Updated: 2026-09-08
## Summary

Consolidates three findings on one file, `UtilitiesCS/Threading/UiThread.cs`, that were filed separately as #784, #787, and #788 after the #781 and #782 reviews. (1) `Init()` accepts a non-STA caller and installs that worker's non-pumping dispatcher and context into set-once process-global state (#787). (2) `Init()` consumes its single-shot latch before `Initialize()` runs, so a failed first attempt can never be retried, and the naive re-arm was measured to regress in #782 (#788). (3) `SynchronizationContextAwaiter.IsCompleted` compares contexts by reference, so any context captured inside a WPF dispatcher operation always posts instead of continuing inline on the UI thread (#784). All three touch the same initialization and awaiter code and should ship as one change with one test suite.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8 VSTO add-in hosted by Outlook desktop; `main` at `04a54e68`
- Command/flags used: `vstest.console.exe <test assemblies> /InIsolation`; runtime probe in the #781 feature folder (`evidence/other/dispatcher-synccontext-probe.2026-09-05T10-40.md`)
- Data source or fixture: `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs:329` (MTA caller of `UiThread.Init(false)`)

## Steps to Reproduce

1. #787: call `UiThread.Init(false)` from an MTA thread (the in-repo instance is the test at `QfcHomeControllerRunAsyncTests.cs:329`). It returns normally and every later `UiThread.Dispatcher` / `UiSyncContext` / `UiThreadId` read marshals onto a thread with no message loop.
2. #788: arrange for `Initialize()` to throw (headless or non-STA), call `Init()`, fix the condition, call `Init()` again. The second call is a no-op because `_loaded.CheckAndSetFirstCall` at `UiThread.cs:36` was consumed before `Initialize()` ran.
3. #784: construct an `ItemViewer` through `ItemViewerQueue.Dequeue` (inside `UiThread.Dispatcher.Invoke`, so `UiSyncContext` is a `DispatcherSynchronizationContext`), then on the UI thread evaluate `viewer.UiSyncContext.GetAwaiter().IsCompleted`. It is `false`, so the continuation posts instead of running inline.

## Expected Behavior

- `Init()` rejects a non-STA caller with a named `InvalidOperationException` before capturing anything.
- A failed `Initialize()` leaves the latch re-armed so a later `Init()` retries, without reintroducing the regression #782 measured.
- `IsCompleted` is true when the caller already runs on the owning UI thread, regardless of which `SynchronizationContext` instance is ambient.

## Actual Behavior

See the three reproduction steps. #787 succeeds silently and poisons the globals for the process lifetime; #788 leaves `UiThread.Dispatcher` throwing an exception that names `Init()` as the remedy while `Init()` is a no-op; #784 adds one queued hop per await and changes ordering relative to already-queued UI work.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: `UtilitiesCS/Threading/UiThread.cs` line 100 (verified 2026-09-05): `public bool IsCompleted => _context == SynchronizationContext.Current;` (reference comparison). Probe result: `Invoke ctx == outer ambient : False` on .NET Framework 4.8 STA. #787 and #788 are missing-precondition and ordering defects with no diagnostic output.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium, carried from #787: in production `ThisAddIn.cs:35-40` is the only `Init()` caller and runs on the Outlook STA, so the hazards are reachable today only from test code, but a worker-thread read of the lazy accessors before startup completes would poison the process. #784 and #788 are Low individually.

## Suspected Cause / Notes

- #787: no `Thread.CurrentThread.GetApartmentState() == ApartmentState.STA` check in `Init()` or `Initialize()`; `CaptureUiVariables()` reads `SynchronizationContext.Current`, `AutoScaleFactor`, `Dispatcher.CurrentDispatcher`, and the managed thread id from the caller unconditionally.
- #788: latch consumed at `UiThread.cs:36` before `Initialize()`; the naive fix (re-arm on throw) was applied and withdrawn in #782 after measuring a reproducible regression. Read the #782 feature folder before choosing the fix.
- #784: reference equality on `SynchronizationContext` is too strict for a context captured inside a dispatcher operation. CORRECTION (2026-09-07, recorded during preparation): an earlier draft of this note proposed bare owning-thread identity (`UiThread.UiThreadId == Thread.CurrentThread.ManagedThreadId`) and attributed it to #781. That attribution is wrong and the predicate is unsafe. `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:263-272` records the opposite rule: a captured context is the authoritative boundary and bare owner-thread identity must never substitute, because a continuation resumed after `ConfigureAwait(false)` can land on a recycled thread-pool thread whose managed id equals the owner's. A bare-id predicate also breaks `QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs:183-199`. The correct predicate keeps reference equality as its fast path and admits only contexts that are demonstrably UI-owned while the caller stands on the owning UI thread; see `research/research.2026-09-07T20-20.md` section R6 in the #809 active folder. Correction posted to #809 as issuecomment-5578563264.
- Superseded issues: #784, #787, #788 (close with a pointer to this issue).

## Proposed Fix / Validation Ideas

Acceptance criteria:

- [ ] AC1: `Init()` throws a named `InvalidOperationException` when called from a non-STA thread, before any global is captured; the MTA test caller at `QfcHomeControllerRunAsyncTests.cs:329` is corrected or given an STA host.
- [ ] AC2: A failed `Initialize()` does not consume the latch; a subsequent `Init()` retries and succeeds. The #782 regression scenario is reproduced as a test and passes with the chosen design.
- [ ] AC3: `SynchronizationContextAwaiter.IsCompleted` returns true on the owning UI thread regardless of ambient context instance, and false elsewhere; ordering-sensitive callers in `ItemViewer` and `EfcFormController` still pass their existing tests.
- [ ] AC4: Unit tests cover STA/MTA rejection, latch re-arm after throw, and awaiter inline-vs-post decisions with a fake dispatcher seam; no real Outlook host.

Validation:

- [ ] Unit coverage areas: `UiThread.Init`, `Initialize`, `CaptureUiVariables`, `SynchronizationContextAwaiter`.
- [ ] Integration scenario to retest: full nine-assembly `/InIsolation` run; QuickFiler launch, item load, and breadcrumb open on a live host.
- [ ] Manual verification notes: no change in observable UI behavior; verify no new keyboard-focus regressions after #677/#796.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
