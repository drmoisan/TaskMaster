---
name: 927-review-residuals
description: Issue #927 (evidence and identity hygiene sweep) review outcome - PENDING-CI disposition for CI-only ACs, C# coverage noise with zero production change, cobertura ignore-pattern projection trap, bundled PoshQC artifact unfit for scripts/hygiene
metadata:
  type: project
---

Review of #927 (2026-09-29, head 32d93a367, plan revision 1.17): PASS on delivered work, 0 blocking,
3 ACs PENDING-CI (AC4 CI Pester coverage, AC13 CI mstest-coverage plus the P6-T39 re-anchor, AC16
hygiene context), plus the modified-workflow green-run rule. No pull request existed at review time,
and ci.yml triggers only on push to main/development or pull_request, so a fast-forward push of the
branch produces no run.

**Why:** The caller supplied a "record as PENDING-CI, not FAIL" ruling for items whose evidence can
only come from the pull-request head; the rest of the criteria were verifiable locally and were
re-run (gates 1, 2, 4, 9, the three raw-document gates and check-ignore all returned zero).

**How to apply:**
- C# first-party coverage moved -0.01 pp on both line and branch (7 lines, 1 branch, all in
  UtilitiesCS) with ZERO production C# changed; disposition PASS / collector noise, with the
  authoritative comparison re-anchored to main's own CI at the merge base. Do not FAIL a
  sub-0.02-point movement when the changed files are test classes outside the denominator.
- `.gitignore` now carries `*cobertura*.xml`; a permitted projection copied into an evidence tree
  under the route's default stem `coverage.cobertura.jacoco.xml` is silently ignored by `git add`.
  Rename on copy (the 18 retained projections use other stems).
- `artifacts/pester/powershell-coverage.xml` from `run_poshqc_test` instruments only
  `.claude/hooks` (all zero) and carries no `scripts/hygiene` package; the direct-Pester JaCoCo
  under the ignored `coverage/` directory has the real per-file counters. Record the canonical-path
  artifact as FAIL/non-blocking on its own line and give the language verdict on another.
- With the session cwd holding a stale `artifacts/pr_context.summary.txt` whose changed-file lines
  are all .md/.yml/.csproj, the SubagentStop hook's language set is empty and only the three
  path checks run; the 3-`..` traversal advertisement form (session root -> `.claude/worktrees/<wt>`)
  resolves from the session cwd.
- The validator's 7-cell-row trap applies to ANY table in the policy audit (a test-metrics table
  with 7 columns would be parsed as coverage rows); keep every non-coverage table at != 7 columns.
- Writing agent memory under the item worktree's `.claude/agent-memory/` puts a governance path
  into the branch's porcelain; committing it onto the feature branch would contradict this feature's
  AC19 GOVERNANCE=0 clause, so the orchestrator must commit memory separately (as the parallel run
  did in its docs(memory) commits).

See [[review-residuals-index]], [[csharp-coverage-constants-nondeterministic]],
[[poshqc-bundled-coverage-artifact-reads-zero]], [[review-worktree-differs-from-session-cwd-mirror-artifacts]].
