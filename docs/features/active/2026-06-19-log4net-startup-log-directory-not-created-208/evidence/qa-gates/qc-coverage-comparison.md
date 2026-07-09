# QC — Coverage No-Regression & New-Code Threshold (Issue #208, [P2-T5])

Timestamp: 2026-07-09T09-44

Command / Comparison method: Extract the first-party TaskMaster module line-rate and the new-unit
class line-rate from the baseline Cobertura (evidence/baseline/baseline.cobertura.xml, [P0-T5]) and
the post-change Cobertura (evidence/qa-gates/post-change.cobertura.xml, [P2-T4]) via
`grep '<package ... name="TaskMaster"'` and `grep '<class ... name="...LogDirectoryInitializer"'`.

EXIT_CODE: 0

Output Summary:
- Baseline coverage:
  - Whole-process root line-rate: 56.51% (40604/71851 lines).
  - First-party TaskMaster module: 66.53%.
  - Extracted unit: not present at baseline (added in Phase 1).
- Post-change coverage:
  - Whole-process root line-rate: 15.20% (12972/85354 lines) — NOT comparable to baseline (different
    instrumented module set between runs; see note below).
  - First-party TaskMaster module: 67.27%.
  - Extracted unit (TaskMaster.Logging.LogDirectoryInitializer): 100% line-rate.
- New/changed-code coverage:
  - New production unit LogDirectoryInitializer: 100% (>= 90% new-code floor — PASS).
  - Other changed lines are in ThisAddIn.cs, which is [ExcludeFromCodeCoverage] (VSTO add-in
    lifecycle class) and therefore not in the coverage denominator; the changed logic there is thin
    wiring delegating to the covered unit.
- No-regression verdict: PASS. The stable first-party TaskMaster module rate did not regress
  (66.53% -> 67.27%, a slight increase). Every changed line that is in the coverage denominator (the
  new unit) is covered at 100%.

Note on the whole-process root figure: the `.coverage` collector instrumented a different set of
loaded modules between the baseline and post-change runs (lines-valid 71851 vs 85354, and root
branch-rate 0.47 vs 1.0), which is a known instability of whole-process coverage on this solution.
The no-regression judgement therefore uses the first-party TaskMaster module rate and the per-class
new-unit rate, both of which are stable and directly attributable to this change.
