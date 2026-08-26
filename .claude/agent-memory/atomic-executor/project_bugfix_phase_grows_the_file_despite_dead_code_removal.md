---
name: bugfix-phase-grows-the-file-despite-dead-code-removal
description: A multi-defect bugfix plan that opens with a dead-code removal will still grow the owned file; planning-time "net reduction" estimates are systematically low, so an acceptance clause asserting the file shrinks becomes unsatisfiable
metadata:
  type: project
---

A multi-defect bugfix branch that opens with a dead-code removal usually ends with the owned file
**larger** than it started, not smaller. Never accept a plan acceptance clause that asserts the
feature "reduces" a file's excess over the 500-line cap.

**Why:** #468's spec predicted `QfcCollectionController.cs` would land at 2,120-2,180 lines after
removing twelve dead members from 2,349. The removal landed exactly as forecast (-241 → 2,108), but
the fourteen commits after it added **+329**, ending at **2,437** — about 270 lines above the top of
the estimate and 88 lines above the base commit. P15-T7's acceptance required the statement "its
excess over the cap is a pre-existing condition this feature **reduces** rather than creates," and
that statement was false, so it could not be written.

The estimate failed because it modelled fixes as small edits. They are not. In a seven-defect bugfix
each fix tends to add a guard clause, an extracted pure helper, or a seam, and every new member
carries the XML documentation the C# policy requires for a non-obvious contract. In #468 the three
AC-20 seams alone were +77 (28 + 12 + 37), and the single `#470-2` reconciliation was +107 (two new
static helpers plus a six-value diagnostic message).

**How to apply:** at preflight, reject or rewrite any acceptance clause that asserts a net line-count
reduction across a fix-bearing phase. Ask instead for the measured post-feature size plus a
statement of whether the excess is pre-existing — a recording obligation, which is always
satisfiable, rather than a directional claim, which is not. If the clause survives into execution,
record the true per-commit line-count table and mark the sub-clause NOT MET rather than asserting
something false. See [[418-plan-rationale-clauses-are-evidence]] for the general form of this
failure: unmeasured world-state claims embedded in plan prose.

Related: an open issue may already own the cap violation (#623 for this file), and its recorded
baseline goes stale by exactly the amount the feature added.
