# Policy Audit - folder-tree-cache-and-refresh (Issue #214)

Timestamp: 2026-06-24T20:08:00-04:00
Base Branch: main
Feature Branch: refactor/folder-tree-cache-and-refresh-214
Feature Folder: docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214
Review Type: Feature-review remediation re-review

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | C# production and test files in the issue #214 remediation diff | 4178 | PASS: 4178 passed, 0 failed | 82.91% repository line coverage (99029/119447) | 82.98% repository line coverage (99577/120000) | Folder tree scoped 92.54%; EmailDataMiner folder extraction 94.52%; FilterOlFolders issue scope 100.00%; SubjectMap issue scope 94.05% |
| TypeScript | 0 | N/A | N/A | N/A | N/A - out of scope | N/A - out of scope |
| PowerShell | 0 | N/A | N/A | N/A | N/A - out of scope | N/A - out of scope |

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - out of scope` (no TypeScript files in diff)
- TypeScript post-change coverage artifact: `N/A - out of scope` (no TypeScript files in diff)
- PowerShell baseline coverage artifact: `N/A - out of scope` (no PowerShell files in diff)
- PowerShell post-change coverage artifact: `N/A - out of scope` (no PowerShell files in diff)
- Per-language comparison summary: see Section 1.2.1 below; C# evidence at `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/remediation-final-coverage-comparison.2026-06-24T19-23.md`

**Non-negotiable verdict rule:** Numeric baseline and post-change coverage metrics are present for the single in-scope language, C#. TypeScript and PowerShell are out of scope because the branch diff contains no files for those languages.

**Fail-closed rule:** Required C# coverage evidence exists and contains numeric values. The audit verdict is PASS because no remaining policy blocker was identified in the remediation re-review.

## Executive Summary

The remediation evidence addresses the previously reported policy blockers for issue #214. The final C# toolchain ran in the required order after the last test-fixture correction: CSharpier, .NET analyzers, nullable analysis with warnings as errors, and MSTest with coverage. The final coverage evidence reports repository coverage at 82.98% and issue-scoped coverage above the 90% threshold for instrumented issue #214 areas.

The refreshed PR context was collected against `main`. The collector reports the committed `HEAD` SHA, while the remediation changes remain in the working tree. This re-review therefore uses the refreshed PR context plus the current worktree diff, final QA evidence, and remediation evidence files.

## Rejected Scope Narrowing

No scope narrowing was accepted for this re-review. The review covers the issue #214 remediation against `main`, the refreshed PR context, and the current working tree remediation diff.

## Evidence Location Compliance

All remediation evidence produced during execution is under `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/`, including remediation baseline, regression testing, caller migration, policy checks, diff checks, issue updates, and QA gates.

## 1. General Unit Test Policy Compliance

Status: PASS

Evidence:
- Regression tests use fakes, adapters, Moq, FluentAssertions, and MSTest.
- No live Outlook COM tests were added.
- Final MSTest coverage run passed 4178/4178 tests.

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 82.91% repository line coverage (99029/119447) -> Post-change: 82.98% repository line coverage (99577/120000). Change: +0.07 percentage points. New/changed-code coverage: folder tree scoped 92.54%, EmailDataMiner folder extraction 94.52%, FilterOlFolders issue scope 100.00%, SubjectMap issue scope 94.05%. Disposition: PASS. Evidence: `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/remediation-final-coverage-comparison.2026-06-24T19-23.md`.
- TypeScript: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A - out of scope. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - no TypeScript files in diff.
- PowerShell: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A - out of scope. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - no PowerShell files in diff.

## 2. General Code Change Policy Compliance

Status: PASS

Evidence:
- The remediation plan is checked through P4-T5 and P4-T6 is in progress at this re-review step.
- `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs` has no branch or working-tree diff.
- Active feature markdown no longer contains out-of-scope startup issue references.
- `git diff --check main` remaining whitespace exceptions are generated TRX artifacts documented in `evidence/diff-check/remediation-diff-check.2026-06-24T19-23.md`.

## 3. Language-Specific Code Change Policy Compliance

Status: PASS

Evidence:
- CSharpier final evidence: `evidence/qa-gates/remediation-final-csharpier.2026-06-24T19-23.md`.
- Analyzer final evidence: `evidence/qa-gates/remediation-final-dotnet-analyzers.2026-06-24T19-23.md`.
- Nullable final evidence: `evidence/qa-gates/remediation-final-nullable.2026-06-24T19-23.md`.

## 4. Language-Specific Unit Test Policy Compliance

Status: PASS

Evidence:
- Final MSTest coverage evidence: `evidence/qa-gates/remediation-final-mstest-coverage.2026-06-24T19-23.md`.
- Test run result: 4178 total, 4178 passed, 0 failed.
- Test framework remains MSTest with Moq and FluentAssertions in new/updated tests.

## 5. Test Coverage Detail

Status: PASS

Evidence:
- Repository coverage: 82.98% (99577/120000).
- Folder tree/cache scoped coverage: 92.54% (657/710 ranges).
- EmailDataMiner folder extraction coverage: 94.52% (138/146 ranges).
- FilterOlFolders scoped coverage: 100.00% (53/53 ranges).
- SubjectMap orchestration coverage: 94.05% (79/84 ranges).
- Coverage comparison evidence: `evidence/qa-gates/remediation-final-coverage-comparison.2026-06-24T19-23.md`.

## 6. Test Execution Metrics

Status: PASS

Final command:
`C:\Users\DanMoisan\.dotnet\tools\dotnet-coverage.exe collect --output docs\features\active\2026-06-24-folder-tree-cache-and-refresh-214\evidence\qa-gates\remediation-final-coverage.2026-06-24T19-23.xml --output-format xml --settings coverage.config -- C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`

Result: exit code 0, 4178 passed, 0 failed.

## 7. Code Quality Checks

Status: PASS

Final commands:
1. `dotnet tool run csharpier format .`
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. MSTest coverage command listed in section 6.

## 8. Gaps and Exceptions

Status: PASS WITH DOCUMENTED GENERATED-OUTPUT EXCEPTION

Documented exception:
- Generated TRX whitespace in prior coverage-results diagnostic artifacts remains documented as generated output in `evidence/diff-check/remediation-diff-check.2026-06-24T19-23.md`.

No policy blocker remains from this exception.

## 9. Summary of Changes

The remediation adds cooperative async traversal, actual Outlook notification subscription ownership through testable adapters, request-scope-aware cache reuse, store-scoped refresh preservation, and EmailDataMiner caller migration away from issue #214 throwaway `FolderTree` construction.

## 10. Compliance Verdict

PASS. The remediation satisfies the policy checks reviewed for issue #214.

## Appendix A: Test Inventory

Representative added or updated coverage includes:
- `OutlookFolderHierarchyReaderTests`
- `OutlookFolderNotificationSinkTests`
- `OutlookFolderTreeServiceInvalidationTests`
- `OutlookFolderTreeServiceScopeTests`
- `FolderTreeSnapshotBuilderTests`
- `EmailDataMiner_Additional_Tests`
- `OlFolderClassifierGroup_Tests`

## Appendix B: Toolchain Commands Reference

1. `dotnet tool run csharpier format .`
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. `C:\Users\DanMoisan\.dotnet\tools\dotnet-coverage.exe collect --output docs\features\active\2026-06-24-folder-tree-cache-and-refresh-214\evidence\qa-gates\remediation-final-coverage.2026-06-24T19-23.xml --output-format xml --settings coverage.config -- C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`
