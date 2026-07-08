# Phase 3 — AC8 Scope Confirmation (Part A) (Issue #232)

Timestamp: 2026-07-03T12-08
Command: git diff --stat 00507b595297c3e6970634a1855f1144c987dbdf -- QuickFiler/ QuickFiler.Test/

git diff --stat output (Part A, before Phase 4 Part B edits):
```
 .../Controllers/QfcCollectionControllerTests.cs    | 172 +++++++++++++++++++++
 QuickFiler/Controllers/QfcCollectionController.cs  |  16 +-
 2 files changed, 186 insertions(+), 2 deletions(-)
```

Confirmation statements:
- Part A production change is confined to QuickFiler/Controllers/QfcCollectionController.cs (16 lines) plus its dedicated test file QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs (+172). No other production file is modified for Part A.
- QuickFiler/Controllers/QfcDatamodel.cs is UNTOUCHED (git diff returns empty). The fixed-batch-without-backfill pattern in InitEmailQueue / InitEmailQueueAsync / DequeueNextItemGroupAsync / WaitForQueue is not modified.
- No QfcHighConfidencePreFilterLoader.cs change appears (the dormant Issue #171 pre-filter loader is not wired up).
- The removespecificcontrolgroupcounter reentrancy-counter logic is UNCHANGED: the field declaration (line 1142), Interlocked.Increment (1146), the >1 Error check (1222), and Interlocked.Decrement (1232) are byte-identical to baseline. The P2-T2 guard added a separate method-local bool (swapAlreadyRegistered) and did not alter the counter or its hygiene.

Note: Part B (additive logging in QfcDatamodel.cs, QfcItemController.FolderHandling.cs, QfcHighConfidencePreFilter.cs) is applied in Phase 4 and is a separate, non-overlapping change set. This AC8 confirmation covers Part A's non-scope-creep guarantee; the final consolidated diff is recorded at P6-T3.
