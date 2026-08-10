---
name: parallel-close
description: Terminate an open-mode parallel run. Rejected while any item is in flight, in which case nothing is appended and no state changes. A successful close appends one run-scoped mutations[] entry and performs no recompute. This is the only way an open-mode run terminates; open mode never auto-completes.
argument-hint: "[parallel-slug]"
context: fork
agent: parallel-orchestrator
---

# Parallel Close Skill

A user invocation (`/parallel-close <parallel-slug>`) forks the `parallel-orchestrator` agent with
this procedure in context. The parallel slug of the run to terminate is:

$ARGUMENTS

This skill implements the mutation-protocol close operation (spec FR3). It terminates an
`open`-mode run, which has no other termination signal: the mode-dependent completion semantics in
the `## Mutation Protocol (F6)` section of `.claude/skills/parallel-orchestrate/SKILL.md` state that
an `open`-mode run never auto-completes. Read that section before applying anything here.

## Prerequisites

- A parallel run is in progress and `artifacts/orchestration/parallel-orchestrator-state.json`
  tracks the `parallel_slug` named in `$ARGUMENTS`.
- The run's `mode` is `open`. A `closed`-mode run completes through its completion predicate — every
  non-withdrawn item `merged` or `worktree_removed` — and needs no close. Report that and stop
  rather than closing a `closed`-mode run to force completion.

## Re-Derive Durable State Before Applying Anything

The checkpoint is a CACHE of durable state, not the source of truth
(`.claude/rules/parallel-orchestration.md`, Cache Doctrine). Before deciding the close, re-derive
the run's true per-item state and rewrite the checkpoint from it when they disagree:

- `git worktree list --porcelain` — worktree existence and path per item.
- `git branch` — branch existence and name per item.
- `gh pr view <pr> --json state,mergedAt,headRefOid` — pull-request state and merge outcome per
  item.

The close gate turns entirely on whether any item is in flight, so a stale in-flight set decides
the gate wrongly. Closing while an item is genuinely running would abandon that work implicitly,
which is exactly what the gate exists to prevent. The re-derivation is mandatory.

## Procedure

1. Re-derive durable state as above and resolve the run's `items[]`.

2. **Gate the close on no item being in flight.** Call `decide_close(items)` from
   `scripts/dev_tools/parallel_mutation_protocol.py`. It raises the dedicated rejection exception
   carrying EVERY in-flight key, so the rejection names all the work that must finish first rather
   than one item at a time. On rejection: report the blocking keys and stop. A rejected close
   appends NO `mutations[]` entry and makes NO state change — do not record the attempt.

3. **Append exactly one run-scoped `mutations[]` entry** for a successful close, built by
   `build_close_entry` from `scripts/dev_tools/parallel_mutation_protocol.py`:

   | Case | `op` | `item_key` | `prior_state` | `new_state` | `disposition` | `recolor_generation` |
   | --- | --- | --- | --- | --- | --- | --- |
   | Close | `close` | null (run-scoped) | null | null | null | `g` (unchanged) |

   The entry is run-scoped, so `item_key` is null: the close acts on the run, not on an item. The
   `at` timestamp comes from the engine's injected clock seam.

4. **Do not recompute.** Run termination changes no cohort assignment, so `recolor_generation` is
   UNCHANGED and `cohorts[]` is not rewritten. Do not call `recolor_unstarted` as part of a close.

5. **Stop admitting.** After the close is recorded, the run accepts no further `/parallel-add`. The
   close record is the run's final mutation; nothing may be appended to `mutations[]` after it.

6. **Validate the checkpoint** before treating the close as applied. Run the
   `validate_orchestration_artifacts` MCP tool with `artifact_type:
   "parallel-orchestrator-state"`. A non-empty error list means the close was applied incorrectly;
   correct the checkpoint rather than proceeding.

## Constraints

- One close per run, one `mutations[]` entry per successful close.
- No field and no enum member is added to `mutations[]` or to any state or merge-status enum. The
  nine parallel enums are owned by `.claude/rules/parallel-orchestration.md` and are consumed, never
  extended.
- The close performs no destructive side effect: it closes no pull request and removes no worktree.
  An item whose work should be destroyed must be removed first through `/parallel-remove` with
  `--disposition abandon`, which is also what makes the close gate passable.
- The close does not change any item's state. Items that never started remain in their recorded
  states; the close records that the run stopped admitting, not that those items were withdrawn.

## Completion Requirements

- Report the `parallel_slug`, the gate outcome, and the single appended `mutations[]` entry — or,
  for a rejected close, the full list of in-flight keys and the explicit confirmation that nothing
  was appended and no state changed.
- Report the unchanged `recolor_generation`.
- Report the checkpoint validation result.
