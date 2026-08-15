---
name: acceptance-edits-must-be-false-before-true-after
description: Every acceptance clause added or rewritten during a preflight revision must be verified FALSE against the pre-edit file and TRUE only after the task's edit; also verify quoted anchors are the heading that OPENS each region
metadata:
  type: feedback
---

When applying preflight revision deltas, test each rewritten acceptance clause for non-vacuity: read the actual pre-edit file and confirm the clause is **false before** the task's edit and **true after** it. A clause that already holds at branch head is a no-op gate and hides the very defect it was written to catch.

**Why:** In the #494 iteration-1 revision pass, P3-T7's acceptance was rewritten to "the string `below 80 percent` is replaced by wording that states the same 80 floor" plus "mentions no `85` and no `75`". Both clauses were already satisfied by `.claude/hooks/validate-feature-review-coverage.ps1` before any edit (line 29 already read `below 80 percent`; the `.SYNOPSIS`/`.DESCRIPTION` block already contained neither numeral), so the three substantive facts the task body mandated went untested. Iteration 2 had to re-fix it. The same pass also had to fix a scope-lock gate (P2-T19) whose first sentence was false at branch head because `.claude/agent-memory/` is already dirty — see [[agent-memory-is-tracked-scope-git-gates]].

**How to apply:** For each acceptance clause, name the concrete string or condition that is absent pre-edit. Prefer clauses keyed to newly added facts ("statements (ii), (iii), (iv) are absent from the pre-edit block") over clauses keyed to what is already present. Require the artifact to record pre-edit and post-edit text when the delta is prose. Related failure modes: [[zero-hit-grep-gates-need-carveouts]] (unsatisfiable by construction) and [[diff-gates-need-a-commit-task]] (passes vacuously with no commit).

**Companion rule — anchor a region by the heading that opens it.** #494 P0-T5 listed three `CLAUDE.md` regions (181-208, 377-386, 397-402) against a quoted-anchor list shifted by one: 181-208 was anchored on a bullet 13 lines inside it, and 377-386 and 397-402 were each anchored on the *next* region's heading (one of them outside the region entirely). Re-location then produces the wrong spans and the disjointness gate silently passes on the wrong text. Pair each region with the heading at its first line and state the terminator ("ends at the line before the next `##`/`###` heading").
