---
name: prepared-epic-stale-child-branches
description: After /epic-plan, the prepared child branches survive as stale local refs that collide when execution children recreate them; delete them pre-wave-0 after proving ancestry into the integration branch
metadata:
  type: feedback
---

Before launching wave 0 of an `epic-planner`-prepared epic, check for leftover local child
branches (`bug/*`, `feature/*`) from preparation and delete them once ancestry is proven.

**Why:** `epic-planner` prepares each child on its own branch, then fans the preparation commits
into `epic/<slug>-integration`. The child branches are left behind as local refs. When an
execution child later runs `git checkout -b <same-name>`, the name is taken, and the framework
falls back to a suffixed branch (the `-r2` pattern visible in older QuickFiler worktrees), which
desynchronizes the branch recorded in the epic checkpoint from the branch the child actually
pushes and PRs from. Related: [[feedback_hung_child_recovery_blocked_by_removal_gate]].

**How to apply:** Prove each stale branch is a strict ancestor of the integration branch first —
`git rev-list --left-right --count epic/<slug>-integration...<branch>` must show `0` on the right
side. Then `git branch -D` it; nothing is lost because the work is already reachable from a pushed
ref. Leave the *remote* copies alone: they are ancestors of the integration tip, so a child's first
push is a fast-forward. Also tell each child to `git checkout -B <branch>
origin/epic/<slug>-integration` explicitly — the framework creates the isolated worktree from the
session HEAD, not from the integration branch, so the prepared feature folder and its committed
`plan-path` are otherwise absent from the child's tree.
