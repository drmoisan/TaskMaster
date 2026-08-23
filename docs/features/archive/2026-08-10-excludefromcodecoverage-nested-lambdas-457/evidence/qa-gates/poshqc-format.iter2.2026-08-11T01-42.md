# [P3-T2] PoshQC format — toolchain loop iteration 2

Timestamp: 2026-08-11T01-42
Iteration: **2**
Command: `mcp__drm-copilot__run_poshqc_format` with
`workspace_root = C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a3f0c78078ca2265a` and
`scan_folders = ["scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1", "scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1", "tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1", "tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1"]`
EXIT_CODE: MCP `ok:true` (the format surface emits no process exit code)

Iteration 2 exists because the `[P3-T3]` iteration-1 analyze gate failed on a new
`PSUseBOMForUnicodeEncodedFile` diagnostic and its remediation changed files. See
`poshqc-analyze.iter1.2026-08-11T01-38.md`.

MCP Result (verbatim):

```json
{"ok":true,"tool":"run_poshqc_format","workspace_root":"C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-a3f0c78078ca2265a","summary":"Ran bundled PoshQC format against 'C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-a3f0c78078ca2265a' with 4 selected scan folder(s)."}
```

## Files rewritten

**no file rewritten**

Formatter fixed-point measurement over all four files (each compared byte-for-byte against
`Invoke-Formatter -ScriptDefinition <current content>`):

| File | at formatter fixed point | lines (`wc -l`) |
|---|---|---|
| `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` | **True** | 389 |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | **True** | 457 |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` | **True** | 443 |
| `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` | **True** | 490 |

`ANY_FILE_WOULD_BE_REWRITTEN: False`

Line-count changes since iteration 1 are fully attributable to the iteration-1 remediation, not to
the formatter:

- `Invoke-MSTestWithCoverage.ClosureFilter.ps1`: 387 -> 389. The dead-`else` removal replaced one
  line with three (a two-line explanatory comment plus the simplified assignment) and the
  `$null -ne $presentMembers -and` guard removal replaced one line with one.
- `Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`: 374 -> 443 (+69), the two added coverage tests.
- The two `Helpers` files are unchanged at 457 and 490.

Encoding verification after the remediation: `grep -c -P "[^\x00-\x7F]"` on
`scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` returns 0 (exit 1). The file is pure
ASCII, so `PSUseBOMForUnicodeEncodedFile` no longer applies and no BOM is required.

## Scan granularity

FORMAT_SCAN_GRANULARITY: file-honored

`git status --porcelain -uall -- scripts/vscode tests/scripts/vscode` immediately after the format run
(verbatim):

```
 M scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
 M tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1
?? scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1
?? tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1
```

Byte-identical to the listing recorded by `[P2-T11]`. Every path is this feature's own surface.
`scripts/vscode/Sync-PackageReferences.ps1`, `scripts/vscode/Invoke-MSTest.ps1` and
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` — the three files preflight measured as
formatter-dirty — are all absent, so the scan was not coerced to the containing folder.

Restore branch: **NOT TAKEN**. No `git checkout -- <path>` was required.

The helpers-module diff remains exactly 2 added / 0 removed lines (`git diff --numstat` reports
`2  0  scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`).

## Output Summary

`run_poshqc_format` completed `ok:true`. No file rewritten
(`ANY_FILE_WOULD_BE_REWRITTEN: False` across all four). Scan granularity `file-honored`; no restore
required. Restricted changed-file listing byte-identical to `[P2-T11]`. The loop proceeds to
`[P3-T3]` at iteration 2.
