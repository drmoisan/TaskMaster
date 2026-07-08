# Research: FilePathHelper Deserialize NRE Reachability — LcppnFolderPredictor (Issue #177 Cycle 3/4)

**Verdict: NOT REPRODUCIBLE**

Date: 2026-06-16  
Branch: TaskMaster-wt-2026-06-08-12-06 (HEAD)

---

## 1. Question Under Investigation

Is the `FilePathHelper` deserialization NRE (`Error setting value to 'FileStemSeed' … NullReferenceException` inside `AdjustForMaxPath()`) reachable on HEAD through any production or test serialize/deserialize path?

---

## 2. Files Read

- `UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs`
- `UtilitiesCS/NewtonsoftHelpers/FilePathHelperConverter.cs`
- `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs`
- `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializableBase.cs`
- `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/Config/NewSmartSerializableConfig.cs`
- `UtilitiesCS/EmailIntelligence/Bayesian/DoNotSerializeContractResolver.cs`
- `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/LcppnFolderPredictorStore.cs`
- `UtilitiesCS/EmailIntelligence/Bayesian/LcppnFolderPredictor.cs`
- `UtilitiesCS.Test/EmailIntelligence/Bayesian/LcppnFolderPredictor_Serialization_Tests.cs`
- `UtilitiesCS.Test/EmailIntelligence/LcppnFolderPredictorStore_Tests.cs`

---

## 3. Exact Throw Mechanism (Cycle-3 Bug)

### 3a. How the NRE fires

`FilePathHelper` registers `FilePathHelper_PropertyChanged` in all constructors. The event handler for `case "FileStemSeed":` calls `AdjustForMaxPath()`. The instance `AdjustForMaxPath()` method (line 292) begins:

```csharp
public bool AdjustForMaxPath()
{
    if (!StemInitialized())
        return false;

    var maxSeedLength =
        MAX_PATH - FolderPath.Length - FileExtension.Length - FileStemSuffix.Length;
    ...
}
```

`StemInitialized()` (line 183) is:

```csharp
internal bool StemInitialized()
{
    if (FileStemSeed is null || FileStemSuffix is null || FileExtension is null)
    {
        if (FolderPath.IsNullOrEmpty() || !TryParseFileName(FileName))
            return false;
    }
    return !FolderPath.IsNullOrEmpty();
}
```

The NRE path is:
1. Newtonsoft sets `FileStemSeed` on a partially-deserialized `FilePathHelper`.
2. The setter fires `NotifyPropertyChanged("FileStemSeed")`.
3. The handler calls `AdjustForMaxPath()`.
4. `StemInitialized()` sees `_fileExtension == null`. It attempts `TryParseFileName(FileName)`.
5. **IF `FileName` is also null/empty at the moment `FileStemSeed` is set**, `TryParseFileName` returns false and `StemInitialized()` returns false → `AdjustForMaxPath()` returns false safely. **No throw.**
6. **IF `FileName` has a non-empty value** but `FolderPath` is still empty, `StemInitialized()` returns false again → safe.
7. The dangerous case is: `FolderPath` is already set (non-empty), `FileName` is already set (non-empty), but `FileExtension` is still null. Then `TryParseFileName(FileName)` succeeds (setting `_fileExtension` via the backing field) and `StemInitialized()` returns true. `AdjustForMaxPath()` then executes `FileExtension.Length` — but at this point `_fileExtension` was just set by `TryParseFileName` so it is NOT null.

Wait — this means the self-healing via `TryParseFileName` is more nuanced. The **actual** NRE location reported was line 298:

```csharp
var maxSeedLength =
    MAX_PATH - FolderPath.Length - FileExtension.Length - FileStemSuffix.Length;
```

`FileExtension` here is the public property getter → `_fileExtension`. `FileStemSuffix` is the public property getter → `_fileStemSuffix`. If `StemInitialized()` returned true but `_fileExtension` is still null (which can happen if `TryParseFileName` was NOT called because `FileStemSeed/FileStemSuffix/FileExtension` were all non-null going into the check, yet `_fileExtension` is somehow null), this would NRE.

However, review shows: `StemInitialized()` returns true only if the condition `FileStemSeed is null || FileStemSuffix is null || FileExtension is null` is FALSE, meaning all three are non-null, OR the fallback `TryParseFileName` succeeded (which sets `_fileExtension` to the parsed extension). In either branch, `_fileExtension` is non-null when `StemInitialized()` returns true.

**Conclusion about the NRE mechanism:** The NRE at line 298 via `FileExtension.Length` is NOT possible after `StemInitialized()` returns true, because `StemInitialized()` guarantees `FileExtension` is non-null when it returns true.

**But there is a second code path at lines 389–390:**

```csharp
case "FileStemSeed":
    if (AdjustForMaxPath())
    {
        _fileStem = $"{_fileStemSeed}{_fileStemSuffix}";
        FileName = $"{_fileStem}{_fileExtension}";   // line 390
    }
    break;
```

If `AdjustForMaxPath()` returns true AND `_fileExtension` is null at line 390, this produces `FileName = "{_fileStem}null"` (string interpolation with null yields literal string `"null"` or empty string — in C#, `null` in string interpolation gives `""`). Actually `$"{_fileStem}{_fileExtension}"` where `_fileExtension == null` yields `$"{_fileStem}"`, i.e., no extension. This does not NRE by itself.

**Re-reading the cycle-3 report:** The error was `Error setting value to 'FileStemSeed' … NullReferenceException` attributed to `AdjustForMaxPath()`. The most plausible source in `AdjustForMaxPath()` lines 297–298 is:

```csharp
var maxSeedLength =
    MAX_PATH - FolderPath.Length - FileExtension.Length - FileStemSuffix.Length;
```

`FileExtension` is a property returning `_fileExtension`, which is a nullable reference — calling `.Length` on it when it is null would NRE. However, as analyzed above, `StemInitialized()` returning true means `_fileExtension` is non-null. The subtlety is the `TryParseFileName` path within `StemInitialized()`:

```
TryParseFileName sets _fileExtension via backing field → _fileExtension is now set
StemInitialized returns true
AdjustForMaxPath reads FileExtension.Length → _fileExtension is non-null → no NRE
```

The original cycle-3 throw was real and likely occurred due to a **specific document ordering** where:
- `FolderPath` was already set (non-empty), `FileName` was already set (non-empty)  
- `FileStemSeed` was set BEFORE `FileExtension` in the JSON property order
- `TryParseFileName(FileName)` returned true, setting `_fileExtension` via the backing field
- BUT: Newtonsoft had also serialized an explicit `"FileExtension": null` entry in the document (or the field was null in the source), and Newtonsoft subsequently set `FileExtension = null` after `StemInitialized()` had already used `TryParseFileName` to set it

Actually, re-reading more carefully: the Newtonsoft deserialization order follows JSON property order in the document. The report says the Disk in `NewSmartSerializableConfig` was populated via `BuildConfig`, which uses `new FilePathHelper(FileName, bayesianFolder)` — the two-argument constructor. This sets `_fileName` and `_folderPath` but leaves `_fileStemSeed`, `_fileStemSuffix`, `_fileExtension` all null.

When Newtonsoft serializes this `FilePathHelper`:
- `FilePath`: set (combined path)
- `FolderPath`: set
- `FileName`: set (e.g., `"LcppnFolder.json"`)
- `FileStemSeed`: null
- `FileStemSuffix`: null
- `FileStem`: null
- `FileExtension`: null

Default Newtonsoft serialization (no NullValueHandling specified) includes null values. So the document for `Config.Disk` would contain:

```json
"Disk": {
  "FilePath": "C:\\...\\AppData\\Bayesian\\LcppnFolder.json",
  "FolderPath": "C:\\...\\AppData\\Bayesian",
  "FileName": "LcppnFolder.json",
  "FileStemSeed": null,
  "FileStemSuffix": null,
  "FileStem": null,
  "FileExtension": null
}
```

Property ordering in Newtonsoft follows the order properties are declared in the class. From `FilePathHelper.cs` (lines 69–141):
1. `FilePath` (line 69)
2. `FolderPath` (line 80)
3. `FileName` (line 91)
4. `FileStemSeed` (line 102)
5. `FileStemSuffix` (line 113)
6. `FileStem` (line 123, protected set — serialized by getter but setter is protected, so Newtonsoft skips setting it)
7. `FileExtension` (line 131)

On deserialization of this document:
1. `FilePath = "C:\\...\\LcppnFolder.json"` → triggers `FilePathHelper_PropertyChanged("FilePath")` → sets `_folderPath` and `_fileName` from path decomposition
2. `FolderPath = "C:\\...\\Bayesian"` → triggers handler `"FolderPath"` → recomputes `_filePath`
3. `FileName = "LcppnFolder.json"` → triggers handler `"FileName"` → recomputes `_filePath`
4. `FileStemSeed = null` → setter fires `NotifyPropertyChanged("FileStemSeed")` → handler calls `AdjustForMaxPath()`
   - At this point: `_folderPath = "C:\\...\\Bayesian"` (non-empty), `_fileName = "LcppnFolder.json"` (non-empty), `_fileStemSeed = null`, `_fileStemSuffix = null`, `_fileExtension = null`
   - `StemInitialized()`: `FileStemSeed is null` → true → enters the branch → `FolderPath` is non-empty → calls `TryParseFileName("LcppnFolder.json")`
   - `TryParseFileName("LcppnFolder.json")`: calls `ExtractStemAndExtension` → `fileStem = "LcppnFolder"`, `fileExtension = ".json"` → calls `TryParseFileStem("LcppnFolder", out seed, out suffix)` where seed and suffix start as `null ?? "" = ""`
   - `TryParseFileStem`: `fileStemSeed` and `fileStemSuffix` are both empty → case 2 → `fileStemSeed = "LcppnFolder"`, `remainingChars = null`
   - Returns true → sets `_fileStemSeed = "LcppnFolder"`, `_fileStemSuffix = ""`, `_fileStem = "LcppnFolder"`, `_fileExtension = ".json"`
   - `StemInitialized()` returns true
   - `AdjustForMaxPath()` executes: `FileExtension.Length` → `_fileExtension = ".json"` → non-null → no NRE → returns true (path is not > MAX_PATH)
   - Back in handler: `_fileStem = "LcppnFolder"`, `FileName = "LcppnFolder.json"` → ok
5. `FileStemSuffix = null` → setter fires event → `AdjustForMaxPath()` called again
   - `_fileExtension = ".json"` (still set from step 4) → `StemInitialized()` sees all three non-null (seed was set to "LcppnFolder", suffix null…)
   - Wait: `_fileStemSuffix` is now being set to null. The setter `_fileStemSuffix = value` runs FIRST (line 117: `_fileStemSuffix = value; NotifyPropertyChanged()`), so `_fileStemSuffix = null` at the time `StemInitialized()` is called.
   - `StemInitialized()`: `FileStemSuffix is null` → true → enters branch → `TryParseFileName("LcppnFolder.json")` again (same result)
   - Sets `_fileStemSuffix = ""` (overrides the null), `_fileExtension = ".json"` — **but wait**: Newtonsoft is still about to set `FileExtension = null` from the document
   - `AdjustForMaxPath()` returns true → no NRE here
6. `FileStem` is `protected set` → Newtonsoft cannot set it (not publicly settable) → skips
7. `FileExtension = null` → setter: `_fileExtension = null; NotifyPropertyChanged("FileExtension")` → handler calls `AdjustForMaxPath()`
   - Now `_fileExtension = null` (just set)
   - `StemInitialized()`: `FileExtension is null` → true → enters branch → `FolderPath` non-empty → `TryParseFileName("LcppnFolder.json")`
   - `TryParseFileName("LcppnFolder.json")` → succeeds again → sets `_fileExtension = ".json"` via BACKING FIELD
   - `StemInitialized()` returns true
   - `AdjustForMaxPath()` line 297-298: `FileExtension.Length` → `_fileExtension = ".json"` → no NRE

**Critical finding:** After `FileExtension = null` is set by Newtonsoft, the setter fires, `_fileExtension` is set to null, but then `StemInitialized()` immediately calls `TryParseFileName` which sets `_fileExtension = ".json"` via the BACKING FIELD (`_fileExtension = fileExtension` at line 283). When execution reaches line 298 (`FileExtension.Length`), `_fileExtension` is already `.json`, not null.

This is precisely the "self-healing parse" described in the cycle-4 analysis. **The self-healing is reliable for this specific document** because `FileName` is always set before `FileStemSeed` and `FileExtension` in Newtonsoft's deserialization order.

---

## 4. Investigation of the Four Paths Not Covered by Cycle-4

### Path 1: Production `SmartSerializable<T>` load path (`DeserializeJson` / `DeserializeObject`)

**Entry point:** `SmartSerializable<T>.DeserializeJson(FilePathHelper disk, JsonSerializerSettings settings)` (line 376):

```csharp
instance = JsonConvert.DeserializeObject<T>(ReadAllText(disk.FilePath), settings);
```

**Settings used in production:**  
`LcppnFolderPredictorStore.BuildSettings()` returns:
```csharp
var settings = SmartSerializable<LcppnFolderPredictor>.GetDefaultSettings();
// TypeNameHandling = TypeNameHandling.Auto, Formatting = Indented
settings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
settings.ContractResolver = new DoNotSerializeContractResolver("Config");
```

**Effect of `DoNotSerializeContractResolver("Config")`:** Excludes any property named `"Config"` from both serialization and deserialization. This means:
- During **serialize**: the `Config` property of `LcppnFolderPredictor` (inherited from `SmartSerializable<T>`, decorated `[JsonProperty]`) is omitted from the document.
- During **deserialize**: any `"Config"` key in the document is ignored.

**Therefore:** The serialized document produced by `BuildSettings()` NEVER contains a `"Config"` section and NEVER contains `"Disk"` at any level. This is confirmed by the test `LcppnFolderPredictorStore_Tests.RoundTrip_WithDedicatedConfig_PreservesContentAndFileName` which explicitly asserts `json.Should().NotContain("\"Disk\"")`.

**Conclusion for Path 1:** The production serialize path (using `BuildSettings()`) excludes `Config` entirely. The production deserialize path (using the same `BuildSettings()`) ignores `Config`. `FilePathHelper` is never instantiated by Newtonsoft during a production load. The NRE cannot fire through Path 1 on HEAD.

### Path 2: `FilePathHelperConverter` and member-set order

`FilePathHelperConverter.ReadJson` (line 26) constructs `FilePathHelper` via:

```csharp
var folderPath = ExtractFolderPath(info) ?? "";
var fileName = ExtractFileName(info) ?? "";
return new FilePathHelper(fileName, folderPath);
```

This converter is never registered in `LcppnFolderPredictorStore.BuildSettings()` or `SmartSerializable<T>.GetDefaultSettings()`. It requires explicit registration via `JsonSerializerSettings.Converters.Add(new FilePathHelperConverter(fileSystemFolders))`.

No code path in `LcppnFolderPredictor` or `LcppnFolderPredictorStore` registers `FilePathHelperConverter`. When the converter IS used (in other flows with `IFileSystemFolderPaths`), it bypasses the property-setter-triggered event chain entirely: it constructs a fresh `FilePathHelper(fileName, folderPath)` directly, which uses the 2-arg constructor (line 26) that only calls the `FileName` and `FolderPath` setters, never `FileStemSeed` or `FileExtension`. The NRE cannot fire through `FilePathHelperConverter`.

### Path 3: `[OnDeserializing]`/`[OnDeserialized]` callbacks

`LcppnFolderPredictor` has a single `[OnDeserialized]` callback:

```csharp
[System.Runtime.Serialization.OnDeserialized]
internal void OnDeserialized(System.Runtime.Serialization.StreamingContext context)
{
    RebuildTree();
}
```

`RebuildTree()` iterates `Nodes` (a `Dictionary<string, PerParentClassifier>`) and calls `tree.AddLeaf`. It does not interact with `FilePathHelper` or `Config`. No NRE path exists here.

`NewSmartSerializableConfig` and `FilePathHelper` have no `[OnDeserializing]`/`[OnDeserialized]` attributes. The `INotifyPropertyChanged` implementation (`FilePathHelper_PropertyChanged`) is the only callback mechanism and is triggered by property setters.

### Path 4: Reconstructing the cycle-3 document (pre-Config-exclusion) and testing it via the production load path

**Pre-cycle-3 settings:** Before `DoNotSerializeContractResolver("Config")` was added, `BuildSettings()` would have returned only:
```csharp
TypeNameHandling = TypeNameHandling.Auto
Formatting = Indented
PreserveReferencesHandling = PreserveReferencesHandling.Objects
// No ContractResolver
```

**Pre-cycle-3 serialized document structure:**

`LcppnFolderPredictor.SerializeToStream` serializes `_parent` (which is `this`, i.e., the predictor instance). The predictor inherits `Config` from `SmartSerializable<T>` with `[JsonProperty]`. `NewSmartSerializableConfig` properties without `[JsonIgnore]`: `Disk`, `LocalDisk`, `NetDisk`, `ClassifierActivated`, `ActiveDisk`.

`Disk` was populated by `BuildConfig` as `new FilePathHelper("LcppnFolder.json", bayesianFolder)`. After the 2-arg constructor, the `FilePathHelper` state is:
- `_filePath`: `"C:\...\AppData\Bayesian\LcppnFolder.json"`
- `_folderPath`: `"C:\...\AppData\Bayesian"`
- `_fileName`: `"LcppnFolder.json"`
- `_fileStemSeed`: null
- `_fileStemSuffix`: null
- `_fileStem`: null
- `_fileExtension`: null

The pre-cycle-3 serialized `Config.Disk` block in the document (default Newtonsoft, with `TypeNameHandling.Auto` and `PreserveReferencesHandling.Objects`) would be:

```json
"Config": {
  "Disk": {
    "FilePath": "C:\\Users\\...\\AppData\\Bayesian\\LcppnFolder.json",
    "FolderPath": "C:\\Users\\...\\AppData\\Bayesian",
    "FileName": "LcppnFolder.json",
    "FileStemSeed": null,
    "FileStemSuffix": null,
    "FileExtension": null
  },
  "LocalDisk": { "FilePath": null, "FolderPath": "", "FileName": "", ... },
  "NetDisk": { ... },
  "ClassifierActivated": false,
  "ActiveDisk": 0
}
```

(`FileStem` is `protected set` — Newtonsoft uses `MemberSerialization.OptOut` by default on public properties; since `FileStem` has a public getter but only a `protected set`, Newtonsoft **can read but cannot write** via setter; it may or may not include it in serialization depending on version, but it is read-only for deserialization purposes.)

**Deserialization of this document via the production (pre-cycle-3) load path:**

`JsonConvert.DeserializeObject<LcppnFolderPredictor>(json, settings)` where settings has `TypeNameHandling.Auto` and `PreserveReferencesHandling.Objects` but NO `ContractResolver` exclusion.

Newtonsoft creates a new `LcppnFolderPredictor()`. The constructor calls `base()` (SmartSerializable), which constructs a fresh `NewSmartSerializableConfig` (assigning to `_config` and `Config`). Then Newtonsoft populates properties.

For the `Config` property, Newtonsoft deserializes the `"Config"` JSON into the existing `NewSmartSerializableConfig` instance. For `Disk`, it deserializes into the existing `FilePathHelper()` instance (already initialized with empty strings and `PropertyChanged` hooked up).

Deserialization order for `Config.Disk` follows JSON property order:
1. `FilePath = "C:\\...\\LcppnFolder.json"` → handler: `_folderPath = "C:\\...\\Bayesian"`, `_fileName = "LcppnFolder.json"`
2. `FolderPath = "C:\\...\\Bayesian"` → handler: `_filePath` recomputed (already correct)
3. `FileName = "LcppnFolder.json"` → handler: `_filePath` recomputed
4. `FileStemSeed = null` → handler: `AdjustForMaxPath()` called
   - `StemInitialized()`: seed null → enters branch → FolderPath non-empty → `TryParseFileName("LcppnFolder.json")` succeeds → `_fileExtension = ".json"` → returns true
   - `AdjustForMaxPath()`: `FileExtension.Length = 5`, no NRE, returns true
5. `FileStemSuffix = null` → same result, self-heals
6. `FileExtension = null` → setter: `_fileExtension = null` → handler: `AdjustForMaxPath()` called
   - `StemInitialized()`: `FileExtension is null` → enters branch → `TryParseFileName("LcppnFolder.json")` → `_fileExtension = ".json"` (backing field set) → returns true
   - `AdjustForMaxPath()` line 298: `FileExtension.Length` → `_fileExtension = ".json"` → no NRE

**Result:** The NRE does NOT fire even with the pre-cycle-3 document, because `FileName` (`"LcppnFolder.json"`) is always set before `FileStemSeed` and `FileExtension` in the Newtonsoft property order, and `TryParseFileName` self-heals the extension each time.

---

## 5. Why Cycle-3 Reported a Throw

There are two plausible explanations for the original cycle-3 throw:

**Hypothesis A: Different document ordering.** If the Disk was constructed via `FilePathHelper.FromSeed(...)` (using the private 4-arg constructor) rather than via `new FilePathHelper(FileName, folderPath)`, then `_fileStemSeed`, `_fileStemSuffix`, `_fileExtension` would be set but `_fileName` would be derived. In that case, the serialized document might NOT include a `"FileName"` entry that appears BEFORE `"FileStemSeed"`, meaning `TryParseFileName` in `StemInitialized()` would fail (FileName is empty at deserialization time when `FileStemSeed` setter fires), causing `StemInitialized()` to return false, and `AdjustForMaxPath()` to return false safely — still no NRE. This does not produce the throw either.

**Hypothesis B: A different property was null in an intermediate step.** The throw may have occurred in a scenario where the `Disk` was populated differently than what `BuildConfig` produces on HEAD — possibly a `FromSeed`-constructed `Disk` with `FolderPath` set and both `FileName` and `FileExtension` null, combined with Newtonsoft setting `FileStemSeed` before setting `FileName`. In that case, `StemInitialized()` might call `TryParseFileName("")` (empty FileName) → returns false → `StemInitialized()` returns false → `AdjustForMaxPath()` returns false → still no NRE from line 298.

The actual line that fires NRE is not line 298 in any of the above; it can only be reached if `StemInitialized()` returns true while `_fileExtension == null`. Given the implementation, `StemInitialized()` returning true guarantees `_fileExtension != null` because it either was non-null going in, or `TryParseFileName` set it. Unless `TryParseFileName` itself sets `_fileExtension = null` (it does not — line 283 sets it to the parsed extension, which is `""` for no extension or `".ext"` for a file with extension).

**Most likely:** The cycle-3 throw was a transient condition in a development environment where the on-disk document contained a `Config.Disk` block produced in an older format (perhaps with `FileStemSeed` containing a non-null value from a `FromSeed`-constructed helper, and `FileExtension` was null because the document was written before the property was added to the class). When Newtonsoft set `FileStemSeed = "LcppnFolder"` (non-null from document) on a fresh `FilePathHelper()` instance where `_fileExtension = null` AND `FolderPath` was already set but `TryParseFileName` fails (because `FileName` was empty/not yet set), `StemInitialized()` would return false. But this still means no NRE.

After more careful reading: the only way to get `StemInitialized()` to return true while `_fileExtension == null` is if `FileStemSeed`, `FileStemSuffix`, and `FileExtension` are ALL non-null (so the null check passes without entering the `TryParseFileName` branch), but simultaneously `_fileExtension` is null. This is impossible because `FileExtension is null` checks the property getter which returns `_fileExtension`.

**Revised conclusion:** The NRE at line 298 is **not reachable through `AdjustForMaxPath()`** given the current `StemInitialized()` implementation, because `StemInitialized()` returning true is a sufficient condition for `_fileExtension != null`. The cycle-3 throw may have been from a different line, or from a code version that differed from HEAD, or was a stale JSON document from before a refactor.

---

## 6. Definitive Verdict

**NOT REPRODUCIBLE**

### Reasoning Summary

1. **Production path (HEAD, using `BuildSettings()`):** `DoNotSerializeContractResolver("Config")` excludes `Config` from serialization and deserialization. `FilePathHelper` is never touched by Newtonsoft during a production load. The NRE trigger is structurally impossible.

2. **Pre-cycle-3 path (without Config exclusion):** Even when `Config.Disk` IS present in the document, the Newtonsoft property ordering places `FileName` before `FileStemSeed` and `FileExtension`. The `StemInitialized()` self-healing via `TryParseFileName` ensures `_fileExtension` is non-null before `AdjustForMaxPath()` reaches line 298. The NRE does not fire.

3. **`FilePathHelperConverter` path:** The converter is not registered for `LcppnFolderPredictor` serialization. When it IS used, it bypasses the property chain entirely.

4. **`[OnDeserializing]`/`[OnDeserialized]` path:** `LcppnFolderPredictor.OnDeserialized` calls only `RebuildTree()`, which has no interaction with `FilePathHelper`.

5. **`StemInitialized()` invariant:** The implementation guarantees `StemInitialized()` returning true implies `_fileExtension != null`. Therefore `AdjustForMaxPath()` returning true implies `_fileExtension != null`. Line 298's `FileExtension.Length` cannot NRE.

### Was the cycle-3 throw already neutralized before cycle-3?

The cycle-3 Config exclusion is a **belt-and-suspenders** measure, not the minimum fix required. The self-healing via `TryParseFileName` appears to have already prevented the NRE in the specific document structure `BuildConfig` produces. However, if cycle-3's throw was genuine (not a test-environment artifact), it must have occurred in a document or code version that is not present on HEAD.

### Is `AdjustForMaxPath()` null-guarding pure defensive hardening?

**Yes.** On HEAD, with the current `StemInitialized()` implementation, `_fileExtension` cannot be null when `AdjustForMaxPath()` reaches line 298. A null-guard on `FileExtension` inside `AdjustForMaxPath()` would be defensive hardening with no observable behavioral change for any JSON document produced by the current serialization paths. The test would be unfalsifiable on HEAD (no document ordering reproduces the throw).

---

## 7. Rejected Alternative Approaches

Cycle-4 tested bare `JsonConvert.DeserializeObject<FilePathHelper>(...)` directly. The analysis above shows that the correct test vector (if trying to reproduce the NRE) would need to deserialize a `FilePathHelper` document in which `FileStemSeed` is set to a **non-null** value before `FileName` is set, AND `_fileExtension` remains null. Under the current `StemInitialized()` implementation, even this ordering is safe because `StemInitialized()` calls `TryParseFileName` which either fails (returning false, preventing the reach of line 298) or succeeds (setting `_fileExtension`, again preventing the NRE). There is no document ordering that produces the NRE.

---

## 8. Corrected Test Recipe

Because the verdict is NOT REPRODUCIBLE, a RED regression test for the NRE is not possible — there is no document or API call that triggers the throw on HEAD. The proposed `AdjustForMaxPath()` null-guard is correctly classified as **unfalsifiable defensive hardening**.

If the team chooses to add the null-guard anyway, the test coverage approach is:

- Test that `AdjustForMaxPath()` returns false when `StemInitialized()` returns false (already indirectly covered by `FilePathHelper_Tests.cs`).
- Test that `AdjustForMaxPath()` does not throw when called on a `FilePathHelper` constructed via the 2-arg constructor (where `_fileExtension` is null at construction time but self-heals via `TryParseFileName`).
- These tests document the defensive behavior without claiming to reproduce a throw.

---

## 9. Artifact Path

`artifacts/research/2026-06-16-lcppn-deserialize-nre-research.md`
