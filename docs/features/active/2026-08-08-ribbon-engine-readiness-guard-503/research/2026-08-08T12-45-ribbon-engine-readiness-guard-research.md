# Research — Ribbon Engine Readiness Guard (Issue #503)

- **Issue:** #503
- **Branch:** `bug/ribbon-engine-readiness-guard-503`
- **Work mode:** full-bug
- **Requirements source:** `docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/issue.md`
- **Timestamp:** 2026-08-08T12-45
- **Status:** Research complete — research only, no code changed.

## 0. Requirement numbering used in this document

The GitHub issue body (verified by fetching <https://github.com/drmoisan/TaskMaster/issues/503>) contains **no explicitly numbered requirements**; only "Steps to Reproduce" is numbered. This document therefore uses the following stable numbering, derived from the issue's own sections, and every downstream reference should use the same mapping:

| Ref | Source text in `issue.md` | Line |
|---|---|---|
| **R1** | "There is no published readiness signal on `AppItemEngines`/`IAppItemEngines`" — a readiness signal must be introduced | 66 |
| **R2** | "the Explorer ribbon XML declares no `getEnabled` callback for the engine-backed buttons" — `getEnabled` wiring must be added | 66 |
| **R3** | "Clicking a not-yet-ready command produces no exception" — click-handler guards | 33 |
| **R4** | "Do not change the async engine construction logic, config loading, or dictionary population order inside `AppItemEngines.InitAsync()`" | 78 |
| **R5a** | "Unit coverage areas: readiness signal …; `RibbonController` engine-readiness predicate; `RibbonViewer` click-handler guards; `RibbonExplorer.xml` `getEnabled` wiring …" | 72 |
| **R5b** | "Integration scenario to retest: click each engine-backed ribbon command immediately after add-in reload" | 73 |
| **R5c** | "Manual verification notes: verify `Ribbon.InvalidateControl(...)`/`Invalidate()` refreshes the enabled state" | 74 |
| **R6** | "Preserve existing `SB`/`TrainAsync` behavior once engines are loaded" | 79 |

---

## 1. Current-state analysis (verified against source)

### 1.1 `TaskMaster\AppGlobals\AppItemEngines.cs`

- Line 26–27: `[ExcludeFromCodeCoverage] public class AppItemEngines : IAppItemEngines`. **The attribute is at type level, so every member of this class is excluded from coverage measurement.** Coverage XML in the repo confirms the tool honours the attribute (`reason="attribute_excluded"` entries, e.g. `docs/features/archive/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/final-coverage.xml:62227`). Any readiness logic placed on this class is therefore **uncoverable**.
- Line 119–123: `public ConcurrentDictionary<string, IConditionalEngine<MailItemHelper>> InboxEngines { get; protected set; } = [];` — initialised to an **empty** dictionary at field-initializer time. This is the sole reason the race window exists.
- Line 40–86 `InitAsync()`:
  - Line 50: `var configs = await Globals.AF.Manager.Configuration;` (first await — the long pole).
  - Line 63–84: the whole `InboxEngines` property is **assigned once**, at the end, from `ToConcurrentDictionaryAsync(...)`. The dictionary is never incrementally filled. Consequence: readiness transitions from "no keys" to "all keys" in a **single reference assignment**, which makes any per-key `ContainsKey` probe a precise and race-free readiness signal for that key.
  - Line 64: `.Where(config => config.Value.Engine)` — an engine whose config has `Engine == false` is **never** added. So a coarse "initialisation complete" flag would report ready for an engine that will never exist.
  - Line 68: `EngineInitializer.TryGetValue(config.Key, out var engineAsync)` and line 83 `.Where(tup => tup.Engine is not null)` — factories that return null are dropped as well.
- Line 111–117 `RestartEngineAsync(string engineName)`: writes a single key via `InboxEngines[engineName] = await engine(Globals);`. Readiness for that key must therefore be re-evaluated after a restart; a per-key probe handles this automatically, a one-shot `IsInitialized` flag does not.
- Line 129–204 `EngineInitializer` / `GetEngineInitializer()` — the fixed key set is `"Spam"`, `"Triage"`, `"Project"`, `"Context"`, `"Actionable"`.
- Lines 237–252 (`ShowDiskDialog`) and 276–282 (`ShowSaveInfo`) already use `TryGetValue` and are therefore **already race-safe** (they silently no-op).

### 1.2 `UtilitiesCS\Interfaces\IGlobals\IAppItemEngines.cs` — cross-project contract

Full contract (18 lines): `InboxEngines` (get-only), `ToggleEngineAsync`, `EngineActiveAsync`, `ShowSaveInfo`, `ShowDiskDialog`, `RestartEngineAsync`, `InitAsync`.

**Exhaustive implementer / test-double inventory** (verified by `rg ':\s*IAppItemEngines|,\s*IAppItemEngines'` over all `*.cs`, plus a full `rg IAppItemEngines`):

| Kind | Location | Effect if a member is added to the interface |
|---|---|---|
| Production implementer | `TaskMaster\AppGlobals\AppItemEngines.cs:27` | **Must implement the new member** (compile break otherwise) |
| Moq dynamic mock | `TaskMaster.Test\AppGlobals\ApplicationGlobalsTests.cs:92, 120, 334` | No compile break. `MockBehavior.Strict` throws only if the new member is *called*; these mocks only set up `InitAsync` |
| Moq dynamic mock | `TaskMaster.Test\AppGlobals\ApplicationGlobalsStartupTimingTests.cs:183` | Same |
| Moq dynamic mock | `TaskMaster.Test\AppGlobals\AppEventsTests.Helpers.cs:20, 39` | Same |
| `IApplicationGlobals` double (property only) | `QuickFiler.Test\Controllers\EfcHomeControllerTests.cs:212` → `public IAppItemEngines Engines => null;` | **No change required** — implements `IApplicationGlobals`, not `IAppItemEngines` |
| `IApplicationGlobals` double | `QuickFiler.Test\Controllers\EfcHomeControllerMetricsTests.cs:218` | No change required |
| `IApplicationGlobals` double | `QuickFiler.Test\Controllers\EfcHomeControllerLifecycleTests.cs:398` | No change required |
| `IApplicationGlobals` double | `UtilitiesCS.Test\EmailIntelligence\EmailDataMiner_TestSupport.cs:71` → `throw new NotImplementedException()` | No change required |
| `IApplicationGlobals` double | `TaskMaster.Test\AppGlobals\AppToDoObjectsTestDoubles.cs:171` | No change required |
| `IApplicationGlobals` double | `TaskMaster.Test\AppGlobals\AppOlObjectsTests.cs:419` | No change required |
| `IApplicationGlobals` double | `TaskMaster.Test\AppGlobals\AppOlObjectsCoverageTests.cs:328` | No change required |
| Consumer | `TaskMaster\AppGlobals\ApplicationGlobals.cs:461` `public IAppItemEngines Engines { get; private set; }` | No change required |
| Consumer | `TaskMaster\Ribbon\RibbonController.Intelligence.cs:204` `internal IAppItemEngines Engines => Globals.Engines;` | No change required |
| Interface declaration | `UtilitiesCS\Interfaces\IGlobals\IApplicationGlobals.cs:16` | No change required |

**Net ripple of adding an interface member: exactly one production class.** The ripple is *smaller* than expected, so it is not by itself a decisive criterion (see §3).

**Default interface members are unavailable.** `TaskMaster.csproj:30` targets `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` (`LangVersion` is `preview`, line 31). Microsoft's C# version history states for C# 8.0: *"Default interface members require enhancements in the CLR. Those features were added in the CLR for .NET Core 3.0."* (<https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history>). A high `LangVersion` does not help — the runtime, not the compiler, is the constraint (Roslyn reports `CS8701 Target runtime doesn't support default interface implementation`). **Confirmed: any member added to `IAppItemEngines` must be implemented explicitly in `AppItemEngines`, i.e. in an `[ExcludeFromCodeCoverage]` class.** This is the decisive constraint.

### 1.3 `TaskMaster\Ribbon\RibbonController.Intelligence.cs`

- The type is `public partial class RibbonController`; the **type-level `[ExcludeFromCodeCoverage]` is declared on the other partial**, `TaskMaster\Ribbon\RibbonController.cs:36`. It applies to the whole type, therefore **`RibbonController.Intelligence.cs` is also uncoverable**, even though `TaskMaster.Test\Ribbon\RibbonControllerTests.cs` exercises it.
- Line 190–202 `internal SpamBayes SB` — sets a `WindowsFormsSynchronizationContext` if none is current, then `Globals?.Engines?.InboxEngines?.TryGetValue("Spam", out var engine) ?? false ? engine as SpamBayes : null`. **Returns `null` during the window.**
- Line 279–292 `internal Triage Triage` — identical shape for key `"Triage"`. **Returns `null` during the window.**
- Line 204 `internal IAppItemEngines Engines => Globals.Engines;` — **not** null-safe on `Globals`.
- The shared `SynchronizationContext` pattern (`if (SynchronizationContext.Current is null) SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());`) appears **13 times** in this file (lines 90, 101, 111, 121, 131, 145, 155, 165, 194, 208, 259, 283, 370). It is a side-effecting property getter; a unit test that touched `SB`/`Triage`/`TriageAsync` would install a real `WindowsFormsSynchronizationContext` on the test thread. **New tests must not go through these members.**
- File length: **412 lines**.

### 1.4 `TaskMaster\Ribbon\RibbonViewer.cs`

- Line 31–33: `[System.Runtime.InteropServices.ComVisible(true)] [ExcludeFromCodeCoverage] public class RibbonViewer : Office.IRibbonExtensibility`. It is **not** currently `partial`.
- Line 42–54: an existing **internal test-seam constructor** `RibbonViewer(Func<Task> loadFolderFilterAsync, Action<Exception> reportFolderFilterInitializationFailure)` that sets `_controller = null`. Precedent for delegate-seam injection, but it deliberately bypasses the controller, so it cannot be extended to inject a `Mock<IAppItemEngines>` in a way that would produce covered lines (the class is excluded).
- Line 56: `private Office.IRibbonUI _ribbon;` set at line 91 in `Ribbon_Load`.
- File length: **487 lines** (cap is 500).

**Exhaustive enumeration of callbacks that dereference an engine from `InboxEngines`:**

| # | Callback (file:line) | Dereference path | Engine key | Failure during window | Ribbon control id |
|---|---|---|---|---|---|
| 1 | `TrainSpam_Click` (255–256) | `Controller.SB.TrainAsync(...)` | `Spam` | `NullReferenceException` | `TrainSpam` |
| 2 | `TrainHam_Click` (258–259) | `Controller.SB.TrainAsync(...)` | `Spam` | `NullReferenceException` | `TrainHam` |
| 3 | `TestSpam_Click` (261–264) | `Controller.Engines.InboxEngines[SpamBayes.GroupName].Engine` (**indexer**) | `Spam` | `KeyNotFoundException` | `TestSpam` |
| 4 | `TriageSetA_Click` (303–304) | `_controller.Triage.OlLogic.TrainSelectionAsync("A")` | `Triage` | `NullReferenceException` | `TriageSetA` |
| 5 | `TriageSetB_Click` (306–307) | same, `"B"` | `Triage` | `NullReferenceException` | `TriageSetB` |
| 6 | `TriageSetC_Click` (309–310) | same, `"C"` | `Triage` | `NullReferenceException` | `TriageSetC` |
| 7 | `ClearTriage_Click` (316–317) | `_controller.Triage.OlLogic.UnTrainSelectionAsync()` | `Triage` | `NullReferenceException` | `ClearTriage` |
| 8 | `FilterViewer_Click` (325–326) | `_controller.Triage.OlLogic.FilterViewAsync()` | `Triage` | `NullReferenceException` | `FilterTriageGroup` |

**Callbacks that touch engines but are already race-safe** (documented so the plan does not over-scope):

- `ClearSpam_Click` (252–253) → `ClearSpamManagerAsync` (Intelligence.cs:206) uses `(await Globals.AF.Manager.Configuration).TryGetValue(...)` then `RestartEngineAsync`; no `InboxEngines` dereference. Safe.
- `SpamSaveNetwork_Click` / `SpamSaveLocal_Click` / `GetSaveLocation_Click` / `TriageSaveNetwork_Click` / `TriageSaveLocal_Click` / `TriageGetSaveLocation_Click` → `ShowDiskDialog` / `ShowSaveInfo` use `TryGetValue`. Safe (silent no-op).
- `SpamBayesEnabled_Click` / `TriageEnabled_Click` / `*_GetPressed` → `ToggleEngineAsync` / `EngineActiveAsync` read `Configuration`, not `InboxEngines`. Safe.
- `TriageSelection_Click`, `SetPrecision_Click`, `ResetTriage_Click` → use `TriageAsync` (`AsyncLazy<Triage>` set by `ResetTriage()` from `SetGlobals`, RibbonController.cs:56) or construct a fresh `Triage`. **Not** an `InboxEngines` dereference.
- `TestSpamVerbose_Click`, `SpamMetrics_Click`, `SpamInvestigateErrors_Click` → `throw new NotImplementedException()` (Intelligence.cs:235–248). Pre-existing, out of scope.

**Correction to the issue text.** `issue.md:16` asserts the race "affects every ribbon command backed by an engine in `InboxEngines` (Triage, Project, Context, Actionable)". Verified: **no ribbon callback dereferences the `Project`, `Context`, or `Actionable` engines.** `BuildCategoryClassifier_Click` (242–243) and `BuildActionableClassifier_Click` (245–246) construct fresh `CategoryClassifierGroup` / `ActionableClassifierGroup` instances via `RibbonController.BuildCategoryClassifierAsync` / `BuildActionableClassifierAsync` (Intelligence.cs:119–137) and never read `InboxEngines`. The reachable defect surface is exactly the 8 rows above (`Spam` × 3, `Triage` × 5). The catalog should still be built as an extensible map so future `Project`/`Context`/`Actionable` commands are one-line additions.

### 1.5 `TaskMaster\Ribbon\RibbonExplorer.xml`

- Root: `<customUI onLoad="Ribbon_Load" xmlns="http://schemas.microsoft.com/office/2009/07/customui">` (line 2) — the **2009 (customUI14) namespace**.
- Current length: **519 lines** (already above the 500-line guidance; see §7).
- The 8 controls that need `getEnabled`, all `<button>` elements:

| Control id | Element | Line | Parent |
|---|---|---|---|
| `TrainSpam` | `button` | 99–104 | `group id="SpamBayesGroup"` |
| `TrainHam` | `button` | 105–110 | `group id="SpamBayesGroup"` |
| `TestSpam` | `button` | 150–155 | `menu id="OtherSpamActions"` |
| `TriageSetA` | `button` | 445 | `group id="TriageGroup"` |
| `TriageSetB` | `button` | 446 | `group id="TriageGroup"` |
| `TriageSetC` | `button` | 447 | `group id="TriageGroup"` |
| `FilterTriageGroup` | `button` | 449–454 | `menu id="OtherTriageActions"` |
| `ClearTriage` | `button` | 455–460 | `menu id="OtherTriageActions"` |

**Schema permission (per Microsoft's CustomUI control reference, <https://learn.microsoft.com/en-us/previous-versions/office/developer/office-2007/aa338199(v=office.12)>):**

| Element | `enabled` / `getEnabled` permitted? |
|---|---|
| `button` | **Yes** — listed in the Common attribute set |
| `toggleButton` | **Yes** |
| `checkBox` | **Yes** |
| `menu` | **Yes** |
| `editBox`, `comboBox`, `dropDown`, `gallery`, `dynamicMenu`, `labelControl`, `splitButton`, `command` | Yes |
| `group`, `tab` | **No** — `group` and `tab` expose `getVisible` but **not** `getEnabled` |

**Recommendation: put `getEnabled` on the 8 buttons only.** Do **not** put it on `menu id="OtherSpamActions"` or `menu id="OtherTriageActions"`: those menus also contain save-location, folder-settings and enable-toggle commands that are safe and useful during initialisation, so disabling the container would over-restrict the UI. Do **not** attempt `group`-level disabling — the schema does not permit it.

### 1.6 Existing test patterns

`TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs` (161 lines) — parses the **embedded resource** `TaskMaster.Ribbon.RibbonExplorer.xml` via `typeof(RibbonController).Assembly.GetManifestResourceStream(...)` (lines 47–59) and asserts structural rules with `XDocument` + FluentAssertions. Fully deterministic, no COM, no temp files. **This is the correct home for the R2 `getEnabled` wiring assertions.**

`TaskMaster.Test\Ribbon\RibbonControllerTests.cs` (452 lines):

- `CreateController()` (lines 56–73) builds `(ApplicationGlobals)FormatterServices.GetUninitializedObject(typeof(ApplicationGlobals))`, reflectively sets the private `_quickFilerSettings` field, then reflectively sets `RibbonController.Globals`.
- **Can this pattern inject a `Mock<IAppItemEngines>`?** Mechanically yes: `ApplicationGlobals.Engines` (ApplicationGlobals.cs:461) is `public IAppItemEngines Engines { get; private set; }`, so its compiler-generated backing field `<Engines>k__BackingField` can be set by reflection on the uninitialized instance. **But it is the wrong seam for this work**, for three reasons:
  1. `RibbonController` is `[ExcludeFromCodeCoverage]` (RibbonController.cs:36), so nothing exercised through it counts toward the coverage floor.
  2. Reaching the readiness decision through `RibbonController.SB`/`Triage` would execute the `SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext())` side effect on the test thread (§1.3).
  3. It couples the tests to a private backing-field name, which is brittle.
  The pattern remains available as a *fallback* if the planner wants one end-to-end wiring test, but it must not be the primary coverage vehicle.

`TaskMaster.Test\AppGlobals\HookReadinessCoordinatorTests.cs` + `TaskMaster\AppGlobals\HookReadinessCoordinator.cs` — **the canonical precedent for this exact problem shape.** `HookReadinessCoordinator` (114 lines) is an `internal sealed` host-neutral decision seam that is *deliberately not* `[ExcludeFromCodeCoverage]`; its own doc comment (lines 36–46) states the pattern: *"This is the unit-tested decision seam. It contains no COM, no DispatcherTimer, and no clock … The DispatcherTimer and polling cadence that drive this coordinator are owned by the COM glue in `AppEvents.Hook()` and are COM/VSTO-exempt."* `EngineInitTimingProbe.cs` (110 lines) and `StartupDiagnosticsProbe.cs` carry the same explicit "intentionally NOT marked `[ExcludeFromCodeCoverage]`" remark. **The recommended design in §4 replicates this precedent.**

Internals are reachable: `[assembly: InternalsVisibleTo("TaskMaster.Test")]` at `TaskMaster\ThisAddIn.cs:14` and `TaskMaster\Properties\AssemblyInfo.cs:38`.

### 1.7 Where `InitAsync()` runs, and where `IRibbonUI` is reachable

Call sites of `AppItemEngines.InitAsync()`:

1. `ApplicationGlobals.cs:135` — `await Engines.InitAsync();` inside `LoadParallelAsync()`.
2. `ApplicationGlobals.cs:417` — `protected internal virtual Task InitializeEnginesPhaseAsync() => Task.Run(() => Engines.InitAsync());`, awaited at line 215 inside `LoadSequentialAsync()`.
3. `ApplicationGlobals.cs:427` — `IdleAsyncQueue.AddEntry(false, Engines.InitAsync);` inside `LoadWhenIdle()`. (No caller of `LoadWhenIdle()` was found outside the class.)

Live startup path: `ThisAddIn.Application_Startup` (ThisAddIn.cs:71–81) enqueues `await _globals.LoadAsync(false)` with **`useUiThread: true`**. `IdleAsyncQueue.OnApplicationIdle` (IdleAsyncQueue.cs:70–77) then runs it as `await await UiThread.Dispatcher.InvokeAsync(async () => { await entry.actionAsync(); await Task.Yield(); })`. `LoadAsync(false)` (ApplicationGlobals.cs:57, 80–82) selects `LoadSequentialAsync()`.

**Threading conclusion:** because the whole `LoadAsync` continuation chain is pumped by `UiThread.Dispatcher` (a WPF `DispatcherSynchronizationContext` on the STA), the continuation after `await InitializeEnginesPhaseAsync()` **does** resume on the UI/STA thread in the live path, even though the engine construction itself ran on a thread-pool thread via `Task.Run`. So a ribbon-refresh call placed after `await _globals.LoadAsync(false)` in `ThisAddIn` is already on the correct thread. However, the `LoadWhenIdle()` path enqueues with `useUiThread: false` (thread pool) and `LoadParallelAsync()` inherits whatever context its caller had. **Recommendation: marshal the refresh explicitly through `UtilitiesCS.Threading.UiThread.Dispatcher` (UiThread.cs:135) rather than relying on the ambient context.** `IRibbonUI` is a COM object handed to `Ribbon_Load` on the STA and must be called back on the STA.

`Office.IRibbonUI` is reachable only through `RibbonViewer._ribbon` (RibbonViewer.cs:56). The existing precedent for using it is `RibbonController.ToggleEventsHook(Office.IRibbonUI Ribbon)` (RibbonController.cs:182–198), which calls `Ribbon.InvalidateControl("BtnHookToggle")` — but it receives the `IRibbonUI` as a parameter from `RibbonViewer.BtnHookToggle_Click` (RibbonViewer.cs:135–138) rather than holding it. `RibbonController._viewer` is set by `RibbonViewer.Ribbon_Load` → `_controller.SetViewer(this)` (RibbonViewer.cs:92, RibbonController.cs:64–67), so a controller→viewer refresh call is reachable.

### 1.8 Office ribbon `getEnabled` / invalidation mechanics (documented)

- **Callback signature.** From Microsoft's callback signature table (<https://learn.microsoft.com/en-us/previous-versions/office/developer/office-2007/aa722523(v=office.12)>, Table 4): `getEnabled` → **`C#: bool GetEnabled(IRibbonControl control)`**. VSTO additionally requires the method be **`public`**, that its name match the XML attribute value exactly, and that its signature match a valid callback shape; *"If you create a callback method that does not match a valid signature, the code will compile, but nothing will occur when the user clicks the control."* (<https://learn.microsoft.com/en-us/visualstudio/vsto/ribbon-xml>).
- **One callback can serve many controls.** *"All callback methods have an `IRibbonControl` parameter that represents the control that called the method. You can use this parameter to reuse the same callback method for multiple controls."* (same page). So one `EngineCommand_GetEnabled` dispatching on `control.Id` is the documented pattern.
- **Response caching.** *"For each of the callbacks that the add-in implements, the responses are cached … This process remains in place for the control until the add-in signals that the cached values are invalid by using the `InvalidateControl` method, at which time, the callback procedure is again called and the return response is cached."* (<https://learn.microsoft.com/en-us/office/vba/api/office.iribbonui.invalidatecontrol>). **Therefore the post-initialisation refresh is load-bearing: without it the buttons stay disabled for the session.**
- `IRibbonUI.InvalidateControl(bstrControlID)` invalidates one control; `IRibbonUI.Invalidate()` invalidates every control's cached callback values.
- **Callback ordering is unspecified.** *"Is it possible to predict or control the order in which callbacks are called? No. You should not add logic to your Fluent UI solutions that depends on callbacks being called in a certain order."* (aa722523, §"Is it possible to predict…"). Tests must assert *set* membership of invalidated ids, not sequence.
- **Threading.** `IRibbonUI` is an Office COM object supplied on the STA during `onLoad`. Calling it from a thread-pool continuation is an STA/COM violation. `InitAsync()` is launched via `Task.Run` (ApplicationGlobals.cs:417), i.e. it *completes* on a thread-pool thread; only the awaiting continuation is marshalled back, and only when a synchronization context was captured. **Implication: the refresh call must be explicitly marshalled to `UiThread.Dispatcher` at the shim, not left to the ambient context.**

---

## 2. Behaviour semantics

**Readiness definition (per engine key `k`).** `k` is *ready* iff `Globals?.Engines?.InboxEngines` is non-null **and** `InboxEngines.TryGetValue(k, out var e)` is true **and** `e is not null`. String comparison is the `ConcurrentDictionary` default — **ordinal, case-sensitive** — so `"spam"` is not `"Spam"`.

**State model.**

| State | `InboxEngines` contents | `GetEnabled(id)` | Click behaviour |
|---|---|---|---|
| S0 — pre-init | empty (field initializer) | `false` | no-op + one diagnostic line; **no exception** |
| S1 — init in flight | still empty (single terminal assignment, §1.1) | `false` | as S0 |
| S2 — init complete, engine present | key present, non-null | `true` | **unchanged from today** (R6) |
| S3 — init complete, engine filtered out by config (`Engine == false`) or factory returned null | key absent | `false` | no-op + diagnostic; **no exception** |
| S4 — engine restarted (`RestartEngineAsync`) | key re-assigned | recomputed on next query | unchanged |
| S5 — `InitAsync` threw | empty | `false` | no-op; fail-safe |

**Transitions.** S0/S1 → S2 or S3 happens at the single `InboxEngines = …` assignment (AppItemEngines.cs:63). The ribbon does not observe the transition until an `InvalidateControl`/`Invalidate` call forces a re-query; until then the cached `false` persists.

**Success / failure conditions.**

- Success: a click during S0/S1/S3/S5 produces no exception and no user-visible side effect; a click in S2 behaves byte-identically to today.
- Failure: any exception escaping an `async void` handler; any change to which engines get constructed or in what order; any command that stays disabled after S2 + refresh.

**Edge cases.**

- `Globals` null (before `RibbonController.SetGlobals`) — must yield `false`, not NRE. Note `RibbonController.Engines` (Intelligence.cs:204) is **not** null-safe on `Globals`; the readiness accessor must be `() => Globals?.Engines`.
- Unknown / null / empty `control.Id` → `false` (a non-engine control must never be disabled by this callback).
- Ribbon not yet loaded (`_ribbon is null`) when the refresh fires → no-op, no throw.
- The action itself throwing (e.g. `TrainAsync` fails on a real mail item) → **must propagate**, per the repo's fail-fast rule. The guard suppresses *invocation*, never *errors*.
- Double refresh (e.g. startup refresh plus a post-`RestartEngineAsync` refresh) → idempotent.

---

## 3. Options analysis

### Option (a) — `Task InitTask` / `bool IsInitialized` on `AppItemEngines` + `IAppItemEngines`

Add a coarse "initialisation finished" signal, set at the end of `InitAsync()`.

- **R1:** satisfied in form.
- **R4:** *violated in spirit.* `InitAsync()` must gain a statement (`_isInitialized = true;` or capturing the task), which is a change inside the method R4 fences off. It is additive rather than reordering, but it is still an edit to the constrained method.
- **Correctness gap (decisive):** `InitAsync` filters on `config.Value.Engine` (AppItemEngines.cs:64) and drops null factories (line 83). In state S3 the flag reports `true` while `InboxEngines["Spam"]` is absent — so `TrainSpam_Click` would be *enabled* and would still NRE. It also does not react to `RestartEngineAsync` (line 111) replacing a key.
- **Coverage:** the flag lives on `[ExcludeFromCodeCoverage] AppItemEngines`; no coverable lines produced.
- **Ripple:** 1 production implementer (no DIM available, §1.2).
- **Verdict: rejected** — wrong granularity, produces a false-positive enabled state, touches `InitAsync`.

### Option (b) — per-engine predicate `bool IsEngineReady(string engineName)` on `IAppItemEngines`

- **R1:** satisfied, with the right granularity.
- **R4:** satisfied — `InitAsync()` untouched.
- **Coverage (decisive):** the implementation necessarily lives on `AppItemEngines`, which is `[ExcludeFromCodeCoverage]` (line 26). Because .NET Framework 4.8.1 has **no default-interface-member support** (§1.2), the body cannot be placed on the interface either. The new logic would therefore be **entirely uncoverable**, which is exactly the substitution the maintainer rejected in the issue #227 precedent (`[ExcludeFromCodeCoverage]` as a stand-in for a real testability seam).
- **Ripple:** 1 production class; Moq mocks unaffected at compile time (they would need `.Setup(x => x.IsEngineReady(...))` only where called).
- **Public surface:** widens a cross-project interface for a single consumer, against the "keep public surface minimal / prefer `internal`" rule.
- **Verdict: rejected** — correct semantics, uncoverable implementation.

### Option (c) — readiness computed from the existing `InboxEngines` member, in a new host-neutral type ✅ **SELECTED**

- **R1:** satisfied. The readiness signal is *published* — it is a first-class, named, unit-tested type (`EngineReadinessGate`) with an explicit contract, rather than an ad-hoc inline expression. `IAppItemEngines.InboxEngines` is already on the interface (line 9), so no contract change is needed to observe it.
- **R4:** satisfied **with a zero-line diff to `AppItemEngines.cs`** — the strongest possible compliance.
- **Correctness:** exactly per-key. Handles S3 (config-filtered engine) and S4 (`RestartEngineAsync`) correctly and automatically, because it reads the live dictionary.
- **Coverage (decisive advantage):** the gate, the control→engine catalog, and the click guard are all new types that are **not** `[ExcludeFromCodeCoverage]`, are constructed from plain delegates, and are directly unit-testable with Moq + FluentAssertions. This matches the `HookReadinessCoordinator` / `EngineInitTimingProbe` precedent verbatim.
- **Ripple:** **zero** test doubles and **zero** interface changes.
- **Race safety:** `ConcurrentDictionary.TryGetValue` is thread-safe, and `InitAsync` performs a single reference assignment of the whole dictionary, so a probe never observes a partially populated map.
- **Trade-off (recorded):** the readiness contract is a *convention* over an existing member rather than a compiler-enforced interface member. Mitigation: the gate is the single chokepoint, it is `internal sealed`, and it is covered by unit tests that pin the semantics.

**Rejected alternatives — one-line summary.** (a) `IsInitialized`/`InitTask`: wrong granularity (reports ready for engines that were config-filtered out), and requires editing `InitAsync()`. (b) `IsEngineReady` on `IAppItemEngines`: semantically correct but the body can only live on the `[ExcludeFromCodeCoverage]` `AppItemEngines` because net481 has no default interface members, so it produces no covered lines.

---

## 4. Recommended design

### 4.1 Seam choice

Per `.claude/rules/csharp.md` "DI Seams" preference order:

1. **Interface seam** — not used for the readiness contract itself (that is Option (b), rejected above). The gate *consumes* the existing `IAppItemEngines` interface, which already provides the needed observation point.
2. **Injectable delegate seam (chosen)** — `Func<IAppItemEngines>` supplies the engines container, and `Action<string>` supplies the diagnostic sink. This mirrors `EngineInitTimingProbe(Action<string> emit)` (EngineInitTimingProbe.cs:37) and `HookReadinessCoordinator(IOutlookReadinessGate, Action)` (HookReadinessCoordinator.cs:63) exactly.
3. **Adapter seam** — used for the ribbon: `Action<string> invalidateControl` adapts the static/COM `IRibbonUI.InvalidateControl` so the "which controls to invalidate" decision is testable without an `IRibbonUI`.

### 4.2 Type inventory (all new types in `TaskMaster\Ribbon\`, all `internal sealed`, none `[ExcludeFromCodeCoverage]`)

**`EngineCommandCatalog`** — pure static map, no dependencies.
- `static bool TryGetEngineName(string controlId, out string engineName)` — ordinal lookup; `false` for null/unknown.
- `static IReadOnlyCollection<string> ControlIds { get; }` — the 8 ids from §1.4.
- Rationale: the control-id↔engine-key binding is the one piece of knowledge shared by the XML, the `getEnabled` callback, the click guards, and the refresh; centralising it is what lets a single test assert XML/code agreement.

**`EngineReadinessGate`** — the R1 readiness signal.
- `EngineReadinessGate(Func<IAppItemEngines> enginesAccessor)`; null accessor → `ArgumentNullException` (constructor-time invariant, per repo policy).
- `bool IsEngineReady(string engineName)` — `false` for null/whitespace name, null accessor result, null `InboxEngines`, missing key, or null value; otherwise `true`.
- `bool TryGetEngine(string engineName, out IConditionalEngine<MailItemHelper> engine)` — same predicate, returning the instance, for callers that want the engine.
- Contains **zero** `Microsoft.Office.*` references.

**`EngineGatedCommandRunner`** — the R3 click guard and the `getEnabled` decision.
- `EngineGatedCommandRunner(EngineReadinessGate gate, Action<string> emitSkipDiagnostic)`.
- `bool IsCommandEnabled(string controlId)` → `EngineCommandCatalog.TryGetEngineName(...) && gate.IsEngineReady(name)`; `false` for unknown ids.
- `Task RunAsync(string controlId, Func<Task> action)` — null `action` → `ArgumentNullException`; if `!IsCommandEnabled(controlId)` emit **one** structured skip line and return `Task.CompletedTask` **without invoking `action`**; otherwise `await action()` and let any exception propagate (fail-fast).
- Critically, the caller passes a **lambda**, so `Controller.SB` / `Controller.Triage` are only dereferenced *inside* the lambda and are never evaluated when the gate is closed. This is what turns the NRE into a no-op without a `?.` sprinkle across `RibbonViewer`.

**`EngineCommandRefreshPlanner`** (or a static method on the catalog) — the R5c refresh decision.
- `static void InvalidateAll(Action<string> invalidateControl)` — null delegate → `ArgumentNullException`; invokes the delegate once per `EngineCommandCatalog.ControlIds` entry.
- Keeps "which controls to invalidate" coverable while `IRibbonUI.InvalidateControl` stays in the excluded shim.

### 4.3 Thin, uncovered shims (all in already-excluded types)

- `TaskMaster\Ribbon\RibbonController.EngineCommands.cs` (new partial of the existing `[ExcludeFromCodeCoverage]` `RibbonController`):
  - lazily builds `EngineReadinessGate(() => Globals?.Engines)` — the `?.` is what makes the pre-`SetGlobals` case safe;
  - exposes `internal bool IsEngineCommandEnabled(string controlId)`, `internal Task RunEngineCommandAsync(string controlId, Func<Task> action)`, and `internal void RefreshEngineCommands()` which forwards to `_viewer`.
- `TaskMaster\Ribbon\RibbonViewer.EngineCommands.cs` (new partial; requires changing `public class RibbonViewer` → `public partial class RibbonViewer` at RibbonViewer.cs:33):
  - `public bool EngineCommand_GetEnabled(Office.IRibbonControl control) => _controller?.IsEngineCommandEnabled(control?.Id) ?? false;` — the **only** new Office-typed surface;
  - `internal void InvalidateEngineCommands()` — null-checks `_ribbon`, marshals to `UiThread.Dispatcher` when `!Dispatcher.CheckAccess()`, then calls `EngineCommandRefreshPlanner.InvalidateAll(_ribbon.InvalidateControl)`;
  - hosts the relocated `#region Spam Manager` and `#region Triage` callbacks, rewritten as e.g.
    `public async void TrainSpam_Click(Office.IRibbonControl control) => await Controller.RunEngineCommandAsync("TrainSpam", () => Controller.SB.TrainAsync(Controller.OlSelection, true));`
- `TaskMaster\ThisAddIn.cs` — one statement after `await _globals.LoadAsync(false);` (line 76): `_ribbonController.RefreshEngineCommands();`. Already inside a `useUiThread: true` idle-queue entry, i.e. on the STA (§1.7).

### 4.4 Architecture-boundaries compliance (`.claude/rules/architecture-boundaries.md`)

Rules 1–4 ban new runtime code that references `Microsoft.Office.Tools.*` / `Microsoft.Office.Interop.Outlook`, exposes `[ComVisible(true)]`, or uses desktop ribbon callbacks. Findings:

- **No enforcement stage exists in this repository today.** There is no `*.ArchitectureTests` project (`rg`/glob for `*ArchitectureTests*` → no files) and no `quality-tiers.yml` at repo root (the file the rule requires for classification is absent). The rule set is aspirational for the future No-COM backend, and the ".NET (when the backend exists)" qualifier in the rule text is explicit.
- **The work is nevertheless compliant in substance** under the recommended design: no new `[ComVisible(true)]` type is introduced (the attribute stays on the existing `RibbonViewer`, and a `partial` split does not add a second attribute); the only new Office-typed member is one `bool EngineCommand_GetEnabled(Office.IRibbonControl)` inside the pre-existing COM-visible, coverage-exempt shim; and all four new decision types have **zero** `Microsoft.Office.*` or `Microsoft.Office.Interop.Outlook` using directives.
- **Recommendation to record in the plan:** state explicitly that the Office-typed surface added by this fix is exactly one method on an existing exempt shim, and that the decision logic is host-neutral and portable if the ribbon is later replaced by an Office.js command surface.

---

## 5. Requirements mapping and file-change list

| Ref | Satisfied by |
|---|---|
| R1 | `EngineReadinessGate` (new, covered) |
| R2 | `getEnabled="EngineCommand_GetEnabled"` on 8 buttons + `EngineCommand_GetEnabled` shim + XML regression tests |
| R3 | `EngineGatedCommandRunner.RunAsync` + rewritten handlers |
| R4 | **zero-line diff to `AppItemEngines.cs`** and to `IAppItemEngines.cs` |
| R5a | `EngineReadinessGateTests`, `EngineCommandCatalogTests`, `EngineGatedCommandRunnerTests`, extended `RibbonExplorerXmlTests` |
| R5b | Manual (see §8) |
| R5c | `EngineCommandRefreshPlanner` unit test + manual confirmation (see §8) |
| R6 | Handlers keep the identical `Controller.SB.TrainAsync(Controller.OlSelection, true)` expression inside the lambda; enabled-path tests assert the action is invoked exactly once |

### Enumerated file changes with predicted line deltas

| # | File (absolute) | Action | Current | Predicted | Δ |
|---|---|---|---|---|---|
| 1 | `C:\...\TaskMaster\Ribbon\EngineCommandCatalog.cs` | **new** | — | ~75 | +75 |
| 2 | `C:\...\TaskMaster\Ribbon\EngineReadinessGate.cs` | **new** | — | ~85 | +85 |
| 3 | `C:\...\TaskMaster\Ribbon\EngineGatedCommandRunner.cs` | **new** | — | ~90 | +90 |
| 4 | `C:\...\TaskMaster\Ribbon\EngineCommandRefreshPlanner.cs` | **new** | — | ~45 | +45 |
| 5 | `C:\...\TaskMaster\Ribbon\RibbonController.EngineCommands.cs` | **new** (partial) | — | ~75 | +75 |
| 6 | `C:\...\TaskMaster\Ribbon\RibbonViewer.EngineCommands.cs` | **new** (partial) | — | ~150 | +150 |
| 7 | `C:\...\TaskMaster\Ribbon\RibbonViewer.cs` | edit: `class`→`partial class` (line 33); move `#region Spam Manager` (250–296) and `#region Triage` (298–347) into file 6 | 487 | **~389** | **−98** |
| 8 | `C:\...\TaskMaster\Ribbon\RibbonExplorer.xml` | edit: add `getEnabled` to 8 buttons | 519 | 527 | +8 |
| 9 | `C:\...\TaskMaster\ThisAddIn.cs` | edit: 1 refresh call + why-comment after line 76 | 300 | ~303 | +3 |
| 10 | `C:\...\TaskMaster\TaskMaster.csproj` | edit: 6 `<Compile Include>` entries near lines 407–463 | 1000+ | +6 | +6 |
| 11 | `C:\...\TaskMaster\AppGlobals\AppItemEngines.cs` | **NO CHANGE** (R4) | 286 | 286 | 0 |
| 12 | `C:\...\UtilitiesCS\Interfaces\IGlobals\IAppItemEngines.cs` | **NO CHANGE** | 18 | 18 | 0 |
| 13 | `C:\...\TaskMaster\AppGlobals\ApplicationGlobals.cs` | **NO CHANGE** | — | — | 0 |
| 14 | `C:\...\TaskMaster.Test\Ribbon\EngineCommandCatalogTests.cs` | **new** | — | ~140 | +140 |
| 15 | `C:\...\TaskMaster.Test\Ribbon\EngineReadinessGateTests.cs` | **new** | — | ~190 | +190 |
| 16 | `C:\...\TaskMaster.Test\Ribbon\EngineGatedCommandRunnerTests.cs` | **new** | — | ~210 | +210 |
| 17 | `C:\...\TaskMaster.Test\Ribbon\EngineCommandRefreshPlannerTests.cs` | **new** | — | ~90 | +90 |
| 18 | `C:\...\TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs` | edit: add 4 tests | 161 | ~265 | +104 |
| 19 | `C:\...\TaskMaster.Test\TaskMaster.Test.csproj` | edit: 4 `<Compile Include>` entries near line 311 | — | +4 | +4 |

Every predicted file stays under the 500-line cap. Note the **net −98 on `RibbonViewer.cs`** is what buys headroom; without the region move, `RibbonViewer.cs` would land at roughly 495–505 lines and risk breaching the cap.

### Test-double ripple (exhaustive)

**Zero.** No `IAppItemEngines` member is added, so none of the four `Mock<IAppItemEngines>` sites (`ApplicationGlobalsTests.cs:92,120,334`; `ApplicationGlobalsStartupTimingTests.cs:183`; `AppEventsTests.Helpers.cs:20,39`) and none of the seven `IApplicationGlobals` hand-rolled doubles (`EfcHomeControllerTests.cs:212`, `EfcHomeControllerMetricsTests.cs:218`, `EfcHomeControllerLifecycleTests.cs:398`, `EmailDataMiner_TestSupport.cs:71`, `AppToDoObjectsTestDoubles.cs:171`, `AppOlObjectsTests.cs:419`, `AppOlObjectsCoverageTests.cs:328`) requires an edit. This is the single largest practical advantage of Option (c) over Options (a)/(b), which would each require editing `AppItemEngines.cs`.

---

## 6. Testing implications (no test code written)

Framework: **MSTest** + **Moq** + **FluentAssertions** only. Every test below is deterministic, uses no temp files, no `Thread.Sleep`/`Task.Delay`, no WinForms message pump, no `Form`/`MessageBox`, and no live COM. `IConditionalEngine<MailItemHelper>` is mockable (`new Mock<IConditionalEngine<MailItemHelper>>().Object` — precedent at `EngineInitTimingProbeTests.cs:21–22`).

**`EngineReadinessGateTests` (R1, R5a)**
1. `IsEngineReady_WhenAccessorReturnsNull_ReturnsFalse` — models pre-`SetGlobals`.
2. `IsEngineReady_WhenInboxEnginesIsEmpty_ReturnsFalse` — **the #503 repro window**.
3. `IsEngineReady_WhenKeyPresentWithNonNullEngine_ReturnsTrue`.
4. `IsEngineReady_WhenKeyPresentWithNullValue_ReturnsFalse`.
5. `IsEngineReady_WithNullOrWhitespaceName_ReturnsFalse`.
6. `IsEngineReady_IsOrdinalCaseSensitive` — `"spam"` vs `"Spam"`.
7. `IsEngineReady_AfterDictionaryPopulated_ReturnsTrue` — mutate the same `ConcurrentDictionary` between two calls; models the S1→S2 transition and `RestartEngineAsync`, with no timing dependency.
8. `TryGetEngine_WhenReady_OutputsSameInstance` / `_WhenNotReady_OutputsNull`.
9. `Constructor_WithNullAccessor_ThrowsArgumentNullException`.

**`EngineCommandCatalogTests` (R2, R5a)**
10. `TryGetEngineName_ForEachEngineBackedControlId_ReturnsExpectedEngineName` — data rows for all 8 ids from §1.4.
11. `TryGetEngineName_ForUnknownControlId_ReturnsFalse`.
12. `TryGetEngineName_WithNullControlId_ReturnsFalse`.
13. `ControlIds_ContainsExactlyTheEightEngineBackedControlIds`.
14. `ControlIds_ContainsNoDuplicates`.

**`EngineGatedCommandRunnerTests` (R3, R6, R5a)**
15. `RunAsync_WhenEngineNotReady_DoesNotInvokeAction` — **the primary regression test for #503**: asserts no throw and that the action delegate was never entered.
16. `RunAsync_WhenEngineNotReady_EmitsOneSkipDiagnosticContainingControlIdAndEngineName`.
17. `RunAsync_WhenEngineReady_InvokesActionExactlyOnce` — R6.
18. `RunAsync_WhenEngineReady_AwaitsActionToCompletion` — drive with a `TaskCompletionSource` completed synchronously by the test; no delays.
19. `RunAsync_WhenActionThrows_PropagatesException` — fail-fast; the guard must not become a swallow-all.
20. `RunAsync_WithUnknownControlId_DoesNotInvokeAction`.
21. `RunAsync_WithNullAction_ThrowsArgumentNullException`.
22. `IsCommandEnabled_ReturnsFalseWhenNotReady_TrueWhenReady_FalseForUnknownId` (three focused tests) — this is the `getEnabled` decision.

**`EngineCommandRefreshPlannerTests` (R5c, R5a)**
23. `InvalidateAll_InvokesDelegateOnceForEachEngineBackedControlId` — assert the captured id **set** equals `EngineCommandCatalog.ControlIds` (not the order — Office documents callback order as unspecified).
24. `InvalidateAll_WithNullDelegate_ThrowsArgumentNullException`.

**`RibbonExplorerXmlTests` additions (R2, R5a)**
25. `RibbonExplorerXml_EveryEngineBackedControlDeclaresGetEnabledCallback` — for each `EngineCommandCatalog.ControlIds` entry, the element exists in the XML and carries `getEnabled="EngineCommand_GetEnabled"`.
26. `RibbonExplorerXml_GetEnabledCallbackMatchesOfficeSignatureOnRibbonViewer` — reflection over `typeof(RibbonViewer)`: a `public` instance method `EngineCommand_GetEnabled` returning `bool` with exactly one parameter of type `Microsoft.Office.Core.IRibbonControl`. This is the guard against the documented "wrong signature compiles but silently does nothing" failure mode.
27. `RibbonExplorerXml_GetEnabledIsDeclaredOnlyOnEngineBackedControls` — no other element carries `getEnabled="EngineCommand_GetEnabled"`, so the fix cannot silently over-disable the UI.
28. `RibbonExplorerXml_EngineBackedControlsAreSchemaLegalForGetEnabled` — every catalog id resolves to a `button`/`toggleButton`/`checkBox`/`menu` element (the schema-permitted set, §1.5), never `group`/`tab`.

**Coverage targets.** CLAUDE.md (authority #1) requires repository line coverage `>= 80%` and `>= 90%` for any new module/class/method; `.claude/rules/general-unit-test.md` states `>= 85%` line / `>= 75%` branch. Target the **stricter** of the two: the four new types should reach `>= 90%` line coverage, which is achievable because they contain no host-bound code. Baseline and post-change coverage XML must be captured under `docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/baseline/` and `.../evidence/qa-gates/` respectively (canonical evidence locations).

**Explicitly NOT tested by unit tests** (and why): `RibbonViewer.EngineCommand_GetEnabled`, `RibbonViewer.InvalidateEngineCommands`, `RibbonController.*` shims, and the `ThisAddIn` refresh call. All live in `[ExcludeFromCodeCoverage]` types and consist of null-checks plus one delegating call each. This matches the ratified COM/VSTO/WinForms exemption in CLAUDE.md and the `HookReadinessCoordinator` precedent. The exemption is being used for *thin wiring only* — the decision logic is fully covered — which is the distinction the maintainer required in the #227 precedent.

---

## 7. Constraints recorded

- **R4 honoured absolutely:** `AppItemEngines.InitAsync()` engine construction, config loading, and dictionary-population order are untouched; `AppItemEngines.cs` has a **zero-line diff**.
- **500-line cap** (`.claude/rules/general-code-change.md`): `RibbonViewer.cs` is at **487/500** and `RibbonController.Intelligence.cs` at **412/500**. Adding the new callbacks directly to `RibbonViewer.cs` would breach the cap. **Recommendation: partial-class split** into `RibbonViewer.EngineCommands.cs` (requires `public class` → `public partial class` at RibbonViewer.cs:33) **and** relocation of the `#region Spam Manager` / `#region Triage` blocks (lines 250–347) into it, bringing `RibbonViewer.cs` to ~389 lines. `RibbonController` is already `partial`, so `RibbonController.EngineCommands.cs` needs no declaration change.
- **`RibbonExplorer.xml` is already 519 lines**, above the 500 guidance, *before* this change; the 8 added attributes take it to ~527. The cap text scopes to "production code, test code, or reusable script file"; an embedded declarative UI resource is arguably outside that scope, and the overage is pre-existing. **Recommendation: record the fact, do not expand scope to split the ribbon XML in this bugfix.** If the planner disagrees, splitting the ribbon into multiple embedded resources is a separate, larger change (the loader at `RibbonViewer.GetCustomUI`, line 70–84, returns a single resource).
- **Toolchain note:** CSharpier is file-based and formats `*.cs` only — it will **not** reformat `RibbonExplorer.xml`. The XML must be hand-edited to match the surrounding one-attribute-per-line style.
- **Nullable:** `TaskMaster.csproj` declares no `<Nullable>` element, but the type-check gate runs `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true`. New files must be nullable-clean under that global override. Follow the `HookReadinessCoordinator` style (guard clauses + `?? throw new ArgumentNullException`) rather than the scoped `#nullable enable annotations` pragma used in `EngineInitTimingProbe.cs:60`, which was only needed there for a pre-existing `?` annotation.
- **Determinism:** no `Thread.Sleep` / `Task.Delay` (also banned via `BannedSymbols.txt`), no wall-clock reads, no temp files, no message pump, no live COM.

---

## 8. Automation Feasibility

**Can be verified fully automatically (no human, no third-party UI):**

- R1 — readiness semantics across all six states S0–S5 (`EngineReadinessGateTests`).
- R3 — the guard suppresses invocation and does not swallow real errors (`EngineGatedCommandRunnerTests`).
- R2 — that the 8 controls declare `getEnabled`, that no other control does, that each is a schema-legal element type, and that the named callback exists on `RibbonViewer` with the exact Office-required signature `bool (Office.IRibbonControl)` (extended `RibbonExplorerXmlTests`, using the embedded resource — no Outlook needed).
- R5c (partially) — that the refresh planner requests invalidation for exactly the 8 engine-backed control ids (`EngineCommandRefreshPlannerTests`).
- R4 — enforceable as a diff assertion: `AppItemEngines.cs` and `IAppItemEngines.cs` must appear with zero changed lines in the branch diff. (Precedent for source-text assertions in this suite: `RibbonControllerTests.RibbonFolderOperations_DoNotConstructThrowawayFolderTrees`, lines 296–311, reads a production file and asserts on its text.)
- Full toolchain: CSharpier, both msbuild gates, and `vstest.console.exe /EnableCodeCoverage`.

**Requires human interaction with a third-party UI — cannot be automated in this repository:**

- **R5b** — clicking each of the 8 ribbon commands in a live Outlook Explorer immediately after add-in reload, and confirming no `NullReferenceException`/`KeyNotFoundException` appears in the log. This requires a running Outlook process, a live mail profile, and physical/UI-automated clicks on the Office ribbon. There is no Outlook UI-automation harness in this repository, and the general unit-test policy prohibits tests that depend on external processes.
- **R5c (the visual half)** — confirming that Office actually greys the buttons out during initialisation and re-enables them after `InvalidateControl` fires. Office's callback-caching behaviour is internal to the host; only Outlook can demonstrate it. The `getEnabled` cache/invalidate contract is documented (§1.8) but not locally observable.
- Confirming that the `getEnabled` callback is *bound* at all (VSTO silently ignores signature mismatches). Test 26 reduces this risk to near zero by asserting the reflected signature, but only a live load proves binding.

**Recommended handling.** Treat R5b and R5c-visual as a **documented manual verification checklist** in the feature folder, executed by the maintainer, with the outcome recorded under `docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/manual-verification/`. Do not attempt to satisfy them with an automated test; do not mark the corresponding acceptance criteria complete on the strength of unit tests alone. Everything else on the acceptance list is automatable and should gate the PR.

**Partial mitigation available.** `TaskMaster.Test\AppGlobals\LiveOutlookHarnessRunner.cs` / `LiveOutlookHookupIntegrationTests.cs` exist as a live-Outlook harness precedent. Extending that harness to drive ribbon callbacks was **not** evaluated in depth here; it would still require a live Outlook process and therefore cannot run in the standard deterministic unit-test pass. Flagged as optional follow-up, not a recommendation for this fix.

---

## 9. Out-of-scope defects discovered (recommend promotion to separate issues)

These were found while establishing ground truth. None should be fixed inside #503; each should be promoted through the issue-promotion lifecycle so it is not lost when this feature folder merges.

1. **Five orphan `onAction` callbacks in `RibbonExplorer.xml`.** Verified by `rg` over all `*.cs`: no method exists for `BtnMigrateIDs_Click` (XML line 82), `MoveEntireConversation_Clicked` (line 262), `SaveAttachments_Clicked` (line 268), `SaveEmailCopy_Clicked` (line 274), `SavePictures_Clicked` (line 280). The `RibbonViewer` methods are named `MoveEntireConversation_Click`, `SaveAttachments_Click`, `SaveEmailCopy_Click`, `SavePictures_Click` (RibbonViewer.cs:180, 186, 192, 198) — a `_Clicked` vs `_Click` suffix mismatch. Per Microsoft's VSTO guidance the code compiles and **nothing happens when the user clicks the control**, so all four Quick Filer settings check boxes and the Migrate-IDs button are silently inert. Severity: medium (silent feature loss). A generalised "every `onAction`/`get*` callback resolves to a public `RibbonViewer` method" XML test would catch this class of bug, but it would fail immediately on these five, so it must ship with the fix, not with #503.

2. **Invalid `getPressed` callback signatures.** `RibbonViewer.SpamBayesEnabled_GetPressed` (line 279) and `TriageEnabled_GetPressed` (line 333) are declared `public async Task<bool> ...(Office.IRibbonControl control)`. The documented signature is `bool GetPressed(IRibbonControl control)`; an `async Task<bool>` does not match, so the check-box pressed state is never applied. Both are referenced from the XML (`getPressed="SpamBayesEnabled_GetPressed"` line 140, `getPressed="TriageEnabled_GetPressed"` line 501).

3. **Fire-and-forget `ToggleEngineAsync`.** `SpamBayesEnabled_Click` (line 276–277) and `TriageEnabled_Click` (line 330–331) call `Controller.Engines.ToggleEngineAsync(...)` without `await` in a `void` method, discarding the task and any exception.

4. **`RibbonController.Engines` is not null-safe on `Globals`** (Intelligence.cs:204), unlike the sibling `SB`/`Triage` properties which use `Globals?.`. Reached by `TestSpam_Click` and all six `Spam*`/`Triage*` config callbacks before `SetGlobals` runs.

---

## 10. Summary of the recommendation

Introduce a **per-key readiness signal computed from the existing `IAppItemEngines.InboxEngines` member**, implemented in four new host-neutral `internal sealed` types under `TaskMaster\Ribbon\` that are deliberately *not* `[ExcludeFromCodeCoverage]`, following the `HookReadinessCoordinator` precedent. Wire them into the ribbon through one new `public bool EngineCommand_GetEnabled(Office.IRibbonControl)` shim on a new `RibbonViewer` partial, add `getEnabled="EngineCommand_GetEnabled"` to the 8 engine-backed `<button>` elements, route the 8 affected click handlers through a gated runner whose lambda defers the engine dereference, and invalidate those 8 controls once from `ThisAddIn` after `LoadAsync` completes on the STA. `AppItemEngines.cs` and `IAppItemEngines.cs` are not modified at all, the test-double ripple is zero, and all decision logic is unit-testable with MSTest + Moq + FluentAssertions.
