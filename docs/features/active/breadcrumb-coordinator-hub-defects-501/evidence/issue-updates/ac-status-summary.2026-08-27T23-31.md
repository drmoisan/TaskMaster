# Acceptance-Criteria Status Summary (P8-T1, P8-T34)

Timestamp: 2026-08-27T23-31

Work Mode: `full-bug`. The sole acceptance-criteria source is `FF/spec.md` under its
`## Acceptance Criteria` heading, per the `acceptance-criteria-tracking` skill.

| AC ID | Evidence artifact |
| --- | --- |
| AC-01 | `FF/evidence/qa-gates/closepending-split.2026-08-27T20-53.md` |
| AC-02 | `FF/evidence/regression-testing/green-462.2026-08-27T20-17.md` |
| AC-03 | `FF/evidence/regression-testing/green-462-suite.2026-08-27T20-17.md` |
| AC-04 | `FF/evidence/regression-testing/green-500-lifetime.2026-08-27T20-27.md` |
| AC-05 | `FF/evidence/regression-testing/green-500-hub-and-501.2026-08-27T20-47.md` |
| AC-06 | `FF/evidence/regression-testing/green-500-lifetime.2026-08-27T20-27.md` |
| AC-07 | `FF/evidence/regression-testing/green-500-hub-and-501.2026-08-27T20-47.md` |
| AC-08 | `FF/evidence/regression-testing/green-500-hub-and-501.2026-08-27T20-47.md` |
| AC-09 | `FF/evidence/regression-testing/green-500-hub-and-501.2026-08-27T20-47.md` |
| AC-10 | `FF/evidence/regression-testing/green-500-hub-and-501.2026-08-27T20-47.md` |
| AC-11 | `FF/evidence/qa-gates/logging-verification-501.2026-08-27T20-48.md; FF/evidence/regression-testing/red-501-starvation.2026-08-27T20-40.md; FF/evidence/regression-testing/green-500-hub-and-501.2026-08-27T20-47.md` |
| AC-12 | `FF/evidence/regression-testing/green-502.2026-08-27T20-36.md` |
| AC-13 | `FF/evidence/regression-testing/green-502.2026-08-27T20-36.md` |
| AC-14 | `FF/evidence/qa-gates/build-after-additems.2026-08-27T20-33.md; FF/evidence/regression-testing/green-502-suite.2026-08-27T20-37.md; FF/evidence/qa-gates/addItemsCore-seam.2026-08-27T23-31.md` |
| AC-15 | `FF/evidence/regression-testing/green-502-lease-leak.2026-08-27T20-31.md` |
| AC-16 | `FF/evidence/regression-testing/red-462-reopen.2026-08-27T20-12.md; FF/evidence/regression-testing/green-462.2026-08-27T20-17.md` |
| AC-17 | `FF/evidence/regression-testing/red-500-lifetime-lock.2026-08-27T20-24.md; FF/evidence/regression-testing/green-500-lifetime.2026-08-27T20-27.md` |
| AC-18 | `FF/evidence/regression-testing/red-501-starvation.2026-08-27T20-40.md; FF/evidence/regression-testing/green-500-hub-and-501.2026-08-27T20-47.md` |
| AC-19 | `FF/evidence/regression-testing/red-502-lease-leak.2026-08-27T20-29.md; FF/evidence/regression-testing/green-502-lease-leak.2026-08-27T20-31.md` |
| AC-20 | `FF/evidence/qa-gates/unmodified-tests-audit.2026-08-27T20-52.md; FF/evidence/regression-testing/green-462-suite.2026-08-27T20-17.md` |
| AC-21 | `FF/evidence/qa-gates/unmodified-tests-audit.2026-08-27T20-52.md; FF/evidence/regression-testing/green-462-suite.2026-08-27T20-17.md` |
| AC-22 | `FF/evidence/qa-gates/unmodified-tests-audit.2026-08-27T20-52.md; FF/evidence/regression-testing/green-501-suite.2026-08-27T20-48.md` |
| AC-23 | `FF/evidence/qa-gates/line-counts-after-split.2026-08-27T20-22.md; FF/evidence/qa-gates/project-file-budget.2026-08-27T20-52.md; FF/evidence/qa-gates/post-merge-base-reconciliation.2026-08-27T23-31.md` |
| AC-24 | `FF/evidence/qa-gates/new-test-file-budget.2026-08-27T20-54.md; FF/evidence/qa-gates/project-file-budget.2026-08-27T20-52.md; FF/evidence/qa-gates/post-merge-base-reconciliation.2026-08-27T23-31.md` |
| AC-25 | `FF/evidence/qa-gates/line-count-audit.2026-08-27T20-49.md; FF/evidence/qa-gates/line-count-audit-postmerge.2026-08-27T23-31.md` |
| AC-26 | `FF/evidence/qa-gates/ownership-precommit.2026-08-27T20-51.md; FF/evidence/qa-gates/post-merge-base-reconciliation.2026-08-27T23-31.md; FF/evidence/qa-gates/scope-lock.2026-08-27T23-31.md` |
| AC-27 | `FF/evidence/qa-gates/determinism-scan.2026-08-27T20-50.md` |
| AC-28 | `FF/evidence/qa-gates/nfr-entry-time-verdict.2026-08-27T20-54.md; FF/evidence/regression-testing/green-500-lifetime.2026-08-27T20-27.md` |
| AC-29 | `FF/evidence/qa-gates/post-merge-csharpier.2026-08-27T23-31.md` |
| AC-30 | `FF/evidence/qa-gates/post-merge-msbuild-analyzers.2026-08-27T23-31.md` |
| AC-31 | `FF/evidence/qa-gates/post-merge-msbuild-nullable.2026-08-27T23-31.md` |
| AC-32 | `FF/evidence/qa-gates/post-merge-test-coverage.2026-08-27T23-31.md; FF/evidence/qa-gates/coverage-delta.2026-08-27T23-31.md; FF/evidence/qa-gates/post-merge-toolchain-attestation.2026-08-27T23-31.md` |

## Reconciliation (P8-T34)

- Acceptance criteria in `FF/spec.md`: **32**
- Marked `- [x]`: **32**
- Marked `- [ ]`: **0**
- Rows in this table: **32**
- Every row cites at least one evidence artifact that exists on disk.

Each criterion was checked off individually against the artifact named in its row, not batch-flipped.
Where a criterion has two halves, both halves are cited: AC-11 cites the source-inspection artifact for
its logging half and the red-to-green pair for its non-propagation half; AC-25 cites both the
pre-format and post-format line-count legs; AC-32 cites the test run, the coverage delta and the
four-step attestation.

## Criteria whose evidence was regenerated after the base merge

The branch merged integration tip `69e83171` (adding merged features 493 and 444) during this resumed
run, so every criterion whose evidence is a toolchain measurement was re-verified against the merged
tree rather than carried forward:

| AC | Superseded pre-merge artifact | Artifact of record |
| --- | --- | --- |
| AC-25 | `line-count-audit-postformat.2026-08-27T21-07.md` | `line-count-audit-postmerge.2026-08-27T23-31.md` |
| AC-29 | `final-csharpier-format/check.2026-08-27T20-57.md` | `post-merge-csharpier.2026-08-27T23-31.md` |
| AC-30 | `final-msbuild-analyzers.2026-08-27T20-58.md` | `post-merge-msbuild-analyzers.2026-08-27T23-31.md` |
| AC-31 | `final-msbuild-nullable.2026-08-27T20-59.md` | `post-merge-msbuild-nullable.2026-08-27T23-31.md` |
| AC-32 | `final-test-coverage.2026-08-27T21-02.md`, `coverage-delta.2026-08-27T21-05.md` | `post-merge-test-coverage.2026-08-27T23-31.md`, `coverage-delta.2026-08-27T23-31.md` |

The superseded artifacts are retained as the audit trail of the pre-merge state. They are not the
artifacts of record for their criteria.

## Disclosures

1. **AC-14 was strengthened during this run.** The P7-T6 coverage gate showed the superseded-`AddItems`
   skip path was never executed by any test. An `AddItemsCore` seam was added, mirroring the ratified
   `SetSuggestionsCore` seam, and `AddItemsCore_SupersededLeaseSkipsAppendAndSettlesTheLease` now proves
   the lease is settled at runtime. AC-14 previously rested on an XML comment plus a compile check; it
   now also rests on an executing assertion.

2. **AC-11's logging half is verified at source, not at runtime.** A `log4net` `MemoryAppender`
   assertion is feasible in principle — the test project does reference `log4net`, contrary to ruling
   PD-2's premise — but `BreadcrumbMessengerHubTests.cs` stands at 492 of the 500-line budget (AC-25)
   and AC-24 forbids a third new test file, so there is no compliant placement. The plan task P5-T8 was
   amended to record the true reason instead of the false one. AC-11's non-propagation half IS verified
   at runtime by a red-to-green test. Full analysis:
   `FF/evidence/qa-gates/logging-verification-501.2026-08-27T20-48.md`.

3. **Criteria about issue closure remain out of scope.** This feature merges into
   `epic/quickfiler-bug-family-integration`, not the default branch, and GitHub registers closing
   references only for pull requests targeting the default branch. No criterion here is worded as
   "closed by the merge", so none is affected, but the distinction is recorded deliberately.
