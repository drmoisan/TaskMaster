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

## Second confirmation on #444 (2026-08-27) — the tell is a checkpoint mtime you did not author

The parent relaunched a 444 child to "finish the CI gate and merge", having verified the worktree idle at 20:44:10Z. At 20:52:01Z — before this run had written anything — `artifacts/orchestration/orchestrator-state.json` gained a complete, correct `ci_gate` plus `step9_status: "passed"` and `next_step: "S10_merge"`. Twenty seconds later the owner merged PR #654 and flipped `step9_status` to `"verified"`.

**Why:** a parent's idleness check has a shelf life of minutes. The checkpoint is the cheapest ownership probe available, because you know exactly which writes are yours: **any `last_updated` or mtime on the shared checkpoint that postdates your own last write, while your feature number is still in `issue-num`, is a live co-owner** — not the parent handing you a baton, and not a stale record. On #444 the co-owner was ~1 step ahead, exactly as on #442.

**How to apply:**

- Read the shared checkpoint's mtime and `last_updated` *before* your first write, and again immediately before any irreversible call (`gh pr merge`, `git push`). A value you cannot account for means cede.
- Cede, then **verify independently rather than relaying**. Every load-bearing claim in the co-owner's record was re-derived here from `gh` and `git`: run conclusion + per-job conclusions, `headSha == PR headRefOid`, `git merge-base --is-ancestor <head> <tip>`, zero `awk '$1==0 && $2>0'` numstat rows across `base_before..tip`, and both siblings' `.csproj` include counts on the tip. All matched. Reporting a co-owner's merge you have verified is a complete result, not a halt.
- Do not race a merge just because the parent addressed the instruction to you. `gh pr merge` is not idempotent in its side effects (branch deletion, epic barrier state), and the merge had already satisfied the objective.

