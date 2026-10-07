---
name: preimplementation-gate-path-leg-has-no-parallel-resolution
description: In a parallel run with NO orchestrator-state.json in the session checkout, the pre-implementation gate denies every scripts/ or tests/ Edit and every non-exempt git commit from an item child; parallel mode is resolved only on the Agent-delegation leg
metadata:
  type: project
---

Measured 2026-09-29 on parallel item 928 (run bugs-2026-09-28). The session-root copy of
`enforce-orchestration-preimplementation-gate.ps1` resolves `Parallel mode: true` only for the
Agent-delegation leg (`Resolve-OrchestrationDelegationMode` on the prompt). The Edit/Write
`file_path` leg and the Bash command leg go to `Get-CheckpointContent`, which reads the relative
`artifacts/orchestration/orchestrator-state.json` in the SESSION checkout. Earlier runs passed only
because a sibling's checkpoint happened to sit there (see [[preimplementation-gate-reads-sibling-checkpoint]]).
When the session checkout holds only parallel-planner / parallel-orchestrator state files, every
executor edit under `scripts/` or `tests/` is denied, so a parallel item cannot implement at all.

The Agent(atomic-executor) spawn was still allowed (delegation leg), so the block surfaces mid-plan,
on the first production edit, not at delegation.

Commit mechanics measured the same day: `git -C <wt> add -- docs/features/active/...` was allowed,
and `git -C <wt> commit -m ... -m ... -- docs/features/active/<folder>` was allowed when the whole
line carried no `<`, `>`, `$` or backtick. The same commit WITHOUT the trailing `-- <pathspec>` was
denied. This narrows the older note that `git -C` always defeats the exemption.

**How to apply:** at item start, Glob the session checkout for `artifacts/orchestration/orchestrator-state.json`.
If it is absent and the brief forbids writing it, report the block to the parent before delegating
execution, instead of discovering it at the first production edit. The fix belongs upstream in
drm-copilot (give the path and command legs the same parallel resolution the model-routing hook has).
