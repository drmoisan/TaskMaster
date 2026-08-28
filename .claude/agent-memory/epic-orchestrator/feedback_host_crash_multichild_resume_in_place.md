---
name: host-crash-multichild-resume-in-place
description: A host-process crash kills every in-flight wave child at once but is cleanly recoverable by reusing each worktree in place; file mtimes — not the crash report — are authoritative for when work actually stopped
metadata:
  type: feedback
---

When the VS Code / host `claude.exe` process dies, EVERY in-flight wave child dies with it
simultaneously. This is cleanly recoverable: relaunch each child as a FRESH `Agent(orchestrator)`
reusing its EXISTING worktree and branch in place — no `isolation: "worktree"`, no new branch.

**Why:** All the child's progress is on disk. Verified on quickfiler-bug-family wave 0 batch A
(2026-08-26, dead pid 21368): four children at `next_step: S5_atomic_execution`, 1/2/4/1 commits
ahead of the integration tip and 12/8/3/0 uncommitted entries respectively, no PR and no pushed
branch for any of them. `isolation: "worktree"` would have stranded all of that in a new
directory; a new branch would have collided with the branch still checked out in the old worktree.
Reuse-in-place sidesteps both the worktree-removal gate and the branch collision because nothing
is removed or recreated.

**How to apply:**
- **Distrust the reported crash time; trust mtimes.** The resume briefing said the crash was
  ~08:45. `find -printf '%TY-...'` showed source writes through 09:00:26–09:01:11 in all four
  worktrees — the host actually died ~16 minutes later and more work survived than briefed. Always
  re-derive the real stop time from the newest non-`.git`/`obj`/`bin` file per worktree.
- **Prove the pid is dead, and that nothing newer owns the tree.** `Get-Process -Id <pid>` plus a
  full `claude.exe` enumeration: every live host must post-date the last worktree write. Only then
  is it safe to spawn into the worktree.
- **Leave the stale git locks alone.** Every child worktree stays `locked ... (pid <dead>)`. That
  lock is stale over LIVE work; leaving it protects the uncommitted work from prune. Do not
  unlock, force-remove, or prune.
- **Expect orphaned build processes.** 17 MSBuild node-reuse workers and a VBCSCompiler survived
  the host death and can hold `obj/`/`bin/` locks. Do NOT kill them (shared machine). Instead tell
  each relaunched child to report a locked-output build failure in `blocked_reason`.
- **Checkpoint bookkeeping:** flip the old receipts to `terminated_host_crash`, add a per-feature
  `crash_episodes[]` record with surviving HEAD / commits-ahead / uncommitted count / child
  `next_step` at death, and add fresh receipts carrying `recovery_mode` and
  `supersedes_receipt_terminated_at`. `merge_status` stays the canonical `worktree_created`.
- **Tell the child its checkpoint is the one INSIDE the worktree** (absolute path). A
  non-isolated delegate otherwise resolves `artifacts/orchestration/orchestrator-state.json`
  against the invoking directory and loses its own S-step progress.

**Contrast:** [[api500-abandoned-child-fresh-redelegation]] is the same reuse-in-place remedy for a
single child killed by API 500s. [[hung-child-recovery-blocked-by-removal-gate]] is the case with
NO clean recovery — there the prior pid was still LIVE. Liveness of the owning pid is the
discriminator between "resume in place" and "halt and report".
