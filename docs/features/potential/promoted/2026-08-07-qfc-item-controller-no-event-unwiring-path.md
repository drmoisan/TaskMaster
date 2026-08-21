# qfc-item-controller-no-event-unwiring-path (Issue #481)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-item-controller-no-event-unwiring-path/ (Issue #481)
- Discovered during: preparation research for epic #136 child F10 (issue #453)

- Issue: #481
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/481
- Last Updated: 2026-08-08
## Summary

`QfcItemController` subscribes to roughly 22 events across its wiring partials and never detaches
any of them. `Cleanup()` nulls collaborator fields but performs no unsubscription, so handlers
remain reachable from live event sources after the controller is logically torn down.

## Affected Code

- `QuickFiler/Controllers/QfcItemController.EventWiring.cs` — 25 `+=` subscription operators.
  `WireIntentEvents()` alone makes 16 subscriptions; `WireControlTreeEvents()` adds further
  per-control, per-button, and per-menu-item subscriptions.
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:392-421` — `Cleanup()` nulls 17
  collaborator fields and detaches nothing.

Across all ten `QfcItemController.*.cs` partials there are exactly three `-=` operators
(`ViewerSetup.cs:152`, `:155`, `:399`), all for the single unrelated `BreadcrumbUnhandledArrow`
event. Every other subscription is permanent for the lifetime of the event source.

## Why This Is a Defect

QuickFiler pools and reuses item viewers. A subscription that outlives its controller keeps the
controller object graph reachable from the viewer's event source, which prevents collection and — more
importantly — allows a stale controller to receive and act on events after `Cleanup()`. At least one
observed consequence is a swallowed `NullReferenceException` when a post-cleanup
`WebViewInitializationCompleted` handler dereferences a field that `Cleanup()` has already nulled.

This is the same defect class recorded for `EmailMoveMonitor` in issue #426 (a leaked
`BeforeItemMove` subscription), here at substantially larger scale.

## Reproduction Sketch

Wire a controller against a viewer, call `Cleanup()`, then raise any wired event on the viewer.
The handler still executes against a controller with nulled fields.

## Suspected Fix

Introduce a symmetric unwiring path — an `UnwireIntentEvents()` / `UnwireControlTreeEvents()` pair
mirroring the wiring methods — and invoke it from `Cleanup()` before the fields are nulled.
Detachment must use the same delegate identity used at subscription time, which for lambda-based
subscriptions requires capturing the delegate in a field first.

## Severity

Medium. No data loss. Causes handler execution against torn-down state, swallowed exceptions, and
retention of the controller graph across pooled-viewer reuse.

## Related

- #426 — `EmailMoveMonitor` rejected-item hook retention (same defect class, different owner).
- #458 — `WebView2BreadcrumbHost` handler retention with pooled viewer.

## Scope

Out of scope for epic #136 child F10, whose NFR prohibits behavior change to observable QuickFiler
flows. Adding an unwiring path changes teardown semantics and must be scheduled and tested on its
own.
