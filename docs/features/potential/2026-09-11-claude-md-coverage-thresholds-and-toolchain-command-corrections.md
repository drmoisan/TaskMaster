# claude-md-coverage-thresholds-and-toolchain-command-corrections (Potential Bug)

- Date captured: 2026-09-11
- Author: Dan Moisan
- Status: Draft

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

## Summary

`CLAUDE.md` names a test command the repository deliberately never runs (#828), states coverage thresholds that the maintainer has now settled differently from two rules files (#563, decision recorded 2026-09-11), and cites `.globalconfig` twice as an analyzer-severity source when that file does not exist (#727 sub-finding 5). All three are documentation-only corrections to one file this repository owns.

## Environment

- OS/version: not applicable (documentation)
- Python version: not applicable
- Command/flags used: `CLAUDE.md` lines 390 and 408 (`vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`); UT2 coverage section; C#1 item 2 and C#7
- Data source or fixture: `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`, `.editorconfig`

## Steps to Reproduce

1. Read CUT3 step 4 and the "C# Toolchain" step 4: both name `vstest.console.exe ... /EnableCodeCoverage`. Read `Invoke-MSTestWithCoverage.ps1` lines 19 to 26: the implemented route is an outer `dotnet-coverage collect` wrapping an inner vstest that is never given `/EnableCodeCoverage`, because the built-in collector conflicts with the outer instrumentation.
2. Read UT2: "Repository-wide line coverage must remain >= 80%" on a testable denominator, no branch floor stated. Compare `.claude/rules/general-unit-test.md` (85 line / 75 branch). The maintainer decision on #563 is 80 line, 75 branch, PowerShell 80 line, recorded in CLAUDE.md alone.
3. Search CLAUDE.md for `.globalconfig`: two references (C#1 item 2, C#7). `Test-Path .globalconfig` is false; `.editorconfig` is the actual severity source.

## Expected Behavior

- CUT3 step 4 and the C# Toolchain step 4 name the route actually run: `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (or the VS Code task that wraps it), with a one-line note that `/EnableCodeCoverage` is deliberately omitted from the inner vstest call.
- UT2 states: C# line >= 80%, C# branch >= 75%, PowerShell line >= 80% (Pester measures no branch), new code >= 90%, the three exemption classes unchanged, and one sentence recording that these figures were settled on 2026-09-11 under #563 and that the `.claude/rules/` figures are upstream-owned and not authoritative here.
- Both `.globalconfig` references read `.editorconfig`.

## Actual Behavior

Reviewers citing CUT3 step 4 issue false PARTIAL verdicts (#809 AC6, and every later item citing it); the 80-versus-85 divergence forces a FAIL row on every review at the measured 84.56%; the `.globalconfig` citations point at a file that does not exist.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: none; documentation finding.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

- Only `CLAUDE.md` is in scope. `.claude/rules/csharp.md` repeats the same vstest command and the 80% figure but is push-down owned; it is not edited here.
- `Invoke-MSTestWithCoverage.Threshold.ps1` enforces 80 line and no branch check. Adding the 75 branch assertion to the script is the CI gates item's work, not this one's.
- Gate risk: the pre-implementation hook treats `CLAUDE.md` as a root file; the executor's plan must declare it in the Write Set.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: none (documentation).
- [ ] Integration scenario to retest: none.
- [x] Manual verification notes: `Select-String` for `/EnableCodeCoverage` and `.globalconfig` in CLAUDE.md returns zero; the UT2 table states 80 / 75 / 80.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

Closes #828 and #563 on merge.
