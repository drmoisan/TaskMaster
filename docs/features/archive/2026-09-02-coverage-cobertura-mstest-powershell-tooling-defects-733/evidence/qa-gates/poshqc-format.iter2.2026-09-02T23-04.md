# P5-T1 — PoshQC format gate (Final QA Loop, iteration 2)

Timestamp: 2026-09-02T23-04

Loop restart reason: P5-T2 on iteration 1 modified `scripts/vscode/Invoke-MSTest.ps1` to resolve
the newly introduced `PSUseOutputTypeCorrectly` diagnostic, so the toolchain loop restarted from
the formatting step.

Command: `mcp__drm-copilot__run_poshqc_format` with
`workspace_root` = the item worktree repository root and
`scan_folders` = `["scripts/vscode", "tests/scripts/vscode"]`.

MCP payload:
`{"ok":true,"tool":"run_poshqc_format","summary":"Ran bundled PoshQC format against the workspace root with 2 selected scan folder(s)."}`

EXIT_CODE: not emitted by the MCP tool. `ok` = true is the tool's success signal.

## Rewrite detection method

SHA-256 hashes of all 21 files under both scan folders captured immediately before and
immediately after the run, because this write-mode tool reports `ok: true` whether or not it
rewrote anything.

## Output Summary

- No file rewritten. All 21 hashes are byte-identical before and after, including
  `scripts/vscode/Invoke-MSTest.ps1` at 9D7A04B8D6CF496D, which carries the iteration-1
  remediation. The formatter accepted that edit without reformatting it.
- The unary comma in `Get-MSTestAssemblyPathList`'s return statement
  (`scripts/vscode/Invoke-MSTest.ps1` line 100) survived the format run unchanged, confirmed both
  by the unchanged file hash and by direct search of the file. The array-shape assertions added by
  task H1 re-run as part of P5-T4 on this iteration.
- No out-of-write-set path was rewritten, so no `git checkout --` reversion was performed on this
  iteration. Reversions performed: none.

Unchanged hashes (SHA-256, first 16 hex characters), all in-scope files:

| File | Hash before | Hash after |
|---|---|---|
| scripts/vscode/Invoke-MSTest.ps1 | 9D7A04B8D6CF496D | 9D7A04B8D6CF496D |
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

Every reported path is in this plan's Phase 5 write set (6 production files, 7 test files).
`scripts/vscode/Invoke-VSBuild.ps1`, which the iteration-1 autofix had rewritten and P5-T3
reverted, is correctly absent from this list, confirming the reversion held.

This is the final format iteration: it changed no file, so the loop does not restart on account
of formatting.
