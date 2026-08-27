# Issue closure set (AC-28 input)

Timestamp: 2026-08-26T16-28

Command: not applicable (this artifact is a written closure record, not a command step)

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

Seven issues are closed by the merge of this feature: **#286, #468, #469, #470, #471, #473, #474**.
Each is mapped below to the acceptance criteria in
`docs/features/active/qfc-collection-controller-defects-468/spec.md` that cover it, and to the exact
closing-keyword line the PR body will carry.

The closing keywords appear **only** in the PR body. They are deliberately absent from every commit
message on this branch, because a closing keyword in a commit message closes the issue on merge
regardless of the surrounding wording — including a negated mention.

AC-28 is checked off by the orchestrator, which authors the PR body and observes the merge. This
artifact is the input the orchestrator must satisfy.

---

## Closure map

### Issue #286 — reentrancy counter leaked on the exceptional exit path

- Covered by: **AC-1**.
- Evidence: `evidence/regression-testing/p3-t2-fail-before.2026-08-26T09-42.md`,
  `evidence/regression-testing/p3-t3-fail-before.2026-08-26T09-48.md`,
  `evidence/regression-testing/p3-t5-pass-after.2026-08-26T09-53.md`.
- Closing line: `Fixes #286`

### Issue #468 — twelve dead members in the collection controller

- Covered by: **AC-2** (the twelve members are absent), **AC-3** (the live members are retained),
  **AC-16** (residual-risk search).
- Evidence: `evidence/qa-gates/p1-t3-dead-identifier-sweep.2026-08-26T08-45.md`,
  `evidence/qa-gates/p1-t4-live-member-nonregression.2026-08-26T08-45.md`,
  `evidence/other/p1-t1-reflective-caller-search.2026-08-26T08-25.md`,
  `evidence/qa-gates/p1-t8-suite.2026-08-26T08-45.md`.
- Closing line: `Fixes #468`

### Issue #469 — move-path defects (four defects)

- Covered by: **AC-4** (defect 1, null item controller), **AC-5** (defect 2, diagnostics array
  length), **AC-6** (defect 3, ordered move collection), **AC-7** (defect 4, undo-stack contract).
- Evidence: `evidence/regression-testing/p6-t5-pass-after.2026-08-26T10-22.md`,
  `evidence/regression-testing/p4-t7-pass-after.2026-08-26T10-12.md`,
  `evidence/regression-testing/p12-t3-pass-after.2026-08-26T11-37.md`.
- Closing line: `Fixes #469`

### Issue #470 — conversation-path defects (three defects)

- Covered by: **AC-8** (defect 1, negative-index guards), **AC-9** (defect 2, insertion-count
  reconciliation), **AC-10** (defect 3, `SetVisualDigits` guarding).
- Evidence: `evidence/regression-testing/p8-t4-pass-after.2026-08-26T10-48.md`,
  `evidence/regression-testing/p7-t12-pass-after.2026-08-26T10-39.md`,
  `evidence/regression-testing/p9-t3-pass-after.2026-08-26T11-02.md`.
- Closing line: `Fixes #470`

### Issue #471 — panel-height sign inversion on conversation collapse

- Covered by: **AC-11**.
- Evidence: `evidence/regression-testing/p10-t6-fail-before.2026-08-26T11-16.md`,
  `evidence/regression-testing/p10-t9-pass-after.2026-08-26T11-19.md`,
  `evidence/regression-testing/p10-t10-neutrality.2026-08-26T11-21.md`.
- Closing line: `Fixes #471`

### Issue #473 — background-task drain and cancellation (two defects)

- Covered by: **AC-12** (defect 1, drain window), **AC-13** (defect 2, cancellation propagation and
  single-logging).
- Evidence: `evidence/regression-testing/p11-t4-fail-before.2026-08-26T11-28.md`,
  `evidence/regression-testing/p11-t6-pass-after.2026-08-26T11-30.md`,
  `evidence/regression-testing/p5-t1-fail-before.2026-08-26T10-24.md`,
  `evidence/regression-testing/p5-t2-fail-before.2026-08-26T10-27.md`,
  `evidence/regression-testing/p5-t5-pass-after.2026-08-26T10-33.md`.
- Closing line: `Fixes #473`

### Issue #474 — parent coupling and modal readiness getter (two defects)

- Covered by: **AC-14** (defect 1, `_parent` retype and downcast removal), **AC-15** (defect 2,
  readiness inspectable without a dialog).
- Evidence: `evidence/regression-testing/p2-t6-fail-before.2026-08-26T09-14.md`,
  `evidence/regression-testing/p2-t10-pass-after.2026-08-26T09-21.md`,
  `evidence/qa-gates/p2-t9-downcast-sweep.2026-08-26T09-20.md`,
  `evidence/regression-testing/p13-t6-pass-after.2026-08-26T16-18.md`.
- Closing line: `Fixes #474`

---

## Closing-keyword block for the PR body

The PR body must carry these seven lines, verbatim, as a contiguous block:

```
Fixes #286
Fixes #468
Fixes #469
Fixes #470
Fixes #471
Fixes #473
Fixes #474
```

Issue #444 is a **sibling**, not a member of this closure set. It receives a handoff record
(`evidence/other/downstream-handoff-444.2026-08-26T16-26.md`) and must **not** appear with a closing
keyword: this feature removes one dormant instance of the duplicate-registration hazard but does not
close the class of defect, which requires a change in
`QuickFiler/Controllers/KbdActions.cs` that the scope lock forbids here.

---

## Acceptance verification

- Exactly seven issues are listed: #286, #468, #469, #470, #471, #473, #474.
- Each maps to at least one acceptance criterion: #286 → AC-1; #468 → AC-2, AC-3, AC-16; #469 →
  AC-4, AC-5, AC-6, AC-7; #470 → AC-8, AC-9, AC-10; #471 → AC-11; #473 → AC-12, AC-13; #474 → AC-14,
  AC-15.
- Every per-defect acceptance criterion AC-1 through AC-16 is claimed by exactly one issue; none is
  unassigned and none is double-counted.
- AC-17 through AC-29 are process criteria and are not attributed to a single issue.
