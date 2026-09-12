---
name: cobertura-perfile-attribution-contract
description: How to read per-file line/branch coverage out of this repo's Cobertura reports — per-file attribution works for partial classes, but the line-rate attribute is inflated and must be recomputed
metadata:
  type: project
---

Cobertura in this repo emits **one `<class>` element per (type, source file) pair**, so per-file
attribution for a partial-class family works: `QfcItemController` (10 partials) yields 10 `<class>`
elements with 10 distinct `filename` values, all sharing `name="QuickFiler.Controllers.QfcItemController"`.
A single method whose lines span two partials appears under both class elements.

**But the `line-rate`/`branch-rate` ATTRIBUTE is not trustworthy.** Two compounding defects in
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`:
1. `Get-CoberturaCoverageSummary` selects `.//lines/line`, which matches both the
   `<methods><method><lines>` subtree and the class-level `<lines>` block — every line counted twice.
   Root `lines-valid` equals the raw `<line number=` element count exactly.
2. `Merge-CoberturaClassesByFilename` unions class-level `<lines>` across a filename group (correctly,
   max hits) but never merges the other members' `<methods>`, then recomputes the rate over the mixed
   node set. For any file with an async method or lambda (which gets a companion state-machine /
   `<>c` class) the emitted rate is a blend that **overstates** the true per-file rate.

**Correct recipe:** union `./lines/line` (class-level) children only, across all `<class>` elements
with that `filename`, keyed on `@number` with MAX `@hits`. line rate = hits>0 / total. branch rate =
sum of `(covered/total)` parsed from `@condition-coverage` on `@branch="True"` lines. Report N/A, not
0%, when the denominator is zero, and treat "filename absent from report" as a third state.

**Why:** issue #441 documents defect 1; defect 2 was proven arithmetically against the committed
report `docs/features/active/2026-08-06-.../evidence/qa-gates/coverage-final.cobertura.xml`
(`QfcHomeController.Iteration.cs`: attribute 0.8625 = 69/80 blended, true 45/56 = 80.36%). #424's own
delta evidence independently arrived at the same class-level-only recipe.

**How to apply:** any coverage-gate research or plan in this repo — especially epic
`quickfiler-per-file-coverage` (#136) — must recompute rather than read the attribute. Corollary:
epic.md's "Measured Coverage Baseline" table is quantitatively inflated (Lines column ~2x, Line %
overstated for async-heavy files), and its 70.19% repository baseline is a RAW unprocessed figure not
comparable to any post-processed report. See [[quickfiler-percoverage-epic-136]] and
[[quickfiler-interface-only-files-433]].
