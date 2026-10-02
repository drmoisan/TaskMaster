---
name: peer-can-take-your-item-branch-mid-prep
description: During a long preparation run a second orchestrator (a relaunch) checked out the item branch in another worktree, detaching mine, and pushed an older plan; check branch ownership before the final commit
metadata:
  type: project
---

Observed 2026-09-29 on the issue 927 preparation run (parallel run bugs-2026-09-28), after seven
preflight rounds over about six hours. At commit time `git commit` reported `[detached HEAD ...]`.
The reflog showed `checkout: moving from bug/...-927 to HEAD` that this run never issued: a second
worktree (`git worktree list` showed it locked, holding the branch) had imported this run's
feature folder and checkpoint copy, committed it as "preserve preparation work" plus a later
plan-round import, and pushed that tip to origin. Its checkpoint had a planner round of its own
pending. A branch can be checked out in only one worktree, so taking it detached mine silently.

**Why:** a parent that believes a child is dead relaunches it; the relaunch copies on-disk state
and continues. See [[feedback_detect_concurrent_orchestrator_before_delegating]] and
[[stale-checkpoint-is-not-a-dead-agent]] in the project memory.

**How to apply:** before the terminal commit (and ideally before each delegation in a long run),
run `git rev-parse --abbrev-ref HEAD` and `git worktree list`. If the branch is held elsewhere,
stand down: pin your commit with a local branch so it is not garbage-collected, do not push
(non-fast-forward, and force is prohibited), record the exact content delta against the peer's
tip, and report both SHAs to the parent.
