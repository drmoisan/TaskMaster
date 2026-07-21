# Acceptance-Criteria Mapping (P12-T10)

Timestamp: 2026-07-19T11-57

| AC | Statement (summary) | Satisfying evidence (concrete artifact paths) | Status |
|---|---|---|---|
| AC1 | Every compiled in-scope hand-written file carries `#nullable enable` and compiles zero CS86xx under the pragma-only build | `evidence/qa-gates/qc-nullable-pragma-gate.2026-07-19T11-57.md` (P12-T3, section B isolated gate: 0 CS86xx across 37 files); per-batch `evidence/other/batch0..8`, `to-depricate`, `examples`-pragma-verify | PASS |
| AC2 | No project/solution `<Nullable>` element; no global `/p:Nullable=enable` | `evidence/qa-gates/qc-no-project-nullable.2026-07-19T11-57.md` (P12-T7) | PASS |
| AC3 | 6 `*.Designer.cs` under OlFolderTools left oblivious, not cross-blocked | `evidence/qa-gates/qc-designer-oblivious.2026-07-19T11-57.md` (P12-T8); `evidence/other/batch6-pragma-verify` (P7-T5), `batch7-pragma-verify` (P8-T5) | PASS |
| AC4 | No behavior change; no new types/post-condition attrs/record/init; existing guards preserved; no new runtime guard beyond strictly required | per-batch "no new runtime guard" notes in `evidence/other/batch0..8-pragma-verify` + `to-depricate`/`examples`-pragma-verify; `evidence/qa-gates/qc-nullable-pragma-gate` (P12-T3) | PASS |
| AC5 | Annotations consistent with #363/#364/#369 upstream signatures (TimeOutTask.RunWithTimeout non-null Task<TResult>; TryCopyToAsyncWithTimeout Task<bool>; IsNullOrEmpty non-refining on net481) | `evidence/other/batch4-pragma-verify.md` (P5-T2, OneDrive) + `evidence/qa-gates/qc-nullable-pragma-gate` (P12-T3); non-refining `IsNullOrEmpty` justified-`!` sites in `batch5-pragma-verify` (FolderPredictorEvaluator) and `batch3-pragma-verify` (RecipientStatic) | PASS |
| AC6 | Clean baseline test run captured before edits; no test/coverage regression attributable to this child | `evidence/baseline/baseline-tests-coverage.2026-07-19T10-54.md` (P0-T3); `evidence/qa-gates/qc-tests-coverage.2026-07-19T11-57.md` (P12-T4); `evidence/qa-gates/qc-coverage-delta.2026-07-19T11-57.md` (P12-T5) | PASS |
| AC7 | Six Maintainer Decisions recorded in spec.md, not silently resolved | `evidence/qa-gates/qc-maintainer-flags.2026-07-19T11-57.md` (P12-T9) | PASS |
| AC8 | No in-scope file exceeds 500 as a result of edits; the three pre-existing >500-line files flagged, not split, not worsened in a status-changing way | `evidence/qa-gates/qc-line-count.2026-07-19T11-57.md` (P12-T6); `batch3`/`batch8`-pragma-verify (P4-T2/P9-T1/P9-T2) | PASS |

All eight acceptance criteria are satisfied by the cited evidence artifacts.
