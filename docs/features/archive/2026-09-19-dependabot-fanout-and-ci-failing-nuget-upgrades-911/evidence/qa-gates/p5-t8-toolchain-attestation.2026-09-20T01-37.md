# Single-Pass Toolchain Attestation — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T09-13-30
- Task: [P5-T8]
- EXIT_CODE: 0

## The Seven Cited Artifacts

All seven exist on disk under
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/`.

| Step | Task | Artifact | Timestamp | `EXIT_CODE` | Outcome |
|---|---|---|---|---|---|
| 1 | [P5-T1] | `p5-t1-poshqc-format.iter1.2026-09-20T01-37.md` | 2026-09-20T09-07-32 | **0** | pass |
| 2 | [P5-T2] | `p5-t2-poshqc-analyze.iter1.2026-09-20T01-37.md` | 2026-09-20T09-08-05 | **1** | pass |
| 3 | [P5-T3] | `p5-t3-pester-coverage.iter1.2026-09-20T01-37.md` | 2026-09-20T09-09-38 | **0** | pass |
| 4 | [P5-T4] | `p5-t4-csharpier-check.iter1.2026-09-20T01-37.md` | 2026-09-20T09-09-58 | **0** | pass |
| 5 | [P5-T5] | `p5-t5-msbuild-analyzers.iter1.2026-09-20T01-37.md` | 2026-09-20T09-10-27 | **0** | pass |
| 6 | [P5-T6] | `p5-t6-msbuild-nullable.iter1.2026-09-20T01-37.md` | 2026-09-20T09-10-56 | **0** | pass |
| 7 | [P5-T7] | `p5-t7-mstest-coverage.iter1.2026-09-20T01-37.md` | 2026-09-20T09-13-17 | **0** | pass |

## Step 2's Exit Code Is 1 and That Is a Pass

`EXIT_CODE: 1` on the analyzer step is the **expected** outcome and the artifact declares
`ExpectedExitCode: 1`. The PoshQC analyzer exits 1 whenever its diagnostic set is non-empty, and
13 pre-existing findings remain on this tree in `scripts/vscode/` files this cycle does not
touch. The gate for that step is the equality with the [P0-T7] baseline `N` of 13 and a count of
exactly 0 in each of the seven files this cycle modified. Both hold.

Recording it as a pass with its expectation declared is what stops a later reader from treating
a non-zero code as a failed gate, and what stops an executor from treating `ok:true` as the
acceptance condition.

## Strictly Increasing Timestamps

```
09-07-32 < 09-08-05 < 09-09-38 < 09-09-58 < 09-10-27 < 09-10-56 < 09-13-17
```

**Strictly increasing across all seven.** They are recorded at second resolution deliberately:
steps 3 and 4 are 20 seconds apart and steps 5 and 6 are 29 seconds apart, so minute resolution
would have produced ties and the ordering could not have been demonstrated.

The ordering proves the seven ran **in sequence within one pass**, in the order the policy
requires: format, lint, then test for PowerShell; then format, analyze, type-check, test for C#.

## No Cited Artifact Belongs to an Earlier Loop Iteration

Every cited artifact carries the `iter1` suffix. **No `iter2` or later artifact exists for this
cycle**, because the loop did not restart: [P5-T1] rewrote 0 files and every subsequent step
passed on its first run.

Two `iter2` files exist in this evidence tree and neither is cited here:
`p9-t1-poshqc-format.iter2.2026-09-19T09-44.md` and
`p9-t2-poshqc-analyze.iter2.2026-09-19T09-44.md`. Both carry the **`2026-09-19T09-44`**
timestamp of the predecessor cycle, which did restart its loop. This cycle's artifacts all carry
`2026-09-20T01-37`, so the two cannot be confused.

`R`, the count of retained artifacts from abandoned QA-loop iterations of this cycle, is
therefore **0**. [P5-T13] records the same figure.

## Attestation

The full toolchain ran in order and every stage passed in a **single pass**, with no restart:

1. **Format**, PowerShell: PoshQC format, 0 rewrites of 46 files.
2. **Lint**, PowerShell: PoshQC analyze, 13 findings equal to baseline, 0 in owned files.
3. **Test**, PowerShell: Pester, 318 passed, 0 failed, 0 skipped, 94.43 percent line coverage.
4. **Format**, C#: CSharpier check, 1,623 files, 0 findings.
5. **Lint**, C#: MSBuild analyzer rebuild, 0 warnings, 0 errors, 36 compile lines.
6. **Type-check**, C#: MSBuild nullable rebuild, 0 warnings, 0 errors, 36 compile lines.
7. **Test**, C#: MSTest with coverage, 7,343 passed, 0.8593 line and 0.8010 branch.

Type checking is not applicable to PowerShell and is skipped for it, per
`.claude/rules/powershell.md`.

## Output Summary

All seven cited artifacts exist, all seven recorded a passing outcome as their own tasks define
it, their seven timestamps are strictly increasing, and none belongs to an earlier iteration of
this cycle's loop. The attestation holds and the loop does not restart.
