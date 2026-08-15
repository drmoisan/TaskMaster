---
name: never-plan-a-mid-plan-halt-on-mcp-availability
description: "Never schedule a mid-plan halt on MCP tool availability — the executor's tool surface differs from the orchestrator's; plan a record-blocker-and-continue branch instead"
metadata:
  type: feedback
---

A plan task must never say "verify tool X is available; if not, halt". The executor contract permits blocking
only during preflight, before P0-T1, so a mid-plan halt strands the run (in #494 it would have stopped at task
79 of 97 with Phases 0-4 complete and no defined resumption).

**Why:** the orchestrator's session and a delegated executor subagent's session do not expose the same MCP
surface. `mcp__drm-copilot__potential_to_issue` in particular is reachable from the orchestrator even when the
executor cannot call it, so an executor-observed absence is not a real blocker for the work — only for *that
agent*. See [[project_planner_mcp_validator_not_in_tool_surface]] for the same asymmetry hitting the plan
validator, and [[poshqc-mcp-and-msbuild-invocation-facts]] for the availability-vs-failure distinction.

**How to apply:** in Phase 0, add a *probe* task that records whether the tool is callable (e.g. "record
`PROMOTION MCP UNAVAILABLE` and continue"). Downstream tasks then branch on the recorded probe result:
author the inputs anyway (potential entries under `docs/features/potential/`), record the blocker to
`<FEATURE>/evidence/issue-updates/`, run the plan to completion, and have the dependent check-off tasks record a
`remediation-required` outcome rather than a PASS. State explicitly that the orchestrator completes the
promotion from the recorded paths, and forbid filing by any other route. Never let the branch be "halt", and
never let it be "use a different tool".
