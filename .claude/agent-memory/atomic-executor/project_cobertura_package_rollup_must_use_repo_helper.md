---
name: cobertura-package-rollup-must-use-repo-helper
description: Koverage root lines-covered/lines-valid come from a per-class dedup+max-hits merge of BOTH the class-direct and method-level line views, so a hand-written ./classes/class/lines/line node count cannot be asserted equal to them - call Get-CoberturaPackageLineSummary instead
metadata:
  type: project
---

A plan that derives per-package coverage counters by hand and then asserts they equal the root
`<coverage>` attributes of a Koverage-processed Cobertura document states an equality that is not
guaranteed by either available node axis.

**Mechanism.** `ConvertTo-KoverageCoberturaXml` sets the root attributes from
`Get-CoberturaCoverageSummary` (`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`), which sums
`Get-CoberturaPackageLineSummary` (`scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1`) over
`./package`, which sums `Get-CoberturaClassLineSummary` over `.//class`. That last helper builds a
map keyed by line **number** from the concatenation of `./lines/line` and
`./methods/method/lines/line`, keeps the **maximum** `hits` seen across the two views, and takes the
widest `condition-coverage` pair. Branches are counted only for lines carrying `branch="True"`.

Consequences for an assertion author:

- `./classes/class/lines/line` (class-direct only) misses any line that appears solely in the
  method view, and reads a `hits` of 0 where the method view recorded a hit — common around async
  state machines, which can emit method-level lines with no class-direct twin.
- `.//lines/line` double-counts, because the two views overlap.
- Counting a line as a branch whenever it carries a `condition-coverage` attribute over-counts
  relative to the helper's `branch="True"` test.

**How to apply.** When a plan needs package-level counters that must reconcile with the root
attributes (for example a JaCoCo projection whose acceptance is exact equality), dot-source
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` — it dot-sources `PackageRate.ps1` and
`Threshold.ps1` for you — and call `Get-CoberturaPackageLineSummary -PackageNode $pkg`. It returns
`LineRate`, `BranchRate`, `LinesCovered`, `LinesValid`, `BranchesCovered`, `BranchesValid` as
strings, and the identity with the root attributes then holds by construction rather than by
assumption. Note both those files call `Set-StrictMode -Version Latest`, which applies to the
caller's scope after dot-sourcing, so use `GetAttribute()` rather than bare attribute property
access in the surrounding block.

Related: [[koverage-cobertura-postprocessing-shape]],
[[project_async_state_machine_emits_no_method_element]],
[[project_coverage_delta_reproduce_baseline_counting_method]].
