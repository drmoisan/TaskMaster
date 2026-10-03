# P0-T2 anchors (issue #973)

Timestamp: 2026-10-03T10-41
Command: git -C <execution-worktree-root> rev-parse --show-toplevel --abbrev-ref HEAD; git -C <execution-worktree-root> rev-parse HEAD; git -C <execution-worktree-root> merge-base HEAD origin/main; git -C <execution-worktree-root> rev-parse origin/main; git -C <execution-worktree-root> diff --name-only 993fdd01566dee82e5f37acb761a600feaaa1454 HEAD; git -C <execution-worktree-root> status --porcelain --untracked-files=all
EXIT_CODE: 0
Output Summary: worktree matches the launch path; branch correct; merge-base equals the base SHA; start porcelain empty; PLAN-START-HEAD is the P0-T1 interim evidence commit (a6915d62f), which sits after the orchestrator preparation commit 27adc0f26.

WORKTREE-MATCH: True
WORKTREE-LEAF: agent-a24d410b914bcefd7
BRANCH: bug/remaining-stale-binding-redirect-pairs-973
PLAN-START-HEAD: a6915d62fe9d85218e5453fc5ac5cd5674b04984
MERGE-BASE: 993fdd01566dee82e5f37acb761a600feaaa1454
ORIGIN-MAIN: f8ea1b5dcc6514bc0088bc80965c188bfd717557

INHERITED:
.claude/agent-memory/atomic-executor/project_pester5_result_shape_container_tests_and_ci_codecoverage.md
.claude/agent-memory/atomic-executor/project_pester5_string_compare_message_shape.md
.claude/agent-memory/atomic-planner/MEMORY.md
.claude/agent-memory/atomic-planner/index_legacy_preflight_seams_pre_900.md
.claude/agent-memory/atomic-planner/project_973_r0_range_guard_vacuous_before_and_coverage_tolerance_seams.md
.claude/agent-memory/atomic-planner/project_973_r1_current_census_union_string_should_message_and_self_hit_pattern_seams.md
.claude/agent-memory/atomic-planner/project_973_r2_log_entry_self_hit_pester_unroll_and_figure_carrying_artifact_seams.md
.claude/agent-memory/atomic-planner/project_973_r3_scope_fold_scripted_region_move_and_renumbered_tail_seams.md
.claude/agent-memory/atomic-planner/project_973_r4_member_multiset_formatter_blank_lines_and_optional_trailer_seams.md
.claude/agent-memory/atomic-planner/project_973_r5_positional_restore_rule_and_enumeration_self_hit_recurrence_seams.md
.claude/agent-memory/atomic-planner/project_973_r6_fold_governing_clarification_into_dictated_sentence_seam.md
.claude/agent-memory/orchestrator/MEMORY.md
.claude/agent-memory/orchestrator/index_overflow_legacy_entries.md
.claude/agent-memory/orchestrator/isolated-session-clock-and-composed-research-timestamps.md
.claude/agent-memory/prd-feature/MEMORY.md
.claude/agent-memory/prd-feature/feedback_ac_gates_verify_satisfiability.md
.claude/agent-memory/prd-feature/feedback_negative_control_must_isolate_the_code_fix.md
.claude/agent-memory/prd-feature/feedback_scope_amendment_narrow_exclusions_and_log.md
.claude/agent-memory/task-researcher/MEMORY.md
.claude/agent-memory/task-researcher/project_binding_redirect_drift_and_missing_asyncenumerable_973.md
.claude/agent-memory/task-researcher/project_graph_usings_removable_and_models_name_collisions_973.md
.claude/agent-memory/task-researcher/project_ixnet_v7_lib_ref_clash_packages_config.md
.claude/agent-memory/task-researcher/reference_nuget_flatcontainer_nuspec_webfetch.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/baseline/phase0-instructions-read.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/issue-updates/issue-973.2026-10-03T10-31.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/other/preflight-clearance.2026-10-03T00-41.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/other/preflight-clearance.2026-10-03T10-30.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/issue.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/plan.2026-10-02T22-16.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/research/2026-10-02T22-35-stale-binding-redirect-pairs-research.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/research/2026-10-02T22-53-system-linq-asyncenumerable-install-research.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/research/2026-10-03T00-41-graph-usings-and-claude-md-bullet-research.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/runbooks/verify-designer-and-addin-load.runbook.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/spec.md
docs/features/potential/promoted/2026-10-02-remaining-stale-binding-redirect-pairs.md

START-PORCELAIN: (empty)

Acceptance: MERGE-BASE equals 993fdd01566dee82e5f37acb761a600feaaa1454 (met); WORKTREE-MATCH True (met); BRANCH matches (met); START-PORCELAIN carries no path outside .claude/agent-memory/ or the feature folder (met, empty).
