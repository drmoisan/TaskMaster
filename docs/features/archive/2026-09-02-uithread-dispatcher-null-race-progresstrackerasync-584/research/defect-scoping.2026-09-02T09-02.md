# Defect scoping research — #584 uithread-dispatcher-null-race-progresstrackerasync

Timestamp: 2026-09-02T09-02
Verified against: `origin/main` at commit `5ebaaf105d8241f309f704d1ff90af2e32e5a6c1`

## Why this research was needed

The GitHub issue body's `Environment`, `Steps to Reproduce`, `Expected Behavior`, and
`Actual Behavior` sections are empty (`(not provided in potential file)`). The issue's own comment
thread explains why: `mcp__drm-copilot__potential_to_issue` extracts only the `## Summary` section
from a potential document; the fuller analysis was posted as an issue comment instead. This
research establishes the defect directly against current `origin/main` rather than trusting the
issue body alone.

## Step 1 — Confirm the defect exists on `origin/main`

Command: `git show origin/main:UtilitiesCS/Threading/UiThread.cs`

Result (lines 135-140 of the file as committed):

```csharp
public static Dispatcher Dispatcher
{
    get => _dispatcher;
    private set => _dispatcher = value;
}
private static Dispatcher _dispatcher = null!; // set in Initialize() before any access
```

Confirmed: no null guard, no lazy initialisation. Contrast with the same file's `UiSyncContext`
(lines ~122-131) and `AutoScaleFactor` (lines ~148-158), both of which check
`if (_field is null) { Init(); }` before returning. `Dispatcher` is the only one of the three
UI-thread-state properties in this class without that pattern — direct evidence the omission is an
inconsistency rather than a deliberate design choice.

Command: `git show origin/main:UtilitiesCS/Threading/ProgressTrackerAsync.cs`

Result (lines 31-35):

```csharp
public async Task<ProgressTrackerAsync> InitializeAsync()
{
    UiDispatcher = UiThread.Dispatcher;

    await UiDispatcher.InvokeAsync(() =>
```

Confirmed: `UiDispatcher` (a `Dispatcher`-typed field on `ProgressTrackerAsync`, unrelated to
`UiThread`'s own dispatcher field) is assigned directly from `UiThread.Dispatcher` and dereferenced
on the following line with no guard. If `UiThread.Dispatcher` is `null`, `UiDispatcher.InvokeAsync`
throws `NullReferenceException` here — matching the issue body's reported stack location
(`ProgressTrackerAsync.cs:35`).

## Step 2 — Enumerate every production consumer of `UiThread.Dispatcher`

Command: `git grep -n "UiThread.Dispatcher\b" -- '*.cs'` (repo root, excluding `obj/`)

Full result set (production, non-test files only):

- `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs:441` — commented out, not live code.
- `UtilitiesCS/HelperClasses/ThemeHelpers/ThemeControlGroup.cs:218,222` — unguarded direct calls.
- `UtilitiesCS/HelperClasses/ToolTips/QfcTipsDetails.cs:254,277` — unguarded direct calls.
- `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs:46,57` — already guarded; see Step 3.
- `UtilitiesCS/Threading/IdleActionQueue.cs:78` — unguarded, but the caller
  (`OnApplicationIdle`-style dispatch) wraps in `catch (Exception ex)`; see Step 4.
- `UtilitiesCS/Threading/IdleAsyncQueue.cs:72` — unguarded, wrapped in `catch (Exception ex)`;
  see Step 4 (this is the file with the pre-existing `ForceDispatcherNull` test helper the new
  regression test's technique is modeled on).
- `UtilitiesCS/Threading/ProgressTracker.cs:33,39` — unguarded direct calls (synchronous sibling of
  `ProgressTrackerAsync`; same shape, not named as a fix site by the assignment, not touched here).
- `UtilitiesCS/Threading/ProgressTrackerAsync.cs:33,39` — the reported fix site; see Step 1.
- `UtilitiesCS/Threading/ProgressTrackerPane.cs:13,16` — unguarded direct calls.
- `UtilitiesCS/Threading/WpfUiDispatcher.cs:25` — default constructor argument
  `: this(() => UiThread.Dispatcher)`; the lambda is only invoked when the seam's default provider
  is used, and the seam already exists specifically to let callers substitute a test double instead.
- `QuickFiler/Controllers/EfcHomeController.cs:297`, `EfcItemController.cs:998,1007`,
  `KeyboardHandler.cs:362,370,401`, `QfcCollectionController.cs` (8 call sites),
  `QfcFormController.Actions.cs:255`, `QfcFormController.EventHandlers.cs:197,237,242`,
  `QfcHomeController.cs:360`, `QfcQueue.cs:474,482,490`,
  `Helper Classes/ConversationResolver.Loading.cs:150,320`,
  `Helper Classes/EfcViewerQueue.cs:20,67`, `Helper Classes/EmailMoveMonitor.cs:40`,
  `Helper Classes/ItemViewerQueue.cs:21,27,88,90` — all unguarded direct calls; these are live
  production UI-thread marshalling call sites reachable only from a running VSTO/Outlook host where
  `UiThread.Init()` has already run during add-in startup (per `TaskMaster/ThisAddIn.cs:190,227`),
  not from a headless unit test host.
- `TaskMaster/AppGlobals/ApplicationGlobals.cs:293`,
  `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs:344`,
  `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs:71,114`, `TaskMaster/ThisAddIn.cs:227` —
  same category as above; production host-startup code.

Conclusion: none of the unguarded production call sites require a code change for this fix. Every
one of them today either (a) is inside VSTO/Outlook host code that only executes after
`UiThread.Init()` has run at add-in startup, so it is not exposed to the pre-`Init()` null state at
all, or (b) is inside `IdleAsyncQueue`/`IdleActionQueue`'s dispatch loop, which already wraps the
call in a broad `catch (Exception ex)` (see Step 4).

## Step 3 — Existing repository precedent for the fix shape

`UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs:57-66`:

```csharp
// UiThread.Dispatcher is set-once state populated by UiThread.Init() and is null
// outside a live host, so that null state is surfaced as InvalidOperationException to
// preserve the strict contract callers relied on.
Dispatcher? dispatcher =
    _currentThreadDispatcherProvider() ?? _fallbackDispatcherProvider();
if (dispatcher is null)
{
    throw new InvalidOperationException(
        "The UI dispatcher has not been captured. Call UiThread.Init() before yielding folder tree work."
    );
}
```

This file's `_fallbackDispatcherProvider` field is typed `Func<Dispatcher?>` and its production
default is `() => UtilitiesCS.UiThread.Dispatcher` — i.e., this call site already treats
`UiThread.Dispatcher`'s return value as potentially null at runtime, despite the property's
non-nullable compile-time signature. This is the strongest available evidence that the correct fix
shape for #584 is to push this same `InvalidOperationException` contract into `UiThread.Dispatcher`
itself, rather than requiring every consumer to duplicate the guard.

Archived issue #508 (`docs/features/archive/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/issue.md`)
independently confirms this precedent: it fixed `WpfDispatcherYieldTests`'s dependence on ambient
`UiThread.Dispatcher` state via an injectable seam, explicitly preserved the
`InvalidOperationException` contract as a stated acceptance criterion (AC2), and recorded "any
broader refactor of `UiThread` static state" as out of that issue's scope — i.e., the maintainer
already flagged the `UiThread.Dispatcher` accessor itself as a known, deliberately deferred fix
site, which #584 is.

## Step 4 — Verify no existing test depends on a silent-null (non-throwing) `UiThread.Dispatcher`

`UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`,
`AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException` (read in full):

- Forces `UiThread._dispatcher` to `null` via reflection (its own `ForceDispatcherNull` helper).
- Asserts `actDelegate.Should().NotThrow(...)` around `InvokeOnIdle()`, whose target method
  (`IdleAsyncQueue.OnApplicationIdle`) wraps the `UiThread.Dispatcher.InvokeAsync(...)` call in
  `catch (Exception ex)`.
- Asserts `callCount.Should().Be(0, "action must not run when the UiThread Dispatcher is
  unavailable")`.
- Does **not** assert on the concrete exception type. The doc-comment on the test explicitly says
  "the `NullReferenceException` is caught by the internal try/catch," but the assertion itself is
  type-agnostic (`NotThrow()` on the wrapping delegate, not `Should().Throw<NullReferenceException>()`
  on the inner call). Converting the thrown type to `InvalidOperationException` at the source
  changes nothing this test can observe.

`UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` (read in full, 4 test methods):

- All four tests construct `WpfDispatcherYield` with explicit injected `Func<Dispatcher?>` provider
  delegates (`CountingDispatcherProvider`). None of them exercises the parameterless constructor
  `WpfDispatcherYield()`, so none of them ever calls the real `UiThread.Dispatcher` property. The
  fix under #584 cannot affect these tests' outcomes.

`UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceConcurrencyTests.cs`,
`GetSnapshotAsync_WorkerOriginatedColdBuild_UsesCapturedStaDispatcher` (read in full):

- Uses `new WpfDispatcherYield()` (the parameterless constructor, so its fallback provider is the
  real `UiThread.Dispatcher`), on a worker thread with no thread-affinitized dispatcher.
- If `UiThread.Dispatcher` is null at the time this test runs (test-order-dependent; see archived
  #508's analysis of the same ambient-state hazard), `WpfDispatcherYield.YieldAsync` already throws
  `InvalidOperationException` today (`WpfDispatcherYield.cs`'s own existing guard), before and after
  this fix — the fix only moves the throw one call frame earlier, from `WpfDispatcherYield.cs`'s own
  `if (dispatcher is null)` check to `UiThread.Dispatcher`'s getter. The test asserts no exception
  type or message text from this path, so its pass/fail outcome is unchanged by this fix in either
  direction (both before and after, the test's outcome depends only on whether `UiThread.Init()` ran
  earlier in the process — a pre-existing, documented, and explicitly out-of-scope ambient-state
  dependency per #508's own "Dependencies / Risks" section, not introduced or worsened by #584's
  fix).

`UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`,
`InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker` (read in full): sets
`UiThread._dispatcher` via reflection to a real, valid `Dispatcher.CurrentDispatcher` instance
before calling `InitializeAsync()`, restoring the prior value in `finally`. Never exercises the
null path within its own assertions; the fix does not change this test's outcome. This test's
own reflection-based, unsynchronized mutation of the shared static is itself the most likely
mechanism behind the originally observed flake (a concurrently running test thread reading
`UiThread.Dispatcher` while this test's `finally` block is mid-restore to a prior `null` value) —
recorded as a follow-up in `spec.md`'s "Rollout & Follow-up," not fixed by this change.

## Conclusion / scope determination

The defect is confirmed present and structural on `origin/main`. The minimal, targeted fix is
confined to `UtilitiesCS/Threading/UiThread.cs`'s `Dispatcher` accessor plus one new deterministic
test file/class in `UtilitiesCS.Test/Threading/`. `UtilitiesCS/Threading/ProgressTrackerAsync.cs`
does not require a code change: the fix relocates the failure point to `UiThread.Dispatcher`'s own
getter, which is invoked from `ProgressTrackerAsync.cs:33` before any dereference happens, so the
same line already receives a self-diagnosing exception without modification. This conclusion is
recorded in `spec.md` AC3 as a plan-time hypothesis to be re-verified, not assumed, during
implementation.
