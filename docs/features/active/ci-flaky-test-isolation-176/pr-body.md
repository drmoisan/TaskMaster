## Summary

CI run #197 (the push-merge of PR #174 into `main`) failed in the "Run MSTest suite with coverage" step. Formatting, .NET analyzers, and nullable analysis all passed. Two tests failed intermittently due to test-isolation defects, not production regressions (PR #174 was documentation/archive only). This fixes both root causes so `main` CI is deterministic again, ahead of tightening branch-protection rules on `main`.

Closes #176.

## Root causes and fixes

### 1. Non-thread-safe tracking list (`OlFolderClassifierGroup_Tests`)
Production `OlFolderClassifierGroup.BuildFolderClassifiersAsync` invokes `BuildClassifierAsync` concurrently via `AsyncMultiTasker.AsyncMultiTaskChunker`. The test double recorded each group key into a plain `List<string>` from inside that concurrent callback. Concurrent `List<T>.Add` is not thread-safe and corrupts the backing array — observed in CI as `BuiltGroupingKeys {<null>, "Inbox"}` instead of `{"Inbox", "Projects"}`.

Fix: change the test double tracking store to a thread-safe `ConcurrentBag<string>`, exposed as `IEnumerable<string>`. No assertion changed.

### 2. Real shared-file write handle (`PhysicalFileSystemAdapters_Tests`)
`PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo` opened write/append handles (`AppendText()`, `Open(FileMode.Open)` which defaults to ReadWrite, `OpenWrite()`) against the real `TaskMaster.sln`. Under parallel CI the solution file is held open by another process, so the write opens threw `IOException`.

Fix: add a narrow injectable-delegate seam to `PhysicalFileInfoAdapter` (per the repository `csharp.md` DI-seams guidance) so the three write-mode members can be covered deterministically. The public constructor binds the delegates to the real `FileInfo` method groups, so production runtime behavior is unchanged. The test now exercises read-only members against the real `.sln` (deterministic with `FileShare.ReadWrite`) and verifies write-mode delegation against test-owned sentinel streams via the seam — no temporary/scratch file (which the unit-test policy prohibits) and no write handle on any shared file.

## Scope
- Production: `UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs` (additive seam only; defaults preserve behavior)
- Tests: `OlFolderClassifierGroup_Tests.cs`, `PhysicalFileSystemAdapters_Tests.cs`

## Verification
- csharpier: clean
- msbuild analyzers + code style: build succeeded, zero new warnings in scoped files
- nullable (`TreatWarningsAsErrors`): zero new diagnostics in scoped files
- MSTest: affected classes pass; `PhysicalFileInfoAdapter.cs` coverage increased (0.8909 -> 0.9155); write-mode delegation lines covered
- Authoritative full-suite validation is this PR's CI run.

## Follow-up
- After `main` is green, port both fixes to `development` to prevent reintroduction on the next `development` -> `main` merge.

🤖 Generated with [Claude Code](https://claude.com/claude-code)
