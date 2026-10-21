# WinFormsPumpHost Suite Determinism — Research (#511 + #571)

Timestamp: 2026-08-21T18-20

Feature: `winformspumphost-suite-determinism-511` (epic child 1 of 4,
`quickfiler-suite-determinism-foundation`)

Scope of this document: research only. No source file, project file, configuration file, or
`.claude/**` file was modified. No build and no test run was executed. Every claim below is
grounded in a file read or a grep against the worktree at
`<repo-root>\.claude\worktrees\agent-a5bd77000d205e542`, or is explicitly
labelled as documented framework behaviour or as an open question.

Paths in this document are repository-relative for readability. The absolute root for every one of
them is the worktree path above.

---

## Executive summary

1. **#511's "visible window" is not attributable to `WinFormsPumpHost` or to anything in this
   feature's blast radius.** `Application.Run(new ApplicationContext())` with no `MainForm` shows
   nothing, `QuickFiler.Test/Form1.cs` is never instantiated, and the `ItemViewer`'s WebView2
   children never receive a window handle. The only enabled test in the whole nine-assembly corpus
   that shows a real top-level `Form` is `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs:73`
   (`viewer.Show()` on `ProgressViewer : Form`). That is a different assembly and a different
   defect. Confidence: high.
2. **#571's root cause is a single unguarded `Control.Invoke`.** Every other Control-marshalling
   call reached during initialization is guarded by `InvokeRequired`, which returns `false` for a
   handle-less control; `QfcItemController.InvokeBeginInvoke`
   (`QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:248-258`) is the only one that is
   not. That, and only that, is why the two synchronous `Initialize` paths fail while the three
   asynchronous ones do not.
3. **Recommended direction:** deterministically create the `ItemViewer`'s window handle on the pump
   thread inside the shared harness (read `viewer.Handle`), following the maintainer-ratified
   in-repo precedent at `Tags.Test/TagControllerRendering.StaTests.cs:37-48`. Do not replace the
   pump with a synchronization-context seam, and do not change `InvokeBeginInvoke`'s production
   shape in this child.
4. **The epic's coverage-justification line numbers have NOT drifted** — all seven cited positions
   are exact. What is wrong is that the list is *incomplete*: there are seven de-exemption comment
   blocks in `QfcItemController.Initialization.cs`, not five.
5. **A hard, previously unrecorded constraint:**
   `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` is **497 lines** — three
   lines below the 500-line cap. It is the natural home for a `ToggleTips`/`InvokeBeginInvoke`
   regression test and it is effectively full. Combined with the ban on editing
   `QuickFiler.Test.csproj`, this materially constrains where regression tests can live and argues
   against the production-guard remedy.

---

## Q1 — What creates the visible window that #511 reports?

**Answer: nothing in this feature's blast radius. Confidence: high for the pump host and the
QuickFiler test assembly; medium-high for the WebView2 sub-question.**

### Q1.1 The pump host itself creates no visible window

`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs:295-346` (`RunPumpThread`) does exactly three
things that touch WinForms:

- `:303-305` installs a `WindowsFormsSynchronizationContext`.
- `:323` subscribes `Application.ThreadException`.
- `:325-326` `applicationContext = new ApplicationContext(); Application.Run(applicationContext);`

The `ApplicationContext` is constructed with the parameterless constructor, so its `MainForm` is
`null`. `Application.Run(ApplicationContext)` shows a window only through `context.MainForm`; with
no `MainForm` there is no window to show. No `Form`, `UserControl`, or `Control` is created
anywhere in `WinFormsPumpHost.cs` — grep for `new Form`, `.Show()`, `.ShowDialog()` returns only
the XML-doc mention at `:12` and the `Application.Run` call at `:326`.

Two **invisible** windows are created on the pump thread as a side effect, and neither is a desktop
window:

- A WPF message-only dispatcher window, because `Dispatcher.CurrentDispatcher` is touched on the
  pump thread (`WinFormsPumpHost.cs:245` `Dispatcher.FromThread(_thread)`;
  `QuickFiler/Viewers/ItemViewer.cs:28` `_uiDispatcher = Dispatcher.CurrentDispatcher;`).
- The WinForms *parking window*, if and when any parentless child control's handle is created on
  that thread. It is never shown.

### Q1.2 `QuickFiler.Test/Form1.cs` is dead — confirmed

`Form1` appears in `QuickFiler.Test` only at `QuickFiler.Test/Form1.cs:5,7`,
`QuickFiler.Test/Form1.Designer.cs:3,195,202,203`, the `.csproj` compile/resource entries
(`QuickFiler.Test/QuickFiler.Test.csproj:161,164,165,180,181`), and the stale
`QuickFiler.Test/QuickFiler.Test.csproj.bak`. There is **no construction site**. Ground truth
confirmed; this remains #491's scope, not ours.

A second dead form-ish declaration exists: `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs:243`
declares `public class QfcFormViewerDerived : QfcFormViewer` with a `Show()` override at `:248`.
Grep for `QfcFormViewerDerived` across all `*.cs` returns only the declaration at `:243` and its
constructor at `:245` — it is never instantiated, so it never shows anything. (Reporting it because
it is adjacent to #491's scope; it is not ours to remove.)

### Q1.3 The WebView2 controls do not produce a window in these tests

`QuickFiler/Viewers/ItemViewer.Designer.cs` constructs two
`Microsoft.Web.WebView2.WinForms.WebView2` controls at `:46` and `:49`, wraps them in
`ISupportInitialize.BeginInit()`/`EndInit()` at `:89-90` and `:6166-6167`, and adds them to the
table-layout panel at `:116` and `:119`. Their only event wiring is
`ItemViewer.Designer.cs:256` → `ItemViewer.cs:166-169`, whose entire body is
`Console.WriteLine("Parent Changed")`.

In the pump-hosted tests the browser process is unreachable: the harness injects a
`Mock<IWebViewCoreInitializer>` whose `CreateEnvironmentAsync` and `EnsureCoreWebView2Async` both
throw `WebViewSentinelException`
(`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs:261-281`). More
fundamentally, a WinForms `Control` has **no HWND at all** until its handle is created, and nothing
in the initialization path creates the `ItemViewer`'s handle (see Q2), so the WebView2 children have
no window either.

Residual uncertainty (why this is "medium-high", not "high"): the WebView2 WinForms control's
implicit-initialization trigger (`ISupportInitialize.EndInit`, `OnParentChanged`,
`OnVisibleChanged`, `OnHandleCreated`) is third-party code that is not in this repository and that I
could not read. If any of those paths creates a visible window on a *handle-less, parentless*
control, that would contradict the finding. This is recorded in Open Questions with a cheap
verification.

### Q1.4 `WpfDispatcherYieldTests` creates no window

#511 names this suite. It is at `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`
(class at `:13`), and its `StaDispatcherHost` is at `:172-199`. That host runs
`System.Windows.Threading.Dispatcher.Run()` on an STA thread (`:183`) and creates no `Form` and no
`Control`. Note that #511's own text points at
`UtilitiesCS.Test/Threading/WpfUiDispatcherTests.cs` for the analogue; a `StaDispatcherHost` does
exist there at `:161`, and seven more copies exist elsewhere
(`TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs:334`,
`UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersControllerInitializationTests.cs:347`,
`UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceInvalidationTests.cs:404`,
`.../OutlookFolderTreeServiceDisposalTests.cs:409`, `.../OutlookFolderTreeServiceConcurrencyTests.cs:133`,
`.../OutlookFolderHierarchyReaderTests.cs:402`, `.../FolderTreeSnapshotBuilderYieldTests.cs:118`).
None of them creates a window.

### Q1.5 What the visible window IS attributable to

A repository-wide grep for `.Show()` / `.ShowDialog()` / `Application.Run(` across every `*Test*`
project yields exactly one enabled call that shows a real top-level `Form`:

```
UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs:73        viewer.Show();
```

inside `[TestMethod] CancelSource_WhenAssigned_EnablesButtonAndCancelsSameSourceOnClick`
(`:40-41`), on a real `ProgressViewer` constructed at `:49`. `ProgressViewer` is a `Form`:
`UtilitiesCS/Threading/ProgressViewer.cs:16` — `public partial class ProgressViewer : Form`. The
class is `[STATestClass]` (`:30`), so `Show()` executes on a real STA thread and produces a genuine
desktop window. The test never calls `Hide()`; it disposes the viewer in `finally` (`:89-92`), so
the window is transient but real.

Every other candidate is exonerated:

| Candidate | Verdict | Evidence |
| --- | --- | --- |
| `UtilitiesCS.Test/ResourceTests.cs:21,29,112` (`frm.ShowDialog()`) | Not run | `[Ignore("Interactive form smoke test; excluded from unattended test runs.")]` at `:17`, `:25`, `:108` |
| `UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersControllerRefreshDisposalTests.cs:133,188` (`viewer.Show()`) | Fake, not a Form | `RecordingFilterViewer : IFilterOlFoldersViewer` at `FilterOlFoldersControllerInitializationTests.cs:420`; `Show()` is a counter |
| `UtilitiesCS.Test/Threading/ProgressPane_Tests.cs:68,118,176` (`new ProgressPane()`) | `UserControl`, never shown | `UtilitiesCS/Threading/ProgressPane.cs:15` — `: UserControl` |
| `QuickFiler.Test/QfcViewer_Test.cs:27,43,58,61,67` | All commented out | leading `//` on every line |
| `MyBox`/`InputBox`/`NotImplementedDialog` `DialogInvoker = viewer => viewer.ShowDialog()` | Seam assignment, viewer is a double | e.g. `UtilitiesCS.Test/Dialogs/MyBoxModelessTests.cs:49` asserts "the real viewer.Show() must never be called in a test" |
| `ProgressTrackerPane` | Not a control | `UtilitiesCS/Threading/ProgressTrackerPane.cs:9` — `: IProgress<(int, string)>` |

**Consequence for the spec.** #511's Actual-Behavior bullet "A visible window appeared during the
run, because the host constructs a real WinForms control and pumps a real message loop" states a
causal claim that the evidence does not support. This child cannot honestly close that bullet by
changing `WinFormsPumpHost`. The spec should:

- scope #511 to the *load-flakiness* half of the report, which is real and is this child's to fix;
- record the visible-window finding as an evidence-based re-attribution to
  `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs:73`; and
- promote that re-attribution to its own issue through the promotion lifecycle rather than leaving
  it as prose in a feature folder that disappears at merge.

A one-line fix exists for the re-attributed defect (`ProgressViewer` construction is already
headless-capable via `CreateHeadlessViewer` at `ProgressViewer_Tests.cs:33-34`), but it is in
`UtilitiesCS.Test`, outside this child's declared file set, and belongs to the separate issue.

---

## Q2 — Why do only 2 of the 6 pump-hosted tests fail?

**Answer: because `Control.InvokeRequired` returns `false` for a handle-less control, and every
Control-marshalling call on the initialization paths is guarded by it except
`QfcItemController.InvokeBeginInvoke`.**

### Q2.1 The failing call site

`QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:248-258`:

```csharp
public void InvokeBeginInvoke(bool async, System.Action action)
{
    if (async)
    {
        _itemViewer.BeginInvoke(action);
    }
    else
    {
        _itemViewer.Invoke(action);
    }
}
```

`ToggleTips` at `:202-217` is the only caller reached during initialization (`:204`
`InvokeBeginInvoke(async, ...)`).

Documented framework behaviour (`System.Windows.Forms.Control`):

- `Control.Invoke` **and** `Control.BeginInvoke` both throw
  `InvalidOperationException("Invoke or BeginInvoke cannot be called on a control until the window
  handle has been created.")` when no control in the target's parent chain has a created handle.
  The `async == true` branch is therefore *not* safe either; it is simply never taken by the tests
  that fail.
- `Control.InvokeRequired` searches up the parent chain for a control with a window handle and
  **returns `false` when none is found**. This is the documented behaviour, not an implementation
  detail.

### Q2.2 Per-test trace

`_itemViewer` is `IItemViewer` (`QuickFiler/Controllers/QfcItemController.cs:51`), bound in the
harness to a real `QuickFiler.ItemViewer` constructed on the pump thread at
`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs:84`. It is never
parented to a `Form` and its handle is never forced, so `IsHandleCreated` is `false` for the whole
test.

| # | Test | file:line | Entry point | Reaches `Control.Invoke`? | Why |
| --- | --- | --- | --- | --- | --- |
| 1 | `InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState` | `Part3.cs:40` | `InitializeSequentialAsync()` (`Initialization.cs:295`) | **No** | `SetThemeLight(async: true)` → `Theme.SetQfcTheme(true)` → `_uiDispatcher.InvokeAsync` (`Theme.cs:431`), the injected inline dispatcher. Tips use `ToggleTipsAsync` (`Initialization.cs:318`-region → `FocusAndTheme.cs:219`), which awaits `tip.ToggleAsync` and never touches `Control.Invoke`. |
| 2 | `InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme` | `Part3.cs:83` | `InitializeGraphicsAsync()` (`Initialization.cs:263`) | **No** | `SetThemeDark(async: false)` (`Initialization.cs:279`) → `Theme.SetQfcTheme(false)` → the `else if (_lblItemNumber.InvokeRequired)` guard at `Theme.cs:433` evaluates **false** (no handle anywhere), so the `else` at `:437-440` calls `SetQfcTheme()` inline. Tips/nav use the `Async` variants. |
| 3 | `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` | `Part3.cs:131` | `Initialize(bool async: false)` (`Initialization.cs:168`) | **YES — fails** | `Initialization.cs:185` `ToggleTips(async: false, ...)` → `FocusAndTheme.cs:204` → `:256` `_itemViewer.Invoke(action)`, unguarded. |
| 4 | `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` | `Part3.cs:175` | private nine-arg `Initialize(...)` (`Initialization.cs:138`) with `async: false` | **YES — fails** | `Initialization.cs:161` `Initialize(async);` funnels into case 3. |
| 5 | `InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults` | `Part3.cs:245` | `InitializeAsync()` (`Initialization.cs:202`) | **No** | `SetThemeDark/Light(async: true)` (`Initialization.cs:216-219`) → `_uiDispatcher.InvokeAsync`. Tips/nav use the `Async` variants. Execution stops at the mocked web-view seam. |
| 6 | `ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups` | `ViewerSetupTests.cs:426` | `ResolveControlGroupsAsync(ItemViewer)` (`ViewerSetup.cs:258`) | **No** | The member only awaits `itemViewer.UiSyncContext` (`:269`) and builds `QfcTipsDetails`; no `Control.Invoke`. |

This confirms the epic's Hard Constraint 3 claim ("only the two synchronous `Initialize` paths
reach `Control.Invoke`") and supplies the missing mechanism: **it is not that the async paths use
`BeginInvoke` instead — `BeginInvoke` would throw identically. It is that they never call
`InvokeBeginInvoke` at all, and the one sibling that does marshal synchronously
(`Theme.SetQfcTheme(false)`) is `InvokeRequired`-guarded.**

### Q2.3 The guard pattern is the repository's own convention

Two production call sites in the same execution path already use it:

- `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs:433` — `else if (_lblItemNumber.InvokeRequired)`.
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:361` — `if (_itemViewer.InvokeRequired)`
  inside `AssignControls`, which is on the `Initialize(bool)` path via `PopulateControls`
  (`ViewerSetup.cs:313-318`).

Both are reached *before* `ToggleTips` in `Initialize(bool)`, and both take the non-marshalling
branch. `InvokeBeginInvoke` is the sole outlier.

### Q2.4 A contradiction I could not resolve

Static reading predicts that tests 3 and 4 fail on **every** run, because `Control.Invoke` throws
unconditionally when no handle exists and I found no code path in
`ResolveControlGroups` → `SetupThemes` → `PopulateControls` that creates one:

- `ResolveControlGroups` (`ViewerSetup.cs:208-252`) uses `GetAllChildren`
  (`UtilitiesCS/Extensions/WinFormsExtensions.cs:146-158`), which only walks `Control.Controls`.
- `QfcTipsDetails` (`UtilitiesCS/HelperClasses/ToolTips/QfcTipsDetails.cs`) contains no
  `Handle`/`CreateControl`/`CreateGraphics` reference; its only `Invoke` mentions are commented out
  at `:46-49` and `:94-97`.
- `QfcThemeHelper.SetupThemes` (`QuickFiler/Helper Classes/QfcThemeHelper.cs:36-93`) only captures
  control references into a `QfcThemeControlSet`.
- `AssignControls` (`ViewerSetup.cs:358-...`) sets `Text`/colour properties, which WinForms caches
  without creating a handle.

#571 nevertheless records "run 1 passed both tests, run 2 failed both, run 3 passed both", and
class-isolated runs passing 9 of 9 every attempt. Either (a) some third-party path — most plausibly
the WebView2 control's `ISupportInitialize.EndInit` or implicit-initialization logic — creates the
`ItemViewer`'s handle non-deterministically, or (b) the recorded observation attributes a different
failure mode to these two names. See Open Questions. **This does not change the recommendation**:
forcing the handle removes the dependency in the passing direction whichever explanation holds.

---

## Q3 — Candidate remedies for #571, evaluated

Common evaluation axes: deterministic handle; visible window; production behaviour change;
pump-hosted coverage preserved; interaction with `UiThreadDispatcherGate`
(`Part2.cs:51`, acquired at `:67`, released at `:74` and `:341`).

### The `.Handle` versus `CreateControl()` distinction (load-bearing — stated precisely)

- **`Control.Handle` (getter).** Documented: reading `Handle` forces creation of the control's
  window handle if it does not already exist. It creates **only that control's** handle. For a
  parentless child control, WinForms parks the new HWND on the thread's hidden parking window; the
  parking window is never shown, so nothing becomes visible. In-repo precedent, explicitly
  maintainer-ratified: `Tags.Test/TagControllerRendering.StaTests.cs:39-41`

  ```csharp
  // Act: force invisible handle creation, then invoke the real draw path.
  var handle = checkBox.Handle;
  handle.Should().NotBe(IntPtr.Zero);
  ```

  with the class doc at `:12-17` stating "an unshown WinForms `CheckBox` control (never a `Form`)
  is constructed on an STA thread; the test never shows a window, uses no message
  pump/timer/sleep, and disposes the control." A second precedent is
  `UtilitiesCS.Test/EmailIntelligence/OSBrowser_Tests.cs:233` — `_ = browser.Handle;`.

- **`Control.CreateControl()`.** Documented: it does **not** create the handle if the control's
  `Visible` property is `false`. That caveat does **not** save us here — a parentless `UserControl`
  reports `Visible == true` (the visibility walk terminates at the control itself when there is no
  parent), so `CreateControl()` *would* create the handle. The real objection is different and
  stronger: `CreateControl()` **recurses into every visible child control** and additionally fires
  `OnCreateControl`. On `ItemViewer` that means creating handles for both
  `Microsoft.Web.WebView2.WinForms.WebView2` controls (`ItemViewer.Designer.cs:46,49`), which is
  exactly the third-party surface Q1.3 flags as unverified. In-repo precedent exists
  (`UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperViewerTests.cs:113`) but on a much simpler
  control tree.

  **Summary: `.Handle` forces creation regardless of `Visible` and touches only the one control;
  `CreateControl()` is `Visible`-gated and recursive. For `ItemViewer`, `.Handle` is strictly the
  narrower instrument.**

### Evaluation table

| # | Remedy | Deterministic handle | Visible window | Production change | Pump coverage preserved | Gate interaction |
| --- | --- | --- | --- | --- | --- | --- |
| **(a)** | Read `viewer.Handle` on the pump thread in `BuildPumpHarnessCoreAsync` (and in `ViewerSetupTests.cs:432-435`) | **Yes** — documented, unconditional | **No** — parked on the hidden parking window; ratified precedent | **None** | **Yes, all 8** | None. Runs inside the section already serialized by the gate. |
| (b) | `viewer.CreateControl()` | Yes here (parentless `UserControl` is `Visible`) | No, same parking mechanism | None | Yes | None | 
| (c) | Parent the viewer to a hidden `Form` created on the pump thread | Yes (force the `Form`'s handle) | No if never `Show()`n — `Form` is created with `Visible == false` | None | Yes | None, but adds a `Form` to dispose in `Restore` |
| (d) | `Application.Run(new ApplicationContext { MainForm = hiddenForm })` | Yes | **Likely YES** — `Application.Run(ApplicationContext)` makes `MainForm` visible when the loop starts | None | Yes | None | 
| (e) | Add an anchor control to `WinFormsPumpHost` generically | Yes for the anchor, **not** for `ItemViewer` — `FindMarshalingControl` walks *parents*, and the viewer is not parented to the anchor | No | None | Yes | None |
| (f) | Make `InvokeBeginInvoke` consult `InvokeRequired`/`IsHandleCreated` | N/A — removes the need | No | **Yes** | Yes | None |

### Discussion

**(a) is the recommendation.** It is the narrowest change that removes the race; it is confined to
test-support code; it changes no production line; it preserves all eight consumer tests and all
seven coverage justifications; and it has an explicit, maintainer-ratified in-repo precedent whose
comment already asserts the no-visible-window property.

**(b)** is acceptable but strictly wider than (a) for no benefit, and it drags the two WebView2
controls into handle creation. Reject on minimality.

**(c)** works and is arguably the most faithful simulation of production (in production the viewer
*is* parented). It costs a `Form` that must be created, tracked, and disposed on the pump thread in
`PumpHarness.Restore` (`Part2.cs:331-342`), and it widens the blast radius of a shared fixture that
two test classes depend on. Reject as second choice, not as wrong.

**(d)** should be rejected. `Application.Run(ApplicationContext)` starts the message loop and makes
`context.MainForm` visible; that is how `Application.Run(Form)` shows a window at all. Adopting it
would *introduce* the visible window that #511 complains about. Confidence: medium-high; if a
future author wants it, the visibility behaviour must be verified empirically first.

**(e)** does not work for the stated purpose and this is worth recording so it is not re-proposed.
`Control.Invoke` resolves its marshaling control by walking the target's **parent chain**. An
anchor control owned by the host is not an ancestor of the harness's `ItemViewer`, so the viewer
still has no handle in its chain and still throws. (e) would only help if the host *parented* every
consumer's control, which is remedy (c) in disguise.

**(f)** is the one remedy that fixes the production asymmetry rather than the test. It is
attractive on the merits — `InvokeBeginInvoke` is the only unguarded marshaller in the class, and
`Theme.cs:433` plus `ViewerSetup.cs:361` establish the house pattern. It is nevertheless **not
recommended for this child**, for four reasons:

1. It changes production behaviour in a bug-fix child whose mandate is test determinism. On a
   handle-less control the guard would silently run UI mutation on the calling thread instead of
   throwing; that is a real behavioural change, not an annotation.
2. It would make the pump-hosted `Initialize(bool)` test pass **without ever exercising a real
   `Control.Invoke`**, which is a coverage regression in substance even if not in line count —
   precisely the outcome epic Hard Constraint 3 exists to prevent.
3. The `IItemViewer` UI-thread seam consolidation is explicitly out of scope: epic.md:75-77 assigns
   `IItemViewer`/`ItemViewer` rework (#489) to the third epic's ItemViewer child.
4. **Its natural test home is full.**
   `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` is 497 lines against the
   500-line cap. See Q6.

If a later reviewer disagrees, (f) should be raised as its own issue against `InvokeBeginInvoke`,
not folded in here.

### Why (a) is not a prohibited timing hack

`.claude/rules/csharp.md:95` prohibits "Adding sleeps, retries, or timing hacks to mask flaky
behavior." The distinguishing test is *whether the race still exists after the change*:

- A sleep, a retry, or a timing tolerance leaves the race in place and lowers the probability of
  observing it. The failure remains reachable; only its frequency changes.
- Reading `viewer.Handle` on the pump thread before the act **eliminates the precondition of the
  failure**. After it, `IsHandleCreated` is `true` unconditionally and for the whole lifetime of
  the fixture, on every machine, at every load level. There is no residual window in which the
  test can fail for this reason, so there is nothing left to mask.

It is also not a wall-clock wait, not probabilistic, and not order-dependent — the three properties
the determinism rules in `.claude/rules/general-unit-test.md` ("Determinism Infrastructure") care
about. `Tags.Test/TagControllerRendering.StaTests.cs:12-17` records that the maintainer already
accepted exactly this reasoning for exactly this instrument. The spec must nevertheless state this
reading explicitly, because #571's own "Suspected Cause / Notes" (`:99-103`) asserts the opposite
("Adding a sleep, a retry, or a handle-forcing call would violate the 'Prohibited Behaviors'
section"). That sentence is the one place where the promoted record and the epic disagree; the
epic's reading (epic.md:117-121) governs, and this document supplies the argument it asks for.

### Known side effect of (a) that the plan must anticipate

Forcing the `ItemViewer`'s handle flips currently-`false` `InvokeRequired` guards to `true` whenever
they are evaluated off the pump thread. Two are on the paths under test:

- `Theme.cs:433` `_lblItemNumber.InvokeRequired` — evaluated during
  `InitializeGraphicsAsync`'s `SetThemeDark(async: false)`, which resumes on a thread-pool thread
  after `await Task.Run(...)` (`Initialization.cs:266-275`). It will now marshal to the pump
  thread via `_lblItemNumber.Invoke` (`Theme.cs:435`) instead of running inline.
- `ViewerSetup.cs:361` `_itemViewer.InvokeRequired` in `AssignControls`, reached from
  `PopulateControlsAsync` → `AssignControlsAsync` (`ViewerSetup.cs:342-356`).

Both should succeed, because a live pump is precisely what the fixture provides, and both become
*more* production-faithful. But this is a genuine behaviour change in the tests and is the most
likely source of a surprise during execution. The plan should treat "tests 1, 2, 5, 6 still pass
after the handle is forced" as an explicit acceptance criterion, not an assumption.

---

## Q4 — Blast radius of changing `WinFormsPumpHost`

The epic's count of "eight consumer tests plus thirteen self-tests" is **verified exact**.

### Consumers (8)

| # | Test method | file:line | Uses `BuildPumpHarnessAsync`? | Host construction |
| --- | --- | --- | --- | --- |
| 1 | `InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState` | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs:40` | Yes (`:47`) | `:43` |
| 2 | `InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme` | `...Part3.cs:83` | Yes (`:90`) | `:86` |
| 3 | `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` | `...Part3.cs:131` | Yes (`:138`) | `:134` |
| 4 | `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` | `...Part3.cs:175` | Yes (`:183`) | `:179` |
| 5 | `InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults` | `...Part3.cs:245` | Yes (`:252`) | `:248` |
| 6 | `CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController` | `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs:305` | Yes (`:313`) | `:308` |
| 7 | `CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing` | `...SeamFactoryTests.cs:376` | Yes (`:384`) | `:379` |
| 8 | `ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups` | `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs:426` | **No** — builds its own viewer at `:432-435` | `:429` |

Consumer 8 is the outlier that matters for remedy (a): it does **not** go through
`BuildPumpHarnessAsync`, so it does not take `UiThreadDispatcherGate` and it will not receive a
forced handle if the change is made only in `BuildPumpHarnessCoreAsync`. It also does not currently
need one (Q2 row 6), but leaving it asymmetric is a latent trap. The plan should either force the
handle in both places or record explicitly why consumer 8 is exempt.

### Self-tests (13), all in `QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs`

`:32`, `:59`, `:88`, `:115`, `:153`, `:183`, `:218`, `:270`, `:302`, `:334`, `:367`, `:395`, `:416`.

### Which assertions would break under a host change

I read all 13. **None asserts the absence of a handle**, and none asserts on any pump-host internal
beyond its public surface. What they do assert:

- `:38-50` — `SyncContext` is non-null and is a `WindowsFormsSynchronizationContext`; `ThreadId`
  differs from the MSTest thread. *Unaffected by any remedy.*
- `:74`, `:101`, `:134-140`, `:168`, `:194-199`, `:230`, `:249-255` — work runs on `host.ThreadId`.
  *Unaffected.*
- `:284-287`, `:317-320`, `:349-352` — exception identity and message from faulted work.
  *Unaffected.*
- `:380-386` — every posting member faults with `ObjectDisposedException` after `StopAsync`.
  *Unaffected by (a); would need review under (d)/(e) because shutdown ordering changes.*
- `:405` — `Dispose` is idempotent. *Unaffected by (a); under (d) the `MainForm` closing would
  itself end the loop, changing this path.*
- `:437-440` — `StopAsync` rethrows an exception recorded by `Application.ThreadException`.
  *Unaffected by (a); under (d)/(e) a `MainForm`/anchor changes what the loop owns at shutdown.*

**Conclusion: remedy (a) has a blast radius of zero on the 13 self-tests and touches one shared
fixture method plus, optionally, one standalone test's arrange block.** Remedies (d) and (e) would
require re-reading the shutdown self-tests. This asymmetry is a further argument for (a).

---

## Q5 — Coverage justifications that must not be deleted (re-derived)

**Correction to the delegation premise: these line numbers have NOT drifted.** All seven cited
positions in the epic and in my instructions are exact against the current worktree. What is wrong
is that the enumeration is *incomplete* — `QfcItemController.Initialization.cs` carries **seven**
de-exemption comment blocks, not five. The epic's five are exactly the lines on which the literal
string `WinFormsPumpHost` appears; two further blocks depend on the pump seam without naming it.

### `QuickFiler/Controllers/QfcItemController.Initialization.cs`

| Block | Lines | Member (line) | Quote (first line) | Depends on |
| --- | --- | --- | --- | --- |
| A **(not in the epic list)** | 135-137 | private nine-arg `Initialize` (`:138`) | "#230: de-exempted. The overload funnels into Initialize(bool); the former barrier was the missing WinForms message pump for that body, not headless construction. Covered by QfcItemController_InitializationTests.InitializeNineArgOverload_ThroughThePumpHost_*." | Consumer **4** — one of the two failing tests |
| B | 164-167 (epic cites `:166`) | `Initialize(bool async)` (`:168`) | "#230: de-exempted. The orchestration runs against a real ItemViewer and its tail dispatches InitializeWebViewAsync through the viewer's WPF dispatcher; both require a live message loop, which the WinFormsPumpHost test seam supplies. Covered by QfcItemController_InitializationTests.InitializeBool_ThroughThePumpHost_*." | Consumer **3** — the other failing test |
| C **(not in the epic list)** | 196-201 | `InitializeAsync()` (`:202`) | "#230: de-exempted. The former barrier was the missing WinForms message pump for this orchestration, not headless construction. Covered by QfcItemController_InitializationTests.InitializeAsync_ThroughThePumpHost_*, which runs every line and asserts the controlled fault at the mocked web-view seam." | Consumer **5** |
| D | 259-262 (epic cites `:261`) | `InitializeGraphicsAsync()` (`:263`) | "#230: de-exempted. The former barrier was the missing WinForms message pump, not headless construction: the orchestration marshals through the concrete ItemViewer's WinForms context. The WinFormsPumpHost test seam supplies that loop, so the member is covered by ...InitializeGraphicsAsync_ThroughThePumpHost_*." | Consumer **2** |
| E | 291-294 (epic cites `:293`) | `InitializeSequentialAsync()` (`:295`) | same wording as D, "...covered by ...InitializeSequentialAsync_ThroughThePumpHost_*." | Consumer **1** |
| F | 403-408 (epic cites `:404`) | `CreateAsync(...)` (`:409`) | "#230: de-exempted. The optional seam parameters below give the factory the injection point it previously lacked, and the WinFormsPumpHost test seam supplies the message loop InitializeAsync needs. Covered by QfcItemController_SeamFactoryTests.CreateAsync_WithFaultingWebViewSeam_*..." | Consumer **7** |
| G | 447-450 (epic cites `:448`) | `CreateSequentialAsync(...)` (`:451`) | "#230: de-exempted. The optional seam parameters below give the factory the injection point it previously lacked, and the WinFormsPumpHost test seam supplies the message loop InitializeSequentialAsync needs. Covered by QfcItemController_SeamFactoryTests.CreateSequentialAsync_WithInjectedSeams_*." | Consumer **6** |

### `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`

| Block | Lines | Member (line) | Nature | Depends on |
| --- | --- | --- | --- | --- |
| H | 30-40, with `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` at `:41` (epic cites `:31`) | `InitializeWebViewAsync()` (`:42`) | **A RETAINED exemption**, not a de-exemption. Quote: "Residual, retained. #230 resolved the pump barrier: the `await _itemViewer.UiSyncContext` on line 55 is now drainable by the WinFormsPumpHost test seam, and tests do reach the IWebViewCoreInitializer seam call. The RESIDUAL barrier is the ((ItemViewer)_itemViewer).L0v2h2_WebView2.CoreWebView2 dependency below..." | Consumers 5 and 7 reach the seam call; the attribute stays |
| I | 254-257 (epic cites `:256`) | `ResolveControlGroupsAsync(ItemViewer)` (`:258`) | De-exemption. Quote: "#230: de-exempted. The former barrier was the missing WinForms message pump - the member awaits itemViewer.UiSyncContext, which never resumes on a thread-pool MSTest thread. The WinFormsPumpHost test seam supplies that loop, so the member is now covered by QfcItemController_ViewerSetupTests.ResolveControlGroupsAsync_ThroughThePumpHost_*." | Consumer **8** |

Note on H: the internal reference "on line 55" inside the comment is still accurate —
`ViewerSetup.cs:55` is `CoreWebView2EnvironmentOptions options = new("–incognito ");` and the
`await _itemViewer.UiSyncContext;` is at `:58`. That internal cross-reference is off by three and
should be left alone (it is out of scope and rewriting it invites churn), but a reader should not
be surprised by it.

**Evidence value.** Every one of the eight pump-hosted consumer tests is the named coverage evidence
for at least one de-exempted production member. Deleting, `[Ignore]`-ing, or reclassifying any of
them out of the unit suite invalidates the corresponding comment and re-opens the exemption
question for that member. That is the concrete content of epic Hard Constraint 3, and it is why
#511's literal proposed remedy ("replace the real pump with an injectable synchronization-context
seam", `511.md:62`) must not be executed as written.

Also note that `QuickFiler/Viewers/ItemViewer.cs:20` carries a whole-type
`[ExcludeFromCodeCoverage]`, so the viewer itself contributes nothing to the coverage denominator;
the entire coverage value of the pump-hosted tests is in `QfcItemController`.

---

## Q6 — The 500-line cap

The cap is stated in `.claude/rules/general-code-change.md` ("File Size Limit ... No production
code, test code, or reusable script file may exceed **500 lines**") and in `CLAUDE.md` § General
Code Change Policy 4.1.

### Verified current sizes and headroom

| File | Last line | Headroom to 500 |
| --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | **497** | **3** |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 467 | 33 |
| `QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs` | 443 | 57 |
| `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` | 436 | 64 |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | 409 | 91 |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` | 290 | **210** |
| `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | 326 | 174 |
| `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` | 482 | 18 |

`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` at **482 lines** is a new finding worth flagging:
only 18 lines of headroom in the host itself. Remedies (d) and (e), which add members to the host,
have very little room; remedy (a), which does not touch the host at all, has none of that problem.

### No wildcard include — confirmed

`QuickFiler.Test/QuickFiler.Test.csproj` is a legacy non-SDK project with explicit `<Compile Include>`
entries only. Grep for `Compile Include="**` and for `*.cs` glob patterns returns nothing; the
relevant entries are literal paths:

```
145:    <Compile Include="Controllers\QfcItemController.InitializationTests.cs" />
146:    <Compile Include="Controllers\QfcItemController.InitializationTests.Part2.cs" />
147:    <Compile Include="Controllers\QfcItemController.InitializationTests.Part3.cs" />
159:    <Compile Include="TestSupport\WinFormsPumpHost.cs" />
160:    <Compile Include="TestSupport\WinFormsPumpHostTests.cs" />
```

**Therefore no new test file can be added without editing the csproj, and the csproj is off-limits
for this child** (epic.md:136-139: "#511/#571 and #445 add no compile entry").

### Can the regression tests fit?

Yes, comfortably, **provided they are placed in `Part3.cs`**.

Recommended placement, with 210 lines of headroom in `Part3.cs`:

1. `BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` — asserts
   `harness.Viewer.IsHandleCreated` is `true` and, queried from the pump thread,
   `harness.Viewer.InvokeRequired` is `false`. ~35 lines with the required XML doc.
2. `InitializeBool_ThroughThePumpHost_ReachesControlInvokeWithoutThrowing` — a focused regression
   for #571 asserting that `ToggleTips(async: false, ...)` through `InvokeBeginInvoke` completes.
   ~40 lines. (Arguably subsumed by existing consumer 3, but a named regression test is what the
   Bugfix Workflow in `CLAUDE.md` requires.)
3. Optionally `BuildPumpHarness_DoesNotCreateTheWebViewChildHandles` — asserts the two WebView2
   children remain handle-less after the fix, pinning the minimality property from Q3. ~30 lines.

Total ≈ 105 lines against 210 available. `WinFormsPumpHostTests.cs`'s 57 lines of headroom are
**not** needed under remedy (a), because the host is unchanged. That is a further argument for (a):
under (d)/(e) the host would change, self-tests would need to be added, and 57 lines is thin for
two documented MSTest methods.

**If a future decision requires touching `InvokeBeginInvoke` (remedy (f)), its natural test home
`FocusAndThemeTests.cs` has 3 lines of headroom and cannot absorb a test.** The options would be to
put the test in `Part3.cs` (acceptable but poorly located), or to split `FocusAndThemeTests.cs`,
which requires a csproj compile entry and is therefore blocked for this child. This is a concrete,
independent reason to defer (f) to its own issue.

---

## Q7 — Load-flakiness beyond the handle

**Answer: the missing handle explains #571 but does NOT fully explain #511. There is a second,
independent load-sensitivity, and it is an amplifier rather than a root cause.**

### Timeout inventory

| Constant | Value | file:line | Applied to |
| --- | --- | --- | --- |
| `PumpTimeoutMs` | **60000** (60 s) | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs:38` | `Part3.cs:39,82,130,174,244` |
| `PumpTimeoutMs` | **60000** | `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs:34` | `:425` |
| `PumpTimeoutMs` | **60000** | `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs:293` | `:304,375` |
| `TimeoutMs` | **30000** (30 s) | `QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs:24` | all 13 self-tests |

**Judgement on adequacy.** 60 s for a pump-hosted initialization and 30 s for a host self-test are
generous in absolute terms and are documented as harness bounds, not waits
(`InitializationTests.cs:32-37`, `WinFormsPumpHostTests.cs:16-20`). Under sustained ~96% CPU with
coverage instrumentation attached, a full `ItemViewer` construction plus `MailItemHelper`
materialization plus theme setup is not obviously inside 60 s, but I have no measurement and will
not assert one. The values are defensible; the **failure mode when they fire** is the problem.

### No sleeps, no retries, no wall-clock waits — confirmed

Grep for `Thread.Sleep`, `Task.Delay`, `SpinWait` in `WinFormsPumpHost.cs` and
`WinFormsPumpHostTests.cs` returns nothing. This is consistent with `BannedSymbols.txt` enforcement
described in `.claude/rules/csharp.md` § Analyzer Stack. The host's waits are all on deterministic
signals.

### Genuine blocking waits (all unbounded, all on completion signals)

| Site | Call | Thread | Risk |
| --- | --- | --- | --- |
| `WinFormsPumpHost.cs:60` | `_ready.Wait()` | MSTest | Blocks until the pump thread sets `_ready` in a `finally` (`:315`). Unbounded, but the `finally` is unconditional. |
| `WinFormsPumpHost.cs:65` | `_thread.Join()` (startup-failure path) | MSTest | Reached only when `_initializationError != null`, which returns immediately at `:318-321`. |
| `WinFormsPumpHost.cs:240` | `StopAsync().GetAwaiter().GetResult()` in `Dispose` | MSTest | **Sync-over-async.** Safe only because the host's contract (`:22-24`) guarantees no `SynchronizationContext` is installed on the MSTest thread. Exercised by `WinFormsPumpHostTests.cs:35` (`using`) and `:401`. |
| `WinFormsPumpHost.cs:264` | `_thread.Join()` in `StopCoreAsync` | Continuation | Unbounded; depends on the loop having exited, which `_stopped.Task` at `:263` already proves. |
| `Part2.cs:67` | `UiThreadDispatcherGate.WaitAsync()` | MSTest | **Unbounded and process-wide.** See below. |

### The real load amplifier: `[Timeout]` plus the process-wide gate

`UiThreadDispatcherGate` (`Part2.cs:51`) is a `SemaphoreSlim(1, 1)` acquired at `:67` and released
in exactly two places: the catch block at `:72-76` (construction failure) and
`PumpHarness.Restore` at `:341`, which every consumer calls from `finally`.

MSTest's `[Timeout]` on a `Task`-returning test does **not** abort the test's continuation; it
records a failure and moves on while the underlying task keeps running. Consequences under load:

1. A pump test that overruns 60 s is reported failed, but its `finally` — and therefore
   `Restore()`, the `SwapUiThreadDispatcher` rollback (`Part2.cs:139-149,339`), and the gate release
   — has not yet run.
2. The next pump test in either `QfcItemController_InitializationTests` or
   `QfcItemController_SeamFactoryTests` blocks on `WaitAsync()` at `:67` for up to its own 60 s.
3. Meanwhile the timed-out test's `Restore()` may fire mid-flight and revert the process-wide
   static `UtilitiesCS.UiThread._dispatcher` out from under the newly started test — the exact
   hazard the gate's own doc comment at `Part2.cs:36-46` describes.

That is a **cascade**: one load-induced overrun converts into several correlated failures, which
matches #511's report that six full-suite attempts were needed for one clean baseline far better
than a single flaky assertion would.

Two aggravating details:

- `QfcItemController_ViewerSetupTests.ResolveControlGroupsAsync_ThroughThePumpHost_*`
  (`ViewerSetupTests.cs:426`) constructs its own `ItemViewer` on its own pump host and **never takes
  the gate**, so it runs concurrently with the gated tests under class-level parallelization. It
  does not swap the static dispatcher, so it is not a correctness hazard today, but it does add a
  third live message pump and a third full `ItemViewer` control tree to the process under load.
- Nine test assemblies share one testhost process under
  `vstest.console.exe ... /InIsolation`, so the pump threads compete with everything else in the
  suite for CPU.

### What this child can and cannot fix

- Fixing the handle (Q3 remedy (a)) removes #571 entirely and removes one whole class of #511's
  failures.
- The `[Timeout]`/gate cascade is **not** fixed by the handle. Mitigating it properly means either
  (i) making the gate release exception-safe against a non-running `finally` — which MSTest's
  timeout semantics make hard — or (ii) serializing the pump-hosted tests at the framework level
  rather than with a semaphore, e.g. by `[DoNotParallelize]` on the classes that share the static.
  Option (ii) is a small, honest change with no timing content and is worth costing in the spec.
- The remaining CPU-contention sensitivity of running three real message pumps under 96% load is
  inherent to keeping the pump-hosted coverage. It should be stated as a residual, not silently
  claimed as fixed.

---

## Reconciliation of #511 and #571

### Recommended direction

**Keep the real message pump. Make the fixture deterministic. Re-scope #511's visible-window claim
to the evidence.**

Concretely:

1. In `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs`,
   `BuildPumpHarnessCoreAsync` (`:79-132`), immediately after the viewer is constructed on the pump
   thread at `:84`, force the viewer's window handle **on the pump thread** by reading
   `viewer.Handle` inside the same `host.InvokeAsync` factory (or a second `InvokeAsync`), with a
   comment citing `Tags.Test/TagControllerRendering.StaTests.cs:39-41` and stating why this is not a
   timing hack.
2. Apply the same one line to the standalone consumer at `ViewerSetupTests.cs:432-435`, or record
   why it is exempt.
3. Add the regression tests to `Part3.cs` (210 lines of headroom), not to a new file.
4. Consider `[DoNotParallelize]` on `QfcItemController_InitializationTests` and
   `QfcItemController_SeamFactoryTests` to close the `[Timeout]`/gate cascade of Q7.
5. Change **no** production file. Change **no** `.claude/**` file. Change **no** `.csproj`.
6. Re-attribute #511's visible-window symptom to
   `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs:73` and promote it to its own issue through
   the promotion lifecycle. Do not claim a fix this child cannot make.

### Argument for

- It is the only direction that satisfies epic Hard Constraint 3: all eight pump-hosted consumer
  tests survive, and all nine coverage-justification blocks (seven de-exemptions in
  `Initialization.cs`, one de-exemption and one retained exemption in `ViewerSetup.cs`) stay true.
- It removes the race rather than reducing its probability, which is the distinction
  `.claude/rules/csharp.md:95` actually draws.
- It has an in-repo, maintainer-ratified precedent for the exact instrument, with an explicit
  no-visible-window assertion attached (`Tags.Test/TagControllerRendering.StaTests.cs:12-17,39-41`).
- Its blast radius on the 13 self-tests is zero, and it needs none of
  `WinFormsPumpHost.cs`'s scarce 18 lines of headroom.
- It is one line of behaviour in a shared fixture, which is the smallest change that can work.

### Argument against the main alternative

The main alternative is #511's literal proposal: **replace the real pump with an injectable
synchronization-context / dispatcher seam, and move any irreducible cases out of the unit suite**
(`511.md:62`).

Against it:

- **It deletes the evidence it is supposed to protect.** Every one of the nine justification blocks
  named in Q5 says, in terms, that the member is covered *because the pump seam supplies a live
  message loop*. Replacing the pump with a fake context makes `await _itemViewer.UiSyncContext`
  (`ViewerSetup.cs:269`, `:58`) resume on a synthetic context, which is a different behaviour from
  the one those members were de-exempted for. Reclassifying the tests out of the unit suite deletes
  the coverage outright.
- **The seam it proposes already exists and is already used.** `IItemViewer` re-declares
  `InvokeRequired`, `Invoke`, `BeginInvoke` at `QuickFiler/Viewers/IItemViewer.cs:135-137`
  specifically for mockability, and `UtilitiesCS.Threading.IUiDispatcher` is held at
  `QfcItemController.cs:66`. Both are exercised without any pump at
  `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs:99-115`
  (`BuildExecutingViewer`) and throughout `QfcItemController.ConversationTests.cs` (e.g. `:216`,
  `:329`). The pump-hosted tests exist *precisely because* the seam-mocked tests do not exercise the
  concrete `ItemViewer` control tree. Adding a third seam would duplicate coverage that already
  exists while destroying coverage that does not.
- **It does not fix the thing it was filed for.** Q1 shows the visible window is not the pump's.
  Replacing the pump would leave `ProgressViewer_Tests.cs:73` showing a window on every full-suite
  run.
- **Cost and risk are an order of magnitude higher.** It rewrites a 482-line test-support type with
  18 lines of headroom, touches all eight consumers and all 13 self-tests, and would require csproj
  edits that this child is forbidden to make.

The honest statement of the trade: the alternative buys a suite with no real message loop, which is
genuinely more robust under CPU contention, at the cost of nine coverage justifications and a
rewrite that this child is not scoped or permitted to perform. If the maintainer later decides the
pump must go, that is a separate, larger piece of work and it must be preceded by an explicit
decision about what happens to the de-exempted members — not folded into a determinism fix.

---

## Line-number drift corrections

Every `file:line` citation from `511.md`, `571.md`, and `epic.md`, checked against the worktree.

| Source | Citation | True current position | Status |
| --- | --- | --- | --- |
| `571.md:61` | `QfcItemController.FocusAndTheme.cs:256` (`_itemViewer.Invoke`) | `:256` | **Exact** |
| `571.md:63,90` | `QfcItemController.FocusAndTheme.cs:204` (`ToggleTips` → `InvokeBeginInvoke`) | `:204` | **Exact** |
| `571.md:86` | `QfcItemController.FocusAndTheme.cs:256` (`InvokeBeginInvoke` calls `IItemViewer.Invoke`) | method at `:248`, call at `:256` | **Exact** |
| `571.md:95-98` | "a sibling test in the same file already documents this hazard and works around it with a headless `ProgressTrackerPane` built via `FormatterServices.GetUninitializedObject` (see the comment block in `UtilitiesCS.Test/Extensions/AsyncSerialization_Tests.cs`)" | **Premise false.** That file contains no `GetUninitializedObject` call; its only `ProgressTrackerPane` uses are `(ProgressTrackerPane)null!` at `:73` and doc text at `:286,288`. The `GetUninitializedObject` pattern lives at `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs:33-34` (`CreateHeadlessViewer`). Also `ProgressTrackerPane` is not a control at all (`UtilitiesCS/Threading/ProgressTrackerPane.cs:9` — `: IProgress<...>`). | **Wrong file; wrong type** |
| `571.md:99-103` | "Adding a sleep, a retry, or a handle-forcing call would violate the 'Prohibited Behaviors' section of `.claude/rules/csharp.md`" | `.claude/rules/csharp.md:95` reads "Adding sleeps, retries, or timing hacks to mask flaky behavior." It does not name handle forcing. `epic.md:117-121` governs and permits it. | **Overstated** |
| `511.md:21,58` | `UtilitiesCS.Test/Threading/WpfUiDispatcherTests.cs` `StaDispatcherHost` | `:161`. The `WpfDispatcherYieldTests` suite it also names is at `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs:13`, with its own `StaDispatcherHost` at `:172`. | **Both exist; two different files** |
| `511.md:37` | "A visible window appeared during the run, because the host constructs a real WinForms control and pumps a real message loop" | Causal claim unsupported. See Q1. | **Re-attributed** |
| `epic.md:60` | `QfcItemController.FocusAndTheme.cs:256` | `:256` | **Exact** |
| `epic.md:104,225` | `QfcItemController.Initialization.cs:166, 261, 293, 404, 448` | `:166`, `:261`, `:293`, `:404`, `:448` all land inside the intended comment blocks | **Exact but incomplete** — two further de-exemption blocks at `:135-137` and `:196-201` are omitted |
| `epic.md:105,226` | `QfcItemController.ViewerSetup.cs:31, 256` | `:31` (inside the 30-40 **retained**-exemption block) and `:256` (inside the 254-257 de-exemption block) | **Exact**; note `:31` is a retained exemption, not a de-exemption |
| `epic.md:109` | `QuickFiler/Controllers/QfcItemController.cs:51` (`IItemViewer _itemViewer`) | `:51` | **Exact** |
| `epic.md:112` | `QuickFiler/Controllers/QfcItemController.cs:66` (`IUiDispatcher _uiDispatcher`) | `:66` | **Exact** |
| `epic.md:110` | `QuickFiler/Viewers/IItemViewer.cs:95-100` (`Invoke`/`BeginInvoke`/`InvokeRequired` re-declaration) | **`:135-137`** (`InvokeRequired` `:135`, `Invoke` `:136`, `BeginInvoke` `:137`), inside `#pragma warning disable CS0108` at `:134`-`:139`; `int Height` at `:138` | **Drifted, +40** |
| `epic.md:113` | `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs:99-115` (`BuildExecutingViewer`) | `:99-115` | **Exact** |
| `epic.md:92` | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs:51` (`UiThreadDispatcherGate`) | `:51` | **Exact** |
| `epic.md:117` | `.claude/rules/csharp.md:95` | `:95` | **Exact** |
| `epic.md:129-131` | `QuickFiler.Test/QuickFiler.Test.csproj:161-165` (Form1 compile) and `:180-181` (Form1.resx) | `:161`, `:164`, `:165`; `:180`, `:181` | **Exact** |
| `epic.md:125` | "116 explicit `<Compile Include>` entries" | Not recounted (out of scope); the structural claim — explicit entries, no wildcard — is **verified** | **Structure confirmed** |
| `epic.md:56` | "Eight consumer tests plus thirteen self-tests" | 8 consumers and 13 self-tests, enumerated in Q4 | **Exact** |
| `epic.md:245-247` | `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` "exactly at the 500-line cap" | Not verified (outside this child's scope) | **Unchecked** |
| Delegation prompt | "`ItemViewer.cs:21` — `public partial class ItemViewer : UserControl, IItemViewer, IContainerControlLocal`" | `:21`; note the `[ExcludeFromCodeCoverage]` at `:20` | **Exact** |
| Delegation prompt | `Part3.cs` 290 / `Part2.cs` 409 / `WinFormsPumpHostTests.cs` 443 / `FocusAndTheme.cs` 326 | Confirmed (last closing brace on each of those lines) | **Exact** |
| Delegation prompt | "These line numbers have drifted — re-derive them" (re: the Q5 coverage justifications) | **They have not drifted.** All seven positions are exact. | **Premise corrected** |

Additional drifted citation found in a code comment, recorded for awareness but **out of scope**:
`QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs:119` cites
`Theme.cs:414-432` for `SetQfcTheme(bool)`; the member is now at
`UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs:427-445`. Do not fix it in this child (it is in a
497-line file with three lines of headroom and belongs to no issue here).

---

## Testing implications (no test code written)

Consistent with `CLAUDE.md` § General Unit Test Policy, `.claude/rules/general-unit-test.md`, and
the C# Unit Test Policy (MSTest + Moq + FluentAssertions, no temporary files).

1. **Bugfix workflow.** `CLAUDE.md` § Bugfix Workflow requires a failing regression test first. For
   #571 the failing test can be written as an assertion on the harness invariant
   (`harness.Viewer.IsHandleCreated`), which fails deterministically before the fix and passes
   after — a better regression than relying on the intermittent end-to-end symptom.
2. **Placement.** All new tests go in
   `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` (210 lines of
   headroom). No new file, therefore no `.csproj` edit.
3. **Scenario coverage** for the new fixture behaviour: positive (handle created on the pump
   thread), boundary (`InvokeRequired` is `false` when queried *on* the pump thread and `true` when
   queried off it), and minimality (the WebView2 children remain handle-less). The negative case —
   `Control.Invoke` throwing without a handle — should **not** be added as a new test, because it
   would assert framework behaviour rather than repository behaviour.
4. **Regression scope for execution.** All eight consumer tests and all 13 self-tests must be run
   and must pass, not just the two named in #571, because forcing the handle flips
   `InvokeRequired` guards on the other paths (Q3, "Known side effect").
5. **Determinism evidence for #511.** The leading indicator in `epic.md:13` is ten consecutive
   green full-suite runs under induced CPU load. That is an evidence artifact, and it belongs under
   `docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/`
   per the evidence-location invariant. No `artifacts/` sub-path other than
   `artifacts/orchestration/` may hold it.
6. **Run command.** `vstest.console.exe <assemblies> /EnableCodeCoverage /InIsolation
   /TestCaseFilter:"TestCategory!=LiveOutlook"`, with `\.claude\` excluded from recursive
   `*.Test.dll` discovery (epic.md:216-221). Omitting `/InIsolation` fabricates roughly 1,695
   phantom failures.
7. **Coverage.** `QuickFiler/Viewers/ItemViewer.cs:20` is `[ExcludeFromCodeCoverage]` for the whole
   type, so the fixture change moves no coverage. `QfcItemController` coverage must not regress;
   the nine justification blocks of Q5 are the checklist.

---

## Open questions / unverifiable without execution

I could not run a build or a test. The following are genuinely open.

1. **The #571 intermittency mechanism is unexplained (highest-value open question).** Static
   reading says consumers 3 and 4 must fail on every run, because `Control.Invoke` throws
   unconditionally without a handle and I found no handle-creating call in
   `ResolveControlGroups` → `SetupThemes` → `PopulateControls`. #571 records them passing on some
   runs. Cheapest disambiguation: run
   `/TestCaseFilter:"FullyQualifiedName~QfcItemController_InitializationTests"` and, in the same
   run, assert `harness.Viewer.IsHandleCreated` at the top of consumer 3. If it is `false` and the
   test still passes, my reading of `Control.Invoke` is wrong; if it is sometimes `true`, something
   third-party creates the handle and that something must be identified before the fix is called
   minimal. **Either outcome leaves remedy (a) correct**; only the *explanation* in the spec
   changes.
2. **WebView2 implicit initialization.** Whether
   `Microsoft.Web.WebView2.WinForms.WebView2`'s `ISupportInitialize.EndInit`,
   `OnParentChanged`, `OnVisibleChanged`, or `OnHandleCreated` can create a window on a parentless,
   handle-less control. Third-party code not present in this repository. Verification: after
   forcing `viewer.Handle`, assert
   `viewer.L0v2h2_WebView2.IsHandleCreated == false` and
   `viewer.L0vhBreadcrumb_WebView2.IsHandleCreated == false`. This is proposed as regression test 3
   in Q6.
3. **Whether reading `.Handle` on `ItemViewer` really leaves children handle-less.** This follows
   from `Control.CreateHandle` being non-recursive while `Control.CreateControl` is recursive.
   High confidence from documented `Control` semantics, but not executed. Same verification as (2).
4. **Whether `Application.Run(ApplicationContext)` shows `context.MainForm`.** Asserted at
   medium-high confidence in Q3 remedy (d). Not executed. Only matters if someone revives (d).
5. **Whether `PumpTimeoutMs = 60000` is actually adequate under ~96% CPU with coverage attached.**
   No measurement exists. #511 records six attempts for a clean baseline but does not record which
   tests failed or with what message. A fresh capture under induced load is called for by
   `511.md:41` and should accompany the fix.
6. **MSTest `[Timeout]` semantics on a `Task`-returning test in this exact MSTest version.** The Q7
   cascade argument depends on the timed-out test's continuation surviving and its `finally`
   running late. This is the documented behaviour for async MSTest tests, but the specific version
   in `QuickFiler.Test` was not confirmed and the cascade was not reproduced.
7. **Whether `[DoNotParallelize]` is available and appropriate.** Proposed in the Reconciliation as
   a Q7 mitigation. Its presence in the referenced MSTest version and its interaction with
   `UiThreadDispatcherGate` were not verified.
8. **The `ProgressViewer_Tests.cs:73` re-attribution was not reproduced.** It is a code reading
   (`ProgressViewer : Form` at `UtilitiesCS/Threading/ProgressViewer.cs:16`, `viewer.Show()` in an
   enabled `[STATestClass]` `[TestMethod]`), not an observation of a window on a desktop. Before
   the follow-up issue is filed, someone should watch a full-suite run and confirm the window is
   the `ProgressViewer`.
9. **Whether the visible window observed on 2026-08-08 was a single event or recurrent.**
   `511.md:41` records that no failure log was retained. If the window is not reproducible, the
   re-attribution in Q1.5 is the best available explanation but is not the only possible one.
