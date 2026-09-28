# Preflight Round 2 Delta — Issue #742

Timestamp: 2026-09-12T17-35
Reviewer signal: PREFLIGHT: REVISIONS REQUIRED
Convergence signal: CONVERGENCE: NO FURTHER ROUNDS EXPECTED
Plan under review: docs/features/active/2026-09-02-quickfiler-date-time-format-missing-invariant-culture-742/plan.2026-09-12T16-09.md

## Outcome of round 2

Round 2 performed a full-document pass and confirmed as correct: the rewritten evidence-artifact
discipline paragraph, the AC11 mapping, the coverage-runner self-review bullet, both corrected spec
citation ranges, all three newly added supporting citations, all seventeen acceptance-criteria mappings,
all eleven baseline discovery-count controls re-run against the tree, the residual-count arithmetic under
the corrected regular expression, and the absence of any branch-coordination task.

It found exactly one defect, carrying two faults in a single sentence of the planner's self-review
record, both introduced by the round-1 replacement text itself:

1. A backtick leak. The bullet backticked the non-Metrics partial-class file of the QfcHomeController
   class, which this plan's own scope-boundary paragraph and the spec's non-goals section both name as
   out of scope. The change-footprint harvester collects whitespace-free backticked tokens and has no
   notion of polarity, so that path would have been wrongly attributed to this item's write footprint.
   This is the same defect class round 1 found in the same block, reintroduced by the fix for it.
2. A path error. The bullet cited a bare relative path for the assembly-info file. The file exists only
   at its project-qualified path; nothing exists at the bare path.

## Disposition

The reviewer supplied verbatim replacement text and stated that it had checked that replacement against
the same rule it remediates. The orchestrator applied that text verbatim, with no paraphrase and no
additional edit, rather than spending a planner round on a single-sentence textual correction. The
rationale is that the previous revision round produced five self-initiated planner edits beyond the
supplied delta, each of which became a new unreviewed region requiring another review pass; applying
reviewer-authored text directly avoids adding such a region.

The applied region is therefore text the round-2 reviewer wrote and verified. Provenance is recorded here
because the edited passage is the planner's own declaration block and the edit was not made by the
planner.

## Orchestrator verification after applying

- The plan validator returns ok with no warnings, which also confirms the file retained LF line endings.
- A fixed-string search for the backticked out-of-scope path across the plan and the spec prints no line
  and exits 1.
- Discovery-count control for that negative result: the same search form, run for a backticked in-scope
  path, printed 8 hits in the plan and 9 in the spec and exited 0. The search can therefore match, so the
  zero result is a real observation rather than a search that matches nothing for an unrelated reason.

## Reviewer observation deliberately NOT acted on

Round 2 recorded one non-blocking observation and explicitly declined to require a delta for it: in the
two coverage tasks, the instruction to delete the raw-output directory sits after the transcription step,
so on a failing attempt the restart clause returns control before the deletion is reached and a stray raw
file can persist on disk between attempts. The reviewer noted that the directory is ignored by version
control, that no gate can observe the condition, and that the next successful attempt overwrites and then
deletes the same path.

No change was made. A reviewer-declined optional change is not adopted on the orchestrator's own
initiative, because an elective edit widens the delta and the substitute text would itself be unreviewed.
