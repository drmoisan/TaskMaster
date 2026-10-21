# Policy Compliance Audit: Issue #439 EfcViewer lineage and segment navigation

**Audit Date:** 2026-08-24
**Code Under Test:** C# implementation and test changes in `988e819b..f1b8e504`, as enumerated by `artifacts/pr_context.appendix.txt`.

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---:|---:|---|---:|---:|---:|
| C# | 16 code/test files | 17 audited / 6,474 suite | PASS: 6,474/6,474 suite; 97/97 focused | 85.5756% | 84.7835% | 98.522167% |

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: N/A - no TypeScript files changed.
- TypeScript post-change coverage artifact: N/A - no TypeScript files changed.
- PowerShell baseline coverage artifact: N/A - no PowerShell files changed.
- PowerShell post-change coverage artifact: N/A - no PowerShell files changed.
- Per-language comparison summary: C# baseline 85.5756%, final 84.7835%, changed/new 98.522167%; PASS.
- C# baseline: `evidence/qa-gates/issue-439-baseline.normalized.cobertura.xml`.
- C# post-change: `evidence/qa-gates/issue-439-final.normalized.cobertura.xml`.
- Comparison: `evidence/qa-gates/issue-439-coverage-comparison.md` (PASS; all six numeric comparison files non-regressed, with the documented `BreadcrumbDocumentAssets` structural exception).

## Executive Summary

The branch was reviewed against `main` at merge base `988e819b3bf3d31d6bbe523a2ce6c66189ce718d` through `f1b8e504d9d84f2327c919cb27bdb7b076424a6b`. The authoritative PR-context summary and appendix, committed plan, specification, diff, and final QA evidence were inspected. C# formatter, analyzer, nullable, focused regression, full coverage-suite, and normalized coverage-comparison evidence are PASS. The explicit headless audit of all 17 added or modified Issue #439 tests found only pure router/row/renderer/codec components and Moq seams; no prohibited window/control/handle construction, UI pump, Outlook COM object, filesystem, network, or subprocess invocation was found. `EfcFormController.cs` no longer contains `[ExcludeFromCodeCoverage]`.

The audit is **NON-COMPLIANT** because the new `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` is 531 lines and the modified `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` is 596 lines. Both exceed the repository-wide 500-line limit. The latter was 450 lines at the selected base, so the branch introduced that production-file violation. In addition, the modified `EfcFormController.cs` is only 81/721 = 11.234397% covered, below the mandatory 80% modified-file coverage floor.

Policy documents evaluated: `AGENTS.md` general code-change and unit-test policy, `.agents/skills/csharp/SKILL.md`, and the C# unit-test requirements in `AGENTS.md`.

## 1. General Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Independence, isolation, determinism | PASS | 17 added/modified Issue #439 tests use fixed values, pure components, and strict or standard Moq seams. |
| External-dependency prohibition and headless constraint | PASS | Static invocation audit found no prohibited calls; `issue-439-post-fix-regression.md` records 97 focused tests passing headlessly. Three matches were explanatory comments only. |
| Positive, negative, edge, and error scenarios | PASS | Root expansion, already-rooted values, fallback, malformed messages, ancestor/child activation, collapse, and boundary cases are covered. |
| Coverage | FAIL | Final normalized comparison is 200/203 changed/new instrumentable lines and final repository coverage is 84.7835%, but modified `EfcFormController.cs` is 81/721 = 11.234397%, below 80%. |
| Readability and maintainability | FAIL | The new Issue #439 router test file is 531 lines, exceeding the 500-line test-file limit. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.5756%; Post-change: 84.7835%; Change: -0.7921 percentage points; New/changed-code coverage: 98.522167%. Disposition: FAIL because modified `EfcFormController.cs` is 11.234397%, below the 80% modified-file floor. Evidence: `evidence/qa-gates/issue-439-coverage-comparison.md`.

## 2. General Code Change Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Objective, plan, and supporting documents | PASS | `spec.md`, `plan.2026-08-24T17-30.md`, evidence artifacts, and commit evidence are present. |
| Separation of concerns and explicit boundaries | PASS | `BreadcrumbBridgeRouter` remains the non-UI bridge; host/provider dependencies remain interfaces; row/codec/renderer responsibilities are separate. |
| Error handling and logging | PASS | Provider cancellation and exceptions fall back to a selectable one-segment row; invalid inbound actions are rejected and logged. |
| Cohesive files under 500 lines | FAIL | New Issue #439 tests: 531 lines. Modified router: 596 lines, increased from 450 in the selected base. |
| Toolchain loop | PASS | Final documented loop passed format, analyzers, nullable analysis, focused regression, full coverage-suite, and normalized comparison. |

## 3. Language-Specific Code Change Policy Compliance

### C#

| Requirement | Status | Evidence |
|---|---|---|
| CSharpier formatting | PASS | `dotnet tool run csharpier format .`, exit 0, no final change; `evidence/qa-gates/csharpier-final.md`. |
| .NET analyzers | PASS | `msbuild ... EnableNETAnalyzers=true ...`, exit 0, zero analyzer findings. |
| Nullable/compiler analysis | PASS | `msbuild ... Nullable=enable ... TreatWarningsAsErrors=true`, exit 0, zero compiler and nullable findings. |
| Focused and coverage-enabled tests | PASS | 97 focused tests and 6,474 coverage-suite tests passed; normalized metrics meet thresholds. |
| File-size rule | FAIL | `BreadcrumbBridgeRouter.cs` is 596 lines after a 146-line increase from base. |

## 4. Language-Specific Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| MSTest framework | PASS | All changed tests use `[TestClass]` and `[TestMethod]`. |
| Moq and FluentAssertions | PASS | Router and Efc binding tests use Moq seams; assertions use FluentAssertions. |
| No WinForms/WebView2/Outlook/real GUI construction | PASS | Static call audit and execution evidence confirm no prohibited headless-boundary action. |
| Test file size | FAIL | `BreadcrumbBridgeRouterIssue439Tests.cs` has 531 physical lines. |

## 5. Test Coverage Detail

The normalized comparison derives metrics from the baseline and final normalized Cobertura XML only. Repository line coverage is 84.7835%; changed/new remaining instrumentable production lines are 200/203 = 98.522167%. `BreadcrumbBridgeRouter`, `BreadcrumbRow`, `BreadcrumbRowBuilder`, `BreadcrumbMessages`, `BreadcrumbMessageCodec`, and `BreadcrumbHtmlRenderer` are non-regressed. `EfcFormController` is no longer excluded and is numeric at 81/721 = 11.234397%; this fails the feature-review workflow's mandatory 80% floor for modified files. `BreadcrumbDocumentAssets` is the sole documented no-sequence-point exception.

## 6. Test Execution Metrics

| Metric | Value | Status |
|---|---:|---|
| Focused regression | 97 passed, 0 failed | PASS |
| Coverage suite | 6,474 passed, 0 failed | PASS |
| Changed/new tests specifically audited | 17 | PASS, headless |
| New Issue #439 test-file size | 531 physical lines | FAIL |

## 7. Code Quality Checks

| Check | Command or evidence | Result | Status |
|---|---|---|---|
| Diff whitespace | `git diff --check 988e819b..HEAD` | No output | PASS |
| Formatting | `dotnet tool run csharpier format .` | Exit 0; final pass unchanged | PASS |
| Analyzers | documented final MSBuild command | 0 findings | PASS |
| Nullable | documented final MSBuild command | 0 findings | PASS |
| Tests and coverage | final normalized coverage wrapper | 6,474/6,474; 84.7835% repository, but EfcForm 11.234397% | FAIL |

## 8. Gaps and Exceptions

### Identified Gaps

1. `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` is a newly added 531-line test file, exceeding the repository limit by 31 lines.
2. `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` increased from 450 to 596 lines and now exceeds the repository limit by 96 lines.
3. Modified `QuickFiler/Controllers/EfcFormController.cs` is 81/721 = 11.234397% covered, below the 80% per-modified-file coverage floor.

### Approved Exceptions

None. `BreadcrumbDocumentAssets` has the existing documented structural no-sequence-point coverage exception; it is not a file-size-policy exception.

### Removed/Skipped Tests

None. No review-time test was removed or skipped.

## 9. Summary of Changes

The range contains the Issue #439 feature commit `c39db103`, evidence commit `0b28c7e5`, and completion documentation commit `f1b8e504`, plus the feature specification and planning commits. Implementation preserves original filing targets while resolving archive-rooted hierarchy paths, adds typed segment/child activation, renders Unicode lineage separators, and expands children using a provider-bound stable key. The controller coverage-exclusion attribute was removed.

## 10. Compliance Verdict

### Overall Status: NON-COMPLIANT

Functional behavior and headless-test rules are PASS. The file-size failures and EfcForm per-file coverage failure require remediation before PR readiness can be approved. No policy document was modified.

## Appendix A: Test Inventory

- `BreadcrumbBridgeRouterIssue439Tests`: 7 Issue #439 router scenarios.
- `BreadcrumbBridgeRouterQueueTests`: 2 modified fallback/key-capture scenarios.
- `EfcFormControllerTests`: 1 archive-root binding-boundary scenario.
- `BreadcrumbHtmlRendererTests`: 2 renderer/document-bridge scenarios.
- `BreadcrumbMessageCodecTests`: 2 typed-message validation scenarios.
- `BreadcrumbRowBuilderTests`: 1 filing-target/score scenario.
- `BreadcrumbRowStateTests`: 2 active-segment/state-preservation scenarios.

## Appendix B: Toolchain Commands Reference

```powershell
git diff --check 988e819b..HEAD
dotnet tool run csharpier format .
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true
pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/evidence/qa-gates/issue-439-final.normalized.cobertura.xml
```

**Audit Completed By:** Codex feature reviewer
**Audit Date:** 2026-08-24
**Policy Version:** Current
