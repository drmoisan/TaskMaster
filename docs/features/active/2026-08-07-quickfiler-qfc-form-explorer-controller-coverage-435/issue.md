# quickfiler-qfc-form-explorer-controller-coverage (Issue #435)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-qfc-form-explorer-controller-coverage/ (Issue #435)
- Parent epic issue: #136 (QuickFiler per-file 80% coverage)
- Epic manifest: `docs/features/epics/quickfiler-per-file-coverage/epic.md` (child F6, wave 1, band C3)
- Integration branch: `epic/quickfiler-per-file-coverage-integration`

- Issue: #435
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/435
- Last Updated: 2026-08-08
- Work Mode: full-feature

## Problem / Why

Child F6 of epic #136 owns the `QfcFormController` partial-class family plus `QfcExplorerController`
and their interface declarations — 10 compiled files, approximately 1,611 lines in
`QuickFiler/QuickFiler.csproj`. Today this cluster does not meet the per-file 80% line-coverage floor
mandated by issue #136:

- `QuickFiler/Controllers/QfcExplorerController.cs` (323 lines) carries `[ExcludeFromCodeCoverage]`
  and has no tests at all. Per the epic's Shared Design section 1, that attribute is treated as
  unratified: the CLAUDE.md COM/VSTO exemption qualifier "without an injectable seam" is a live
  obligation, not standing permission, so the attribute must be removed and the file covered through
  seam extraction unless F1's ledger ratifies a specific irreducible remainder.
- The four `QfcFormController.*` partials (196 + 399 + 302 + 232 lines) have partial coverage from
  `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` and `QfcFormControllerSeamTests.cs`, but
  the event-handler and setup/disposal paths cross the form/viewer boundary and are the least
  reachable.
- Actual current per-file coverage for each of the ten files is unmeasured; the epic mandates
  numeric per-file evidence rather than aggregate assembly coverage.

Two distinct files are both named `IQfcFormController.cs` — one under `Controllers/`, one under
`Interfaces/`. The duplication is unexplained and needs a recorded determination.

## Proposed Behavior

Raise every `testable` file in the F6 set to at least 80% line coverage, verified with F1's per-file
coverage harness and recorded as numeric evidence, without changing observable QuickFiler behavior.
Where a path is unreachable from a deterministic unit test, introduce a seam (interface seam first,
then injectable delegate, then adapter) rather than exempting the file. Extend the existing
`IQfcFormViewer` interface rather than inventing a parallel viewer abstraction.

## Acceptance Criteria (early draft)

- [ ] Every `testable` file in the F6 set reaches >= 80% line coverage, verified with F1's per-file
      harness and recorded as numeric evidence under `<FEATURE>/evidence/qa-gates/`.
- [ ] `QfcExplorerController.cs` has `[ExcludeFromCodeCoverage]` removed and reaches the floor via
      seam extraction, unless F1's ledger ratifies a specific irreducible remainder.
- [ ] No production file in scope exceeds 500 lines.
- [ ] Tests use MSTest, Moq, and FluentAssertions; deterministic, isolated, no temporary files, no
      external services, no live forms.
- [ ] Coverage per file spans positive path plus invalid-input, boundary, and error-handling
      behavior.
- [ ] The full C# toolchain passes in final form: csharpier, analyzer build, nullable build,
      coverage-enabled vstest.
- [ ] No behavior change to observable QuickFiler flows.

## Constraints & Risks

- `QfcExplorerController` depends on the Outlook `Explorer`/`MAPIFolder` interop surface; the form
  controller's event handlers and disposal paths cross the form/viewer boundary.
- `Viewers/QfcFormViewer.cs` (the concrete `IQfcFormViewer` implementation) belongs to sibling F15
  and must not be edited. Any required interface growth must be designed so the concrete viewer needs
  no edit, or the required edit must be recorded in `spec.md` as a cross-child contract note.
- `Controllers/KeyboardHandler.cs` belongs to sibling F3 and must not be edited; this controller only
  consumes it.
- `coverage.config` and shared build property files belong to F1 and must not be modified here.
- Setup and disposal carry state-transition invariants (double-dispose, dispose-before-setup).
- Depends on F1 (`quickfiler-coverage-ledger`, wave 0) for the per-file measurement harness and the
  ratified exemption ledger at
  `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`.

## Test Conditions to Consider

- [ ] Unit coverage areas: form-controller construction/initialization, event-handler dispatch,
      action methods, setup and disposal ordering, explorer-controller folder/selection behavior.
- [ ] State transitions: double-dispose, dispose-before-setup, repeated setup.
- [ ] Error handling: null and invalid dependencies, viewer callbacks that throw.
- [ ] No integration, CLI, or API surface in scope.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/quickfiler-qfc-form-explorer-controller-coverage/` folder from the template
