# Remediation Plan: Bayesian Email Sorter Unit Tests Post-Remediation Review (#248)

- **Issue:** #248
- **Requirements Source:** `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/remediation-inputs.2026-07-06T19-22.md`
- **Feature Folder:** `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248`
- **Original Plan:** `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/plan.2026-07-06T18-07.md`
- **Prior Remediation Plan:** `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/remediation-plan.2026-07-06T19-09.md`
- **Review Artifacts:** `policy-audit.2026-07-06T19-22.md`, `code-review.2026-07-06T19-22.md`, `feature-audit.2026-07-06T19-22.md`
- **PR Context:** `artifacts/pr_context.summary.txt`, `artifacts/pr_context.appendix.txt`
- **Planning Handoff Receipt:** `resolve_atomic_plan_prompt` returned success for this target file.
- **Status:** Blocked by repository-wide C# coverage debt.

## Remediation Scope

This plan records the post-remediation review blocker from `remediation-inputs.2026-07-06T19-22.md`.

- COV-1 remains unresolved: repository-wide C# line coverage is 20.21%, below the required 80% floor.
- TOOL-1 formatting enforcement passed with `dotnet tool run csharpier format .`; command-text reconciliation remains policy-owner follow-up.
- No production or test implementation edits are authorized by this post-remediation review.
- The required outcome is blocked state until coverage is raised to policy level or a policy-compliant exception is approved outside this feature review.

### Phase 0 — Blocker Baseline

- [ ] [P0-T1] Confirm the post-remediation review policy inputs.
  - Files: `AGENTS.md`, `.agents/skills/feature-review/SKILL.md`, `.agents/skills/feature-review-workflow/SKILL.md`, `.agents/skills/csharp/SKILL.md`, `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/remediation-inputs.2026-07-06T19-22.md`
  - Acceptance: Executor confirms policy still requires repository-wide C# line coverage >= 80% and that no local policy exception is recorded.

- [ ] [P0-T2] Confirm COV-1 remains the active PR-readiness blocker.
  - Files: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/remediation-baseline/coverage-floor-disposition.2026-07-06T19-09.md`, `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharp-vstest-coverage-remediation-final.2026-07-06T19-09.md`
  - Acceptance: Executor records that final line coverage remains 20.21% against the 80.00% floor and that issue #248 changed no production files.

### Phase 1 — Blocked Disposition

- [ ] [P1-T1] Preserve blocked status without implementation changes.
  - Files: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/policy-audit.2026-07-06T19-22.md`, `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/code-review.2026-07-06T19-22.md`, `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/feature-audit.2026-07-06T19-22.md`
  - Acceptance: Executor makes no production or test implementation edits and preserves COV-1 as blocked unless policy-compliant coverage evidence changes.

- [ ] [P1-T2] Route future work outside this issue scope.
  - Files: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/remediation-inputs.2026-07-06T19-22.md`
  - Acceptance: Executor records that broad repository-wide C# coverage expansion is outside the authorized issue #248 post-remediation review scope.

### Phase 2 — Final Validation

- [ ] [P2-T1] Validate post-remediation review artifacts.
  - Files: `policy-audit.2026-07-06T19-22.md`, `code-review.2026-07-06T19-22.md`, `feature-audit.2026-07-06T19-22.md`
  - Acceptance: `validate_orchestration_artifacts` passes for policy-audit, code-review, and feature-audit.

- [ ] [P2-T2] Validate this blocked remediation plan.
  - Files: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/remediation-plan.2026-07-06T19-22.md`
  - Acceptance: `validate_orchestration_artifacts` passes with `artifact_type: plan`.
