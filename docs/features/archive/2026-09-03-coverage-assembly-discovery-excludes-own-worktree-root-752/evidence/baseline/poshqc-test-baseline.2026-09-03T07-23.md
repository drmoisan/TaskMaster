# PoshQC Test Baseline ([P0-T9])

Timestamp: 2026-09-03T11-54

Command: `mcp__drm-copilot__run_poshqc_test` with `workspace_root` of `<repo-root>` and `scan_folders` of `tests/scripts/vscode`

EXIT_CODE: 0

MCP RESULT OK: true

## Tool payload

Recorded as returned, with one class-level substitution applied at capture time: the tool echoes the `workspace_root` argument, which is this item's absolute worktree root; that one value is written as `<repo-root>` per this plan's Host-path hygiene rule. Every other character is as returned.

```
{"ok":true,"tool":"run_poshqc_test","workspace_root":"<repo-root>","summary":"Ran bundled PoshQC test against '<repo-root>' with 1 selected scan folder(s)."}
```

Output Summary: The MCP test run over `tests/scripts/vscode` returned `ok: true` on the pre-change tree. `scan_folders` was supplied explicitly because this repository has no `config/poshqc-scan.json` from which the tool could resolve a scan set. The payload carries no pass count, no failure count, and no coverage figure, so the numeric baseline is captured separately by `[P0-T10]`.
