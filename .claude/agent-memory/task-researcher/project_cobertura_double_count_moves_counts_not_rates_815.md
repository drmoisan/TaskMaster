---
name: cobertura-double-count-moves-counts-not-rates-815
description: "#815: the Cobertura .//line double count roughly DOUBLES the four counters but leaves the derived percentages within 0.00-0.46pp, so it cannot explain a multi-point rate swing; measured exactly on two committed documents"
metadata:
  type: project
---

Issue #815 claimed the pinned `.//line` aggregation inflated issue 809's first-party BRANCH coverage
from a true 77.03% to a reported 79.38% (2.35 points). **Measurement refutes the magnitude.**

**Measured (2026-09-08), each tally reconciled exactly against the document's own root attributes:**

- RAW `docs/features/archive/2026-07-18-utilitiescs-nullable-helperclasses-364/evidence/baseline/coverage-baseline.2026-07-19T08-51.cobertura.xml`:
  class-level 136,359 / method-level 137,194 / all-descendant 273,553. Branch naive 24,582/50,732 vs
  deduped 12,291/25,366 — **48.4546% either way, exactly**. Line 72.0416% vs 72.0686% (−0.027pp).
- POST-PROCESSED `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/qa-gates/postchange.cobertura.xml`:
  class 64,406 / method 50,405 / all-descendant 114,811. Branch naive 23,249/29,162 = 79.7236% vs
  deduped 13,120/16,524 = 79.3997% (+0.324pp). Line +0.462pp.

**Why the rate is invariant:** the method block and the class rollup are two views of the same
instrumentation, so the naive sum adds `(c/d)` twice where the truth is `(c/d)` once. Doubling both
sides leaves the ratio fixed. The rate only moves when covered and uncovered branches are duplicated
at *different* multiplicities.

Two independent corroborations already in the repo:
`.claude/agent-memory/feature-review/project_791-review-residuals.md:41-43` ("the derived percentages
match to the digit under both selections") and the committed test comment at
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1:210-211` ("the branch RATIO is
unchanged by the double count, so this must assert branches-valid/branches-covered, never
branch-rate").

**How to apply:**
- The defect is real but is a COUNT defect, not a RATE defect. Never accept a claim that this
  mechanism moved a percentage by more than ~0.5pp without a reconciled measurement.
- The 77.03% figure is unreconstructable: 809's raw Cobertura is NOT committed (its whole
  `evidence/` tree is `.md` only, because `coverage/` is gitignored at `.gitignore:144`). Do not gate
  any fix on reproducing it.
- Measurement technique when `pwsh` is unavailable: these files are 2-space pretty-printed with one
  `<line>` per physical line, so depth-anchored `Grep -c` on `^ {12}<line number=` (class rollup) and
  `^ {16}<line number=` (method) gives exact node counts, and the sum must equal an unanchored
  `<line number=` count. Validate by reproducing the document's own root attributes.
- The site is PLAN PROSE only. `Grep` for `\.//line` returns 81 files and **zero** under `scripts/`;
  no skill, rule or workflow pins it. Script code (`Get-CoberturaClassLineSummary`,
  `Get-CoberturaPackageLineSummary`, `Get-CoberturaCoverageSummary`) is correct.
- `Get-CoberturaCoverageSummary` takes only `-XmlDocument` — no allowlist — so it cannot serve a
  raw-document first-party aggregation; a new sibling file is needed (Helpers.ps1 is 469/500).
- `Assert-CoberturaLineCoverageThreshold` enforces LINE >= 80% only, on the post-processed root
  attribute. There is no branch gate in script code anywhere.

Related: [[cobertura-root-attrs-raw-vs-postprocessed]], [[cobertura-closure-exemption-457]],
[[coverage-threshold-reconciliation-494]].
