# cobertura-max-hits-update-branch-untested (Issue #537)

- Date captured: 2026-08-10
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/cobertura-max-hits-update-branch-untested/ (Issue #537)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #537
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/537
- Last Updated: 2026-08-11
## Summary

The `max(hits)` update assignment in `Get-CoberturaClassLineSummary` is exercised by no test, so the deduplication rule is pinned only for the first-entry-wins ordering and a regression to first-wins would pass the suite.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (PowerShell / Pester 5)
- Command/flags used: `Invoke-Pester tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 -CodeCoverage scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`
- Data source or fixture: `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:220`

## Steps to Reproduce

1. Check out the branch that landed #441 / #478.
2. Run the Pester suite for `Invoke-MSTestWithCoverage.Helpers.Tests.ps1` with code coverage over `Invoke-MSTestWithCoverage.Helpers.ps1`.
3. Inspect coverage of the `max(hits)` update assignment inside `Get-CoberturaClassLineSummary`.

## Expected Behavior

Every branch of the stated deduplication contract should be pinned by a test. `Get-CoberturaClassLineSummary` documents `max(hits)` resolution for duplicate line numbers, so both orderings should be covered: the case where the first entry already carries the larger hits value, and the case where a later entry carries a strictly larger value and must overwrite it.

## Actual Behavior

Only the first ordering is tested. Fixture F4 presents line 5 with `hits=1` in `.ctor ()` and `hits=0` in `.ctor (int)`, so the maximum is established by the first entry encountered and the update assignment at line 220 never executes. Coverage confirms it: new code measures 39/40 covered statements, and the single uncovered statement is that assignment. A regression that replaced `max(hits)` with first-entry-wins would leave the suite fully green.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet: new-code coverage 39/40 = 97.50%; the one uncovered statement is the `max(hits)` update at `Helpers.ps1:220`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

No defect in current behavior — the implementation is correct and the arithmetic oracle (79957 / 56124 / 23109 / 13472) is reproduced exactly. This is a test-adequacy gap on the headline `max(hits)` semantics of #441, raised in that feature's review as non-blocking finding NF-1. New-code coverage of 97.50% already clears the `>= 90%` floor, so no gate is failing.

## Suspected Cause / Notes

Fixture F4 was designed to prove deduplication happens at all, and its `hits` ordering happens to make the update path unreachable. The gap was recorded explicitly rather than concealed in `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/evidence/other/ac-status-summary.2026-08-10T23-30.md` and in the review's finding NF-1.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: add one fixture where the class-level rollup carries `hits="0"` for a line and a method-level entry carries `hits="1"` for the same line, asserting `CoveredLines = 1`.
- [x] Integration scenario to retest: confirm new-code statement coverage reaches 40/40 and that a deliberate first-entry-wins mutation now fails the suite.
- [x] Manual verification notes: the fixture must follow the existing pattern — inline single-quoted here-string, no file on disk, no mock, explicit `-ProjectNames` — and respect the per-block line budgets in the #441 plan's § Test-File Line Budget.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
