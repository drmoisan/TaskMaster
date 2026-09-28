---
name: mcp-orchestrator-state-validator-is-existence-only-for-routing
description: require_model_routing=true on the MCP orchestrator-state validator accepts a routing receipt whose model contradicts ModelRouting.psm1; it checks presence, not correctness, and the authoritative Python validator is absent from TaskMaster
metadata:
  type: project
---

`mcp__drm-copilot__validate_orchestration_artifacts` with `artifact_type: orchestrator-state` and
`require_model_routing: true` is **not an oracle for per-receipt model correctness** in this repo.

Observed 2026-09-17 (item 900) with a deliberate negative control: a copy of a passing checkpoint was
written with the `atomic-executor` / `C3` / `preferred` receipt's `table_model` and `model` both
flipped to `fable`. `Resolve-DelegationModel -Agent atomic-executor -Band C3 -FablePolicy preferred`
returns `opus` (atomic-executor is outside `PREFERRED_OVERLAY_AGENTS`), so the receipt was
provably wrong. The validator returned `ok:true` for it.

This matches `.claude/rules/orchestrator-state.md`: "The MCP TypeScript surface performs the existence
check only (delegated-agent set subset of routing-receipt-agent set); the Python validator remains
authoritative for per-receipt correctness." The authoritative implementations
`scripts/dev_tools/validate_orchestrator_state.py`, `compute_complexity_floor.py` and
`resolve_delegation_model.py` **do not exist in TaskMaster** — only the portable PowerShell module
`.claude/lib/model-routing/ModelRouting.psm1` does.

**Why:** a passing `model_routing_preflight` therefore proves only that *some* receipt exists naming
the target agent. Recording that pass as evidence of a correct routing decision is the
gate-passes-for-unrelated-reasons trap.

**How to apply:** derive the receipt from the resolver directly before writing it, and run a control
call in the same batch so you can see the function discriminate:

- `Get-ComplexityFloor -SignalsPresent @('concurrency_or_ordering')` returns `C3`;
  `-SignalsPresent @()` returns `C1` (control).
- `Resolve-DelegationModel -Agent <a> -Band <b> -FablePolicy <p>` — note the parameter is **`-Band`**,
  not `-ComplexityBand`; the latter throws "A parameter cannot be found".
- Under `preferred`, C3 resolves to `fable` for atomic-planner and feature-review, and to `opus` for
  atomic-executor and pr-author. If every agent you test returns the same model, your control failed.

Record the resolver output in the checkpoint as the real evidence, and record the preflight pass with
its limitation stated. Also note the seeded checkpoint may be missing eleven required keys
(`change_budget_estimate`, `short-name`, `relativeFile`, `work-mode`, `step5_status`..`step10_status`,
`blocked_reason`) — the validator names them all in one pass, so fix them in a single round. See
[[orchestrator-state-flat-keys-and-enum]], [[model-routing-feature-review-is-always-fable]],
[[model-routing-hook-reads-canonical-path-only]].
