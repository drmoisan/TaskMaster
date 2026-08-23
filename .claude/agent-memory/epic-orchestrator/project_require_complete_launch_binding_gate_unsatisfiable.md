---
name: require-complete-launch-binding-gate-unsatisfiable
description: require_complete on an epic checkpoint demands a per-feature launch_binding with receipt FILES under artifacts/orchestration/epic-child-launches/ that no epic-orchestrator run actually writes — record the gap, never retrofit it
metadata:
  type: project
---

`validate_orchestration_artifacts` with `require_complete: true` on an
`epic-orchestrator-state` checkpoint emits **five errors per feature** for a per-feature
`launch_binding` object it expects but that no run produces:

- `launch_binding.worktree_path` must be a non-empty canonical absolute path
- `launch_binding.launch_receipt_path` must be under `artifacts/orchestration/epic-child-launches/`
- `launch_binding.launch_status_path` must be under `artifacts/orchestration/epic-child-launches/`
- `launch_binding.delegation_receipt` must be an object
- `launch_binding.model_routing_receipt` must be an object

**Why:** `artifacts/orchestration/epic-child-launches/` has never existed in this repository, and
`epic-orchestrator` writes no launch receipt or status file at delegation time. Two of the five
required fields are therefore paths to files that are never created. The launch facts *are*
recorded truthfully, in top-level `delegation_receipts[]` and `model_routing_receipts[]` — just not
in the shape the gate reads. So the gate is unsatisfiable without fabricating launch evidence after
the fact, which is the same falsification prohibition that governs `merge_status`.

**Precedent, so this is not a one-off:** the `build-ci-coverage-gate-fidelity` epic hit the
identical gate with 25 errors (five features x five), recorded it in its `epic-status.md`, and did
not close it. The `quickfiler-suite-determinism-foundation` epic hit it again with 20 errors
(four features x five) on 2026-08-22.

**How to apply:** Expect `require_complete` to fail on any epic you run, even one with every child
merged — this gate is independent of the descoped-child case and adds its errors on top. Separate
the error classes explicitly when you report: `merge_status is not merged/worktree_removed` is a
real disposition, the 5N `launch_binding` errors are this structural gap. Record the gap in the
checkpoint and in `epic-status.md`; do NOT create `epic-child-launches/` files retrospectively.
The fix belongs outside any epic: either `epic-orchestrator` writes launch receipts at delegation
time, or the gate reads the receipt arrays that already exist. Worth promoting as its own defect.

Related: [[feedback_premise_falsified_child_halt]],
[[project_epic_checkpoint_schema_gotchas]].
