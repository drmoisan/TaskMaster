# Parallel Kickoff: bugs-2026-09-06

Planned by parallel-planner on 2026-09-07T04:40:00Z. All items are prepared: promoted, active folders
created, research complete, spec and user-story written, atomic plans approved, preflight ALL CLEAR,
blast radii declared and V1/V2-clear. Planning state:
artifacts/orchestration/parallel-planner-state.json (run branch: parallel/bugs-2026-09-06-plan).

## Invocation Prompt

Run `/parallel-run bugs-2026-09-06` to execute this run, or paste the prompt below.

Use the parallel-orchestrator subagent to execute the prepared run whose manifest is
docs/features/parallel/bugs-2026-09-06/parallel.md on the plan-home branch
parallel/bugs-2026-09-06-plan. Each item resumes at atomic execution from its committed plan-path on
its own pushed feature branch rather than re-planning, and each item opens its own pull request
against main.

## Item Summary

| issue_num | feature_folder | cohort | complexity | branch | plan-path |
| --- | --- | --- | --- | --- | --- |
| 798 | docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798 | 0 | C3 | bug/quickfiler-crash-column-add-timeout-swallowed-keynotfound-798 | docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/plan.2026-09-06T22-00.md |
| 799 | docs/features/active/2026-09-06-breadcrumb-lineage-below-archive-root-and-suggestion-path-consistency-799 | 1 | C3 | bug/breadcrumb-lineage-below-archive-root-799 | docs/features/active/2026-09-06-breadcrumb-lineage-below-archive-root-and-suggestion-path-consistency-799/plan.2026-09-06T22-01.md |
| 796 | docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796 | 2 | C3 | bug/quickfiler-folder-dropdown-closes-on-open-796 | docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/plan.2026-09-06T21-59.md |
| 797 | docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797 | 2 | C3 | bug/folder-settings-never-persist-797 | docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/plan.2026-09-06T22-00.md |

## Execution Notes

Cohorts 0 and 1 hold one item each and run alone. Cohort 2 holds items 796 and 797, which share no
path and no module and may run concurrently. The configured max_concurrency of 4 is therefore never
binding: the widest cohort is 2.

Every conflict edge in this run rests on a project-file overlap plus the corresponding module
overlap. Each item adds at least one new C# file, and every project in this repository is
non-SDK-style with an explicit compile-entry list and no wildcard globbing, so each item must edit
the project file of every assembly it adds a file to. That contention is genuine.

Each item branch was cut from origin/main at c431dc32 and is therefore based on a main tip that will
have advanced by execution time. Each execution child should merge origin/main before starting.

None of the four preparation worktrees was bootstrapped: the repository-local .NET SDK, the
dotnet-coverage global tool and the packages tree are absent, and neither msbuild nor vstest is on
PATH. Every plan handles this in its own Phase 0, but a fresh execution worktree needs the same
bootstrap before any toolchain gate can run.

## Integrity

planning_commit: 7e971dc553e3b210fd98101844916029e52a8ba7

| plan-path | plan-hash |
| --- | --- |
| docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/plan.2026-09-06T22-00.md | 0054173631de55c939e3c914fe390ff9b9140ff9 |
| docs/features/active/2026-09-06-breadcrumb-lineage-below-archive-root-and-suggestion-path-consistency-799/plan.2026-09-06T22-01.md | c16d60ea943c321aa5225325397ce24e7a4373fb |
| docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/plan.2026-09-06T21-59.md | 39ce1d452f754a2878759ba35187868054099911 |
| docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/plan.2026-09-06T22-00.md | e4a9ddc4f7bab8c5b482f0498a258fca3bc7491c |
