---
name: resumed-child-orchestrator-shares-worktree
description: A resumed child orchestrator can be live in the same feature worktree you are recovering; duplicate evidence sets and bulk same-millisecond file writes are the tells. Verify ownership before writing, and cede rather than race.
metadata:
  type: feedback
---

Before "recovering" a stalled feature worktree, establish whether a session already owns it. On #442 (2026-08-27) a resumed **child orchestrator** was executing the same feature in the same worktree while the parent session independently re-ran the whole Phase 6 toolchain. Both produced correct, identical results — and two competing evidence sets.

**Why:** duplicated work is the small cost; the real risk is two actors driving one branch to PR, plus an audit trail where two artifacts claim the same `[P#-T#]` with different timestamp conventions. The owner session was further along (it had already done the CFN-4 promotion, AC check-off, and PR-statement drafting) so the parent's parallel effort was pure redundancy.

**How to apply:**

- **Two distinct signals, do not conflate them.** (a) ~100 files sharing an mtime to the millisecond is a *bulk sync/checkout*, not an agent typing — on #442, 81 of 82 such files were byte-identical to the integration base, i.e. an unrecorded partial merge. (b) Files appearing steadily seconds apart, with prose content, is a *live agent*. Signal (a) is safe to discard-and-merge properly; signal (b) means stop.
- Timestamp convention is a fingerprint: that session labelled artifacts in **UTC**, the parent in **local** (`14-19` vs `10-23` on a UTC-4 host). Two sets of the same gate 4 hours "apart" on one day is usually one host and two conventions, not two runs.
- Check `git cat-file -e <base>:<path>` and `git stash list` before concluding a mystery file is foreign — it rules out the merge and your own stash cheaply.
- Do not `git add` a whole evidence directory while another actor writes into it. Doing so swept 11 of the child's artifacts into a commit whose message described only the parent's four.
- When the owner is live and ahead, **stop before PR** and report. Committed work is safe; a raced PR is not.

Related: [[one-executor-per-worktree]], [[feedback_stale_checkpoint_is_not_a_dead_agent]], [[csharp-coverage-denominator-two-figures]].
