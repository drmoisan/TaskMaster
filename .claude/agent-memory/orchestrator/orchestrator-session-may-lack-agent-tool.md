---
name: orchestrator-session-may-lack-agent-tool
description: An orchestrator session can be launched with NO Task/Agent tool at all, which blocks every mandatory delegated step; check the tool surface before promising delivery
metadata:
  type: feedback
---

An orchestrator session can be launched with **no `Task`/`Agent` delegation tool in its tool surface at
all**. Observed 2026-09-03 on the #736 parallel item: the available tools were Read, Grep, Glob, Write,
Edit, Bash and the `mcp__drm-copilot__*` set only. Every remaining step of the run (atomic execution,
feature-review) is a mandatory delegated step, so the correct outcome was
`blocked_reason: spawn_agent_unavailable`, not local implementation.

**Why:** The orchestrate contract is orchestration-only — "For required delegated steps, delegation is
mandatory. If a handoff cannot be started, resumed, or completed, stop execution and record blocked
state. Do not perform the step locally." An orchestrator that quietly executes a 77-task plan itself
produces unreviewed work under a role that exists specifically to keep planning, execution, and review
in separate agents, and the resulting branch carries no executor or reviewer receipt.

**How to apply:** Check for the delegation tool *before* committing to a delivery promise, not after
finishing the setup work. If it is absent, still do all the non-delegated work that is durable and
useful — base reconciliation, checkpoint repair, plan re-validation, citation re-anchoring — commit and
push it so a resumed session starts at the delegating step with zero rework, then report blocked with a
precise `what_was_completed` / `what_remains` split. `spawn_agent_unavailable` is in the validator's
`VALID_BLOCKED_REASONS`, so the checkpoint still validates while blocked. Contrast
[[blocked-reason-enum-cannot-express-substantive-halt]], which is about halts the enum *cannot* express;
this one it expresses exactly.

Related: [[mcp-tools-available-to-orchestrator]], [[no-sendmessage-relaunch-with-resume-brief]].
