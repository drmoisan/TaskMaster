# Remediation Inputs: qfc-high-confidence-queue-filter (Issue #218)

Timestamp: 2026-06-26T20-58

## Source Review Artifacts

- Policy audit: `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/policy-audit.2026-06-26T20-58.md`
- Code review: `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/code-review.2026-06-26T20-58.md`
- Feature audit: `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/feature-audit.2026-06-26T20-58.md`
- PR context summary: `artifacts/pr_context.summary.txt`
- PR context appendix: `artifacts/pr_context.appendix.txt`
- Original feature plan: `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/plan.2026-06-26T20-28.md`
- Acceptance criteria source: `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/issue.md`

## Remediation Trigger Summary

Remediation is required under the feature-review workflow because the policy audit contains FAIL and PARTIAL findings. The issue #218 acceptance criteria pass, and the implementation code review has no blocker or major correctness findings. The blocking findings are policy readiness findings.

## Enumerated Fix List

1. **C# repository-wide coverage threshold is not met.**
   - **Current evidence:** `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/evidence/qa-gates/coverage-comparison-218.md` records post-change C# line coverage as 100578 / 162106 lines, line-rate 0.6204458810901509.
   - **Expected behavior:** C# coverage evidence for the reviewed branch must satisfy the repository-wide line coverage threshold of at least 80% or have an authorized exception recorded outside policy-file weakening.
   - **Verification commands:** Run C# MSTest coverage with `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation /ResultsDirectory:TestResults\issue218-remediation-final`, convert the latest `.coverage` file to Cobertura, and compare against the issue #218 baseline.

2. **Changed C# files exceed the 500-line repository limit.**
   - **Current evidence:** `QuickFiler/Controllers/QfcDatamodel.cs` has 843 lines, `QuickFiler/Controllers/QfcHomeController.cs` has 739 lines, and `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs` has 1475 lines.
   - **Expected behavior:** Touched production and test files should be brought under the repository 500-line limit or an authorized exception must be documented without weakening policy files.
   - **Verification commands:** Run a deterministic line-count check over changed C# files and rerun the full C# toolchain after any refactor.

3. **Changed-production-line coverage is not isolated as a numeric percentage.**
   - **Current evidence:** `coverage-comparison-218.md` records no coverage regression and confirms issue #218 test coverage, but it does not provide a numeric changed-production-line coverage percentage for the production diff.
   - **Expected behavior:** Remediation evidence must explicitly state changed-production-line coverage for issue #218 changed production lines and whether it satisfies the applicable threshold.
   - **Verification commands:** Generate a changed-line coverage report for `QuickFiler/Controllers/QfcDatamodel.cs` and `QuickFiler/Controllers/QfcHomeController.cs` using the final Cobertura artifact and the merge-base diff.

## Do Not Do

- Do not modify repository policy files to make the review pass.
- Do not remove or weaken the issue #218 acceptance criteria.
- Do not remove focused issue #218 tests.
- Do not mark remediation complete without rerunning CSharpier, analyzer build, nullable build, MSTest coverage, and coverage comparison.
- Do not introduce manual-only validation steps unless the workflow records a blocked state.
- Do not change production behavior outside the remediation scope without updating the plan and evidence.

## Required Context Package

Any remediation planner or executor must read:

- `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/remediation-inputs.2026-06-26T20-58.md`
- `artifacts/pr_context.summary.txt`
- `artifacts/pr_context.appendix.txt`
- `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/policy-audit.2026-06-26T20-58.md`
- `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/code-review.2026-06-26T20-58.md`
- `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/feature-audit.2026-06-26T20-58.md`
- `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/plan.2026-06-26T20-28.md`
- `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/issue.md`
