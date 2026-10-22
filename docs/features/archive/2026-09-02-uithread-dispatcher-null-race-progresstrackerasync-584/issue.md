# uithread-dispatcher-null-race-progresstrackerasync

- Work Mode: full-bug
- Issue: #584
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/584
- Promotion Type: bug
- Base Branch: main
- Merge Base: 5ebaaf105d8241f309f704d1ff90af2e32e5a6c1
- Branch: bug/uithread-dispatcher-null-race-progresstrackerasync-584
- Last Updated: 2026-09-02

## Note on this file's provenance

`mcp__drm-copilot__new_active_feature_folder` produced `spec.md` and a plan template for this
folder but no `issue.md`, because there is no promotable source under
`docs/features/potential/**`: the GitHub issue body itself records that the originating potential
document was authored inside an epic-child worktree that is prohibited from committing under
`docs/features/potential/**`, so issue #584 is the durable record, not a file in this repository.
This `issue.md` was authored by hand from `gh issue view 584 --json body` plus the issue's comment
thread, per the recorded exception for this scenario.

## Summary

`UtilitiesCS.Threading.UiThread.Dispatcher` is a static property backed by a `null!`-initialised
field with no lazy initialisation. When it is read before `Initialize()` has completed, it returns
`null`, and `ProgressTrackerAsync.InitializeAsync()` dereferences it immediately. The result is a
non-deterministic `NullReferenceException` that surfaces only under full-suite CPU load.

This was observed once during the post-change QC run for issue #449 and did not reproduce in
isolation or in two subsequent clean full-suite runs. No test was modified, no retry was added, and
no timing tolerance was applied to hide it.

## Environment

(not provided in the potential file; not required to reproduce — the defect is structural, not
environment-specific. See Root Cause below.)

## Steps to Reproduce

(not provided in the potential file as a deterministic repro; the issue was observed once during a
full-suite run and is not reliably reproducible on demand by timing alone. A deterministic
regression test is required instead — see "Deterministic repro strategy" below.)

## Expected Behavior

(not provided in the potential file; inferred from the issue's own "Proposed direction": accessing
`UiThread.Dispatcher` before `UiThread.Init()` has completed should either lazily initialise or fail
fast with a clear, explicit exception naming the missing `Initialize()` call, instead of silently
returning `null` and surfacing as an unattributed `NullReferenceException` at a downstream call
site.)

## Actual Behavior

(not provided in the potential file; from the issue body: a `NullReferenceException` is thrown at
`UtilitiesCS/Threading/ProgressTrackerAsync.cs:35`, inside `ProgressTrackerAsync.InitializeAsync()`,
because `UiThread.Dispatcher` returned `null`.)

## Logs / Screenshots

(not provided in the potential file.)

## Impact / Severity

(not provided in the potential file. From the issue body's "Impact" section: erodes trust in the
full-suite gate because a non-deterministic failure cannot be distinguished from a real regression;
production reachability of the same null-return path is unassessed; the `null!` suppression is
load-bearing and hides the hazard from the nullable analyser.)

## Root cause — structural, not merely a timing flake

Verified against `origin/main` at `5ebaaf105d8241f309f704d1ff90af2e32e5a6c1` on 2026-09-02.

`UtilitiesCS/Threading/UiThread.cs:135-140`:

```csharp
public static Dispatcher Dispatcher
{
    get => _dispatcher;
    private set => _dispatcher = value;
}
private static Dispatcher _dispatcher = null!; // set in Initialize() before any access
```

The accessor returns the backing field unconditionally, with no guard and no lazy initialisation.
The `null!` null-forgiving operator suppresses the nullable-flow diagnostic that would otherwise
flag the hazard.

`UiThread.AutoScaleFactor` and `UiThread.UiSyncContext` in the same file are lazy-initialising
counter-examples in the same class, demonstrating the omission on `Dispatcher` is unintentional
rather than a deliberate design choice.

Consumer, `UtilitiesCS/Threading/ProgressTrackerAsync.cs:31-35`:

```csharp
public async Task<ProgressTrackerAsync> InitializeAsync()
{
    UiDispatcher = UiThread.Dispatcher;

    await UiDispatcher.InvokeAsync(() =>
```

`UiDispatcher` is assigned from the static and dereferenced on the next statement with no guard, so
a null static becomes an unattributed `NullReferenceException` at the `InvokeAsync` call site.

## Existing repository precedent for the fix shape

`UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs:57-66` already treats a null
`UiThread.Dispatcher` fallback as an explicit, strict contract violation:

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

This is the same idiom the issue's own "Proposed direction" item 2 recommends applying at the
source (`UiThread.Dispatcher` itself) instead of duplicating it per call site.
`UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` (`AddEntry_UseUiThreadTrue_...`) already
asserts only that no exception escapes `IdleAsyncQueue.OnApplicationIdle`'s internal
`catch (Exception ex)` when the dispatcher is unavailable, and does not assert on the concrete
exception type, so converting the failure from `NullReferenceException` to `InvalidOperationException`
at the source does not change that test's outcome.

## Deterministic repro strategy

Because the original failure is timing-dependent (1 of 3 full-suite runs; 0 of 1 isolated runs), the
regression test does not attempt to reproduce the race by timing. Instead it uses the same
reflection-based static-field access pattern already established in this test assembly (for example
`UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs`'s `ForceDispatcherNull`/`RestoreDispatcher`
helpers) to force `UiThread`'s private `_dispatcher` backing field to its pre-`Initialize()` state
(`null`) and assert on `UiThread.Dispatcher`'s accessor contract directly. This is deterministic,
requires no sleep/retry/timing tolerance, and directly exercises the structural defect rather than
its timing-dependent symptom.

## Scope boundary for this fix

- In scope: `UtilitiesCS/Threading/UiThread.cs` (the `Dispatcher` accessor's null contract) and a
  new deterministic regression test in `UtilitiesCS.Test/Threading/`.
- Verified not required: `UtilitiesCS/Threading/ProgressTrackerAsync.cs`. Once `UiThread.Dispatcher`
  fails fast instead of returning `null`, the exception is raised at
  `UiDispatcher = UiThread.Dispatcher;` (the property access itself), before
  `UiDispatcher.InvokeAsync(...)` is ever reached, so the consumer already receives a
  self-diagnosing failure without a code change. Recorded here because the assignment prompt named
  this file as a fix site to verify, not assume.
- Out of scope, per the issue's own comment thread: the "injectable-seam conversion" replacing the
  ~62 remaining direct reads of `UiThread.Dispatcher` across ~29 production files with the existing
  `IUiDispatcher` seam. That is a multi-phase, multi-assembly refactor with no bounded blast radius,
  already identified and explicitly deferred by the maintainer's own analysis on this issue thread.
  Also out of scope: adding synchronization around the shared static in
  `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`'s existing reflection-based mutation of
  `UiThread._dispatcher` (a `#493`-shaped test-isolation concern in a different test assembly than
  `#493` itself covered). This is noted as a candidate follow-up, not addressed here, to keep this
  fix to the 1-3 production-file budget described in the assignment.
- Explicitly not touched, per binding scope constraints: `.claude/**`, `.codex/**`, `.agents/**`,
  `config/blast-radius.json`, `config/orchestration-routing.json`.

## Cross-references

- #493 (`uithread-dispatcher-static-swap-no-restore`): adjacent, test-side isolation defect on the
  same static; already resolved independently, does not touch `UtilitiesCS/Threading/UiThread.cs`.
- #508 (`wpf-dispatcher-yield-test-order-dependent`, archived): established the
  `InvalidOperationException`-on-missing-dispatcher contract for `WpfDispatcherYield` and confirmed
  via its own dependency/risk notes that "any broader refactor of `UiThread` static state" was out of
  that issue's scope — the narrow fix proposed here is exactly that deferred, bounded follow-up for
  the `Dispatcher` accessor itself.

## Source

From: docs/features/potential/2026-08-22-uithread-dispatcher-null-race-progresstrackerasync.md
(not present on disk; see "Note on this file's provenance" above)
