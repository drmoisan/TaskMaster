# ribbon-engine-toggle-state-guards (Bug)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Active
- Issue: #505
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/505
- Also closes: #506, #518
- Issue URL (#506): https://github.com/drmoisan/TaskMaster/issues/506
- Issue URL (#518): https://github.com/drmoisan/TaskMaster/issues/518
- Work Mode: full-bug
- Branch: bug/ribbon-engine-toggle-state-guards-505

> Bundling note: #505, #506, and #518 are delivered as a single unit of work. All three land in
> `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs`, and #518's call sites overlap the exact
> methods #505 and #506 rewrite. They are also causally coupled: a synchronous `bool GetPressed`
> (#505) requires cached state that the awaited toggle (#506) refreshes and invalidates, and both
> must respect the null-guard contract (#518). The maintainer's #518 comment explicitly recommends
> addressing all three together.

## Summary

Three defects in the Spam Config and Triage Config ribbon submenu callbacks:

1. **#505 — invalid `getPressed` signature.** `SpamBayesEnabled_GetPressed` and
   `TriageEnabled_GetPressed` are declared `async Task<bool>`, but Office's `getPressed` callback
   contract requires a synchronous `bool GetPressed(Office.IRibbonControl control)`. Office cannot
   bind a `Task<bool>`-returning callback, so the two toggle buttons never reflect real engine
   activation state. The failure is silent.
2. **#506 — fire-and-forget toggle.** `SpamBayesEnabled_Click` and `TriageEnabled_Click` are `void`
   methods whose body is an unawaited `Controller.Engines.ToggleEngineAsync(...)`. The returned
   `Task` is discarded, so the toggle has no ordering guarantee and any exception is swallowed into
   an unobserved task. The sibling handlers in the same regions (`SpamSaveNetwork_Click`,
   `SpamSaveLocal_Click`, `TriageSaveNetwork_Click`, `TriageSaveLocal_Click`) are correctly written
   as `async void` with `await`, so these two are an inconsistency rather than a deliberate pattern.
3. **#518 — unguarded `Engines` dereference.** Ten production call sites dereference
   `Controller.Engines.<member>` with no null guard. #507 (merged, PR #519) changed the property to
   `Globals?.Engines`, so it now returns `null` instead of throwing; that relocates the
   `NullReferenceException` to the call site rather than eliminating it.

## Environment

- OS/version: Windows 11, Outlook desktop (VSTO add-in host)
- Runtime: .NET Framework 4.8.1, TaskMaster VSTO add-in
- Command/flags used: Outlook Explorer ribbon, Spam Manager and Triage configuration menus
- Data source or fixture: `TaskMaster/Ribbon/RibbonExplorer.xml` embedded resource; live Outlook profile

## Steps to Reproduce

1. Open Outlook with the TaskMaster add-in loaded and let initialization complete.
2. Open the Spam Manager save-options menu and observe the "SpamBayes Enabled" toggle button — its
   pressed state is not driven by `EngineActiveAsync` because the callback never binds (#505).
3. Click the toggle and observe that the handler returns before `ToggleEngineAsync` has awaited
   `Globals.AF.Manager.Configuration` and flipped `ClassifierActivated`; induce a failure inside the
   configuration load and observe that no error is surfaced (#506).
4. Reload the add-in so the ribbon is constructed before the controller's `Globals` is assigned, then
   invoke any of the ten listed callbacks and observe a `NullReferenceException` raised at the call
   site rather than inside `get_Engines()` (#518).
5. Repeat steps 2-3 for the "Triage Enabled" toggle button.

## Expected Behavior

- Both `*_GetPressed` callbacks expose the exact Office-required signature
  `public bool <Name>(Office.IRibbonControl control)` so Office binds them, and they return the real
  engine activation state without blocking the STA.
- Both `*_Click` handlers observe the toggle's completion and its exceptions: the state change is
  either complete when the handler returns, or the handler is `async void` with an explicit `await`
  and a boundary `try`/`catch` that reports failure through the project logging pattern.
- After a toggle completes, the corresponding ribbon control is invalidated so Office re-queries
  `getPressed` and the button reflects the new state.
- Every call site that reaches the engines degrades gracefully when the engines are not yet
  available, rather than dereferencing `null`.

## Actual Behavior

On `main` at `f910ff2f`, in `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs`:

```csharp
public void SpamBayesEnabled_Click(Office.IRibbonControl control, bool pressed) =>
    Controller.Engines.ToggleEngineAsync(SpamBayes.GroupName);          // line 119-120

public async Task<bool> SpamBayesEnabled_GetPressed(Office.IRibbonControl control) =>
    await Controller.Engines.EngineActiveAsync(SpamBayes.GroupName);    // line 122-123

public void TriageEnabled_Click(Office.IRibbonControl control, bool pressed) =>
    Controller.Engines.ToggleEngineAsync("Triage");                     // line 188-189

public async Task<bool> TriageEnabled_GetPressed(Office.IRibbonControl control) =>
    await Controller.Engines.EngineActiveAsync("Triage");               // line 191-192
```

The file contains 11 `Engines.` references in total. `TestSpam_Click` (line 105) is already gated —
its dereference sits inside a lambda passed to `Controller.RunEngineCommandAsync(...)`, which
`EngineGatedCommandRunner` evaluates only after `EngineReadinessGate` reports ready. That leaves
**10 unguarded sites**, independently verified against `origin/main` at `f910ff2f`:

| Line | Callback | Expression |
|---|---|---|
| 120 | `SpamBayesEnabled_Click` | `Controller.Engines.ToggleEngineAsync(SpamBayes.GroupName)` |
| 123 | `SpamBayesEnabled_GetPressed` | `Controller.Engines.EngineActiveAsync(SpamBayes.GroupName)` |
| 126 | `SpamSaveNetwork_Click` | `Controller.Engines.ShowDiskDialog(SpamBayes.GroupName, false)` |
| 129 | `SpamSaveLocal_Click` | `Controller.Engines.ShowDiskDialog(SpamBayes.GroupName, true)` |
| 132 | `GetSaveLocation_Click` | `Controller.Engines.ShowSaveInfo(SpamBayes.GroupName)` |
| 189 | `TriageEnabled_Click` | `Controller.Engines.ToggleEngineAsync("Triage")` |
| 192 | `TriageEnabled_GetPressed` | `Controller.Engines.EngineActiveAsync("Triage")` |
| 195 | `TriageSaveNetwork_Click` | `Controller.Engines.ShowDiskDialog("Triage", false)` |
| 198 | `TriageSaveLocal_Click` | `Controller.Engines.ShowDiskDialog("Triage", true)` |
| 201 | `TriageGetSaveLocation_Click` | `Controller.Engines.ShowSaveInfo("Triage")` |

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet: see the source excerpts above. Office does not surface a user-visible error for a
  signature-incompatible callback; the failure is silent unless Outlook runs with the "Show add-in
  user interface errors" developer option enabled. The fire-and-forget toggle produces no log entry
  on failure, which is the #506 defect.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Highest of the three (#505, Medium). Two configuration toggles display a state that is not tied to
the underlying setting, misrepresenting engine configuration to the user. The commands themselves
still work, so this is primarily a state-display and error-observability defect rather than a
functional break. #506 and #518 are each Low on their own: #506's happy path usually works because
the configuration task is typically complete by the time a user reaches the menu, and #518 has the
same narrow reachable window as #507 (the callback must run before `SetGlobals`).

## Suspected Cause / Notes

- The four #505/#506 methods were relocated verbatim from `RibbonViewer.cs` into
  `RibbonViewer.EngineCommands.cs` by #503 (PR #515); the defects were carried forward unchanged.
- #503 already established the host-neutral testable-seam mechanism used for exactly this class of
  problem: `EngineReadinessGate`, `EngineGatedCommandRunner`, `EngineCommandCatalog`, and
  `EngineCommandRefreshPlanner`, with `RibbonController.RunEngineCommandAsync` /
  `IsEngineCommandEnabled` as the thin glue. The #518 comment recommends routing the ten sites
  through that existing gate rather than adding ten ad-hoc null checks.
- `IAppItemEngines.EngineActiveAsync` and `ToggleEngineAsync` both `await Globals.AF.Manager.Configuration`
  before reading or mutating `loader.Config.ClassifierActivated`, which is why a synchronous
  `getPressed` needs cached state rather than a blocking wait.
- `RibbonViewer` and `RibbonController` both carry `[ExcludeFromCodeCoverage]` under the ratified
  VSTO/COM ribbon-handler exemption. Any logic extracted out of the handlers is NOT exempt and must
  be unit-tested.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: the extracted host-neutral toggle-state cache (synchronous read, async
      refresh, invalidation-on-toggle, null-engines degradation), plus reflection-pinned callback
      signatures in the existing `RibbonExplorerXmlTests` style.
- [ ] Integration scenario to retest: Spam Config and Triage Config submenus in a live Outlook
      profile — toggle state renders correctly, survives a reopen, and a pre-`SetGlobals` invocation
      does not throw.
- [ ] Manual verification notes: run Outlook with "Show add-in user interface errors" enabled to
      confirm no callback-binding error is reported.

## Next Step

- [x] Promote to GitHub issue (bug-report template) — #505, #506, #518 already promoted
- [x] Move to active fix folder / branch — `bug/ribbon-engine-toggle-state-guards-505`

## Acceptance Criteria

Authoritative acceptance criteria for this `full-bug` work mode live in `spec.md`. The criteria
below are the issue-level restatement used to confirm all three GitHub issues are closed.

### From #505 (async getPressed signature)

- [x] AC1 — `SpamBayesEnabled_GetPressed` is declared `public bool SpamBayesEnabled_GetPressed(Office.IRibbonControl control)`; no `async`, no `Task<bool>`.
- [x] AC2 — `TriageEnabled_GetPressed` is declared `public bool TriageEnabled_GetPressed(Office.IRibbonControl control)`; no `async`, no `Task<bool>`.
- [x] AC3 — Both callbacks return the real engine activation state derived from `IAppItemEngines.EngineActiveAsync`, without blocking the STA on an async call.
- [x] AC4 — A test pins both signatures by reflection so a future regression fails the build rather than failing silently in Office.

### From #506 (fire-and-forget toggle)

- [x] AC5 — `SpamBayesEnabled_Click` observes the completion of `ToggleEngineAsync` rather than discarding the returned `Task`.
- [x] AC6 — `TriageEnabled_Click` observes the completion of `ToggleEngineAsync` rather than discarding the returned `Task`.
- [x] AC7 — A fault raised inside the toggle is observed and reported through the project logging pattern rather than being swallowed into an unobserved task.
- [x] AC8 — Both handlers match the `async void` + `await` + boundary `try`/`catch` shape already used by the sibling `*SaveNetwork_Click` / `*SaveLocal_Click` handlers in the same regions.
- [x] AC9 — After a toggle completes, the corresponding ribbon control is invalidated so Office re-queries `getPressed`.

### From #518 (unguarded Engines dereference)

- [x] AC10 — All 10 unguarded `Controller.Engines.<member>` call sites listed in the Actual Behavior table are guarded and degrade gracefully when the engines are unavailable.
- [x] AC11 — The already-gated `TestSpam_Click` site is left functionally unchanged; the guarded-site count is verified as exactly 10 and reported.
- [x] AC12 — No call site raises a `NullReferenceException` when invoked before `SetGlobals` has assigned `Globals`.
- [x] AC13 — `RibbonController.Engines` remains `Globals?.Engines` (the #507 fix); the `?.` is not reverted.

### Cross-cutting

- [x] AC14 — Logic extracted out of the `[ExcludeFromCodeCoverage]` ribbon handlers is host-neutral and unit-tested; the exemption is neither removed nor widened to manufacture coverage.
- [x] AC15 — Failing regression tests are written first and demonstrated red before the fix, per the `CLAUDE.md` bugfix workflow.
- [x] AC16 — The full C# toolchain passes in a single final pass: `csharpier .` -> analyzer msbuild -> type-check msbuild (CI's command, without the defective `/p:Nullable=enable`, per #522) -> `vstest.console.exe /EnableCodeCoverage`.
- [x] AC17 — Scope is held to these three issues; any further defect found is promoted to its own issue rather than fixed here.

## Delivery Note (2026-08-08)

Delivered on `bug/ribbon-engine-toggle-state-guards-505` from `origin/main` at `f910ff2f`. All
seventeen issue-level criteria above are checked off against verified evidence in
`docs/features/active/2026-08-08-ribbon-engine-toggle-state-guards-505/evidence/`. The authoritative
criteria for this `full-bug` delivery are AC-1 through AC-23 in `spec.md`; the mapping from these
items to those criteria is the issue-tag coverage map at the end of the `spec.md` acceptance
section.

Three points a reviewer should read before treating anything here as complete:

1. **AC9 is checked off on automated evidence only.** It maps to spec **AC-9** (the recorded
   update-before-invalidate call sequence, verified by
   `ExecuteToggleAsync_PerformsToggleThenRefreshThenCacheThenInvalidate_InOrder`) **and** to spec
   **AC-22**, which is MANUAL-ONLY. **AC-22 is still `- [ ]` in `spec.md` and is pending maintainer
   execution** of `evidence/manual-verification/ac22-checklist.2026-08-08T21-44.md`. Live-Outlook
   confirmation that the corrected `getPressed` actually binds, that each toggle survives a menu
   reopen, and that the ten callbacks are safe pre-`SetGlobals` has **not** been performed. AC9's
   check-off above records only what unit tests can establish.
2. **The type-check gate deliberately omits `/p:Nullable=enable`** (AC16), using CI's actual
   command instead, per issue **#522**. This is a documented deviation recorded in the
   `## Verification` section of `spec.md`, not non-compliance.
3. **All promotions are complete** (AC17). Research §10 item 1 is already tracked as **#504**;
   item 3 was resolved during spec authoring; item 2 — unguarded `Globals` dereferences in
   `RibbonController.Intelligence.cs` — was promoted by the orchestrator as **#524** on
   2026-08-09T00-20 through `new_potential_bug_entry` -> `potential_to_issue` (bug, full-bug), after
   an independent tracker re-check confirmed no existing issue covered it. The receipt is recorded
   in `evidence/issue-updates/research-defect-promotions.2026-08-08T21-43.md`. The `WinFormsPumpHost`
   load-flakiness observed during Phase 5 is already tracked as **#511**.
4. **One non-blocking Major finding from code review is promoted, not fixed here** (CR-1, recorded
   in `code-review.2026-08-08T21-59.md`). `EngineToggleStateCoordinator.ApplyPrimeAsync` can let an
   in-flight prime overwrite a fresher toggle-written cache value, leaving a stale toggle display
   until the next click. It is display-only, self-correcting on the next click, strictly better than
   the merge-base behavior (in which the toggles never reflected engine state at all), and violates
   no acceptance criterion. Promoted as **#525** rather than widened into this delivery.

Intended user-visible change to call out at review: the six Spam/Triage save-options buttons
(**Network**, **Local**, **Current Location**) now render **disabled** until their engine finishes
loading, instead of being always enabled and silently doing nothing, and re-enable automatically
after the post-load refresh.
