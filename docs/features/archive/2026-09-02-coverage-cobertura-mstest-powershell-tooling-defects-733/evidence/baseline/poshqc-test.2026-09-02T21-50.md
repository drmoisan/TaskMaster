# Phase 0 — Pester Test and Coverage Baseline (P0-T7)

Timestamp: 2026-09-02T21-50

Task: [P0-T7]

## Command 1 — MCP test run

Command: mcp__drm-copilot__run_poshqc_test
  workspace_root = the item worktree repository root for this run
  scan_folders = ["scripts/vscode", "tests/scripts/vscode"]

EXIT_CODE: not applicable — this MCP tool returns no exit code, no pass/fail/skip counts, no
per-test names, and no coverage figure. The returned payload is recorded verbatim below in
place of one, and the numeric evidence comes from Command 2.

MCP payload:

```
ok: true
tool: run_poshqc_test
workspace_root: <item worktree repository root>
summary: Ran bundled PoshQC test against '<item worktree repository root>' with 2 selected scan folder(s).
```

## Command 2 — Direct Pester run with coverage

Command: pwsh -NoProfile -Command with a single-quoted outer wrapper and a double-quoted inner
script, importing Pester 5, building a New-PesterConfiguration with
`Run.Path = "tests/scripts/vscode"`, `Run.PassThru = $true`,
`Output.Verbosity = "Detailed"`, `CodeCoverage.Enabled = $true`,
`CodeCoverage.Path` set to the four existing production files, and
`CodeCoverage.OutputPath` set to the baseline XML path below, then the explicit trailing
branch `if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }`.

EXIT_CODE: 0

Pester version: 5.6.1. PSScriptAnalyzer version: 1.25.0.

Coverage XML written to:
docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733/evidence/baseline/pester-coverage.2026-09-02T21-50.xml

## (a) Overall counts

Passed: 70
Failed: 0
Skipped: 0
Total: 70

Pester's own summary line: `Tests Passed: 70, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 0`.
Run duration: 15.78s.

Note on scope: `Run.Path` is the whole tests/scripts/vscode folder, so the 70 includes two
test files outside this plan's write set
(tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1 and
tests/scripts/vscode/Invoke-VSBuild.Tests.ps1). Their counts are listed below for
completeness so the overall total reconciles.

## (b) Per-test-file counts

| Test file | Passed | Failed | Skipped |
|---|---|---|---|
| tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | 25 | 0 | 0 |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1 | 11 | 0 | 0 |
| tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 | 26 | 0 | 0 |
| tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1 (out of write set) | 2 | 0 | 0 |
| tests/scripts/vscode/Invoke-VSBuild.Tests.ps1 (out of write set) | 6 | 0 | 0 |

25 + 11 + 26 + 2 + 6 = 70, reconciling with the overall total.

## (c) Per-production-file coverage

Derived from `$r.CodeCoverage.CommandsExecuted` and `$r.CodeCoverage.CommandsMissed`,
filtered by each entry's `.File` property, because
`$r.CodeCoverage.CoveragePercent` is a single aggregate across all four analyzed files and
cannot render a per-file verdict.

| Production file | Executed | Missed | Total commands | Percent |
|---|---|---|---|---|
| scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | 230 | 25 | 255 | 90.2 |
| scripts/vscode/Invoke-MSTestWithCoverage.ps1 | 100 | 11 | 111 | 90.09 |
| scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 | 111 | 0 | 111 | 100 |
| scripts/vscode/Invoke-MSTest.ps1 | 31 | 14 | 45 | 68.89 |

Aggregate across all four files: `$r.CodeCoverage.CoveragePercent` = 90.4214559386973
(522 analyzed commands in 4 files).

Baseline observation, recorded for the Phase 5 delta comparison: only
scripts/vscode/Invoke-MSTest.ps1 is below the uniform 85 percent line-coverage floor at
baseline, at 68.89 percent. Its 14 missed commands are concentrated in the bare top-level
script body (lines 92, 99, 104, 109, 119, 120, 122, 124, 128, 129, 130) and two wrapper
seams. This is the region Phase 4's `Get-MSTestAssemblyPathList` extraction makes testable.
No file coverage figure is changed by Phase 0; this is a record of the pre-change state.

## (d) Branch coverage

branch coverage: not emitted by Pester 5.

This is a measured fact, not a placeholder. Pester 5.6.1 reports command (instruction) and
line coverage only; no branch-coverage figure appears anywhere in its result object or in the
JaCoCo XML it writes. The uniform 75 percent branch-coverage threshold in
.claude/rules/quality-tiers.md does not apply to PowerShell for exactly this reason, per
.claude/rules/powershell.md and .claude/rules/general-unit-test.md. The `/ 75%` shown in
Pester's own console line `Covered 90.42% / 75%` is Pester's built-in default
CoveragePercentTarget for LINE coverage, not a branch figure and not a repository gate.

## Output Summary

Baseline is green: 70 passed, 0 failed, 0 skipped, direct-run EXIT_CODE 0. Per-file test
counts for the three in-scope test files are 25, 11, and 26. Per-file command coverage for the
four production files is 90.2, 90.09, 100, and 68.89 percent respectively, with an aggregate
of 90.42 percent over 522 analyzed commands. Branch coverage is not emitted by Pester 5. The
coverage XML carries no absolute host path, verified by search.
