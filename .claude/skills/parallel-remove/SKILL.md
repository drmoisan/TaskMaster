---
name: parallel-remove
description: Remove one item from a running parallel run per the state-dependent removal behavior table — withdraw and recolor for an unstarted item, detach or abandon for an in-flight item, reject for a merged item. No default disposition is ever inferred. The abandon path runs through one deterministic CLI invocation and is hook-gated.
argument-hint: "[item] [--disposition detach|abandon]"
context: fork
agent: parallel-orchestrator
---

# Parallel Remove Skill

A user invocation (`/parallel-remove <item> [--disposition detach|abandon]`) forks the
`parallel-orchestrator` agent with this procedure in context. The item key and optional disposition
are:

$ARGUMENTS

This skill implements the mutation-protocol remove operation (spec FR2). It mutates a parallel run
that is already executing, so the pinning invariant and the recompute boundary in the
`## Mutation Protocol (F6)` section of `.claude/skills/parallel-orchestrate/SKILL.md` govern what
this operation may disturb. Read that section before applying anything here.

## Prerequisites

- A parallel run is in progress and `artifacts/orchestration/parallel-orchestrator-state.json`
  tracks its `parallel_slug`.
- `$ARGUMENTS` names exactly one item key — an integer `items[].issue_num`. Item keys are integers
  throughout this surface; there is no string key.

## Re-Derive Durable State Before Applying Anything

The checkpoint is a CACHE of durable state, not the source of truth
(`.claude/rules/parallel-orchestration.md`, Cache Doctrine). Before deciding a removal, re-derive
the target item's true state and rewrite the checkpoint from it when they disagree:

- `git worktree list --porcelain` — worktree existence and path.
- `git branch` — branch existence and name.
- `gh pr view <pr> --json state,mergedAt,headRefOid` — pull-request state and merge outcome.

The removal branch is selected by the item's state, so a stale state selects the wrong branch. An
item the checkpoint calls `scheduled` but which is really `in_flight` would be silently withdrawn
and recolored, moving work that is already running. The re-derivation is therefore mandatory.

## State-Dependent Behavior Table (Normative)

Implement this table exactly, one branch per row:

| Item state | Behavior |
| --- | --- |
| `proposed`, `admitted`, `prepared`, `scheduled` | Mark `withdrawn`, drop the vertex, recolor the unstarted subgraph (recompute). |
| `in_flight` | **Reject** unless `--disposition` is supplied. A default disposition is never inferred. |
| `in_flight` with `--disposition detach` | Let the item finish and merge on its own; the run stops tracking it. No recompute. |
| `in_flight` with `--disposition abandon` | Close the PR, remove the worktree, mark `withdrawn`. Destructive; hook-gated. No recompute. |
| `merged` | Reject; the change is already in `main`. |

An item already `withdrawn` or `blocked` is not a live removal target and is rejected as an unknown
item.

## No Default Disposition

Removing an item that is `in_flight` without an explicit `--disposition` is REJECTED. No default is
inferred, in either direction. The choice between letting running work finish and destroying it
changes what happens to a pull request and a worktree, so it belongs to the caller and never to
this procedure. Do not guess, do not prompt-and-assume, and do not pick `detach` because it is the
non-destructive option.

## Rejected Removals Change Nothing

A rejected removal — `in_flight` without a disposition, `merged`, or an unknown or already-withdrawn
item — fails fast with a specific error, appends NO `mutations[]` entry, and makes NO state change.
Do not record a partial removal, and do not record the rejection itself in `mutations[]`.

## Procedure

1. Re-derive durable state as above and resolve the item key against `items[]`.

2. Decide the removal by calling `decide_removal(item_key, items, disposition)` from
   `scripts/dev_tools/parallel_mutation_protocol.py`. The engine raises the dedicated rejection
   exception for every rejected row; surface its message and stop.

3. **Unstarted removal (recompute).** Set the item's state to `withdrawn`, drop its vertex, and
   recolor by calling `recolor_unstarted(unstarted_items, conflict_edges, pinned,
   current_generation, current_cohort=current_cohort,
   highest_pinned_cohort=highest_pinned_cohort)`. Write
   `RecolorResult.cohort_assignments` into `cohorts[]` and set the top-level `recolor_generation`
   to `RecolorResult.generation`; the generation increments by exactly one. The result names no
   pinned key, so no in-flight item moves.

   `current_cohort` is F3's top-level field, read from the re-verified durable state: the lowest
   current-generation cohort index still holding a non-terminal item. Under the per-edge barrier an
   in-flight item is not confined to that index, so `highest_pinned_cohort` — the highest
   current-generation cohort index occupied by any in-flight item — is read from the same
   re-verified state. The returned indices are ABSOLUTE and are written VERBATIM into
   `cohorts[].index`, never re-based to zero. Returned keys whose index equals `current_cohort` are
   MERGED into the single existing current-generation cohort entry at that index alongside its
   pinned members, never written as a second entry carrying the same `index`, which F3 invariant 13
   rejects.

4. **`detach` (no recompute).** Set the item's state to `withdrawn` and record
   `disposition: "detach"` in the mutation entry. `recolor_generation` is UNCHANGED: the detached
   item was pinned and was never a vertex of the unstarted subgraph, so its departure cannot change
   the induced subgraph. Perform no side effect — the item's own pull request continues to its own
   merge outcome and the run simply stops tracking it. The `closed`-mode completion predicate
   excludes withdrawn items, so the run does not wait for a detached item.

5. **`abandon` (no recompute, destructive, CLI-only).** Set the item's state to `withdrawn` and
   record `disposition: "abandon"` in the mutation entry. `recolor_generation` is UNCHANGED, for the
   same reason as `detach`. Execute the destructive side effects — closing the pull request and
   removing the worktree — through the single deterministic CLI invocation below and through nothing
   else:

   ```bash
   poetry run python scripts/dev_tools/parallel_mutation_abandon_cli.py --item <key> --disposition abandon --confirm-abandon --pr <pr-number> --worktree <worktree-path>
   ```

   Executing the abandon disposition through ad hoc `gh pr close` or `git worktree remove` commands
   is PROHIBITED. The prohibition is not stylistic: the abandon gate matches on the tokens of the
   invocation above, so an ad hoc command is not matchable and would bypass the confirmation
   contract entirely. One invocation, one item, both side effects.

6. **Append exactly one `mutations[]` entry** for a successful removal, built by
   `build_remove_entry` from `scripts/dev_tools/parallel_mutation_protocol.py`:

   | Case | `op` | `item_key` | `prior_state` | `new_state` | `disposition` | `recolor_generation` |
   | --- | --- | --- | --- | --- | --- | --- |
   | Remove, unstarted | `remove` | item key | prior state (`proposed`/`admitted`/`prepared`/`scheduled`) | `withdrawn` | null | `g` + 1 |
   | Remove, `detach` | `remove` | item key | `in_flight` | `withdrawn` | `detach` | `g` (unchanged) |
   | Remove, `abandon` | `remove` | item key | `in_flight` | `withdrawn` | `abandon` | `g` (unchanged) |

   `disposition` is non-null only on an in-flight removal and is null on the unstarted row. The `at`
   timestamp comes from the engine's injected clock seam.

7. **Validate the checkpoint** before treating the removal as applied. Run the
   `validate_orchestration_artifacts` MCP tool with `artifact_type:
   "parallel-orchestrator-state"`. A non-empty error list means the removal was applied incorrectly;
   correct the checkpoint rather than proceeding.

## Abandon Confirmation-Marker Contract

The abandon path is guarded by the PreToolUse hook
`.claude/hooks/enforce-parallel-abandon-gate.ps1` on the `Bash` matcher. The contract is:

- A Bash command carrying the disposition token for abandon MUST also carry the explicit
  confirmation marker `--confirm-abandon` in the SAME command.
- A command carrying the abandon disposition token WITHOUT the confirmation marker is DENIED. The
  deny reason is prefixed `PARALLEL_ABANDON_BLOCKED`.
- A command carrying both tokens is allowed.
- A command carrying neither is out of scope and is allowed unchanged.

The two token values are declared once each in
`scripts/dev_tools/parallel_mutation_abandon_cli.py` (the producer) and once each in the hook (the
consumer), and the seam test
`tests/scripts/dev_tools/test_parallel_abandon_token_seam.py` parses all three artifacts — the CLI,
the hook, and the invocation line in step 5 above — at run time to prove they still agree. Renaming
a token in one artifact without the identical rename in the other two fails that test. The
invocation in step 5 is the file's only executable abandon command line, and the seam test parses
that one line; do not add a second one.

When the gate denies a command, the correct response is to add the confirmation marker
deliberately, not to reformulate the command to evade the match. Reformulating to evade the gate
defeats the only mechanism protecting a destructive operation.

## Constraints

- One removal per invocation; at most one `mutations[]` entry per invocation.
- No field and no enum member is added to `mutations[]`, `items[]`, or any state or merge-status
  enum. The nine parallel enums are owned by `.claude/rules/parallel-orchestration.md` and are
  consumed, never extended.
- Neither `detach` nor `abandon` recomputes. An unstarted item previously deferred because of a
  conflict with the removed item keeps its cohort assignment: the assignment stays valid and is at
  most conservative, and no opportunistic recompute is performed.
- No in-flight item other than the removal target changes cohort or state.

## Completion Requirements

- Report the item key, the branch of the behavior table taken, the disposition recorded, the
  resulting `recolor_generation`, and the single appended `mutations[]` entry — or, for a rejected
  removal, the rejection and the explicit confirmation that nothing was appended and no state
  changed.
- For an abandon, report the CLI's exit code and both side-effect outcomes.
- Report the checkpoint validation result.
