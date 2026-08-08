# Research — `QuickFiler/Controllers/QfcFormController.EventHandlers.cs`

Timestamp: 2026-08-07T22-05

| Field | Value |
| --- | --- |
| Production file | `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a8220048ded06d508\QuickFiler\Controllers\QfcFormController.EventHandlers.cs` |
| Exact line count | 399 |
| `[ExcludeFromCodeCoverage]` | **No.** The file declares `internal partial class QfcFormController` at line 18 with no attribute; no member in the file carries the attribute either. Verified by reading the entire file. |
| Compiled | Yes — `QuickFiler/QuickFiler.csproj` line 319: `<Compile Include="Controllers\QfcFormController.EventHandlers.cs" />` |
| Feature child | F6 (issue #435), epic #136 |

Current numeric per-file line coverage is **unmeasured**. It cannot be determined without running the
toolchain. The command that would produce it is
`scripts\vscode\Invoke-MSTestWithCoverage.ps1` (Cobertura output), consumed by F1's per-file
report harness. Do not substitute an invented figure.

---

## 1. Type-level context (read for grounding, not owned by this artifact)

`QfcFormController` is a four-file partial: `QfcFormController.cs` (196), `.SetupDisposal.cs` (232),
`.EventHandlers.cs` (399, this artifact), `.Actions.cs` (302).

Constructor (`QfcFormController.cs` L27-51) takes eight parameters:
`IApplicationGlobals appGlobals`, `IQfcFormViewer formViewer`, `IQfcQueue qfcQueue`,
`QfEnums.InitTypeEnum initType`, `System.Action parentCleanup`, `IQfcHomeController parent`,
`CancellationTokenSource tokenSource`, `CancellationToken token`. Every one of these except
`initType`/`parentCleanup`/the token pair is an interface and therefore already Moq-able. The ctor
also performs three side effects that a test must tolerate: `_globals.AF.MaximizeQuickFileWindow = MaximizeFormViewer`,
`_formViewer.SetController(this)`, and `_movedItems = _globals.AF.MovedMails`
(`SloStack<IMovedMailInfo>`, which a default Moq `IAppAutoFileObjects` returns as `null`).

Private fields consumed by this file and declared in `QfcFormController.cs` L71-92:
`_globals`, `_parent`, `_formViewer`, `_qfcQueue`, `_groups`, `_states`, `_rowStyleTemplate`,
`_rowStyleExpanded`, `_itemsPerIteration` (declared in `.SetupDisposal.cs` L120), `_darkMode`,
`_themes`, `_movedItems`, `_undoQueue` (`BlockingCollection<IMovedMailInfo>`), `_undoConsumerTask`,
`_helperTasks`, `WriteMetrics` (private delegate `WriteMetricsDelegate`), `Iterate`.

`RegisterFormEventHandlers` / `UnregisterFormEventHandlers` (`.SetupDisposal.cs` L149-203) wire and
unwire exactly five handlers declared in **this** file:

| Viewer event | Handler in this file |
| --- | --- |
| `IQfcFormViewer.OkClicked` | `ButtonOK_Click(object, EventArgs)` — `async void` |
| `IQfcFormViewer.CancelClicked` | `ButtonCancel_Click(object, EventArgs)` — `async void` |
| `IQfcFormViewer.UndoClicked` | `ButtonUndo_Click(object, EventArgs)` |
| `IQfcFormViewer.ItemsPerLoadValueChanged` | `SpnEmailPerLoad_ValueChanged(object, EventArgs)` — `async void` |
| `IQfcFormViewer.SkipClicked` | `ButtonSkip_Click(object, EventArgs)` — `async void` |

`SetupLightDark` (`.SetupDisposal.cs` L84) additionally subscribes `_globals.Ol.PropertyChanged += DarkMode_CheckedChanged`,
and `Cleanup` (L212) unsubscribes it. So `DarkMode_CheckedChanged` is reachable both directly and
through the `Ol` property-change event.

---

## 2. Current test coverage inventory

Search scope: the whole `QuickFiler.Test` tree. A grep for `QfcFormController` returns 11 files; nine
of them (`QfcHomeController*Tests.cs`, `QfcViewer_Test.cs`, `QuickFiler.Test.csproj`) reference only
`Mock<IQfcFormController>` or a commented-out line and therefore execute **zero** production lines of
this file. The two files that instantiate the concrete controller are listed below.

### 2.1 `QuickFiler.Test/Controllers/QfcFormControllerTests.cs`

| Test method (line) | Production member reached (this file) | Depth reached |
| --- | --- | --- |
| `DarkMode_CheckedChanged_ShouldUpdateTheme` (L377) | `DarkMode_CheckedChanged` | Full method **except** the `_formViewer?.UiSyncContext is not null` branch (mock returns `null`), and only the `DarkMode == true` arm |
| `ButtonCancel_Click_ShouldCancelAction` (L392) | `ActionCancelAsync` | Full method with all optionals null: `_parent.TokenSource` is `null` so `Cancel()` is skipped; `UiSyncContext` null so the `await` is skipped; `_groups` null. **Does not call `ButtonCancel_Click`** despite its name |
| `ButtonOK_Click_ShouldPerformAction` (L405) | `ActionOkAsync` | `_initType == Sort` so the throw arm is skipped; `_groups` is null so `_groups?.ReadyForMove == true` is false. Body of the `else if` is **not** entered |
| `LoadUiFromQueue_ShouldLoadUi` (L418) | `LoadUiFromQueue` | Full method; `TryDequeueAsync` unstubbed returns default `(null, null)` |
| `MoveAndIterate_ShouldMoveAndIterate` (L431) | `MoveAndIterate` | **Guard only** (L149-152 `_groups is null` → return) |
| `BackGroundMoveAsync_ShouldMoveEmails` (L444) | `BackGroundMoveAsync` | **Guard only** (L219-222 `_groups is null` → return) |
| `ButtonUndo_Click_ShouldUndoAction` (L457) | `ButtonUndo_Click(object, EventArgs)` | Full 2-line method; delegates into `UndoDialog` (Actions.cs), which returns at its own guard |
| `SpnEmailPerLoad_ValueChanged_ShouldChangeValue_EqualsItemPerIteration` (L470) | `SpnEmailPerLoadHandler` | `WorkerComplete=true` skips the delay loop; `case n == _itemsPerIteration` arm |
| `SpnEmailPerLoad_ValueChanged_ShouldChangeValue_GreaterItemPerIteration` (L487) | `SpnEmailPerLoadHandler` | `case n > _itemsPerIteration` arm, including `UnregisterNavigation` / `ChangeIterationSize` / `RegisterNavigation` |
| `AdjustTlp_ShouldAdjustTlp` (L535) | `AdjustTlp` | **Guard only** (`_rowStyleTemplate` is null → return at L299-302) |
| `ButtonSkip_Click_ShouldSkipGroup` (L549) | `ButtonSkipHandler` → `SkipGroupAsync` | `ButtonSkipHandler` full; `SkipGroupAsync` `Count+JobsRunning > 0` arm |
| `SkipGroupAsync_ShouldSkipGroup` (L570) | `SkipGroupAsync` | Same arm as above, invoked directly |

### 2.2 `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs`

| Test method (line) | Production member reached (this file) | Depth reached |
| --- | --- | --- |
| `OkClicked_WhenRaised_RoutesToControllerWithoutThrowing` (L169) | `ButtonOK_Click` (async void) → `ActionOkAsync` | Try block only; `catch` not entered |
| `CancelClicked_WhenRaised_CancelsParentTokenSource` (L184) | `ButtonCancel_Click` → `ActionCancelAsync` | Adds the `_parent.TokenSource.Cancel()` line that `QfcFormControllerTests` misses; `catch` not entered |
| `UndoClicked_WhenRaised_RoutesToControllerWithoutThrowing` (L203) | `ButtonUndo_Click(object, EventArgs)` | Same as §2.1 |
| `ItemsPerLoadValueChanged_WhenRaised_RoutesToSpinnerHandler` (L219) | `SpnEmailPerLoad_ValueChanged` → `SpnEmailPerLoadHandler` | Only path that enters the `async void` spinner shim; `catch` not entered |
| `SkipClicked_WhenRaised_TogglesSkipButtonTextAndEnabled` (L245) | `ButtonSkip_Click` → `ButtonSkipHandler` → `SkipGroupAsync` | Reaches `SkipGroupAsync`'s **final `else`** (`logger.Info` at L391) because `Count=0`, `JobsRunning=0`, and `Worker` is `null` |
| `ButtonSkipHandler_WhenInvoked_TogglesSkipButtonTextAndEnabled` (L267) | `ButtonSkipHandler` → `SkipGroupAsync` | Same `else` arm, invoked directly |

### 2.3 Anti-duplication conclusion

Already covered — **do not re-test**: `DarkMode_CheckedChanged` dark arm; `ActionCancelAsync` all-null
path and the `TokenSource.Cancel()` line; `ActionOkAsync` `Sort`/not-ready path; `LoadUiFromQueue`;
`ButtonUndo_Click(object, EventArgs)`; `SpnEmailPerLoadHandler` equal-count and greater-count arms;
`ButtonSkipHandler` both viewer-null-free arms; `SkipGroupAsync` dequeue arm and exhausted-queue
`else` arm; the four `async void` happy paths.

Genuinely uncovered — the plan's target set: every `catch` block in this file (4); the
`_formViewer?.UiSyncContext is not null` branch in `DarkMode_CheckedChanged`; the light arm of
`DarkMode_CheckedChanged`; the `NotImplementedException` arm of `ActionOkAsync`; the
`_groups.ReadyForMove == true` body of `ActionOkAsync`; all three non-guard arms of `MoveAndIterate`
plus its inner `catch`; the body of `BackGroundMoveAsync`; `ButtonUndo_Click()` (the
parameterless overload — **no test reaches it at all**); the `WorkerComplete == false` delay loop and
the `n > 0` and `default` switch arms of `SpnEmailPerLoadHandler`; the entire body of `AdjustTlp`
(both the insert and the remove branch); the `_formViewer is null` arm of `ButtonSkipHandler`; the
`_qfcQueue is null` guard and the `Worker.IsBusy` popup arm of `SkipGroupAsync`.

---

## 3. Member-by-member reachability table

`C` = covered, `P` = partially covered, `U` = unreachable from any current deterministic test.

| # | Member (lines) | Kind | State | Concrete blocker for the uncovered portion |
| --- | --- | --- | --- | --- |
| 1 | `DarkMode_CheckedChanged(object, EventArgs)` (22-38) | internal void | P | `SynchronizationContext.SetSynchronizationContext` mutates ambient thread state; the `UiSyncContext is not null` arm is never entered because the mock returns null. No production blocker — a fake `SynchronizationContext` reaches it. Light arm needs `_globals.Ol.DarkMode == false` **and** `_darkMode == false` |
| 2 | `ButtonCancel_Click(object, EventArgs)` (70-82) | public **async void** | P | `catch` unreachable: an `async void` that rethrows cannot be awaited, so the test cannot observe the exception deterministically — it surfaces on the captured `SynchronizationContext`/thread pool |
| 3 | `ActionCancelAsync()` (84-94) | public async Task | P | Only the `await _formViewer.UiSyncContext` line is uncovered; `UiThread.GetAwaiter(SynchronizationContext)` (UtilitiesCS/Threading/UiThread.cs L108) completes synchronously when the context equals `SynchronizationContext.Current`, so a fake context reaches it. No seam needed |
| 4 | `ButtonOK_Click(object, EventArgs)` (96-108) | public **async void** | P | Same async-void `catch` blocker as #2 |
| 5 | `ActionOkAsync()` (110-134) | public async Task | P | Throw arm needs `_initType` without the `Sort` flag (private field; existing tests already use reflection `SetPrivateField`). Ready-for-move arm needs a `Mock<IQfcCollectionController>` with `ReadyForMove == true` **and** `_parent.KeyboardHandler` (`IQfcKeyboardHandler`, mockable) **and** then falls into `MoveAndIterate`, which is itself blocked (#7) |
| 6 | `LoadUiFromQueue()` (136-143) | internal async Task | C | — |
| 7 | `MoveAndIterate()` (145-213) | internal async Task | P (guard only) | Three separate hard blockers: (a) L180 and L204 `System.Windows.Forms.MessageBox.Show(...)` block on a modal popup — a unit-test-policy violation; (b) L178 `_formViewer.Worker?.IsBusy` reads a concrete `System.ComponentModel.BackgroundWorker` whose `IsBusy` is **non-virtual**, so Moq cannot force `true`; (c) L199 `UiThread.Dispatcher.InvokeAsync(...)` reads a static WPF `Dispatcher` that is `null` unless `UiThread.Init()` has run (which shows a hidden WinForms form) |
| 8 | `BackGroundMoveAsync()` (215-234) | internal async Task | P (guard only) | L228 and L233 `UiThread.Dispatcher.InvokeAsync(...)` — same static-dispatcher blocker as #7(c). `_globals.FS.Filenames.EmailSession` and `WriteMetrics` are interface/delegate reachable |
| 9 | `ButtonUndo_Click(object, EventArgs)` (236-239) | public void | C | — |
| 10 | `ButtonUndo_Click()` (241-245) | public void | **U** | Not invoked by any test. No production blocker — it is a 1-statement forward to `UndoDialog()`; it is simply untested |
| 11 | `SpnEmailPerLoadHandler(object, EventArgs)` (247-283) | public async Task | P | L252-255 `while (!_parent.WorkerComplete) await Task.Delay(100);` — `Task.Delay` is on the repo banned-API list and a real wall-clock wait is prohibited in tests. `case n > 0` and `default` arms have no blocker beyond arranging `ItemsPerLoadValue` |
| 12 | `SpnEmailPerLoad_ValueChanged(object, EventArgs)` (285-295) | public **async void** | P | `catch` unreachable for the async-void reason; note this one **swallows** (`log.Error`, no rethrow), unlike #2/#4/#15 |
| 13 | `AdjustTlp(TableLayoutPanel, int)` (297-328) | internal void | P (guard only) | No hard blocker. Requires `_rowStyleTemplate` (private `RowStyle`) set by reflection and a real in-memory `TableLayoutPanel`. `TableLayoutHelper.InsertSpecificRow`/`RemoveSpecificRow` take the in-memory path when `panel.InvokeRequired == false`, which holds for a never-shown panel (UtilitiesCS/HelperClasses/Windows Forms/TableLayoutHelper.cs L21, L62). Existing tests already construct `new TableLayoutPanel()` in this assembly |
| 14 | `ButtonSkipHandler(object, EventArgs)` (330-343) | public async Task | P | Only the `_formViewer is null` early arm (L332-336) is uncovered; needs `_formViewer` nulled by reflection or by calling `Cleanup()` first |
| 15 | `ButtonSkip_Click(object, EventArgs)` (345-359) | public **async void** | P | Async-void `catch` blocker (#2) |
| 16 | `SkipGroupAsync()` (361-395) | public async Task | P | L382 `MessageBox.Show(...)` popup blocker and the non-virtual `BackgroundWorker.IsBusy` blocker (same as #7(a)/(b)). The `_qfcQueue is null` guard (L363-366) needs `_qfcQueue` nulled by reflection |

### 3.1 Verified findings recorded, not to be changed

- **Four `async void` handlers.** `ButtonCancel_Click`, `ButtonOK_Click`, `SpnEmailPerLoad_ValueChanged`,
  `ButtonSkip_Click`. Three rethrow after logging; `SpnEmailPerLoad_ValueChanged` swallows. All four
  are required by `RegisterFormEventHandlers`' `EventHandler` subscription, so the signature must not
  change.
- **`BackGroundMoveAsync` does not await the metrics write.** At L228 the argument
  `async () => await WriteMetrics(...)` binds to `Dispatcher.InvokeAsync<TResult>(Func<TResult>, DispatcherPriority)`
  with `TResult = Task` (an async lambda's inferred return type is `Task`, and a non-void delegate
  target is preferred over `Action`). `await` on the resulting `DispatcherOperation<Task>` yields the
  inner `Task` and discards it. The same shape applies at L199 for `_parent.IterateQueueAsync`. This
  is fire-and-forget behavior. **Record only — do not "fix" it**; the acceptance criteria forbid
  behavior change, and any seam must preserve this discard.
- **`_globals.Ol.PropertyChanged`** is an `EventHandler`-shaped subscription of `DarkMode_CheckedChanged`
  made in `SetupLightDark` and removed in `Cleanup`. Not owned by this file, but it is the second call
  path into member #1.
- **`_undoQueue` / `_undoConsumerTask` / `_helperTasks` are never touched by this file.** All three
  are read/written exclusively in `QfcFormController.Actions.cs` (`UndoDialog`, `UndoConsumer`,
  `LoadItems`). `SloStack<IMovedMailInfo> _movedItems` is read here only indirectly, via
  `BackGroundMoveAsync`'s `_groups.MoveEmailsAsync(_movedItems)` (L225), which passes it straight
  through to a mockable `IQfcCollectionController`.
- **`Microsoft.Office.Interop.Outlook` parameter surface:** this file declares **no** member that
  takes or returns a `MailItem`, `MAPIFolder`, `Store`, or `Application`. The Outlook `using`
  directive at L9 is inherited boilerplate shared across the four partials. This materially lowers
  the seam cost relative to `Actions.cs`.
- **`ProgressTracker`** does not appear in this file.
- **No `Thread.Sleep`, no `DateTime.Now`/`UtcNow`, no `Stopwatch`** in this file. The only banned-API
  use is the single `await Task.Delay(100)` at L254.

---

## 4. Seam design proposal

Hierarchy applied per `.claude/rules/csharp.md`: interface seam > injectable delegate > adapter.

### S1 — UI dispatch (`UiThread.Dispatcher`) → existing `IUiDispatcher` interface seam

**Sites:** L199 (`InvokeAsync(_parent.IterateQueueAsync)`), L233 (`InvokeAsync(() => _groups.CleanupBackground())`).
**Tier:** interface seam (tier 1). No new type is introduced.

`UtilitiesCS.Threading.IUiDispatcher` already exists (`UtilitiesCS/Threading/IUiDispatcher.cs`) with a
production adapter `WpfUiDispatcher` (`UtilitiesCS/Threading/WpfUiDispatcher.cs`) that forwards 1:1 to
`UiThread.Dispatcher`. The in-repo precedent is `QfcItemController`, which takes an optional
`UtilitiesCS.Threading.IUiDispatcher uiDispatcher = null` constructor parameter
(`QfcItemController.Initialization.cs` L38, L57) and defaults it with
`_uiDispatcher ??= new UtilitiesCS.Threading.WpfUiDispatcher();` (L380). Adopt exactly that shape:

- Add an optional trailing ctor parameter `UtilitiesCS.Threading.IUiDispatcher uiDispatcher = null`
  and a private field `_uiDispatcher`, defaulting to `new WpfUiDispatcher()`. **This edit lands in
  `QfcFormController.cs`, not in this file** — see §4.7 (INTRA-CHILD COORDINATION).
- L233 `UiThread.Dispatcher.InvokeAsync(() => _groups.CleanupBackground())` →
  `_uiDispatcher.InvokeAsync(() => _groups.CleanupBackground())`. `WpfUiDispatcher.InvokeAsync(Action)`
  is `Dispatcher.InvokeAsync(action).Task` — semantically identical.
- L199 `UiThread.Dispatcher.InvokeAsync(_parent.IterateQueueAsync)` →
  `_uiDispatcher.InvokeAsync(_parent.IterateQueueAsync)`, which binds
  `InvokeAsync<TResult>(Func<TResult>)` with `TResult = Task` and returns `Task<Task>`. Awaiting it
  discards the inner task exactly as today. Semantics preserved.

**Why this beats a delegate here:** the interface already exists, is already mocked elsewhere in
`QuickFiler.Test`, and adds zero new production lines to the coverage denominator.

### S2 — UI dispatch with an explicit `DispatcherPriority` and a `Func<Task>` payload → injectable delegate

**Site:** L228-231 (`InvokeAsync(async () => await WriteMetrics(...), DispatcherPriority.ContextIdle)`).
**Tier:** injectable delegate (tier 2), with a documented reason for skipping tier 1.

`IUiDispatcher` has **no** `InvokeAsync<TResult>(Func<TResult>, DispatcherPriority)` member. The only
priority-bearing member is `InvokeAsync(Action, DispatcherPriority, CancellationToken)`, and routing
the async lambda through it would rebind it from `Func<Task>` to `Action` — i.e. convert a discarded
`Task` into an `async void`, which changes failure behavior. That is a behavior change and is
rejected. Growing `IUiDispatcher` would mean editing `UtilitiesCS`, which is outside the epic's
QuickFiler denominator; also rejected.

Proposed seam (field declared in `QfcFormController.cs`, used here):

```csharp
// Default preserves the current binding exactly: Dispatcher.InvokeAsync<Task>(Func<Task>, priority)
// returns DispatcherOperation<Task>, whose .Task is a Task<Task>; awaiting it discards the inner task.
internal Func<Func<Task>, System.Windows.Threading.DispatcherPriority, Task> UiInvokeWithPriorityAsync
{ get; set; } = (work, priority) => UiThread.Dispatcher.InvokeAsync(work, priority).Task;
```

Call site becomes `await UiInvokeWithPriorityAsync(async () => await WriteMetrics(...), DispatcherPriority.ContextIdle);`.

### S3 — `MessageBox.Show` → injectable dialog delegate

**Sites (this file):** L180-186 ("Still loading emails…", `MoveAndIterate`), L204-209
("Finished Moving Emails", `MoveAndIterate`), L382-387 ("Still loading emails…", `SkipGroupAsync`).
**Tier:** injectable delegate (tier 2). A static WinForms API has no interface to extract; a
one-member adapter interface would add a production file whose body is untestable, so the delegate is
the smaller seam.

All three sites are *notification-only* (`MessageBoxButtons.OK`, return value discarded). A single
shared delegate suffices for this file:

```csharp
internal Action<string, string, MessageBoxButtons, MessageBoxIcon> ShowMessage { get; set; } =
    (text, caption, buttons, icon) => MessageBox.Show(text, caption, buttons, icon);
```

`Actions.cs` additionally needs a **result-returning** prompt for `UndoDialog`; see that artifact for
`PromptYesNo`. The two seams are distinct and must not be merged, because merging would force the
notification sites to consume a `DialogResult` they currently ignore.

This is a hard requirement, not a preference: a unit test must never produce a popup requiring human
interaction (`CLAUDE.md` § UT4, epic Shared Design §2).

### S4 — `BackgroundWorker.IsBusy` → injectable predicate delegate

**Sites:** L178 (`MoveAndIterate`), L380 (`SkipGroupAsync`).
**Tier:** injectable delegate (tier 2).

`IQfcFormViewer.Worker` (`QuickFiler/Interfaces/IQfcFormViewer.cs` L18) returns the concrete
`System.ComponentModel.BackgroundWorker`. `BackgroundWorker.IsBusy` is a non-virtual public property,
so Moq cannot force it to `true`, and driving it to `true` for real requires `RunWorkerAsync` plus a
wall-clock race — nondeterministic and prohibited.

The interface-seam alternative would be adding `bool IsWorkerBusy { get; }` to `IQfcFormViewer`. That
is rejected: this project targets .NET Framework 4.8, so **default interface members are not
available**, and adding the member would require editing
`QuickFiler/Viewers/QfcFormViewer.cs`, which belongs to sibling child F15 and must not be edited.

```csharp
internal Func<bool> IsWorkerBusy { get; set; } = () => false; // assigned in ctor to the real probe
```

Because the default value must read `_formViewer` (a field, not available in a field initializer in a
way that survives `Cleanup()` nulling it), initialize it as
`IsWorkerBusy = () => _formViewer?.Worker?.IsBusy == true;` inside the constructor. Call sites become
`else if (IsWorkerBusy())`. Note that L178 currently reads `_formViewer.Worker?.IsBusy == true`
(no null-conditional on `_formViewer`) while L380 reads `_formViewer?.Worker?.IsBusy == true`. Adopting
the null-safe form at both sites makes L178 strictly more tolerant. **This is a latent behavior
difference the plan author must decide on explicitly** — see §8, OQ-3.

### S5 — `await Task.Delay(100)` polling loop → injectable delay delegate

**Site:** L252-255 in `SpnEmailPerLoadHandler`.
**Tier:** injectable delegate (tier 2).

`Task.Delay` is on the repo banned-API list for test code (`.claude/rules/general-unit-test.md`
§ Determinism Infrastructure) and is a banned symbol at `suggestion` severity in production
(`.claude/rules/csharp.md` § Banned symbols). A `FakeTimeProvider` does not help here because the loop
does not read a clock; it awaits a fixed delay.

```csharp
internal Func<int, Task> DelayAsync { get; set; } = milliseconds => Task.Delay(milliseconds);
```

Test substitutes `_ => Task.CompletedTask` and flips a `Mock<IQfcHomeController>.WorkerComplete`
sequence (`SetupSequence(...).Returns(false).Returns(false).Returns(true)`) so the loop iterates a
bounded, deterministic number of times with no wall-clock wait.

### S6 — `async void` catch blocks → extract an awaitable core

**Sites:** members #2, #4, #12, #15.
**Tier:** structural extraction, no new dependency.

Each `async void` handler already delegates to an awaitable core (`ActionCancelAsync`, `ActionOkAsync`,
`SpnEmailPerLoadHandler`, `ButtonSkipHandler`) but keeps the `try`/`catch` **inside** the `async void`,
which is precisely the part that cannot be awaited or asserted. Move the try/catch into a new
`internal async Task <Name>CoreAsync(object sender, EventArgs e)` and reduce the `async void` to a
one-line `await` forward. Example for cancel:

```csharp
public async void ButtonCancel_Click(object sender, EventArgs e) =>
    await ButtonCancelCoreAsync(sender, e);

internal async Task ButtonCancelCoreAsync(object sender, EventArgs e)
{
    try
    {
        SynchronizationContext.SetSynchronizationContext(_formViewer.UiSyncContext);
        await ActionCancelAsync();
    }
    catch (System.Exception ex) { logger.Error(ex.Message, ex); throw; }
}
```

The public signature, the subscription in `RegisterFormEventHandlers`, and the rethrow/swallow
semantics are all preserved. Tests then `await ButtonCancelCoreAsync(...)` and assert with
FluentAssertions `Should().ThrowAsync<...>()` (or `NotThrowAsync` for the spinner, which swallows).

**Note on the `ButtonCancel_Click` / `ButtonOK_Click` / `ButtonSkip_Click` throw path:** to force the
`catch`, the test arranges the inner awaitable to throw — e.g. `_parent.TokenSource` returning a
`CancellationTokenSource` already disposed (`Cancel()` throws `ObjectDisposedException`), or a
`Mock<IQfcCollectionController>.Cleanup()` set up to `Throws<InvalidOperationException>()`. Both are
deterministic and use only existing interface seams.

### S7 — What deliberately gets **no** seam

- `SynchronizationContext.SetSynchronizationContext` (L26, L74, L100, L250, L348). Reachable directly;
  tests must save and restore `SynchronizationContext.Current` in `[TestCleanup]` to preserve test
  independence. No seam needed.
- `logger` / `log` (log4net statics). No assertion depends on them; no seam.
- `_qfcQueue`, `_parent`, `_groups`, `_formViewer`, `_globals` — already interfaces.
- `TableLayoutHelper.InsertSpecificRow` / `RemoveSpecificRow` — verified in-memory when
  `InvokeRequired == false`. No seam.
- `_parent.FilerQueue.Consumer` (L167, L193). `FilerQueue` is a concrete class
  (`QuickFiler/Controllers/FilerQueue.cs`, owned by sibling F2) but has a public parameterless
  constructor and `public Task Consumer { get; private set; } = Task.CompletedTask;` (L42), so
  `_mockParent.SetupGet(p => p.FilerQueue).Returns(new FilerQueue())` gives an already-completed
  awaitable with no seam and no edit to F2's file.

### 4.7 File-size and ownership constraints

- **`QfcFormController.EventHandlers.cs` stays under 500 lines.** Every seam *field/property* is
  declared in `QfcFormController.cs` (the fields region), not here. Inside this file the S1/S2/S3/S4/S5
  substitutions are token-level and net-neutral to slightly negative (the three
  `MessageBox.Show(...)` calls shrink from 6 lines to 1-2 each, roughly −12 lines). The S6 extraction
  adds four new method shells at roughly +8 lines each, ≈ +32. Net projection: **399 − 12 + 32 ≈ 419
  lines**, comfortably under 500 with ~80 lines of headroom. **No further partial split is required.**
  If a later remediation cycle pushes past 500, the split point is the four button handlers into a new
  partial at `QuickFiler/Controllers/QfcFormController.EventHandlers.Buttons.cs`.
- `QuickFiler/Viewers/QfcFormViewer.cs` (F15) — **not edited**. No `IQfcFormViewer` member is added
  (S4 explicitly avoids it). **No `CROSS-CHILD CONTRACT NOTE (F15)` is required for this file.**
- `QuickFiler/Controllers/KeyboardHandler.cs` (F3) — **not edited**. `ActionOkAsync` L125-128 consumes
  `_parent.KeyboardHandler` only through the `IQfcKeyboardHandler` interface
  (`IFilerHomeController.KeyboardHandler`), which is already mockable.
- `QuickFiler/Controllers/QfcCollectionController.cs` (F11) — **not edited**. Every use in this file
  goes through `IQfcCollectionController`.
- `QuickFiler/Controllers/FilerQueue.cs` (F2) — **not edited**.
- `coverage.config` and shared build property files (F1) — **not modified**.
- .NET Framework 4.8: no default interface members, no `init` accessors, no `record`/`record struct`.
  Every seam above is a plain property or field with a getter/setter and a `Func<>`/`Action<>` type.
- **INTRA-CHILD COORDINATION (not cross-child):** the constructor parameter for S1 and the seam field
  declarations for S2-S5 land in `QfcFormController.cs`, which a sibling researcher on the same child
  F6 is documenting. Both files are inside F6's assignment, so this is a within-child sequencing note
  for the plan author, not a cross-child contract.

---

## 5. Proposed test cases

Target file: a **new** `QuickFiler.Test/Controllers/QfcFormControllerEventHandlerTests.cs`. Rationale:
`QfcFormControllerTests.cs` is 827 lines and `QfcFormControllerSeamTests.cs` is 378; the 500-line
ceiling applies to test code as well, and the existing seam file already documents the convention of
splitting rather than growing (`QfcFormControllerSeamTests.cs` L21-23). If the case list below
exceeds 500 lines, split at case 24 into `...EventHandlerSkipTests.cs`.

Each numbered entry is exactly one MSTest method and becomes one atomic plan task.

**`DarkMode_CheckedChanged`**

1. `DarkMode_CheckedChanged_WhenUiSyncContextPresent_SetsSynchronizationContext` — viewer returns a
   fake `SynchronizationContext`; expect `SynchronizationContext.Current` equals it after the call.
2. `DarkMode_CheckedChanged_WhenDarkModeFalse_SetsLightNormalTheme` — `_globals.Ol.DarkMode == false`;
   expect `ActiveTheme == "LightNormal"`.
3. `DarkMode_CheckedChanged_WhenGlobalsOlIsNull_RetainsExistingDarkModeField` — `_globals.Ol` null;
   expect no throw and `ActiveTheme` derived from the pre-existing `_darkMode`.

**`ButtonCancel_Click` / `ActionCancelAsync`**

4. `ButtonCancelCoreAsync_WhenActionCancelThrows_LogsAndRethrows` — `_groups.Cleanup()` set to throw;
   expect `Should().ThrowAsync<InvalidOperationException>()`.
5. `ActionCancelAsync_WhenUiSyncContextPresent_AwaitsContextAndHidesViewer` — ambient context set to
   the same fake instance so the awaiter completes synchronously; expect `_formViewer.Hide()` once.
6. `ActionCancelAsync_WhenGroupsPresent_CallsGroupsCleanupBeforeControllerCleanup` — expect
   `IQfcCollectionController.Cleanup()` invoked once and `_parentCleanup` invoked once.

**`ButtonOK_Click` / `ActionOkAsync`**

7. `ActionOkAsync_WhenInitTypeLacksSortFlag_ThrowsNotImplementedException` — `_initType` set by
   reflection to a non-`Sort` value; expect `NotImplementedException` whose message names
   `ActionOkAsync`.
8. `ActionOkAsync_WhenGroupsReadyAndKeyboardActive_TogglesKeyboardDialogOnce` — `ReadyForMove` true,
   `KeyboardHandler.KbdActive` true; expect `ToggleKeyboardDialog()` once.
9. `ActionOkAsync_WhenGroupsReadyAndKeyboardInactive_DoesNotToggleKeyboardDialog` — `KbdActive` false;
   expect `ToggleKeyboardDialog()` never.
10. `ButtonOkCoreAsync_WhenActionOkThrows_LogsAndRethrows` — non-`Sort` `_initType` makes the core
    throw; expect `Should().ThrowAsync<NotImplementedException>()`.

**`MoveAndIterate`** (each guard condition gets its own case — the guard at L149 has four operands)

11. `MoveAndIterate_WhenQueueIsNull_ReturnsWithoutCachingMoveObjects` — `_qfcQueue` nulled by
    reflection; expect `CacheMoveObjects()` never.
12. `MoveAndIterate_WhenGroupsIsNull_ReturnsWithoutCachingMoveObjects` — `_groups` null (default state).
13. `MoveAndIterate_WhenParentIsNull_ReturnsWithoutCachingMoveObjects` — `_parent` nulled by reflection.
14. `MoveAndIterate_WhenFormViewerIsNull_ReturnsWithoutCachingMoveObjects` — `_formViewer` nulled by
    reflection.
15. `MoveAndIterate_WhenQueueHasWork_CachesMovesLoadsUiAndIterates` — `Count + JobsRunning > 0`; expect
    `CacheMoveObjects()` once, `TryDequeueAsync` once, `IterateQueueAsync()` once.
16. `MoveAndIterate_WhenLoadUiThrows_AwaitsFilerQueueAndCancels` — `TryDequeueAsync` throws; expect
    `_formViewer.Hide()` (proving `ActionCancelAsync` ran in the `catch`) and no rethrow.
17. `MoveAndIterate_WhenQueueEmptyAndWorkerBusy_ShowsStillLoadingMessageAndDoesNotMove` —
    `IsWorkerBusy` seam returns true; expect the injected `ShowMessage` received text
    `"Still loading emails. Please try again in a few seconds."` with `MessageBoxIcon.Error`, and
    `MoveEmailsAsync` never.
18. `MoveAndIterate_WhenQueueEmptyAndDataModelIncomplete_RetriesIterateOnUiDispatcher` —
    `DataModel.Complete == false`; expect the injected `IUiDispatcher.InvokeAsync` invoked once and no
    message shown.
19. `MoveAndIterate_WhenQueueEmptyAndDataModelComplete_ShowsFinishedMessageAndCancels` — expect
    `ShowMessage` received `"Finished Moving Emails"` with `MessageBoxIcon.Information` and
    `_formViewer.Hide()` once.

**`BackGroundMoveAsync`** (three-operand guard, one case per operand)

20. `BackGroundMoveAsync_WhenGroupsIsNull_DoesNotMoveEmails` — expect `MoveEmailsAsync` never.
21. `BackGroundMoveAsync_WhenFilenamesIsNull_DoesNotMoveEmails` — `_globals.FS.Filenames` null.
22. `BackGroundMoveAsync_WhenWriteMetricsIsNull_DoesNotMoveEmails` — `WriteMetrics` nulled by
    reflection.
23. `BackGroundMoveAsync_WhenDependenciesPresent_MovesEmailsWritesMetricsAndCleansBackground` — expect
    `MoveEmailsAsync(_movedItems)` once, the priority-dispatch seam invoked once with
    `DispatcherPriority.ContextIdle`, and `CleanupBackground()` once.

**`ButtonUndo_Click`**

24. `ButtonUndoClickParameterless_WhenInvoked_DelegatesToUndoDialog` — the currently untested overload;
    expect no throw and the same observable effect as the two-argument overload (guard short-circuit).

**`SpnEmailPerLoadHandler` / `SpnEmailPerLoad_ValueChanged`**

25. `SpnEmailPerLoadHandler_WhenWorkerNotComplete_PollsUntilCompleteWithoutWallClockWait` —
    `WorkerComplete` sequence false→false→true, `DelayAsync` seam substituted with
    `Task.CompletedTask`; expect the seam invoked exactly twice.
26. `SpnEmailPerLoadHandler_WhenCountPositiveButBelowIteration_LeavesIterationSizeUnchanged` —
    `ItemsPerLoadValue = 3`, `_itemsPerIteration = 8`; expect `ChangeIterationSize` never and
    `_itemsPerIteration` still 8.
27. `SpnEmailPerLoadHandler_WhenCountIsZero_RestoresSpinnerToCurrentIteration` — `ItemsPerLoadValue = 0`;
    expect `ItemsPerLoadValue` reset to 8.
28. `SpnEmailPerLoadHandler_WhenCountIsNegative_RestoresSpinnerToCurrentIteration` — boundary
    companion to 27 with `ItemsPerLoadValue = -1`.
29. `SpnEmailPerLoadHandler_WhenSynchronizationContextAlreadySet_DoesNotReplaceIt` — ambient context
    pre-set; expect `SynchronizationContext.Current` unchanged.
30. `SpnEmailPerLoadValueChangedCore_WhenHandlerThrows_LogsAndSwallows` — arrange the handler to throw
    (e.g. `_formViewer.ItemsPerLoadValue` getter throws); expect `Should().NotThrowAsync()`, which
    pins the swallow-not-rethrow contract that distinguishes this handler from the other three.

**`AdjustTlp`**

31. `AdjustTlp_WhenTlpIsNull_ReturnsWithoutThrowing` — first guard operand.
32. `AdjustTlp_WhenRowStyleTemplateIsNull_ReturnsWithoutThrowing` — second guard operand (this is what
    the existing `AdjustTlp_ShouldAdjustTlp` accidentally covers; the new case makes the intent
    explicit and asserts `RowCount` unchanged).
33. `AdjustTlp_WhenNewCountExceedsCurrent_InsertsRowsAndGrowsMinimumHeight` — template set by
    reflection with a known `RowStyle` height; expect `RowStyles.Count` increased by the difference and
    `MinimumSize.Height` increased by `round(height * diff)`.
34. `AdjustTlp_WhenNewCountBelowCurrent_RemovesRowsAndShrinksMinimumHeight` — expect the mirrored
    assertions.
35. `AdjustTlp_WhenNewCountEqualsCurrent_LeavesPanelUnchanged` — boundary; expect `RowStyles.Count` and
    `MinimumSize` both unchanged.
36. `AdjustTlp_WhenCurrentRowCountIsZero_ClampsOldCountToZeroBeforeInserting` — panel with
    `RowCount == 0` so `oldCount` is `-1` before the `Math.Max(0, oldCount)` clamp at L307; expect
    insertion of exactly `newCount` rows.

**`ButtonSkipHandler` / `ButtonSkip_Click`**

37. `ButtonSkipHandler_WhenFormViewerIsNull_SkipsGroupWithoutTouchingButtonState` — `_formViewer`
    nulled by reflection; expect `TryDequeueAsync` reached and no `VerifySet` on the skip button.
38. `ButtonSkipCoreAsync_WhenSkipThrows_LogsAndRethrows` — `TryDequeueAsync` throws; expect
    `Should().ThrowAsync<...>()`.
39. `ButtonSkipCoreAsync_WhenSynchronizationContextIsNull_SetsViewerContext` — expect
    `SynchronizationContext.Current` equals the viewer's fake context.

**`SkipGroupAsync`**

40. `SkipGroupAsync_WhenQueueIsNull_ReturnsWithoutDequeuing` — `_qfcQueue` nulled by reflection; expect
    no interaction with the parent.
41. `SkipGroupAsync_WhenQueueHasWork_SwapsStopWatchAndAwaitsIterate` — expect `SwapStopWatch()` once,
    `IterateQueueAsync()` once, `CleanupBackground()` once, in that order (use a Moq `MockSequence` or
    an ordered callback recorder).
42. `SkipGroupAsync_WhenIterateTaskIsNull_CompletesWithoutAwaiting` — `_parent` nulled after dequeue is
    not possible; instead `IterateQueueAsync()` set up to return `null` so the `iterate is not null`
    branch at L375 takes the false arm; expect no throw.
43. `SkipGroupAsync_WhenQueueEmptyAndWorkerBusy_ShowsStillLoadingMessage` — `IsWorkerBusy` seam true;
    expect `ShowMessage` received the error-icon message and `TryDequeueAsync` never.
44. `SkipGroupAsync_WhenQueueEmptyAndWorkerIdle_DoesNotShowAnyMessage` — negative companion to 43 that
    pins the `logger.Info` arm; expect the `ShowMessage` seam never invoked.

**Cancellation**

45. `MoveAndIterate_WhenTokenAlreadyCancelled_PropagatesOperationCanceledFromDequeue` — the token
    passed to `TryDequeueAsync` is the controller's; set the mock to honour it via
    `Callback` + `ThrowIfCancellationRequested`; expect `OperationCanceledException`. Note: this file
    contains **no** direct `Token.ThrowIfCancellationRequested()` call — the two direct calls live in
    `QfcFormController.Actions.cs` (L81, L137) and are covered by that artifact's cases.

**Registration round-trip (state transition)**

46. `UnregisterFormEventHandlers_AfterRegister_StopsRoutingSkipClickToHandler` — register, unregister,
    then raise `SkipClicked`; expect `TryDequeueAsync` never. (The `Register`/`Unregister` methods
    themselves belong to `.SetupDisposal.cs`; this case exists here because it pins the *handlers
    declared in this file* as the subscription payload. Coordinate with the `.SetupDisposal.cs`
    artifact to avoid duplicating it — see §8, OQ-5.)

---

## 6. Determinism and policy notes

Framework: **MSTest** `[TestClass]`/`[TestMethod]`, **Moq** for all boundaries, **FluentAssertions**
for new assertions (`CLAUDE.md` § CUT1-CUT2). Arrange–Act–Assert with a one-line intent comment per
test.

Prohibited and avoided, per site:

| Banned in tests | Production site in this file | How the seam removes it |
| --- | --- | --- |
| Modal popup requiring human interaction | L180, L204, L382 `MessageBox.Show` | S3 `ShowMessage` delegate; the test asserts on a captured invocation and never opens a window |
| `Task.Delay` / real wall-clock wait | L254 `await Task.Delay(100)` | S5 `DelayAsync` delegate substituted with `Task.CompletedTask`; the loop's iteration count is driven by a `SetupSequence` on `WorkerComplete`, not by elapsed time |
| Live form / UI thread dependency | L199, L228, L233 `UiThread.Dispatcher` | S1 `IUiDispatcher` mock (executes the delegate inline) and S2 delegate. Without the seam these lines are order-dependent: `UiThread._dispatcher` is `null!` until `UiThread.Init()` runs, and `QfcHomeControllerRunAsyncTests.cs` L329 calls `UiThread.Init(false)` — so today's behavior depends on assembly test ordering. Removing that coupling is a determinism win, not only a coverage win |
| Non-deterministic concurrency | L178, L380 `BackgroundWorker.IsBusy` | S4 `IsWorkerBusy` predicate |
| Temporary files | none in this file | — |
| External services / live Outlook COM | none — this file declares no COM-typed parameter | — |

Ambient-state hygiene: five sites call `SynchronizationContext.SetSynchronizationContext`. Every test
that reaches one must capture `SynchronizationContext.Current` in `[TestInitialize]` and restore it in
`[TestCleanup]`, otherwise test independence (UT1) is violated for the whole assembly.

**STA last-resort clause: NOT invoked for this file.** Cases 33-36 construct an in-memory,
never-shown `TableLayoutPanel`, which the epic's Shared Design §3 permits, but no `[STATestMethod]`
scoping is required: `TableLayoutHelper` takes its non-`Invoke` path when `InvokeRequired == false`,
and `QfcFormControllerTests.cs` already constructs `TableLayoutPanel` successfully in this assembly's
default apartment (L215, L540, L558, L596). **No `*.StaTests.cs` file is needed for
`QfcFormController.EventHandlers.cs`.** If a future execution cycle shows an apartment-dependent
failure in cases 33-36, the fallback is `QuickFiler.Test/Controllers/QfcFormControllerAdjustTlp.StaTests.cs`
and nothing else moves.

Every proposed case is independent (fresh controller per test via the existing
`CreateQfcFormController()` helper shape), isolated to one member, sub-millisecond, and deterministic.

---

## 7. Upstream dependency on F1

F1 (`quickfiler-coverage-denominator-and-exemption-ledger`, wave 0) is being prepared concurrently and
its outputs **do not exist on disk yet**. Their absence is not a gap and is not reported as a blocker.
This file's verification consumes the F1 contract as follows:

1. **Denominator authority.** `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`
   is the authority for whether `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` is
   classified `testable` or `ratified-exempt`. The expectation from this research is `testable`: the
   file carries no `[ExcludeFromCodeCoverage]`, declares no COM-typed parameter, and every blocker
   identified in §3 is removable by a seam listed in §4. If F1's ledger nonetheless classifies it
   `ratified-exempt`, §5 collapses to zero required cases and the plan must cite the ledger entry
   rather than proceed.
2. **Measurement mechanism.** F1's per-file coverage report harness (derived from the Cobertura output
   of `scripts\vscode\Invoke-MSTestWithCoverage.ps1`) is the only accepted evidence mechanism. The
   numeric per-file result for this file is committed under
   `docs/features/active/2026-08-07-quickfiler-qfc-form-explorer-controller-coverage-435/evidence/qa-gates/`.
   Aggregate `QuickFiler.dll` coverage does not satisfy the acceptance criterion (epic Shared Design §6).
3. **Policy reconciliation and seam conventions.** F1 settles refactor-first-exempt-the-remainder and
   the seam hierarchy. §4 is written to that convention and needs no local re-derivation.

---

## 8. Open questions / findings

- **OQ-1 — Where do the seam fields live?** §4 places every seam field/property and the optional
  `IUiDispatcher` ctor parameter in `QfcFormController.cs`. That file is documented by a sibling
  researcher **within the same child F6**. The plan author must sequence the `QfcFormController.cs`
  edit as a Phase-0/early task so the `.EventHandlers.cs` and `.Actions.cs` tasks can consume it. This
  is intra-child sequencing, not a cross-child contract.
- **OQ-2 — Shared vs per-file seams.** `ShowMessage` (S3), `IsWorkerBusy` (S4) and the dispatcher seams
  (S1/S2) are used by **both** `.EventHandlers.cs` and `.Actions.cs`. Declare each exactly once. The
  `Actions.cs` artifact proposes an additional, distinct `PromptYesNo` seam for `UndoDialog`; the two
  message seams must remain separate.
- **OQ-3 — Latent null-safety asymmetry at the worker probe.** L178 reads `_formViewer.Worker?.IsBusy`
  (no null-conditional on `_formViewer`) while L380 reads `_formViewer?.Worker?.IsBusy`. Adopting the
  S4 predicate makes both null-safe, which is strictly more tolerant at L178. Decide explicitly
  whether that counts as a behavior change under AC "no behavior change to observable QuickFiler
  flows"; the recommendation is that it does not (the pre-guard at L149 already returns when
  `_formViewer is null`, so L178 is unreachable with a null viewer today).
- **OQ-4 — Fire-and-forget dispatch is preserved, not fixed.** L199 and L228 discard the inner `Task`
  (see §3.1). The seams are shaped to preserve that. If the maintainer wants it fixed, it is a
  separate issue, not F6 scope.
- **OQ-5 — Test-case ownership overlap with `.SetupDisposal.cs`.** Case 46 asserts a
  register/unregister round trip. `RegisterFormEventHandlers`/`UnregisterFormEventHandlers` are
  declared in `.SetupDisposal.cs`. Assign the case to exactly one artifact's plan phase to avoid two
  atomic tasks writing the same test.
- **Finding F-1 — `ButtonUndo_Click()` (parameterless) has zero test reach.** It is declared on
  `QuickFiler.Controllers.IQfcFormController` (L25) and on `QuickFiler.Interfaces.IQfcFormController`
  (L11) but is invoked by no test and by no production call site found in this file's partial family.
  Case 24 pins its current behavior; do not delete it (that would be an API change).
- **Finding F-2 — Two distinct `IQfcFormController` interfaces exist.**
  `QuickFiler/Controllers/IQfcFormController.cs` (43 lines, implemented by this class) and
  `QuickFiler/Interfaces/IQfcFormController.cs` (25 lines, declaring `MaximizeQfcFormViewer` /
  `MinimizeQfcFormViewer` / `ButtonCancel_Click()` / `ButtonOK_Click()`, none of which
  `QfcFormController` implements). The concrete class declares `: IQfcFormController` inside
  `namespace QuickFiler.Controllers`, so it binds to the `Controllers` one. The `Interfaces` one
  appears to be dead. This duplication is called out in the issue as needing a recorded determination;
  both files are in F6's set but neither is this artifact's file, so the determination belongs to the
  interface-file researcher.
- **Finding F-3 — The existing seam test `LoadItemsAsync_MailItemPath_DoesNotApplyPostDisplayHighConfidenceRemoval`
  reads production source text off disk** (`QfcFormControllerSeamTests.cs` L59-84, L352-374) and
  string-matches exact method signatures in `QfcFormController.Actions.cs`. It executes zero
  production lines and it will break if any signature in that file is reflowed. It does not touch this
  file, but the plan author must know it exists before touching `Actions.cs`. Full detail is in the
  `QfcFormController.Actions.cs.md` artifact.
- **Finding F-4 — Static-dispatcher test-order coupling already exists in this assembly.**
  `QfcHomeControllerRunAsyncTests.cs` L329 calls `UiThread.Init(false)`, which constructs and shows a
  hidden `SyncContextForm`; `QfcItemController.TestSupport.cs` L238 and `WpfUiDispatcherTests.cs` L42
  seed `UiThread._dispatcher` reflectively. Any new test that reaches an unseamed
  `UiThread.Dispatcher` would inherit that ordering dependency. Adopting S1/S2 removes the exposure
  for this file rather than adding to it.
