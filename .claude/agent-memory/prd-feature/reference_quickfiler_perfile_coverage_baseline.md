---
name: quickfiler-perfile-coverage-baseline
description: Where to find indicative per-file Cobertura rates when scoping an epic #136 (quickfiler-per-file-coverage) child, and why the line-rate ATTRIBUTE must never be used as an acceptance figure
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
Related: [[ac-gates-verify-satisfiability]].
