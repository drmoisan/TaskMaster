# quickfiler-efc-form-item-controller-coverage (Potential Feature)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> https://github.com/drmoisan/TaskMaster/issues/452
- Work Mode: full-feature

> Audit-trail note: this file was recreated after promotion. The MCP `potential_to_issue` operation
> reported `destination_path` here and populated the active feature folder's `issue.md`, but the
> potential document itself did not persist on disk. Recreated so the promotion chain is auditable.

## Problem / Why

Three of the four production files in the EFC form/item controller cluster carry a real
`[ExcludeFromCodeCoverage]` attribute, which removes them from instrumentation entirely. They do not
appear in any coverage report, so they are **unmeasured, not covered**:

| File | Lines | Attribute |
| --- | --- | --- |
| `QuickFiler/Controllers/EfcItemController.cs` | 1,170 | Yes (`:25`) |
| `QuickFiler/Controllers/EfcFormController.cs` | 1,086 | Yes (`:27`) |
| `QuickFiler/Viewers/EfcViewer.cs` | 162 | Yes (`:20`) |
| `QuickFiler/Viewers/EfcViewer.Designer.cs` | 4,277 | No (generated; suppressed via the partial type) |

Both controllers also breach the repository 500-line file-size limit.

## Proposed Behavior

Extract injectable seams so the controllers' and viewer's logic is reachable by deterministic unit
tests, split both oversized controllers into cohesive partials under 500 lines, remove the three
exemption attributes, and bring every testable file to the epic's per-file coverage floors with
numeric evidence. No observable QuickFiler behavior changes.

## Acceptance Criteria (early draft)

Superseded by AC1-AC11 in the active feature folder's `spec.md` and `user-story.md`.

- [x] Per-file line coverage >= 80% and branch coverage >= 75%, verified with F1's harness
- [x] `[ExcludeFromCodeCoverage]` removed from all three files, or an irreducible remainder ratified by F1's ledger
- [x] No production file over 500 lines; new files reach >= 90% line coverage
- [x] MSTest + Moq + FluentAssertions; deterministic, isolated, no temp files, no live forms, no popups
- [x] Full C# toolchain green; repository-wide coverage retained or improved
- [x] No behavior change to observable QuickFiler flows

## Constraints & Risks

- Epic child F9 (wave 1) of `quickfiler-per-file-coverage`; parent epic issue #136.
- Depends on F1 `quickfiler-coverage-ledger` (issue #432) for the per-file harness and exemption ledger.
- Sibling-owned files are off-limits: F8's `EfcHomeControllerDependencies.cs` and
  `EfcHomeControllerDependencyFactories.cs`, F4's `EfcThemeHelper.cs` and `EfcViewerQueue.cs`,
  F12's `BreadcrumbBridgeRouter.cs`, F5's `EfcDataModel.cs`.
- `QuickFiler.csproj` and `QuickFiler.Test.csproj` are legacy non-SDK projects with explicit
  `<Compile Include>` entries and CRLF line endings; new files require additive edits that will
  conflict at fan-in.
- Open issue #439 is a live behavior defect in these files that this feature must **not** fix.
- Open issue #441 corrupts Cobertura per-file rate attributes and threatens the numeric evidence.

## Next Step

- [x] Promote to GitHub issue (feature request template) — issue #452
- [x] Create `docs/features/active/2026-08-07-quickfiler-efc-form-item-controller-coverage-452/`
