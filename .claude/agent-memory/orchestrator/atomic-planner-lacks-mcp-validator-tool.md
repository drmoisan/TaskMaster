---
name: atomic-planner-lacks-mcp-validator-tool
description: Agent(atomic-planner) has no mcp__drm-copilot__* tool in its surface, so the orchestrator must run the mandatory plan validator gate itself after every planning or revision delegation
metadata:
  type: project
---

`Agent(atomic-planner)` is provisioned with Read, Grep, Glob, Edit, and Write only. It has no Bash and no `mcp__drm-copilot__*` tool, so it CANNOT run `validate_orchestration_artifacts` — the gate that `atomic-plan-contract` makes mandatory before a plan may be treated as approved.

**Why:** `atomic-plan-contract` says "run the MCP validator; do not treat human-readable summaries as a substitute for validator success." A planner asked to self-validate can only do one of two things: report honestly that the tool is absent, or fabricate a pass. Observed twice on 2026-08-21 (#449): the planner correctly reported "VALIDATOR NOT RUN — tool unavailable in this agent's surface. I will not claim a gate passed that I did not execute," and substituted a structural self-check (phase-heading form, task-ID sequence, line-ending uniformity). That is the right behavior and should be reinforced, not corrected.

**How to apply:**
- The ORCHESTRATOR runs the plan validator after every planner delegation. Do not put "run the validator and report its result" in the planner prompt as a hard requirement; instead say explicitly "I will run the validator myself; do not fabricate a validator result." That removes the pressure to invent a pass.
- Independently verify the planner's edit list against the file rather than trusting the report. On #449 the planner's prose said "all 91 tasks unchanged" while its own per-phase enumeration summed to 98 (the real count) — a harmless miscount, but it shows the narrative can drift from the artifact.
- The same absence applies to the planner writing memory: on #449 it declined to write `.claude/agent-memory/` because that tree is tracked and the plan's own final tasks gate on an empty `git status --porcelain`. Sound reasoning; the orchestrator should carry such flagged-for-upstream items in its own report.

Related: [[preflight-catches-vacuous-gates]], [[verify-subagent-capability-claims]] (verify a capability CLAIM against `.claude/agents` — but a report of a tool's absence that matches the registered tool list is accurate, not an excuse).
