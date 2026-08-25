# Policy Compliance Audit: Efc full-path destination resolution regression (#609)

**Audit Date:** 2026-08-25
**Code Under Test:** `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`; `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs`; compatibility tests in `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` and `UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs`.

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---:|---|---|---:|---:|---:|
| C# | 4 | 6 Issue609 regression tests | PASS: 6,479/6,479 full suite; 2/2 reviewer-focused | 53,760/63,418 (84.7709%) | 53,760/63,418 (84.7709%) | `ProjectSuggestionPath`: 100% lines and branches |

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - out of scope`.
- TypeScript post-change coverage artifact: `N/A - out of scope`.
- PowerShell baseline coverage artifact: `N/A - out of scope`.
- PowerShell post-change coverage artifact: `N/A - out of scope`.
- Per-language comparison summary: `evidence/qa-gates/issue-609-case-variant-coverage-comparison.2026-08-25T14-29.md`.
- C# baseline/final artifacts: `evidence/remediation-baseline/issue-609-case-variant-baseline.cobertura.xml` and `evidence/qa-gates/issue-609-case-variant-final.cobertura.xml`.

## Executive Summary

The audited branch is `bug/efc-full-path-destination-resolution-regression-609` at `67db82a928e6b0c023ed16bf42ca48e526f07a0e`, compared with `origin/main` at `507a40a549d573b221da0fb59c3e18af5ce8d473` using merge base `b5c751519c6cf0eaeb2326d9e80b2439aeee7265`. The active feature folder was deterministically selected from refreshed PR context and the #609 scoping documents. The branch remains within its scoped production change: startup suggestion presentation projects only an archive-root-plus-separator prefix, and the paired `FolderScore` mirrors the projected string.

The prior review's case-sensitivity gap is resolved. `ProjectSuggestionPath` now uses `StringComparison.OrdinalIgnoreCase`, and `Issue609_FolderPredictor_ProjectsCaseVariantInRootFullSuggestionPath` has both direct fail-before evidence (expected test exit 1) and post-fix evidence. Final recorded formatting, analyzer, nullable, coverage-enabled MSTest, coverage comparison, and diff-hygiene evidence all pass at the reviewed head; this reviewer independently reran the two FolderPredictor Issue609 tests, CSharpier check, and diff check successfully.

**Policy documents evaluated:** `AGENTS.md`; `.github/instructions/general-code-change.instructions.md`; `.github/instructions/general-unit-test.instructions.md`; `.github/instructions/csharp-code-change.instructions.md`; `.github/instructions/csharp-unit-test.instructions.md`; and the feature-review workflow skills.

## 1. General Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Independence, isolation, determinism | PASS | Tests use deterministic path literals and Moq `IApplicationGlobals`/Outlook seams; no live Outlook, network, UI, or temporary file. |
| Readability and Arrange-Act-Assert | PASS | `Issue609_` test names, local setup, action, and FluentAssertions make the expected projection explicit. |
| Coverage and scenario completeness | PASS | Exact-case in-root, case-variant in-root, already-relative, out-of-root, display array, row mirror, and aligned score-key behavior are asserted. |
| External dependency policy | PASS | Changed tests use mocks; source inspection found no temporary-file creation or external-service use. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 84.7709% lines -> Post-change: 84.7709% lines. Change: +0.0000% lines. New/changed-code coverage: 100%. Disposition: PASS. Evidence: `evidence/qa-gates/issue-609-case-variant-coverage-comparison.2026-08-25T14-29.md`.

## 2. General Code Change Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Scoped objective and documented plan | PASS | `spec.md`, `plan.2026-08-25T12-47.md`, and `plan.remediation.2026-08-25T14-18.md` constrain the correction to FolderPredictor startup projection. |
| Simplicity and separation of concerns | PASS | One private helper is used by both string and row projections; router, configuration, controller, persistence, COM, and filesystem paths are unchanged. |
| Module/API boundaries | PASS | No public API changed. The `FolderScore` copy preserves row/display alignment without altering the source scorer. |
| Naming, comments, and documentation | PASS | `ProjectSuggestionPath` accurately names the helper; no misleading or stale public documentation was introduced. |
| File-size rule | PASS | The changed C# source and test files remain below the repository's 500-line limit. |
| Diff hygiene | PASS | Reviewer command `git diff --check b5c751519c6cf0eaeb2326d9e80b2439aeee7265..67db82a928e6b0c023ed16bf42ca48e526f07a0e` exited 0. |

## 3. Language-Specific Code Change Policy Compliance

### C#

| Requirement | Status | Evidence |
|---|---|---|
| CSharpier | PASS | `dotnet tool run csharpier check .` exited 0 in reviewer verification; head evidence also records exit 0. |
| .NET analyzers | PASS | `evidence/qa-gates/csharp-analyzers-final.2026-08-25T14-29.md` records Rebuild exit 0 and zero analyzer diagnostics. |
| Compiler/nullable analysis | PASS | `evidence/qa-gates/csharp-nullable-final.2026-08-25T14-29.md` records Rebuild exit 0 with zero compiler and nullable diagnostics. |
| Type and path contract safety | PASS | Case-insensitive Outlook root matching is ordinal and culture-independent; the required separator prevents a prefix-only false match. |

## 4. Language-Specific Unit Test Policy Compliance

### C#

| Requirement | Status | Evidence |
|---|---|---|
| MSTest, Moq, and FluentAssertions | PASS | The changed regression tests use `[TestMethod]`, Moq seams, and FluentAssertions. |
| Positive, boundary, and compatibility coverage | PASS | Case variant coverage supplements direct lookup, relative filing-target, ancestor/child activation, `@` root, and non-regression compatibility tests. |
| No prohibited temporary files | PASS | Inspection of changed test code and final evidence shows no temporary-file creation. |

## 5. Test Coverage Detail

`FolderPredictor.cs` is covered at 89.5717% line coverage, above the repository threshold. The case-variant comparison artifact records 100% line and branch coverage for the changed `ProjectSuggestionPath` method. The direct regression suite records 2/2 pass after the production correction; the coverage-enabled full suite records 6,479/6,479 pass.

## 6. Test Execution Metrics

| Metric | Value | Status |
|---|---:|---|
| Full coverage suite | 6,479/6,479 passed | PASS |
| Focused FolderPredictor Issue609 tests | 2/2 passed in reviewer run | PASS |
| Case-variant fail-before | Expected exit 1; observed exit 1 | PASS |
| CSharpier check | 1,520 files checked | PASS |
| Analyzer and nullable rebuilds | exit 0, zero diagnostics | PASS |

## 7. Code Quality Checks

| Check | Command | Result | Status |
|---|---|---|---|
| Formatting | `dotnet tool run csharpier check .` | Reviewer exit 0 | PASS |
| Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | Recorded head evidence: exit 0, 0 diagnostics | PASS |
| Nullable | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | Recorded head evidence: exit 0, 0 diagnostics | PASS |
| Tests | `Invoke-MSTestWithCoverage.ps1` against the case-variant Cobertura output | Recorded head evidence: exit 0, 6,479/6,479 passed | PASS |
| Diff hygiene | `git diff --check b5c751519c6cf0eaeb2326d9e80b2439aeee7265..67db82a928e6b0c023ed16bf42ca48e526f07a0e` | Reviewer exit 0 | PASS |

## 8. Gaps and Exceptions

### Identified Gaps

None. The earlier case-sensitive comparison finding is resolved and directly covered by fail-before/pass-after evidence.

### Approved Exceptions

None.

### Removed/Skipped Tests

None. The case-variant test was added as the direct regression for the prior finding.

## 9. Summary of Changes

The branch adds Issue609 compatibility tests for router lookup and filing boundaries, FolderPredictor startup-projection tests, and one private projection helper. The remediation commit updates the helper to use ordinal ignore-case comparison and adds the case-variant regression. The production diff remains limited to `FolderPredictor.cs`; no policy documents were modified.

## 10. Compliance Verdict

### Overall Status: FULLY COMPLIANT

All applicable general, C#, and test-policy checks are satisfied by inspected source, refreshed PR context, canonical head evidence, and reviewer check-only verification. The branch is suitable for normal PR review flow.

## Appendix A: Test Inventory

- `FolderPredictorTests.Issue609_FolderPredictor_ProjectsOnlyInRootFullSuggestionPaths`
- `FolderPredictorTests.Issue609_FolderPredictor_ProjectsCaseVariantInRootFullSuggestionPath`
- `BreadcrumbBridgeRouterIssue439Tests.Issue609_DirectRowSelection_UsesFullLookupAndRelativeFilingTarget`
- `BreadcrumbBridgeRouterIssue439Tests.Issue609_AncestorActivation_EmitsArchiveRelativeFilingTarget`
- `BreadcrumbBridgeRouterIssue439Tests.Issue609_ImmediateChildActivation_EmitsArchiveRelativeFilingTarget`
- `EmailFilerConfig_Tests.Issue609_ResolvePaths_PrefixesAtMailboxArchiveRootExactlyOnce`

## Appendix B: Toolchain Commands Reference

```powershell
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-25-efc-full-path-destination-resolution-regression-609/evidence/qa-gates/issue-609-case-variant-final.cobertura.xml
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~Issue609_FolderPredictor" /InIsolation
```

**Audit Completed By:** feature-reviewer-c3
**Policy Version:** Current as of 2026-08-25
