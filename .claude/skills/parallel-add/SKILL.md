---
name: parallel-add
description: Admit one new item into a running parallel run — preparation via a preparation-mode child orchestrator run, conflict-edge computation against all items including in-flight ones, and the admission decision that either places the item in the current cohort or defers it and recolors the unstarted subgraph. Appends exactly one mutations[] entry. In-flight items are never moved.
argument-hint: "[issue|potential-entry]"
context: fork
agent: parallel-orchestrator
---

# Parallel Add Skill

A user invocation (`/parallel-add <issue|potential-entry>`) forks the `parallel-orchestrator`
agent with this procedure in context. The issue reference or potential-entry path to admit is:

$ARGUMENTS

This skill implements the mutation-protocol add operation (spec FR1). It mutates a parallel run
that is already executing: the run's cohorts, its `recolor_generation`, and its `mutations[]`
audit log all belong to the parallel-orchestrator checkpoint, and the pinning invariant of the
`## Mutation Protocol (F6)` section of `.claude/skills/parallel-orchestrate/SKILL.md` governs what
this operation may and may not disturb. Read that section before applying anything here.

## Prerequisites

- A parallel run is in progress and `artifacts/orchestration/parallel-orchestrator-state.json`
  tracks its `parallel_slug`. This skill does not start a run; use `/parallel-plan` and
  `/parallel-run` for that.
- `$ARGUMENTS` names exactly one item: a GitHub issue number or reference, or a path to an entry
  under `docs/features/potential/`. An argument naming more than one item is rejected; admit one
  item per invocation so each admission decision and its mutation entry stay attributable.

## Re-Derive Durable State Before Applying Anything

The checkpoint is a CACHE of durable state, not the source of truth
(`.claude/rules/parallel-orchestration.md`, Cache Doctrine). Before computing any admission
decision, re-derive the run's true state and rewrite the checkpoint from it when they disagree:

- `git worktree list --porcelain` — worktree existence and path per item.
- `git branch` — branch existence and name per item.
- `gh pr view <pr> --json state,mergedAt,headRefOid` — pull-request state and merge outcome per
  item.

The in-flight set this operation reads is derived from that re-verified state. Admitting against a
stale in-flight set is the one way this operation can violate the pinning invariant, so the
re-derivation is mandatory and is not an optimization to skip when the checkpoint "looks current".

## Procedure

1. **Enter `proposed`.** Add the item to `items[]` in state `proposed` with its `issue_num` as the
   primary key. Item keys are integers throughout this surface; there is no string key.

2. **Prepare the item.** Run preparation through a preparation-mode child `Agent(orchestrator)`
   run, reusing the existing `route_id: preparation` contract UNCHANGED: promotion, research,
   `spec.md`, `user-story.md`, the atomic plan, and preflight clearance. Do not fork a variant
   contract for parallel admission. Preparation yields the item's DECLARED blast radius, and only
   the planner-computed declared radius is authoritative for scheduling. The item's lifecycle
   advances `proposed` -> `admitted` -> `prepared` during this step, recorded as item-state updates in
   `items[]` with the checkpoint's lifecycle timestamps.

3. **Compute conflict edges over ALL items, including in-flight ones.** Invoke the landed
   contention relation `conflicts(a, b, config)` from `scripts/dev_tools/compute_blast_radius.py`
   (defined in `scripts/dev_tools/_blast_radius_conflicts.py`). `a` and `b` are the two items'
   `BlastRadius` value objects, not strings, and `config` is the required parsed
   `config/blast-radius.json` mapping. Map each conflicting pair onto an `(int, int)` conflict edge
   of `items[].issue_num` values, normalized so `a < b`. Do not reimplement the relation and do not
   compute edges over the unstarted subset only: an in-flight conflict is precisely what the
   admission decision turns on.

4. **Decide admission.** Call
   `decide_admission(candidate, conflict_edges, in_flight, current_cohort_members=current_cohort_members)`
   from `scripts/dev_tools/parallel_mutation_protocol.py`.
   - `ADMIT_CURRENT_COHORT` — the candidate shares no edge with any member of the current cohort,
     pinned or unstarted. Admit it into the current cohort. NO recompute occurs and
     `recolor_generation` is unchanged — precisely because the candidate conflicts with no
     current-cohort member, so no cohort assignment needs to change.
   - `DEFER_AND_RECOLOR` — the candidate shares an edge with at least one member of the current
     cohort, pinned or not-yet-launched. Defer it to a future cohort and recolor by calling
     `recolor_unstarted(unstarted_items, conflict_edges, pinned, current_generation, current_cohort=current_cohort)`.
     The recolor is a recompute: `recolor_generation` increments by exactly one, and it places every
     unstarted item at an index at or above `current_cohort`, strictly above it when a pinned
     conflict exists.

   Derive `current_cohort_members` from the re-verified durable state, not from the cached
   checkpoint: it is the full membership of the current-generation cohort at `current_cohort`,
   INCLUDING its not-yet-launched `scheduled` members. Derive `current_cohort` from that same
   re-verified state; it is F3's top-level `current_cohort` field and is the index the pinned items
   occupy. Both matter because `max_concurrency` caps simultaneously in-flight items independently
   of cohort size and refills each freed slot from the same current cohort — see
   `## Cohort Barrier and Max-Concurrency Slot Filling` in
   `.claude/skills/parallel-orchestrate/SKILL.md` — so the current cohort durably holds `scheduled`
   members that a candidate can contend with.

   A conflict with an unstarted member of the CURRENT cohort defers the candidate and recolors,
   because the next `max_concurrency` batch would otherwise launch the two concurrently. A conflict
   with an unstarted item OUTSIDE the current cohort does not defer: the cohort barrier keeps the
   two from running concurrently, so the coloring's existing separation already resolves it.

5. **Apply the recolor result.** Write `RecolorResult.cohort_assignments` into `cohorts[]` and set
   the top-level `recolor_generation` to `RecolorResult.generation`. The result's key set equals the
   unstarted set exactly and contains no pinned key: no in-flight item changes cohort or state as a
   result of this admission. Verify that before writing, and stop rather than write a result that
   names a pinned key. The admit branch performs no recolor at all, precisely because the candidate
   conflicts with no current-cohort member.

   The returned `cohort_assignments` values are **ABSOLUTE cohort indices**. Write them VERBATIM
   into `cohorts[].index`; never re-base them to zero. When the lowest returned index equals
   `current_cohort` — the no-pinned-conflict case, where the offset is not applied — the returned
   keys at that index are **MERGED into the single existing current-generation cohort entry at
   `current_cohort`** alongside its pinned members, and are never written as a second cohort entry
   carrying the same `index`, because F3 invariant 13 requires current-generation `cohorts[].index`
   values to be unique (`scripts/dev_tools/_parallel_state_structures.py:282-305` — duplicate-index
   detection at 282-293, error emission at 301-305).

6. **Append exactly one `mutations[]` entry**, at admission-decision time, built by
   `build_add_entry` from `scripts/dev_tools/parallel_mutation_protocol.py`:

   | Case | `op` | `item_key` | `prior_state` | `new_state` | `disposition` | `recolor_generation` |
   | --- | --- | --- | --- | --- | --- | --- |
   | No-conflict admit | `add` | item key | null | `scheduled` | null | `g` (unchanged) |
   | Deferred admit | `add` | item key | null | `scheduled` | null | `g` + 1 |

   `prior_state` is null on BOTH add rows. The accompanying `prepared` -> `scheduled` transition is
   not lost and is not recorded in the mutation entry: it is recorded as an item-state update in
   `items[]`, the same mechanism that records `proposed` -> `admitted` -> `prepared` in step 2. The
   `at` timestamp comes from the engine's injected clock seam.

7. **Validate the checkpoint** before treating the admission as applied. Run the
   `validate_orchestration_artifacts` MCP tool with `artifact_type:
   "parallel-orchestrator-state"`. A non-empty error list means the admission was applied
   incorrectly; correct the checkpoint rather than proceeding.

## Constraints

- One admission per invocation, one `mutations[]` entry per successful admission. A failed
  preparation appends no entry and leaves `items[]` without the candidate.
- No field and no enum member is added to `mutations[]`, `conflict_edges[]`, `items[]`, or any
  state or merge-status enum. The nine parallel enums are owned by
  `.claude/rules/parallel-orchestration.md` and are consumed, never extended.
- This operation never moves, restates, or re-derives an in-flight item's cohort or state.
- This operation performs no destructive side effect: it closes no pull request and removes no
  worktree. Those belong to `/parallel-remove` with `--disposition abandon`.

## Completion Requirements

- Report the admitted item key, the admission outcome, the resulting `recolor_generation`, and the
  single appended `mutations[]` entry.
- Report the cohort index the item landed in, and confirm explicitly that no in-flight item's
  cohort or state changed.
- Report the checkpoint validation result.
