# QuickFiler Queue/Datamodel Defects — Consolidated Research

- **Feature:** `quickfiler-queue-datamodel-defects` (primary issue #446; also closes #448, #426, #427)
- **Timestamp:** 2026-08-24T09-50
- **Worktree HEAD:** `988e819b`
- **Citation baseline:** every file:line below was re-verified against the CURRENT worktree. The four promoted potential documents were captured at `fb32b923` (2026-08-07); corrections are called out inline.
- **Mode:** read-only research. No production or test source file was modified.

---

## 0. Citation re-verification summary (fb32b923 -> 988e819b)

| Potential doc claim | Status at `988e819b` | Correction |
| --- | --- | --- |
| #446: `QfcHomeController.Iteration.cs:21` calls two-arg dequeue | **Confirmed, line unchanged** | call spans `:21-24` |
| #446: `QfcDatamodel.QueueProcessing.cs:66-76` two-arg delegates with default deadline | **Confirmed, lines unchanged** | — |
| #446: `QfcStreamingDequeueConfidenceGate.cs:22` = 12 s default | **Confirmed, line unchanged** | — |
| #446: `QfcHomeController.Iteration.cs:32` infers exhaustion | **Confirmed, structure unchanged** | the `else` opens at `:32`; the `CompleteAddingAsync` call is at `:35` |
| #446: `QfcQueue.cs:59` reaches `_queue.CompleteAdding()` | **Confirmed, line unchanged** | — |
| #448: loop at `QfcFormController.Actions.cs:253-292` | **Confirmed, lines unchanged** | — |
| #448: condition at `:258`, threshold branch at `:279`, reset at `:288-291` | **Confirmed, lines unchanged** | — |
| #448: `_undoQueue` declared `QfcFormController.cs:90`, `??=` guard at `Actions.cs:211` | **Confirmed, lines unchanged** | — |
| #448: "F6 introduced an injectable start-delegate around `Task.Run(UndoConsumer)`" | **NOT TRUE in this worktree** | `QfcFormController.Actions.cs:211` is a bare `_undoConsumerTask ??= Task.Run(UndoConsumer);`. No start-delegate seam, no `Func<...>` property, no `docs/features/**/*435*` folder exists. See §2.5. |
| #426: `UnhookDequeuedNodes` at `QueueProcessing.cs:107-128` | **MOVED** | now `QfcDatamodel.QueueProcessing.cs:145-166` |
| #426: `TryUnhookOrReplace` at `QueueProcessing.cs:18`, `UnhookItem` at `:33` | **MOVED** | now `:29-64`, with `_moveMonitor.UnhookItem(node)` at `:44` |
| #426: bare take delegate at `QueueProcessing.cs:82` | **MOVED** | now `QfcDatamodel.QueueProcessing.cs:118` |
| #426: `EmailMoveMonitor.HookItem` at `:46-58`, `_hookedItems` at `:44`, subscribe at `:57` | **Confirmed / minor drift** | `HookItem` is `:46-61`; `_hookedItems` at `:44`; `folder.BeforeItemMove += BeforeItemMove` at `:57` |
| #426: `UnhookAll` at `EmailMoveMonitor.cs:185` | **Confirmed, line unchanged** | body `:185-200` |
| #426: `UnhookAll` runs only from `QfcDatamodel.cs:80` | **Confirmed, line unchanged** | but see §3.4 — `QfcCollectionController.cs:1007` also calls `UnhookAll` on its *own separate* monitor instance |
| #426: pinned test at `QfcStreamingDequeueConfidenceGateTests.cs:226-237` | **MOVED** | now `QfcStreamingDequeueConfidenceGateTests.cs:298-310` |
| #427: `ScoreRemainingQueueMailItemAsync` at `QfcDatamodel.cs:346-360` | **MOVED** | now `QfcDatamodel.cs:363-377`; the dropped `TopFolder` is at `:376` (`return score.Score;`) |
| #427: `LoadFolderHandlerAsync` at `FolderHandling.cs:57-90` | **Confirmed start / longer body** | now `QfcItemController.FolderHandling.cs:57-131` |
| #427: carrier overload at `QfcFormController.Actions.cs:114-120`, plain at `:62` | **Confirmed, lines unchanged** | `:114-117` (one-arg carrier), `:120-164` (two-arg carrier), `:62-65` (one-arg plain), `:67-105` (two-arg plain) |

Nothing in the four defects has been fixed by intervening work. All four are live at `988e819b`.

---

## 1. Defect #446 — `IterateQueueAsync` deadline closes the queue early (HIGHEST severity)

### 1.1 Current `IterateQueueAsync` body

`QuickFiler/Controllers/QfcHomeController.Iteration.cs:11-53`:

```csharp
11        public async Task IterateQueueAsync()
12        {
13            Token.ThrowIfCancellationRequested();
14
15            if (_datamodel.Complete)
16            {
17                return;
18            }
19            try
20            {
21                var listObjects = await _datamodel.DequeueNextItemGroupAsync(
22                    _formController.ItemsPerIteration,
23                    2000
24                );
25                if (listObjects.Count > 0)
26                {
27                    //await UiThread.Dispatcher.InvokeAsync(async () => await QfcQueue.EnqueueAsync(listObjects, _formController.Groups));
28                    await QfcQueue
29                        .EnqueueAsync(listObjects, _formController.Groups)
30                        .ConfigureAwait(false);
31                }
32                else
33                {
34                    //logger.Debug($"{nameof(IterateQueueAsync)} completed");
35                    await QfcQueue.CompleteAddingAsync(Token, 10000);
36                }
37            }
38            catch (OperationCanceledException)
39            { ... }
42            catch (System.Exception)
43            { ... rethrow unless cancelled ... }
53        }
```

The offending empty-batch branch is `:32-36`; the irreversible close is `:35`.

`IterateQueueAsync` is reached from three places, all owned or fire-and-forget:
- `QfcHomeController.Iteration.cs:76` (`Iterate2`, fire-and-forget `_ = IterateQueueAsync();`)
- `QfcHomeController.cs:323` (`await Task.Run(IterateQueueAsync);` at the end of `RunAsync`) — **not owned**
- directly from tests

### 1.2 The two-argument `DequeueNextItemGroupAsync` overload

`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:66-76`:

```csharp
66        public async Task<IList<MailItem>> DequeueNextItemGroupAsync(int quantity, int timeOut)
67        {
68            // Issue #424: the pre-existing two-argument contract is preserved exactly; it delegates
69            // with the default first-batch deadline and no progress sink.
70            return await DequeueNextItemGroupAsync(
71                quantity,
72                timeOut,
73                QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline,
74                null
75            );
76        }
```

**Yes — it still delegates to the deadline-bearing path.** The four-argument overload is at `:78-99`; when `HighConfidenceModeEnabled` it routes to `DequeueWithHighConfidenceGateAsync` (`:110-130`), which constructs the gate with `firstBatchDeadline` at `:117-126`.

`DefaultFirstBatchDeadline` is declared at `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs:22`:

```csharp
22        internal static readonly TimeSpan DefaultFirstBatchDeadline = TimeSpan.FromSeconds(12);
```

Normal (non-high-confidence) mode goes to `DequeueDirectAsync` (`QueueProcessing.cs:101-108`) and never sees a deadline, so this defect is high-confidence-mode-only.

### 1.3 Why a deadline-expired empty result is indistinguishable from exhaustion

The gate's signature is `internal async Task<IList<MailItem>> DequeueAsync(int quantity, int timeOut, CancellationToken token)` — `QfcStreamingDequeueConfidenceGate.cs:87-91`. Its **return type carries no reason field**, and every exit returns the same `accepted` list:

| Line | Exit | Meaning |
| --- | --- | --- |
| `:98` | `return accepted;` (empty) | `quantity <= 0` — degenerate |
| `:113` | `return accepted;` | **deadline expired** (guarded by `:110`, logged at `:112`) |
| `:122` | `return accepted;` | take returned null and (`timeOut <= 0`) or (already waited once **and** `_sourceActive()` false) — **source exhausted** |
| `:148` | `return accepted;` | `accepted.Count == quantity` — quantity satisfied |

`DequeueWithHighConfidenceGateAsync` (`QueueProcessing.cs:128-129`) then does `(await gate.DequeueAsync(...)).ToList()` and hands it to `UnhookDequeuedNodes`, which returns `IList<MailItem>`. The four-argument datamodel overload returns `IList<MailItem>`; the two-argument overload returns `IList<MailItem>`; `IQfcDatamodel` declares `Task<IList<MailItem>>` at `QuickFiler/Interfaces/IQfcDatamodel.cs:26` and `:40-45`.

**Mechanism, stated exactly:** `IterateQueueAsync` observes only `listObjects.Count`. `Count == 0` is produced by both `:113` (deadline) and `:122` (exhaustion). There is no other observable difference — the deadline path emits a `logger.Debug` line (`LogDeadlineExpiry`, `:151-159`) and optionally invokes `_debugLog`, but `_debugLog` is `null` on the production path (`QueueProcessing.cs:122` passes `null` for `debugLog`), and the caller cannot read log4net output. So today the caller cannot distinguish the two causes by any means.

A worked failure: high-confidence mode, low-yield folder, first-batch deadline 12 s, per-item scoring cost ~1 s. `IterateQueueAsync` calls with `timeOut = 2000`. The gate scans ~12 candidates, accepts none, hits `:110`, returns empty at `:113`. `IterateQueueAsync` takes the `else` at `:32` and calls `CompleteAddingAsync`, which reaches `BlockingCollection<T>.CompleteAdding()` at `QfcQueue.cs:59`. The UI queue is closed for the rest of the session while the master queue still holds unscanned items.

### 1.4 Contract-change options on `IQfcDatamodel`

**Complete caller inventory for `DequeueNextItemGroupAsync` (production):**

| # | Call site | Overload | Owned by this feature? |
| --- | --- | --- | --- |
| 1 | `QuickFiler/Controllers/QfcHomeController.Iteration.cs:21-24` | 2-arg | **Yes** |
| 2 | `QuickFiler/Controllers/QfcHomeController.Iteration.cs:62-65` (`Iterate`) | 2-arg | **Yes** |
| 3 | `QuickFiler/Controllers/QfcHomeController.cs:260-263` (`Run`) | 2-arg | **No** (sibling child) |
| 4 | `QuickFiler/Controllers/QfcHomeController.cs:299-304` (`RunAsync`) | 4-arg | **No** (sibling child) |
| 5 | `QuickFiler/Controllers/QfcQueue.cs:476-479` (`ChangeIterationSize`) | 2-arg | **No** |

**Implementers of `IQfcDatamodel`:** exactly one — `QfcDatamodel` (`QuickFiler/Controllers/QfcDatamodel.cs:26`). `QuickFiler/Notes/notes_interfaces.cs:26` declares an unrelated same-named interface but that file is **not** in `QuickFiler/QuickFiler.csproj` (verified: no `Notes\` entry), so it does not compile. All other references are `Mock<IQfcDatamodel>` in tests (20 sites), and Moq generates missing members automatically.

**Option A — leave the contract alone; pass `Timeout.InfiniteTimeSpan` at the iteration call site.**

Signature: unchanged. `IterateQueueAsync` switches from the 2-arg to the existing 4-arg overload:
`_datamodel.DequeueNextItemGroupAsync(_formController.ItemsPerIteration, 2000, Timeout.InfiniteTimeSpan, null)`.

- Touched files: `QfcHomeController.Iteration.cs` only. **Fully inside the owned set.**
- Source-breaking: no.
- The sentinel is already supported and tested: `QfcStreamingDequeueConfidenceGate.cs:75` accepts `Timeout.InfiniteTimeSpan`, and `QfcStreamingDequeueConfidenceGateTests.Part2.cs:262` (`DequeueAsync_DisabledSentinel_ReproducesUnboundedPreChangeBehavior`) pins it.
- With the deadline disabled, exit `:113` becomes unreachable, so `Count == 0` implies exit `:122`, i.e. genuine exhaustion. The bug is closed by *removing* one of the two causes rather than by distinguishing them.
- Cost: the post-UI scan is again unbounded. It runs on a background task (`Task.Run(IterateQueueAsync)` at `QfcHomeController.cs:323`, or fire-and-forget at `Iteration.cs:76`), so it does not block the UI thread, but a large low-yield folder delays the next batch indefinitely. This is exactly the pre-#424 behaviour, and #424's `spec.md` asserted the post-UI site was "left unchanged" — so Option A makes the code match what #424 claimed.
- Existing pinned test impact: `QfcHomeControllerIterationTests.cs` sets up `DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>())` (2-arg) at `:84`, `:130`, `:201`, `:268`, `:372`. Switching to the 4-arg overload makes those setups miss, so a Moq loose mock returns `null` and `listObjects.Count` throws `NullReferenceException`. **Four tests would need their setups retargeted** (`IterateQueueAsync_DataModelComplete` `:77-121`, `IterateQueueAsync_QueueEmpty` `:123-182`, `IterateQueueAsync_Queue2` `:184-256`, `IterateQueueAsync_WhenDequeueReturnsFullQualifiedPage_EnqueuesAllItems` `:258-310`, plus `Iterate_HighConfidenceEnabled_DoesNotLoadDirectSynchronousBatch` `:356-402` for call site #2 if that is changed too).

**Option B (RECOMMENDED) — add one additive member to `IQfcDatamodel` that returns the batch plus its stop reason.**

Exact signature to add to `QuickFiler/Interfaces/IQfcDatamodel.cs` (owned file):

```csharp
/// <summary>
/// Issue #446. Dequeues the next group and reports WHY the gate stopped, so a caller can
/// distinguish a deadline-bounded empty batch from genuine source exhaustion. The three
/// pre-existing overloads are unchanged and remain the batch-only contract.
/// </summary>
Task<QfcDequeueBatch> DequeueNextItemGroupWithOutcomeAsync(
    int quantity,
    int timeOut,
    TimeSpan firstBatchDeadline,
    Action<int, int, int> progress
);
```

with the carrier declared alongside it (or in the gate file):

```csharp
public enum QfcDequeueStop { QuantitySatisfied, SourceExhausted, DeadlineExpired }

public readonly struct QfcDequeueBatch
{
    public QfcDequeueBatch(IList<MailItem> items, QfcDequeueStop stop, int scanned) { ... }
    public IList<MailItem> Items { get; }
    public QfcDequeueStop Stop { get; }
    public int Scanned { get; }
}
```

- Implementers to update: `QfcDatamodel` only — the implementation lands in `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` (owned).
- Callers to update: `QfcHomeController.Iteration.cs:21` only. All four other call sites (including the two outside the owned set) keep using the existing overloads unchanged.
- Source-breaking: **no.** Adding a member to an interface with exactly one in-repo implementer, which we own, is not breaking for any other production file; `Mock<IQfcDatamodel>` picks the member up automatically and returns `default(QfcDequeueBatch)` unless a test sets it up.
- `net481` constraint: `readonly struct` is fine; `init` accessors and `record` are **not** available on net48/net481 (no `IsExternalInit`). Use a constructor plus get-only properties. (Precedent recorded for `ResourceTimingRow`.)
- Existing pinned test impact: `QfcHomeControllerIterationTests.cs` — the same four tests as Option A need their setups retargeted, because `IterateQueueAsync` no longer calls the 2-arg overload. `IterateQueueAsync_QueueEmpty` (`:123-182`) additionally has to state which stop reason it is simulating; it becomes two tests (exhausted -> `CompleteAddingAsync` once; deadline-expired -> `CompleteAddingAsync` never).

**Option C — change the return type of the existing overloads.** Rejected. Source-breaking at all five production call sites (two outside the owned set), plus ~20 Moq `ReturnsAsync(new List<MailItem>())` setups across seven test files. No benefit over Option B.

**Option D — add an optional out/callback parameter to an existing overload.** Rejected. Adding a parameter changes the interface method's signature, so every `It.IsAny<...>()` arity-matched setup (for example `QfcHomeControllerIssue218Tests.cs:100-109`, `:204-213`; `QfcHomeControllerRunAsyncHighConfidenceTests.cs`) stops matching, silently returning `null` and producing `NullReferenceException` rather than a compile error.

**Recommendation: Option B.** It is additive, keeps all non-owned call sites untouched, makes the two causes explicitly distinguishable (which is what the defect actually is), preserves #424's bound on the post-UI scan instead of discarding it, and is the shape that also serves #427 and #426 (see §5). Option A is a legitimate fallback if the epic wants the absolute-minimum diff, but it deletes the deadline rather than interpreting it.

The corrected guard in `IterateQueueAsync` becomes:

```csharp
var batch = await _datamodel.DequeueNextItemGroupWithOutcomeAsync(...);
if (batch.Items.Count > 0) { await QfcQueue.EnqueueAsync(batch.Items, _formController.Groups)...; }
else if (batch.Stop == QfcDequeueStop.SourceExhausted) { await QfcQueue.CompleteAddingAsync(Token, 10000); }
// DeadlineExpired: leave the queue open; a later iteration will supply items.
```

### 1.5 Is `CompleteAdding()` genuinely irreversible here?

Yes. `QuickFiler/Controllers/QfcQueue.cs:46-71`:

```csharp
46        public async Task CompleteAddingAsync(CancellationToken token, int timeout)
47        {
48            CancellationTokenSource functionTimeoutSource = new CancellationTokenSource(timeout);
49            CancellationTokenSource linkedTokenSource =
50                CancellationTokenSource.CreateLinkedTokenSource(token, functionTimeoutSource.Token);
51
52            try
53            {
54                while (_jobsRunning > 0)
55                {
56                    //logger.Debug(...)
57                    await Task.Delay(100, linkedTokenSource.Token);
58                }
59                _queue.CompleteAdding();
60            }
61            catch (OperationCanceledException e) { ... throw e; }
71        }
```

`_queue` is `BlockingCollection<(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)>` at `QfcQueue.cs:38-39`. `BlockingCollection<T>.CompleteAdding()` sets `IsAddingCompleted` permanently; there is no reopen API, and `QfcQueue` never reassigns `_queue` except in `ChangeIterationSize` (`QfcQueue.cs:501`, which builds a fresh collection but is only reached from the items-per-iteration change flow). So once `IterateQueueAsync` reaches `:35` the session's queue is closed. This is the only production caller of `CompleteAddingAsync`.

Note also that `CompleteAddingAsync` throws on timeout (`:69`); `QfcQueueCoverageExpansionTests.cs:178-190` pins "throws and leaves queue open". That existing pin is unaffected by the proposed fix.

### 1.6 Testability seams for the guard

Already available, no new seam required:

- `IQfcDatamodel` is fully mockable. `QfcHomeControllerIterationTests.cs` already drives `IterateQueueAsync` entirely through `Mock<IQfcDatamodel>` + `Mock<IQfcQueue>` + a reflection-set `_formController` (`:153-160`). A test that returns `QfcDequeueBatch(empty, DeadlineExpired, n)` and asserts `CompleteAddingAsync` was **never** called is a pure-mock test with no clock at all.
- `QfcHomeController` already owns an injectable `internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;` at `QuickFiler/Controllers/QfcHomeController.Metrics.cs:17` — same partial class as `QfcHomeController.Iteration.cs`, so it is in scope if ever needed. It is set through `QfcHomeController.cs:41`/`:54` (`LaunchAsync(..., TimeProvider timeProvider = null)`).
- For the *gate-level* reason test, `FakeTimeProvider` (`Microsoft.Extensions.Time.Testing`) is already the established mechanism: `QfcStreamingDequeueConfidenceGateTests.Part2.cs:36-69` builds a low-yield gate whose score loader advances the fake clock by one second per candidate. A new test asserting `Stop == DeadlineExpired` fits that helper directly.
- For the *datamodel-level* wiring test, `QfcDatamodelTests.cs:231-241` supplies `CreateUninitializedDatamodel()` (`FormatterServices.GetUninitializedObject`) plus `SetPrivateField`, and `model.TimeProvider = fake`.

**Nothing needs to be added.** The only cost is that the gate's own `DequeueAsync` must start returning the reason — see §5 for the reflection hazard that creates in the test helper.

---

## 2. Defect #448 — `QfcFormController.UndoConsumer` non-terminating loop (HANG)

### 2.1 Current loop, verbatim

`QuickFiler/Controllers/QfcFormController.Actions.cs:253-292`:

```csharp
253        internal async Task UndoConsumer()
254        {
255            var sw = new Stopwatch();
256            sw.Start();
257            bool exit = false;
258            while (!_undoQueue.IsCompleted || exit)
259            {
260                if (_undoQueue.TryTake(out var item))
261                {
262                    var helper = await MailItemHelper.FromMailItemAsync(
263                        item.MailItem,
264                        _globals,
265                        default,
266                        true
267                    );
268                    (await _globals.AF.Manager["Folder"]).UnTrain(
269                        helper.FolderInfo.RelativePath,
270                        helper.Tokens,
271                        1
272                    );
273                    var mail = item.UndoMove();
274                    await UiThread.Dispatcher.InvokeAsync(
275                        () => _groups.AddItemGroup(mail),
276                        System.Windows.Threading.DispatcherPriority.ContextIdle
277                    );
278                }
279                else if (sw.ElapsedMilliseconds > 10000)
280                {
281                    exit = true;
282                }
283                else
284                {
285                    await Task.Delay(200);
286                }
287            }
288            if (exit)
289            {
290                _undoConsumerTask = null;
291            }
292        }
```

**Both described conditions are still exactly as reported:** `while (!_undoQueue.IsCompleted || exit)` at `:258`, and `else if (sw.ElapsedMilliseconds > 10000)` at `:279`.

### 2.2 Does anything call `CompleteAdding()` on `_undoQueue`?

**No.** A repo-wide grep over `**/*.cs` for `CompleteAdding` returns exactly these production sites:

- `QuickFiler/Controllers/QfcQueue.cs:59` — `_queue.CompleteAdding()` (the UI queue, unrelated)
- `QuickFiler/Controllers/IQfcQueue.cs:24` and `QuickFiler/Controllers/IQfcQueue1.cs:27` — `CompleteAddingAsync` declarations
- `QuickFiler/Controllers/QfcHomeController.Iteration.cs:35` — the #446 call

`_undoQueue` is declared `private BlockingCollection<IMovedMailInfo> _undoQueue = [];` at `QuickFiler/Controllers/QfcFormController.cs:90`. Its only other references are `Add` at `Actions.cs:232`, `TryTake` at `Actions.cs:260`, and `Dispose` at `QfcFormController.SetupDisposal.cs:216`. `BlockingCollection<T>.IsCompleted` is `IsAddingCompleted && Count == 0`; since `CompleteAdding()` is never called, `IsAddingCompleted` is permanently `false`, so `IsCompleted` is permanently `false` and `!_undoQueue.IsCompleted` alone holds the loop condition true regardless of `exit`.

### 2.3 The busy-spin

After `sw.ElapsedMilliseconds > 10000` first becomes true, each subsequent iteration:

1. evaluates `!_undoQueue.IsCompleted` -> `true`, so the loop continues;
2. `_undoQueue.TryTake(out item)` -> `false` on an empty queue;
3. takes the `else if` at `:279` (the elapsed condition remains true forever, since `sw` is never stopped or reset);
4. sets `exit = true` (already true) at `:281`;
5. **reaches no `await`** and loops immediately.

Confirmed: the `else if` branch at `:279-282` contains no `await` and no yield. The only awaits in the loop are inside the take branch (`:262`, `:268`, `:274`) and the `Task.Delay(200)` at `:285`, which is in the *third* branch and is unreachable once the threshold is crossed. The result is a tight CPU-bound spin on a thread-pool thread.

### 2.4 `_undoConsumerTask` reset reachability

`_undoConsumerTask` is declared `private Task _undoConsumerTask;` at `QuickFiler/Controllers/QfcFormController.cs:91`.

The `??=` guard is at `QuickFiler/Controllers/QfcFormController.Actions.cs:211`:

```csharp
211            _undoConsumerTask ??= Task.Run(UndoConsumer);
```

inside `UndoDialog()` (`:204-251`), guarded at `:206-209` by `if (_movedItems is null || _globals?.Ol?.App is null) return;`.

The reset at `:288-291` is **unreachable in any normal termination**, because the `while` at `:258` never exits normally. `_undoConsumerTask` therefore stays non-null for the life of the controller and the `??=` guard prevents any later `UndoDialog()` from starting a fresh consumer.

There is one abnormal exit: `QfcFormController.SetupDisposal.cs:216` disposes `_undoQueue` in `Cleanup()` without ever calling `CompleteAdding()` and without cancelling or awaiting `_undoConsumerTask`. A spinning consumer then calls `TryTake` on a disposed `BlockingCollection<T>`, which throws `ObjectDisposedException`; the task faults, the exception is never observed (the task is fire-and-forget), and `:288-291` is skipped because the throw unwinds out of the loop body. `Cleanup()` also sets `_globals = null` (`:217`) and `_groups = null` (`:220`) while the consumer may still be in its take branch. This is a secondary hazard worth covering in the fix, not a separate defect.

`UndoDialog()` is reached from `QfcFormController.EventHandlers.cs:236-238` and `:241-243` (`ButtonUndo_Click`), wired at `QfcFormController.SetupDisposal.cs:172`. Note `QfcFormController.EventHandlers.cs` and `QfcFormController.SetupDisposal.cs` are **not** in the owned file set.

### 2.5 The claimed #435/F6 start-delegate seam — IT DOES NOT EXIST

The potential document states F6 "works around it by introducing an injectable start-delegate around `Task.Run(UndoConsumer)`". **That seam is not present at `988e819b`.** Evidence:

- `QfcFormController.Actions.cs:211` is a bare `_undoConsumerTask ??= Task.Run(UndoConsumer);` with no delegate indirection.
- Repo-wide grep for `UndoConsumer` finds only `Actions.cs:211`, `Actions.cs:253`, and `QfcFormControllerTests.cs:688`. No `Func<Task>`/`Action` starter field, property, or parameter exists.
- `QfcFormControllerSeamTests.cs` documents only "Seam B" (intent command events / skip-button state) and "Seam D" (`CaptureTlpCellStates` / `GetKeyEventExclusionControls`) — `:16-24`, `:132`, `:243`, `:289`.
- `QfcFormControllerTests.cs:687-701` (`UndoConsumer_ShouldConsumeUndoQueue`) is a tautological placeholder (`await Task.CompletedTask; Assert.IsTrue(true);` under a narrow `MSTEST0032` suppression).
- No feature folder matching `docs/features/**/*435*` exists in this worktree.

There is no evidence that the loop is ever started during a test run today: `UndoDialog_ShouldUndoMoves` (`QfcFormControllerTests.cs:674-685`) constructs the controller through `CreateQfcFormController()` (`:75-87`), whose `Mock<IAppAutoFileObjects>` never sets up `MovedMails`, so `_movedItems` is `null` and `UndoDialog()` returns at the `:206` guard before reaching `:211`.

**Conclusion:** this feature must build the seam from scratch. Treat the potential document's F6 sentence as stale.

### 2.6 Required determinism seams (no wall-clock wait)

`.claude/rules/general-unit-test.md` § "Determinism Infrastructure" bans `Thread.Sleep`, `Task.Delay`, real wall-clock waits and `Date.now()`/`DateTime.Now` in test code, and requires an injected `TimeProvider` plus `FakeTimeProvider` for .NET. `UndoConsumer` currently uses two banned constructs *in production*, both of which a test would otherwise have to wait on: `new Stopwatch()` at `:255` and `await Task.Delay(200)` at `:285`.

Both are replaceable by the **one** seam the repository already uses everywhere else — `System.TimeProvider` — which supplies elapsed-time measurement *and* delay from a single injectable object:

```csharp
// Declared in the OWNED partial file QfcFormController.Actions.cs (legal: QfcFormController
// is a partial class, so no edit to the non-owned QfcFormController.cs is required).
/// <summary>
/// Injectable time/delay seam (issue #448). Defaults to <see cref="TimeProvider.System"/> so
/// production timing is unchanged; tests assign a FakeTimeProvider to drive the idle-exit
/// path with no wall-clock wait. Mirrors QfcDatamodel.TimeProvider and
/// QfcHomeController.TimeProvider.
/// </summary>
internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;
```

Loop rewrite using it:

```csharp
internal async Task UndoConsumer()
{
    long start = TimeProvider.GetTimestamp();
    while (!_undoQueue.IsCompleted)
    {
        if (_undoQueue.TryTake(out var item)) { ...; start = TimeProvider.GetTimestamp(); }
        else if (TimeProvider.GetElapsedTime(start) > IdleTimeout) { break; }
        else { await TimeProvider.Delay(TimeSpan.FromMilliseconds(200)).ConfigureAwait(false); }
    }
    _undoConsumerTask = null;
}
```

Why `TimeProvider` and not two separate seams (an `IClock` plus a `Func<TimeSpan, Task>` delay delegate):
- `TimeProvider.GetTimestamp()` / `GetElapsedTime(long)` is precisely the `Stopwatch` replacement, and it is the pattern already proven in this exact subsystem at `QfcStreamingDequeueConfidenceGate.cs:102`, `:110`.
- `TimeProvider.Delay(TimeSpan, CancellationToken)` is the `Task.Delay` replacement, already used at `QfcDatamodel.QueueProcessing.cs:173`, `QfcDatamodel.FrameBuilding.cs:43`, `QfcHomeController.Metrics.cs:222`, and `QfcStreamingDequeueConfidenceGate.cs:126-128`.
- Both packages are already referenced: `Microsoft.Bcl.TimeProvider 10.0.11` and `Microsoft.Extensions.TimeProvider.Testing` in `QuickFiler.Test/packages.config:18` and `:85`.
- Inventing a second abstraction would be a new seam shape competing with a repo-wide convention.

The `Task.Run(UndoConsumer)` start site also needs a seam so a test can drive the loop synchronously without a background thread:

```csharp
// Also declared in the owned QfcFormController.Actions.cs partial.
internal Func<Func<Task>, Task> UndoConsumerStarter { get; set; } = body => Task.Run(body);
// call site at :211 becomes:
_undoConsumerTask ??= UndoConsumerStarter(UndoConsumer);
```

A test then sets `UndoConsumerStarter = body => body();` to run the loop inline on the test's own continuation chain, and advances a `FakeTimeProvider` to cross the idle threshold. **Injection point for both properties: the owned file `QfcFormController.Actions.cs`.** No edit to `QfcFormController.cs` (field declarations) is required, because C# allows a partial class's members to be split across files; the existing `_undoQueue`/`_undoConsumerTask` fields stay where they are.

### 2.7 Correct loop condition and idle-exit semantics

- Condition: `while (!_undoQueue.IsCompleted)` and an explicit `break` on idle timeout, **or** the conjunction the potential document suggests, `while (!_undoQueue.IsCompleted && !exit)`. Either is correct; the `break` form is clearer and makes `exit` unnecessary.
- Idle timer must be **reset after every successful take** (`start = TimeProvider.GetTimestamp()` inside the take branch). The current code never resets `sw`, so a long-running undo session would cross the 10 s threshold while still productive. This is a latent second bug in the same loop; fixing the condition without resetting the timer would convert a hang into a premature exit.
- `_undoConsumerTask = null;` must run unconditionally on exit (and ideally in a `finally`), so a later `UndoDialog()` can start a fresh consumer.
- The empty-queue path must always yield: only the take branch and the delay branch may loop without a break.

**Existing pinned tests broken by this change: none.** The sole `UndoConsumer` test (`QfcFormControllerTests.cs:687-701`) is a tautology that asserts nothing about the loop, and `UndoDialog_ShouldUndoMoves` (`:674-685`) never reaches `:211`. Replacing the placeholder with a real test is an improvement, not a contract change; it is also the natural place to retire the `MSTEST0032` suppression at `:698-700`.

---

## 3. Defect #426 — `EmailMoveMonitor` rejected-item hook retention (COM retention)

### 3.1 Current dequeue paths

| Member | Current location | Notes |
| --- | --- | --- |
| `UnhookDequeuedNodes` | `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:145-166` | private; iterates `nodes` and calls `TryUnhookOrReplace` at `:157` |
| `TryUnhookOrReplace` | `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:29-64` | `internal`; guard `:31-37`; retry loop `:40-63` |
| `_moveMonitor.UnhookItem(node)` | `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:44` | inside `TryUnhookOrReplace`'s try block |

`UnhookDequeuedNodes` is invoked from three places, all in the owned `QueueProcessing.cs`: `:107` (`DequeueDirectAsync`), `:129` (`DequeueWithHighConfidenceGateAsync`), `:142` (`DequeueNextItemGroup`). It only ever sees the **accepted** list.

### 3.2 The gate's take delegate and where rejected candidates are dropped

`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:110-130`:

```csharp
110        private async Task<IList<MailItem>> DequeueWithHighConfidenceGateAsync(
111            int quantity, int timeOut, TimeSpan? firstBatchDeadline = null,
114            Action<int, int, int> progress = null)
115        {
117            var gate = new QfcStreamingDequeueConfidenceGate(
118                () => _masterQueue.TryTakeFirst(),
119                ScoreRemainingQueueMailItemAsync,
120                _globals.QfSettings.HighConfidenceThreshold,
121                TimeProvider,
122                null,
123                () => _remainingLoadActive,
124                firstBatchDeadline,
125                progress
126            );
127
128            var nodes = (await gate.DequeueAsync(quantity, timeOut, _token)).ToList();
129            return UnhookDequeuedNodes(nodes);
130        }
```

The potential document's `() => _masterQueue.TryTakeFirst()` is confirmed, now at **`:118`** (was `:82`).

Rejected candidates are dropped in `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs:133-145`:

```csharp
133                long score = await _scoreLoader(mailItem, token).ConfigureAwait(false);
134                token.ThrowIfCancellationRequested();
135                scanned++;
136                LogScore(mailItem, score);
137
138                if (score >= _cutoff)
139                {
140                    accepted.Add(mailItem);
141                }
142
143                // Report after the accept decision so `accepted` reflects this candidate. ...
145                _progressCallback?.Invoke(scanned, accepted.Count, quantity);
```

There is **no `else`**. A below-cutoff item has already been removed from `_masterQueue` by `_tryTakeNext()` at `:116`, is never added to `accepted`, and therefore never reaches `UnhookDequeuedNodes`. Its `EmailMoveAction` entry stays in `_hookedItems` for the session.

### 3.3 `EmailMoveMonitor` hook lifecycle and the STA thread-affinity contract

| Member | Location |
| --- | --- |
| `_marshalToSta` field | `QuickFiler/Helper Classes/EmailMoveMonitor.cs:29` |
| Constructor + marshal seam default | `:38-42` (`_marshalToSta = marshalToSta ?? (action => UiThread.Dispatcher.Invoke(action));`) |
| `_hookedItems` | `:44` (`private List<EmailMoveAction> _hookedItems = [];`) |
| `HookItem` | `:46-61` — marshals; `folder.BeforeItemMove += BeforeItemMove` at `:57`; `_hookedItems.Add(new EmailMoveAction(mail, folder, moveAction))` at `:58` |
| `UnhookItem` | `:63-88` — null guard `:65-68`; marshals `:72-87`; `-=` at `:83`; `_hookedItems.Remove` at `:84` |
| `UnhookItemAsync` (dormant) | `:90-124` |
| `UnhookAll` | `:185-200` — marshals; per-item `-=` at `:195`; `_hookedItems.Clear()` at `:197` |
| `BeforeItemMove` handler body | `:204-223` |
| `EmailMoveAction` | `:226-261`; ctor reads `mail.EntryID`/`folder.EntryID` at `:239-240` |
| Interface contract | `QuickFiler/Interfaces/IEmailMoveMonitor.cs:13-38` |

**Thread-affinity contract (issues #214 and #420), stated exactly.** The contract is documented in the interface XML doc at `IEmailMoveMonitor.cs:6-12` ("All Outlook COM member access performed by implementations is marshaled to the captured Outlook STA thread, so callers may invoke these members from any thread ... without raising cross-thread COMException") and in the class doc at `EmailMoveMonitor.cs:24-28`.

Operations that **must** run on the captured STA thread (all currently inside a `_marshalToSta(...)` lambda):
- `mail.Parent` cast to `Folder` (`:54`, `:75`, `:142`)
- `folder.EntryID` / `mail.EntryID` reads (`:55`, `:74`, `:75`, `:110-111`, `:160`, and inside `EmailMoveAction`'s ctor at `:239-240`)
- `folder.BeforeItemMove += / -=` (`:57`, `:83`, `:119`, `:195`)
- construction of `EmailMoveAction` (`:58`) — because its ctor performs the EntryID reads

Operations that **must not** be marshalled (they are pure bookkeeping and are deliberately performed under the `lock (_hookedItems)` *inside* the marshalled block so that lock and COM access share one critical section):
- the null-argument early return in `UnhookItem` (`:65-68`) — verified by `EmailMoveMonitorTests.cs:134-145`, which asserts `_marshalInvocationCount == 0` for a null call
- LINQ over the cached `MailEntryId`/`FolderEntryId` strings (`:56`, `:78-79`, `:114-115`) — these read the cached strings captured at hook time, **not** live COM
- `_hookedItems.Add` / `.Remove` / `.Clear`

The invariant the fix must preserve is: **exactly one `_marshalToSta` invocation per public operation**, and no COM member touched outside it. `EmailMoveMonitorTests.cs:176-198` (`AllComAccess_FlowsThroughInjectedMarshalDelegate`) pins the counts at 1 / 1 / 1 for `HookItem` / `UnhookItem` / `UnhookAll`. Any fix that calls `UnhookItem` once per rejected item is automatically compliant — it adds one marshal hop per rejection and preserves the shape. A fix that batched several unhooks into one marshal hop would need a new monitor member and would break that pinned test.

### 3.4 Additional finding: three independent monitor instances

Not in the potential document, and material to the fix. `EmailMoveMonitor` is field-initialised in three unrelated places and never injected in production:

- `QuickFiler/Controllers/QfcDatamodel.cs:103` — `private IEmailMoveMonitor _moveMonitor = new EmailMoveMonitor();` (hooks at `:357`, `:400`, `:452`; unhooks at `QueueProcessing.cs:44`; `UnhookAll` at `QfcDatamodel.cs:80`)
- `QuickFiler/Controllers/QfcQueue.cs:40` — separate instance (hooks at `:230`; unhooks at `:76`, `:130`)
- `QuickFiler/Controllers/QfcCollectionController.cs:78` — separate instance (hooks at `:256`, `:284`, `:364`, `:451`, `:1942`; unhooks at `:1124`, `:1187`; `UnhookAll` at `:1007`)

Consequence: the `_moveMonitor.UnhookItem(group.MailItem)` at `QfcQueue.cs:76` can never match a hook registered by the datamodel, because it consults a different `_hookedItems` list. This does **not** change the #426 fix (the datamodel's own monitor is the one retaining the rejected items), but it means the fix must call the **datamodel's** `_moveMonitor`, and it means no other component can incidentally release those hooks. Tests inject by reflection: `SetPrivateField(model, "_moveMonitor", moveMonitor.Object)` — `QfcQueuePurePathsTests.cs:125`, `QfcQueueCoverageExpansionTests.cs:119`/`:145`/`:207`, `QfcCollectionControllerTests.cs:359`.

### 3.5 Evaluation of the three candidate fix directions

**Direction 1 — take delegate that unhooks on rejection.** Not implementable as stated: the take delegate runs *before* scoring, so it cannot know whether the item will be rejected. The only way to express it is to unhook at take time and re-hook accepted items, which is Direction 3.

**Direction 2 (RECOMMENDED) — explicit rejection callback on the gate, wired by the datamodel to `_moveMonitor.UnhookItem`.**

- Shape: add an optional final constructor parameter `Action<MailItem> onRejected = null` to `QfcStreamingDequeueConfidenceGate` (`QfcStreamingDequeueConfidenceGate.cs:55-64`) and invoke it in a new `else` at `:138-141`.
- Touched files: `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` (owned) and `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:117-126` (owned).
- **Stays entirely inside the owned set.** No non-owned file changes.
- Thread affinity: preserved unconditionally. The callback is `_moveMonitor.UnhookItem`, which self-marshals at `EmailMoveMonitor.cs:72`. The gate calls it from whatever thread it is on; that is exactly the calling convention the interface documents.
- Failure isolation: wrap the callback invocation so a monitor failure cannot abort the scan (the accepted-path equivalent, `TryUnhookOrReplace` at `QueueProcessing.cs:40-63`, already has retry/recovery logic). A `try { onRejected(item); } catch (Exception e) { logger.Error(...); }` around the single call is the minimal equivalent; do **not** reuse `TryUnhookOrReplace`, whose recovery path pulls a replacement item out of `_masterQueue` — semantics that make no sense for a discarded candidate.
- Testability with Moq: excellent. The gate is already constructed reflectively in tests (`QfcStreamingDequeueConfidenceGateTests.cs:26-156`), so a rejection-callback test asserts the delegate fired once per below-cutoff candidate. The datamodel-level wiring test uses the `SetPrivateField(model, "_moveMonitor", Mock<IEmailMoveMonitor>)` pattern already proven at `QfcQueuePurePathsTests.cs:104-134`.

**Direction 3 — unhook at take, re-hook accepted.** Rejected. It doubles the marshal hops for accepted items (unhook + re-hook, two STA round-trips per accepted item on top of the existing one in `UnhookDequeuedNodes`), it changes the accepted-path behaviour that `QfcQueuePurePathsTests.cs:119-133` pins (`UnhookItem` exactly once per accepted item), and a re-hook rebuilds `EmailMoveAction` with a fresh `mail.Parent` read — which for an item mid-move could resolve to a different folder. Higher risk, no benefit.

**Recommendation: Direction 2.**

### 3.6 The drop-on-reject contract must not change

`DequeueAsync_BelowThresholdItemsAreDiscarded` **still exists** and still pins the contract, now at `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs:298-310`:

```csharp
298        [TestMethod]
299        public async Task DequeueAsync_BelowThresholdItemsAreDiscarded()
300        {
301            var item = CreateMailItem("discard", "entry-discard");
302            object gate = CreateGate(
303                new Queue<MailItem>(new[] { item }),
304                new Dictionary<MailItem, long> { [item] = 899 }
305            );
306
307            IList<MailItem> result = await DequeueAsync(gate, 1, 0, CancellationToken.None);
308
309            result.Should().BeEmpty();
310        }
```

Direction 2 leaves this assertion true: the rejected item is still absent from the result, it is still gone from the source queue, and the only new observable is that the monitor hook is released. Do not modify this test.

---

## 4. Defect #427 — post-`Show()` duplicate scoring (LOW)

### 4.1 `ScoreRemainingQueueMailItemAsync` drops the computed `TopFolder`

`QuickFiler/Controllers/QfcDatamodel.cs:363-377` (**moved** from the cited `:346-360`):

```csharp
363        private async Task<long> ScoreRemainingQueueMailItemAsync(
364            MailItem mailItem,
365            CancellationToken cancel
366        )
367        {
368            var scoringService = new FolderScoringService();
369            var score = await scoringService
370                .ScoreAsync(mailItem, _globals, cancel)
371                .ConfigureAwait(false);
372            logger.Debug(
373                $"Probability debug [QfcDatamodel.ScoreRemainingQueueMailItemAsync (master-queue admission)] "
374                    + $"Subject='{mailItem.Subject}' EntryID='{mailItem.EntryID}' Score={score.Score}"
375            );
376            return score.Score;
377        }
```

`FolderScoringService.ScoreAsync` return shape — `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:143-147` (interface) and `:170-189` (implementation):

```csharp
143        Task<(long Score, string TopFolder)> ScoreAsync(
144            MailItem mailItem,
145            IApplicationGlobals globals,
146            CancellationToken token
147        );
```

```csharp
178            var helper = await MailItemHelper.FromMailItemAsync(mailItem, globals, token, false);
179            var predictor = new FolderPredictor(globals, helper, FolderPredictor.InitOptions.FromField);
184            predictor = await predictor.InitAsync(helper, FolderPredictor.InitOptions.FromField);
185
186            long score = predictor.Suggestions.TopScore();
187            string topFolder = predictor.Suggestions.ToArray(1).FirstOrDefault() ?? string.Empty;
188            return (score, topFolder);
```

Confirmed: `TopFolder` is computed at `:187` and discarded at `QfcDatamodel.cs:376`. Note also that a **fully-initialised `FolderPredictor`** is built at `:179-184` and discarded at `:189` — only two scalars survive.

### 4.2 `LoadFolderHandlerAsync` re-runs `FolderPredictor` with `InitOptions.FromField`

`QuickFiler/Controllers/QfcItemController.FolderHandling.cs:57-131` (the potential cited `:57-90`; the body is longer now). The relevant hunk `:64-85`:

```csharp
64                    _folderHandler = await Task.Run(
65                            async () =>
66                            {
67                                var fp = _folderPredictorFactory(
68                                    _globals,
69                                    ItemHelper.ThrowIfNull(),
70                                    FolderPredictor.InitOptions.FromField
71                                );
72
73                                return await fp.InitAsync(
74                                    ItemHelper,
75                                    FolderPredictor.InitOptions.FromField
76                                );
77                            },
78                            cancel
79                        )
80                        .ConfigureAwait(false);
81                    logger.Debug(
82                        $"Probability debug [QfcItemController.LoadFolderHandlerAsync (FromField)] "
                              ...
85                    );
```

This is the same `FolderPredictor` + `InitAsync(FromField)` sequence `FolderScoringService.ScoreAsync` already ran. Both `Probability debug` lines described in the potential document are still emitted.

### 4.3 The carrier type and both `LoadItemsAsync` overloads

`QfcPreScoredItem` — `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:98-122`; `public readonly struct`; `MailItem` at `:115`, `PredeterminedFolder` at `:121`.

Overloads in the owned `QuickFiler/Controllers/QfcFormController.Actions.cs`:

| Overload | Lines |
| --- | --- |
| `public async Task LoadItemsAsync(IList<MailItem> listObjects)` | `62-65` (delegates to the two-arg form) |
| `public async Task LoadItemsAsync(IList<MailItem> listObjects, ProgressTracker progress)` | `67-105` |
| `public async Task LoadItemsAsync(IList<QfcPreScoredItem> preScored)` | `114-117` (XML doc `107-113`) |
| `public async Task LoadItemsAsync(IList<QfcPreScoredItem> preScored, ProgressTracker progress)` | `120-164` (XML doc `119`) |

**Confirmed: the pre-scored overload is dormant on the live path.** The exact call site that selects the plain overload is `QuickFiler/Controllers/QfcHomeController.cs:310`:

```csharp
310            await _formController.LoadItemsAsync(listEmail);
```

where `listEmail` is `IList<MailItem>` produced at `:283-290` (`InitEmailQueueAsync`) and possibly replaced at `:299-304` by the high-confidence gate result. The synchronous sibling `Run()` selects the plain `LoadItems(IList<MailItem>)` at `QfcHomeController.cs:266`.

`QfcHomeController.cs` is **NOT in this feature's owned set.**

The rest of the carrier chain is fully built and functional, just unreachable: `QfcCollectionController.LoadControlsAndHandlers_01Async(IList<QfcPreScoredItem>, ...)` at `QfcCollectionController.cs:428` copies `scored.PredeterminedFolder` at `:471` into `QfcItemGroup.PredeterminedFolder` (`QfcItemGroup.cs:50`, set at `QfcCollectionController.cs:616`), which reaches `QfcItemController._predeterminedFolder` (declared `QfcItemController.cs:248`, assigned `QfcItemController.Initialization.cs:108`) and is consumed in the owned `QfcItemController.FolderHandling.cs:193-199`.

`HighConfidencePreFilterLoader` (`QfcHomeController.cs:236-244`) is likewise dormant — no production caller; referenced only by tests.

### 4.4 The pinned overload-selection tests (treat as specification)

**`QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs`** (261 lines):

- `RunAsync_HighConfidenceEnabled_DoesNotPreFilterInitialGuiBatch` — `:137-182`. Asserts (a) `HighConfidencePreFilterLoader` is **not** invoked, with the rationale "remaining-queue admission now owns high-confidence filtering" (`:157-159`); (b) `LoadItemsAsync(IList<MailItem>)` `Times.Once` with "the initial GUI batch must use the plain MailItem load path" (`:160-164`); (c) the 4-arg `DequeueNextItemGroupAsync` `Times.Once`, "the first displayed page must come from the dequeue-layer gate" (`:165-176`); (d) `LoadItemsAsync(IList<QfcPreScoredItem>)` **`Times.Never`**, "RunAsync must not use the carrier-list overload for the initial batch" (`:177-181`).
- `RunAsync_HighConfidence_LoadsInitialBatchWithoutPreFilter` — `:184-259`. Asserts the invocation sequence equals exactly `["LoadItemsAsync"]` (`:244`) and re-pins `LoadItemsAsync(IList<QfcPreScoredItem>) Times.Never` (`:255-258`).

**`QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs`** (473 lines) carries the same carrier-overload `Times.Never` pins at `:246` and `:277`.

**`QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs:352-374`** — `LoadItemsAsync_MailItemPath_DoesNotApplyPostDisplayHighConfidenceRemoval`. This is a **source-text** test: it reads `QuickFiler/Controllers/QfcFormController.Actions.cs` from disk via `ReadControllerSource` / `ResolveRepositoryPath` (`:59-84`) and requires two exact literals to be present, in this order:
- `"public async Task LoadItemsAsync(IList<MailItem> listObjects, ProgressTracker progress)"` (currently `Actions.cs:67`, on a single line)
- `"public async Task LoadItemsAsync(IList<QfcPreScoredItem> preScored)"` (currently `Actions.cs:114`, on a single line)

and asserts the substring between them contains neither `ApplyHighConfidenceFilterAsync` nor `RemoveBelowThresholdAsync`. **Any edit to the owned `QfcFormController.Actions.cs` that reflows either signature across lines, reorders the two overloads, or inserts the named identifiers into the `IList<MailItem>` region will break this test.** CSharpier reflows a signature only when it exceeds the print width — both are currently within it, so a parameter addition to either signature is the specific risk.

**What deliberately updating these pins would mean.** They are the landed decision of issue #233: high-confidence enforcement moved from post-display filtering to dequeue-time gating, and the carrier-list path was deliberately parked. Activating the carrier overload reverses part of that decision. It is a legitimate change, but it must be argued and the four assertions rewritten (not deleted), stating that the carrier list is now the high-confidence load path and that dequeue-time enforcement is still the *filtering* mechanism. The `Times.Never` on `HighConfidencePreFilterLoader` should stay — the pre-filter class remains dormant; only the carrier *overload* would become live.

### 4.5 Can the #427 fix be confined to the owned files? **NO — flagged loudly.**

Two independent blockers:

**Blocker 1 — the overload-selection call site is not owned.** Only `QfcHomeController.cs:310` decides which `LoadItemsAsync` runs. That file belongs to a sibling epic child. Changing it here would collide with the sibling.

**Blocker 2 (more serious) — activating the carrier overload does NOT eliminate the duplicate scoring.** This is a correction to the potential document's premise. `_predeterminedFolder` is consumed only for *selection* inside `AssignFolderComboBox` (`QfcItemController.FolderHandling.cs:193-199`). The surrounding code still requires a fully-initialised predictor:

```csharp
170            if (_folderHandler?.FolderArray?.Length > 0)
...
182                _itemViewer.SetFolderItems(_folderHandler.FolderArray);
...
189                if (_folderHandler.Suggestions != null)
190                {
191                    _itemViewer.SetFolderSuggestions(_folderHandler.FolderRowArray);
192                }
```

`FolderArray`, `Suggestions` and `FolderRowArray` all come from `_folderHandler` (`IFolderSearchHandler`, declared `QfcItemController.cs:41`), which is produced only by `LoadFolderHandler`/`LoadFolderHandlerAsync`. So even on the carrier path the item controller must still run `FolderPredictor.InitAsync(FromField)`. Carrying only `TopFolder` forward changes which entry is preselected — a behaviour the code *already* implements — and saves nothing.

Genuinely removing the second scoring pass requires carrying the **initialised `FolderPredictor` / `IFolderSearchHandler`** from `FolderScoringService.ScoreAsync` (`QfcHighConfidencePreFilter.cs:184`, where it is discarded) all the way to `_folderHandler`. That touches, at minimum:
- `QfcHighConfidencePreFilter.cs` (widen `IFolderScoringService.ScoreAsync`'s tuple, or return the predictor) — *not owned*
- `QfcItemGroup.cs:50` (new carried member) — *not owned*
- `QfcCollectionController.cs:428-471`, `:616` — *not owned*
- `QfcItemController.cs:41`/`:83-89`, `QfcItemController.Initialization.cs:63-64`/`:108`/`:398-400` (constructor and factory) — *not owned*
- `QfcHomeController.cs:310` — *not owned*

**Recommended disposition for #427.** Do not attempt the full fix in this feature. Two viable scopes:

- **Scope 427-A (recommended): producer-side only, inside the owned set.** Stop discarding `TopFolder` in `QfcDatamodel.cs:363-377` and carry `(score, topFolder)` through the gate so the datamodel can expose `IList<QfcPreScoredItem>` on the new `QfcDequeueBatch` (§5). Nothing consumes it yet. This closes the "the folder is discarded" half of the defect, lands the contract the sibling children need, and requires **zero** changes to non-owned files and **zero** changes to the pinned Issue218 / RunAsyncHighConfidence tests, because `QfcHomeController.RunAsync` still calls the plain overload. Record the consumer-side work (activating `LoadItemsAsync(IList<QfcPreScoredItem>)` and threading the predictor) as a follow-up issue against the sibling children's files.
- **Scope 427-B: full fix, cross-child.** Requires an explicit coordination agreement with the sibling children that own `QfcHomeController.cs`, `QfcCollectionController.cs`, `QfcItemController.cs`, and `QfcItemController.Initialization.cs`, plus deliberate rewriting of five pinned assertions. Do not undertake it inside this feature without that agreement.

If the epic insists on closing #427 within this feature, **Scope 427-A plus a follow-up issue is the only option that does not write a non-owned file.** State that explicitly in the plan rather than silently widening the file set.

---

## 5. Cross-cutting A — one coherent result shape and the ordering constraint

All three of #446, #427 and #426 press on the same path: `QfcStreamingDequeueConfidenceGate.DequeueAsync` -> `QfcDatamodel.DequeueWithHighConfidenceGateAsync` -> `IQfcDatamodel` -> `QfcHomeController`. Three bolt-ons would mean three separate signature churns through the same reflective test helper. One shape serves all three.

### 5.1 Proposed unified shape

**Layer 1 — gate internals (`QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`, owned).**

```csharp
internal enum QfcDequeueStop { QuantitySatisfied, SourceExhausted, DeadlineExpired }

internal readonly struct QfcGateBatch          // net481: no `record`, no `init`
{
    public QfcGateBatch(IList<QfcPreScoredItem> accepted, QfcDequeueStop stop, int scanned) { ... }
    public IList<QfcPreScoredItem> Accepted { get; }   // #427: carries TopFolder
    public QfcDequeueStop Stop { get; }                // #446: the reason
    public int Scanned { get; }
}

// score loader widens from Task<long> to Task<(long Score, string TopFolder)>   -> #427
private readonly Func<MailItem, CancellationToken, Task<(long Score, string TopFolder)>> _scoreLoader;

// new optional ctor parameter, invoked in a new `else` at the accept decision -> #426
private readonly Action<MailItem> _onRejected;

internal async Task<QfcGateBatch> DequeueAsync(int quantity, int timeOut, CancellationToken token)
```

Return-site mapping: `:98` -> `QuantitySatisfied` (degenerate, empty); `:113` -> `DeadlineExpired`; `:122` -> `SourceExhausted`; `:148` -> `QuantitySatisfied`.

**Layer 2 — datamodel (`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` + `QfcDatamodel.cs`, both owned).**

- `ScoreRemainingQueueMailItemAsync` (`QfcDatamodel.cs:363-377`) changes its return type from `Task<long>` to `Task<(long Score, string TopFolder)>`, returning the whole tuple instead of `score.Score`. It has exactly two consumers: `QueueProcessing.cs:119` (the gate) and `QfcDatamodel.cs:355` (passed as a method group to `QfcRemainingQueueAdmission`).

  **Verified:** `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs` (48 lines, NOT owned) declares `Func<MailItem, CancellationToken, Task<long>> scoreLoader` at `:17`, null-checks it at `:23-26`, and **never assigns it to a field and never invokes it** — `TryQueueAsync` (`:34-46`) only calls `_addToQueue` and `_hookItem`. The parameter is dead (a legacy of the pre-#233 admission-time scoring design). Widening `ScoreRemainingQueueMailItemAsync` therefore breaks only the *method-group conversion* at `QfcDatamodel.cs:355`, which an adapter lambda fixes entirely inside the owned file:

  ```csharp
  // QfcDatamodel.cs:355 becomes:
  async (m, t) => (await ScoreRemainingQueueMailItemAsync(m, t)).Score,
  ```

  **No change to `QfcRemainingQueueAdmission.cs` is required.** (Separately: the dead `scoreLoader` parameter is a cleanup candidate for whichever sibling child owns that file — do not remove it here, as `QfcDatamodelTests.cs:21-46` constructs the type with five arguments and five tests pass throwing score loaders to prove admission never scores.)
- `DequeueWithHighConfidenceGateAsync` wires `onRejected: item => TryReleaseRejectedHook(item)` and returns the gate batch.
- `UnhookDequeuedNodes` continues to operate on the accepted `MailItem` list, unchanged.
- New public carrier at the boundary:

```csharp
public readonly struct QfcDequeueBatch
{
    public IList<MailItem> Items { get; }                 // always populated (all modes)
    public IList<QfcPreScoredItem> PreScored { get; }     // populated in high-confidence mode; empty otherwise
    public QfcDequeueStop Stop { get; }
}
```

**Layer 3 — interface (`QuickFiler/Interfaces/IQfcDatamodel.cs`, owned).** One additive member, `DequeueNextItemGroupWithOutcomeAsync`, as specified in §1.4 Option B but returning `QfcDequeueBatch`. All three existing overloads stay, unchanged, and keep delegating internally — so the four non-owned production call sites (`QfcHomeController.cs:260`, `:299`, `QfcQueue.cs:476`, plus `QfcHomeController.Iteration.cs:62` if left alone) compile untouched.

**Layer 4 — caller (`QuickFiler/Controllers/QfcHomeController.Iteration.cs`, owned).** `IterateQueueAsync` switches to the new member and gates `CompleteAddingAsync` on `Stop == SourceExhausted`.

This single shape delivers: #446 (the `Stop` field), #427 producer-side (the `PreScored` list), #426 (the `onRejected` hook), with one signature churn instead of three.

### 5.2 Ordering constraint

**Land #426 first, then #446, then #427-A.** Rationale:

1. **#426 is independent of the result shape.** It adds a constructor parameter and an `else` clause; it does not change `DequeueAsync`'s return type. Landing it first means its regression test (a plain Moq assertion on `IEmailMoveMonitor.UnhookItem`) is written against the current, simplest signature.
2. **#446 changes the gate's return type**, which invalidates the reflective cast in the test helper (see §5.3). Doing it second means exactly one migration of that helper.
3. **#427-A changes the score-loader delegate type**, which changes the gate constructor's parameter *types*, invalidating the same helper's `GetConstructor(types: ...)` arrays. Doing it third, immediately after #446, batches the two helper edits into adjacent commits.

If the plan prefers a single atomic change, do #426 + #446 + #427-A together in one phase and update the helper once. Do **not** interleave them across phases with intervening green gates, because each intermediate state re-breaks the same helper.

### 5.3 Hazard: the gate test helper fails OPEN, not closed

`QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs:26-156` builds the gate by reflection with a **descending fallback chain** of `GetConstructor(types: ...)` lookups:

- 8-type (with progress) — `:45-76`
- 7-type (with deadline) — `:78-107`
- 6-type (with sourceActive) — `:109-136`
- 5-type (base) — `:138-155`, ending in `constructor.Should().NotBeNull(...)`

Adding a ninth constructor parameter makes the 8-type lookup return `null`; the 7-type and 6-type lookups also return `null` (there is no 7- or 6-parameter constructor declared — the widest declared constructor is the 8-parameter one at `:55-64`, and the only other is the 5-parameter convenience overload at `:33-40`); the chain then **succeeds** on the 5-type lookup and silently constructs a gate with `sourceActive = null`, the default deadline, and no progress callback. Every deadline and source-active test would then be exercising a differently-configured gate while still passing or failing for the wrong reason.

`DequeueAsync` itself is retrieved by exact parameter types and cast at `:192-205`:

```csharp
202            var task =
203                (Task<IList<MailItem>>)
204                    method.Invoke(gate, new object[] { quantity, timeOut, token });
```

Changing the return type to `Task<QfcGateBatch>` makes this an `InvalidCastException` at runtime — a loud failure, which is the desirable outcome.

**Action for the plan:** every change to the gate's constructor arity or parameter types must be accompanied by an update to `CreateGate` in `QfcStreamingDequeueConfidenceGateTests.cs`, and the fallback chain should be trimmed to a single exact lookup with a `Should().NotBeNull()` guard so it fails closed. Twenty-one test methods across the three parts consume these helpers.

---

## 6. Cross-cutting B — test inventory and placement

| Test file | Lines | What it covers | Natural home for |
| --- | --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` | 317 | `QfcRemainingQueueAdmission` admission paths (`:48-219`); high-confidence dequeue liveness wait (`:102-138`); issue #222 `TimeProvider` seam tests for `ToggleOfflineMode` and `WaitForQueue` (`:221-315`). Owns `CreateUninitializedDatamodel` (`:231-232`) and `SetPrivateField` (`:234-241`). | **#426** datamodel-level wiring (rejected item reaches `_moveMonitor.UnhookItem`); **#427-A** `ScoreRemainingQueueMailItemAsync` tuple return |
| `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs` | 255 | Issue #424 `_remainingLoadActive` producer-liveness across the `async void` first await (`:79-137`, `:188-245`) | not the natural home for any of the four; leave alone |
| `QuickFiler.Test/Controllers/QfcQueueTests.cs` | 67 | one test: `RemoveItem_WhenTokenPreCancelled_DoesNotThrow` (`:30-31`) | spare capacity, but wrong subject |
| `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` | 136 | `QfcQueue` no-jobs fast paths (`:52-102`); **`DequeueNextItemGroupAsync_HighConfidenceDisabled_PreservesDirectBatchDequeue` (`:104-134`) — the exact `Mock<IEmailMoveMonitor>` + `SetPrivateField` pattern #426 needs** | **#426 (best home)** — plenty of headroom, established pattern at `:119-133` |
| `QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs` | 290 | `QfcQueue.Dequeue`/`TryDequeueAsync` unhook + carrier-folder preservation (`:101-214`); TLP/renumber helpers (`:216-289`) | secondary #426 home if `QfcQueuePurePathsTests` grows |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` | 373 | Gate core: score selection + logging, backfill, exhaustion, inclusive threshold, cancellation, **drop-on-reject (`:298-310`)**, empty-source wait, source-active polling. Owns the reflective `CreateGate`/`DequeueAsync` helpers (`:26-206`) | **#426** gate-level rejection-callback test; helper migration for #446/#427-A |
| `...Tests.Part2.cs` | 455 | Issue #424 deadline behaviour: eight tests (`:76-455`) including `DequeueAsync_DeadlineExpiresWithZeroAccepted_ReturnsEmptyListAtTheBound` (`:124-143`) and the `Timeout.InfiniteTimeSpan` sentinel (`:262`) | **#446** gate-level "empty + `DeadlineExpired`" assertion — but the file is at 455/500, see §7 |
| `...Tests.Part3.cs` | 152 | Issue #424 progress-callback tests (`:29-151`) | **#446 and #427-A gate-level tests (best home — 348 lines of headroom)** |
| `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` | 464 | `IterateQueueAsync` empty/non-empty/complete branches (`:77-310`); `Iterate`/`Iterate2`/`SwapStopWatch` (`:312-462`) | **#446 caller-level tests** — but only 36 lines of headroom, see §7 |
| `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` | 314 | Hook/unhook bookkeeping, shared-folder subscribe-once, null no-op, marshal-delegate accounting (`:176-198`), `UnhookAll`, ThreadPool-origin marshalling (`:266-312`) | **#426** monitor-level assertions if any are needed (probably none — the defect is in the *caller*, not the monitor) |
| `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` | 827 | Broad `QfcFormController` surface, incl. the `UndoConsumer` placeholder (`:687-701`) and `UndoDialog_ShouldUndoMoves` (`:674-685`) | **#448** — but the file is already 327 lines over the cap, see §7 |
| `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` | 378 | Seam B / Seam D behaviour; the source-text overload-order test (`:352-374`) | **#448 (best home — 122 lines of headroom, and it is the file explicitly created "so the pre-existing `QfcFormControllerTests.cs` is not grown further", `:22-23`)** |
| `QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs` | 261 | The overload-selection pins (`:137-259`) | **#427** if 427-B is ever attempted; untouched under 427-A |

### 6.1 Can every regression test live in an existing file?

**Yes — no new test file is required, and therefore no `QuickFiler.Test.csproj` edit is required.** Recommended placement:

| Defect | Test | File | Headroom after |
| --- | --- | --- | --- |
| #446 (gate) | deadline expiry reports `DeadlineExpired`; source drain reports `SourceExhausted` | `QfcStreamingDequeueConfidenceGateTests.Part3.cs` (152) | ~300 lines |
| #446 (caller) | empty + `DeadlineExpired` -> `CompleteAddingAsync` **never**; empty + `SourceExhausted` -> **once** | `QfcHomeControllerIterationTests.cs` (464) | **~36 lines — tight** |
| #448 | idle-exit terminates with a `FakeTimeProvider`; `_undoConsumerTask` reset; no busy-spin (delay delegate invoked each idle iteration) | `QfcFormControllerSeamTests.cs` (378) | ~120 lines |
| #426 (gate) | below-cutoff candidate invokes `onRejected` exactly once; accepted candidate does not | `QfcStreamingDequeueConfidenceGateTests.cs` (373) | ~125 lines |
| #426 (datamodel) | high-confidence dequeue calls `IEmailMoveMonitor.UnhookItem` for the rejected item | `QfcQueuePurePathsTests.cs` (136) | ~360 lines |
| #427-A | `ScoreRemainingQueueMailItemAsync` surfaces `TopFolder`; gate batch carries `QfcPreScoredItem` | `QfcDatamodelTests.cs` (317) + `QfcStreamingDequeueConfidenceGateTests.Part3.cs` | comfortable |

**The one at-risk placement is #446's caller-level tests in `QfcHomeControllerIterationTests.cs` (464/500).** Two mitigations, in order of preference:
1. Rewrite `IterateQueueAsync_QueueEmpty` (`:123-182`, 60 lines) into a shared arrange helper plus two compact tests. The four `IterateQueueAsync_*` tests share ~40 lines of duplicated mock setup (`:127-160`, `:190-234`, `:261-294`, `:272-294`); extracting one `ArrangeIterate(...)` helper frees roughly 80-100 lines and comfortably accommodates the new cases. This is the recommended route — it stays in one file and reduces the line count.
2. If (1) is rejected, add `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.Part2.cs` as a `public partial class QfcHomeControllerIterationTests` (following the established `QfcStreamingDequeueConfidenceGateTests.Part2/.Part3` pattern; note `[TestClass]` must appear on the base file only — `AllowMultiple = false`, repeating it is CS0579, documented at `QfcStreamingDequeueConfidenceGateTests.Part2.cs:17-18`). That file **would** require a `Compile Include` entry.

**If a new file is unavoidable**, the `Compile Include` item group at `QuickFiler.Test/QuickFiler.Test.csproj:57-175` is **not** alphabetically sorted — it is loosely grouped by subject (Breadcrumb block `:58-95`, Kbd block `:96-100`, Efc block `:104-115`, Qfc block `:116-160`, TestSupport `:161-163`, Helper Classes `:164-171`, root `:172-174`). The correct insertion is by **subject adjacency**, not alphabet:

- `Controllers\QfcHomeControllerIterationTests.Part2.cs` -> immediately after line 132 (`Controllers\QfcHomeControllerIterationTests.cs`), before line 133.
- `Controllers\QfcStreamingDequeueConfidenceGateTests.Part4.cs` -> immediately after line 130 (`...Tests.Part3.cs`), before line 131.
- `Helper Classes\EmailMoveMonitorRejectionTests.cs` -> immediately after line 165 (`Helper Classes\EmailMoveMonitorTests.cs`), before line 166.

---

## 7. Cross-cutting C — determinism seam inventory

`.claude/rules/general-unit-test.md` bans `setTimeout`, `Thread.Sleep`, `Task.Delay`, real wall-clock waits, and `Date.now()`/`DateTime.Now` outside a clock interface **in test code**, and requires `FakeTimeProvider` for .NET async tests.

### 7.1 Production seams already available

| Seam | Location | Notes |
| --- | --- | --- |
| `internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;` | `QuickFiler/Controllers/QfcDatamodel.cs:112` | consumed at `QfcDatamodel.QueueProcessing.cs:121` (passed to the gate), `:173` (`WaitForQueue` poll), `QfcDatamodel.FrameBuilding.cs:43` |
| `internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;` | `QuickFiler/Controllers/QfcHomeController.Metrics.cs:17` | consumed at `Metrics.cs:27`, `:107`, `:222`. Same partial class as `QfcHomeController.Iteration.cs`, so `IterateQueueAsync` can use it |
| `TimeProvider timeProvider = null` launch parameter | `QuickFiler/Controllers/QfcHomeController.cs:41`, assigned `:54` | how tests seed the home controller's provider |
| `private readonly TimeProvider _timeProvider;` | `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs:27`, defaulted `:69` | `GetTimestamp()`/`GetElapsedTime()` at `:102`, `:110`; `Delay` at `:126-128` |
| `TimeSpan? firstBatchDeadline` ctor seam | `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs:62`, validated `:74-84` | `Timeout.InfiniteTimeSpan` disables |
| `Action<System.Action> _marshalToSta` | `QuickFiler/Helper Classes/EmailMoveMonitor.cs:29`, ctor `:38-42` | thread-affinity seam; tests pass a synchronous pass-through |
| `Func<CancellationToken, Task<bool>> RemainingEmailLoader` | `QuickFiler/Controllers/QfcDatamodel.cs:128` | worker-body seam |
| `Func<...> HighConfidencePreFilterLoader` | `QuickFiler/Controllers/QfcHomeController.cs:236-244` | dormant on the live path |

**Gaps that must be filled by this feature:** `QfcFormController` has **no** `TimeProvider` seam and **no** consumer-start seam. `QfcFormController.Actions.cs:255` uses `new Stopwatch()` and `:285` uses `await Task.Delay(200)` directly. Both must be replaced per §2.6, and both new properties can be declared in the owned `QfcFormController.Actions.cs` partial.

Also present but out of scope: raw `Task.Delay` in production at `QfcQueue.cs:57`, `:121`, `:283` and `QfcItemController.EventWiring.cs:135`; raw `Stopwatch` at `QfcHomeController.Iteration.cs:57`, `:72`, `:82` (metrics only, not awaited).

### 7.2 Test-side seams already available

| Seam | Location |
| --- | --- |
| `FakeTimeProvider` (`Microsoft.Extensions.Time.Testing`) | used in `QfcStreamingDequeueConfidenceGateTests.cs:8`/`:316`/`:342`, `...Part2.cs` (12 references), `...Part3.cs` (4), `QfcDatamodelTests.cs:9`/`:106`/`:254`/`:288`, `QfcDatamodelLivenessTests.cs`, `QfcHomeControllerMetricsTests.cs` |
| Packages already referenced | `QuickFiler.Test/packages.config:18` (`Microsoft.Bcl.TimeProvider 10.0.11`), `:85` (`Microsoft.Extensions.TimeProvider.Testing`) |
| `CreateUninitializedDatamodel()` + `SetPrivateField` | `QfcDatamodelTests.cs:231-241`; reused by `QfcQueuePurePathsTests.cs` |
| `Mock<IEmailMoveMonitor>` injection by reflection | `QfcQueuePurePathsTests.cs:119-125`, `QfcQueueCoverageExpansionTests.cs:119`/`:145`/`:207`, `QfcCollectionControllerTests.cs:359` |
| Synchronous pass-through marshal delegate | `EmailMoveMonitorTests.cs:63-70` (`CountingPassThrough`) |
| `[assembly: InternalsVisibleTo("QuickFiler.Test")]` | `QuickFiler/Properties/AssemblyInfo.cs:5` — internals are directly reachable; the reflective helpers in the gate tests are a stylistic choice, not a necessity |
| `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` | `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:11` — lets Moq proxy internal interfaces such as `IEmailMoveMonitor` |

**Conclusion:** reuse `TimeProvider` + `FakeTimeProvider` throughout. Do not introduce a new clock abstraction, a `Func<TimeSpan, Task>` delay delegate, or an `IStopwatch` interface.

---

## 8. Cross-cutting D — file sizes against the 500-line cap

`.claude/rules/general-code-change.md`: no production, test, or reusable script file may exceed 500 lines.

### Owned production files

| File | Lines | Headroom | Note |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcDatamodel.cs` | **496** | **4** | **AT THE CAP.** #427-A's edit to `ScoreRemainingQueueMailItemAsync` (`:363-377`) must be net-neutral or shrinking. New types (`QfcDequeueBatch`, `QfcDequeueStop`) must **not** go here. |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | 177 | 323 | comfortable; the natural home for the datamodel-side carrier wiring |
| `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | 171 | 329 | comfortable; the natural home for `QfcDequeueStop` / the gate batch type |
| `QuickFiler/Controllers/QfcFormController.Actions.cs` | 302 | 198 | comfortable for the #448 rewrite plus two seam properties |
| `QuickFiler/Controllers/QfcHomeController.Iteration.cs` | 86 | 414 | comfortable |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | 59 | 441 | comfortable; the natural home for the new member and, if desired, `QfcDequeueBatch` |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | 235 | 265 | comfortable (untouched under 427-A) |
| `QuickFiler/Helper Classes/EmailMoveMonitor.cs` | 262 | 238 | comfortable (likely untouched — the #426 fix is in the caller) |

Adjacent non-owned files, for context: `QuickFiler/Controllers/QfcQueue.cs` = **610 (already over cap, pre-existing)**; `QuickFiler/Controllers/QfcCollectionController.cs` = **2349 (far over, pre-existing)**; `QuickFiler/Controllers/QfcHomeController.cs` = 487; `QfcItemController.cs` = 323; `QfcFormController.cs` = 196; `QfcHighConfidencePreFilter.cs` = 191; `QfcRemainingQueueAdmission.cs` = 48; `IEmailMoveMonitor.cs` = 39; `QfcItemGroup.cs` = 52. Do not let this feature grow `QfcQueue.cs` or `QfcCollectionController.cs` — they are already violations and any addition makes it worse.

### Candidate test files

| File | Lines | Headroom |
| --- | --- | --- |
| `QfcQueueTests.cs` | 67 | 433 |
| `QfcQueuePurePathsTests.cs` | 136 | 364 |
| `QfcStreamingDequeueConfidenceGateTests.Part3.cs` | 152 | 348 |
| `QfcDatamodelLivenessTests.cs` | 255 | 245 |
| `QfcHomeControllerIssue218Tests.cs` | 261 | 239 |
| `QfcQueueCoverageExpansionTests.cs` | 290 | 210 |
| `EmailMoveMonitorTests.cs` | 314 | 186 |
| `QfcDatamodelTests.cs` | 317 | 183 |
| `QfcHighConfidencePreFilterTests.cs` | 359 | 141 |
| `QfcStreamingDequeueConfidenceGateTests.cs` | 373 | 127 |
| `QfcFormControllerSeamTests.cs` | 378 | 122 |
| `QfcStreamingDequeueConfidenceGateTests.Part2.cs` | **455** | **45 — near cap** |
| `QfcHomeControllerIterationTests.cs` | **464** | **36 — near cap** |
| `QfcHomeControllerRunAsyncHighConfidenceTests.cs` | **473** | **27 — near cap** |
| `QfcItemController.FolderHandlingTests.cs` | **498** | **2 — AT CAP** |
| `QfcFormControllerTests.cs` | **827** | **-327 — ALREADY OVER CAP (pre-existing)** |

**Placement rules for the plan:**
- Do **not** add lines to `QfcFormControllerTests.cs` (827) or `QfcItemController.FolderHandlingTests.cs` (498). Replacing the `UndoConsumer` placeholder at `QfcFormControllerTests.cs:687-701` with a *shorter or equal* body is acceptable and desirable; growing the file is not.
- Do **not** add to `...GateTests.Part2.cs` (455) or `QfcHomeControllerRunAsyncHighConfidenceTests.cs` (473).
- Prefer `...GateTests.Part3.cs` (152), `QfcQueuePurePathsTests.cs` (136), `QfcFormControllerSeamTests.cs` (378).
- `QfcHomeControllerIterationTests.cs` (464) needs the dedup described in §6.1 before it can absorb the #446 caller tests.

---

## 9. Cross-cutting E — nullable opt-in status of owned production files

Nullable enforcement in this repository is **per-file opt-in** (`#nullable enable` directive) with `/p:TreatWarningsAsErrors=true` promoting `CS86xx` in participating files. A repo-wide grep for `#nullable` in `QuickFiler/**/*.cs` returns exactly 22 files, **all** under `QuickFiler/Viewers/` plus `QuickFiler/Controllers/BreadcrumbOutboundQueue.cs` and `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`.

| Owned file | `#nullable enable`? |
| --- | --- |
| `QuickFiler/Controllers/QfcDatamodel.cs` | **No** |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | **No** |
| `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | **No** |
| `QuickFiler/Controllers/QfcFormController.Actions.cs` | **No** |
| `QuickFiler/Controllers/QfcHomeController.Iteration.cs` | **No** |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | **No** |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | **No** |
| `QuickFiler/Helper Classes/EmailMoveMonitor.cs` | **No** |

**None of the owned production files participates in nullable analysis.** The nullable gate (`msbuild ... /p:TreatWarningsAsErrors=true`) will not police `CS86xx` on our edits there. Do **not** add `#nullable enable` to any of them as part of this work: `QfcDatamodel.cs` is at 496/500 lines, and opting a file in retroactively is a scope expansion whose blast radius (every existing member in the file) is unrelated to these four defects. Zero `QuickFiler.Test/**/*.cs` files carry the directive either.

Note that `/p:TreatWarningsAsErrors=true` still promotes **non-nullable** warnings (e.g. `CS0618` obsolete usage) to errors in these files. `QfcDatamodel.cs:437`/`:458` already carries a narrow `#pragma warning disable CS0618` for that reason. Analyzer diagnostics from step 2 of the toolchain (`EnableNETAnalyzers` / `EnforceCodeStyleInBuild`) apply normally.

---

## 10. Summary of changes required OUTSIDE the owned file set

| Defect | Recommended scope | Non-owned files touched |
| --- | --- | --- |
| #446 (Option B) | new additive `IQfcDatamodel` member + `IterateQueueAsync` guard | **None** |
| #448 | `TimeProvider` + start-delegate seams declared in the `QfcFormController.Actions.cs` partial; loop rewrite | **None** |
| #426 (Direction 2) | gate rejection callback wired by the datamodel | **None** |
| #427 Scope A (recommended) | producer side only; carrier reaches the datamodel boundary and stops there | **None** |
| #427 Scope B (full fix) | activate the carrier overload and thread the predictor | **`QfcHomeController.cs`, `QfcCollectionController.cs`, `QfcItemController.cs`, `QfcItemController.Initialization.cs`, `QfcHighConfidencePreFilter.cs`, `QfcItemGroup.cs` — all owned by sibling epic children. DO NOT attempt without explicit coordination.** |

Test files that must be edited regardless of scope (all pre-existing, no `.csproj` change): `QfcStreamingDequeueConfidenceGateTests.cs` (helper migration), `QfcHomeControllerIterationTests.cs` (four setups retargeted + dedup). Test files that must **not** be weakened: `QfcStreamingDequeueConfidenceGateTests.cs:298-310` (drop-on-reject), `QfcFormControllerSeamTests.cs:352-374` (source-text signature order), `QfcHomeControllerIssue218Tests.cs:137-259` and `QfcHomeControllerRunAsyncHighConfidenceTests.cs:246`/`:277` (overload selection — untouched under 427-A).

---

## 11. Testing implications (strategy only; no test code proposed)

Consistent with `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`, and the C# Unit Test Policy (MSTest + Moq + FluentAssertions).

**Fail-before requirement.** Each of the four defects admits a genuine failing-first test:
- #446: caller-level — dequeue returns empty with `Stop == DeadlineExpired`; assert `CompleteAddingAsync` was never called. Fails today because the current contract has no `Stop` and the empty branch is unconditional.
- #448: `UndoConsumer` driven past the idle threshold on a `FakeTimeProvider` must complete. Fails today (hangs) — so the failing-first run must be bounded by the test framework's timeout, not by a wall-clock wait inside the test. Use `[Timeout(...)]` on the failing-first run only, or assert on the *shape* (the delay delegate is invoked on every idle iteration, which is false after the threshold today) so the RED state is an assertion failure rather than a hang. **The second form is strongly preferred** — a hanging RED test is not a usable gate.
- #426: gate-level — one below-cutoff candidate; assert the rejection callback fired once. Fails today (no callback exists, so the test does not compile until the seam lands; write it against the seam and let the RED state be the assertion, not a compile error, by adding the seam and the test in one task and the `else` clause in the next).
- #427-A: `ScoreRemainingQueueMailItemAsync` surfaces a non-empty `TopFolder` for a scored item. Fails today (`return score.Score;`).

**Coverage.** `QfcDatamodel` is `[ExcludeFromCodeCoverage]` at `QfcDatamodel.cs:25` (a type-level attribute on one partial declaration, so it covers `QfcDatamodel.QueueProcessing.cs` too), and `FolderScoringService` is excluded at `QfcHighConfidencePreFilter.cs:166`. Coverage credit for this work therefore accrues to `QfcStreamingDequeueConfidenceGate` (not excluded), `QfcFormController` (not excluded), and `QfcHomeController` (not excluded). Plan the coverage comparison accordingly and do not expect the datamodel edits to move the number.

**Determinism.** Every new test uses `FakeTimeProvider`. No `Thread.Sleep`, no `Task.Delay`, no `DateTime.Now`, no temporary files, no live Outlook COM. `MailItem` and `Folder` are `Mock<>`-ed as in `QfcStreamingDequeueConfidenceGateTests.cs:18-24` and `EmailMoveMonitorTests.cs:72-85`.

**Scenario completeness per defect.** Positive (the fixed behaviour), negative (the other stop reason still closes the queue; an accepted item is still not unhooked twice), boundary (deadline exactly at the bound — already pinned by `...Part2.cs:124-143`; score exactly at cutoff — already pinned by `QfcStreamingDequeueConfidenceGateTests.cs:265-278`), error handling (a throwing rejection callback must not abort the scan; `CompleteAddingAsync` timeout still throws — pinned at `QfcQueueCoverageExpansionTests.cs:178-190`), and state transition (`_undoConsumerTask` null -> task -> null across two `UndoDialog()` calls).

**Toolchain.** `dotnet tool run csharpier format .` -> `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` -> `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` -> `vstest.console.exe <assemblies> /EnableCodeCoverage`, restarting from step 1 on any failure or auto-fix. Note that CSharpier may reflow a widened `LoadItemsAsync` signature and break `QfcFormControllerSeamTests.cs:352-374`; run the format step before assuming that test is green.

---

## 12. Items I could not verify

- **Issue #435 / epic child F6 content.** No `docs/features/**/*435*` folder exists in this worktree and no start-delegate seam is present in the source. I could not determine whether F6 was abandoned, reverted, or never merged — only that its claimed artifact is absent at `988e819b`. The plan must not assume the seam exists.
- **Issues #214 and #420 source documents.** I verified the thread-affinity *contract* from the XML documentation at `IEmailMoveMonitor.cs:6-12` and `EmailMoveMonitor.cs:24-37` and from the pinned tests at `EmailMoveMonitorTests.cs:176-198` and `:266-312`. I did not read the #214/#420 issue bodies, which are not present in this worktree.
- **Runtime behaviour.** No test run, build, or Outlook session was executed; this is static analysis only. Claims about hang behaviour, COM retention growth, and duplicate `Probability debug` lines are derived from the code, not observed.
Everything else in this document was verified directly against the current worktree. In particular, the `QfcRemainingQueueAdmission` question raised during analysis was resolved (see §5.1): its `scoreLoader` parameter is dead, and an adapter lambda at `QfcDatamodel.cs:355` keeps #427-A entirely inside the owned file set.
