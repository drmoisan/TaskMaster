# P4-T11 Footprint and Scope Boundary (AC3, AC15, AC16 diff clause)

Timestamp: 2026-09-29T09-44
Command: (one pwsh payload, MERGE-BASE = 177b6d78e1b2408e5aedbd794cef3aad6b7fb372) git diff --name-only MERGE-BASE; git status --porcelain --untracked-files=all; git diff --name-only MERGE-BASE -- QuickFiler.Test; git diff --name-only MERGE-BASE -- .claude; git diff --name-only MERGE-BASE..HEAD -- .claude config; git log --oneline --diff-filter=A MERGE-BASE..HEAD -- docs\features\potential; git diff --exit-code MERGE-BASE -- QuickFiler UtilitiesCS TaskMaster.runsettings scripts\vscode\TaskMaster.cli.runsettings config; CMD-ADDED-LINES (git diff -U0 MERGE-BASE -- QuickFiler.Test UtilitiesCS.Test); Get-FileHash -Algorithm SHA256 TaskMaster.runsettings
EXIT_CODE: 0 (scoped to the git diff --exit-code span over the production and configuration paths)

Output Summary:

## git diff --name-only MERGE-BASE (working tree), verbatim

    .claude/agent-memory/atomic-executor/MEMORY.md
    .claude/agent-memory/atomic-planner/MEMORY.md
    .claude/agent-memory/atomic-planner/project_planner_mcp_validator_not_in_tool_surface.md
    .claude/agent-memory/orchestrator/MEMORY.md
    .claude/agent-memory/prd-feature/MEMORY.md
    .claude/agent-memory/task-researcher/MEMORY.md
    QuickFiler.Test/QuickFiler.Test.csproj
    QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs
    QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs
    QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs
    QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs
    UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/baseline/coverage-baseline.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/baseline/p0-t12-pre-edit-census.2026-09-29T09-05.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/baseline/p0-t13-phase0-commit.2026-09-29T09-07.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/baseline/p0-t2-mode-preconditions.2026-09-29T08-54.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/baseline/p0-t3-worktree-context.2026-09-29T08-55.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/baseline/p0-t4-channel-and-toolchain.2026-09-29T08-56.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/baseline/p0-t5-nuget-restore.2026-09-29T08-56.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/baseline/p0-t6-csharpier-check.2026-09-29T08-56.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/baseline/p0-t7-msbuild-analyzers.2026-09-29T08-59.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/baseline/p0-t8-msbuild-nullable.2026-09-29T08-59.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/baseline/p0-t9-stall-probe.2026-09-29T09-00.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/baseline/phase0-instructions-read.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/baseline/test-run-baseline.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/fail-before-exception.2026-09-29T09-07.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/mutation-inline-precondition.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/mutation-null-owner-escape.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/mutation-openread-sentinel.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/mutation-owner-only-dispatcher-guard.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/p2-t1-helper-census.2026-09-29T09-10.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/p2-t10-fix-commit.2026-09-29T09-21.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/p2-t2-csproj-census.2026-09-29T09-11.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/p2-t3-part2-census.2026-09-29T09-12.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/p2-t4-primary-census.2026-09-29T09-13.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/p2-t5-boundary-census.2026-09-29T09-14.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/p2-t6-fileinfowrapper-census.2026-09-29T09-15.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/p2-t7-csharpier-scoped.2026-09-29T09-16.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/p2-t8-build-after-fix.2026-09-29T09-18.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/p2-t9-pass-before-controls.2026-09-29T09-20.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/p3-t9-post-control-clean-tree.2026-09-29T09-30.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/issue.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/plan.2026-09-28T20-01.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/research/2026-09-28T20-15-tests-depend-on-uncontrolled-environment-research.md
    docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/spec.md
    docs/features/potential/promoted/2026-09-28-tests-depend-on-uncontrolled-environment.md

(git also printed one line-ending warning for .claude/agent-memory/task-researcher/MEMORY.md; not a path entry.)

## git status --porcelain --untracked-files=all, verbatim

     M .claude/agent-memory/atomic-executor/MEMORY.md
     M .claude/agent-memory/atomic-planner/MEMORY.md
     M .claude/agent-memory/atomic-planner/project_planner_mcp_validator_not_in_tool_surface.md
     M .claude/agent-memory/orchestrator/MEMORY.md
     M .claude/agent-memory/prd-feature/MEMORY.md
     M .claude/agent-memory/task-researcher/MEMORY.md
     M docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/plan.2026-09-28T20-01.md
    ?? .claude/agent-memory/atomic-executor/index_artifact_hygiene_and_misc.md
    ?? .claude/agent-memory/atomic-executor/project_loop_iteration_vs_single_artifact_glob_and_hash_anchor.md
    ?? .claude/agent-memory/atomic-executor/project_outcome_branch_sets_keyed_on_route_miss_derived_flag_cases.md
    ?? .claude/agent-memory/atomic-executor/project_pinned_target_source_prose_carries_census_tokens.md
    ?? .claude/agent-memory/atomic-planner/project_931_uncontrolled_environment_tests_plan_seams.md
    ?? .claude/agent-memory/orchestrator/delegation-prompt-needs-canonical-issue-and-branch-lines.md
    ?? .claude/agent-memory/prd-feature/reference_ifileinfo_seam_filestream_sentinel_not_memorystream.md
    ?? .claude/agent-memory/task-researcher/project_taskrun_triage_931.md
    ?? docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/qa-gates/coverage-final.md
    ?? docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/qa-gates/p4-t1-csharpier-format.2026-09-29T09-33.md
    ?? docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/qa-gates/p4-t2-csharpier-check.2026-09-29T09-34.md
    ?? docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/qa-gates/p4-t3-msbuild-analyzers.2026-09-29T09-34.md
    ?? docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/qa-gates/p4-t4-msbuild-nullable.2026-09-29T09-35.md
    ?? docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/qa-gates/p4-t9-post-format-census.2026-09-29T09-42.md
    ?? docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/qa-gates/toolchain-pass.md
    ?? docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/parallel-suite-quickfiler-test.md
    ?? docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/parallel-suite-utilitiescs-test.md

The porcelain span is the name-listing diff's companion (it lists the untracked Phase 4 artifacts the anchored diff cannot see).

## Derived sets

INHERITED-AND-EXCLUDED:
- Clause A (INHERITED-CLAUSE-A of P0-T3): docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/issue.md; docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/plan.2026-09-28T20-01.md; docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/research/2026-09-28T20-15-tests-depend-on-uncontrolled-environment-research.md; docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/spec.md; docs/features/potential/promoted/2026-09-28-tests-depend-on-uncontrolled-environment.md
- Clause B (prefix .claude/agent-memory/): the six modified and eight untracked .claude/agent-memory/ paths listed above
- No Write Set path was subtracted.

THIS-ITEM-FOOTPRINT:
- QuickFiler.Test/QuickFiler.Test.csproj
- QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs
- QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs
- QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs
- QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs
- UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs
- every docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/ path listed in the two spans above (baseline, regression-testing and qa-gates artifacts of this run)
- nothing else

QUICKFILER-TEST-CHANGED:
- QuickFiler.Test/QuickFiler.Test.csproj
- QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs
- QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs
- QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs
- QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs

CLAUDE-CHANGED:
- .claude/agent-memory/atomic-executor/MEMORY.md
- .claude/agent-memory/atomic-planner/MEMORY.md
- .claude/agent-memory/atomic-planner/project_planner_mcp_validator_not_in_tool_surface.md
- .claude/agent-memory/orchestrator/MEMORY.md
- .claude/agent-memory/prd-feature/MEMORY.md
- .claude/agent-memory/task-researcher/MEMORY.md

COMMITTED-CLAUDE-OR-CONFIG: NONE

INHERITED-PROMOTION-RECORD: e13757267 docs(bug-931): promote tests-depend-on-uncontrolled-environment into an active full-bug feature folder
(the promoted record docs/features/potential/promoted/2026-09-28-tests-depend-on-uncontrolled-environment.md was committed on this branch deliberately before execution, lies in INHERITED-CLAUSE-A because it is in the committed MERGE-BASE...HEAD diff, and is subtracted from THIS-ITEM-FOOTPRINT under Clause A)

PRODUCTION-AND-CONFIG-DIFF-EXIT: 0

## CMD-ADDED-LINES

ADDED-LINE-COUNT: 306
ADDED-DONOTPARALLELIZE: 0
ADDED-THREAD-SLEEP: 0
ADDED-TASK-DELAY: 0
ADDED-TIMEOUT: 0
ADDED-RETRY: 0
ADDED-WORKERS: 0
ADDED-SCOPE: 0

RUNSETTINGS-HASH-NOW: 199408CA53CE4E12AE1A894FC66A0926124F3AC0D6447BD93B0C121338297FFA (equals RUNSETTINGS-HASH from P0-T4)

Acceptance: THIS-ITEM-FOOTPRINT is exactly the six Write Set code paths plus paths under the feature folder; QUICKFILER-TEST-CHANGED is exactly the four QuickFiler.Test Write Set .cs paths plus the project file (the five QuickFiler.Test Write Set entries, so none of the eight files AC3 names and neither DoNotParallelize carrier changed); every CLAUDE-CHANGED path begins .claude/agent-memory/; COMMITTED-CLAUDE-OR-CONFIG NONE; INHERITED-PROMOTION-RECORD recorded; PRODUCTION-AND-CONFIG-DIFF-EXIT 0; the seven ADDED- counts are 0 over ADDED-LINE-COUNT 306; RUNSETTINGS-HASH-NOW equals RUNSETTINGS-HASH; the porcelain span is present. All nine hold.

Note on the count wording: the plan's acceptance text for QUICKFILER-TEST-CHANGED says "the four QuickFiler.Test Write Set paths", while the plan's Write Set lists five QuickFiler.Test entries (four .cs files and QuickFiler.Test/QuickFiler.Test.csproj). The observed list equals those five Write Set entries exactly and contains no other path.
