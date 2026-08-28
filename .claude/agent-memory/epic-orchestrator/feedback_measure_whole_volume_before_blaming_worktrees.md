---
name: measure-whole-volume-before-blaming-worktrees
description: On disk exhaustion, measure the whole volume before recommending a worktree or build-output purge — one 2.51 TiB stray temp file dwarfed all 66 agent worktrees combined by 7x
metadata:
  type: feedback
---

When an epic halts on "No space left on device", **find the largest consumers on the volume before proposing
any cleanup.** Do not reason from the assumption that accumulated agent worktrees and `bin`/`obj` are the cause.

**Why:** On the quickfiler-bug-family epic I hit 0 bytes free on a 3.7 TiB volume, saw 78 worktrees (66
`agent-*`, 16 pinned at one stale HEAD) and 288 `bin`/`obj` dirs, and recommended purging them. A child then
measured properly and found the real cause: a single stray
`C:\Users\DanMoisan\AppData\Local\Temp\_m8_probe.txt` at **2,756,440,788,820 bytes (2.51 TiB, 68% of the
volume)** — captured Python interpreter banner output that had ballooned. Everything I proposed purging totalled
~370 GB: `.claude/worktrees` 264 GB, the session worktree 52 GB, `.nuget/packages` 9 GB, rest of `%TEMP%`
44 GB. My recommendation would have destroyed real work to reclaim under 15% of what one junk file held.

**How to apply:** Measure first, recommend second, and withdraw a bad recommendation explicitly once
falsified. A stale mtime distinguishes a static hog from live consumption — that file had not been written in
two days, so the last few MB draining away was the live children's `msbuild`/`vstest` activity, not the file.
Deleting a huge file outside the repo in the user's profile is still the *user's* call: it is irreversible and
not yours to make, even when the evidence clearly says junk. Also note partial relief is not safety — free
space returning to 71 GB let the toolchain run while 2.51 TiB was still held hostage. Related:
[[preserve-halted-child-worktree]], [[merged-child-worktree-still-locked-defer-removal]].
