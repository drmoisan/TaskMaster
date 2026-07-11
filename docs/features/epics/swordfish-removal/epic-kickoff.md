# Epic Kickoff: swordfish-removal

Planned by epic-planner on 2026-07-10T22:05:00Z. All child features are prepared: issues
promoted, active folders created, research complete, spec/user-story written, atomic plans
approved, preflight ALL CLEAR. Planning state:
artifacts/orchestration/epic-planner-state.json (branch: epic/swordfish-removal-integration).

## Invocation Prompt

Run `/epic-run swordfish-removal` to execute this epic, or paste the prompt below.

Use the epic-orchestrator subagent to execute the prepared epic at
docs/features/epics/swordfish-removal/epic.md. The integration branch
epic/swordfish-removal-integration already contains every prepared feature folder and approved
atomic plan; child features resume at atomic execution from their committed plan-path rather
than re-planning. Execute per the epic-orchestrate skill: wave-scheduled child orchestrator runs
in isolated worktrees, merge-on-green fan-in to the integration branch, and the final
integration-to-main PR.

## Feature Summary

| issue_num | feature_folder | wave | complexity | plan-path |
| --- | --- | --- | --- | --- |
| 306 | docs/features/active/2026-07-10-swordfish-dictionary-lineage-306 | 0 | C3 | docs/features/active/2026-07-10-swordfish-dictionary-lineage-306/plan.2026-07-10T20-14.md |
| 307 | docs/features/active/2026-07-10-swordfish-collection-stack-lineage-307 | 0 | C3 | docs/features/active/2026-07-10-swordfish-collection-stack-lineage-307/plan.2026-07-10T20-14.md |
| 309 | docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309 | 0 | C1 | docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/plan.2026-07-10T20-17.md |
| 310 | docs/features/active/2026-07-10-swordfish-raw-usage-cleanup-310 | 0 | C2 | docs/features/active/2026-07-10-swordfish-raw-usage-cleanup-310/plan.2026-07-10T20-20.md |
| 308 | docs/features/active/2026-07-10-swordfish-interface-project-teardown-308 | 1 | C3 | docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/plan.2026-07-10T20-16.md |

## Execution Notes for epic-orchestrator

- Dependency graph: #308 (F5, wave 1) depends on #306, #307, #309, #310 (wave 0). Wave-0
  children carry no ordering constraints among themselves and run in parallel worktrees.
- F5's plan opens with a WI-0 HALT gate asserting F1–F4 have landed on the integration branch
  before teardown begins; do not schedule F5 until all four wave-0 children have fanned in.
- The epic brief's original "clean `ConcurrentObservableCollection` already exists" premise was
  verified FALSE during preparation; F2's plan (Phase 1) CREATES the clean base. The manifest's
  Shared Design section carries the correction and evidence pointer.
- All five preflights returned `PREFLIGHT: ALL CLEAR` in preparation worktrees whose absolute
  paths no longer exist; per stored orchestrator feedback, epic-child plan Phase 0 policy-read
  paths are stale — redirect each executor to the CURRENT worktree's files in the delegation
  prompt.
- Preserved on-disk JSON compatibility is a cross-cutting acceptance criterion; research
  verified all audited persisted payloads are flat/bare-array (no `$type` embedding), with
  round-trip fixtures planned per persisted type.
