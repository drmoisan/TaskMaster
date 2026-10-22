# P6-T12 Footprint and Scope Boundary

Timestamp: 2026-10-03T12-55
ITERATION: 1
Command: git -C WORKTREE merge-base HEAD origin/main (no fetch); then CMD-FOOTPRINT with MERGE-BASE 94287369908cc920b21b0e3256314f988ad7d2f5 and INHERITED the 27 quoted INHERITED-CLAUSE-A paths of P0-T3, run as pwsh -NoProfile -Command with Set-Location to the item worktree (the payload pairs git diff --name-only MERGE-BASE with git status --porcelain --untracked-files=all)
EXIT_CODE: 0 (scoped to the CMD-FOOTPRINT payload, its process exit code)
Output Summary: the anchor has not moved; no path outside the eighteen Write Set paths, FEATURE/, Clause A and Clause B is changed; every Write Set path is changed; exactly the two planned deletions; no raw coverage or test document; every numstat matches.

- ANCHOR-RECHECK: 94287369908cc920b21b0e3256314f988ad7d2f5

```
FOOTPRINT-PATHS: 122
SUBTRACTED-CLAUSE-A: 27
SUBTRACTED-CLAUSE-B: 24
OUTSIDE-WRITE-SET: 0
WRITE-SET-MISSING: 0
DELETED-PATHS: ToDoModel/Email Utilities/SortItemsToExistingFolder.cs,UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs
RAW-DOC-PATHS: 0
NUMSTAT-UCS: 0	1	UtilitiesCS/UtilitiesCS.csproj
NUMSTAT-UCT: 3	0	UtilitiesCS.Test/UtilitiesCS.Test.csproj
NUMSTAT-QFT: 1	0	QuickFiler.Test/QuickFiler.Test.csproj
NUMSTAT-SPEC956: 4	2	docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md
NUMSTAT-S: 0	9	UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs
NUMSTAT-M: 0	9	UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs
NUMSTAT-TST2: 37	0	UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs
```

No OUTSIDE: and no MISSING: rows were printed.

## Acceptance (P6-T12, all eight required)

1. ANCHOR-RECHECK equals MERGE-BASE of P0-T3 (94287369908cc920b21b0e3256314f988ad7d2f5): met.
2. OUTSIDE-WRITE-SET: 0: met.
3. WRITE-SET-MISSING: 0: met.
4. DELETED-PATHS lists exactly UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.LegacyAttachmentSaving.cs and ToDoModel/Email Utilities/SortItemsToExistingFolder.cs: met.
5. RAW-DOC-PATHS: 0: met.
6. NUMSTAT-UCS reads 0 1 UtilitiesCS/UtilitiesCS.csproj, NUMSTAT-UCT reads 3 0 UtilitiesCS.Test/UtilitiesCS.Test.csproj, NUMSTAT-QFT reads 1 0 QuickFiler.Test/QuickFiler.Test.csproj, NUMSTAT-SPEC956 reads 4 2 and the spec path: met.
7. NUMSTAT-S and NUMSTAT-M each read 0 9 and the respective path: met.
8. The subtracted Clause A (27) and Clause B (24) counts are recorded: met.
