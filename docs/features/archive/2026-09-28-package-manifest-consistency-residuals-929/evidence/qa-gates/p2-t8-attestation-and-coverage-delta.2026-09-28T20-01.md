# P2-T8 — Single-pass attestation and coverage reconciliation

Timestamp: 2026-09-30T11-07
Command: Read the seven final-iteration (iter2) artifacts P2-T1 to P2-T7 and the P0-T12 and P0-T16 baselines; compute the deltas below.
EXIT_CODE: 0
Output Summary:

Final iteration: iter2 (iteration 1 failed at P2-T7 on one timing-dependent MSTest test; see qa-gates/p2-t7-mstest-coverage.iter1.2026-09-28T20-01.md).

| Task | Artifact | EXIT_CODE | Expected | Timestamp |
|---|---|---|---|---|
| P2-T1 | docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t1-poshqc-format.iter2.2026-09-28T20-01.md | 0 | 0 | 2026-09-30T10-55 |
| P2-T2 | docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t2-poshqc-analyze.iter2.2026-09-28T20-01.md | 0 | 0 | 2026-09-30T10-56 |
| P2-T3 | docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t3-pester.iter2.2026-09-28T20-01.md | 0 | 0 | 2026-09-30T10-58 |
| P2-T4 | docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t4-csharpier-check.iter2.2026-09-28T20-01.md | 0 | 0 (the P0-T9 exit code) | 2026-09-30T11-00 |
| P2-T5 | docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t5-msbuild-analyzers.iter2.2026-09-28T20-01.md | 0 | 0 | 2026-09-30T11-01 |
| P2-T6 | docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t6-msbuild-nullable.iter2.2026-09-28T20-01.md | 0 | 0 | 2026-09-30T11-02 |
| P2-T7 | docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p2-t7-mstest-coverage.iter2.2026-09-28T20-01.md | 0 | 0 | 2026-09-30T11-06 |

- All seven exit codes equal their expected values.
- The seven timestamps are non-decreasing in task order and all seven artifacts carry the iter2 suffix.

C# coverage (P2-T7 iter2 minus P0-T12):
- Line: 85.92 minus 85.91 = +0.01 percentage points (at least -0.5)
- Branch: 80.08 minus 80.08 = 0.00 percentage points (at least -0.5)

PowerShell coverage (CI Pester job):
- Aggregate on the pushed head: 94.51 percent (run 36722780748, head b96926588d562f994430e7ba7301de5de86f206c, Pester job 109911885533 conclusion success); at least 80
- Baseline: 94.51 percent (run 36666302259 on main, head 231e1c0b55105aeb626bf5a6e8d0266a567cacad, Pester job 109731601928 conclusion success)
- ConsistencyVerifier.psm1 covered: 158 on the pushed head versus 158 at baseline (at least the P0-T16 value); missed 2 versus 2
- No PowerShell branch figure exists: Pester emits no branch counter.
- This change adds no new module, so no 90 percent new-module gate applies.

Result: no blocking regression; no delta below -0.5.
