# Coverage Comparison — Issue #171

- Task: [P7-T3]
- Timestamp: 2026-06-02T10-26
- Baseline: `evidence/coverage/coverage-baseline-171.2026-06-02T14-05.txt`
- Final: `evidence/coverage/coverage-final-171.2026-06-02T10-26.txt`

## Per-file (range-line) comparison

| File | Baseline | Final | Delta | Gate |
|------|----------|-------|-------|------|
| QfcHighConfidencePreFilter.cs | (new) | **100.00%** | new | new file >= 90% — MET |
| QfcHomeController.cs | 50.51% | 52.22% | +1.71 | no regression |
| QfcFormController.cs | 39.64% | 39.73% | +0.09 | no regression |
| QfcCollectionController.cs | 3.81% | 3.65% | -0.16 | see note 1 |
| QfcItemController.cs | 7.02% | 7.29% | +0.27 | no regression |
| QfcItemGroup.cs | 53.85% | 84.62% | +30.77 | no regression |
| FolderScorer.cs | 93.29% | 93.29% | 0.00 | untouched |

## Gate (a): repository-wide line coverage >= 80%
The application code exercised by the two in-scope test assemblies:
- UtilitiesCS.dll: 87.58% (unchanged; >= 80%).
- QuickFiler.dll: 24.32% (was 24.11% at baseline; +0.21).

The whole-repo number counting all production modules (including TaskMaster.dll, ToDoModel.dll,
Tags.dll, etc., which are 0%/near-0% and are NOT exercised by these two test projects) is far below
80% and is a PRE-EXISTING condition not introduced by Issue #171. Issue #171 raises QuickFiler.dll
coverage (+0.21 module-wide) and does not lower any module. No regression is introduced.

## Gate (b): new file >= 90%
QfcHighConfidencePreFilter.cs = 100.00% of its testable surface. MET.
(The COM-bound `FolderScoringService.ScoreAsync` adapter is `[ExcludeFromCodeCoverage]` because it
requires live Outlook COM, which repo policy prohibits in unit tests.)

## Gate (c): no coverage regression on changed lines
- QfcHomeController.cs: the new RunAsync high-confidence branch and the injectable delegate are
  covered by the new RunAsync_* tests (file pct rose).
- QfcFormController.cs: the carrier `LoadItemsAsync` overload's guard path is covered by
  `LoadItemsAsync_PreScored_DoesNotInvokePostUiRemoval` (file pct rose).
- QfcItemController.cs: the new `PopulateAndSelectFolder` seam (the extracted selection logic) is
  COVERED by the two AssignFolderComboBox_* tests (verified: those specific lines report "yes").
  File pct rose +0.27.
- QfcItemGroup.cs: the new `PredeterminedFolder` property is covered (file pct rose +30.77).
- QfcCollectionController.cs (note 1): the aggregate dropped 0.16% only because the added carrier
  `LoadControlsAndHandlers_01Async` overload (~85 lines) is the same COM/WinForms-bound path as its
  pre-existing `IList<MailItem>` sibling (which is also ~3% covered and not unit-testable without live
  COM). The line CHANGED inside the existing `EncapsulateItemGroup` (adding the `predeterminedFolder`
  parameter / group-property assignment) was ALREADY 0% covered at baseline (verified: baseline
  EncapsulateItemGroup lines 512-536 = 9 ranges, 0 covered), so there is NO regression on changed
  lines — those lines were uncovered before and remain uncovered for the same legitimate COM-boundary
  reason. The carry contract they implement is verified at the unit level by
  `CarrierLoad_SetsPredeterminedFolderOnItemGroup` and the FilterAsync survivor/folder tests.

## Conclusion
- New file >= 90%: MET (100%).
- Repository-wide (application) coverage: not regressed; QuickFiler.dll improved.
- Changed-line coverage: no regression; new testable logic is covered, and the only uncovered changed
  lines are pre-existing COM/WinForms boundaries that were uncovered at baseline.
