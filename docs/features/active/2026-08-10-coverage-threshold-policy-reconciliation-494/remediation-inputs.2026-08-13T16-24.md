# Remediation Inputs: Issue #494 documentation-scope reconciliation

Timestamp: 2026-08-13T16-24

## Authoritative requirements

- Primary: this file.
- Binding boundary: `evidence/other/upstream-claude-policy-reconciliation-prompt.2026-08-11T12-41.md`, Usage boundary.
- Review artifacts: `policy-audit.2026-08-13T16-24.md`, `code-review.2026-08-13T16-24.md`, and `feature-audit.2026-08-13T16-24.md`.
- Canonical PR context: `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt`.
- Original feature plan: `plan.2026-08-10T14-10.md`.

## Required fixes

1. In `issue.md`, narrow the User-Authorized Scope Correction and AC1/AC6 wording so it prohibits TaskMaster Claude runtime customization paths (`CLAUDE.md` and non-memory `.claude/**` rules, hooks, skills, agents, settings, and generated runtime assets) while explicitly permitting repository-specific `.claude/agent-memory/**` records.
   - Expected behavior: the six reviewed memory paths are correctly treated as permitted and no non-memory Claude runtime path is allowed.
   - Verification: `git diff --name-status epic/build-ci-coverage-gate-fidelity-integration...HEAD -- CLAUDE.md .claude` and targeted text checks of AC1/AC6.
2. Make the same scope distinction in `spec.md` and `plan.2026-08-10T14-10.md`, including every statement that says no `.claude/**` changes.
   - Expected behavior: one consistent local scope contract, without weakening the prohibition against Claude runtime customizations or external repository work.
   - Verification: `rg -n "agent-memory|\.claude/\*\*|Claude runtime|Scope Correction" issue.md spec.md plan.2026-08-10T14-10.md`.
3. Re-evaluate AC1 and AC6 against the corrected wording and fresh range evidence; update only their markdown checkbox markers if and only if individually verified.
   - Expected behavior: source status agrees with the feature audit and evidence map.
   - Verification: targeted Pester test, targeted analyzer baseline comparison, `git diff --check`, and a fresh feature audit.

## Do not do

- Do not modify `CLAUDE.md`, non-memory `.claude/**`, `.agents/skills/**`, production PowerShell, or Pester tests.
- Do not remove or rewrite the six existing `.claude/agent-memory/**` files.
- Do not contact or modify an external repository and do not require an upstream receipt, release, publication, or validation.
- Do not change the 80% local threshold or issue #512-owned C# toolchain policy.
- Do not weaken acceptance criteria, omit validation, or silently skip a failed check.

## Existing verification evidence

- Targeted Pester: passed for the two modified test files (51 passing tests recorded in `evidence/qa-gates/powershell-test-coverage.2026-08-13T16-08.md`).
- Targeted analyzer: one pre-existing warning at `Invoke-MSTestWithCoverage.Helpers.ps1:141`; no evaluator diagnostic.
- Diff whitespace: `git diff --check epic/build-ci-coverage-gate-fidelity-integration...HEAD` exited 0.
