# [P0-T8] PoshQC Pester test baseline

Timestamp: 2026-08-11T00-20
Command (policy record): `mcp__drm-copilot__run_poshqc_test` with
`workspace_root = C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a3f0c78078ca2265a` and
`scan_folders = ["tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1"]`
Command (paired direct run, source of every numeric value below):
`pwsh -NoProfile -Command 'Import-Module Pester -MinimumVersion 5.0; $c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = "scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1"; $c.CodeCoverage.OutputPath = "docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/baseline/pester-coverage.2026-08-11T00-20.xml"; $r = Invoke-Pester -Configuration $c; "Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Coverage=$($r.CodeCoverage.CoveragePercent)"; $hit = @($r.CodeCoverage.CommandsExecuted | Where-Object { $_.File -like "*Invoke-MSTestWithCoverage.ClosureFilter.ps1" }).Count; $miss = @($r.CodeCoverage.CommandsMissed | Where-Object { $_.File -like "*Invoke-MSTestWithCoverage.ClosureFilter.ps1" }).Count; "ClosureFilterCommands=$($hit+$miss) Executed=$hit Percent=$(if (($hit+$miss) -gt 0) { [math]::Round(100*$hit/($hit+$miss),2) } else { 0 })"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'`
EXIT_CODE: 0 (from the paired direct run)

MCP Result (verbatim):

```json
{"ok":true,"tool":"run_poshqc_test","workspace_root":"C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-a3f0c78078ca2265a","summary":"Ran bundled PoshQC test against 'C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-a3f0c78078ca2265a' with 1 selected scan folder(s)."}
```

`run_poshqc_test` returns only `{ok, tool, workspace_root, summary}` — no exit code, no counts, no
coverage. Every numeric value in this artifact comes from the paired direct Pester run, per the plan's
Conventions section.

`CodeCoverage.Path` for this task is exactly
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, because
`scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` does not exist yet and naming a
non-existent path would fault the run.

Pester Coverage Artifact: `docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/baseline/pester-coverage.2026-08-11T00-20.xml`

`ls coverage.xml` at the repository root returns "No such file or directory", confirming the mandatory
`CodeCoverage.OutputPath` redirection took effect and no stray repo-root artifact was written.

## Output Summary

```
Passed=19 Failed=0 Skipped=0 Coverage=90.2542372881356
ClosureFilterCommands=0 Executed=0 Percent=0
```

- Passed: **19**
- Failed: **0**
- Skipped: **0**
- PowerShell line/command coverage of `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`:
  **90.25%** (236 analyzed commands in 1 file; Pester reported `Covered 90.25% / 75%`)
- branch coverage: not emitted by Pester 5
- `ClosureFilterCommands=0 Executed=0 Percent=0` — expected and non-blocking at this task: the
  `ClosureFilter` module does not exist yet. This emission is load-bearing only at `[P3-T1]` and
  `[P3-T4]`.

Pester version: 5.6.1. Discovery found 19 tests in 1 file; all 19 passed in 10.7s.

## Deviation recorded (not acted on)

The plan's `[P3-T1]` states "Baseline for that file, measured at preflight: Passed=8, Failed=0,
Skipped=0. Post-change the file must report Passed=9". The measured baseline is **Passed=19**. That
plan figure was recorded against the pre-#441 form of
`tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`; issue #441 (PR #538, merged into
this branch's base at `fb257cd6e0c56cbf5eacf7e6a73641cc0414c930`) added 11 tests to that file. The
plan expectation is written against the pre-#441 file and is therefore documented as a deviation,
not treated as a reason to remove or weaken any landed test. The substantive intent of that clause —
"every pre-existing test in that file still passes, and the file gains exactly one test (case 6)" — is
preserved and re-measured at `[P3-T1]` against the corrected figures **19 -> 20**.

## Test names (all passing)

Describe `ConvertTo-KoverageCoberturaXml` (12):
preserves backslash separators for nested Windows paths while making them workspace-relative;
strips active and stale TaskMaster roots while preserving already relative paths;
merges duplicate class entries that point to the same source file;
normalizes stale TaskMaster roots before merging duplicate production class entries;
excludes .Test packages from the report and from the aggregate covered/valid line totals;
counts each source line once when methods repeat the class-level rollup;
counts each branch line once when methods repeat the class-level rollup;
computes the merged per-file line-rate from the merged rollup alone;
deduplicates a repeated line number by taking the maximum hits value;
retains method-level lines when the class-level rollup element is absent;
preserves the primary class methods subtree and every hits value when merging;
still throws when the document has no packages node.

Describe `Get-KoverageProjectAllowlist` (3):
excludes projects that resolve to a .Test assembly name;
retains non-test production projects in the allowlist;
applies the .Test exclusion to the project-file base-name fallback.

Describe `Get-CoberturaClassLineSummary` (4):
retains the candidate condition-coverage when its total is greater;
retains the candidate condition-coverage when totals tie and its covered count is greater;
retains the existing condition-coverage when neither precedence condition holds;
returns zero totals for a class with neither a lines nor a methods element.
