# [P9-T2] Coverage Exemption Inventory (for maintainer ratification)

Timestamp: 2026-07-10T06:06:59Z

Enumerates every `[ExcludeFromCodeCoverage]` in the #298 touched production files,
reconciled against the plan's Coverage Exemption Inventory and the P0-T7 exemption
findings. Reducibility note per site confirms no coverable logic is hidden behind an
exemption. Testable seams are never exempt.

## REMOVED (previously #197 class-level; now measured)

| File | Change |
|------|--------|
| `EditFilterController.cs` | class-level `[ExcludeFromCodeCoverage]` removed; retargeted to `IEditFilterViewer` + viewer-factory + `_tagSelector` seams. Controller logic measured. |
| `FlagTasks.cs` | class-level removed; pure statics extracted to `FlagCalculations.cs` (measured); remaining exemptions method-level only. |
| `AutoCreateProject.cs` | class-level removed; host-neutral members measured. |
| `AutoAssignContext.cs` | class-level removed; host-neutral branches measured. |
| `AutoAssignPeople.cs` | class-level removed; host-neutral branches measured. |

## RETAINED — class-level on Form partials (irreducible WinForms/COM)

| Site | Justification | Reducibility |
|------|---------------|--------------|
| `EditFilterViewer.cs` (`: Form, IEditFilterViewer`) + `EditFilterViewer.designer.cs` | `Form`-derived + Designer-generated control construction; interface pass-throughs read/write live `Control.Text`. | Logic already extracted to `EditFilterController`; only thin control binding remains. |
| `ManageFilters.cs` (`: Form, IManageFiltersViewer`) + `ManageFilters.Designer.cs` | `Form`-derived; `FastObjectListView`/`OLVColumn` pass-throughs bind live BrightIdeasSoftware controls. | Logic extracted to `ManageFiltersController`; only control binding remains. |

## RETAINED / ADDED — method-level (single irreducible live-host calls)

`EditFilterController.cs`
- `DefaultViewerFactory()` — default seam: `new EditFilterViewer()` (live Form). Tests inject a `Mock<IEditFilterViewer>`. Reducibility: the entire controller uses the injected seam; only the production default constructs a form.
- `DefaultTagSelector(...)` — default seam: `new TagViewer()` + `ShowDialog()` (modal dialog). Tests inject a canned `(cancelled, selection)`. Reducibility: selection logic runs through the injected delegate; only the default shows a dialog.
- `DeleteFilterDialog(...)` (static) — **ADDED beyond plan.** Constructs the viewer via the default factory and calls `viewer.ShowDialog()`. Irreducible live-form bridge; not unit-testable under the STA/no-form policy. Reducibility: no branch logic beyond the OK/cancel `DialogResult` check that only a live modal can produce; nothing coverable is hidden.

`FlagTasks.cs` (Outlook-bound; no cheap seam)
- `ctor`, `Run`, `InitializeToDoList`, `PopulateUdf`, `GetSelection`, `GetUserInputFlagsToAdjust` — `ActiveExplorer()`/`Selection`/`MessageBox.Show`/`new TaskViewer()`/`new TaskController()`. Reducibility: all pure flag math extracted to `FlagCalculations.cs` (measured, >= 90%); statics delegate to it.

`AutoCreateProject.cs`
- `DefaultChooseProgram` — live program-selection dialog seam.
- `DefaultCreateCategory` — live MAPI `CreateCategory` seam.
- `CreateProjectTaskItem`, `GetTaskItems` — `Ol.App.Session.GetDefaultFolder`/live `Items`.
- `AutoFindAsync`, `ToHelper` — `MailItemHelper.FromMailItemAsync` live-Interop.
- Reducibility: `FilterList`, `GetNextProjectID`, `TryAutoExtractProgram`, `StripPrefix`, `ChooseOrCreateProgramName`, `AddColorCategory` (early-return + no-program-null branches), and the `NotImplementedException` throwers are all measured. `AddColorCategory` itself is measured; only the live MAPI call is behind the exempt `DefaultCreateCategory` seam — an improvement over the plan's "exempt the MAPI line inside AddColorCategory".

`AutoAssignContext.cs`
- `RunContextClassifierAsync` — classifier-engine invocation on a live `MailItemHelper`.
- `DefaultToHelper` — constructs `MailItemHelper` from a live item (async).
- Reducibility: `FilterList`, the `NotImplementedException` throwers, and `AutoFindAsync`'s null-helper early-return `[]` branch are measured. The classifier call was extracted out of `AutoFindAsync` so `AutoFindAsync` itself is measured.

`AutoAssignPeople.cs`
- `RunPeopleClassifier` — `AutoFile.AutoFindPeople` on a live helper (may show a missing-recipients dialog).
- `DefaultToHelper` — constructs `MailItemHelper` from a live `MailItem`.
- **`AddChoicesToDict`** — single call `_globals.TD.People.AddMissingEntries(olMail)` reading recipients from a live `MailItem`.
- **`AddColorCategory`** — single call `CreateCategoryModule.CreateCategory(...)` against live MAPI.
- Reducibility: `FilterList`, the `AutoFind(null)`/unknown-type early-return `[]` branches, and the `MailItem`-branch routing through the `_toHelper` seam are measured. Each exempt member is a one-line irreducible COM call.

`ManageFiltersController.cs`
- `DefaultEditFilterFactory(...)` — **ADDED beyond plan.** The production default of the injected `_editFilterFactory` seam; constructs an `EditFilterController`, which builds and shows a live WinForms form. Irreducible live-form bridge; not unit-testable under the STA/no-form policy. Reducibility: the only logic is the null-vs-non-null entry ternary selecting which `EditFilterController` constructor to call; that branch selection is asserted through the **injected** seam in the `AddFilter` (null) and `EditSelected` (non-null) tests, so no coverable logic is hidden. All orchestration (`LoadFilters`, `EditSelected`, `AddFilter`, `EditFilterCallback`, `DeleteSelected`) remains measured at 100%.

### Beyond-plan sites flagged for maintainer ratification

Four exemptions go beyond the plan's explicit enumeration; each is a single irreducible
live-host statement with no hidden coverable logic:

1. `AutoAssignPeople.AddChoicesToDict` — `_globals.TD.People.AddMissingEntries(liveMailItem)` (live recipient read). Unlike `AutoAssignContext`'s `NotImplementedException` stub (measured via throw test), this is a genuine COM call.
2. `AutoAssignPeople.AddColorCategory` — MAPI `CreateCategoryModule.CreateCategory(...)` (live category creation).
3. `EditFilterController.DeleteFilterDialog` (static) — default-factory viewer + `ShowDialog()` (live modal).
4. `ManageFiltersController.DefaultEditFilterFactory` — default of the injected factory seam; builds a live-form controller.

Sites 3 and 4 were required to satisfy the plan's own `>= 90%` new-class threshold
without violating the higher-authority STA/no-form policy (the two directives conflict
at these irreducible live-form bridges). Coverage post-exemption: `ManageFiltersController`
100%, `EditFilterController` 95.07% (see `qa-gates/coverage-delta.md`). Maintainer
ratification requested for all four sites.

## NEVER exempt (measured)

`FlagCalculations.cs` (100%), retargeted `EditFilterController.cs` **orchestration**
logic (event handlers, `SelectItems`, `ApplySelectionText`, `Initialize`/
`InitializeFactory`, OK/Cancel handlers — all measured), `ManageFiltersController.cs`
**orchestration** logic (`LoadFilters`/`EditSelected`/`AddFilter`/`EditFilterCallback`/
`DeleteSelected` — all measured at 100%), `FlagChangeGroup.cs` `TryEnqueue`,
`FlagChangeTrainingQueue.cs` logic, `FlagChangeItem.cs`. The only `ManageFiltersController`
/`EditFilterController` exemptions are the irreducible live-form bridges noted above,
never orchestration logic.

## Interface-only files (no exemption; excluded from measurement by policy)

`IEditFilterViewer.cs`, `IManageFiltersViewer.cs` carry no `[ExcludeFromCodeCoverage]`;
they are legitimately 0% executable and excluded per the interface-only clarification
in `.claude/rules/general-unit-test.md`.
