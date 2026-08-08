# quickfiler-itemviewer-coverage (Issue #456)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-08-07-quickfiler-itemviewer-coverage-456/ (Issue #456)
- Parent epic issue: #136 (QuickFiler Per-File 80% Coverage)
- Epic child: F14 of `quickfiler-per-file-coverage`
- Depends on: F1 `quickfiler-coverage-denominator-and-exemption-ledger` (wave 0)

- Issue: #456
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/456
- Last Updated: 2026-08-07
- Work Mode: full-feature

## Problem / Why

The `ItemViewer` form family under `QuickFiler/Viewers/` is entirely invisible to coverage
measurement. `ItemViewer` is a partial type spread across six hand-written source files plus a
6,224-line generated designer file, and the single `[ExcludeFromCodeCoverage]` attribute on
`ItemViewer.cs` suppresses instrumentation for the whole type. The only member of the family that
is measured, `ItemViewerExpanded.cs`, sits at 39.0% line and 8.3% branch coverage — below both the
80% line and 75% branch gates that issue #136 and `.claude/rules/general-unit-test.md` set.

Under the epic's ratified policy reconciliation, `[ExcludeFromCodeCoverage]` on a testable seam is a
Blocking finding. The attribute on `ItemViewer.cs` has never been argued against the
irreducible-remainder standard, so the family is unratified exempt rather than legitimately exempt.

## Proposed Behavior

Bring every `testable` file in the `ItemViewer` family to at least 80% line and 75% branch
coverage, verified with F1's per-file harness, by extracting host-neutral seams from the
`UserControl`-derived partials and removing the type-level `[ExcludeFromCodeCoverage]` attribute.
Classify `IItemViewer.cs` as `interface-only / not-measured` and the two `*.Designer.cs` files per
F1's ledger rules. No observable behavior change to QuickFiler flows.

## Acceptance Criteria (early draft)

- [ ] Every `testable` file in the family reaches >= 80% line and >= 75% branch coverage.
- [ ] The `[ExcludeFromCodeCoverage]` attribute on `ItemViewer.cs` is removed, or an irreducible
      remainder is ratified in F1's ledger with a file-specific rationale.
- [ ] `IItemViewer.cs` is reported N/A as `interface-only / not-measured`, with no attribute added.
- [ ] The two `*.Designer.cs` files are classified per the ledger's generated-code rules.
- [ ] No production file in scope exceeds 500 lines; created files reach >= 90% line coverage.
- [ ] Full C# toolchain green; repository-wide coverage retained or improved.

## Constraints & Risks

- `ItemViewer` derives from `UserControl`, so the epic's STA last-resort clause for never-shown
  in-memory controls is available, but only after seam extraction has been exhausted.
- A partial type may carry `[ExcludeFromCodeCoverage]` on only one part; annotating two parts is
  CS0579. The designer partial therefore cannot be exempted independently of the hand-written
  partials once the attribute is removed.
- `ItemViewer.WebViewThread.cs` and `ItemViewer.Breadcrumb.cs` cross the WebView2 thread boundary
  and carry ordering invariants; tests must use an injected clock and fake timers.
- Sibling boundaries: drop-down/WebView2 host files belong to F13, breadcrumb bridge and messenger
  files to F12, `QfcItemController.*` to F10. None may be edited by this child.
- `UtilitiesCS` grants no `InternalsVisibleTo` to `QuickFiler.Test`; any seam must be local.

## Test Conditions to Consider

- [ ] Unit coverage for command dispatch, display-state transitions, and folder-search filtering.
- [ ] Breadcrumb ordering invariants across the WebView2 thread boundary with a fake clock.
- [ ] Expanded-viewer population and teardown paths currently at 39.0% line / 8.3% branch.
- [ ] STA-scoped construction of the never-shown control, in a dedicated `*.StaTests.cs` file, only
      where no seam can isolate the logic.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/quickfiler-itemviewer-coverage/` folder from the template
