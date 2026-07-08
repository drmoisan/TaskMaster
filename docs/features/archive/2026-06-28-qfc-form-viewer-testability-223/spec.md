# qfc-form-viewer-testability - Refactor Spec

- **Issue:** #223
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-06-28T20-20
- **Status:** Draft
- **Version:** 0.1

## Intent & Outcomes

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


## Scope (structural changes)

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


## Invariants (must not change)

- Runtime behavior of the QuickFiler form is unchanged: OK/Cancel/Undo/Skip clicks,
  items-per-load spinner, TLP swap during iteration, and Alt-key keyboard-dialog toggle
  must behave exactly as before. This is a structural/testability refactor, not a behavior
  change.
- `QfcFormViewer`, `QfcFormViewerDark`, `QfcFormViewerExpanded` remain Form-derived and
  `[ExcludeFromCodeCoverage]`; Designer files are untouched.
- The set of controls rendered and their wiring outcomes are preserved; only the seam
  through which controllers reach them changes.

## Non-Goals

- Splitting `QfcCollectionController.cs` (2300 lines) — pre-existing debt, only a
  net-negative edit this cycle.
- Unifying the diverged `QfcFormViewerDark`/`QfcFormViewerExpanded` variants beyond adopting
  the shared `IsAltKeyCommand` predicate.
- Adding interfaces to `ItemViewer`/`ItemViewerExpanded` or making those UserControls
  unit-testable.
- Any new end-user behavior, performance change, or UX change.

## Dependencies / Touchpoints

Consumers updated in-repo (no external consumers of `IQfcFormViewer`):
`QfcFormController`, `QfcHomeController`, `QfcCollectionController`. `IQfcQueue`/`QfcQueue`
unchanged (Seam C uses the retained getter). `KeyboardHandler` unaffected.
- Required coordination (other teams, CI/CD, release tooling): none beyond required CI
  checks on the PR.

## Risks & Mitigations

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


## Technical Specifications

Production files expected to change (8 total — 7 edits + 1 new):

| File | Change | Seam |
|---|---|---|
| `QuickFiler/Controllers/QfcFormController.cs` | Phase 0 partial-class split (to < 500 lines each); Seam B/C/D consumer rewrites | 0, B, C, D |
| `QuickFiler/Controllers/QfcFormKeyHandler.cs` (NEW) | `internal static bool IsAltKeyCommand(Keys)` | A |
| `QuickFiler/Viewers/QfcFormViewer.cs` | Implement 13 new intent members; remove 7 old property impls; `SwapItemTableLayout`; 3 Seam D methods; call `IsAltKeyCommand` | A, B, C, D |
| `QuickFiler/Viewers/QfcFormViewerDark.cs` | Call `IsAltKeyCommand`; add `[ExcludeFromCodeCoverage]` | A |
| `QuickFiler/Viewers/QfcFormViewerExpanded.cs` | Same as Dark | A |
| `QuickFiler/Interfaces/IQfcFormViewer.cs` | Remove 7 members; narrow `L1v0L2L3v_TableLayout` to get-only; add 13 intent members | B, C, D |
| `QuickFiler/Controllers/QfcCollectionController.cs` | Rewrite `ActivateQueuedTlp` to call `SwapItemTableLayout` (net −3 lines) | C |
| `QuickFiler/Controllers/QfcHomeController.cs` | Replace `L1v1L2h5_SpnEmailPerLoad.Enabled`/`L1v1L2h5_BtnSkip.Enabled` with `ItemsPerLoadEnabled`/`SkipButtonEnabled` | B |

- Public interfaces/contracts affected: `IQfcFormViewer` (final shape: 23 members — see
  research doc §3). Breaking change, all consumers updated in-repo.
- Data flow: `CaptureTlpCellStates()` returns a plain `TlpCellStates` from the Form;
  `GetKeyEventExclusionControls()` returns `IReadOnlyList<Control>`. No data-format change.
- Logging/telemetry: unchanged.
- Migration/backfill: none.

## Test Strategy

- Regression/new tests:
  - `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs` (NEW): `IsAltKeyCommand` for
    `Keys.Alt`, `Keys.Alt | Keys.Left`, `Keys.Control`, `Keys.None`.
  - `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` (UPDATE): migrate removed-member
    mock setups to intent members; add command-event routing tests via Moq `Raise`; skip-flow
    `VerifySet` tests; `CaptureItemSettings` populated/null/early-return tests;
    `RegisterFormEventHandlers` exclusion-control `Verify`.
- Invariant validation: existing `QfcFormControllerTests` behavior assertions must continue
  to pass after the seam migration (no behavioral change).
- Edge/negative: null `CaptureTlpCellStates()`; null `L1v0L2L3v_TableLayout` RowStyles
  early-return path.
- Coverage targets: new non-exempt code (`QfcFormKeyHandler`) >= 90%; changed
  `QfcFormController` lines no coverage regression; repo-wide >= 80%. Form implementations of
  the new members remain `[ExcludeFromCodeCoverage]`.
- Toolchain (in order): `csharpier .` → `msbuild ... /p:EnableNETAnalyzers=true
  /p:EnforceCodeStyleInBuild=true` → `msbuild ... /p:Nullable=enable
  /p:TreatWarningsAsErrors=true` → `vstest.console.exe <QuickFiler.Test assembly>
  /EnableCodeCoverage`.
- Manual validation: none required (structural refactor; behavior preserved).

## Definition of Done

- [ ] Structure matches this spec; legacy paths retired or redirected
- [ ] Invariants validated with tests or comparisons
- [ ] Imports/tooling/entry points updated
- [ ] Edge cases and error handling verified
- [ ] Tests, linting, and type checks clean
- [ ] Docs updated (initiative/README/tasks as needed)
- [ ] Toolchain pass completed (format → lint → type-check → test)

## Seeded Test Conditions (from potential)
- [ ] Unit coverage: `IsAltKeyCommand` for `Keys.Alt`, `Keys.Alt | Keys.Left`,
- [ ] `Keys.Control`, `Keys.None`.
- [ ] Unit coverage: command-event routing (`OkClicked`/`CancelClicked`/`UndoClicked`/
- [ ] `SkipClicked`/`ItemsPerLoadValueChanged`) via Moq `Raise`.
- [ ] Unit coverage: skip flow state transitions; `CaptureItemSettings` populated vs. null
- [ ] vs. early-return (null RowStyles) paths; exclusion-control usage in
- [ ] `RegisterFormEventHandlers`.
- [ ] No temporary files; deterministic; MSTest + Moq + FluentAssertions only.
