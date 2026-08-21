# quickfiler-itemviewer-coverage (Potential — Promoted)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> https://github.com/drmoisan/TaskMaster/issues/456
- Active folder: `docs/features/active/2026-08-07-quickfiler-itemviewer-coverage-456/`
- Parent epic issue: #136 (QuickFiler Per-File 80% Coverage)
- Epic child: F14 of `quickfiler-per-file-coverage`
- Depends on: F1 `quickfiler-coverage-ledger` (issue #432, wave 0)

> Reconstructed for the audit trail. The MCP `potential_to_issue` receipt reported this
> `destination_path`, but the file was not present on disk afterward. The issue and the active
> feature folder were both created successfully.

## Problem / Why

The `ItemViewer` form family under `QuickFiler/Viewers/` is entirely invisible to coverage
measurement. `ItemViewer` is a partial type spread across six hand-written source files plus a
6,224-line generated designer file, and the single `[ExcludeFromCodeCoverage]` attribute on
`ItemViewer.cs:20` suppresses instrumentation for the whole type. The only member of the family that
is measured, `ItemViewerExpanded.cs`, sits at 37.74% line and 8.33% branch coverage — below both the
80% line and 75% branch gates that issue #136 and `.claude/rules/general-unit-test.md` set.

Under the epic's ratified policy reconciliation, `[ExcludeFromCodeCoverage]` on a testable seam is a
Blocking finding. The attribute on `ItemViewer.cs` has never been argued against the
irreducible-remainder standard, so the family is unratified exempt rather than legitimately exempt.

## Proposed Behavior

Bring every `testable` file in the `ItemViewer` family to at least 80% line and 75% branch coverage,
verified with F1's per-file harness, by extracting host-neutral seams from the `UserControl`-derived
partials and removing the type-level `[ExcludeFromCodeCoverage]` attribute. Classify `IItemViewer.cs`
as `interface-only / not-measured`. No observable behavior change to QuickFiler flows.

## Acceptance Criteria (early draft)

Superseded by the twelve acceptance criteria in
`docs/features/active/2026-08-07-quickfiler-itemviewer-coverage-456/spec.md`.

- [ ] Every `testable` file in the family reaches >= 80% line and >= 75% branch coverage.
- [ ] The `[ExcludeFromCodeCoverage]` attribute on `ItemViewer.cs` is removed, or an irreducible
      remainder is ratified in F1's ledger with a file-specific rationale.
- [ ] `IItemViewer.cs` is reported N/A as `interface-only / not-measured`, with no attribute added.
- [ ] The two `*.Designer.cs` files are classified per the ledger's generated-code rules.
- [ ] No production file in scope exceeds 500 lines; created files reach >= 90% line coverage.
- [ ] Full C# toolchain green; repository-wide coverage retained or improved.

## Constraints & Risks

- `ItemViewer` derives from `UserControl`, not `Form`. Research established that no STA test file is
  required: ten existing plain `[TestMethod]`s already construct a live headless `ItemViewer`.
- A partial type may carry `[ExcludeFromCodeCoverage]` on only one part; annotating two parts is
  CS0579. The designer partial therefore cannot be exempted independently of the hand-written
  partials once the attribute is removed.
- Sibling boundaries: drop-down/WebView2 host files belong to F13, breadcrumb bridge and messenger
  files to F12, `QfcItemController.*` to F10, `ToolStripMenuItemCb.cs` to F15.
- `UtilitiesCS` grants no `InternalsVisibleTo` to `QuickFiler.Test`; `QuickFiler` does, so seams may
  be `internal`.

## Next Step

- [x] Promote to GitHub issue (feature request template) — issue #456
- [x] Create `docs/features/active/2026-08-07-quickfiler-itemviewer-coverage-456/` from the template
