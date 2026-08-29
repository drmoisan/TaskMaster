---
name: model-routing-feature-review-is-always-fable
description: feature-review resolves to fable only under fable_policy preferred; under available it follows the base table (C3 opus). Resolve the model before delegating - the PR hook compares receipt.model to the table with no override field
metadata:
  type: reference
---

**Corrected 2026-08-29 (issue #635 run).** The original claim in this note - that `feature-review` is
always `fable` - is true only under `fable_policy: preferred`. It is NOT policy-independent, and
acting on it under a different policy produces a receipt that does not match the table.

Measured directly with `Resolve-DelegationModel`:

| Agent | Policy | C1 | C2 | C3 | C4 |
|---|---|---|---|---|---|
| `feature-review` | `available` | haiku | sonnet | **opus** | fable |
| `feature-review` | `preferred` | haiku | sonnet | **fable** | fable |
| `atomic-executor` | `available` | haiku | sonnet | **opus** | fable |

The mechanism is in `.claude/lib/model-routing/ModelRouting.psm1`: `PREFERRED_OVERLAY_AGENTS` is
`atomic-planner`, `prd-feature`, `feature-review`, `task-researcher`, and the overlay redirects the
**C3 cell only**, and **only under the `preferred` policy**. `atomic-executor` and `pr-author` are not
overlay agents, so their C3 cell is `opus` under every policy.

Under `preferred` there is no honest band at which a `feature-review` delegation issued at `opus`
becomes conformant, because both C3 and C4 resolve to `fable`. Under `available` there is: C3.

**Why this bites:** `enforce-pr-author-skill.ps1` runs the orchestrator-state validator inside the
PreToolUse hook, and `OrchestratorStateModelReceipts.psm1` compares each
`model_routing_receipts[i].model` to `resolve_delegation_model(agent, complexity_band, fable_policy)`
and fails the PR with:

```
Checkpoint model_routing_receipts #0 model opus does not equal
resolve_delegation_model(agent, complexity_band, fable_policy) fable.
```

There is **no override, justification, or waiver field** in the receipt schema. The only fields read are
`agent`, `complexity_band`, `fable_policy`, `table_model`, `clamped_from`, `model`. The clamp fields
apply only when `fable_policy` is the disabled literal.

**How to apply:** never recall a model from this note - resolve it. One `Resolve-DelegationModel` call
against the session's actual `fable_policy` costs nothing and is the only thing the hook will accept.
The floor is computed separately: `FLOOR_SIGNAL_NAMES` is only `classifier_or_model_logic`,
`auth_or_token_handling`, `concurrency_or_ordering`, `cross_module_contract_change`, so a docs-only or
audit-only item has floor C1 and you must justify any higher assessed band in the rationale.

If you have already delegated off-table, do not write the table's model into the receipt to get past
the hook; that asserts a review was done by a model that did not do it. Leave `model_routing_receipts`
empty and record a top-level `model_routing_deviation` block naming the agent, the table model, the
model actually used, and the direction of the error. The hook only validates receipts that are present,
so an empty array passes while the deviation stays visible. Over-provisioning (opus where the table says
fable) cannot degrade output quality - it is a cost deviation, not a correctness one - but say so
explicitly rather than letting it look like an omission.

See [[orchestrator-state-flat-keys-and-enum]] and [[checkpoint-receipt-namespaces-and-owner-race]].
