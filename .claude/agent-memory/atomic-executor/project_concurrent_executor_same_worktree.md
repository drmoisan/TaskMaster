---
name: concurrent-executor-same-worktree
description: Two executors running the same atomic plan in one git worktree corrupts shared files; detect via file-mtime progression during your own turn
metadata:
  type: project
---

During the #298 taskvisualization-secondary-testability run, a second executor was actively producing the SAME plan's output in the SAME worktree (`winforms-298`) concurrently with the delegated executor.

Detection signals (reusable):
- `git status` reported "clean" at turn start (~01:17), then new `?? ` / ` M ` files appeared mid-turn that the executor never wrote.
- File mtimes showed sequential plan progression DURING the turn: `FlagCalculations.cs` 01:23 (P4-T1) → `FlagTasks.cs` 01:24 (P4-T2) → `AutoCreateProject.cs` 01:27 (P5-T1). A single synchronous agent turn cannot write files "in the background," so mtimes advancing while doing unrelated read tool-calls proves a separate live writer.
- After a targeted `git stash push -u -- <files>` and later `git stash pop`, the csproj came back with includes the stashed version lacked — i.e., the concurrent writer 3-way-merged into the popped csproj.

**Why:** The orchestrator apparently launched (or left alive) a duplicate executor on the same branch/worktree. Racing it corrupts `FlagTasks.cs`, the csproj, and later test/coverage runs.

**How to apply:** Before Phase 0 baseline, snapshot `git status --porcelain` and key mtimes; re-check after a few tool calls. If untracked/modified files you did not author appear or advance, STOP — do not stash/reset (it clobbers the other writer via 3-way merge) and do not race. Report the concurrent-execution conflict to the orchestrator and let it enforce a single executor per worktree. See [[epic-orchestrator-worktree-isolation]] for the worktree-isolation principle.
