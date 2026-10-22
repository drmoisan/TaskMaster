# P0-T4 — Format Baseline

Timestamp: 2026-09-09T10-42
Task: [P0-T4]
Command: `mcp__drm-copilot__run_poshqc_format` with `scan_folders` = [`scripts/vscode`, `tests/scripts/vscode`]
EXIT_CODE: 0

Tool return:

```
{"ok":true,"tool":"run_poshqc_format","summary":"Ran bundled PoshQC format against the workspace with 2 selected scan folder(s)."}
```

The `workspace_root` value the tool echoes is an absolute host path and is omitted from the
transcription above; the folders scanned were `scripts/vscode` and `tests/scripts/vscode`.

## Discriminating tree observation

Command: `git status --porcelain --untracked-files=all -- scripts/vscode tests/scripts/vscode`
EXIT_CODE: 0

```
(no output)
```

Output Summary: The formatter returned ok, and the scoped porcelain observation taken immediately
after it printed nothing. The two folders therefore carried no pre-existing formatter drift, and the
formatter rewrote no file. The tree observation is the discriminating signal here; the ok flag alone
is not, because this is a write-mode tool that exits successfully whether or not it repaired a file.
