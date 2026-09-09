# P0-T5 — Analyzer Baseline For `scripts/vscode`

Timestamp: 2026-09-09T10-43
Task: [P0-T5]
Command: `mcp__drm-copilot__run_poshqc_analyze` with `scan_folders` = [`scripts/vscode`]
EXIT_CODE: 1
ExpectedExitCode: 1

Tool return:

```
{"ok":false,"tool":"run_poshqc_analyze","summary":"Command exited with code 1.",
 "stderr_excerpt":"Exception: PSScriptAnalyzer reported 16 issue(s)."}
```

The `workspace_root` value the tool echoes is an absolute host path and is omitted from the
transcription above. ANSI colour escapes present in the raw `stderr_excerpt` are also omitted.

## Recorded baseline

BASELINE_ANALYZER_FINDINGS = 16

Output Summary: PoshQC analyze over `scripts/vscode` exits 1 and reports exactly 16 PSScriptAnalyzer
issues. The observed integer equals the expected value of 16 that the plan records as measured on
this branch head and independently recorded on 2026-08-04 by the issue 400 delivery, so there is no
discrepancy to note. These 16 findings are pre-existing and out of scope for this feature; no task in
this plan demands zero findings for this folder. P5-T4 compares its observed count against this
recorded baseline of 16.
