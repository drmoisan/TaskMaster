# Research — Issue #810 (quickfiler-teardown-and-dropdown-residuals-793-808)

- **Timestamp:** 2026-09-07T22-10
- **Issue:** #810, work mode `full-bug`
- **Worktree:** `<repo-root>/.claude/worktrees/agent-ac28f83f99cbba6b5`
- **Branch:** `TaskMaster-wt-2026-09-06T17-16`
- **Method:** read-only inspection with the Read/Grep/Glob tools against the current working
  tree. **The Bash tool is disabled in this session** (`Error: No such tool available: Bash`), so no
  `git`, `msbuild`, `vstest`, or `csharpier` command was executed and no line count, diff, or build
  result in this artifact is derived from a command. Every line number below was re-derived by
  reading the file in this session.

## Scope

The issue enumerates seven defects (AC1–AC7) plus a toolchain criterion (AC8). This artifact answers
the eight research questions the delegation posed, in order, and then converges on one recommended
approach.

Two documents in this repository are the authoritative source-finding text and were both read in
full:

- `docs/features/active/2026-09-06-quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791/code-review.2026-09-06T15-31.md`
  — findings N1 (`_tokenSource` disposed but not nulled) and N2 (`_parentCleanup?.Invoke()` without
  `finally`).
- `docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/code-review.2026-09-07T17-05.md`
  — findings CR-1 (stale `FinishClose` comment), CR-2 (commit-pending latch never cleared on
  consumption), CR-3 (AC2 guard also gates the #791 Cancel teardown), CR-4 (dead
  `SearchOwnsDropDownDismissal`), CR-7 (AC2 producer untested).

**Correction to one statement in the delegation prompt.** The delegation places the dead
`SearchOwnsDropDownDismissal` accessor in `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` ("same
file/area"). It is not there. It is declared at
`QuickFiler/Controllers/QfcItemController.EventHandlers.cs:209`, which is a different file in a
different directory owned by a different type. This is consistent with CR-4, which cites
`QfcItemController.EventHandlers.cs line 209`. AC6 therefore spans two unrelated files, not one.

**State of the feature folder.** `docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/spec.md`
and `plan.2026-09-07T21-59.md` are both unmodified scaffolding templates — `spec.md` has no populated
Context, Root Cause, Proposed Fix, or Test Strategy section, and its Acceptance Criteria list is the
generic eight-item template, not the AC1–AC8 list the issue defines. The working acceptance criteria
for this research are therefore `issue.md:66-73`. `spec.md` must be populated before any numeric
acceptance criterion is asserted against it.

---

## Q1 — Callers of `ParkFocusAndCancelSelectors`, and the minimal way to scope the AC2 guard

### The method and the guard

`QuickFiler/Controllers/QfcFormController.Deactivate.cs:91` declares
`internal void ParkFocusAndCancelSelectors()`. Its body runs in four steps:

| Lines | Step |
|---|---|
| `:93-99` | Emit the AC6 entry diagnostic via `FormatDeactivationDiagnostics`. |
| `:100-103` | If `_formViewer?.IsWebView2Focused == true`, call `_formViewer.ParkFocusOffWebView2()`. |
| `:105-109` | Snapshot `_groups?.ItemGroups` into `groups`; return when null. |
| `:111-121` | **The AC2 guard.** `if (_formViewer?.IsDeactivationSelfInflictedByOwnPopup == true) { return; }`, carrying the `Issue #796 (AC2)` comment block at `:111-117`. |
| `:123-147` | The per-item cancel loop, with a per-item boundary `catch` at `:135-146`. |

### Complete caller set

Two production call sites. No test invokes the method directly. See
`## Numeric Derivation Evidence`, family 1, for the exhaustive derivation.

| # | Call site | Reached from | Is the self-inflicted predicate meaningful here? |
|---|---|---|---|
| 1 | `QuickFiler/Controllers/QfcFormController.Deactivate.cs:27` — `internal void FormViewer_Deactivated(object sender, EventArgs e) => ParkFocusAndCancelSelectors();` | The `IQfcFormViewer.FormDeactivated` event, subscribed at `QfcFormController.SetupDisposal.cs:175` and unsubscribed at `:204`. That event is the `Form.Deactivate` routing. | **Yes.** The caller is handling a deactivation, so "was this deactivation caused by our own popup?" is exactly the question being asked. |
| 2 | `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:144` — `RunTeardownStage("park-focus", ParkFocusAndCancelSelectors);` | The `park-focus` stage of `ActionCancelAsync` (the ordered #791 Cancel teardown). | **No.** There is no deactivation event at all on this path. The predicate degenerates to "is any breadcrumb popup open?", and if one is, the teardown's synchronous selector-cancel stage is skipped entirely. This is CR-3. |

### Why the defect matters (and why it is latent)

The class-level documentation at `QfcFormController.Deactivate.cs:12-14` states the invariant the
skipped loop enforces: "no breadcrumb `ToolStripDropDown` may stay open, or WinForms modal menu mode
keeps redirecting thread keyboard messages to the popup after the user has left."

CR-3 traced the mitigation chain and this researcher confirmed each link exists at the cited
coordinates: `ActionCancelAsync` → `groups-cleanup` (`EventHandlers.cs:166`) →
`QfcCollectionController.Cleanup()` → `RemoveControls()` → per-item `ItemController.Cleanup()` →
`(_itemViewer as ItemViewer)?.ResetBreadcrumb()` → lifecycle `Reset()` →
`BreadcrumbDropDownOpenCoordinator.Reset()` (`QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:193-207`),
which posts `if ((!_host.IsOpen || !_host.Close(Uncommitted)) && _isSelectorOpen()) _cancelSelector();`
through `_operations.PostAsync`. So the popup **is** closed during Cancel — later, through a
different path, and via a fire-and-forget post rather than the synchronous, ordered stage #791
specified. The net effect is a weakened ordering guarantee, not a lost responsibility.

### Candidate designs for narrowing the guard

**Design A — boolean parameter on the existing method, supplied by both callers.**

```
internal void ParkFocusAndCancelSelectors(bool honourSelfInflictedGuard)
```
with `Deactivate.cs:27` becoming `ParkFocusAndCancelSelectors(honourSelfInflictedGuard: true)` and
`EventHandlers.cs:144` becoming
`RunTeardownStage("park-focus", () => ParkFocusAndCancelSelectors(honourSelfInflictedGuard: false))`.
The guard at `:118` becomes `if (honourSelfInflictedGuard && _formViewer?.IsDeactivationSelfInflictedByOwnPopup == true)`.

- Advantages: one parameter, one added conjunct, two call-site edits, zero new types and zero new
  members. Named arguments make each call site self-documenting. This is the shape CR-3 itself
  proposed (`code-review.2026-09-07T17-05.md:205-207`), so the audit trail from finding to fix is
  direct. The predicate's polarity, its placement below the focus-parking step, and its behavior
  under `FormViewer_Deactivated` are all untouched.
- Limitation: a `bool` parameter is a mode flag. Mitigated by there being exactly two callers, both
  inside the same type, both passing a named literal.
- Repo alignment: matches CLAUDE.md "Simplicity first — prefer the simplest design that works and is
  easy to read. Avoid cleverness and deep indirection."

**A material compiler constraint that shapes this design.** The parameter must be *required*, not
defaulted, **or** the teardown call site must be wrapped in a lambda. `EventHandlers.cs:144` passes
`ParkFocusAndCancelSelectors` as a **method group** converted to `System.Action`. C# method-group
conversion does not apply optional-argument defaults, so
`internal void ParkFocusAndCancelSelectors(bool honourSelfInflictedGuard = true)` would break that
conversion (CS0123). Recommending a required parameter plus an explicit lambda at the teardown call
site removes the hazard entirely and forces both callers to state their intent. This is a
verified-by-reading constraint on the source, and it is the single most likely way an implementer
gets this task wrong on the first attempt.

**Design B — two methods over a shared private core.** Keep `ParkFocusAndCancelSelectors()` for the
deactivation path and add e.g. `ParkFocusAndCancelSelectorsForTeardown()`, both delegating to a
private `ParkFocusAndCancelSelectorsCore(bool honourSelfInflictedGuard)`.

- Advantage: preserves the zero-arg method group at `EventHandlers.cs:144` verbatim, so that line
  needs no edit at all.
- Limitation: three members where one suffices; two public-ish names to keep in sync; the XML doc
  block and the `<see cref="ParkFocusAndCancelSelectors"/>` references at `:23`, `:31`, `:62` all
  need re-pointing. `QfcFormController.Deactivate.cs` currently holds 150 lines, so headroom is not
  the constraint — clarity is.

**Design C — caller-supplied enum** (e.g. `SelectorCancelTrigger.FormDeactivation` /
`.CancelTeardown`). Rejected: introduces a new internal type to express a two-state decision with
two total call sites, and every consumer would still branch on it with a single `if`. It buys
extensibility that no third caller is anticipated to need.

### Recommendation for Q1

**Design A with a required parameter and a lambda at the teardown call site.** It is the smallest
change that makes the predicate's applicability explicit at both call sites, it does not add a
member or a type, and it is the shape the originating review already ratified.

---

## Q2 — The ordered Cancel teardown sequence

Defined in `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:126-173`, method
`public async Task ActionCancelAsync()`. Every stage except the two marked below runs through
`RunTeardownStage(string stage, System.Action body)` (`:26-37`), which logs completion at DEBUG and
any escaping exception at ERROR with the stage name, so a throwing stage cannot skip a later one.

| Order | Line | Stage name | Body |
|---|---|---|---|
| 1 | `:128-129` | *(no stage name)* | Read `_parent?.TokenSource?.IsCancellationRequested`, log `Cancel teardown starting. AlreadyCancelled=…` at INFO. |
| 2 | `:133` | `cancel-token` | `_parent?.TokenSource?.Cancel()`. Deliberately **ahead of the first await** so the seam test raising `CancelClicked` observes the token cancelled by the time `Mock.Raise` returns. |
| 3 | `:137-141` | *(no stage name)* | `await uiContext` when `_formViewer?.UiSyncContext` is non-null — marshal to the UI context. |
| 4 | `:143` | `reset-keyboard` | `ResetKeyboardActive` (`:40-49`) — toggles the keyboard dialog only when it is active. |
| 5 | **`:144`** | **`park-focus`** | **`ParkFocusAndCancelSelectors`** — the stage this issue's AC1 targets. |
| 6 | `:145` | `unregister-handlers` | `UnregisterCancelPathHandlers` (`:52-56`) = `_groups?.UnregisterNavigation()` then `UnregisterFormEventHandlers()`. |
| 7 | `:146` | `hide-form` | `_formViewer?.Hide()`. |
| 8 | `:150-153` | `quiesce-loader` | Captures `quiesce = _parent?.DataModel?.QuiesceLoaderAsync(LoaderQuiesceBound)`. `LoaderQuiesceBound` is `TimeSpan.FromSeconds(5)` (`:23`). |
| 9 | `:155-164` | `quiesce-await` (error label only) | `await quiesce` when non-null, inside its own `try`/`catch` logging `Stage=quiesce-await`. |
| 10 | `:166` | `groups-cleanup` | `_groups?.Cleanup()`. |
| 11 | `:168-172` | `controller-cleanup` | Inside `finally`: `RunTeardownStage("controller-cleanup", Cleanup)` then an unconditional INFO line. |

### The ordering guarantee issue #791 established

Two ordering constraints are separately asserted by
`QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs`:

- `ActionCancelAsync_UnregistersHandlersBeforeGroupsCleanup` (`:213-238`) — navigation and form
  keyboard handlers are unregistered **before** the item rows are removed, because reversed, the
  recursive unsubscribe no longer reaches the item controls' `PreviewKeyDown`/`KeyDown`
  subscriptions.
- `ActionCancelAsync_AwaitsLoaderQuiesceBeforeGroupsCleanup` (`:247-270`) — the background loader is
  quiesced before any datamodel field is nulled, and a completed quiesce does not short-circuit the
  remaining stages.

Plus the exception-safety guarantee: `ActionCancelAsync_GroupsCleanupThrows_StillInvokesParentCleanup`
(`:278-299`) and `ActionCancelAsync_CalledTwice_InvokesParentCleanupOnce` (`:350-366`).

The `park-focus` stage's placement at position 5 — after the keyboard reset and **before** handler
unregistration, hide, and row removal — is what makes it the synchronous, ordered point at which a
popup is guaranteed gone. AC1's defect is that this stage currently no-ops whenever a popup is open,
which is precisely the case the stage exists to handle.

### An existing test already covers most of the AC1 surface

`ActionCancelAsync_ParksFocusAndCancelsBreadcrumbSelectors`
(`QfcFormControllerCancelTeardownTests.cs:188-204`) already asserts both item controllers get
`CancelBreadcrumbSelector()` on the Cancel path. It passes today only because
`_mockFormViewer` is a loose `Mock<IQfcFormViewer>` and Moq's default `bool` return is `false`. The
RED-first regression test for AC1 is therefore a near-clone with one added Arrange line:
`_mockFormViewer.SetupGet(x => x.IsDeactivationSelfInflictedByOwnPopup).Returns(true);`. That test
fails before the fix (zero cancels) and passes after.

---

## Q3 — `_tokenSource` in `QfcHomeController`: lifetime, sharers, and minimal fix

### Origin and every assignment

| Site | Statement |
|---|---|
| `QuickFiler/Controllers/QfcHomeController.cs:54` | `var tokenSource = new CancellationTokenSource();` inside `LaunchAsync`. This is the single production instance for a QuickFiler session. |
| `:117` | `_tokenSource = tokenSource;` inside `InitAsync`. |
| `:465` | `_tokenSource = new CancellationTokenSource();` inside `internal void CreateCancellationToken()`. |

### Every reader / handoff of the same instance

| Site | Consumer |
|---|---|
| `:56` | `new ProgressTracker(tokenSource)` → `UtilitiesCS/Threading/ProgressTracker.cs:22` `_cancelSource = tokenSource`. |
| `:102` | `Init()` passes `this._tokenSource` to `QfcFormControllerLoader`. |
| `:122-127` | `InitAsync` passes `this.TokenSource` to `QfcAsyncDataModelLoader` → `QfcDatamodel.LoadAsync(...)` → `model.TokenSource = tokenSource` (`QfcDatamodel.cs:66`). |
| `:144` | `InitAsync` passes `TokenSource` to `QfcFormControllerLoader` → `QfcFormController.cs:39` `_tokenSource = tokenSource`. |
| `:285` | `RunAsync` passes `TokenSource` to `_datamodel.InitEmailQueueAsync(...)`. |
| `:470-473` | `public CancellationTokenSource TokenSource { get => _tokenSource; }` — the interface member `IFilerHomeController.TokenSource` (`QuickFiler/Interfaces/IFilerHomeController.cs:24`). |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs:386` | `_tokenSource = _homeController.TokenSource;` — every item controller holds the same instance. |
| `QuickFiler/Controllers/QfcCollectionController.cs:42` | `_tokenSource = tokenSource;`. |

### Every `Cancel()` on the shared instance

| Site | Context |
|---|---|
| `QuickFiler/Controllers/QfcDatamodel.cs:77` | First statement of `QfcDatamodel.Cleanup()`. |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:50` | First statement of `QfcDatamodel.QuiesceLoaderAsync(TimeSpan)`. |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:133` | The `cancel-token` teardown stage, `_parent?.TokenSource?.Cancel()`. |
| `UtilitiesCS/Threading/ProgressViewer.cs:75` | `_cancelSource!.Cancel()` on the progress dialog's Cancel button. **A fourth sharer the #791 review did not name**, and the only one driven by a user gesture rather than by teardown order. |

### The only `Dispose()`

`QuickFiler/Controllers/QfcHomeController.cs:389`, inside the second guarded block of
`Cleanup()` (`:386-400`). The block nulls `Globals`, `_formViewer`, `_explorerController`,
`_formController` and `_keyboardHandler` (`:390-394`) but leaves both `_tokenSource` **and**
`_datamodel` non-null. On .NET Framework 4.8, `CancellationTokenSource.Cancel()` after `Dispose()`
raises `ObjectDisposedException`.

### Reachability today

`QfcFormController.Cleanup()` invokes `_parentCleanup` (which *is* `QfcHomeController.Cleanup`, wired
at `QfcHomeController.cs:100` and `:142`) then nulls it (`SetupDisposal.cs:259-260`), and
`RibbonController` never calls `QfcHomeController.Cleanup()` directly — it only supplies
`ReleaseQuickFiler` as the callback (`TaskMaster/Ribbon/RibbonController.cs:106,120,141,148`). So a
second `QfcHomeController.Cleanup()` is not reachable through the normal chain today, which is what
keeps N1 latent. Two things weaken that:

1. `_datamodel` is **not** nulled by `Cleanup()`, so a second pass would call `_datamodel.Cleanup()`
   → `_tokenSource?.Cancel()` on the datamodel's copy of the same disposed instance.
2. `ProgressViewer`'s Cancel button holds the same instance and is not part of any teardown ordering.

### Minimal fix and race analysis

**Recommended: `_tokenSource = null;` on the line immediately after `_tokenSource?.Dispose();`**, i.e.
between `:389` and `:390`. This is exactly N1's recommendation and it makes the field consistent with
the five siblings around it.

- Does nulling introduce a race with another thread? **No new one.** `_tokenSource` is a plain
  (non-`volatile`) reference field already written on the UI thread at `:117` and read from other
  threads via the `TokenSource` property (for example `RunAsync`'s `Task.Run` closure at `:280-287`).
  A concurrent reader could observe `null` where it previously observed a disposed instance —
  strictly better, because every consumer of the property either passes it to a constructor or
  guards it with `?.` (`QfcDatamodel.cs:77`, `QfcDatamodel.QueueProcessing.cs:50`,
  `QfcFormController.EventHandlers.cs:133` all use `?.Cancel()`). The one consumer that does **not**
  null-guard is `ProgressViewer.cs:75` (`_cancelSource!.Cancel()`), but it holds its own captured
  reference assigned at construction; nulling the home controller's field does not affect it.
- Do **not** mark the field `volatile`. Prior work on this repository recorded that adding `volatile`
  to a field of this shape produced a CS0420 build break at a `ref`/`out`-style use site; there is no
  evidence a memory barrier is required here, and adding one is not minimal.
- A "different lifetime" alternative — making `QfcHomeController` the sole owner and having every
  sharer take a `CancellationToken` instead of the source — is a real improvement but is a
  multi-file API change across `QfcDatamodel`, `QfcFormController`, `QfcCollectionController`,
  `QfcItemController` and `ProgressTracker`. Out of scope; report only.

**Also worth doing in the same block, and cheap:** null `_datamodel` alongside the other five fields,
so a repeat `Cleanup()` cannot re-enter `QfcDatamodel.Cleanup()` at all. This is a one-line addition
that removes the only in-repo path by which a disposed source is reached after nulling. It is
strictly within AC3's wording ("no longer leaves a disposed shared `CancellationTokenSource`
reachable by later `Cancel()` callers") and should be treated as part of AC3, not as scope creep.

**File-size constraint.** `QuickFiler/Controllers/QfcHomeController.cs` is 496 lines as read in this
session. The `.claude/rules/general-code-change.md` ceiling is 500. Two added statements plus a short
`// why` comment lands at roughly 499–500. **This is the tightest constraint in the whole change
set.** If a comment is wanted, put the rationale on the existing
`/// <summary>Issue #791. Two guarded blocks under one <c>finally</c> …</summary>` doc at `:370`
rather than adding new comment lines inside the method.

---

## Q4 — `QfcFormController.Cleanup()`: statement order, throw sites, and the `finally` restructure

`QuickFiler/Controllers/QfcFormController.SetupDisposal.cs:213-261`. Exact current order:

| Line | Statement | Can it throw? |
|---|---|---|
| `:215-218` | `if (_globals?.Ol is not null) { _globals.Ol.PropertyChanged -= DarkMode_CheckedChanged; }` | Yes — an event-remove on a COM-backed `Ol` object. |
| `:220` | `UnregisterFormEventHandlers();` | **Yes.** It reaches `_formViewer.Controls.ForAllControls(...)` (`:185-197`) and `_formViewer.GetKeyEventExclusionControls()` (`:196`), both on a live viewer. |
| `:221-222` | `var undoQueue = _undoQueue; var undoConsumer = _undoConsumerTask;` | No. |
| `:223-230` | `try { undoQueue?.CompleteAdding(); } catch (ObjectDisposedException) { }` | Already guarded. |
| `:231-249` | Either dispose the queue now or defer it onto a `TaskScheduler.Default` continuation on the undo consumer. | Practically no. |
| `:250` | `_globals = null;` | No. |
| `:251` | `_formViewer?.Dispose();` | **Yes.** `IQfcFormViewer : IForm` reaches `IDisposable` (`UtilitiesCS/Interfaces/IWinForm/IForm.cs:8` → `IScrollableControl` → `IControl : … IDisposable`). Disposing a real `QfcFormViewer` disposes WebView2 child controls. |
| `:252-258` | `_formViewer = null; _groups = null; _rowStyleTemplate = null; _parent = null; _movedItems = null; WriteMetrics = null; Iterate = null;` | No. |
| `:259` | `_parentCleanup?.Invoke();` | Yes (the callback body). |
| `:260` | `_parentCleanup = null;` | No. |

A throw from `:215`, `:220`, or `:251` skips `:259` entirely, and the ribbon buttons stay inert for
the rest of the Outlook session. This is N2.

### Is "exactly once" already latched?

**Partly.** `_parentCleanup = null;` at `:260` is a real latch, so a *successful* first pass makes a
second pass inert — which is what `ActionCancelAsync_CalledTwice_InvokesParentCleanupOnce`
(`QfcFormControllerCancelTeardownTests.cs:350-366`) pins. But the latch has two holes:

1. If an earlier statement throws, neither `:259` nor `:260` runs, so a later `Cleanup()` invokes the
   callback for the first time — correct, but only by accident of the caller retrying.
2. If `_parentCleanup?.Invoke()` itself throws, `:260` never runs, so a later `Cleanup()` invokes it
   a **second** time. `RunTeardownStage("controller-cleanup", Cleanup)` swallows the first throw, so
   this is reachable.

### Minimal restructure

Wrap `:215-258` in a `try` and place a read-and-clear in the `finally`:

```
finally
{
    System.Action parentCleanup = _parentCleanup;
    _parentCleanup = null;
    parentCleanup?.Invoke();
}
```

Reading the field into a local and clearing it **before** invoking is what makes "exactly once"
unconditional: it holds even when the callback throws, and the throw then propagates out of
`Cleanup()` into `RunTeardownStage`'s ERROR log rather than into the Outlook UI thread. A plain
`finally { _parentCleanup?.Invoke(); _parentCleanup = null; }` closes hole 1 but not hole 2.

### Constraint on this edit

`QfcFormControllerCleanupTests.Cleanup_SourceContainsNoSynchronousWait`
(`QuickFiler.Test/Controllers/QfcFormControllerCleanupTests.cs:377-397`) reads the whole text of
`QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` (`:105-130`) and asserts it contains none
of `.Wait(`, `.Result`, `Thread.Sleep`, `Task.Delay` after whitespace normalization. The proposed
restructure introduces none of the four. No other test scans this file.

`SetupDisposal.cs` is 265 lines; a `try {` / `}` pair, three `finally` lines and a short comment
leave it comfortably under the ceiling.

---

## Q5 — `BreadcrumbDropDownHost`: the commit-pending latch and every path to `FinishClose`

### Every touch point of `IsCommitPending`

Exhaustively enumerated in `## Numeric Derivation Evidence`, family 3. Four production sites:

| Site | Role |
|---|---|
| `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs:109` | Declaration: `internal bool IsCommitPending { get; set; }`, with the lifetime contract documented at `:91-108` ("The lifetime is one popup opening: `ShowPopup` clears it as each fresh native show begins, and nothing else clears it"). |
| `BreadcrumbDropDownHost.Open.cs:123` | **Clear**, first statement of `internal void ShowPopup(Point location, bool takeFocus)`. |
| `BreadcrumbDropDownHost.cs:255-256` | **Set**, inside `public bool Close(BreadcrumbDropDownCloseReason reason)`: `if (reason == …ExplicitCommit) IsCommitPending = true;`. |
| `BreadcrumbDropDownHost.cs:447` | **Read**, inside `FinishClose`: `if (reason == …Uncommitted && !IsCommitPending) _cancelSelection();`. |

Three test sites, all in `QuickFiler.Test/Viewers/`: `BreadcrumbPendingOpenCloseTests.cs:141` (set),
`BreadcrumbDropDownCloseOrderingTests.cs:80` (set) and `:105` (read, asserting a fresh show cleared
it).

### Every path that reaches `FinishClose`

`FinishClose(BreadcrumbDropDownCloseReason reason)` is declared at `BreadcrumbDropDownHost.cs:432`.
Three call sites, and the in-code comment at `:436-438` independently names the same three:

| # | Call site | Guard on the way in |
|---|---|---|
| 1 | `BreadcrumbDropDownHost.cs:415`, inside `private void CompleteClose(reason, closeNative)` (`:403-417`) | `CompleteClose` early-returns at `:405-406` when `!OpenState && !_openLifetime.IsPendingClose`. `CompleteClose` is itself reached from `Close()` (`:259`, `:262`), `ResetCoreAsync` (`:317`, guarded by `if (OpenState)`), and `DisposeCoreAsync` (`:339`, guarded by `if (OpenState && !_resetPending)`). |
| 2 | `BreadcrumbDropDownHost.Diagnostics.cs:75`, inside `OnDropDownClosed` | Guarded twice: `:68-69` and again inside the scheduled body at `:72-73`, both `if (_disposed \|\| _programmaticClose \|\| !OpenState) return;`. |
| 3 | `BreadcrumbDropDownHost.cs:465`, inside `internal void RestoreAfterOpenFailure()` (`:455-467`) | **Unconditional.** The method sets `OpenState = false` at `:458` and calls `FinishClose(Uncommitted)` at `:465` with no state test. Reached from `BreadcrumbDropDownOpenLifetime.HandleOpenFailureAsync` (`QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs:376`), which is reached from the `catch` wrapping the whole open sequence (`:251-255`). |

### The CR-2 defect, restated against the current tree

The documented "one popup opening" lifetime holds **only when a native show actually begins**.
`ShowPopup` is the sole clear site, and it is called from
`BreadcrumbDropDownOpenLifetime.ShowCurrentSurface` at `:276`, i.e. after
`_host.OpenState = true` at `:268` and after placement succeeded. An open that throws anywhere in
`OpenCoreAsync` before `:276` — surface creation, placement, `ValidatePlacement`'s
`InvalidOperationException` at `:283-286` — routes to `HandleOpenFailureAsync` →
`RestoreAfterOpenFailure` → `FinishClose(Uncommitted)` while the latch still carries the **previous**
popup lifetime's committed state. The cancel is suppressed and a selector session is left open with
no popup.

### Where exactly must the latch be cleared

**At the end of `FinishClose`, unconditionally, as the last operation.** That is the only single
point through which all three paths pass, and it makes the invariant "the latch does not survive any
close, on any path" a structural property rather than a per-path obligation.

The specific reason to prefer `FinishClose` over CR-2's alternative of clearing it inside
`RestoreAfterOpenFailure`: clearing in `RestoreAfterOpenFailure` fixes only the one path that is
known to be broken today, leaving the invariant restated as a rule two other call sites must
independently honour. Clearing in `FinishClose` also strictly preserves the existing behavior of
paths 1 and 2, because on those paths the latch's only consumer is the `if` at `:447`, evaluated
earlier in the same `CompleteAll` call.

Mechanically, `FinishClose` is a `CompleteAll(params Action[])` call (`:434-452`); the clear should be
a fourth operation after `FocusAnchorIfPermitted`, or a plain statement after the `CompleteAll` call.
A plain statement after `CompleteAll` would **not** run when an earlier operation throws, because
`CompleteAll` rethrows the first failure at `:486-487`. Therefore the clear must be **inside**
`CompleteAll`'s operation list, not after it.

### The 500-line constraint on `BreadcrumbDropDownHost.cs`

`BreadcrumbDropDownHost.cs` is 496 lines as read in this session — 4 lines of headroom against the
500-line ceiling in `.claude/rules/general-code-change.md`. AC5 (a fourth `CompleteAll` operation plus
its `// why` comment) and AC6 (replacing the stale comment at `:450` with the corrected two-clause
text) together will exceed it.

**Recommendation: relocate `FinishClose` and `RestoreAfterOpenFailure` from
`BreadcrumbDropDownHost.cs` into `BreadcrumbDropDownHost.Open.cs`** (131 lines), which already
declares `IsCommitPending` (`:109`) and `ShowPopup` (`:118`). That co-locates the entire latch
lifetime — declare, clear-on-show, read-and-clear-on-close — in one file, and it is the same
technique the #796 change itself used ("The 500-line ceiling was managed by relocation rather than by
exception", `code-review.2026-09-07T17-05.md:283-285`). Relocation is safe: no test resolves either
method by file path. The one file-anchored assertion in this area is
`BreadcrumbDropDownCloseOrderingTests.cs:228-232`, whose `because:` string names
`BreadcrumbDropDownHost.Diagnostics.cs` for `OnDropDownClosed` — a different method, not being moved,
and the assertion itself is `handler.Should().NotBeNull()` on a reflection lookup against the type,
which is file-agnostic. Verified: `QuickFiler.Test` contains no `File.ReadAllText` against any
`Viewers/` production path (see Q8 search records).

If relocation is rejected, the fallback is to move only `FinishClose`; moving nothing is not viable
within the ceiling.

### AC6, part 1 — the stale comment

`BreadcrumbDropDownHost.cs:450` currently reads:

```
// Issue #677: only the focus step is gated; the cancel step above always runs.
```

The cancel step above stopped always running at `:447`. CR-1's suggested replacement is accurate and
should be adopted substantially as written: "Issue #677 / #796: the focus step is gated on the
may-take-focus predicate and the cancel step above is gated on the pending-commit latch; the two
gates are independent." Under the AC5 fix the comment should also record that the latch is cleared as
the final operation of this method.

### Regression test for AC5

`BreadcrumbDropDownCloseOrderingTests.CloseOrderingHostHarness` (`:159-266`) is already exactly the
harness needed and requires no new fixture: it drives a real `BreadcrumbDropDownHost` under an
`InlineSynchronizationContext` with a panel surface, a stub messenger, and counting delegates for
`_focusPending`, `_focusAnchor`, `_cancelSelection` and `showPopup`. The RED-first test is:

1. `harness.OpenAndSettle()` — `ShowPopup` clears the latch.
2. `harness.Host.IsCommitPending = true` — simulate the committed close.
3. `harness.Host.RestoreAfterOpenFailure()` — `internal`, directly callable from `QuickFiler.Test`
   via the assembly-level `InternalsVisibleTo("QuickFiler.Test")` at `QfcHomeController.cs:15`.
4. Assert `harness.CancelCount == 1` (0 before the fix) **and** `harness.Host.IsCommitPending == false`.

A second test should assert the latch is cleared on the ordinary paths too — extend
`NativeCloseWhileCommitPending_DoesNotCancelSelection` (`:74-90`) with
`harness.Host.IsCommitPending.Should().BeFalse()` after the act, which proves the clear is on every
path and not just the failure path. That file is 290 lines, with ample headroom.

---

## Q6 — Is `SearchOwnsDropDownDismissal` genuinely dead?

**Yes.** Declared at `QuickFiler/Controllers/QfcItemController.EventHandlers.cs:209`:

```csharp
internal bool SearchOwnsDropDownDismissal => _searchOwnedDismissal;
```

Full derivation in `## Numeric Derivation Evidence`, family 2. Summary: repository-wide, the exact
identifier occurs 11 times across 7 files. Exactly **one** of those is source code — the declaration
above. The other 10 are inside `docs/features/**` markdown (this issue's `issue.md`, and the #796
policy audit, feature audit, code review, and two evidence files). Zero occurrences in
`QuickFiler.Test/`, zero in any `.Designer.cs`, `.resx`, `.csproj`, or XML file, and zero reflective
references by string name (a `GetProperty(` / `GetMember(` / `GetMethod("get_` sweep across the whole
test tree returns 29 hits, none naming this member).

Corroborating independent evidence already in the repository:
`docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/qa-gates/p9-t7-coverage-delta.md:184`
records the per-line hit counts for this file and shows `209(0)` — the accessor's line was executed
zero times across the entire `QuickFiler.Test` run, while every sibling latch line (183, 220, 234,
254, 257, 263) shows `(1)`.

**Deleting it is safe and does not create a new warning.** The backing field `_searchOwnedDismissal`
(`:203`) remains fully live: written at `:183`, `:220`, `:234`, `:257`, `:265` and read at `:263`. It
therefore cannot regress to CS0649 ("never assigned") or CS0169 ("never used"). The #796 evidence
file `p6-t1-ac4-seam.md:65` records that CS0649 *was* observed on this field at an intermediate commit
of that branch, before the producers landed — that condition no longer exists.

**Rejected alternative** (CR-6's option): instead of deleting, give the accessor a reader by
rewriting `QfcItemController.SearchDismissalTests.cs:85` to drive the real
`TextBoxSearch_TextChanged` path rather than
`QfcItemControllerTestSupport.SetField(controller, "_searchOwnedDismissal", true)`. That is a better
end state — it removes a string-literal coupling to a private field name — but it changes a
deliberate re-pin test on a file this issue does not otherwise touch, and AC6 asks for removal. Report
as a follow-up.

---

## Q7 — Testing: project, fixtures, seams, and how to test the AC2 producer headlessly

### Test project and build constraint

`QuickFiler.Test/QuickFiler.Test.csproj` — a **legacy non-SDK project with explicit
`<Compile Include>` items** (verified: `Viewers\BreadcrumbDropDownCloseOrderingTests.cs` at `:83`,
`Controllers\QfcFormControllerCleanupTests.cs` at `:157`,
`Controllers\QfcFormControllerDeactivateTests.cs` at `:158`,
`Controllers\QfcFormControllerCancelTeardownTests.cs` at `:173`). **Every new test file requires a
matching `<Compile Include>` entry or it silently does not compile.** The same is true of
`QuickFiler/QuickFiler.csproj` for any new production file (`Viewers\BreadcrumbDropDownHost.Open.cs`
at `:416`, `Viewers\BreadcrumbDropDownHost.Diagnostics.cs` at `:417`).

Internals are visible: `[assembly: InternalsVisibleTo("QuickFiler.Test")]` at
`QuickFiler/Controllers/QfcHomeController.cs:15`.

### Existing files that cover the neighbouring behavior

| File | Covers | Lines |
|---|---|---|
| `QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs` | The #677 contract and the #796 AC2 consumer. 9 tests. **This file is the AC2 regression fence and must stay green unmodified.** | 305 |
| `QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs` | The #791 ordered teardown, including `ActionCancelAsync_ParksFocusAndCancelsBreadcrumbSelectors` at `:188`. | 393 |
| `QuickFiler.Test/Controllers/QfcFormControllerCleanupTests.cs` | `QfcFormController.Cleanup()` (#731 finding 2), incl. the source-scan forward guard at `:377`. | 399 |
| `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs` | `QfcHomeController.Cleanup()` (#791 AC2), incl. `Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted` at `:79`. | 118 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs` | The #796 AC3 latch, with the reusable `CloseOrderingHostHarness`. | 290 |
| `QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs` | The #796 AC4 provenance latch (touches `_searchOwnedDismissal` by reflection at `:85`). | — |

### Injectable seams available

- **Controller layer:** `Mock<IQfcFormViewer>`, `Mock<IQfcHomeController>`, `Mock<IQfcDatamodel>`,
  `Mock<IQfcCollectionController>`, `Mock<IQfcItemController>`, `Mock<IQfcKeyboardHandler>`, plus
  private-field injection by reflection (`SetPrivateField`, established in all three controller
  suites). `new Control.ControlCollection(new Control())` + an empty exclusion list satisfies the
  guard at the top of `Register`/`UnregisterFormEventHandlers` **without creating a window handle** —
  this is the established headless idiom (`QfcFormControllerDeactivateTests.cs:49-54`).
- **Host layer:** the `BreadcrumbDropDownHost` constructor overload taking a `LegacySurfaceFactory`
  plus explicit `focusPending` / `focusAnchor` / `cancelSelection` / `showPopup` delegates
  (`BreadcrumbDropDownHost.cs:79-99`), driven under an `InlineSynchronizationContext`, with
  `FormatterServices.GetUninitializedObject(typeof(CoreWebView2Environment))` for the environment.
- **Determinism:** `FakeTimeProvider` (`Microsoft.Extensions.Time.Testing`), injected `Action<string>`
  log sinks (`QfcDatamodel.QuiesceLoaderAsync`'s `QuiesceDebugLog` at
  `QfcDatamodel.QueueProcessing.cs:35`), `TaskCompletionSource`, and the `CapturingSynchronizationContext`
  pattern at `QfcFormControllerCancelTeardownTests.cs:372-391` for observing `async void` escapes.

### AC7 — testing the AC2 producer without live Outlook and without handle creation

The producer is `QfcFormViewer.IsDeactivationSelfInflictedByOwnPopup`
(`QuickFiler/Viewers/QfcFormViewer.cs:245-246`):

```csharp
public bool IsDeactivationSelfInflictedByOwnPopup =>
    _breadcrumbPopupOwners.Values.Any(popupIsOpen => popupIsOpen());
```

fed by `internal void SetBreadcrumbPopupOwner(Control itemViewer, Func<bool> popupIsOpen)`
(`:226-234`) over `private readonly Dictionary<Control, Func<bool>> _breadcrumbPopupOwners`
(`:212-213`). The registration call is
`(FindForm() as QfcFormViewer)?.SetBreadcrumbPopupOwner(this, () => host.IsOpen);` at
`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:216`.

Both types carry class-level `[ExcludeFromCodeCoverage]` — `QfcFormViewer.cs:17`,
`ItemViewer.cs:20` — which is what CR-7 records. `QfcFormViewer` is
`public partial class QfcFormViewer : Form, IQfcFormViewer` in namespace `QuickFiler` (not
`QuickFiler.Viewers`, despite the folder).

**Recommended: extract the registry into a host-neutral type.** A new
`QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs` holding the dictionary, a `Register(Control,
Func<bool>)` method with the existing null-ignore guard, and an `AnyOpen` property carrying the
`Any(...)` derivation. `QfcFormViewer` keeps one `private readonly BreadcrumbPopupOwnerRegistry`
field and two forwarding one-liners. This:

- Makes the derivation directly unit-testable with **no** reflection, **no** `Form` instance, **no**
  handle creation and **no** WebView2: the test constructs the registry, registers a bare
  `new Control()` (already the established headless idiom) with a `() => true` / `() => false`
  predicate, and asserts. `System.Windows.Forms.Control` is only used as a dictionary key — no
  member that forces handle creation is touched.
- Is the response `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy prescribes:
  "extract all logic into host-neutral, testable modules and leave only the thinnest possible wiring
  in the host-bound entry point."
- Costs one new production file (~45 lines) plus one `<Compile Include>` line in
  `QuickFiler/QuickFiler.csproj`, and shrinks `QfcFormViewer.cs`.

Behaviors to pin: no registration → `false` (the GENUINE case, which is the load-bearing polarity at
`QuickFiler/Interfaces/IQfcFormViewer.cs:79-85`); one registration returning `false` → `false`; one
returning `true` → `true`; two registrations, one `true` → `true`; re-registering the same key
**replaces** rather than appends (the documented reason for a keyed dictionary at
`QfcFormViewer.cs:209-211`); null `itemViewer` or null `popupIsOpen` is ignored.

**Rejected alternative:** build a `QfcFormViewer` with
`FormatterServices.GetUninitializedObject(typeof(QfcFormViewer))` and set `_breadcrumbPopupOwners`
by reflection. There is repository precedent for `GetUninitializedObject` on viewer types
(`EfcHomeControllerExecuteMovesTests.cs:235` on `EfcViewer`;
`ViewerQueueStaticWrapperTests.cs:333`), and `QuickFiler.Test` contains no
`new QfcFormViewer()` anywhere (the two occurrences at `QfcViewer_Test.cs:26,36` and
`QfcHomeControllerRunAsyncTests.cs:155,289` are all commented out). But it leaves a `Form`-derived
object with every field null, requires writing a `readonly` field by reflection, keeps the derivation
inside a `[ExcludeFromCodeCoverage]` type so the test buys no coverage, and couples the test to a
private field's spelling — the exact trade-off CR-6 flagged as undesirable elsewhere in this same
change family.

**Explicitly not covered by either option:** the *wiring* hop at `ItemViewer.Breadcrumb.cs:216`
(`FindForm() as QfcFormViewer`) remains untestable without a real form hierarchy. AC7 should be read
as covering the derivation, and the residual should be stated rather than implied.

---

## Q8 — Coverage state on this branch

### Files expected to change (coverage-relevant production paths)

`QuickFiler/Controllers/QfcFormController.Deactivate.cs`,
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs`,
`QuickFiler/Controllers/QfcFormController.SetupDisposal.cs`,
`QuickFiler/Controllers/QfcHomeController.cs`,
`QuickFiler/Controllers/QfcItemController.EventHandlers.cs`,
`QuickFiler/Viewers/BreadcrumbDropDownHost.cs`,
`QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs`,
`QuickFiler/Viewers/QfcFormViewer.cs`, plus one new file if the AC7 extraction is adopted.

### Coverage artifact search — NEGATIVE

- **SearchScope:** the entire worktree
  `<repo-root>/.claude/worktrees/agent-ac28f83f99cbba6b5`, plus a targeted enumeration of
  `docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/**` and
  `docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/**/*.xml`.
- **SearchPatterns:** Glob `**/coverage*.xml`; Glob
  `docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/**`; Glob
  `docs/features/active/2026-09-06-…-796/evidence/**/*.xml`.
- **SearchResult:** The 810 feature folder contains exactly three files — `spec.md`,
  `plan.2026-09-07T21-59.md`, `issue.md` — and **no `evidence/` directory at all**, therefore no
  baseline coverage artifact for this issue. The #796 feature folder contains **no `.xml` coverage
  document** (its coverage evidence is markdown-only). Roughly 100 `coverage*.xml` documents exist
  elsewhere under `docs/features/**`, all belonging to other, earlier issues; none is a valid
  baseline for this branch. **Conclusion: no coverage artifact exists on this branch for issue #810.**
  A baseline Cobertura run is a required Phase 0 task before any changed-line coverage claim.

### Per-file coverage facts that *are* recorded in-repo (secondary evidence only)

These come from prior issues' evidence and are **not** a baseline for this branch:

- `docs/features/active/…-791/code-review.2026-09-06T15-31.md` (findings table, Observation row):
  `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` **58.12%** line coverage and
  `QuickFiler/Controllers/QfcHomeController.cs` **76.36%**, both below the 85% uniform per-file floor,
  both dispositioned FAIL-but-non-blocking under the ratified CLAUDE.md UT2 exemption class (c).
  **Expect the same disposition to recur on this branch for the same two files.**
- `docs/features/active/…-796/evidence/qa-gates/p9-t7-coverage-delta.md:184` records
  `QfcItemController.EventHandlers.cs` line `209(0)` — the dead accessor AC6 removes is confirmed
  uncovered, so its removal is a small coverage improvement.
- `QuickFiler/Viewers/QfcFormViewer.cs` and `QuickFiler/Viewers/ItemViewer.cs` carry class-level
  `[ExcludeFromCodeCoverage]` (`:17` and `:20`) and therefore emit **no** `<class>` element at all in
  Cobertura — absence of a method element is the exemption signal, not a zero.

### Nullable-gate note

`BreadcrumbDropDownHost.cs:1` and `BreadcrumbDropDownHost.Open.cs:1` both carry `#nullable enable`, so
`/p:TreatWarningsAsErrors=true` promotes their `CS86xx` diagnostics to errors. The four controller
files and `QfcFormViewer.cs` do **not** carry the pragma. A new `BreadcrumbPopupOwnerRegistry.cs`
should carry `#nullable enable` to match the file it sits beside.

---

## Numeric Derivation Evidence

### Family 1 — Complete caller set of `QfcFormController.ParkFocusAndCancelSelectors`

- **Complete Family:** every construct in the repository that can cause
  `QfcFormController.ParkFocusAndCancelSelectors` to execute — direct invocations, method-group
  delegate conversions, and reflective invocation by member name. The method has exactly one
  declaration and no overloads (verified: a single `internal void ParkFocusAndCancelSelectors`
  signature at `QfcFormController.Deactivate.cs:91`; no other declaration of that name exists).
- **Exhaustive Search Scope:** the entire worktree, all file extensions, no path filter.
- **Inclusion Rules:** any executable construct naming the member — invocation expression, method
  group in a delegate-conversion position, or a string literal used for reflective lookup.
- **Exclusion Rules:** the declaration itself; XML-doc `<see cref="…"/>` references; occurrences
  inside `FormatDeactivationDiagnostics` / `FormatItemCancelDiagnostics` log-message string literals
  (these are diagnostic text about the method, never a lookup key); occurrences in
  `docs/features/**` markdown.
- **Primary Search Strategy or Query Expression:** Grep, pattern `ParkFocusAndCancelSelectors`,
  `output_mode: content`, no glob — a repository-wide exact-identifier sweep over every file type.
- **Primary Member Set:**
  1. `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:144` — method-group conversion to
     `System.Action`: `RunTeardownStage("park-focus", ParkFocusAndCancelSelectors);`
  2. `QuickFiler/Controllers/QfcFormController.Deactivate.cs:27` — direct invocation:
     `internal void FormViewer_Deactivated(object sender, EventArgs e) => ParkFocusAndCancelSelectors();`

  Excluded by the stated rules from the same sweep: `Deactivate.cs:91` (declaration);
  `Deactivate.cs:23`, `:31`, `:62` (`<see cref>` doc references); `Deactivate.cs:56`, `:76`
  (log-message literals); all `docs/features/**` hits.
- **Primary Count:** **2**
- **Cross-check Search Strategy or Query Expression:** a *structural* sweep that never mentions the
  member name — Grep, pattern `RunTeardownStage\(|FormDeactivated \+=|FormDeactivated -=`, glob
  `QuickFiler/**/*.cs`, enumerating every teardown stage and every subscription to the deactivation
  event; combined with a reflective-lookup sweep, Grep pattern
  `GetMethod\(|GetProperty\(|InvokePrivate\(|nameof\(QfcFormController`, glob
  `QuickFiler.Test/Controllers/QfcFormController*.cs`.
- **Cross-check Member Set:**
  - Teardown-stage sweep returns nine `RunTeardownStage(` sites at `EventHandlers.cs:26` (the
    declaration), `:133` `cancel-token`, `:143` `reset-keyboard`, **`:144` `park-focus`**, `:145`
    `unregister-handlers`, `:146` `hide-form`, `:150` `quiesce-loader`, `:166` `groups-cleanup`,
    `:170` `controller-cleanup`. Exactly one of the eight stages, `:144`, has
    `ParkFocusAndCancelSelectors` as its body.
  - Event-subscription sweep returns exactly two sites, `SetupDisposal.cs:175`
    (`_formViewer.FormDeactivated += this.FormViewer_Deactivated;`) and `:204` (the matching `-=`).
    Both name `FormViewer_Deactivated`, whose entire body is the invocation at `Deactivate.cs:27`.
  - Reflective sweep returns two `GetMethod(` hits in the `QfcFormController*` test files —
    `QfcFormControllerUndoHandoffTests.cs:175` (targets the *test class itself*) and
    `QfcFormControllerTests.cs:156` (`"MaximizeFormViewer"`). **Neither names this member**, so there
    is no reflective caller.
  - Derived member set: {`EventHandlers.cs:144`, `Deactivate.cs:27`}.
- **Cross-check Count:** **2**
- **Member-set Comparison:** normalized primary set = {`QuickFiler/Controllers/QfcFormController.EventHandlers.cs:144`,
  `QuickFiler/Controllers/QfcFormController.Deactivate.cs:27`}; normalized cross-check set =
  {`QuickFiler/Controllers/QfcFormController.EventHandlers.cs:144`,
  `QuickFiler/Controllers/QfcFormController.Deactivate.cs:27`}. **Identical. Counts agree at 2.**
  The two strategies are independent: the primary keys on the member name and would miss a purely
  reflective caller; the cross-check keys on the two structural mechanisms (teardown-stage table and
  event subscription) plus an explicit reflective sweep, and never uses the member name.

### Family 2 — Complete reference set of `SearchOwnsDropDownDismissal`

- **Complete Family:** every occurrence of the member `SearchOwnsDropDownDismissal` anywhere in the
  repository, in any file type, including compiled source, generated Designer/resx files, project
  files, and reflective lookups by string. The member has one declaration and no overloads (a get-only
  expression-bodied `internal bool` property).
- **Exhaustive Search Scope:** the entire worktree, all file extensions, no path filter.
- **Inclusion Rules:** any textual occurrence of the identifier, in any file type.
- **Exclusion Rules:** none for the count; occurrences are then classified as source vs.
  documentation.
- **Primary Search Strategy or Query Expression:** Grep, pattern `SearchOwnsDropDownDismissal`,
  `output_mode: content`, no glob — exact case-sensitive identifier match across the whole tree.
- **Primary Member Set:** 12 lines across 7 files.
  - **Source (1):** `QuickFiler/Controllers/QfcItemController.EventHandlers.cs:209` — the declaration.
  - **Documentation (11), all under `docs/features/**`:**
    `…-810/issue.md:19`, `:71`;
    `…-796/policy-audit.2026-09-07T17-05.md:287`, `:292`;
    `…-796/feature-audit.2026-09-07T17-05.md:281`;
    `…-796/evidence/qa-gates/p9-t7-coverage-delta.md:218`, `:224`;
    `…-796/evidence/qa-gates/p6-t1-ac4-seam.md:31`;
    `…-796/code-review.2026-09-07T17-05.md:213`, `:297`.
  - **Source occurrences that are not the declaration: none. Test-tree occurrences: none.**
- **Primary Count (source, non-declaration references):** **0** (total occurrences 12; source
  occurrences 1, and that one is the declaration).
- **Cross-check Search Strategy or Query Expression:** two independent sweeps that do not use the full
  identifier — (a) Grep, pattern `SearchOwns`, `-i: true`, `output_mode: count`, no glob (a
  case-insensitive prefix match that would also catch a differently-cased or partially-renamed
  reference); (b) Grep, pattern `GetProperty\(|GetMember\(|GetMethod\("get_`, glob
  `QuickFiler.Test/**/*.cs` (every reflective member lookup in the test assembly, which is the only
  way an `internal` accessor could be read without naming it in a compile-time expression).
- **Cross-check Member Set:**
  - (a) returns 11 total occurrences across 7 files with the distribution
    `QuickFiler\Controllers\QfcItemController.EventHandlers.cs:1`, `…-810/issue.md:2`,
    `…-796/policy-audit…:2`, `…-796/feature-audit…:1`, `…-796/…/p9-t7-coverage-delta.md:2`,
    `…-796/…/p6-t1-ac4-seam.md:1`, `…-796/code-review…:2` — one source file, one hit, which is the
    declaration line. (The 11-vs-12 difference is a per-line count in one document that carries two
    occurrences on a single line; the *file* distribution and the single-source-hit conclusion are
    identical.)
  - (b) returns 29 reflective-lookup sites across `QuickFiler.Test/`. Enumerated targets:
    `IsFolderDropDownOpen`, `BtnDelItem`, `Bounds`, `OpensBelow`, `Count`, `DataModel`,
    `FormController`, `UiScheduler`, `UiDispatcher`, `UiSyncContext`, plus parameterized
    `GetProperty(property)` / `GetProperty(name)` helpers in `BreadcrumbDropDownHostTests.cs:381`,
    `BreadcrumbDropDownLifecycleTests.cs:146`, `BreadcrumbSelectorCoordinatorTests.cs:475`,
    `QfcItemControllerBreadcrumbDropDownTests.cs:348`, `EfcDataModelTests.cs:380` and
    `ConversationResolverTests.cs:418`. **No call site anywhere passes the literal
    `"SearchOwnsDropDownDismissal"`** — confirmed by sweep (a), which is a superset of every string
    literal containing the prefix.
  - Derived member set of non-declaration source references: {} (empty).
- **Cross-check Count (source, non-declaration references):** **0**
- **Member-set Comparison:** normalized primary set of non-declaration source references = {};
  normalized cross-check set = {}. **Identical, both empty. Counts agree at 0.** Independent
  corroboration from an unrelated instrument: the #796 per-line coverage record at
  `p9-t7-coverage-delta.md:184` shows `209(0)` — the runtime executed the accessor zero times across
  the whole `QuickFiler.Test` run, which is what a member with no reader must show.
- **Assertion licensed by this evidence:** `SearchOwnsDropDownDismissal` has exactly one occurrence in
  compiled source — its declaration — and zero readers anywhere, including tests, generated files and
  reflective lookups. It is dead and can be deleted without changing behavior.

### Family 3 — Complete touch-point set of `BreadcrumbDropDownHost.IsCommitPending`

- **Complete Family:** every declaration, write and read of the `IsCommitPending` member across all
  three `BreadcrumbDropDownHost` partial-class files and the whole test assembly. The member is a
  single auto-implemented `internal bool { get; set; }` with no overloads and no backing-field
  declaration of its own.
- **Exhaustive Search Scope:** the entire worktree for the primary; `QuickFiler*/**/*.cs` for the
  cross-check.
- **Inclusion Rules:** the declaration, every assignment, every read.
- **Exclusion Rules:** occurrences in `docs/features/**` markdown; test-method *names* containing the
  substring (they are not touch points).
- **Primary Search Strategy or Query Expression:** Grep, pattern `IsCommitPending`,
  `output_mode: content`, no glob — exact-identifier sweep across the whole tree.
- **Primary Member Set (production):**
  1. `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs:109` — declaration.
  2. `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs:123` — write `false`, in `ShowPopup`.
  3. `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:256` — write `true`, in `Close(ExplicitCommit)`.
  4. `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:447` — read, in `FinishClose`.

  **Primary Member Set (test):** `BreadcrumbPendingOpenCloseTests.cs:141` (write),
  `BreadcrumbDropDownCloseOrderingTests.cs:80` (write), `:105` (read).
- **Primary Count:** **4** production touch points (1 declaration, 2 writes, 1 read); **3** test touch
  points.
- **Cross-check Search Strategy or Query Expression:** Grep, pattern `CommitPending`, `-i: true`,
  glob `QuickFiler*/**/*.cs` — a case-insensitive substring sweep restricted to compiled C# in both
  the production and test projects. This is a strictly broader textual net than the primary (it would
  catch `isCommitPending`, `_commitPending`, `CommitPendingLatch`, or any partially renamed variant)
  applied to a narrower, compile-relevant file set.
- **Cross-check Member Set:** 10 lines — the same four production lines
  (`Open.cs:109`, `Open.cs:123`, `BreadcrumbDropDownHost.cs:256`, `BreadcrumbDropDownHost.cs:447`),
  the same three test touch points (`BreadcrumbPendingOpenCloseTests.cs:141`,
  `BreadcrumbDropDownCloseOrderingTests.cs:80`, `:105`), and three additional hits that are
  **test-method names**, excluded by the stated rules:
  `BreadcrumbPendingOpenCloseTests.cs:135` (`CloseWhilePendingOpenAndCommitPending_DoesNotCancelSelection`),
  `BreadcrumbDropDownCloseOrderingTests.cs:74` (`NativeCloseWhileCommitPending_DoesNotCancelSelection`),
  `:99` (`NativeCloseWithNoCommitPending_StillCancelsSelection`). No differently-cased or
  differently-spelled variant exists.
- **Cross-check Count:** **4** production touch points; **3** test touch points.
- **Member-set Comparison:** normalized production sets are identical —
  {`BreadcrumbDropDownHost.Open.cs:109`, `BreadcrumbDropDownHost.Open.cs:123`,
  `BreadcrumbDropDownHost.cs:256`, `BreadcrumbDropDownHost.cs:447`} in both records. Normalized test
  sets are identical. **Counts agree at 4 and 3.**
- **Assertion licensed by this evidence:** the latch has exactly one clear site today
  (`ShowPopup`, `Open.cs:123`) and exactly one read site (`FinishClose`, `BreadcrumbDropDownHost.cs:447`).
  Adding a clear inside `FinishClose` therefore makes the read site and the second clear site the same
  method, which is what makes the one-popup-lifetime invariant hold on all three close paths.

### Family 4 — Complete call-site set of `BreadcrumbDropDownHost.FinishClose`

- **Complete Family:** every call site of the private method `FinishClose(BreadcrumbDropDownCloseReason)`
  across the three `BreadcrumbDropDownHost` partial files. One declaration, no overloads.
- **Exhaustive Search Scope:** the entire worktree, glob `**/*.cs`.
- **Inclusion Rules:** invocation expressions and method-group references.
- **Exclusion Rules:** the declaration; comment and XML-doc mentions; test-method names.
- **Primary Search Strategy or Query Expression:** Grep, pattern
  `FinishClose|OnDropDownClosed|RestoreAfterOpenFailure|CompleteClose`, glob `**/*.cs`,
  `output_mode: content` — an identifier sweep over `FinishClose` together with every method named in
  the code's own claim about which paths reach it, so the claim and its subject are enumerated by the
  same query.
- **Primary Member Set:**
  1. `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:415` — `() => FinishClose(reason)` inside
     `CompleteClose`.
  2. `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:465` — `() => FinishClose(Uncommitted)` inside
     `RestoreAfterOpenFailure`.
  3. `QuickFiler/Viewers/BreadcrumbDropDownHost.Diagnostics.cs:75` — `FinishClose(Uncommitted)` inside
     the scheduled body of `OnDropDownClosed`.

  Excluded: `BreadcrumbDropDownHost.cs:432` (declaration); `:206`, `:436` (comment/doc text);
  `BreadcrumbDropDownHostTests.Part3.cs:41,76,105,134` (test-method names).
- **Primary Count:** **3**
- **Cross-check Search Strategy or Query Expression:** exhaustive **read** of all three partial-class
  files in full — `BreadcrumbDropDownHost.cs` (496 lines), `BreadcrumbDropDownHost.Open.cs` (131
  lines), `BreadcrumbDropDownHost.Diagnostics.cs` (79 lines) — enumerating every method body and
  classifying which can transitively reach the close-completion point; corroborated by the code's own
  independent claim in the comment at `BreadcrumbDropDownHost.cs:436-438` ("FinishClose is the single
  completion point for the programmatic close path (CompleteClose), the native-close path
  (OnDropDownClosed), and RestoreAfterOpenFailure").
- **Cross-check Member Set:** {`CompleteClose` (`:403-417`), reached from `Close` (`:259`, `:262`),
  `ResetCoreAsync` (`:317`) and `DisposeCoreAsync` (`:339`)}; {`RestoreAfterOpenFailure` (`:455-467`),
  reached from `BreadcrumbDropDownOpenLifetime.cs:376`}; {`OnDropDownClosed`
  (`Diagnostics.cs:54-77`), reached from the `DropDown.Closed` subscription at
  `BreadcrumbDropDownHost.cs:171`}. Reading `Open.cs` in full confirms it contains **no** call site.
  Derived call-site set: {`BreadcrumbDropDownHost.cs:415`, `BreadcrumbDropDownHost.cs:465`,
  `BreadcrumbDropDownHost.Diagnostics.cs:75`}.
- **Cross-check Count:** **3**
- **Member-set Comparison:** normalized primary set = normalized cross-check set =
  {`QuickFiler/Viewers/BreadcrumbDropDownHost.cs:415`, `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:465`,
  `QuickFiler/Viewers/BreadcrumbDropDownHost.Diagnostics.cs:75`}. **Identical. Counts agree at 3.**
- **Assertion licensed by this evidence:** `FinishClose` is the single completion point of all three
  close paths, and exactly one of the three — `RestoreAfterOpenFailure` at `:465` — reaches it with
  no state guard. Clearing the latch inside `FinishClose` covers all three by construction.

---

## Recommended approach

One branch, seven code changes, in this order.

**1. AC1 + AC2 — scope the self-inflicted guard to the deactivation caller.**
- `QuickFiler/Controllers/QfcFormController.Deactivate.cs`: change the signature at `:91` to
  `internal void ParkFocusAndCancelSelectors(bool honourSelfInflictedGuard)`; add the conjunct at
  `:118`; extend the `Issue #796 (AC2)` comment block at `:111-117` with one sentence saying the
  predicate is meaningful only for a deactivation and is therefore supplied by the caller; update
  `:27` to `ParkFocusAndCancelSelectors(honourSelfInflictedGuard: true)`.
- `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:144`: change to
  `RunTeardownStage("park-focus", () => ParkFocusAndCancelSelectors(honourSelfInflictedGuard: false));`.
  The stage-name literal `"park-focus"` must not change — it is the marker
  `QfcFormControllerCancelTeardownTests.MarkerParkFocus` (`:35`) compares against.
- Use a **required** parameter. A defaulted parameter breaks the method-group conversion at `:144`
  (CS0123); the lambda form above sidesteps it entirely and is self-documenting at the call site.

**2. AC3 — stop leaving a disposed source reachable.**
- `QuickFiler/Controllers/QfcHomeController.cs`: add `_tokenSource = null;` immediately after
  `_tokenSource?.Dispose();` (`:389`), and add `_datamodel = null;` alongside the five sibling
  nullings at `:390-394`. Watch the 500-line ceiling: the file is at 496.

**3. AC4 — ribbon-release callback under `finally`, exactly once.**
- `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs`: wrap `:215-258` in `try`, and replace
  `:259-260` with the read-and-clear `finally` shown in Q4.

**4. AC5 + AC6 (host half) — latch lifetime and the stale comment.**
- Relocate `FinishClose` and `RestoreAfterOpenFailure` from `BreadcrumbDropDownHost.cs` into
  `BreadcrumbDropDownHost.Open.cs`, which already owns the latch's declaration and its other clear
  site. This is required by the 500-line ceiling, not optional.
- Add `() => IsCommitPending = false` as the final operation in `FinishClose`'s `CompleteAll` list —
  **inside** the list, not after the call, because `CompleteAll` rethrows.
- Replace the stale comment (currently `BreadcrumbDropDownHost.cs:450`) with CR-1's two-clause text
  plus a clause describing the new clear.

**5. AC6 (item-controller half) — delete the dead accessor.**
- Delete `QuickFiler/Controllers/QfcItemController.EventHandlers.cs:205-209` (the XML doc block and
  the accessor). The backing field and all six of its live uses stay.

**6. AC7 — make the AC2 producer testable.**
- Extract `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs` (new, `#nullable enable`, namespace
  `QuickFiler.Viewers`); reduce `QfcFormViewer.cs:209-246` to one field and two forwarding members;
  add the `<Compile Include>` entry to `QuickFiler/QuickFiler.csproj`.

**7. Tests, RED-first per the CLAUDE.md Bugfix Workflow.**

| AC | Test | Home | Fails before the fix because |
|---|---|---|---|
| AC1 | `ActionCancelAsync_SelfInflictedByOwnPopup_StillCancelsEverySelector` | extend `QfcFormControllerCancelTeardownTests.cs` (393 lines) | With `IsDeactivationSelfInflictedByOwnPopup` set to `true`, the guard returns before the loop and cancel count is 0. |
| AC2 | none — `QfcFormControllerDeactivateTests.cs` stays **byte-unmodified** | — | Its 9 tests, especially `FormDeactivated_SelfInflictedByOwnPopup_DoesNotCancelAnySelector` (`:287`) and `FormDeactivated_CancelsSelectorOnEveryItemController` (`:178`), are the #677/#796 fence. Remaining green unmodified is the AC2 evidence. |
| AC3 | extend `Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted` with a second `Cleanup()` asserting no throw, plus a new test asserting `TokenSource` is null after cleanup | `QfcHomeControllerCleanupTests.cs` (118 lines) | Second pass re-enters `_datamodel.Cleanup()` → `Cancel()` on a disposed source. |
| AC4 | `Cleanup_ViewerDisposeThrows_StillInvokesParentCleanupOnce` — `_mockFormViewer.Setup(x => x.Dispose()).Throws(...)`, assert the parent callback ran exactly once and a second `Cleanup()` does not run it again | `QfcFormControllerCleanupTests.cs` (399 lines) | `_parentCleanup?.Invoke()` at `:259` is skipped by the throw at `:251`. |
| AC5 | `RestoreAfterOpenFailure_WithStaleCommitPending_StillCancelsAndClearsLatch`, using the existing `CloseOrderingHostHarness`; plus a latch-cleared assertion appended to `NativeCloseWhileCommitPending_DoesNotCancelSelection` | `BreadcrumbDropDownCloseOrderingTests.cs` (290 lines) | The stale latch suppresses `_cancelSelection`, so `CancelCount` is 0. |
| AC6 | none required (deletion); the analyzer/nullable rebuilds are the evidence | — | — |
| AC7 | new `QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs` (6 cases per Q7) + `<Compile Include>` in `QuickFiler.Test/QuickFiler.Test.csproj` | new file | The type does not exist. |

`QfcFormControllerCleanupTests.cs` at 399 lines and `QfcFormControllerCancelTeardownTests.cs` at 393
have room for one test each; a third addition to either would need a partial-class split, for which
`QfcStreamingDequeueConfidenceGateTests` (four `Part` files) is the established pattern.

---

## Risks and constraints

### R1 (leads) — The issue-677 keyboard-lock contract

**What the contract guarantees.** From
`docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/spec.md:66-67,74-82`
and the class doc at `QuickFiler/Controllers/QfcFormController.Deactivate.cs:9-15`, two things must be
true the moment activation leaves the QuickFiler form:

1. **No WebView2 child window may keep the shared Outlook UI thread's Win32 keyboard focus.** Outlook
   and QuickFiler share one UI thread, one input queue and one focus window; once a WebView2 holds
   thread-wide focus, the runtime does not reliably release it on click-back
   (MicrosoftEdge/WebView2Feedback #951, open upstream), so every keystroke typed "into" Outlook is
   consumed by the browser surface until QuickFiler is closed.
2. **No breadcrumb `ToolStripDropDown` may stay open**, or WinForms modal menu mode keeps redirecting
   thread keyboard messages to the popup after the user has left.

**Exact call sites and invariants that enforce it.**

| Enforcement | Location |
|---|---|
| Deactivation routing | `IQfcFormViewer.FormDeactivated` (`QuickFiler/Interfaces/IQfcFormViewer.cs:57`) → `QfcFormController.FormViewer_Deactivated` (`Deactivate.cs:26-27`); subscribed `SetupDisposal.cs:175`, unsubscribed `:204`. |
| Clause 1 — focus parking | `Deactivate.cs:100-103` → `IQfcFormViewer.ParkFocusOffWebView2()` → `QfcFormViewer.cs:207` (`this.ActiveControl = _l1v1L2h2_ButtonOK`), gated on `IsWebView2Focused` (`QfcFormViewer.cs:190-201`). |
| Clause 2 — selector cancel | `Deactivate.cs:123-147` → `IQfcItemController.CancelBreadcrumbSelector()` → `QfcItemController.FolderHandling.cs:161` → `ItemViewer.FolderSearch.cs:43` → `BreadcrumbCoordinator.CancelSelector()`. |
| Clause 1 — no re-steal after departure | `BreadcrumbDropDownHost.MayTakeFocus` (`:216`), evaluated **at execution time** inside `FocusPending()` (`:294-298`) and `FocusAnchorIfPermitted()` (`:303-307`); supplied by `ItemViewer.Breadcrumb.cs:212` as `MayRestoreBreadcrumbFocus` (`:274-278`, true only when `Form.ActiveForm` is this form). |
| Polarity invariant | `IQfcFormViewer.cs:79-85` — `false` means GENUINE and must not be inverted; a viewer that reports nothing keeps the #677 contract exactly. |
| Test fences | `QfcFormControllerDeactivateTests.cs` (9 tests) and `BreadcrumbDropDownHostTests.Part3.cs` (8 tests, incl. `FinishClose_PredicateFlipsFalseAfterScheduling_DoesNotFocusAnchor`). |

**Narrowing designs that PRESERVE the contract.** Any design in which the extra condition is
consumed **only** at `Deactivate.cs:118` and the deactivation caller supplies the value that keeps
the guard active — Design A (required boolean parameter, `true` from `FormViewer_Deactivated`) and
Design B (two entry points over a shared core). Under both, for the `Form.Deactivate` path the guard's
condition, its polarity, its placement **below** the focus-parking step, and Moq's default-`false`
behavior are byte-identical to today. The falsifiable check is that
`QfcFormControllerDeactivateTests.cs` stays green **unmodified** — in particular
`FormDeactivated_SelfInflictedByOwnPopup_DoesNotCancelAnySelector` (`:287-303`), which is the AC2
half, and `FormDeactivated_CancelsSelectorOnEveryItemController` (`:178-194`), which is the #677 half.

**Narrowing designs that BREAK the contract.** Each of the following would weaken it and is
therefore **out of scope, to be reported, not made**:

- Deleting the guard at `:118` outright, or making it unconditional-false. Restores the #796 defect:
  the popup a gesture just opened is cancelled by the deactivation that opening caused.
- Inverting `IsDeactivationSelfInflictedByOwnPopup`'s polarity, or changing the "reports nothing →
  `false`" default at `QfcFormViewer.cs:245-246`. The interface documents the polarity as
  load-bearing precisely because Moq's default makes an inversion silent.
- Moving the guard **above** the focus-parking step at `:100-103`. #796 recorded this decision
  explicitly as `AC2-PARK-FOCUS-SUPPRESSED: NO`, on measured evidence that parking did not run on two
  of the three defective gestures. Suppressing parking would leave clause 1 unenforced exactly when a
  popup WebView2 is likely to hold focus.
- Touching `MayTakeFocus`, `FocusPending()`, `FocusAnchorIfPermitted()`, or the `ItemViewer` predicate
  wiring at `ItemViewer.Breadcrumb.cs:212`. None is required by any AC here.
- Removing `ParkFocusOffWebView2()` from either path, or removing the `park-focus` stage from
  `ActionCancelAsync`.

### R2 — Un-suppressing the cancel loop on the Cancel path re-enters the focus machinery

Once AC1 lands, the `park-focus` teardown stage will, with a popup open, call
`CancelBreadcrumbSelector()` per item. That chain reaches `host.Close(Uncommitted)` →
`CompleteClose` → `FinishClose` → `FocusAnchorIfPermitted` (`BreadcrumbDropDownHost.cs:451`), and
`MayRestoreBreadcrumbFocus` (`ItemViewer.Breadcrumb.cs:274-278`) returns **true** while QuickFiler is
still the active form — which it is during a Cancel *button click*. In principle the anchor WebView2
could be re-focused after `ParkFocusOffWebView2()` ran earlier in the same stage.

Three facts bound the risk, and none of them closes it from source alone:

- The cancel is asynchronous. `BreadcrumbDropDownOpenCoordinator` posts its work through
  `_operations.PostAsync` (`:182`, `:197`) and `Close()` routes through
  `_openLifetime.InvalidateAndSchedule` (`BreadcrumbDropDownHost.cs:259`), so `FinishClose` most
  likely lands after the `hide-form` stage (`EventHandlers.cs:146`), by which point `Form.ActiveForm`
  is no longer this form and the predicate returns false.
- Even if it lands early, `_formViewer?.Dispose()` (`SetupDisposal.cs:251`) disposes the WebView2
  children a few stages later, which the #677 research names as the mechanism by which closing
  QuickFiler recovers focus today.
- The current behavior is not better: today the popup is closed anyway, later, through
  `ResetBreadcrumb`, which reaches the same `FinishClose`.

**Recommendation:** do not reorder the #791 stages to accommodate this — the stage order is pinned by
two tests and was the point of #791. Record the interaction in the spec's Risks section and verify it
in a live-Outlook runbook step ("Cancel with a breadcrumb popup open; confirm Outlook keyboard input
works immediately afterwards").

### R3 — File-size ceilings are the binding constraint on two files

`QuickFiler/Controllers/QfcHomeController.cs` at 496 lines and
`QuickFiler/Viewers/BreadcrumbDropDownHost.cs` at 496 lines both sit 4 lines under the 500-line
ceiling in `.claude/rules/general-code-change.md`. AC3 adds two statements to the first; AC5+AC6 add
several lines plus comments to the second. The host file **requires** the relocation described in Q5;
the home controller requires comment discipline. The #791 review already flagged both files as
within four lines of the ceiling and predicted "the next edit to any of them will need a split".

### R4 — Legacy project files

Both `QuickFiler/QuickFiler.csproj` and `QuickFiler.Test/QuickFiler.Test.csproj` are non-SDK projects
with explicit `<Compile Include>` items. A new file that is not registered compiles to nothing and
its tests silently do not run. Add both entries in the same task as the file.

### R5 — Coverage disposition will recur

`QfcFormController.EventHandlers.cs` (58.12%) and `QfcHomeController.cs` (76.36%) were both below the
85% per-file floor at the #791 head and are both changed again here. Expect the same
FAIL-but-non-blocking disposition under the ratified CLAUDE.md UT2 exemption class (c), and record it
in the policy audit rather than discovering it at review time. `QfcFormViewer.cs` emits no `<class>`
element at all (class-level `[ExcludeFromCodeCoverage]`), so the AC7 extraction is the only way any
of that logic enters the coverage denominator.

### R6 — No baseline exists

There is no coverage artifact on this branch (search record in Q8). A Phase 0 baseline Cobertura run
is a precondition for any changed-line coverage claim, and its absence must produce a BLOCKED or
INCOMPLETE verdict per the fail-closed evidence rule in the plan template.

### R7 — `spec.md` is an unpopulated template

`docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/spec.md` still carries
the generic scaffold, including a generic eight-item Acceptance Criteria list that does not match
issue #810's AC1–AC8. Until it is populated, `issue.md:66-73` is the only authoritative AC list, and
no numeric acceptance criterion should be asserted against `spec.md`.

### COLLISION check — files a concurrent run owns

The delegation names `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs`,
`TaskMaster/ThisAddIn.cs` and `UtilitiesCS/Threading/UiThread.cs` as owned by a concurrent run.

**No collision.** The recommended approach requires none of the three. Specifically:

- `QfcHomeControllerRunAsyncTests.cs` — AC3's tests belong in
  `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs`, which already owns
  `Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted` and is the correct home. `RunAsyncTests`
  exercises `RunAsync`, not `Cleanup`. (For completeness: it does reference `TokenSource` indirectly
  through `RunAsync`, and its two `new QfcFormViewer()` occurrences at `:155` and `:289` are
  commented out, so the AC3 field change cannot affect it at compile time.)
- `TaskMaster/ThisAddIn.cs` — not reached. The ribbon-release chain this issue touches ends at
  `TaskMaster/Ribbon/RibbonController.ReleaseQuickFiler` (`:148`), which is **read** for context here
  and not modified.
- `UtilitiesCS/Threading/UiThread.cs` — not reached. `UiThread.Dispatcher` is used by
  `QfcFormController.EventHandlers.cs:276,319,324` and `QfcHomeController.cs:360`, all outside the
  changed statements.

One adjacent file **is** worth naming so it is not edited by reflex: `UtilitiesCS/Threading/ProgressViewer.cs:75`
holds an un-null-guarded `_cancelSource!.Cancel()` on the same shared token source. It is reported in
"Out of scope" below and must not be changed on this branch.

---

## Out of scope / report-only

1. **`ProgressViewer.cs:75` un-guarded `Cancel()` on the shared source.**
   `UtilitiesCS/Threading/ProgressViewer.cs:75` calls `_cancelSource!.Cancel()` from the progress
   dialog's Cancel button, on the same `CancellationTokenSource` created at `QfcHomeController.cs:54`.
   It is the only `Cancel()` on that instance that is neither null-guarded nor ordered by teardown.
   AC3's nulling does not affect it (it holds its own captured reference), but it is a fourth sharer
   the #791 review did not enumerate. Promote as a follow-up.
2. **Token-source ownership redesign.** Replacing the shared `CancellationTokenSource` handoff with
   `CancellationToken` parameters across `QfcDatamodel`, `QfcFormController`,
   `QfcCollectionController`, `QfcItemController` and `ProgressTracker` is the structural fix for the
   whole family. Multi-file API change; out of scope.
3. **`QfcDatamodel` coverage exclusion.** The class-level `[ExcludeFromCodeCoverage]` at
   `QfcDatamodel.cs:25` removes `QuiesceLoaderAsync`, `TryQueueRemainingMailItemAsync` and the whole
   queue-processing partial from the coverage denominator. The #791 review already recommended
   extracting the host-neutral logic. Out of scope; the direct conflict between
   `.claude/rules/general-unit-test.md`'s Coverage Exclusion Policy and CLAUDE.md UT2's ratified
   exemption pre-exists this branch.
4. **CR-6 — reflection-by-field-name in the AC4 re-pin.**
   `QfcItemController.SearchDismissalTests.cs:85` sets `_searchOwnedDismissal` by string literal.
   Driving the real `TextBoxSearch_TextChanged` path instead would remove the coupling. Not required
   by AC6, which asks for removal of the accessor.
5. **CR-5 — `_breadcrumbPopupOwners` entries are never removed.** `QfcFormViewer.cs:212-234` gains
   entries and never loses one. Growth is bounded by the item-viewer pool and a stale entry cannot
   wrongly report `true` (a disposed host leaves `OpenState` false), so this is informational. If the
   AC7 extraction is adopted, the new registry is the natural home for an eventual `Unregister`; do
   not add one speculatively on this branch.
6. **`ItemViewer.Breadcrumb.cs:216` registration hop remains untested.** `FindForm() as QfcFormViewer`
   needs a real form hierarchy. AC7 covers the derivation, not the wiring.
7. **The #791 N12 observation — misleading unconditional log lines.**
   `QfcFormController.EventHandlers.cs:171` and `QfcHomeController.cs:404` both log "ribbon release
   callback invoked" unconditionally, including when the callback did not run. AC4 makes the first of
   these true more often but does not make it always true (a `RunTeardownStage` catch still swallows a
   throw). Not in any AC; report.
8. **The reverse-ordering limit of the commit-pending latch.** CR-1's analysis (§1, "The limit of the
   protection") records that a native uncommitted-reason close arriving **before** the commit's
   `Close(ExplicitCommit)` would still cancel. AC5 does not change that and should not be read as
   doing so.
9. **`QuickFiler.Test.csproj` exceeds 500 lines.** Pre-existing; the ceiling rule enumerates
   "production code, test code, or reusable script file", and an MSBuild project file is none of
   these. Adding a `<Compile Include>` line for the AC7 test grows it further. Recorded so it is not
   read as a new violation.

---

## Files expected to change

### Production

| Path | AC | Change |
|---|---|---|
| `QuickFiler/Controllers/QfcFormController.Deactivate.cs` | AC1, AC2 | Required `honourSelfInflictedGuard` parameter at `:91`; conjunct at `:118`; comment extension at `:111-117`; call-site update at `:27`. |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | AC1 | `:144` becomes a lambda passing `honourSelfInflictedGuard: false`. Stage literal `"park-focus"` unchanged. |
| `QuickFiler/Controllers/QfcHomeController.cs` | AC3 | `_tokenSource = null;` after `:389`; `_datamodel = null;` with the sibling nullings at `:390-394`. **496 lines — ceiling risk.** |
| `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` | AC4 | `try` around `:215-258`; read-and-clear `finally` replacing `:259-260`. |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | AC5, AC6 | Remove `FinishClose` (`:432-453`) and `RestoreAfterOpenFailure` (`:455-467`) — relocated. **496 lines; relocation is required, not optional.** |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` | AC5, AC6 | Receive both relocated methods; add the latch clear as the final `CompleteAll` operation; corrected comment; update the lifetime doc at `:102-107` to say `FinishClose` also clears it. |
| `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` | AC6 | Delete `:205-209` (XML doc + dead accessor). |
| `QuickFiler/Viewers/QfcFormViewer.cs` | AC7 | Replace `:212-213`, `:226-234`, `:245-246` with one registry field and two forwarding members. |
| `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs` | AC7 | **New file.** `#nullable enable`, namespace `QuickFiler.Viewers`, no `[ExcludeFromCodeCoverage]`. |
| `QuickFiler/QuickFiler.csproj` | AC7 | Add `<Compile Include="Viewers\BreadcrumbPopupOwnerRegistry.cs" />` near `:415-419`. |

### Test

| Path | AC | Change |
|---|---|---|
| `QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs` | AC1 | One added test (`ActionCancelAsync_SelfInflictedByOwnPopup_StillCancelsEverySelector`). 393 lines. |
| `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs` | AC3 | Extend `Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted` with a second `Cleanup()`; add a `TokenSource`-is-null assertion. 118 lines. |
| `QuickFiler.Test/Controllers/QfcFormControllerCleanupTests.cs` | AC4 | One added test (`Cleanup_ViewerDisposeThrows_StillInvokesParentCleanupOnce`). 399 lines. |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs` | AC5 | One added test using the existing `CloseOrderingHostHarness`; one added assertion to `NativeCloseWhileCommitPending_DoesNotCancelSelection`. 290 lines. |
| `QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs` | AC7 | **New file**, six cases. |
| `QuickFiler.Test/QuickFiler.Test.csproj` | AC7 | Add `<Compile Include="Viewers\BreadcrumbPopupOwnerRegistryTests.cs" />`. |

### Deliberately NOT changed

- `QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs` — the AC2 fence; must stay
  byte-unmodified and green.
- `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs`, `TaskMaster/ThisAddIn.cs`,
  `UtilitiesCS/Threading/UiThread.cs` — owned by a concurrent run; not required (see COLLISION check).
- `UtilitiesCS/Threading/ProgressViewer.cs` — report-only item 1.
- `QuickFiler/Controllers/QfcDatamodel*.cs` — report-only items 2 and 3.
