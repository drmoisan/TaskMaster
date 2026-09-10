---
name: branch-coverage-denominator-trap-raw-cobertura
description: The raw Cobertura root branch-rate is ~13 points below the post-processed first-party figure, so a line-only coverage check passes while branch silently sits below the 75% floor
metadata:
  type: project
---

C# coverage in this repo has two denominators for **branch** rate, not just for line rate, and the
gap is far wider on branch. Measured on issue #826, 2026-09-09, at the same commit:

| Artifact | Packages | line-rate | branch-rate |
|---|---|---|---|
| Raw `dotnet-coverage` Cobertura (`coverage/826-raw/p7-t4.cobertura.xml`) | 23 | 0.8613 | **0.6649** |
| Post-processed, first-party (`Invoke-MSTestWithCoverage.ps1` output) | 9 | 0.8570 | **0.7987** |

**Why it is a trap:** the raw root includes the nine `*.Test` assemblies and five vendor packages
(`log4net`, `Mono.Reflection`, `Microsoft.IO.RecyclableMemoryStream`, `System.Linq.Async`,
`System.Interactive`). The test assemblies are near-fully covered and **inflate** the line rate, so
line looks comfortable either way. The vendor packages are barely branch-covered and **crush** the
branch rate to 8 points *below* the uniform 75% floor in `.claude/rules/quality-tiers.md`. So a
gate that reads the raw root and checks line only reports a clean pass while sitting on a failing
branch number.

The post-processed first-party denominator is the policy-conformant one, and it is what
`Assert-CoberturaLineCoverageThreshold` inside `Invoke-MSTestWithCoverage.ps1` actually asserts.
`LinesValid 65402` is the first-party signature; a `lines-valid` in the 200000 range means you are
reading a raw merged root.

**How to apply:**
- Never read a branch figure off a raw Cobertura root. Run
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1` and read the `First-party coverage:` line it
  prints, which gives both figures on the correct denominator in one place.
- If a plan's coverage tasks capture only a line figure, that is a gap even when the plan passes:
  C# is branch-capable, so the 75% branch floor applies and an unmeasured floor is an unchecked
  one. On #826 no plan task or evidence artifact recorded a branch number at all; the reviewer
  caught it and I closed it by parsing both roots.
- When comparing a coverage figure to a sibling feature's, check the denominators match first. The
  nine-named-assembly vstest form and the `-SearchRoot .` script form give different test totals
  (7190/7192 vs 7213/7215 on the same tree) and different package counts.

Related: [[csharp-coverage-denominator-two-figures]], [[jacoco-not-cobertura-for-evidence]],
[[cobertura-postprocessing-is-a-zero-exit-proxy-not-a-test-result]],
[[feature-review-coverage-85-floor-trap]].
