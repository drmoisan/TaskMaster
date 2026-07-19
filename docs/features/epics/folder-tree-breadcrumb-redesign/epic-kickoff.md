# Epic Kickoff: folder-tree-breadcrumb-redesign

Planned by epic-planner on 2026-07-17T02-39. All child features are prepared: issues promoted,
active folders created, research complete, spec/user-story written, atomic plans approved,
preflight ALL CLEAR. Planning state: artifacts/orchestration/epic-planner-state.json (branch:
epic/folder-tree-breadcrumb-redesign-integration).

## Invocation Prompt

Run `/epic-run folder-tree-breadcrumb-redesign` to execute this epic, or paste the prompt below.

Use the epic-orchestrator subagent to execute the prepared epic at
docs/features/epics/folder-tree-breadcrumb-redesign/epic.md. The integration branch
epic/folder-tree-breadcrumb-redesign-integration already contains every prepared feature folder
and approved atomic plan; child features resume at atomic execution from their committed
plan-path rather than re-planning. Execute per the epic-orchestrate skill: wave-scheduled child
orchestrator runs in isolated worktrees, merge-on-green fan-in to the integration branch, and
the final integration-to-main PR.

## Feature Summary

| issue_num | feature_folder | wave | complexity | plan-path |
| --- | --- | --- | --- | --- |
| 350 | docs/features/active/2026-07-16-folder-hierarchy-live-provider-350 | 0 | C3 | docs/features/active/2026-07-16-folder-hierarchy-live-provider-350/plan.2026-07-16T21-52.md |
| 349 | docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349 | 1 | C4 | docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/plan.2026-07-16T21-52.md |
| 351 | docs/features/active/2026-07-16-quickfiler-breadcrumb-webview2-351 | 1 | C4 | docs/features/active/2026-07-16-quickfiler-breadcrumb-webview2-351/plan.2026-07-16T21-53.md |

Execution order: wave 0 (350) merges first; on green, waves 1 features (349, 351) run in parallel
against the updated integration branch, then a final integration-to-main PR.
