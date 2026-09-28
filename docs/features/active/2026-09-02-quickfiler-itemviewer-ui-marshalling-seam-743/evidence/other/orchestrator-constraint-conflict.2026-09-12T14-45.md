# Orchestrator Note: Constraint Conflict Requiring Explicit Resolution (Issue #743)

Timestamp: 2026-09-12T14-45
Collected by: orchestrator (preparation mode)
Method: Read of the cited #729 research artifact and of the inherited constraint set
EXIT_CODE: 0

## The conflict

`CLAUDE.md` requires that conflicting instructions be surfaced rather than silently interpreted. This is
one, and it sits at the centre of the item, so the spec must resolve it explicitly rather than leave it
to the executor.

**Source A — the recommendation this item inherits.**
`docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/research/research-729.2026-09-02T09-30.md`
section 4.3, "Definitive recommendation — conclusion (b)", states:

> The only durable fix is a `QuickFiler/` production seam: give the members under test an injectable UI-marshalling
> abstraction (an `IUiDispatcher`/`SynchronizationContext` parameter or settable seam on `QfcItemController`, and an
> interface over the `WebView2` control accepted by `WebView2BreadcrumbHost`'s constructor) **so a synchronous fake can
> replace the message loop entirely.**

**Source B — the constraint that remains in force.**
The delegation prompt and the #592 constraint note both state that the determinism epic's prohibitions on
a timing tolerance and on a fake `SynchronizationContext` replacing the real pump REMAIN in force, and
that only the no-production-edits prohibition is re-opened for this item.

The final clause of Source A is precisely what Source B forbids.

## Why the constraint exists

The #511 and #571 closing comments record the reason. #511 proposed replacing the real message pump with
an injectable context; executed literally, that deletes the very tests #571 exists to stabilize. The
constraint is not arbitrary caution. It prevents the item from "fixing" the flake by removing the test
coverage that detects the underlying behavior, which would satisfy a green suite while losing the
guarantee.

## Resolution

The constraint set is explicit about which prohibition is lifted, so this resolves determinately rather
than requiring a maintainer decision:

1. Production edits ARE permitted. The injectable seam may therefore be added to production code, which
   is the substance of Source A's recommendation and the substance of this issue.
2. The seam MUST NOT be used to replace the real pump in the existing pump-hosted tests. Those tests stay
   pump-hosted and keep exercising the real message loop.
3. The seam's legitimate purpose is to let members that do NOT require a real Win32 message loop be
   tested without constructing the full pump fixture, which shortens the expensive path without deleting
   the guarantee.

So Source A's seam is adopted and Source A's final clause, "replace the message loop entirely", is
rejected. The word "entirely" is what fails; a partial, additive seam is permitted and is the intended
remedy.

## What the spec must therefore state, explicitly and per test

The spec cannot leave this implicit. For every test touched it must say which of two categories it falls
into:

- **RETAINED pump-hosted.** Keeps `WinFormsPumpHost` and the real message loop. Its coverage contribution
  is the thing the constraint protects.
- **MOVED to the seam.** Demonstrably does not depend on real Win32 handle creation or
  `Control.BeginInvoke` marshalling, and therefore loses nothing by not running on the pump.

A test may only be placed in the second category with a stated justification. Acceptance criterion 4,
retained-or-improved coverage of `QfcItemController.Initialization.cs` and `QfcItemController.ViewerSetup.cs`,
is the numeric check on that judgment, subject to the coverage-visibility caveat recorded in
`orchestrator-citation-verification.2026-09-12T13-50.md`.

## A second, smaller conflict in the scaffolded spec

The template `spec.md` that `new_active_feature_folder` generated carries the promoted record's
"Expected Behavior" prose, which reads:

> ... either by scaling the harness bound to the environment or by allowing a synchronous fake to replace the real
> message pump for the members under test.

Both of those alternatives are forbidden. Scaling the harness bound is a timing tolerance, and the second
clause is the Source A problem again. That sentence must be rewritten when `spec.md` is authored, not
carried forward.

## Output Summary

The cited #729 recommendation ends in a clause the inherited constraint forbids. The conflict resolves
determinately: adopt the injectable seam, reject replacing the message loop entirely, keep the existing
pump-hosted tests on the real pump, and require the spec to classify every touched test as retained
pump-hosted or moved to the seam with justification. The scaffolded `spec.md` additionally carries two
forbidden remedies in its Expected Behavior prose and must be rewritten rather than extended.
