# Parallel Planner Memory Index

- [Bug corpus is 71% QuickFiler](project_bug_corpus_is_quickfiler_concentrated.md) — measure the module histogram before fanning out; "all bugs" is 51 cohorts for 72 items, so max_concurrency is inert
- [Parallel surface status + spurious-contention defects](project_parallel_surface_partial_port.md) — #545 fixed, but 3 config/extractor defects make large runs effectively serial (83.3% density, mean cohort width 1.45)
- [TaskMaster lacks extensions/ and scripts/dev_tools/](reference_taskmaster_lacks_python_toolchain.md) — drm-copilot MCP bugs aren't fixable here; use the PS/bash ports, never `poetry run python -m scripts.dev_tools`
- [Blast-radius extractor mechanics](reference_blast_radius_extractor_mechanics.md) — Get-PlanPaths only sees backtick-delimited paths; bare prose paths silently fail OPEN
- [Parallel surface cannot express ordering](project_parallel_cannot_express_ordering.md) — depends_on/wave are prohibited; lane-sequential or flight-ordered work belongs to /epic-plan
- [drm-copilot is the governance upstream](reference_drm_copilot_upstream.md) — check it when a rule/config/library a skill references is missing locally
- [Planner git commits must be single bare segments](feedback_planner_git_commits_must_be_single_bare_segments.md) — no `cd`, no angle brackets, no dedicated worktree; checkout the plan branch in the session worktree
- [Parallel artifact authoring gotchas](reference_parallel_artifact_authoring_gotchas.md) — quote ISO timestamps, no prose in kickoff table sections, validator path is workspace-relative
- [Worktree lock pid is the session, not the subagent](reference_worktree_lock_pid_is_the_session_not_the_subagent.md) — a live pid on a locked worktree proves nothing; walk your own ancestry first
- [Default to open mode; expect mid-flight knob changes](feedback_default_to_open_mode_for_parallel_runs.md) — operator wants /parallel-add to stay available; a max_concurrency raise can be honoured immediately
- [Never backtick exclusion paths in delegation prompts](feedback_never_backtick_exclusion_paths_in_delegation_prompts.md) — children echo them into plans, the extractor reads them as write claims, and V1/V2 cannot see it
- [Unchanged ref does not prove a dead child](feedback_unchanged_ref_does_not_prove_a_dead_child.md) — after an interruption, check liveness separately before relaunching; always require fast-forward-only pushes
