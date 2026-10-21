# Research: Folder Settings never persist; User Email shows "Error Loading" (Issue #797)

- Date: 2026-09-06
- Branch: `bug/folder-settings-never-persist-797`
- Base commit: `c431dc3297e864041d829e8d79b348960b8d8019`
- Requirements source: `docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/issue.md` (AC1-AC8, authoritative, not renumbered or weakened here)
- Scope: research only. No source file was modified.

All line numbers below were re-derived by reading the files in this worktree at the base commit. Where a line number or attribution in `issue.md` has moved or is incorrect, that is called out explicitly.

---

## 0. Corrections to the citations in `issue.md`

These are verified corrections, not disagreements with the acceptance criteria. Every AC stands as written.

| Cited in `issue.md` | Verified state at `c431dc32` | Effect |
|---|---|---|
| `SmartSerializableBase.cs:167-188` is the `Deserialize<T,U>(loader)` on the live StoresWrapper path | Those line numbers are correct **for that file**, but the live path does **not** enter `SmartSerializableBase`. `AppOlObjects.SmartSerializable` is an `ISmartSerializableNonTyped` (`TaskMaster\AppGlobals\AppOlObjects.cs:40-41`); `SmartSerializableNonTyped.Deserialize<T,U>` (`UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializableNonTyped.cs:54-56`) calls `GetInstance<T>()` which returns `SmartSerializable<T>` (`:34-35`), so overload resolution binds `SmartSerializable<T>.Deserialize<U>(SmartSerializable<U> loader)` at `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializable.cs:214-234`. | The defect shape is identical in both classes (config copy guarded by `if (instance is not null)`), but the **regression surface** (A3) and any edit target must be read off `SmartSerializable.cs`, not `SmartSerializableBase.cs`. |
| null return at `SmartSerializableBase.cs:335-342` | Correct for that file. The corresponding live site is `SmartSerializable.cs:388-394` (`DeserializeJson`, `if (!DiskExists(disk)) return instance;` where `instance` is `null`). | Same behaviour, different file. |
| instance-only config copy at `:176-180` | Correct for `SmartSerializableBase.cs`. Live site: `SmartSerializable.cs:222-225` (`if (instance is not null) { instance.Config.CopyFrom(loader.Config, true); }`). | Same. |
| CONTRAST overload at `:190-240`, unconditional copy at `~236` | Correct for `SmartSerializableBase.cs` (`:190-245`, copy at `:236`). Live sibling: `SmartSerializable.cs:257-310`, unconditional copy at `:302`. | Same. |
| `SmartSerializable.cs:442-448` (`Serialize`) and `:550-559` (`RequestSerialization`) | **Exactly correct.** | No change. |
| `FilePathHelper.cs:72-102` default `FilePath` of `""` | Correct: `_filePath = ""` at `UtilitiesCS\HelperClasses\FileSystem\FilePathHelper.cs:71`, property `:72-80`; the `FolderPath`/`FileName` siblings follow at `:82-102`. | No change. |
| `IntelligenceResources.resx` ~176-203 | Correct: the `StoresWrapper` data element is `UtilitiesCS\IntelligenceResources.resx:176-204`, `FileName` `StoresWrapper.json` at `:185`, `SpecialFolderName` `AppData` at `:187`. | No change. |
| `StoreWrapper.cs:179-217` (`GetSmtpAddressFromStore`), `~83` (`Init` calls it once) | Correct: method `:179-217`, throwing read `RootFolder?.Session?.CurrentUser` at `:184`, single call site `UserEmailAddress = GetSmtpAddressFromStore();` at `:83`. | No change. |
| `StoreWrapperController.cs:288-296`, `~169`, `348-357`, `391-418`, `456-474`, single `&` at `~464` | All correct. `PopulateWithCurrent` is `:279-314`; the unguarded dereferences are `:288-291`; the null-safe reads are `:294-296`. | No change. |
| `AppOlObjects.JunkFolders.cs:27-34` | The .NET user-settings write **pair** is `:27-34`; the method `ApplyJunkFolderSelections` that the reflection call targets is `:36-45`. | Minor: the reflection target is at `:36-45`, the setting writers at `:24-34`. |
| `AppAutoFileObjects.cs:217-222` (RecentFolders, `askUserOnError` overload) | Correct: `TaskMaster\AppGlobals\AppAutoFileObjects.cs:217-224`, `SloLinkedList<string>.Static.DeserializeAsync(config, true)` at `:219-222`. | No change. |
| "the same session's `ThreadMonitor` captured the UI thread inside `_ExchangeUser.get_PrimarySmtpAddress()` at 17:35:21, so a second caller of this chain also blocks on it" | **Partly incorrect.** The captured stack at 17:35:21 is `RecipientStatic.GetRecipientAddress` -> `RecipientStatic.GetRecipientInfo` -> `MailItemHelper.InitLazyFields` -> `QfcCollectionController.GetPartiallyInitializedHelperAsync`. That is the QuickFiler recipient-resolution path in `UtilitiesCS\OutlookObjects\Recipient\RecipientStatic.cs:458`, **not** `StoreWrapper.GetSmtpAddressFromStore`. | The blocking hazard for `_ExchangeUser.get_PrimarySmtpAddress()` is real and repeatedly evidenced (17:35, 17:40 x3, 17:43, 19:06, 20:02) but it is attributable to a **different, out-of-scope** caller. Do not fix that caller under #797. |

Runtime evidence actually confirming root causes 1 and 2 (read-only; log lives outside the repo):

```
2026-09-06 17:29:59,517 [VSTA_Main] WARN  TaskMaster.AppOlObjects - StoresWrapper config deserialized to null; rebuilding from live stores.
2026-09-06 17:29:59,592 [VSTA_Main] ERROR UtilitiesCS.OutlookObjects.Store.StoreWrapper - Error retrieving PrimarySmtpAddress from secondary inbox. The operation failed.
```

The same two lines recur at 19:09:20 and 19:26:35, i.e. once per Outlook start, confirming that the file is never created and the SMTP lookup fails on every start.

---

## A. Serializer bootstrap

### A1. Every `Deserialize` overload, and whether it copies the loader's `Config`/`Disk`

#### A1.1 `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializableBase.cs` (as asked)

| Line | Signature | Copies loader `Config`? | File present | File absent | Deserialize error |
|---|---|---|---|---|---|
| `:126` | `Deserialize<T>(string fileName, string folderPath)` | n/a (no loader) | delegates to `:132` | delegates | delegates |
| `:132` | `Deserialize<T>(string, string, bool askUserOnError)` | n/a | delegates to `:247` | delegates | delegates |
| `:140` | `Deserialize<T>(string, string, bool, JsonSerializerSettings)` | n/a | delegates to `:247` | delegates | delegates |
| `:167` | `Deserialize<T,U>(SmartSerializable<U> loader)` | **Yes, but only inside `if (instance is not null)` at `:177-180`** (`config?.CopyFrom(loader.Config, true)` at `:179`) | copies | **returns `null`, copies nothing** (`DeserializeJson<T>` returns `null` at `:339-342`) | `DeserializeJson<T>` logs and returns `null` at `:347-350`, so also **copies nothing** |
| `:190` | `Deserialize<T,U>(SmartSerializable<U> loader, bool askUserOnError, Func<T>? altLoader)` | **Yes, unconditionally at `:236`** | copies | `CreateEmpty` at `:220`, then copy at `:236`, then `Serialize(instance!)` at `:241` | `CreateEmpty` at `:231`, then copy at `:236`, then write |
| `:247` | `protected Deserialize<T>(FilePathHelper disk, bool, JsonSerializerSettings)` | Copies `disk.FilePath` and settings only, at `:291-296` | copies | `CreateEmpty` at `:274` then `:291-296` then write at `:300` | `CreateEmpty` at `:285` then `:291-296` then write |

Related non-`Deserialize`-named members on the same class: `TryDeserialize<T,U>` `:152` (wraps `:167`); `DeserializeAsync<T,U>` `:305`, `:314`, `:324` (wrap `:167`, `:190`, `:190`); `DeserializeJson<T>` `:335`, `:382`; `DeserializeObject<T>` `:362`.

Only `:167` (single-argument loader form) discards the loader's disk configuration, and only on the file-absent / deserialize-error paths.

#### A1.2 `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializable.cs` (the class actually on the StoresWrapper path)

| Line | Signature | Copies loader `Config`? | File present | File absent | Deserialize error |
|---|---|---|---|---|---|
| `:177` | `Deserialize(string, string)` | n/a | delegates to `:182` | delegates | delegates |
| `:182` | `Deserialize(string, string, bool)` | n/a | delegates to `:312` | delegates | delegates |
| `:189` | `Deserialize(string, string, bool, JsonSerializerSettings)` | n/a | delegates to `:312` | delegates | delegates |
| `:214` | `Deserialize<U>(SmartSerializable<U> loader)` | **Yes, guarded at `:222-225`** | copies | **returns `null` (as `instance!`), copies nothing** | `DeserializeJson` logs at `:399-402` and returns `null`; copies nothing |
| `:236` | `Deserialize<U>(ISmartSerializable<U> loader)` | Same guarded shape at `:244-247` | copies | returns `null` | returns `null` |
| `:257` | `Deserialize<U>(SmartSerializable<U>, bool, Func<T>?)` | **Yes, unconditionally at `:302`**, plus `instance!.Serialize()` at `:306` when `writeInstance` | copies | `CreateEmpty` `:286` -> copy `:302` -> write `:306` | `CreateEmpty` `:297` -> copy `:302` -> write |
| `:312` | `protected Deserialize(FilePathHelper, bool, JsonSerializerSettings)` | Copies `disk.FilePath` only, at `:355` | copies | `CreateEmpty` `:338` -> `:355` -> write `:359` | `CreateEmpty` `:349` -> `:355` -> write |

Related members: `TryDeserialize<U>` `:200`; `DeserializeAsync<U>` `:364`, `:372`, `:378`; `DeserializeJson` `:388`, `:432`; `DeserializeObject` `:410`; nested `Static` forwarders at `:569`, `:572`, `:575`, `:582`, `:588`, `:592`, `:599`.

Live trace for #797: `TaskMaster\AppGlobals\AppOlObjects.StoreLoading.cs:41-44` -> `SmartSerializableNonTyped.cs:54-56` -> `SmartSerializable.cs:214-234` -> `SmartSerializable.cs:388-394` returns `null` because `DiskExists(disk)` is false -> the `if (instance is not null)` guard at `:222` skips the copy -> `StoreLoading.cs:51-53` logs the observed WARN -> `StoreLoading.cs:64` calls `BuildFreshStoresWrapper()` (`StoreLoading.cs:32-33`) -> `new StoresWrapper(_globals).Init()` whose `Config.Disk.FilePath` is the `FilePathHelper` default `""` (`FilePathHelper.cs:71`) -> `StoreWrapperController.SaveChanges` `:356` calls `Model.Serialize()` -> `SmartSerializable.cs:444` `if (Config.Disk.FilePath != "")` is false -> silent return.

### A2. Minimal change for AC1, and where it belongs

**Recommendation: put the fix in `TaskMaster\AppGlobals\AppOlObjects.StoreLoading.cs` only. Do not change `SmartSerializable.cs` or `SmartSerializableBase.cs` for AC1.**

Rationale (evidence-based, not preference):

1. **There is no instance to copy onto in the serializer.** On the file-absent path `DeserializeJson` returns `null` (`SmartSerializable.cs:391-394`). "Make the file-absent path adopt the loader's disk configuration" is therefore not expressible as a copy inside `Deserialize<U>(loader)`; it would require *constructing* an instance, which changes the method's null-returning contract.
2. **That null contract is load-bearing for a second production caller.** `TaskMaster\AppGlobals\AppAutoFileObjects.FolderPredictorLoad.cs:69-85` calls `FolderPredictorDeserializer(loader)` (default `LcppnFolderPredictor.Static.DeserializeAsync(loader)`, `:42`), and its own comment at `:74-76` states: "DeserializeAsync returns null when the dedicated file is absent (fail-soft); the holder then stays null and the accessor falls back to flat." Making the overload return a constructed instance would silently disable that fallback.
3. **Blast radius.** The `ReusableTypeClasses` tree is shared with three concurrent sibling work items. `SmartSerializable.cs` is already **613 lines** and `SmartSerializableBase.cs` **545 lines** — both already over the 500-line cap in `.claude/rules/general-code-change.md`. Any edit there is a merge-conflict magnet and worsens an existing violation. `AppOlObjects.StoreLoading.cs` is **75 lines** with ample headroom and is owned solely by this issue.
4. **The seam already exists and is already unit-tested.** `BuildFreshStoresWrapper()` is `protected internal virtual` (`StoreLoading.cs:32-33`) and is overridden by `TestableAppOlObjects` in `TaskMaster.Test\AppGlobals\AppOlObjectsCoverageTests.cs` (see `:78-143`, `:186-201`). `LoadStoresAsync` already holds the `config` loader in scope at `:39` and can apply `config.Config` to the freshly built wrapper at `:64` with no new type, no new interface, and no COM.

Concrete minimal shape (for the planner, not applied here): in `LoadStoresAsync`, after `StoresWrapper = BuildFreshStoresWrapper();` at `:64`, when the `TryGetValue` at `:39` produced a `config`, copy the loader configuration onto the fresh wrapper — `StoresWrapper.Config.CopyFrom(config.Config, true)` (`NewSmartSerializableConfig.CopyFrom` is `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\Config\NewSmartSerializableConfig.cs:197-214`, and copies `Disk`, `LocalDisk`, `NetDisk`, `ActiveDisk` and the three settings lazies). The `config not found` branch at `:57` has no loader and must remain a fresh build with an empty path; AC2's new error log then makes that case visible rather than silent.

**Not recommended (rejected alternatives, kept brief):**
- Changing `SmartSerializable.cs:214` to construct-and-copy on the null path: breaks the LCPPN fail-soft contract (point 2), edits an over-cap shared file, and would need `askUserOnError` semantics it does not have.
- Switching the call site to the `askUserOnError` overload (`SmartSerializable.cs:257`, the `RecentFolders` design reference): this *would* fix AC1 with one call-site change, but the overload calls `AskUser` -> `MyBox.ShowDialog` (`:163-168`) during VSTO startup and `CreateEmpty` -> `new T()` (`:136`), i.e. `new StoresWrapper()` with no globals rather than `BuildFreshStoresWrapper()`'s `new StoresWrapper(_globals).Init()`. `Globals` would be null and `GetFilteredStores` (`StoresWrapper.cs:190-193`) would throw. Rejected on both counts.

### A3. Complete regression surface — every caller of the affected overload

Affected overload: `SmartSerializable<T>.Deserialize<U>(SmartSerializable<U> loader)`, `SmartSerializable.cs:214-234`.

**Because the A2 recommendation makes no change to this overload, the behavioural regression surface is empty.** The list is nonetheless enumerated in full so the planner can confirm that, and so a reviewer can verify that no alternative was chosen that would touch it.

Direct in-repo callers of `SmartSerializable.cs:214`:

| # | Call site | Kind | Would A2 change it? |
|---|---|---|---|
| 1 | `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializableNonTyped.cs:56` (`Deserialize<T,U>` forwarder) | production forwarder | No |
| 2 | `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializable.cs:205` (`TryDeserialize<U>`) | production forwarder | No |
| 3 | `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializable.cs:369` (`DeserializeAsync<U>(config)`) | production forwarder | No |
| 4 | `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializable.cs:583` (`Static.Deserialize<U>`) | production forwarder | No |
| 5 | `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializable.cs:590` (`Static.DeserializeAsync<U>`) | production forwarder | No |
| 6 | `UtilitiesCS\ReusableTypeClasses\SerializableNew\Concurrent\Observable\ScoDictionaryNew.cs:147` (explicit `ISmartSerializable<>` impl) | production forwarder; no in-repo caller found | No |
| 7 | `UtilitiesCS\ReusableTypeClasses\SerializableNew\Concurrent\Observable\SloStack.cs:85` (`DeserializeAsync(config)`) | production forwarder | No |
| 8 | `UtilitiesCS\ReusableTypeClasses\SerializableNew\Concurrent\Observable\SloLinkedList.cs:71` and `:161` | production forwarders | No |

Terminal (leaf) production entry points that actually reach it at runtime:

| # | Entry point | Path | Would A2 change it? |
|---|---|---|---|
| A | `TaskMaster\AppGlobals\AppOlObjects.StoreLoading.cs:41-44` | via forwarder 1 | **Yes — this is the intended fix site.** The overload's own behaviour is unchanged; the caller gains a post-fresh-build config copy. |
| B | `TaskMaster\AppGlobals\AppAutoFileObjects.FolderPredictorLoad.cs:42` (`LcppnFolderPredictor.Static.DeserializeAsync(loader)`; `LcppnFolderPredictor : SmartSerializable<LcppnFolderPredictor>` at `UtilitiesCS\EmailIntelligence\Bayesian\LcppnFolderPredictor.cs:23-25`) | via forwarders 5 -> 3 | No. Its documented fail-soft null contract (`FolderPredictorLoad.cs:74-76`) is preserved exactly. |

Production entry points that use a **different** overload and are therefore out of the surface: `TaskMaster\AppGlobals\AppToDoObjects.cs:174-178` (3-argument `askUserOnError` form), `TaskMaster\AppGlobals\AppAutoFileObjects.cs:190-194` and `:219-222`, `UtilitiesCS\EmailIntelligence\ClassifierGroups\ManagerAsyncLazy.cs:293-297`, `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapEncoder.cs:23`, `:41`, `:97`, `ToDoModel\Data Model\Project\ProgramData.cs:75`, `:83`, `:97`.

Test callers that pin the current behaviour of the affected overload (must keep passing): `UtilitiesCS.Test\ReusableTypeClasses\SmartSerializable_Tests.cs:406`, `:422`, `:449`, `:468`, `:659`, `:686`, `:708`, `:728`, `:742-749`; `UtilitiesCS.Test\ReusableTypeClasses\SmartSerializableNonTyped_Tests.cs` (whole file); `UtilitiesCS.Test\ReusableTypeClasses\SloLinkedList_Tests.cs:291`, `:367`, `:385`; `UtilitiesCS.Test\ReusableTypeClasses\SerializableNew\Concurrent\Observable\SloStack_Tests.cs:263`, `:367-368`, `:385-387`; `TaskMaster.Test\AppGlobals\AppOlObjectsTests.cs:199-205`; `TaskMaster.Test\AppGlobals\AppOlObjectsCoverageTests.cs:41-47`, `:117-123`, `:157-164`.

### A4. How the resource-defined disk configuration is constructed

1. The resource entry: `UtilitiesCS\IntelligenceResources.resx:176-204`, key `StoresWrapper`, with `Config.Disk = { FileName: "StoresWrapper.json", RelativePath: "", SpecialFolderName: "AppData" }` (`:184-188`), an identical `LocalDisk` (`:189-193`), a `NetDisk` on `"Flow"` (`:194-198`), and `"ActiveDisk": 1` (`:200`).
2. The reader: `UtilitiesCS\EmailIntelligence\IntelligenceConfig.cs`. `GetSerializedConfigurations()` at `:237-246` reads `IntelligenceResources.ResourceManager` and returns every resource as a `name -> json` map. `ReadConfigurationAsync()` at `:77-163` deserializes each into a `SmartSerializableLoader` via `DeserializeLoaderAsync` (`:248-261`) and exposes the result as `IntelligenceConfig.Config` (`:62-66`), a `ConcurrentDictionary<string, SmartSerializableLoader>` surfaced on `IApplicationGlobals.IntelRes` (`UtilitiesCS\Interfaces\IGlobals\IApplicationGlobals.cs:17`).
3. The path materialisation: `SmartSerializableLoader.GetSettings()` (`UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializableLoader.cs:80-88`) registers `new FilePathHelperConverter(Globals.FS)` (`:86`). `FilePathHelperConverter.ReadJson` (`UtilitiesCS\NewtonsoftHelpers\FilePathHelperConverter.cs:29-42`) resolves `SpecialFolderName` through `FileSystemFolders.SpecialFolders` (`:44-65`) and returns `new FilePathHelper(fileName, folderPath)`, whose constructor sets `FilePath = Path.Combine(folderPath, fileName)` (`FilePathHelper.cs:28-34`).
4. `"AppData"` resolves to `%LocalAppData%\TaskMaster`: `TaskMaster\AppGlobals\AppFileSystemFolderPaths.cs:216-223` registers `AppData` as `[Environment.GetFolderPath(SpecialFolder.LocalApplicationData), nameof(TaskMaster)]`.
5. `DeserializeConfig` finishes with `instance.Config.ActivateMostRecent()` (`SmartSerializableLoader.cs:195`), which copies `LocalDisk` or `NetDisk` into `Disk` (`NewSmartSerializableConfig.cs:145-169`).

**How a caller obtains the correctly-pathed loader:** exactly what `LoadStoresAsync` already does — `_globals.IntelRes.Config.TryGetValue("StoresWrapper", out var config)` at `TaskMaster\AppGlobals\AppOlObjects.StoreLoading.cs:39`. The `config` in scope at `:39-54` **already carries** `Config.Disk.FilePath == %LocalAppData%\TaskMaster\StoresWrapper.json`. Nothing new needs to be constructed for AC1; the value is present and is simply discarded on the fresh-build branch.

---

## B. Serialize guard and deferred write

### B1. The guard, and the available logger seam

`UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializable.cs:442-448`:

```csharp
public void Serialize()
{
    if (Config.Disk.FilePath != "")
    {
        RequestSerialization(Config.Disk.FilePath);
    }
}
```

Two properties matter for AC2. First, the comparison is `!= ""` only — a **null** `FilePath` passes the guard and reaches `RequestSerialization(null)`; `FilePathHelper.FilePathHelper_PropertyChanged` can assign `_filePath = null!` at `:358` and `:366`. AC2's wording ("empty or null") therefore covers a genuinely reachable second case. Second, the sibling `SmartSerializableBase.Serialize<T>(T instance)` at `SmartSerializableBase.cs:422-430` already uses `IsNullOrEmpty` and is the consistent shape to converge on.

Logger seam available on the type: `SmartSerializable<T>` declares `private static readonly log4net.ILog logger` at `:26-28`. It is **not injectable**. Three options, in ascending intrusiveness:

- **(a) log4net `MemoryAppender` attached to `typeof(SmartSerializable<TestSmartItem>).FullName`.** No production change beyond the `logger.Error(...)` call itself. The pattern is already proven in `TaskMaster.Test\AppGlobals\ApplicationGlobalsStartupTimingTests.cs:202-218` and `TaskMaster.Test\AppGlobals\AppEventsTests.Helpers.cs:228-241`. `UtilitiesCS.Test` has a direct `log4net` reference (`UtilitiesCS.Test\UtilitiesCS.Test.csproj:576-578`) so the same helper compiles there, though `UtilitiesCS.Test` does not currently use it anywhere. **Recommended** — it adds no new production surface to a shared, over-cap file.
- (b) A `protected virtual void LogSerializeError(string message)` hook overridden by the existing `SmartSerializableHarness` (`UtilitiesCS.Test\ReusableTypeClasses\SmartSerializable_Tests.cs:771-806`). Matches the file's existing protected-seam idiom (`ReadAllText`, `DiskExists`, `ShowDialog`, `CreateStreamWriter`, `TimerFactory`).
- (c) An injectable `Action<string>` sink, the shape used by `StoreWrapperInitProbe` (`UtilitiesCS\OutlookObjects\Store\StoreWrapperInitProbe.cs`, constructed as `new StoreWrapperInitProbe(s => logger.Debug(s))` at `StoreWrapper.cs:92`). More surface than (b) for no extra benefit here.

### B2. `RequestSerialization` timer mechanics

`SmartSerializable.cs:550-559`:

```csharp
protected void RequestSerialization(string filePath)
{
    if (_serializationRequested.CheckAndSetFirstCall)
    {
        _timer = TimerFactory(TimeSpan.FromSeconds(3));
        _timer.Elapsed += (sender, e) => SerializeThreadSafe(filePath);
        _timer.AutoReset = false;
        _timer.StartTimer();
    }
}
```

- **Timer type:** `ITimerWrapper` (`UtilitiesCS\Interfaces\ITimerWrapper.cs:6-10`) produced by `TimerFactory` (`SmartSerializable.cs:547-548`), defaulting to `new TimerWrapper(interval)`. `TimerWrapper` wraps a `System.Timers.Timer` through `SystemTimersTimerAdapter` (`UtilitiesCS\ReusableTypeClasses\TimedActions\TimerWrapper.cs:36-77`), whose `Elapsed` callback runs on a **ThreadPool** thread, not the UI thread.
- **Interval:** fixed 3 seconds, `AutoReset = false` (single shot).
- **Repeat calls coalesce; they do not reset.** The gate is `ThreadSafeSingleShotGuard.CheckAndSetFirstCall` (`UtilitiesCS\Threading\ThreadSafeSingleShotGuard.cs:24-27`, an `Interlocked.Exchange`). The first call inside a window arms the timer; every subsequent call returns immediately without creating or restarting a timer. Consequently the **`filePath` captured is the first caller's**, and the guard is only re-armed in `SerializeThreadSafe`'s `finally` at `:498` (`_serializationRequested = new ThreadSafeSingleShotGuard();`). A second `Serialize(otherPath)` inside the window silently writes to the first path.
- **Process exit:** `System.Timers.Timer` raises `Elapsed` on a ThreadPool (background) thread. Background threads are not joined at process exit, so a write still pending when Outlook tears down the AppDomain is **lost with no log entry**. This is exactly the AC4 loss window.

### B3. Candidate hosts for an AC4 flush

| Candidate | Location | Verdict |
|---|---|---|
| `ThisAddIn_Shutdown` | `TaskMaster\ThisAddIn.cs:287-291`, wired at `:302` | **Unusable.** The body carries the stock VSTO comment: "Note: Outlook no longer raises this event." Verified present at the base commit. A flush placed here would never run. |
| `ThisAddIn.Designer.OnShutdown` | `TaskMaster\ThisAddIn.Designer.cs:162-165` | Designer-generated; must not be hand-edited, and it is downstream of the same unraised-event problem. |
| `AppOlObjects.FolderTreeService` disposal | `TaskMaster\AppGlobals\AppOlObjects.FolderTreeService.cs:374` (`Dispose()`), `:240`, `:246`, `:407` | Exists, but is a folder-tree-service lifetime hook, not an application-shutdown hook, and there is no evidence it runs at Outlook teardown. Out of scope per the footprint constraint. |
| **Synchronous write on explicit Save** | `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs:356` (`Model.Serialize();` inside `SaveChanges`) | **Recommended.** |

**Is a synchronous write on explicit Save feasible without changing deferred behaviour for other callers?** Yes. `SerializeThreadSafe(string filePath)` is already `public` on `SmartSerializable<T>` (`:474-501`); it takes the write lock, writes through the injectable `CreateStreamWriter`, and re-arms the single-shot guard in its `finally`. Calling it directly from `SaveChanges` in place of (or in addition to) `Model.Serialize()` writes the file inline on the UI thread and leaves `Serialize()`/`RequestSerialization` untouched for every other caller. Two facts to respect:

1. `SerializeThreadSafe` calls `_parent.ThrowIfNull(...)` at `:476-478`. `StoresWrapper` sets `base._parent = this` in both constructors (`UtilitiesCS\OutlookObjects\Store\StoresWrapper.cs:28`, `:33`), so the guard is satisfied on both the deserialized and fresh-built models.
2. AC2's empty/null-path error must be evaluated **before** any synchronous write, otherwise the fix would substitute one silent failure (`Serialize` no-op) for another (`File.CreateText("")` throwing inside `SerializeThreadSafe`'s own catch at `:490-493`). The cleanest shape is a new small guarded method rather than a bare `SerializeThreadSafe` call from the controller.

`SmartSerializable<T>.Serialize()` is **not virtual**, so a Moq mock of `StoresWrapper` cannot intercept it. AC4 tests must use the `CreateStreamWriter` seam (see F1) rather than `Mock<StoresWrapper>.Verify`.

---

## C. Junk-folder double persistence (AC5)

### C1. The reflection lookup, and whether the target exists

`UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs:391-418`:

```csharp
internal void PersistJunkFolderSelections()
{
    var olObjects = Globals?.Ol;
    if (olObjects is null) { return; }

    var applyMethod = olObjects
        .GetType()
        .GetMethod(
            "ApplyJunkFolderSelections",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            [typeof(string), typeof(string)],
            null
        );

    if (applyMethod is null)
    {
        logger.Warn(
            "Unable to persist junk-folder selections because the Outlook globals implementation does not expose ApplyJunkFolderSelections."
        );
        return;
    }

    applyMethod.Invoke(olObjects, [JunkEmail?.RelativePath, JunkPotential?.RelativePath]);
}
```

Resolved method: `TaskMaster.AppOlObjects.ApplyJunkFolderSelections(string junkCertainRelativePath, string junkPotentialRelativePath)`, declared at `TaskMaster\AppGlobals\AppOlObjects.JunkFolders.cs:36-45`. It is `internal void`, instance, two `string` parameters — an exact match for the `BindingFlags` and parameter-type array above.

**The method does exist with a matching signature, and the reflection lookup succeeds in production.** The `applyMethod is null` warn branch at `:409-415` is reached only by test doubles that do not declare the method (`NoApplyOlObjects` in `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperControllerTests.cs:214`). The issue text's "silently returns with only a warning when the method is not found" describes a real but production-unreachable branch; the substantive AC5 defect is the double persistence itself plus the untyped, rename-fragile binding.

Argument-order note: the invocation passes `JunkEmail?.RelativePath` first and `JunkPotential?.RelativePath` second, matching `(junkCertainRelativePath, junkPotentialRelativePath)`. `JunkEmail` on the controller mirrors `StoreWrapper.JunkCertain` (`StoreWrapperController.cs:290`, `:351`), so the order is semantically correct today — but it is enforced by nothing except positional agreement, which is precisely the fragility AC5 targets.

### C2. The two mechanisms, and how they diverge

**Mechanism 1 — per-store JSON.** `SaveChanges` (`StoreWrapperController.cs:348-357`) assigns `Current.JunkCertain = JunkEmail` (`:351`) and `Current.JunkPotential = JunkPotential` (`:352`), then `Model.Serialize()` (`:356`). These live on `StoreWrapper` (`UtilitiesCS\OutlookObjects\Store\StoreWrapper.cs:227-229`) as `FolderMinimalWrapper`s whose `RelativePath` is computed relative to **the selected store's** root: `SelectFolder` builds `new FolderMinimalWrapper(folder, Current.RootFolder!)` at `StoreWrapperController.cs:429`, and `FolderMinimalWrapper.ToRelativePath()` strips `OlRoot.FolderPath` (`UtilitiesCS\OutlookObjects\Folder\FolderMinimalWrapper.cs:84`).

**Mechanism 2 — .NET user settings.** `ApplyJunkFolderSelections` (`AppOlObjects.JunkFolders.cs:36-45`) writes `Properties.Settings.Default.OlJunkCertain` and `.JunkPotential` (writers at `:27-34`), calls `Properties.Settings.Default.Save()` (`:43`), then `RefreshJunkFolderSelections()` (`:44`), which nulls `_junkCertain`/`_junkPotential` and re-reads them (`:47-53`). The read side, `LoadJunkCertain` (`:106-140`) and `LoadJunkPotential` (`:55-89`), resolves the stored relative path against `new OutlookFolderNode(Root)` (`:69`, `:120`) where `Root` is the **default store's** root folder (`TaskMaster\AppGlobals\AppOlObjects.cs:206-214`, `App.Session.DefaultStore.GetRootFolder()`).

Concrete divergence paths, all currently live:

1. **Different roots.** Mechanism 1 stores a path relative to the *selected* store's root; mechanism 2 resolves it against the *default* store's root. Selecting junk folders while a non-default store is chosen in the dialog writes a settings value that `LoadJunkCertain` will fail to resolve (or, worse, resolve to a same-named folder in the wrong store), while the JSON side records the correct value.
2. **Store scope.** `Properties.Settings` holds exactly one global pair, but the JSON holds one pair *per store*. Saving on store B unconditionally overwrites the settings written for store A. `PersistJunkFolderSelections` is called on **every** `SaveChanges`, including saves that changed only the archive root or the exclude-store checkbox (`AnyChanges`, `:236-243`).
3. **Asymmetric failure today.** Because of root cause 1, mechanism 1 writes nothing at all while mechanism 2 writes successfully. The two stores of truth are therefore already divergent on every machine that has never had `StoresWrapper.json`.
4. **Null propagation.** `applyMethod.Invoke(..., [JunkEmail?.RelativePath, JunkPotential?.RelativePath])` passes `null` when either wrapper or its `RelativePath` is null; `WriteJunkCertainSetting(null)` stores `null`, and `LoadJunkCertain` then early-returns `null` at `:109-112`. The JSON side, by contrast, retains the previous `FolderMinimalWrapper`.

### C3. The typed seam that replaces the reflection call

**Project reference direction (verified):** `TaskMaster\TaskMaster.csproj:520` declares `<ProjectReference Include="..\UtilitiesCS\UtilitiesCS.csproj">`. `UtilitiesCS\UtilitiesCS.csproj` declares exactly one `ProjectReference`, to `..\SVGControl\SVGControl.csproj` (`:1118-1121`). The dependency is one-way, `TaskMaster -> UtilitiesCS`, and **must stay that way**. (`UtilitiesCS\Interfaces\IGlobals\IApplicationGlobals.cs:2` carries `using TaskMaster;`, which resolves because `UtilitiesCS\Interfaces\IGlobals\IAppEvents.cs:6` declares `namespace TaskMaster` inside `UtilitiesCS` itself — it is not a reference to the `TaskMaster` project.)

**Recommended seam: a new narrow interface in `UtilitiesCS`, implemented by `TaskMaster.AppOlObjects`.**

- Interface: `IJunkFolderSelectionSink` (name for the planner to confirm), one member `void ApplyJunkFolderSelections(string junkCertainRelativePath, string junkPotentialRelativePath)`.
- Project and file: `UtilitiesCS\Interfaces\IGlobals\IJunkFolderSelectionSink.cs`, alongside the existing `IStoreDisableService.cs` and `IStoreRehookService.cs` in the same folder — a directly analogous precedent (a `UtilitiesCS`-declared service interface implemented in `TaskMaster` and consumed from `UtilitiesCS`).
- Implementer: `TaskMaster.AppOlObjects`, via the junk partial `TaskMaster\AppGlobals\AppOlObjects.JunkFolders.cs`. The existing method at `:36-45` must be promoted from `internal` to `public` to satisfy the implicit interface implementation (C# requires public implicit implementations), or declared explicitly as `void IJunkFolderSelectionSink.ApplyJunkFolderSelections(...)` to keep the surface closed. **Explicit implementation is preferred** so the public surface of `AppOlObjects` does not widen.
- Consumption: `StoreWrapperController.PersistJunkFolderSelections` becomes `if (Globals?.Ol is IJunkFolderSelectionSink sink) { sink.ApplyJunkFolderSelections(...); } else { logger.Warn(...); }` — a compile-checked call with no `System.Reflection` dependency (the `using System.Reflection;` at `StoreWrapperController.cs:6` can then be removed if no other use remains).
- **Direction respected:** the interface lives in `UtilitiesCS`, the implementation in `TaskMaster`. `UtilitiesCS` gains no reference to `TaskMaster`. This is identical in shape to `IStoreDisableService` (`UtilitiesCS\Interfaces\IGlobals\IStoreDisableService.cs:54-104`).

**Why not add the member to `IOlObjects`** (`UtilitiesCS\Interfaces\IGlobals\IOlObjects.cs:11-38`): every concrete implementer would have to be updated, including the test stub `OlObjectsStubBase : IOlObjects` at `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperControllerTests.cs:139-194` and the `IApplicationGlobals` stubs in `QuickFiler.Test` (`EfcHomeControllerTests.cs:202`, `EfcHomeControllerMetricsTests.cs:462`, `EfcHomeControllerLifecycleTests.cs:388`). A separate opt-in interface keeps the change local and preserves the existing `NoApplyOlObjects` negative test (`StoreWrapperControllerTests.cs:214`) unchanged in intent.

AC5 also permits removing the double-persistence path entirely. That is the larger behavioural change (it would strand `LoadJunkCertain`/`LoadJunkPotential`, which are the only consumers of the settings values and are read by `IOlObjects.JunkCertain`/`JunkPotential`, `IOlObjects.cs:34-35`). The typed seam plus a loud failure is the lower-risk reading of AC5 and is what this research recommends.

---

## D. SMTP lookup (AC6)

### D1. Full call chain to the placeholder

1. `UtilitiesCS\OutlookObjects\Store\StoresWrapper.cs:49` — `Stores = filteredStores.Select(store => new StoreWrapper(store).Init()).ToList();` (fresh build), or `:151` / `:157` inside `AddOrRestoreStore` (rewire path; `Restore` also calls `Init()` at `StoreWrapper.cs:120`).
2. `UtilitiesCS\OutlookObjects\Store\StoreWrapper.cs:82-86` — `UserEmailAddress = GetSmtpAddressFromStore();`, once per `Init`, inside the `CurrentStoreContext.Begin(DisplayName)` scope opened at `:62`.
3. `UtilitiesCS\OutlookObjects\Store\StoreWrapper.cs:179-217` — `GetSmtpAddressFromStore()`. The chain is `RootFolder?.Session?.CurrentUser` (`:184`) -> `currentUser?.AddressEntry` (`:190`) -> `addressEntry?.GetExchangeUser()` (`:196`) -> `exchangeUser?.PrimarySmtpAddress` (`:202`). A `COMException` anywhere is caught at `:209`, logged at `:211-214`, and converted to `return null;` at `:215`. The observed failure threw at `:184`, matching the log's `StoreWrapper.cs:line 184`.
4. `UtilitiesCS\OutlookObjects\Store\StoreWrapper.cs:173-174` — `[JsonIgnore] public string? UserEmailAddress { get; internal set; }`. `JsonIgnore` means it is not persisted, so a successful lookup is not cached across restarts.
5. `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs:296` — the placeholder render:

```csharp
Viewer.UserEmail.Text = Current?.UserEmailAddress ?? "Error Loading";
```

**Exact literal:** `"Error Loading"`. It occurs at exactly three sites, all in `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs`: `:294` (Inbox), `:295` (Root Folder), `:296` (User Email). There are no other occurrences of the literal in any `.cs` file in the repository.

### D2. Alternative sources for the mailbox SMTP address already reachable from the types in scope

| # | Source | Location | Notes |
|---|---|---|---|
| 1 | `StoreWrapper.DisplayName` | `UtilitiesCS\OutlookObjects\Store\StoreWrapper.cs:153`, assigned at `:38` from `InnerStore.DisplayName` | Already read before the failing chain, at no extra COM cost. In the reported environment it *is* the SMTP address (`dmoisan@realgoodfoods.com`, per the `[store-filter]` log line). Requires an `@`-containing sanity test before use. |
| 2 | `IOlObjects.UserEmailAddress` | interface `UtilitiesCS\Interfaces\IGlobals\IOlObjects.cs:17`; implementation `TaskMaster\AppGlobals\AppOlObjects.cs:347-357` (cached in `_userEmailAddress` at `:343`) | Reachable from the controller as `Globals?.Ol?.UserEmailAddress`. This is the **application-level** address (default store), so it is a correct fallback only for the default store; for a secondary store it is a different mailbox. |
| 3 | `AppOlObjects.ResolveCurrentUserEmailAddress` | `TaskMaster\AppGlobals\AppOlObjects.cs:359-382` | Backs source 2. Notably it already does the UI-thread marshalling (`UiThread.UiSyncContext.Send`, `:364-369`) and catches `COMException` (`:377-381`). |
| 4 | `AppOlObjects.TryGetSmtpAddress(AddressEntry)` | `TaskMaster\AppGlobals\AppOlObjects.cs:384-412` | **The existing in-repo precedent for AC6's fallback ordering**: try `GetExchangeUser()?.PrimarySmtpAddress` inside its own `try/catch (COMException)` (`:391-399`), then fall back to `addressEntry.Address` when it contains `"@"` inside a second `try/catch` (`:401-409`), then `null`. `StoreWrapper.GetSmtpAddressFromStore` has a single outer catch and no `Address` fallback — the direct gap AC6 names. Note this helper lives in `TaskMaster`, not `UtilitiesCS`, so `StoreWrapper` cannot call it; the shape should be replicated inside `GetSmtpAddressFromStore`. |
| 5 | `StoreWrapper.GlobalAddressBook` | `UtilitiesCS\OutlookObjects\Store\StoreWrapper.cs:176-177`, populated only by `RestoreGlobalAddresses(Application)` at `:141-147` | **Dead.** A repository-wide search finds no caller of `RestoreGlobalAddresses` outside its own declaration. Not a usable fallback without new wiring. |
| 6 | Existing consumers that already tolerate a null per-store address | `TaskVisualization\AutoCreateProject.cs:136-137` (`Stores.FirstOrDefault(x => !x.UserEmailAddress.IsNullOrEmpty())?.UserEmailAddress`) | Shows an established cross-store "first non-empty" pattern that could be reused as a last-resort fallback in the dialog. |

Not available: `Outlook.Account` / `Session.Accounts` is **not read anywhere in the repository** (searched across all `.cs`). Using it would be new COM surface, and `StoreWrapper` holds no `Application` reference (only `InnerStore`, `:165`), so it is not reachable from the types in scope without a new parameter. Recorded as unverified/unavailable rather than recommended.

Recommended fallback order for AC6, all inside `GetSmtpAddressFromStore` except the last: (1) `PrimarySmtpAddress`; (2) `AddressEntry.Address` when it contains `@` (mirroring `TryGetSmtpAddress`); (3) `DisplayName` when it contains `@`; and at the controller, (4) a specific unavailability message carrying the caught exception's `Message`, replacing `"Error Loading"`.

### D3. Where a retry-on-dialog-open is wired

`StoreWrapperController.Launch()` (`UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs:117-137`) assigns `Viewer.DisplayName.DataSource = readiness.DisplayNames;` at `:134`. `StoreWrapperViewer`'s constructor wires `DisplayName.SelectedValueChanged += DisplayName_SelectedValueChanged;` (`UtilitiesCS\OutlookObjects\Store\StoreWrapperViewer.cs:14`), so setting the data source raises `DisplayName_SelectedValueChanged` (`StoreWrapperController.cs:153-171`), which sets `Current` at `:169` and calls `PopulateWithCurrent()` at `:170` — **before** `Viewer.ShowDialog()` at `:136`.

Therefore **`PopulateWithCurrent()` (`:279-314`) is the single method that runs both when the dialog opens and on every store re-selection.** It is the correct retry site.

**Thread:** yes, the UI thread. `Launch` is `[ExcludeFromCodeCoverage]` and reached from the ribbon click (`RibbonViewer.FolderSettings_Click -> RibbonController.FolderStoresSettings -> StoreWrapperController.Launch`, confirmed by the stack at log line 2682). `PopulateWithCurrent` itself opens with an `if (Viewer.InvokeRequired) { Viewer.Invoke(() => PopulateWithCurrent()); return; }` marshal at `:281-285`, so its body always executes on the UI (STA) thread.

### D4. The blocking hazard, and whether a reusable seam exists

**Hazard, stated precisely.** `_ExchangeUser.get_PrimarySmtpAddress()` is an Outlook interop call that can block the STA for many seconds. The evidence in `debug_2026-09-06.log` (lines 2887-2892, 7814, 8005, 8291, 9108, 13039, 14842) shows the UI thread parked inside it repeatedly — but via `RecipientStatic.GetRecipientAddress` (`UtilitiesCS\OutlookObjects\Recipient\RecipientStatic.cs:458`) in the QuickFiler mail-load path, **not** via `StoreWrapper.GetSmtpAddressFromStore`. Adding a retry at `PopulateWithCurrent` therefore reintroduces a blocking call on the UI thread at dialog-open time, on a chain independently demonstrated to be capable of long blocks.

**Existing seams, evaluated:**

- `UtilitiesCS\Threading\TimeOutTask.cs` — the only timeout primitive, `public static class TimeOutTask` at `:13` with `RunWithTimeout` overloads at `:21`, `:97`, `:165`, `:250`, `:324`, `:402`, `:480`, `:551`, `:633`, `:715` (plus private recursive companions). Every overload executes the work via `Task.Run(...)` on a **ThreadPool (MTA)** thread (for example `:63`). **Not suitable for Outlook COM.** An Outlook interop object is STA-apartment-bound; calling it from an MTA thread marshals the call back to the STA, so the STA still blocks and the caller's timeout merely abandons the wait while the worker remains blocked. Verified property of the code, not speculation about Outlook: `AppOlObjects.ResolveCurrentUserEmailAddress` documents precisely this constraint at `TaskMaster\AppGlobals\AppOlObjects.cs:361-369` ("Outlook COM objects must be accessed from the STA thread on which they were created... marshal synchronously to the UI thread to avoid COMException 0xEF640201").
- `UtilitiesCS\Threading\UiThread.cs` — `UiSyncContext` (`:113-126`), `UiThreadId` (`:128-133`), `Dispatcher` (`:153-171`). These marshal work **onto** the UI thread; they do not move COM work off it.
- `UtilitiesCS\Threading\ThreadMonitor.cs` plus `CurrentStoreContext` (`UtilitiesCS\Threading\CurrentStoreContext.cs`, already wrapped around this exact chain at `StoreWrapper.cs:62-87`) — observational attribution only.

**Conclusion for D4:** no existing seam in `UtilitiesCS` makes an Outlook COM property read genuinely non-blocking. The honest options for AC6 are (a) do the retry synchronously on the UI thread and accept the same latency the current startup path already incurs, bounding the risk by attempting the retry at most once per dialog open and only when `UserEmailAddress` is null; or (b) treat a non-blocking SMTP read as a separate, larger piece of work and file it. Option (a) is what AC6 as written requires; option (b) should be recorded as a potential follow-up, not folded in.

---

## E. Remaining acceptance criteria

### E1. AC7 — the leading `\\` store prefix

The two producing reads are `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs:294-295`:

```csharp
Viewer.Inbox.Text = Current?.Inbox?.FolderPath ?? "Error Loading";
Viewer.RootFolder.Text = Current?.RootFolder?.FolderPath ?? "Error Loading";
```

`Current.Inbox` and `Current.RootFolder` are `Outlook.Folder?` (`UtilitiesCS\OutlookObjects\Store\StoreWrapper.cs:167-171`) assigned from live COM at `StoreWrapper.cs:65` (`GetRootFolder()`) and `:74-76` (`GetDefaultFolder(olFolderInbox)`). `MAPIFolder.FolderPath` is Outlook's native `\\<store>\<folder>` form; there is no transformation between the COM read and the label.

**No dedicated trim helper exists in `UtilitiesCS`.** The three closest existing behaviours are:

- `UtilitiesCS\OutlookObjects\Folder\FolderNavigator.cs:16-19` — `if (FolderPath.StartsWith(@"\\")) { FolderPath = FolderPath.Substring(2); }`, inside a navigation method, not reusable as a helper.
- `UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailFilerConfig.cs:258` — `folderPath.TrimStart('\\')` as the out-of-ancestor fallback of `GetStem`.
- `UtilitiesCS\OutlookObjects\Folder\ArchiveStemContract.cs` — `IsFullOutlookPath` (`:41-56`) and `TryMakeArchiveRelative` (`:106-145`). These are **root-relative** operations, not store-prefix trims; `TryMakeArchiveRelative` returns false for a path that is not under the supplied root and never passes the input through (`:129-141`), so it cannot be used as a plain display trim.

Recommendation: add a small pure private static helper in the `StoreWrapperController` display partial (see F/G) rather than extending `ArchiveStemContract`, which is a shared filing-boundary contract with its own test suite (`UtilitiesCS.Test\OutlookObjects\Folder\ArchiveStemContractTests.cs`) and is consumed by the breadcrumb/EFC work items. Keeping the trim local respects the "do not broaden the shared trees" constraint.

Note the existing assertion `StoreWrapperControllerTests`/`StoreWrapperViewerTests` do not pin the `\\` form for Inbox or Root Folder, so AC7 adds coverage rather than changing an existing expectation.

### E2. AC8 — null `Current`

**Assignment that can produce null:** `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs:168-169`

```csharp
var displayName = Viewer.DisplayName.SelectedValue?.ToString();
Current = Model.Stores!.Find(store => store.DisplayName == displayName);
```

`List<T>.Find` returns `null` when no element matches (for example when `SelectedValue` is null or when a store's `DisplayName` read failed and left it null at `StoreWrapper.cs:38`). `Current` is declared `public StoreWrapper Current { get; internal set; } = null!;` at `:89`, so the compiler does not flag the assignment.

**Unguarded dereference:** `:288-291`, the first four statements of `PopulateWithCurrent` after the `InvokeRequired` marshal:

```csharp
ArchiveOutlook = Current.ArchiveRoot;
ArchiveFS = Current.ArchiveFsRoot;
JunkEmail = Current.JunkCertain;
JunkPotential = Current.JunkPotential;
```

These use `Current.` (no `?.`), while the very next block at `:294-296` uses `Current?.` — an internal inconsistency inside one method. A null `Current` therefore throws `NullReferenceException` at `:288` before any placeholder can be rendered. `GetRelativeFsPath` at `:459-460` has the same unguarded `Current.ArchiveFsRoot` dereference and is called from `:298`.

**Placeholder text that should render instead:** the existing literals already in the method — `"Error Loading"` for Inbox / Root Folder / User Email (`:294-296`), `"Please select an archive"` for Archive Outlook and Archive FS (`:297`, `:466`, `:473`), `"Please select a folder"` for Junk Email and Junk Potential (`:311-312`). Under AC6 the User Email literal becomes the new specific unavailability message; the other five are unchanged.

**Existing test that must be updated (call this out explicitly in the plan).** `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperController_Tests.ButtonAndPopulate.cs:123-135`, `PopulateWithCurrent_NullCurrent_SetsErrorLoadingText`, currently asserts `act.Should().Throw<NullReferenceException>();` — the test **name** describes the AC8 behaviour but the **assertion** codifies the bug. Per the General Code Change Policy (existing tests are part of the spec), this deliberate inversion must be named in the change description.

### E3. The single-ampersand in `GetRelativeFsPath`

`UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs:464`:

```csharp
if (specialFolder.IsNullOrEmpty() & relativePath.IsNullOrEmpty())
```

**What the short-circuit was for:** nothing operative. Both operands call the extension `UtilitiesCS\Extensions\StringExtensions.cs:15`, `public static bool IsNullOrEmpty(this string? str) => string.IsNullOrEmpty(str);`. An extension method on a null receiver does not throw — the receiver is passed as an ordinary argument — and `string.IsNullOrEmpty` accepts null. Neither operand has a side effect.

**Can the non-short-circuit form throw?** No. Verified: `IsNullOrEmpty` is null-tolerant, both operands are pure, and the tuple is produced by `FsConverter(...)` at `:463` whose default implementation `FilePathHelperConverter.GetSerializablePath` (`UtilitiesCS\NewtonsoftHelpers\FilePathHelperConverter.cs:166-195`) always returns two non-null strings (`name` is `"Not Found"` when nothing matches, `:178`).

So `&` versus `&&` is **behaviourally inert here** — it is a readability/consistency defect, not a latent crash. Fixing it is safe and costless; the change description should not claim it repairs a fault. (Secondary observation, recorded but out of scope: because `GetSerializablePath` never returns an empty `name`, the `:464` condition is effectively unreachable in production, so `:466` is dead. That is a separate finding, not part of AC1-AC8.)

---

## F. Testability and test layout

### F1. Injectable file-system and path seams (no temporary files permitted)

The repository prohibits creating temporary files in tests (`.claude/rules/general-unit-test.md`, "External Dependencies"). **Adequate seams already exist**; no new seam is required for the serializer work.

| Seam | Type | Location | Use |
|---|---|---|---|
| `ReadAllText` | `protected Func<string, string>` | `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializable.cs:43-48` (base sibling `SmartSerializableBase.cs:25-30`) | supply JSON in memory |
| `DiskExists` | `protected Func<FilePathHelper, bool>` | `SmartSerializable.cs:50-55` (base `SmartSerializableBase.cs:32-37`) | simulate file present/absent |
| `CreateStreamWriter` | `protected Func<string, StreamWriter>` | `SmartSerializable.cs:467-472` (base `SmartSerializableBase.cs:444-449`) | capture a write into a `MemoryStream` instead of disk |
| `ShowDialog` | `protected Func<string,string,MessageBoxButtons,MessageBoxIcon,DialogResult>` | `SmartSerializable.cs:57-64` (base `SmartSerializableBase.cs:39-46`) | suppress the `askUserOnError` dialog |
| `TimerFactory` | `protected Func<TimeSpan, ITimerWrapper>` | `SmartSerializable.cs:547-548` (base `SmartSerializableBase.cs:529-530`) | fire the 3-second deferred write deterministically |
| `FilePathHelper.Exists()` | `public virtual bool` | `UtilitiesCS\HelperClasses\FileSystem\FilePathHelper.cs:151-166` | mockable via Moq |
| `IFileSystemFolderPaths.SpecialFolders` | interface | `UtilitiesCS\Interfaces\IGlobals\IFileSystemFolderPaths.cs` | supply a fake `AppData` root for path construction |

All five `protected` seams are already exposed to tests by the established harness `SmartSerializableHarness : SmartSerializable<TestSmartItem>` at `UtilitiesCS.Test\ReusableTypeClasses\SmartSerializable_Tests.cs:771-806` (setters at `:792-806`), with a matching harness in `SmartSerializableBase_Tests.cs`. The deterministic timer double is `UtilitiesCS.Test\TestHelpers\ManualFireTimerWrapper.cs:19` (`ManualFireTimerWrapper : ITimerWrapper`), used with a `FireElapsed()` call at `SmartSerializable_Tests.cs:596-613`. `StopPrivateTimer` helpers exist at `SmartSerializable_Tests.cs:759` and `SmartSerializableBase_Tests.cs:583` to avoid leaking real timers.

For AC4 specifically: `SerializeThreadSafe` writes through `CreateStreamWriter` (`SmartSerializable.cs:484`), so a synchronous-flush test can assert the write occurred by injecting a `MemoryStream`-backed writer and checking a signal — the exact pattern already used at `SmartSerializable_Tests.cs:598-613`. No disk write, no temp file.

For AC2 the logger is not injectable; see B1 option (a), the `MemoryAppender` pattern proven at `TaskMaster.Test\AppGlobals\ApplicationGlobalsStartupTimingTests.cs:202-218`.

For AC1 the fresh-build path needs no filesystem at all: `TestableAppOlObjects` already injects a stub `ISmartSerializableNonTyped` and a canned fresh wrapper, and counts `BuildFreshStoresWrapperInvocationCount` (`TaskMaster.Test\AppGlobals\AppOlObjectsCoverageTests.cs:78-143`).

**No new seam is required.** The one genuine gap is the AC2 logger, and the `MemoryAppender` route closes it without adding production surface.

### F2. Correct test project and directory per production type, and existing coverage

| Production type | File | Test project + directory | Existing test files |
|---|---|---|---|
| `SmartSerializable<T>` | `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializable.cs` | `UtilitiesCS.Test\ReusableTypeClasses\` | `SmartSerializable_Tests.cs` (896 lines) |
| `SmartSerializableBase` | `...\SmartSerializableBase.cs` | `UtilitiesCS.Test\ReusableTypeClasses\` | `SmartSerializableBase_Tests.cs` (726 lines) |
| `SmartSerializableNonTyped` | `...\SmartSerializableNonTyped.cs` | `UtilitiesCS.Test\ReusableTypeClasses\` | `SmartSerializableNonTyped_Tests.cs` (149) |
| `SmartSerializableLoader` | `...\SmartSerializableLoader.cs` | `UtilitiesCS.Test\ReusableTypeClasses\` | `SmartSerializableLoader_Tests.cs` (180) |
| `NewSmartSerializableConfig` | `...\Config\NewSmartSerializableConfig.cs` | `UtilitiesCS.Test\ReusableTypeClasses\` | `NewSmartSerializableConfig_Tests.cs` (395) |
| `StoreWrapperController` | `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs` | `UtilitiesCS.Test\OutlookObjects\Store\` | `StoreWrapperController_Tests.cs` (182), `.ButtonAndPopulate.cs` (396), `.ExcludeStore.cs` (164), `.Launch.cs` (480), `StoreWrapperControllerTests.cs` (216) |
| `StoreWrapper` | `UtilitiesCS\OutlookObjects\Store\StoreWrapper.cs` | `UtilitiesCS.Test\OutlookObjects\Store\` | `StoreWrapperTests.cs` (285) — already covers `GetSmtpAddressFromStore` null and `COMException` paths at `:73-118` |
| `StoresWrapper` | `UtilitiesCS\OutlookObjects\Store\StoresWrapper.cs` | `UtilitiesCS.Test\OutlookObjects\Store\` | `StoresWrapperTests.cs` (431), `.StoreIdExclusion.cs` (222), `StoresWrapperDisableTests.cs` (369), `StoresWrapperRehookTests.cs` (94) |
| `StoreWrapperViewer` | `UtilitiesCS\OutlookObjects\Store\StoreWrapperViewer.cs` | `UtilitiesCS.Test\OutlookObjects\Store\` | `StoreWrapperViewerTests.cs` (167) |
| `AppOlObjects` (StoreLoading partial) | `TaskMaster\AppGlobals\AppOlObjects.StoreLoading.cs` | `TaskMaster.Test\AppGlobals\` | `AppOlObjectsCoverageTests.cs` (347), `AppOlObjectsTests.cs` (438) |
| `AppOlObjects` (JunkFolders partial) | `TaskMaster\AppGlobals\AppOlObjects.JunkFolders.cs` | `TaskMaster.Test\AppGlobals\` | `AppOlObjectsTests.cs` (`LoadJunkCertain` tests around `:140-178`) |
| `FilePathHelper` | `UtilitiesCS\HelperClasses\FileSystem\FilePathHelper.cs` | `UtilitiesCS.Test\HelperClasses\` | present in the project; not modified by this fix |

**File-size constraint — a hard planning input.** The 500-line cap in `.claude/rules/general-code-change.md` is already exceeded by two files in scope and nearly exceeded by two more:

| File | Lines | Consequence |
|---|---|---|
| `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializable.cs` | **613** | already over cap; AC2 adds lines. Either accept a documented pre-existing overage (no new violation *class*), or split the `#region Serialization` (`:440-561`) into a `partial` sibling. A split is a shared-tree refactor and conflicts with the "no broad refactor" constraint — recommend **accepting the pre-existing overage and adding the minimum lines**, and recording it. |
| `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializableBase.cs` | **545** | already over cap. **Do not edit** under this issue. |
| `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs` | **478** | 22 lines of headroom. AC5, AC6, AC7 and AC8 all land here and will exceed 500. A **partial split is required**: move `PopulateWithCurrent`, `BindExcludeStoreCheckbox`, `GetRelativeFsPath` and the new display helpers into `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.Display.cs`. The `AppOlObjects.*.cs` partials are the in-repo precedent. |
| `TaskMaster\AppGlobals\AppOlObjects.cs` | **493** | 7 lines of headroom. **Do not add to it**; the AC5 explicit interface implementation belongs in `AppOlObjects.JunkFolders.cs` (186 lines). |
| `TaskMaster\AppGlobals\AppOlObjects.StoreLoading.cs` | **75** | ample headroom for AC1. |
| `UtilitiesCS\OutlookObjects\Store\StoreWrapper.cs` | **233** | ample headroom for AC6. |
| `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperController_Tests.Launch.cs` | **480** | near cap; new controller tests should go to a new partial file, not here. |
| `UtilitiesCS.Test\ReusableTypeClasses\SmartSerializable_Tests.cs` | **896** | already over cap; new serializer tests should go to a new file. |

### F3. Non-SDK-style projects — every `.csproj` needing a hand-added `Compile Include`

Verified: **every** project in the solution is non-SDK-style (`<Project ToolsVersion="15.0"` or `"17.0"` with `xmlns="http://schemas.microsoft.com/developer/msbuild/2003"` and a terminal `<Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />`). No `Sdk=` attribute appears in any `.csproj`. A new `.cs` file is **not** picked up by a wildcard; it must be added by hand.

| New file would live in | `.csproj` that needs the `Compile Include` entry | Existing entries to insert beside |
|---|---|---|
| `UtilitiesCS\OutlookObjects\Store\` (e.g. `StoreWrapperController.Display.cs`) | `UtilitiesCS\UtilitiesCS.csproj` | store-folder entries near `:920-1035`; interface entries at `:1024-1035` |
| `UtilitiesCS\Interfaces\IGlobals\` (e.g. `IJunkFolderSelectionSink.cs`) | `UtilitiesCS\UtilitiesCS.csproj` | `:1029-1030` (`IStoreDisableService.cs`, `IStoreRehookService.cs`) |
| `TaskMaster\AppGlobals\` (only if a new partial is added; not currently needed) | `TaskMaster\TaskMaster.csproj` | `:417-422` (`AppOlObjects.*.cs` block) |
| `UtilitiesCS.Test\OutlookObjects\Store\` (new controller/wrapper test partials) | `UtilitiesCS.Test\UtilitiesCS.Test.csproj` | `:373-396` and `:526-529` |
| `UtilitiesCS.Test\ReusableTypeClasses\` (new serializer tests) | `UtilitiesCS.Test\UtilitiesCS.Test.csproj` | `:466-470` |
| `TaskMaster.Test\AppGlobals\` (new AC1 tests, if not appended to existing files) | `TaskMaster.Test\TaskMaster.Test.csproj` | `:289-309` |

Note also that `TaskMaster\TaskMaster.csproj` needs **no** edit if AC1 is implemented entirely inside the already-registered `AppOlObjects.StoreLoading.cs` (`:421`) and the AC5 implementation inside the already-registered `AppOlObjects.JunkFolders.cs` (`:420`).

### F4. Mocking approach and mockability of the types in scope

Required stack (CLAUDE.md, `CUT1`/`CUT2`): **MSTest** (`[TestClass]`/`[TestMethod]`), **Moq**, **FluentAssertions**.

| Type | Mockable as-is? | Evidence |
|---|---|---|
| `IApplicationGlobals`, `IOlObjects`, `IStoreWrapperViewer`, `ISmartSerializableNonTyped` | Yes — interfaces, mocked throughout | `StoreWrapperController_Tests.ButtonAndPopulate.cs:213-226`; `AppOlObjectsCoverageTests.cs:37-47` |
| `StoreWrapper` | Constructible without COM: `new StoreWrapper(null)` | `StoreWrapperController_Tests.ButtonAndPopulate.cs:33`, `:50`, `:84`, `:141`, `:186` |
| `StoresWrapper` | `Mock<StoresWrapper>` works (public parameterless ctor, non-sealed); `Init()` and `RewireAfterDeserializeAsync()` are `virtual` (`StoresWrapper.cs:37`, `:68`) | `StoreWrapperController_Tests.ButtonAndPopulate.cs:31` |
| `SmartSerializable<T>.Serialize()` / `SerializeThreadSafe` | **Not virtual** (`SmartSerializable.cs:442`, `:474`) — Moq cannot intercept. Use the `CreateStreamWriter` seam instead. | `SmartSerializable_Tests.cs:598-613` |
| `StoreWrapperController` | Concrete, but every method under test is `internal`/`public` and `SelectFolder` is `internal virtual` (`:420`); `InternalsVisibleTo` is in effect for `UtilitiesCS.Test` | `StoreWrapperController_Tests.*` throughout |
| `StoreWrapperViewer` | Real WinForms viewer is constructible in tests without creating a window handle; `InvokeRequired` then returns false | `StoreWrapperController_Tests.ButtonAndPopulate.cs:170-177` (with the documented rationale) |
| `AppOlObjects` | Subclassable — `BuildFreshStoresWrapper` and `AwaitStoreRewireAsync` are `protected internal virtual` (`AppOlObjects.StoreLoading.cs:27`, `:32`) | `TestableAppOlObjects` in `TaskMaster.Test\AppGlobals\AppOlObjectsCoverageTests.cs` |
| `Outlook.Folder`, `Outlook.NameSpace`, `Outlook.ExchangeUser` | Mockable as COM interfaces with Moq | `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperTests.cs:120-134` (`CreateRootFolderWithPrimarySmtpAddress`) |

Two recorded hazards for the test author, both already documented in-repo: `Mock<T>` over `Task`-bearing interfaces can throw `TypeInitializationException` in this test binary because `System.Threading.Tasks.Extensions 4.2.0.1` is absent from the test output (`StoreWrapperController_Tests.ButtonAndPopulate.cs:170-175`); and `FolderMinimalWrapper`/`FilePathHelper` comparisons in `PairwiseEquals` (`StoreWrapperController.cs:266-277`) are reference-equality, which the mirroring test at `:167-204` depends on.

**No new mocking seam is required** for AC1, AC3, AC5, AC6, AC7 or AC8. AC2 needs the logger route from B1; AC4 needs the `CreateStreamWriter` route from F1.

---

## Numeric Derivation Evidence

The acceptance criteria AC1-AC8 contain no numeric assertions. This section supports the two enumerations stated above (A1 and A3) so a reviewer can verify them independently.

### Claim N1 — the number of members named exactly `Deserialize` declared on `SmartSerializableBase`

- **Complete Family:** all method declarations whose identifier is exactly `Deserialize` (any arity, any generic arity, any accessibility) declared directly in `class SmartSerializableBase`.
- **Exhaustive Search Scope:** the entire file `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializableBase.cs`, lines 1-545 (whole class body; the class is not `partial` — no `partial` modifier appears at `:19`, and no other file declares `SmartSerializableBase`).
- **Inclusion Rules:** identifier exactly `Deserialize`; declaration site (not a call site); any accessibility including `protected`.
- **Exclusion Rules:** `TryDeserialize`, `DeserializeAsync`, `DeserializeJson`, `DeserializeObject`; commented-out code; members of nested types.
- **Primary Search Strategy:** full sequential read of the file (lines 1-545) with manual identification of each declaration.
- **Primary Member Set:** `{ :126 Deserialize<T>(string,string), :132 Deserialize<T>(string,string,bool), :140 Deserialize<T>(string,string,bool,JsonSerializerSettings), :167 Deserialize<T,U>(SmartSerializable<U>), :190 Deserialize<T,U>(SmartSerializable<U>,bool,Func<T>?), :247 protected Deserialize<T>(FilePathHelper,bool,JsonSerializerSettings) }`
- **Primary Count:** 6
- **Cross-check Search Strategy or Query Expression:** ripgrep over the same file with `^\s*(public|protected|private|internal)[^;=]*\bDeserialize\w*\s*(<[^>]*>)?\s*\(`, which matches every accessibility-modified declaration line whose identifier begins with `Deserialize` at a word boundary (thereby also surfacing the `Async`/`Json`/`Object` variants for exclusion, and excluding `TryDeserialize` because no word boundary precedes `Deserialize` there).
- **Cross-check Member Set:** raw matches `{ :126, :132, :140, :167, :190, :247, :305, :314, :324, :335, :362, :382 }`; after applying the exclusion rules (removing `:305`, `:314`, `:324` `DeserializeAsync`; `:335`, `:382` `DeserializeJson`; `:362` `DeserializeObject`) the set is `{ :126, :132, :140, :167, :190, :247 }`.
- **Cross-check Count:** 6
- **Member-set Comparison:** the normalized primary set `{126,132,140,167,190,247}` and the normalized cross-check set `{126,132,140,167,190,247}` are identical. No member appears in one and not the other. Count agreement: 6 = 6.

### Claim N2 — the number of members named exactly `Deserialize` declared on `SmartSerializable<T>` (excluding the nested `Static` class)

- **Complete Family:** all method declarations whose identifier is exactly `Deserialize` declared directly in `class SmartSerializable<T>`, excluding the nested `public static class Static`.
- **Exhaustive Search Scope:** the entire file `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializable.cs`, lines 1-613. The type is not `partial` (`:23`) and no other file declares it.
- **Inclusion Rules / Exclusion Rules:** as N1, plus explicit exclusion of the seven forwarders inside `Static` (`:569`, `:572`, `:575`, `:582`, `:588`, `:592`, `:599`, region delimited by `:565` and `:609`).
- **Primary Search Strategy:** full sequential read of lines 1-613.
- **Primary Member Set:** `{ :177 Deserialize(string,string), :182 Deserialize(string,string,bool), :189 Deserialize(string,string,bool,JsonSerializerSettings), :214 Deserialize<U>(SmartSerializable<U>), :236 Deserialize<U>(ISmartSerializable<U>), :257 Deserialize<U>(SmartSerializable<U>,bool,Func<T>?), :312 protected Deserialize(FilePathHelper,bool,JsonSerializerSettings) }`
- **Primary Count:** 7
- **Cross-check Search Strategy or Query Expression:** ripgrep over the same file with the same declaration-line regex as N1, then partition by the `#region Static` boundary at `:563-611`.
- **Cross-check Member Set:** raw matches `{ :177, :182, :189, :214, :236, :257, :312, :364, :372, :378, :388, :410, :432, :569, :572, :575, :582, :588, :592, :599 }`; removing the `Async`/`Json`/`Object` variants (`:364, :372, :378, :388, :410, :432`) and the seven `Static` forwarders (`:569, :572, :575, :582, :588, :592, :599`) leaves `{ :177, :182, :189, :214, :236, :257, :312 }`.
- **Cross-check Count:** 7
- **Member-set Comparison:** the normalized primary set `{177,182,189,214,236,257,312}` and the normalized cross-check set `{177,182,189,214,236,257,312}` are identical. Count agreement: 7 = 7. Note the primary read additionally established the semantic property that only `:214` and `:236` skip the loader-config copy on the null path — a property the regex alone cannot establish, which is why the regex is used only as a completeness cross-check.

### Claim N3 — the number of occurrences of the literal `"Error Loading"` in production source

- **Complete Family:** every occurrence of the exact string literal `Error Loading` in any `.cs` file in the repository.
- **Exhaustive Search Scope:** all `.cs` files under the worktree root.
- **Inclusion Rules:** exact case-sensitive substring `Error Loading` inside a C# source file. **Exclusion Rules:** Markdown, XML, coverage artefacts, log files.
- **Primary Search Strategy:** ripgrep for the alternation `Error Loading|Please select an archive|Please select a folder` restricted to `--type cs`, then filtering to the first alternative.
- **Primary Member Set:** `{ StoreWrapperController.cs:294, StoreWrapperController.cs:295, StoreWrapperController.cs:296 }`
- **Primary Count:** 3
- **Cross-check Search Strategy or Query Expression:** independent full sequential read of `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs` (lines 1-478) plus inspection of the two placeholder-asserting test files `StoreWrapperViewerTests.cs` and `StoreWrapperController_Tests.ButtonAndPopulate.cs`, which reference the other two placeholder literals but never `Error Loading`.
- **Cross-check Member Set:** `{ StoreWrapperController.cs:294, :295, :296 }`; zero occurrences in any test file (`StoreWrapperViewerTests.cs:77-80` and `StoreWrapperController_Tests.ButtonAndPopulate.cs:89`, `:102` reference only the archive/folder placeholders).
- **Cross-check Count:** 3
- **Member-set Comparison:** the two normalized member sets are identical. Count agreement: 3 = 3. Consequence: replacing the User Email placeholder under AC6 touches exactly one line (`:296`) and breaks no existing assertion.

---

## G. Write set

### G1. Consolidated create/modify list

**Production — modify**

- `TaskMaster\AppGlobals\AppOlObjects.StoreLoading.cs` — AC1: apply the resolved loader configuration to the freshly built wrapper.
- `TaskMaster\AppGlobals\AppOlObjects.JunkFolders.cs` — AC5: explicit implementation of the new typed sink interface.
- `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializable.cs` — AC2 (error log on empty/null `FilePath`) and AC4 (guarded synchronous flush entry point). Already 613 lines; add the minimum.
- `UtilitiesCS\OutlookObjects\Store\StoreWrapper.cs` — AC6: fallback chain inside `GetSmtpAddressFromStore`, plus a retry-capable entry point and a captured failure reason.
- `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs` — AC4 (flush on Save), AC5 (typed seam call), AC8 (null-`Current` guard). Content must be moved out to the new display partial to stay under 500 lines.

**Production — create**

- `UtilitiesCS\Interfaces\IGlobals\IJunkFolderSelectionSink.cs` — AC5 typed seam.
- `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.Display.cs` — AC6/AC7/AC8 display logic relocated from `StoreWrapperController.cs`, plus the `\\`-trim helper.

**Tests — modify**

- `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperController_Tests.ButtonAndPopulate.cs` — AC8: invert `PopulateWithCurrent_NullCurrent_SetsErrorLoadingText` (`:123-135`) from asserting the throw to asserting the placeholders.
- `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperControllerTests.cs` — AC5: retarget the reflection-era `NoApplyOlObjects` / `RecordingOlObjects` doubles (`:196-214`) to the typed sink.
- `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperTests.cs` — AC6: SMTP fallback ordering cases.
- `TaskMaster.Test\AppGlobals\AppOlObjectsCoverageTests.cs` — AC1: assert the fresh wrapper adopts the loader's `Config.Disk.FilePath`.

**Tests — create**

- `UtilitiesCS.Test\ReusableTypeClasses\SmartSerializableSerializeGuardTests.cs` — AC2 and AC4 (`SmartSerializable_Tests.cs` is already 896 lines).
- `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperController_Tests.Display.cs` — AC6/AC7/AC8 (`.Launch.cs` is already 480 lines).

**Non-SDK-style `Compile Include` carriers — modify**

- `UtilitiesCS\UtilitiesCS.csproj` — entries for `Interfaces\IGlobals\IJunkFolderSelectionSink.cs` and `OutlookObjects\Store\StoreWrapperController.Display.cs`.
- `UtilitiesCS.Test\UtilitiesCS.Test.csproj` — entries for `ReusableTypeClasses\SmartSerializableSerializeGuardTests.cs` and `OutlookObjects\Store\StoreWrapperController_Tests.Display.cs`.
- `TaskMaster.Test\TaskMaster.Test.csproj` — entry only if a new test file is created there rather than appending to `AppOlObjectsCoverageTests.cs`.
- `TaskMaster\TaskMaster.csproj` — **no entry needed**, because AC1 and AC5 land in files already registered at `:420-421`.

**Feature documentation — modify**

- `docs\features\active\2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797\issue.md` — AC check-off only.

### G2. Files whose extension a downstream extractor may drop

One file in the write set carries an extension in the affected set (`.resx`, `.config`, `.props`, `.targets`) — and it is **read-only** for this change, listed here solely because the research references it:

- `UtilitiesCS\IntelligenceResources.resx` — spelled out in words: UtilitiesCS backslash IntelligenceResources dot **r-e-s-x**. It is the source of the `StoresWrapper.json` / `AppData` disk configuration described in A4. **It is not in the write set.** No modification to it is proposed, because the resource already carries the correct path; the defect is that the path is discarded downstream.

Three `.csproj` files **are** in the write set (`UtilitiesCS\UtilitiesCS.csproj`, `UtilitiesCS.Test\UtilitiesCS.Test.csproj`, and conditionally `TaskMaster.Test\TaskMaster.Test.csproj`). Spelled out in words, each ends in dot **c-s-p-r-o-j**. A downstream extractor that strips the extension would produce paths a human must repair by appending `.csproj`.

No `.config`, `.props` or `.targets` file is created or modified by this change.

### G3. Solution file and repository-root build property files

The fix does **not** require touching the solution file or any repository-root build property file. Stated in plain prose without backticks: the Visual Studio solution file named TaskMaster dot sln at the repository root is not modified, because every project that receives a new source file already exists in the solution and only its own project file changes. There is no Directory dot Build dot props file and no Directory dot Build dot targets file anywhere in this repository, so no such file is created, modified, or otherwise involved. No file at the repository root is written by this change.

### Constraints confirmed

- No edit is proposed under the dot claude tree, the dot codex tree, or the dot agents tree.
- No edit is proposed to either published JSON file under the config directory.
- No edit is proposed to any GitHub workflow file.
- The footprint stays inside: the serializer under the reusable type classes tree (one file, `SmartSerializable.cs`, minimum lines); the Outlook store wrapper and its controller; and the store-loading and junk-folder partials in the TaskMaster AppGlobals, plus one new narrow interface file in the already-established `UtilitiesCS\Interfaces\IGlobals` folder. No broad refactor of the reusable type classes tree is proposed; in particular `SmartSerializableBase.cs` (545 lines, over cap) is not edited at all.

---

## Testing implications (strategy only, no test code)

Per `.claude/rules/general-unit-test.md` and CLAUDE.md `CUT1`/`CUT2`: MSTest, Moq, FluentAssertions; Arrange-Act-Assert; no temporary files; no `Thread.Sleep`/`Task.Delay`/wall-clock waits.

- **AC1** — extend the existing `TestableAppOlObjects` harness: given a `SmartSerializableLoader` whose `Config.Disk.FilePath` is a fake `AppData` path and a `Deserialize` stub returning null, assert `sut.StoresWrapper.Config.Disk.FilePath` equals the loader's path after `LoadStoresAsync()`. Negative case: config key absent -> fresh build, path remains empty, AC2's error is logged on the subsequent save. No filesystem access.
- **AC2** — `MemoryAppender` attached to `typeof(SmartSerializable<TestSmartItem>).FullName`; call `Serialize()` with `Config.Disk.FilePath` set to `""` and, separately, to `null`; assert one `Error`-level event each and assert no timer was armed. Detach in a `finally`.
- **AC3** — manual verification only (restart of Outlook); record in the manual-verification evidence, not as an automated test.
- **AC4** — inject `ManualFireTimerWrapper` via `TimerFactory` and a `MemoryStream`-backed `CreateStreamWriter`; assert the explicit-save path writes **without** firing the timer, and that the pre-existing deferred path still requires a timer fire. Both assertions in the same file establish that other callers' behaviour is unchanged.
- **AC5** — a test double implementing the new sink interface records the two arguments and their order; a second double implementing only `IOlObjects` asserts the loud-failure branch. `PersistJunkFolderSelections_WhenApplyMethodIsMissing_DoesNotThrow` (`StoreWrapperControllerTests.cs:124-137`) is retargeted, not deleted.
- **AC6** — table-driven over the `Mock<Outlook.Folder>` chain already built by `CreateRootFolderWithPrimarySmtpAddress` (`StoreWrapperTests.cs:120-134`): PrimarySmtpAddress present; PrimarySmtpAddress throws and `Address` contains `@`; both fail and `DisplayName` contains `@`; all fail -> specific message containing the exception reason. Plus a controller test asserting the retry runs on a second `PopulateWithCurrent` when `UserEmailAddress` is null and does **not** run when it is already populated.
- **AC7** — pure-function tests over the new trim helper (leading `\\` present, absent, single `\`, empty, null), plus one `PopulateWithCurrent` test asserting the rendered `Inbox`/`RootFolder` label text.
- **AC8** — the inverted existing test plus a `GetRelativeFsPath` null-`Current` test, both asserting placeholders rather than a throw.

Coverage: every changed member is reachable through an existing seam, so the `>= 90%` new-code target and the no-regression-on-changed-lines rule are attainable without an `ExcludeFromCodeCoverage` attribute anywhere in this change.
