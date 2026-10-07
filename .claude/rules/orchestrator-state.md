---
paths:
  - "artifacts/orchestration/*orchestrator-state.json"
  - "artifacts/orchestration/*planner-state.json"
  - "scripts/dev_tools/*orchestrator_state*"
  - "extensions/drm-copilot/src/lib/validate/orchestrator-state-*"
  - "scripts/dev_tools/compute_complexity_floor.py"
  - "scripts/dev_tools/resolve_delegation_model.py"
  - ".claude/hooks/validate-orchestrator-output.ps1"
  - ".claude/hooks/enforce-model-routing-receipt.ps1"
  - "config/orchestration-routing.json"
  - ".claude/agents/orchestrator.md"
  - ".claude/agents/epic-orchestrator.md"
  - ".claude/agents/parallel-orchestrator.md"
  - ".claude/agents/epic-planner.md"
  - ".claude/agents/parallel-planner.md"
  - ".claude/skills/orchestrate/SKILL.md"
  - ".claude/skills/epic-orchestrate/SKILL.md"
  - ".claude/skills/parallel-orchestrate/SKILL.md"
  - ".claude/skills/epic-plan/SKILL.md"
  - ".claude/skills/parallel-plan/SKILL.md"
description: Checkpoint invariants for the orchestration state artifact and the surfaces that write or validate it.
---

# Orchestrator-State Remediation-Cycle and Human-Interaction Invariants

This rule governs remediation-cycle records and the optional `human_interaction` block in the orchestrator-state checkpoint at `artifacts/orchestration/orchestrator-state.json`. It documents the invariants that must hold for each remediation cycle and for the remediation-loop review outcomes and attempt count, plus three invariants for the `human_interaction` block, so that resume and review workflows do not depend on a structurally invalid checkpoint.

## Foreign Schema Warning (do not copy verbatim)

A hardened snapshot from another repository contains a JSON Schema for the orchestrator-state artifact whose `$id` references a foreign origin (`drmoisan.github.io/mix-calculator/`). That schema MUST NOT be copied verbatim into this repository: its `$id`, its top-level required-field set, and its cycle-level `additionalProperties: false` do not match this repository's checkpoint contract. The invariants below are re-expressed here as prose and enforced by validator logic in `scripts/dev_tools/validate_orchestrator_state.py`, not by importing a foreign schema file.

This prohibition is specific to the disqualified foreign schema identified by the `drmoisan.github.io/mix-calculator/` `$id`. A schema whose `$id` is repo-local and whose required-field set and `additionalProperties` policy match this repository's checkpoint contract is not the disqualified foreign artifact; even so, the repository's enforcement mechanism remains the Python validator prose-and-logic above, not an imported schema file.

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

These invariants apply only when the checkpoint contains a top-level `human_interaction` block. A checkpoint with no `human_interaction` key (the existing checkpoint shape) is unaffected: it validates exactly as before and produces no new errors. The invariants are additive and support the autonomous-execution mandate documented in `.claude/skills/orchestrate/SKILL.md`.

## Invariants (human_interaction block)

1. **Required `requirements` list.** When `human_interaction` is present, it must be an object containing a `requirements` list. A non-object `human_interaction`, or a `requirements` value that is not a list, is a malformed block.

2. **Per-requirement `response` enum membership.** Each requirement must be an object whose `response` value is one of `scope_change`, `exception`, or `halt`. A requirement that is not an object, or whose `response` is outside this enum, is a malformed requirement.

3. **Exception requires `runbook_path`.** A requirement whose `response == "exception"` must carry a non-empty `runbook_path` string. A missing, non-string, or empty/whitespace-only `runbook_path` on an `exception` requirement is a malformed requirement.

## Blocked-Reason Vocabulary

The checkpoint key `blocked_reason` accepts exactly the twelve members below. The authoritative definition is `scripts/dev_tools/_orchestrator_state_blocked_reason.py`; the TypeScript module `extensions/drm-copilot/src/lib/validate/orchestrator-state-blocked-reason.ts` and the grouped arrays in `.claude/lib/orchestrator-state/OrchestratorState.psm1` mirror it.

| Member | Partition | Definition |
|---|---|---|
| `none` | not blocked | The run is not blocked. JSON `null` is equivalent for classification. |
| `spawn_agent_unavailable` | mechanical | Existing member; meaning unchanged. |
| `delegation_launch_failed` | mechanical | Existing member; meaning unchanged. |
| `delegate_no_receipt` | mechanical | Existing member; meaning unchanged. |
| `delegate_contract_incomplete` | mechanical | Existing member; meaning unchanged. |
| `validator_failed` | mechanical | Existing member; meaning unchanged. |
| `user_requested_stop` | mechanical | Existing member; meaning unchanged. |
| `premise_falsified` | non-mechanical | Every delegation and validator succeeded, but evidence gathered during execution falsified the premise on which the plan was built, so continuing would implement an invalid plan. |
| `external_dependency` | non-mechanical | The run cannot proceed because a system, service, runtime, or artifact outside the repository's control is unavailable or mismatched, and no in-repository remediation can resolve it. |
| `policy_hold` | non-mechanical | The run is stopped because proceeding requires a policy decision, exception, or authorization that the orchestrator is not permitted to grant autonomously. |
| `awaiting_ci` | non-mechanical | The run is waiting for a CI result that has not yet completed, and no remediation is warranted until that result is available. |
| `human_decision_required` | non-mechanical | The run requires a human to choose between alternatives or approve a direction before it can continue. |

Partition definitions:

- **Not blocked:** `none`, JSON `null`, or key absent at a gate. Plain validation still reports an absent key through the required-key check.
- **Mechanical:** the orchestration process itself did not complete a step: a tool or delegation failed, a delegate returned no or an incomplete receipt, a validator failed, or the operator stopped execution. Membership is defined by the published constant, not by the wording.
- **Non-mechanical:** every process step could proceed, but the work cannot continue for a reason that no retry or remediation cycle of the process resolves.

The classification is recoverable from `blocked_reason` alone by looking the value up in the published partition (`classify_blocked_reason` in Python, `classifyBlockedReason` in TypeScript).

Enforcement parity: the Python, PowerShell, and TypeScript validators enforce this vocabulary identically, with case-sensitive comparison. A value outside the vocabulary, including a case variant of any member, is rejected with `Checkpoint has invalid blocked_reason: <value>`. Every non-`none` member, including the five non-mechanical members, blocks completion and PR-creation readiness with the existing messages.

Relationship to `human_interaction.requirements[].response == "halt"`: a human-decision halt may appear in both places. `blocked_reason: "human_decision_required"` classifies the run's halt, and a `human_interaction.requirements[]` entry with `response: "halt"` describes the specific requirement. There is no cross-field rule between them: either may appear without the other, and neither is validated against the other.

Producer guidance for `premise_falsified`: set it when every delegation and validator succeeded but evidence gathered during execution falsified the plan's premise. The evidence path is still recorded in free-form checkpoint keys; `blocked_reason` carries the classification only.

Extension point for #484: #484 consumes `external_dependency`, `policy_hold`, `awaiting_ci`, and `human_decision_required` without renaming, removing, or re-partitioning them; any further halt class is a separate contract change that appends the literal to the non-mechanical partition in all three runtimes, updates the partition oracle and corpus, and updates this section.

## Complexity-Assessment Scope and Backward Compatibility

These invariants apply only when the checkpoint contains a top-level `complexity_assessments` array. A checkpoint with no `complexity_assessments` key (the existing checkpoint shape) is unaffected: it validates exactly as before and produces no new errors. The invariants are additive and support the two-axis model-selection mechanism documented in `.claude/skills/orchestrate/SKILL.md` (`## Model Selection`). Enforcement is the Python validator, not an imported schema.

## Invariants (complexity_assessments array)

Each entry records one assessed phase with the shape `{ phase, band, floor, signals_present[], rationale, assessed_at }`.

1. **Band enum membership.** Each entry's `band` must be one of `C1`, `C2`, `C3`, `C4`. A missing or out-of-enum `band` is a malformed entry.

2. **Band at or above floor.** Each entry must satisfy `band >= floor` using the band ordering `C1 < C2 < C3 < C4`. The floor constrains the lower bound only; a `band` below its `floor` is a malformed entry.

3. **Floor equals the computed floor.** Each entry's `floor` must equal `compute_complexity_floor(signals_present)` (the reference implementation in `scripts/dev_tools/compute_complexity_floor.py`). A `floor` that does not equal the recomputed value is a malformed entry. C4 is never floor-forced; the floor never exceeds `C3`.

4. **Non-empty `rationale`.** Each entry's `rationale` must be a non-empty string. A missing, non-string, or empty/whitespace-only `rationale` is a malformed entry.

The validator never judges the merit of the assessed band; it checks shape, floor equality, and lower-bound ordering only.

## Model-Routing-Receipt Scope and Backward Compatibility

These invariants apply only when the checkpoint contains a top-level `model_routing_receipts` array. A checkpoint with no `model_routing_receipts` key (the existing checkpoint shape) is unaffected: it validates exactly as before and produces no new errors. The invariants are additive. Enforcement is the Python validator, not an imported schema.

## Invariants (model_routing_receipts array)

Each entry records one delegation with the shape `{ agent, phase, complexity_band, fable_policy, table_model, clamped_from | null, model }`.

1. **Model equals the resolved model.** Each entry's `model` must equal `resolve_delegation_model(agent, complexity_band, fable_policy)["model"]` (the reference implementation in `scripts/dev_tools/resolve_delegation_model.py`). A `complexity_band` outside `C1`..`C4` is a malformed entry. A `model` that does not equal the resolved model is a malformed entry.

2. **Disabled-mode clamp.** Under `fable_policy == "disabled"` no entry's `model` may equal `fable`; any entry whose `table_model == "fable"` must record `clamped_from == "fable"` and `model == "opus"`. A disabled-mode entry that resolves to `fable`, or a disabled-mode `fable` table cell that does not record the clamp, is a malformed entry.

## Model-Budget Contract

The session `model_budget.fable_policy` switch is a three-way enum `disabled | available | preferred` defined in `config/orchestration-routing.json`, defaulting to `disabled`. It governs only the delegation model tier and is not a route input. `disabled` removes `fable` from the consideration set and clamps `fable` cells to `opus`; `available` applies the base `complexity_to_model` table as-is; `preferred` applies the `preferred_overlay` (which redirects only the C3 cell to `fable` for the overlay agents `atomic-planner`, `prd-feature`, `feature-review`, `task-researcher`) and leaves `atomic-executor` and `pr-author` C3 cells at `opus`. `route` is never an input to model selection.

## Require-Model-Routing Mode Scope and Backward Compatibility

The complexity-assessment and model-routing-receipt invariants above are key-gated: they run only when their key is present, so a checkpoint that omits both arrays passes at every stage. The `require_model_routing` mode adds an existence gate that closes that gap without changing the default behavior. It is an opt-in keyword on `validate_orchestrator_state_text(..., require_model_routing=False)` (CLI flag `--require-model-routing`, accepted by the dispatcher `orchestrator-state` subcommand and by the bare-module CLI; MCP parameter `require_model_routing`), defaulting off. Plain, `require_complete`, and `require_pr_creation_ready` calls are unaffected and produce byte-identical results.

## Bare-Module CLI Contract

The validator module runs directly as `python -m scripts.dev_tools.validate_orchestrator_state <path> [--require-complete] [--require-model-routing] [--require-pr-creation-ready] [--require-codex-model-routing] [--require-codex-topology]`. The dispatcher `python -m scripts.dev_tools.validate_orchestration_artifacts orchestrator-state` stays available and accepts the same flags. Exit code 0 means the checkpoint passed and the success line `orchestrator-state validation passed: <path>` is written to stdout. Exit code 1 means validation errors, written one per stderr line. Exit code 2 means the path is missing, unreadable, or not UTF-8, with one stderr diagnostic and no traceback; argparse usage errors also exit 2.

## Invariants (require_model_routing mode)

These invariants apply only when a caller passes `require_model_routing=True` and the checkpoint records at least one delegation. A checkpoint with zero delegations (no well-formed `delegation_receipts[]` entry and a `next_step` that names no delegating agent) imposes no requirement, so genuinely old, delegation-free checkpoints stay valid.

1. **Required routing receipt once delegated.** Once the checkpoint records a delegation, the set of `model_routing_receipts[].agent` must be a superset of the delegated-agent set (each well-formed `delegation_receipts[].agent_name` plus a `next_step` that names a delegating agent). A delegated agent with no matching receipt is a violation. The delegating agent set excludes `orchestrator` (the caller, not a delegated subagent).

2. **Required complexity assessment per matched phase.** Each phase named by a routing receipt whose agent is in the delegated-agent set must have a `complexity_assessments[]` entry for that phase.

3. **Per-entry consistency reused, not reimplemented.** Present receipts and assessments must satisfy the model-routing-receipt and complexity-assessment invariants above; the gate reuses `_validate_model_routing_receipts` and `_validate_complexity_assessments` and never reimplements `compute_complexity_floor` or `resolve_delegation_model`. The gate logic lives in `scripts/dev_tools/_orchestrator_state_model_routing_gate.py`; enforcement is the Python validator, not an imported schema.

The completion hook (`.claude/hooks/validate-orchestrator-output.ps1`) passes `--require-model-routing` alongside `--require-complete` and surfaces a gate failure as the `MODEL_ROUTING_BLOCKED:` block reason. The PreToolUse deterrent (`.claude/hooks/enforce-model-routing-receipt.ps1`) performs presence-only gating before a delegation. The MCP TypeScript surface performs the existence check only (delegated-agent set ⊆ routing-receipt-agent set); the Python validator remains authoritative for per-receipt correctness.

## Epic Launch-Binding Activation Scope

This section governs when the epic child launch-binding gate runs. It exists because the gate admitted the generic `require_complete` flag into an otherwise Codex-specific activation set, which made Codex-only launch evidence a universal completion requirement (issue #524).

The evidence the gate demands — `launch_receipt_path`, `launch_status_path`, `delegation_receipt`, and `model_routing_receipt` on each feature — has exactly one production writer, `.codex/scripts/launch-epic-child-wave.ps1`, on the Codex runtime. No Claude-runtime producer writes it, so an epic executed on the Claude runtime could never satisfy an unconditional gate.

1. **Unconditional under the two Codex enforcement flags.** When a caller passes `require_codex_model_routing` or `require_codex_topology`, launch-binding validation runs for every feature exactly as before, including the existing `skip_not_started` filter. Behaviour under either Codex flag is unchanged.

2. **Key-gated per feature under `require_complete` alone.** When `require_complete` is the only flag passed, the gate validates a feature only when that feature carries `launch_receipt_path` or `launch_status_path`. A feature carrying neither key is skipped and contributes zero errors.

3. **The presence test is deliberately either-key, so a partial binding still fails.** Presence means key membership, not value truthiness: a key present with an empty or null value still arms the gate. A feature carrying one launch path key and not the other is therefore validated, and the absent key produces its error. The test is never "both keys"; that spelling would let a half-written binding pass unexamined, and the partial-binding failure is the property that distinguishes this scope rule from deleting the gate.

The rest of the completion gate is unchanged. An epic whose feature is not merged, or whose `epic_merge_pr.merge_commit_sha` is missing or empty, still fails under `require_complete`.

Enforcement is validator logic plus this prose, never an imported JSON Schema. The activation scope lives in `scripts/dev_tools/_epic_orchestrator_state_launch_binding.py`, and the TypeScript parity port at `extensions/drm-copilot/src/lib/validate/epic-orchestrator-state-launch-binding.ts` reproduces it with byte-identical error strings. No schema file is authored, imported, or read for it.

## Standalone-Merge-Authorization Scope and Backward Compatibility

These invariants apply only when a checkpoint contains a top-level `standalone_merge_authorizations` array. The key may appear in any of the three checkpoints the epic merge gate reads: `artifacts/orchestration/orchestrator-state.json`, `artifacts/orchestration/epic-orchestrator-state.json`, and `artifacts/orchestration/parallel-orchestrator-state.json`. A checkpoint with no such key (the existing checkpoint shape) is unaffected: it validates exactly as before and produces no new errors. The invariants are additive and key-gated. No JSON Schema file is authored, imported, or read for the block (see the Foreign Schema Warning above); the enforcing surface is the `PreToolUse` merge gate (issue #670).

Each entry records one authorized standalone merge with the shape `{ pr_number, pr_url, issue_num, branch_name, authorized_by, authorized_at, session_id, basis, run_slug? }`. The gate consults the array only for a `gh pr merge --merge` command that names an explicit pull request number, and only after its per-feature, epic, and parallel allow conditions have all declined.

Honest disclosure: the standalone-merge authorization record is a policy-level, auditable declaration and is
not a cryptographic or security control.
`authorized_by` is a declaration that the gate does not verify, because the runtime exposes no attested agent identity at Bash `PreToolUse` time. The record is not tamper-proof: any actor able to write `artifacts/orchestration/*.json` inside the authorizing session can write one. The `session_id` cross-check binds a record to the session that wrote it, so a stale record from a previous run or a record copied between runs authorizes nothing; it does not stop same-session forgery. This is a documented accepted trade, not an unexamined gap.

## Invariants (standalone_merge_authorizations array)

1. **PR-specific block.** The value of `standalone_merge_authorizations` must be a non-empty array whose every entry is an object with a `pr_number` that is a JSON integer greater than zero. Any other shape (a boolean or other blanket flag, a string, an object, an empty array, a non-object entry, or a `pr_number` that is absent, `null`, zero, negative, fractional, a string, or an array) is rejected with `STANDALONE_MERGE_AUTHORIZATION_NOT_PR_SPECIFIC`.
2. **Binding by `pr_number`.** The gate selects the entry whose `pr_number` equals the pull request number extracted from the command. When records exist but none matches, the gate denies with `STANDALONE_MERGE_AUTHORIZATION_PR_MISMATCH`; when no checkpoint carries the key, it denies with `STANDALONE_MERGE_AUTHORIZATION_ABSENT`.
3. **`pr_url`.** Non-empty, and it must end with `/pull/` followed by the entry's own `pr_number` and nothing else.
4. **`issue_num`.** A JSON integer greater than zero.
5. **`branch_name`.** A non-empty string. It is never compared against the live branch or the working directory.
6. **`authorized_by`.** A non-empty string; presence only.
7. **`authorized_at`.** A date-time string that parses with the invariant culture, assuming and adjusting to UTC. No clock is read.
8. **`basis` and `run_slug`.** `basis` is at least 20 characters after trimming. `run_slug` is optional; when present it is a non-empty string.
9. **`session_id`.** A non-empty string equal, by ordinal comparison, to the `session_id` of the live hook envelope. An envelope without a `session_id` fails closed.

Field checks 3 through 9 run in the fixed order `pr_url`, `issue_num`, `branch_name`, `authorized_by`, `authorized_at`, `basis`, `run_slug`, `session_id`; the first failure wins and the gate denies with `STANDALONE_MERGE_AUTHORIZATION_MALFORMED` naming that field.

## Issue-Adoption Scope and Backward Compatibility

These invariants apply only when a checkpoint contains a top-level `issue_adoption` key. The key records that the orchestration adopted a GitHub issue that already existed before orchestration started (transferred, filed by hand, or created by epic decomposition), so the issue was never created through `potential_to_issue`. The key is presence-gated: a checkpoint without it validates byte-identically to before and produces no new errors. It is evaluated only by the routing-contract completion check (`validate_routing_contract` and its TypeScript and PowerShell ports). It is not a member of `REQUIRED_STATE_KEYS`, it is not read by plain (non-completion) validation, and it is not read by the PR-creation-readiness gate. No JSON Schema file is authored, imported, or read for it.

Honest disclosure: the validators perform no network I/O. They cannot confirm that the issue exists, so `evidence` and `verified_via` are an auditable declaration of the read-only verification the orchestrator performed, not proof of it. This matches the `standalone_merge_authorizations` precedent above: the record is a policy-level declaration, not a security control.

## Invariants (issue_adoption object)

The object has the shape `{ issue_num, issue_url, origin, verified_via, verified_at, evidence, waived_tools, potential_record? }`.

| Field | Rule |
| --- | --- |
| `issue_num` | A string of decimal digits without a leading zero; equal to the checkpoint `issue-num` when that is a string. |
| `issue_url` | A string ending with `/issues/` followed by `issue_num`. |
| `origin` | One of `transferred`, `filed_before_orchestration`, `epic_decomposition`. |
| `verified_via` | One of `gh_issue_view`, `gh_api_get`, `github_mcp_issue_read`. |
| `verified_at` | Present, not `null`, and not a blank string. |
| `evidence` | A string with non-whitespace content. |
| `waived_tools` | A non-empty list of non-blank tool names that includes `potential_to_issue`. |
| `potential_record` | Required when waiving a promotion-entry tool: a path under `docs/features/potential/` ending in `.md`. |

The rules run in this order, and their errors accumulate:

1. A present value that is not an object (including `null`) yields `Checkpoint issue_adoption must be an object when present.` and evaluation stops.
2. `issue_num` must be a string of decimal digits without a leading zero (a JSON number is invalid). Only when it is valid and the checkpoint `issue-num` is a different string, `issue_num` must equal the checkpoint issue-num.
3. `issue_url` must end with `/issues/` followed by a valid `issue_num`.
4. `origin` must be one of the three origin values.
5. `verified_via` must be one of the three verification values.
6. `verified_at` must be present: absent, `null`, and blank strings are rejected; any other value passes.
7. `evidence` must be a non-empty string.
8. `waived_tools` must be a non-empty list of non-blank strings; otherwise no further rule-8 or rule-9 check runs. Each entry, in list order, receives the first applicable error of: listed more than once; not in the closed waivable set; not required by the selected route after promotion-type resolution; already holding a successful MCP receipt. The list must include `potential_to_issue`.
9. Only when the rule-8 shape check passed: each distinct waived promotion-entry tool (`new_potential_entry` or `new_potential_bug_entry`) requires a valid `potential_record`.

The closed waivable set is `potential_to_issue`, `new_potential_entry`, and `new_potential_bug_entry`. `new_active_feature_folder`, `collect_pr_context`, `validate_orchestration_artifacts`, and every other tool can never be waived. All comparisons are ordinal and case-sensitive.

Fail-closed behavior: any adoption error waives nothing, so each missing receipt is still reported as `Checkpoint missing successful MCP receipt: <tool>.`. Adoption errors are placed after the receipt-loop errors and before the `local_execution_overrides` errors. The waiver affects only receipt presence: the declared `required_mcp_tools` equality check with the routing matrix is unchanged, so a checkpoint must still declare every route tool it waives.

## Enforcement

- `scripts/dev_tools/validate_orchestrator_state.py` appends one error per violated invariant when a `remediation_loop` is present, using the existing validator message style (literal, checkpoint-context prefixed). The validator returns a list of error strings and does not mutate its input.
- `scripts/dev_tools/validate_orchestrator_state.py` likewise appends one error per violated `human_interaction` invariant when a `human_interaction` key is present, using the same literal, checkpoint-context-prefixed message style. The check does not import or read any schema file.
- `scripts/dev_tools/validate_orchestrator_state.py` appends one error per violated `complexity_assessments` invariant when a `complexity_assessments` key is present, delegating to `scripts/dev_tools/_orchestrator_state_complexity.py`, which recomputes the floor via `compute_complexity_floor`. The check does not import or read any schema file.
- `scripts/dev_tools/validate_orchestrator_state.py` appends one error per violated `model_routing_receipts` invariant when a `model_routing_receipts` key is present, delegating to `scripts/dev_tools/_orchestrator_state_model_routing.py`, which recomputes the resolved model via `resolve_delegation_model`. The check does not import or read any schema file.
- `scripts/dev_tools/validate_orchestrator_state.py` appends one error per violated `require_model_routing` invariant only when the caller passes `require_model_routing=True`, delegating to `scripts/dev_tools/_orchestrator_state_model_routing_gate.py`, which reuses the complexity and model-routing per-entry validators. When the flag is not passed the gate does not run, so existing calls are byte-identical.
- The validator is consumed by the MCP tool `validate_orchestration_artifacts`; backward compatibility for existing step-based checkpoints is preserved.
- The standalone-merge authorization invariants are enforced by the `PreToolUse` merge gate (`.claude/hooks/enforce-epic-merge-gate.ps1` with its dot-sourced helpers file, and `.codex/hooks/enforce-epic-merge-gate.ps1`) at merge time. The Python checkpoint validator
  does not currently validate standalone_merge_authorizations
  entries; adding that check is recorded as follow-up FU-3.
- The `issue_adoption` invariants are enforced at completion by the routing-contract check in all three runtimes: `scripts/dev_tools/_orchestrator_state_issue_adoption.py` (Python authority), `extensions/drm-copilot/src/lib/validate/orchestrator-state-issue-adoption.ts`, and `.claude/lib/orchestrator-state/OrchestratorStateIssueAdoption.psm1`, with byte-identical error strings pinned by the shared corpus under `tests/fixtures/orchestrator_state_issue_adoption/`.
