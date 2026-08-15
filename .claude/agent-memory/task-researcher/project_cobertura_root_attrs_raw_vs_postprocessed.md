---
name: cobertura-root-attrs-raw-vs-postprocessed
description: A repo Cobertura report's root lines-valid means two different things depending on whether it was post-processed by ConvertTo-KoverageCoberturaXml; never compare the two figures (issues #441/#478)
metadata:
  type: project
---

A Cobertura file in this repo carries root `lines-valid`/`lines-covered`/`branches-*` computed by one
of two incompatible formulas. Tell them apart before comparing any two coverage numbers:

- **RAW** `dotnet-coverage` output — absolute `filename` attributes, no `<sources>` element. Root
  totals are the **class-level `<lines>` rollup only** (proven on the #424 baseline: class-level
  `<line>` count = 79957 = `lines-valid` exactly; minus 23833 uncovered = 56124 = `lines-covered`
  exactly; raw all-descendant count is 161086). Branches likewise class-level-only.
- **POST-PROCESSED** by `ConvertTo-KoverageCoberturaXml` — relative filenames, `<sources><source>.`
  present. `Get-CoberturaCoverageSummary` **overwrites** the correct root totals with a
  `.//lines/line` descendant-axis sum, counting every line twice (class rollup + method copy). On the
  #424 final report: `lines-valid=110849` = the raw `<line number=` count; class-level-only would be
  62345 / 53013 = 0.850317 vs the emitted 0.856453.

**Why:** issues #441 and #478. Cobertura repeats each line under `<class><lines>` and again under
`<class><methods><method><lines>`. The merge function's own union at `Helpers.ps1:219` is already the
correct child axis; the defect enters only at `Helpers.ps1:122` and reaches merged classes indirectly
through the `$classSummaryXml` delegation at `Helpers.ps1:270-273`.

**How to apply:**
- Never compare a raw figure against a post-processed figure. `coverage-delta.2026-08-07T00-48.md:65`
  blamed a +38.6% denominator jump on "dotnet-coverage denominator instability"; it was actually
  raw-vs-post-processed formula mismatch.
- The strongest correctness oracle for any fix to this arithmetic: run the corrected
  `Get-CoberturaCoverageSummary` over a RAW report and require it to reproduce that document's own
  root attributes exactly.
- `.claude/agent-memory/atomic-executor/project_coverage_delta_reproduce_baseline_counting_method.md:34-36`
  states root attributes "are already deduped ... need no adjustment". True for raw only; false for
  any post-processed artifact. Correct it once #441 lands.
- Branch counts are inflated by the identical mechanism, but the *ratio* can be unchanged while the
  counts are wrong (verified on `QfcHomeController.Iteration.cs`: 8/12 and 12/18 both = 0.666667).
  Assert on `branches-valid`/`branches-covered`, never on `branch-rate` alone.
