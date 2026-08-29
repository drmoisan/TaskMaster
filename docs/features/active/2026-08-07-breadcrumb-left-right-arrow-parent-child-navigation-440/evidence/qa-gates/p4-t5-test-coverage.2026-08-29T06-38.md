# Phase 4 — Test-With-Coverage Step, Full Suite (issue #440, plan task P4-T5)

Timestamp: 2026-08-29T06-38

Command:

```
pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\final440.cobertura.xml
```

The coverage filename ends in `.cobertura.xml` per Global rule 8, so the P4-T2
formatting verification cannot pick it up. This run is broad enough to detect a
regression in the QuickFiler consumers of the changed state machine, which the scoped
Phase 3 runs alone would not guarantee.

EXIT_CODE: 0

## Output Summary

### (a) Run summary, verbatim

```
Test Run Successful.
Total tests: 6859
     Passed: 6859
```

There is no `Failed:` line and no `Failed` result line anywhere in the captured
output. The runner reported a readable total on the first execution, so no second
execution was required or performed.

### (b) Test counts

- `FinalTotalTests`: **6859**
- `FinalPassedTests`: **6859**

Gate: `FinalTotalTests` must be at or above `BaselineTotalTests` plus 2. The P0-T13
baseline was 6857, so the floor is 6859. Observed 6859. PASS. The change adds exactly
the two tests P1-T1 and P1-T2 created and removes none.

### (c) Failure set

- `FinalFailureSet`: **none** (empty).
- `BaselineFailureSet` recorded by P0-T13: **none** (empty).

Gate: `FinalFailureSet` must be a subset of `BaselineFailureSet`. The empty set is a
subset of the empty set. PASS.

### (d) Repository-wide coverage, from the root `coverage` element

| Attribute | Verbatim value |
| --- | --- |
| `line-rate` | `0.853026` |
| `branch-rate` | `0.792558` |
| `lines-covered` | `54760` |
| `lines-valid` | `64195` |
| `branches-covered` | `13036` |
| `branches-valid` | `16448` |

- `FinalLineCoveragePercent`: **85.3026**
- `FinalBranchCoveragePercent`: **79.2558**

### (e) Per-file counters for `UtilitiesCS\OutlookObjects\Folder\BreadcrumbStateModel.cs`

Obtained by the same method P0-T13 used: dot-source
`scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1` and call
`Get-CoberturaClassLineSummary` on the single `class` element whose `filename`
attribute equals that path. Exactly one `class` element matched
(`MATCHING_CLASS_NODES=1`).

- `FinalFileTotalLines`: **120**
- `FinalFileCoveredLines`: **118**
- `FinalFileTotalBranches`: **42**
- `FinalFileCoveredBranches`: **39**

Derived uncovered counts:

- Final uncovered lines: 120 - 118 = **2**
- Final uncovered branches: 42 - 39 = **3**

### (f) Coverage-threshold branch (Global rule 7)

The threshold branch was **not** taken. The wrapper printed
`Post-processing coverage XML for Koverage compatibility...` followed by
`Done. Coverage artifact: <repo-root>\coverage\final440.cobertura.xml`, which only
occurs when `Assert-CoberturaLineCoverageThreshold` did not throw and the write-back
executed. No `COVERAGE-THRESHOLD-THROW:` record was produced, and no raw
pre-processing document needed to be copied aside or converted in memory.

### Discovery

Discovered test assemblies: 9, the same set P0-T13 recorded. No discovered path
expressed relative to the repository root contains a `.claude` segment, so no
`CONTAMINATED-DISCOVERY:` record was produced.

## Redaction note

The raw Cobertura document and the captured run log remain under the gitignored
`coverage/` tree and are not copied under this feature folder.
