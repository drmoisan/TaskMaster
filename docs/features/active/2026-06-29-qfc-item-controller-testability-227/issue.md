# qfc-item-controller-testability (Issue #227)

- Date captured: 2026-06-29
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-item-controller-testability/ (Issue #227)
- Type: refactor (testability)

- Issue: #227
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/227
- Last Updated: 2026-06-29
- Work Mode: full-feature

## Problem / Why

`QuickFiler/Controllers/QfcItemController.cs` is approximately 2,498 lines — far above the
repository 500-line file cap — and has roughly 5% line coverage (74 of 1,288 lines). It is
the controller for `QuickFiler/Viewers/ItemViewer.cs`, whose interface
`QuickFiler/Viewers/IItemViewer.cs` re-exposes raw WinForms control types (e.g.
`ButtonSVG`, `ComboBox`, `TableLayoutPanel`, `Label`, `TextBox`, `WebView2`,
`FastObjectListView`) directly to consumers. Because the controller couples directly to
these UI types and to Outlook Interop objects, its logic cannot be unit-tested without a
live Outlook process and window handles. This is the same coupling pattern remediated for
`QfcFormViewer`/`IQfcFormViewer` under issue #223.

## Proposed Behavior

Apply the issue #223 strategy to these three files:

1. Split `QfcItemController` along logical responsibility groupings into partial-class files
   each under 500 lines, with a logical class structure (e.g. construction/initialization,
   control resolution & population, conversation rendering, folder handling, event wiring &
   UI event handlers, navigation/keyboard, toggles, theming, mail actions, properties).
2. Narrow `IItemViewer` to intent-level members (command events, state properties, intent
   methods) in place of raw clickable/raw WinForms control types where consumers allow,
   and add seams where pure logic can be extracted from Form/UserControl-bound code.
3. Keep Form-derived, Designer-generated, and UserControl-derived code
   `[ExcludeFromCodeCoverage]` per the repository COM/VSTO/WinForms exemption.
4. Reorganize `QuickFiler.Test/Controllers/QfcItemControllerTests.cs` into test files that
   mirror the new partial-class structure.
5. Add MSTest + Moq + FluentAssertions unit tests until coverage of the testable
   denominator for the affected code is at or above 80%.

## Acceptance Criteria (early draft)

- [ ] AC1: `QfcItemController` is split into partial-class files, each under 500 lines, with
  a logical responsibility-based structure; no behavior change.
- [ ] AC2: `IItemViewer` is narrowed toward intent-level members and seams are added where
  pure logic can be extracted to enable controller behavior verification.
- [ ] AC3: Form/Designer/UserControl-derived code carries `[ExcludeFromCodeCoverage]` per
  the repo exemption; testable seams are NOT exempt.
- [ ] AC4: Test files mirror the new partial-class structure (one test file per cluster).
- [ ] AC5: Coverage of the testable denominator for the affected code is >= 80%; new code
  >= 90%; changed lines do not regress.
- [ ] AC6: No production file modified in this cycle exceeds 500 lines after the change.
- [ ] AC7: Full C# toolchain passes in order — csharpier, .NET analyzers,
  nullable/TreatWarningsAsErrors, MSTest with coverage — with no regressions.

## Constraints & Risks

- Large surface area: a 2,498-line controller and a wide view interface; the refactor will
  touch/create many production and test files and is likely to require multiple phases.
- Runtime behavior of the QuickFiler item viewer must be unchanged; this is a structural and
  testability refactor only.
- Outlook Interop coupling (`MailItem`, `MAPIFolder`, etc.) and WinForms coupling limit
  which clusters are unit-testable; some classes will be legitimately exempted.
- Interface narrowing is a breaking change to `IItemViewer`, updated in-repo across all
  consumers; external consumers must be confirmed absent.

## Test Conditions to Consider

- [ ] Unit coverage of extracted pure logic and seam-routed controller behavior via Moq
  event raising / `VerifySet` / `Verify`.
- [ ] No temporary files; deterministic; MSTest + Moq + FluentAssertions only.

## Next Step

- [ ] Promote to GitHub issue (refactor template)
- [ ] Create `docs/features/active/qfc-item-controller-testability/` folder from the template
