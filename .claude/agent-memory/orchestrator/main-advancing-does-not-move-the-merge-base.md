---
name: main-advancing-does-not-move-the-merge-base
description: "Reconcile against an advanced origin/main" is not an instruction to merge; measure merge-base first, because a sibling merge usually leaves it unchanged and merging is the only thing that breaks the plan's anchor
metadata:
  type: feedback
---

When a coordinator says origin/main has advanced and tells you to reconcile your item branch before
building, **measure `git merge-base HEAD origin/main` before doing anything**. A sibling item merging
into main advances the main tip but normally does NOT move your merge base: your branch was cut from
the older tip, and that older tip is still the last common ancestor. If the measured merge base equals
the `BASE-SHA` the plan's anchor task already recorded, the anchor is still valid and still
re-derivable, and the correct reconciliation is to change nothing.

**Why:** merging `origin/main` is the operation that invalidates the anchor, not main advancing. Merge
and the merge base jumps to the new main tip, so the plan's recorded BASE-SHA goes stale and every
anchored `git diff` span that transcribes it starts listing the sibling's changed files, spuriously
failing footprint and changed-line coverage gates. Verified 2026-09-13 on parallel item 838: origin/main
moved 2405a829d -> 39ce2892b when sibling 583 merged, the merge base stayed 2405a829d (exactly the
recorded BASE-SHA), and 2405a829d..39ce2892b touched only the sibling's own two QuickFiler files plus
its feature docs. Merging would have converted a no-op into a plan-wide citation invalidation for zero
benefit. [[merging-main-invalidates-plan-base-anchor]] describes how to survive a merge that is
genuinely required; this entry is the prior question that often makes it unnecessary.

**How to apply:** before delegating execution, run `git merge-base HEAD origin/main` and diff
`<merge-base>..origin/main --name-only` to measure the sibling's actual footprint against your declared
Write Set. If the merge base is unchanged and the footprints are disjoint, record a
`base_reconciliation` block in the checkpoint with the measured tip, the measured merge base, the plan's
recorded BASE-SHA, `decision: do-not-merge-origin-main`, and the overlap measurement. Then state the
decision and its rationale in the executor's prompt and tell it to STOP and report rather than merging
if it disagrees — the executor cannot be course-corrected once launched. A local merge is not needed to
build representatively either: CI evaluates the PR merge result, and disjoint footprints cannot conflict.

Corollary: re-measure at every phase boundary, not once, since another sibling may merge mid-run.
