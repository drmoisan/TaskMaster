# File-Size Constraint Check — Issue #171

- Task: [P7-T5]
- Timestamp: 2026-06-02T10-26
- Baseline: `evidence/baseline/file-line-counts-171.2026-06-02T14-05.txt`

## Oversized controller deltas

| File | Baseline | Final | Delta | Nature of additions |
|------|----------|-------|-------|---------------------|
| QfcCollectionController.cs | 2206 | 2297 | +91 | one carrier `LoadControlsAndHandlers_01Async` overload (mirrors the existing one) + a `predeterminedFolder` parameter and one assignment in `EncapsulateItemGroup` |
| QfcItemController.cs | 2431 | 2498 | +67 | one constructor overload (`predeterminedFolder`), one private field, and the extracted `PopulateAndSelectFolder` seam invoked by `AssignFolderComboBox` |
| QfcFormController.cs | 1082 | 1142 | +60 | one carrier `LoadItemsAsync` overload pair (parameterless + ProgressTracker) |
| QfcHomeController.cs | 724 | 759 | +35 | one injectable delegate property + the conditional pre-filter branch in `RunAsync` |

Total controller additions: +253 lines (interface/overload/property/seam glue only).

## New file (where the bulk of new logic lives)

| File | Lines |
|------|-------|
| QuickFiler/Controllers/QfcHighConfidencePreFilter.cs | 182 |

The carrier type, the `IFolderScoringService` seam, the default `FolderScoringService` adapter, and
the `FilterAsync` scoring/filter logic — the substantive new behavior — are all in the new file, not
in the oversized controllers.

## Assessment
- All four controllers were already over the 500-line limit before Issue #171 (pre-existing
  violations, not introduced here).
- The per-file additions are minimal glue: a single overload (or overload pair), a single property,
  a single field, and one extracted seam. No file received a materially larger or unrelated body.
- The bulk of the new scoring/filter logic (182 lines) is isolated in the new file
  `QfcHighConfidencePreFilter.cs`, satisfying the "not materially worse" constraint and AC8.

Conclusion: file-size constraint satisfied — oversized files are not made materially worse.
