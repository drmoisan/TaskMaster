# Issue #494 — Receipt-Gated Remediation Plan

- **Issue:** #494
- **Work mode:** `full-bug`; acceptance criteria are tracked only in `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md`.
- **Primary remediation source:** `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/remediation-inputs.2026-08-11T13-57.md`.
- **Audits addressed:** `policy-audit.2026-08-11T13-57.md`, `code-review.2026-08-11T13-57.md`, and `feature-audit.2026-08-11T13-57.md` in the feature folder.

## Scope and evidence conventions

- The existing TaskMaster prompt `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/other/upstream-claude-policy-reconciliation-prompt.2026-08-11T12-41.md` is the only upstream-change deliverable. This plan does not authorize any write in `C:\Users\DanMoisan\repos\drm-copilot` or any other upstream checkout.
- Do not edit `CLAUDE.md`, executable `.claude/**`, source, tests, configuration, `artifacts/**`, `issue.md`, or `.claude/agent-memory/**` in TaskMaster. The only permitted execution writes are canonical feature evidence files below `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/<kind>/`, refreshed PR-context files after a valid receipt exists, fresh review artifacts, and individual verified checkbox markers in `spec.md`.
- Evidence filenames use an execution-time `yyyy-MM-ddTHH-mm` suffix. Command evidence includes `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Evidence is never written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, or `artifacts/evidence/`.

## Human interaction requirement

```yaml
human_interaction:
  requirements:
    - id: upstream-release-validation-receipt
      requirement: An upstream customization owner must supply a release/validation receipt satisfying the required-field list in evidence/other/upstream-reconciliation-receipt-check.2026-08-11T13-46.md.
      response: halt
      blocked_reason: BLOCKED: UPSTREAM CUSTOMIZATION RELEASE EVIDENCE ABSENT
      resume_condition: Resume only after a receipt is present below docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/other/ and passes automated required-field validation.
```

This is a recorded halt condition, not a manual operator task. No plan task requests a person to perform verification or edits.

### Phase 0 — Policy, Scope, and Input Baseline

- [ ] [P0-T1] Read `AGENTS.md`, `.agents/skills/policy-compliance-order/SKILL.md`, `.agents/skills/atomic-plan-contract/SKILL.md`, `.agents/skills/evidence-and-timestamp-conventions/SKILL.md`, and `.agents/skills/acceptance-criteria-tracking/SKILL.md` in that order; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/remediation-baseline/phase0-instructions-read.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact lists every file in order and records the TaskMaster no-`CLAUDE.md`/no-executable-`.claude/**` boundary, the no-upstream-write boundary, and `spec.md` as the full-bug AC source.

- [ ] [P0-T2] Capture `git rev-parse HEAD` and `git status --porcelain` in `C:\Users\DanMoisan\repos\TaskMaster`; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/remediation-baseline/taskmaster-git-state.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact records the SHA and complete porcelain output without claiming a clean worktree or modifying any baseline path.

- [ ] [P0-T3] Inspect `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/other/upstream-claude-policy-reconciliation-prompt.2026-08-11T12-41.md`; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/remediation-baseline/upstream-prompt-contract-check.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact confirms that the prompt names issue #494, identifies the upstream-only source boundary, requires reconciled policy and fail-closed gate behavior, deterministic tests, publication information, every affected future TaskMaster path, and #512 non-interference; it records no claimed upstream execution.

### Phase 1 — Automated Receipt Gate

- [ ] [P1-T1] Search `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/other/` for an upstream release/validation receipt and validate it against `evidence/other/upstream-reconciliation-receipt-check.2026-08-11T13-46.md`; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/other/upstream-reconciliation-receipt-check.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact records the exact search scope, filename patterns, every candidate path, and a field-by-field result for upstream changed source paths, generation/publication mechanism, exact validation commands/results/exit codes, final policy values, missing/malformed-input behavior, branch disposition, deterministic test evidence, future TaskMaster output paths, and #512 non-interference.

- [ ] [P1-T2] Write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/other/upstream-reconciliation-disposition.<runtime ISO timestamp>.md` from the P1-T1 result.
  - Acceptance: a complete valid receipt produces `RECEIPT-VALIDATED`; any absent, incomplete, or malformed receipt produces exactly `BLOCKED: UPSTREAM CUSTOMIZATION RELEASE EVIDENCE ABSENT`, identifies the missing fields, and cites the `human_interaction` halt requirement in this plan.

- [ ] [P1-T3] If P1-T2 is blocked, halt plan execution after recording its evidence and leave every task in Phase 2 unchecked.
  - Acceptance: execution reports the recorded blocked reason and resume condition without adding a manual task, editing any upstream checkout, or checking off any `spec.md` acceptance criterion.

### Phase 2 — Receipt-Dependent TaskMaster Reconciliation

- [ ] [P2-T1] After P1-T2 reports `RECEIPT-VALIDATED`, run `git diff --check c7d398c2aa0da6963de239ff6719b4b23a7d3f45..HEAD` and inspect the changed paths for `CLAUDE.md` and executable `.claude/**`; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/qa-gates/taskmaster-scope-validation.<runtime ISO timestamp>.md`.
  - Acceptance: the command exits zero and the artifact proves no direct TaskMaster `CLAUDE.md` or executable `.claude/**` customization path changed; any prohibited path records `REMEDIATION-REQUIRED` and prevents subsequent acceptance check-off.

- [ ] [P2-T2] Refresh `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` against `epic/build-ci-coverage-gate-fidelity-integration` using the repository automation adapter.
  - Acceptance: both refreshed files identify the current HEAD and merge base and enumerate the validated receipt and feature evidence without a stale branch or SHA claim.

- [ ] [P2-T3] Produce fresh `policy-audit.<runtime ISO timestamp>.md`, `code-review.<runtime ISO timestamp>.md`, and `feature-audit.<runtime ISO timestamp>.md` in `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494`, then run `validate_orchestration_artifacts` for each audit.
  - Acceptance: each validator succeeds; the audits cite P1-T1, P1-T2, and P2-T1 evidence, evaluate AC1 through AC10 individually, and retain a blocked or failed verdict for any unverified receipt field.

- [ ] [P2-T4] For each `spec.md` AC individually marked PASS by the P2-T3 feature audit, change only its matching marker from `- [ ]` to `- [x]` in `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md`; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/issue-updates/ac-status-summary.<runtime ISO timestamp>.md`.
  - Acceptance: the summary lists all ten ACs with evidence paths, checked and unchecked totals, and remaining criteria; criterion text is unchanged and every non-PASS criterion remains unchecked.
