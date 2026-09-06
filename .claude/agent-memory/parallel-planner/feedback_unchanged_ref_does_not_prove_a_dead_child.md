---
name: unchanged-ref-does-not-prove-a-dead-child
description: After a rate-limit or session interruption, an unchanged branch head does NOT prove the background child died — it is equally consistent with still-running; never relaunch on that inference alone
metadata:
  type: feedback
---

**Rule: never conclude a background child is dead from an unchanged pushed ref.** An unchanged head
is equally consistent with a child that is still running and has not reached its commit step. Before
relaunching, establish liveness separately from artifact state.

**Why.** On the `bugs-2026-09-02` run (2026-09-02) a session rate limit fired while seven
radius-correction children were in flight. On resume I correctly re-derived ground truth from the
pushed refs and saw three corrected heads and four unchanged ones, then relaunched the four. Two
minutes later item #584's ORIGINAL child reported success and pushed `12a11031`. It had never died;
it was still working. The relaunch was redundant, and by the same reasoning the relaunches for #564,
#729 and #730 were probably duplicates too.

This is the same failure mode as [[stale-checkpoint-is-not-a-dead-agent]] in the main-session
memory, reached from the opposite direction: there the frozen artifact was a checkpoint, here it was
a branch head. The invariant is identical — **artifact staleness is not a liveness signal.**

**What saved it.** Every relaunch prompt carried the instruction "a plain fast-forward, never a
force push." Both the original and the duplicate branch from the same base commit, so neither
commit is a descendant of the other and the second push is rejected as non-fast-forward. The child
reports the failure instead of overwriting. Without that clause a duplicate could have force-pushed
its own commit over the original's correction and silently discarded it.

**How to apply.**

1. On resume after any interruption, re-derive artifact state from the refs — that part is right and
   the cache doctrine requires it.
2. Then check liveness independently before relaunching anything: `git worktree list --porcelain`
   for a live agent worktree still holding the branch, and process state if reachable. A branch held
   by a locked worktree with recent index or HEAD mtimes is a running child, not a corpse.
3. Prefer WAITING over relaunching when the work is idempotent-but-expensive. A duplicate
   orchestrator child costs a full preparation run; waiting costs nothing but wall-clock.
4. ALWAYS put "plain fast-forward, never force push" in any prompt that asks a child to push to an
   existing branch. It converts a duplicate-child race from data loss into a reported failure.
5. If a duplicate has already been launched, do not try to cancel it — there is no cancel. Record
   the hazard, let the fast-forward rule arbitrate, and verify the final head against the expected
   correction rather than against the child's self-report.

See [[planner-git-commits-must-be-single-bare-segments]] for the related fact that a planner-created
worktree cannot be cleaned up, which is why these agent worktrees accumulate (53 live at the time of
this incident) and why a stale one holding a branch name is the normal case rather than the
exception.
