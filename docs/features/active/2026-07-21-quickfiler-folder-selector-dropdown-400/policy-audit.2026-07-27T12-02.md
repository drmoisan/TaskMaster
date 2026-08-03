# Policy Audit: QuickFiler Folder Selector Drop-Down (#400)

**Audit Date:** 2026-07-27  
**Reviewed range:** `origin/main` merge base `e63ddc7c18ca71e2c968b3329e42d965d45af1eb` to `83efd313c3f49b66d5f2e133467770284cca7253`  
**Feature folder:** `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400`

## Executive Summary

**Status: FAIL — remediation required.** This independent full-feature audit reviewed the fresh PR-context pair, the complete merge-base diff, `spec.md`, the remediation plan and inputs, current P8/P9/P10 evidence, and live read-only Git and coverage-artifact results. The C# successor evidence supports the claimed C# quality gates, but two Major policy findings prevent a full-feature PASS: the complete branch diff fails whitespace integrity, and the changed PowerShell scope lacks passing, attributable coverage at the repository policy threshold.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---:|---:|---|---:|---:|---:|
| C# | 68 | 6,075 | PASS | 84.1506% | 84.5568% | 90.6250% changed measurable host-neutral source points |
| PowerShell | 5 | 30 | PASS execution only | 0.0000% no attributable baseline metric | 0.0000% aggregate 0/1364; changed script absent | 0.0000% no attributable changed-code metric |
| TypeScript | 0 | 0 | N/A | N/A | N/A | N/A |

Coverage evidence:

- C# baseline: `evidence/baseline/coverage-accounting-remediation-baseline.2026-07-21T22-16.md`.
- C# post-change: `evidence/qa-gates/coverage-nonnumeric-adapter-member-coverage-relative-output.2026-07-27T10-55.cobertura.xml`.
- C# comparison: `evidence/qa-gates/coverage-nonnumeric-adapter-member-coverage-relative-output-delta.2026-07-27T11-16.md`.
- TypeScript baseline coverage artifact: N/A - zero changed TypeScript files.
- TypeScript post-change coverage artifact: N/A - zero changed TypeScript files.
- PowerShell baseline coverage artifact: `evidence/baseline/coverage-wrapper-poshqc-test-baseline.2026-07-23T14-15.md`; no valid numeric baseline comparison is available for the complete changed PowerShell scope.
- PowerShell post-change coverage artifact: `artifacts/pester/powershell-coverage.xml`; FAIL because it has no changed `Invoke-MSTestWithCoverage.ps1` entry and aggregate counters are 0/1364.
- Per-language comparison summary: Section 1.2.1.

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 84.1506% -> Post-change: 84.5568%. Change: +0.4062 percentage points. New/changed-code coverage: 90.6250%. Evidence: `coverage-nonnumeric-adapter-member-coverage-relative-output-delta.2026-07-27T11-16.md`. Disposition: PASS.
- PowerShell: Baseline: 0.0000% (no attributable baseline metric) -> Post-change: 0.0000% (aggregate counters 0/1364 and no changed-script entry). Change: 0.0000 percentage points; comparison is invalid because attribution is absent. New/changed-code coverage: 0.0000%. Evidence: `artifacts/pester/powershell-coverage.xml`. Disposition: FAIL.
- TypeScript: no changed files. Disposition: N/A.

## Rejected Scope Narrowing

The review retained the complete feature-vs-merge-base scope. It did not treat the P9 C# coverage subset, the later P9-T61 history comparison, or the P8/P9 execution scope as a substitute for the complete branch diff. The supplied request required review against `e63ddc7...83efd313` with no scope narrowing; that requirement was applied.

## 1. General Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Deterministic C# regression and full-suite evidence | PASS | P8-T82 records two direct unbuffered `6,056/6,056` process-clean passes; P9-T57 records `6,075/6,075` with zero failures/skips. |
| PowerShell changed-code coverage | FAIL | The Pester receipt records 30/30 tests, but its referenced `artifacts/pester/powershell-coverage.xml` has no `Invoke-MSTestWithCoverage.ps1` class entry. Its aggregate line counters are `0/1364`; no source-attributable >=80% / >=90% result exists for the changed script. |
| No prohibited test dependencies | PASS | Reviewed C# and Pester evidence describes deterministic seams and no live Outlook/WebView2 requirement. |

## 2. General Code Change Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Cohesive changed C# files at or below 500 lines | PASS | Read-only complete-diff check found no changed `.cs` file above 500 lines; P9-T61 also lists its post-P9-T34 C# paths at 327, 302, and 494 lines. |
| Full diff integrity | FAIL | `git diff --check e63ddc7...83efd313` exited 1 with 2,764 diagnostics, including trailing whitespace in committed `evidence/regression-testing/*.trx` and `*.stdout.txt` files. |
| Configuration/filter/threshold integrity | PASS | `git diff --quiet <merge-base>..HEAD -- coverage.config scripts/vscode/TaskMaster.cli.runsettings .csharpierignore` exited 0. P9-T57 records the retained coverage and runsettings hashes. |

## 3. Language-Specific Code Change Policy Compliance

### C#

PASS. P9-T41 format/check, P9-T42 analyzer build, and P9-T43 nullable build are recorded for the source-current hashes. P9-T44 passed 19/19 focused cases. P9-T57 passed all eight Debug assemblies. P9-T59 uses exact Cobertura filename and `(filename,line)` source-range attribution, not merged class identity; its named host-neutral ranges are 90.5660% and 90.6977%, with named members at 100%.

### PowerShell

FAIL. Five PowerShell files changed in the complete branch diff, including `scripts/vscode/Invoke-MSTestWithCoverage.ps1` and its tests. Formatting and analyzer evidence exists; the Pester execution passed, but the available coverage XML does not instrument the changed script and reports aggregate line coverage of 0/1364. Required coverage verification is not satisfied.

## 4. Language-Specific Unit Test Policy Compliance

### C#

PASS. P8-T82 reauthorizes the retained P8-T73 history: each direct, process-owned run passed 6,056/6,056 with no timeout, cleanup, failures, or skips. P9-T57 passed 6,075/6,075 with zero failures/skips.

### PowerShell

FAIL. The Pester result proves execution only. It does not prove coverage for the changed production script or the required repository threshold.

## 5. Test Coverage Detail

| Language | Result | Evidence |
|---|---|---|
| C# | PASS | P9-T57: 92,380/109,252 = 84.5568%. P9-T59: changed measurable host-neutral ranges 288/318 = 90.5660% and 234/258 = 90.6977%; all named measurable members 100%. P9-T60 independently confirms the bounded nonnumeric direct-adapter provenance. |
| PowerShell | FAIL | Five `.ps1` changes are in the merge-base diff. The only cited coverage artifact is `artifacts/pester/powershell-coverage.xml`; it has zero entries for `Invoke-MSTestWithCoverage.ps1` and aggregate line counters of 0/1364. |

## 6. Test Execution Metrics

| Metric | Value | Status |
|---|---:|---|
| P8-T82 direct suite, run 1 | 6,056/6,056 | PASS |
| P8-T82 direct suite, run 2 | 6,056/6,056 | PASS |
| P9-T44 focused suite | 19/19 | PASS |
| P9-T57 coverage suite | 6,075/6,075 | PASS |
| Pester wrapper suite | 30/30 | PASS (execution only) |

## 7. Code Quality Checks

| Check | Command/evidence | Status |
|---|---|---|
| C# formatter | P9-T41 `csharpier format` then `csharpier check` | PASS |
| C# analyzers | P9-T42 analyzer-enabled `msbuild` | PASS |
| C# nullable | P9-T43 warnings-as-errors `msbuild` | PASS |
| C# tests and coverage | P9-T44 and P9-T57 | PASS |
| PowerShell format/analyze/test | `coverage-wrapper-poshqc-*.2026-07-25T20-30.md` | PARTIAL: test execution passes, coverage does not satisfy policy |
| Complete diff whitespace | `git diff --check e63ddc7...83efd313` | FAIL |

## 8. Gaps and Exceptions

### Identified Gaps

1. **Major — complete feature diff whitespace failure.** Remediate every diagnostic emitted by `git diff --check e63ddc7...83efd313`, including raw committed evidence artifacts, then record a clean complete-range result. P9-T61's working-tree/later-history check is not a valid substitute.
2. **Major — PowerShell coverage verification failure.** Run the repository PowerShell coverage gate for the complete changed PowerShell scope, produce source-attributable results for the changed script(s), and meet the applicable repository and changed-code thresholds without changing filters, exclusions, or thresholds.

### Approved Exceptions

None.

### Removed/Skipped Tests

None identified from the current evidence.

## 9. Summary of Changes

The complete branch adds/modifies 68 C# files, five PowerShell files, four project files, HTML, configuration/runtime-hook files, and extensive feature evidence. The primary implementation adds folder-selector state, popup lifecycle, readiness, dispatcher, and adapter seams; it also changes the MSTest coverage wrapper to use an output-adjacent derived coverage settings file. C# behavior and C# coverage evidence are current for the reviewed source hashes.

## 10. Compliance Verdict

### Overall Status: NON-COMPLIANT

The feature cannot be approved because the full branch violates the whitespace integrity gate and does not demonstrate required coverage for its changed PowerShell code. These are Major findings; remediation is required before another full-feature review.

### Policy-by-Policy Summary

- General Code Change Policy: FAIL — complete branch diff integrity fails.
- General Unit Test Policy: FAIL — changed PowerShell coverage is not demonstrated.
- C# policy and C# unit-test policy: PASS on the reviewed successor evidence.
- PowerShell policy: FAIL for coverage verification; format/analyze/test execution evidence is otherwise present.

### Metrics Summary

- C#: 84.5568% repository coverage; applicable named measured source ranges >=90%.
- PowerShell: Pester execution 30/30, but coverage attribution for the changed script is absent and aggregate artifact counters are 0/1364.
- Complete merge-base whitespace check: 2,764 diagnostics.

### Recommendation

**Needs revision.** Preserve the reviewed C# evidence, correct the complete-diff whitespace defects, and establish policy-compliant PowerShell coverage before a new full-feature review.

## Appendix A: Test Inventory

- P8-T82 direct all-eight-assembly VSTest run 1 and run 2.
- P9-T44 19 focused C# cases.
- P9-T57 all-eight-assembly coverage suite.
- Pester wrapper suite: 30 test cases.

## Appendix B: Toolchain Commands Reference

```powershell
git diff --check e63ddc7c18ca71e2c968b3329e42d965d45af1eb..83efd313c3f49b66d5f2e133467770284cca7253
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput <feature>/evidence/qa-gates/coverage.cobertura.xml
```

**Audit Completed By:** Codex feature reviewer  
**Policy Version:** Current as of 2026-07-27
