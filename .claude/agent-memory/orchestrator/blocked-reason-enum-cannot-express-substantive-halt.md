---
name: blocked-reason-enum-cannot-express-substantive-halt
description: checkpoint blocked_reason is a 7-member enum covering only mechanical delegation/validator failures; a halt because the plan's premise was falsified has no valid member
metadata:
  type: reference
---

`artifacts/orchestration/orchestrator-state.json`'s `blocked_reason` is validated against a fixed
vocabulary (`VALID_BLOCKED_REASONS`, in `.claude/lib/orchestrator-state/OrchestratorState.psm1`
around line 96):

```
none, spawn_agent_unavailable, delegation_launch_failed, delegate_no_receipt,
delegate_contract_incomplete, validator_failed, user_requested_stop
```

Free text is rejected with `Checkpoint has invalid blocked_reason: <value>`.

**Every member describes a mechanical failure** — an agent that would not spawn, a delegation that
would not launch, a missing or malformed receipt, a validator that failed, or an explicit user stop.
There is **no member for a substantive halt**: the case where every delegation succeeded, every
validator passed, and the orchestrator stopped because the work's own evidence falsified the plan's
premise.

**How to apply:** record `blocked_reason: "none"` to keep the checkpoint schema-valid, and put the
real reason in free-form sibling keys (a `halt` object and a `blocking_findings` object both
validate fine — the schema checks required keys, not unknown ones). Do **not** reach for a
mechanical member such as `validator_failed` to make the field look populated; it misrepresents what
happened and will mislead whoever resumes. Say plainly in the report that the enum could not express
the halt. The gap is worth promoting to an issue.

Related: the validator also demands a fuller `delegation_receipts[]` shape than is obvious —
`step`, `agent_id`, `skill_source`, `started_at`, `completed_at`, `result_signal`, `artifact_paths`
are all required per receipt. See [[completion-gate-receipt-shapes]] and
[[orchestrator-state-flat-keys-and-enum]].
