# Policy Compliance Audit: Folder-tree dispatcher thread affinity

**Audit Date:** 2026-08-06
**Reviewed range:** `ce0c91e686bf7e060aaab6f185ee6883269e4fd4..369e974e7396f0321c6b51f5ae96c0794fa12460`

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
| --- | --- | --- | --- | --- | --- | --- |
| C# | 11 production, 22 test, 4 project | 6,166 tests | PASS: 6,166/6,166 | 69.2280% lines | 84.8015% lines | 99.7730% |
| Python | 0 files | N/A | N/A | N/A | N/A | N/A |
| TypeScript | 0 files | N/A | N/A | N/A | N/A | N/A |
| PowerShell | 0 files | N/A | N/A | N/A | N/A | N/A |
| Bash | 0 files | N/A | N/A | N/A | N/A | N/A |
| JSON | 0 files | N/A | N/A | N/A | N/A | N/A |

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: N/A - out of scope
- TypeScript post-change coverage artifact: N/A - out of scope
- PowerShell baseline coverage artifact: N/A - out of scope
- PowerShell post-change coverage artifact: N/A - out of scope
- Per-language comparison summary: Section 1.2.1 and `evidence/qa-gates/remediation-cycle4-coverage-and-quality-delta.2026-08-06T18-36.md`

## Executive Summary

Status: PASS. The committed issue #420 C# repair satisfies dispatcher, lifecycle, deterministic-test, formatting, and coverage requirements. Cycle-4 evidence records a complete format, analyzer, nullable, and MSTest-with-coverage pass. This audit independently reran the non-mutating CSharpier check and exact base-to-head whitespace check; both passed. The five packages.config compatibility warnings are recorded as pre-existing warnings, not new diagnostics.

Policies evaluated: `AGENTS.md` standing, general code-change, general unit-test, C# code-change, and C# unit-test policies; `.agents/skills/csharp/SKILL.md`; and the revision-7 feature plan.

## 1. General Unit Test Policy Compliance

| Requirement | Status | Evidence |
| --- | --- | --- |
| Independence, isolation, determinism | PASS | Cycle-4 fixtures use mocks, fakes, controlled tasks, and dedicated STA dispatchers; P5 records no Outlook, network, temporary-file, sleep, timer, polling, or retry dependency. |
| Positive, negative, lifecycle, and concurrency coverage | PASS | `evidence/regression-testing/remediation-cycle4-acceptance-criteria-mapping.2026-08-06T18-20.md` maps AC1-AC6 and CR-001-CR-006 to current tests. |
| Repository coverage >=80% | PASS | 93,687/110,478 lines (84.8015%). |
| New/changed-code coverage >=90% | PASS | Direct changed-line scan: 879/881 (99.7730%); target methods/classes/modules are at least 95%. |
| No changed-line coverage regression | PASS | `evidence/qa-gates/remediation-cycle4-coverage-and-quality-delta.2026-08-06T18-36.md` reconciles denominators and records no regression. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 69.2280% lines. Post-change: 84.8015% lines. Change: +15.5735 percentage points. New/changed-code coverage: 99.7730%. Disposition: PASS. The final cycle-4 delta separately reconciles the 110,478-line P5/P6 scope and records no changed-line regression. Evidence: `evidence/baseline/mstest-coverage.2026-08-04T19-25.md`; `evidence/qa-gates/remediation-cycle4-coverage-and-quality-delta.2026-08-06T18-36.md`.
- TypeScript: N/A - out of scope.
- PowerShell: N/A - out of scope.

## 2. General Code Change Policy Compliance

| Requirement | Status | Evidence |
| --- | --- | --- |
| Objective, plan, and regression-first workflow | PASS | `issue.md`, `spec.md`, revision-7 plan, fail-before artifacts, and green regression artifacts exist. |
| Targeted design and contracts | PASS | `IOutlookFolderTreeService.GetSnapshotAsync` is retained and dispatch ownership remains internal. |
| Error handling and lifecycle behavior | PASS | Current evidence covers retry, dispatcher failure, disposal during build/refresh, cleanup observation, and viewer ownership. |
| File structure and 500-line limit | PASS | Independent line-count scan found every changed C# file at or below 500 lines; the approved lifecycle partial is 296 lines. |
| Documentation and evidence | PASS | The specification records final design, QA evidence, coverage values, and no waiver. |

## 3. Language-Specific Code Change Policy Compliance

### C#

| Requirement | Status | Evidence |
| --- | --- | --- |
| CSharpier | PASS | `dotnet tool run csharpier check .`: 1,474 files checked, exit 0. |
| Analyzer build | PASS | Cycle-4 analyzer evidence: exit 0; no analyzer errors. |
| Nullable compilation | PASS | Cycle-4 nullable evidence: exit 0; no nullable diagnostics. |
| Thread-affinity and API safety | PASS | Diff inspection plus AC1-AC5 evidence confirms captured-dispatcher composition/build/refresh and strict WPF yielding. |

## 4. Language-Specific Unit Test Policy Compliance

### C#

| Requirement | Status | Evidence |
| --- | --- | --- |
| MSTest, Moq, and FluentAssertions | PASS | Current test surfaces use the required framework and deterministic fakes/mocks. |
| Dispatcher and lifecycle regressions | PASS | Cycle-4 evidence covers worker-first composition, forced-yield continuation, cleanup, disposal, refresh faults, and view lifecycle. |
| Toolchain test gate | PASS | `remediation-cycle4-mstest-coverage.2026-08-06T18-35.md`: 6,166 passed, 0 failed. |

## 5. Test Coverage Detail

| Component | Coverage | Status |
| --- | ---: | --- |
| `AppOlObjects.FolderTreeService.cs` | 291/292 | PASS |
| `FilterOlFoldersController.cs` | 101/102 | PASS |
| `FilterOlFoldersController.Lifecycle.cs` | 334/335 | PASS |
| `OutlookFolderTreeService.cs` | 359/359 | PASS |
| `WpfUiDispatcher.cs` | 12/12 | PASS |

## 6. Test Execution Metrics

| Metric | Value | Status |
| --- | ---: | --- |
| Full repository test result | 6,166 passed / 0 failed | PASS |
| Repository line coverage | 84.8015% | PASS |
| Changed production coverage | 99.7730% | PASS |
| Independent formatter check | 1,474 files, exit 0 | PASS |
| Independent whitespace check | exit 0 | PASS |

## 7. Code Quality Checks

| Check | Command | Result | Status |
| --- | --- | --- | --- |
| Formatting | `dotnet tool run csharpier check .` | 1,474 files checked; exit 0 | PASS |
| Analyzer | revision-7 P6-T2 command | Recorded cycle-4 exit 0 | PASS |
| Nullable | revision-7 P6-T3 command | Recorded cycle-4 exit 0 | PASS |
| Tests and coverage | revision-7 P6-T4 command | 6,166/6,166; 84.8015% line coverage | PASS |
| Whitespace | `git diff --check ce0c91e686bf7e060aaab6f185ee6883269e4fd4..369e974e7396f0321c6b51f5ae96c0794fa12460` | exit 0 | PASS |

## 8. Gaps and Exceptions

No policy exception, waiver, suppressed coverage threshold, or coverage-scope change was found. AC8 remains unchecked in `spec.md`; its documented content is complete, and this audit-only assignment does not authorize source requirement edits.

## 9. Summary of Changes

1. `b4e9daac` — retained traversal on the captured Outlook STA dispatcher.
2. `369e974e` — repaired post-disposal UI mutation, completion, cleanup, and coverage evidence paths.

## 10. Compliance Verdict

### Overall Status: FULLY COMPLIANT

The committed issue #420 scope satisfies reviewed C# policy gates. The unchecked AC8 checkbox is an external source-tracking action, not a missing implementation, validation, or scope-deviation record.

## Appendix A: Test Inventory

- `AppOlObjectsFolderTreeServiceLifecycleTests` and its cycle-4 coverage partial.
- `FilterOlFoldersControllerInitializationTests` and `FilterOlFoldersControllerRefreshDisposalTests` partials.
- Folder-tree concurrency, disposal, invalidation, traversal-cancellation, hierarchy-reader, yield, dispatcher, and ribbon test surfaces.

## Appendix B: Toolchain Commands Reference

- `dotnet tool run csharpier check .`
- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-04-folder-tree-dispatcher-thread-affinity-420/evidence/qa-gates/remediation-cycle4-coverage-final.cobertura.xml`
- `git diff --check ce0c91e686bf7e060aaab6f185ee6883269e4fd4..369e974e7396f0321c6b51f5ae96c0794fa12460`

**Audit Completed By:** feature-review agent
**Audit Date:** 2026-08-06
**Policy Version:** Current
