# quickfiler-form-viewers-bayesian-coverage (Issue #496)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-08-08-quickfiler-form-viewers-bayesian-coverage-496/ (Issue #496)
- Parent epic issue: [#136](https://github.com/drmoisan/TaskMaster/issues/136)
- Epic: `quickfiler-per-file-coverage` (child F15, wave 1)
- Integration branch: `epic/quickfiler-per-file-coverage-integration`

- Issue: #496
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/496
- Last Updated: 2026-08-08
- Work Mode: full-feature

## Problem / Why

Child F15 of epic #136 owns the small WinForms viewers, the Bayesian performance pair, and the
generated `Properties/` remainder of `QuickFiler.csproj`. Three testable files sit well below both
the 80% line and 75% branch coverage floors mandated by issue #136
(`Controllers/BayesianPerformanceController.cs` at 66.0%/57.1%,
`Viewers/BayesianPerformanceViewer.cs` at 54.3%/12.5%, `Viewers/ToolStripMenuItemCb.cs` at
61.5%/50.0%), and two files carry an unratified `[ExcludeFromCodeCoverage]` attribute
(`Viewers/QfcFormViewer.cs`, `Viewers/QfcItemViewerExpanded.cs`) that must be judged against F1's
irreducible-remainder standard rather than assumed. F15 also owns the classification of seven
generated `*.Designer.cs`/`Properties/` files that the epic's ledger denominator depends on.

## Proposed Behavior

Raise every file in the F15 set that F1's ledger classifies `testable` to >= 80% line and >= 75%
branch coverage using seams (interface seam > injectable delegate > adapter), remove or ratify the
two existing `[ExcludeFromCodeCoverage]` attributes against F1's ledger, and classify the seven
generated/`Properties/` files per the ledger's rules - without changing observable QuickFiler
behavior and without editing sibling F6's `IQfcFormViewer` consumption contract.

## Acceptance Criteria

- [ ] Every `testable` file in the F15 set reaches >= 80% line AND >= 75% branch coverage, measured
      with F1's per-file harness, recorded under `<FEATURE>/evidence/qa-gates/`.
- [ ] The two `[ExcludeFromCodeCoverage]` attributes are removed with the code covered, or retained
      only where F1's ledger ratifies an irreducible remainder against one of the four ratified
      grounds.
- [ ] Generated `*.Designer.cs` and `Properties/` files are classified `measured-not-gated` per
      epic Ruling DEC-5: measured and counted toward totals, not individually gated, carrying no
      attribute. No shape-assertion tests are written to manufacture coverage for generated files.
- [ ] A file with `branches-valid = 0` reports branch coverage as N/A, never 0%, and never fails.
- [ ] Repository-wide coverage is retained or improved, measured as a self-consistent before/after
      pair on this branch.
- [ ] No production file exceeds 500 lines (generated Designer files exempt); any new file created
      reaches >= 90% line coverage.
- [ ] Full C# toolchain (csharpier, analyzers, nullable, MSTest with coverage) green.
- [ ] No behavior change to observable QuickFiler flows.

## Constraints & Risks

- `QfcFormViewer.cs` implements `IQfcFormViewer`, consumed by sibling F6 (#435, already prepared);
  F6's plan expects no viewer-side edit - preserve that. Read F6's `spec.md` first; record any
  unavoidable interface change as a cross-child contract note rather than editing F6's files.
- Form-derived types: epic Ruling DEC-1 applies directly - unshown Form construction on an STA
  thread, disposed in `finally`, is ratified, reusing the `RunWithViewer` harness shape from
  `QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs` verbatim. STA-bound
  tests live in dedicated `*.StaTests.cs` files. Never call `.Show()`/`.ShowDialog()` or pump a
  message loop.
- `BayesianPerformanceViewer.cs` (12.5% branch) and `ToolStripMenuItemCb.cs` (50.0% branch) need
  untaken-guard and error-path coverage, not more happy-path tests.
- Any thin-forwarder adapter must be a class-level-exempt, `sealed`, non-`partial` type per the
  epic's `#457` lambda-suppression trap.
- `csharpier` is pinned at 1.2.6 and requires the `format` subcommand
  (`dotnet tool run csharpier format .`).
- A CRLF plan is acceptable; the MCP plan validator accepts it. Do not normalize line endings.
- Include a NuGet restore in Phase 0 - `packages/` is gitignored and msbuild does not restore
  `packages.config` projects.

## Test Conditions

- [ ] Unit coverage areas: `BayesianPerformanceController.cs` scoring/threshold branches,
      `BayesianPerformanceViewer.cs` UI-update guards, `ToolStripMenuItemCb.cs` checked-state and
      click-forwarding branches, `QfcFormViewer.cs` and `QfcItemViewerExpanded.cs` seams.
- [ ] Integration scenarios: none - internal testability change only.
- [ ] CLI/API examples: none.

## Source

From: docs/features/potential/2026-08-08-quickfiler-form-viewers-bayesian-coverage.md (promotion
receipt; source potential document was not persisted to disk under this checkout - see epic memory
note on promotion-potential-md-may-not-persist).
