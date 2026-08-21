# breadcrumb-webview-post-executes-under-upgrade-lifetime-lock (Issue #500)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/breadcrumb-webview-post-executes-under-upgrade-lifetime-lock/ (Issue #500)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #500
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/500
- Last Updated: 2026-08-08
## Summary

`BreadcrumbCoordinatorUpgradeLifetime.TryRunCurrent` invokes the caller's action while holding its
`_sync` monitor, so a WebView2 post runs under two nested locks (`lifetime._sync` then `hub._sync`)
and reaches an out-of-process SDK call from inside both. Because `lock` is re-entrant, the lock does
not deliver the atomicity it appears to: a re-entrant call on the same thread can mutate `_current`
between the currency check and the completion of the action, which is the exact invariant
`TryRunCurrent` exists to enforce.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (C# / .NET Framework 4.8.1 WinForms VSTO add-in with Microsoft WebView2)
- Command/flags used: n/a — reached through the QuickFiler ItemViewer breadcrumb selector
- Data source or fixture: any breadcrumb suggestion population that issues a render/selector post

## Steps to Reproduce

This is a concurrency and re-entrancy defect established by code inspection rather than a
deterministic user-facing repro. No existing test reproduces it, and constructing one requires a
re-entrant STA message pump, which repository unit-test policy prohibits.

1. Populate breadcrumb suggestions so `BreadcrumbBridgeCoordinator.PostRenderAndSelectorAsync` runs.
2. Observe that `_messenger.PostJson` executes inside `BreadcrumbCoordinatorUpgradeLifetime._sync`.
3. In production the messenger is `BreadcrumbMessengerHub`, whose `PostJson` takes its own `_sync`
   and, still holding it, calls `PostToSurface`, reaching the WebView2 SDK.

## Expected Behavior

The currency check and the guarded action should be atomic with respect to lease invalidation, and
no out-of-process SDK call should be made while a lock is held. Locks should cover state mutation
only, with the action invoked outside them, re-checking currency as needed.

## Actual Behavior

The action runs inside the lock. Because `Monitor` is re-entrant, a re-entrant `BeginPopulation`,
`Invalidate`, or `TryDispose` on the same thread acquires `lifetime._sync` successfully and mutates
`_current` mid-action, defeating the guarantee. Separately, a re-entrant `Attach`/`Detach` during the
hub's broadcast would throw `InvalidOperationException` because the hub holds `_sync` across its
`foreach`.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: n/a — no exception is raised on the current wiring; the defect is a silently unenforced
  invariant.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Rationale: no deadlock is demonstrable on current code and the lock ordering is consistent, so this
is a latent correctness hazard rather than an active failure. It is recorded at Medium because an STA
COM call can pump messages and re-enter managed code, which is precisely the condition that voids the
intended atomicity.

## Suspected Cause / Notes

Verified call chain, established independently from three files during preparation research for epic
#136 child F12 (issue #495):

1. `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:266-275` — the render post is wrapped by
   `_upgradeLifetime.Guard(lease, ...)` and dispatched.
2. `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs:130` — `Guard` wraps the action in
   `() => TryRunCurrent(lease, action)`.
3. `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs:139-146` — `TryRunCurrent` takes
   `lock (_sync)` and calls `action()` at `:145` **inside** the lock.
4. `QuickFiler/Viewers/BreadcrumbMessengerHub.cs:126` — `PostJson` takes the hub's own `_sync` and,
   still holding it, calls `PostToSurface` at `:133`, holding `_sync` across the `foreach` at `:131`.

Supporting observations:

- **No lock inversion exists.** `BreadcrumbMessengerHub.OnSurfaceMessageReceived` (`:157-173`)
  snapshots the handler under its lock and invokes it outside, so the inbound path does not take the
  reverse order. No deadlock is demonstrable on current code.
- **The exposure is wider than `Guard`.** `RunSynchronous`
  (`BreadcrumbCoordinatorUpgradeLifetime.cs:115`) places the whole body of `SetSuggestions` under the
  lock, including the async upgrade kick-off.
- **Re-entrant self-acquisition is already routine on the happy path**, with no COM involved:
  `SetSuggestions` produces three nested acquisitions on one thread
  (`BreadcrumbCoordinatorUpgradeLifetime.cs:139` → `:105` → `:139`) because
  `BreadcrumbUiDispatcher.cs:84` executes inline.
- **The file contradicts itself.** `BreadcrumbCoordinatorUpgradeLifetime.cs` deliberately calls
  `lease.Cancel()`, `DisposeLease`, and `_report` *outside* the lock in five places; `:145` is the
  sole departure from its own convention.

Recorded as LD-1 in
`.../research/2026-08-08T01-15-breadcrumb-bridge-coordinator.md`, as LD-A in
`.../research/2026-08-08T01-15-breadcrumb-coordinator-upgrade-lifetime.md`, and as LD-1 in
`.../research/2026-08-08T02-10-breadcrumb-messenger-hub.md`, all under
`docs/features/active/2026-08-08-quickfiler-breadcrumb-bridge-coverage-495/`.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: a test that proves the currency invariant holds across a re-entrant
      mutation, using an injectable re-entrant action rather than a real message pump so it stays
      within unit-test policy.
- [ ] Integration scenario to retest: breadcrumb suggestion population followed by rapid
      re-population, confirming no stale render reaches the surface.
- [ ] Manual verification notes: the candidate fix is to move `action()` outside `lock (_sync)` in
      `TryRunCurrent` and re-check currency after the call, matching the convention the same file
      already follows in five other places. This changes concurrency semantics and so was out of
      scope for #495 under the epic's no-behavior-change NFR. Consider also narrowing the hub's
      `_sync` so `PostToSurface` is not called under it.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
