# Feature Audit: qfc-form-viewer-testability (#223)

**Audit Date:** 2026-06-29
**Feature Folder:** `docs/features/active/2026-06-28-qfc-form-viewer-testability-223`
**Base Branch:** `main`
**Head Branch:** `TaskMaster-wt-2026-06-28-18-50`
**Work Mode:** `full-feature`
**Audit Type:** Cycle-1 remediation closing reaudit (exit)

---

## Scope and Baseline

- **Base branch:** `main` (commit `86b555bf2a26f91a5f59f7dbccf6a6ac56d8e16a`)
- **Head branch/commit:** `TaskMaster-wt-2026-06-28-18-50` (commit `f4b455e6a3ca536b3fc47fa7026b076efbacf453`)
- **Merge base:** `86b555bf2a26f91a5f59f7dbccf6a6ac56d8e16a`
- **Evidence sources:**
  - Primary: `artifacts/pr_context.summary.txt`
  - Secondary baseline diff: `artifacts/pr_context.appendix.txt`
  - Feature evidence: `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/evidence/**`
  - Canonical coverage artifact: `artifacts/csharp/coverage.xml`
  - Additional evidence: direct `git diff`/`git grep`/`awk` head-state inspection, independent Cobertura parsing, and an independent CSharpier check
- **Feature folder used:** `docs/features/active/2026-06-28-qfc-form-viewer-testability-223`
- **Requirements source:** `issue.md` (AC1–AC7)
- **Work mode resolution note:** `issue.md` declares `- Work Mode: full-feature`, which normally resolves AC sources to `spec.md` and `user-story.md`. However, `user-story.md` does not exist in this feature folder, and `spec.md` contains no `## Acceptance Criteria` checkbox section (only `## Definition of Done` and `## Seeded Test Conditions`). The only enumerated, checkbox-format acceptance criteria in the feature folder are `issue.md` AC1–AC7, which the review request designated as the authoritative AC source. Those seven are evaluated here; `spec.md` Definition of Done and Seeded Test Conditions are treated as supplementary and are not separately checked off.
- **Scope note:** Audit covers the full feature-vs-base diff (74 files, +3751 / -992). PR-context artifacts were present and current (head matches `f4b455e6`); no regeneration was required. The PR-context summary overview line ("Core logic changes: 0 files; Docs/templates/agents/tooling: 57 files") misclassifies the C# code changes as docs; the authoritative scope is the `git diff` name-status (15 `.cs` + 2 `.csproj` code files), which this audit used.

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
5. AC5: New MSTest coverage verifies, via Moq event raising / `VerifySet` / `Verify`, that command events route to the correct controller methods, that the skip flow toggles `SkipButtonText`/`SkipButtonEnabled`, and that `CaptureItemSettings` handles both the populated and null `CaptureTlpCellStates()` results. New non-exempt code meets the >= 90% coverage floor; changed lines do not regress coverage; repo-wide coverage stays >= 80% (satisfied-with-documented-exception per the ratified authority-scoped exception).
6. AC6: No production file modified in this cycle exceeds 500 lines after the change (`QfcFormController.cs` split into partial classes). `QfcCollectionController.cs` is a pre-existing cap violation touched only with a net-negative edit; disposition recorded.
7. AC7: Full C# toolchain passes in order — csharpier, .NET analyzers, nullable/TreatWarningsAsErrors, MSTest with coverage — with no regressions.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 1 | `IsAltKeyCommand` pure unit, called by 3 viewers; Dark/Expanded exempt | PASS | `QfcFormKeyHandler.cs` (internal static, `HasFlag(Keys.Alt)`); call sites in `QfcFormViewer.cs:60`, `QfcFormViewerDark.cs:43`, `QfcFormViewerExpanded.cs:43`; `[ExcludeFromCodeCoverage]` on Dark:16/Expanded:16 | `git grep -n IsAltKeyCommand`; `git grep -n ExcludeFromCodeCoverage` | Clean extraction; verified directly this reaudit. |
| 2 | Intent events/state replace 4 Buttons + NumericUpDown; no raw clickable control on interface | PASS | `IQfcFormViewer.cs` lines 37-49: `OkClicked`/`CancelClicked`/`UndoClicked`/`SkipClicked` events, `SkipButtonText`/`SkipButtonEnabled`, `ItemsPerLoadValue`/`ItemsPerLoadValueChanged`/`ItemsPerLoadEnabled`; no `Button`/`NumericUpDown` member type remains (the retained `List<Control> Buttons` is a panel-collection getter, not a clickable control); `QfcHomeController` migrated to `ItemsPerLoadEnabled`/`SkipButtonEnabled` | Read `IQfcFormViewer.cs`; `git diff` of `QfcHomeController.cs` | 23-member interface confirmed. |
| 3 | `SwapItemTableLayout` added; `L1v0L2L3v_TableLayout` get-only; `ActivateQueuedTlp` swaps via new method | PASS | `IQfcFormViewer.cs:24` (get-only), `:29` (`SwapItemTableLayout`); `QfcCollectionController.cs:843` `ActivateQueuedTlp` calls `_formViewer.SwapItemTableLayout(tlp)` | Read interface; `git grep -n SwapItemTableLayout` | Setter removed; net -3 lines. |
| 4 | `CaptureTlpCellStates`/`GetKeyEventExclusionControls`/`ItemViewerTemplateMargin` added; templates removed; consumers updated | PASS | `IQfcFormViewer.cs:32-34` adds the three members; `QfcItemViewerTemplate`/`QfcItemViewerExpandedTemplate` absent from interface; consumer rewrites in `QfcFormController.SetupDisposal.cs` (`CaptureItemSettings`, `RegisterFormEventHandlers`) | Read interface; `git grep` templates (absent); `ac-traceability` P3-T4/T7/T8 | Consumer rewrite confirmed. |
| 5 | New MSTest routing/skip/capture coverage; new code >= 90%; no changed-line regression; repo-wide >= 80% | PASS (documented exception) | Tests present (`QfcFormControllerSeamTests.cs` 11 cases, `QfcFormKeyHandlerTests.cs` 4 cases) using Moq `Raise`/`VerifySet`/`Verify`; new code 100% and changed-type +12.62pp (39.24%→51.86%) no-regression, both re-derived from `artifacts/csharp/coverage.xml` this reaudit. Repo-wide first-party measured 73.35% (testable denominator) / 74.11% (Cobertura root), below the bare 80% floor, accepted under the maintainer-ratified authority-scoped exception (`maintainer-decision.2026-06-29.md`); pre-existing, not introduced; residual tracked under #197 | Parse `artifacts/csharp/coverage.xml`; `coverage-delta.2026-06-28T20-52.md`; `repo-wide-coverage-testable-denominator.2026-06-28T21-30.md` | Routing/skip/capture, new-code, and changed-line sub-claims PASS unconditionally; the repo-wide sub-claim is measured and dispositioned under the ratified exception. |
| 6 | No modified production file > 500 after change; QfcCollectionController net-negative debt disposition recorded | PASS | Split files: 195/311/399/232; `QfcFormKeyHandler` 20; `QfcFormViewer` 262; `QfcHomeController` 454; others < 500. `QfcCollectionController.cs` 2296 (baseline 2299, net -3, `[ExcludeFromCodeCoverage]`); `QfcFormControllerTests.cs` 821 (baseline 823, net -2); dispositions recorded | `awk END{print NR}` per file; `git show 86b555bf:...` baseline | All cycle-modified-and-grown production files < 500; pre-existing-debt dispositions present and net-negative. |
| 7 | Full C# toolchain passes in order, no regressions | PASS | `evidence/qa-gates/final-csharpier.2026-06-28T21-30.md` (0), `final-analyzers` (0), `final-nullable` (0), `final-tests-coverage` (4566/4566). No `.cs`/`.csproj` changed after the gate run. Reviewer independently re-ran `csharpier check` on 3 key files → exit 0 this reaudit | executor evidence; `dotnet tool run csharpier check <files>`; `git diff --name-only e9192710 HEAD -- '*.cs' '*.csproj'` (empty) | msbuild/vstest verified from executor evidence (not reproduced locally); csharpier independently re-verified; source unchanged since gate. |

---

## Summary

**Overall Feature Readiness:** READY (PASS)

**Criteria summary:**
- **PASS:** 7 criteria (AC1, AC2, AC3, AC4, AC5, AC6, AC7) — AC5 is PASS satisfied-with-documented-exception
- **PARTIAL:** 0 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Prior-cycle blocking findings — disposition:**

1. **Finding 1 (canonical C# coverage artifact absent): RESOLVED.** `artifacts/csharp/coverage.xml` exists and is well-formed Cobertura (root `line-rate="0.741108"`; 71654/96685); a repo-wide first-party testable-denominator figure is recorded (73.35%–74.11%).
2. **Finding 2 (AC5 repo-wide sub-claim unverified): RESOLVED.** The repo-wide figure is measured (73.35%/74.11%, below the bare 80% floor) and dispositioned under the maintainer-ratified authority-scoped exception scoped to #223. The new-code (100%), changed-line no-regression (+12.62pp), and test-presence sub-claims are fully satisfied. AC5 is PASS with documented exception.

**Residual (non-blocking) observations:**

1. Repo-wide first-party coverage remains below 80% (pre-existing); uplift owned by #197 under `feature/csharp-coverage-uplift`.
2. Two pre-existing 500-line-cap files remain over cap (`QfcCollectionController.cs`, `QfcFormControllerTests.cs`); accepted as net-negative debt, carried forward as policy debt.

**Recommendation:** Ready for merge for issue #223. No remediation cycle is required. The repo-wide coverage uplift to `>= 80%` is tracked separately under #197.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules:
- Criteria evaluated as **PASS** may be checked off in the authoritative source file(s) if they are markdown checkboxes and not already checked.
- Criteria evaluated as **PARTIAL**, **FAIL**, or **UNVERIFIED** must remain unchecked.

All seven criteria (AC1–AC7) are evaluated PASS this reaudit (AC5 PASS satisfied-with-documented-exception). All seven are already checked `[x]` in `issue.md` (AC5 was re-checked in commit `f4b455e6` under the ratified maintainer decision). No source-file edit was required; the checked state is consistent with this reaudit's PASS evaluations.

### AC Status Summary

- Source: `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/issue.md`
- Total AC items: 7
- Checked off (delivered): 7
- Remaining (unchecked): 0
- Items remaining: none

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `issue.md` | 7 | 7 | 0 | Checkbox-backed; AC5 PASS with documented authority-scoped exception |
| `spec.md` | 0 | 0 | 0 | Prose-only Definition of Done; no `## Acceptance Criteria` checkboxes |
| `user-story.md` | 0 | 0 | 0 | Not authoritative — file absent |
