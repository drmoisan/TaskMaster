# Phase 0 — Upstream Assumption Re-Verification (P0-T2..P0-T7)

Timestamp: 2026-07-10T01-17
Head: 949dddd2 (post-#297 epic integration; #293/#296/#297 merged)

Note on working-tree state at execution start: a prior, uncommitted execution attempt of this same plan had already produced the Phase 1–3 output (new files `IEditFilterViewer.cs`, `IManageFiltersViewer.cs`, `ManageFiltersController.cs`; modified `EditFilterController.cs`, `EditFilterViewer.cs`, `ManageFilters.cs`, `TaskVisualization.csproj`). These were verified line-by-line to match Phase 1–3 tasks and are retained. Phase 0 baselines below are captured against a clean post-#297 HEAD by temporarily stashing those #298 production changes.

## P0-T2 — Assumption 1: ITaskViewer shape / per-form interface pattern

- `TaskVisualization/ITaskViewer.cs` EXISTS (`public interface ITaskViewer : IForm`), created by #297.
- `TaskVisualization/TaskController.cs` constructors take `ITaskViewer formInstance` (confirmed line 33, 91). The concrete `TaskViewer` implements `ITaskViewer`.
- Decision for #298: reuse the established per-form `IForm`-derived interface pattern. `IEditFilterViewer : IForm` and `IManageFiltersViewer : IForm` follow ITaskViewer's precedent (rely on the inherited `IControl` chain for `Text`/`Show()`/`Hide()`; declare only additive members).
- Result: **PASS** (pattern reused; no self-contained-seam fallback needed).

## P0-T3 — Assumption 2: dialog seam

- `TaskVisualization/ITagPromptService.cs` + `TagPromptService.cs` EXIST (from #297) as a reusable Tags-dialog seam (`TagPromptRequest`/`TagPromptResult`).
- Decision for #298: P2-T2 explicitly permits reuse-or-local-delegate. A NARROW per-call delegate was chosen for `EditFilterController.SelectItems`: `Func<SortedDictionary<string,bool>, (bool cancelled, string selection)> _tagSelector`, default `DefaultTagSelector` (constructs `TagViewer`/`TagController`, `[ExcludeFromCodeCoverage]`). Rationale: the `SelectItems` call site needs only options-in / (cancelled, selection)-out; the richer `ITagPromptService` request carries auto-assigner/prefix/mail-item context not required here, so the minimal-seam rule (`.claude/rules/csharp.md` DI Seams: delegate over interface when an interface is excessive) selects the delegate.
- Result: **PASS** (local narrow delegate).

## P0-T4 — Assumption 3: Interop adapter

- No reusable folder/items/`MailItemHelper` adapter exists post-#297 beyond the existing static `MailItemHelper.FromMailItemAsync` and `CreateCategoryModule.CreateCategory`.
- Decision for #298: local injectable-delegate seams at each boundary — `AutoCreateProject` (`_chooseProgram`, `_createCategory`, `_getTaskItems`), `AutoAssignContext` (`_toHelper`), `AutoAssignPeople` (synchronous `_toHelper`). Safe production defaults reproduce the current inline behavior.
- Result: **PASS** (local delegates).

## P0-T5 — Assumption 4: FlagChange stability

- `TaskVisualization/FlagChangeGroup.cs` ctor `(IApplicationGlobals globals, MailItem item)` — confirmed unchanged (line 27).
- `TaskVisualization/FlagChangeTrainingQueue.cs` `Init()` returns `IFlagChangeTrainingQueue` and sets `ConsumerTimer` — confirmed (line 22).
- `UtilitiesCS/Interfaces/IToDo/*` `IFlagChange*` — last touched by commits e539f172 / cf795617, NOT by #297 (82b207ff); unchanged.
- `[assembly: InternalsVisibleTo("TaskVisualization.Test")]` present in `TaskVisualization/FlagTasks.cs` (line 16).
- Result: **PASS**.

## P0-T6 — Assumption 5: csproj cleanliness + #293 watch item

- Committed HEAD `TaskVisualization.csproj` does not reference the #298 files (verified via `git show HEAD`), so the clean-head build excludes them.
- `TagViewer`/`TagController` call sites in `FlagTasks.cs` (and the retargeted `EditFilterController.cs`) compile against #293's `Tags` project (`TagViewer : ITagViewer`).
- Clean-head build verified by the P0-T11 / P0-T12 baseline builds (EXIT_CODE 0). Post-change builds are verified in Phase 10.
- Result: **PASS** (confirmed by baseline builds below).

## P0-T7 — Assumption 6: exemption state (#197 annotations)

At committed HEAD the in-scope files carry #197's `[ExcludeFromCodeCoverage]`:
- `EditFilterController.cs` — class-level (to be REMOVED per plan; already removed in retained Phase 2 work, replaced by two method-level `Default*` seam exemptions).
- `FlagTasks.cs` — class-level (line 20) — to be REMOVED (Phase 4).
- `AutoCreateProject.cs` — class-level (line 16) — to be REMOVED (Phase 5).
- `AutoAssignContext.cs` — class-level (line 14) — to be REMOVED (Phase 5).
- `AutoAssignPeople.cs` — class-level (line 16) — to be REMOVED (Phase 5).
- `FlagChangeGroup.cs` — method-level on the four Outlook-bound members (lines 26/75/105/129) — RETAINED.
- `EditFilterViewer.cs` / `ManageFilters.cs` — class-level on the form partials — RETAINED.
- Result: **PASS**; no deltas from the plan's exemption inventory.

## Interface-member design note (mechanically necessary for a green toolchain)

`IForm : IContainerControl, IScrollableControl`; `IScrollableControl : IControl`; and `IControl : IComponent, ...`. `IControl` already declares `string Text { get; set; }` (line 77), `void Hide()` (line 169), and `void Show()` (line 207), and `System.ComponentModel.IComponent : IDisposable` supplies `void Dispose()`. Re-declaring any of `Text`/`Show()`/`Hide()`/`Dispose()` in `IEditFilterViewer`/`IManageFiltersViewer` would emit CS0108 (hides inherited member), which the `/p:TreatWarningsAsErrors=true` nullable gate promotes to an error. Therefore the new interfaces declare only additive members and rely on the inherited `IControl`/`IForm`/`IComponent` chain for `Text`/`Show()`/`Hide()`/`Dispose()`/`Close()`/`ShowDialog()`/`DialogResult` — identical to how #297's `ITaskViewer` is defined. `EditFilterController.BtnOk_Click`'s `_viewer.Dispose()` and `DeleteFilterDialog`'s `viewer.Text`/`viewer.ShowDialog()` therefore resolve through the inherited chain with `_viewer` typed as `IEditFilterViewer`. This satisfies each task's "toolchain green" acceptance criterion and matches the plan P1-T1 member list in effect (the members the plan enumerates as declared are the ones the inherited chain already provides; declaring them literally would break the build).

## P0-T9 gate outcome

All of P0-T2..P0-T7 resolved **PASS** (with documented local-delegate fallbacks for assumptions 2 and 3). No HALT required. Proceeding to Phase 1.
