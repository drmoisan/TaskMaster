---
name: cobertura-class-helper-property-names-differ-from-package
description: Get-CoberturaClassLineSummary emits LineMap/TotalLines/CoveredLines and NO rate property, while Get-CoberturaPackageLineSummary emits LineRate/LinesCovered/LinesValid; a plan that asks for the package names from the class helper is unsatisfiable as written
metadata:
  type: project
---

The two coverage summary helpers in `scripts/vscode/` return **different property names for the same
concepts**, and the class-level one returns no rate at all. A plan that assumes they match will write
an acceptance condition nothing can satisfy.

| Helper | File | Emits |
|---|---|---|
| `Get-CoberturaPackageLineSummary` | `Invoke-MSTestWithCoverage.PackageRate.ps1` (`.OUTPUTS`, ~line 33) | `LineRate`, `BranchRate`, `LinesCovered`, `LinesValid`, `BranchesCovered`, `BranchesValid` |
| `Get-CoberturaClassLineSummary` | `Invoke-MSTestWithCoverage.Helpers.ps1` (`.OUTPUTS`, ~line 181) | `LineMap`, `TotalLines`, `CoveredLines`, `TotalBranches`, `CoveredBranches` |

Note the count naming is **inverted**, not merely different: package says `LinesCovered`/`LinesValid`,
class says `CoveredLines`/`TotalLines`. And the class helper emits **no rate property whatsoever**.

**Why:** item 871's plan demanded "the `LineRate`, `LinesCovered` and `LinesValid` from
`Get-CoberturaClassLineSummary`" in four separate tasks (P0-T12, P5-T5, P6-T1, P6-T2), and repeated the
error in its own evidence-conventions preamble. Those names are correct for the package helper only.
The planner had clearly checked one helper and generalised. This is exactly the defect class the
atomic-plan contract's "observe a command's success-case output before asserting over that output"
rule exists to prevent, and it is not detectable by any validator rule — a reviewer reading the plan
cannot see it either, because the names are plausible.

**How to apply:**

- When a plan cites class-level coverage figures, record them with an **explicit mapping note in the
  artifact**: `LinesCovered` is the helper's `CoveredLines`, `LinesValid` is its `TotalLines`, and
  `LineRate` is read from the class element's own `line-rate` attribute. Cross-check by recomputing
  `CoveredLines / TotalLines` to six places and confirming exact agreement with the attribute; if they
  disagree, the document is suspect rather than the mapping.
- Reproduce the identical mapping note in **every** artifact that reports class-level figures, not
  just the first. A reviewer who meets the explanation at one site and not the next will read the
  second as an unexplained substitution.
- Prefer a plan delta when one is available. On the parallel surface `Agent(atomic-planner)` is denied
  `PRD_FEATURE_BLOCKED`, so the mapping-plus-record route may be the only one open; record it as a
  deviation with `acceptance_conditions_changed: none` rather than editing the approved plan in place.
- The class element *does* carry a `line-rate` attribute even though the helper does not surface it.
  That is consistent with the plan preamble's true claim that class elements carry a rate attribute but
  carry no `lines-covered`/`lines-valid` attributes.

Related: [[cobertura-line-rate-attribute-is-wrong]] and [[cobertura-line-rate-double-counts]] for when
the attribute itself cannot be trusted, and [[csharp-coverage-denominator-two-figures]] for the
denominator question these figures feed.
