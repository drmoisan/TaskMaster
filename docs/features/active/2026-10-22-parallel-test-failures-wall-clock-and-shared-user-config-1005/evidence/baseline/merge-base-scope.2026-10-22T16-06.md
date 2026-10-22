# Merge-Base Scope (Issue #1005)

Timestamp: 2026-10-22T16-06
Command: git fetch origin main; git merge-base origin/main HEAD; git diff --name-status <merge-base> HEAD; git status --porcelain
EXIT_CODE: 0
Output Summary: MERGE-BASE = 346651242a2bd6ed25d703a4b1ecb5a055c6ae72. The branch diff holds 15 paths, all documentation (`.md`) under `.claude/agent-memory/`, the feature folder, and `docs/features/potential/promoted/`. No path ends in `.cs`, `.csproj`, or `.runsettings` in either listing. Decision D-F holds: the baseline measures the merge-base code.

## git diff --name-status <merge-base> HEAD

```
M	.claude/agent-memory/atomic-planner/MEMORY.md
A	.claude/agent-memory/atomic-planner/project_1005_r0_settings_store_seam_and_probe_seams.md
A	.claude/agent-memory/atomic-planner/project_1005_r1_jacoco_projection_is_package_level_only.md
M	.claude/agent-memory/prd-feature/feedback_ac_gates_verify_satisfiability.md
M	.claude/agent-memory/task-researcher/MEMORY.md
A	.claude/agent-memory/task-researcher/project_settings_save_shared_user_config_1005.md
A	docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/evidence/other/preflight-fixes.2026-10-22T15-47.md
A	docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/evidence/other/preflight-fixes.2026-10-22T16-01.md
A	docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/issue.md
A	docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/plan.2026-10-22T11-02.md
A	docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/research/2026-10-22T15-21-parallel-test-isolation-wall-clock-and-user-config-research.md
A	docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/spec.md
A	docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/user-story.md
A	docs/features/potential/promoted/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config.md
A	docs/features/potential/promoted/2026-10-22-tests-mutate-shared-settings-default-in-memory.md
```

## git status --porcelain

```
 M docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/plan.2026-10-22T11-02.md
?? docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/evidence/baseline/
```

Result: PASS. No `.cs`, `.csproj`, or `.runsettings` path appears.
