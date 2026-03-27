# Code Review — 2026-03-25T14-00

**Feature folder:** `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/`
**Branch:** `feature/utilities-coverage-part-three-87` (commit `bd8fc03`)
**Base:** `main` @ `0d6c60f`
**Work Mode:** `minor-audit`
**Reviewer:** feature-reviewer agent
**Date:** 2026-03-25

---

## 1. Executive Summary

**What changed:**
- `QuickFiler/Controllers/QfcItemController.cs`: 7 lines added/changed to restore the `Keys.Right`
  keyboard registration that was dropped during the async migration. `RegisterFocusAsyncActions()`
  now adds `Keys.Right → ToggleExpansionAsync()` and `UnregisterFocusAsyncActions()` now removes it.
- `QuickFiler.Test/Controllers/QfcItemControllerTests.cs`: 150 lines added — a new
  `QfcItemController_KeyboardRegistrationTests` class with two regression tests covering the
  register and unregister flows for `Keys.Right`.

**Top 3 risks:**

1. **`ToggleExpansionAsync()` vs `ToggleExpansionAsync(Enums.ToggleState.On)` (Low risk):**
   The implementation toggles (expand ↔ collapse) rather than always forcing expand. The issue
   description says "equivalent to pressing 'E'" and the 'E' binding also uses the no-arg overload,
   so toggle is consistent. However, the plan specified `.On`. If the product intent is Right-arrow
   should always expand (not collapse an already-expanded item), the current implementation would
   allow Right-arrow to collapse. This is a minor behavioral nuance, not a defect.

2. **Reflection-based field injection in tests (Acceptable):**
   `KeyboardRegistrationQfcItemController` uses `typeof(QfcItemController).GetField("_kbdHandler", ...)`
   to inject the mock handler. This is a brittle seam — if `_kbdHandler` is renamed or moved to a
   base class, the test silently throws a NullReferenceException at runtime rather than a compile
   error. The comment documents the constraint (private field, no constructor injection). Risk is
   low for a stable legacy type, but reviewers should be aware that refactoring `_kbdHandler`
   requires updating the test.

3. **Coverage numeric gap (Informational):**
   The plan requires a numeric line-coverage percentage in both the baseline and QA coverage artifacts.
   The binary `.coverage` format produced by vstest does not print an inline percentage to stdout.
   Both artifacts document this limitation explicitly. Repository-wide coverage has not regressed
   (74 tests, all passed vs 72 at baseline), and the fix adds 2 deterministic targeted tests for 7
   new production lines.

**Go/No-Go Recommendation:** **Go.** The change is minimal, correctly targeted, and well-evidenced.
All toolchain gates pass. The two deviations from the plan (toggle vs. force-expand; method naming)
are informational and do not block merge.

---

## 2. Findings Table

| # | Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|----------|------|----------|---------|---------------|-----------|---------|
| F-01 | Minor | `QfcItemController.cs` | Line ~1345 | `ToggleExpansionAsync()` (toggle) used instead of plan-specified `ToggleExpansionAsync(Enums.ToggleState.On)` (force-expand). | Confirm with product owner whether Right-arrow should always expand or toggle. If always-expand is intended, change to `(Enums.ToggleState.On)`. | Issue says "equivalent to pressing 'E'"; 'E' uses toggle; the current behavior is self-consistent. | `git show bd8fc03 -- QuickFiler/Controllers/QfcItemController.cs` |
| F-02 | Nit | `QfcItemControllerTests.cs` | `KeyboardRegistrationQfcItemController` ctor | Reflection-based field injection (`GetField("_kbdHandler")`) is fragile to rename. | Add a `// NOTE: if _kbdHandler is renamed, update this string` comment, or make the field internal for testability in a future refactor. | Low immediate risk; `_kbdHandler` is stable in this legacy type. | Code diff |
| F-03 | Nit | Plan / evidence | `baseline-coverage.md`, `qa-test.md` | Numeric line-coverage percentage not captured (binary `.coverage` only). | Acceptable as documented. Future runs should consider a coverage report converter step. | vstest limitation is known; evidence explains it. | `baseline-coverage.md` note section |

No Blockers. No Major findings.

---

## 3. Test Quality Audit

| Criterion | Status | Notes |
|-----------|--------|-------|
| Framework: MSTest | PASS | `[TestClass]`, `[TestMethod]` throughout |
| Mocking: Moq | PASS | `Mock<IQfcKeyboardHandler>` with real `KbdActions<>` instances |
| Assertions: FluentAssertions | PASS | `.Should().BeTrue(because:...)` and `.Should().BeFalse(because:...)` |
| AAA structure | PASS | `// Arrange`, `// Act`, `// Assert` comments present |
| Independence | PASS | Each test creates its own controller and mock stub |
| Isolation | PASS | No shared state; no external services; no temp files |
| Determinism | PASS | Pure in-memory operations |
| Fast execution | PASS | 2 new tests run in < 1s combined |
| Failure messages | PASS | FluentAssertions `because:` string explains exactly what must be true and why |
| Fail-before evidence | PASS | Both tests fail before fix (EXIT_CODE: 1 in `regression-fail-before.md`) |
| Pass-after evidence | PASS | Both tests pass after fix (74/74 in `qa-test.md`) |
| No temp files | PASS | |
| No external dependencies | PASS | |
| Coverage for new code | PASS | New tests directly exercise the 7 changed production lines |

---

## 4. Security / Correctness

| Check | Status | Notes |
|-------|--------|-------|
| No secrets in code | PASS | No credentials, tokens, or paths |
| No unsafe subprocess usage | PASS | No process spawning |
| Input validation at boundaries | N/A | Key registration is an internal framework call, not a user-input boundary |
| No new COM interop surface | PASS | Fix reuses existing `_kbdHandler.KeyActionsAsync` collection |
| Correct cleanup on unregister | PASS | `Remove` is symmetric with `Add`; verified by regression test |
