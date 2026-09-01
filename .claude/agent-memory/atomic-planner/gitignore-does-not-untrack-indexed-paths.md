---
name: gitignore-does-not-untrack-indexed-paths
description: Never infer "untracked" from a .gitignore pattern; a force-added path stays tracked and keeps appearing in git status — verify against the index
metadata:
  type: feedback
---

A `.gitignore` pattern does not make a path untracked. `.gitignore` governs untracked paths only and has no
effect on a path already recorded in the index, so a path force-added once (`git add -f`, or an agent run that
staged it) remains tracked forever and keeps appearing in `git status` even though its directory is ignored.
`artifacts/orchestration/orchestrator-state.json` in TaskMaster is exactly this case: `.gitignore:57` ignores
`artifacts/`, yet the file is in the index, `git ls-files --error-unmatch` on it exits 0, and `git check-ignore`
on it exits 1 (check-ignore is index-aware unless `--no-index` is passed).

**Why:** In the #648 plan I "corrected" a reviewer's true statement into a false one by reasoning from
`.gitignore:57` alone, asserting the path could never appear in `git status --porcelain`. It had been showing as
` M` throughout the run. Replacing a true clause with a false one is worse than the defect being fixed.

**How to apply:** Before writing any tracked/untracked/ignored claim into a plan, verify it. With a shell:
`git ls-files --error-unmatch <path>` (exit 0 = tracked) plus `git check-ignore -v <path>` (exit 1 = not
ignored). Without a shell: grep the worktree's index binary — `.git/worktrees/<id>/index` for a linked worktree,
`.git/index` otherwise — for the repository-relative path, and pair it with a negative control (a path that
exists on disk and is genuinely ignored, e.g. `.claude/settings.local.json`, must be absent from the index).
Related: [[agent-memory-is-tracked-scope-git-gates]], [[gitignore-bracket-classes-defeat-literal-grep]],
[[stale-build-output-is-not-evidence-of-existence]].
