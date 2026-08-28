---
name: shared-checkpoint-read-modify-write-corrupts
description: Never read-modify-write the shared session-root orchestrator-state.json; a sibling can swap it between your read and your write, and the PR hook also restricts delegation_receipts to the promotion key only
metadata:
  type: feedback
---

Two failures that surfaced together when creating an epic-child PR.

**1. Write the worktree copy, then `cp` it to the session root. Never read-modify-write the session-root file.**

The canonical path `artifacts/orchestration/orchestrator-state.json` in the shared session
directory is rotated among live siblings, sometimes minutes apart. A loop of the form
"for each of [worktree_path, session_root_path]: load, mutate, dump" reads whichever
sibling owns the session-root file at that instant and writes YOUR keys on top of THEIR
identity. The result is a hybrid carrying their `issue-num` and `branch_name` alongside
your `pr_gate` and `ci_gate`, which corrupts their record and fails your hook.

Correct sequence: mutate only your own worktree checkpoint, then `cp` it over the
session-root path in a separate step. Archive the occupant first under a disclosed name
(`orchestrator-state.<their-issue>-displaced-by-<yours>.<ts>.json`). Siblings in this repo
also keep an `orchestrator-state.<issue>-master.json`, so check for one before assuming
your overwrite destroyed the only copy.

**Why:** observed on 2026-08-27. Between writing the checkpoint at 23:57:37Z and the PR
hook reading it seconds later, sibling 489 had swapped the file in; my write produced a
489/476 hybrid and `gh pr create` was denied with `ORCHESTRATOR_STATE_PREFLIGHT_FAILED:
Checkpoint missing required key: relativeFile`. The missing keys were mine; the file was
theirs.

**How to apply:** any time you touch the session-root checkpoint in a session whose cwd is
not your feature worktree. Re-read and re-verify identity (`issue-num`, `branch_name`)
immediately before relying on that file.

**2. `delegation_receipts` accepts only the `promotion` key.**

The same hook rejected `delegation_receipts.atomic_executor` and
`delegation_receipts.feature_review` with `unsupported key`. Per-agent receipts belong
under a top-level `agent_receipts` object keyed by the hyphenated agent name
(`atomic-planner`, `atomic-executor`, `feature-review`, `task-researcher`, `prd-feature`),
with `feature_review` as its own top-level summary object. Unknown *top-level* keys are
tolerated; only the `delegation_receipts` sub-keys are constrained.

**How to apply:** copy the shape from a completed sibling archive
(`orchestrator-state.<issue>-completed.<ts>.json`) rather than inventing one. Those
archives also carry the accepted `pr_gate`, `evidence`, `model_budget`, `skill_receipts`
and `merge_receipt` shapes.

Related: [[child-orchestrator-pr-hook-reads-session-root]],
[[model-routing-hook-reads-canonical-path-only]],
[[orchestrator-state-flat-keys-and-enum]],
[[parent-session-can-commit-into-child-worktree]]
