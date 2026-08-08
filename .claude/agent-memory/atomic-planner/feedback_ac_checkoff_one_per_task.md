---
name: feedback-ac-checkoff-one-per-task
description: Executor preflight rejects batched AC check-off tasks — the acceptance-criteria-tracking protocol requires exactly one AC per check-off task, each with its own evidence pointer
metadata:
  type: feedback
---

AC check-off tasks in atomic plans must flip exactly ONE acceptance-criterion checkbox each, with that AC's own evidence pointer. A task that flips 2+ checkboxes (e.g. "Check off S-AC1, S-AC2, S-AC3 and U-AC1, U-AC2") is a blocking preflight finding.

**Why:** #230 preflight B4 (2026-08-07) — four batched check-off tasks (5+2+6+5 ACs) were all rejected; only the single-AC task passed. The `acceptance-criteria-tracking` protocol requires individual check-off as each AC is verified, and "individually" is enforced at task granularity, not just at ordering granularity.

**How to apply:** When distributing check-offs across phases, emit one `[P#-T#]` per AC (renumbering downstream tasks), plus one final reconciliation task verifying all boxes are `[x]`. Budget the task-count accordingly — a 19-AC feature adds ~20 check-off tasks. Related: [[plan-validator-task-id-sequential-constraint]].
