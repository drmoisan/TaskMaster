# Policy Compliance Audit: Folder-tree dispatcher thread affinity

**Audit Date:** 2026-08-04  
**Code Under Test:** Full uncommitted diff from `origin/main` to `bug/folder-tree-dispatcher-thread-affinity-420`, including nine production C# files, eight test sources, and two project manifests.

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
| --- | --- | --- | --- | --- | --- | --- |
| C# | 9 production, 8 test sources, 2 manifests | 6,082 recorded MSTest tests | PASS: 6,082 / 6,082 | 69.2280% repository line coverage | 84.5459% repository line coverage | FAIL: several new methods are 0% and added-line rates are below 90% |
| TypeScript | 0 | N/A | N/A | N/A | N/A | N/A |
| PowerShell | 0 | N/A | N/A | N/A | N/A | N/A |

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: N/A - out of scope
- TypeScript post-change coverage artifact: N/A - out of scope
- PowerShell baseline coverage artifact: N/A - out of scope
- PowerShell post-change coverage artifact: N/A - out of scope
- Per-language comparison summary: Section 1.2.1 and `evidence/qa-gates/coverage-and-quality-delta.2026-08-04T19-40.md`

## Executive Summary

Status: FAIL. The C# formatter check and both build modes succeeded during review, and the recorded full MSTest-with-coverage run reports 6,082 passing tests. However, the branch does not meet the repository coverage policy or the full-bug acceptance criteria. The final coverage report explicitly records low added-line coverage, including zero-covered new methods, and incorrectly treats the `>=90%` threshold as inapplicable even though new methods were introduced. Code inspection also identified untested disposal, dispatcher, and cold-view lifecycle races.

**Policy documents evaluated:**

- PASS: `AGENTS.md` general code-change policy.
- FAIL: `AGENTS.md` general unit-test policy, specifically `>=90%` coverage for new methods and no coverage reduction for changed code.
- PARTIAL: `AGENTS.md` C# code-change and unit-test policy.

## 1. General Unit Test Policy Compliance

| Requirement | Status | Evidence |
| --- | --- | --- |
| Independence and isolation | PARTIAL | New tests use fakes and dedicated STA dispatchers, but none exercises UI-dispose versus worker-composition, dispose during a build, or a viewer closing during cold initialization. |
| Determinism | PASS | The added regression tests use in-process fakes, no Outlook, network, temporary files, sleeps, retry loops, or timers. |
| Concurrency and state transitions | FAIL | The changed dispatch/disposal state transitions are not fully covered; see code review findings CR-001 through CR-004. |
| Repository line coverage `>=80%` | PASS | Recorded final evidence reports 84.5459%. |
| New methods/classes/modules `>=90%` | FAIL | Final Cobertura reports 0% for `FilterOlFoldersController.CreateAsync(IApplicationGlobals)` and `WpfUiDispatcher.InvokeAsync(Func<Task<TResult>>)`. The added-line table also reports 0% for `RibbonViewer.cs`, `TryFunctionalityInConstruction.cs`, `IUiDispatcher.cs`, and `WpfUiDispatcher.cs`. |
| Changed-code coverage not reduced | FAIL | `coverage-and-quality-delta.2026-08-04T19-40.md` reports `FilterOlFoldersController.cs` at 41.18% added-line coverage and does not establish no regression for modified behavior. The reported baseline/final repository scopes are not comparable: 79,137 versus 109,324 lines. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 69.2280% lines -> Post-change: 84.5459% lines. Change: +15.3179% lines, not comparable because coverage scopes differ. New/changed-code coverage: 0.00% minimum observed, below the required 90%. Disposition: FAIL. Evidence: `evidence/baseline/mstest-coverage.2026-08-04T19-25.md`, `evidence/qa-gates/mstest-coverage.2026-08-04T19-38.md`, `evidence/qa-gates/coverage-and-quality-delta.2026-08-04T19-40.md`, and `evidence/qa-gates/coverage-final.cobertura.xml`.
- TypeScript: N/A - out of scope.
- PowerShell: N/A - out of scope.

## 2. General Code Change Policy Compliance

| Requirement | Status | Evidence |
| --- | --- | --- |
| Objective, plan, and regression-first workflow | PASS | Issue #420, `spec.md`, plan, and fail-before evidence exist. |
| Minimal targeted fix | PARTIAL | The core STA dispatch repair is targeted, but it introduces a synchronous worker-to-UI initialization while holding a service gate and adds an unawaited public-constructor initialization path. |
| Error handling and contracts | FAIL | The public constructor discards `InitializeAsync` failures; service disposal and publication do not preserve a disposed-state invariant. |
| Files below 500 lines | PASS | Final inventory reports all changed C# sources below the limit. |
| Documentation and evidence | PARTIAL | The final inventory and coverage delta contain incorrect or unsupported PASS assertions. |

## 3. Language-Specific Code Change Policy Compliance

### C#

| Requirement | Status | Evidence |
| --- | --- | --- |
| CSharpier | PASS | Review command `dotnet tool run csharpier check .` exited 0 and checked 1,464 files. |
| Analyzer build | PASS with pre-existing warnings | Review command with `EnableNETAnalyzers=true` exited 0; five `System.Reactive` packages.config warnings remain, matching baseline-style package warnings. |
| Nullable build | PASS with pre-existing warnings | Review command with `Nullable=enable` and `TreatWarningsAsErrors=true` exited 0; the same five package warnings remain. |
| UI/COM thread-affinity contract | FAIL | The branch does not prove notification cleanup, disposal, publication, and all initialization paths remain safe on the captured STA dispatcher. |

## 4. Language-Specific Unit Test Policy Compliance

### C#

| Requirement | Status | Evidence |
| --- | --- | --- |
| MSTest, Moq, FluentAssertions | PASS | Existing and added tests use the required framework and libraries. |
| Positive and negative behavior | PARTIAL | Worker-originated cold build, forced yield, and factory completion are exercised, but closed-view, dispatch-failure, UI/worker composition, notification refresh, and disposal races are not. |
| New-method coverage | FAIL | No approved exception supports waiving the `>=90%` new-method requirement. |

## 5. Test Coverage Detail

| Component | Recorded coverage finding | Status |
| --- | --- | --- |
| `FilterOlFoldersController` | Public `CreateAsync(IApplicationGlobals)` has line-rate 0 in `coverage-final.cobertura.xml` (lines 40-41). | FAIL |
| `WpfUiDispatcher` | New `InvokeAsync(Func<Task<TResult>>)` has line-rate 0 (line 42). | FAIL |
| `AppOlObjects` | Added lines are 17/33 hit (51.52%). | FAIL |
| `OutlookFolderTreeService` | Added lines are 15/25 hit (60.00%). | FAIL |
| `OutlookFolderHierarchyReader` | Added lines are 2/18 hit (11.11%). | FAIL |

## 6. Test Execution Metrics

| Metric | Value | Status |
| --- | --- | --- |
| Recorded full MSTest run | 6,082 passed, 0 failed | PASS |
| Recorded repository line coverage | 84.5459% | PASS |
| Required new-method coverage | Not met; several new methods are 0% | FAIL |
| Review formatter check | 1,464 files checked | PASS |
| Review analyzer build | Exit 0 | PASS with pre-existing package warnings |
| Review nullable build | Exit 0 | PASS with pre-existing package warnings |

## 7. Code Quality Checks

| Check | Command | Result | Status |
| --- | --- | --- | --- |
| Formatting | `dotnet tool run csharpier check .` | Exit 0 | PASS |
| Analyzers | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | Exit 0; five pre-existing package warnings | PASS |
| Nullable | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true` | Exit 0; five pre-existing package warnings | PASS |
| Tests and coverage | Recorded `Invoke-MSTestWithCoverage.ps1` result | 6,082 passed; coverage policy fails | FAIL |

## 8. Gaps and Exceptions

### Identified Gaps

- New methods and changed lines are below the required `>=90%` coverage target. The asserted waiver is unsupported by `AGENTS.md`.
- The coverage baseline and final reports use materially different denominators and cannot establish the claimed repository delta.
- The branch lacks deterministic coverage for service-gate/UI-dispatch deadlock, dispatcher-owned notification cleanup, publish-after-dispose, cold-view closure, public constructor failure, notification refresh dispatch, and ribbon failure propagation.

### Approved Exceptions

None. No approved exception record authorizes the coverage waiver or the listed untested concurrency paths.

## 9. Summary of Changes

The branch dispatches initial service composition and folder-tree builds to `WpfUiDispatcher`, retains dispatcher context around traversal yields, and introduces asynchronous FilterOlFolders initialization. These changes address the original `WpfDispatcherYield` exception path, and the branch contains no `Task.Yield` fallback. The implementation remains non-compliant because the changed lifecycle and disposal behavior has blocking untested defects and insufficient coverage.

## 10. Compliance Verdict

### Overall Status: NON-COMPLIANT

Remediation is required before PR readiness. The required coverage policy is not satisfied, and the review identified high-severity thread-affinity and lifecycle defects.

## Appendix A: Test Inventory

- `OutlookFolderTreeServiceConcurrencyTests.GetSnapshotAsync_WorkerOriginatedColdBuild_UsesCapturedStaDispatcher`
- `AppOlObjectsFolderTreeServiceTests.FolderTreeService_WorkerFirstAccess_ComposesOnCapturedStaDispatcher`
- `FolderTreeSnapshotBuilderYieldTests.BuildSnapshotAsync_AfterForcedYield_KeepsSubsequentYieldsOnDispatcher`
- `OutlookFolderHierarchyReaderTests.ReadRecordsAsync_AfterForcedYield_KeepsFolderAccessOnDispatcher`
- `WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict`
- `FilterOlFoldersControllerInitializationTests.CreateAsync_WiresViewerOnlyAfterSnapshotCompletes`
- `TryFunctionalityInConstructionTests.TryLoadFolderFilterAsync_AwaitsControlledInitialization`

## Appendix B: Toolchain Commands Reference

```powershell
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-04-folder-tree-dispatcher-thread-affinity-420/evidence/qa-gates/coverage-final.cobertura.xml
```

**Audit Completed By:** feature-reviewer-c3  
**Audit Date:** 2026-08-04  
**Policy Version:** Current
