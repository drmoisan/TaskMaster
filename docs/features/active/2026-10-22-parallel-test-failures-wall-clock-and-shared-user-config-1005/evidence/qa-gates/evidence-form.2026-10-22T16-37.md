# Evidence Form Check (P7-T7)

Timestamp: 2026-10-22T16-37
Command: git diff --name-status <merge-base> -- <FEATURE>/evidence/ ; git status --porcelain --untracked-files=all -- <FEATURE>/evidence/ ; content search of <FEATURE>/evidence/ for a drive-rooted path (a letter, a colon, then a slash or backslash), a Users profile segment, the local account name and its 8.3 short form, and the per-user application data segment
EXIT_CODE: 0
Output Summary: PASS. 31 evidence files (2 committed preflight records plus 29 untracked from this run). No path ends in .trx or .coverage; the only .xml files are baseline-coverage-projection.2026-10-22T16-10.jacoco.xml and final-coverage-projection.2026-10-22T16-35.jacoco.xml; no path contains "cobertura"; the drive-rooted and user-profile search returned 0 matches.

## Listing

Tracked changes since MERGE-BASE under `evidence/` (git diff --name-status):

- `A evidence/other/preflight-fixes.2026-10-22T15-47.md`
- `A evidence/other/preflight-fixes.2026-10-22T16-01.md`

Untracked under `evidence/` (git status --porcelain, 29 entries):

- `baseline/`: `phase0-instructions-read.2026-10-22T16-05.md`, `baseline-toolchain-bootstrap.2026-10-22T16-06.md`, `merge-base-scope.2026-10-22T16-06.md`, `baseline-toolchain-format.2026-10-22T16-07.md`, `baseline-toolchain-analyzers.2026-10-22T16-07.md`, `baseline-toolchain-typecheck.2026-10-22T16-08.md`, `baseline-coverage-projection.2026-10-22T16-10.jacoco.xml`, `baseline-coverage.2026-10-22T16-10.md`, `baseline-file-sizes.2026-10-22T16-10.md`, `baseline-test-disposition.2026-10-22T16-11.md`
- `regression-testing/`: `ac4-search-before.2026-10-22T16-12.md`, `settings-probe-fail-before.2026-10-22T16-15.md`, `fail-before-exception.2026-10-22T16-15.md`, `settings-store-adapter-tests.2026-10-22T16-17.md`, `idlist-probe-pass-after.2026-10-22T16-18.md`, `taskmaster-settings-probe-pass-after.2026-10-22T16-25.md`, `dfdeedle-pass-after.2026-10-22T16-26.md`, `ac4-search-after.2026-10-22T16-27.md`
- `other/`: `ribbon-static-state-check.2026-10-22T16-23.md`
- `qa-gates/`: `parallel-constraints-scan.2026-10-22T16-28.md`, `scope-fence.2026-10-22T16-29.md`, `parallel-repeat-runs.2026-10-22T16-30.md`, `final-toolchain-format.2026-10-22T16-32.md`, `final-toolchain-analyzers.2026-10-22T16-32.md`, `final-toolchain-typecheck.2026-10-22T16-33.md`, `final-coverage-projection.2026-10-22T16-35.jacoco.xml`, `final-toolchain-tests.2026-10-22T16-35.md`, `coverage-comparison.2026-10-22T16-36.md`, `file-sizes.2026-10-22T16-37.md`

(This artifact is the 30th file written by the run.)

## Checks

| Check | Result |
| --- | --- |
| Paths ending `.trx` | 0 |
| Paths ending `.coverage` | 0 |
| `.xml` files other than `baseline-coverage-projection.*.jacoco.xml` / `final-coverage-projection.*.jacoco.xml` | 0 |
| Paths containing `cobertura` | 0 |
| Drive-rooted path or user-profile segment in evidence content | 0 matches |

Raw trx and post-processed Cobertura documents remain under the ignored `coverage/` folder and under `SCRATCH`; none is in the evidence tree.
