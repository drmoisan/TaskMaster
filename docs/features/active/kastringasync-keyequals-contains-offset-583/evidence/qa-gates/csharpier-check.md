# CSharpier Check (P5-T2)

- Timestamp: 2026-09-13T01-45
- Command: <resolved dotnet executable> tool run csharpier check . (run from repository root,
  read-only, repository-wide)
- EXIT_CODE: 0

## Verbatim output

```
Checked 1624 files in 5552ms.
```

## Output Summary

Exit code 0; only the "Checked N files in T ms." summary line appears, with no per-file line
ahead of it — 0 files needing formatting. Matches the recorded repo-wide clean shape ("Checked
1517 files in 6574ms.", 0 needing formatting) at
docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/qa-gates/csharpier-check.2026-08-22T09-53.md;
the higher file count reflects the larger repository tree at this later point in time.
