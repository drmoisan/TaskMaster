# disabled-stores-settings-ui - Feature Spec

- **Issue:** #265
- **Epic:** #260 (store-lockup-resilience)
- **Wave:** 2
- **Depends on:** F1 (#261), F2 (#262), F3 (#263)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-07
- **Status:** Draft
- **Version:** 0.1
- **Work Mode:** full-feature

## Overview

F5 delivers a persistent settings surface that lists the stores currently disabled by the
store-lockup-resilience epic and lets the user reenable any of them. Today the only reenable path
is the transient modeless notification delivered by F4 (#264); once dismissed, the user has no
place to view or manage disabled stores. The existing "TaskMaster -> Settings -> Folder Settings"
entry point opens `StoreWrapperController` + `StoreWrapperViewer`, a single-store detail editor
(a ComboBox plus labels) that has no list of stores and no enable/disable surface.

This feature adds a new, cohesive list-oriented dialog that reads the disabled set from F1's
service and offers a per-store Reenable action. It is a pure consumer of F1's public contract:

- `IApplicationGlobals.StoreDisable.GetDisabledStores()` returns
  `IReadOnlyCollection<DisabledStoreEntry>`, where each entry carries a store identity and a scope
  of `SessionOnly` or `FutureSessions`. F5 uses this to populate and refresh the list.
- `IApplicationGlobals.StoreDisable.ReenableAsync(StoreIdentity identity)` performs the reenable.
  F1 orchestrates F3's runtime rehook and its own state-clearing/persistence internally; F5 does
  not call F3 directly and does not persist anything itself.

The dialog is reached through a new, additive ribbon button placed in the existing Settings menu,
sibling to the existing Folder Settings button. The existing single-store editor is left intact.

## Intent & Outcomes

- Give the user a discoverable, persistent place to see which stores are disabled, independent of
  the transient F4 notification.
- Visually distinguish session-only disablement from future-sessions disablement so the user
  understands whether a store will return automatically next session.
- Provide a per-row Reenable action that routes through F1's `ReenableAsync` and reflects the
  resulting state without the UI drifting from the service's actual state.
- Keep all decision and orchestration logic in a testable controller behind an `IViewer` seam so
  the feature is unit-testable with Moq, without a live Outlook process, without a real
  `DataGridView`, and without temporary files.

## Scope

- Add a new sibling Controller + `IViewer` + Form trio in `UtilitiesCS/OutlookObjects/Store/`:
  - `DisabledStoresController` (testable),
  - `IDisabledStoresViewer` (interface-only),
  - `DisabledStoresViewer` + `DisabledStoresViewer.Designer.cs` (WinForms, exempt).
- Add a pure row view-model, `DisabledStoreRow`, projected from F1's `DisabledStoreEntry`.
- Add a new, additive ribbon button in the existing Settings menu
  (`TaskMaster/Ribbon/RibbonExplorer.xml`, Settings menu region, lines 228-437), with its callback
  in `RibbonViewer.cs` and dispatch in `RibbonController.cs`.
- Populate the list from `StoreDisable.GetDisabledStores()` on open and re-fetch after every
  Reenable action.
- Route the per-row Reenable action through `StoreDisable.ReenableAsync(identity)`.
- Reuse the existing dialog-open readiness gate (`StoreWrapperController.EvaluateLaunchReadiness`)
  so the new dialog and the single-store editor share one readiness behavior once F2 lands. The
  least-invasive way to avoid duplicating that logic is a behavior-preserving extraction of the
  readiness body into a shared internal helper that both controllers call; the extraction leaves
  `StoreWrapperController.EvaluateLaunchReadiness` semantically unchanged.

## Non-Scope

- No change to F1's service contract, persistence mechanism, or identity convention. F5 consumes
  the contract fixed by the epic manifest's Shared Design Alignment section.
- No direct call to F3's rehook interface. Reenable goes only through `ReenableAsync`.
- No change to `IStoreWrapperViewer`, `StoreWrapperViewer.cs`, or `StoreWrapperViewer.Designer.cs`
  beyond the behavior-preserving readiness-helper extraction described above. The single-store
  editor keeps its current behavior and its existing tests.
- No extension of the single-store editor to host the disabled-store list. See the surface
  decision below for the rationale.
- No new persistence mechanism, no new configuration key, and no new serialized artifact.
- No UI for the pre-existing exclusion lists; F5 owns the disabled-store list UI and its wiring
  only.
- No management of the disable action (that is owned by F1/F4); F5 only lists and reenables.

## Surface Decision and Rationale

**Decision:** Add a new sibling `DisabledStoresController` + `IDisabledStoresViewer` +
`DisabledStoresViewer`/`.Designer.cs` trio, reached through a new, additive ribbon button in the
existing Settings menu. Do not extend `StoreWrapperViewer`.

**Existing entry points (context).** Two ribbon buttons currently route to the same single-store
editor through `StoreWrapperController`:

- `FolderSettings` (Settings menu) -> `RibbonViewer.FolderSettings_Click` ->
  `RibbonController.FolderStoresSettings()` -> `new StoreWrapperController(Globals).Launch()`.
- `SpamFolderSettings` (labeled "Junk Folder Settings", Spam Bayes group) ->
  `RibbonViewer.SpamFolderSettings_Click` -> the same `FolderStoresSettings()` path.

Both open the identical single-store dialog; there is no store-scoped differentiation between them
in code today. F5 does not alter either of these paths.

**Rationale for a new sibling rather than extending the single-store editor:**

- Single responsibility. The single-store editor edits one `StoreWrapper` at a time
  (archive/junk folder assignment). The disabled-store list is a fleet-wide, many-store concern.
  Hosting a grid, a button column, and click handling for an unrelated concern inside the existing
  `IStoreWrapperViewer`/`StoreWrapperViewer`/Designer trio would couple two unrelated domain
  concepts in one class, contrary to the repository's design principles.
- Zero regression risk to a stable, already-tested surface. Extending the single-store editor
  would require touching `StoreWrapperViewer` and its Designer, risking the currently-passing
  `StoreWrapperController_Tests.*` and `StoreWrapperViewerTests` coverage for an unrelated feature.
- Correct UX placement. The `SpamFolderSettings` button is labeled "Junk Folder Settings"; hosting
  a fleet-wide disabled-store list under a spam-scoped label would be a UX mismatch. A dedicated
  "Disabled Stores" button reads clearly.
- Discoverability. A dedicated Settings-menu button gives the persistent surface the issue asks
  for, rather than burying the list inside an unrelated single-store form.

**Rationale for the additive ribbon button.** The Settings menu already hosts the Folder Settings
button. Adding one sibling `<button>` is a small, additive markup change with no behavioral risk to
existing buttons, and it keeps the new capability where users already look for store settings.

## Controller Design

`DisabledStoresController` owns the authoritative in-memory row list. The grid is bound to that
list for display, but all click-handling logic resolves the clicked row from the controller's own
list by index, never by reading back from a live grid control. This is the central testability
decision: `System.Windows.Forms.DataGridViewCellEventArgs` is constructible directly in a test
(`new DataGridViewCellEventArgs(columnIndex, rowIndex)`) with no live grid, no Form, and no STA
thread, so the entire click path is exercisable with Moq.

Illustrative shape (exact accessibility follows the `StoreWrapperController` precedent):

```csharp
public class DisabledStoresController
{
    internal IApplicationGlobals Globals { get; }
    public IDisabledStoresViewer Viewer { get; internal set; }
    internal List<DisabledStoreRow> Rows { get; set; } = new();

    public void Launch();                                 // readiness gate -> populate -> ShowDialog
    internal void PopulateRows();                          // GetDisabledStores() -> Rows -> Dgv.DataSource
    public void Dgv_CellContentClick(object sender, DataGridViewCellEventArgs e);
    internal Task ReenableAsync(DisabledStoreRow row);     // StoreDisable.ReenableAsync(identity) -> refresh
}
```

### List population from F1

- `PopulateRows()` calls `Globals.StoreDisable.GetDisabledStores()`, projects each
  `DisabledStoreEntry` into a `DisabledStoreRow`, assigns `Rows`, and binds the grid to that list
  (for example `Viewer.Dgv.DataSource = new BindingList<DisabledStoreRow>(Rows)`), so the grid is a
  direct projection of the service result with no manual row bookkeeping.
- `DisabledStoreRow` is a pure POCO with no WinForms dependency:
  - `Identity` — the F1 store identity used to call `ReenableAsync`.
  - `DisplayName` — the display text (may equal the identity's display name).
  - `ScopeLabel` — human-readable scope text, for example "Session Only" / "Future Sessions".
  - `IsFutureSession` — bool derived from the entry's `FutureSessions` scope; drives the visual
    distinction rendered by the Designer/cell-formatting layer.
- `Launch()` applies the shared readiness gate (same behavior as
  `StoreWrapperController.EvaluateLaunchReadiness`). If the model is not ready it shows the same
  "not available yet" warning as the single-store editor and leaves `Viewer` null; otherwise it
  constructs the viewer, calls `PopulateRows()`, and shows the dialog modally.

### Row-index resolution

- `Dgv_CellContentClick(sender, e)` returns early when `e.RowIndex < 0` (header/invalid) or when
  `e.ColumnIndex` is not the Reenable button column.
- Otherwise it resolves `var row = Rows[e.RowIndex]` from the controller's own list and invokes the
  reenable path with that row. No live grid read is required.

### Reenable then refresh

- The reenable path calls `Globals.StoreDisable.ReenableAsync(row.Identity)` inside a try/catch
  that mirrors the existing controller error pattern (log via log4net, surface a `MyBox` dialog on
  failure using the same `MyBox.DialogInvoker` seam already proven in the single-store tests).
- After the call, it unconditionally calls `PopulateRows()` again to re-fetch the true current
  state from `GetDisabledStores()`. Re-fetch-after-action, rather than manual row mutation, is the
  chosen ordering rule: the grid is always a direct projection of the latest service result, on
  open and after every Reenable click, so the displayed state cannot drift from the service — this
  holds whether the call succeeded or failed.
- Because `ReenableAsync` is `Task`-returning (per the epic manifest, the rehook involves readiness
  retry), the click handler follows the existing `Viewer.InvokeRequired` / `Viewer.Invoke(...)`
  marshaling convention used by `StoreWrapperController`'s folder-picker handlers before touching
  `Viewer` state from a continuation. To avoid duplicate in-flight invocations on the same row, the
  handler disables the Reenable action for the duration of the call and re-enables it via the
  post-call `PopulateRows()` refresh.

### Scope distinction

- Each row's `IsFutureSession` flag is independent per row; the controller makes no assumption that
  all rows share one scope. Both scopes may be present simultaneously.
- The controller sets `ScopeLabel` and `IsFutureSession` on each row. The visual differentiation
  (distinct cell/row style for future-sessions rows) is applied in the Designer/cell-formatting
  layer, which is WinForms-exempt code driven by the controller-supplied `IsFutureSession` flag.

## Dependency on F1, F2, and F3

- **F1 (#261) — direct consumer.** F5 calls only `StoreDisable.GetDisabledStores()` and
  `StoreDisable.ReenableAsync(identity)` on the `StoreDisable` member of `IApplicationGlobals`, and
  relies on F1's `DisplayName`-primary identity convention. F1 orchestrates F3's rehook plus its
  own state-clearing and persistence inside `ReenableAsync`; F5 does not duplicate any of that.
- **F2 (#262) — prerequisite for dialog-open reliability.** F2 fixes the root cause of the null
  store model so the settings dialogs open with a populated model. F5's dialog reuses the same
  readiness evaluation the single-store editor already applies; once F2 lands, both dialogs become
  reliably openable through one shared readiness function. The dependency on F2 is about
  dialog-open reliability and UX consistency across the Settings-menu family, not a data dependency
  of the disabled-store list itself (`GetDisabledStores()` operates on F1's independent
  disabled-store model).
- **F3 (#263) — indirect only.** F5 does not call F3's rehook interface. Reenable flows through
  `ReenableAsync`, which F1's implementation wires to F3 internally per the epic's Shared Design
  Alignment. F5 stays decoupled from F3's COM-hookup mechanics.

## Determinism and Testability

- All decision and orchestration logic lives in `DisabledStoresController` behind
  `IDisabledStoresViewer`. Controller unit tests mock `IApplicationGlobals` / the `StoreDisable`
  service and `IDisabledStoresViewer` with Moq, and never construct a live `DataGridView`, a live
  Form, or a live Outlook object.
- Row-index-based click resolution lets tests construct
  `new DataGridViewCellEventArgs(columnIndex, rowIndex)` directly and drive `Dgv_CellContentClick`
  with no STA thread, avoiding the STA-construction constraint that grid-bearing Forms otherwise
  impose in this repository's test runner.
- **Empty-list handling.** When `GetDisabledStores()` returns an empty collection, `PopulateRows()`
  binds an empty list; the dialog opens with headers only and no rows, with no special-case
  branch.
- **Failure handling.** When `ReenableAsync` throws or returns a faulted `Task`, the exception is
  caught, logged, and surfaced through the `MyBox` dialog seam; it does not escape the click
  handler. `PopulateRows()` still runs afterward, so the displayed state matches the service even
  when the reenable attempt failed.
- **Both scopes present.** Tests seed the mocked service with a mix of `SessionOnly` and
  `FutureSessions` entries and assert each row's `IsFutureSession` / `ScopeLabel` independently.
- **Determinism.** No wall-clock reads, no `Thread.Sleep` / `Task.Delay`, no real timers, and no
  temporary files in tests, per repository policy. Async paths are driven by completed/faulted
  `Task` results returned from the mocked service.
- **Test precedent.** Follow the existing Controller + `IViewer` seam and its tests under
  `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.*` (Moq for globals/viewer,
  FluentAssertions, MSTest, the `MyBox.DialogInvoker` seam, and the reflection-based
  `SetInternalProperty` / `GetInternalProperty` helpers for seeding/asserting `internal` members).
- **New-code coverage.** `DisabledStoresController.cs` and `DisabledStoreRow.cs` are testable and
  target the repository's new-code coverage requirement. `IDisabledStoresViewer.cs` is
  interface-only and legitimately reports 0% executable coverage.
- **WinForms exemption.** `DisabledStoresViewer.cs` and `DisabledStoresViewer.Designer.cs` are
  WinForms form-derived and Designer-generated code and fall under the repository's
  COM/VSTO/WinForms coverage exemption. The ribbon dispatch method inherits the class-level
  `[ExcludeFromCodeCoverage]` already present on `RibbonController`.

## File List

### New files — testable

| File | Purpose | Coverage |
|---|---|---|
| `UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs` | Controller: readiness gate reuse, list population from F1, row-index click resolution, ReenableAsync-then-refresh, failure handling | New-code target |
| `UtilitiesCS/OutlookObjects/Store/DisabledStoreRow.cs` | Pure row view-model (`Identity`, `DisplayName`, `ScopeLabel`, `IsFutureSession`) | New-code target |
| `UtilitiesCS/OutlookObjects/Store/IDisabledStoresViewer.cs` | Interface-only viewer seam; extends the existing `IForm` mirror, exposes `Dgv` and the `CellContentClick` forwarding signature | Interface-only (0% executable) |

### New files — WinForms-exempt

| File | Purpose |
|---|---|
| `UtilitiesCS/OutlookObjects/Store/DisabledStoresViewer.cs` | Thin partial `Form`; wires `Dgv.CellContentClick` in the constructor and forwards to the controller |
| `UtilitiesCS/OutlookObjects/Store/DisabledStoresViewer.Designer.cs` | Designer-generated `DataGridView`, text columns for display name and scope, `DataGridViewButtonColumn` for Reenable, and scope-based cell styling |

### Modified files — ribbon wiring (exempt)

| File | Change |
|---|---|
| `TaskMaster/Ribbon/RibbonExplorer.xml` | Add one additive `<button>` under the existing Settings menu (lines 228-437), e.g. `id="DisabledStoresSettings"`, `onAction="DisabledStoresSettings_Click"`, label "Disabled Stores" — markup only |
| `TaskMaster/Ribbon/RibbonViewer.cs` | Add `DisabledStoresSettings_Click(...)` forwarding to the controller dispatch, mirroring `FolderSettings_Click` |
| `TaskMaster/Ribbon/RibbonController.cs` | Add `internal void DisabledStoresSettings() { new DisabledStoresController(Globals).Launch(); }`, mirroring `FolderStoresSettings()`; inherits the class-level `[ExcludeFromCodeCoverage]` |

### Modified files — behavior-preserving refactor

| File | Change |
|---|---|
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` | Extract the `EvaluateLaunchReadiness()` body into a shared internal helper reused by both controllers; `EvaluateLaunchReadiness()` becomes a one-line delegation with unchanged inputs/outputs so existing `StoreWrapperController_Tests.Launch.cs` tests continue to pass unmodified |

## Acceptance Criteria

- [x] **AC1 — Dedicated surface via additive ribbon button.** A new "Disabled Stores" button in the
      existing Settings menu opens a new dialog backed by `DisabledStoresController` +
      `IDisabledStoresViewer`. The existing Folder Settings and Junk Folder Settings buttons and the
      single-store `StoreWrapperController`/`StoreWrapperViewer` editor are unchanged.
- [x] **AC2 — List reflects service state on open.** On open, the dialog shows one row per entry
      returned by `StoreDisable.GetDisabledStores()`, populated by `DisabledStoresController` from
      that call.
- [x] **AC3 — Scope is visually distinguished.** Session-only rows and future-sessions rows are
      distinguishable: the controller sets `ScopeLabel` and `IsFutureSession` per row, and the
      Designer/cell-formatting layer renders a distinct style for future-sessions rows. Both scopes
      may be present at once, each resolved independently.
- [x] **AC4 — Per-row Reenable routes through F1.** A Reenable action on a row invokes
      `StoreDisable.ReenableAsync(identity)` exactly once with that row's identity, resolved from the
      controller's own list by `DataGridViewCellEventArgs.RowIndex`. F5 does not call F3 directly and
      does not persist state itself.
- [x] **AC5 — Refresh after reenable.** After a Reenable action, the controller unconditionally
      re-fetches `GetDisabledStores()` and rebinds the list, so the displayed rows match the current
      service state after every action.
- [x] **AC6 — Empty list.** When `GetDisabledStores()` returns an empty collection, the dialog opens
      with no rows and no exception.
- [x] **AC7 — Reenable failure is surfaced without crashing.** When `ReenableAsync` throws or
      returns a faulted `Task`, the exception is caught, logged, and surfaced through the `MyBox`
      dialog seam; it does not escape the click handler, and the list is still refreshed from
      `GetDisabledStores()` afterward.
- [x] **AC8 — Controller + IViewer seam, Moq-testable, no live Outlook, no temp files.** All logic
      is unit-tested through `IDisabledStoresViewer` with Moq and a mocked `StoreDisable` service,
      driving clicks via a directly-constructed `DataGridViewCellEventArgs` with no live
      `DataGridView`, no live Outlook, and no temporary files.
- [x] **AC9 — Dialog-open readiness reuse (F2 dependency).** The dialog applies the same readiness
      gate as the single-store editor via a shared readiness helper; the extraction leaves
      `StoreWrapperController.EvaluateLaunchReadiness` behavior unchanged, and existing
      `StoreWrapperController_Tests.*` continue to pass unmodified.
- [x] **AC10 — Toolchain and coverage.** The full C# toolchain passes in order (CSharpier, .NET
      analyzers, nullable analysis with `TreatWarningsAsErrors`, MSTest with coverage);
      `DisabledStoresController.cs` and `DisabledStoreRow.cs` meet the new-code coverage target, and
      WinForms form-derived / Designer-generated files are handled under the repository
      COM/VSTO/WinForms coverage exemption.

## Acceptance Criteria — Evidence Mapping

All AC1-AC10 verified locally on branch `feature/disabled-stores-settings-ui-265`. Backing
evidence artifacts under `docs/features/active/2026-07-07-disabled-stores-settings-ui-265/evidence/`:

- AC1 — `evidence/other/non-interference-confirmation.md` (additive ribbon button; existing editor unchanged) + `evidence/regression-testing/controller-tests-pass.md`.
- AC2 — `evidence/regression-testing/controller-tests-pass.md` (PopulateRows_ProjectsServiceEntriesIntoRows).
- AC3 — `evidence/regression-testing/controller-tests-pass.md` (ScopeLabel/IsFutureSession per-row asserts) + Designer CellFormatting styling.
- AC4 — `evidence/regression-testing/controller-tests-pass.md` (Dgv_CellContentClick_OnReenableColumn_InvokesReenableWithRowIdentityOnce).
- AC5 — `evidence/regression-testing/controller-tests-pass.md` (ReenableAsync_OnSuccess_...RefetchesDisabledStores).
- AC6 — `evidence/regression-testing/controller-tests-pass.md` (PopulateRows_WhenServiceReturnsEmpty_...).
- AC7 — `evidence/regression-testing/controller-tests-pass.md` (ReenableAsync_WhenServiceThrows_SurfacesViaMyBox...).
- AC8 — `evidence/regression-testing/controller-tests-pass.md` (Moq/IViewer seam, DataGridViewCellEventArgs, no live grid/Outlook/temp files).
- AC9 — `evidence/regression-testing/readiness-extraction-behavior-preserving.md` (51/51 StoreWrapper tests pass) + `evidence/other/non-interference-confirmation.md`.
- AC10 — `evidence/qa-gates/qa-01-format.md`, `qa-02-analyzers.md`, `qa-03-nullable.md`, `qa-04-test-coverage.md`, `qa-05-coverage-delta.md`.

## Definition of Done

- [x] Acceptance criteria in this spec and in `user-story.md` are mapped to implementation tasks and
      verification evidence.
- [x] The new Controller + `IViewer` + Form trio and `DisabledStoreRow` exist and follow the
      existing single-store editor's structural conventions.
- [x] The list populates from `StoreDisable.GetDisabledStores()` on open and after every reenable.
- [x] Reenable routes only through `StoreDisable.ReenableAsync(identity)`; F3 is not called directly.
- [x] Empty-list, both-scopes, and reenable-failure behaviors are validated by deterministic tests
      with no live Outlook and no temporary files.
- [x] The behavior-preserving readiness-helper extraction leaves existing single-store editor tests
      passing unmodified.
- [x] Docs updated under `docs/features/active/2026-07-07-disabled-stores-settings-ui-265/`.
- [x] Full C# toolchain pass completed in order: CSharpier -> .NET analyzers ->
      nullable/`TreatWarningsAsErrors` -> MSTest with coverage.
