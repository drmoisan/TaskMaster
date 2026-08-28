# quickfiler-home-controller-metrics — Research

- **Feature:** `quickfiler-home-controller-metrics` (epic child)
- **Primary issue:** #442. Also closes #443 and #451.
- **Date:** 2026-08-24
- **Mode:** READ-ONLY research. No production or test code was written.
- **Workspace:** `<repo-root>/.claude/worktrees/<preparation-agent-worktree>` (a preparation-mode
  agent worktree; the execution worktree resolves its own root via `git rev-parse --show-toplevel`)

> **Timestamp provenance note.** The Bash tool is disabled in this session and no shell clock
> could be queried. The filename timestamp `2026-08-24T10-00` is derived from the session date
> (2026-08-24) and is known to be at or after `09-40`, the timestamp of the sibling artifact
> `docs/features/active/quickfiler-home-controller-metrics-442/plan.2026-08-24T09-40.md`. The
> minute component is therefore approximate, not clock-read. Flagged rather than asserted.

---

## 0. Ownership map (verified)

| File | Lines | Owned? | Note |
|---|---:|---|---|
| `QuickFiler/Controllers/QfcHomeController.cs` | 487 | **OWNED** | 13 lines of headroom under the 500-line cap |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | 234 | **OWNED** | primary landing site for new code |
| `QuickFiler/Controllers/EfcHomeController.cs` | 441 | **OWNED** | |
| `QuickFiler/Controllers/EfcHomeController.Metrics.cs` | 87 | **OWNED** | |
| `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs` | 144 | **OWNED** | |
| `QuickFiler/Controllers/QfcHomeController.Iteration.cs` | 86 | forbidden (446) | holds `SwapStopWatch()` |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | 399 | forbidden (446) | holds both swap→write orderings |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 2000+ | forbidden (468) | holds `GetMoveDiagnostics`, `xComma` |
| `QuickFiler/Controllers/EfcFormController.cs` | 700+ | forbidden (464) | |

Three further files are **not** in the owned list and therefore also may not be written, which
constrains several otherwise-obvious designs:

- `QuickFiler/QuickFiler.csproj` — this is a legacy non-SDK project with explicit
  `<Compile Include=...>` entries (`QuickFiler.csproj:293-296, 323-325`). **A new production
  `.cs` file cannot be added without editing it.** All new production code must therefore land in
  one of the five owned partial files.
- `QuickFiler/Controllers/EfcHomeControllerDependencies.cs` — the EFC seam container. No new EFC
  dependency delegate can be introduced.
- `QuickFiler/Interfaces/IFilerHomeController.cs` and `QuickFiler/Controllers/IQfcHomeController.cs`
  — no public signature on `WriteMetricsAsync(string)` or `QuickFileMetrics_WRITE(string)` may
  change.

---

## 1. Verification of the orchestrator's ground truth

Every supplied ground-truth item was checked against source. All are **confirmed**, with three
refinements.

| Claim | Verdict | Evidence |
|---|---|---|
| Injectable `TimeProvider` seam exists | Confirmed | `QfcHomeController.Metrics.cs:17` — `internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;` |
| `_metricsConsumers` starts at 0, only decremented | Confirmed | init `QfcHomeController.cs:356`; decrements at `QfcHomeController.cs:366` and `QfcHomeController.Metrics.cs:228`. A repo-wide grep for `_metricsConsumers` returns exactly those four sites — **no increment exists** |
| CAS guard can never be true | Confirmed | `QfcHomeController.Metrics.cs:226` — `Interlocked.CompareExchange(ref _metricsConsumers, 0, 2) == 2` |
| Timer is a local, never started, never disposed | Confirmed | `QfcHomeController.Metrics.cs:229-230` — `var timer = new System.Timers.Timer(2000); timer.Elapsed += TimedConsumerAsync;`. No `Start()`, no `Enabled`, no `Dispose()`, and `timer` leaves scope at line 231 |
| `_fileName` is `static`, written never read | Confirmed | declared `QfcHomeController.cs:358`; sole write `QfcHomeController.Metrics.cs:153`; zero reads |
| `TimedConsumerAsync` decrements and blocks | Confirmed | `QfcHomeController.cs:362-386`; L366 decrement, L367 `_metrics.GetConsumingEnumerable().ToArray()`. Repo-wide grep confirms **`_metrics.CompleteAdding()` is called nowhere** |
| `Metrics.cs:42` uses `_stopWatchMoved.Elapsed.Seconds` | Confirmed | `QfcHomeController.Metrics.cs:42` |
| `Metrics.cs:121` uses `StopWatch.Elapsed.Seconds` | Confirmed | `QfcHomeController.Metrics.cs:121`; the commented-out prior form is at L120 |
| `QfcHomeController.cs:267-268` constructs and starts `_stopWatch` | Confirmed | and there is a **second, unlisted** start pair at `QfcHomeController.cs:315-316` in `RunAsync` |
| EFC `_stopWatch` constructed at L76 and L225, never started | Confirmed | `EfcHomeController.cs:76`, `EfcHomeController.cs:225`; repo-wide grep for `_stopWatch` in `QuickFiler/Controllers/` shows **no `Start()` on any EFC path** |
| EFC field L383, property L386, `_isExecuting` L389 | Confirmed (offset) | field `EfcHomeController.cs:383`; property declared L384 with getter at L386; `private volatile bool _isExecuting;` at **L389** |
| `EfcHomeController.Metrics.cs:23` `.Seconds` | Confirmed | |
| `EfcHomeController.Metrics.cs:26-29` bare `NotImplementedException` | Confirmed | |
| `Metrics.cs:80-81` missing comma; L79 `xComma` on Subject only | Confirmed | `EfcHomeController.Metrics.cs:79-82` |
| EFC seams `MetricsNowFactory` / `MetricsLineWriter` exist | Confirmed | `EfcHomeController.Metrics.cs:39,51`; declared `EfcHomeControllerDependencies.cs:125,127`; defaults wired `EfcHomeControllerDependencies.cs:77-78` |

**Refinement 1 — a second QFC stopwatch start.** `QfcHomeController.cs:315-316` (inside `RunAsync`)
also does `_stopWatch = new Stopwatch(); _stopWatch.Start();`. `Run()` at L267-268 is the
synchronous path; `RunAsync` at L315-316 is the path actually taken by `LaunchAsync`
(`QfcHomeController.cs:72`). Both are in an owned file.

**Refinement 2 — `Iterate()`/`Iterate2()` also restart the stopwatch.**
`QfcHomeController.Iteration.cs:57-58` and `:72-73` each do `_stopWatch = new Stopwatch(); Start();`.
Those are in a **forbidden** file. This matters for A/B below: the QFC interval is restarted from
four distinct production sites, three of which are outside this feature's write scope.

**Refinement 3 — `QuickFileMetrics_WRITE(string)` is an interface member.**
`QuickFiler/Interfaces/IFilerHomeController.cs:41` declares `void QuickFileMetrics_WRITE(string filename);`.
#451's Defect 6 offers "implement it or remove it"; **removal is not available**, because the
interface file is not owned. The overload must be implemented.

---

## 2. Research question A — flush timing (#442)

### A.1 Every caller of the two metrics-write entry points

| Entry point | Declared at | Called from | Lifecycle phase | Owned? |
|---|---|---|---|---|
| `QfcHomeController.WriteMetricsAsync(string)` | `QfcHomeController.Metrics.cs:90` | `QfcFormController.cs:47` binds it to the `WriteMetrics` delegate; invoked at `QfcFormController.EventHandlers.cs:229` inside `BackGroundMoveAsync` | after `MoveEmailsAsync` completes, on the UI dispatcher at `ContextIdle` priority | call site forbidden (446) |
| `QfcHomeController.QuickFileMetrics_WRITE(string)` | `QfcHomeController.Metrics.cs:19` | **no production caller.** Repo-wide grep finds only the interface declaration (`IFilerHomeController.cs:41`) and two tests | dead in production; live only under test | — |
| `EfcHomeController.QuickFileMetrics_WRITE(string, string, List<MailItemHelper>)` | `EfcHomeController.Metrics.cs:12` | `EfcHomeController.ExecuteMoves.cs:141` inside `HandleMoveResult` | immediately after a successful `MoveToFolderAsync` | **owned** |
| `EfcHomeController.QuickFileMetrics_WRITE(string, string, List, int)` | `EfcHomeController.Metrics.cs:31` | the 3-arg overload above (`Metrics.cs:23`), and tests | — | **owned** |
| `EfcHomeController.QuickFileMetrics_WRITE(string)` | `EfcHomeController.Metrics.cs:26` | **no production caller.** Interface-mandated; throws | — | **owned** |

The EFC path already writes to disk synchronously through `_dependencies.MetricsLineWriter`
(`EfcHomeController.Metrics.cs:51`, defaulting to `FileIO2.WriteTextFile` at
`EfcHomeControllerDependencies.cs:78`). **#442 is a QFC-only defect.** The EFC path is not
affected by the never-flushed bug.

### A.2 QFC controller lifecycle and the last safe flush point

```
LaunchAsync                       QfcHomeController.cs:38
  └ InitAsync                     QfcHomeController.cs:63 → :111
  └ RunAsync(progress)            QfcHomeController.cs:72  → :274
      ├ _stopWatch = new; Start() QfcHomeController.cs:315-316
      └ IterateQueueAsync         QfcHomeController.cs:323 → Iteration.cs:11

user presses OK  → QfcFormController.EventHandlers.cs:96 ButtonOK_Click
  └ ActionOkAsync                 EventHandlers.cs:110
      └ MoveAndIterate            EventHandlers.cs:145
          ├── queue non-empty branch (EventHandlers.cs:154-177)
          │     CacheMoveObjects();  moveTask = BackGroundMoveAsync();   [L156-157]
          │     await LoadUiFromQueue();   ← calls SwapStopWatch()       [L161 → L142]
          │     await moveTask;                                          [L175]
          └── end-of-database branch (EventHandlers.cs:187-212)
                CacheMoveObjects();                                      [L190]
                _parent.SwapStopWatch();                                 [L191]
                await BackGroundMoveAsync();                             [L192]
                MessageBox "Finished Moving Emails"                      [L204]
                await ActionCancelAsync();                               [L210]

ActionCancelAsync                 EventHandlers.cs:84
  ├ _parent?.TokenSource?.Cancel();                                      [L86]
  ├ _groups?.Cleanup();                                                  [L92]
  └ Cleanup()  (QfcFormController)  SetupDisposal.cs:208
        ├ _globals = null;                                               [SetupDisposal.cs:217]
        ├ WriteMetrics = null;                                           [SetupDisposal.cs:224]
        └ _parentCleanup?.Invoke()  →  QfcHomeController.Cleanup()       [SetupDisposal.cs:226]

QfcHomeController.Cleanup         QfcHomeController.cs:388
  ├ _datamodel.Cleanup();                                                [L390]
  ├ Globals = null;                                                      [L391]
  └ ParentCleanup.Invoke();                                              [L396]
```

`BackGroundMoveAsync` (`EventHandlers.cs:215-234`) is the metrics producer. Note the invocation
shape at L228-231:

```csharp
await UiThread.Dispatcher.InvokeAsync(
    async () => await WriteMetrics(_globals.FS.Filenames.EmailSession),
    System.Windows.Threading.DispatcherPriority.ContextIdle
);
```

`UiThread.Dispatcher` is a WPF `Dispatcher` (`UtilitiesCS/Threading/UiThread.cs:135`).
`Dispatcher.InvokeAsync(Func<Task>, priority)` returns `DispatcherOperation<Task>`; awaiting that
operation yields the inner `Task` **without awaiting it**. The metrics write is therefore
effectively fire-and-forget past its first internal suspension point. This is in a forbidden file
and is recorded as an observation only, but it constrains the flush design: a flush placed *inside*
`WriteMetricsAsync` is not reliably awaited by the caller.

**Last point at which a flush can still run safely.** `QfcHomeController.Cleanup()`
(`QfcHomeController.cs:388`) at its **first statement**, before `Globals = null` at L391. At that
instant `Globals` is still live, so `Globals.FS.SpecialFolders` and `Globals.FS.Filenames.EmailSession`
are both reachable. One statement later they are not.

Two hazards at that point:

1. `ActionCancelAsync` cancels `_parent.TokenSource` at `EventHandlers.cs:86` **before** invoking
   `Cleanup()`. So `this.Token` is already cancelled when `Cleanup()` runs. A flush at Cleanup must
   pass `CancellationToken.None` / `default`, never `Token`. (`QfcHomeController.cs:376` already
   passes `default` to `WriteTextFileAsync`; that precedent is correct and must be preserved.)
2. `Cleanup()` is synchronous (`void`, mandated by `IFilerHomeController.cs:17`). An `async` flush
   at that point would have to be `.GetAwaiter().GetResult()`-blocked on the UI thread, or
   fire-and-forget. This is a decisive argument against relying on a Cleanup-time flush.

### A.3 Where `CompleteAdding()` belongs and who owns the timer

If the producer/consumer design is retained: `_metrics.CompleteAdding()` must be called exactly
once, at the top of `QfcHomeController.Cleanup()` (`QfcHomeController.cs:388`), before
`Globals = null`, and the consumer must be awaited to completion before the method returns —
otherwise `GetConsumingEnumerable()` (`QfcHomeController.cs:367`) blocks the consumer thread
forever, and the last batch is lost. The timer would have to be an instance field, started at
first enqueue, `Stop()`+`Dispose()`d in `Cleanup()` before `CompleteAdding()`.

That is four new lifecycle obligations on a synchronous `void Cleanup()` whose caller has already
cancelled the token. **This is the argument against retaining the design**, developed in A.4.

### A.4 Recommendation — replace the producer/consumer with a direct awaited append

**Recommended: Option A — direct awaited append through an injectable writer seam.**

Concretely, in the owned `QfcHomeController.Metrics.cs`:

1. Add a writer seam mirroring the EFC precedent at `EfcHomeControllerDependencies.cs:78`:
   `internal Func<string, string[], string, CancellationToken, Task> MetricsFileWriter { get; set; } = FileIO2.WriteTextFileAsync;`
2. Replace `QfcHomeController.Metrics.cs:153-154` (`_fileName = filename; await NonBlockingProducer(strOutput, Token);`)
   with a guarded, null-filtered `await MetricsFileWriter(filename, lines, myDocuments, Token)`.
3. Delete `NonBlockingProducer` (both overloads, `Metrics.cs:190-232`) and the dead consumer-
   scheduling block (`Metrics.cs:226-231`).
4. Delete `_metrics`, `_metricsConsumers`, `_lockObject`, `_fileName`
   (`QfcHomeController.cs:353-358`) and `TimedConsumerAsync` (`QfcHomeController.cs:362-386`).

**Why this and not "fix the existing design".** The General Code Change Policy ranks *simplicity
first* and *fail fast and explicitly*. Concretely:

- The producer/consumer exists to batch appends across a 2-second window. The array reaching
  `WriteMetricsAsync` is already the complete batch for one OK-click (`GetMoveDiagnostics` returns
  one line per moved item), and `FileIO2.WriteTextFileAsync` (`FileIO2.cs:50-89`) opens the file
  once per call and writes all lines inside one `using`. The batching the queue provides is
  therefore a second-order optimisation over an already-batched call, for which the policy demands
  a demonstrated need. None is documented.
- Retaining the queue means owning: an increment that pairs with the two decrements, a started and
  disposed timer, a `CompleteAdding()` in a synchronous `Cleanup()` whose token is already
  cancelled, and a join on the consumer before `Globals` is nulled. Every one of those is a new
  state-transition invariant, and three of them sit on a `void` method that cannot await.
- Deleting the machinery **frees roughly 33 lines in `QfcHomeController.cs`**, taking it from 487
  to about 454 and restoring real headroom under the 500-line cap. Option B would consume the
  remaining 13 lines and overflow.
- Testability improves rather than degrades: a `Func` seam is assertable with a captured-argument
  list and zero timers, satisfying the "no wall-clock waits" rule in
  `.claude/rules/general-unit-test.md` without needing `TimeProvider.CreateTimer` plumbing.

**Honest trade-off.** Option A moves the file I/O onto the metrics call path. Three costs:

1. The append now runs inside the `ContextIdle` dispatcher continuation rather than on a timer
   thread. `WriteTextFileAsync` is genuinely async and retries `IOException` up to 100 times with
   `await Task.Delay(100)` (`FileIO2.cs:75-87`), so a locked file could keep that continuation
   alive for up to ~10 seconds. On the current design that retry loop runs on a timer thread
   instead. Mitigation: the continuation is asynchronous, so the UI message pump is not blocked;
   only the dispatcher operation stays pending.
2. Losing the queue loses the (currently nonexistent) ability to coalesce writes across iterations.
   Since the queue has never drained once in production, no behaviour is regressed — the change
   moves from "zero writes" to "one append per OK-click", not from "batched writes" to
   "unbatched writes".
3. `Task.Delay` inside `FileIO2.WriteTextFileAsync` is not `TimeProvider`-driven. Tests must
   therefore never exercise the real writer — they must inject the seam. That is the same
   discipline the EFC tests already follow (`EfcHomeControllerMetricsTests.cs:64-89`).

**Rejected alternative — Option B (repair the producer/consumer).** Add an increment paired with
the CAS, promote the timer to a disposable instance field, start it, call `CompleteAdding()` and
join the consumer at the top of `Cleanup()`. It is implementable entirely inside owned files, and
it preserves the original author's batching intent. It is rejected because it adds four lifecycle
invariants to a synchronous `void Cleanup()` running under an already-cancelled token, costs lines
in the file with only 13 to spare, and requires timer-driven tests to buy back the determinism that
Option A gets for free.

### A.5 Disposition of `_fileName`

**Delete it.** `QfcHomeController.cs:358` declares `private static string _fileName;`. It has
exactly one write (`Metrics.cs:153`) and zero reads; `TimedConsumerAsync` uses
`Globals.FS.Filenames.EmailSession` (`QfcHomeController.cs:373`) instead. It is additionally
`static` on a per-instance concern, so two concurrent controllers would race on it. Under Option A
the filename is already a parameter of `WriteMetricsAsync(string filename)` and flows straight to
the writer seam, so no field is needed. Deleting it removes one line and one latent cross-instance
shared-state hazard. `_lockObject` (`QfcHomeController.cs:357`) is likewise `static`, unreferenced
anywhere in the repository, and should be deleted with it.

### A.6 Newly-visible defect once the flush works (cross-feature)

`QfcCollectionController.GetMoveDiagnostics` allocates `new string[_itemGroupsToMove.Count + 1]`
(`QfcCollectionController.cs:2284`) but fills only indices `0..Count-1` (`:2286-2325`). **The final
element is always `null`.** `FileIO2.WriteTextFileAsync` then calls `sw.WriteLineAsync(null)`
(`FileIO2.cs:72`), appending a blank line to the CSV on every write.

This is invisible today precisely because nothing is ever written. **Fixing #442 makes it
manifest.** `QfcCollectionController.cs` is forbidden (feature 468). The owned-file-only mitigation
is to filter `null`/empty entries in `WriteMetricsAsync` before handing the array to the writer
seam — a defensive one-liner in `QfcHomeController.Metrics.cs` that is correct regardless of what
feature 468 does. **Recommend doing this, and raising the array-sizing defect as a cross-feature
note to 468.**

---

## 3. Research question B — stopwatch semantics (#443, #451)

### B.1 `SwapStopWatch()` and the `_stopWatch` / `_stopWatchMoved` relationship

```csharp
// QuickFiler/Controllers/QfcHomeController.Iteration.cs:79-84   [FORBIDDEN — feature 446]
public void SwapStopWatch()
{
    _stopWatchMoved = _stopWatch;
    _stopWatch = new Stopwatch();
    _stopWatch.Start();
}
```

Fields: `_stopWatchMoved` at `QfcHomeController.cs:443`, `_stopWatch` at `:444`, public
`StopWatch => _stopWatch` at `:445-448` (all owned).

**Post-swap invariant:** `_stopWatchMoved` holds the *completed* interval; `_stopWatch` is a
freshly started zero. So at any metrics-write point that happens **after** a swap, the correct
reading is `_stopWatchMoved`; `StopWatch` reads ~0.

Call sites of `SwapStopWatch()` — all three are in **forbidden** files:

| Site | Context | Owned? |
|---|---|---|
| `QfcFormController.EventHandlers.cs:142` | end of `LoadUiFromQueue()` | no (446) |
| `QfcFormController.EventHandlers.cs:191` | end-of-database branch of `MoveAndIterate` | no (446) |
| `QfcFormController.EventHandlers.cs:372` | `SkipGroupAsync()` | no (446) |

The two writers currently **disagree**: `QuickFileMetrics_WRITE` reads `_stopWatchMoved`
(`Metrics.cs:42`, `:44`) while `WriteMetricsAsync` reads `StopWatch` (`Metrics.cs:121`). The
commented-out `Metrics.cs:120` shows `WriteMetricsAsync` previously read `_stopWatchMoved`, i.e.
the two agreed before a regression.

### B.2 The decisive cross-feature question — is #443's stopwatch fix achievable inside owned files?

**Answer: partially, and the split is clean.**

**End-of-database path — YES, fully fixable inside owned files.**
`EventHandlers.cs:190-192` executes strictly in order: `CacheMoveObjects()` → `SwapStopWatch()` →
`await BackGroundMoveAsync()`. There is no interleaving. By the time `WriteMetricsAsync` runs,
`_stopWatchMoved` deterministically holds the completed interval and `_stopWatch` is a fresh zero.
Changing `QfcHomeController.Metrics.cs:121` from `StopWatch.Elapsed` to `_stopWatchMoved.Elapsed`
is a **one-line edit in an owned file** that makes this path correct and simultaneously re-aligns
the two writers. This is exactly the defect #443 describes as "approximately 0 seconds regardless
of the real interval".

**`MoveAndIterate` (queue non-empty) path — NO, not fixable inside owned files.**
`EventHandlers.cs:154-177`:

```
L156  _groups.CacheMoveObjects();
L157  var moveTask = BackGroundMoveAsync();     // will call WriteMetricsAsync
L161  await LoadUiFromQueue();                  // → EventHandlers.cs:142 SwapStopWatch()
L175  await moveTask;
```

`moveTask` is started but not awaited until L175, and `LoadUiFromQueue` performs the swap at L142.
The swap and the metrics write are therefore **concurrent and unordered**. Neither field is
deterministically correct:

| Interleaving | `_stopWatchMoved` holds | `_stopWatch` holds |
|---|---|---|
| swap completes before the write | the current interval (**correct**) | ~0 |
| write completes before the swap | the *previous* group's interval (stale) | the current interval (**correct**) |

Making this deterministic requires ordering the swap relative to the write — i.e. editing
`QfcFormController.EventHandlers.cs` (feature 446) or `QfcHomeController.Iteration.cs`
(feature 446). Both are forbidden.

**Owned-file-only alternatives considered and rejected:**

- *Snapshot in a property setter.* Convert the `_stopWatchMoved` field
  (`QfcHomeController.cs:443`) into a private auto-property so its setter can capture
  `value.Elapsed` at swap time. This compiles without touching `Iteration.cs` (the assignment
  `_stopWatchMoved = _stopWatch;` binds to the property). **Rejected:** it fixes *which value* is
  captured, not *when the capture happens relative to the write*. The race is unchanged. It also
  breaks the two existing tests that set `_stopWatchMoved` by field reflection
  (`QfcHomeControllerMetricsTests.cs:142`, `:226`, `:367`; `QfcHomeControllerIterationTests.cs:456`).
- *Have `WriteMetricsAsync` call `SwapStopWatch()` itself.* Calling a method declared in a
  forbidden file is permitted; only writing the file is not. **Rejected:** on `MoveAndIterate` the
  metrics write fires at `ContextIdle` after `MoveEmailsAsync`, which is *later* than the true end
  of the user-interaction interval; and if the write's swap won the race, `LoadUiFromQueue`'s
  subsequent swap would zero the interval for the *next* write. It converts one race into two.
- *Capture at `CacheMoveObjects()` time.* This is the semantically correct interval boundary and
  both branches call it (`EventHandlers.cs:156`, `:190`). **Rejected:** the method is on
  `QfcCollectionController` (forbidden, 468) and both call sites are in `EventHandlers.cs`
  (forbidden, 446).

**Recommendation.** Make the one-line `Metrics.cs:121` change. It converts the end-of-database path
from deterministically wrong (~0) to deterministically right, and converts the `MoveAndIterate`
path from "correct-or-zero depending on a race" to "correct-or-one-batch-stale depending on the
same race" — the same non-determinism, but with both outcomes now a real duration of the right
order of magnitude rather than one of them being zero. Then raise the race itself as a
**cross-feature note to feature 446**: the fix there is to move `_parent.SwapStopWatch()` out of
`LoadUiFromQueue()` (`EventHandlers.cs:142`) and place it immediately after
`_groups.CacheMoveObjects()` at `EventHandlers.cs:156`, mirroring the end-of-database ordering at
`:190-191`. That single relocation makes both branches identical and removes the race.

### B.3 Where `_stopWatch.Start()` belongs on the EFC side

The QFC pattern to mirror is `QfcHomeController.cs:267-268` / `:315-316`
(`_stopWatch = new Stopwatch(); _stopWatch.Start();`).

Both EFC construction sites are in owned files and both need the start:

| Site | Method | Reached by |
|---|---|---|
| `EfcHomeController.cs:76` | `internal EfcHomeController(globals, parentCleanup, dependencies, mail)` — only inside `if (DataModel.Mail is not null)` at L73 | the public `EfcHomeController(...)` ctor at L47-52 |
| `EfcHomeController.cs:225` | `protected async Task InitAsync(globals, mailItems, initType)` | `CreateAsync` (L104/L113) and `LoadFinderAsync` (L140/L149) via `HandleSelectionChangedAsync` (L170) → `InitAsync` (L186) |

**Recommended edit at both sites:** `_stopWatch = Stopwatch.StartNew();`

Rationale for `StartNew()` over two statements: it is one line instead of two at each site
(4 lines saved across the file), it is atomic so no future edit can separate the construction from
the start, and it cannot re-introduce the exact defect being fixed. `System.Diagnostics` is already
imported at `EfcHomeController.cs:3`.

Both sites sit at the point where the EFC session's UI is being constructed — the correct
interval start, matching QFC's semantics (start when the form is about to be shown, stop when the
move completes). No EFC code stops or swaps the stopwatch, so `Elapsed` at write time is
"time since session init", which is the intended metric.

### B.4 `.Seconds` → `.TotalSeconds` — complete site and signature inventory

**QFC sites (owned):**

| Site | Current | Required |
|---|---|---|
| `QfcHomeController.Metrics.cs:42` | `double duration = _stopWatchMoved.Elapsed.Seconds;` | `.TotalSeconds` |
| `QfcHomeController.Metrics.cs:121` | `Duration = StopWatch.Elapsed.Seconds;` | `_stopWatchMoved.Elapsed.TotalSeconds` (both fixes at once) |

No signature change is needed on the QFC side: `duration` / `Duration` are already declared
`double` (`Metrics.cs:42`, `:99`), and `GetMoveDiagnostics` already takes `double duration`
(`IQfcCollectionController.cs:109`, `QfcCollectionController.cs:2272-2279`).

One consequential detail at `Metrics.cs:123`:
`OlStartTime = OlEndTime.Subtract(new TimeSpan(0, 0, 0, (int)Duration));` — this reconstructs a
`TimeSpan` from a truncated integer. `QuickFileMetrics_WRITE` does the equivalent correctly at
`Metrics.cs:44` (`endTime.Subtract(_stopWatchMoved.Elapsed)`). Recommend aligning `:123` to the
`:44` form: `OlStartTime = OlEndTime.Subtract(_stopWatchMoved.Elapsed);`. This is the defect #443
describes as "the calendar appointment span and the CSV duration disagree", and it is an owned
one-liner. Note it must read `Elapsed` **before** the `Duration /= emailsLoaded` division at
`Metrics.cs:129`, which the `:123` position already satisfies.

**EFC sites (owned) — signature widening required:**

| Site | Current | Required |
|---|---|---|
| `EfcHomeController.Metrics.cs:23` | `..., _stopWatch.Elapsed.Seconds);` | `..., _stopWatch.Elapsed.TotalSeconds);` |
| `EfcHomeController.Metrics.cs:35` | `int elapsedSeconds` (4-arg `QuickFileMetrics_WRITE`) | `double elapsedSeconds` |
| `EfcHomeController.Metrics.cs:57` | `int elapsedSeconds` (`BuildQuickFileMetricLines`) | `double elapsedSeconds` |
| `EfcHomeController.Metrics.cs:71` | `var duration = elapsedSeconds;` (infers `int`) | infers `double` after the widening |
| `EfcHomeController.Metrics.cs:72` | `duration /= moved.Count;` — **currently integer division** | becomes real division |
| `EfcHomeController.Metrics.cs:74` | `(duration / 60d)` | unchanged; already promotes |

`Metrics.cs:72` is a **behaviour change hidden inside the type widening**: today `120 / 7` yields
`17`; after widening it yields `17.142857…`, which `durationText.ToString("##0")` at `:73` rounds
to `17`. For the existing fixture (`120 / 1`) the rendered text is unchanged, but for
multi-item moves the rounding boundary shifts. Call this out explicitly in the change description.

**Callers of the widened signatures:**

| Caller | File | Owned? | Action |
|---|---|---|---|
| `QuickFileMetrics_WRITE(f, s, m)` → 4-arg | `EfcHomeController.Metrics.cs:23` | yes | passes a `double` after the change |
| `BuildQuickFileMetricLines` | `EfcHomeController.Metrics.cs:38-43` | yes | `elapsedSeconds` flows through |
| No other production caller | — | — | repo-wide grep confirms |

Both widened members are `internal`/`internal static`; `QuickFiler` grants
`InternalsVisibleTo("QuickFiler.Test")` (`QfcHomeController.cs:18`), so the only external consumers
are the test project. **No public API breaks.** The `IFilerHomeController` and `IQfcHomeController`
signatures are untouched.

---

## 4. Research question C — time seam and testability

### C.1 The existing `TimeProvider` seam, and whether `Stopwatch` needs one

`QfcHomeController.Metrics.cs:17` declares the seam; it is set from `LaunchAsync`'s optional
parameter at `QfcHomeController.cs:41,54`. It is consumed at four sites:
`Metrics.cs:27` and `:107` (`GetLocalNow().LocalDateTime`), `Metrics.cs:222`
(`TimeProvider.Delay`), and `QfcHomeController.cs:77` (logging).

`System.Diagnostics.Stopwatch` is **not** `TimeProvider`-driven. It reads
`Stopwatch.GetTimestamp()` directly, so a `FakeTimeProvider` cannot move it.

**Verified fact: `TimeProvider.GetTimestamp()` and `GetElapsedTime(long)` are available on
net481 in this repository.** Evidence: `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs:102`
(`long start = _timeProvider.GetTimestamp();`) and `:110`
(`_timeProvider.GetElapsedTime(start) >= _firstBatchDeadline`), compiling against
`Microsoft.Bcl.TimeProvider` **10.0.11** referenced at `QuickFiler/QuickFiler.csproj:66-67`
(`lib\net462\Microsoft.Bcl.TimeProvider.dll`) and `QuickFiler.Test/packages.config:18`. The test
project additionally references `Microsoft.Extensions.TimeProvider.Testing` **10.9.0**
(`QuickFiler.Test/QuickFiler.Test.csproj:255-256`, `packages.config:85-88`), which supplies
`FakeTimeProvider` — already used at `QfcHomeControllerMetricsTests.cs:12,318-319`.

So a `TimeProvider`-based interval seam is *available*. **It is nonetheless not recommended here.**

**Recommended minimal seam: none for QFC; the existing parameter seam for EFC.**

- **EFC** already has the right shape. `internal void QuickFileMetrics_WRITE(filename, selectedFolder, moved, elapsedSeconds)`
  (`EfcHomeController.Metrics.cs:31-36`) takes the elapsed value **as a parameter**, and
  `internal static string[] BuildQuickFileMetricLines(...)` (`:55-60`) is a pure function. Every
  duration assertion is therefore already deterministic with no clock at all — see
  `EfcHomeControllerMetricsTests.cs:35-61` and `:83`. Widening `int` → `double` preserves this
  exactly. **No new seam is needed for #451's duration correctness.**
  For the *stopwatch-started* assertion, `EfcHomeController.StopWatch` is public
  (`EfcHomeController.cs:384-387`), so `controller.StopWatch.IsRunning.Should().BeTrue()` is a
  deterministic, wall-clock-free assertion. This is the exact pattern already used for QFC at
  `QfcHomeControllerRunAsyncTests.cs:303` (`Assert.IsTrue(_controller.StopWatch.IsRunning);`).
- **QFC** duration reads occur at `Metrics.cs:42` and `:121`. Both can be asserted without any
  clock by injecting a *pre-populated* `Stopwatch` via the existing reflection helper — the
  established pattern at `QfcHomeControllerMetricsTests.cs:245-251`, `:332`, `:367`. A `Stopwatch`
  whose `Elapsed` is a known non-zero value can be produced deterministically without sleeping
  (construct, `Start()`, `Stop()` — then assert only the *identity* of the field read, not a
  numeric duration). The high-value assertion for #443 is **"which stopwatch was read"**, not
  "what number came out": set `_stopWatchMoved` to a running/populated instance and `_stopWatch` to
  a fresh zero, then assert the `duration` argument reaching `GetMoveDiagnostics` is non-zero.
  Before the fix that argument is `0`; after it is not. That is a clean red/green with no timer.

  Introducing a `TimeProvider.GetTimestamp()`/`GetElapsedTime()` interval abstraction on the QFC
  side would additionally require changing `SwapStopWatch()` (forbidden, `Iteration.cs:79`), the
  four stopwatch-construction sites (two owned, two forbidden), and the public
  `Stopwatch StopWatch { get; }` member of `IFilerHomeController` (`IFilerHomeController.cs:27`,
  forbidden). **It is not achievable inside owned files and is therefore out of scope.** Record it
  as a candidate follow-up if the epic later consolidates the timing model.

### C.2 net481 constraints that bite

- **No `init` accessors, no `record`, no `record struct`, no `IsExternalInit`.** Confirmed as a
  standing repository constraint. Any new value type introduced by this feature must be a plain
  `readonly struct` or a class with a constructor, not a positional record. Since the recommended
  change set introduces **no new type**, this constraint is not actually binding for this feature.
- **`TimeProvider` is a package type, not a BCL type.** It comes from `Microsoft.Bcl.TimeProvider`
  10.0.11. Both `QuickFiler` and `QuickFiler.Test` already reference it
  (`QuickFiler.csproj:66-67`; `QuickFiler.Test.csproj:206-207`), so no `.csproj` edit is needed —
  which matters because neither `.csproj` is owned.
- **`FakeTimeProvider` cannot drive `System.Timers.Timer`.** Only `TimeProvider.CreateTimer` is
  fake-drivable. This is an additional, independent reason Option B (A.4) would have needed the
  timer replaced as well as repaired.
- **Legacy non-SDK projects with explicit `Compile Include`.** No new `.cs` file — production or
  test — can be added without a `.csproj` edit. See §0 and D.4.

### C.3 Asserting the flush path with no timer wait

Under Option A, the flush becomes assertable by injecting one delegate. Exactly this must become
injectable on `QfcHomeController` (declared in owned `QfcHomeController.Metrics.cs`):

```
internal Func<string, string[], string, CancellationToken, Task> MetricsFileWriter { get; set; }
    = FileIO2.WriteTextFileAsync;
```

With that seam, a test:

1. builds a controller via the existing `BuildLooseMetricsController()` helper
   (`QfcHomeControllerMetricsTests.cs:259-310`), whose mock `GetMoveDiagnostics` currently returns
   `Array.Empty<string>()` (`:301`) and would be changed in the new test to return a known
   non-empty array;
2. assigns `controller.MetricsFileWriter = (f, lines, root, ct) => { captured.Add(...); return Task.CompletedTask; };`
3. `await controller.WriteMetricsAsync("metrics.csv");`
4. asserts `captured` contains exactly one entry with the expected filename, folder root, and lines.

No timer, no `Task.Delay`, no `Thread.Sleep`, no disk, no temp file — compliant with
`.claude/rules/general-unit-test.md` ("Banned APIs in test code") and with the repository's
prohibition on temporary files in tests.

The **red** state for that test is unambiguous: on today's code, `WriteMetricsAsync` enqueues into
`_metrics` and returns; no writer is ever invoked, so `captured` is empty. That is the regression
test #442's bugfix workflow requires.

For the EFC side the analogous seam already exists and the analogous test already exists in
skeleton form (`EfcHomeControllerMetricsTests.cs:64-89` asserts `MetricsLineWriter` invocation).

---

## 5. Research question D — existing tests

### D.1 Established construction and mocking patterns

**`QfcHomeControllerMetricsTests.cs` (421 lines)** — `namespace QuickFiler.Controllers.Tests`, one
`[TestClass]`.

- **Two coexisting construction strategies.** A strict `MockRepository` fixture built in
  `[TestInitialize] Setup()` (`:34-55`, `MockBehavior.Strict` at `:38`) producing the field
  `_controller`; and per-test **loose** controllers built inline (`:80-131`, `:166-216`) or via the
  shared helper `BuildLooseMetricsController()` (`:259-310`). The helper is the modern pattern and
  the one new tests should use.
- **No live Outlook.** The controller is obtained through the plain public constructor
  `new QfcHomeController(mockGlobals.Object, () => { })` (`:131`, `:216`, `:307`) — never
  `LaunchAsync`. COM types (`Outlook.Application`, `NameSpace`, `Folder`, `Folders`,
  `AppointmentItem`) are moqqed directly; `Folders.GetEnumerator()` returns
  `new ArrayList().GetEnumerator()` (`:94-96`, `:178-180`, `:273-275`) so
  `UtilitiesCS.Calendar.GetCalendar("Email Time", ...)` returns `null` and the appointment branch
  is skipped.
- **Reflection for private fields.** `SetPrivateField(target, name, value)` at `:245-251` using
  `BindingFlags.NonPublic | BindingFlags.Instance`. Used for `_formController` (`:308`),
  `_stopWatch` (`:332`), `_stopWatchMoved` (`:367`), and inline at `:135-146`, `:219-230`.
- **Clock seam.** `FixedClock()` at `:318-319` returns
  `new FakeTimeProvider(new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero))`; assigned via
  `controller.TimeProvider = fake` (`:334`, `:369`, `:405`). The XML doc at `:312-317` records the
  rationale: **Moq cannot mock the non-virtual `GetLocalNow()`; `FakeTimeProvider` is the
  prescribed seam.** Expected values are derived from the fake's own `GetLocalNow().LocalDateTime`
  so the test is time-zone independent.
- **Assertion style.** FluentAssertions for exception/shape (`act.Should().NotThrow()` at `:152`,
  `:240`; `.Should().BeFalse(...)` at `:411-413`) and `Moq.Verify(..., Times.Once)` for
  interaction assertions (`:343-354`, `:378-389`).
- **`GetMoveDiagnostics` ref-parameter mocking** has two forms in-file: a captured local
  `AppointmentItem refAppointment = null;` (`:110`, `:196`) in the older tests, and
  `ref It.Ref<AppointmentItem>.IsAny` (`:298`, `:351`, `:386`) in the newer ones. Prefer the latter.
- **`Console.SetOut(new DebugTextWriter());`** at `:37`.

**`EfcHomeControllerMetricsTests.cs` (244 lines)** — same namespace, one `[TestClass]`, no
`[TestInitialize]`.

- **Zero Moq.** The file imports Moq (`:9`) but uses **hand-written fakes** throughout:
  `FakeApplicationGlobals` (`:194-223`) and `FakeFileSystemFolderPaths` (`:225-242`), both
  implementing the real interfaces with `null`-returning members for everything unused.
- **Construction:** `CreateController(specialFolders, writer)` at `:150-168` builds an
  `EfcHomeControllerDependencies` with only three of its arguments supplied — `dataModelFactory`,
  `metricsNowFactory: () => new DateTime(2026, 7, 4, 13, 5, 0)`, `metricsLineWriter: writer`
  (`:160-165`) — then `new EfcHomeController(globals, () => { }, dependencies)` (`:167`), i.e. the
  `internal` 3-arg constructor at `EfcHomeController.cs:54`.
- **`EfcDataModel` without Outlook:**
  `(EfcDataModel)FormatterServices.GetUninitializedObject(typeof(EfcDataModel))` then
  `dataModel.Mail = null` (`:170-176`). Because `Mail` is null, the
  `if (DataModel.Mail is not null)` guard at `EfcHomeController.cs:73` is **false**, so the
  constructor block at L74-94 — including `_stopWatch = new Stopwatch();` at **L76** — never runs.
  **Consequence for B.3:** a test asserting the stopwatch is started at the L76 site must supply a
  data model whose `Mail` is non-null; the current helper cannot reach that line at all.
- **Writer capture:** `MetricWrite` record-shaped class at `:178-192` (plain class with a
  constructor and get-only properties — the net481-compatible form, not a `record`).
- **Purity exploited:** `BuildQuickFileMetricLines` is asserted directly as a static pure function
  with no controller at all (`:19-61`).
- **Assertion style:** FluentAssertions exclusively — `.Should().BeEmpty()`, `.Should().Equal(...)`,
  `.Should().ContainSingle()`, `.Should().Throw<NotImplementedException>()`.

### D.2 The pinning assertion for the missing separator

`QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs:56-60`, inside
`BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine` (declared at `:35`):

```csharp
            result
                .Should()
                .Equal(
                    "07/04/2026,01:05,Quarterly Update,SingleSorted,120,2.00,RecipientSender,Email,Archive/Target,06/30/2026,09:45:10"
                );
```

The substring `RecipientSender` is the concatenation of `ToRecipientsName = "Recipient"` (`:43`)
and `SenderName = "Sender"` (`:44`), produced by the missing comma between
`{itemInfo.ToRecipientsName}` (end of `EfcHomeController.Metrics.cs:80`) and
`{itemInfo.SenderName}` (start of `:81`). After the separator fix the expected string becomes
`...,2.00,Recipient,Sender,Email,...`.

### D.3 Tests that will break — by name

**Break from the `.Seconds` → `.TotalSeconds` / `int` → `double` widening:**

| Test | File:line | Why |
|---|---|---|
| `BuildQuickFileMetricLines_WithNullOrEmptyMovedItems_ReturnsNoLines` | `EfcHomeControllerMetricsTests.cs:20` | passes literal `120` to a widened `double` parameter — **compiles unchanged** (implicit `int`→`double`), so this is a *compile-safe* call. **No break.** Listed for completeness |
| `BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine` | `EfcHomeControllerMetricsTests.cs:35` | **Breaks** — on the separator, not the widening. `120 / 1` still renders `120`/`2.00` |
| `QuickFileMetricsWrite_WithMyDocumentsFolder_InvokesInjectedWriter` | `EfcHomeControllerMetricsTests.cs:64` | passes `60` (`:83`) to a widened parameter; asserts only `.Contain("Subject")` (`:88`). **No break** |
| `QuickFileMetricsWrite_WithoutMyDocumentsFolder_DoesNotInvokeWriter` | `EfcHomeControllerMetricsTests.cs:92` | same shape. **No break** |
| `QuickFileMetricsWrite_WithNoMovedItems_DoesNotInvokeWriter` | `EfcHomeControllerMetricsTests.cs:117` | same shape. **No break** |
| `QuickFileMetrics_WRITE_WithEmptyList_SkipsBodyAndDoesNotThrow` | `EfcHomeControllerTests.cs:81` | **not** in an owned test file. Comments at `:85-86` state `_stopWatch` is null under the private constructor; the guard at `EfcHomeController.Metrics.cs:18-21` returns before `:23`, so behaviour is unchanged. **No break expected — verify** |
| `QuickFileMetrics_WRITE_WithNullList_SkipsBodyAndDoesNotThrow` | `EfcHomeControllerTests.cs` (name confirmed via `QuickFiler.Test`) | same. **No break expected — verify** |

**Break from the separator fix:**

- `BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine`
  (`EfcHomeControllerMetricsTests.cs:35`) — must be updated deliberately, per #451's constraint.

**Break from changing the `NotImplementedException` overload:**

- `QuickFileMetricsWriteFilenameOnly_PreservesNotImplementedContract`
  (`EfcHomeControllerMetricsTests.cs:138-148`). Its name and its `act.Should().Throw<NotImplementedException>()`
  at `:147` explicitly pin the defective contract. Must be rewritten or removed.

**Break from changing `Metrics.cs:121` to read `_stopWatchMoved`:**

- `WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps`
  (`QfcHomeControllerMetricsTests.cs:328`). It sets **only** `_stopWatch`
  (`SetPrivateField(controller, "_stopWatch", new Stopwatch());` at `:332`) and leaves
  `_stopWatchMoved` null. After the change, `Metrics.cs:121` dereferences a null
  `_stopWatchMoved` and the test throws `NullReferenceException`. **This test must be updated in
  the same commit** — set `_stopWatchMoved` instead of (or in addition to) `_stopWatch`.

**Break from the flush redesign (Option A, deleting `NonBlockingProducer`):**

- `NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay`
  (`QfcHomeControllerMetricsTests.cs:401`). Inspected in full: the body at `:404-416` never calls
  `NonBlockingProducer`. It exercises `_controller.TimeProvider.Delay(TimeSpan.FromMilliseconds(20))`
  directly. It will therefore **still compile and still pass** after `NonBlockingProducer` is
  deleted, but its name and XML doc (`:392-399`) would become false. Recommend renaming it (e.g.
  `TimeProviderDelaySeam_HonorsInjectedTwentyMillisecondDelay`) and rewriting the doc comment, or
  deleting it if the delay seam has no remaining production consumer. **Check:** after Option A,
  `TimeProvider.Delay` has no production call site — `Metrics.cs:222` is inside the deleted
  `NonBlockingProducer`. Deleting the test is then the honest choice; keeping it would assert only
  that `FakeTimeProvider` works.

**Unaffected but worth naming (they touch adjacent state):**

- `SwapStopWatch_ExecutesCorrectly` — `QfcHomeControllerIterationTests.cs:435-458`. Reads
  `_stopWatch` and `_stopWatchMoved` by reflection. Unaffected as long as both remain **fields**
  (a decisive argument against the property-snapshot idea rejected in B.2).
- `StopWatch_PropertyWorksCorrectly` — `QfcHomeControllerPropertyTests.cs:232-252`. Sets
  `_stopWatch` by reflection and asserts the `StopWatch` property returns it. Unaffected.
- `QfcHomeControllerRunAsyncTests.cs:303` — `Assert.IsTrue(_controller.StopWatch.IsRunning);`.
  Unaffected; and it is the template for the new EFC start assertion.
- `QuickFileMetrics_WRITE_WhenGetCalendarReturnsNull_DoesNotThrow`
  (`QfcHomeControllerMetricsTests.cs:76`) and `GetMoveDiagnostics_NullAppointment_DoesNotThrow`
  (`:162`). Both already set `_stopWatchMoved` (`:141-146`, `:225-230`), so the `Metrics.cs:42`
  widening does not disturb them.
- `QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine` (`:363`). Sets `_stopWatchMoved` at `:367`.
  Unaffected.

### D.4 `.csproj` entries and whether a new test file is needed

Both existing test files are already registered:

- `QuickFiler.Test/QuickFiler.Test.csproj:110` — `<Compile Include="Controllers\EfcHomeControllerMetricsTests.cs" />`
- `QuickFiler.Test/QuickFiler.Test.csproj:133` — `<Compile Include="Controllers\QfcHomeControllerMetricsTests.cs" />`

`QuickFiler.Test.csproj` is a legacy non-SDK project with explicit `Compile Include` entries, and
it is **not** in the owned-file list. **No new test file may be created.** All new test methods
must be added to the two existing files. This is achievable for every regression identified:

- #442 flush regression → `QfcHomeControllerMetricsTests.cs` (writer-seam capture, C.3).
- #443 wrong stopwatch → `QfcHomeControllerMetricsTests.cs` (duration argument reaching
  `GetMoveDiagnostics` is non-zero when `_stopWatchMoved` is populated and `_stopWatch` is fresh).
- #443 `.Seconds` truncation → `QfcHomeControllerMetricsTests.cs`.
- #451 stopwatch started → `EfcHomeControllerMetricsTests.cs` (`StopWatch.IsRunning`; note the
  `Mail`-null constraint from D.1).
- #451 `.TotalSeconds`, separator, `xComma` coverage → `EfcHomeControllerMetricsTests.cs`
  (`BuildQuickFileMetricLines` is a pure static — the cheapest possible assertions).
- #451 `NotImplementedException` overload → `EfcHomeControllerMetricsTests.cs` (rewrite the
  existing pinning test).

The two test files are 421 and 244 lines. The 500-line cap applies to test files as well
(`.claude/rules/general-code-change.md`, "File Size Limit"). **`QfcHomeControllerMetricsTests.cs`
has only ~79 lines of headroom.** Budget new QFC test methods tightly, or reuse
`BuildLooseMetricsController()` aggressively to keep each method short. `EfcHomeControllerMetricsTests.cs`
has ~256 lines of headroom and is comfortable.

---

## 6. Research question E — CSV contract and downstream consumers

### E.1 Is there any reader of the session metrics CSV?

**No. Verified.** A repo-wide grep for `EmailSession` restricted to code file types
(`*.cs`, `*.py`, `*.ps1`, `*.ipynb`, `*.R`, `*.sql`) returns exactly six files:

| File | Role |
|---|---|
| `TaskMaster/Properties/Settings.Designer.cs:436-454` | default filename `99999EmailSession.csv` |
| `TaskMaster/AppGlobals/AppStagingFilenames.cs:85-93` | settings-backed accessor |
| `UtilitiesCS/Interfaces/IGlobals/IAppStagingFilenames.cs:10` | interface declaration |
| `QuickFiler/Controllers/QfcHomeController.cs:373` | **writer** |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:229` | **writer** (supplies the filename) |
| `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:141` | **writer** |

There is **no parser, no reader, and no schema consumer** anywhere in the repository. The file is
write-only from the codebase's perspective; any consumption is external and manual (a spreadsheet).

**This materially lowers the risk of the separator and duration changes.** #451's "Constraints &
Risks" note about "potential downstream consumers" is therefore satisfied by evidence: none exists
in-repo. The residual risk is limited to a human-maintained spreadsheet whose column count would
shift by one on EFC rows. Recommend stating that explicitly in the PR description rather than
gating on it.

### E.2 `GetMoveDiagnostics` — signature, ownership, and emitted column order

**Signature** (`QuickFiler/Controllers/QfcCollectionController.cs:2272-2279`, declared on
`QuickFiler/Interfaces/IQfcCollectionController.cs:109`):

```csharp
public string[] GetMoveDiagnostics(
    string durationText,
    string durationMinutesText,
    double duration,
    string dataLineBeg,
    DateTime endTime,
    ref AppointmentItem olAppointment
)
```

**Ownership: NOT owned** (`QfcCollectionController.cs` belongs to feature 468). Read-only here.

**QFC emitted column order** (`QfcCollectionController.cs:2311-2322`). Note the literal space after
`{dataLineBeg}` at `:2312`, which leaves a leading space on the Subject column:

```
<date MM/dd/yyyy>,<time hh:mm>, <xComma(Subject)>,QuickFiled,<durationText>,<durationMinutesText>,
<xComma(ToRecipientsName)>,<xComma(SenderName)>,Email,<xComma(SelectedFolder)>,
<SentDate MM/dd/yyyy>,<SentDate HH:mm>
```

12 fields. All four free-text fields are `xComma`-sanitised. The `qf is null` fallback at
`:2320-2321` emits six literal placeholder columns, preserving the shape. Trailing array element is
`null` — see A.6.

**EFC emitted column order** (`EfcHomeController.Metrics.cs:76-83`), current defective form:

```
<date MM/dd/yyyy>,<time hh:mm>,<xComma(Subject)>,SingleSorted,<durationText>,<durationMinutesText>,
<ToRecipientsName><SenderName>,Email,<selectedFolder>,<SentDate MM/dd/yyyy>,<SentDate HH:mm:ss>
```

**11 fields today, 12 after the separator fix** — which then matches the QFC shape. Two further
divergences from QFC that the fix should also close, both inside the owned file:

- Only `Subject` is `xComma`-sanitised (`:79`). `ToRecipientsName`, `SenderName`, and
  `selectedFolder` are raw (`:80-81`). QFC sanitises all four. #451 Defect 5.
- The last column uses `HH:mm:ss` (`:82`) where QFC uses `HH:mm` (`:2316`). Recommend **leaving
  this alone** — it is not a defect (24-hour, unambiguous, higher precision), and changing it is an
  unforced column-content change.

`QfcCollectionController.xComma` (`QfcCollectionController.cs:2330-2344`) is `public static` and is
already called cross-class from `EfcHomeController.Metrics.cs:79`, so extending its use to the other
three fields requires no change to the forbidden file.

### E.3 The `CultureInfo.CurrentCulture` and `"hh:mm"` defects — scope call

**Culture defect.** `ToString("##0")` and `ToString("##0.00")` with no `IFormatProvider` bind to
`CultureInfo.CurrentCulture`. On any culture using `,` as the decimal separator (de-DE, fr-FR,
es-ES, pt-BR …), `durationMinutesText` renders as `2,00` and **splits one CSV field into two**,
corrupting every downstream row.

Affected sites, all with ownership:

| Site | Expression | Owned? |
|---|---|---|
| `QfcHomeController.Metrics.cs:31` | `$"{now:MM/dd/yyyy},{now:hh:mm},"` | **owned** |
| `QfcHomeController.Metrics.cs:53` | `duration.ToString("##0")` | **owned** |
| `QfcHomeController.Metrics.cs:56` | `(duration / 60d).ToString("##0.00")` | **owned** |
| `QfcHomeController.Metrics.cs:108,110` | `now.ToString("MM/dd/yyyy")`, `now.ToString("hh:mm")` | **owned** |
| `QfcHomeController.Metrics.cs:132` | `Duration.ToString("##0")` | **owned** |
| `QfcHomeController.Metrics.cs:135` | `(Duration / 60d).ToString("##0.00")` | **owned** |
| `EfcHomeController.Metrics.cs:67,68` | `currentDateTime.ToString("MM/dd/yyyy")`, `("hh:mm")` | **owned** |
| `EfcHomeController.Metrics.cs:73,74` | `duration.ToString("##0")`, `(duration / 60d).ToString("##0.00")` | **owned** |
| `EfcHomeController.Metrics.cs:81,82` | `SentDate.ToString("MM/dd/yyyy")`, `("HH:mm:ss")` | **owned** |
| `QfcCollectionController.cs:2294` | `{minutes:N0}`, `{seconds:N1}` (appointment body, not CSV) | **not owned** |
| `QfcCollectionController.cs:2316` | `SentDate.ToString(...)` | **not owned** |

**Every CSV-corrupting numeric site is in an owned file.** The forbidden sites are either the
appointment body (not CSV) or date formats using `/` and `:` literals, which are culture-stable
under a custom format string.

**Scope recommendation: fix the two numeric `##0` / `##0.00` families in the owned files
(`Metrics.cs:53,56,132,135` and `EfcHomeController.Metrics.cs:73,74`) by adding
`CultureInfo.InvariantCulture`; leave the date/time formats alone.**

Reasoning. This is not scope creep dressed as tidiness — it is a direct consequence of the fixes
being made. #442 is what causes the QFC CSV to be written *at all* for the first time, and #451's
`.TotalSeconds` widening is what causes `duration` to acquire a fractional part on the EFC path
(`120/7` becomes `17.142857…` instead of `17`). Both changes *increase* the surface on which a
comma decimal separator can corrupt a row. Fixing #442 and #443 while leaving the corruption in
place would ship a feature whose stated purpose — producing usable metrics — is defeated on any
non-invariant machine. Six `CultureInfo.InvariantCulture` arguments in two owned files is a
proportionate cost. `System.Globalization` needs adding to the `using` list in both files.

**`"hh:mm"` defect — recommend OUT of scope.** `hh` is the 12-hour clock and the format string
carries no `tt` designator, so 14:30 renders `02:30` and is indistinguishable from 02:30. Sites:
`QfcHomeController.Metrics.cs:31,110` and `EfcHomeController.Metrics.cs:68` — all owned, so it is
*possible*. It is nonetheless recommended out of scope because:

1. It is a **content** change to an existing column, not a **shape** change, and unlike the
   separator and culture defects it does not corrupt the row structure or block the feature's
   purpose. Ambiguous-but-parseable is materially different from structurally-broken.
2. It would break three passing tests on their asserted literals —
   `WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps` (`QfcHomeControllerMetricsTests.cs:336-337`),
   `QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine` (`:371-372`), and
   `BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine`
   (`EfcHomeControllerMetricsTests.cs:59`, `01:05`) — for a defect none of the three issues lists
   as an acceptance criterion. #443 mentions it only under "Related formatting defects … fix
   together or split as judged".
3. This feature is already changing the *duration* columns' values and the EFC row's *column
   count*. Adding a third simultaneous change to the *timestamp* column enlarges the diff a
   downstream spreadsheet owner has to reconcile in one step.

**Recommended disposition: promote the `"hh:mm"` defect to its own tracked issue** via the
promotion lifecycle, referencing `QfcHomeController.Metrics.cs:31,110` and
`EfcHomeController.Metrics.cs:68`, rather than leaving it as prose in this document.

---

## 7. Research question F — minimal change set, risk, and sequencing

### F.1 Recommended landing site for new code

`QfcHomeController.cs` is 487 lines against a 500-line cap — 13 lines of headroom. **New QFC code
must land in `QfcHomeController.Metrics.cs`** (234 lines, ~266 lines of headroom). Fortunately the
recommended change set is **net-negative** for `QfcHomeController.cs`:

| Change | File | Δ lines |
|---|---|---|
| Delete `_metrics`, `_metricsConsumers`, `_lockObject`, `_fileName` | `QfcHomeController.cs:353-358` | about −6 |
| Delete `TimedConsumerAsync` | `QfcHomeController.cs:362-386` | about −27 |
| **`QfcHomeController.cs` net** | | **about −33 → ~454 lines** |
| Add `MetricsFileWriter` seam (+ XML doc) | `QfcHomeController.Metrics.cs` | about +8 |
| Delete `NonBlockingProducer` ×2 and the dead consumer block | `QfcHomeController.Metrics.cs:190-232` | about −43 |
| Replace `:153-154` with a null-filtered awaited write | `QfcHomeController.Metrics.cs` | about +5 |
| **`QfcHomeController.Metrics.cs` net** | | **about −30 → ~204 lines** |

Both owned QFC files end comfortably under the cap with more headroom than they started with.
`EfcHomeController.cs` (441) gains no lines if `Stopwatch.StartNew()` replaces the two-line pattern;
`EfcHomeController.Metrics.cs` (87) grows by roughly 15 lines for the implemented overload plus
`xComma` calls. No file approaches 500.

### F.2 Minimal, lowest-risk change set (all inside owned files)

**#442 — never flushed** (`QfcHomeController.cs`, `QfcHomeController.Metrics.cs`)
1. Add `internal Func<string, string[], string, CancellationToken, Task> MetricsFileWriter` seam,
   defaulting to `FileIO2.WriteTextFileAsync`.
2. Replace `Metrics.cs:153-154` with a null/empty-filtered `await MetricsFileWriter(filename, lines, myDocuments, Token)`.
3. Delete `NonBlockingProducer` (both overloads) and the unreachable consumer-scheduling block
   (`Metrics.cs:190-232`).
4. Delete `_metrics`, `_metricsConsumers`, `_lockObject`, `_fileName`, `TimedConsumerAsync`
   (`QfcHomeController.cs:353-386`).
5. Remove the now-unused `using System.Collections.Concurrent;` (`QfcHomeController.cs:2`) and
   `using System.Timers;` (`:11`) — **verify no other member in the partial uses them** before
   removing; `FilerQueue` (`:435`) is a distinct type in `QuickFiler.Controllers`.

**#443 — duration misread** (`QfcHomeController.Metrics.cs`)

6. `:121` → `Duration = _stopWatchMoved.Elapsed.TotalSeconds;` (both defects, one line).
7. `:123` → `OlStartTime = OlEndTime.Subtract(_stopWatchMoved.Elapsed);` (aligns with `:44`).
8. `:42` → `double duration = _stopWatchMoved.Elapsed.TotalSeconds;`.
9. `:53, :56, :132, :135` → add `CultureInfo.InvariantCulture`.

**#451 — inert EFC duration** (`EfcHomeController.cs`, `EfcHomeController.Metrics.cs`)

10. `EfcHomeController.cs:76` and `:225` → `_stopWatch = Stopwatch.StartNew();`.
11. `EfcHomeController.Metrics.cs:23` → `.TotalSeconds`.
12. `EfcHomeController.Metrics.cs:35` and `:57` → `double elapsedSeconds`.
13. `EfcHomeController.Metrics.cs:80-81` → insert the missing `,` between `ToRecipientsName` and
    `SenderName`.
14. `EfcHomeController.Metrics.cs:80-81` → wrap `ToRecipientsName`, `SenderName`, and
    `selectedFolder` in `QfcCollectionController.xComma(...)`.
15. `EfcHomeController.Metrics.cs:73-74` → add `CultureInfo.InvariantCulture`.
16. `EfcHomeController.Metrics.cs:26-29` → implement the interface-mandated overload (see F.4).
17. `EfcHomeController.ExecuteMoves.cs:48-57` → replace the `volatile` check-then-set in
    `TryBeginExecuteMoves` with `Interlocked.CompareExchange`; change `_isExecuting`
    (`EfcHomeController.cs:389`) from `private volatile bool` to `private int`, and
    `ResetExecuteMovesState` (`ExecuteMoves.cs:59-62`) to `Interlocked.Exchange(ref _isExecuting, 0)`.
    All three sites are owned. (#451 Defect 3.)

**Defensive (see A.6)**

18. Filter `null`/whitespace entries out of the `GetMoveDiagnostics` result in `WriteMetricsAsync`
    before the write, so the trailing-`null` array element from
    `QfcCollectionController.cs:2284` does not append a blank line once flushing works.

### F.3 What cannot be fixed inside owned files — cross-feature notes

| Defect | Location | Owner | Note to raise |
|---|---|---|---|
| `SwapStopWatch()` races the metrics write on the `MoveAndIterate` path, making the QFC duration non-deterministic | `QfcFormController.EventHandlers.cs:157` vs `:161→:142` | **446** | Move `_parent.SwapStopWatch()` out of `LoadUiFromQueue()` (`:142`) to immediately after `_groups.CacheMoveObjects()` at `:156`, mirroring the end-of-database ordering at `:190-191`. Removes the race and makes both branches identical |
| `GetMoveDiagnostics` returns an array one element longer than it fills; the trailing element is always `null` | `QfcCollectionController.cs:2284` | **468** | Size the array `_itemGroupsToMove.Count`, not `+ 1`. Latent today; becomes a blank CSV line the moment #442 lands |
| `await UiThread.Dispatcher.InvokeAsync(async () => await WriteMetrics(...))` does not await the inner task — the metrics write is fire-and-forget past its first suspension | `QfcFormController.EventHandlers.cs:228-231` | **446** | Use `.Task.Unwrap()` (the pattern already present at `UtilitiesCS/Threading/WpfUiDispatcher.cs:61`) so failures surface and the write completes before `ActionCancelAsync` cancels the token |
| `"hh:mm"` renders 14:30 as `02:30` with no AM/PM designator | `QfcHomeController.Metrics.cs:31,110`; `EfcHomeController.Metrics.cs:68` | *owned, but recommended out of scope* | Promote to its own issue (E.3) |

### F.4 The `NotImplementedException` overload — how to satisfy #451 Defect 6

Removal is unavailable (`IFilerHomeController.cs:41`; §1 Refinement 3). Two owned-file options:

**Option 1 (recommended) — guarded delegation.** Derive the two missing arguments from state the
controller already holds, reusing the existing pure static:

```
public void QuickFileMetrics_WRITE(string filename)
{
    // guard: _formController / DataModel / Mail may all be null on the Find path
    // selectedFolder  <- _formController.SelectedFolder      (EfcFormController)
    // moved           <- SelectMoveMetricsItems(             (ExecuteMoves.cs:111, owned static)
    //                        DataModel.ConversationResolver.ConversationInfo.SameFolder,
    //                        _formController.MoveConversation,
    //                        DataModel.Mail.EntryID)
    // then delegate to the 3-arg overload (Metrics.cs:12)
}
```

This mirrors `ExecuteMovesCoreAsync` (`ExecuteMoves.cs:66-72`) exactly, adds no new seam, touches no
forbidden file, and returns early when prerequisites are absent — consistent with the existing
`moved is null || moved.Count == 0` guard at `Metrics.cs:18-21`. `SelectMoveMetricsItems` is
`internal static` and already unit-tested in isolation.

**Option 2 — throw `NotSupportedException` with a message** explaining that the EFC path requires
the folder and moved-item arguments. This satisfies "no bare `NotImplementedException`" literally
but leaves an interface member that cannot succeed. **Rejected** against the General Code Change
Policy's "Public APIs should be usable" and #451's AC wording.

The existing pinning test `QuickFileMetricsWriteFilenameOnly_PreservesNotImplementedContract`
(`EfcHomeControllerMetricsTests.cs:138-148`) must be rewritten under either option.

### F.5 Sequencing

Bugfix workflow (`CLAUDE.md`, "Bugfix Workflow") requires a failing regression test first, per
defect. Recommended order, each step ending in a full green toolchain pass:

1. **EFC first.** Its seams already exist, its tests are hand-fake based with no COM, and
   `BuildQuickFileMetricLines` is pure. Lowest risk, fastest red/green. Covers #451 items 10-16.
2. **EFC re-entrancy** (item 17) — independent of the metrics path; can ride along or be split.
3. **QFC stopwatch** (#443, items 6-9). One-line-per-site; requires updating
   `WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps` in the same commit.
4. **QFC flush** (#442, items 1-5, 18) last. It is the largest diff, it deletes the most code, and
   it is the change that makes A.6's blank-line defect observable — so it should land when the
   other two are already green and any new CSV output is already correct.

### F.6 Residual risks

| Risk | Likelihood | Mitigation |
|---|---|---|
| Deleting `NonBlockingProducer` orphans `TimeProvider.Delay`, leaving the seam's only consumer in a test | high (certain, if Option A is taken) | Delete or rename `NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay`; state the deletion in the change description (D.3) |
| Removing `using System.Collections.Concurrent;` / `using System.Timers;` breaks an unnoticed consumer in the same partial | low | Verify by compilation; the analyzer step will surface an unused-using or a missing-type error either way |
| `int` → `double` in `BuildQuickFileMetricLines` silently changes `##0` rounding for multi-item moves | medium | Add a `BuildQuickFileMetricLines` test with `moved.Count > 1` asserting the exact rendered `durationText`, so the new rounding is pinned deliberately |
| `QfcHomeControllerMetricsTests.cs` (421 lines) crosses the 500-line cap when new tests are added | medium | ~79 lines of headroom; reuse `BuildLooseMetricsController()` and keep methods short. If it overflows, the only compliant remedy is to move tests into `EfcHomeControllerMetricsTests.cs` (wrong home) — so budget carefully from the start |
| EFC L76 stopwatch-start assertion is unreachable with the existing test helper (`Mail` is null, so `EfcHomeController.cs:73` short-circuits) | high | Build a data model with a non-null `Mail` for that one test, or assert only the L225 (`InitAsync`) site and cover L76 by inspection. Record whichever is chosen |
| A human-maintained spreadsheet consumes the CSV and breaks on the EFC column-count change | unknown (no in-repo evidence either way) | E.1 establishes there is no in-repo consumer. State the column-shape change explicitly in the PR body |

---

## 8. Testing implications (strategy only — no test code)

Per `.claude/rules/general-unit-test.md` and the C# Unit Test Policy: MSTest, Moq,
FluentAssertions; no temp files; no `Thread.Sleep` / `Task.Delay` / wall-clock waits; tests live in
the existing two files (D.4).

**#442 — flush.** Red before / green after by asserting an injected `MetricsFileWriter` delegate is
invoked exactly once with the expected filename, folder root, and line array. Negative case:
`SpecialFolders` without `MyDocuments` → writer not invoked (mirrors
`QuickFileMetricsWrite_WithoutMyDocumentsFolder_DoesNotInvokeWriter`,
`EfcHomeControllerMetricsTests.cs:92`). Edge case: `GetMoveDiagnostics` returns an array with a
trailing `null` → the writer receives no null entries (pins item 18).

**#443 — wrong stopwatch.** Populate `_stopWatchMoved` with a non-zero interval and leave
`_stopWatch` at zero, then assert the `duration` argument reaching the mocked `GetMoveDiagnostics`
is non-zero. Red today (reads the zeroed `_stopWatch`), green after. Uses the existing
`SetPrivateField` helper and `Moq.Verify` with a `It.Is<double>(d => d > 0)` matcher — no clock.

**#443 — `.Seconds` truncation.** Assert on a `TimeSpan` exceeding one minute that the value
reaching `GetMoveDiagnostics` reflects the total, not the 0-59 component. A `Stopwatch` cannot be
set to an arbitrary elapsed value without either reflection into its internal ticks or a wall-clock
wait; the wait is prohibited. **Recommended approach:** assert the truncation fix on the EFC side,
where `BuildQuickFileMetricLines` takes the elapsed value as a plain `double` parameter and needs no
stopwatch at all. On the QFC side, assert only "the moved stopwatch was read", which is the
behaviourally significant half. Record this as a deliberate coverage boundary rather than papering
over it.

**#451 — stopwatch started.** `controller.StopWatch.IsRunning.Should().BeTrue()` after
construction, per the `QfcHomeControllerRunAsyncTests.cs:303` precedent. Two tests, one per
construction path, subject to the `Mail`-null constraint noted in F.6.

**#451 — CSV shape.** Extend the pure-function assertions on `BuildQuickFileMetricLines`: exact
line with the separator present; a comma inside `ToRecipientsName` / `SenderName` /
`selectedFolder` is stripped by `xComma`; a `moved.Count > 1` case pinning the new rounding.

**#451 — re-entrancy.** `TryBeginExecuteMoves` returns `true` once and `false` on the second call
before `ResetExecuteMovesState`; after reset it returns `true` again. Deterministic without
concurrency. A genuinely concurrent assertion is not deterministic and should not be attempted.

**#451 — interface overload.** Replace the `NotImplementedException` pin with: guarded early return
when `_formController` / `DataModel` are absent (does not throw), and delegation when they are
present.

**Coverage.** All five owned files are QuickFiler controller code. Per `CLAUDE.md`'s COM/VSTO
exemption, `EfcHomeController`/`QfcHomeController` classes that directly depend on
`Microsoft.Office.Interop.Outlook` without an injectable seam may be exempt — but the specific
members changed here (`BuildQuickFileMetricLines`, `SelectMoveMetricsItems`, the metrics writers
behind their seams, `TryBeginExecuteMoves`) are **testable seams and are explicitly NOT exempt**.
The changed lines must meet the floor. Baseline and final coverage artifacts belong under
`docs/features/active/quickfiler-home-controller-metrics-442/evidence/` per
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`.

---

## 9. Open items and unverified points

- **Timestamp minute component** is approximate; the shell clock could not be read (see the header
  note).
- **`Microsoft.Bcl.TimeProvider` DLL contents were not inspected directly** — the `packages/`
  directory is not restored in this worktree. `GetTimestamp()`/`GetElapsedTime()` availability is
  established *behaviourally*, from compiling production code at
  `QfcStreamingDequeueConfidenceGate.cs:102,110`, which is stronger evidence than a manifest read.
- **`QuickFileMetrics_WRITE_WithNullList_SkipsBodyAndDoesNotThrow`** was identified by name from
  `QuickFiler.Test` search results and archived TRX artifacts; the file
  `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs` was not read in full. Its behaviour under
  the `int` → `double` widening is **expected to be unaffected but is unverified** — confirm during
  implementation.
- **Whether `using System.Collections.Concurrent;` (`QfcHomeController.cs:2`) has another consumer
  in the same partial** was not exhaustively verified. `FilerQueue` at `:435` is a
  `QuickFiler.Controllers` type, not a concurrent-collections type, but the compiler is the
  authority here.
- **Line counts** for `QfcFormController.EventHandlers.cs` (399) and `QfcHomeController.Iteration.cs`
  (86) come from the tail of the Read output and are exact; the `QfcCollectionController.cs` and
  `EfcFormController.cs` figures in §0 are approximate (both exceed 500 lines already — a
  pre-existing condition owned by features 468 and 464 respectively, not introduced here).
