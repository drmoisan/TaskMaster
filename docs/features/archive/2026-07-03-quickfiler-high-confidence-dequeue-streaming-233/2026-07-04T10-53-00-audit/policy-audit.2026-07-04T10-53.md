# Policy Compliance Audit: QuickFiler High-Confidence Dequeue Streaming (#233)

**Audit Date:** 2026-07-04T10-53
**Issue:** #233
**Feature Folder:** `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233`
**Base Branch:** `main`
**Merge Base:** `ec4af1f0924b175a725fe50a5d2a61f7d27a3318`
**Head SHA:** `3752331b5026cc633366739c07c689938d638c72`
**Review Mode:** feature-review-workflow
**PR Context:** `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt`

**Code Under Test:** C# production and test changes under `QuickFiler/`, `QuickFiler.Test/`, C# project files, and issue #233 feature artifacts.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---:|---|---|---|---|---|
| C# | 20 C# / C# project diff entries in PR context | 387 MSTest tests in current review run | PASS execution | Repository-path 22.86% | Repository-path 22.87% | `QfcStreamingDequeueConfidenceGate.cs` 57/60 = 95.00% |

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - no TypeScript files changed`
- TypeScript post-change coverage artifact: `N/A - no TypeScript files changed`
- PowerShell baseline coverage artifact: `N/A - no PowerShell files changed`
- PowerShell post-change coverage artifact: `N/A - no PowerShell files changed`
- C# baseline coverage artifact: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/r4-baseline-coverage-comparison.md`
- C# post-change coverage artifact: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-22-18-coverage-comparison.md`
- Per-language comparison summary: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-22-18-coverage-comparison.md`

## Executive Summary

This audit reviewed the full feature branch for issue #233 against `main` at merge base `ec4af1f0924b175a725fe50a5d2a61f7d27a3318`. The current branch head is `3752331b5026cc633366739c07c689938d638c72`; the refreshed PR context artifacts match that head and were used as the primary evidence source.

The C# command sequence passed for formatting, analyzer build, nullable warnings-as-errors build, and VSTest execution. The current VSTest review run passed 387/387 tests. Coverage remains non-compliant because repository-path C# coverage is 22.87%, below the repository-wide 80% floor required by repository policy and AC10. Focused coverage for the new non-COM-bound gate passes at 95.00%, and the coverage comparison shows no regression from the recorded baseline.

**Policy documents evaluated:**
- PASS: `AGENTS.md`
- PASS: general code change policy in `AGENTS.md`
- PASS: general unit test policy in `AGENTS.md`
- PASS: C# policy in `AGENTS.md` and `.agents/skills/csharp/SKILL.md`
- PASS: feature-review-workflow, feature-review, evidence, acceptance tracking, policy audit template, PR context, and remediation handoff skill contracts

## 1. General Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Independence, isolation, speed, determinism | PARTIAL | Current VSTest run passed 387/387 in 6.2545 seconds. Source-text tests in changed files read production source from disk and are more brittle than behavior tests. |
| New/changed-code coverage | PASS | `remediation-22-18-coverage-comparison.md` records `QfcStreamingDequeueConfidenceGate.cs` at 57/60 = 95.00%. |
| Repository-wide coverage >= 80% | FAIL | `remediation-22-18-coverage-comparison.md` records repository-path coverage at 13120/57379 = 22.87%. |
| External dependency avoidance | PARTIAL | Unit tests use mocks for Outlook boundaries, but changed tests also use source-file reads through `File.ReadAllText` in `QfcDatamodelTests.cs` and `QfcQueuePurePathsTests.cs`. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 22.86% repository-path lines. Post-change: 22.87% repository-path lines. Change: +0.01 percentage points. New/changed-code coverage: 95.00% for `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`. Disposition: FAIL because repository-wide C# coverage remains below 80%. Evidence: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/remediation-22-18-coverage-comparison.md`.

## 2. General Code Change Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Formatting | PASS | `dotnet tool run csharpier -- check .` exited 0; output: `Checked 1235 files in 3797ms.` |
| Linting | PASS | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` exited 0 with 0 warnings and 0 errors. |
| Type checking | PASS | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true` exited 0 with 0 warnings and 0 errors. |
| Testing | PASS execution, FAIL coverage | `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage ...` passed 387/387 tests; coverage evidence remains below the repository-wide floor. |
| Whitespace | PASS | `git diff --check ec4af1f0924b175a725fe50a5d2a61f7d27a3318...HEAD` exited 0. |
| File size policy | PASS | Changed C# and C# test files are below 500 lines; highest observed changed file is `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` at 480 lines. |

## 3. Language-Specific Code Change Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| CSharpier | PASS | Current review command exited 0. |
| .NET analyzers | PASS | Current review analyzer build exited 0 with 0 warnings and 0 errors. |
| Nullable warnings-as-errors | PASS | Current review nullable build exited 0 with 0 warnings and 0 errors. |
| MSTest with coverage | FAIL | Test execution passed, but AC10 coverage threshold remains failed. The current run did not emit a new `.coverage` attachment under `review-2026-07-04T10-53-vstest-results`; the coverage verdict therefore relies on existing Cobertura comparison evidence from the refreshed PR context. |

## 4. Language-Specific Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| MSTest framework | PASS | Current review command executed `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` with VSTest. |
| Moq and FluentAssertions conventions | PASS | Diff inspection shows added issue #233 tests using Moq and FluentAssertions. |
| Behavior-focused tests | PARTIAL | Many added tests cover behavior through seams, but source-text assertions in changed tests verify implementation strings rather than observable behavior. |
| Coverage | FAIL | Repository-path C# coverage remains 22.87%, below the 80% floor. |

## 5. Test Coverage Detail

| Area | Status | Evidence |
|---|---|---|
| `QfcStreamingDequeueConfidenceGate.cs` | PASS | 57/60 = 95.00% in `remediation-22-18-coverage-comparison.md`. |
| Repository-path C# coverage | FAIL | 13120/57379 = 22.87% in `remediation-22-18-coverage-comparison.md`. |
| Coverage regression | PASS | Remediation repository-path coverage is above the R4 baseline by 0.01 percentage points. |
| AC10 | FAIL | AC10 remains unchecked in both authoritative source files. |

## 6. Test Execution Metrics

| Metric | Value | Status |
|---|---:|---|
| Current review tests | 387 | PASS |
| Current review passed | 387 | PASS |
| Current review failed | 0 | PASS |
| Current review execution time | 6.2545 seconds | PASS |
| Repository-path line coverage | 22.87% | FAIL |
| Focused new gate coverage | 95.00% | PASS |

## 7. Code Quality Checks

| Check | Command | Result | Status |
|---|---|---|---|
| Whitespace | `git diff --check ec4af1f0924b175a725fe50a5d2a61f7d27a3318...HEAD` | Exit 0 | PASS |
| CSharpier check | `dotnet tool run csharpier -- check .` | Exit 0 | PASS |
| Analyzer build | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | Exit 0; 0 warnings, 0 errors | PASS |
| Nullable build | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true` | Exit 0; 0 warnings, 0 errors | PASS |
| VSTest with coverage flag | `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /ResultsDirectory:docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\review-2026-07-04T10-53-vstest-results` | 387 passed, 0 failed; no new coverage attachment found in the target directory | PASS execution, PARTIAL coverage artifact |
| Coverage threshold | Existing `dotnet-coverage` comparison evidence | Repository-path coverage 22.87% | FAIL |

## 8. Gaps and Exceptions

### Identified Gaps

1. AC10 remains unmet because repository-path C# coverage is 22.87%, below the 80% repository-wide floor.
2. No approved exception artifact authorizes AC10 check-off.
3. The current VSTest review run passed, but it did not emit a new `.coverage` attachment in the review results directory.
4. Changed unit tests include source-file reads and source-text assertions in `QfcDatamodelTests.cs` and `QfcQueuePurePathsTests.cs`; these should be replaced with behavioral verification or moved to non-unit audit evidence.
5. Live PR and CI status remain unavailable because GitHub CLI is not installed.

### Approved Exceptions

None recorded.

## 9. Summary of Changes

### Commits in This PR/Branch

1. `0008b6b1` - `feat(#233): stream high-confidence dequeue filtering`
2. `f71e4cb6` - `test(#233): isolate EmailMoveMonitor dispatcher cleanup`
3. `46bc5c71` - `fix(quickfiler): gate high-confidence dequeue startup`
4. `58053309` - `docs(qfc): fix remediation whitespace evidence`
5. `2ac150fa` - `test(#233): complete remediation evidence and split tests`
6. `787bb461` - `docs(#233): record post-remediation review artifacts`
7. `25f3d18c` - `docs(quickfiler): record issue 233 remediation review evidence`
8. `3752331b` - `docs(quickfiler): record issue 233 final remediation validation`

### Files Modified

The PR context reports 152 files changed, including 18 `.cs` files, 2 `.csproj` files, 124 `.md` files, and 8 `.xml` files. Material C# production changes include the dequeue-time high-confidence gate, first-page routing through dequeue for high-confidence mode, and queue admission changes. Material test changes include high-confidence streaming, first-page routing, logging, disabled-mode parity, and regression coverage under `QuickFiler.Test/Controllers/`.

## 10. Compliance Verdict

### Overall Status: FAIL

The feature branch passes formatting, analyzer, nullable, whitespace, and test execution checks. It remains policy-non-compliant because repository-path C# coverage is 22.87%, below the repository-wide 80% floor required by AC10 and repository policy. AC10 must remain unchecked unless coverage is raised to the floor or an approved repository exception is recorded.

### Metrics Summary

- PASS: Whitespace comparison against merge base.
- PASS: CSharpier check.
- PASS: Analyzer build: 0 warnings, 0 errors.
- PASS: Nullable build: 0 warnings, 0 errors.
- PASS: VSTest: 387 passed, 0 failed.
- PASS: Focused new gate coverage: 95.00%.
- FAIL: Repository-path C# coverage: 22.87%.
- PARTIAL: Changed tests include source-file reads and source-text assertions.

### Recommendation

Blocked for normal PR readiness. Remediate AC10 coverage disposition and replace source-text unit assertions with behavior tests or non-unit audit evidence before requesting merge readiness.

**Audit Completed By:** Codex
**Audit Date:** 2026-07-04T10-53
**Policy Version:** Current as of audit date

## Appendix A: Test Inventory

- `QfcStreamingDequeueConfidenceGateTests`
- `QfcDatamodelTests`
- `QfcHomeControllerIterationTests`
- `QfcQueuePurePathsTests`
- `QfcHomeControllerRunAsyncTests`
- `QfcHomeControllerRunAsyncHighConfidenceTests`
- `QfcFormControllerSeamTests`
- `QfcCollectionControllerTests`
- `QfcItemController.FolderHandlingTests`
- Current review full QuickFiler VSTest evidence: 387 tests passed.

## Appendix B: Toolchain Commands Reference

```powershell
git diff --check ec4af1f0924b175a725fe50a5d2a61f7d27a3318...HEAD
dotnet tool run csharpier -- check .
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /ResultsDirectory:docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\review-2026-07-04T10-53-vstest-results
```
