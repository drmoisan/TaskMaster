---
name: ribbon-toggle-state-guards-505
description: "#505/#506/#518 bundle research: toggle vs command guard asymmetry (config vs InboxEngines), EngineCommandCatalog<->XML test coupling forces atomic changes, MessageBox in NotifyEngineCommandNotReady blocks viewer-level tests"
metadata:
  type: project
---

Bundled bug research (#505 async getPressed, #506 fire-and-forget toggle, #518 ten unguarded `Controller.Engines.` sites), 2026-08-08. Recommended: new host-neutral `EngineToggleStateCoordinator` (last-known-state cache, lazy prime-on-read, update-cache-then-invalidate) for the 4 toggle/getPressed sites; route the 6 ShowDiskDialog/ShowSaveInfo sites through the existing #503 `RunEngineCommandAsync` gate by adding 6 button ids to `EngineCommandCatalog`.

**Why (non-obvious facts, expensive to rediscover):**

1. **Guard asymmetry is semantic, not stylistic.** `ToggleEngineAsync`/`EngineActiveAsync` operate on `Globals.AF.Manager.Configuration` (`AppItemEngines.cs:92-109`), while `InitAsync` filters `config.Value.Engine` before populating `InboxEngines` — so a readiness gate keyed on `InboxEngines` would permanently block re-enabling a disabled engine. `ShowSaveInfo`/`ShowDiskDialog` DO require the `InboxEngines` key (no-op otherwise), so the readiness gate is exactly right for them.
2. **`EngineCommandCatalog` membership is load-bearing in three `RibbonExplorerXmlTests`:** every catalog id must declare `getEnabled="EngineCommand_GetEnabled"` in the XML (set-EQUALITY both ways) and must be a `button` element. Adding catalog ids forces XML edits in the same task; `checkBox` ids (the two enable toggles) can never be added.
3. **`NotifyEngineCommandNotReady` calls `MessageBox.Show`** (`RibbonController.EngineCommands.cs:100`), so any unit test that drives a gate-closed path through viewer/controller glue hangs vstest. Behavioral tests must sit at the seam with injected sinks; viewer-level red tests are limited to reflection pins and the getPressed no-throw repro.
4. **Signature pins must compare parameter types by `Type.FullName == "Microsoft.Office.Core.IRibbonControl"`** — TaskMaster.Test has no Office PIA compile reference (`RibbonExplorerXmlTests.cs:280-287`).
5. `AsyncLazy<T>` (`UtilitiesCS/ReusableTypeClasses/AsyncLazy/AsyncLazy.cs`) has NO non-triggering completed-value probe, so "read config synchronously when materialized" is not implementable without new `IAppItemEngines` surface (which bodies in the excluded `AppItemEngines` — the net481 no-DIM trap from [[ribbon-engine-readiness-503]]).
6. XML control ids diverge from method names: the "current location" buttons are `GetSaveState`/`TriageGetSaveState`, not `*SaveLocation*`; toggles are `SpamBayesEnabledToggle`/`TriageEnabledToggle` (checkBox). `SpamBayes.GroupName == "Spam"` (SpamBayes.cs:328).

**How to apply:** for any future ribbon guard work, first ask whether the operation needs the engine *instance* (InboxEngines-keyed gate) or the engine *configuration* (availability-only guard + cached read); and check whether the proposed test path can reach a MessageBox before writing viewer-level tests.

Research artifact: `docs/features/active/2026-08-08-ribbon-engine-toggle-state-guards-505/research/2026-08-08T19-30-ribbon-engine-toggle-state-guards-research.md`
