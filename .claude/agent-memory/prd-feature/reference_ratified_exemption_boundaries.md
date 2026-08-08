---
name: ratified-exemption-boundaries
description: Before scoping any [ExcludeFromCodeCoverage] removal, check docs/features/archive/ for a maintainer-decision artifact that already ratified that boundary
metadata:
  type: reference
---

Some QuickFiler/UtilitiesCS types have a **maintainer-ratified** `[ExcludeFromCodeCoverage]`
boundary recorded under `docs/features/archive/<feature>/maintainer-decision.<date>.md` plus a
sibling `evidence/other/exemption-boundary.<timestamp>.md`. A feature child has no authority to
overturn one; it re-verifies each member against current source and removes only where the ratified
rationale has demonstrably lapsed.

Known instance (verify it still exists before citing):
`docs/features/archive/2026-06-29-qfc-item-controller-testability-227/` — issue #227, ratified
2026-07-02, 19 members for the `QfcItemController` partial family, reached after five cycles that
went 103 -> 41 -> 24 -> 19 with the maintainer denying each intermediate count. Composition: 9
blocked by the unbuilt WinForms message-pump seam (open issue **#230**, explicitly NOT a merge
condition), 6 `async void` shells with tested `*Core` bodies, 3 deliberate `virtual` test seams, 1
external-runtime dependency.

**How to apply:** when an epic acceptance criterion says "exemption count falls to zero", check for
a ratification artifact first. If one exists, the correct spec move is to *reconcile* the AC — the
coverage ledger RECORDS the ratification as the governing authority — not to plan removals. Frame
the exemption criterion around re-verification + resolving unratified drift + deleting dead members
that carry attributes, and never promise N -> 0. Related: [[quickfiler-perfile-coverage-baseline]],
[[ac-gates-verify-satisfiability]].
