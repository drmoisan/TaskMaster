---
name: reuse-existing-item-worktrees
description: Reuse the planner-era per-item worktrees instead of creating new isolated ones — omit the isolation parameter and pass the absolute worktree path as a binding directive in the delegation prompt
metadata:
  type: feedback
---

When a parallel run reaches execution and the planner preparation worktrees still hold the
item feature branches, REUSE those worktrees. Do not detach their HEADs so that
`isolation: "worktree"` can build thirteen more.

**Why:** Operator instruction on run `bugs-2026-09-02` (2026-09-02): "please use existing
worktrees". TaskMaster is a C# repository, so each worktree carries a full `bin`/`obj` tree and
roughly 13,000 checked-out files; the repository already carried around fifty worktrees before the
run started. Creating one per item doubles that for no benefit, and the existing worktrees already
hold the warm build state the item needs. This SUPERSEDES the detach-first approach in
[[free-item-branches-by-detaching]] whenever the existing worktree is healthy — that note remains
correct about never using `git worktree remove` on a preparation worktree, and about the two
removal gates failing closed.

**How to apply:**

- **Omit the `isolation` parameter entirely on the child spawn.** There is no way to point
  `isolation: "worktree"` at a directory that already exists, and git refuses a second checkout of
  a branch, so the isolated spawn would fail anyway. Keep `run_in_background: true`.
- **Make the worktree path the FIRST line of the delegation prompt, as a binding directive.**
  Without SDK isolation the child inherits the session working directory, which is not its
  worktree. State that every read, write, edit, build, and git command must be rooted at the
  absolute worktree path, that the Bash tool resets its working directory between calls so every
  bash invocation needs its own `cd`, and that Read/Write/Edit take absolute paths. Name the
  session worktree and the main checkout explicitly as directories the child must not touch.
- **Audit each worktree before reusing it.** Check four things: the locking pid is dead
  (`Get-Process -Id <pid>`), `git status --porcelain` holds nothing but agent-memory noise, the
  local branch ref is an ancestor of its remote (`git merge-base --is-ancestor <b> origin/<b>`),
  and the local ref actually equals the remote. On this run SEVEN of thirteen local refs lagged
  their remotes; see [[parallel-run-execution-playbook]].
- **Fast-forward inside the worktree, not through a refspec.** The branch is checked out, so
  `git fetch origin <b>:<b>` is refused. Use `git -C <worktree> merge --ff-only origin/<b>`, which
  succeeds even with a dirty tree as long as the dirty paths are untouched by the incoming commits.
- **Tell the child which dirty files pre-exist and are not its work.** The planner-era subagents
  leave uncommitted `.claude/agent-memory/` writes behind; name the count so the child does not
  adopt them into its commits.
- **The shared-checkpoint hazard is unchanged, not worsened.** Item children resolve
  `artifacts/orchestration/orchestrator-state.json` to the session root whether or not they are
  isolated, so reuse costs nothing here. Still instruct each child to keep a per-item working
  checkpoint and synchronize the canonical path only immediately before a hook must read it. See
  [[children-share-one-orchestrator-state-file]].
