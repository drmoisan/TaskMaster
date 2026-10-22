# Research addendum: #972 fold and liveness-test determinism (issue #968)

- Issue: #968 `focus-and-theme-tests-leak-shared-dispatcher-setup`, with #972 (five residuals of the #950 review) folded in by the Coordinator Scope Amendment (`issue.md`, section "Coordinator Scope Amendment (2026-10-02T22-15, binding)").
- Branch: `bug/focus-and-theme-tests-leak-shared-dispatcher-setup-968`, item worktree head `4c6de5e84`, base `94287369`.
- Date: 2026-10-02
- Scope of this record: research questions (a) to (f) of the fold only. The #968 core design (fixture pin counting, the two dead theme-test calls, D1-D8) is in `research/2026-10-02T05-50-dispatcher-pin-call-sites-research.md` and is cited, not repeated. #972 item 5 (`try`/`finally` around `transactionA` in R4) is already designed there as D4; section 7 of this record only confirms it.
- Method: every file:line below was re-read in the item worktree during this session with the Read and Grep tools. Web-sourced facts are marked `[V-web]`. The Bash tool was not surfaced in this session, so `git -C <worktree> log --oneline 94287369..HEAD -- QuickFiler QuickFiler.Test` could not be executed here; the orchestrator's statement that main has not touched `QuickFiler/` or `QuickFiler.Test/` since the base is therefore carried as an orchestrator-supplied fact, not re-verified. All citations were taken from the worktree's current files, so they are valid for the worktree regardless.
- Paths are repository-relative. `FEATURE` = `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968`.

## 1. (a) The three `SynchronousBackgroundWorker` helpers

### 1.1 Declarations, quoted in full

`QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs:47-60`:

```csharp
        /// <summary>
        /// Test-side worker whose <see cref="RaiseDoWork"/> raises <c>DoWork</c> synchronously on
        /// the calling thread through the protected <c>OnDoWork</c>, so the privately subscribed
        /// <c>Worker_DoWork</c> runs to its first incomplete await before <c>InitEmailQueue</c>
        /// returns. Issue #950: this replaces the bounded waits on a thread-pool worker.
        /// </summary>
        private sealed class SynchronousBackgroundWorker : BackgroundWorker
        {
            public void RaiseDoWork() => OnDoWork(new DoWorkEventArgs(null));
        }

        /// <summary>The synchronous starter assigned to <c>QfcDatamodel.WorkerStarter</c>.</summary>
        private static void StartSynchronously(BackgroundWorker worker) =>
            ((SynchronousBackgroundWorker)worker).RaiseDoWork();
```

`QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs:59-73`:

```csharp
        /// <summary>
        /// Test-side worker whose <see cref="RaiseDoWork"/> raises <c>DoWork</c> synchronously on
        /// the calling thread through the protected <c>OnDoWork</c>, so the privately subscribed
        /// <c>Worker_DoWork</c> runs to its first incomplete await before <c>InitEmailQueue</c>
        /// returns (issue #950). Duplicated per file, following the convention documented on
        /// <c>QfcDatamodelLivenessTests</c>.
        /// </summary>
        private sealed class SynchronousBackgroundWorker : BackgroundWorker
        {
            public void RaiseDoWork() => OnDoWork(new DoWorkEventArgs(null));
        }

        /// <summary>The synchronous starter assigned to <c>QfcDatamodel.WorkerStarter</c>.</summary>
        private static void StartSynchronously(BackgroundWorker worker) =>
            ((SynchronousBackgroundWorker)worker).RaiseDoWork();
```

`QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs:114-127`:

```csharp
        /// <summary>
        /// Test-side worker whose <see cref="RaiseDoWork"/> raises <c>DoWork</c> synchronously on
        /// the calling thread through the protected <c>OnDoWork</c>, so no worker started by
        /// <c>InitEmailQueue</c> outlives the test (issue #950). Duplicated per file, following
        /// the convention documented on <c>QfcDatamodelLivenessTests</c>.
        /// </summary>
        private sealed class SynchronousBackgroundWorker : BackgroundWorker
        {
            public void RaiseDoWork() => OnDoWork(new DoWorkEventArgs(null));
        }

        /// <summary>The synchronous starter assigned to <c>QfcDatamodel.WorkerStarter</c>.</summary>
        private static void StartSynchronously(BackgroundWorker worker) =>
            ((SynchronousBackgroundWorker)worker).RaiseDoWork();
```

### 1.2 Diff

- Code: the three class bodies (`private sealed class SynchronousBackgroundWorker : BackgroundWorker { public void RaiseDoWork() => OnDoWork(new DoWorkEventArgs(null)); }`) and the three `StartSynchronously` starters are byte-identical. Members: one public method each, no fields, no constructor, no `Dispose` override.
- Doc comments differ in prose only (Liveness: "Issue #950: this replaces the bounded waits on a thread-pool worker"; Teardown and ZeroBatch: "Duplicated per file, following the convention documented on QfcDatamodelLivenessTests"; ZeroBatch additionally phrases the purpose as "so no worker started by InitEmailQueue outlives the test").
- Behaviour: identical. `OnDoWork` is `protected virtual` on `System.ComponentModel.BackgroundWorker`; raising it directly invokes the `DoWork` subscribers (here `QfcDatamodel.Worker_DoWork`, subscribed at `QuickFiler/Controllers/QfcDatamodel.cs:193`) on the calling thread and never starts a thread-pool work item.

### 1.3 Every use site (repo-wide grep `SynchronousBackgroundWorker` over `*.cs`: 13 lines in 3 files, all in `QuickFiler.Test/Controllers/`)

| File | Line | Use |
|---|---|---|
| `QfcDatamodelLivenessTests.cs` | 53 | declaration |
| | 60 | cast inside `StartSynchronously` |
| | 127 | `var worker = new SynchronousBackgroundWorker();` (test 1) |
| | 201 | `var worker = new SynchronousBackgroundWorker();` (`StartHeldOpenLoader`, used by tests 2, 3, 4) |
| `QfcDatamodelTeardownTests.cs` | 66 | declaration |
| | 73 | cast inside `StartSynchronously` |
| | 220 | `using (var worker = new SynchronousBackgroundWorker())` |
| `QfcInitEmailQueueZeroBatchTests.cs` | 33 | doc comment ("the nested SynchronousBackgroundWorker") |
| | 120 | declaration |
| | 127 | cast inside `StartSynchronously` |
| | 148 | `model.InitEmailQueue(0, new SynchronousBackgroundWorker())` inline |
| | 173 | `var worker = new SynchronousBackgroundWorker();` |
| | 221 | `model.InitEmailQueue(2, new SynchronousBackgroundWorker())` inline |

`StartSynchronously` assignments: Liveness `:128`, `:202`; Teardown `:222`; ZeroBatch `:143`, `:172`, `:202`.

### 1.4 Recommendation: one shared helper

- Location: new file `QuickFiler.Test/TestSupport/SynchronousBackgroundWorker.cs`. The project's existing shared test helpers live there (`TestSupport/WinFormsPumpHost.cs`, `TestSupport/DedicatedWorkerThread.cs`, both `internal` in namespace `QuickFiler.Test.TestSupport`, see `DedicatedWorkerThread.cs:4,21`). No existing test-support file is a natural host: `QfcItemController.TestSupport.cs` is item-controller specific (440 lines) and `BayesianPerformanceController.TestSupport.cs` / `QfcCollectionController.TestSupport.cs` are controller specific. A new file keeps the helper discoverable and avoids growing a near-limit file.
- Namespace and name: `QuickFiler.Test.TestSupport.SynchronousBackgroundWorker`. The three consumer files (namespace `QuickFiler.Controllers.Tests`) add `using QuickFiler.Test.TestSupport;`, the convention already used by six files (for example `Controllers/QfcItemController.SeamFactoryTests.cs`, `Controllers/QfcItemController.ViewerSetupTests.cs`).
- Shape: `internal sealed class SynchronousBackgroundWorker : BackgroundWorker` with `internal void RaiseDoWork()` and the starter moved onto the class as `internal static void StartSynchronously(BackgroundWorker worker) => ((SynchronousBackgroundWorker)worker).RaiseDoWork();`, so each consumer replaces `model.WorkerStarter = StartSynchronously;` with `model.WorkerStarter = SynchronousBackgroundWorker.StartSynchronously;` and deletes both its nested class and its private starter. Carry the Liveness doc comment (the fullest of the three) and drop the "Duplicated per file" sentences.
- `IDisposable`: `BackgroundWorker` derives from `System.ComponentModel.Component`, which implements `IDisposable` through the `Dispose()` / `protected virtual Dispose(bool)` pattern. The helper adds no fields, handles or subscriptions, so it must not override `Dispose(bool)`: there is nothing to release and an empty override would only invite analyzer noise. Disposal is the caller's responsibility (section 4).
- `QuickFiler.Test/QuickFiler.Test.csproj` (legacy non-SDK project, explicit `Compile` items; `*.csproj` is excluded from CSharpier by `.csharpierignore`). Neighbouring lines today:

```xml
226	    <Compile Include="Controllers\QfcQueueTests.cs" />
227	    <Compile Include="TestSupport\WinFormsPumpHost.cs" />
228	    <Compile Include="TestSupport\DedicatedWorkerThread.cs" />
229	    <Compile Include="TestSupport\WinFormsPumpHostTests.cs" />
230	    <Compile Include="NoLiveFormInTestAssemblyTests.cs" />
```

  Add after line 228: `    <Compile Include="TestSupport\SynchronousBackgroundWorker.cs" />`. The three consumer entries stay unchanged: `Controllers\QfcDatamodelLivenessTests.cs` (:157), `Controllers\QfcInitEmailQueueZeroBatchTests.cs` (:161), `Controllers\QfcDatamodelTeardownTests.cs` (:183). (The original research already requires a new entry next to :203 for the pin-count test file; both additions go in the same csproj edit.)

### 1.5 Line counts (physical lines, last `}` line of each file as read)

| File | Before | Expected after (estimate) | Basis |
|---|---|---|---|
| `QfcDatamodelLivenessTests.cs` | 312 | about 325 | minus 14 (helper + starter + blanks), minus about 10 (the two `Advance`/`Yield` blocks of test 1), plus about 15 (the `ArmingFakeTimeProvider` nested helper, section 5) and about 20 (test 1 rewrite, `using` blocks for the workers, helper-signature change) |
| `QfcDatamodelTeardownTests.cs` | 244 | about 229 | minus 15 (helper + starter + blanks); disposal already present |
| `QfcInitEmailQueueZeroBatchTests.cs` | 232 | about 225 | minus 14 (helper + starter), plus about 7 (three `using` blocks) |
| new `TestSupport/SynchronousBackgroundWorker.cs` | 0 | about 30 | class, starter, doc |

All stay far below 500. The executor records the exact counts in evidence.

## 2. (b) The `_remainingLoadActive` comment

### 2.1 Current comment, `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:15-24`

```csharp
        /// <summary>
        /// Issue #424: honest producer-liveness signal. Set <see langword="true"/> immediately before
        /// each <c>RunWorkerAsync()</c> call and cleared in a <c>finally</c> once the awaited
        /// <c>RemainingEmailLoader</c> completes. <c>BackgroundWorker.IsBusy</c> cannot serve this
        /// role: <c>Worker_DoWork</c> is <c>async void</c>, so it returns at its first yielding await
        /// and reports idle while the loader is still producing. Both the dequeue gate's
        /// <c>sourceActive</c> signal and <see cref="WaitForQueue"/> consume this flag. Declared
        /// <c>volatile</c> because it is written on the worker thread and read by dequeue callers.
        /// </summary>
        private volatile bool _remainingLoadActive;
```

The #950 review recorded the drift as CR-2 / F-2: "reword the `_remainingLoadActive` doc comment in QfcDatamodel.QueueProcessing.cs to name `WorkerStarter`" (`docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/code-review.2026-10-02T14-30.md:52`).

### 2.2 Every read and write across the solution (grep `_remainingLoadActive` over `*.cs`: 12 lines)

Production (3 writes, 2 reads):
- Write `true`: `QfcDatamodel.cs:284` (zero-batch path of `InitEmailQueue`, immediately before `WorkerStarter(worker)` at :285) and `:311` (positive-batch path, before `WorkerStarter(worker)` at :312).
- Write `false`: `QfcDatamodel.cs:227`, in the `finally` of `Worker_DoWork` (:221-228) that wraps `e.Result = await loaderTask` (:219); runs on success and on throw.
- Read: `QfcDatamodel.QueueProcessing.cs:305` (`() => _remainingLoadActive`, the `sourceActive` lambda handed to `QfcStreamingDequeueConfidenceGate`, consumed at `QfcStreamingDequeueConfidenceGate.cs:246`) and `:406` (`WaitForQueue` loop condition).

Tests (reflection by name only; no production API exposes the flag): `QfcDatamodelLivenessTests.cs:169` (`GetField("_remainingLoadActive")`), `QfcDatamodelTests.cs:114,126,266,279`, `QfcQueuePurePathsTests.cs:246`, `QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs:116` (all `SetPrivateField(model, "_remainingLoadActive", ...)`).

### 2.3 Actual post-#950 behaviour

- The flag is set to `true` by `InitEmailQueue` on the caller's thread, which in production is the `QfcHomeController.Run()` caller (`QfcHomeController.cs:252`) or a `Task.Run` pool thread via `InitEmailQueueAsync` (`QfcDatamodel.cs:330`), and in tests the MSTest worker thread. It is set immediately before `WorkerStarter(worker)`, not before "each `RunWorkerAsync()` call": since #950 the start goes through the injectable `WorkerStarter` seam (`QfcDatamodel.cs:144-152`), whose production value calls `RunWorkerAsync()` (:41, :53) and whose test value raises `DoWork` synchronously on the calling thread.
- It is cleared on the continuation thread of the awaited loader task, which is whichever thread completes `RemainingEmailLoader`'s task (a pool thread in production; in the liveness tests the test thread or a drained test-owned context). "Written on the worker thread" is therefore no longer accurate in either environment; what remains true is that writer and readers are different threads with no shared lock, which is the reason for `volatile`.
- The two consumers are unchanged.

### 2.4 Proposed replacement (summary body; the executor wraps to the file's column width)

```csharp
        /// <summary>
        /// Issue #424 producer-liveness signal, read by the dequeue gate's <c>sourceActive</c> and by
        /// <see cref="WaitForQueue"/>. <c>InitEmailQueue</c> sets it just before handing the worker to
        /// <see cref="WorkerStarter"/>, and <c>Worker_DoWork</c> clears it in a <c>finally</c> when the
        /// awaited <see cref="RemainingEmailLoader"/> task completes, because <c>BackgroundWorker.IsBusy</c>
        /// already reads idle at that handler's first incomplete await (issue #950 made the start
        /// synchronous in tests, so no particular thread owns either write). Volatile: the writers and
        /// the readers share no other fence.
        /// </summary>
```

Three sentences; states why the flag exists, why `IsBusy` cannot replace it, and why it is volatile, without restating the code.

## 3. (c) Legacy members in `QuickFiler/Controllers/QfcDatamodel.cs` (495 lines)

### 3.1 Candidates enumerated (every member of the file reviewed against callers)

Kept, with the proof that they are live:
- `Complete` (:158-163), `MovedItems` (:165-168), `InitEmailQueueAsync` (:317-333), `UndoMove` and `DequeueNextItemGroup(int)` (QueueProcessing :141, :339): all are members of `IQfcDatamodel` (`QuickFiler/Interfaces/IQfcDatamodel.cs:138-148`), so they implement interface members and are not unused regardless of direct call counts. `InitEmailQueueAsync` is also called at `QfcHomeController.cs:282`; `Complete` at `QfcHomeController.Iteration.cs:16` and `QfcFormController.EventHandlers.cs:276`; `DequeueNextItemGroup` at `QfcHomeController.Iteration.cs:78`.
- `Token`, `TokenSource` (:170-182): written by `LoadAsync` (:67-68) and read throughout.
- `SetupWorker` (:188-195), `Worker_DoWork` (:197-241), `InitEmailQueue` (:271-315), `LoadRemainingEmailsToQueueAsync(CancellationToken)` (:335-376, the one-argument overload that the constructors assign to `RemainingEmailLoader` at :40 and :52), `Application_NewMailEx` (:476-491), `Cleanup` (:77-103), `LoadAsync` (:56-75), both constructors: live.
- Commented-out field assignments `//_blockingQueue = null;` etc. (:99-101) are comments, not members.

Candidates that appear unused ("legacy"): four, examined in section 3.2.

### 3.2 Caller proof per candidate

1. `log` (:109-111), a second `private static readonly log4net.ILog` alongside `logger` (:28-30). Private, so reachable only from the three partial files or by reflection. Grep `\blog\b` over `QuickFiler/Controllers/QfcDatamodel*.cs`: three hits, the declaration at `QfcDatamodel.cs:109` and two prose words ("log level", "field log") in doc comments at `QueueProcessing.cs:71,90`. Repo-wide grep `"log"|GetField\("log|nameof\(log\)` over `*.cs`: no matches. Zero readers.
2. `Worker_RunWorkerCompleted` (:243-265, including its three-line comment). Grep over `*.cs`: the only references in `QuickFiler` are the declaration and the commented-out subscription at `:194` (`//worker.RunWorkerCompleted += new ...(Worker_RunWorkerCompleted);`). `QfcHomeController.cs:92,132,344,379` and `QfcHomeControllerRunAsyncTests.cs:325,376` concern a different method of the same name on `QfcHomeController`: the test's `GetMethod("Worker_RunWorkerCompleted", NonPublic | Instance)` is invoked on `_controller` (`QfcHomeControllerRunAsyncTests.cs:373-380`), a `QfcHomeController`. Zero subscribers, zero reflective callers.
3. `LoadRemainingEmailsToQueue(BackgroundWorker, CancellationToken)` (:378-416, synchronous). Grep `LoadRemainingEmailsToQueue\b` over `*.cs`: the declaration, the commented-out call at `:210`, two commented `nameof` uses (`:363`, `:404`), and two live `nameof(LoadRemainingEmailsToQueue)` expressions in log strings at `:369` and `:410`. `:410` is inside the method itself. `:369` is inside the live one-argument `LoadRemainingEmailsToQueueAsync` and is a compile-time symbol reference, not an invocation; it must be retargeted to `nameof(LoadRemainingEmailsToQueueAsync)` in the same edit (which also corrects the log line, which today names the wrong method). Zero invocations.
4. `LoadRemainingEmailsToQueueAsync(BackgroundWorker, CancellationToken)` (:418-465, the two-argument overload with the `#pragma warning disable CS0618` block). Grep `LoadRemainingEmailsToQueueAsync` over `*.cs`: `:40` and `:52` are method-group conversions to `Func<CancellationToken, Task<bool>>`, which overload resolution binds to the one-argument overload only (the two-argument overload is not applicable to a one-parameter delegate type); `:130` is a `<see cref>` naming the one-argument overload explicitly; `:209` is a commented-out call to the two-argument overload; `:462` is a commented `nameof` inside the method; the two test-file hits (`QfcInitEmailQueueZeroBatchTests.cs:28`, `QfcDatamodelLivenessTests.cs:104`) are doc prose about the one-argument loader. Zero invocations.

Cross-cutting checks: `QuickFiler` grants `InternalsVisibleTo("QuickFiler.Test")` (`QuickFiler/Properties/AssemblyInfo.cs:5`, `QuickFiler/Controllers/QfcHomeController.cs:15`) and `DynamicProxyGenAssembly2` (`QfcHighConfidencePreFilter.cs:11`, `Legacy/IAcceleratorCallbacks.cs:5`); all four candidates are `private`, so neither grant exposes them. The reflective sweep of the datamodel test files (`GetMethod("|GetField("|GetProperty("` over `QuickFiler.Test/Controllers/QfcDatamodel*.cs`) names only `ToggleOfflineMode`, `WaitForQueue` and `_remainingLoadActive`. The partial siblings are `QfcDatamodel.FrameBuilding.cs` and `QfcDatamodel.QueueProcessing.cs` (Glob `QuickFiler/Controllers/QfcDatamodel*.cs`); neither references any candidate. No designer file belongs to the type.

## Numeric Derivation Evidence

- Complete Family: log, Worker_RunWorkerCompleted, LoadRemainingEmailsToQueue, LoadRemainingEmailsToQueueAsync-BackgroundWorker-overload
- Exhaustive Search Scope: every `*.cs` file in the entire repository (all production, test, designer and notes files in every project), plus an extension-unfiltered sweep of the entire repository for the method names, plus the `IQfcDatamodel` interface, both `QfcDatamodel.*.cs` partial siblings, and the `InternalsVisibleTo` grants of `QuickFiler`
- Inclusion Rules: a member is counted as caller-free when every occurrence of its name outside its own declaration and body is a comment, a commented-out statement, a doc-comment reference, a reference to a different type's member of the same name, or a `nameof` symbol reference that is not an invocation and that the same edit retargets
- Exclusion Rules: members that implement an `IQfcDatamodel` interface member, members with any live invocation, event subscription, reflection-by-string lookup against `QfcDatamodel`, override, or designer wiring are excluded from the caller-free set; the one-argument `LoadRemainingEmailsToQueueAsync(CancellationToken)` overload is excluded because the constructors assign it to `RemainingEmailLoader`
- Primary Search Strategy or Query Expression: content grep with the Grep tool over every `*.cs` file in the entire repository using the alternation `Worker_RunWorkerCompleted|LoadRemainingEmailsToQueue\b|LoadRemainingEmailsToQueueAsync|InitEmailQueueAsync|TryUnhookOrReplace|UndoMove|MovedItems|DequeueNextItemGroup\(` plus `\blog\b` restricted to the three `QfcDatamodel*.cs` partials for the private field log, reading each hit to classify it as declaration, invocation, comment, `nameof`, or same-named member of another type, and separating the LoadRemainingEmailsToQueueAsync-BackgroundWorker-overload from the one-argument overload by each hit's argument list and delegate target type
- Primary Member Set: log, Worker_RunWorkerCompleted, LoadRemainingEmailsToQueue, LoadRemainingEmailsToQueueAsync-BackgroundWorker-overload
- Primary Count: 4
- Cross-check Search Strategy or Query Expression: files-with-matches grep with no file-type filter over the entire repository for the bare identifiers `Worker_RunWorkerCompleted|LoadRemainingEmailsToQueue` (97 files, of which 5 are `*.cs`: `QfcHomeController.cs`, `QfcDatamodel.cs`, `QfcInitEmailQueueZeroBatchTests.cs`, `QfcHomeControllerRunAsyncTests.cs`, `QfcDatamodelLivenessTests.cs`; the other 92 are Markdown, text evidence and agent-memory files), each `*.cs` file read in full at the hit lines to attribute the LoadRemainingEmailsToQueueAsync-BackgroundWorker-overload and LoadRemainingEmailsToQueue references, combined with the string-literal and reflection sweep `"log"|GetField\("log|nameof\(log\)` over `*.cs` for the private field log, a reflection sweep `GetMethod\("|GetField\("|GetProperty\("` over `QuickFiler.Test/Controllers/QfcDatamodel*.cs`, a read of `IQfcDatamodel.cs:101-167`, a Glob of `QuickFiler/Controllers/QfcDatamodel*.cs`, and a grep of `InternalsVisibleTo` under `QuickFiler/`
- Cross-check Member Set: log, Worker_RunWorkerCompleted, LoadRemainingEmailsToQueue, LoadRemainingEmailsToQueueAsync-BackgroundWorker-overload
- Cross-check Count: 4
- Member-set Comparison: the normalized primary and cross-check member sets are identical (the same four names in the same sense), and both counts are 4; the assertion that exactly these four members have zero callers is supported

### 3.3 Consequences of removing the four members

- Resulting physical line count of `QfcDatamodel.cs`: 495 minus 121 = about 374. Breakdown: `log` field :109-111 (3 lines); `Worker_RunWorkerCompleted` with its comment :243-265 plus the separating blank :242 (24); `LoadRemainingEmailsToQueue` :378-416 plus blank :377 (40); two-argument `LoadRemainingEmailsToQueueAsync` :418-465 plus blank :417 (49); the empty `#region Linked List Locking` / `#endregion` block :469-473 (5). Removing the commented-out references that only make sense with those members (`:194`, `:209-210`, `:363`) brings it to about 370. The executor records the exact figure.
- `using` directives: none becomes unused because of the removal. The two-argument overload's `ToAsyncEnumerable()`/`ForEachAwaitWithCancellationAsync` come from `System.Linq` (System.Linq.Async), which `Enumerable.Range`, `.Select` and `.ToList()` in `InitEmailQueue` still need; `TaskCanceledException`, `String.Format`, `MessageBox` (still used at :339) and `RunWorkerCompletedEventArgs`'s namespace `System.ComponentModel` (still used by `BackgroundWorker`) all remain referenced. Whether `System.Collections`, `System.Collections.Concurrent`, `System.Text`, `System.Xml.Linq` or `ToDoModel` (:2-3, :8, :12, :17) were already unused before this change was not established and is independent of it; leave them unless the analyzer build reports IDE0005 for them.
- `#pragma warning disable/restore CS0618` (:436, :457) disappears with the overload; it is the file's only pragma.
- Test coverage of the removed members: none. No test references any of the four (section 3.2), and the whole type is `[ExcludeFromCodeCoverage]` (`QfcDatamodel.cs:25`; a type-level attribute on one partial declaration applies to the type, so it covers all three partial files). The removed lines are therefore not in the measured denominator today and removing them changes no first-party figure. "Changed-line coverage must not drop" is satisfied vacuously; record the pre- and post-change first-party totals as unchanged within run noise.
- Behaviour: none of the four is reachable, so no production behaviour changes and no concurrency or ordering invariant is affected (section 8).

## 4. (d) Worker ownership and disposal

### 4.1 Production ownership (verified)

- `InitEmailQueue` stores the caller's worker in `_worker` (`QfcDatamodel.cs:273`), subscribes `Worker_DoWork` and registers a cancel callback on `_token` (:192-193; on the uninitialized test instances `_token` is `default`, whose `Register` is a no-op because `CanBeCanceled` is false), and starts it through `WorkerStarter` (:285, :312).
- `Cleanup()` only calls `_worker?.CancelAsync()` and nulls the field (:80, :102). Nothing in `QuickFiler/Controllers/QfcDatamodel*.cs` calls `Dispose` on the worker (grep `_worker\b.*Dispose|worker\.Dispose` under `QuickFiler/`: no matches).
- The production worker is `_formViewer.Worker` (`QfcHomeController.cs:252-255`, `:284`), a component owned by the form viewer, not by the datamodel. The datamodel never takes disposal ownership, so a test that disposes a worker it created cannot double-dispose anything.

### 4.2 Construction sites and current disposal

| Site | Current disposal | Proposed shape |
|---|---|---|
| `QfcDatamodelLivenessTests.cs:127` (test 1) | none | `using (var worker = new SynchronousBackgroundWorker()) { ... }` spanning the whole act/assert, so the worker outlives the loader release (section 5) |
| `QfcDatamodelLivenessTests.cs:201` (`StartHeldOpenLoader`, callers at :221, :249, :285) | none; the worker is created inside the helper and not returned | change the helper to accept the worker as a parameter (`StartHeldOpenLoader(SynchronousBackgroundWorker worker, Func<...> loaderBody, out TaskCompletionSource<bool> release)`) and have each of the three callers own it in a `using` block around the test body; this keeps ownership in the test method, mirrors `QfcDatamodelTeardownTests.cs:220`, and avoids disposing a worker the helper handed to the model |
| `QfcDatamodelTeardownTests.cs:220` | `using` block already | keep |
| `QfcDatamodelTeardownTests.cs:180` (`new BackgroundWorker {...}`) | `using` block already | keep |
| `QfcInitEmailQueueZeroBatchTests.cs:148` (inline `new` inside a lambda) | none | hoist into `using (var worker = new SynchronousBackgroundWorker())` around act and assert and pass `worker` |
| `QfcInitEmailQueueZeroBatchTests.cs:173` | none | wrap in `using` |
| `QfcInitEmailQueueZeroBatchTests.cs:221` (inline `new`) | none | hoist into `using` |
| `QfcDatamodelTests.cs:108`, `:261` (`new BackgroundWorker()` assigned by reflection) | none | related sibling nit in the same component; wrap in `using` (section 6.2) |

Use the `using (...) { }` block form, not a `using var` declaration: `QuickFiler.Test.csproj` sets no `LangVersion` (only `QuickFiler/QuickFiler.csproj:14` does, `preview`), a grep for `using var |\?\?=|is not null` finds hits in only five test files, and the block form is what the sibling file already uses (`QfcDatamodelTeardownTests.cs:180-181, 220`), so it is the conservative choice regardless of the effective language version.

Double-dispose and late continuations: `Component.Dispose()` is idempotent, the datamodel never disposes, and `Worker_DoWork`'s post-await line `bw.CancellationPending` (:232) is a plain property read that `BackgroundWorker` does not guard with a disposed check (framework knowledge, not re-verified against reference source). In test 2 (`RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces`, :218-234) the `release` source is created with `RunContinuationsAsynchronously` (:190-192), so the loader's continuation runs on a pool thread after `release.SetResult(true)` and may execute after the `using` block disposes the worker; by the above that read is harmless, and the behaviour is unchanged from today, where the worker is simply never disposed.

## 5. (e) Liveness test 1: scheduling dependence and a deterministic completion signal

### 5.1 The test as it stands, `QfcDatamodelLivenessTests.cs:109-164` (the act and cleanup, :127-164)

```csharp
            var worker = new SynchronousBackgroundWorker();
            model.WorkerStarter = StartSynchronously;

            // Act — the issue #244 zero-batch short-circuit is COM-free and starts the worker
            // through the issue #950 seam, which raises DoWork on this thread.
            model.InitEmailQueue(0, worker);

            loaderEntered
                .Task.IsCompleted.Should()
                .BeTrue("the synchronous starter must reach the injected RemainingEmailLoader");

            Task<IList<MailItem>> pending = model.DequeueNextItemGroupAsync(1, 200);
            fake.Advance(TimeSpan.FromMilliseconds(200));
            await Task.Yield();
            fake.Advance(TimeSpan.FromMilliseconds(200));
            await Task.Yield();

            // Assert
            pending
                .IsCompleted.Should()
                .BeFalse(
                    "the loader is still producing, so the gate must keep polling rather than treat "
                        + "an empty queue as an exhausted source and return an early partial batch"
                );

            // Cleanup — release the loader and let the dequeue drain on the honest signal.
            loaderRelease.SetResult(true);
            for (int i = 0; i < 20 && !pending.IsCompleted; i++)
            {
                fake.Advance(TimeSpan.FromMilliseconds(200));
                await Task.Yield();
            }

            pending
                .IsCompleted.Should()
                .BeTrue("once the loader completes, the gate exits on genuine exhaustion");
            (await pending).Should().BeEmpty();
```

### 5.2 What the production code does under the test

- `DequeueNextItemGroupAsync(1, 200)` (QueueProcessing :183-193) chains through the four-argument overload (:195-216; `_globals.QfSettings.HighConfidenceModeEnabled` is true from the strict mock at Liveness :90-98), `DequeueWithHighConfidenceGateAsync` (:264-278) and `DequeueWithHighConfidenceGateWithOutcomeAsync` (:292-315), which constructs the gate with `sourceActive: () => _remainingLoadActive` (:305) and the model's `TimeProvider` (:303), and awaits `gate.DequeueAsync(1, 200, _token)` (:311). None of these three awaits uses `ConfigureAwait(false)`, so each captures the `SynchronizationContext` current on the calling thread at call time.
- `QfcStreamingDequeueConfidenceGate.DequeueAsync` (`QfcStreamingDequeueConfidenceGate.cs:190-301`): with an empty master queue `_tryTakeNext()` returns null (:243); on the first pass `alreadyWaitedForEmptySource` is false, so it sets it and awaits `_timeProvider.Delay(200 ms).ConfigureAwait(false)` (:252-256). On each later pass it returns `SourceExhausted` only when `alreadyWaitedForEmptySource && !sourceCanStillProduce` (:246-250); while the flag is true it arms another 200 ms delay. The production completion point the test waits for is therefore the first poll that follows a delay and observes `_remainingLoadActive == false`, which returns at :249 and lets the three datamodel continuations complete `pending`.
- Timer mechanics `[V-web]`: on net481 `TimeProvider.Delay` is the `Microsoft.Bcl.TimeProvider` extension `TimeProviderTaskExtensions.Delay`, whose `DelayState : TaskCompletionSource<bool>` is constructed without `RunContinuationsAsynchronously` and is completed by the timer callback's `TrySetResult(true)`; `FakeTimeProvider` is an unsealed `public class FakeTimeProvider : TimeProvider`, does not override `Delay`, exposes `public override ITimer CreateTimer(...)`, and `Advance(TimeSpan)` invokes due timer callbacks synchronously on the calling thread (`WakeWaiters` -> `InvokeCallback`).

### 5.3 The scheduling dependence

1. After `fake.Advance(200)` fires the gate's timer, the gate's `ConfigureAwait(false)` continuation is run inline on the advancing thread only if that thread's `SynchronizationContext.Current` is null or exactly `SynchronizationContext` and `TaskScheduler.Current` is the default (the TPL's inlining rule for context-free await continuations); otherwise it is queued to the thread pool. MSTest worker threads carry whatever context an earlier test class left on them: `QfcItemControllerTestSupport.EnsureSynchronizationContext` installs a plain one and never restores it (`QfcItemController.TestSupport.cs:90-96`, D8 in the original research), and any test that installs a derived context on a pool thread without restoring it makes the next test on that thread take the queued path. The test cannot know which path it is on.
2. On the queued path the second `fake.Advance(200)` can run before the gate has re-armed its timer; a timer created afterwards is due 200 ms after the already-advanced clock, so that advance is lost. `await Task.Yield()` gives no ordering guarantee relative to a queued pool work item; it only moves the test to another pool thread, where the ambient context may differ again.
3. `loaderRelease.SetResult(true)` clears the flag inline (through the loader lambda's continuation and `Worker_DoWork`'s `finally`) only under the same inlining conditions on the thread that happens to run the test after the second `Yield`; otherwise the clear is posted and races the next advance.
4. The `for (i < 20 && !pending.IsCompleted)` loop is a bounded retry whose success depends on the pool scheduling the queued continuations within twenty yields. It passes today in practice; under pool starvation during the parallel run it can exhaust its budget, and the final `IsCompleted.Should().BeTrue()` then fails without any production defect. The sibling test `QfcDatamodelTests.cs:96-131` has the same first two steps (:116-119, :126-128) but no retry loop, so on the queued path its lost advance leaves `await pending` (:128) waiting indefinitely rather than failing (section 6.2).

### 5.4 Deterministic shape (test-only; no production seam required)

Signal 1, "the gate armed its next wait": a `FakeTimeProvider` subclass that completes a `TaskCompletionSource<bool>` after forwarding `CreateTimer` to the base. Precedents: `UtilitiesCS.Test/TestHelpers/ArmingBarrierTimeProvider.cs:19-54` (a forwarding decorator with `Armed`, `ReArm()`, and `RunContinuationsAsynchronously` signals, with the remark at :14-17 that advancing past a deadline the loop has not yet created hangs a test) and `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs:358-367` (`private sealed class CountingTimeProvider : FakeTimeProvider` overriding `CreateTimer`, which proves the override compiles in this project). Recommended: `internal sealed class ArmingFakeTimeProvider : FakeTimeProvider` with `Task Armed`, `void ReArm()`, and the override `CreateTimer` that calls `base.CreateTimer(...)` first and then `TrySetResult(true)`; signals created with `TaskCreationOptions.RunContinuationsAsynchronously` so a test continuation never runs inside the gate's `CreateTimer` call. Place it in `QuickFiler.Test/TestSupport/ArmingFakeTimeProvider.cs` (second consumer in section 6.2; add `<Compile Include="TestSupport\ArmingFakeTimeProvider.cs" />` next to the new worker helper). Subclassing is preferred over copying the UtilitiesCS.Test decorator because it is shorter and the project already subclasses `FakeTimeProvider`; linking the UtilitiesCS.Test file across projects would import a foreign namespace and was rejected.

Signal 2, "the dequeue completed": `pending` itself, the task the production code already returns.

Signal 3, "the loader completion cleared the flag": make it synchronous and self-checking. Register every production await of the loader path and the dequeue path while no `SynchronizationContext` is installed, so their continuations run inline on the completing thread instead of being posted to a context the test does not drain, then assert the flag through the existing `ReadLivenessFlag` (:167-172) before advancing the clock. A small `IDisposable` scope helper that sets `SynchronizationContext.Current` to null and restores the previous value on dispose (about 12 lines, nested in the test class) expresses this; `DrainableSynchronizationContext` (:68-87) is not used by test 1 because its `Drain()` must run on the creating thread (:80) and test 1 awaits across threads.

Proposed body (names and messages indicative):

```csharp
            using (var worker = new SynchronousBackgroundWorker())
            {
                model.WorkerStarter = SynchronousBackgroundWorker.StartSynchronously;
                Task<IList<MailItem>> pending;
                using (NoSynchronizationContext())
                {
                    model.InitEmailQueue(0, worker);
                    loaderEntered.Task.IsCompleted.Should().BeTrue("...");
                    pending = model.DequeueNextItemGroupAsync(1, 200);
                }
                clock.Armed.IsCompleted.Should().BeTrue("the gate arms its first empty-queue wait before returning");
                clock.ReArm();

                // Act — the first wait expires while the loader is still producing.
                clock.Advance(TimeSpan.FromMilliseconds(200));
                Task first = await Task.WhenAny(clock.Armed, pending);

                // Assert
                first.Should().BeSameAs(clock.Armed, "the gate must arm a second wait rather than return an early partial batch");
                pending.IsCompleted.Should().BeFalse("...");

                // Cleanup — complete the loader; with no captured context its continuations clear the flag inline.
                using (NoSynchronizationContext()) { loaderRelease.SetResult(true); }
                ReadLivenessFlag(model).Should().BeFalse("the loader's completion must clear the flag before the next poll");
                clock.Advance(TimeSpan.FromMilliseconds(200));
                (await pending).Should().BeEmpty();
            }
```

Why each step is deterministic:
- `pending` runs synchronously down to the gate's first `Delay`, so `Armed` is complete before the call returns; `ReArm()` precedes the advance, and the second arming can only happen after the advance, so the re-arm cannot be missed.
- `Task.WhenAny(clock.Armed, pending)` completes whether the gate's continuation is inlined inside `Advance` or queued to the pool. If the liveness flag regressed to a dishonest `IsBusy`-style signal, the gate would return after its first wait and `pending` would win the race, producing a crisp assertion failure instead of a hang; this is the regression sensitivity the old `IsCompleted.Should().BeFalse()` claimed but could not show.
- The flag checkpoint fails crisply if the inline assumption were ever violated (a different TPL rule or a custom scheduler), rather than flaking.
- The final advance fires the second timer; the gate's poll sees the flag false and returns `SourceExhausted`; the three datamodel continuations captured no context and complete `pending` on whichever thread ran the gate; `await pending` is the completion signal.
- No `Thread.Sleep`, `Task.Delay`, retry loop, timeout change, `[DoNotParallelize]` or `Workers=1`.

Test-only or production seam: test-only. The gate already takes the `TimeProvider` through the datamodel's `TimeProvider` property (`QfcDatamodel.cs:126`), and `pending` already is the production task. No production file changes for (e).

### 5.5 Showing the old shape is nondeterministic; fail-before status

A deterministic fail-before run of the old test is structurally impossible: its failure requires the pool to delay a queued continuation past a bounded retry, which the test cannot force, and the production behaviour under test is correct both before and after. The plan should record a fail-before exception dossier for this test-quality change with this structural reason. Two supporting observations can be recorded instead: (i) a reading of the mechanism in section 5.3 with the `[V-web]` facts in 5.2; (ii) an optional regression-sensitivity check of the new test by temporarily editing `QfcDatamodel.QueueProcessing.cs:305` to `() => false` in the executor's working copy, running only the two liveness-style tests, observing the `BeSameAs(clock.Armed)` assertion fail, and reverting before any gate. (ii) is evidence of the new test's sensitivity, not a fail-before of the old one, and must be labelled as such.

## 6. (f) Other related defects in the files above

### 6.1 Related, in scope

| # | File:lines | Defect | Change |
|---|---|---|---|
| F1 | `QfcDatamodel.cs:369` | The live one-argument `LoadRemainingEmailsToQueueAsync` logs `nameof(LoadRemainingEmailsToQueue)`, naming the wrong method; it also becomes a compile error once the synchronous method is removed. | Retarget to `nameof(LoadRemainingEmailsToQueueAsync)` in the removal edit. |
| F2 | `QfcDatamodel.cs:194`, `:209-210`, `:363` (and `:404`, `:462` inside removed members) | Commented-out code referring to the removed members. | Delete with the members. The remaining commented-out lines (`:63`, `:70`, `:99-101`, `:262-263` go with `Worker_RunWorkerCompleted`, `:301`, `:351`, `:394` goes with the sync method, `:478`) are the same kind of noise in a touched file; recommend deleting `:99-101` (fields that no longer exist) and leaving the rest to keep the diff reviewable, at the planner's discretion. |
| F3 | `QfcDatamodel.cs:469-473` | Empty `#region Linked List Locking` / `#endregion`. | Delete. |
| F4 | `QfcDatamodel.QueueProcessing.cs:285` | Doc comment cites `TryUnhookOrReplace` "(:31-66)"; the method is at `:146-181` of the same file. | Drop the stale line range (line numbers in doc comments do not survive edits). |
| F5 | `QfcDatamodelTeardownTests.cs:63-64`, `QfcInitEmailQueueZeroBatchTests.cs:33`, `:117-118`, `QfcDatamodelLivenessTests.cs:19-24` | Doc text about "duplicated per file" and "the nested SynchronousBackgroundWorker" becomes false after consolidation. | Reword alongside (a); the Liveness header keeps its sentence about the reflection helpers (section 6.3). |
| F6 | `QfcInitEmailQueueZeroBatchTests.cs:97-99` | Doc says assigning the inert loader "before starting a real BackgroundWorker is what makes it safe"; since #950 the worker is the synchronous test worker. | Minor reword while the file is open. |

### 6.2 Related sibling call sites in `QfcDatamodelTests.cs` (same component, same root cause; in scope under the directive)

- `DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive` (:96-131) drives the same gate with the same `fake.Advance(200); await Task.Yield();` step (:118-119) and then sets the flag false by reflection and advances once more before `await pending` (:126-128). On the queued path of section 5.3 the second advance can precede the gate's re-arm, after which no timer is due and `await pending` never completes. Apply the section 5.4 shape (`ArmingFakeTimeProvider`, `ReArm`, `WhenAny`, then the flag write, then the advance and `await pending`); because the flag is written directly by reflection here, no loader or context handling is needed. The file is 371 lines, so the edit fits.
- `:108` and `:261`: `new BackgroundWorker()` assigned into `_worker` by reflection and never disposed; wrap in `using` blocks (the `WaitForQueue` test at :253-283 is otherwise deterministic: its only advance follows the flag write, and the loop's first wait is armed synchronously by the reflective invoke).
- The `ToggleOfflineMode` test (:220-245) arms one delay synchronously and advances once; it is deterministic and needs no change.

### 6.3 Observed, intentionally not changed

- Duplicated `CreateUninitializedDatamodel` / `SetPrivateField` reflection helpers in the three touched files and in `QfcDatamodelTests.cs`, `QfcQueuePurePathsTests.cs`, `QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs`. This duplication is documented as a deliberate convention (`QfcDatamodelLivenessTests.cs:19-24`) and #972 item 1 names only the worker helper. Consolidating it would touch three further files outside the fold for no behavioural gain; leave it and keep the header sentence that documents it.
- `QfcDatamodel.QueueProcessing.cs:140-144` `UndoMove()` throws `NotImplementedException` behind a TODO. It implements `IQfcDatamodel.UndoMove` (:139) and is a pre-existing design gap unrelated to this item; no change and no new issue is needed beyond what is already visible in the code.
- Test 2 of the liveness file (:218-234) releases its loader with a `RunContinuationsAsynchronously` source and returns without observing the continuation. This is the #950 design and is unaffected by disposal (section 4.2); no change.
- `QfcItemControllerTestSupport.EnsureSynchronizationContext` never restores the installed context (D8 of the original research). It is the most likely source of the ambient-context variation in section 5.3, but changing it alters the premise of unrelated handler tests; the section 5.4 shape is robust to it by construction. No change; record as observed.

### 6.4 Completely unrelated (report for filing only)

None found in the files examined for this addendum.

## 7. Confirmation of #972 item 5 (D4)

`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` R4 (:218-251) disposes `transactionA` at :251 with no `try`/`finally`, exactly as D4 in `research/2026-10-02T05-50-dispatcher-pin-call-sites-research.md` (section 6) describes, with the same proposed fix (wrap in `try`/`finally` with an idempotent re-dispose, about six lines, 470 to about 476). Nothing in this addendum changes that design. The spec must record it as closing #972 item 5.

## 8. Paths the folded scope creates or modifies, coverage implications, invariants

Production (both files are in the `[ExcludeFromCodeCoverage]` type `QfcDatamodel`, `QfcDatamodel.cs:25`; `coverage.config` carries no `QfcDatamodel` entry, so the attribute is the only mechanism and it already removes the whole type from the measured denominator):
- `QuickFiler/Controllers/QfcDatamodel.cs`: remove the four caller-free members, the empty region and the dead commented references; retarget the `nameof` at :369. Coverage: no measured lines change; first-party totals unchanged. Invariants: none; all removed code is unreachable.
- `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`: doc-comment rewrite at :15-23 and the stale range at :285. Coverage: comment-only. Invariants: none.

Test:
- `QuickFiler.Test/TestSupport/SynchronousBackgroundWorker.cs` (new).
- `QuickFiler.Test/TestSupport/ArmingFakeTimeProvider.cs` (new).
- `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs`: helper removal, `using` ownership (including the `StartHeldOpenLoader` signature), test 1 rewrite, doc rewording.
- `QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs`: helper removal, doc rewording.
- `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs`: helper removal, three `using` blocks, doc rewording.
- `QuickFiler.Test/Controllers/QfcDatamodelTests.cs`: section 6.2 sibling test and two `using` blocks.
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`: D4 (already in the #968 plan).
- `QuickFiler.Test/QuickFiler.Test.csproj`: two new `TestSupport\...` `Compile` items after :228, plus the pin-count test item from the original research next to :203.
- Unchanged from the original research and still in scope: `QfcItemController.UiThreadDispatcherFixture.cs`, `QfcItemController.TestSupport.cs`, `QfcItemController.FocusAndThemeTests.cs`, and the new `QfcItemController.UiThreadDispatcherPinCountTests.cs`.
- `FEATURE/spec.md` and the plan: AC20 amended to name exactly the two production paths above; #972 items 1-5 and the liveness-test item mapped to ACs; the fail-before exception dossier for the test-quality rewrites (section 5.5).

Coverage of test assemblies is excluded by the coverage route (test-assembly exclusion derived in `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, cited in the original research section 7), so none of the test edits moves a figure.

Concurrency or ordering invariants: no production invariant changes. On the test side, the only ordering that changes is in the two rewritten dequeue tests, which replace an uncontrolled interleaving (advance, yield, retry) with explicit signals (`Armed`, `pending`, the flag checkpoint); the shared worker helper and the `using` blocks change no ordering because the datamodel never disposes or awaits the worker.

## 9. Testing implications summary

- Parallel regime unchanged (Workers=0, ClassLevel); no new `[DoNotParallelize]`, timeouts, sleeps, delays or retries.
- Run the four datamodel test classes (`QfcDatamodelTests`, `QfcDatamodelLivenessTests`, `QfcDatamodelTeardownTests`, `QfcInitEmailQueueZeroBatchTests`) together under `scripts/vscode/TaskMaster.cli.runsettings` before and after; all must pass; record the before/after line counts of the five touched test files and `QfcDatamodel.cs`.
- Evidence under `FEATURE/evidence/regression-testing/` for the two rewritten dequeue tests (pass-after, plus the labelled sensitivity check if performed) and `FEATURE/evidence/qa-gates/` for the unchanged first-party coverage totals; the fail-before exception dossier goes with the plan per the atomic-plan contract.
