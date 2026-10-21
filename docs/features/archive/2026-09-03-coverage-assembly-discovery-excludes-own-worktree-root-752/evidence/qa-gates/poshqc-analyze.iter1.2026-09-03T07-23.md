# PoshQC Analyze Stage, Iteration 1 ([P3-T2])

Timestamp: 2026-09-03T12-11

Command: `mcp__drm-copilot__run_poshqc_analyze` with `workspace_root` of `<repo-root>` and `scan_folders` of `scripts/vscode` and `tests/scripts/vscode`

EXIT_CODE: 1

ExpectedExitCode: 1

POST-CHANGE MCP ANALYZER ISSUE COUNT: 16

BASELINE MCP ANALYZER ISSUE COUNT: 16 (from `evidence/baseline/poshqc-analyze-baseline.2026-09-03T07-23.md`)

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

Output Summary: The post-change count is 16, which is equal to and therefore not greater than the baseline count of 16, so this stage passes. `ExpectedExitCode: 1` is carried forward from the `[P0-T7]` baseline artifact: this tool exits non-zero on any Warning and the repository already carried a pre-existing unsuppressed Warning under `scripts/vscode` before this item changed anything, so an exit-code equality assertion would not be a meaningful gate here. The rule-level comparison, which the exit code and the bare count cannot supply, is recorded by `[P3-T3]`.
