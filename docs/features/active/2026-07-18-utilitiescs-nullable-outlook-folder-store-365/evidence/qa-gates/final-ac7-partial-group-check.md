# Final AC7 — Partial-Class Groups Remediated Together (P12-T9)

Timestamp: 2026-07-19T16-40

## Group 1: FolderPredictor.cs + FolderPredictor.IFolderSearchHandler.cs
- Both parts pragma-enabled and annotated in the single task **P4-T11** (batch F3d), one commit
  (`feat(365): nullable-enable Batch F3d FolderPredictor partial pair (P4 T11-T14)`).
- Evidence: `evidence/qa-gates/batch-f3d-nullable-gate.md` reports zero CS86xx for BOTH files together.
- Shared-member consistency: `FolderPredictor`'s `IFolderSearchHandler` implementation matches the interface
  shape decided in P1-T2 (FolderArray/Suggestions/FolderRowArray non-null; FindFolder default-null optional
  params). The partial declaration file carries the pragma; the members live in FolderPredictor.cs.

## Group 2: StoresWrapper.cs + StoresWrapper.Filtering.cs
- Both parts pragma-enabled and annotated in the single task **P8-T4** (batch S2b), one commit
  (`feat(365): nullable-enable Batch S2 store domain classes (P8 T4-T9)`).
- Evidence: `evidence/qa-gates/batch-s2b-nullable-gate.md` reports zero CS86xx for BOTH files together.
- Shared-member consistency: the static `StoreIsIncluded` overload (Filtering.cs) and the instance
  `ShouldIncludeStore` (StoresWrapper.cs) carry a consistent nullable shape for `storeId`/`excludedStoreIds`/
  `filePath`; `Globals`/`Stores` nullable are declared once on the primary part and consumed consistently.

Both partial-class groups were remediated in their respective single tasks with consistent shared-member
nullability (AC7).
