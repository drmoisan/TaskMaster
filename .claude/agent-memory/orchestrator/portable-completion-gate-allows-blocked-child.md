---
name: portable-completion-gate-allows-blocked-child
description: This repo lacks the Python orchestrator-state validator, so the orchestrator SubagentStop completion hook uses the portable PowerShell path, which does NOT require full completion — a blocked/partial child orchestrator can terminate cleanly.
metadata:
  type: project
---

In TaskMaster, `python -c 'import scripts.dev_tools.validate_orchestration_artifacts'` fails (module not present). The orchestrator SubagentStop hook `validate-orchestrator-output.ps1` therefore falls back to the portable `Test-OrchestratorStateCompletionReadiness` (`.claude/lib/orchestrator-state/OrchestratorStateCompletion.psm1`) via the `Test-PythonOrchestratorValidatorAvailable` seam.

**Why this matters:** the portable path does NOT enforce full completion (no require-complete). It only checks: (a) base presence — all `REQUIRED_STATE_KEYS` present, every `step5..10_status` in the enum, `blocked_reason` in the enum; and (b) the model-routing existence gate — every delegated agent (`delegation_receipts[].agent_name` + a delegating `next_step`) has a matching `model_routing_receipts[].agent`. It does NOT recompute the model formula (Python-only, documented non-goal).

**How to apply:** a child orchestrator that legitimately halts (e.g. `blocked_pending_maintainer_ratification`) CAN terminate cleanly. To also open the PR past `enforce-pr-author-skill.ps1`, set `blocked_reason: "none"` and steps 5-8 to a non-`pending`/non-`blocked` status (e.g. `verified`/`not-applicable`), and keep `local_execution_overrides`/`delegation_bypasses` empty. Record the real halt state in `next_step` (a non-delegating label so it does not trip the routing gate), plus custom fields like `terminal_status`/`ratification_pending`. `blocked_reason`'s enum has no "maintainer ratification" value — the halt is expressed outside that field. See [[orchestrator-state-flat-keys-and-enum]] and [[pr-author-hook-blocks-gh-in-this-repo]].
