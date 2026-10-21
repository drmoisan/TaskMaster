# [P6-T9] AC7 Residual

Timestamp: 2026-09-08T10-17

RESIDUAL-IN-SCOPE: NO

## The residual, stated explicitly

AC7 covers the derivation only. The registration hop at `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:216`, which is `FindForm() as QfcFormViewer`, remains untestable without a real form hierarchy and is not in scope for this issue. Nothing in this plan changed that line, and no test added here exercises it.

What the extraction made measurable is the store and the disjunction over it: `BreadcrumbPopupOwnerRegistry.Register` and `BreadcrumbPopupOwnerRegistry.AnyOpen`, now covered by the six cases [P6-T8] ran green.

What remains unmeasurable is the wiring on either side of the registry:

- the registration hop named above, which needs a live form hierarchy to resolve `FindForm()` to a `QfcFormViewer`;
- the two forwarding members left on `QuickFiler/Viewers/QfcFormViewer.cs`, `SetBreadcrumbPopupOwner` and `IsDeactivationSelfInflictedByOwnPopup`. That class keeps its class-level coverage exemption attribute, so it still emits no Cobertura class element at all and its lines cannot be reported as covered or uncovered in either direction. [P7-T9] records its changed lines as `NOT MEASURABLE` with that reason rather than as zero.

## Why this is recorded rather than fixed

Making the registration hop testable would require a seam over `FindForm()`, which is a change to the item-viewer wiring rather than to the derivation, and D16 lists the `ItemViewer.Breadcrumb.cs` predicate wiring among the prohibited changes for this issue. The residual is therefore recorded here so it is visible at review rather than absorbed silently, and it is not acted on.
