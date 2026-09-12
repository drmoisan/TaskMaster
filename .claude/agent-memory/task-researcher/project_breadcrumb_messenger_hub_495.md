---
name: breadcrumb-messenger-hub-495
description: "#495/epic #136 F12 BreadcrumbMessengerHub.cs: a Component finalizer can make a branch outcome GC-dependent (fake coverage); one .cs with 3 types emits ONE Cobertura class element; brief's Lines column is coverable-not-physical"
metadata:
  type: project
---

Findings from researching `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` (F12 / issue #495) on
2026-08-08. The baseline (294 coverable lines, 100.00% line, 114/118 = 96.61% branch) was confirmed
exactly — the first F12 file whose brief table survived re-measurement unchanged.

**Why:** three of these are transferable measurement traps that will recur in other #136 children,
and one is a scoping trap that would silently mis-size a plan.

**How to apply:**

1. **A `System.ComponentModel.Component` subclass can have its `Dispose(bool)` `false` arm "covered"
   by the GC finalizer, not by any test.** `Component.Dispose()` passes `true` + SuppressFinalize;
   only `~Component()` passes `false`. If tests construct the owner and never dispose it, finalization
   during the run marks the arm covered — a branch outcome that can vanish on the next run with no
   diff to explain it. Treat any covered `if (disposing)` false-arm as *presumed non-deterministic*
   and pin it with a reflection call on the `protected Dispose(bool)`. Same logic applies to any
   finalizer-reachable code.

2. **One `.cs` file declaring several top-level types emits exactly ONE Cobertura `<class>` element**
   in this repo's merged reports: the class-level `<lines>` block is the union across all types
   (correct), while the `<methods>` subtree is primary-only (issue #478). Sibling types get **no**
   `<class>` element of their own — a harness keyed on `<class name>` silently drops them. Keying on
   `filename` (the epic directive) is what makes the numbers right. `BreadcrumbMessengerHub.cs` is a
   good positive control: 3 top-level types, 1 class element, 294 union lines.

3. **The F12 brief's `Lines` column is coverable lines, not physical lines, and is unlabelled.**
   294 coverable vs 456 physical for this file. A planner computing 500-minus-that gets 206 lines of
   headroom instead of the true 44. Same mislabelling on all five F12 rows.

4. **Coverage concentration is per-type, not per-file.** 42% of this file's coverable lines belong to
   two `internal` types (`BreadcrumbCollapsedAttachment`, `BreadcrumbResourceOwner`) that no
   hub-named test targets; `BreadcrumbResourceOwner`'s 13 lines are covered *only* as a side effect
   of live `ItemViewer` construction in F13/F14-owned test files. For any file at 100% line, split
   the line count by declared type before writing the retain-or-improve section — that is where the
   real risk lives.

5. The "injected clock and fake timers" instruction is refuted here for the third consecutive
   breadcrumb file (zero `DateTime`/`Timer`/`Task.Delay`/`TimeProvider` tokens). See
   [[qfc-breadcrumb-dropdown-f13-455]]. For this file determinism is *completion-source* control,
   not even scheduler control — no `SynchronizationContext` is needed at all.

Verdict recorded: zero production edits; all 4 residual outcomes closable from `QuickFiler.Test`
through the existing `InternalsVisibleTo` grant; 3 latent defects (nested-lock SDK call, broadcast
aborts mid-way while the message stays cached as delivered, naive `MessageType` string scan).
