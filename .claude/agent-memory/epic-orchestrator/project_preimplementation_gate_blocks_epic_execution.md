---
name: preimplementation-gate-blocks-epic-execution
description: enforce-orchestration-preimplementation-gate.ps1 denies every Agent(orchestrator) epic-EXECUTION delegation; epic-orchestrator-state.json is in the write-exemption list but is not a readiness source, and repointing the path would still fail on schema
metadata:
  type: project
---

`.claude/hooks/enforce-orchestration-preimplementation-gate.ps1` (PreToolUse, matcher `Agent`)
blocks the entire epic-orchestrator execution surface. Verified live 2026-08-25 against
commit 41eb2a5e on the `quickfiler-bug-family` epic; four verbatim denials recorded in
`delegation_failures[]`.

**Why:** the hook carries TWO path sets and only one knows about the epic surface.

- `$script:CheckpointPaths` (PLURAL, ~line 24) — the WRITE-exemption list. It DOES include
  `artifacts/orchestration/epic-orchestrator-state.json`. Consumed at ~line 104 only, by the
  pathspec classifier ("writing a checkpoint is bookkeeping, not implementation"). Issue #539
  extended this list to the epic/parallel surfaces.
- `$script:CheckpointPath` (SINGULAR, ~line 17) — the READINESS source, hard-coded to
  `artifacts/orchestration/orchestrator-state.json`. Consumed at ~line 241 only, by
  `Get-CheckpointContent` -> `Test-OrchestrationReady`. Never generalized alongside the plural list.

A genuine epic kickoff always trips `Test-ImplementationDelegation` (whole-payload regex on
`atomic-executor|implementation|execute`). The only exemption is `Test-PreparationModeDelegation`,
which needs BOTH `Preparation mode: true.` and `route_id: preparation.` — those are
epic-plan/parallel-plan markers, so `/epic-plan` passes and `/epic-run` cannot.

**Repointing the constant is NOT the fix.** `Test-OrchestrationReady` demands four SCALAR root
fields: `issue-num`, `feature-folder` (must `StartsWith('docs/features/active/')`), `route_id` or
`path_selected`, and `lifecycle_ready`. The epic checkpoint satisfies only `route_id`. It has no
top-level `issue-num`/`feature-folder`/`lifecycle_ready` — it carries `features[]` keyed
`issue_num`/`feature_folder` with UNDERSCORES, not the gate's hyphens. The gate asks a singular
question; an epic schedules N features and has no single truthful answer.

**How to apply:** on any `/epic-run`, read the hook before delegating — if `$script:CheckpointPath`
is still singular, the run is blocked and re-probing only burns denied calls. Fix belongs upstream
in drm-copilot (`.claude` is push-down-owned, see [[project_claude_files_are_pushdown_owned_fix_upstream]]),
as a polymorphic readiness branch keyed on the epic-mode kickoff literals `Epic mode: true.` +
`epic_checkpoint_path: artifacts/orchestration/epic-orchestrator-state.json`, validating
`route_id == 'epic'` and the target feature's presence in `features[]` with a `plan_path`.

Do NOT route around it: fabricating `orchestrator-state.json` falsely certifies a single-feature
route, and word-golfing the prompt to dodge the regex is circumvention. See
[[feedback_inline_child_lifecycle_prohibited]] — record `delegation_failures[]` verbatim and halt.
