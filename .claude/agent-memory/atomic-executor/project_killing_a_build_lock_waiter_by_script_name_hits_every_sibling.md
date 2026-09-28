---
name: killing-a-build-lock-waiter-by-script-name-hits-every-sibling
description: Matching pwsh processes on the shared build-lock acquire script name kills EVERY parallel item's queued waiter, not just your own; scope the kill by PID captured at launch
metadata:
  type: project
---

The shared machine build lock at `parallel-build-lock/` is acquired by every parallel item with the
same one-line payload, which dot-sources the same `acquire.txt`. Every item's waiter therefore
carries an identical command line. A cleanup that matches on the script name —
`Get-CimInstance Win32_Process | Where-Object { $_.CommandLine.Contains("acquire.txt") }` — matched
12 processes on 2026-09-13 when only one of them was the caller's own waiter, and killed all of them.

**Why:** the caller wanted to avoid stranding the lock after an early stop. That concern was real but
the remedy was mis-scoped. Two facts make the broad kill both unnecessary and harmful:

- A waiter that has not printed `ACQUIRED` holds nothing. Check `parallel-build-lock/LOCK/holder.txt`
  first: if it reads `COORDINATOR-HOLD|<timestamp>` or names another item, your waiter never held the
  lock and there is nothing to release and nothing to strand.
- The kill also terminates the *caller's own* shell chain, so the command exits 255 and the output is
  truncated mid-list. The damage is not visible in the command's own result.

**How to apply:** before terminating a lock waiter, read `LOCK/holder.txt`. If it does not name your
issue number, do nothing — leave the waiter to reach its own TIMEOUT, which is bounded at 60 minutes.
If you must terminate it, capture the PID at launch and kill that PID alone; never match on the
script name. If you have already run a broad kill, say so in the completion report: the siblings'
acquire commands died and their agents must re-run acquire once the coordinator hold lifts.

Related: [[project_concurrent_executor_same_worktree]], [[project_sibling_worktree_shared_tooling_hazard]].
