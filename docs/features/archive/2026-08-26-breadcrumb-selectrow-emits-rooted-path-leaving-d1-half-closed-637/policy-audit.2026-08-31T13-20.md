# Policy Compliance Audit: Issue #637 rooted breadcrumb selection normalization

**Audit Date:** 2026-08-31
**Base / Head:** `main` (`3be3f237a8551df3f27f83d9d1af2f26074fc93a`) / `952a760fb19ff9c10007fe2ebb42f8cadd49a886`

| Language | Files changed | Tests | Result | Baseline line coverage | Post-change line coverage | Changed/new code coverage |
|---|---:|---:|---|---:|---:|---|
| C# | 9 | 6,894 | PASS, 6,894 passed and 0 failed | 85.3358% | 85.3389% | 100.0% — initial feature changed-line evidence covers the new helper; post-split remediation changes only test/project paths |
| MSBuild XML | 2 | N/A | PASS; analyzer and nullable rebuilds passed | N/A | N/A | N/A |
| Markdown | 108 | N/A | PASS; `git diff --check main...HEAD` passed | N/A | N/A | N/A |

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - zero TypeScript files changed in main...HEAD`
- TypeScript post-change coverage artifact: `N/A - zero TypeScript files changed in main...HEAD`
- PowerShell baseline coverage artifact: `N/A - zero PowerShell files changed in main...HEAD`
- PowerShell post-change coverage artifact: `N/A - zero PowerShell files changed in main...HEAD`
- Per-language comparison summary: `### 1.2.1 Per-Language Coverage Comparison`

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.3358% lines -> Post-change: 85.3389% lines. Change: +0.0031 percentage points. New/changed-code coverage: 100.0% for the initial helper lines; the post-split remediation changed no production-code sequence points. Disposition: PASS. Evidence: `evidence/qa-gates/p2-t5-mstest-coverage.md`, `evidence/qa-gates/p2-t6-coverage-comparison.md`, and `evidence/qa-gates/p7-t7-changed-line-coverage.md`.
- TypeScript: zero changed files. Disposition: N/A.
- PowerShell: zero changed files. Disposition: N/A.

## Executive Summary

This post-remediation audit reviewed the full `main...HEAD` feature range using fresh canonical PR context. The remediation splits the previously oversized Issue #439 test fixture into two cohesive partial files: 455 and 253 logical lines, respectively. Both are below the repository 500-line maximum. Live `git diff --check main...HEAD` and CSharpier check passed; current-head QA evidence records successful analyzer, nullable, and MSTest-with-coverage commands. No policy blocker remains.

## 1. General Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Independence and isolation | PASS | New rooted-selection tests use existing `IFolderHierarchyProvider` and `IBreadcrumbWebHost` Moq seams and detach the in-memory log appender in cleanup. |
| Determinism and test outcomes | PASS | `p2-t5-mstest-coverage.md` records 6,894 passed and 0 failed across 9 assemblies. |
| Positive, negative, boundary, and error scenarios | PASS | Issue #637 tests cover root-exact, under-root, case/trailing-separator, relative, Trash, out-of-root, separator near-miss, no-bound-root, and `SelectFirstRow`; helper tests cover root-exact, out-of-root, null/empty, and ancestor edge cases. |
| External dependencies and temporary files | PASS | Reviewed tests use mocks or pure static calls; no test temporary-file or external-service dependency was introduced. |
| Readability and maintainability | PASS | The remediated Issue #439 fixture is now split into two partial files below 500 lines. |

## 2. General Code Change Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Objective and plan | PASS | `issue.md`, `spec.md`, research, original atomic plan, and remediation plan define the completed scope. |
| Simplicity and separation of concerns | PASS | `SelectRow` normalizes only eligible full paths; a pure internal helper isolates `MoveToFolderAsync(string, ...)` normalization. |
| Naming, contracts, and comments | PASS | `ToFilingStemOrVerbatim` documents its verbatim/no-throw contract; stale deferral records now state that producer normalization is implemented. |
| Cohesive modules and 500-line maximum | PASS | Source and remediated test fixture logical line counts are within policy: 455 and 253 for the two Issue #439 partial fixtures. |
| Toolchain execution | PASS | The ordered current-head evidence set records CSharpier format/check, analyzer rebuild, nullable rebuild, and MSTest coverage as successful. |

## 3. Language-Specific Code Change Policy Compliance

### C#

| Requirement | Status | Evidence |
|---|---|---|
| CSharpier formatting | PASS | Live `dotnet tool run csharpier check .` checked 1,565 files without a violation; `p2-t1` format also exited 0. |
| Analyzer diagnostics | PASS | `p2-t3-msbuild-analyzers.md` records a successful analyzer-enabled rebuild with 0 errors. |
| Nullable posture | PASS | `p2-t4-msbuild-nullable.md` records a successful warnings-as-errors rebuild with 0 errors. |
| Focused design and visibility | PASS | The new helper is `internal static` on a partial `EfcDataModel`; no public API was added. |

## 4. Language-Specific Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| MSTest framework | PASS | Changed tests use `[TestClass]` and `[TestMethod]`. |
| Moq and FluentAssertions | PASS | Router coverage uses Moq seams and FluentAssertions; pure helper tests use FluentAssertions. |
| File-size limit for modified tests | PASS | `p1-t5-fixture-split-verification.md` records 455 and 253 logical lines; direct post-remediation count confirmed the same files remain below 500. |

## 5. Test Coverage Detail

The current-head post-split coverage artifact (`evidence/qa-gates/p2-t5-mstest-coverage.md`) records 85.3389% repository line coverage, exceeding the 80% floor, with 6,894 passed and 0 failed. `p2-t6-coverage-comparison.md` records a +0.0031 percentage-point change from the remediation baseline and no changed production-code sequence points for the fixture-only remediation. Earlier feature evidence records coverage for the new filing-stem helper; the remediation did not alter production code.

## 6. Test Execution Metrics

The current-head coverage wrapper discovered nine test assemblies, completed successfully, and reported 6,894 total tests passed with zero failures. Analyzer and nullable rebuilds each completed with zero errors; the evidence retains five pre-existing `System.Reactive` packages.config compatibility warnings from unrelated projects.

## 7. Code Quality Checks

`git diff --check main...HEAD` exited 0 during this review. `dotnet tool run csharpier check .` completed successfully during this review. The exact-head QA evidence additionally records successful format, analyzer, nullable, and coverage commands in required order.

## 8. Gaps and Exceptions

No policy exception is required. The prior 694-line modified test-fixture finding is resolved by the partial-fixture split. The authoritative `spec.md` remains unchanged by that remediation, including AC21's deliberate specification-correction language.

## 9. Summary of Changes

The feature normalizes rooted Outlook paths in `SelectRow`, uses a total internal filing-stem helper for the string `MoveToFolderAsync` path, corrects the superseded rooted-selection assertion without changing its rooted provider-lookup assertion, updates stale deferral records, and remediates the test fixture-size policy finding by splitting the Issue #439 tests.

## 10. Compliance Verdict

PASS — the full feature range satisfies the reviewed repository policy requirements. The post-remediation fixture split removes the only prior blocking finding, while the feature's behavior, AC21 invariant-driven specification correction, and QA evidence remain intact.

## Appendix A: Test Inventory

- `BreadcrumbBridgeRouterIssue637Tests`: rooted-path selection coverage.
- `EfcDataModelIssue614Tests`: archive-stem and helper coverage.
- `BreadcrumbBridgeRouterIssue439Tests` and `.Activation`: corrected expected selection and retained provider-lookup assertion.
- `EfcSelectionGuardTests`: unchanged behavior with updated stale-deferral wording.

## Appendix B: Toolchain Commands Reference

`dotnet tool run csharpier format .`

`dotnet tool run csharpier check .`

`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

`pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\p7-t5-remediation.cobertura.xml`
