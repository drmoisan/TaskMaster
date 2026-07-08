# Epic Status: store-lockup-resilience (Issue #260)

> This document is a human-readable projection of the epic checkpoint
> (`artifacts/orchestration/epic-orchestrator-state.json`). It is regenerated from the
> checkpoint at each lifecycle boundary and must not be hand-edited. The checkpoint JSON is the
> durable, machine-authoritative source.

- **Integration branch:** `epic/store-lockup-resilience-integration`
- **Integration base provenance:** created off `origin/main` at `954c7840` (preferred path — planning
  docs already landed on `main` via PR #268; no deviation recorded).
- **Model budget:** `fable_policy: disabled`
- **Current wave:** 0
- **Phase:** implementation — wave 0 in progress (F1, F2 delegated)
- **Last updated:** 2026-07-08T02:52:25Z

## Feature Status

| Feature | Issue | Folder | Wave | Depends on | merge_status | PR | Merge SHA |
|---|---|---|---|---|---|---|---|
| F1 store-disable-service | #261 | `2026-07-07-store-disable-service-261` | 0 | — | worktree_created | — | — |
| F2 folder-settings-store-model-null | #262 | `2026-07-07-folder-settings-store-model-null-262` | 0 | — | worktree_created | — | — |
| F3 store-runtime-reenable | #263 | `2026-07-07-store-runtime-reenable-263` | 1 | F1, F2 | not_started | — | — |
| F4 store-lockup-detect-notify | #264 | `2026-07-07-store-lockup-detect-notify-264` | 2 | F1, F3 | not_started | — | — |
| F5 disabled-stores-settings-ui | #265 | `2026-07-07-disabled-stores-settings-ui-265` | 2 | F1, F2, F3 | not_started | — | — |

## Wave Plan

- **Wave 0 (parallel):** F1 #261, F2 #262
- **Wave 1:** F3 #263 (after F1, F2 durably merged)
- **Wave 2 (parallel):** F4 #264, F5 #265 (after F3 durably merged; F1/F2 already merged)

## Integration → main

- **epic_merge_pr:** not opened.

## Operational Notes

- The epic-orchestrator session operates from its native worktree branch and never checks out the
  integration branch in that worktree (doing so collides with child worktree/branch creation).
  Integration-branch `epic-status.md` commits are made from a dedicated integration worktree at
  `TaskMaster-epic-int`.
- The machine-authoritative checkpoint (`artifacts/orchestration/epic-orchestrator-state.json`) is
  gitignored and lives in the session worktree, where the wave-barrier PreToolUse hook reads it.
