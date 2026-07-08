# Feature Audit: qfc-form-viewer-testability (#223)

**Audit Date:** 2026-06-28
**Feature Folder:** `docs/features/active/2026-06-28-qfc-form-viewer-testability-223`
**Base Branch:** `main`
**Head Branch:** `TaskMaster-wt-2026-06-28-18-50`
**Work Mode:** `full-feature`
**Audit Type:** Initial acceptance review

---

## Scope and Baseline

- **Base branch:** `main` (commit `86b555bf2a26f91a5f59f7dbccf6a6ac56d8e16a`)
- **Head branch/commit:** `TaskMaster-wt-2026-06-28-18-50` (commit `e91927105abde2ceadd10a7011bc17d714108afd`)
- **Merge base:** `86b555bf2a26f91a5f59f7dbccf6a6ac56d8e16a`
- **Evidence sources:**
  - Primary: `artifacts/pr_context.summary.txt`
  - Secondary baseline diff: `artifacts/pr_context.appendix.txt`
  - Feature evidence: `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/evidence/**`
  - Additional evidence: direct `git diff`/`git grep`/`awk` head-state inspection and an independent CSharpier check
- **Feature folder used:** `docs/features/active/2026-06-28-qfc-form-viewer-testability-223`
- **Requirements source:** `issue.md` (AC1–AC7)
- **Work mode resolution note:** `issue.md` declares `- Work Mode: full-feature`, which normally resolves AC sources to `spec.md` and `user-story.md`. However, `user-story.md` does not exist in this feature folder, and `spec.md` contains no `## Acceptance Criteria` checkbox section (only `## Definition of Done` and `## Seeded Test Conditions`). The only enumerated, checkbox-format acceptance criteria in the feature folder are `issue.md` AC1–AC7, which the review request designated as the authoritative AC source. Those seven are evaluated here; `spec.md` Definition of Done and Seeded Test Conditions are treated as supplementary and are not separately checked off.
- **Scope note:** Audit covers the full feature-vs-base diff (46 files, +2278 / -992). PR-context artifacts were present and current (generated 2026-06-29 01:24 UTC against the head commit); no regeneration was required.

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/issue.md` — only checkbox-format AC source
- `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/spec.md` — supplementary (prose Definition of Done; no `## Acceptance Criteria` section)
- `user-story.md` — absent (not present in feature folder)

### Acceptance criteria

1. AC1: `QfcFormKeyHandler.IsAltKeyCommand(Keys)` exists as a pure, non-Form unit and is called by `QfcFormViewer`, `QfcFormViewerDark`, and `QfcFormViewerExpanded` `ProcessCmdKey` overrides; `QfcFormViewerDark` and `QfcFormViewerExpanded` carry `[ExcludeFromCodeCoverage]`.
2. AC2: `IQfcFormViewer` exposes intent-level command events and state properties in place of the four `Button` properties and the `NumericUpDown` property; no raw clickable control type remains on the interface.
3. AC3: `IQfcFormViewer` exposes `SwapItemTableLayout(TableLayoutPanel)`; `L1v0L2L3v_TableLayout` is get-only on the interface; `ActivateQueuedTlp` performs the swap through the new method.
4. AC4: `IQfcFormViewer` exposes `CaptureTlpCellStates()`, `GetKeyEventExclusionControls()`, and `ItemViewerTemplateMargin`; `QfcItemViewerTemplate` and `QfcItemViewerExpandedTemplate` are removed from the interface; `CaptureItemSettings` and `RegisterFormEventHandlers` consume the new members.
5. AC5: New MSTest coverage verifies, via Moq event raising / `VerifySet` / `Verify`, that command events route to the correct controller methods, that the skip flow toggles `SkipButtonText`/`SkipButtonEnabled`, and that `CaptureItemSettings` handles both the populated and null `CaptureTlpCellStates()` results. New non-exempt code meets the >= 90% coverage floor; changed lines do not regress coverage; repo-wide coverage stays >= 80%.
6. AC6: No production file modified in this cycle exceeds 500 lines after the change (`QfcFormController.cs` split into partial classes). `QfcCollectionController.cs` is a pre-existing cap violation touched only with a net-negative edit; disposition recorded.
7. AC7: Full C# toolchain passes in order — csharpier, .NET analyzers, nullable/TreatWarningsAsErrors, MSTest with coverage — with no regressions.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 1 | `IsAltKeyCommand` pure unit, called by 3 viewers; Dark/Expanded exempt | PASS | `QfcFormKeyHandler.cs` (internal static, `HasFlag(Keys.Alt)`); call sites in `QfcFormViewer.cs:60`, `QfcFormViewerDark.cs:43`, `QfcFormViewerExpanded.cs:43`; `[ExcludeFromCodeCoverage]` on Dark:16/Expanded:16 | `git grep -n IsAltKeyCommand`; `git grep -n ExcludeFromCodeCoverage` | Clean extraction; verified directly. |
| 2 | Intent events/state replace 4 Buttons + NumericUpDown; no raw clickable control on interface | PASS | `IQfcFormViewer.cs` lines 37-49: `OkClicked`/`CancelClicked`/`UndoClicked`/`SkipClicked` events, `SkipButtonText`/`SkipButtonEnabled`, `ItemsPerLoadValue`/`ItemsPerLoadValueChanged`/`ItemsPerLoadEnabled`; no `Button`/`NumericUpDown` member remains; `QfcHomeController` migrated to `ItemsPerLoadEnabled`/`SkipButtonEnabled` | Read `IQfcFormViewer.cs`; `git diff` of `QfcHomeController.cs` | 23-member interface confirmed by count. |
| 3 | `SwapItemTableLayout` added; `L1v0L2L3v_TableLayout` get-only; `ActivateQueuedTlp` swaps via new method | PASS | `IQfcFormViewer.cs:24` (get-only), `:29` (`SwapItemTableLayout`); `QfcCollectionController.cs:843` `ActivateQueuedTlp` calls `_formViewer.SwapItemTableLayout(tlp)` | Read interface; `git diff QfcCollectionController.cs` | Setter removed; net -3 lines. |
| 4 | `CaptureTlpCellStates`/`GetKeyEventExclusionControls`/`ItemViewerTemplateMargin` added; templates removed; consumers updated | PASS | `IQfcFormViewer.cs:32-34` adds the three members; `QfcItemViewerTemplate`/`QfcItemViewerExpandedTemplate` absent from interface; consumer rewrites in `QfcFormController.SetupDisposal.cs` (`CaptureItemSettings`, `RegisterFormEventHandlers`) | Read interface; `ac-traceability` P3-T4/T7/T8 | Consumer rewrite confirmed via interface + SetupDisposal partial + traceability. |
| 5 | New MSTest routing/skip/capture coverage; new code >= 90%; no changed-line regression; repo-wide >= 80% | PARTIAL | Tests present (`QfcFormControllerSeamTests.cs` 11 cases, `QfcFormKeyHandlerTests.cs` 4 cases) using Moq `Raise`/`VerifySet`/`Verify`; new code 100% (2/2); changed-type +12.62pp no-regression. Repo-wide first-party >= 80% NOT measured; canonical `artifacts/csharp/coverage.xml` absent | `git grep`/Read test files; `coverage-delta.2026-06-28T20-52.md`; `ls artifacts/csharp` (absent) | Routing/skip/capture and new-code/changed-line sub-claims PASS; the repo-wide >= 80% sub-claim is unverified, so the criterion is PARTIAL. |
| 6 | No modified production file > 500 after change; QfcCollectionController net-negative debt disposition recorded | PASS | Split files: 195/311/399/232; `QfcFormKeyHandler` 20; `QfcFormViewer` 262; others < 500. `QfcCollectionController.cs` 2296 (baseline 2299, net -3, `[ExcludeFromCodeCoverage]`); disposition recorded in `baseline-file-sizes` and `ac-traceability` | `awk END{print NR}` per file; `git show 86b555bf:...` baseline | All cycle-modified-and-grown production files < 500; pre-existing-debt disposition present. |
| 7 | Full C# toolchain passes in order, no regressions | PASS | `evidence/qa-gates/final-csharpier` (0), `final-analyzers` (0), `final-nullable` (0), `final-tests-coverage` (196/196). Reviewer independently re-ran `csharpier check` on 4 key files → exit 0 | executor evidence; `dotnet tool run csharpier check <files>` | msbuild/vstest verified from executor evidence (not reproduced locally); csharpier independently re-verified. |

---

## Summary

**Overall Feature Readiness:** NEEDS REVISION

**Criteria summary:**
- **PASS:** 6 criteria (AC1, AC2, AC3, AC4, AC6, AC7)
- **PARTIAL:** 1 criterion (AC5)
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Top gaps preventing PASS:**

1. AC5 is PARTIAL: the repo-wide first-party >= 80% coverage sub-claim is unverified — the canonical C# coverage artifact (`artifacts/csharp/coverage.xml`) is absent and no repo-wide first-party (testable-denominator) measurement exists. The new-code (100%) and changed-line no-regression sub-claims are satisfied.
2. Two pre-existing 500-line-cap files remain over cap (`QfcCollectionController.cs`, `QfcFormControllerTests.cs`); accepted as net-negative debt this cycle (non-blocking) but carried forward as policy debt.

**Recommended follow-up verification steps:**

1. Produce `artifacts/csharp/coverage.xml` (Cobertura) and a repo-wide first-party testable-denominator coverage measurement; confirm the >= 80% floor, then re-evaluate AC5.
2. Re-run the feature audit after the coverage artifact exists; if the floor is confirmed, AC5 moves to PASS and overall readiness moves to PASS.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules:
- Criteria evaluated as **PASS** may be checked off in the authoritative source file(s) if they are markdown checkboxes and not already checked.
- Criteria evaluated as **PARTIAL**, **FAIL**, or **UNVERIFIED** must remain unchecked.

AC1, AC2, AC3, AC4, AC6, and AC7 are PASS and remain checked `[x]` in `issue.md` (the executor had already checked all seven). AC5 is evaluated as PARTIAL; to reflect the unverified repo-wide coverage sub-claim, AC5 was reverted to unchecked `[ ]` in `issue.md` by this review.

### AC Status Summary

- Source: `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/issue.md`
- Total AC items: 7
- Checked off (delivered): 6
- Remaining (unchecked): 1
- Items remaining: AC5 (repo-wide >= 80% coverage sub-claim unverified pending canonical C# coverage artifact)

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `issue.md` | 7 | 6 | 1 | Checkbox-backed; AC5 reverted to unchecked (PARTIAL) |
| `spec.md` | 0 | 0 | 0 | Prose-only Definition of Done; no `## Acceptance Criteria` checkboxes |
| `user-story.md` | 0 | 0 | 0 | Not authoritative — file absent |
