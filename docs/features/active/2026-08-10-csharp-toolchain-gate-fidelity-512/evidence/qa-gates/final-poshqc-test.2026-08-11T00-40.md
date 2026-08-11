# Final QC step 3 (PowerShell) — PoshQC test and direct Pester with coverage ([P6-T3])

Timestamp: 2026-08-11T00-40

## Channel 1 — the MCP function

Command: `mcp__drm-copilot__run_poshqc_test` with `workspace_root = "C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ac1a08c3569adb7eb"` and `scan_folders = ["tests/scripts/vscode"]`
EXIT_CODE: 0

```json
{
  "ok": true,
  "tool": "run_poshqc_test",
  "workspace_root": "C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-ac1a08c3569adb7eb",
  "summary": "Ran bundled PoshQC test against 'C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-ac1a08c3569adb7eb' with 1 selected scan folder(s)."
}
```

`MCP_DETAIL_UNAVAILABLE: run_poshqc_test emits no numeric pass/fail counts`
(carried forward from [P0-T16], which established this.)

## Channel 2 — direct Pester with coverage (the authoritative numeric channel)

Command: the same dual-channel command defined in [P0-T16], delivered via
`pwsh -NoProfile -ExecutionPolicy Bypass -File coverage/run-pester-coverage.ps1` (the recorded
quoting deviation from [P0-T16] applies unchanged).

EXIT_CODE: 0

### Counts

| Metric | [P0-T16] baseline (direct channel) | [P6-T3] final | Assertion |
|---|---|---|---|
| `TotalCount` | 40 | **41** | baseline + 1 — PASS |
| `PassedCount` | 40 | **41** | baseline + 1 — PASS |
| **`FailedCount`** | 0 | **0** | zero failures — PASS |
| `SkippedCount` | 0 | 0 | — |
| Elapsed | 51.23 s | 57.12 s | — |

All five original `It` blocks in `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` plus the one added
by [P1-T1] pass:

```
[Passed] ConvertTo-MSBuildPropertyArgument.adds the /p: prefix for bare property assignments
[Passed] ConvertTo-MSBuildPropertyArgument.preserves an existing /p: prefix
[Passed] Get-MSBuildBuildArguments.returns each additional MSBuild property as a separate argument
[Passed] Get-MSBuildBuildArguments.emits /t:Rebuild in the target position when -Target Rebuild is supplied
[Passed] Get-RequestedMSBuildProperties.maps analyzer switches to the expected MSBuild properties
[Passed] Get-RequestedMSBuildProperties.emits no MSBuild property for the deprecated -EnableNullable switch
```

### Coverage for `scripts/vscode/Invoke-VSBuild.ps1`

| Metric | [P0-T16] baseline | [P6-T3] final |
|---|---|---|
| **Line Coverage** | **85.71%** | **85.71%** (`CoveragePercent` = 85.7142857142857) |
| Commands analyzed | 49 | **49** |
| Commands executed | 42 | **42** |
| Commands missed | 7 | **7** |
| Files analyzed | 1 | 1 |
| Pester headline | `Covered 85.71% / 75%. 49 analyzed Commands in 1 File.` | same |
| **Branch Coverage** | structurally unavailable | structurally unavailable |

A numeric **line**-coverage figure was obtained; no `UNVERIFIED` placeholder is used.

Uncovered commands, all in the un-seamed I/O tail, none in the region this feature edits (line
numbers shifted by the [P2-T1]-[P2-T4] insertions):

| [P0-T16] line | [P6-T3] line | Command |
|---|---|---|
| 37 | 42 | `throw 'MSBuildProperty entries must not be empty.'` |
| 124 | 134 | `throw "Solution not found: $resolvedSolutionPath"` |
| 129 | 139 | `throw 'vswhere.exe was not found. ...'` |
| 134 | 144 | `throw 'MSBuild.exe not found via vswhere. ...'` |
| 154 | 164 | `& $msbuildPath @msbuildArguments` |
| 155 | 165 | `if ($LASTEXITCODE -ne 0) { ... }` |
| 156 | 166 | `throw "MSBuild failed with exit code $LASTEXITCODE"` |

The missed-command **set** is unchanged; only their line numbers moved, consistent with the recorded
insertion deltas.

### `BranchCoverage:` — structural unavailability, with evidence

Pester version: **5.6.1**.

`$c.CodeCoverage.PSObject.Properties.Name`:

```
Enabled, OutputFormat, OutputPath, OutputEncoding, Path, ExcludeTests,
RecursePaths, CoveragePercentTarget, UseBreakpoints, SingleHitBreakpoints
```

`$r.CodeCoverage.PSObject.Properties.Name`:

```
CoveragePercent, CoveragePercentTarget, CoverageReport, CommandsAnalyzedCount,
CommandsExecutedCount, CommandsMissedCount, FilesAnalyzedCount, CommandsMissed,
CommandsExecuted, FilesAnalyzed
```

Neither list contains any branch-coverage counter. Pester 5's coverage model is **command-based**,
not branch-based. **Branch coverage is structurally unavailable from this runner.** This is recorded
on the same terms [P0-T16] granted and [P6-T4] consumes.

## `.csproj` sync guard

| Capture | `git status --porcelain -- '*.csproj'` |
|---|---|
| Immediately before this task | (empty) |
| Immediately after this task | (empty) |

Sync console line emitted: `Sync-PackageReferences: All HintPaths are up to date`. **No `.csproj` was
rewritten and no revert was required.**

## Tool-byproduct note

This run regenerated the untracked repository-root `coverage.xml` (Pester's default
`CodeCoverage.OutputPath`). It is not a repository source file. It is removed before [P6-T5] so the
CSharpier file count returns to the [P0-T8] baseline of 1517; the attribution is recorded in
`FEATURE/evidence/qa-gates/csharpier-check.2026-08-10T23-50.md`.

## Output Summary

MCP `run_poshqc_test` returned `EXIT_CODE: 0` with no numeric detail. The direct Pester 5.6.1 channel
returned `EXIT_CODE: 0` with **41 tests, 41 passed, 0 failed** — the [P0-T16] direct-channel baseline
of 40 plus one — and **85.71% line coverage** (42 of 49 commands) for
`scripts/vscode/Invoke-VSBuild.ps1`, identical to the baseline figure. Branch coverage is
structurally unavailable from Pester 5.6.1, evidenced by the two enumerated property lists. No
`.csproj` was rewritten.
