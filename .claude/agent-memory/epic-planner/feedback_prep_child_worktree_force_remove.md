---
name: prep-child-worktree-force-remove
description: A prepared child worktree may block git worktree remove with uncommitted orchestrator-agent-memory scratch; force-remove is safe once HEAD equals the merged tip
metadata:
  type: feedback
---

During fan-in cleanup, `git worktree remove` of a completed preparation-mode child worktree can
fail with "contains modified or untracked files" even though the feature deliverables were
committed and merged.

**Why:** The child `orchestrator` run writes to its own persistent memory namespace
(`.claude/agent-memory/orchestrator/`) inside its isolated worktree. Those writes (a modified
`MEMORY.md` plus new note files) are uncommitted and are not feature deliverables. In the
utilitiescs-nullable-remediation four-child fan-in (2026-07-18), the newtonsofthelpers worktree
(agent-ade6f7ef5156375b4) blocked removal for exactly this reason.

**How to apply:** Before force-removing, confirm the worktree's `HEAD` equals the branch tip you
already merged (`git -C <wt> rev-parse HEAD` == the fanned-in commit) and that `git -C <wt> status
--porcelain` shows only `.claude/agent-memory/orchestrator/**` entries. If so, the feature folder
is fully captured on the integration branch and `git worktree remove --force` discards only the
orchestrator's own memory scratch — do NOT commit that scratch onto the epic integration branch
(wrong agent namespace, out of scope). If status shows any `docs/features/active/<slug>/**` change,
STOP: that is unmerged feature work, not scratch.
Related: [[concurrent-prep-children-worktree-isolation]].
