# Remediation Inputs: Coverage Threshold Policy Reconciliation (Issue #494)

Timestamp: 2026-08-11T13-57
Status: REMEDIATION-REQUIRED
Primary requirement source for remediation planning: this file.

## Blocking condition

`evidence/other/upstream-reconciliation-receipt-check.2026-08-11T13-46.md` establishes that no valid upstream customization release/validation receipt exists. The absence blocks verification of the upstream-owned policy reconciliation and gate implementation. `evidence/other/upstream-reconciliation-disposition.2026-08-11T13-46.md` records `BLOCKED: UPSTREAM CUSTOMIZATION RELEASE EVIDENCE ABSENT`.

## Required fixes

1. In the upstream customization source repository, locate the canonical sources that generate or publish TaskMaster `CLAUDE.md` and executable `.claude/**` customization surfaces. Do not modify those generated/customization paths in TaskMaster.
2. Reconcile coverage thresholds, denominator/exemption policy, authority declaration, branch-coverage disposition, and the #424/#230 written-policy disposition according to issue #494 and `spec.md` AC1-AC5.
3. Resolve the false `quality-tiers.yml`, `tier-classification`, and `docs/ci.research.md` claims and record every required threshold-site disposition for AC6 and AC10.
4. Update the upstream feature-review coverage gate so its documentation and implementation agree, required coverage artifacts fail closed when missing or malformed, and the documented branch-coverage disposition is applied.
5. Add deterministic upstream tests for permitted, below-threshold, missing-input, malformed-input, and exact-boundary scenarios. Include an auditable below-threshold negative-path result.
6. Regenerate/package/publish the upstream customization output without directly placing generated TaskMaster `CLAUDE.md` or executable `.claude/**` files in this feature branch.
7. Write an upstream release/validation receipt below `<FEATURE>/evidence/other/`. It must include: upstream changed source paths; generation/publication mechanism; exact validation commands with results and exit codes; final coverage-policy values; missing/malformed-input behavior; branch-coverage disposition; deterministic test evidence; every generated TaskMaster path affected by a future supported publication; and explicit issue #512 non-interference.
8. Refresh `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` against `epic/build-ci-coverage-gate-fidelity-integration`, then repeat feature review. Only then individually check off any verified `spec.md` criteria.

## Required verification commands

- Upstream repository: run the upstream generator/package validation and its complete deterministic test suite; record exact commands, exit codes, and output summaries in the receipt.
- TaskMaster receipt validation: inspect the receipt against the required-field list in `evidence/other/upstream-reconciliation-receipt-check.2026-08-11T13-46.md`.
- TaskMaster scope validation: `git diff --check c7d398c2aa0da6963de239ff6719b4b23a7d3f45..HEAD` and confirm no direct TaskMaster `CLAUDE.md` or executable `.claude/**` customization change.
- Re-review validation: run `validate_orchestration_artifacts` for policy audit, code review, and feature audit after the fresh review artifacts are written.

## Do not do

- Do not directly edit TaskMaster `CLAUDE.md` or executable `.claude/**` customization paths.
- Do not alter issue #512-owned C# toolchain commands, `.claude/rules/csharp.md`, or C# QA-skill content.
- Do not select or lower a threshold merely to match the measured C# coverage value.
- Do not weaken coverage, test, or review policy to obtain a passing review.
- Do not check off `spec.md` acceptance criteria without individual verified evidence.
- Do not modify TaskMaster source, tests, or configuration for this evidence-only remediation unless a separately approved scope change authorizes it.

## Context package

- Canonical PR context: `artifacts/pr_context.summary.txt`; `artifacts/pr_context.appendix.txt`.
- Review artifacts: `policy-audit.2026-08-11T13-57.md`; `code-review.2026-08-11T13-57.md`; `feature-audit.2026-08-11T13-57.md`.
- Original feature plan: `plan.2026-08-10T14-10.md`.
- Requirements: `issue.md`; `spec.md`; `user-story.md`.
- Existing upstream boundary and receipt evidence: `evidence/other/upstream-claude-policy-reconciliation-prompt.2026-08-11T12-41.md`; `evidence/other/upstream-reconciliation-receipt-check.2026-08-11T13-46.md`; `evidence/other/upstream-reconciliation-disposition.2026-08-11T13-46.md`; `evidence/other/upstream-ac-validation.2026-08-11T13-46.md`.
