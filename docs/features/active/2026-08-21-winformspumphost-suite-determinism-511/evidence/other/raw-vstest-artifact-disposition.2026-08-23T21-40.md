# Disposition of the Raw vstest Artifacts

Timestamp: 2026-08-23T21-40
Decided by: orchestrator, at maintainer instruction during the resumed `/orchestrate` session
Canonical issue number for this feature is 511.

## What was removed

| Class | Files | Size |
| --- | --- | --- |
| `*.trx` (vstest TRX logs) | 56 | 358.3 MB |
| `*.coverage` (binary VS coverage attachments) | 42 | 822.2 MB |
| **Total** | **98** | **1,180.6 MB** |

Per-directory TRX counts at deletion time:

| Count | Directory (under `evidence/`) |
| --- | --- |
| 1 | `baseline/p0-t15` |
| 10 | `regression-testing/p1-t3` |
| 10 | `regression-testing/p1-t4` |
| 1 | `regression-testing/p2-t6` |
| 1 | `regression-testing/p3-t4` |
| 1 | `regression-testing/p3-t5` |
| 1 | `regression-testing/p3-t6` |
| 1 | `regression-testing/p3-t7` |
| 10 | `regression-testing/p4-t2` |
| 10 | `regression-testing/supplementary-node-contention` |
| 10 | `regression-testing/supplementary-node-contention-b` |

No non-markdown file other than these two classes existed under the evidence tree, so nothing else
was affected.

## Why they were removed rather than committed

1. **They were never committable.** `.gitignore:140` already excludes `*.coverage` repository-wide.
   `*.trx` was **not** excluded, which meant a `git add -A` would have staged roughly 358 MB of TRX
   into a repository whose entire pack is about 126 MiB. The repository's demonstrated policy is
   against committing raw machine test and coverage artifacts.

2. **They carry no audit value that is not already committed.** The distilled per-run markdown
   records are the evidence of record. Before deletion, the orchestrator independently re-derived
   the decisive figures directly from the raw TRX XML and compared them against the committed
   distillation `regression-testing/determinism-ten-runs.2026-08-21T18-10.md`:

   - per-run totals and failed counts for all ten `p4-t2` runs, which matched exactly;
   - the identity of the single failure in run 5,
     `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`, in the sibling-owned
     `UtilitiesCS.Test` assembly, which matched;
   - the per-run outcome of all four of this child's named tests, `Passed` in 10 of 10 runs, which
     matched.

   The committed markdown is therefore a faithful distillation, not a lossy summary of the claim.

3. **The deletion was explicitly instructed** by the maintainer, who directed that files which will
   eventually be committed be committed and that the remainder be deleted.

## What replaces them

- `regression-testing/determinism-ten-runs.2026-08-21T18-10.md` carries the ten TRX paths, per-run
  total and failed counts, per-run durations, the bracketing CPU-utilization samples, and the
  pre-fix versus post-fix comparison.
- `regression-testing/named-tests-ten-runs.2026-08-21T18-10.md` and
  `regression-testing/regression-tests-ten-runs.2026-08-21T18-10.md` carry the per-run tables for
  the four named tests.
- `regression-testing/webview-child-handle-measurement.2026-08-21T18-10.md` carries the four
  measured configurations that falsified the plan's premise.
- This artifact records what was deleted and why.

## Recurrence prevention

`evidence/.gitignore` now excludes `*.trx`, `*.coverage`, `*.coveragexml` and the
`vstest.console.exe` per-run scratch directories, so the remaining toolchain runs in this feature
cannot leak raw artifacts into a `git add -A`. That file is committed alongside this record.

## Reversibility

This deletion is not reversible from the repository. The runs that produced these artifacts are
reproducible by re-executing the commands recorded in each distilled markdown record, which carry
the exact `Command:` line and `Timestamp:` for every run.
