---
name: legacy-scodictionary-removal-315
description: Issue #315 (legacy-scodictionary-removal) test-file classification — DELETE vs RETARGET vs OUT-OF-SCOPE for the 5 code-referencing test files, plus csproj/interface residue findings
metadata:
  type: project
---

Issue #315 retires the legacy `ScoDictionary<TKey,TValue>` (Swordfish-based,
`UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/SCODictionary.cs`) now that F1 (#306,
see [[swordfish-removal-epic-306]]) re-pointed all production consumers to `ScoDictionaryNew`. Zero
production consumers of the old class remain.

**Why:** worktree `legacy-scodictionary-removal-315`; classification work needed to scope an atomic
plan without breaking generic-infrastructure test coverage.

**How to apply:**
- DELETE (whole file): `SCODictionary_Tests.cs` + `SCODictionary_Additional_Tests.cs` (one partial
  class spanning two files) — tests `Filename`/`Filepath`/`Folderpath`/backup-loader API that has no
  `ScoDictionaryNew` equivalent (`Config`/`ism`/`SmartSerializable<T>` shape instead).
- RETARGET (partial-file swap `ScoDictionary<...>` -> `ScoDictionaryNew<...>`, pure type swap, no
  JSON-shape risk): `SmartSerializableBase_Tests.cs` (3 usages), `SmartSerializableNonTyped_Tests.cs`
  (5 usages), `SmartSerializableStatic_Tests.cs` (1 usage). Safe because `ScoDictionaryNew` has a
  parameterless ctor (`where T : class, new()` constraint satisfied) and its
  `ISmartSerializable<ScoDictionaryNew<TKey,TValue>>` implementation is commented out in
  `ScoDictionaryNew.cs:22`, so `IsSmartSerializable` still returns false — same as the old class.
  Both types serialize as flat `{"key":value}` JSON under bare/default settings (JsonDictionaryContract
  ignores non-dictionary members unless the globals `ScoDictionaryConverter`/`WrapperScoDictionary`
  path is explicitly registered, which none of these tests do).
- COMMENT-ONLY (no change): `FolderRemapController_Tests.cs`, `SubjectMapEncoder_Tests.cs`,
  `FolderScorer.cs:239-240`.
- OUT OF SCOPE (verified zero old-class references): `IntelligenceConfig_Tests.cs`,
  `ScoDictionaryConverterTests.cs`, `WrapperScoDictionaryTest.cs` — all exclusively reference
  `ScoDictionaryNew`.
- `IScoDictionary<TKey,TValue>` interface (file `ISCODictionary.cs`) and `IPeopleScoDictionary` have
  zero source-level dependency on the `ScoDictionary` class itself — safe to leave for F5 (#308);
  do not delete as part of #315.
- Exact csproj lines: `UtilitiesCS.csproj:1048` (`SCODictionary.cs`);
  `UtilitiesCS.Test.csproj:380-381` (`SCODictionary_Tests.cs`, `SCODictionary_Additional_Tests.cs`).
- Residual Swordfish binding after deletion: only `UtilitiesCS.Test/ReusableTypeClasses/
  ObservableDictionary_Tests.cs` still has `using Swordfish.NET.Collections;`, but it tests the
  unrelated `ObservableDictionary` type, not `ScoDictionary` — out of scope, untouched.
- Existing on-disk-compat coverage (`ScoDictionaryNew_OnDiskCompatibility_Tests.cs`) already proves
  the flat-JSON/no-wrapper-token claim for all 4 persisted dictionaries; the RETARGET files don't need
  a new compat assertion since they test generic infra, not persisted dictionaries.
Full research: docs/features/active/2026-07-11-legacy-scodictionary-removal-315/research/2026-07-11T11-20-scodictionary-retirement-classification.md
