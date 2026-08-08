---
name: cobertura-line-double-count
description: Repo-wide Cobertura lines-valid is ~2x actual because Get-CoberturaCoverageSummary uses the `.//lines/line` descendant axis, matching both method-level and class-level <line> nodes; merged class line-rate is also biased
metadata:
  type: project
---

`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` computes the repo-wide coverage header
attributes with `$cls.SelectNodes('.//lines/line')` (line ~122 in `Get-CoberturaCoverageSummary`).
Real `dotnet-coverage` Cobertura output nests `<line>` nodes **twice** per class: once under
`class/methods/method/lines` and again in a class-level `class/lines` rollup. The `.//` descendant
axis matches both, so `lines-valid` counts every executable line about twice.

Verified 2026-08-07 against
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`:
header says `lines-valid="110849"` and a literal count of `<line number=` in the same file returns
exactly 110849.

Second, related defect: `Merge-CoberturaClassesByFilename` recomputes a merged `<class>`'s
`line-rate` through the same function on a node that contains the *primary* class's `<methods>`
plus the *unioned* class-level `<lines>`. The primary class's lines therefore carry double weight,
so the merged `line-rate` attribute is not a faithful per-file rate. Merged values are identifiable
by their exactly-6-decimal formatting (`[math]::Round(x, 6)`); unmerged values retain full double
precision.

**Why:** discovered while researching the per-file coverage harness for issue #432 (epic #136),
which required confirming the Cobertura schema from real output rather than generic knowledge.

**How to apply:** any new per-file or per-class coverage computation must select `./lines/line`
(direct children of `<class>`), or dedupe by line number taking `max(hits)`, and must never read
the `<class>` `line-rate` attribute. Do not "fix" the aggregate in a feature that is not scoped to
it — every existing gate and committed evidence baseline is calibrated to the current number. See
[[quickfiler-coverage-ledger-432]].
