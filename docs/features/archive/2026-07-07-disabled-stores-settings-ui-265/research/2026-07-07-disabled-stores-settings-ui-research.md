# F5 — Disabled Stores Settings UI (Issue #265) — Implementation Research

- Epic: #260 (store-lockup-resilience)
- Depends on: F1 (#261, `IStoreDisableService`), F2 (#262, Folder Settings null-model fix), F3 (#263, runtime rehook)
- Research date: 2026-07-07

## 1. Current State Analysis

### 1.1 Entry points into the existing settings dialog

Two ribbon buttons route to the same single-store editor:

- `TaskMaster\Ribbon\RibbonExplorer.xml:235-240` — `FolderSettings` button (Settings menu, TaskMaster group), `onAction="FolderSettings_Click"`.
- `TaskMaster\Ribbon\RibbonExplorer.xml:132-137` — `SpamFolderSettings` button (labeled "Junk Folder Settings", Spam Bayes group), `onAction="SpamFolderSettings_Click"`.
- `TaskMaster\Ribbon\RibbonViewer.cs:180-181` (`FolderSettings_Click`) and `:267-268` (`SpamFolderSettings_Click`) both forward to `_controller.FolderStoresSettings()` / `Controller.FolderStoresSettings()`.
- `TaskMaster\Ribbon\RibbonController.cs:259-263`:
  ```csharp
  internal void FolderStoresSettings()
  {
      var wrapper = new StoreWrapperController(Globals);
      wrapper.Launch();
  }
  ```
  `RibbonController` carries a class-level `[ExcludeFromCodeCoverage]` (`TaskMaster\Ribbon\RibbonController.cs:36-37`), so any new method added to it is exempt under the repo's VSTO-lifecycle/ribbon-handler coverage exemption without a separate attribute.

Both entry points open the identical dialog; there is no store-scoped differentiation between "Folder Settings" and "Junk Folder Settings" in code today.

### 1.2 `StoreWrapperController` / `StoreWrapperViewer` (single-store editor)

`UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs` is a Controller in a Controller+`IViewer` pair:

- `EvaluateLaunchReadiness()` (:108-125) is a **pure, internal, side-effect-free** read of `Globals?.Ol?.StoresWrapper`. It returns a `StoreLaunchReadiness` struct with `.State` (`Ready` / `ModelUnavailable` / `StoresUnavailable`), `.Model`, and `.DisplayNames`. This is the exact seam F2 fixes at the root (making `Globals.Ol.StoresWrapper` reliably non-null); F5 does not need to change this method, only decide whether/how to reuse it.
- `Launch()` (:130-150, `[ExcludeFromCodeCoverage]`) gates on that readiness result, shows a `MyBox.ShowDialog(...)` warning and returns early (leaving `Viewer` null) if not ready, otherwise constructs `Viewer = new StoreWrapperViewer(this)`, binds `Viewer.DisplayName.DataSource = readiness.DisplayNames`, and calls `Viewer.ShowDialog()` (modal).
- Remaining methods (`ButtonOk_Click`, `DisplayName_SelectedValueChanged`, `ArchiveFS_Click`, etc.) edit **one** `StoreWrapper` at a time (archive/junk folder assignment) and persist via `Model.Serialize()` (:299-307, `SaveChanges()`).
- UI-thread marshaling convention: click handlers that touch Outlook check `Viewer.InvokeRequired` and re-dispatch via `Viewer.Invoke(...)` before proceeding (e.g. `ArchiveFS_Click`, `ArchiveOutlook_Click`, `JunkEmail_Click`, `JunkPotential_Click`).

`IStoreWrapperViewer` (`UtilitiesCS\OutlookObjects\Store\IStoreWrapperViewer.cs`) extends `UtilitiesCS.Interfaces.IWinForm.IForm` (a hand-written mirror of `System.Windows.Forms.Form`'s public surface, used purely to make `Form.ShowDialog()`/`Close()`/`Invoke()` mockable) and adds concrete WinForms control properties directly (`ComboBox DisplayName`, `Label ArchiveFS`, `Button ButtonOk`, ...) plus forwarding method signatures. `StoreWrapperViewer.cs` is a thin partial `Form` that wires its own WinForms events to itself, then forwards to `Controller`. This is the "Make testable" region convention (:67-120 in `StoreWrapperViewer.cs`): every control the controller touches is exposed as a settable property so tests can substitute values without a live designer surface.

Testing precedent (`UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperController_Tests.Launch.cs`, `StoreWrapperViewerTests.cs`):
- Controller-level tests mock `IApplicationGlobals`/`IOlObjects` with Moq and never touch WinForms at all (`Launch_WhenStoresWrapperIsNull_...`, `EvaluateLaunchReadiness_...`).
- Some tests mock `IStoreWrapperViewer` fully via Moq (e.g. `dispatchedViewer = new Mock<IStoreWrapperViewer>()`, then `dispatchedViewer.Verify(x => x.Close(), Times.Once)`), reflecting internal state via `SetInternalProperty`/`GetInternalProperty` reflection helpers (controller's `Viewer`, `Model`, `Current` properties are `internal set`).
- Other tests (`StoreWrapperViewerTests.cs`) construct the **real** `StoreWrapperViewer` Form directly (`new StoreWrapperViewer(controller)`), without any explicit STA thread wrapper, and it works — `Label`/`ComboBox`/`Button` construction and property access apparently does not force Win32 handle creation in this test runner.

### 1.3 `DgvForm` (reusable grid shell) and its STA constraint

`UtilitiesCS\HelperClasses\DvgForm.cs` / `.Designer.cs` define `DgvForm : Form` — a thin shell exposing a single docked `internal DataGridView Dgv`. It has no Controller/`IViewer` pair today; its only production consumer is `UtilitiesCS\Extensions\DfMLNet.cs` / `PrettyPrint.cs`, which binds a `DataTable` directly to `Dgv.DataSource` for ad-hoc dataframe pretty-printing — not a settings-dialog pattern, and not MVC-structured.

Critically, `UtilitiesCS.Test\HelperClasses\DvgForm_Tests.cs` needed to construct `DgvForm` on **an explicit STA thread** (`new Thread(...) { ApartmentState = STA }`) even just to invoke its private resize handler — unlike `StoreWrapperViewerTests`, which constructs a Form with `ComboBox`/`Label`/`Button` controls with no STA wrapper. `UtilitiesCS.Test\HelperClasses\TipsController_TableLayoutPanel_Tests.cs` shows the same STA-thread pattern for `TableLayoutPanel`-bearing forms. `UtilitiesCS.Test\test.runsettings` confirms global STA execution is intentionally disabled repo-wide ("Tests that require an STA apartment must opt in ... with MSTest's STATestMethod or STATestClass attributes" — in practice this repo opts in via manual `Thread`/`ApartmentState.STA` wrapping rather than the MSTest attributes). **Implication for F5:** any test that constructs a real `DataGridView`-bearing Form needs the same manual STA wrapping; controller-level tests should avoid this entirely by mocking the viewer interface (see §6).

### 1.4 `IApplicationGlobals` and the (not-yet-existing) F1 service

`UtilitiesCS\Interfaces\IGlobals\IApplicationGlobals.cs` currently exposes `FS`, `Ol`, `TD`, `AF`, `Events`, `QfSettings`, `Engines`, `IntelRes`. F1 (#261, `docs\features\active\2026-07-07-store-disable-service-261\spec.md`) adds `IStoreDisableService` with `DisableSessionOnly(identity)`, `DisableForFutureSessions(identity)`, `Reenable(identity)`, `IsDisabled(identity)`, `GetDisabledStores()`, exposed as a new member on `IApplicationGlobals` (exact property name is F1's choice and not yet fixed in any merged code — none of `IStoreDisableService`, `StoreDisableService`, `GetDisabledStores`, or `DisabledStore` appear anywhere under `UtilitiesCS\` or `TaskMaster\` today; they exist only in the four features' still-draft spec/issue/user-story documents). F1's spec states the future-sessions list is persisted on `StoresWrapper` "beside the existing exclusion lists" (`UtilitiesCS\OutlookObjects\Store\StoresWrapper.cs:313-333` shows the existing `[JsonProperty]` exclusion-list fields this would sit next to) and keyed by the same `DisplayName`-primary identity `StoreWrapper` already uses (confirmed convention: `StoresWrapper.RewireOlObjectsAsync` at :102-103 matches stores by `store.DisplayName`).

**Open contract questions for the atomic planner to confirm once F1/F3 land** (not answerable from this repo's current state, since F1/F2/F3 are still draft specs with no implementation):
- The exact `IApplicationGlobals` member name exposing `IStoreDisableService` (e.g. `Globals.Disable` or similar).
- Whether `IStoreDisableService.Reenable(identity)` is synchronous or `Task`-returning (F3's rehook touches live COM and uses "transient-retry patterns," which suggests async, but F1's spec does not fix a signature).
- The exact shape of `GetDisabledStores()`'s return type (a scope enum vs. two separate lists/collections for session-only vs. future-sessions).

F5's design below treats these as narrow, swappable integration points and does not hard-code assumptions beyond what the epic manifest's "Shared Design Alignment" section commits to (identity convention, and that `Reenable` internally invokes F3's rehook — see §3).

## 2. Candidate Approaches

### (a) Extend `StoreWrapperViewer` with a "Disabled Stores" section/tab

Add a grid or list panel to the existing single-store editor dialog, reachable from the same `FolderSettings`/`SpamFolderSettings` entry points.

- Advantage: zero new ribbon surface; users already know where "Folder Settings" is.
- Limitation: mixes two unrelated domain concepts — per-store folder-assignment editing (`ArchiveOutlook`/`ArchiveFS`/`JunkEmail`/`JunkPotential`, one store at a time) and fleet-wide disablement management (many stores at once) — inside one `IStoreWrapperViewer`/`StoreWrapperViewer`/Designer trio. This violates the repo's "one class, one responsibility" design principle and would grow an already-exempt Designer file (currently sized for a single-store form) to host an unrelated grid, columns, and a button-column click handler. It also risks regressing existing, currently-passing `StoreWrapperViewerTests`/`StoreWrapperController_Tests` coverage by touching a stable, already-tested surface for an unrelated feature.
- The "Junk Folder Settings" label on `SpamFolderSettings` would also make a fleet-wide "disabled stores" panel appear under a spam-scoped menu label, which is a UX mismatch not present in the other two options.

### (b) A sibling form using the `DgvForm`-style Controller + `IViewer` pattern

A new `DisabledStoresController` + `IDisabledStoresViewer` + `DisabledStoresViewer`/`.Designer.cs` trio, structurally mirroring `StoreWrapperController`/`IStoreWrapperViewer`/`StoreWrapperViewer`, hosting a `DataGridView` with a `DataGridViewButtonColumn` for Reenable (per the issue's explicit implementation hint).

- Advantage: single responsibility preserved; the existing single-store editor is untouched (zero regression risk to `StoreWrapperController`/`StoreWrapperViewer`); the new Controller/`IViewer` pair can be sized and tested independently; matches the issue text's explicit implementation hint ("use the reusable `DgvForm` DataGridView shell (or a new designer following the same pattern)").
- Limitation: `DgvForm` itself has no Controller/`IViewer` abstraction and is not testable without STA-thread wrapping (§1.3); it cannot be reused as-is if the goal is controller-level unit testability. The practical shape is "a new designer following the same pattern," not literal reuse of `DgvForm`.

### (c) A new ribbon item under Settings, independent of Folder Settings

Not mutually exclusive with (b) — this is an entry-point decision, not a UI-surface decision.

### Recommendation

**Adopt (b) as the UI surface** — a new sibling Controller + `IViewer` pair, not an extension of `StoreWrapperViewer` — **reached via a new, additive ribbon button (c)** in the same `Settings` menu as `FolderSettings` (`TaskMaster\Ribbon\RibbonExplorer.xml:228-240`), e.g. `id="DisabledStoresSettings"`, `onAction="DisabledStoresSettings_Click"`, label "Disabled Stores". This is the least invasive option that does not touch the existing, tested `StoreWrapperController`/`StoreWrapperViewer` pair, keeps the new grid-based, list-oriented UI as its own cohesive class per the general/C# design principles ("one clear domain concept" — fleet-wide disablement state, vs. "clear domain concept" of single-store folder assignment already owned by `StoreWrapperController`), and gives users a discoverable, persistent surface (the issue's stated goal — "independent of the transient modeless notification") rather than burying it inside an unrelated single-store form. The ribbon XML change is a small, additive markup diff with no behavioral risk to existing buttons.

**Rejected alternatives:** (a) is rejected because it couples two unrelated domain concepts into one Designer/interface pair and risks regressing a stable, already-tested dialog for an unrelated concern.

## 3. Controller Design

### 3.1 New files (production)

- `UtilitiesCS\OutlookObjects\Store\IDisabledStoresViewer.cs` — narrow interface extending `IForm` (mirrors `IStoreWrapperViewer`'s shape): exposes `DataGridView Dgv { get; set; }` and a single WinForms-typed forwarding method, e.g. `void Dgv_CellContentClick(object sender, DataGridViewCellEventArgs e);`.
- `UtilitiesCS\OutlookObjects\Store\DisabledStoresViewer.cs` + `DisabledStoresViewer.Designer.cs` — thin partial `Form` mirroring `StoreWrapperViewer.cs`: wires `Dgv.CellContentClick += Dgv_CellContentClick` in the constructor and forwards to `Controller.Dgv_CellContentClick(sender, e)`; Designer file adds the `DataGridView` (docked fill, matching `DvgForm.Designer.cs`'s layout) plus a `DataGridViewButtonColumn` for "Reenable" and text columns for display name and scope.
- `UtilitiesCS\OutlookObjects\Store\DisabledStoreRow.cs` — a plain, pure POCO/row-view-model (no WinForms dependency): `Identity` (string, the F1 identity), `DisplayName` (string, for display — may equal `Identity`), `ScopeLabel` (string, e.g. "Session Only" / "Future Sessions"), `IsFutureSession` (bool, drives the "visually distinguished" styling in the Designer-side cell formatting). Fully unit-testable, not WinForms-exempt.
- `UtilitiesCS\OutlookObjects\Store\DisabledStoresController.cs` — the testable Controller (see §3.2).

### 3.2 Controller responsibilities (testable, Moq-friendly, no live `DataGridView`)

The key testability decision is that **the Controller owns the authoritative in-memory row list, not the grid.** The `Viewer.Dgv.DataSource` is bound to that same list for display, but the Controller's click-handling logic resolves "which row was clicked" from its own list by row index, not by reading back from the live grid control. This mirrors `System.Windows.Forms.DataGridViewCellEventArgs`, which is a plain POCO-like args class (`public DataGridViewCellEventArgs(int columnIndex, int rowIndex)`) constructible in a test with no live grid, Form, or STA thread required — avoiding the `DgvForm`/`TableLayoutPanel` STA constraint identified in §1.3 entirely for controller tests.

Proposed shape (signatures illustrative; exact `IStoreDisableService` member/type names depend on F1's landed contract, see §1.4):

```csharp
public class DisabledStoresController
{
    internal IApplicationGlobals Globals { get; }
    public IDisabledStoresViewer Viewer { get; internal set; }
    internal List<DisabledStoreRow> Rows { get; set; } = new();

    public void Launch();                       // readiness gate + populate + ShowDialog
    internal void PopulateRows();                 // GetDisabledStores() -> Rows -> Viewer.Dgv.DataSource
    public void Dgv_CellContentClick(object sender, DataGridViewCellEventArgs e);
    internal void Reenable(DisabledStoreRow row);  // calls IStoreDisableService.Reenable(row.Identity)
}
```

- **`Launch()`** reuses the same readiness gate as `StoreWrapperController.EvaluateLaunchReadiness()` before opening the dialog. Because that method is `internal` (not `private`) on `StoreWrapperController` and is a pure function of `Globals` with no controller-instance state, the least-invasive way to avoid duplicating the null-check logic (per the repo's "avoid copy-paste" policy) is to extract it into a small shared internal static helper (e.g. `StoreLaunchReadinessEvaluator.Evaluate(IApplicationGlobals)`) that both `StoreWrapperController.EvaluateLaunchReadiness()` and `DisabledStoresController.Launch()` call — behavior-preserving for `StoreWrapperController` (existing tests in `StoreWrapperController_Tests.Launch.cs` call `controller.EvaluateLaunchReadiness()` and would continue to pass unchanged if that method becomes a one-line delegation). This is how F5 "interacts with the existing single-store editor without breaking it" (§4 below): it reuses the readiness *logic*, not the `StoreWrapperController` *instance* or its `Viewer`/`Model`/`Current` state.
- **`PopulateRows()`** calls the F1 service's `GetDisabledStores()`, projects each entry into a `DisabledStoreRow` (setting `IsFutureSession` from whatever scope indicator F1's contract exposes), assigns `Rows`, and sets `Viewer.Dgv.DataSource = new BindingList<DisabledStoreRow>(Rows)` (or equivalent) so the grid reflects the list without manual row-add/remove bookkeeping.
- **`Dgv_CellContentClick(sender, e)`** checks `e.RowIndex >= 0` and that `e.ColumnIndex` is the Reenable button column, resolves `var row = Rows[e.RowIndex]`, and calls `Reenable(row)`.
- **`Reenable(row)`** calls the F1 service's `Reenable(row.Identity)` inside a try/catch (mirroring `StoreWrapperController.SelectFolder()`'s `catch (Exception e) { logger.Error(...); }` pattern), then unconditionally calls `PopulateRows()` again to re-fetch the true current state from the service — this is simpler and less bug-prone than manually removing/updating the clicked row, and naturally satisfies "the row updates to reflect the new state" (AC2) whether the call succeeded, partially succeeded, or failed. On failure, additionally surface a `MyBox.ShowDialog(...)` message (same seam already used and already tested via `MyBox.DialogInvoker` in `StoreWrapperController_Tests.Launch.cs`) so the user sees the failure without a crash (AC/test-condition "reenable failure surfaced without crashing").
- Per the epic's "Shared Design Alignment" section ("F3's rehook is invoked by F1's `Reenable` ... and by F4/F5 reenable actions"), `DisabledStoresController.Reenable` should call **only** `IStoreDisableService.Reenable(identity)` — it must not separately orchestrate F3's rehook or F1's state-clearing/persistence; those are internal to F1's `Reenable` implementation. This keeps F5's controller thin and its unit tests focused on "was `Reenable(identity)` invoked with the right identity and did the row list refresh," not on rehook/persistence mechanics (which belong to F1/F3's own test suites).
- UI-thread marshaling: if `Reenable` (or the underlying service call) is `Task`-returning, the click handler should follow the existing `Viewer.InvokeRequired`/`Viewer.Invoke(...)` convention already used by `StoreWrapperController`'s folder-picker click handlers before touching `Viewer` state from a callback continuation.

## 4. F2 as Prerequisite and Non-Interference with the Single-Store Editor

- F2 fixes `Globals.Ol.StoresWrapper` at its root cause so it is reliably non-null after startup completes (epic manifest's "Root-Cause Note"). F5's dialog reuses the same readiness evaluation (§3.2) that `StoreWrapperController.Launch()` already applies, so once F2 lands, both the single-store editor and the new disabled-stores dialog become reliably openable using one shared, already-covered-by-tests readiness function.
- F5 does not modify `StoreWrapperController`, `IStoreWrapperViewer`, or `StoreWrapperViewer` beyond the proposed extraction of the shared readiness helper (§3.2), which is designed to be behavior-preserving for `StoreWrapperController.EvaluateLaunchReadiness()` (same inputs, same `StoreLaunchReadiness` outputs, same three states). Existing `StoreWrapperController_Tests.*` and `StoreWrapperViewerTests.cs` are expected to continue passing unchanged.
- `IStoreDisableService.GetDisabledStores()` itself does not, per F1's spec, depend on `StoresWrapper.Stores` (it operates on the epic's independent disabled-store model). The dependency on F2 is about **dialog-open reliability and UX consistency across the Settings-menu family**, not a data dependency of the disabled-store list itself.

## 5. Behavior Semantics

- **Success:** `Launch()` opens the dialog with one row per currently-disabled store, each visually distinguished by scope (`IsFutureSession` drives a distinct `ScopeLabel` text and, at the Designer/cell-formatting layer, a distinct row style — e.g. `DefaultCellStyle`/`CellFormatting` differentiation, which is WinForms-exempt code). Clicking Reenable on a row calls `IStoreDisableService.Reenable(identity)` and the grid refreshes to the service's current state.
- **Failure:** an exception from the service's `Reenable` call is caught, logged via log4net (matching the existing `logger.Error($"...", e)` pattern), surfaced to the user via `MyBox.ShowDialog(...)`, and does not throw out of the click handler; the grid still refreshes from `GetDisabledStores()` afterward so the displayed state matches reality even if the reenable attempt failed.
- **Ordering:** re-fetch-after-action (not manual row mutation) is the chosen ordering rule — the grid is always a direct projection of the latest `GetDisabledStores()` result, both on open and after every Reenable click, avoiding any possibility of the UI drifting from the service's actual state.
- **Edge cases:**
  - Empty disabled list: `PopulateRows()` binds an empty collection; the dialog opens with headers only and no rows, no special-case branching required.
  - Both scopes present simultaneously: each row's `IsFutureSession` flag is independent per row; no assumption that all rows share one scope.
  - Readiness not yet met (F2 scenario): `Launch()` shows the same "not available yet" warning as `StoreWrapperController.Launch()` and leaves `Viewer` null, consistent with the existing dialog family's behavior.
  - Double-click / re-entrancy on the same row while a Reenable call is in flight: out of scope for a first implementation given no evidence F1's `Reenable` signature is async (§1.4 open question); if it proves to be `Task`-returning, the click handler should disable the clicked row's button (or the whole grid) for the duration of the call to prevent duplicate invocations, then re-enable via the post-call `PopulateRows()` refresh.

## 6. Requirements Mapping

| Acceptance criterion (issue #265) | Design element | File(s) |
|---|---|---|
| Lists disabled stores, session-only vs. future-sessions visually distinguished | `DisabledStoresController.PopulateRows()` projects `GetDisabledStores()` into `DisabledStoreRow.IsFutureSession`/`ScopeLabel`; Designer-side cell formatting renders the distinction | `DisabledStoresController.cs`, `DisabledStoreRow.cs`, `DisabledStoresViewer.Designer.cs` |
| Per-row Reenable invokes F3 rehook and clears disablement, persisting for future-sessions rows, row updates | `Dgv_CellContentClick` → `Reenable(row)` → `IStoreDisableService.Reenable(identity)` (F1 orchestrates F3 rehook + clear + persist internally, per epic's Shared Design Alignment) → unconditional `PopulateRows()` refresh | `DisabledStoresController.cs` |
| List reflects current service state on open and after reenable | `PopulateRows()` called from both `Launch()` and post-`Reenable()` | `DisabledStoresController.cs` |
| Controller + `IViewer` seam, Moq-testable, no live Outlook, no temp files | `IDisabledStoresViewer` interface + row-index-based click forwarding avoids any live `DataGridView`/Outlook dependency in controller tests | `IDisabledStoresViewer.cs`, `DisabledStoresController.cs` |
| Full toolchain passes; WinForms designer code exempt | `DisabledStoresViewer.cs`/`.Designer.cs` fall under the repo's WinForms form-derived/Designer-generated coverage exemption; `DisabledStoresController.cs`/`DisabledStoreRow.cs` are new testable code targeting >=90% coverage | `DisabledStoresViewer.cs`, `DisabledStoresViewer.Designer.cs`, `DisabledStoresController.cs`, `DisabledStoreRow.cs` |

## 7. Testing Implications

- **Controller unit tests** (new `UtilitiesCS.Test\OutlookObjects\Store\DisabledStoresControllerTests.cs`, mirroring the existing `StoreWrapperController_Tests.*` split-by-concern file layout):
  - Mock `IApplicationGlobals`/`IOlObjects` (readiness gate) and a mocked F1 service to assert `PopulateRows()`/`Launch()` project `GetDisabledStores()` results into `Rows` correctly, including the scope flag, with **no live Outlook** and **no temp files**, matching repo policy.
  - Mock `IDisabledStoresViewer` fully via Moq (same pattern as `dispatchedViewer = new Mock<IStoreWrapperViewer>()` in `StoreWrapperViewerTests.cs`) to assert `Viewer.Close()` / dialog-flow calls without ever constructing a real `DataGridView`.
  - Construct `DataGridViewCellEventArgs(columnIndex, rowIndex)` directly (no live grid, no STA thread) to drive `Dgv_CellContentClick` and assert the mocked service's `Reenable(identity)` is invoked exactly once with the expected identity (Moq `Verify`).
  - Empty-list case: seed the mocked service to return an empty collection and assert `Rows` is empty and no exception is thrown.
  - Failure case: seed the mocked service's `Reenable` to throw; assert the exception does not escape the click handler (`Should().NotThrow()`, matching the existing `Launch_WhenStoresWrapperIsNull_...` test's assertion style), that `MyBox.DialogInvoker` is invoked (reusing the existing `MyBox.DialogInvoker` seam already proven in `StoreWrapperController_Tests.Launch.cs`), and that `PopulateRows()` still runs afterward.
  - Reuse the reflection-based `SetInternalProperty`/`GetInternalProperty` helper pattern from `StoreWrapperViewerTests.cs` if `Rows`/`Viewer` need to be seeded or asserted as `internal` members.
- **Designer/Viewer-level code** (`DisabledStoresViewer.cs`/`.Designer.cs`): falls under the repo's WinForms form-derived and Designer-generated coverage exemption (per `CLAUDE.md`'s C# Unit Test Policy exemption list, items (a)/(b)). An optional, narrow smoke test mirroring `DvgForm_Tests.cs`'s STA-thread-wrapped construction check (verifying the form constructs and wires `CellContentClick` without throwing) is a reasonable but not mandatory addition, consistent with how `DvgForm_Tests.cs` treats `DgvForm`.
- No new test infrastructure is required beyond what `StoreWrapperController_Tests.*`/`StoreWrapperViewerTests.cs` already establish (Moq, FluentAssertions, MSTest, the `MyBox.DialogInvoker` seam, and reflection-based internal-property helpers).

## 8. File-by-File Change List

### New files — testable (must meet the repo's >=90%-for-new-code target)

| File | Purpose |
|---|---|
| `UtilitiesCS\OutlookObjects\Store\DisabledStoresController.cs` | Controller: readiness gate reuse, row population from `IStoreDisableService`, Reenable click handling, refresh-after-action |
| `UtilitiesCS\OutlookObjects\Store\DisabledStoreRow.cs` | Pure row/view-model POCO (`Identity`, `DisplayName`, `ScopeLabel`, `IsFutureSession`) |
| `UtilitiesCS\OutlookObjects\Store\IDisabledStoresViewer.cs` | Interface-only file (type-only; legitimately reports 0% executable coverage per the general unit-test policy's type-only-module clarification) |

### New files — WinForms-exempt (COM/VSTO/WinForms coverage exemption)

| File | Purpose |
|---|---|
| `UtilitiesCS\OutlookObjects\Store\DisabledStoresViewer.cs` | Thin partial `Form`; forwards `CellContentClick` to Controller |
| `UtilitiesCS\OutlookObjects\Store\DisabledStoresViewer.Designer.cs` | Designer-generated `DataGridView` + columns (including the `DataGridViewButtonColumn` for Reenable) |

### Modified files — thin, exempt ribbon wiring

| File | Change |
|---|---|
| `TaskMaster\Ribbon\RibbonExplorer.xml` | Add one `<button>` under the existing `Settings` menu (`:228-437` region), e.g. `id="DisabledStoresSettings"`, `onAction="DisabledStoresSettings_Click"`, label "Disabled Stores" — markup only |
| `TaskMaster\Ribbon\RibbonViewer.cs` | Add `DisabledStoresSettings_Click(Office.IRibbonControl control) => _controller.DisabledStoresSettings();`, mirroring `FolderSettings_Click` at `:180-181` |
| `TaskMaster\Ribbon\RibbonController.cs` | Add `internal void DisabledStoresSettings() { new DisabledStoresController(Globals).Launch(); }`, mirroring `FolderStoresSettings()` at `:259-263`; inherits the class-level `[ExcludeFromCodeCoverage]` already on `RibbonController` |

### Modified files — behavior-preserving refactor to avoid duplicated readiness logic

| File | Change |
|---|---|
| `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs` | Extract the body of `EvaluateLaunchReadiness()` into a shared internal static helper reused by both controllers; `EvaluateLaunchReadiness()` becomes a one-line delegation with unchanged inputs/outputs so existing tests in `StoreWrapperController_Tests.Launch.cs` continue to pass unmodified |

### Production file count

- Testable (subject to >=90% new-code coverage target): 3 new files (`DisabledStoresController.cs`, `DisabledStoreRow.cs`, `IDisabledStoresViewer.cs` — the last is type-only and legitimately reports 0% executable coverage) + 1 modified (`StoreWrapperController.cs`, behavior-preserving extraction only).
- WinForms-exempt: 2 new files (`DisabledStoresViewer.cs`, `DisabledStoresViewer.Designer.cs`).
- Ribbon wiring (exempt via class-level `[ExcludeFromCodeCoverage]` on `RibbonController`, and markup-only for the `.xml`): 3 modified files (`RibbonExplorer.xml`, `RibbonViewer.cs`, `RibbonController.cs`).
- Total: 5 new production files, 4 modified production files.

### Cross-feature impacts

- **F1 (#261):** F5 is a pure consumer of `IStoreDisableService` (`GetDisabledStores()`, `Reenable(identity)`) and the `DisplayName`-primary identity convention. No F5 file changes are needed if F1 changes its internal persistence details, only if its public member names/signatures on `IApplicationGlobals`/`IStoreDisableService` change from what the atomic plan assumes (see open questions, §1.4).
- **F2 (#262):** F5 shares (via the proposed extraction) the exact readiness-evaluation logic F2 fixes at the root; no F5-specific change is needed once F2 lands — the shared helper benefits from F2's fix automatically.
- **F3 (#263):** F5 does not call F3's rehook interface directly; it calls only `IStoreDisableService.Reenable(identity)`, which F1's implementation wires to F3 internally per the epic's Shared Design Alignment. This keeps F5 decoupled from F3's COM-hookup mechanics entirely.
- **F4 (#264):** No direct code dependency; F4's modeless notification and F5's settings dialog are independent consumers of the same F1 service, and both funnel "reenable" through `IStoreDisableService.Reenable(identity)` rather than duplicating rehook/persistence logic.
- **Existing `StoreWrapperController`/`StoreWrapperViewer` (`FolderSettings`/`SpamFolderSettings`):** unaffected except for the behavior-preserving readiness-helper extraction; no change to `IStoreWrapperViewer`, `StoreWrapperViewer.cs`, or `StoreWrapperViewer.Designer.cs`.
