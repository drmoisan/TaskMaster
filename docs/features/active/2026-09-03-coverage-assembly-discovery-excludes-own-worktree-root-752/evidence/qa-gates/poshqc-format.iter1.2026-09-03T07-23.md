# PoshQC Format Stage, Iteration 1 ([P3-T1])

Timestamp: 2026-09-03T12-10

Command:
1. `pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $f = @("scripts/vscode/Invoke-MSTestWithCoverage.ps1","tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1"); foreach ($x in $f) { "PREHASH " + $x + " " + (Get-FileHash -LiteralPath $x -Algorithm SHA256).Hash }; exit 0'`
2. `mcp__drm-copilot__run_poshqc_format` with `workspace_root` of `<repo-root>` and `scan_folders` of `scripts/vscode` and `tests/scripts/vscode`
3. the same hash command as 1, with `PREHASH` replaced by `POSTHASH`
4. `git -C <repo-root> status --porcelain -uall -- scripts/vscode tests/scripts/vscode`

EXIT_CODE: 0

BRANCH: A

SCAN FOLDERS USED: `scripts/vscode`, `tests/scripts/vscode`

Branch A is in force because the `[P0-T5]` probe artifact records `DRIFT-IN-PRODUCTION-FILE: NONE`.

## Tool payload

Recorded as returned, with one class-level substitution applied at capture time: the tool echoes the `workspace_root` argument, which is this item's absolute worktree root; that one value is written as `<repo-root>` per this plan's Host-path hygiene rule. Every other character is as returned.

```
{"ok":true,"tool":"run_poshqc_format","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC format against '<repo-root>' with 2 selected scan folder(s)."}
```

## Hash pair

```
PREHASH scripts/vscode/Invoke-MSTestWithCoverage.ps1 D21109DC38DB03A2B1800E91C5F335B6EA684580734B84DF23BF2ACECA9554E3
PREHASH tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1 1A131CADE6AC5E9AA31430544ACC402C882B423B403EBA00F0C90833C91D0133
POSTHASH scripts/vscode/Invoke-MSTestWithCoverage.ps1 D21109DC38DB03A2B1800E91C5F335B6EA684580734B84DF23BF2ACECA9554E3
POSTHASH tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1 1A131CADE6AC5E9AA31430544ACC402C882B423B403EBA00F0C90833C91D0133
```

WRITE SET REWRITTEN BY FORMATTER: NONE

## Porcelain immediately after the format run, verbatim

```
M  scripts/vscode/Invoke-MSTestWithCoverage.ps1
 A tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1
```

RESTORED PATHS: NONE

## Porcelain after restoration, verbatim

```
M  scripts/vscode/Invoke-MSTestWithCoverage.ps1
 A tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1
```

Output Summary: The formatter rewrote neither Write Set file; both SHA256 hashes are unchanged across the run, so `WRITE SET REWRITTEN BY FORMATTER:` reads `NONE` and this phase does not restart. The porcelain capture names only Write Set paths — the `[P2-T1]` fix, staged, and the new test file, intent-to-add — so the Format-Drift Rule required no restoration and `RESTORED PATHS:` reads `NONE`. The tool's exit code is not the acceptance signal for this task; the hash pair is, because a write-mode formatter exits 0 both when it changes nothing and when it repairs, and both Write Set files were already dirty relative to `HEAD` from Phases 1 and 2.
