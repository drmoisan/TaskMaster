# Finding B Root-Cause Confirmation Against Source (Cycle 5, Issue #181)

Timestamp: 2026-06-08T21-53

Confirmed citations establishing the JObject-defeats-reflection mechanism under `TypeNameHandling.None`:

1. `UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs:25`
   - `[JsonProperty("RemainingObject")] public object RemainingObject { get; set; }`
   - `RemainingObject` is declared `object`. With `TypeNameHandling.None` and no `$type` discriminator in the JSON, Newtonsoft has no concrete type to bind and materializes an untyped `Newtonsoft.Json.Linq.JObject`.

2. `UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs:39-78` (`ToDerived()`):
   - Line 54: `var remainingObjectType = RemainingObject.GetType();` — when `RemainingObject` is a `JObject`, this is `JObject`'s type.
   - Lines 55-63: `remainingObjectType.GetField("<Config>k__BackingField", ...) ?? remainingObjectType.GetField("_Config", ...)` — `JObject` has neither field; returns null.
   - Line 64: `var configValue = configField?.GetValue(RemainingObject) as NewSmartSerializableConfig;` — null because `configField` is null.
   - Lines 65-73: fallback `remainingObjectType.GetProperty("Config", ...)` — `JObject` exposes no CLR `Config` property; `configProperty` is null, so `configValue` stays null.
   - Lines 75-78: `if (configValue is not null) { derivedInstance.Config = configValue; }` — guard is false, so `derivedInstance.Config` is left at its constructed default.

3. Derived default Config: `UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/Observable/ScoDictionaryNew.cs:99-104`
   - `[JsonProperty] public NewSmartSerializableConfig Config { get => ism.Config; set => ism.Config = value; }`
   - The default `NewSmartSerializableConfig` (`NewSmartSerializableConfig.cs:22-27`) has `protected FilePathHelper _disk = new FilePathHelper();` so `Disk.FileName == ""`. This is exactly the observed `people.Config.Disk.FileName == ""` failure (P0-T8).

4. `UtilitiesCS/NewtonsoftHelpers/ScoDictionaryConverter.cs`:
   - Generic converter `ScoDictionaryConverter<TDerived,TKey,TValue>.ReadJson` lines 16-28: line 24-26 deserializes `WrapperScoDictionary<TDerived, TKey, TValue>`; line 27 returns `wrapper?.ToDerived()`. This is the path the failing test uses (`new ScoDictionaryConverter<PeopleScoDictionaryNew, string, string>()`, `PeopleScoDictionaryNewTests.cs:241`).
   - Non-generic converter `ScoDictionaryConverter.ReadJson` lines 50-66: line 64 `serializer.Deserialize(reader, wrapperType)`; line 65 invokes `ToDerived` via reflection. Same `ToDerived()` mechanism.

Conclusion: Confirmed. Under `TypeNameHandling.None`, `RemainingObject` binds to a `JObject`; the reflective `Config` field/property lookups in `ToDerived()` all return null; `Config` is left at its empty default. The defect is in the serialization layer (`WrapperScoDictionary.ToDerived()`), reachable via `ScoDictionaryConverter`. Source matches the documented mechanism; proceeding with the minimal fix in `WrapperScoDictionary.cs` (P2-T3). No HALT required.
