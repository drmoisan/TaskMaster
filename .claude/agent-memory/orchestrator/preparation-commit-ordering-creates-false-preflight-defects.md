---
name: preparation-commit-ordering-creates-false-preflight-defects
description: In preparation mode the commit happens AFTER preflight clears, so every preflight round observes an uncommitted tree and reports the pending promotion files as a plan defect; the fix is to commit and re-run, never to make Phase 0 do the commit
metadata:
  type: feedback
---

Preparation mode orders the run as: promotion, research, documents, planning, preflight until ALL CLEAR,
**then** commit and push. So every preflight round necessarily observes a tree in which the promotion
outputs are still uncommitted. On issue #869 this produced the same false defect **twice**, from two
different preflight rounds, each time phrased as a confident blocking finding with correct evidence:

- Round 2 (`B2`): "the potential-entry deletion is unstaged; make the final commit task stage it with
  `git add -A`."
- Round 4 (`D7`): "the scoped porcelain gate returns seven entries, so the plan halts at P0-T3; make
  P0-T3 commit those seven paths."

Both observations were true of the tree at the moment of observation. Both fixes would have been wrong,
and the second would have been **actively destructive**: at execution time those paths are already
committed, so a prescribed `git commit` finds nothing staged, exits non-zero, and halts the plan at its
first substantive task. A "fix" that converts a passing gate into a guaranteed halt is worse than the
defect it closes.

**Why:** the reviewer cannot see your future commit, and nothing in the plan or the worktree tells it
that a preparation commit is scheduled. It is reasoning correctly from the only state it can observe.

**How to apply:**
- When a preflight finding's entire content is "these promotion/planning files are uncommitted",
  classify it as a transient-premise finding, not a plan defect. Overrule the proposed task edit.
- Resolve it by **making the preparation commit and re-running the confirming round**, not by arguing.
  The re-run observes an empty scoped porcelain and the finding evaporates factually rather than by
  assertion. This is cheaper and more honest than a prose debate, and it leaves the plan text untouched.
- Keep any clean-tree gate the reviewer added. On #869 the gate was doing exactly its job — it detected
  a genuinely uncommitted tree. The gate is right; only its proposed remedy was wrong.
- Tell the next round explicitly what you committed and that the plan text is unchanged, or it re-derives
  the same finding.
- Scope such a gate to the roots the plan's own assertions cover (here `.github scripts tests docs`).
  An UNSCOPED clean-tree halt is a defect in its own right: `.claude/agent-memory/**` is always dirty
  after a subagent writes a memory entry, the pre-implementation gate's exempt pathspecs do not cover it,
  so it can never be committed by a preparation run — and a plan that halts on it stops at Phase 0 every
  time. I wrote that unscoped version myself in a revision delta and preflight caught it.

**A clean-tree gate in Phase 0 fights the plan, and each fix needs another exclusion.** The unscoped
version I wrote cost three further preflight rounds on #869, because the plan necessarily dirties the
very tree the gate inspects, and each round found one more writer:

1. round 3 — the gate halts on `.claude/agent-memory/**`, which a preparation run can never commit;
2. round 5 — it halts on the Phase 0 evidence artifacts that the two tasks *before* the gate write;
3. round 6 — it still halts on the **plan document itself**, because the executor's task-completion
   protocol writes each check-off into it, and the plan file sits at the feature-folder ROOT, one level
   above the evidence directory the previous fix excluded.

That third one is the non-obvious one and it generalises: any Phase 0 gate placed after the first
check-off cannot distinguish a pre-existing plan modification from the check-off it just made. If you
put a clean-tree gate in a plan, scope it to the roots the plan does NOT write — here `.github`,
`scripts`, `tests` and the potential-features directory — rather than to a broad root plus a growing
exclusion list. Verify the exact command against the real tree before proposing it; the round-6 reviewer
did, and reported that the fixed form still matched 150 tracked files under the code roots, which is how
you show the gate can still fail rather than asserting it.

Related: [[shared-checkpoint-read-modify-write-corrupts]], [[evidence-and-lifecycle-for-every-change]],
[[convergence-signal-is-systematically-optimistic]], [[preflight-catches-vacuous-gates]].
