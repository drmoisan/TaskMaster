---
name: quickfiler-perfile-coverage-baseline
description: Where to find indicative per-file Cobertura line/branch rates when scoping an epic #136 (quickfiler-per-file-coverage) child, and why the number changes the child's framing
metadata:
  type: reference
---

When scoping any child of epic #136 `quickfiler-per-file-coverage`, look for an already-committed
Cobertura report before assuming a file is uncovered. As of 2026-08-07 the usable one is
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
— it carries `<class line-rate= branch-rate= filename="QuickFiler\Controllers\...">` entries for the
whole assembly. Grep it by `filename=` to get per-file rates in one pass.

**Why it matters:** the F8 (`EfcHomeController`, issue #437) seed assumed the six files were
uncovered; in fact all six measured 93-100% line coverage. That inverted the child's framing from
"reach 80%" to "retain 80% + close named gaps + fix a coverage-reproducibility hazard", and made
`Timing.cs`'s 66.67% *branch* rate (below the 75% floor) the only genuinely unmet threshold. Expect
the same pattern on sibling children.

**How to apply:** cite these figures as INDICATIVE only — they were captured on another feature's
branch. The epic's acceptance authority is F1's per-file harness re-run on the child's own branch,
committed under `<FEATURE>/evidence/qa-gates/`. Say so explicitly in the spec.
Related: [[ac-gates-verify-satisfiability]].
