# Baseline — #584 `EXIT_CODE:` Field Census (P0-T12, S3-5 member set, SD3)

Timestamp: 2026-09-05T19-41

Command:

```powershell
Get-ChildItem -Path 'docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence' -Recurse -File -Filter '*.md' |
    ForEach-Object { Get-Content -LiteralPath $_.FullName | Select-String -Pattern '^EXIT_CODE:' }
```

Each match is partitioned by whether the matched line satisfies `^EXIT_CODE: -?[0-9]+$` exactly.

EXIT_CODE: 0

Output Summary:

## Population

| Measure | Value | Expected |
|---|---|---|
| Matched `^EXIT_CODE:` lines | 37 | 37 |
| Distinct files carrying such a line | 37 | 37 |
| Conforming (matches `^EXIT_CODE: -?[0-9]+$`) | 22 | 22 |
| Deviating | 15 | 15 |

Every file carries exactly one `EXIT_CODE:` field, so the line count and the file count coincide at
37. All four figures match the acceptance condition.

All paths below are relative to
`docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/`.

## Conforming set (22)

| File | Line | Matched text |
|---|---|---|
| `baseline/p0-t10-utilitiescs-tests-coverage.md` | 10 | `EXIT_CODE: 0` |
| `baseline/p0-t11-quickfiler-tests.md` | 10 | `EXIT_CODE: 0` |
| `baseline/p0-t12-threshold-reconciliation.md` | 11 | `EXIT_CODE: 0` |
| `baseline/p0-t7-csharpier-check.md` | 10 | `EXIT_CODE: 0` |
| `baseline/p0-t8-analyzer-build.md` | 10 | `EXIT_CODE: 0` |
| `baseline/p0-t9-nullable-build.md` | 10 | `EXIT_CODE: 0` |
| `baseline/phase0-instructions-read.md` | 9 | `EXIT_CODE: 0` |
| `other/p5-t12-ac-status-summary.md` | 10 | `EXIT_CODE: 0` |
| `qa-gates/p2-t3-file-size.md` | 13 | `EXIT_CODE: 0` |
| `qa-gates/p3-t1-analyzer-build.md` | 10 | `EXIT_CODE: 0` |
| `qa-gates/p4-t2-format-check.md` | 10 | `EXIT_CODE: 0` |
| `qa-gates/p4-t3-analyzer-build.md` | 10 | `EXIT_CODE: 0` |
| `qa-gates/p4-t4-nullable-build.md` | 10 | `EXIT_CODE: 0` |
| `qa-gates/p4-t5-utilitiescs-tests.md` | 13 | `EXIT_CODE: 0` |
| `qa-gates/p4-t7-coverage-delta.md` | 14 | `EXIT_CODE: 0` |
| `qa-gates/p4-t8-loop-closure.md` | 11 | `EXIT_CODE: 0` |
| `regression-testing/p1-t3-build-before-fix.md` | 10 | `EXIT_CODE: 0` |
| `regression-testing/p1-t4-expect-fail.md` | 10 | `EXIT_CODE: 1` |
| `regression-testing/p3-t2-regression-green.md` | 10 | `EXIT_CODE: 0` |
| `regression-testing/p3-t3-at-risk-tests.md` | 10 | `EXIT_CODE: 0` |
| `regression-testing/p3-t6-quickfiler-wpfuidispatcher.md` | 10 | `EXIT_CODE: 0` |
| `regression-testing/p4-t6-first-pass-failure.md` | 13 | `EXIT_CODE: 1` |

## Deviating set (15)

| File | Line | Matched text | Owning task |
|---|---|---|---|
| `qa-gates/p4-t6-quickfiler-tests.md` | 16 | `EXIT_CODE:` | P5-T10 |
| `qa-gates/p2-t2-nullforgiving-removed.md` | 11 | `EXIT_CODE:` | P5-T10 |
| `qa-gates/p2-t4-emailmovemonitor-reflection-target.md` | 18 | `EXIT_CODE:` | P5-T10 |
| `qa-gates/p1-t5-donotparallelize.md` | 11 | `EXIT_CODE:` | P5-T10 |
| `qa-gates/p4-t1-format.md` | 15 | `EXIT_CODE:` | P5-T10 |
| `qa-gates/p3-t5-no-timing-tokens.md` | 12 | `EXIT_CODE:` | P5-T10 |
| `other/p3-t4-progresstrackerasync-unmodified.md` | 13 | `EXIT_CODE:` | P5-T10 |
| `other/p5-t10-footprint.md` | 11 | `EXIT_CODE:` | P5-T10 |
| `baseline/p0-t13-parallel-bucket-census.md` | 13 | `EXIT_CODE:` | P5-T10 |
| `baseline/p0-t14-reflective-dispatcher-census.md` | 12 | `EXIT_CODE:` | P5-T10 |
| `baseline/p0-t5-toolchain-resolution.md` | 30 | `EXIT_CODE:` | P5-T10 |
| `baseline/p0-t2-uithread-rederivation.md` | 11 | `EXIT_CODE: 0 (both commands)` | P5-T11 |
| `baseline/p0-t3-progresstrackerasync-rederivation.md` | 12 | `EXIT_CODE: 0 (all three commands)` | P5-T11 |
| `baseline/p0-t4-test-rederivation.md` | 13 | `EXIT_CODE: 0 (all four commands)` | P5-T11 |
| `baseline/p0-t6-mcp-probe.md` | 12 | `EXIT_CODE: non-zero (tool invocation error; no exit code is returned by the MCP transport)` | P5-T11 |

## Reconciliation against the Phase 5 task lists

The eleven files P5-T10 enumerates and the four files P5-T11 enumerates together form a set of
fifteen paths. That set is identical to the deviating set measured here, path for path and line
number for line number. There is no divergence to report before Phase 5 begins.

The 15 deviating files plus the 22 conforming files account for the full population of 37, so no
file carrying an `EXIT_CODE:` field is unaccounted for.
