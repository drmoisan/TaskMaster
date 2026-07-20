# utilitiescs-nullable-newtonsofthelpers — Research Findings

- **Feature:** `utilitiescs-nullable-newtonsofthelpers` (epic child, issue #367 / manifest placeholder 9004)
- **Epic:** `utilitiescs-nullable-remediation`, Wave 0, complexity C3
- **Cluster:** `UtilitiesCS/NewtonsoftHelpers/`
- **Timestamp:** 2026-07-18T22-05
- **Mode:** research only (no source modified)
- **Scope constraint:** per-file `#nullable enable` opt-in; annotation and null-safety ONLY; no behavior change; no project/solution `<Nullable>`; pragma-only verification build (no `/p:Nullable=enable`). These are maintainer-mandated hard constraints carried from the epic manifest and the sibling spec (`utilitiescs-nullable-helperclasses/spec.md`); no alternative architecture is proposed.

## Method / commands run (read-only)

- `Timestamp: 2026-07-18T22-05` `Command: Glob UtilitiesCS/NewtonsoftHelpers/**/*.cs` `EXIT_CODE: 0` — enumerated 19 in-scope files.
- `Timestamp: 2026-07-18T22-05` `Command: Grep Newtonsoft.Json in UtilitiesCS.csproj + packages.config` `EXIT_CODE: 0` — confirmed reference version.
- `Timestamp: 2026-07-18T22-05` `Command: Grep for converter/binder/tracewriter type names across *.cs` `EXIT_CODE: 0` — identified consumers.
- `Timestamp: 2026-07-18T22-05` `Command: Grep banned APIs (DateTime.Now|UtcNow|Random.Shared|Thread.Sleep|Task.Delay|new Random) in NewtonsoftHelpers` `EXIT_CODE: 1 (no matches)` — none present.
- `Timestamp: 2026-07-18T22-05` `Command: Glob UtilitiesCS.Test/NewtonsoftHelpers/**/*.cs` `EXIT_CODE: 0` — enumerated existing tests.
- No compiler baseline build was executed in this read-only pass. The exact CS86xx counts per file must be captured by the implementer with the pragma-only baseline command (see Testing Implications). All framework-nullability statements below are grounded in the referenced Newtonsoft.Json 13.0.4 public API.

---

## 1. Confirmed in-scope file list (from disk)

19 `.cs` files confirmed under `UtilitiesCS/NewtonsoftHelpers/` (two nested subfolders present: `MonoExtension/`, `SDIL Reader/`). Line counts observed during reads.

| # | File (relative to `UtilitiesCS/NewtonsoftHelpers/`) | Lines | Base type / role | Already `#nullable`? |
|---|---|---|---|---|
| 1 | `AllInclusiveBinder.cs` | 21 | plain class (stub) | No |
| 2 | `AppGlobalsConverter.cs` | 44 | `JsonConverter<IApplicationGlobals>` | No |
| 3 | `DerivedCompositionConverter_ConcurrentDictionary.cs` | 229 | plain generic class (NOT a JsonConverter) | No |
| 4 | `FilePathHelperConverter.cs` | 217 | `JsonConverter<FilePathHelper>` | No |
| 5 | `KnownTypesBinder.cs` | 25 | `ISerializationBinder` | No |
| 6 | `MonoExtension/MonoExtension.cs` | 146 | static extension (Mono.Reflection) | No |
| 7 | `NConsoleTraceWriter.cs` | 38 | `ITraceWriter` | No |
| 8 | `NLogTraceWriter.cs` | 56 | `ITraceWriter` (GLOBAL namespace — see §6) | No |
| 9 | `NonRecursiveConverter.cs` | 95 | abstract `JsonConverter` | **Partial** — `#nullable enable` at line 27 |
| 10 | `PeopleScoConverter.cs` | 78 | `JsonConverter<PeopleScoDictionaryNew>` | No |
| 11 | `PeopleScoRemainingObjectConverter.cs` | 30 | non-generic `JsonConverter` | No |
| 12 | `ScDictionaryConverter.cs` | 39 | `JsonConverter<TDerived>` | No |
| 13 | `ScoDictionaryConverter.cs` | 86 | `JsonConverter<TDerived>` + non-generic `JsonConverter` | No |
| 14 | `SDIL Reader/ILGlobals.cs` | 191 | static IL helper | No |
| 15 | `SDIL Reader/ILInstruction.cs` | 161 | plain class | No |
| 16 | `SDIL Reader/MethodBodyReader.cs` | 291 | plain class (IL parser) | No |
| 17 | `WrapperPeopleScoDictionaryNew.cs` | ~607 | plain class (reflection) | No |
| 18 | `WrapperScDictionary.cs` | ~520 | plain generic class (reflection) | No |
| 19 | `WrapperScoDictionary.cs` | ~645 | plain generic class (reflection) | No |

Notes:
- `NonRecursiveConverter.cs` already carries `#nullable enable` at line 27; its `ReadJson`/`WriteJson`/`OnReadJson`/`OnWriteJson` already use `object?`. Remediation for this file is essentially moving/adding the pragma to the top of the file so the whole file is opted in, then confirming zero CS86xx (the pre-pragma members `CanRead`/`CanWrite` are `bool`, no obligation).
- `AppGlobalsConverter.cs`, `FilePathHelperConverter.cs`, and `ScoDictionaryConverter.cs` (the non-generic inner class) declare namespace `UtilitiesCS` (root) rather than `UtilitiesCS.NewtonsoftHelpers`; `PeopleScoConverter.cs`, `PeopleScoRemainingObjectConverter.cs`, and `WrapperPeopleScoDictionaryNew.cs` declare namespace `ToDoModel.Data_Model.People`. Namespaces are unchanged by this work.

---

## 2. Framework-defined nullability of the overrides (Newtonsoft.Json 13.0.4)

`UtilitiesCS.csproj` (lines 264-265) references `..\packages\Newtonsoft.Json.13.0.4\lib\net45\Newtonsoft.Json.dll`; `packages.config` line 103 pins `Newtonsoft.Json` `13.0.4` for `net481`. Newtonsoft.Json 13.0.x is compiled with nullable reference types enabled and embeds `[Nullable]`/`[NullableContext]` metadata in every target-framework assembly (including `lib/net45`). Consequently, when an in-scope file opts into `#nullable enable`, the compiler enforces the framework-declared nullability on the overrides. **Annotations must MATCH the framework signatures below, not restate them differently.** The override-compatibility rules that apply: an override may narrow a nullable base return to non-null (safe, no warning), and may widen a non-null base parameter to nullable (safe, no warning); the reverse directions produce CS8764 (return) / CS8765 (parameter).

### `JsonConverter<T>` (generic) — v13 declared signatures
```
public abstract T? ReadJson(JsonReader reader, Type objectType, T? existingValue, bool hasExistingValue, JsonSerializer serializer);
public abstract void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer);
```
- Nullable positions: `existingValue` (`T?`), `value` (`T?`), and the `ReadJson` return (`T?`).
- Non-null positions: `reader`, `objectType`, `serializer`, `writer`.
- Applies to: `AppGlobalsConverter` (T=`IApplicationGlobals`), `FilePathHelperConverter` (T=`FilePathHelper`), `ScDictionaryConverter<TDerived,...>` (T=`TDerived`), `ScoDictionaryConverter<TDerived,...>` generic (T=`TDerived`), `PeopleScoConverter` (T=`PeopleScoDictionaryNew`).
- Consequence: every `existingValue` parameter must become `T? existingValue`, and every `WriteJson` `value` must become `T? value` (else CS8765). The `ReadJson` return may stay non-null where the body always returns non-null (`AppGlobalsConverter` returns `_globals`), or be `T?` where the body returns a nullable expression (`ScDictionaryConverter`/`ScoDictionaryConverter`/`PeopleScoConverter` all return `wrapper?.ToDerived()`).

### non-generic `JsonConverter` — v13 declared signatures
```
public abstract object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer);
public abstract void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer);
public abstract bool CanConvert(Type objectType);
```
- Nullable positions: `existingValue` (`object?`), `value` (`object?`), `ReadJson` return (`object?`).
- Applies to: `PeopleScoRemainingObjectConverter`, `ScoDictionaryConverter` (inner non-generic class), `NonRecursiveConverter` (already conformed).
- `CanConvert(Type objectType)`: `objectType` is non-null.

### `ISerializationBinder` — v13 declared signatures
```
Type BindToType(string? assemblyName, string typeName);
void BindToName(Type serializedType, out string? assemblyName, out string? typeName);
```
- `assemblyName` (in `BindToType`) is `string?` (nullable); `typeName` is non-null; the return `Type` is **non-null**.
- `BindToName` out params are both `out string?` (nullable); `serializedType` non-null.
- Applies to: `KnownTypesBinder`. See §5 for the `BindToType` return-null contract decision.

### `ITraceWriter` — v13 declared signatures
```
void Trace(TraceLevel level, string message, Exception? ex);
TraceLevel LevelFilter { get; }
```
- `ex` is `Exception?` (nullable); `message` non-null.
- Applies to: `NConsoleTraceWriter`, `NLogTraceWriter`. Both must annotate `Exception? ex` on `Trace`.
- `AllInclusiveBinder` implements no Newtonsoft interface (it is an unused stub; its commented body suggests it was intended for an `ISerializationBinder`), so it has no framework-fixed signature.

Legitimately-nullable parameters/returns to annotate (matching, not changing, the framework): `existingValue`, `value` (both converter families), the non-generic/`ScDictionary`/`Sco`/`People` `ReadJson` returns, `BindToType`'s `assemblyName` in-parameter, `BindToName`'s two `out` params, and `Trace`'s `ex`. `serializer`, `reader`, `writer`, `objectType`, `serializedType`, and `typeName`/`message` are **non-null** and must stay non-null.

---

## 3. Cross-module consumers and which annotations become contracts

Grep for the type names across `*.cs` returned these production consumers outside `UtilitiesCS/NewtonsoftHelpers/`:

- `UtilitiesCS/ReusableTypeClasses/SerializableNew/Concurrent/Observable/ScoDictionaryNew.cs` — the base dictionary that registers `ScoDictionaryConverter` (via `[JsonConverter]`/serializer settings). **This is the primary cross-module contract surface**: `ScoDictionaryConverter`'s `ReadJson`/`WriteJson` nullability and the `WrapperScoDictionary` public members are consumed through it.
- `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializableLoader.cs` and `Config/ConfigController.cs` — serialization plumbing that installs converters and trace writers.
- `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` — consumes the dictionary converters during store (de)serialization.
- `UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs`, `UtilitiesCS/EmailIntelligence/Bayesian/Performance/BayesianSerializationHelper.cs` — serialization consumers.
- `ToDoModel/Data Model/People/PeopleScoConverter.cs` — a **separate** `PeopleScoConverter` living in `ToDoModel` (distinct from the in-scope `UtilitiesCS/NewtonsoftHelpers/PeopleScoConverter.cs`, which itself declares namespace `ToDoModel.Data_Model.People`). Confirm which is registered before assuming the in-scope file is live; both share the namespace.
- `TaskMaster/AppGlobals/AppAutoFileObjects.cs`, `TaskMaster/AppGlobals/Deprecated/SmartSerializableConfigOld.cs` — app-level consumers of `AppGlobalsConverter`.

Which annotations become cross-module contracts:
- **Converters registered on a serialized type** (`ScoDictionaryConverter`, `ScDictionaryConverter`, `PeopleScoConverter`, `AppGlobalsConverter`, `FilePathHelperConverter`) expose a framework-fixed override surface; because the override signatures are pinned by Newtonsoft, the only degrees of freedom that ripple outward are (a) the `ReadJson` return nullability (already forced `T?`/`object?` by the nullable bodies) and (b) the public constructors (`FilePathHelperConverter(IFileSystemFolderPaths)`, `AppGlobalsConverter(IApplicationGlobals)`). Keep constructor parameter nullability non-null (they are required dependencies).
- **Wrapper public members** consumed by the converters and by reflection callers: `WrapperScoDictionary<>.CoDictionary`, `.RemainingObject`, `WrapperScDictionary<>.ConcurrentDictionary`, `.RemainingObject`, `WrapperPeopleScoDictionaryNew.CoDictionary`, `.RemainingObject`, and their `ToDerived()`/`ToComposition()` return types. `RemainingObject` is a `[JsonProperty]` public `object` that is only populated during (de)serialization; its annotation (`object?` vs `= null!`) is a contract decision (see §5).
- Cross-cluster dependency note: `FilePathHelperConverter` dereferences `FilePathHelper` (a `HelperClasses`/root type owned by sibling child #364) and `ScDictionaryConverter`/`ScoDictionaryConverter`/wrappers dereference `ScDictionary`/`ScoDictionaryNew`/`NewSmartSerializableConfig`/`ConcurrentObservableDictionary` (owned by the ReusableTypes child #9003) and `ThrowIfNull`/`IsNullOrEmpty` (Extensions child #363). Per the epic manifest, NewtonsoftHelpers has `depends_on: []`; those other clusters may or may not be opted in during this work. Because members of a not-yet-opted-in (oblivious) type impose **no** nullable obligation on a nullable-context caller, this cluster can be remediated independently regardless of the sibling clusters' state. `UtilitiesCS/Extensions/NullExtensions.cs` is **already** `#nullable enable` (line 12): `ThrowIfNull<T>(this T? argument) where T : notnull` returns non-null `T`, so the many `x.ThrowIfNull()` call sites in the wrappers give correct non-null flow for free.

---

## 4. Risk ordering — high-contract vs mechanical

**Highest contract-sensitivity / highest risk (do last):**
1. `FilePathHelperConverter.cs` — largest converter body, `ReadJson`/`WriteJson` override nullability plus many `TryGetValue`/`reader.Value as string` null flows and a return-null helper (`ExtractFolderPath`); cross-module (FilePathHelper serialization).
2. `ScoDictionaryConverter.cs` and `ScDictionaryConverter.cs` and `PeopleScoConverter.cs` — the `[JsonConverter]`-registered contracts consumed across ReusableTypes / OutlookObjects / EmailIntelligence.
3. The three wrappers (`WrapperScoDictionary.cs`, `WrapperScDictionary.cs`, `WrapperPeopleScoDictionaryNew.cs`) — heavy reflection, largest files, feed the converters; high mechanical volume plus the `RemainingObject` and `ModifyGet/SetMethod` return-nullability decisions.

**Medium (isolated but non-trivial):**
4. `DerivedCompositionConverter_ConcurrentDictionary.cs` — reflection, `Dictionary<string,object>` value-nullability, `Activator.CreateInstance` cast.
5. `SDIL Reader/MethodBodyReader.cs`, `SDIL Reader/ILInstruction.cs`, `SDIL Reader/ILGlobals.cs` — IL parsing, uninitialized non-null fields, reflection returning nullable. Isolated (consumed only inside the cluster).

**Low / mechanical:**
6. `AppGlobalsConverter.cs`, `PeopleScoRemainingObjectConverter.cs`, `NonRecursiveConverter.cs` (already conformed), `KnownTypesBinder.cs`, `NConsoleTraceWriter.cs`, `NLogTraceWriter.cs`, `MonoExtension/MonoExtension.cs`, `AllInclusiveBinder.cs`.

---

## 5. File-specific hazards (evidence-cited)

- **`AllInclusiveBinder.cs:18`** — `return null;` from `public Assembly[] GetAssemblies()`. Under nullable this is CS8603. Behavior-preserving contract decision: annotate return `Assembly[]?` (plain class, no interface constraint). Effectively a stub, low blast radius.
- **`AppGlobalsConverter.cs:23-42`** — `ReadJson` `existingValue` → `IApplicationGlobals?`; `WriteJson` `value` → `IApplicationGlobals?` (CS8765 otherwise). `ReadJson` return may stay non-null (`return _globals;`). `_globals` field is assigned in the ctor (non-null) — OK.
- **`DerivedCompositionConverter_ConcurrentDictionary.cs`** — `ConcurrentDictionary`/`RemainingObject`/`AdditionalFields`/`AdditionalProperties` are non-null auto-props; only some are set in a given ctor path → CS8618. `Activator.CreateInstance(typeof(TDerived), true)` returns `object?` cast to `(TDerived)` (line 67) → CS8600/CS8601 (`!` behavior-preserving). `field.GetValue(...)` / `property.GetValue(...)` return `object?` added into `Dictionary<string, object>` (lines 53, 59) → either widen the dictionary to `Dictionary<string, object?>` (annotation-only, honest) or `!`. `newClassType.GetField/GetProperty` return nullable — already `if (... != null)` guarded.
- **`FilePathHelperConverter.cs`** — `TryGetValue(..., out string x)` at lines 43, 47, 49, 89 → `out string?` under nullable. `reader.Value` is `object?`; `reader.Value as string` (lines 127, 161) is `string?`; line 127 `(reader.Value as string).ThrowIfNull()` returns non-null via the already-annotated extension — OK. `ExtractFolderPath(Dictionary<string,string>)` returns `null` (lines 44, 60) → return `string?`. `GetErrorMessage` (lines 142-145): `reader is JsonTextReader` guard then re-casts into a separate `textReader` local (`reader as JsonTextReader`, nullable) before `textReader.LineNumber` → CS8602; behavior-preserving null-flow correction is the `is JsonTextReader textReader` pattern (a pattern-match tightening, not a behavior change) or `!`. `WriteJson` `value` → `FilePathHelper? value`, and `value.FolderPath`/`value.FileName` then need `value!`/guard (Newtonsoft does not pass null here; behavior-preserving).
- **`KnownTypesBinder.cs:16`** — `BindToType` returns `KnownTypes.SingleOrDefault(...)` which is **null when no match**, but `ISerializationBinder.BindToType` return is non-null `Type`. Making the implementation return `Type?` would be CS8766 (does not match the implemented member). Behavior-preserving decision: keep `Type` and apply `!` with a `// why` comment (Newtonsoft tolerates a null return by falling back to default binding; the current runtime behavior returns null, so `!` preserves it). `BindToName` (line 19): `out string assemblyName` set to `null` (line 21) → annotate `out string? assemblyName` to match the interface. `KnownTypes` auto-prop is non-null but uninitialized → CS8618 (annotate `IList<Type>?` or `= null!`; it is a caller-populated public setter, so `IList<Type>?` is the honest annotation).
- **`MonoExtension/MonoExtension.cs`** — `instruction.Operand` (Mono.Reflection `Instruction.Operand`, `object`) is cast to value types (`(sbyte)`, `(byte)`, `(int)`, `(long)`, `(float)`, `(double)`) and reference types (`(SignatureHelper)`, `(string)`, `(Type)`, `(FieldInfo)`, `(LocalVariableInfo)`). Mono.Reflection is not nullable-annotated (oblivious), so expected CS86xx here is low; the `is`-pattern branches (lines 79-115) already narrow. Likely near-zero diagnostics — confirm against baseline.
- **`NConsoleTraceWriter.cs`** — `Trace` `ex` → `Exception?` (line 30). `Log` property `Action<string, Exception>` (line 28) is never initialized (CS8618) and `Log?.Invoke(message, ex)` passes a nullable `ex` → annotate `Action<string, Exception?>? Log`. Contract on a public property.
- **`NLogTraceWriter.cs`** — `Trace` `ex` → `Exception?` (line 31). `GetLogFunction` returns `null` for `TraceLevel.Off` (line 51) → return `Action<string, Exception>?`; the caller already uses `logFunction?.Invoke(...)`. `MethodBase.GetCurrentMethod()` returns `MethodBase?` and `.DeclaringType` is `Type?` (lines 11-13) passed to `LogManager.GetLogger(Type)` → CS8604; behavior-preserving `!`. `Logger.Error`/`Warn`/`Info`/`Debug` are log4net `ILog` method groups (log4net is not nullable-annotated → oblivious).
- **`NonRecursiveConverter.cs`** — already `#nullable enable` (line 27) with `object?` overrides matching the non-generic base. Action: move the pragma to the top of the file (before line 22 so `CanRead`/`CanWrite` and the class are covered) and confirm zero CS86xx. `[ThreadStatic] private static bool` fields are value types — no obligation.
- **`PeopleScoConverter.cs:15-27`** — generic `JsonConverter<PeopleScoDictionaryNew>`: `existingValue` → `PeopleScoDictionaryNew?`; `ReadJson` return `PeopleScoDictionaryNew?` (body `wrapper?.ToDerived()` is nullable — `serializer.Deserialize(...) as WrapperPeopleScoDictionaryNew` is `WrapperPeopleScoDictionaryNew?`). `WriteJson` `value` → `PeopleScoDictionaryNew?`.
- **`PeopleScoRemainingObjectConverter.cs`** — non-generic: `existingValue`/`value` → `object?`; `ReadJson` returns `jObject.ToObject<PeopleScoRemainingObject>(serializer)` which is `PeopleScoRemainingObject?` → return `object?` (matches base). `JObject.Load(reader)` is non-null.
- **`ScDictionaryConverter.cs` / `ScoDictionaryConverter.cs` (generic)** — `existingValue` → `TDerived?`; return `TDerived?` (`wrapper?.ToDerived()`). Constraint `where TDerived : ScDictionary<...>` / `: ScoDictionaryNew<...>` makes `TDerived` a reference type so `TDerived?` is a valid annotation. `WriteJson` `value` → `TDerived?`.
- **`ScoDictionaryConverter.cs` (inner non-generic, lines 39-85)** — `existingValue`/`value` → `object?`. `objectType.GetScoDictionaryNewGenerics()` and `MakeGenericType`/`GetMethod("ToDerived", [])?.Invoke(...)` return `object?` — matches `object?` return. `WriteJson` line 70 `value.GetType()` on `object? value` → CS8602; behavior-preserving `value!`/guard (Newtonsoft never serializes a null value here). `Activator.CreateInstance(wrapperType)` (line 78) returns `object?` fed to `toComposition?.Invoke(wrapper, [value])`.
- **`SDIL Reader/ILGlobals.cs`** — `multiByteOpCodes`/`singleByteOpCodes` are non-null static fields assigned only in `LoadOpCodes()` (lines 113-119) → CS8618; `modules = null` (line 115) → `Module[]? modules`. `info1.GetValue(null)` returns `object?` cast to `(OpCode)` (line 127) → CS8605 (unbox-possibly-null); guarded by `FieldType == typeof(OpCode)` so `!` is behavior-preserving. `ProcessSpecialTypes(string)` is non-null in/out.
- **`SDIL Reader/ILInstruction.cs`** — `operand` (`object`) and `operandData` (`byte[]`) are non-null private fields never initialized (settable via properties) → CS8618; annotate `object? operand` / `byte[]? operandData` (the public `Operand`/`OperandData` become `object?`/`byte[]?`). `GetCode()` already guards `if (operand != null)`; `fOperand.ReflectedType`/`mOperand.ReflectedType` are `Type?` (lines 61, 77, 94) → CS8602 inside the try blocks; `!` (or the surrounding `catch {}` already absorbs) is behavior-preserving. `operand.ToString()` (lines 108-117, 124) after the `!= null` guard is OK.
- **`SDIL Reader/MethodBodyReader.cs`** — `instructions = null`, `il = null`, `mi = null` (lines 15-17) are non-null fields explicitly null-initialized → annotate `List<ILInstruction>? instructions`, `byte[]? il`, `MethodInfo? mi`. The `Read*` helpers dereference the `il` field heavily (`il[position++]`), only reachable after the ctor sets `il`; annotating `il` as `byte[]?` forces CS8602 on those accesses — behavior-preserving choices are `= null!` on `il` with a `// why` comment (invariant: `il` non-null once `ConstructInstructions` runs) or localized `!`. `this.mi.DeclaringType.GetGenericArguments()` (line 159) → `DeclaringType` is `Type?` → CS8602 → `!` (a decompiled method always has a declaring type here). `module.ResolveMethod`/`ResolveType`/`ResolveField` overloads return nullable — assigned to `object? Operand`, so OK. `mi.GetMethodBody()` returns `MethodBody?` (line 284) — already `!= null` guarded; `.GetILAsByteArray()` returns `byte[]?` → the result feeds `il` (now nullable) — OK.
- **Wrappers (`WrapperScDictionary.cs`, `WrapperScoDictionary.cs`, `WrapperPeopleScoDictionaryNew.cs`)** — recurring mechanical patterns needing `!` or `?`:
  - `(TDerived)Activator.CreateInstance(typeof(TDerived), true)` (e.g. WrapperScDictionary:41, WrapperSco:46) → `object?`→`TDerived` cast, CS8600.
  - `property.DeclaringType.GetGenericArguments()` (WrapperScDictionary:237, WrapperSco:353) → `DeclaringType` is `Type?`, CS8602, `!`.
  - `property.GetGetMethod().Attributes` / `GetSetMethod().Attributes` (WrapperScDictionary:258-259, WrapperSco:381-382) → `MethodInfo?`, CS8602, `!`.
  - `getMethod.GetMethodBody().GetILAsByteArray()` (WrapperScDictionary:501, WrapperSco:626, WrapperPeople:577) → `MethodBody?`, CS8602, plus `GetILAsByteArray()` is `byte[]?`.
  - `getMethod.Module.ResolveField(metadataToken)` returned from a non-null-declared `GetBackingField` (all three) → `ResolveField` is `FieldInfo?`, CS8603, `!`.
  - `field.GetValue(...)`/`property.GetValue(...)` return `object?`; assigned via `SetValue` (object? param, OK) — but where fed into non-null locals, watch flow.
  - `RemainingObject` `[JsonProperty]` public `object` is **not** initialized in the ctor (WrapperScDictionary:21, WrapperSco:26, WrapperPeople:33) → CS8618. Contract decision: `object?` (honest — it is only populated during (de)serialize) vs `= null!` (behavior-preserving, keeps the non-null JSON contract). Recommend `object?` unless a downstream consumer treats it as non-null; verify against `ScoDictionaryNew.cs` usage.
  - `ModifyGetMethod`/`ModifySetMethod` return-nullability is **inconsistent** across the three files: `WrapperScDictionary.ModifySetMethod` throws on a null setter (non-null return), while `WrapperScoDictionary`/`WrapperPeopleScoDictionaryNew.ModifySetMethod` `return null;` (nullable return). Annotate each to its actual behavior (`MethodBuilder?` where it can return null; the callers in `ReplicateProperty` already null-check with `if (... is not null)` in the Sco/People variants but **not** in `WrapperScDictionary.ReplicateProperty:239-243`). Preserve current behavior exactly per file; do not unify.
  - `WrapperScoDictionary.cs:83-91` — `RemainingObject is JObject remainingObjectJson`, then `remainingObjectJson["Config"]` returns `JToken?` (line 85), guarded `if (configToken is not null && configToken.Type != JTokenType.Null)`, then `configToken.ToObject<NewSmartSerializableConfig>()` returns `NewSmartSerializableConfig?` (line 88) fed to `NormalizeEmptyDiskFilePaths(config)` which already null-guards (line 171). This JObject/JToken path is the notable nullable-member-access hazard; the existing guards make it largely mechanical.
  - `WrapperPeopleScoDictionaryNew.cs:24-26` — `MethodBase.GetCurrentMethod().DeclaringType` (`Type?`) into `LogManager.GetLogger(Type)` → CS8604, `!`. `configField?.GetValue(...) as NewSmartSerializableConfig` (line 67) is `NewSmartSerializableConfig?`, already `is not null` guarded.

---

## 6. Pre-existing policy conditions

- **500-line limit (General Code Change Policy §4 / CLAUDE.md C#5) — PRE-EXISTING violations, flag only:** `WrapperScoDictionary.cs` (~645), `WrapperPeopleScoDictionaryNew.cs` (~607), `WrapperScDictionary.cs` (~520) already exceed 500 lines before any pragma is added. Annotation-only work adds a `#nullable enable` line plus per-line annotations and cannot bring these under 500 without a refactor, which is outside annotation-only scope. **Do not split; flag as a known pre-existing exception** (same handling as `PrettyPrint.cs` in the sibling #364 spec). Every other in-scope file is well under 500.
- **Banned APIs:** grep for `DateTime.Now`/`DateTime.UtcNow`/`Random.Shared`/`Thread.Sleep`/`Task.Delay`/`new Random` across `NewtonsoftHelpers/` returned no matches (EXIT_CODE 1). `MethodBodyReader.cs` has `using System.Threading;` but no `Thread.Sleep`. No banned-API remediation is in scope for this cluster.
- **`NLogTraceWriter.cs` declares its class in the GLOBAL namespace** (no `namespace` block; lines 9-56). This is a pre-existing structural oddity, not a nullable issue; leave the namespace unchanged (moving it would be a behavior/reference change out of scope). Note it so the executor does not "fix" it.
- **Duplicate `PeopleScoConverter`:** an in-scope `UtilitiesCS/NewtonsoftHelpers/PeopleScoConverter.cs` and an out-of-scope `ToDoModel/Data Model/People/PeopleScoConverter.cs` both exist under namespace `ToDoModel.Data_Model.People`. Confirm which is registered/live before finalizing the in-scope file's `ReadJson` return contract; annotate only the in-scope file.

---

## 7. Existing test coverage

A comprehensive test area exists at `UtilitiesCS.Test/NewtonsoftHelpers/` (MSTest). Files present:

- `AllInclusiveBinder_Tests.cs`, `KnownTypesBinder_Tests.cs`
- `AppGlobalsConverterTests.cs` and `AppGlobalsConverterTests_Unfinished.cs` (under `UtilitiesCS.Test/Threading/`)
- `FilePathHelperConverterTests.cs`
- `DerivedCompositionConverter_ConcurrentDictionaryTests.cs`
- `NConsoleTraceWriter_Tests.cs`; `NLogTraceWriter_Test.cs` (under `UtilitiesCS.Test/HelperClasses/`)
- `NonRecursiveConverter_Tests.cs`
- `PeopleScoConverter_Tests.cs`, `PeopleScoRemainingObjectConverter_Tests.cs`, `WrapperPeopleScoDictionaryNew_Tests.cs`; plus `ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs`
- `ScDictionaryConverter_Tests.cs`, `ScoDictionaryConverterTests.cs`
- `WrapperScDictionaryTest.cs`, `WrapperScoDictionaryTest.cs` (helper `MyTypeBuilder.cs`)
- `MonoExtension_Tests.cs`, `MonoExtension/MonoExtensionCoverageTests.cs`
- `SDILReader/ILGlobals_Tests.cs`, `SDILReader/ILInstruction_Tests.cs`, `SDILReader/MethodBodyReader_Tests.cs`

Every in-scope production file has at least one corresponding test file. These are the behavior-identical regression oracle: re-run each batch's tests after annotation and require green with no behavior change, and confirm no coverage regression on changed lines (CLAUDE.md UT2; changed lines are annotation-only, so behavior coverage should be unchanged). Because the edits are annotations, guards, and `!` operators only, the primary risk to catch is an accidental behavior change from a mis-placed guard or an over-eager null-flow "correction" — the existing tests cover the (de)serialization round trips that would surface such a regression.

---

## 8. Recommended batch sequence (for the atomic plan)

Subdirectory-cohesive, foundational/low-risk first, cross-module/high-contract last. Each batch opts in its files with `#nullable enable`, drives them to zero CS86xx under the pragma-only build, and re-runs that batch's tests green.

- **Batch 1 — Leaf / isolated helpers (no framework override, no cross-module contract):** `AllInclusiveBinder.cs`, `MonoExtension/MonoExtension.cs`.
- **Batch 2 — SDIL Reader subfolder (cohesive, isolated IL parsing):** `SDIL Reader/ILGlobals.cs`, `SDIL Reader/ILInstruction.cs`, `SDIL Reader/MethodBodyReader.cs`.
- **Batch 3 — Trace writers (`ITraceWriter`):** `NConsoleTraceWriter.cs`, `NLogTraceWriter.cs`.
- **Batch 4 — Binder + simple converters (framework overrides, small bodies):** `KnownTypesBinder.cs`, `AppGlobalsConverter.cs`, `PeopleScoRemainingObjectConverter.cs`, `NonRecursiveConverter.cs` (move pragma to top, verify).
- **Batch 5 — Reflection composition helper:** `DerivedCompositionConverter_ConcurrentDictionary.cs`.
- **Batch 6 — Wrappers (foundational to the dictionary converters; heavy reflection, >500-line flags):** `WrapperScDictionary.cs`, `WrapperScoDictionary.cs`, `WrapperPeopleScoDictionaryNew.cs`.
- **Batch 7 — Dictionary converters (consume Batch-6 wrappers; the `[JsonConverter]`-registered cross-module contracts):** `ScDictionaryConverter.cs`, `ScoDictionaryConverter.cs`, `PeopleScoConverter.cs`.
- **Batch 8 — High-contract finish:** `FilePathHelperConverter.cs`.

Rationale for the wrapper-before-converter ordering: `ScDictionaryConverter`/`ScoDictionaryConverter`/`PeopleScoConverter` all consume `wrapper.ToDerived()`/`ToComposition()` return types; settling the wrappers' nullability first prevents re-touching the converters when a wrapper contract changes.

### Files requiring deliberate CONTRACT decisions (not purely mechanical)
- `AllInclusiveBinder.cs` — `GetAssemblies()` return `Assembly[]?`.
- `KnownTypesBinder.cs` — `BindToType` return-null vs framework non-null `Type` (`!` behavior-preserving); `BindToName` `out string?`; `KnownTypes` property nullability.
- `NConsoleTraceWriter.cs` — public `Log` property `Action<string, Exception?>?`.
- `AppGlobalsConverter.cs` — override param nullability (return may stay non-null).
- `FilePathHelperConverter.cs` — `WriteJson` `value` nullability + deref decision; `ExtractFolderPath` return `string?`; cross-module.
- `ScDictionaryConverter.cs`, `ScoDictionaryConverter.cs`, `PeopleScoConverter.cs` — `ReadJson` return `TDerived?`/`object?` as registered cross-module contracts.
- The three wrappers — `RemainingObject` property (`object?` vs `= null!`) and the per-file `ModifyGet/SetMethod` return-nullability (`MethodBuilder?`) that must each match existing per-file behavior without unifying.

### Files that are largely MECHANICAL annotation
`MonoExtension/MonoExtension.cs`, `SDIL Reader/ILGlobals.cs`, `SDIL Reader/ILInstruction.cs`, `SDIL Reader/MethodBodyReader.cs`, `NLogTraceWriter.cs` (`Exception? ex` + `GetLogFunction` return + logger `!`), `PeopleScoRemainingObjectConverter.cs`, `NonRecursiveConverter.cs` (already conformed), `DerivedCompositionConverter_ConcurrentDictionary.cs`.

---

## Testing Implications (no test code written here)

- **Baseline first, per batch:** run the pragma-only build to capture the exact CS86xx set for the batch's files, then drive to zero. Verification command (per the epic/#364 deviation — do NOT add `/p:Nullable=enable`):
  `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
- **Regression oracle:** re-run the matching `UtilitiesCS.Test/NewtonsoftHelpers/` (and `Threading/AppGlobalsConverterTests.cs`, `HelperClasses/NLogTraceWriter_Test.cs`, `ToDoModel.Test` People tests) suites after each batch; require green and behavior-identical.
- **Coverage:** changed lines are annotation/guard-only; confirm no coverage regression on changed lines (CLAUDE.md UT2). MSTest + Moq + FluentAssertions per policy.
- **Full toolchain final pass:** csharpier → analyzer/codestyle build → pragma-only `TreatWarningsAsErrors` build → vstest with coverage.
- **Do NOT introduce temp files** in any added/modified test (none are required for annotation-only work).

## Evidence-location note

Per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, any baseline/QA/coverage artifacts produced during execution must be written under `docs/features/active/utilitiescs-nullable-newtonsofthelpers/evidence/<kind>/`, not under `artifacts/`.
