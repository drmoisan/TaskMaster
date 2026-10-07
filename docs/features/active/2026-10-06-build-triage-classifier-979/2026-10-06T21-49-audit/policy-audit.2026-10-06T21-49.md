# Policy Compliance Audit: Build Triage Classifier (#979)

Timestamp: 2026-10-06T21-49
Base branch: `main`
Merge-base: `c76e830c18976221b5730f84b8d88aebbfc4f04b`
Reviewed head: `ca8b98d6a69cfbb38439571c2105cda3994ea8f0`

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---:|---|---|---:|---:|---:|
| C# | 13 | 34 reviewer-focused | PASS | 65.1433% | 65.2006% | Rebuild methods: 93.55% to 100% |

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - out of scope`.
- TypeScript post-change coverage artifact: `N/A - out of scope`.
- PowerShell baseline coverage artifact: `N/A - out of scope`.
- PowerShell post-change coverage artifact: `N/A - out of scope`.
- Per-language comparison summary: `evidence/qa-gates/p5-t5-vstest-coverage.2026-10-06T20-42.md`.
- C# baseline/final artifacts: `evidence/baseline/p0-t5-vstest-coverage.2026-10-06T20-00.md` and `evidence/qa-gates/p5-t5-vstest-coverage.2026-10-06T20-42.md`.

## Executive Summary

PARTIAL — formatting, analyzer, nullable, and focused test requirements passed. The feature contains a blocking functional finding documented in the code review. The user-authorized issue-979 coverage exception waives only the aggregate 80 percent threshold; it does not waive functional acceptance criteria.

## 1. General Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Independence and isolation | PASS | Focused tests use injected delegates and mocks without Outlook, filesystem, network, or temporary files. |
| Deterministic behavior | PARTIAL | Existing tests verify injected callback behavior but omit the disabled-engine execution path. |
| Scenario completeness | PARTIAL | Valid, invalid, and null labels are covered; disabled Triage engine behavior is not. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 65.1433% lines -> Post-change: 65.2006% lines. Change: +0.0573% lines. New/changed-code coverage: Rebuild methods 93.55% to 100%. Disposition: FAIL. Evidence: `evidence/baseline/p0-t5-vstest-coverage.2026-10-06T20-00.md` and `evidence/qa-gates/p5-t5-vstest-coverage.2026-10-06T20-42.md`. The aggregate threshold has an issue-979 exception, but the functional finding remains blocking.

## 2. General Code Change Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Scope and design | PARTIAL | The model and rebuild implementation are scoped, but the ribbon controller does not honor its command contract for a disabled engine. |
| File-size rule | PASS | The Triage partial companion is 97 lines and the existing Triage file is below 500 lines. |
| Diff hygiene | PASS | `git diff --check main...HEAD` exited 0. |

## 3. Language-Specific Code Change Policy Compliance

### C#

| Requirement | Status | Evidence |
|---|---|---|
| CSharpier | PASS | Reviewer check exited 0. |
| .NET analyzers | PASS | Reviewer Rebuild exited 0 with zero diagnostics. |
| Nullable/compiler analysis | PASS | Reviewer Rebuild with warnings as errors exited 0. |

## 4. Language-Specific Unit Test Policy Compliance

### C#

| Requirement | Status | Evidence |
|---|---|---|
| MSTest, Moq, FluentAssertions | PASS | New model and rebuild tests follow the repository convention. |
| Required controller scenario | PARTIAL | The callback injection test passes but does not execute an absent `InboxEngines["Triage"]` path. |
| Temporary-file prohibition | PASS | Changed tests do not create temporary files. |

## 5. Test Coverage Detail

Feature-method coverage meets the 90 percent requirement: `RebuildFromMinedMailAsync` is 93.55 percent and the staged-rebuild, persistence, and replacement methods are 100 percent. Aggregate coverage is 65.2006 percent, below 80 percent but above the 65.1433 percent baseline; the user authorized this issue-979-only exception.

## 6. Test Execution Metrics

| Metric | Value | Status |
|---|---:|---|
| Recorded full coverage suite | 5,491/5,491 passed | PASS |
| Reviewer-focused UtilitiesCS tests | 15/15 passed | PASS |
| Reviewer-focused TaskMaster ribbon tests | 19/19 passed | PASS |
| CSharpier check | 1,646 files checked | PASS |
| Analyzer and nullable rebuilds | zero diagnostics | PASS |

## 7. Code Quality Checks

| Check | Command | Result | Status |
|---|---|---|---|
| Formatting | `dotnet tool run csharpier check .` | exit 0 | PASS |
| Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | exit 0 | PASS |
| Nullable | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | exit 0 | PASS |
| Focused tests | VSTest filtered to issue-979 model/rebuild/ribbon tests | 34/34 passed | PASS |

## Policy Evidence

- `dotnet tool run csharpier check .` completed successfully: 1,646 files checked.
- `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` completed successfully with zero warnings and zero errors.
- `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` completed successfully with zero warnings and zero errors.
- Focused VSTest verification passed: 15 UtilitiesCS tests and 19 TaskMaster ribbon tests.
- `git diff --check main...HEAD` reported no whitespace errors.

## 8. Gaps and Exceptions

`evidence/other/coverage-exception.2026-10-06T21-37.md` records the user's one-time authorization for issue #979 to proceed despite aggregate coverage of 65.2006 percent, below the 80 percent repository threshold. Feature-method coverage remains at or above 90 percent, and the aggregate measure improved from the 65.1433 percent baseline. The exception is correctly limited to the aggregate threshold for this issue.

The absent-engine ribbon path is not excepted and requires remediation.

## Finding

| ID | Status | Evidence | Required action |
| --- | --- | --- | --- |
| PA-979-1 | PARTIAL | The enabled ribbon action can return without invoking the rebuild when Triage is disabled. | Correct the controller path and add deterministic coverage for the disabled-engine case. |

## Review Scope Assumption

The audit reviewed the full branch diff against `main`. The refreshed PR context includes unrelated orchestration tooling from earlier commits; this finding concerns the issue-979 feature files only.

## 9. Summary of Changes

The feature retains `Triage` on mined-mail records, rebuilds classifier state from valid labels, and adds the requested ribbon action. Remediation is required only for the action's disabled-engine behavior.

## 10. Compliance Verdict

### Overall Status: PARTIAL

The aggregate coverage shortfall is authorized for issue #979 only. The functional command gap remains an autonomous blocking finding; the branch is not ready for PR creation until the remediation is executed and reviewed.

## Appendix A: Test Inventory

- `MinedMailInfoTests.ConstructorAndDeepCopy_PreserveTriageValue`
- `MinedMailInfoTests.JsonRoundTrip_PreservesNullTriage`
- `TriageClassifierRebuild_Tests.RebuildFromMinedMailAsync_ValidTriageLabels_RebuildsAllClassifierState`
- `TriageClassifierRebuild_Tests.RebuildFromMinedMailAsync_InvalidTriageLabel_DoesNotMutateAggregateState`
- `RibbonViewerEngineCallbackShapeTests.BuildTriageClassifierAsync_AwaitsInjectedRebuildOperation`
- `RibbonExplorerXmlTests.RibbonExplorerXml_BuildTriageClassifierIsInSettingsFolderClassifierMenu`

## Appendix B: Toolchain Commands Reference

```powershell
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~TriageClassifierRebuild_Tests|FullyQualifiedName~MinedMailInfoTests|FullyQualifiedName~RibbonViewerEngineCallbackShapeTests|FullyQualifiedName~RibbonExplorerXmlTests"
```
