# P2-T11 — Toolchain-Loop Outcome

Timestamp: 2026-09-01T14-47

LoopIterations: 2

That field is the counter the Phase 2 preamble's ceiling of 3 is measured against; it is the same
quantity and not a separate count. 2 does not exceed 3, so no `LOOP_CEILING_EXCEEDED:` line is
recorded and the ceiling-exceeded outcome does not apply.

## The four stages, in order, with the artifact recorded for each

All paths are relative to
`docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/qa-gates/`.

1. **Formatting** — `p2-t1-csharpier-format.md`
2. **Linting** — `p2-t3-analyzer-rebuild.md`
3. **Type checking** — `p2-t4-nullable-rebuild.md`
4. **Testing** — `p2-t6-quickfiler-test-full.md`

## Iteration history

**Iteration 1.** P2-T1 was executed. Its before-and-after SHA-256 pair was unequal
(`3fa83d3eee142b3539d3311e86504354b76b88b072af25e4e92e327c0f20efeb` before,
`c7f4ae79f251e1c2503d57237479fe8301f75fdb6b5697cb8de2a0a43cf7eee1` after), so the formatting stage
rewrote a tracked file. The Phase 2 restart rule applies to a stage that rewrites a tracked file just
as it does to one that fails, so the loop restarted from P2-T1 rather than continuing to P2-T2. No
stage after P2-T1 was executed in iteration 1. The rewrite was a line-ending normalisation from LF to
CRLF.

**Iteration 2.** P2-T1 was executed a second time. Its before-and-after SHA-256 pair was equal
(`c7f4ae79f251e1c2503d57237479fe8301f75fdb6b5697cb8de2a0a43cf7eee1` on both sides), so the formatting
stage rewrote nothing. The loop then ran to completion:

- P2-T2, read-only whole-tree check: `EXIT_CODE: 0`, `SourceScopedDrift: none`, set-equal to baseline.
- P2-T3, analyzer gate: `EXIT_CODE: 0`, `0 Error(s)`, 5 warnings against a baseline of 5, no
  diagnostic naming the owned path.
- P2-T4, nullable gate: `EXIT_CODE: 0`, `0 Error(s)`, 5 warnings against a baseline of 5, no
  diagnostic naming the owned path.
- P2-T5, scoped run: `EXIT_CODE: 0`, `Test Run Successful.`, `Total tests: 2`.
- P2-T6, full suite: `EXIT_CODE: 0`, `Test Run Successful.`, `Passed: 1285` against a baseline of 1285.
- P2-T7, coverage run: `EXIT_CODE: 0`, equal to the baseline coverage run's exit code.
- P2-T9, file-size audit: 104 lines, at most 500.
- P2-T10, unit-test policy audit: all seven findings PASS, no correction made, so no remediation
  restart was directed.

**The final iteration completed all four stages without a failure and without a file rewrite.** No
stage in iteration 2 failed, and no stage in iteration 2 rewrote a tracked file.
