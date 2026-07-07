# Remediation Inputs: Bayesian Email Sorter Unit Tests Post-Remediation Review (#248)

Timestamp: 2026-07-06T19-22

## Primary Requirements Source

This remediation input file is the authoritative requirements source for the post-remediation blocked disposition from the issue #248 feature review.

## Review Artifacts

- Policy audit: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/policy-audit.2026-07-06T19-22.md`
- Code review: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/code-review.2026-07-06T19-22.md`
- Feature audit: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/feature-audit.2026-07-06T19-22.md`
- PR context summary: `artifacts/pr_context.summary.txt`
- PR context appendix: `artifacts/pr_context.appendix.txt`
- Original plan: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/plan.2026-07-06T18-07.md`
- Prior remediation plan: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/remediation-plan.2026-07-06T19-09.md`

## Findings Requiring Remediation or Blocked Disposition

### COV-1: Repository-wide C# line coverage below policy floor

- Severity: Blocker
- Finding source: `policy-audit.2026-07-06T19-22.md`, sections `## 8. Gaps and Exceptions` and `## 10. Compliance Verdict`
- Evidence source: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/remediation-baseline/coverage-floor-disposition.2026-07-06T19-09.md`
- Current evidence:
  - Final full-suite C# line coverage: 20.21%
  - Required repository-wide C# line coverage floor: 80.00%
  - Numeric gap: 59.79 percentage points
  - Final remediation tests: 486 total, 486 passed, 0 failed
  - Disposition: `BLOCKED_BY_REPOSITORY_WIDE_COVERAGE_DEBT`
- Expected behavior:
  - The branch must not be marked PR-ready while repository-wide C# line coverage remains below 80%.
  - Any future remediation must either raise repository-wide C# line coverage to at least 80% or record an explicit policy-compliant exception approved outside this feature-review workflow.
  - The workflow must not weaken policy, lower the threshold, or mark C# coverage as out of scope while C# files are changed.
- Required verification commands:
  - `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`
  - Coverage comparison producing numeric baseline, final, delta, and changed-code coverage values.

### TOOL-1: CSharpier command contract mismatch follow-up

- Severity: Minor
- Finding source: `policy-audit.2026-07-06T19-22.md`, sections `## 2.5 After Making Changes - Toolchain Execution` and `## 8. Gaps and Exceptions`
- Evidence source: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharpier-remediation-final.2026-07-06T19-09.md`
- Current evidence:
  - `dotnet tool run csharpier format .` exited 0.
  - The policy-listed shorthand `dotnet tool run csharpier .` remains incompatible with pinned CSharpier 1.2.6.
- Expected behavior:
  - Formatting enforcement must remain in place.
  - Policy-owner follow-up should reconcile repository command text with the pinned local CLI syntax.
- Required verification command:
  - `dotnet tool run csharpier format .` or a policy-approved equivalent.

## Do Not Do

- Do not modify production or test implementation files as part of this review handoff.
- Do not weaken policy documents or lower coverage thresholds.
- Do not mark C# coverage as N/A, informational, or out of scope while C# files are changed in the branch.
- Do not silently skip formatter, analyzer, nullable, test, or coverage verification.
- Do not create evidence outside `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/<kind>/`.
- Do not create manual-only remediation steps unless the automated workflow records blocked state.

## Required Remediation Outputs

- A blocked remediation plan at `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/remediation-plan.2026-07-06T19-22.md`.
- No production or test code changes from this post-remediation review unless a future authorized workflow expands scope.
- A future feature-review re-run only after COV-1 is resolved or explicitly exempted by policy.
