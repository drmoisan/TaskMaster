# Policy Compliance Audit: Build Triage Classifier (#979)

Audit date: 2026-10-07
Review type: Remediation pass 3 full-feature re-review
Base branch: `origin/main` at `5ddf7f03d6b92b2981cd0d5d74f10a0733e80964`
Merge base: `5ddf7f03d6b92b2981cd0d5d74f10a0733e80964`
Reviewed branch: `feature/build-triage-classifier-979`
Reviewed head: `95f9bab63319ff15ba0e5008c484879b392317be`

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---:|---:|---|---:|---:|---:|
| C# | 13 C# files and 3 project files | 5,495 standard-QC tests | PASS | 65.1433% | 65.2006% | 93.55% to 100% for measured feature methods |

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - no TypeScript source changed`.
- TypeScript post-change coverage artifact: `N/A - no TypeScript source changed`.
- PowerShell baseline coverage artifact: `N/A - no PowerShell source changed`.
- PowerShell post-change coverage artifact: `N/A - no PowerShell source changed`.
- C# baseline coverage: `evidence/baseline/p0-t5-vstest-coverage.2026-10-06T20-00.md`.
- C# post-change coverage: `evidence/qa-gates/p5-t5-vstest-coverage.2026-10-06T20-42.md`.
- C# final functional QA: `evidence/qa-gates/p3-t5-utilities-vstest.2026-10-06T23-29.md` and `evidence/qa-gates/p3-t6-taskmaster-vstest.2026-10-06T23-30.md`.
- Per-language comparison summary: Section 1.2.1.
- Coverage authorization: `evidence/other/coverage-exception.2026-10-06T21-37.md` plus the controlling review instruction applying the one-time exception to every issue #979 coverage requirement.
- No other programming language is in the isolated committed feature range.

## Executive Summary

PASS. The isolated issue #979 branch satisfies the applicable general and C# policies for all noncoverage requirements. The complete `origin/main..HEAD` range contains the intended C# feature, tests, project inclusions, ribbon XML, and feature evidence. It contains no `.agents` or `.codex` path. `git diff --check` passes for both the complete committed range and the final remediation commit. This resolves PA-979-3/CR-979-3 and PA-979-4/CR-979-4.

The final accepted C# evidence records 5,017 UtilitiesCS tests and 478 TaskMaster tests passing, zero analyzer diagnostics, and zero compiler/nullability diagnostics. The reviewer also reran `dotnet tool run csharpier check .` at the reviewed head; 1,650 files were clean. Aggregate and changed-code coverage values remain documented, and every issue #979 coverage requirement is nonblocking under the user's one-time exception. All noncoverage gates pass.

Policy sources evaluated: `AGENTS.md`, `.github/copilot-instructions.md`, the general code-change and unit-test instructions, the C# code-change and unit-test instructions, `.agents/skills/policy-compliance-order/SKILL.md`, `.agents/skills/csharp/SKILL.md`, and the feature-review workflow skills.

## 1. General Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Independence, isolation, and determinism | PASS | Feature tests use fixed in-memory records, Moq, and injected delegates. They do not require Outlook, a network service, or temporary files. |
| Positive flows | PASS | A, B, and C labels rebuild classifier counts, shared token state, persistence, and active replacement. |
| Negative and edge flows | PASS | Null collections, missing AppData, empty data, null/empty/lowercase/out-of-contract labels, and invalid-only input are covered. |
| State transitions | PASS | Tests verify the pre-publication initialization, exactly-once persistence, exactly-once manager replacement, callback dispatch, and disabled-engine lazy resolution. |
| Framework and assertions | PASS | New and moved tests use MSTest and FluentAssertions; Moq is used for external collaborators where applicable. |
| Test organization | PASS | Feature tests are in focused files. New focused files are 44, 84, and 258 lines; the split ribbon aggregate is 496 lines. |
| Functional execution | PASS | UtilitiesCS: 5,017 passed; TaskMaster: 478 passed with the repository-standard `TestCategory!=LiveOutlook` filter. |
| Coverage requirements | AUTHORIZED EXCEPTION | Numeric results are retained below. The user's one-time issue #979 authorization makes all coverage requirements nonblocking without changing repository policy. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 65.1433% lines -> Post-change: 65.2006% lines. Change: +0.0573 percentage points. New/changed-code coverage: 93.55% to 100%. Disposition: FAIL against repository aggregate requirements, waived for issue #979 under the controlling authorization. Evidence: `evidence/qa-gates/p5-t5-vstest-coverage.2026-10-06T20-42.md` and `evidence/other/coverage-exception.2026-10-06T21-37.md`.

## 2. General Code Change Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Objective and plan | PASS | `issue.md`, `spec.md`, `user-story.md`, the original implementation plan, and the Cycle 3 remediation plan define and track the work. |
| Simplicity and reuse | PASS | The rebuild reuses `ClassifierFactory`, `GenerateClassifierBase`, `ToAsyncLazy`, the existing Bayesian staging path, manager configuration, and serialization patterns. |
| Separation of concerns | PASS | Model preservation, rebuild behavior, ribbon XML, viewer callback, and controller dispatch remain in their existing layers. |
| Cohesive modules | PASS | The new rebuild partial contains only mined-mail rebuild logic; focused tests isolate mapping, rebuild, and ribbon behavior. |
| File-size limit | PASS | All issue #979 added or expanded files satisfy the 500-line limit; the prior aggregate-test concern was resolved by extraction. |
| API and dependency discipline | PASS | No external dependency or breaking public API was added. Test delegates are internal. |
| Policy-file ownership | PASS | `git diff --name-only 5ddf7f03..95f9bab6 -- .agents .codex` returns no paths. PA-979-3 is resolved. |
| Diff hygiene | PASS | `git diff --check 5ddf7f03..95f9bab6` and `git diff-tree --check 95f9bab6^ 95f9bab6` both exit 0. PA-979-4 is resolved. |
| Supporting documentation | PASS | Feature requirements, implementation plan, remediation plan, QA evidence, and acceptance summary are present under the issue #979 feature folder. |

## 3. Language-Specific Code Change Policy Compliance

### C#

| Requirement | Status | Evidence |
|---|---|---|
| CSharpier | PASS | Reviewer rerun: `dotnet tool run csharpier check .` checked 1,650 files with exit 0. Prior final-QA evidence also passed. |
| .NET analyzers and code style | PASS | `evidence/qa-gates/p3-t3-analyzers-retry.2026-10-06T23-28.md` records zero warnings and zero errors. |
| Compiler and nullability | PASS | `evidence/qa-gates/p3-t4-nullable-retry.2026-10-06T23-29.md` records zero warnings and zero errors. |
| Null-safe contracts | PASS | `MinedMailInfo.Triage` is nullable, and the rebuild explicitly handles unavailable paths, null collections, and invalid-only collections. |
| Focused types and methods | PASS | `Triage.MinedMailRebuild.cs` is a focused partial, while existing domain and manager abstractions retain their responsibilities. |
| Resource and lifecycle safety | PASS | The asynchronous controller/viewer flow is awaited at the application boundary; classifier state is published only after successful reconstruction and persistence. |

## 4. Language-Specific Unit Test Policy Compliance

### C#

| Requirement | Status | Evidence |
|---|---|---|
| MSTest | PASS | Feature test classes and methods use MSTest attributes. |
| Moq | PASS | Item, application, configuration, file-system, and manager collaborators use the repository's established mocking pattern where needed. |
| FluentAssertions | PASS | New and moved assertions use FluentAssertions. |
| Scenario completeness | PASS | Tests cover label preservation, exact filtering, invalid-only no-op behavior, rebuild state, persistence, manager replacement, staged loading, missing AppData, XML location, callback shape, dispatch, and disabled-engine resolution. |
| No prohibited temporary files | PASS | The feature tests use in-memory values and mocks; no runtime temporary file is created. |
| Full suites | PASS | 5,495 accepted C# tests passed with no failures or skips. |

## 5. Test Coverage Detail

The numeric baseline is 65.1433% and the post-change result is 65.2006%, an increase of 0.0573 percentage points. Measured issue #979 methods range from 93.55% to 100%. Ribbon behavior is verified by unit tests and is excluded from instrumentation by an existing class-level attribute. The user's one-time authorization applies to all issue #979 coverage requirements, including aggregate, new-code, and changed-line expectations. The exception is confined to issue #979 and does not waive any formatter, analyzer, compiler, functional-test, scope, file-size, acceptance, or diff-hygiene requirement.

## 6. Test Execution Metrics

| Metric | Result | Status |
|---|---:|---|
| UtilitiesCS standard-QC tests | 5,017 passed, 0 failed, 0 skipped | PASS |
| TaskMaster standard-QC tests | 478 passed, 0 failed, 0 skipped | PASS |
| Combined accepted C# tests | 5,495 passed | PASS |
| Reviewer CSharpier check | 1,650 files clean | PASS |
| Analyzer rebuild diagnostics | 0 warnings, 0 errors | PASS |
| Compiler/nullability diagnostics | 0 warnings, 0 errors | PASS |
| Complete-range diff diagnostics | 0 | PASS |
| `.agents`/`.codex` paths in range | 0 | PASS |
| Authoritative acceptance criteria | 12/12 supported | PASS |
| `issue.md` cross-checks | 5/5 supported | PASS |

## 7. Code Quality Checks

| Check | Command or mechanism | Result | Status |
|---|---|---|---|
| C# formatting | `dotnet tool run csharpier check .` | 1,650 files clean | PASS |
| C# analyzers | Repository analyzer rebuild evidence | Zero warnings and errors | PASS |
| C# compiler/nullability | Repository warnings-as-errors rebuild evidence | Zero warnings and errors | PASS |
| UtilitiesCS tests | VSTest final-QA evidence | 5,017 passed | PASS |
| TaskMaster tests | VSTest final-QA evidence | 478 passed | PASS |
| Patch identity | `git range-diff --no-color 35e748279..f09f2ae2 5ddf7f03..562b8bb1` | Three exact `=` mappings; no changed, added, or removed patch | PASS |
| Scope isolation | `git diff --name-only 5ddf7f03..95f9bab6 -- .agents .codex` | No output | PASS |
| Whitespace | `git diff --check 5ddf7f03..95f9bab6` | Exit 0 | PASS |
| Remediation commit whitespace | `git diff-tree --check 95f9bab6^ 95f9bab6` | Exit 0 | PASS |
| Working typed-file isolation | `git diff --name-only HEAD -- '*.cs' '*.csproj'` | No output | PASS |

## 8. Gaps and Exceptions

### Resolved prior findings

| ID | Status | Resolution evidence |
|---|---|---|
| PA-979-3 / CR-979-3 | RESOLVED | The merge base is current `origin/main`; the three issue patches retain exact range-diff identity; the complete feature range contains zero `.agents` or `.codex` paths; the former branch is preserved at `refs/heads/backup/issue-979-pre-isolation-f09f2ae2`. |
| PA-979-4 / CR-979-4 | RESOLVED | The documented 19 trailing spaces were removed as whitespace-only corrections. Both the complete range and remediation commit pass Git's whitespace checks. |

### Approved exception

- All coverage requirements for issue #979 only. Authorization is recorded in `evidence/other/coverage-exception.2026-10-06T21-37.md` and broadened by the controlling review instruction to every issue #979 coverage requirement. Numeric results remain visible; the exception does not modify policy or apply to any noncoverage gate.

### Remaining gaps

None. No noncoverage blocker, Major finding, meaningful PARTIAL result, or unverified required gate remains.

## 9. Summary of Changes

The feature preserves nullable Triage values through mined-mail construction, copy, serialization, and projection; rebuilds the Triage classifier from exact A/B/C labels; initializes class totals and shared token state; persists configuration; replaces the active classifier through the existing manager; and exposes the rebuild through `TaskMaster -> Settings -> Folder Classifier -> Build Triage Classifier`. Tests cover model, mapping, rebuild, persistence, replacement, XML, callback, and disabled-engine behavior.

The Cycle 3 remediation rebased the three product patches onto current `origin/main`, excluded unrelated policy and harness history, preserved the former state through explicit refs, corrected historical trailing whitespace without changing artifact meaning, and committed the evidence at `95f9bab63319ff15ba0e5008c484879b392317be`.

## 10. Compliance Verdict

### Overall Status: PASS — FULLY COMPLIANT FOR ALL NONCOVERAGE REQUIREMENTS

The complete feature range passes the policy audit. All coverage requirements are authorized exceptions for issue #979 only. PA-979-3/CR-979-3 and PA-979-4/CR-979-4 are resolved, and no remediation handoff is required.

## Appendix A: Test Inventory

- `MinedMailInfoTests.ConstructorAndDeepCopy_PreserveTriageValue`
- `MinedMailInfoTests.JsonRoundTrip_PreservesNullTriage`
- `EmailDataMinerTriageMapping_Tests.ToMinedMail_TriageValue_PreservesValue`
- `EmailDataMiner_Tests.ToMinedMail_WhenItemsProvided_ProjectsItemFieldsIntoSerializableModels`
- Six `TriageClassifierRebuild_Tests` methods covering valid state, invalid-label exclusion, persistence/replacement, invalid-only no-op behavior, staged loading, and missing AppData
- `RibbonExplorerXmlTests.RibbonExplorerXml_BuildTriageClassifierIsInSettingsFolderClassifierMenu`
- Three ribbon callback/controller tests covering callback shape, awaited dispatch, injected rebuild, and absent-engine initialization

## Appendix B: Toolchain Commands Reference

```powershell
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation
vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"
git diff --check 5ddf7f03d6b92b2981cd0d5d74f10a0733e80964..95f9bab63319ff15ba0e5008c484879b392317be
git diff-tree --check 95f9bab63319ff15ba0e5008c484879b392317be^ 95f9bab63319ff15ba0e5008c484879b392317be
git diff --name-only 5ddf7f03d6b92b2981cd0d5d74f10a0733e80964..95f9bab63319ff15ba0e5008c484879b392317be -- .agents .codex
git range-diff --no-color 35e7482798dd0b7003afb8f7a75263c807f8da37..f09f2ae2d44f34cfaa1854d065a5d10c1dbc2bf7 5ddf7f03d6b92b2981cd0d5d74f10a0733e80964..562b8bb1cf0c0b67846640c7f7aa409a07277ce9
```

Audit completed by: Codex feature reviewer
Policy version: Current as of 2026-10-07
