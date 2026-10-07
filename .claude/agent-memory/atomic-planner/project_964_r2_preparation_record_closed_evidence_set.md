---
name: project-964-r2-preparation-record-closed-evidence-set
description: Parallel-run preparation commits evidence/other/preflight-clearance.<ts>.md before execution; any closed-set "no other file under FEATURE/evidence" gate must admit it and pin it by blob
metadata:
  type: project
---

The parallel-run preparation contract commits exactly one `FEATURE/evidence/other/preflight-clearance.<yyyy-MM-ddTHH-mm>.md` BEFORE execution. A Write Set confirmation task that says "no other file exists under FEATURE/evidence" is then unsatisfiable (found by the orchestrator after preflight cleared #964 v1.1).

**Why:** the record is written by a step outside the plan, so a closed evidence set authored from the plan's own tasks never names it.

**How to apply:** at authoring time, name the record in the Write Set as a preparation-phase record not touched by any task; at P0 record its path (Glob) and `git rev-parse HEAD:<path>` blob; at the closure task require exactly one match and `git hash-object <path>` equal to that blob (working-tree hash, valid before or after commits). Hygiene sweeps scan it too: keep their counts as lower bounds and make a host hit in it a stop, not a repair, since repair would break the blob pin. Related: [[project-964-partial-split-sink-guard-plan-seams]].
