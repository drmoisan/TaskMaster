# Issue #731 — QuickFiler controller lifecycle/disposal defects: minimal fix design

- **Date:** 2026-09-02T13-10
- **Issue:** #731 (consolidates #620, #621, #622, #634, #683)
- **Branch:** `bug/quickfiler-controller-lifecycle-disposal-defects-731` (cut from `origin/main`)
- **Scope of this document:** research and design only. No production code was written. No `spec.md`,
  `user-story.md`, or plan file was authored.
- **Tooling note:** this session had `Read`/`Grep`/`Glob`/`WebFetch`/`Write`/`Edit` only. No shell,
  no `git`, no `gh`. Every citation below was re-derived by reading the working-tree files directly;
  none was copied from the delegation prompt. Two file/line citations in the delegation prompt were
  wrong and are corrected in Finding 1.

All paths in this document are repository-relative.

---

## Executive summary of the five recommendations

| # | Finding | Recommendation | Change size |
|---|---|---|---|
| 1 | Three separate `EmailMoveMonitor` instances | **Do NOT share.** Sharing is a behaviour change that would silently drop two of three per-mail move actions. Adopt the issue's own second option: document why three instances are required, and add a structural regression test that pins the three-owner topology. | Comments + 1 new test file |
| 2 | `Cleanup()` never stops `_undoConsumerTask` | `CompleteAdding()` first, then defer `Dispose()` onto the consumer's completion (observing its fault). Never block the UI thread on `Task.Wait`. | ~15 lines in `QfcFormController.SetupDisposal.cs` + 1 new test file |
| 3 | Dead `scoreLoader` ctor parameter | Remove the parameter and its guard. Also remove the second dead parameter `globals` (verified equally dead — the issue did not notice it). Update the one test factory. | −8 lines prod, ~−20 lines test |
| 4 | Unsynchronised reentrancy-counter read | Replace the bare read with `Volatile.Read(ref …)`. Do **not** mark the field `volatile` — that produces CS0420 against the two `Interlocked` `ref` call sites, which the repo's `TreatWarningsAsErrors` gate turns into a build error. | 1 line |
| 5 | `SetupDisposal.cs` coverage debt (#683) | No separate work item in this plan. Finding 2's regression tests move this file's coverage as a side effect; record the post-fix figure as evidence and leave the residual gap to #683. | 0 |

---

## Finding 1 — three separate `IEmailMoveMonitor` instances

### 1.1 Verified current state

Three production field initialisers, all independently re-derived:

| File | Line | Declaration |
|---|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | 83 | `private IEmailMoveMonitor _moveMonitor = new EmailMoveMonitor();` |
| `QuickFiler/Controllers/QfcDatamodel.cs` | 103 | `private IEmailMoveMonitor _moveMonitor = new EmailMoveMonitor();` |
| `QuickFiler/Controllers/QfcQueue.cs` | 40 | `private IEmailMoveMonitor _moveMonitor = new EmailMoveMonitor();` |

**Correction to the delegation prompt.** The prompt attributes `_moveMonitor.UnhookAll(); _moveMonitor = null;`
to "QfcQueue.cs line ~80-81". Those two statements are at
`QuickFiler/Controllers/QfcDatamodel.cs:80-81`, inside `QfcDatamodel.Cleanup()` (`:75`). `QfcQueue`
never calls `UnhookAll` at all. The second `UnhookAll()` call site is
`QuickFiler/Controllers/QfcCollectionController.cs:751`, inside `RemoveControls()` (`:735`).

Complete production usage map:

| Owner | Hook sites (action registered) | Unhook sites |
|---|---|---|
| `QfcDatamodel` | `:357` (`_moveMonitor.HookItem` passed as a delegate to `QfcRemainingQueueAdmission`), `:384`, `:436` — action is `x => _masterQueue.Remove(x)` | `QfcDatamodel.QueueProcessing.cs:46`, `:215` (`UnhookItem`); `QfcDatamodel.cs:80` (`UnhookAll`) |
| `QfcCollectionController` | `:318`, `:346`, `:426`, `:1805`, and `QfcCollectionController.CarrierLoad.cs:59` — action is `x => RemovedItemMonitor(x.EntryID)` | `:876`, `:941` (`UnhookItem`); `:751` (`UnhookAll`) |
| `QfcQueue` | `QfcQueue.Enqueue.cs:91` — action is `async x => await RemoveItem(x)` | `QfcQueue.cs:76`, `:130` (`UnhookItem`) |

### 1.2 Object graph and construction order

`QuickFiler/Controllers/QfcHomeController.cs` `InitAsync` (`:108-150`) is the common ancestor of all
three, but it constructs only two of them directly:

1. `:122` — `QfcAsyncDataModelLoader(...)` is started **first and asynchronously**; the default
   (`:168-170`) is `QfcDatamodel.LoadAsync`, which reaches `new QfcDatamodel(appGlobals)` at
   `QfcDatamodel.cs:64`.
2. `:136` — `QfcQueue = QfcQueueLoader(Token, this, Globals)`; default at `:193-194` is
   `new QfcQueue(token, homeController, globals)`.
3. `:137` — `QfcFormControllerLoader(...)`; default at `:206-226` is
   `new QfcFormController(...).Init()`.
4. `:149` — `_datamodel = await dataModelTask`.

`QfcCollectionController` is a **grandchild**: it is constructed only inside `QfcFormController`, at
`QuickFiler/Controllers/QfcFormController.Actions.cs:50`, `:84`, and `:140` (three call sites). Its
sole other construction anywhere is the test at
`QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs:50`.

So the three owners are **not** siblings created by one factory call: the datamodel is created on a
background task that is awaited last, and the collection controller is created two levels down, well
after `InitAsync` returns.

### 1.3 Existing DI seam for `IEmailMoveMonitor`

`QuickFiler/Helper Classes/EmailMoveMonitor.cs:38` already has an injectable constructor:

```csharp
public EmailMoveMonitor(Action<System.Action> marshalToSta = null)
```

That seam exists for STA marshalling (defaulting to `UiThread.Dispatcher.Invoke`), not for instance
sharing. The constructor body only builds a delegate (`SetupBeforeItemMove()` at `:41`, defined
`:204-223`), so the type is safe to construct headless — `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`
does exactly that at `:94`, `:114`, `:138`, `:154`, `:182`, `:208` with `new EmailMoveMonitor(CountingPassThrough())`.

There is **no** existing constructor-injection precedent for `IEmailMoveMonitor` in any of the three
owner types, and no factory that returns one.

### 1.4 Blocking obstacle A — accessibility

`IEmailMoveMonitor` is declared `internal` (`QuickFiler/Interfaces/IEmailMoveMonitor.cs:13`) and
`EmailMoveMonitor` is `internal` (`QuickFiler/Helper Classes/EmailMoveMonitor.cs:18`).

All three owner types and every interface in the chain are `public`:

- `public partial class QfcCollectionController` (`QfcCollectionController.cs:22`) with a single
  `public` constructor (`:30-39`)
- `public partial class QfcQueue(...)` — a **public primary constructor** (`QfcQueue.cs:20-24`)
- `public partial class QfcDatamodel` (`QfcDatamodel.cs:26`) with `public QfcDatamodel(IApplicationGlobals, CancellationToken)` (`:43`)
- `public interface IQfcDatamodel` / `IQfcQueue` / `IQfcCollectionController` / `IQfcFormController`
  (`QuickFiler/Interfaces/IQfcDatamodel.cs:83`, `QuickFiler/Controllers/IQfcQueue.cs:12`,
  `QuickFiler/Interfaces/IQfcCollectionController.cs:14`, `QuickFiler/Controllers/IQfcFormController.cs:13`)

Adding an `IEmailMoveMonitor` parameter to any of those public constructors is **CS0051 (inconsistent
accessibility)**. It cannot be done without either (a) promoting `IEmailMoveMonitor` to `public`,
which widens QuickFiler's public surface against CLAUDE.md §C#5.2, or (b) adding parallel `internal`
constructor overloads to three public types plus widening three `internal Func<...>Loader` seam
delegate types on `QfcHomeController`.

An `internal` property seam (the shape `QfcDatamodel` already uses for `TimeProvider` at
`QfcDatamodel.cs:112` and `RemainingEmailLoader` at `:128`, and `QfcHomeController` uses for all six
`*Loader` properties) avoids CS0051 and is the repo-idiomatic mechanism. Accessibility is therefore
solvable. It is not the reason to reject sharing.

### 1.5 Blocking obstacle B — sharing is a behaviour change that loses move actions

This is the decisive finding, and it is not mentioned in the issue.

The same `MailItem` object is hooked by more than one owner. `QfcCollectionController` hooks the
items it receives from the datamodel's master queue (`:346`, `:426`), `QfcQueue` hooks the items it
enqueues (`QfcQueue.Enqueue.cs:91`), and `QfcDatamodel` hooked those same items when it loaded them
(`:384`, `:436`). Each owner registers a **different** move action.

Now read `EmailMoveMonitor`'s bookkeeping (`QuickFiler/Helper Classes/EmailMoveMonitor.cs`):

- `_hookedItems` is a `List<EmailMoveAction>` (`:44`) that permits duplicate mail EntryIDs.
- `HookItem` (`:46-61`) subscribes `folder.BeforeItemMove += BeforeItemMove` **only for the first
  hooked item of that folder** (`:56-57`).
- `BeforeItemMove` (`:206-222`) does
  `_hookedItems.FirstOrDefault(x => x.Mail.EntryID == mail.EntryID)` and then invokes
  `hookedItem.MoveAction(mail)` and removes **that one entry**.
- `UnhookItem` (`:63-88`) likewise resolves `FirstOrDefault(x => x.MailEntryId == mailEntryId)` and
  removes one entry; the folder unsubscribe is gated on `count == 1` per folder (`:78`, `:82`).
- `UnhookAll` (`:185-200`) clears the whole list for the instance.

Consequences of collapsing the three instances into one:

1. **Two of three move actions stop firing.** Today three separate monitors each add their own
   `folder.BeforeItemMove` subscription, so a move raises three handlers and all three actions run.
   With one shared monitor there is one subscription and one `FirstOrDefault`, so exactly one action
   runs and the other two mail entries are orphaned. `_masterQueue.Remove`, `RemovedItemMonitor`, and
   `QfcQueue.RemoveItem` are not interchangeable.
2. **`UnhookAll` scope collapses.** `QfcCollectionController.RemoveControls():751` runs on the
   page-teardown path (`RemoveControls` is reachable from `QfcCollectionController.cs:2128`, inside
   `Cleanup()` at `:2126`; the async twin is at `:2114`/`:2112`). With a shared monitor that call
   would also unhook every item the datamodel still has queued, so the master queue would stop
   tracking moves for the remaining session.
3. **Folder unsubscribe becomes wrong.** The `count == 1` predicate counts entries per folder; with
   three entries per mail the folder handler would be unsubscribed at a different, later point than
   today.

Making sharing safe would require changing `EmailMoveMonitor` to a multi-action, per-owner-scoped
registry (multi-map keyed by EntryID, per-owner unhook tokens, `UnhookAll` scoped to an owner). That
is a redesign of a COM-bound helper class with a live `BeforeItemMove` subscription — the opposite of
the "smallest deterministic change" the repo's bugfix workflow requires, and it would need a live
Outlook folder to validate end to end.

### 1.6 Recommendation for Finding 1

**Take the issue's second option: document, do not share.**

The issue text already sanctions this: *"Share one `IEmailMoveMonitor` instance … **or document
explicitly why three instances are intentional**"*.

Concrete minimal change:

1. Add an XML/inline comment above each of the three field initialisers
   (`QfcCollectionController.cs:83`, `QfcDatamodel.cs:103`, `QfcQueue.cs:40`) stating that the
   instance is deliberately per-owner because `EmailMoveMonitor.BeforeItemMove` dispatches at most
   one action per `MailItem` (`EmailMoveMonitor.cs:212-218`) and `UnhookAll` is instance-scoped
   (`:185-200`), so a shared instance would drop the other owners' move actions and would let one
   owner's teardown unhook another owner's items. Cite issue #731 finding 1 and #620.
2. Add one structural regression test that pins the topology, so a future "tidy-up" cannot silently
   collapse it. Suggested shape (source-inspection, matching the precedent in
   `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs:49-62`, `ReadControllerSource` /
   `ResolveRepositoryPath`): assert each of the three files contains exactly one
   `new EmailMoveMonitor()` field initialiser, and assert that no type declares more than one
   `IEmailMoveMonitor`-typed field.
3. Optionally correct the stale class comment at `EmailMoveMonitor.cs:17`
   (`// TODO: Determine what EmailMoveMonitor was supposed to be used for. It is now malfunctioning.
   Temprorarily disabling.`). The class is fully live with 9 production hook/unhook call sites; the
   comment is misleading and is the likely origin of the "state can diverge" framing in #620. This is
   a comment-only edit.

**Rejected alternative (recorded briefly):** constructor injection of one shared monitor created in
`QfcHomeController.InitAsync` and threaded through `QfcQueueLoader`, `QfcAsyncDataModelLoader`,
`QfcFormControllerLoader`, and the three `new QfcCollectionController(...)` sites in
`QfcFormController.Actions.cs`. Rejected for §1.5 (behaviour regression) primarily and §1.4
(CS0051 on three public constructors) secondarily. For the record, had it been pursued, the exact
edit set would have been: `QfcHomeController.cs:122/136/137/149` plus the seam property types at
`:156-170`, `:188-194`, `:196-226`; `QfcQueue.cs:20-24`; `QfcDatamodel.cs:34/43/54/64`;
`QfcFormController.cs:27-51`; `QfcCollectionController.cs:30-39`; and
`QfcFormController.Actions.cs:50/84/140`.

### 1.7 Tests relevant to Finding 1

All of these inject the monitor by **reflection on the private field name `_moveMonitor`**, so any
fix that keeps that field name is compatible; any fix that renames it breaks all of them.

- `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` — the monitor's own hook/unhook
  bookkeeping (`:23` class, `:53-54` `[TestCleanup]`, monitor built at `:94/:114/:138/:154/:182/:208`).
  This is where the "one action per mail" semantics of §1.5 are already exercised.
- `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:298`, `:316`, `:324` —
  `Mock<IEmailMoveMonitor>(MockBehavior.Loose)` set via `SetControllerField(controller, "_moveMonitor", …)`.
- `QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs:141-144` — documents that
  `_moveMonitor` is a field initialiser and is therefore `null` on `GetUninitializedObject` instances.
- `QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs:113-129`, `:140-153`, `:203-213` —
  `MockBehavior.Strict` monitor via `SetPrivateField(queue, "_moveMonitor", …)`.
- `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs:120-134`, `:171-186`, `:236-239` — same
  pattern against `QfcDatamodel`/`QfcQueue`.
- `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs:98-114`, `:236`
  — strict monitor injected into the datamodel (`SetPrivateField(model, "_moveMonitor", …)`).
- `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs:123`, `:134`, `:137`, `:189`, `:205`, `:208`
  — the only file in the repository that overrides `QfcDataModelLoader`, `QfcAsyncDataModelLoader`,
  `QfcQueueLoader` or `QfcFormControllerLoader`. It would have been the whole test-side blast radius
  of the rejected alternative.
- `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs:109-141` — asserts
  `typeof(QfcCollectionController).GetConstructors()` `.Should().ContainSingle()` and that
  `parameters[4]` is `IQfcFormController`. Any added **public** constructor overload or any parameter
  inserted before index 4 breaks this test. (An `internal` overload would not, since
  `GetConstructors()` returns public members only.)

---

## Finding 2 — `Cleanup()` never stops the undo consumer task

### 2.1 Verified current state

- Fields: `QuickFiler/Controllers/QfcFormController.cs:90-91`
  ```csharp
  private BlockingCollection<IMovedMailInfo> _undoQueue = [];
  private Task _undoConsumerTask;
  ```
- Start seam: `QuickFiler/Controllers/QfcFormController.Actions.cs:218`
  ```csharp
  internal Func<Func<Task>, Task> UndoConsumerStarter { get; set; } = body => Task.Run(body);
  ```
  It is a **plain `Task.Run` onto the ThreadPool** — there is no STA marshalling, no
  `TaskScheduler.FromCurrentSynchronizationContext`, and no dispatcher hop in the starter itself.
  The XML doc at `:213-217` states tests assign `body => body()` to run inline.
- Lazy start: `QfcFormController.Actions.cs:268` — `_undoConsumerTask ??= UndoConsumerStarter(UndoConsumer);`
  inside `UndoDialog()` (`:261`).
- Consumer loop: `QfcFormController.Actions.cs:317-351`. `while (!_undoQueue.IsCompleted)` (`:322`),
  non-blocking `_undoQueue.TryTake(out var item)` (`:324`), per-item work through the
  `UndoItemProcessor` seam (`:326`, seam declared `:230-234`), idle exit after
  `UndoConsumerIdleTimeout` = 10 s (`:315`, `:333-336`), otherwise
  `await TimeProvider.Delay(TimeSpan.FromMilliseconds(200))` (`:339-341`, clock seam at `:211`).
  `finally { _undoConsumerTask = null; }` (`:345-350`) — with the comment at `:347-348` already
  acknowledging *"disposing `_undoQueue` mid-take can produce"* an exception.
- Cleanup: `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs:210-230`. It calls
  `_undoQueue?.Dispose();` (`:218`) and `_parentCleanup?.Invoke();` (`:228`). `_undoConsumerTask` is
  **not referenced anywhere in the file** (verified: the identifier appears only at
  `QfcFormController.cs:91`, `Actions.cs:268` and `Actions.cs:349` in production).
- Default per-item work is genuinely UI-thread-bound: `ProcessUndoItemAsync`
  (`Actions.cs:241-259`) does live `MailItemHelper.FromMailItemAsync`, `item.UndoMove()`, and
  `await UiThread.Dispatcher.InvokeAsync(() => _groups.AddItemGroup(mail), ContextIdle)` (`:255-258`).

### 2.2 What the current race actually does

Verified against the Microsoft API reference (fetched 2026-09-02):

- `BlockingCollection<T>.Dispose()` — *"The `Dispose` method is not thread-safe."* and *"leaves the
  `BlockingCollection<T>` in an unusable state."*
- `BlockingCollection<T>.TryTake(out T item)` — declares **`ObjectDisposedException`: "The
  `BlockingCollection<T>` has been disposed."** All four `TryTake` overloads declare it.
- `BlockingCollection<T>.CompleteAdding()` — *"After a collection has been marked as complete for
  adding, adding to the collection is not permitted and attempts to remove from the collection will
  not wait when the collection is empty."* It also declares `ObjectDisposedException`, so
  `CompleteAdding` must be called **before** `Dispose`, never after.

So today, when `Cleanup()` disposes the queue while a consumer is parked in its 200 ms idle delay,
the consumer's next loop iteration touches `_undoQueue.IsCompleted` / `_undoQueue.TryTake` on a
disposed instance and throws `ObjectDisposedException`.

**Is it swallowed?** There is no `catch` in `UndoConsumer` — only `try/finally` (`:320`, `:345`). The
exception propagates out of the async method, faulting the `Task` returned by `Task.Run` at
`Actions.cs:218`. That task is stored in `_undoConsumerTask`, which the `finally` immediately sets to
`null`, and nobody ever awaits it. On .NET Framework 4.8.1 the default is
`ThrowUnobservedTaskExceptions = false` (no `<ThrowUnobservedTaskExceptions>` element exists anywhere
in this repository — verified by repo-wide grep), so the fault is dropped at finalisation with no log
entry. This is the same defect class as issue #670, and the repository has no
`TaskScheduler.UnobservedTaskException` backstop
(`docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/spec.md:107`
explicitly declined to adopt one).

### 2.3 Why a synchronous `Wait` is the wrong fix

`Cleanup()` has exactly **one** production caller: the unqualified `Cleanup();` at
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs:93`, inside `ActionCancelAsync()`
(`:84-94`), immediately after `await _formViewer.UiSyncContext;` (`:89`) — i.e. **on the UI thread**.
(`ActionCancelAsync` is reached from the `async void` `ButtonCancel_Click` at `:70-82`.)

If `Cleanup()` blocked on `_undoConsumerTask.Wait(timeout)`, and the consumer were suspended at
`await UiThread.Dispatcher.InvokeAsync(...)` inside `ProcessUndoItemAsync` (`Actions.cs:255`), the UI
thread would be blocked waiting for a continuation that only the UI thread can run. That is a hard
deadlock for the full duration of the timeout, and it directly violates the standing directive in
`.claude/agent-memory/orchestrator/feedback_vsto_startup_sta_threading_directive.md`
("The STA must always pump… never a synchronous block"). It would also introduce a wall-clock wait
into a code path the tests must drive, which `.claude/rules/general-unit-test.md` bans.

### 2.4 Recommended minimal fix

Replace `SetupDisposal.cs:218` (`_undoQueue?.Dispose();`) with a signal-then-deferred-dispose
sequence. Shape (design, not final code):

1. Capture `var queue = _undoQueue;` and `var consumer = _undoConsumerTask;` into locals.
2. `queue?.CompleteAdding();` — this is the **stop signal the loop already reads** at
   `Actions.cs:322`. Once the queue drains, `IsCompleted` becomes `true` and the loop exits normally
   through its existing `finally`. No new cancellation plumbing, no token, no change to
   `UndoConsumer`. Wrap in the narrowest possible guard for the already-disposed case.
3. If `consumer is null`, dispose the queue immediately (no consumer can be mid-`TryTake`).
4. Otherwise, dispose the queue from a continuation on `consumer` that runs on
   `TaskScheduler.Default`, and read the antecedent's `Exception` inside that continuation so the
   fault is observed and routed to the existing `logger`. This is the same "contain the fault at a
   boundary" shape already ratified for #670/#726 in
   `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs` (guarded wrapper + injectable
   error sink + null/throwing-sink defence at `:44-65`). A precedent for `ContinueWith` in this
   assembly exists at `QuickFiler/Controllers/EfcItemController.cs:203`.
5. Do **not** null `_undoQueue`: `UndoConsumer` dereferences the field on every iteration, and
   `UndoDialog` is already inert post-cleanup because its guard at `Actions.cs:263-266`
   (`if (_movedItems is null || _globals?.Ol?.App is null) return;`) trips once `Cleanup()` has set
   `_globals = null` (`SetupDisposal.cs:219`) and `_movedItems = null` (`:225`).

Notes on ordering and safety:
- `CompleteAdding()` before `Dispose()` is mandatory (the docs declare `ObjectDisposedException` on
  `CompleteAdding`), and it is also what makes the deferred dispose terminate.
- The one residual risk is a consumer that never drains because `UndoItemProcessor` hangs; in that
  case the queue is never disposed. That is strictly better than today's behaviour (dispose under an
  active consumer) and does not block the UI thread.
- File budget: `SetupDisposal.cs` is 234 lines, well under the 500-line ceiling, so the fix fits in
  the file the issue names. `QfcFormController.cs` is 195 lines and `QfcFormController.Actions.cs` is
  361 lines, both with headroom if a helper is preferred there.

**Rejected alternative (recorded briefly):** `_undoQueue.CompleteAdding()` followed by
`_undoConsumerTask?.Wait(TimeSpan.FromMilliseconds(N))` and then `Dispose()`. Rejected for §2.3
(UI-thread deadlock against `ProcessUndoItemAsync`'s dispatcher hop) and because it would put a
timing-dependent wait into a synchronous teardown path.

### 2.5 Tests relevant to Finding 2

Already covering adjacent behaviour (all in `QuickFiler.Test/Controllers/`):

- `QfcFormControllerSeamTests.cs` — the entire issue #448 region, `:356-494`. Helpers:
  `ArrangeUndoConsumer(clock, processor, queuedItems)` at `:374-390` (sets
  `UndoConsumerStarter = body => body()`, replaces `UndoItemProcessor`, and reaches `_undoQueue` by
  reflection at `:384`); `GetPrivateField`/`SetPrivateField` at `:43-47`; `CreateQfcFormController`
  at `:64-76`; `CountingTimeProvider` at `:359-368`. Existing tests:
  `UndoConsumer_EveryIdleIteration_InvokesTimeProviderDelay` (`:397`),
  `UndoConsumer_IdleBeyondThreshold_Completes` (`:414`),
  `UndoConsumer_SuccessfulTake_ResetsIdleTimer` (`:435`),
  `UndoConsumer_OnExit_ResetsUndoConsumerTask` (`:469`, plants a `Task.CompletedTask` sentinel into
  `_undoConsumerTask` and asserts both the idle and throwing exit paths clear it).
  **This file is 497 lines — three lines under the 500-line ceiling. A new region cannot be added
  here.**
- `QfcFormControllerTests.cs:253-264` — `Cleanup_ShouldCleanupResources`. It calls `_controller.Cleanup()`
  at `:260` and has **no assertions** (the body ends with a `// Assert / // Add assertions based on
  the expected behavior of the method` placeholder). This is the only test in the repository that
  invokes `QfcFormController.Cleanup()`, and it is currently vacuous.
- `QfcFormControllerUndoHandoffTests.cs` — issue #633 ordering tests. Relevant as a **pattern
  source**, not as coverage of this defect: it demonstrates deterministic concurrency via
  `TaskCompletionSource` gates and dispatcher queue order with no sleep/delay/timeout
  (`:196-214` `EnqueueOneGatedItemAsync`, `:230` `UiThreadDispatcherFixture.BeginTransactionAsync`,
  `:232` `QfcItemControllerTestSupport.StartRunningDispatcher`), and it already exercises the
  post-`Cleanup` null-`_parent` state at `:374-395`.
- `QfcFormControllerDeactivateTests.cs` — form deactivate handler wiring/unwiring
  (`:95`, `:112`, `:133`, `:152`, `:171`, `:193`, `:226`). It covers
  `RegisterFormEventHandlers`/`UnregisterFormEventHandlers`, which `Cleanup()` calls at `:217`, but
  nothing about the undo queue.

**Where the new regression test belongs.** A new file,
`QuickFiler.Test/Controllers/QfcFormControllerCleanupTests.cs`, because `QfcFormControllerSeamTests.cs`
is at 497/500 lines. `QuickFiler.Test.csproj` is a legacy (non-SDK) project with explicit
`<Compile Include="…" />` entries (e.g. `:139`, `:143`, `:149`, `:150`), so the new file **must** be
registered there or it will not compile into the assembly.

Suggested failing-first assertions (all achievable with the existing seams, no live COM, no
dispatcher, no wall clock):
1. Arrange a controller with `UndoConsumerStarter = body => body()`, a `FakeTimeProvider`, and an
   inert `UndoItemProcessor`; start the consumer; call `Cleanup()`; advance the fake clock; then
   assert the consumer task reaches `RanToCompletion` rather than `Faulted`. Pre-fix this fails with
   `ObjectDisposedException`.
2. Assert that after `Cleanup()` the queue's `IsAddingCompleted` was set before disposal (observable
   via the fault-free termination in (1); a direct `IsAddingCompleted` read after disposal would
   itself throw).
3. Assert `Cleanup()` with `_undoConsumerTask == null` still disposes the queue and does not throw
   (the never-opened-undo-dialog path, which is the common case).
4. Assert `Cleanup()` returns without blocking when the consumer is parked (guards against the
   rejected `Wait` design regressing in later).

---

## Finding 3 — dead `scoreLoader` constructor parameter

### 3.1 Verified current state

`QuickFiler/Controllers/QfcRemainingQueueAdmission.cs` is 48 lines total.

```
:9   internal sealed class QfcRemainingQueueAdmission
:11-13  three readonly fields: _addToQueue, _hookItem, _removeFromQueue
:15-21  internal ctor (globals, scoreLoader, addToQueue, hookItem, removeFromQueue)
:23-26  if (scoreLoader is null) throw new ArgumentNullException(nameof(scoreLoader));
:28-31  the other three are assigned with ?? throw
:34-46  TryQueueAsync — uses only _addToQueue, _hookItem, _removeFromQueue
```

`scoreLoader` appears exactly three times (`:17` declaration, `:23` guard, `:25` throw argument) and
is never assigned or invoked. **Additional finding not in the issue:** the first parameter
`IApplicationGlobals globals` (`:16`) appears exactly once — its own declaration. It is neither
guarded nor stored nor used. It is equally dead.

### 3.2 Callers

- **Production: exactly one.** `QuickFiler/Controllers/QfcDatamodel.cs:353-359`, inside
  `TryQueueRemainingMailItemAsync` (`:348`). It passes
  `async (m, t) => (await ScoreRemainingQueueMailItemAsync(m, t)).Score` as `scoreLoader` and
  `_globals` as `globals`.
- **Test: exactly one construction site.** `QuickFiler.Test/Controllers/QfcDatamodelTests.cs:39-45`,
  inside the private factory `CreateQueueAdmission(bool highConfidenceEnabled, double threshold,
  IList<MailItem> added, IList<MailItem> hooked, Func<MailItem, CancellationToken, Task<long>> scoreLoader)`
  at `:21-46`. Five tests call that factory: `:55`, `:82`, `:147`, `:174`, `:203`.

`ScoreRemainingQueueMailItemAsync` (in `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`) is
independently used and independently tested, so removing the dead parameter does not orphan it — this
matches the delegation prompt's note and is confirmed by the separate `_moveMonitor.UnhookItem`
call sites at `QfcDatamodel.QueueProcessing.cs:46` and `:215`.

### 3.3 Prior art and why "wire it up" is wrong

`.claude/agent-memory/task-researcher/project_qfc_high_confidence_dual_pipeline.md` records that the
non-scoring behaviour is **intentional** #233 design: threshold enforcement lives at dequeue time in
`QfcStreamingDequeueConfidenceGate`, not at admission. That intent is currently pinned by
`QfcDatamodelTests.TryQueueRemainingMailItemAsync_HighConfidenceEnabled_IgnoresThresholdAtAdmission`
(`:76-95`), which passes a `scoreLoader` that throws
`AssertFailedException("Threshold scoring belongs to dequeue-time enforcement.")` and asserts it is
never invoked.

So the parameter is not a half-finished feature; it is a vestige of a design that was deliberately
moved elsewhere. Wiring it up would re-introduce admission-time scoring that #233 removed.

### 3.4 Recommendation for Finding 3

Remove the dead parameter and its guard.

1. `QfcRemainingQueueAdmission.cs`: delete the `scoreLoader` parameter (`:17`) and the guard
   (`:23-26`). Recommend also deleting the equally-dead `globals` parameter (`:16`) in the same edit —
   leaving it behind reproduces exactly the API-dishonesty defect the issue is closing, in the same
   48-line file, at a cost of one production line and a handful of test lines. Flagging this as an
   orchestrator decision because the issue text names only `scoreLoader`; if the spec keeps scope
   strictly to `scoreLoader`, `globals` should be promoted as its own potential entry rather than
   left unrecorded. Remove the now-unused `using System.Threading;` / `using System.Threading.Tasks;`
   / `using UtilitiesCS;` only if the compiler/analyzers show them unused after the edit —
   `CancellationToken` and `Task` are still used by `TryQueueAsync` (`:34`), so
   `System.Threading` and `System.Threading.Tasks` must stay; `UtilitiesCS` is needed only by
   `IApplicationGlobals` and becomes removable if `globals` goes.
2. `QfcDatamodel.cs:353-359`: drop the corresponding arguments. The
   `async (m, t) => (await ScoreRemainingQueueMailItemAsync(m, t)).Score` lambda disappears entirely.
3. `QuickFiler.Test/Controllers/QfcDatamodelTests.cs`: remove the `scoreLoader` parameter from
   `CreateQueueAdmission` (`:21-46`) and from the five call sites. If `globals` is also removed, the
   `settings`/`globals` strict mocks at `:29-37` and the `highConfidenceEnabled`/`threshold` factory
   parameters become inert and should go with them.
4. `TryQueueRemainingMailItemAsync_HighConfidenceEnabled_IgnoresThresholdAtAdmission` (`:76-95`) loses
   its mechanism. **Do not simply delete it** — it is the only pin on the #233 intent. Replace it with
   a structural test asserting `QfcRemainingQueueAdmission`'s single constructor declares no
   `Func<MailItem, CancellationToken, Task<long>>` parameter and the type declares no scoring
   delegate field, carrying the same "threshold scoring belongs to dequeue-time enforcement"
   rationale in its `because:` message. The precedent for a structural pin of this kind is
   `QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs:41-75`.

`QfcRemainingQueueAdmission` is `internal sealed`, so this is not a public API break.

### 3.5 Tests relevant to Finding 3

- `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` — the only file that constructs the type.
  Factory `:21-46`; tests at `:49` (`…AddsAndHooksWithoutScoring`), `:76`
  (`…IgnoresThresholdAtAdmission`), `:141` (`…AddsBelowThresholdCandidate`), `:168`
  (`…HighConfidenceDisabled_AddsAndHooksWithoutScoring`), `:198`
  (`…NullMailItem_DoesNotScoreAddOrHook`). All five call `admission.TryQueueAsync` directly
  (`:67`, `:94`, `:159`, `:184`, `:213`); none goes through `QfcDatamodel.TryQueueRemainingMailItemAsync`
  despite the test names.

---

## Finding 4 — unsynchronised reentrancy-counter read

### 4.1 Verified current state

`QuickFiler/Controllers/QfcCollectionController.cs` (2327 lines total):

| Line | Statement | Kind |
|---|---|---|
| 909 | `private static int removespecificcontrolgroupcounter = 0;` | declaration |
| 913 | `Interlocked.Increment(ref removespecificcontrolgroupcounter);` | write (first statement of `RemoveSpecificControlGroupAsync`, `:911`) |
| 991 | `if (removespecificcontrolgroupcounter > 1)` | **bare read** |
| 1008 | `Interlocked.Decrement(ref removespecificcontrolgroupcounter);` | write (inside the issue-#286 `finally` at `:1002-1009`) |

Line 991 is the **only** read. The read's body is a `logger.Error(...)` diagnostic (`:993-995`), so a
missed observation degrades a diagnostic; it does not corrupt state. `QfcCollectionController` is
`partial` (`:22`) with a second part at `QfcCollectionController.CarrierLoad.cs`, which contains no
reference to the identifier.

### 4.2 Why `volatile` is the wrong choice here

Marking the field `volatile` and then passing it by `ref` to `Interlocked.Increment` (`:913`) and
`Interlocked.Decrement` (`:1008`) produces **CS0420** — *"a reference to a volatile field will not be
treated as volatile"* — at both call sites. The repository's type-check gate is

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

(CLAUDE.md § C#1.3 and the toolchain section), and neither `QuickFiler/QuickFiler.csproj` nor
`QuickFiler.Test/QuickFiler.Test.csproj` carries a `NoWarn` or `WarningsNotAsErrors` element — the
only `TreatWarningsAsErrors` mentions in either file are the comments at
`QuickFiler/QuickFiler.csproj:591` and `QuickFiler.Test/QuickFiler.Test.csproj:508` confirming that
analyzer severities are held at `suggestion` *specifically so nothing breaks that build*. So
`volatile` would turn two clean lines into two build errors. The issue's first suggestion is
therefore not viable as written.

### 4.3 Recommendation for Finding 4

Change line 991 to read through `Volatile.Read`:

```csharp
if (Volatile.Read(ref removespecificcontrolgroupcounter) > 1)
```

Leave the declaration at `:909` and both `Interlocked` writes untouched. One line changed, no new
`using` needed (`System.Threading` is already imported at `QfcCollectionController.cs:8`).

**Why `Volatile.Read` over `Interlocked.CompareExchange(ref …, 0, 0)`:** both are correct, but
`Volatile.Read` is a pure acquire load with no write traffic, it reads as a read at the call site,
and it is the established idiom **in this very assembly**:

- `QuickFiler/Viewers/WebView2Messenger.cs` — `private int _disposeRequested;` (`:25`),
  `Interlocked.Exchange(ref _disposeRequested, 1)` (`:75`), `Volatile.Read(ref _disposeRequested) != 0`
  (`:127`). This is the identical shape: `int` field, `Interlocked` writes, `Volatile.Read` guard.
- `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.Lifecycle.cs:214`
  — `private bool IsDisposed => Volatile.Read(ref _disposeState) != 0;` (same shape).
- `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:137`/`:352` — `Volatile.Read`/`Volatile.Write` pair,
  with the `#476` rationale in the surrounding comment at `:131-136` and `:351`.

`volatile` as a field modifier does exist in the repo, but only on fields that are **not** passed to
`Interlocked`: `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:23`
(`private volatile bool _remainingLoadActive`), `UtilitiesCS/Threading/CurrentStoreContext.cs:33`,
`QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs:30`, and the four fields in
`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs:39-42`. That distinction is exactly why
`Volatile.Read` is the right tool here.

### 4.4 Design smell to record, not to fix

`removespecificcontrolgroupcounter` is `private **static**`, so it is shared process-wide across every
`QfcCollectionController` instance. A QuickFiler session and a popped-out EFC session
(`QfcCollectionController.cs:730` constructs an `EfcHomeController`) would share it. Making it
instance-level would be a behaviour change to a diagnostic whose original intent (detect reentrancy
of `RemoveSpecificControlGroupAsync`) is arguably process-wide, and it would break the existing test
fixture that resets it as static state
(`QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs:41-79`, which uses
`BindingFlags.NonPublic | BindingFlags.Static`). **Recommend: flag as a follow-up potential entry,
do not change in this issue.**

### 4.5 Tests relevant to Finding 4

- `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` — owns the counter's test
  surface. `ReentrancyCounterField` const at `:30`; `[TestInitialize]` reset at `:41-57`;
  `[TestCleanup]` reset at `:63-79`; `ReadReentrancyCounter()` at `:84-98`. It also contains the
  issue-#286 tests named in the #634 potential doc
  (`RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter` and
  `…ThrowLaterInBody_RestoresReentrancyCounter`), which **must continue to pass unchanged** — a
  visibility fix changes no single-threaded observable behaviour.
- `QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs` — shared asserting reflection
  helpers and the `GetUninitializedObject` builder (documented at `:138-144`).

A memory-visibility fix cannot be proven by a deterministic unit test; a thread-racing test would
violate `.claude/rules/general-unit-test.md` determinism and UT1. The repo has already settled this
question: use a **structural proxy** test with an explicit disclaimer, per
`QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs:29-39` ("This assertion is a
STRUCTURAL PROXY for the memory-ordering fix and is explicitly NOT a proof that the race is
eliminated"). Recommended shape: a source-inspection assertion (using the
`ReadControllerSource`/`ResolveRepositoryPath` helpers already present at
`QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs:49-62`) that the only read of
`removespecificcontrolgroupcounter` in `QfcCollectionController.cs` goes through `Volatile.Read`,
plus a copy of the same disclaimer in the `<remarks>`.

---

## Finding 5 — `QfcFormController.SetupDisposal.cs` coverage debt (#683)

### 5.1 Current scope and status of #683

`gh` was not available in this session (no shell tool), so the GitHub issue could not be read
directly. The authoritative in-repo record is
`docs/features/potential/promoted/2026-08-28-qfcformcontroller-setupdisposal-coverage-debt.md`:

- Issue #683, `https://github.com/drmoisan/TaskMaster/issues/683`, captured 2026-08-28, Last Updated
  2026-08-28, Impact **Low**.
- Status line (`:5`): `Promoted -> docs/features/active/qfcformcontroller-setupdisposal-coverage-debt/ (Issue #683)`.
- Reported figure (`:14`, `:33`): whole-file line coverage **70.70 %**, **46 lines uncovered**, all 46
  verified pre-existing at the issue-#677 merge-base baseline; the two lines #677 added to this file
  are 100 % covered.
- Proposed work (`:52-54`): a dedicated test-coverage pass over the 46 uncovered lines, then re-run
  full-suite coverage and confirm the file reaches >= 80 %.
- Cited evidence source (`:38`, `:48`):
  `docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/feature-audit.2026-08-28T12-31.md`
  and `…/policy-audit.2026-08-28T12-31.md` sections 5 and 8.

### 5.2 Is there in-flight work that this plan could collide with?

**No.** Verified by enumerating every `docs/features/active/*/issue.md`: there are 40 active feature
folders and **none** is `…-683` or `qfcformcontroller-setupdisposal-coverage-debt`. The status line in
the promoted document names a target folder that has not been created. The only #683 material in the
repository is the promoted potential document above plus a passing mention in
`docs/features/potential/promoted/2026-09-02-quickfiler-controller-lifecycle-disposal-defects.md:41`.

The active folder for this issue,
`docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/`, currently
contains `issue.md`, a templated `spec.md`, and `plan.2026-09-02T12-02.md`. The `spec.md` is the
promotion template verbatim (its "Actual" section at `:36-40` is the issue body); it does not yet
commit to any design, so nothing in this research contradicts an approved decision.

### 5.3 Is a current coverage figure for the file available?

No current figure exists in this working tree. Searching `docs/features/active/*/evidence/` produced
no coverage artifact that names `QfcFormController.SetupDisposal.cs`; the only coverage XML files in
the repository are under `docs/features/archive/*/evidence/`, all predating 2026-06-25 and therefore
predating both #677 and the current file contents. The 70.70 % / 46-lines figure from the #683
document (measured 2026-08-28 against the #677 feature-review run) is the most recent available and
should be treated as the baseline, not as the current value.

### 5.4 Recommendation for Finding 5

**No separate action in this issue's plan.** Rationale:

1. Finding 2's fix lands in `SetupDisposal.cs` — specifically in `Cleanup()` (`:210-230`), which is
   part of the currently-uncovered surface. The new regression tests proposed in §2.5 will execute
   `Cleanup()` on at least four distinct paths (consumer running, consumer null, consumer parked,
   consumer already completed), where today the only test that calls it
   (`QfcFormControllerTests.cs:253-264`) is assertion-free.
2. The repo's post-change toolchain already runs `vstest.console.exe … /EnableCodeCoverage`
   (CLAUDE.md § CUT3 step 4), so the new figure is produced as a by-product of the mandatory gate.
   Recording it costs one evidence file, not a work item.
3. The remaining 46-line gap covers `CaptureItemSettings`, `RemoveTemplatesAndSetupTlp`,
   `SetupLightDark`, `SpaceForEmail`, `LoadItemsPerIteration`, and the
   `RegisterFormEventHandlers`/`UnregisterFormEventHandlers` bodies — all WinForms/`IQfcFormViewer`-bound
   surface unrelated to this issue's four code findings. Closing that is #683's stated scope
   ("pure test-addition work") and would be scope creep here.

Concrete: add a single acceptance criterion of the form *"re-measure `SetupDisposal.cs` whole-file
line coverage in the final QA run and record it under
`docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/evidence/coverage/`,
comparing against the #683 baseline of 70.70 % / 46 uncovered lines; leave any residual gap to #683."*
Do **not** make reaching >= 80 % on this file an acceptance criterion of #731.

---

## Numeric Derivation Evidence

Four numeric claims in this document could become `spec.md` acceptance criteria. Each is derived
below with two independent enumerations.

### N1 — `IEmailMoveMonitor` instance owners in production = 3

- **Complete family:** every production declaration in the `QuickFiler` project of a field, property,
  parameter, or local of type `IEmailMoveMonitor`, together with every `new EmailMoveMonitor(...)`
  expression.
- **Exhaustive search scope:** all `.cs` files under `QuickFiler/` (production assembly), including
  every part of every `partial` class. Test and docs files are excluded from the count but were
  inspected to confirm they are not production owners.
- **Inclusion rules:** a site counts as an *owner* if it both declares an `IEmailMoveMonitor`-typed
  member and initialises it with an instance the type controls.
- **Exclusion rules:** the interface declaration itself; the implementing class declaration; call
  sites that merely invoke `HookItem`/`UnhookItem`/`UnhookAll` on an already-declared member; all
  `QuickFiler.Test/` files; all `docs/` and `.claude/` files.
- **Primary search strategy:** ripgrep for the alternation `_moveMonitor|IEmailMoveMonitor|new EmailMoveMonitor`
  restricted to `QuickFiler/**/*.cs`. This deliberately covers all three of the field name, the type
  name, and the construction expression, so a renamed field or a differently-named local is still caught.
- **Primary member set:**
  1. `QuickFiler/Controllers/QfcCollectionController.cs:83`
  2. `QuickFiler/Controllers/QfcDatamodel.cs:103`
  3. `QuickFiler/Controllers/QfcQueue.cs:40`
  (Non-owner hits in the same result: `QfcQueue.Enqueue.cs:91`; `QfcQueue.cs:76`, `:130`;
  `QfcCollectionController.CarrierLoad.cs:59`; `QfcDatamodel.cs:80`, `:81`, `:357`, `:384`, `:436`;
  `QfcCollectionController.cs:318`, `:346`, `:426`, `:751`, `:876`, `:941`, `:1805`;
  `QfcDatamodel.QueueProcessing.cs:46`, `:215`; plus the two declarations
  `QuickFiler/Interfaces/IEmailMoveMonitor.cs:13` and `QuickFiler/Helper Classes/EmailMoveMonitor.cs:18`.)
- **Primary count:** 3
- **Cross-check search strategy:** a deliberately different query — ripgrep for the bare substring
  `MoveMonitor` (no underscore, no `new`, no `I` prefix) across **all** `.cs` files in the repository
  in `files_with_matches` mode, then open each production file returned and read its member
  declarations directly. This is a different expression over a wider scope, so it would catch an
  owner whose field is named something other than `_moveMonitor`.
- **Cross-check member set:** the query returned 13 files. Production files:
  `QuickFiler/Interfaces/IEmailMoveMonitor.cs` (interface decl — excluded),
  `QuickFiler/Helper Classes/EmailMoveMonitor.cs` (impl decl — excluded),
  `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` (comment only — excluded),
  `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` (comment only — excluded),
  `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` (usage only — excluded),
  **`QuickFiler/Controllers/QfcQueue.cs` (owner, `:40`)**,
  **`QuickFiler/Controllers/QfcDatamodel.cs` (owner, `:103`)**,
  **`QuickFiler/Controllers/QfcCollectionController.cs` (owner, `:83`)**.
  Test files (excluded by rule): `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`,
  `QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs`,
  `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs`,
  `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs`,
  `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`.
- **Cross-check count:** 3
- **Member-set comparison:** normalised to `{file:line}`, primary = `{QfcCollectionController.cs:83,
  QfcDatamodel.cs:103, QfcQueue.cs:40}`; cross-check = the same three. Identical. Assertion accepted.

### N2 — reads of `removespecificcontrolgroupcounter` in production = 1 (and writes = 2)

- **Complete family:** every syntactic occurrence of the identifier
  `removespecificcontrolgroupcounter` in production source, classified as declaration, read, or write.
- **Exhaustive search scope:** the entire repository (all file types), so that no `partial`-class part
  and no reflective string reference is missed. `QfcCollectionController` is `partial`
  (`QfcCollectionController.cs:22` + `QfcCollectionController.CarrierLoad.cs`), so a
  single-file read would not be exhaustive; the identifier is also `private static`, so a reflective
  string reference from a test is a real possibility and must be surfaced.
- **Inclusion rules:** a *read* is any occurrence whose value is consumed without being passed by
  `ref` to an `Interlocked` method. A *write* is any `Interlocked.*(ref …)` occurrence. The field
  initialiser at the declaration is neither.
- **Exclusion rules:** occurrences in `docs/`, `.claude/`, and `QuickFiler.Test/`.
- **Primary search strategy:** repository-wide ripgrep for the literal identifier
  `removespecificcontrolgroupcounter`, then classify each production hit by reading the surrounding
  statement.
- **Primary member set (production only):**
  - `QuickFiler/Controllers/QfcCollectionController.cs:909` — declaration
  - `QuickFiler/Controllers/QfcCollectionController.cs:913` — write (`Interlocked.Increment`)
  - `QuickFiler/Controllers/QfcCollectionController.cs:991` — **read** (`if (… > 1)`)
  - `QuickFiler/Controllers/QfcCollectionController.cs:1008` — write (`Interlocked.Decrement`)
  (One test hit, excluded by rule: `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs:30`,
  the `ReentrancyCounterField` string const used for reflective get/set.)
- **Primary count:** reads = 1, writes = 2, declarations = 1.
- **Cross-check search strategy:** a different method entirely — a contiguous line-by-line read of the
  whole enclosing region `QfcCollectionController.cs:860-1010` (which spans the field declaration and
  the complete body of `RemoveSpecificControlGroupAsync` from its opening brace to its closing
  `finally`), plus a separate read of the region `:720-800` and `:300-440` to confirm no other member
  of the class touches the counter, plus a `Cleanup`-targeted grep over
  `QuickFiler/Controllers/*.cs` that returned no counter reference from the second `partial` part.
- **Cross-check member set:** `{:909 declaration, :913 Interlocked.Increment, :991 bare read,
  :1008 Interlocked.Decrement}` — no further occurrence appears between `:860` and `:1010`, and the
  `RemoveSpecificControlGroup` synchronous twin (`:860-907`) contains none.
- **Cross-check count:** reads = 1, writes = 2, declarations = 1.
- **Member-set comparison:** primary and cross-check member sets are identical at
  `{909, 913, 991, 1008}` with identical classifications. Assertion accepted.

### N3 — `QfcRemainingQueueAdmission` constructor call sites: production = 1, test = 1

- **Complete family:** every expression in the repository that constructs a
  `QfcRemainingQueueAdmission`, plus every method that invokes its only public surface
  (`TryQueueAsync`), so that an indirect factory cannot hide a second construction.
- **Exhaustive search scope:** all `.cs` files in the repository. The type is `internal sealed`
  (`QfcRemainingQueueAdmission.cs:9`), so no other assembly can construct it and no derived type can
  exist; the only assembly with access is `QuickFiler` plus whatever `InternalsVisibleTo` grants
  (`QuickFiler.Test`, evidenced by that project's direct construction).
- **Inclusion rules:** a site counts if it evaluates `new QfcRemainingQueueAdmission(...)`.
- **Exclusion rules:** the type declaration; the constructor declaration; `docs/` and `.claude/`
  prose references.
- **Primary search strategy:** repository-wide ripgrep for the type name
  `QfcRemainingQueueAdmission`, then classify each hit.
- **Primary member set:**
  - Production construction: `QuickFiler/Controllers/QfcDatamodel.cs:353`
  - Test construction: `QuickFiler.Test/Controllers/QfcDatamodelTests.cs:39`
  - Non-construction hits: `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs:9` (class decl),
    `:15` (ctor decl); `QuickFiler/QuickFiler.csproj:346` (`<Compile Include>`);
    `QuickFiler.Test/Controllers/QfcDatamodelTests.cs:21` (factory return type), `:195` (doc comment).
- **Primary count:** production = 1, test = 1.
- **Cross-check search strategy:** a different query targeting the *usage* surface rather than the
  type name — ripgrep for the alternation `TryQueueAsync|CreateQueueAdmission|admission\.` across all
  `.cs` files. This finds every consumer of an instance, so any second construction site (including
  one built via a helper whose name does not contain the type name) would surface as an otherwise
  unexplained `admission.` receiver.
- **Cross-check member set:** consumers found —
  `QuickFiler/Controllers/QfcDatamodel.cs:360` (`admission.TryQueueAsync`, the receiver constructed at
  `:353`); `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs:34` (the method declaration itself);
  `QuickFiler.Test/Controllers/QfcDatamodelTests.cs:21`/`:39` (the single factory) with its five
  callers at `:55`, `:82`, `:147`, `:174`, `:203` and their five `admission.TryQueueAsync` calls at
  `:67`, `:94`, `:159`, `:184`, `:213`. Every `admission.` receiver traces to one of the two
  construction expressions; no third exists.
- **Cross-check count:** production = 1 (`QfcDatamodel.cs:353`), test = 1
  (`QfcDatamodelTests.cs:39`, reached from five call sites).
- **Member-set comparison:** primary construction set `{QfcDatamodel.cs:353, QfcDatamodelTests.cs:39}`
  equals the cross-check's derived construction set. Identical. Assertion accepted.

### N4 — production callers of `QfcFormController.Cleanup()` = 1

- **Complete family:** every production call expression in the repository whose target is
  `QfcFormController.Cleanup()`, whether written unqualified (`Cleanup();`), through a field
  (`_formController.Cleanup()`), through an interface (`IQfcFormController` /
  `IFilerFormController`), or through a delegate captured as `parentCleanup`.
- **Exhaustive search scope:** all `.cs` files under `QuickFiler/`, including `Legacy/` and `Notes/`,
  because `QfcFormController` is `internal partial` and spread across five files
  (`QfcFormController.cs`, `.Actions.cs`, `.Deactivate.cs`, `.EventHandlers.cs`, `.SetupDisposal.cs`),
  so an unqualified `Cleanup();` can appear in any of them.
- **Inclusion rules:** a call counts if the receiver's static type is `QfcFormController`,
  `IQfcFormController`, or `IFilerFormController`.
- **Exclusion rules:** `Cleanup()` on `IQfcItemController`, `IQfcCollectionController`,
  `IQfcDatamodel`, `IFilerHomeController`, `EfcFormController`, `EfcHomeController`,
  `QfcHomeController`, `QfcCollectionController`, and the legacy `QuickFileController`/`QfcLauncher`
  types; `CleanupBackground()`; `Cleanup_Files()`; `ExplConvView_Cleanup()`; all method-group
  *arguments* named `Cleanup` (these pass a different type's method as `parentCleanup`); all comments;
  all test files.
- **Primary search strategy:** ripgrep for `Cleanup\(\)|CleanupBackground\(\)` restricted to the five
  `QuickFiler/Controllers/QfcFormController*.cs` files — i.e. every part of the `partial` class, which
  is where an unqualified self-call must live.
- **Primary member set:**
  - `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs:210` — the **declaration**, excluded.
  - `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:93` — `Cleanup();` inside
    `ActionCancelAsync()`. **Counted.**
  - `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:92` — `_groups?.Cleanup();`
    (`IQfcCollectionController`), excluded.
  - `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:242`, `:383` — `CleanupBackground()`,
    excluded.
- **Primary count:** 1
- **Cross-check search strategy:** a different, deliberately over-broad query — ripgrep for the bare
  substring `Cleanup` across **all** of `QuickFiler/**/*.cs` (not just the `QfcFormController` parts),
  then resolve the receiver type of every `.Cleanup()`-shaped hit. This covers callers that live
  outside the partial class and would catch a `_formController.Cleanup()` anywhere in the assembly.
- **Cross-check member set:** the 120 hits resolve as — interface declarations
  (`IQfcItemController.cs:83`, `IQfcFormController.cs:15`, `IQfcDatamodel.cs:131`,
  `IQfcCollectionController.cs:114`/`:116`, `IFilerHomeController.cs:17`, `IFilerFormController.cs:15`,
  `Notes/notes_interfaces.cs:19`/`:58`); other types' `Cleanup()` declarations
  (`QfcItemController.ViewerSetup.cs:447`, `EfcItemController.cs:231`, `QfcDatamodel.cs:75`,
  `EfcFormController.cs:219`, `EfcHomeController.cs:342`, `QfcHomeController.cs:370`,
  `QfcCollectionController.cs:2126`/`:2112`, `QfcFormController.SetupDisposal.cs:210`,
  `Legacy/QuickFileController.cs:659`, `Legacy/QfcLauncher.cs:57`); other types' `Cleanup()` calls
  (`QfcCollectionController.cs:747`, `:763`, `:790`, `:1539` on `IQfcItemController`;
  `QfcHomeController.cs:372` on `IQfcDatamodel`; `EfcFormController.cs:543`, `:574`, `:797`, `:807`,
  `:882` self-calls on `EfcFormController`; `Legacy/QfcFormLegacyViewer.cs:87` and
  `Legacy/QuickFileController.cs:673` on legacy types); `parentCleanup` field/parameter/argument
  occurrences (`QfcFormController.cs:32`, `:45`, `:72`; `SetupDisposal.cs:228`, `:229`;
  `QfcHomeController.cs:29`, `:32`, `:37`, `:62`, `:100`, `:110`, `:119`, `:142`, `:154`, `:378`;
  `EfcHomeController.cs:49`–`:289`; `EfcFormController.cs:35`–`:236`;
  `Legacy/QuickFileController.cs:86`, `:87`, `:94`, `:105`, `:664`; `Legacy/QfcLauncher.cs:13`, `:14`,
  `:16`, `:19`, `:30`, `:62`); comments; `SortEmail.Cleanup_Files()`; `CleanupBackground()`. **The
  only hit whose receiver's static type is `QfcFormController` is `QfcFormController.EventHandlers.cs:93`.**
  Notably, no `_formController.Cleanup()` expression exists anywhere: `QfcHomeController.Cleanup()`
  (`:370-379`) sets `_formController = null` (`:376`) without calling it, and the `Cleanup` method
  groups passed at `QfcHomeController.cs:100` and `:142` are `QfcHomeController`'s own method,
  supplied as the form controller's `parentCleanup` argument.
- **Cross-check count:** 1
- **Member-set comparison:** primary caller set `{QfcFormController.EventHandlers.cs:93}` equals the
  cross-check caller set `{QfcFormController.EventHandlers.cs:93}`. Identical. Assertion accepted.

---

## Consolidated file-change map (design only)

| Finding | Production files touched | Test files touched | Notes |
|---|---|---|---|
| 1 | `QfcCollectionController.cs:83` (comment), `QfcDatamodel.cs:103` (comment), `QfcQueue.cs:40` (comment), optionally `Helper Classes/EmailMoveMonitor.cs:17` (stale TODO) | new `QuickFiler.Test/Controllers/QfcMoveMonitorTopologyTests.cs` (+ csproj entry) | No behaviour change. Keeps `QfcCollectionController.cs` at 2327 lines +/- comment lines only. |
| 2 | `QfcFormController.SetupDisposal.cs:210-230` | new `QuickFiler.Test/Controllers/QfcFormControllerCleanupTests.cs` (+ csproj entry) | `SetupDisposal.cs` is 234/500 — room. `QfcFormControllerSeamTests.cs` is 497/500 — must NOT grow. |
| 3 | `QfcRemainingQueueAdmission.cs:16-26`, `QfcDatamodel.cs:353-359` | `QuickFiler.Test/Controllers/QfcDatamodelTests.cs:21-46` + call sites `:55/:82/:147/:174/:203`; replace `:76-95` with a structural pin | `internal sealed` type — no public API break. |
| 4 | `QfcCollectionController.cs:991` (one line) | optional structural pin in `QfcCollectionControllerDefects468Tests.cs` (currently well under the cap) or in the new file from Finding 1 | Must NOT use `volatile` (CS0420 vs `TreatWarningsAsErrors`). |
| 5 | none | none | Evidence-only AC; residual gap stays with #683. |

Files explicitly **not** touched, per the binding scope constraints:
`QuickFiler/Controllers/QfcHomeController.Metrics.cs` and
`QuickFiler/Controllers/EfcHomeController.Metrics.cs` (sibling parallel work item). No file split of
`QfcCollectionController.cs` is proposed; the only change to it is one line (Finding 4) plus three
comment lines (Finding 1).

## Testing strategy summary (no test code written)

- Framework/library obligations per CLAUDE.md § CUT1–CUT2: MSTest attributes, Moq for boundaries,
  FluentAssertions for assertions with explicit `because:` messages.
- Determinism per `.claude/rules/general-unit-test.md`: Finding 2's tests must use the existing
  `FakeTimeProvider` clock seam (`QfcFormController.Actions.cs:211`) and the inline
  `UndoConsumerStarter = body => body()` seam (`:218`), never `Thread.Sleep`, `Task.Delay`, or a real
  wall-clock wait. Finding 4's test is structural, with the standard "structural proxy, not a proof"
  disclaimer.
- No temporary files, no live Outlook COM, no shown `Form`, and no STA-bound control is required by
  any test proposed here; the `UndoItemProcessor` seam (`Actions.cs:230-234`) keeps
  `ProcessUndoItemAsync`'s COM and dispatcher calls out of the undo tests, exactly as the existing
  #448 tests already do.
- Both new test files must be registered with `<Compile Include="…" />` in
  `QuickFiler.Test/QuickFiler.Test.csproj` (legacy non-SDK project; see existing entries at `:139`,
  `:143`, `:149`, `:150`).
- Toolchain per CLAUDE.md: `csharpier format` -> analyzer `msbuild /t:Rebuild … EnableNETAnalyzers`
  -> nullable `msbuild /t:Rebuild … TreatWarningsAsErrors=true` -> `vstest.console.exe … /EnableCodeCoverage`,
  restarting from step 1 on any failure or auto-fix.
