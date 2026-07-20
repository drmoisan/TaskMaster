---
name: epic-planner-state-ready-for-execution-mode
description: validate_orchestration_artifacts epic-planner-state has a hardened require_ready_for_execution mode with a different schema than the shape this repo's epic-planner runs actually produce
metadata:
  type: reference
---

The `mcp__drm-copilot__validate_orchestration_artifacts` tool with `artifact_type: "epic-planner-state"` has two contracts:

- **Default (no flag):** the operative bar this repo's epic-planner runs validate against. Requires top-level `max_parallel_features` (int 1-8) and per-feature `research_path` (see [[epic-planner-state-required-fields]]). Passes on the checkpoint shape using `preparation_status: "complete"`, `preflight_status: "clear"`, `next_step: "complete"`.

- **`require_ready_for_execution: true`:** a hardened/execution-ready contract that the preparation-only epic-planner checkpoint does NOT satisfy. It demands: `next_step == "EPIC_EXECUTION_READY"`; per-feature `preparation_status == "prepared"` and `preflight_status == "PREFLIGHT: ALL CLEAR"` (exact strings, not "complete"/"clear"); per-feature `model_routing_receipt` and `topology_receipt` objects; per-feature launch-binding objects (`branch_name`, `worktree_path`, `launch_receipt_path`/`launch_status_path` under `artifacts/orchestration/epic-child-launches/`); a top-level `topology_receipt`; and `kickoff_prompt_path` as a plain string equal to `artifacts/orchestration/epic-kickoff-<slug>.md`. It also does filesystem existence checks for each `docs/features/active/<slug>` folder and the committed `epic-kickoff.md`, plus `PREFLIGHT: ALL CLEAR` text under each feature folder — checked against the passed `workspace_root`.

**Why:** those launch/topology/model-routing receipts and `epic-child-launches/` files are execution-time constructs epic-orchestrator produces, not epic-planner. A preparation-only planning run legitimately lacks them; do not fabricate them to pass the flag.

**How to apply:** validate epic-planner-state with the DEFAULT call at planning completion; treat that pass plus direct on-disk verification (13 feature folders with issue/spec/user-story/plan/research, manifest cycle-free) as the completion bar. Two extra gotchas for `require_ready_for_execution`: (1) run it against the *integration worktree* root, not the session cwd — the session worktree is on its own branch and lacks the integration-branch feature folders, producing false "requires feature folder" errors; (2) committed feature folders carry no standalone `PREFLIGHT: ALL CLEAR` text because child orchestrator checkpoints are gitignored/local-only (see [[concurrent-prep-children-worktree-isolation]]) — preflight clearance lives in the planning checkpoint's per-feature `preflight_status`, so the text-evidence check fails even when preflight genuinely cleared.
