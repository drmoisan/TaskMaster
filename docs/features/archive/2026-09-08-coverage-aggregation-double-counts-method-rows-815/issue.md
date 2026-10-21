# coverage-aggregation-double-counts-method-rows (Issue #815)

- Date captured: 2026-09-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/coverage-aggregation-double-counts-method-rows/ (Issue #815)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #815
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/815
- Last Updated: 2026-09-08
- Work Mode: full-bug

## Summary

The coverage aggregation method pinned in atomic plans double-counts method rows when it sums
Cobertura output, so reported first-party coverage is inflated. On issue 809's delivery the plan's
artifacts reported 79.38% first-party branch coverage; recomputing the same run de-duplicated gives
77.03%. The method is reused by other plans, so every gate that depends on it reads high.

## Environment

- OS/version: Windows 11 Pro 10.0.26200, .NET Framework 4.8
- Python version: not applicable
- Command/flags used: the repository coverage route (`dotnet-coverage`), Cobertura output consumed by
  the aggregation step written into atomic plans
- Data source or fixture: the raw Cobertura report from issue 809's final QA gate run

## Steps to Reproduce

1. Run the repository coverage route to produce a Cobertura report.
2. Aggregate first-party branch coverage using the method pinned in a current atomic plan.
3. Recompute the same figure de-duplicating method rows before summing.
4. Compare: the two disagree, with the pinned method reporting the higher value.

## Expected Behavior

A coverage figure quoted in a QA gate equals the figure a de-duplicated recomputation from the same
raw Cobertura report produces. Two methods over one report do not give two answers.

## Actual Behavior

The pinned aggregation counts method rows more than once. Measured on issue 809: 79.38% reported
versus 77.03% recomputed, a 2.35 point overstatement on first-party branch coverage.

Both figures clear the 75% branch threshold, so no verdict changed on 809 and the merge was not
affected. The defect is that the counting method is wrong and is copied forward into other plans,
where a smaller true margin would not survive the same overstatement.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: the recomputation was performed by the feature reviewer for issue 809 from the raw
  Cobertura report committed under that item's `evidence/qa-gates/` tree.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High because it silently corrupts a quality gate across many future plans rather than affecting one
item. A gate that reports a number nobody can reproduce provides no assurance, and the error is in
the optimistic direction, so it fails to stop the cases it exists to stop.

## Suspected Cause / Notes

Surfaced by the feature review for issue 809 on 2026-09-08 and ranked by the run's orchestrator as
the highest-value of that item's eight follow-ups, on the grounds that it is the only one whose
effect propagates beyond the item that found it.

Related wording defect worth settling in the same change: `CLAUDE.md` CUT3 step 4 names
`vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` while the actual coverage route in
use is `dotnet-coverage`. That mismatch is what made issue 809's AC6 read as PARTIAL on wording
alone; the reviewer recomputed AC6's measurable clauses from raw Cobertura and they pass.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: a test over a small fixed Cobertura fixture containing duplicate method
      rows, asserting the aggregation returns the de-duplicated rate.
- [ ] Integration scenario to retest: re-derive the coverage figures for a recently merged item and
      confirm the corrected method reproduces the reviewer's recomputation.
- [ ] Manual verification notes: identify every atomic plan template or skill that pins the current
      aggregation text, so the correction propagates rather than being fixed in one plan.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
