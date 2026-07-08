Timestamp: 2026-06-24T19-23
Task: P2-T1
Evidence Type: expected fail-before regression

Command:
`vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:GetSnapshotAsync_StoreSnapshotThenAllStoresRequest_RebuildsCoveredScope,GetSnapshotAsync_StoreSnapshotThenDifferentStoreRequest_RebuildsRequestedStore`

EXIT_CODE: 1

Output Summary:
- `GetSnapshotAsync_StoreSnapshotThenAllStoresRequest_RebuildsCoveredScope` failed.
- `GetSnapshotAsync_StoreSnapshotThenDifferentStoreRequest_RebuildsRequestedStore` failed.
- Both failures reported `Expected reader.EnumerationCount to be 2, but found 1.`

Interpretation:
The current cache reuses a store-specific current snapshot for an all-store request and for a different store request. This confirms the request-scope mismatch defect before the P2-T2 implementation.
