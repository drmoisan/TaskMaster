# cobertura-package-rates-not-recomputed (Issue #529)

- Date captured: 2026-08-10
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/cobertura-package-rates-not-recomputed/ (Issue #529)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #529
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/529
- Last Updated: 2026-08-11
## Summary

Package-level `line-rate` and `branch-rate` are never recomputed after Cobertura package filtering and class merging, so every surviving `<package>` carries a stale rate computed over a different, larger class set.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (PowerShell)
- Command/flags used: `ConvertTo-KoverageCoberturaXml -XmlContent <cobertura> -RepoRoot <root> -PathSeparator '\'`
- Data source or fixture: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`

## Steps to Reproduce

1. Dot-source `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`.
2. Run `ConvertTo-KoverageCoberturaXml` over a raw `dotnet-coverage` Cobertura document that contains `.Test` packages and multiple same-filename classes.
3. Inspect the `line-rate` and `branch-rate` attributes of any surviving `<package>` element in the output and compare them against a rate recomputed from that package's post-filter, post-merge class set.

## Expected Behavior

After package filtering removes `.Test` packages and class merging unions same-filename classes, each surviving `<package>` should carry `line-rate` and `branch-rate` recomputed over its own post-processing class set.

## Actual Behavior

`ConvertTo-KoverageCoberturaXml` writes only the six root `<coverage>` attributes and the `line-rate` / `branch-rate` / `complexity` attributes of merged class elements. It never recomputes `<package line-rate=...>` or `<package branch-rate=...>`, so each package retains the value the generator emitted for a different, larger class set.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet: the stale values are consumed downstream by `scripts/temp-extract-coverage.ps1:47`, which reads `$pkg.'line-rate'` for per-assembly reporting.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Per-assembly reported coverage is wrong wherever filtering or merging changed a package's class set. Repository-wide root attributes are unaffected (those were corrected by #441).

## Suspected Cause / Notes

The rate-recomputation pass in `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` operates at the root and merged-class levels only; the intermediate `<package>` level was never included.

Deliberately out of scope for #441 / #478: recomputing package-level rates widens the diff without serving either issue, and `CLAUDE.md` § Bugfix Workflow step 2 mandates the minimal targeted fix. Recorded as follow-up candidate 1 in `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/spec.md` § Rollout & Follow-up.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: add a Pester fixture asserting a package's `line-rate` after a filter-and-merge pass equals the rate derived from its surviving classes.
- [x] Integration scenario to retest: reprocess the committed `-424` sample and confirm per-package rates agree with a recomputation from the emitted class set.
- [x] Manual verification notes: cross-check against `scripts/temp-extract-coverage.ps1` per-assembly output.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
