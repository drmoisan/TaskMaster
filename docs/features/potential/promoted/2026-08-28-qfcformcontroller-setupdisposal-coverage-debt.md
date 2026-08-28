# qfcformcontroller-setupdisposal-coverage-debt (Issue #683)

- Date captured: 2026-08-28
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfcformcontroller-setupdisposal-coverage-debt/ (Issue #683)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #683
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/683
- Last Updated: 2026-08-28
## Summary

`QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` has pre-existing whole-file line coverage of 70.70%, below this repository's 80% line-coverage floor, though none of the uncovered lines were touched by issue #677's changes.

## Environment

- OS/version: Windows, .NET Framework / VSTO toolchain
- Command/flags used: `vstest.console.exe ... /EnableCodeCoverage` against `QuickFiler.Test.dll`
- Data source or fixture: Cobertura coverage report from the issue #677 feature-review run

## Steps to Reproduce

1. Run the full `QuickFiler.Test` suite with code coverage enabled.
2. Inspect the Cobertura report for `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs`.

## Expected Behavior

Per this repository's coverage policy (CLAUDE.md, repository-wide line coverage >= 80%), this file should meet or exceed the 80% line-coverage floor.

## Actual Behavior

The file's whole-file line coverage is 70.70%, with 46 lines uncovered. All 46 uncovered lines were verified pre-existing at the issue #677 merge-base baseline (not introduced or touched by that fix); the two lines #677 added to this file are both 100% covered.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: N/A — see `docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/feature-audit.2026-08-28T12-31.md` section covering the coverage table for the dispositioned FAIL row.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Low

## Suspected Cause / Notes

Discovered during feature review of issue #677 (`docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/policy-audit.2026-08-28T12-31.md`, section 8 and section 5's coverage table). Dispositioned non-blocking for #677 because the gap is pre-existing and #677's own changed lines in this file are fully covered — but it remains outstanding repository-wide coverage debt that should be closed with a dedicated test-coverage pass over this file's untested lines (likely disposal/setup edge cases and event-handler branches not currently exercised by `QuickFiler.Test`).

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: identify and add MSTest/Moq/FluentAssertions coverage for the 46 currently-uncovered lines in `QfcFormController.SetupDisposal.cs`
- [ ] Integration scenario to retest: re-run full-suite coverage and confirm the file's whole-file line coverage reaches >= 80%
- [ ] Manual verification notes: none required (pure test-addition work)

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
