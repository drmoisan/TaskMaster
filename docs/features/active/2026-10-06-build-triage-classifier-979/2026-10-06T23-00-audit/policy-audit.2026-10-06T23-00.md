# Policy Compliance Audit: Build Triage Classifier (#979)

Timestamp: 2026-10-06T23-00  
Review type: Post-remediation re-review  
Base branch: `main`  
Merge base: `c76e830c18976221b5730f84b8d88aebbfc4f04b`  
Reviewed branch: `feature/build-triage-classifier-979`  
Reviewed head: `3a355e14a57109f5470fcf3b7d747351bade5804`

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---:|---:|---|---:|---:|---:|
| C# | 11 C# files and 1 project file | 5,491 standard-QC tests | PASS | 52.7659% remediation baseline | 65.1604% | Feature methods 93.55% to 100%; ribbon behavior tested but excluded from instrumentation |
| PowerShell | 7 hook scripts | Canonical PoshQC test gate | PASS | 0.00% unmeasured sentinel | 0.00% unmeasured sentinel | 0.00% unmeasured sentinel |

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - no TypeScript source changed`.
- TypeScript post-change coverage artifact: `N/A - no TypeScript source changed`.
- PowerShell baseline coverage artifact: `UNAVAILABLE - no numeric baseline artifact was produced`.
- PowerShell post-change coverage artifact: `UNAVAILABLE - no numeric post-change artifact was produced`.
- Per-language comparison summary: C# coverage is documented in `evidence/qa-gates/p2-t5-vstest-coverage.2026-10-06T22-51.md`; PowerShell numeric coverage is unavailable and is recorded as a waived coverage FAIL.
- C# final coverage and test evidence: `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/qa-gates/p2-t5-vstest-coverage.2026-10-06T22-51.md`.
- C# final comparison: `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/qa-gates/p2-t6-remediation-summary.2026-10-06T22-51.md`.
- Issue-specific coverage authorization: `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/other/coverage-exception.2026-10-06T21-37.md`, supplemented by the user's exact session instruction, "I give a one time exception to the coverage requirements."
- PowerShell numeric coverage artifact: absent. This is recorded as a coverage FAIL and is nonblocking only because the same issue-979 exception covers coverage requirements.
- TypeScript and Python coverage artifacts: not applicable because no TypeScript or Python source changed.

## Executive Summary

FAIL. The remediation commit fixes the previously reported disabled-engine command path. The feature behavior, C# formatter/analyzer/compiler checks, standard-QC tests, and PowerShell PoshQC checks pass. The full committed branch diff still violates the repository's 500-line file rule because issue-specific tests were added to three test files at or above the limit. This non-coverage policy finding is autonomous and is not covered by the user's one-time coverage exception.

The review covers all 120 files in the committed branch range, including the seven PowerShell hook files introduced by commit `35e7482798dd0b7003afb8f7a75263c807f8da37`. Those files are outside the issue's product behavior but are part of the branch diff and therefore remain material to this review.

## 1. General Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Independence, isolation, and determinism | PASS | Issue tests use in-memory data, injected delegates, Moq, and MSTest; accepted runs passed without Outlook, network, or temporary-file dependencies. |
| Positive, negative, edge, and state-transition scenarios | PASS | Tests cover A/B/C labels, null and invalid labels, no-valid-data behavior, persistence, manager replacement, ribbon XML, callback dispatch, and absent-engine lazy initialization. |
| Temporary-file prohibition | PASS | No changed issue-979 unit test creates temporary files. |
| Framework and assertion conventions | PASS | Changed C# tests use MSTest, Moq where applicable, and FluentAssertions. |
| Test-file size | FAIL | Three changed test files violate or worsen the 500-line limit; detailed evidence appears in section 2. |

### 1.2.1 Per-Language Coverage Comparison

- **C#: FAIL, waived for issue #979.** The accepted isolated-and-merged result is 65.1604% (`128,768 / 197,617`), below the repository-wide 80% threshold. Feature production coverage is 100% for `RebuildFromStagedMinedMailAsync`, `PersistClassifierGroupAsync`, `ReplaceClassifierGroup`, and `MinedMailInfo`, and 93.55% for `RebuildFromMinedMailAsync`. `RibbonController` has an existing class-level coverage exclusion, while both command paths passed behavioral tests.
- **PowerShell: FAIL, waived for issue #979.** PoshQC tests passed, but the review found no numeric PowerShell coverage artifact for the seven changed hook scripts.
- C#: Baseline: 52.7659% lines -> Post-change: 65.1604% lines. Change: +12.3945 percentage points. New/changed-code coverage: 93.55% to 100% for instrumented feature methods. Disposition: FAIL against the aggregate threshold, waived for issue #979. Evidence: `evidence/qa-gates/p2-t5-vstest-coverage.2026-10-06T22-51.md`.
- PowerShell: Baseline: 0.00% lines -> Post-change: 0.00% lines. Change: +0.00 percentage points. New/changed-code coverage: 0.00%. Disposition: FAIL, waived for issue #979. Evidence: canonical PoshQC test output confirms execution but no numeric coverage artifact exists. These required numeric fields are unmeasured sentinels; they are not evidence that the scripts executed with zero coverage.

The coverage failures are reported rather than converted to PASS. They do not block issue #979 because the user granted a one-time exception to coverage requirements. No non-coverage requirement is waived.

## 2. General Code Change Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Scope and behavior | PASS | The model, rebuild workflow, persistence/replacement, ribbon wiring, and disabled-engine lifecycle are cohesive and match issue #979. |
| Full committed branch reviewed | PASS | PR context records 120 files, 3,007 insertions, and 259 deletions across `c76e830...3a355e14`; the PowerShell harness commit was retained in scope. |
| File-size rule | FAIL | `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` is 531 lines after +35/-0 from a 496-line baseline; `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/ClassifierGroups_Tests.cs` is 1,976 lines after +244/-0 from a 1,732-line baseline; `UtilitiesCS.Test/EmailIntelligence/EmailDataMiner_Tests.cs` is 611 lines after +2/-0 from a 609-line baseline. |
| Diff hygiene | PASS | `git diff --check c76e830c18976221b5730f84b8d88aebbfc4f04b..3a355e14a57109f5470fcf3b7d747351bade5804` exited 0. |
| Public API and dependency discipline | PASS | The implementation adds no external dependency and retains existing classifier and ribbon lifecycles. |

### File-size baseline interpretation

The remediation must remain limited to the issue-specific additions. `RibbonExplorerXmlTests.cs` was 496 lines at the merge base, so its new 35-line test must move to a focused file and the original file must return to 500 lines or fewer. The other two files already exceeded 500 lines at the merge base. The branch must remove its issue-specific additions from those files and place the behavior in focused files under 500 lines; it is not required to restructure unrelated baseline tests. This returns the preexisting oversized files to their base content and prevents issue #979 from widening an existing violation.

## 3. Language-Specific Code Change Policy Compliance

### C#

| Requirement | Status | Evidence |
|---|---|---|
| CSharpier | PASS | `dotnet tool run csharpier check .` checked 1,646 files successfully. |
| .NET analyzers and code style | PASS | Full rebuild completed with zero warnings and zero errors. |
| Compiler and nullable analysis | PASS | Full warnings-as-errors rebuild completed with zero warnings and zero errors. |
| Null-safe Triage model | PASS | `MinedMailInfo.Triage` is nullable and construction, deep-copy, JSON, and mining paths preserve the value. |
| Focused file structure | FAIL | Feature tests were appended to oversized or near-limit aggregate test files instead of focused files. |

### PowerShell

| Requirement | Status | Evidence |
|---|---|---|
| Formatting | PASS | `mcp__drm_copilot__run_poshqc_format` completed successfully against exact head `3a355e14` in a detached review worktree. |
| Static analysis | PASS | `mcp__drm_copilot__run_poshqc_analyze` completed successfully for `.codex/hooks`. |
| Tests | PASS | `mcp__drm_copilot__run_poshqc_test` completed successfully for `.codex/hooks`. |
| Script size | PASS | All seven changed scripts are at or below 497 physical lines. |

## 4. Language-Specific Unit Test Policy Compliance

### C#

| Requirement | Status | Evidence |
|---|---|---|
| MSTest | PASS | New and modified tests use `[TestClass]`, `[TestMethod]`, and `[DataTestMethod]`. |
| Moq | PASS | External collaborators and application globals are represented with repository-standard mocks. |
| FluentAssertions | PASS | New assertions use FluentAssertions. |
| Functional regression coverage | PASS | The absent-engine regression and original injected-rebuild bypass both pass in the accepted TaskMaster run. |
| File organization | FAIL | The three changed oversized/near-limit files require issue-scoped extraction. |

### PowerShell

| Requirement | Status | Evidence |
|---|---|---|
| PoshQC test gate | PASS | The canonical PoshQC test tool returned success for the complete changed hook scope. |
| Numeric coverage | FAIL, WAIVED | No numeric coverage artifact exists; issue #979's one-time coverage exception makes this nonblocking. |

## 5. Test Coverage Detail

The accepted C# test run merged separate UtilitiesCS and TaskMaster coverage files after diagnostic evidence identified combined-host ordering interference. The aggregate result is 65.1604%. Feature methods measured between 93.55% and 100%. The ribbon controller remains uninstrumented because of an existing class-level exclusion, but both the injected bypass and absent-engine lazy-resolution paths have passing behavioral tests. PowerShell coverage was not measured. These facts remain recorded as coverage failures under the exact issue-979-only exception.

## 6. Test Execution Metrics

| Metric | Result | Status |
|---|---:|---|
| UtilitiesCS standard-QC tests | 5,013 passed, 0 failed | PASS |
| TaskMaster standard-QC tests | 478 passed, 0 failed | PASS |
| Combined accepted C# tests | 5,491 passed, 0 failed, 0 skipped | PASS |
| CSharpier files checked | 1,646 | PASS |
| Analyzer rebuild diagnostics | 0 warnings, 0 errors | PASS |
| Warnings-as-errors rebuild diagnostics | 0 warnings, 0 errors | PASS |
| PowerShell PoshQC gates | format, analyze, and test passed | PASS |

`LiveOutlook` tests were excluded by the repository's standard-QC contract because they require an interactive Outlook profile. The accepted evidence identity-checks both test assemblies by path, SHA-256, MVID, and build time.

## 7. Code Quality Checks

| Check | Command or mechanism | Result | Status |
|---|---|---|---|
| C# formatting | `dotnet tool run csharpier check .` | 1,646 files clean | PASS |
| C# analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | zero diagnostics | PASS |
| C# compiler/nullability | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | zero diagnostics | PASS |
| C# tests | VSTest on UtilitiesCS.Test and TaskMaster.Test with `TestCategory!=LiveOutlook` for TaskMaster | 5,491 passed | PASS |
| PowerShell formatting | canonical PoshQC format MCP tool | success | PASS |
| PowerShell analysis | canonical PoshQC analyze MCP tool | success | PASS |
| PowerShell tests | canonical PoshQC test MCP tool | success | PASS |
| Whitespace | `git diff --check c76e830...3a355e14` | no errors | PASS |

## 8. Gaps and Exceptions

| ID | Status | Remediability | Evidence | Required action |
|---|---|---|---|---|
| PA-979-2 | FAIL | autonomous | Issue-specific tests add 35, 244, and 2 lines to three files at or above the 500-line limit. | Extract the issue-specific additions into focused test files under 500 lines, restore the preexisting aggregate files to their base content where applicable, update the legacy project compile list, and rerun all non-coverage gates. |

The user's exception applies only to coverage requirements for issue #979. It does not cover PA-979-2. No policy document was modified, and no exception is inferred for file structure or functional behavior.

## 9. Summary of Changes

The branch adds nullable Triage retention to mined mail, rebuilds the Triage classifier from valid A/B/C records, persists and installs the rebuilt group, exposes the requested ribbon command, and resolves the disabled-engine path through the existing lazy Triage lifecycle. It also includes seven PowerShell orchestration hook changes from an earlier branch commit. Functional and toolchain evidence passes, while issue-specific test placement requires remediation.

## 10. Compliance Verdict

### Overall Status: FAIL — REMEDIATION_REQUIRED

The branch is not ready for PR creation. PA-979-2 is an autonomous, non-coverage policy violation. Remediation is limited to extracting issue-specific tests into focused files and rerunning the required non-coverage gates; broad restructuring of unrelated baseline tests is outside this remediation scope.

## Appendix A: Test Inventory

- `MinedMailInfoTests.ConstructorAndDeepCopy_PreserveTriageValue`
- `MinedMailInfoTests.JsonRoundTrip_PreservesNullTriage`
- `MinedMailInfo_Tests.Constructor_WithItemInfo_MapsAllSupportedProperties`
- `EmailDataMiner_Tests.ToMinedMail_WhenItemsProvided_ProjectsItemFieldsIntoSerializableModels`
- `TriageClassifierRebuild_Tests.RebuildFromMinedMailAsync_ValidTriageLabels_RebuildsAllClassifierState`
- `TriageClassifierRebuild_Tests.RebuildFromMinedMailAsync_InvalidTriageLabel_DoesNotMutateAggregateState`
- `TriageClassifierRebuild_Tests.RebuildFromMinedMailAsync_ValidTrainingData_PersistsAndReplacesManagerOnce`
- `TriageClassifierRebuild_Tests.RebuildFromMinedMailAsync_NoValidTrainingData_DoesNotPersistOrReplaceManager`
- `TriageClassifierRebuild_Tests.RebuildFromStagedMinedMailAsync_StagedTrainingData_PersistsAndReplacesManager`
- `TriageClassifierRebuild_Tests.RebuildFromStagedMinedMailAsync_MissingAppData_DoesNotLoadStagedMail`
- `RibbonExplorerXmlTests.RibbonExplorerXml_BuildTriageClassifierIsInSettingsFolderClassifierMenu`
- `RibbonViewerEngineCallbackShapeTests.BuildTriageClassifierAsync_AwaitsInjectedRebuildOperation`
- `RibbonViewerEngineCallbackShapeTests.BuildTriageClassifierAsync_WhenTriageEngineIsAbsent_UsesLazyTriageBeforeInjectedRebuild`

## Appendix B: Toolchain Commands Reference

```powershell
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation
vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"
git diff --check c76e830c18976221b5730f84b8d88aebbfc4f04b..3a355e14a57109f5470fcf3b7d747351bade5804
```

PowerShell checks used the canonical `run_poshqc_format`, `run_poshqc_analyze`, and `run_poshqc_test` MCP operations against `.codex/hooks` in a detached exact-head worktree.
