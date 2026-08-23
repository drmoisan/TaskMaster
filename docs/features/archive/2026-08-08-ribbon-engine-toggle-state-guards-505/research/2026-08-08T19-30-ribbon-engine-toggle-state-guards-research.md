# Research — ribbon-engine-toggle-state-guards (#505, #506, #518)

- **Issues:** #505 (invalid `getPressed` signature), #506 (fire-and-forget toggle), #518 (unguarded `Engines` dereference)
- **Work mode:** full-bug (bundled)
- **Branch:** `bug/ribbon-engine-toggle-state-guards-505`, from `origin/main` at `f910ff2f`
- **Last Updated:** 2026-08-08T19-30
- **Author:** task-researcher agent
- **Status:** Complete

All file paths below are relative to the worktree root
`C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a406ae4b7a2ce151f`.
All line numbers were verified against the current branch head in this worktree.

---

## 1. Current State Analysis

### 1.1 The defective file

`TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` (207 lines):

| Line | Member | Defect |
|---|---|---|
| 119–120 | `SpamBayesEnabled_Click(control, bool pressed)` | `void`, body is unawaited `Controller.Engines.ToggleEngineAsync(SpamBayes.GroupName)` (#506, #518) |
| 122–123 | `SpamBayesEnabled_GetPressed(control)` | `async Task<bool>` — Office cannot bind it (#505, #518) |
| 125–126 | `SpamSaveNetwork_Click` | `async void` + `await Controller.Engines.ShowDiskDialog(...)`, unguarded (#518) |
| 128–129 | `SpamSaveLocal_Click` | same shape, unguarded (#518) |
| 131–132 | `GetSaveLocation_Click` | `void`, `Controller.Engines.ShowSaveInfo(...)`, unguarded (#518) |
| 188–189 | `TriageEnabled_Click` | mirror of 119–120 (#506, #518) |
| 191–192 | `TriageEnabled_GetPressed` | mirror of 122–123 (#505, #518) |
| 194–195 | `TriageSaveNetwork_Click` | unguarded (#518) |
| 197–198 | `TriageSaveLocal_Click` | unguarded (#518) |
| 200–201 | `TriageGetSaveLocation_Click` | unguarded (#518) |

`TestSpam_Click` (lines 100–107) is already gated through
`Controller.RunEngineCommandAsync("TestSpam", () => ...)` and must remain functionally unchanged
(AC11). Total `Engines.` references in the file: 11; unguarded: exactly 10, matching the issue
table.

### 1.2 The #503 seam this work must extend (all verified)

- `TaskMaster/Ribbon/EngineCommandCatalog.cs:36-49` — `Map` contains exactly 8 control ids:
  `TrainSpam`, `TrainHam`, `TestSpam` → `"Spam"`; `TriageSetA/B/C`, `ClearTriage`,
  `FilterTriageGroup` → `"Triage"`. **None of the ten defect sites' control ids are present.**
- `TaskMaster/Ribbon/EngineReadinessGate.cs:64-101` — per-key probe of
  `enginesAccessor()?.InboxEngines`; a null accessor result is "not ready" by contract (lines
  37-41).
- `TaskMaster/Ribbon/EngineGatedCommandRunner.cs:97-111` — `RunAsync(controlId, Func<Task>)`
  defers the engine dereference into the lambda; gate closed → one notification, no invocation.
  Deliberately contains **no catch clause** (lines 22-25).
- `TaskMaster/Ribbon/EngineCommandRefreshPlanner.cs:45-56` — `InvalidateAll(Action<string>)`
  iterates `EngineCommandCatalog.ControlIds`.
- `TaskMaster/Ribbon/RibbonController.EngineCommands.cs` — thin glue:
  `EngineCommands` lazy property (lines 38-45, accessor `() => Globals?.Engines!`),
  `IsEngineCommandEnabled` (54-57), `RunEngineCommandAsync` (70-73), `RefreshEngineCommands`
  (82-85), `NotifyEngineCommandNotReady` (97-101, `logger.Warn` + `MessageBox.Show`).
- `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs:38-39` — `EngineCommand_GetEnabled` is the
  synchronous-callback precedent: `_controller?.IsEngineCommandEnabled(control?.Id!) ?? false`.
- `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs:63-81` — `InvalidateEngineCommands` marshals
  `IRibbonUI.InvalidateControl` through `UtilitiesCS.UiThread.Dispatcher` when off the STA.
- `TaskMaster/ThisAddIn.cs:82` — the single production caller of
  `_ribbonController.RefreshEngineCommands()`, inside the post-`LoadAsync` idle-queue lambda.

### 1.3 Engine-side semantics (drives the guard-shape decision in §5)

`TaskMaster/AppGlobals/AppItemEngines.cs` (type-level `[ExcludeFromCodeCoverage]`, line 26):

- `ToggleEngineAsync` (92-99) and `EngineActiveAsync` (101-109) `await
  Globals.AF.Manager.Configuration` and read/flip `loader.Config.ClassifierActivated`. They
  operate on **configuration**, not on `InboxEngines`.
- `InitAsync` (63-64) filters `config.Value.Engine` before populating `InboxEngines`, so an
  engine that is configured off never enters `InboxEngines`.
- `ShowDiskDialog` (237-252) and `ShowSaveInfo` (276-282) both start with
  `InboxEngines.TryGetValue(engineName, ...)` and no-op when the key is absent. They can only do
  useful work when the engine is **present in `InboxEngines`**.

Consequence: the `EngineReadinessGate` (keyed on `InboxEngines`) is the semantically correct
guard for the six save/info command sites, but the **wrong** guard for the two toggle pairs — a
user must be able to toggle an engine that is currently inactive (absent from `InboxEngines`),
and `getPressed` must report state for an inactive engine. This asymmetry is the core of §4/§5.

- `Configuration` is `AsyncLazy<ConcurrentDictionary<string, SmartSerializableLoader>>`
  (`UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs:54`). `AsyncLazy<T>`
  (`UtilitiesCS/ReusableTypeClasses/AsyncLazy/AsyncLazy.cs:19-49`) wraps `Lazy<Task<T>>` with
  factories dispatched via `Task.Run`; it exposes only await/`Start` — **no non-triggering
  "completed value" probe**.
- Engine keys: `SpamBayes.GroupName == "Spam"`
  (`UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.cs:328`); Triage uses the
  literal `"Triage"` throughout.

### 1.4 Coverage and nullable context

- `RibbonViewer.cs:31-32` (`[ComVisible(true)]`, `[ExcludeFromCodeCoverage]`) and
  `RibbonController.cs:36` (`[ExcludeFromCodeCoverage]`) are type-level, so both partial files
  are exempt. The four #503 seam types are deliberately NOT exempt and are fully unit-tested —
  this is the placement precedent (AC14).
- **Nullable:** no file under `TaskMaster/Ribbon/` carries a `#nullable` pragma (verified by
  grep; the only `#nullable` directives in the `TaskMaster` project are in five `AppGlobals`
  files). The #503 seam files instead use targeted null-forgiving operators with explanatory
  comments (`EngineCommandCatalog.cs:78-81`, `RibbonController.EngineCommands.cs:40-43`,
  `RibbonViewer.EngineCommands.cs:34-37`). **Recommendation:** new files follow the same
  convention — no `#nullable enable` pragma, null contracts documented in XML doc comments,
  null-forgiving used only where the #503 files already model it. This keeps the diff consistent
  with the sibling seam files and avoids new annotation obligations under the (defective, #522)
  `/p:Nullable=enable` gate; the type-check gate to reason about is CI's command without that
  flag.

---

## 2. Research Question A — the Office `getPressed` contract and XML wiring

### 2.1 Required signatures

For the Office 2009 CustomUI `checkBox` control (both defect controls are `checkBox`, not
`toggleButton`):

- `getPressed` → `public bool <Name>(Office.IRibbonControl control)` — synchronous, returns
  `bool`, single `IRibbonControl` parameter.
- `onAction` (checkBox/toggleButton form) → `public void <Name>(Office.IRibbonControl control,
  bool pressed)`.

VSTO matches callbacks by name and signature and **silently ignores a mismatch** — the code
compiles and Office simply never invokes the method. This is documented in-repo at
`RibbonViewer.EngineCommands.cs:27-32` ("VSTO silently ignores a signature mismatch — the code
compiles and nothing happens — which is why `RibbonExplorerXmlTests` pins this signature by
reflection"). Working in-repo exemplars of the exact required shapes:
`ToggleDarkMode_GetPressed` / `ToggleDarkMode_Click` (`RibbonViewer.cs:167-171`) and the four
QF-settings pairs (`RibbonViewer.cs:177-199`).

### 2.2 XML wiring (`TaskMaster/Ribbon/RibbonExplorer.xml`)

| Control id | Element | Line | Wiring |
|---|---|---|---|
| `SpamBayesEnabledToggle` | `checkBox` | 140-145 | `getPressed="SpamBayesEnabled_GetPressed"`, `onAction="SpamBayesEnabled_Click"` |
| `TriageEnabledToggle` | `checkBox` | 519-524 | `getPressed="TriageEnabled_GetPressed"`, `onAction="TriageEnabled_Click"` |
| `SpamSaveNetwork` | `button` | 115-120 | `onAction="SpamSaveNetwork_Click"`, **no `getEnabled`** |
| `SpamSaveLocal` | `button` | 121-126 | `onAction="SpamSaveLocal_Click"`, no `getEnabled` |
| `GetSaveState` | `button` | 127-132 | `onAction="GetSaveLocation_Click"`, no `getEnabled` |
| `TriageSaveNetwork` | `button` | 500-505 | `onAction="TriageSaveNetwork_Click"`, no `getEnabled` |
| `TriageSaveLocal` | `button` | 506-511 | `onAction="TriageSaveLocal_Click"`, no `getEnabled` |
| `TriageGetSaveState` | `button` | 512-517 | `onAction="TriageGetSaveLocation_Click"`, no `getEnabled` |

Note the id/engine-name divergence: the "current location" buttons are `GetSaveState` /
`TriageGetSaveState` (not `*SaveLocation*`). The catalog additions in §5 must use these exact
ids.

### 2.3 The `RibbonExplorerXmlTests` pattern to extend

`TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs`:

- `LoadRibbonDocument()` (48-60) loads the embedded resource
  `TaskMaster.Ribbon.RibbonExplorer.xml` via `typeof(RibbonController).Assembly`.
- Catalog↔XML agreement is asserted three ways, all **derived from
  `EngineCommandCatalog.ControlIds`** (no duplicated literal list):
  1. every catalog id exists in XML and declares `getEnabled="EngineCommand_GetEnabled"`
     (177-216);
  2. **set equality** — only catalog ids may declare that callback (224-245);
  3. every catalog id is a `button` element, because `group`/`tab` do not permit `getEnabled`
     (253-273).
- The signature pin (289-314): reflect the method off `typeof(RibbonViewer)` with
  `BindingFlags.Public | BindingFlags.Instance`, assert `ReturnType` is `bool`, exactly one
  parameter, and compare the parameter type by **`Type.FullName ==
  "Microsoft.Office.Core.IRibbonControl"`** — the test project has no compile-time reference to
  the Office PIA (documented at lines 280-287), so `typeof(Office.IRibbonControl)` is not
  available in test code. Any new signature pin must copy this `FullName` comparison.

Implication of the set-equality tests: adding ids to `EngineCommandCatalog` **forces** matching
`getEnabled` attributes into the XML in the same change, and adding a `checkBox` id would fail
test (3) ("must be a button"). This is load-bearing for the §5 design split.

---

## 3. Research Question B — synchronous `getPressed` design

A `checkBox` `getPressed` must return `bool` on the STA, but the truth
(`EngineActiveAsync`) is behind `await Globals.AF.Manager.Configuration` (§1.3). Options:

### 3.1 Option B-block — block the STA (`.Result` / `.Wait()` / `GetAwaiter().GetResult()`) — REJECTED

- **Deadlock risk:** ribbon callbacks run on the Outlook STA. The controller's `SB`/`Triage`
  getters and many ribbon paths install a `WindowsFormsSynchronizationContext` on that thread
  (`RibbonController.Intelligence.cs:194-197, 283-286`). If `EngineActiveAsync` is started on a
  thread with that context, its continuation after `await configs` is posted back to the STA;
  blocking the STA in `.Result` while the continuation needs the STA message pump is a
  deterministic deadlock. Whether the context is installed at the moment Office queries
  `getPressed` depends on which callbacks ran first — i.e. the failure is intermittent, the
  worst kind.
- **Latency even when it does not deadlock:** the first `Configuration` await triggers the full
  classifier-configuration disk load inside the `AsyncLazy` factory. Office queries `getPressed`
  when the menu opens; blocking the UI thread for a multi-second disk load on menu-open is the
  exact freeze class issue #211/#424 instrumentation exists to hunt.
- **Policy:** the repo's determinism rules ban blocking waits in tests, and CLAUDE.md C#2.4
  requires `async`/`await` for I/O-bound operations. A production blocking wait here would also
  be untestable deterministically.

### 3.2 Option B-lazy-read — synchronously read the materialized `Configuration` when complete, else fall back — REJECTED

`AsyncLazy<T>` exposes no non-triggering completed-value probe (§1.3); reading it would require
either touching `Lazy<Task<T>>.Value.Result` (blocking/triggering, see B-block) or adding
surface to `AsyncLazy<T>`/`IAppItemEngines`. A new `IAppItemEngines` member is the known #503
dead end: net481 has no default interface members, so the body must live on `AppItemEngines`,
which is `[ExcludeFromCodeCoverage]` (line 26) — the logic would be uncoverable by construction.
It also couples the ribbon layer to `Globals.AF.Manager`, which `IAppItemEngines` deliberately
hides.

### 3.3 Option B-cache — last-known-state cache with async prime and invalidation — **RECOMMENDED**

A host-neutral coordinator owns a per-engine-key `bool` cache:

- **Synchronous read.** `GetPressed(engineName)` returns the cached value; a never-primed key
  returns `false` (unchecked), which is also the correct pre-`SetGlobals` degradation (AC12) and
  matches `EngineCommand_GetEnabled`'s "false when unknown" convention.
- **Lazy prime.** On a cache miss with engines available, `GetPressed` starts (at most one)
  prime task per key: `await enginesAccessor().EngineActiveAsync(name)` → store → invoke the
  injected `invalidateControl(controlId)` delegate so Office re-queries `getPressed` and the
  checkbox corrects itself. Prime faults are observed and routed to the injected error-log
  delegate — no unobserved task. This removes any need to touch `ThisAddIn` and covers the
  "menu opened before `LoadAsync` finished" window that an eager post-load prime would miss.
- **Refresh on toggle (AC9).** The toggle path (§5.1) awaits `ToggleEngineAsync`, re-reads
  `EngineActiveAsync`, updates the cache, **then** invalidates the control — the
  update-before-invalidate ordering is the invariant that prevents Office re-querying stale
  state, and is unit-testable as a recorded call sequence.
- **Correctness under ordering:** single writer per key for the toggle path (user gesture on the
  STA); the prime task and a concurrent toggle both end by writing the freshest
  `EngineActiveAsync` result, and every write is followed by an invalidation, so the UI
  converges. A `ConcurrentDictionary<string, bool>` read is safe from any thread.
- **STA safety:** `GetPressed` performs a dictionary read only. The prime/toggle awaits run
  wherever the continuation lands; the only STA-affine call, `IRibbonUI.InvalidateControl`,
  stays behind the injected delegate whose production implementation marshals through
  `UiThread.Dispatcher` exactly like `InvalidateEngineCommands`
  (`RibbonViewer.EngineCommands.cs:71-80`).
- **Before `SetGlobals`:** `enginesAccessor()` returns null → `GetPressed` returns `false` and
  starts nothing; a click notifies-and-no-ops (§5.1). No NRE (AC12).
- **Testability:** constructor-injected `Func<IAppItemEngines>`, `Action<string>` invalidate,
  and error-log delegate; `IAppItemEngines` contains no COM types, so a plain
  `Mock<IAppItemEngines>` plus `TaskCompletionSource`-backed setups give full deterministic
  coverage with no timers and no STA.
- **Complexity:** one new sealed class, ~150-250 lines including docs; no interface changes; no
  changes to `AppItemEngines`.

No existing repo pattern already implements this shape — every existing `getPressed`
(`ToggleDarkMode_GetPressed`, the four QF-settings callbacks) reads an already-synchronous
property. The nearest precedents are the #503 delegate-injected seam types themselves, which is
what B-cache copies.

---

## 4. Research Question C — where the extracted logic lives

**Recommended new type:** `internal sealed class EngineToggleStateCoordinator` —
`TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, namespace `TaskMaster` (matching all four
#503 seam files, which use `namespace TaskMaster` despite the folder). Deliberately NOT
`[ExcludeFromCodeCoverage]`, with the standard remark used by the #503 types
(`EngineReadinessGate.cs:25-28`).

Proposed surface (constructor-injected delegates, no COM types, no `MessageBox`):

```csharp
internal EngineToggleStateCoordinator(
    Func<IAppItemEngines> enginesAccessor,      // returns null before SetGlobals; supported
    Action<string> invalidateControl,           // receives a ribbon control id
    Action<string> notifyUnavailable,           // one message per blocked toggle click
    Action<string, Exception> logError)         // observed prime/toggle faults

internal bool GetPressed(string engineName)          // sync cache read + lazy prime start
internal Task HandleToggleClickAsync(string engineName)  // boundary: guard → toggle → refresh → invalidate; catches and logs (AC7)
internal Task ExecuteToggleAsync(string engineName)  // no catch; propagates — the testable core ordering
```

Plus the engine-key ↔ control-id map (`"Spam"` → `SpamBayesEnabledToggle`, `"Triage"` →
`TriageEnabledToggle`). Keep it as a small `internal static` catalog (either a private map inside
the coordinator or a sibling `EngineToggleCatalog` mirroring `EngineCommandCatalog`) so the XML
tests and the coordinator share one source of truth. It must remain **separate from
`EngineCommandCatalog`**: catalog membership drives the `getEnabled` set-equality and
"must be a button" tests (§2.3), and the toggles must not acquire readiness-gated `getEnabled`
semantics (§1.3).

**Thin glue (all inside existing exempt types, no new exemptions — AC14):**

- `TaskMaster/Ribbon/RibbonController.EngineCommands.cs` (currently 103 lines): a lazy
  `_engineToggleCoordinator` mirroring the `EngineCommands` property (lines 38-45), wired with
  `() => Globals?.Engines!`, `controlId => _viewer?.InvalidateEngineToggle(controlId)`,
  `NotifyEngineCommandNotReady` (reuse, line 97) and `(m, ex) => logger.Error(m, ex)`; plus two
  forwarders `IsEngineToggleActive(string)` / `HandleEngineToggleClickAsync(string)`.
- `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs`: an `internal void
  InvalidateEngineToggle(string controlId)` following the `InvalidateEngineCommands` dispatcher
  marshal (lines 63-81), and the rewritten callbacks (§5).

**Projects:** production type in `TaskMaster` (it depends on `EngineCommandCatalog` siblings and
is ribbon-specific; `UtilitiesCS` placement would strand it away from its only consumers — every
#503 seam sits in `TaskMaster\Ribbon\`). Tests in `TaskMaster.Test\Ribbon\`
(`EngineToggleStateCoordinatorTests.cs`), the placement used by all four #503 seam test files.
File-size check: all touched files stay far below the 500-line ceiling (largest is
`RibbonViewer.EngineCommands.cs` at 207 lines; the rewrite adds roughly 40-60 lines of
docs+glue).

---

## 5. Research Question D — guarding the ten sites

The ten sites split into two semantic groups; one guard shape per group.

### 5.1 The four toggle sites (lines 120, 123, 189, 192) — new coordinator, NOT the readiness gate

Routing these through `RunEngineCommandAsync` would be wrong twice over:

1. **Semantics.** The gate is keyed on `InboxEngines` presence, but `ToggleEngineAsync` /
   `EngineActiveAsync` operate on configuration (§1.3). An engine configured off is absent from
   `InboxEngines` (`AppItemEngines.cs:63-64`), so a readiness-gated toggle could never re-enable
   a disabled engine, and a readiness-gated `getPressed` could never show its true state.
2. **Shape.** `getPressed` is a synchronous read Office polls; `RunEngineCommandAsync` returns a
   `Task` and emits a user-facing "still loading" notification per blocked call —
   notification-per-poll is not acceptable for a read. This is the read/command asymmetry: reads
   get a cached-state answer with a silent `false` default; commands get gated invocation with
   one notification.

Rewritten callbacks (thin glue, mirroring `EngineCommand_GetEnabled` at lines 38-39):

```csharp
public bool SpamBayesEnabled_GetPressed(Office.IRibbonControl control) =>
    _controller?.IsEngineToggleActive(SpamBayes.GroupName) ?? false;

public async void SpamBayesEnabled_Click(Office.IRibbonControl control, bool pressed) =>
    await Controller.HandleEngineToggleClickAsync(SpamBayes.GroupName);
```

(Triage mirrors with `"Triage"`.) This satisfies AC1-AC3 (exact Office signature; real state via
cached `EngineActiveAsync`; no STA blocking), AC5-AC6 (awaited completion), AC7 (coordinator
catches and logs), AC8 (`async void` + `await`, one awaited expression, same shape as the
sibling `*Save*_Click` handlers), AC9 (cache-update-then-invalidate), and AC10/AC12 for these
four sites (null engines → notify/no-op or `false`).

### 5.2 The six command sites (lines 126, 129, 132, 195, 198, 201) — existing `RunEngineCommandAsync` gate

For `ShowDiskDialog`/`ShowSaveInfo` the readiness gate is semantically exact: both methods can
only do useful work when the engine is present in `InboxEngines` (§1.3), which is precisely what
`EngineReadinessGate.IsEngineReady` tests. This follows the maintainer's #518 recommendation.

Required changes:

1. **`EngineCommandCatalog.Map` gains six entries** (`EngineCommandCatalog.cs:36-49`):
   `SpamSaveNetwork`, `SpamSaveLocal`, `GetSaveState` → `"Spam"`; `TriageSaveNetwork`,
   `TriageSaveLocal`, `TriageGetSaveState` → `"Triage"`. All six XML elements are `button`s, so
   the schema-legal test (§2.3 item 3) continues to pass.
2. **`RibbonExplorer.xml` gains `getEnabled="EngineCommand_GetEnabled"` on those six buttons** —
   forced by the existing set-equality tests (§2.3 items 1-2), which derive their expectations
   from `ControlIds` and therefore go red until the XML matches. Intentional UX consequence: the
   six buttons render disabled until their engine is loaded, instead of silently no-oping; and
   `EngineCommandRefreshPlanner.InvalidateAll` (driven by `ControlIds`) automatically re-enables
   them after the post-load refresh at `ThisAddIn.cs:82`. The #503 comment at
   `RibbonExplorerXmlTests.cs:218-222` (save-location commands "remain safe and useful during
   initialization") described the hazard of disabling them via a **containing menu**; with
   per-control gating they are disabled exactly while they would no-op, which is more honest,
   and the plan should update that comment to match.
3. **Callback rewrite** — each becomes the `TrainSpam_Click` shape (lines 88-92), with the
   engine dereference deferred into the lambda so it is never evaluated when the gate is closed
   (gate open implies `Globals?.Engines` non-null, so no null-conditional is needed inside):

   ```csharp
   public async void SpamSaveNetwork_Click(Office.IRibbonControl control) =>
       await Controller.RunEngineCommandAsync(
           "SpamSaveNetwork",
           () => Controller.Engines.ShowDiskDialog(SpamBayes.GroupName, false));
   ```

   The two `void` `ShowSaveInfo` sites wrap as
   `() => { Controller.Engines.ShowSaveInfo(...); return Task.CompletedTask; }` and the handler
   becomes `async void` + `await`, matching AC8's sibling shape. `GetSaveLocation_Click` and
   `TriageGetSaveLocation_Click` keep their method names (pinned by `onAction` in XML) while
   using catalog ids `GetSaveState`/`TriageGetSaveState`.

AC11: `TestSpam_Click` is untouched; the change set guards exactly the 10 enumerated sites and
the count is reported as review evidence (§7, T-map). AC13: `RibbonController.Engines`
(`RibbonController.Intelligence.cs:204`, `internal IAppItemEngines Engines => Globals?.Engines;`)
is not modified.

---

## 6. Research Question E — error handling and logging

- **Logging pattern:** per-type static log4net logger — `RibbonController.cs:47-49`,
  `RibbonViewer.cs:60-62`, `AppItemEngines.cs:29-31`:
  `private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);`
- **Boundary shape for `async void` (AC7):** the in-repo precedent is
  `RibbonViewer.RunFolderFilterCallback` (`RibbonViewer.cs:289-309`): `try { await work(); }
  catch (Exception ex) { report(ex); }` with the report itself guarded and logged via
  `logger.Error`. For this work the boundary lives in the **coordinator's**
  `HandleToggleClickAsync` (host-neutral, unit-testable): it catches, formats a message naming
  the engine key, and invokes the injected `logError` delegate — production wiring
  `logger.Error(message, exception)`. The viewer handler stays a single awaited expression that
  can no longer fault (parity with the #503 gate philosophy: `EngineGatedCommandRunner` still
  never catches, `ExecuteToggleAsync` still propagates; only the click *boundary* observes).
- **Blocked-toggle notification:** reuse `NotifyEngineCommandNotReady`
  (`RibbonController.EngineCommands.cs:97-101`, `logger.Warn` + `MessageBox.Show`) as the
  injected `notifyUnavailable` sink, keeping presentation out of the tested type. The message
  text decision ("engines not available yet") belongs to the coordinator, mirroring
  `EngineGatedCommandRunner.BuildNotReadyMessage` (lines 117-137).
- The six command sites inherit the #503 error philosophy unchanged: gate-open faults propagate
  out of the `async void` handler exactly as they do today for `TrainSpam_Click` et al.

---

## 7. Research Question F — test strategy (red-first)

Framework MSTest, mocking Moq, assertions FluentAssertions; no temp files, no
`Thread.Sleep`/`Task.Delay`, no real waits, no WinForms pump/forms/`BackgroundWorker`. **No STA
test is needed anywhere in this design** — the coordinator is pure delegates+dictionary, and the
COM-touching lines stay in exempt glue. No `*.StaTests.cs` file is required.

**Hard constraint discovered:** viewer/controller-level behavioral tests must not drive a path
that reaches `NotifyEngineCommandNotReady`, because it calls `MessageBox.Show`
(`RibbonController.EngineCommands.cs:100`) and would hang vstest. Behavioral assertions therefore
live at the coordinator/catalog seam with injected sinks (the #503 test approach,
`EngineGatedCommandRunnerTests.cs:29-36`).

### Red-before-fix regression tests (AC15)

| # | Test (file → test) | Red because (today) | Green after | ACs |
|---|---|---|---|---|
| R1 | `RibbonExplorerXmlTests` — for each of `SpamBayesEnabledToggle`/`TriageEnabledToggle`: the XML `getPressed` attribute resolves to a public instance method on `RibbonViewer` returning `bool` with a single parameter whose `ParameterType.FullName == "Microsoft.Office.Core.IRibbonControl"` (copy the §2.3 pattern; also pin `onAction` → `void (IRibbonControl, bool)`) | `SpamBayesEnabled_GetPressed`/`TriageEnabled_GetPressed` return `Task<bool>` | signatures corrected | AC1, AC2, AC4 |
| R2 | `RibbonExplorerXmlTests` (or a viewer test) — reflection-invoke each `*_GetPressed` on `new RibbonViewer(new RibbonController())` (pre-`SetGlobals`); if the result is a `Task`, await it; assert no exception and, once synchronous, result `false`. Reflection invocation keeps the test compiling across the signature change. | faulted task / NRE from `Controller.Engines.EngineActiveAsync` with null `Engines` | returns `false`, no throw, no UI | AC3 (degradation half), AC12 |
| R3 | `EngineCommandCatalogTests` — `TryGetEngineName` maps the six command ids to `"Spam"`/`"Triage"`; `ControlIds` has 14 entries | ids absent from `Map` | catalog extended | AC10 |
| R4 | `RibbonExplorerXmlTests` — existing set-equality test forces `getEnabled` onto the six buttons once R3's catalog change lands (goes red mid-change, green when XML updated in the same task) | XML lacks the attributes | XML updated | AC10 |
| R5 | reflection shape pin — `SpamBayesEnabled_Click`/`TriageEnabled_Click` return `void`, take `(IRibbonControl, bool)` by `FullName`, and carry `AsyncStateMachineAttribute` (pins the awaited `async void` shape); same pin on the six command handlers where the shape changes (`GetSaveLocation_Click`, `TriageGetSaveLocation_Click` gain the attribute) | the two toggle Clicks are plain `void`, the two `ShowSaveInfo` handlers are plain `void` | rewritten handlers | AC5, AC6, AC8 |

R1 is the authoritative deterministic repro for #505 (the silent-binding failure surfaces as a
build-time-red test, which is exactly what AC4 requires).

### New-seam unit tests (written with the coordinator, TDD; new-code target ≥ 90 %)

`TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` — scenario completeness per UT2:

- ctor null-argument contracts (`ArgumentNullException`, matching `EngineGatedCommandRunner.cs:60-65`).
- `GetPressed` unknown/null/whitespace key → `false`, no prime, no invalidate.
- `GetPressed` with null engines accessor result → `false`, nothing started (pre-`SetGlobals`).
- `GetPressed` cache miss with engines available → starts exactly one prime; a second call
  during the in-flight prime (TCS not yet completed) starts no second prime; on completion the
  cache holds the value and `invalidateControl` received the mapped control id
  (`SpamBayesEnabledToggle` for `"Spam"`).
- prime fault → `logError` invoked with the exception; `GetPressed` still returns `false`; no
  unobserved task (expose the prime `Task` internally so the test awaits it deterministically).
- `ExecuteToggleAsync` ordering: recorded sequence is `ToggleEngineAsync` →
  `EngineActiveAsync` → cache visible via `GetPressed` → `invalidateControl` (AC5/AC6/AC9,
  update-before-invalidate invariant).
- `ExecuteToggleAsync` propagates the toggle fault unchanged; `HandleToggleClickAsync` observes
  it, calls `logError`, does not throw, does not invalidate (AC7).
- `HandleToggleClickAsync` with null engines → one `notifyUnavailable` message, no toggle call,
  no throw (AC10/AC12 for the toggle sites).

All async control via `TaskCompletionSource`; Moq `MockBehavior.Strict` where the #503 tests use
it; Arrange-Act-Assert with doc comments per repo style.

### Toolchain (AC16)

`csharpier .` → analyzer msbuild → type-check via CI's command
(`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`,
**not** `/p:Nullable=enable`, per #522) → `vstest.console.exe <assemblies> /EnableCodeCoverage`,
excluding stale `\.claude\worktrees\` builds from the test-assembly glob.

---

## 8. Research Question G — blast radius

- **Callers of the four defect methods:** none in code. A solution-wide grep for the four names
  finds only their declarations in `RibbonViewer.EngineCommands.cs`; they are reached
  exclusively via the XML `onAction`/`getPressed` strings (§2.2), so the signature changes break
  no compile-time caller.
- **`IAppItemEngines` is not modified**, so its single implementer (`AppItemEngines`) and every
  `Mock<IAppItemEngines>` compile unchanged. Specifically
  `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs:92-135` (strict mocks setting up
  `ToggleEngineAsync`/`EngineActiveAsync`/`ShowSaveInfo`/`ShowDiskDialog`/`RestartEngineAsync`)
  is unaffected.
- **#507 tests** (`TaskMaster.Test/Ribbon/RibbonControllerTests.Engines.cs:19-71`) assert
  `RibbonController.Engines` null-return and forwarding; the property is untouched (AC13) —
  unaffected.
- **`RibbonExplorerXmlTests`:** the two catalog-derived assertions (§2.3 items 1-2) constrain
  the catalog+XML change to land atomically; the "must be a button" test stays green because
  only `button` ids are added. The comment at lines 218-222 needs a wording update (§5.2).
- **`EngineCommandRefreshPlanner`/`InvalidateAll`:** now iterates 14 ids. Harmless — invalidation
  is idempotent (`EngineCommandRefreshPlanner.cs:36-41`) and the post-load refresh at
  `ThisAddIn.cs:82` is unchanged. Note it does **not** invalidate the two toggle checkboxes
  (they are deliberately outside `EngineCommandCatalog`); toggle invalidation flows through the
  coordinator's own delegate.
- **`EngineCommandCatalogTests` / `EngineGatedCommandRunnerTests` /
  `EngineCommandRefreshPlannerTests`:** verify per-entry behavior and derive from `ControlIds`;
  audit for any hard-coded count of 8 during planning (`EngineCommandCatalogTests.cs` should be
  read in the plan's Phase 0 and updated if it pins the entry set).
- **UX change (intended, document in PR):** the six save/info buttons become disabled until
  their engine is loaded; previously they were always enabled and silently no-oped.

---

## 9. Rejected alternatives (summary)

- **Blocking the STA in `getPressed`** — intermittent deadlock via captured
  `WindowsFormsSynchronizationContext`, menu-open freeze during config disk load, violates
  async-I/O and determinism policy (§3.1).
- **Synchronous read of the materialized `Configuration`** — `AsyncLazy<T>` has no
  non-triggering probe; any new `IAppItemEngines` member bodies in the coverage-excluded
  `AppItemEngines` (net481, no DIM); couples ribbon to `Globals.AF.Manager` (§3.2).
- **Routing the toggle pairs through `RunEngineCommandAsync` / adding the checkboxes to
  `EngineCommandCatalog`** — readiness (`InboxEngines`) is the wrong predicate for
  config-flipping operations; would permanently block re-enabling a disabled engine; `checkBox`
  ids fail the existing "must be a button" XML test; notification-per-poll is wrong for a read
  (§5.1).
- **Ten ad-hoc `?.` null checks at the call sites** — explicitly disrecommended by the
  maintainer's #518 comment; leaves silent no-ops with no user feedback and no tested decision
  logic.
- **Eagerly priming the cache from `ThisAddIn` post-load** — touches the VSTO entry point,
  misses the menu-opened-before-load window, adds ordering coupling; lazy prime-on-read covers
  both windows with no host change (§3.3). A planner may still add it later as an optimization
  without design change.

## 10. Out of scope — promote to issues (AC17)

Do not fold any of these into the fix. Before filing, check the tracker for existing issues —
items 1-2 were already identified during #503 research and may have been promoted then.

1. **Five orphan `onAction` callbacks in `RibbonExplorer.xml`:** `BtnMigrateIDs_Click`, plus the
   `_Clicked`-vs-`_Click` suffix mismatches `MoveEntireConversation_Clicked` (XML line 265),
   `SaveAttachments_Clicked` (271), `SaveEmailCopy_Clicked` (277), `SavePictures_Clicked` (283)
   — the `RibbonViewer` methods are `*_Click` (`RibbonViewer.cs:180-199`), so all four QF
   settings checkboxes are inert. A generalized "every XML callback resolves with the documented
   signature" test should ship with that fix (it is red on these orphans today).
2. **`RibbonController.Intelligence.cs` unguarded `Globals` dereferences reachable from ribbon
   callbacks pre-`SetGlobals`**, e.g. `ClearSpamManagerAsync` (`Globals.AF...` line 220,
   `Globals.Engines.RestartEngineAsync` line 230) and the QF-settings toggles (lines 29-58).
   Same defect class as #518 but outside the ten enumerated sites.
3. **`spec.md` in this feature folder is an unfilled template** — the planner should populate it
   from `issue.md` before authoring the atomic plan (process gap, not a code issue).
