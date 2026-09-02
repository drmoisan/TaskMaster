---
name: reviewer-enumeration-may-be-deliberately-narrow
description: Before "completing" a task-ID enumeration in a reviewer-supplied delta, check whether the omitted IDs are excluded because a different clause already binds them
metadata:
  type: feedback
---

When a preflight delta hands you a verbatim enumeration of task IDs, do not add the IDs that look
missing until you have checked what predicate the enumeration's trailing clause asserts. The
omitted IDs are frequently omitted on purpose because a *sibling* clause already binds them, and
adding them makes the trailing clause false.

**Why:** On issue #670 round 4, a delta bound section 0's host-path obligation with
`P1-T6, P2-T6, P3-T3, P4-T3 and P4-T4 reuse the $msbuild variable ... so no resolved path enters
those artifacts`. I judged the list incomplete because P3-T5, P3-T6 and P3-T10 also rebuild with
P0-T10's `& $msbuild` command, and added them. That broke the clause: those three tasks ALSO run
vstest, so a resolved `vstest.console.exe` path does enter their artifacts — and the immediately
preceding sentence had already bound them via the `<vs-install>` test-runner placeholder. The
reviewer's five were exactly the msbuild-only artifacts. I caught it in self-review and reverted
to the verbatim text.

**How to apply:** For each ID you are tempted to add, read its task body for every command it
records, not just the one the enumeration is about. If it appears in an adjacent enumeration, it
is already bound and belongs there, not here. A generalizing tail such as "where an executor
records a resolved path instead, the same placeholder applies" is the reviewer's own signal that
the list is a positive claim about a subset, not an exhaustive partition — so an apparent gap is
not a defect. Related: [[acceptance-edits-must-be-false-before-true-after]],
[[single-numeral-gates-must-name-the-role]].
