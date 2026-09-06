---
name: preimplementation-gate-reads-sibling-checkpoint
description: enforce-orchestration-preimplementation-gate.ps1 resolves its checkpoint path relative to the session root, so in parallel mode a sibling item's write to that shared file blocks every source write in YOUR worktree
metadata:
  type: project
---

`enforce-orchestration-preimplementation-gate.ps1` sets `$script:CheckpointPath` to the **relative**
path `artifacts/orchestration/orchestrator-state.json`, which resolves against the Claude session
cwd — the session-root worktree — not the agent worktree doing the work.

In parallel mode every concurrent item shares that one file. When a sibling orchestrator overwrites
it with its own payload, `Test-OrchestrationReady` evaluates the SIBLING's state. If the sibling's
payload lacks `lifecycle_ready`, the gate returns false and begins denying **every** `.cs` and `.ps1`
write in your worktree, even though your own item's checkpoint is correct and ready throughout.

Observed on issue #736 (2026-09-04): an executor mid-Phase-3 was blocked on every source write for
about 3.5 minutes while sibling item #752 held the shared file. The executor's own worktree
checkpoint had `issue-num=736` and `lifecycle_ready=True` the whole time.

**How to apply.** Do not "fix" it by writing your payload into the shared file — that only moves the
denial onto the sibling and can corrupt its run; see
[[shared-checkpoint-read-modify-write-corrupts]]. Poll and resume: the window closes when the sibling
finishes its write. Tell executors to expect it and to wait rather than diagnose it as a defect in
their own item, because the error text names no item and reads like a local lifecycle failure.

Report it upward as an infrastructure defect rather than absorbing it silently. The fix belongs
upstream in drm-copilot and is the same class as
[[model-routing-hook-reads-canonical-path-only]], which was already repaired for parallel and epic
mode by reading from the item worktree; this gate never got the corresponding fix. Per
[[project_claude_files_are_pushdown_owned_fix_upstream]] the repair must land upstream, not here.
