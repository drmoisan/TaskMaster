# P5-T24 acceptance status summary (issue #973)

Timestamp: 2026-10-06T18-46
Command: Grep tool over spec.md `^- \[x\] AC[0-9]+ ` (count) and `^- \[ \] AC[0-9]+ ` (content, -n); Grep tool over plan.2026-10-02T22-16.md `^- \[x\] \[P` (count) and `^- \[ \] \[P[0-9]+-T[0-9]+\]` (content, -n)
EXIT_CODE: 0
Output Summary: 21 of 23 acceptance criteria are checked off. AC18 is pending manual verification, as planned. AC17 is NOT MET as written: Azure.Core.dll and its requesting assembly Microsoft.Kiota.Authentication.Azure.dll are absent from TaskMaster\bin\Debug. Before this record's own check-off the plan shows 105 tasks checked and 3 unchecked (P5-T17, P5-T24, P5-T25).

### Acceptance Criteria Status
- Source: docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/spec.md
- Total AC items: 23
- Checked off (delivered): 21
- Remaining (unchecked): 2
- Items remaining:
  - AC17 (invariant trace delivered): NOT MET. The System.Linq.AsyncEnumerable half holds. The Azure.Core redirect value is correct (0.0.0.0-1.63.0.0 to 1.63.0.0), but neither Azure.Core.dll nor Microsoft.Kiota.Authentication.Azure.dll is present in TaskMaster\bin\Debug; both are in UtilitiesCS\bin\Debug only. Evidence: evidence/qa-gates/p5-t17-ac17-checkoff.2026-10-06T18-36.md. Needs a planner or orchestrator ruling.
  - AC18 (manual verification; non-regression evidence only): PENDING-MANUAL (maintainer runbook; evidence/other/p5-t18-ac18-pending-manual.2026-10-06T18-43.md).

Spec counts: `^- \[x\] AC[0-9]+ ` = 21; `^- \[ \] AC[0-9]+ ` = 2 (line 373 AC17, line 374 AC18); sum 23.

Pending-CI: none of the 23 criteria is CI-dependent in its text. The CI Pester coverage job (_pester.yml) and the CI MSTest coverage job for the four excluded shell-icon classes (_mstest-coverage.yml) are recorded as CI-covered in poshqc-test.md and mstest-coverage-projection.md, and CI on the PR head confirms them.

### Plan checkbox state (before this task's own check-off)

`^- \[x\] \[P` = 105
Unchecked:
- P5-T17 (line 527): AC17: NOT MET (Azure.Core target assembly not present in TaskMaster\bin\Debug); the run continued per the Phase 5 rule.
- P5-T24 (line 534): this task.
- P5-T25 (line 535): commit C, next.

On the full-pass path the remaining list would be exactly AC18. This run is not on the full-pass path because of AC17.
