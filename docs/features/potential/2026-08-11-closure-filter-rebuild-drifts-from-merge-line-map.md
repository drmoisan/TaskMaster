# closure-filter-rebuild-drifts-from-merge-line-map (Potential Bug)

- Date captured: 2026-08-11
- Author: Dan Moisan
- Status: Draft

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

## Summary

The retained-line rebuild inside `Remove-CoberturaExemptClosureCoverage` duplicates the line-map loop already
implemented in `Merge-CoberturaClassesByFilename`, but the two copies are not equivalent: the filter's copy omits
stale `condition-coverage` removal and does not copy `<conditions>` child elements. Two near-identical loops that
must agree, maintained separately, will drift.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (PowerShell 7 / Pester 5.6.1)
- Command/flags used: `./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
- Data source or fixture: a Cobertura closure class carrying `<conditions>` children and a `condition-coverage`
  attribute on a line that survives partial method removal

## Steps to Reproduce

1. Construct a Cobertura fixture whose closure class carries both exempt and non-exempt methods, where a retained
   line carries a `condition-coverage` attribute and `<conditions>` children.
2. Run `Remove-CoberturaExemptClosureCoverage` over it.
3. Compare the rebuilt `<lines>` against the equivalent output of `Merge-CoberturaClassesByFilename` for the same input shape.

## Expected Behavior

Both rebuild paths produce structurally identical retained-line elements, including branch condition data, so that a
class's branch figures are the same regardless of which code path last rebuilt it.

## Actual Behavior

The filter's rebuild can leave a stale `condition-coverage` attribute in place and drops `<conditions>` children,
so a class rebuilt by the filter and one rebuilt by the merge can disagree on branch data for the same input.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: no captured log; identified by code review of issue #457 (finding CR-1), recorded in
  `docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/code-review.2026-08-11T01-33.md`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

No incorrect figure has been observed in practice; the repository's measured branch rate moved only -0.000102 across
the #457 change. This is recorded as maintainability and drift risk in code that computes a gate input, not as a
demonstrated wrong number.

## Suspected Cause / Notes

The duplication was deliberate and constrained. `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` sat at 455 of
a 500-line ceiling, and spec AC 13 for #457 fixed that file's change surface at exactly two added lines, so
extracting a shared helper into it was not available to that feature. The clean resolution is to lift the shared
line-map rebuild into one function callable from both sites, which is a change to the helpers module that #457 was
explicitly not authorized to make.

Files to inspect: `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` (retained-line rebuild),
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` (`Merge-CoberturaClassesByFilename` line-map loop).

Related: issues #529, #530 and #537 also concern Cobertura post-processing arithmetic and may share a fix surface.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: a shared-helper test asserting identical retained-line output from both call sites, including `condition-coverage` and `<conditions>`
- [ ] Integration scenario to retest: full coverage run, comparing repository branch figures before and after the extraction
- [ ] Manual verification notes: confirm both modules remain under the 500-line ceiling after the extraction

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

Source: code review of issue #457 (finding CR-1), epic `build-ci-coverage-gate-fidelity` wave 1. Deliberately not
absorbed into #457, whose production surface is fixed at two files and exactly two edits by spec AC 13.
