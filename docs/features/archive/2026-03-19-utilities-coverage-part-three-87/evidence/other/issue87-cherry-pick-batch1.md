# Cherry-Pick Batch 1 — Issue #87 Clean Branch

- **Timestamp:** 2026-03-27T01:23 UTC
- **Command:** `git -C c:\Users\DanMoisan\repos\TaskMaster-issue87-clean cherry-pick 078fd77 3206593 cce7c5a fff20c7`
- **EXIT_CODE:** 0 (after conflict resolution)
- **Conflicts Resolved:**
  - `078fd77`: `ScoCollection_Tests.cs`, `SerializableList_Tests.cs` — accepted theirs (incoming issue #87 test code)
  - `3206593`: `SerializableList.cs` — accepted theirs (captured writer delegate fix)
  - `fff20c7`: `SerializableList_Tests.cs` — accepted theirs (file-system seam tests)
- **Output Summary:** All 4 commits applied. Resulting HEAD SHA: `293ce410776737938e39ed16f2d73839e85cdb58`.
