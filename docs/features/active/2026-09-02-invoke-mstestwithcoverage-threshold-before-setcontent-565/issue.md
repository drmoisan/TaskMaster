# invoke-mstestwithcoverage-threshold-before-setcontent (Potential Bug)

- Date captured: 2026-09-02
- Author: Dan Moisan
- Status: Already promoted (GitHub issue #565 opened 2026-08-15, prior to this session)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #565
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/565
- Last Updated: 2026-09-02

- Work Mode: full-bug

## Summary

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` asserts the coverage threshold before it writes the
post-processed Cobertura document to disk. When the assertion fails, the script throws and the
post-processed document is discarded, leaving the raw un-post-processed document at the output path.

## Environment

- OS/version: Windows 11 Pro (repo default)
- Python version: n/a — PowerShell 7+ coverage/test-runner scripts
- Command/flags used: `scripts/vscode/Invoke-MSTestWithCoverage.ps1`
- Data source or fixture: n/a

## Steps to Reproduce

1. Run `scripts/vscode/Invoke-MSTestWithCoverage.ps1` against a test suite whose measured line
   coverage is below the configured 80% threshold.
2. Observe that `Assert-CoberturaLineCoverageThreshold` (in `Invoke-MSTestWithCoverage.Helpers.ps1`)
   throws before the post-processed XML is persisted.
3. Inspect the coverage output path named by `-CoverageOutput` after the throw.

## Expected Behavior

The artifact on disk should be the same post-processed Cobertura document that the threshold
assertion judged, in both the passing and failing case.

## Actual Behavior

At `scripts/vscode/Invoke-MSTestWithCoverage.ps1:341-343` the threshold assertion runs ahead of the
`Set-Content` that persists the post-processed XML. On a failing run, the artifact left on disk is
the raw `dotnet-coverage` output — absolute paths, third-party packages included, unmerged duplicate
classes, and the double-counted line totals that #441 corrected.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: n/a — see file/line citation above.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Only the failure path is affected; a passing run writes the correct document. But the failure path is
exactly when someone reads the artifact to diagnose the shortfall, and what they find is a document
with different numbers than the one that produced the failure message. It also means a failed gate
leaves behind an artifact that, if fed to any downstream consumer, reports the pre-#441 inflated
denominator.

## Suspected Cause / Notes

Statement ordering defect only, not a logic change. Found during the `build-ci-coverage-gate-fidelity`
epic fan-in review; identified independently by two review passes. Note: issue #563 (threshold VALUE
contradiction) is a separate, deliberately excluded concern — this fix must not change the threshold
value, only the statement order.

## Proposed Fix / Validation Ideas

- [x] Move the `Set-Content` above the `Assert-CoberturaLineCoverageThreshold` call so the judged
      document is persisted before the threshold is evaluated.
- [ ] Add a Pester test under `tests/scripts/vscode/` asserting that a sub-threshold run still leaves
      the post-processed document on disk (not the raw `dotnet-coverage` output).

## Next Step

- [x] Promote to GitHub issue (bug-report template) — already promoted as issue #565.
- [ ] Move to active fix folder / branch
