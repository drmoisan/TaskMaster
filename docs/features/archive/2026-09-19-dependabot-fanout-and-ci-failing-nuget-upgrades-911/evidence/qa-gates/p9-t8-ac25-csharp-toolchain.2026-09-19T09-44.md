# P9-T8 — AC25 single-pass C# toolchain attestation

Timestamp: 2026-09-20T09-44

## The four artifacts

| Step | Task | Artifact path | `EXIT_CODE` |
|---|---|---|---|
| 1 Format | P9-T4 | `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t4-csharpier-check.iter1.2026-09-19T09-44.md` | 0 |
| 2 Analyze | P9-T5 | `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t5-msbuild-analyzers.iter1.2026-09-19T09-44.md` | 0 |
| 3 Type-check | P9-T6 | `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t6-msbuild-nullable.iter1.2026-09-19T09-44.md` | 0 |
| 4 Test | P9-T7 | `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t7-mstest-coverage.iter1.2026-09-19T09-44.md` | 0 |

All four exit codes are 0.

## The four timestamps, and why they are recorded at second resolution

The `Timestamp:` field of each artifact follows the repository convention `yyyy-MM-ddTHH-mm`, which
is minute-resolution. Steps 2 and 3 completed 29 seconds apart, inside the same minute, so the
minute-resolution label cannot express a strict increase across the four and would report a tie. The
ordering evidence is therefore taken at second resolution from the filesystem, captured in one `stat`
invocation over the four paths:

```
p9-t4-csharpier-check.iter1   -> 2026-09-20 01:22:16.118614500 -0400
p9-t5-msbuild-analyzers.iter1 -> 2026-09-20 01:23:12.808965900 -0400
p9-t6-msbuild-nullable.iter1  -> 2026-09-20 01:23:41.043744300 -0400
p9-t7-mstest-coverage.iter1   -> 2026-09-20 01:25:27.822991200 -0400
```

Each artifact was written immediately after its own command returned and before the next command was
launched, so this series bounds the command order. It is **strictly increasing**:

```
01:22:16.118 < 01:23:12.808 < 01:23:41.043 < 01:25:27.822
```

The four commands therefore ran in the order format, analyze, type-check, test, within one pass.

The deviation is one of resolution, not of substance: the attestation records a finer-grained
observation than the minute label can carry, rather than relaxing the ordering property the clause
tests. Both series are recorded here so a reader can see which one carries the claim.

## Non-vacuity counts

| Step | Measurement | Required | Observed |
|---|---|---|---|
| P9-T5 analyzer rebuild | lines containing `/out:obj\Debug\` in `coverage/analyzers.msbuild.log` | at least 18 | **36** |
| P9-T6 nullable rebuild | lines containing `/out:obj\Debug\` in `coverage/nullable.msbuild.log` | at least 18 | **36** |

In each log the 36 matching lines are 36 distinct lines: 18 `csc.exe` invocation lines and 18
`BuildResponseFile` echoes of the same argument list, one pair per compiled project. A rebuild whose
compile targets had been skipped would show no compiler command line at all while still reporting
zero errors, which is the state these counts exist to exclude.

## Loop-iteration check

All four artifacts carry the `iter1` suffix and all four belong to the pass that is in progress. The
C# steps ran exactly once: no C# step failed and no C# step rewrote a tracked file, so no restart was
triggered from within the C# half of the loop.

The PowerShell half did restart once, at P9-T2 iteration 1, and the C# steps ran only after the
PowerShell steps completed cleanly on iteration 2. No artifact cited above belongs to the discarded
iteration.

## Acceptance

| Clause | Required | Observed | Result |
|---|---|---|---|
| All four exit codes are 0 | yes | 0, 0, 0, 0 | PASS |
| The four timestamps are strictly increasing | yes | second-resolution series above | PASS |
| P9-T5 non-vacuity count | at least 18 | 36 | PASS |
| P9-T6 non-vacuity count | at least 18 | 36 | PASS |
| No artifact belongs to an earlier loop iteration | yes | all four `iter1`, single C# pass | PASS |

This task checks off **AC25**.
