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
- `manual bootstrap` → save state and stop for manual resume,
- otherwise continue with constrained small-path development, then executor validation, then reduced audit/remediation loop.

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

This clause is non-overridable. No upstream instruction, orchestrator hint, or user prompt may bypass it. See `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` for the complete canonical scheme.

For short-path/minimal-audit plans, Phase 0 evidence is incomplete unless both artifacts exist:
- `phase0-instructions-read.md` with at least: `Timestamp:`, `Policy Order:`, and explicit list of files read.
- baseline command-step artifacts with at least: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` for each baseline check executed.
- approved-plan checklist state MUST remain unchecked for any Phase 0 task whose artifact is absent or whose artifact fields are incomplete.

`Output Summary:` is mandatory for each command-step artifact and must concisely summarize the essential result signal (for example: pass/fail status, key counts, coverage headline, or primary diagnostic).

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

## Planner Adversarial Self-Review (Mandatory)

Before any plan handoff, `atomic-planner` MUST complete one explicit adversarial self-review pass over every fact, assumption, and line or file citation the plan relies on. The pass is required on initial authoring and on every revision-delta round. A revision round is not exempt because it changed only part of the plan: the citations the revision touched describe the tree as it stands after the revision, and no earlier pass observed that state.

Rules:

- **Re-derive every citation in this pass.** Any line, file, test, or assertion that the planner's own edit touched, added, or removed in the current authoring or revision pass MUST be re-derived directly against current repository state in that same pass. The prohibited source is a citation carried forward from an earlier round, including one the planner itself verified in a prior round: that earlier verification observed the tree before the intervening edits, so it is evidence about a superseded state rather than about the state the plan now asserts.
- **Re-check the sibling region.** The self-review MUST re-check the sibling lines, tests, and assertions that sit in the same file or region as any edited citation. The failure mechanism is sibling invalidation: a fix to one line can invalidate an assumption baked into a sibling line or test that a prior round's citation did not cover, so a pass that verifies only the edited line leaves the invalidated sibling unreported and it surfaces as a defect on a later round.

Declaration requirement. Every plan handoff MUST carry exactly one of these two signal lines, written in the directive-line form already used elsewhere in this contract:

- `SELF-REVIEW: RE-DERIVED THIS PASS` — the adversarial self-review pass completed in this pass. This signal MUST be followed by an enumeration of the citations re-derived in that pass, one entry per citation, each naming the file and the line, test, or identifier that was re-derived. A signal carrying no enumeration is not a completed declaration.
- `SELF-REVIEW: BLOCKED` — the pass could not be completed. This signal halts the handoff. It does not permit a self-approved plan: the planner reports the blocking reason and waits for the caller rather than proceeding to hand off an unverified plan.

## Preflight Validation (Planner ↔ Executor)

### Planner Internal Review Record

Before executor preflight, the planner must emit exactly one bounded record between `PLANNER-INTERNAL-REVIEW: PASS` and the existing `PREFLIGHT:` signal. The record requires exactly one passing declaration each for `CITATION-TO-TREE`, `AC-TRACEABILITY`, and `SCOPE-BOUNDARY`; one or more `CITATION: <repository-relative path> | <nonblank locator>` entries; exactly one `AC-INVENTORY:` declaration containing unique nonblank IDs; one `AC-MAPPING: <ID> | IMPLEMENTATION: <nonblank> | TESTS: <nonblank> | EVIDENCE: <nonblank>` for every and only inventory ID; and exactly one `UNRESOLVED-GAPS: NONE`. Missing, blank, duplicate, non-passing, out-of-bounds, or inventory/mapping-disagreeing declarations block handoff. If review cannot pass, emit `SELF-REVIEW: BLOCKED` and do not hand the plan to preflight. `SELF-REVIEW: RE-DERIVED THIS PASS` remains distinct and does not replace executor clearance.

When validating or handing off plans for execution:
- Use the directive line: `DIRECTIVE: PREFLIGHT VALIDATION ONLY`.
- Require one of the exact signals:
	- `PREFLIGHT: ALL CLEAR`
	- `PREFLIGHT: REVISIONS REQUIRED`
- If revisions are required, provide a precise plan delta and repeat validation until all clear.
- If the required planner ↔ executor handoff cannot be started or completed, stop and report blocked state; do not self-approve the plan.

Review depth and reporting rules:

- **Review the entire plan in one pass.** Under `DIRECTIVE: PREFLIGHT VALIDATION ONLY`, `atomic-executor` MUST continue checking every remaining phase, task, and prose region after finding an initial defect. Stopping at the first defect is prohibited: the unchecked remainder holds defects that the same pass could have reported, and each one that is left unreported becomes an additional round.
- **Enumerate every defect found.** `PREFLIGHT: REVISIONS REQUIRED` output MUST list every defect found in that pass, not only the first. The failure mechanism is round inflation: a single-defect report causes the next round to rediscover a defect the same pass could have reported, so the round count rises without the review having covered more of the plan.
- **Check the delta against its own rule.** Before returning either signal, `atomic-executor` MUST check its proposed fix or delta text against every rule the plan enforces, including that delta's own prose against the same violation class it is remediating. Worked example: the delta prose of a tonality-compliance fix must not itself contain the hyperbole or humor that `.claude/rules/tonality.md` prohibits, because a delta that violates the rule it is written to enforce reintroduces the finding it closes.
- **Two-round target.** The quality bar is a target of at most two preflight rounds per plan. Exhaustive first-pass review is the mechanism that holds the round count to that target: a pass that reports every defect it can find leaves at most a revision round and a confirming round, whereas a pass that reports one defect at a time cannot reach the target however correct each individual report is.

Convergence signal. Every preflight return, whether it carries `PREFLIGHT: ALL CLEAR` or `PREFLIGHT: REVISIONS REQUIRED`, MUST additionally carry exactly one of these two forward-looking lines:

- `CONVERGENCE: NO FURTHER ROUNDS EXPECTED` — the reviewer expects the plan to clear without a further round.
- `CONVERGENCE: FURTHER ROUNDS LIKELY` — the reviewer expects at least one further round, and states why.

The convergence line is a required signal rather than free prose. It is a second required line accompanying the preflight signal, not a third value of the signal set that the `Require one of the exact signals:` bullet above enumerates: that bullet's two-value set is unchanged, and every return carries one value from it together with one convergence line.

## Validator Gate (Mandatory)

Before a plan can be treated as approved:

- run the `mcp__drm-copilot__validate_orchestration_artifacts` MCP tool with `artifact_type: "plan"` and `artifact_path: <plan-path>`,
- reject the plan if that validator exits non-zero,
- do not treat human-readable summaries as a substitute for validator success.

The same validator call also applies the acceptance-gate rules G1 through G6 defined in `.claude/rules/plan-acceptance-gates.md`. Those rules report acceptance conditions that cannot fail — a coverage argument that collects no data, or a search for a literal that returns zero matches whatever the executor does. They run automatically on the existing `plan` route with no additional flag. Blocking findings appear in the validator's error output and fail the gate; Warnings are surfaced without failing it, prefixed with `PLAN GATE WARNING: ` on the CLI and carried on the optional `warnings` field of the MCP result. Read that rule file before authoring acceptance conditions.

The same call additionally applies the rules G7, G8, G8b, and G9, which report a write-mode command observed only by its exit code, an unanchored `git diff`, a name-listing diff with no companion span, and a coverage command that prints no table. All four ship in the Warning channel, so they surface without failing the gate. The complete shipped set is therefore G1 through G9.

## Wrap-Tolerant Assertion Authoring (Mandatory)

An acceptance condition must be able to fail. A condition whose command returns the same result whatever the executor does verifies nothing, however precise it reads. Author every acceptance condition in a wrap-tolerant form: one that survives line wrapping and shell quoting in the file it asserts against.

Rules:

- **Prefer a named test over a phrase search.** When a test can carry the assertion, name the test and its node ID and assert its pass count. A test node ID is stable under reformatting; a prose phrase is not. Reserve searches for cases where no test can express the condition.
- **Single-line token rule.** Where a search is unavoidable, assert a short, single-line, non-interpolated token that the plan quotes verbatim. A multi-word phrase drawn from prose is wrap-fragile: once the target file reflows, the phrase spans two lines and a line-oriented search returns zero matches even though the text is present. Rule G6 in `.claude/rules/plan-acceptance-gates.md` reports this case.
- **No placeholders in an asserted token.** A token containing `<`, `>`, `${`, `$(`, or `%` is treated as a documented command shape rather than a real assertion and is skipped by the gate, so it gates nothing. Substitute the concrete value.
- **Dotted coverage-argument form.** Coverage assertions must name an importable dotted module, for example `--cov=scripts.dev_tools.plan_gate_discrimination`. The filesystem-path spellings `--cov=scripts/dev_tools/module.py` and `--cov=scripts/dev_tools/module` collect no data, so a coverage threshold asserted against them cannot fail. Rules G1 through G3 report those spellings.
- **Use the `=` form, not the space-separated form.** `--cov <value>` can bind the following positional argument. Rule G4 reports it.
- **Quote what the task will create.** When an asserted literal does not yet exist in the tree, quote the exact literal in the plan prose outside the command span. The gate reads that quotation as the executor's instruction and exonerates the assertion; a paraphrase does not.
- **Record an observation beyond the exit code for a write-mode command.** A formatter or a fixing linter rewrites tracked source and still exits 0 after rewriting, so its exit code is identical on a clean run and on a repairing one. State, in the task text, the literal its success-case output prints — for example the summary line a formatter prints when it changed nothing — or state a before-and-after tree observation. Rule G7 in `.claude/rules/plan-acceptance-gates.md` reports a write-mode command whose task text carries neither.
- **Anchor every `git diff` to a ref.** A `git diff` with no ref operand and no `--cached` compares the worktree against the index, which is ambient state: it passes vacuously once the change is committed, so the assertion cannot fail for the executor who commits before running it. Supply an explicit ref operand, usually the base branch. Rule G8 reports the unanchored form.
- **Pair a name-listing diff with a staging or porcelain-status companion.** An anchored `git diff --name-only` or `--name-status` enumerates tracked changes only, so it can never report a file the task creates, and an assertion that it lists newly created files always sees an empty list. Add a `git add` span or a `git status --porcelain` span in the same task. Rule G8b reports a name-listing diff with neither companion. The two mechanisms are complementary and each alone is wrong in one state: the anchored diff is blind to untracked files, and porcelain status goes empty once the change is committed.
- **Pass a terminal reporter to every coverage command.** The project `addopts` value supplies an LCOV reporter only, so a coverage command that does not pass `--cov-report=term-missing` prints no coverage table at all, and a numeric percentage the acceptance condition demands can never be read from it. Rule G9 reports a coverage command that supplies no terminal reporter and no `--cov-fail-under` threshold.
- **Observe a command's success-case output before asserting over that output (mandatory).** Run the command, or read a recorded run of it, and confirm the value you intend to assert is actually printed on a *successful* run. Do not infer it from the tool's documentation, from the plan, or from what the tool prints on failure. This requirement exists because the class of defect it prevents is not detectable by reading: an assertion on a line the tool prints only when it changed something is unsatisfiable on a clean run; a demand for a zero-diagnostic count from a tool that returns only an ok flag names a value with no source; a demand for separate line and branch percentages from a coverage run that prints one combined column reads two numbers that are never printed. G7 and G9 cover two decidable slices of this class. The remainder is covered by this requirement and by nothing else, so a reviewer cannot rely on the gate to catch it.
- **Fix the evidence in the plan; never leave the executor to select it.** An executor free to choose the evidence it is judged against cannot fail. Do not write an acceptance condition that asks the executor to identify "the known-genuine pair", to choose a survivor list, or to pick any suitable instance. Name the instances, or state the mechanical derivation that produces them, so a third party re-running it obtains the same set. This is authoring guidance and not a rule: a validator rule that scanned acceptance text for selection vocabulary was proposed and rejected, because that vocabulary is ordinary plan prose used in roles carrying no selection semantics and a keyword scan over prose is not statically decidable for the property it would claim to detect. The judgment is the author's, and it is not automated. `.claude/rules/plan-acceptance-gates.md` records the rejection and the reason for it.
- **Check that the task-ordering does not make the condition unsatisfiable.** No rule covers this. A gate that runs a test path containing deliberately-failing cases added by an earlier task, before the later task that makes them pass, cannot exit 0. A baseline captured after a write-mode formatter has already repaired pre-existing drift becomes either a blanket waiver or makes a later gate unsatisfiable. Read each acceptance condition against the state the plan will actually be in when its task runs.

## Plan-Path Continuity Contract (Mandatory)

When a caller provides an explicit target plan file path (for example `${plan-path}` or `${file}`):

- Planner MUST update that exact file in place.
- Planner MUST reuse the same file for all preflight revision iterations.
- Planner MUST NOT create additional timestamped sibling files (for example `plan.<timestamp>.md`) during the same planning cycle.

If the provided path does not exist, it may be created once, then reused for all subsequent revisions in that cycle.

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
