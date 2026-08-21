# quickfiler-qfc-form-explorer-controller-coverage (Potential — Promoted)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted to issue #435 on 2026-08-07
- Issue: https://github.com/drmoisan/TaskMaster/issues/435
- Active folder: `docs/features/active/2026-08-07-quickfiler-qfc-form-explorer-controller-coverage-435/`
- Parent epic issue: #136 (QuickFiler per-file 80% coverage)
- Epic manifest: `docs/features/epics/quickfiler-per-file-coverage/epic.md` (child F6, wave 1, band C3)
- Integration branch: `epic/quickfiler-per-file-coverage-integration`

> Recreated for the audit trail. The `potential_to_issue` MCP tool reported this destination path in its
> receipt and populated the active folder's `issue.md`, but did not leave the promoted markdown on disk.
> The promotion itself succeeded — issue #435 exists and the active folder was created from it.

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
then injectable delegate, then adapter) rather than exempting the file.

## Acceptance Criteria (early draft)

Superseded by `AC-1` through `AC-7` in the active folder's `spec.md` and `US-1` through `US-7` in
`user-story.md`, which are the authoritative sources under Work Mode `full-feature`.

- [ ] Every `testable` file in the F6 set reaches >= 80% line coverage, verified with F1's per-file
      harness and recorded as numeric evidence under `<FEATURE>/evidence/qa-gates/`.
- [ ] `QfcExplorerController.cs` has `[ExcludeFromCodeCoverage]` removed and reaches the floor via
      seam extraction, unless F1's ledger ratifies a specific irreducible remainder.
- [ ] No production file in scope exceeds 500 lines.
- [ ] Tests use MSTest, Moq, and FluentAssertions; deterministic, isolated, no temporary files, no
      external services, no live forms.
- [ ] Coverage per file spans positive path plus invalid-input, boundary, and error-handling
      behavior.
- [ ] The full C# toolchain passes: csharpier, analyzer build, nullable build, coverage-enabled
      vstest.
- [ ] No behavior change to observable QuickFiler flows.

## Outcome of Preparation

Research surfaced three latent defects and one pre-existing policy violation, each promoted to its
own issue rather than left as prose that would disappear when this feature folder merges:

- **#448** — `QfcFormController.UndoConsumer()` never terminates (`Actions.cs:258`).
- **#449** — `QfcExplorerController` latent defects: `ExplConvView_Cleanup` throws
  `NotImplementedException` on a public interface member; `OpenQFItem` re-resolves the active
  explorer; and a dead 139-line region duplicates code maintained in `UtilitiesCS`/`ToDoModel`.
- **#450** — `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` is 827 lines against the
  repository's 500-line limit.

## Next Step

- [x] Promote to GitHub issue (feature request template) — issue #435
- [x] Create `docs/features/active/2026-08-07-quickfiler-qfc-form-explorer-controller-coverage-435/`
- [x] Research, feature documents, atomic plan, and preflight clearance complete
- [ ] Atomic execution — deferred to `epic-orchestrator`
