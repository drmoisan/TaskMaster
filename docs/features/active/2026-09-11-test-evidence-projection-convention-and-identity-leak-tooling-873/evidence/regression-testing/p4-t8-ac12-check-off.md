# P4-T8 — AC12 Check-Off

Timestamp: 2026-09-13T06-18
Task: [P4-T8]

Command: <edit of the AC12 checkbox in this feature folder's spec.md, followed by a case-sensitive read of the amended line>
EXIT_CODE: 0

Criterion checked off: AC12 — Both argument-builder family members carry the two switches. Exactly one
criterion was checked off by this task.

## Family cardinality

The complete argument-builder family is the two members the research artifact's Numeric Derivation
Evidence section derives twice, with identical member sets and a count of 2 under both the primary
derivation and the cross-check: `Get-DotnetCoverageArgumentList` in
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` and `Get-VsTestArgumentList` in
`scripts/vscode/Invoke-MSTest.ps1`. NO_THIRD_BUILDER_CLAIMED: true. This task claims no third builder
and none was found.

## Supporting test results

Coverage member, from P3-T6, recorded passed in
`evidence/regression-testing/p3-t6-results-directory-tests.md` and re-observed passing in this phase's
whole-folder run:

- `includes the explicit results directory and trx log file name in the coverage argument list`
- `places both new switches after the argument separator`

Plain member, from P4-T5, recorded passed in `evidence/regression-testing/p4-t5-plain-results-directory-tests.md`:

- `includes the explicit results directory and trx log file name in the vstest argument list`

All three are recorded as passed. The Phase 4 whole-folder Pester run recorded in
`evidence/qa-gates/p4-t6-phase4-toolchain.md` reports 131 passed, 0 failed, 0 skipped, which includes
every one of them.

Output Summary: AC12's checkbox is marked. The two coverage-member tests and the one plain-member test
are all recorded as passed, the family is the derived two members, and no third builder is claimed.
