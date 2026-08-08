---
name: per-phase-size-gates-need-scoped-csharpier
description: A per-phase 500-line gate measured before CSharpier runs is not load-bearing — insert a scoped `csharpier format` immediately before every interim size gate and before any "byte-identical after CSharpier" assertion
metadata:
  type: feedback
---

In a multi-phase plan where CSharpier only runs at Phase 0 (baseline `check`) and the final QC loop (`format`/`check`), every interim per-phase file-size gate measures **unformatted** line counts while the final gate measures **post-format** counts. A file can pass its phase gate at 480 and fail the final gate after formatting.

**Why:** #435 (F6) preflight flagged this as Blocking. Five per-phase size gates and one "the two `LoadItemsAsync` signature lines must remain single-line after CSharpier" assertion were all unobservable at the point they were written, because CSharpier had not run in those phases.

**How to apply:**
- Insert a task immediately before each per-phase size gate: `csharpier format <explicit file list>` followed by `csharpier check` over the same paths, evidence at `<FEATURE>/evidence/qa-gates/pN-scoped-format.<ISO>.md`, acceptance `check` exit 0. List paths explicitly — never `.` (see [[csharpier-format-not-pipe-files-gate]]).
- Restate each size gate as measuring the post-format count, and have the final gate assert equality with the per-phase numbers. That equality is what proves the interim gates were load-bearing.
- Any clause of the form "X remains byte-identical / single-line **after CSharpier**" needs a scoped format of that file earlier in the same phase, or the clause is unverifiable where it sits.
- If a phase asserts a production file is byte-identical to baseline, keep that file off the scoped-format command line.
- Guard the repo-root final `csharpier format .`: add an acceptance clause to the Phase 0 baseline `csharpier check .` task that the unformatted count must be zero, else record the file list and scope the final format/check to this child's own diff set. Otherwise a sibling-owned file gets rewritten into the diff and the diff-scope gate fails with no authorized resolution.

Related: [[test-fixture-sizing-lines-per-test]].
