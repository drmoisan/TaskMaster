---
name: quickfiler-perfile-coverage-baseline
description: Where to find indicative per-file Cobertura rates for an epic #136 (quickfiler-per-file-coverage) child, why the number changes the child's framing, and why the @line-rate/@branch-rate attributes themselves are corrupt (issue #441)
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

**CRITICAL CORRECTION (added 2026-08-07 during F9/#452 preparation): do NOT read the rates from
`@line-rate`/`@branch-rate`.** Open issue #441 is worse than its title. `Merge-CoberturaClassesByFilename`
(`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:167-292`) rebuilds merged `<lines>` correctly
but then recomputes the rate attributes using a defective `.//lines/line` descendant selector, which
double-counts because each `<class>` carries every line twice (method-level plus a class-level
rollup). **Per-file rates are corrupted, not just the repository total.** Proof: `FilerQueue.cs`
records `line-rate="0.405797"` = 28/69 while its true class-level rate is 18/49 = 0.367347;
`branch-rate="0.428571"` = 6/14 while the true rate is 5/10 = 0.5. Direction is not uniform — line
was overstated, branch understated. The epic's own baseline table at `epic.md:161` is wrong for that
file.

**Detection tell:** `Get-CoberturaCoverageSummary` rounds to six decimals (helpers `:137-138`) while
dotnet-coverage emits full double precision. A 16-significant-digit rate was never merged and is
trustworthy; a rate with <= 6 decimals went through the defective path. Most QuickFiler entries carry
the rewrite signature.

**How to apply:** derive rates yourself from the direct-child axis
`/coverage/packages/package/classes/class/lines/line`, grouped by `class/@filename`, deduped by
`@number` taking `max(@hits)`; branch from `condition-coverage="(c/t)"` on `@branch="True"` lines.
Never use the `.//` descendant axis or the root `@lines-valid`. Then cite the figures as INDICATIVE
only — they were captured on another feature's branch. The epic's acceptance authority is F1's
per-file harness re-run on the child's own branch, committed under `<FEATURE>/evidence/qa-gates/`.
Say so explicitly in the spec, and disclose the #441 workaround in the evidence artifact.
Related: [[ac-gates-verify-satisfiability]].
