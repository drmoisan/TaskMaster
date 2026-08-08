---
name: qfc455-reentrant-dispose-seam
description: F13/#455 — an already-existing disposal-callback reentrancy point can open an otherwise-unreachable async window, closing branches without adding a production seam
metadata:
  type: project
---

When a class rejects work by *completing a cancellation task* rather than by setting a flag, the
"stale generation observed after the await" re-check is normally unreachable from a test, because the
same invalidation that makes the state stale is also what releases the `await Task.WhenAny(work,
cancellation)` — so the method always exits through the cancellation arm and never evaluates the
re-check. Measured instance: `QuickFiler/Viewers/BreadcrumbCollapsedSurfaceController.cs:197` and the
four `&&` jumps at `:243-247` (6 uncovered branch outcomes, the lowest branch figure in F13).

**Why:** the obvious fix is to inject a `Func<Task,Task,Task<Task>>` "when-any" seam so a fake can
mutate state inside the window. That is a purely test-motivated production API, and on this type it
would add a constructor where none exists — breaking F12's `BreadcrumbMessengerHub.cs:290` and F14's
`ItemViewer.Breadcrumb.cs:266` at compile time. Before adding such a seam, look for an **existing**
point where the class already calls caller-supplied code outside its lock. Here it was
`SafeDispose(replacedReadyMessenger as IDisposable)` at `:163` — it runs after the lock and before the
attachment task starts, so a test fake whose `Dispose()` re-enters the controller opens the window
with zero production change. Four of the six outcomes close that way; the other two are provably dead
(the generation counter and its cancellation source are written together under one lock, so the
operands guarding them can never disagree).

**How to apply:** when a residual branch looks like it needs a scheduling seam, first grep the class
for outward calls made outside the lock — disposal callbacks, error sinks, injected `Action`s,
event raises. Those are free reentrancy points. State the residual determinism assumption explicitly
(here: `Task.WhenAny` resolving to argument index 0 when both arguments are already complete) and
have the test assert only outcomes that hold on either path, then confirm the branch actually moved
by re-measuring — never let the assertion depend on the tie-break.

Related: [[quickfiler-percoverage-epic-136]], [[feedback-exemption-audit-check-proven-techniques]].
