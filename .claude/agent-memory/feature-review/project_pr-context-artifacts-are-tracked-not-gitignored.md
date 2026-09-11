---
name: pr-context-artifacts-are-tracked-not-gitignored
description: In TaskMaster artifacts/pr_context.summary.txt and .appendix.txt are TRACKED in git and main carries a stale pair from an unrelated feature; regenerating them dirties a tracked file in the branch under review, so prefer direct git derivation and record the deviation
metadata:
  type: project
---

`artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` are **tracked** files in
TaskMaster, not gitignored. Verified on #730 (2026-09-02): `git check-ignore -v` exits 1 (not ignored)
and `git ls-files --error-unmatch` succeeds. `origin/main` currently carries a pair generated for a
*different* feature (`bug/claude-md-cites-ciyml-for-moved-toolchain-commands-564`, Head SHA
`fafe3d4d`), so an unrelated branch will almost always find them stale under the
`pr-context-artifacts` head-binding cross-check.

**Why:** the standing instruction is "regenerate if stale," and [[pr-context-mcp-unavailable-manual-fallback]]
says to hand-author them. But hand-authoring over a *tracked* file writes an unrelated modification
into the branch under review — the reviewer would be injecting a change into the delivery it is
auditing, violating the no-mutation principle. The earlier fallback memory was written for the case
where the files were *absent*, which is a different situation with no such cost.

**How to apply:** Check `git ls-files --error-unmatch artifacts/pr_context.summary.txt` before
regenerating. If tracked and unmodified, do NOT overwrite. Derive the diff directly from git
(`git diff --name-status origin/main...HEAD`, `--numstat`, `git grep HEAD`) — strictly stronger
evidence than the collector summary anyway — and record the staleness plus the deliberate
non-regeneration as an explicit deviation in the policy audit.

Before deciding it matters for the termination hook, simulate: dot-source
`validate-feature-review-coverage.ps1` and run `Get-ChangedLanguageSet` against *both* the session-cwd
and item-worktree copies. On #730 the stale summary was docs-only, so it yielded `count=0` and the
coverage-row enforcement path was provably unreachable regardless. A stale summary is only dangerous
when it lists `.cs`/`.ps1`/`.py`/`.ts` paths that the real branch does not have — that would demand
coverage rows for languages with no changed files. See [[coverage-hook-skips-when-no-pr-context-summary]].
