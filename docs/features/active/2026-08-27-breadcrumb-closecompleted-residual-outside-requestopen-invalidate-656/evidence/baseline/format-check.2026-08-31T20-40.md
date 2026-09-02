# Baseline — Format Gate (read-only) (Issue #656)

Timestamp: 2026-09-01T14-39
Task: [P0-T8]

Command:
```
dotnet tool run csharpier check .
```

EXIT_CODE: 0

Output Summary: `Checked 1566 files in 4614ms.` — the final summary line of the command output,
transcribed verbatim. CSharpier reported no file requiring formatting and exited 0.

Notes:

- `check` is read-only, so the exit code alone is a genuine observation of tree state. The
  write-mode `format` command is deliberately excluded from the baseline so that the baseline cannot
  become a blanket waiver for pre-existing formatting drift.
- A non-zero exit here would have meant pre-existing repository-wide format drift, which would place
  AC-14 outside this item's two-file footprint. That did not occur: the pre-change tree is already
  format-clean, so any later `csharpier check` failure is attributable to this item's own edits.
