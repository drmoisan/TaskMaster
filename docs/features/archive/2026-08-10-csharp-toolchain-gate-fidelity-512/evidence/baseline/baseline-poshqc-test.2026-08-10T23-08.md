# Baseline — PoshQC test (MCP) and direct Pester with coverage ([P0-T16])

Timestamp: 2026-08-10T23-08

## Channel 1 — the MCP function (for the record)

Command: `mcp__drm-copilot__run_poshqc_test` with `workspace_root = "C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ac1a08c3569adb7eb"` and `scan_folders = ["tests/scripts/vscode"]`
EXIT_CODE: 0

Return payload, verbatim:

```json
{
  "ok": true,
  "tool": "run_poshqc_test",
  "workspace_root": "C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-ac1a08c3569adb7eb",
  "summary": "Ran bundled PoshQC test against 'C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-ac1a08c3569adb7eb' with 1 selected scan folder(s)."
}
```

`MCP_DETAIL_UNAVAILABLE: run_poshqc_test emits no numeric pass/fail counts`

The payload carries only `ok`, `tool`, `workspace_root` and `summary`. It emits no pass/fail counts,
no per-`It` enumeration, and no coverage figure. `ok: true` is recorded as `EXIT_CODE: 0`, matching
the expected value. **The cross-channel numeric-agreement assertion is therefore not applicable**;
all numeric counts below are taken from the direct channel, which is authoritative for this feature.

## Channel 2 — direct Pester (the authoritative numeric channel)

Command (statements exactly as the plan prescribes):

```powershell
$c = New-PesterConfiguration
$c.Run.Path = 'tests/scripts/vscode'
$c.CodeCoverage.Enabled = $true
$c.CodeCoverage.Path = 'scripts/vscode/Invoke-VSBuild.ps1'
$c.Output.Verbosity = 'Detailed'
$c.Run.PassThru = $true
Invoke-Pester -Configuration $c
```

EXIT_CODE: 0

**Recorded quoting deviation.** The plan gives this as a single-quoted `pwsh -NoProfile -Command`
payload with doubled single quotes, in order to prevent the parent shell interpolating `$c`. The
available parent shell here is Bash, which cannot express a single-quoted string containing single
quotes. The identical statements were therefore delivered through
`pwsh -NoProfile -ExecutionPolicy Bypass -File coverage/run-pester-coverage.ps1`. This **strengthens**
the non-interpolation property the plan requires: a script file is parsed only by PowerShell, so `$c`
cannot be expanded by any parent. `$c.Run.PassThru = $true` was added because Pester 5 returns no
result object without it, and the numeric counts below are unobtainable otherwise.

### Numeric results

| Metric | Value |
|---|---|
| Pester version | **5.6.1** |
| Test files discovered | 4 |
| `TotalCount` | **40** |
| `PassedCount` | **40** |
| `FailedCount` | **0** |
| `SkippedCount` | 0 |
| Process `$LASTEXITCODE` | 0 |
| Elapsed | 51.23 s |

**Baseline total test count = 40.** [P2-T7] and [P6-T3] assert 40 + 1 = **41** after [P1-T1] adds one
`It`.

The five `It` blocks in `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` (the file this feature
modifies) all pass at the merge base:

```
ConvertTo-MSBuildPropertyArgument.adds the /p: prefix for bare property assignments
ConvertTo-MSBuildPropertyArgument.preserves an existing /p: prefix
Get-MSBuildBuildArguments.returns each additional MSBuild property as a separate argument
Get-RequestedMSBuildProperties.maps analyzer switches to the expected MSBuild properties
Get-RequestedMSBuildProperties.maps nullable switches to the expected MSBuild properties
```

### Coverage for `scripts/vscode/Invoke-VSBuild.ps1`

| Metric | Value |
|---|---|
| **Line Coverage** | **85.71%** (`CoveragePercent` = 85.7142857142857) |
| Commands analyzed | 49 |
| Commands executed | 42 |
| Commands missed | 7 |
| Files analyzed | 1 |
| Pester headline | `Covered 85.71% / 75%. 49 analyzed Commands in 1 File.` |
| **Branch Coverage** | **structurally unavailable — see below** |

85.71% is **at or above** the `.claude/rules/powershell.md` floor of 85%.
**No `PREEXISTING_COVERAGE_SHORTFALL:` is recorded**, because the measured line coverage meets the
policy floor.

Uncovered commands (all in the un-seamed I/O tail, none in the pure region this feature edits):

| Line | Command |
|---|---|
| 37 | `throw 'MSBuildProperty entries must not be empty.'` |
| 124 | `throw "Solution not found: $resolvedSolutionPath"` |
| 129 | `throw 'vswhere.exe was not found. Install Visual Studio 2022 (or Build Tools) with MSBuild components.'` |
| 134 | `throw 'MSBuild.exe not found via vswhere. Install Visual Studio MSBuild components.'` |
| 154 | `& $msbuildPath @msbuildArguments` |
| 155 | `if ($LASTEXITCODE -ne 0) { ... }` |
| 156 | `throw "MSBuild failed with exit code $LASTEXITCODE"` |

### `BranchCoverage:` — structural unavailability, with evidence

Pester version inspected: **5.6.1**.

`PesterConfiguration.CodeCoverage` exposes exactly these properties (enumerated at run time from
`$c.CodeCoverage.PSObject.Properties.Name`):

```
Enabled, OutputFormat, OutputPath, OutputEncoding, Path, ExcludeTests,
RecursePaths, CoveragePercentTarget, UseBreakpoints, SingleHitBreakpoints
```

The result object `$r.CodeCoverage` exposes exactly these properties:

```
CoveragePercent, CoveragePercentTarget, CoverageReport, CommandsAnalyzedCount,
CommandsExecutedCount, CommandsMissedCount, FilesAnalyzedCount, CommandsMissed,
CommandsExecuted, FilesAnalyzed
```

Neither list contains any branch-coverage counter. Pester 5's code-coverage model is
**command-based**, not branch-based: it instruments commands and reports executed/analyzed command
counts. **Branch coverage is structurally unavailable from this runner.** Per [P0-T16]'s acceptance,
that statement with this evidence is the only permitted substitute for the number, and [P6-T4]
consumes it on the same terms.

## `.csproj` sync guard

`tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` dot-sources `scripts/vscode/Invoke-VSBuild.ps1` with
`-NoExecute` (line 6), which reaches the unconditional `Sync-PackageReferences.ps1` call at line 144
before the `-NoExecute` early return at line 150.

| Capture | `git status --porcelain -- '*.csproj'` |
|---|---|
| Immediately before this task | (empty) |
| Immediately after this task | (empty) |

Sync console line emitted: `Sync-PackageReferences: All HintPaths are up to date` — i.e. it changed
nothing (the `$fixCount -eq 0` early return at `Sync-PackageReferences.ps1:112` guards the
`WriteAllText` at :148). **No `.csproj` was rewritten and no revert was required.**

## Output Summary

MCP `run_poshqc_test` returned `EXIT_CODE: 0` with no numeric detail
(`MCP_DETAIL_UNAVAILABLE`). The direct Pester 5.6.1 channel returned `EXIT_CODE: 0` with **40 tests,
40 passed, 0 failed**, and **85.71% line coverage** (42 of 49 commands) for
`scripts/vscode/Invoke-VSBuild.ps1`, which meets the 85% floor. Branch coverage is structurally
unavailable from Pester 5.6.1, evidenced by the two enumerated property lists. No `.csproj` was
rewritten.
