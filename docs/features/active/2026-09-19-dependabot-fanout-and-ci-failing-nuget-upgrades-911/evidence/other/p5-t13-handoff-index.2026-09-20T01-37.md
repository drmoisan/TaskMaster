# Review-Handoff Index — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T09-15-22
- Task: [P5-T13]
- EXIT_CODE: 0

## Anchors and Commits

| Anchor | Value |
|---|---|
| Merge base with `origin/main` | `b5621910c5b97d2471e368e87e80dc294207111b` |
| [P0-T2] anchor SHA, the head this cycle started from | `4043b913468f913649be3e6aa189b1be8310df00` |
| [P1-T15] Phase 1 commit | `7cda4543995f52b8f2f41165de086c6b2eefb036` |
| [P2-T11] Phase 2 commit | `4a858005862593199541dbafe3450195d4e680fd` |
| [P3-T15] Phase 3 commit | `07b4872eae664e9e5242c79e2ed546a1ee9fe797` |
| [P4-T6] Phase 4 commit | `597bb2fcb14970e7222f6adad596405773753fc7` |

The merge base is `b5621910c` and not the review's `734112ed2`, because the branch took a clean
merge of `origin/main` after the review.

## Artifact Count

| Term | Value |
|---|---|
| `.md` artifacts, one per task from [P0-T1] through [P5-T14] | **74** |
| Derived from the `Task Counts` table as 13 + 15 + 11 + 15 + 6 + 14 | 74 |
| `R`, retained artifacts from abandoned QA-loop iterations | **0** |
| **Total, 74 plus R** | **74** |

The two figures are recorded **separately**, as the acceptance requires. A bare 74 would fail on
a correct run that restarted the loop, and a bare floor would be satisfied by a run that dropped
an artifact.

`R = 0` because the final QA loop completed in **one** iteration: [P5-T1] rewrote 0 files and
every subsequent step passed on its first run, so no `iter2` artifact of this cycle exists.
[P5-T8] records the same figure and enumerates the retained set as empty.

Two `iter2` files exist in this evidence tree — `p9-t1-poshqc-format.iter2` and
`p9-t2-poshqc-analyze.iter2` — and both carry the **`2026-09-19T09-44`** timestamp of the
predecessor cycle, whose loop did restart. Neither belongs to this cycle's set and neither is
listed below.

**Directory distribution:** `remediation-baseline` **13**, being every Phase 0 task;
`regression-testing` 15; `qa-gates` 45; `other` 1, this index.

## The Copied Non-Markdown Evidence Forms

| Task | Path | Status |
|---|---|---|
| [P0-T12] | `remediation-baseline/p0-t12-coverage-projection.2026-09-20T01-37.jacoco.xml` | **mandatory**, produced, 1,467 bytes |
| [P0-T12] | `remediation-baseline/p0-t12-test-results.2026-09-20T01-37.summary.txt` | conditional, **produced**, 298 bytes |
| [P5-T7] | `qa-gates/p5-t7-coverage-projection.2026-09-20T01-37.jacoco.xml` | **mandatory**, produced, 1,467 bytes |
| [P5-T7] | `qa-gates/p5-t7-test-results.2026-09-20T01-37.summary.txt` | conditional, **produced**, 298 bytes |

Both mandatory projections exist. **Both conditional test-result summaries were produced**, so
there is no not-produced case and no `TEST-RESULT-SUMMARY: not produced` reason to quote. All
four carry zero host-path occurrences.

## The Five Artifacts That Read a JaCoCo LINE Figure From a Coverage Document

Enumerated rather than described, so the set is not the executor's to choose.

| Expected member | Task | Carries the gate rule 12 standing-in statement |
|---|---|---|
| `remediation-baseline/p0-t8-pester.2026-09-20T01-37.md` | [P0-T8] | **yes** |
| `qa-gates/p1-t10-pester-coverage.2026-09-20T01-37.md` | [P1-T10] | **yes** |
| `qa-gates/p2-t7-pester-coverage.2026-09-20T01-37.md` | [P2-T7] | **yes** |
| `qa-gates/p3-t11-pester-coverage.2026-09-20T01-37.md` | [P3-T11] | **yes** |
| `qa-gates/p5-t3-pester-coverage.iter1.2026-09-20T01-37.md` | [P5-T3] | **yes** |

| Measurement | Value |
|---|---|
| Expected count | **5** |
| Actual count carrying the statement | **5** |

[P1-T11] and [P5-T9] record coverage figures too and are deliberately **not** in this set. They
are **consumers** that cite the five by path; only a task that read a coverage document can
stand in for a permitted evidence form. Both say so in their own text.

## Full Artifact Index — 72 Listed Here, Plus This File and [P5-T14]

Every listed path exists on disk. `EXIT_CODE` is `n/a` for the two artifacts that record no
single command: the fail-before exception dossier, which records a structural argument, and the
remote probe, which records four invocations with their own exit codes inline.

| Path under `evidence/` | Task | `EXIT_CODE` |
|---|---|---|
| `remediation-baseline/phase0-instructions-read.2026-09-20T01-37.md` | [P0-T1] | 0 |
| `remediation-baseline/p0-t2-anchor.2026-09-20T01-37.md` | [P0-T2] | 0 |
| `remediation-baseline/p0-t3-hostpath-census.2026-09-20T01-37.md` | [P0-T3] | 0 |
| `remediation-baseline/p0-t4-spec-amendment.2026-09-20T01-37.md` | [P0-T4] | 0 |
| `remediation-baseline/p0-t5-size-and-text-baseline.2026-09-20T01-37.md` | [P0-T5] | 0 |
| `remediation-baseline/p0-t6-poshqc-format.2026-09-20T01-37.md` | [P0-T6] | 0 |
| `remediation-baseline/p0-t7-poshqc-analyze.2026-09-20T01-37.md` | [P0-T7] | 1 |
| `remediation-baseline/p0-t8-pester.2026-09-20T01-37.md` | [P0-T8] | 0 |
| `remediation-baseline/p0-t9-csharpier-check.2026-09-20T01-37.md` | [P0-T9] | 0 |
| `remediation-baseline/p0-t10-msbuild-analyzers.2026-09-20T01-37.md` | [P0-T10] | 0 |
| `remediation-baseline/p0-t11-msbuild-nullable.2026-09-20T01-37.md` | [P0-T11] | 0 |
| `remediation-baseline/p0-t12-mstest-numeric-baseline.2026-09-20T01-37.md` | [P0-T12] | 0 |
| `remediation-baseline/p0-t13-remote-probe.2026-09-20T01-37.md` | [P0-T13] | n/a |
| `regression-testing/fail-before-exception.2026-09-20T01-37.md` | [P1-T1] | n/a |
| `regression-testing/p1-t2-line151.2026-09-20T01-37.md` | [P1-T2] | 0 |
| `regression-testing/p1-t3-line180.2026-09-20T01-37.md` | [P1-T3] | 0 |
| `regression-testing/p1-t4-line248.2026-09-20T01-37.md` | [P1-T4] | 0 |
| `regression-testing/p1-t5-line290.2026-09-20T01-37.md` | [P1-T5] | 0 |
| `regression-testing/p1-t6-line293.2026-09-20T01-37.md` | [P1-T6] | 0 |
| `regression-testing/p1-t7-line330.2026-09-20T01-37.md` | [P1-T7] | 0 |
| `regression-testing/p1-t8-lines336-337.2026-09-20T01-37.md` | [P1-T8] | 0 |
| `regression-testing/p1-t9-line345.2026-09-20T01-37.md` | [P1-T9] | 0 |
| `qa-gates/p1-t10-pester-coverage.2026-09-20T01-37.md` | [P1-T10] | 0 |
| `qa-gates/p1-t11-sync-coverage-reconciliation.2026-09-20T01-37.md` | [P1-T11] | 0 |
| `qa-gates/p1-t12-poshqc-format.2026-09-20T01-37.md` | [P1-T12] | 0 |
| `qa-gates/p1-t13-poshqc-analyze.2026-09-20T01-37.md` | [P1-T13] | 1 |
| `qa-gates/p1-t14-size.2026-09-20T01-37.md` | [P1-T14] | 0 |
| `qa-gates/p1-t15-commit.2026-09-20T01-37.md` | [P1-T15] | 0 |
| `qa-gates/p2-t1-extraction.2026-09-20T01-37.md` | [P2-T1] | 0 |
| `regression-testing/p2-t2-r5-fail-before.2026-09-20T01-37.md` | [P2-T2] **[expect-fail]** | 1 |
| `qa-gates/p2-t3-r5-fix.2026-09-20T01-37.md` | [P2-T3] | 0 |
| `regression-testing/p2-t4-r5-pass-after.2026-09-20T01-37.md` | [P2-T4] | 0 for all three runs |
| `qa-gates/p2-t5-r9b-comment.2026-09-20T01-37.md` | [P2-T5] | 0 |
| `qa-gates/p2-t6-r9c-lister-visibility.2026-09-20T01-37.md` | [P2-T6] | 0 |
| `qa-gates/p2-t7-pester-coverage.2026-09-20T01-37.md` | [P2-T7] | 0 |
| `qa-gates/p2-t8-poshqc-format.2026-09-20T01-37.md` | [P2-T8] | 0 |
| `qa-gates/p2-t9-poshqc-analyze.2026-09-20T01-37.md` | [P2-T9] | 1 |
| `qa-gates/p2-t10-size.2026-09-20T01-37.md` | [P2-T10] | 0 |
| `qa-gates/p2-t11-commit.2026-09-20T01-37.md` | [P2-T11] | 0 |
| `regression-testing/p3-t1-workflow-fail-before.2026-09-20T01-37.md` | [P3-T1] **[expect-fail]** | 1 for all four runs |
| `qa-gates/p3-t2-r3-write-gate.2026-09-20T01-37.md` | [P3-T2] | 0 |
| `regression-testing/p3-t3-r3-write-set.2026-09-20T01-37.md` | [P3-T3] | 0 |
| `qa-gates/p3-t4-r6-disclosure-guard.2026-09-20T01-37.md` | [P3-T4] | 0 |
| `regression-testing/p3-t5-r6-idempotence.2026-09-20T01-37.md` | [P3-T5] | 0 |
| `qa-gates/p3-t6-r7-dead-filter.2026-09-20T01-37.md` | [P3-T6] | 0 |
| `qa-gates/p3-t7-r8-commit-identity.2026-09-20T01-37.md` | [P3-T7] | 0 |
| `regression-testing/p3-t8-workflow-pass-after.2026-09-20T01-37.md` | [P3-T8] | 0 for all six runs |
| `qa-gates/p3-t9-actionlint.2026-09-20T01-37.md` | [P3-T9] | 0 |
| `qa-gates/p3-t10-workflow-footprint.2026-09-20T01-37.md` | [P3-T10] | 0 |
| `qa-gates/p3-t11-pester-coverage.2026-09-20T01-37.md` | [P3-T11] | 0 |
| `qa-gates/p3-t12-poshqc-format.2026-09-20T01-37.md` | [P3-T12] | 0 |
| `qa-gates/p3-t13-poshqc-analyze.2026-09-20T01-37.md` | [P3-T13] | 1 |
| `qa-gates/p3-t14-size.2026-09-20T01-37.md` | [P3-T14] | 0 |
| `qa-gates/p3-t15-commit.2026-09-20T01-37.md` | [P3-T15] | 0 |
| `qa-gates/p4-t1-substitution-map.2026-09-20T01-37.md` | [P4-T1] | 0 |
| `qa-gates/p4-t2-sanitisation.2026-09-20T01-37.md` | [P4-T2] | 0 |
| `qa-gates/p4-t3-residual.2026-09-20T01-37.md` | [P4-T3] | 0 |
| `qa-gates/p4-t4-autoclose-list.2026-09-20T01-37.md` | [P4-T4] | 0 |
| `qa-gates/p4-t5-md-only.2026-09-20T01-37.md` | [P4-T5] | 0 |
| `qa-gates/p4-t6-commit.2026-09-20T01-37.md` | [P4-T6] | 0 |
| `qa-gates/p5-t1-poshqc-format.iter1.2026-09-20T01-37.md` | [P5-T1] | 0 |
| `qa-gates/p5-t2-poshqc-analyze.iter1.2026-09-20T01-37.md` | [P5-T2] | 1 |
| `qa-gates/p5-t3-pester-coverage.iter1.2026-09-20T01-37.md` | [P5-T3] | 0 |
| `qa-gates/p5-t4-csharpier-check.iter1.2026-09-20T01-37.md` | [P5-T4] | 0 |
| `qa-gates/p5-t5-msbuild-analyzers.iter1.2026-09-20T01-37.md` | [P5-T5] | 0 |
| `qa-gates/p5-t6-msbuild-nullable.iter1.2026-09-20T01-37.md` | [P5-T6] | 0 |
| `qa-gates/p5-t7-mstest-coverage.iter1.2026-09-20T01-37.md` | [P5-T7] | 0 |
| `qa-gates/p5-t8-toolchain-attestation.2026-09-20T01-37.md` | [P5-T8] | 0 |
| `qa-gates/p5-t9-coverage-reconciliation.2026-09-20T01-37.md` | [P5-T9] | 0 |
| `qa-gates/p5-t10-file-size-audit.2026-09-20T01-37.md` | [P5-T10] | 0 |
| `qa-gates/p5-t11-footprint.2026-09-20T01-37.md` | [P5-T11] | 0 |
| `qa-gates/p5-t12-finding-status.2026-09-20T01-37.md` | [P5-T12] | 0 |
| `other/p5-t13-handoff-index.2026-09-20T01-37.md` | [P5-T13] | 0, this file |
| `qa-gates/p5-t14-commit.2026-09-20T01-37.md` | [P5-T14] | written by the next task |

The four exit codes of `1` are the PoshQC analyzer gates at [P0-T7], [P1-T13], [P2-T9], [P3-T13]
and [P5-T2], each declaring `ExpectedExitCode: 1`; the tool exits 1 on any non-empty diagnostic
set and 13 pre-existing findings remain. The two `[expect-fail]` artifacts also record exit 1
and declare the expectation.

## Phase 6 Artifacts Are Not Listed

[P6-T1] through [P6-T6] produce six further artifacts. They fall outside the 74 this index
counts, which the acceptance defines as one per task **from [P0-T1] through [P5-T14]**.

## Output Summary

74 markdown artifacts plus `R = 0` retained iteration artifacts, both figures recorded
separately and derived from the enumeration. 13 sit under `remediation-baseline`, one per
Phase 0 task. Four copied non-markdown evidence forms, both mandatory projections and both
conditional summaries, all produced. The five coverage-reading artifacts are enumerated and all
five carry the gate rule 12 standing-in statement, expected 5 and actual 5.
