Timestamp: 2026-08-31T10:30:11-04:00
Search 1: rg -n "deferred to issue #637" --glob "*.cs" .
Search 2: rg -n "#637" --glob "*.cs" .
EXIT_CODE: 0 for both searches
Output Summary: Search 1 returned exactly 3 deferral records. Search 2 returned the same 3-line set; no additional #637 references required classification.

Deferral records (verbatim):
- QuickFiler/Controllers/EfcSelectionGuard.cs:30: /// normalization in BreadcrumbBridgeRouter.SelectRow is deferred to issue #637.
- QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:146: // RC-1 inversion: rooted values are never filing stems here; normalization is deferred to issue #637.
- QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs:152: "a rooted value is never a filing stem at this surface and producer-side normalization is deferred to issue #637"
