# qfc-form-viewer-testability (Issue #223)

- Date captured: 2026-06-28
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-form-viewer-testability/ (Issue #223)
- Type: refactor (testability)

- Issue: #223
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/223
- Last Updated: 2026-06-29
- Work Mode: full-feature

## Problem / Why

`QuickFiler/Viewers/QfcFormViewer.cs` is a WinForms `Form` whose public interface
`IQfcFormViewer` re-exposes raw WinForms control types (four `Button` properties and
one `NumericUpDown`) and item-viewer template UserControls. Consumers
(`QfcFormController`, `QfcHomeController`, `QfcCollectionController`) couple directly to
these UI types, so unit tests against `Mock<IQfcFormViewer>` can only assert that event
wiring "does not throw"; they cannot verify that a control event routes to the correct
controller behavior, nor exercise the template-snapshot and TLP-swap logic. Pure routing
logic (the Alt-key predicate in `ProcessCmdKey`) is embedded in `Form` overrides that
cannot be invoked without a live window handle.

The user has already introduced `IQfcFormViewer` as the first step of a Passive-View MVP
refactor and has requested a full review and a refactor that maximizes unit testability.

## Proposed Behavior

Narrow `IQfcFormViewer` to intent-level members and extract the small amount of pure
logic out of the Form so controller behavior becomes verifiable with MSTest + Moq +
FluentAssertions, while the Form-derived and Designer-generated code remains
`[ExcludeFromCodeCoverage]` per the repository COM/VSTO/WinForms exemption.

Four seams, all delivered this cycle:

- **Seam A (Task 1):** Extract `QfcFormKeyHandler.IsAltKeyCommand(Keys)` (pure static) and
  call it from the three form variants' `ProcessCmdKey`. Add `[ExcludeFromCodeCoverage]`
  to `QfcFormViewerDark` and `QfcFormViewerExpanded`.
- **Seam B (Task 2):** Replace the five raw control properties with command events
  (`OkClicked`, `CancelClicked`, `UndoClicked`, `SkipClicked`, `ItemsPerLoadValueChanged`)
  and state properties (`SkipButtonText`, `SkipButtonEnabled`, `ItemsPerLoadValue`,
  `ItemsPerLoadEnabled`).
- **Seam C:** Add `void SwapItemTableLayout(TableLayoutPanel newTlp)`, absorb the only
  setter write in `QfcCollectionController.ActivateQueuedTlp`, and narrow
  `L1v0L2L3v_TableLayout` to get-only.
- **Seam D:** Add `CaptureTlpCellStates()`, `GetKeyEventExclusionControls()`, and
  `ItemViewerTemplateMargin`; remove `QfcItemViewerTemplate` and
  `QfcItemViewerExpandedTemplate` from the interface; refactor
  `QfcFormController.CaptureItemSettings` and `RegisterFormEventHandlers` to use them.

Phase 0 prerequisite: split `QfcFormController.cs` (1142 lines) into partial classes to
satisfy the 500-line file cap before adding code.

## Acceptance Criteria (early draft)

- [x] AC1: `QfcFormKeyHandler.IsAltKeyCommand(Keys)` exists as a pure, non-Form unit and is
  called by `QfcFormViewer`, `QfcFormViewerDark`, and `QfcFormViewerExpanded`
  `ProcessCmdKey` overrides; `QfcFormViewerDark` and `QfcFormViewerExpanded` carry
  `[ExcludeFromCodeCoverage]`.
- [x] AC2: `IQfcFormViewer` exposes intent-level command events and state properties in
  place of the four `Button` properties and the `NumericUpDown` property; no raw clickable
  control type remains on the interface.
- [x] AC3: `IQfcFormViewer` exposes `SwapItemTableLayout(TableLayoutPanel)`;
  `L1v0L2L3v_TableLayout` is get-only on the interface; `ActivateQueuedTlp` performs the
  swap through the new method.
- [x] AC4: `IQfcFormViewer` exposes `CaptureTlpCellStates()`,
  `GetKeyEventExclusionControls()`, and `ItemViewerTemplateMargin`;
  `QfcItemViewerTemplate` and `QfcItemViewerExpandedTemplate` are removed from the
  interface; `CaptureItemSettings` and `RegisterFormEventHandlers` consume the new members.
- [x] AC5: New MSTest coverage verifies, via Moq event raising / `VerifySet` / `Verify`,
  that command events route to the correct controller methods, that the skip flow toggles
  `SkipButtonText`/`SkipButtonEnabled`, and that `CaptureItemSettings` handles both the
  populated and null `CaptureTlpCellStates()` results. New non-exempt code meets the
  >= 90% coverage floor; changed lines do not regress coverage; repo-wide coverage stays
  >= 80%.
- [x] AC6: No production file modified in this cycle exceeds 500 lines after the change
  (`QfcFormController.cs` split into partial classes). `QfcCollectionController.cs` is a
  pre-existing cap violation touched only with a net-negative edit; disposition recorded.
- [x] AC7: Full C# toolchain passes in order — csharpier, .NET analyzers,
  nullable/TreatWarningsAsErrors, MSTest with coverage — with no regressions.

## Constraints & Risks

- `QfcFormController.cs` (1142 lines) and `QfcCollectionController.cs` (2300 lines) are
  pre-existing 500-line-cap violations. Phase 0 splits the former (it gains code this
  cycle). The latter receives only a net-negative edit and is treated as pre-existing debt;
  splitting it would be a broad out-of-scope refactor of an `[ExcludeFromCodeCoverage]`
  class. Feature-review may flag this; disposition is a review-time decision.
- `QfcFormViewerDark`/`QfcFormViewerExpanded` are structurally diverged from `QfcFormViewer`
  and do not implement `IQfcFormViewer`; they are touched only by Seam A.
- `ItemViewer`/`ItemViewerExpanded` are UserControl-derived and remain Form-bound; Seam D
  keeps them as private Form fields and exposes only plain-C# snapshot results.
- Interface narrowing is a breaking change to `IQfcFormViewer`, updated in-repo across all
  consumers; no external consumers exist.

## Test Conditions to Consider

- [ ] Unit coverage: `IsAltKeyCommand` for `Keys.Alt`, `Keys.Alt | Keys.Left`,
  `Keys.Control`, `Keys.None`.
- [ ] Unit coverage: command-event routing (`OkClicked`/`CancelClicked`/`UndoClicked`/
  `SkipClicked`/`ItemsPerLoadValueChanged`) via Moq `Raise`.
- [ ] Unit coverage: skip flow state transitions; `CaptureItemSettings` populated vs. null
  vs. early-return (null RowStyles) paths; exclusion-control usage in
  `RegisterFormEventHandlers`.
- [ ] No temporary files; deterministic; MSTest + Moq + FluentAssertions only.

## Next Step

- [ ] Promote to GitHub issue (refactor template)
- [ ] Create active feature folder from the template

## Research

- `artifacts/research/2026-06-28T18-00-qfc-form-viewer-testability-research.md`
- `artifacts/research/2026-06-28T19-00-qfc-seam-c-d-implementation-research.md`
