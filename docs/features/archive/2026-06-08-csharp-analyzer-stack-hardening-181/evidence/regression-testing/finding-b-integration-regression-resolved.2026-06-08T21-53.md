# Finding B Integration-Test Regression Discovery and In-Budget Resolution (Cycle 5, Issue #181)

Timestamp: 2026-06-08T21-53

## Discovery

The Phase 5 full first-party suite run (`trx-final/final-full.trx`) showed three NEW failures not present in the cycle-5 baseline (`trx-full/baseline-full.trx`, where all three PASSED):
- `TypedConverter_IntegrationTest_SerializeAndDeserialize`
- `UntypedConverter_IntegrationTest_SerializeAndDeserialize`
- `UntypedConverter_IntegrationTest_SerializeAndDeserialize_InternalJsonProperty`
(all in `UtilitiesCS.Test/NewtonsoftHelpers/ScoDictionaryConverterTests.cs`)

Failure message (identical for all three):
```
Expected property actual.Config.Disk.FilePath to be "", but found <null>.
Expected property actual.Config.LocalDisk.FilePath to be "", but found <null>.
Expected property actual.Config.NetDisk.FilePath to be "", but found <null>.
```

## Root cause

These tests round-trip a `TestDerived`/`TestDerived2` whose default `Config` has empty `Disk`/`LocalDisk`/`NetDisk` (`FilePath == ""` from the field initializer, never modified). Under their settings, `RemainingObject` binds to a `JObject` (confirmed via captured stdout: `RemainingObject.Config.Disk == {FileName:"", RelativePath:"", SpecialFolderName:"Not Found"}`). Before the Finding B fix, `ToDerived()`'s reflective Config lookup returned null and `derivedInstance.Config` kept its DEFAULT (empty disks, `FilePath == ""`), so equivalence held. The Finding B fix reconstructs Config via `configToken.ToObject<NewSmartSerializableConfig>()`; deserializing an empty disk produces a `FilePathHelper` whose `FilePath` setter routes through `FilePathHelper_PropertyChanged`, which leaves the backing field at `null` for empty paths. `null != ""` broke the full-graph `BeEquivalentTo`.

This is a pre-existing `FilePathHelper` null-vs-empty-string inconsistency (a default helper reports `""`; a deserialized-empty helper reports `null`) that the faithful Config reconstruction exposed. The People test (`Config.Disk.FileName == "pplkey.json"`) was unaffected because it only reads `FileName`, not `FilePath`.

## In-budget resolution (no scope change, no test weakened)

Resolved strictly inside the authorized Finding B file `UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs` by adding two private static helpers invoked immediately after the JObject `ToObject` reconstruction:
- `NormalizeEmptyDiskFilePaths(NewSmartSerializableConfig config)` — applies the normalization to `Disk`, `LocalDisk`, `NetDisk`.
- `NormalizeEmptyDiskFilePath(FilePathHelper disk)` — when `disk.FileName` is empty and `disk.FilePath` is null, sets `disk.FilePath = ""`, restoring the default invariant documented by `SmartSerializable_Tests` (`Config.Disk.FilePath.Should().BeEmpty()`).

The normalization only touches disks with an empty `FileName`, so populated disks (e.g. the People `pplkey.json` disk) are never altered. No other production file changed; `ScoDictionaryConverter.cs` was NOT modified. No `[Ignore]`, no weakened assertion, no banned symbol, no timing hack.

## Verification

Targeted run (`/InIsolation`): all six relevant tests PASS — the three integration tests, both People tests, and the Consume test.
Full first-party suite re-run (`trx-final2/final-full2.trx`): 4064 total, 4055 passed, 9 failed; the 9 failures are EXCLUSIVELY pre-existing flaky wall-clock-timer/dispatcher tests (verified to pass in isolation), and none of the three integration tests nor any of the four target tests appear among them. Post-change coverage 59.06% vs baseline 59.04% (no regression).

## Scope-change disposition

The directive's SCOPE-CHANGE RULE requires HALT only when a new finding would require a production file OUTSIDE the authorized budget, an unauthorized suppression, a file-size breach, or a genuinely unresolvable new defect. Here the regression was fully resolved within the already-authorized Finding B file `WrapperScoDictionary.cs` with a faithful, non-fragile normalization that restores a documented default invariant and weakens nothing. No additional production file was required, so no HALT was triggered; execution completed the plan as written.
