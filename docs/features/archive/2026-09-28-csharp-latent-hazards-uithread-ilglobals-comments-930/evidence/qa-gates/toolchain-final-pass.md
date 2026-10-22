# Toolchain final pass ([P2-T8])

Timestamp: 2026-09-29T09-24

Latest iteration: 1

| Step | Artifact | Timestamp | EXIT_CODE | Iteration |
|---|---|---|---|---|
| [P2-T1] format (write mode) | EVIDENCE/qa-gates/final-01-csharpier-format.md | 2026-09-29T09-18 | 0 | 1 |
| [P2-T2] format check | EVIDENCE/qa-gates/final-02-csharpier-check.md | 2026-09-29T09-18 | 0 | 1 |
| [P2-T3] file-size audit | EVIDENCE/qa-gates/final-03-file-size.md | 2026-09-29T09-18 | 0 | 1 |
| [P2-T4] analyzer Rebuild | EVIDENCE/qa-gates/final-04-analyzers.md | 2026-09-29T09-19 | 0 | 1 |
| [P2-T5] nullable Rebuild | EVIDENCE/qa-gates/final-05-nullable.md | 2026-09-29T09-20 | 0 | 1 |
| [P2-T6] MSTest with coverage | EVIDENCE/qa-gates/final-06-mstest-coverage.md | 2026-09-29T09-22 | 0 | 1 |

- Key figures: FORMAT_CHANGED_OWNED_PATCH=False, FORMAT_NEW_PATHS empty; CHECK_EXIT=0 (Checked 1623 files); all six LINES values at most 500; analyzer MSBUILD_EXIT=0, 0 Error(s), 0 Warning(s) (baseline 0); nullable MSBUILD_EXIT=0, 0 Error(s), CS86_ERROR_LINES=0; RUNNER_RESULT=COMPLETED, Total 7322 passed 7322 failed 0, SEQUENCE_FILES=0, First-party coverage: lines 56084/65736 (85.32%), branches 13597/17054 (79.73%).
- LOOP-ITERATIONS: 1
- SOURCE-REWRITE-COMMITS: NONE
- Decision D13 repeat: not used.

LOOP: CLEAN PASS
