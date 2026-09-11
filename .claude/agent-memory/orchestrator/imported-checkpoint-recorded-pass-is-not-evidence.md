---
name: imported-checkpoint-recorded-pass-is-not-evidence
description: A model_routing_preflight or validator pass recorded inside an imported checkpoint is a claim about a past artifact state, not proof the checkpoint you now hold validates
metadata:
  type: feedback
---

When resuming from a predecessor's checkpoint, re-run the orchestrator-state validator yourself before relying on any pass it records. Do not treat `model_routing_preflight.status: "pass"` inside the imported file as evidence that the file validates.

**Why:** on the issue #823 resume, the imported checkpoint recorded `status: "pass"` for `require_model_routing=true`, quoting `ok=true`. Running the same validation against that same imported content returned `ok=false` with 22 errors: every `delegation_receipts.agents[]` entry was missing seven required keys (`step`, `agent_name`, `agent_id`, `skill_source`, `started_at`, `result_signal`, `artifact_paths`), and an added `delegation_receipts.preflight` namespace was rejected outright — only `agents` and `promotion` are supported. The recorded pass described some earlier state, or was never re-checked after the receipts were written.

**How to apply:** immediately after importing a predecessor checkpoint, and again before terminating. Budget an edit pass to bring receipts up to the full schema; the fix is mechanical but touches every receipt. Note the same namespace restriction bites whenever you invent a new top-level key under `delegation_receipts` — put new agent records in the `agents` list instead. Related: [[checkpoint-receipt-namespaces-and-owner-race]], [[orchestrator-state-validator-divergence]], [[completion-gate-receipt-shapes]].
