# Policy Compliance Audit: PR #190 CI-failure remediation (cycle 1) — `.csharpierignore` project-file exclusion

**Audit Date:** 2026-06-13
**Code Under Test:** `.csharpierignore` (repo root) — appended `*.csproj`, `*.props`, `*.targets` globs with a 3-line rationale comment. This is the entire functional change of cycle 1.

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|----------|--------------|-------|-------------|-------------------|---------------------|-------------------|
| C# | 0 files | N/A | N/A | N/A (no `.cs` changed) | N/A (no `.cs` changed) | N/A |
| PowerShell | 0 files | N/A | N/A | N/A (no `.ps1` changed) | N/A (no `.ps1` changed) | N/A |
| TypeScript | 0 files | N/A | N/A | N/A (no `.ts` changed) | N/A (no `.ts` changed) | N/A |

**Note:** The sole changed file in the cycle-1 working tree is `.csharpierignore`, a CSharpier ignore-list configuration file. No source file in any language (`.cs`, `.ps1`, `.ts`, `.py`) was modified, added, or removed in this cycle. There are therefore zero changed files in any language category, so all coverage rows are N/A by the "zero changed files on the branch" exemption in the Coverage Verification rules. Coverage cannot regress because no executable source line changed.

### Coverage Evidence Checklist

- TypeScript baseline coverage artifact: `N/A - out of scope` (no `.ts` files changed in cycle 1)
- TypeScript post-change coverage artifact: `N/A - out of scope` (no `.ts` files changed in cycle 1)
- PowerShell baseline coverage artifact: `N/A - out of scope` (no `.ps1` files changed in cycle 1)
- PowerShell post-change coverage artifact: `N/A - out of scope` (no `.ps1` files changed in cycle 1)
- Per-language comparison summary: see Section 1.2.1 below

**Non-negotiable verdict rule:** No policy audit may report PASS unless it includes numeric baseline and post-change coverage metrics for every language in scope, plus changed/new-code coverage when required. In this cycle no language is in scope for coverage (zero changed source files), so the numeric-metric requirement is satisfied vacuously and documented as N/A.

**Fail-closed rule:** If any required baseline artifact, QA artifact, or coverage-comparison artifact is missing, the verdict must be BLOCKED or INCOMPLETE, never PASS. All cycle-1 evidence artifacts referenced below were inspected and are present.

**Evidence rule:** No audit evidence was synthesized or backfilled. Every verdict cites a cycle-1 evidence artifact or a direct `git diff` / file inspection performed during this review.

---

## Rejected Scope Narrowing

The cycle inputs and plan correctly state that the cycle-1 *functional change* is a single file (`.csharpierignore`). This is a factual scope description of what changed, not an instruction to narrow the audit, and it matches the verified branch working-tree diff. The reviewer independently confirmed via `git diff --stat HEAD` that `.csharpierignore` is the only modified tracked file (6 insertions). No caller instruction attempted to suppress a coverage check or mark a language with changed files as out of scope. No scope narrowing was detected, and none was rejected.

The audit scope (full branch diff working-tree change for cycle 1) was honored: every changed file was inspected. The branch additionally carries the committed #188/#189 change set, which was already adjudicated in the cycle-0 artifacts (`policy-audit.2026-06-12T20-04.md`, `code-review.2026-06-12T20-04.md`, `feature-audit.2026-06-12T20-04.md`); cycle 1 introduces no further source change to those files.

---

## Evidence Location Compliance

All cycle-1 evidence artifacts are written under the canonical `<FEATURE>/evidence/<kind>/` path:

- `evidence/baseline/csharpier-check-before.2026-06-13T01-05.md`
- `evidence/baseline/csharpierignore-preedit.2026-06-13T01-05.md`
- `evidence/baseline/phase0-instructions-read.2026-06-13T01-05.md`
- `evidence/qa-gates/csharpier-check-after.2026-06-13T01-05.md`
- `evidence/qa-gates/scope-and-cs-noregress.2026-06-13T01-05.md`
- `evidence/qa-gates/ci-rerun-required.2026-06-13T01-05.md`

No cycle-1 evidence was written to a non-canonical path (`artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, `artifacts/evidence/`). The branch working-tree diff contains no file under those paths. No evidence-location violation found. EVIDENCE_LOCATION_OVERRIDE_REJECTED: none.

---

## Executive Summary

Cycle 1 is a CI-failure remediation. The PR #190 required check "Format, build, analyze, and test" failed at its "Verify formatting" step (`dotnet csharpier check .`) because CSharpier v1 began inspecting `.csproj` files and 8 pre-existing repository project files do not end with a single trailing newline. The failure is pre-existing and repo-wide; `origin/main` fails the same gate today (verified via the cycle inputs and the documented root cause). No `.csproj` was modified on this branch, so the failure is not introduced by the feature work.

The user-approved fix appends `*.csproj`, `*.props`, `*.targets` to `.csharpierignore` with a rationale comment, restoring the file-type scope that CLAUDE.md C#1 already documents ("`csharpier` is file-based and formats only `*.cs` without touching project files"). Evidence confirms the gate transitioned from exit 1 (8 `.csproj` failures, 1060 files checked) to exit 0 (1040 files checked, zero failures of any file type). The 20-file delta equals the now-excluded project files. No `.cs` formatting regressed.

Only the CSharpier format/verify gate is applicable to this change. The analyzer/build, nullable/type-check, and test/coverage gates are N/A because no compiled source (`.cs`) or build input changed; this is documented and justified, not skipped to narrow scope.

**Policy documents evaluated:**
- [PASS] `CLAUDE.md` — General Code Change Policy, C# Code Change Policy (C#1 tooling scope), Module & File Structure
- [PASS] `.claude/rules/general-code-change.md`
- [PASS] `.claude/rules/general-unit-test.md` (no tests in scope; coverage requirements evaluated as N/A)

**Language-specific policies evaluated:**
- N/A Python: no `.py` files changed in cycle 1
- N/A PowerShell: no `.ps1` files changed in cycle 1
- N/A C# source: no `.cs` files changed in cycle 1; the change is to a CSharpier configuration file, evaluated against CLAUDE.md C#1

**Temporary artifacts cleanup:**
- [PASS] No temporary or throwaway scripts were created during cycle 1; the change is a single configuration-file edit.
- [PASS] No ongoing tooling scripts were added.

---

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Independence** | N/A | No tests added or modified in cycle 1. |
| **Isolation** | N/A | No tests added or modified in cycle 1. |
| **Fast Execution** | N/A | No tests added or modified in cycle 1. |
| **Determinism** | N/A | No tests added or modified in cycle 1. |
| **Readability & Maintainability** | N/A | No tests added or modified in cycle 1. |

### 1.2 Coverage and Scenarios

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Baseline Coverage Documented** | N/A | No source line changed; coverage baseline is not affected by an ignore-list edit. No `.cs`/`.ps1`/`.ts`/`.py` files changed in cycle 1. |
| **No Coverage Regression** | N/A | Coverage cannot regress: zero executable source lines changed. Verified via `git diff --stat HEAD` (only `.csharpierignore` changed) and the after-edit csharpier run reporting zero `.cs` formatting failures (`evidence/qa-gates/scope-and-cs-noregress.2026-06-13T01-05.md`). |
| **New Code Coverage ≥90%** | N/A | No new code files; the change adds 6 lines to a configuration file. |
| **Comprehensive Coverage** | N/A | No behavior under test was changed. |
| **Positive Flows** | N/A | No tests in scope. |
| **Negative Flows** | N/A | No tests in scope. |
| **Edge Cases** | N/A | No tests in scope. |
| **Error Handling** | N/A | No tests in scope. |
| **Concurrency** | N/A | No tests in scope. |
| **State Transitions** | N/A | No tests in scope. |

### 1.2.1 Per-Language Coverage Comparison

No language has changed source files in cycle 1; each checklist entry is retained with `N/A - out of scope` per template rule.

- C#: Baseline: N/A lines -> Post-change: N/A lines. Change: 0% lines. New/changed-code coverage: `N/A - out of scope`. Disposition: N/A. Evidence: `evidence/qa-gates/scope-and-cs-noregress.2026-06-13T01-05.md` (confirms zero `.cs` files changed).
- PowerShell: Baseline: N/A cmds -> Post-change: N/A cmds. Change: 0% cmds. New/changed-code coverage: `N/A - out of scope`. Disposition: N/A. Evidence: `git diff --stat HEAD` (no `.ps1` changed).
- TypeScript: Baseline: N/A lines -> Post-change: N/A lines. Change: 0% lines. New/changed-code coverage: `N/A - out of scope`. Disposition: N/A. Evidence: `git diff --stat HEAD` (no `.ts` changed).

### 1.3 Test Structure and Diagnostics

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clear Failure Messages** | N/A | No tests in scope. |
| **Arrange-Act-Assert Pattern** | N/A | No tests in scope. |
| **Document Intent** | N/A | No tests in scope. |

### 1.4 External Dependencies and Environment

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Avoid External Dependencies** | N/A | No tests in scope. |
| **Use Mocks/Stubs** | N/A | No tests in scope. |
| **Environment Stability** | N/A | No tests in scope; no temporary files created. |

### 1.5 Policy Audit Requirement

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Pre-submission Review** | [PASS] | This audit constitutes the required pre-submission policy review for the cycle-1 change. |

---

## 2. General Code Change Policy Compliance

### 2.1 Before Making Changes

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Clarify the objective** | [PASS] | Objective is precise: restore the CSharpier "C# source only" scope so the "Verify formatting" CI step passes. Documented in `remediation-inputs.2026-06-13T01-05.md`. |
| **Read existing change plans** | [PASS] | `remediation-plan.2026-06-13T01-05.md` exists and was followed; Phase 0 instruction-read evidence at `evidence/baseline/phase0-instructions-read.2026-06-13T01-05.md`. |
| **Document the plan** | [PASS] | `remediation-plan.2026-06-13T01-05.md` documents scope lock, toolchain applicability, and acceptance criteria. |

### 2.2 Design Principles

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Simplicity first** | [PASS] | The simplest available fix: three ignore globs plus a comment. The selected option avoids touching any `.csproj` (which would have widened scope) and avoids pinning/downgrading CSharpier. |
| **Reusability** | [PASS] | The globs are placed alongside existing ignore entries; consistent with the existing pattern. |
| **Extensibility** | N/A | No public API surface; configuration file. |
| **Separation of concerns** | [PASS] | The change cleanly separates project-file ownership (Visual Studio) from C# source formatting (CSharpier), matching CLAUDE.md C#1. |

### 2.3 Module & File Structure

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Cohesive modules** | [PASS] | `.csharpierignore` remains a single-purpose ignore list. |
| **Under 500 lines** | [PASS] | Post-edit `.csharpierignore` is 14 lines (verified by inspection of the current file). |
| **Public vs internal** | N/A | Configuration file; no API surface. |
| **No circular dependencies** | N/A | Configuration file; no imports. |

### 2.4 Naming, Docs, and Comments

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Descriptive names** | N/A | Glob entries are standard file extensions. |
| **Docs/docstrings** | N/A | Configuration file. |
| **Comment why, not what** | [PASS] | The added 3-line comment explains the rationale (project files are owned by Visual Studio and are not C# source; CSharpier formats C# source only per CLAUDE.md C#1), not merely what the globs do. |

### 2.5 After Making Changes - Toolchain Execution

| Requirement | Status | Evidence |
|------------|--------|----------|
| **1. Formatting** | [PASS] | `dotnet csharpier check .` after edit: EXIT_CODE 0, 1040 files checked, zero failures (`evidence/qa-gates/csharpier-check-after.2026-06-13T01-05.md`). Before edit: EXIT_CODE 1, 8 `.csproj` failures (`evidence/baseline/csharpier-check-before.2026-06-13T01-05.md`). |
| **2. Linting** | N/A | Analyzer/build gate not applicable: no `.cs`/build inputs changed. Justified in `remediation-plan.2026-06-13T01-05.md` Toolchain Applicability and `evidence/qa-gates/ci-rerun-required.2026-06-13T01-05.md`. |
| **3. Type checking** | N/A | Nullable/type-check gate not applicable: no `.cs` changed. |
| **4. Testing** | N/A | Test/coverage gate not applicable: no production or test code changed; coverage cannot regress. |
| **Full toolchain loop** | [PASS] | The only applicable gate (CSharpier verify) passed in a single pass after the edit; restart not required. |
| **Explicit reporting** | [PASS] | Commands, exit codes, and results are recorded in the cycle-1 evidence artifacts and in this audit. |

### 2.6 Summarize and Document

| Requirement | Status | Evidence |
|------------|--------|----------|
| **Summarize changes** | [PASS] | Change summarized in `remediation-inputs` and `remediation-plan`; the diff is 6 insertions to `.csharpierignore`. |
| **Design choices explained** | [PASS] | The two rejected alternatives (pin/downgrade CSharpier; add trailing newlines to `.csproj`) are documented in `remediation-inputs.2026-06-13T01-05.md` "Out of scope". |
| **Update supporting documents** | [PASS] | The cycle-1 evidence and plan artifacts document the change; no other runbook/README is affected by an ignore-list edit. |
| **Provide next steps** | [PASS] | Next step recorded: push the edit and confirm the required CI check re-runs green on the branch head (`evidence/qa-gates/ci-rerun-required.2026-06-13T01-05.md`). |

---

## 3. Language-Specific Code Change Policy Compliance

### Section 3 (C# configuration scope)

This cycle changed no `.cs` source. The relevant C# policy is CLAUDE.md C#1, which governs the CSharpier tool scope.

| Requirement | Status | Evidence |
|------------|--------|----------|
| **CSharpier formats C# source only (`*.cs`), not project files** (CLAUDE.md C#1) | [PASS] | The added globs exclude only `*.csproj`/`*.props`/`*.targets` from the CSharpier check. They contain no `.cs` pattern, so no C# source is excluded. The change restores the documented C#1 scope rather than weakening it. Verified by inspecting the post-edit `.csharpierignore` (globs `*.csproj`, `*.props`, `*.targets` only). |
| **Do not use `dotnet format` / do not rewrite `.csproj`** (CLAUDE.md C#1) | [PASS] | No `.csproj` was rewritten or reformatted. The fix excludes project files from the formatter rather than reformatting them, consistent with C#1's prohibition on tooling that rewrites `.csproj`. |
| **No weakening of C# source formatting enforcement** | [PASS] | `dotnet csharpier check .` still inspects all `.cs` files (1040 files checked after edit) and reported zero `.cs` formatting failures (`evidence/qa-gates/scope-and-cs-noregress.2026-06-13T01-05.md`). |

---

## 4. Language-Specific Unit Test Policy Compliance

No tests were added or modified in cycle 1. All language-specific unit-test requirements are N/A for this cycle.

---

## 5. Test Coverage Detail

No functions, classes, or modules were added or changed in cycle 1. No coverage detail applies.

---

## 6. Test Execution Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Tests | 0 (no tests in cycle-1 scope) | N/A |
| Tests Passed | 0 | N/A |
| Tests Failed | 0 | N/A |
| Execution Time | N/A | N/A |
| Functions/Classes Tested | N/A | N/A |
| Code Coverage (if applicable) | N/A (no source line changed) | N/A |

---

## 7. Code Quality Checks

The only applicable code-quality gate for this cycle is the CSharpier format/verify gate.

| Check | Command | Result | Status |
|-------|---------|--------|--------|
| CSharpier verify (before) | `dotnet csharpier check .` | EXIT 1; 8 `.csproj` trailing-newline failures; 1060 files | [PASS] (fail-before captured) |
| CSharpier verify (after) | `dotnet csharpier check .` | EXIT 0; 0 failures; 1040 files | [PASS] |
| C# source no-regression | `dotnet csharpier check .` (after) | 0 `.cs` files reported unformatted | [PASS] |

**Notes:** The 8 `.csproj` trailing-newline failures are pre-existing and repo-wide (the same gate fails on `origin/main`), not introduced by this branch. The 20-file delta between the before run (1060) and after run (1040) corresponds to the now-excluded project files.

---

## 8. Gaps and Exceptions

### Identified Gaps
**None.** All applicable policy requirements are met. Non-applicable gates (analyzer, nullable, test/coverage) are justified by the absence of any `.cs`/build-input change.

### Approved Exceptions
**None.** The N/A toolchain gates are not exceptions; they are inapplicable because no compiled source changed. The user approved the `.csharpierignore` exclusion approach (option 1) over the two rejected alternatives.

### Removed/Skipped Tests
**None.** No tests were planned, removed, or skipped in cycle 1.

---

## 9. Summary of Changes

### Commits in This PR/Branch

Cycle-1 change is uncommitted in the working tree at audit time. Branch HEAD is `ece2686649edae363c148be0751641b04a2ec1d2`; merge-base with `origin/main` is `aa63315bd432ffbf092cfbb5caa02ee673e7b326`. Commit/push is handled by the orchestrator.

### Files Modified

1. **`.csharpierignore`** (MODIFIED)
   - Appended a 3-line rationale comment plus globs `*.csproj`, `*.props`, `*.targets`.
   - 6 insertions; no deletions; no existing glob removed or reordered.
   - Restores CLAUDE.md C#1 "C# source only" CSharpier scope.

No other file was modified in cycle 1.

---

## 10. Compliance Verdict

### Overall Status: ✅ FULLY COMPLIANT

The cycle-1 `.csharpierignore` change correctly and minimally resolves the failing "Verify formatting" gate (exit 1 -> exit 0), aligns with CLAUDE.md C#1, does not weaken C# source formatting enforcement, and modifies no source code, project file, or workflow file. All required evidence artifacts are present at canonical evidence paths.

**Fail-closed reminder:** No required baseline artifact, QA artifact, or coverage-comparison artifact is missing. Coverage metrics are N/A because no language has changed source files in cycle 1, which is the documented "zero changed files" exemption — not a missing-evidence condition.

---

### Policy-by-Policy Summary

#### General Code Change Policy (Section 2)
- ✅ Before Making Changes: objective, plan, and prior plans documented
- ✅ Design Principles: simplest viable fix; correct separation of concerns
- ✅ Module & File Structure: file remains cohesive and 14 lines
- ✅ Naming, Docs, Comments: rationale comment explains why
- ✅ Toolchain Execution: applicable CSharpier gate passes; other gates justified N/A
- ✅ Summarize & Document: change and rejected alternatives documented

#### Language-Specific Code Change Policy (Section 3)
**For C# (configuration scope):**
- ✅ CSharpier scope restored to `*.cs` only, per CLAUDE.md C#1
- ✅ No `.csproj` rewritten; no `.cs` source excluded
- ✅ C# source formatting enforcement preserved (0 `.cs` failures after edit)

#### General Unit Test Policy (Section 1)
- N/A Core Principles, Coverage, Structure, Dependencies — no tests in cycle-1 scope
- ✅ Policy Audit requirement satisfied by this document

#### Language-Specific Unit Test Policy (Section 4)
- N/A — no tests added or modified

---

### Metrics Summary

- N/A test count — no tests in cycle-1 scope
- ✅ CSharpier verify: exit 1 -> exit 0 (8 `.csproj` failures cleared)
- ✅ 0 `.cs` files reported unformatted after edit
- ✅ Only `.csharpierignore` changed (6 insertions)
- N/A line coverage — no source line changed

---

### Recommendation

**Ready for merge** (subject to the final CI gate). The local CSharpier verify gate passes. After the orchestrator pushes the change, the required CI check "Format, build, analyze, and test" must re-run green on the branch head; that runner-side re-run is the final merge gate. The `modified-workflow-needs-green-run` rule does not apply because no workflow YAML changed.

---

## Appendix A: Test Inventory

No tests were added or modified in cycle 1. Test inventory is empty for this cycle.

---

## Appendix B: Toolchain Commands Reference

**CSharpier (the only applicable gate for cycle 1):**
```bash
# Verify formatting (the failing CI step), run from repo root
dotnet csharpier check .
# or, if the global tool is not on PATH:
dotnet tool run csharpier check .
```

**Scope confirmation:**
```bash
git diff --stat HEAD     # expect: .csharpierignore | 6 ++++++  (1 file changed)
```

---

**Audit Completed By:** feature-reviewer agent
**Audit Date:** 2026-06-13
**Policy Version:** Current (as of audit date)
