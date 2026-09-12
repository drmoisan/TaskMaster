---
name: cobertura-line-rate-double-counts
description: This repo's committed Cobertura reports double-count per-method <lines> against the class-level <lines> block, so line-rate/branch-rate are not per-file figures and distort in BOTH directions; recompute from <line> children (open issue #441)
metadata:
  type: project
---

Never read `line-rate` or `branch-rate` off this repo's Cobertura output to make a gate decision.
The post-processing emits per-method `<lines>` blocks **and** a class-level `<lines>` block, so the
denominator is counted twice. Tracked as open issue **#441** — do not re-file it.

**Why:** It is a silent-wrong-answer defect, not a crash, and it distorts in both directions so no
correction factor exists. Proof by arithmetic: `QfcItemController.FocusAndTheme.cs` emits
`line-rate=0.756032 = 282/373` for a file that is only **326 lines long** — 373 coverable lines is
impossible; the true figure is 176/237 = 74.3% line, 40/68 = 58.8% branch. It also *deflates*:
`Conversation.cs` emits 91.18% against a true 88.24%, while `Initialization.cs` emits 90.11% against
a true 91.79%.

The dangerous direction is the false pass: `MailActions.cs` emits `branch-rate="0.75"`, which appears
to clear the 75% branch floor, when the true rate is **72.7%** — a fail.

**How to apply:**
- Recompute from the class-level `<line>` children. That block already applies the max-hits union
  across a type and its `<>c` closure class (verified: a lambda reporting `hits="0"` on a line where
  the class-level block reports `hits="1"`).
- Decide "has coverable lines" on `<line>` child count, never on `line-rate`. A declaration-only file
  reports `line-rate="0"` because it has no lines, not because it is uncovered — keying on the rate
  mis-reports every interface-only file as a 0% failure.
- Report emitted and corrected figures side by side and flag divergence, so a reviewer can see which
  gate decisions moved.
- Epic #136's "Measured Coverage Baseline" table carries the uncorrected numbers; treat it as
  directional only.

Related: [[feature-review-coverage-85-floor-trap]],
[[feedback_repowide_coverage_run_full_suite]]
