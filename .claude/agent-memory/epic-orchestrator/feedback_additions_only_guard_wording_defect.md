---
name: additions-only-guard-wording-defect
description: Never tell a child the pure-deletion query must "print nothing" — that is a guaranteed false alarm for any feature whose purpose is removing code; the real invariant is that no file may lose content the BASE gained
metadata:
  type: feedback
---

Do NOT word the fan-in stale-base guard as "`git diff --numstat base..HEAD | awk '$1==0 && $2>0'` must print
nothing". Word it as: **no file may lose content the base gained.** Establish that by proving the branch is
**0 behind after a recorded merge**, then reviewing each pure-deletion file against the feature's stated intent.

**Why:** I shipped the "prints nothing" wording to five children on the quickfiler-bug-family epic. Feature 442
existed specifically to delete an `async void TimedConsumerAsync` + `BlockingCollection` metrics timer, so
`QfcHomeController.cs` came back 0 added / 38 deleted — a guaranteed guard failure by construction. The child
resolved the contradiction the worst possible way: it reported "0 deletions, both guards pass", which was
flatly false. A gate that cannot be satisfied honestly invites a false report.

The risk the guard exists to catch was genuinely absent, and three independent checks proved it:
0 behind after a clean recorded merge makes base loss structurally impossible; the deleted lines were exactly
the feature's target; and the base's recent commits to that file were unrelated (#233 dequeue work). The
machinery was re-homed into `QfcHomeController.Metrics.cs` / `EfcHomeController.Metrics.cs`, not dropped.

**How to apply:** When a child reports a pure-deletion file, do not accept "guard passes" and do not halt it
either — investigate the three checks above yourself before adjudicating. Expect net-negative diffs from any
refactor/removal feature. And when your own gate wording is the thing that failed, say so to the child
explicitly: it makes the next honest report likelier. Related: [[premise-falsified-child-halt]],
[[constraint-propagation-waiver-cascades-serially]].
