# P2-T13 Reduced-Audit Handoff

Timestamp: 2026-09-29T09-27
Task: P2-T13
Command: none (handoff record)
EXIT_CODE: 0

### Acceptance Criteria Status
- Source: docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/issue.md (section `## Acceptance Criteria`)
- Total AC items: 7
- Checked off (delivered): 5 (Grep count of `^- \[x\] AC` at this point)
- Remaining (unchecked): 2
- Items remaining:
  - AC6: PoshQC analyze reports no findings on any changed PowerShell file, the Pester suite passes, and the Pester line-coverage figure over the CI Pester population (the scripts/dependencies and scripts/vscode folders, as measured by the Pester workflow) remains at or above 80% and does not fall below its recorded baseline, with every changed production line covered. (AC6-STATUS: PENDING, below-baseline; see below)
  - AC7: All committed evidence follows the CLAUDE.md Committed Test Evidence Format section and contains no absolute host path, developer account name, or host name. (checked by P2-T14, the next task)

## Coverage route

COVERAGE-ROUTE: C (baseline P0-T7 and final P2-T3 iteration 1 both by CMD-PESTER-DIRECT). No Route B handoff artifact was written, because the direct Pester run completed and printed its POPULATION_LINE line in this session.

## Open finding for the orchestrator: loop not closed, plan revision required

- P2-T3 iteration 1 met every JUnit condition (334 tests, 0 failures, 0 errors, 0 skipped; new suite 14 of 14) but reported CHANGED-LINES-UNCOVERED: 408 for scripts/vscode/Invoke-MSTestWithCoverage.ps1. Line 408 is the scoped-arm `Write-Warning`. FINAL_POPULATION_LINE_PERCENT is 94.46 against a baseline of 94.49; the one extra missed line is the whole difference.
- Measured cause, from three diagnostic runs recorded in the P2-T3 artifact: Pester 5.6.1 uses breakpoint-based coverage by default (`CodeCoverage.UseBreakpoints` True). Each test file parses the entry point with `Parser::ParseFile` and dot-sources its own copy. The line breakpoints bind to the copy in the first file that executes `Invoke-MSTestWithCoverageMain`, which in the full run is Invoke-MSTest.RunSettings.Tests.ps1. Hits from a later file's copy are not recorded. With the new test file alone, or ordered first, line 408 is covered (ci=2). Ordered after Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1, it is not (ci=0).
- A fourth diagnostic run, with `CodeCoverage.UseBreakpoints = $false` (Pester's profiler-based coverage) and the order that reproduces the miss, credited line 408 (ci=2).
- The CI Pester workflow (.github/workflows/_pester.yml lines 44 to 47) does not set `UseBreakpoints`, so CI measures the same way. Its gate is population at least 80 percent, which 94.46 meets, so the CI gate is not affected.
- No fix exists inside the Write Set under the approved specifications. The Test Specification mandates the parse-and-dot-source import, and the Production Specification fixes the conditional block text. The earlier-sorting sibling test files are Out of Scope, and Invoke-MSTest.RunSettings.Tests.ps1 is at 499 lines. D16 prescribes a restart only after a fix, so iteration 2 was not run. P2-T3 and P2-T4 are left unchecked.

Revision options for the atomic-planner (none evaluated beyond the diagnostics above; the orchestrator decides):

1. Change the measurement: CMD-PESTER-DIRECT, and optionally the CI workflow as a separate item, sets `$c.CodeCoverage.UseBreakpoints = $false`. Verified on the two-file reproduction only. The baseline must then be re-measured the same way so the route-equality condition in P2-T12 holds.
2. Add one scoped-run case to the first-binding test file. That file is Invoke-MSTest.RunSettings.Tests.ps1, which is at 499 lines and so would need a split, which widens scope.
3. Revise the Production Specification so the scoped and unscoped arms live in the path-loaded Scope part file, and the entry point makes one unconditional call. This conflicts with D3's "unchanged in text and order" framing. That the path-loaded part file credits hits from any test file is unverified.
4. Record a ratified measurement exception for line 408, citing the diagnostics, and amend AC6's changed-line clause for this item.

Next step: the orchestrator routes this finding to atomic-planner for a revision delta covering P2-T3, P2-T4 and AC6, then runs the reduced audit. AC6 must not be checked off until a revised measurement gives FINAL at or above BASELINE by one route and CHANGED-LINES-UNCOVERED: none for the entry point.

## Evidence artifacts produced by P0-T1 through P2-T12 (feature-folder relative)

- evidence/baseline/phase0-instructions-read.md (P0-T1)
- evidence/baseline/p0-t2-mode-and-ac-source.2026-09-29T08-53.md (P0-T2)
- evidence/baseline/p0-t3-base-anchor.2026-09-29T08-54.md (P0-T3)
- evidence/baseline/p0-t4-tree-facts.2026-09-29T08-56.md (P0-T4)
- evidence/baseline/p0-t5-format-baseline.2026-09-29T09-01.md (P0-T5)
- evidence/baseline/p0-t6-analyze-baseline.2026-09-29T09-02.md (P0-T6)
- evidence/baseline/p0-t7-test-baseline.2026-09-29T09-05.md (P0-T7)
- evidence/other/p1-t1-implementation-handoff.2026-09-29T09-07.md (P1-T1)
- evidence/regression-testing/p1-t2-test-authoring.2026-09-29T09-12.md (P1-T2)
- evidence/regression-testing/p1-t3-expect-fail.2026-09-29T09-14.md (P1-T3)
- evidence/regression-testing/p1-t4-scope-part-file.2026-09-29T09-17.md (P1-T4)
- evidence/regression-testing/p1-t5-entry-point-edit.2026-09-29T09-19.md (P1-T5)
- evidence/regression-testing/p1-t6-pass-after.2026-09-29T09-21.md (P1-T6)
- evidence/qa-gates/p1-t7-format-measured.2026-09-29T09-23.md (P1-T7)
- evidence/qa-gates/p2-t1-format.iter1.2026-09-29T09-16.md (P2-T1, iteration 1)
- evidence/qa-gates/p2-t2-analyze.iter1.2026-09-29T09-18.md (P2-T2, iteration 1)
- evidence/qa-gates/p2-t3-test-coverage.iter1.2026-09-29T09-23.md (P2-T3, iteration 1; coverage condition not met)
- evidence/qa-gates/p2-t4-loop-closure.2026-09-29T09-25.md (P2-T4; loop not closed)
- evidence/qa-gates/p2-t5-file-size-and-untouched-neighbors.2026-09-29T09-26.md (P2-T5)
- evidence/qa-gates/p2-t6-scope-lock.2026-09-29T09-27.md (P2-T6)
- evidence/regression-testing/p2-t7-ac1-check-off.2026-09-29T09-27.md (P2-T7)
- evidence/regression-testing/p2-t8-ac2-check-off.2026-09-29T09-27.md (P2-T8)
- evidence/regression-testing/p2-t9-ac3-check-off.2026-09-29T09-27.md (P2-T9)
- evidence/regression-testing/p2-t10-ac4-check-off.2026-09-29T09-27.md (P2-T10)
- evidence/regression-testing/p2-t11-ac5-check-off.2026-09-29T09-27.md (P2-T11)
- evidence/regression-testing/p2-t12-ac6-check-off.2026-09-29T09-27.md (P2-T12)

Timestamp labels: the Phase 1 labels are later than the Phase 1 commit time, and the P2-T1 label (a clock read) is earlier than several of them. The P2-T4 to P2-T12 labels were assigned without a clock read. The six labels from P2-T7 to P2-T12, originally 09-29 to 09-34, were later than the wall clock. They were renamed to the clock-read minute 09-27 before this record was written. The labels are not used for ordering; the task identifiers are.

## Commit state

No commit was made by this executor. Phase 0 and Phase 1 content, including the three Write Set PowerShell files, was committed by the orchestrator at ab3b41890 and 313d0b918. The Phase 2 evidence artifacts, the plan check-off edits and the issue.md AC check-offs are uncommitted in the worktree. Nothing was staged, and the pre-existing .claude/agent-memory changes were not touched.

Output Summary: Handoff written. 5 of 7 AC checked off (AC1 to AC5). AC7 is pending P2-T14. AC6 is PENDING (below-baseline, 94.46 against 94.49, with entry-point line 408 uncovered). The cause is Pester breakpoint-coverage binding to the first test file's parsed copy of the entry point, confirmed by four diagnostic runs. P2-T3 and P2-T4 are unchecked, and a plan revision is required before the reduced audit can pass AC6.
