# Policy Compliance Audit: csharp-analyzer-stack-hardening (Issue #181)

**Audit Date:** 2026-06-08
**Audit Type:** Cycle-2 exit reaudit (post-remediation, post-CI-green)
**Code Under Test:** Branch `feature/csharp-analyzer-stack-181` @ `cdf9a45f961597e4a699e2f59933967fdf7236ff` vs base `main` @ `2a522ed831865c2918ab02df153ef2929b0617dc` (merge base identical, verified `git merge-base main HEAD`). Changed files: 15 first-party `*.csproj`, 15 first-party `packages.config`, repo-root `BannedSymbols.txt` (new), `.editorconfig` (new, +567), `.claude/rules/csharp.md`, one production `.cs` file (`UtilitiesCS/Extensions/IEnumerableExtensions.cs`, formatting-only, cycle-2 remediation), plus feature docs/evidence and `.claude/agent-memory` notes.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 31 build-config files + 1 production `.cs` (formatting-only) | 4064 collected | PASS on authoritative CI (run 27158840914, GREEN) | 58.89% lines (raw, all modules) | 58.99% lines (raw, all modules) | N/A — 0 production `.cs` LINES added/changed; the single `.cs` edit is whitespace-only (1 insert, 5 deletes collapsing a lambda) |

**Note:** C# is the only language with changed files. The raw line-coverage figure is collected over all instrumented modules (including vendored, COM/interop, and auto-generated code that the CI coverage configuration scopes out). It is the no-regression reference, not the policy-gate value; the authoritative repo-wide 80% and new-code 90% policy gates are evaluated by the PR GitHub Actions CI run (PR #182, run 27158840914), which is GREEN.

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - out of scope` (no TypeScript files changed)
- TypeScript post-change coverage artifact: `N/A - out of scope` (no TypeScript files changed)
- PowerShell baseline coverage artifact: `N/A - out of scope` (no PowerShell `.ps1`/`.psm1` files changed)
- PowerShell post-change coverage artifact: `N/A - out of scope` (no PowerShell `.ps1`/`.psm1` files changed)
- C# baseline coverage artifact: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/baseline/baseline-test-coverage.2026-06-08T12-12.md`
- C# post-change coverage artifact: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/final-test-coverage.2026-06-08T18-06.md` and canonical Cobertura `artifacts/csharp/coverage.xml` (line-rate 0.5899, lines-covered 101734 of lines-valid 172456)
- Per-language comparison summary: section 1.2.1 below; `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/coverage-delta.2026-06-08T18-06.md`

**Non-negotiable verdict rule:** This audit reports numeric baseline and post-change coverage for the only in-scope language (C#). The one modified production `.cs` file (`UtilitiesCS/Extensions/IEnumerableExtensions.cs`) is a formatting-only change (verified: the diff is a lambda-body line collapse, 1 insert / 5 deletes, no token/identifier/logic change); its class-level line coverage in the Cobertura artifact is 95.97% (119/124), above both the 80% modified-file floor and the 90% new-code target, with no regression. New/changed-code coverage as a distinct obligation is N/A because zero production `.cs` LINES of logic were added or changed.

---

## Rejected Scope Narrowing

The caller-supplied "Scope note (informational, not a narrowing instruction)" enumerated the branch diff contents and was explicitly labelled non-narrowing. It is recorded here for completeness and was NOT treated as a scope narrowing. The caller did not instruct the audit to limit to a plan/task/phase subset, to skip any toolchain or coverage check, or to mark any language with changed files as out of scope.

The PR-context summary artifact (`artifacts/pr_context.summary.txt`) continues to misclassify C# build-config changes (`.csproj`, `packages.config`, `.editorconfig`, `BannedSymbols.txt`) under a non-core-logic category. That classification was rejected for scope purposes: the audit scope is the full branch diff against `main` @ `2a522ed8`, which contains 31 C# build-configuration files, a new repo-root `BannedSymbols.txt`, a +567-line `.editorconfig`, and one formatting-only production `.cs` file. All are in scope and audited here as C# changes. No caller instruction marked any language's coverage as out of scope, plan-scope-only, informational-only, or not applicable; none was accepted as such.

---

## Evidence Location Compliance

Scan of the branch diff for files written under forbidden evidence paths (`artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`, `artifacts/evidence/`, `artifacts/coverage/`, `artifacts/regression-testing/`, `artifacts/post-change/`):

- Command: `git diff 2a522ed8..cdf9a45f --name-only | grep -E '^artifacts/(baselines|baseline|qa|qa-gates|evidence|coverage|regression-testing|post-change)/'`
- Result: no matches. No evidence-location violations in the branch diff.
- All committed feature evidence is under canonical `<FEATURE>/evidence/{baseline,issue-updates,other,qa-gates,regression-testing}/` subpaths (verified by enumerating the diff evidence subpaths).
- The canonical Cobertura coverage `artifacts/csharp/coverage.xml` is a working-tree artifact (allowed non-evidence orchestration location for the language coverage artifact per the feature-review coverage table); it is not committed in the branch diff.
- The repository's PowerShell `validate_evidence_locations.py` script is not present in this repo; the enforcement hook present is `.claude/hooks/enforce-evidence-locations.ps1`. The manual diff scan above substitutes for the script scan. **Status: PASS.**

---

## Executive Summary

This change adopts a fixed five-analyzer C# static-analysis stack (Meziantou.Analyzer 3.0.101, SonarAnalyzer.CSharp 10.27.0.140913, Roslynator.Analyzers 4.15.0, AsyncFixer 2.1.0, Microsoft.CodeAnalysis.BannedApiAnalyzers 3.3.4) wired into the 15 first-party legacy non-SDK / `packages.config` projects only. It adds a repo-root `BannedSymbols.txt` enforcing five time/random banned symbols (RS0030 at suggestion severity for initial rollout), a new `.editorconfig` carrying new analyzer severities (all at suggestion, plus naming and file-scoped-namespace preferences), and documents the mechanism, the SecurityCodeScan.VS2019 deferral (Roslyn 5.6 / CS8032 incompatibility), and a TimeProvider seam guidance section in `.claude/rules/csharp.md`. Cycle 2 added a single formatting-only edit to `UtilitiesCS/Extensions/IEnumerableExtensions.cs` to clear a pre-existing `main` CSharpier regression that was blocking the CI formatting gate; that edit carries no logic, behavior, or public-API change.

**Cycle-2 status change:** PR #182 required checks are GREEN at the branch head (`cdf9a45f`): run 27158840914 "Format, build, analyze, and test" = pass (6m1s), actionlint = pass, conclusion `success`. This satisfies AC6 (PR CI GREEN, including nullable-as-errors and MSTest-with-coverage) and corroborates AC5 (no nullable regression) in the authoritative environment. The cycle-1 single material gap (AC6 unverified) is now closed.

**Policy documents evaluated:**
- PASS `CLAUDE.md` / `general-code-change.md` (cross-language code change policy)
- PASS `general-unit-test.md` (cross-language unit test policy)

**Language-specific policies evaluated:**
- N/A `python` rules (no Python files changed)
- N/A `powershell` rules (no PowerShell files changed)
- N/A `typescript` rules (no TypeScript files changed)
- PASS C# Code Change Policy (CLAUDE.md C#1–C#7) and C# Unit Test Policy — applies to `*.csproj`/`*.props`/`*.targets`/build-config and the single formatting-only `.cs` edit; no `.cs` logic and no test changes occurred.

This change is build configuration, central editor configuration, banned-symbol policy, documentation, and one formatting-only `.cs` edit. No production `.cs` logic or test C# code was added or modified, so the unit-test policy sections are evaluated for impact (no test regression) rather than new-test authorship. The four-stage toolchain passes GREEN on the authoritative CI.

**Temporary artifacts cleanup:**
- PASS Phase 1 scratch files (`evidence_ids_*.txt`) were removed (recorded in `invariant-check.2026-06-08T12-12.md`).
- PASS No new ongoing tooling scripts were introduced by this change.

---

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Independence** - Tests run in any order | N/A PASS | No tests were added or modified. Existing MSTest suites were run unchanged and pass GREEN on CI run 27158840914. |
| **Isolation** - Each test targets single behavior | N/A PASS | No test code changed. |
| **Fast Execution** - Tests complete quickly | N/A PASS | No test code changed; existing suite of 4064 tests executed on CI. |
| **Determinism** - Consistent results | PASS | The previously-flaky wall-clock-timer test family passed on the authoritative GREEN CI run (27158840914, conclusion success). No nondeterministic failure remained at the branch head. |
| **Readability & Maintainability** - Clear structure | N/A PASS | No test code changed. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Baseline Coverage Documented** | PASS | Baseline 58.89% lines (101554 / 172456). Command: `vstest.console.exe <7 *.Test.dll> /EnableCodeCoverage /InIsolation /Logger:trx`. Source: `evidence/baseline/baseline-test-coverage.2026-06-08T12-12.md`. |
| **No Coverage Regression** | PASS | Post-change 58.99% lines (101734 / 172456). Change: +0.10 pp. No regression. Source: `evidence/qa-gates/coverage-delta.2026-06-08T18-06.md`. |
| **New Code Coverage >=90%** | PASS | The single modified production file `IEnumerableExtensions.cs` is a formatting-only change; its class-level line coverage is 95.97% (119/124) in `artifacts/csharp/coverage.xml`, above the 90% target. No new logic lines were added. |
| **Comprehensive Coverage** | N/A | No new functions/classes added. |
| **Positive Flows** | N/A | No test code changed. |
| **Negative Flows** | N/A | No test code changed. |
| **Edge Cases** | N/A | No test code changed. |
| **Error Handling** | N/A | No test code changed. |
| **Concurrency** | N/A | No test code changed. |
| **State Transitions** | N/A | No test code changed. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 58.89% lines. Post-change: 58.99% lines. Change: +0.10% lines. New/changed-code coverage: 95.97% (modified file `IEnumerableExtensions.cs`, 119/124 lines; formatting-only edit, no logic delta). Disposition: PASS. Evidence: `evidence/baseline/baseline-test-coverage.2026-06-08T12-12.md`, `evidence/qa-gates/final-test-coverage.2026-06-08T18-06.md`, `evidence/qa-gates/coverage-delta.2026-06-08T18-06.md`, `artifacts/csharp/coverage.xml`.
- Python: Baseline: N/A - out of scope. Post-change: N/A - out of scope. Change: N/A. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (no Python files in branch diff).
- PowerShell: Baseline: N/A - out of scope. Post-change: N/A - out of scope. Change: N/A. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (no PowerShell files in branch diff).
- TypeScript: Baseline: N/A - out of scope. Post-change: N/A - out of scope. Change: N/A. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (no TypeScript files in branch diff).

Repo-wide raw line coverage (58.99%) is below the 80% policy threshold, but this raw figure spans all instrumented modules including vendored/COM/interop/auto-generated assemblies that the CI coverage configuration scopes out. The authoritative repo-wide 80% gate is the PR GitHub Actions CI run, which is GREEN at the branch head (PR #182, run 27158840914). Because this change adds zero production `.cs` logic lines, it cannot lower scoped coverage; the no-regression check (post-change >= baseline) holds.

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clear Failure Messages** | N/A PASS | No test code changed. |
| **Arrange-Act-Assert Pattern** | N/A PASS | No test code changed. |
| **Document Intent** | N/A PASS | No test code changed. |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Avoid External Dependencies** | N/A PASS | No test code changed. |
| **Use Mocks/Stubs** | N/A PASS | No test code changed. |
| **Environment Stability** | N/A PASS | No test code changed; no temporary files introduced. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Pre-submission Review** | PASS | This audit plus `evidence/qa-gates/acceptance-summary.2026-06-08T18-06.md`, `evidence/qa-gates/ci-green.2026-06-08T18-06.md`, and `evidence/other/invariant-check.2026-06-08T12-12.md` constitute the pre-submission review. |

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clarify the objective** | PASS | Issue #181; `issue.md`, `spec.md`, `user-story.md` document the objective (adapt the hardened analyzer stack to the legacy build). |
| **Read existing change plans** | PASS | `plan.2026-06-08T12-12.md` and `remediation-plan.2026-06-08T18-06.md` are present and were followed; Phase 0 read policy files (`evidence/baseline/phase0-instructions-read.2026-06-08T18-06.md`). |
| **Document the plan** | PASS | `plan.2026-06-08T12-12.md` documents phases P0–P6; `remediation-plan.2026-06-08T18-06.md` documents the single-file formatting fix. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Simplicity first** | PASS | File-based `<Analyzer Include>` wiring chosen because projects are legacy non-SDK `packages.config`; no Central Package Management, no PackageReference migration, no `dotnet restore` introduced (`.claude/rules/csharp.md` Mechanism section). Cycle-2 fix applies CSharpier output verbatim (no hand-formatting). |
| **Reusability** | PASS | Single repo-root `BannedSymbols.txt` referenced by every first-party project via `$(MSBuildThisFileDirectory)..\BannedSymbols.txt`; severities centralized in one `.editorconfig`. |
| **Extensibility** | PASS | Severity-first invariant documented so future analyzer promotions are controlled centrally in `.editorconfig`. |
| **Separation of concerns** | PASS | Build-config, severity policy, banned-symbol policy, and documentation are each in their dedicated files. |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Cohesive modules** | PASS | Each changed file has a single concern (analyzer wiring per project, central severities, banned symbols, rules doc, one formatting fix). |
| **Under 500 lines** | INFO | `.editorconfig` is 567 lines; configuration/markdown are not production/test/script source code per the file-size limit and are exempt. `IEnumerableExtensions.cs` net line count decreased by 4. No source code file newly exceeds 500 lines. |
| **Public vs internal** | N/A | No code API surface changed. |
| **No circular dependencies** | PASS | Analyzer packages are developmentDependency only; no project-reference graph changed. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Descriptive names** | PASS | `.csproj` analyzer `<ItemGroup>` carries an explanatory comment referencing Issue #181 and the suggestion-severity rationale. |
| **Docs/docstrings** | PASS | `.claude/rules/csharp.md` documents the analyzer stack, mechanism, severity-first invariant, deferral, and time-seam guidance. |
| **Comment why, not what** | PASS | The `.csproj` comment and the `.editorconfig` comments explain the protected-nullable-gate rationale for suggestion severity. |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| **1. Formatting** | PASS | Command: `dotnet tool run csharpier check .`. Result on CI: pass (the cycle-2 fix to `IEnumerableExtensions.cs` cleared the last finding). Source: `evidence/regression-testing/format-check-after-fix.2026-06-08T18-06.md`, `evidence/qa-gates/final-format.2026-06-08T18-06.md`, CI run 27158840914. |
| **2. Linting** | PASS | Command: `msbuild TaskMaster.sln -t:Rebuild ... -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true`. Result: EXIT 0, 0 errors. Source: `evidence/qa-gates/final-analyzer-build.2026-06-08T18-06.md`; corroborated GREEN on CI. |
| **3. Type checking** | PASS | Command: `msbuild TaskMaster.sln -t:Rebuild ... -p:Nullable=enable -p:TreatWarningsAsErrors=true`. Result on CI: pass; 0 first-party errors, 0 CS8032, no regression. Source: `evidence/qa-gates/final-nullable-build.2026-06-08T18-06.md`; CI run 27158840914. |
| **4. Testing** | PASS | Command: `vstest.console.exe <7 *.Test.dll> /EnableCodeCoverage`. Result on CI: pass, coverage collected. Source: `evidence/qa-gates/final-test-coverage.2026-06-08T18-06.md`; CI run 27158840914. |
| **Full toolchain loop** | PASS | All four stages pass GREEN in a single authoritative CI pass (run 27158840914, conclusion success). The cycle-1 limitation (local baseline non-green CSharpier finding) was resolved by the cycle-2 formatting fix. |
| **Explicit reporting** | PASS | All commands and results recorded in feature evidence and CI run; re-stated here. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Summarize changes** | PASS | `acceptance-summary.2026-06-08T18-06.md`, `plan.2026-06-08T12-12.md`, and `remediation-plan.2026-06-08T18-06.md` summarize the change. |
| **Design choices explained** | PASS | `.claude/rules/csharp.md` records the file-based wiring choice, severity-first invariant, and SecurityCodeScan deferral rationale. |
| **Update supporting documents** | PASS | `.claude/rules/csharp.md` updated; `issue.md` AC checked off; `evidence/issue-updates/issue-181.2026-06-08T12-12.md` and `pr-181.2026-06-08T13-50.md` mirrors present. |
| **Provide next steps** | PASS | Documented follow-ups: RS0030 promotion to warning after legacy cleanup; SecurityCodeScan re-evaluation with a Roslyn-5.x-compatible analyzer; legacy banned-symbol call-site migration. |

---

## 3. Language-Specific Code Change Policy Compliance

### Section 3C# : C# Code Change Policy Compliance

#### C#1 Tooling & Baseline

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Formatting with CSharpier** | PASS | `dotnet tool run csharpier check .` passes on CI after the cycle-2 fix to the single unformatted file; CSharpier (not `dotnet format`) was used, preserving `.csproj` semantics. Source: `format-check-after-fix.2026-06-08T18-06.md`, CI run 27158840914. |
| **Linting / .NET analyzers** | PASS | Analyzer/code-style build EXIT 0, 0 errors; new analyzers active at suggestion severity. Source: `final-analyzer-build.2026-06-08T18-06.md`. |
| **Type checking / nullable** | PASS | Nullable `TreatWarningsAsErrors` build passes on CI; 0 first-party errors; 0 CS8032; protected gate not regressed. Source: `final-nullable-build.2026-06-08T18-06.md`, CI run 27158840914. |
| **CPM not introduced** | PASS | No `Directory.Packages.props`; no PackageReference migration; `packages.config` style retained. Source: `invariant-check.2026-06-08T12-12.md` Invariant 2. |
| **Restore via nuget restore** | PASS | `nuget.exe restore TaskMaster.sln` EXIT 0 with the 5 new analyzer packages. Source: `final-restore.2026-06-08T18-06.md`. |

#### C#2–C#7 Design, Structure, Dependencies

| Requirement | Status | Evidence |
|------------|--------|----------|
| **First-party scope; vendored excluded** | PASS | 15 first-party projects wired; SVGControl/UtilitiesSwordfish untouched (`git diff` shows no vendored files; `invariant-check` Invariant 7). 135 `<Analyzer Include>` lines + 15 `<AdditionalFiles BannedSymbols.txt>`. |
| **Severity-first invariant** | PASS | All new analyzer diagnostics at `dotnet_analyzer_diagnostic.severity = suggestion`; RS0030 at suggestion; the single `severity = warning` line preserves the pre-existing baseline MSTEST0032 warning (documented). |
| **No unauthorized suppression (CS8032)** | PASS | No `dotnet_diagnostic.CS8032` entry, no `<WarningsNotAsErrors>` containing CS8032 anywhere. Source: `invariant-check` Invariant 6. |
| **Dependencies analyzer-only** | PASS | All 5 packages added with `developmentDependency="true"`; no runtime dependency added (Microsoft.Bcl.TimeProvider already present). |
| **Formatting-only `.cs` edit** | PASS | `UtilitiesCS/Extensions/IEnumerableExtensions.cs` diff is a single lambda-body line collapse (1 insert / 5 deletes); no token/identifier/control-flow change; class coverage 95.97% with no regression. |

---

## 4. Language-Specific Unit Test Policy Compliance

No C# test code was added or modified by this change. The existing MSTest/Moq/FluentAssertions suites were executed unchanged and pass GREEN on the authoritative CI (run 27158840914). The C# Unit Test Policy framework selection (MSTest), mocking (Moq), and assertion (FluentAssertions) conventions are retained and documented as unchanged in `.claude/rules/csharp.md` (`invariant-check` Invariant 3). No new tests were required because no production code logic paths were added (the single `.cs` edit is formatting-only).

---

## 5. Test Coverage Detail

The only modified production file is `UtilitiesCS/Extensions/IEnumerableExtensions.cs` (formatting-only). Its main class line coverage in `artifacts/csharp/coverage.xml` is 95.97% (119/124). The changed region (the `System.Threading.Timer` lambda in `WithProgressReporting`) is in a 100%-covered state machine. No new or modified functions, classes, or modules introduced new logic; per-component coverage tables are otherwise omitted. The no-regression coverage comparison is in section 1.2 and `evidence/qa-gates/coverage-delta.2026-06-08T18-06.md`.

---

## 6. Test Execution Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Tests | 4064 | collected |
| Tests Passed | 4064 on authoritative CI run 27158840914 | PASS |
| Tests Failed | 0 on the GREEN CI run | PASS |
| Skipped | 2 | PASS |
| Code Coverage (raw, all modules) | 58.99% lines | raw figure; authoritative scoped gate is CI (GREEN) |

---

## 7. Code Quality Checks

**For C#:**

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| CSharpier Formatting | `dotnet tool run csharpier check .` | pass on CI (cycle-2 fix cleared the last finding) | PASS |
| .NET Analyzers / Code Style | `msbuild TaskMaster.sln -t:Rebuild ... -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true` | EXIT 0, 0 errors | PASS |
| Nullable Type Check | `msbuild TaskMaster.sln -t:Rebuild ... -p:Nullable=enable -p:TreatWarningsAsErrors=true` | pass on CI; 0 first-party errors, 0 CS8032, no regression | PASS |
| MSTest with Coverage | `vstest.console.exe <7 *.Test.dll> /EnableCodeCoverage` | pass on CI; coverage collected | PASS |

**Notes:** All four stages pass GREEN in a single authoritative CI pass (PR #182, run 27158840914, conclusion success). The cycle-1 non-green local CSharpier finding (a pre-existing `main` regression on `IEnumerableExtensions.cs`, unrelated to the analyzer stack) was resolved by the cycle-2 formatting-only fix.

---

## 8. Gaps and Exceptions

### Identified Gaps
- **None blocking.** The cycle-1 gap (AC6 PR CI GREEN unverified) is closed: PR #182 run 27158840914 is GREEN at the branch head (`cdf9a45f`), verified independently via `gh pr checks 182` (both required checks pass) and `gh run view 27158840914` (conclusion success, headSha cdf9a45f).

### Approved Exceptions
- **`.editorconfig` 567 lines:** Configuration file, not production/test/script source code; exempt from the 500-line source limit. The size is the generated analyzer severity map documented in `evidence/other/editorconfig-severity-map.2026-06-08T12-12.md`.
- **SecurityCodeScan.VS2019 deferral:** Documented, not silent. Version 5.6.7 emits CS8032 under Roslyn 5.6, which cannot be neutralized via `.editorconfig` and would break the protected nullable gate. The package was dropped entirely; no CS8032 suppression and no substitute were introduced. Recorded in `.claude/rules/csharp.md`. This is an authorized adaptation under the issue mandate "adapted so it builds cleanly with zero new build/CI failures."

### Removed/Skipped Tests
- **None.** No tests were removed or skipped.

---

## 9. Summary of Changes

### Files Modified (functional scope)

1. **`BannedSymbols.txt`** (NEW) — 5 banned time/random symbols (DateTime.Now, DateTime.UtcNow, Random.Shared, Thread.Sleep, Task.Delay overloads) with remediation messages.
2. **`.editorconfig`** (NEW, +567) — global analyzer default at suggestion; per-rule severities; file-scoped-namespace preference; naming rules; RS0030 at suggestion; preserves baseline MSTEST0032 at warning.
3. **15 first-party `*.csproj`** (MODIFIED) — analyzer `<ItemGroup>` with 9 `<Analyzer Include>` DLL items plus `<AdditionalFiles ..\BannedSymbols.txt>`.
4. **15 first-party `packages.config`** (MODIFIED) — 5 analyzer packages each as `developmentDependency="true"`.
5. **`.claude/rules/csharp.md`** (MODIFIED) — TimeProvider seam guidance; Analyzer Stack section; mechanism; severity-first invariant; SecurityCodeScan deferral note.
6. **`UtilitiesCS/Extensions/IEnumerableExtensions.cs`** (MODIFIED, cycle-2, formatting-only) — CSharpier collapse of a Timer-lambda body onto one line; no logic change.

No production `.cs` logic or test `.cs` source files were modified.

---

## 10. Compliance Verdict

### Overall Status: COMPLIANT

The implementation is well-scoped, fully documented, and introduces no `.cs` logic changes (the single `.cs` edit is formatting-only). The analyzer stack, banned-symbol policy, central severities, and rules documentation all satisfy their acceptance criteria with concrete evidence. All four toolchain stages pass GREEN on the authoritative CI (PR #182, run 27158840914), closing the cycle-1 AC6 gap. The repo-wide 80% coverage gate, the nullable-as-errors step, and the MSTest-with-coverage step all pass on CI.

**Blocking findings (FAIL + blocking PARTIAL): 0.**

---

### Policy-by-Policy Summary

#### General Code Change Policy (Section 2)
- PASS Before Making Changes: plan, remediation plan, and policy reads documented.
- PASS Design Principles: simplest viable file-based wiring; centralized config; CSharpier output applied verbatim.
- PASS Module & File Structure: cohesive; `.editorconfig` size exempt as config.
- PASS Naming, Docs, Comments: rationale comments present.
- PASS Toolchain Execution: all four stages GREEN in a single CI pass.
- PASS Summarize & Document: complete.

#### Language-Specific Code Change Policy (Section 3)

**For C#:**
- PASS Tooling & Baseline: format/analyzer/nullable/test all GREEN on CI.
- PASS Design & Type-Safety: severity-first, no suppression, vendored excluded, formatting-only `.cs` edit.
- PASS Dependencies: analyzer-only developmentDependency packages.

#### General Unit Test Policy (Section 1)
- N/A Core Principles: no test code changed; existing suite GREEN on CI.
- PASS Coverage & Scenarios: no regression; modified-file coverage 95.97%.
- N/A Test Structure: no test code changed.
- N/A External Dependencies: no test code changed.
- PASS Policy Audit: this document plus invariant/acceptance/CI-green evidence.

#### Language-Specific Unit Test Policy (Section 4)
**For C#:**
- PASS Framework & Scope: MSTest/Moq/FluentAssertions retained, unchanged; GREEN on CI.

---

### Metrics Summary

- PASS 4064/4064 tests passing on authoritative CI run 27158840914 (0 failures).
- PASS Analyzer/code-style build: 0 errors.
- PASS Nullable gate: 0 first-party errors; no regression; 0 CS8032 (GREEN on CI).
- PASS Coverage no-regression: 58.89% -> 58.99% raw (+0.10 pp); modified-file coverage 95.97%.
- PASS Evidence-location compliance: no forbidden paths in diff.
- PASS AC6: PR #182 CI GREEN at branch head (run 27158840914, conclusion success).

---

### Recommendation

**Go.** All acceptance criteria are satisfied. PR #182 required checks (Format/build/analyze/test and actionlint) are GREEN at the branch head `cdf9a45f`, satisfying AC6 and corroborating AC5 on the authoritative environment. The change is technically complete, scoped, non-regressing, and ready for merge. Zero blocking findings remain.

---

## Appendix A: Test Inventory

No tests were added or modified by this change. The existing first-party MSTest suites executed for the no-regression check are: QuickFiler.Test, Tags.Test, TaskMaster.Test, TaskVisualization.Test, ToDoModel.Test, UtilitiesCS.Test, VBFunctions.Test (4064 tests total). Full per-test enumeration is out of scope for a build-config + formatting-only change; the executed assembly list and pass/fail counts are recorded in `evidence/qa-gates/final-test-coverage.2026-06-08T18-06.md` and the GREEN CI run 27158840914.

---

## Appendix B: Toolchain Commands Reference

**For C# (commands run by the implementer and on CI, re-cited here):**
```powershell
# Formatting
dotnet tool restore
dotnet tool run csharpier check .

# Restore (CI parity — nuget, not dotnet)
nuget.exe restore TaskMaster.sln

# Linting / .NET analyzers + code style
msbuild TaskMaster.sln -t:Rebuild -p:Configuration=Debug "-p:Platform=Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true

# Type checking / nullable (protected gate)
msbuild TaskMaster.sln -t:Rebuild -p:Configuration=Debug "-p:Platform=Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true

# Testing with coverage
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll Tags.Test\bin\Debug\Tags.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll ToDoModel.Test\bin\Debug\ToDoModel.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll /EnableCodeCoverage /InIsolation /Logger:trx
```

**Reviewer scope/diff/CI verification commands:**
```bash
git rev-parse HEAD                                            # cdf9a45f961597e4a699e2f59933967fdf7236ff
git merge-base main HEAD                                      # 2a522ed8... (identical to base)
git diff 2a522ed8..cdf9a45f --name-only -- "*.cs"            # UtilitiesCS/Extensions/IEnumerableExtensions.cs (formatting-only)
git diff 2a522ed8..cdf9a45f -- UtilitiesCS/Extensions/IEnumerableExtensions.cs   # 1 insert / 5 deletes, lambda collapse
gh pr checks 182                                              # both required checks pass
gh run view 27158840914 --json conclusion,headSha            # success, cdf9a45f
```

---

**Audit Completed By:** feature-reviewer agent
**Audit Date:** 2026-06-08 (cycle-2 exit reaudit)
**Policy Version:** Current (as of audit date)
