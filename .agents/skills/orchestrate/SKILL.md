---
name: orchestrate
description: Route a repository request through the deterministic orchestration workflow for feature, bug, research, planning, execution, and review handoffs.
argument-hint: "[objective]"
---

# Orchestrate Skill

This skill frames work for the already-active main session, which serves as the orchestrator runtime for end-to-end feature or bug delivery.

## Entry-Point Contract

The already-active main session is the canonical orchestrator runtime for this
skill. Optional orchestrator profiles, agent configuration files, or named
profiles are configuration aids only; they do not replace the active
main-session orchestration contract.

The main session owns route selection, checkpoint updates, lifecycle sequencing,
delegation decisions, and completion gating unless a required workflow step is
explicitly delegated by this skill or by `orchestrator-workflow`.

## Prerequisites

Before proceeding, the orchestrator must:

1. Read `AGENTS.md` for repository tone policy and architectural context.
2. Read applicable `.agents/skills/` files for the languages in scope.
3. Read the policy files listed in the compliance reading order section of `AGENTS.md`.

## Checkpoint Handling

On every invocation, the main session must:

1. Read `artifacts/orchestration/orchestrator-state.json` to check for existing state.
2. If a valid checkpoint exists with a matching objective, resume from the recorded `next_step`.
3. If no checkpoint exists or the objective is new, begin the orchestration lifecycle from the start.
4. Read `config/orchestration-routing.json` before route selection and copy the
   selected route's required agents, skills, and MCP tools into checkpoint
   state.

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
- `mcp_call_receipts`: one successful receipt for each required MCP tool
- `local_execution_overrides`: an empty list at completion
- `delegation_bypasses`: an empty list at completion
- `lifecycle_operations`: any lifecycle operation must record `surface: "mcp"`

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
- `task-researcher` — performs deep research and writes findings to `artifacts/research/`
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
- `artifacts/research/` — research outputs from task-researcher
- `artifacts/pr_context` — PR context artifacts
- `artifacts/reviews/` — review staging artifacts
- `artifacts/status/` — status update artifacts
- `artifacts/python/` — Python coverage and lcov outputs
- `artifacts/pester/` — Pester coverage outputs
- `artifacts/csharp/` — C# coverage outputs

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
2. If the result does not include `REVIEW_STATUS: PASS` or `REVIEW_STATUS: REMEDIATION_REQUIRED`, stop and record blocked state.
3. If the result is `REVIEW_STATUS: PASS`, advance to the PR creation gate.
4. If the result is `REVIEW_STATUS: REMEDIATION_REQUIRED`, require both `REMEDIATION_INPUTS: <path>` and `REMEDIATION_PLAN: <path>` and then enter the remediation loop.

## Remediation Loop (R1–R5)

A bounded loop consisting of five steps. The loop variable `remediation_pass` starts at 1 and increments at R5 before returning to R1.

- **R1 — Remediation plan of record:** Use the exact `REMEDIATION_PLAN: <path>` returned by the review as the starting plan of record for the loop.
- **R2 — Preflight clearance:** Delegate to `atomic-executor` for precondition validation only (no implementation). If the executor does not return `PREFLIGHT: ALL CLEAR`, return to R1 by re-delegating to `atomic-planner` against the same remediation-plan path with the required-changes output from the executor. Only after `PREFLIGHT: ALL CLEAR` may the orchestrator advance to R3.
- **R3 — Remediation execution:** Delegate to `atomic-executor` with full execution authorization. Each task's toolchain loop (format → lint → type-check → test) is mandatory; no skipping.
- **Pre-R4 commit:** Stage all changes (`git add -A`), run MCP tool `collect_commit_context`, delegate to `commit_steward` using the resulting artifact, and commit with the generated message. Advance to R4 only after a successful commit.
- **R4 — Re-audit:** Refresh PR context via MCP tool `collect_pr_context`, then delegate to `feature-reviewer` with the same inputs as the original review (resolved base branch, feature folder, refreshed PR context artifacts, acceptance-criteria source). No scope narrowing. The canonical issue number line must be included.
- **R5 — Loop-exit decision:** If the re-audit returns `REVIEW_STATUS: PASS`, exit the loop and advance to the PR creation gate. Otherwise, record `remediation_pass` increment in the checkpoint and return to R1.

**Termination guard:** If `remediation_pass` reaches 3 without resolution, the orchestrator records `step6_status: "blocked_remediation_loop_limit"` in the checkpoint and halts. No further automation is attempted.

## Issue Number Consistency

The canonical issue number is derived once from the active feature folder name: extract the trailing integer from the folder base name (e.g., `2026-04-26-push-down-claude-customizations-162` yields `162`). Record as `issue_num` in the checkpoint.

Every delegation prompt to `atomic-planner`, `atomic-executor`, and
`feature-reviewer` must include the line:

> `Canonical issue number for this feature is <issue_num>. All artifact content, file paths, and cross-references must use this number.`

If a subagent artifact references a different issue number, the orchestrator rejects it, requests correction, and records the discrepancy under `artifact_errors` in the checkpoint.

## CI Green Gate

Before PR/DONE completion, the orchestrator must observe the live PR head SHA
and required GitHub checks through `gh`. The checkpoint must record the checked
head SHA and CI result. DONE is blocked unless the required checks pass for the
current PR head SHA.

## PR Creation Gate

The orchestrator must not create a PR, push a branch for PR purposes, or report work complete until all four conditions are simultaneously true:

1. `blocking_findings_resolved: true` — the most recent `feature-reviewer`
   result produced zero blocking findings.
   Equivalent deterministic gate: the latest review returned `REVIEW_STATUS: PASS`.
2. The AC verification artifact (`p14-acceptance-criteria-checkoff.md` or equivalent) confirms all acceptance criteria pass.
3. The mandatory toolchain passed in its most recent run on the branch (no linting/type-check/test failures).
4. The checkpoint `next_step` is `S8_create_pr`.
5. The required CI checks pass for the current PR head SHA.

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
