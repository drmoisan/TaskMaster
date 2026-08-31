---
name: harness-git-status-may-describe-another-worktree
description: The gitStatus block supplied at session start can describe a different checkout than the agent worktree a plan targets; never quote it as a fact about the target worktree.
metadata:
  type: feedback
---

Never cite the harness-supplied `gitStatus` block as an environment fact about the worktree a plan
targets. Measure inside the target path, or mark the fact unverified.

**Why:** on issue #637 the harness status described the session worktree
`TaskMaster-wt/2026-08-29T00-11`, where `docs/features/active/2026-08-07-...-440` was untracked. The
plan targeted the agent worktree `.claude/worktrees/agent-a68051a23e4479267`, where the same folder is
tracked — it had merged to `main` at the branch base commit. A preflight reviewer's contrary judgment
("every sibling folder is committed and clean") was correct and was overruled on the strength of the
wrong checkout's status. This repository routinely has several worktrees checked out at once, so the
two states diverge as a matter of course rather than exceptionally.

**How to apply:** before writing any tracked/untracked, dirty/clean, branch or HEAD claim into a plan,
run the command with `git -C <target worktree>` and observe the output. When no shell tool is
available in the session, two substitutes are workable and both were used on #637 R3:
- tracked-ness: grep the target worktree's index for the path literal. Resolve the index via the
  worktree's `.git` file (`gitdir: <main>/.git/worktrees/<name>`); the index is binary but ripgrep
  reports a match. Pair every positive probe with a negative control — a path known to be untracked —
  so a spurious match is detectable. Index membership proves tracked, not unmodified.
- presence on disk: `Glob`/`Grep` over the path.
Otherwise state the fact as unverified rather than inferring it. Prefer a justification that does not
depend on observed tree state at all: an executor runs later than the planning pass, so a
forward-looking reason ("a concurrent run can create this condition before execution") is both true
and stable. See [[project_637_selectrow_rooted_path_plan_seams]].
