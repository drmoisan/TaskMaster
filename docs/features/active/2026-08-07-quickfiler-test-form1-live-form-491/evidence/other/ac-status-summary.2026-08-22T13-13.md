Timestamp: 2026-08-22T13-13

### Acceptance Criteria Status
- Source: `docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/spec.md`
- Total AC items: 11
- Checked off (delivered): 7
- Remaining (unchecked): 4
- Items remaining:
  1. No `System.Windows.Forms.Form`-derived type is compiled into the `QuickFiler.Test` assembly, proven by a named MSTest guard test — NOT MET: the guard test exists and runs, but fails because of a pre-existing, out-of-scope second Form-derived type (`QuickFiler.Controllers.Tests.QfcHomeControllerTests+QfcFormViewerDerived`, extending production type `QuickFiler.Viewers.QfcFormViewer : Form`). See `evidence/regression-testing/phase1-guard-red.2026-08-22T13-13.md` and `evidence/qa-gates/phase3-guard-green.2026-08-22T13-13.md`.
  2. `vstest.console.exe` run with coverage/isolation/`LiveOutlook` filter completes with zero failing tests — NOT MET: one failing test (the new guard test, for the same reason as item 1). See `evidence/qa-gates/phase3-vstest.2026-08-22T13-13.md`.
  3. No pre-existing `QuickFiler.Test` test regresses; test-count and pass-count parity apart from the one new guard test — PARTIALLY TRUE BUT LEFT UNCHECKED: no pre-existing test actually regressed (post-change total is exactly baseline + 1, and the baseline's own unrelated flaky failure did not recur), but the new guard test itself is the one post-change failure, so the AC is not checked off pending resolution of item 1. See `evidence/qa-gates/phase4-test-count-parity.2026-08-22T13-13.md`.
  4. Post-change line coverage >= baseline line coverage, both recorded as actual numbers — NOT MET: no valid post-change coverage figure could be captured, because the coverage-capture harness (`Invoke-MSTestWithCoverage.ps1`) throws before completing Koverage post-processing whenever any test fails (the same guard-test failure as item 1). See `evidence/qa-gates/phase4-coverage-capture.2026-08-22T13-13.md`, `phase4-coverage-postchange.2026-08-22T13-13.md`, and `phase4-coverage-comparison.2026-08-22T13-13.md`.

Note on `issue.md`: the three-item `## Acceptance Criteria (early draft)` block in `issue.md` is an early draft superseded by the 11 criteria in `spec.md` under the `full-bug` work mode; its three unchecked boxes are not unmet acceptance criteria for this plan and should not be read as such by a later reviewer.

All four remaining items trace to a single root cause discovered during Phase 1 execution: `QuickFiler.Controllers.Tests.QfcHomeControllerTests` (line 243) declares a nested class `QfcFormViewerDerived : QfcFormViewer`, and `QfcFormViewer` is itself `: Form` (production type, `QuickFiler/Viewers/QfcFormViewer.cs:18`). This second Form-derived type was not discovered during this plan's preflight research (which searched specifically for the literal `Form1`, not for all Form-derived types generally) and is outside the file set this plan owns (`Form1.cs`, `Form1.Designer.cs`, `Form1.resx`, `QuickFiler.Test.csproj`, `NoLiveFormInTestAssemblyTests.cs`). It is fully documented in `evidence/regression-testing/phase1-guard-red.2026-08-22T13-13.md`.
