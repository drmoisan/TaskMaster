# Root-cause research: UtilitiesCS.Test nondeterminism (Issue #811; supersedes #780, #803, #594)

- Timestamp: 2026-09-07T22-10
- Issue: #811
- Branch: `bug/utilitiescs-test-determinism-780-803-594-811` (based on `origin/main` at `04a54e681bd21e841e124c016df30672ee701b75`)
- Method: static reading of the worktree with Read/Grep/Glob only. No build, no test run, no git history query was possible in this session (the shell was unavailable). Every line number below is the CURRENT line number in the worktree as of this timestamp. Paths are repository-relative.

## Headline findings

1. The #803 maintainer correction is correct on every claim that is verifiable from the tree (claims 1 to 5 and 7 CONFIRMED; claim 6, the millisecond timings, is not verifiable from the tree and is recorded as PARTIALLY CONFIRMED because the code path is consistent with it).
2. The issue Summary's stated mechanism for the `DfDeedle` failure (static-seam pollution by `DfDeedle_COM_Tests`) is REFUTED: the three seams are restored in `finally` blocks, none of them is read on the failing path, and no other test class in the assembly's parallel phase reads them at all.
3. The real mechanism is a 250 ms wall-clock deadline (`250 ms x rowCount`, rowCount = 1 in the test) on two `Task.Run(...).TimeoutAfter(...)` hops inside `OlTableExtensions.EtlByRowAsync`, whose "retry" argument is inert, whose `TimeoutException` is swallowed by `EtlAsync`, and whose null result is then dereferenced at `DfDeedle.cs:186-189`.
4. The `TimeProvider` seam needed for a deterministic fix is ALREADY in place end-to-end: `Microsoft.Bcl.TimeProvider 10.0.11` is referenced by `UtilitiesCS` and `UtilitiesCS.Test`, `Microsoft.Extensions.TimeProvider.Testing 10.9.0` by `UtilitiesCS.Test`, `TimeOutTask.TimeoutAfter` already accepts a `TimeProvider?`, and `DfDeedle.AddQfcColumnsAsync` (issue #798) already threads one. No package change is required.
5. AC1's 500 ms window has no production caller and no legitimate purpose; deletion is the correct fix.
6. AC3's "two `Console.Out` races" is not the count of anything in the current tree: there are FOUR capture-and-assert sites, all four already serialized with `[DoNotParallelize]`. The remaining AC3 work is to replace that serialization stopgap with a `TextWriter` seam.

---

## Section 1 - Verification of the #803 correction

### Claim 1 - all three static seams are saved and restored in `finally` blocks

Verdict: CONFIRMED.

`UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`:

- `MessageBoxInvoker`: lines 197-211, 222-236, 261-281, 317-334. Each has the shape `var original = DfDeedle.MessageBoxInvoker; DfDeedle.MessageBoxInvoker = ...; try { ... } finally { DfDeedle.MessageBoxInvoker = original; }`.
- `TableEtlInvoker`: lines 399-414:
  ```csharp
  var originalEtl = DfDeedle.TableEtlInvoker;
  DfDeedle.TableEtlInvoker = _ => (injectedData, injectedColInfo);
  try { ... } finally { DfDeedle.TableEtlInvoker = originalEtl; }
  ```
- `StoreTableEtlInvoker`: lines 762-782 and 827-847, same shape.

Additional finding beyond the correction: the assembly runs at `ExecutionScope.ClassLevel` (`UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21`), so the tests inside `DfDeedle_COM_Tests` execute sequentially on one worker; there is no intra-class race between a writer and a reader of these seams.

### Claim 2 - none of the seams is reachable from the failing path

Verdict: CONFIRMED.

Readers of the three seams in production (`Grep TableEtlInvoker|StoreTableEtlInvoker|MessageBoxInvoker`, `*.cs`, whole repo):

| Seam | Read site | On the failing path? |
|---|---|---|
| `TableEtlInvoker` | `UtilitiesCS/Extensions/DfDeedle.cs:94` inside the SYNCHRONOUS `GetEmailDataInView(Explorer)` (lines 86-102) | No |
| `StoreTableEtlInvoker` | `UtilitiesCS/Extensions/DfDeedle.FrameUtilities.cs:143` inside `FromDefaultFolder(Store, ...)` | No |
| `MessageBoxInvoker` | `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs:27, 170, 194` inside `AddQfcColumns` / `EnsureTriageColumnExists` | Reachable in principle from `AddQfcColumnsAsync`, but only when the folder lacks the `Triage` user-defined property. The failing test builds the folder with `BuildFolderWithUdp("Triage")` (test line 434), so `HasUserDefinedProperty` returns `true` at `DfDeedle.QfcColumns.cs:165-168` and no dialog seam is invoked. |

The failing path `GetEmailDataInViewAsync` (`DfDeedle.cs:139-219`) calls `table.EtlAsync(...)` directly at line 180:

```csharp
var tableSnapshot = await table.EtlAsync(
    token,
    tokenSource,
    0,
    progress.Increment(2).SpawnChild(96)
);
```

Cross-class observability check (`Grep DfDeedle\.(FromDefaultFolder|GetEmailDataInView|AddQfcColumnsAsync)\(`): the only test callers are `DfDeedle_COM_Tests.cs` (lines 405, 507, 525, 547, 573, 602, 768, 833) and `DfDeedleQfcColumnTimeoutTests.cs:121`. The latter class is `[DoNotParallelize]` (its line 22). No other test class in the parallel phase can observe a swapped seam. The Summary's "static seam mutation ... races other classes" therefore describes a race that has no second participant.

### Claim 3 - `EtlAsync` sets `milliseconds = 250 * rowCount` and `EtlByRowAsync` awaits two `Task.Run(...).TimeoutAfter(250, 3)`

Verdict: CONFIRMED.

`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs:80-82`:

```csharp
var rowCount = table.GetRowCount();
int milliseconds = 250 * rowCount;
var attempts = 3;
```

The test's strict mock returns `GetRowCount() == 1` (`DfDeedle_COM_Tests.cs:442`), so `milliseconds == 250`.

Branch selection at lines 90-107: `MAPIFields.BinaryToStringFields` (`UtilitiesCS/OutlookObjects/Fields/MAPIFields.cs:121-127`) contains `"ConversationId"`, and the test's column dictionary contains `ConversationId` (test lines 453-460), so the `EtlByRowAsync` branch is taken (line 98), not the `GetArray` branch.

`EtlByRowAsync` (private, lines 229-263) awaits two timed hops:

```csharp
var rows = await Task.Run(() => table.CastToRowArray(progress?.SpawnChild(65)), token)
    .TimeoutAfter(timeout, attempts);                 // lines 244-245
...
var jagged = await Task.Run(
        () => rows.EtlByRow(objectConverters, binIndices, objFields, objIndices, progress?.SpawnChild()),
        token
    )
    .TimeoutAfter(timeout, attempts);                 // lines 248-259
```

Each hop is a separate 250 ms wall-clock deadline that starts when `TimeoutAfter` is called, i.e. before the thread pool has scheduled the `Task.Run` body.

### Claim 4 - the retry count is inert because `TimeoutAfter(int)` returns a faulted proxy rather than throwing

Verdict: CONFIRMED.

`UtilitiesCS/Threading/TimeOutTask.cs:824-849` (generic) and `924-940` (non-generic):

```csharp
public static Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, int millisecondsTimeout, int repeatAttempts)
{
    Task<TResult> result = null!;
    try
    {
        result = task.TimeoutAfter(millisecondsTimeout);
    }
    catch (TimeoutException)
    {
        logger.Warn($"Task timed out. {repeatAttempts} attempts remaining.");
        if (repeatAttempts > 0)
        {
            result = task.TimeoutAfter(millisecondsTimeout, repeatAttempts - 1);
        }
        ...
    }
    return result!;
}
```

`task.TimeoutAfter(millisecondsTimeout)` binds to the overload at lines 862-922 (`TimeoutAfter<TResult>(this Task<TResult>, int, TimeProvider? = null)`). That overload never throws: it returns `task` itself (line 873), or a proxy `tcs.Task` (line 921) that a `TimeProvider` timer later faults via `myTcs.TrySetException(new TimeoutException())` (line 895). Even the zero-timeout short-circuit (lines 880-885) faults the proxy with `tcs.SetException` rather than throwing. The `catch (TimeoutException)` at line 836 therefore cannot execute, `repeatAttempts` is never consulted, and the log line "attempts remaining" is unreachable.

The only tests of this overload (`UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs:191-215`) pass already-completed tasks, which hit the `task.IsCompleted` short-circuit, so the inert branch is also untested.

### Claim 5 - on expiry `EtlAsync` swallows the exception and returns `(data!, columnDictionary)` with `data` null; `DfDeedle.cs:186` dereferences it

Verdict: CONFIRMED.

`OlTableExtensions.Etl.cs:83` initialises `object[,]? data = null;`. Lines 117-123 and 129:

```csharp
catch (TimeoutException)
{
    logger.Error(
        $"{DateTime.Now.ToString("mm:ss.fff")} {nameof(ETL)} timed out {attempts} times with a timeout of {milliseconds} milliseconds. Canceling"
    );
    tokenSource.Cancel();
}
...
return (data!, columnDictionary);
```

When the proxy from either hop faults, the `await` inside `EtlByRowAsync` rethrows `TimeoutException`, it propagates out of `EtlByRowAsync` into the `catch` above, `data` remains `null`, and the null-forgiving `data!` places a null into the non-nullable tuple element `object[,] data`. `tokenSource.Cancel()` is called, but in the test the `tokenSource` is a fresh `new CancellationTokenSource()` (test line 480) unrelated to the `CancellationToken.None` passed as `token`, so nothing downstream observes the cancellation.

`UtilitiesCS/Extensions/DfDeedle.cs:186-189`:

```csharp
LogDfTiming(
    "GetEmailDataInViewAsync table snapshot ready | table snapshot",
    $"rowCount={tableSnapshot.Item1.GetLength(0)}; columnCount={tableSnapshot.Item1.GetLength(1)}; etlElapsedMs={etlStopwatch.ElapsedMilliseconds}"
);
```

The statement begins at line 186; the dereference `tableSnapshot.Item1.GetLength(0)` is on line 188. The CI stack trace's "line 186" is the sequence point of the multi-line invocation statement, consistent with this site.

Additional finding: in production, `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs:82-109` wraps the call in `catch (TaskCanceledException)` (returns null quietly) and `catch (System.Exception)` (logs and rethrows). Because the `NullReferenceException` at line 188 fires before the cancelled token is next consulted (line 204 `Task.Run(..., token)`), a real Outlook ETL timeout surfaces as an unattributed NRE rather than the intended cancellation. The defect is a production defect, not only a test defect.

### Claim 6 - timings of 99 ms and 96 ms on green runs versus 389 ms on the failing run against a 250 ms deadline

Verdict: PARTIALLY CONFIRMED (mechanism consistent; the numbers are not verifiable from this tree).

No TRX from the PR #802 run exists in the worktree. The only committed TRX records of this test failing are in `docs/features/archive/2026-06-08-ci-flaky-test-isolation-176/evidence/` (`baseline-full.trx:1143`, `full2.trx:4241`, `run1.trx:8751`); those record durations of 9.8 ms to 11.7 ms and a DIFFERENT failure, `System.TypeInitializationException: The type initializer for 'Moq.Async.AwaitableFactory' threw an exception ... Could not load file or assembly 'System.Threading.Tasks.Extensions, Version=4.2.0.1'` (a June 2026 assembly-binding fault, since resolved). They do not corroborate or contradict the 389 ms figure.

What the code does establish: the test path performs at least four thread-pool scheduling hops (`AddQfcColumnsAsync` `Task.Run` at `DfDeedle.QfcColumns.cs:119`, two `Task.Run` in `EtlByRowAsync`, and `Task.Run` at `DfDeedle.cs:204`) plus a progress `Timer` (`Etl.cs:364-373`), under 24 class-level workers and coverage instrumentation. A 250 ms budget for a hop that includes pool scheduling latency is a plausible thing to exceed under that load, and nothing else in the path has a deadline that small (next smallest is 1000 ms at `DfDeedle.cs:208`).

### Claim 7 - the #798 guard at `DfDeedle.cs:195` runs after the line-186 dereference

Verdict: CONFIRMED.

`DfDeedle.cs:191-195`:

```csharp
// Guards the row builder's unchecked column indexing. ...
ValidateRequiredEmailColumns(tableSnapshot.Item2, folderName);
```

It executes after lines 186-189, and it inspects `Item2` (the column map), never `Item1` (the data array). Even if it were hoisted above line 186 it would not guard this path.

---

## Section 2 - Root cause per acceptance criterion

### AC1 / issue #780 - `DictionaryExtensions.TryAddValuesAsync`

Current signature and full body, `UtilitiesCS/Extensions/DictionaryExtensions.cs:169-180` (file carries `#nullable enable` at line 15):

```csharp
public static async Task<bool> TryAddValuesAsync<TKey, TValue>(
    this ConcurrentDictionary<TKey, TValue> dictionary,
    TKey key,
    TValue value,
    CancellationToken token
)
{
    var linkedTS = CancellationTokenSource.CreateLinkedTokenSource(token);
    linkedTS.CancelAfter(500);

    return await Task.Run(() => dictionary.TryAddValues(key, value), linkedTS.Token);
}
```

The hard-coded window is `linkedTS.CancelAfter(500);` at line 177. Note also that `linkedTS` is never disposed, so every call leaks a timer until it fires.

Call sites (search: `TryAddValuesAsync` over `*.{cs,vb,xaml,ps1}`, whole repository, 3 hits):

| Hit | Kind |
|---|---|
| `UtilitiesCS/Extensions/DictionaryExtensions.cs:169` | definition |
| `UtilitiesCS.Test/Extensions/DictionaryExtensions_Tests.cs:237` | test method name |
| `UtilitiesCS.Test/Extensions/DictionaryExtensions_Tests.cs:244` | the only invocation: `await dictionary.TryAddValuesAsync("value", 2, CancellationToken.None)` |

There is NO production caller.

What the window actually does: the linked token is passed only to `Task.Run`, so it can cancel the work only BEFORE the pool starts it (a `Task.Run` body cannot be interrupted by its token once running). The body `TryAddValues` (lines 123-136) is a compare-and-swap loop over `ConcurrentDictionary.TryUpdate` that completes in one iteration absent contention and is bounded in practice. The window therefore measures thread-pool scheduling latency, not the operation, and converts latency above 500 ms into `TaskCanceledException`. That is exactly the failure recorded (`d__10\`2.MoveNext() ... line 179`, the `await` line).

Observations per caller if the window is removed versus made `TimeProvider`-driven:

- Removed: the only caller (the test) observes `true` and the updated value regardless of scheduling latency; a pre-cancelled `token` still yields `TaskCanceledException` from `Task.Run`, so caller-driven cancellation semantics are unchanged.
- `TimeProvider`-driven: identical for the test if it passes an un-advanced `FakeTimeProvider`; adds a parameter with no production consumer and preserves a deadline that has no semantic on a CAS loop.

Does the 500 ms window have a legitimate production purpose? No. There is no production consumer, the body cannot hang on I/O, and the token cannot interrupt the body anyway. It is purely defensive and is itself the defect.

Recommendation: DELETE the linked token source and `CancelAfter`; pass the caller's `token` straight to `Task.Run`:

```csharp
return await Task.Run(() => dictionary.TryAddValues(key, value), token);
```

This satisfies AC1's first disjunct ("no longer cancels on a fixed wall-clock window"). Tests: keep `TryAddValuesAsync_UpdatesExistingValue` (it becomes deterministic with no change); add one test that a pre-cancelled token yields `TaskCanceledException` (locks the surviving contract) and, if the planner wants a RED-first artifact for AC1, note that the existing test cannot be made to fail deterministically before the fix without a sleep (the failure is load-dependent), so the RED test for AC1 should instead assert the contract shape (for example, via a `CancellationToken` that is cancelled after the work has been observed to start, proving the outer token and not an internal timer governs cancellation).

### AC2 / issues #803, #594 item 1 - `DfDeedle.GetEmailDataInViewAsync` and `DfDeedle_COM_Tests`

Current body of `GetEmailDataInViewAsync`, `UtilitiesCS/Extensions/DfDeedle.cs:139-219` (file has `#nullable enable` at line 23), abridged to the load-bearing lines:

```csharp
139  public static async Task<Frame<int, string>> GetEmailDataInViewAsync(
140      Explorer activeExplorer, CancellationToken token, CancellationTokenSource tokenSource, ProgressTracker progress)
145  {
146      token.ThrowIfCancellationRequested();
156      Outlook.Table table = await activeExplorer.GetTableInViewAsync(token, 0);
158      var currentFolder = activeExplorer.CurrentFolder;
159      var storeID = activeExplorer.CurrentFolder.StoreID;
166      var folderName = currentFolder?.Name ?? "(unknown folder)";
176      await AddQfcColumnsAsync(table, currentFolder!, token, 0);
179      var etlStopwatch = Stopwatch.StartNew();
180      var tableSnapshot = await table.EtlAsync(token, tokenSource, 0, progress.Increment(2).SpawnChild(96));
186      LogDfTiming("GetEmailDataInViewAsync table snapshot ready | table snapshot",
188          $"rowCount={tableSnapshot.Item1.GetLength(0)}; columnCount={tableSnapshot.Item1.GetLength(1)}; ...");   // <-- dereference
195      ValidateRequiredEmailColumns(tableSnapshot.Item2, folderName);                                             // <-- #798 guard
204      Frame<int, string> df = await Task.Run(() => Email2dArrayToDf(storeID, tableSnapshot.Item1, tableSnapshot.Item2), token)
208          .TimeoutAfter(1000, 2);                                                                                  // <-- second inert-retry deadline
217      progress.Report(100);
218      return df;
219  }
```

`EtlAsync` / `EtlByRowAsync` bodies: quoted in Section 1, claims 3 and 5 (`OlTableExtensions.Etl.cs:66-130` and `229-263`). `TimeoutAfter` overloads: `TimeOutTask.cs:824-849` (`(Task<T>, int, int)`), `862-922` (`(Task<T>, int, TimeProvider?)`), `924-940` (`(Task, int, int)`), `949-1009` (`(Task, int, TimeProvider?)`).

`DfDeedle_COM_Tests` static-seam usage: Section 1, claim 1. The failing test itself (lines 422-489) touches no static seam.

Other wall-clock deadlines on the same test path, in execution order (all currently system-clock driven and all untouched by the test):

| Site | Deadline | Seam available today |
|---|---|---|
| `DfDeedle.cs:156` -> `OlTableExtensions.TableAccess.cs:31-118` `GetTableInViewAsync` | 2000 ms via `TimeOutTask.RunWithTimeout(view.GetTable, token, 2000, 1, false, timeoutSourceFactory)` (line 56-63), plus up to two outer retries (`counter < 2`, lines 79, 97) | `Func<int, CancellationTokenSource>? timeoutSourceFactory` (line 36); NOT passed by `DfDeedle.cs:156` |
| `DfDeedle.cs:176` -> `DfDeedle.QfcColumns.cs:102-156` `AddQfcColumnsAsync` | 3 x 3000 ms | `TimeProvider? timeProvider` (line 108); NOT passed by `DfDeedle.cs:176` |
| `DfDeedle.cs:180` -> `Etl.cs:244-245, 248-259` | 2 x 250 ms | none (int retry count only) |
| `DfDeedle.cs:204-208` | 1000 ms | none (int retry count only) |

The 250 ms pair is the one that tripped; the 1000 ms one is the same mechanism with four times the margin and would produce the same NRE-free but still nondeterministic `TimeoutException` if it ever expired (that proxy is awaited directly, so it throws rather than returning null).

MINIMAL production change set that makes the failing path deterministic with no timing tolerance:

1. `OlTableExtensions.Etl.cs` - `EtlAsync`: add a trailing optional parameter `TimeProvider? timeProvider = null`; replace `.TimeoutAfter(milliseconds, attempts)` at line 114 with `.TimeoutAfter(milliseconds, timeProvider)`; pass `timeProvider` into `EtlByRowAsync`. `EtlByRowAsync` (line 229): replace `int attempts` with `TimeProvider? timeProvider`; lines 245 and 259 become `.TimeoutAfter(timeout, timeProvider)`. Drop the now-unused `attempts` local (line 82) and correct the log text at line 120, which currently claims "timed out {attempts} times" for a retry that never happened (also an opportunity to remove the banned `DateTime.Now` on that line, which is at RS0030 suggestion severity). Behaviour with `timeProvider == null` is byte-for-byte the current behaviour, because the `(int, int)` overload already delegated to the `(int, TimeProvider? = null)` overload and its retry was inert.
2. `DfDeedle.cs` - `GetEmailDataInViewAsync`: add a trailing optional parameter `TimeProvider? timeProvider = null`; pass it to `AddQfcColumnsAsync(..., timeProvider: timeProvider)` at line 176, to `EtlAsync(..., timeProvider: timeProvider)` at line 180, and replace `.TimeoutAfter(1000, 2)` at line 208 with `.TimeoutAfter(1000, timeProvider)`.
3. `DfDeedle.cs` - insert, between line 185 and line 186, the AC2 guard:
   ```csharp
   if (tableSnapshot.data is null)
   {
       throw new InvalidOperationException(
           $"The table snapshot for folder '{folderName}' was not produced: the table ETL "
               + "timed out or was cancelled before returning any rows, so the email data "
               + "frame cannot be built for this folder.");
   }
   ```
   The file is under `#nullable enable`; a null test on a non-nullable tuple element compiles without a nullable diagnostic. `folderName` is already in scope (line 166). This mirrors the wording style of `ValidateRequiredEmailColumns` (`DfDeedle.QfcColumns.cs:289-294`).

Test-side change for the currently failing test (`DfDeedle_COM_Tests.cs:477-482`): pass `timeProvider: new FakeTimeProvider()` and never advance it. Every deadline on the path is then armed on a clock that does not move, so no deadline can fire regardless of host load, and the test asserts exactly what it asserted before. This is not a timing tolerance: the deadline is not widened, it is placed under the test's control.

Widening the 250 ms deadline (or raising `GetRowCount()` in the mock, as `OlTableExtensions_Tests.cs:960-963` already does with `Returns(120)` "so the timeout cannot fire under test-host contention") is an AC5 violation and is rejected. The existing `Returns(120)` workaround is itself a timing tolerance in the tree; the plan should convert that test to the fake clock in the same change since `EtlAsync` gains the parameter anyway.

AC2's seam clause ("no longer mutates process-wide static seams in a way another class can observe"): as established in Section 1, no other class can observe them today, so the clause is already true by absence of a reader. To make it true by construction, the recommended disposition is to convert the two ETL delegate statics into optional parameters and delete the statics:

- `DfDeedle.cs:69-72` `TableEtlInvoker` -> parameter `Func<object, (object[,] data, Dictionary<string, int> columnInfo)>? etl = null` on `GetEmailDataInView(Explorer)` (production caller: `QfcDatamodel.FrameBuilding.cs:15`, source-compatible).
- `DfDeedle.cs:81-84` `StoreTableEtlInvoker` -> same optional parameter on `FromDefaultFolder(Store, ...)` (`DfDeedle.FrameUtilities.cs:125`, read at line 143; production callers `ToDoModel/Data Model/ID/IDList.cs:130, 226` and `DfDeedle.FrameUtilities.cs:160`, all source-compatible).
- `MessageBoxInvoker` (`DfDeedle.cs:54-60`) stays static: it is a modal-dialog stub reached through private methods that tests invoke by reflection with fixed argument arrays; parameterising it would rewrite five tests for no determinism gain. Document it in the test-class header as restored-in-`finally` and reader-free.

This disposition is severable. If the planner prefers the smallest diff, the fallback the issue text itself sanctions is `[DoNotParallelize]` on `DfDeedle_COM_Tests` with a comment stating the three seams are process-global, that this class is their only writer, and that the attribute exists to keep that true if a reader is ever added. That fallback masks nothing today because nothing races.

### AC3 / issue #594 items 2 and 3 - `Console.Out` races

Search performed over every `*.Test` project (`Console\.SetOut|Console\.SetError|Console\.Out\b|Console\.Error\b|StringWriter`, glob `*.Test/**/*.cs`), then restricted to `UtilitiesCS.Test` for counts. No `Console.SetError` or `Console.Error` use exists in any test project. Findings for `UtilitiesCS.Test`:

Capture-and-assert sites (a `StringWriter` installed with `Console.SetOut`, restored in `finally`, then asserted on). These are the VICTIM side of a race:

| # | File | Class | Test method | Lines | Production member exercised |
|---|---|---|---|---|---|
| 1 | `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs` | `StackGeek_Tests` | `Main_RunsSampleScenarioWithoutThrowing` | 151-167 | `GFG.Main` writes via `Console.WriteLine` (`UtilitiesCS/ReusableTypeClasses/Other/StackGeek.cs:187-191`) |
| 2 | `UtilitiesCS.Test/HelperClasses/PrettyPrint_Tests.cs` | `PrettyPrint_Tests` | `DataFramePrettyHelpers_RenderRowsMarkdownAndConsoleOutput` | 194-219 | `PrettyPrinters.PrettyPrint(DataFrame)` / `PrettyPrint(DataFrameRow)` (`UtilitiesCS/HelperClasses/PrettyPrint.cs:25, 27`) |
| 3 | `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` | `DASLFilterParserTests` | `PrintTree_WritesIndentedTreeToConsole` | 107-122 | `DASLFilterParser.PrintTree` (`UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs:97-104`) |
| 4 | `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` | `OlTableExtensions_Tests` | `EnumerateTable_WritesFormattedOutputAndMovesToStart` | 1635-1651 | `OlTableExtensions.EnumerateTable` (`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs:423-425`) |

All four classes ALREADY carry `[DoNotParallelize]` (lines 15, 19, 14 and 20 respectively), each with a comment naming this exact race as the reason. The duplicate `UtilitiesCS.Test/OutlookObjects/DASLFilterParser_Tests.cs` that issue #520 identified as the colliding twin no longer exists (Glob `**/DASLFilterParser*Tests*.cs` returns only the `Filter DASL` file).

Aggressor sites (replace `Console.Out` and either never restore it or restore it at an arbitrary time). None of these asserts on console content, so none can be a victim:

- 24 files call `Console.SetOut(new DebugTextWriter())` from a `[ClassInitialize]`/`[TestInitialize]` method with no restore (for example `Frexp_Test.cs:16`, `BayesianClassifierTests.cs:22`, `ScoDictionaryConverterTests.cs:25`, `Triage_OlLogicTests.cs:27`; `ObsoleteBayesianClassifier_Tests.cs` does it twice at 61 and 476). `DebugTextWriter` is `UtilitiesCS/HelperClasses/Logging/DebugTextWriter.cs:12` (a `StreamWriter` over `Debug`); two nested copies also exist at `UtilitiesCS.Test/DeedleTests.cs:28` and `UtilitiesCS.Test/Extensions/DeedleTests.cs:27`.
- `UtilitiesCS.Test/HelperClasses/NLogTraceWriter_Test.cs:22-23, 56` sets a `DebugTextWriter` in `[TestInitialize]` and restores in `[TestCleanup]`; not `[DoNotParallelize]`; its tests (lines 59-117) verify a mock logger, not console text.

Mechanism of interference (as described in issue #520 and consistent with the code): class A saves `originalOut` and installs `writer_A`; class B on another worker saves `originalOut` (now `writer_A`) and installs `writer_B`; A's `finally` restores the real console, detaching `writer_B`; B's production call writes to the console and `writer_B.ToString()` is empty, so B's `Contain(...)` assertion fails. With four victims and roughly 25 aggressors, any victim could lose.

Is the count two? No. The count of capture-and-assert sites in the current tree is FOUR (derivation in the Numeric Derivation Evidence section below). "Two" in #594 is most plausibly the number of such failures observed in one run, which the tree cannot verify. Under MSTest's documented execution model, tests marked `[DoNotParallelize]` run sequentially after the parallel set completes, so as of the current tree none of the four victims can overlap an aggressor; the race is currently suppressed by the stopgap, not eliminated. (This ordering claim is from the MSTest adapter's documented behaviour; it was not confirmed by a run in this session.)

Recommended fix (eliminate the shared-console dependency; `.claude/rules/csharp.md` "DI Seams" preference 2, injectable delegate/argument seam):

| Production member | Change | Non-test callers that must remain source-compatible |
|---|---|---|
| `DASLFilterParser.PrintTree(TreeNode<string> node, int level)` (`DASLFilterParser.cs:97`, file has `#nullable enable`) | Add `TextWriter? writer = null`; body writes to `(writer ?? Console.Out)` and passes `writer` on recursion | `UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs:74`; test `Triage_OlLogicTests.cs:142` |
| `PrettyPrinters.PrettyPrint(this DataFrame)` and `PrettyPrint(this DataFrameRow)` (`PrettyPrint.cs:25, 27`, `#nullable enable`) | Add `TextWriter? writer = null`; `(writer ?? Console.Out).WriteLine(...)`. Zero line growth (the file is already 680 lines, over the 500-line cap; do not grow it) | `TaskMaster/Ribbon/TryFunctionalityInConstruction.cs:66` |
| `OlTableExtensions.EnumerateTable(this Outlook.Table)` (`TableAccess.cs:381`, `#nullable enable`) | Add `TextWriter? writer = null`; replace the three `Console.WriteLine` at 423-425 | `TaskMaster/Ribbon/TryFunctionalityInConstruction.cs:51` |
| `GFG.Main(String[] args)` (`StackGeek.cs:173`, `#nullable enable`) | Extract the body into `public static void Run(TextWriter writer)` and have `Main` call `Run(Console.Out)` | none (sample code; no callers outside the test) |

Test-side: the four tests pass their own `StringWriter` and stop touching `Console.Out`. Then remove `[DoNotParallelize]` and its comment from `StackGeek_Tests`, `DASLFilterParserTests`, `PrettyPrint_Tests` and `OlTableExtensions_Tests`, whose comments cite the console redirect as the sole reason. Removing the attribute is what makes the AC4 ten-run gate exercise the seam rather than the stopgap. If the planner judges `OlTableExtensions_Tests` (1855 lines, many COM-mock tests) too large to re-parallelize in a bugfix, keep its attribute but rewrite the comment so it no longer claims the console as the reason.

Out of scope but recorded: the ~25 `DebugTextWriter` aggressors remain process-global mutations with no restore; once no test captures `Console.Out` they harm nothing, but they are the reason `Console.Out` is an arbitrary writer for the rest of the run. `OlTableExtensions.TableAccess.cs:78, 96` also write diagnostics with `Console.WriteLine` in production instead of the logger.

### AC4 - the determinism gate

Workflow: `.github/workflows/_mstest-coverage.yml`, called from `.github/workflows/ci.yml:30-32` as job `mstest-coverage`. Discovery (lines 86-92) takes every `*.Test.dll` under `\bin\Debug\` excluding `\obj\` and `\ref\`. The nine assemblies (each `.csproj` sets `<OutputPath>bin\Debug\</OutputPath>` for the Debug|AnyCPU configuration):

```
QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
SVGControl.Test\bin\Debug\SVGControl.Test.dll
Tags.Test\bin\Debug\Tags.Test.dll
TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
TaskTree.Test\bin\Debug\TaskTree.Test.dll
TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

CI invocation, verbatim from `_mstest-coverage.yml:99`:

```
& $vstestPath $testAssemblies /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"
```

Two properties of that invocation matter for fidelity:

- CI passes NO `/Settings:`. Parallelism therefore comes only from `[assembly: Parallelize(Workers = 0, Scope = ClassLevel)]` in `UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21`; no other test project declares the attribute (Grep `Parallelize` over `*.cs`), so in CI only `UtilitiesCS.Test` runs in parallel. `Workers = 0` means the processor count of the host: 24 on the local workstation, and whatever the GitHub-hosted `windows-latest` runner provides (GitHub's published specification is 4 cores for standard runners; not verifiable from the tree). The failures reproduced in both environments.
- The local scripts differ: `scripts/vscode/Invoke-MSTest.ps1:54` and `Invoke-MSTestWithCoverage.ps1:76` add `/Settings:scripts\vscode\TaskMaster.cli.runsettings`, which applies `Workers 0 / ClassLevel` to ALL nine assemblies. A local run is therefore a heavier load than CI for the other eight assemblies.

Known local-only obstacles (from `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/`):

- `shell-icon-stall-probe.md` (2026-09-07T00-55): the four shell-icon classes (`ShellUtilities_Tests`, `ShellUtilitiesStatic_Tests`, `SysImageListHelper*`, `OSBrowser*`) no longer stall the testhost through `SHGetFileInfo` on this workstation, but one of the 23 tests fails per run with `Win32 handle that was passed to Icon is not valid`, the failing test moving between sibling classes. Verdict recorded: `SHELL_ICON_EXCLUSION: REQUIRED`, filter extension `&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser`. CI runs those classes unfiltered.
- `vstest-coverage-baseline.md` (2026-09-07T01-00): `/InIsolation` is required; assembly discovery from a worktree under `.claude/worktrees/` must apply the dot-claude exclusion to the path RELATIVE to the worktree root, or the discovery returns zero assemblies and the gate is vacuous; `/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None` was used as a hang guard. That baseline (9 assemblies, no in-proc coverage, dotnet-coverage collect wrapper) executed 7023 tests in 54.5 s.

Proposed concrete ten-run gate (PowerShell, run from the worktree root; `<feature>` is this feature folder):

```powershell
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
$assemblies = @(
  'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll','SVGControl.Test\bin\Debug\SVGControl.Test.dll',
  'Tags.Test\bin\Debug\Tags.Test.dll','TaskMaster.Test\bin\Debug\TaskMaster.Test.dll',
  'TaskTree.Test\bin\Debug\TaskTree.Test.dll','TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll',
  'ToDoModel.Test\bin\Debug\ToDoModel.Test.dll','UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll',
  'VBFunctions.Test\bin\Debug\VBFunctions.Test.dll')
$filter = 'TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser'
foreach ($run in 1..10) {
  & $vstest $assemblies /EnableCodeCoverage /InIsolation `
      "/Logger:trx;LogFileName=ac4-run-$run.trx" "/ResultsDirectory:coverage\trx\ac4" `
      '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' "/TestCaseFilter:$filter"
  if ($LASTEXITCODE -ne 0) { throw "AC4 run $run failed with exit code $LASTEXITCODE" }
}
```

Then read `<Counters total= executed= passed= failed= ...>` from each of the ten TRX files and record a ten-row table (run, total, passed, failed, wall-clock) in `docs/features/active/<feature>/evidence/regression-testing/ac4-ten-run.<timestamp>.md`. Do not commit the raw TRX or `.coverage` files: the TRX embeds `computerName` and absolute paths, and `/EnableCodeCoverage` writes large binary `.coverage` files; `coverage\` is gitignored. `LogFileName=` is set so the default `<account>_<host>_<timestamp>.trx` naming is not produced.

Honest duration estimate: the #798 baseline measured 54.5 s per nine-assembly run without in-proc coverage. `/EnableCodeCoverage` adds instrumentation overhead that has not been measured for the full nine-assembly set in any committed evidence (#780 reports 35 s for two assemblies with coverage). Expect roughly 1 to 2 minutes per run, i.e. 10 to 20 minutes for the gate, plus one build. The filter deviation (shell-icon exclusion) must be recorded alongside the result, and the CI run on the PR provides the unfiltered form.

Note that the local gate, because of the runsettings difference above, should NOT pass `/Settings:` if CI parity is the goal; the command above omits it deliberately so that parallelism comes from the assembly attribute exactly as in CI.

### AC5 - the no-timing-hack constraint

Every timing construct in the files the fix will touch, with disposition:

| File | Line(s) | Construct | Disposition |
|---|---|---|---|
| `UtilitiesCS/Extensions/DictionaryExtensions.cs` | 176-177 | `CreateLinkedTokenSource(token)` + `CancelAfter(500)` | REMOVE (AC1) |
| `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs` | 81 | `int milliseconds = 250 * rowCount` | keep value; deadline becomes clock-driven |
| same | 82 | `var attempts = 3` (inert) | remove |
| same | 114 | `.TimeoutAfter(milliseconds, attempts)` | -> `.TimeoutAfter(milliseconds, timeProvider)` |
| same | 120 | `DateTime.Now` (banned symbol, suggestion severity) in the timeout log | remove while editing the line |
| same | 147-148, 158 | `EtlAsyncOld`: same 250 ms x rows deadline and inert retry | dead in production (only caller `OlTableExtensions_Tests.cs:1005`); leave or delete as the planner decides; deleting frees 38 lines in a 475-line file |
| same | 245, 259 | `.TimeoutAfter(timeout, attempts)` | -> `.TimeoutAfter(timeout, timeProvider)` |
| same | 316-325, 364-373 | `new Timer(..., 0, 500)` progress-report timers | unchanged; they only call `progress.Report`, which the test's `SilentProgressTracker` no-ops |
| `UtilitiesCS/Extensions/DfDeedle.cs` | 156 | `GetTableInViewAsync(token, 0)` (2000 ms window inside, up to 3 attempts) | unchanged; see Section 5 |
| same | 176 | `AddQfcColumnsAsync(..., 0)` (3 x 3000 ms) | pass `timeProvider` |
| same | 180 | `EtlAsync(...)` | pass `timeProvider` |
| same | 208 | `.TimeoutAfter(1000, 2)` (inert retry) | -> `.TimeoutAfter(1000, timeProvider)` |
| `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` | 21, 127 | `AttemptLimit = 3`, `TimeoutAfter(3000, timeProvider)` | unchanged (already seamed by #798) |
| `UtilitiesCS/Threading/TimeOutTask.cs` | 824-849, 924-940 | inert `(int, int)` overloads | unchanged; file is 1012 lines (over cap) and the overloads keep two test callers (`TimeOutTask_Tests.cs:197, 210`) and `EtlAsyncOld`; recommend a follow-up to delete them |
| `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` | 35, 56-63, 79, 97 | `timeoutMs = 2000`, `RunWithTimeout`, `counter < 2` retries | unchanged (only `EnumerateTable` is touched for AC3) |
| `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` | - | no `Thread.Sleep`, `Task.Delay`, `CancelAfter`, `TimeoutAfter`, `WaitOne`, retry or stopwatch (Grep over the file returned nothing) | - |
| `UtilitiesCS.Test/Extensions/DictionaryExtensions_Tests.cs` | - | none | - |
| `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` | 960-963 | `mockTable.Setup(t => t.GetRowCount()).Returns(120)` with the comment "so the timeout cannot fire under test-host contention" | an existing timing tolerance on the very method under repair; convert to the fake clock |
| same | 72-110, 802-825 | `RunTableRetry` tests | logic tests of a retry helper, not timing; unchanged |
| `UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs` | 197, 210 | `TimeoutAfter(100, 3)` on already-completed tasks | short-circuit path; not timing-dependent; unchanged |
| `StackGeek_Tests.cs`, `PrettyPrint_Tests.cs`, `DASLFilterParserTests.cs`, `NLogTraceWriter_Test.cs` | - | none | - |

`BannedSymbols.txt` (repository root, 7 entries, verbatim):

```
P:System.DateTime.Now;Do not use DateTime.Now. Inject System.TimeProvider and call GetLocalNow() for testable, deterministic time.
P:System.DateTime.UtcNow;Do not use DateTime.UtcNow. Inject System.TimeProvider and call GetUtcNow() for testable, deterministic time.
P:System.Random.Shared;Do not use Random.Shared. Inject a seeded or deterministic random source for testability.
M:System.Threading.Thread.Sleep(System.Int32);Do not call Thread.Sleep. Use async/await with an injected time abstraction or a cancellation token.
M:System.Threading.Thread.Sleep(System.TimeSpan);Do not call Thread.Sleep. Use async/await with an injected time abstraction or a cancellation token.
M:System.Threading.Tasks.Task.Delay(System.Int32);Do not call Task.Delay directly in production code. Inject a time abstraction (System.TimeProvider).
M:System.Threading.Tasks.Task.Delay(System.TimeSpan);Do not call Task.Delay directly in production code. Inject a time abstraction (System.TimeProvider).
```

RS0030 severity: `suggestion` (`.editorconfig:545-548`: "RS0030 held at suggestion for initial rollout. Promotion to warning is a post-cleanup follow-up (143 existing banned-symbol usages, see issue #181 evidence)"). Consequence for the plan: the analyzer will NOT fail the build on a new `Thread.Sleep`/`Task.Delay`; AC5 must be enforced by a grep gate over the diff, not by the toolchain. `CancelAfter`, `TimeoutAfter`, `WaitOne` and `new CancellationTokenSource(int)` are not in `BannedSymbols.txt` at all. The analyzer is wired into `UtilitiesCS.csproj:1312-1314` and `UtilitiesCS.Test.csproj:973-975` via `Microsoft.CodeAnalysis.BannedApiAnalyzers 5.6.0` with `<AdditionalFiles Include="$(MSBuildThisFileDirectory)..\BannedSymbols.txt" />`.

---

## Numeric Derivation Evidence

AC3 states a number ("the two `Console.Out` races"). The derivation below is for the population the fix must address: test methods in `UtilitiesCS.Test` that capture `Console.Out` into a writer and assert on its content.

- Complete Family: test methods in `UtilitiesCS.Test` that (a) call `Console.SetOut` with a writer they own, and (b) assert on that writer's captured text.
- Exhaustive Search Scope: every `*.cs` file under `UtilitiesCS.Test/` (the assembly that carries the class-level `Parallelize` attribute). Other test assemblies were searched with the same primary pattern for completeness and hold no capture-and-assert sites (`VBFunctions.Test/ComputerInfo_Test.cs:15`, six `QuickFiler.Test` files, `TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs:34`, `ToDoModel.Test/.../PeopleScoDictionaryNewTests.cs:26` and the two `TreeNodeTests*.cs` at lines 28/30 all install a `DebugTextWriter`/`tw` without asserting on it).
- Inclusion Rules: the `Console.SetOut` argument is a `StringWriter` (or other writer) created by the test, and the test method contains an assertion over `writer.ToString()`/`output.ToString()`.
- Exclusion Rules: `Console.SetOut(new DebugTextWriter())` (fire-and-forget redirect), `Console.SetOut(originalOut)`/`Console.SetOut(original)`/`Console.SetOut(this.originalOut)` (restores), commented-out calls, and `StringWriter` instances not passed to `Console.SetOut`.
- Primary Search Strategy or Query Expression: `Grep` pattern `Console\.SetOut`, path `UtilitiesCS.Test`, count mode -> 37 occurrences across 28 files; then content mode to classify each occurrence by argument.
- Primary Member Set: `StackGeek_Tests.Main_RunsSampleScenarioWithoutThrowing` (`Console.SetOut(writer)` at 153), `PrettyPrint_Tests.DataFramePrettyHelpers_RenderRowsMarkdownAndConsoleOutput` (196), `DASLFilterParserTests.PrintTree_WritesIndentedTreeToConsole` (109), `OlTableExtensions_Tests.EnumerateTable_WritesFormattedOutputAndMovesToStart` (1640). The remaining 33 occurrences are: 4 restores in those same tests (162, 218, 118, 1645), 3 comment mentions (StackGeek 12, PrettyPrint 17, DASL 12), 1 commented-out call (`BayesianClassifierTests_UnfinishedStubs.cs:31`), 2 in `NLogTraceWriter_Test.cs` (set 23 / restore 56, no content assertion), and 23 `new DebugTextWriter()` installs (22 files with one each, `ObsoleteBayesianClassifier_Tests.cs` with two).
- Primary Count: 4.
- Cross-check Search Strategy or Query Expression: `Grep` pattern `StringWriter` over `*.Test/**/*.cs`, then keep only files that also contain `Console.Out` and whose `StringWriter` line is adjacent to a `Console.SetOut` call.
- Cross-check Member Set: `StackGeek_Tests.cs:152`, `PrettyPrint_Tests.cs:195`, `DASLFilterParserTests.cs:107`, `OlTableExtensions_Tests.cs:1635`. Rejected `StringWriter` hits: `NonRecursiveConverter_Tests.cs:300`, `FileIO2_Tests.cs:151, 206, 277`, `EmailDataMiner_Additional_Tests.cs:473`, `ClassifierGroupUtilities_TestSupport.cs:186, 228-229`, `ClassifierGroupUtilities_Tests.cs:196`, `BayesianSerializationHelper_Tests.cs:510, 522` (that file's only `Console.SetOut`, line 31, installs a `DebugTextWriter`).
- Cross-check Count: 4.
- Member-set Comparison: the two member sets are identical after normalisation to (file, method). The count of capture-and-assert sites is 4, not 2. All four already carry `[DoNotParallelize]`.

---

## Section 3 - Seam availability

Both packages are ALREADY referenced. Exact records (`Grep Bcl\.TimeProvider|TimeProvider\.Testing` over `packages.config` and `*.csproj`):

| Package | Version | `packages.config` | `.csproj` reference (HintPath `..\packages\<id>.<ver>\lib\net462\...dll`) |
|---|---|---|---|
| `Microsoft.Bcl.TimeProvider` | 10.0.11 (assembly 10.0.0.11) | `UtilitiesCS/packages.config:28`, `UtilitiesCS.Test/packages.config:23`, `TaskMaster/packages.config:16`, `TaskMaster.Test/packages.config:18`, `QuickFiler/packages.config:19`, `QuickFiler.Test/packages.config:19` | `UtilitiesCS.csproj:96-97`, `UtilitiesCS.Test.csproj:602-603`, `TaskMaster.csproj:148-149`, `TaskMaster.Test.csproj:74-75`, `QuickFiler.csproj:66-67`, `QuickFiler.Test.csproj:263-264` |
| `Microsoft.Extensions.TimeProvider.Testing` | 10.9.0 | `UtilitiesCS.Test/packages.config:91`, `TaskMaster.Test/packages.config:85`, `QuickFiler.Test/packages.config:86` | `UtilitiesCS.Test.csproj:654-655`, `TaskMaster.Test.csproj:125-126`, `QuickFiler.Test.csproj:312-313` |

In-repo precedent already using the seam on this exact call chain:

- `TimeOutTask.TimeoutAfter(Task<T>, int, TimeProvider?)` and `(Task, int, TimeProvider?)` (`TimeOutTask.cs:862, 949`) arm their timer with `(timeProvider ?? TimeProvider.System).CreateTimer(...)`.
- `DfDeedle.AddQfcColumnsAsync(..., TimeProvider? timeProvider = null)` (`DfDeedle.QfcColumns.cs:102-109, 127`) - the #798 shape this fix should copy.
- Tests: `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` (`using Microsoft.Extensions.Time.Testing;`, `FakeTimeProvider`, and a reusable `ArmingBarrierTimeProvider` at lines 50-85 that signals when the production code has armed a timer so a test can advance the clock only after the deadline exists), `UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs:82-112` (`FakeTimeProvider` advanced past the deadline faults the proxy), `UtilitiesCS.Test/Threading/ThreadMonitorTests.cs`.

No `packages.config` or `.csproj` change is required by this fix. `.claude/rules/csharp.md:55-63` explicitly endorses using the seam where `Microsoft.Bcl.TimeProvider` is already present.

---

## Section 4 - Recommended fix design

Production files (all in `UtilitiesCS`; all carry `#nullable enable`):

| # | File (current length) | Method | Change |
|---|---|---|---|
| P1 | `UtilitiesCS/Extensions/DictionaryExtensions.cs` (283) | `TryAddValuesAsync` (169-180) | Delete lines 176-177; `return await Task.Run(() => dictionary.TryAddValues(key, value), token);` |
| P2 | `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs` (475) | `EtlAsync` (66-130) | Add trailing `TimeProvider? timeProvider = null`; line 114 -> `.TimeoutAfter(milliseconds, timeProvider)`; pass `timeProvider` at line 98-106; drop `attempts` (82) and fix the log text (119-121) |
| P2 | same | `EtlByRowAsync` (229-263) | `int attempts` -> `TimeProvider? timeProvider`; lines 245, 259 -> `.TimeoutAfter(timeout, timeProvider)` |
| P3 | `UtilitiesCS/Extensions/DfDeedle.cs` (315) | `GetEmailDataInViewAsync` (139-219) | Add trailing `TimeProvider? timeProvider = null`; pass it at 176 (`timeProvider: timeProvider`), 180 (`timeProvider: timeProvider`), 208 (`.TimeoutAfter(1000, timeProvider)`); insert the null guard before line 186 |
| P4 (severable) | `UtilitiesCS/Extensions/DfDeedle.cs`, `UtilitiesCS/Extensions/DfDeedle.FrameUtilities.cs` (276) | `GetEmailDataInView` (86-102), `FromDefaultFolder(Store, ...)` (125-148) | Replace the `TableEtlInvoker` / `StoreTableEtlInvoker` statics (62-84) with optional delegate parameters; `FromDefaultFolder(Stores, ...)` at 160 forwards the parameter |
| P5 | `UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs` (122) | `PrintTree` (97-104) | Add `TextWriter? writer = null`; write to `(writer ?? Console.Out)`; forward on recursion |
| P6 | `UtilitiesCS/HelperClasses/PrettyPrint.cs` (680, pre-existing over cap) | `PrettyPrint(DataFrame)`, `PrettyPrint(DataFrameRow)` (25, 27) | Add `TextWriter? writer = null`; zero line growth |
| P7 | `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` (431) | `EnumerateTable` (381-428) | Add `TextWriter? writer = null`; lines 423-425 write to it |
| P8 | `UtilitiesCS/ReusableTypeClasses/Other/StackGeek.cs` (199) | `GFG.Main` (173-192) | Extract `Run(TextWriter writer)`; `Main` calls `Run(Console.Out)` |

Test files (`UtilitiesCS.Test`; none of the files in scope carries `#nullable enable`, so nullable annotations added there are inert and `null!` is unnecessary):

| # | File (current length) | Change |
|---|---|---|
| T1 | `Extensions/DictionaryExtensions_Tests.cs` (296) | Keep `TryAddValuesAsync_UpdatesExistingValue`; add a pre-cancelled-token test and a test proving the outer token governs cancellation |
| T2 | `Extensions/DfDeedle_COM_Tests.cs` (870, over cap; modify in place, do not grow) | `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`: pass `timeProvider: new FakeTimeProvider()`; if P4 is adopted, replace the three static-seam swaps (399-414, 762-782, 827-847) with the delegate argument and delete their `try/finally`; update the class header (lines 20-33) |
| T3 | NEW `Extensions/DfDeedleEtlTimeoutTests.cs` | RED-first regression for AC2: strict `Table` mock whose `GetNextRow` blocks on a gate; barrier clock advanced 250 ms once the first `TimeoutAfter` timer is armed; assert `GetEmailDataInViewAsync` throws `InvalidOperationException` naming `Inbox` (fails before the fix with `NullReferenceException`); release the gate in `finally` so the orphaned `Task.Run` completes. Second test: un-advanced `FakeTimeProvider`, gate never engaged, frame has one row (the deterministic green path) |
| T4 | NEW `OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs` | `EtlAsync` unit tests on the fake clock: deadline expiry returns `data == null` and cancels `tokenSource` (documents the surviving contract); un-advanced clock returns the transformed rows. Retire the `Returns(120)` tolerance at `OlTableExtensions_Tests.cs:960-963` by passing the fake clock there instead |
| T5 | `Extensions/DfDeedleQfcColumnTimeoutTests.cs` (exactly 500) | Optional pure move of the nested `ArmingBarrierTimeProvider` (lines 50-85) to NEW `TestHelpers/ArmingBarrierTimeProvider.cs` so T3 can reuse it without duplication; the file cannot otherwise grow by even one line |
| T6 | `ReusableTypeClasses/StackGeek_Tests.cs` (276), `HelperClasses/PrettyPrint_Tests.cs` (408), `OutlookObjects/Filter DASL/DASLFilterParserTests.cs` (126), `OutlookObjects/Table/OlTableExtensions_Tests.cs` (1855, over cap; modify in place) | Pass a `StringWriter` to the seamed member; delete `Console.Out` save/set/restore; remove `[DoNotParallelize]` and its comment (or rewrite the comment for `OlTableExtensions_Tests` if retained) |

Ripple beyond `UtilitiesCS` and `UtilitiesCS.Test`:

- `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs:82-89` calls `GetEmailDataInViewAsync` with four positional arguments; the new optional parameter is source-compatible; `QuickFiler` recompiles in the same solution (an optional parameter is binary-breaking only for pre-compiled callers, of which there are none outside the solution).
- `TaskMaster/Ribbon/TryFunctionalityInConstruction.cs:51, 66` call `EnumerateTable()` and `df.PrettyPrint()`; source-compatible with the optional `TextWriter?`.
- `UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs:74` calls `parser.PrintTree(logicTree, 0)`; source-compatible.
- If P4 is adopted: `ToDoModel/Data Model/ID/IDList.cs:130, 226` call `FromDefaultFolder(Store, ...)`; source-compatible with an optional trailing delegate. `ToDoModel` recompiles.
- No change to `TimeOutTask.cs`, to any `packages.config`, or to any `.csproj` (new test files in a legacy non-SDK project DO require a `<Compile Include=...>` item in `UtilitiesCS.Test.csproj`; that is the only project-file edit and it is a Compile item, not a package change).

File-size budget check: `OlTableExtensions.Etl.cs` is at 475 of 500; P2 adds roughly one line per signature plus a short `<param>` comment and removes one local, so keep the added documentation to two or three lines or delete the dead `EtlAsyncOld` (lines 132-169) in the same edit. `DfDeedle.cs` at 315 has room. `PrettyPrint.cs` (680), `TimeOutTask.cs` (1012), `DfDeedle_COM_Tests.cs` (870) and `OlTableExtensions_Tests.cs` (1855) are pre-existing violations of the 500-line cap; the plan must not add net lines to them and must say so.

Rejected alternatives (brief):

- Widening the 250 ms deadline or raising `GetRowCount()` in the mock: AC5 violation; rejected.
- `[DoNotParallelize]` on `DfDeedle_COM_Tests` as the AC2 fix: does not address the cause (a wall-clock deadline fires under load whether or not the class is serialized, because the sequential phase still runs under coverage instrumentation and pool contention from the test host); acceptable only as documentation of the seam clause.
- Changing `EtlAsync`'s tuple to `object[,]? data` and rethrowing the `TimeoutException`: cleaner contract but widens the diff into every `EtlAsync` consumer and the un-annotated test files; keep as a follow-up.
- `TimeProvider` seam for `TryAddValuesAsync`: preserves a deadline that has no semantic; deletion is simpler and AC1 permits it.

---

## Section 5 - Risks and open questions

1. Is `EtlAsync`'s timeout load-bearing for real Outlook COM calls? Partly. `Table.GetNextRow`/`Row.GetValues` are synchronous COM calls that can stall on a slow store, and the deadline is the only thing that returns control to the caller; but the deadline cannot stop the work (the `Task.Run` body keeps running inside the interop marshaller, exactly as the #798 comment at `DfDeedle.QfcColumns.cs:114-118` records), and its expiry today produces an NRE in `GetEmailDataInViewAsync` rather than a cancellation. The recommended design keeps the production deadline byte-for-byte (system clock when `timeProvider` is null) and only makes the test control the clock, so no production safety behaviour is removed; the guard converts the post-expiry NRE into a descriptive `InvalidOperationException` that `QfcDatamodel.FrameBuilding.cs:102-108` logs and rethrows. Whether QuickFiler should instead treat that as a cancellation (return null) is a product decision outside this fix.
2. The 250 ms-per-row budget is small for real stores as well as for tests (a one-row folder gets 250 ms for two pool hops plus COM enumeration). This research does not recommend changing it under #811 because doing so is a timing change with no deterministic test, but it should be promoted as a potential defect.
3. `GetTableInViewAsync` (`DfDeedle.cs:156`) keeps a 2000 ms system-clock window on the same test path via `new CancellationTokenSource(ms)` (`TimeOutTask.cs:52-54, 199-201`), seamed by a `Func<int, CancellationTokenSource>` factory rather than a `TimeProvider`. It has eight times the margin of the 250 ms window and did not trip in the recorded failure, but it is the next-smallest deadline on the path. Options: pass a factory returning a never-cancelling `CancellationTokenSource` from the test (requires threading `timeoutSourceFactory` through `GetEmailDataInViewAsync`, a second seam type), or unify on `TimeProvider` (Microsoft.Bcl.TimeProvider ships `TimeProviderTaskExtensions.CreateCancellationTokenSource(this TimeProvider, TimeSpan)`; availability in 10.0.11 was not verified this session). Recommend recording it as a residual risk and a follow-up rather than widening this fix.
4. MSTest ordering of `[DoNotParallelize]` tests after the parallel set is asserted from documentation, not from a run in this session. If it is wrong, the four console-capture tests could still race today; the seam fix removes the dependency either way.
5. The ten-run gate uses the shell-icon exclusion locally; the unfiltered form only runs in CI. One CI run is not ten. The evidence artifact must state both facts.
6. `Workers = 0` makes the local worker count (24) differ from the CI runner's, and the local `/Settings:` scripts parallelize eight assemblies CI runs sequentially. The proposed gate omits `/Settings:` for CI parity; if the planner prefers the heavier local load as a stress test, say which one the evidence represents.
7. RS0030 is at suggestion severity, so no toolchain step will fail on a newly introduced `Thread.Sleep`/`Task.Delay`; AC5 needs an explicit diff grep in the QA loop.
8. Removing `[DoNotParallelize]` from `OlTableExtensions_Tests` (1855 lines) re-parallelizes a large COM-mock class whose only documented serialization reason is the console; if any undocumented shared state exists there, the ten-run gate is the detector. Keeping the attribute with a corrected comment is the conservative fallback.
9. The RED test in T3 depends on advancing the fake clock only after the first `TimeoutAfter` timer is armed (`FakeTimeProvider` schedules timers relative to its current time at creation); reuse the `ArmingBarrierTimeProvider` pattern or the test will hang. `DfDeedleQfcColumnTimeoutTests.cs:44-49` documents the constraint.
10. No git history was consulted (shell unavailable). Line numbers were read from the worktree at the timestamp above; the plan should re-verify them at execution time.
