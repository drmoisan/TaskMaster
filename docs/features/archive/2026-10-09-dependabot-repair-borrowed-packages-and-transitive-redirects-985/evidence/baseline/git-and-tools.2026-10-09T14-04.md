# Git State and Tool Availability (P0-T2)

Timestamp: 2026-10-09T14-04
Command: git rev-parse --abbrev-ref HEAD; git rev-parse HEAD; git merge-base HEAD origin/main; git diff --name-only BASE-SHA HEAD; git status --porcelain; pwsh -NoProfile -File CMDDIR\985-probe.ps1 -RehearsalRoot REHEARSAL
EXIT_CODE: 0
Output Summary:
- BRANCH: bug/dependabot-repair-borrowed-packages-and-transitive-redirects-985 (equals FIX-BRANCH)
- HEAD: 9911fe138952e2b93476850582847c2831e1cbbd
- BASE-SHA: 9911fe138952e2b93476850582847c2831e1cbbd
- Committed diff BASE-SHA..HEAD: empty (no committed path outside the permitted set)
- Probe: PWSH-VERSION 7.6.6; PESTER-AVAILABLE 5.6.1; NUGET FOUND (NuGet Version: 7.6.0.59); VSWHERE True; MSBUILD FOUND; DOTNET-COVERAGE FOUND; LONG-PATHS-ENABLED 1; REHEARSAL-ROOT-LENGTH 162
- Result: all required tools present; no STOP condition.

## git diff --name-only BASE-SHA HEAD

(empty)

## PreExistingWorktreePaths:

```
 M .claude/agent-memory/atomic-executor/MEMORY.md
 M .claude/agent-memory/atomic-planner/MEMORY.md
 M .claude/agent-memory/task-researcher/MEMORY.md
?? .claude/agent-memory/atomic-executor/index_overflow_entries.md
?? .claude/agent-memory/atomic-executor/project_mstest_runner_summary_absent_on_failure_voids_flaky_carveout.md
?? .claude/agent-memory/atomic-planner/index_preflight_seams_900_to_973.md
?? .claude/agent-memory/atomic-planner/project_985_r0_rehearsal_fidelity_and_hygiene_path_seams.md
?? .claude/agent-memory/task-researcher/index_epic136_percoverage_children.md
?? .claude/agent-memory/task-researcher/index_quickfiler_efc_defects.md
?? .claude/agent-memory/task-researcher/project_dependabot_repair_workflow_run_script_source_985.md
?? docs/features/active/2026-10-09-dependabot-repair-borrowed-packages-and-transitive-redirects-985/
?? docs/features/potential/promoted/2026-10-09-dependabot-repair-borrowed-packages-and-transitive-redirects.md
```

## Probe output (985-probe.ps1)

```
PWSH-VERSION: 7.6.6
PESTER-AVAILABLE: 5.6.1
NUGET: FOUND
NUGET-VERSION: NuGet Version: 7.6.0.59
VSWHERE: True
MSBUILD: FOUND
DOTNET-COVERAGE: FOUND
LONG-PATHS-ENABLED: 1
REHEARSAL-ROOT-LENGTH: 162
```
