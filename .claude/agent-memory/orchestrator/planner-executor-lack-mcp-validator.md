---
name: planner-executor-lack-mcp-validator
description: atomic-planner and atomic-executor subagents have no mcp__drm-copilot__* tools, so the orchestrator must run the plan validator gate itself — never accept a delegated "validator passed" claim
metadata:
  type: feedback
---

`Agent(atomic-planner)` and `Agent(atomic-executor)` in this repo are provisioned
with file-level tools only (Read/Grep/Glob/Edit/Write, plus a narrow Bash allowlist
for the executor). **Neither has `mcp__drm-copilot__validate_orchestration_artifacts`.**
Instructing a planner to "run the validator and report the result" produces a correct
but useless "VALIDATOR NOT RUN — tool unavailable" report.

**Why:** the `atomic-plan-contract` skill makes the MCP validator a mandatory gate
before a plan can be treated as approved, and explicitly says human-readable summaries
are not a substitute. If the orchestrator delegates the gate and the delegate cannot
run it, the gate silently does not happen. Observed three times on epic #136 child F4
(#434): the initial plan generation and both revision passes all reported the tool
absent. Each time the planner correctly refused to claim a pass — but the orchestrator
still had to run it.

**How to apply:** always run the plan validator from the orchestrator thread yourself,
immediately after the planner reports. Still ask the planner to attempt it (its refusal
is a useful integrity signal), but treat the orchestrator-side run as the authoritative
gate. The same applies to the orchestrator-state validator. Related:
[[mcp-tools-available-to-orchestrator]] (same root cause: when a worker says an MCP
tool is unavailable, run it yourself rather than accepting the block) and
[[feedback_verify_subagent_capability_claims]] (but note this claim IS true — verify,
do not assume it is an excuse).
