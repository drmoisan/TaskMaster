---
name: delegation-prompt-needs-canonical-issue-and-branch-lines
description: Every gated Agent() delegation is denied TARGET_WORKTREE_NOT_DERIVABLE unless the prompt carries "Canonical issue number for this feature is N." and a "branch: <item branch>" line
metadata:
  type: project
---

A PreToolUse hook on `Agent` denies a delegation with `TARGET_WORKTREE_NOT_DERIVABLE` when the
prompt carries neither a canonical issue-number line nor a branch signal. Verified 2026-09-28 on the
issue 931 preparation run (isolated worktree, `Parallel mode` marker deliberately absent per the
parent's instruction): the first `Agent(task-researcher)` call was refused before launch.

**Why:** the model-routing gate refuses to check a delegation against a checkpoint that may belong
to a different item, so it needs to identify the item from the prompt text itself.

**How to apply:** put these two lines at the top of every delegation prompt, including
atomic-planner and atomic-executor preflight rounds:

```
Canonical issue number for this feature is 931.
branch: bug/<slug>-931
```

With both lines present, every delegation in that run (researcher, prd-feature, 5 planner and 5
executor rounds) was admitted. They are plain prose, so they satisfy a parent instruction to write
the issue number in prose and omit the `Parallel mode` / `issue_num:` markers. Related:
[[model-routing-hook-reads-canonical-path-only]].
