---
name: step-status-completed-write-locks-checkpoint
description: Setting step8/9/10_status to "completed" before a ci_gate exists makes enforce-completion-consistency.ps1 reject EVERY later edit to orchestrator-state.json, including unrelated ones
metadata:
  type: project
---

`.claude/hooks/enforce-completion-consistency.ps1` blocks any write to
`artifacts/orchestration/orchestrator-state.json` when the checkpoint "asserts completion" without a
`ci_gate` object carrying `conclusion == "success"` and a non-empty `head_sha`.

`Test-CompletionAsserted` (~line 131) returns true when **any** of these hold:

- `next_step == "complete"`, or
- `completed_steps` contains `S12_complete`, or
- **any of `step8_status` / `step9_status` / `step10_status` equals `"completed"`**.

The third condition is the trap. Marking step 8 `"completed"` the moment atomic execution finishes is
the natural thing to do, and it silently write-locks the checkpoint for the rest of the run. The error
(`COMPLETION_CONSISTENCY_BLOCKED`) names the missing `ci_gate` and reads like a completion-gate
problem, so it is easy to misdiagnose as "I need to fabricate CI evidence." You do not.

**Why:** the hook cannot tell a genuine completion claim from a status field set early, so it treats
`"completed"` on those three fields as the claim itself.

**How to apply:** while a remediation loop is open, keep `step8_status` at `in_progress` — which is
also the truthful value, since each remediation cycle modifies production code and step 8 is not
actually finished. Leave `completed_steps` alone (the monotonic hook guards it, and the hook only
looks for the literal `S12_complete` there). Set the three status fields to `"completed"` only once
real `ci_gate` evidence exists. If you are already locked out, flipping the offending status field is
a single edit that succeeds on its own because the *resulting* payload no longer asserts completion.

Note the enum: `in_progress`, not `in-progress` — see [[orchestrator-state-flat-keys-and-enum]].
Related: [[completion-gate-receipt-shapes]], [[epic-child-self-merge-step9-passed-vs-verified]].
