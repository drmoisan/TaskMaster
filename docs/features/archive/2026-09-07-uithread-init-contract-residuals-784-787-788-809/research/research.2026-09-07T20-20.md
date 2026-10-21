# Research — UiThread init-contract residuals (#809, superseding #784 / #787 / #788)

Timestamp: 2026-09-07T20-20

All paths below are repository-relative. Every line number was read in this run against the working
tree unless the claim is explicitly marked `CARRIED FROM PROMPT, NOT RE-VERIFIED` or `UNKNOWN`.

## Verification of the prompt's premises

| Premise stated in the delegation prompt | Status |
|---|---|
| `Init()` at `UtilitiesCS/Threading/UiThread.cs:19-40`, latch read at `:36`, no apartment check | **VERIFIED.** `Init()` occupies lines 19-40; `if (_loaded.CheckAndSetFirstCall)` is line 36; neither `Init()` nor `Initialize()` reads `Thread.CurrentThread.GetApartmentState()`. |
| `Initialize()` at `:48-79` constructs `SyncContextForm`, captures four globals | **VERIFIED.** `new SyncContextForm()` at `:51`, `Show()` at `:54`, `CaptureUiVariables()` at `:57`, assignments at `:58-61`, `Hide()` at `:78`. |
| `ThreadSafeSingleShotGuard.CheckAndSetFirstCall` is an `Interlocked.Exchange` at `UtilitiesCS/Threading/ThreadSafeSingleShotGuard.cs:24-27` | **VERIFIED**, exact lines 24-27, body `Interlocked.Exchange(ref _state, CALLED) == NOTCALLED`. |
| `UiThread.cs:100` is `public bool IsCompleted => _context == SynchronizationContext.Current;` | **VERIFIED** verbatim at line 100. |
| Both lazy getters still call `Init()` | **VERIFIED.** `UiSyncContext` getter: `UiThread.cs:117-120`. `AutoScaleFactor` getter: `UiThread.cs:183-186`, with the `SizeF(1f, 1f)` fallback at `:187`. (The #782 artifacts cite `:128-131` and `:194-197`; those were the line numbers before that delivery's edits. The behaviour is unchanged; only the line numbers moved.) |
| `DictionaryExtensions.cs` line 177 declares the 500 ms budget | **VERIFIED.** `UtilitiesCS/Extensions/DictionaryExtensions.cs:177` is `linkedTS.CancelAfter(500);`, inside `TryAddValuesAsync` (`:169-180`), whose only work is `Task.Run(() => dictionary.TryAddValues(key, value), linkedTS.Token)` at `:179`. |
| `UiThread.cs` carries `#nullable enable` | **VERIFIED**, line 1. |
| Target is .NET Framework, no `init`/`record` | **VERIFIED indirectly**: `UtilitiesCS.Test/packages.config:146-147` pins `targetFramework="net481"`. Not re-derived from `UtilitiesCS.csproj`. |

### The #780 ambiguity (asked for explicitly)

`UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue`
(`UtilitiesCS.Test/Extensions/DictionaryExtensions_Tests.cs:236-249`) **is** the test tracked as the
independent flake under issue #780. `docs/features/potential/promoted/2026-09-04-tryaddvaluesasync-wall-clock-timeout-flaky.md`
names the same fully-qualified test (`:39-41`), the same exception type `TaskCanceledException`, and
the same signature — "the failing test alone took 21 s" (`:47`) — as the #782 regression record.

The #782 record **does** address the ambiguity, but weakly. `docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/other/code-review.2026-09-05T23-00.md:94-100`
records one run with the re-arm line (5179/5180, that test failing at 21 s), one run without it
(5180/5180), and the branch base at 6992/6992 before and after. `evidence/qa-gates/p1-t9-phase1-tests.md:70-82`
adds one subsequent clean run and concludes the failure was "delivery-attributable rather than the
issue #780 flake".

Assessment: the attribution rests on a **single with/without pair** against a test that is
independently documented as intermittent with an identical failure signature. It is plausible but
not statistically established. This matters because the entire "do not re-arm the latch" constraint
depends on it. See R1 for a second, independent reason to doubt the recorded mechanism.

The test body itself touches no `UiThread` member — it is a pure thread-pool canary
(`ConcurrentDictionary<string,int>`, `Task.Run`, 500 ms `CancelAfter`).

---

## R1 — Interaction between AC1 and AC2

### The exact reachable surface

`Init()` is reachable from only three places in the entire tree. Two are direct calls and one is the
pair of lazy getters:

| Path | Site | Apartment |
|---|---|---|
| Direct | `TaskMaster/ThisAddIn.cs:35-40` (`ThisAddIn_Startup`) | Outlook STA |
| Direct | `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs:329` | MSTest default (see R4) |
| Lazy | `UiThread.cs:119` inside the `UiSyncContext` getter | caller's |
| Lazy | `UiThread.cs:185` inside the `AutoScaleFactor` getter | caller's |

`UiThread.Dispatcher` never calls `Init()` (`UiThread.cs:153-171`; the `<remarks>` at `:141-148`
states the asymmetry deliberately). So the AC1 blast radius is bounded by exactly **six** lazy-read
sites in production plus the two direct calls:

`UiThread.UiSyncContext` readers (3):
- `UtilitiesCS/Threading/ThreadMonitor.cs:143`
- `TaskMaster/AppGlobals/AppOlObjects.cs:367`
- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:179`

`UiThread.AutoScaleFactor` readers (3):
- `TaskMaster/ThisAddIn.cs:114`
- `UtilitiesCS/EmailIntelligence/OlFolderTools/FolderRemap/FolderRemapViewer.cs:40`
- `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersViewer.cs:79`

### Testing the hypothesis

**The hypothesis is CORRECT as a mechanism statement, and it is stronger than stated: with AC1 in
place, the expensive path (`new SyncContextForm()` + `Show()`) is unreachable from any non-STA
caller, so a re-armed latch cannot produce repeated WinForms construction on a worker thread.** The
apartment check is a constant-time read of `Thread.CurrentThread.GetApartmentState()` followed by a
throw; the starvation mechanism the #782 artifacts describe requires the form construction, which no
longer runs.

However — and this is the finding the planner most needs — **I could not confirm that the recorded
#782 mechanism was ever real**, for two independent reasons:

1. **Every in-repo path that reaches the lazy `AutoScaleFactor` getter in a test run is already
   STA-hosted.** `UtilitiesCS.Test/EmailIntelligence/FolderRemapViewer_Tests.cs:26-27` carries
   `[STATestClass]`, and its `SetController_WithSyntheticController_ConfiguresTreeDelegates`
   (`:117-131`) calls `viewer.SetController(controller)`, which reaches `SetupRenderer` →
   `UiThread.AutoScaleFactor` (`FolderRemapViewer.cs:38-40`) and asserts `act.Should().NotThrow()`.
   `UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersViewer_Tests.cs:26-27` is likewise
   `[STATestClass]` with the same shape at `:39-51`. On an STA thread `Initialize()` succeeds, the
   latch is consumed once, and no catch ever fires.

2. **I found no test that reaches the lazy `UiSyncContext` getter with a null backing field.** The
   #782 remediation record `evidence/other/r1-r2-maintainer-disposition.2026-09-06T00-15.md:74-76`
   quotes the measured uncovered-line set for `UiThread.cs` as
   `28,29,30,32,33,34,67,68,69,70,71,72,73,74,75,76,118,119,120` — lines 118-120 are exactly the
   `if (_uiSyncContext is null) { Init(); }` block. That block is **uncovered**, i.e. never executed
   with a null field in a measured run. `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs:461-487`
   installs a context into `UiThread._uiSyncContext` by reflection before exercising
   `FolderPredictor.EnterUiContextAsyncAction`, so it bypasses the lazy path entirely.

3. **The #788 entry names `TaskMaster/AppGlobals/AppOlObjects.cs:367` and `TaskMaster/ThisAddIn.cs:114`
   as "the readers that make `TaskMaster.Test` the assembly where this surfaces"
   (`docs/features/potential/promoted/2026-09-05-uithread-init-latch-not-rearmed-after-failed-initialize.md:62`).
   I searched `TaskMaster.Test` for `AppOlObjects`, `UserEmailAddress`, `ResolveCurrentUser`,
   `SetUpBrightIdeasSettings` and found no test that drives either reader** (`TaskMaster.Test`
   exercises `AppOlObjects.TryGetSmtpAddress`, `ReadJunkPotentialSetting`,
   `EmitPerStoreInboxAttribution`, and the folder-tree service; not the email-address resolver).
   `ThisAddIn.SetUpBrightIdeasSettings` is a private method on a VSTO type with no test caller.

Taken together: the #782 mechanism narrative requires `Initialize()` to throw at least once in that
test run, and I can find no path in the current tree where it does. Either the tree changed since
`b95a5252`, or the observed 21-second `TaskCanceledException` was the #780 flake after all.

**Conclusion for the planner.** Do not build the design on the assumption that the #782 mechanism is
real, and do not build it on the assumption that it is fake. AC1 removes the mechanism *if it
exists*, which is enough to make the AC2 design safe either way. But AC2's own requirement — "the
#782 regression scenario is reproduced as a test and passes with the chosen design" — cannot be
discharged by pointing at the #782 artifacts; it needs a Phase 0 probe that establishes, by
measurement, whether `Initialize()` throws on an MTA thread in this repository's test host. That one
fact decides whether the AC2 test is a real regression test or a vacuous one.

### The cost of AC1: sites that would newly throw

With an STA precondition evaluated before the latch, a non-STA read of `UiThread.UiSyncContext` or
`UiThread.AutoScaleFactor` **whose backing field is still null** throws `InvalidOperationException`
where today it either silently captures a worker's non-pumping context or returns the
`SizeF(1f, 1f)` fallback (`UiThread.cs:187`). Site-by-site:

| Site | Can it run off the STA thread? | New throw? |
|---|---|---|
| `UtilitiesCS/Threading/ThreadMonitor.cs:143` | **Yes.** Reached from `Tick()` (`:105`), which is a `TimeProvider.CreateTimer` callback started in `Run()` (`:93-102`) — a thread-pool thread. | **No.** `ThreadMonitor` is constructed only at `UiThread.cs:68-74`, inside `Initialize()`, *after* `UiSyncContext` is assigned at `:58`. `_uiSyncContext` is guaranteed non-null on this path, so the getter never calls `Init()`. |
| `TaskMaster/AppGlobals/AppOlObjects.cs:367` | **Yes, by construction.** The enclosing branch is entered only when `Thread.CurrentThread.ManagedThreadId != UiThread.UiThreadId` (`:364`). | **Yes, if `_uiSyncContext` is null.** In production `ThisAddIn.cs:35` runs first, so the field is populated and no throw occurs. In any headless/unit context it would throw. Today it instead constructs a `SyncContextForm` on the worker and then `Send`s inline on that worker, i.e. it performs the COM read on the wrong apartment — the exact failure `:361-363` says it is preventing. **The AC1 throw is strictly better than the current silent-wrong behaviour here.** |
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:179` | **Yes.** `EnterUiContextAsyncAction` is a static default delegate invoked from wherever the predictor runs. | **Yes, if `_uiSyncContext` is null.** No existing test hits it with a null field (`FolderPredictorTests.cs:479` pre-installs the field). |
| `TaskMaster/ThisAddIn.cs:114` | No. `SetUpBrightIdeasSettings` is called from `Application_Startup` (`:61`) on the Outlook STA, after `Init()` at `:35`. | No. |
| `UtilitiesCS/.../FolderRemapViewer.cs:40` | Only if a caller invokes `SetController` off the UI thread. | **No in the current test suite** — the only driver is `[STATestClass] FolderRemapViewer_Tests`. |
| `UtilitiesCS/.../FilterOlFoldersViewer.cs:79` | Same. | **No in the current test suite** — `[STATestClass] FilterOlFoldersViewer_Tests`. |

Net: **three production sites gain a possible new throw** (`AppOlObjects.cs:367`,
`FolderPredictor.cs:179`, and — only in a hypothetical off-UI-thread caller —
`FolderRemapViewer.cs:40` / `FilterOlFoldersViewer.cs:79`), and **zero existing tests are affected**,
because every in-repo lazy-read driver is STA-hosted. The one in-repo caller that AC1 definitively
breaks is `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs:329` (see R5).

---

## R2 — Live-read census

Method: `Grep` over `**/*.cs` for the regular expression
`UiThread\.(UiSyncContext|AutoScaleFactor|Dispatcher|UiThreadId|Init)`. This matches both the bare
`UiThread.` form and the fully-qualified `UtilitiesCS.UiThread.` form. Excluded from the "live" count:
XML documentation, ordinary comments, commented-out code, and the exception-message literal at
`UiThread.cs:136`. A second `Grep` for `using static .*UiThread` returned no matches, so there is no
unqualified read anywhere; the regex is exhaustive for this family.

### Production — 56 live sites across 31 files

| File | Live lines | Count | Can it run off the Outlook STA? Evidence |
|---|---|---|---|
| `TaskMaster/ThisAddIn.cs` | 35 (`Init`), 114 (`AutoScaleFactor`), 227 (`Dispatcher`) | 3 | No. `ThisAddIn_Startup` / `Application_Startup` are VSTO host events on the STA. |
| `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` | 71, 114 | 2 | No. Ribbon `onAction` callbacks are raised by Office on the STA. `Dispatcher` read only — never triggers `Init()`. |
| `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs` | 344 | 1 | Possibly. `UiThread.Dispatcher.CheckAccess()` — `Dispatcher` read, no `Init()`; throws today if uninitialized. |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | 364 (`UiThreadId`), 367 (`UiSyncContext`) | 2 | **Yes, by construction** — `:364` gates the branch on being *off* the UI thread. |
| `TaskMaster/AppGlobals/ApplicationGlobals.cs` | 293 | 1 | `Dispatcher` read for a `DispatcherTimer` heartbeat; started from startup on the STA. |
| `UtilitiesCS/HelperClasses/ToolTips/QfcTipsDetails.cs` | 254, 277 | 2 | Yes (`await ... InvokeAsync`), `Dispatcher` only. |
| `UtilitiesCS/Threading/ThreadMonitor.cs` | 143 | 1 | **Yes** — thread-pool timer callback (`:96-101`, `:105`). `UiSyncContext`. |
| `UtilitiesCS/Threading/ProgressTrackerPane.cs` | 13, 16 | 2 | `Dispatcher` only. |
| `UtilitiesCS/Threading/ProgressTrackerAsync.cs` | 33 | 1 | `Dispatcher` only. |
| `UtilitiesCS/Threading/ProgressTracker.cs` | 33 | 1 | `Dispatcher` only. |
| `UtilitiesCS/HelperClasses/ThemeHelpers/ThemeControlGroup.cs` | 218, 222 | 2 | `Dispatcher` only. |
| `UtilitiesCS/Threading/WpfUiDispatcher.cs` | 25 | 1 | Lazy provider `() => UiThread.Dispatcher`; evaluated on the caller's thread. `Dispatcher` only. |
| `UtilitiesCS/Threading/IdleAsyncQueue.cs` | 72 | 1 | Yes (async continuation). `Dispatcher` only. |
| `UtilitiesCS/Threading/IdleActionQueue.cs` | 78 | 1 | Yes (async continuation). `Dispatcher` only. |
| `UtilitiesCS/HelperClasses/SegmentStopWatch.cs` | 24 | 1 | Yes — constructor runs anywhere. `UiThreadId` is a plain field read; **never triggers `Init()`**. |
| `UtilitiesCS/EmailIntelligence/.../FolderRemapViewer.cs` | 40 | 1 | Caller-dependent. `AutoScaleFactor` — **can trigger `Init()`**. |
| `UtilitiesCS/EmailIntelligence/.../FilterOlFoldersViewer.cs` | 79 | 1 | Caller-dependent. `AutoScaleFactor` — **can trigger `Init()`**. |
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | 179 | 1 | Yes. `UiSyncContext` — **can trigger `Init()`**. |
| `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` | 46 | 1 | Lazy fallback provider. `Dispatcher` only. |
| `QuickFiler/Helper Classes/ItemViewerQueue.cs` | 21, 27, 88, 90 | 4 | Yes (queue schedulers). `Dispatcher` only. |
| `QuickFiler/Helper Classes/EfcViewerQueue.cs` | 20, 67 | 2 | Yes. `Dispatcher` only. |
| `QuickFiler/Helper Classes/ConversationResolver.Loading.cs` | 150, 320 | 2 | Yes (`await`ed loaders). `Dispatcher` only. |
| `QuickFiler/Helper Classes/EmailMoveMonitor.cs` | 44 | 1 | Default `_marshalToSta`; runs on the monitor's thread. `Dispatcher` only. |
| `QuickFiler/Controllers/QfcQueue.cs` | 476, 484, 492 | 3 | Yes. `Dispatcher` only. |
| `QuickFiler/Controllers/QfcHomeController.cs` | 360 | 1 | **Yes** — `Worker_RunWorkerCompleted` is a `BackgroundWorker` completion handler. `Dispatcher` only. |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | 276, 319, 324 | 3 | Yes. `Dispatcher` only. |
| `QuickFiler/Controllers/QfcFormController.Actions.cs` | 255 | 1 | Yes. `Dispatcher` only. |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 951, 982, 1210, 1220, 1238, 1256, 1333 | 7 | Yes. `Dispatcher` only. |
| `QuickFiler/Controllers/KeyboardHandler.cs` | 362, 370, 401 | 3 | Yes. `Dispatcher` only. |
| `QuickFiler/Controllers/EfcItemController.cs` | 998, 1007 | 2 | Yes. `Dispatcher` only. |
| `QuickFiler/Controllers/EfcHomeController.cs` | 297 | 1 | Yes. `Dispatcher` only. |

Excluded as non-live (documentation, comments, commented-out code, message literal):
`TaskMaster/ThisAddIn.cs:190`; `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs:54,93`;
`TaskMaster/Ribbon/EngineToggleStateCoordinator.cs:42`; `TaskMaster/AppGlobals/ApplicationGlobals.cs:159,271`;
`UtilitiesCS/Threading/UiThread.cs:136`; `UtilitiesCS/Threading/SyncContextForm.cs:26`;
`UtilitiesCS/Threading/IUiDispatcher.cs:11`; `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs:57,65`;
`UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs:441`; `QuickFiler/Helper Classes/EmailMoveMonitor.cs:38`;
`QuickFiler/Controllers/QfcQueue.cs:502`; `QuickFiler/Controllers/QfcHomeController.Iteration.cs:31`;
`QuickFiler/Controllers/QfcCollectionController.cs:933`.

### Test — 6 live sites across 3 files

| File | Line | What | Thread |
|---|---|---|---|
| `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs` | 329 | `UiThread.Init(false)` | MSTest worker; plain `[TestMethod]` (`:325-326`) on a class with no `[STATestClass]` — see R4/R5 |
| `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | 139, 144, 165 | `UiThread.Dispatcher` read + `DispatcherNotInitializedMessage` | MSTest worker |
| `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` | 136 | `DispatcherNotInitializedMessage` | MSTest worker |

`QuickFiler.Test/Controllers/QfcHomeControllerTests.cs:170` is a commented-out `UiThread.Init(false);`
and is excluded. All other test-file matches are comments or XML documentation.

### Comparison with the #782 figure

The #782 delivery adopted **49 live reads across 25 production files**, measured at tag
`pre-782-base` (`docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/other/code-review.2026-09-05T23-00.md:156-160`).

**My count does not agree: 56 live sites across 31 production files.** The divergence is +7 sites and
+6 files. I cannot decompose it precisely because the #782 artifact publishes the figure but not its
member set (the same limitation it records against the PR #778 review body's "26 files"). Two
contributing factors are identifiable: (a) my count includes the one production `UiThread.Init(` call
at `TaskMaster/ThisAddIn.cs:35`, which the phrase "live *reads*" may have excluded — subtracting it
gives 55; (b) the tree has moved since `pre-782-base` (for example `QuickFiler/Controllers/QfcHomeController.cs`
gained its issue-#791 `Cleanup()` region at `:370-379`, and `QuickFiler.Test`/`QuickFiler` have taken
several deliveries since). The planner should treat 56/31 as the current figure and should not
restate 49/25.

---

## R3 — Testability seams

### `SyncContextForm` public surface

`UtilitiesCS/Threading/SyncContextForm.cs` — file is 50 lines; namespace is `QuickFiler.Viewers` but
it compiles into the **`UtilitiesCS`** assembly (`UtilitiesCS/UtilitiesCS.csproj:1100-1103`, with the
`.resx` at `:1213-1214`). Declaration at `:16`: `public partial class SyncContextForm : Form`.

| Member | Line | Signature |
|---|---|---|
| `SyncContextForm()` | 18-22 | `public SyncContextForm()` — calls `InitializeComponent()` |
| `FormAutoScaleFactor` | 24 | `public System.Drawing.SizeF FormAutoScaleFactor { get; private set; }` |
| `UiSyncContext` | 28 | `public SynchronizationContext UiSyncContext { get; private set; } = null!;` |
| `UiDispatcher` | 30 | `public Dispatcher UiDispatcher { get; private set; } = null!;` |
| `UiThreadId` | 32 | `public int UiThreadId { get; private set; }` |
| `CaptureUiVariables()` | 34-40 | `public void CaptureUiVariables()` — assigns `SynchronizationContext.Current`, `this.AutoScaleFactor`, `Dispatcher.CurrentDispatcher`, `Thread.CurrentThread.ManagedThreadId` |

Everything else `Initialize()` uses (`ShowInTaskbar`, `WindowState`, `Show()`, `Hide()`) is inherited
from `Form`.

### Seam options for driving a FAILING `Initialize()`

| Option | Verdict | Evidence / consequence |
|---|---|---|
| **Injectable factory delegate for the capture object** (`internal static Func<IUiCaptureSource> SyncContextFormFactory { get; set; }` on `UiThread`, defaulting to the real form) | **RECOMMENDED** | Directly precedented in this repo: `QuickFiler/Helper Classes/ItemViewerQueue.cs:11-27` (`internal static Func<ItemViewer> ProductionViewerFactory { get; set; }` plus three scheduler delegates and `ResetProductionCoreDefaultsForTesting()` at `:83-91`), and `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:161-184` (four `internal static` settable delegates, restored in a `finally` by `FolderPredictorTests.cs:451-458`). A throwing factory makes `Initialize()` fail deterministically with no form, no STA host, no timing. |
| **Interface extracted from `SyncContextForm`** | Workable, heavier | Requires an interface covering `CaptureUiVariables`, the four capture properties, and `ShowInTaskbar`/`WindowState`/`Show`/`Hide`. It changes the public shape of a `Form`-derived type and adds a second production type for one test need. The factory option subsumes it if the factory's return type is a small interface rather than `SyncContextForm` itself. |
| **Injectable apartment-state provider** | Not required for AC1, useful for AC2 | Not needed to test AC1 rejection — MSTest's default apartment is already MTA (R4), so a plain `[TestMethod]` *is* the MTA case and `[STATestMethod]` *is* the STA case. It *is* useful to test "STA caller whose `Initialize()` throws" without needing the factory to distinguish; but the factory alone already covers that. Adding both is redundant. |
| **Internal test hook with `InternalsVisibleTo`** | **Available today** | `UtilitiesCS/Properties/AssemblyInfo.cs:18-20` grants `InternalsVisibleTo` to `DynamicProxyGenAssembly2`, **`UtilitiesCS.Test`**, and `ToDoModel.Test`. Two further duplicate grants to `UtilitiesCS.Test` exist at `UtilitiesCS/HelperClasses/Tokenizer.cs:11` and `UtilitiesCS/OutlookObjects/Item/OlItemSummary.cs:10`. **`QuickFiler.Test` is NOT granted** (the commented-out attempt is at `UtilitiesCS/HelperClasses/ToolTips/QfcTipsDetails.cs:15`, and `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs:27-29` states the same conclusion). Consequence: any `internal` seam on `UiThread` is reachable from `UtilitiesCS.Test` only. All AC4 tests must therefore live in `UtilitiesCS.Test`. |

### Resetting `UiThread`'s process-global statics between MSTest tests

`UtilitiesCS.Test/Threading/UiThread_Tests.cs` (215 lines, read in full) contains two test classes:

- `SynchronizationContextAwaiter_Tests` (`:9-104`) — five `[TestMethod]`s. It does **not** carry
  `[DoNotParallelize]` and does **not** reset any `UiThread` static. Its one context mutation
  (`SetSynchronizationContext` at `:42`) is restored in a `finally` at `:54-57`.
- `UiThread_Dispatcher_Tests` (`:128-214`) — `[TestClass]` + `[DoNotParallelize]` (`:129`), two
  `[TestMethod]`s, plus a private `StaDispatcherHost` (`:186-213`) that owns a real STA thread
  running `Dispatcher.Run()` and shuts it down with `BeginInvokeShutdown` + `Join` on dispose.
  It controls **only `_dispatcher`**, and does so through `UiThreadDispatcherScope`.

Existing static-reset machinery found by grepping `**/*.cs` for the literal field names
`"_loaded"`, `"_uiSyncContext"`, `"_autoScaleFactor"`, `"_syncContextForm"`, `"_uiThreadId"`,
`"_dispatcher"` — exactly four hits:

| Site | Field | Notes |
|---|---|---|
| `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs:117` | `_dispatcher` | Install/restore scope; explicitly documented as **not** thread-safe, relying on `[DoNotParallelize]` on every consuming class (`:19-25`). |
| `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:136` | `_dispatcher` | The QuickFiler.Test equivalent, with a two-lock design (`FieldLock` + `TransactionGate`, `:31-32`) and a parked never-pumping STA dispatcher (`:149-177`). |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs:470` | `_uiSyncContext` | Reads the prior value, installs an `ImmediateSynchronizationContext`, restores in `finally` (`:475-486`). |
| `QuickFiler.Test/Controllers/QfcHomeControllerPropertyTests.cs:335` | `_uiSyncContext` | **Not `UiThread`** — this is an *instance* field of `QfcHomeController` (`BindingFlags.Instance`, `:337`). Irrelevant. |

**No test anywhere resets `_loaded`, `_autoScaleFactor`, `_uiThreadId`, or `_syncContextForm`.** That
gap is the principal AC4 obstacle: a test that drives `Init()` through a failure consumes the
process-global latch for every later test in the assembly.

Recommended shape (see `## Recommended design`): an `internal static void ResetForTesting()` on
`UiThread` that assigns a fresh `ThreadSafeSingleShotGuard` and nulls the five capture fields,
paired with a `UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs` snapshot/restore `IDisposable`
modelled on `UiThreadDispatcherScope`, and `[DoNotParallelize]` on every consuming test class. The
`internal ... ForTesting()` idiom is already repo-precedented at
`QuickFiler/Helper Classes/ItemViewerQueue.cs:69-91`.

---

## R4 — Apartment state in MSTest in this repository

**MSTest's default apartment state for this repository's runs is MTA.** Determined from three
independent pieces of in-tree evidence:

1. `UtilitiesCS.Test/test.runsettings` is a 6-line file whose entire content is a comment plus an
   empty `<RunSettings />`. The comment (`:2-5`) reads verbatim: *"Global STA execution is
   intentionally disabled. Tests that require an STA apartment must opt in with MSTest's
   STATestMethod or STATestClass attributes so the rest of the suite can run under the default
   threading model and participate in parallel execution."*
2. No `.runsettings` in the tree sets `ExecutionThreadApartmentState`. A `Grep` for that token over
   `**/*.{cs,runsettings,config,xml}` returned only prose in archived feature specs
   (`docs/features/archive/2026-07-09-taskvisualization-core-testability-refactor-297/spec.md:384`,
   `.../2026-07-09-tagcontroller-testability-refactor-293/spec.md:334`,
   `.../2026-07-09-tasktree-testability-refactor-296/plan.2026-07-09T16-07.md:114`), each describing
   assembly-wide STA as an option **not** taken. The two runsettings actually used —
   `TaskMaster.runsettings` and `scripts/vscode/TaskMaster.cli.runsettings` — configure only
   `<Parallelize>` and coverage.
3. The #782 delivery record describes the ambient MSTest worker as MTA in two places:
   `evidence/other/code-review.2026-09-05T23-00.md:48` ("the sentinel now comes from a shut-down STA
   host instead of the pooled MTA worker") and `research/research.2026-09-05T16-10.md:899`, which
   classifies `QfcHomeControllerRunAsyncTests.cs:329` as MTA on exactly this reasoning.

### The two concrete mechanisms available

**To run a test body on an STA thread:** `[STATestClass]` or `[STATestMethod]` from
`MSTest.TestFramework` 4.4.0 (`UtilitiesCS.Test/packages.config:146-147`;
`UtilitiesCS.Test/UtilitiesCS.Test.csproj:761-765`). Widely used already —
`UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs:30`, `.../ProgressPane_Tests.cs:28`,
`.../Dialogs/MyBox_Tests.cs:25`, `.../EmailIntelligence/FolderRemapViewer_Tests.cs:26`,
`.../EmailIntelligence/FilterOlFoldersViewer_Tests.cs:26`, `.../Extensions/WinFormsExtensions_Tests.cs`
(twelve `[STATestMethod]`s), `Tags.Test/*.StaTests.cs`.

**To run a delegate on an STA thread from an MTA test:** a dedicated thread with
`SetApartmentState(ApartmentState.STA)` before `Start()`. There are at least a dozen in-repo hosts;
the ones closest to this work are:
- `UtilitiesCS.Test/Threading/UiThread_Tests.cs:186-213` — `StaDispatcherHost`, runs `Dispatcher.Run()`,
  shuts down deterministically.
- `UtilitiesCS.Test/Threading/WpfUiDispatcherTests.cs:185` and
  `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs:241` — sibling copies.
- `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs:51-69` — a full WinForms `Application.Run`
  message pump on an STA thread (issue #230), the only in-repo host that actually pumps WinForms.
  It lives in `QuickFiler.Test`, not `UtilitiesCS.Test`.

**To run a delegate on an MTA thread:** do nothing — a plain `[TestMethod]` already is MTA. If a
specific test class must be STA-hosted but still needs an MTA Act, spawn a `Thread` and leave its
apartment at the default (`ApartmentState.MTA`) and `Join()` it, which is the shape the #787 potential
entry itself proposes (`docs/features/potential/promoted/2026-09-05-uithread-init-accepts-non-sta-callers.md:76`).

**Mechanism the plan should use for AC4:** put the STA-rejection cases in an `[STATestClass]` and the
MTA-rejection cases in a plain `[TestClass]`, both in `UtilitiesCS.Test`, both `[DoNotParallelize]`,
both wrapping their Act in the `UiThreadStateScope` from R3. Do not introduce an
`ExecutionThreadApartmentState` runsettings change — `test.runsettings:2-5` records that decision and
reversing it would put the whole assembly on STA and remove class-level parallelism.

---

## R5 — The MTA test caller that must be reconciled

**Exact call:** `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs:329` —
`UiThread.Init(false);`

**Enclosing test:** `public void Worker_RunWorkerCompleted_HandlesCompletionCorrectly()`, declared at
`:326` under a plain `[TestMethod]` at `:325`. It is the last method in the file (file ends at `:361`).
The call is the first line of the Arrange block.

**Why the test calls it.** The Act (`:346-353`) invokes the private
`QfcHomeController.Worker_RunWorkerCompleted` by reflection with a non-cancelled, non-faulted
`RunWorkerCompletedEventArgs` (`:343`). That drives the `else` branch of
`QuickFiler/Controllers/QfcHomeController.cs:357-367`, whose body is
`UiThread.Dispatcher.Invoke(() => { _formViewer.ItemsPerLoadEnabled = true; _formViewer.SkipButtonEnabled = true; });`
at `:360-364`. `UiThread.Dispatcher` throws `InvalidOperationException` when `_dispatcher` is null
(`UiThread.cs:159-167`), so the test needs the static populated. `UiThread.Init(false)` is the
cheapest way it found to do that.

**What it depends on afterwards.** The two assertions at `:356-357`
(`mockFormViewer.Object.ItemsPerLoadEnabled` and `.SkipButtonEnabled` both true) require that the
lambda passed to `Dispatcher.Invoke` **actually executed before `Invoke` returned**. That holds today
only because `Init()` captured `Dispatcher.CurrentDispatcher` on the *same* MSTest worker thread, so
`CheckAccess()` is true and `Invoke` runs the delegate inline. There is no message pump anywhere in
this test. This is a latent order-dependency: if any earlier test in the same process had already
consumed `_loaded` (or installed a `_dispatcher` belonging to a different, non-pumping thread), this
`Invoke` would either use a stale dispatcher or block. `UtilitiesCS.Test/Threading/UiThread_Tests.cs:151-156`
documents the reciprocal hazard from the other side.

### Reconciliation options

| Option | Consequence |
|---|---|
| **A. Add `[STATestMethod]` to `:325`** (or `[STATestClass]` to the class) | Minimal edit; `Init()` then passes the apartment check. **But it does not remove the order-dependency and may make it worse.** If `_loaded` was already consumed on some other thread, `Init()` becomes a no-op and `UiThread.Dispatcher` returns a dispatcher belonging to a foreign thread; `Invoke` would then post to a dispatcher with no running frame and **block until the test times out**. It also leaves a never-shut-down `Dispatcher` on a pooled MSTest STA worker, which is precisely the hazard finding C10 of #782 removed from `UiThread_Tests.cs` (`code-review.2026-09-05T23-00.md:48`; rationale quoted at `UiThread_Tests.cs:178-185`). |
| **B. Remove the `Init()` call and install a pumped dispatcher for the test's duration** — `RECOMMENDED` | `QuickFiler.Test` already owns the machinery: `QfcItemController.UiThreadDispatcherFixture.Exchange(...)` / `BeginTransactionAsync()` + `UiThreadDispatcherTransaction.Install(...)` (`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:55-63`, `:122-126`, `:242-254`) atomically swaps `UiThread._dispatcher` and restores it on dispose. Install a dispatcher from a **pumping** STA host — the `StaDispatcherHost` shape at `UtilitiesCS.Test/Threading/UiThread_Tests.cs:186-213` (which calls `Dispatcher.Run()`), replicated in `QuickFiler.Test`, or `WinFormsPumpHost` (`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs`). `Dispatcher.Invoke` from the MSTest thread then marshals to the host thread, runs the lambda, and returns synchronously, so the two assertions at `:356-357` still hold. This removes the `UiThread.Init` dependency entirely and makes the test order-independent. |
| **C. Do not use the fixture's `EnsureDispatcher()`** | Explicitly rejected: its parked dispatcher never runs a frame (`UiThreadDispatcherFixture.cs:143-177`), so `Dispatcher.Invoke` from the MSTest thread would block forever. |
| **D. Inject an `IUiDispatcher` into `QfcHomeController`** | The seam exists (`UtilitiesCS/Threading/IUiDispatcher.cs`, `WpfUiDispatcher.cs`) and `QfcItemController` already uses it (`QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs:21`). But `QfcHomeController.cs:360` is not routed through it, so this is a production change to a file outside the issue's stated scope. Larger than option B for the same benefit. |

**Assertions affected:** only `:356` and `:357`. Nothing else in the test method reads `UiThread`.
Option B preserves both; option A preserves both only when the latch happens to be unconsumed.

---

## R6 — The awaiter fix and its ordering-sensitive callers

### The #781 precedent — and it says the opposite of what #809's notes assume

The breadcrumb UI-boundary guard is `QuickFiler/Viewers/BreadcrumbUiDispatcher.IsCurrentBoundary()`
at `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:255-278`. Quoted verbatim, `:263-272`:

```csharp
// When a context was captured it is the authoritative boundary, so only an ambient
// reference match to that exact context proves the caller is on it. Bare owner-thread
// identity must never substitute here: a continuation resumed after
// ConfigureAwait(false) can be scheduled onto a recycled thread-pool thread whose
// managed thread ID equals the captured owner thread ID, which would run UI work inline
// and complete the returned task without any post ever crossing the captured context.
if (_context != null)
{
    return ReferenceEquals(SynchronizationContext.Current, _context);
}
```

Thread identity is used **only** on the `_context == null` path, which
`CreateForCurrentThreadTests()` (`:62-65`) constructs for host-neutral tests
(`:276-277`). The related `DispatchValue<T>` guard is stricter still — `:164-166`:
`ReferenceEquals(_executingDispatcher, this)`, with the comment *"Ambient context and thread identity
do not survive awaits."*

**Therefore the #809 `## Suspected Cause / Notes` claim at `issue.md:59` — that
`UiThread.UiThreadId == Thread.CurrentThread.ManagedThreadId` is "the same ownership test #781 adopted
for the breadcrumb UI-boundary guard" — is FALSE.** #781 adopted reference equality and explicitly
rejected bare thread identity, in a comment written for exactly this reason. The planner must not
cite #781 as precedent for a thread-id-only predicate.

The runtime probe is at
`docs/features/active/2026-09-05-breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781/evidence/other/dispatcher-synccontext-probe.2026-09-05T10-40.md`.
Its recorded output (`:36-45`) on .NET Framework 4.8 STA:

```
ReuseDispatcherSynchronizationContextInstance: True
outer ambient type            : System.Windows.Forms.WindowsFormsSynchronizationContext
inside Dispatcher.Invoke type : System.Windows.Threading.DispatcherSynchronizationContext
Invoke ctx == outer ambient   : False
Invoke#1 ctx == Invoke#2 ctx  : True
InvokeAsync ctx == Invoke#1   : True
ambient after ops == outer    : True
```

Two consequences the issue does not draw out. First, `Invoke#1 ctx == Invoke#2 ctx : True` means the
defect **does not reproduce inside a dispatcher operation**: a viewer's captured
`DispatcherSynchronizationContext` *is* reference-equal to the ambient one during any later dispatcher
operation on the same dispatcher, so `IsCompleted` is already true there today. The defect is confined
to awaits taken on the UI thread *outside* a dispatcher operation. Second,
`ambient after ops == outer : True` means the WinForms context is restored afterwards, so the
persistent UI context and the dispatcher context are two distinct, both-valid UI-owned instances.

### Awaiter callers (every live site)

Search: `Grep` over `**/*.cs` for `await (UiThread\.)?UiSyncContext|UiSyncContext\.GetAwaiter|await .*SyncContext;`.
Note that no caller anywhere awaits `UiThread.UiSyncContext` itself; every production caller awaits a
**viewer-level** `UiSyncContext` property, which is captured in the viewer constructor
(`QuickFiler/Viewers/ItemViewer.cs:26`, `.../EfcViewer.cs:26`, `.../QfcFormViewer.cs:23`,
`.../ItemViewerExpanded.cs:21`, `.../QfcItemViewer.cs:24`, `.../QfcItemViewerExpanded.cs:24` — all
`_context = SynchronizationContext.Current;`).

| Site | Ordering change if `IsCompleted` becomes true on the UI thread | Existing test asserting the ordering |
|---|---|---|
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:64` | **Yes, materially.** `:67` immediately calls `TaskScheduler.FromCurrentSynchronizationContext()`. Today the continuation runs inside a posted callback whose ambient context is the *dispatcher* context; inline, the ambient would be the persistent WinForms context. The resulting `TaskScheduler` targets a different (but still UI-affine) context. **A predicate that returns true when `SynchronizationContext.Current` is null would make this line throw `InvalidOperationException`.** | None found. |
| `QuickFiler/Controllers/EfcItemController.cs:191` | Same shape — `TaskScheduler.FromCurrentSynchronizationContext()` at `:201`. | None found. |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:287` | Removes one queued hop. | None found. |
| `QuickFiler/Controllers/EfcItemController.cs:1104` | Removes one queued hop. | None found. |
| `QuickFiler/Controllers/EfcFormController.cs:877` (`ActionCancelAsync`) | Inline instead of queued: `_formViewer.Close()` at `:879` then `Cleanup()` at `:880` would run before already-queued UI work rather than after it. | None found for `EfcFormController.ActionCancelAsync`. (`QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs:213` and `:247` assert ordering, but for **`QfcFormController`**, which does not use this awaiter.) |
| `QuickFiler/Controllers/EfcFormController.cs:911` (`ActionDeleteAsync`) | Inline instead of queued before `ApplyDeleteGesture()`. | `QuickFiler.Test/Controllers/EfcFormControllerTests.cs:392` `ActionDeleteAsync_AwaitedTwice_LeavesExactlyOneTrashRowInFolderRows`. It injects a bare `new SynchronizationContext()` into `_context` (`:401`) and asserts only the resulting `_folderRows`, not ordering. **Unaffected under the recommended predicate** — ambient on the MSTest thread is null, so the predicate returns false and the continuation posts to the thread pool exactly as today. |
| `QuickFiler/Controllers/EfcFormController.cs:927`, `:953` (`CreateFolderAsync`) | Inline instead of queued around `Hide()` / `Dispose()` / `Cleanup()`. | None found. |
| `QuickFiler/Controllers/EfcFormController.cs:1264` | Removes one queued hop. | None found. |
| `QuickFiler/Controllers/QfcCollectionController.cs:782` (`RemoveControlsAsync`) | Inline: the `TlpLayout` toggle at `:784-785` and `TableLayoutHelper.RemoveSpecificRow` at `:788` would run before already-queued layout work. | None found asserting ordering. |
| `QuickFiler/Controllers/QfcCollectionController.cs:2018` | Removes one queued hop. | None found. |
| `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:263` | Inline: `CreateEnvironmentAsync` / `EnsureCoreWebView2Async` at `:265-269` start sooner. | None found. |
| `QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs:190` | **BREAKS under a thread-id-only predicate.** See below. | `AwaitingSyncContext_FromTheTestThread_ResumesOnThePumpThread` (`:183`) asserts `Thread.CurrentThread.ManagedThreadId == host.ThreadId` after the await (`:191-199`). |
| `QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs:245` | Same exposure. | `BothMarshalRoutes_WpfDispatcherAndSyncContext_ExecuteOnThePumpThread` (`:218`), assertion at `:255`. |

Commented-out awaiter sites, excluded: `QuickFiler/Controllers/EfcFormController.cs:1040`,
`QuickFiler/Controllers/QfcItemController.cs:316`, `UtilitiesCS/Threading/IdleActionQueue.cs:79`.

`ItemViewerQueue.Dequeue` (`QuickFiler/Helper Classes/ItemViewerQueue.cs:46-55`) does **not** await a
context. Its relevance is upstream: it dequeues through `ViewerQueueCore` whose production schedulers
are `UiThread.Dispatcher.InvokeAsync` / `.Invoke` (`:21`, `:27`, `:88`, `:90`), so an `ItemViewer`
built through it runs its constructor — and therefore `_context = SynchronizationContext.Current`
(`QuickFiler/Viewers/ItemViewer.cs:26`) — inside a dispatcher operation. That is the whole mechanism
of #784. `ItemViewer` carries `[ExcludeFromCodeCoverage]` at `QuickFiler/Viewers/ItemViewer.cs:20`.

### Evaluating the proposed predicate `UiThread.UiThreadId == Thread.CurrentThread.ManagedThreadId`

**(a) Awaiter over a NON-UI context, caller on the UI thread — the proposed predicate is WRONG, and
an existing test proves it.** `QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs:183-199` awaits
`host.SyncContext`, a `WindowsFormsSynchronizationContext` belonging to the pump thread
(`WinFormsPumpHost.cs:303-307`), from the MSTest thread, and asserts the continuation resumes on
`host.ThreadId`. If any earlier test in the `QuickFiler.Test` process has run `UiThread.Init()` on
the pooled MSTest worker — which
`QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs:329` does today, and which
`UtilitiesCS.Test/Threading/UiThread_Tests.cs:151-156` documents as an observed cross-class effect —
then `UiThread.UiThreadId` equals the MSTest worker's id, the bare predicate returns `true`, the
continuation runs **inline on the MSTest thread**, and the assertion at `:194-199` fails. This is not
hypothetical: it is the same reasoning `BreadcrumbUiDispatcher.cs:263-268` records.

**(b) `UiThreadId` still `-1`.** `_uiThreadId = -1` (`UiThread.cs:133`). `Thread.ManagedThreadId` is
never negative, so a bare comparison is *safe* in this case — it simply returns false. The
consequence is a silent behaviour cliff rather than a bug: before `Init()` the awaiter always posts;
after `Init()` it may continue inline. Any predicate should still guard the sentinel explicitly so
the intent is legible.

**(c) Second STA thread.** Its `ManagedThreadId` differs from `_uiThreadId`, so the predicate returns
false and the continuation posts. Correct, but for the wrong reason — the predicate would also return
false for a *legitimately* matching context on that thread, which is the today behaviour and is safe.

**Recommended predicate.** Keep reference equality as the fast path, then admit exactly two
additional UI-owned cases, and only while actually standing on the owning UI thread with a non-null
ambient context. Because `SynchronizationContextAwaiter` is nested inside `UiThread`, it can read the
private statics `_uiThreadId`, `_uiSyncContext` and `_dispatcher` **directly**; it must not read the
`UiSyncContext` or `Dispatcher` *properties*, because the first lazily calls `Init()` (which under
AC1 would throw from a worker thread) and the second throws when unset.

```csharp
public bool IsCompleted
{
    get
    {
        SynchronizationContext? ambient = SynchronizationContext.Current;
        if (ReferenceEquals(_context, ambient))
        {
            return true;
        }
        // A null ambient context means there is nothing to resume onto: continuing inline would
        // break TaskScheduler.FromCurrentSynchronizationContext() at the two WebView2 setup sites.
        if (ambient is null)
        {
            return false;
        }
        if (_uiThreadId == -1 || _uiThreadId != Thread.CurrentThread.ManagedThreadId)
        {
            return false;
        }
        // The persistent UI context captured at Init() time.
        if (ReferenceEquals(_context, _uiSyncContext))
        {
            return true;
        }
        // A dispatcher context is UI-owned only when this thread's dispatcher is the UI dispatcher.
        return _context is DispatcherSynchronizationContext
            && ReferenceEquals(Dispatcher.FromThread(Thread.CurrentThread), _dispatcher);
    }
}
```

Why not resolve the `DispatcherSynchronizationContext`'s own dispatcher: .NET Framework 4.8's
`System.Windows.Threading.DispatcherSynchronizationContext` exposes no public `Dispatcher` property,
so the owning dispatcher can only be reached by reflection, which is not acceptable in production.
`Dispatcher.FromThread` is the reflection-free substitute; it returns `null` rather than creating a
dispatcher, so it is side-effect-free.

Case-by-case: (a) `host.SyncContext` is a `WindowsFormsSynchronizationContext`, not
`_uiSyncContext` and not a `DispatcherSynchronizationContext`, so both `WinFormsPumpHostTests`
assertions still see `false` and still post — **the predicate is safe for the existing tests**;
(b) the `-1` sentinel is checked explicitly; (c) a second STA thread fails the id check.

**`default(SynchronizationContextAwaiter)` under this proposal.** No field is added, so the struct's
default state is unchanged: `_context` is `null`. Today `IsCompleted` evaluates
`null == SynchronizationContext.Current`, which is `true` on a thread with no ambient context and
`false` otherwise; the recommended predicate's first clause,
`ReferenceEquals(null, ambient)`, produces exactly the same two answers, and the `ambient is null`
early-return covers the remaining branch. `OnCompleted` still `NullReferenceException`s on a default
instance, as it does today (`UiThread.cs:102-103`). **There is no default-state regression, and this
is a positive reason to prefer a field-free predicate over "store the owning thread id in the awaiter
at construction time."** The construction-time capture is also unsound on its own terms: `GetAwaiter()`
(`UiThread.cs:108-111`) runs at the `await` site on the *awaiting* thread, not on the thread where the
context was captured, so a captured id would record the wrong thread.

---

## R7 — Existing tests that must keep passing

| File | Test methods at risk | Which change touches them |
|---|---|---|
| `UtilitiesCS.Test/Threading/UiThread_Tests.cs` (`SynchronizationContextAwaiter_Tests`, `:9-104`) | `Constructor_NullContext_ThrowsArgumentNullException` (`:13`); `IsCompleted_WhenContextIsNotCurrent_ReturnsFalse` (`:23`); `IsCompleted_WhenContextMatchesCurrent_ReturnsTrue` (`:37`); `GetResult_DoesNotThrow` (`:61`); `OnCompleted_PostsCallbackToContext` (`:75`) | **#784.** `:23` uses a bare `new SynchronizationContext()` with no ambient context installed — under the recommended predicate the `ambient is null` early-return keeps it `false`. `:37` installs the same instance as ambient (`:42`) so the reference fast path keeps it `true`. Both still pass. **Neither class carries `[DoNotParallelize]`; if AC4 adds latch-mutating tests to this file, the attribute must be added.** |
| `UtilitiesCS.Test/Threading/UiThread_Tests.cs` (`UiThread_Dispatcher_Tests`, `:128-214`) | `Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize` (`:133`); `Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance` (`:149`) | **#787/#788.** `:149-156` explicitly documents its dependence on `QfcHomeControllerRunAsyncTests` calling `UiThread.Init(false)`; changing that call (R5) changes the premise of this comment, though not the assertion, because the test installs a known null prior at `:157`. The method name's `NamingInitialize` suffix is deliberately inaccurate and **must not be renamed** — `code-review.2026-09-05T23-00.md:145-153` records that its fully-qualified name is quoted inside a committed `TestCaseFilter` evidence artifact. |
| `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` | `YieldAsync_WithoutDispatcher_RemainsStrict` — asserts `.WithMessage(UiThread.DispatcherNotInitializedMessage)` at `:136`; a second assertion at `:196` is `observedException.Message.Should().Contain("UiThread.Init()")` | **#787/#788.** If the shipped fix changes `DispatcherNotInitializedMessage` (`UiThread.cs:135-136`), the `:136` assertion moves with the constant but the `:196` substring assertion does **not** — the literal `UiThread.Init()` must survive in the message text. |
| `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` | The `ForceDispatcherNull` region (`:137-171`, `:225-245`) forces `UiThread.Dispatcher` to null through `UiThreadDispatcherScope` | **#787/#788.** Any change to the `Dispatcher` accessor or its message. |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` | `EnterUiContextAsync_WhenUiSyncContextPostsSynchronously_CompletesUsingDefaultAction` (`:462`) — installs `UiThread._uiSyncContext` by reflection at `:479`, restores at `:485` | **#787.** It never leaves the field null, so the lazy `Init()` is not reached. It would break only if the fix changed the field name. |
| `UtilitiesCS.Test/EmailIntelligence/FolderRemapViewer_Tests.cs` (`[STATestClass]`, `:26`) | `SetController_WithSyntheticController_ConfiguresTreeDelegates` (`:118`) — reaches `UiThread.AutoScaleFactor` and asserts `NotThrow` | **#787/#788.** This is the in-repo test that actually drives `Init()` → `Initialize()` to success. Under AC1 it must remain STA. |
| `UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersViewer_Tests.cs` (`[STATestClass]`, `:26`) | `SetController_WithSyntheticController_ConfiguresBothTreeDelegates` (`:39`) | Same. |
| `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs` | `Worker_RunWorkerCompleted_HandlesCompletionCorrectly` (`:326`) | **#787.** The one caller AC1 breaks. See R5. |
| `QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs` | `AwaitingSyncContext_FromTheTestThread_ResumesOnThePumpThread` (`:183`); `BothMarshalRoutes_WpfDispatcherAndSyncContext_ExecuteOnThePumpThread` (`:218`) | **#784.** These are the two tests that a thread-id-only predicate would break. See R6(a). |
| `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` | `ActionDeleteAsync_AwaitedTwice_LeavesExactlyOneTrashRowInFolderRows` (`:392`) | **#784.** Injects `new SynchronizationContext()` into `EfcViewer._context` (`:401`); still posts under the recommended predicate. |
| `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` | Snapshots/restores `UiThread.Dispatcher` through the QuickFiler fixture (`:27-50`) | **#788** (only if the latch design touches `_dispatcher`). |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs`, `.TestSupport.cs`, `.SeamDispatcherTests.cs`, `.UiThreadDispatcherFixture.cs`, `WpfUiDispatcherTests.cs` | All consumers of the `UiThread._dispatcher` reflection fixture | **#788** (same condition). |

---

## R8 — Coverage and file size

- **`UtilitiesCS/Threading/UiThread.cs` is 195 lines.** The 500-line repository limit
  (`.claude/rules/general-code-change.md`, "File Size Limit") leaves roughly 305 lines of headroom,
  so all three fixes can land in this one file without a partial-class split.
- **Owning test assembly: `UtilitiesCS.Test`.** `UtilitiesCS/UtilitiesCS.csproj:1112` compiles
  `Threading\UiThread.cs`; `UtilitiesCS.Test/UtilitiesCS.Test.csproj:503` compiles
  `Threading\UiThread_Tests.cs` and `:76` compiles `TestHelpers\UiThreadDispatcherScope.cs`. Both are
  **legacy (non-SDK) csproj files with explicit `<Compile Include>` items**, so any new test file or
  new production file must be added to the csproj by hand.
- **Neither `UiThread` nor any nested type carries `[ExcludeFromCodeCoverage]`.** A `Grep` for
  `ExcludeFromCodeCoverage` over `UtilitiesCS/Threading` returns hits only in `ThreadMonitor.cs`
  (`:92`, `:104`, `:137`, `:201`) and a documentation mention in `LockupStallDecider.cs:49`.
  `SyncContextForm.cs` carries none either. `coverage.config` (repo root, 24 lines) excludes only
  third-party module paths (Deedle, FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing,
  MSTest) — **no first-party exclusion applies to `UiThread.cs`.**
- **Measured baseline for this file**, quoted from
  `docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/other/r1-r2-maintainer-disposition.2026-09-06T00-15.md:74-83`:
  19 uncovered lines — `28,29,30,32,33,34,67,68,69,70,71,72,73,74,75,76,118,119,120` — for
  **76.83% line / 65.00% branch**, below the 80% modified-file trigger floor. The maintainer
  **WAIVED** raising it in #782 and recorded that doing so needs a seam extraction on the
  `ThreadMonitor` block at `:67-76`, "the same class of change already carved out to issues #787 and
  #788" (`:85-93`). #809 is that carve-out, so the coverage floor is now in scope for this delivery:
  the R3 factory seam covers `:67-76` (the `_monitorUiThread` branch) and AC4's tests cover
  `:28-34` (the two null-guard branches of `Init()`) and `:118-120` (the lazy `UiSyncContext` path).
  All 19 currently-uncovered lines are reachable through the recommended design.

---

## Recommended design

### #787 — STA precondition on `Init()`

**Change.** Insert a precondition as the **first statement** of `Init()` (`UiThread.cs:19-40`), before
the four field assignments at `:26-35` and before the latch read at `:36`:

```csharp
ApartmentState apartment = Thread.CurrentThread.GetApartmentState();
if (apartment != ApartmentState.STA)
{
    throw new InvalidOperationException(NonStaInitMessage(apartment));
}
```

with a sibling of the existing message constant:

```csharp
internal const string NonStaInitMessagePrefix =
    "UiThread.Init() must be called on the UI (STA) thread during host startup. Observed apartment state: ";

private static string NonStaInitMessage(ApartmentState observed) =>
    NonStaInitMessagePrefix + observed;
```

Placing it before `:26` matters: the four assignments at `:26-35` mutate process-global monitoring
configuration even when the latch is already consumed, so a non-STA caller currently poisons
`_monitorUiThread`, `_onLockupDetected`, `_monitorTimeProvider`, and `_lockupAttributionThresholdMs`
regardless of the latch. AC1 says "before any global is captured"; that includes these four.

`internal const` matches `DispatcherNotInitializedMessage` (`UiThread.cs:135-136`) and is reachable
from `UtilitiesCS.Test` via `UtilitiesCS/Properties/AssemblyInfo.cs:19`. Split the constant from the
formatted message so a test can assert the stable prefix without pinning the enum rendering.

**Seam.** None required — MSTest's default apartment is MTA (R4), so a plain `[TestMethod]` supplies
the rejection case directly and `[STATestMethod]` supplies the acceptance case.

**Tests (`UtilitiesCS.Test/Threading/`, new file, `[DoNotParallelize]`).**
1. Plain `[TestMethod]`: `Init()` throws `InvalidOperationException` whose message starts with
   `NonStaInitMessagePrefix` and contains `MTA`.
2. Plain `[TestMethod]`: after the throw, `UiThread._loaded` is still unconsumed and the four
   monitoring fields are unchanged (read through the R3 reset scope).
3. `[STATestMethod]`: `Init()` from an STA thread does not throw and populates all four capture
   fields.
4. Reconcile `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs:329` per R5 option B.

### #788 — retry after a failed `Initialize()`, without re-arming the shared latch

**Change.** Replace the single-shot guard's role in `Init()` with a *success-recorded* latch plus a
serializing lock, so the latch is set only after `Initialize()` returns:

```csharp
private static readonly object InitLock = new object();
private static bool _initialized;

// inside Init(), after the STA precondition and the four assignments:
lock (InitLock)
{
    if (_initialized)
    {
        return;
    }
    Initialize();
    _initialized = true;
}
```

`ThreadSafeSingleShotGuard` is retired from `UiThread` (the type stays; it has other consumers to be
confirmed by the planner). A failed `Initialize()` propagates with `_initialized` still `false`, so a
later `Init()` retries — which is exactly AC2.

**Why this does not reintroduce the #782 regression.** The #782 mechanism, as recorded, is: a
re-armed latch makes *every subsequent read of `UiSyncContext` or `AutoScaleFactor`* re-enter
`Initialize()`, reconstruct and `Show()` a WinForms `SyncContextForm`, and throw again, starving the
thread pool. That mechanism has two necessary preconditions, and **the #787 precondition removes the
second one on every thread where the starvation could occur.** Verified in R1: the only ways to reach
`Initialize()` are the two direct `Init()` calls and the two lazy getters at `UiThread.cs:119` and
`:185`. With the STA check as the first statement of `Init()`, a non-STA reader of either lazy getter
now fails at a single `GetApartmentState()` read and a `throw` — it never reaches `new
SyncContextForm()` at `:51` or `Show()` at `:54`. The expensive-and-throwing body is therefore
unreachable from any thread-pool thread, which is where thread-pool starvation would have to
originate. On the STA thread itself, `Initialize()` succeeds (verified in R1 against
`FolderRemapViewer_Tests.cs:118-131` and `FilterOlFoldersViewer_Tests.cs:39-51`), so the retry loop
never engages there either. The `lock (InitLock)` additionally serializes concurrent first attempts,
which the `Interlocked.Exchange` latch never did — `code-review.2026-09-05T23-00.md:42` records that
pre-existing race as no-action finding C04.

Two residual obligations the planner must carry, because R1 could not close them:
- The recorded mechanism requires `Initialize()` to throw somewhere in the test run, and I found no
  in-tree path where it does (R1, points 1-3). **Add a Phase 0 measurement** that establishes, on
  this host, whether `new SyncContextForm(); Show();` throws on an MTA thread. If it does not, the
  #782 regression narrative is refuted and the AC2 "reproduce the #782 scenario" clause must be
  restated as "reproduce a forced-throw scenario through the R3 factory seam".
- Regardless of the outcome, run the full nine-assembly suite and record
  `TryAddValuesAsync_UpdatesExistingValue` explicitly, per
  `docs/features/potential/promoted/2026-09-05-uithread-init-latch-not-rearmed-after-failed-initialize.md:78`.
  Because that test is the documented #780 flake, a **single** failure is not sufficient evidence of
  regression; require at least three repetitions before attributing it.

**Seam.** `internal static Func<IUiCaptureSource> SyncContextFormFactory { get; set; }` on `UiThread`,
defaulting to `() => new SyncContextFormAdapter()`, where `IUiCaptureSource` exposes
`ShowInTaskbar`/`WindowState`/`Show()`/`Hide()`/`CaptureUiVariables()` and the four capture
properties from `SyncContextForm` (R3). Pattern precedent: `ItemViewerQueue.ProductionViewerFactory`
(`QuickFiler/Helper Classes/ItemViewerQueue.cs:11-27`, reset at `:83-91`) and
`FolderPredictor`'s four static delegates (`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:161-184`).
Plus `internal static void ResetForTesting()` on `UiThread` clearing `_initialized`,
`_uiSyncContext`, `_autoScaleFactor`, `_uiThreadId` (to `-1`), `_dispatcher`, `_syncContextForm`,
and restoring the default factory.

**Tests (`UtilitiesCS.Test`, `[STATestClass]` where `Initialize()` must succeed, `[DoNotParallelize]`
everywhere, all wrapped in a new `UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs`).**
1. Factory throws → `Init()` propagates; a second `Init()` with a working factory succeeds and
   populates all four fields. (AC2 core.)
2. Factory throws → the four capture fields are all still unset afterwards.
3. Factory throws → a subsequent read of `AutoScaleFactor` from an **MTA** thread throws the AC1
   `InvalidOperationException` rather than re-entering the factory; assert the factory invocation
   count did not increase. **This is the direct AC2 anti-regression test**: it asserts the absence of
   the #782 retry storm as an invocation count rather than as a wall-clock duration.
4. Two concurrent STA `Init()` calls invoke the factory exactly once.

### #784 — `IsCompleted` on the owning UI thread

**Change.** Replace `UiThread.cs:100` with the block given in R6, reading the private statics
`_uiThreadId`, `_uiSyncContext`, `_dispatcher` directly rather than through their properties, and
adding `using System.Windows.Threading;` (already present, `UiThread.cs:11`).

**Seam.** None in production. Tests need only `SynchronizationContext.SetSynchronizationContext` (the
pattern already at `UiThread_Tests.cs:42`), an STA host with a real dispatcher (`StaDispatcherHost`,
`UiThread_Tests.cs:186-213`), and the R3 reset scope to control `_uiThreadId` / `_uiSyncContext` /
`_dispatcher`.

**Tests (`UtilitiesCS.Test`, extending `SynchronizationContextAwaiter_Tests`, which must gain
`[DoNotParallelize]` once it mutates `UiThread` statics).**
1. Reference match with a non-null ambient → `true` (preserves the existing `:37` behaviour).
2. Ambient is `null`, `_context` non-null → `false` (protects
   `TaskScheduler.FromCurrentSynchronizationContext()` at `QfcItemController.ViewerSetup.cs:67` and
   `EfcItemController.cs:201`).
3. `_uiThreadId == -1` → `false`.
4. On an STA host thread: install `_uiThreadId`/`_dispatcher` for that thread, capture a context
   inside `dispatcher.Invoke(...)`, restore the WinForms ambient, then assert `IsCompleted == true`.
   **This is the AC3 defect test.**
5. Same arrangement, but the captured dispatcher context belongs to a *different* thread's
   dispatcher → `false`.
6. A foreign `WindowsFormsSynchronizationContext` while `_uiThreadId` equals the current thread id →
   `false`. **This is the regression guard for the `WinFormsPumpHostTests` failure mode in R6(a).**
7. `default(SynchronizationContextAwaiter).IsCompleted` on a context-free thread → `true`, matching
   today's behaviour.

**Additionally:** amend the issue's `## Suspected Cause / Notes` bullet at `issue.md:59`. It cites
#781 as precedent for a thread-id predicate; `BreadcrumbUiDispatcher.cs:263-272` records the opposite
rule. Leaving that sentence standing invites a future planner to implement the predicate that R6(a)
shows breaks two existing tests.

---

## Open questions for the planner

1. **UNKNOWN — does `new SyncContextForm(); Show();` throw on an MTA thread in this test host?**
   This is the single fact the entire #782 constraint rests on, and it cannot be established by
   reading. Searched: `UtilitiesCS/Threading/SyncContextForm.cs` and `.Designer.cs` (no OLE-requiring
   control), `UtilitiesCS/Threading/UiThread.cs`, and every `.Test` file matching `UiThread.` or
   `AutoScaleFactor`. Two in-tree records point opposite ways:
   `UtilitiesCS.Test/Threading/UiThread_Tests.cs:151-156` states that `QfcHomeControllerRunAsyncTests`
   calling `UiThread.Init(false)` "populates the same process-global static" (implying success on an
   MTA worker), while the #782 mechanism requires a throw. Resolve by measurement in Phase 0.
2. **UNKNOWN — the member set behind the #782 figure "49 live reads across 25 production files".**
   My independent census gives 56 across 31 (R2). The #782 artifacts publish the figure but not the
   list, so the +7/+6 divergence cannot be decomposed. Searched:
   `docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/**` for `49`, `re-arm`,
   `SyncContextForm`, and `TryAddValuesAsync`.
3. **UNKNOWN — whether `ThreadSafeSingleShotGuard` has consumers outside `UiThread`.** I read
   `UtilitiesCS/Threading/ThreadSafeSingleShotGuard.cs` in full but did not enumerate its callers, so
   I cannot say whether retiring it from `UiThread` leaves the type orphaned. Search
   `**/*.cs` for `ThreadSafeSingleShotGuard` before deciding whether to delete or retain it.
4. **UNKNOWN — whether `ApartmentState` on `Thread.CurrentThread.GetApartmentState()` can return
   `Unknown` in this host.** `.NET Framework` documents `ApartmentState.Unknown` for threads whose
   apartment has not been set. The recommended precondition rejects anything that is not `STA`, which
   is the conservative reading of AC1, but the planner should confirm the intent is "reject `MTA` and
   `Unknown`" rather than "reject `MTA` only".
5. **Open decision — is `QuickFiler.Test` to be granted `InternalsVisibleTo`?** It currently is not
   (`UtilitiesCS/Properties/AssemblyInfo.cs:18-20`). If AC4's tests must observe the new
   `internal` factory or `ResetForTesting()` from `QuickFiler.Test`, a new grant is needed; the
   recommended design avoids this by placing every AC4 test in `UtilitiesCS.Test`.
6. **Open decision — R5 option A versus option B.** Option A (`[STATestMethod]`) is a one-line edit
   but preserves an order-dependency that can convert into a test hang; option B removes the
   dependency but touches `QuickFiler.Test` fixture wiring. The recommendation is B; the planner
   should confirm the added scope is acceptable.
