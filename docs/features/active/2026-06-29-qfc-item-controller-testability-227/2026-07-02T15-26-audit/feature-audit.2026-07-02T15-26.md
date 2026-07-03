# Feature Audit: QfcItemController Testability — Cycle-3 Targeted Residual Reduction (#227)

**Audit Date:** 2026-07-02
**Feature Folder:** `docs/features/active/2026-06-29-qfc-item-controller-testability-227/`
**Base Branch:** `main` (merge-base `4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`)
**Head Branch:** `TaskMaster-wt-2026-06-29-09-38` — committed HEAD `0a212191` (cycle-1 + cycle-2) plus the uncommitted working tree carrying cycle-3 (Phases 9-11) delivery.
**Work Mode:** `full-feature` (from `issue.md` line 11 → AC sources are `spec.md` and `user-story.md`)
**Audit Type:** Post-remediation acceptance verification (cycle 3)

---

## Scope and Baseline

- **Base branch:** `main` (commit `4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`)
- **Head branch/commit:** `0a212191` committed + uncommitted working tree (cycle-3 delivery, Phases 9-11)
- **Merge base:** `4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`
- **Evidence sources:**
  - Primary: `evidence/qa-gates/final-tests-coverage.2026-07-02T15-12.md`, `final-analyzers.2026-07-02T15-09.md`, `final-nullable.2026-07-02T15-10.md`, `final-csharpier.2026-07-02T15-08.md`, `final-file-sizes.2026-07-02T15-15.md`, `final-residual-verification.2026-07-02T15-16.md`
  - Regression: `evidence/regression-testing/coverage-delta.2026-07-02T15-14.md`
  - Exemption boundary: `evidence/other/exemption-boundary.2026-07-02T15-05.md`
  - Per-phase gates: `evidence/qa-gates/p9-residual-verification.2026-07-02T14-30.md`, `p10a-folderpredictor-seam-verification.2026-07-02T14-45.md`, `p10b-theme-seam-verification.2026-07-02T15-03.md`
  - Direct source inspection of all changed production and test files (independent of the delivered evidence narrative)
- **Feature folder used:** `docs/features/active/2026-06-29-qfc-item-controller-testability-227/` (feature root; no `vN/` selected-version subfolder)
- **Requirements source:** `spec.md` v0.4 (AC1-AC10). `user-story.md` was not located in the feature folder for this feature (`ls` confirms only `issue.md`, `maintainer-decision.2026-07-01.md`, `plan.2026-06-29T09-51.md`, `plan.2026-06-29T10-15.md`, `spec.md`, plus the cycle audit/remediation/evidence folders). Repo-wide search (`find . -iname "user-story*"`) confirms `user-story.md` files exist for other features but not this one. Per `full-feature` mode rules, `spec.md` is treated as the sole available authoritative AC source; the absence of `user-story.md` is documented as an assumption, consistent with the same finding recorded in the cycle-2 audit (`2026-07-02T10-47-audit/feature-audit.2026-07-02T10-47.md:24`).
- **Scope note:** This audit evaluates the **full working-tree content** (committed `0a212191` + uncommitted cycle-3 changes) as the delivered scope, per the scope invariant (full branch diff vs. resolved base, not any plan/task/phase subset). No caller instruction attempted to narrow this scope. All evaluations below are independently re-verified from source and evidence artifacts, not accepted from the delivered narrative at face value — see AC8/AC10 for a material finding this independent check surfaced.

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `spec.md` (v0.4) — sole located source for AC1-AC10 (`user-story.md` absent for this feature)

### Acceptance criteria (spec.md v0.4)

1. **AC1:** `QfcItemController` is split into partial-class files, each under 500 lines, with a logical responsibility-based structure; no behavior change; all existing tests pass.
2. **AC2:** `private ItemViewer _itemViewer` and the public constructor parameters are changed to `IItemViewer`; `Mock<IItemViewer>` is injectable into the controller.
3. **AC3:** `IItemViewer` is narrowed to intent-level members (display-state properties, command events, intent methods); raw clickable/raw control types are removed from the interface; `ItemViewer.cs` provides forwarding implementations and remains `[ExcludeFromCodeCoverage]`.
4. **AC4:** Test files mirror the new partial-class structure (one test file per testable cluster), each under 500 lines, with explicit csproj entries.
5. **AC5:** Coverage of the affected testable (non-exempt) denominator is >= 80%; new/extracted code (including the new seam types) >= 90%; changed lines do not regress. Repo-wide floor handled under the authority-scoped exception precedent (#197).
6. **AC6:** No production file modified exceeds 500 lines after the change (re-verified after the redesign, including the new seam files).
7. **AC7:** Full C# toolchain passes in order — csharpier, .NET analyzers, nullable/TreatWarningsAsErrors, MSTest with coverage — with no regressions (re-verified after the redesign).
8. **AC8:** The cycle-1 exemption set is reduced by removing `[ExcludeFromCodeCoverage]` from the members that have no genuine testability barrier and covering them with tests; no member that can be exercised through the narrowed `IItemViewer` or a mockable collaborator retains an exemption. Cycle-3 scope: the 17 members the cycle-2 residual re-audit found actionable (9 test-only, Tier 1; 8 via the new `FolderPredictor` factory-delegate and `Theme`+`IUiDispatcher` seams, Tier 2) are de-exempted and covered — 41→24.
9. **AC9:** The four behavioral seams (`IUiDispatcher`, `IWebViewCoreInitializer`, `IMailItemActions` + collaborator factory delegates, and thin-delegator `async void` handlers) are introduced per the DI-seam rule ordering, are covered to >= 90%, and preserve runtime behavior. No leaf-control interface layer is introduced. Cycle 3 extends (does not replace) this seam set.
10. **AC10:** Every residual `[ExcludeFromCodeCoverage]` is individually justified with a specific per-member technical reason (no blanket/category exemption), the boundary is minimized (no member reducible via an already-established seam/technique in this codebase retains an exemption), and the boundary is documented for maintainer ratification at review.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| AC1 | Partial-class split, each < 500 lines, no behavior change | PASS | 10 `QfcItemController*.cs` files independently re-measured, all ≤ 466 lines | `wc -l QuickFiler/Controllers/QfcItemController*.cs` | Delivered cycle-1; intact and re-confirmed this cycle. |
| AC2 | `_itemViewer`/ctor params are `IItemViewer`; `Mock<IItemViewer>` injectable | PASS | `QfcItemController.cs:66`; ctors take `IItemViewer` (`Initialization.cs:33`); tests inject `Mock<IItemViewer>` throughout | source read | Delivered cycle-1; unmodified this cycle. |
| AC3 | `IItemViewer` narrowed to intent members; `ItemViewer` forwards; exempt | PASS | `QuickFiler/Viewers/IItemViewer.cs` (120 lines, intent members) | source read | Delivered cycle-1/2; unmodified this cycle. |
| AC4 | Test files mirror partial structure, < 500 lines, csproj-wired | PASS | All per-cluster + Seam test files ≤ 449 lines; csproj entries independently confirmed present (`QuickFiler.csproj`, `QuickFiler.Test.csproj`, `UtilitiesCS.csproj`, `UtilitiesCS.Test.csproj`) | `wc -l`; `git diff --numstat` on the four csproj files | New cycle-3 files (`Theme.DispatcherTests.cs`, `IFolderSearchHandler.cs`, `FolderPredictor.IFolderSearchHandler.cs`) all correctly wired. |
| AC5 | Affected non-exempt denom ≥ 80%; new/extracted ≥ 90%; no changed-line regression | PARTIAL | Affected denom 77.40% (1243/33/330 of 1606) — improved +3.81pp from 73.59% baseline, no regression, but below the stated 80% target. New/extracted code is ~100% covered except two of the 17 newly-instrumented members (`ToggleFocus()`/`ToggleFocus(Enums.ToggleState)`) whose substantive body is never executed by any test (see AC8/AC10). | `evidence/qa-gates/final-tests-coverage.2026-07-02T15-12.md`; `evidence/regression-testing/coverage-delta.2026-07-02T15-14.md`; direct source read of `QfcItemController.FocusAndTheme.cs` and `FocusAndThemeTests.cs` | Denominator has not crossed 80% across three cycles; spec's narrative treats AC5 as `[x]` but the ≥80% sub-target is not literally met. |
| AC6 | No modified/created production file > 500 lines | PASS | All 10 controller partials, `Theme.cs` (451), `Theme.Rendering.cs` (105), `IFolderSearchHandler.cs` (32), `FolderPredictor.IFolderSearchHandler.cs` (10) ≤ 500. `FolderPredictor.cs` (823) is a documented pre-existing exception, independently confirmed unchanged beyond `partial` (`git diff --numstat` shows `+1/-1`). | `wc -l` (independently re-measured); `git diff --numstat 4611fd60 -- UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | `QfcCollectionController.cs` (2296) and `QfcFormControllerTests.cs` (821) are pre-existing, not modified this cycle — out of scope. |
| AC7 | Full toolchain green in order, no regressions | PASS | csharpier/analyzers/nullable EXIT_CODE 0; 347/347 QuickFiler.Test + 4093/4093 UtilitiesCS.Test pass | `evidence/qa-gates/final-*.2026-07-02T15-0[8-9]*`, `final-*2026-07-02T15-1[0-2]*` (EXIT_CODE 0 each) | Verified from evidence artifacts; commands and exit codes match the mandated toolchain order. |
| AC8 | Over-broad exemptions removed + covered; no seam/`IItemViewer`-reachable member stays exempt | PARTIAL | 41 → 24 confirmed by independent grep re-count (24 matches, exact). However, independent behavioral verification of the 17 claimed de-exemptions found 15 genuinely covered (real assertions on outcomes) and 2 (`ToggleFocus()`, `ToggleFocus(Enums.ToggleState)`) covered only by a test that verifies the `Invoke` wrapper was called, never executing or asserting on the method's actual logic. | `grep -rnE "ExcludeFromCodeCoverage\]" QuickFiler/Controllers/QfcItemController*.cs UtilitiesCS/Threading/WpfUiDispatcher.cs QuickFiler/Viewers/WebView2CoreInitializer.cs QuickFiler/Interfaces/MailItemActionsAdapter.cs` (24 matches); direct read of `QfcItemController.FocusAndTheme.cs:27-123` and `FocusAndThemeTests.cs:124-151` | The count (24) is accurate; the *quality* claim ("covered by ≥1 passing test") is overstated for 2 of the 17 removed members. Cycle-2's own accepted review already documented the underlying barrier (`Theme.SetQfcTheme(async:false)` faulting on a handle-less double) as genuine; cycle-3 avoided rather than resolved it. |
| AC9 | Four seams introduced per DI-seam ordering, covered ≥ 90%, behavior preserved; no leaf-control layer | PASS | `FolderPredictor` factory-delegate mirrors the established pattern exactly (verified source read); `Theme`+`IUiDispatcher` retrofit is a minimal, verbatim-preserving extension (verified via `git diff` of `Theme.cs`/`QfcThemeHelper.cs`) with genuine per-line test coverage (`Theme.DispatcherTests.cs`, 4 tests, 100% of the 4 changed lines); `grep` across the controller/`IItemViewer`/`ItemViewer` partials confirms no `IButton`/`ILabel`/`IList<IButton>` (Option B) was introduced. | source read; `evidence/qa-gates/p10b-theme-seam-verification.2026-07-02T15-03.md`; `grep -rn "IButton\|ILabel\|IComboBox\|ITextBox" QuickFiler/` (no matches) | This AC concerns the four seam *types* themselves (which are genuinely well-built and tested), not the specific Tier-1 test-only de-exemptions covered under AC8 — the `ToggleFocus` finding does not implicate this AC. |
| AC10 | Residual exemptions individually justified per-member; boundary documented for ratification | PARTIAL | 24 residuals each carry an inline per-member comment and a per-member entry in `evidence/other/exemption-boundary.2026-07-02T15-05.md`, individually justified by category. However, the "boundary is minimized" claim ("no member reducible via an already-established seam/technique retains an exemption") implies the converse — that every de-exemption this cycle was a genuine reduction — which does not hold for `ToggleFocus()`/`ToggleFocus(Enums.ToggleState)` (AC8). An honestly-reconciled boundary would retain 26 (or genuinely test 2 additional members), not 24. | `evidence/other/exemption-boundary.2026-07-02T15-05.md`; `evidence/qa-gates/final-residual-verification.2026-07-02T15-16.md`; independent source verification of the 17 claimed de-exemptions | The 24-member boundary as documented is not yet ready for maintainer ratification as literally accurate; 22 of the 24 residual justifications hold up to independent scrutiny. |

---

## Summary

**Overall Feature Readiness:** NEEDS REVISION

**Criteria summary:**
- **PASS:** 7 criteria (AC1, AC2, AC3, AC4, AC6, AC7, AC9)
- **PARTIAL:** 3 criteria (AC5, AC8, AC10)
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Top gaps preventing PASS:**

1. **AC8/AC10 (reduction honesty):** `ToggleFocus()`/`ToggleFocus(Enums.ToggleState)`
   (`QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:27-123`) were de-exempted using a test
   that verifies only that `_itemViewer.Invoke` was called, never executing or asserting on the actual
   method behavior. Remediate by either genuinely testing the state-transition logic (decoupling it
   from the `Invoke` wrapper, mirroring `ToggleFocusOnAsync`/`ToggleFocusOffAsync`) or restoring the
   exemption with the same per-member justification cycle-2 used.
2. **AC5 (coverage floor):** the affected non-exempt denominator (77.40%) remains below the spec's
   stated ≥80% target across three cycles. This is a pre-existing, not newly-introduced, gap; it should
   be explicitly dispositioned (accepted-with-exception or scheduled for further uplift) rather than
   implicitly carried forward.
3. **Process gate (not an AC):** cycle-3 changes are uncommitted; the branch must be committed before
   merge (tracked in the policy audit and code review).

**Recommended follow-up verification steps:**

1. Resolve the `ToggleFocus`/`ToggleFocus(Enums.ToggleState)` finding (genuine test or honest
   re-exemption), then re-run the residual grep and final coverage gate to confirm the corrected boundary
   count and coverage percentage.
2. Commit the cycle-3 working-tree changes and re-confirm `git status` is clean, then re-run the final
   toolchain against the committed head to confirm parity with the evidence.
3. Explicitly disposition the sub-80% affected-denominator reading in `spec.md` (exception or scheduled
   uplift) before requesting maintainer ratification of the exemption boundary.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules: criteria evaluated PASS may be checked off if represented as
checkboxes and not already checked; criteria evaluated PARTIAL/FAIL/UNVERIFIED must remain unchecked.

`spec.md`'s AC1-AC4, AC6, AC7, AC9 are already marked `[x]` by the executor and remain correctly checked
per this audit's PASS findings. `spec.md`'s AC5, AC8, AC10 are already marked `[ ]` (unchecked) by the
executor pending maintainer ratification; this audit's independent PARTIAL findings for AC5, AC8, AC10
confirm they should remain unchecked — no check-off change was made to `spec.md` by this audit.

The repository naming convention is confirmed: the third artifact is `feature-audit.<timestamp>.md`
(this file), not `feature-review.<timestamp>.md`.

### AC Status Summary

- Source: `spec.md` (v0.4)
- Total AC items: 10 (AC1-AC10)
- Checked off (delivered): 7 (AC1, AC2, AC3, AC4, AC6, AC7, AC9)
- Remaining (unchecked): 3 (AC5, AC8, AC10)
- Items remaining:
  - AC5: Coverage of the affected testable (non-exempt) denominator is >= 80% — currently 77.40%.
  - AC8: The cycle-1 exemption set is reduced ... no member that can be exercised through the narrowed `IItemViewer` or a mockable collaborator retains an exemption — 2 of 17 cycle-3 de-exemptions are not behaviorally verified.
  - AC10: Every residual `[ExcludeFromCodeCoverage]` is individually justified ... the boundary is minimized — not literally true while the 2 `ToggleFocus` members remain unverified.

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `spec.md` | 10 | 7 | 3 | Checkbox-backed; AC5/AC8/AC10 remain unchecked, consistent with both the executor's own pending-ratification framing and this audit's independent PARTIAL findings. |

**Feature-audit blocking-finding count: 2** (AC8 and AC10 PARTIAL for the same underlying
`ToggleFocus`/`ToggleFocus(Enums.ToggleState)` reduction-honesty finding; AC5's sub-80% reading is a
pre-existing, non-newly-introduced open item tracked but not counted as a new cycle-3 blocking finding).
