# P8-T8 — Acceptance-Criteria Walk (AC-1 through AC-31)

Timestamp: 2026-08-26T11-30

Command: `pwsh -NoProfile -Command 'Select-String -LiteralPath "docs/features/active/breadcrumb-router-navigation-defects-498/spec.md" -Pattern "^- \[[ x]\] \*\*AC-" | ForEach-Object { $_.Line }; "EXIT_CODE: $LASTEXITCODE"'`, then a per-criterion edit changing the leading marker of each newly satisfied criterion from `- [ ]` to `- [x]` in that same file.

EXIT_CODE: 0

## Output Summary

**All 31 acceptance criteria are checked in `spec.md`. Zero remain unchecked. Zero are UNMET.**

Seven criteria were newly checked by this task — AC-19, AC-20, AC-21, AC-22, AC-29, AC-30 and AC-31 —
each against evidence produced in Phases 7 and 8 of this same execution. The other 24 were already
checked by earlier phases (21 as this feature's work, 3 as RETIRED-INHERITED). Only the leading marker
was changed; no criterion text was modified, and no criterion was added.

Post-edit verification of `spec.md`: 31 lines match `- [x] **AC-`, 0 lines match `- [ ] **AC-`.

### 31-row disposition table

| Criterion | Disposition | Evidence artifact (repo-relative) |
|---|---|---|
| AC-1 | SATISFIED | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/p2-t6-498-controls.md` |
| AC-2 | SATISFIED | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/other/p2-t7-498-invariants.md` |
| AC-3 | SATISFIED | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/other/p2-t7-498-invariants.md` |
| AC-4 | SATISFIED | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/p3-t4-499-green.md` |
| AC-5 | SATISFIED | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/p3-t6-499-preservation.md` |
| AC-6 | SATISFIED | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/p3-t6-499-preservation.md` |
| AC-7 | SATISFIED | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/p4-t6-qfc-prereq-boundaries.md` |
| AC-8 | SATISFIED | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/p4-t5-qfc-prereq-green.md` |
| AC-9 | SATISFIED | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/p4-t6-qfc-prereq-boundaries.md` |
| AC-10 | **RETIRED-INHERITED** (PR #605) | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/qa-gates/p7-t8-inherited-439-coverage.md` |
| AC-11 | SATISFIED | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/p4-t7-qfc-lineage.md` |
| AC-12 | **RETIRED-INHERITED** (PR #605) | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/qa-gates/p7-t8-inherited-439-coverage.md` |
| AC-13 | **RETIRED-INHERITED** (PR #605) | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/qa-gates/p7-t8-inherited-439-coverage.md` |
| AC-14 | SATISFIED | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/other/p5-t6-d7-rung-recorded.md` |
| AC-15 | SATISFIED | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/p6-t4-440-efc-left-green.md` |
| AC-16 | SATISFIED | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/p6-t8-440-efc-right-green.md` |
| AC-17 | SATISFIED | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/p6-t16-440-qfc-router-green.md` |
| AC-18 | SATISFIED | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/p6-t18-d1-handling-order.md` |
| AC-19 | **SATISFIED (newly checked)** | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/qa-gates/p7-t4-ac19-message-shapes.md` |
| AC-20 | **SATISFIED (newly checked)** | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/qa-gates/p7-t5-ac20-selector-session.md` |
| AC-21 | **SATISFIED (newly checked)** | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/qa-gates/p7-t7-ac21-supersession-record.md` |
| AC-22 | **SATISFIED (newly checked)** | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/qa-gates/p7-t6-ac22-400-residual.md` |
| AC-23 | SATISFIED | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/p6-t19-d2-boundaries.md` |
| AC-24 | SATISFIED | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/p6-t19-d2-boundaries.md` |
| AC-25 | SATISFIED | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/p2-t3-498-red.md` |
| AC-26 | SATISFIED | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/p3-t2-499-red.md` |
| AC-27 | SATISFIED | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/p4-t3-qfc-prereq-red.md` |
| AC-28 | SATISFIED | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/p6-t10-440-qfc-model-red.md` |
| AC-29 | **SATISFIED (newly checked)** | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/qa-gates/p8-t6-clean-pass.md` |
| AC-30 | **SATISFIED (newly checked)** | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/qa-gates/p7-t3-ownership-diff.md` |
| AC-31 | **SATISFIED (newly checked)** | `docs/features/active/breadcrumb-router-navigation-defects-498/evidence/qa-gates/p7-t2-file-size.md` |

Totals: **SATISFIED 28, RETIRED-INHERITED 3, UNMET 0.**

### Verification detail for the seven newly checked criteria

- **AC-19 (message shapes).** `FolderBreadcrumbAssetContractTests` ran complete: 15 total, 15 passed, 0
  failed. `LeftAndRightBreadcrumbMessages_RemainSupported` (`:359-367`) is present in the TRX with
  outcome `Passed`. `git status --porcelain --untracked-files=all` over
  `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs` produced no output, and the path is
  absent from the cumulative feature diff recorded by `P7-T3`, so it passed **unmodified**.
- **AC-20 (selector session).** `BreadcrumbStateModelSelectorTests` ran complete: 9 total, 9 passed, 0
  failed. `git status` over both
  `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs` and
  `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSelectorTests.cs` produced no output; both
  are absent from the feature diff.
- **AC-21 (supersession record).** A fixed-string search of `spec.md` for `AC-9 supersession record`
  returns 2 matches, one of which is the section heading at `spec.md:304`. The disposition table at
  `spec.md:306-311` names one retracted clause (expand/collapse, retracted in part) and three preserved
  clauses (unhandled-key behavior, committed/original/pending selector session, and #400 AC-5 to AC-8).
- **AC-22 (#400 residual contract).** The #400 Up/Down/Enter/Escape and state-sequence tests ran
  complete: 32 total, 32 passed, 0 failed. All twelve `[TestMethod]` members declared in
  `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs` were individually
  confirmed present in the TRX with outcome `Passed`. That file is unmodified in both the working tree
  and the cumulative feature diff. The filter had to be resolved by TYPE
  (`BreadcrumbStateModelTests`) rather than by file name, for the pre-existing reason recorded in
  `p7-t6-ac22-400-residual.md`.
- **AC-29 (toolchain).** The final toolchain pass (pass 3) was clean at every step: csharpier format 0
  files rewritten (SHA-256 verified), csharpier check exit 0 over 1525 files, analyzer Rebuild exit 0
  with 0 errors, nullable Rebuild exit 0 with 0 errors, full-suite coverage run exit 0 with 6514/6514
  passed and 0 failed. No degradation branch was used or available at any gate.
- **AC-30 (ownership).** The cumulative in-scope diff against the `P0-T10` baseline commit contains 18
  paths, every one of which is in this plan's OWNED list. All ten files the criterion names explicitly —
  `EfcFormController.cs`, `KbdActions.cs`, `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`,
  `BreadcrumbRowBuilder.cs`, `BreadcrumbDocumentAssets.cs`, `BreadcrumbHtmlRenderer.cs`,
  `BreadcrumbSelectionMap.cs`, `IFolderHierarchyProvider.cs`, `FolderBreadcrumbAssetContractTests.cs`
  and `BreadcrumbBridgeRouterIssue439Tests.cs` — are absent from the diff, plus
  `BreadcrumbSelectionSession.cs` and `FolderPredictor.cs`.
- **AC-31 (file size).** `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` finishes at **310** lines,
  down from 596 at baseline, INCLUDING this feature's additions. Its two new partial siblings carry
  `<Compile Include>` entries in `QuickFiler/QuickFiler.csproj` at lines 291 and 292, adjacent to the
  existing router entry at line 290. Every other file written by this feature is at or under 500 lines
  (maximum 495). `QuickFiler/Resources/FolderBreadcrumb.html` is at **490** lines and was not split. The
  three pre-existing violations are byte-identical to their `P0-T16` baseline by `git hash-object`.

### Retired criteria left checked, not re-verified

AC-10, AC-12 and AC-13 were already checked in `spec.md` as RETIRED inherited-and-verified criteria
delivered by pull request #605. They were left checked, were not re-verified as this feature's work, and
their evidence pointer is `evidence/qa-gates/p7-t8-inherited-439-coverage.md`, which shows the inherited
`BreadcrumbBridgeRouterIssue439Tests` suite still passing 10/10 with its file untouched after this
feature's changes.

### Unmet criteria

**None.** No criterion was left unchecked.
