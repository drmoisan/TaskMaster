# P9-T14 — Review-handoff index

Timestamp: 2026-09-20T09-44

## Commit anchors

| Anchor | SHA |
|---|---|
| Merge base, from P0-T3 | `734112ed25bba293cb074e71fee2286bc3b72fae` |
| Batch A, from P2-T8 | `48f0c710a9a970587ab8b17956be224513c1f7fd` |
| Batch B, from P4-T7 | `596e7a70c78443861576f21a572bd2a919f02c66` |
| Batch C, from P6-T6 | `6b2426689eaece9bdd79998d9b9b9880fb0f9991` |
| Batch D, from P7-T11 | `e3ea87babd60be04cbd2af50c6fcbe96d8b60518` |
| Phase 8 evidence, from P8-T6 | `8bc97a13d21a803f3b750ae82778488fd244729e` |
| Phase 9 head, from P9-T13 | `655e6ec14696b6ad7b66e88231c4914ce276e972` |

## Artifact totals

| Folder | Artifacts |
|---|---|
| `evidence/baseline/` | **26** |
| `evidence/qa-gates/` | 97 |
| `evidence/other/` | 6 |
| `evidence/regression-testing/` | 2 |
| `evidence/issue-updates/` | 2 |
| **Total** | **133** |

Every one of the 133 listed paths was confirmed present on disk in a single existence sweep; none
was missing.

## Acceptance

| Clause | Required | Observed | Result |
|---|---|---|---|
| Artifacts listed | at least 85 | 133 | PASS |
| Every listed path exists on disk | yes | 133 of 133 | PASS |
| `evidence/baseline/` artifact count | at least 20 | 26 | PASS |
| Artifacts carrying the gate rule 12 standing-in statement | expected 6, actual recorded | expected 6, actual 6 | PASS |

## The six artifacts that record a JaCoCo LINE figure

Gate rule 12 requires the standing-in statement of every artifact that records a JaCoCo LINE figure,
and of no other. The population is six tasks. Each was searched for the statement and each carries
it.

| Task | Artifact | Standing-in statement present |
|---|---|---|
| P0-T18 | `evidence/baseline/p0-t18-pester.2026-09-19T09-44.md` | yes |
| P1-T6 | `evidence/qa-gates/p1-t6-packagegraph-run.2026-09-19T09-44.md` | yes |
| P2-T3 | `evidence/qa-gates/p2-t3-pester.2026-09-19T09-44.md` | yes |
| P4-T3 | `evidence/qa-gates/p4-t3-pester.2026-09-19T09-44.md` | yes |
| P6-T3 | `evidence/qa-gates/p6-t3-pester.2026-09-19T09-44.md` | yes |
| P9-T3 | `evidence/qa-gates/p9-t3-pester.iter1.2026-09-19T09-44.md` | yes |

EXPECTED-STANDING-IN-COUNT: 6
ACTUAL-STANDING-IN-COUNT: 6

No shortfall, so no artifact is named as omitting it. The obligation does not extend to the other
seventeen Pester tasks, which record no coverage figure. That assertion is why the honesty clause is
enforced rather than merely stated: under gate rule 1 an obligation with no failing condition is
unenforced, and before this clause nothing in the plan could fail for omitting it.

## Test-result summary status for the two coverage runs

| Task | Test-result summary | Copied to |
|---|---|---|
| P2-T7 | produced | `evidence/qa-gates/p2-t7-test-results.2026-09-19T09-44.summary.txt` |
| P9-T7 | produced | `evidence/qa-gates/p9-t7-test-results.2026-09-19T09-44.summary.txt` |

Neither run carried `TEST-RESULT-SUMMARY: not produced`, so no permitted form is missing from the
handoff.

## Deferred acceptance criteria

AC18, AC19 and AC20 are unchecked. Each is unverifiable rather than failing: P8-T1 measured
`CREDENTIAL-PRESENT: false` from a secrets query that exited 0 with an empty name list, and
`DEPENDABOT-PR-COUNT: 0` from an open-pull-request query that exited 0. All three are carried by
issue **#914**, https://github.com/drmoisan/TaskMaster/issues/914.

## Open item for the reviewer

`evidence/qa-gates/p9-t12-change-footprint.2026-09-19T09-44.md` records one path in the change
footprint that falls outside the four classes its acceptance enumerates:
`docs/features/potential/promoted/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades.md`. It
was introduced by the feature-promotion commit `d46ae2dc6` before Phase 0 and no plan task can avoid
producing it. It is recorded as a plan-clause omission rather than a scope violation.

## Every artifact

| Artifact path, relative to the feature folder | Task | `EXIT_CODE` | Criterion discharged |
|---|---|---|---|
| `evidence/baseline/p0-t1-worktree-anchor.2026-09-19T09-44.md` | P0-T1 | 0 | none |
| `evidence/baseline/p0-t10-cold-state-census.2026-09-19T09-44.md` | P0-T10 | 0 | none |
| `evidence/baseline/p0-t11-ac6-cold-analyzer-build-red.2026-09-19T09-44.md` | P0-T11 | 1 | AC6 red control |
| `evidence/baseline/p0-t12-nullable-build.2026-09-19T09-44.md` | P0-T12 | 1 | none |
| `evidence/baseline/p0-t13-csharpier-check.2026-09-19T09-44.md` | P0-T13 | 0 | none |
| `evidence/baseline/p0-t14-mstest-coverage.2026-09-19T09-44.md` | P0-T14 | 1 | none |
| `evidence/baseline/p0-t15-poshqc-format.2026-09-19T09-44.md` | P0-T15 | 0 | none |
| `evidence/baseline/p0-t16-format-revert.2026-09-19T09-44.md` | P0-T16 | 0 | none |
| `evidence/baseline/p0-t17-poshqc-analyze.2026-09-19T09-44.md` | P0-T17 | 1 | none |
| `evidence/baseline/p0-t18-pester.2026-09-19T09-44.md` | P0-T18 | 0 | none |
| `evidence/baseline/p0-t19-analyzer-census.2026-09-19T09-44.md` | P0-T19 | 0 | none |
| `evidence/baseline/p0-t20-manifest-census.2026-09-19T09-44.md` | P0-T20 | 0 | none |
| `evidence/baseline/p0-t21-format-and-nuget-census.2026-09-19T09-44.md` | P0-T21 | 0 | none |
| `evidence/baseline/p0-t22-dependabot-census.2026-09-19T09-44.md` | P0-T22 | 0 | none |
| `evidence/baseline/p0-t23-pester-scope-census.2026-09-19T09-44.md` | P0-T23 | 0 | none |
| `evidence/baseline/p0-t25-commit.2026-09-19T09-44.md` | P0-T25 | 0 | none |
| `evidence/baseline/p0-t3-diff-anchor.2026-09-19T09-44.md` | P0-T3 | 0 | none |
| `evidence/baseline/p0-t4-batch-budget-state.2026-09-19T09-44.md` | P0-T4 | 0 | none |
| `evidence/baseline/p0-t5-sdk-bootstrap.2026-09-19T09-44.md` | P0-T5 | 0 | none |
| `evidence/baseline/p0-t6-tool-restore.2026-09-19T09-44.md` | P0-T6 | 0 | none |
| `evidence/baseline/p0-t7-package-restore.2026-09-19T09-44.md` | P0-T7 | 0 | none |
| `evidence/baseline/p0-t8-dotnet-coverage.2026-09-19T09-44.md` | P0-T8 | 0 | none |
| `evidence/baseline/p0-t9-pester-provision.2026-09-19T09-44.md` | P0-T9 | 0 | none |
| `evidence/baseline/p2-t7-mstest-numeric-baseline.2026-09-19T09-44.md` | P2-T7 | 0 | none |
| `evidence/baseline/p5-t5-ac22-fail-before.2026-09-19T09-44.md` | P5-T5 | 1 | AC22 fail-before |
| `evidence/baseline/phase0-instructions-read.2026-09-19T09-44.md` | P0-T2 | 0 | none |
| `evidence/issue-updates/p8-t5-followup-issue-body.2026-09-19T09-44.md` | P8-T5 | n/a | none |
| `evidence/issue-updates/p8-t5-followup-issue.2026-09-19T09-44.md` | P8-T5 | 0 | none |
| `evidence/other/p0-t24-plan-sync-verification.2026-09-19T09-44.md` | P0-T24 | 0 | none |
| `evidence/other/p1-t1-spec-write-set-amendment.2026-09-19T09-44.md` | P1-T1 | 0 | none |
| `evidence/other/p2-t9-batch-a-boundary.2026-09-19T09-44.md` | P2-T9 | 0 | none |
| `evidence/other/p4-t8-batch-b-boundary.2026-09-19T09-44.md` | P4-T8 | 0 | none |
| `evidence/other/p6-t7-batch-c-boundary.2026-09-19T09-44.md` | P6-T7 | 0 | none |
| `evidence/other/p8-t1-credential-availability.2026-09-19T09-44.md` | P8-T1 | 0 | none |
| `evidence/qa-gates/p1-t10-analyzer-census-post-fix.2026-09-19T09-44.md` | P1-T10 | 0 | none |
| `evidence/qa-gates/p1-t11-903-manifest-entries.2026-09-19T09-44.md` | P1-T11 | 0 | none |
| `evidence/qa-gates/p1-t12-nuget-pin.2026-09-19T09-44.md` | P1-T12 | 0 | none |
| `evidence/qa-gates/p1-t13-pester-workflow-scope.2026-09-19T09-44.md` | P1-T13 | 0 | none |
| `evidence/qa-gates/p1-t14-ac6-cold-analyzer-build-green.2026-09-19T09-44.md` | P1-T14 | 0 | AC6 |
| `evidence/qa-gates/p1-t2-csharpierignore.2026-09-19T09-44.md` | P1-T2 | 0 | none |
| `evidence/qa-gates/p1-t3-ac2-format-scope-control.2026-09-19T09-44.md` | P1-T3 | 1 | AC2 |
| `evidence/qa-gates/p1-t4-packagegraph-module.2026-09-19T09-44.md` | P1-T4 | 0 | none |
| `evidence/qa-gates/p1-t5-packagegraph-tests-authored.2026-09-19T09-44.md` | P1-T5 | 0 | none |
| `evidence/qa-gates/p1-t6-packagegraph-run.2026-09-19T09-44.md` | P1-T6 | 0 | none |
| `evidence/qa-gates/p1-t7-normalisation.2026-09-19T09-44.md` | P1-T7 | 0 | none |
| `evidence/qa-gates/p1-t8-ac3-normaliser-idempotence.2026-09-19T09-44.md` | P1-T8 | 0 | AC3 |
| `evidence/qa-gates/p1-t9-898-analyzer-realignment.2026-09-19T09-44.md` | P1-T9 | 0 | none |
| `evidence/qa-gates/p2-t1-poshqc-format.2026-09-19T09-44.md` | P2-T1 | 0 | none |
| `evidence/qa-gates/p2-t2-poshqc-analyze.2026-09-19T09-44.md` | P2-T2 | 1 | none |
| `evidence/qa-gates/p2-t3-pester.2026-09-19T09-44.md` | P2-T3 | 0 | none |
| `evidence/qa-gates/p2-t4-csharpier-check.2026-09-19T09-44.md` | P2-T4 | 0 | none |
| `evidence/qa-gates/p2-t5-msbuild-analyzers.2026-09-19T09-44.md` | P2-T5 | 0 | none |
| `evidence/qa-gates/p2-t6-msbuild-nullable.2026-09-19T09-44.md` | P2-T6 | 0 | none |
| `evidence/qa-gates/p2-t7-coverage-projection.2026-09-19T09-44.jacoco.xml` | P2-T7 | n/a | none |
| `evidence/qa-gates/p2-t7-test-results.2026-09-19T09-44.summary.txt` | P2-T7 | n/a | none |
| `evidence/qa-gates/p2-t8-commit.2026-09-19T09-44.md` | P2-T8 | 0 | none |
| `evidence/qa-gates/p3-t1-packagecompatibility-module.2026-09-19T09-44.md` | P3-T1 | 0 | none |
| `evidence/qa-gates/p3-t10-ac4-nuget-pin.2026-09-19T09-44.md` | P3-T10 | 0 for both. | AC4 |
| `evidence/qa-gates/p3-t2-packagecompatibility-tests-authored.2026-09-19T09-44.md` | P3-T2 | 0 | none |
| `evidence/qa-gates/p3-t3-ac9-asset-level-gate.2026-09-19T09-44.md` | P3-T3 | 0 | AC9 |
| `evidence/qa-gates/p3-t4-sync-package-references.2026-09-19T09-44.md` | P3-T4 | 0 | none |
| `evidence/qa-gates/p3-t5-sync-tests-authored.2026-09-19T09-44.md` | P3-T5 | 0 | none |
| `evidence/qa-gates/p3-t6-ac7-framework-exclusion.2026-09-19T09-44.md` | P3-T6 | 0 | AC7 |
| `evidence/qa-gates/p3-t7-dependabot-consolidation.2026-09-19T09-44.md` | P3-T7 | 0 | none |
| `evidence/qa-gates/p3-t8-dependabotconfig-tests-authored.2026-09-19T09-44.md` | P3-T8 | 0 | none |
| `evidence/qa-gates/p3-t9-ac1-dependabot-consolidated.2026-09-19T09-44.md` | P3-T9 | 0 | AC1 |
| `evidence/qa-gates/p4-t1-poshqc-format.2026-09-19T09-44.md` | P4-T1 | 0 (MCP `ok:true` in both rounds) | none |
| `evidence/qa-gates/p4-t2-poshqc-analyze.2026-09-19T09-44.md` | P4-T2 | 1 | none |
| `evidence/qa-gates/p4-t3-pester.2026-09-19T09-44.md` | P4-T3 | 0 | none |
| `evidence/qa-gates/p4-t4-csharpier-check.2026-09-19T09-44.md` | P4-T4 | 0 | none |
| `evidence/qa-gates/p4-t5-actionlint.2026-09-19T09-44.md` | P4-T5 | 0 | none |
| `evidence/qa-gates/p4-t6-csharp-input-invariance.2026-09-19T09-44.md` | P4-T6 | 0 | none |
| `evidence/qa-gates/p4-t7-commit.2026-09-19T09-44.md` | P4-T7 | 0 | none |
| `evidence/qa-gates/p5-t1-projectconsistency-passthrough.2026-09-19T09-44.md` | P5-T1 | 0 | none |
| `evidence/qa-gates/p5-t10-consistencyverifier-run.2026-09-19T09-44.md` | P5-T10 | 0 | none |
| `evidence/qa-gates/p5-t11-analyzerrepair-tests-authored.2026-09-19T09-44.md` | P5-T11 | 0 | none |
| `evidence/qa-gates/p5-t12-analyzer-item-repair.2026-09-19T09-44.md` | P5-T12 | 0 | none |
| `evidence/qa-gates/p5-t13-ac12-analyzer-derivation.2026-09-19T09-44.md` | P5-T13 | 0 | AC12 |
| `evidence/qa-gates/p5-t14-ac13-sibling-survival.2026-09-19T09-44.md` | P5-T14 | 0 | AC13 |
| `evidence/qa-gates/p5-t15-ac11-version-reconciliation.2026-09-19T09-44.md` | P5-T15 | 0 | AC11 |
| `evidence/qa-gates/p5-t16-ac14-binding-redirects.2026-09-19T09-44.md` | P5-T16 | 0 | AC14 |
| `evidence/qa-gates/p5-t17-ac8-orphan-hintpaths.2026-09-19T09-44.md` | P5-T17 | 0 | AC8 |
| `evidence/qa-gates/p5-t18-ac16-verifier-both-directions.2026-09-19T09-44.md` | P5-T18 | 0 | AC16 |
| `evidence/qa-gates/p5-t19-ac23-reference-completeness.2026-09-19T09-44.md` | P5-T19 | 0 | AC23 |
| `evidence/qa-gates/p5-t2-consistencyverifier-passthrough.2026-09-19T09-44.md` | P5-T2 | 0 | none |
| `evidence/qa-gates/p5-t20-ac21-908-divergence-resolved.2026-09-19T09-44.md` | P5-T20 | 0 | AC21 |
| `evidence/qa-gates/p5-t22-file-size-audit.2026-09-19T09-44.md` | P5-T22 | 0 | none |
| `evidence/qa-gates/p5-t3-analyzeritemrepair-passthrough.2026-09-19T09-44.md` | P5-T3 | 0 | none |
| `evidence/qa-gates/p5-t4-projectconsistency-tests-authored.2026-09-19T09-44.md` | P5-T4 | 0 | none |
| `evidence/qa-gates/p5-t6-version-reconciliation.2026-09-19T09-44.md` | P5-T6 | 0 | none |
| `evidence/qa-gates/p5-t7-binding-redirect-reconciliation.2026-09-19T09-44.md` | P5-T7 | 0 | none |
| `evidence/qa-gates/p5-t8-verifier.2026-09-19T09-44.md` | P5-T8 | 0 | none |
| `evidence/qa-gates/p5-t9-consistencyverifier-tests-authored.2026-09-19T09-44.md` | P5-T9 | 0 | none |
| `evidence/qa-gates/p6-t1-poshqc-format.2026-09-19T09-44.md` | P6-T1 | 0 | none |
| `evidence/qa-gates/p6-t2-poshqc-analyze.2026-09-19T09-44.md` | P6-T2 | 1 | none |
| `evidence/qa-gates/p6-t3-pester.2026-09-19T09-44.md` | P6-T3 | 0 | none |
| `evidence/qa-gates/p6-t4-csharp-input-invariance.2026-09-19T09-44.md` | P6-T4 | 0 | none |
| `evidence/qa-gates/p6-t5-csharpier-check.2026-09-19T09-44.md` | P6-T5 | 0 | none |
| `evidence/qa-gates/p6-t6-commit.2026-09-19T09-44.md` | P6-T6 | 0 | none |
| `evidence/qa-gates/p7-t1-composition-root.2026-09-19T09-44.md` | P7-T1 | 0 | none |
| `evidence/qa-gates/p7-t10-file-size-audit.2026-09-19T09-44.md` | P7-T10 | 0 | none |
| `evidence/qa-gates/p7-t11-commit.2026-09-19T09-44.md` | P7-T11 | 0 | none |
| `evidence/qa-gates/p7-t2-repair-tests-authored.2026-09-19T09-44.md` | P7-T2 | 0 | none |
| `evidence/qa-gates/p7-t3-ac10-skip-and-proceed.2026-09-19T09-44.md` | P7-T3 | 0 | AC10 |
| `evidence/qa-gates/p7-t4-ac5-analyzer-verifier.2026-09-19T09-44.md` | P7-T4 | 0 | AC5 |
| `evidence/qa-gates/p7-t5-ac15-repair-idempotence.2026-09-19T09-44.md` | P7-T5 | 0 | AC15 |
| `evidence/qa-gates/p7-t6-repair-workflow.2026-09-19T09-44.md` | P7-T6 | 0 | none |
| `evidence/qa-gates/p7-t7-ac17-workflow-static-validity.2026-09-19T09-44.md` | P7-T7 | 0 | AC17 |
| `evidence/qa-gates/p7-t8-workflow-readme.2026-09-19T09-44.md` | P7-T8 | 0 | none |
| `evidence/qa-gates/p7-t9-ac26-documentation-pin.2026-09-19T09-44.md` | P7-T9 | 0 | AC26 |
| `evidence/qa-gates/p8-t2-ac18-repair-identity.2026-09-19T09-44.md` | P8-T2 | 0 | AC18 |
| `evidence/qa-gates/p8-t3-ac19-required-checks.2026-09-19T09-44.md` | P8-T3 | 0 | AC19 |
| `evidence/qa-gates/p8-t4-ac20-disclosure.2026-09-19T09-44.md` | P8-T4 | 0 | AC20 |
| `evidence/qa-gates/p8-t6-commit.2026-09-19T09-44.md` | P8-T6 | 0 | none |
| `evidence/qa-gates/p9-t1-poshqc-format.iter1.2026-09-19T09-44.md` | P9-T1 | 0 | none |
| `evidence/qa-gates/p9-t1-poshqc-format.iter2.2026-09-19T09-44.md` | P9-T1 | 0 | none |
| `evidence/qa-gates/p9-t10-file-size-audit.2026-09-19T09-44.md` | P9-T10 | 0 | none |
| `evidence/qa-gates/p9-t11-ac-status-summary.2026-09-19T09-44.md` | P9-T11 | n/a | none |
| `evidence/qa-gates/p9-t12-change-footprint.2026-09-19T09-44.md` | P9-T12 | 0 | none |
| `evidence/qa-gates/p9-t13-commit.2026-09-19T09-44.md` | P9-T13 | 0 | none |
| `evidence/qa-gates/p9-t2-poshqc-analyze.iter1.2026-09-19T09-44.md` | P9-T2 | 1 | none |
| `evidence/qa-gates/p9-t2-poshqc-analyze.iter2.2026-09-19T09-44.md` | P9-T2 | 1 | none |
| `evidence/qa-gates/p9-t3-pester.iter1.2026-09-19T09-44.md` | P9-T3 | 0 | AC24 |
| `evidence/qa-gates/p9-t4-csharpier-check.iter1.2026-09-19T09-44.md` | P9-T4 | 0 | none |
| `evidence/qa-gates/p9-t5-msbuild-analyzers.iter1.2026-09-19T09-44.md` | P9-T5 | 0 | none |
| `evidence/qa-gates/p9-t6-msbuild-nullable.iter1.2026-09-19T09-44.md` | P9-T6 | 0 | none |
| `evidence/qa-gates/p9-t7-coverage-projection.2026-09-19T09-44.jacoco.xml` | P9-T7 | n/a | none |
| `evidence/qa-gates/p9-t7-mstest-coverage.iter1.2026-09-19T09-44.md` | P9-T7 | 0 | none |
| `evidence/qa-gates/p9-t7-test-results.2026-09-19T09-44.summary.txt` | P9-T7 | n/a | none |
| `evidence/qa-gates/p9-t8-ac25-csharp-toolchain.2026-09-19T09-44.md` | P9-T8 | n/a | AC25 |
| `evidence/qa-gates/p9-t9-coverage-reconciliation.2026-09-19T09-44.md` | P9-T9 | n/a | none |
| `evidence/regression-testing/898-cold-restore-red-run.2026-09-19T11-40.md` | pre-Phase-0 control, cited by P0-T11 and P1-T14 | 0 | AC5, AC6 and AC22 red control |
| `evidence/regression-testing/p5-t21-ac22-fail-before-pass-after.2026-09-19T09-44.md` | P5-T21 | n/a | AC22 |
