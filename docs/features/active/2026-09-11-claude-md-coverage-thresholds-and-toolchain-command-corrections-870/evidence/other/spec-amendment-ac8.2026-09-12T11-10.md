# Spec amendment: AC8 change-footprint exclusion set

Timestamp: 2026-09-12T11-10
Author: orchestrator (issue 870 preparation run)
Defect owner: orchestrator. The defective acceptance criterion was authored in the orchestrator's own
delegation brief to prd-feature, not introduced by prd-feature or by atomic-planner.

## Finding

AC8 as originally authored required the change-footprint check to report exactly one changed path
outside this item's feature folder. That criterion could never pass.

Measured in this worktree at base commit 2405a829d6afd3b12eb7c228d57158a97cb4e2ca:

```
git status --porcelain
 D docs/features/potential/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections.md
?? docs/features/active/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections-870/
?? docs/features/potential/promoted/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections.md
```

The promotion lifecycle step deletes the potential record and creates a promoted copy of it. Both
paths lie outside the feature folder, and both are committed by the preparation step that precedes
execution, so both are present in any diff the executor takes against the Phase 0 base commit. With
only the feature folder excluded, the check reports three remaining paths rather than one, whatever
the executor does. The criterion was unsatisfiable in the failing direction rather than vacuous, so
neither the plan validator nor a reading of the criterion in isolation would surface it.

## Amendment applied to spec.md

1. The Write Set section now lists four entries rather than two. The two added entries are the
   promotion lifecycle records. Both filenames carry this item's own slug, so neither can collide
   with another concurrently scheduled item, and declaring them is truthful: they are genuinely part
   of this item's diff.
2. AC8 now names a three-entry exclusion set (the feature folder plus the two promotion lifecycle
   records), anchors the diff on the Phase 0 base commit, and requires the porcelain status companion
   in the same check. It also records why the exclusion set must include the promotion records, so a
   later editor does not narrow it back.
3. A Path Notation Convention section was added at the top of spec.md recording that only paths this
   change actually writes are wrapped in backticks, and why that is load-bearing rather than
   cosmetic.

## Reachable pass and reachable fail

The amended criterion is not vacuous. It passes only when the corrected root instructions file is the
sole remaining path. It fails if the executor edits any other production file, if it commits an
unrelated file, or if it leaves a stray untracked file outside the three excluded entries, because
the porcelain companion reports untracked paths that the name-only diff cannot.

## Propagation required

The plan holds a second copy of this assertion at task P2-T8. The amendment is not complete until
that task carries the same three-entry exclusion set. That propagation was relayed to atomic-planner
as a verbatim delta immediately after this artifact was written.
