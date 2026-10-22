# Policy Compliance Audit: Issue #637 rooted breadcrumb selection normalization

**Audit Date:** 2026-08-31
**Base / Head:** `main` (`3be3f237a8551df3f27f83d9d1af2f26074fc93a`) / `a314228b9c3d9a4944a9e88e1a4eb4bd9c4b0f7b`

| Language | Files changed | Tests | Result | Baseline line coverage | Post-change line coverage | Changed/new code coverage |
|---|---:|---:|---|---:|---:|---|
| C# | 8 | 6,894 | PASS, 6,894 passed and 0 failed | 85.3428% | 85.3545% | 100.0% — changed production lines are covered |
| MSBuild XML | 2 | N/A | PASS; analyzer and nullable rebuilds passed | N/A | N/A | N/A |
| Markdown | 85 | N/A | PASS; `git diff --check main...HEAD` passed | N/A | N/A | N/A |

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - out of scope; zero TypeScript files changed`
- TypeScript post-change coverage artifact: `N/A - out of scope; zero TypeScript files changed`
- PowerShell baseline coverage artifact: `N/A - out of scope; zero PowerShell files changed`
- PowerShell post-change coverage artifact: `N/A - out of scope; zero PowerShell files changed`
- Per-language comparison summary: `### 1.2.1 Per-Language Coverage Comparison`

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.3428% lines -> Post-change: 85.3545% lines. Change: +0.0117 percentage points. New/changed-code coverage: 100.0% of emitted sequence points for `ToFilingStemOrVerbatim`, with no zero-hit changed-line intersection. Disposition: PASS. Evidence: `evidence/baseline/p0-t16-coverage-headline.md`, `coverage/p7-t5-postmerge.cobertura.xml`, and `evidence/qa-gates/p7-t7-changed-line-coverage.md`.

## Executive Summary

The functional change and current-head C# toolchain evidence support rooted-path normalization. The audit is not compliant overall because the feature modifies `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`, which has 694 lines. The repository policy applies the 500-line limit to modified test files, with no applicable exception. The specification's no-growth record does not override that policy.

## 1. General Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Independence and isolation | PASS | New router tests use Moq seams and clean up their in-memory log appender; helper tests invoke a pure static method. |
| Determinism and test results | PASS | Current-head coverage run: 9 assemblies, 6,894 passed, 0 failed. |
| Positive, negative, and boundary scenarios | PASS | Tests cover under-root, root-exact, out-of-root, boundary-near-miss, no-bound-root, relative, trash, and `SelectFirstRow` routes. |
| External dependencies and temporary files | PASS | Changed tests use mocks and no temporary-file APIs. |
| Readability and maintainability | FAIL | A modified test fixture remains 694 lines. |

## 2. General Code Change Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Objective and plan | PASS | `issue.md`, `spec.md`, research, and the atomic plan document the change. |
| Simplicity and separation of concerns | PASS | `SelectRow` normalizes only full rooted paths; a pure partial-file helper preserves the data-model size boundary. |
| Naming, contracts, and comments | PASS | The helper's documented total no-throw contract and stale deferral correction match its behavior. |
| Cohesive modules and 500-line maximum | FAIL | `BreadcrumbBridgeRouterIssue439Tests.cs` is modified and 694 lines. |
| Toolchain execution | PASS | Current-head CSharpier, analyzer, nullable, and MSTest coverage commands passed. |

## 3. Language-Specific Code Change Policy Compliance

### C#

| Requirement | Status | Evidence |
|---|---|---|
| CSharpier formatting | PASS | Current-head format and check passed. |
| Analyzer diagnostics | PASS | Analyzer rebuild passed with 0 errors; five existing `System.Reactive` packages.config warnings remained. |
| Nullable posture | PASS | Warnings-as-errors rebuild passed with 0 errors. |
| Focused design | PASS | The new partial type avoids extending `EfcDataModel.cs` beyond 500 lines and adds no public API. |

## 4. Language-Specific Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| MSTest framework | PASS | Changed tests use `[TestClass]` and `[TestMethod]`. |
| Moq and FluentAssertions | PASS | Router tests use Moq and FluentAssertions; pure tests do not add unnecessary mocks. |
| File-size limit for modified tests | FAIL | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` is 694 lines, above the policy maximum. |

## 5. Test Coverage Detail

Baseline line coverage was 85.3428% (`evidence/baseline/p0-t16-coverage-headline.md`). Current-head Cobertura reports 54,836 covered of 64,245 valid lines, or 85.3545%. Existing changed-line evidence records no zero-hit intersection and 100% condition coverage (6/6) for the helper gate. Coverage meets the 80% repository floor and the new-helper target.

## 6. Test Execution Metrics

The post-merge coverage run discovered 9 assemblies and reported 6,894 passed, 0 failed. No current-head test failure was observed.

## 7. Code Quality Checks

`git diff --check main...HEAD` passed. CSharpier, analyzer rebuild, nullable rebuild, and MSTest coverage passed. The remaining quality failure is structural: the modified Issue #439 test fixture exceeds the maximum file length.

## 8. Gaps and Exceptions

The specification records that the 694-line Issue #439 fixture did not grow, but that is not an approved repository-policy exception. Remediation must split or reduce it below 500 lines while preserving test semantics and non-SDK project compile includes.

## 9. Summary of Changes

The feature normalizes rooted Outlook paths in `SelectRow`, adds a total filing-boundary helper, corrects one Issue #439 expected selection as the deliberate AC21 specification correction, and updates stale deferral records.

## 10. Compliance Verdict

FAIL — remediation is required before PR readiness because a modified test file exceeds the repository 500-line limit. The other reviewed behavioral, toolchain, and coverage checks passed.

## Appendix A: Test Inventory

- `BreadcrumbBridgeRouterIssue637Tests`: rooted-path selection coverage.
- `EfcDataModelIssue614Tests`: archive-stem and helper coverage.
- `BreadcrumbBridgeRouterIssue439Tests`: corrected expected selection and retained provider-lookup assertion.
- `EfcSelectionGuardTests`: unchanged behavior with stale-deferral wording correction.

## Appendix B: Toolchain Commands Reference

`dotnet tool run csharpier format .`

`dotnet tool run csharpier check .`

`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

`pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\p7-t5-postmerge.cobertura.xml`
