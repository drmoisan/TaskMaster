# ribbon-engine-toggle-state-guards (Spec)

- **Issue:** #505 (also closes #506, #518)
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-08T20-15
- **Status:** Ready for planning
- **Version:** 1.0
- **Work Mode:** full-bug
- **Branch:** `bug/ribbon-engine-toggle-state-guards-505` (from `origin/main` at `f910ff2f`)
- **Feature folder:** `docs/features/active/2026-08-08-ribbon-engine-toggle-state-guards-505/`

> **Authoritative AC source.** Work mode is `full-bug`. Per `.claude/skills/acceptance-criteria-tracking/SKILL.md`, this file is the **sole** authoritative acceptance-criteria source for issues #505, #506, and #518. No `user-story.md` exists for this delivery and none is to be created.

> **Authority order for this document.** The research artifact `research/2026-08-08T19-30-ribbon-engine-toggle-state-guards-research.md` is authoritative over `issue.md` wherever the two conflict. No conflict was found during authoring; the `## Correction Log` at the end of this document records that finding and remains the place to log any correction discovered later.

All file paths are relative to the worktree root
`C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a406ae4b7a2ce151f`. Line numbers were verified against the branch head in this worktree.

---

## Context

### Problem statement — three defects, one unit of work

Three defects share the Spam Config and Triage Config submenu callbacks in
`TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs`:

1. **#505 — invalid `getPressed` signature.** `SpamBayesEnabled_GetPressed` (lines 122-123) and
   `TriageEnabled_GetPressed` (lines 191-192) are declared `async Task<bool>`. The Office CustomUI
   `checkBox` `getPressed` contract requires a synchronous
   `public bool <Name>(Office.IRibbonControl control)`. VSTO matches callbacks by name and
   signature and silently ignores a mismatch (documented in-repo at
   `RibbonViewer.EngineCommands.cs:27-32`), so the two toggles never reflect real engine
   activation state and no error is reported.
2. **#506 — fire-and-forget toggle.** `SpamBayesEnabled_Click` (lines 119-120) and
   `TriageEnabled_Click` (lines 188-189) are `void` methods whose body discards the `Task` returned
   by `Controller.Engines.ToggleEngineAsync(...)`. There is no completion ordering, and a fault
   becomes an unobserved task. The sibling handlers in the same regions
   (`SpamSaveNetwork_Click` et al.) are `async void` with `await`, so this is an inconsistency,
   not a deliberate pattern.
3. **#518 — unguarded `Engines` dereference.** Ten call sites dereference
   `Controller.Engines.<member>` with no null guard. The merged #507 fix made
   `RibbonController.Engines` return `Globals?.Engines`
   (`TaskMaster/Ribbon/RibbonController.Intelligence.cs:204`), so before `SetGlobals` the property
   returns `null` and the `NullReferenceException` is raised at the call site instead of inside
   `get_Engines()`.

These are one unit of work rather than three because they are causally coupled in a single file:
a correct synchronous `getPressed` (#505) requires cached state; that cache is only correct if the
toggle is awaited, the cache refreshed, and the control invalidated in order (#506); and both
paths must honor the null-engines contract created by #507 (#518). Fixing any one alone would
rewrite the same four methods a second and third time. The maintainer's #518 comment explicitly
recommends addressing all three together.

Environment:

- OS/version: Windows 11, Outlook desktop (VSTO add-in host)
- Runtime: .NET Framework 4.8.1, TaskMaster VSTO add-in
- Trigger: Outlook Explorer ribbon, Spam Manager and Triage configuration menus
- Data source or fixture: `TaskMaster/Ribbon/RibbonExplorer.xml` embedded resource; live Outlook profile

Impact / Severity:

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Highest of the three (#505, Medium). Two configuration toggles display state not tied to the
underlying setting. #506 and #518 are each Low alone; see `issue.md` Impact section.

---

## Repro & Evidence

Steps to reproduce (from `issue.md`):

1. Open Outlook with the TaskMaster add-in loaded and let initialization complete.
2. Open the Spam Manager save-options menu; the "SpamBayes Enabled" toggle's pressed state is not
   driven by `EngineActiveAsync` because Office never binds the `Task<bool>` callback (#505).
3. Click the toggle; the handler returns before `ToggleEngineAsync` has awaited
   `Globals.AF.Manager.Configuration` and flipped `ClassifierActivated`. A failure induced inside
   the configuration load surfaces no error (#506).
4. Reload the add-in so the ribbon is constructed before `SetGlobals` assigns `Globals`, then
   invoke any of the ten listed callbacks: `NullReferenceException` at the call site (#518).
5. Repeat steps 2-3 for the "Triage Enabled" toggle.

Expected: see *Required behavior* below. Actual: the source excerpts in `issue.md` §Actual
Behavior, verified against this worktree.

### Verified defect surface — exactly 10 unguarded sites

`TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` contains **11** `Engines.` references.
`TestSpam_Click` (line 105) is already gated: its dereference sits inside a lambda passed to
`Controller.RunEngineCommandAsync(...)`, which `EngineGatedCommandRunner` evaluates only after
`EngineReadinessGate` reports ready. The remaining **10** are unguarded:

| Line | Callback | Expression | Group |
|---|---|---|---|
| 120 | `SpamBayesEnabled_Click` | `Controller.Engines.ToggleEngineAsync(SpamBayes.GroupName)` | toggle |
| 123 | `SpamBayesEnabled_GetPressed` | `Controller.Engines.EngineActiveAsync(SpamBayes.GroupName)` | toggle |
| 126 | `SpamSaveNetwork_Click` | `Controller.Engines.ShowDiskDialog(SpamBayes.GroupName, false)` | command |
| 129 | `SpamSaveLocal_Click` | `Controller.Engines.ShowDiskDialog(SpamBayes.GroupName, true)` | command |
| 132 | `GetSaveLocation_Click` | `Controller.Engines.ShowSaveInfo(SpamBayes.GroupName)` | command |
| 189 | `TriageEnabled_Click` | `Controller.Engines.ToggleEngineAsync("Triage")` | toggle |
| 192 | `TriageEnabled_GetPressed` | `Controller.Engines.EngineActiveAsync("Triage")` | toggle |
| 195 | `TriageSaveNetwork_Click` | `Controller.Engines.ShowDiskDialog("Triage", false)` | command |
| 198 | `TriageSaveLocal_Click` | `Controller.Engines.ShowDiskDialog("Triage", true)` | command |
| 201 | `TriageGetSaveLocation_Click` | `Controller.Engines.ShowSaveInfo("Triage")` | command |

### XML wiring facts (drives the design split)

- The two toggle controls are `checkBox` elements: `SpamBayesEnabledToggle`
  (`TaskMaster/Ribbon/RibbonExplorer.xml:141`) and `TriageEnabledToggle` (line 520), wired
  `getPressed="SpamBayesEnabled_GetPressed"` / `onAction="SpamBayesEnabled_Click"` (and the Triage
  mirror).
- The six command controls are `button` elements with **no** `getEnabled`:
  `SpamSaveNetwork` (xml 115-120), `SpamSaveLocal` (121-126), `GetSaveState` (127-132),
  `TriageSaveNetwork` (500-505), `TriageSaveLocal` (506-511), `TriageGetSaveState` (512-517).
  Note the id/engine-name divergence: the "current location" buttons are `GetSaveState` /
  `TriageGetSaveState`, while their `onAction` handlers are `GetSaveLocation_Click` /
  `TriageGetSaveLocation_Click`.
- An existing test (`TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs:253-273`) requires every
  `EngineCommandCatalog` id to resolve to a `button` element, so the two `checkBox` toggles can
  never join `EngineCommandCatalog`.

---

## Scope & Non-Goals

**In scope**

- Rewriting the two `*_GetPressed` callbacks to the exact synchronous Office signature (#505).
- Rewriting the two toggle `*_Click` handlers to awaited, exception-observing form (#506).
- Guarding all 10 unguarded `Controller.Engines.<member>` sites (#518), using two guard shapes
  matched to the two semantic groups (see *Proposed Fix*).
- One new host-neutral, fully unit-tested seam type (`EngineToggleStateCoordinator`) plus its
  engine-key-to-control-id map, under `TaskMaster/Ribbon/`.
- Six new `EngineCommandCatalog` entries and the matching `getEnabled` attributes in
  `RibbonExplorer.xml` (forced by the existing catalog/XML set-equality tests).
- Thin glue inside the existing `[ExcludeFromCodeCoverage]` `RibbonController` / `RibbonViewer`
  partials; no new coverage exemption.
- Red-first regression tests, new-seam unit tests, coverage evidence, and a manual-verification
  checklist.

**Out of scope / non-goals**

- **Re-fixing #507.** `RibbonController.Engines => Globals?.Engines`
  (`RibbonController.Intelligence.cs:204`) is merged and correct; the `?.` must not be reverted
  and the property is not modified.
- **Refactoring the rest of the ribbon surface.** Only the ten enumerated sites and the glue
  required to guard them change; `TestSpam_Click` and every other callback are untouched.
- **Issue #522** — the defective `CLAUDE.md` type-check command (`/p:Nullable=enable` on a
  solution where nullable is per-file opt-in). This spec documents the deviation (see
  *Verification*) but does not fix the command or the underlying nullable debt.
- **The further defects catalogued in research §10 for separate promotion:** the five orphan
  `onAction` callbacks in `RibbonExplorer.xml` (`BtnMigrateIDs_Click` and the four
  `_Clicked`-vs-`_Click` QF-settings mismatches, xml lines 265-283), and the unguarded `Globals`
  dereferences in `RibbonController.Intelligence.cs` reachable pre-`SetGlobals` (e.g. lines 220,
  230, 29-58). Each is promoted to its own issue, not fixed here.
- No change to `IAppItemEngines`, `AppItemEngines`, `AsyncLazy<T>`, or `ThisAddIn`.
- No eager cache prime from the VSTO entry point (rejected in research §9; a planner may propose
  it later as a separate optimization).
- No Outlook UI-automation harness; live-ribbon behavior remains manual verification.

**Explicitly excluded systems, integrations, datasets:** no live Outlook process, no live mail
profile, no network, no filesystem, and no temporary files in any automated test.

---

## Root Cause Analysis

- **#505:** the two `*_GetPressed` methods were relocated verbatim from `RibbonViewer.cs` by #503
  (PR #515) with the pre-existing `async Task<bool>` shape intact. Office requires a synchronous
  `bool`, but the truth (`IAppItemEngines.EngineActiveAsync`) is behind
  `await Globals.AF.Manager.Configuration` (`TaskMaster/AppGlobals/AppItemEngines.cs:101-109`),
  so a naive synchronous rewrite would have to block the STA — which is why the defect persisted.
- **#506:** the two toggle `_Click` handlers discard the `Task` from `ToggleEngineAsync`
  (`AppItemEngines.cs:92-99`); a fault in the awaited configuration load is swallowed into an
  unobserved task.
- **#518:** the ten sites predate any readiness/guard seam; after #507, `Controller.Engines`
  yields `null` pre-`SetGlobals` and each site dereferences it directly.

Why the existing #503 gate cannot cover all ten sites: `EngineReadinessGate` probes
`enginesAccessor()?.InboxEngines` per key (`TaskMaster/Ribbon/EngineReadinessGate.cs:64-101`),
but `ToggleEngineAsync` / `EngineActiveAsync` operate on **configuration**
(`AppItemEngines.cs:92-109`), and `InitAsync` filters engines that are configured off out of
`InboxEngines` (`AppItemEngines.cs:63-64`). A disabled engine never enters `InboxEngines`, so a
readiness-gated toggle could never re-enable it, and a readiness-gated `getPressed` could never
report its true state. Conversely, `ShowDiskDialog` (`AppItemEngines.cs:237-252`) and
`ShowSaveInfo` (`AppItemEngines.cs:276-282`) begin with `InboxEngines.TryGetValue(...)` and no-op
when the key is absent, so for the six command sites the readiness gate is semantically exact.

---

## Proposed Fix

### Design summary — two guard shapes for two semantics

This distinction is load-bearing and must survive into the plan:

1. **The four toggle/getPressed sites (lines 120, 123, 189, 192)** are backed by *configuration*,
   not `InboxEngines`. They route through a **new host-neutral
   `EngineToggleStateCoordinator`** that owns a last-known-state cache, an async prime, the
   awaited toggle path, and the invalidation ordering. They must **not** route through
   `RunEngineCommandAsync`: the readiness predicate is wrong for them (a disabled engine is absent
   from `InboxEngines`, so the gate would permanently block re-enabling), `getPressed` is a
   synchronous poll for which a notification-per-blocked-call is unacceptable, and the `checkBox`
   ids cannot join `EngineCommandCatalog` because the existing XML test requires catalog ids to be
   `button` elements (`RibbonExplorerXmlTests.cs:253-273`).
2. **The six save/info command sites (lines 126, 129, 132, 195, 198, 201)** are backed by
   `InboxEngines` and no-op without the key, so the **existing
   `RibbonController.RunEngineCommandAsync` gate** (#503, `EngineGatedCommandRunner.cs:97-111`)
   is semantically exact. They join `EngineCommandCatalog` (six new entries at
   `EngineCommandCatalog.cs:36-49`: `SpamSaveNetwork`, `SpamSaveLocal`, `GetSaveState` →
   `"Spam"`; `TriageSaveNetwork`, `TriageSaveLocal`, `TriageGetSaveState` → `"Triage"`) and gain
   `getEnabled="EngineCommand_GetEnabled"` in the XML — forced by the existing catalog-derived
   set-equality tests (`RibbonExplorerXmlTests.cs:177-245`).

This is the read/command asymmetry: reads get a cached-state answer with a silent `false`
default; commands get gated invocation with one user-facing notification.

### New type: `EngineToggleStateCoordinator`

`TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, `internal sealed`, namespace `TaskMaster`
(matching the four #503 seam files), deliberately **not** `[ExcludeFromCodeCoverage]`.

Surface (constructor-injected delegates; no COM types; no `MessageBox`; no logger reference —
logging is a delegate):

```csharp
internal EngineToggleStateCoordinator(
    Func<IAppItemEngines> enginesAccessor,      // returns null before SetGlobals; supported
    Action<string> invalidateControl,           // receives a ribbon control id
    Action<string> notifyUnavailable,           // one message per blocked toggle click
    Action<string, Exception> logError)         // observed prime/toggle faults

internal bool GetPressed(string engineName)               // sync cache read + lazy prime start
internal Task HandleToggleClickAsync(string engineName)   // boundary: guard -> toggle -> refresh -> invalidate; catches and logs
internal Task ExecuteToggleAsync(string engineName)       // no catch; propagates — the testable core ordering
```

Plus an engine-key-to-control-id map (`"Spam"` → `SpamBayesEnabledToggle`, `"Triage"` →
`TriageEnabledToggle`), kept **separate from `EngineCommandCatalog`** so the toggles never acquire
readiness-gated `getEnabled` semantics and the catalog's button-only XML tests stay valid.
`SpamBayes.GroupName == "Spam"`
(`UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.cs:328`).

### Thin glue (all inside existing exempt types; no new exemption)

- `TaskMaster/Ribbon/RibbonController.EngineCommands.cs`: a lazy `_engineToggleCoordinator`
  mirroring the `EngineCommands` property (lines 38-45), wired with `() => Globals?.Engines!`,
  `controlId => _viewer?.InvalidateEngineToggle(controlId)`, `NotifyEngineCommandNotReady`
  (reused, line 97) and `(m, ex) => logger.Error(m, ex)`; plus forwarders
  `IsEngineToggleActive(string)` / `HandleEngineToggleClickAsync(string)`.
- `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs`: `internal void
  InvalidateEngineToggle(string controlId)` following the `InvalidateEngineCommands`
  dispatcher-marshalling pattern (lines 63-81), and the rewritten callbacks:

```csharp
public bool SpamBayesEnabled_GetPressed(Office.IRibbonControl control) =>
    _controller?.IsEngineToggleActive(SpamBayes.GroupName) ?? false;

public async void SpamBayesEnabled_Click(Office.IRibbonControl control, bool pressed) =>
    await Controller.HandleEngineToggleClickAsync(SpamBayes.GroupName);
```

(Triage mirrors with `"Triage"`.) Each of the six command handlers becomes the `TrainSpam_Click`
shape (lines 88-92), with the engine dereference deferred into the lambda; the two `void`
`ShowSaveInfo` handlers wrap as `() => { Controller.Engines.ShowSaveInfo(...); return
Task.CompletedTask; }` and become `async void` + `await`. `GetSaveLocation_Click` and
`TriageGetSaveLocation_Click` keep their method names (pinned by `onAction` in the XML) while
using catalog ids `GetSaveState` / `TriageGetSaveState`.

### Required behavior (testable statements)

**B1 — Synchronous `getPressed` contract.** Both `*_GetPressed` callbacks are declared exactly
`public bool <Name>(Office.IRibbonControl control)` — public instance, `bool` return, one
parameter, no `async`, no `Task<bool>`. Office polls this on the STA; the implementation performs
a dictionary read only and never awaits, blocks, or throws.

**B2 — Cached-state read semantics and the pre-prime default.** `GetPressed(engineName)` returns
the cached value for the key. A never-primed key returns `false` (unchecked) — the correct
pre-`SetGlobals` degradation and the same "false when unknown" convention as
`EngineCommand_GetEnabled` (`RibbonViewer.EngineCommands.cs:38-39`). On a cache miss with engines
available, `GetPressed` starts at most one prime task per key:
`await enginesAccessor().EngineActiveAsync(name)` → store → invoke `invalidateControl(controlId)`
so Office re-queries and the checkbox corrects itself. A second read during an in-flight prime
starts no second prime. A prime fault is observed and routed to `logError`; `GetPressed` still
returns `false`; no unobserved task remains. With a null engines accessor result, `GetPressed`
returns `false` and starts nothing.

**B3 — Ordering invariant on toggle.** `ExecuteToggleAsync` performs, in order:
`await ToggleEngineAsync(name)` → `await EngineActiveAsync(name)` → cache update → control
invalidation. Update-before-invalidate is the invariant that prevents Office re-querying stale
state; it is verified as a recorded call sequence in unit tests.

**B4 — Observed-and-logged failure at the `async void` boundary.** The click boundary lives in
`HandleToggleClickAsync` (host-neutral, unit-testable): it catches a fault from the toggle path,
formats a message naming the engine key, invokes `logError` (production wiring
`logger.Error(message, exception)`, the per-type log4net pattern of `RibbonViewer.cs:60-62`), does
not rethrow, and does not invalidate. `ExecuteToggleAsync` itself never catches — faults propagate
to the boundary, preserving the #503 fail-fast philosophy (`EngineGatedCommandRunner.cs:22-25`
deliberately contains no catch). The viewer handler is a single awaited expression that can no
longer fault.

**B5 — Graceful degradation at all ten sites.** Before `SetGlobals` (null engines):
the two `getPressed` sites return `false`; the two toggle clicks produce exactly one
`notifyUnavailable` message and invoke nothing; the six command sites are blocked by the closed
readiness gate with one "still loading" notification and no action invocation. No site raises
`NullReferenceException`.

**B6 — Ready-path preservation.** Once engines are available, the six command sites execute the
same engine expressions as today inside their lambdas; `TestSpam_Click` is functionally
unchanged; toggling an engine that is configured off remains possible (the coordinator never
consults `InboxEngines`).

**B7 — Intended UX change (command sites).** The six save/info buttons render disabled until
their engine is loaded (previously always enabled and silently no-oping), and are re-enabled by
the existing post-load refresh (`EngineCommandRefreshPlanner.InvalidateAll` driven by
`ControlIds`; production call site `TaskMaster/ThisAddIn.cs:82`). The comment at
`RibbonExplorerXmlTests.cs:218-222` is updated to match. The planner-visible consequence: the
catalog change and the XML `getEnabled` change must land atomically because the set-equality
tests derive expectations from `ControlIds`.

### Design constraints

- **Host-neutral seam.** `EngineToggleStateCoordinator` follows the #503 precedent
  (`EngineReadinessGate`, `EngineGatedCommandRunner`): `internal sealed`, constructor-injected
  delegates, null-argument `ArgumentNullException` at construction, zero `Microsoft.Office.*` and
  zero `Microsoft.Office.Interop.Outlook` references, no `MessageBox`, no WinForms types. The only
  STA-affine operation, `IRibbonUI.InvalidateControl`, stays behind the injected delegate whose
  production implementation marshals through `UtilitiesCS.UiThread.Dispatcher` exactly like
  `InvalidateEngineCommands` (`RibbonViewer.EngineCommands.cs:71-80`).
- **No STA blocking — prohibition.** `.Result`, `.Wait()`, and `GetAwaiter().GetResult()` on the
  engine tasks are prohibited anywhere in this change. Rationale: ribbon callbacks run on the
  Outlook STA, and controller paths install a `WindowsFormsSynchronizationContext` on that thread
  (`RibbonController.Intelligence.cs:194-197, 283-286`). A continuation of
  `await Globals.AF.Manager.Configuration` posted back to a blocked STA is a deterministic
  deadlock, and whether the context is installed when Office polls `getPressed` depends on which
  callbacks ran first — an intermittent failure. Even absent deadlock, the first `Configuration`
  await triggers the full classifier-configuration disk load, which would freeze menu-open.
- **Nullable handling.** `#nullable` is per-file opt-in in this repository and **no** file under
  `TaskMaster/Ribbon/` carries the pragma (verified by grep; the only `#nullable` directives in
  the `TaskMaster` project are in five `AppGlobals` files). New files follow the sibling seam
  convention: no `#nullable enable` pragma, null contracts documented in XML doc comments,
  null-forgiving operators only where the #503 files already model them
  (`EngineCommandCatalog.cs:78-81`, `RibbonController.EngineCommands.cs:40-43`,
  `RibbonViewer.EngineCommands.cs:34-37`).
- **File-size cap.** Every touched file stays at or under 500 lines. Largest touched file today is
  `RibbonViewer.EngineCommands.cs` at 207 lines; the rewrite adds roughly 40-60 lines of
  docs+glue.
- **Coverage placement.** `RibbonViewer` (`RibbonViewer.cs:31-32`) and `RibbonController`
  (`RibbonController.cs:36`) carry type-level `[ExcludeFromCodeCoverage]` under the ratified
  VSTO/COM ribbon-handler exemption. The exemption is neither removed nor widened; all decision
  logic lives in the non-exempt coordinator.

### Rejected alternatives (research §3, §9)

- Blocking the STA in `getPressed` — intermittent deadlock, menu-open freeze, policy violation.
- Synchronous read of the materialized `Configuration` — `AsyncLazy<T>`
  (`UtilitiesCS/ReusableTypeClasses/AsyncLazy/AsyncLazy.cs:19-49`) exposes no non-triggering
  completed-value probe; a new `IAppItemEngines` member would body in the coverage-excluded
  `AppItemEngines` (net481 has no default interface members).
- Routing the toggles through `RunEngineCommandAsync` / adding the checkboxes to
  `EngineCommandCatalog` — wrong predicate, permanently blocks re-enabling, fails the button-only
  XML test, notification-per-poll.
- Ten ad-hoc `?.` null checks — disrecommended by the maintainer's #518 comment; silent no-ops
  with no tested decision logic.
- Eager prime from `ThisAddIn` post-load — touches the VSTO entry point and misses the
  menu-opened-before-load window; lazy prime-on-read covers both windows.

### Dependencies or blocked work

None. No new NuGet package, no project-reference change, no `IAppItemEngines` change (every
`Mock<IAppItemEngines>` compiles unchanged, including
`TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs:92-135`), no #507-test impact
(`TaskMaster.Test/Ribbon/RibbonControllerTests.Engines.cs:19-71` asserts a property this change
does not touch). `InternalsVisibleTo("TaskMaster.Test")` already exists. The legacy non-SDK
projects require explicit `<Compile Include>` entries for new files.

---

## Assumptions, Constraints, Dependencies

- **Assumptions:** `ConcurrentDictionary<string, bool>` reads are safe from any thread; the
  toggle path has a single writer per key (user gesture on the STA); every cache write is
  followed by an invalidation, so the UI converges even when a prime and a toggle overlap.
- **Constraints:** .NET Framework 4.8.1 (no default interface members); 500-line file cap;
  MSTest + Moq + FluentAssertions only; no temporary files, no `Thread.Sleep`/`Task.Delay`, no
  wall-clock reads, no message pump, no live COM in tests.
- **External dependencies:** none beyond packages already referenced.

---

## Data / API / Config Impact

- **User-facing changes:** the two toggles begin reflecting real activation state and correcting
  themselves after the async prime completes; the six save/info buttons render disabled until
  their engine loads (B7). A blocked toggle click produces the existing "not available"
  notification instead of silently doing nothing or throwing.
- **API changes:** one new `internal sealed` type plus internal glue members; no public
  cross-project contract changes; the two `*_GetPressed` signatures change but have zero
  compile-time callers (they are reached only via XML strings — research §8).
- **Data or migration:** none.
- **Logging/telemetry:** observed toggle/prime faults are logged via `logger.Error`; blocked
  clicks reuse the existing `NotifyEngineCommandNotReady` warning path
  (`RibbonController.EngineCommands.cs:97-101`).
- **Compatibility notes:** no CLI flag, config schema, or version change. Rollback is a straight
  revert; no data or contract migration.

---

## Test Strategy

Framework: **MSTest** + **Moq** + **FluentAssertions**, Arrange–Act–Assert with descriptive names
and doc comments. Every automated test is deterministic and uses **no** temporary files, **no**
`Thread.Sleep`/`Task.Delay`/real wall-clock waits, **no** real WinForms message pump, **no**
`Form`, `MessageBox`, or `BackgroundWorker`, and **no** live COM or Outlook process. All async
control flows through `TaskCompletionSource`; `MockBehavior.Strict` where the #503 tests use it.
No STA test is needed anywhere in this design; no `*.StaTests.cs` file is required.

**Hard constraint:** viewer/controller-level behavioral tests must not drive a path that reaches
`NotifyEngineCommandNotReady`, because it calls `MessageBox.Show`
(`RibbonController.EngineCommands.cs:100`) and would hang vstest. Behavioral assertions live at
the coordinator/catalog seam with injected sinks (the #503 approach,
`EngineGatedCommandRunnerTests.cs:29-36`).

### Red-before-fix regression tests (research §7, R1-R5)

Each is written first and demonstrated failing against the pre-fix code, then passing after the
fix, per the `CLAUDE.md` bugfix workflow:

| # | Test (file → assertion) | Red because (today) | ACs |
|---|---|---|---|
| R1 | `RibbonExplorerXmlTests` — for `SpamBayesEnabledToggle`/`TriageEnabledToggle`, the XML `getPressed` attribute resolves to a public instance method on `RibbonViewer` returning `bool` with one parameter whose `ParameterType.FullName == "Microsoft.Office.Core.IRibbonControl"` (the test project has no Office PIA reference, so the `FullName` comparison at `RibbonExplorerXmlTests.cs:289-314` is mandatory); also pins `onAction` → `void (IRibbonControl, bool)` | both `*_GetPressed` return `Task<bool>` | AC-1, AC-2, AC-4 |
| R2 | reflection-invoke each `*_GetPressed` on `new RibbonViewer(new RibbonController())` (pre-`SetGlobals`); if the result is a `Task`, await it; assert no exception and, once synchronous, `false`. Reflection invocation keeps the test compiling across the signature change | faulted task / NRE from null `Engines` | AC-3, AC-12 |
| R3 | `EngineCommandCatalogTests` — `TryGetEngineName` maps the six command ids to `"Spam"`/`"Triage"`; `ControlIds` has 14 entries | ids absent from `Map` | AC-10 |
| R4 | `RibbonExplorerXmlTests` — the existing set-equality tests force `getEnabled` onto the six buttons once R3's catalog change lands (red mid-change, green when XML updated in the same task) | XML lacks the attributes | AC-10 |
| R5 | reflection shape pin — `SpamBayesEnabled_Click`/`TriageEnabled_Click` return `void`, take `(IRibbonControl, bool)` by `FullName`, and carry `AsyncStateMachineAttribute` (pins the awaited `async void` shape); same pin on the command handlers whose shape changes (`GetSaveLocation_Click`, `TriageGetSaveLocation_Click` gain the attribute) | the two toggle Clicks and the two `ShowSaveInfo` handlers are plain `void` | AC-5, AC-6, AC-8 |

R1 is the authoritative deterministic repro for #505: the silent-binding failure surfaces as a
build-time-red test, which is what AC-4 requires.

### New-seam unit tests (written with the coordinator; new-code target >= 90%)

`TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` — scenario completeness per UT2:

- Constructor null-argument contracts (`ArgumentNullException`, matching
  `EngineGatedCommandRunner.cs:60-65`).
- `GetPressed` unknown/null/whitespace key → `false`, no prime, no invalidate.
- `GetPressed` with null engines accessor result → `false`, nothing started (pre-`SetGlobals`).
- `GetPressed` cache miss with engines available → exactly one prime; a second call during the
  in-flight prime (TCS not yet completed) starts no second prime; on completion the cache holds
  the value and `invalidateControl` received the mapped control id (`SpamBayesEnabledToggle` for
  `"Spam"`).
- Prime fault → `logError` invoked with the exception; `GetPressed` still returns `false`; no
  unobserved task (the prime `Task` is exposed internally so the test awaits it
  deterministically).
- `ExecuteToggleAsync` ordering: recorded sequence `ToggleEngineAsync` → `EngineActiveAsync` →
  cache visible via `GetPressed` → `invalidateControl` (the B3 update-before-invalidate
  invariant).
- `ExecuteToggleAsync` propagates the toggle fault unchanged; `HandleToggleClickAsync` observes
  it, calls `logError`, does not throw, does not invalidate (B4).
- `HandleToggleClickAsync` with null engines → one `notifyUnavailable` message, no toggle call, no
  throw (B5).

Planning note from research §8: audit `EngineCommandCatalogTests` (and the other #503 seam tests)
for any hard-coded entry count of 8 before extending the catalog.

### Manual validation (cannot be automated in this repository)

Recorded under `docs/features/active/2026-08-08-ribbon-engine-toggle-state-guards-505/evidence/manual-verification/`:

1. With "Show add-in user interface errors" enabled, confirm no callback-binding error is
   reported for either toggle (proves the corrected `getPressed` actually binds — VSTO does not
   report a mismatch).
2. Toggle each engine; confirm the checkbox state updates after the click and survives a menu
   reopen.
3. Reload the add-in and invoke the ten callbacks before initialization completes; confirm no
   `NullReferenceException` and the expected disabled/notified behavior.

---

## Verification — toolchain and the documented #522 deviation

Run in this exact order and repeat until all steps pass in a single uninterrupted pass:

1. **Format:** `dotnet tool run csharpier .` (or `csharpier .`)
2. **Analyze:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. **Type-check:** `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. **Test:** `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` — excluding stale
   `\.claude\worktrees\` builds from the test-assembly glob.

> **Deliberate, documented deviation from `CLAUDE.md` (issue #522).** The `CLAUDE.md` type-check
> command adds `/p:Nullable=enable /p:TreatWarningsAsErrors=true`. That command is known-defective
> and tracked as **issue #522**: nullable is per-file opt-in in this solution, and forcing
> `/p:Nullable=enable` solution-wide reports 200-414 errors that are red on `main` regardless of
> any change. The repository's CI (`.github/workflows/ci.yml`) deliberately omits
> `/p:Nullable=enable`. Verification for this delivery therefore uses CI's actual command (step 3
> above). A reviewer encountering the missing `/p:Nullable=enable` should read this paragraph and
> the #522 citation as the authority, not as non-compliance.

Evidence: toolchain outputs and coverage XML under
`docs/features/active/2026-08-08-ribbon-engine-toggle-state-guards-505/evidence/qa-gates/`;
merge-base baseline coverage under `.../evidence/baseline/` captured before implementation.

---

## Coverage Posture

- The modified handlers live in type-level `[ExcludeFromCodeCoverage]` classes
  (`RibbonViewer.cs:31-32`, `RibbonController.cs:36`) under the ratified VSTO/COM exemption, so
  **this change adds little or no coverage surface**. A flat repo-wide coverage number after this
  change is therefore not a regression and must not be treated as one.
- The newly extracted `EngineToggleStateCoordinator` (and its key/control-id map) is **not**
  exempt and must reach the repository new-code floor: **>= 90% line coverage** per `CLAUDE.md`
  § UT2.
- No coverage regression on changed lines.
- No baseline evidence exists in this feature folder at spec-authoring time. The merge-base
  baseline is captured under `.../evidence/baseline/` before implementation; the repo-wide figure
  is a **record-and-report** obligation compared against that baseline on the testable
  denominator defined in `CLAUDE.md` § UT2, not an absolute numeric floor imposed by this change.
- The exemption is neither removed nor widened; no `[ExcludeFromCodeCoverage]` attribute is added
  to any new type.

---

## Acceptance Criteria

Traceability: each criterion cites the `issue.md` restatement tags (iAC1-iAC17) it covers; every
issue tag maps to at least one criterion below. Criteria marked **MANUAL-ONLY** must never be
checked off on the strength of unit tests; they require recorded live-Outlook verification.

### Signatures and toggle state (#505)

- [x] **AC-1 (iAC1)** `SpamBayesEnabled_GetPressed` is declared exactly
  `public bool SpamBayesEnabled_GetPressed(Office.IRibbonControl control)` — no `async`, no
  `Task<bool>`. Verified by the R1 reflection pin (return type `bool`, one parameter,
  `ParameterType.FullName == "Microsoft.Office.Core.IRibbonControl"`).
- [x] **AC-2 (iAC2)** `TriageEnabled_GetPressed` is declared exactly
  `public bool TriageEnabled_GetPressed(Office.IRibbonControl control)`. Verified by the same R1
  pin.
- [x] **AC-3 (iAC3)** Both callbacks return engine activation state derived from
  `IAppItemEngines.EngineActiveAsync` through the coordinator's cache, and the change contains no
  `.Result`, `.Wait()`, or `GetAwaiter().GetResult()` on engine tasks anywhere (grep over the
  branch diff returns zero occurrences). The cached-read semantics of B2 — never-primed key
  returns `false`; at most one prime per key; prime completion updates the cache and invalidates
  the mapped control; prime fault is logged and leaves `false` — are each verified by a named
  test in `EngineToggleStateCoordinatorTests`.
- [x] **AC-4 (iAC4)** The R1 signature pins for both `getPressed` and both `onAction` callbacks
  exist in `RibbonExplorerXmlTests` (or a sibling viewer test), so a future signature regression
  fails the build rather than failing silently in Office.

### Awaited toggle and ordering (#506)

- [x] **AC-5 (iAC5)** `SpamBayesEnabled_Click` observes the completion of the toggle: it is
  `async void`, awaits `Controller.HandleEngineToggleClickAsync(SpamBayes.GroupName)`, and
  carries `AsyncStateMachineAttribute` (R5 pin). No discarded `Task` remains at line 120's
  replacement.
- [x] **AC-6 (iAC6)** `TriageEnabled_Click` likewise awaits `HandleEngineToggleClickAsync("Triage")`
  and carries `AsyncStateMachineAttribute` (R5 pin).
- [x] **AC-7 (iAC7)** A fault raised inside the toggle path is observed at the
  `HandleToggleClickAsync` boundary and reported through the injected `logError` delegate
  (production wiring `logger.Error(message, exception)`); the boundary does not rethrow and does
  not invalidate; `ExecuteToggleAsync` itself propagates the fault unchanged (no new
  `catch (Exception)` below the boundary). Verified by named tests in
  `EngineToggleStateCoordinatorTests` asserting on the injected delegates.
- [x] **AC-8 (iAC8)** Both toggle handlers match the `async void` + single awaited expression
  shape of the sibling `*SaveNetwork_Click`/`*SaveLocal_Click` handlers in the same regions;
  the two `ShowSaveInfo` handlers (`GetSaveLocation_Click`, `TriageGetSaveLocation_Click`) also
  become `async void` + `await`. Verified by the R5 shape pins.
- [x] **AC-9 (iAC9)** After a toggle completes, the coordinator updates the cache **before**
  invoking `invalidateControl` with the mapped control id, so Office re-queries `getPressed`
  against fresh state. Verified by a recorded-sequence test
  (`ToggleEngineAsync` → `EngineActiveAsync` → cache readable → `invalidateControl`) in
  `EngineToggleStateCoordinatorTests`.

### Guarded dereferences (#518)

- [x] **AC-10 (iAC10)** All 10 unguarded sites in the defect-surface table are guarded with the
  shape matched to their semantics: the four toggle/getPressed sites route through
  `EngineToggleStateCoordinator` (never through `RunEngineCommandAsync`), and the six command
  sites route through `Controller.RunEngineCommandAsync` with the engine dereference deferred
  into the lambda, backed by six new `EngineCommandCatalog` entries (`SpamSaveNetwork`,
  `SpamSaveLocal`, `GetSaveState` → `"Spam"`; `TriageSaveNetwork`, `TriageSaveLocal`,
  `TriageGetSaveState` → `"Triage"`) and matching `getEnabled="EngineCommand_GetEnabled"` XML
  attributes. Verified by R3, R4, and source inspection of the ten rewritten call sites.
- [x] **AC-11 (iAC11)** `TestSpam_Click` is functionally unchanged, and the guarded-site count is
  verified and reported as exactly **10**: a grep of `Engines\.` over the post-change
  `RibbonViewer.EngineCommands.cs` shows every remaining production dereference inside a gated
  lambda or behind the coordinator, and the count of newly guarded sites (10) plus the
  pre-existing gated site (1) is recorded in the review evidence.
- [x] **AC-12 (iAC12)** No call site raises `NullReferenceException` when invoked before
  `SetGlobals` has assigned `Globals`: `getPressed` returns `false` (R2); a toggle click emits
  exactly one `notifyUnavailable` message and invokes nothing; the six command sites are blocked
  by the closed gate without invoking their action. Verified by R2 plus named coordinator tests.
- [x] **AC-13 (iAC13)** `RibbonController.Engines` remains exactly
  `internal IAppItemEngines Engines => Globals?.Engines;`
  (`RibbonController.Intelligence.cs:204`) — a zero-line diff on that member; the `?.` is not
  reverted and #507 is not re-fixed. Verified by branch diff inspection and the untouched
  `RibbonControllerTests.Engines.cs` suite passing unchanged.

### Cross-cutting (iAC14-iAC17)

- [x] **AC-14 (iAC14)** `EngineToggleStateCoordinator` (and any sibling map type) is host-neutral
  — zero `Microsoft.Office.*` and zero `Microsoft.Office.Interop.Outlook` using directives, no
  `MessageBox`, no WinForms types — and carries no `[ExcludeFromCodeCoverage]` attribute; no
  existing exemption is removed or widened, and no new exemption is added anywhere in the change.
  Verified by grep over the new and changed files.
- [x] **AC-15 (iAC15)** The R1-R5 regression tests are written first and demonstrated red against
  the pre-fix code (recorded run output under `.../evidence/regression-testing/`), then green
  after the fix, per the `CLAUDE.md` bugfix workflow.
- [x] **AC-16 (iAC16)** The full toolchain passes in a single uninterrupted final pass:
  `csharpier .` reports no changes; the analyzer msbuild
  (`/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`) completes with zero errors and
  no new diagnostics; the type-check msbuild uses **CI's command**
  (`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`,
  without the defective `/p:Nullable=enable`, per issue #522 as documented in *Verification*) and
  completes with zero errors; `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
  reports zero failed and zero skipped tests, with stale `\.claude\worktrees\` assemblies excluded
  from the glob. Outputs recorded under `.../evidence/qa-gates/`.
- [x] **AC-17 (iAC17)** Scope is held to #505/#506/#518: the research §10 defects (orphan
  `onAction` callbacks; `RibbonController.Intelligence.cs` unguarded `Globals` dereferences) and
  any further defect found during execution are promoted to their own issues through the
  promotion lifecycle, not fixed here. Verified by recorded promotion receipts (checking the
  tracker first for existing issues) and by the branch diff containing no changes outside the
  declared scope.

### Quality gates (decomposed from iAC14-iAC16 for independent verification)

- [x] **AC-18 (iAC14)** `EngineToggleStateCoordinatorTests` covers every scenario listed in
  *Test Strategy* (constructor contracts; unknown/null key; null accessor; single prime;
  prime fault; toggle ordering; fault propagation vs. boundary observation; blocked click), and
  `EngineToggleStateCoordinator` reaches **>= 90% line coverage** in the final coverage XML under
  `.../evidence/qa-gates/`.
- [x] **AC-19 (iAC16)** A merge-base coverage baseline is captured under `.../evidence/baseline/`
  before implementation; the post-change figure is compared against it showing no regression on
  changed lines; the repo-wide figure on the `CLAUDE.md` § UT2 testable denominator is recorded
  and reported in the comparison artifact (record-and-report, not an independent numeric floor —
  the modified handlers are coverage-exempt, so a flat repo-wide figure is expected and is not a
  regression).
- [x] **AC-20 (iAC14, iAC16)** No automated test in this change creates a temporary file, calls
  `Thread.Sleep` or `Task.Delay`, reads the wall clock, constructs a `Form`, `MessageBox`, or
  `BackgroundWorker`, starts a WinForms message pump, or touches live COM/Outlook; no test drives
  a path reaching `NotifyEngineCommandNotReady`. Verified by grep over the new and changed test
  files plus the AC-16 test run.
- [x] **AC-21 (iAC10)** Every file touched by this change is at or under **500 lines** after the
  change, verified by a line count of each path in the branch diff. (`RibbonExplorer.xml` retains
  its pre-existing recorded overage from #503; it is a declarative embedded UI resource and is
  not remediated here.)
- [ ] **AC-22 (iAC9) — MANUAL-ONLY.** In a live Outlook session with "Show add-in user interface
  errors" enabled: no callback-binding error is reported for either toggle; each toggle's state
  updates after a click and survives a menu reopen; the ten callbacks invoked before
  initialization completes produce no `NullReferenceException`. Outcome recorded under
  `.../evidence/manual-verification/`; must not be checked off on the strength of unit tests.
- [x] **AC-23** This spec and `issue.md` reflect the delivered outcome, including any deviation
  recorded in a `## Delivery Notes and Deviations` section, and the `issue.md` restatement items
  iAC1-iAC17 are checked off in `issue.md` as their covering criteria here are verified.

**Issue-tag coverage map:** iAC1→AC-1; iAC2→AC-2; iAC3→AC-3; iAC4→AC-4; iAC5→AC-5; iAC6→AC-6;
iAC7→AC-7; iAC8→AC-8; iAC9→AC-9, AC-22; iAC10→AC-10, AC-21; iAC11→AC-11; iAC12→AC-12;
iAC13→AC-13; iAC14→AC-14, AC-18, AC-20; iAC15→AC-15; iAC16→AC-16, AC-19, AC-20; iAC17→AC-17.

---

## Risks & Mitigations

| Risk | Impact | Mitigation |
|---|---|---|
| VSTO silently ignores a `getPressed`/`onAction` signature mismatch | Toggles never bind; the bug persists undetected | AC-1/AC-2/AC-4 reflection pins fail the build on regression; AC-22 live check proves binding |
| Blocking wait sneaks into the synchronous `getPressed` path | Intermittent STA deadlock or menu-open freeze | AC-3 greps the diff for `.Result`/`.Wait()`/`GetAwaiter().GetResult()`; the coordinator's read is a dictionary lookup by construction |
| Stale pressed state after a toggle | Checkbox misrepresents configuration until the next poll | AC-9 pins the update-before-invalidate ordering as a recorded call sequence |
| Toggle routed through the readiness gate | A disabled engine could never be re-enabled | The two-guard-shape design is pinned in AC-10; the toggles stay outside `EngineCommandCatalog`, whose button-only XML test would also fail |
| The click boundary becomes a swallow-all | Real engine faults hidden | AC-7: only `HandleToggleClickAsync` observes; `ExecuteToggleAsync` propagates; no new broad catch below the boundary |
| Catalog/XML set-equality tests break mid-change | Red pipeline between the catalog and XML edits | AC-10 requires the catalog and XML edits to land atomically (R3/R4 are red until both are in) |
| A behavioral test reaches `MessageBox.Show` | vstest hangs | AC-20 forbids driving `NotifyEngineCommandNotReady`; behavioral assertions live at the coordinator seam with injected sinks |
| Scope creep into research §10 defects | Larger, riskier diff | AC-17 requires promotion, not in-scope fixes |
| Reviewer reads the missing `/p:Nullable=enable` as non-compliance | False blocking finding | The *Verification* section records the deviation with the #522 citation; AC-16 restates it |

---

## Rollout & Follow-up

- **Release/rollout:** ships with the next add-in build. No configuration change, no migration,
  no feature flag. Rollback is a straight revert.
- **Post-fix verification:** execute the AC-22 manual checklist against a live Outlook profile
  and record the outcome under `.../evidence/manual-verification/` before merge.
- **Follow-up work:** promote the research §10 defects (checking the tracker first — items 1-2
  may already have been promoted during #503); a generalized "every XML callback resolves with
  the documented signature" test ships with the orphan-callback fix, not here.
- **Links:** issues <https://github.com/drmoisan/TaskMaster/issues/505>,
  <https://github.com/drmoisan/TaskMaster/issues/506>,
  <https://github.com/drmoisan/TaskMaster/issues/518>; research
  `research/2026-08-08T19-30-ribbon-engine-toggle-state-guards-research.md`; branch
  `bug/ribbon-engine-toggle-state-guards-505`; predecessor delivery
  `docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/`.

---

## Correction Log

### 2026-08-08 — spec authoring

No conflict between `issue.md` and the research artifact was found during authoring; the
defect-surface table, the 11-total/10-unguarded count, the #507 property text, and the XML
element facts were re-verified against this worktree (`RibbonViewer.EngineCommands.cs` lines
105/120/123/126/129/132/189/192/195/198/201; `RibbonController.Intelligence.cs:204`;
`RibbonExplorer.xml:141, 520`). One process note: the issue-level AC16 text names the `CLAUDE.md`
toolchain generically; this spec binds the type-check step to CI's exact command and records the
#522 deviation explicitly so it cannot be misread as non-compliance.

---

## Delivery Notes and Deviations

Delivered 2026-08-08 on `bug/ribbon-engine-toggle-state-guards-505` from `origin/main` at
`f910ff2f21c67a03cf8eebcb340727d5415d8e08`. Twenty-two of the twenty-three acceptance criteria are
checked off above; **AC-22 remains `- [ ]` by design** (MANUAL-ONLY).

### What was delivered

- Two new host-neutral, non-exempt types under `TaskMaster/Ribbon/`:
  `EngineToggleCatalog` (engine key to toggle control id) and `EngineToggleStateCoordinator`
  (synchronous cached `getPressed`, at-most-one lazy prime per key, awaited toggle with
  update-before-invalidate ordering, observed-and-logged click boundary).
- Thin glue inside the existing `[ExcludeFromCodeCoverage]` types: `EngineToggles` plus
  `IsEngineToggleActive` / `HandleEngineToggleClickAsync` on `RibbonController`, and
  `InvalidateEngineToggle` on `RibbonViewer`.
- The four toggle/`getPressed` callbacks rewritten to the exact Office shapes; the six save/info
  command callbacks routed through `RunEngineCommandAsync` with the engine dereference deferred
  into the lambda; six new `EngineCommandCatalog` entries and six matching
  `getEnabled="EngineCommand_GetEnabled"` attributes in `RibbonExplorer.xml`, landed atomically.
- Regression tests R1-R5 written first and demonstrated red, then green; 22 new seam test members
  (25 executed cases) with the new coordinator at 0.991 line coverage and the catalog at 1.000.

### Deviations and notable outcomes

1. **Type-check command (issue #522).** The type-check gate uses CI's command
   (`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`)
   and deliberately omits the `CLAUDE.md` variant's `/p:Nullable=enable`, per the *Verification*
   section above. This is a documented deviation, not non-compliance. #522 was not fixed.
2. **Intended UX change (B7).** The six save/info buttons now render disabled until their engine
   loads, instead of being always enabled and silently no-oping. They re-enable automatically via
   the existing post-load refresh. The stale comment at `RibbonExplorerXmlTests.cs` was updated to
   describe the new per-control gating.
3. **No `Part2` test-file split.** `EngineToggleStateCoordinatorTests.cs` is 459 physical lines
   after formatting, under the 500-line cap, so the conditional split was not triggered and the
   test project gained three `<Compile Include>` entries rather than four.
4. **Prime-fault observation uses a continuation, not a second `catch`.** `ExecuteToggleAsync` and
   the prime path contain no `catch`; the prime's fault is observed by a `ContinueWith`
   continuation that reads `Task.Exception`. The file therefore holds exactly one `catch (`, inside
   `HandleToggleClickAsync`, as the boundary-catch rule requires.
5. **Coverage-exemption uncertainty resolved empirically.** The plan required the
   `[ExcludeFromCodeCoverage]` narrative to be stated as expected-but-unverified, because
   `coverage.config` supplies a custom `<CodeCoverage>` block that could displace the default
   `<Attributes>` excludes. It was probed directly: `RibbonViewer.cs` and `RibbonController.cs` are
   **absent** from the final Cobertura document, so the attribute is being honored. Recorded in the
   coverage-comparison artifact.
6. **Accepted pre-existing size overages.** `RibbonExplorer.xml` (539 to 545 lines) and
   `TaskMaster.csproj` (582 to 584 lines) exceed 500 lines. Both were already over at the
   merge-base and are declarative resource / MSBuild project files rather than production, test, or
   reusable-script code. Every `.cs` file in the diff is at or under 500 lines.
7. **Phase 5 was restarted once for an environmental cause.** A first final-QC pass aborted at the
   test gate on `QuickFiler.Test`'s `WinFormsPumpHost` message-pump tests, which fail under machine
   load with a WinForms handle-creation race. `QuickFiler` has no reference to `TaskMaster`, its
   binaries are compiled from merge-base source, and the same tests pass 4/4 in isolation once load
   is reduced. The cause was removed rather than worked around; no test was weakened and no
   `QuickFiler` source was touched. The known flakiness is already tracked as issue **#511**. The
   recorded final pass is uninterrupted, with an identical before/after tree fingerprint.
8. **One promotion deferred.** Research §10 item 2 (unguarded `Globals` dereferences in
   `RibbonController.Intelligence.cs`) has no existing tracker issue and requires promotion through
   the MCP lifecycle, which is outside the executing agent's tool set. The prepared potential-entry
   title and body are recorded under `evidence/issue-updates/` for the orchestrator. Item 1 is
   already promoted as **#504**; item 3 was resolved during authoring.

### Evidence index

| Kind | Location |
|---|---|
| Baseline (12 artifacts) | `evidence/baseline/` |
| Red-before / green-after (4 artifacts) | `evidence/regression-testing/` |
| QA gates (14 artifacts) | `evidence/qa-gates/` |
| Promotion dispositions | `evidence/issue-updates/research-defect-promotions.2026-08-08T21-43.md` |
| AC-22 manual checklist (PENDING) | `evidence/manual-verification/ac22-checklist.2026-08-08T21-44.md` |
| Phase notes and commits | `evidence/other/` |

Raw Cobertura documents and MSBuild logs are intentionally **not** committed; they live under the
gitignored `coverage/` directory and are regenerable from the recorded commands. Numeric headline
values are recorded in the Markdown artifacts.
