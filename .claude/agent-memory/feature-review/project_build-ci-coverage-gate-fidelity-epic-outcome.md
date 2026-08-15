---
name: build-ci-coverage-gate-fidelity-epic-outcome
description: The 80-vs-85 coverage contradiction was NOT removed by the epic; #494 deferred it upstream and added a runner gate at 80 while the review hook still enforces 85 — two live gates disagree
metadata:
  type: project
---

Outcome of the `build-ci-coverage-gate-fidelity` epic fan-in review (2026-08-15, head `22b5de02`,
0 Blocking / 5 Major).

**The always-loaded coverage-threshold contradiction is still live.** `CLAUDE.md:303` and
`.claude/rules/csharp.md:44` say `>= 80%` repo-wide line; `.claude/rules/general-unit-test.md:23-24`
and `.claude/rules/quality-tiers.md:33-34` say `>= 85%` line / `>= 75%` branch uniformly. None of
those four files' threshold text was changed by the epic.

**Why:** `#494`'s spec contains a "User-Authorized Scope Correction" (`spec.md:25-55`) that
supersedes every instruction to edit `CLAUDE.md`, non-memory Claude runtime paths, or
`.agents/skills/**`. Decisions D1-D7 (reconcile to 80/90, make `CLAUDE.md` § UT2 the single
authority, cite-do-not-restate, delete the false `quality-tiers.yml` claims) are SPECIFIED but NOT
APPLIED; 7 of 10 ACs are satisfied by an upstream prompt artifact merely *carrying a requirement*.
The authorization is not corroborated in-repo — `artifacts/orchestration/epic-orchestrator-state.json`
has no `human_interaction` block and the child checkpoint is gitignored.

**Now there are TWO live numeric gates that disagree:**
- `Assert-CoberturaLineCoverageThreshold` (`Invoke-MSTestWithCoverage.Helpers.ps1:459`) throws below **80%**, called from `Invoke-MSTestWithCoverage.ps1:341`.
- `.claude/hooks/validate-feature-review-coverage.ps1:313` FAILs below **85.0**, `:323` `$BranchFloor = 75.0`, while its own `.SYNOPSIS:29` documents "below 80 percent".

Any repo-wide figure in `[80, 85)` passes one and fails the other.

**Useful measured constants (from committed evidence, do not re-derive):**
- C# repo-wide corrected: line ~85.55% (53381/62401), branch ~79.04% — clears BOTH camps' floors.
  Three runs spread 0.0176 pp with identical `lines-valid`, so D5's reproducibility exit condition is met.
- PowerShell repo-wide: **not a constant — measure it in-session.** A second reviewer run at the
  same head read 68.90% (494/717) against the committed 71.51% (502/702). See
  [[powershell-coverage-nondeterministic-vsbuild-tests]] for the cause. What IS stable: the
  shortfall sits in never-tested scripts (`temp-extract-coverage` 0/58, `TestProcessCleanup` 0/29,
  `Invoke-Restore` 0/16, `run-actionlint` 0/9, `Install-RepoDotNetSdk` 3/33) that are rarely in a
  diff, while the branch's own changed files aggregate above 91%.
- **PowerShell branch coverage does not exist**: Pester 5.6.1 is command-based and has no branch
  counter. Committed JaCoCo contains zero `<counter type="BRANCH">`. Do not chase it.
- Follow-ups from the epic are all really filed, with numbers: #529, #530, #531, #532 (441's four),
  #535 (512's SD1 mirror sites), #536, #537, and #558/#559/#560 (457's three AC15 residuals).
  Do not re-report them as owed.

**How to apply:** in any later review touching coverage policy, do not assume the epic fixed the
contradiction. Check which gate applies to the decision at hand. Also note `CLAUDE.md:194,202,210`
still point at `.github/workflows/ci.yml` for the csharpier/analyzer/nullable steps, which the #553
CI split moved into `_format-check.yml` / `_build-analyzers.yml` / `_build-nullable.yml`.

Related: [[epic-fanin-artifact-path-and-hook-regex]], [[553-ci-split-review-pattern]]
