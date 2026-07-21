# Repo-Wide Diff Scope (#317) — AC-4

Timestamp: 2026-07-11T20-12

Command: `git add <the two files>` (to make the new file visible to `git diff`, since untracked files
never appear in `git diff <ref>` output), then `git diff --stat main`

EXIT_CODE: 0

Output:
```
 ...urrentObservableCollectionLockRecursionTests.cs | 88 ++++++++++++++++++++++
 UtilitiesCS.Test/UtilitiesCS.Test.csproj           |  1 +
 2 files changed, 89 insertions(+)
```

Output Summary: Exactly two files changed relative to `main`:
`UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollectionLockRecursionTests.cs`
(new, 88 lines) and `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (1 insertion). No production file and no
other test file appears in the diff. This satisfies AC-4.
