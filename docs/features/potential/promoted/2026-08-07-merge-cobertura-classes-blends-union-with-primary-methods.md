# merge-cobertura-classes-blends-union-with-primary-methods (Issue #478)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/merge-cobertura-classes-blends-union-with-primary-methods/ (Issue #478)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #478
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/478
- Last Updated: 2026-08-08
## Summary

`Merge-CoberturaClassesByFilename` unions the class-level `<lines>` of all `<class>` elements sharing
a `filename` correctly, but never merges the corresponding `<methods>` subtrees. It then recomputes
`line-rate` over the descendant axis `.//lines/line`, which sees the correct union **plus** only the
primary class's method-level lines. The emitted per-file `line-rate` is therefore a blend of two
different denominators and matches neither. This is distinct from, and additional to, issue #441.

## Environment

- OS/version: n/a (PowerShell post-processing defect, reproducible from committed data)
- Python version: n/a
- Command/flags used: `scripts/vscode/Invoke-MSTestWithCoverage.ps1` post-processing path
- Data source or fixture:
  `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`

## Steps to Reproduce

1. Inspect `Merge-CoberturaClassesByFilename` in
   `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, beginning at `:167`.
2. Observe the class-level `<lines>` union at `:217-268`. It correctly takes max hits per line number
   across every `<class>` element sharing a `filename`.
3. Observe that the `<methods>` subtrees of the non-primary group members are **not** merged into the
   primary class element.
4. Observe the rate recomputation, which selects over the descendant axis `.//lines/line`. That axis
   matches both the merged class-level `<lines>` and the *unmerged, primary-only* `<methods>/<method>/<lines>/<line>`.
5. Verify arithmetically against `QfcHomeController.Iteration.cs` in the committed report: the true
   per-file figure computed from the class-level union alone is **45/56 = 80.36%**, while the emitted
   attribute is `0.8625`, which is exactly **69/80** — the blended numerator and denominator.
6. Cross-check the recipe independently: `#424`'s own delta evidence
   (`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-delta.2026-08-07T00-48.md:10,41`)
   arrived at the class-level-only computation, confirming that the attribute is not the right source.

## Expected Behavior

The emitted per-file `line-rate` should equal the rate computed from the merged class-level `<lines>`
set alone: distinct line numbers, max hits per number, hit count over total count.

## Actual Behavior

The emitted `line-rate` mixes the merged class-level line set with the primary class's method-level
line set, producing a figure that corresponds to neither the true per-file rate nor any single
class's rate.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: `QfcHomeController.Iteration.cs` — true per-file `45/56 = 0.8036`; emitted attribute
  `0.8625 = 69/80`. Confirmed against `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:167`
  (function start) and `:217-268` (the correct union). Discovered during preparation research for
  issue #454 (epic #136, child F11); full analysis in
  `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/research/coverage-harness-contract.md`
  section A.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Any consumer reading the per-file `line-rate` attribute gets a wrong number, and the error is not in
a consistent direction because it depends on how much of a file's coverage sits in the primary class
versus its siblings. Epic #136 gates every one of its fifteen children on a per-file line rate, so an
uncorrected attribute would produce false passes and false failures across the whole epic.

## Suspected Cause / Notes

The union logic and the rate recomputation were written against different assumptions: the union
operates on the class-level `<lines>` element specifically, while the recomputation reuses the
same `.//lines/line` descendant axis that issue #441 identifies as the source of the double-count.
Fixing #441's axis alone would **not** fix this defect; the `<methods>` merge is separately missing.

Related: this is why epic #136's harness directive tells children to recompute per-file rates from
deduplicated `<line>` nodes and never to read the `<class>` `line-rate` attribute. That guidance is
correct and this issue documents the underlying reason.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: a Pester case with two `<class>` elements sharing one `filename`, each
      with its own `<methods>`, asserting the merged `line-rate` equals the class-level union rate.
- [x] Integration scenario to retest: re-run post-processing over the committed `#424` report and
      assert `QfcHomeController.Iteration.cs` reports `0.8036`, not `0.8625`.
- [x] Manual verification notes: fix alongside #441, since both live in the same two functions and a
      partial fix would leave the attribute wrong in a different way. Re-capture any committed
      coverage baseline that was derived from the attribute.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
