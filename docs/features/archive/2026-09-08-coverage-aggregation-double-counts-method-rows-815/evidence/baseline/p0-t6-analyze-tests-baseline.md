# P0-T6 — Analyzer Baseline For `tests/scripts/vscode`

Timestamp: 2026-09-09T10-43
Task: [P0-T6]
Command: `mcp__drm-copilot__run_poshqc_analyze` with `scan_folders` = [`tests/scripts/vscode`]
EXIT_CODE: 0

Tool return:

```
{"ok":true,"tool":"run_poshqc_analyze","summary":"Ran bundled PoshQC analyze against the workspace with 1 selected scan folder(s)."}
```

The `workspace_root` value the tool echoes is an absolute host path and is omitted from the
transcription above.

The tool printed no `PSScriptAnalyzer reported N issue(s)` sentence on this run. Per the task text
that absence is not a failure: a zero-finding run is not required to print it.

## Recorded baseline

BASELINE_ANALYZER_FINDINGS = 0 (inferred from the ok return, per the mechanism below)

Output Summary: PoshQC analyze over `tests/scripts/vscode` returns ok and exits 0. That ok return is
itself the zero-finding observation, because the same tool was observed in P0-T5 to exit non-zero
with an explicit finding-count sentence whenever its finding count is greater than zero. A zero
demand is therefore satisfiable for this folder, and P5-T5 asserts it again after this feature's new
test file lands in the folder.
