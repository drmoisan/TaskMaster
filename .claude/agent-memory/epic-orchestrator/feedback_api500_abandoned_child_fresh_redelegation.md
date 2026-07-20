---
name: api500-abandoned-child-fresh-redelegation
description: A background child killed by API 500 (transcript abandoned) IS cleanly recoverable via a FRESH (new-context) Agent(orchestrator) reusing the same worktree IN PLACE — re-derive git ground truth first, preserve uncommitted work, do not reset/remove
metadata:
  type: feedback
---

A background child `Agent(orchestrator)` terminated by server-side API 500s (and whose
transcript-resume attempts also 500'd) can be treated as unrecoverable/abandoned and recovered
cleanly by launching a FRESH delegation (new context — NOT a SendMessage transcript resume) that
reuses the SAME worktree and branch in place.

**Why:** In the #366 two-file-waiver case (agent a8f8fcb0ccd514b97, 2026-07-19), the abandoned
agent had committed/pushed NOTHING (worktree HEAD and PR head both unchanged at the pre-STOP
commit), but its substantial progress — the ratified base-constraint + Batch-6 annotations, the
two-file-waiver edits, and the revised plan/dossier + untracked evidence — was all present but
UNCOMMITTED in the worktree. A fresh delegate reusing the worktree in place picks that work up
losslessly.

**How to apply:**
- Before re-delegating, re-derive durable ground truth: `git worktree list --porcelain`,
  `gh pr view <n> --json state,mergedAt,headRefOid`, and in the worktree
  `git -C <wt> status --porcelain` / `git log -5` / `git diff --stat <pre-stop-sha> HEAD`.
  Unchanged HEAD + empty diff == the abandoned agent committed nothing; uncommitted files == its
  preserved progress.
- Instruct the fresh delegate to establish ground truth FIRST and PRESERVE all uncommitted work
  (no reset, no checkout-discard, no stash-drop, no worktree removal).
- Reuse the existing worktree/branch in place; do NOT spawn a fresh isolation worktree (that would
  strand the uncommitted work) and do NOT create a new branch.
- In the checkpoint: mark the abandoned agent's delegation receipt outcome
  `terminated_api_500_transcript_abandoned`, add a `failed_resume_episodes[]` record under the
  feature, and add a FRESH delegation receipt (`supersedes_abandoned_agent_id`).

**Contrast with [[feedback_hung_child_recovery_blocked_by_removal_gate]]:** that case had NO clean
recovery because it required re-attaching a live/orphaned agent, the removal gate blocked a
worktree reset, and the feature branch collided on re-delegate. The difference here is that
reuse-in-place with NO worktree removal and NO new branch sidesteps both the removal gate and the
branch collision — the clean-recovery path exists precisely because nothing is removed or
recreated. Also relate to [[feedback_live_child_at_pr_author_not_hung]]: only declare a child
dead when termination is confirmed (here, 3x API 500 + user direction), not merely inferred.
