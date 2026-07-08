Timestamp: 2026-06-24T19-23
Task: P2-T3
Evidence Type: expected fail-before regression

Command:
`vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:FolderChanged_AfterAllStoreSnapshot_PreservesUnaffectedStoreNodes`

EXIT_CODE: 1

Output Summary:
- `FolderChanged_AfterAllStoreSnapshot_PreservesUnaffectedStoreNodes` failed.
- Failure message: `Expected publishedSnapshot.GetNodesForStore("store-b") to contain a single item, but the collection is empty.`

Interpretation:
A store-scoped refresh after an all-store snapshot publishes only the refreshed store scope and drops unaffected store nodes from the published snapshot.
