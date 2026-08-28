---
name: model-routing-feature-review-is-always-fable
description: feature-review resolves to fable at EVERY band (C3 and C4), so an opus review can never be made table-conformant; the PR hook compares receipt.model to the table with no override field
metadata:
  type: reference
---

`Resolve-DelegationModel -Agent feature-review -FablePolicy preferred` returns **fable at both C3 and
C4** (C4 is the maximum band; C5 throws). There is therefore NO honest complexity band at which a
`feature-review` delegation issued at `opus` becomes conformant. Band inflation cannot rescue it.

`atomic-executor` at C3 returns `opus`; `atomic-planner` returns `fable`. Only the executor gets opus.

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

**How to apply:** choose the model from the table BEFORE delegating — `Resolve-DelegationModel` is one
cheap pwsh call. If you have already delegated off-table, do not write the table's model into the
receipt to get past the hook; that asserts a review was done by a model that did not do it. Leave
`model_routing_receipts` empty and record a top-level `model_routing_deviation` block naming the agent,
the table model, the model actually used, and the direction of the error. The hook only validates
receipts that are present, so an empty array passes while the deviation stays visible. Over-provisioning
(opus where the table says fable) cannot degrade output quality — it is a cost deviation, not a
correctness one — but say so explicitly rather than letting it look like an omission.

See [[orchestrator-state-flat-keys-and-enum]] and [[checkpoint-receipt-namespaces-and-owner-race]].
