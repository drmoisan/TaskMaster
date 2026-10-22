# Feature Audit: Remove `stackMovedItems` parameter from `MoveEmailsAsync` (#629)

**Audit Date:** 2026-09-02
**Feature Folder:** `docs/features/active/2026-08-26-qfc-remove-stackmoveditems-parameter-629`
**Base Branch:** `main`
**Head Branch:** `feature/qfc-remove-stackmoveditems-parameter-629`
**Work Mode:** `full-feature`
**Audit Type:** Initial acceptance review

---

## Scope and Baseline

- **Base branch:** `main`
- **Head branch/commit:** `feature/qfc-remove-stackmoveditems-parameter-629` (working tree, pre-commit
  at time of audit)
- **Merge base:** `origin/main` post-merge (branch merged clean at commit `fe91e965`)
- **Evidence sources:**
  - Primary: `evidence/qa-gates/*` (toolchain and coverage evidence)
  - Secondary baseline diff: `evidence/baseline/*`
  - Feature evidence: `evidence/` tree in this folder
  - Additional evidence: `evidence/other/*` (test disposition, footprint check)
- **Feature folder used:** `docs/features/active/2026-08-26-qfc-remove-stackmoveditems-parameter-629`
- **Requirements source:** `spec.md` (Acceptance Criteria section), cross-referenced against `issue.md`
- **Work mode resolution note:** `spec.md` header explicitly states `Work Mode: full-feature`.
- **Scope note:** `issue.md` was the only pre-existing content in this feature folder; `spec.md`,
  `user-story.md`, and the atomic plan were authored during this orchestration run since the folder had
  only empty scaffolds for those files.

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `spec.md` — primary (contains the full AC1–AC8 list)

### Acceptance criteria

1. AC1. `IQfcCollectionController.MoveEmailsAsync` declares zero parameters.
2. AC2. `QfcCollectionController.MoveEmailsAsync` declares zero parameters and its body contains no
   `stackMovedItems` reference.
3. AC3. The sole call site invokes `await _groups.MoveEmailsAsync();` with no argument.
4. AC4. No `QuickFiler.Test` file contains a `Mock<IQfcCollectionController>` `Setup`/`Verify` that
   still names the old single-parameter overload.
5. AC5. `MoveEmailsAsync_WithNullStack_BehavesIdenticallyToAnEmptyStack` is retired or rewritten to no
   longer assert on the removed parameter's shape.
6. AC6. The full `QuickFiler.Test` suite passes with no regression.
7. AC7. A single clean toolchain pass completed in order: CSharpier check, analyzer build, nullable
   build, and the full MSTest suite with coverage.
8. AC8. The diff touches only the files listed under "Implementation Strategy" in `spec.md`, plus this
   feature folder's own evidence and documentation.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 1 | AC1 — interface has zero parameters | PASS | `QuickFiler/Interfaces/IQfcCollectionController.cs:63` reads `Task MoveEmailsAsync();` | `grep -n "MoveEmailsAsync" QuickFiler/Interfaces/IQfcCollectionController.cs` | Direct file inspection. |
| 2 | AC2 — implementation has zero parameters, no stale reference | PASS | `QuickFiler/Controllers/QfcCollectionController.cs:2152`, `public async Task MoveEmailsAsync()`; no `stackMovedItems` token remains in the method body | `grep -n "stackMovedItems" QuickFiler/Controllers/QfcCollectionController.cs` returns 0 hits | Actual line was 2152, not the 2253 the issue guessed; verified directly, not assumed. |
| 3 | AC3 — call site updated | PASS | `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:228`, `await _groups.MoveEmailsAsync();` | Direct file inspection | Actual line was 228, not the 225 the issue guessed. |
| 4 | AC4 — no stale mock signatures | PASS | `QfcFormControllerUndoHandoffTests.cs` Setup (formerly line ~75) and Verify (formerly line ~396) both updated | Full-repo grep for `MoveEmailsAsync(It.IsAny` and `MoveEmailsAsync(null)` returns 0 hits; `evidence/baseline/p0-t8-mock-sweep.md` | Sweep found 6 total call sites (4 direct + 2 mock), one more than `issue.md` named. |
| 5 | AC5 — null-stack test retired or rewritten | PASS | Rewritten to `MoveEmailsAsync_WithEmptyItemGroupsToMove_DoesNotThrow`, preserving early-return-branch coverage | `evidence/other/p1-t5-test-disposition.md` | Rewrite chosen over deletion specifically to avoid a branch-coverage loss. |
| 6 | AC6 — full suite passes, no regression | PASS | 6949/6949 passing at both baseline and final | `evidence/baseline/p0-t7-baseline-coverage.md`, `evidence/qa-gates/p2-t5-final-coverage.md` | No count change. |
| 7 | AC7 — single clean toolchain pass | PASS | CSharpier check exit 0; analyzer build exit 0, 0 errors; nullable build exit 0, 0 errors; MSTest 6949/6949 | `evidence/qa-gates/p2-t2-csharpier-check.md`, `p2-t3-analyzer-build.md`, `p2-t4-nullable-build.md`, `p2-t5-final-coverage.md` | Single pass, no restarts required. |
| 8 | AC8 — footprint matches prediction exactly | PASS | Exactly 5 production/test files touched, matching `spec.md`'s Implementation Strategy list | `evidence/other/p1-t6-footprint-check.md` | Confirmed via `git diff --name-only` against `origin/main`. |

---

## Summary

**Overall Feature Readiness:** PASS

**Criteria summary:**
- **PASS:** 8 criteria
- **PARTIAL:** 0 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Top gaps preventing PASS:**

1. None.

**Recommended follow-up verification steps:**

1. Re-run the footprint check (`evidence/other/p1-t6-footprint-check.md` equivalent) after the final
   commit, to confirm no unrelated files were staged.
2. Confirm CI reproduces the same 6949/6949 pass count and clean toolchain result before merge.

---

## Acceptance Criteria Check-Off

### AC Status Summary

- Source: `spec.md`
- Total AC items: 8
- Checked off (delivered): 8
- Remaining (unchecked): 0
- Items remaining: None.

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `spec.md` | 8 | 8 | 0 | Checkbox-backed; all 8 already checked `[x]` in the source file with inline evidence citations as of this audit. |
