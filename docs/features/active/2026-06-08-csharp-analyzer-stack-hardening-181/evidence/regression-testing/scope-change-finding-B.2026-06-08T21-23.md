# Scope-Change Finding — Finding B Root Cause Is Outside FilePathHelper.cs (Cycle 4, Issue #181)

Timestamp: 2026-06-08T21-23
Reported by: atomic-executor
Status: HALT before commit (SCOPE-CHANGE RULE invoked). No production code changed. Working tree clean except carried-forward ToDoItemTests.cs (G6) and evidence artifacts.

## Summary

The plan ([P1-T2], [P1-T3]) is premised on a minimal `FilePathHelper.cs` constructor fix resolving Finding B (`People_Deserialize_CanDeserializePatternCorrectly`, empty `Config.Disk.FileName`). Empirical diagnostics during execution prove that premise is incorrect: the constructor is already correct for the deserialization inputs, and the converter that would build `Disk` is never invoked. The actual root cause is in the `ScoDictionaryConverter` / `WrapperScoDictionary` deserialization flow under `TypeNameHandling.None`, which is OUTSIDE the two production files the plan authorizes (guardrail G5). Fixing it would require touching production files beyond `FilePathHelper.cs`, exceeding the cycle-4 production budget.

## Evidence (deterministic, reproducible)

### 1. The direct constructor already yields the correct FileName

A reflection probe loading the built `UtilitiesCS.dll` constructed `new FilePathHelper("pplkey.json", "C:\\Users\\user\\AppData\\Roaming")` (the exact inputs the converter would pass during the People deserialization) and observed:

```
FileName=[pplkey.json]
FolderPath=[C:\Users\user\AppData\Roaming]
FilePath=[C:\Users\user\AppData\Roaming\pplkey.json]
```

It also confirmed the `Constructor_WithFileNameAndFolderPath_ShouldSetFilePath` invariant:
```
T2 FileName=[test.json] FolderPath=[C:\data] FilePath=[C:\data\test.json]
```

Therefore the `(fileName, folderPath)` constructor is NOT the source of the empty `FileName` in Finding B. A constructor edit in `FilePathHelper.cs` cannot fix Finding B.

### 2. FilePathHelperConverter.ReadJson is never invoked for Disk

A temporary diagnostic `Console.WriteLine("DIAGPROBE ...")` was added to `FilePathHelperConverter.ReadJson` (and the change has been fully reverted; `git diff` for both FilePathHelper.cs and FilePathHelperConverter.cs is empty). UtilitiesCS was rebuilt and `People_Deserialize_CanDeserializePatternCorrectly` was run in isolation. The DIAGPROBE line appeared ZERO times in the run output, while the test still failed with `Config.Disk.FileName == ""`. This proves the `FilePathHelperConverter` does not participate in deserializing `Config.Disk` for this test.

### 3. Actual root cause: untyped RemainingObject defeats Config population

`ScoDictionaryConverter<TDerived,TKey,TValue>.ReadJson` (UtilitiesCS/NewtonsoftHelpers/ScoDictionaryConverter.cs:16-28) deserializes into `WrapperScoDictionary<TDerived,TKey,TValue>`. That wrapper's `RemainingObject` is declared as `object` (UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs:24-25). With the test's `settings.TypeNameHandling = TypeNameHandling.None`, Newtonsoft deserializes `RemainingObject` as an untyped `JObject` (no `$type` metadata to bind a concrete type).

`WrapperScoDictionary.ToDerived()` (lines 39-142) then reflects over `RemainingObject.GetType()` looking for a `<Config>k__BackingField` / `_Config` field or a `Config` property (lines 54-73). On a `JObject` those lookups all return null, so `configValue` is null and `derivedInstance.Config` is left at its default value (`new NewSmartSerializableConfig()`), whose `_disk = new FilePathHelper()` has an empty `FileName`. This matches the observed assertion failure exactly (`Config` non-null, `Config.Disk.FileName == ""`).

The working sibling `People_DeserializeShortcut_CanDeserializePatternCorrectly` uses `PeopleScoConverter` (a different code path) and passes, consistent with the failure being specific to the `ScoDictionaryConverter` + `TypeNameHandling.None` + untyped-`RemainingObject` path.

## Why this is a scope change (not a micro-action)

- Guardrail G5 limits production touch points to exactly `UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs` and `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs`.
- A correct Finding B fix must change deserialization behavior in `WrapperScoDictionary.cs` and/or `ScoDictionaryConverter.cs` (e.g., binding `RemainingObject` to a concrete type, or populating `Config` from the `JObject`), neither of which is in the authorized set.
- Editing `FilePathHelper.cs` cannot resolve Finding B (proven in items 1-2 above), so completing [P1-T2]/[P1-T3] as written would not make test 3 pass.

This is an independent new outcome not described by the task, so per the SCOPE-CHANGE RULE the executor halts before commit and reports rather than widening scope or re-ignoring/weakening the test.

## Status of the other three target tests (not blocked by this finding)

- Finding A (tests 1 and 2: `FromSeed_ShouldBuildFileNameFromParts`, `CalcMaxSeedLength_WhenInitialized_ShouldSubtractComponentLengths`): root cause is correctly the terminal `FilePath = Path.Combine(...)` re-entry in the seed constructor, IS inside `FilePathHelper.cs`, and IS fixable within budget ([P1-T3]). NOT executed in this cycle because the cycle is halted at [P1-T2] before any production edit.
- Finding C (test 4: `Consume_WhenSequenceProvided_ReturnsItemsAndReportsProgress`): root cause is inside `SubjectMapSco.Orchestration.cs` and IS fixable within budget ([P2-T2]). NOT executed because the cycle is halted.

The executor did not apply ANY production edit (Findings A and C fixes were not started) to keep the halt clean and avoid a partial-fix working tree, per the SCOPE-CHANGE RULE ("HALT immediately before commit, do NOT widen scope").

## Recommended next action (for orchestrator, not executed here)

Open a new remediation cycle whose inputs/plan either:
- (a) expand the authorized production budget to include `UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs` and/or `UtilitiesCS/NewtonsoftHelpers/ScoDictionaryConverter.cs` for Finding B, OR
- (b) re-scope Finding B to the serialization layer with a revised root-cause statement,

and keep Findings A and C within the existing two-file budget. The revised plan should update the [P1-T2] root-cause text, since the current text ("the People-deserialization path yields a non-empty FileName" via a FilePathHelper constructor fix) is contradicted by the evidence above.
