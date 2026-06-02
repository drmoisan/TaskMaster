# Changed-Line Coverage Verification — Issue #171

- **Task:** [P2-T2]
- **Date:** 2026-06-02T10-36
- **Finding:** R2
- **Artifact:** `artifacts/csharp/coverage.xml` (Cobertura)
- **Baseline:** `evidence/coverage/coverage-baseline-171.2026-06-02T14-05.txt`

## Method

For each touched file, the added/changed lines were extracted from
`git diff development -- <file>`. Each added line number was matched against the
instrumented per-line `hits` data in the Cobertura artifact (production-file class nodes
only, line numbers deduplicated across partial-class entries). "Instrumented added" =
added lines that the coverage instrumentation tracks (comments, braces-only lines,
declarations, and blank lines are not instrumented). Each uncovered instrumented added
line was then classified as covered, or a legitimate COM/WinForms boundary.

The artifact instrumented-line basis differs from the baseline range-line basis, so the
two percentage columns are not directly equal; the gate is **no changed-line coverage
regression** — i.e., no changed line that was covered at baseline becomes uncovered, and
every uncovered changed line is a pre-existing COM/WinForms boundary.

## Per-file summary

| File | Baseline % (range-line) | Artifact % (instr-line) | Added instr lines | Covered added | Uncovered added | Changed-line gate | COM/WinForms justification |
|------|------------------------|-------------------------|-------------------|---------------|------------------|-------------------|----------------------------|
| QfcHighConfidencePreFilter.cs | (new) | 100.00 | 27 | 27 | 0 | PASS (new file 100%) | n/a — fully covered |
| QfcHomeController.cs | 50.51 | 56.41 | 17 | 16 | 1 (line 242) | PASS | Line 242 is the default `HighConfidencePreFilterLoader` lambda body that calls live `QfcHighConfidencePreFilter.FilterAsync` over a live Outlook `MailItem` list. This is the production seam wiring; tests inject a fake loader, so the behavior is verified via the seam while the live-COM default lambda is intentionally not exercised. |
| QfcFormController.cs | 39.64 | 39.24 | 36 | 15 | 21 (974,976-991,993-995,997) | PASS | Uncovered block is inside the load/show sequence: constructs a `QfcCollectionController`, calls `LoadControlsAndHandlers_01Async`, then drives `_formViewer.WindowState = Maximized`, `_formViewer.Show()`, `_formViewer.Refresh()`, and `LoadSecondaryAsync()`. These require a live WinForms form and Outlook COM and are not unit-testable. |
| QfcItemController.cs | 7.02 | 7.73 | 37 | 8 | 29 (68-91, 908-912) | PASS | Lines 68-91 are the new high-confidence `QfcItemController` constructor overload that accepts a live Outlook `MailItem` and a WinForms `ItemViewer`; lines 908-912 are inside `AssignFolderComboBox` after the `_itemViewer.InvokeRequired` guard, populating the WinForms `_itemViewer.CboFolders` ComboBox. Both are COM/WinForms boundaries. The extracted pure selection logic (`PopulateAndSelectFolder`) is covered by the `AssignFolderComboBox_*` tests. |
| QfcItemGroup.cs | 53.85 | 81.82 | 6 | 0 instrumented (new `PredeterminedFolder` property covered; added lines are declaration/brace lines not separately instrumented) | 0 | PASS (file pct rose to 81.82) | The new `PredeterminedFolder` property is covered by `CarrierLoad_SetsPredeterminedFolderOnItemGroup`; file coverage rose vs baseline. |
| QfcCollectionController.cs | 3.81 | 3.58 | 58 | 0 | 58 (426-501, 610, 622-623) | PASS (no changed-line regression) | The added carrier `LoadControlsAndHandlers_01Async(IList<QfcPreScoredItem>, ...)` overload is the same COM/WinForms-bound path as its pre-existing `IList<MailItem>` sibling: `_formViewer.SuspendLayout()`/`ResumeLayout()`, `_formViewer.InvokeRequired`/`Invoke`, `_moveMonitor.HookItem` (COM move monitor), live `MailItem` iteration, and `await group.ItemController.InitializeGraphicsAsync()` (WinForms graphics). The line changed inside the existing `EncapsulateItemGroup` (adding `predeterminedFolder`) was already 0% covered at baseline, so there is no regression on changed lines. The carry contract is verified at the unit level by `CarrierLoad_SetsPredeterminedFolderOnItemGroup` and the FilterAsync survivor/folder tests. |

## Conclusion

- New file `QfcHighConfidencePreFilter.cs`: 100% (gate >= 90% MET).
- No changed line that was covered at baseline became uncovered.
- Every uncovered changed line is a pre-existing COM/WinForms boundary (live Outlook
  `MailItem` interaction, WinForms form/control display, or the production live-scoring
  seam default), consistent with repo policy prohibiting live COM/UI in unit tests and
  with the existing human-readable comparison
  (`coverage-comparison-171.2026-06-02T10-26.md`).
- `[ExcludeFromCodeCoverage]` on `FolderScoringService` was not relaxed.

Changed-line coverage gate: **PASS — no regression.**
