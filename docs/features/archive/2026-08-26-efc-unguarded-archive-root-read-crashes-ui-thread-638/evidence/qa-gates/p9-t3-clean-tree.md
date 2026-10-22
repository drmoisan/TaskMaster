# [P9-T3] Plan execution summary and evidence index (Issue 638)

Timestamp: 2026-08-29T12-52

Command: `git add -- docs/.../2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638`
then `git commit`, using the same three pathspecs as [P9-T1].

EXIT_CODE: 0

Output Summary: every phase of
`plan.2026-08-29T07-41.md` executed in order. Two tasks recorded a
REMEDIATION-REQUIRED outcome on a branch their own task text defines; both are itemized
below. All other tasks passed their acceptance conditions.

## Per-phase outcome

| Phase | Title | Outcome |
|---|---|---|
| 0 | Baseline capture and policy reads | COMPLETE; [P0-T12] recorded REMEDIATION-REQUIRED on its exit-code justification |
| 1 | Scope lock and citation re-derivation | COMPLETE; all four citations re-derived exactly as the plan states |
| 2 | Diagnostic seam declaration | COMPLETE |
| 3 | Regression tests, fail before the fix | COMPLETE; 11 tests, 5 failed as required by the `[expect-fail]` task |
| 4 | Minimal fix | COMPLETE |
| 5 | Regression pass after the fix | COMPLETE; 11 of 11 pass, both sentinels pass unedited |
| 6 | Final QC toolchain loop | COMPLETE; loop ran twice, second pass clean in all five steps |
| 7 | Coverage measurement and delta | COMPLETE; [P7-T3] recorded REMEDIATION-REQUIRED on the mode-equality clause |
| 8 | Acceptance criteria and spec updates | COMPLETE; 18 of 20 criteria checked off, AC17 and AC20 left unchecked |
| 9 | Commit, footprint verification, clean tree | COMPLETE |

## Non-passing outcomes

1. **[P0-T12] — REMEDIATION-REQUIRED.** The baseline coverage-harness run exited 1 for a
   cause other than the 80 percent coverage threshold: one pre-existing failing test,
   `QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally`,
   a five-second wall-clock timing assertion in a file this change does not touch. The
   literal `is below the required 80% threshold.` was absent from the run output, so the
   task's own branch required recording the observed exit code without `ExpectedExitCode:`
   and treating the task as REMEDIATION-REQUIRED. The same task's direct-harness run
   exited 0 with `BASELINE_FAILURE_SET: none`, so the carve-out list [P6-T5] consumes is
   empty. Every acceptance field the task requires is present.
2. **[P7-T3] — REMEDIATION-REQUIRED.** `BASELINE_COVERAGE_XML_MODE: raw` does not equal
   `POSTCHANGE_COVERAGE_XML_MODE: koverage-processed`, a direct consequence of the [P0-T12]
   outcome above: the baseline run terminated before the script's post-processing step and
   left raw root attributes on disk. The delta clause itself passes at `+14.63` points
   against a `-0.50` tolerance, and the blocking change-scoped clause in [P7-T2] passes at
   93.10 percent, but the mode mismatch means the repo-wide delta measures the denominator
   rather than this change. [P8-T19] therefore left AC17 unchecked.

## Measured figures

```
BASELINE_UNFORMATTED_COUNT:               0
BASELINE_ANALYZER_ERRORS:                 0
BASELINE_NULLABLE_ERRORS:                 0
BASELINE_REPO_LINE_COVERAGE_PERCENT:      70.70   (COVERAGE_XML_MODE: raw)
BASELINE_FAILURE_SET:                     none    (direct harness, exit 0)
PRECHANGE_EFCDATAMODEL_LINE_COUNT:        423
POSTFIX_EFCDATAMODEL_LINE_COUNT:          485
POSTFIX_ARCHIVEROOTTESTS_LINE_COUNT:      389
[P3-T15] fail-before:                     Total 11, Failed 5, Passed 6
[P5-T1] pass-after:                       Total 11, Passed 11, Failed 0
[P5-T2] sentinels:                        Total 2,  Passed 2,  Failed 0
[P6-T5] full suite:                       Total 6870, Passed 6870, Failed 0
[P6-T5] QuickFiler. namespace failures:   0
[P6-T5] TaskMaster. namespace failures:   0
POSTCHANGE_REPO_LINE_COVERAGE_PERCENT:    85.33   (COVERAGE_XML_MODE: koverage-processed)
POSTCHANGE_REPO_BRANCH_COVERAGE_PERCENT:  79.31
CHANGED_LINE_COVERAGE_PERCENT:            93.10   (27 of 29 lines)
DELTA_REPO_LINE_COVERAGE_POINTS:          +14.63
[P7-T4] Decision:                         WRITTEN
```

## Evidence artifacts produced by this plan

| # | Artifact | EXIT_CODE |
|---|---|---|
| 1 | `evidence/baseline/phase0-instructions-read.md` | n/a (policy-read record) |
| 2 | `evidence/baseline/p0-t6-dotnet-tool-restore.md` | 0 |
| 3 | `evidence/baseline/p0-t7-solution-restore.md` | 0 |
| 4 | `evidence/baseline/p0-t8-dotnet-coverage-probe.md` | 0 |
| 5 | `evidence/baseline/p0-t9-csharpier-check.md` | 0 |
| 6 | `evidence/baseline/p0-t10-msbuild-analyzers.md` | 0 |
| 7 | `evidence/baseline/p0-t11-msbuild-nullable.md` | 0 |
| 8 | `evidence/baseline/p0-t12-vstest-coverage.md` | 1 (REMEDIATION-REQUIRED) |
| 9 | `evidence/baseline/p0-t12-direct-harness-baseline.md` | 0 |
| 10 | `evidence/baseline/p1-t4-tree-facts.md` | 0 |
| 11 | `evidence/other/p2-t3-seam-compile.md` | 0 |
| 12 | `evidence/other/p3-t14-tests-compile.md` | 0 |
| 13 | `evidence/regression-testing/p3-t15-regression-fail-before.md` | 1 (ExpectedExitCode 1) |
| 14 | `evidence/other/p4-t6-fix-compile.md` | 0 |
| 15 | `evidence/other/p4-t7-file-size.md` | 0 |
| 16 | `evidence/regression-testing/p5-t1-regression-pass-after.md` | 0 |
| 17 | `evidence/regression-testing/p5-t2-sentinel-tests.md` | 0 |
| 18 | `evidence/other/p5-t3-untouched-tests.md` | 0 |
| 19 | `evidence/qa-gates/p6-t1-csharpier-format.md` | 0 |
| 20 | `evidence/qa-gates/p6-t2-csharpier-check.md` | 0 |
| 21 | `evidence/qa-gates/p6-t3-msbuild-analyzers.md` | 0 |
| 22 | `evidence/qa-gates/p6-t4-msbuild-nullable.md` | 0 |
| 23 | `evidence/qa-gates/p6-t5-vstest-coverage.md` | 0 |
| 24 | `evidence/qa-gates/p6-t6-loop-closure.md` | 0 |
| 25 | `evidence/qa-gates/p7-t1-coverage-postchange.md` | 0 |
| 26 | `evidence/qa-gates/p7-t2-coverage-changed-lines.md` | 0 |
| 27 | `evidence/qa-gates/p7-t3-coverage-delta.md` | 0 (REMEDIATION-REQUIRED) |
| 28 | `evidence/qa-gates/p7-t4-canonical-coverage-artifact.md` | 0 |
| 29 | `evidence/other/p8-t2-followup-issue-dossier.md` | 0 |
| 30 | `evidence/qa-gates/p9-t2-change-footprint.md` | 0 |
| 31 | `evidence/qa-gates/p9-t3-clean-tree.md` | 0 (this file) |

31 artifact paths, all relative to
`docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/`.

## Acceptance-criteria status against `spec.md`

- Source: `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/spec.md`
- Total AC items: 20
- Checked off: 18 (AC1 through AC16, AC18, AC19)
- Remaining: 2
  - **AC17** — coverage. Left unchecked because [P7-T3] is REMEDIATION-REQUIRED on the
    mode-equality clause. The change-scoped measurement it also demands passes at 93.10
    percent.
  - **AC20** — the three follow-up issues for the non-goals are not yet filed. The
    ready-to-file dossier is `evidence/other/p8-t2-followup-issue-dossier.md`; filing is an
    orchestrator responsibility under
    `.claude/skills/feature-promotion-lifecycle/SKILL.md`.

## Files changed by this plan

- `QuickFiler/Controllers/EfcDataModel.cs` (modified)
- `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` (created)
- `QuickFiler.Test/QuickFiler.Test.csproj` (modified — one `<Compile Include>` entry)
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/spec.md`
  (AC check-offs and header fields)
- `docs/features/active/2026-08-26-efc-unguarded-archive-root-read-crashes-ui-thread-638/plan.2026-08-29T07-41.md`
  (checklist and header)
- the 31 evidence artifacts listed above

Paths written but outside the diff because they are gitignored:
`artifacts/csharp/coverage.xml`, `coverage/coverage.cobertura.xml`, the `TestResults`
run directories and MSBuild transcripts, and the `packages/` directory materialized by
[P0-T7].
