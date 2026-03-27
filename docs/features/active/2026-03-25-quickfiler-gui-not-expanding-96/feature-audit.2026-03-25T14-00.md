# Feature Audit — 2026-03-25T14-00

**Feature folder:** `docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/`
**Branch:** `feature/utilities-coverage-part-three-87` (commit `bd8fc03`)
**Base:** `main` @ `0d6c60f`
**Work Mode:** `minor-audit` → AC source: `issue.md`
**Auditor:** feature-reviewer agent
**Date:** 2026-03-25

---

## 1. Scope and Baseline

| Field | Value |
|-------|-------|
| Base branch | `main` @ `0d6c60f0de93d09276ca98b20e0ea41ff8fd5647` |
| Head commit | `bd8fc039eb08e2086b6137f0819a9850ae0d1b14` |
| AC source | `issue.md` (`Work Mode: minor-audit`) |
| Evidence sources used | `evidence/baseline/`, `evidence/regression-testing/`, `evidence/qa-gates/` |
| Production files changed | `QuickFiler/Controllers/QfcItemController.cs` (+7 lines) |
| Test files changed | `QuickFiler.Test/Controllers/QfcItemControllerTests.cs` (+150 lines) |

---

## 2. Acceptance Criteria Inventory

Extracted from `issue.md` § "Proposed Fix / Validation Ideas":

| ID | Criterion | Source Location |
|----|-----------|----------------|
| AC-1 | Add `Keys.Right → ToggleExpansionAsync(...)` to `RegisterFocusAsyncActions()` | `issue.md` line 62 |
| AC-2 | Uncomment / add `Keys.Right` removal in `UnregisterFocusAsyncActions()` | `issue.md` line 63 |
| AC-3 | Unit coverage: add tests asserting `Keys.Right` is present after `RegisterFocusAsyncActions()` and absent after `UnregisterFocusAsyncActions()` | `issue.md` line 64 |
| AC-4 | Integration scenario: manually reproduce in Outlook after deploying the fix | `issue.md` line 65 |
| AC-5 | Manual verification: confirm Right arrow expands conversation and mailto: is no longer triggered | `issue.md` line 66 |

---

## 3. Acceptance Criteria Evaluation

| ID | Criterion | Status | Evidence | Verification Command | Notes |
|----|-----------|--------|----------|---------------------|-------|
| AC-1 | Keys.Right registered in `RegisterFocusAsyncActions()` | PASS | `git show bd8fc03 -- QuickFiler/Controllers/QfcItemController.cs` shows `_kbdHandler.KeyActionsAsync.Add(ItemHelper.EntryId, Keys.Right, (x) => this.ToggleExpansionAsync())` added at line ~1345 | `git show bd8fc03 -- QuickFiler/Controllers/QfcItemController.cs` | Signature deviation: plan said `ToggleExpansionAsync(Enums.ToggleState.On)`; implementation uses no-arg `ToggleExpansionAsync()` (toggle). Consistent with 'E' key binding. |
| AC-2 | Keys.Right removal in `UnregisterFocusAsyncActions()` | PASS | `git show bd8fc03` shows `_kbdHandler.KeyActionsAsync.Remove(ItemHelper.EntryId, Keys.Right)` added to `UnregisterFocusAsyncActions()` | `git show bd8fc03 -- QuickFiler/Controllers/QfcItemController.cs` | |
| AC-3 | Unit tests: Keys.Right registered after RegisterFocus, absent after UnregisterFocus | PASS | `qa-test.md`: 74/74 passed; both `RegisterFocusAsyncActions_RightArrowKey_IsRegisteredInKeyActionsAsync` and `UnregisterFocusAsyncActions_AfterRegister_RemovesRightArrowFromKeyActionsAsync` in passed set. `regression-fail-before.md`: both EXIT_CODE: 1 pre-fix. | `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /EnableCodeCoverage` | Fail-before / pass-after chain complete. |
| AC-4 | Integration: manually reproduce in Outlook | UNVERIFIED | Not verifiable by automated tools | Manual | Requires live Outlook session with the QuickFiler add-in deployed. |
| AC-5 | Manual verification: Right arrow expands, mailto: not triggered | UNVERIFIED | Not verifiable by automated tools | Manual | Requires live Outlook session. |

---

## 4. Acceptance Criteria Check-Off Update

The following items in `issue.md` § "Proposed Fix / Validation Ideas" are now checked:

- [x] `AC-1`: `issue.md` line 62 — already marked `[x]` in source
- [x] `AC-2`: `issue.md` line 63 — already marked `[x]` in source
- [x] `AC-3`: `issue.md` line 64 — already marked `[x]` in source
- [ ] `AC-4`: `issue.md` line 65 — remains unchecked (manual integration test)
- [ ] `AC-5`: `issue.md` line 66 — remains unchecked (manual verification)

No changes to `issue.md` checkboxes are required; the source file already reflects the correct state.

---

## 5. Summary

**Overall feature readiness: PASS (automated scope)**

All automated acceptance criteria (AC-1 through AC-3) are met with full evidence. The two
remaining items (AC-4, AC-5) are manual integration verifications that require a live Outlook
session and are not blockable by automated review.

**Top gaps:**
- AC-4 and AC-5 are manual-only and cannot be automated in this codebase due to COM interop
  constraints. They should be completed by the developer/QA before the PR is merged to `main`
  if the team requires manual sign-off.

**Recommended follow-up steps:**
1. Manually deploy the fix to a test Outlook instance and verify Right-arrow expands the
   conversation (AC-4, AC-5).
2. Confirm with product owner whether Right-arrow should always expand or toggle
   (see Code Review F-01 regarding `ToggleExpansionAsync()` vs `ToggleExpansionAsync(On)`).
3. Close GitHub issue #96 on merge.
