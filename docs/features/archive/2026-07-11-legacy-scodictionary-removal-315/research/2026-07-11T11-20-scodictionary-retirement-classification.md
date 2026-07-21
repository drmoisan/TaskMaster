# ScoDictionary Retirement — Test Classification Research

Timestamp: 2026-07-11T11-20
Scope: classify the five test files that reference the legacy `ScoDictionary<TKey,TValue>`
(`UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/SCODictionary.cs`) in live code, and
determine the exact production/test footprint of retiring that class. No source files were
modified; all findings below are from direct reads of the feature worktree
(`C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315`).

## Summary verdicts

| File | Classification | Rationale |
|---|---|---|
| `UtilitiesCS.Test/ReusableTypeClasses/SCODictionary_Tests.cs` | **DELETE** | Every test exercises `ScoDictionary`-only serialization API surface (`Filename`/`Filepath`/`Folderpath` setters, `Serialize`/`Deserialize` overloads, backup-loader flow) that has no equivalent shape on `ScoDictionaryNew` (which uses `Config`/`ism`/`SmartSerializable<T>` instead). Nothing here is generic-infrastructure coverage. |
| `UtilitiesCS.Test/ReusableTypeClasses/SCODictionary_Additional_Tests.cs` | **DELETE** | `partial class SCODictionary_Tests` continuation. Defines `RecordingScoDictionary : ScoDictionary<string,int>` overriding `DirectoryExists`, `ReadAllText`, `CreateText`, `CreateAsyncWriteStream`, `ShowMessageBox`, `ShowMyBoxDialog` — protected virtual hooks that exist only on the legacy class. Must be deleted together with `SCODictionary_Tests.cs` (single logical test class spanning two files). |
| `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableBase_Tests.cs` | **RETARGET** (partial file) | Two test methods (`DeserializeObject_ValidJson_ReturnsInstance`, `DeserializeObject_InvalidJson_ReturnsNull`) use `ScoDictionary<string,int>` purely as a stand-in concrete type to exercise `SmartSerializableBase.DeserializeObject<T>()` generic JSON round-trip infrastructure. All other tests in the file use `BaseTestItem`/`BaseLoaderItem`/`TestData`/`ScDictionary` (a distinct, already-first-party, non-Swordfish type at `ReusableTypeClasses/SerializableNew/Concurrent/ScDictionary.cs` — not in scope). |
| `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableNonTyped_Tests.cs` | **RETARGET** (partial file) | Four test methods use `ScoDictionary<string,int>` as a stand-in "type that does not implement `ISmartSerializable<>`" and as a generic JSON round-trip subject: `IsSmartSerializable_ScoDictionaryInstance_ReturnsFalse`, `IsSmartSerializable_TypeOverload_ScoDictionary_ReturnsFalse`, `DeserializeObject_ValidJson_ReturnsInstance`, `DeserializeObject_InvalidJson_ReturnsDefault`. One method (`DeserializeObject_SmartSerializable_SetsConfigJsonSettings`) uses the unrelated `ScDictionary` type — not in scope. |
| `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableStatic_Tests.cs` | **RETARGET** (partial file) | One test method, `IsSmartSerializable_ScoDictionary_ReturnsFalse`, uses `typeof(ScoDictionary<string,int>)` as a stand-in "type that does not implement `ISmartSerializable<>`". All other methods use unrelated types (`SmartSerializable<>`, `ConcurrentObservableCollection<int>`, `string`, `int`, `ScBag<int>`, `object`). |
| `UtilitiesCS.Test/EmailIntelligence/FolderRemapController_Tests.cs` | **COMMENT-ONLY** | Line 162 is a doc comment ("Uses an empty ScoDictionary so Serialize() is a no-op"); the actual code at line 169 constructs `new ScoDictionaryNew<string, string>()`. No live reference to the old class. No change needed. |
| `UtilitiesCS.Test/EmailIntelligence/SubjectMapEncoder_Tests.cs` | **COMMENT-ONLY** | Lines 91 and 102 are doc comments referencing `ScoDictionary`'s old null-folderpath throw behavior for historical context; line 116 documents the current `ScoDictionaryNew` path. No live reference to the old class. No change needed. |
| `UtilitiesCS.Test/EmailIntelligence/IntelligenceConfig_Tests.cs` | **OUT OF SCOPE** | Confirmed via full-file grep: every code reference is to `ScoDictionaryNew`, `PeopleScoDictionaryNew`, or the private static `IsDerivedFromScoDictionaryNew` helper. No reference to the old `ScoDictionary` class anywhere in the file (doc comments and identifiers alike are all `...ScoDictionaryNew...`). |
| `UtilitiesCS.Test/NewtonsoftHelpers/ScoDictionaryConverterTests.cs` | **OUT OF SCOPE** | Confirmed via full-file grep: `TestDerived`/`TestDerived2`/`DerivedSimple` all derive from `ScoDictionaryNew<string,int>`; `ScoDictionaryConverter<TestDerived,...>` and `ScoDictionaryConverter` (parameterless) are the current, in-use converter types (not tied to the old class). No reference to the old `ScoDictionary` class. |
| `UtilitiesCS.Test/NewtonsoftHelpers/WrapperScoDictionaryTest.cs` | **OUT OF SCOPE** | Confirmed via full-file grep: `TestDerived : ScoDictionaryNew<string,int>`, `DerivedTest2 : ScoDictionaryNew<string,string>`, and all `WrapperScoDictionary<TestDerived,...>` / `WrapperScoDictionary<DerivedTest2,...>` instantiations are generic-parameterized against `ScoDictionaryNew`-derived types only. No reference to the old `ScoDictionary` class. |

## Per-file change detail (RETARGET files)

### `SmartSerializableBase_Tests.cs`
- Line 52: `var source = new ScoDictionary<string, int>();` → `new ScoDictionaryNew<string, int>();`
- Line 58: `sut.DeserializeObject<ScoDictionary<string, int>>(json, settings)` → `sut.DeserializeObject<ScoDictionaryNew<string, int>>(json, settings)`
- Line 73: `sut.DeserializeObject<ScoDictionary<string, int>>(` (in `DeserializeObject_InvalidJson_ReturnsNull`) → `sut.DeserializeObject<ScoDictionaryNew<string, int>>(`
- No other lines in the file reference the old class.

### `SmartSerializableNonTyped_Tests.cs`
- Line 24: `var instance = new ScoDictionary<string, int>();` → `new ScoDictionaryNew<string, int>();`
- Line 50: `var type = typeof(ScoDictionary<string, int>);` → `typeof(ScoDictionaryNew<string, int>)`
- Line 76: `var dict = new ScoDictionary<string, int>();` → `new ScoDictionaryNew<string, int>();`
- Line 82: `sut.DeserializeObject<ScoDictionary<string, int>>(json, settings)` → `sut.DeserializeObject<ScoDictionaryNew<string, int>>(json, settings)`
- Line 96: `sut.DeserializeObject<ScoDictionary<string, int>>(` (invalid-json test) → `sut.DeserializeObject<ScoDictionaryNew<string, int>>(`
- Comment text at lines 23 and 49 ("ScoDictionary does not implement ISmartSerializable<>") should be updated to say `ScoDictionaryNew` to stay accurate, since the underlying fact (no `ISmartSerializable<>` implementation — see below) still holds for the new type.
- Test method names (`IsSmartSerializable_ScoDictionaryInstance_ReturnsFalse`, `IsSmartSerializable_TypeOverload_ScoDictionary_ReturnsFalse`) may be left as-is or renamed to `...ScoDictionaryNew...`; this is a style choice with no behavioral impact.

### `SmartSerializableStatic_Tests.cs`
- Line 29: `var type = typeof(ScoDictionary<string, int>);` → `typeof(ScoDictionaryNew<string, int>)`
- Comment at line 28 should be updated similarly.

## Drop-in verification for the RETARGET substitution

`ScoDictionaryNew<TKey, TValue>` (`UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/Observable/ScoDictionaryNew.cs`) is used in every RETARGET usage only via:
1. A parameterless constructor — `public ScoDictionaryNew() : base() { InitIsm(); }` exists and satisfies the `where T : class, new()` constraint on `SmartSerializableBase.DeserializeObject<T>(...)`.
2. `typeof(ScoDictionaryNew<string,int>)` as a plain `Type` value — no constructor needed.
3. `.Add(...)` (inherited `IDictionary<TKey,TValue>` member from `ConcurrentObservableDictionary<TKey,TValue>`, confirmed present in the base at `UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Dictionary/ConcurrentObservableDictionary.cs`).

None of the RETARGET usages need the old class's `(IDictionary<TKey,TValue> source)`, `(IEqualityComparer<TKey>)`, or `(int capacity)` constructors — those exist only in `SCODictionary_Tests.cs` (a DELETE file). `ScoDictionaryNew` has a materially different constructor set (`IEnumerable<KeyValuePair<TKey,TValue>> collection`, `(int concurrencyLevel, int capacity)`, etc.) — this divergence is irrelevant to the RETARGET files since they only use the parameterless constructor or `typeof(...)`.

**`IsSmartSerializable` behavior parity (confirmed by reading source):**
- `ScoDictionaryNew<TKey,TValue>`'s class declaration is `: ConcurrentObservableDictionary<TKey, TValue>, /*ISmartSerializable<ScoDictionaryNew<TKey, TValue>>,*/ IScoDictionaryNew<TKey, TValue>` — the `ISmartSerializable<>` implementation is commented out (`ScoDictionaryNew.cs:22`).
- `SmartSerializableStatic.IsSmartSerializable(this Type type)` and `SmartSerializableNonTyped.IsSmartSerializable(Type type)` both check `type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISmartSerializable<>))`.
- Because the interface is commented out, `typeof(ScoDictionaryNew<string,int>).GetInterfaces()` does not include `ISmartSerializable<>`, so `IsSmartSerializable` returns `false` for `ScoDictionaryNew` — identical to the old `ScoDictionary` (which never implemented `ISmartSerializable<>` at all). The three `...ReturnsFalse` RETARGET tests keep passing under the swap.

**`DeserializeObject<T>` side note (non-breaking):** `SmartSerializableBase.DeserializeObject<T>` calls `SetConfig(instance, settings.DeepCopy())` after a successful deserialize, which uses reflection (`typeof(T).GetProperty("Config")`) to set `instance.Config.JsonSettings` if a `Config` property exists. The old `ScoDictionary` has no `Config` property, so this was always a no-op for the DELETE-file tests. `ScoDictionaryNew` *does* expose `[JsonProperty] public NewSmartSerializableConfig Config { get; set; }`, so after the swap `SetConfig` will actually populate `Config.JsonSettings` on the deserialized instance. None of the four RETARGET `DeserializeObject_*` tests assert on `Config`, so this behavioral difference does not affect any existing assertion — it is a side effect worth knowing about but not a blocker.

## JSON on-disk compatibility — verdict: **compatible, existing coverage sufficient, no new assertion needed**

Evidence:
- `ScoDictionaryNew<TKey,TValue>` extends `ConcurrentObservableDictionary<TKey,TValue>` and implements `IDictionary<TKey,TValue>`, so Newtonsoft's default contract resolver treats it via a `JsonDictionaryContract` (dictionary entries only) unless a converter forces object-contract wrapping. The plain, unconfigured `JsonConvert.SerializeObject`/`DeserializeObject` calls used throughout the RETARGET tests (via `sut.GetDefaultSettings()` / `NewSmartSerializableConfig.GetDefaultSettings()`, both `{ TypeNameHandling = Auto, Formatting = Indented }`, no converters registered) therefore serialize/deserialize only the dictionary's key/value entries — `Config`, `Name`, and `ism` are silently excluded, exactly mirroring the old `ScoDictionary`'s flat `{"key":value}` shape (see `SCODictionary.cs:226-227`, where `TypeNameHandling.Auto` is commented out and the type is serialized via `serializer.Serialize(sw, this)` under a bare `JsonSerializerSettings`).
- The globals-registered wrapper path — `ScoDictionaryNew.GetSettingsJson<T>(globals)` / `ScoDictionaryNew.Static.GetSettingsJson<T>(globals)` — explicitly adds `new ScoDictionaryConverter<T, TKey, TValue>()` to `settings.Converters` and sets `PreserveReferencesHandling = PreserveReferencesHandling.All`. That converter (`UtilitiesCS/NewtonsoftHelpers/ScoDictionaryConverter.cs`) wraps/unwraps via `WrapperScoDictionary<TDerived,TKey,TValue>` (`{"CoDictionary": {...}, "RemainingObject": {...}}`, `UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs`). **None of the RETARGET test methods use `GetSettingsJson(globals)` or register `ScoDictionaryConverter`** — they all use bare/default settings — so none of them exercise the wrapper shape.
- `UtilitiesCS.Test/ReusableTypeClasses/SerializableNew/ScoDictionaryNew_OnDiskCompatibility_Tests.cs` (already in the tree, out of scope for this change — it already tests only `ScoDictionaryNew`) directly proves the flat-shape/no-wrapper-token claim for all four production-persisted dictionaries (DictRemap, FilteredFolderScraping, FolderRemap, SubjectMap Encoder) using the identical default settings path (`NewSmartSerializableConfig.GetDefaultSettings()`), asserting the serialized output never contains `$type`, `$id`, `CoDictionary`, or `RemainingObject`. This existing file is sufficient coverage for the "persisted dictionary" on-disk-compatibility concern; the RETARGET files above are generic-infrastructure tests (not persisted-dictionary tests) and need no additional on-disk-compatibility assertion of their own.

Verdict: retargeting the four files above from `ScoDictionary<string,int>`/`typeof(ScoDictionary<...>)` to `ScoDictionaryNew<string,int>`/`typeof(ScoDictionaryNew<...>)` is a pure type swap with no JSON-shape regression risk, given none of them touch the globals-converter path.

## `IScoDictionary<TKey,TValue>` interface and converter/wrapper orphan check

- `IScoDictionary<TKey,TValue>` (`UtilitiesCS/Interfaces/IReusableTypeClasses/IScoDictionary.cs`, filename `ISCODictionary.cs` on disk) is a standalone interface declaration with **no reference to the `ScoDictionary` class** in its own source — it compiles independently of `SCODictionary.cs`.
- After deleting `SCODictionary.cs`, the only remaining production implementer of `IScoDictionary<TKey,TValue>` is gone, but the interface itself, and `IPeopleScoDictionary : IScoDictionary<string, string>` (`UtilitiesCS/Interfaces/IToDo/IPeopleScoDictionary.cs`), remain syntactically valid — neither derives from nor references `ScoDictionary`/`SCODictionary.cs`. **Safe to leave in place; not deleted by this change.** (Per the task's explicit instruction, interface/project teardown belongs to F5 (#308) — out of scope here.)
- `ToDoModel/Data Model/People/PeopleScoDictionary.cs` is fully block-commented (confirmed by direct read — every line is `//`-prefixed, including the `public class PeopleScoDictionary : ScoDictionary<string, string>, IPeopleScoDictionary` declaration at line 19). It is inert and requires no change.
- `IPeopleScoDictionary` has exactly one other reference in the tree, also commented out: `TaskMaster/AppGlobals/AppToDoObjects.cs:205` (`//public IPeopleScoDictionary DictPPL => ...`) and `UtilitiesCS/Interfaces/IGlobals/IToDoObjects.cs:13` (`//IPeopleScoDictionary DictPPL { get; }`). No live consumer.
- `ScoDictionaryConverter`, `ScoDictionaryConverter<TDerived,TKey,TValue>`, and `WrapperScoDictionary<TDerived,TKey,TValue>` are all constrained to and used exclusively against `ScoDictionaryNew<TKey,TValue>` (`where TDerived : ScoDictionaryNew<TKey, TValue>`), not the old class. They are **not orphaned** by this change — they remain live production code supporting `PeopleScoDictionaryNew`/`ScoDictionaryNew.GetSettingsJson(globals)`.

## Exact `<Compile Include>` lines to remove

**`UtilitiesCS/UtilitiesCS.csproj`** (production project) — one line:
```
    <Compile Include="ReusableTypeClasses\Serializable\Concurrent\SCO\SCODictionary.cs" />
```
(found at line 1048 of `UtilitiesCS.csproj`; confirmed to be the only `<Compile Include>` entry referencing `SCODictionary.cs`/the old `ScoDictionary` source file in this project.)

**`UtilitiesCS.Test/UtilitiesCS.Test.csproj`** (test project) — two lines, for the DELETE files:
```
    <Compile Include="ReusableTypeClasses\SCODictionary_Tests.cs" />
    <Compile Include="ReusableTypeClasses\SCODictionary_Additional_Tests.cs" />
```
(found at lines 380–381 of `UtilitiesCS.Test.csproj`.)

No other `<Compile Include>` entries reference these three files. `IScoDictionary.cs`/`IPeopleScoDictionary.cs` and the `ScoDictionaryNew`/`ScoDictionaryConverter`/`WrapperScoDictionary` family each have their own separate, unaffected `<Compile Include>` entries (not to be touched by this change).

## Residual `Swordfish.NET.Collections` binding check — verdict: **fully eliminated for ScoDictionary; one unrelated residual remains (out of scope)**

Full-repo grep for `using Swordfish.NET.Collections;` (`.cs` files only) returns exactly five hits:
1. `UtilitiesSwordfish.Test/ObservableSortedDictionaryTest.xaml.cs` — vendored Swordfish test harness, unrelated to `ScoDictionary`.
2. `UtilitiesSwordfish/Collections/IConcurrentObservableCollection.cs` — vendored Swordfish source, unrelated (collection, not dictionary; F2/F5 territory).
3. `UtilitiesCS/Interfaces/IReusableTypeClasses/IScoCollection.cs` — collection interface, unrelated to `ScoDictionary` (F5 territory per epic scoping).
4. **`UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/SCODictionary.cs`** — the file being deleted by this change. This is the only binding tied to `ScoDictionary` itself.
5. `UtilitiesCS.Test/ReusableTypeClasses/ObservableDictionary_Tests.cs` — tests Swordfish's `ObservableDictionary` type directly (not `ScoDictionary`/`ScoDictionaryNew`); not one of the five files in scope for this task and not touched by it.

After deleting `SCODictionary.cs` and its two csproj entries, **no remaining source file binds `Swordfish.NET.Collections` on behalf of `ScoDictionary`**. Hit #5 (`ObservableDictionary_Tests.cs`) is a pre-existing, independent Swordfish binding for an unrelated type and is out of scope for this refactor (not one of the five head-start files, does not reference `ScoDictionary`).

## Files in scope vs out of scope

**In scope (to be changed by this refactor):**
- `UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/SCODictionary.cs` — delete (production source).
- `UtilitiesCS/UtilitiesCS.csproj` — remove one `<Compile Include>` line (see above).
- `UtilitiesCS.Test/ReusableTypeClasses/SCODictionary_Tests.cs` — delete.
- `UtilitiesCS.Test/ReusableTypeClasses/SCODictionary_Additional_Tests.cs` — delete.
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` — remove two `<Compile Include>` lines (see above).
- `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableBase_Tests.cs` — retarget 3 usages (lines 52, 58, 73) to `ScoDictionaryNew`.
- `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableNonTyped_Tests.cs` — retarget 5 usages (lines 24, 50, 76, 82, 96) to `ScoDictionaryNew`; optionally update 2 comment lines (23, 49).
- `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableStatic_Tests.cs` — retarget 1 usage (line 29) to `ScoDictionaryNew`; optionally update 1 comment line (28).

**Out of scope (verified, no live reference to the old `ScoDictionary` class; no change needed):**
- `UtilitiesCS.Test/EmailIntelligence/FolderRemapController_Tests.cs` — comment-only.
- `UtilitiesCS.Test/EmailIntelligence/SubjectMapEncoder_Tests.cs` — comment-only.
- `UtilitiesCS.Test/EmailIntelligence/IntelligenceConfig_Tests.cs` — all references are `ScoDictionaryNew`/`PeopleScoDictionaryNew`.
- `UtilitiesCS.Test/NewtonsoftHelpers/ScoDictionaryConverterTests.cs` — all references are `ScoDictionaryNew`-derived types.
- `UtilitiesCS.Test/NewtonsoftHelpers/WrapperScoDictionaryTest.cs` — all references are `ScoDictionaryNew`-derived types.
- `UtilitiesCS/Interfaces/IReusableTypeClasses/IScoDictionary.cs` (`ISCODictionary.cs`), `UtilitiesCS/Interfaces/IToDo/IPeopleScoDictionary.cs` — interfaces, no `ScoDictionary` dependency; owned by F5 (#308) for any future teardown.
- `ToDoModel/Data Model/People/PeopleScoDictionary.cs` — fully commented out, inert.
- `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs` (line 239–240) — comment-only historical note.
- `UtilitiesCS/NewtonsoftHelpers/ScoDictionaryConverter.cs`, `UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs` — live production code for `ScoDictionaryNew`, not orphaned.
- `UtilitiesCS.Test/ReusableTypeClasses/ObservableDictionary_Tests.cs` — tests a different Swordfish type (`ObservableDictionary`), no `ScoDictionary` reference.

## Recommended execution order for the atomic plan

1. Retarget the three SmartSerializable test files first (compile-safe intermediate state; both `ScoDictionary` and `ScoDictionaryNew` still exist).
2. Delete `SCODictionary_Tests.cs` and `SCODictionary_Additional_Tests.cs`, and remove their two `<Compile Include>` lines from `UtilitiesCS.Test.csproj`.
3. Delete `SCODictionary.cs` and remove its one `<Compile Include>` line from `UtilitiesCS.csproj`.
4. Run the full C# toolchain (csharpier → analyzer build → nullable/warnings-as-errors build → vstest with coverage) per `CLAUDE.md`/`csharp-code-change-policy` before considering the change complete.
