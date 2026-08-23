# Zero-regression contract

Timestamp: 2026-07-21T16-05Z

Command: `Compare P0-T6 through P0-T10 canonical baseline evidence`

EXIT_CODE: 0

## Effective baseline

| Gate | Numeric baseline | Permitted baseline debt |
|---|---:|---|
| CSharpier | 1408 checked; 0 unformatted | None |
| Analyzer build | 0 errors; 5 warnings | System.Reactive `packages.config` compatibility warning in five named projects |
| Nullable build | 0 errors; 5 warnings | Same five System.Reactive package-management warnings; no nullable/compiler warning |
| MSTest with coverage | 5713 passed; 0 failed; 0 skipped | None |
| Repository line coverage | 87397/104178; 0.838920 | Existing uncovered lines may remain only when all absolute and changed-scope final thresholds pass |

The initial unrestored-worktree build failures recorded in P0-T7 and P0-T8 are environment setup history, not permitted final diagnostics. The successful post-restore results are the effective diagnostic baseline.

## Final comparison rules

- Final analyzer, nullable, and test diagnostics may not add any identity, file, failure, or skipped test to the effective baseline set.
- Repository-wide line coverage must be at least 80%.
- Every new class and method must have at least 90% line coverage.
- Every new or changed selector type must have at least 90% line coverage.
- Changed-line coverage may not regress from the numeric baseline and must satisfy the plan's absolute changed-scope threshold.
- Existing low or absent per-file baselines, including the two uncovered `ItemViewer` partial filenames and `QfcItemController.ViewerSetup.cs`, are not exceptions for changed code.
- Any unavailable final numeric value forces `REMEDIATION_REQUIRED`; it cannot be reported as `PASS`.

Output Summary: P0-T6 through P0-T10 establish a clean formatter baseline, a five-warning package-management diagnostic allowance, zero test failures, and numeric coverage thresholds for final zero-regression comparison.
