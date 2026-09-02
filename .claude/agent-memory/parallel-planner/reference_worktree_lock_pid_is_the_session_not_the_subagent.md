---
name: worktree-lock-pid-is-the-session-not-the-subagent
description: A locked agent worktree names the parent claude.exe session pid, not a per-subagent pid, so a live pid on a lock proves nothing about whether a subagent is still working
metadata:
  type: reference
---

`git worktree list --porcelain` renders an agent worktree lock as
`locked claude agent agent-<id> (pid NNNNN)`. That pid is the **Claude session's own
`claude.exe` process**, not a process belonging to the subagent that created the worktree.

Verified 2026-08-29 while resuming the `bugs-638-644-647` parallel run: four worktrees were locked
naming pid 13948, and walking this session's own process ancestry
(`Get-CimInstance Win32_Process` up the `ParentProcessId` chain from `$PID`) put 13948 in the chain
as the parent `claude.exe`. Its start time also matched the session worktree's own name.

**Why it matters:** every worktree a session's subagents create is locked with the SAME pid, and
that pid stays alive for as long as the session does. So `Get-Process -Id <pid>` returning a live
process is entirely compatible with every subagent that ever held those worktrees being dead. The
liveness check that feels decisive is not.

**How to apply:** when asked to check whether a worktree is "held by a live process", first walk
your own ancestry and compare. If the lock pid is your own session, the lock tells you nothing
about subagent liveness and you must fall back to the other signals — HEAD/index mtimes, whether
the worktree's `artifacts/orchestration/orchestrator-state.json` is still advancing, and whether
the branch has gained commits. Multiple worktrees sharing one pid is itself the tell.

Two `powershell -NoProfile -Command` traps cost invocations here: a `$p.Property` expression inside
a double-quoted bash string gets mangled, and `powershell -File` on a scratchpad script is refused
by the execution policy. Use `powershell -NoProfile -ExecutionPolicy Bypass -File` with a heredoc
script.

Related: [[planner-git-commits-must-be-single-bare-segments]] records the sibling constraint that
`git worktree remove` is unavailable to the planner at all.
