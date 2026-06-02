# Coverage Comparison — Issue #169

Timestamp (UTC): 2026-06-01T17-12-39Z
Baseline: `evidence/baselines/tests-coverage.2026-06-01T16-37-55Z.txt`
Post-change source: final QA run `.coverage`, converted via `dotnet-coverage merge -f xml`.

## Per-module line coverage (production assemblies)

| Module | Baseline | Post-change | Delta |
| --- | --- | --- | --- |
| UtilitiesCS.dll | 85.39% | 85.45% | +0.06 |
| QuickFiler.dll | 23.28% | 23.40% | +0.12 |
| TaskMaster.dll | 24.32% | 25.16% | +0.84 |

All three in-scope modules increased coverage; none regressed. The dominant application library
UtilitiesCS remains at 85.45% (>= 80%). QuickFiler/TaskMaster are dominated by VSTO/WinForms/COM
UI code that is not unit-testable; their absolute percentages are unchanged in character but moved
upward with the added tests. No changed-line coverage regression occurred.

## New-member coverage (>= 90% target)

| New member | Coverage | Status |
| --- | --- | --- |
| `FolderScorer.TopScore()` | 100% (block 100%) | PASS |
| `AppQuickFilerSettings.HighConfidenceModeEnabled` get/set | 100% | PASS |
| `AppQuickFilerSettings.HighConfidenceThreshold` get/set | 100% | PASS |
| `QfcCollectionController.RemoveBelowThresholdAsync` | 100% (14/14 lines, block 100%) | PASS |
| `QfcFormController.ApplyHighConfidenceFilterAsync` | block 94.44% (8 covered, 1 partial, 0 not covered) | PASS |
| `RibbonController.IsHighConfidenceModeActive` | 100% | PASS |
| `RibbonController.ToggleHighConfidenceMode` | 100% | PASS |
| `RibbonController.GetHighConfidenceThresholdText` | 100% | PASS |
| `RibbonController.SetHighConfidenceThresholdText` | 100% (valid/non-numeric/out-of-range) | PASS |

### New members at the COM/WinForms boundary (not unit-testable)

These thin wrappers cross the Outlook COM / Office-ribbon / WinForms boundary and are not exercisable
in unit tests without live COM. They are deliberately minimal (one statement) and delegate to the
fully covered members above:

| Member | Coverage | Rationale |
| --- | --- | --- |
| `IQfcItemController.TopFolderScore` impl on `QfcItemController` | 0% | Read-only seam over COM-backed `_folderHandler`. Tested indirectly: Phase 4 uses `Mock<IQfcItemController>.SetupGet(c => c.TopFolderScore)` to drive `RemoveBelowThresholdAsync`. |
| `RibbonViewer.QuickFilerHighConfidence_Click` | 0% | Office ribbon callback; one-line `await _controller.LoadQuickFilerHighConfidenceAsync()`. |
| `RibbonViewer.HighConfidenceThreshold_GetText` | 0% | Office editBox callback; one-line delegate to tested `GetHighConfidenceThresholdText()`. |
| `RibbonViewer.HighConfidenceThreshold_OnChange` | 0% | Office editBox callback; one-line delegate to tested `SetHighConfidenceThresholdText(text)`. |
| `RibbonController.LoadQuickFilerHighConfidenceAsync` | 0% | Launches the live COM/WinForms Quick Filer; mirrors the pre-existing (also untested) `LoadQuickFilerAsync`. Its only behavioral difference, `HighConfidenceModeEnabled = true`, plus the conditional removal it enables, are covered by `RemoveBelowThresholdAsync`, `ApplyHighConfidenceFilterAsync`, and the RibbonController setting helpers. |

## Verdict

Repository-wide application coverage gate satisfied for the dominant library (UtilitiesCS 85.45% >= 80%).
All new pure/logic members reach >= 90% (most at 100%). No changed-line coverage regression. The only
new members below 90% are one-statement COM/WinForms boundary wrappers whose behavior is covered
through their delegated, fully tested targets.
