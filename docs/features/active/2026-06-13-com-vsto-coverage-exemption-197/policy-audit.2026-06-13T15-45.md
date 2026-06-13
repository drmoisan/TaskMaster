# Policy Compliance Audit: COM/VSTO/WinForms Coverage Exemption (Issue #197)

**Audit Date:** 2026-06-13
**Code Under Test:** Feature branch `refactor/com-vsto-coverage-exemption-197` (head `a564add0`) vs base `origin/main` (merge-base `1b3f5350`). Changed files (non-doc): 29 `.cs` files (`[ExcludeFromCodeCoverage]` additions: 25 class-level + IDList method-level on 4 members; 28 enumerated class targets total counting the partial `ThisAddIn` code-behind), `coverage.config`, `TaskMaster.runsettings`, `CLAUDE.md`, `.claude/rules/general-unit-test.md`, plus four `.claude/agent-memory/` files (non-policy memory notes).

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 29 .cs + 2 config files | 4068 tests | ✅ 4066 pass, 2 fail (pre-existing flaky, identical set) | 59.03% lines (production-only deduped, 38,820/65,768) | 71.73% lines (production-only deduped, 37,010/51,594) | N/A — no new production code; attributes/config/docs only |

**Note:** Only C# source/config and Markdown policy/memory files changed. No Python, PowerShell, TypeScript, Bash, or JSON source files changed in this branch.

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - out of scope` (no TypeScript files changed in the branch diff)
- TypeScript post-change coverage artifact: `N/A - out of scope` (no TypeScript files changed in the branch diff)
- PowerShell baseline coverage artifact: `N/A - out of scope` (no PowerShell files changed in the branch diff)
- PowerShell post-change coverage artifact: `N/A - out of scope` (no PowerShell files changed in the branch diff)
- C# baseline coverage artifact: `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/baseline/coverage-firstparty.baseline.cobertura.xml` (also `artifacts/csharp/coverage-firstparty.baseline.cobertura.xml`, `coverage/coverage.baseline.firstparty.cobertura.xml`)
- C# post-change coverage artifact: `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/coverage-firstparty.postexemption.cobertura.xml` (also `coverage/coverage.final.firstparty.cobertura.xml`, `artifacts/csharp/coverage-firstparty.postexemption.cobertura.xml`)
- Per-language comparison summary: see §1.2.1 below and `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/coverage-delta.md`

**Non-negotiable verdict rule:** This audit reports numeric baseline (59.03%) and post-change (71.73%) C# coverage. New-code coverage is N/A because no new executable production code was added (the change is `[ExcludeFromCodeCoverage]` attributes, required `using` directives, two coverage-config excludes, and policy/memory docs).

---

## Executive Summary

This branch implements Issue #197: a formal coverage exemption for architecturally-untestable Outlook-COM / VSTO / WinForms-bound C# code. The implementation is non-behavioral: it adds `[ExcludeFromCodeCoverage]` attributes (class-level on 25 enumerated COM/VSTO/WinForms classes across QuickFiler, TaskMaster, ToDoModel, Tags; method-level on the four Outlook-dependent `IDList` members), two `ModulePath` excludes for the `TaskVisualization` assembly (`coverage.config` and `TaskMaster.runsettings`), and policy-documentation updates in `CLAUDE.md` and `.claude/rules/general-unit-test.md` recording the testable-denominator definition and exemption authority.

Toolchain evidence from the executor's per-phase and final QA gates shows the full C# toolchain green in the final pass: csharpier (no diff, EXIT_CODE 0), msbuild analyzers + code style (EXIT_CODE 0), msbuild nullable + warnings-as-errors (EXIT_CODE 0), and the MSTest suite (4066/4068 pass; the 2 failures are the same pre-existing flaky timing/threading tests present at the Phase 0 baseline — behavior parity confirmed).

The exempt/non-exempt boundary was verified against the design memo §2 tables (`exemption-boundary-verification.md`): every enumerated COM/VSTO/WinForms target carries the attribute and is absent from the post-change denominator, and every enumerated testable seam (`ToDoLoader`, `IDList.GetNextToDoID`, `KbdActions<>`, `TagController` pure-logic methods, settings/path helpers) is NOT annotated and remains in the denominator. This reviewer independently confirmed via diff inspection that `IDList.GetNextToDoID` is unannotated while the Outlook ctors and `RefreshIDList` overloads carry the attribute, and that `Tags/TagController.cs` carries no exemption attribute.

One acceptance criterion (AC4) is not met: the measured post-exemption rate is 71.73%, which is 1.47 pp below the design memo §3 estimate lower bound (73.2%). This is assessed below as a non-blocking, authority-scoped estimate deviation rather than an implementation defect. See §8 and the feature audit for the detailed reasoning.

**Policy documents evaluated:**
- ✅ `general-code-change.instructions.md` (`.claude/rules/general-code-change.md`)
- ✅ `general-unit-test.instructions.md` (`.claude/rules/general-unit-test.md`)

**Language-specific policies evaluated:**
- N/A `python-code-change.instructions.md` + `python-unit-test.instructions.md` (no Python files changed)
- N/A `powershell-code-change.instructions.md` + `powershell-unit-test.instructions.md` (no PowerShell files changed)
- N/A Bash: shfmt + shellcheck + bats (no Bash files changed)
- N/A JSON: format_json + validate_json (no governed JSON files changed)
- ✅ C# Code Change Policy + C# Unit Test Policy (CLAUDE.md §C#1–C#7, §CUT1–CUT3)

This change adds no tests and changes no production behavior. Verification is by re-measurement and toolchain pass. Coverage was verified by inspecting the executor's pre-existing Cobertura artifacts; no coverage generation was re-run by this reviewer.

**Temporary artifacts cleanup:**
- ✅ No temporary/one-time scripts were created by this feature; the change is attribute/config/doc-only.
- N/A No ongoing tooling scripts were introduced.
- No scripts created during development; nothing to dispose.

---

## Rejected Scope Narrowing

The caller prompt included the context line: "The roadmap increment tests that would raise covered code are explicitly OUT OF SCOPE for #197." This statement is consistent with the feature scope and the maintainer-ratified spec Non-Goals; it does not attempt to narrow the audit scope of changed files and is not a rejected narrowing. No caller instruction attempted to limit the branch-diff scope, to mark C# coverage as "informational only," or to skip any toolchain check for a language with changed files. The full feature-vs-base audit was performed. No narrowing was detected.

---

## Evidence Location Compliance

The branch diff was scanned for evidence files written under forbidden `artifacts/` sub-paths (`artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`, `artifacts/evidence/`, `artifacts/coverage/`, `artifacts/regression-testing/`, `artifacts/post-change/`).

- Scan command: `git diff --name-only 1b3f5350..HEAD | grep -E '^artifacts/(baselines?|qa|qa-gates|evidence|coverage|regression-testing|post-change)/'`
- Result: no matches. All feature evidence is written under the canonical `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/<kind>/` scheme (baseline/, qa-gates/).
- The `validate_evidence_locations.py` script referenced by the agent contract is not present in this repository; the equivalent check was performed via the `git diff`-based scan above. No violations found.

**Verdict:** PASS — no evidence written to forbidden `artifacts/` locations; all feature evidence is under the canonical feature `evidence/` tree.

---

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Independence** - Tests run in any order | N/A PASS | No tests were added or modified. The existing MSTest suite (4068 tests) is the behavior regression guard; it ran green except 2 pre-existing flaky timing tests. |
| **Isolation** - Each test targets single behavior | N/A PASS | No test changes. Existing suite unchanged. |
| **Fast Execution** - Tests complete quickly | N/A PASS | No test changes. Full suite executes via the coverage pipeline. |
| **Determinism** - Consistent results | ⚠️ PARTIAL | 2 pre-existing flaky timing/threading tests (`AddEntry_UseUiThreadTrue_...`, `RequestTask_WithProvidedTask_...`) plus an intermittent `AppQuickFilerSettings` shared-static race were documented as pre-existing in `test-result-parity.md`. These are not introduced by this feature (identical failing set before/after) but are pre-existing UT4 shared-global/timing weaknesses. |
| **Readability & Maintainability** - Clear structure | N/A PASS | No test changes. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Baseline Coverage Documented** | ✅ PASS | **Baseline (pre-development):** 59.03% lines (38,820/65,768 production-only deduped). **Command:** `pwsh -NoProfile scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput coverage/coverage.baseline.cobertura.xml`. **Timestamp:** 2026-06-13 12:05. Recorded in `evidence/baseline/mstest-coverage-baseline.md`. |
| **No Coverage Regression** | ✅ PASS | **Post-change coverage:** 71.73% lines (37,010/51,594). **Change:** +12.7 pp on the testable denominator. The rate rose because the architecturally-untestable COM/VSTO/WinForms denominator was removed by design; the lines that left were near-uncovered. No genuine coverage regression for retained measured code. Evidence: `evidence/qa-gates/coverage-delta.md`. |
| **New Code Coverage ≥90%** | N/A PASS | No new executable production code. The diff adds only `[ExcludeFromCodeCoverage]` attributes, `using System.Diagnostics.CodeAnalysis;` directives, two config excludes, and documentation. There is no new code to which the ≥90% rule applies. |
| **Comprehensive Coverage** | N/A | No new behavior introduced; scenario completeness is unchanged from baseline. |
| **Positive Flows** - Valid inputs | N/A | No test changes. |
| **Negative Flows** - Invalid inputs | N/A | No test changes. |
| **Edge Cases** - Boundary conditions | N/A | No test changes. The method-level `IDList` annotation edge case (only Outlook-dependent members exempted; `GetNextToDoID` retained) was verified by diff inspection and `exemption-boundary-verification.md`. |
| **Error Handling** - Error paths | N/A | No test changes. |
| **Concurrency** - If applicable | N/A | No concurrency code changed. |
| **State Transitions** - If applicable | N/A | No stateful behavior changed. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 59.03% lines (production-only first-party deduped, 38,820/65,768) -> Post-change: 71.73% lines (37,010/51,594). Change: +12.70% lines (testable-denominator redefinition, not a regression). New/changed-code coverage: N/A - no new executable production code (attributes/config/docs only). Disposition: PASS (repo-wide testable-denominator gate of 80% is a forward target for the redefined denominator; #197 delivers the exemption mechanism, not the floor — the sub-80% rate is the maintainer-ratified expected outcome per spec §Risks "Floor still not reached"). Evidence: `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/coverage-delta.md`, `.../coverage-firstparty.postexemption.cobertura.xml`.
- TypeScript: Baseline: N/A% lines -> Post-change: N/A% lines. Change: N/A% lines. New/changed-code coverage: `N/A - out of scope`. Disposition: N/A (zero changed files on the branch). Evidence: `N/A - out of scope`.
- PowerShell: Baseline: N/A% cmds -> Post-change: N/A% cmds. Change: N/A% cmds. New/changed-code coverage: `N/A - out of scope`. Disposition: N/A (zero changed files on the branch). Evidence: `N/A - out of scope`.
- Python: Baseline: N/A% lines -> Post-change: N/A% lines. Change: N/A% lines. New/changed-code coverage: `N/A - out of scope`. Disposition: N/A (zero changed files on the branch). Evidence: `N/A - out of scope`.

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clear Failure Messages** | N/A PASS | No test changes. |
| **Arrange-Act-Assert Pattern** | N/A PASS | No test changes. |
| **Document Intent** | N/A PASS | No test changes. |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Avoid External Dependencies** | N/A PASS | No test changes. The feature's premise is precisely that the exempted code requires a live Outlook COM host and therefore cannot be unit-tested under UT4. |
| **Use Mocks/Stubs** | N/A PASS | No test changes. |
| **Environment Stability** | N/A PASS | No temporary files created. No mutable global state introduced. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Pre-submission Review** | ✅ PASS | This audit document plus the accompanying code-review and feature-audit artifacts satisfy the pre-submission review requirement. |

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clarify the objective** | ✅ PASS | Objective is documented in `issue.md` and `spec.md` (Issue #197): redefine the 80% floor to a testable denominator by exempting COM/VSTO/WinForms code. Design basis: `artifacts/research/2026-06-12-com-vsto-coverage-exemption-design.md`. |
| **Read existing change plans** | ✅ PASS | `plan.2026-06-13T11-28.md` is the atomic plan; Phase 0 evidence (`evidence/baseline/phase0-instructions-read.md`) records the policy read order. |
| **Document the plan** | ✅ PASS | Phased plan (config+docs, then per-assembly annotation batches) present in `plan.2026-06-13T11-28.md`. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Simplicity first** | ✅ PASS | Two-layer mechanism (assembly exclude for the near-wholly-COM `TaskVisualization`; class/method attributes elsewhere) is the simplest approach that preserves testable seams. |
| **Reusability** | N/A | No reusable logic added; diagnostic attributes only. |
| **Extensibility** | ✅ PASS | The documented convention (annotate new COM-bound classes) is the extension point; recorded in the policy docs. |
| **Separation of concerns** | ✅ PASS | The exemption preserves the separation between testable seams (kept in denominator) and untestable COM/WinForms glue (exempted), which is the core design intent verified in `exemption-boundary-verification.md`. |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Cohesive modules** | ✅ PASS | No module reorganization; attributes added in place. |
| **Under 500 lines** | ⚠️ PARTIAL | Several changed `.cs` files exceed 500 lines at baseline (`QfcCollectionController.cs` 2299, `EfcItemController.cs` 1168, `EfcFormController.cs` 1014, `RibbonController.cs` 986, `QfcDatamodel.cs` 764, `KeyboardHandler.cs` 605, `ToDoEvents.cs` 594). These are **pre-existing** sizes; this change adds only two lines (`using` + attribute) per file and does not introduce or materially worsen the violation. Not attributable to this feature; recorded as a pre-existing observation, not a FAIL for #197. |
| **Public vs internal** | ✅ PASS | No visibility changes. `[ExcludeFromCodeCoverage]` does not alter the public surface (spec §Invariants). |
| **No circular dependencies** | ✅ PASS | No dependency changes; only a framework `using` directive added. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Descriptive names** | N/A | No new identifiers introduced. |
| **Docs/docstrings** | ✅ PASS | Policy docs (`CLAUDE.md`, `.claude/rules/general-unit-test.md`) updated with the exemption rationale, exclusion categories (a/b/c), mechanism, authority note, and explicit not-exempt seam list. |
| **Comment why, not what** | ✅ PASS | The policy-doc additions explain the rationale (COM-host binding, UT4 prohibition) rather than restating the attribute mechanics. |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| **1. Formatting** | ✅ PASS | **Command:** `dotnet tool run csharpier check .` **Result:** EXIT_CODE 0, no diff (`evidence/qa-gates/final-csharpier.md`, 2026-06-13T14-15). |
| **2. Linting** | ✅ PASS | **Command:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` **Result:** EXIT_CODE 0 (`evidence/qa-gates/final-analyzer.md`, 2026-06-13T14-16). |
| **3. Type checking** | ✅ PASS | **Command:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` **Result:** EXIT_CODE 0 (`evidence/qa-gates/final-nullable.md`, 2026-06-13T14-17). |
| **4. Testing** | ✅ PASS | **Command:** `pwsh -NoProfile scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput coverage/coverage.final.cobertura.xml` **Result:** 4066/4068 pass; 2 pre-existing flaky failures, identical to baseline (`evidence/qa-gates/final-mstest-coverage.md`, `evidence/qa-gates/test-result-parity.md`). |
| **Full toolchain loop** | ✅ PASS | Per-phase loops (phases 1–6) and a final loop all green; documented across `evidence/qa-gates/phase*-{csharpier,analyzer,nullable,mstest}.md` and `final-*.md`. |
| **Explicit reporting** | ✅ PASS | Commands and results are recorded in the QA-gate evidence artifacts and surfaced in the PR context summary verification section. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Summarize changes** | ✅ PASS | Summarized in `spec.md`, `coverage-delta.md`, and the executor's per-phase artifacts. |
| **Design choices explained** | ✅ PASS | The hybrid (assembly-exclude + class/method-attribute) choice and the rejection of Koverage tiering (Option C) are documented in spec §Scope/§Non-Goals. |
| **Update supporting documents** | ✅ PASS | `CLAUDE.md`, `.claude/rules/general-unit-test.md`, and `spec.md` AC/DoD checkboxes updated. |
| **Provide next steps** | ✅ PASS | Out-of-scope roadmap increment tests identified as the path to the 80% floor (`coverage-delta.md` remediation flag). |

---

## 3. Language-Specific Code Change Policy Compliance

### Section 3C#: C# Code Change Policy Compliance

#### C#1 Tooling & Baseline

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Formatting — csharpier** | ✅ PASS | `dotnet tool run csharpier check .` EXIT_CODE 0 (`final-csharpier.md`). File-based csharpier used; no `dotnet format`. |
| **Linting — .NET analyzers** | ✅ PASS | msbuild with `EnableNETAnalyzers=true /EnforceCodeStyleInBuild=true` EXIT_CODE 0 (`final-analyzer.md`). |
| **Type checking — nullable** | ✅ PASS | msbuild with `Nullable=enable /TreatWarningsAsErrors=true` EXIT_CODE 0 (`final-nullable.md`). |

#### C#2–C#7 Design / Structure / Naming / Dependencies

| Requirement | Status | Evidence |
|------------|--------|----------|
| **No behavior/contract change** | ✅ PASS | `[ExcludeFromCodeCoverage]` is a non-behavioral diagnostic attribute. Diff inspection confirms only attribute + `using` additions; no method bodies, signatures, or member visibility changed (spec §Invariants; verified across all 29 `.cs` files in the diff). |
| **No new dependencies** | ✅ PASS | `System.Diagnostics.CodeAnalysis` is part of the framework BCL; no package added. |
| **Naming conventions** | N/A | No new members. |
| **File size** | ⚠️ PARTIAL | See §2.3 — pre-existing oversize files, not introduced by this change. |

---

## 4. Language-Specific Unit Test Policy Compliance

No unit tests were added or modified. C# Unit Test Policy (CUT1–CUT3) applies to test changes; none exist in this branch. The existing MSTest suite served as the unchanged behavior regression guard and was confirmed green (4066/4068, identical pre/post failing set). Sections 4A (Python) and 4B (PowerShell) are not applicable — no Python or PowerShell test files changed.

---

## 5. Test Coverage Detail

No new tests were authored; there is no per-test coverage detail to report for this feature. The relevant coverage evidence is the denominator change documented in §1.2.1 and `coverage-delta.md`:

- Baseline production-only deduped denominator: 65,768 lines-valid, 38,820 covered (59.03%).
- Post-exemption denominator: 51,594 lines-valid, 37,010 covered (71.73%).
- Lines-valid removed: 14,174 (TaskVisualization assembly + 25 annotated classes + 4 IDList members).
- Covered lines removed: 1,810.
- Exemption boundary verified exact against design memo §2 (`exemption-boundary-verification.md`): 0 testable seams exempted; all enumerated COM/VSTO/WinForms targets exempted.

---

## 6. Test Execution Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Tests | 4068 | ✅ |
| Tests Passed | 4066 (99.95%) | ✅ |
| Tests Failed | 2 (pre-existing flaky timing; identical set pre/post) | ⚠️ pre-existing |
| Execution Time | Not separately recorded by the pipeline artifact | N/A |
| Functions/Classes Tested | Unchanged from baseline | N/A |
| Code Coverage | 71.73% lines (production-only deduped testable denominator) | ⚠️ below 80% forward floor; expected per ratified spec |

---

## 7. Code Quality Checks

**For C#:**

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| CSharpier Formatting | `dotnet tool run csharpier check .` | EXIT_CODE 0, no diff | ✅ |
| .NET Analyzers + Code Style | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | EXIT_CODE 0 | ✅ |
| Nullable + Warnings-as-Errors | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` | EXIT_CODE 0 | ✅ |
| MSTest Suite with Coverage | `pwsh -NoProfile scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput coverage/coverage.final.cobertura.xml` | 4066/4068 pass | ✅ |

**Notes:**
The 2 failing tests (`AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException`, `RequestTask_WithProvidedTask_InvokesTaskAfterInterval`) are pre-existing flaky timing/threading tests recorded at the Phase 0 baseline (roadmap §0.1). The failing set is identical before and after this change, confirming behavior parity. The pipeline returns a non-zero exit code on these failures; the coverage Cobertura is still produced and re-deduped.

---

## 8. Gaps and Exceptions

### Identified Gaps

- **AC4 (post-exemption rate within design memo §3 range):** The measured rate is 71.73%, 1.47 pp below the §3 lower bound (73.2%). Root cause (per `coverage-delta.md`): fewer lines-valid were removed than the §3 midpoint (14,174 vs 15,326), while more covered lines were removed than estimated (1,810 vs ~833) because several annotated controllers/viewers carried more incidentally-covered lines than the per-assembly estimate assumed. The exemption **scope** is correct (boundary verified exact against §2); only the §3 numeric estimate was optimistic. This is an estimate-accuracy deviation, not an implementation defect.

  - Disposition: **Non-blocking.** The §3 figures are explicitly labeled "estimates"/"range" in the spec; the authoritative deliverable of #197 is the exemption mechanism and its correct boundary, both of which PASS. The maintainer-ratified spec (§Risks "Floor still not reached") already states the post-exemption rate is expected to be below 80% and that reaching the floor requires the out-of-scope roadmap increment tests. A 1.47 pp shortfall against an estimated range — with the boundary verified correct — does not indicate incorrect exemption work and does not change behavior parity. It is recorded as a deviation requiring maintainer awareness, not code remediation.

- **Repo-wide testable-denominator coverage (71.73%) is below the 80% forward floor.** This is the expected, ratified outcome of an exemption-only feature; closing the remaining gap is the explicitly out-of-scope roadmap increment work (spec §Non-Goals). Not a #197 defect.

### Approved Exceptions

- **No unit tests added for exempted COM/VSTO/WinForms code.** Justification: the exempted classes require a live Outlook COM host / WinForms runtime and cannot be unit-tested without violating UT4 (external-dependency prohibition). This is the maintainer-ratified premise of the feature (CLAUDE.md UT2 exemption text; ratified 2026-06-13).

### Removed/Skipped Tests

- **None.** No tests were removed or skipped. The full suite remained the unchanged regression guard.

---

## 9. Summary of Changes

### Commits in This PR/Branch

Branch `refactor/com-vsto-coverage-exemption-197`, head `a564add0`, range `1b3f5350..a564add0`. (Individual commit hashes not enumerated in PR context; the diff is the authoritative artifact.)

### Files Modified

1. **`coverage.config`, `TaskMaster.runsettings`** (MODIFIED) — add `<ModulePath>.*TaskVisualization.*</ModulePath>` to `ModulePaths/Exclude`. Removes the `TaskVisualization` assembly from instrumentation.
2. **25 `.cs` files** (MODIFIED) — class-level `[ExcludeFromCodeCoverage]` + `using System.Diagnostics.CodeAnalysis;` on enumerated COM/VSTO/WinForms classes in QuickFiler (14), TaskMaster (6), ToDoModel (5 class-level), Tags (2).
3. **`ToDoModel/Data Model/ID/IDList.cs`** (MODIFIED) — method-level `[ExcludeFromCodeCoverage]` on 2 Outlook-dependent constructors and 2 `RefreshIDList` overloads; `GetNextToDoID` deliberately left unannotated.
4. **`CLAUDE.md`, `.claude/rules/general-unit-test.md`** (MODIFIED) — record the COM/VSTO/WinForms exemption policy, testable-denominator definition, and authority note.
5. **4 `.claude/agent-memory/` files** (MODIFIED/NEW) — atomic-executor / atomic-planner memory notes. Non-policy memory files; editing them is not a policy-document violation.
6. **`docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/**`** (NEW) — issue.md, spec.md, plan, and evidence artifacts.

---

## 10. Compliance Verdict

### Overall Status: ⚠️ PARTIALLY COMPLIANT

The implementation is non-behavioral, fully toolchain-green, and the exemption boundary is verified exact against the design memo §2. The only unmet item (AC4) is an estimate-range deviation, not an implementation defect, and the sub-80% testable-denominator rate is the maintainer-ratified expected outcome of an exemption-only feature. This audit identifies no blocking policy FAIL: the AC4 deviation and the sub-80% rate are both authority-scoped, expected outcomes documented in the ratified spec. The branch is ready for normal PR flow with the AC4 deviation recorded for maintainer awareness.

**Fail-closed reminder:** All required baseline and post-change C# coverage artifacts are present (paths listed in the Coverage Evidence Checklist), and numeric baseline/post-change metrics are reported. No required artifact is missing.

---

### Policy-by-Policy Summary

#### General Code Change Policy (Section 2)
- ✅ Before Making Changes: objective, plan, and policy-read evidence present.
- ✅ Design Principles: simplest boundary-preserving mechanism.
- ⚠️ Module & File Structure: pre-existing oversize files, not introduced here.
- ✅ Naming, Docs, Comments: policy docs updated with rationale.
- ✅ Toolchain Execution: csharpier/analyzer/nullable/MSTest all green in final pass.
- ✅ Summarize & Document: spec/docs/evidence updated.

#### Language-Specific Code Change Policy (Section 3)

**For C#:**
- ✅ Tooling & Baseline: csharpier + msbuild analyzer + nullable all EXIT_CODE 0.
- ✅ Design & Type-Safety: no behavior/contract/visibility change.
- ✅ Dependencies/Analyzer Config: BCL attribute only; no new package.

#### General Unit Test Policy (Section 1)
- N/A Core Principles: no test changes (2 pre-existing flaky tests noted).
- ⚠️ Coverage & Scenarios: 71.73% testable-denominator (below 80% forward floor, expected and ratified).
- N/A Test Structure: no test changes.
- N/A External Dependencies: no test changes.
- ✅ Policy Audit: this document.

#### Language-Specific Unit Test Policy (Section 4)
- N/A C#/Python/PowerShell: no test files changed.

---

### Metrics Summary

- ✅ 4066/4068 tests passing (99.95%); identical failing set pre/post (behavior parity).
- ⚠️ 71.73% line coverage on the redefined testable denominator (below 80% forward floor; expected per ratified spec).
- ✅ Exemption boundary verified exact against design memo §2 (0 testable seams exempted).
- ✅ All four C# code-quality checks passing (csharpier, analyzers, nullable, MSTest).

---

### Recommendation

**Ready for merge (with recorded AC4 deviation).**

The feature delivers a correct, non-behavioral, toolchain-green exemption mechanism with a verified boundary. The single unmet acceptance criterion (AC4) is a numeric estimate-range deviation that does not reflect an implementation defect; the maintainer has already ratified that the post-exemption rate would fall below 80% and that the §3 figures are estimates. No code remediation is required for #197. Recommended follow-up (out of scope for #197): the roadmap increment tests to raise the testable denominator toward the 80% floor; the maintainer should note the starting point is 71.73% rather than the estimated ~75.2%.

---

## Appendix A: Test Inventory

No tests were added or modified by this feature. The complete existing test inventory (4068 MSTest tests) is unchanged. The two pre-existing flaky tests are:
- `AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException`
- `RequestTask_WithProvidedTask_InvokesTaskAfterInterval`

---

## Appendix B: Toolchain Commands Reference

**For C#:**
```powershell
# Formatting
dotnet tool run csharpier check .

# Linting / analyzers + code style
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

# Type checking (nullable + warnings as errors)
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true

# Testing with coverage
pwsh -NoProfile scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput coverage/coverage.final.cobertura.xml
```

**Reviewer scope/evidence commands:**
```bash
git diff --name-only 1b3f5350..HEAD -- '*.cs'
git diff 1b3f5350..HEAD -- '*.cs' 'coverage.config' 'TaskMaster.runsettings' 'CLAUDE.md' '.claude/rules/general-unit-test.md'
git diff --name-only 1b3f5350..HEAD | grep -E '^artifacts/(baselines?|qa|qa-gates|evidence|coverage|regression-testing|post-change)/'
```

---

**Audit Completed By:** feature-review agent
**Audit Date:** 2026-06-13
**Policy Version:** Current (as of audit date)
