---
name: convergence-signal-is-systematically-optimistic
description: CONVERGENCE:: NO FURTHER ROUNDS EXPECTED was emitted and wrong on two consecutive preflight rounds; a round that applies structural fixes creates state no prior pass observed, so budget three rounds when round 1 finds blocking defects
metadata:
  type: feedback
---

The `CONVERGENCE:` line that `atomic-executor` must emit with every preflight verdict is a
prediction, not a measurement, and it is **biased optimistic**. On issue #287 both round 1 and
round 2 returned `PREFLIGHT: REVISIONS REQUIRED` together with
`CONVERGENCE: NO FURTHER ROUNDS EXPECTED`, and both were wrong: round 2 found three new defects
after round 1's ten were applied.

**Why this is structural, not reviewer error.** Round 1's fixes were themselves substantive — one
tightened five separate acceptance conditions to require a green baseline suite. Those edits create
tree and plan state that no earlier pass observed, which is precisely the sibling-invalidation
mechanism the `atomic-plan-contract` warns planners about. Round 2's Defect 2 was exactly that: the
round-1 fix required a green *uninstrumented* run but left the *instrumented* baseline task with no
such requirement, so the two coverage figures could still come from different denominators. No
round-1 pass could have seen that, because the clause it invalidated did not exist until round 1
wrote it.

The corollary: a reviewer predicting convergence is reasoning about defects it has already found,
not about defects the pending fixes will create. The prediction is sound only when the fixes are
purely textual.

**How to apply.**

- Treat `CONVERGENCE: NO FURTHER ROUNDS EXPECTED` as information, never as authorization to skip the
  confirming round. Run the next round regardless.
- Budget **three** rounds, not two, whenever round 1 returns any *blocking* defect whose fix changes
  an acceptance condition rather than only its wording. Two rounds is realistic only when round 1
  finds nothing blocking.
- When relaying deltas, ask the next round explicitly to check for sibling invalidation caused by
  the previous round's own fixes, naming which conditions changed. That is what turned round 2 from
  a rubber stamp into a pass that found a real denominator defect.
- Exceeding the two-round target is worth reporting to the parent, but report it with the reason.
  Three rounds driven by genuine layered findings is a healthy result; three rounds driven by a
  reviewer reporting one defect at a time is the round-inflation failure the contract prohibits.
  Distinguish the two by whether each round's findings are *new classes* or rediscoveries.

Related: [[preflight-catches-vacuous-gates]],
[[absence-from-failure-list-is-not-a-pass-gate]],
[[multi-location-fact-residuals-drive-preflight-rounds]],
[[coverage-mode-raw-vs-processed-is-flake-sensitive]].
