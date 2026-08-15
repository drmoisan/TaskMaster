# Remediation Plan: Issue #494 document-scope correction

- **Feature:** `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494`
- **Work mode:** `full-bug`.
- **Acceptance-criteria source:** The `## Acceptance Criteria` section in `spec.md` is the sole acceptance-criteria source for this remediation. `issue.md`, `user-story.md`, prior plans, review artifacts, and evidence provide context only and must not be used for acceptance-criteria check-off.
- **Inputs:** `remediation-inputs.2026-08-13T16-24.md` and the existing active `spec.md` and `user-story.md`.
- **Supersession:** The assigned document-scope correction is authoritative for this plan. It supersedes only the older remediation-input instructions that request edits to `issue.md` or `plan.2026-08-10T14-10.md`; those files are outside this remediation and are not to be changed or re-verified as implementation outputs.

## Scope and execution constraints

- The existing upstream prompt at `<FEATURE>/evidence/other/upstream-claude-policy-reconciliation-prompt.2026-08-11T12-41.md` is the complete local TaskMaster deliverable. This remediation must neither apply the prompt nor request, require, or prove an upstream receipt, release, publication, validation, or external-repository change.
- This plan authorizes changes only to `<FEATURE>/spec.md`, `<FEATURE>/user-story.md`, this exact plan file, and new canonical evidence below `<FEATURE>/evidence/<kind>/`.
- Requirements in `<FEATURE>/remediation-inputs.2026-08-13T16-24.md` to edit `<FEATURE>/issue.md` or `<FEATURE>/plan.2026-08-10T14-10.md` are superseded by the assigned document-scope correction. The executor must not add tasks or changes for either superseded file.
- `CLAUDE.md`, every non-memory `.claude/**` path (including rules, hooks, skills, agents, settings, and generated runtime assets), `.agents/skills/**`, all source, all PowerShell and Pester files, configuration, `artifacts/**`, and external repositories are prohibited.
- The following six pre-existing `.claude/agent-memory/**` records are an immutable protected-path-classification exception only: `.claude/agent-memory/atomic-executor/MEMORY.md`; `.claude/agent-memory/atomic-executor/project_511_is_a_testhost_crash_not_n_failing_tests.md`; `.claude/agent-memory/atomic-executor/project_pester5_result_shape_container_tests_and_ci_codecoverage.md`; `.claude/agent-memory/atomic-planner/MEMORY.md`; `.claude/agent-memory/atomic-planner/poshqc-mcp-and-msbuild-invocation-facts.md`; and `.claude/agent-memory/atomic-planner/project_494_threshold_reconciliation_plan_seams.md`. Do not edit, create, delete, rename, stage, or otherwise modify any of these six records; do not modify their content or history.
- Historical, forward-looking Claude and Codex scenarios in `user-story.md` are descriptive records only. They must be labelled non-executable and must not authorize runtime-policy, coverage-runner, Pester, or external work.
- Active implementation scope is limited to the existing TaskMaster coverage runner and Pester work already present in the repository. This remediation does not reopen, implement, test, re-evaluate, or plan that coverage work.
- `<FEATURE>` means `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494`. Every evidence artifact must be stored under `<FEATURE>/evidence/<kind>/` and include `Timestamp:`, `Command:` and `EXIT_CODE:` when a command runs, and `Output Summary:`.

### Phase 0 — Policy and protected-path baseline

- [x] [P0-T1] Read `AGENTS.md`, `.agents/skills/policy-compliance-order/SKILL.md`, `.agents/skills/atomic-plan-contract/SKILL.md`, `.agents/skills/evidence-and-timestamp-conventions/SKILL.md`, `<FEATURE>/remediation-inputs.2026-08-13T16-24.md`, `<FEATURE>/spec.md`, `<FEATURE>/user-story.md`, and this plan in that order; record the ordered read list in `<FEATURE>/evidence/remediation-baseline/phase0-policy-read.2026-08-13T16-24.md`.
  - Acceptance: the artifact contains `Timestamp:`, `Output Summary:`, the ordered file list, and an explicit statement that only the `## Acceptance Criteria` section in `<FEATURE>/spec.md` is the acceptance-criteria source for this remediation.
- [x] [P0-T2] Run `git status --porcelain` and `git diff --name-status epic/build-ci-coverage-gate-fidelity-integration...HEAD -- CLAUDE.md .claude .agents/skills`; save the unmodified outputs and protected-path classification to `<FEATURE>/evidence/remediation-baseline/protected-path-baseline.2026-08-13T16-24.md`.
  - Acceptance: the artifact records each exact command, `EXIT_CODE:`, and `Output Summary:`; classifies exactly the six paths named in Scope and execution constraints as immutable classification-only records; reports any changed `CLAUDE.md`, non-memory `.claude/**`, or `.agents/skills/**` path as remediation-required; and records pre-existing working-tree paths as exclusions from this remediation.

### Phase 1 — Acceptance source and user-story scope correction

- [x] [P1-T1] Update `<FEATURE>/spec.md` only to state that its `## Acceptance Criteria` section is the sole acceptance-criteria source for this remediation and that no other feature document, historical plan, or evidence artifact may be used for acceptance-criteria check-off.
  - Acceptance: `<FEATURE>/spec.md` contains one unambiguous sole-source statement adjacent to its active acceptance-criteria section, and no acceptance-criteria text or checkbox marker is otherwise changed.
- [x] [P1-T2] Update `<FEATURE>/user-story.md` only to state that the `## Acceptance Criteria` section in `<FEATURE>/spec.md` is the sole acceptance-criteria source for this remediation and that `user-story.md` is narrative context only.
  - Acceptance: `<FEATURE>/user-story.md` contains the same sole-source rule as P1-T1 without retaining a conflicting reference to `issue.md` or any other acceptance-criteria authority.
- [x] [P1-T3] Add one operative scope-correction statement to `<FEATURE>/user-story.md` that identifies the existing upstream prompt as the local TaskMaster deliverable; prohibits `CLAUDE.md`, all non-memory `.claude/**`, `.agents/skills/**`, and external repositories; and limits the active scope to the existing coverage runner and Pester work without reopening that work.
  - Acceptance: the statement explicitly says that no upstream receipt, release, publication, validation, or external execution is required; lists rules, hooks, skills, agents, settings, and generated runtime assets as prohibited non-memory `.claude/**` classes; and does not authorize a coverage-runner or Pester edit, test, re-evaluation, or implementation task.
- [x] [P1-T4] Add the exact six paths named in Scope and execution constraints to the operative scope-correction statement in `<FEATURE>/user-story.md` as immutable, pre-existing `.claude/agent-memory/**` records permitted solely for protected-path classification.
  - Acceptance: `<FEATURE>/user-story.md` names all and only those six paths and expressly prohibits editing, creating, deleting, renaming, staging, or otherwise modifying them, including their content and history.
- [x] [P1-T5] Label the three forward-looking sections headed `Scenario — Planning a C# bug fix, after this feature lands`, `Scenario — A coverage regression reaches the gate`, and `Scenario — A future divergence appears` in `<FEATURE>/user-story.md` as historical and non-executable.
  - Acceptance: each named heading contains a `Historical, non-executable` label, and its body remains narrative rather than an implementation instruction or an authorization to alter Claude, Codex, coverage-runner, Pester, or external-repository surfaces.

### Phase 2 — Consistency and protected-path verification

- [x] [P2-T1] Run `rg -n "sole acceptance-criteria source|acceptance-criteria authority|local TaskMaster deliverable|non-memory|agent-memory|Historical, non-executable|external repositor|coverage runner|Pester" <FEATURE>/spec.md <FEATURE>/user-story.md`; write the output and source-consistency determination to `<FEATURE>/evidence/other/acceptance-source-and-scope-consistency.2026-08-13T16-24.md`.
  - Acceptance: the artifact has `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`; demonstrates that both documents identify `spec.md` as the sole acceptance-criteria source; demonstrates the user-story scope correction; confirms all three scenario labels; and records no coverage implementation or test re-evaluation result.
- [x] [P2-T2] Run `git diff --name-status epic/build-ci-coverage-gate-fidelity-integration...HEAD -- CLAUDE.md .claude .agents/skills` and `git diff --check epic/build-ci-coverage-gate-fidelity-integration...HEAD`; write both results and the protected-path determination to `<FEATURE>/evidence/qa-gates/protected-path-validation.2026-08-13T16-24.md`.
  - Acceptance: the artifact records both commands, exit codes, and output summaries; treats only the exact six immutable agent-memory paths as classification-only; reports any `CLAUDE.md`, non-memory `.claude/**`, or `.agents/skills/**` change as remediation-required; and records successful whitespace validation when `git diff --check` exits zero.

### Phase 3 — Final scoped QA and plan validation

- [x] [P3-T1] Compare the final `git status --porcelain` output with P0-T2 and write the scoped working-tree result to `<FEATURE>/evidence/qa-gates/remediation-final-scope-lock.2026-08-13T16-24.md`.
  - Acceptance: after excluding pre-existing working-tree paths recorded by P0-T2, this remediation changes only `<FEATURE>/spec.md`, `<FEATURE>/user-story.md`, this plan file, and canonical `<FEATURE>/evidence/<kind>/` paths; it introduces no changed protected runtime path, source, PowerShell file, Pester file, configuration path, `artifacts/**` path, or external-repository work.
- [x] [P3-T2] Run `mcp__drm-copilot__validate_orchestration_artifacts` with `artifact_type: "plan"` and `artifact_path: "docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/remediation-plan.2026-08-13T16-24.md"`.
  - Acceptance: validation succeeds for this exact plan path; the plan has sequential task IDs P0-T1 through P0-T2, P1-T1 through P1-T5, P2-T1 through P2-T2, and P3-T1 through P3-T2; and no task plans coverage-runner or Pester implementation, test execution, or coverage re-evaluation.
