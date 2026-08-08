---
name: task-counts-must-be-mechanical-and-recorded
description: Never report a plan task count from memory or estimation — count `^- \[ \] \[P\d+-T\d+\]` matches, and record per-phase totals in the plan header so later passes reconcile instead of re-counting
metadata:
  type: feedback
---

Any task count reported for a plan must come from a mechanical match on
`^- \[ \] \[P\d+-T\d+\]` (Grep `output_mode: count`), never from estimation or recall. Record the
grand total AND the per-phase totals in the plan header as a `**Task Count:**` field.

A **line count** and a **unique-ID count** can legitimately differ, and the difference is diagnostic:
- line count > unique-ID count means a duplicated task ID exists (a real defect — fix it);
- they should otherwise be equal, and every phase should run `T1..Tn` with no gaps.

If a later pass reports a different number, reconcile by re-running both counts before assuming tasks
were dropped. A plan whose phases are all gap-free and were never renumbered cannot have lost a task
silently — a deletion would leave a gap or force a renumber.

**Why:** On #454 (437 tasks, 24 phases) I reported "436" for v1.0 from a unique-ID tally while a
preflight pass mechanically counted "419" in v1.1, and neither matched the true 437. The user
correctly refused to accept a reassuring number over a verified one and asked for an explicit
"was anything lost" statement. Two unverified counts cost a full revision round-trip on a plan far
too large to eyeball.

**How to apply:** applies to every plan large enough that the count is not visually obvious (roughly
>40 tasks). Report the number with its per-phase breakdown and state plainly when an earlier figure
of your own was wrong — the correction is the deliverable, not the embarrassment. See
[[plan-validator-task-id-sequential-constraint]] for why gap-free numbering is what makes the
"nothing was lost" argument sound.
