# P6-T1 — Promotion Dispositions for the Research §10 Defects (AC-17)

Timestamp: 2026-08-08T21-43

Scope discipline: none of the items below was fixed inside this delivery. Plan rule 17 and AC-17
require promotion, not in-scope repair.

---

## Item 1 — Five orphan `onAction` callbacks in `RibbonExplorer.xml`

`BtnMigrateIDs_Click`, plus the `_Clicked`-vs-`_Click` suffix mismatches
`MoveEntireConversation_Clicked` (xml line 265), `SaveAttachments_Clicked` (271),
`SaveEmailCopy_Clicked` (277), `SavePictures_Clicked` (283) — the `RibbonViewer` methods are
`*_Click`, so all four QuickFiler settings checkboxes are inert.

**Disposition: ALREADY PROMOTED — issue #504. Recorded, not re-promoted.**

Tracker verification:

```
$ gh issue view 504 --json number,title,state,url
{"number":504,"state":"OPEN","title":"Bug: ribbon-dead-callback-names","url":"https://github.com/drmoisan/TaskMaster/issues/504"}
```

This matches the plan's section 6 record that item 1 was promoted during #503 (recorded in
`docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/plan.2026-08-08T11-59.md`
section 6). No action taken.

---

## Item 2 — Unguarded `Globals` dereferences in `RibbonController.Intelligence.cs`

Reachable from ribbon callbacks before `SetGlobals` has assigned `Globals`; for example
`ClearSpamManagerAsync` (`Globals.AF...` at line 220, `Globals.Engines.RestartEngineAsync` at line
230) and the QuickFiler-settings toggles (lines 29-58). Same defect class as #518 but outside the
ten enumerated sites, so deliberately excluded from this delivery's scope lock — indeed
`RibbonController.Intelligence.cs` is a section 4.4 protected zero-line-diff path.

### Tracker search evidence (read-only `gh`, as the task permits)

```
$ gh issue list --search "RibbonController.Intelligence Globals dereference" --state all --limit 20 --json number,title,state
[]

$ gh issue list --search "Intelligence.cs" --state all --limit 10 --json number,title,state
[]

$ gh issue list --search "unguarded Globals SetGlobals ribbon" --state all --limit 20 --json number,title,state
[{"number":518,"state":"OPEN","title":"Bug: Bug: ribbon-engines-callers-unguarded-null-deref"},
 {"number":507,"state":"CLOSED","title":"Bug: ribbon-controller-engines-null-unsafe"}]
```

The only hits are #518 (closed by this delivery; scoped to the ten `Controller.Engines.<member>`
sites in `RibbonViewer.EngineCommands.cs`, not to `Globals` dereferences inside
`RibbonController.Intelligence.cs`) and #507 (closed; the `Engines` property itself). **No existing
issue covers item 2.** Promotion is therefore required.

### Disposition: PROMOTION DEFERRED TO ORCHESTRATOR

Promotion must go through the promotion lifecycle, in this order:

1. `mcp__drm-copilot__new_potential_entry`
2. `mcp__drm-copilot__potential_to_issue`
3. `mcp__drm-copilot__new_active_feature_folder`

Those three MCP tools are **not in the executing agent's tool set**, and `gh issue create` /
`gh issue new` / `gh api -X POST` are blocked by `.claude/hooks/enforce-promotion-mcp-only.ps1`. A
manually created issue would produce no promotion receipt, so none was attempted. The prepared
potential-entry content is handed back to the orchestrator below.

**Prepared title**

```
Bug: ribbon-controller-intelligence-unguarded-globals-deref
```

**Prepared body**

```markdown
# ribbon-controller-intelligence-unguarded-globals-deref (Bug)

- Date captured: 2026-08-08
- Source: research §10 item 2 of
  docs/features/active/2026-08-08-ribbon-engine-toggle-state-guards-505/research/2026-08-08T19-30-ribbon-engine-toggle-state-guards-research.md
- Related: #507 (merged), #518 (closed by #505 delivery)

## Summary

`TaskMaster/Ribbon/RibbonController.Intelligence.cs` contains ribbon-callback-reachable code paths
that dereference `Globals` without a guard. Before `SetGlobals` has run, `Globals` is unassigned,
so each site raises `NullReferenceException` out of an `async void` Office handler, where it is
neither reported nor observable by the user.

This is the same defect class as #518, but at different call sites. #518 was scoped to the ten
`Controller.Engines.<member>` sites in `RibbonViewer.EngineCommands.cs`; these sites are in the
controller partial and were explicitly held out of that scope (that file is a protected
zero-line-diff path in the #505/#506/#518 delivery).

## Known sites (verified against origin/main at f910ff2f)

| Line | Member | Expression |
|---|---|---|
| 220 | `ClearSpamManagerAsync` | `Globals.AF...` |
| 230 | `ClearSpamManagerAsync` | `Globals.Engines.RestartEngineAsync(...)` |
| 29-58 | QuickFiler-settings toggle callbacks | `Globals...` |

The list is indicative rather than exhaustive; the fix should begin with a full enumeration of
`Globals` dereferences in that file that are reachable from a ribbon callback.

## Expected behavior

No ribbon callback raises `NullReferenceException` when invoked before initialization completes.
Each site degrades gracefully, consistent with the seam pattern established by #503 and extended by
#505: host-neutral, unit-tested decision logic behind an injected accessor, with the COM-touching
glue left in the `[ExcludeFromCodeCoverage]` shim.

## Suggested approach

Reuse the existing seams rather than adding ad-hoc `?.` operators, which the maintainer
disrecommended on #518: `EngineReadinessGate` / `EngineGatedCommandRunner` /
`EngineCommandCatalog` for readiness-gated commands, and `EngineToggleStateCoordinator` /
`EngineToggleCatalog` for configuration-backed toggles.

## Impact / Severity

Low. Same narrow reachable window as #507 and #518: the callback must run before `SetGlobals`.
```

---

## Item 3 — `spec.md` in this feature folder was an unfilled template

**Disposition: RESOLVED DURING AUTHORING — no action.** `spec.md` was populated before planning and
is the authoritative AC source (AC-1 through AC-23) for this `full-bug` delivery. Nothing to
promote.

---

## Additional out-of-scope observation found during execution

`QuickFiler.Controllers.Tests.QfcItemController_InitializationTests` /
`QfcItemController_CreationTests` — the `WinFormsPumpHost` message-pump test family — fail under
machine load with
`Invoke or BeginInvoke cannot be called on a control until the window handle has been created` and
with 60-second `[Timeout]` expiries. Encountered at Phase 5 and fully diagnosed in
`<FEATURE>\evidence\other\phase5-attempt1-aborted.2026-08-08T21-30.md`.

**Disposition: ALREADY TRACKED — issue #511. Recorded, not re-promoted.**

```
$ gh issue list --search "WinFormsPumpHost" --state all --limit 10 --json number,title,state
[{"number":511,"state":"OPEN","title":"Bug: winformspumphost-tests-load-flaky-visible-window"}]
```

No `QuickFiler` source was modified and no test was weakened, per plan rule 17.

---

## Summary for the orchestrator

| Item | Disposition | Action needed from the orchestrator |
|---|---|---|
| 1 — orphan `onAction` callbacks | Already promoted: **#504** (OPEN) | None |
| 2 — `RibbonController.Intelligence.cs` unguarded `Globals` | **PROMOTION DEFERRED TO ORCHESTRATOR** | Run the three MCP promotion calls with the prepared title and body above |
| 3 — `spec.md` template gap | Resolved during authoring | None |
| Extra — `WinFormsPumpHost` load flakiness | Already tracked: **#511** (OPEN) | None |
