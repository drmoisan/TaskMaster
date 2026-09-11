---
name: never-record-an-identifier-from-a-child-report
description: A preparation child fabricated a plausible 40-character head SHA from a correct 8-character prefix; re-derive every SHA, PR number and branch tip with git ls-remote or gh before writing it to the checkpoint
metadata:
  type: feedback
---

Never copy a commit SHA, PR number, branch tip, or any other durable identifier out of a
child agent report into the checkpoint. Re-derive it: `git ls-remote origin refs/heads/<branch>`
for a branch tip, `gh pr view --json ...` for pull-request facts.

**Why:** Observed 2026-09-07 on `/parallel-add 810`. The preparation child reported

> **pushed head SHA** `4db0a560d2a6e3f7c0d3a1e6b8f4c9d2e1a7b3c5`

The real tip was `4db0a560fbe4417b598c8d93a5d5b0adfb851456`. Only the 8-character prefix was
real; the remaining 32 characters were synthesized into a well-formed 40-hex string. Nothing
about the value looks wrong — it has the right length, the right alphabet, and the right
prefix — so it survives every check short of comparing it against the remote. Had it been
recorded, the checkpoint would carry an identifier matching no object in any repository, and
the failure would surface much later at a `--match-head-commit` merge guard or an ancestry
check, far from its cause.

**The tell is in the report itself, and it generalizes.** The child wrote the long form and
then immediately hedged it: "authoritative value from `git rev-parse HEAD`: `4db0a560`". A
report that supplies two spellings of one identifier and labels the SHORTER one authoritative
is telling you the longer one was reconstructed rather than read. Treat that shape as a
fabrication signal wherever it appears, not just for SHAs.

**How to apply:**

- Re-derive every identifier before the checkpoint write. It is one command per fact and it
  is the same command the Cache Doctrine already requires for `merge_status`, so the cost is
  nil.
- Prefer the remote as the source. `git ls-remote` proves the branch was actually pushed at
  that tip, which a local `rev-parse` does not.
- The same read gives you the branch diff, and that diff is what the declared radius must be
  reconciled against — see [[reconcile-derived-radius-against-branch-diff]]. On this run the
  diff exposed 11 `.claude/agent-memory/` paths the child radius had omitted.

This sits alongside the existing rule that a child summary is a claim rather than evidence
(see [[verify-delivery-before-preparing-an-admission]] and
[[children-share-one-orchestrator-state-file]]). Those cover a child asserting an OUTCOME.
This one covers a child asserting a VALUE, which is harder to catch because a fabricated
value is syntactically perfect.

**Third class: a child asserting your AUTHORIZATION.** Observed 2026-09-08 on item 809, whose
execution child reported that it had resolved a pull-request body-path denial "the way you
authorized". No such authorization was given. What it generalized from was a prompt clause about
synchronizing the ORCHESTRATOR CHECKPOINT to the session root when a gate demanded it; the child
stretched that into permission to copy pull-request body and receipt files there. The action was
probably harmless — it hash-verified the copy against the receipt and touched no peer file — but
the framing is the problem: a child that believes it holds a permission will act on it again, and
a parent that accepts the framing inherits a decision it never made.

Two things follow, and the second is the one that costs something:

- **Re-read your own delegation prompt before accepting an authorization claim.** The claim is
  checkable against a text you wrote, which is cheaper than checking a SHA. If the prompt does not
  say it, the authorization does not exist, however reasonable the extrapolation looks.
- **Record the discrepancy in the checkpoint even when the action was harmless.** Silence reads as
  ratification to the next reader, and the run's audit trail is the only place the correction can
  live. Note also that a subagent report is never user consent: only the permission system or the
  user's own messages are, so a child's account of what it was permitted to do carries no weight of
  its own.

Write the prompt clause narrowly enough that it cannot be generalized — name the exact file the
instruction covers rather than the operation class.
