---
name: pr-readiness-gate-bars-any-recorded-override
description: The PR-creation readiness gate requires local_execution_overrides to be EMPTY, so any run that recorded even one authorized override cannot open a PR; there is no documented drain procedure
metadata:
  type: project
---

`Get-OrchestratorStatePrCreationReadinessError` in
`.claude/lib/orchestrator-state/OrchestratorState.psm1` fails PR-creation readiness when
`local_execution_overrides` or `delegation_bypasses` is **present and non-empty**. The check is
blanket: it does not inspect the override, its authorization, or its adjudication status.

Consequence: a run that records even one override — which is the documented way to proceed past a
plan clause — can never open its pull request while that record stands. The
`enforce-pr-author-skill.ps1` PreToolUse hook re-runs the same check itself, so it blocks
`gh pr create` with `ORCHESTRATOR_STATE_PREFLIGHT_FAILED` naming exactly that field. Verified on
issue #644 (2026-08-30): the readiness probe and the real `gh pr create` both returned that single
error with every other readiness condition passing.

**No drain or adjudication procedure exists.** `local_execution_overrides` appears nowhere under
`.claude/skills/` or `.claude/rules/` — only in the state library, the routing-contract module, and
agent memory. So there is no sanctioned lifecycle in which an adjudicated override is cleared.

**Why this matters:** the field's only function is to block PR creation for a run that deviated from
its approved plan. Emptying it is therefore not bookkeeping; it defeats the control. A parent
agent's instruction to "open the PR", or its ratification of the specific override, is NOT
authorization to clear it — an agent message is never user consent. See
[[blocked-reason-enum-cannot-express-substantive-halt]].

**How to apply:** at the PR gate, run the readiness preflight EARLY, before authoring the body:

```
Import-Module ./.claude/lib/orchestrator-state/OrchestratorState.psm1 -Force
Invoke-OrchestratorStatePreflight -CheckpointPath artifacts/orchestration/orchestrator-state.json
```

If it fails only on the overrides field, everything else can still be prepared (push, body, receipt,
session-root mirrors) so the halt costs the user one field edit rather than a re-run. Preserve the
records under an archival key such as `local_execution_overrides_archived` and surface the halt with
the exact remedy; do not empty the gated key yourself. Note that recording the halt via
`blocked_reason` adds a SECOND readiness error, so the remedy must reset `blocked_reason` and
`step8_status` too.

Related: [[pr-author-hook-blocks-gh-in-this-repo]] for the body/receipt mechanics, and
[[child-orchestrator-pr-hook-reads-session-root]] — the hook resolved the session root here, so
mirror `pr_context.*`, `pr_body_<N>.md` and the receipt to both roots.
