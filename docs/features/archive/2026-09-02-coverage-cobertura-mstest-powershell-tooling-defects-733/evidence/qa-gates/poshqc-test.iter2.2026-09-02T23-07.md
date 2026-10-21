# P5-T4 — Pester test and coverage gate (Final QA Loop, iteration 2, final)

Timestamp: 2026-09-02T23-07

## Command 1 — MCP test run

Command: `mcp__drm-copilot__run_poshqc_test` with
`workspace_root` = the item worktree repository root and
`scan_folders` = `["scripts/vscode", "tests/scripts/vscode"]`.

MCP payload:

```
ok: true
tool: run_poshqc_test
summary: Ran bundled PoshQC test against the workspace root with 2 selected scan folder(s).
```

EXIT_CODE: not emitted. This MCP tool returns no exit code, no pass/fail/skip counts, no
per-test names, and no coverage figure, which is why the plan's Conventions pair it with the
direct Pester run below. The numeric evidence comes from Command 2.

## Command 2 — Direct Pester run with coverage

Command: `pwsh -NoProfile -Command` with a single-quoted outer wrapper and a double-quoted inner
script, building a `New-PesterConfiguration` with `Run.Path` set to the 7 test files in this
plan's Phase 5 write set, `Run.PassThru = $true`, `CodeCoverage.Enabled = $true`,
`CodeCoverage.Path` set to the 6 production files in this plan's Phase 5 write set,
`CodeCoverage.OutputPath` set to the XML path below, and the explicit trailing branch
`if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }`.

EXIT_CODE: 0

Pester version: 5.6.1.

Coverage XML written to:
`docs/features/active/2026-09-02-coverage-cobertura-mstest-powershell-tooling-defects-733/evidence/qa-gates/pester-coverage.final-qc.iter2.2026-09-02T23-07.xml`

The XML was searched for absolute host paths and contains none: no account name, no drive-letter
prefix, and no worktree path segment appears anywhere in it.

## Write set used

The plan's Conventions section, written before Phases 1 and 4 ran, could not name the files those
phases created under the plan's own authority. The enumerated Phase 5 write set is therefore:

Production (6), all supplied to `CodeCoverage.Path`:
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`,
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`,
`scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1`,
`scripts/vscode/Invoke-MSTest.ps1`,
`scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1`,
`scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`.

Tests (7), all supplied to `Run.Path`:
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`,
`tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`,
`tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`,
`tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1`,
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1`,
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1`,
`tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1`.

`scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` is included in the coverage denominator.
Omitting it would have made the at-or-above-85-percent assertion pass vacuously for a production
file this item created.

## Counts

Pester's own summary line: `Tests Passed: 73, Failed: 0, Skipped: 0, Inconclusive: 0, NotRun: 0`.
Discovery found 73 tests in 7 files. Run duration 17.83s.

Passed: 73
Failed: 0
Skipped: 0
Total: 73

Per-test-file counts:

| Test file | Passed | Failed | Skipped |
|---|---|---|---|
| tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | 20 | 0 | 0 |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1 | 12 | 0 | 0 |
| tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 | 27 | 0 | 0 |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 | 2 | 0 | 0 |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1 | 2 | 0 | 0 |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1 | 5 | 0 | 0 |
| tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1 | 5 | 0 | 0 |

20 + 12 + 27 + 2 + 2 + 5 + 5 = 73, reconciling with the overall total.

## Per-production-file coverage

Derived from `$r.CodeCoverage.CommandsExecuted` and `$r.CodeCoverage.CommandsMissed` filtered by
each entry's `.File` property, because `$r.CodeCoverage.CoveragePercent` is a single aggregate
across all six analyzed files and cannot render a per-file verdict.

| Production file | Executed | Missed | Total commands | Percent | At or above 85% |
|---|---|---|---|---|---|
| scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | 228 | 23 | 251 | 90.84 | yes |
| scripts/vscode/Invoke-MSTestWithCoverage.ps1 | 100 | 11 | 111 | 90.09 | yes |
| scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 | 111 | 0 | 111 | 100 | yes |
| scripts/vscode/Invoke-MSTest.ps1 | 34 | 13 | 47 | 72.34 | NO |
| scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1 | 25 | 0 | 25 | 100 | yes |
| scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1 | 15 | 2 | 17 | 88.24 | yes |

Aggregate: `$r.CodeCoverage.CoveragePercent` = 91.2811387900356 over 562 analyzed commands in
6 files. Pester's own console line: `Covered 91.28% / 75%. 562 analyzed Commands in 6 Files.`

Branch coverage: not emitted by Pester 5. This is a measured fact, not a placeholder — Pester
5.6.1 reports command and line coverage only, and no branch figure appears in its result object
or in the JaCoCo XML it writes. The `/ 75%` in Pester's console line is its built-in default
`CoveragePercentTarget` for line coverage, not a branch figure.

## Coverage shortfall — scripts/vscode/Invoke-MSTest.ps1

This file is at 72.34 percent, below the uniform 85 percent line-coverage floor. It is recorded
as a measured gap. No file was exempted and no production file was excluded from measurement;
`.claude/rules/general-unit-test.md`'s Coverage Exclusion Policy prohibits both.

The 13 missed commands sit on 12 lines, all in the top-level script body or in process-launch
seams that cannot execute in a test host:

| Line | Construct |
|---|---|
| 31 | `throw` in `Resolve-RunSettingsPath` when the runsettings file is absent |
| 74 | `& $VsTestPath @VsTestArgs` — the external-process invocation inside `Invoke-VsTestExe` |
| 124 | `throw` when the resolved search root does not exist |
| 131 | `throw` when `vswhere.exe` is absent |
| 136 | `throw` when `vstest.console.exe` is not found via vswhere |
| 145, 146 | the two `Write-Host` progress lines in the top-level body (146 carries 2 commands) |
| 148 | the top-level `Get-VsTestArgumentList` call |
| 150 | the top-level `if ($NoExecute)` early return guard |
| 154 | the top-level `Invoke-VsTestExe` call |
| 155, 156 | the top-level `$LASTEXITCODE` check and its `throw` |

Reaching them requires either extracting the entire remaining top-level body into functions, or
launching `vswhere.exe` and `vstest.console.exe` for real. Neither is a task in this plan.

## Comparison note

Baseline (P0-T7) recorded this file at 68.89 percent (31 executed, 14 missed, 45 total). It is
now at 72.34 percent (34 executed, 13 missed, 47 total): 2 commands were added by P4-T4's
`Get-MSTestAssemblyPathList` extraction and 3 more commands are now executed, so the figure moved
up 3.45 percentage points. The shortfall is pre-existing and was reduced, not introduced, by this
plan. The full delta analysis is in the P5-T5 artifact.

## Output Summary

- MCP test run: `ok` true.
- Direct Pester run: EXIT_CODE 0. Passed 73, Failed 0, Skipped 0, Total 73.
- Per-file coverage: 90.84, 90.09, 100, 72.34, 100, 88.24 percent. Aggregate 91.28 percent over
  562 commands in 6 files.
- Five of six production files meet the 85 percent floor. `scripts/vscode/Invoke-MSTest.ps1` at
  72.34 percent does not, and is reported as an open gap.
- No test failed and no file was changed by this task, so the Final QA Loop does not restart. This
  is the final iteration: format (iteration 2) rewrote nothing, analyze (iteration 2) matched the
  baseline diagnostic set exactly, type-check is Not Applicable for PowerShell, and this test run
  is green.
