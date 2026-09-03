# P5-T1 — PoshQC format gate (Final QA Loop, iteration 3)

Timestamp: 2026-09-02T23-23

Iteration 3 was opened by the targeted remediation that closed P5-T5 criterion (d) on
`scripts/vscode/Invoke-MSTest.ps1`. That remediation edited three files and created one:

- `scripts/vscode/Invoke-MSTest.ps1` (entry-point body extracted into `Invoke-MSTestMain`,
  plus the new `Get-VsTestConsolePath` seam and the dot-source-guarded top-level wiring),
- `tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1` (new),
- `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` (BeforeAll comment and dot-source
  simplified now that the top-level body no longer runs on dot-source),
- `tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1` (same BeforeAll change).

## Command 1 — MCP format run

Command: `mcp__drm-copilot__run_poshqc_format` with
`workspace_root` = the item worktree repository root and
`scan_folders` = `["scripts/vscode", "tests/scripts/vscode"]`.

EXIT_CODE: n/a (MCP tool returns an ok/summary payload, no exit code)

MCP payload:

```
ok: true
tool: run_poshqc_format
summary: Ran bundled PoshQC format against the item worktree with 2 selected scan folder(s).
```

## Command 2 — Rewrite detection

Command: `pwsh -NoProfile -Command` computing a SHA-256 hash and line count for every `*.ps1`
file in both scan folders immediately before and immediately after the format run, followed by
`git status --porcelain -- scripts/vscode tests/scripts/vscode`.

EXIT_CODE: 0

All 21 hashes are byte-for-byte identical before and after the format run. No file was rewritten,
inside or outside this plan's write set, so no reversion was required.

Verbatim `git status --porcelain -- scripts/vscode tests/scripts/vscode` output:

```
 M scripts/vscode/Invoke-MSTest.ps1
 M scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1
 M scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
 M scripts/vscode/Invoke-MSTestWithCoverage.ps1
 M tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1
?? scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1
?? scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1
?? tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1
?? tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1
?? tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1
?? tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1
?? tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1
```

Every listed path is a prior-phase or remediation edit already inside this plan's write set under
`scripts/vscode/` or `tests/scripts/vscode/`. None of the seven files outside the write set that
live in these two folders (`Install-RepoDotNetSdk.ps1`, `Invoke-Restore.ps1`, `Invoke-VSBuild.ps1`,
`Sync-PackageReferences.ps1`, `TestProcessCleanup.ps1`, `Install-RepoDotNetSdk.Tests.ps1`,
`Invoke-VSBuild.Tests.ps1`) is reported as modified.

## File-size check against the 500-line ceiling

| File | Lines | Under 500 |
|---|---|---|
| scripts/vscode/Invoke-MSTest.ps1 | 202 | yes |
| scripts/vscode/Invoke-MSTestWithCoverage.ps1 | 350 | yes |
| scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | 469 | yes |
| scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 | 413 | yes |
| scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1 | 65 | yes |
| scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1 | 56 | yes |
| tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 | 488 | yes |
| tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1 | 79 | yes |
| tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1 | 144 | yes |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | 494 | yes |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1 | 486 | yes |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1 | 71 | yes |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 | 70 | yes |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1 | 15 | yes |

The new regression tests were placed in a new sibling file rather than appended to
`Invoke-MSTest.RunSettings.Tests.ps1`, because that file measured 488 lines before this iteration
and had 12 lines of headroom against the ceiling.

## Output Summary

- MCP format: `ok` true across both scan folders.
- No file rewritten: 21 of 21 SHA-256 hashes identical before and after.
- No out-of-scope path modified, so no `git checkout --` reversion was needed.
- The Final QA Loop does not restart on this step.
