# breadcrumb-capturecurrentortests-silently-degrades-in-production (Issue #475)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/breadcrumb-capturecurrentortests-silently-degrades-in-production/ (Issue #475)
- Work Mode: full-bug
- Discovered during: preparation research for issue #455 (epic #136, child F13)

- Issue: #475
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/475
- Last Updated: 2026-08-08
## Summary

`BreadcrumbPopupUiOperations.CaptureCurrentOrTests()` inverts a deliberate fail-fast guard into a
silent degradation. When no `SynchronizationContext` is present it falls back to a **test-mode**
dispatcher whose documented contract is to *report* cross-thread work rather than schedule it. Four
production call sites use this method, so on any thread without a synchronization context the
breadcrumb popup silently never opens: no exception, no user-visible error, only a log line.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1 WinForms VSTO add-in with Microsoft WebView2
- Affected path: QuickFiler breadcrumb folder-selector drop-down construction

## Suspected Cause

`QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:43-54` establishes the intended production contract —
fail fast:

```csharp
internal static BreadcrumbUiDispatcher CaptureCurrent()
{
    SynchronizationContext context =
        SynchronizationContext.Current
        ?? throw new InvalidOperationException(
            "Breadcrumb UI components must be constructed on an owning UI synchronization context."
        );
    ...
}
```

`QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:86-89` overrides that contract:

```csharp
internal static BreadcrumbPopupUiOperations CaptureCurrentOrTests() =>
    SynchronizationContext.Current == null
        ? CreateForCurrentThreadTests()
        : CaptureCurrent();
```

The fallback target is documented at `BreadcrumbUiDispatcher.cs:58-60` as:

> Creates an owner-thread-only boundary for host-neutral unit tests without a UI pump.
> Cross-thread work is reported instead of being scheduled on a generic context.

So the exact condition the production guard was written to reject — a missing synchronization
context — is the condition that silently selects a dispatcher that does not marshal.

## Production Call Sites (verified 2026-08-07)

```
QuickFiler/Viewers/BreadcrumbDropDownHost.cs:98
QuickFiler/Viewers/BreadcrumbDropDownHost.cs:118
QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:156
QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:192
```

None is test-only. `CreateForCurrentThreadTests()` is named for test use but is reachable from all
four.

## Steps to Reproduce

1. Construct the breadcrumb drop-down host from a thread where `SynchronizationContext.Current` is
   null — for example a thread-pool continuation, a background worker, or any path that has lost
   the WinForms context.
2. Request the drop-down open.
3. Observe that no popup appears, no exception is raised, and the only trace is a reported failure
   through the dispatcher's error sink.

## Expected Behavior

Production construction off the owning UI synchronization context is a programming error and should
fail fast with the `InvalidOperationException` that `CaptureCurrent()` already defines. The
test-mode dispatcher should not be reachable from production call sites.

## Actual Behavior

The construction succeeds, the drop-down is wired to a dispatcher that reports rather than
marshals, and the feature silently does nothing.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Severity is High because the failure mode is silent and user-facing: the folder selector simply
does not open, with no diagnostic surfaced to the user and no exception to correlate in a crash
report. It also violates `CLAUDE.md` § "Error Handling" ("fail fast and explicitly; do not silently
ignore errors") and `.claude/rules/general-code-change.md` § "Error Handling and Logging".

There is a secondary design concern: a test-only affordance is reachable from production code. The
repository's determinism guidance expects test seams to be injected by the test, not selected at
runtime by probing ambient state.

## Suggested Remediation

Preferred: delete `CaptureCurrentOrTests()` and have the four production call sites use
`CaptureCurrent()`, restoring fail-fast. Tests construct `BreadcrumbPopupUiOperations` through its
existing injectable constructor (`BreadcrumbPopupUiOperations.cs:62-78`) or supply a fake
`SynchronizationContext`, both of which are already used by the existing test suite — so no test
loses its seam.

Alternative, if some production path genuinely runs without a context: make that path explicit by
passing the dispatcher in, rather than probing `SynchronizationContext.Current` inside a static
factory.

## Why this is not fixed under epic #136

Epic #136 child F13 (issue #455) carries a hard no-behavior-change NFR. Restoring the throw changes
observable behavior on the affected paths, so it belongs in its own issue.

Note also that `ItemViewer.Breadcrumb.cs` is assigned to child F14, not F13, so two of the four call
sites are outside F13's file assignment. This reinforces that the fix belongs in a standalone issue
rather than inside either child.

## Related

- Issue #455 — F13, breadcrumb drop-down and WebView2 host coverage (where this was found).
- Issue #136 — parent epic.
- Issue #462 — breadcrumb drop-down coordinator stale `_closePending`; a second silent-failure mode
  in the same open/close path. Worth scheduling together.

## Next Step

- [ ] Promote to GitHub issue
- [ ] Confirm whether any production path legitimately runs without a synchronization context
