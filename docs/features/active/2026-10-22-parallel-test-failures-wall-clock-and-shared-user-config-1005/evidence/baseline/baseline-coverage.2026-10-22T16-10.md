# Baseline Coverage (TEST-COVERAGE)

Timestamp: 2026-10-22T16-10
Command: pwsh -NoProfile -File <repo>\scripts\vscode\Invoke-MSTestWithCoverage.ps1
EXIT_CODE: 0
Output Summary: Test Run Successful. Total 7599, executed 7599, passed 7599, failed 0 (trx summary). First-party coverage: lines 57542/66677 (86.30%), branches 14036/17419 (80.58%). Repository line 86.30% and branch 80.58% against the 80 and 75 percent floors. Per-file line coverage: IDList.cs 24.36% (38/156), AppQuickFilerSettings.cs 100% (30/30), AppStagingFilenames.cs 30.77% (20/65), AppAutoFileObjects.cs 45.66% (184/403). DfDeedle_COM_Tests executed: 26.

## Runner route

- Route: the unscoped script `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, invoked through PowerShell 7 with an absolute script path. It applies `scripts/vscode/TaskMaster.cli.runsettings` (Workers 0 -> 24 workers reported, Scope ClassLevel), `/InIsolation`, `TestCategory!=LiveOutlook`, and the `\.claude\` worktree exclusion, inside `dotnet-coverage collect`.
- Shell-icon carve-out: not used. The run did not stall (test time 41.6 s, script elapsed 95 s); the four shell-icon classes ran and passed.

## Trx summary (derived from the trx, raw trx not committed)

```
Test run outcome: Completed
Total 7599, executed 7599, passed 7599, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
```

## One-line first-party coverage summary

```
First-party coverage: lines 57542/66677 (86.30%), branches 14036/17419 (80.58%)
```

## Repository figures (post-processed Cobertura root)

| Metric | Value | Floor |
| --- | --- | --- |
| Line | 86.30% (57542/66677) | 80% |
| Branch | 80.58% (14036/17419) | 75% |

## Per-file line coverage (post-processed Cobertura, union of class line entries per file)

| File | Covered / Valid | Line % |
| --- | --- | --- |
| `ToDoModel/Data Model/ID/IDList.cs` | 38 / 156 | 24.36 |
| `TaskMaster/AppGlobals/AppQuickFilerSettings.cs` | 30 / 30 | 100.00 |
| `TaskMaster/AppGlobals/AppStagingFilenames.cs` | 20 / 65 | 30.77 |
| `TaskMaster/AppGlobals/AppAutoFileObjects.cs` | 184 / 403 | 45.66 |

## DfDeedle_COM_Tests count (for P5-T2)

- `DfDeedle_COM_Tests` tests executed: 26 (26 UnitTest definitions with class `UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests`, 26 results, all Passed).

## Failed tests

- None.

## Artifacts

- JaCoCo projection copy: `evidence/baseline/baseline-coverage-projection.2026-10-22T16-10.jacoco.xml` (no host path content).
- Post-processed Cobertura copy: `SCRATCH` `issue-1005-baseline-cobertura.xml` (not committed). Trx copy: `SCRATCH` `issue-1005-baseline.trx` (not committed).
