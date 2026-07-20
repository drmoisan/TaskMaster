---
name: model-routing-scripts-absent-on-epic-integration-base
description: Model-selection reference scripts may be absent on an epic integration branch based on an un-merged PR head; compute routing by hand, MCP validator still works
metadata:
  type: project
---

On the `epic/utilitiescs-nullable-remediation-integration` branch (based on PR #361 head `20d163ac` = origin/main + one gate-repair commit, NOT origin/main), the model-selection reference scripts do not exist in the checkout: `scripts/dev_tools/compute_complexity_floor.py`, `scripts/dev_tools/resolve_delegation_model.py`, and `scripts/dev_tools/validate_orchestrator_state.py` are all absent (grep/find return nothing).

**How to apply:** When preparing an epic child whose integration branch is based on an un-merged PR head, compute the model-routing values by hand from the documented rules in `.claude/rules/orchestrator-state.md` and `config/orchestration-routing.json`, rather than shelling to the reference scripts. For a `cross_module_contract_change` signal (floor=true) → floor C3; under `fable_policy: available` the base `complexity_to_model` table applies as-is so C3 → opus (no clamp) for every delegated agent. The MCP tool `mcp__drm-copilot__validate_orchestration_artifacts` (artifact_type orchestrator-state, require_model_routing=true; and artifact_type plan) IS available to the orchestrator and remains the authoritative gate — it validated both the checkpoint and the em-dash+LF plan without the local scripts present. See [[mcp-tools-available-to-orchestrator]] and [[parallel-preparation-children-shared-worktree]].
