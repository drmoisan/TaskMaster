---
name: plan-delegation-to-typed-engineer-without-dispatch-tool
description: A plan task that directs the executor to delegate code authoring to csharp-typed-engineer is unexecutable when the executor's tool surface has no sub-agent dispatch tool; perform the edits inline and record the substitution in the handoff artifact
metadata:
  type: project
---

Minor-audit plans routinely open Phase 1 with a handoff task worded "the executor delegates the
code-authoring tasks to `csharp-typed-engineer` and re-verifies each acceptance condition
itself." When `atomic-executor` is launched as a sub-agent, its tool surface is Read, Grep, Glob,
Edit, Write, Bash, and the PoshQC MCP tools — there is no Task or Agent tool, so the delegation
cannot be performed.

**Why:** blocking is forbidden after execution begins, and the delegation is a means rather than
an independent outcome — the plan's real requirement is that the edits stay bounded by the task
text and that every acceptance condition is verified against the tree.

**How to apply:** perform the edits inline, bounded by the task text, and record the substitution
explicitly in the handoff artifact under a heading naming the executing worker, stating that the
re-verification obligation is unchanged and how it is discharged. Do not silently claim the
delegation happened, and do not treat the missing tool as a preflight blocker discovered
mid-plan. An orchestrator that wants a genuine delegation must either dispatch the engineer
itself or give the executor a dispatch tool. Related:
[[project_followup_promotion_task_is_unexecutable_by_executor]].
