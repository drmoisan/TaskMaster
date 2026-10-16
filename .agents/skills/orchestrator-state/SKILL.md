---
name: orchestrator-state
description: Orchestrator-state remediation-cycle and human-interaction invariants.
---

# Converted rule

Source: legacy Claude rule `orchestrator-state`.

# Orchestrator-State Remediation-Cycle and Human-Interaction Invariants

This rule governs remediation-cycle records and the optional `human_interaction` block in the orchestrator-state checkpoint at `artifacts/orchestration/orchestrator-state.json`. It documents the invariants that must hold for each remediation cycle and for the remediation-loop review outcomes and attempt count, plus three invariants for the `human_interaction` block, so that resume and review workflows do not depend on a structurally invalid checkpoint.

## Foreign Schema Warning (do not copy verbatim)

A hardened snapshot from another repository contains a JSON Schema for the orchestrator-state artifact whose `$id` references a foreign origin (`drmoisan.github.io/mix-calculator/`). That schema MUST NOT be copied verbatim into this repository: its `$id`, its top-level required-field set, and its cycle-level `additionalProperties: false` do not match this repository's checkpoint contract. The invariants below are re-expressed here as prose and enforced by validator logic in `scripts/dev_tools/validate_orchestrator_state.py`, not by importing a foreign schema file.

This prohibition is specific to the disqualified foreign schema identified by the `drmoisan.github.io/mix-calculator/` `$id`. A schema whose `$id` is repo-local and whose required-field set and `additionalProperties` policy match this repository's checkpoint contract is not the disqualified foreign artifact; even so, the repository's enforcement mechanism remains the Python validator prose-and-logic above, not an imported schema file.

## Portable Handoff Projection Invariants

A provider-native destination checkpoint materialized from the portable handoff
contract must retain the following linkage:

- `provider`, `checkpoint_expression`, and `destination_projector` identify the
  destination expression and selected adapter.
- `plan-path` and `next_step` equal the portable envelope's exact plan path and
  recorded lifecycle transition. The destination must not rediscover a plan or
  replace either value from local convention.
- `portable_handoff` retains the handoff ID, envelope SHA-256, latest history
  entry SHA-256, adapter identity, source validator, identity and binding
  fields, source checkpoint/archive facts, exact plan proof, lifecycle,
  capabilities, and scheduler context.
- Historical source receipts remain opaque references under
  `portable_handoff.source.expression.historical_receipts`. They must not be
  rewritten as destination-provider receipts.
- `destination_evidence` starts as `pending_first_delegation` with an empty
  receipt list. Destination routing, topology, model, and receipts may be
  recorded only for the first new delegation after checkpoint materialization.

The portable lifecycle permits only the registered state transitions:
`legacy_v1` to migration, `preparation_complete` to
`prepared_to_atomic_execution`, `validated` to destination materialization,
`materialized` to `atomic_execution`, and a bounded scheduled-child return from
an authorized child execution phase. An attempted replay of a completed phase
is invalid.

A failed contract, binding, capability, authority, plan, replay, dirty-worktree,
candidate, archive, or replacement check must produce the deterministic blocked
result and leave the source checkpoint authoritative. A blocked result must
retain the primary `HANDOFF_*` code and affected paths where applicable; it must
not record a completed transition or destination delegation.

For parallel and epic children, `return_to_scheduler` accepts only a result
whose run, item, parent checkpoint path/hash, scheduler owner, child execution
owner, return contract, plan hash, child checkpoint hash, and result hash match
the envelope. The child may return that bounded result but may not assume
cohort or wave ordering, barriers, fan-in, integration, cleanup, or parent
completion authority.

## Scope and Backward Compatibility

These invariants apply only when the checkpoint contains a top-level `remediation_loop`; the per-cycle invariants apply when it has a `cycles` array, and the review-outcome and attempt-count invariants apply when their keys are present. A checkpoint with no `remediation_loop` (the existing step-based checkpoint shape) is unaffected: it validates exactly as before and produces no new errors. The invariants are additive.

## Invariants (per remediation cycle)

1. **Non-empty `plan_path`.** Each cycle's `plan_path` must be a non-empty string. A missing value, a non-string value, or an empty/whitespace-only string is a malformed cycle.

2. **Execution requires cleared preflight.** A cycle's `execution_status` may be in `{in_progress, complete, failed}` only when that cycle's `preflight.final_status` is exactly `'clear'`. Any other preflight status with one of those execution statuses is a malformed cycle (execution was recorded before preflight cleared).

3. **Exit gate requires zero blocking findings.** When a cycle's `exit_condition_met == true`, its `blocking_count` must be `0`. A non-zero `blocking_count` with `exit_condition_met == true` is a malformed cycle (the exit gate was marked satisfied while blocking findings remained).

4. **`candidate_applied` is a boolean (R5).** When a cycle object carries `candidate_applied`, its value must be a JSON boolean, and any other value is a malformed cycle.

5. **A completed attempt requires completed execution (R6).** A cycle whose `candidate_applied == true` must have `execution_status == "complete"`, and `candidate_applied == true` with any other execution status is a malformed cycle.

6. **`opened_by_review` references a remediation review (R11).** When a cycle object carries `opened_by_review`, its value must be a non-negative integer (not a boolean) that indexes an object entry of the `remediation_loop.review_outcomes` list whose `verdict` is `REMEDIATION_REQUIRED`, and a value that is not such an integer, is out of range, or references an outcome with any other verdict is a malformed cycle.

7. **`completed_attempts` counts completed attempts (R7).** When `remediation_loop.completed_attempts` is present, it must be a non-negative integer (not a boolean) equal to the number of object cycles whose `candidate_applied` is `true` (zero when `cycles` is absent or not a list), and any other value is a malformed loop.

## Invariants (remediation_loop.review_outcomes)

These invariants apply only when `remediation_loop` contains a `review_outcomes` key. The orchestrator appends one entry per review, including a `PASS` review, in the shape `{"verdict": <verdict>, "findings": [{"remediability": <class>}, ...]}`, where `findings` lists the review's blocking findings only. A halt or wait verdict records no remediation cycle.

Review verdicts (exact, case-sensitive):

| Verdict | Condition over the classes of the review's blocking findings |
|---|---|
| `PASS` | No blocking findings. |
| `REMEDIATION_REQUIRED` | At least one `autonomous` finding. |
| `HALT_NON_REMEDIABLE` | No `autonomous` finding, and at least one `external_dependency`, `policy_hold`, or `human_decision_required` finding. |
| `AWAITING_CI` | At least one finding, and every finding is `awaiting_ci`. |

Remediability classes (exact, case-sensitive) and the `blocked_reason` member (#523) that a halt or wait records for each class:

| Class | Meaning | `blocked_reason` member |
|---|---|---|
| `autonomous` | Repository remediation can resolve the finding without a human decision. | none (remediation loop) |
| `external_dependency` | A system, service, runtime, or published artifact outside the repository is unavailable or mismatched. | `external_dependency` |
| `policy_hold` | Proceeding requires a policy decision, exception, or authorization that the orchestrator may not grant. | `policy_hold` |
| `awaiting_ci` | The finding resolves when a CI result that has not completed becomes available. | `awaiting_ci` |
| `human_decision_required` | A human must choose between alternatives or approve a direction. | `human_decision_required` |

Derivation order. The verdict is a pure function of the findings' classes, evaluated in this order: no findings yields `PASS`; any `autonomous` finding yields `REMEDIATION_REQUIRED`; any `external_dependency`, `policy_hold`, or `human_decision_required` finding yields `HALT_NON_REMEDIABLE`; otherwise the verdict is `AWAITING_CI`.

1. **`review_outcomes` is a list of objects (R8).** `review_outcomes` must be a list, and each entry must be an object. A non-list value or a non-object entry is malformed.

2. **Verdict and class membership (R9).** Each outcome's `verdict` must be a string among the four verdicts, its `findings` must be a list (a missing key is not a list), and each finding must be an object whose `remediability` is a string among the five classes. Comparisons are case-sensitive, so a case variant such as `Pass` or `Autonomous` is malformed.

3. **Verdict matches its findings (R10).** When an outcome passes the membership checks, its `verdict` must equal the verdict that the derivation order yields for its findings' classes. A mismatch is a malformed outcome.

## Human-Interaction Scope and Backward Compatibility

These invariants apply only when the checkpoint contains a top-level `human_interaction` block. A checkpoint with no `human_interaction` key (the existing checkpoint shape) is unaffected: it validates exactly as before and produces no new errors. The invariants are additive and support the autonomous-execution mandate documented in `.agents/skills/orchestrate/SKILL.md`.

## Invariants (human_interaction block)

1. **Required `requirements` list.** When `human_interaction` is present, it must be an object containing a `requirements` list. A non-object `human_interaction`, or a `requirements` value that is not a list, is a malformed block.

2. **Per-requirement `response` enum membership.** Each requirement must be an object whose `response` value is one of `scope_change`, `exception`, or `halt`. A requirement that is not an object, or whose `response` is outside this enum, is a malformed requirement.

3. **Exception requires `runbook_path`.** A requirement whose `response == "exception"` must carry a non-empty `runbook_path` string. A missing, non-string, or empty/whitespace-only `runbook_path` on an `exception` requirement is a malformed requirement.

## Enforcement

- `scripts/dev_tools/validate_orchestrator_state.py` appends one error per violated invariant when a `remediation_loop` is present, using the existing validator message style (literal, checkpoint-context prefixed). The validator returns a list of error strings and does not mutate its input.
- `scripts/dev_tools/validate_orchestrator_state.py` likewise appends one error per violated `human_interaction` invariant when a `human_interaction` key is present, using the same literal, checkpoint-context-prefixed message style. The check does not import or read any schema file.
- The validator is consumed by the MCP tool `validate_orchestration_artifacts`; backward compatibility for existing step-based checkpoints is preserved.
