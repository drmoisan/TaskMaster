# Policy Audit - folder-tree-cache-and-refresh (Issue #214)

- Component: Outlook folder-tree cache and refresh service
- Date: 2026-06-24T19-23
- Reviewer: feature-reviewer agent
- Review type: feature branch audit
- Work Mode: full-feature
- Base branch: `main` / `origin/main` @ `168eba0ba1f79290be9eda29edc4332ac1ce2061`
- Head: `refactor/folder-tree-cache-and-refresh-214` @ `c2423376f0e37e61737aba57a788b3be3bdd0bf4`
- Diff range: `168eba0ba1f79290be9eda29edc4332ac1ce2061..c2423376f0e37e61737aba57a788b3be3bdd0bf4`
- Active feature folder: `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214`
- Template note: the required MCP policy-audit template resolver was not exposed in the available `drm-copilot` tool surface for this session. This audit preserves the canonical major headings and records the resolver limitation as an audit exception.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 75 `.cs` files and 4 `.csproj` files in branch diff | 4167 | PASS: 4167 passed, 0 failed | 82.54% repository line coverage (96077/116403) | 82.91% repository line coverage (99030/119447) | Folder tree scoped 97.95%; `EmailDataMiner.FolderExtraction.cs` 95.68%; FilterOlFolders issue scope 100%; SubjectMap issue scope 94.05% |
| TypeScript | 0 | N/A | N/A | N/A - out of scope | N/A - out of scope | N/A - out of scope |
| PowerShell | 0 | N/A | N/A | N/A - out of scope | N/A - out of scope | N/A - out of scope |

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - out of scope` (no TypeScript files in diff)
- TypeScript post-change coverage artifact: `N/A - out of scope` (no TypeScript files in diff)
- PowerShell baseline coverage artifact: `N/A - out of scope` (no PowerShell files in diff)
- PowerShell post-change coverage artifact: `N/A - out of scope` (no PowerShell files in diff)
- Per-language comparison summary: see Section 1.2.1 below; C# evidence at `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/final-coverage-comparison.md`

**Non-negotiable verdict rule:** Numeric baseline and post-change coverage metrics are present for the single in-scope language, C#. TypeScript and PowerShell are out of scope because the branch diff contains no files for those languages.

**Fail-closed rule:** Required coverage evidence exists and contains numeric values, but the audit verdict is FAIL because behavioral review found acceptance-criteria defects that measured coverage did not catch.

## Executive Summary

Verdict: FAIL.

The C# QA evidence records passing CSharpier, .NET analyzer, nullable, MSTest, and coverage runs. The final coverage comparison reports repository coverage at 82.91% and issue-scoped new-code coverage above the required thresholds. File-size evidence reports all touched C# source and test files at or below 500 lines. The branch also preserves the startup-scope exclusion: `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs` has no diff.

The policy audit is still FAIL because feature review identified implementation defects that invalidate checked acceptance criteria:

- Production hierarchy enumeration is performed synchronously before the first dispatcher yield, so live Outlook folder traversal is not cooperatively yielded during enumeration.
- The production notification sink is constructed with no Outlook event subscriptions, so folder/store add, remove, move, or rename invalidation is not wired in production.
- The shared service returns the current cached snapshot without verifying that it satisfies the requested store scope.
- Store-scoped notification refreshes can publish a single-store snapshot as the service-wide current snapshot.
- `EmailDataMiner.FolderExtraction.cs` retains direct `FolderTree` construction helpers and reachable scrape paths; the caller-migration evidence did not search that partial file.
- `git diff --check main..HEAD` fails on generated evidence artifacts with trailing whitespace and blank-line-at-EOF diagnostics.

Remediation is required before PR readiness.

## Rejected Scope Narrowing

No scope narrowing was accepted. The audit used the full branch diff from `origin/main` to `c2423376`, the canonical PR-context summary and appendix, and the active feature folder supplied in the user request.

## Evidence Location Compliance

PASS with one template exception. Feature evidence is stored under `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/<kind>/`, including `baseline`, `qa-gates`, `other`, and `issue-updates`. The large coverage XML and TRX files are under the active feature folder evidence tree. No review evidence was written under forbidden `artifacts/` evidence subpaths.

The policy-audit template resolver required by policy was unavailable in the exposed MCP tool surface. This audit uses the required headings and will be validated structurally through `validate_orchestration_artifacts`.

## 1. General Unit Test Policy Compliance

Verdict: FAIL.

- PASS: Final MSTest coverage evidence reports `4167` total tests, `4167` passed, `0` failed, using `TestCategory!=LiveOutlook`.
- PASS: No-live-Outlook evidence scanned added test lines and found no live Outlook COM construction.
- PASS: New tests use fake hierarchy, fake clock, fake dispatcher yield, fake notification source, and cancellation seams.
- FAIL: Test coverage does not catch production notification wiring absence. `OutlookFolderNotificationSink(Outlook.NameSpace)` delegates to `Array.Empty<IOutlookFolderNotificationSubscription>()`, so the production constructor does not subscribe Outlook event sources despite fake-based tests passing.
- FAIL: Test coverage does not catch request-scope cache correctness. A current single-store snapshot can satisfy a later all-store or different-store request because `GetSnapshotAsync` returns `_snapshot` without validating `request.StoreIds`.
- FAIL: Test coverage does not prove live traversal yields during COM enumeration. `FolderTreeSnapshotBuilder.BuildSnapshotAsync` calls `_reader.ReadFolders(...)` before the first await/yield point.

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 82.54% repository line coverage (96077/116403) -> Post-change: 82.91% repository line coverage (99030/119447). Change: +0.37 percentage points. New/changed-code coverage: folder tree scoped 97.95%, `EmailDataMiner.FolderExtraction.cs` 95.68%, FilterOlFolders issue scope 100%, SubjectMap issue scope 94.05%. Disposition: PASS for measured coverage, FAIL for behavioral sufficiency. Evidence: `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/final-coverage-comparison.md`.
- TypeScript: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A - out of scope. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - no TypeScript files in diff.
- PowerShell: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A - out of scope. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - no PowerShell files in diff.

## 2. General Code Change Policy Compliance

Verdict: FAIL.

- PASS: File-size evidence reports 69 touched C# files at or below 500 lines.
- PASS: New folder-tree snapshot and service types are cohesive and placed in focused files.
- FAIL: Separation between live Outlook enumeration and cooperative yielding is incomplete. The implementation collects all live folder records synchronously in the reader before the builder can yield.
- FAIL: Cache scope invariants are not enforced. The service stores one `_snapshot` but does not track whether that snapshot covers all stores or only a requested subset.
- FAIL: `git diff --check main..HEAD` failed with trailing-whitespace and blank-line-at-EOF diagnostics in evidence files, including `baseline-dotnet-analyzers.md`, `baseline-nullable.md`, `.trx` outputs, and `final-coverage-comparison.md`.

## 3. Language-Specific Code Change Policy Compliance

Verdict: FAIL.

- PASS: Final CSharpier evidence reports `dotnet tool run csharpier format .` exit code 0.
- PASS: Final .NET analyzer evidence reports `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` exit code 0.
- PASS: Final nullable evidence reports `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` exit code 0, with 0 warnings and 0 errors.
- FAIL: C# behavioral contracts for the cache service are not satisfied by the implementation. `OutlookFolderTreeService.GetSnapshotAsync` does not validate request coverage before returning a current snapshot, and store-scoped refresh publication can drop unaffected stores.
- FAIL: The C# Outlook notification lifecycle requirement is not implemented for production Outlook sources. The concrete `OutlookFolderNotificationSink` public constructor keeps an empty subscription collection.

## 4. Language-Specific Unit Test Policy Compliance

Verdict: PARTIAL.

- PASS: The C# test suite uses MSTest, FluentAssertions, Moq-compatible fake seams, and no live Outlook COM tests for issue #214 additions.
- PASS: Coverage evidence reports issue-scoped thresholds met: folder tree scoped coverage 97.95%, `EmailDataMiner.FolderExtraction.cs` 95.68%, `FilterOlFoldersController` scoped 100%, and `SubjectMapSco.Orchestration.cs` 94.05%.
- PARTIAL: The test suite omits negative coverage for production subscription construction, request-scope cache compatibility, store-scoped refresh preservation of unaffected stores, and direct `FolderTree` construction in the new `EmailDataMiner.FolderExtraction.cs` partial.

## 5. Test Coverage Detail

Verdict: PASS for measured coverage, FAIL for behavioral sufficiency.

Coverage evidence:

- `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/final-mstest-coverage.md`
- `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/final-coverage-comparison.md`

Recorded results:

- Repository coverage: 82.91% (99030/119447), threshold >= 80%.
- Issue #214 folder tree scoped coverage: 97.95%, threshold >= 90%.
- `EmailDataMiner.FolderExtraction.cs`: 95.68%, threshold >= 90%.
- `FilterOlFoldersController` issue-scoped snapshot coverage: 100%, threshold >= 90%.
- `SubjectMapSco.Orchestration.cs`: 94.05%, threshold >= 90%.
- TaskMaster Ribbon issue-scoped methods: accepted by method-level non-instrumentation rationale due existing type-level `[ExcludeFromCodeCoverage]`.

Measured coverage passes, but coverage is not sufficient to validate the production notification and request-scope behaviors listed in this audit.

## 6. Test Execution Metrics

Verdict: PASS for recorded execution.

- Final MSTest command: `dotnet-coverage collect ... vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook"`
- EXIT_CODE: 0
- Total tests: 4167
- Passed: 4167
- Failed: 0
- Live Outlook COM tests excluded by filter and separate no-live-COM evidence.

## 7. Code Quality Checks

Verdict: FAIL.

Recorded final QA evidence:

- CSharpier: PASS, exit code 0.
- .NET analyzers: PASS, exit code 0.
- Nullable/TreatWarningsAsErrors: PASS, exit code 0.
- MSTest with coverage: PASS, exit code 0.
- File-size check: PASS.
- Banned API search: PASS for the stated patterns.
- Startup-scope exclusion: PASS for `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs` no-diff.

Reviewer check-only command:

- `git diff --check main..HEAD`: FAIL. Diagnostics are confined to generated evidence files and TRX outputs, but the branch diff is not whitespace-clean.

## 8. Gaps and Exceptions

1. FAIL: production notification subscriptions are not created by `OutlookFolderNotificationSink(Outlook.NameSpace)`.
2. FAIL: cooperative yielding occurs after synchronous hierarchy read, not during live Outlook enumeration.
3. FAIL: cache request-scope compatibility is not enforced.
4. FAIL: store-scoped refreshes can replace the global current snapshot with a partial snapshot.
5. FAIL: `EmailDataMiner.FolderExtraction.cs` retains direct `FolderTree` construction paths and was omitted from caller-migration evidence.
6. FAIL: `git diff --check` reports whitespace diagnostics in generated evidence files.
7. PARTIAL: Policy-audit template resolver unavailable in exposed MCP tool surface; structural headings preserved.

## 9. Summary of Changes

The branch adds a cached Outlook folder hierarchy service, snapshot model, dispatcher-yield and deadline seams, notification sink abstraction, caller integrations for Ribbon, EmailDataMiner, FilterOlFolders, and SubjectMap workflows, plus C# unit tests and feature evidence for issue #214. It also adds large baseline and final coverage artifacts under the active feature folder.

`TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs` is unchanged in the branch diff.

## 10. Compliance Verdict

FAIL.

The branch is not PR-ready. Toolchain and measured coverage evidence pass, but core acceptance criteria for cooperative live traversal, production notification lifecycle, request-scoped cache correctness, multi-store refresh correctness, and in-scope direct `FolderTree` retirement are not met. Remediation is required.

## Appendix A: Test Inventory

Reviewed test evidence and changed test areas:

- `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs`
- `TaskMaster.Test/Ribbon/RibbonControllerTests.cs`
- `UtilitiesCS.Test/EmailIntelligence/EmailDataMiner_FolderExtractionCoverage_Tests.cs`
- `UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersController_Tests.cs`
- `UtilitiesCS.Test/EmailIntelligence/SubjectMapSco_Orchestration_Tests.cs`
- `UtilitiesCS.Test/OutlookObjects/Folder/*Tests.cs`
- Fake hierarchy, fake notification, fake clock, fake dispatcher yield, and fake resolver support under `UtilitiesCS.Test/OutlookObjects/Folder/Fakes/`

Coverage and QA evidence:

- `evidence/qa-gates/final-mstest-coverage.md`
- `evidence/qa-gates/final-coverage-comparison.md`
- `evidence/qa-gates/final-qa-summary.md`
- `evidence/other/no-live-outlook-com-tests.md`

## Appendix B: Toolchain Commands Reference

Evidence commands reviewed:

1. `dotnet tool run csharpier format .`
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. `C:\Users\DanMoisan\.dotnet\tools\dotnet-coverage.exe collect --output docs\features\active\2026-06-24-folder-tree-cache-and-refresh-214\evidence\qa-gates\final-coverage-repository.xml --output-format xml -- C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook" /ResultsDirectory:docs\features\active\2026-06-24-folder-tree-cache-and-refresh-214\evidence\qa-gates\coverage-results-repository`
5. `git diff --name-status main..HEAD -- TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs`
6. `git diff --unified=0 main..HEAD -- TaskMaster UtilitiesCS TaskMaster.Test UtilitiesCS.Test | rg -n "^\+.*(Application\.DoEvents|DateTime\.(Now|UtcNow)|Random\.Shared|Thread\.Sleep|Task\.Delay|Task\.Run\s*\(|new\s+FolderTree\s*\()" -S`
7. `rg -n "<out-of-scope-startup-issue-reference-pattern>" docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214 artifacts/pr_context.summary.txt artifacts/pr_context.appendix.txt --glob "!**/evidence/**/*.xml" --glob "!**/*.trx"`
8. `git diff --check main..HEAD`
