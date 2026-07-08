# Feature Audit: QfcItemController Testability — Cycle-2 Seam Redesign (#227)

**Audit Date:** 2026-07-02
**Feature Folder:** `docs/features/active/2026-06-29-qfc-item-controller-testability-227/`
**Base Branch:** `main` (merge-base `4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`)
**Head Branch/Scope:** `TaskMaster-wt-2026-06-29-09-38` — committed HEAD `bfc8364b` (cycle-1) plus the uncommitted working tree carrying cycle-2 (Phases 5–8). This audit evaluates the **working-tree content** as the delivered scope.
**Work Mode:** `full-feature` (from `issue.md` line 11 → AC sources are `spec.md` and `user-story.md`)
**Audit Type:** Post-remediation acceptance verification (cycle 2)

---

## Scope and Baseline

- **Base branch:** `main` (commit `4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`)
- **Head branch/commit:** `bfc8364b` committed + uncommitted working tree (cycle-2 delivery)
- **Merge base:** `4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`
- **Evidence sources:**
  - Primary: `evidence/qa-gates/final-r2-tests-coverage.2026-07-02T10-45.md`, `final-r2-analyzers`, `final-r2-nullable`, `final-r2-csharpier`, `final-r2-file-sizes`, `final-r2-exemption-delta`
  - Regression: `evidence/regression-testing/coverage-delta-r2.2026-07-02T10-45.md`
  - Exemption boundary: `evidence/other/exemption-boundary.2026-07-02T10-30.md`, `evidence/qa-gates/p7r-residual-verification.2026-07-02T10-30.md`
  - Direct source inspection of the seam files and all `QfcItemController*.cs` partials
- **Requirements source:** `spec.md` v0.3 (AC1–AC10) + `user-story.md`. This cycle's acceptance is driven by AC5, AC6, AC7 (re-verified) and AC8, AC9, AC10 (added for the redesign).
- **Work mode resolution note:** `issue.md` carries an explicit `- Work Mode: full-feature` marker.
- **Scope note:** Working-tree-only validation — the cycle-2 diff is not yet committed; the audit treats the working tree as the delivered branch head. `user-story.md` was not located as a separate authoritative testability source for the redesign ACs; `spec.md` v0.3 is the authoritative AC source for AC5–AC10.

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `spec.md` (v0.3) — primary source for AC1–AC10 (all marked `[x]` by the executor)

### Acceptance criteria (this cycle's testability set — AC5, AC8, AC9, AC10 — plus re-verified AC6, AC7)

- **AC5:** Coverage of the affected testable (non-exempt) denominator ≥ 80%; new/extracted code
  (incl. new seam types) ≥ 90%; changed lines do not regress; repo-wide floor under the
  authority-scoped exception (#197). Met by making members testable, not by exempting them.
- **AC6:** No production file modified/created exceeds 500 lines after the redesign (incl. new seam files).
- **AC7:** Full C# toolchain passes in order — csharpier, analyzers, nullable/TWAE, MSTest w/coverage — no regressions.
- **AC8:** Cycle-1 exemption set reduced by de-exempting the no-barrier members and covering them; no
  member exercisable through `IItemViewer`/a mockable collaborator retains an exemption.
- **AC9:** The four behavioral seams introduced per DI-seam ordering, covered ≥ 90%, behavior preserved;
  no leaf-control interface layer.
- **AC10:** Any residual `[ExcludeFromCodeCoverage]` individually justified per-member (no blanket/category
  exemption); reduced boundary documented for maintainer ratification.
- **AC1–AC4:** (delivered cycle-1) partial-class split; `IItemViewer` field-type + narrowing; test-file mirror.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| AC1 | Partial-class split, each < 500 lines, no behavior change | PASS | 10 `QfcItemController*.cs` all < 500 (largest `Initialization.cs` = 446) | `wc -l QuickFiler/Controllers/QfcItemController*.cs` | Delivered cycle-1; re-confirmed intact. |
| AC2 | `_itemViewer`/ctor params are `IItemViewer`; `Mock<IItemViewer>` injectable | PASS | `QfcItemController.cs:51`; ctors take `IItemViewer`; tests inject `Mock<IItemViewer>` | source read | Delivered cycle-1. |
| AC3 | `IItemViewer` narrowed to intent members; `ItemViewer` forwards; exempt | PASS | `QuickFiler/Viewers/IItemViewer.cs` (intent members; raw `Label`/`Component` retained per declined Option B) | source read | Delivered cycle-1; raw leaf controls intentionally retained (Option B declined). |
| AC4 | Test files mirror partial structure, < 500 lines, csproj-wired | PASS | per-cluster + Seam test files; `QuickFiler.Test.csproj` entries | `git status`; `wc -l` | New Phase 5/6 test files wired. |
| AC5 | Affected non-exempt denom ≥ 80%; new/extracted ≥ 90%; no changed-line regression | PASS | Affected denom 885/1051 = **84.21%** (≥80); new/extracted **100%** (≥90); no changed-line regression | `evidence/regression-testing/coverage-delta-r2.2026-07-02T10-45.md`; `evidence/qa-gates/final-r2-tests-coverage.2026-07-02T10-45.md` | Denominator grew 239→1051 as members moved from exempt into the tested set — the intended Option A effect. Verified from artifacts (not re-run). |
| AC6 | No modified/created production file > 500 lines | PASS | All 10 controller partials + 6 seam files < 500 (largest 446); `IItemViewer.cs` = 120 | `wc -l` (independently re-measured) | `QfcCollectionController.cs` (2296) and `QfcFormControllerTests.cs` (821) are pre-existing, not modified this cycle — out of scope. |
| AC7 | Full toolchain green in order, no regressions | PASS | csharpier/analyzers/nullable EXIT_CODE 0; 328/328 tests pass | `evidence/qa-gates/final-r2-*.2026-07-02T10-45.md` | Verified from evidence artifacts (EXIT_CODE 0 each). |
| AC8 | Over-broad exemptions removed + covered; no seam/`IItemViewer`-reachable member stays exempt | PASS | 103 → 41; de-exempted members each mapped to ≥1 passing test; residuals verified as genuinely barrier-bound | `evidence/qa-gates/final-r2-exemption-delta.2026-07-02T10-45.md`; `p7r-residual-verification.2026-07-02T10-30.md`; source spot-checks | Verified: de-exempted `SetThemeDark/Light` are `Theme`-safe via `async:true` deferral; exempt `ToggleFocus` faults on the handle-less `Theme` (`SetQfcTheme(async:false)`). Genuine distinction. |
| AC9 | Four seams introduced per DI-seam ordering, covered ≥ 90%, behavior preserved; no leaf-control layer | PASS | Seam files present; extracted cores 100% covered; grep confirms no `IButton`/`IList<IButton>` | source read; `evidence/qa-gates/p6r-tests-coverage.2026-07-02T10-17.md` | COM/dispatcher migration atomic; event-wiring order preserved; Option B not introduced. |
| AC10 | Residual exemptions individually justified per-member; boundary documented for ratification | PASS | 41 residuals each with inline per-member comment + per-member entry in the verification artifact; boundary submitted for ratification | `evidence/other/exemption-boundary.2026-07-02T10-30.md`; `p7r-residual-verification.2026-07-02T10-30.md`; source grep = 38 controller attrs + 3 adapter shims | No blanket/category exemption remains. One refinement (`ApplyReadEmailFormat` interleaves 2 seam-testable statements with the exempt `Theme` line) is a non-blocking improvement, not an AC failure. |

---

## Summary

**Overall Feature Readiness:** PASS (pending the pre-merge commit of the delivered working tree)

**Criteria summary:**
- **PASS:** 10 (AC1–AC10)
- **PARTIAL:** 0
- **UNVERIFIED:** 0
- **FAIL:** 0

**Top gaps preventing PASS:**
1. None at the acceptance-criteria level. All ten ACs are met by the delivered working-tree content.
2. Process gate (not an AC): the cycle-2 changes are uncommitted; the branch must be committed before merge.

**Recommended follow-up verification steps:**
1. Commit the cycle-2 working-tree changes and re-confirm `git status` is clean, then re-run the
   final toolchain against the committed head to confirm parity with the evidence.
2. (Optional refinement) Extract the `_mailActions` writes in `ApplyReadEmailFormat` into a tested
   core, and emit the canonical `artifacts/csharp/coverage.xml` from the r2 run.

---

## Acceptance Criteria Check-off

The `spec.md` AC checkboxes (AC1–AC10) are already marked `[x]` by the executor. This audit confirms
those check-offs are supported by evidence for AC5–AC10; no additional check-off change is required.
The repository naming convention is confirmed: the third artifact is `feature-audit.<timestamp>.md`
(this file), not `feature-review.<timestamp>.md`.

### AC Status Summary

- Source: `spec.md` (v0.3)
- Total AC items: 10 (AC1–AC10)
- Checked off (delivered): 10
- Remaining (unchecked): 0
- Items remaining: None.

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `spec.md` | 10 | 10 | 0 | Checkbox-backed; all AC verified PASS against working-tree delivery |

**Feature-audit blocking-finding count: 0** (all ACs PASS; the uncommitted-worktree item is a
process/merge gate tracked in the policy audit).
