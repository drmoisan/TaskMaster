---
name: delegation-prompt-needs-canonical-issue-and-branch-lines
description: Agent() delegations are denied with TARGET_WORKTREE_NOT_DERIVABLE unless the prompt carries "Canonical issue number for this feature is N." and a "branch:" label; and a plan revision-log bullet starting "- [P0-T2]" fails the plan validator as a malformed task line
description: Every gated Agent() delegation is denied TARGET_WORKTREE_NOT_DERIVABLE unless the prompt carries "Canonical issue number for this feature is N." and a "branch: <item branch>" line
metadata:
  type: project
---

Two small gates hit on the issue 930 preparation run (2026-09-28, bugs-2026-09-28 parallel slug).

1. The model-routing PreToolUse hook denied the first `Agent(atomic-planner)` call with
   `TARGET_WORKTREE_NOT_DERIVABLE`: the prompt named the issue only in prose ("issue 930 (nine-three-zero)").
   It requires the literal line `Canonical issue number for this feature is 930.` plus a
   `branch: <item branch>` label. Adding both lines let every later delegation through. This is compatible
   with an operator rule forbidding "Parallel mode" markers or `issue_num` keys.

2. The MCP plan validator rejected the plan with `Line N: task line must match - [ ] [P#-T#] <Title>` because
   the planner's revision-log prose contained a bullet beginning `- [P0-T2] records ...`. Any line starting
   with a dash and a bracket is parsed as a task line. Rewording to `- Task [P0-T2] ...` fixed it.

**Why:** both deny messages are clear only once seen; each cost one extra round trip.

**How to apply:** put both identifier lines at the top of every delegation prompt for a parallel or epic
item; tell the planner never to start a non-task bullet with a bracketed task ID. Related:
[[model-routing-hook-reads-canonical-path-only]], [[mcp-plan-validator-requires-lf]].
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
With both lines present, every delegation in that run (researcher, prd-feature, 5 planner and 5
executor rounds) was admitted. They are plain prose, so they satisfy a parent instruction to write
the issue number in prose and omit the `Parallel mode` / `issue_num:` markers. Related:
[[model-routing-hook-reads-canonical-path-only]].
