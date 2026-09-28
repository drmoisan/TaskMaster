---
name: external-actor-can-merge-your-child-pr-midrun
description: A human/external actor can merge your epic-child PR while your toolchain pass runs; re-read PR state before the CI gate, prove tree equality, and land stranded evidence via a docs-only follow-up PR
metadata:
  type: feedback
---

Before dispatching the CI gate or assuming you will perform the merge, re-read the PR's live state
with `gh pr view <N> --json state,mergedAt,mergeCommit`. The repository owner can merge an epic-child
PR at any moment, including while your post-base-merge toolchain pass is still executing.

**Why:** On child 501 the parent handed off "base re-merge, fresh CI gate, merge" with PR #659 OPEN
and MERGEABLE. Between the local base merge and the CI dispatch, the owner merged #659 as `4cb709db`.
Discovering this only at `git push` time (the branch showed `1 behind / 2 ahead` and the single
base-only commit was literally `Merge pull request #659`) is late but recoverable; discovering it
after opening a duplicate PR is not.

**Recurrence (child 488, 2026-08-28) — the window is smaller than you think.** PR #672 was verified
OPEN/MERGEABLE, CI run 33150408095 concluded `success` at 07:14:00Z, the base was re-confirmed
0-behind at 07:14:31Z, and the owner merged at 07:15:20Z — inside the ~3 minutes between the gate
check and the `gh pr merge` call. So re-read PR state *immediately before merging*, not only before
the CI gate. Two things made this benign and are worth reproducing: the external merge landed AFTER
the gate concluded green (check `mergedAt` against the run's completion time before claiming the merge
was gated), and nothing was stranded because all evidence was committed before PR creation, so no
follow-up PR was needed. The fidelity proof was a true two-parent merge whose tree hash equalled HEAD's
exactly, with an empty `git diff <merge> HEAD`.

**Recurrence (child 825, 2026-09-09) — a parent's "ground truth" block can be stale on arrival.**
A resume prompt opened with a carefully-derived facts block ("PR #834 is OPEN, mergeable MERGEABLE,
mergeStateStatus CLEAN", timestamped 22:02Z) and instructed the child to merge on green. `gh pr view`
returned `MERGED` on the first call: the owner had merged at 22:02:23Z, *within the same minute the
parent derived the block*. The lesson generalises beyond external merges: a parent's re-derived ground
truth is a measurement, not a guarantee, and its staleness window starts the moment it is written. Verify
every load-bearing premise it asserts before acting on it, especially the ones the prompt tells you to
verify — and also the ones it presents as settled. Here the prompt's own step 1 (re-check HEAD and
cleanliness) was satisfied, while the unchecked premise (PR still open) was the one that had changed.

**NOT a recurrence (parallel item 839, 2026-09-13) — MISATTRIBUTED, corrected by the
parallel-orchestrator that performed the actions.** The item child recorded this as an external-actor
event. It was not one. The run coordinator performed all three actions itself, from this session, in
this order: `gh pr update-branch 876`, then `gh pr merge 876 --merge` after recording `ci_green`, then
`git worktree remove` after durably confirming the merge. Issue #839 then closed one second after the
merge, automatically, from the `Closes #839` line in the pull-request body — not by any human action.

The child's inference is understandable and worth naming as a trap: **`gh` acting under the operator's
credentials is indistinguishable, through the GitHub API, from the operator acting by hand.** The
update-branch merge commit shows `author drmoisan, committer GitHub`, which reads exactly like a human
using the *Update branch* button. A child cannot tell its own parent's `gh` calls from a maintainer's
clicks, so it should describe what it OBSERVED ("the head moved and the PR merged") and attribute the
actor only as unknown, rather than naming one. Inventing a third party to explain a parent's action
puts a false causal claim into durable shared memory, which is how a wrong lesson outlives the run
that produced it.

The operational content below is retained because it is correct and useful regardless of who acted;
only the attribution was wrong. Item 3's premise in particular was false: nothing deleted the worktree
"under" the child — the coordinator removed it deliberately, after the merge was durably confirmed and
after the child had already ended.

Three things happened in the seven minutes after `gh pr create` returned PR #876, and each is worth
anticipating:

1. **The head SHA the child reported went stale in about forty seconds.** It opened the PR at
   `bed094d5f` and said so. The coordinator immediately ran `gh pr update-branch`, producing merge commit
   `3c3f89f72` (`Merge branch 'main' into <branch>`, author drmoisan, committer GitHub). CI then ran
   against the merge result, not against what I pushed. `gh pr checks --watch` exiting 0 tells you
   nothing about WHICH head was tested — always re-read `headRefOid` alongside the check states and
   report the head CI actually ran on. This is favourable when it happens (you get a green signal on
   the real merge outcome) but only if you notice.
2. **It moves the merge base, so a plan's BASE-SHA anchor stops describing the tip.** Harmless only
   because every anchored gate had already executed and its evidence was committed; the pre-update
   head remains the merge's first parent and therefore a true ancestor. Never re-run an anchored gate
   after such an update and compare it with the committed figures — different base, incomparable.
3. **After the merge, the COORDINATOR removed the item worktree** — deliberately, with
   `git worktree remove`, after recording `merge_status: merged` and durably confirming the merge, and
   after the child had already ended. It was not a stray cleanup process and it did not race the child.
   Note that the removal gate REFUSES this until the checkpoint records a terminal merge status, so the
   ordering is enforced rather than merely intended.

   The derived lesson survives the corrected attribution and is worth keeping: **do every checkpoint
   and memory write BEFORE the pull request goes green**, not after. A child's gitignored checkpoint
   becomes unwritable the moment its worktree is retired, so a `completed` status planned for
   afterwards may never land — and on this surface the parent retires the worktree as soon as the merge
   is confirmed, which can be under a minute after green.

**Recurrence on the PARALLEL surface (item 895, run `bugs-2026-09-17`, 2026-09-17) — the coordinator
reuses ONE session-root checkpoint path and overwrites yours with the next item's seed.** This is a
second, distinct mechanism for the "your post-green writes never land" failure above, and it is worse
than worktree removal because the file still exists and still validates — it just describes a sibling.

Sequence: PR #901 opened at 05:54, merged at 06:02:45Z, issue #895 auto-closed one second later from
the `Closes #895` line. I finished the CI durability check at 06:12, wrote `ci_gate.conclusion`,
`pr_gate`, `next_step: complete` and `step9`/`step10` to
`<session-root>/artifacts/orchestration/orchestrator-state.json`, and validated it clean. The mirror
copy to the item worktree then failed with "Could not find a part of the path": the coordinator had
already removed the worktree. Re-reading the session-root file showed it now held **item #900's seed**
(`issue-num: 900`, its own branch and plan path, `model_routing_receipts: []` again). My completed
state was gone, replaced rather than deleted.

Two consequences worth internalising:

- **Your terminal checkpoint write has no durable home once the coordinator advances.** The rule above
  ("do every checkpoint and memory write BEFORE the pull request goes green") is not merely prudent on
  this surface, it is the only thing that works. Treat the final-status write as best-effort and put
  the load-bearing record in your final report to the caller, which is the one channel the coordinator
  actually reads.
- **Do not repair it.** Once the file holds a sibling's identity, writing your state back over it is
  writing a sibling's checkpoint, which the standing constraint forbids and which would strand item
  #900 exactly as #895 was stranded. Read `issue-num` before any late checkpoint write; if it is not
  yours, stop and report. Contrast [[model-routing-hook-reads-canonical-path-only]], where the same
  file WAS mine and repairing it was correct — the ownership check is what separates the two cases.

**The merge was not gated on green, and the timestamps prove it.** `mstest-coverage` takes 8m12s and
the run started around 05:56, so it could not have concluded before ~06:04; the merge landed at
06:02:45Z. The guidance above to "check `mergedAt` against the run's completion time before claiming
the merge was gated" earns its keep here: the honest statement is that the checks were green when I
verified them at 06:12 against the merged head, NOT that they were green when someone merged. Do not
launder an ungated merge into a gated one by reporting only the final green state.

**I also fell into the misattribution trap this file names.** I wrote "merged by an external actor on
the drmoisan account" into the checkpoint on the strength of `mergedBy.login`. That field cannot
distinguish the operator clicking Merge from the parallel-orchestrator calling `gh pr merge` under the
operator's credentials, and on this surface the coordinator merging is the *expected* path. Say "the
pull request was merged at 06:02:45Z; I did not merge it and cannot identify the actor" and stop
there.

Verify a merge you did not perform: `gh api repos/<o>/<r>/commits/<sha> --jq '{message, parents}'`.
Two parents means a real merge commit rather than a squash (which matters where squash is banned),
and the merged content itself can be confirmed independently with
`gh api repos/<o>/<r>/contents/<path>?ref=main`, which needs no local checkout at all — the only
verification route still open once your worktree is gone.

**How to apply:**

- When the base tip moves, always `git log --oneline origin/<base> ^HEAD` before re-merging. A single
  base-only commit whose subject names YOUR pr number means the merge already happened.
- Determine whether your reconciliation is already represented by comparing TREES, not commits:
  `git rev-parse <their-merge>^{tree}` vs `git rev-parse <your-merge>^{tree}`. A merge of A into B and
  a merge of B into A produce the same tree when neither side conflicted, so an externally merged
  commit can be provably equivalent to the reconciliation you just validated locally. That equality
  is what lets you claim your local toolchain pass gates the merged tree.
- Evidence you committed after the external merge is stranded on the branch. Land it with a
  **docs-only follow-up PR** into the same integration base. That also restores
  `git merge-base --is-ancestor HEAD origin/<base>`, which the epic worktree-removal gate needs; a
  branch left ahead of the integration tip can block the parent later.
- Reuse the same `artifacts/pr_body_<issue>.md` + `.receipt.json` pair with a FRESH sha256 and a fresh
  `created_at`. See [[pr-author-receipt-staleness-is-mtime-vs-created-at]] and
  [[pr-author-hook-blocks-gh-in-this-repo]]; the readiness preflight itself only checks that
  step5-8 are not pending/blocked, `blocked_reason` is `none`, and `local_execution_overrides` /
  `delegation_bypasses` are empty or absent.
- Record the external merge honestly in the checkpoint (who merged, when, and that you did not gate
  it beforehand) rather than presenting it as your own merge.
