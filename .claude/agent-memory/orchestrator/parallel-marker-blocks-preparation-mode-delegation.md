---
name: parallel-marker-blocks-preparation-mode-delegation
description: In a preparation-mode run, the literal marker "Parallel mode: true" in a delegation prompt routes the pre-implementation gate to the nonexistent parallel-orchestrator-state.json and denies the delegation; use "Preparation mode: true" instead
metadata:
  type: project
---

Two hooks read the SAME literal marker out of the delegation prompt text and want DIFFERENT
checkpoints, and in a preparation-mode run their demands are incompatible.

- `enforce-model-routing-receipt.ps1` wants `Parallel mode: true` plus `issue_num:` so it resolves
  the ITEM worktree's checkpoint instead of a sibling's — see
  [[model-routing-hook-reads-canonical-path-only]].
- The pre-implementation gate reads the same marker and then evaluates readiness against
  `artifacts/orchestration/parallel-orchestrator-state.json`. In a preparation run that file does not
  exist, so the delegation is denied with:

```
PREIMPLEMENTATION_GATE_BLOCKED: this parallel-mode delegation was evaluated against
artifacts/orchestration/parallel-orchestrator-state.json, and the failed readiness predicate is
'checkpoint-absent'.
```

Verified 2026-09-12 on item #816 of slug `bugs-2026-09-11`, delegating `atomic-executor` for
`DIRECTIVE: PREFLIGHT VALIDATION ONLY`.

**Why the block is a false positive here.** The parallel-orchestrator checkpoint is owned by
`parallel-orchestrator` and is written only when parallel EXECUTION starts. A preparation-mode
orchestrator's route is `preparation`, not `parallel`; its own valid route checkpoint is
`artifacts/orchestration/orchestrator-state.json`. The gate's purpose — refuse implementation without
a validated readiness checkpoint — is satisfied by that file.

**The fix is to state the real route, not to fabricate the missing file.** Replace the parallel
markers with:

```
Preparation mode: true
route_id: preparation
issue 816
```

Write the issue number as prose (`issue 816`), not as the `issue_num:` key, so the parallel resolver
does not re-arm. Both hooks then resolve `artifacts/orchestration/orchestrator-state.json`, which in
an agent worktree is your own copy because the hook's relative default resolves against the hook
process cwd.

**Do NOT seed a synthetic `parallel-orchestrator-state.json`.** That is fabricating orchestration
receipts for a surface the run does not own, and the same rejection is already recorded in
`docs/features/parallel/bugs-2026-09-11/RESUME-STATE.md`. Record the deviation in your own checkpoint
instead, with the rejected alternative named.

**Restore the parallel markers for the EXECUTION child.** By then `parallel-orchestrator` owns a real
parallel checkpoint, so the markers are correct and the model-routing resolution they buy is the one
you want. Related: [[preimplementation-gate-reads-sibling-checkpoint]],
[[prd-feature-hook-blocks-reused-prep-worktree-topology]].
