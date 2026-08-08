# ribbon-engine-readiness-guard (Spec)

- **Issue:** #503
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-08
- **Status:** Delivered
- **Version:** 1.0
- **Work Mode:** full-bug
- **Branch:** `bug/ribbon-engine-readiness-guard-503`
- **Feature folder:** `docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/`

> **Authoritative AC source.** Work mode is `full-bug`. Per `.claude/skills/acceptance-criteria-tracking/SKILL.md`, this file is the **sole** authoritative acceptance-criteria source for issue #503. No `user-story.md` exists for this issue and none is to be created.

> **Authority order for this document.** The research artifact `research/2026-08-08T12-45-ribbon-engine-readiness-guard-research.md` is authoritative over `issue.md` wherever the two conflict. Every such conflict is recorded in the `## Correction Log` at the end of this document.

---

## Context

Engine-backed Explorer-ribbon commands are invokable before `AppItemEngines.InitAsync()` has populated `Globals.Engines.InboxEngines`. During that window the ribbon has no way to observe initialization progress, so a click reaches a dereference of an engine that does not yet exist and throws out of an `async void` handler.

Environment:

- OS/version: Windows 11, Outlook desktop (VSTO add-in host)
- Runtime: .NET Framework 4.8.1 (`TaskMaster.csproj:30` `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>`), TaskMaster VSTO add-in
- Trigger: Outlook Explorer ribbon, an engine-backed button clicked immediately after add-in reload
- Data source or fixture: Live Outlook profile; `Globals.AF.Manager.Configuration` still resolving

Impact / Severity:

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

An unhandled exception on a routine ribbon click immediately after add-in reload. The window is short but reliably reachable, and the failure mode is an unhandled `async void` throw on the message-pump synchronization context rather than a recoverable message.

---

## Repro & Evidence

Steps to reproduce:

1. Reload the TaskMaster add-in (or restart Outlook) so `AppItemEngines.InitAsync()` begins.
2. Before `InitAsync()` completes, click "Train Spam" (or any of the eight commands enumerated in *Verified defect surface* below) in the Explorer ribbon.
3. Observe the unhandled exception.

Expected:

Engine-dependent ribbon commands are not invokable until their backing engine in `InboxEngines` is available. Clicking a not-yet-ready command produces no exception; once `InitAsync()` completes, the commands become enabled and behave exactly as they do today.

Actual:

`RibbonController.SB` (`TaskMaster/Ribbon/RibbonController.Intelligence.cs:190-202`) evaluates
`Globals?.Engines?.InboxEngines?.TryGetValue("Spam", out var engine) ?? false ? engine as SpamBayes : null`
against an empty `ConcurrentDictionary` and returns `null`. `RibbonViewer.TrainSpam_Click` (`TaskMaster/Ribbon/RibbonViewer.cs:255-256`) then executes `await Controller.SB.TrainAsync(Controller.OlSelection, true)`, which throws `NullReferenceException`. Because the handler is `async void`, the exception surfaces on the message-pump synchronization context rather than at the call site.

```text
System.NullReferenceException: Object reference not set to an instance of an object.
   at TaskMaster.RibbonViewer.TrainSpam_Click(IRibbonControl control)
```

### Verified root cause (file + line)

| Fact | Location | Consequence |
|---|---|---|
| `InboxEngines` is initialized to an **empty** `ConcurrentDictionary` at field-initializer time | `TaskMaster\AppGlobals\AppItemEngines.cs:119-123` | The race window exists from add-in load until `InitAsync()` finishes. |
| `InitAsync()` first awaits `Globals.AF.Manager.Configuration` | `TaskMaster\AppGlobals\AppItemEngines.cs:50` | The window is long enough to be reached by a user click. |
| The whole `InboxEngines` property is assigned **once**, at the end, from `ToConcurrentDictionaryAsync(...)`; it is never incrementally filled | `TaskMaster\AppGlobals\AppItemEngines.cs:63-84` | Readiness transitions from "no keys" to "all keys" in a single reference assignment, so a per-key `TryGetValue` probe is precise and race-free. |
| `InitAsync()` filters on `.Where(config => config.Value.Engine)` and drops null factory results | `TaskMaster\AppGlobals\AppItemEngines.cs:64`, `:83` | An engine that is configured off, or whose factory returned null, **never** enters the dictionary. |
| `RestartEngineAsync(string)` re-assigns a single key | `TaskMaster\AppGlobals\AppItemEngines.cs:111-117` | Readiness for that key must be re-evaluated after a restart. |
| No readiness signal is published, and `RibbonExplorer.xml` declares no `getEnabled` callback for the engine-backed buttons | `TaskMaster\Ribbon\RibbonExplorer.xml` | The ribbon cannot reflect initialization progress, so the controls stay clickable. |
| `AppItemEngines` is `[ExcludeFromCodeCoverage]` at type level | `TaskMaster\AppGlobals\AppItemEngines.cs:26-27` | Any readiness logic placed on this class is uncoverable. |
| `RibbonController` and `RibbonViewer` are `[ExcludeFromCodeCoverage]` at type level | `TaskMaster\Ribbon\RibbonController.cs:36`, `TaskMaster\Ribbon\RibbonViewer.cs:31-33` | Any readiness logic placed on these classes is likewise uncoverable. |

### Verified defect surface — exactly 8 handlers

Established by call-graph inspection of every ribbon callback (research §1.4). This is the corrected set; see `## Correction Log` entry 1.

| # | Handler (`TaskMaster\Ribbon\RibbonViewer.cs`) | Dereference path | Engine key | Exception during window | Ribbon control id |
|---|---|---|---|---|---|
| 1 | `TrainSpam_Click` (255-256) | `Controller.SB.TrainAsync(...)` | `Spam` | `NullReferenceException` | `TrainSpam` |
| 2 | `TrainHam_Click` (258-259) | `Controller.SB.TrainAsync(...)` | `Spam` | `NullReferenceException` | `TrainHam` |
| 3 | `TestSpam_Click` (261-264) | `Controller.Engines.InboxEngines[SpamBayes.GroupName].Engine` (**indexer**) | `Spam` | **`KeyNotFoundException`** | `TestSpam` |
| 4 | `TriageSetA_Click` (303-304) | `_controller.Triage.OlLogic.TrainSelectionAsync("A")` | `Triage` | `NullReferenceException` | `TriageSetA` |
| 5 | `TriageSetB_Click` (306-307) | same, `"B"` | `Triage` | `NullReferenceException` | `TriageSetB` |
| 6 | `TriageSetC_Click` (309-310) | same, `"C"` | `Triage` | `NullReferenceException` | `TriageSetC` |
| 7 | `ClearTriage_Click` (316-317) | `_controller.Triage.OlLogic.UnTrainSelectionAsync()` | `Triage` | `NullReferenceException` | `ClearTriage` |
| 8 | `FilterViewer_Click` (325-326) | `_controller.Triage.OlLogic.FilterViewAsync()` | `Triage` | `NullReferenceException` | `FilterTriageGroup` |

Callbacks verified **already race-safe** and therefore explicitly out of scope: `ClearSpam_Click`, `SpamSaveNetwork_Click`, `SpamSaveLocal_Click`, `GetSaveLocation_Click`, `TriageSaveNetwork_Click`, `TriageSaveLocal_Click`, `TriageGetSaveLocation_Click` (all use `TryGetValue` and silently no-op), `SpamBayesEnabled_Click`, `TriageEnabled_Click`, `*_GetPressed` (read `Configuration`, not `InboxEngines`), `TriageSelection_Click`, `SetPrecision_Click`, `ResetTriage_Click` (use `TriageAsync`/a fresh `Triage`), `BuildCategoryClassifier_Click`, `BuildActionableClassifier_Click` (construct fresh classifier groups).

---

## Requirements (canonical numbering)

The GitHub issue body contains no explicitly numbered requirements; the research artifact established the stable numbering below from `issue.md`'s own sections. **All downstream artifacts must use this mapping.** R1–R5 are the five requirements; R6 is the preservation constraint stated alongside R4.

| Ref | Requirement (source text) | `issue.md` line |
|---|---|---|
| **R1** | "There is no published readiness signal on `AppItemEngines`/`IAppItemEngines`" — a readiness signal must be introduced | 66 |
| **R2** | "the Explorer ribbon XML declares no `getEnabled` callback for the engine-backed buttons" — `getEnabled` wiring must be added | 66 |
| **R3** | "Clicking a not-yet-ready command produces no exception" — click-handler guards, with a user-facing "still loading" indication instead of a silent failure | 33, 72 |
| **R4** | "Do not change the async engine construction logic, config loading, or dictionary population order inside `AppItemEngines.InitAsync()`" | 78 |
| **R5a** | "Unit coverage areas: readiness signal …; engine-readiness predicate; click-handler guards; `RibbonExplorer.xml` `getEnabled` wiring validated by the existing ribbon-XML regression suite" | 72 |
| **R5b** | "Integration scenario to retest: click each engine-backed ribbon command immediately after add-in reload" | 73 |
| **R5c** | "Manual verification notes: verify `Ribbon.InvalidateControl(...)`/`Invalidate()` refreshes the enabled state" | 74 |
| **R6** | "Preserve existing `SB`/`TrainAsync` behavior once engines are loaded" | 79 |

### Issue acceptance bullets

| Ref | Bullet (source text) | `issue.md` line |
|---|---|---|
| **A1** | "Clicking a not-yet-ready command produces no exception" | 33 |
| **A2** | "once `InitAsync()` completes, the commands become enabled and behave exactly as they do today" | 33 |
| **A3** | "Integration scenario to retest: click each engine-backed ribbon command immediately after add-in reload; confirm no exception and confirm normal behavior after `InitAsync()` completes" | 73 |
| **A4** | "Manual verification notes: verify `Ribbon.InvalidateControl(...)`/`Invalidate()` refreshes the enabled state once initialization finishes" | 74 |

---

## Scope & Non-Goals

**In scope**

- A per-engine-key readiness contract, computed from the existing `IAppItemEngines.InboxEngines` member, implemented in new host-neutral types.
- `getEnabled` wiring on the eight engine-backed `<button>` elements in `TaskMaster\Ribbon\RibbonExplorer.xml`, plus the single Office-typed callback shim.
- Defense-in-depth guards on the eight handlers listed above, so a click during the window is a no-op with a user-facing "still loading" indication.
- A post-initialization ribbon refresh (`IRibbonUI.InvalidateControl` per control id) marshalled to the UI/STA thread.
- A partial-class split of `RibbonViewer.cs` so no file breaches the 500-line cap.
- Unit tests, coverage evidence, and a manual-verification checklist.

**Out of scope / non-goals**

- **No change to `AppItemEngines.InitAsync()`** — engine construction, config loading, and dictionary population order are untouched (R4). `TaskMaster\AppGlobals\AppItemEngines.cs` takes a **zero-line diff**.
- **No change to `UtilitiesCS\Interfaces\IGlobals\IAppItemEngines.cs`** — zero-line diff. See *Design rationale* for why an interface member is rejected on the merits.
- **No change to `SB`/`Triage`/`TrainAsync` behavior once engines are loaded** (R6). The enabled path executes byte-identical expressions.
- No `IsInitialized` / `InitTask` coarse readiness flag. Rejected on correctness grounds, not convenience — see *Design rationale*.
- No change to `TaskMaster\AppGlobals\ApplicationGlobals.cs`.
- No splitting of `RibbonExplorer.xml` (already 519 lines before this change; the overage is pre-existing and the file is a declarative embedded UI resource, not production/test/script code).
- No fix for the out-of-scope defects catalogued in research §9 (orphan `onAction` callbacks, invalid `getPressed` signatures, fire-and-forget `ToggleEngineAsync`, non-null-safe `RibbonController.Engines`). Each is to be promoted to its own issue.
- No Outlook UI-automation harness. R5b and the visual half of R5c remain manual.

**Explicitly excluded systems, integrations, datasets:** no live Outlook process, no live mail profile, no network, no filesystem, no temporary files in any automated test.

---

## Root Cause Analysis

`InboxEngines` is an empty `ConcurrentDictionary` from field-initializer time until the single terminal assignment at the end of `InitAsync()` (`AppItemEngines.cs:63-84`). `InitAsync()` first awaits `Globals.AF.Manager.Configuration` (line 50) and then asynchronously constructs each engine, so the empty-dictionary window spans the whole configuration + construction period. Because neither `AppItemEngines` nor `IAppItemEngines` publishes a readiness signal, and because `RibbonExplorer.xml` declares no `getEnabled` callback for the engine-backed buttons, the ribbon renders those controls enabled throughout the window. Clicking one reaches an engine dereference — `null` via `RibbonController.SB`/`Triage`, or a missing-key indexer in `TestSpam_Click` — inside an `async void` handler, so the resulting `NullReferenceException` or `KeyNotFoundException` escapes onto the message-pump synchronization context unhandled.

---

## Proposed Fix

### Design summary (what changes where)

Introduce a **per-engine-key readiness signal computed from the existing `IAppItemEngines.InboxEngines` member**, implemented in four new host-neutral `internal sealed` types under `TaskMaster\Ribbon\` that are **deliberately NOT `[ExcludeFromCodeCoverage]`**. Wire them into the ribbon through one new Office-typed `getEnabled` shim on a new `RibbonViewer` partial, add `getEnabled="EngineCommand_GetEnabled"` to the eight engine-backed `<button>` elements, route the eight affected handlers through a gated runner whose lambda defers the engine dereference, and invalidate those eight control ids once from `ThisAddIn` after `LoadAsync` completes on the STA.

This is research option **(c)**. It follows the `HookReadinessCoordinator` / `EngineInitTimingProbe` precedent already established in this repository: a host-neutral, unit-tested decision seam plus a thin, coverage-exempt COM/VSTO glue layer.

### The readiness contract

**Definition.** For an engine key `k`, `k` is *ready* if and only if all of the following hold:

1. the engines accessor returns a non-null `IAppItemEngines`;
2. `InboxEngines` on that instance is non-null;
3. `InboxEngines.TryGetValue(k, out var e)` returns `true`;
4. `e` is not null.

String comparison is the `ConcurrentDictionary` default — **ordinal and case-sensitive** — so `"spam"` is not `"Spam"`. Readiness is recomputed on every query; it is never cached in the gate.

**Why per-key and not global.** A coarse `IsInitialized`/`InitTask` flag is **incorrect on the merits**, not merely inconvenient:

- `InitAsync()` filters on `config.Value.Engine` (`AppItemEngines.cs:64`) and drops null factory results (line 83). An engine that is configured off never enters the dictionary. A global "initialized" flag would report **ready** for a command that will never work, so the button would be enabled and the click would still throw. The coarse flag converts a timing bug into a permanent bug.
- `RestartEngineAsync` (`AppItemEngines.cs:111-117`) re-assigns a single key. A one-shot flag cannot represent a per-key restart; a live per-key probe handles it automatically.
- Setting a flag requires adding a statement inside `InitAsync()`, the method R4 fences off. The per-key probe requires a **zero-line diff** to `AppItemEngines.cs`.

**State model.**

| State | `InboxEngines` contents | `GetEnabled(id)` | Click behavior |
|---|---|---|---|
| S0 — pre-init | empty (field initializer) | `false` | no-op + one "still loading" indication; **no exception** |
| S1 — init in flight | still empty (single terminal assignment) | `false` | as S0 |
| S2 — init complete, engine present | key present, non-null | `true` | **unchanged from today** (R6) |
| S3 — init complete, engine filtered out by config or null factory | key absent | `false` | no-op + indication; **no exception** |
| S4 — engine restarted via `RestartEngineAsync` | key re-assigned | recomputed on next query | unchanged |
| S5 — `InitAsync` threw | empty | `false` | no-op; fail-safe |

**Invariants.**

- The guard suppresses **invocation**, never **errors**. If the engine is ready and the action itself throws, the exception propagates (repo fail-fast rule). The guard must not become a swallow-all.
- Unknown, null, or empty `control.Id` yields `false` — the callback must never disable a non-engine control it does not own.
- The refresh is idempotent; a second refresh (for example after `RestartEngineAsync`) is harmless.

### Design rationale — why not an interface member

`IAppItemEngines` must not gain a member. .NET Framework 4.8.1 has **no default interface member support** (that CLR feature arrived in .NET Core 3.0; Roslyn reports `CS8701 Target runtime doesn't support default interface implementation` regardless of `LangVersion`). Any new interface member could therefore only be bodied inside `AppItemEngines`, which is `[ExcludeFromCodeCoverage]` at type level (`AppItemEngines.cs:26`). The new decision logic would be **entirely uncoverable** — exactly the substitution of a coverage attribute for a real testability seam that was rejected in the issue #227 precedent. Reading `InboxEngines`, which is already on the interface, needs no contract change at all.

Secondary benefit, recorded for the planner: the test-double ripple is **zero**. No `Mock<IAppItemEngines>` site and no hand-rolled `IApplicationGlobals` double requires an edit.

**Trade-off recorded.** The readiness contract is a convention over an existing member rather than a compiler-enforced interface member. Mitigation: the gate is the single chokepoint, it is `internal sealed`, and its semantics are pinned by unit tests.

### Boundaries and invariants to preserve

- `AppItemEngines.InitAsync()` engine construction, config loading, and dictionary population order: untouched (R4).
- `RibbonController.SB` / `RibbonController.Triage` / `TrainAsync` behavior on the ready path: untouched (R6). Handlers keep the identical expression, relocated inside a lambda.
- `[ComVisible(true)]` stays exactly where it is, on the existing `RibbonViewer`. A `partial` split adds no second attribute and no new COM-visible type.
- No file may exceed 500 lines after the change.

### Dependencies or blocked work

None. No new NuGet package, no project reference change, no interface change, no test-double change. `[assembly: InternalsVisibleTo("TaskMaster.Test")]` already exists (`TaskMaster\ThisAddIn.cs:14`, `TaskMaster\Properties\AssemblyInfo.cs:38`), so the new `internal` types are directly testable.

### Implementation strategy (what changes, not sequencing)

#### New host-neutral types — all in `TaskMaster\Ribbon\`, all `internal sealed`, none `[ExcludeFromCodeCoverage]`

Each type contains **zero** `Microsoft.Office.*` and zero `Microsoft.Office.Interop.Outlook` references, and is constructed from plain delegates so it is directly unit-testable with Moq.

1. **`EngineCommandCatalog.cs`** — the single source of truth for the control-id to engine-key binding.
   - `static bool TryGetEngineName(string controlId, out string engineName)` — ordinal lookup; `false` for null, empty, or unknown ids.
   - `static IReadOnlyCollection<string> ControlIds { get; }` — exactly the eight ids from the defect-surface table.
   - Rationale: this binding is shared by the XML, the `getEnabled` callback, the click guards, and the refresh. Centralizing it is what makes a single test able to assert XML/code agreement. The map is built to be extensible so a future `Project`/`Context`/`Actionable` command is a one-line addition.

2. **`EngineReadinessGate.cs`** — the R1 readiness signal.
   - `EngineReadinessGate(Func<IAppItemEngines> enginesAccessor)`; null accessor throws `ArgumentNullException` at construction (constructor-time invariant, per repo policy).
   - `bool IsEngineReady(string engineName)` — implements the readiness contract above.
   - `bool TryGetEngine(string engineName, out IConditionalEngine<MailItemHelper> engine)` — same predicate, returning the instance for callers that need it.

3. **`EngineGatedCommandRunner.cs`** — the R3 click guard and the `getEnabled` decision.
   - `EngineGatedCommandRunner(EngineReadinessGate gate, Action<string> notifyNotReady)`; null arguments throw `ArgumentNullException`.
   - `bool IsCommandEnabled(string controlId)` → `EngineCommandCatalog.TryGetEngineName(controlId, out var name) && gate.IsEngineReady(name)`; `false` for unknown ids. This is the `getEnabled` decision.
   - `Task RunAsync(string controlId, Func<Task> action)` — null `action` throws `ArgumentNullException`; when `!IsCommandEnabled(controlId)`, emit **exactly one** "still loading" notification and return a completed task **without invoking `action`**; otherwise `await action()` and let any exception propagate.
   - The caller passes a **lambda**, so `Controller.SB` / `Controller.Triage` are dereferenced only *inside* the lambda and are never evaluated when the gate is closed. This is what converts the `NullReferenceException`/`KeyNotFoundException` into a no-op without scattering `?.` through `RibbonViewer`.

4. **`EngineCommandRefreshPlanner.cs`** — the R5c refresh decision.
   - `static void InvalidateAll(Action<string> invalidateControl)` — null delegate throws `ArgumentNullException`; invokes the delegate once per `EngineCommandCatalog.ControlIds` entry.
   - Keeps "which controls to invalidate" coverable while `IRibbonUI.InvalidateControl` stays inside the exempt shim.

#### Thin, uncovered shims (all inside types that are already `[ExcludeFromCodeCoverage]`)

5. **`TaskMaster\Ribbon\RibbonController.EngineCommands.cs`** — new partial of the existing `[ExcludeFromCodeCoverage] partial class RibbonController`.
   - Lazily builds `new EngineReadinessGate(() => Globals?.Engines)`. The `?.` is what makes the pre-`SetGlobals` case safe; note the existing `RibbonController.Engines` (`RibbonController.Intelligence.cs:204`) is **not** null-safe on `Globals` and must not be used as the accessor.
   - Exposes `internal bool IsEngineCommandEnabled(string controlId)`, `internal Task RunEngineCommandAsync(string controlId, Func<Task> action)`, and `internal void RefreshEngineCommands()` forwarding to `_viewer`.
   - Must **not** route through `RibbonController.SB`/`Triage` for the readiness decision: those getters side-effect a `WindowsFormsSynchronizationContext` onto the calling thread.

6. **`TaskMaster\Ribbon\RibbonViewer.EngineCommands.cs`** — new partial; requires changing `public class RibbonViewer` to `public partial class RibbonViewer` at `RibbonViewer.cs:33`.
   - `public bool EngineCommand_GetEnabled(Office.IRibbonControl control) => _controller?.IsEngineCommandEnabled(control?.Id) ?? false;` — the **only** new Office-typed surface introduced by this fix.
   - `internal void InvalidateEngineCommands()` — no-op when `_ribbon` is null; marshals to `UtilitiesCS.Threading.UiThread.Dispatcher` when the current thread is not the dispatcher thread; then calls `EngineCommandRefreshPlanner.InvalidateAll(_ribbon.InvalidateControl)`.
   - Hosts the relocated `#region Spam Manager` and `#region Triage` callbacks, rewritten in the shape
     `public async void TrainSpam_Click(Office.IRibbonControl control) => await Controller.RunEngineCommandAsync("TrainSpam", () => Controller.SB.TrainAsync(Controller.OlSelection, true));`

7. **`TaskMaster\Ribbon\RibbonViewer.cs`** — `class` becomes `partial class` (line 33); the `#region Spam Manager` and `#region Triage` blocks (lines 250-347) move into file 6. This is required, not cosmetic: the file is **487 lines against a 500-line cap**, so the new callbacks cannot be added in place. After the move it lands at roughly 389 lines.

8. **`TaskMaster\Ribbon\RibbonExplorer.xml`** — add `getEnabled="EngineCommand_GetEnabled"` to the eight `<button>` elements listed in *Ribbon wiring* below.

9. **`TaskMaster\ThisAddIn.cs`** — one refresh statement plus a why-comment after `await _globals.LoadAsync(false);` (line 76).

10. **`TaskMaster\TaskMaster.csproj`** and **`TaskMaster.Test\TaskMaster.Test.csproj`** — `<Compile Include>` entries for the new files (legacy non-SDK projects require explicit includes).

#### Ribbon wiring

The eight `<button>` elements in `TaskMaster\Ribbon\RibbonExplorer.xml` that receive `getEnabled="EngineCommand_GetEnabled"`:

| Control id | Element | Line | Parent |
|---|---|---|---|
| `TrainSpam` | `button` | 99-104 | `group id="SpamBayesGroup"` |
| `TrainHam` | `button` | 105-110 | `group id="SpamBayesGroup"` |
| `TestSpam` | `button` | 150-155 | `menu id="OtherSpamActions"` |
| `TriageSetA` | `button` | 445 | `group id="TriageGroup"` |
| `TriageSetB` | `button` | 446 | `group id="TriageGroup"` |
| `TriageSetC` | `button` | 447 | `group id="TriageGroup"` |
| `FilterTriageGroup` | `button` | 449-454 | `menu id="OtherTriageActions"` |
| `ClearTriage` | `button` | 455-460 | `menu id="OtherTriageActions"` |

Constraints on the wiring:

- **Do not** put `getEnabled` on `menu id="OtherSpamActions"` or `menu id="OtherTriageActions"`. Those menus also contain save-location, folder-settings, and enable-toggle commands that are safe and useful during initialization; disabling the container would over-restrict the UI.
- **Do not** attempt `group`- or `tab`-level disabling. The CustomUI schema exposes `getVisible` but **not** `getEnabled` on `group` and `tab`.
- The root element uses the 2009 (`customUI14`) namespace (`RibbonExplorer.xml:2`); XML assertions must be namespace-aware.
- CSharpier is file-based and formats `*.cs` only. The XML must be hand-edited to match the surrounding one-attribute-per-line style.

**Required VSTO callback signature.** `bool GetEnabled(Office.IRibbonControl control)` — a `public` instance method whose name matches the XML attribute value exactly. VSTO silently ignores signature mismatches: a wrong signature compiles and nothing happens when the user clicks or when Office queries the control. One callback may serve many controls, dispatching on `control.Id`; that is the documented pattern and is what `EngineCommand_GetEnabled` does.

**Refresh after initialization is load-bearing.** Office caches each callback's response per control until the add-in invalidates it. Without an explicit invalidation the eight buttons remain disabled for the entire session even after `InitAsync()` succeeds. `IRibbonUI.InvalidateControl(controlId)` invalidates one control; `IRibbonUI.Invalidate()` invalidates every control's cached values. This fix uses `InvalidateControl` once per catalog id (via `EngineCommandRefreshPlanner.InvalidateAll`) so it never disturbs unrelated controls' cached state; `Invalidate()` is the documented fallback if a control id proves unreachable.

**UI/STA marshalling requirement.** `IRibbonUI` is an Office COM object handed to `Ribbon_Load` on the STA and must be called back on the STA. `InitAsync()` is launched via `Task.Run` (`ApplicationGlobals.cs:417`) and therefore *completes* on a thread-pool thread; only an awaiting continuation is marshalled back, and only when a synchronization context was captured. In the live startup path (`ThisAddIn.Application_Startup` enqueues `LoadAsync(false)` with `useUiThread: true`, and `IdleAsyncQueue` runs it through `UiThread.Dispatcher`) the continuation does resume on the STA — but `LoadWhenIdle()` enqueues with `useUiThread: false` and `LoadParallelAsync()` inherits its caller's context. The refresh must therefore be **explicitly marshalled through `UtilitiesCS.Threading.UiThread.Dispatcher`** inside `InvalidateEngineCommands()`, not left to the ambient context.

#### Defense-in-depth: click-handler guards and the "still loading" indication (R3)

`getEnabled` alone is not sufficient. Office's callback caching means a control can be visually enabled while the underlying engine is absent (for example between an engine restart and the next invalidation), and a signature mismatch would silently disable the wiring entirely. Each of the eight handlers is therefore also routed through `EngineGatedCommandRunner.RunAsync`, which re-checks readiness at click time and defers the engine dereference into the lambda.

When the gate is closed, the runner emits exactly one notification through its injected `Action<string>` sink, carrying the control id and the engine key. The sink is the seam: the **decision** to notify and the **content** of the notification are host-neutral and unit-tested, while the **presentation** (status text / non-modal indication that the engine is still loading) lives in the coverage-exempt `RibbonViewer` shim. Automated tests assert on the injected sink and never construct a `Form` or `MessageBox`.

#### Error handling and logging updates

- Skipped clicks produce a single structured notification line; they never throw.
- Errors thrown by the action on the ready path propagate unchanged. No new `catch (Exception)` is introduced.
- Constructor preconditions throw `ArgumentNullException`. No `Debug.Assert` is used for user-facing validation.

#### Rollback / feature-flag considerations

None required. The change is additive plus a mechanical partial-class split. Rollback is a straight revert; `AppItemEngines.cs` and `IAppItemEngines.cs` are not touched, so no data or contract migration is involved.

### Technical specifications (interfaces/contracts)

#### Inputs / outputs and formats

| Member | Input | Output | Failure mode |
|---|---|---|---|
| `EngineCommandCatalog.TryGetEngineName` | `string controlId` (ordinal) | `bool` + `out string engineName` | `false` + `null` for null/empty/unknown |
| `EngineCommandCatalog.ControlIds` | — | `IReadOnlyCollection<string>` of exactly 8 ids, no duplicates | — |
| `EngineReadinessGate.IsEngineReady` | `string engineName` (ordinal, case-sensitive) | `bool` | `false` for null/whitespace, null accessor result, null `InboxEngines`, missing key, null value |
| `EngineReadinessGate.TryGetEngine` | `string engineName` | `bool` + `out IConditionalEngine<MailItemHelper>` | `false` + `null` |
| `EngineGatedCommandRunner.IsCommandEnabled` | `string controlId` | `bool` | `false` for unknown id |
| `EngineGatedCommandRunner.RunAsync` | `string controlId`, `Func<Task> action` | `Task` | `ArgumentNullException` on null action; action exceptions propagate |
| `EngineCommandRefreshPlanner.InvalidateAll` | `Action<string> invalidateControl` | `void` | `ArgumentNullException` on null delegate |
| `RibbonViewer.EngineCommand_GetEnabled` | `Office.IRibbonControl control` | `bool` | `false` when `_controller` or `control` is null |

#### Required configuration keys and defaults

None. No new configuration key, no new default. Existing engine configuration (`config.Value.Engine`) is read only indirectly, through the presence or absence of a dictionary key.

#### Backward-compatibility expectations

- `IAppItemEngines` is unchanged, so every implementer and test double compiles unmodified.
- Every existing ribbon control keeps its current behavior; only the eight engine-backed buttons gain a `getEnabled` attribute.
- The ready-path behavior of all eight handlers is byte-identical to today's.

#### Performance constraints

`IsEngineReady` is one `ConcurrentDictionary.TryGetValue` plus null checks — O(1), lock-free, allocation-free. `getEnabled` is queried by Office on ribbon paint and after invalidation, at human-interaction frequency. No measurable latency, throughput, or memory impact is expected, and no performance budget is imposed.

### Architecture-boundaries compliance

Assessed against `.claude/rules/architecture-boundaries.md`. No enforcement stage exists in this repository today (there is no `*.ArchitectureTests` project and no `quality-tiers.yml` at repo root), and the rule text itself qualifies the .NET assertions with "when the backend exists". The work is nevertheless compliant in substance:

- **Rule 3 (`[ComVisible(true)]` banned in new production code):** no new COM-visible type. The attribute stays on the existing `RibbonViewer`; a `partial` split does not add a second attribute.
- **Rules 1, 2, 4 (VSTO / Outlook Interop / desktop ribbon callbacks):** the only new Office-typed member is exactly one method, `public bool EngineCommand_GetEnabled(Office.IRibbonControl)`, inside the pre-existing COM-visible, coverage-exempt `RibbonViewer` shim. All four new decision types carry **zero** `Microsoft.Office.*` and `Microsoft.Office.Interop.Outlook` using directives.
- **Rule 8 (behavior in host-neutral modules):** all readiness, catalog, guard, and refresh-planning logic is host-neutral and portable unchanged if the desktop ribbon is later replaced by an Office.js command surface. Only the shim would be rewritten.

---

## Assumptions, Constraints, Dependencies

**Assumptions**

- `ConcurrentDictionary.TryGetValue` is thread-safe and `InitAsync` performs a single whole-dictionary reference assignment, so a probe never observes a partially populated map.
- `InternalsVisibleTo("TaskMaster.Test")` remains in place.
- `UtilitiesCS.Threading.UiThread.Dispatcher` is available and represents the STA on which `Ribbon_Load` received the `IRibbonUI`.

**Constraints**

- .NET Framework 4.8.1 — no default interface members.
- 500-line cap for every production, test, and reusable script file. `RibbonViewer.cs` is at 487/500 and `RibbonController.Intelligence.cs` at 412/500 before the change.
- `TaskMaster.csproj` declares no `<Nullable>` element, but the type-check gate runs `/p:Nullable=enable /p:TreatWarningsAsErrors=true`. New files must be nullable-clean under that global override. Use guard clauses and `?? throw new ArgumentNullException(...)` in the `HookReadinessCoordinator` style rather than a scoped `#nullable enable annotations` pragma.
- `Thread.Sleep` and `Task.Delay` are banned (`BannedSymbols.txt`); no wall-clock reads, no temporary files, no message pump, no live COM in tests.
- `RibbonExplorer.xml` is already 519 lines before this change. The overage is pre-existing and is recorded, not remediated here.

**External dependencies**

MSTest, Moq, FluentAssertions only — all already referenced. No new package.

---

## Data / API / Config Impact

- **User-facing changes:** the eight engine-backed ribbon buttons appear disabled until their backing engine is loaded, and become enabled once initialization completes. A click that still reaches a closed gate produces a "still loading" indication instead of an unhandled exception.
- **API changes:** none to any public cross-project contract. Four new `internal sealed` types and one new `public` COM callback method on the existing `RibbonViewer`.
- **Data or migration:** none.
- **Logging/telemetry:** one structured skip notification per blocked click, carrying the control id and the engine key. No existing log statement is changed or removed.
- **Compatibility notes:** no CLI flag, config schema, or version change.

---

## Test Strategy

Framework: **MSTest** (`[TestClass]`/`[TestMethod]`) + **Moq** + **FluentAssertions**, Arrange–Act–Assert. Every automated test is deterministic and uses **no** temporary files, **no** `Thread.Sleep`/`Task.Delay`, **no** wall-clock reads, **no** real WinForms message pump, **no** `Form` or `MessageBox`, and **no** live COM or Outlook process. `IConditionalEngine<MailItemHelper>` is mockable via `new Mock<IConditionalEngine<MailItemHelper>>().Object`.

Tests must not reach the readiness decision through `RibbonController.SB`/`Triage`/`TriageAsync`: those getters install a real `WindowsFormsSynchronizationContext` on the calling thread as a side effect.

### `TaskMaster.Test\Ribbon\EngineReadinessGateTests.cs` (R1, R5a)

Covers states S0–S5: accessor returns null; `InboxEngines` empty (**the #503 repro window**); key present with non-null engine; key present with null value; null/whitespace engine name; ordinal case sensitivity (`"spam"` vs `"Spam"`); dictionary mutated between two calls on the same instance (models the S1→S2 transition and `RestartEngineAsync` with no timing dependency); `TryGetEngine` ready/not-ready; constructor null-accessor `ArgumentNullException`.

### `TaskMaster.Test\Ribbon\EngineCommandCatalogTests.cs` (R2, R5a)

Data-driven mapping for all eight control ids to their engine keys; unknown id; null id; `ControlIds` equals exactly the eight ids; `ControlIds` contains no duplicates.

### `TaskMaster.Test\Ribbon\EngineGatedCommandRunnerTests.cs` (R3, R6, R5a)

`RunAsync` when not ready does not invoke the action and does not throw (**the primary #503 regression test**); emits exactly one notification containing the control id and engine key; when ready invokes the action exactly once (R6); awaits the action to completion using a `TaskCompletionSource` completed synchronously by the test; propagates an exception thrown by a ready action (fail-fast, not swallow-all); unknown control id does not invoke the action; null action throws `ArgumentNullException`; `IsCommandEnabled` returns `false` when not ready, `true` when ready, `false` for an unknown id.

### `TaskMaster.Test\Ribbon\EngineCommandRefreshPlannerTests.cs` (R5c, R5a)

`InvalidateAll` invokes the delegate once per catalog id, asserted as **set** equality against `EngineCommandCatalog.ControlIds` — never as an ordered sequence, because Office documents callback ordering as unspecified; null delegate throws `ArgumentNullException`.

### `TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs` additions (R2, R5a)

Parses the embedded resource `TaskMaster.Ribbon.RibbonExplorer.xml` via `GetManifestResourceStream` with `XDocument` (existing pattern; no Outlook, no filesystem). New assertions: every catalog control id exists in the XML and carries `getEnabled="EngineCommand_GetEnabled"`; no other element carries that value (guards against over-disabling); every catalog id resolves to a schema-legal element type for `getEnabled` (`button`), never `group`/`tab`; and a reflection assertion that `typeof(RibbonViewer)` exposes a `public` instance method named `EngineCommand_GetEnabled` returning `bool` with exactly one parameter of type `Microsoft.Office.Core.IRibbonControl` — the guard against the documented "wrong signature compiles but silently does nothing" failure mode.

### Deliberately not unit-tested, and why

`RibbonViewer.EngineCommand_GetEnabled`, `RibbonViewer.InvalidateEngineCommands`, the `RibbonController` engine-command shims, and the `ThisAddIn` refresh call all live in `[ExcludeFromCodeCoverage]` types and consist of null checks plus one delegating call each. This is the ratified COM/VSTO/WinForms exemption in CLAUDE.md § UT2 applied to **thin wiring only**; every decision is covered. This is the distinction required by the issue #227 precedent.

### Coverage targets and evidence

- The four new types must reach **>= 90% line coverage** (CLAUDE.md § UT2 new-module floor, the stricter of the applicable thresholds). This is achievable because they contain no host-bound code.
- No coverage regression on changed lines.
- Baseline coverage XML must be captured at the merge-base under `docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/baseline/` **before** implementation; final coverage XML under `.../evidence/qa-gates/`; the comparison recorded alongside. No baseline exists at spec authoring time, so the repo-wide figure is a record-and-report obligation measured against that captured baseline, not an absolute pass/fail floor imposed by this change (see AC24).
- Manual-verification outcomes recorded under `.../evidence/manual-verification/`.

### Toolchain commands (format → lint → type-check → test)

1. `dotnet tool run csharpier .`
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

Restart from step 1 if any step fails or changes files.

### Manual validation (cannot be automated in this repository)

There is no Outlook UI-automation harness in this repository, and the general unit-test policy prohibits tests that depend on external processes. The following are executed by the maintainer against a live Outlook profile and recorded under `.../evidence/manual-verification/`:

1. Reload the add-in and, during initialization, click each of the eight engine-backed commands. Confirm no `NullReferenceException` and no `KeyNotFoundException` in the log, and that a "still loading" indication appears.
2. Confirm Office visually greys the eight buttons during initialization.
3. After initialization completes, confirm the eight buttons become enabled without an add-in restart (this is what proves the `InvalidateControl` refresh fired and that the callback is actually bound — VSTO silently ignores signature mismatches, so only a live load proves binding).
4. Confirm each of the eight commands behaves exactly as before once enabled.

---

## Acceptance Criteria

Traceability tags: `R1`-`R6` are the requirements table above; `A1`-`A4` are the issue acceptance bullets. Criteria marked **MANUAL-ONLY** must **never** be checked off on the strength of unit tests; they require recorded live-Outlook verification.

- [x] **AC1 (R1)** `TaskMaster\Ribbon\EngineReadinessGate.cs` exists, is `internal sealed`, carries no `[ExcludeFromCodeCoverage]` attribute, contains zero `Microsoft.Office.*` using directives, and implements the per-key readiness contract (non-null accessor result, non-null `InboxEngines`, key present, value non-null). Verified by `TaskMaster.Test\Ribbon\EngineReadinessGateTests.cs`.
- [x] **AC2 (R1)** `EngineReadinessGate.IsEngineReady` returns `false` for a null accessor result, a null `InboxEngines`, an empty dictionary (the #503 window), a missing key, a null dictionary value, and a null/whitespace engine name; and is ordinal case-sensitive (`"spam"` is not `"Spam"`). Verified by named tests in `EngineReadinessGateTests`.
- [x] **AC3 (R1)** Readiness is recomputed on every query, so a key added or replaced after the first query (models the S1→S2 transition and `RestartEngineAsync`) flips the result without any restart or timing dependency. Verified by a test that mutates the same `ConcurrentDictionary` between two calls, with no `Thread.Sleep`/`Task.Delay`.
- [x] **AC4 (R1)** `EngineReadinessGate` throws `ArgumentNullException` from its constructor when the engines accessor is null. Verified by `Constructor_WithNullAccessor_ThrowsArgumentNullException`.
- [x] **AC5 (R2)** All eight engine-backed `<button>` elements in `TaskMaster\Ribbon\RibbonExplorer.xml` (`TrainSpam`, `TrainHam`, `TestSpam`, `TriageSetA`, `TriageSetB`, `TriageSetC`, `FilterTriageGroup`, `ClearTriage`) declare `getEnabled="EngineCommand_GetEnabled"`. Verified by a namespace-aware `XDocument` assertion over the embedded resource in `TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs`.
- [x] **AC6 (R2)** No element other than those eight declares `getEnabled="EngineCommand_GetEnabled"`, so the fix cannot silently over-disable the UI; in particular `menu id="OtherSpamActions"`, `menu id="OtherTriageActions"`, and every `group`/`tab` element are unmodified. Verified by a negative assertion in `RibbonExplorerXmlTests`.
- [x] **AC7 (R2)** Every catalog control id resolves in the XML to an element type for which the CustomUI schema permits `getEnabled` (`button`), never `group` or `tab`. Verified by a schema-legality assertion in `RibbonExplorerXmlTests`.
- [x] **AC8 (R2)** `RibbonViewer` exposes a `public` instance method `EngineCommand_GetEnabled` returning `bool` with exactly one parameter of type `Microsoft.Office.Core.IRibbonControl`, matching the required VSTO callback signature `bool GetEnabled(Office.IRibbonControl control)`. Verified by a reflection assertion in `RibbonExplorerXmlTests`.
- [x] **AC9 (R2)** `EngineCommandCatalog` maps exactly the eight control ids to their engine keys (`Spam` x3, `Triage` x5), returns `false` for null/empty/unknown ids, and exposes a duplicate-free `ControlIds` collection of exactly those eight ids. Verified by `TaskMaster.Test\Ribbon\EngineCommandCatalogTests.cs`.
- [x] **AC10 (R3, A1)** All eight affected handlers (`TrainSpam_Click`, `TrainHam_Click`, `TestSpam_Click`, `TriageSetA_Click`, `TriageSetB_Click`, `TriageSetC_Click`, `ClearTriage_Click`, `FilterViewer_Click`) invoke their engine dereference only inside a lambda passed to `EngineGatedCommandRunner.RunAsync`, so no engine is dereferenced when the gate is closed. Verified by source inspection plus `RunAsync_WhenEngineNotReady_DoesNotInvokeAction` in `TaskMaster.Test\Ribbon\EngineGatedCommandRunnerTests.cs`.
- [x] **AC11 (R3, A1)** `EngineGatedCommandRunner.RunAsync` completes without throwing when the engine is not ready, covering both reported exception types: `NullReferenceException` (the `Controller.SB`/`Controller.Triage` path) and `KeyNotFoundException` (the `TestSpam_Click` dictionary-indexer path). This is the primary regression test for #503.
- [x] **AC12 (R3)** A blocked click emits exactly one user-facing "still loading" notification through the injected `Action<string>` sink, containing the control id and the engine key. Verified by `EngineGatedCommandRunnerTests` asserting on the injected sink, with no `Form`, `MessageBox`, or message pump constructed in the test.
- [x] **AC13 (R3)** `RunAsync` throws `ArgumentNullException` for a null action, and does not invoke the action for an unknown control id. Verified by named tests.
- [x] **AC14 (R3)** The guard suppresses invocation but never errors: when the engine is ready and the action throws, the exception propagates unchanged. Verified by `RunAsync_WhenActionThrows_PropagatesException`. No new `catch (Exception)` is introduced anywhere in the change.
- [x] **AC15 (R4)** `TaskMaster\AppGlobals\AppItemEngines.cs` and `UtilitiesCS\Interfaces\IGlobals\IAppItemEngines.cs` each show a **zero-line diff** against the merge-base. Verified by `git diff --numstat <merge-base>..HEAD` reporting neither path. No `IsInitialized`/`InitTask` flag and no new `IAppItemEngines` member is introduced.
- [x] **AC16 (R6, A2)** On the ready path the eight handlers execute expressions identical to today's (for example `Controller.SB.TrainAsync(Controller.OlSelection, true)`), and `RunAsync` invokes the supplied action exactly once and awaits it to completion. Verified by `RunAsync_WhenEngineReady_InvokesActionExactlyOnce` and `RunAsync_WhenEngineReady_AwaitsActionToCompletion` (driven by a synchronously completed `TaskCompletionSource`), plus source diff review of the eight handlers.
- [x] **AC17 (R5c, A4)** `EngineCommandRefreshPlanner.InvalidateAll` invokes the supplied invalidation delegate once for each of the eight catalog control ids, asserted as **set** equality (not sequence order, because Office documents callback ordering as unspecified), and throws `ArgumentNullException` for a null delegate. Verified by `TaskMaster.Test\Ribbon\EngineCommandRefreshPlannerTests.cs`.
- [x] **AC18 (R5c, A4)** `RibbonViewer.InvalidateEngineCommands()` no-ops when `_ribbon` is null and explicitly marshals the `IRibbonUI` call to `UtilitiesCS.Threading.UiThread.Dispatcher` rather than relying on the ambient synchronization context; `ThisAddIn` invokes the refresh exactly once after `await _globals.LoadAsync(false)`. Verified by source inspection of the shim and the call site (both are in `[ExcludeFromCodeCoverage]` types by design).
- [ ] **AC19 (R5b, A3) — MANUAL-ONLY.** In a live Outlook Explorer, immediately after add-in reload and before initialization completes, each of the eight engine-backed ribbon commands is clicked and produces no `NullReferenceException` and no `KeyNotFoundException` in the log, and shows the "still loading" indication. Requires a running Outlook process and a live mail profile; **must not be checked off on the strength of unit tests.** Outcome recorded under `docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/manual-verification/`.
- [ ] **AC20 (R5b, A2, A3) — MANUAL-ONLY.** In the same live session, after `InitAsync()` completes, each of the eight commands behaves exactly as it did before this change. Requires live Outlook; outcome recorded under `.../evidence/manual-verification/`.
- [ ] **AC21 (R5c, A4) — MANUAL-ONLY.** In a live Outlook session, Office visually greys the eight buttons during initialization and re-enables them after the post-`InitAsync()` invalidation fires, without an add-in restart. This also confirms the `getEnabled` callback is actually bound, which VSTO does not report on a signature mismatch. Office's callback-caching behavior is internal to the host and is not locally observable; **must not be checked off on the strength of unit tests.** Outcome recorded under `.../evidence/manual-verification/`.
- [x] **AC22 (R5a)** `dotnet tool run csharpier .` reports no formatting changes; `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` completes with zero errors and no new analyzer diagnostics; `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` completes with zero errors; `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` reports zero failed and zero skipped tests. All four steps pass in a single uninterrupted pass, and the artifacts are recorded under `.../evidence/qa-gates/`.
- [x] **AC23 (R5a)** Each of the four new types (`EngineCommandCatalog`, `EngineReadinessGate`, `EngineGatedCommandRunner`, `EngineCommandRefreshPlanner`) reaches **>= 90% line coverage** in the final coverage XML under `.../evidence/qa-gates/`.
- [x] **AC24 (R5a)** A merge-base coverage baseline is captured under `.../evidence/baseline/` before implementation, and the post-change coverage figure is compared against it. The comparison shows no regression on changed lines and no reduction in the repository line-coverage figure on the testable denominator defined in CLAUDE.md § UT2. The absolute repo-wide figure is recorded and reported in the comparison artifact; it is not restated here as an independent numeric floor, because no baseline existed at spec authoring time.
- [x] **AC25** Every file touched by this change is at or under **500 lines** after the change, verified by a line count of each path in the branch diff. This specifically requires the `RibbonViewer.cs` partial-class split (487/500 before the change, so the new callbacks cannot be added in place) with the `#region Spam Manager` and `#region Triage` blocks relocated into `RibbonViewer.EngineCommands.cs`. `RibbonExplorer.xml` (519 lines, pre-existing overage, declarative embedded UI resource) is recorded as an accepted pre-existing exception and is not remediated here.
- [x] **AC26** No `[ExcludeFromCodeCoverage]` attribute is added to any new type carrying readiness decision logic. Verified by grepping the four new files for the attribute and finding zero occurrences.
- [x] **AC27** The only new `Microsoft.Office.*`-typed member introduced by this change is `public bool EngineCommand_GetEnabled(Office.IRibbonControl control)` on the pre-existing `[ComVisible(true)] [ExcludeFromCodeCoverage] RibbonViewer`. No new `[ComVisible(true)]` type is added, and the four new decision types contain zero `Microsoft.Office.*` and zero `Microsoft.Office.Interop.Outlook` using directives (`.claude/rules/architecture-boundaries.md` rules 1-4 and 8). Verified by grep over the new and changed files.
- [x] **AC28** No automated test in this change creates a temporary file, calls `Thread.Sleep` or `Task.Delay`, reads the wall clock, constructs a `Form` or `MessageBox`, starts a WinForms message pump, or touches live COM/Outlook. Verified by grep over the new and changed test files plus the `BannedSymbols.txt` analyzer result in the AC22 build.
- [x] **AC29** The out-of-scope defects catalogued in research §9 (orphan `onAction` callbacks, invalid `getPressed` signatures, fire-and-forget `ToggleEngineAsync`, non-null-safe `RibbonController.Engines`) are each promoted to a separate GitHub issue through the promotion lifecycle, and none is fixed inside #503. Verified by the recorded promotion receipts.
- [x] **AC30** This spec, `issue.md`, and the plan reflect the delivered outcome, including any deviation from the design recorded here, and the manual-verification checklist for AC19-AC21 is present in the feature folder with its outcomes recorded.

---

## Risks & Mitigations

| Risk | Impact | Mitigation |
|---|---|---|
| VSTO silently ignores a `getEnabled` signature mismatch — the wiring compiles and does nothing | Buttons never disable; the bug persists undetected | AC8 reflection test pins the exact signature; AC21 live check proves binding; the AC10 click guard makes the fix correct even if the callback never binds |
| Office caches `getEnabled` responses per control until invalidated | Buttons stay disabled for the whole session after initialization succeeds | AC17 covers the invalidation set; AC18 pins the single refresh call site; AC21 confirms it live |
| `IRibbonUI` called off the STA | COM failure or silent no-op at runtime | AC18 requires explicit marshalling through `UiThread.Dispatcher` rather than relying on the ambient context |
| `RibbonViewer.cs` breaches the 500-line cap | Blocking policy violation | AC25 requires the partial-class split with region relocation, which nets the file down to roughly 389 lines |
| The guard degenerates into a swallow-all and hides real engine errors | Silent failures worse than the original bug | AC14 requires exception propagation on the ready path and forbids new broad catches |
| Readiness treated as global rather than per-key | An engine configured off would report ready and still throw | The readiness contract is per-key by construction; AC2 pins the missing-key and null-value cases |
| Over-disabling by putting `getEnabled` on menus or groups | Safe commands (save location, folder settings, enable toggles) become unusable during initialization | AC6 asserts no element other than the eight carries the callback; AC7 asserts schema legality |
| Scope creep into the research §9 defects | Larger, riskier diff; the bugfix workflow requires a minimal targeted fix | AC29 requires separate issue promotion instead of in-scope fixes |

---

## Rollout & Follow-up

- **Release/rollout:** ships with the next add-in build. No configuration change, no migration, no feature flag. Rollback is a straight revert.
- **Post-fix verification:** execute the AC19-AC21 manual checklist against a live Outlook profile and record the outcome under `.../evidence/manual-verification/` before merge.
- **Follow-up work:** promote each research §9 defect to its own issue (AC29). Optionally evaluate whether the existing `TaskMaster.Test\AppGlobals\LiveOutlookHarnessRunner.cs` harness can be extended to drive ribbon callbacks; this was not assessed in depth, would still require a live Outlook process, and is not a recommendation for this fix.
- **Links:** issue <https://github.com/drmoisan/TaskMaster/issues/503>; research `research/2026-08-08T12-45-ribbon-engine-readiness-guard-research.md`; branch `bug/ribbon-engine-readiness-guard-503`.

---

## Correction Log

### 2026-08-08 — corrections applied to `issue.md` on the authority of the research artifact

1. **Affected handler set narrowed to a verified 8.** `issue.md:16` states: *"The same initialization race affects every ribbon command backed by an engine in `InboxEngines` (Triage, Project, Context, Actionable)."* This is **false** for `Project`, `Context`, and `Actionable`. Call-graph inspection of every ribbon callback found that **no ribbon callback dereferences those engines**: `BuildCategoryClassifier_Click` (`RibbonViewer.cs:242-243`) and `BuildActionableClassifier_Click` (`RibbonViewer.cs:245-246`) construct fresh `CategoryClassifierGroup`/`ActionableClassifierGroup` instances via `RibbonController.BuildCategoryClassifierAsync`/`BuildActionableClassifierAsync` (`RibbonController.Intelligence.cs:119-137`) and never read `InboxEngines`. The verified defect surface is exactly the eight handlers in *Verified defect surface* above: `Spam` x3 and `Triage` x5. The catalog is nevertheless built as an extensible map so a future `Project`/`Context`/`Actionable` command is a one-line addition.

2. **`TestSpam_Click` throws `KeyNotFoundException`, not `NullReferenceException`.** `issue.md` frames the defect as a `NullReferenceException` throughout, and mentions `TestSpam_Click` only in *Suspected Cause / Notes* (line 68). `TestSpam_Click` (`RibbonViewer.cs:261-264`) uses the dictionary **indexer** `Controller.Engines.InboxEngines[SpamBayes.GroupName]`, which throws `KeyNotFoundException` during the same window. Both exception types are in scope and both are named in AC11.

3. **The readiness signal is not added to `AppItemEngines`/`IAppItemEngines`.** `issue.md:66` and `issue.md:72` phrase R1 and R5a as a "readiness signal on `AppItemEngines`/`IAppItemEngines`". That placement is rejected: .NET Framework 4.8.1 has no default interface members, so any new interface member could only be bodied inside the `[ExcludeFromCodeCoverage]` `AppItemEngines` class and would be entirely uncoverable. R1 is instead satisfied by a first-class, named, unit-tested `EngineReadinessGate` that reads the already-published `IAppItemEngines.InboxEngines` member, giving both files a zero-line diff (AC15) and a stronger R4 compliance than the original phrasing would have permitted.

4. **A coarse `IsInitialized`/`InitTask` flag is rejected on correctness, not convenience.** `InitAsync` filters on `config.Value.Engine` (`AppItemEngines.cs:64`), so an engine that is configured off never enters the dictionary; a global "initialized" flag would report ready for a command that will never work, leaving the button enabled and the click still throwing. It also cannot represent `RestartEngineAsync` re-assigning a single key.

5. **`RibbonExplorer.xml` scope.** `issue.md` implies XML edits only. The file is 519 lines before the change (already above the 500-line guidance). The overage is pre-existing, the file is a declarative embedded UI resource rather than production/test/script code, and splitting it is a separate and larger change. Recorded as an accepted pre-existing exception in AC25 rather than remediated here.

---

## Delivery Notes and Deviations

Delivered 2026-08-08 on branch `bug/ribbon-engine-readiness-guard-503` against merge-base `003c5715055d7d1933db68a742531332756e30b2`, per plan `plan.2026-08-08T11-59.md`.

### What shipped

Four new host-neutral `internal` types under `TaskMaster\Ribbon\`, none marked `[ExcludeFromCodeCoverage]`, each at **100% line coverage**: `EngineCommandCatalog`, `EngineReadinessGate`, `EngineGatedCommandRunner`, `EngineCommandRefreshPlanner`. Two new thin partials inside the pre-existing coverage-exempt shims (`RibbonController.EngineCommands.cs`, `RibbonViewer.EngineCommands.cs`), `getEnabled="EngineCommand_GetEnabled"` on the eight engine-backed `<button>` elements, one post-initialization refresh in `ThisAddIn.cs`, and the `RibbonViewer.cs` partial-class split (487 to 388 lines). 45 new tests; suite 6293 to 6338, zero failed, zero skipped. `AppItemEngines.cs`, `IAppItemEngines.cs`, and `ApplicationGlobals.cs` each took a zero-line diff.

### Deviation 1 — notification presentation (design decision D3, section 5.6 of the plan)

The spec's parenthetical "non-modal indication" is aspirational. **The repository has no non-modal notification surface**, and introducing one is scope creep for a bug fix. The injected sink is therefore implemented in the `[ExcludeFromCodeCoverage]` `RibbonController.NotifyEngineCommandNotReady` shim as one `logger.Warn(message)` plus one `MessageBox.Show(message)`, matching the established user-facing notice mechanism already used at `RibbonViewer.cs:413`, `RibbonController.cs:80,167,189,196`, and `RibbonController.FolderTree.cs:250,263`.

The binding criterion AC12 requires only that exactly one notification is emitted through the injected `Action<string>` sink carrying the control id and the engine key, which is fully unit-tested by `RunAsync_WhenEngineNotReady_EmitsExactlyOneNotificationContainingControlIdAndEngineName`. No test constructs a `Form`, a `MessageBox`, or a message pump.

### Deviation 2 — CSharpier was scoped, not repo-wide, for the mutating pass (plan section 3 rule 5, decision D4)

`csharpier format` was invoked only with the thirteen scope-locked `.cs` paths and **never** repo-wide, and never with `AppItemEngines.cs` or `IAppItemEngines.cs` in its argument list. Rationale: a repo-wide `csharpier format .` would reformat any file that is unformatted at the merge-base, which would break the AC15 zero-line-diff requirement. The read-only repo-wide `csharpier check .` gate was still run and returned exit 0 over 1498 files (1488 at the merge-base; the difference is exactly the ten new `.cs` files). This makes the AC15 guarantee structural rather than dependent on a measurement taken at plan time.

### Deviation 3 — `UiThread` namespace correction

The plan and this spec both refer to `UtilitiesCS.Threading.UiThread.Dispatcher`. The type is declared in `UtilitiesCS\Threading\UiThread.cs` but its namespace is `UtilitiesCS`, not `UtilitiesCS.Threading`. `RibbonViewer.EngineCommands.cs` therefore imports `using UtilitiesCS;`. This is a namespace-versus-folder discrepancy in existing source; the explicit-marshalling design AC18 requires is unchanged.

### Deviation 4 — three null-forgiving annotations added for the nullable gate

The P6-T5 command (`msbuild /t:Build /p:Nullable=enable`) returns exit 0, but MSBuild's up-to-date check skips `CoreCompile` when only `/p:` values change, so that result alone does not prove new code is nullable-clean. A forced `/t:Rebuild` verification surfaced three diagnostics in authored code, each resolved without behaviour change:

- `EngineGatedCommandRunner.BuildNotReadyMessage` now looks up the already-non-null `renderedControlId` rather than the raw `controlId` (identical outcome: `"(null)"` is not a catalog key, so it still resolves to `"(unmapped)"`).
- `RibbonController.EngineCommands.cs` uses `() => Globals?.Engines!`, recording that a null result is a supported value the gate treats as "not ready".
- `RibbonViewer.EngineCommands.cs` uses `control?.Id!`, recording that a null id is a supported input yielding `false`.

All six new production files are now nullable-clean under a forced rebuild. The residual 220 `CS86xx` errors in `TaskMaster.csproj` are **pre-existing debt in untouched files** (including 18 in the AC15-protected `AppItemEngines.cs` and 40 in the AC15-protected `ApplicationGlobals.cs`) and are recorded, not remediated.

### No other deviation occurred

Apart from the four items above, the change was delivered exactly as designed in this spec and planned in `plan.2026-08-08T11-59.md`. Design option (c) was implemented without re-litigation; no `IsInitialized`/`InitTask` flag was introduced; no member was added to `IAppItemEngines`; no new `catch (Exception)` was introduced anywhere; no new `[ComVisible(true)]` type was added; and the only new `Microsoft.Office.*`-typed member is `public bool EngineCommand_GetEnabled(Office.IRibbonControl control)`.

### Outstanding

**AC19, AC20, and AC21 remain unchecked by design.** They are MANUAL-ONLY and require a live Outlook profile. The maintainer checklist is at `evidence/manual-verification/ac19-ac21-checklist.2026-08-08T15-00.md`, carrying `Status: PENDING MAINTAINER EXECUTION`. They must not be checked off on the strength of unit tests.

Two pre-existing defects observed during execution but deliberately not fixed (both outside the scope lock) are recorded for routing in `evidence/issue-updates/out-of-scope-promotions.2026-08-08T15-05.md`: the `CS2002` duplicate `<Compile Include>` in `UtilitiesCS.Test.csproj`, and the repository-wide nullable debt in `TaskMaster.csproj`.

