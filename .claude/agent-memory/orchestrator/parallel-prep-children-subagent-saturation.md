---
name: parallel-prep-children-subagent-saturation
description: With 8 concurrent epic preparation children, the 20-subagent cap saturates; delegations fail with "Concurrent subagent limit reached" and must be retried, never downgraded to in-thread work
metadata:
  type: feedback
---

When an epic runs preparation children in parallel (`epic-planner` with `max_parallel_features: 8`),
the shared 20-concurrent-subagent cap saturates. `Agent(...)` calls return
`Concurrent subagent limit reached. You can run 20 subagents at once. Do not retry.`

Wait for capacity and retry. Do NOT do the delegated step in-thread, and do NOT report the child
blocked.

**Why:** the cap is shared across all sibling worktrees, so saturation is a transient scheduling
condition, not a capability failure. Required delegated steps stay required. Doing research or
planning in the orchestrator thread would violate the delegation model and produce a child whose
`delegation_receipts` do not match its artifacts. See
[[feedback_verify_subagent_capability_claims]].

**How to apply:**
- Launch what you can, note which delegations were refused, and retry as slots free.
- Use a background `sleep` (foreground sleep is blocked) to wait between retries; each completing
  sibling or own subagent frees a slot.
- Sequence work so the retryable delegations are the ones whose inputs are already on disk.
- Observed on epic #136 (`quickfiler-per-file-coverage`, 2026-08-07): only 2 of 4 research agents
  launched initially; the remaining 2 took roughly 3 retry rounds over ~25 minutes.

Related: [[unplanned-epic-child-worktree-mechanics]],
[[parallel-preparation-children-shared-worktree]].
