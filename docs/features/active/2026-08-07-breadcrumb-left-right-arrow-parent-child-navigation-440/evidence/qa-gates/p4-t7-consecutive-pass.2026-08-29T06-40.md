# Phase 4 — Consecutive Clean Toolchain Pass (issue #440, plan task P4-T7)

Timestamp: 2026-08-29T06-40

The five Phase 4 command steps ran in one uninterrupted sequence, in the plan's
order, with no intervening step and no failure.

| Order | Task | Artifact filename | Recorded timestamp | EXIT_CODE |
| --- | --- | --- | --- | --- |
| 1 | P4-T1 formatting | `p4-t1-csharpier-format.2026-08-29T06-34.md` | 2026-08-29T06-34 | 0 |
| 2 | P4-T2 formatting verification | `p4-t2-csharpier-check.2026-08-29T06-35.md` | 2026-08-29T06-35 | 0 |
| 3 | P4-T3 analyzer | `p4-t3-analyzer-build.2026-08-29T06-36.md` | 2026-08-29T06-36 | 0 |
| 4 | P4-T4 type-check | `p4-t4-nullable-build.2026-08-29T06-36.md` | 2026-08-29T06-36 | 0 |
| 5 | P4-T5 test with coverage | `p4-t5-test-coverage.2026-08-29T06-38.md` | 2026-08-29T06-38 | 0 |

The timestamps are in ascending order. P4-T3 and P4-T4 share the minute label
2026-08-29T06-36 because this repository's ISO-8601 evidence convention is
minute-granular; their execution order is unambiguous and is fixed both by the plan
and by the on-disk creation order of their two distinct msbuild log files,
`coverage\logs\p4-t3-analyzer.msbuild.txt` written at 06:36:04 and
`coverage\logs\p4-t4-nullable.msbuild.txt` written at 06:36:42.

## Rewritten-file count from the final P4-T1 run

**0**. The formatter rewrote no file, confirmed by identical SHA-256 digests before
and after for all three owned files.

## Gate evaluation

- P4-T1 through P4-T4 each require exit code 0. All four recorded exit code 0. PASS.
- P4-T5 requires exit code 0, or a non-zero exit code accompanied by a
  `COVERAGE-THRESHOLD-THROW:` record and a `Test Run Successful.` vstest summary.
  P4-T5 recorded exit code **0**, so the first alternative is satisfied directly and
  the second alternative is not needed. No `COVERAGE-THRESHOLD-THROW:` record exists
  for this run.

## Restarts

**0 restarts.** The sequence listed above is the only sequence that ran; it was not
preceded by a discarded attempt. No step failed and no step rewrote a file, so
Global rule 11's restart condition was never triggered.

## Conclusion

A full C# toolchain pass completed with zero errors in a single final pass, in the
order formatting, formatting verification, analyzer, type-check, test. This, together
with the four non-vacuity counts recorded in the P4-T3 and P4-T4 artifacts, is the
evidence for AC-14.
