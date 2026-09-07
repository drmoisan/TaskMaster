# Research record — issue #798

Timestamp: 2026-09-07T02-01
Branch `bug/quickfiler-crash-column-add-timeout-swallowed-keynotfound-798` at base commit `c431dc32`. Runtime corroboration from `TaskMaster/bin/Debug/logs/debug_2026-09-06.log`. All line numbers read in this worktree.

Provenance note: produced by the `task-researcher` delegation for issue #798 and persisted by the orchestrator, unaltered except for HTML-entity decoding and the removal of backticks from paths named in negative-polarity ("not edited") sentences, so blast-radius extraction cannot read them as write claims.

## A. Causal chain — confirmation and re-citation

File line counts (repo caps production files at 500):

| File | Lines | Over 500? |
|---|---|---|
| `UtilitiesCS/Extensions/DfDeedle.cs` | 410 | no (90 headroom) |
| UtilitiesCS/Threading/TimeOutTask.cs | 1011 | **yes, pre-existing** |
| UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs | 296 | no |
| UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs | 474 | no (26 headroom) |
| `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` | 154 | no |
| `QuickFiler/Controllers/QfcDatamodel.cs` | 483 | no (17 headroom) |
| QuickFiler/Controllers/QfcHomeController.cs | 496 | no (4 headroom) |
| `TaskMaster/Ribbon/RibbonViewer.cs` | 388 | no |
| TaskMaster/Ribbon/RibbonController.cs | 270 | no |

Test files cited: `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` = 882 (**over cap, pre-existing**); UtilitiesCS.Test/Extensions/DfDeedle_Tests.cs = 316; UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs = 217; TaskMaster.Test/Ribbon/EngineGatedCommandRunnerTests.cs = 346.

**Step 1 — CONFIRMED.** `UtilitiesCS/Extensions/DfDeedle.cs:134-201`; call at `:164` = `await AddQfcColumnsAsync(table, currentFolder!, token, 0);`. Both citations exact.

**Step 2 — CONFIRMED, one citation CORRECTED.** `DfDeedle.cs:318-343` exact. The `TimeoutAfter` citation is **wrong**: the issue cites TimeOutTask.cs:924-940, which is `TimeoutAfter(this Task, int, int repeatAttempts)`. The call at `DfDeedle.cs:327` binds to `TimeoutAfter(this Task, int, TimeProvider? = null)` at **TimeOutTask.cs:949-1009** (binding argument in section B). Recursion at `:333`/`:340` while `counter < 2` gives attempts at counter 0,1,2; on the third, `2 < 2` is false, the `catch` body is empty, the method returns normally. 3 x 3000 ms = 9000 ms. Log lines 14100 to 14101: `19:21:50,529` to `19:21:59,614` = 9.085 s, with **no log line of any kind in between**, confirming silence (the bound overload has no logging). Overlap claim CONFIRMED: the overload never cancels or awaits the source task, so each retry starts a *new* `Task.Run` against the same COM `Table`.

**Step 3 — CONFIRMED.** `DfDeedle.cs:296-316`; adds at `:310-312`, removes at `:313-315`. Log line 14102 records `columnCount=5`.

**Step 4 — CONFIRMED, citations CORRECTED.** `EtlAsync` = OlTableExtensions.Etl.cs:66-130 (exact), `columnDictionary` built at `:84`. `GetColumnDictionary(this Outlook.Table)` = OlTableExtensions.cs:200-237, not `:200-230` (the issue truncates the duplicate-key fallback, which runs to `:236`). Schema mapping at `:207-209` via `MAPIFields.SchemaToField` (UtilitiesCS/OutlookObjects/Fields/MAPIFields.cs:83-95).

**Step 5 — CONFIRMED, citation CORRECTED.** `Email2dToRecords` = `DfDeedle.cs:214-237`, not `:214-240`. Unchecked reads at `:226-230`, access order `EntryID`, `MessageClass`, `SentOn`, `ConversationId`, `Triage`. **Additional finding not in the chain:** the same five unchecked reads exist a second time in `GetEmailDataFromTable` at `DfDeedle.cs:120-124`, the synchronous path used by `GetEmailDataInView` (`:86-97`) and `QfcDatamodel.InitDf` (`QfcDatamodel.FrameBuilding.cs:15`). A fix applied only to `Email2dToRecords` leaves it unguarded.

**Step 6 — CONFIRMED, ordering in the issue Summary CORRECTED.** `throw e;` at `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs:108`, catch block `:102-109`. The Summary says the exception was "rethrown with `throw e`, wrapped by the timeout helper". The wrapping happens **first**: `DfDeedle.cs:186-190` runs the transform as `Task.Run(...).TimeoutAfter(1000, 2)`; on fault, `MarshalTaskResults` (TimeOutTask.cs:797-820) executes `proxy.TrySetException(source.Exception)` at `:805`, where `source.Exception` is an `AggregateException`. The awaited proxy therefore throws `AggregateException`, and `throw e;` merely resets that already-wrapped exception's stack. **This matters for AC4: `throw;` restores the stack but does not unwrap the `AggregateException`.**

**Step 7 — CONFIRMED, citations CORRECTED.** `QfcHomeController.LaunchAsync` = QuickFiler/Controllers/QfcHomeController.cs:58-84 (the issue says `:58-81`; the try/catch closes at `:81`, the method returns at `:83`), catches only `OperationCanceledException`. `QuickFiler_Click` = `TaskMaster/Ribbon/RibbonViewer.cs:148-151` (exact); the "`:153-159`" siblings are `QuickFilerHighConfidence_Click` `:153-156` and `SortEmail_Click` `:158-159`. `RibbonController.LoadQuickFilerAsync` `:112-125`, `LoadQuickFilerHighConfidenceAsync` `:133-146`, `SortEmailAsync` `:239-247` — none contains a `catch`.

**Step 7b — CONFIRMED.** `EnsureTriageColumnExists` = `DfDeedle.cs:345-390` (exact); `HasUserDefinedProperty` = `:392-408` (exact). A missing `Triage` property throws `InvalidOperationException` at `:307`, so the issue's rejection of that hypothesis stands.

## B. `TimeoutAfter`

Complete overload set — **4 declarations**, none accepting a `CancellationToken`, none cancelling or awaiting the wrapped task:

| # | Declaration | Lines | Cancels? | Wraps in `AggregateException`? |
|---|---|---|---|---|
| 1 | `TimeoutAfter<TResult>(Task<TResult>, int, int repeatAttempts)` | 824-849 | abandons (delegates to #2) | indirectly |
| 2 | `TimeoutAfter<TResult>(Task<TResult>, int, TimeProvider? = null)` | 862-922 | **abandons** | **yes**, `MarshalTaskResults` `:805` |
| 3 | `TimeoutAfter(Task, int, int repeatAttempts)` | 924-940 | abandons (delegates to #4) | indirectly |
| 4 | `TimeoutAfter(Task, int, TimeProvider? = null)` | 949-1009 | **abandons** | **yes**, same path |

**Binding:** `Task.Run(Action, CancellationToken)` returns non-generic `Task`, eliminating #1 and #2. Of the non-generic pair, #3 needs two explicit arguments after the receiver and only `3000` is supplied; #4 has a defaulted third parameter. **The call binds to #4, TimeOutTask.cs:949-1009**, whose body is:

```csharp
public static Task TimeoutAfter(this Task task, int millisecondsTimeout, TimeProvider? timeProvider = null)
{
    if (task.IsCompleted || (millisecondsTimeout == Timeout.Infinite)) { return task; }
    TaskCompletionSource<VoidTypeStruct> tcs = new TaskCompletionSource<VoidTypeStruct>();
    if (millisecondsTimeout == 0) { tcs.SetException(new TimeoutException()); return tcs.Task; }
    ITimer timer = (timeProvider ?? TimeProvider.System).CreateTimer(
        state => { var myTcs = (TaskCompletionSource<VoidTypeStruct>)state;
                   myTcs.TrySetException(new TimeoutException()); },
        tcs, TimeSpan.FromMilliseconds(millisecondsTimeout), Timeout.InfiniteTimeSpan);
    task.ContinueWith((antecedent, state) => {
            var tuple = (Tuple<ITimer, TaskCompletionSource<VoidTypeStruct>>)state;
            tuple.Item1.Dispose();
            MarshalTaskResults(antecedent, tuple.Item2);
        }, Tuple.Create(timer, tcs), CancellationToken.None,
        TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    return tcs.Task;
}
```

**Defect found in overloads #1 and #3.** Both wrap a non-awaited call: `try { result = task.TimeoutAfter(ms); } catch (TimeoutException) {...}` (`:832-847`, `:928-938`). `TimeoutAfter(ms)` returns a proxy that faults *later* and never throws synchronously, so the `catch` is unreachable and the retry never happens. This affects `DfDeedle.cs:190` and OlTableExtensions.Etl.cs:114, :158, :245, :259. Out of AC scope; recorded so the plan does not assume a retry that does not occur, and as a candidate follow-up issue.

**Does a cancelling overload already exist?** `RunWithTimeout<T1,T2,TResult>(Func<T1,T2,TResult>, ...)` (public `:324-343`, private `:345-400`) does link a timeout CTS and runs `Task.Run(() => function(arg1, arg2), combinedToken.Token)` at `:367`. It is **not** a correct AC1 fix: (1) on exhaustion it logs `logger.Warn` at `:387` and returns `default` at `:399` rather than throwing; (2) `Task.Run(..., token)` only suppresses *scheduling* — it cannot interrupt a synchronous COM call already running, and each retry at `:375` starts a *new* `Task.Run`, reproducing today's overlap; (3) it exposes no `TimeProvider`, so its timeout is real wall-clock (`RunWithTimeout` at `:324` omits the `timeoutSourceFactory` parameter that the `T1` arity has at `:172`).

**Recommended AC1 shape — no new API.** True cancellation of a blocking COM call is unavailable on .NET Framework; the only mechanism satisfying AC1's non-overlap clause is "the retry waits for it". Start the work exactly once (`var work = Task.Run(() => columnAdder(table, folder), token);`), then loop up to three times awaiting `work.TimeoutAfter(3000, timeProvider)` on **the same instance**. No second COM call is ever started, the 9000 ms budget is preserved, and after the third `TimeoutException` throw a descriptive exception naming the folder and the step. This reuses overload #4 unchanged and avoids editing TimeOutTask.cs, which at 1011 lines is already over the 500-line cap.

## C. `AddQfcColumns` family

Signatures, all `private static` on `public static partial class DfDeedle` (`DfDeedle.cs:25`):

```csharp
private static void AddQfcColumns(Table table, MAPIFolder folder)                     // :296
private static async Task AddQfcColumnsAsync(Table table, MAPIFolder folder,
    CancellationToken token, int counter)                                             // :318-323
private static bool EnsureTriageColumnExists(MAPIFolder folder)                       // :345
private static bool HasUserDefinedProperty(MAPIFolder folder, string propertyName)    // :392
```

**Every caller of `AddQfcColumnsAsync`** (exhaustive `*.cs` search): production `DfDeedle.cs:164` (the only one); self-recursion `:333`, `:340`; tests by reflection at `DfDeedle_COM_Tests.cs:495-499` (MethodInfo), `:511-515` and `:534-538` (invocations with a **four-element** `object[]`).

**Does it return normally after the third attempt?** Yes — `DfDeedle.cs:336-342`: the `catch (TimeoutException)` body is a single `if (!token.IsCancellationRequested && counter < 2)`. With `counter == 2` control falls out of the catch and the returned `Task` completes successfully. The caller at `:164` sees a completed await, no exception, no return value and no log line — indistinguishable from success — and proceeds to `table.EtlAsync(...)` at `:168`.

**Smallest seam satisfying design policy: make `AddQfcColumnsAsync` `internal` with two optional parameters.**

```csharp
internal static async Task AddQfcColumnsAsync(
    Table table, MAPIFolder folder, CancellationToken token, int counter,
    Action<object, object>? columnAdder = null, TimeProvider? timeProvider = null)
```

- `internal` suffices: `UtilitiesCS/Properties/AssemblyInfo.cs:19` declares `[assembly: InternalsVisibleTo("UtilitiesCS.Test")]`.
- **`Action<object, object>` is mandatory, not stylistic.** The existing seams `TableEtlInvoker` (`:69-72`) and `StoreTableEtlInvoker` (`:81-84`) carry an in-source comment stating `object` is used "to avoid CS1769 (embedded interop types cannot be used as generic type arguments across assembly boundaries)". `Action<Table, MAPIFolder>` reproduces that error; ordinary interop-typed parameters are fine.
- **Optional parameters, not a fourth static seam.** `UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21` declares `[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]`, so a new class mutating shared statics would race `DfDeedle_COM_Tests`, which mutates `MessageBoxInvoker` in five tests (`:198, :223, :264, :318`). Optional parameters carry no cross-class state.
- **Cost:** the two reflection `Invoke` calls pass four arguments and will throw `TargetParameterCountException` against a six-parameter method (reflection does not apply C# defaults absent `Type.Missing` plus `BindingFlags.OptionalParamBinding`). Replace the reflection helper with direct `internal` calls, which also shrinks the already-882-line test file.

Pre-existing latent defect, out of scope: `DfDeedle_COM_Tests` mutates the shared static `MessageBoxInvoker` under class-level parallelization without `[DoNotParallelize]`.

## D. Where the AC3 check belongs

**`columnInfo` concrete type is `System.Collections.Generic.Dictionary<string, int>`** everywhere: produced by `GetColumnDictionary` (OlTableExtensions.cs:200, `dict` at `:217`, returned `:236`); returned from `EtlAsync` as `Task<(object[,] data, Dictionary<string,int> columnInfo)>` (OlTableExtensions.Etl.cs:66-73, `:129`); consumed by `Email2dArrayToDf` (`DfDeedle.cs:203-207`), `Email2dToRecords` (`:214-218`), `GetEmailDataFromTable` (`:108-112`).

**Required keys as the code actually uses them (5):** `"EntryID"`, `"MessageClass"`, `"SentOn"`, `"ConversationId"`, `"Triage"` — from `DfDeedle.cs:226-230` and the duplicate set at `:120-124`. Note the casing asymmetry (`EntryID` capital D, `ConversationId` lowercase d), sourced from `MAPIFields.SchemaToField` (MAPIFields.cs:83-95). A validator must compare ordinally, since the dictionary uses the default ordinal comparer (OlTableExtensions.cs:217).

**Is the folder name in scope at `Email2dToRecords`? No.** Neither it nor `Email2dArrayToDf` receives anything but `storeID`, `data` and `columnInfo`.

**Minimal fix.** `GetEmailDataInViewAsync` holds `currentFolder` at `DfDeedle.cs:153` and already reads `currentFolder?.Name` for the log at `:158`. Therefore:

1. Add a pure `internal static void ValidateRequiredEmailColumns(Dictionary<string,int> columnInfo, string folderName)` that throws naming every missing key and the folder.
2. Call it in `GetEmailDataInViewAsync` between `:177` and `:181`, passing `tableSnapshot.Item2` and a `string folderName` captured on the STA near `:158` — passing the *string*, not the COM `MAPIFolder`, into the later `Task.Run` lambda at `:186-189` avoids a cross-apartment property read.
3. Call it again in the synchronous `GetEmailDataInView` between `:94` and `:96` (`currentFolder` in scope at `:89`), closing the duplicate site at `:120-124`.

**Do not add a parameter to `Email2dToRecords`, `Email2dArrayToDf` or `GetEmailDataFromTable`:** all three are pinned by fixed-arity tests — UtilitiesCS.Test/Extensions/DfDeedle_Tests.cs:264-286 reflection-invokes `Email2dArrayToDf` with a three-element `object[]`; `DfDeedle_COM_Tests.cs:359` calls `GetEmailDataFromTable("store-X", data, columnInfo)` directly. Caller-side placement satisfies AC3's "or its caller" wording with zero breakage, and the validator is trivially testable from a plain dictionary.

`GetColumnDictionary` and `EtlAsync` are the wrong home: both are generic table utilities serving callers with legitimately different column sets (for example `ExtractData2` at OlTableExtensions.cs:245, `Store`-keyed branch at `:256`).

## E. AC4 — `throw e;` sites

```csharp
            catch (System.Exception e)                       // QfcDatamodel.FrameBuilding.cs:102-109
            {
                await ToggleOfflineMode(offline);
                logger.Error(
                    $"{nameof(DfDeedle.GetEmailDataInViewAsync)} Error. \n {e.Message}\n{e.StackTrace}"
                );
                throw e;
            }
```

Exhaustive `QuickFiler` `*.cs` search:

| File | Line | Method | Same partial family? |
|---|---|---|---|
| `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` | 108 | `GetEmailsInViewDfAsync` | yes — AC4 target |
| `QuickFiler/Controllers/QfcDatamodel.cs` | 359 | `LoadRemainingEmailsToQueueAsync` | **yes — identical sibling defect** |
| `QuickFiler/Controllers/QfcDatamodel.cs` | 400 | `LoadRemainingEmailsToQueue(BackgroundWorker, CancellationToken)` | **yes — identical sibling defect** |
| QuickFiler/Controllers/QfcQueue.cs | 71 | different type | no |
| QuickFiler/Helper Classes/cInfoMail.cs | 162 | commented out | no |

QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs contains none. **Recommendation:** fix all three in the `QfcDatamodel` family; leave QfcQueue.cs:71 alone.

**AC4 caveat:** `throw;` restores the stack but the ribbon still observes an `AggregateException`, because wrapping occurs at TimeOutTask.cs:805 inside `DfDeedle.cs:190`. AC5's dialog must render inner exceptions or the user sees only "One or more errors occurred."

## F. Ribbon boundary

```csharp
        public async void QuickFiler_Click(Office.IRibbonControl control)                // :148-151
        { await _controller.LoadQuickFilerAsync(); }

        public async void QuickFilerHighConfidence_Click(Office.IRibbonControl control)  // :153-156
        { await _controller.LoadQuickFilerHighConfidenceAsync(); }

        public async void SortEmail_Click(Office.IRibbonControl control) =>              // :158-159
            await _controller.SortEmailAsync();
```

**All 24 `async void` members in `TaskMaster/Ribbon/RibbonViewer.cs`.** In AC5 scope (3): `:148`, `:153`, `:158`. Already guarded, no change (1): `internal async void RunFolderFilterCallback()` `:289-309`, which wraps its await in try/catch, reports through an injected `Action<System.Exception>`, and defends the reporter with a nested catch. Deliberately out of scope (20): `BtnHideHeadersNoChildren_Click` `:105`, `BtnShowHeadersNoChildren_Click` `:110`, `BtnSplitToDoID_Click` `:120`, `UndoSort_Click` `:161`, `FindFolder_Click` `:164`, `TestClassifier_Click` `:217`, `TestClassifierVerbose_Click` `:220`, `GetConfusionDrivers_Click` `:223`, `ChartMetrics_Click` `:226`, `InvestigateErrors_Click` `:229`, `ScrapeAndMine_Click` `:236`, `BuildFolderClassifier_Click` `:239`, `BuildCategoryClassifier_Click` `:242`, `BuildActionableClassifier_Click` `:245`, `CompareFolders_Click` `:250`, `RebuildSubjectMap_Click` `:320`, `TokenizeEmail_Click` `:326`, `MineEmails_Click` `:329`, `BuildClassifier_Click` `:332`, `Intelligence_Click` `:348`. A further 20 live in the sibling partial TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs (`:126, :129, :135, :141, :169, :191, :197, :208, :227, :230, :236, :242, :252, :258, :261, :264, :279, :297, :303, :314`), also out of scope; TaskMaster/Ribbon/RibbonController.EngineCommands.cs:98-106 documents that their awaited tasks are contractually non-faulting.

**Established error mechanism.** The repository has **no** non-modal notice surface — RibbonController.EngineCommands.cs:151-157 states this and names `logger.Warn` plus `MessageBox.Show` as the established mechanism. Concrete examples:

```csharp
        private void NotifyEngineCommandNotReady(string message)   // RibbonController.EngineCommands.cs:158-162
        { logger.Warn(message); MessageBox.Show(message); }

        private static void ReportFolderFilterInitializationFailure(System.Exception exception)  // RibbonViewer.cs:311-315
        {
            logger.Error("Unable to initialize the folder-filter viewer.", exception);
            MessageBox.Show($"Unable to initialize the folder-filter viewer: {exception.Message}");
        }
```

There is no `MessageBoxInvoker`-style seam in `TaskMaster` (the one at `DfDeedle.cs:54-60` is `UtilitiesCS`-internal); `MyBox` is not used under `TaskMaster/Ribbon/`. Logger field: `private static readonly log4net.ILog logger` at `RibbonViewer.cs:60-62`; `logger.Error(string, Exception)` is the established shape.

**`RibbonViewer` type facts.** Not form-derived, not Designer-generated — `public partial class RibbonViewer : Office.IRibbonExtensibility` (`:33`), second partial at RibbonViewer.EngineCommands.cs:16, no `.Designer.cs`. It **does** carry `[ExcludeFromCodeCoverage]` (`:32`) with `[ComVisible(true)]` (`:31`). It **is** instantiated by an existing test: TaskMaster.Test/Ribbon/RibbonViewerEngineCallbackShapeTests.cs:172 runs `new RibbonViewer(new RibbonController())`; `TaskMaster.Test/TaskMaster.Test.csproj:345-348` project-references `TaskMaster`; TaskMaster/Properties/AssemblyInfo.cs:38 and TaskMaster/ThisAddIn.cs:14 grant `InternalsVisibleTo("TaskMaster.Test")`. It already has an internal delegate-taking test constructor at `:42-54`.

**Conclusion for AC5 testability.** Because the type is coverage-exempt and the handlers are `async void`, behaviour placed directly in them is neither measurable nor observable. The ratified pattern is issue #503's `EngineGatedCommandRunner` (TaskMaster/Ribbon/EngineGatedCommandRunner.cs, 139 lines), whose doc at `:26-31` states it is "deliberately NOT marked `[ExcludeFromCodeCoverage]`: it is host-neutral decision logic... Presentation of the notification is the injected sink's concern and lives in the coverage-exempt ribbon shim." Tested at TaskMaster.Test/Ribbon/EngineGatedCommandRunnerTests.cs (346 lines). AC5 should extract `TaskMaster/Ribbon/RibbonCommandBoundary.cs` — an `internal sealed class` with `internal async Task RunAsync(string commandName, Func<Task> action)` catching `System.Exception`, forwarding to injected `Action<string, System.Exception>` log and `Action<string>` presentation sinks, and never rethrowing. The three handlers become one-liners; `RibbonViewer` stays exempt. Add a reflection shape pin mirroring `RibbonViewerEngineCallbackShapeTests.AssertAwaitedAsyncVoidShape` (`:298-310`).

## G. Logging and timing (AC2)

**`[Df timing]` — `DfDeedle.cs`:** logger `:27-29`; helper `LogDfTiming(string phase, string? details = null)` `:41-48` (prefixes `[Df timing] `, appends `BuildDfTimingContext()` `:36-39` = `threadId=...; syncContext=...`, emits at `logger.Debug`); elapsed via `Stopwatch.StartNew()` at `:143`, `:167`, `:181`, reported as `elapsedMs={sw.ElapsedMilliseconds}`.

```csharp
            LogDfTiming(                                                          // DfDeedle.cs:174-177
                "GetEmailDataInViewAsync table snapshot ready | table snapshot",
                $"rowCount={tableSnapshot.Item1.GetLength(0)}; columnCount={tableSnapshot.Item1.GetLength(1)}; etlElapsedMs={etlStopwatch.ElapsedMilliseconds}"
            );
```

Rendered (log line 14103): `[Df timing] GetEmailDataInViewAsync table snapshot ready | table snapshot | threadId=1; syncContext=System.Windows.Forms.WindowsFormsSynchronizationContext | rowCount=225; columnCount=5; etlElapsedMs=59`

**`[Table timing]` — OlTableExtensions.cs:** logger `:25-27`; helper `LogTableTiming` `:39-46` via `BuildTableTimingContext()` `:34-37`, at `logger.Debug`; `Stopwatch.StartNew()` at OlTableExtensions.Etl.cs:30, `:77` and `:349` in the family.

```csharp
            LogTableTiming(                                                // OlTableExtensions.Etl.cs:125-128
                "EtlAsync complete | ETL over table snapshots",
                $"rowCount={rowCount}; columnCount={columnDictionary.Count}; elapsedMs={etlStopwatch.ElapsedMilliseconds}"
            );
```

Rendered (log line 14102): `[Table timing] EtlAsync complete | ETL over table snapshots | threadId=1; syncContext=System.Windows.Forms.WindowsFormsSynchronizationContext | rowCount=225; columnCount=5; elapsedMs=29`

**Exact AC2 call sites** (all in `DfDeedle.cs`, so `LogDfTiming` is the correct helper):

| Requirement | Site | Line |
|---|---|---|
| `HasUserDefinedProperty` invocation | from `EnsureTriageColumnExists` | `:352` |
| `HasUserDefinedProperty` body — the `folder.UserDefinedProperties` enumeration (leading slow-COM hypothesis) | `foreach (UserDefinedProperty property in folder.UserDefinedProperties)` | `:399` |
| `Columns.Add` x3 | `"SentOn"` / `Schemas.ConversationId` / `Schemas.Triage` | `:310`, `:311`, `:312` |
| `Columns.Remove` x3 | `"Subject"` / `"CreationTime"` / `"LastModificationTime"` | `:313`, `:314`, `:315` |

Time the enumeration inside `HasUserDefinedProperty` (`:392-408`) rather than the cheap guard, and each of the six `Columns` calls individually so a single slow column is attributable.

## H. Test infrastructure

| Project | Assembly under test (from `ProjectReference`) |
|---|---|
| `QuickFiler.Test` | `QuickFiler` `:470`, `UtilitiesCS` `:474`, `TaskVisualization` `:478` |
| SVGControl.Test | SVGControl |
| Tags.Test | Tags |
| `TaskMaster.Test` | `TaskMaster` `:345`, `ToDoModel` `:349`, `UtilitiesCS` `:353` |
| TaskTree.Test | TaskTree |
| TaskVisualization.Test | TaskVisualization |
| ToDoModel.Test | ToDoModel |
| `UtilitiesCS.Test` | `UtilitiesCS` `:916`, `TaskMaster` `:912` |
| VBFunctions.Test | VBFunctions |

`TaskMaster.Test` does **not** reference `QuickFiler`, so an AC5 test must not depend on QuickFiler types.

**All three needed projects are non-SDK-style:** `<Project ToolsVersion="15.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">` on line 2, `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` (`UtilitiesCS.Test/UtilitiesCS.Test.csproj:17`, `QuickFiler.Test/QuickFiler.Test.csproj:18`, `TaskMaster.Test/TaskMaster.Test.csproj:17`), explicit `<Compile Include>` items. Every new `.cs` file needs an entry. Patterns to match:

- `UtilitiesCS.Test/UtilitiesCS.Test.csproj:189` — `    <Compile Include="Extensions\DfDeedle_COM_Tests.cs" />`
- `TaskMaster.Test/TaskMaster.Test.csproj:323` — `    <Compile Include="Ribbon\EngineGatedCommandRunnerTests.cs" />`
- `QuickFiler.Test/QuickFiler.Test.csproj:146` — `    <Compile Include="Controllers\QfcDatamodelTests.cs" />`

Package versions (identical in `UtilitiesCS.Test/packages.config` and `TaskMaster.Test/packages.config`, all `targetFramework="net481"`): `MSTest.TestFramework` 4.4.0, `MSTest.TestAdapter` 4.4.0, `MSTest.Analyzers` 4.4.0, `Moq` 4.20.72, `FluentAssertions` 8.10.0, `Microsoft.Bcl.TimeProvider` 10.0.11, `Microsoft.Extensions.TimeProvider.Testing` 10.9.0.

`UtilitiesCS.Test` parallelizes at class level (`UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21`); new classes mutating process-wide state need `[DoNotParallelize]` (precedent UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs:10).

**Log assertion for AC2:** a `MemoryAppender` helper exists at TaskMaster.Test/AppGlobals/AppEventsTests.Helpers.cs:228-246 (attach and detach against `(Hierarchy)LogManager.GetRepository()`, `logger.Level = Level.Debug`). **No equivalent exists in `UtilitiesCS.Test`** (exhaustive search returned no files), so a new AC2 test there needs its own roughly 18-line helper. Neither UtilitiesCS/Properties/AssemblyInfo.cs nor TaskMaster.Test/Properties/AssemblyInfo.cs declares a log4net repository attribute, so the default repository is shared and cross-assembly capture should work — **this could not be verified by execution** (no build or test run was performed); treat it as a Phase-0 check, with the fallback of asserting AC2 indirectly through the injected column-adder.

## I. Deterministic time

**`TimeProvider` and `FakeTimeProvider` are already available to these `net481` projects — the issue's open question is resolved.**

- `UtilitiesCS.Test/packages.config:23` — `Microsoft.Bcl.TimeProvider` 10.0.11, net481.
- `UtilitiesCS.Test/packages.config:90-94` — `Microsoft.Extensions.TimeProvider.Testing` 10.9.0, net481.
- `TaskMaster.Test/packages.config:18`, `:85-88` — the same two packages at the same versions.
- UtilitiesCS/packages.config and TaskMaster/packages.config also carry `Microsoft.Bcl.TimeProvider`, so production use is supported.
- Production already uses it: TimeOutTask.cs:862-866 and `:949-953` accept `TimeProvider? = null` and call `(timeProvider ?? TimeProvider.System).CreateTimer(...)` at `:888` and `:975`; QuickFiler/Controllers/QfcHomeController.cs:51 sets `controller.TimeProvider = timeProvider ?? TimeProvider.System`.
- Tests already use it: UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs:5 imports `Microsoft.Extensions.Time.Testing`, `:20` declares `FrozenClock()`, and `:27, :42, :57, :72, :88, :104` drive `TimeoutAfter` from it. Its doc at `:12-19` records that this exists to remove a wall-clock race.

Threading a `TimeProvider?` into `AddQfcColumnsAsync` plus the injected `Action<object,object>` column-adder drives the entire timeout path with **zero wall-clock wait and no COM object**. The ban is not test-only: BannedSymbols.txt:4-7 bans `Thread.Sleep(int)`, `Thread.Sleep(TimeSpan)`, `Task.Delay(int)` and `Task.Delay(TimeSpan)` with the message "Do not call Task.Delay directly in production code. Inject a time abstraction (System.TimeProvider)." A `Task.WhenAny(work, Task.Delay(...))` implementation of AC1 would fail the analyzer gate; re-applying `TimeoutAfter(ms, timeProvider)` to a single held `Task` is the only analyzer-clean, deterministically testable shape.

## J. Blast radius and sibling overlap

**Direct check:** exhaustive search of the DfDeedle, TimeOutTask and OlTableExtensions files in UtilitiesCS for `RelativePath`, `ArchiveRoot`, `OlRoot`, `FolderPath`, `ToArchiveRelative`, `Serializ` and `JsonConverter` returned **no matches**. None contains folder-path-projection or settings-serialization code; siblings 2 and 3 have no reason to touch them.

| Proposed file | Sibling overlap | Narrower alternative |
|---|---|---|
| `UtilitiesCS/Extensions/DfDeedle.cs` | No | n/a |
| `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` (new) | No | n/a |
| `UtilitiesCS/UtilitiesCS.csproj` | **Yes, likely** — sibling 2 may add or move UtilitiesCS files, producing conflicting `<Compile Include>` insertions | Insert adjacent to the existing `Extensions\DfDeedle.cs` entry, not at the end of the `ItemGroup` |
| UtilitiesCS/Threading/TimeOutTask.cs | No, and no edit proposed | n/a |
| The OlTableExtensions partial files in UtilitiesCS | No, and no edit proposed | n/a |
| `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` | Low (sibling 1 is viewer and selector work) | change is one token; trivially re-applicable |
| `QuickFiler/Controllers/QfcDatamodel.cs` | Low | same |
| `TaskMaster/Ribbon/RibbonViewer.cs` | Low (no sibling is ribbon work) | confine to three handler bodies plus one field and one sink; no reordering or reformatting |
| `TaskMaster/Ribbon/RibbonCommandBoundary.cs` (new) | No | n/a |
| `TaskMaster/TaskMaster.csproj` | Low | same adjacency guidance |
| `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` | Low | restrict to the two reflection `Invoke` argument arrays |

Scope narrowing already applied: the TimeOutTask and OlTableExtensions files are excluded from the write set.

## K. Proposed write set

**Production (6):**

1. `UtilitiesCS/Extensions/DfDeedle.cs` — modify: relocate the four column methods to (2); add two `ValidateRequiredEmailColumns` calls (after `:177`; between `:94` and `:96`); capture `string folderName` on the STA near `:158`. Net line-count reduction.
2. `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` — create: partial holding the relocated methods with AC1 (a single held `Task`, three `TimeoutAfter(3000, timeProvider)` deadlines, and a descriptive throw naming folder and step) and AC2 timing, plus the pure AC3 validator.
3. `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` — modify: `:108` `throw e;` becomes `throw;`.
4. `QuickFiler/Controllers/QfcDatamodel.cs` — modify: `:359` and `:400` `throw e;` become `throw;`.
5. `TaskMaster/Ribbon/RibbonCommandBoundary.cs` — create: host-neutral `internal sealed class`, `RunAsync(string, Func<Task>)`, injected log and presentation sinks, **not** `[ExcludeFromCodeCoverage]`.
6. `TaskMaster/Ribbon/RibbonViewer.cs` — modify: route `:148`, `:153`, `:158` through the boundary; add the field and a `ReportRibbonCommandFailure` sink modelled on `:311-315`.

`production_file_count = 6`

**Test (5):**

1. `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` — create; AC1 and AC2 with `FakeTimeProvider` plus a never-returning injected adder; assert exactly one adder invocation across all three deadlines (non-overlap), the descriptive throw, and the `[Df timing]` lines. Carries `[DoNotParallelize]`.
2. `UtilitiesCS.Test/Extensions/DfDeedleRequiredColumnValidationTests.cs` — create; AC3: five negative cases (one per key), one positive, one message-content assertion.
3. `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` — modify; update the invocations at `:511-515` and `:534-538` (preferably replacing the reflection helper at `:495-499` with direct `internal` calls, shrinking the file).
4. `TaskMaster.Test/Ribbon/RibbonCommandBoundaryTests.cs` — create; AC5 pass-through, log-once, present-once, no-propagate, throwing-sink containment, plus the reflection shape pin.
5. `QuickFiler.Test/Controllers/QfcDatamodelRethrowTests.cs` — create; AC4: `FormatterServices.GetUninitializedObject`, set `_globals` with `Ol.NamespaceMAPI.Offline == true` so `ToggleOfflineMode` (`QfcDatamodel.FrameBuilding.cs:34-46`) short-circuits without touching `CommandBars`, set `Token` and `TokenSource` via the public setters (`QfcDatamodel.cs:158-170`), reflection-invoke `GetEmailsInViewDfAsync`, assert the observed `StackTrace` still contains the originating frame.

`test_file_count = 5`

**Project compile-entry files (5):** `UtilitiesCS/UtilitiesCS.csproj`, `TaskMaster/TaskMaster.csproj`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (2 entries), `TaskMaster.Test/TaskMaster.Test.csproj`, `QuickFiler.Test/QuickFiler.Test.csproj`.

Total diff surface: 16 paths.

## Numeric Derivation Evidence

**N1 — `TimeoutAfter` has exactly 4 overloads.**

- *Complete Family:* every method declaration named `TimeoutAfter` in the repository, all arities, generic and non-generic, all accessibilities.
- *Exhaustive Search Scope:* the whole worktree for the identifier; then the TimeOutTask file in UtilitiesCS/Threading read from line 1 to 1011.
- *Inclusion Rules:* a method *declaration* named `TimeoutAfter`.
- *Exclusion Rules:* invocations, comments, `docs/**`.
- *Primary Search Strategy or Query Expression:* repo-wide regex `TimeoutAfter\(` over `*.cs` excluding `**/docs/**`, then manual declaration/invocation separation. Declaration hits: `:824, :862, :924, :949`. Invocation hits excluded: TimeOutTask.cs:834, :841, :930, :936; OlTableExtensions.Etl.cs:114, :158, :245, :259; OlTableExtensions.cs:165; `DfDeedle.cs:190, :327`; 22 hits under the UtilitiesCS.Test Threading folder.
- *Primary Member Set:* { `TimeoutAfter<TResult>(Task<TResult>,int,int)`@824, `TimeoutAfter<TResult>(Task<TResult>,int,TimeProvider?)`@862, `TimeoutAfter(Task,int,int)`@924, `TimeoutAfter(Task,int,TimeProvider?)`@949 }
- *Primary Count:* 4
- *Cross-check Search Strategy or Query Expression:* a different, file-scoped regex `TimeoutAfter` restricted to the TimeOutTask file, inspecting each hit's line prefix for the `public static` declaration form; corroborated by a full sequential read of lines 1-1011, which surfaces no further declaration in the `RunWithTimeout` regions `:19-794` or the internal helpers `:797-822`.
- *Cross-check Member Set:* { @824, @862, @924, @949 }
- *Cross-check Count:* 4
- *Member-set Comparison:* normalized by line and signature, {824,862,924,949} is identical to {824,862,924,949}. No asymmetric member. Counts agree at 4.

**N2 — the RibbonViewer file has 24 `async void` members: 3 in AC5 scope, 1 already guarded, 20 out of scope.**

- *Complete Family:* every method in the RibbonViewer file under TaskMaster/Ribbon returning `void` with the `async` modifier, at any accessibility.
- *Exhaustive Search Scope:* that file, lines 1-388, read in full.
- *Inclusion Rules:* declaration lines containing `async void` followed by an identifier, block-bodied or expression-bodied, any accessibility.
- *Exclusion Rules:* commented-out declarations; the sibling EngineCommands partial; `async Task` members.
- *Primary Search Strategy or Query Expression:* broad regex `async void` across `TaskMaster/Ribbon/`, filtered to the RibbonViewer file. Hits: 105, 110, 120, 148, 153, 158, 161, 164, 217, 220, 223, 226, 229, 236, 239, 242, 245, 250, 289, 320, 326, 329, 332, 348.
- *Primary Member Set:* the 24 named members listed in section F (18 pre-`:289` public handlers, `RunFolderFilterCallback@289`, and the 5 post-`:289` public handlers).
- *Primary Count:* 24
- *Cross-check Search Strategy or Query Expression:* a distinct accessibility-anchored regex `^\s*public\s+(static\s+)?async\s+void\s+\w+` scoped to that file alone, which by construction cannot match `internal` or `private`. It returned 23 hits (all of the above except 289). The one member present in the primary set and absent here, `RunFolderFilterCallback@289`, was independently confirmed by direct read as `internal async void RunFolderFilterCallback()`. 23 public plus 1 internal equals 24.
- *Cross-check Member Set:* the 23 public members plus `RunFolderFilterCallback@289`.
- *Cross-check Count:* 24
- *Member-set Comparison:* normalized by name and line, the sets are identical (24 members). The strategies differ only in what the accessibility anchor can express, and that single difference is closed by direct source reading. AC5's three named handlers (`:148`, `:153`, `:158`) are in both sets; `:289` is the guarded member; 24 minus 3 minus 1 equals 20 out of scope.

**N3 — write-set counts.**

- *Complete Family:* every repository file this diff creates or modifies.
- *Exhaustive Search Scope:* the section-K write set, partitioned into production `.cs`, test `.cs`, and project files.
- *Inclusion Rules:* a `.cs` file outside a `*.Test` directory is production; a `.cs` file inside a `*.Test` directory is test.
- *Exclusion Rules:* `.csproj` files (reported separately); `docs/` artifacts.
- *Primary Search Strategy or Query Expression:* direct enumeration of section K — production: `DfDeedle.cs`, `DfDeedle.QfcColumns.cs`, `QfcDatamodel.FrameBuilding.cs`, `QfcDatamodel.cs`, `RibbonCommandBoundary.cs`, `RibbonViewer.cs`; test: `DfDeedleQfcColumnTimeoutTests.cs`, `DfDeedleRequiredColumnValidationTests.cs`, `DfDeedle_COM_Tests.cs`, `RibbonCommandBoundaryTests.cs`, `QfcDatamodelRethrowTests.cs`.
- *Primary Member Set:* the 6 production and 5 test paths above.
- *Primary Count:* production 6, test 5.
- *Cross-check Search Strategy or Query Expression:* derive the sets independently from the acceptance criteria rather than from section K. AC1 plus AC2 gives `DfDeedle.QfcColumns.cs` (new) plus `DfDeedle.cs` = 2 production; AC3 gives a validator and call sites already counted = 0 new; AC4 gives `QfcDatamodel.FrameBuilding.cs` plus `QfcDatamodel.cs` = 2; AC5 gives `RibbonCommandBoundary.cs` plus `RibbonViewer.cs` = 2; AC6 is manual = 0. Total 6. Tests: AC1 and AC2 give 1 new plus the forced signature repair of `DfDeedle_COM_Tests.cs` = 2; AC3 gives 1; AC4 gives 1; AC5 gives 1; AC6 gives 0. Total 5.
- *Cross-check Member Set:* production { `DfDeedle.QfcColumns.cs`, `DfDeedle.cs`, `QfcDatamodel.FrameBuilding.cs`, `QfcDatamodel.cs`, `RibbonCommandBoundary.cs`, `RibbonViewer.cs` }; test { `DfDeedleQfcColumnTimeoutTests.cs`, `DfDeedle_COM_Tests.cs`, `DfDeedleRequiredColumnValidationTests.cs`, `QfcDatamodelRethrowTests.cs`, `RibbonCommandBoundaryTests.cs` }.
- *Cross-check Count:* production 6, test 5.
- *Member-set Comparison:* normalized by repository-relative path, the AC-derived and section-K sets are element-for-element identical in both partitions. `production_file_count = 6`, `test_file_count = 5`.

## Risks

1. **Highest risk — AC1 converts a silent degradation into a hard launch failure.** Today the crash occurs only when the missing column is later indexed; after AC1 the launch fails deterministically at the column-add step on any folder exceeding 9 s. If "T&E" is slow for a persistent environmental reason (a large `UserDefinedProperties` collection, a throttled connection), AC1 turns an intermittent crash into a reproducible inability to open QuickFiler there. AC6 accepts this, but only the AC2 timing logs make the difference diagnosable — **AC2 must land in the same change as AC1, not after it.**
2. `throw;` alone does not unwrap the `AggregateException` (wrapped upstream at TimeOutTask.cs:805); AC5's dialog must render inner exceptions.
3. The `AddQfcColumnsAsync` signature change breaks the two reflection tests at `DfDeedle_COM_Tests.cs:511-515` and `:534-538` — this must be an explicit plan task.
4. `UtilitiesCS.Test` parallelizes at class level; new classes touching shared statics need `[DoNotParallelize]`. The pre-existing unguarded `MessageBoxInvoker` mutation in `DfDeedle_COM_Tests` is a latent flake and a candidate follow-up issue.
5. Unverified: that a `MemoryAppender` attached from `UtilitiesCS.Test` captures `UtilitiesCS.DfDeedle` output. Evidence supports it (no log4net repository attribute in either assembly) but no build or test run was performed.

Also recorded as candidate follow-up issues, both out of AC1-AC6 scope: the unreachable `catch (TimeoutException)` in `TimeoutAfter` overloads `:824-849` and `:924-940` (their documented retry never executes, affecting `DfDeedle.cs:190` and four Etl call sites), and the duplicate unchecked column indexing at `DfDeedle.cs:120-124`.

## Orchestrator verification of this record

The orchestrator independently re-derived three load-bearing claims before accepting it:

- `UtilitiesCS.Test/packages.config:23` and `:91` confirm `Microsoft.Bcl.TimeProvider` 10.0.11 and `Microsoft.Extensions.TimeProvider.Testing`, both `net481`.
- The four `TimeoutAfter` declarations at `:824`, `:862`, `:924`, `:949` were confirmed by an independent declaration-anchored search.
- The `throw e;` inventory was confirmed: `QfcDatamodel.FrameBuilding.cs:108`, `QfcDatamodel.cs:359`, `QfcDatamodel.cs:400`, plus QfcQueue.cs:71 (a different type, out of scope) and a commented-out occurrence in cInfoMail.cs:162.
