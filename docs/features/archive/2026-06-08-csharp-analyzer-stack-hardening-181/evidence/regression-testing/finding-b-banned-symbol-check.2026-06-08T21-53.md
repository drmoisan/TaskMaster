# Finding B Banned-Symbol / Out-of-Budget Check (Cycle 5, Issue #181)

Timestamp: 2026-06-08T21-53

Confirmation:
- The Finding B edit touched exactly ONE production file: `UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs` (within the authorized Finding B budget; `ScoDictionaryConverter.cs` was NOT modified). No other production file was changed for Finding B.
- The edit adds `using Newtonsoft.Json.Linq;` and a JObject fallback branch in `ToDerived()` that reads the `Config` token and calls `configToken.ToObject<NewSmartSerializableConfig>()`.
- No banned symbol was introduced. The edit contains none of: `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`.
- No analyzer-wiring, `.editorconfig`/`.globalconfig`, `BannedSymbols.txt`, `<Analyzer Include>`/`packages.config`, or vendored-project file was touched (G4).
- The analyzer build (P0-T4 form) and the Debug build both succeed with 0 errors after the edit; the dedicated final analyzer/nullable gates are run in Phase 5.

## Addendum — NormalizeEmptyDiskFilePaths helper (still in budget)

During Phase 5, the full-suite run surfaced a transient regression in three previously-green `ScoDictionaryConverterTests` integration tests (`TypedConverter_IntegrationTest_SerializeAndDeserialize`, `UntypedConverter_IntegrationTest_SerializeAndDeserialize`, `UntypedConverter_IntegrationTest_SerializeAndDeserialize_InternalJsonProperty`): full-graph `BeEquivalentTo` equivalence broke because a reconstructed empty `FilePathHelper` reports `FilePath == null` while a default-constructed one reports `FilePath == ""`. The resolution stayed strictly inside the authorized Finding B file `UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs`:
- Added two private static helpers `NormalizeEmptyDiskFilePaths(NewSmartSerializableConfig)` and `NormalizeEmptyDiskFilePath(FilePathHelper)` that, for each reconstructed disk with an empty `FileName` and a null `FilePath`, set `FilePath = ""` to restore the documented default invariant (`SmartSerializable_Tests` asserts `Config.Disk.FilePath.Should().BeEmpty()`).
- No other production file was touched. `ScoDictionaryConverter.cs` remains unmodified. No banned symbol (`Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`) introduced. No test weakened, no `[Ignore]` added. The People `Disk.FileName == "pplkey.json"` path is unaffected because the normalization only touches disks whose `FileName` is empty.
