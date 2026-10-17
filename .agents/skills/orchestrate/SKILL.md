---
name: orchestrate
description: Route a repository request through the deterministic orchestration workflow for feature, bug, research, planning, execution, and review handoffs.
---

# Orchestrate Skill

This skill frames root-session intake and deterministic deployment for end-to-end feature or bug delivery.

## Entry-Point Contract

The root session owns the read-only intake, production-file budget estimate, route selection,
and deployment decision. Work inside the applicable language budget remains a single-feature
small route, but implementation is delegated to the complexity-specific typed-engineer profile.
Over-budget, cross-cutting, mixed-language, or unsupported standalone work is deployed to the
complexity-specific `orchestrator-<profile>` agent, which owns checkpoint updates, lifecycle
sequencing, specialist delegation, and completion gating. Epic planning and execution use only
the forced `epic-planner` and `epic-orchestrator` personas through their root skills.

Agent profile selection is operational, not advisory. Do not execute a large standalone route in
the root thread and do not implement a small route in the coordinating thread.

## Prerequisites

Before proceeding, the orchestrator must:

1. Read `AGENTS.md` for repository tone policy and architectural context.
2. Read applicable `.agents/skills/` files for the languages in scope.
3. Read the policy files listed in the compliance reading order section of `AGENTS.md`.
4. Read the `execution-environment` skill (`.agents/skills/execution-environment/SKILL.md`) before delegating any worker. Delegation prompts and plans reference that brief instead of restating tool routes, command-text restrictions, the batch budget, hook context, or CI shape.

## Checkpoint Handling

On every invocation, the main session must:

1. Read `artifacts/orchestration/orchestrator-state.json` to check for existing state.
2. If a valid checkpoint exists with a matching objective, resume from the recorded `next_step`.
3. If no checkpoint exists or the objective is new, begin the orchestration lifecycle from the start.
4. Read `config/orchestration-routing.json` before route selection and copy the
   selected route's required agents, skills, and MCP tools into checkpoint
   state.

## Prepared-State Portable Handoff Intake

Treat a portable prepared-state checkpoint as a resume request, not as a new
orchestration intake. Before delegation:

1. Validate the portable envelope through the published, workspace-explicit
   orchestration authority. Require the repository, workspace, branch lineage,
   issue, feature folder, work mode, scheduler context, capabilities, and exact
   next transition to match the current checkout. Supply the complete
   caller-controlled independent expected context on every call to
   `resolve_orchestration_topology`, `resolve_provider_routing`, and
   `transition_prepared_orchestration`: `expected_repository_id`,
   `expected_workspace_root`, `expected_branch`, `expected_source_head_sha`,
   `allowed_head_relationship`, `expected_issue_number`,
   `expected_feature_folder`, `expected_work_mode`, `expected_plan_path`, and
   `expected_plan_sha256`. Derive each value from the destination checkout and
   the caller's own record. A call that omits a value, or that copies one from
   the envelope under validation, is rejected before any service invocation.
2. Prove the plan from the independently expected repository-relative path and
   raw-byte SHA-256 the caller supplied in `expected_plan_path` and
   `expected_plan_sha256`. Do not search for another plan, rewrite the path,
   follow a symlink outside the repository, substitute a newer plan, or read
   the plan identity from the envelope.
3. Use `transition_prepared_orchestration` as the only authority permitted to
   materialize the provider-native destination checkpoint. A dry run may
   validate and project the destination state but must not archive or replace
   checkpoint bytes.
4. Preserve the ordered completed phases and reject any request to replay one
   of them. Do not rerun promotion, research, feature-document authoring,
   atomic planning, or preflight when the envelope records them as complete.
5. Preserve source-provider model, profile, topology, launch, and receipt data
   as opaque historical evidence. Resolve Codex topology, model routing, and
   launch evidence only for the first new destination delegation after
   materialization; never synthesize Codex receipts for source-provider work.

For an ordinary handoff whose recorded next transition is `atomic_execution`,
resume the exact approved plan through the ordinary Codex atomic-executor path
at `atomic_execution`. Do not invoke a parent scheduler or rediscover lifecycle
work. Any contract, authority, plan, replay, or materialization failure returns
the deterministic blocked result before destination delegation.

## Epic Entry Boundary

This standalone workflow must not invoke `epic-planner` or `epic-orchestrator`. If intake names
an epic manifest, requests epic planning, or requires multi-feature epic execution, stop before
delegation and report exactly `EPIC_ENTRY_REQUIRES_ROOT`. Direct the user to root-session
`epic-plan`, `epic-run`, or `epic-orchestrate` as appropriate. Both epic personas delegate to
ordinary orchestrators; permitting an orchestrator-originated epic invocation would create an
invalid recursive delegation chain. The Codex root-provenance hooks enforce the stronger
root-only policy and use `EPIC_INVOCATION_ORIGIN_BLOCKED` for unauthorized starts.

## Axis 1 Deployment Topology

Apply the deterministic production-file axis before model selection:

- Inside the applicable language budget: use the small route and delegate implementation to
  `python-typed-engineer-<profile>`, `powershell-typed-engineer-<profile>`,
  or `csharp-typed-engineer-<profile>`. The typed-engineer delegation and result must have a
  receipt. TypeScript has no canonical direct-mode budget, so standalone TypeScript work fails
  closed to the large orchestrator topology.
- Outside the applicable budget, cross-cutting, mixed-language, or unsupported: deploy
  `orchestrator-<profile>` and let that agent run the large path.
- Epic planning: deploy the forced `epic-planner` Astra/Ultra persona.
- Prepared or manual epic execution: deploy the forced `epic-orchestrator` Astra/Ultra persona.

Only the production-file limit selects inside versus outside the language topology budget. The
test-file estimate remains in the receipt and governs typed-engineer batching; it does not change
the selected topology. The `<profile>` is produced by the independent C1-C4 resolver after the
topology is known. File count does not choose a model, and complexity does not change the
small/large result.
Persist the deterministic topology resolver output in `codex_topology_receipts[]` before running
the model resolver or spawning the selected logical agent.

## Preparation Mode

A parent prompt containing the literal marker `Preparation mode: true` selects only
`route_id: preparation`. This route is the planning phase of `epic-plan`, not a reduced execution
route.

- Copy the exact required agents, skills, and MCP tools from the central `preparation` route.
- Perform promotion through the MCP surface, research, `spec.md`, `user-story.md`, atomic
  planning, and atomic-executor preflight only.
- Run single-pass preflight against the same plan path (`### Single-Pass Preflight` in
  `atomic-plan-contract`): trivial defects are fixed in place, the plan returns to the planner at
  most once with the full `PREFLIGHT-DEFECT:` list, and a second substantive result halts with
  `blocked_reason: "validator_failed"`. The preflight directive goes in the delegation prompt,
  never in the plan file.
- Commit the prepared feature folder and approved plan to the worktree branch.
- Stop with `completed_steps` containing `S3_promotion` and `S4_atomic_planning`,
  `next_step: "S5_atomic_execution"`, all execution-through-CI step statuses exactly
  `not-applicable`, and `blocked_reason: "none"`.
- Do not edit production code, execute the plan, author or edit a PR, run feature review, monitor
  CI, set `next_step: "complete"`, record `S12_complete`, or claim feature completion.
- Premise-falsified halt: use `blocked_reason: "premise_falsified"` when every delegation and
  validator succeeded but evidence gathered during execution falsified the plan's premise. Still
  record the evidence path in free-form checkpoint keys; `blocked_reason` carries only the
  classification. The preparation terminal checkpoint above still sets `blocked_reason: "none"`.
  The full `blocked_reason` vocabulary and its partition are in `.claude/rules/orchestrator-state.md`.

Only the literal JSON Boolean `false` in the route configuration disables the CI requirement.
Missing, malformed, string-valued, or unknown route data fails closed. The preparation mutation
hook is a deterrent; the MCP completion validator is authoritative.

## Codex Model Deployment

The deterministic size route selects topology. The independent C1-C4 assessment selects the
checked-in Codex deployment agent. Before every delegation:

1. Resolve and persist the topology receipt from languages, file counts, context, and route
   markers before selecting the logical agent.
2. Record the phase assessment, deterministic floor, signals, rationale, execution context, and
   monotonic orchestration complexity ceiling.
3. Resolve and persist the provider-aware model-routing receipt with logical and deployment agents,
   model, reasoning effort, and C3 overlay fields.
4. Spawn the exact deployment agent recorded in the receipts.
5. Require the `SubagentStart` model attestation to match the receipt before accepting mutation
   or completion.

C3 defaults to Sol/High when it is the standalone orchestration ceiling. It elevates to
Astra/High only for epic preparation/execution children or when a C4 sibling sets the ceiling to
C4. If the required model/profile is unavailable, record `model_unavailable` and stop without a
silent fallback.

## Read-Only Intake and Route Selection Gate

Before any lifecycle MCP call, the main session must complete a read-only scope
assessment. This gate is the read-only scope assessment for orchestration
intake. It includes policy reads, checkpoint reads, route config reads,
objective and scope assessment, language/file assessment, route selection, and
route metadata persistence.

The route-selection gate must be complete before calling any of:

- `new_potential_entry`
- `new_potential_bug_entry`
- `potential_to_issue`
- `new_active_feature_folder`

Until this gate is complete, the main session may read repository files and
checkpoint state, but must not create lifecycle entries, promote issues, create
active feature folders, edit implementation files, run formatters or tests,
stage files, commit files, or delegate implementation.

## Route-Derived Work Mode

The main session must derive `${work-mode}` from the selected route before
lifecycle automation starts:

- small route -> `minor-audit`
- large feature route -> `full-feature`
- large bug route -> `full-bug`

The selected `route_id`, exact route metadata, and derived `${work-mode}` must
be persisted in `artifacts/orchestration/orchestrator-state.json` before any
lifecycle MCP call or implementation action. Route metadata must include the
selected route's `required_agents`, `required_skills`, and
`required_mcp_tools` copied from `config/orchestration-routing.json`.

## Lifecycle Branch Sequencing

After route metadata is persisted and before potential entry creation, the main
session must create or verify a slug-only pre-issue branch:

- `${pre-issue-branch}`: `${promotion-type}/${short-name}`

The pre-issue branch must be derived only from `${promotion-type}` and
`${short-name}` because no numeric issue number exists yet. The main session
must create or verify this branch before calling `new_potential_entry` or
`new_potential_bug_entry`.

After potential entry creation, `potential_to_issue` must return a numeric
`${issue-num}` before the main session renames the branch to the final issue
branch:

- `${final-branch}`: `${promotion-type}/${short-name}-${issue-num}`

The branch rename to `${final-branch}` must complete before calling
`new_active_feature_folder`.

## Pre-Implementation Gate

The pre-implementation gate must pass before any edits, formatters, tests,
staging, commits, or implementation delegation. This gate covers edits, formatters, tests, staging, commits, and implementation delegation. The main session must verify and persist all of the following before implementation can begin:

- checkpoint route metadata is present and matches the selected route;
- selected `${work-mode}` is present and matches the selected route;
- lifecycle readiness is complete for the selected path;
- branch state records `${pre-issue-branch}`, `${final-branch}`, and branch
  rename status where lifecycle setup is required;
- required MCP receipts exist for completed lifecycle operations.

If any required item is missing, implementation is blocked until the checkpoint
and lifecycle state are corrected.

## Plan-Path Resolution Gate

After active feature folder creation and before any planning delegation, the
main session must resolve `${plan-path}` from the active feature folder:

1. Enumerate existing `${feature-folder}/plan*.md` files in deterministic
   filename order.
2. If one or more files exist, persist `${plan-path}` as the first existing
   file and require every planner and executor handoff to use that exact path.
3. If no `plan*.md` file exists, create exactly one canonical target path using
   the repository's feature-folder plan naming convention, persist that path,
   and reuse it for all revisions.
4. Do not default to `${feature-folder}/plan.md` when a timestamped scaffolded
   plan already exists.
5. If checkpoint state names a different plan path than the resolved existing
   plan file, correct the checkpoint before planner delegation. Do not create a
   second plan artifact to satisfy an incorrect checkpoint value.

## Pre-Implementation Violation Handling

If an implementation action is attempted before a required orchestration gate
passes, the main session must stop implementation and persist a blocked
checkpoint state. The required checkpoint outcome is blocked checkpoint state.
The checkpoint or companion artifact must record:

- violated gate name;
- attempted action;
- known mutated files, if any;
- corrective next step;
- current route metadata, lifecycle, branch, and MCP receipt state.

After a pre-implementation gate violation, the main session must not continue
implementation. It may only record the violation, restore or reconcile state
when policy permits, and resume from the corrective orchestration step.

## Hard Enforcement Boundary

The hard completion boundary for Codex orchestration is the deterministic
orchestrator-state validator exposed through the `drm-copilot` MCP server, not
a Codex lifecycle hook. Before any DONE transition, PR creation gate, or final
completion report, the orchestrator must validate the canonical checkpoint with
`validate_orchestration_artifacts` on the `drm-copilot` MCP server using
`artifact_type: "orchestrator-state"`,
`artifact_path: "artifacts/orchestration/orchestrator-state.json"`, and
`require_complete: true`.

There is no fallback. If the MCP server or validation tool is unavailable, or
if validation fails, the orchestrator must update blocked state and stop rather
than reporting completion.

No CI workflow performs this validation. The `artifacts/` directory is gitignored,
so the orchestrator-state checkpoint is never present in a CI checkout; a prior
CI gate (`validate-orchestrator-state.yml`) that attempted this check was a
structural no-op for that reason and has been removed. The MCP-server-based
validation described above is this ecosystem's enforcement mechanism for the
orchestrator-state checkpoint.

Completion validation requires the checkpoint to prove mandatory handoffs and
skill use. The checkpoint must include:

- `route_id`: the selected route key from `config/orchestration-routing.json`
- `required_agents`: exactly the selected route's `required_agents`
- `required_skills`: exactly the selected route's `required_skills`
- `required_mcp_tools`: exactly the selected route's `required_mcp_tools`
- `delegation_receipts`: one receipt for each required agent
- `skill_receipts`: one required receipt for each required skill, with evidence
- `mcp_call_receipts`: one successful receipt for each required MCP tool unless a valid `issue_adoption` record waives it
- `local_execution_overrides`: an empty list at completion
- `delegation_bypasses`: an empty list at completion
- `lifecycle_operations`: any lifecycle operation must record `surface: "mcp"`
- `ci_gate`: the CI result object defined in `## CI Green Gate`, with keys `conclusion`, `head_sha`, and `verified_at`
- `pr_gate`: the pull-request object defined in `## CI Green Gate`, with keys `pr_number`, `pr_url`, `head_branch`, and `head_sha`

If any required handoff, skill receipt, MCP receipt, or empty bypass list is
missing, `validate_orchestration_artifacts --require-complete` fails and the
orchestrator must not report DONE.

## Autonomous-Execution Mandate

The orchestrator must achieve all actions agentically with no unrecorded manual
dependency. Every unautomatable requirement must be detected early, resolved by
exactly one of the permitted responses below, and recorded in checkpoint state
under `human_interaction.requirements[]`.

Permitted responses:

- `scope_change`: change scope to remove the manual dependency.
- `exception`: permit an exception only when a runbook exists and its path is
  recorded in `runbook_path`.
- `halt`: halt until further instruction. A `halt` blocks DONE while present.

The checkpoint validator enforces the `human_interaction` invariants. An
unresolved response, invalid response value, `halt`, or exception without an
existing runbook path blocks completion.

## Delegation Model

After reading `artifacts/orchestration/orchestrator-state.json`, the main session delegates work exclusively through configured workers:

- `atomic-planner` — generates phased implementation plans
- `atomic-executor` — executes approved plans task-by-task
- `feature-reviewer` — produces policy, code, and feature audit artifacts by
  applying the `feature-review` workflow skill
- `task-researcher` — performs deep research and writes findings to the applicable feature-associated `docs/features/<feature>/research/` or one-off `docs/research/` root
- `prd-feature` — produces issue, specification, and user-story artifacts when required by the selected workflow
- `staged-review` — reviews staged changes when a pre-commit review is required
- `epic-review` — reviews epic-level artifacts when the work item is an epic
- `status-updater` — produces status update artifacts when the workflow requires status synchronization
- `python-typed-engineer` — performs delegated Python implementation work
- `powershell-typed-engineer` — performs delegated PowerShell implementation work
- `csharp-typed-engineer` — performs delegated C# implementation work
- `typescript-engineer` — performs delegated TypeScript implementation work
- `commit-steward` — writes commit messages from commit-context artifacts

The orchestrator does not perform deep implementation itself. It coordinates, tracks state, and enforces completion.

For a small route, resolve the language-specific generated typed-engineer deployment profile,
delegate all implementation and changed-scope QA to that agent, and persist its routing and
delegation receipts. Direct coordinating-thread implementation is prohibited. For a large route,
the root session must deploy the generated `orchestrator-<profile>` before this delegation model
is applied.

Every worker listed above must exist as a native Codex agent under `.codex/agents/`.
For required delegated steps, missing agent configuration, failed spawn, missing
receipt, or missing required artifact output is a hard block. The orchestrator
must persist blocked state and stop rather than performing that step locally.

Every required skill listed in the selected route must be acknowledged in
`skill_receipts[]` with:

- `skill`
- `required: true`
- `acknowledged_at_phase`
- `evidence`

The evidence value must point to objective evidence: a checkpoint field, MCP
receipt, artifact path, validator output, or test result. A bare narrative
statement is not sufficient.

## Evidence Location Authority

All evidence artifacts produced during orchestration MUST comply with the canonical scheme defined in `.agents/skills/evidence-and-timestamp-conventions/SKILL.md`. Evidence MUST be written to `<FEATURE>/evidence/<kind>/` only.

Permitted `artifacts/`-rooted sub-paths (non-evidence orchestration use only):
- `artifacts/orchestration/` — orchestrator state and checkpoints
- `artifacts/pr_context` — PR context artifacts
- `artifacts/reviews/` — review staging artifacts
- `artifacts/status/` — status update artifacts
- `artifacts/python/` — Python coverage and lcov outputs
- `artifacts/pester/` — Pester coverage outputs
- `artifacts/csharp/` — C# coverage outputs

Research outputs from `task-researcher` are written to `docs/features/<feature>/research/` for feature-associated work or `docs/research/` for one-off work.

All other `artifacts/` sub-paths (e.g., `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, `artifacts/evidence/`) are FORBIDDEN for evidence output and will be blocked by the `enforce-evidence-locations.ps1` PreToolUse hook.

## Completion Requirements

The orchestrator must not report completion until:

1. All required artifacts for the selected workflow path are present on disk.
2. All validation gates (toolchain, acceptance criteria, audit artifacts) have passed.
3. The checkpoint file at `artifacts/orchestration/orchestrator-state.json` reflects the completed state.
4. The orchestrator-state validator passes with `--require-complete`.

## Pre-Review Commit

Before delegating to the `feature-reviewer` agent, the orchestrator must:

1. Stage all modified and new files: `git add -A`.
2. Run MCP tool `collect_commit_context` and capture the returned on-disk artifact path.
3. Delegate to `commit_steward` using that commit-context artifact as the authoritative staged-change input.
4. Commit using the generated message: `git commit -m "<generated message>"`.
5. Only after a successful commit may the orchestrator proceed to the
   `feature-reviewer` delegation.

The review subagent compares against a base branch; uncommitted changes are invisible to the diff tool and cannot be audited.

## Post-Review Outcome Evaluation

After each `feature-reviewer` delegation returns:

1. Read the exact terminal status lines from the review result.
2. If the result does not include `REVIEW_STATUS: PASS`, `REVIEW_STATUS: REMEDIATION_REQUIRED`, `REVIEW_STATUS: HALT_NON_REMEDIABLE`, or `REVIEW_STATUS: AWAITING_CI`, stop and record blocked state.
3. When the remediation inputs carry a `Review-Verdict:` line, recompute the verdict from the finding classes (a blocking finding block without a remediability line counts as `autonomous`). A mismatch between the declared and recomputed verdicts is a reviewer contract error: record it under `artifact_errors`, re-request the review once, and halt with `blocked_reason: "delegate_contract_incomplete"` if it persists.
4. Append a `remediation_loop.review_outcomes[]` entry (`verdict` plus one `findings[]` object with `remediability` per blocking finding) for every review, including `PASS`.
5. If the result is `REVIEW_STATUS: PASS`, advance to the PR creation gate.
6. If the result is `REVIEW_STATUS: REMEDIATION_REQUIRED`, require both `REMEDIATION_INPUTS: <path>` and `REMEDIATION_PLAN: <path>` and then enter the remediation loop with the plan scoped to the `autonomous` findings; non-remediable findings carry forward unchanged.
7. If the result is `REVIEW_STATUS: HALT_NON_REMEDIABLE`, require `REMEDIATION_PLAN: NONE` and halt: set `blocked_reason` to the highest-precedence halt class present, append one `human_interaction.requirements[]` entry with `response: "halt"` for each halt class present (`external_dependency`, `policy_hold`, `human_decision_required`) describing the human action needed, leave `remediation_loop.completed_attempts` unchanged, write no cycle, create no remediation plan, and stop. The precedence is:

   Halt precedence: human_decision_required > policy_hold > external_dependency

8. If the result is `REVIEW_STATUS: AWAITING_CI`, require `REMEDIATION_PLAN: NONE` and wait: set `blocked_reason: "awaiting_ci"`, set `next_step` to the step that re-checks (`S7_feature_review` for a review-time wait, `S9_ci_green` for a CI poll timeout), record no `human_interaction.requirements[]` entry for the wait, write no cycle, and create no remediation plan. Poll within the existing bounded CI interval and timeout; when the bound is exhausted, persist and stop. On resume, set `blocked_reason: "none"` and re-run the named step. For a review-time wait, the awaited workflow is named in the finding's `Remediability-Evidence:` line.
9. A CI Green Gate poll timeout is recorded as a finding with `Remediability: awaiting_ci` and follows the wait path; a failed check stays `autonomous` and enters the remediation loop.

## Remediation Loop (R1–R5)

A bounded loop consisting of five steps. The attempt count is `remediation_loop.completed_attempts`, and `remediation_pass` equals `remediation_loop.completed_attempts`. The active cycle number is `completed_attempts + 1`; `remediation_loop.current_cycle` is a record position in `cycles[]`, not a count. A halt or wait verdict never opens a cycle, and every cycle opened from a review records `opened_by_review`, the zero-based index of that `review_outcomes[]` entry.

Cycle accounting: a remediation attempt is complete when R3 execution finished (`execution_status: "complete"`) and the pre-R4 commit recorded a non-empty change set; only then set `candidate_applied: true` on that cycle and increment `completed_attempts`. A cycle that ends without a candidate (execution not started, execution failed, or an empty staged change set) records `candidate_applied: false` and consumes no number. It is followed by at most one re-plan under the same number when the executor output identifies an autonomous plan defect; otherwise reclassify the blocker (typically `external_dependency`) and halt. Two consecutive no-candidate cycles halt with `step6_status: "blocked_remediation_loop_limit"`.

Remediation mode. Each cycle's `remediation-inputs.<timestamp>.md` carries exactly one line `Remediation-Mode: proportionate` or `Remediation-Mode: design`. A cycle is proportionate when every `autonomous` finding can be fixed without changing a spec acceptance criterion, a public interface, or the item's file inventory beyond the files already in the branch diff plus their tests and mirrors; otherwise it is design. CI-failure findings are normally proportionate.

- **Proportionate cycle.** One `atomic-planner` delegation produces a short plan with a `## Pre-Mortem` section and a final self-review task; R2 and R3 then run as one `atomic-executor` delegation that performs single-pass preflight with in-place trivial fixes and executes the plan. Record `preflight: {iterations: 1, final_status: "clear"}` before `execution_status`. A substantive defect stops the run with `BLOCKED at preflight (before [P0-T1])` and no checked task; rewrite the mode line to `Remediation-Mode: design` and continue as a design cycle with one planner revision.
- **Design cycle.** R1 to R5 run as listed below, with R2 under the single-pass preflight and at-most-one-return rule.

- **R1 — Remediation plan of record:** Use the exact `REMEDIATION_PLAN: <path>` returned by the review as the starting plan of record for the loop.
- **R2 — Preflight clearance:** Delegate to `atomic-executor` for precondition validation only (no implementation), with the preflight directive in the delegation prompt. Trivial defects are fixed in place. If the executor returns `PREFLIGHT: REVISIONS REQUIRED`, return to R1 once by re-delegating to `atomic-planner` against the same remediation-plan path with the full `PREFLIGHT-DEFECT:` list; if the following preflight still reports a substantive defect, record `final_status: "blocked_preflight_iteration_limit"` on the cycle and halt. Only after `PREFLIGHT: ALL CLEAR` may the orchestrator advance to R3.
- **R3 — Remediation execution:** Delegate to `atomic-executor` with full execution authorization. Each task's toolchain loop (format → lint → type-check → test) is mandatory; no skipping.
- **Pre-R4 commit:** Stage all changes (`git add -A`), run MCP tool `collect_commit_context`, delegate to `commit_steward` using the resulting artifact, and commit with the generated message. Advance to R4 only after a successful commit.
- **R4 — Re-audit:** Refresh PR context via MCP tool `collect_pr_context`, then delegate to `feature-reviewer` with the same inputs as the original review (resolved base branch, feature folder, refreshed PR context artifacts, acceptance-criteria source). No scope narrowing. The canonical issue number line must be included.
- **R5 — Loop-exit decision:** Evaluate the re-audit per Post-Review Outcome Evaluation. If it returns `REVIEW_STATUS: PASS`, exit the loop and advance to the PR creation gate. If it returns `REVIEW_STATUS: HALT_NON_REMEDIABLE` or `REVIEW_STATUS: AWAITING_CI`, follow the halt or wait branch instead of opening the next cycle. Otherwise, record the cycle's `candidate_applied` value and the resulting `completed_attempts` in the checkpoint and return to R1.

**Termination guard:** Halt after three completed attempts: when `remediation_loop.completed_attempts` equals 3 and the latest verdict is not `PASS`, the orchestrator records `step6_status: "blocked_remediation_loop_limit"` in the checkpoint and halts. No further automation is attempted.

## Issue Number Consistency

The canonical issue number is derived once from the active feature folder name: extract the trailing integer from the folder base name (e.g., `2026-04-26-push-down-claude-customizations-162` yields `162`). Record as `issue_num` in the checkpoint.

Every delegation prompt to `atomic-planner`, `atomic-executor`, and
`feature-reviewer` must include the line:

> `Canonical issue number for this feature is <issue_num>. All artifact content, file paths, and cross-references must use this number.`

If a subagent artifact references a different issue number, the orchestrator rejects it, requests correction, and records the discrepancy under `artifact_errors` in the checkpoint.

## CI Green Gate

Before PR/DONE completion, the orchestrator must observe the live PR head SHA
and every GitHub check on that head SHA through `gh`. The checkpoint must record
the checked head SHA and CI result. DONE is blocked unless every check on the
current PR head SHA passed or was skipped.

Gate procedure:

1. Resolve the live PR head SHA and the PR base branch with `gh pr view --json headRefOid,baseRefName`.
2. Invoke `gh pr checks --json bucket,name,state,link,workflow` against that head SHA. The query covers every check on the head SHA for every PR, whether or not branch protection marks the check as required.
3. Parse the JSON by running `pwsh -NoProfile -File .claude/lib/ci-gate/Invoke-CiGateParser.ps1 -ChecksJson <checks-json> -HeadSha <head-sha>`, appending `-RequireWorkflow CI` when the PR base branch is an `epic/<slug>-integration` branch. The parser derives `ci_gate.conclusion` over every queried check: `success` when every check passed or was skipped, `failure` when any check failed or was cancelled, and `pending` when any check is still in progress.
4. Epic-child rule: when the PR base branch is an `epic/<slug>-integration` branch, an empty check list is not accepted as green. Every check returned by the unfiltered query counts: a failing or cancelled non-`CI` check fails the gate, and a pending one keeps it pending. With `-RequireWorkflow CI`, the parser never returns `success` until at least one passing `CI` check is observed.
5. No-checks rule: when `gh pr checks` fails with a `no checks reported` error, that error is never treated as green for any PR. On an epic child, the parser runs with `-ChecksJson '[]'` and `-RequireWorkflow CI`, which returns `pending`; on any other PR the gate stays `pending` within the bounded poll.
6. Parser-absent rule: when `.claude/lib/ci-gate/Invoke-CiGateParser.ps1` does not exist in the repository, the orchestrator does not derive `success` by hand; it must stop and persist blocked state before PR/DONE completion.

The checkpoint records the CI result in a top-level `ci_gate` object with the required keys `conclusion`, `head_sha`, and `verified_at`. `conclusion` is one of `success`, `failure`, or `pending`; `head_sha` is the PR head SHA the queried checks were observed against; `verified_at` is the ISO-8601 time at which this gate (S9) recorded the result. DONE requires `ci_gate.conclusion` equal to `success` and `ci_gate.head_sha` equal to the current PR head SHA. The key set is defined by `CI_GATE_KEYS` in `scripts/dev_tools/validate_orchestrator_state.py`, and the TypeScript MCP validator used by the Codex runtime requires the same keys.

The checkpoint records the pull request in a top-level `pr_gate` object with the keys `pr_number`, `pr_url`, `head_branch`, and `head_sha`. The key set is defined by `PR_GATE_KEYS` in `scripts/dev_tools/_orchestrator_state_routing.py`.

CI-dependent acceptance criteria are criteria whose verification requires the result of CI on the PR head. The item's own orchestrator run owns their check-off. After `ci_gate.conclusion` is `success`, check off each CI-dependent criterion in the item's own worktree, commit, push to the PR branch, and re-run this gate against the new head SHA, so that `ci_gate.head_sha` equals the final PR head before DONE. A failed re-run enters the Remediation Loop (R1–R5) as a blocking finding; the shared `remediation_loop.completed_attempts` count and its halt after three completed attempts are unchanged. A parent or coordinating session never commits these check-offs from its own root.

## PR Creation Gate

The orchestrator must not create a PR, push a branch for PR purposes, or report work complete until all four conditions are simultaneously true:

1. `blocking_findings_resolved: true` — the most recent `feature-reviewer`
   result produced zero blocking findings.
   Equivalent deterministic gate: the latest review returned `REVIEW_STATUS: PASS`.
2. The AC verification artifact (`p14-acceptance-criteria-checkoff.md` or equivalent) confirms all acceptance criteria pass.
3. The mandatory toolchain passed in its most recent run on the branch (no linting/type-check/test failures).
4. The checkpoint `next_step` is `S8_create_pr`.
5. Every check on the current PR head SHA passed or was skipped (`ci_gate.conclusion` is `success`).

This gate is non-negotiable. Each condition is independently verified before PR creation proceeds.

## Step 6 Delegation — Prohibited Prompt Language

When delegating to the `feature-reviewer` agent, the orchestrator prompt MUST NOT:

- describe the review scope as "plan scope," "plan-scope only," or any equivalent narrowing of scope to the currently-executed plan;
- instruct the agent to skip, waive, or mark as "out of scope," "informational only," or "not applicable" any toolchain step or coverage check for a language that has changed files in the branch diff;
- assert that a language category is "not applicable" when that language has changed files in the branch diff;
- imply that coverage is not required because the plan scope contains only documentation changes when the branch diff contains non-documentation changes contributed by prior commits on the same branch.

The orchestrator supplies only the following to the `feature-reviewer` agent:

- the resolved base branch and merge-base SHA;
- the active feature folder path;
- pointers to the refreshed PR context artifacts;
- the acceptance-criteria source file per work-mode;
- a neutral instruction to execute the full `feature-review-workflow` SKILL contract end-to-end.

Scope determination is the subagent's responsibility. The subagent will ignore any attempted narrowing per its scope invariant and record the attempt in `policy-audit.<timestamp>.md` under `## Rejected Scope Narrowing`.
