# 2026-09-07-uithread-init-contract-residuals-784-787-788 (Spec)

- **Issue:** #809
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-07T20-40
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** full-bug (this file is the sole acceptance-criteria source; `user-story.md` is intentionally absent)
- **Research of record:** `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/research/research.2026-09-07T20-20.md`

## Context
Consolidates three findings on one file, `UtilitiesCS/Threading/UiThread.cs`, that were filed separately as #784, #787, and #788 after the #781 and #782 reviews. (1) `Init()` accepts a non-STA caller and installs that worker's non-pumping dispatcher and context into set-once process-global state (#787). (2) `Init()` consumes its single-shot latch before `Initialize()` runs, so a failed first attempt can never be retried, and the naive re-arm was measured to regress in #782 (#788). (3) `SynchronizationContextAwaiter.IsCompleted` compares contexts by reference, so any context captured inside a WPF dispatcher operation always posts instead of continuing inline on the UI thread (#784). All three touch the same initialization and awaiter code and should ship as one change with one test suite.

Environment:
- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8 VSTO add-in hosted by Outlook desktop; `main` at `04a54e68`
- Command/flags used: `vstest.console.exe <test assemblies> /InIsolation`; runtime probe in the #781 feature folder (`evidence/other/dispatcher-synccontext-probe.2026-09-05T10-40.md`)
- Data source or fixture: `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs:329` (MTA caller of `UiThread.Init(false)`)

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium, carried from #787: in production `ThisAddIn.cs:35-40` is the only `Init()` caller and runs on the Outlook STA, so the hazards are reachable today only from test code, but a worker-thread read of the lazy accessors before startup completes would poison the process. #784 and #788 are Low individually.


## Repro & Evidence
Steps to Reproduce:
1. #787: call `UiThread.Init(false)` from an MTA thread (the in-repo instance is the test at `QfcHomeControllerRunAsyncTests.cs:329`). It returns normally and every later `UiThread.Dispatcher` / `UiSyncContext` / `UiThreadId` read marshals onto a thread with no message loop.
2. #788: arrange for `Initialize()` to throw (headless or non-STA), call `Init()`, fix the condition, call `Init()` again. The second call is a no-op because `_loaded.CheckAndSetFirstCall` at `UiThread.cs:36` was consumed before `Initialize()` ran.
3. #784: construct an `ItemViewer` through `ItemViewerQueue.Dequeue` (inside `UiThread.Dispatcher.Invoke`, so `UiSyncContext` is a `DispatcherSynchronizationContext`), then on the UI thread evaluate `viewer.UiSyncContext.GetAwaiter().IsCompleted`. It is `false`, so the continuation posts instead of running inline.

Expected:
- `Init()` rejects a non-STA caller with a named `InvalidOperationException` before capturing anything.
- A failed `Initialize()` leaves the latch re-armed so a later `Init()` retries, without reintroducing the regression #782 measured.
- `IsCompleted` is true when the caller already runs on the owning UI thread, regardless of which `SynchronizationContext` instance is ambient.

Actual:
See the three reproduction steps. #787 succeeds silently and poisons the globals for the process lifetime; #788 leaves `UiThread.Dispatcher` throwing an exception that names `Init()` as the remedy while `Init()` is a no-op; #784 adds one queued hop per await and changes ordering relative to already-queued UI work.

Logs / Screenshots:
- [x] Attached minimal logs or screenshot
- Snippet: `UtilitiesCS/Threading/UiThread.cs` line 100 (verified 2026-09-05): `public bool IsCompleted => _context == SynchronizationContext.Current;` (reference comparison). Probe result: `Invoke ctx == outer ambient : False` on .NET Framework 4.8 STA. #787 and #788 are missing-precondition and ordering defects with no diagnostic output.

Research-verified refinement of the #784 repro (research section R6, from the #781 probe output): the probe also records `Invoke#1 ctx == Invoke#2 ctx : True` and `ambient after ops == outer : True`. Two consequences follow. First, the defect does **not** reproduce while execution is inside a dispatcher operation on the same dispatcher, because the viewer's captured `DispatcherSynchronizationContext` is reference-equal to the ambient one there; the defect is confined to awaits taken on the UI thread **outside** a dispatcher operation. Second, the persistent WinForms context and the dispatcher context are two distinct, both-valid UI-owned instances on the same thread. The repro step above remains correct as written; the refinement narrows where the fix changes behaviour.


## Scope & Non-Goals

### Write Set (authoritative)

These are the only files this delivery modifies or adds. Every other repository path cited in this document is a read-only citation supporting an argument, not a change target.

Production:
- `UtilitiesCS/Threading/UiThread.cs` — all three defect fixes plus the test seams.
- `UtilitiesCS/Threading/IUiCaptureSource.cs` — new narrow interface for the capture object (see Proposed Fix).
- `UtilitiesCS/Threading/SyncContextForm.cs` — declaration gains `IUiCaptureSource`; no new members (all six are already declared or inherited).
- `UtilitiesCS/UtilitiesCS.csproj` — explicit `<Compile Include>` for the new interface file. This is a legacy, non-SDK, `packages.config` project (research R8), so a new file that is not listed does not compile.

Test:
- `UtilitiesCS.Test/Threading/UiThread_Tests.cs` — awaiter cases, plus `[DoNotParallelize]` on any class that mutates `UiThread` statics.
- `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs` — new; the AC1 and AC2 cases.
- `UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs` — new; snapshot/restore of the process-global statics.
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` — explicit `<Compile Include>` for the two new test files (same legacy-project constraint).
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` — gains `[DoNotParallelize]` only; no other change.
- `UtilitiesCS.Test/EmailIntelligence/FolderRemapViewer_Tests.cs` — gains `[DoNotParallelize]` only; no other change.
- `UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersViewer_Tests.cs` — gains `[DoNotParallelize]` only; no other change.
- `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs` — the MTA `Init()` caller, reconciled per decision D4.

Write Set amendment (2026-09-07, recorded during preparation preflight round 1). The three
`[DoNotParallelize]`-only test files above were added to this list after preflight established that
they are unprotected writers of the `UiThread` process-global statics: `FolderPredictorTests` writes
`UiThread._uiSyncContext` by reflection at `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs:479`,
and the two `[STATestClass]` viewer test classes drive `Init()` to success transitively and so write
all four capture fields. None of the three carries `[DoNotParallelize]` today. Under the MSTest
parallelization that `scripts/vscode/TaskMaster.cli.runsettings:4-7` configures — the element form
`<Parallelize>` carrying `<Workers>0</Workers>` and `<Scope>ClassLevel</Scope>`, not an attribute
form — the serial and parallel buckets overlap in wall-clock
time, so marking only the classes this delivery adds does not stop a writer in another class of the
same assembly; the guarantee holds only when no writer of the shared static remains in the parallel
bucket. The collision does not exist today, because the currently marked class owns only `_dispatcher`;
it is introduced by this delivery, which clears and rewrites the remaining statics. The amendment is
therefore a consequence of this change rather than pre-existing debt, and each of the three files
gains exactly one attribute line.

- In scope:
  - The three defects #787, #788, and #784, all resident in `UtilitiesCS/Threading/UiThread.cs`.
  - The minimum test seams required to exercise them without a live Outlook host: an injectable capture-object factory and a test-only static reset.
  - Raising the measured coverage of `UtilitiesCS/Threading/UiThread.cs` above the CLAUDE.md floor. Research R8 records that #782 waived this uplift and identified #809 as the carve-out that would deliver it, and that all 19 currently-uncovered lines are reachable through the recommended seam.

- Out of scope / non-goals:
  - **Deleting `ThreadSafeSingleShotGuard` (decision D2, settled).** Only UiThread's own use of the type at `UtilitiesCS/Threading/UiThread.cs:46` is removed. The type is retained. Its consumers were enumerated across the repository by the maintainer at approximately twenty sites, including UtilitiesCS/ReusableTypeClasses/TimedActions/TimedBatchAction.cs:45, UtilitiesCS/Threading/IdleActionQueue.cs:55, UtilitiesCS/Threading/ApplicationIdleTimer.cs:444-445, UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs:240-242, and TaskVisualization/FlagChangeTrainingQueue.cs:32. Deleting it would be a large unrelated change. (This closes research open question 3.)
  - **Injecting an `IUiDispatcher` into `QfcHomeController` (research R5 option D).** The seam exists at UtilitiesCS/Threading/IUiDispatcher.cs and UtilitiesCS/Threading/WpfUiDispatcher.cs and QfcItemController already consumes it, but QuickFiler/Controllers/QfcHomeController.cs:360 is not routed through it. Routing it would be a production change outside this issue's stated scope, and research R5 records that it is larger than option B for the same benefit.
  - **Granting `InternalsVisibleTo` to `QuickFiler.Test` (decision D3, settled).** No new grant is added. Every AC4 test lives in `UtilitiesCS.Test`, which already holds the grant at `UtilitiesCS/Properties/AssemblyInfo.cs:18-20`. QuickFiler.Test must not be granted access to UtilitiesCS internals. (This closes research open question 5.)
  - **Assembly-wide STA execution.** No `ExecutionThreadApartmentState` change to any `.runsettings`. `UtilitiesCS.Test/test.runsettings:2-5` records the standing decision that global STA is intentionally disabled and that STA is opt-in per class or method; reversing it would remove class-level parallelism across the whole assembly.
  - **Renaming `Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize`.** Research R7 records that this method's fully-qualified name is quoted inside a committed `TestCaseFilter` evidence artifact, so the deliberately inaccurate `NamingInitialize` suffix must survive.
  - Changing the observable behaviour of any of the 56 live `UiThread` read sites other than through the three fixed defects.
  - Any change requiring a live Outlook process to verify.

- Explicitly excluded systems, integrations, or datasets:
  - Outlook Interop, Microsoft Graph, the classifier engines, and all persistence paths. This delivery touches only in-process threading state.
  - `ThreadMonitor` behaviour. The `_monitorUiThread` block at `UiThread.cs:66-76` becomes reachable in tests through the new factory seam, but its logic is not changed.

### Blast radius (measured read census, decision D6)

Research R2 measured **56 live `UiThread` read/call sites across 31 production files** in this run, by `Grep` over `**/*.cs` for `UiThread\.(UiSyncContext|AutoScaleFactor|Dispatcher|UiThreadId|Init)`, with a second `Grep` confirming no `using static` alias exists, and with documentation, comments, commented-out code, and the message literal at `UiThread.cs:136` excluded. The test-side figure is 6 live sites across 3 files.

The #782 delivery adopted **49 live reads across 25 production files** at tag `pre-782-base`. That earlier figure is not restated here as current. The +7/+6 divergence **cannot be decomposed**, because the #782 artifact publishes the figure but not its member set. Two contributing factors are identifiable but not sufficient to account for the gap: the current count includes the one production `UiThread.Init(` call, which "live reads" may have excluded, and the tree has taken several deliveries since that tag. Treat 56/31 as the current figure. (This leaves research open question 2 UNKNOWN and unresolvable from in-tree artifacts.)

Only 8 of the 56 sites can reach `Init()` at all: the two direct calls (`TaskMaster/ThisAddIn.cs:35`, `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs:329`) and the six lazy reads of `UiSyncContext` or `AutoScaleFactor`. All remaining sites read `Dispatcher`, `UiThreadId`, or an already-populated field, and `UiThread.Dispatcher` never calls `Init()` (`UiThread.cs:153-171`).


## Root Cause Analysis

### #787 — `Init()` accepts a non-STA caller

Neither `Init()` (`UiThread.cs:19-40`) nor `Initialize()` (`UiThread.cs:48-79`) reads `Thread.CurrentThread.GetApartmentState()` (verified in research). `SyncContextForm.CaptureUiVariables()` (`UtilitiesCS/Threading/SyncContextForm.cs:34-40`) unconditionally reads `SynchronizationContext.Current`, `this.AutoScaleFactor`, `Dispatcher.CurrentDispatcher`, and `Thread.CurrentThread.ManagedThreadId` from whatever thread called it, and `Initialize()` copies all four into set-once process-global statics at `UiThread.cs:58-61`. A worker-thread caller therefore installs a dispatcher with no message loop and a non-UI context for the remaining process lifetime.

A second, separable part of the same defect: the four monitoring-configuration assignments at `UiThread.cs:26-35` execute on **every** `Init()` call, before the latch is read at `:36`. A non-STA caller mutates `_monitorUiThread`, `_onLockupDetected`, `_monitorTimeProvider`, and `_lockupAttributionThresholdMs` even when the latch is already consumed and `Initialize()` will not run. AC1's phrase "before any global is captured" covers these four.

### #788 — the latch is consumed before `Initialize()` runs

`if (_loaded.CheckAndSetFirstCall)` at `UiThread.cs:36` is backed by `Interlocked.Exchange(ref _state, CALLED) == NOTCALLED` (`UtilitiesCS/Threading/ThreadSafeSingleShotGuard.cs:24-27`, verified). The latch transitions on the *attempt*, not on the *outcome*. If `Initialize()` throws, the exception propagates, the statics stay null, and every later `Init()` is a silent no-op. `UiThread.Dispatcher` then throws a message that names `UiThread.Init()` as the remedy (`UiThread.cs:135-136`) while `Init()` cannot help.

The naive repair — re-arming the latch in a `catch` — was applied and withdrawn in #782 after a measured regression. The status of that measurement is addressed in the Proposed Fix under "Why the AC2 design does not reintroduce the #782 regression" and in decision D5; it is not assumed true here.

A separate, pre-existing weakness in the same code: `Interlocked.Exchange` admits a race in which a second caller observes the latch consumed and returns while the first caller is still inside `Initialize()`, so the second caller can read a half-populated static set. #782 recorded this as no-action finding C04.

### #784 — `IsCompleted` uses reference equality

`UiThread.cs:100` is `public bool IsCompleted => _context == SynchronizationContext.Current;`. `SynchronizationContext` does not overload `==`, so this is reference identity. A viewer that is constructed inside `UiThread.Dispatcher.Invoke` captures a `DispatcherSynchronizationContext` (`QuickFiler/Viewers/ItemViewer.cs:26` and its five siblings all execute `_context = SynchronizationContext.Current;` in the constructor). Later, on the UI thread but outside a dispatcher operation, the ambient context is the persistent `WindowsFormsSynchronizationContext`, which is a different instance, so `IsCompleted` is false and every `await` on that context takes a queued hop.

**CORRECTION (recorded 2026-09-07; supersedes the sentence carried in the seeded template and in the original GitHub issue body).** An earlier draft of this analysis asserted that the correct predicate is bare owning-thread identity, `UiThread.UiThreadId == Thread.CurrentThread.ManagedThreadId`, and attributed that test to #781. **That attribution is false and the predicate is unsafe.** `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:263-272` records the opposite rule verbatim: when a context was captured it is the authoritative boundary, and bare owner-thread identity must never substitute, because a continuation resumed after `ConfigureAwait(false)` can be scheduled onto a recycled thread-pool thread whose managed thread id equals the owner's. #781 used thread identity only on the `_context == null` path constructed for host-neutral tests, and its sibling `DispatchValue<T>` guard at `:164-166` is stricter still. Independently, a bare-id predicate breaks a live test: `QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs:183-199` awaits a foreign `WindowsFormsSynchronizationContext` from the MSTest thread and asserts the continuation resumes on the pump thread, and if any earlier test in that process has run `UiThread.Init()` on the pooled MSTest worker — which `QfcHomeControllerRunAsyncTests.cs:329` does today — the bare predicate returns true, the continuation runs inline on the MSTest thread, and the assertion fails. The corrected note is already recorded in `issue.md`. The predicate this specification adopts is the field-free one in the Proposed Fix.

Superseded issues: #784, #787, #788 (close with a pointer to #809).


## Proposed Fix

### Design summary (what changes where):

Three changes, all in `UtilitiesCS/Threading/UiThread.cs`, plus one narrow new interface and two test-only seams.

1. **#787** — an apartment-state precondition inserted as the **first statement** of `Init()`, ahead of the four monitoring assignments and ahead of the latch.
2. **#788** — the single-shot guard is replaced *inside `UiThread` only* by a success-recorded flag guarded by a serializing lock, so the flag is set after `Initialize()` returns rather than before it runs. `ThreadSafeSingleShotGuard` itself is retained (decision D2).
3. **#784** — `IsCompleted` is replaced by a field-free predicate that keeps reference equality as its fast path and additionally admits exactly two demonstrably UI-owned cases while the caller stands on the owning UI thread.

### Boundaries and invariants to preserve:

- **`default(SynchronizationContextAwaiter)` behaviour is unchanged.** The new predicate adds no field to the struct, so the default instance still has `_context == null`. Today `IsCompleted` evaluates `null == SynchronizationContext.Current`, which is true on a context-free thread and false otherwise; the new predicate's first clause `ReferenceEquals(_context, ambient)` produces the same answer on the first branch and the `ambient is null` early return covers the second. `OnCompleted` still throws `NullReferenceException` on a default instance, exactly as at `UiThread.cs:102-103` today. This is a positive reason to prefer a field-free predicate: a construction-time capture of the owning thread id would also be unsound, because `GetAwaiter()` (`UiThread.cs:108-111`) runs at the `await` site on the awaiting thread, not on the thread where the context was captured.
- **A null ambient context must continue to return `false`.** Two call sites take `TaskScheduler.FromCurrentSynchronizationContext()` immediately after the await — `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:64` with the scheduler at `:67`, and `QuickFiler/Controllers/EfcItemController.cs:191` with the scheduler at `:201`. That factory throws `InvalidOperationException` when the ambient context is null. A predicate that returned true with a null ambient would continue inline on a thread with no context and break both sites.
- **Both `WinFormsPumpHostTests` assertions must continue to pass**: `AwaitingSyncContext_FromTheTestThread_ResumesOnThePumpThread` (`QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs:183`, assertion at `:191-199`) and `BothMarshalRoutes_WpfDispatcherAndSyncContext_ExecuteOnThePumpThread` (`:218`, assertion at `:255`). Under the adopted predicate the awaited context is a `WindowsFormsSynchronizationContext` that is neither `_uiSyncContext` nor a `DispatcherSynchronizationContext`, so the predicate returns false and the continuation posts to the pump thread exactly as today.
- **`UiThread.Dispatcher` remains deliberately non-lazy.** The `<remarks>` at `UiThread.cs:141-148` states the asymmetry on purpose. Do not add an `Init()` call to that accessor.
- **The literal `UiThread.Init()` must survive inside `DispatcherNotInitializedMessage`.** `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs:196` asserts on that substring rather than on the constant, so the assertion does not move if the constant is edited.
- **Field names `_uiSyncContext` and `_dispatcher` must not change.** Four existing test helpers reach them by reflection (research R3): `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs:117`, `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:136`, and `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs:470`.
- **The 500-line file limit.** `UiThread.cs` is 195 lines, leaving roughly 305 lines of headroom; no partial-class split is required.

### Dependencies or blocked work:

None external. One internal ordering dependency: the AC2 design's safety argument depends on the AC1 precondition being present and being the first statement of `Init()`. Ship them together; do not land the latch change alone.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

See the Write Set above. No other file is modified.

#### Functions/classes/CLI commands impacted:

- `UiThread.Init(bool, Action<LockupAttribution>?, TimeProvider?, int)` — gains the precondition and the lock.
- `UiThread.Initialize()` — obtains its capture object from the factory instead of constructing `SyncContextForm` directly.
- `UiThread.SynchronizationContextAwaiter.IsCompleted` — replaced.
- `SyncContextForm` — declaration gains `IUiCaptureSource`.
- New: `IUiCaptureSource`, `UiThread.SyncContextFormFactory`, `UiThread.ResetForTesting()`, `UiThread.NonStaInitMessagePrefix`.

#### Data flow and validation changes:

**#787 — the STA precondition.** Insert as the first statement of `Init()`, before the assignments at `UiThread.cs:26-35` and before the latch read at `:36`:

```csharp
ApartmentState apartment = Thread.CurrentThread.GetApartmentState();
if (apartment != ApartmentState.STA)
{
    throw new InvalidOperationException(NonStaInitMessage(apartment));
}
```

with a sibling of the existing message constant:

```csharp
internal const string NonStaInitMessagePrefix =
    "UiThread.Init() must be called on the UI (STA) thread during host startup. Observed apartment state: ";

private static string NonStaInitMessage(ApartmentState observed) =>
    NonStaInitMessagePrefix + observed;
```

`internal const` matches the visibility of `DispatcherNotInitializedMessage` (`UiThread.cs:135-136`) and is reachable from `UtilitiesCS.Test` through the existing grant. The prefix is split from the formatted message so a test can assert the stable text without pinning the enum's rendering.

Placement ahead of `:26` is load-bearing, not cosmetic: those four assignments mutate process-global monitoring configuration on every call regardless of the latch, so a non-STA caller poisons them today even when `Initialize()` never runs.

**Decision D1 (settled): the rejection is `!= STA`.** The precondition rejects any apartment state that is not `ApartmentState.STA`, which rejects both `ApartmentState.MTA` and `ApartmentState.Unknown`. AC1 says "non-STA thread" and `!= STA` is the reading that matches it. This closes research open question 4; the design does not depend on whether `Unknown` is actually observable on this host.

**#788 — retry after a failed `Initialize()`.** Replace the latch's role in `Init()` with a success-recorded flag plus a serializing lock, so the flag is set only after `Initialize()` returns:

```csharp
private static readonly object InitLock = new object();
private static bool _initialized;

// inside Init(), after the STA precondition and the four assignments:
lock (InitLock)
{
    if (_initialized)
    {
        return;
    }
    Initialize();
    _initialized = true;
}
```

A failed `Initialize()` propagates with `_initialized` still `false`, so a later `Init()` retries. `_loaded` and the `using`-level dependency on `ThreadSafeSingleShotGuard` are removed from this file only; the type is retained (decision D2). The lock additionally closes the pre-existing C04 race, which `Interlocked.Exchange` never covered.

**#784 — the awaiter predicate.** Replace `UiThread.cs:100` with the block below, reproduced from research section R6:

```csharp
public bool IsCompleted
{
    get
    {
        SynchronizationContext? ambient = SynchronizationContext.Current;
        if (ReferenceEquals(_context, ambient))
        {
            return true;
        }
        // A null ambient context means there is nothing to resume onto: continuing inline would
        // break TaskScheduler.FromCurrentSynchronizationContext() at the two WebView2 setup sites.
        if (ambient is null)
        {
            return false;
        }
        if (_uiThreadId == -1 || _uiThreadId != Thread.CurrentThread.ManagedThreadId)
        {
            return false;
        }
        // The persistent UI context captured at Init() time.
        if (ReferenceEquals(_context, _uiSyncContext))
        {
            return true;
        }
        // A dispatcher context is UI-owned only when this thread's dispatcher is the UI dispatcher.
        return _context is DispatcherSynchronizationContext
            && ReferenceEquals(Dispatcher.FromThread(Thread.CurrentThread), _dispatcher);
    }
}
```

`using System.Windows.Threading;` is already present at `UiThread.cs:11`.

**Why this predicate reads the private statics directly.** `SynchronizationContextAwaiter` is nested inside `UiThread`, so it can read `_uiThreadId`, `_uiSyncContext`, and `_dispatcher` as fields. It **must** do so rather than read the `UiSyncContext` or `Dispatcher` properties, for two distinct reasons: the `UiSyncContext` property lazily calls `Init()` (`UiThread.cs:117-120`), which under the AC1 precondition would now **throw** when the predicate is evaluated from a worker thread, turning a boolean query into an exception; and the `Dispatcher` property throws `InvalidOperationException` whenever the backing field is unset (`UiThread.cs:160-167`). A predicate must be side-effect-free and total, so it reads the fields.

**Why not resolve the dispatcher context's own dispatcher.** .NET Framework 4.8's `System.Windows.Threading.DispatcherSynchronizationContext` exposes no public `Dispatcher` property, so its owning dispatcher is reachable only by reflection, which is not acceptable in production code. `Dispatcher.FromThread` is the reflection-free substitute and is side-effect-free: it returns `null` rather than creating a dispatcher.

**The `-1` sentinel.** `_uiThreadId` is initialised to `-1` (`UiThread.cs:133`) and `Thread.ManagedThreadId` is never negative, so a bare comparison would already be safe. The sentinel is nevertheless checked explicitly so the pre-`Init()` behaviour is legible rather than incidental.

#### Error handling and logging updates:

- One new exception type/site: `InvalidOperationException` from `Init()` on a non-STA caller, carrying `NonStaInitMessagePrefix` plus the observed apartment state. No logging is added; `UiThread` has no logger today and the failure is a startup contract violation that must be loud rather than recorded.
- No exception is swallowed. A throwing `Initialize()` continues to propagate unchanged; the only difference is that `_initialized` remains false.
- Three production sites gain a *possible* new throw where they previously behaved silently (research R1): `TaskMaster/AppGlobals/AppOlObjects.cs:367`, `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:179`, and — only for a hypothetical off-UI-thread caller — `UtilitiesCS/EmailIntelligence/OlFolderTools/FolderRemap/FolderRemapViewer.cs:40` and `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersViewer.cs:79`. Each throws only when the backing field is still null. In production `ThisAddIn.cs:35` runs first, so the field is populated and none of them throws. At `AppOlObjects.cs:367` the new throw is strictly better than today's behaviour: the enclosing branch is entered only when the caller is *off* the UI thread (`:364`), so today it constructs a `SyncContextForm` on the worker and performs the COM read on the wrong apartment, which is the exact failure the comment at `:361-363` says it is preventing.
- Research R1 verified that **zero existing tests are affected** by these new throws, because every in-repo lazy-read driver is STA-hosted. The single in-repo caller AC1 definitively breaks is the MTA `Init()` call handled by decision D4 below.

#### Rollback/feature-flag considerations (if applicable):

None. No feature flag is introduced. The change is a single revert of `UtilitiesCS/Threading/UiThread.cs` plus its test files if a defect is found post-merge. A flag would be counterproductive here: the precondition's value is that it is unconditional.

### Why the AC2 design does not reintroduce the #782 regression

The #782 mechanism, as recorded, is: a re-armed latch makes every subsequent read of `UiSyncContext` or `AutoScaleFactor` re-enter `Initialize()`, reconstruct and `Show()` a WinForms `SyncContextForm`, throw again, and starve the thread pool.

Research R1 verified the complete reachable surface of `Initialize()`. It is reachable from exactly four places: the two direct `Init()` calls (`TaskMaster/ThisAddIn.cs:35`, `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs:329`) and the two lazy getters at `UiThread.cs:119` and `UiThread.cs:185`. `UiThread.Dispatcher` never calls `Init()`. With the AC1 precondition as the **first** statement of `Init()`, a non-STA reader of either lazy getter fails at one `GetApartmentState()` read and a `throw`; it never reaches `new SyncContextForm()` at `:51` or `Show()` at `:54`. The expensive, potentially-throwing body is therefore unreachable from any thread-pool thread, which is where thread-pool starvation would have to originate. On the STA thread itself, `Initialize()` succeeds — verified against `UtilitiesCS.Test/EmailIntelligence/FolderRemapViewer_Tests.cs:118-131` and `UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersViewer_Tests.cs:39-51`, both `[STATestClass]`, both asserting `NotThrow` — so no retry loop engages there either. The `lock (InitLock)` additionally serializes concurrent first attempts.

**Decision D5 (settled): the #782 narrative is to be measured, not assumed.** The argument above stands independently of whether the recorded #782 mechanism was ever real. It removes the mechanism if the mechanism exists, and costs nothing if it does not. That independence is deliberate, because research R1 could not confirm the narrative and recorded three reasons to doubt it:

1. Every in-repo path that reaches the lazy `AutoScaleFactor` getter in a test run is already STA-hosted, so `Initialize()` succeeds and no catch fires.
2. No test reaches the lazy `UiSyncContext` getter with a null backing field. The #782 record's own measured uncovered-line set for `UiThread.cs` includes `118,119,120`, which is exactly the `if (_uiSyncContext is null) { Init(); }` block, so that block was never executed with a null field in a measured run.
3. The readers #788 named as the surfacing path have no test driver: a search of `TaskMaster.Test` for `AppOlObjects`, `UserEmailAddress`, `ResolveCurrentUser`, and `SetUpBrightIdeasSettings` found no test exercising either reader, and `ThisAddIn.SetUpBrightIdeasSettings` is a private method on a VSTO type with no test caller.

Separately, the regression test that #782 attributed the failure to — `UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` — is the same fully-qualified test that is independently documented as an intermittent flake under issue #780, with the same `TaskCanceledException` type and the same "21 s" signature. The #782 attribution rests on a single with/without run pair against that test. It is plausible; it is not statistically established.

Two obligations follow, and they are binding on the plan:

- **Measure it.** Establish by measurement on the execution host whether `new SyncContextForm(); Show();` throws when executed on an MTA thread. This is research open question 1 and it is UNKNOWN; it cannot be settled by reading, and two in-tree records point in opposite directions (`UtilitiesCS.Test/Threading/UiThread_Tests.cs:151-156` implies success on an MTA worker, while the #782 mechanism requires a throw). If it does not throw, the #782 narrative is refuted and AC2's "the #782 regression scenario is reproduced as a test" clause is discharged instead by a forced-throw scenario driven through the factory seam. Record the measurement under the feature folder's `evidence/other/` directory.
- **Do not attribute a single flake.** Run the full suite and record `TryAddValuesAsync_UpdatesExistingValue` explicitly. Because that test is the documented #780 flake, **a single failure is not sufficient evidence of a regression; at least three repetitions are required before attributing it** to this delivery.

### Technical specifications (interfaces/contracts):

#### Behavioural contract for `Init()`

**Precondition.** The calling thread's apartment state must be `ApartmentState.STA`. Any other value, including `MTA` and `Unknown`, is rejected with `InvalidOperationException` whose message begins with `NonStaInitMessagePrefix` and ends with the observed apartment state (decision D1).

**On rejection, nothing is captured.** Because the precondition is the first statement, none of `_monitorUiThread`, `_onLockupDetected`, `_monitorTimeProvider`, or `_lockupAttributionThresholdMs` is assigned; the initialization flag is not set; `Initialize()` does not run; no `SyncContextForm` is constructed or shown; and `_uiSyncContext`, `_autoScaleFactor`, `_uiThreadId`, and `_dispatcher` are left exactly as they were.

**On acceptance, what is captured.** The four monitoring-configuration values are assigned from the arguments. Then, under `InitLock`, if initialization has not yet succeeded, `Initialize()` runs and captures four values from the capture object: `UiSyncContext`, `AutoScaleFactor`, `UiThreadId`, and `Dispatcher` (`UiThread.cs:58-61`). If `monitorUiThread` is true, a `ThreadMonitor` is constructed and run against the calling thread.

**Retry semantics.** The initialization flag is set only after `Initialize()` returns normally. If `Initialize()` throws, the exception propagates to the caller, the flag stays false, and a subsequent `Init()` from an STA thread retries the full initialization. Once the flag is set, every later `Init()` performs the apartment check and the four monitoring assignments and then returns without re-initializing — so the four monitoring values remain settable after initialization, matching today's behaviour. Concurrent first attempts are serialized by `InitLock`; exactly one of them runs `Initialize()`.

**Idempotence and thread affinity.** `Init()` is idempotent with respect to the captured UI state after the first success. It is not idempotent with respect to the monitoring configuration, which is by design and unchanged.

#### Inputs/outputs and formats:

`IUiCaptureSource` (new, `internal`, in `UtilitiesCS/Threading/IUiCaptureSource.cs`) exposes exactly the members `Initialize()` uses today: `bool ShowInTaskbar { get; set; }`, `FormWindowState WindowState { get; set; }`, `void Show()`, `void Hide()`, `void CaptureUiVariables()`, and the four read-only capture properties `SynchronizationContext UiSyncContext`, `System.Drawing.SizeF FormAutoScaleFactor`, `Dispatcher UiDispatcher`, and `int UiThreadId`. `SyncContextForm` already declares all four capture properties and `CaptureUiVariables()` (`UtilitiesCS/Threading/SyncContextForm.cs:24-40`) and inherits the other four members from `Form`, so it satisfies the interface without gaining a member. Research recommended a separate adapter type; implementing the interface directly on `SyncContextForm` is preferred here under the simplicity-first design principle, because it adds no second production type for a single test need.

`UiThread.SyncContextFormFactory` is `internal static Func<IUiCaptureSource> { get; set; }` defaulting to `() => new SyncContextForm()`. `Initialize()` calls it instead of `new SyncContextForm()` at `:51`, and `_syncContextForm` becomes `IUiCaptureSource?`. This shape is directly precedented in the repository by `QuickFiler/Helper Classes/ItemViewerQueue.cs:11-27` (`internal static Func<ItemViewer> ProductionViewerFactory`, reset at `:83-91`) and by the four `internal static` delegates on `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:161-184`.

`UiThread.ResetForTesting()` is `internal static void`. It clears `_initialized`, `_uiSyncContext`, `_autoScaleFactor`, `_dispatcher`, and `_syncContextForm`, sets `_uiThreadId` back to `-1`, and restores the default factory. The `internal ... ForTesting()` idiom is repo-precedented at `QuickFiler/Helper Classes/ItemViewerQueue.cs:69-91`.

#### Required configuration keys and defaults:

None. No configuration key, environment variable, or settings entry is added.

#### Backward-compatibility expectations:

- The public surface of `UiThread` is unchanged. `Init`, `UiSyncContext`, `UiThreadId`, `Dispatcher`, `AutoScaleFactor`, `GetAwaiter`, and `SynchronizationContextAwaiter` keep their existing signatures and accessibility. The two new members are `internal`.
- `SyncContextForm` gains an interface on an `internal` type in its declaration; no member is added, removed, or changed.
- The only intentional behaviour change for existing callers is the three fixed defects. `Init()` from a non-STA thread changes from silent success to a thrown exception; this is the point of AC1.

#### Performance constraints (latency/throughput/memory):

- `Init()` gains one `GetApartmentState()` read and one uncontended `lock` acquisition on a path that runs once per process. Not measurable.
- `IsCompleted` gains, in the worst case, one type test and one `Dispatcher.FromThread` call. It is evaluated once per `await` on a `SynchronizationContext`. The reference fast path is unchanged for the common case. `Dispatcher.FromThread` is a lookup, not a construction.
- The intended net effect is a *reduction* in queued UI hops at the sites listed under Risks; no site gains a hop.


## Assumptions, Constraints, Dependencies

- Assumptions (environment, data, access):
  - MSTest's default apartment for this repository's runs is MTA, so a plain `[TestMethod]` supplies the AC1 rejection case and `[STATestMethod]` supplies the acceptance case. Research R4 established this from three independent in-tree sources: `UtilitiesCS.Test/test.runsettings:2-5` states that global STA is intentionally disabled; no `.runsettings` in the tree sets `ExecutionThreadApartmentState`; and the #782 records describe the ambient MSTest worker as MTA in two places.
  - `Dispatcher.FromThread` returns the UI dispatcher on the owning UI thread after `Init()` has succeeded, because `Initialize()` captures `Dispatcher.CurrentDispatcher` from that same thread (`UtilitiesCS/Threading/SyncContextForm.cs:34-40`). This is a carried inference from the capture code, not a measured result.
  - Whether `new SyncContextForm(); Show();` throws on an MTA thread is **UNKNOWN** and must be measured (decision D5).
  - The member set behind the #782 figure of 49/25 is **UNKNOWN** and cannot be recovered from the published artifacts (decision D6).
- Constraints (budget, performance, compatibility):
  - Target framework is .NET Framework 4.8. There is no `IsExternalInit` polyfill, so `init` accessors, `record`, and `record struct` fail to compile with CS0518. Any new value type must be a plain `readonly struct`; any new reference type must be an ordinary class or interface.
  - `UiThread.cs` carries `#nullable enable` at line 1, so nullable-flow diagnostics on this file become build errors under the `/p:TreatWarningsAsErrors=true` gate. The nullable annotations on `IUiCaptureSource` and on `_syncContextForm` must be exact.
  - `UtilitiesCS.csproj` and `UtilitiesCS.Test.csproj` are legacy non-SDK `packages.config` projects with explicit `<Compile Include>` items. A new file that is not added to the project does not compile and its tests silently do not exist.
  - MSTest, Moq, and FluentAssertions only. Test files live in the matching `*.Test` project mirroring the production layout; colocation in the production tree is prohibited.
  - Creation or use of temporary files in tests is prohibited without exception.
  - Every acceptance criterion must be verifiable without a live Outlook process.
  - New analyzer diagnostics must not appear. The repository's analyzer severities are held at `suggestion` precisely because `TreatWarningsAsErrors` would otherwise promote them.
- External dependencies (services, libraries, releases):
  - None. No package is added, removed, or upgraded.

### Coverage-threshold divergence (recorded, not silently resolved)

The repository states two different line-coverage floors:

| Source | Line floor | Branch floor | New-code target |
|---|---|---|---|
| `CLAUDE.md` (General Unit Test Policy, UT2) | >= 80% | not stated | >= 90% for new modules, classes, methods |
| `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` | >= 85% | >= 75% | not stated |

`.claude/skills/policy-compliance-order/SKILL.md` places `CLAUDE.md` first in the precedence order, ahead of `.claude/rules/general-unit-test.md`. **`CLAUDE.md` therefore governs: the applicable floors for this delivery are >= 80% line and >= 90% for members newly added.** The divergence is recorded here rather than resolved by preference; reconciling the two documents is a separate governance change and is not in this delivery's scope.

Measured baseline for the file in scope, quoted by research R8 from the #782 evidence: **76.83% line, 65.00% branch, with 19 uncovered lines** — `28,29,30,32,33,34,67,68,69,70,71,72,73,74,75,76,118,119,120`. #782 waived raising it and recorded that doing so needs the seam extraction "already carved out to issues #787 and #788". This delivery is that carve-out. All 19 lines are reachable through the design above: the factory seam covers `67-76` (the `_monitorUiThread` branch), the AC1 and AC2 tests cover `28-34` (the two null-guard branches of `Init()`), and the lazy-path test covers `118-120`. Neither `UiThread` nor any nested type carries `[ExcludeFromCodeCoverage]`, and `coverage.config` excludes only third-party module paths, so no exclusion applies to this file.


## Data / API / Config Impact
- User-facing or API changes: none. No public signature changes; no UI, CLI, ribbon, or settings surface is touched. The two new members are `internal`.
- Data or migration considerations: none. No persisted data, schema, or settings file is read or written.
- Logging/telemetry updates (if any): none. See "Error handling and logging updates" for why no logger is introduced.
- Compatibility notes (CLI flags, config schemas, versioning): none. No `.runsettings`, `coverage.config`, `packages.config`, `.editorconfig`, or `BannedSymbols.txt` change. Two `.csproj` files change only by gaining `<Compile Include>` items for new source files.


## Test Strategy

All tests run without a live Outlook process. All new tests live in `UtilitiesCS.Test`, which holds the existing `InternalsVisibleTo` grant (decision D3); the one `QuickFiler.Test` edit uses only public and already-reflected surface.

### Seams

1. **`UiThread.SyncContextFormFactory`** — an injectable `Func<IUiCaptureSource>` that lets a test drive a *failing* `Initialize()` deterministically, with no form, no STA host, and no timing dependency. It also makes the `_monitorUiThread` block at `UiThread.cs:66-76` reachable, which is what closes 10 of the 19 uncovered lines.
2. **`UiThread.ResetForTesting()`** — restores the process-global statics. This is the principal AC4 obstacle today: research R3 found that **no test anywhere resets `_loaded`, `_autoScaleFactor`, `_uiThreadId`, or `_syncContextForm`**, so a test that drives `Init()` through a failure would consume the process-global latch for every later test in the assembly. Wrap it in a new `UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs` snapshot/restore `IDisposable`, modelled on the existing `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs`.

### Apartment-state mechanism

Use the two mechanisms MSTest already gives this repository (research R4):

- **STA test body**: `[STATestClass]` or `[STATestMethod]` from `MSTest.TestFramework` 4.4.0, already used at `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs:30`, `UtilitiesCS.Test/EmailIntelligence/FolderRemapViewer_Tests.cs:26`, and roughly a dozen other sites.
- **MTA test body**: a plain `[TestMethod]`. No attribute is needed; the default already is MTA.
- **STA delegate from an MTA test**: a dedicated `Thread` with `SetApartmentState(ApartmentState.STA)` before `Start()`, the shape of `StaDispatcherHost` at `UtilitiesCS.Test/Threading/UiThread_Tests.cs:186-213`, which runs `Dispatcher.Run()` and shuts down deterministically with `BeginInvokeShutdown` plus `Join`.

Do **not** change `ExecutionThreadApartmentState` in any `.runsettings`.

### `[DoNotParallelize]` requirements

Every test class that mutates any `UiThread` static must carry `[DoNotParallelize]`, because the statics are process-global for the whole test assembly and the reset scopes are explicitly documented as not thread-safe (`UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs:19-25`). Specifically:

- `UiThread_Dispatcher_Tests` already carries it (`UtilitiesCS.Test/Threading/UiThread_Tests.cs:129`) — unchanged.
- `SynchronizationContextAwaiter_Tests` (`UtilitiesCS.Test/Threading/UiThread_Tests.cs:9-104`) does **not** carry it today and does not currently mutate `UiThread` statics. The new AC3 cases install `_uiThreadId`, `_uiSyncContext`, and `_dispatcher`, so the attribute **must be added** as part of this delivery.
- The new `UiThreadInitContract_Tests` classes carry it from the start.

### Tests per defect

**#787 / AC1 — apartment-state rejection.**
1. Plain `[TestMethod]` (MTA): `Init()` throws `InvalidOperationException` whose message starts with `NonStaInitMessagePrefix` and names the observed apartment state.
2. Plain `[TestMethod]` (MTA): after the throw, initialization has not been recorded and the four monitoring-configuration fields are unchanged, read back through the reset scope. This is the direct test of "before any global is captured".
3. `[STATestMethod]`: `Init()` from an STA thread does not throw and populates all four capture fields.
4. `[STATestMethod]`: a caller whose apartment is `Unknown` is not directly constructible in MSTest; instead assert the predicate shape by driving an MTA thread and an STA thread and confirming the boundary is `== STA` rather than `!= MTA`. Where the `Unknown` case cannot be produced on this host, record it as untested rather than asserting it.

**#788 / AC2 — retry after a failed `Initialize()`.** All in `UtilitiesCS.Test`, `[STATestClass]` where `Initialize()` must succeed, `[DoNotParallelize]` throughout, every Act wrapped in `UiThreadStateScope`.
1. Factory throws, so `Init()` propagates; a second `Init()` with a working factory succeeds and populates all four capture fields. This is the AC2 core case.
2. Factory throws; the four capture fields are all still unset afterwards.
3. Factory throws; a subsequent read of `AutoScaleFactor` from an **MTA** thread throws the AC1 `InvalidOperationException` rather than re-entering the factory. **Assert this as a factory invocation count that did not increase, not as a wall-clock duration.** This is the direct anti-regression test for the #782 retry storm, and expressing it as an invocation count makes it deterministic and immune to host speed; a duration-based assertion would be a timing hack and is prohibited.
4. Two concurrent STA `Init()` calls invoke the factory exactly once, covering the serializing lock and the pre-existing C04 race.

**#784 / AC3 — the awaiter predicate.** Extending `SynchronizationContextAwaiter_Tests`.
1. Reference match with a non-null ambient context returns `true`, preserving the existing behaviour asserted at `UiThread_Tests.cs:37`.
2. Ambient context is `null` and `_context` is non-null returns `false`, protecting the two `TaskScheduler.FromCurrentSynchronizationContext()` sites.
3. `_uiThreadId == -1` returns `false`.
4. On an STA host thread: install `_uiThreadId` and `_dispatcher` for that thread, capture a context inside `dispatcher.Invoke(...)`, restore the WinForms ambient, then assert `IsCompleted == true`. **This is the AC3 defect test** — it is the only case that fails against the current code.
5. Same arrangement but the captured dispatcher context belongs to a different thread's dispatcher: `false`.
6. A foreign `WindowsFormsSynchronizationContext` while `_uiThreadId` equals the current thread id: `false`. **This is the regression guard for the `WinFormsPumpHostTests` failure mode**; it pins the reason the bare-id predicate was rejected.
7. `default(SynchronizationContextAwaiter).IsCompleted` on a context-free thread returns `true`, matching today's behaviour.

**Decision D4 (settled) — reconciling the MTA test caller.** Remove the `UiThread.Init(false)` call at `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs:329` and install a pumping dispatcher for the test's duration through the existing `QuickFiler.Test` machinery: `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` (`Exchange(...)` / `BeginTransactionAsync()` plus `UiThreadDispatcherTransaction.Install(...)`, `:55-63`, `:122-126`, `:242-254`), supplied with a dispatcher from a host that actually pumps — `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` (whose WPF dispatcher route is already asserted to execute on the pump thread by `BothMarshalRoutes_WpfDispatcherAndSyncContext_ExecuteOnThePumpThread`), or a `Dispatcher.Run()` host of the `StaDispatcherHost` shape.

This is research option B. **Option A (`[STATestMethod]` on the existing method) is rejected** for two recorded reasons: it preserves the order-dependency rather than removing it, and can convert it into a test hang — if `_loaded` was already consumed on another thread, `Init()` becomes a no-op, `UiThread.Dispatcher` returns a foreign thread's dispatcher, and `Invoke` posts to a dispatcher with no running frame and blocks until timeout; and it leaves a never-shut-down `Dispatcher` on a pooled MSTest STA worker, which is precisely the hazard #782 finding C10 removed from `UiThread_Tests.cs`. Option C (the fixture's `EnsureDispatcher()`) is rejected because its parked dispatcher never runs a frame and `Invoke` would block forever. Option D is out of scope, above.

Only two assertions in that test depend on `UiThread` — `:356` and `:357`, both requiring that the lambda passed to `Dispatcher.Invoke` executed before `Invoke` returned. Option B preserves both, because `Invoke` from the MSTest thread marshals to the pumping host thread, runs the lambda, and returns synchronously.

### Existing tests that must keep passing

Research R7 enumerated these. Each must be re-run and reported:

- `UtilitiesCS.Test/Threading/UiThread_Tests.cs` — all five `SynchronizationContextAwaiter_Tests` methods and both `UiThread_Dispatcher_Tests` methods. `IsCompleted_WhenContextIsNotCurrent_ReturnsFalse` (`:23`) stays false through the `ambient is null` early return; `IsCompleted_WhenContextMatchesCurrent_ReturnsTrue` (`:37`) stays true through the reference fast path.
- `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` — the `DispatcherNotInitializedMessage` assertion at `:136` and the `UiThread.Init()` substring assertion at `:196`.
- `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` — the `ForceDispatcherNull` region (`:137-171`, `:225-245`).
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs:462` — reflects on `_uiSyncContext`; breaks only if that field is renamed, which this design does not do.
- `UtilitiesCS.Test/EmailIntelligence/FolderRemapViewer_Tests.cs:118` and `UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersViewer_Tests.cs:39` — the two in-repo tests that actually drive `Init()` to success. Both must remain `[STATestClass]`.
- `QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs:183` and `:218` — the two tests a bare-id predicate would break.
- `QuickFiler.Test/Controllers/EfcFormControllerTests.cs:392` — injects a bare `new SynchronizationContext()`; the ambient on the MSTest thread is null, so it still posts.
- `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` and the five `QfcItemController` fixture consumers — all `UiThread._dispatcher` reflection consumers.

### Coverage impact and targets

Target `UtilitiesCS/Threading/UiThread.cs` at >= 80% line coverage per `CLAUDE.md`, up from the 76.83% line / 65.00% branch baseline, and each newly added member at >= 90%. Produce the coverage report with the repository's `vstest.console.exe ... /EnableCodeCoverage` command and store it under the feature folder's `evidence/qa-gates/` directory. `evidence/qa-gates/` is the canonical location for QA gate output under `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`; `evidence/coverage/` is not one of the canonical evidence sub-paths and must not be used.

### Validation

- Unit coverage areas: `UiThread.Init`, `UiThread.Initialize`, `SyncContextForm.CaptureUiVariables`, `UiThread.SynchronizationContextAwaiter`.
- Integration scenario to retest: full nine-assembly `/InIsolation` run, with `TryAddValuesAsync_UpdatesExistingValue` recorded explicitly across at least three repetitions per decision D5.
- Manual verification on a live host (not an acceptance criterion; reported separately): QuickFiler launch, item load, and breadcrumb open. Confirm no change in observable UI behaviour and no new keyboard-focus regressions after #677 and #796.
- Toolchain commands, run in order and restarted from step 1 on any failure or auto-fix: `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`); `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`; `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`.


## Acceptance Criteria

- [x] AC1: `Init()` throws a named `InvalidOperationException` when called from a non-STA thread, before any global is captured; the MTA test caller at `QfcHomeControllerRunAsyncTests.cs:329` is corrected or given an STA host.
- [x] AC2: A failed `Initialize()` does not consume the latch; a subsequent `Init()` retries and succeeds. The #782 regression scenario is reproduced as a test and passes with the chosen design.
- [x] AC3: `SynchronizationContextAwaiter.IsCompleted` returns true on the owning UI thread regardless of ambient context instance, and false elsewhere; ordering-sensitive callers in `ItemViewer` and `EfcFormController` still pass their existing tests.
- [x] AC4: Unit tests cover STA/MTA rejection, latch re-arm after throw, and awaiter inline-vs-post decisions with a fake dispatcher seam; no real Outlook host.
- [x] AC5: An evidence artifact under this feature folder's `evidence/other/` directory records a measurement of whether `new SyncContextForm(); Show();` throws when executed on an MTA thread on the execution host, and the AC2 regression test is justified against that measured result rather than against the #782 narrative. The full-suite run records `UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` across at least three repetitions, and no single failure of that test is attributed to this delivery.
- [x] AC6: A coverage report produced by `vstest.console.exe ... /EnableCodeCoverage` and stored under this feature folder's `evidence/qa-gates/` directory shows `UtilitiesCS/Threading/UiThread.cs` at >= 80% line coverage (the `CLAUDE.md` floor, which takes precedence per `.claude/skills/policy-compliance-order/SKILL.md`), above the 76.83% baseline, with each member newly added by this delivery at >= 90% and no changed line losing coverage.

AC1 through AC4 are reproduced verbatim from the `## Acceptance Criteria` section of `issue.md`. AC5 and AC6 are added by this specification for outcomes the research established that AC1 through AC4 do not cover: the required measurement behind AC2's #782 clause, and the coverage uplift that #782 waived and explicitly carved out to this issue.


## Risks & Mitigations

- Technical or operational risks:
  - **Process-global static state shared across an entire test assembly.** `UiThread`'s captured fields, the initialization flag, and the factory are process-wide for the whole `UtilitiesCS.Test` run. A test that mutates any of them changes the premise of every later test in that process, and research R3 found that no existing test resets four of those fields at all. Concretely, `UtilitiesCS.Test/Threading/UiThread_Tests.cs:151-156` already documents an observed cross-class effect from `QfcHomeControllerRunAsyncTests` populating the same static from a different assembly's run.
  - **Order-dependency in the current tests.** `QfcHomeControllerRunAsyncTests.cs:326` passes today only because `Init()` captured `Dispatcher.CurrentDispatcher` on the same MSTest worker that later calls `Invoke`, so `CheckAccess()` is true and the delegate runs inline. There is no message pump anywhere in that test. If an earlier test had consumed the latch or installed a foreign dispatcher, the `Invoke` would use a stale dispatcher or block.
  - **AC3 changes execution ordering at eleven production await sites.** Research R6 enumerated them and found **no existing test asserts ordering at any of them**, so the suite cannot detect an ordering regression. The highest-consequence sites are `QuickFiler/Controllers/EfcFormController.cs:877` (`Close()` then `Cleanup()` would run before already-queued UI work rather than after it), `QuickFiler/Controllers/QfcCollectionController.cs:782` (a `TlpLayout` toggle and a row removal would run before queued layout work), and the two `TaskScheduler.FromCurrentSynchronizationContext()` sites where the resulting scheduler would target the persistent WinForms context instead of the dispatcher context.
  - **The AC2 regression test may be vacuous.** If `Initialize()` cannot be made to throw on this host by any in-tree path, a test that claims to reproduce the #782 scenario would assert nothing about the real failure mode.
  - **Three production sites gain a new throw**, all currently unreachable in tests and unreachable in production after `ThisAddIn.cs:35` runs, but reachable in any future headless or worker-thread caller.
  - **Legacy csproj items are easy to omit.** A new test file that is not added to `UtilitiesCS.Test.csproj` compiles into nothing and its tests silently do not exist, which would make AC4 appear satisfied when it is not.

- Mitigations and rollbacks:
  - Introduce `ResetForTesting()` plus a `UiThreadStateScope` snapshot/restore `IDisposable`, and require `[DoNotParallelize]` on every class that mutates a `UiThread` static. This is the direct mitigation for the shared-state risk and is a precondition of AC4, not an optional extra.
  - Apply decision D4: remove the `Init()` dependency from `QfcHomeControllerRunAsyncTests` entirely and install a pumping dispatcher scoped to the test. This makes the test order-independent rather than merely making it pass.
  - For the ordering risk, add the AC3 regression guard that pins a foreign `WindowsFormsSynchronizationContext` to `false`, and confirm on a live host that QuickFiler launch, item load, and breadcrumb open are unchanged. This is a residual risk that the automated suite cannot fully close; state it as residual in the review rather than as covered.
  - For the vacuity risk, apply decision D5: measure first, then restate the AC2 test in terms of the measured result — a forced-throw scenario driven through the factory seam if the #782 narrative is refuted.
  - Verify the two `.csproj` edits by confirming the new tests appear in the vstest discovery output, not by inspecting the project files alone.
  - Rollback is a revert of the Write Set. No data migration, no feature flag, and no configuration change has to be undone.


## Rollout & Follow-up
- Release/rollout steps: ships in the ordinary add-in build. No staged rollout, no flag, no configuration change. The change is inert until `ThisAddIn_Startup` calls `Init()`, which it already does on the Outlook STA.
- Post-fix monitoring or clean-up tasks:
  - Close #784, #787, and #788 with a pointer to #809.
  - Correct the GitHub issue body for #809, which still carries the original incorrect bare-owning-thread-identity sentence. The copy in `issue.md` has already been corrected.
  - If the decision-D5 measurement refutes the #782 narrative, record that outcome against the #782 feature folder so a future reader does not re-derive the withdrawn constraint from the original artifacts.
  - Follow-up candidates, not in this delivery: routing `QfcHomeController` through `IUiDispatcher`; reconciling the 80% versus 85% coverage-floor divergence between `CLAUDE.md` and `.claude/rules/general-unit-test.md`; and the ordering-assertion gap at the eleven awaiter call sites, none of which has an ordering test today.
- Links:
  - Issue: https://github.com/drmoisan/TaskMaster/issues/809
  - Superseded: #784, #787, #788. Related: #781 (breadcrumb UI-boundary guard), #782 (PR 778 post-merge residuals), #780 (`TryAddValuesAsync` flake).
  - Research of record: `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/research/research.2026-09-07T20-20.md`
  - Requirements of record: `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/issue.md`
