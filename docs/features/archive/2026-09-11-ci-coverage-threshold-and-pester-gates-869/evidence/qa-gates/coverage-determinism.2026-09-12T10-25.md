# Coverage determinism proof (P6-T6)

Timestamp: 2026-09-14T20-04

Purpose: demonstrate that the measured coverage of the package-reference sync script is the same on two consecutive clean runs, which is the regression proof for the non-determinism issue #869 reports.

## Which measurement wrote the first document

The JaCoCo document present on disk as this task began was written by **P6-T4**, the post-uplift measurement. The P6-T5 authorized branch did not fire, so no re-measurement was performed under it and P6-T4's document is unambiguously the first of the two compared here.

## First document — read before this task's own run

Command: `pwsh -NoProfile -Command '<worktree prologue>; [xml]$j = Get-Content coverage/pester-coverage.xml -Raw; ...'`
EXIT_CODE: 0
Output:

```
RUN1 Sync-PackageReferences.ps1 LINEcovered=0 LINEmissed=84
```

- LINE covered: **0**
- LINE missed: **84**

## Second document — written by this task's own run

The same directory-scoped Pester command as P0-T8 and P6-T4 was run again, writing to the same explicit output path `coverage/pester-coverage.xml`.

EXIT_CODE: 0
Output:

```
PESTER Passed=174 Failed=0 Total=174
RUN2 Sync-PackageReferences.ps1 LINEcovered=0 LINEmissed=84
RUN2 REPORT LINE covered=731 missed=140
```

- LINE covered: **0**
- LINE missed: **84**

## The two pairs are equal

| Run | Document written by | LINE covered | LINE missed |
| --- | --- | --- | --- |
| 1 | P6-T4 | 0 | 84 |
| 2 | this task | 0 | 84 |

**The two pairs are equal.** The measured coverage of `scripts/vscode/Sync-PackageReferences.ps1` is identical on two consecutive clean runs.

The report-level LINE counter is also identical between the two runs, at 731 covered and 140 missed, and the test counts are identical at 174 passed and 0 failed. The measurement is therefore stable in the aggregate as well as for the file the defect affected.

## Contrast with the pre-fix pair

The P0-T11 artifact records the pre-fix per-file LINE counters, measured on the unchanged tree:

| File | Pre-fix (P0-T11) | Post-fix (this task) |
| --- | --- | --- |
| `Sync-PackageReferences.ps1` | 35 covered, 49 missed | 0 covered, 84 missed |
| `Invoke-VSBuild.ps1` | 36 covered, 7 missed | see note below |

The change in the sync script is the intended consequence of the fix, not a regression. Before the fix, `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` dot-sourced an unguarded production body that executed `Sync-PackageReferences.ps1` against the real repository root, and those 35 covered lines were an artefact of that accidental execution. How many of its lines executed depended on whether the `packages` directory had been restored and whether any HintPath was stale, which is exactly the non-determinism the issue reports: the issue records 53 of 84 on one run against 71 of 84 on another for this same file. With the invocation guard in place the sync script is no longer executed by any test, so it measures 0 of 84 — and it measures that same value on every run.

The build script's own lines were retained rather than lost: `Invoke-VSBuild.ps1` is now driven under mocks through `Invoke-VSBuildMain`, whose LINE counter the P6-T5 artifact records at 20 covered of 20, alongside covered counts on all three of its seams and its three pre-existing pure helpers. The aggregate LINE figure rose from 78.90 percent to 83.93 percent across the delivery, so removing the accidental execution did not cost the directory its floor.

## No project file was written

Command: `git -C "<repo-root>" status --porcelain=v1 --untracked-files=all -- '*.csproj'`
EXIT_CODE: 0
Output verbatim: empty.

Checked after both runs. Neither run wrote a project file. On the pre-fix tree this was the observable side effect of the accidental execution; the sync script's file-write call is now unreachable from the test suite because the script is never invoked by it.

Output Summary: the package-reference sync script measures 0 covered of 84 LINE entries on both consecutive runs, and the report-level counter is identical at 731 of 871 on both. The pre-fix pair was 35 covered of 84. No project file was written by either run.
