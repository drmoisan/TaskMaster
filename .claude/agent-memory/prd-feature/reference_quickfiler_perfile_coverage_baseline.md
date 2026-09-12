---
name: quickfiler-perfile-coverage-baseline
description: Where to find indicative per-file Cobertura rates for an epic #136 (quickfiler-per-file-coverage) child, why the number changes the child's framing, and why the @line-rate/@branch-rate attributes themselves are corrupt (issue #441)
metadata:
  type: reference
---

When scoping any child of epic #136 `quickfiler-per-file-coverage`, look for an already-committed
Cobertura report before assuming a file is uncovered. As of 2026-08-07 the usable one is
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
— grep it by `filename=` to get per-file entries in one pass. An exempted or interface-only file emits
no `<class>` element at all; absence is not 0%.

**The `line-rate` / `branch-rate` ATTRIBUTE on `<class>` is not trustworthy** (established by F11/#454
research, `research/coverage-harness-contract.md` §A.4). Two defects inflate it: issue **#441**
(`Get-CoberturaCoverageSummary` at `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:98` selects
`.//lines/line` at `:122`, matching both the method subtree and the class-level block, so every line is
double-counted) and issue **#478** (`Merge-CoberturaClassesByFilename` at `:167` unions class-level
`<lines>` correctly but never merges the other members' `<methods>`, so the recomputed rate blends a
correct union with a primary-only subtree). Verified case: `QfcHomeController.Iteration.cs` is truly
45/56 = 80.36% but the attribute says 0.8625. Async-heavy files are the common wrong case.

Correct recipe: union **`./lines/line` children only** (class-level; exclude `./methods//lines/line`)
across all `<class>` elements sharing the `@filename`, keyed on `@number` taking `MAX(@hits)`. Branch =
sum of `@condition-coverage` `(covered/total)` over `@branch="True"` lines. Zero lines or zero
conditions -> report **N/A**, never 0%. Per-file attribution DOES survive a partial split: Cobertura
emits one `<class>` per `(type, source file)` pair — `QfcItemController`'s 10 partials produce 10
elements with 10 distinct filenames.

Also: CI emits no Cobertura at all (`/EnableCodeCoverage` -> binary `.coverage`), so repo-wide figures
must be produced locally, and neither CI nor `Invoke-MSTestWithCoverage.ps1` filters
`.claude/worktrees` — a stale-worktree pre-flight assertion is required. epic.md's per-file baseline
table and its 70.19% repo figure are both unreliable (attribute-derived and raw-unprocessed
respectively); never use them as a child's comparator.

**Why it matters:** the F8 (#437) seed assumed six files were uncovered; all six measured 93-100% line,
inverting the child's framing. Expect the same pattern on siblings — but cite these figures as
INDICATIVE only, and never as acceptance evidence. Acceptance authority is a recomputed figure on the
child's own branch, committed under `<FEATURE>/evidence/qa-gates/`. Say so explicitly in the spec.

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
