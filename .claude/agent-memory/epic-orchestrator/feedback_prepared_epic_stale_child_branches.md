---
name: prepared-epic-stale-child-branches
description: After /epic-plan the prepared child branches survive as stale local refs that collide when execution children recreate them; delete them only if deletable, otherwise give execution children distinct -exec branch names
metadata:
  type: feedback
---

Before launching wave 0 of an `epic-planner`-prepared epic, check for leftover local child
branches (`bug/*`, `feature/*`) from preparation and resolve the name collision one of two ways.

**Why:** `epic-planner` prepares each child on its own branch, then fans the preparation commits
into `epic/<slug>-integration`. The child branches are left behind as local refs. When an
execution child later runs `git checkout -b <same-name>`, the name is taken, and the framework
falls back to a suffixed branch (the `-r2` pattern visible in older QuickFiler worktrees), which
desynchronizes the branch recorded in the epic checkpoint from the branch the child actually
pushes and PRs from. Related: [[feedback_hung_child_recovery_blocked_by_removal_gate]].

**How to apply — two cases, decided by whether the stale branch is still checked out.**

*Case 1, branch is free.* Prove it is a strict ancestor of the integration branch —
`git rev-list --left-right --count epic/<slug>-integration...<branch>` must show `0` on the right
side — then `git branch -D` it. Nothing is lost because the work is reachable from a pushed ref.
Leave the *remote* copies alone: they are ancestors of the integration tip, so a child's first push
is a fast-forward. Execution children then use the canonical names.

*Case 2, branch is checked out in a framework-locked worktree.* This is the common case when
`/epic-plan` and `/epic-run` share one `claude.exe` process: every preparation worktree under
`.claude/worktrees/` is still locked with `claude agent <id> (pid <session-pid>)`, and
`git worktree remove` fails with *"cannot remove a locked working tree"*. The lock will not release
while the session that owns it is alive, so waiting does not help and forcing is prohibited
(see [[feedback_merged_child_worktree_still_locked_defer_removal]]). Do **not** unlock, do not
`remove -f -f`, and do not delete the branch out from under a live worktree. Instead give every
execution child a distinct branch name — the `-exec` suffix works — and record that name in the
checkpoint's `features[].branch_name` and in the delegation receipt, so the checkpoint matches
reality from the first write.

**Check the stale branches for unmerged content before dismissing them.** In the QuickFiler
determinism epic three of four preparation branches were *not* ancestors of the integration tip:
each carried one or two `.claude/agent-memory` commits the preparation subagents wrote and that
`epic-planner` deliberately did not fan in (it treats them as non-deliverables). That content was
operationally load-bearing for execution — C# agent-worktree SDK/NuGet bootstrap steps, `trx`
needing an explicit results directory, an `atomic-planner` that has no MCP validator tool. Cherry-pick
those commits onto the integration branch before wave 0 so every child worktree inherits them;
expect a conflict on each namespace's `MEMORY.md` index and resolve it by keeping both lines.
Cherry-picking does **not** make the source branches ancestors (new SHAs), so it does not by itself
unlock Case 1.

Either way, tell each child to `git checkout -B <branch> origin/epic/<slug>-integration`
explicitly — the framework creates the isolated worktree from the session HEAD, not from the
integration branch, so the prepared feature folder and its committed `plan-path` are otherwise
absent from the child's tree.
