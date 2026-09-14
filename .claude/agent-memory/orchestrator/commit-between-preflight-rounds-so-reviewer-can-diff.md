---
name: commit-between-preflight-rounds-so-reviewer-can-diff
description: Commit the plan after each preflight revision round; otherwise the next round has no diff and must re-read the whole plan, which is slower and misses which sentences an applied delta falsified
metadata:
  type: feedback
---

Commit the revised plan to the item branch after EVERY preflight revision round, before launching the
next round, and tell the reviewer the two SHAs and the exact `git diff A B -- <feature folder>`
command.

**Why:** on item #816 (2026-09-12) the plan carried exactly one commit, which predated round 1. Round 2
opened its report with a method note saying a round-over-round diff was unavailable, so it re-read all
311 lines and re-derived every citation instead. Two costs followed:

1. It could not key its confirmation table to the round-1 defect numbers — "the numbering is not
   recoverable from the tree" — so it had to enumerate all sixteen subjects by topic and guess which
   B-number each was. That is pure waste, and it makes the orchestrator's job of checking "was every
   item applied" harder rather than easier.
2. The sibling-invalidation check is exactly the check a diff makes cheap and a full re-read makes
   expensive. The failure mode is a sentence NEAR an applied edit that the edit falsified. With a
   diff you walk each hunk's neighbourhood; without one you must hold the whole document in mind. On
   this item that class of defect appeared in both rounds — B8's superseded premise had survived in
   four neighbouring sentences, and round 2's Delta 1 was a cross-task citation falsified by an
   earlier round's edit.

**How to apply:** after the planner reports a revision applied, run the plan validator, then commit
with a pathspec under `docs/features/active/` (the pre-implementation gate's exempt form) and push.
Then put this in the next round's prompt: the two SHAs, the literal diff command, and the sentence
"the working tree is identical to <later SHA> for these files, so the diff is authoritative for what
changed." It costs one commit per round and converts a full re-read into a hunk walk.

The commit is worth it for durability alone — see
[[byte-exact-copy-via-git-plumbing]] for what it costs to recover uncommitted preparation work when a
run dies. Related: [[preflight-sibling-invalidation-cascade]],
[[forward-planner-handoff-records-to-preflight]],
[[convergence-signal-is-systematically-optimistic]].
