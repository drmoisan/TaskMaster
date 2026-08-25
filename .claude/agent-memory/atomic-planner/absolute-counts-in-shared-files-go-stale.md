---
name: absolute-counts-in-shared-files-go-stale
description: A count-based acceptance clause over a file the feature does not wholly own must be lower-bound or baseline-relative, never a hard-coded absolute, or an unrelated PR landing mid-cycle makes it fail for the wrong reason
metadata:
  type: feedback
---

When an acceptance clause counts something in a file the feature does NOT wholly own (test-method
totals, occurrence counts, line counts), author it as a **lower bound** or as **baseline-relative to a
Phase 0 recorded figure** — never as a hard-coded absolute.

Two correct shapes:

- Lower bound, when the plan only needs to know nothing was deleted:
  "records a `[TestMethod]` total of at least `23` (the count observed on the tree that carries PR #610;
  the artifact records the exact observed number, and a total below 23 means gate tests were deleted)".
- Baseline-relative, when a later task must match an earlier measurement:
  "records a passed count equal to the `[TestMethod]` total recorded by `[P0-T15]` (23 on the tree that
  carries PR #610)", and for a task that adds tests, "the `[P0-T15]`-recorded total plus the six methods
  added by `[P1-T2]` ... (29 on the tree that carries PR #610)".

Keep the parenthetical concrete number. It makes the clause readable and reviewable without turning the
gate itself into an equality against a figure an unrelated PR can move.

**Do NOT soften a count that IS the substantive gate.** In the same revision, the `[TestMethod]` total
became a lower bound but the `GetConstructor` count stayed a hard equality (pre-change 4, post-change 1),
because that count is the thing the task exists to change. Softening it would have made the gate vacuous.
The test is: does the count measure the change this task makes (keep it exact), or does it merely
describe the surrounding file (make it relative)?

**Why:** feature 446 was preflight-cleared against tree b5c75151, then PR #610 landed on `main` and
appended two test methods to a gate test class the feature co-owns. Every `21` in the plan and spec became
wrong, and a hard `records 21 passed` clause would have failed a correct run. The absolute would have gone
stale again on the next unrelated PR touching that file.

**How to apply:** when authoring or revising any count-based acceptance, first ask whether the feature
owns the whole file. If sibling epic children or unrelated PRs also write it, use one of the two shapes
above. Related: [[acceptance-edits-must-be-false-before-true-after]],
[[feedback_wiring_gates_must_be_wiring_sensitive]], [[single-numeral-gates-must-name-the-role]],
[[never-pin-head-sha-as-plan-expectation]].
