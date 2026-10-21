# Policy Compliance Audit: Efc full-path destination resolution regression (#609)

**Audit Date:** 2026-08-25
**Code Under Test:** `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`; `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs`; `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`; `UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs`

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---:|---:|---|---:|---:|---:|
| C# | 4 | 5 added regression tests | PARTIAL | 84.7756% | 84.7853% | New projection branches: 100% (6/6) |

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - out of scope`.
- TypeScript post-change coverage artifact: `N/A - out of scope`.
- PowerShell baseline coverage artifact: `N/A - out of scope`.
- PowerShell post-change coverage artifact: `N/A - out of scope`.
- Per-language comparison summary: `### 1.2.1 Per-Language Coverage Comparison` below.
- C# baseline: `evidence/baseline/issue-609-baseline.cobertura.xml` — 53,752 / 63,405 = 84.7756%.
- C# post-change: `evidence/qa-gates/issue-609-remediation-final.cobertura.xml` — 53,761 / 63,418 = 84.7853%.
- Comparison: `evidence/qa-gates/issue-609-remediation-coverage-comparison.2026-08-25T14-18.md`.

## Executive Summary

The audited branch is `bug/efc-full-path-destination-resolution-regression-609` at `a8f6c276f4ddf8138f2bc2888536148ef17d4fa2`, compared with `origin/main` at `b5c751519c6cf0eaeb2326d9e80b2439aeee7265`. The committed QA evidence records successful formatting, analyzer, nullable, and coverage-enabled MSTest gates. The change follows the intended narrow scope, but the startup projection uses a case-sensitive prefix match despite the scoped Outlook hierarchy-path contract being case-insensitive. Compliance is therefore PARTIAL and remediation is required.

Policy documents evaluated: `AGENTS.md`; general code-change and unit-test policies; C# code-change and unit-test policies; and the feature review workflow policies. No temporary scripts or policy documents were introduced or modified.

## 1. General Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Independence, isolation, and determinism | PASS | New tests use strict Moq seams and deterministic strings; no Outlook, UI, network, or temporary files. |
| Readability and Arrange-Act-Assert | PASS | `Issue609_` test names and local arrange/act/assert blocks are clear. |
| Coverage and scenarios | PARTIAL | Exact-case in-root, relative, and out-of-root paths are covered, but no case-variant in-root hierarchy-path scenario covers the case-insensitive contract. |
| External dependency policy | PASS | The changed tests use mocks only. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 84.7756% lines -> Post-change: 84.7853% lines. Change: +0.0097 percentage points. New/changed-code coverage: projection branches 100% (6/6). Disposition: PASS for coverage thresholds; overall audit remains PARTIAL because the case-insensitive Outlook hierarchy behavior is untested. Evidence: `evidence/baseline/issue-609-baseline.cobertura.xml`; `evidence/qa-gates/issue-609-remediation-final.cobertura.xml`; `evidence/qa-gates/issue-609-remediation-coverage-comparison.2026-08-25T14-18.md`.

## 2. General Code Change Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Scoped objective and plan | PASS | `spec.md`, both plans, and PR context identify a narrow FolderPredictor projection with router/configuration compatibility tests. |
| Simplicity and separation of concerns | PASS | The production change is confined to a private projection helper and aligned row-score construction. |
| Module and API boundaries | PASS | No public API or unrelated production boundary was changed; diff inspection confirms router, `EmailFilerConfig`, `EfcDataModel`, and controller sources are unchanged. |
| Correctness and compatibility | PARTIAL | `ProjectSuggestionPath` at `FolderPredictor.cs:852-858` calls `StartsWith(archivePrefix, StringComparison.Ordinal)`, leaving case-variant in-root Outlook paths unprojected. |
| Toolchain execution | PASS | Final evidence records a restarted clean C# toolchain and 6,479/6,479 coverage-enabled MSTest passes. |

## 3. Language-Specific Code Change Policy Compliance

### C#

| Requirement | Status | Evidence |
|---|---|---|
| CSharpier | PASS | `evidence/qa-gates/csharpier-check-final.2026-08-25T14-18.md`: check exit 0; reviewer recheck `dotnet tool run csharpier check .` exit 0. |
| Analyzers | PASS | `evidence/qa-gates/csharp-analyzers-final.2026-08-25T14-18.md`: rebuild exit 0, analyzer diagnostics 0. |
| Nullable/compiler analysis | PASS | `evidence/qa-gates/csharp-nullable-final.2026-08-25T14-18.md`: rebuild exit 0, compiler and nullable diagnostics 0. |
| Null and path contract safety | PARTIAL | The null-globals guard is covered, but case-sensitive path comparison conflicts with the feature's case-insensitive Outlook `FolderPath` contract. |

## 4. Language-Specific Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| MSTest/Moq/FluentAssertions | PASS | Changed test projects use MSTest attributes, Moq, and FluentAssertions. |
| Positive, boundary, and compatibility coverage | PARTIAL | Direct selection, ancestor, immediate-child, `@` mailbox prefixing, exact root, relative, and out-of-root cases are covered; case-variant in-root is absent. |
| No prohibited temporary files | PASS | Source inspection and evidence show none. |

## 5. Test Coverage Detail

`FolderPredictor.cs` improved from 468/524 (89.3130%) to 481/537 (89.5717%). The committed comparison reports the new null-globals branch as 2/2 and archive-prefix conditional as 4/4. Numeric coverage passes policy thresholds but does not cover the missing case-insensitive behavior.

## 6. Test Execution Metrics

| Metric | Value | Status |
|---|---|---|
| Full coverage suite | 6,479 / 6,479 passed | PASS |
| Filtered Issue609/Issue439/configuration suite | 27 / 27 passed | PASS (committed evidence) |
| Reviewer format recheck | 1,520 files checked | PASS |
| Reviewer VSTest recheck | `vstest.console.exe` unavailable on reviewer PATH | UNVERIFIED locally; committed final evidence inspected |

## 7. Code Quality Checks

| Check | Command | Result | Status |
|---|---|---|---|
| Formatting | `dotnet tool run csharpier check .` | exit 0 in committed evidence and reviewer recheck | PASS |
| Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | exit 0; 0 diagnostics | PASS |
| Nullable | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | exit 0; 0 compiler/nullable diagnostics | PASS |
| Tests | `Invoke-MSTestWithCoverage.ps1` | exit 0; 6,479/6,479 passed | PASS |
| Diff hygiene | `git diff --check origin/main...HEAD` | exit 0 | PASS |

## 8. Gaps and Exceptions

### Identified Gaps

1. `FolderPredictor.cs:852-858` compares a persisted Outlook hierarchy path with `StringComparison.Ordinal`. The feature specification states that the Outlook `FolderPath` contract is case-insensitive. A suggestion whose archive root differs only by case remains full and can reintroduce the double-prefix failure.

### Approved Exceptions

None.

### Removed/Skipped Tests

No planned tests were removed. The reviewer could not rerun VSTest because its executable is not on PATH; this does not replace the committed coverage evidence and is recorded as a local limitation.

## 9. Summary of Changes

1. `6541e749` — `test(efc): cover archive-relative path boundary`.
2. `a8f6c276` — `fix(efc): project startup suggestions relative to archive`.

The branch adds Efc router and configuration compatibility tests, adds a direct FolderPredictor regression, and introduces a private startup suggestion projection. The project remains narrowly scoped, but the comparison semantics require correction and a regression test.

## 10. Compliance Verdict

### Overall Status: PARTIALLY COMPLIANT

The branch has valid baseline and final QA/coverage evidence and follows the intended scope. It is not fully compliant with the specified Outlook path semantics because the root comparison is case-sensitive. Remediation is required before merge readiness.

## Appendix A: Test Inventory

- `BreadcrumbBridgeRouterIssue439Tests.Issue609_DirectRowSelection_UsesFullLookupAndRelativeFilingTarget`
- `BreadcrumbBridgeRouterIssue439Tests.Issue609_AncestorActivation_EmitsArchiveRelativeFilingTarget`
- `BreadcrumbBridgeRouterIssue439Tests.Issue609_ImmediateChildActivation_EmitsArchiveRelativeFilingTarget`
- `EmailFilerConfig_Tests.Issue609_ResolvePaths_PrefixesAtMailboxArchiveRootExactlyOnce`
- `FolderPredictorTests.Issue609_FolderPredictor_ProjectsOnlyInRootFullSuggestionPaths`

## Appendix B: Toolchain Commands Reference

```powershell
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-25-efc-full-path-destination-resolution-regression-609/evidence/qa-gates/issue-609-remediation-final.cobertura.xml
```

**Audit Completed By:** feature-reviewer-c3
**Policy Version:** Current as of 2026-08-25
