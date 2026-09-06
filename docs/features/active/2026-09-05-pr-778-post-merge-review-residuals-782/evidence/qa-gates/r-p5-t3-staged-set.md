# [P5-T3] The staged set for the first commit

Timestamp: 2026-09-06T01-58

Command:

```powershell
git diff --cached --name-only
```

EXIT_CODE: 0

Output Summary: 38 staged paths. All 38 are either under this feature's folder or one of the two
`UtilitiesCS.Test` assertion files. None is `artifacts/orchestration/orchestrator-state.json`, none
is under `.claude/`, and none is under `coverage/` or `TestResults/`.

STAGED_COUNT: 38
PATHS_OUTSIDE_ALLOWED_SET: 0
ORCHESTRATOR_STATE_MATCHES: 0
DOTCLAUDE_MATCHES: 0
COVERAGE_OR_TESTRESULTS_MATCHES: 0

## The four checks and their counts

| Check | Search | Count | Required |
|---|---|---|---|
| 1 | staged paths not under `docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/` and not one of the two `UtilitiesCS.Test` files | 0 | 0 |
| 2 | `Select-String -SimpleMatch 'orchestrator-state.json'` over the staged list | 0 | 0 |
| 3 | `Select-String -SimpleMatch '.claude/'` over the staged list | 0 | 0 |
| 4 | staged paths under `coverage/` or `TestResults/` | 0 | 0 |

All four report zero, which is the required outcome for each.

## The full staged list

```text
UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs
UtilitiesCS.Test/Threading/UiThread_Tests.cs
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/baseline/p0-t7-coverage.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/other/ac-status-summary.2026-09-05T23-15.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/other/code-review.2026-09-05T23-00.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/other/r1-r2-maintainer-disposition.2026-09-06T00-15.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/qa-gates/r-p1-t10-assertion-token-gate.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/qa-gates/r-p1-t3-analyzer-build.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/qa-gates/r-p1-t4-assertion-tests.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/qa-gates/r-p2-t4-spec-claim-gate.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/qa-gates/r-p2-t8-spec-wildcard-gate.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/qa-gates/r-p4-t1-format.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/qa-gates/r-p4-t2-format-check.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/qa-gates/r-p4-t3-analyzer-build.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/qa-gates/r-p4-t4-nullable-build.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/qa-gates/r-p4-t5-tests-coverage.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/qa-gates/r-p4-t6-coverage-comparison.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/qa-gates/r-p4-t7-loop-closure.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/qa-gates/r-p5-t1-dotclaude-untouched.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/regression-testing/r-p1-t5-mutation-applied.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/regression-testing/r-p1-t6-mutation-build.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/regression-testing/r-p1-t7-fail-before.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/regression-testing/r-p1-t8-mutation-reverted.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/regression-testing/r-p1-t9-pass-after.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t1-instructions-read.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t10-tests-coverage.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t11-anchor.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t12-dotclaude-baseline.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t2-claim-inventory.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t3-assertion-sites.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t4-pre782-message.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t5-retained-cobertura-reaggregation.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t6-retained-document-provenance.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t7-csharpier-check.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t8-analyzer-build.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/remediation-baseline/r-p0-t9-nullable-build.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/remediation-plan.2026-09-06T00-15.md
docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/spec.md
```

## Why this artifact is not itself in the staged set it records

This file is written after `git add` ran, so it is not in the index at the moment the list above was
taken. An artifact cannot be a member of the set it records without invalidating the record. It is
committed by [P5-T7] in a second commit together with the two other artifacts written after the first
commit, which is why an amend is not used.

## Staging method

The staging was a single `git add --` invocation naming each path explicitly. `git add -A` and
`git add .` were not used, and no pathspec supplied to it could reach
`artifacts/orchestration/orchestrator-state.json`. Three of the eleven pathspecs are directories —
this feature's `evidence/remediation-baseline/`, `evidence/regression-testing/`, and
`evidence/qa-gates/` sub-paths — because every file this plan creates under them is intended for the
commit. They are the only directories named.
