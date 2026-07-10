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
- `DeleteFilterDialog(...)` (static) — **REMOVED (M1, remediation cycle 1).** The dead static live-form bridge and its only caller path (the private single-argument constructor and the `System.Windows.Forms` using) were deleted; it is no longer an exempt site.

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
- `DefaultCreateCategory` — **ADDED (B2, remediation cycle 1).** Default of the injected `_createCategory` seam; single call `CreateCategoryModule.CreateCategory(...)` against live MAPI. Tests inject a stub delegate. Reducibility: the entire `AddColorCategory` forwarding runs through the injected seam; only the default performs the live MAPI call.
- Reducibility: `AddChoicesToDict` is now **measured (B1, remediation cycle 1)** — the exemption was removed and a Moq `IPeopleScoDictionaryNew` pass-through test forwards a `MailItem` and asserts the returned list. `AddColorCategory` is now **measured (B2, remediation cycle 1)** — the exemption was removed and it delegates to the injected `_createCategory` seam; only the live MAPI call remains behind the newly exempt `DefaultCreateCategory`. `FilterList`, the `AutoFind(null)`/unknown-type early-return `[]` branches, and the `MailItem`-branch routing through the `_toHelper` seam are measured.

`ManageFiltersController.cs`
- `DefaultEditFilterFactory(...)` — **ADDED beyond plan.** The production default of the injected `_editFilterFactory` seam; constructs an `EditFilterController`, which builds and shows a live WinForms form. Irreducible live-form bridge; not unit-testable under the STA/no-form policy. Reducibility: the only logic is the null-vs-non-null entry ternary selecting which `EditFilterController` constructor to call; that branch selection is asserted through the **injected** seam in the `AddFilter` (null) and `EditSelected` (non-null) tests, so no coverable logic is hidden. All orchestration (`LoadFilters`, `EditSelected`, `AddFilter`, `EditFilterCallback`, `DeleteSelected`) remains measured at 100%.

### Beyond-plan sites flagged for maintainer ratification

Remediation cycle 1 resolved the four #298 findings. B1 removed the
`AutoAssignPeople.AddChoicesToDict` exemption (now measured); B2 removed the
`AutoAssignPeople.AddColorCategory` exemption (now measured) and moved the live MAPI
call behind the newly exempt `AutoAssignPeople.DefaultCreateCategory` seam; M1 deleted
the dead static `EditFilterController.DeleteFilterDialog`. Two exemptions remain beyond
the plan's explicit enumeration; each is a single irreducible live-host statement with
no hidden coverable logic:

1. `AutoAssignPeople.DefaultCreateCategory` — default of the injected `_createCategory` seam; MAPI `CreateCategoryModule.CreateCategory(...)` (live category creation). The forwarding logic in `AddColorCategory` is measured through the injected stub delegate.
2. `ManageFiltersController.DefaultEditFilterFactory` — default of the injected factory seam; builds a live-form controller.

Site 2 was required to satisfy the plan's own `>= 90%` new-class threshold without
violating the higher-authority STA/no-form policy (the two directives conflict at this
irreducible live-form bridge). Coverage post-exemption: `ManageFiltersController` 100%.
Maintainer ratification requested for both sites.

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
