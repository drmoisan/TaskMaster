---
name: default-to-open-mode-for-parallel-runs
description: Operator wants parallel runs planned in open mode so items can be admitted with /parallel-add while the run is in flight; also raises max_concurrency mid-preparation
metadata:
  type: feedback
---

Propose `mode: open` rather than `closed` when planning a parallel run, and confirm it rather than
defaulting silently to `closed`.

**Why:** on the `bugs-638-644-647` run (2026-08-29) the operator asked for open mode explicitly,
mid-preparation, with the stated reason "so that i can add items once running". The manifest
default is `closed` (invariant M3), so defaulting silently produces a run the operator cannot
extend without re-planning. The same operator also raised `max_concurrency` from 2 to 3 mid-flight,
so treat both knobs as things to surface early rather than settle unilaterally.

**How to apply:**

- Set `mode: open` in BOTH the checkpoint and the manifest frontmatter. Do it before manifest
  authoring; changing it afterwards means rewriting and re-validating a committed artifact.
- State the cost when confirming: open mode makes the ORCHESTRATOR completion gate stricter, not
  looser. Under `require_complete` with `mode == 'open'`, invariant 21 additionally requires a
  `mutations[]` entry with `op == 'close'`. The run will not report complete until the operator
  closes it. That gate is parallel-orchestrator scope and costs the planner nothing.
- Open mode does not change cohort seeding. Seeding still runs once at `generation: 0` over the
  prepared items; a later admission triggers a recolor at a higher generation, which is F6/F8 scope.
- **A mid-flight `max_concurrency` raise can be honoured immediately.** Re-run
  `compute-concurrency-batches.sh` at the new cap. If the raise merges the remaining waves into the
  current one, launch the newly-merged items right away — going from 2 running to 3 running is AT
  the new cap, not above it. Record both the superseded and current batch computations in the
  checkpoint so the wave history stays auditable.
- A LOWER cap mid-flight is the asymmetric case and is not symmetric to a raise: children already
  running cannot be un-launched, so record the overshoot rather than claiming compliance.

See [[parallel-artifact-authoring-gotchas]] for the manifest-side authoring traps and
[[planner-git-commits-must-be-single-bare-segments]] for the commit form.
