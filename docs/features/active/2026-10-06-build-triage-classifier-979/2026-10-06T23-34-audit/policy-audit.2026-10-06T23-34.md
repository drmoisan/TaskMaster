# Policy Compliance Audit: Build Triage Classifier (#979)

Audit date: 2026-10-06
Review type: Final post-remediation feature review
Base branch: `main`
Merge base: `c76e830c18976221b5730f84b8d88aebbfc4f04b`
Reviewed branch: `feature/build-triage-classifier-979`
Reviewed head: `f09f2ae2d44f34cfaa1854d065a5d10c1dbc2bf7`

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---:|---:|---|---:|---:|---:|
| C# | 13 C# files and 3 project files | 5,495 standard-QC tests | PASS | 65.1433% | 65.2006% last merged numeric result; final binary coverage collected | 93.55% to 100% for measured feature methods |
| PowerShell | 7 hook scripts | Canonical PoshQC gates | PASS | 0.00% unmeasured sentinel | 0.00% unmeasured sentinel | 0.00% unmeasured sentinel |

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - no TypeScript source changed`.
- TypeScript post-change coverage artifact: `N/A - no TypeScript source changed`.
- PowerShell baseline coverage artifact: `UNAVAILABLE - no numeric baseline artifact was produced`.
- PowerShell post-change coverage artifact: `UNAVAILABLE - no numeric post-change artifact was produced`.
- Per-language comparison summary: Section 1.2.1.
- C# evidence: `evidence/baseline/p0-t5-vstest-coverage.2026-10-06T20-00.md`, `evidence/qa-gates/p5-t5-vstest-coverage.2026-10-06T20-42.md`, `evidence/qa-gates/p3-t5-utilities-vstest.2026-10-06T23-29.md`, and `evidence/qa-gates/p3-t6-taskmaster-vstest.2026-10-06T23-30.md`.
- Coverage disposition: C# aggregate coverage and unavailable PowerShell numeric coverage are FAIL results waived only for issue #979 by the user's one-time exception. No repository policy was changed to encode the exception.

## Executive Summary

FAIL. The issue #979 implementation and its two functional remediations pass the C# formatter, analyzer, compiler, functional-test, file-size, and acceptance checks. The final C# suites report 5,017 UtilitiesCS tests and 478 TaskMaster tests passing. The extraction restores the previously affected aggregate test files to their merge-base behavior or reduces them, and each new focused file remains below 500 lines.

Two autonomous non-coverage findings block PR readiness. First, the full branch includes unrelated commit `35e748279`, which modifies eight policy files under `.agents/skills/` despite the policy-compliance hard constraint and adds 26 unrelated Codex configuration and hook files. Second, `git diff --check c76e830..f09f2ae` exits 2 with 19 trailing-whitespace diagnostics in four review/remediation Markdown artifacts committed by `f09f2ae`. The user's exception applies to issue #979 coverage requirements only and does not waive either finding.

Policy sources evaluated: `AGENTS.md`, `.agents/skills/policy-compliance-order/SKILL.md`, `.agents/skills/csharp/SKILL.md`, and the required feature-review workflow skills.

## 1. General Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Independence, isolation, and determinism | PASS | Feature tests use fixed in-memory inputs, Moq, injected delegates, and no Outlook, network, or temporary-file dependency. |
| Positive, negative, edge, and state-transition scenarios | PASS | Tests cover A/B/C, null, empty, lowercase, invalid labels, staged loading, persistence, replacement, XML placement, callback dispatch, and absent-engine initialization. |
| Framework and assertion conventions | PASS | New and moved C# tests use MSTest, Moq where applicable, and FluentAssertions. |
| Test organization and file size | PASS | Focused files are 44, 258, and 84 lines; `RibbonExplorerXmlTests.cs` is 496 lines; preexisting oversized aggregates receive no issue #979 additions. |
| Functional test execution | PASS | UtilitiesCS: 5,017/5,017; TaskMaster standard-QC: 478/478. |
| Coverage thresholds | FAIL, WAIVED | The issue #979-only exception covers all coverage requirements. Coverage results remain recorded as failures and do not alter standing policy. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 65.1433% lines -> Post-change: 65.2006% lines. Change: +0.0573 percentage points. New/changed-code coverage: 93.55% to 100%. Disposition: FAIL against repository aggregate requirements, waived for issue #979. Evidence: `evidence/qa-gates/p5-t5-vstest-coverage.2026-10-06T20-42.md`.
- PowerShell: Baseline: 0.00% lines -> Post-change: 0.00% lines. Change: +0.00 percentage points. New/changed-code coverage: 0.00%. Disposition: FAIL, waived for issue #979. Evidence: no numeric artifact exists; these values are unmeasured sentinels rather than measured zero coverage.

## 2. General Code Change Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Objective and feature plan | PASS | Issue #979, specification, user story, research, and atomic plan document the requested behavior. |
| Feature design and behavior | PASS | Triage preservation, exact-label filtering, rebuild initialization, persistence, replacement, and ribbon wiring use established repository patterns. |
| File-size rule for issue changes | PASS | All new files remain below 500 lines; the extraction removes issue-specific additions from oversized aggregates. |
| Policy-file constraint | FAIL | Eight `.agents/skills/*/SKILL.md` files differ from the merge base through unrelated commit `35e748279`; `.agents/skills/policy-compliance-order/SKILL.md:32` prohibits those modifications. |
| Feature scope cohesion | FAIL | Commit `35e748279` contributes 34 unrelated harness files and 1,329 insertions/257 deletions to the feature-vs-base diff. |
| Diff hygiene | FAIL | `git diff --check c76e830c18976221b5730f84b8d88aebbfc4f04b..f09f2ae2d44f34cfaa1854d065a5d10c1dbc2bf7` exits 2 with 19 trailing-whitespace diagnostics across four Markdown artifacts. |
| Dependency and API discipline | PASS | The product feature adds no external dependency and keeps test seams internal. |

## 3. Language-Specific Code Change Policy Compliance

### C#

| Requirement | Status | Evidence |
|---|---|---|
| CSharpier | PASS | Reviewer rerun: `dotnet tool run csharpier check .` checked 1,649 files successfully. |
| .NET analyzers and code style | PASS | Final remediation evidence records zero warnings and zero errors. |
| Compiler and nullable analysis | PASS | Final warnings-as-errors rebuild records zero warnings and zero errors without forcing project-wide nullable enablement. |
| Null-safe Triage model | PASS | Nullable Triage is preserved through constructor, deep copy, JSON, and mining projections. |
| Focused file structure | PASS | The final test extraction resolves the previous file-size finding. |

### PowerShell

| Requirement | Status | Evidence |
|---|---|---|
| Formatting, analysis, and tests | PASS | Canonical PoshQC gates passed at `3a355e14`; `git diff 3a355e14..f09f2ae -- .codex/hooks` is empty, so the reviewed script blobs are unchanged. |
| Script size | PASS | All seven changed scripts are at or below 497 physical lines. |
| Scope and policy ownership | FAIL | The PowerShell harness and its associated policy/configuration changes belong to unrelated commit `35e748279`, not issue #979. |

## 4. Language-Specific Unit Test Policy Compliance

### C#

| Requirement | Status | Evidence |
|---|---|---|
| MSTest | PASS | Feature tests use MSTest attributes. |
| Moq | PASS | External application, file-system, and item-information collaborators use repository-standard mocks. |
| FluentAssertions | PASS | New assertions use FluentAssertions. |
| Full functional suites | PASS | 5,495 tests passed with the documented `LiveOutlook` exclusion. |
| File organization | PASS | The issue-specific test classes now reside in focused files and legacy project files include each exactly once. |

### PowerShell

| Requirement | Status | Evidence |
|---|---|---|
| PoshQC test gate | PASS | Canonical format, analysis, and test operations passed for the unchanged hook blobs. |
| Numeric coverage | FAIL, WAIVED | No numeric coverage artifact exists; the one-time issue #979 exception is nonblocking for coverage only. |

## 5. Test Coverage Detail

The last merged numeric C# result is 65.2006% against a 65.1433% baseline. Measured feature methods range from 93.55% to 100%; ribbon behavior is covered by functional tests but excluded from instrumentation by an existing class-level attribute. The final extraction run produced coverage binaries for both assemblies but did not emit a new numeric percentage. PowerShell numeric coverage is unavailable. All of these coverage requirements are waived for issue #979 only by the user's instruction; the audit retains the underlying FAIL classifications.

## 6. Test Execution Metrics

| Metric | Result | Status |
|---|---:|---|
| UtilitiesCS standard-QC tests | 5,017 passed, 0 failed, 0 skipped | PASS |
| TaskMaster standard-QC tests | 478 passed, 0 failed, 0 skipped | PASS |
| Combined accepted C# tests | 5,495 passed | PASS |
| CSharpier files checked | 1,649 | PASS |
| Analyzer rebuild diagnostics | 0 warnings, 0 errors | PASS |
| Warnings-as-errors rebuild diagnostics | 0 warnings, 0 errors | PASS |
| PowerShell PoshQC gates | format, analyze, and test passed | PASS |
| Diff hygiene diagnostics | 19 | FAIL |

## 7. Code Quality Checks

| Check | Command or mechanism | Result | Status |
|---|---|---|---|
| C# formatting | `dotnet tool run csharpier check .` | 1,649 files clean | PASS |
| C# analyzers | repository analyzer rebuild | zero diagnostics | PASS |
| C# compiler/nullability | repository warnings-as-errors rebuild | zero diagnostics | PASS |
| C# tests | accepted VSTest runs on both assemblies | 5,495 passed | PASS |
| PowerShell toolchain | canonical PoshQC gates plus unchanged-blob proof | success | PASS |
| File-size remediation | physical line counts and merge-base diff | prior blocker resolved | PASS |
| Whitespace | `git diff --check c76e830..f09f2ae` | exit 2, 19 diagnostics | FAIL |
| Policy-file scope | `git diff --name-only c76e830..f09f2ae -- .agents/skills` | 8 changed policy files | FAIL |

## 8. Gaps and Exceptions

| ID | Status | Remediability | Evidence | Required action |
|---|---|---|---|---|
| PA-979-3 | FAIL | autonomous | Commit `35e748279` is wholly confined to 34 `.agents`/`.codex` files, including eight prohibited policy-file changes, and no later commit changes those paths. | Isolate the three issue #979 commits onto a clean `main` base, preserving the inherited original branch state under an explicit backup ref. Do not edit or revert the inherited policy files. |
| PA-979-4 | FAIL | autonomous | `git diff --check c76e830..f09f2ae` reports 19 trailing-whitespace diagnostics in the `23-00` audit set and `23-01` remediation inputs. | Remove the 19 trailing spaces, then rerun the exact committed-range diff check. |

Approved exception: all coverage requirements for issue #979 only, authorized by the user in the active session. The exception does not apply to PA-979-3 or PA-979-4 and does not alter policy files.

## 9. Summary of Changes

The branch preserves nullable Triage data in `MinedMailInfo`, rebuilds the Triage classifier from exact A/B/C mined-mail labels, initializes counts and shared token state, persists and replaces classifier state, and exposes the requested ribbon command. Commit `3a355e14` repairs disabled-engine resolution. Commit `f09f2ae` extracts issue-specific tests into focused files and completes the final non-coverage C# gates. Commit `35e748279` is inherited work that must remain preserved outside the issue #979 PR history.

## 10. Compliance Verdict

### Overall Status: FAIL — REMEDIATION_REQUIRED

The product behavior and acceptance criteria pass, and the prior file-size finding is resolved. The branch is not ready for PR creation until the unrelated harness/policy changes and the 19 whitespace diagnostics are removed. Both findings are autonomous and require a remediation plan.

## Appendix A: Test Inventory

- `MinedMailInfoTests.ConstructorAndDeepCopy_PreserveTriageValue`
- `MinedMailInfoTests.JsonRoundTrip_PreservesNullTriage`
- `EmailDataMinerTriageMapping_Tests.ToMinedMail_TriageValue_PreservesValue`
- `EmailDataMiner_Tests.ToMinedMail_WhenItemsProvided_ProjectsItemFieldsIntoSerializableModels`
- Six `TriageClassifierRebuild_Tests` methods covering state, filtering, persistence, replacement, staged loading, and missing AppData
- `RibbonExplorerXmlTests.RibbonExplorerXml_BuildTriageClassifierIsInSettingsFolderClassifierMenu`
- Three ribbon callback/controller tests covering signature, dispatch, injected rebuild, and absent-engine initialization

## Appendix B: Toolchain Commands Reference

```powershell
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation
vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"
git diff --check c76e830c18976221b5730f84b8d88aebbfc4f04b..f09f2ae2d44f34cfaa1854d065a5d10c1dbc2bf7
git diff --name-only c76e830c18976221b5730f84b8d88aebbfc4f04b..f09f2ae2d44f34cfaa1854d065a5d10c1dbc2bf7 -- .agents/skills
```
