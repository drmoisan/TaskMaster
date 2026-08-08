---
name: cobertura-exemption-and-branchrate-gotchas
description: Two Cobertura/coverlet measurement gotchas found during epic #136 F10 (issue #453) — method-level [ExcludeFromCodeCoverage] does not exempt lambdas, and class branch-rate double-counts conditions
metadata:
  type: project
---

Two measurement facts about this repo's Cobertura output (coverlet via
`Invoke-MSTestWithCoverage.ps1`), established 2026-08-07 while researching epic #136 child F10.

1. **A method-level `[ExcludeFromCodeCoverage]` does NOT propagate to the method's
   compiler-generated lambda closures.** In
   `QuickFiler/Controllers/QfcItemController.Navigation.cs`, `ToggleExpansionAsync(ToggleState)`
   carries the attribute at line 191, yet its two lambda bodies (lines 197, 202) appear as their own
   `<method line-rate="0">` entries AND in the class-level `<lines>` block with `hits="0"`.
   Consequence: **every exempt method containing a lambda silently contributes permanently
   uncovered lines to its file's denominator.**

2. **The `<class branch-rate>` attribute double-counts conditions**, summing the per-method
   `<lines>` blocks and the class-level `<lines>` block. A branch point inside a *named* method
   therefore counts twice; a branch inside a lambda-only closure counts once. Verified exactly on
   three files: EventWiring 21/32 = 0.65625, EventHandlers 26/40 = 0.65, Navigation 23/30 =
   0.766667. **Practical effect: covering one `if` inside a named method raises the numerator by 2,
   not 1** — a branch projection built on the class-level block alone will be wrong.

   `line-rate` behaves differently and needs care. For `QfcItemController.EventHandlers.cs` the
   attribute was `0.7956989247311828`, which is exactly 74/93 — the **class-level block alone**,
   not the double-counted 149/187 = 0.7968. A sibling memory ([[quickfiler-percoverage-epic-136]])
   records line-rate as double-counted; that disagreement is unresolved, and files whose attribute
   is rounded to 5 dp are not diagnostic either way. Reconcile the arithmetic **both** ways on the
   specific file before trusting any projection.

   Added 2026-08-07 from the F10 Initialization/ViewerSetup pass — a usable discriminator: **files
   with no exempt member agree exactly** (`QfcItemController.cs`: attribute `line-rate="1"` and
   `branch-rate="0.785714…"` = exactly 11/14 recomputed), while the two files containing exempt
   members diverge in *opposite* directions (Initialization attr 0.901099 vs recomputed 123/134 =
   91.8%; ViewerSetup attr 0.743682 vs recomputed 116/160 = 72.5%). Because the sign is not
   consistent, the attribute is not even a safe bound on an exempt-bearing file. The class-level
   `<lines>` block is demonstrably a **max-hits union**: `<SaveParameters>b__118_0` reports source
   lines 382-388 at `hits="0"` in its own `<method>` entry and `hits="1"` in the class block.

**Why:** a child that assumes an exemption removes a whole method from measurement, or that computes
its own branch target from the class-level block only, will produce a number that disagrees with
F1's harness and fail its gate for reasons it cannot see.

**How to apply:** when reading a committed Cobertura artifact for any epic-#136 child, reconcile the
arithmetic both ways before trusting a projection, and check whether an exempt method contains
lambdas. Report finding 1 to F1 as a ledger/harness note. Always still cite F1's harness run on the
child's own branch as the acceptance authority.

Related: [[quickfiler-percoverage-epic-136]],
[[feedback-exemption-audit-check-proven-techniques]], [[qfc-item-controller-227-r2-denial]].
