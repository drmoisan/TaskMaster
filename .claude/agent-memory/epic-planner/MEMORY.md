# Epic-Planner Memory Index

- [epic-planner-state required fields](feedback_epic_planner_state_required_fields.md) — checkpoint validator also requires max_parallel_features + per-feature research_path, and exact enums verdict=epic|non_epic, next_step=NON_EPIC_RECOMMENDED
- [Check in-flight branches before decomposing](feedback_check_inflight_branches_before_decomposition.md) — bugs spawned by another feature's work often collide with that unmerged branch; diff candidate files against every local branch before planning waves
- [epic-plan tooling not vendored](reference_epic_plan_tooling_not_vendored.md) — epic_wave_computation.py + epic-manifest validator absent in this repo; verify DAG/waves manually, no `plan`-type validation of epic.md
- [Concurrent prep children need worktree isolation](feedback_concurrent_prep_children_worktree_isolation.md) — each concurrent prep child gets isolation:worktree + child-scoped orchestrator-state.<slug>.json or siblings overwrite the canonical checkpoint
- [Force-remove prep-child worktree](feedback_prep_child_worktree_force_remove.md) — worktree remove may block on uncommitted orchestrator-agent-memory scratch; force-remove is safe once HEAD == merged tip and only .claude/agent-memory/orchestrator/** is dirty
- [epic-planner-state require_ready_for_execution mode](reference_epic_planner_state_ready_for_execution_mode.md) — hardened contract wants execution-time launch/topology/model-routing receipts epic-planner doesn't produce; validate with DEFAULT call, don't fabricate; run against integration worktree root
