---
name: verify-line-spans-and-computed-literals
description: Any line span or computed literal a plan task writes into acceptance text must be recomputed/re-read from source first — off-by-one spans delete still-referenced declarations and guessed format-string output fails the assertion
metadata:
  type: feedback
---

Two classes of plan-task text are load-bearing enough that the executor cannot repair them, so the
planner must derive them from the source rather than estimate:

1. **Line spans in "replace L<a>-L<b> with X" tasks.** Read the full span. A property-collapse task
   whose span starts one line too early swallows the backing field declaration.
2. **Numeric literals an assertion will compare against.** Recompute the whole expression chain
   including format-string rounding.

**Why:** #437 preflight defects B6 and B2.
- B6: `[P2-T2]` said `EfcHomeController.cs` L285-290 → `internal System.Action ParentCleanup =>
  _parentCleanup;`. L285 is `private System.Action _parentCleanup;`, still assigned at L64/L100 and
  read at L349. Applying the span literally breaks the build. Correct span was L286-290, with an
  explicit "retain the field declaration at L285" clause.
- B2: `[P5-T2]` asserted `",40,0.66,"`. Production computes `120 / 3 = 40` then
  `(40 / 60d).ToString("##0.00")`. `40/60d = 0.666…` and .NET custom numeric formats round **away
  from zero**, so the emitted text is `"0.67"`. The bullet also claimed the inputs "characterize
  integer truncation", but `120 / 3` is exact, so it characterized nothing.

**How to apply:**
- For a replace-span task, read `a-1` through `b+1` and name in the task text every declaration inside
  the span that must survive.
- For an asserted literal, write the derivation into the bullet itself (`from 120 / 3 = 40 and
  (40 / 60d).ToString("##0.00") == "0.67"`) so preflight can check the arithmetic without opening the
  source.
- Never assert that inputs "characterize" a defect (truncation, rounding, off-by-one) unless those
  specific inputs actually trigger it.

See also [[research-claims-as-acceptance-clauses]],
[[never-assert-method-name-on-lambda-valued-delegate]],
[[stale-build-output-is-not-evidence-of-existence]].
