# breadcrumb-suggestions-upgrade-silently-stale-on-superseded-lease (Issue #502)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/breadcrumb-suggestions-upgrade-silently-stale-on-superseded-lease/ (Issue #502)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #502
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/502
- Last Updated: 2026-08-08
## Summary

`BreadcrumbCoordinatorUpgradeLifetime.RunSynchronous` discards the `bool` that `TryRunCurrent`
returns, so when a lease has been superseded the guarded action is skipped silently. In
`BreadcrumbBridgeCoordinator.SetSuggestions` the assignment to the public `SuggestionsUpgrade` handle
lives *inside* that skipped action, so the method returns normally while `SuggestionsUpgrade` still
holds its previous value and the caller awaits an upgrade that will never run.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (C# / .NET Framework 4.8.1 WinForms VSTO add-in with Microsoft WebView2)
- Command/flags used: n/a — reached through the QuickFiler ItemViewer breadcrumb selector
- Data source or fixture: two breadcrumb suggestion populations issued close enough together that the
  first lease is superseded before its guarded action runs

## Steps to Reproduce

This is a concurrency and ordering defect established by code inspection. No existing test reproduces
it, because nothing in the suite supersedes a lease between `BeginPopulation` and `RunSynchronous`.

1. Call `BreadcrumbBridgeCoordinator.SetSuggestions` to begin a suggestion population.
2. Arrange for the lease to be invalidated between `BeginPopulation`
   (`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:104`) and `RunSynchronous` (`:105`) — nothing
   spans the two atomically.
3. Await the coordinator's `SuggestionsUpgrade` handle.

## Expected Behavior

Either the caller learns that the population was superseded — via a return value, an exception, or a
`SuggestionsUpgrade` set to a completed or cancelled task — or `SuggestionsUpgrade` is left in a state
that cannot be mistaken for a fresh in-flight upgrade.

## Actual Behavior

`SetSuggestions` returns normally. `SuggestionsUpgrade` silently retains its previous value, so a
caller awaiting it either waits on a stale completed task or believes a new upgrade is in flight when
none is. Nothing observable distinguishes this from a successful population.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: n/a — the defect is a silent no-op with no error text.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Rationale: reachable only under a supersession race that current call patterns make narrow, and the
observable consequence is a stale suggestion set rather than data loss. Recorded for traceability
because the silent-skip mechanism is general and would become more reachable if suggestion population
were made more concurrent.

## Suspected Cause / Notes

Verified call chain:

1. `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs:115` — `RunSynchronous` calls
   `TryRunCurrent` and **discards its `bool` result**. This is the structural mechanism.
2. `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs:141` — `TryRunCurrent` evaluates lease
   currency and returns `false` without running the action when the lease is not current.
3. `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:105-114` — `SetSuggestions` passes a lambda to
   `RunSynchronous`, and `:112` assigns `SuggestionsUpgrade = PopulateSuggestionsAsync(rows, lease)`
   **inside** that lambda. If the lambda never runs, the assignment never happens.
4. Nothing spans `BeginPopulation` (`:104`) and `RunSynchronous` (`:105`) atomically, so the
   supersession window is real.

`AddItems` (`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:131-147`) has the same structure but
exposes no observable handle at all — its dispatch task is discarded at `:141` — so the same skip is
entirely invisible there.

Discovered during preparation research for epic #136 child F12 (issue #495), recorded as LD-3 in
`.../research/2026-08-08T01-15-breadcrumb-bridge-coordinator.md` (the observable symptom) and as LD-B
in `.../research/2026-08-08T01-15-breadcrumb-coordinator-upgrade-lifetime.md` (the mechanism), both
under `docs/features/active/2026-08-08-quickfiler-breadcrumb-bridge-coverage-495/`.

Distinct from the lock-scope defect filed from the same research, which concerns `action()` executing
*inside* `_sync` rather than the discarded return value. The two share a file and should be
cross-linked but have different fixes.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: a failing regression test first, per the repository Bugfix Workflow,
      superseding a lease between `BeginPopulation` and `RunSynchronous` and asserting the caller can
      observe that the population did not run.
- [ ] Integration scenario to retest: rapid successive breadcrumb suggestion populations, confirming
      the surface reflects the latest set.
- [ ] Manual verification notes: candidate fixes are to surface `TryRunCurrent`'s `bool` through
      `RunSynchronous` to its callers, or to hoist the `SuggestionsUpgrade` assignment out of the
      guarded lambda so the handle is always replaced. Both change an observable contract, which is
      why this was out of scope for #495 under the epic's no-behavior-change NFR.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
