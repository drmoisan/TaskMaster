---
name: target-worktree-hook-needs-issue-and-branch-lines
description: An Agent(atomic-executor) spawn is denied with TARGET_WORKTREE_NOT_DERIVABLE unless the prompt carries the literal line "Canonical issue number for this feature is N." and a "branch:" label; the atomic-planner spawn was not gated
metadata:
  type: reference
---

Verified 2026-09-28 on the issue 928 preparation run (isolated agent worktree, parallel run
bugs-2026-09-28). The first `Agent(atomic-executor)` preflight spawn was denied:

`TARGET_WORKTREE_NOT_DERIVABLE: the call carries neither a canonical issue number line nor a branch
signal (--head, --branch, or branch:)`

The earlier `Agent(atomic-planner)` spawn with the same style of prompt was allowed, so the gate
is not uniform across delegates.

**How to apply:** put both lines near the top of every delegation prompt, planner included:

```
Canonical issue number for this feature is 928.
branch: bug/<slug>-928
```

This does not conflict with a parent instruction to "write the issue number in prose and add no
issue_num key": the line is prose, not a key, and it carries no "Parallel mode" marker.

Also from that run: under worktree isolation the Bash tool refused `pwsh` for the orchestrator,
but the Write tool bootstrapped `artifacts/orchestration/orchestrator-state.json` without a
pre-implementation-gate denial (contrast [[bootstrapping-orchestrator-state-json-first-write]]).
Preflight took 4 rounds (10, 7, 2, 0 defects); most defects came from coverage-measurement
commands the planner could not run itself. See [[delta-application-is-itself-a-defect-source]].
