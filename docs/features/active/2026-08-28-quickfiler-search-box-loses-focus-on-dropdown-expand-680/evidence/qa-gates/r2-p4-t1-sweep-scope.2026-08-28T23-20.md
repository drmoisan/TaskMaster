Timestamp: 2026-08-28T23-20
Command: Get-ChildItem -Recurse -File under the feature folder; git status --porcelain (directory
entries expanded to their contained files); de-duplicated union of both sets
EXIT_CODE: 0
Output Summary: R2_SWEEP_SCOPE_COUNT = 95 (strictly greater than 0). The `git status --porcelain` set at
this moment was a strict subset of the feature-folder tree (all pending changes are under the feature
folder), so the union equals the feature-folder file count. Full path list below.

```
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/code-review.2026-08-28T16-27.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/code-review.2026-08-28T17-48.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/delivery-report.2026-08-28T16-40.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/baseline/baseline-context.2026-08-28T14-55.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/baseline/p0-t10-coverage-baseline.2026-08-28T14-55.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/baseline/p0-t3-sdk.2026-08-28T14-55.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/baseline/p0-t4-tool-restore.2026-08-28T14-55.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/baseline/p0-t5-restore.2026-08-28T14-55.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/baseline/p0-t6-dotnet-coverage.2026-08-28T14-55.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/baseline/p0-t7-csharpier-check.2026-08-28T14-55.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/baseline/p0-t8-analyzers.2026-08-28T14-55.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/baseline/p0-t9-nullable.2026-08-28T14-55.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/baseline/phase0-instructions-read.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/issue-updates/issue-680.2026-08-28T16-14.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/other/base-state-677.2026-08-28T15-20.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/other/harness-seams.2026-08-28T15-22.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/other/hv-runbook-680.2026-08-28T16-12.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/other/p3-t1-cr1-addendum-verified.2026-08-28T19-32.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/other/r2-p2-t5-addendum-verified.2026-08-28T23-11.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/other/r2-p3-t1-timestamp-note-verified.2026-08-28T23-16.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/other/trx-sanitisation.2026-08-28T16-45.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/qa-gates/p4-t1-format.2026-08-28T19-35-restart-note.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/qa-gates/p4-t1-format.2026-08-28T19-38.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/qa-gates/p4-t1-pinned-diff.2026-08-28T16-05.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/qa-gates/p4-t2/p4-t2.trx
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/qa-gates/p4-t2-analyzers.2026-08-28T19-42.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/qa-gates/p4-t2-pinned-suites.2026-08-28T16-07.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/qa-gates/p4-t3-nullable.2026-08-28T19-46.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/qa-gates/p4-t4/p4-t4.trx
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/qa-gates/p4-t4-qft-full-run.2026-08-28T19-50.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/qa-gates/p4-t5-coverage-final.2026-08-28T20-05.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/qa-gates/p4-t6-file-size-audit.2026-08-28T20-10.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/qa-gates/p4-t7-commit-readiness.2026-08-28T20-12.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/qa-gates/p6-t1-format.2026-08-28T16-20.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/qa-gates/p6-t2-analyzers.2026-08-28T16-20.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/qa-gates/p6-t3-nullable.2026-08-28T16-20.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/qa-gates/p6-t4-coverage-final.2026-08-28T16-20.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/qa-gates/p6-t5-coverage-delta.2026-08-28T16-20.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/qa-gates/p6-t6-file-sizes.2026-08-28T16-20.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/qa-gates/r2-p1-t1-sanitize.2026-08-28T22-45.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/qa-gates/r2-p1-t2-sweep.2026-08-28T22-48.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/qa-gates/r2-p1-t3-xml-and-escaping-check.2026-08-28T22-53.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/qa-gates/r2-p2-t4-relocated-sanitized.2026-08-28T23-06.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p1-t1-relocation-verified.2026-08-28T19-05.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p1-t2-build.2026-08-28T19-07.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p1-t3/p1-t3.trx
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p1-t3-hosttests-post-move.2026-08-28T19-09.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p2-t10/p2-t10.trx
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p2-t10-red-run-dismissal.2026-08-28T15-47.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p2-t1-test-added.2026-08-28T19-15.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p2-t2-build.2026-08-28T19-17.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p2-t2-red-build.2026-08-28T15-30.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p2-t3/p2-t3.trx
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p2-t3-new-test-green.2026-08-28T19-27.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p2-t3-red-run-host.2026-08-28T15-30.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p2-t9-seam-build.2026-08-28T15-45.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p3-t5-fixa-build.2026-08-28T15-55.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p3-t6/p3-t6.trx
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p3-t6-green-run-host.2026-08-28T15-57.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p3-t8-fixb-build.2026-08-28T16-00.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p3-t9/p3-t9.trx
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p3-t9-green-run-dismissal.2026-08-28T16-02.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/r2-p2-t1-source-verified.2026-08-28T22-56.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/r2-p2-t2-green-preserved.2026-08-28T22-58.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/r2-p2-t3-restored.2026-08-28T23-01.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/r-p2-t3/p2-t3.trx
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/remediation-baseline/p0-t2-context.2026-08-28T18-16.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/remediation-baseline/p0-t3-format.2026-08-28T18-18.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/remediation-baseline/p0-t4-analyzers.2026-08-28T18-22.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/remediation-baseline/p0-t5-nullable.2026-08-28T18-25.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/remediation-baseline/p0-t6/p0-t6.trx
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/remediation-baseline/p0-t6-hosttests-baseline.2026-08-28T18-30.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/remediation-baseline/p0-t7/p0-t7.trx
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/remediation-baseline/p0-t7-qft-baseline.2026-08-28T18-32.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/remediation-baseline/p0-t8-coverage-baseline.2026-08-28T18-55.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/remediation-baseline/phase0-instructions-read.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/remediation-baseline/r2-p0-t2-context.2026-08-28T22-29.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/remediation-baseline/r2-p0-t3-before-sweep.2026-08-28T22-33.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/remediation-baseline/r2-p0-t4-before-xml-check.2026-08-28T22-36.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/remediation-baseline/r2-p0-t5-p2t3-before-state.2026-08-28T22-39.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/remediation-baseline/r2-phase0-instructions-read.2026-08-28T22-27.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/feature-audit.2026-08-28T16-27.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/feature-audit.2026-08-28T17-48.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/issue.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/plan.2026-08-28T12-56.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/policy-audit.2026-08-28T16-27.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/policy-audit.2026-08-28T17-48.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/remediation-inputs.2026-08-28T16-27.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/remediation-inputs.2026-08-28T17-48.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/remediation-plan.2026-08-28T17-15.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/remediation-plan.2026-08-28T18-05.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/research/2026-08-28T11-00-quickfiler-search-box-focus-loss-680-research.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/rollout-notes.2026-08-28T16-42.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/runbooks/quickfiler-search-focus-hv-680.runbook.md
docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/spec.md
```

Note: this artifact itself was created after the enumeration above ran and is therefore not self-listed
in R2_SWEEP_SCOPE_COUNT. It is a new evidence artifact authored by this same plan and, like the other
`r2-p#-t#-*.<ts>.md` artifacts this plan produces, is composed under D4's host-path hygiene rule (no
absolute host path, account name, or machine name in any `Command:`/`Output Summary:` field), so its
absence from the P4-T3 sweep scope carries no risk of an unswept leak.
