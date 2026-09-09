# P5-T1 — Final QA Loop, Stage 1: Format

Timestamp: 2026-09-09T11-22
Task: [P5-T1]
Command: `mcp__drm-copilot__run_poshqc_format` with `scan_folders` = [`scripts/vscode`, `tests/scripts/vscode`]
EXIT_CODE: 0

Tool return:

```
{"ok":true,"tool":"run_poshqc_format","summary":"Ran bundled PoshQC format against the workspace with 2 selected scan folder(s)."}
```

The `workspace_root` value the tool echoes is an absolute host path and is omitted from the
transcription above; the folders scanned were `scripts/vscode` and `tests/scripts/vscode`, supplied
explicitly rather than left to default.

## Second loop pass, 2026-09-09T11-31

The loop restarted from this stage after P5-T6 failed on its first pass and the repair changed two
test files. The formatter was re-run with the identical arguments and returned the identical result.

| Field | Value |
| --- | --- |
| Re-run timestamp | 2026-09-09T11-31 |
| Re-run exit code | 0 |
| Re-run tool return | `{"ok":true,"tool":"run_poshqc_format","summary":"Ran bundled PoshQC format against the workspace with 2 selected scan folder(s)."}` |

The second pass is the one the loop-completion declaration in
`evidence/qa-gates/p5-t9-toolchain-loop.md` records as clean.

Output Summary: The formatter returned ok. **The exit status alone is not the acceptance signal for
this tool**: it rewrites tracked source in place and still exits 0 after rewriting, so a clean run and
a repairing run are indistinguishable by exit code. The discriminating observation is the scoped tree
comparison in `evidence/qa-gates/p5-t2-format-tree-observation.md`, which is satisfiable here because
P4-T1 committed both folders.
