# Remediation Inputs: Bayesian Email Sorter Unit Tests (#248)

Timestamp: 2026-07-06T19-09

## Primary Requirements Source

This remediation input file is the authoritative requirements source for the remediation plan generated from the issue #248 feature review.

## Review Artifacts

- Policy audit: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/policy-audit.2026-07-06T19-09.md`
- Code review: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/code-review.2026-07-06T19-09.md`
- Feature audit: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/feature-audit.2026-07-06T19-09.md`
- PR context summary: `artifacts/pr_context.summary.txt`
- PR context appendix: `artifacts/pr_context.appendix.txt`
- Original plan: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/plan.2026-07-06T18-07.md`

## Findings Requiring Remediation

### COV-1: Repository-wide C# line coverage below policy floor

- Severity: Major policy blocker
- Finding source: `policy-audit.2026-07-06T19-09.md`, sections `## 8. Gaps and Exceptions` and `## 10. Compliance Verdict`
- Evidence source: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/csharp-coverage-comparison.2026-07-06T18-07.md`
- Current evidence:
  - Baseline full-suite line coverage: 18.54%
  - Final full-suite line coverage: 20.21%
  - Delta: +1.67 percentage points
  - Required repository-wide floor: 80%
- Expected behavior:
  - The remediation executor must either produce C# coverage evidence showing repository-wide line coverage at or above 80%, or record a valid blocked disposition if the required uplift cannot be completed within an automated remediation cycle.
  - The remediation executor must not weaken the coverage policy, lower the threshold, or mark coverage as informational.
- Required verification commands:
  - `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`
  - Coverage comparison command or script used by the repository evidence process to produce numeric baseline, final, delta, and changed-code coverage values.

### TOOL-1: CSharpier command contract mismatch

- Severity: Minor policy/tooling blocker
- Finding source: `policy-audit.2026-07-06T19-09.md`, sections `## 2.5 After Making Changes - Toolchain Execution`, `## 7. Code Quality Checks`, and `## 8. Gaps and Exceptions`
- Evidence source: `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/qa-gates/minor-audit-readiness.2026-07-06T18-07.md`
- Current evidence:
  - Planned command `dotnet tool run csharpier .` exited 1 because the pinned local CSharpier CLI requires an explicit subcommand.
  - Compatible command `dotnet tool run csharpier format .` exited 0.
- Expected behavior:
  - The remediation executor must reconcile the formatter invocation used by issue execution with the repository-approved command contract without weakening formatting requirements.
  - The final remediation evidence must show a passing formatter command and explain whether the accepted invocation is the policy-listed command, an approved equivalent, or a tooling correction that preserves formatting enforcement.
- Required verification command:
  - `dotnet tool run csharpier .` or an approved repository-equivalent CSharpier command that is documented in remediation evidence.

## Do Not Do

- Do not modify production or test implementation files during this review handoff.
- Do not weaken policy documents or lower coverage thresholds to make the review pass.
- Do not mark C# coverage as N/A, informational, or out of scope while C# files are changed in the branch.
- Do not silently skip formatter, analyzer, nullable, test, or coverage verification.
- Do not create evidence outside `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/<kind>/`.
- Do not create manual-only remediation steps unless the automated workflow records a blocked disposition.

## Required Remediation Outputs

- Updated or new remediation evidence under `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/evidence/remediation-baseline/`, `evidence/regression-testing/`, or `evidence/qa-gates/` as appropriate.
- A remediation execution plan at `docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/remediation-plan.2026-07-06T19-09.md`.
- A subsequent feature review re-run after remediation completes.
