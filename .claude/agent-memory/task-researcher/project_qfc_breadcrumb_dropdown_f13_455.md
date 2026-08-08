---
name: qfc-breadcrumb-dropdown-f13-455
description: Issue #455 / epic #136 F13 — the three breadcrumb drop-down files already PASS the 75% branch floor (91-92%); Cobertura class `name` is the wrong key; async rethrow creates an unreachable line
metadata:
  type: project
---

Epic #136 child F13 (issue #455) research, 2026-08-07. Three files researched:
`QuickFiler/Viewers/BreadcrumbDropDownHost.cs` (480), `BreadcrumbDropDownOpenLifetime.cs` (477),
`BreadcrumbDropDownOpenCoordinator.cs` (309).

**The branch-coverage premise handed down by the epic and the delegation prompt was wrong for these
files.** Measured from the committed #424 Cobertura
(`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`):
Host 99.42% line / 91.49% branch; OpenLifetime 99.13% / 91.86%; Coordinator 98.25% / 92.05%. All
three pass both the 80% line and 75% branch gates already. Total uncovered lines across all three:
nine. The work is outcome pinning, not gap closure.

**Why:** the epic generalised from F8's `EfcHomeController.Timing.cs` (100% line / 66.67% branch) and
from the assumption that F13's files are `[ExcludeFromCodeCoverage]`. Only the WebView2 trio
(`WebView2BreadcrumbHost/CoreInitializer/Messenger`) carries the attribute; these three do not.

**How to apply:** before planning coverage work on any QuickFiler file, read its `line-rate` AND
`branch-rate` out of a committed Cobertura report first. Do not budget test-authoring effort against
an assumed shortfall.

Three durable techniques discovered here, reusable on any QuickFiler coverage child:

1. **Key the per-file harness on `filename`, never on `<class name>`.** `BreadcrumbDropDownOpenLifetime.cs`
   reports its class element as `QuickFiler.Viewers.BreadcrumbDropDownOpenLease` — the 11-line struct
   declared first in the file — while carrying all 343 lines of the 453-line class. A name-keyed
   harness reports the file as absent. Also: async state machines (`OpenCoreAsync`, `RollbackAsync`)
   get no `<method>` entry at all, so a `<methods>`-based harness under-counts.
2. **An unconditional `throw;` at the end of a `catch` inside an `async` method makes the catch's
   closing brace a permanently-uncovered line.** `BreadcrumbDropDownOpenLifetime.cs:358-359` is the
   example. 99.13% is that file's reachable ceiling. Record as irreducible; do not chase.
3. **Defensive guards duplicated inside a posted lambda are usually unreachable.** Where a guard is
   checked before `PostAsync` and again inside the posted body, the inner copy's true-side is often
   unreachable because the invalidation path that would set it also cancels the post. Check the
   scheduling primitive before writing a test for the inner copy.

Deterministic-test vehicles that already exist and work (no clock/timer seam is needed anywhere in
these files — there is no wall-clock read, timer, or delay in any of the three):
`BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext` (queued, `DrainOne`/
`DrainAll`/`PendingCount`/`PostCount`) and `BreadcrumbDropDownLifecycleConcurrencyTests.InlineSynchronizationContext`
(synchronous re-entrancy). See [[quickfiler-percoverage-epic-136]].

Latent defects recorded for promotion: `_disposed = disposing` (not `|=`) in
`BreadcrumbDropDownOpenLifetime.ScheduleInvalidating`; `_closePending` never cleared on the
successful-close path in `BreadcrumbDropDownOpenCoordinator.CloseCore`, which can silently drop a
reopen request; `_host.IsOpen` read while holding `_sync` in `RequestOpen`; `_resetPending` can stick
`true` in `BreadcrumbDropDownHost.Reset`.
