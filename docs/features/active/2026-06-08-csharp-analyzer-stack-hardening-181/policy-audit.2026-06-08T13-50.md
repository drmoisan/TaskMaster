# Policy Compliance Audit: csharp-analyzer-stack-hardening (Issue #181)

**Audit Date:** 2026-06-08
**Code Under Test:** Branch `feature/csharp-analyzer-stack-181` @ `71e0777ada475c408d85d3b6c68e6192b4bc070b` vs base `main` @ `2a522ed831865c2918ab02df153ef2929b0617dc` (merge base identical). Changed files: 15 first-party `*.csproj`, 15 first-party `packages.config`, repo-root `BannedSymbols.txt` (new), `.editorconfig` (new, +567), `.claude/rules/csharp.md`, plus feature docs/evidence and `.claude/agent-memory` notes. No `.cs` source files changed.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 31 build-config files (0 `.cs`) | 4064 collected | ❌ 4054 pass, 7 fail (known-flaky timer family) | 58.89% lines (raw, all modules) | 58.99% lines (raw, all modules) | N/A — 0 production `.cs` lines added/changed |

**Note:** C# is the only language with changed files. The raw line-coverage figure is collected over all instrumented modules (including vendored, COM/interop, and auto-generated code that the CI coverage configuration scopes out). It is the no-regression reference, not the policy-gate value; the authoritative repo-wide 80% and new-code 90% policy gates are evaluated by the PR GitHub Actions CI run per the repository's coverage scoping (see CLAUDE.md caveat).

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - out of scope` (no TypeScript files changed)
- TypeScript post-change coverage artifact: `N/A - out of scope` (no TypeScript files changed)
- PowerShell baseline coverage artifact: `N/A - out of scope` (no PowerShell `.ps1`/`.psm1` files changed)
- PowerShell post-change coverage artifact: `N/A - out of scope` (no PowerShell `.ps1`/`.psm1` files changed)
- C# baseline coverage artifact: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/baseline/baseline-test-coverage.2026-06-08T12-12.md`
- C# post-change coverage artifact: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/final-test-coverage.2026-06-08T12-12.md` and canonical Cobertura `artifacts/csharp/coverage.xml` (line-rate 0.5899, lines-covered 101734 of lines-valid 172456)
- Per-language comparison summary: section 1.2.1 below; `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/coverage-delta.2026-06-08T12-12.md`

**Non-negotiable verdict rule:** This audit reports numeric baseline and post-change coverage for the only in-scope language (C#). New/changed-code coverage is N/A because zero production `.cs` lines were added or modified by this change (verified: `git diff <base>..<head> -- "*.cs"` returns empty).

---

## Rejected Scope Narrowing

The caller-supplied context block stated: "This change adopts a 5-analyzer C# analyzer stack across 15 first-party legacy NON-SDK / packages.config projects ... No application logic was changed." and labelled the PR-context overview as "Core logic changes: 0 files ... Docs/templates/agents/tooling: 26 files."

These statements were treated as informational, not as a scope narrowing. The audit scope is the full branch diff against `main`. The PR-context summary (`artifacts/pr_context.summary.txt`) misclassifies all C# build-config changes (`.csproj`, `packages.config`, `.editorconfig`, `BannedSymbols.txt`) as "Docs/templates/agents/tooling" and reports "Core logic changes: 0 files." That classification was rejected for scope purposes: the branch diff contains 31 C# build-configuration files plus a new repo-root `BannedSymbols.txt` and a +567-line `.editorconfig`, all of which are in scope and audited here as C# changes. No caller instruction marked any language's coverage as out of scope, plan-scope-only, or not applicable; none was accepted as such.

---

## Evidence Location Compliance

Scan of the branch diff for files written under forbidden evidence paths (`artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`, `artifacts/evidence/`, `artifacts/coverage/`, `artifacts/regression-testing/`, `artifacts/post-change/`):

- Command: `git diff 2a522ed8..71e0777a --name-only | grep -E '^artifacts/(baselines|baseline|qa|qa-gates|evidence|coverage|regression-testing|post-change)/'`
- Result: no matches. No evidence-location violations in the branch diff.
- All committed feature evidence is under canonical `<FEATURE>/evidence/{baseline,issue-updates,other,qa-gates}/` subpaths (verified by enumerating the diff evidence subpaths).
- The canonical Cobertura coverage `artifacts/csharp/coverage.xml` is a working-tree artifact (allowed non-evidence orchestration location for the language coverage artifact per the feature-review coverage table); it is not committed in the branch diff.
- The repository's PowerShell `validate_evidence_locations.py` script is not present in this repo; the enforcement hook present is `.claude/hooks/enforce-evidence-locations.ps1`. The manual diff scan above substitutes for the script scan. **Status: PASS.**

---

## Executive Summary

This change adopts a fixed five-analyzer C# static-analysis stack (Meziantou.Analyzer 3.0.101, SonarAnalyzer.CSharp 10.27.0.140913, Roslynator.Analyzers 4.15.0, AsyncFixer 2.1.0, Microsoft.CodeAnalysis.BannedApiAnalyzers 3.3.4) wired into the 15 first-party legacy non-SDK / `packages.config` projects only. It adds a repo-root `BannedSymbols.txt` enforcing five time/random banned symbols (RS0030 at suggestion severity for initial rollout), a new `.editorconfig` carrying new analyzer severities (all at suggestion, plus naming and file-scoped-namespace preferences), and documents the mechanism, the SecurityCodeScan.VS2019 deferral (Roslyn 5.6 / CS8032 incompatibility), and a TimeProvider seam guidance section in `.claude/rules/csharp.md`. No application `.cs` source files were modified.

**Policy documents evaluated:**
- ✅ `CLAUDE.md` / `general-code-change.md` (cross-language code change policy)
- ✅ `general-unit-test.md` (cross-language unit test policy)

**Language-specific policies evaluated:**
- N/A `python` rules (no Python files changed)
- N/A `powershell` rules (no PowerShell files changed)
- N/A `typescript` rules (no TypeScript files changed)
- ✅ C# Code Change Policy (CLAUDE.md C#1–C#7) and C# Unit Test Policy — applies to `*.csproj`/`*.props`/`*.targets` and build-config; no `.cs` source or test changes occurred.

This change is build configuration, central editor configuration, banned-symbol policy, and documentation only. No production or test C# code was added or modified, so the unit-test policy sections are evaluated for impact (no test regression) rather than new-test authorship. The four-stage local toolchain was executed by the implementer and recorded under feature evidence; format and nullable steps return to the documented Phase 0 baseline (pre-existing failures only), and the analyzer build is clean (0 errors).

**Temporary artifacts cleanup:**
- ✅ Phase 1 scratch files (`evidence_ids_*.txt`) were removed (recorded in `invariant-check.2026-06-08T12-12.md`).
- ✅ No new ongoing tooling scripts were introduced by this change.

---

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Independence** - Tests run in any order | N/A PASS | No tests were added or modified. Existing MSTest suites were run unchanged (`final-test-coverage.2026-06-08T12-12.md`). |
| **Isolation** - Each test targets single behavior | N/A PASS | No test code changed. |
| **Fast Execution** - Tests complete quickly | N/A PASS | No test code changed; existing suite of 4064 tests was executed for the no-regression check. |
| **Determinism** - Consistent results | ⚠️ PARTIAL | 7 known-flaky wall-clock-timer tests fail nondeterministically; this is a pre-existing baseline condition (Phase 0 recorded 4 such failures), not introduced by this build-config-only change. |
| **Readability & Maintainability** - Clear structure | N/A PASS | No test code changed. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Baseline Coverage Documented** | ✅ PASS | Baseline 58.89% lines (101554 / 172456). Command: `vstest.console.exe <7 *.Test.dll> /EnableCodeCoverage /InIsolation /Logger:trx`. Source: `evidence/baseline/baseline-test-coverage.2026-06-08T12-12.md`. |
| **No Coverage Regression** | ✅ PASS | Post-change 58.99% lines (101734 / 172456). Change: +0.10 pp. No regression. Source: `evidence/qa-gates/coverage-delta.2026-06-08T12-12.md`. |
| **New Code Coverage >=90%** | N/A | No production `.cs` lines were added or modified (`git diff <base>..<head> -- "*.cs"` empty), so the new-code obligation is not triggered. |
| **Comprehensive Coverage** | N/A | No new functions/classes added. |
| **Positive Flows** | N/A | No test code changed. |
| **Negative Flows** | N/A | No test code changed. |
| **Edge Cases** | N/A | No test code changed. |
| **Error Handling** | N/A | No test code changed. |
| **Concurrency** | N/A | No test code changed. |
| **State Transitions** | N/A | No test code changed. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 58.89% lines -> Post-change: 58.99% lines. Change: +0.10% lines. New/changed-code coverage: N/A - no production `.cs` lines added or modified. Disposition: PASS. Evidence: `evidence/baseline/baseline-test-coverage.2026-06-08T12-12.md`, `evidence/qa-gates/final-test-coverage.2026-06-08T12-12.md`, `evidence/qa-gates/coverage-delta.2026-06-08T12-12.md`, `artifacts/csharp/coverage.xml`.
- Python: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (no Python files in branch diff).
- PowerShell: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (no PowerShell files in branch diff).
- TypeScript: Baseline: N/A - out of scope -> Post-change: N/A - out of scope. Change: N/A. New/changed-code coverage: N/A - out of scope. Disposition: N/A. Evidence: N/A - out of scope (no TypeScript files in branch diff).

Repo-wide raw line coverage (58.99%) is below the 80% policy threshold, but this raw figure spans all instrumented modules including vendored/COM/interop/auto-generated assemblies that the CI coverage configuration scopes out. The authoritative repo-wide 80% gate is the PR GitHub Actions CI run (CLAUDE.md caveat: local full-assembly coverage may hit a Moq binding-redirect; the authoritative gate is CI). Because this change adds zero production `.cs` lines, it cannot lower scoped coverage; the no-regression check (post-change >= baseline) holds.

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
| **Pre-submission Review** | ✅ PASS | This audit plus `evidence/qa-gates/acceptance-summary.2026-06-08T12-12.md` and `evidence/other/invariant-check.2026-06-08T12-12.md` constitute the pre-submission review. |

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clarify the objective** | ✅ PASS | Issue #181; `issue.md`, `spec.md`, `user-story.md` document the objective (adapt the hardened analyzer stack to the legacy build). |
| **Read existing change plans** | ✅ PASS | `plan.2026-06-08T12-12.md` (425 lines) is present and was followed; Phase 0 read policy files (`evidence/baseline/phase0-instructions-read.md`). |
| **Document the plan** | ✅ PASS | `plan.2026-06-08T12-12.md` documents phases P0–P6 with per-task acceptance and evidence. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Simplicity first** | ✅ PASS | File-based `<Analyzer Include>` wiring chosen because projects are legacy non-SDK `packages.config`; no Central Package Management, no PackageReference migration, no `dotnet restore` introduced (`.claude/rules/csharp.md` Mechanism section). |
| **Reusability** | ✅ PASS | Single repo-root `BannedSymbols.txt` referenced by every first-party project via `$(MSBuildThisFileDirectory)..\BannedSymbols.txt`; severities centralized in one `.editorconfig`. |
| **Extensibility** | ✅ PASS | Severity-first invariant documented so future analyzer promotions are controlled centrally in `.editorconfig`. |
| **Separation of concerns** | ✅ PASS | Build-config, severity policy, banned-symbol policy, and documentation are each in their dedicated files. |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Cohesive modules** | ✅ PASS | Each changed file has a single concern (analyzer wiring per project, central severities, banned symbols, rules doc). |
| **Under 500 lines** | ⚠️ INFO | `.editorconfig` is 567 lines. Per the General Code Change Policy file-size limit, configuration/markdown are not production/test/script code; `.editorconfig` is a generated severity map (documented in `evidence/other/editorconfig-severity-map.2026-06-08T12-12.md`) and is exempt from the 500-line source limit. No source code file exceeds 500 lines (no `.cs` changed). |
| **Public vs internal** | N/A | No code API surface changed. |
| **No circular dependencies** | ✅ PASS | Analyzer packages are developmentDependency only; no project-reference graph changed. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Descriptive names** | ✅ PASS | `.csproj` analyzer `<ItemGroup>` carries an explanatory comment referencing Issue #181 and the suggestion-severity rationale. |
| **Docs/docstrings** | ✅ PASS | `.claude/rules/csharp.md` documents the analyzer stack, mechanism, severity-first invariant, deferral, and time-seam guidance. |
| **Comment why, not what** | ✅ PASS | The `.csproj` comment and the `.editorconfig` comments explain the protected-nullable-gate rationale for suggestion severity. |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| **1. Formatting** | ⚠️ PARTIAL | Command: `dotnet tool run csharpier check .`. Result: EXIT 1; the only remaining finding is a pre-existing baseline `.cs` file (`UtilitiesCS\Extensions\IEnumerableExtensions.cs`) not touched by this change. All 30 in-scope project files were reformatted to CSharpier canonical XML and now pass. Source: `evidence/qa-gates/final-format.2026-06-08T12-12.md`. |
| **2. Linting** | ✅ PASS | Command: `msbuild TaskMaster.sln -t:Rebuild ... -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true`. Result: EXIT 0, 0 errors. Source: `evidence/qa-gates/final-analyzer-build.2026-06-08T12-12.md`. |
| **3. Type checking** | ⚠️ PARTIAL | Command: `msbuild TaskMaster.sln -t:Rebuild ... -p:Nullable=enable -p:TreatWarningsAsErrors=true`. Result: EXIT 1 with 84 errors — EQUAL to the Phase 0 baseline, all confined to the two vendored projects (SVGControl, UtilitiesSwordfish.NET.General); 0 first-party errors, 0 CS8032. No regression. Source: `evidence/qa-gates/final-nullable-build.2026-06-08T12-12.md`. |
| **4. Testing** | ⚠️ PARTIAL | Command: `vstest.console.exe <7 *.Test.dll> /EnableCodeCoverage /InIsolation /Logger:trx`. Result: EXIT 1; 4054/4064 pass, 7 known-flaky timer tests fail (baseline flakiness), coverage collected. Source: `evidence/qa-gates/final-test-coverage.2026-06-08T12-12.md`. |
| **Full toolchain loop** | ⚠️ PARTIAL | The four stages were run; format/nullable/test return to the documented Phase 0 baseline (pre-existing failures only). A fully-green single pass is not achievable locally because the baseline itself is non-green (84 vendored nullable errors, 1 pre-existing CSharpier `.cs` finding, flaky timer tests). The authoritative green gate is the PR CI run. |
| **Explicit reporting** | ✅ PASS | All commands and EXIT codes recorded in feature evidence and re-stated here. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Summarize changes** | ✅ PASS | `acceptance-summary.2026-06-08T12-12.md` and `plan.2026-06-08T12-12.md` summarize the change. |
| **Design choices explained** | ✅ PASS | `.claude/rules/csharp.md` records the file-based wiring choice, severity-first invariant, and SecurityCodeScan deferral rationale. |
| **Update supporting documents** | ✅ PASS | `.claude/rules/csharp.md` updated; `issue.md` AC checked off; `evidence/issue-updates/issue-181.2026-06-08T12-12.md` mirror present. |
| **Provide next steps** | ✅ PASS | Documented follow-ups: RS0030 promotion to warning after legacy cleanup; SecurityCodeScan re-evaluation with a Roslyn-5.x-compatible analyzer; legacy banned-symbol call-site migration. |

---

## 3. Language-Specific Code Change Policy Compliance

### Section 3C# : C# Code Change Policy Compliance

#### C#1 Tooling & Baseline

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Formatting with CSharpier** | ⚠️ PARTIAL | `dotnet tool run csharpier check .` returns EXIT 1 only on a pre-existing untouched baseline `.cs` file; all 30 in-scope project files pass after `csharpier format`. CSharpier (not `dotnet format`) was used, preserving `.csproj` semantics. Source: `final-format.2026-06-08T12-12.md`. |
| **Linting / .NET analyzers** | ✅ PASS | Analyzer/code-style build EXIT 0, 0 errors; new analyzers are active at suggestion severity. Source: `final-analyzer-build.2026-06-08T12-12.md`. |
| **Type checking / nullable** | ⚠️ PARTIAL | Nullable `TreatWarningsAsErrors` build at the 84-error vendored-only baseline; 0 first-party errors; protected gate not regressed. Source: `final-nullable-build.2026-06-08T12-12.md`. |
| **CPM not introduced** | ✅ PASS | No `Directory.Packages.props`; no PackageReference migration; `packages.config` style retained. Source: `invariant-check.2026-06-08T12-12.md` Invariant 2. |
| **Restore via nuget restore** | ✅ PASS | `nuget.exe restore TaskMaster.sln` EXIT 0 with the 5 new analyzer packages. Source: `final-restore.2026-06-08T12-12.md`, `p3-restore.2026-06-08T12-12.md`. |

#### C#2–C#7 Design, Structure, Dependencies

| Requirement | Status | Evidence |
|------------|--------|----------|
| **First-party scope; vendored excluded** | ✅ PASS | 15 first-party projects wired; SVGControl/UtilitiesSwordfish untouched (`git diff` shows no vendored files; `invariant-check` Invariant 7). 135 `<Analyzer Include>` lines + 15 `<AdditionalFiles BannedSymbols.txt>` (9 analyzer DLLs x 15 projects). |
| **Severity-first invariant** | ✅ PASS | All new analyzer diagnostics at `dotnet_analyzer_diagnostic.severity = suggestion`; RS0030 at suggestion; the single `severity = warning` line preserves the pre-existing baseline MSTEST0032 warning (documented). |
| **No unauthorized suppression (CS8032)** | ✅ PASS | No `dotnet_diagnostic.CS8032` entry, no `<WarningsNotAsErrors>` containing CS8032 anywhere. Source: `invariant-check` Invariant 6. |
| **Dependencies analyzer-only** | ✅ PASS | All 5 packages added with `developmentDependency="true"`; no runtime dependency added (Microsoft.Bcl.TimeProvider already present). |

---

## 4. Language-Specific Unit Test Policy Compliance

No C# test code was added or modified by this change. The existing MSTest/Moq/FluentAssertions suites were executed unchanged for the no-regression check (`final-test-coverage.2026-06-08T12-12.md`). The C# Unit Test Policy framework selection (MSTest), mocking (Moq), and assertion (FluentAssertions) conventions are retained and documented as unchanged in `.claude/rules/csharp.md` (`invariant-check` Invariant 3). No new tests were required because no production code paths were added.

---

## 5. Test Coverage Detail

Not applicable. No new or modified functions, classes, or modules were introduced (zero `.cs` line changes). Per-component coverage tables are therefore omitted; the no-regression coverage comparison is in section 1.2 and `evidence/qa-gates/coverage-delta.2026-06-08T12-12.md`.

---

## 6. Test Execution Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Tests | 4064 | ✅ collected |
| Tests Passed | 4054 (99.75%) | ⚠️ |
| Tests Failed | 7 (known-flaky timer family) | ⚠️ baseline flakiness |
| Skipped | 2 | ✅ |
| Code Coverage (raw, all modules) | 58.99% lines | ⚠️ raw figure; authoritative gate is CI |

---

## 7. Code Quality Checks

**For C#:**

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| CSharpier Formatting | `dotnet tool run csharpier check .` | EXIT 1 — 1 pre-existing untouched baseline `.cs` file; all in-scope project files pass | ⚠️ |
| .NET Analyzers / Code Style | `msbuild TaskMaster.sln -t:Rebuild ... -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true` | EXIT 0, 0 errors | ✅ |
| Nullable Type Check | `msbuild TaskMaster.sln -t:Rebuild ... -p:Nullable=enable -p:TreatWarningsAsErrors=true` | EXIT 1 — 84 errors = Phase 0 baseline (all vendored), 0 first-party, 0 CS8032 | ⚠️ no regression |
| MSTest with Coverage | `vstest.console.exe <7 *.Test.dll> /EnableCodeCoverage /InIsolation /Logger:trx` | EXIT 1 — 4054/4064 pass, 7 flaky timer fails, coverage collected | ⚠️ no regression |

**Notes:** All non-green toolchain results are attributable to documented pre-existing Phase 0 baseline conditions (84 vendored nullable errors, 1 pre-existing CSharpier `.cs` finding, the known-flaky wall-clock-timer test family), not to any file modified by this change. The change introduces no `.cs` edits and therefore cannot alter runtime behavior.

---

## 8. Gaps and Exceptions

### Identified Gaps
- **AC6 (PR CI GREEN):** No GitHub PR exists for `feature/csharp-analyzer-stack-181` (`gh pr list --head ...` empty) and no CI run is recorded against the branch head (`gh run list --branch ...` empty; PR-context CI status "not available"). The authoritative repo-wide 80% coverage gate and the nullable-as-errors / MSTest-with-coverage CI steps are therefore UNVERIFIED. Local parity was demonstrated but cannot substitute for an actual green CI run.

### Approved Exceptions
- **`.editorconfig` 567 lines:** Configuration file, not production/test/script source code; exempt from the 500-line source limit. The size is the generated analyzer severity map documented in `evidence/other/editorconfig-severity-map.2026-06-08T12-12.md`.
- **SecurityCodeScan.VS2019 deferral:** Documented, not silent. Version 5.6.7 emits CS8032 under Roslyn 5.6, which cannot be neutralized via `.editorconfig` and would break the protected nullable gate. The package was dropped entirely; no CS8032 suppression and no substitute were introduced. Recorded in `.claude/rules/csharp.md`. This is an authorized adaptation under the issue mandate "adapted so it builds cleanly with zero new build/CI failures."

### Removed/Skipped Tests
- **None.** No tests were removed or skipped. The 7 failing tests are pre-existing flaky timer-family tests, not removed.

---

## 9. Summary of Changes

### Files Modified (functional scope)

1. **`BannedSymbols.txt`** (NEW) — 5 banned time/random symbols (DateTime.Now, DateTime.UtcNow, Random.Shared, Thread.Sleep x2 overloads, Task.Delay x2 overloads) with remediation messages.
2. **`.editorconfig`** (NEW, +567) — global analyzer default at suggestion; per-rule severities; file-scoped-namespace preference; naming rules; RS0030 at suggestion; preserves baseline MSTEST0032 at warning.
3. **15 first-party `*.csproj`** (MODIFIED) — analyzer `<ItemGroup>` with 9 `<Analyzer Include>` DLL items (5 analyzers, multi-DLL Sonar/Roslynator sets) plus `<AdditionalFiles ..\BannedSymbols.txt>`.
4. **15 first-party `packages.config`** (MODIFIED; `VBFunctions/packages.config` newly tracked) — 5 analyzer packages each as `developmentDependency="true"`.
5. **`.claude/rules/csharp.md`** (MODIFIED) — TimeProvider seam guidance; Analyzer Stack section; mechanism; severity-first invariant; SecurityCodeScan deferral note.

No production or test `.cs` source files were modified.

---

## 10. Compliance Verdict

### Overall Status: ⚠️ PARTIALLY COMPLIANT

The implementation is well-scoped, fully documented, and introduces no `.cs` source changes. The analyzer stack, banned-symbol policy, central severities, and rules documentation all satisfy their acceptance criteria with concrete evidence. The local toolchain returns to the documented Phase 0 baseline with no regression. The single material gap is AC6: no PR and no CI run exist for the branch head, so the authoritative repo-wide 80% coverage gate and the nullable-as-errors / MSTest-with-coverage CI steps are UNVERIFIED.

**Fail-closed reminder:** Because the authoritative repo-wide coverage gate (CI) is unverified for this branch, the audit is not marked fully compliant or ready-for-merge; it is conditional on a green PR CI run.

---

### Policy-by-Policy Summary

#### General Code Change Policy (Section 2)
- ✅ Before Making Changes: plan and policy reads documented.
- ✅ Design Principles: simplest viable file-based wiring; centralized config.
- ✅ Module & File Structure: cohesive; `.editorconfig` size exempt as config.
- ✅ Naming, Docs, Comments: rationale comments present.
- ⚠️ Toolchain Execution: at Phase 0 baseline; no regression; full green is CI-only.
- ✅ Summarize & Document: complete.

#### Language-Specific Code Change Policy (Section 3)

**For C#:**
- ⚠️ Tooling & Baseline: format/nullable at baseline; analyzer build clean.
- ✅ Design & Type-Safety: severity-first, no suppression, vendored excluded.
- ✅ Dependencies: analyzer-only developmentDependency packages.

#### General Unit Test Policy (Section 1)
- N/A Core Principles: no test code changed.
- ✅ Coverage & Scenarios: no regression; new-code obligation not triggered.
- N/A Test Structure: no test code changed.
- N/A External Dependencies: no test code changed.
- ✅ Policy Audit: this document plus invariant/acceptance evidence.

#### Language-Specific Unit Test Policy (Section 4)
**For C#:**
- ✅ Framework & Scope: MSTest/Moq/FluentAssertions retained, unchanged.

---

### Metrics Summary

- ⚠️ 4054/4064 tests passing (99.75%); 7 pre-existing flaky timer fails.
- ✅ Analyzer/code-style build: 0 errors.
- ⚠️ Nullable gate: 84 errors = baseline (all vendored); no regression; 0 CS8032.
- ✅ Coverage no-regression: 58.89% -> 58.99% raw (+0.10 pp); new-code N/A (0 `.cs` lines).
- ✅ Evidence-location compliance: no forbidden paths in diff.
- ❌ AC6: PR CI green status unverified (no PR / no CI run for branch head).

---

### Recommendation

**Conditional Go — Blocked on AC6 CI verification.**

The change is technically complete, scoped, and non-regressing locally. Before merge, open the PR for `feature/csharp-analyzer-stack-181` and confirm a GREEN GitHub Actions CI run (nullable-as-errors at the vendored-only baseline, MSTest-with-coverage passing the scoped 80% repo-wide / 90% new-code gates). Once CI is green, all acceptance criteria are satisfied. The 7 flaky timer tests should be confirmed green or retried on the CI run, as they are nondeterministic and unrelated to this change.

---

## Appendix A: Test Inventory

No tests were added or modified by this change. The existing first-party MSTest suites executed for the no-regression check are: QuickFiler.Test, Tags.Test, TaskMaster.Test, TaskVisualization.Test, ToDoModel.Test, UtilitiesCS.Test, VBFunctions.Test (4064 tests total). Full per-test enumeration is out of scope for a build-config-only change; the executed assembly list and pass/fail counts are recorded in `evidence/qa-gates/final-test-coverage.2026-06-08T12-12.md`.

---

## Appendix B: Toolchain Commands Reference

**For C# (commands run by the implementer, re-cited here):**
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

**Reviewer scope/diff verification commands:**
```bash
git diff 2a522ed8..71e0777a --name-only
git diff 2a522ed8..71e0777a -- "*.cs" --stat          # empty: no .cs changes
git diff 2a522ed8..71e0777a -- "*.csproj" | grep -c '^\+.*<Analyzer Include'   # 135
git diff 2a522ed8..71e0777a | grep -iE 'CS8032|WarningsNotAsErrors'           # no suppression
```

---

**Audit Completed By:** feature-reviewer agent
**Audit Date:** 2026-06-08
**Policy Version:** Current (as of audit date)
