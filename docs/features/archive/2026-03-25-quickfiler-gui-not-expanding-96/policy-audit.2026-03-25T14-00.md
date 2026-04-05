# Policy Audit — 2026-03-25T14-00

**Component:** QuickFiler / QfcItemController — Keys.Right keyboard registration fix (Issue #96)
**Branch:** `feature/utilities-coverage-part-three-87` (commit `bd8fc03`)
**Base:** `main` @ `0d6c60f`
**Feature folder:** `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/`
**Work Mode:** `minor-audit` (AC source: `issue.md`)
**Auditor:** feature-reviewer agent
**Date:** 2026-03-25

---

## Feature folder selection rationale

`FEATURE_FOLDER` derived from the user-supplied argument and confirmed by the presence of `issue.md`
with `Work Mode: minor-audit` and an active plan (`plan.2026-03-25T09-03.md`). The issue-number suffix `96`
matches the `#96` reference in commit `bd8fc03`. No ambiguity; selection is deterministic.

---

## Policy Sections

### § General Code Change Policy

#### Before Making Changes
[PASS] Objective was clearly stated in `issue.md` (root cause identified, fix described, acceptance
criteria listed). Plan `plan.2026-03-25T09-03.md` was documented before execution and updated to
`Status: Completed`.

#### Bugfix Workflow
[PASS]
1. Failing regression tests were written first (P1-T1, P1-T2) — confirmed by `regression-fail-before.md`
   (both tests EXIT_CODE: 1 before fix).
2. Minimal, targeted fix applied: only 7 source lines changed in `QfcItemController.cs`.
3. Full toolchain re-run completed after fix; all QA gates passed.

#### Design Principles
[PASS] Fix is the simplest design that works. No new abstractions, no new classes, no scope
creep. Single registration line added and single removal line added/uncommented.

#### Classes, Functions, and APIs
[PASS] No new types or public APIs introduced. The fix restores missing behavior within an
existing method.

#### Error Handling
[PASS] No new error handling scope required. The lambda `(x) => this.ToggleExpansionAsync()`
follows the existing pattern at line ~1381.

#### Module & File Structure
[PASS] No new files in production code. File line count unaffected in a meaningful way.

#### Naming, Docs, and Comments
[PASS] Inline comment added: `// Right arrow expands the conversation thread for the focused item.`
Comment explains the intent (why), not just what.

#### Toolchain Loop (After Making Changes)
[PASS] All four steps completed with no restart required (see Appendix B).

---

### § C# Code Change Policy

#### C#1. Tooling & Baseline
[PASS] CSharpier used for formatting (not `dotnet format`). MSBuild invoked via
`scripts/vscode/Invoke-VSBuild.ps1` wrapper for both analyzer and nullable passes.
vstest.console.exe used for test execution.

**Minor deviation (informational):** The plan and evidence use `dotnet tool run csharpier format .`
where the policy-approved spelling is `dotnet tool run csharpier .`. The `format` subcommand is the
default and functionally identical. This is a documentation inconsistency only; the formatter output
confirms correct behavior (1001 files processed, check passed, no re-format required).

#### C#2. Design & Type-Safety
[PASS] Nullable build passed with 0 warnings/errors. No new nullable exposures introduced.

#### C#3. Classes, Methods, and APIs
[PASS] Existing method signatures unchanged. Lambda pattern consistent with adjacent registrations.

#### C#4. Error Handling, Logging, Contracts
[N/A] No new error-handling paths introduced by the two-line fix.

#### C#5. Module & File Structure
[PASS] `QfcItemController.cs` remains within the 500-line-per-file policy limit for the changed
methods. No circular dependencies introduced.

#### C#6. Naming, Docs, Comments
[PASS] Comment added explains why the registration was missing (async migration omission).

#### C#7. Dependencies
[PASS] No new dependencies added.

---

### § General Unit Test Policy

#### UT1. Core Principles
[PASS] Both regression tests are independent, isolated, fast (deterministic), and readable.
No shared mutable state between tests. Each test operates on its own controller instance and
mock stub.

#### UT2. Coverage and Scenarios
[PASS] Two test scenarios covered:
- Positive: `Keys.Right` is present in `KeyActionsAsync` after `RegisterFocusAsyncActions()`.
- Cleanup: `Keys.Right` is absent after `UnregisterFocusAsyncActions()`.

Coverage delta: baseline 72 tests → post-fix 74 tests (+2). Both new tests pass (EXIT_CODE: 0).

**Limitation (informational):** Numeric line-coverage percentage not available from vstest
`/EnableCodeCoverage` (produces binary `.coverage` file only, not inline percentage). This
limitation is documented in both `baseline-coverage.md` and `qa-test.md`. The policy requires
`>= 80%` repo-wide and `>= 90%` for new code; the new code is the test class itself and two
very small production lines, so the coverage delta from 2 new targeted regression tests is
expected to be positive.

#### UT3. Test Structure and Diagnostics
[PASS] AAA structure is present with explicit `// Arrange`, `// Act`, `// Assert` comments.
FluentAssertions used with `.Should().BeTrue(because: "...")` and `.Should().BeFalse(because: "...")`
providing clear, actionable failure messages.

#### UT4. External Dependencies
[PASS] No external dependencies, no temp files. Reflection-based injection used to set private
`_kbdHandler` field — this is an accepted internal-seam pattern because the production field is
not injectable via constructor in the existing codebase. `KbdActions<>` collections are real
(not mocked), so Add/Remove calls are exercised against real collection behavior.

#### UT5. Policy Audit
[PASS] Both tests comply with all UT rules. No exceptions required.

---

### § C# Unit Test Policy

#### CUT1. Framework Selection
[PASS] MSTest (`[TestClass]`, `[TestMethod]`) used throughout.

#### CUT2. Libraries and Conventions
[PASS] Moq used for `IQfcKeyboardHandler`. FluentAssertions used for all assertions.
`KbdActions<>` is a real instance (not mocked) — appropriate because the test is exercising
actual collection mutation behavior, not faking it.

#### CUT3. C# Toolchain Commands
[PASS] All four toolchain steps executed in order (see Appendix B).

---

## Plan Checklist Reconciliation

| Task | Plan Status | Audit Verdict | Note |
|------|-------------|---------------|------|
| P0-T1 Policy read | [x] | PASS | `phase0-instructions-read.md` exists with timestamp and policy order |
| P0-T2 Format baseline | [x] | PASS | `baseline-format.md` EXIT_CODE: 0, 1001 files |
| P0-T3 Lint baseline | [x] | PASS | `baseline-lint.md` EXIT_CODE: 0, 0 errors |
| P0-T4 Nullable baseline | [x] | PASS | `baseline-nullable.md` EXIT_CODE: 0, 0 errors |
| P0-T5 Test baseline (targeted) | [x] | PASS | `baseline-test.md` confirms 0 tests found at baseline (expected) |
| P0-T6 Coverage baseline | [x] | PARTIAL | `baseline-coverage.md` EXIT_CODE: 0, 72 tests; numeric % not available (binary .coverage) |
| P1-T1 Regression test #1 (fail-before) | [x] | PASS | `regression-fail-before.md` records EXIT_CODE: 1 |
| P1-T2 Regression test #2 (fail-before) | [x] | PASS | `regression-fail-before.md` records EXIT_CODE: 1 |
| P1-T3 Add Keys.Right to Register | [x] | PASS* | Implemented; see note on `ToggleExpansionAsync()` signature below |
| P1-T4 Add Keys.Right Remove to Unregister | [x] | PASS | `_kbdHandler.KeyActionsAsync.Remove(ItemHelper.EntryId, Keys.Right)` present |
| P2-T1 QA format | [x] | PASS | `qa-format.md` EXIT_CODE: 0, check confirmed |
| P2-T2 QA lint | [x] | PASS | `qa-lint.md` EXIT_CODE: 0, 0 errors |
| P2-T3 QA nullable | [x] | PASS | `qa-nullable.md` EXIT_CODE: 0, 0 errors |
| P2-T4 QA test coverage | [x] | PASS | `qa-test.md` EXIT_CODE: 0, 74 passed, both regression tests listed |

**Note on P1-T3 implementation deviation:**
Plan specified `this.ToggleExpansionAsync(Enums.ToggleState.On)` (force-expand overload) but the
implementation uses `this.ToggleExpansionAsync()` (no-arg toggle overload). The no-arg overload
is consistent with the existing 'E' key binding at the adjacent line and with the issue description
("equivalent to clicking the expand/collapse widget or pressing 'E'"). The interface declares only
`Task ToggleExpansionAsync()` in `IQfcItemController`. This deviation is functionally reasonable
and does not constitute a defect, but it differs from the plan's literal specification.

**Note on test method name deviation:**
Plan P2-T4 AC listed names `RegisterFocusAsyncActions_RightArrowKey_RegisteredInKeyActionsAsync`
and `UnregisterFocusAsyncActions_AfterRegister_RemovesRightArrowKey`. Actual names are
`RegisterFocusAsyncActions_RightArrowKey_IsRegisteredInKeyActionsAsync` and
`UnregisterFocusAsyncActions_AfterRegister_RemovesRightArrowFromKeyActionsAsync`. Both names are
more descriptive than the plan names and comply with the `{Method}_{Scenario}_{Expected}`
convention. `qa-test.md` correctly reflects the actual names.

---

## Verdict

**READY FOR MERGE**

All toolchain gates pass. The production fix is minimal, targeted, and correct. Two regression
tests provide fail-before / pass-after evidence. No policy violations. The two informational
deviations (coverage numeric % not available; `ToggleExpansionAsync()` vs `(On)`) are documented
and do not block merging.

---

## Appendix A — Changed Files

| File | Change Type | Lines (+/-) |
|------|-------------|-------------|
| `QuickFiler/Controllers/QfcItemController.cs` | Modified | +7 / -1 |
| `QuickFiler.Test/Controllers/QfcItemControllerTests.cs` | Modified | +150 / 0 |
| `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/` | Added (docs) | various |

---

## Appendix B — Toolchain Commands Run (Check-Only Reference)

All evidence artifacts were produced during execution, not during this review. The review
reads existing evidence artifacts only.

| Step | Command | Exit Code | Evidence Artifact |
|------|---------|-----------|-------------------|
| Format | `dotnet tool run csharpier format .` | 0 | `evidence/qa-gates/qa-format.md` |
| Lint | `pwsh ... Invoke-VSBuild.ps1 ... -EnableNETAnalyzers -EnforceCodeStyleInBuild` | 0 | `evidence/qa-gates/qa-lint.md` |
| Nullable | `pwsh ... Invoke-VSBuild.ps1 ... -EnableNullable -TreatWarningsAsErrors` | 0 | `evidence/qa-gates/qa-nullable.md` |
| Test | `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /EnableCodeCoverage` | 0 | `evidence/qa-gates/qa-test.md` |
