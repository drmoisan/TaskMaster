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

## Second confirmed occurrence — #501 breadcrumb-coordinator-hub, 2026-08-27

Worktree `agent-a5f370e5c08f8ddb0`, plan `breadcrumb-coordinator-hub-defects-501/plan.2026-08-24T09-40.md`.
Detected during preflight, before writing anything. Two additional signals, both cheaper and more
decisive than the source-file mtime sweep:

- **The plan file's own checkbox count advances while you only read.** Fingerprinted the plan at
  preflight (sha256 `6bf57583...`, `DONE: 0`). Minutes later, without writing, it was sha256
  `446430ed...`, `DONE: 5`, then `DONE: 7`. The plan file IS the shared todo list, so a check-off
  count that rises during your own turn is proof of a second writer by itself. Re-hash the plan
  immediately before the first check-off, always.
- **A bootstrap side-effect DIRECTORY growing localises the other agent to a task ID.** Phase 0
  provisioning tasks have large filesystem side effects (`.dotnet-sdk/` from
  `Install-RepoDotNetSdk.ps1`, `packages/` from `Invoke-Restore.ps1`). Sampling `du -sk .dotnet-sdk`
  twice ~20s apart names the task the sibling is inside even before it writes its evidence artifact.
  Here `.dotnet-sdk/` appeared holding only `dotnet.exe` + `host/`, grew to 764 MB, then `packages/`
  followed — the sibling was mid-P0-T6, then P0-T7, then P0-T8.

**Why this one mattered:** the launching agent's prompt was the literal string
`SendMessage placeholder - not used` — no plan path, no directive, no task. A degenerate or
placeholder delegation message is itself a signal that the harness may have double-launched; treat
it as a reason to fingerprint shared state before acting, not as a licence to go find work to do.

**How to apply (addendum):** blocking is permitted only at preflight, and a concurrent-writer
conflict is detectable at preflight for free — hash the plan, count the evidence dir, sample twice.
Do that BEFORE the first check-off. Once you have written even one artifact you have joined the race
and the clean stop is no longer available.
