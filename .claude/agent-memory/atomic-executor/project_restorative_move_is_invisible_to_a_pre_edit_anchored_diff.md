---
name: restorative-move-is-invisible-to-a-pre-edit-anchored-diff
description: A later task that moves a block back to the position it held at the diff anchor produces zero changed lines for that block; plan rationale claiming "the diff will enumerate the moved lines" is false and is cheap to test by simulation.
metadata:
  type: project
---

When task A displaces a block of lines and a later task B moves that block back to where it
sat at the diff anchor, a diff anchored before task A enumerates **nothing** for the moved
block. The net tree state equals the anchor state for those lines, so they are neither
additions nor modifications. Any plan clause justified by "task B's move is a comment-only
edit that the `git diff -U0` span will nonetheless enumerate as changed lines" is factually
wrong, even when the clause it justifies is otherwise correct for other reasons.

Observed on issue #796: executed task P1-T4 inserted two formatter methods between a
`<summary>`/`<remarks>` pair and the member it documented; task P4-T11 moves the pair back.
Task P9-T7's comment-line exemption cited P4-T11's move as its reason. Simulation against
`c7ae69f1` showed 61 added lines, 33 of them `///`, and **no added line containing `791`** —
the moved pair's distinguishing token. The exemption was still needed, but for the 33 doc
comment lines P1-T4 itself added, not for the move.

**Why:** plan rationale is evidence (see [[418-plan-rationale-clauses-are-evidence]]), so a
false reason is a real defect even when the operative acceptance survives it.

**How to apply:** to test a claim about what a diff will enumerate after an unexecuted edit,
simulate it without touching the tree: write the anchor blob and a synthesised post-edit file
into the scratchpad and run `git diff --no-index -U0` between them, then classify the `+`
lines. This is read-only, takes one script, and converts a "very likely" into a measurement.
Related: [[preflight-moving-base-two-dot-diff-inertness-test]].
