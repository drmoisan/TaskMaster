---
name: delta-application-is-itself-a-defect-source
description: Applying a preflight delta introduces NEW defects at a rate proportional to delta size — three consecutive rounds on issue 877 each shipped a contradiction between inserted branch text and an untouched sibling clause in the same task
metadata:
  type: feedback
---

Budget a preflight round to review **your own application** of the previous round's delta. Do not treat an applied delta as closed just because the reviewer's convergence line said no further rounds were expected.

**Why:** on issue #877 the defect counts by round were 16, 7, 2, 0 — and the round-2 and round-3 findings were largely *damage from applying the prior delta*, not pre-existing faults. Three consecutive applications shipped the same failure shape: the inserted text was correct in isolation and contradicted an untouched clause sitting next to it in the same task.

- Round 1's class-(b) carry-forward rule stranded a sibling span **inside the very task it was written to fix**.
- Round 2's new STOP branch told the executor both to halt and to leave a criterion unchecked at a later task it would never reach.
- Round 3's R8: a new branch recorded a non-zero exit while the task's untouched `Acceptance:` opening still demanded `EXIT_CODE: 0`.

The reviewer's `CONVERGENCE: NO FURTHER ROUNDS EXPECTED` was wrong in rounds 1, 2 and 3 — consistent with [[convergence-signal-is-systematically-optimistic]].

**How to apply:** after applying a multi-defect delta, run a confirming round **scoped to the edited tasks and every task referencing them by ID**, not a full re-review. The scoped round-4 on #877 cost a third of a full pass and returned clear. Tell the reviewer explicitly not to manufacture findings to justify the round, and tell it which regions you edited — it can then check your application rather than rediscovering the plan. Damage rate tracks delta size, so a 2-item delta needs a much lighter check than a 16-item one.

Related: [[preflight-sibling-invalidation-cascade]], [[apply-every-part-of-a-multipart-delta]].
