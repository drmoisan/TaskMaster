# Policy Audit: Issue 614

**Audit Timestamp:** 2026-08-27T03-52
**Review Head:** `eaf29fb1b1341a0217e5feb4759cd22fd1deb8d6`
**Base Branch:** `main`
**Merge Base:** `c279d40bddacdba00c29a9724d1b5b17f9ebbc90`
**Feature Folder:** `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614`

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New/Changed Code Coverage |
|---|---:|---:|---|---|---|---|
| C# | 11 production `.cs`, 19 test `.cs`, 6 project/package files | 6,587 | 6,587 pass, 0 fail | 84.7797% line; 78.6938% branch | 84.8796% line; 78.8657% branch | 100% line for all named changed production methods |
| TypeScript | 0 | N/A | N/A | N/A — out of scope | N/A — out of scope | N/A — out of scope |
| PowerShell | 0 | N/A | N/A | N/A — out of scope | N/A — out of scope | N/A — out of scope |

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: N/A — no TypeScript files changed.
- TypeScript post-change coverage artifact: N/A — no TypeScript files changed.
- PowerShell baseline coverage artifact: N/A — no PowerShell files changed.
- PowerShell post-change coverage artifact: N/A — no PowerShell files changed.
- Per-language comparison summary: C# baseline 84.7797% line and 78.6938% branch; exact-head 84.8796% line and 78.8657% branch; changed production methods 100% line coverage; PASS. TypeScript and PowerShell are not applicable because no files in those languages changed.

## Executive Summary

The complete Issue 614 feature diff and all three remediation cycles were reviewed against repository policy. Independent exact-head verification passed the required C# toolchain in order: CSharpier check, analyzer rebuild, nullable rebuild, and MSTest with coverage. All 6,587 tests passed. Exact-head coverage was 84.8796% line and 78.8657% branch, both above the merge-base baseline. Changed production methods were independently verified at 100% line coverage.

The audit records two documentation/evidence defects as explicitly accepted, nonblocking risks under `artifacts/orchestration/orchestrator-state.json` at `human_interaction.requirements[0]`. The approval is limited to those two artifacts. It does not waive code behavior, tests, coverage, CI, review findings, or strict validator failures. AC24 remains unchecked and partially met; `spec.md` was not modified. There are no blocking policy findings.

**Policy outcome:** PARTIALLY COMPLIANT — ACCEPTED DOCUMENTATION RISK; REVIEW PASS
**Blocking findings:** 0

## 1. General Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Independent, isolated, deterministic unit tests | PASS | Changed tests use injected delegates, Moq, and controlled values. The cycle-3 environment seam removes host OneDrive environment dependence. |
| MSTest framework | PASS | Changed C# tests use MSTest attributes and conventions. |
| No unapproved temporary-file creation | PASS | Review of the changed tests found no new runtime temporary-file dependency. |
| Positive, negative, boundary, and regression coverage | PASS | Regression coverage includes archive-root selection, hierarchy-only full paths, case variants, producer boundaries, redaction, and prior issue interactions. |
| Repository-wide line coverage >= 80% | PASS | Exact-head line coverage: 53,986 / 63,603 = 84.8796%. |
| New or changed methods target >= 90% | PASS | `ArchiveStemContract`, `EfcSelectionGuard`, `ArchiveRootPathGuard`, the changed `ApplicationGlobals` constructor, and the changed `LoadBasicMethod` executable lines are 100% covered. |
| Full test suite passes | PASS | 6,587 passed, 0 failed, across 9 test assemblies. |

## 2. General Code Change Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| Defect regression tests accompany the fix | PASS | The feature and remediation cycles include targeted regression tests for root selection, producer boundaries, case variance, prior issue compatibility, redaction, and deterministic initialization. |
| Minimal boundary-preserving implementation | PASS | The implementation separates hierarchy full-path derivation from archive-relative filing through focused guards/contracts. Cycle 3 adds an environment-reader seam without altering default production behavior. |
| Public contracts are explicit | PASS | New guard and contract types expose focused APIs; invalid state is rejected explicitly. |
| I/O and environment access are isolated | PASS | `ApplicationGlobals` injects a `Func<string, string>` into folder-path construction for tests while retaining the production default. |
| File-size policy | PASS | Review of changed production and test files found no newly introduced file over the repository 500-line limit. |
| No opportunistic unrelated production changes | PASS | Production changes are traceable to Issue 614 requirements and remediation findings. |
| Supporting evidence and plans maintained | PARTIAL — ACCEPTED RISK | Two preserved artifacts have approved documentation/evidence defects; details are in Section 8. |

## 3. Language-Specific Code Change Policy Compliance

### C#

| Requirement | Status | Evidence |
|---|---|---|
| CSharpier formatting | PASS | `dotnet tool run csharpier check .` checked 1,530 files and exited 0. |
| .NET analyzer enforcement | PASS | Analyzer rebuild succeeded with 0 errors. Five existing `System.Reactive` `packages.config` warnings remained; no new analyzer error was introduced. |
| Nullable/type-safety enforcement | PASS | Nullable rebuild with warnings-as-errors succeeded with 0 errors. The same five pre-existing package warnings were emitted. |
| Explicit null and invariant handling | PASS | Guards reject invalid or unavailable roots and preserve archive-relative contracts. |
| Focused classes and methods | PASS | New types have narrow responsibilities: stem normalization, selection validation, and archive-root protection. |
| Production behavior preserved by test seam | PASS | Existing constructors continue to supply the default environment reader; only tests supply a deterministic delegate. |

No changed Python, PowerShell, or TypeScript production implementation was identified in the feature diff, so their language-specific code-change gates are not applicable.

## 4. Language-Specific Unit Test Policy Compliance

| Requirement | Status | Evidence |
|---|---|---|
| MSTest attributes and project conventions | PASS | Changed test classes and methods follow the repository MSTest structure. |
| Moq for dependency isolation | PASS | Existing and changed tests use Moq where interaction isolation is required. |
| FluentAssertions preference | PASS | Changed assertions follow repository conventions; no new assertion framework was introduced. |
| Deterministic environment behavior | PASS | Cycle-3 test constructors inject a fixed OneDrive environment mapping instead of reading the executing host. |
| Full suite execution with coverage | PASS | Canonical runner completed all 9 assemblies with 6,587 / 6,587 passing. |

## 5. Test Coverage Detail

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 84.7797% line / 78.6938% branch. Post-change: 84.8796% line / 78.8657% branch. Change: +0.0999 percentage points line and +0.1719 percentage points branch. New/changed-code coverage: 100% line for the named changed production methods. Disposition: PASS. Evidence: `coverage/coverage.cobertura.xml` and `evidence/baseline/test-coverage.2026-08-26T11-36.md`.

| Language | Baseline | Post-change | Delta | New/changed code | Disposition |
|---|---|---|---|---|---|
| C# | 84.7797% line; 78.6938% branch | 84.8796% line; 78.8657% branch | +0.0999 pp line; +0.1719 pp branch | 100% line for named changed methods | PASS |
| TypeScript | N/A | N/A | N/A | N/A | N/A — no changed files |
| PowerShell | N/A | N/A | N/A | N/A | N/A — no changed files |

### Repository Coverage

| Measurement | Merge-base baseline | Exact-head independent run | Delta | Gate |
|---|---:|---:|---:|---|
| Line coverage | 53,769 / 63,422 = 84.7797% | 53,986 / 63,603 = 84.8796% | +0.0999 percentage points | PASS |
| Branch coverage | 12,676 / 16,108 = 78.6938% | 12,751 / 16,168 = 78.8657% | +0.1719 percentage points | PASS |

### Changed Production Methods and Types

| Target | Covered / Coverable lines | Result |
|---|---:|---|
| `ArchiveStemContract` | 51 / 51 | PASS — 100% |
| `EfcSelectionGuard` | 17 / 17 | PASS — 100% |
| `ArchiveRootPathGuard` | 20 / 20 | PASS — 100% |
| Changed `ApplicationGlobals` constructor | 18 / 18 | PASS — 100% |
| Changed `ApplicationGlobals.LoadBasicMethod` executable lines | 17 / 17 | PASS — 100% |

The independent exact-head coverage result differs slightly from the cycle-3 executor's recorded 84.8938% line and 78.8780% branch result. The current result remains above the original merge-base baseline and the cycle-3 pre-remediation baseline. This run-to-run covered-line variation is disclosed but is not a gate failure.

## 6. Test Execution Metrics

| Metric | Result |
|---|---:|
| Test assemblies | 9 |
| Total tests | 6,587 |
| Passed | 6,587 |
| Failed | 0 |
| Elapsed test time | 49.8862 seconds |
| Coverage artifact | `coverage/coverage.cobertura.xml` |

## 7. Code Quality Checks

| Check | Command | Result |
|---|---|---|
| Formatting | `dotnet tool run csharpier check .` | PASS — 1,530 files checked |
| Analyzer build | `MSBuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | PASS — 0 errors; 5 pre-existing package warnings |
| Nullable build | `MSBuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true` | PASS — 0 errors; 5 pre-existing package warnings |
| Tests and coverage | `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .` | PASS — 6,587 / 6,587 |

The commands were executed in the required formatter → analyzer → nullable → test order at exact head `eaf29fb1b1341a0217e5feb4759cd22fd1deb8d6`.

## 8. Gaps and Exceptions

### Accepted Risk 1: Mixed-expectation final test evidence

`evidence/qa-gates/final-test-coverage.2026-08-26T22-27.md` contains a preserved normalized `FAIL` caused by mixed expectation sources. The recorded file hash is `CCA698B1CFB2EDFF6B768C45749F7C08038033AFD705DD1CA863E945AD7F6D5D`. The user explicitly approved skipping this documentation/evidence defect. The file was not modified solely for this finding.

### Accepted Risk 2: Change-description issue cross-reference

`change-description.2026-08-26.md` preserves an incorrect `#637` reference where `#638` is intended. The recorded file hash is `679C6A759BCE3D5388986CCB77552925782DF60ADC39F2DAD939BC06C8D05943`. The user explicitly approved skipping this documentation defect. The file was not modified solely for this finding.

### AC24 remains unchecked

AC24 requires a direct `vstest.console.exe ... /EnableCodeCoverage` invocation and evidence under `<FEATURE>/evidence/qa/`. The repository's authoritative canonical runner executed successfully and stored evidence through the established `evidence/qa-gates` convention, but the literal AC wording is not fully satisfied. AC24 therefore remains unchecked and PARTIAL. `spec.md` remains unchanged with SHA-256 `E64AB58252595DF0B2BC86AA58E44F0B82955BB6525B356D734B3BD7E6A79AC5`.

The approval applies only to these documentation/evidence conditions. Any code behavior, test, coverage, CI, review, or strict-validator failure remains blocking.

## 9. Summary of Changes

The feature introduces explicit archive-stem and selection guards, prevents the EFC store-root selection path from leaking a full Outlook hierarchy into the filing boundary, preserves hierarchy display/navigation behavior, and supplies regression coverage for case variants and prior issue interactions. Remediation cycles addressed review and hosted-test findings, including deterministic injection of the OneDrive environment reader for `ApplicationGlobals` tests.

## 10. Compliance Verdict

**PARTIALLY COMPLIANT — ACCEPTED DOCUMENTATION RISK; REVIEW PASS**

All code, test, formatting, analyzer, nullable, and coverage gates independently passed at the exact review head. The only partial conditions are the two explicitly approved documentation/evidence defects and the resulting unchecked AC24. They are recorded as nonblocking accepted risk. Blocking finding count: **0**.

## Appendix A: Test Inventory

The complete exact-head suite comprised 9 assemblies and 6,587 tests. Feature-specific coverage includes archive stem normalization, EFC selection validation, archive-root protection, producer integration, case-variant paths, prior Issue 609/439/499 interactions, path redaction, and deterministic `ApplicationGlobals` initialization.

## Appendix B: Toolchain Commands Reference

```powershell
dotnet tool run csharpier check .

& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug '/p:Platform=Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug '/p:Platform=Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true

pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot .
```
