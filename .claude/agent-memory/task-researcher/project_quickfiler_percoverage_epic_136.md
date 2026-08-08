---
name: quickfiler-percoverage-epic-136
description: Epic #136 per-file coverage — read committed Cobertura, but NEVER trust the <class line-rate> attribute (issue #441 double-count); recompute from the class-level <lines> union
metadata:
  type: project
---

For epic #136 (`quickfiler-per-file-coverage`, children F1–F16), per-file line coverage can be read
directly from Cobertura artifacts already committed under
`docs/features/active/<feature>/evidence/qa-gates/coverage-final.cobertura.xml` — no build or test
run needed. Grep the file for `filename="QuickFiler\Controllers\<File>.cs"` and read the
`line-rate` / `branch-rate` on the `<class>` element, then read the `<lines>` block for the exact
`hits="0"` line numbers.

**Why:** the epic mandates per-file (not per-assembly) evidence, and a child researcher who assumes
"low coverage" and plans a broad test suite will duplicate large amounts of existing test code. Two
F8 files were measured at 93.16% (`EfcHomeController.ExecuteMoves.cs`) and 97.59%
(`EfcHomeController.Metrics.cs`) — both already past the 80% target, with 8 and 1 uncovered lines
respectively. 2,502 lines of existing tests already cover this family.

**How to apply:** before proposing any tests for an epic-#136 child, locate a committed Cobertura
artifact, confirm its method line-sets align with the current file's line numbering (that is the
staleness check — the artifact comes from a sibling feature branch, not HEAD), and reconcile the
arithmetic: the tool double-counts (per-method `<lines>` + class-level `<lines>`), so
`line-rate = (total_entries - hits_zero_entries) / total_entries` across both blocks. If that
reconciles exactly, the parse is correct. Always still cite F1's harness as the authority for
acceptance evidence.

**Consequence — never report the emitted rate. Report the recomputed one.** This defect is tracked
as open issue **#441** ("Cobertura post-processing double-counts `<line>` nodes" in
`Invoke-MSTestWithCoverage.ps1` / `Get-CoberturaCoverageSummary` / `Merge-CoberturaClassesByFilename`).
The **class-level `<lines>` union is the authoritative per-file map** (a superset of the method
blocks, including async state-machine and display-class lines that appear in no `<method>`).
Confirmed on F10 files (2026-08-07): `QfcItemController.FocusAndTheme.cs` emits 0.756032 = 282/373
but is really 176/237 = 74.3% line and ~40/66 = ~61% branch; `QfcItemController.MailActions.cs`
emits 0.777778 = 147/189 but is really 96/125 = 76.8% line and 16/22 = 72.7% branch — and its
emitted branch-rate of exactly 0.75 would falsely pass the 75% floor. The epic's baseline table
quotes the inflated denominators (373 for a 326-line file, 189 for a 224-line file), so a child that
trusts it understates its own gap.

**CORRECTION (2026-08-07, F10/#453) — the error is NOT always inflation, so there is no correction
factor.** It deflates whenever a method-block entry is uncovered but the class-level union masks it
via max-hits. Proven on `QfcItemController.Initialization.cs`: true union 123/134 = 91.79%, emitted
`line-rate` 0.901099 = 246/273 — **lower** than the truth, because `[ExcludeFromCodeCoverage]`
methods still emit uncovered `<InitializeAsync>b__115_0`-style closure methods whose zero-hit
entries the union masks. (Two related facts: the attribute does **not** propagate to lambdas
declared inside the method, so those closure lines are already in the denominator and already
uncovered; and interface-only files emit no `<class>` element and must be reported N/A, never 0%.)
Issue #441's title says "inflating" only — that refinement should be added as a comment there.

Related: [[qfc-item-controller-230-pump-seam-blocks-exemption-removal]],
[[feedback-exemption-audit-check-proven-techniques]], [[qfc-item-controller-227-r2-denial]].
