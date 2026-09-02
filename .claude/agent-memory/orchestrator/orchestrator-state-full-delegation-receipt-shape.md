---
name: orchestrator-state-full-delegation-receipt-shape
description: The MCP orchestrator-state validator demands eight keys per delegation receipt plus a top-level relativeFile; a minimal receipt fails, and completing a REAL delegation's fields is not receipt padding
metadata:
  type: project
---

Verified 2026-09-01 on #656 while preparing the PR-creation preflight.

`mcp__drm-copilot__validate_orchestration_artifacts` with `artifact_type: orchestrator-state` rejects
a compact delegation receipt. Each entry in `delegation_receipts.agents[]` must carry **all eight**:

`step`, `agent_name`, `agent_id`, `skill_source`, `started_at`, `completed_at`, `result_signal`,
`artifact_paths`

Plus one top-level key that is easy to miss because nothing else references it: **`relativeFile`**.
Set it to the active folder's `issue.md` path (the same convention other checkpoints on main use).

A receipt carrying only `{agent, phase, model, status}` produces one "missing key" line per absent
field per receipt — 16 errors for two receipts — which reads like a schema mismatch but is just the
compact form.

**Boundary against the padding prohibition.** The standing instruction is not to invent
`delegation_receipts` entries for lifecycle phases you never delegated, because the validator derives
the delegated-agent set from that array and then demands model-routing receipts for delegations that
never happened. Filling in the eight required fields of a delegation you *actually performed* is the
opposite operation and is required. The rule is about the number of array entries, not the
completeness of each entry.

`require_model_routing: true` then passes provided `model_routing_receipts[]` has one entry per agent
named in the array — which is exactly why an over-long array breaks a previously-passing validation.

Related: [[orchestrator-state-flat-keys-and-enum]],
[[checkpoint-receipt-namespaces-and-owner-race]], [[completion-gate-receipt-shapes]].
