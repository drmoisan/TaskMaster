---
name: checkpoint-receipt-namespaces-and-owner-race
description: delegation_receipts accepts ONLY {agents, promotion} with 8 required keys per agent row; and the shared checkpoint can change owner between your archive and your write
metadata:
  type: reference
---

Two things that cost a denied PR and an un-archived sibling record on the #501 run.

## delegation_receipts accepts exactly two namespaces

`OrchestratorStateReceipts.psm1` rejects every top-level key outside `{agents, promotion}`:

```
Checkpoint delegation_receipts object contains unsupported key: feature_review
Checkpoint delegation_receipts object contains unsupported key: followups
```

- `promotion` may carry only `potential_entry`, `issue`, `feature_folder`.
- `agents` must be a **list**, and each row needs all 8 of `step`, `agent_name`, `agent_id`,
  `skill_source`, `started_at`, `completed_at`, `result_signal`, `artifact_paths`.

Put anything else (follow-up issue lists, review verdicts, finding dispositions) at the checkpoint's
TOP level under your own key, not inside `delegation_receipts`. Top-level keys are not enum-checked.

## The shared checkpoint can change hands mid-operation

`artifacts/orchestration/orchestrator-state.json` at the session root is shared by every live child.
On this run it changed owner **twice inside two minutes** (476 to 489). Archiving the holder and then
writing as two separate steps archived 476 while 489 had already taken it, so 489 was displaced with no
archive.

**How to apply:** re-read `issue-num` and archive in the SAME operation that writes, not in an earlier
one — read the current owner, copy it to `orchestrator-state.<N>-displaced-by-<mine>.<ts>.json` if it is
not yours, then write, all in one command. Do not rely on an ownership check from a minute ago.

Mitigating fact worth knowing: siblings keep their own `orchestrator-state.<N>-master.json`, so a
displaced sibling usually loses nothing — verify the master's `last_updated` before treating a
displacement as damage. Disclose it in `concurrency_notes` either way.

## MCP path conventions contradict each other

- `validate_orchestration_artifacts` needs a **workspace-RELATIVE** `artifact_path`; it joins onto
  `workspace_root`, so an absolute path yields a doubled path and `ENOENT`.
- the promotion tool needs an **ABSOLUTE** `potential_path` (see
  [[potential-to-issue-needs-absolute-path]]).

Same server, opposite conventions. Check which one you are calling.

See [[model-routing-feature-review-is-always-fable]] and
[[shared-checkpoint-read-modify-write-corrupts]].
