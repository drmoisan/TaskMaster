# Epic Status: store-lockup-resilience (Issue #260)

- Phase: Planning (documentation only) — implementation on hold pending user approval
- Integration branch: `epic/store-lockup-resilience-integration` (not yet created; created at wave-0 launch)
- Last updated: 2026-07-07

This file is a human-readable projection of the planned epic state. During implementation it is
regenerated from the epic checkpoint (`artifacts/orchestration/epic-orchestrator-state.json`),
which is created at wave-0 launch after approval.

## Feature Status

| Feature | Issue | Wave | Depends on | Planning status | Merge status |
|---|---|---|---|---|---|
| F1 store-disable-service | #261 | 0 | — | preflight clear (docs ready) | not_started |
| F2 folder-settings-store-model-null (bug) | #262 | 0 | — | preflight clear (docs ready) | not_started |
| F3 store-runtime-reenable | #263 | 1 | F1, F2 | preflight clear (docs ready) | not_started |
| F4 store-lockup-detect-notify | #264 | 2 | F1, F3 | preflight clear (docs ready) | not_started |
| F5 disabled-stores-settings-ui | #265 | 2 | F1, F2, F3 | preflight clear (docs ready) | not_started |

Planning status advances per feature: folder created → research → spec/user-story → atomic plan
→ preflight clear → docs committed.

## Milestones

- M1 Epic promoted (#260) and 5 child issues promoted + linked — Done
- M2 epic-plan.md manifest authored — Done
- M3 Per-feature research complete — Done (all 5 research docs written; cross-feature reconciliation applied to epic-plan.md)
- M4 Per-feature spec/user-story authored — Done (F1/F3/F4/F5 spec+user-story; F2 spec only per full-bug)
- M5 Per-feature atomic plans authored + preflight clear + docs committed — Done (all 5 plans MCP-validated and PREFLIGHT: ALL CLEAR; F1/F2/F3/F4 required one revision each)
- M6 User approval to begin implementation — Not started (awaiting review)
