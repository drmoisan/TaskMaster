# [P0-T9] Formatter baseline (read-only)

- Issue: #792
- Timestamp: 2026-09-17T18-39
- Command: `dotnet tool run csharpier check .` (run with the item worktree as the working directory; manifest-pinned CSharpier 1.2.6 restored in [P0-T4])
- EXIT_CODE: 0
- Output Summary: `Checked 1641 files in 4263ms.` — the command printed exactly one line and reported no unformatted file.

## Observations

Verbatim check line: `Checked 1641 files in 4263ms.`

BASELINE-DRIFT-SET: (empty; exit 0, no file reported)

DRIFT-IN-WRITE-SET: 0
