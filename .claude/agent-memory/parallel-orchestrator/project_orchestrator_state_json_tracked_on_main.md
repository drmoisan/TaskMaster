---
name: orchestrator-state-json-tracked-on-main
description: artifacts/orchestration/orchestrator-state.json is TRACKED on main despite .gitignore listing artifacts/, so every child orchestrator dirties a tracked file and pollutes its own diff footprint
metadata:
  type: project
---

`artifacts/orchestration/orchestrator-state.json` is tracked in git on `main`. Verified 2026-09-01
with `git ls-tree -r --name-only origin/main`, which returns exactly that one path under
`artifacts/`. `.gitignore` line 57 lists `artifacts/`, but an ignore rule does not untrack a file
that was already added, so the entry is inert for this path.

**Why:** It was added by commit `e8e628f0` from an unrelated Codex run. Consequence: every child
orchestrator that writes its checkpoint modifies a TRACKED file, so the change shows up in
`git status` and in any anchored repository-wide diff. That matters because item plans routinely
assert a footprint clause — "the anchored diff carries no path outside the named code paths and the
feature folder" — and a dirtied checkpoint silently violates it. On item 647 of run
bugs-638-644-647 the child contained it locally with `git update-index --skip-worktree` and
committed nothing to that path.

**How to apply:**

- Expect a dirty `artifacts/orchestration/orchestrator-state.json` in item worktrees. It is not
  evidence that the child went out of scope.
- The local containment is `git update-index --skip-worktree` on that path inside the worktree. It
  is per-worktree and does not fix the repository.
- The real fix is `git rm --cached` on `main` so the existing `.gitignore` entry takes effect. That
  is a repository change outside any single item's scope; do not fold it into an item's branch,
  because it would breach that item's own footprint clause. Route it as its own issue.
- Do not confuse this with the parallel checkpoint. `artifacts/orchestration/parallel-orchestrator-state.json`
  is NOT tracked, so parent-side checkpoint writes are invisible to git and need no containment.
- The stale-record hazard is separate and still live: the session-root copy of this file may name a
  different item than the one running. Verify the issue number before relying on it, per
  [[children-share-one-orchestrator-state-file]].
