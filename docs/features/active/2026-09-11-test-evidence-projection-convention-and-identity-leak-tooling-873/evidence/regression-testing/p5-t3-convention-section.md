# P5-T3 — Committed Test Evidence Format Section

Timestamp: 2026-09-13T06-18
Task: [P5-T3]

Command: pwsh -NoProfile -Command '<read CLAUDE.md and count case-sensitive fixed-string matches of the exact heading line>'
EXIT_CODE: 0

```
HEADING_COUNT: 1
```

The case-sensitive match count of the heading line `## Committed Test Evidence Format` in `CLAUDE.md`
is exactly 1, where the Phase 0 baseline recorded 0. Phase 0 halted-if-nonzero on that baseline
precisely so this after-state check would be falsifiable, and it was: the baseline count was 0.

## Section body

The section body names all three permitted forms and the prohibition:

- For a coverage run, a package-level JaCoCo projection of the post-processed Cobertura document.
- For a coverage run, the existing one-line first-party coverage summary, committed alongside that
  projection.
- For a test run, a test-result summary derived from the trx document.
- The prohibition: a raw coverage collector document and a raw test-platform document are both
  prohibited, and neither may be added to git in any form, including under a feature folder's evidence
  tree.

The body also records why the projection loses nothing — it carries every figure the tool reported, and
the summary states which figures are derived rather than reported — and why this file is the rule's
home: the evidence-and-timestamp conventions document and the atomic-plan contract are both push-down
owned from an upstream repository and an edit to either is reverted on the next push-down, whereas this
file is owned here and is loaded into every agent session.

## Scope of the edit

Command: `git diff --numstat -- CLAUDE.md`
EXIT_CODE: 0
Output: `16	2	CLAUDE.md`, covering this task and P5-T4 together. Fourteen of the sixteen added lines
are this section, including its blank lines; the remaining two added and the two removed are P5-T4's
replacement of the two test-console toolchain steps.

Nothing else in the file was reformatted, reflowed or reordered. This file is also touched by a sibling
item in another cohort of this run, so the edit was kept to the one added section and the two named
step lines: a broad edit would create a merge conflict for that sibling and could silently revert its
work at fan-in.

Output Summary: The heading count is exactly 1 against a baseline of 0, the section body names all
three permitted forms and the prohibition, and the file diff is confined to the added section and
P5-T4's two step lines.
