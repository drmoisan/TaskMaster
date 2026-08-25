# breadcrumb-coordinator-hub-defects (Issue #501)

- Issue: #501
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/501
- Also closes: #462, #500, #502
- Type: bug
- Work Mode: full-bug
- Epic: quickfiler-bug-family
- Owner: drmoisan
- Last Updated: 2026-08-24

## Summary

Four pre-existing defects in the QuickFiler breadcrumb coordinator and messenger hub share a single
root theme: an ordering or lifetime invariant that the code states but does not enforce. This one
feature closes all four. Each defect has a promoted potential document that carries the authoritative
file:line evidence, offending code block, root cause, and suggested fix.

| Issue | Defect | Owning production file | Severity |
| --- | --- | --- | --- |
| #462 | `CloseCore` never clears `_closePending` on the successful-close path, so a later `RequestOpen` silently returns the closed sentinel | `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | Medium |
| #500 | `TryRunCurrent` invokes the guarded action inside `_sync`, so a WebView2 post runs under nested re-entrant locks and the currency check is not atomic with the action | `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` (and hub `_sync` scope) | Medium |
| #501 | `PostJson` writes the replay cache before broadcasting and wraps the broadcast in no `try`/`catch`, so one throwing surface starves later attachments while the cache records delivery | `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | Medium |
| #502 | `RunSynchronous` discards `TryRunCurrent`'s `bool`, so a superseded lease silently skips the guarded action and `SuggestionsUpgrade` retains a stale handle | `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` and `BreadcrumbBridgeCoordinator.cs` | Low |

## Authoritative Requirement Sources

The promoted potential document for each issue is the authoritative requirement source and is richer
than the GitHub issue body:

- #462 — `docs/features/potential/promoted/2026-08-07-breadcrumb-dropdown-coordinator-stale-closepending-drops-reopen.md`
- #500 — `docs/features/potential/promoted/2026-08-08-breadcrumb-webview-post-executes-under-upgrade-lifetime-lock.md`
- #501 — `docs/features/potential/promoted/2026-08-08-breadcrumb-hub-postjson-caches-before-broadcast-starves-attachments.md`
- #502 — `docs/features/potential/promoted/2026-08-08-breadcrumb-suggestions-upgrade-silently-stale-on-superseded-lease.md`

## Ownership Boundary

Files this feature owns and may write:

- `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`
- `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs`
- `QuickFiler/Viewers/BreadcrumbMessengerHub.cs`
- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`

Files this feature must NOT write (owned by sibling epic children):

- `QuickFiler/Viewers/WebView2Messenger.cs` and `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` — sibling feature 476
- `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`, `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`, `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`, `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` — sibling feature 488

If a fix appears to require one of the excluded files, record it in `spec.md` as a cross-feature note
and keep it out of the plan.

## Test Placement Constraint

`QuickFiler.Test/Viewers/` already carries `Compile Include` entries for
`BreadcrumbDropDownOpenCoordinatorTests.cs` (plus `.Part2` and `.Part3`),
`BreadcrumbCoordinatorUpgradeLifetimeTests.cs`, `BreadcrumbMessengerHubTests.cs`,
`BreadcrumbMessengerHubCoverageTests.cs` and `BreadcrumbBridgeCoordinatorTests.cs`. Prefer adding test
methods to those files so no project-file edit is needed. A genuinely new test file requires a
`Compile Include` added only within the alphabetical `Breadcrumb*` neighbourhood of the item group at
`QuickFiler.Test/QuickFiler.Test.csproj` lines 57-175, which is shared with sibling children.

## Determinism Constraint

`.claude/rules/general-unit-test.md` prohibits real wall-clock waits, `Thread.Sleep` and `Task.Delay`
in test code. Every regression test for these defects must drive its ordering through injected
delegates, controllable seams, or synchronous fakes rather than timing.

## Acceptance Criteria

The authoritative acceptance criteria for this `full-bug` feature live in `spec.md`. This section is
a pointer, not a second source.

- See `docs/features/active/breadcrumb-coordinator-hub-defects-501/spec.md`, section `## Acceptance Criteria`.
