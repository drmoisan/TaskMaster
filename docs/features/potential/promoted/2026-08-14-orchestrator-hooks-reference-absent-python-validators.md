# orchestrator-hooks-reference-absent-python-validators (Issue #555)

- Date captured: 2026-08-14
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/orchestrator-hooks-reference-absent-python-validators/ (Issue #555)
- Severity: Medium
- Discovered during: orchestration of issue #553 (CI parallel job split)

- Issue: #555
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/555
- Last Updated: 2026-08-14
## Summary

`.claude/hooks/validate-orchestrator-output.ps1` delegates its routing-contract
validation to a Python module that does not exist in this repository. The hook's
default invoker runs:

```
python -m scripts.dev_tools.validate_orchestration_artifacts \
    <ArtifactType> <CheckpointPath> --require-complete --require-model-routing
```

There is no `scripts/dev_tools/` directory in TaskMaster. `scripts/` contains only
`dev-tools/`, `vscode/`, and `temp-extract-coverage.ps1`. A repository-wide search for
`validate_orchestrator_state*` returns no results.

## Observed Behavior

Verified on 2026-08-14 in `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-14T09-01`:

- `find . -name "validate_orchestrator_state*"` (excluding `node_modules` and
  `.claude/worktrees`) returns nothing.
- `ls scripts/` returns `dev-tools/`, `vscode/`, `temp-extract-coverage.ps1`.
- The hook treats the subprocess exit code as the complete failure discriminator
  (`.claude/hooks/validate-orchestrator-output.ps1`, around line 228). A missing module
  causes `python -m` to exit non-zero, which the hook reads as a validation failure.

Consequence: any `Agent(orchestrator)` delegation that reaches the `SubagentStop` hook
would be blocked, and a model-routing gate failure would be reported under the
`MODEL_ROUTING_BLOCKED:` reason regardless of whether the checkpoint is actually valid.

The defect did not surface during issue #553 because that orchestration ran in the main
session rather than as an `Agent(orchestrator)` subagent, so the `SubagentStop` matcher
never fired.

## Related Documentation Drift

The same absent paths are cited as authoritative enforcement in checked-in rule files:

- `.claude/rules/orchestrator-state.md` names
  `scripts/dev_tools/validate_orchestrator_state.py`,
  `scripts/dev_tools/compute_complexity_floor.py`,
  `scripts/dev_tools/resolve_delegation_model.py`,
  `scripts/dev_tools/_orchestrator_state_model_routing_gate.py`, and others.
- `.claude/rules/parallel-orchestration.md` names
  `scripts/dev_tools/validate_parallel_orchestrator_state.py`,
  `scripts/dev_tools/parallel_manifest_contract.py`, and several helper modules.

None of these exist here. The working enforcement surface in TaskMaster is the
`mcp__drm-copilot__validate_orchestration_artifacts` MCP tool, which is backed by the
bundled TypeScript implementation and was confirmed working during issue #553.

## Expected Behavior

One of:

1. The Python validator modules are vendored into TaskMaster so the hook's default
   invoker resolves; or
2. The hook's default invoker is repointed at the MCP-backed validator that this
   repository actually ships; or
3. The hook fails open with a clear diagnostic when the validator is unavailable,
   rather than reporting a routing-contract failure it did not evaluate.

In every case the rule files should cite the enforcement mechanism that exists in this
repository rather than paths inherited from the reference repository.

## Impact

- `Agent(orchestrator)` delegations from `epic-planner`, `epic-orchestrator`, and
  `parallel-orchestrator` would be blocked at `SubagentStop` with a misleading reason.
- The block reason attributes the failure to model routing, sending a reader to
  investigate checkpoint contents when the actual cause is a missing module.
- Rule files assert enforcement that is not in place, which overstates the guarantees a
  reader can rely on.

## Acceptance Criteria

- [ ] `.claude/hooks/validate-orchestrator-output.ps1` resolves its routing-contract
      validation against a validator that exists in this repository.
- [ ] When the validator cannot be located, the hook emits a distinct, accurate
      diagnostic rather than `MODEL_ROUTING_BLOCKED:`.
- [ ] `.claude/rules/orchestrator-state.md` and `.claude/rules/parallel-orchestration.md`
      cite the enforcement mechanism actually present in TaskMaster.
- [ ] A test exercises the validator-absent path and asserts the diagnostic.

## Next Step

- [ ] Promote to GitHub issue (bug template)
