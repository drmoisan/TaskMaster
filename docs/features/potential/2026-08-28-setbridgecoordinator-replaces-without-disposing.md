# setbridgecoordinator-replaces-without-disposing (Potential Bug)

- Date captured: 2026-08-28
- Author: Dan Moisan
- Status: Draft

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

## Summary

`BreadcrumbItemViewerLifecycleCoordinator.SetBridgeCoordinator` replaces its bridge coordinator without
disposing the outgoing instance, while the same type's `Dispose()` **does** dispose it. The type
therefore owns the bridge coordinator at teardown but not at replacement. The path is dormant today
only because of the method's reference-equality guard, and because #488's D3 fix was deliberately
implemented as **fail-fast** rather than as explicit re-initialization.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (.NET Framework 4.8.1, VSTO / WinForms)
- Command/flags used: n/a — identified by source reading during the #488 orchestrator comment cross-check
- Data source or fixture: `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`

## Steps to Reproduce

1. Obtain a coordinator whose `_bridgeCoordinator` is already set.
2. Call `SetBridgeCoordinator` with a genuinely different instance, so the reference-equality guard does
   not short-circuit.
3. Observe that `UnsubscribeBridge()` runs and the field is overwritten, but the outgoing coordinator is
   never disposed.

## Expected Behavior

Replacement and teardown should agree on ownership. If `Dispose()` disposes the bridge coordinator, so
should the replacement path — or neither should, and ownership should sit with the caller.

## Actual Behavior

`SetBridgeCoordinator` calls `UnsubscribeBridge()`, which only detaches four event handlers and disposes
nothing, then assigns the new instance. `Dispose()` by contrast calls `_bridgeCoordinator?.Dispose()`
and then nulls the field. On replacement the outgoing coordinator's `BreadcrumbMessengerHub` and its
four event subscriptions leak.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: no captured log; verified against source in
  `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/research/2026-08-25T10-20-orchestrator-comment-crosscheck.md`
  under "Claim 2".

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Unreachable in the delivered code, so no user is affected today. The severity is latent: it becomes live
the moment any change lets a second, different bridge coordinator be installed.

## Suspected Cause / Notes

**The dormancy is contingent and must not be assumed permanent.** #488's D3 fix makes
`ItemViewer.InitializeBreadcrumbPipeline` throw `InvalidOperationException` on a second, different
`IFolderHierarchyProvider` rather than re-initializing. Under fail-fast, `InitializeBreadcrumbPipeline`
never constructs a second `BreadcrumbBridgeCoordinator`, so nothing new ever reaches
`SetBridgeCoordinator`'s replacement branch.

**If D3 were ever amended to adopt explicit re-initialization instead, this defect becomes live and MUST
be pulled into scope in the same change-set.** #488's `spec.md` records that coupling under its D3
design section, in a paragraph headed "Load-bearing coupling — do not lose this", and #488's constraint
C7 forbids substituting a re-initialization branch for the throw.

Whoever picks this up should decide the ownership question outright rather than only patching the
replacement branch: either the coordinator owns the bridge coordinator on both paths, or on neither.

Files to inspect: `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`,
`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: a test installing a genuinely different bridge coordinator and asserting the outgoing one is disposed exactly once
- [ ] Integration scenario to retest: `SetBridgeCoordinator_SameReference_DoesNotDuplicateSubscriptions`, which must stay green
- [ ] Manual verification notes: confirm no double-dispose when replacement is followed by `Dispose()`

## Next Step

Promote to a GitHub issue against `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`,
cross-referencing #488's D3 fail-fast decision as the reason it is currently dormant.
