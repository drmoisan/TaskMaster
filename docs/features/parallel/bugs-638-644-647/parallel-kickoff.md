# Parallel Kickoff: bugs-638-644-647

Planned by parallel-planner on 2026-08-29T15:52:00Z. All items are prepared: promoted, active
folders created, research complete, spec written, atomic plans approved, preflight ALL CLEAR, blast
radii declared and V1/V2-clear. All three items carry work mode full-bug, so spec.md is the
acceptance-criteria source and no user-story.md is expected or present. Planning state:
artifacts/orchestration/parallel-planner-state.json (run branch: parallel/bugs-638-644-647-plan).

Every unordered pair of these items conflicts, so the generation-0 cohort table is three singleton
cohorts and the run executes serially regardless of max_concurrency.

## Invocation Prompt

Run `/parallel-run bugs-638-644-647` to execute this run, or paste the prompt below.

Use the parallel-orchestrator subagent to execute the prepared run whose manifest is
docs/features/parallel/bugs-638-644-647/parallel.md on the plan-home branch
parallel/bugs-638-644-647-plan. Each item resumes at atomic execution from its committed plan-path
on its own pushed feature branch rather than re-planning, and each item opens its own pull request
against main.

## Item Summary

| issue_num | feature_folder | cohort | complexity | branch | plan-path |
| --- | --- | --- | --- | --- | --- |
| 638 | docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638 | 0 | C3 | bug/efc-unguarded-archive-root-read-crashes-ui-thread-638 | docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/plan.2026-08-29T07-41.md |
| 644 | docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644 | 1 | C3 | bug/qfc-unregister-navigation-count-mismatch-orphan-644 | docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/plan.2026-08-29T07-42.md |
| 647 | docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647 | 2 | C3 | bug/fileio2-write-retry-reports-success-on-final-failure-647 | docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647/plan.2026-08-29T07-48.md |

## Integrity

planning_commit: a0402aa506f85c8ace06d8faaa9cfb0fd4365296

| plan-path | plan-hash |
| --- | --- |
| docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/plan.2026-08-29T07-41.md | 0d897262ce19b4ea9adc8c288eac886cab135404 |
| docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/plan.2026-08-29T07-42.md | bde39151a5a665ae690bae7f5da3f78b921b3810 |
| docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647/plan.2026-08-29T07-48.md | cccd1a0435a2e0d0b791645a426f5a0a7cb1369a |
