---
name: issue-656-bypass-path-does-not-exist
description: "#656 _closeCompleted residual: the bypassing reopen path the issue assumes does NOT exist; only open transition is BreadcrumbDropDownOpenLifetime.cs:268, reachable only via RequestOpen; the two nominated owner files are at the 500-line cap"
metadata:
  type: project
---

Issue #656 (`_closeCompleted` stays stale after a host reopen that bypasses `RequestOpen`/`Invalidate`)
has a **false premise on shipped code**. Verified 2026-08-31 by exhaustive enumeration:

- The repository's ONLY statement that makes the breadcrumb drop-down host open is
  `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs:268` (`_host.OpenState = true;`).
  `BreadcrumbDropDownHost.IsOpen => OpenState` is get-only; all four other `OpenState` writes are `false`.
- Closed chain: `RequestOpen (…OpenCoordinator.cs:115, clears the flag at :114) → OpenCoreAsync (:218)
  → BeginOpenCore (:258/:259) → BreadcrumbDropDownHost.Open.cs:22/:37 → :88 →
  BreadcrumbDropDownOpenLifetime.cs:67-69 → :243 → :268`. No production caller of
  `IBreadcrumbDropDownHost.OpenAsync` exists outside `BeginOpenCore`.
- Native-show family also checked: `ShowPopup` / `_showPopup` / `ShowOwnedPopup` are invoked once each,
  downstream of `OpenState = true`; the only `ToolStripDropDown` event subscribed is `Closed`.

**Why:** the issue's "Suspected Cause" asserts the bypassing paths "live in the ItemViewer breadcrumb
lifecycle host surface" (feature 488's files). They do not. That claim came from #501's SR-4 known
limitation being recorded as an ownership hand-off, not from an enumeration.

**How to apply:** treat #656 as latent-correctness hardening, not a user-facing defect. Do NOT site a
fix in `BreadcrumbItemViewerLifecycleCoordinator.cs` (497 lines) or `BreadcrumbDropDownHost.cs`
(498 lines) — both are 2-3 lines from the repo's 500-line cap, so any edit forces a partial split first.
The only sane footprint is `BreadcrumbDropDownOpenCoordinator.cs` (378 lines) plus
`QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs` (173 lines; Part2 is 455 and
the primary partial is 463, both too full for a new test).

Two facts that constrain any remedy:

1. SR-4 (`docs/features/active/breadcrumb-coordinator-hub-defects-501/spec.md:426-437`) rejected
   `if (_closeCompleted && !_host.IsOpen) return true;` because it reads `_host.IsOpen` under `_sync`.
   **Honest qualification:** `RequestOpen` at `…OpenCoordinator.cs:112` ALREADY reads `_host.IsOpen`
   under `_sync`, so "never read IsOpen under the lock" is not an invariant the file holds. SR-4's real
   objection is adding a *second* instance of a pattern a sibling feature was removing. Don't overstate it.
2. The refinement would be a **no-op in production anyway**: `BreadcrumbUiDispatcher.Dispatch`/`DispatchValue`
   run INLINE on the captured boundary (`BreadcrumbUiDispatcher.cs:78-95`, `:166-178`), so
   `CompleteClose` → `OpenState = false` executes synchronously inside `_host.Close` before it returns
   `true`. `!_host.IsOpen` is therefore already true when the suppression is evaluated.

Bypass test seam already exists and needs no new plumbing: `ControlledHost.SetOpen(bool)` at
`QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs:407`, already used for exactly this
purpose at `…Part2.cs:349`. No test anywhere references `_closeCompleted` by name or reflection.

Related: [[breadcrumb-navigation-defects-439-440-498-499]], [[issue-469-already-fixed-residual-is-629]].
