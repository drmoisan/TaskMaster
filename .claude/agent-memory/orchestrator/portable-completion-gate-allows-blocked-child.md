---
name: portable-completion-gate-allows-blocked-child
description: CORRECTED 2026-08-24 — the portable PowerShell completion gate is now FULL --require-complete parity, not lenient; it also demands required_agents/required_skills/required_mcp_tools arrays inside the checkpoint matching the routing matrix, with a bug-route tool swap.
metadata:
  type: project
---

**This memory previously said the portable gate was lenient. That is no longer true.** Issue #475
rewrote `.claude/hooks/validate-orchestrator-output.ps1` so the portable PowerShell path is the ONLY
path: the interpreter-subprocess leg and its capability probe are both gone. `scripts/dev_tools/`
does not exist in TaskMaster, but that no longer buys any leniency.

For `artifact_type: orchestrator-state` the hook runs
`Test-OrchestratorStateCompletionReadiness` (`.claude/lib/orchestrator-state/OrchestratorStateCompletion.psm1`),
described in its own header as a row-by-row port of the Python surface
`--require-complete --require-model-routing`. It composes step statuses, `blocked_reason`, the
`pr_gate` and `ci_gate` contracts, phase completeness, the routing contract, the preparation
terminal contract, and the model-routing gate. Only `epic-orchestrator-state` and
`parallel-orchestrator-state` get the lenient structural check (exists / parses / object root).

**The non-obvious requirement: the checkpoint must carry the route's required-* lists itself.**
`Get-OrchestratorStateRoutingContractError` compares three checkpoint arrays against
`config/orchestration-routing.json` and emits, for a mismatch or absence:

```
Checkpoint required_agents must match routing matrix for route <route>.
Checkpoint required_skills must match routing matrix for route <route>.
Checkpoint required_mcp_tools must match routing matrix for route <route>.
```

So `required_agents`, `required_skills`, and `required_mcp_tools` are top-level checkpoint keys, not
just config. **Bug-route swap:** when `promotion-type` is exactly `"bug"`, every
`new_potential_entry` occurrence in the route's `required_mcp_tools` is substituted with
`new_potential_bug_entry`, preserving matrix order. The substituted list drives BOTH the exact-match
check and the receipt-presence loop, so record `new_potential_bug_entry` in the checkpoint array and
in `mcp_call_receipts` — never the feature-type name. See
[[completion-gate-receipt-shapes]] for the `evidence` key that receipts also need
(`skill_receipts` need `skill` + `required: true` + non-blank `evidence`; `mcp_call_receipts` need
`tool` + `ok: true` + non-blank `evidence`).

**Preparation route is explicitly modelled.** `Get-OrchestratorStatePreparationTerminalError` is
value-gated on the RAW route value being exactly `preparation`
(`route_id` when the KEY is present, else `path_selected`). It then requires `next_step` to equal the
preparation sentinel and ALL SIX `step5..10_status` keys to read exactly `not-applicable`. A
preparation run therefore passes the completion gate cleanly without asserting completion.

**How to apply:** stop assuming a partial or halted child slides past this hook. Run the gate
yourself before terminating — it is a two-line PowerShell call and it names each violated invariant:

```
Import-Module ./.claude/lib/orchestrator-state/OrchestratorStateCompletion.psm1 -Force
Test-OrchestratorStateCompletionReadiness -CheckpointPath artifacts/orchestration/orchestrator-state.json
```

`ExitCode 0` with empty `Output` is the pass. Related:
[[orchestrator-state-flat-keys-and-enum]], [[orchestrator-state-validator-divergence]],
[[blocked-reason-enum-cannot-express-substantive-halt]],
[[checkpoint-bootstrap-blocked-by-its-own-gate]].
