# Research — `QuickFiler/Controllers/QfcFormController.Actions.cs`

Timestamp: 2026-08-07T22-05

| Field | Value |
| --- | --- |
| Production file | `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a8220048ded06d508\QuickFiler\Controllers\QfcFormController.Actions.cs` |
| Exact line count | 302 |
| `[ExcludeFromCodeCoverage]` | **No.** The file declares `internal partial class QfcFormController` at line 18 with no attribute; no member carries the attribute either. Verified by reading the entire file. |
| Compiled | Yes — `QuickFiler/QuickFiler.csproj` line 320: `<Compile Include="Controllers\QfcFormController.Actions.cs" />` |
| Feature child | F6 (issue #435), epic #136 |

Current numeric per-file line coverage is **unmeasured**. It cannot be determined without running the
toolchain. The command that would produce it is
`scripts\vscode\Invoke-MSTestWithCoverage.ps1` (Cobertura output), consumed by F1's per-file report
harness. Do not substitute an invented figure.

---

## 1. Type-level context (read for grounding, not owned by this artifact)

`QfcFormController` is a four-file partial: `QfcFormController.cs` (196), `.SetupDisposal.cs` (232),
`.EventHandlers.cs` (399), `.Actions.cs` (302, this artifact).

Constructor (`QfcFormController.cs` L27-51) takes `IApplicationGlobals`, `IQfcFormViewer`,
`IQfcQueue`, `QfEnums.InitTypeEnum`, `System.Action parentCleanup`, `IQfcHomeController`,
`CancellationTokenSource`, `CancellationToken`. It dereferences three of them immediately —
`_globals.AF.MaximizeQuickFileWindow = MaximizeFormViewer` (L43), `_formViewer.SetController(this)`
(L44), `parent.WriteMetricsAsync` (L47) — so `_globals`, `_formViewer` and `_parent` **cannot** be
null at construction. `tokenSource` is stored without dereference, so it can be passed as `null`.

Fields this file reads or writes, declared in `QfcFormController.cs` L71-92 unless noted:
`_globals`, `_formViewer`, `_parent`, `_tokenSource`, `_states` (`TlpCellStates`), `_groups`
(`IQfcCollectionController`), `_rowStyleTemplate`, `_rowStyleExpanded`, `_helperTasks`
(`List<Task<MailItemHelper>>`), `_movedItems` (`SloStack<IMovedMailInfo>`), `_undoQueue`
(`BlockingCollection<IMovedMailInfo>`, initialized to `[]` at L90), `_undoConsumerTask` (`Task`).

`Cleanup()` (`.SetupDisposal.cs` L208-228) nulls `_globals`, `_formViewer`, `_groups`,
`_rowStyleTemplate`, `_parent`, `_movedItems`, and disposes `_undoQueue`. That is the only
behavior-realistic route to a null `_globals`/`_formViewer`/`_parent` and is therefore relevant to the
guard tests below.

---

## 2. Current test coverage inventory

Search scope: the whole `QuickFiler.Test` tree. Only two test files instantiate the concrete
controller; the nine other files that mention `QfcFormController` reference `Mock<IQfcFormController>`
or a commented-out line and execute zero production lines of this file.

### 2.1 `QuickFiler.Test/Controllers/QfcFormControllerTests.cs`

| Test method (line) | Production member reached | Depth reached |
| --- | --- | --- |
| `LoadItems_ShouldLoadItems` (L591) | `LoadItems(TableLayoutPanel, List<QfcItemGroup>)` | **Guard only** — `_groups` is null at L24 → return |
| `LoadItems_ShouldLoadMailItems` (L606) | `LoadItems(IList<MailItem>)` | **Guard only** — `_states` is null at L40 → return. The `QfcCollectionController` construction at L49 is never reached |
| `LoadItemsAsync_ShouldLoadMailItemsAsync` (L620) | `LoadItemsAsync(IList<MailItem>)` → `LoadItemsAsync(IList<MailItem>, ProgressTracker)` | 1-arg forwarder fully; 2-arg **guard only** (`_states` null) |
| `MaximizeFormViewer_ShouldMaximizeForm` (L634) | `MaximizeFormViewer()` | **Full.** The mock stubs `Invoke(It.IsAny<Delegate>())` with `.Callback<Delegate>(action => action.DynamicInvoke())`, so the inner `Action` really runs and `WindowState = Maximized` is observed |
| `MinimizeFormViewer_ShouldMinimizeForm` (L654) | `MinimizeFormViewer()` | **Full**, same mechanism |
| `UndoDialog_ShouldUndoMoves` (L674) | `UndoDialog()` | **Guard only** — `_movedItems` is null at L206 (a loose `Mock<IAppAutoFileObjects>` returns `null` for `MovedMails`) → return. No `Task.Run`, no `MessageBox` |
| `UndoConsumer_ShouldConsumeUndoQueue` (L687) | **none** | The body is `await Task.CompletedTask;` plus a suppressed `Assert.IsTrue(true)`. It executes **zero** production lines and is documented in-file as a pre-existing tautological placeholder (L694-700) |
| `Viewer_Activate_ShouldThrowNotImplementedException` (L703) | `Viewer_Activate()` | **Full** — asserts `NotImplementedException` |
| `ApplyHighConfidenceFilterAsync_WhenModeEnabled_RemovesBelowThresholdOnce` (L715) | `ApplyHighConfidenceFilterAsync` | Enabled arm, `RemoveBelowThresholdAsync(0.9)` verified |
| `ApplyHighConfidenceFilterAsync_WhenGroupsIsNull_DoesNothing` (L734) | `ApplyHighConfidenceFilterAsync` | First guard operand |
| `ApplyHighConfidenceFilterAsync_WhenQfSettingsIsNull_DoesNotRemove` (L749) | `ApplyHighConfidenceFilterAsync` | Second guard operand |
| `ApplyHighConfidenceFilterAsync_WhenModeDisabled_NeverRemoves` (L764) | `ApplyHighConfidenceFilterAsync` | Disabled arm |
| `LoadItemsAsync_PreScored_DoesNotInvokePostUiRemoval` (L799) | `LoadItemsAsync(IList<QfcPreScoredItem>)` → 2-arg overload | 1-arg forwarder fully; 2-arg **guard only** (`_states` null). The test's own XML doc (L787-798) states this explicitly |

`LoadUiFromQueue_ShouldLoadUi` (L418), `ButtonSkip_Click_ShouldSkipGroup` (L549) and
`SkipGroupAsync_ShouldSkipGroup` (L570) also reach `LoadItems(TableLayoutPanel, List<QfcItemGroup>)`
indirectly, but each stops at the same `_groups is null` guard.

### 2.2 `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs`

| Test method (line) | Production member reached | Depth reached |
| --- | --- | --- |
| `UndoClicked_WhenRaised_RoutesToControllerWithoutThrowing` (L203) | `UndoDialog()` | Guard only, via `ButtonUndo_Click` |
| `LoadItemsAsync_MailItemPath_DoesNotApplyPostDisplayHighConfidenceRemoval` (L352) | **none** | This is a **source-text inspection** test: it reads `QuickFiler/Controllers/QfcFormController.Actions.cs` from disk with `File.ReadAllText` (helper at L59-84) and asserts on substrings. It executes zero production lines. See Finding F-1 |

### 2.3 Anti-duplication conclusion

Already covered — **do not re-test**: `MaximizeFormViewer()` and `MinimizeFormViewer()` (both full);
`Viewer_Activate()`; all four branches of `ApplyHighConfidenceFilterAsync`; the two 1-argument
`LoadItemsAsync` forwarders; the `_groups is null` guard of `LoadItems(TableLayoutPanel, …)`.

Genuinely uncovered — the plan's target set: the body of `LoadItems(TableLayoutPanel, …)`; five of the
six guard operands and the entire body of `LoadItems(IList<MailItem>)`; five of six guard operands,
the `Token.ThrowIfCancellationRequested()` line and the entire body of both 2-argument
`LoadItemsAsync` overloads; the entire body of `UndoDialog()` (all five decision paths); the entire
body of `UndoConsumer()`.

---

## 3. Member-by-member reachability table

`C` = covered, `P` = partially covered, `U` = unreachable from any current deterministic test.

| # | Member (lines) | Kind | State | Concrete blocker for the uncovered portion |
| --- | --- | --- | --- | --- |
| 1 | `LoadItems(TableLayoutPanel, List<QfcItemGroup>)` (22-30) | public void | P (guard only) | None. Needs `_groups` injected as a `Mock<IQfcCollectionController>` (existing tests already do this via `SetPrivateField(_controller, "_groups", …)`, e.g. `QfcFormControllerTests.cs` L512) |
| 2 | `LoadItems(IList<MailItem>)` (32-60) | public void | P (guard only) | Two blockers: (a) L46-48 calls the static `MailItemHelper.FromMailItemAsync` once per element — a COM-bound materializer (`UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Loading.cs` L132) that dereferences the live `MailItem`; (b) L49-58 constructs `new QfcCollectionController(...)` concretely, whose ctor immediately dereferences `_formViewer.L1v0L2L3v_TableLayout`, `_formViewer.L1v0L2_PanelMain`, `_homeController.KeyboardHandler` and `_globals.Ol.DarkMode` (`QfcCollectionController.cs` L44-52) and throws on a loose mock |
| 3 | `LoadItemsAsync(IList<MailItem>)` (62-65) | public async Task | C | — |
| 4 | `LoadItemsAsync(IList<MailItem>, ProgressTracker)` (67-105) | public async Task | P (guard only) | L83-92 `new QfcCollectionController(...)` — same blocker as #2(b). Everything after it (`LoadControlsAndHandlers_01Async`, `progress?.Report(100)`, `WindowState`/`Show`/`Refresh`, `LoadSecondaryAsync`) is interface-reachable once the construction is seamed |
| 5 | `LoadItemsAsync(IList<QfcPreScoredItem>)` (114-117) | public async Task | C | — |
| 6 | `LoadItemsAsync(IList<QfcPreScoredItem>, ProgressTracker)` (120-164) | public async Task | P (guard only) | L139-148 `new QfcCollectionController(...)` — same blocker |
| 7 | `ApplyHighConfidenceFilterAsync(IQfcCollectionController)` (171-182) | internal async Task | **C** | Documented as dormant (issue #171 post-display helper superseded by the issue #233 dequeue-time gate), but it is still executable code in the denominator. It is already fully covered by four tests; no new work |
| 8 | `MaximizeFormViewer()` (187-192) | public void | **C** | `IQfcFormViewer` inherits `object Invoke(Delegate method)` from `IControl` (`UtilitiesCS/Interfaces/IWinForm/IControl.cs` L176), so **a Moq'd `IQfcFormViewer` does satisfy `Invoke`**. The existing tests stub it with a `Callback<Delegate>` that calls `DynamicInvoke()`, so the inner delegate really executes and the `WindowState` setter is observed via `SetupSet`. No seam required |
| 9 | `MinimizeFormViewer()` (197-202) | public void | **C** | Same as #8 |
| 10 | `UndoDialog()` (204-251) | internal void | P (guard only) | Three blockers: (a) L225 and L238 `MessageBox.Show(..., MessageBoxButtons.YesNo)` and L248 `MessageBox.Show("Nothing to undo")` open modal popups — a hard unit-test-policy violation, not a preference; (b) L211 `_undoConsumerTask ??= Task.Run(UndoConsumer)` starts a real background task which, per Finding F-2, never terminates; (c) `_movedItems` must be a real `SloStack<IMovedMailInfo>` — see §4.7 for why `Serialize()` at L250 is nonetheless disk-safe |
| 11 | `UndoConsumer()` (253-292) | internal async Task | **U** | Four blockers: (a) L255-256 `new Stopwatch()` + L279 `sw.ElapsedMilliseconds > 10000` is a wall-clock read; (b) L285 `await Task.Delay(200)` is a banned API and a real wall-clock wait; (c) L262-267 static `MailItemHelper.FromMailItemAsync`; (d) L268-272 `(await _globals.AF.Manager["Folder"]).UnTrain(...)` where `ManagerAsyncLazy` is a concrete non-mockable type (see §4.5); plus (e) L274-277 `UiThread.Dispatcher.InvokeAsync(...)`. See Finding F-2 for the two lines that remain uncoverable even after all seams |
| 12 | `Viewer_Activate()` (295-298) | public void | **C** | Body is `throw new NotImplementedException();` only. Already pinned. **Do not implement it** — that would be a behavior change the acceptance criteria forbid |

---

## 4. Seam design proposal

Hierarchy per `.claude/rules/csharp.md`: interface seam > injectable delegate > adapter. Seams S1
(`IUiDispatcher`) and S3 (`ShowMessage`) are shared with `QfcFormController.EventHandlers.cs`; declare
each exactly once (see that artifact and §8, OQ-1).

### A1 — `new QfcCollectionController(...)` → injectable factory returning `IQfcCollectionController`

**Sites:** L49-58 (`LoadItems(IList<MailItem>)`), L83-92 (`LoadItemsAsync(IList<MailItem>, …)`),
L139-148 (`LoadItemsAsync(IList<QfcPreScoredItem>, …)`). **All three verified independently by
reading the file.**
**Tier:** injectable delegate (tier 2).

`QuickFiler/Controllers/QfcCollectionController.cs` belongs to sibling child **F11 and must not be
edited**. Its 8-parameter constructor (L30-39: `IApplicationGlobals AppGlobals`,
`IQfcFormViewer viewerInstance`, `QfEnums.InitTypeEnum InitType`, `IFilerHomeController homeController`,
`IFilerFormController parent`, `CancellationTokenSource tokenSource`, `CancellationToken token`,
`TlpCellStates tlpStates`) is not virtual and the type has no injectable seam of its own. An interface
seam is therefore impossible without editing F11's file; the delegate is the smallest available seam.

Recommended shape — a zero-argument factory assigned in the constructor, which preserves the current
semantics of reading the fields **at call time**:

```csharp
// QfcFormController.cs, fields region
internal Func<IQfcCollectionController> CreateCollectionController { get; set; }

// QfcFormController.cs, constructor tail
CreateCollectionController = () =>
    new QfcCollectionController(
        AppGlobals: _globals,
        viewerInstance: _formViewer,
        InitType: QfEnums.InitTypeEnum.Sort,
        homeController: _parent,
        parent: this,
        tokenSource: TokenSource,
        token: Token,
        _states
    );
```

Each of the three call sites collapses to `_groups = CreateCollectionController();`. Rationale for the
zero-argument shape over an 8-parameter `Func<,,,,,,,,>`: it preserves the named-argument readability
that the current call sites rely on, keeps the three sites byte-identical to each other, and avoids an
unwieldy nine-type-argument generic. **No consumer changes and no sibling-file changes** — the
default value reproduces today's construction exactly, and `_groups` remains typed
`IQfcCollectionController` (`QfcFormController.cs` L157).

### A2 — `MessageBox.Show(..., MessageBoxButtons.YesNo)` → injectable prompt delegate

**Sites:** L225-229 (per-item "undo this move?"), L238-242 ("Continue Undoing Moves?"), L248
("Nothing to undo").
**Tier:** injectable delegate (tier 2). `MessageBox` is a static WinForms API with no interface to
extract, and a one-member adapter class would add a production file whose body is untestable.

A unit test must **never** produce a popup requiring human interaction. This is a hard policy
requirement (`CLAUDE.md` § UT4 "Avoid External Dependencies"/"Environment Stability"; epic Shared
Design §2 "never show popups (a popup requiring human interaction is a unit-test-policy violation)"),
not a stylistic preference. Without this seam, `UndoDialog()` past its guard would block the test run
indefinitely.

```csharp
internal Func<string, string, MessageBoxButtons, DialogResult> PromptUser { get; set; } =
    (text, caption, buttons) => MessageBox.Show(text, caption, buttons);
```

L225 → `PromptUser(message, "Undo Dialog", MessageBoxButtons.YesNo)`.
L238 → `PromptUser("Continue Undoing Moves?", "Undo Dialog", MessageBoxButtons.YesNo)`.
L248 → `_ = PromptUser("Nothing to undo", string.Empty, MessageBoxButtons.OK);` — behaviorally
identical to the one-argument `MessageBox.Show(text)` overload, which renders an empty caption and a
single OK button. See §8, OQ-2 if the plan author prefers a separate one-argument notice seam.

**Enumerated decision-loop branches** (each becomes its own test case in §5):

| Branch | Lines | Condition | Effect |
| --- | --- | --- | --- |
| message-is-null | 219-222 | `UndoMoveMessage(olApp)` returns `null` | `i++`, no prompt, loop continues |
| undo-yes | 230-233 | first prompt returns `Yes` | `_undoQueue.Add(_movedItems.Pop(i))`, `i` not advanced |
| undo-no | 234-237 | first prompt returns `No` | `i++` |
| repeat-no | 238-243 | second prompt returns `No` | `repeatResponse = No` → `while` exits, and the L246 "Nothing to undo" notice is **suppressed** |
| nothing-to-undo | 246-249 | loop exits with `repeatResponse == Yes` (stack exhausted, or empty from the start) | third notice shown |

Note the ordering invariant these cases pin: `_undoQueue.Add(_movedItems.Pop(i))` removes from the
stack at ordinal `i` **before** enqueueing, and `SloStack<T>` defines top-of-stack as index 0
(`UtilitiesCS/.../SloStack.cs` L14-21), with `Pop(int)` shifting higher ordinals down (L145-151). A
"yes" therefore leaves `i` pointing at what was the next element, which is why `i` is not incremented
on the yes path. That asymmetry is exactly what the undo-queue ordering test must assert.

### A3 — `Task.Run(UndoConsumer)` → injectable background-start delegate

**Site:** L211 `_undoConsumerTask ??= Task.Run(UndoConsumer);`
**Tier:** injectable delegate (tier 2).

Without this seam, any `UndoDialog` test that passes the guard spawns a real background task that,
per Finding F-2, spins forever and leaks into the test host for the remainder of the run — a direct
violation of test isolation and fast execution.

```csharp
internal Func<Func<Task>, Task> StartBackground { get; set; } = work => Task.Run(work);
```

Call site: `_undoConsumerTask ??= StartBackground(UndoConsumer);`. Tests substitute a recorder that
captures the delegate and returns `Task.CompletedTask` without executing it, which also lets a test
assert the `??=` idempotence (started at most once across repeated `UndoDialog()` calls).

### A4 — `Stopwatch` + `Task.Delay(200)` → injectable elapsed probe and delay delegate

**Sites:** L255-256 (`new Stopwatch()`, `sw.Start()`), L279 (`sw.ElapsedMilliseconds > 10000`),
L285 (`await Task.Delay(200)`).
**Tier:** injectable delegate (tier 2).

`Task.Delay` and real wall-clock waits are banned in tests
(`.claude/rules/general-unit-test.md` § Determinism Infrastructure) and `Task.Delay` / `Stopwatch`-free
timing is the repo direction (`.claude/rules/csharp.md` § Banned symbols lists `Task.Delay`). A
`FakeTimeProvider` is the guidance-preferred time seam, but it is a poor fit here because the loop's
delay is a fixed `Task.Delay` rather than a clock read; two narrow delegates express the same intent
with less machinery:

```csharp
internal Func<int, Task> DelayAsync { get; set; } = ms => Task.Delay(ms);
internal Func<Stopwatch, long> ElapsedMilliseconds { get; set; } = sw => sw.ElapsedMilliseconds;
```

`ElapsedMilliseconds` keeps the `Stopwatch` instance local (no ownership change) while letting a test
return a scripted sequence such as `0, 0, 10001`. `DelayAsync` is substituted with
`_ => Task.CompletedTask`.

A `TimeProvider`-based alternative is available: `Microsoft.Bcl.TimeProvider` is referenced in the
repository and `UiThread.Init` already accepts a `TimeProvider?` (`UiThread.cs` L22). If the plan
author prefers a single time abstraction over two delegates, injecting `TimeProvider` and replacing
`Stopwatch` with `TimeProvider.GetTimestamp()`/`GetElapsedTime()` is acceptable — but it changes the
timing source type, so it must be justified as behavior-neutral. The two-delegate form is recommended
for minimal diff.

### A5 — `_globals.AF.Manager["Folder"]` untrain step → injectable delegate

**Site:** L268-272 `(await _globals.AF.Manager["Folder"]).UnTrain(helper.FolderInfo.RelativePath, helper.Tokens, 1);`
**Tier:** injectable delegate (tier 2).

`IAppAutoFileObjects.Manager` (`UtilitiesCS/Interfaces/IGlobals/IAppAutoFileObjects.cs` L37) is typed
as the concrete `ManagerAsyncLazy`, which derives from
`ConcurrentObservableDictionary<string, AsyncLazy<BayesianClassifierGroup>>`, requires an
`IApplicationGlobals` in its constructor and calls `ResetConfigAsyncLazy()` there
(`UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs` L28-42). Its indexer is
inherited and non-virtual, so Moq cannot intercept `Manager["Folder"]`. An interface seam would
require editing `UtilitiesCS`, outside the epic's QuickFiler denominator.

```csharp
internal Func<MailItemHelper, Task> UnTrainFolderAsync { get; set; } // assigned in the ctor
// default:
UnTrainFolderAsync = async helper =>
    (await _globals.AF.Manager["Folder"]).UnTrain(helper.FolderInfo.RelativePath, helper.Tokens, 1);
```

### A6 — `MailItemHelper.FromMailItemAsync` → injectable factory delegate

**Sites:** L46-48 (`LoadItems(IList<MailItem>)`), L262-267 (`UndoConsumer`).
**Tier:** injectable delegate (tier 2) — the target is a static method, and the repo's own precedent
for exactly this call is the `IFolderScoringService` seam plus an `[ExcludeFromCodeCoverage]` adapter
(`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` L156-178).

```csharp
internal Func<MailItem, IApplicationGlobals, CancellationToken, bool, Task<MailItemHelper>>
    CreateMailItemHelperAsync { get; set; } = MailItemHelper.FromMailItemAsync;
```

`MailItemHelper` has a public parameterless constructor (`MailItemHelper.cs` L80), so a test can
return `Task.FromResult(new MailItemHelper())` without touching COM.

### A7 — `UiThread.Dispatcher.InvokeAsync` → existing `IUiDispatcher` interface seam (shared)

**Site:** L274-277 `await UiThread.Dispatcher.InvokeAsync(() => _groups.AddItemGroup(mail), DispatcherPriority.ContextIdle);`
**Tier:** interface seam (tier 1). Shared with `QfcFormController.EventHandlers.cs`.

`UtilitiesCS.Threading.IUiDispatcher` exposes
`Task InvokeAsync(Action action, DispatcherPriority priority, CancellationToken token)`
(`IUiDispatcher.cs` L27), and `WpfUiDispatcher` forwards it to `Dispatcher.InvokeAsync(action, priority, token).Task`
(`WpfUiDispatcher.cs` L46-50). Because the payload here is an `Action` (not an async lambda), routing
it through the three-argument member with `CancellationToken.None` is semantically identical. The
in-repo precedent is `QfcItemController`, which takes an optional
`UtilitiesCS.Threading.IUiDispatcher uiDispatcher = null` ctor parameter
(`QfcItemController.Initialization.cs` L38, L57) and defaults it with
`_uiDispatcher ??= new UtilitiesCS.Threading.WpfUiDispatcher();` (L380).

Without the seam this line is order-dependent: `UiThread._dispatcher` is `null!` until `UiThread.Init()`
runs, and `QfcHomeControllerRunAsyncTests.cs` L329 calls `UiThread.Init(false)` — so behavior in this
assembly depends on test ordering today.

### 4.7 What deliberately gets **no** seam

- **`_formViewer.Invoke(...)` at L189 and L199.** `IControl.Invoke(Delegate method)` is an interface
  member; a Moq'd `IQfcFormViewer` satisfies it, and the existing tests already execute the inner
  delegate via `.Callback<Delegate>(action => action.DynamicInvoke())` and assert the resulting
  `WindowState` through `SetupSet`. Both methods are already fully covered; no seam and no new test.
  *(If a future test chooses not to run the callback, the assertion route would be
  `_mockFormViewer.Verify(fv => fv.Invoke(It.IsAny<Delegate>()), Times.Once)` plus a manual
  `DynamicInvoke` of the captured delegate — but that is unnecessary here.)*
- **`_movedItems.Serialize()` at L250.** Verified disk-safe: `SloStack<T>.Serialize()` forwards to
  `SmartSerializable<T>.Serialize()`, whose entire body is
  `if (Config.Disk.FilePath != "") { RequestSerialization(...); }`
  (`UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs` L442-448), and
  `FilePathHelper._filePath` defaults to `""`
  (`UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs` L71). A freshly constructed
  `new SloStack<IMovedMailInfo>()` therefore writes nothing. **No temporary file is created**, so the
  prohibition on temp files in tests is satisfied without a seam.
- **`_undoQueue` (`BlockingCollection<IMovedMailInfo>`).** Concrete but directly drivable from a test:
  `Add(...)` to seed, `CompleteAdding()` to terminate the consumer loop, `Count` to assert ordering.
  No seam.
- **`IMovedMailInfo`** (`UndoMoveMessage(Application)`, `UndoMove()`, `MailItem`) is an interface
  (`UtilitiesCS/Interfaces/IEmailIntelligence/IMovedMailInfo.cs`) and is fully Moq-able. So is
  `Microsoft.Office.Interop.Outlook.Application`; the existing suite already mocks Outlook interop
  interfaces (`QfcFormControllerTests.cs` L814 mocks `MailItem`).
- **`ProgressTracker`.** `Report(double)` is `virtual` (`UtilitiesCS/Threading/ProgressTracker.cs` L141)
  and the type has a `ProgressTracker(CancellationTokenSource)` constructor (L20) that touches no UI as
  long as `Initialize()` is not called. `Mock<ProgressTracker>(new CancellationTokenSource())` is
  therefore usable directly to verify `progress?.Report(100)`. No seam.
- **`QfcPreScoredItem`** is a `readonly struct` with a public two-argument constructor
  (`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` L98-112); the existing suite already
  constructs one (`QfcFormControllerTests.cs` L814). No seam.

### 4.8 File-size and ownership constraints

- **`QfcFormController.Actions.cs` shrinks, not grows.** The three `new QfcCollectionController(...)`
  blocks (10 lines each) collapse to one line each (≈ −27); the three `MessageBox.Show` blocks shrink
  (≈ −8); the `FromMailItemAsync` call (6 lines) and the `UnTrain` call (5 lines) each collapse to one
  (≈ −9); the dispatcher call (4 lines) collapses (≈ −2). Projected: **302 − ~46 ≈ 256 lines.**
  Comfortably under 500. **No further partial split is required for this file.**
- The seam field/property declarations and the optional `IUiDispatcher` constructor parameter land in
  `QfcFormController.cs` (196 lines → roughly 226 after ~30 lines of seam declarations). Still under 500.
- `QuickFiler/Controllers/QfcCollectionController.cs` (F11) — **not edited** (A1 is why).
- `QuickFiler/Viewers/QfcFormViewer.cs` (F15) — **not edited**. No member is added to
  `IQfcFormViewer`; `Invoke`, `WindowState`, `Show`, `Refresh` are all already on the interface chain.
  **No `CROSS-CHILD CONTRACT NOTE (F15)` is required for this file.**
- `QuickFiler/Controllers/KeyboardHandler.cs` (F3) — **not edited**; this file does not reference it.
- `coverage.config` and shared build property files (F1) — **not modified**.
- .NET Framework 4.8: no default interface members, no `init` accessors, no `record`/`record struct`
  (they fail CS0518 in this repo). Every seam above is a plain property with a getter/setter typed as
  `Func<>`/`Action<>`.
- **INTRA-CHILD COORDINATION (not cross-child):** the seam declarations belong in `QfcFormController.cs`,
  documented by a sibling researcher on the same child F6. The plan must sequence that edit first.

### 4.9 Hard constraint imposed by an existing test — read before editing this file

`QfcFormControllerSeamTests.cs` L352-374 reads this production file's **text** and asserts:

- `"public async Task LoadItemsAsync(IList<MailItem> listObjects, ProgressTracker progress)"` occurs
  (currently L67, on a single line);
- `"public async Task LoadItemsAsync(IList<QfcPreScoredItem> preScored)"` occurs **after** it
  (currently L114);
- the text between those two indices contains neither `"ApplyHighConfidenceFilterAsync"` nor
  `"RemoveBelowThresholdAsync"`.

Therefore the plan must **not** rename either overload, **not** reorder them, **not** cause CSharpier to
reflow either signature onto multiple lines (which would happen if a parameter were added), and **not**
introduce either forbidden identifier into the `LoadItemsAsync(IList<MailItem>, ProgressTracker)` body.
Adding a parameter to the `MailItem` overload would break this test. The A1 seam is compatible because
it changes only the method **body**.

---

## 5. Proposed test cases

Target file: a **new** `QuickFiler.Test/Controllers/QfcFormControllerActionTests.cs`.
`QfcFormControllerTests.cs` is already 827 lines and `QfcFormControllerSeamTests.cs` 378; the 500-line
ceiling applies to test code too, and `QfcFormControllerSeamTests.cs` L21-23 documents the existing
split convention. If the list below exceeds 500 lines, split at case 32 into
`QfcFormControllerUndoTests.cs`.

Each numbered entry is exactly one MSTest method and becomes one atomic plan task. No batching.

**`LoadItems(TableLayoutPanel, List<QfcItemGroup>)` — 3-operand guard**

1. `LoadItems_TlpOverload_WhenGroupsIsNull_DoesNotLoadControls` — `_groups` null (default); expect no
   interaction with any collection controller.
2. `LoadItems_TlpOverload_WhenTlpIsNull_DoesNotLoadControls` — `_groups` mocked, `tlp` null; expect
   `LoadControlsAndHandlers_01` never.
3. `LoadItems_TlpOverload_WhenItemGroupsIsNull_DoesNotLoadControls` — `_groups` mocked, `itemGroups`
   null; expect `LoadControlsAndHandlers_01` never.
4. `LoadItems_TlpOverload_WhenAllArgumentsPresent_ForwardsToCollectionController` — expect
   `LoadControlsAndHandlers_01(tlp, itemGroups)` exactly once with the same instances.

**`LoadItems(IList<MailItem>)` — 6-operand guard (one case per operand)**

5. `LoadItems_MailItemOverload_WhenListIsNull_ReturnsWithoutBuildingHelpers`
6. `LoadItems_MailItemOverload_WhenGlobalsIsNull_ReturnsWithoutBuildingHelpers` — `_globals` nulled by
   reflection (or by calling `Cleanup()` first; reflection is preferred so the operand is attributable).
7. `LoadItems_MailItemOverload_WhenFormViewerIsNull_ReturnsWithoutBuildingHelpers`
8. `LoadItems_MailItemOverload_WhenParentIsNull_ReturnsWithoutBuildingHelpers`
9. `LoadItems_MailItemOverload_WhenTokenSourceIsNull_ReturnsWithoutBuildingHelpers` — construct the
   controller with a `null` `tokenSource`; no reflection needed.
10. `LoadItems_MailItemOverload_WhenStatesIsNull_ReturnsWithoutBuildingHelpers` — default state; asserts
    the operand the existing `LoadItems_ShouldLoadMailItems` reaches incidentally.
11. `LoadItems_MailItemOverload_WhenDependenciesPresent_CreatesOneHelperTaskPerMailItem` — three mocked
    `MailItem`s, seamed helper factory; expect the factory invoked three times and `_helperTasks.Count == 3`.
12. `LoadItems_MailItemOverload_WhenDependenciesPresent_AssignsGroupsFromFactoryAndLoadsControls` —
    expect `CreateCollectionController` invoked once, `Groups` equal to the returned mock, and
    `LoadControlsAndHandlers_01(listObjects, _rowStyleTemplate, _rowStyleExpanded)` once.
13. `LoadItems_MailItemOverload_WhenListIsEmpty_CreatesNoHelperTasksButStillCreatesGroups` — boundary;
    expect zero factory calls and one collection-controller creation.

**`LoadItemsAsync(IList<MailItem>, ProgressTracker)` — 6-operand guard, cancellation, body**

14. `LoadItemsAsync_MailItem_WhenListIsNull_ReturnsWithoutCreatingGroups`
15. `LoadItemsAsync_MailItem_WhenGlobalsIsNull_ReturnsWithoutCreatingGroups`
16. `LoadItemsAsync_MailItem_WhenFormViewerIsNull_ReturnsWithoutCreatingGroups`
17. `LoadItemsAsync_MailItem_WhenParentIsNull_ReturnsWithoutCreatingGroups`
18. `LoadItemsAsync_MailItem_WhenTokenSourceIsNull_ReturnsWithoutCreatingGroups`
19. `LoadItemsAsync_MailItem_WhenStatesIsNull_ReturnsWithoutCreatingGroups`
20. `LoadItemsAsync_MailItem_WhenTokenAlreadyCancelled_ThrowsOperationCanceled` — all six operands
    satisfied, controller constructed with a pre-cancelled token; expect
    `Should().ThrowAsync<OperationCanceledException>()` from `Token.ThrowIfCancellationRequested()` at L81.
21. `LoadItemsAsync_MailItem_WhenProgressSupplied_ReportsOneHundredAfterLoad` — expect
    `Mock<ProgressTracker>.Verify(p => p.Report(100d), Times.Once)`.
22. `LoadItemsAsync_MailItem_WhenProgressIsNull_CompletesWithoutThrowing` — null-conditional arm at L98.
23. `LoadItemsAsync_MailItem_WhenLoadSucceeds_MaximizesShowsAndRefreshesViewer` — expect
    `WindowState = Maximized` set once, `Show()` once, `Refresh()` once.
24. `LoadItemsAsync_MailItem_WhenLoadSucceeds_LoadsSecondaryAfterPrimary` — expect
    `LoadControlsAndHandlers_01Async` then `LoadSecondaryAsync`, in that order.
25. `LoadItemsAsync_MailItem_WhenPrimaryLoadThrows_PropagatesAndDoesNotShowViewer` — error handling;
    expect the exception surfaces and `Show()` never.

**`LoadItemsAsync(IList<QfcPreScoredItem>, ProgressTracker)` — 6-operand guard, cancellation, body**

26. `LoadItemsAsync_PreScored_WhenListIsNull_ReturnsWithoutCreatingGroups`
27. `LoadItemsAsync_PreScored_WhenGlobalsIsNull_ReturnsWithoutCreatingGroups`
28. `LoadItemsAsync_PreScored_WhenFormViewerIsNull_ReturnsWithoutCreatingGroups`
29. `LoadItemsAsync_PreScored_WhenParentIsNull_ReturnsWithoutCreatingGroups`
30. `LoadItemsAsync_PreScored_WhenTokenSourceIsNull_ReturnsWithoutCreatingGroups`
31. `LoadItemsAsync_PreScored_WhenStatesIsNull_ReturnsWithoutCreatingGroups`
32. `LoadItemsAsync_PreScored_WhenTokenAlreadyCancelled_ThrowsOperationCanceled` — L137.
33. `LoadItemsAsync_PreScored_WhenCarrierListSupplied_ForwardsCarriersToCollectionController` — expect
    `LoadControlsAndHandlers_01Async(preScored, _rowStyleTemplate, _rowStyleExpanded)` once with the
    same carrier instances (preserving `PredeterminedFolder`).
34. `LoadItemsAsync_PreScored_WhenProgressSupplied_ReportsOneHundredAfterLoad`
35. `LoadItemsAsync_PreScored_WhenLoadSucceeds_MaximizesShowsRefreshesAndLoadsSecondary`
36. `LoadItemsAsync_PreScored_WhenLoadSucceeds_NeverInvokesPostUiRemoval` — extends the existing
    guard-only `LoadItemsAsync_PreScored_DoesNotInvokePostUiRemoval` to the **positive** path; expect
    `RemoveBelowThresholdAsync` never (this is the behavioral assertion that the source-text test at
    `QfcFormControllerSeamTests.cs` L352 was standing in for).

**`UndoDialog()` — guard, five decision paths, ordering, idempotence**

37. `UndoDialog_WhenMovedItemsIsNull_ReturnsWithoutStartingConsumer` — first guard operand; expect the
    `StartBackground` seam never invoked.
38. `UndoDialog_WhenOutlookApplicationIsNull_ReturnsWithoutStartingConsumer` — second guard operand
    (`_globals.Ol.App` null).
39. `UndoDialog_WhenInvoked_StartsUndoConsumerExactlyOnce` — call `UndoDialog()` twice; expect
    `StartBackground` invoked once (pins the `??=` at L211).
40. `UndoDialog_WhenUndoMessageIsNull_SkipsItemWithoutPrompting` — single item whose
    `UndoMoveMessage` returns `null`; expect `PromptUser` never for that item and the stack unchanged.
41. `UndoDialog_WhenUserConfirmsUndo_MovesItemFromStackToUndoQueue` — prompts scripted `Yes` then `No`;
    expect `_undoQueue.Count == 1`, the stack shrunk by one, and the enqueued instance is the popped one.
42. `UndoDialog_WhenUserDeclinesUndo_LeavesItemOnStack` — prompts `No` then `No`; expect
    `_undoQueue.Count == 0` and the stack unchanged.
43. `UndoDialog_WhenUserDeclinesRepeat_StopsAfterFirstItem` — two items, second prompt returns `No`;
    expect exactly two `PromptUser` invocations and the second item never examined.
44. `UndoDialog_WhenStackIsEmpty_ShowsNothingToUndoNotice` — empty `SloStack`; expect the
    "Nothing to undo" notice exactly once.
45. `UndoDialog_WhenAllItemsUndone_ShowsNothingToUndoNotice` — every prompt `Yes`; loop exhausts the
    stack with `repeatResponse == Yes`; expect the notice once and `_undoQueue.Count` equal to the
    original stack depth.
46. `UndoDialog_WhenUserDeclinesRepeat_SuppressesNothingToUndoNotice` — negative companion to 44/45
    that pins the L246 condition.
47. `UndoDialog_WhenMultipleItemsUndone_PreservesStackOrderInUndoQueue` — three items pushed in a known
    order; expect the `BlockingCollection` drained in top-of-stack-first order, matching
    `SloStack`'s index-0-is-top contract.
48. `UndoDialog_WhenInvoked_SerializesMovedItemsExactlyOnceOnExit` — assert via a `SloStack` whose
    `Config.Disk.FilePath` remains `""`; expect completion with no file created (guards the temp-file
    prohibition).

**`UndoConsumer()`**

49. `UndoConsumer_WhenQueueHasItemAndIsCompleted_MaterializesHelperUntrainsAndReAddsItemGroup` — seed
    one item, call `CompleteAdding()` before invoking; expect the helper factory once, the untrain
    delegate once, `UndoMove()` once, and the dispatcher seam invoking `AddItemGroup(mail)` once.
50. `UndoConsumer_WhenQueueEmptyAndNotCompleted_AwaitsInjectedDelayWithoutWallClockWait` — the
    substituted delay delegate calls `CompleteAdding()` on its first invocation so the loop terminates;
    expect the delay seam invoked exactly once and zero elapsed wall-clock dependence.
51. `UndoConsumer_WhenQueueDrainedInOrder_ProcessesItemsInEnqueueOrder` — two items; expect the untrain
    delegate invoked twice with the helpers in enqueue order.
52. `UndoConsumer_WhenUntrainThrows_PropagatesAndLeavesConsumerTaskUnchanged` — error handling; the
    method has **no** `catch`, so the exception must surface to the caller.
53. `UndoConsumer_WhenQueueCompletedAndEmpty_ReturnsImmediatelyWithoutTouchingHelperFactory` — boundary;
    expect zero seam invocations.

**`Viewer_Activate()`**

54. *No new case.* `Viewer_Activate_ShouldThrowNotImplementedException`
    (`QfcFormControllerTests.cs` L703) already pins the current behavior. Do **not** implement the
    method — implementing it is a behavior change the acceptance criteria forbid.

**`ApplyHighConfidenceFilterAsync`**

55. *No new case.* Four existing tests already cover both guard operands and both mode arms. Dormancy
    does not exempt it from the denominator, but it is already at full branch coverage.

---

## 6. Determinism and policy notes

Framework: **MSTest** `[TestClass]`/`[TestMethod]`, **Moq** for all boundaries, **FluentAssertions**
for new assertions (`CLAUDE.md` § CUT1-CUT2). Arrange–Act–Assert with a one-line intent comment.

Prohibited and avoided, per site:

| Banned in tests | Production site in this file | How the seam removes it |
| --- | --- | --- |
| Modal popup requiring human interaction | L225, L238, L248 `MessageBox.Show` | A2 `PromptUser` delegate. The test scripts `DialogResult` values and asserts on captured invocations; no window is created. **Without this seam every `UndoDialog` test past the guard blocks the run indefinitely.** |
| `Task.Delay` / real wall-clock wait | L285 `await Task.Delay(200)` | A4 `DelayAsync` substituted with `Task.CompletedTask` |
| Wall-clock read | L255-256, L279 `Stopwatch` / `ElapsedMilliseconds` | A4 `ElapsedMilliseconds` probe returns a scripted sequence |
| Unbounded background task leaking into the host | L211 `Task.Run(UndoConsumer)` | A3 `StartBackground` captures the delegate and returns `Task.CompletedTask` without running it. Critical: per Finding F-2 the real task never terminates |
| Live form / UI-thread dependency | L274-277 `UiThread.Dispatcher` | A7 `IUiDispatcher` mock executes the action inline. Removes an existing assembly-ordering dependency (`UiThread._dispatcher` is `null!` until `UiThread.Init()`; `QfcHomeControllerRunAsyncTests.cs` L329 calls it) |
| Live Outlook COM | L47/L262 `MailItemHelper.FromMailItemAsync`; L268 `Manager["Folder"].UnTrain` | A6 and A5 delegates |
| Temporary files | L250 `_movedItems.Serialize()` | **No seam needed** — verified no-op when `Config.Disk.FilePath == ""`, which is the default (§4.7). No file is created |
| External services | none beyond the above | — |
| Constructing `QfcCollectionController` (which would need live WinForms/COM) | L49, L83, L139 | A1 factory returns a `Mock<IQfcCollectionController>` |

**STA last-resort clause: NOT invoked for this file.** No case above constructs a WinForms control.
Case 4 passes a `TableLayoutPanel` only as an opaque argument to a mocked
`IQfcCollectionController.LoadControlsAndHandlers_01`, and `QfcFormControllerTests.cs` already
constructs `TableLayoutPanel` successfully in this assembly's default apartment (L215, L540, L558,
L596). **No `*.StaTests.cs` file is needed for `QfcFormController.Actions.cs`.**

**One irreducible remainder is claimed, and only one** (Finding F-2): the `exit = true` assignment at
L281 and the `if (exit) { _undoConsumerTask = null; }` block at L288-291, roughly 7 of 302 lines
(≈ 2.3%). These are unreachable in any *terminating* execution because of the loop condition, not
because of a missing seam. They do not threaten the 80% floor and they do not qualify for the STA
clause, which addresses UI-host coupling rather than non-terminating control flow.

All other proposed cases are independent (fresh controller per test), isolated to one member,
sub-millisecond, and deterministic.

---

## 7. Upstream dependency on F1

F1 (`quickfiler-coverage-denominator-and-exemption-ledger`, wave 0) is being prepared concurrently and
its outputs **do not exist on disk yet**. Their absence is not a gap and is not a blocker. This file's
verification consumes the F1 contract as follows:

1. **Denominator authority.** `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` is
   the authority for whether `QuickFiler/Controllers/QfcFormController.Actions.cs` is classified
   `testable` or `ratified-exempt`. The expectation from this research is `testable`: the file carries
   no `[ExcludeFromCodeCoverage]`, and every blocker in §3 is removable by a seam in §4 except the
   ~7 lines in Finding F-2. If F1's ledger records a partial irreducible remainder, Finding F-2 is the
   evidence to cite for it.
2. **Measurement mechanism.** F1's per-file coverage report harness (derived from the Cobertura output
   of `scripts\vscode\Invoke-MSTestWithCoverage.ps1`) is the only accepted evidence mechanism. The
   numeric per-file result is committed under
   `docs/features/active/2026-08-07-quickfiler-qfc-form-explorer-controller-coverage-435/evidence/qa-gates/`.
   Aggregate `QuickFiler.dll` coverage does not satisfy the acceptance criterion (epic Shared Design §6).
3. **Policy reconciliation and seam conventions.** F1 settles refactor-first-exempt-the-remainder and
   the seam hierarchy; §4 is written to that convention.

---

## 8. Open questions / findings

- **OQ-1 — Seam ownership and de-duplication.** `IUiDispatcher` (A7) is shared with
  `QfcFormController.EventHandlers.cs`, which also needs a *notification* message seam
  (`ShowMessage(text, caption, buttons, icon)`) distinct from this file's *prompt* seam
  (`PromptUser(text, caption, buttons) -> DialogResult`). Declare each exactly once in
  `QfcFormController.cs` and do not merge the two message seams — merging would force the
  notification sites to consume a `DialogResult` they currently ignore.
- **OQ-2 — Routing of `MessageBox.Show("Nothing to undo")` (L248).** §4 routes it through `PromptUser`
  with an empty caption and `MessageBoxButtons.OK`, which is behaviorally identical to the
  one-argument overload. The alternative is a third one-argument `ShowNotice(string)` seam. Decide
  which, and be consistent.
- **OQ-3 — Two-delegate timing seam vs `TimeProvider`.** A4 proposes `DelayAsync` +
  `ElapsedMilliseconds`. `.claude/rules/csharp.md` § Time seam prefers injecting `System.TimeProvider`
  for new/touched time-dependent code, and `Microsoft.Bcl.TimeProvider` is available in the repo. The
  two-delegate form is recommended for minimal diff and because the loop's dominant cost is a fixed
  `Task.Delay`, not a clock read. Confirm the choice before planning.
- **OQ-4 — Source-text test brittleness.** `QfcFormControllerSeamTests.cs` L352-374 reads this
  production file from disk and string-matches two exact method signatures. It also performs
  filesystem I/O inside a unit test, which sits uneasily with the no-external-dependency rule.
  Proposed case 36 supplies the equivalent *behavioral* assertion. Decide whether F6 replaces the
  source-text test with case 36 (recommended, and it removes the constraint in §4.9) or keeps both.
  Either way, §4.9's constraints bind until the source-text test is removed.
- **OQ-5 — Guard-operand test granularity.** §5 enumerates all six operands for each of the three
  6-operand guards (18 cases, 5-13 / 14-19 / 26-31). Three of those operands (`_globals`,
  `_formViewer`, `_parent`) can only be nulled by reflection or by first calling `Cleanup()`.
  Reflection (`SetPrivateField`, already used at `QfcFormControllerTests.cs` L44-53) is recommended so
  each case attributes a single operand. Confirm the planner wants all 18 rather than a
  `Cleanup()`-based composite.
- **Finding F-1 — One existing test executes zero production lines and one is tautological.**
  `LoadItemsAsync_MailItemPath_DoesNotApplyPostDisplayHighConfidenceRemoval`
  (`QfcFormControllerSeamTests.cs` L352) is source-text inspection only, and
  `UndoConsumer_ShouldConsumeUndoQueue` (`QfcFormControllerTests.cs` L687) is a documented tautological
  placeholder with a narrow `MSTEST0032` suppression. Neither contributes coverage to this file. Cases
  36 and 49-53 respectively replace their intent with real behavioral assertions.
- **Finding F-2 — `UndoConsumer` cannot terminate once its timeout branch fires (latent defect;
  record, do not fix).** The loop condition at L258 is
  `while (!_undoQueue.IsCompleted || exit)`. `exit` is set true only at L281, inside
  `else if (sw.ElapsedMilliseconds > 10000)`. Once `exit` is true the disjunction is permanently true,
  so the loop re-enters forever, `TryTake` keeps failing, the elapsed check keeps passing, and no
  `await` is reached — a busy spin at 100% of a thread. Consequently the post-loop
  `if (exit) { _undoConsumerTask = null; }` at L288-291 is **unreachable in any terminating
  execution**, and `_undoConsumerTask` is never reset, so `UndoDialog`'s `??=` never restarts a
  consumer. The condition was almost certainly intended to be `&&`. Changing it is a behavior change
  and is **out of F6 scope**; per repository practice this should be promoted to its own GitHub issue.
  Two consequences bind the plan: (a) the A3 `StartBackground` seam is mandatory, not optional, or
  every `UndoDialog` test leaks a spinning thread; (b) the ~7 lines at L279-282 and L288-291 are the
  file's only claimed irreducible remainder.
- **Finding F-3 — `ApplyHighConfidenceFilterAsync` is dormant but fully covered.** Its XML doc
  (L166-170) states issue #233 enforces confidence at dequeue time instead. It remains executable code
  in the denominator; no exemption is warranted and none is needed, since four existing tests already
  cover both guard operands and both mode arms.
- **Finding F-4 — `MaximizeFormViewer` and `MinimizeFormViewer` need no seam.**
  `IQfcFormViewer` inherits `object Invoke(Delegate method)` through `IForm : IContainerControl,
  IScrollableControl` → `IControl` (`UtilitiesCS/Interfaces/IWinForm/IControl.cs` L176). A Moq'd
  viewer satisfies it, and the existing tests execute the inner delegate via a `Callback<Delegate>`
  that calls `DynamicInvoke()`, then assert the `WindowState` transition with `SetupSet`. This answers
  the question of how a test asserts the state change when a mock does not run the delegate: here the
  mock *is* configured to run it, so the assertion is direct.
- **Finding F-5 — Two distinct `IQfcFormController` interfaces exist.**
  `QuickFiler/Controllers/IQfcFormController.cs` (43 lines) is the one this class implements and it
  declares all six `LoadItems`/`LoadItemsAsync` overloads (L28-33) plus `Viewer_Activate` (L41).
  `QuickFiler/Interfaces/IQfcFormController.cs` (25 lines) declares `MaximizeQfcFormViewer` /
  `MinimizeQfcFormViewer` / `ButtonCancel_Click()` / `ButtonOK_Click()`, none of which
  `QfcFormController` implements — it appears to be dead. The issue calls for a recorded
  determination; both files are in F6's set but neither is this artifact's file, so the determination
  belongs to the interface-file researcher.
