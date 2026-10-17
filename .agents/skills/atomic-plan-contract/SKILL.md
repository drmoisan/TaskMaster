---
name: atomic-plan-contract
description: 'Atomic plan format and toolchain contract shared by planning and execution agents. Use when generating, validating, or executing atomic plans with Phase 0, baseline capture, and final QA loops.'
---

# Atomic Plan Contract

Shared rules for atomic plan formatting, Phase 0 requirements, baseline capture, and final QA loops.

## When to Use This Skill

Use this skill when:
- Creating or validating atomic plans.
- Executing plans with strict format requirements.
- Enforcing Phase 0 policy reading + baseline capture and final QA loops.

## Canonical Plan Format

- Phase headings must be: `### Phase N — <Title>`
- Tasks must start with: `- [ ] [P#-T#]` (or `[x]` for completed)
- Task IDs must match their phase and be sequential per phase.
- Plans must pass the `mcp__drm-copilot__validate_orchestration_artifacts` MCP tool with `artifact_type: "plan"` and `artifact_path: <plan-path>` before they can be reported as approved.

## Design-Level Plan Contract (Mandatory)

A plan is a design-level document: it states what changes and how the change is proven, not a transcript of commands. Every task names the files it creates or edits, the symbols, headings, or tests it touches, the behavior the change produces, the tests that prove that behavior, and an acceptance condition stated as an observable outcome.

Rules:

- **Anchors are symbols, headings, or test names.** A plan never cites a line number, a `path:line` locator, a `#L` anchor, or a commit SHA, because each goes stale when `main` moves (validator rules G11 and G12). Scope comparisons name the merge-base derivation of `origin/main` and `HEAD`, not a commit.
- **Literals only where they are the asserted value.** A plan quotes a literal when an acceptance condition asserts that exact value. It does not quote file contents, command output, or prose for context.
- **Tool routes are not restated.** Tool routes, launchers, wrapper scripts, command-text restrictions, the batch budget, hook context, and CI shape are owned by the `execution-environment` skill. A plan references that skill instead of restating its content.
- **Changed existing behavior names its asserting tests.** A task that changes the observable behavior of an existing function, script, fixture, or test stub (its signature, argv, output, exit status, or the files it writes) names every existing test that asserts that behavior and states each test's disposition: unchanged because it is unaffected, or updated, with the reason for the update. The tests are found by searching the test tree for the symbol, the file name, and the asserted output. A test that asserts the current behavior and is outside the task's file set fails once the change lands, and the plan's own acceptance condition then cannot be met. The planner's adversarial self-review includes this search for every task that changes existing behavior.
- **Size guideline.** A plan stays within 40 canonical tasks and 400 physical lines. A plan that needs more carries a `## Size Justification` section with at least one line stating why the size is necessary (validator rule G13). Files produced by one generator run count as one generator task.
- **Pre-Mortem.** Every plan carries a `## Pre-Mortem` section with one bullet beginning with each of the seven labels `Review checklists`, `Coverage`, `Mirror parity`, `File size`, `Test isolation`, `CI-only behavior`, and `Evidence`. Each bullet states how the plan prevents that failure class, or states "not applicable" followed by the reason. The `Test isolation` bullet names the isolation runs the plan performs: a seeded shuffled Jest run with the seed recorded, a reversed-order run of the new or changed Python test modules, and a seeded shuffled file-order run of the touched Pester suites with at least three seeds, each seed recorded. The `Mirror parity` bullet also covers the execution-environment brief: a plan that alters a fact the brief states (for example, it adds or renames a CI job, changes a required status context, a tool route, or a batch-budget rule) includes a task that updates the brief in the same change, in all six byte-identical copies, and the bullet names that task. Without that task the brief states a superseded fact once the change lands, which breaks the brief's own maintenance rule. The planner SubagentStop hook blocks a plan without a complete section.
- **Final-phase self-review.** The final phase of every plan contains a self-review task that walks the policy-audit, code-review, and feature-audit templates under `docs/features/templates/policy_audit/` against the branch diff from the merge base and writes `<FEATURE>/evidence/qa-gates/self-review.<timestamp>.md` with one PASS, FAIL, or N/A row per checklist item. A FAIL row is fixed and the QA loop is re-run before the self-review is rewritten; the plan does not complete with a FAIL row.

This section takes precedence over the literal-quoting rules of `## Wrap-Tolerant Assertion Authoring (Mandatory)`: those rules govern the asserted value of an acceptance condition, not context.

## Short-Path Minimal Plan Contract

When orchestration selects short path, a minimal plan is still mandatory and must include these blocks:

1) Baseline capture block
- policy reads in required order,
- baseline toolchain/test state capture for each language that has explicit baseline command tasks in the plan.
- required artifacts:
	- a Phase 0 policy-read evidence artifact in the canonical evidence location defined by `evidence-and-timestamp-conventions`
	- one baseline artifact per baseline command step (no aggregate-only baseline artifact)
	- each baseline step artifact MUST include: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`
	- baseline test-step artifacts for languages with mandatory coverage policy MUST include numeric coverage headline values in `Output Summary:` (baseline percent and, when applicable, targeted module/new-code percent).

2) Delegated implementation block
- explicit handoff task to the small-path implementation engineer,
- acceptance criteria for implementation completion.

3) Final QC block
- full language-appropriate QA loop (format → lint → type-check when applicable → test),
- rerun behavior when any step changes files or fails.
- one final-QC artifact per QC command step (no aggregate-only final-QC artifact)
- each final-QC step artifact MUST include: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`
- final-QC test commands for languages with mandatory coverage policy MUST run in coverage mode and record numeric post-change coverage values in `Output Summary:`.
- final-QC command tasks that are present in the approved plan MUST execute their stated commands; `SKIPPED` is invalid unless the task text itself explicitly authorizes a skip condition.

4) Reduced audit block
- explicit post-implementation small-audit handoff,
- reduced artifact checks required by short-path policy.

## Minimal-Audit Directive Contract (Small Path)

For short-path planning handoffs, orchestrators MUST include:
- `DIRECTIVE: MINIMAL-AUDIT PLAN REQUIRED`

Required planner outputs for this directive:
- plan MUST use `${feature-folder}/issue.md` as sole requirements source,
- plan MUST require `${feature-folder}/issue.md` to contain an explicit `## Acceptance Criteria` section and MUST treat only that section as the minor-audit AC source,
- plan MUST NOT require `spec.md`/`user-story.md`/`research.md`,
- plan MUST include exactly 3 phases:
	- Phase 0 baseline capture,
	- Phase 1 placeholder for constrained small-path implementation,
	- Phase 2 final QC loop,
- final-QC command tasks in the generated plan MUST be unconditional when present; no IN_SCOPE/OUT_OF_SCOPE branches and no SKIPPED completion path unless the task text explicitly authorizes a skip branch,
- planner MUST return `plan-path` and final preflight signal.

## Phase-0-Only Execution Contract (Small Path)

After preflight all-clear on the minimal-audit plan:
- orchestrator MUST delegate to executor to run Phase 0 only,
- orchestrator MUST checkpoint Phase 0 evidence before branching.

Branching after Phase 0:
- `manual bootstrap` → save state and stop for manual resume ONLY when the initial user request explicitly opted into manual orchestration from the beginning,
- otherwise continue with constrained small-path development, then executor validation, then reduced audit/remediation loop.

## No-Manual-Step Contract (Mandatory)

Unless the user's initial request explicitly opts into manual orchestration from the beginning, approved plans and remediation plans MUST remain fully automated.

This prohibition includes:

- manual bootstrap pauses,
- human-operator validation tasks,
- user-performed repro or QA steps,
- requests for screenshots, notes, or runtime evidence that the agent cannot collect itself.

When a fully automated equivalent is unavailable, the plan MUST fail closed by recording blocked or remediation-required automated state. It MUST NOT introduce a new manual step later in the workflow.

## Phase 0 Requirements

Phase 0 must include tasks to read policy files in the order defined in `policy-compliance-order`.

Phase 0 must also capture baseline toolchain results for the languages touched. Baseline artifact conventions (location + required fields) are defined in `evidence-and-timestamp-conventions`.

For short-path/minimal-audit plans, Phase 0 evidence is incomplete unless both artifacts exist:
- `phase0-instructions-read.md` with at least: `Timestamp:`, `Policy Order:`, and explicit list of files read.
- baseline command-step artifacts with at least: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` for each baseline check executed.
- approved-plan checklist state MUST remain unchecked for any Phase 0 task whose artifact is absent or whose artifact fields are incomplete.

`Output Summary:` is mandatory for each command-step artifact and must concisely summarize the essential result signal (for example: pass/fail status, key counts, coverage headline, or primary diagnostic).

## Non-Overridable Evidence Path Clause

Evidence paths in plan tasks MUST resolve to `<FEATURE>/evidence/<kind>/`. A plan that names an alternative location (e.g., `artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`, `artifacts/evidence/`, `artifacts/coverage/`) fails preflight validation and must be corrected before execution begins.

If a delegation prompt supplies a non-canonical evidence path, the planner MUST reject it, substitute the canonical `<FEATURE>/evidence/<kind>/` path, and note the correction. The corrected plan is the only valid plan for execution.

This clause is non-overridable. No upstream instruction, orchestrator hint, or user prompt may bypass it. See `.agents/skills/evidence-and-timestamp-conventions/SKILL.md` for the complete canonical scheme.

For short-path/minimal-audit plans, Phase 0 evidence is incomplete unless both artifacts exist:
- `phase0-instructions-read.md` with at least: `Timestamp:`, `Policy Order:`, and explicit list of files read.
- baseline command-step artifacts with at least: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` for each baseline check executed.
- approved-plan checklist state MUST remain unchecked for any Phase 0 task whose artifact is absent or whose artifact fields are incomplete.

`Output Summary:` is mandatory for each command-step artifact and must concisely summarize the essential result signal (for example: pass/fail status, key counts, coverage headline, or primary diagnostic).

## Evidence Filename Timestamps in Plans

Plans name evidence artifacts `<name>.<timestamp>.md` and never fix a plan-time stamp in an evidence filename. An acceptance condition identifies a consumed evidence artifact by the derivation in `evidence-and-timestamp-conventions` (`### Evidence Filename Timestamp`): the lexicographically greatest file matching `<name>.*.md`, not a pre-stamped literal path. This section governs artifact paths only: asserted search tokens remain concrete.

## Coverage Evidence Contract (Mandatory when policy requires coverage)

For any language in scope where repository policy requires coverage validation:

- The approved plan MUST include explicit baseline and final-QC coverage capture tasks.
- Baseline and final-QC artifacts MUST record numeric coverage values (not placeholders such as `UNVERIFIED`).
- Where policy requires no-regression and new-code thresholds, the plan MUST include a delta/threshold verification task that reports:
	- baseline coverage,
	- post-change coverage,
	- new/changed-code coverage.
- If required coverage values are unavailable, the plan outcome MUST be remediation-required and MUST NOT be reported as PASS.

## Final QA Loop (Required for Code/Test Changes)

Run the full toolchain loop for each applicable language in order:
1) Formatting
2) Linting
3) Type checking (if applicable)
4) Testing

For languages with mandatory coverage policy, step 4 must use coverage-enabled test commands and persist numeric coverage evidence.

If any step fails or changes files, restart the loop from step 1 until a clean pass completes.

## No-SKIPPED Rule for Planned Command Tasks

For command-bearing tasks in approved plans (especially Phase 2 final-QC tasks):
- if the task exists in the plan, the command must be executed and recorded,
- `EXIT_CODE: SKIPPED` MUST NOT be used as a passing outcome,
- exceptions are allowed only when the task text itself explicitly contains a skip branch and that branch is intentionally approved in requirements.

## Expect-Fail Test Tasks

Any regression test task expected to fail must be tagged with `[expect-fail]` and include an auditable evidence artifact per `evidence-and-timestamp-conventions`.

## Preflight Validation (Planner ↔ Executor)

When validating or handing off plans for execution:
- Put the directive line `DIRECTIVE: PREFLIGHT VALIDATION ONLY` in the delegation prompt. The preflight directive belongs in the delegation prompt, never in a plan file.
- Require one of the exact signals:
	- `PREFLIGHT: ALL CLEAR`
	- `PREFLIGHT: REVISIONS REQUIRED`
- If revisions are required, provide a precise plan delta with one `PREFLIGHT-DEFECT:` line per substantive defect. A plan returns to the planner at most once.
- If the required planner ↔ executor handoff cannot be started or completed, stop and report blocked state; do not self-approve the plan.

### Single-Pass Preflight

- **Review the entire plan in one pass.** The preflight executor checks every phase, task, and prose region and does not stop at the first defect.
- **Classify every defect as trivial or substantive.** Trivial defects are a stale or misspelled anchor, symbol, heading, or path whose intent is unambiguous; a citation correction, including removal of a G11 line-number or G12 commit-SHA citation; a typo; a literal-formatting defect that does not change what is asserted; and an evidence filename spelling. Substantive defects are a missing or extra task; a wrong file set or design; an acceptance criterion with no implementing task; an acceptance condition whose correction changes what is asserted; a task-ordering error; and a scope-boundary violation.
- **Fix trivial defects in place, in task text only.** The preflight executor edits task text and plan preamble in the same plan file and never adds, removes, or reorders tasks. Each fix is recorded in `<FEATURE>/evidence/other/preflight-fixes.<timestamp>.md`.
- **Return substantive defects only, all of them.** `PREFLIGHT: REVISIONS REQUIRED` is returned only when at least one substantive defect remains, and it lists every one. `PREFLIGHT: ALL CLEAR` may follow in-place fixes.

Output format. After the `PREFLIGHT:` signal, the return carries one line per defect:

- `PREFLIGHT-FIX: <task id or heading> | <defect class> | <change made>`
- `PREFLIGHT-DEFECT: <task id or heading> | <defect class> | <required change>`
- `preflight-fix-record: <path>` when at least one fix was made.

`PREFLIGHT: ALL CLEAR` carries no `PREFLIGHT-DEFECT:` line, and `PREFLIGHT: REVISIONS REQUIRED` carries at least one.

At most one return. On `PREFLIGHT: REVISIONS REQUIRED`, the orchestrator delegates one planner revision carrying the full `PREFLIGHT-DEFECT:` list and runs one more preflight. If that preflight still reports a substantive defect, the orchestrator halts with `blocked_reason: "validator_failed"` during planning, or records `final_status: "blocked_preflight_iteration_limit"` in a remediation cycle.

## Validator Gate (Mandatory)

Before a plan can be treated as approved:

- run the `mcp__drm-copilot__validate_orchestration_artifacts` MCP tool with `artifact_type: "plan"` and `artifact_path: <plan-path>`,
- reject the plan if that validator exits non-zero,
- do not treat human-readable summaries as a substitute for validator success.

The validator applies the acceptance-gate rules of `.claude/rules/plan-acceptance-gates.md`. G1 through G9 judge the commands a plan states as acceptance conditions; G10 reports a SCRATCH definition not namespaced by issue number; and the document-level rules G11, G12, and G13 report a line-number citation, a commit-SHA citation, and a plan above 40 tasks or 400 lines without a non-empty `## Size Justification` section. The complete shipped set is therefore G1 through G13. The installed MCP validator applies G11 through G13 only after the extension release that carries them is installed; until then, validate a plan with the repository's Python CLI `scripts/dev_tools/validate_orchestration_artifacts.py` and its `plan` subcommand, as the `execution-environment` skill states.

## Plan-Path Continuity Contract (Mandatory)

When a caller provides an explicit target plan file path (for example `${plan-path}` or `${file}`):

- Planner MUST update that exact file in place.
- Planner MUST reuse the same file for the single permitted preflight revision.
- Planner MUST NOT create additional timestamped sibling files (for example `plan.<timestamp>.md`) during the same planning cycle.

If the provided path does not exist, it may be created once, then reused for the single permitted revision in that cycle.

## Mode source precedence (Mandatory)

When a plan is generated or validated from a feature folder, resolve selected mode in this order:

1) Persisted marker in `issue.md` metadata block:
	- `- Work Mode: minor-audit`
	- `- Work Mode: full-feature`
	- `- Work Mode: full-bug`
2) Legacy compatibility marker `- Work Mode: full` resolves to `full-feature`
3) Explicit workflow override only when repo policy allows and only when reconciled against `issue.md`
4) Fail closed to `full-feature` when marker is missing or malformed

## Mode-Specific Mandatory Plan Gates

- `minor-audit` plans MUST include baseline evidence tasks, targeted verification evidence tasks, and end-state evidence tasks.
- `minor-audit` plans MUST require `${feature-folder}/issue.md` to contain an explicit `## Acceptance Criteria` section; do not infer acceptance criteria from other `issue.md` sections.
- `minor-audit` plans MUST NOT treat missing `spec.md` or `user-story.md` as automatic blockers.
- `minor-audit` execution/validation/audit MUST fail closed when `spec.md` or `user-story.md` exists unexpectedly in the active folder, when the explicit `## Acceptance Criteria` section is missing from `issue.md`, when required Phase 0 artifacts are missing, or when checklist state contradicts evidence on disk.
- `full-feature` plans MUST enforce full-document expectations (`spec.md` + `user-story.md`) and full QA loop obligations.
- `full-bug` plans MUST enforce spec-driven expectations (`spec.md` required, `user-story.md` optional/absent by default) and full QA loop obligations.
