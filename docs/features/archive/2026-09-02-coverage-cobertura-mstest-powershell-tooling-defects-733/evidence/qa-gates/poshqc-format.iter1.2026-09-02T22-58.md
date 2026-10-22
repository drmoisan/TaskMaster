# P5-T1 — PoshQC format gate (Final QA Loop, iteration 1)

Timestamp: 2026-09-02T22-58

Command: `mcp__drm-copilot__run_poshqc_format` with
`workspace_root` = the item worktree repository root and
`scan_folders` = `["scripts/vscode", "tests/scripts/vscode"]`.

MCP payload (no exit code is emitted by this tool):
`{"ok":true,"tool":"run_poshqc_format","summary":"Ran bundled PoshQC format against the workspace root with 2 selected scan folder(s)."}`

EXIT_CODE: not emitted by the MCP tool. `ok` = true is the tool's success signal.

## Rewrite detection method

`mcp__drm-copilot__run_poshqc_format` is a write-mode command: it rewrites files in place and
reports `ok: true` whether or not it changed anything, so its payload alone cannot distinguish a
clean run from a repairing one. SHA-256 hashes of every file under both scan folders (21 files)
were captured immediately before and immediately after the run and compared.

## Output Summary

- No file rewritten. All 21 file hashes under `scripts/vscode` and `tests/scripts/vscode` are
  byte-identical before and after the format run, including all 13 files in this plan's write set.
- Because no file changed, the Final QA Loop does not restart on account of this task.

Unchanged hashes (SHA-256, first 16 hex characters), all in-scope files:

| File | Hash before | Hash after |
|---|---|---|
| scripts/vscode/Invoke-MSTest.ps1 | 2621F1EF76B651B9 | 2621F1EF76B651B9 |
| scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 | D46E707423D52F2B | D46E707423D52F2B |
| scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | BB84C28E577EB3CB | BB84C28E577EB3CB |
| scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1 | A6F057A086E4CC94 | A6F057A086E4CC94 |
| scripts/vscode/Invoke-MSTestWithCoverage.ps1 | 6B40FD3D73D732A7 | 6B40FD3D73D732A7 |
| scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1 | 00D96099A91DC7B4 | 00D96099A91DC7B4 |
| tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1 | 6C67112C9A741992 | 6C67112C9A741992 |
| tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 | 4762D3D86F82C956 | 4762D3D86F82C956 |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1 | BBB2BE59D45F132A | BBB2BE59D45F132A |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | 90D6BC4017D0D573 | 90D6BC4017D0D573 |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1 | 5F038A2DB5D1EA14 | 5F038A2DB5D1EA14 |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 | 01BF5D7D45CF0954 | 01BF5D7D45CF0954 |
| tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1 | 7F5822F5450FB5FD | 7F5822F5450FB5FD |

Out-of-write-set files in the same scan folders, also unchanged (no reversion required):
`scripts/vscode/Install-RepoDotNetSdk.ps1`, `scripts/vscode/Invoke-Restore.ps1`,
`scripts/vscode/Invoke-VSBuild.ps1`, `scripts/vscode/Sync-PackageReferences.ps1`,
`scripts/vscode/TaskMaster.cli.runsettings`, `scripts/vscode/TestProcessCleanup.ps1`,
`tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1`, `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1`.

## Drift check — git status --porcelain -- scripts/vscode tests/scripts/vscode

Captured immediately after the format run:

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
?? tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1
?? tests/scripts/vscode/Invoke-MSTestWithCoverage.PackageRate.Tests.ps1
?? tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1
```

Every reported path is in this plan's write set as enumerated for Phase 5 (6 production files,
7 test files). No out-of-write-set path was rewritten, so no `git checkout --` reversion was
performed. Reversions performed: none.
