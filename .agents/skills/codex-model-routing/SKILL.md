---
name: codex-model-routing
description: Resolve and validate the deterministic Codex topology plus the exact deployment agent, model slug, and reasoning effort. Use before spawning routed Codex workers, when scope or an orchestration ceiling changes, when resuming a delegating checkpoint, or when validating Codex topology and model receipts.
---

# Codex Model Routing

Keep model selection independent from the production-file-count route. File count
selects the engineer/orchestrator topology; this skill selects the exact deployed
agent profile.

## Resolve topology first

1. Record the implementation languages, estimated production and test file counts,
   execution context, and any cross-cutting marker.
2. Run the canonical topology resolver before choosing a logical agent:

   ```powershell
   poetry run python -m scripts.dev_tools.resolve_codex_topology `
     --language <python|powershell|csharp|typescript> `
     --production-file-count <count> `
     --test-file-count <count> `
     --execution-context <context>
   ```

   Add `--cross-cutting` when that route marker applies. Root epic entry instead
   supplies `--root-persona epic-planner` or `--root-persona epic-orchestrator`.
3. Persist the returned object in `codex_topology_receipts[]` with a non-empty
   `phase` before delegation.
4. Use the returned `logical_agent` as the input to the model resolver below.
   Do not replace a small typed-engineer result with an orchestrator, or an
   orchestrator result with a typed engineer.

Production-file count alone selects the small versus large topology: Python and
C# allow up to 3 production files, and PowerShell allows up to 2. The recorded
test-file caps govern typed-engineer execution batches without changing topology.
TypeScript has no canonical direct-mode budget and therefore fails closed to the
large orchestrator topology. Epic children always use the orchestrator topology;
epic root personas are forced independently of file count.

## Resolve a deployment

1. Read `config/orchestration-routing.json` and use its
   `codex_model_policy` block without substituting aliases.
2. Assess `complexity_band` (`C1`-`C4`) using the shared complexity scale and
   deterministic floor signals. C4 remains judgment-only.
3. Record `execution_context` as one of `standalone`,
   `epic_preparation_child`, or `epic_execution_child`.
4. Record the monotonic `orchestration_complexity_ceiling`. It may increase as
   scope is discovered, but it must not decrease during the run. When it rises,
   add `ceiling_transition` with exact `from`, `to`, and the non-empty unique
   `affected_delegation_ids` that must be re-resolved under the higher ceiling.
5. Run the canonical resolver:

   ```powershell
   poetry run python -m scripts.dev_tools.resolve_codex_deployment `
     --logical-agent <agent> `
     --complexity-band <C1|C2|C3|C4> `
     --execution-context <context> `
     --orchestration-complexity-ceiling <C1|C2|C3|C4>
   ```

6. Persist the returned object in `codex_model_routing_receipts[]` with a
   non-empty `phase` before spawning the returned `deployment_agent`.
7. Spawn the generated agent profile. Do not spawn the base alias and claim that
   a different model was selected.

## Normal routed-delegation launch binding

Before every normal nested `spawn_agent` call, resolve independently for that delegation. Validate the generated profile name, model, reasoning effort, path, and SHA-256 against the resolver result and the generated profile on disk. Add
the exact validated receipt to `codex_model_routing_receipts[]`, including its non-empty `phase` and delegation identifier, in the selected checkpoint that `SubagentStart` reads. Durably flush the selected checkpoint before launch, then launch only the resolver-returned `deployment_agent`.

Reject the launch when the receipt is late, a generic alias is supplied, the
checkpoint is ambiguous, profile validation fails, persistence or durable flush
fails, or start attestation returns `routing_valid: false`. Do not accept child output or child mutations after `routing_valid: false`; retain downstream
recorder, authority-store, mutation-gate, and stop-gate enforcement.

The route name `feature-review` resolves to the native
`feature-reviewer-<profile>` agent family; retain `feature-review` as the
receipt's logical agent name.

## Fixed routing outcomes

- C1: `gpt-5.6-luna`, low reasoning.
- C2: `gpt-5.6-terra`, medium reasoning.
- C3 standalone with a C3 ceiling: `gpt-5.6-terra`, high reasoning.
- C3 in either epic-child context, or C3 under a C4 ceiling:
  `gpt-5.6-sol`, high reasoning through the `-c3-elevated` profile.
- C4: `gpt-5.6-sol`, max reasoning.
- `epic-planner` and `epic-orchestrator`: always `gpt-5.6-sol`, ultra
  reasoning.

If the exact model is unavailable, record `model_unavailable`, leave the work
incomplete, and request a policy change. Do not silently fall back.

## Validation

Before accepting delegated results or reporting completion, validate the
checkpoint with the Codex routing gate:

```powershell
poetry run python -m scripts.dev_tools.validate_orchestration_artifacts `
  orchestrator-state artifacts/orchestration/orchestrator-state.json `
  --require-codex-topology `
  --require-codex-model-routing
```

For epic execution use `epic-orchestrator-state` with the same flag. The MCP
`validate_orchestration_artifacts` surface is the authoritative completion gate
when available.
