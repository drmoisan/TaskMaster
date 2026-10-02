---
name: preparation-clearance-record-breaks-closed-evidence-set
description: The parallel-add preparation contract commits evidence/other/preflight-clearance.<ts>.md before execution; a plan whose final gate asserts "no other file under evidence" then cannot pass. Tell the planner up front.
metadata:
  type: project
---

On the #964 preparation (2026-10-02) the plan cleared preflight round 2, then I noticed its P2-T19
asserted that no file outside the Write Set evidence list exists under `FEATURE/evidence`. The
parent's preparation contract requires committing `evidence/other/preflight-clearance.<ts>.md`
before execution, so that gate was unsatisfiable. Fixing it cost two more rounds (the fix itself
introduced three defects: a backslash Glob path that `git rev-parse HEAD:<path>` cannot resolve, a
hygiene sweep that printed totals only so a hit could not be attributed, and lower bounds that a
single missing file could pass because the record added one).

**Why:** the clearance record records the plan blob it cleared, so it is written after the plan,
and the plan never knows about it unless told.

**How to apply:** in the FIRST planner prompt of any preparation run that will commit a clearance
record, state that `evidence/other/preflight-clearance.<ts>.md` will exist, is committed before
execution, must never be modified, and must be admitted by any closed-evidence-set or file-count
gate (pin it by blob at P0, record paths in forward-slash form). Write the record only after the
final clearance, and remove any draft with `git clean -f -- <path>` before writing the final one.
Related: [[delta-application-is-itself-a-defect-source]], [[convergence-signal-is-systematically-optimistic]].
