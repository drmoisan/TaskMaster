# Policy Compliance Audit: Koverage Coverage Allowlist `.Test` Exclusion (Issue #193)

**Audit Date:** 2026-06-12
**Code Under Test:**
- `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` (PowerShell, MODIFIED)
- `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` (PowerShell, MODIFIED)

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| PowerShell | 2 files | 6 tests | ✅ 6 pass, 0 fail | 85.34% cmds (file, pre-change) | 87.98% cmds (file, post-change) | 100% (changed function `Get-KoverageProjectAllowlist`) |

**Note:** No other language is in scope for the #193 change set. The two changed source files are PowerShell. Other branch artifacts (root `coverage.xml` Pester JaCoCo report, `artifacts/` orchestration files) are related orchestration bookkeeping and are not part of the #193 source change set.

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: file-level Pester CodeCoverage on `Invoke-MSTestWithCoverage.Helpers.ps1` (pre-change), recorded in `docs/features/active/2026-06-12-coverage-metric-includes-test-assemblies-193/evidence/regression-testing/fail-before.2026-06-13T01-56.md` and re-confirmed during this review
- PowerShell post-change coverage artifact: `docs/features/active/2026-06-12-coverage-metric-includes-test-assemblies-193/evidence/qa-gates/final-toolchain.2026-06-13T01-56.md` (file coverage 87.98%; changed function 100%)
- Per-language comparison summary: Section 1.2.1 below

**Non-negotiable verdict rule:** Numeric baseline and post-change coverage for the single in-scope language (PowerShell) plus changed-code coverage are recorded below.

**Fail-closed rule:** All required baseline, QA, and coverage-comparison evidence is present; verdict is not blocked on missing evidence.

**Evidence rule:** All findings below are derived from independent re-runs (PSScriptAnalyzer, Invoke-Formatter, Pester) and diff inspection performed during this review, not from memory.

---

## Executive Summary

Issue #193 (work mode `minor-audit`) modifies one production PowerShell helper function, `Get-KoverageProjectAllowlist`, to resolve each project's assembly name (from `<AssemblyName>` or project-file base-name fallback) and skip any name ending in `.Test` (case-insensitive). Because `ConvertTo-KoverageCoberturaXml` removes any `<package>` whose name is not in the allowlist before recomputing aggregate totals, the change strips `.Test` packages from both the numerator (`lines-covered`) and denominator (`lines-valid`). The test file adds four failing-first Pester regressions covering both name-resolution paths and the post-processing strip, and adjusts one pre-existing path-normalization test to pass `-ProjectNames` explicitly so it no longer depends on the production allowlist.

The change is minimal, cohesive, and confined to one production file and its mirrored test file. The PowerShell toolchain (format, analyze, test) was re-run during this review and passes: format clean, zero new analyzer findings, 6/6 Pester tests pass. The single PSScriptAnalyzer warning on the production file is pre-existing on HEAD and outside the changed function.

**Policy documents evaluated:**
- ✅ `general-code-change.md` (PowerShell source + test change)
- ✅ `general-unit-test.md` (test additions)

**Language-specific policies evaluated:**
- N/A `python.md`
- ✅ `powershell.md` (wrapper/adapter seam mocking, no temp files, determinism, file-size limit, change budget)
- N/A Bash
- N/A JSON / C# / TypeScript

The reviewer artifact-naming convention is confirmed: this audit is named `policy-audit.2026-06-12T22-29.md` (ISO-8601 `yyyy-MM-ddTHH-mm`) and written to the active feature folder root.

**Temporary artifacts cleanup:**
- ✅ No temporary or one-time scripts were created by the #193 change set in the source tree. A transient `/tmp/baseline-helper.ps1` was created only inside this reviewer's session for baseline analyzer comparison and is outside the repository.
- ✅ No ongoing tooling scripts were added.
- The root `coverage.xml` (untracked Pester JaCoCo report) is test/orchestration bookkeeping, not a #193 deliverable.

---

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Independence** - Tests run in any order | ✅ PASS | Each `It` block constructs its own in-memory XML string and calls the function under test. The fallback-branch test sets and relies only on local `Mock` scopes (auto-removed at `It` boundary). No shared mutable state. |
| **Isolation** - Each test targets single behavior | ✅ PASS | Two `Describe` blocks: `ConvertTo-KoverageCoberturaXml` (path normalization, merge, `.Test` strip) and `Get-KoverageProjectAllowlist` (allowlist exclusion, production retention, base-name fallback). One behavior per `It`. |
| **Fast Execution** - Tests complete quickly | ✅ PASS | Full file completed in 15.87s (Pester 5.6.1). Slowest tests (~3.7-4.0s) call `Get-KoverageProjectAllowlist` with default `-RepoRoot`, which scans real project files; the mocked fallback test runs in 121ms. Acceptable for the suite. |
| **Determinism** - Consistent results | ✅ PASS | Inputs are literal here-strings; the fallback test mocks `Get-ChildItem`/`Get-Content` so it touches no disk. No randomness, clocks, or network. |
| **Readability & Maintainability** - Clear structure | ✅ PASS | Descriptive `It` names; each new test has an explanatory comment tying it to AC and Issue #193. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Baseline Coverage Documented** | ✅ PASS | **Baseline (pre-development):** 85.34% commands on `Invoke-MSTestWithCoverage.Helpers.ps1` (file level).<br>**Command:** Pester CodeCoverage on the helper file with the pre-change test file.<br>**Timestamp:** 2026-06-13 01:56.<br>**Note:** Recorded in `evidence/qa-gates/final-toolchain.2026-06-13T01-56.md` and `evidence/regression-testing/fail-before.2026-06-13T01-56.md`. |
| **No Coverage Regression** | ✅ PASS | **Post-change coverage:** 87.98% commands (file level).<br>**Change:** +2.64% file level.<br>**Status:** No regression. Changed function `Get-KoverageProjectAllowlist` is 100% covered post-change.<br>Baseline: 85.34% → Post-change: 87.98% (+2.64%). |
| **New Code Coverage >=90%** | ✅ PASS | **Modified file:** `Invoke-MSTestWithCoverage.Helpers.ps1`.<br>**New/changed code coverage:** 100% of the changed function `Get-KoverageProjectAllowlist`, including the `<AssemblyName>` path, the base-name fallback, and the `.Test` exclusion branch.<br>**Calculation method:** Per-command coverage on the changed function from the post-change Pester run; both name-resolution branches are exercised by dedicated tests. |
| **Comprehensive Coverage** | ✅ PASS | `Get-KoverageProjectAllowlist` (lines 3-47): 3 tests (exclusion, retention, fallback).<br>`ConvertTo-KoverageCoberturaXml` (lines 281-344): `.Test` strip + numerator/denominator recompute test, plus retained path-normalization and merge tests.<br>Untested file remainder (87.98% < 100%) is in `Merge-CoberturaClassesByFilename` and error-throw paths, pre-existing and outside change scope. |
| **Positive Flows** - Valid inputs | ✅ PASS | `retains non-test production projects in the allowlist` (UtilitiesCS retained); `excludes .Test packages...` retains UtilitiesCS package and its lines. |
| **Negative Flows** - Invalid inputs | N/A PASS | The changed function has no user-supplied invalid-input contract beyond `RepoRoot`; the relevant negative case here is the exclusion of `.Test` names, covered by `excludes projects that resolve to a .Test assembly name`. |
| **Edge Cases** - Boundary conditions | ✅ PASS | Base-name fallback (no `<AssemblyName>` element) exercised by `applies the .Test exclusion to the project-file base-name fallback`; case-insensitive suffix match enforced via `OrdinalIgnoreCase`. |
| **Error Handling** - Error paths | N/A PASS | The changed function adds no new throw paths. Existing `<packages>`-missing throw in `ConvertTo-KoverageCoberturaXml` is unchanged and outside scope. |
| **Concurrency** - If applicable | N/A | Pure synchronous string/XML transforms; no concurrency. |
| **State Transitions** - If applicable | N/A | No stateful component introduced. |

### 1.2.1 Per-Language Coverage Comparison

- PowerShell: Baseline: 85.34% commands -> Post-change: 87.98% commands. Change: +2.64% commands. New/changed-code coverage: 100%. Disposition: PASS. Evidence: `docs/features/active/2026-06-12-coverage-metric-includes-test-assemblies-193/evidence/qa-gates/final-toolchain.2026-06-13T01-56.md` (re-confirmed by Pester 6/6 during this review).

Note on repo-wide PowerShell coverage: a repo-wide PowerShell coverage artifact is not produced for this minor-audit. The change is a 14-line edit to one helper function within a file that already exceeds the 80% file-level command-coverage floor (85.34% baseline -> 87.98% post-change), and the changed lines are fully covered. No coverage regression is present on the changed lines.

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clear Failure Messages** | ✅ PASS | Uses Pester `Should -Contain`, `Should -Not -Contain`, `Should -Be`, `Should -BeNullOrEmpty` — each produces an actionable diff on failure. The fail-before run demonstrated readable failure output. |
| **Arrange-Act-Assert Pattern** | ✅ PASS | Each test arranges input XML / mocks, acts via the function call, then asserts on the result. |
| **Document Intent** | ✅ PASS | Descriptive `It` names plus per-test comments referencing the AC and Issue #193. |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Avoid External Dependencies** | ✅ PASS | No network, DB, or live process. The default-`RepoRoot` allowlist tests read on-disk project files in the repo (a deterministic, version-controlled adapter boundary); the fallback test mocks the filesystem entirely. |
| **Use Mocks/Stubs** | ✅ PASS | `Get-ChildItem` and `Get-Content` are mocked in the fallback test to force the base-name path and avoid disk dependence — an adapter-seam application (powershell.md Design Seams option 3). |
| **Environment Stability** | ✅ PASS | No temporary files created by the tests. No mutable global/script state introduced. Confirmed by diff inspection. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Pre-submission Review** | ✅ PASS | This document is the required pre-submission policy review for #193. No outstanding review items. |

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clarify the objective** | ✅ PASS | Objective stated in `issue.md` (#193): exclude `.Test` assemblies from the Koverage coverage metric. |
| **Read existing change plans** | ✅ PASS | `plan.2026-06-12T21-54.md` present in the feature folder (bugfix-workflow phased plan). |
| **Document the plan** | ✅ PASS | Plan and issue document the fix approach and validation ideas. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Simplicity first** | ✅ PASS | Single-function edit: resolve name once, exclude `.Test`, then add. No new abstractions. |
| **Reusability** | ✅ PASS | The resolved-name logic is consolidated so both the `<AssemblyName>` and fallback paths share one `.Test` check, removing the prior duplicated `Add` calls. |
| **Extensibility** | ✅ PASS | `ConvertTo-KoverageCoberturaXml` already accepts an injectable `-ProjectNames`, preserved for callers/tests. |
| **Separation of concerns** | ✅ PASS | Allowlist construction (`Get-KoverageProjectAllowlist`) remains separate from XML post-processing (`ConvertTo-KoverageCoberturaXml`). |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Cohesive modules** | ✅ PASS | Helper module remains a cohesive set of Koverage post-processing functions. |
| **Under 500 lines** | ✅ PASS | `Invoke-MSTestWithCoverage.Helpers.ps1`: 344 lines (was 334). `Invoke-MSTestWithCoverage.Helpers.Tests.ps1`: 171 lines (was 81). Both under 500. |
| **Public vs internal** | ✅ PASS | No change to the public surface; same exported functions. |
| **No circular dependencies** | ✅ PASS | Test dot-sources the helper; no cycles introduced. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Descriptive names** | ✅ PASS | `$resolvedName` clearly names the intermediate. Function names unchanged. |
| **Docs/docstrings** | ✅ PASS | A multi-line comment explains why `.Test` projects are excluded and the numerator/denominator effect. |
| **Comment why, not what** | ✅ PASS | The added comment states the rationale (coverage-metric correctness), not a restatement of the code. |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| **1. Formatting** | ✅ PASS | **Command:** `Invoke-Formatter -ScriptDefinition <file>` on both files (this review).<br>**Result:** FORMAT-CLEAN for both; no changes needed. |
| **2. Linting** | ✅ PASS | **Command:** `Invoke-ScriptAnalyzer -Path <file>` on both files (this review).<br>**Result:** Test file 0 findings; production file 1 finding (`PSUseSingularNouns`), pre-existing on HEAD and outside the changed function. Analyzer delta: 0 new findings. |
| **3. Type checking** | N/A | Not applicable for PowerShell. |
| **4. Testing** | ✅ PASS | **Command:** `Invoke-Pester` on the test file (this review).<br>**Result:** 6 passed, 0 failed, 0 skipped. |
| **Full toolchain loop** | ✅ PASS | Format -> analyze -> test completed in one clean pass during this review (no auto-fix changes). |
| **Explicit reporting** | ✅ PASS | Commands and results documented here and in `evidence/qa-gates/final-toolchain.2026-06-13T01-56.md`. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Summarize changes** | ✅ PASS | Summarized in `issue.md` and this audit. |
| **Design choices explained** | ✅ PASS | The decision to gate on assembly name (not project path) and to leave `ConvertTo-KoverageCoberturaXml` unchanged is documented in `issue.md` and the code-review artifact. |
| **Update supporting documents** | ✅ PASS | `issue.md` AC checkboxes updated; feature evidence present. |
| **Provide next steps** | ✅ PASS | Remaining step in `issue.md` is the minor-audit review (this artifact) and PR. |

---

## 3. Language-Specific Code Change Policy Compliance

### Section 3B: PowerShell Code Change Policy Compliance

#### 3B.1 Tooling & Baseline

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Formatting with Invoke-Formatter** | ✅ PASS | FORMAT-CLEAN on both changed files (this review). |
| **Linting with PSScriptAnalyzer** | ✅ PASS | 0 new findings; 1 pre-existing `PSUseSingularNouns` outside change scope. |
| **Fix all findings** | ✅ PASS | No new findings to fix. The pre-existing finding is outside the changed function and is not renamed to avoid an unrelated refactor (consistent with minor-audit scope discipline). |
| **PowerShell 7+ compatible** | ✅ PASS | Uses .NET types and language features available on PowerShell 7+; no version-specific constructs added. |

#### 3B.2 PowerShell Design & Safety

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Advanced functions** | ✅ PASS | `Get-KoverageProjectAllowlist` retains `[CmdletBinding()]`, `[OutputType]`, and a validated `-RepoRoot`. |
| **Parameter validation** | ✅ PASS | No new parameters; existing validation preserved. |
| **Avoid global state** | ✅ PASS | No global/script-scoped mutable state added. |
| **Error handling** | ✅ PASS | No new silent catch-alls. Behavior remains fail-explicit at the `<packages>` check in the post-processor. |

#### 3B.3 Structure, Naming, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Cohesive and under 500 lines** | ✅ PASS | 344 and 171 lines respectively. |
| **Approved verbs** | ✅ PASS | `Get-KoverageProjectAllowlist` uses approved verb `Get`. (The pre-existing `PSUseSingularNouns` finding concerns a plural noun on `Get-CoberturaLineConditionCoverageParts`, not a verb violation, and is outside scope.) |
| **Comment why** | ✅ PASS | Added comment explains the coverage-metric rationale. |

#### 3B.4 Running the Toolchain

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Step 1: Format** | ✅ PASS | FORMAT-CLEAN. |
| **Step 2: Analyze** | ✅ PASS | 0 new findings. |
| **Step 3: Type check** | N/A | Not applicable for PowerShell. |
| **Step 4: Test** | ✅ PASS | 6/6 Pester pass. |
| **Rerun loop if needed** | ✅ PASS | Single clean pass; no rerun required. |

---

## 4. Language-Specific Unit Test Policy Compliance

### Section 4B: PowerShell Unit Test Policy Compliance

#### 4B.1 Framework and Scope

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Use Pester v5.x** | ✅ PASS | Pester 5.6.1; uses `BeforeAll`, `Describe`/`It`, modern `Should` syntax. |
| **Use PoshQC Configuration** | ⚠️ PARTIAL | PoshQC MCP test gate returned exit code 1 across the broader folder scan (folder includes other files with findings); the changed test file itself passes 6/6 via `Invoke-Pester` directly. The repo PoshQC settings file is not present in this worktree, so defaults were used. This does not indicate a #193 regression. |
| **PowerShell 7+ Compatible** | ✅ PASS | No version-specific constructs. |

#### 4B.2 Test Style and Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Focused Unit Tests** | ✅ PASS | One behavior per `It`. |
| **Test Behavior Over Implementation** | ✅ PASS | Tests assert on observable output (package presence, aggregate totals, allowlist contents), not internal mechanics. |
| **Mocking Used Sparingly** | ✅ PASS | Only the fallback test mocks `Get-ChildItem`/`Get-Content`, to force the no-`<AssemblyName>` path deterministically. |
| **Organization** | ✅ PASS | Test file `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` mirrors code file `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`. |

#### 4B.3 Naming and Readability

| Requirement | Status | Evidence |
|------------|--------|----------|
| **File Naming** - *.Tests.ps1 | ✅ PASS | `Invoke-MSTestWithCoverage.Helpers.Tests.ps1`. |
| **Describe/Context/It Structure** | ✅ PASS | 2 Describe blocks, 6 It blocks. |
| **Logical Grouping** | ✅ PASS | Grouped by function under test. |
| **Docstrings/Comments** | ✅ PASS | Self-documenting names plus per-test rationale comments. |

#### 4B.4 Running the Toolchain

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Use PoshQCTest Command** | ⚠️ PARTIAL | PoshQC MCP path returns non-zero on the folder scan due to unrelated files; the #193 test file passes 6/6 via direct Pester. No #193-attributable failure. |
| **No Alternative Test Runners** | ✅ PASS | Only Pester is used. |

---

## 5. Test Coverage Detail

### Get-KoverageProjectAllowlist (3 tests)

| Test Name | Scenario Type | Lines Covered | Status |
|-----------|--------------|---------------|--------|
| excludes projects that resolve to a .Test assembly name | Negative (exclusion) | 24-46 (`.Test` continue branch) | ✅ |
| retains non-test production projects in the allowlist | Positive | 24-46 (add branch) | ✅ |
| applies the .Test exclusion to the project-file base-name fallback | Edge case (fallback) | 35-41 (fallback + exclusion) | ✅ |

**Coverage:** 100% of `Get-KoverageProjectAllowlist` (lines 3-47).

**Not covered:** None within the changed function.

### ConvertTo-KoverageCoberturaXml (3 tests)

| Test Name | Scenario Type | Lines Covered | Status |
|-----------|--------------|---------------|--------|
| preserves backslash separators for nested Windows paths... | Positive (path normalize) | 311-313 | ✅ |
| merges duplicate class entries that point to the same source file | Edge case (merge) | 315 | ✅ |
| excludes .Test packages from the report and from the aggregate covered/valid line totals | Negative (strip + recompute) | 305-309, 328-334 | ✅ |

**Coverage:** File-level 87.98%; remaining uncovered code is in `Merge-CoberturaClassesByFilename` and error-throw paths, pre-existing and outside change scope.

**Not covered:** Pre-existing untested branches outside the #193 change scope.

---

## 6. Test Execution Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Tests | 6 | ✅ |
| Tests Passed | 6 (100%) | ✅ |
| Tests Failed | 0 | ✅ |
| Execution Time | 15.87s total | ✅ Fast |
| Average Time per Test | ~2.6s | ✅ Acceptable (real project-file scan dominates) |
| Discovery Time | 102ms | ✅ |
| Functions/Classes Tested | 2/6 functions directly; changed function 1/1 | ✅ |
| Test File Size | 171 lines | ✅ Maintainable |
| Code Coverage (if applicable) | 87.98% file cmds; 100% changed function | ✅ |

---

## 7. Code Quality Checks

**For PowerShell:**

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| Invoke-Formatter | `Invoke-Formatter -ScriptDefinition <file>` | FORMAT-CLEAN both files | ✅ |
| PSScriptAnalyzer | `Invoke-ScriptAnalyzer -Path <file>` | 0 new findings; 1 pre-existing out of scope | ✅ |
| Pester Tests | `Invoke-Pester` on changed test file | 6 passed, 0 failed | ✅ |

**Notes:**
- Pre-existing PSScriptAnalyzer finding: `PSUseSingularNouns` on `Get-CoberturaLineConditionCoverageParts` (baseline line 123; shifted to line 133 by the added comment block). Confirmed present on `HEAD` via analysis of the baseline file. It is outside the changed function `Get-KoverageProjectAllowlist`. Note: prior #193 evidence (`final-toolchain.2026-06-13T01-56.md`) attributed this finding to `Merge-CoberturaClassesByFilename`; the actual function PSScriptAnalyzer flags is `Get-CoberturaLineConditionCoverageParts`. Either way the finding pre-exists on HEAD and is outside the change scope; the attribution discrepancy does not affect the verdict.
- A pre-existing, unrelated folder-level Pester failure (`Install-RepoDotNetSdk.Tests.ps1` expects SDK 8.0.205 vs committed `global.json` pin 10.0.200, tracked as SDK-PIN-001) is confirmed unrelated to #193 and is not counted against this change.

---

## Evidence Location Compliance

A scan of the #193 change set (the two changed PowerShell files plus the untracked feature folder) found no evidence artifacts written to forbidden `artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`, `artifacts/evidence/`, or `artifacts/coverage/` paths. All #193 evidence resides under `docs/features/active/2026-06-12-coverage-metric-includes-test-assemblies-193/evidence/` (`qa-gates/`, `regression-testing/`). No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` entries required.

The root `coverage.xml` is an untracked Pester JaCoCo report (test bookkeeping), not an evidence artifact this review produced and not under a forbidden evidence path.

---

## 8. Gaps and Exceptions

### Identified Gaps
- Repo-wide PowerShell coverage artifact: not produced for this minor-audit. Mitigation: changed lines are fully covered, the changed file is above the 80% command floor before and after, and the change is a 14-line localized edit. This is not a blocking gap for a `minor-audit` change of this size.

### Approved Exceptions
- **None.** No policy exceptions are claimed.

### Removed/Skipped Tests
- **None.** No tests were removed or skipped. One pre-existing test was adjusted to pass `-ProjectNames` explicitly so it does not depend on the production allowlist that now excludes `.Test`; coverage of that path is unchanged.

---

## 9. Summary of Changes

### Commits in This PR/Branch

The #193 change set is present as uncommitted working-tree modifications on `feature/csharp-coverage-uplift` (branch HEAD `4a21a5b8`, equal to `origin/main`). No #193 commits exist yet; the diff is the staged/working-tree change set.

### Files Modified

1. **`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`** (MODIFIED)
   - `Get-KoverageProjectAllowlist`: resolve assembly name from `<AssemblyName>` or project-file base-name fallback, then skip any name ending `.Test` (case-insensitive); single `Add` for retained names.
   - Added rationale comment. +10 net lines (334 -> 344).

2. **`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`** (MODIFIED)
   - Added 4 regressions (3 in a new `Get-KoverageProjectAllowlist` Describe, 1 strip/recompute test in `ConvertTo-KoverageCoberturaXml`).
   - Adjusted the path-normalization test to pass `-ProjectNames @('QuickFiler.Test')` explicitly. +90 net lines (81 -> 171).

3. **Feature folder docs/evidence** (NEW, untracked) — `issue.md`, `plan.*.md`, `evidence/qa-gates/`, `evidence/regression-testing/`.

---

## 10. Compliance Verdict

### Overall Status: ✅ FULLY COMPLIANT

The #193 change set complies with the General Code Change Policy, the General Unit Test Policy, and the PowerShell standards. The change is minimal and cohesive, the toolchain passes cleanly with zero new analyzer findings, changed-line coverage is 100%, no temporary files are used, and the file-size and change-budget limits are respected (1 production file, 1 test file — within the 2-production / 3-test caps).

**Fail-closed reminder:** All required baseline, QA, and coverage-comparison evidence for the in-scope language is present; no verdict is blocked on missing evidence.

---

### Policy-by-Policy Summary

#### General Code Change Policy (Section 2)
- ✅ Before Making Changes: plan and issue present
- ✅ Design Principles: simple, cohesive, single-responsibility edit
- ✅ Module & File Structure: both files under 500 lines
- ✅ Naming, Docs, Comments: rationale comment, descriptive intermediate name
- ✅ Toolchain Execution: clean single pass
- ✅ Summarize & Document: issue and audits updated

#### Language-Specific Code Change Policy (Section 3)

**For PowerShell:**
- ✅ Tooling & Baseline: format clean, 0 new analyzer findings
- ✅ PowerShell Design & Safety: advanced function preserved, no global state
- ✅ Structure & Naming: under limit, approved verb
- ✅ Toolchain: format -> analyze -> test clean

#### General Unit Test Policy (Section 1)
- ✅ Core Principles: independent, isolated, deterministic, fast, readable
- ✅ Coverage & Scenarios: changed-line coverage 100%, no regression
- ✅ Test Structure: AAA, clear diagnostics
- ✅ External Dependencies: no temp files, filesystem mocked in fallback test
- ✅ Policy Audit: this document

#### Language-Specific Unit Test Policy (Section 4)

**For PowerShell:**
- ✅ Framework & Scope: Pester 5.6.1
- ✅ Test Style & Structure: behavior-focused, sparing mocks
- ✅ Naming & Readability: descriptive names, comments
- ⚠️ Toolchain: PoshQC MCP folder scan non-zero on unrelated files; #193 test file passes 6/6 via direct Pester

---

### Metrics Summary

- ✅ 6/6 tests passing (100%)
- ✅ Changed function 100% covered; file 87.98%
- ✅ No coverage regression on changed lines (+2.64% file level)
- ✅ Proper file organization: test mirrors source
- ✅ All code-quality checks passing (0 new analyzer findings)
- ✅ Test execution time: 15.87s

---

### Recommendation

**Ready for merge.**

The change satisfies all policy requirements with no blocking findings. Recommended (non-blocking) follow-ups: (1) reconcile the `Merge-CoberturaClassesByFilename` vs `Get-CoberturaLineConditionCoverageParts` attribution in the prior QA-gate evidence note; (2) optionally address the pre-existing `PSUseSingularNouns` finding in a separate cleanup change.

---

## Appendix A: Test Inventory

### Complete Test List

1. ConvertTo-KoverageCoberturaXml › preserves backslash separators for nested Windows paths while making them workspace-relative
2. ConvertTo-KoverageCoberturaXml › merges duplicate class entries that point to the same source file
3. ConvertTo-KoverageCoberturaXml › excludes .Test packages from the report and from the aggregate covered/valid line totals
4. Get-KoverageProjectAllowlist › excludes projects that resolve to a .Test assembly name
5. Get-KoverageProjectAllowlist › retains non-test production projects in the allowlist
6. Get-KoverageProjectAllowlist › applies the .Test exclusion to the project-file base-name fallback

---

## Appendix B: Toolchain Commands Reference

**For PowerShell:**
```powershell
# Formatting (per file)
Invoke-Formatter -ScriptDefinition (Get-Content -Raw <file>)

# Linting (per file)
Invoke-ScriptAnalyzer -Path <file>

# Testing
Invoke-Pester -Path ./tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1
```

---

**Audit Completed By:** feature-reviewer (Claude)
**Audit Date:** 2026-06-12
**Policy Version:** Current (as of audit date)
