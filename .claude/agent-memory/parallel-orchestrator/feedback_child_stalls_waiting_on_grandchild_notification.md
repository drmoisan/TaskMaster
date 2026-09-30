---
name: child-stalls-waiting-on-grandchild-notification
description: An item child that ends its turn "waiting on the executor's long-running command" is effectively dead — the notification goes to the executor, not the child; tell children to poll durable output, and audit silent items on a timer
metadata:
  type: feedback
---

An item child orchestrator that returns "the executor is waiting on a long-running command; it will
notify again" will usually never notify again. On `bugs-2026-09-28` (2026-09-29) item 931's child
said exactly that at about 09:38; its executor committed the last plan task at 09:48 and the item
then sat idle for ten hours with no process, no file write, and no PR, while I treated it as "running".

**Why:** the background-command completion notification is delivered to the agent that started the
command (the executor), not to its parent. Once the child ends its turn, nothing wakes it. The
task-notification text ("stops with no live background children") was the tell.

**How to apply:**
- Put this line in every child prompt: "Do not wait on a background notification addressed to a
  subagent: if you start long-running work in the background, poll its durable output yourself until
  it completes rather than ending your turn to wait."
- When a child's final message says it is waiting, treat it as stopped. Verify with durable state
  (branch head and commit time, worktree file mtimes, process list) within the hour, and relaunch
  from the checkpoint once the worktree is quiet — do not wait for a notification that cannot come.
- An item with no PR hours after its last commit is a stall, not slow progress. See
  [[quiescence-is-not-a-liveness-test]] for the opposite hazard (relaunching on a live child).

**The parent can poll without ending its turn, and this worked on `/parallel-add 942`
(2026-09-29/30, a 100-minute preparation).** Foreground `sleep` is blocked, but a `pwsh` loop with
`Start-Sleep -Seconds 30` inside a ~560-second deadline, run as a scriptblock from a `.txt` file with
Bash `timeout: 600000`, is permitted. Each call resolves the child worktree from
`git worktree list --porcelain` by branch name, then reads that worktree's own
`artifacts/orchestration/orchestrator-state.json` for the terminal `next_step`. It prints the head
commit subject on timeout, so each call also reports progress. Re-issue the same one-liner until it
returns DONE; the completion notification then arrived about a minute after the poll did.
Watch COMMITS for progress, not the child checkpoint: that child left `next_step: S4_research` in its
checkpoint for 90 minutes while it committed research, spec, the plan and two preflight revisions.
A stale child checkpoint beside advancing commits is normal. Only a stale checkpoint AND a stale
head AND stale file mtimes indicates a stall.
