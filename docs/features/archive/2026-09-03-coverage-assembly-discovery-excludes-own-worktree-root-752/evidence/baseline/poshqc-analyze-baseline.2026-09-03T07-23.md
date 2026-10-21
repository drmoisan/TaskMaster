# PoshQC Analyze Baseline ([P0-T7])

Timestamp: 2026-09-03T11-53

Command: `mcp__drm-copilot__run_poshqc_analyze` with `workspace_root` of `<repo-root>` and `scan_folders` of `scripts/vscode` and `tests/scripts/vscode`

EXIT_CODE: 1

ExpectedExitCode: 1

BASELINE MCP ANALYZER ISSUE COUNT: 16

## Tool payload

Recorded as returned, with one class-level substitution applied at capture time: the tool echoes the `workspace_root` argument, which is this item's absolute worktree root; that one value is written as `<repo-root>` per this plan's Host-path hygiene rule. Every other character is as returned.

```
{
  "ok": false,
  "tool": "run_poshqc_analyze",
  "workspace_root": "<repo-root>",
  "summary": "Command exited with code 1.",
  "stderr_excerpt": "Exception: PSScriptAnalyzer reported 16 issue(s)."
}
```

(ANSI colour escape sequences present in the returned `stderr_excerpt` are omitted from the block above; no other character differs.)

Output Summary: The MCP analyzer reports 16 issues across `scripts/vscode` and `tests/scripts/vscode` on the pre-change tree and exits non-zero, which is the documented behaviour of this tool on any Warning. `EXIT_CODE` is set from the `ok` field (false, so 1) and `ExpectedExitCode: 1` is declared because the count is greater than zero. The tool returns no rule names, files, or severities, so the rule-level baseline is captured separately in `pssa-diagnostic-set-baseline.2026-09-03T07-23.md` by `[P0-T8]`.
