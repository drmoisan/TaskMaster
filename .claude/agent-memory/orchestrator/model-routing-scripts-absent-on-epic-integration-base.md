---
name: model-routing-scripts-absent-on-epic-integration-base
description: The python model-selection scripts may be absent, but portable PowerShell equivalents exist at .claude/lib/model-routing/ModelRouting.psm1 — run those instead of computing by hand
metadata:
  type: project
---

On the `epic/utilitiescs-nullable-remediation-integration` branch (based on PR #361 head `20d163ac` = origin/main + one gate-repair commit, NOT origin/main), the model-selection reference scripts do not exist in the checkout: `scripts/dev_tools/compute_complexity_floor.py`, `scripts/dev_tools/resolve_delegation_model.py`, and `scripts/dev_tools/validate_orchestrator_state.py` are all absent (grep/find return nothing).

**CORRECTION (2026-08-26, epic child 468 on `epic/quickfiler-bug-family-integration`).** Do NOT compute by hand first. `scripts/dev_tools/` was again absent on that base, but portable PowerShell equivalents of the same reference formulas DO exist in the checkout and are the better move:

- `.claude/lib/model-routing/ModelRouting.psm1` exports `Get-ComplexityFloor -SignalsPresent @(...)` and `Resolve-DelegationModel -Agent <a> -Band <C1..C4> -FablePolicy <policy>`. The parameter is **`-Band`, not `-ComplexityBand`** — the obvious spelling throws "A parameter cannot be found".
- `.claude/lib/orchestrator-state/OrchestratorState.psm1` exports `Invoke-OrchestratorStatePreflight -CheckpointPath ...`, and the `Get-OrchestratorState*Error` validators are what the hooks actually run.

Verified values from those modules: `Get-ComplexityFloor @()` → `C1`; under `fable_policy: preferred`, C3 resolves to `opus` for `atomic-executor` and `pr-author`, and to `fable` for `feature-review`, `atomic-planner`, `prd-feature`, `task-researcher` (the `preferred_overlay` agent list). Running the module is cheap and removes the risk that a hand-computed `floor` fails the validator's floor-equality check.

**How to apply:** when `scripts/dev_tools/` is missing, import the two `.claude/lib/` modules and call them; compute by hand only if those are missing too. When preparing an epic child whose integration branch is based on an un-merged PR head, compute the model-routing values by hand from the documented rules in `.claude/rules/orchestrator-state.md` and `config/orchestration-routing.json`, rather than shelling to the reference scripts. For a `cross_module_contract_change` signal (floor=true) → floor C3; under `fable_policy: available` the base `complexity_to_model` table applies as-is so C3 → opus (no clamp) for every delegated agent. The MCP tool `mcp__drm-copilot__validate_orchestration_artifacts` (artifact_type orchestrator-state, require_model_routing=true; and artifact_type plan) IS available to the orchestrator and remains the authoritative gate — it validated both the checkpoint and the em-dash+LF plan without the local scripts present. See [[mcp-tools-available-to-orchestrator]] and [[parallel-preparation-children-shared-worktree]].
