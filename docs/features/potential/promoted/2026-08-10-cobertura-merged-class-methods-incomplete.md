# cobertura-merged-class-methods-incomplete (Issue #530)

- Date captured: 2026-08-10
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/cobertura-merged-class-methods-incomplete/ (Issue #530)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #530
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/530
- Last Updated: 2026-08-11
## Summary

A merged Cobertura class retains only the primary class's `<methods>`, so the emitted document's method-level lines do not account for all of that class's class-level lines.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (PowerShell)
- Command/flags used: `ConvertTo-KoverageCoberturaXml` / `Merge-CoberturaClassesByFilename` in `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`
- Data source or fixture: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`

## Steps to Reproduce

1. Dot-source `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`.
2. Reprocess a Cobertura document containing two or more `<class>` elements that share a `filename` (for example `Ns.Foo` and its compiler-generated partner `Ns.Foo.<>c`).
3. Inspect the merged class: compare the count of `<line>` children in its class-level `<lines>` rollup against the set of lines described by its `<methods>` subtree.

## Expected Behavior

The merged class's `<methods>` subtree should describe the same source lines as its merged class-level `<lines>` rollup, or the divergence should be an explicitly documented and tested contract.

## Actual Behavior

`Merge-CoberturaClassesByFilename` unions the class-level `<lines>` of same-filename classes but leaves `<methods>` un-merged; the merged class carries only the primary class's `<method>` children. For `QuickFiler\Controllers\QfcHomeController.Iteration.cs` in the `-424` sample, the merged class-level rollup has 56 lines while the retained `<methods>` describe only 24.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet: merged class-level rollup 56 lines vs. retained `<methods>` 24 lines for `QfcHomeController.Iteration.cs`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Per-method `line-rate` data in the emitted document is incomplete for merged classes. Root and per-file line rates are correct as of #441 / #478.

## Suspected Cause / Notes

This was deliberately **not** fixed in #441 / #478, for recorded reasons:

- Sibling classes sharing a filename are compiler-generated partners (`Foo` and `Foo.<>c`, async state machines) that routinely both declare `name=".ctor" signature="()"`. Appending sibling `<method>` elements produces duplicate `(name, signature)` pairs, breaking any consumer that keys methods that way — including the per-method `line-rate` technique this repository uses for coverage-delta work.
- Deduplicating by `(name, signature)` would be worse: it discards genuinely distinct methods.
- Stripping `<methods>` was rejected outright: it destroys per-method `line-rate` data that coverage-delta work actively relies on.

Fixture F6 in `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` currently **pins** the existing behaviour (methods neither merged nor stripped). Any fix here must update F6 deliberately, not incidentally.

Recorded as follow-up candidate 2 in `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/spec.md` § Rollout & Follow-up.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: a fixture asserting the merged class's method-described line set against its class-level rollup, with an explicit disambiguation rule for duplicate `(name, signature)` pairs.
- [x] Integration scenario to retest: reprocess the `-424` sample and confirm no consumer keyed on `(name, signature)` regresses.
- [x] Manual verification notes: F6 must be updated deliberately as part of any fix.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
