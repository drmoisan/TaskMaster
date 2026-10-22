# Phase 0 — Baseline PowerShell Pester run with coverage (P0-T8)

Timestamp: 2026-09-14T18-06

Command: `pwsh -NoProfile -Command '<worktree prologue>; Import-Module Pester -MinimumVersion 5.0.0 -Force; $c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = "scripts/vscode"; $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/pester-coverage.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=" + $r.PassedCount + " Failed=" + $r.FailedCount + " Skipped=" + $r.SkippedCount + " Total=" + $r.TotalCount; "PESTER COMMANDPERCENT=" + $r.CodeCoverage.CoveragePercent; "PESTER MODULEVERSION=" + (Get-Module Pester).Version.ToString()'`
EXIT_CODE: 0

## Printed result lines, verbatim

```
PESTER Passed=133 Failed=0 Skipped=0 Total=133
PESTER COMMANDPERCENT=79.0419161676647
PESTER MODULEVERSION=5.6.1
```

## The four counts

- Passed: 133
- Failed: 0
- Skipped: 0
- Total: 133

This task deliberately does not assert a zero failure count. Pester does not exit non-zero on a failing test case, so the count is read from the printed line rather than from the exit code. The count read is 0 on this run.

## Command coverage figure

PESTER COMMANDPERCENT = `79.0419161676647`

Pester's own console line corroborates it: `Covered 79.04% / 75%. 1,002 analyzed Commands in 14 Files.` The 14-file figure matches the 14 production PowerShell scripts now present under `scripts/vscode`, recorded in the P0-T4 artifact, and is two larger than the 12 the research record enumerated on 2026-09-12. This is the command (instruction) figure and carries no threshold; the LINE figure that the gate asserts is read in P0-T9.

## Pester module version

PESTER MODULEVERSION = `5.6.1`

Acceptance statement, recorded explicitly as the task requires: the recorded module version begins with `5.`. This is load-bearing rather than decorative. Two Pester versions are installed on this host, a 5.x and the legacy 3.4.0 that ships with Windows PowerShell, and an unqualified import binds by module-name resolution order and can bind 3.4.0, which has no `New-PesterConfiguration` command and no `CodeCoverage.OutputFormat` support, so the run would fail for a reason unrelated to the suite. The explicit `Import-Module Pester -MinimumVersion 5.0.0 -Force` in the payload is what excludes 3.4.0, and every other Pester invocation in this plan carries the same import for the same reason. Because the recorded version begins with `5.`, the halt branch did not fire, and `5.6.1` is the literal P7-T3 pins in the workflow file. No version literal in this plan is executor-chosen.

## Coverage document

`coverage/pester-coverage.xml` exists. Checked with `pwsh -NoProfile -Command '<worktree prologue>; Test-Path -LiteralPath "coverage/pester-coverage.xml"'`, which printed `True`.

The document is written to the explicit path the configuration sets rather than to the Pester default, so no repository-root `coverage.xml` is produced. P0-T16 records the confirming check.

## Pre-fix side effect

Command: `git -C "<repo-root>" status --porcelain=v1 -- '*.csproj'`
EXIT_CODE: 0
Output verbatim: empty.

No project file was modified by this run, so no restoration was required and the restoration branch of this task did not fire. The output being empty is recorded as the measurement, not assumed: the accidental execution of the package-reference sync script that `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` triggers still occurs on this pre-fix tree, but on this run it wrote no project file, because `nuget restore TaskMaster.sln` had already been run in P0-T5 and every HintPath therefore resolved, so the sync script's fix count stayed zero and its file-write call was never reached. That dependence on ambient restore state is precisely the non-determinism this delivery removes; the determinism proof in P6-T6 measures the post-fix behaviour against the pre-fix per-file counters recorded in P0-T11.

Output Summary: Pester 5.6.1 ran 133 test cases with 0 failures and 0 skips over `tests/scripts/vscode`, producing `coverage/pester-coverage.xml` in JaCoCo format over `scripts/vscode`. The informational command figure is 79.04 percent across 1,002 analyzed commands in 14 files. No project file was written.
